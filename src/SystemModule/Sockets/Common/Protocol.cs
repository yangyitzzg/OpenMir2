using System;

namespace SystemModule.Sockets.Common
{
    /// <summary>
    /// 协议类
    /// </summary>
    public struct Protocol
    {
        /// <summary>
        /// 值
        /// </summary>
        private readonly string value;

        /// <summary>
        /// 表示无协议
        /// </summary>
        public static readonly Protocol None = new Protocol();

        /// <summary>
        /// 获取http协议
        /// </summary>
        public static readonly Protocol Http = new Protocol("http");

        /// <summary>
        /// TCP协议
        /// </summary>
        public static readonly Protocol TCP = new Protocol("tcp");

        /// <summary>
        /// UDP协议
        /// </summary>
        public static readonly Protocol UDP = new Protocol("udp");

        /// <summary>
        /// 获取WebSocket协议
        /// </summary>
        public static readonly Protocol WebSocket = new Protocol("ws");

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="value">值</param>
        public Protocol(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException();
            }
            this.value = value;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(value))
            {
                return "None";
            }
            return value;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (value == null)
            {
                return string.Empty.GetHashCode();
            }
            return value.ToLower().GetHashCode();
        }

        /// <summary>
        /// 比较是否和目标相等
        /// </summary>
        /// <param name="obj">目标</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Protocol)
            {
                return GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Protocol a, Protocol b)
        {
            if (string.IsNullOrEmpty(a.value) && string.IsNullOrEmpty(b.value))
            {
                return true;
            }
            return string.Equals(a.value, b.value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 不等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Protocol a, Protocol b)
        {
            bool state = a == b;
            return !state;
        }
    }
}