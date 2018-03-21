// See LICENSE file in the root directory
//

using System.Collections.Generic;

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
using System.Text;
using LGK.Networking.Profiler;
#endif

namespace LGK.Networking.LLAPI.Client
{
    public class ClientNetworkManager : IClientNetworkManager
    {
#if (NETWORK_PROFILER_ENABLED || NETWORK_DEBUGGER_ENABLED) && UNITY_EDITOR
        const string DEBUGGING_NAME = "ClientManager";
#endif
        const byte HEADER_BYTE_COUNT = 4;

        readonly IInternalClientPeer m_ClientPeer;

        private readonly NetworkWriter m_NetworkWriter;
        private readonly NetworkReader m_NetworkReader;

        private readonly byte m_ReliableChannel;
        private readonly byte m_ReliableSequenceChannel;
        private readonly byte m_ReliableStateUpdateChannel;
        private readonly byte m_UnrealiableChannel;

        public event ClientEvent.ConnectingDelegate ConnectingEvent;
        public event ClientEvent.ConnectingFailedDelegate ConnectingFailedEvent;
        public event ClientEvent.ConnectedDelegate ConnectedEvent;
        public event ClientEvent.DisconectedDelegate DisconnectedEvent;

        public Dictionary<ushort, MessageHandlerDelegate> m_Handlers;

        public ClientNetworkManager(ClientConfig config)
        {
            m_Handlers = new Dictionary<ushort, MessageHandlerDelegate>();
            m_ClientPeer = new ClientPeer(config);

            m_ReliableChannel = m_ClientPeer.CreateChannel(ChannelType.Reliable);
            m_ReliableSequenceChannel = m_ClientPeer.CreateChannel(ChannelType.ReliableSequenced);
            m_ReliableStateUpdateChannel = m_ClientPeer.CreateChannel(ChannelType.ReliableStateUpdate);
            m_UnrealiableChannel = m_ClientPeer.CreateChannel(ChannelType.Unreliable);

            m_NetworkWriter = new NetworkWriter(new byte[config.BufferSize]);
            m_NetworkReader = new NetworkReader(m_ClientPeer.RecievedBuffer);

            m_ClientPeer.ConnectingEvent += HandleConnectingEvent;
            m_ClientPeer.ConnectingFailedEvent += HandleConnectingFailedEvent;
            m_ClientPeer.ConnectedEvent += HandleConnectedEvent;
            m_ClientPeer.DisconnectedEvent += HandleDisconnectedEvent;
            m_ClientPeer.DataEvent += HandleDataEvent;
        }

        public bool IsActive { get { return m_ClientPeer.IsActive; } }

        public IConnection Connection { get { return m_ClientPeer.Connection; } }

        public bool Connect(string serverAddress, int serverPort)
        {
            NetworkTransport.InitContext(this);

            return m_ClientPeer.Connect(serverAddress, serverPort);
        }

        public void ProcessMessage()
        {
            m_ClientPeer.ProcessPacket();
        }

        public void SendReliable(ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(msgType, m_ReliableChannel);

            SendBuffer(m_ReliableChannel);

            ProfilerOutgoingMessage(msgType);
        }

        public void SendReliableSequence(ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(msgType, m_ReliableSequenceChannel);

            SendBuffer(m_ReliableSequenceChannel);

            ProfilerOutgoingMessage(msgType);
        }

        public void SendReliableStateUpdate(ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(msgType, m_ReliableStateUpdateChannel);

            SendBuffer(m_ReliableStateUpdateChannel);

            ProfilerOutgoingMessage(msgType);
        }

        public void SendUnreliable(ushort msgType, IMessageSerializer serializer)
        {
            SerializeMessageToBuffer(msgType, serializer);

            DebugOutgoingMessage(msgType, m_UnrealiableChannel);

            SendBuffer(m_UnrealiableChannel);

            ProfilerOutgoingMessage(msgType);
        }

        void SerializeMessageToBuffer(ushort msgType, IMessageSerializer serializer)
        {
            m_NetworkWriter.SeekZero();
            m_NetworkWriter.StartMessage(msgType);
            serializer.Serialize(m_NetworkWriter);
            m_NetworkWriter.FinishMessage();
        }

        void SendBuffer(byte channelType)
        {
            m_ClientPeer.Send(channelType, m_NetworkWriter.BufferArray, m_NetworkWriter.FilledLength);
        }

        void DebugOutgoingMessage(ushort msgType, byte channelType)
        {
#if NETWORK_DEBUGGER_ENABLED
            UnityEngine.Debug.Log($"{DEBUGGING_NAME} Outgoing : ConnectionId:{m_ClientPeer.Connection.ConnectionId} MsgType:{msgType}, ChannelId:{channelType} Size:{m_NetworkWriter.FilledLength}");
#endif
        }

        void ProfilerOutgoingMessage(ushort msgType)
        {
#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
            var profilerName = new StringBuilder(DEBUGGING_NAME);
            profilerName.Append(msgType);
            NetworkProfiler.RecordMessageOutgoing(profilerName.ToString(), m_NetworkWriter.FilledLength);
#endif
        }

        public void Disconnect()
        {
            m_ClientPeer.Disconnect();

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

        #region Network Event Handler

        void HandleConnectingEvent()
        {
            ConnectingEvent?.Invoke();
        }

        void HandleConnectingFailedEvent(NetworkError error)
        {
            ConnectingFailedEvent?.Invoke(error);
        }

        void HandleConnectedEvent()
        {
            ConnectedEvent?.Invoke();
        }

        void HandleDisconnectedEvent()
        {
            DisconnectedEvent?.Invoke();
        }

        void HandleDataEvent(int channelId, byte[] buffer, int length)
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
                UnityEngine.Debug.Log($"{DEBUGGING_NAME} Incoming : MsgType:{msgType}, ChannelId:{channelId} Size:{msgSize}");
#endif

#if NETWORK_PROFILER_ENABLED && UNITY_EDITOR
                var profilerName = new StringBuilder(DEBUGGING_NAME);
                profilerName.Append(msgType);
                NetworkProfiler.RecordMessageIncoming(profilerName.ToString(), msgSize);
#endif

                m_NetworkReader.Lock((ushort)(msgSize - HEADER_BYTE_COUNT));

                if (m_Handlers.TryGetValue(msgType, out handler))
                    handler.Invoke(m_ClientPeer.Connection, m_NetworkReader);
                else
                    UnityEngine.Debug.LogWarning($"Unknown message ID {msgType} from channel:{channelId}");

                m_NetworkReader.CheckReading();
            }
        }

        #endregion
    }
}

