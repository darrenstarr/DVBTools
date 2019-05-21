using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using DVBToolsCommon;

namespace TransportMux
{
    public class MPEGAudioStream : InputStream
    {
        // packet_start_code_prefix			(24 bits)	(24)
        // stream_id						(8 bits)	(32)
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
        // DSM_trick_mode_flag				(1 bit)		(61)
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
        public const int PACKET_HEADER_LENGTH = 4;

        public const ushort MPEG_AUDIO_SYNCMASK = 0xFFE0;
        public const int PACKET_LENGTH = 188;
        public const int PACKET_PAYLOAD_LENGTH = (PACKET_LENGTH - PACKET_HEADER_LENGTH);
        public const int PACKET_PAYLOAD_LENGTH_PES = (PACKET_PAYLOAD_LENGTH - PES_HEADER_LENGTH_PTS);

	    public enum MPEGAudioVersion
	    {
		    Version2_5	= 0x00,					// unofficial
		    Version_Reserved	= 0x01,
		    Version2_0	= 0x02,					// MPEG Version 2	(ISO13818-3)
		    Version1_0	= 0x03					// MPEG Version 1	(ISO11172-3)
	    };

	    public enum MPEGAudioLayer
	    {
		    Layer_Reserved	= 0x00,
		    Layer_3			= 0x01,
		    Layer_2			= 0x02,
		    Layer_1			= 0x03
	    };

	    public enum MPEGAudioChannelMode
	    {
		    ChannelMode_Stereo		=	0x00,
		    ChannelMode_JointStereo	=	0x01,
		    ChannelMode_DualChannel	=	0x02,
		    ChannelMode_Mono		=	0x03
	    };

        // Fields from mpeg audio header
	    public MPEGAudioVersion	VersionId;
	    public MPEGAudioLayer Layer;
	    public int CrcProtection;
	    public int BitRateIndex;
	    public int FrequencyIndex;
	    MPEGAudioChannelMode ChannelMode;

	    public int SampleRate;
	    public int BitRate;
	    public int FrameSize;
	    public int Slots;

        long audioSample = 0;
        StreamBuffer streamBuffer = new StreamBuffer();

        public override double InitialPTS
        {
            get
            {
                return base.InitialPTS;
            }
            set
            {
                base.InitialPTS = value + streamDelay;
                InitialPtsInt = (long)(base.InitialPTS * 90000);
                InitialTime = (long)(base.InitialPTS * 27000000);
                if (InitialTime < TimeDivisionClocks)
                    InitialTime = 0;
                else
                    InitialTime -= TimeDivisionClocks;
		        nextTimeStamp = InitialPtsInt;
            }
        }
	    long InitialPtsInt;
        long InitialTime;

	    long Pts;
    	//long TimePerFrame;
        double TimePerFrame;

	    public byte StreamId = 0xC0;
        byte ContinuityCounter = 0;
	    long nextTimeStamp;
        long BytesBeforeEndOfFrame = 0;

        public long LastStamp = 0;

        public long CurrentStreamTime = 0;
	    public long PrebufferTime;
	    long timeDivision;
        public double TimeDivision
        {
            set
            {
                timeDivision = (long)(value * 90000);
            }
            get
            {
                return (double)timeDivision / 90000.0;
            }
        }
        public long TimeDivisionClocks
        {
            get
            {
                return timeDivision * 300;
            }
        }

        TransportPackets Packets = new TransportPackets();

        void bufferMore()
        {
            if (Packets.Count == 0)
            {
                while (!reader.AtEnd && Packets.Count < (1000000 / 188))
                    readNextBuffer(timeDivision);
            }
        }

        public string LanguageCode = "unk";

        FileStream inputFileStream = null;
        BigEndianReader reader = null;

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

