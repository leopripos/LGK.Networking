// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public interface IServerPeer
    {
        bool IsActive { get; }

        int Port { get; }

        event ServerEvent.ListenDelegate ListenEvent;
        event ServerEvent.ShutdownDelegate ShutdownEvent;
        event ServerEvent.ConnectDelegate ConnectedEvent;
        event ServerEvent.DisconectDelegate DisconnectedEvent;
        event ServerEvent.DataDelegate DataEvent;

        bool Listen(int port);

        bool Listen(string address, int port);

        byte CreateChannel(ChannelType type);

        void ProcessPacket();

        bool Send(int connId, int channelId, byte[] buffer, int length);

        bool Send(IConnection conn, int channelId, byte[] buffer, int length);

        void Disconnect(int connId);

        void Disconnect(IConnection conn);

        void DisconnectAll();

        void Shutdown();
    }
}

