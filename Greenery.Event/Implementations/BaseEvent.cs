using Greenery.Event.ObjectModels;
using System;

namespace Greenery.Event.Implementations
{
    /// <summary>
    /// 事件基础模型
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        /// <summary>
        /// 初始化事件创建时间及事件标识
        /// </summary>
        public BaseEvent()
        {
            CreateDt = DateTime.Now;
            Id = Guid.NewGuid();
        }

        #region 属性
        /// <summary>
        /// 事件创建时间
        /// </summary>
        public DateTime CreateDt { get; set; }
        /// <summary>
        /// 事件描述
        /// </summary>
        public EventDescription Descriptions { get; set; }
        /// <summary>
        /// 事件标识
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 源事件发布器Id
        /// </summary>
        public Guid PublisherId { get; set; }
        #endregion 属性
    }
}
