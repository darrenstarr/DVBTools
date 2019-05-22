namespace DVBToolsCommon.MPEG
{
    // 6.2.2.1 Sequence header
    //sequence_header() {                         No. of bits     Mnemonic
    //    sequence_header_code                    32              bslbf             0   0xFFFF
    //    horizontal_size_value                   12              uimsbf            4   0xFFF0
    //    vertical_size_value                     12              uimsbf            5   0x0FFF
    //    aspect_ratio_information                4               uimsbf            7   0xF0
    //    frame_rate_code                         4               uimsbf            7   0x0F
    //    bit_rate_value                          18              uimsbf            8   0x00FFFFC0
    //    marker_bit                              1               bslbf             10  0x20
    //    vbv_buffer_size_value                   10              uimsbf            10  0x1FF8
    //    constrained_parameters_flag             1               bslbf             11  0x04
    //    load_intra_quantiser_matrix             1               uimsbf            11  0x02
    //    if ( load_intra_quantiser_matrix )
    //        intra_quantiser_matrix[64]          8*64            uimsbf
    //    load_non_intra_quantiser_matrix         1               uimsbf            X   0x01
    //    if ( load_non_intra_quantiser_matrix )
    //        non_intra_quantiser_matrix[64]      8*64            uimsbf
    //    next_start_code()
    //}
    public class SequenceHeader : VideoComponent
    {
        public int horizontalSizeValue;
        public int verticalSizeValue;
        public int aspectRatioInformation;
        public int frameRateCode;
        public int bitRateValue;
        public int vbvBufferSizeValue;
        public int constrainedParametersFlag;
        public int loadIntraQuantiserMatrix;
        public byte[] intraQuantiserMatrix;
        public int loadNonIntraQuantiserMatrix;
        public byte[] nonIntraQuantiserMatrix;

        public SequenceHeader()
            : base()
        {
        }

        public int Load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 15 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 14)
                return 0;

            int index = startIndex;

            horizontalSizeValue = Read16(buffer, index + 4) >> 4;
            verticalSizeValue = Read16(buffer, index + 5) & 0x0FFF;
            aspectRatioInformation = buffer[index + 7] >> 4;
            frameRateCode = buffer[index + 7] & 0x0F;
            bitRateValue = (int)(Read32(buffer, index + 8) >> 14);
            vbvBufferSizeValue = (Read16(buffer, index + 10) & 0x1FF8) >> 3;
            constrainedParametersFlag = (buffer[index + 11] & 0x04) >> 2;
            loadIntraQuantiserMatrix = (buffer[index + 11] & 0x02) >> 1;

            index += 11;

            if (loadIntraQuantiserMatrix == 1)
            {
                if ((bufferLength - index) < 68)
                    return 0;

                intraQuantiserMatrix = new byte[64];
                for (int i = 0; i < 64; i++)
                {
                    intraQuantiserMatrix[i] = (byte)(((buffer[index] & 0x1) << 7) | (buffer[index + 1] >> 1));
                    index++;
                }
            }

            loadNonIntraQuantiserMatrix = buffer[index++] & 0x1;

            if (loadNonIntraQuantiserMatrix == 1)
            {
                if ((bufferLength - index) < 68)
                    return 0;

                nonIntraQuantiserMatrix = new byte[64];
                for (int i = 0; i < 64; i++)
                    nonIntraQuantiserMatrix[i] = buffer[index++];
            }

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
