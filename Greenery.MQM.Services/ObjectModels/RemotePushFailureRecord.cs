using MongoDB.Bson;
using System;

namespace Greenery.MessageQueueMiddleware.ObjectModels
{
    public class RemotePushFailureRecord
    {
#if MQM
        public ObjectId _id  { get; set; }
        public Guid AgentId { get; set; }

#endif
        public PublishItem Content { get; set; }
    }
}
