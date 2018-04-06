// See LICENSE file in the root directory
//

using System;
using System.Collections.Generic;
using UNET = UnityEngine.Networking;

namespace LGK.Networking.LLAPI.Server
{
    public class ServerPeer : IInternalServerPeer
    {
        private readonly byte[] m_RecievedBuffer = null;
        private readonly List<Connection> m_Connections;

        private readonly ConnectionConfig m_ConnectionConfig;
        private readonly ushort m_MaxConnection;

        private int m_SocketId = NetworkTransport.INVALID_SOCKET;
        private int m_Port;

        public event ServerEvent.ListenDelegate ListenEvent;
        public event ServerEvent.ShutdownDelegate ShutdownEvent;
        public event ServerEvent.ConnectDelegate ConnectedEvent;
        public event ServerEvent.DisconectDelegate DisconnectedEvent;
        public event ServerEvent.DataDelegate DataEvent;

        public ServerPeer(ServerConfig config)
        {
            m_Connections = new List<Connection>(config.MaxConnection);
            m_RecievedBuffer = new byte[config.BufferSize];

            m_ConnectionConfig = new ConnectionConfig();
            m_ConnectionConfig.ConnectTimeout = config.ConnectTimeout;
            m_ConnectionConfig.MaxConnectionAttempt = config.MaxConnectingTry;
            m_ConnectionConfig.DisconnectTimeout = config.DisconnectTimeout;

            m_MaxConnection = config.MaxConnection;
        }

        #region IInternalServerPeer implementation

        byte[] IInternalServerPeer.RecievedBuffer
        {
            get { return m_RecievedBuffer; }
        }

        #endregion

        #region INetworkServer implementation

        public bool IsActive
        {
            get { return m_SocketId != NetworkTransport.INVALID_SOCKET; }
        }

        public int Port
        {
            get { return m_Port; }
        }

        public bool Listen(int port)
        {
            return Listen("127.0.0.1", port);
        }

        public bool Listen(string address, int port)
        {
            m_Port = port;

            m_SocketId = UNET.NetworkTransport.AddHost(new SocketContract(m_ConnectionConfig, m_MaxConnection), m_Port, address);

            if (IsActive && ListenEvent != null)
                ListenEvent.Invoke();

            return this.IsActive;
        }

        public byte CreateChannel(ChannelType type)
        {
            if (IsActive)
                throw new Exception("Create channel just valid when peer not active");

            return m_ConnectionConfig.AddChannel((UNET.QosType)type);
        }

        public bool Send(int connectionId, int channelId, byte[] buffer, int length)
        {
            var conn = FindConnection(connectionId);
            if (conn == null)
                return false;

            return Send(conn, channelId, buffer, length);
        }

        public bool Send(IConnection conn, int channelId, byte[] buffer, int length)
        {
#if NETWORK_DEBUGGER_ENABLED
            var logMessage = new System.Text.StringBuilder("ServerPeer");
            logMessage.Append(" Outgoing");
            logMessage.Append(" ConnectionId:").Append(conn.ConnectionId);
            logMessage.Append(" ChannelId:").Append(channelId);
            logMessage.Append(" Size:").Append(length);

            UnityEngine.Debug.Log(logMessage);
#endif

            byte error;
            UNET.NetworkTransport.Send(m_SocketId, conn.ConnectionId, channelId, buffer, length, out error);

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
            var profilerName = new System.Text.StringBuilder("ServerPeer");
            profilerName.Append(channelId);
            Profiler.NetworkProfiler.RecordChannelOutgoing(profilerName.ToString(), (ushort)length);
#endif

            ((Connection)conn).LastError = (NetworkError)error;

            return conn.LastError == NetworkError.None;
        }

        public void Disconnect(int connectionId)
        {
            var conn = FindConnection(connectionId);
            if (conn == null)
                return;

            Disconnect(conn);

        }

        public void Disconnect(IConnection conn)
        {
            ((Connection)conn).LastError = NetworkError.None;

            InternalDisonnect((Connection)conn);
        }

