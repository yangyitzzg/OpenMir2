using GameGate.Conf;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SystemModule;
using SystemModule.ByteManager;
using SystemModule.Core.Config;
using SystemModule.DataHandlingAdapters;
using SystemModule.Packets.ServerPackets;
using SystemModule.SocketComponents.Event;
using SystemModule.Sockets.Common;
using SystemModule.Sockets.Config;
using SystemModule.Sockets.Extensions;
using SystemModule.Sockets.Interface;
using SystemModule.Sockets.SocketEventArgs;
using NetworkMonitor = SystemModule.NetworkMonitor;
using TcpClient = SystemModule.Sockets.Components.TCP.TcpClient;

namespace GameGate.Services
{
    /// <summary>
    /// 网关客户端(GameGate-GameSrv)
    /// </summary>
    public class ClientThread
    {
        private readonly TcpClient ClientSocket;
        private readonly GameGateInfo GateInfo;
        private readonly IPEndPoint LocalEndPoint;
        /// <summary>
        /// 用户会话
        /// </summary>
        public readonly SessionInfo[] SessionArray = new SessionInfo[GateShare.MaxSession];
        /// <summary>
        ///  网关游戏服务器之间检测是否失败（超时）
        /// </summary>
        private bool CheckServerFail { get; set; }
        /// <summary>
        /// 网关游戏服务器之间检测是否失败次数
        /// </summary>
        private int CheckServerFailCount { get; set; }
        /// <summary>
        /// 网关是否就绪
        /// </summary>
        private bool GateReady { get; set; }
        /// <summary>
        /// 是否链接成功
        /// </summary>
        private bool Connected { get; set; }
        /// <summary>
        /// 历史最高在线人数
        /// </summary>
        private int OnlineCount { get; set; }
        /// <summary>
        /// 运行状态
        /// </summary>
        private RunningState RunningState { get; set; }
        private int CheckServerTick { get; set; }
        /// <summary>
        /// Session管理
        /// </summary>
        private static SessionContainer SessionContainer => SessionContainer.Instance;
        /// <summary>
        /// 日志
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly NetworkMonitor _networkMonitor;
        /// <summary>
        /// 发送封包（网关-》客户端）
        /// </summary>
        private readonly Channel<ServerSessionMessage> _messageChannel;

        public ClientThread(IPEndPoint endPoint, GameGateInfo gameGate, NetworkMonitor networkMonitor)
        {
            GateInfo = gameGate;
            RunningState = RunningState.Waiting;
            Connected = false;
            LocalEndPoint = endPoint;
            CheckServerTick = HUtil32.GetTickCount();
            _networkMonitor = networkMonitor;
            _messageChannel = Channel.CreateUnbounded<ServerSessionMessage>();
            ClientSocket = new TcpClient();
            ClientSocket.Connected += ClientSocketConnect;
            ClientSocket.Disconnected += ClientSocketDisconnect;
            ClientSocket.Received += ClientSocketRead;
            //ClientSocket.OnError += ClientSocketError;
        }

        public bool IsConnected => Connected;

        public string EndPoint => $"{GateInfo.ServerAdress}:{GateInfo.ServerPort}";

        public byte ThreadId => GateInfo.ServiceId;

        public RunningState Running => RunningState;
        
        /// <summary>
        /// 返回等待处理的消息数量
        /// </summary>
        public int QueueCount => _messageChannel.Reader.Count;