	    public MPEGAudioStream(string fileName)
        {
            // Testing the buffer maximum at the maximum level
            streamBuffer.BufferSize = 3584;

	        Open(fileName);

	        // sync_word = 0x7FF		(11 bits)		0x00	0xFFE0
	        // version_id				(2 bits)		0x01	0x18
	        // layer					(2 bits)		0x01	0x06
	        // protection				(1 bit)			0x01	0x01
	        // bit_rate_code			(4 bits)		0x02	0xF0
	        // frequency				(2 bits)		0x02	0x0C
	        // padding_bit				(1 bit)			0x02	0x02
	        // private_bit				(1 bit)			0x02	0x01
	        // mode						(2 bits)		0x03	0xC0
	        // mode_extension			(2 bits)		0x03	0x30
	        // copyright				(1 bit)			0x03	0x08
	        // original_or_copy			(1 bit)			0x03	0x04
	        // emphasis					(2 bits)		0x03	0x03

	        ushort tag = 0x00;
	        tag = (ushort) (reader.ReadByte() & 0xFF);
	        tag <<= 8;
	        tag |= (ushort) (reader.ReadByte() & 0xFF);
	        while(!reader.AtEnd)
	        {
		        if((tag & MPEG_AUDIO_SYNCMASK) == MPEG_AUDIO_SYNCMASK)
		        {
			        BytesBeforeEndOfFrame = reader.Position - 2;

			        VersionId = (MPEGAudioVersion) ((tag & 0x0018) >> 3);
			        Layer = (MPEGAudioLayer) ((tag & 0x0006) >> 1);
			        CrcProtection = tag & 0x0001;

			        tag = reader.ReadByte();		// 0x02
			        BitRateIndex = (tag & 0xF0) >> 4;
			        FrequencyIndex = (tag & 0x0C) >> 2;

			        tag = reader.ReadByte();		// 0x03
			        ChannelMode = (MPEGAudioChannelMode) ((tag & 0xC0) >> 6);

			        SampleRate = mpegAudioFrequencyTable[(int)VersionId,FrequencyIndex];
			        BitRate = (int)mpegAudioBitRatesTable[(int)VersionId,(int)Layer-1,BitRateIndex];
			        Slots = (int)mpegAudioSlotsTable[(int)Layer-1];
			        FrameSize = BitRate * Slots * 1000 / SampleRate;
        			
			        Pts = (long)(mpegAudioSamplesTable[(int)Layer-1] * 90000 / SampleRate);
			        //TimePerFrame = (long) (mpegAudioSamplesTable[(int)Layer-1] * 90000 / SampleRate);
                    TimePerFrame = (double)mpegAudioSamplesTable[(int)Layer - 1] / (double)SampleRate;
        			
                    PrebufferTime = 0;
			        timeDivision = (long) (90000.0 * 0.065);

                    reader.Position = 0;

			        return;
		        }
		        tag <<= 8;
		        tag |= (ushort) (reader.ReadByte() & 0xFF);
	        }
	        throw new Exception("No MPEG-1 audio header found");
        }

        ~MPEGAudioStream()
        {
            Close();
        }

	    public override ulong NextPacketTime
        {
	        get 
	        {
                if (Packets.Count == 0)
                {
                    while (!reader.AtEnd && Packets.Count < (1000000 / 188))
                        readNextBuffer(timeDivision);
                }

                if (Packets.Count == 0)
                    return 0xFFFFFFFFFFFFFFFF;

                return Packets[0].StreamTime;
            }
        }

        public override void  GenerateProgramMap(ByteArray Map)
        {
 	        // stream_type = 0x03					(8 bits)
	        Map.appendBits((byte) 0x03, 7, 0);

	        // reserved = '111b'					(3 bits)
	        Map.appendBits((byte) 0x7, 2, 0);

	        // elementary_PID						(13 bits)
	        Map.appendBits(PID, 12, 0);

	        // reserved = '1111b'					(4 bits)
	        Map.appendBits((byte) 0xF, 3, 0);

	        // ES_info_length = 0x006 (6 bytes)		(12 bits)
	        Map.appendBits((ushort) 0x006, 11, 0);

	        // ISO_639 Language Code Descriptor
	        //		descriptor_tag = 0x0A (ISO-639)			(8 bits)
	        Map.appendBits((byte) 0x0A, 7, 0);

	        //		descriptor_length = 0x04				(8 bits)
	        Map.appendBits((byte) 0x04, 7, 0);

	        //		ISO_639_language_code					(24 bits)
	        Map.appendBits((byte) LanguageCode[0], 7, 0);
	        Map.appendBits((byte) LanguageCode[1], 7, 0);
	        Map.appendBits((byte) LanguageCode[2], 7, 0);

	        //		audio_type = 0x00						(8 bits)
	        Map.appendBits((byte) 0x00, 7, 0);
        }

