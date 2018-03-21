// See LICENSE file in the root directory
//
using UNET = UnityEngine.Networking;

namespace LGK.Networking
{
    public enum ChannelType
    {
        Unreliable = UNET.QosType.Unreliable,
        UnreliableFragmented = UNET.QosType.UnreliableFragmented,
        UnreliableSequenced = UNET.QosType.UnreliableSequenced,
        Reliable = UNET.QosType.Reliable,
        ReliableFragmented = UNET.QosType.ReliableFragmented,
        ReliableSequenced = UNET.QosType.ReliableSequenced,
        StateUpdate = UNET.QosType.StateUpdate,
        ReliableStateUpdate = UNET.QosType.ReliableStateUpdate,
        AllCostDelivery = UNET.QosType.AllCostDelivery,
    }
}

