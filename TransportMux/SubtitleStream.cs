using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using DVBToolsCommon;

namespace TransportMux
{
    public class SubtitleStream : InputStream
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
        const int PES_HEADER_LENGTH_PTS = 14;

        // sync_byte = 0x47					(8 bits)	(8)
        // transport_error_indicator		(1 bit)		(9)
        // payload_unit_start_indicator		(1 bit)		(10)
        // transport_priority				(1 bit)		(11)
        // PID								(13 bits)	(24)
        // transport_scrambling_code		(2 bits)	(26)
        // adaptation_field_control			(2 bits)	(28)
        // continuity_counter				(4 bits)	(32)
        //												=32 bits or 4 bytes
        const int PACKET_HEADER_LENGTH = 4;

        const int PACKET_LENGTH = 188;
        const int PACKET_PAYLOAD_LENGTH = (PACKET_LENGTH - PACKET_HEADER_LENGTH);
        const int PACKET_PAYLOAD_LENGTH_PES = (PACKET_PAYLOAD_LENGTH - PES_HEADER_LENGTH_PTS);

        const int MultiplexBitRate = 192000;
        const int MultiplexByteRate = MultiplexBitRate / 8;

        public ushort CompositionPageId = 2;
        public ushort AncillaryPageId = 2;
        public string LanguageCode = "unk";        

        SubtitleItemList SubtitleItems;
        //long lastPresentationTime = 0;

        public override double InitialPTS
        {
            set
            {
                base.InitialPTS = value;
                InitialPtsInt = (long)(value * 90000);
            }
        }
        long InitialPtsInt;
        byte ContinuityCounter = 0;
        
        TransportPackets Packets = new TransportPackets();

        public SubtitleStream(string fileName)
        {
            SubtitleItems = new SubtitleItemList(fileName);
        }

        ~SubtitleStream()
        {
            Close();
        }

        void bufferMore()
        {
            if (Packets.Count == 0)
            {
                Packets = SubtitleItems.RegionList.ReadMorePackets(InitialPtsInt, PID);
                /*if (SubtitleItems.Count > 0 && SubtitleItems[0] != null)
                {
                    SubtitleItem item = SubtitleItems[0];
                    SubtitleItems.RemoveAt(0);

                    while (lastPresentationTime > 0 && (item.PresentationTime - lastPresentationTime) >= 650)
                    {
                        lastPresentationTime += 650;
                        packetizePadding(lastPresentationTime);
                    }

                    packetizeFile(item.fileName, item.PresentationTime);
                    lastPresentationTime = item.PresentationTime;
                }*/
            }
        }

        public override TransportPacket TakePacket()
        {
            if (Packets.Count == 0)
                bufferMore();

            return Packets.TakeFirst();
        }

        public override ulong  NextPacketTime
        {
            get
            {
                if (Packets.Count == 0)
                    bufferMore();

                if (Packets.Count == 0)
                    return 0xFFFFFFFFFFFFFFFF;

                return Packets[0].StreamTime;
            }
        }

        public override void Close()
        {
        }

        public override void  GenerateProgramMap(ByteArray Map)
        {
	        // stream_type = 0x81					(8 bits)
	        Map.appendBits((byte) 0x06, 7, 0);

	        // reserved = '111b'					(3 bits)
	        Map.appendBits((byte) 0x7, 2, 0);

	        // elementary_PID						(13 bits)
	        Map.appendBits(PID, 12, 0);

	        // reserved = '1111b'					(4 bits)
	        Map.appendBits((byte) 0xF, 3, 0);

	        // ES_info_length = 0x00a (10 bytes)	(12 bits)
	        Map.appendBits((ushort) 0x00a, 11, 0);

	        // Subtitling Descriptor
	        //		descriptor_tag = 0x59 (subtitling)		(8 bits)
	        Map.appendBits((byte) 0x59, 7, 0);

	        //		descriptor_length = 0x08				(8 bits)
	        Map.appendBits((byte) 0x08, 7, 0);

	        // ISO_639 Language Code Descriptor
	        //		ISO_639_language_code					(24 bits)
	        Map.appendBits((byte) LanguageCode[0], 7, 0);
	        Map.appendBits((byte) LanguageCode[1], 7, 0);
	        Map.appendBits((byte) LanguageCode[2], 7, 0);

	        //		subtitling_type = 0x10					(8 bits)
	        Map.appendBits((byte) 0x10, 7, 0);

	        //		composition_page_id						(16 bits)
	        Map.appendBits(CompositionPageId, 15, 0);

	        //		ancillary_page_id						(16 bits)
	        Map.appendBits(AncillaryPageId, 15, 0);
        }

        public override ushort  PID
        {
	        set 
	        {
                base.PID = value;
                Packets.ChangePID(value);
	        }
        }

