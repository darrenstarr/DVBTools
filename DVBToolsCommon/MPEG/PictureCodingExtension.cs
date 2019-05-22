namespace DVBToolsCommon.MPEG
{
    // 6.2.3.1 Picture coding extension
    //picture_coding_extension() {                    No . of bits    Mnemonic
    //    extension_start_code                        32              bslbf         0   0xFFFF
    //    extension_start_code_identifier             4               uimsbf        4   0xF0
    //    f_code[0][0] /* forward horizontal */       4               uimsbf        4   0x0F
    //    f_code[0][1] /* forward vertical */         4               uimsbf        5   0xF0
    //    f_code[1][0] /* backward horizontal */      4               uimsbf        5   0x0F
    //    f_code[1][1] /* backward vertical */        4               uimsbf        6   0xF0
    //    intra_dc_precision                          2               uimsbf        6   0x0C
    //    picture_structure                           2               uimsbf        6   0x03
    //    top_field_first                             1               uimsbf        7   0x80
    //    frame_pred_frame_dct                        1               uimsbf        7   0x40
    //    concealment_motion_vectors                  1               uimsbf        7   0x20
    //    q_scale_type                                1               uimsbf        7   0x10
    //    intra_vlc_format                            1               uimsbf        7   0x08
    //    alternate_scan                              1               uimsbf        7   0x04
    //    repeat_first_field                          1               uimsbf        7   0x02
    //    chroma_420_type                             1               uimsbf        7   0x01
    //    progressive_frame                           1               uimsbf        8   0x80
    //    composite_display_flag                      1               uimsbf        8   0x40
    //    if ( composite_display_flag ) {
    //        v_axis                                  1               uimsbf        8   0x20
    //        field_sequence                          3               uimsbf        8   0x1C
    //        sub_carrier                             1               uimsbf        8   0x02
    //        burst_amplitude                         7               uimsbf        8   0x01FC
    //        sub_carrier_phase                       8               uimsbf        9   0x03FC
    //    }
    //    next_start_code()
    //}
    public class PictureCodingExtension : Extension
    {
        public enum PictureStructure
        {
            Reserved = 0,
            TopField = 1,
            BottomField = 2,
            FramePicture = 3
        }

        public int extensionStartCodeIdentifier;
        public int[] fCode = new int[4];
        public int intraDCPrecision;
        public PictureStructure pictureStructure;
        public int topFieldFirst;
        public int framePredFrameDCT;
        public int concealmentMotionVectors;
        public int qScaleType;
        public int intraVLCFormat;
        public int alternateScan;
        public int repeatFirstField;
        public int chroma420Type;
        public int progressiveFrame;
        public int compositeDisplayFlag;
        public int vAxis;
        public int fieldSequence;
        public int subCarrier;
        public int burstAmplitude;
        public int subCarrierPhase;

        public int IntraDCPrecision
        {
            get
            {
                return intraDCPrecision + 8;
            }
        }

        public int Frame
        {
            get
            {
                return fieldSequence >> 1;
            }
        }

        public int Field
        {
            get
            {
                return fieldSequence;
            }
        }

        public PictureCodingExtension()
            : base()
        {
        }

        public int Load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 14 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 14)
                return 0;

            int index = startIndex;

            index += 4;
            extensionStartCodeIdentifier = buffer[index] >> 4;
            fCode[0] = buffer[index++] & 0x0F;
            fCode[1] = buffer[index] >> 4;
            fCode[2] = buffer[index++] & 0x0F;
            fCode[3] = buffer[index] >> 4;
            intraDCPrecision = (buffer[index] & 0xC) >> 2;
            pictureStructure = (PictureStructure)(buffer[index++] & 0x3);
            topFieldFirst = buffer[index] >> 7;
            framePredFrameDCT = (buffer[index] & 0x40) >> 6;
            concealmentMotionVectors = (buffer[index] & 0x20) >> 5;
            qScaleType = (buffer[index] & 0x10) >> 4;
            intraVLCFormat = (buffer[index] & 0x08) >> 3;
            alternateScan = (buffer[index] & 0x04) >> 2;
            repeatFirstField = (buffer[index] & 0x02) >> 1;
            chroma420Type = buffer[index++] & 0x1;
            progressiveFrame = buffer[index] >> 7;
            compositeDisplayFlag = (buffer[index] & 0x40) >> 6;

            if (compositeDisplayFlag == 1)
            {
                vAxis = (buffer[index] & 0x20) >> 5;
                fieldSequence = (buffer[index] & 0x1C) >> 2;
                subCarrier = (buffer[index] & 0x02) >> 1;
                burstAmplitude = (Read16(buffer, index) & 0x1FC) >> 2;
                index++;
                subCarrierPhase = (Read16(buffer, index) & 0x3FC) >> 2;
            }
            index++;

            while (index < (bufferLength - 4))
            {
                if ((Read32(buffer, index) >> 8) == 1)
                    return index - startIndex;

                index++;
            }

            return 0;
        }
    }
}
