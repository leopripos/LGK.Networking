// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public interface IConnection
    {
        int SocketId { get; }

        int ConnectionId { get; }

        bool IsConnected { get; }

        NetworkError LastError { get; }
    }
}

