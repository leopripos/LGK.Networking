// See LICENSE file in the root directory
//

using System.Collections.Generic;

namespace LGK.Networking
{
    public interface IServerNetworkManager
    {
        bool IsActive { get; }
        int Port { get; }

        event ServerEvent.ListenDelegate ListenEvent;
        event ServerEvent.ShutdownDelegate ShutdownEvent;
        event ServerEvent.ConnectDelegate ConnectedEvent;
        event ServerEvent.DisconectDelegate DisconnectedEvent;

        bool Listen(int port);
        bool Listen(string address, int port);

        void RegisterHandler(ushort msgType, MessageHandlerDelegate handler);
        void RemoveHandler(ushort msgType);

        void ProcessMessage();

        void SendSyncObject(int connId, ushort msgType, IObjectSerializer serializer);
        void SendSyncObject(IList<int> connIds, ushort msgType, IObjectSerializer serializer);

        void SendReliable(int connId, ushort msgType, IMessageSerializer serializer);
        void SendReliableSequence(int connId, ushort msgType, IMessageSerializer serializer);
        void SendReliableStateUpdate(int connId, ushort msgType, IMessageSerializer serializer);
        void SendUnreliable(int connId, ushort msgType, IMessageSerializer serializer);

        void SendReliable(IList<int> connIds, ushort msgType, IMessageSerializer serializer);
        void SendReliableSequence(IList<int> connIds, ushort msgType, IMessageSerializer serializer);
        void SendReliableStateUpdate(IList<int> connIds, ushort msgType, IMessageSerializer serializer);
        void SendUnreliable(IList<int> connIds, ushort msgType, IMessageSerializer serializer);

        void Disconnect(int connId);
        void Disconnect(IConnection conn);
        void DisconnectAll();

        void Shutdown();
    }
}