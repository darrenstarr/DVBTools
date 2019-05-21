using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // Table 2-33 – Program Stream pack header
    //Syntax                                                  No. of bits   Mnemonic
    //pack_header() {
    //    pack_start_code                                     32            bslbf       0   0xFFFFFFFF
    //    '01'                                                2             bslbf       4   0xC0
    //    system_clock_reference_base [32..30]                3             bslbf       5   0x38
    //    marker_bit                                          1             bslbf       5   0x04
    //    system_clock_reference_base [29..15]                15            bslbf       5   0x03FFF8
    //    marker_bit                                          1             bslbf       7   0x04
    //    system_clock_reference_base [14..0]                 15            bslbf       7   0x03FFF8
    //    marker_bit                                          1             bslbf       9   0x04
    //    system_clock_reference_extension                    9             uimsbf      9   0x03FE
    //    marker_bit                                          1             bslbf       9   0x01
    //    program_mux_rate                                    22            uimsbf      10  0xFFFFFC
    //    marker_bit                                          1             bslbf       12  0x02
    //    marker_bit                                          1             bslbf       12  0x01
    //    reserved                                            5             bslbf       13  0xF8
    //    pack_stuffing_length                                3             uimsbf      14  0x07
    //    for (i = 0; i < pack_stuffing_length; i++) {
    //        stuffing_byte                                   8             bslbf
    //    }
    //    if (nextbits() = = system_header_start_code) {
    //        system_header ()
    //    }
    //}
    internal class PackHeader : VideoComponent
    {
        public UInt64 systemClockReferenceBase;
        public UInt64 systemClockReferenceExtension;
        public uint programMuxRate;
        public int packStuffingLength;
        public bool isMpeg2;

        public PackHeader() : base()
        {
        }

        /// <summary>
        /// Loads an MPEG-2 pack_header()
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        /// <todo>
        /// Implement MPEG-1 maybe
        /// </todo>
        public int load(byte[] buffer, int startIndex, int bufferLength)
        {
            int index = startIndex + 4;

            UInt64 temp = buffer[index++];
            isMpeg2 = (temp & 0x40) == 0x40 ? true : false;

            systemClockReferenceBase = (temp & 0x38) << 27;
            systemClockReferenceBase |= (temp & 0x03) << 28;
            temp = buffer[index++];
            systemClockReferenceBase |= temp << 20;
            temp = buffer[index++];
            systemClockReferenceBase |= (temp & 0xF8) << 12;
            systemClockReferenceBase |= (temp & 0x03) << 13;
            temp = buffer[index++];
            systemClockReferenceBase |= temp << 5;
            temp = buffer[index++];
            systemClockReferenceBase |= temp >> 3;

            systemClockReferenceExtension = (temp & 0x3) << 7;
            temp = buffer[index++];
            systemClockReferenceExtension = temp >> 1;

            uint tempInt = buffer[index++];
            programMuxRate = tempInt << 14;
            tempInt = buffer[index++];
            programMuxRate |= tempInt << 6;
            tempInt = buffer[index++];
            programMuxRate |= tempInt >> 2;

            packStuffingLength = (int)(buffer[index++] & 0x7);
            index += packStuffingLength;

            return index - startIndex;
        }
    }
}
