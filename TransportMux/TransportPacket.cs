namespace TransportMux
{
    public class TransportPacket
    {
        public byte[] Packet;
        public bool HasPCR = false;
        public bool HasAdaptation = false;
        public bool IsAligned = false;
        public uint PayloadSize = 0;
        public byte ContinuityIndicator = 0;
        public ushort pid = 0;
        public ushort PID
        {
            get
            {
                return pid;
            }
            set
            {
                pid = value;
                if (Packet.Length > 4)       // the header is in place
                {
                    // sync_byte = 0x47							(8 bits)	(8)			0x00	0xFF
                    // transport_error_indicator				(1 bit)		(9)			0x01	0x80
                    // payload_unit_start_indicator				(1 bit)		(10)		0x01	0x40
                    // transport_priority						(1 bit)		(11)		0x01	0x20
                    // PID										(13 bits)	(24)		0x01	0x1F	0x02	0xFF
                    // transport_scrambling_code				(2 bits)	(26)		0x03	0xC0
                    // adaptation_field_control					(2 bits)	(28)		0x03	0x30
                    // continuity_counter						(4 bits)	(32)		0x03	0x0F
                    Packet[1] = (byte)(((value >> 8) & 0x1F) | (Packet[1] & 0xE0));
                    Packet[2] = (byte)(value & 0xFF);
                }
            }
        }
        public ulong StreamTime = 0;
        public ulong DecoderStamp = 0;

        public bool BreakPacket = false;
        public bool RandomAccess = false;
        public uint pcrIndex = 0;

        public bool IsPadding = false;
        private bool discontinuityIndicator = false;
        public bool DiscontinuityIndicator
        {
            get
            {
                return discontinuityIndicator;
            }
            set
            {
                discontinuityIndicator = value;
                if (Packet.Length > 6 && HasAdaptation)      // enough room for adaptation field
                {
                    if (value)
                        Packet[5] = (byte)((Packet[5] & 0x7F) | 0x80);
                    else
                        Packet[5] &= 0x7F;
                }
            }
        }

        public TransportPacket(byte[] buffer)
        {
            Packet = buffer;

            // sync_byte = 0x47							(8 bits)	(8)			0x00	0xFF
            // transport_error_indicator				(1 bit)		(9)			0x01	0x80
            // payload_unit_start_indicator				(1 bit)		(10)		0x01	0x40
            // transport_priority						(1 bit)		(11)		0x01	0x20
            // PID										(13 bits)	(24)		0x01	0x1F	0x02	0xFF
            // transport_scrambling_code				(2 bits)	(26)		0x03	0xC0
            // adaptation_field_control					(2 bits)	(28)		0x03	0x30
            // continuity_counter						(4 bits)	(32)		0x03	0x0F
            // adaptation_field_length					(8 bits)	(40)		0x04	0xFF
            // discontinuity_indicator					(1 bit)		(41)		0x05	0x80
            // random_access_indicator					(1 bit)		(42)		0x05	0x40
            // elementary_stream_priority_indicator		(1 bit)		(43)		0x05	0x20
            // PCR_flag									(1 bit)		(44)		0x05	0x10
            // OPRC_flag								(1 bit)		(45)		0x05	0x08
            // splicing_point_flag						(1 bit)		(46)		0x05	0x04
            // transport_private_data_flag				(1 bit)		(47)		0x05	0x02
            // adaptation_field_extension_flag			(1 bit)		(48)		0x05	0x01

            HasAdaptation = ((buffer[3] & 0x20) == 0x20) ? true : false;
            IsAligned = ((buffer[1] & 0x40) == 0x40) ? true : false;
            HasPCR = false;
            if(HasAdaptation && (buffer[4] > 0) && ((buffer[5] & 0x10) == 0x10))
                HasPCR = true;
            if (HasAdaptation && (buffer[5] & 0x80) == 0x80)
                discontinuityIndicator = true;
            else
                discontinuityIndicator = false;

            ContinuityIndicator = (byte)(buffer[3] & 0x0F);            
        }

        public TransportPacket()
        {
            Packet = new byte[188];
        }

        private void FillPacket(byte value)
        {
            for (int i = 0; i < Packet.Length; i++)
                Packet[i] = value;
        }

        public void ConstructNullPacket()
        {
            FillPacket(0xFF);

            // sync_byte = 0x47					    (8 bits)	08
            Packet[0] = 0x47;

            // transport_error_indicator = 0		(1 bit)		01
            // payload_unit_start_indicator			(1 bit)		02
            // transport_priority					(1 bit)		03
            // PID									(13 bits)	16      (Null packet has PCR of 0x1FFF
            Packet[1] = 0x1F;
            Packet[2] = 0xFF;

            // transport_scrambling_control	= '00'	(2 bits)	18          0x00
            // adaptation_field_control				(2 bits)	20          0x01
            // continuity_counter					(4 bits)	24          ContinuityIndicator
            Packet[3] = (byte) (0x10 | (ContinuityIndicator & 0x0F));
        }

        public void IncrementContinuityCounter()
        {
            ContinuityIndicator ++;
            ContinuityIndicator &= 0xF;
            Packet[3] = (byte) ((Packet[3] & 0xF0) | ContinuityIndicator);
        }

        public void SetPCR(ulong pcr)
        {
            if (!HasPCR)
                return;

            ulong programClockReference = pcr / 300;
            ulong programClockReferenceExtension = pcr % 300;
            ulong output = (programClockReference & 0x1FFFFFFFF) << 15;
            output |= 0x7E00;    // reserved bits
            output |= (programClockReferenceExtension & 0x1FF);

            // program_clock_reference_base				(33 bits)	33
            // reserved									(6 bits)	39 (all ones)
            // program_clock_reference_extension		(9 bits)	48
            Packet[pcrIndex] = (byte)((output >> 40) & 0xFF);
            Packet[pcrIndex + 1] = (byte)((output >> 32) & 0xFF);
            Packet[pcrIndex + 2] = (byte)((output >> 24) & 0xFF);
            Packet[pcrIndex + 3] = (byte)((output >> 16) & 0xFF);
            Packet[pcrIndex + 4] = (byte)((output >> 8) & 0xFF);
            Packet[pcrIndex + 5] = (byte)((output & 0xFF));
        }

        public long Construct(byte [] packet, long startIndex, long packetBufferLength)
        {
	        // sync_byte = 0x47					(8 bits)	08
            Packet[0] = 0x47;

            long bytesAvailable = packetBufferLength - startIndex;

            long bytesRemaining = 184;
            if((bytesRemaining - 1) > bytesAvailable)       // Force adaptation field if even 1 byte of padding is needed
                HasAdaptation = true;
            if (bytesAvailable < bytesRemaining)        // Make sure to stuff at the end of the packet
                HasAdaptation = true;
            if (bytesAvailable == 183 && !HasPCR)
                HasPCR = true;

	        // transport_error_indicator = 0		(1 bit)		01
	        // payload_unit_start_indicator			(1 bit)		02
	        // transport_priority					(1 bit)		03
	        // PID									(13 bits)	16
            Packet[1] = (byte) ((PID >> 8) & 0x1F);
            if(IsAligned)
                Packet[1] |= 0x40;
            Packet[2] = (byte) (PID & 0xFF);

	        // transport_scrambling_control	= '00'	(2 bits)	18
	        // adaptation_field_control				(2 bits)	20
	        // continuity_counter					(4 bits)	24
            Packet[3] = (byte)(ContinuityIndicator & 0x0F);
            if(HasAdaptation)
                Packet[3] |= 0x30;
            else
                Packet[3] |= 0x10;

            uint index = 4;
            if(HasAdaptation)
            {
        		int stuffingLength = 0;
		
		        // The minimum size of a transport stream header is 4 bytes
		        // It is required to have at least a single byte in the adaptation field
		        // To specify the length of the adaptation field. So the stuffing can
		        // be at most 183 bytes
		        if(bytesAvailable < 183)
		        {
			        stuffingLength = (int) (183 - bytesAvailable);
			        BreakPacket = true;
		        }

		        bytesRemaining -= stuffingLength;

		        // Save the location of where the length should be stored
		        uint adaptationFieldLengthIndex = index;

		        // Stick a dummy value in
                Packet[index++] = 0x00;

		        if(HasPCR || RandomAccess || stuffingLength > 0)		// or other flags that may be of interest
		        {
			        // discontinuity_indicator = 0				(1 bit)		01		0x00
			        // random_access_indicator = 0				(1 bit)		02		0x00
			        // elementary_stream_priority_indicator	= 0	(1 bit)		03		0x00
			        // PCR_flag	= 1								(1 bit)		04		0x10
			        // OPRC_flag = 0							(1 bit)		05		0x00
			        // splicing_point_flag = 0					(1 bit)		06		0x00
			        // transport_private_data_flag				(1 bit)		07		0x00
			        // adaptation_field_extension_flag			(1 bit)		08		0x00
			        //															=	0x10
			        byte flags = 0x00;

                    if (DiscontinuityIndicator)
                        flags |= 0x80;
			        if(RandomAccess)
				        flags |= 0x40;
			        if(HasPCR)
				        flags |= 0x10;
				
                    Packet[index++] = flags;
        			stuffingLength --;
		        }

		        if(HasPCR)
		        {
			        pcrIndex = index;

			        // program_clock_reference_base				(33 bits)	33
			        // reserved									(6 bits)	39
			        // program_clock_reference_extension		(9 bits)	48
                    for(int i=0; i<6; i++)
                        Packet[index++] = 0x00;
  			        stuffingLength -= 6;
		        }

		        for(int i=0; i<stuffingLength; i++)
                    Packet[index++] = 0xFF;

                Packet[adaptationFieldLengthIndex] = (byte)(index - adaptationFieldLengthIndex - 1);
	        }

            bytesRemaining = 188 - index;

            for (int i = 0; i < bytesRemaining; i++)
                Packet[index++] = packet[startIndex + i];

        	return bytesRemaining;
        }
    }
}
