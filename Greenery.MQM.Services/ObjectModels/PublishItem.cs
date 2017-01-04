using System;
#if MQM
using MongoDB.Bson;
#endif

namespace Greenery.MessageQueueMiddleware.ObjectModels
{
    public class PublishItem
    {
#if MQM
        public ObjectId _id { get; set; }
#endif
        public string Content { get; set; }

        /// <summary>
        /// 订阅描述
        /// </summary>
        public string[] Descriptions { get; set; }

        public Guid PublisherId { get; set; }
    }
}
