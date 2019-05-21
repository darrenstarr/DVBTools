using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TransportMux
{
    /// <summary>
    /// Provides a relatively good performance big-endian buffered file reading class.
    /// </summary>
    /// This class provides decent performance for reading relatively large linear files
    /// by using a multi-megabyte buffer for forward storing.
    /// <todo>
    /// Optimize the peek32() function so that if a user peeks across a buffer boundary,
    /// the system will not perform too much buffer swaps to compensate for it.
    /// </todo>
    public class BigEndianReaderBackEnd : BinaryReader
    {
        const int InternalBufferLength = 2 * 1024 * 1024;

        byte[] buffer = new byte[InternalBufferLength];
        long bufferLength = 0;
        long bufferStarts = 0;
        long bufferEnds;

        long position = 0;

        /// <summary>
        /// The length of the stream.
        /// </summary>
        /// If the size of the stream varies, meaning another source is feeding the stream, this object is not
        /// updated to reflect the change. 
        public long Length = 0;        

        /// <summary>
        /// Either returns the current position within the stream or sets it.
        /// </summary>
        public long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                if (value >= bufferStarts && value <= (bufferEnds))
                    return;

                ((FileStream)base.BaseStream).Position = value;
                bufferLength = Read(buffer, 0, buffer.Length);
                bufferStarts = value;
                bufferEnds = bufferStarts + bufferLength - 1;
            }
        }

        /// <summary>
        /// Returns the number of bytes that are remaining before the end of the file stream
        /// </summary>
        public long Remaining
        {
            get
            {
                return Length - position;
            }
        }

        protected BigEndianReaderBackEnd(FileStream stream) : base(stream)
        {
            Length = stream.Length;
            bufferStarts = stream.Position;
            bufferLength = Read(buffer, 0, buffer.Length);
            bufferEnds = bufferStarts + bufferLength - 1;
        }

        void bufferMore()
        {
            bufferStarts = position;
            bufferLength = Read(buffer, 0, buffer.Length);
            bufferEnds = bufferStarts + bufferLength - 1;
        }

        public override byte ReadByte()
        {
            byte result = buffer[position - bufferStarts];
            position++;
            if (position > bufferEnds)
                bufferMore();
            return result;
        }

        /// <summary>
        /// Returns the next two bytes as a single big endian integer
        /// </summary>
        /// <returns>A 16-bit big endian integer</returns>
        public override ushort ReadUInt16()
        {
            ushort a = (ushort) (((ushort)ReadByte()) << 8);
            ushort b = ((ushort)ReadByte());
            return (ushort) (a | b);
        }

        /// <summary>
        /// Returns the next three bytes as a single big endian integer
        /// </summary>
        /// <returns>A 24-bit big endian integer</returns>
        public uint ReadUInt24()
        {
            uint a = (uint)(((uint)ReadByte()) << 16);
            uint b = (uint)(((uint)ReadByte()) << 8);
            uint c = ((uint)ReadByte());
            return (uint)(a | b | c);
        }

        /// <summary>
        /// returns the next 4 bytes as a single big endian integer
        /// </summary>
        /// <returns>A 32-bit big endian integer</returns>
        public override uint ReadUInt32()
        {
            uint a = (uint)(((uint)ReadByte()) << 24);
            uint b = (uint)(((uint)ReadByte()) << 16);
            uint c = (uint)(((uint)ReadByte()) << 8);
            uint d = ((uint)ReadByte());
            return (uint)(a | b | c | d);
        }

        /// <summary>
        /// Returns the next 8 bytes as a single big endian integer
        /// </summary>
        /// <returns>A 64-bute big endian integer</returns>
        public override ulong ReadUInt64()
        {
            ulong a = (ulong)(((ulong)ReadByte()) << 56);
            ulong b = (ulong)(((ulong)ReadByte()) << 48);
            ulong c = (ulong)(((ulong)ReadByte()) << 40);
            ulong d = (ulong)(((ulong)ReadByte()) << 32);
            ulong e = (ulong)(((ulong)ReadByte()) << 24);
            ulong f = (ulong)(((ulong)ReadByte()) << 16);
            ulong g = (ulong)(((ulong)ReadByte()) << 8);
            ulong h = ((ulong)ReadByte());
            return (ulong)(a | b | c | d | e | f | g | h);
        }

        /// <summary>
        /// Returns the next 2 bytes of the stream as a single signed big-endian integer
        /// </summary>
        /// <returns>A 16-bit signed integer</returns>
        public override short ReadInt16()
        {
            short a = (short)(((short)ReadByte()) << 8);
            short b = ((short)ReadByte());
            return (short)(a | b);
        }

        /// <summary>
        /// Returns the next 4 bytes of the stream as a single signed big-endian integer
        /// </summary>
        /// <returns>A 32-bute signed big endian integer</returns>
        public override int ReadInt32()
        {
            int a = (int)(((int)ReadByte()) << 24);
            int b = (int)(((int)ReadByte()) << 16);
            int c = (int)(((int)ReadByte()) << 8);
            int d = ((int)ReadByte());
            return (int)(a | b | c | d);
        }

        /// <summary>
        /// Returns the next 8 bytes of the stream as a single signed big-endian integer
        /// </summary>
        /// <returns>A 64-bit signed big endian integer</returns>
        public override long ReadInt64()
        {
            long a = (long)(((long)ReadByte()) << 56);
            long b = (long)(((long)ReadByte()) << 48);
            long c = (long)(((long)ReadByte()) << 40);
            long d = (long)(((long)ReadByte()) << 32);
            long e = (long)(((long)ReadByte()) << 24);
            long f = (long)(((long)ReadByte()) << 16);
            long g = (long)(((long)ReadByte()) << 8);
            long h = ((long)ReadByte());
            return (long)(a | b | c | d | e | f | g | h);
        }

        /// <summary>
        /// Returns the next 4 bytes as a big endian integer, but does not advance the file position
        /// </summary>
        /// <returns>A 32-bit unsigned integer</returns>
        public uint Peek32()
        {
            long current = position;
            uint result = ReadUInt32();
            Position = current;
            return result;
        }
    }

    public class BigEndianReader : BigEndianReaderBackEnd
    {
        long LengthRead = -1;

        long FileLength = 0;

        public BigEndianReader(FileStream stream)
            : base(stream)
        {
            FileLength = stream.Length;
        }

        public void SeekRelative(long delta)
        {
            Position += delta;
        }

        public bool AtEnd
        {
            get
            {
                if (LengthRead < 0)
                    LengthRead = ((FileStream)(BaseStream)).Length;

                return (Position >= LengthRead) ? true : false;
            }
        }

        public string ReadFixedLengthString(int length)
        {
            string result = "";

            for (int i = 0; i < length; i++)
                result += (char)ReadByte();

            return result;
        }

        public char readUTF8Char()
        {
            byte current = ReadByte();
            char result = (char)0;

            if ((current & 0x80) == 0)
                return (char)current;

            if ((current & 0xE0) == 0xC0)
            {
                result = (char)(current & 0x1F);
                result <<= 6;
                current = ReadByte();
                if ((current & 0xC0) != 0x80)
                    throw new Exception("Invalid UTF-8 sequence");
                result += (char)(current & 0x3F);
                return result;
            }

            if ((current & 0xF0) == 0xE0)
            {
                result = (char)(current & 0x0F);
                for (int i = 0; i < 2; i++)
                {
                    result <<= 6;
                    current = ReadByte();
                    if ((current & 0xC0) != 0x80)
                        throw new Exception("Invalid UTF-8 sequence");
                    result += (char)(current & 0x3F);
                }
                return result;
            }

            if ((current & 0xF8) == 0xF0)
            {
                result = (char)(current & 0x07);
                for (int i = 0; i < 3; i++)
                {
                    result <<= 6;
                    current = ReadByte();
                    if ((current & 0xC0) != 0x80)
                        throw new Exception("Invalid UTF-8 sequence");
                    result += (char)(current & 0x3F);
                }
                return result;
            }

            if ((current & 0xFC) == 0xF8)
            {
                result = (char)(current & 0x03);
                for (int i = 0; i < 4; i++)
                {
                    result <<= 6;
                    current = ReadByte();
                    if ((current & 0xC0) != 0x80)
                        throw new Exception("Invalid UTF-8 sequence");
                    result += (char)(current & 0x3F);
                }
                return result;
            }

            if ((current & 0xFE) == 0xFC)
            {
                result = (char)(current & 0x01);
                for (int i = 0; i < 4; i++)
                {
                    result <<= 6;
                    current = ReadByte();
                    if ((current & 0xC0) != 0x80)
                        throw new Exception("Invalid UTF-8 sequence");
                    result += (char)(current & 0x3F);
                }
                return result;
            }

            throw new Exception("Invalid UTF-8 sequence");
        }

        public string readNullTerminatedUTF8()
        {
            string result = "";

            char ch = readUTF8Char();
            while (ch != (char)0)
            {
                result += ch;
                ch = readUTF8Char();
            }

            return result;
        }

        public string readFourCC()
        {
            string result = "";
            for (int i = 0; i < 4; i++)
                result += (Char)ReadByte();
            return result;
        }

        public string ReadFourCC()
        {
            string result = "";
            for (int i = 0; i < 4; i++)
                result += (Char)ReadByte();
            return result;
        }
    }
}
