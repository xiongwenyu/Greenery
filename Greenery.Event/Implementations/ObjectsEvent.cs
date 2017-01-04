using Newtonsoft.Json;

namespace Greenery.Event.Implementations
{
    /// <summary>
    /// 通用事件模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectsEvent<T> : BaseEvent
    {
        public ObjectsEvent() : base()
        { }
        /// <summary>
        /// 事件内容
        /// </summary>
        public T Content { get; set; }
    }

    /// <remarks>用于网络传输封装</remarks>
    public class EventJsonWrapper : BaseEvent
    {
        public EventJsonWrapper() { }
        public EventJsonWrapper(IEvent eventObject)
        {
            EventObject = JsonConvert.SerializeObject(eventObject);
            base.PublisherId = eventObject.PublisherId;
            base.Descriptions = eventObject.Descriptions;
            base.CreateDt = eventObject.CreateDt;
            base.Id = eventObject.Id;
        }

        public string EventObject { get; set; }

        public override string ToString()
        {
            return EventObject;
        }
    }
}
