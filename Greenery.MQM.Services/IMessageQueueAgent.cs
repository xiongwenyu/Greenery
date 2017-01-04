﻿using Greenery.MessageQueueMiddleware.ObjectModels;
using System;

namespace Greenery.MessageQueueMiddleware
{
    public interface IMessageQueueAgent
    {
        Guid AgentId { get; }
        bool Authentication(ClientIndetity subscriber, string key);
        void Subscribe(Guid publisherId, SubscribeItem subscribeItem);
        void UnSubscribe(Guid publisherId, SubscribeItem subscribeItem);
        void Publish(PublishItem publishItem);
        void RetryRemoteSubscriptionCallbackFailure(ClientIndetity subscriber);
        void RemotePublishCallBack(string[] descriptions, string content);
    }
}
