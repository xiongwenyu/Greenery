using Greenery.MessageQueueMiddleware;
using System;

namespace Greenery.MQMForRedis
{
    public class RedisMessageQueueAdapter : IMessageQueueAdapter
    {
        private static readonly string splite = "#@.@#";
        public void Publish(string[] descriptions, string content)
        {
            var key = string.Join(splite, descriptions);
            RedisManager.Publish(key, content);
        }

        public void Subscribe(string group, RemotePublish callback)
        {
            RedisManager.Subscribe(group + "*", (key, value) =>
            {
                var descriptions = key.ToString().Split(new string[] { splite }, StringSplitOptions.RemoveEmptyEntries);
                callback(descriptions, value);
            });

        }

        public void UnSubscribe(string group)
        {
            RedisManager.UnSubscribe(group + "*");
        }
    }
}
