using System;
using Greenery.Event.ObjectModels;
#if MQM
using MongoDB.Bson;
#endif

namespace Greenery.MessageQueueMiddleware.ObjectModels
{
    /// <summary>
    /// 订阅项目信息
    /// </summary>
    public class SubscribeItem
    {
#if MQM
        public ObjectId _id { get; set; }
#endif
        /// <summary>
        /// 订阅描述
        /// </summary>
        public string[] Descriptions { get; set; }
        /// <summary>
        /// 订阅过滤模式
        /// </summary>
        public FilterMode FilterMode { get; set; }
        /// <summary>
        /// 事件发布器Id
        /// </summary>
        public Guid PublisherId { get; set; }

    }
    public class ClientIndetity
    {
#if MQM
        public ObjectId _id { get; set; }
        public Guid AgentId { get; set; }

#endif
        /// <summary>
        /// 事件发布器Id
        /// </summary>
        public Guid PublisherId { get; set; }

        public string SessionId { get; set; }

        public bool IsWebSite { get; set; }

        public string WebSiteHost { get; set; }
        
    }
}