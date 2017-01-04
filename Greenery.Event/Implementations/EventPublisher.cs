using Greenery.Event.ObjectModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Greenery.Event.Extensions;
using Newtonsoft.Json;

namespace Greenery.Event.Implementations
{
    /// <summary>
    /// 事件发布器
    /// </summary>
    public class EventPublisher : IPublisher
    {
        private const string PUBLISHERID = "PUBLISHERID.Greenery";

        internal static List<LocalSubscribeItem> subscribes = new List<LocalSubscribeItem>();
        static EventPublisher()
        {
            InitPublisherId();
        }


        static Guid publisherId = Guid.Empty;

        public event RemoteSubscribe RemoteSubscribeHandler;

        /// <summary>
        /// 发布器编号（初次初始化后，需要做持久化保存，保证程序再次启动不发生变化）
        /// </summary>
        public Guid PublisherId
        {
            get
            {
                if (publisherId == Guid.Empty)
                {
                    InitPublisherId();
                }
                return publisherId;
            }
        }

        public bool IsWebSitePublisher { get; private set; }

        public string WebSiteHost
        {
            get
            {
                var host = GreeneryEventConfigurationSection.GetConfig().WebSiteHost;
                if (string.IsNullOrEmpty(host))
                {
                    throw new Exception();
                }
                return host;
            }
        }


