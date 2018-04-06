// See LICENSE file in the root directory
//

using System.Collections.Generic;

namespace LGK.Networking.LLAPI.Server
{
    public class ServerNetworkManager : IServerNetworkManager
    {
        const byte HEADER_BYTE_COUNT = 4;

        readonly IInternalServerPeer m_ServerPeer;

        private readonly NetworkWriter m_NetworkWriter;
        private readonly NetworkSyncWriter m_NetworkSyncWriter;
        private readonly NetworkReader m_NetworkReader;

        private readonly byte m_ReliableChannel;
        private readonly byte m_ReliableSequenceChannel;
        private readonly byte m_ReliableStateUpdateChannel;
        private readonly byte m_UnrealiableChannel;

        public event ServerEvent.ListenDelegate ListenEvent;
        public event ServerEvent.ShutdownDelegate ShutdownEvent;
        public event ServerEvent.ConnectDelegate ConnectedEvent;
        public event ServerEvent.DisconectDelegate DisconnectedEvent;

        public Dictionary<ushort, MessageHandlerDelegate> m_Handlers;

        public ServerNetworkManager(ServerConfig config)
        {
            m_Handlers = new Dictionary<ushort, MessageHandlerDelegate>();
            m_ServerPeer = new ServerPeer(config);

            m_ReliableChannel = m_ServerPeer.CreateChannel(ChannelType.Reliable);
            m_ReliableSequenceChannel = m_ServerPeer.CreateChannel(ChannelType.ReliableSequenced);
            m_ReliableStateUpdateChannel = m_ServerPeer.CreateChannel(ChannelType.ReliableStateUpdate);
            m_UnrealiableChannel = m_ServerPeer.CreateChannel(ChannelType.Unreliable);

            m_NetworkWriter = new NetworkWriter(new byte[config.BufferSize]);
            m_NetworkReader = new NetworkReader(m_ServerPeer.RecievedBuffer);

            m_NetworkSyncWriter = new NetworkSyncWriter(m_NetworkWriter);

            m_ServerPeer.ListenEvent += HandleListenEvent;
            m_ServerPeer.ShutdownEvent += HandleShutdownEvent;
            m_ServerPeer.ConnectedEvent += HandleConnectedEvent;
            m_ServerPeer.DisconnectedEvent += HandleDisconnectedEvent;
            m_ServerPeer.DataEvent += HandleDataEvent;
        }

        public bool IsActive { get { return m_ServerPeer.IsActive; } }

        public int Port { get { return m_ServerPeer.Port; } }

        public bool Listen(int port)
        {
            NetworkTransport.InitContext(this);

            return m_ServerPeer.Listen(port);
        }

        public bool Listen(string address, int port)
        {
            return m_ServerPeer.Listen(address, port);
        }

        public void ProcessMessage()
        {
            m_ServerPeer.ProcessPacket();
        }

        public void SendSyncObject(int connId, ushort msgType, IObjectSerializer serializer)
        {
            SerializeObjectToBuffer(msgType, serializer);

            DebugOutgoingMessage(connId, msgType, m_ReliableChannel);

            SendBuffer(connId, m_ReliableChannel);

            ProfileOutgoingMessage(msgType);
        }

        public void SendSyncObject(IList<int> connIds, ushort msgType, IObjectSerializer serializer)
        {
            SerializeObjectToBuffer(msgType, serializer);

            var count = connIds.Count;
            for (int i = 0; i < count; i++)
            {
                DebugOutgoingMessage(connIds[i], msgType, m_ReliableChannel);

                SendBuffer(connIds[i], m_ReliableChannel);

                ProfileOutgoingMessage(msgType);
            }
        }

        public void SendReliable(int connId, ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(connId, msgType, m_ReliableChannel);

            SendBuffer(connId, m_ReliableChannel);

            ProfileOutgoingMessage(msgType);
        }

