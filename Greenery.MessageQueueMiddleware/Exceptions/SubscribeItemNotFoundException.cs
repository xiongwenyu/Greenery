namespace Greenery.MessageQueueMiddleware.Exceptions
{
    public class SubscribeItemNotFoundException : MQMException
    {
        public SubscribeItemNotFoundException(string message) : base("600", message)
        { }
    }
}
