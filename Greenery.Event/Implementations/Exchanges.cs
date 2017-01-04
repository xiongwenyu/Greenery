using Greenery.Event.ObjectModels;
using System;
using System.Linq;
using Greenery.SocketClient.Protocol;
using System.Net;
using Greenery.MessageQueueMiddleware.ObjectModels;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Greenery.Event.Implementations
{
    public class Exchanges : BaseEventHandler<EventJsonWrapper>
    {
        static ExchangesClient socketClient;
        static Exchanges current;
        byte[] authRouteCode = new byte[] { 0x01, 0x00, 0x00, 0x01 };
        byte[] publishRouteCode = new byte[] { 0x01, 0x00, 0x00, 0x04 };
        byte[] subscribeRouteCode = new byte[] { 0x01, 0x00, 0x00, 0x02 };
        private readonly string cacheDirectoryName;

        public Exchanges(IPublisher publisher)
        {
            var assemblyFile = this.GetType().Assembly.CodeBase;
            var assemblyFileLike = new Uri(assemblyFile);
            var assemblyDirectoryName = Path.GetDirectoryName(assemblyFileLike.AbsolutePath);
            cacheDirectoryName = Path.Combine(assemblyDirectoryName, "MessageTransfer");
            if (!Directory.Exists(cacheDirectoryName))
                Directory.CreateDirectory(cacheDirectoryName);
            current = this;

            Publisher = publisher;
            Publisher.RemoteSubscribeHandler += RemoteSubscribeProxy;
            socketClient = new ExchangesClient();
            TryConnectRemote();
        }
        /// <summary>
        ///每10秒检查一次连接状态
        /// </summary>
        private void TryConnectRemote()
        {
            Task.Factory.StartNew(() =>
            {
                if (!socketClient.IsConnected)
                    ConnectRemote();
                Thread.Sleep(10000);
                TryConnectRemote();
            });

        }
        private void RemoteSubscribeProxy(Guid publisherId, FilterMode mode, string[] descriptions)
        {
            if (mode != FilterMode.FullEvent)
            {
                if (!socketClient.IsConnected)
                {
                    ConnectRemote();
                    return;
                }
                var message = socketClient.SendObjectToJsonStreamWithResponse(subscribeRouteCode, new SubscribeItem()
                {
                    Descriptions = descriptions,
                    FilterMode = mode,
                    PublisherId = publisherId
                });
                SocketResult<string> result;
                if (message.TryReadFromJsonStream(out result))
                {
                    if (result != null && result.Code != "200")
                    {
                        throw new Exception(result.Message);
                    }

                }
            }
        }

        private void ConnectRemote()
        {
            GreeneryEventConfigurationSection config = GreeneryEventConfigurationSection.GetConfig();
            var ips = Dns.GetHostAddresses(config.MQMIP);
            var task = socketClient.ConnectAsync(new IPEndPoint(ips.FirstOrDefault(), config.MQMPort));
            task.Wait();
            if (socketClient.IsConnected)
            {
                var authToken = new PublisherInformaction()
                {
                    IsWebSite = Publisher.IsWebSitePublisher,
                    MQMToken = config.MQMPassword,
                    PublisherId = Publisher.PublisherId,
                    WebSiteHost = Publisher.WebSiteHost
                };
                var message = socketClient.SendObjectToJsonStreamWithResponse(authRouteCode, authToken);
                SocketResult<string> result;
                if (message.TryReadFromJsonStream(out result))
                {
                    if (result != null && result.Code != "200")
                    {
                        throw new Exception(result.Message);
                    }
                    else
                    {
                        var subs = Publisher.GetSubscribes();
                        foreach (var item in subs)
                        {
                            RemoteSubscribeProxy(Publisher.PublisherId, item.FilterMode, item.Descriptions);
                        }
                        RetryFailed();
                    }
                }
            }
        }

        public void RetryFailed()
        {
            try
            {
                if (!Directory.Exists(cacheDirectoryName))
                    Directory.CreateDirectory(cacheDirectoryName);
                var files = Directory.GetFiles(cacheDirectoryName);
                foreach (var item in files)
                {
                    try
                    {
                        var fs = new FileStream(item, FileMode.Open, FileAccess.Read);

                        StreamReader sr = new StreamReader(fs);
                        var content = sr.ReadToEnd();
                        sr.Close();
                        fs.Close();
                        var eventObject = JsonConvert.DeserializeObject<EventJsonWrapper>(content);
                        var message = socketClient.SendObjectToJsonStreamWithResponse(publishRouteCode, new PublishItem() { Content = eventObject.ToString(), Descriptions = eventObject.Descriptions.Description, PublisherId = Publisher.PublisherId });
                        SocketResult<string> result;
                        if (message.TryReadFromJsonStream(out result))
                        {
                            if (result != null && result.Code == "200")
                            {
                                File.Delete(item);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //todolog
                    }
                }
            }
            catch (Exception)
            {
                //todolog
            }
        }

        public override void Execute(EventJsonWrapper eventObject)
        {
            if (!socketClient.IsConnected) ConnectRemote();
            if (!socketClient.IsConnected) goto DoError;

            var message = socketClient.SendObjectToJsonStreamWithResponse(publishRouteCode, new PublishItem() { Content = eventObject.ToString(), Descriptions = eventObject.Descriptions.Description, PublisherId = Publisher.PublisherId });
            SocketResult<string> result;
            if (message.TryReadFromJsonStream(out result))
            {
                if (result != null && result.Code != "200")
                {
                    goto DoError;
                }
                else
                {
                    return;
                }
            }
            DoError:
            if (!Directory.Exists(cacheDirectoryName))
                Directory.CreateDirectory(cacheDirectoryName);
            var file = Path.Combine(cacheDirectoryName, Guid.NewGuid().ToString("N"));
            var json = JsonConvert.SerializeObject(eventObject);
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }

        }

        private IPublisher Publisher { get; set; }



        public class ExchangesReciver : ISocketPackageHandler
        {
            public ExchangesReciver()
            {
                Route = new DefaultRouteProvider(4).GetRoute(new byte[] { 0x01, 0x00, 0x00, 0x05 });
            }
            public string Route { get; set; }

            public void RemoteCallback(SocketClient.Protocol.SocketClient client, SockectPackageMessage package)
            {
                string eventJson;
                if (package.TryReadFromTextStream(out eventJson))
                {
                    var task = current.Publisher.RemotePublishAsync(eventJson);
                    task.Wait();
                }
            }
        }
    }


    public class ExchangesClient : SocketClient.Protocol.SocketClient
    {
        public ExchangesClient() : base(new DefaultRouteProvider(4))
        {
            Initialize();
        }

    }
}
