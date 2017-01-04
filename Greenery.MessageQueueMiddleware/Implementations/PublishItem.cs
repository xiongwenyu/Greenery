using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greenery.MessageQueueMiddleware.Implementations
{
    public class PublishItem
    {
        public string Content { get; internal set; }

        /// <summary>
        /// 订阅描述
        /// </summary>
        public string[] Descriptions { get; internal set; }

        public Guid PublisherId { get; internal set; }
    }
}
