// See LICENSE file in the root directory
//
namespace LGK.Networking
{
    public interface IClientNetworkManager
    {
        bool IsActive { get; }

        IConnection Connection { get; }

        event ClientEvent.ConnectingDelegate ConnectingEvent;
        event ClientEvent.ConnectingFailedDelegate ConnectingFailedEvent;
        event ClientEvent.ConnectedDelegate ConnectedEvent;
        event ClientEvent.DisconectedDelegate DisconnectedEvent;

        bool Connect(string serverAddress, int serverPort);
        void Disconnect();

        void ProcessMessage();

        void RegisterHandler(ushort msgType, MessageHandlerDelegate handler);
        void RemoveHandler(ushort msgType);

        void SendReliable(ushort msgType, IMessageSerializer serializer);
        void SendReliableSequence(ushort msgType, IMessageSerializer serializer);
        void SendReliableStateUpdate(ushort msgType, IMessageSerializer serializer);
        void SendUnreliable(ushort msgType, IMessageSerializer serializer);
    }
}