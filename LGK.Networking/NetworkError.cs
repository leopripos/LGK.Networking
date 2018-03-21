// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public enum NetworkError
    {
        None,
        WrongHost,
        WrongConnection,
        WrongChannel,
        NoResources,
        BadMessage,
        Timeout,
        MessageToLong,
        WrongOperation,
        VersionMismatch,
        CRCMismatch,
        DNSFailure,
        UsageError
    }
}

