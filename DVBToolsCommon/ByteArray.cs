namespace DVBToolsCommon
{
    using System;

    public class ByteArray
    {
        public byte[] buffer = null;
        public int length = 0;

        public long MaxSize = -1;
        public long GrowSize = 65536;
        private byte BitModeBuffer;
        private int CurrentBit;
        private bool BitMode;

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

        private void Grow()
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

        private void Grow(long upTo)
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

        public void Append(byte value)
        {
            Grow();
            buffer[length++] = value;
        }

        public void Append(ushort value)
        {
            Grow();
            buffer[length++] = (byte)(value >> 8);
            buffer[length++] = (byte)(value & 0xFF);
        }

        public void Append(uint value)
        {
            Grow();
            buffer[length++] = (byte)(value >> 24);
            buffer[length++] = (byte)((value >> 16) & 0xFF);
            buffer[length++] = (byte)((value >> 8) & 0xFF);
            buffer[length++] = (byte)(value & 0xFF);
        }

        public void Append(short value)
        {
            Append((ushort)value);
        }

        public void Append(int value)
        {
            Append((uint)value);
        }

        public void EnterBitMode()
        {
	        CurrentBit = 7;
	        BitModeBuffer = 0;
	        BitMode = true;
        }

	    public void LeaveBitMode()
	    {
		    if(!BitMode)
			    return;

		    if(CurrentBit != 7)
			    Append(BitModeBuffer);

		    BitMode = false;
	    }

	    public void AppendBit(byte bit)
	    {
		    if(bit != 0)
		    {
			    byte mask = (byte)(0x1 << CurrentBit);
			    BitModeBuffer |= mask;
		    }
		    CurrentBit --;
		    if(CurrentBit < 0)
		    {
			    Append(BitModeBuffer);
			    BitModeBuffer = 0;
			    CurrentBit = 7;
		    }
	    }

        public void AppendBits(long value, int highBit, int lowBit)
        {
            AppendBits((ulong)value, highBit, lowBit);        
        }

	    public void AppendBits(ulong value, int highBit, int lowBit)
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
				    AppendBit(1);
			    else
				    AppendBit(0);

			    mask >>= 1;
		    }
	    }

	    public void AppendBits(uint value, int highBit, int lowBit)
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
				    AppendBit(1);
			    else
				    AppendBit(0);

			    mask >>= 1;
		    }
	    }

	    public void AppendBits(ushort value, int highBit, int lowBit)
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
				    AppendBit(1);
			    else
				    AppendBit(0);

			    mask >>= 1;
		    }
	    }

	    public void AppendBits(byte value, int highBit, int lowBit)
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
				    AppendBit(1);
			    else
				    AppendBit(0);

			    mask >>= 1;
		    }
	    }

        public void Append(byte[] buffer, long startIndex, long length)
        {
            Grow(length + this.length);
            for (int i = 0; i < length; i++)
                this.buffer[this.length++] = buffer[startIndex + i];
        }
    }
}
