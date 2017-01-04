﻿using System;

namespace Greenery.Event.ObjectModels
{
    /// <summary>
    /// 订阅项
    /// </summary>
    public class LocalSubscribeItem
    {
        /// <summary>
        /// 事件处理器（事件回调处理）
        /// </summary>
        internal IEventHandler handler { get; set; }
        /// <summary>
        /// 订阅过滤模式
        /// </summary>
        public FilterMode FilterMode { get; set; }
        /// <summary>
        /// 订阅内容描述
        /// </summary>
        public string[] Descriptions { get; set; }
        /// <summary>
        /// 订阅编号
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 是否取消订阅
        /// </summary>
        public bool IsUnSubscribe { get; set; }
    }
}
