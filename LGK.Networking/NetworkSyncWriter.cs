// See LICENSE file in the root directory
// 

namespace LGK.Networking
{
    public class NetworkSyncWriter
    {
        readonly NetworkWriter m_NetworkWriter;

        ushort m_ComponentDirtyFlagPosition;

        public NetworkSyncWriter(NetworkWriter networkWriter)
        {
            m_NetworkWriter = networkWriter;
            m_ComponentDirtyFlagPosition = 0;
        }

        public void Write(char value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(byte value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(sbyte value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(short value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(ushort value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(int value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(uint value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(long value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(ulong value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(float value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(double value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(decimal value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(string value)
        {
            m_NetworkWriter.Write(value);
        }

        public void Write(bool value)
        {
            m_NetworkWriter.Write(value);
        }

        public void StartSyncMessage(int objectId)
        {
            m_ComponentDirtyFlagPosition = m_NetworkWriter.Position;

            // 1 bytes for size, will be filled out in FinishSyncMessage
            // 1 byte is size of dirty flag
            m_NetworkWriter.Write((byte)0);

            Write(objectId);
        }

        public void FinishSyncMessage(byte componentDirtyFlag)
        {
            if (componentDirtyFlag == 0)
                m_NetworkWriter.BackTo(m_ComponentDirtyFlagPosition);
            else
                m_NetworkWriter.RepaceAt(m_ComponentDirtyFlagPosition, componentDirtyFlag);
        }
    };
}

