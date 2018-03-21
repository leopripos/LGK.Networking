// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public interface IClientPeer
    {
        bool IsActive { get; }

        IConnection Connection { get; }

        event ClientEvent.ConnectingDelegate ConnectingEvent;
        event ClientEvent.ConnectingFailedDelegate ConnectingFailedEvent;
        event ClientEvent.ConnectedDelegate ConnectedEvent;
        event ClientEvent.DisconectedDelegate DisconnectedEvent;
        event ClientEvent.DataDelegate DataEvent;

        bool Connect(string serverAddress, int serverPort);

        byte CreateChannel(ChannelType type);

        bool Send(int channelId, byte[] buffer, int length);

        void ProcessPacket();

        void Disconnect();
    }
}

