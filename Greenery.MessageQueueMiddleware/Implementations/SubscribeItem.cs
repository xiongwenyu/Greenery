using System;
using Greenery.Event.ObjectModels;

namespace Greenery.MessageQueueMiddleware.Implementations
{
    /// <summary>
    /// 订阅项目信息
    /// </summary>
    public class SubscribeItem
    {
        /// <summary>
        /// 订阅描述
        /// </summary>
        public string[] Descriptions { get; internal set; }
        /// <summary>
        /// 订阅过滤模式
        /// </summary>
        public FilterMode FilterMode { get; internal set; }
        /// <summary>
        /// 事件发布器Id
        /// </summary>
        public Guid PublisherId { get; internal set; }

        public string SessionId { get; set; }

        public bool IsWebSite { get; set; }

        public string WebSiteHost { get; set; }
    }
}