        public override TransportPacket TakePacket()
        {
            return Packets.TakeFirst();
        }

        public override ushort PID
        {
	        set 
	        {
                base.PID = value;
                Packets.ChangePID(value);
	        }
        }

        void readNextBuffer(long timeDivision)
        {
	        long bytesBeforeTime = BytesBeforeEndOfFrame;
            long frames = 2;
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
                padding = (int)(bytesToConsume - bytesBeforeTime);
                bytesToConsume = bytesBeforeTime;
            }

            long timeToTransfer = 27000000 * bytesToConsume / ((BitRate * 1000) / 8);
            long timePerPacket = timeToTransfer / packets;

            long packetTime = timeDivision / packets + 30000;
            long prebufferTime = (packetTime * packets) + 6000000;

	        long decoderStamp = (nextTimeStamp * 300);
            //long streamTime = decoderStamp - timeToTransfer; 

	        for(int i=0; i<packets; i++)
	        {
                ByteArray transportData = new ByteArray();
                transportData.MaxSize = 188;

                //------------------------------
		        // Transport Packet Header
		        //------------------------------
		        // sync_byte = 0x47					(8 bits)
		        transportData.append((byte) 0x47);
		        transportData.enterBitMode();
		        // transport_error_indicator		(1 bit)
		        transportData.appendBit(0);
		        // payload_unit_start_indicator		(1 bit)
		        transportData.appendBit((byte)((i == 0) ? 1 : 0));
		        // transport_priority				(1 bit)
		        transportData.appendBit(0);
		        // PID								(13 bits)
		        transportData.appendBits(PID, 12, 0);
		        // transport_scrambling_code		(2 bits)
		        transportData.appendBits((byte) 0x0, 1, 0);
		        // adaptation_field_control			(2 bits)
                transportData.appendBits((byte)((padding > 0) ? 0x3 : 0x1), 1, 0);
		        // continuity_counter				(4 bits)
		        transportData.appendBits(ContinuityCounter, 3, 0);
                ContinuityCounter++;
		        transportData.leaveBitMode();

		        int used = 4;

                if (padding > 0)
                {
                    transportData.append((byte)(padding - 1));
                    if (padding > 1)
                        transportData.append((byte)0x00);
                    for (int paddingIndex = 2; paddingIndex < padding; paddingIndex++)
                        transportData.append((byte)0xff);
                    used += (int)padding;
                    padding = 0;
                }

		        if(i == 0)
		        {
			        //-------------------------------
			        // PES Packet Header
			        //-------------------------------
			        // packet_start_code_prefix			(24 bits)	(24)
			        transportData.append((byte) 0x00);
			        transportData.append((byte) 0x00);
			        transportData.append((byte) 0x01);
			        // stream_id						(8 bits)	(32)
			        transportData.append(StreamId);
			        long packetLengthPosition = transportData.length;

                    ushort packetLength = (ushort)(bytesToConsume + 8);
			        // Fill with dummy value
			        transportData.append(packetLength);

			        // '10'								(2 bits)	(02)	0x8000
			        // PES_scrambling_code				(2 bits)	(04)	0x0000
			        // PES_priority						(1 bit)		(05)	0x0000
			        // data_alignment_indicator			(1 bit)		(06)	0x0000
			        // copyright						(1 bit)		(07)	0x0000
			        // original_or_copy					(1 bit)		(08)	0x0000
			        // PTS_DTS_flags = '10'				(2 bits)	(10)	0x0080
			        // ESCR_flag						(1 bit)		(11)	0x0000
			        // ES_rate_flag						(1 bit)		(12)	0x0000
			        // DSM_trick_mode_flag				(1 bit)		(13)	0x0000
			        // additional_copy_info				(1 bit)		(14)	0x0000
			        // PES_CRC_flag						(1 bit)		(15)	0x0000
			        // PES_extension_flag				(1 bit)		(16)	0x0000
			        //														0x8080
			        transportData.append((ushort) 0x8480);

			        // PES_header_data_length = 0x05	(8 bits)	(08)
			        transportData.append((byte) 0x05);

			        //   '0010'							(4 bits)	(76)
			        //   PTS[32..30]					(3 bits)	(79)
			        //   marker_bit						(1 bit)		(80)
			        //   PTS[29..15]					(15 bits)	(95)
			        //   marker_bit						(1 bit)		(96)
			        //   PTS[14..0]						(15 bits)	(111)
			        //   marker_bit						(1 bit)		(112)
			        transportData.enterBitMode();
			        transportData.appendBits((byte) 0x2, 3, 0);
			        transportData.appendBits(nextTimeStamp, 32, 30);
			        transportData.appendBit(1);
			        transportData.appendBits(nextTimeStamp, 29, 15);
			        transportData.appendBit(1);
			        transportData.appendBits(nextTimeStamp, 14, 0);
			        transportData.appendBit(1);
			        transportData.leaveBitMode();

			        nextTimeStamp += Pts * frames;
			        used += 14;
		        }

		        int payloadLength = PACKET_LENGTH - used;
                long payloadTransmissionTime = (long)27000000 * (long)payloadLength / (((long)BitRate * 1000) / 8);

                for (int k = 0; k < payloadLength; k++)
			        transportData.append((byte) reader.ReadByte());

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

                /*newPacket.StreamTime = (ulong)(InitialTime + LastStamp);
                LastStamp += payloadTransmissionTime;
                newPacket.DecoderStamp = (ulong)decoderStamp;*/

                //if (newPacket.DecoderStamp < (ulong)(InitialTime - (0.100 * 27000000) + LastStamp))
                  //  throw new Exception("Buffer Underrun");

                //Packets.AddPacket(newPacket);
	        }

