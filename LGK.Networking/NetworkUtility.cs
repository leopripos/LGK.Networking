// See LICENSE file in the root directory
//
using System;

namespace LGK.Networking
{
    public static class NetworkUtility
    {
        public static bool IsValidIpV6(string address)
        {
            for (int i = 0; i < address.Length; i++)
            {
                var c = address[i];
                if (
                    (c == ':') ||
                    (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F'))
                {
                    continue;
                }
                return false;
            }
            return true;
        }
    }
}

