namespace Greenery.SocketClient.Protocol
{
    public class DefaultRouteReceiveFilter : RouteReceiveFilter<SockectPackageMessage>
    {
        public DefaultRouteReceiveFilter(IRouteProvider routeProvider)
            : base(routeProvider)
        {
            
        }

        public override SockectPackageMessage LoadPackageInfo(byte[] routeCode, byte[] bodyBuffer)
        {
            SockectPackageMessage msg = new SockectPackageMessage(RouteProvider.GetRoute(routeCode), bodyBuffer);
            return msg;
        }
    }
}
