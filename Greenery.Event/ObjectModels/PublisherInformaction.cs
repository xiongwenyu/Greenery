using System;
#if MQM
using MongoDB.Bson;
#endif


namespace Greenery.Event.ObjectModels
{
    public class PublisherInformaction
    {
#if MQM
        public ObjectId _id { get; set; }
#endif
        /// <summary>
        /// 事件发布器Id
        /// </summary>
        public Guid PublisherId { get; set; }

        public string SessionId { get; set; }

        public bool IsWebSite { get; set; }

        public string WebSiteHost { get; set; }

        public string MQMToken { get; set; }
    }
}
