using System;

namespace Greenery.Event.ObjectModels
{
    /// <summary>
    /// 订阅特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscribeAttribute : Attribute
    {
        public SubscribeAttribute(FilterMode filterMode,params string[] descriptions)
        {
            FilterMode = filterMode;
            Descriptions =descriptions;
        }
        /// <summary>
        /// 订阅描述
        /// </summary>
        public string [] Descriptions{ get; private set; }
        /// <summary>
        /// 订阅模式
        /// </summary>
        public FilterMode FilterMode { get; private set; }
    }
}