        public void DisconnectAll()
        {
            for (int i = 0; i < m_Connections.Count; i++)
            {
                var conn = m_Connections[i];
                if (conn != null)
                {
                    conn.LastError = NetworkError.None;
                    InternalDisonnect(conn);
                }
            }
        }

        public void ProcessPacket()
        {
            if (m_SocketId == NetworkTransport.INVALID_SOCKET)
                return;

            int connectionId;
            int channelId;
            int receivedSize;
            byte error;
            UNET.NetworkEventType networkEvent;

            do
            {
                networkEvent = UNET.NetworkTransport.ReceiveFromHost(m_SocketId, out connectionId, out channelId, m_RecievedBuffer, (int)m_RecievedBuffer.Length, out receivedSize, out error);

                switch (networkEvent)
                {
                    case UNET.NetworkEventType.ConnectEvent:
                        HandleConnect(connectionId, (NetworkError)error);
                        break;
                    case UNET.NetworkEventType.DataEvent:
                        HandleData(connectionId, channelId, receivedSize, (NetworkError)error);
                        break;
                    case UNET.NetworkEventType.DisconnectEvent:
                        HandleDisconnect(connectionId, (NetworkError)error);
                        break;
                    case UNET.NetworkEventType.Nothing:
                        break;
                }
            }
            while (networkEvent != UNET.NetworkEventType.Nothing);
        }

        public void Shutdown()
        {
            if (m_SocketId == NetworkTransport.INVALID_SOCKET)
                return;

            var success = UNET.NetworkTransport.RemoveHost(m_SocketId);

            if (success && ShutdownEvent != null)
                ShutdownEvent.Invoke();

            m_SocketId = NetworkTransport.INVALID_SOCKET;
        }

        #endregion

        void HandleConnect(int connectionId, NetworkError error)
        {
            var connection = new Connection(m_SocketId, connectionId, true, error);

            // add connection at correct index
            while (m_Connections.Count <= connectionId)
            {
                m_Connections.Add(null);
            }
            m_Connections[connectionId] = connection;

            if(ConnectedEvent!= null)
                ConnectedEvent.Invoke(connection);
        }

        void HandleData(int connectionId, int channelId, int receivedSize, NetworkError error)
        {
            var client = FindConnection(connectionId);
            if (client == null)
                return;

            client.LastError = error;

#if NETWORK_DEBUGGER_ENABLED
            var logMessage = new System.Text.StringBuilder("ServerPeer");
            logMessage.Append(" Incoming");
            logMessage.Append(" ConnectionId:").Append(connectionId);
            logMessage.Append(" ChannelId:").Append(channelId);
            logMessage.Append(" Size:").Append(receivedSize);

            UnityEngine.Debug.Log(logMessage);
#endif

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
            var profilerName = new System.Text.StringBuilder("ServerPeer");
            profilerName.Append(channelId);
            Profiler.NetworkProfiler.RecordChannelIncoming(profilerName.ToString(), (ushort)receivedSize);
#endif
            if(DataEvent != null)
                DataEvent.Invoke(client, channelId, m_RecievedBuffer, receivedSize);
        }

        void HandleDisconnect(int connectionId, NetworkError error)
        {
            var conn = FindConnection(connectionId);
            if (conn == null)
                return;

            conn.LastError = error;

            InternalDisonnect(conn);
        }

        Connection FindConnection(int connectionId)
        {
            if (connectionId < 0 || connectionId >= m_Connections.Count)
                return null;

            return m_Connections[connectionId];
        }

        void InternalDisonnect(Connection conn)
        {
            byte disconnecingError;
            UNET.NetworkTransport.Disconnect(m_SocketId, conn.ConnectionId, out disconnecingError);

            conn.IsConnected = false;

            if(DisconnectedEvent!=null)
                DisconnectedEvent.Invoke(conn);

            m_Connections[conn.ConnectionId] = null;
        }
    }
}