        /// <summary>
        /// This function is a hack to remove a padding packet if it's transmitted after a valid 
        /// subtitle packet. The correct solution is to go back and properly design the stream buffer
        /// for the subtitle stream
        /// </summary>
        void KillPacketsAfterTime(ulong time)
        {
            TransportPacket packet = Packets.Last();
            while (packet != null && packet.StreamTime >= (ulong)time)
            {
                Packets.RemoveLast();
                packet = Packets.Last();
                ContinuityCounter--;    // Rolling back the continuity indicator should do the trick
            }
        }

        void packetizePadding(long timeStamp)
        {
            // Stuffing bytes = 188 - (packet header length - pes header length - 4 bytes of padding)

            int stuffingBytes = 188 - (PES_HEADER_LENGTH_PTS + PACKET_HEADER_LENGTH + 12);

            timeStamp *= 90;            // Correct for 90Khz clock
            timeStamp += InitialPtsInt; // Increase spacing for initial PTS

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
	        transportData.appendBit((byte)(1));
	        // transport_priority				(1 bit)
	        transportData.appendBit(0);
	        // PID								(13 bits)
	        transportData.appendBits(PID, 12, 0);
	        // transport_scrambling_code		(2 bits)
	        transportData.appendBits((byte) 0x0, 1, 0);
	        // adaptation_field_control			(2 bits)
	        transportData.appendBits((byte) ((stuffingBytes > 0) ? 0x3 : 0x1), 1, 0);
	        // continuity_counter				(4 bits)
	        transportData.appendBits(ContinuityCounter, 3, 0);
            ContinuityCounter++;
	        transportData.leaveBitMode();

	        long used = 4;
	        if(stuffingBytes > 0)
	        {
		        // Adaptation field for stuffing
		        stuffingBytes--;
                used++;
		        transportData.append((byte) stuffingBytes);	// adaptation_field_length
		        if(stuffingBytes > 0)
		        {
                    used += stuffingBytes;
			        stuffingBytes --;
			        transportData.append((byte) 0x00);	// flags field

                    for(int k=0; k<stuffingBytes; k++)
				        transportData.append((byte) 0xFF);	// stuffing byte
		        }
	        }

            //-------------------------------
            // PES Packet Header
            //-------------------------------
            // packet_start_code_prefix			(24 bits)	(24)
            // stream_id						(8 bits)	(32)
            transportData.append((uint)0x000001BD);
            long packetLengthPosition = transportData.length;

            ushort packetLength = (ushort)(20);
            // Fill with dummy value
            transportData.append(packetLength);

            // '10'								(2 bits)	(02)	0x8000
            // PES_scrambling_code				(2 bits)	(04)	0x0000
            // PES_priority						(1 bit)		(05)	0x0000
            // data_alignment_indicator			(1 bit)		(06)	0x0400
            // copyright						(1 bit)		(07)	0x0000
            // original_or_copy					(1 bit)		(08)	0x0000
            // PTS_DTS_flags = '10'				(2 bits)	(10)	0x0080
            // ESCR_flag						(1 bit)		(11)	0x0000
            // ES_rate_flag						(1 bit)		(12)	0x0000
            // DSM_trick_mode_flag				(1 bit)		(13)	0x0000
            // additional_copy_info				(1 bit)		(14)	0x0000
            // PES_CRC_flag						(1 bit)		(15)	0x0000
            // PES_extension_flag				(1 bit)		(16)	0x0000
            //														0x8480
            transportData.append((ushort)0x8480);

            // PES_header_data_length = 0x05	(8 bits)	(08)
            transportData.append((byte)0x05);

            //   '0010'							(4 bits)	(76)
            //   PTS[32..30]					(3 bits)	(79)
            //   marker_bit						(1 bit)		(80)
            //   PTS[29..15]					(15 bits)	(95)
            //   marker_bit						(1 bit)		(96)
            //   PTS[14..0]						(15 bits)	(111)
            //   marker_bit						(1 bit)		(112)
            transportData.enterBitMode();
            transportData.appendBits((byte)0x2, 3, 0);
            transportData.appendBits(timeStamp, 32, 30);
            transportData.appendBit(1);
            transportData.appendBits(timeStamp, 29, 15);
            transportData.appendBit(1);
            transportData.appendBits(timeStamp, 14, 0);
            transportData.appendBit(1);
            transportData.leaveBitMode();

            used += 14;

            transportData.append((byte)0x20);
            transportData.append((byte)0x00);
            transportData.append((byte)0x0F);
            transportData.append((byte)0xFF);//4
            transportData.append((byte)0x00);
            transportData.append((byte)0x02);
            transportData.append((byte)0x00);
            transportData.append((byte)0x03);//8
            transportData.append((byte)0xFF);
            transportData.append((byte)0xFF);
            transportData.append((byte)0xFF);
            transportData.append((byte)0xFF);//12

            long decoderStamp = (timeStamp * 300);

            long insertTime = (long)(27000000 * 0.040);
            long streamTime = decoderStamp - insertTime;

            TransportPacket newPacket = new TransportPacket(transportData.buffer);
            newPacket.StreamTime = (ulong)streamTime;
            newPacket.DecoderStamp = (ulong)decoderStamp;
            newPacket.PID = PID;
            newPacket.IsPadding = true;

            Packets.AddPacket(newPacket);
        }

