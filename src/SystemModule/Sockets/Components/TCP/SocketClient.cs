using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SystemModule.ByteManager;
using SystemModule.Core.Collections.Concurrent;
using SystemModule.Core.Common;
using SystemModule.Core.Config;
using SystemModule.Core.Run.Action;
using SystemModule.Dependency;
using SystemModule.Extensions;
using SystemModule.Sockets.Common;
using SystemModule.Sockets.Common.Options;
using SystemModule.Sockets.Config;
using SystemModule.Sockets.DataAdapter;
using SystemModule.Sockets.Enum;
using SystemModule.Sockets.Exceptions;
using SystemModule.Sockets.Extensions;
using SystemModule.Sockets.Interface;
using SystemModule.Sockets.SocketEventArgs;
using BytePool = SystemModule.ByteManager.BytePool;
using SocketExtension = SystemModule.SocketComponents.SocketExtension;

namespace SystemModule.Sockets.Components.TCP
{
    /// <summary>
    /// 服务器辅助类
    /// </summary>
    [DebuggerDisplay("ID={ID},IPAdress={IP}:{Port}")]
    public class SocketClient : BaseSocket, ISocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketClient()
        {
            Protocol = Protocol.TCP;
        }

        #region 变量

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        internal string m_id;
        internal ReceiveType m_receiveType;
        internal TcpServiceBase m_service;
        private DataHandlingAdapter m_adapter;
        private DelaySender m_delaySender;
        private Socket m_mainSocket;

        //private int m_maxPackageSize;
        private bool m_online;

        private bool m_useDelaySender;
        private Stream m_workStream;

        #endregion 变量

