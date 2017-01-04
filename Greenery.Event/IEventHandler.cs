using Newtonsoft.Json;

namespace Greenery.Event
{
    /// <summary>
    /// 事件处理器
    /// </summary>
    internal interface IEventHandler
    {
        /// <summary>
        /// 事件处理
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="eventJson">事件</param>
        void Handler(string eventJson);
    }

    /// <summary>
    /// 事件处理器
    /// </summary>
    public abstract class BaseEventHandler<TEvent> : IEventHandler where TEvent : IEvent, new()
    {
        /// <summary>
        /// 事件处理
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="eventObject">事件</param>
        public abstract void Execute(TEvent eventObject);

        public virtual void Handler(string json)
        {
            var eventObject = JsonConvert.DeserializeObject<TEvent>(json);
            Execute(eventObject);
        }

    }
}
