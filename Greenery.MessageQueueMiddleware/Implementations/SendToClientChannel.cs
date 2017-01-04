using Greenery.MessageQueueMiddleware.Implementations.ClientChannels;
using Greenery.MessageQueueMiddleware.ObjectModels;
using log4net;

namespace Greenery.MessageQueueMiddleware.Implementations
{
    public class SendToClientChannel
    {
        public SendToClientChannel(MQMServer server, ILog logger)
        {
            Server = server;
            Logger = logger;
        }
        /// <summary>
        /// 消息中间件服务器对象
        /// </summary>
        public MQMServer Server { get; private set; }
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; private set; }

        public bool SendMesssage(ClientIndetity indetity, string message)
        {
            IClientChannel clentChannel = new SockectClientChannel(Server, indetity.SessionId, Logger);
            if (clentChannel.SendMessage(message))
            {
                return true;
            }
            if (indetity.IsWebSite && !string.IsNullOrEmpty(indetity.WebSiteHost))
            {
                clentChannel = new HttpClientChannel(indetity.WebSiteHost, Logger);
                if (clentChannel.SendMessage(message))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
