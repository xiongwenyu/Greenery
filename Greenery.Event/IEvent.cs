﻿using Greenery.Event.ObjectModels;
using System;

namespace Greenery.Event
{
    /// <summary>
    /// 事件
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 事件编号（事件唯一标识)
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// 事件创建时间
        /// </summary>
        DateTime CreateDt { get; set; }
        /// <summary>
        /// 事件主题及分组
        /// </summary>
        EventDescription Descriptions { get; set; }
        /// <summary>
        /// 源事件发布器Id
        /// </summary>
        Guid PublisherId { get; set; }
    }
}
