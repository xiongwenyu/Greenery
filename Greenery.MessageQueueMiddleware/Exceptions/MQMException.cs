using System;

namespace Greenery.MessageQueueMiddleware.Exceptions
{
    public class MQMException : Exception
    {
        public MQMException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}
