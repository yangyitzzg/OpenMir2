using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SystemModule.SocketComponents.Event;

namespace SystemModule.SocketComponents.AsyncSocketClient
{
    public class ScoketClient
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        private readonly int Buffersize = 1024;
        /// <summary>
        /// 客户端Socket
        /// </summary>
        private Socket _cli = null;
        /// <summary>
        /// 缓冲区
        /// </summary>
        private readonly byte[] _databuffer;
        /// <summary>
        /// 连接是否成功
        /// </summary>
        public bool IsConnected;
        public bool IsBusy = false;
        public IPEndPoint RemoteEndPoint;
        private int totalBytesRead;
        private int totalBytesWrite;
        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event DSCClientOnConnectedHandler OnConnected;
        /// <summary>
        /// 错误事件
        /// </summary>
        public event DSCClientOnErrorHandler OnError;
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event DSCClientOnReceiveHandler OnReceivedData;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DSCClientOnDisconnectedHandler OnDisconnected;

        public ScoketClient()
        {
            _databuffer = new byte[Buffersize];
        }

        public ScoketClient(IPEndPoint endPoint, int buffSize = 0)
        {
            if (buffSize <= 0)
            {
                buffSize = 1024;
            }
            Buffersize = buffSize;
            _databuffer = new byte[Buffersize];
            RemoteEndPoint = endPoint;
        }

        public void Connect()
        {
            if (RemoteEndPoint != null)
            {
                Connect(RemoteEndPoint);
                return;
            }
            throw new Exception("IP地址或端口号错误");
        }

        /// <summary>
        /// 获取接收到的字节总数
        /// </summary>
        public long TotalBytesRead
        {
            get { return totalBytesRead; }
        }

        /// <summary>
        /// 获取发送的字节总数
        /// </summary>
        public long TotalBytesWrite
        {
            get { return totalBytesWrite; }
        }

        public void Connect(IPEndPoint endPoint)//连接到终结点
        {
            try
            {
                _cli = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IsBusy = true;
                RemoteEndPoint = endPoint;
                _cli.BeginConnect(endPoint, HandleConnect, _cli);//开始异步连接
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                RaiseErrorEvent(exception);//引发错误事件
            }
        }

        public void Connect(string ip, int port)//连接到终结点
        {
            try
            {
                this._cli = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                IsBusy = true;
                _cli.BeginConnect(RemoteEndPoint, HandleConnect, _cli);//开始异步连接
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                RaiseErrorEvent(exception);//引发错误事件
            }
        }

        private void HandleConnect(IAsyncResult iar)
        {
            Socket asyncState = (Socket)iar.AsyncState;
            try
            {
                asyncState.EndConnect(iar); //结束异步连接
                if (null != OnConnected)
                {
                    IsConnected = true;
                    RemoteEndPoint = (IPEndPoint)_cli.RemoteEndPoint;
                    OnConnected(this, new DSCClientConnectedEventArgs(_cli)); //引发连接成功事件
                }
                StartWaitingForData(asyncState); //开始接收数据
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();
                IsConnected = false;
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent(); //引发断开连接事件
                }
                RaiseErrorEvent(exception); //引发错误事件
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StartWaitingForData(Socket soc)
        {
            try
            {
                //开始异步接收数据
                soc.BeginReceive(_databuffer, 0, Buffersize, SocketFlags.None, HandleIncomingData, soc);
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                RaiseErrorEvent(exception);//引发错误事件
            }
        }

        private void HandleIncomingData(IAsyncResult parameter)
        {
            Socket asyncState = (Socket)parameter.AsyncState;
            try
            {
                int length = asyncState.EndReceive(parameter);//结束异步接收数据
                if (0 == length)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                else
                {
                    // 增加接收到的字节总数
                    Interlocked.Add(ref totalBytesRead, length);
                    byte[] destinationArray = new byte[length];//目的字节数组
                    Array.Copy(_databuffer, destinationArray, length);//复制数据
                    OnReceivedData?.Invoke(this, new DSCClientDataInEventArgs(_cli, destinationArray, length)); //引发接收数据事件
                    StartWaitingForData(asyncState);//继续接收数据
                }
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();//引发断开连接事件
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                RaiseErrorEvent(exception);//引发错误事件
            }
        }

        public void SendText(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            byte[] buffer = System.Text.Encoding.GetEncoding("gb2312").GetBytes(str);
            Send(buffer);
        }

        public void Send(byte[] buffer)
        {
            try
            {
                //开始异步发送数据
                _cli.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, HandleSendFinished, _cli);
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();//引发断开连接事件
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                RaiseErrorEvent(exception);//引发错误事件
            }
        }

        private void HandleSendFinished(IAsyncResult parameter)
        {
            try
            {
                int length = ((Socket)parameter.AsyncState).EndSend(parameter);//结束异步发送数据
                                                                               // 增加发送计数器
                Interlocked.Add(ref totalBytesWrite, length);
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent();
            }
            catch (SocketException exception)
            {
                if (exception.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    RaiseDisconnectedEvent();//引发断开连接事件
                }
                RaiseErrorEvent(exception);
            }
            catch (Exception exception_debug)
            {
                Debug.WriteLine("调试：" + exception_debug.Message);
            }
        }

        private void RaiseDisconnectedEvent()
        {
            IsConnected = false;
            if (null != OnDisconnected)
            {
                OnDisconnected(this, new DSCClientConnectedEventArgs(_cli));
            }
        }

        private void RaiseErrorEvent(SocketException error)
        {
            if (null != OnError)
            {
                OnError(_cli.RemoteEndPoint, new DSCClientErrorEventArgs(_cli.RemoteEndPoint, error.SocketErrorCode, error));
            }
            IsConnected = false;
        }

        public void SendBuffer(byte[] data)
        {
            Send(data);
        }

        public void Close()
        {
            if (_cli != null)
            {
                _cli.Shutdown(SocketShutdown.Both);
                _cli.Disconnect(true);//Socket 复用
                _cli.Close();
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_cli != null)
                {
                    _cli.Shutdown(SocketShutdown.Both);
                    _cli.Disconnect(true);//Socket 复用
                    _cli.Close();
                }
            }
            catch (Exception) { }
        }

        public void Disconnect(bool isReUse)
        {
            try
            {
                if (_cli != null)
                {
                    _cli.Shutdown(SocketShutdown.Both);
                    _cli.Disconnect(isReUse);//Socket 复用
                    _cli.Close();
                }
            }
            catch (Exception) { }
        }
    }
}