            CurrentStreamTime += timeToTransfer;
        }


        //bits	V1,L1	V1,L2	V1,L3	V2,L1	V2, L2 & L3
        //0000	free	free	free	free	free
        //0001	32		32		32		32		8
        //0010	64		48		40		48		16
        //0011	96		56		48		56		24
        //0100	128		64		56		64		32
        //0101	160		80		64		80		40
        //0110	192		96		80		96		48
        //0111	224		112		96		112		56
        //1000	256		128		112		128		64
        //1001	288		160		128		144		80
        //1010	320		192		160		160		96
        //1011	352		224		192		176		112
        //1100	384		256		224		192		128
        //1101	416		320		256		224		144
        //1110	448		384		320		256		160
        //1111	bad		bad		bad		bad		bad
        static uint [,,] mpegAudioBitRatesTable = new uint[4,3,16]
        {
	        { /* MPEG audio V2.5 */
		        {0,32,48,56,64,80,96,112,128,144,160,176,192,224,256,0},
		        {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160,0},
		        {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160,0}
	        },
	        { /*RESERVED*/
		        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
	        },
	        { /* MPEG audio V2 */
		        {0,32,48,56,64,80,96,112,128,144,160,176,192,224,256,0},
		        {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160,0},
		        {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160,0}
	        },
	        { /* MPEG audio V1 */
		        {0,32,64,96,128,160,192,224,256,288,320,352,384,416,448,0},
		        {0,32,48,56,64,80,96,112,128,160,192,224,256,320,384,0},
		        {0,32,40,48,56,64,80,96,112,128,160,192,224,256,320,0}
	        }
        };

        //bits		MPEG1		MPEG2		MPEG2.5
        //00		44100		22050		11025
        //01		48000		24000		12000
        //10		32000		16000		8000
        //11		reserv.		reserv.		reserv.
        static int [,] mpegAudioFrequencyTable = new int [4,4]
        {
	        /* MPEG audio V2.5 */
	        {11025,12000,8000,0},
	        /* RESERVED */
	        { 0, 0, 0, 0 }, 
	        /* MPEG audio V2 */
	        {22050,24000, 16000,0},
	        /* MPEG audio V1 */
	        {44100, 48000, 32000, 0}
        };

        static uint [] mpegAudioSlotsTable = 
        {
	        12, 
	        144, 
	        144, 
	        0
        };

        static uint [] mpegAudioSamplesTable = 
        {
	        384, 
	        1152, 
	        1152, 
	        0
        };
    }
}