        #region 属性

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TouchSocketConfig Config { get; internal set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => Config?.Container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter => m_adapter;

        /// <summary>
        /// 用于索引的ID
        /// </summary>
        public string ID => m_id;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Socket MainSocket => m_mainSocket;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Online => m_online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType => m_receiveType;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TcpServiceBase Service => m_service;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl { get; private set; }

        #endregion 属性

        #region 事件&委托

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DisconnectEventHandler<ITcpClientBase> Disconnected { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DisconnectEventHandler<ITcpClientBase> Disconnecting { get; set; }


        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时，可通过<see cref="TouchSocketEventArgs.IsPermitOperation"/>终止断开行为。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnecting(DisconnectEventArgs e)
        {
            try
            {
                Disconnecting?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error($"在事件{nameof(Disconnecting)}中发生错误。{0}", ex);
            }
        }

        /// <summary>
        /// 当客户端完整建立TCP连接，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(TouchSocketEventArgs e)
        {
            m_service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        protected virtual void OnConnecting(OperationEventArgs e)
        {
            m_service.OnInternalConnecting(this, e);
        }

        /// <summary>
        /// 在延迟发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnDelaySenderError(Exception ex)
        {
            _logger.Error("发送错误 {0}", ex);
        }

        /// <summary>
        /// 客户端已断开连接，如果从Connecting中拒绝连接，则不会触发。如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(DisconnectEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// 当初始化完成时，执行在<see cref="OnConnecting(OperationEventArgs)"/>之前。
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        private void PrivateOnDisconnected(DisconnectEventArgs e)
        {
            OnDisconnected(e);
            if (!e.Handled)
            {
                m_service.OnInternalDisconnected(this, e);
            }
        }

        private void PrivateOnDisconnecting(DisconnectEventArgs e)
        {
            OnDisconnecting(e);
            if (!e.Handled)
            {
                m_service.OnInternalDisconnecting(this, e);
            }
        }
        #endregion 事件&委托

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ServiceIP { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int ServicePort { get; private set; }

        /// <inheritdoc/>
        public virtual void Close()
        {
            Close($"主动调用{nameof(Close)}");
        }

        /// <inheritdoc/>
        public virtual void Close(string msg)
        {
            if (m_online)
            {
                DisconnectEventArgs args = new DisconnectEventArgs(true, msg)
                {
                    IsPermitOperation = true
                };
                PrivateOnDisconnecting(args);
                if (DisposedValue || args.IsPermitOperation)
                {
                    BreakOut(msg, true);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (m_workStream == null)
            {
                m_workStream = new NetworkStream(m_mainSocket, true);
            }
            return m_workStream;
        }

        /// <summary>
        /// 直接重置内部ID。
        /// </summary>
        /// <param name="newId"></param>
        protected void DirectResetID(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (m_id == newId)
            {
                return;
            }
            string oldId = m_id;
            if (Service.SocketClients.TryRemove(m_id, out SocketClient socketClient))
            {
                socketClient.m_id = newId;
                if (Service.SocketClients.TryAdd(socketClient))
                {
                    return;
                }
                else
                {
                    socketClient.m_id = oldId;
                    if (Service.SocketClients.TryAdd(socketClient))
                    {
                        throw new Exception("ID重复");
                    }
                    else
                    {
                        socketClient.Close("修改新ID时操作失败，且回退旧ID时也失败。");
                    }
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketStatus.ClientNotFind.GetDescription(oldId));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="newId"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void ResetID(string newId)
        {
            DirectResetID(newId);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (!CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            SetAdapter(adapter);
        }

        internal void BeginReceive(ReceiveType receiveType)
        {
            try
            {
                if (receiveType == ReceiveType.Auto)
                {
                    SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                    eventArgs.Completed += EventArgs_Completed;
                    ByteBlock byteBlock = BytePool.Default.GetByteBlock(BufferLength);
                    eventArgs.UserToken = byteBlock;
                    eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                    if (!m_mainSocket.ReceiveAsync(eventArgs))
                    {
                        ProcessReceived(eventArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                BreakOut(ex.Message, false);
            }
        }

        internal void InternalConnected(TouchSocketEventArgs e)
        {
            m_online = true;
            if (Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
            {
                m_useDelaySender = true;
                m_delaySender.SafeDispose();
                m_delaySender = new DelaySender(m_mainSocket, senderOption.QueueLength, OnDelaySenderError)
                {
                    DelayLength = senderOption.DelayLength
                };
            }
            OnConnected(e);
        }

        internal void InternalConnecting(OperationEventArgs e)
        {
            OnConnecting(e);
        }

        internal void InternalInitialized()
        {
            LastReceivedTime = DateTime.Now;
            LastSendTime = DateTime.Now;
            OnInitialized();
        }

        internal void SetSocket(Socket mainSocket)
        {
            m_mainSocket = mainSocket ?? throw new ArgumentNullException(nameof(mainSocket));
            IP = mainSocket.RemoteEndPoint.GetIP();
            Port = SocketExtension.GetPort(mainSocket.RemoteEndPoint);
            ServiceIP = mainSocket.LocalEndPoint.GetIP();
            ServicePort = SocketExtension.GetPort(mainSocket.LocalEndPoint);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (m_online)
            {
                DisconnectEventArgs args = new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开");
                PrivateOnDisconnecting(args);
            }
            Config = default;
            m_adapter.SafeDispose();
            m_adapter = default;
            BreakOut($"{nameof(Dispose)}主动断开", true);
            base.Dispose(disposing);
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected virtual void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(byte[] buffer, int offset, int length)
        {
            return true;
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(ReadOnlyMemory<byte> buffer, int offset, int length)
        {
            return true;
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (Config != null)
            {
                if (Config.GetValue(TouchSocketConfigExtension.MaxPackageSizeProperty) is int v1)
                {
                    adapter.MaxPackageSize = v1;
                }
                if (Config.GetValue(TouchSocketConfigExtension.CacheTimeoutProperty) != TimeSpan.Zero)
                {
                    adapter.CacheTimeout = Config.GetValue(TouchSocketConfigExtension.CacheTimeoutProperty);
                }
                if (Config.GetValue(TouchSocketConfigExtension.CacheTimeoutEnableProperty) is bool v2)
                {
                    adapter.CacheTimeoutEnable = v2;
                }
                if (Config.GetValue(TouchSocketConfigExtension.UpdateCacheTimeWhenRevProperty) is bool v3)
                {
                    adapter.UpdateCacheTimeWhenRev = v3;
                }
            }

            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = PrivateHandleReceivedData;
            adapter.SendCallBack = DefaultSend;
            m_adapter = adapter;
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (SyncRoot)
            {
                if (m_online)
                {
                    m_online = false;
                    this.TryShutdown();
                    m_mainSocket.SafeDispose();
                    m_delaySender.SafeDispose();
                    m_adapter.SafeDispose();
                    m_service?.SocketClients.TryRemove(m_id, out _);
                    PrivateOnDisconnected(new DisconnectEventArgs(manual, msg));
                    Disconnected = null;
                }
                base.Dispose(true);
            }
        }

        private void EndSsl(IAsyncResult result)
        {
            ByteBlock byteBlock = (ByteBlock)result.AsyncState;
            try
            {
                int r = m_workStream.EndRead(result);
                if (r == 0)
                {
                    BreakOut("远程终端主动关闭", false);
                }
                byteBlock.SetLength(r);
                HandleBuffer(byteBlock);
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                BreakOut(ex.Message, false);
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessReceived(e);
            }
            catch (Exception ex)
            {
                e.SafeDispose();
                BreakOut(ex.Message, false);
            }
        }

        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                LastReceivedTime = DateTime.Now;
                if (OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (DisposedValue)
                {
                    return;
                }
                if (m_adapter == null)
                {
                    _logger.Error(TouchSocketStatus.NullDataAdapter.GetDescription());
                    return;
                }
                m_adapter.ReceivedInput(byteBlock);
            }
            catch (Exception ex)
            {
                _logger.Error("在处理数据时发生错误 {0}", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }
            HandleReceivedData(byteBlock, requestInfo);
            m_service.OnInternalReceivedData(this, byteBlock, requestInfo);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (DisposedValue)
            {
                e.SafeDispose();
            }
            else
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    ByteBlock byteBlock = (ByteBlock)e.UserToken;
                    byteBlock.SetLength(e.BytesTransferred);
                    HandleBuffer(byteBlock);
                    try
                    {
                        ByteBlock newByteBlock = new ByteBlock(BufferLength);
                        e.UserToken = newByteBlock;
                        e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Capacity);

                        if (!m_mainSocket.ReceiveAsync(e))
                        {
                            ProcessReceived(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        BreakOut(ex.Message, false);
                    }
                }
                else
                {
                    e.SafeDispose();
                    BreakOut("远程主机主动断开连接", false);
                }
            }
        }

        #region 发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (!m_online)
            {
                throw new NotConnectedException(TouchSocketStatus.NotConnected.GetDescription());
            }
            if (HandleSendingData(buffer, offset, length))
            {
                if (UseSsl)
                {
                    m_workStream.Write(buffer, offset, length);
                }
                else
                {
                    if (m_useDelaySender && length < TouchSocketUtility.BigDataBoundary)
                    {
                        m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                    }
                    else
                    {
                        m_mainSocket.AbsoluteSend(buffer, offset, length);
                    }
                }
                LastSendTime = DateTime.Now;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void DefaultSend(ReadOnlyMemory<byte> buffer, int offset, int length)
        {
            if (!m_online)
            {
                throw new NotConnectedException(TouchSocketStatus.NotConnected.GetDescription());
            }
            if (HandleSendingData(buffer, offset, length))
            {
                if (UseSsl)
                {
                    //m_workStream.Write(buffer, offset, length);
                }
                else
                {
                    if (m_useDelaySender && length < TouchSocketUtility.BigDataBoundary)
                    {
                        m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                    }
                    else
                    {
                        m_mainSocket.AbsoluteSend(buffer, offset, length);
                    }
                }
                LastSendTime = DateTime.Now;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            return EasyTask.Run(() =>
            {
                DefaultSend(buffer, offset, length);
            });
        }

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(IRequestInfo requestInfo)
        {
            if (DisposedValue)
            {
                return;
            }
            if (m_adapter == null)
            {
                throw new ArgumentNullException(nameof(DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }
            if (!m_adapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            m_adapter.SendInput(requestInfo);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (DisposedValue)
            {
                return;
            }
            if (m_adapter == null)
            {
                throw new ArgumentNullException(nameof(DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }
            m_adapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(ReadOnlyMemory<byte> buffer, int offset, int length)
        {
            if (DisposedValue)
            {
                return;
            }
            if (m_adapter == null)
            {
                throw new ArgumentNullException(nameof(DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }
            m_adapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
            if (DisposedValue)
            {
                return;
            }
            if (m_adapter == null)
            {
                throw new ArgumentNullException(nameof(DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }
            if (m_adapter.CanSplicingSend)
            {
                m_adapter.SendInput(transferBytes);
            }
            else
            {
                ByteBlock byteBlock = new ByteBlock(BufferLength);
                try
                {
                    foreach (ArraySegment<byte> item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    m_adapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        #endregion 同步发送

        #region 异步发送

        /// <summary>
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            return EasyTask.Run(() =>
            {
                Send(buffer, offset, length);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return EasyTask.Run(() =>
            {
                Send(requestInfo);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return EasyTask.Run(() =>
            {
                Send(transferBytes);
            });
        }

        #endregion 异步发送

        #region ID发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            m_service.Send(id, buffer, offset, length);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(string id, ReadOnlyMemory<byte> buffer, int offset, int length)
        {
            m_service.Send(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void Send(string id, IRequestInfo requestInfo)
        {
            m_service.Send(id, requestInfo);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public Task SendAsync(string id, byte[] buffer, int offset, int length)
        {
            return m_service.SendAsync(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return m_service.SendAsync(id, requestInfo);
        }

        #endregion ID发送

        #endregion 发送
    }
}