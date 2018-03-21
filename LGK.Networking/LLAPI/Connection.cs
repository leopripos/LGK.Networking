// See LICENSE file in the root directory
//

namespace LGK.Networking.LLAPI
{
    public class Connection : IConnection
    {
        internal int SocketId;
        internal int ConnectionId;
        internal bool IsConnected;

        internal NetworkError LastError;

        int IConnection.SocketId
        {
            get { return this.SocketId; }
        }

        int IConnection.ConnectionId
        {
            get { return this.ConnectionId; }
        }

        bool IConnection.IsConnected
        {
            get { return this.IsConnected; }
        }

        NetworkError IConnection.LastError
        {
            get { return LastError; }
        }

        internal Connection(int socketId, int connectionId, bool isConnected, NetworkError lastError)
        {
            SocketId = socketId;
            ConnectionId = connectionId;
            IsConnected = isConnected;
            LastError = lastError;
        }
    }
}

