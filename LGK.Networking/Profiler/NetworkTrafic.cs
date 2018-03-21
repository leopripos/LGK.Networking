// See LICENSE file in the root directory
//

namespace LGK.Networking.Profiler
{
    [System.Serializable]
    public class TraficInfo
    {
        public string Category;
        public ushort Total;
        public ushort Count;
        public ushort Min;
        public ushort Max;
        public ushort Avg;
    }

    [System.Serializable]
    public class TraficInfoDictionary : SerializableDictionary<string, TraficInfo>
    {
    }


    [System.Serializable]
    public class NetworkTrafic
    {
        [UnityEngine.SerializeField]
        private ushort m_TotalMessageTrafic;
        [UnityEngine.SerializeField]
        private ushort m_TotalChannelTrafic;

        [UnityEngine.SerializeField]
        private TraficInfoDictionary m_MessageTrafics = new TraficInfoDictionary();
        [UnityEngine.SerializeField]
        private TraficInfoDictionary m_ChannelTrafics = new TraficInfoDictionary();

        public ushort TotalMessageTrafic
        {
            get { return m_TotalMessageTrafic; }
        }

        public ushort TotalChannelTrafic
        {
            get { return m_TotalChannelTrafic; }
        }

        public SerializableDictionary<string, TraficInfo> MessageTrafics
        {
            get { return m_MessageTrafics; }
        }

        public SerializableDictionary<string, TraficInfo> ChannelTrafics
        {
            get { return m_ChannelTrafics; }
        }

        public void AddChannelTrafic(string category, ushort size)
        {
            TraficInfo info;

            m_TotalChannelTrafic += size;

            if (m_ChannelTrafics.TryGetValue(category, out info))
            {
                info.Total += size;
                info.Count++;
                info.Min = (size < info.Min ? size : info.Min);
                info.Max = (size > info.Max ? size : info.Max);
                info.Avg = (ushort)(info.Total / info.Count);
            }
            else
            {
                info = new TraficInfo();
                info.Category = category;
                info.Total = size;
                info.Count = 1;
                info.Min = size;
                info.Max = size;
                info.Avg = size;
                m_ChannelTrafics.Add(category, info);
            }
        }
        public void AddMessageTrafic(string category, ushort size)
        {
            TraficInfo info;

            m_TotalMessageTrafic += size;

            if (m_MessageTrafics.TryGetValue(category, out info))
            {
                info.Total += size;
                info.Count++;
                info.Min = (size < info.Min ? size : info.Min);
                info.Max = (size > info.Max ? size : info.Max);
                info.Avg = (ushort)(info.Total / info.Count);
            }
            else
            {
                info = new TraficInfo();
                info.Category = category;
                info.Total = size;
                info.Count = 1;
                info.Min = size;
                info.Max = size;
                info.Avg = size;
                m_MessageTrafics.Add(category, info);
            }
        }

        public void Clear()
        {
            m_TotalChannelTrafic = 0;
            m_TotalMessageTrafic = 0;

            m_MessageTrafics.Clear();
            m_ChannelTrafics.Clear();
        }
    }
}