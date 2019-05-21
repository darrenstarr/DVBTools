namespace TransportMux
{
    using DVBToolsCommon;
    using System;
    using System.IO;

    public class MPEG2VideoStream : InputStream    
    {                
        /// <summary>
        /// Enables quad-byte padding of the last slice of a picture
        /// </summary>
        /// The CableLabs VoD spec requires that in order to support older Motorola set-top boxes,
        /// the last slice in a picture should be padded with zeros until the length of the multiplexed
        /// slice is evenly divisible by 4.
        public bool QuadByteAlign = true;

        public uint vbvBufferSizeValue = 0;
        public uint vbvBufferSizeExtension = 0;
        public uint VBVBufferSize
        {
            get
            {
                return (vbvBufferSizeValue | (vbvBufferSizeExtension << 10)) * 2 * 1024;
            }
        }

        public uint bitRateValue = 0;
        public uint bitRateExtension = 0;
        public uint BitRate
        {
            get
            {
                return (bitRateValue | (bitRateExtension << 18)) * 400;
            }
        }
        public uint ByteRate
        {
            get
            {
                return BitRate / 8;
            }
        }

        public double VBVDelay = 0;

        public double PictureVBVDelay
        {
            get
            {
                if (pictureVBVDelay == 0xFFFF)
                    return 0;

                return (double)pictureVBVDelay / 90000;
            }
        }

        public double AdjustedVBVDelay
        {
            get
            {
                return (double)VBVBufferSize / ((double)ByteRate * 1.10);
            }
        }

        public uint BytesPerPCRMax
        {
            get
            {
                return ByteRate / 10 - 184;     // 1/10th of a second minus a packet payload
            }
        }

        public const long MaximumBitRate = 15000000;

        public const byte MPEG_PICTURE_STRUCTURE_TOP_FIELD = 0x01;
        public const byte MPEG_PICTURE_STRUCTURE_BOTTOM_FIELD = 0x02;
        public const byte MPEG_PICTURE_STRUCTURE_FRAME_PICTURE = 0x03;

        public const byte MPEG_SCALABILITY_DATA_PARTITIONING = 0x00;
        public const byte MPEG_SCALABILITY_SPATIAL = 0x01;
        public const byte MPEG_SCALABILITY_SNR = 0x02;
        public const byte MPEG_SCALABILITY_TEMPORAL = 0x03;

        public const uint MPEG_CODE_PICTURE_START = 0x00000100;
        public const uint MPEG_CODE_SLICE_START_LOW = 0x00000101;
        public const uint MPEG_CODE_SLICE_START_HIGH = 0x000001AF;
        public const uint MPEG_CODE_USER_DATA_START = 0x000001B2;
        public const uint MPEG_CODE_SEQUENCE_HEADER_START = 0x000001B3;
        public const uint MPEG_CODE_SEQUENCE_ERROR_START = 0x000001B4;
        public const uint MPEG_CODE_EXTENSION_START = 0x000001B5;
        public const uint MPEG_CODE_SEQUENCE_END = 0x000001B7;
        public const uint MPEG_CODE_GROUP_START	= 0x000001B8;

        public const byte MPEG_EXTENSION_SEQUENCE = 0x01;
        public const byte MPEG_EXTENSION_SEQUENCE_DISPLAY = 0x02;
        public const byte MPEG_EXTENSION_QUANTIZER_MATRIX = 0x03;
        public const byte MPEG_EXTENSION_COPYRIGHT = 0x04;
        public const byte MPEG_EXTENSION_SEQUENCE_SCALABLE = 0x05;
        public const byte MPEG_EXTENSION_PICTURE_DISPLAY = 0x07;
        public const byte MPEG_EXTENSION_PICTURE_CODING = 0x08;
        public const byte MPEG_EXTENSION_PICTURE_SPATIAL = 0x09;
        public const byte MPEG_EXTENSION_PICTURE_TEMPORAL = 0x0A;

        public enum ErrorCode
        {
            MPEG_ERROR_NONE,
            MPEG_ERROR_NOSEQUENCE,
            MPEG_ERROR_BUFFER_OVERFLOW
        }

        private FileStream inputFile;
        private BigEndianReader reader;

        public double InitialDTS = 0.22919;

        public double SequenceFrameRate;
	    public int SequenceFrames;
	
	    public int TemporalReference;
	    public int PictureCodingType;
	    public bool ProgressiveSequence;			// sequence extension
	    public bool TopFieldFirst;
	    public bool RepeatFirstField;
	    public byte PictureStructure;
	
        public byte [] buffer;
	    public long bufferSize;
	    public long bufferLength;

        public ErrorCode LastError = ErrorCode.MPEG_ERROR_NONE;
        public MPEGGOPTimeCode GopTimeCode = new MPEGGOPTimeCode();
        public long CurrentFrame = 0;
	    public long SequenceStartFrame;

	    public MPEGFrames Frames = new MPEGFrames();
	    public int CurrentOutputFrame;
	    public int FrameStartIndex;
        private TransportPackets TransportPackets = new TransportPackets();
        private bool firstPacket = true;
        private int pictureVBVDelay = 0;

        private void BufferMore()
        {
            if (TransportPackets.Count == 0)
            {
                while (inputFile.Position < inputFile.Length && TransportPackets.Count < (1000000 / 188))
                {
                    if (!ReadNextSequence())
                    {
                        return;
                    }

                    Packetize(InitialDTS);
                }
            }
        }

        private ushort Pid;
        public byte ContinuityIndicator = 0;

        public delegate void MPEGTimeCodeEncountered(string time);
        public MPEGTimeCodeEncountered MPEGTimeCodeEncounteredEvent;
        private long streamLength = 0;
        public override long StreamLength
        {
            get
            {
                return streamLength;
            }
        }

        public override long Position
        {
            get
            {
                if (inputFile == null )
                    return streamLength;
                return inputFile.Position;
            }
        }

        public MPEG2VideoStream(string fileName)
        {
            inputFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            streamLength = inputFile.Length;
            reader = new BigEndianReader(inputFile);

            AllocateInitialBuffer();
            BufferMore();
        }

        ~MPEG2VideoStream()
        {
            Close();
        }

        public override void Close()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }

            if (inputFile != null)
            {
                inputFile.Close();
                inputFile = null;
            }
        }

        private void AllocateInitialBuffer()
        {
            bufferSize = MaximumBitRate / 8;
            buffer = new byte[bufferSize];
            bufferLength = 0;
        }

        private bool ReadNextSequence()
        {
	        bufferLength = 0;
            Frames.Clear();

	        SequenceFrames = 0;
	        SequenceStartFrame = CurrentFrame;
        	
	        if(!FindNextSequenceStart())
	        {
                LastError = ErrorCode.MPEG_ERROR_NOSEQUENCE;
		        return false;
	        }

	        int picture = 0;
	        MPEGFrame newFrame = new MPEGFrame();
	        newFrame.StartIndex = bufferLength - 4;	// -4 compensates for sequence start code
	        newFrame.FrameNumber = CurrentFrame++;

	        if(!ConsumeSequenceHeader())
		        return false;

	        if(!ConsumeExtensions(0))
		        return false;

	        int pictures = 0;
	        bool done = false;
	        while(!done)
	        {
		        uint startCode = reader.Peek32();
                if (startCode == MPEG_CODE_GROUP_START)
                {
                    newFrame.StartOfGOP = true;
                    ConsumeGroupOfPicturesHeader();
                    ConsumeExtensions(1);
                }
                else if (startCode >= MPEG_CODE_SLICE_START_LOW && startCode <= MPEG_CODE_SLICE_START_HIGH)
                    ConsumeSlice();
                else if (startCode == MPEG_CODE_PICTURE_START)
                {
                    picture++;
                    if (picture > 1)
                    {
                        newFrame.EndIndex = bufferLength - 1;
                        Frames.Add(newFrame);
                        newFrame = new MPEGFrame();
                        newFrame.StartIndex = bufferLength;
                        newFrame.FrameNumber = CurrentFrame++;
                    }
                    pictures++;
                    ConsumePictureHeader();

                    if (PictureCodingType == 1)
                        newFrame.FrameType = 'I';
                    else if (PictureCodingType == 2)
                        newFrame.FrameType = 'P';
                    else if (PictureCodingType == 3)
                        newFrame.FrameType = 'B';

                    newFrame.TemporalReference = TemporalReference;
                    newFrame.PresentationNumber = SequenceStartFrame + TemporalReference;
                    newFrame.VBVDelay = PictureVBVDelay;
                }
                else if (startCode == MPEG_CODE_EXTENSION_START)
                    ConsumeExtensions(2);
                else if (startCode == MPEG_CODE_SEQUENCE_END)
                {
                    // consume sequence_end_code
                    ReadAndBuffer32();
                    done = true;
                }
                else
                    done = true;
	        }

	        newFrame.EndIndex = bufferLength - 1;
	        Frames.Add(newFrame);

	        return true;
        }

        private bool FindNextSequenceStart()
        {
	        uint currentStartCode = ReadAndBuffer32();
            long position = reader.Position;
            long length = reader.Length;

            while(position < length && currentStartCode != 0x000001B3)
	        {
		        currentStartCode <<= 8;
		        currentStartCode |= (uint) ReadAndBuffer8();
                position++;
	        }

	        if(currentStartCode == 0x000001B3)
		        return true;

	        return false;
        }

        private bool ConsumeSequenceHeader()
        {
	        TopFieldFirst = false;
	        RepeatFirstField = false;
	        ProgressiveSequence = false;

	        // horizontal_size_value (12 bits) + vertical_size_value (12 bits)
	        ReadAndBuffer24();
        	
	        // aspect_ratio_information (4 bits) + frame_rate_code (4 bits)
	        byte temp = ReadAndBuffer8();
	        SequenceFrameRate = FrameRateFromCode(temp & 0x0f);

	        // bit_rate_value (18 bits) + marker_bit (1 bit) + vbv_buffer_size_value (10 bits) +
	        // constrained parameters_flag (1 bit) + load_intra_quantizer_matrix (1 bit) = 31 bits
	        uint temp32 = ReadAndBuffer32();
            bitRateValue = (temp32 >> 14) & 0x3FFFF;
            vbvBufferSizeValue = (temp32 >> 3) & 0x3FF;

	        if((temp32 & 0x00000002) == 0x00000002)		// load_intra_quantizer_matrix
	        {
                for (int i = 0; i < 63; i++)
                    ReadAndBuffer8();

		        temp32 = ReadAndBuffer8();
	        }
	        if((temp32 & 0x00000001) == 0x00000001)
	        {
		        for(int i=0; i<16; i++)
			        ReadAndBuffer32();
	        }

	        return NextStartCode();
        }

        private bool ConsumeExtensions(int mode)
        {
            var startCode = reader.Peek32();
	        while((mode != 1 && startCode == MPEG_CODE_EXTENSION_START) || startCode == MPEG_CODE_USER_DATA_START)
	        {
		        // extension_start_code (32 bits)
		        ReadAndBuffer32();

                if (mode != 1 && startCode == MPEG_CODE_EXTENSION_START)
                {
                    // extension_start_code_identifier (4 bits) and top nibble of next part of extension
                    byte temp = reader.ReadByte();

                    switch (temp >> 4)
                    {
                        case MPEG_EXTENSION_SEQUENCE:
                            if (!ConsumeSequenceExtension(temp))
                                return false;

                            break;

                        case MPEG_EXTENSION_SEQUENCE_DISPLAY:
                            if (!ConsumeSequenceDisplayExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_SEQUENCE_SCALABLE:
                            if (!ConsumeSequenceScalableExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_PICTURE_CODING:
                            if (!ConsumePicturecodingExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_QUANTIZER_MATRIX:
                            if (!ConsumeQuantizerMatrixExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_PICTURE_DISPLAY:
                            if (!ConsumePictureDisplayExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_PICTURE_SPATIAL:
                            if (!ConsumePictureSpatialScalableExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_PICTURE_TEMPORAL:
                            if (!ConsumePictureTemporalScalableExtension(temp))
                                return false;
                            break;

                        case MPEG_EXTENSION_COPYRIGHT:
                            if (!ConsumeCopyrightExtension(temp))
                                return false;
                            break;
                    }
                }
                else // startCode == MPEG_CODE_USER_DATA_START
                {
                    if (!ConsumeUserData())
                        return false;                    
                }
		        NextStartCode();
                startCode = reader.Peek32();
            }

	        return NextStartCode();
        }

        private bool ConsumeSequenceExtension(byte previousByte)
        {
	        // extension_start_code_identifier			(4 bits)	04
	        // top nibble of extension_start_code		(4 bits)	08
	        Append8(previousByte);

	        // bottom nibble of extension_start_code	(4 bits)	04
	        // progressive_sequence						(1 bit)		05
	        // chroma_format							(2 bits)	07
	        // horizontal_size_extension				(2 bits)	09
	        // vertical_size_extension					(2 bits)	11
	        // bit_rate_extension						(12 bits)	23
	        // marker_bit								(1 bit)		24
	        // vbv_buffer_size_extension				(8 bits)	32
	        uint temp = ReadAndBuffer32();
            bitRateExtension = (temp >> 9) & 0xFFF;
            vbvBufferSizeExtension = temp & 0xFF;

	        ProgressiveSequence = (temp & 0x08000000) == 0x08000000 ? true : false;

	        // low_delay								(1 bit)		01
	        // frame_rate_extension_n					(2 bits)	03
	        // frame_rate_extension_d					(5 bits)	08
	        ReadAndBuffer8();

	        return true;
        }

        private bool ConsumeSequenceDisplayExtension(byte previousByte)
        {
            // extension_start_code_identifier			(4 bits)	04
            // video_format								(3 bits)	07
            // colour_description						(1 bit)		08
            Append8(previousByte);

            // if(colour_description)
            if ((previousByte & 0x01) == 0x01)
            {
                // colour_primaries						(8 bits)	08
                // transfer_characteristics				(8 bits)	16
                // matrix_coefficients					(8 bits)	24
                ReadAndBuffer24();
            }

            // display_horizontal_size					(14 bits)	14
            // marker_bit								(1 bit)		15
            // display_vertical_size					(14 bits)	29
            ReadAndBuffer32();

            return true;
        }

        private bool ConsumeSequenceScalableExtension(byte previousByte)
        {
	        // extension_start_code_identifier			(4 bits)	04
	        // scalable_mode							(2 bits)	06
	        // layer_id (top 2 bits)					(2 bits)	08
	        Append8(previousByte);

	        byte scalable_mode = (byte)((previousByte >> 2) & 0x03);

	        if(scalable_mode == MPEG_SCALABILITY_SPATIAL)
	        {
		        // layer_id (bottom 2 bits)					(2 bits)	02
		        // lower_layer_prediction_horizontal_size	(14 bits)	16
		        // marker_bit								(1 bit)		17
		        // lower_layer_prediction_vertical_size		(14 bits)	31
		        // horizontal_subsampling_factor_m			(5 bits)	36
		        // horizontal_subsampling_factor_n			(5 bits)	41
		        // vertical_subsampling_factor_m			(5 bits)	46
		        // vertical_subsampling_factor_n			(5 bits)	51
		        // -- Round to 7 bytes (56 bits)
		        ReadAndBuffer32();
		        ReadAndBuffer24();
	        }
	        else if(scalable_mode == MPEG_SCALABILITY_TEMPORAL)
	        {
		        // layer_id (bottom 2 bits)					(2 bits)	02
		        // picture_mux_enable						(1 bit)		03
		        // if(picture_mux_enable)	// Ignore since it won't alter the length to mux
		        //   mux_to_progressive_sequence			(1 bit)		04
		        // picture_mux_order						(3 bits)	07
		        // picture_mux_factor						(3 bits)	10
		        // -- round to 2 bytes (16 bits)
		        ReadAndBuffer16();
	        }
	        else
	        {
		        // layer_id (bottom 2 bits)					(2 bits)	02
		        // -- round to 1 byte (8 bits)
		        ReadAndBuffer8();
	        }

	        return true;
        }

        private bool ConsumePicturecodingExtension(byte previousByte)
        {
	        // extension_start_code_identifier			(4 bits)	04
	        // f_code[0][0] - forward horizontal		(4 bits)	08
	        Append8(previousByte);

	        // f_code[0][1] - forward vertical			(4 bits)	04
	        // f_code[1][0] - backward horizontal		(4 bits)	08
	        // f_code[1][1] - backward vertical			(4 bits)	12
	        // intra_dc_precision						(2 bits)	14
	        // picture_structure						(2 bits)	16
	        // top_field_first							(1 bit)		17
	        // frame_pred_frame_dct						(1 bit)		18
	        // concealment_motion_vectors				(1 bit)		19
	        // q_scale_type								(1 bit)		20
	        // intra_vlc_format							(1 bit)		21
	        // alternate_scan							(1 bit)		22
	        // repeat_first_field						(1 bit)		23
	        // chroma_420_type							(1 bit)		24
	        uint temp32 = ReadAndBuffer24();
	        TopFieldFirst = (temp32 & 0x000080) == 0x000080 ? true : false;
	        RepeatFirstField = (temp32 & 0x000002) == 0x000002 ? true : false;
	        PictureStructure = (byte)((temp32 >> 1) & 0x03);

            byte temp = reader.ReadByte();
	        if((temp & 0x40) == 0x40)
	        {
		        // progressive_frame					(1 bit)		01
		        // composite_display_flag				(1 bit)		02
		        // if(composite_display_flag)
		        // v_axis								(1 bit)		03
		        // field_sequence						(3 bits)	06
		        // sub_carrier							(1 bit)		07
		        // burst_amplitude						(7 bits)	14
		        // sub_carrier_phase					(8 bits)	24
		        Append8(temp);
		        ReadAndBuffer16();
	        }
	        else
	        {
		        // progressive_frame					(1 bit)		01
		        // composite_display_flag				(1 bit)		02
		        // - round to 1 byte (8 bits)
		        Append8(temp);
	        }

	        return true;
        }

        private bool ConsumeQuantizerMatrixExtension(byte previousByte)
        {
	        // extension_start_code_identifier			(4 bits)	04
	        // load_intra_quantiser_matrix				(1 bit)		05
	        Append8(previousByte);

	        byte temp = previousByte;
	        if((temp & 0x08) == 0x08)
	        {
		        // if(load_intra_quantiser_matrix)
		        //		intra_quantiser_matrix			(8*64 bits)	512
		        for(int i=0; i<63; i++)
			        ReadAndBuffer8();
		        temp = ReadAndBuffer8();
	        }

	        // load_non_intra_quantiser_matrix			(1 bit)		01
	        if((temp & 0x04) == 0x04)
	        {
		        // if(load_non_intra_quantiser_matrix)
		        //		non_intra_quantiser_matrix		(8*64 bits)	512
		        for(int i=0; i<63; i++)
                    ReadAndBuffer8();
		        temp = ReadAndBuffer8();
	        }

	        // load_chroma_intra_quantiser_matrix		(1 bit)		01
	        if((temp & 0x02) == 0x02)
	        {
		        // if(load_chroma_intra_quantiser_matrix)
		        //		chroma_intra_quantiser_matrix	(8*64 bits)	512
		        for(int i=0; i<63; i++)
			        ReadAndBuffer8();
		        temp = ReadAndBuffer8();
	        }

	        // load_chroma_non_intra_quantiser_matrix	(1 bit)		01
	        if((temp & 0x01) == 0x01)
	        {
		        // if(load_chroma_non_intra_quantiser_matrix)
		        //		chroma_non_intra_quantiser_matrix	(8*64 bits)	512
		        for(int i=0; i<16; i++)
			        ReadAndBuffer32();
	        }

	        return true;
        }

        private bool ConsumePictureDisplayExtension(byte previousByte)
        {
            int numberOfFrameCenterOffsets = 0;
            if (ProgressiveSequence)
            {
                if (RepeatFirstField)
                {
                    if (TopFieldFirst)
                        numberOfFrameCenterOffsets = 3;
                    else
                        numberOfFrameCenterOffsets = 2;
                }
                else
                    numberOfFrameCenterOffsets = 1;
            }
            else
            {
                if (PictureStructure == MPEG_PICTURE_STRUCTURE_TOP_FIELD || PictureStructure == MPEG_PICTURE_STRUCTURE_BOTTOM_FIELD)
                    numberOfFrameCenterOffsets = 1;
                else
                {
                    if (RepeatFirstField)
                        numberOfFrameCenterOffsets = 3;
                    else
                        numberOfFrameCenterOffsets = 2;
                }
            }

            // extension_start_code_identifier			(4 bits)	04
            // 4 bits left over, handled below
            Append8(previousByte);

            // for(i=0; i<number_of_frame_centre_offsets; i++)
            // {
            //   frame_centre_horizontal_offset			(16 bits)	16
            //   marker_bit								(1 bit)		17
            //   frame_center_vertical_offset			(16 bits)   33
            //   marker_bit								(1 bit)		34
            // }
            int bytesToConsume = ((34 * numberOfFrameCenterOffsets) - 4) / 8;
            for(int i=0; i<bytesToConsume; i++)
                ReadAndBuffer8();

            return true;
        }

        private bool ConsumePictureSpatialScalableExtension(byte previousByte)
        {
            // extension_start_code_identifier			(4 bits)	04
            // top 4 bits of lower_layer_temporal_reference	(4 bits)08
            Append8(previousByte);

            // bottom 6 bits of lower_layer_temporal_reference (6 bits)	06
            // marker_bit								(1 bit)		07
            // lower_layer_horizontal_offset			(15 bits)	22
            // marker_bit								(1 bit)		23
            // lower_layer_vertical_offset				(15 bits)	38
            // spatial_temporal_weight_code_table_index	(2 bits)	40
            // lower_layer_progressive_frame			(1 bit)		41
            // lower_layer_deinterlaced_field_select	(1 bit)		42
            ReadAndBuffer32();
            ReadAndBuffer16();

            return true;
        }

        private bool ConsumePictureTemporalScalableExtension(byte previousByte)
        {
            // extension_start_code_identifier			(4 bits)	04
            // reference_select_code					(2 bits)	06
            // top 2 bits of forward_temporal_reference	(2 bits)	08
            Append8(previousByte);

            // bottom 8 bits of forward_temporal_reference (8 bits)	08
            // marker_bit								(1 bit)		09
            // backward_temporal_reference				(10 bits)	19
            // -- round to 3 bytes or 24 bits
            ReadAndBuffer24();

            return true;
        }

        private bool ConsumeCopyrightExtension(byte previousByte)
        {
            // extension_start_code_identifier			(4 bits)	04
            // copyright_flag							(1 bit)		05
            // copyright_identifier (top 3 bits)		(3 bits)	08
            Append8(previousByte);

            // copyright_identifier (bottom 5 bits)		(5 bits)	05
            // original_or_copy							(1 bit)		06
            // reserved									(7 bits)	13
            // marker_bit								(1 bit)		14
            // copyright_number_1						(20 bits)	34
            // marker_bit								(1 bit)		35
            // copyright_number_2						(22 bits)	57
            // marker_bit								(1 bit)		58
            // copyright_number_3						(22 bits)	80
            ReadAndBuffer32();
            ReadAndBuffer32();
            ReadAndBuffer16();

            return true;
        }

        private bool ConsumeGroupOfPicturesHeader()
        {
	        // group_start_code			(32 bits)	(32)
	        ReadAndBuffer32();

	        // time_code				(25 bits)	(25)
	        // closed_gop				(1 bit)		(26)
	        // broken_link				(1 bit)		(27)
	        // - round to 4 bytes (32 bits) 
	        uint time_code  = ReadAndBuffer32();
	        GopTimeCode.DecodeFromGOPHeaderValue(time_code);

            //System.Diagnostics.Debug.WriteLine(GopTimeCode.ToString());
            if (MPEGTimeCodeEncounteredEvent != null)
            {
                MPEGTimeCodeEncounteredEvent(GopTimeCode.ToString());
            }

	        return NextStartCode();
        }

        private bool ConsumePictureHeader()
        {
	        // picture_start_code				(32 bits)	(32)
	        ReadAndBuffer32();

	        // temporal_reference				(10 bits)	(10)	>> 22
	        // picture_coding_type				(3 bits)	(13)	>> 19
	        // vbv_delay						(16 bits)	(29)	>> 3
	        // -- round to 4 bytes (32 bits)
	        uint temp32 = ReadAndBuffer32();

	        TemporalReference = (int)((temp32 >> 22) & 0x3FF);
	        PictureCodingType = (int)((temp32 >> 19) & 0x7);
	        byte pictureCodingType = (byte)((temp32 >> 19) & 0x7);
            pictureVBVDelay = (int)((temp32 >> 3) & 0xFFFF);

	        int bitsRemaining = 3;

	        // if(pictureCodingType == 2 || pictureCodingType == 3)	
	        //	full_pel_forward_vector			(1 bit)		(1)
	        //  forward_f_code					(3 bits)	(4)
	        if(pictureCodingType == 2 || pictureCodingType == 3)
		        bitsRemaining -= 4;

	        // if(pictureCodingType == 3)	
	        //	full_pel_backward_vector		(1 bit)		(1)
	        //  backward_f_code					(3 bits)	(4)
	        if(pictureCodingType == 3)
		        bitsRemaining -= 4;

	        while(bitsRemaining < 1)
	        {
		        bitsRemaining += 8;
		        temp32 <<= 8;
		        temp32 += ReadAndBuffer8();
	        }

            uint mask = (uint)(0x1 << (bitsRemaining - 1));

	        // while(nextbits() == '1')
	        //	extra_bit_picture ('1')			(1 bit)		(1)
	        //	extra_information_picture		(8 bits)	(9)
	        while((temp32 & mask) == mask)
	        {
		        bitsRemaining -= 9;

		        while(bitsRemaining < 1)
		        {
			        bitsRemaining += 8;
			        temp32 <<= 8;
			        temp32 += ReadAndBuffer8();
		        }
	        }

	        return NextStartCode();
        }

        private bool ConsumeSlice()
        {
            long sliceStarts = bufferLength;

            // slice_start_code			(32 bits)	(32)
            ReadAndBuffer32();            

            // There is a lot of information in a slice, however since this is not a decoder, it's not
            // that important to us. So, from this point, we just skip to the next start code

            bool result = NextStartCode();

            if (QuadByteAlign)
            {
                uint startCode = reader.Peek32();
                if (!(startCode >= MPEG_CODE_SLICE_START_LOW && startCode <= MPEG_CODE_SLICE_START_HIGH))
                {
                    long sliceEnds = bufferLength;
                    long sliceLength = sliceEnds - sliceStarts;

                    while ((sliceLength % 4) != 0)
                    {
                        Append8(0);
                        sliceLength++;
                    }
                }
            }

            return result;
        }

        private bool ConsumeUserData()
        {
            // user_data_start_code    (32 bits) (32)
            // Already consumed from consumeExtensions()
            //readAndBuffer32();

            return NextStartCode();
        }

        private bool NextStartCode()
        {
	        uint currentStartCode = ReadAndBuffer24();
	        while(currentStartCode != 0x00000001)
	        {
		        currentStartCode <<= 8;
		        currentStartCode &= 0x00FFFFFF;
		        currentStartCode |= (uint) ReadAndBuffer8();
	        }

	        if(currentStartCode == 0x00000001)
	        {
		        Unbuffer(3);                
		        reader.SeekRelative(-3);

                return true;
	        }

	        return false;
        }

        private double FrameRateFromCode(int code)
        {
            switch (code)
            {
                case 1:
                    return 23.976;
                case 2:
                    return 24;
                case 3:
                    return 25;
                case 4:
                    return 29.97;
                case 5:
                    return 30;
                case 6:
                    return 50;
                case 7:
                    return 59.94;
                case 8:
                    return 60;
            };
            return 0.0;
        }

        private bool Unbuffer(int count)
	    {
		    if(bufferLength > count)
			    bufferLength -= count;

		    return true;
	    }

        private bool Append8(byte value)
	    {
		    if(bufferLength >= bufferSize)
		    {
                LastError = ErrorCode.MPEG_ERROR_BUFFER_OVERFLOW;
			    return false;
		    }
		    buffer[bufferLength++] = value;
		    return true;
	    }

        private bool Append16(ushort value)
	    {
		    if((bufferLength + 1) >= bufferSize)
		    {
                LastError = ErrorCode.MPEG_ERROR_BUFFER_OVERFLOW;
			    return false;
		    }
		    buffer[bufferLength++] = (byte) ((value >> 8) & 0xFF);
		    buffer[bufferLength++] = (byte) (value & 0xFF);
		    return true;
	    }

        private bool Append24(uint value)
	    {
		    if((bufferLength + 2) >= bufferSize)
		    {
                LastError = ErrorCode.MPEG_ERROR_BUFFER_OVERFLOW;
			    return false;
		    }
		    buffer[bufferLength++] = (byte) ((value >> 16) & 0xFF);
		    buffer[bufferLength++] = (byte) ((value >> 8) & 0xFF);
		    buffer[bufferLength++] = (byte) (value & 0xFF);
		    return true;
	    }

        private bool Append32(uint value)
	    {
		    if((bufferLength + 3) >= bufferSize)
		    {
			    LastError = ErrorCode.MPEG_ERROR_BUFFER_OVERFLOW;
			    return false;
		    }
		    buffer[bufferLength++] = (byte) ((value >> 24) & 0xFF);
		    buffer[bufferLength++] = (byte) ((value >> 16) & 0xFF);
		    buffer[bufferLength++] = (byte) ((value >> 8) & 0xFF);
		    buffer[bufferLength++] = (byte) (value & 0xFF);
		    return true;
	    }

        private byte ReadAndBuffer8()
	    {
            byte result = reader.ReadByte();
		    Append8(result);
		    return result;
	    }

        private ushort ReadAndBuffer16()
	    {
            ushort result = reader.ReadUInt16();
		    Append16(result);
		    return result;
	    }

        private uint ReadAndBuffer24()
	    {
            uint result = reader.ReadUInt24();
		    Append24(result);
		    return result;
	    }

        private uint ReadAndBuffer32()
	    {
            uint result = reader.ReadUInt32();
		    Append32(result);
		    return result;
	    }

        private void DumpFrames()
        {
        }

        private void DumpTSPackets()
        {
        }

        private ByteArray PesForFrame(int mpegStream, int index, double initialDTS)
        {
            ByteArray result = new ByteArray();

	        MPEGFrame frame = Frames[index];

            result.Append((uint) (0x000001E0 + mpegStream));
            result.Append((ushort) 0x0000); // Transport stream PES packets don't need lengths if they're video

	        // '10'								(2 bits)	02		= 0x80
	        // PES_scrambling_control = '00'	(2 bits)	04		= 0x00
	        // PES_priority	= '0'				(1 bit)		05		= 0x00
	        // data_alignment indicator	= '1'	(1 bit)		06		= 0x04
	        // copyright = '0'					(1 bit)		07		= 0x00
	        // original_or_copy = '1'			(1 bit)		08		= 0x01
	        //														= 0x85
	        result.Append((byte) 0x85);

            if (frame.FrameNumber == 0)
                VBVDelay = frame.VBVDelay;
	        double frameDuration = 1.0 / SequenceFrameRate;
            double dts = (double)frame.FrameNumber * frameDuration + VBVDelay; // initialDTS;
            double pts = (double)(frame.PresentationNumber + 1) * frameDuration + VBVDelay; // initialDTS;
            if (frame.PresentationNumber == 0)
                InitialPTS = pts;

	        ulong intDts = (ulong) Math.Round(90000.0 * dts);
	        ulong intPts = (ulong) Math.Round(90000.0 * pts);

            //if ((double)intDts / 90000.0 != dts)
              //  System.Diagnostics.Debugger.Break();

	        if(dts == pts)
	        {
		        // PTS_DTS_flags = '10'				(2 bits)	02		= 0x80
		        // ESCR_flag = '0'					(1 bit)		03		= 0x00
		        // ES_rate_flag = '0'				(1 bit)		04		= 0x00
		        // DSM_trick_mode_flag = '0'		(1 bit)		05		= 0x00
		        // additional_copy_info_flag = '0'	(1 bit)		06		= 0x00
		        // PES_CRC_flag = '0'				(1 bit)		07		= 0x00
		        // PES_extension_flag				(1 bit)		08		= 0x00
		        //														= 0x80
                result.Append((byte) 0x80);

		        // if(PTS_DTS_flags == '10')
		        //	'0010'							(4 bits)	04		
		        //  PTS[32..30]						(3 bits)	07
		        //  marker_bit = '1'				(1 bit)		08
		        //  PTS[29..15]						(15 bits)	23
		        //  marker_bit = '1'				(1 bit)		24
		        //  PTS[14..0]						(15 bits)	39
		        //  marker_bit = '1'				(1 bit)		40
		        //	PES_header_data_length = 					40 bits or 5 bytes
                result.Append((byte) 5);

		        result.EnterBitMode();
		        result.AppendBits((byte) 0x2, 3, 0);
		        result.AppendBits(intPts, 32, 30);
		        result.AppendBit(1);
		        result.AppendBits(intPts, 29, 15);
		        result.AppendBit(1);
		        result.AppendBits(intPts, 14, 0);
		        result.AppendBit(1);
		        result.LeaveBitMode();
	        }
	        else
	        {
		        // PTS_DTS_flags = '11'				(2 bits)	02		= 0xC0
		        // ESCR_flag = '0'					(1 bit)		03		= 0x00
		        // ES_rate_flag = '0'				(1 bit)		04		= 0x00
		        // DSM_trick_mode_flag = '0'		(1 bit)		05		= 0x00
		        // additional_copy_info_flag = '0'	(1 bit)		06		= 0x00
		        // PES_CRC_flag = '0'				(1 bit)		07		= 0x00
		        // PES_extension_flag				(1 bit)		08		= 0x00
		        //														= 0xC0
		        result.Append((byte) 0xC0);

		        // if(PTS_DTS_flags == '11')
		        //	'0011'							(4 bits)	04		
		        //  PTS[32..30]						(3 bits)	07
		        //  marker_bit = '1'				(1 bit)		08
		        //  PTS[29..15]						(15 bits)	23
		        //  marker_bit = '1'				(1 bit)		24
		        //  PTS[14..0]						(15 bits)	39
		        //  marker_bit = '1'				(1 bit)		40
		        //	'0001'							(4 bits)	44		
		        //  DTS[32..30]						(3 bits)	47
		        //  marker_bit = '1'				(1 bit)		48
		        //  DTS[29..15]						(15 bits)	63
		        //  marker_bit = '1'				(1 bit)		64
		        //  DTS[14..0]						(15 bits)	79
		        //  marker_bit = '1'				(1 bit)		80
		        //	PES_header_data_length = 					80 bits or 10 bytes
		        result.Append((byte) 10);
		        result.EnterBitMode();
		        result.AppendBits((byte) 0x3, 3, 0);
		        result.AppendBits(intPts, 32, 30);
		        result.AppendBit(1);
		        result.AppendBits(intPts, 29, 15);
		        result.AppendBit(1);
		        result.AppendBits(intPts, 14, 0);
		        result.AppendBit(1);
		        result.AppendBits((byte) 0x1, 3, 0);
		        result.AppendBits(intDts, 32, 30);
		        result.AppendBit(1);
		        result.AppendBits(intDts, 29, 15);
		        result.AppendBit(1);
		        result.AppendBits(intDts, 14, 0);
		        result.AppendBit(1);
		        result.LeaveBitMode();
	        }

	        // Append the frame data
	        result.Append(buffer, frame.StartIndex, frame.Length);

            // TODO : figure out what the heck the quad-byte alignment thing is in Cablelabs VoD
            /*int pesHeaderSize = 14;     // just pts
            if (pts != dts)
                pesHeaderSize += 5;     // adjust for dts

            while (((result.length - pesHeaderSize) % 4) != 0)
                result.append((byte)0x00);*/

	        frame.PESPacket = result;
	        frame.DTS = dts;
	        frame.PTS = pts;

	        return result;
        }

        private void Packetize(double initialDTS)
        {
        	int added = 0;
            for(int i=0; i<Frames.Count; i++)
            {
                ByteArray pesData = PesForFrame(0, i, initialDTS);
                byte[] pes = pesData.buffer;

                double time = Frames[i].DTS - AdjustedVBVDelay;
                if (Frames[i].VBVDelay != 0)
                    time = Frames[i].DTS - Frames[i].VBVDelay;
                
		        if(time < 0)
			        time = 0;

		        ulong streamTime = (ulong) (27000000.0 * time);
                ulong decoderStamp = (ulong)(27000000.0 * Frames[i].DTS);

                long frameTransmissionTime = (long)27000000 * (long)(pesData.length) / (long)(ByteRate * 100 / 100); // * 112 / 100);
                long timeIncrement = frameTransmissionTime / ((pesData.length / 184) + 1);

                long lastPCR = 0;
		        long index = 0;
                int packetNumber = 0;
		        while(index < pesData.length)
		        {
			        TransportPacket newPacket = new TransportPacket();
                    newPacket.PID = PID;
                    newPacket.ContinuityIndicator = ContinuityIndicator;
                    ContinuityIndicator++;

			        if(index == 0)
			        {
                        newPacket.IsAligned = true;
                        newPacket.HasAdaptation = true;
                        newPacket.HasPCR = true;

                        if (firstPacket)
                        {
                            // MD-SP-VOD-CEP-I01040107 - Part 4.6 Transport Stream Requirements
                            // The first PCR packet of the stream must have the transport discontinuity_indicator flag set to 1.
                            newPacket.DiscontinuityIndicator = true;
                            firstPacket = false;
                        }

                        if (Frames[i].StartOfGOP)
                        {
                            // MD-SP-VOD-CEP-I01040107 - Part 4.6 Transport Stream Requirements
                            // Transport packet at the start of a GOP must have random_access_indicator set to 1.
                            newPacket.RandomAccess = true;
                        }
			        }

                    // This section forces a PCR stamp if the current frame is just so big that it will take longer than
                    // the maximum PCR interval to transmit.
                    if ((index - lastPCR) > BytesPerPCRMax)
                    {
                        newPacket.HasAdaptation = true;
                        newPacket.HasPCR = true;
                        lastPCR = index;
                    }

                    newPacket.DecoderStamp = decoderStamp;
                    newPacket.StreamTime = streamTime + ((ulong) timeIncrement * (ulong)packetNumber);
                    if (newPacket.StreamTime > decoderStamp)
                        throw new Exception("Buffer Overflow");

                    packetNumber++;

                    index += newPacket.Construct(pes, index, pesData.length);
			        added ++;

                    TransportPackets.AddPacket(newPacket);
		        }
        	}

	        return;
        }

        public override ushort PID
        {
	        get 
	        { 
		         return Pid;
	        }
            set
            {
                Pid = value;
                TransportPackets.ChangePID(value);
            }
        }

        public override ulong NextPacketTime
        {
	        get 
	        {
                if (TransportPackets.Count == 0)
                {
                    while (inputFile.Position < inputFile.Length && TransportPackets.Count < (1000000 / 188))
                    {
                        if (!ReadNextSequence())
                            return 0xFFFFFFFFFFFFFFFF;

                        Packetize(InitialDTS);
                    }
                }

                if (TransportPackets.Count == 0)
                    return 0xFFFFFFFFFFFFFFFF;

                return TransportPackets[0].StreamTime;
	        }
        }

        public override TransportPacket TakePacket()
        {
            if (TransportPackets.Count == 0)
            {
                while ((inputFile.Position < inputFile.Length) && TransportPackets.Count < (10000000 / 188))
                {
                    if (!ReadNextSequence())
                    {
                        if (TransportPackets.Count == 0)
                        {
                            return null;
                        }
                        break;
                    }

                    Packetize(InitialDTS);
                }
            }

            TransportPacket result = TransportPackets.TakeFirst();

            if (TransportPackets.Count == 0)
                BufferMore();

            return result;
        }

        public override bool  PCRStream
        {
	        get 
	        { 
		        return true;
	        }   
        }

        public override void  GenerateProgramMap(ByteArray Map)
        {
	        // stream_type = 0x02					(8 bits)
	        Map.AppendBits((byte) 0x02, 7, 0);

	        // reserved = '111b'					(3 bits)
	        Map.AppendBits((byte) 0x7, 2, 0);

	        // elementary_PID						(13 bits)
	        Map.AppendBits(PID, 12, 0);

	        // reserved = '1111b'					(4 bits)
	        Map.AppendBits((byte) 0xF, 3, 0);

	        // ES_info_length = 0x000				(12 bits)
	        Map.AppendBits((ushort) 0x000, 11, 0);
        }	    
    }
}
