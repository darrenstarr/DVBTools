using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon
{
    public class ByteArray
    {
        public byte[] buffer = null;
        public int length = 0;

        public long MaxSize = -1;
        public long GrowSize = 65536;

        byte BitModeBuffer;
	    int CurrentBit;
	    bool BitMode;

        public byte this[long index]
        {
            get
            {
                return buffer[index];
            }
            set
            {
                buffer[index] = value;
            }
        }

        public byte this[ushort index]
        {
            get
            {
                return buffer[index];
            }
            set
            {
                buffer[index] = value;
            }
        }

        void grow()
        {
            if (MaxSize > 0)
            {
                if (buffer == null && MaxSize < GrowSize)
                {
                    buffer = new byte[MaxSize];
                    return;
                }

                if (length < MaxSize)
                    return;

                if (buffer != null && MaxSize <= buffer.Length)
                    throw new Exception("Can't grow past maxsize");

                if (buffer != null && (buffer.Length + GrowSize) > MaxSize)
                {
                    byte[] newBuffer = new byte[MaxSize];
                    for (int i = 0; i < length; i++)
                        newBuffer[i] = buffer[i];
                    buffer = newBuffer;
                    return;
                }
            }

            if (buffer == null)
                buffer = new byte[GrowSize];
            else if ((length + 4) >= buffer.Length)
            {
                byte[] newBuffer = new byte[buffer.Length + GrowSize];
                for (int i = 0; i < length; i++)
                    newBuffer[i] = buffer[i];
                buffer = newBuffer;
            }
        }

        void grow(long upTo)
        {
            if (buffer == null)
                buffer = new byte[upTo];
            else if (buffer.Length < upTo)
            {
                byte[] newBuffer = new byte[upTo + buffer.Length];
                for (int i = 0; i < length; i++)
                    newBuffer[i] = buffer[i];
                buffer = newBuffer;
            }
        }

        public void append(byte value)
        {
            grow();
            buffer[length++] = value;
        }

        public void append(ushort value)
        {
            grow();
            buffer[length++] = (byte)(value >> 8);
            buffer[length++] = (byte)(value & 0xFF);
        }

        public void append(uint value)
        {
            grow();
            buffer[length++] = (byte)(value >> 24);
            buffer[length++] = (byte)((value >> 16) & 0xFF);
            buffer[length++] = (byte)((value >> 8) & 0xFF);
            buffer[length++] = (byte)(value & 0xFF);
        }

        public void append(short value)
        {
            append((ushort)value);
        }

        public void append(int value)
        {
            append((uint)value);
        }

        public void enterBitMode()
        {
	        CurrentBit = 7;
	        BitModeBuffer = 0;
	        BitMode = true;
        }

	    public void leaveBitMode()
	    {
		    if(!BitMode)
			    return;

		    if(CurrentBit != 7)
			    append(BitModeBuffer);

		    BitMode = false;
	    }

	    public void appendBit(byte bit)
	    {
		    if(bit != 0)
		    {
			    byte mask = (byte)(0x1 << CurrentBit);
			    BitModeBuffer |= mask;
		    }
		    CurrentBit --;
		    if(CurrentBit < 0)
		    {
			    append(BitModeBuffer);
			    BitModeBuffer = 0;
			    CurrentBit = 7;
		    }
	    }

        public void appendBits(long value, int highBit, int lowBit)
        {
            appendBits((ulong)value, highBit, lowBit);        
        }

	    public void appendBits(ulong value, int highBit, int lowBit)
	    {
		    if(highBit < lowBit)
			    throw new Exception("NBuffer::appendBits(unsigned __int64, int, int) : high bit is lower than low bit");

		    if(highBit > 63 || highBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned __int64, int, int) : highBit is out of range");

		    if(lowBit > 63 || lowBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned __int64, int, int) : lowBit is out of range");

		    ulong mask = (ulong) 0x1 << highBit;
		    for(int index = highBit; index >= lowBit; index--)
		    {
			    if((value & mask) == mask)
				    appendBit(1);
			    else
				    appendBit(0);

			    mask >>= 1;
		    }
	    }

	    public void appendBits(uint value, int highBit, int lowBit)
	    {
		    if(highBit < lowBit)
			    throw new Exception("NBuffer::appendBits(unsigned int, int, int) : high bit is lower than low bit");

		    if(highBit > 31 || highBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned int, int, int) : highBit is out of range");

		    if(lowBit > 31 || lowBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned int, int, int) : lowBit is out of range");

		    uint mask = (uint)(0x1 << highBit);
		    for(int index = highBit; index >= lowBit; index--)
		    {
			    if((value & mask) == mask)
				    appendBit(1);
			    else
				    appendBit(0);

			    mask >>= 1;
		    }
	    }

	    public void appendBits(ushort value, int highBit, int lowBit)
	    {
		    if(highBit < lowBit)
			    throw new Exception("NBuffer::appendBits(unsigned short, int, int) : high bit is lower than low bit");

		    if(highBit > 15 || highBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned short, int, int) : highBit is out of range");

		    if(lowBit > 15 || lowBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned short, int, int) : lowBit is out of range");

		    ushort mask = (ushort) (0x1 << highBit);
		    for(int index = highBit; index >= lowBit; index--)
		    {
			    if((value & mask) == mask)
				    appendBit(1);
			    else
				    appendBit(0);

			    mask >>= 1;
		    }
	    }

	    public void appendBits(byte value, int highBit, int lowBit)
	    {
		    if(highBit < lowBit)
			    throw new Exception("NBuffer::appendBits(unsigned char, int, int) : high bit is lower than low bit");

		    if(highBit > 7 || highBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned char, int, int) : highBit is out of range");

		    if(lowBit > 7 || lowBit < 0)
			    throw new Exception("NBuffer::appendBits(unsigned char, int, int) : lowBit is out of range");

		    byte mask = (byte) (0x1 << highBit);
		    for(int index = highBit; index >= lowBit; index--)
		    {
			    if((value & mask) == mask)
				    appendBit(1);
			    else
				    appendBit(0);

			    mask >>= 1;
		    }
	    }

        public void append(byte[] buffer, long startIndex, long length)
        {
            grow(length + this.length);
            for (int i = 0; i < length; i++)
                this.buffer[this.length++] = buffer[startIndex + i];
        }
    }
}
