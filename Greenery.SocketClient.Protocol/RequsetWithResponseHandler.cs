using System.Threading;
using System.Threading.Tasks;

namespace Greenery.SocketClient.Protocol
{
    internal class RequsetWithResponseHandler : ISocketPackageHandler
    {
        public RequsetWithResponseHandler(SocketClient client, params byte[] routeCode)
        {
            Route = client.RouteProvider.GetRoute(routeCode);
            RouteCode = routeCode;
            Package = null;
            Client = client;
            autoEvent = new AutoResetEvent(false);
        }

        private const int Timeout = 120000;
        private byte[] RouteCode { get; set; }
        AutoResetEvent autoEvent;
        public string Route { get; set; }
        public SocketClient Client { get; set; }

        private SockectPackageMessage Package { get; set; }
        public void RemoteCallback(SocketClient client, SockectPackageMessage package)
        {
            Package = package;
            autoEvent.Set();
        }

        public SockectPackageMessage Request(byte[] body)
        {
            Client.ResponseHandlers.Add(this);
            var task = Task.Factory.StartNew(() =>
            {
                Client.SendBytes(RouteCode, body);
                if (autoEvent.WaitOne(Timeout) && Package != null)
                {
                    Client.ResponseHandlers.Remove(this);
                    return Package;
                }
                else
                {
                    Client.ResponseHandlers.Remove(this);
                    return null;
                }

            });
            task.Wait();
            return task.Result;
        }
    }
}
