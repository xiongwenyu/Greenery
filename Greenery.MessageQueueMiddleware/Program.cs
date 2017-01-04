using System;

namespace Greenery.MessageQueueMiddleware
{
    class Program
    {
        static void Main(string[] args)
        {
            MQMServer.InitServer();
            
            Console.WriteLine("Press ENTER to exit the server.");
            Console.ReadLine();
        }
    }
}
