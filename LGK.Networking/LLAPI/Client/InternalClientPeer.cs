// See LICENSE file in the root directory
//
namespace LGK.Networking.LLAPI.Client
{
    internal interface IInternalClientPeer : IClientPeer
    {
        byte[] RecievedBuffer { get; }
    }
}

