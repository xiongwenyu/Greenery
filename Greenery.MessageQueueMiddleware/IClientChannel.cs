namespace Greenery.MessageQueueMiddleware
{
    public interface IClientChannel
    {
        bool SendMessage(string message);
    }
}
