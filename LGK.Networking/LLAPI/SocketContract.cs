// See LICENSE file in the root directory
//
using UNET = UnityEngine.Networking;

namespace LGK.Networking.LLAPI
{
    public class SocketContract : UNET.HostTopology
    {
        public SocketContract(ConnectionConfig defaultConfig, int maxDefaultConnections)
            : base(defaultConfig, maxDefaultConnections)
        {
            
        }
    }
}

