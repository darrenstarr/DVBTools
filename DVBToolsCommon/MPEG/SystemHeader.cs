using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // Table 2-34 – Program Stream system header
    //Syntax                                          No. of bits     Mnemonic
    //system_header () {
    //    system_header_start_code                    32              bslbf         0   0xFFFFFFFF
    //    header_length                               16              uimsbf        4   0xFFFF
    //    marker_bit                                  1               bslbf         6   0x80
    //    rate_bound                                  22              uimsbf        6   0x7FFFFE
    //    marker_bit                                  1               bslbf         8   0x01
    //    audio_bound                                 6               uimsbf        9   0xFC
    //    fixed_flag                                  1               bslbf         9   0x02
    //    CSPS_flag                                   1               bslbf         9   0x01
    //    system_audio_lock_flag                      1               bslbf         10  0x80
    //    system_video_lock_flag                      1               bslbf         10  0x40
    //    marker_bit                                  1               bslbf         10  0x20
    //    video_bound                                 5               uimsbf        10  0x1F
    //    packet_rate_restriction_flag                1               bslbf         11  0x80
    //    reserved_bits                               7               bslbf         11  0x7F
    //    while (nextbits () = = '1') {
    //        stream_id                               8               uimsbf        
    //        '11'                                    2               bslbf
    //        P-STD_buffer_bound_scale                1               bslbf
    //        P-STD_buffer_size_bound                 13              uimsbf
    //    }
    //}
    internal class SystemHeader : VideoComponent
    {
        public ushort headerLength;
        public uint rateBound;
        public ushort audioBound;
        public bool fixedFlag;
        public bool CSPSFlag;
        public bool systemAudioLockFlag;
        public bool systemVideoLockFlag;
        public ushort videoBound;
        public bool packetRateRestrictionFlag;

        public SystemHeader()
            : base()
        {
        }

        public int load(byte[] buffer, int startIndex, int bufferLength)
        {
            int index = startIndex + 4;            

            headerLength = read16(buffer, index);
            index += 2;
            rateBound = (read32(buffer, index) >> 9) & 0x3FFFFF;
            index += 3;
            audioBound = (ushort) (buffer[index] >> 2);
            fixedFlag = (buffer[index] & 0x02) == 0x02 ? true : false;
            CSPSFlag = (buffer[index++] & 0x01) == 0x01 ? true : false;
            systemAudioLockFlag = (buffer[index] & 0x80) == 0x80 ? true : false;
            systemVideoLockFlag = (buffer[index] & 0x40) == 0x40 ? true : false;
            videoBound = (ushort)(buffer[index++] & 0x1F);
            packetRateRestrictionFlag = (buffer[index++] & 0x80) == 0x80 ? true : false;

            // TODO : Implement
            //    while (nextbits () = = '1') {
            //        stream_id                               8               uimsbf        
            //        '11'                                    2               bslbf
            //        P-STD_buffer_bound_scale                1               bslbf
            //        P-STD_buffer_size_bound                 13              uimsbf
            //    }
            while ((buffer[index] & 0x80) == 0x80)
                index += 3;
            
            return index - startIndex;
        }
    }
}
