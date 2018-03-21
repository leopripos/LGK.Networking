// See LICENSE file in the root directory
//

namespace LGK.Networking.Profiler
{
    public static class NetworkProfiler
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RecordMessageIncoming(string category, ushort size)
        {
            if (ProfilerConfig.IsActive)
                TraficDatabase.CurrentIncoming.AddMessageTrafic(category, size);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RecordChannelIncoming(string category, ushort size)
        {
            if (ProfilerConfig.IsActive)
                TraficDatabase.CurrentIncoming.AddChannelTrafic(category, size);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RecordChannelOutgoing(string category, ushort size)
        {
            if (ProfilerConfig.IsActive)
                TraficDatabase.CurrentOutgoing.AddChannelTrafic(category, size);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RecordMessageOutgoing(string category, ushort size)
        {
            if (ProfilerConfig.IsActive)
                TraficDatabase.CurrentOutgoing.AddMessageTrafic(category, size);
        }
    }
}

