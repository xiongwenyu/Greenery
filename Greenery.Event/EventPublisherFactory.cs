using Greenery.Event.Implementations;

namespace Greenery.Event
{
    public class PublisherFactory
    {
        public static IPublisher Create()
        {
            return new EventPublisher();
        }
    }
}
