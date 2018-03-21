// See LICENSE file in the root directory
//

namespace LGK.Networking.Profiler
{
    [System.Serializable]
    public class NetworkTraficBuffer
    {
        public const int MAX_TRAFIC_COUNT = 300;

        [UnityEngine.SerializeField]
        private NetworkTrafic[] m_Item;

        [UnityEngine.SerializeField]
        private int m_BaseIndex;
        [UnityEngine.SerializeField]
        private int m_LastIndex;
        [UnityEngine.SerializeField]
        private int m_Count;

        public NetworkTraficBuffer()
        {
            m_Item = new NetworkTrafic[MAX_TRAFIC_COUNT];
            m_BaseIndex = 0;
            m_LastIndex = MAX_TRAFIC_COUNT - 1;
            m_Count = 0;
        }

        private int ResolveRealIndex(int index)
        {
            var targetIndex = m_BaseIndex + index;
            if (targetIndex >= m_Item.Length)
                targetIndex -= m_Item.Length;

            return targetIndex;
        }

        private int ResolveVirtualIndex(int index)
        {
            var targetIndex = index - m_BaseIndex;
            if (targetIndex < 0)
                targetIndex = (m_Item.Length - targetIndex);

            return targetIndex;
        }

        public NetworkTrafic Add(NetworkTrafic item)
        {
            if (m_Count == m_Item.Length)
            {
                var reuseableItem = m_Item[m_BaseIndex];

                m_BaseIndex++;
                if (m_BaseIndex == m_Item.Length)
                    m_BaseIndex = 0;

                m_LastIndex++;
                if (m_LastIndex == m_Item.Length)
                    m_LastIndex = 0;

                m_Item[m_LastIndex] = item;

                return reuseableItem;
            }
            else
            {
                m_Item[ResolveRealIndex(m_Count)] = item;
                m_Count++;

                return default(NetworkTrafic);
            }
        }

        public NetworkTrafic this[int index]
        {
            get
            {
                index = ResolveRealIndex(index);
                return m_Item[index];
            }
        }

        public void SoftClear()
        {
            if (m_BaseIndex < m_LastIndex)
            {
                for (var index = m_BaseIndex; index <= m_LastIndex; index++)
                    m_Item[index].Clear();

            }
            else
            {
                for (var index = m_BaseIndex; index < m_Count; index++)
                    m_Item[index].Clear();

                for (var index = 0; index <= m_LastIndex; index++)
                    m_Item[index].Clear();
            }

            m_Count = 0;
        }

        public int Count
        {
            get
            {
                return m_Count;
            }
        }
    }
}