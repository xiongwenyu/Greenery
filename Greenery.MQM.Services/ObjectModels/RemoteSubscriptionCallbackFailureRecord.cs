using MongoDB.Bson;
using System;

namespace Greenery.MessageQueueMiddleware.ObjectModels
{
    public class RemoteSubscriptionCallbackFailureRecord
    {
#if MQM
        public ObjectId _id { get; set; }
#endif
        public string Content { get; set; }

        public Guid PublisherId { get; set; }

    }
}
