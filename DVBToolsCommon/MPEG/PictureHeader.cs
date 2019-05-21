using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // 6.2.3 Picture header
    //picture_header() {                                                  No. of bits Mnemonic
    //    picture_start_code                                              32          bslbf
    //    temporal_reference                                              10          uimsbf    4   0xFFC0
    //    picture_coding_type                                             3           uimsbf    5   0x38
    //    vbv_delay                                                       16          uimsbf    5   0x7FFF8000
    //    if ( picture_coding_type == 2 || picture_coding_type == 3) {
    //        full_pel_forward_vector                                     1           bslbf     i   0x40
    //        forward_f_code                                              3           bslbf     i   0x38
    //    }
    //    if ( picture_coding_type == 3 ) {
    //        full_pel_backward_vector                                    1           bslbf     i   0x04
    //        backward_f_code                                             3           bslbf     i   0x0380
    //    }
    //    while ( nextbits() == ‘1’ ) {
    //        extra_bit_picture /* with the value ‘1’ */                  1           uimsbf    i+1 0x40
    //        extra_information_picture                                   8           uimsbf
    //    }
    //    extra_bit_picture /* with the value ‘0’ */                      1           uimsbf    i+1 0x40
    //    next_start_code()
    //}
    public class PictureHeader : VideoComponent
    {
        public enum PictureCodingType
        {
            Forbidden = 0,
            IntraCoded = 1,
            PredictiveCoded = 2,
            BidirectionallyPredictiveCoded = 3,
            DCIntraCoded = 4,           // Shall not be used, (dc intra-coded(D) in ISO/IEC11172-2)
        }

        public int temporalReference;
        public PictureCodingType pictureCodingType;
        public int vbvDelay;
        public int fullPelForwardVector;
        public int forwardFCode;
        public int fullPelBackwardVector;
        public int backwardFCode;

        public PictureHeader()
            : base()
        {
        }

        public int load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 11 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 11)
                return 0;

            int index = startIndex + 4;

            temporalReference = read16(buffer, index++) >> 6;
            pictureCodingType = (PictureCodingType)((buffer[index] & 0x38) >> 3);
            vbvDelay = (int)((read32(buffer, index += 2) & 0x7FFF8000) >> 15);
            if (pictureCodingType == PictureCodingType.PredictiveCoded || pictureCodingType == PictureCodingType.BidirectionallyPredictiveCoded)
            {
                fullPelForwardVector = (buffer[index] & 0x40) >> 6;
                forwardFCode = (buffer[index] & 0x38) >> 3;
            }
            if (pictureCodingType == PictureCodingType.BidirectionallyPredictiveCoded)
            {
                fullPelBackwardVector = (buffer[index] & 0x04) >> 2;
                backwardFCode = (read16(buffer, index++) & 0x0380) >> 7;
            }

            // We're skipping extra_bit_picture and extra_information_picture since "ISO13838-2 6.3.9 Picture header" reserves value 1
            // for future use, but does not define it. 
            // Not to mention since it makes use of varying bit alignment, I don't think it's worth implementing in this
            // implementation.
            // TODO : Consider implementing extra_bit_picture and extra_information_picture
            index++;

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
