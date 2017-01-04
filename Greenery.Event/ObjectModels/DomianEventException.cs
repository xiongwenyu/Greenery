using System;

namespace Greenery.Event.ObjectModels
{
    /// <summary>
    /// 事件异常信息
    /// </summary>
    public class DomianEventException : Exception
    {
        public DomianEventException(string msg) : base(msg) { }
    }
}
