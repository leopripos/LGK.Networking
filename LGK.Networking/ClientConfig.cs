// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public class ClientConfig
    {
        public ushort ConnectTimeout = 1000;
        public byte MaxConnectingTry = 3;
        public ushort DisconnectTimeout = 2000;

        public ushort BufferSize = 1024;
    }
}

