using System.ComponentModel;

namespace SystemModule.Sockets.Enum
{
    /// <summary>
    /// TouchSocket资源枚举
    /// </summary>
    public enum TouchSocketStatus : byte
    {
        /// <summary>
        /// 未知错误
        /// </summary>
        [Description("未知错误")]
        UnknownError,

        /// <summary>
        /// 操作成功
        /// </summary>
        [Description("操作成功")]
        Success,

        /// <summary>
        /// 操作超时
        /// </summary>
        [Description("操作超时")]
        Overtime,

        /// <summary>
        /// 用户主动取消操作。
        /// </summary>
        [Description("用户主动取消操作。")]
        Canceled,

        /// <summary>
        /// 参数‘{0}’为空。
        /// </summary>
        [Description("参数‘{0}’为空。")]
        ArgumentNull,

        /// <summary>
        ///发生异常，信息：{0}。
        /// </summary>
        [Description("发生异常，信息：{0}。")]
        Exception,

        #region TouchRpc

        /// <summary>
        /// 不允许路由该包，信息：{0}。
        /// </summary>
        [Description("不允许路由该包，信息：{0}。")]
        RoutingNotAllowed,

        /// <summary>
        /// 未找到该公共方法，或该方法未标记为Rpc
        /// </summary>
        [Description("未找到该公共方法，或该方法未标记为Rpc")]
        RpcMethodNotFind,

        /// <summary>
        /// 方法已被禁用
        /// </summary>
        [Description("方法已被禁用")]
        RpcMethodDisable,

        /// <summary>
        /// 函数执行异常，详细信息：{0}
        /// </summary>
        [Description("函数执行异常，详细信息：{0}")]
        RpcInvokeException,

        /// <summary>
        /// 事件操作器异常
        /// </summary>
        [Description("事件操作器异常。")]
        GetEventArgsFail,

        /// <summary>
        /// 通道设置失败。
        /// </summary>
        [Description("通道设置失败。")]
        SetChannelFail,

        /// <summary>
        /// ID为{0}的通道已存在。
        /// </summary>
        [Description("ID为{0}的通道已存在。")]
        ChannelExisted,

        /// <summary>
        /// 远程终端拒绝该操作，反馈信息：{0}。
        /// </summary>
        [Description("远程终端拒绝该操作，反馈信息：{0}。")]
        RemoteRefuse,

        /// <summary>
        /// 从‘{0}’创建写入流失败，信息：{1}。"
        /// </summary>
        [Description("从‘{0}’创建写入流失败，信息：{1}。")]
        CreateWriteStreamFail,

        /// <summary>
        ///没有找到路径‘{0}’对应的流文件。
        /// </summary>
        [Description("没有找到路径‘{0}’对应的流文件。")]
        StreamNotFind,

        /// <summary>
        /// 没有找到ID为{0}的客户端。
        /// </summary>
        [Description("没有找到ID为{0}的客户端。")]
        ClientNotFind,

        /// <summary>
        /// 路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。
        /// </summary>
        [Description("路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。")]
        StreamReferencing,

        /// <summary>
        /// 接收流容器为空
        /// </summary>
        [Description("流容器为空。")]
        StreamBucketNull,

        /// <summary>
        /// 从‘{0}’路径加载流异常，信息：‘{1}’。
        /// </summary>
        [Description("从‘{0}’路径加载流异常，信息：‘{1}’。")]
        LoadStreamFail,

        /// <summary>
        /// 目录‘{0}’已存在。
        /// </summary>
        [Description("目录‘{0}’已存在。")]
        DirectoryExisted,

        /// <summary>
        /// 文件‘{0}’已存在。
        /// </summary>
        [Description("文件‘{0}’已存在。")]
        FileExisted,

        /// <summary>
        /// 文件‘{0}’不存在。
        /// </summary>
        [Description("文件‘{0}’不存在。")]
        FileNotExists,

        /// <summary>
        /// 目录‘{0}’不存在。
        /// </summary>
        [Description("目录‘{0}’不存在。")]
        DirectoryNotExists,

        /// <summary>
        /// 名称为“{0}”的事件已存在
        /// </summary>
        [Description("名称为“{0}”的事件已存在。")]
        EventExisted,

        /// <summary>
        /// 名称为“{0}”的事件不存在
        /// </summary>
        [Description("名称为“{0}”的事件不存在。")]
        EventNotExist,

        /// <summary>
        /// 资源句柄{0}对应的资源没有找到，可能操作已超时。
        /// </summary>
        [Description("资源句柄{0}对应的资源没有找到，可能操作已超时。")]
        ResourceHandleNotFind,

        /// <summary>
        /// 还有{0}个资源没有完成。
        /// </summary>
        [Description("还有{0}个资源没有完成。")]
        HasUnFinished,

        /// <summary>
        /// 文件长度太长。
        /// </summary>
        [Description("文件长度太长。")]
        FileLengthTooLong,

        /// <summary>
        /// 读取文件长度错误。
        /// </summary>
        [Description("读取文件长度错误。")]
        LengthErrorWhenRead,

        /// <summary>
        /// 没有找到任何可用的目标Id。
        /// </summary>
        [Description("没有找到任何可用的目标Id。")]
        NotFindAnyTargetId,

        #endregion TouchRpc

        #region Client

        /// <summary>
        /// 数据处理适配器为空，可能客户端已掉线。
        /// </summary>
        [Description("数据处理适配器为空，可能客户端已掉线。")]
        NullDataAdapter,

        /// <summary>
        /// 客户端没有连接
        /// </summary>
        [Description("客户端没有连接。")]
        NotConnected,

        #endregion Client
    }
}