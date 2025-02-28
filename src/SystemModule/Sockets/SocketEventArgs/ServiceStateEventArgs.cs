﻿using System;
using SystemModule.Sockets.Enum;

namespace SystemModule.Sockets.SocketEventArgs
{
    /// <summary>
    /// 服务器状态事件参数
    /// </summary>
    public class ServiceStateEventArgs : MsgEventArgs
    {
        /// <summary>
        /// 服务器状态事件参数
        /// </summary>
        /// <param name="serverState"></param>
        /// <param name="exception"></param>
        public ServiceStateEventArgs(ServerState serverState, Exception exception)
        {
            ServerState = serverState;
            Exception = exception;
        }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public ServerState ServerState { get; }

        /// <summary>
        /// 异常类
        /// </summary>
        public Exception Exception { get; }
    }
}