// See LICENSE file in the root directory
//
using System;
using System.Net;
using UNET = UnityEngine.Networking;

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
using System.Text;
using LGK.Networking.Profiler;
#endif

namespace LGK.Networking.LLAPI.Client
{
    public class ClientPeer : IInternalClientPeer
    {
#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
        const string PROFILER_CATEGORY_PREFIX = "ClientPeer:";
#endif

        private const byte NOTHING_ERROR = 0;
        private const byte LET_OS_SELECT_PORT = 0;
        private const int MAX_EVENT_PER_FRAME = 500;

        private readonly byte[] m_RecievedBuffer = null;
        private readonly ConnectionConfig m_ConnectionConfig;
        private readonly byte m_MaxConnection;

        private ConnectState m_State;
        private string m_ServerIp;
        private int m_ServerPort;

        private Connection m_Connection;

        public event ClientEvent.ConnectingDelegate ConnectingEvent;
        public event ClientEvent.ConnectingFailedDelegate ConnectingFailedEvent;
        public event ClientEvent.ConnectedDelegate ConnectedEvent;
        public event ClientEvent.DisconectedDelegate DisconnectedEvent;
        public event ClientEvent.DataDelegate DataEvent;

        public ClientPeer(ClientConfig config)
        {
            m_State = ConnectState.None;
            m_RecievedBuffer = new byte[config.BufferSize];

            m_Connection = new Connection(NetworkTransport.INVALID_SOCKET, NetworkTransport.INVALID_CONNECTION, false, NetworkError.None);

            m_ConnectionConfig = new ConnectionConfig();
            m_ConnectionConfig.ConnectTimeout = config.ConnectTimeout;
            m_ConnectionConfig.MaxConnectionAttempt = config.MaxConnectingTry;
            m_ConnectionConfig.DisconnectTimeout = config.DisconnectTimeout;

            m_MaxConnection = 1;
        }

        #region IInternalClientPeer implementation

        byte[] IInternalClientPeer.RecievedBuffer
        {
            get { return m_RecievedBuffer; }
        }

        #endregion

        #region IClientPeer implementation

        public bool IsActive
        {
            get { return m_Connection.SocketId != NetworkTransport.INVALID_SOCKET; }
        }

        public IConnection Connection
        {
            get { return m_Connection; }
        }

        public bool IsConnected
        {
            get { return m_Connection.IsConnected; }
        }

        public NetworkError LastError
        {
            get { return m_Connection.LastError; }
        }

        public byte CreateChannel(ChannelType type)
        {
            if (IsActive)
                throw new Exception("Create channel just valid when peer not active");

            return m_ConnectionConfig.AddChannel((UNET.QosType)type);
        }

        public bool Connect(string serverAdress, int serverPort)
        {
            m_ServerPort = serverPort;

            InternalCreateHost();

            if (this.IsActive)
                StartConnecting(serverAdress);

            return this.IsActive;
        }

        public bool Send(int channelId, byte[] buffer, int length)
        {
#if NETWORK_DEBUGGER_ENABLED
            UnityEngine.Debug.Log($"ClientPeer Outgoing : ChannelId:{channelId} Size:{length}");
#endif
            byte error;
            UNET.NetworkTransport.Send(m_Connection.SocketId, m_Connection.ConnectionId, channelId, buffer, length, out error);

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
            var profilerName = new StringBuilder(PROFILER_CATEGORY_PREFIX);
            profilerName.Append(channelId);
            NetworkProfiler.RecordChannelOutgoing(profilerName.ToString(), (ushort)length);
#endif

            m_Connection.LastError = (NetworkError)error;

            return m_Connection.LastError == NetworkError.None;
        }

        public void ProcessPacket()
        {
            switch (m_State)
            {
                case ConnectState.None:
                case ConnectState.Resolving:
                case ConnectState.Disconnected:
                    return;

                case ConnectState.Failed:
                    HandleConnectingFailed();
                    m_State = ConnectState.Disconnected;
                    return;

                case ConnectState.Resolved:
                    m_State = ConnectState.Connecting;
                    ContinueConnecting();
                    return;

                case ConnectState.Connecting:
                case ConnectState.Connected:
                    break;
            }

            if (m_Connection.ConnectionId == NetworkTransport.INVALID_CONNECTION)
                return;

            int numEvents = 0;
            UNET.NetworkEventType networkEvent;
            do
            {
                int connectionId;
                int channelId;
                int receivedSize;
                byte error;

                networkEvent = UNET.NetworkTransport.ReceiveFromHost(m_Connection.SocketId, out connectionId, out channelId, m_RecievedBuffer, (ushort)m_RecievedBuffer.Length, out receivedSize, out error);
                m_Connection.LastError = (NetworkError)error;

                switch (networkEvent)
                {
                    case UNET.NetworkEventType.ConnectEvent:
                        HandleConnectEvent();
                        break;
                    case UNET.NetworkEventType.DataEvent:
                        HandleDataEvent(channelId, receivedSize);
                        break;
                    case UNET.NetworkEventType.DisconnectEvent:
                        HandleDisconnectEvent();
                        break;
                    case UNET.NetworkEventType.Nothing:
                        break;
                }

                if (++numEvents >= MAX_EVENT_PER_FRAME)
                    break;
                if (m_Connection.SocketId == NetworkTransport.INVALID_SOCKET)
                    break;
            }
            while (networkEvent != UNET.NetworkEventType.Nothing);
        }

