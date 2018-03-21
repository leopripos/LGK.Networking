// See LICENSE file in the root directory
//

using UNET = UnityEngine.Networking;

namespace LGK.Networking.LLAPI
{
    public static class NetworkTransport
    {
        private static bool m_Initialized;
        private static byte m_ContextCounter;

        public const sbyte INVALID_SOCKET = -1;
        public const sbyte INVALID_CONNECTION = 0;

        public static void InitContext(object owner)
        {
            if (!m_Initialized)
            {
                UNET.NetworkTransport.Init();
                m_Initialized = true;
            }

            m_ContextCounter++;
        }

        public static void DestroyContext(object owner)
        {
            m_ContextCounter--;

            if (m_ContextCounter == 0 && m_Initialized)
            {
                UNET.NetworkTransport.Shutdown();
            }
        }

    }
}

