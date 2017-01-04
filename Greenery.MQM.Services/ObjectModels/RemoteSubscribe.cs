using MongoDB.Bson;
using System;

namespace Greenery.MessageQueueMiddleware.ObjectModels
{
    public class RemoteSubscribe
    {
#if MQM
        public ObjectId _id { get; set; }
#endif
        public string Group { get; set; }

        public Guid AgentId { get; set; }

    }
}
