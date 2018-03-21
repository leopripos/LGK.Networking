// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public delegate void MessageHandlerDelegate(IConnection conn, NetworkReader reader);

    public static class ClientEvent
    {
        public delegate void ConnectingDelegate();

        public delegate void ConnectingFailedDelegate(NetworkError error);

        public delegate void ConnectedDelegate();

        public delegate void DisconectedDelegate();

        public delegate void DataDelegate(int channelId, byte[] buffer, int length);
    }

    public static class ServerEvent
    {
        public delegate void ListenDelegate();

        public delegate void ShutdownDelegate();

        public delegate void ConnectDelegate(IConnection conn);

        public delegate void DisconectDelegate(IConnection conn);

        public delegate void DataDelegate(IConnection conn, int channelId, byte[] buffer, int length);
    }
}