	    void packetizeFile(string fileName, long timeStamp)
        {
            FileStream inputFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BigEndianReader reader = new BigEndianReader(inputFileStream);

            long dataLength = inputFileStream.Length;

            /*int expectedAdaptationFieldLength = 0; // (int)((dataLength + PES_HEADER_LENGTH_PTS) % 184);

            int endStuffing = (expectedAdaptationFieldLength == 1) ? 12 : 0;

            byte [] readBuffer = new byte[dataLength + endStuffing];
            reader.Read(readBuffer, 0, (int) dataLength);
            if (expectedAdaptationFieldLength == 1)
            {
                readBuffer[dataLength++] = 0x20;
                readBuffer[dataLength++] = 0x00;
                readBuffer[dataLength++] = 0x0F;
                readBuffer[dataLength++] = 0xFF;//4
                readBuffer[dataLength++] = 0x00;
                readBuffer[dataLength++] = 0x02;
                readBuffer[dataLength++] = 0x00;
                readBuffer[dataLength++] = 0x03;//8
                readBuffer[dataLength++] = 0xFF;
                readBuffer[dataLength++] = 0xFF;
                readBuffer[dataLength++] = 0xFF;
                readBuffer[dataLength++] = 0xFF;//12
            }
            int readIndex = 0;*/
           
	        long lengthAfterPESHeader = dataLength - 14;
	        long packets = (lengthAfterPESHeader / 184) + (((lengthAfterPESHeader % 184) == 0) ? 0 : 1);

	        timeStamp *= 90;
	        timeStamp += InitialPtsInt;
	        long decoderStamp = (timeStamp * 300);

            long insertTime = 27000000 * dataLength / MultiplexByteRate;
            long packetSpacingTime = insertTime / packets;
            insertTime += packetSpacingTime;
            long streamTime = decoderStamp - insertTime;

	        long bytesRemaining = dataLength;            

	        int i = 0;
	        while(bytesRemaining > 0)
	        {
		        long stuffingBytes = 0;
		        int headerSize = (i==0) ? 18 : 4;
		        if(bytesRemaining < 188 - headerSize)
			        stuffingBytes = 188 - headerSize - bytesRemaining;

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
		        transportData.appendBits((byte) ((stuffingBytes > 0) ? 0x3 : 0x1), 1, 0);
		        // continuity_counter				(4 bits)
		        transportData.appendBits(ContinuityCounter, 3, 0);
                ContinuityCounter++;
		        transportData.leaveBitMode();

		        long used = 4;
		        if(stuffingBytes > 0)
		        {
			        // Adaptation field for stuffing
			        stuffingBytes--;
                    used++;
			        transportData.append((byte) stuffingBytes);
                    if (stuffingBytes > 0)
                    {
                        used += stuffingBytes;
                        stuffingBytes--;
                        transportData.append((byte)0x00);	// flags field

                        for (int k = 0; k < stuffingBytes; k++)
                            transportData.append((byte)0xFF);	// stuffing byte
                    }
                    //else
                    //    throw new Exception("Adaptation Field length = 0");
		        }

		        if(i == 0)
		        {
			        //-------------------------------
			        // PES Packet Header
			        //-------------------------------
			        // packet_start_code_prefix			(24 bits)	(24)
			        // stream_id						(8 bits)	(32)
			        transportData.append((uint) 0x000001BD);
			        long packetLengthPosition = transportData.length;

                    ushort packetLength = (ushort)(dataLength + 8);
			        // Fill with dummy value
			        transportData.append(packetLength);

			        // '10'								(2 bits)	(02)	0x8000
			        // PES_scrambling_code				(2 bits)	(04)	0x0000
			        // PES_priority						(1 bit)		(05)	0x0000
			        // data_alignment_indicator			(1 bit)		(06)	0x0400
			        // copyright						(1 bit)		(07)	0x0000
			        // original_or_copy					(1 bit)		(08)	0x0000
			        // PTS_DTS_flags = '10'				(2 bits)	(10)	0x0080
			        // ESCR_flag						(1 bit)		(11)	0x0000
			        // ES_rate_flag						(1 bit)		(12)	0x0000
			        // DSM_trick_mode_flag				(1 bit)		(13)	0x0000
			        // additional_copy_info				(1 bit)		(14)	0x0000
			        // PES_CRC_flag						(1 bit)		(15)	0x0000
			        // PES_extension_flag				(1 bit)		(16)	0x0000
			        //														0x8480
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
			        transportData.appendBits(timeStamp, 32, 30);
			        transportData.appendBit(1);
			        transportData.appendBits(timeStamp, 29, 15);
			        transportData.appendBit(1);
			        transportData.appendBits(timeStamp, 14, 0);
			        transportData.appendBit(1);
			        transportData.leaveBitMode();

			        used += 14;
		        }

		        int payloadLength = PACKET_LENGTH - (int) used;
		        bytesRemaining -= payloadLength;
                for(int k=0; k<payloadLength; k++)
                    transportData.append((byte)reader.ReadByte());

		        TransportPacket newPacket = new TransportPacket(transportData.buffer);
		        newPacket.StreamTime = (ulong)streamTime + (ulong)(i * packetSpacingTime);
                newPacket.DecoderStamp = (ulong)decoderStamp;
                newPacket.PID = PID;

                if (newPacket.StreamTime >= newPacket.DecoderStamp)
                    throw new Exception("Packet is scheduled for delivery after decoder time");

                KillPacketsAfterTime(newPacket.StreamTime);
                Packets.AddPacket(newPacket);
		        i++;
	        }
            reader.Close();
            inputFileStream.Close();
        }
    }

    class SubtitleItem
    {
	    public String fileName = "";
	    public long PresentationTime = -1;
	    public long StartOffset = -1;
	    public long Length = -1;

        public bool IsValid
        {
            get
            {
                if(fileName == string.Empty)
                    return false;
                if(Length == -1 || PresentationTime == -1 || StartOffset == -1)
                    return false;
                return true;
            }
        }
    };

    class TimelineRegion
    {
        public long PacketStart = 0;
        public long PacketEnd
        {
            get
            {
                return PacketStart + (long)PacketCount - 1;
            }
            set
            {
                PacketStart = value - PacketCount + 1;
            }
        }

        public string SourceFile = string.Empty;
        public long SourceFileOffset = 0;
        public bool GeneratePTSPacket = false;
        public bool GeneratePESPadding = false;

        /// <summary>
        /// The presentation time stamp of the subtitle to be transmitted without initial PTS compensation
        /// </summary>
        public long PresentationTimeStamp
        {
            get
            {
                return (long)Milliseconds * 90;
            }
        }

        public long Milliseconds = 0;
        public int ItemLength = 0;
        public int PayloadInFirstPacket
        {
            get
            {
                return Math.Min(TimelineRegionList.PESPacketPayloadSize, ItemLength);
            }
        }
        public int BytesFollowingFirstPacket
        {
            get
            {
                return ItemLength - PayloadInFirstPacket;
            }
        }
        public int FullPacketsFollowingFirst
        {
            get
            {
                return BytesFollowingFirstPacket / TimelineRegionList.PacketPayloadSize;
            }
        }
        public int BytesRemainingAfterFullPackets
        {
            get
            {
                return BytesFollowingFirstPacket % TimelineRegionList.PacketPayloadSize;
            }
        }
        public int AdaptationFieldLength
        {
            get
            {
                return TimelineRegionList.PacketPayloadSize - BytesRemainingAfterFullPackets;
            }
        }
        public int PacketCount
        {
            get
            {
                return FullPacketsFollowingFirst + ((PayloadInFirstPacket > 0) ? 1 : 0) + ((BytesRemainingAfterFullPackets > 0) ? 1 : 0); 
            }
        }
        public int TotalBytesToSend
        {
            get
            {
                return PacketCount * TimelineRegionList.PacketSize;
            }
        }

        public long ExpectedEndPacket
        {
            get
            {
                //       ms per second (1000)            subtitle time in ms
                //    ---------------------------- = ---------------------------
                //     bytes per second (bps / 8)         bytes into stream
                //
                //     ExpectedEndPacket = bytes into stream / Packet Size (188 bytes)
                return ((long)Milliseconds * (long)TimelineRegionList.ByteRate / (long)1000) / (long)TimelineRegionList.PacketSize;
            }
        }

        public long ExpectedStartPacket
        {
            get
            {
                // ExpectedStartPacket = ExpectedEndPacket - PacketCount + 1
                return ExpectedEndPacket - (long)PacketCount + 1;
            }
        }        

        public bool Overlaps(TimelineRegion region)
        {
            // Simple check, see if the specified packet start is within this packet or if the packet end is.
            if ((region.PacketStart >= PacketStart && region.PacketStart <= PacketEnd) ||
               (region.PacketEnd >= PacketStart && region.PacketEnd <= PacketEnd))
                return true;
            return false;
        }

        public bool Overlaps(long startPacket, long endPacket)
        {
            // Simple check, see if the specified packet start is within this packet or if the packet end is.
            if ((startPacket >= PacketStart && startPacket <= PacketEnd) ||
               (endPacket >= PacketStart && endPacket <= PacketEnd))
                return true;
            return false;
        }

        public bool Contains(long packetNumber)
        {
            if (packetNumber >= PacketStart && packetNumber <= PacketEnd)
                return true;
            return false;
        }

        public static bool operator <(TimelineRegion a, TimelineRegion b)
        {
            if (a.PacketEnd < b.PacketStart)
                return true;
            return false;
        }

        public static bool operator >(TimelineRegion a, TimelineRegion b)
        {
            if (a.PacketStart > b.PacketEnd)
                return true;
            return false;
        }

        public void DumpCSV(StreamWriter writer)
        {
            writer.Write(PacketStart.ToString() + "," + PacketEnd.ToString() + "," + ExpectedStartPacket.ToString() + "," + ExpectedEndPacket.ToString());
            writer.Write("," + Milliseconds.ToString() + "," + PresentationTimeStamp.ToString() + "," + ItemLength.ToString() + ",");
            writer.WriteLine(TotalBytesToSend.ToString() + "," + SourceFile + "," + SourceFileOffset.ToString() + "," + (GeneratePTSPacket ? "true" : "false") +"," + (GeneratePESPadding ? "true" : "false"));
        }

        TransportPacket GeneratePTSPadding(long InitialPTS, ushort PID, ref int ContinuityCounter)
        {
            // Stuffing bytes = 188 - (packet header length - pes header length - 4 bytes of padding)

            int stuffingBytes = 188 - (TimelineRegionList.PESHeaderSize + TimelineRegionList.PacketHeaderSize + 12);

            long timeStamp = PresentationTimeStamp + InitialPTS;

            ByteArray transportData = new ByteArray();

            transportData.MaxSize = 188;

            //------------------------------
            // Transport Packet Header
            //------------------------------
            // sync_byte = 0x47					(8 bits)
            transportData.append((byte)0x47);
            transportData.enterBitMode();
            // transport_error_indicator		(1 bit)
            transportData.appendBit(0);
            // payload_unit_start_indicator		(1 bit)
            transportData.appendBit((byte)(1));
            // transport_priority				(1 bit)
            transportData.appendBit(0);
            // PID								(13 bits)
            transportData.appendBits(PID, 12, 0);
            // transport_scrambling_code		(2 bits)
            transportData.appendBits((byte)0x0, 1, 0);
            // adaptation_field_control			(2 bits)
            transportData.appendBits((byte)((stuffingBytes > 0) ? 0x3 : 0x1), 1, 0);
            // continuity_counter				(4 bits)
            transportData.appendBits(ContinuityCounter, 3, 0);
            ContinuityCounter++;
            transportData.leaveBitMode();

            long used = 4;
            if (stuffingBytes > 0)
            {
                // Adaptation field for stuffing
                stuffingBytes--;
                used++;
                transportData.append((byte)stuffingBytes);	// adaptation_field_length
                if (stuffingBytes > 0)
                {
                    used += stuffingBytes;
                    stuffingBytes--;
                    transportData.append((byte)0x00);	// flags field

                    for (int k = 0; k < stuffingBytes; k++)
                        transportData.append((byte)0xFF);	// stuffing byte
                }
            }

            //-------------------------------
            // PES Packet Header
            //-------------------------------
            // packet_start_code_prefix			(24 bits)	(24)
            // stream_id						(8 bits)	(32)
            transportData.append((uint)0x000001BD);
            long packetLengthPosition = transportData.length;

            ushort packetLength = (ushort)(20);
            // Fill with dummy value
            transportData.append(packetLength);

            // '10'								(2 bits)	(02)	0x8000
            // PES_scrambling_code				(2 bits)	(04)	0x0000
            // PES_priority						(1 bit)		(05)	0x0000
            // data_alignment_indicator			(1 bit)		(06)	0x0400
            // copyright						(1 bit)		(07)	0x0000
            // original_or_copy					(1 bit)		(08)	0x0000
            // PTS_DTS_flags = '10'				(2 bits)	(10)	0x0080
            // ESCR_flag						(1 bit)		(11)	0x0000
            // ES_rate_flag						(1 bit)		(12)	0x0000
            // DSM_trick_mode_flag				(1 bit)		(13)	0x0000
            // additional_copy_info				(1 bit)		(14)	0x0000
            // PES_CRC_flag						(1 bit)		(15)	0x0000
            // PES_extension_flag				(1 bit)		(16)	0x0000
            //														0x8480
            transportData.append((ushort)0x8480);

            // PES_header_data_length = 0x05	(8 bits)	(08)
            transportData.append((byte)0x05);

            //   '0010'							(4 bits)	(76)
            //   PTS[32..30]					(3 bits)	(79)
            //   marker_bit						(1 bit)		(80)
            //   PTS[29..15]					(15 bits)	(95)
            //   marker_bit						(1 bit)		(96)
            //   PTS[14..0]						(15 bits)	(111)
            //   marker_bit						(1 bit)		(112)
            transportData.enterBitMode();
            transportData.appendBits((byte)0x2, 3, 0);
            transportData.appendBits(timeStamp, 32, 30);
            transportData.appendBit(1);
            transportData.appendBits(timeStamp, 29, 15);
            transportData.appendBit(1);
            transportData.appendBits(timeStamp, 14, 0);
            transportData.appendBit(1);
            transportData.leaveBitMode();

            used += 14;

            transportData.append((byte)0x20);           // data_identifier
            transportData.append((byte)0x00);           // subtitle_stream_id
            transportData.append((byte)0x0F);           //   sync_byte
            transportData.append((byte)0xFF);//4        //   segment_type = stuffing
            transportData.append((byte)0x00);           //   page_id = 0x0002
            transportData.append((byte)0x02);           
            transportData.append((byte)0x00);           //   segment_length = 0x0003
            transportData.append((byte)0x03);//8
            transportData.append((byte)0xFF);           //     stuffing byte 1
            transportData.append((byte)0xFF);           //     stuffing byte 2
            transportData.append((byte)0xFF);           //     stuffing byte 3
            transportData.append((byte)0xFF);//12       // end_of_PES_data_field_marker

            long decoderStamp = (timeStamp * 300);

            //long insertTime = (long)(27000000 * 0.040);
            //long streamTime = decoderStamp - insertTime;

            long bytesIntoStream = (PacketStart - 1) * TimelineRegionList.PacketSize * 27000000 / TimelineRegionList.ByteRate;
            long streamTime = bytesIntoStream + (InitialPTS * 300);

            TransportPacket newPacket = new TransportPacket(transportData.buffer);
            newPacket.StreamTime = (ulong)streamTime;
            newPacket.DecoderStamp = (ulong)decoderStamp;
            newPacket.PID = PID;
            newPacket.IsPadding = true;

            return newPacket;
        }

        TransportPackets PacketizeFile(long InitialPTS, ushort PID, ref int ContinuityCounter)
        {
            TransportPackets result = new TransportPackets();

            FileStream inputFileStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
            BigEndianReader reader = new BigEndianReader(inputFileStream);

            long timeStamp = PresentationTimeStamp + InitialPTS;

            long bytesRemaining = ItemLength;

            int i = 0;
            while (bytesRemaining > 0)
            {
                long stuffingBytes = 0;
                int headerSize = (i == 0) ? 18 : 4;
                if (bytesRemaining < 188 - headerSize)
                    stuffingBytes = 188 - headerSize - bytesRemaining;

                ByteArray transportData = new ByteArray();
                transportData.MaxSize = 188;

                //------------------------------
                // Transport Packet Header
                //------------------------------
                // sync_byte = 0x47					(8 bits)
                transportData.append((byte)0x47);
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
                transportData.appendBits((byte)0x0, 1, 0);
                // adaptation_field_control			(2 bits)
                transportData.appendBits((byte)((stuffingBytes > 0) ? 0x3 : 0x1), 1, 0);
                // continuity_counter				(4 bits)
                transportData.appendBits(ContinuityCounter, 3, 0);
                ContinuityCounter++;
                transportData.leaveBitMode();

                long used = 4;
                if (stuffingBytes > 0)
                {
                    // Adaptation field for stuffing
                    stuffingBytes--;
                    used++;
                    transportData.append((byte)stuffingBytes);
                    if (stuffingBytes > 0)
                    {
                        used += stuffingBytes;
                        stuffingBytes--;
                        transportData.append((byte)0x00);	// flags field

                        for (int k = 0; k < stuffingBytes; k++)
                            transportData.append((byte)0xFF);	// stuffing byte
                    }
                    //else
                    //    throw new Exception("Adaptation Field length = 0");
                }

                if (i == 0)
                {
                    //-------------------------------
                    // PES Packet Header
                    //-------------------------------
                    // packet_start_code_prefix			(24 bits)	(24)
                    // stream_id						(8 bits)	(32)
                    transportData.append((uint)0x000001BD);
                    long packetLengthPosition = transportData.length;

                    ushort packetLength = (ushort)(ItemLength + 8);
                    // Fill with dummy value
                    transportData.append(packetLength);

                    // '10'								(2 bits)	(02)	0x8000
                    // PES_scrambling_code				(2 bits)	(04)	0x0000
                    // PES_priority						(1 bit)		(05)	0x0000
                    // data_alignment_indicator			(1 bit)		(06)	0x0400
                    // copyright						(1 bit)		(07)	0x0000
                    // original_or_copy					(1 bit)		(08)	0x0000
                    // PTS_DTS_flags = '10'				(2 bits)	(10)	0x0080
                    // ESCR_flag						(1 bit)		(11)	0x0000
                    // ES_rate_flag						(1 bit)		(12)	0x0000
                    // DSM_trick_mode_flag				(1 bit)		(13)	0x0000
                    // additional_copy_info				(1 bit)		(14)	0x0000
                    // PES_CRC_flag						(1 bit)		(15)	0x0000
                    // PES_extension_flag				(1 bit)		(16)	0x0000
                    //														0x8480
                    transportData.append((ushort)0x8480);

                    // PES_header_data_length = 0x05	(8 bits)	(08)
                    transportData.append((byte)0x05);

                    //   '0010'							(4 bits)	(76)
                    //   PTS[32..30]					(3 bits)	(79)
                    //   marker_bit						(1 bit)		(80)
                    //   PTS[29..15]					(15 bits)	(95)
                    //   marker_bit						(1 bit)		(96)
                    //   PTS[14..0]						(15 bits)	(111)
                    //   marker_bit						(1 bit)		(112)
                    transportData.enterBitMode();
                    transportData.appendBits((byte)0x2, 3, 0);
                    transportData.appendBits(timeStamp, 32, 30);
                    transportData.appendBit(1);
                    transportData.appendBits(timeStamp, 29, 15);
                    transportData.appendBit(1);
                    transportData.appendBits(timeStamp, 14, 0);
                    transportData.appendBit(1);
                    transportData.leaveBitMode();

                    used += 14;
                }

                int payloadLength = TimelineRegionList.PacketSize - (int)used;
                bytesRemaining -= payloadLength;
                for (int k = 0; k < payloadLength; k++)
                    transportData.append((byte)reader.ReadByte());
                
                long bytesIntoStream = ((PacketStart - 1) + i) * TimelineRegionList.PacketSize * 27000000 / TimelineRegionList.ByteRate;
                long streamTime = bytesIntoStream + (InitialPTS * 300);

                TransportPacket newPacket = new TransportPacket(transportData.buffer);
                newPacket.StreamTime = (ulong) streamTime;
                newPacket.DecoderStamp = (ulong) timeStamp * 300;
                newPacket.PID = PID;

                if (newPacket.StreamTime >= newPacket.DecoderStamp)
                    throw new Exception("Packet is scheduled for delivery after decoder time");

                result.AddPacket(newPacket);
                i++;
            }
            reader.Close();
            inputFileStream.Close();

            return result;
        }

        public TransportPackets Packetize(long initialPTS, ushort PID, ref int ContinuityCounter)
        {          
            if (GeneratePTSPacket)
            {
                TransportPackets result = new TransportPackets();
                result.AddPacket(GeneratePTSPadding(initialPTS, PID, ref ContinuityCounter));
                return result;
            }
            else
            {
                return PacketizeFile(initialPTS, PID, ref ContinuityCounter);
            }
        }
    }

    class TimelineRegionList : List<TimelineRegion>
    {
        public const int BitRate = 192000;
        public const int BitsPerByte = 8;
        public const int ByteRate = BitRate / BitsPerByte;
        public const int PacketSize = 188;
        public const int PacketHeaderSize = 4;
        public const int PacketPayloadSize = PacketSize - PacketHeaderSize;
        public const int PESHeaderSize = 14;
        public const int PESPacketPayloadSize = PacketPayloadSize - PESHeaderSize;

        int ContinuityCounter = 1;

        public TransportPackets ReadMorePackets(long InitialPTS, ushort PID)
        {
            TransportPackets result = new TransportPackets();
            if (Count == 0)
                return result;
                
            result.Append(this[0].Packetize(InitialPTS, PID, ref ContinuityCounter));
            RemoveAt(0);

            return result;
        }

        /// <summary>
        /// Evaluates the list an inserts new items with PTS stamps when the interval has been reached
        /// </summary>
        /// <param name="msInterval"></param>
        public void PadPresentationStamps(long msInterval)
        {
            int i = 0;
            while (i < (Count - 1))
            {
                TimelineRegion regionA = this[i];
                TimelineRegion regionB = this[i + 1];
                if ((regionA.Milliseconds + msInterval) < regionB.Milliseconds)
                {
                    TimelineRegion newRegion = new TimelineRegion();
                    newRegion.ItemLength = 12;
                    newRegion.GeneratePTSPacket = true;
                    newRegion.Milliseconds = regionA.Milliseconds + msInterval;
                    newRegion.PacketStart = newRegion.ExpectedStartPacket;
                    if (regionA.Overlaps(newRegion) || regionB.Overlaps(newRegion))
                        InsertItemAt(newRegion);
                    else
                        Insert(i + 1, newRegion);
                    continue;
                }
                i++;
            }
        }

        public void DumpLog(string fileName)
        {
            FileStream logStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(logStream);

            for (int i = 0; i < Count; i++)
            {
                TimelineRegion region = this[i];
                region.DumpCSV(writer);
            }

            writer.Close();
            logStream.Close();
        }

        public void AddItemAt(SubtitleItem item)
        {
            TimelineRegion newRegion = new TimelineRegion();
            newRegion.ItemLength = (int) item.Length;
            newRegion.SourceFile = item.fileName;
            newRegion.SourceFileOffset = item.StartOffset;
            newRegion.Milliseconds = item.PresentationTime;
            newRegion.PacketStart = newRegion.ExpectedStartPacket;

            if (Count == 0)
            {                
                Add(newRegion);
                return;
            }

            InsertItemAt(newRegion);
        }

        /// <summary>
        /// Returns the first region that has been found which overlaps the given region to be inserted
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        TimelineRegion overlappingRegion(TimelineRegion region)
        {
            for (int i = 0; i < Count; i++)
            {
                TimelineRegion current = this[i];
                if (current.Overlaps(region))
                    return current;                
            }
            return null;
        }

        /// <summary>
        /// Returns the first region that has been found which overlaps the given region to be inserted
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        TimelineRegion overlappingRegion(long startPacket, long endPacket)
        {
            for (int i = 0; i < Count; i++)
            {
                TimelineRegion current = this[i];
                if (current.Overlaps(startPacket, endPacket))
                    return current;
            }
            return null;
        }

        TimelineRegion regionAtPacket(long packetNumber)
        {
            for (int i = 0; i < Count; i++)
            {
                TimelineRegion current = this[i];
                if (current.Contains(packetNumber))
                    return current;
            }
            return null;
        }

        void MoveItemBack(TimelineRegion item, long newEndPacket)
        {
            long newStartPacket = newEndPacket - item.PacketCount + 1;
            TimelineRegion overlap = overlappingRegion(newStartPacket, newEndPacket);
            while (overlap != null)
            {
                MoveItemBack(overlap, newStartPacket - 1);
                //System.Diagnostics.Debug.WriteLine("Moving back recursively an item which was already there to fit a new item in");
                overlap = overlappingRegion(newStartPacket, newEndPacket);
            }

            item.PacketEnd = newEndPacket;
        }

        void InsertItemAt(TimelineRegion region)
        {
            // Detect an overlap with the current insert position
            TimelineRegion overlap = overlappingRegion(region);
            while(overlap != null)
            {
                // Found an overlap, try to correct it
                // If the region we found should present after this one, then we should shift our packet time back to compensate
                // for it, this will make it so that both packets are delivered in time.
                if (overlap.PresentationTimeStamp > region.PresentationTimeStamp)
                {
                    //System.Diagnostics.Debug.WriteLine("Moving current item back to compensate for a later item");
                    region.PacketEnd = overlap.PacketStart - 1;
                }
                else if (overlap.PresentationTimeStamp < region.PresentationTimeStamp)
                {
                    // If the region we found should present before the new region, then the overlapping region
                    // should be shifted back to make sure we can deliver this region in time.
                    System.Diagnostics.Debug.WriteLine("Moving back an item which was already there to fit a new item in");
                    MoveItemBack(overlap, region.PacketStart - 1);
                }
                else
                {
                    //throw new Exception("Don't know what to do when two packets have the same presentation time");
                    break;
                }

                overlap = overlappingRegion(region);
            }

            TimelineRegion regionBefore = this[0];
            for (int i = 1; i < Count; i++)
            {
                TimelineRegion regionAfter = this[i];
                if (region > regionBefore && region < regionAfter)
                {
                    Insert(i, region);
                    return;
                }
                regionBefore = regionAfter;
            }
            Add(region);
        }
    }

    class SubtitleItemList : List<SubtitleItem> 
    {
        public TimelineRegionList RegionList = new TimelineRegionList();

	    public SubtitleItemList(string fileName) : base()
	    {
            load(fileName);
	    }

	    void load(string fileName)
	    {
            FileInfo fileInfo = new FileInfo(fileName);
            if(!fileInfo.Exists)
                return;

            string filePath = fileInfo.Directory.FullName + @"\";

            FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(inputStream);

            SubtitleItem currentItem = new SubtitleItem();

            while(inputStream.Position < inputStream.Length)
            {
                string line = reader.ReadLine().Trim();

                if(line == string.Empty)
                    continue;

                if(line == "[Packet]")
                {
                    if(currentItem.IsValid)
                    {
                        RegionList.AddItemAt(currentItem);
                        Add(currentItem);
                        currentItem = new SubtitleItem();
                    }
                }
                else if(line.StartsWith("PresentationTime="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.PresentationTime = Convert.ToInt64(line.Substring(index));
                }
                else if(line.StartsWith("SourceFile="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.fileName = filePath + line.Substring(index);
                }
                else if(line.StartsWith("StartOffset="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.StartOffset = Convert.ToInt64(line.Substring(index));
                }
                else if(line.StartsWith("Length="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.Length = Convert.ToInt64(line.Substring(index));
                }
            }

            if (currentItem.IsValid)
            {
                RegionList.AddItemAt(currentItem);
                Add(currentItem);
            }

            RegionList.PadPresentationStamps(500);
            //RegionList.DumpLog("c:\\temp\\regionlist.csv");
	    }
    }


}
