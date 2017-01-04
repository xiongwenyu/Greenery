using System;
using Greenery.MessageQueueMiddleware.Extensions;
using System.Threading.Tasks;
using log4net;

namespace Greenery.MessageQueueMiddleware.Implementations.ClientChannels
{
    public class HttpClientChannel : IClientChannel
    {
        /// <summary>
        /// Web站点Http推送路径
        /// </summary>
        private static readonly string pulishWebRouteCode = "Greenery/Event";
        public HttpClientChannel(string host, ILog logger)
        {
            Host = host;
            Logger = logger;
        }
        /// <summary>
        /// Web 站点地址（根地址）
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; private set; }

        public bool SendMessage(string message)
        {
            try
            {
                var url = new Uri(new UriBuilder(Host).Uri, pulishWebRouteCode);
                url.PostString(message);
                return true;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew((e) =>
                {
                    var exception = e as Exception;
                    if (exception != null && Logger != null)
                    {
                        Logger.Error("Socket客户端消息管道推送消息失败！", exception);
                    }
                }, ex);
            }
            return false;
        }
    }
}
