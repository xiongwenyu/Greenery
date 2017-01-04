﻿using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.SuperSocketProtocol
{
    /// <summary>
    /// 会话管理器
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// socket服务器对象
        /// </summary>
        IAppServer Server { get; set; }
        /// <summary>
        /// 登记一个会话
        /// </summary>
        /// <param name="newInfo">会话信息描述</param>
        void RegisterSession<TSessionInformaction>(TSessionInformaction info) where TSessionInformaction : ISessionInformaction;
        /// <summary>
        /// 宣告会话的存活，以刷新其在内存缓存中的激活时间，避免被逐出
        /// </summary>
        /// <param name="sInfo">会话信息描述</param>
        void NotifyAlive<TSessionInformaction>(TSessionInformaction info) where TSessionInformaction : ISessionInformaction;

        /// <summary>
        /// 释放一个会话，立即将会话逐出缓存并断开
        /// </summary>
        /// <param name="sInfo">会话信息描述</param>
        void ReleaseSession<TSessionInformaction>(TSessionInformaction info) where TSessionInformaction : ISessionInformaction;

        /// <summary>
        /// 使用idCode获取已登记会话，实际上设备会话缓存以idCode为键，设备会话信息为值进行缓存
        /// </summary>
        /// <param name="sInfo">会话信息描述</param>
        /// <returns>会话信息，如果缓存中不存在则返回null</returns>
        IAppSession GetSession<TSessionInformaction>(TSessionInformaction info) where TSessionInformaction : ISessionInformaction;

    }
}