        public void Disconnect()
        {
            if (!IsActive)
                return;

            if (m_Connection.ConnectionId != NetworkTransport.INVALID_CONNECTION)
            {
                byte error;
                UNET.NetworkTransport.Disconnect(m_Connection.SocketId, m_Connection.ConnectionId, out error);
                m_Connection.ConnectionId = NetworkTransport.INVALID_CONNECTION;
                m_Connection.LastError = (NetworkError)error;
            }

            if (m_State == ConnectState.Connected)
            {
                m_State = ConnectState.Disconnected;
                m_Connection.IsConnected = false;
                m_ServerIp = string.Empty;
                m_ServerPort = 0;

                InternalRemoveHost();

                if (DisconnectedEvent != null)
                    DisconnectedEvent.Invoke();
            }
            else
                HandleConnectingFailed();
        }

        #endregion

        void StartConnecting(string serverAdress)
        {
            if (serverAdress.Equals("127.0.0.1") || serverAdress.Equals("localhost"))
            {
                m_ServerIp = "127.0.0.1";
                m_State = ConnectState.Resolved;
            }
            else if (serverAdress.IndexOf(':') != -1 && NetworkUtility.IsValidIpV6(serverAdress))
            {
                m_ServerIp = serverAdress;
                m_State = ConnectState.Resolved;
            }
            else
            {
                m_State = ConnectState.Resolving;
                Dns.BeginGetHostAddresses(serverAdress, HandleDNSResult, this);
            }

            ConnectingEvent?.Invoke();
        }

        void ContinueConnecting()
        {
            byte errorCode;
            m_Connection.ConnectionId = UNET.NetworkTransport.Connect(m_Connection.SocketId, m_ServerIp, m_ServerPort, NetworkTransport.INVALID_CONNECTION, out errorCode);
            m_Connection.LastError = (NetworkError)errorCode;
        }

        void HandleConnectingFailed()
        {
            m_ServerIp = string.Empty;
            m_ServerPort = 0;
            m_Connection = new Connection(NetworkTransport.INVALID_SOCKET, NetworkTransport.INVALID_CONNECTION, false, NetworkError.None);

            InternalRemoveHost();

            ConnectingFailedEvent?.Invoke(m_Connection.LastError);
        }

        void HandleConnectEvent()
        {
            m_State = ConnectState.Connected;

            m_Connection.IsConnected = true;

            ConnectedEvent?.Invoke();
        }

        void HandleDataEvent(int channelId, int receivedSize)
        {
#if NETWORK_DEBUGGER_ENABLED
            UnityEngine.Debug.Log($"ClientPeer Incoming : ChannelId:{channelId} Size:{receivedSize}");
#endif

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
            var profilerName = new StringBuilder(PROFILER_CATEGORY_PREFIX);
            profilerName.Append(channelId);
            NetworkProfiler.RecordChannelIncoming(profilerName.ToString(), (ushort)receivedSize);
#endif

            DataEvent?.Invoke(channelId, m_RecievedBuffer, receivedSize);
        }

        void HandleDisconnectEvent()
        {
            if (m_State == ConnectState.Connecting)
            {
                HandleConnectingFailed();
            }
            else
            {
                m_State = ConnectState.Disconnected;
                m_Connection.ConnectionId = NetworkTransport.INVALID_CONNECTION;
                m_Connection.IsConnected = false;
                m_ServerIp = string.Empty;
                m_ServerPort = 0;

                InternalRemoveHost();

                DisconnectedEvent?.Invoke();

            }
        }

        void HandleDNSResult(IAsyncResult ar)
        {
            try
            {
                IPAddress[] ip = Dns.EndGetHostAddresses(ar);

                if (ip.Length == 0)
                {
                    m_Connection.LastError = NetworkError.DNSFailure;
                    m_State = ConnectState.Failed;
                    return;
                }

                m_Connection.LastError = NetworkError.None;
                m_ServerIp = ip[0].ToString();
                m_State = ConnectState.Resolved;
            }
            catch
            {
                m_Connection.LastError = NetworkError.DNSFailure;
                m_State = ConnectState.Failed;
            }
        }

        void InternalCreateHost()
        {
            m_Connection.SocketId = UNET.NetworkTransport.AddHost(new SocketContract(m_ConnectionConfig, m_MaxConnection), LET_OS_SELECT_PORT);
        }

        void InternalRemoveHost()
        {
            if (m_Connection.SocketId == NetworkTransport.INVALID_SOCKET)
                return;

            UNET.NetworkTransport.RemoveHost(m_Connection.SocketId);
            m_Connection.SocketId = NetworkTransport.INVALID_SOCKET;
        }

        protected enum ConnectState
        {
            None,
            Resolving,
            Resolved,
            Connecting,
            Connected,
            Disconnected,
            Failed
        }
    }
}

