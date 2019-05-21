using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // 6.2.2.3 Sequence extension
    //sequence_extension() {                  No. of bits     Mnemonic
    //    extension_start_code                32              bslbf             0   0xFFFF
    //    extension_start_code_identifier     4               uimsbf            4   0xF0
    //    profile_and_level_indication        8               uimsbf            4   0x0FF0
    //    progressive_sequence                1               uimsbf            5   0x08
    //    chroma_format                       2               uimsbf            5   0x06
    //    horizontal_size_extension           2               uimsbf            5   0x0180
    //    vertical_size_extension             2               uimsbf            6   0x60
    //    bit_rate_extension                  12              uimsbf            6   0x1FFE
    //    marker_bit                          1               bslbf             7   0x01
    //    vbv_buffer_size_extension           8               uimsbf            8   0xFF
    //    low_delay                           1               uimsbf            9   0x80
    //    frame_rate_extension_n              2               uimsbf            9   0x60
    //    frame_rate_extension_d              5               uimsbf            9   0x1F
    //    next_start_code()
    //}
    public class SequenceExtension : Extension
    {
        public enum ChromaFormat
        {
            Reserved = 0,
            FourTwoZero = 1,
            FourTwoTwo = 2,
            FourFourFour = 3
        }

        public int extensionStartCodeIdentifier;
        public int profileAndLevelIndication;
        public int progressiveSequence;
        public ChromaFormat chromaFormat;
        public int horizontalSizeExtension;
        public int verticalSizeExtension;
        public int bitRateExtension;
        public int vbvBufferSizeExtension;
        public int lowDelay;
        public int frameRateExtensionN;
        public int frameRateExtensionD;

        public SequenceExtension()
            : base()
        {
        }

        public int load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 14 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 14)
                return 0;

            int index = startIndex;

            extensionStartCodeIdentifier = buffer[index + 4] >> 4;
            profileAndLevelIndication = (read16(buffer, index + 4) & 0x0FF0) >> 4;
            progressiveSequence = (buffer[index + 5] & 0x08) >> 3;
            chromaFormat = (ChromaFormat)((buffer[index + 5] & 0x06) >> 1);
            horizontalSizeExtension = (read16(buffer, index + 5) & 0x0180) >> 7;
            verticalSizeExtension = (buffer[index + 6] & 0x60) >> 5;
            bitRateExtension = read16(buffer, index + 6) >> 1;
            vbvBufferSizeExtension = buffer[index + 8];
            lowDelay = buffer[index + 9] >> 7;
            frameRateExtensionN = (buffer[index + 9] & 0x60) >> 5;
            frameRateExtensionD = buffer[index + 9] & 0x1F;

            index += 10;

            while (index < (bufferLength - 4))
            {
                if ((read32(buffer, index) >> 8) == 1)
                    return index - startIndex;

                index++;
            }

            return 0;
        }
    }
}
