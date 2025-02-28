using SystemModule.Core.Config;

namespace SystemModule.Sockets.SocketEventArgs
{
    /// <summary>
    /// ConfigEventArgs
    /// </summary>
    public class ConfigEventArgs : TouchSocketEventArgs
    {
        /// <summary>
        /// 实例化2ConfigEventArgs
        /// </summary>
        /// <param name="config"></param>
        public ConfigEventArgs(TouchSocketConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// 具体配置
        /// </summary>
        public TouchSocketConfig Config { get; }
    }
}