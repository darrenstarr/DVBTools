using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // 6.2.2.4 Sequence display extension
    //sequence_display_extension() {          No. of bits     Mnemonic
    //    extension_start_code_identifier     4               uimsbf        4   0xF0
    //    video_format                        3               uimsbf        4   0x0E
    //    colour_description                  1               uimsbf        4   0x01
    //    if ( colour_description ) {
    //        colour_primaries                8               uimsbf        5   0xFF
    //        transfer_characteristics        8               uimsbf        6   0xFF
    //        matrix_coefficients             8               uimsbf        7   0xFF
    //    }
    //    display_horizontal_size             14              uimsbf        8   0xFFFC
    //    marker_bit                          1               bslbf         9   0x02
    //    display_vertical_size               14              uimsbf        9   0x1FFE
    //    next_start_code()
    //}
    public class SequenceDisplayExtension : Extension
    {
        public enum VideoFormat
        {
            Component = 0,
            PAL = 1,
            NTSC = 2,
            SECAM = 3,
            MAC = 4,
            Unspecified = 5
        }

        public enum ColourPrimaries
        {
            Forbidden = 0,
            BT709 = 1,
            UnspecifiedVideo = 2,
            BT470_2SystemM = 4,
            BT470_2SystemBG = 5,
            SMPTE170M = 6,
            SMPTE240M = 7
        }

        public enum TransferCharacteristics
        {
            Forbidden = 0,
            BT709 = 1,
            UnspecifiedVideo = 2,
            BT470_2SystemM = 4,
            BT470_2SystemBG = 5,
            SMPTE170M = 6,
            SMPTE240M = 7,
            LinearTransfer = 8
        }

        public enum MatrixCoefficients
        {
            Forbidden = 0,
            BT709 = 1,
            UnspecifiedVideo = 2,
            FCC = 4,
            BT470_2SystemBG = 5,
            SMPTE170M = 6,
            SMPTE240M = 7
        }

        public int extensionStartCodeIdentifier;
        public VideoFormat videoFormat;
        public int colourDescription;
        public ColourPrimaries colourPrimaries;
        public TransferCharacteristics transferCharacteristics;
        public MatrixCoefficients matrixCoefficients;
        public int displayHorizontalSize;
        public int displayVerticalSize;

        public SequenceDisplayExtension()
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
            videoFormat = (VideoFormat)((buffer[index + 4] & 0xE) >> 1);
            colourDescription = buffer[index + 4] & 0x01;

            index += 5;
            if (colourDescription == 1)
            {
                colourPrimaries = (ColourPrimaries)(buffer[index++]);
                transferCharacteristics = (TransferCharacteristics)(buffer[index++]);
                matrixCoefficients = (MatrixCoefficients)(buffer[index++]);
            }

            displayHorizontalSize = read16(buffer, index++) >> 2;
            displayVerticalSize = (read16(buffer, index += 2) & 0x1FFE) >> 1;

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