        public void SendReliable(IList<int> connIds, ushort msgType, IMessageSerializer serializer)
        {
            if (connIds.Count == 0) return;

            SerializeMessageToBuffer(msgType, serializer);

            var count = connIds.Count;
            for (int i = 0; i < count; i++)
            {
                DebugOutgoingMessage(connIds[i], msgType, m_ReliableChannel);

                SendBuffer(connIds[i], m_ReliableChannel);

                ProfileOutgoingMessage(msgType);
            }
        }

        public void SendReliableSequence(int connId, ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(connId, msgType, m_ReliableSequenceChannel);

            SendBuffer(connId, m_ReliableSequenceChannel);

            ProfileOutgoingMessage(msgType);
        }

        public void SendReliableSequence(IList<int> connIds, ushort msgType, IMessageSerializer serializer)
        {
            if (connIds.Count == 0) return;

            SerializeMessageToBuffer(msgType, serializer);

            var count = connIds.Count;
            for (int i = 0; i < count; i++)
            {
                DebugOutgoingMessage(connIds[i], msgType, m_ReliableSequenceChannel);

                SendBuffer(connIds[i], m_ReliableSequenceChannel);

                ProfileOutgoingMessage(msgType);
            }
        }

        public void SendReliableStateUpdate(int connId, ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(connId, msgType, m_ReliableStateUpdateChannel);

            SendBuffer(connId, m_ReliableStateUpdateChannel);

            ProfileOutgoingMessage(msgType);
        }

        public void SendReliableStateUpdate(IList<int> connIds, ushort msgType, IMessageSerializer serializer)
        {
            if (connIds.Count == 0) return;

            SerializeMessageToBuffer(msgType, serializer);

            var count = connIds.Count;
            for (int i = 0; i < count; i++)
            {
                DebugOutgoingMessage(connIds[i], msgType, m_ReliableStateUpdateChannel);

                SendBuffer(connIds[i], m_ReliableStateUpdateChannel);

                ProfileOutgoingMessage(msgType);
            }
        }

        public void SendUnreliable(int connId, ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(connId, msgType, m_UnrealiableChannel);

            SendBuffer(connId, m_UnrealiableChannel);

            ProfileOutgoingMessage(msgType);
        }

        public void SendUnreliable(IList<int> connIds, ushort msgType, IMessageSerializer serializer)
        {
            if (connIds.Count == 0) return;

            SerializeMessageToBuffer(msgType, serializer);

            var count = connIds.Count;
            for (int i = 0; i < count; i++)
            {
                DebugOutgoingMessage(connIds[i], msgType, m_UnrealiableChannel);

                SendBuffer(connIds[i], m_UnrealiableChannel);

                ProfileOutgoingMessage(msgType);
            }
        }

        void SerializeObjectToBuffer(ushort msgType, IObjectSerializer serializer)
        {
            m_NetworkWriter.SeekZero();
            m_NetworkWriter.StartMessage(msgType);
            serializer.Serialize(m_NetworkSyncWriter);
            m_NetworkWriter.FinishMessage();
        }

        void SerializeMessageToBuffer(ushort msgType, IMessageSerializer serializer)
        {
            m_NetworkWriter.SeekZero();
            m_NetworkWriter.StartMessage(msgType);
            serializer.Serialize(m_NetworkWriter);
            m_NetworkWriter.FinishMessage();
        }

        void SendBuffer(int connId, byte channelType)
        {
            if (m_NetworkWriter.FilledLength <= HEADER_BYTE_COUNT)
                return;

            m_ServerPeer.Send(connId, channelType, m_NetworkWriter.BufferArray, m_NetworkWriter.FilledLength);
        }

        void DebugOutgoingMessage(int connectionId, ushort msgType, byte channelType)
        {
#if NETWORK_DEBUGGER_ENABLED
            var logMessage = new System.Text.StringBuilder("ServerNetworkManager");
            logMessage.Append(" Outgoing");
            logMessage.Append((m_NetworkWriter.FilledLength <= HEADER_BYTE_COUNT) ? " Ignored" : "");
            logMessage.Append(" ConnectionId:").Append(connectionId);
            logMessage.Append(" MsgType:").Append(msgType);
            logMessage.Append(" ChannelId:").Append(channelType);
            logMessage.Append(" Size:").Append(m_NetworkWriter.FilledLength);
#endif
        }

