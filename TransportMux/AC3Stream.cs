namespace TransportMux
{
    using DVBToolsCommon;
    using System;
    using System.IO;

    public class AC3Stream : InputStream
    {
        #region "ISO13818-1 Systems Constants"
        // packet_start_code_prefix			(24 bits)	(24)
        // streaid						(8 bits)	(32)
        // PES_packet_length				(16 bits)	(48)
        // '10'								(2 bits)	(50)
        // PES_scrambling_code				(2 bits)	(52)
        // PES_priority						(1 bit)		(53)
        // data_alignment_indicator			(1 bit)		(54)
        // copyright						(1 bit)		(55)
        // original_or_copy					(1 bit)		(56)
        // PTS_DTS_flags					(2 bits)	(58)
        // ESCR_flag						(1 bit)		(59)
        // ES_rate_flag						(1 bit)		(60)
        // DStrick_mode_flag				(1 bit)		(61)
        // additional_copy_info				(1 bit)		(62)
        // PES_CRC_flag						(1 bit)		(63)
        // PES_extension_flag				(1 bit)		(64)
        // PES_header_data_length			(8 bits)	(72)
        //   '0010'							(4 bits)	(76)
        //   PTS[32..30]					(3 bits)	(79)
        //   marker_bit						(1 bit)		(80)
        //   PTS[29..15]					(15 bits)	(95)
        //   marker_bit						(1 bit)		(96)
        //   PTS[14..0]						(15 bits)	(111)
        //   marker_bit						(1 bit)		(112)
        //												=112 bits or 14 bytes
        public const int PES_HEADER_LENGTH_PTS = 14;

        // sync_byte = 0x47					(8 bits)	(8)
        // transport_error_indicator		(1 bit)		(9)
        // payload_unit_start_indicator		(1 bit)		(10)
        // transport_priority				(1 bit)		(11)
        // PID								(13 bits)	(24)
        // transport_scrambling_code		(2 bits)	(26)
        // adaptation_field_control			(2 bits)	(28)
        // continuity_counter				(4 bits)	(32)
        //												=32 bits or 4 bytes
        private const int PACKET_HEADER_LENGTH = 4;
        private const int PACKET_LENGTH = 188;
        private const int PACKET_PAYLOAD_LENGTH = (PACKET_LENGTH - PACKET_HEADER_LENGTH);
        private const int PACKET_PAYLOAD_LENGTH_PES = (PACKET_PAYLOAD_LENGTH - PES_HEADER_LENGTH_PTS);
        #endregion

        #region "AC-3 Constants"
        public enum BitStreamModes
        {
            MainAudioServiceCompleteMain = 0x00,
            MainAudioServiceMusicAndEffects = 0x01,
            AssociatedServiceVisuallyImpaired = 0x02,
            AssociatedServiceHearingImparied = 0x03,
            AssociatedServiceDialogue = 0x04,
            AssociatedServiceCommentary = 0x05,
            AssociatedServiceEmergency = 0x06,
            AssociatedServiceVoiceOver = 0x07
        };

        public enum AudioCodingModes
        {
            AudioCoding_2_Ch1Ch2 = 0x00,
            AudioCoding_1_C = 0x01,
            AudioCoding_2_LR = 0x02,
            AudioCoding_3_LCR = 0x03,
            AudioCoding_3_LRS = 0x04,
            AudioCoding_4_LCRS = 0x05,
            AudioCoding_4_LRSlSr = 0x06,
            AudioCoding_5_LCRSlSr = 0x07
        };

        public enum CenterLevelMixes
        {
            CenterLevelMix_Minus3db = 0x00,
            CenterLevelMix_Minus4_5db = 0x01,
            CenterLevelMix_Minus6db = 0x02
        };

        public enum SurroundLevelMixes
        {
            SurroundLevelMix_Minus3db = 0x00,
            SurroundLevelMix_Minus6db = 0x01,
            SurroundLevelMix_Zero = 0x02
        };

        public enum DolbySurroundModes
        {
            SurroundMode_NotIndicated = 0x00,
            SurroundMode_NotDolby = 0x01,
            SurroundMode_Dolby = 0x02
        };

        private const int AC3_PACKET_SAMPLES = 1536;

        #region "AC-3 Indexed Values Tables"
        private static readonly uint[] ac3_bitrate_index =
        { 
	        32,
	        40,
	        48,
	        56,
	        64,
	        80,
	        96,
	        112,
	        128,
	        160,
	        192,
	        224,
	        256,
	        320,
	        384,
	        448,
	        512,
	        576,
	        640,
	        0,0,0,0,0,0,0,0,0,0,0,0,0
        };

        private static readonly uint[,] ac3_frame_size = new uint[3, 32]
        {
            {   
		          64,   80,   96,  112,  128,  160,  192,  224, 
                 256,  320,  384,  448,  512,  640,  768,  896,
                1024, 1152, 1280,    0,    0,    0,    0,    0,
                   0,    0,    0,    0,    0,    0,    0,    0
            },
            { 
                  69,   87,  104,  121,  139,  174,  208,  243,
                 278,  348,  417,  487,  557,  696,  835,  975, 
                1114, 1253, 1393,    0,    0,    0,    0,    0,
                   0,    0,    0,    0,    0,    0,    0,    0
            },
            { 
                  96,  120,  144,  168,  192,  240,  288,  336,
                 384,  480,  576,  672,  768,  960, 1152, 1344, 
                1536, 1728, 1920,    0,    0,    0,    0,    0,
                   0,    0,    0,    0,    0,    0,    0,    0
            }
        };
        private static readonly int[] rates = 
        { 
            48000, 
            44100, 
            32000, 
            00000 
        };
        #endregion
        #endregion

        #region "Public properties"
        /// <summary>
        /// Configures the number of audio units to encoder per PES packet
        /// </summary>
        public int AudioUnitsPerPES = 2;

        /// <summary>
        /// The bitrate of the AC-3 stream as read from the AC-3 header
        /// </summary>
        public uint BitRate;

        /// <summary>
        /// The size of the main buffer (MB) as defined on page 12 of ISO13818:2
        /// </summary>
        public long MainBufferSize = 2592;

        /// <summary>
        /// The initial presentation time of the audio. 
        /// </summary>
        /// This value should be set by the multiplexer to ensure that audio begins
        /// to present itself at the same time which the first frame of video presents
        public override double InitialPTS
        {
            set
            {
                base.InitialPTS = value + streamDelay;
                InitialPtsInt = (long)(base.InitialPTS * 90000);
                NextTimeStamp = InitialPtsInt;

                InitialTime = (long)((base.InitialPTS * 27000000) - (MainBufferFillTime * 11 / 10));
            }
        }

        /// <summary>
        /// The language code to be used when constructing the ISO639 language code descriptor within the PMT
        /// </summary>
        public string LanguageCode = "unk";
        #endregion

        #region "Bitstream derived values"
        private int SampleRate;
        private int FrameSize;
        private int BitStreamCode;
        private BitStreamModes BitStreamMode;
        private AudioCodingModes AudioCodingMode;
        private CenterLevelMixes CenterLevelMix;
        private SurroundLevelMixes SurroundLevelMix;
        private DolbySurroundModes SurroundMode;
        private int LFEOn;
        #endregion

        #region "Overridden virtual members from InputStream"
        public override ushort PID
        {
            set
            {
                base.PID = value;
                Packets.ChangePID(value);
            }
        }

        public override ulong NextPacketTime
        {
            get
            {
                if (Packets.Count == 0)
                {
                    while (!reader.AtEnd && Packets.Count < (1000000 / 188))
                        ReadNextBuffer();
                }

                if (Packets.Count == 0)
                    return 0xFFFFFFFFFFFFFFFF;

                return Packets[0].StreamTime;
            }
        }
        #endregion

        /// <summary>
        /// Returns the period of time required to fill the main buffer (MB) to peak from zero based on the stram bitrate
        /// </summary>
        private long MainBufferFillTime
        {
            get
            {
                return (MainBufferSize * 8) / BitRate;
            }
        }

        /// <summary>
        /// The initial time which should be used for inserting packets into the stream. 
        /// </summary>
        /// This value is calculated when the initial PTS is assigned as follows :
        ///   InitialTime = InitialPTS - 1.1 * MainBufferFillTime
        /// <todo>
        /// Find a more "correct" approach to calculating the initial packet time, simply assuming
        /// a 10% over MainBufferFillTime delay, while functional (maybe logical) seems a little too
        /// hackish
        /// </todo>
        private long InitialTime;

        /// <summary>
        /// The 27Mhz representation of the inital presentation time of the first audio unit of the stream
        /// </summary>
        private long InitialPtsInt;

        /// <summary>
        /// The 90Khz representation of the duration of a single coded audio unit
        /// </summary>
        private long AudioUnitDuration90Khz;

        /// <summary>
        ///  A 90Khz representation of a running sum of the next presentation time stamp to print to the stream
        /// </summary>
        /// <todo>
        /// figure out if it makes more sense just to multiply the number of transmitted audio units by the
        /// 90Khz value for representing the playtime of a single frame.
        /// </todo>
        private long NextTimeStamp;

        /// <summary>
        /// The play duration of a single coded audio unit
        /// </summary>
        private double TimePerFrame;

        /// <summary>
        /// The transport stream packet continuity_counter
        /// </summary>
        private byte ContinuityCounter = 0;

        /// <summary>
        /// Tracks the number of bytes from the last audio unit contained within the previously coded PES packet
        /// </summary>
        private long BytesBeforeEndOfFrame = 0;

        /// <summary>
        /// A 27Mhz representation of the transmission time (PCR relative) of the last constructed transport packet
        /// </summary>
        private long LastStamp = 0;

        #region "Values from AC-3 Header needed for descriptor"
        /// <summary>
        /// The sample rate code as extracted from the AC-3 header (fscod)
        /// </summary>
        private int sampleRateCode = 0;

        /// <summary>
        /// Returns the sample rate code to be inserted in the AC-3 Descriptor(0x81) for the stream
        /// </summary>
        /// For the moment, we just return the sampleRateCode as extracted from the AC-3 header as fscod
        private int descriptorSampleRateCode
        {
            get
            {
                return sampleRateCode;
            }
        }

        /// <summary>
        /// The bit rate code as expected by the AC-3 stream descriptor
        /// </summary>
        /// This value is deduced by taking the frmsizecod from the syncinfo in the header and shifting it right 1
        private int bitRateCode = 0;

        /// <summary>
        /// The surround mode value extracted from the AC-3 header as expected by the AC-3 stream descriptor
        /// </summary>
        private int dsurmod = 0;

        /// <summary>
        /// Returns the acmod value as extracted from the AC-3 header bsi block
        /// </summary>
        private int numChannels
        {
            get
            {
                return (int)AudioCodingMode;
            }
        }

        #endregion

        private long audioSample = 0;
        private StreamBuffer streamBuffer = new StreamBuffer();
        private TransportPackets Packets = new TransportPackets();
        private FileStream inputFileStream = null;
        private BigEndianReader reader = null;

        public override void Close()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (inputFileStream != null)
            {
                inputFileStream.Close();
                inputFileStream = null;
            }
        }

        public void Open(string fileName)
        {
            Close();
            inputFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 10 * 1024 * 1024);
            reader = new BigEndianReader(inputFileStream);
        }

	    public AC3Stream(string fileName)
        {
            Open(fileName);

	        ushort tag = 0x00;
	        tag = (ushort) (reader.ReadByte() & 0xFF);
	        tag <<= 8;
	        tag |= (ushort) (reader.ReadByte() & 0xFF);
	        while(!reader.AtEnd)
	        {
		        if(tag == 0x0b77)
		        {
			        BytesBeforeEndOfFrame = reader.Position - 2;

			        ushort crc = (ushort) (reader.ReadByte() & 0xFF);
			        crc <<= 8;
			        crc |= (ushort) (reader.ReadByte() & 0xFF);

			        byte buffer = reader.ReadByte();
			        sampleRateCode = (buffer >> 6) & 0x3;
			        SampleRate = SampleRateFromCode(sampleRateCode);
			        int frameSizeCode = (buffer & 0x3F);
                    bitRateCode = (frameSizeCode >> 1);

			        FrameSize = (int)ac3_frame_size[sampleRateCode,frameSizeCode>>1];
			        if(((frameSizeCode & 1) != 0) && sampleRateCode == 1)
				        FrameSize++;
			        else
				        FrameSize <<= 1;

			        BitRate = ac3_bitrate_index[frameSizeCode >> 1];

			        buffer = reader.ReadByte();
			        BitStreamCode = (buffer >> 3) & 0x1F;
			        BitStreamMode = (BitStreamModes) (buffer & 0x7);

			        buffer = reader.ReadByte();
			        AudioCodingMode = (AudioCodingModes) ((buffer >> 5) & 0x7);

                    int bitShift = 0;
                    if(((int)AudioCodingMode & 0x1) == 0x1 && (int)AudioCodingMode != 0x1)
                    {
    			        CenterLevelMix = (CenterLevelMixes) ((buffer >> 3) & 0x3);
                        bitShift += 2;
                    }
                    if (((int)AudioCodingMode & 0x4) == 0x4)
                    {
                        SurroundLevelMix = (SurroundLevelMixes)((buffer >> 1) & 0x3);
                        bitShift += 2;
                    }
                    if ((int)AudioCodingMode == 2)
                    {
                        if (bitShift == 0)
                            SurroundMode = (DolbySurroundModes)((buffer >> 3) & 0x3);
                        else if (bitShift == 2)
                            SurroundMode = (DolbySurroundModes)((buffer >> 1) & 0x3);
                        else
                            throw new Exception("Somehow acmod allows surmixlev and dsurmod to be present together");
                        bitShift += 2;

                        dsurmod = (int)SurroundMode;
                    }
                    else
                        dsurmod = 0;

                    if (bitShift == 0)
                        LFEOn = ((buffer >> 4) & 0x1);
                    else if (bitShift == 2)
                        LFEOn = ((buffer >> 2) & 0x1);
                    else
                        LFEOn = (buffer & 0x1);

			        TimePerFrame = (double) AC3_PACKET_SAMPLES / (double) SampleRate;
                    AudioUnitDuration90Khz = AC3_PACKET_SAMPLES * 90000 / SampleRate;

                    reader.Position = 0;

			        return;
		        }
		        tag <<= 8;
		        tag |= (ushort) (reader.ReadByte() & 0xFF);
	        }
	        throw new Exception("No AC-3 header found");

        }

        ~AC3Stream()
        {
            Close();
        }

        private void BufferMore()
        {
            if (Packets.Count == 0)
            {
                while (!reader.AtEnd && Packets.Count < (1000000 / 188))
                    ReadNextBuffer();
            }
        }

        private int SampleRateFromCode(int code)
        {        	
        	return rates[code];
        }

        private void ReadNextBuffer()
        {
            long bytesBeforeTime = BytesBeforeEndOfFrame;
            long frames = AudioUnitsPerPES; 
	        bytesBeforeTime += frames * FrameSize;

            StreamBufferEvent ev = new StreamBufferEvent(0 - (FrameSize + 14), InitialPTS + (audioSample * TimePerFrame));
            streamBuffer.AddEvent(ev);
            audioSample++;
            for (int currentFrame = 1; currentFrame < frames; currentFrame++)
            {
                ev = new StreamBufferEvent(0 - FrameSize, InitialPTS + (audioSample * TimePerFrame));
                audioSample++;
                streamBuffer.AddEvent(ev);
            }

            // Attempt to compensate for stream ending
            if (bytesBeforeTime > reader.Remaining)
                bytesBeforeTime = reader.Remaining;

	        long packets = 1 + ((bytesBeforeTime - PACKET_PAYLOAD_LENGTH_PES) / PACKET_PAYLOAD_LENGTH);
	        long bytesToConsume = PACKET_PAYLOAD_LENGTH_PES + (packets - 1) * PACKET_PAYLOAD_LENGTH;
	        BytesBeforeEndOfFrame = bytesBeforeTime - bytesToConsume;
            if (bytesBeforeTime == reader.Remaining)
                BytesBeforeEndOfFrame = 0;

            int padding = 0;
            if (bytesToConsume > bytesBeforeTime)
            {
                padding = (int) (bytesToConsume - bytesBeforeTime);
                bytesToConsume = bytesBeforeTime;
            }

            long decoderStamp = (NextTimeStamp * 300);

            for(int i=0; i<packets; i++)
	        {
                ByteArray transportData = new ByteArray();
                transportData.MaxSize = 188;

		        //------------------------------
		        // Transport Packet Header
		        //------------------------------
		        // sync_byte = 0x47					(8 bits)
		        transportData.Append((byte) 0x47);
		        transportData.EnterBitMode();
		        // transport_error_indicator		(1 bit)
		        transportData.AppendBit(0);
		        // payload_unit_start_indicator		(1 bit)
		        transportData.AppendBit((byte)((i == 0) ? 1 : 0));
		        // transport_priority				(1 bit)
		        transportData.AppendBit(0);
		        // PID								(13 bits)
		        transportData.AppendBits(PID, 12, 0);
		        // transport_scrambling_code		(2 bits)
		        transportData.AppendBits((byte) 0x0, 1, 0);
		        // adaptation_field_control			(2 bits)
		        transportData.AppendBits((byte) ((padding > 0) ? 0x3 : 0x1), 1, 0);
		        // continuity_counter				(4 bits)
		        transportData.AppendBits(ContinuityCounter, 3, 0);
                ContinuityCounter++;
		        transportData.LeaveBitMode();

                int used = 4;

                if (padding > 0)
                {
                    transportData.Append((byte)(padding - 1));
                    if (padding > 1)
                        transportData.Append((byte)0x00);
                    for (int paddingIndex = 2; paddingIndex < padding; paddingIndex++)
                        transportData.Append((byte)0xff);
                    used += (int) padding;
                    padding = 0;
                }

		        if(i == 0)
		        {
			        //-------------------------------
			        // PES Packet Header
			        //-------------------------------
			        // packet_start_code_prefix			(24 bits)	(24)
			        // streaid						(8 bits)	(32)
			        transportData.Append((uint) 0x000001BD);
			        long packetLengthPosition = transportData.length;

                    ushort packetLength = (ushort)(bytesToConsume + 8);
			        // Fill with dummy value
			        transportData.Append(packetLength);

			        // '10'								(2 bits)	(02)	0x8000
			        // PES_scrambling_code				(2 bits)	(04)	0x0000
			        // PES_priority						(1 bit)		(05)	0x0000
			        // data_alignment_indicator			(1 bit)		(06)	0x0400
			        // copyright						(1 bit)		(07)	0x0000
			        // original_or_copy					(1 bit)		(08)	0x0000
			        // PTS_DTS_flags = '10'				(2 bits)	(10)	0x0080
			        // ESCR_flag						(1 bit)		(11)	0x0000
			        // ES_rate_flag						(1 bit)		(12)	0x0000
			        // DStrick_mode_flag				(1 bit)		(13)	0x0000
			        // additional_copy_info				(1 bit)		(14)	0x0000
			        // PES_CRC_flag						(1 bit)		(15)	0x0000
			        // PES_extension_flag				(1 bit)		(16)	0x0000
			        //														0x8480
			        transportData.Append((ushort) 0x8480);

			        // PES_header_data_length = 0x05	(8 bits)	(08)
			        transportData.Append((byte) 0x05);

			        //   '0010'							(4 bits)	(76)
			        //   PTS[32..30]					(3 bits)	(79)
			        //   marker_bit						(1 bit)		(80)
			        //   PTS[29..15]					(15 bits)	(95)
			        //   marker_bit						(1 bit)		(96)
			        //   PTS[14..0]						(15 bits)	(111)
			        //   marker_bit						(1 bit)		(112)
			        transportData.EnterBitMode();
			        transportData.AppendBits((byte) 0x2, 3, 0);
			        transportData.AppendBits(NextTimeStamp, 32, 30);
			        transportData.AppendBit(1);
			        transportData.AppendBits(NextTimeStamp, 29, 15);
			        transportData.AppendBit(1);
			        transportData.AppendBits(NextTimeStamp, 14, 0);
			        transportData.AppendBit(1);
			        transportData.LeaveBitMode();

                    NextTimeStamp += AudioUnitDuration90Khz * frames;
			        used += 14;
		        }

		        int payloadLength = PACKET_LENGTH - used;
                long payloadTransmissionTime = (long)27000000 * (long)payloadLength / (((long)BitRate * 1000) / 8);

                for(int k=0; k<payloadLength; k++)
			        transportData.Append((byte) reader.ReadByte());                

		        TransportPacket newPacket = new TransportPacket(transportData.buffer);

                long streamTime = (InitialTime - ((long)((TimePerFrame * 2) * 27000000)) + LastStamp);
                streamBuffer.SetTime(streamTime);
                while (!streamBuffer.CanAdd(184))
                {
                    long newStreamTime = streamBuffer.NextEventTime;
                    LastStamp += newStreamTime - streamTime;
                    streamTime = newStreamTime;
                    streamBuffer.SetTime(streamTime);                   
                }
                streamBuffer.AddEvent(184, (long)streamTime);

                newPacket.StreamTime = (ulong)streamTime; 
                LastStamp += payloadTransmissionTime;
                newPacket.DecoderStamp = (ulong)decoderStamp;
                decoderStamp += payloadTransmissionTime;
                Packets.AddPacket(newPacket);
	        }
        }

	    public override TransportPacket TakePacket()
        {
            return Packets.TakeFirst();
        }

        public override void GenerateProgramMap(ByteArray map)
        {
	        // stream_type = 0x81					(8 bits)
	        map.AppendBits((byte) 0x81, 7, 0);

	        // reserved = '111b'					(3 bits)
	        map.AppendBits((byte) 0x7, 2, 0);

	        // elementary_PID						(13 bits)
	        map.AppendBits(PID, 12, 0);

	        // reserved = '1111b'					(4 bits)
	        map.AppendBits((byte) 0xF, 3, 0);

            // ES_info_length = 0x013 (19 bytes)	(12 bits)
	        // Old : ES_info_length = 0x00c (12 bytes)	(12 bits)
	        map.AppendBits((ushort) 0x013, 11, 0);

	        // AC-3 Descriptor
	        //		descriptor_tag = 0x05 (registration)	(8 bits)
	        map.AppendBits((byte) 0x05, 7, 0);

	        //		descriptor_length = 0x04				(8 bits)
	        map.AppendBits((byte) 0x04, 7, 0);

	        //		format_identifier = 'AC-3'				(32 bits)
	        map.AppendBits((byte) 'A', 7, 0);
	        map.AppendBits((byte) 'C', 7, 0);
	        map.AppendBits((byte) '-', 7, 0);
	        map.AppendBits((byte) '3', 7, 0);

	        // ISO_639 Language Code Descriptor
	        //		descriptor_tag = 0x0A (ISO-639)			(8 bits)
	        map.AppendBits((byte) 0x0A, 7, 0);

	        //		descriptor_length = 0x04				(8 bits)
	        map.AppendBits((byte) 0x04, 7, 0);

	        //		ISO_639_language_code					(24 bits)
	        map.AppendBits((byte) LanguageCode[0], 7, 0);
	        map.AppendBits((byte) LanguageCode[1], 7, 0);
	        map.AppendBits((byte) LanguageCode[2], 7, 0);

	        //		audio_type = 0x00						(8 bits)
	        map.AppendBits((byte) 0x00, 7, 0);

            // AC-3_audio_stream_descriptor()
            //      descriptor_tag = 0x81                   (8 bits)
            map.AppendBits((byte)0x81, 7, 0);

            //      descriptor_length = 5                   (8 bits)
            map.AppendBits((byte) 0x5, 7, 0);

            //      sample_rate_code                        (3 bits)
            map.AppendBits((byte)descriptorSampleRateCode, 2, 0);

            //      bsid = '01000'                          (5 bits)
            map.AppendBits((byte)0x08, 4, 0);

            //      bit_rate_code                           (6 bits)
            map.AppendBits((byte)bitRateCode, 5, 0);
            
            //      surround_mode                           (2 bits)
            map.AppendBits((byte)dsurmod, 1, 0);

            //      bsmod                                   (3 bits)
            map.AppendBits((byte)BitStreamMode, 2, 0);

            //      num_channels                            (4 bits)
            map.AppendBits((byte)numChannels, 3, 0);

            //      full_svc (always true for us)           (1 bit)
            map.AppendBit(1);

            //      langcod (always 0 for us)               (8 bits)
            map.AppendBits((byte)0, 7, 0);

            //      mainid (always 0 for us)                (3 bits)
            map.AppendBits((byte)0, 2, 0);

            //      priority (always 3=not specified) for us    (2 bits)
            // TODO : Add a "primaryAudio" flag for this class, if set, then this value will be 1, alternatively, then 2
            map.AppendBits((byte)3, 1, 0);

            //      reserved (all ones)                     (3 bits)
            map.AppendBits((byte)7, 2, 0);
        }        
    }
}
