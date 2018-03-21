// See LICENSE file in the root directory
//

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LGK.Networking
{
    // A growable buffer class used by NetworkReader and NetworkWriter.
    // this is used instead of MemoryStream and BinaryReader/BinaryWriter to avoid allocations.
    class NetworkBuffer
    {
        const int INITIAL_SIZE = 64;
        const float GROWTH_FACTOR = 1.5f;
        const int BUFFER_SIZE_WARNING = 1024 * 1024 * 128;

        byte[] m_Buffer;
        ushort m_Position;
        ushort m_ReadUntilPosition;

        internal byte[] BufferArray
        {
            get { return m_Buffer; }
        }

        internal ushort FilledLength
        {
            get { return m_Position; }
        }

        public ushort Position
        {
            get { return m_Position; }
        }

        public int Length
        {
            get { return m_Buffer.Length; }
        }

        public NetworkBuffer()
        {
            m_Buffer = new byte[INITIAL_SIZE];
        }

        // this does NOT copy the buffer
        public NetworkBuffer(byte[] buffer)
        {
            m_Buffer = buffer;
        }

        public void LockReading(ushort size)
        {
            m_ReadUntilPosition = (ushort)(m_Position + size);

            if (m_ReadUntilPosition >= m_Buffer.Length)
            {
                throw new OperationCanceledException("NetworkBuffer:LockReading out of range:" + ToString());
            }
        }

        public void CheckReading()
        {
            if (m_Position != m_ReadUntilPosition)
            {
                UnityEngine.Debug.LogWarning("NetworkBuffer:CheckReading not read all data provided:" + ToString());

                m_Position = m_ReadUntilPosition;
            }
        }

        public byte ReadByte()
        {
            if (m_Position + 1 > m_ReadUntilPosition)
            {
                throw new OperationCanceledException("NetworkBuffer:ReadByte has more byte to read becaused locked by message locker:" + ToString());
            }

            return m_Buffer[m_Position++];
        }

        public void ReadBytes(byte[] buffer, ushort count)
        {
            if (m_Position + count > m_ReadUntilPosition)
            {
                throw new OperationCanceledException("NetworkBuffer:ReadBytes has more byte to read becaused locked by message locker:" + ToString());
            }

            for (ushort i = 0; i < count; i++)
            {
                buffer[i] = m_Buffer[m_Position + i];
            }
            m_Position += count;
        }

        public void WriteByte(byte value)
        {
            ResizeIfNeeded(1);
            m_Buffer[m_Position] = value;
            m_Position += 1;
        }

        public void WriteByte2(byte value0, byte value1)
        {
            ResizeIfNeeded(2);
            m_Buffer[m_Position] = value0;
            m_Buffer[m_Position + 1] = value1;
            m_Position += 2;
        }

        public void WriteByte4(byte value0, byte value1, byte value2, byte value3)
        {
            ResizeIfNeeded(4);
            m_Buffer[m_Position] = value0;
            m_Buffer[m_Position + 1] = value1;
            m_Buffer[m_Position + 2] = value2;
            m_Buffer[m_Position + 3] = value3;
            m_Position += 4;
        }

        public void WriteByte8(byte value0, byte value1, byte value2, byte value3, byte value4, byte value5, byte value6, byte value7)
        {
            ResizeIfNeeded(8);
            m_Buffer[m_Position] = value0;
            m_Buffer[m_Position + 1] = value1;
            m_Buffer[m_Position + 2] = value2;
            m_Buffer[m_Position + 3] = value3;
            m_Buffer[m_Position + 4] = value4;
            m_Buffer[m_Position + 5] = value5;
            m_Buffer[m_Position + 6] = value6;
            m_Buffer[m_Position + 7] = value7;
            m_Position += 8;
        }

        public void WriteBytes(byte[] buffer, ushort count)
        {
            ResizeIfNeeded(count);

            if (count == buffer.Length)
            {
                buffer.CopyTo(m_Buffer, (int)m_Position);
            }
            else
            {
                //CopyTo doesnt take a count :(
                for (int i = 0; i < count; i++)
                {
                    m_Buffer[m_Position + i] = buffer[i];
                }
            }
            m_Position += count;
        }

        public void ReplaceAt(ushort position, byte value)
        {
            if (position >= m_Buffer.Length)
            {
                throw new IndexOutOfRangeException("NetworkReader:ReplaceAt out of range:" + ToString());
            }
            m_Buffer[position] = value;
        }

        void ResizeIfNeeded(ushort count)
        {
            if (m_Position + count < m_Buffer.Length)
                return;

            int newLen = (int)Math.Ceiling(m_Buffer.Length * GROWTH_FACTOR);
            while (m_Position + count >= newLen)
            {
                newLen = (int)Math.Ceiling(newLen * GROWTH_FACTOR);
                if (newLen > BUFFER_SIZE_WARNING)
                {
                    Debug.LogWarning("NetworkBuffer size is " + newLen + " bytes!");
                }
            }

            // only do the copy once, even if newLen is increased multiple times
            byte[] tmp = new byte[newLen];
            m_Buffer.CopyTo(tmp, 0);
            m_Buffer = tmp;
        }

        public void SeekZero()
        {
            m_Position = 0;
            m_ReadUntilPosition = 0;
        }

        public void BackTo(ushort position)
        {
            m_Position = position;
        }

        public override string ToString()
        {
            return String.Format("NetBuf sz:{0} pos:{1} readlock:{2}", m_Buffer.Length, m_Position, m_ReadUntilPosition);
        }
    }
    // end NetBuffer

    // -- helpers for float conversion --
    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntFloat
    {
        [FieldOffset(0)]
        public float floatValue;

        [FieldOffset(0)]
        public uint intValue;

        [FieldOffset(0)]
        public double doubleValue;

        [FieldOffset(0)]
        public ulong longValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntDecimal
    {
        [FieldOffset(0)]
        public ulong longValue1;

        [FieldOffset(8)]
        public ulong longValue2;

        [FieldOffset(0)]
        public decimal decimalValue;
    }

    internal class FloatConversion
    {
        public static float ToSingle(uint value)
        {
            UIntFloat uf = new UIntFloat();
            uf.intValue = value;
            return uf.floatValue;
        }

        public static double ToDouble(ulong value)
        {
            UIntFloat uf = new UIntFloat();
            uf.longValue = value;
            return uf.doubleValue;
        }

        public static decimal ToDecimal(ulong value1, ulong value2)
        {
            UIntDecimal uf = new UIntDecimal();
            uf.longValue1 = value1;
            uf.longValue2 = value2;
            return uf.decimalValue;
        }
    }
}