        /// <summary>
        /// 初始化事件发布器，并执行订阅区块，自动加载SubscribeAttribute标识的订阅内容
        /// </summary>
        /// <param name="areas">多个订阅区块</param>
        public void Initialization(bool isWebSite = false, params ISubscribeArea[] areas)
        {
            IsWebSitePublisher = isWebSite;
            foreach (var item in areas)
            {
                item.RegisterArea(this);
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(o => o != this.GetType().Assembly).ToList();
            foreach (var assembly in assemblies)
            {
                assembly.GetImplementedObjectsByInterface<IEventHandler>((o, type) =>
                {
                    var attrs = type.GetCustomAttributes(typeof(SubscribeAttribute), false);
                    var attr = attrs.FirstOrDefault();
                    if (attr != null)
                    {
                        var subscribeAttribute = (SubscribeAttribute)attr;
                        PriateSubscribeAsync(o, subscribeAttribute.FilterMode, subscribeAttribute.Descriptions);
                    }
                });
            }
            if (!string.IsNullOrEmpty(GreeneryEventConfigurationSection.GetConfig().MQMIP))
                Subscribe(new Exchanges(this), FilterMode.FullEvent, "*");
        }

        #region 事件推送(Publish)

        /// <summary>
        /// 事件推送
        /// </summary>
        /// <typeparam name="TEventContent">事件内容</typeparam>
        /// <param name="description">事件描述</param>
        /// <param name="domainEventContent">领域事件内容</param>
        /// <param name="isLocalLoop">是否为本地环内事件</param>
        public void Publish<TEventContent>(EventDescription description, TEventContent domainEventContent, bool isLocalLoop = false) where TEventContent : new()
        {
            var task = PublishAsync(description, domainEventContent, isLocalLoop);
            task.Wait();
        }

        /// <summary>
        /// 事件推送
        /// </summary>
        /// <typeparam name="TEvent">实现IEvent的事件</typeparam>
        /// <param name="domainEvent">领域事件</param>
        /// <param name="isLocalLoop">是否为本地环内事件</param>
        public void Publish<TEvent>(TEvent domainEvent, bool isLocalLoop = false) where TEvent : IEvent, new()
        {
            var task = PublishAsync(domainEvent, isLocalLoop);
            task.Wait();
        }

        /// <summary>
        /// 异步事件推送
        /// </summary>
        /// <typeparam name="TEventContent">事件内容</typeparam>
        /// <param name="description">事件描述</param>
        /// <param name="domainEventContent">领域事件内容</param>
        /// <param name="isLocalLoop">是否为本地环内事件</param>
        /// <returns>异步任务</returns>
        public Task PublishAsync<TEventContent>(EventDescription description, TEventContent domainEventContent, bool isLocalLoop = false) where TEventContent : new()
        {
            var domainEvent = new ObjectsEvent<TEventContent>() { Content = domainEventContent, Descriptions = description };
            return PublishAsync(domainEvent, isLocalLoop);
        }

        /// <summary>
        /// 异步事件推送
        /// </summary>
        /// <typeparam name="TEvent">实现IEvent的事件</typeparam>
        /// <param name="domainEvent">领域事件</param>
        /// <param name="isLocalLoop">是否为本地环内事件</param>
        /// <returns>异步任务</returns>
        public Task PublishAsync<TEvent>(TEvent domainEvent, bool isLocalLoop = false) where TEvent : IEvent, new()
        {
            return Task.Factory.StartNew(() =>
            {

                if (domainEvent.Equals(default(TEvent)))
                {
                    throw new Exception("事件不能为null!");
                }

                List<LocalSubscribeItem> tempSubscribes;
                if (domainEvent.PublisherId == Guid.Empty)
                    domainEvent.PublisherId = publisherId;
                lock (subscribes)//防止多线程并发
                {
                    tempSubscribes = subscribes.ToList();
                }
                var effectiveSubscribes = new List<LocalSubscribeItem>();
                foreach (var item in tempSubscribes)
                {
                    switch (item.FilterMode)
                    {
                        case FilterMode.WholeMatched:
                            {
                                var subscribeDescriptions = string.Join(PublisherConstant.Separator, item.Descriptions);
                                var eventDescriptions = domainEvent.Descriptions.ToString();
                                if (eventDescriptions == subscribeDescriptions)
                                {
                                    effectiveSubscribes.Add(item);
                                }
                            }
                            break;
                        case FilterMode.StartsWith:
                            {
                                var subscribeDescriptions = string.Join(PublisherConstant.Separator, item.Descriptions);
                                var eventDescriptions = domainEvent.Descriptions.ToString();
                                if (eventDescriptions.StartsWith(subscribeDescriptions))
                                {
                                    effectiveSubscribes.Add(item);
                                }
                            }
                            break;
                        case FilterMode.FullEvent:
                            {
                                if (item.Descriptions.Contains("*") && domainEvent.PublisherId == publisherId && (!isLocalLoop || (isLocalLoop && !(item.handler is Exchanges))))//只能匹配内部事件
                                {
                                    effectiveSubscribes.Add(item);
                                }
                            }
                            break;
                    }
                }
                var json = "";
                foreach (var item in effectiveSubscribes)
                {
                    try
                    {
                        if (item.handler is BaseEventHandler<TEvent>)
                        {
                            var baseHandler = (item.handler as BaseEventHandler<TEvent>);
                            baseHandler.Execute(domainEvent);
                        }
                        else if (item.handler is Exchanges)
                        {
                            var baseHandler = (item.handler as Exchanges);
                            baseHandler.Execute(new EventJsonWrapper(domainEvent));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(json))
                            {
                                json = JsonConvert.SerializeObject(domainEvent);
                            }
                            item.handler.Handler(json);
                        }
                    }
                    catch (Exception)
                    {
                        // to do Log
                    }
                }
            });
        }
        public Task RemotePublishAsync(string eventJson)
        {
            return Task.Factory.StartNew(() =>
            {

                if (string.IsNullOrEmpty(eventJson))
                {
                    throw new Exception("事件不能为null!");
                }

                var domainEvent = JsonConvert.DeserializeObject<EventJsonWrapper>(eventJson);
                if (domainEvent.PublisherId == publisherId) return;

                List<LocalSubscribeItem> tempSubscribes;
                lock (subscribes)//防止多线程并发
                {
                    tempSubscribes = subscribes.ToList();
                }
                var effectiveSubscribes = new List<LocalSubscribeItem>();
                foreach (var item in tempSubscribes)
                {
                    switch (item.FilterMode)
                    {
                        case FilterMode.WholeMatched:
                            {
                                var subscribeDescriptions = string.Join(PublisherConstant.Separator, item.Descriptions);
                                var eventDescriptions = domainEvent.Descriptions.ToString();
                                if (eventDescriptions == subscribeDescriptions)
                                {
                                    effectiveSubscribes.Add(item);
                                }
                            }
                            break;
                        case FilterMode.StartsWith:
                            {
                                var subscribeDescriptions = string.Join(PublisherConstant.Separator, item.Descriptions);
                                var eventDescriptions = domainEvent.Descriptions.ToString();
                                if (eventDescriptions.StartsWith(subscribeDescriptions))
                                {
                                    effectiveSubscribes.Add(item);
                                }
                            }
                            break;
                    }
                }
                foreach (var item in effectiveSubscribes)
                {
                    try
                    {
                        item.handler.Handler(eventJson);
                    }
                    catch (Exception)
                    {
                        // to do Log
                    }
                }
            });
        }

        #endregion 事件推送(Publish)

