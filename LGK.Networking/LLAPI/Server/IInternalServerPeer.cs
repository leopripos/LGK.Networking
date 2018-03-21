// See LICENSE file in the root directory
//

namespace LGK.Networking.LLAPI.Server
{
    internal interface IInternalServerPeer : IServerPeer
    {
        byte[] RecievedBuffer { get; }
    }
}

