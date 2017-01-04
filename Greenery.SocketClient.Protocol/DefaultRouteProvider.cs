using System;

namespace Greenery.SocketClient.Protocol
{
    public class DefaultRouteProvider : IRouteProvider
    {
        public DefaultRouteProvider(int routeLength)
        {
            RouteLength = routeLength;
        }
        public string GetRoute(byte[] routeCode)
        {
            return BitConverter.ToString(routeCode);
        }

        public int RouteLength { get; private set; }
    }
}
