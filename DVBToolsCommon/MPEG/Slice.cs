namespace DVBToolsCommon.MPEG
{
    // 6.2.4 Slice
    //slice() {                                                                   No. of bits     Mnemonic
    //    slice_start_code                                                        32              bslbf
    //    if (vertical_size > 2800)
    //        slice_vertical_position_extension                                   3               uimsbf
    //    if (<sequence_scalable_extension() is present in the bitstream>) {
    //        if (scalable_mode == “data partitioning” )
    //            priority_breakpoint                                             7               uimsbf
    //    }
    //    quantiser_scale_code                                                    5               uimsbf
    //    if ( nextbits() == ‘1’ ) {
    //        intra_slice_flag                                                    1               bslbf
    //        intra_slice                                                         1               uimsbf
    //        reserved_bits                                                       7               uimsbf
    //        while ( nextbits() == ‘1’ ) {
    //            extra_bit_slice /* with the value ‘1’ */                        1               uimsbf
    //            extra_information_slice                                         8               uimsbf
    //        }
    //    }
    //    extra_bit_slice /* with the value ‘0’ */                                1               uimsbf
    //    do {
    //        macroblock()
    //    } while ( nextbits() != ‘000 0000 0000 0000 0000 0000’ )
    //    next_start_code()
    //}
    public class Slice : VideoComponent
    {
        public int sliceVerticalPositionExtension;
        public int priorityBreakpoint;
        public int quantiserScaleCode;
        public int intraSliceFlag;
        public int intraSlice;

        public Slice()
            : base()
        {
        }

        public int Load(int verticalSize, bool scalableExtensionPresent, int scalableMode, byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 14 bytes are available for processing then the header and following start code
            // can't be read. In the case of slice, I just guessed that there would never be a slice less than
            // 10 bytes in length.
            if ((bufferLength - startIndex) < 14)
                return 0;

            int index = startIndex + 4;

            int bitOffset = 0;
            uint value = Read32(buffer, index);

            if (verticalSize > 2800)
            {
                sliceVerticalPositionExtension = (int)(value >> 29);
                bitOffset += 3;
            }

            if (scalableExtensionPresent)
            {
                if (scalableMode == (int)SequenceScalableExtension.ScalableMode.DataPartitioning)
                {
                    uint mask = 0xFE000000 >> bitOffset;
                    priorityBreakpoint = (int)((value & mask) >> (25 - bitOffset));
                    bitOffset += 7;
                }
            }
            // 0001 1011 0111 1100 0011 1110 1001 0110
            uint quantiserScaleMask = 0xF8000000 >> bitOffset;
            quantiserScaleCode = (int)((value & quantiserScaleMask) >> (27 - bitOffset));
            bitOffset += 5;

            intraSliceFlag = (int)((value >> (31 - bitOffset)) & 0x1);
            bitOffset++;
            if (intraSliceFlag == 1)
            {
                intraSlice = (int)((value >> (31 - bitOffset)) & 0x1);
                bitOffset += 8;

                // TODO : Consider implementing extra_bit_slice and extra_information_slice
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
