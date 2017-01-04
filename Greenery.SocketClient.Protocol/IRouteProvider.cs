namespace Greenery.SocketClient.Protocol
{
    public interface IRouteProvider
    {
        string GetRoute(byte[] routeCode);
        int RouteLength { get; }
    }
}