        public void Initialize()
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(IPAddress.Parse(GateInfo.ServerAdress), GateInfo.ServerPort))
                .SetBufferLength(1024);
            config.SetDataHandlingAdapter(() => new PacketFixedHeaderDataHandlingAdapter());
            ClientSocket.Setup(config);
        }

        public void Start()
        {
            try
            {
                ClientSocket.Connect();
            }
            catch (SocketException ex)
            {
                ClientSocketError(null, ex.SocketErrorCode);
            }
        }

        private void ReConnected()
        {
            if (Connected == false)
            {
                Start();
            }
        }

        public void Stop()
        {
            ClientSocket.Close();
        }

        public string GetSessionCount()
        {
            var sessionCount = 0;
            for (var i = 0; i < SessionArray.Length; i++)
            {
                if (SessionArray[i] != null && SessionArray[i].Socket != null)
                {
                    sessionCount++;
                }
            }
            if (sessionCount > OnlineCount)
            {
                OnlineCount = sessionCount;
            }
            return sessionCount + "/" + OnlineCount;
        }

        private void ClientSocketConnect(object sender, MsgEventArgs e)
        {
            var client = sender as TcpClient;
            GateReady = true;
            CheckServerTick = HUtil32.GetTickCount();
            Connected = true;
            RunningState = RunningState.Runing;
            RestSessionArray();
            _logger.Info($"[{LocalEndPoint}] 游戏引擎[{client.MainSocket.RemoteEndPoint}]链接成功.");
            _logger.Debug($"线程[{Guid.NewGuid():N}]连接 {client.MainSocket.RemoteEndPoint} 成功...");
        }

        private void ClientSocketDisconnect(object sender, DisconnectEventArgs e)
        {
            var client = sender as TcpClient;
            for (var i = 0; i < GateShare.MaxSession; i++)
            {
                var userSession = SessionArray[i];
                if (userSession != null)
                {
                    if (userSession.Socket != null && userSession.Socket == client.MainSocket)
                    {
                        userSession.Socket.Close();
                        userSession.Socket = null;
                        userSession.SckHandle = -1;
                    }
                }
            }
            RestSessionArray();
            GateReady = false;
            _logger.Info($"[{LocalEndPoint}] 游戏引擎[{client.MainSocket.RemoteEndPoint}]断开链接.");
            Connected = false;
            CheckServerFail = true;
        }

        /// <summary>
        /// 接收GameSvr发来的封包消息
        /// </summary>
        private void ClientSocketRead(object sender, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            try
            {
                if (requestInfo is DataMessageFixedHeaderRequestInfo myRequestInfo)
                {
                    ProcessServerPacket(myRequestInfo.Header, myRequestInfo.Message);
                    _networkMonitor.Receive(myRequestInfo.BodyLength);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        private void ClientSocketError(object sender, SocketError e)
        {
            switch (e)
            {
                case SocketError.ConnectionRefused:
                    _logger.Warn($"游戏网关[{LocalEndPoint}]链接游戏引擎[{EndPoint}]拒绝链接...");
                    Connected = false;
                    break;
                case SocketError.ConnectionReset:
                    _logger.Info($"游戏引擎[{EndPoint}]主动关闭连接游戏网关[{LocalEndPoint}]...");
                    Connected = false;
                    break;
                case SocketError.TimedOut:
                    _logger.Info($"游戏网关[{LocalEndPoint}]链接游戏引擎时[{EndPoint}]超时...");
                    Connected = false;
                    break;
            }
            GateReady = false;
            CheckServerFail = true;
        }

        private void ProcessServerPacket(ServerMessage packetHeader, ReadOnlySpan<byte> data)
        {
            switch (packetHeader.Ident)
            {
                case Grobal2.GM_STOP: //游戏引擎停止服务,网关不在接收或分配用户连接到该游戏引擎
                    RunningState = RunningState.Stop; //停止分配后，10分钟内不允许尝试连接服务器
                    break;
                case Grobal2.GM_CHECKSERVER:
                    CheckServerFail = false;
                    CheckServerTick = HUtil32.GetTickCount();
                    break;
                case Grobal2.GM_SERVERUSERINDEX:
                    var userSession = SessionContainer.GetSession(ThreadId, packetHeader.SessionId);
                    if (userSession != null)
                    {
                        userSession.SvrListIdx = packetHeader.SessionIndex;
                    }
                    break;
                case Grobal2.GM_RECEIVE_OK:
                    SendServerMsg(Grobal2.GM_RECEIVE_OK, 0, 0, 0, "", 0);
                    break;
                case Grobal2.GM_DATA:
                    unsafe
                    {
                        var packetLen = packetHeader.PackLength < 0 ? -packetHeader.PackLength : packetHeader.PackLength;
                        var sendMsg = new ServerSessionMessage();
                        sendMsg.SessionId = packetHeader.SessionId;
                        sendMsg.BuffLen = (short)packetHeader.PackLength;
                        //sendMsg.Buffer = GateShare.BytePool.Rent(packetLen);
                        sendMsg.Buffer = new byte[packetLen];
                        MemoryCopy.BlockCopy(data, 0, sendMsg.Buffer, 0, packetLen);
                        Enqueue(sendMsg);
                    }
                    break;
                case Messages.GM_TEST:
                    break;
            }
        }
        
        /// <summary>
        /// 添加到消息处理队列
        /// </summary>
        private void Enqueue(ServerSessionMessage sessionPacket)
        {
            _messageChannel.Writer.TryWrite(sessionPacket);
        }

        /// <summary>
        /// 转发GameSvr封包消息
        /// </summary>
        public Task StartMessageQueue(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(async () =>
               {
                   while (await _messageChannel.Reader.WaitToReadAsync(stoppingToken))
                   {
                       if (_messageChannel.Reader.TryRead(out var message))
                       {
                           var userSession = SessionContainer.GetSession(ThreadId, message.SessionId);
                           if (userSession == null)
                           {
                               continue;
                           }
                           try
                           {
                               userSession.ProcessServerPacket(ThreadId, message);
                           }
                           catch (Exception ex)
                           {
                               _logger.Error(ex);
                           }
                           finally
                           {
                               //GateShare.BytePool.Return(message.Buffer);
                           }
                       }
                   }
               }, stoppingToken);
        }

        public void RestSessionArray()
        {
            for (var i = 0; i < GateShare.MaxSession; i++)
            {
                if (SessionArray[i] != null)
                {
                    SessionArray[i].Socket = null;
                    SessionArray[i].SessionIndex = 0;
                    SessionArray[i].ReceiveTick = HUtil32.GetTickCount();
                    SessionArray[i].SckHandle = 0;
                    SessionArray[i].SessionId = 0;
                }
            }
        }

        private void SendServerMsg(ushort command, ushort sessionIndex, int nSocket, ushort userIndex, string data, int nLen)
        {
            var serverMessage = new ServerMessage
            {
                PacketCode = Grobal2.PacketCode,
                Socket = nSocket,
                SessionId = sessionIndex,
                Ident = command,
                SessionIndex = userIndex,
                PackLength = nLen,
            };
            var sendBuffer = SerializerUtil.Serialize(serverMessage);
            if (!string.IsNullOrEmpty(data))
            {
                var strBuff = HUtil32.GetBytes(data);
                var tempBuff = new byte[ServerMessage.PacketSize + data.Length];
                MemoryCopy.BlockCopy(sendBuffer, 0, tempBuff, 0, sendBuffer.Length);
                MemoryCopy.BlockCopy(strBuff, 0, tempBuff, sendBuffer.Length, data.Length);
                Send(tempBuff);
            }
            else
            {
                Send(sendBuffer);
            }
        }

        /// <summary>
        /// 玩家进入游戏
        /// </summary>
        public void UserEnter(ushort sessionId, int socketId, string data)
        {
            SendServerMsg(Grobal2.GM_OPEN, sessionId, socketId, 0, data, data.Length);
        }

        /// <summary>
        /// 玩家退出游戏
        /// </summary>
        public void UserLeave(int socketId)
        {
            SendServerMsg(Grobal2.GM_CLOSE, 0, socketId, 0, "", 0);
        }

        /// <summary>
        /// 发送消息到GameSvr
        /// </summary>
        /// <param name="sendBuffer"></param>
        internal void Send(byte[] sendBuffer)
        {
            if (ClientSocket.Online)
            {
                ClientSocket.Send(sendBuffer);
                _networkMonitor.Send(sendBuffer.Length);
            }
        }

        /// <summary>
        /// 处理超时或空闲会话
        /// </summary>
        public void ProcessIdleSession()
        {
            var currentTick = HUtil32.GetTickCount();
            for (var j = 0; j < SessionArray.Length; j++)
            {
                var userSession = SessionArray[j];
                if (userSession != null && userSession.Socket != null)
                {
                    if ((currentTick - userSession.ReceiveTick) > GateShare.SessionTimeOutTime) //清理超时用户会话 
                    {
                        userSession.Socket.Close();
                        userSession.SckHandle = -1;
                        userSession.Socket = null;
                    }
                }
            }
        }

        /// <summary>
        /// 检查客户端和服务端之间的状态以及心跳维护
        /// </summary>
        public void CheckConnectedState()
        {
            if (Running == RunningState.Stop) //停止服务后暂时停止心跳连接检查
            {
                if ((HUtil32.GetTickCount() - CheckServerTick) > 60 * 10000) //10分钟分不允许尽兴链接服务器
                {
                    ReConnected();
                    _logger.Debug($"游戏引擎维护时间结束,重新连接游戏引擎[{EndPoint}].");
                }
                return;
            }
            if (GateReady)
            {
                SendServerMsg(Grobal2.GM_CHECKCLIENT, 0, 0, 0, "", 0);
                CheckServerFailCount = 0;
                return;
            }
            if (CheckServerFail && CheckServerFailCount <= ushort.MaxValue)
            {
                ReConnected();
                CheckServerFailCount++;
                _logger.Debug($"链接服务器[{EndPoint}] 失败次数[{CheckServerFailCount}]");
                return;
            }
            if (CheckServerFailCount >= ushort.MaxValue)
            {
                _logger.Debug("超过最大重试次数，请重启程序后再次确认链接是否正常。");
                return;
            }
            CheckServerTimeOut();
        }

        private void CheckServerTimeOut()
        {
            if ((HUtil32.GetTickCount() - CheckServerTick) > GateShare.CheckServerTimeOutTime && CheckServerFailCount <= ushort.MaxValue)
            {
                CheckServerFail = true;
                Stop();
                CheckServerFailCount++;
                _logger.Debug($"服务器[{EndPoint}]长时间没有响应,断开链接.失败次数:[{CheckServerFailCount}]");
            }
        }

        public string ConnectedState => IsConnected ? "[green]Connected[/]" : "[red]Not Connected[/]";
    }
}