        #region 事件订阅(Subscribe)

        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <param name="handler">处理回调</param>
        /// <param name="mode">匹配过滤模式</param>
        /// <param name="descriptions">匹配事件描述</param>
        public Guid Subscribe<TEvent>(BaseEventHandler<TEvent> handler, FilterMode mode, params string[] descriptions) where TEvent : IEvent, new()
        {
            var task = SubscribeAsync(handler, mode, descriptions);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <typeparam name="TEvent">实现IEvent的事件</typeparam>
        /// <param name="handler">处理回调</param>
        /// <param name="mode">匹配过滤模式</param>
        /// <param name="descriptions">匹配事件描述</param>
        public Guid Subscribe<TEvent>(Action<TEvent> handler, FilterMode mode, params string[] descriptions) where TEvent : IEvent, new()
        {
            var task = SubscribeAsync(handler, mode, descriptions);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 异步事件订阅
        /// </summary>
        /// <param name="handler">处理回调</param>
        /// <param name="mode">匹配过滤模式</param>
        /// <param name="descriptions">匹配事件描述</param>
        /// <returns>异步任务</returns>
        public Task<Guid> SubscribeAsync<TEvent>(BaseEventHandler<TEvent> handler, FilterMode mode, params string[] descriptions) where TEvent : IEvent, new()
        {
            return PriateSubscribeAsync(handler, mode, descriptions);
        }

        private Task<Guid> PriateSubscribeAsync(IEventHandler handler, FilterMode mode, params string[] descriptions)
        {
            return Task.Factory.StartNew(() =>
            {
                if (handler == null)
                {
                    throw new DomianEventException("事件处理回调不能为空！");
                }
                if (descriptions == null || descriptions.Length == 0)
                {
                    throw new DomianEventException("订阅事件至少需要一条描述！");
                }
                var id = Guid.NewGuid();
                lock (subscribes)
                {
                    subscribes.Add(new LocalSubscribeItem()
                    {
                        handler = handler,
                        Descriptions = descriptions,
                        FilterMode = mode,
                        Id = id
                    });
                }
                RemoteSubscribeHandler?.Invoke(PublisherId, mode, descriptions);
                return id;
            });
        }

        /// <summary>
        /// 异步事件订阅
        /// </summary>
        /// <typeparam name="TEvent">实现IEvent的事件</typeparam>
        /// <param name="handler">处理回调</param>
        /// <param name="mode">匹配过滤模式</param>
        /// <param name="descriptions">匹配事件描述</param>
        /// <returns>异步任务</returns>
        public Task<Guid> SubscribeAsync<TEvent>(Action<TEvent> handler, FilterMode mode, params string[] descriptions) where TEvent : IEvent, new()
        {
            var wrapper = new EventHandlerActionWrapper<TEvent>(handler);
            return SubscribeAsync(wrapper, mode, descriptions);
        }

        #endregion 事件订阅(Subscribe)

        #region 取消事件订阅(UnSubscribe)
        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="subscribeItemId">订阅id</param>
        public void UnSubscribe(Guid subscribeItemId)
        {
            lock (subscribes)
            {
                var item = subscribes.FirstOrDefault(o => o.Id == subscribeItemId);
                if (item != null)
                {
                    item.IsUnSubscribe = true;
                    subscribes.Remove(item);
                }
            }
        }

        /// <summary>
        /// 异步取消事件订阅
        /// </summary>
        /// <param name="subscribeItemId">订阅id</param>
        /// <returns>异步任务</returns>
        public Task UnSubscribeAsync(Guid subscribeItemId)
        {
            return Task.Factory.StartNew(() =>
            {
                UnSubscribe(subscribeItemId);
            });
        }

        /// <summary>
        /// 取消所有事件订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            lock (subscribes)
            {
                foreach (var item in subscribes)
                {
                    item.IsUnSubscribe = true;
                }
            }
            subscribes = new List<LocalSubscribeItem>();
        }

        /// <summary>
        /// 异步取消所有事件订阅
        /// </summary>
        /// <returns></returns>
        public Task UnsubscribeAllAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                UnsubscribeAll();
            });
        }
        #endregion 取消事件订阅(UnSubscribe)

        /// <summary>
        /// 初始化事件发布器Id,如果文件存在则重新加载历史Id,否则新建并保存Id
        /// </summary>
        private static void InitPublisherId()
        {
            var assemblyFile = typeof(EventPublisher).Assembly.CodeBase;
            var assemblyFileLike = new Uri(assemblyFile);
            var assemblyDirectoryName = Path.GetDirectoryName(assemblyFileLike.AbsolutePath);
            var fileFullName = Path.Combine(assemblyDirectoryName, PUBLISHERID);
            lock (PUBLISHERID)
            {
                if (File.Exists(fileFullName))
                {
                    var publisherIdBytes = File.ReadAllBytes(fileFullName);
                    publisherId = new Guid(publisherIdBytes);
                }
                else
                {
                    publisherId = Guid.NewGuid();
                    File.WriteAllBytes(fileFullName, publisherId.ToByteArray());
                }
            }
        }

        public IEnumerable<LocalSubscribeItem> GetSubscribes()
        {
            return subscribes;
        }
    }
}
