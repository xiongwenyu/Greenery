using Greenery.Event.ObjectModels;
using System;

namespace Greenery.Event.Implementations
{
    /// <summary>
    /// 通用事件回调包装器
    /// </summary>
    public class EventHandlerActionWrapper<TEvent> : BaseEventHandler<TEvent>
        where TEvent : IEvent, new()
    {
        public EventHandlerActionWrapper(Action<TEvent> callBack)
        {
            if (callBack == null)
            {
                throw new DomianEventException("事件回调处理方法不能为空！");
            }
            EventHandlerCallBack = callBack;
        }
        /// <summary>
        /// 事件回调
        /// </summary>
        private Action<TEvent> EventHandlerCallBack { get; set; }

        public override void Execute(TEvent eventObject)
        {
            EventHandlerCallBack(eventObject);
        }
    }
}
