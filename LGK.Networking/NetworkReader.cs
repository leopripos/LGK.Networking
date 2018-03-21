// See LICENSE file in the root directory
//

using System;
using System.Text;

namespace LGK.Networking
{
    public class NetworkReader
    {
        const int MAX_STRING_LEGNTH = 1024 * 32;
        const int INITIAL_STRING_BUFFER_SIZE = 1024;

        static byte[] s_StringReaderBuffer;
        static Encoding s_Encoding;

        readonly NetworkBuffer m_Buffer;

        public NetworkReader()
        {
            m_Buffer = new NetworkBuffer();
            Initialize();
        }

        public NetworkReader(byte[] buffer)
        {
            m_Buffer = new NetworkBuffer(buffer);
            Initialize();
        }

        static void Initialize()
        {
            if (s_Encoding == null)
            {
                s_StringReaderBuffer = new byte[INITIAL_STRING_BUFFER_SIZE];
                s_Encoding = new UTF8Encoding();
            }
        }

        internal void SeekZero()
        {
            m_Buffer.SeekZero();
        }

        internal void Lock(ushort size)
        {
            m_Buffer.LockReading(size);
        }

        internal void CheckReading()
        {
            m_Buffer.CheckReading();
        }

        public uint Position
        {
            get { return m_Buffer.Position; }
        }

        public int Length
        {
            get { return m_Buffer.Length; }
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki
        // NOTE: big endian.

        public UInt32 ReadPackedUInt32()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return (UInt32)(240 + 256 * (a0 - 241) + a1);
            }
            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return (UInt32)(2288 + 256 * a1 + a2);
            }
            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16);
            }
            byte a4 = ReadByte();
            if (a0 >= 251)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16) + (((UInt32)a4) << 24);
            }
            throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + a0);
        }

        public UInt64 ReadPackedUInt64()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + 256 * (a0 - ((UInt64)241)) + a1;
            }
            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return 2288 + (((UInt64)256) * a1) + a2;
            }
            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16);
            }
            byte a4 = ReadByte();
            if (a0 == 251)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24);
            }


            byte a5 = ReadByte();
            if (a0 == 252)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
            }


            byte a6 = ReadByte();
            if (a0 == 253)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
            }


            byte a7 = ReadByte();
            if (a0 == 254)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
            }


            byte a8 = ReadByte();
            if (a0 == 255)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48) + (((UInt64)a8) << 56);
            }

            throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);
        }

        public byte ReadByte()
        {
            return m_Buffer.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return (sbyte)m_Buffer.ReadByte();
        }

        public short ReadInt16()
        {
            ushort value = 0;
            value |= m_Buffer.ReadByte();
            value |= (ushort)(m_Buffer.ReadByte() << 8);
            return (short)value;
        }

        public ushort ReadUInt16()
        {
            ushort value = 0;
            value |= m_Buffer.ReadByte();
            value |= (ushort)(m_Buffer.ReadByte() << 8);
            return value;
        }

        public int ReadInt32()
        {
            uint value = 0;
            value |= m_Buffer.ReadByte();
            value |= (uint)(m_Buffer.ReadByte() << 8);
            value |= (uint)(m_Buffer.ReadByte() << 16);
            value |= (uint)(m_Buffer.ReadByte() << 24);
            return (int)value;
        }

        public uint ReadUInt32()
        {
            uint value = 0;
            value |= m_Buffer.ReadByte();
            value |= (uint)(m_Buffer.ReadByte() << 8);
            value |= (uint)(m_Buffer.ReadByte() << 16);
            value |= (uint)(m_Buffer.ReadByte() << 24);
            return value;
        }

        public long ReadInt64()
        {
            ulong value = 0;

            ulong other = m_Buffer.ReadByte();
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 8;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 16;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 24;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 32;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 40;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 48;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 56;
            value |= other;

            return (long)value;
        }

        public ulong ReadUInt64()
        {
            ulong value = 0;
            ulong other = m_Buffer.ReadByte();
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 8;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 16;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 24;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 32;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 40;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 48;
            value |= other;

            other = ((ulong)m_Buffer.ReadByte()) << 56;
            value |= other;
            return value;
        }

        public decimal ReadDecimal()
        {
            Int32[] bits = new Int32[4];

            bits[0] = ReadInt32();
            bits[1] = ReadInt32();
            bits[2] = ReadInt32();
            bits[3] = ReadInt32();

            return new decimal(bits);
        }

        public float ReadSingle()
        {
            uint value = ReadUInt32();
            return FloatConversion.ToSingle(value);
        }

        public double ReadDouble()
        {
            ulong value = ReadUInt64();
            return FloatConversion.ToDouble(value);
        }

        public string ReadString()
        {
            UInt16 numBytes = ReadUInt16();
            if (numBytes == 0)
                return "";

            if (numBytes >= MAX_STRING_LEGNTH)
            {
                throw new IndexOutOfRangeException("ReadString() too long: " + numBytes);
            }

            while (numBytes > s_StringReaderBuffer.Length)
            {
                s_StringReaderBuffer = new byte[s_StringReaderBuffer.Length * 2];
            }

            m_Buffer.ReadBytes(s_StringReaderBuffer, numBytes);

            char[] chars = s_Encoding.GetChars(s_StringReaderBuffer, 0, numBytes);
            return new string(chars);
        }

        public char ReadChar()
        {
            return (char)m_Buffer.ReadByte();
        }

        public bool ReadBoolean()
        {
            int value = m_Buffer.ReadByte();
            return value == 1;
        }

        public override string ToString()
        {
            return m_Buffer.ToString();
        }
    };
}

