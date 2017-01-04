using Greenery.MessageQueueMiddleware;
using System;
using System.Collections.Generic;
using System.Linq;
using Greenery.Event.ObjectModels;
using Greenery.MessageQueueMiddleware.ObjectModels;
using Greenery.MQM.Services;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Greenery.MQM.Storage
{
    public class DefaultMesssageQueueMiddlewareStorage : IMQMStorage
    {
        static MongoClient client = null;
        static IMongoDatabase database = null;
        IMongoDatabase GetConnection()
        {
            if (database == null)
            {
                var connectionString = MQMConfigurationSection.GetConfig().StorageConnectionString;
                if (client == null)
                {
                    client = new MongoClient(connectionString);
                }
                database = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);
            }
            return database;
        }
        IMongoCollection<T> GetEntities<T>(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;
            return GetConnection().GetCollection<T>(name);
        }

        #region 客户端凭证
        /// <summary>
        /// 查找订阅指定描述主题的客户端凭证
        /// </summary>
        /// <param name="agentId">消息队列代理器Id</param>
        /// <param name="descriptions">事件描述</param>
        /// <param name="IngoreFilterMode">是否忽略过滤模式</param>
        /// <returns>订阅指定描述主题的客户端凭证</returns>
        public IEnumerable<ClientIndetity> FindSubscribers(Guid agentId, string[] descriptions, bool IngoreFilterMode = false)
        {
            var filter = Builders<ClientIndetity>.Filter;
            var sFilter = Builders<SubscribeItem>.Filter;
            IEnumerable<Guid> ids;
            if (IngoreFilterMode)
            {
                ids = GetEntities<SubscribeItem>().Find(sFilter.All(o => o.Descriptions, descriptions)).ToList().Select(o => o.PublisherId);
            }
            else
            {
                var exp = sFilter.Or(
                      sFilter.And(sFilter.Eq(o => o.FilterMode, FilterMode.WholeMatched), sFilter.All(o => o.Descriptions, descriptions), sFilter.Size(o => o.Descriptions, descriptions.Length))
                    , sFilter.And(sFilter.Eq(o => o.FilterMode, FilterMode.StartsWith), sFilter.All(o => o.Descriptions, descriptions)));
                ids = GetEntities<SubscribeItem>().Find(exp).ToList().Select(o => o.PublisherId);
            }
            var clients = GetEntities<ClientIndetity>().Find(filter.And(filter.In(o => o.PublisherId, ids), filter.Eq(o => o.AgentId, agentId))).ToList();
            return clients;
        }

        /// <summary>
        /// 查找订阅指定描述主题的客户端凭证是否存在
        /// </summary>
        /// <param name="agentId">消息队列代理器Id</param>
        /// <param name="descriptions">事件描述</param>
        /// <param name="IngoreFilterMode">是否忽略过滤模式</param>
        /// <returns>是否存在指定的订阅信息项目</returns>
        public bool HasSubscribers(Guid agentId, string[] descriptions, bool IngoreFilterMode = false)
        {
            var ids = GetEntities<ClientIndetity>().Find(o => o.AgentId == agentId).ToList().Select(o => o.PublisherId);
            var filter = Builders<ClientIndetity>.Filter;
            var sFilter = Builders<SubscribeItem>.Filter;
            if (IngoreFilterMode)
            {
                return GetEntities<SubscribeItem>().Find(
                   sFilter.And(sFilter.All(o => o.Descriptions, descriptions), sFilter.In(o => o.PublisherId, ids))).Any();
            }
            else
            {
                var exp = sFilter.Or(
                  sFilter.And(sFilter.In(o => o.PublisherId, ids), sFilter.Eq(o => o.FilterMode, FilterMode.WholeMatched), sFilter.All(o => o.Descriptions, descriptions), sFilter.Size(o => o.Descriptions, descriptions.Length))
                , sFilter.And(sFilter.In(o => o.PublisherId, ids), sFilter.Eq(o => o.FilterMode, FilterMode.StartsWith), sFilter.All(o => o.Descriptions, descriptions)));
                return GetEntities<SubscribeItem>().Find(exp).Any();
            }
        }

        /// <summary>
        /// 新增客户端凭证
        /// </summary>
        /// <param name="subscriber">客户端凭证</param>
        /// <returns>是否保存成功</returns>
        public bool AddClientIndetity(ClientIndetity subscriber)
        {
            try
            {
                GetEntities<ClientIndetity>().InsertOne(subscriber);
                return true;
            }
            catch { }
            return false;
        }
        /// <summary>
        /// 保存已更新的客户端凭证
        /// </summary>
        /// <param name="subscriber">客户端凭证</param>
        /// <returns>保存是否成功</returns>
        public bool SaveUpdateClientIndetity(ClientIndetity subscriber)
        {
            try
            {
                BsonDocument bd = BsonExtensionMethods.ToBsonDocument(subscriber);
                var filter = Builders<ClientIndetity>.Filter;

                GetEntities<ClientIndetity>().UpdateOne(o => o.PublisherId == subscriber.PublisherId, Builders<ClientIndetity>.Update
                    .Set(o => o.SessionId, subscriber.SessionId)
                    .Set(o => o.IsWebSite, subscriber.IsWebSite)
                    .Set(o => o.AgentId, subscriber.AgentId)
                    .Set(o => o.WebSiteHost, subscriber.WebSiteHost));
                return true;
            }
            catch { }
            return false;
        }
        /// <summary>
        /// 尝试获取客户端凭证
        /// </summary>
        /// <param name="publisherId">事件发布器Id</param>
        /// <param name="subscriber">客户端凭证</param>
        /// <returns>是否存在</returns>
        public bool TryGetClientIndetity(Guid publisherId, out ClientIndetity subscriber)
        {
            subscriber = null;
            try
            {
                subscriber = GetEntities<ClientIndetity>().Find(o => o.PublisherId == publisherId).FirstOrDefault();
                return subscriber != null;
            }
            catch { }
            return false;
        }
        #endregion 客户端凭证

        #region 订阅项目信息
        public IEnumerable<SubscribeItem> GetSubscribeItems(Guid agentId)
        {
            var clients = GetEntities<ClientIndetity>().Find(o => o.AgentId == agentId).ToList().Select(o => o.PublisherId);

            return GetEntities<SubscribeItem>().Find(Builders<SubscribeItem>.Filter.In(o => o.PublisherId, clients)).ToList();
        }
        /// <summary>
        /// 尝试获取订阅项目信息
        /// </summary>
        /// <param name="publisherId">事件发布器Id</param>
        /// <param name="descriptions">事件描述</param>
        /// <param name="filterMode">匹配过滤方式</param>
        /// <param name="subscribeItem">订阅项目信息</param>
        /// <returns>是否存在订阅项目</returns>
        public bool TryGetSubscribeItem(Guid publisherId, string[] descriptions, FilterMode filterMode, out SubscribeItem subscribeItem)
        {
            var filter = Builders<SubscribeItem>.Filter;

            ;
            subscribeItem = GetEntities<SubscribeItem>().Find(
                filter.And(
                    filter.All(o => o.Descriptions, descriptions),
                    filter.Size(o => o.Descriptions, descriptions.Length),
                    filter.Eq(o => o.FilterMode, filterMode),
                    filter.Eq(o => o.PublisherId, publisherId)
                    )).FirstOrDefault(); ;
            return subscribeItem != null;

        }
        /// <summary>
        /// 新增订阅项目
        /// </summary>
        /// <param name="subscribeItem">订阅项目信息</param>
        /// <returns>是否保存成功</returns>
        public bool AddSubscribeItem(SubscribeItem subscribeItem)
        {
            try
            {
                GetEntities<SubscribeItem>().InsertOne(subscribeItem);
                return true;
            }
            catch { }
            return false;

        }

        /// <summary>
        /// 移除已订阅项目信息
        /// </summary>
        /// <param name="publisherId">事件发布器Id</param>
        /// <param name="subscribeItem">订阅项目信息</param>
        /// <returns>是否保存成功</returns>
        public bool RemoveSubscribeItem(Guid publisherId, SubscribeItem subscribeItem)
        {
            try
            {
                GetEntities<SubscribeItem>().DeleteOne(o => o._id == subscribeItem._id);
                return true;
            }
            catch { }
            return false;

        }
        #endregion 订阅项目信息

        #region 远程代理订阅记录
        /// <summary>
        /// 新增远程代理订阅记录
        /// </summary>
        /// <param name="agentId">消息队列代理器</param>
        /// <param name="group">订阅分组</param>
        /// <returns>保存是否成功</returns>
        public bool AddRemoteSubscribe(Guid agentId, string group)
        {
            try
            {
                GetEntities<RemoteSubscribe>().InsertOne(new RemoteSubscribe() { AgentId = agentId, Group = group });
                return true;
            }
            catch { }
            return false;
        }
        /// <summary>
        /// 是否已订阅远程订阅分组
        /// </summary>
        /// <param name="agentId">消息队列代理器</param>
        /// <param name="group">订阅分组</param>
        /// <returns>是为true,否则为false</returns>
        public bool HasRemoteSubscribe(Guid agentId, string group)
        {
            return GetEntities<RemoteSubscribe>().Find(o => o.AgentId == agentId && o.Group == group).Any();
        }
        /// <summary>
        /// 删除远程订阅分组
        /// </summary>
        /// <param name="agentId">消息队列代理器</param>
        /// <param name="group">订阅分组</param>
        /// <returns>保存是否成功</returns>
        public bool RemoveRemoteSubscribe(Guid agentId, string group)
        {
            try
            {
                if (string.IsNullOrEmpty(group))
                {
                    GetEntities<RemoteSubscribe>().DeleteMany(o => o.AgentId == agentId);
                }
                else
                {
                    GetEntities<RemoteSubscribe>().DeleteOne(o => o.AgentId == agentId && o.Group == group);
                }
                return true;
            }
            catch { }
            return false;

        }
        #endregion 远程代理订阅记录
        /// <summary>
        /// 远程订阅回调失败记录
        /// </summary>
        /// <param name="publisherId">订阅事件发布器Id</param>
        /// <param name="Content">消息</param>
        public bool AddRemoteSubscriptionCallbackFailureRecord(Guid publisherId, string Content)
        {
            try
            {
                GetEntities<RemoteSubscriptionCallbackFailureRecord>().InsertOne(new RemoteSubscriptionCallbackFailureRecord() { PublisherId = publisherId, Content = Content });
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 查找远程订阅回调失败记录
        /// </summary>
        /// <param name="publisherId">事件发布器Id</param>
        /// <returns>消息内容</returns>
        public IEnumerable<RemoteSubscriptionCallbackFailureRecord> FindRemoteSubscriptionCallbackFailureRecord(Guid publisherId)
        {
            return GetEntities<RemoteSubscriptionCallbackFailureRecord>().Find(o => o.PublisherId == publisherId).ToList();
        }
        /// <summary>
        /// 删除远程订阅回调失败记录
        /// </summary>
        /// <param name="publisherId">事件发布器Id</param>
        /// <param name="item">消息内容</param>
        public bool RemoveRemoteSubscriptionCallbackFailureRecord(ObjectId id)
        {
            try
            {
                GetEntities<RemoteSubscriptionCallbackFailureRecord>().DeleteOne(o => o._id == id);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 新增远程推送失败记录
        /// </summary>
        /// <param name="agentId">消息队列代理器Id</param>
        /// <param name="publishItem">推送内容</param>
        public bool AddRemotePushFailureRecord(Guid agentId, PublishItem publishItem)
        {
            try
            {
                GetEntities<RemotePushFailureRecord>().InsertOne(new RemotePushFailureRecord() { Content = publishItem, AgentId = agentId });
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 获取远程推送失败记录
        /// </summary>
        /// <param name="agentId">消息队列代理器Id</param>
        /// <returns>推送信息</returns>
        public IEnumerable<RemotePushFailureRecord> GetRemotePushFailureRecords(Guid agentId)
        {
            return GetEntities<RemotePushFailureRecord>().Find(o => o.AgentId == agentId).ToList();
        }

        /// <summary>
        /// 移除远程推送失败记录
        /// </summary>
        /// <param name="agentId">消息队列代理器Id</param>
        /// <param name="publishItem">推送内容</param>
        public bool RemoveRemotePushFailureRecord(ObjectId id)
        {
            try
            {
                GetEntities<RemotePushFailureRecord>().DeleteOne(o => o._id == id);
                return true;
            }
            catch { }
            return false;

        }
    }
}
