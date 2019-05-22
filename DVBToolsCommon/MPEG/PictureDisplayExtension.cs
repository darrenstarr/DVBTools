namespace DVBToolsCommon.MPEG
{
    // 6.2.3.3 Picture display extension
    //picture_display_extension() {                               No. of bits     Mnemonic
    //    extension_start_code_identifier                         4               uimsbf
    //    for ( i=0; i<number_of_frame_centre_offsets; i++ ) {
    //        frame_centre_horizontal_offset                      16              simsbf
    //        marker_bit                                          1               bslbf
    //        frame_centre_vertical_offset                        16              simsbf
    //        marker_bit                                          1               bslbf
    //    }
    //    next_start_code()
    //}
    public class PictureDisplayExtension : Extension
    {
        public int extensionStartCodeIdentifier;

        public PictureDisplayExtension()
            : base()
        {
        }

        public int Load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 10 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 10)
                return 0;

            int index = startIndex + 4;

            extensionStartCodeIdentifier = buffer[index] >> 4;

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