        void ProfileOutgoingMessage(ushort msgType)
        {
#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
            var profilerName = new System.Text.StringBuilder("ServerNetworkManager");
            profilerName.Append(msgType);
            Profiler.NetworkProfiler.RecordMessageOutgoing(profilerName.ToString(), m_NetworkWriter.FilledLength);
#endif
        }

        public void Disconnect(int connId)
        {
            m_ServerPeer.Disconnect(connId);
        }

        public void Disconnect(IConnection conn)
        {
            m_ServerPeer.Disconnect(conn);
        }

        public void DisconnectAll()
        {
            m_ServerPeer.DisconnectAll();
        }

        public void Shutdown()
        {
            m_ServerPeer.Shutdown();

            NetworkTransport.DestroyContext(this);
        }

        public void RegisterHandler(ushort msgType, MessageHandlerDelegate handler)
        {
            m_Handlers.Add(msgType, handler);
        }

        public void RemoveHandler(ushort msgType)
        {
            m_Handlers.Remove(msgType);
        }

        void HandleListenEvent()
        {
            if(ListenEvent != null)
                ListenEvent.Invoke();
        }

        void HandleShutdownEvent()
        {
            if(ShutdownEvent != null)
                ShutdownEvent.Invoke();
        }

        void HandleConnectedEvent(IConnection conn)
        {
            if(ConnectedEvent != null)
                ConnectedEvent.Invoke(conn);
        }

        void HandleDisconnectedEvent(IConnection conn)
        {
            if(DisconnectedEvent != null)
                DisconnectedEvent.Invoke(conn);
        }

        void HandleDataEvent(IConnection conn, int channelId, byte[] buffer, int length)
        {
            var lastIndex = length - 1;

            m_NetworkReader.SeekZero();

            MessageHandlerDelegate handler;
            while (m_NetworkReader.Position < lastIndex)
            {
                m_NetworkReader.Lock(HEADER_BYTE_COUNT);

                // the reader passed to user code has a copy of bytes from the real stream. user code never touches the real stream.
                // this ensures it can never get out of sync if user code reads less or more than the real amount.
                ushort msgSize = m_NetworkReader.ReadUInt16();
                ushort msgType = m_NetworkReader.ReadUInt16();

#if NETWORK_DEBUGGER_ENABLED
                var logMessage = new System.Text.StringBuilder("ServerNetworkManager");
                logMessage.Append(" Incoming");
                logMessage.Append(" ConnectionId:").Append(conn.ConnectionId);
                logMessage.Append(" MsgType:").Append(msgType);
                logMessage.Append(" ChannelId:").Append(channelId);
                logMessage.Append(" Size:").Append(m_NetworkWriter.FilledLength);

                UnityEngine.Debug.Log(logMessage);
#endif

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
                var profilerName = new System.Text.StringBuilder("ServerNetworkManager");
                profilerName.Append(msgType);
                Profiler.NetworkProfiler.RecordMessageIncoming(profilerName.ToString(), msgSize);
#endif

                m_NetworkReader.Lock((ushort)(msgSize - HEADER_BYTE_COUNT));

                if (m_Handlers.TryGetValue(msgType, out handler))
                    handler.Invoke(conn, m_NetworkReader);
                else
                {
                    var warningMessage = new System.Text.StringBuilder("ClinetNetworkManager");
                    warningMessage.Append(" Incoming");
                    warningMessage.Append(" Unknown Incoming Message");
                    warningMessage.Append(" ConnectionId:").Append(conn.ConnectionId);
                    warningMessage.Append(" MsgType:").Append(msgType);
                    warningMessage.Append(" ChannelId:").Append(channelId);
                    warningMessage.Append(" Size:").Append(m_NetworkWriter.FilledLength);

                    UnityEngine.Debug.LogWarning(warningMessage);
                }

                m_NetworkReader.CheckReading();
            }
        }
    }
}

