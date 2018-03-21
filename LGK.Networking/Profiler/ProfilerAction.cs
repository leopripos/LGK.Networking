// See LICENSE file in the root directory
//


namespace LGK.Networking.Profiler
{
    public static class ProfilerConfig
    {
        public static bool IsActive { get; internal set; }
    }

    public static class TraficDatabase
    {
        internal static NetworkTraficBuffer IncomingBuffer { get; set; }
        internal static NetworkTraficBuffer OutgoinBuffer { get; set; }

        internal static NetworkTrafic CurrentIncoming = new NetworkTrafic();
        internal static NetworkTrafic CurrentOutgoing = new NetworkTrafic();
    }

    public static class ProfilerAction
    {
        public static void SetupDatabase(NetworkTraficBuffer incomingBuffer, NetworkTraficBuffer outgoingBuffer)
        {
            TraficDatabase.IncomingBuffer = incomingBuffer;
            TraficDatabase.OutgoinBuffer = outgoingBuffer;
        }

        public static void Enable()
        {
            ProfilerConfig.IsActive = true;
        }

        public static void Disable()
        {
            ProfilerConfig.IsActive = false;
        }

        public static void Capture()
        {
            var reuseableIncoming = TraficDatabase.IncomingBuffer.Add(TraficDatabase.CurrentIncoming);
            TraficDatabase.CurrentIncoming = (reuseableIncoming == null ? new NetworkTrafic() : reuseableIncoming);
            TraficDatabase.CurrentIncoming.Clear();

            var reuseableOutgoing = TraficDatabase.OutgoinBuffer.Add(TraficDatabase.CurrentOutgoing);
            TraficDatabase.CurrentOutgoing = (reuseableOutgoing == null ? new NetworkTrafic() : reuseableOutgoing);
            TraficDatabase.CurrentOutgoing.Clear();
        }

        public static void Clear()
        {
            TraficDatabase.IncomingBuffer.SoftClear();
            TraficDatabase.OutgoinBuffer.SoftClear();

            TraficDatabase.CurrentIncoming.Clear();
            TraficDatabase.CurrentOutgoing.Clear();
        }

        public static void ClearCurrent()
        {
            TraficDatabase.CurrentIncoming.Clear();
            TraficDatabase.CurrentOutgoing.Clear();
        }
    }
}