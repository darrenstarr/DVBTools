namespace DVBToolsCommon.MPEG
{
    // 6.2.2.6 Group of pictures header
    //group_of_pictures_header() {          range of value  No. of bits     Mnemonic
    //    group_start_code                                  32              bslbf       0   0xFFFFFFFF
    //    time_code                                         25              bslbf       4   0xFFFFFF80
    //drop_frame_flag                               1               uimsbf      4   0x80
    //time_code_hours               0 - 23          5               uimsbf      4   0x7C
    //time_code_minutes             0 - 59          6               uimsbf      4   0x03F0
    //marker_bit                    1               1               bslbf       5   0x08
    //time_code_seconds             0 - 59          6               uimsbf      5   0x07E0
    //time_code_pictures            0 - 59          6               uimsbf      6   0x1F80
    //    closed_gop                                        1               uimsbf      7   0x40
    //    broken_link                                       1               uimsbf      7   0x20
    //    next_start_code()
    //}   
    public class GroupOfPicturesHeader : VideoComponent
    {
        public TimeCode timeCode = new TimeCode();
        public int closedGOP;
        public int brokenLink;

        public GroupOfPicturesHeader()
            : base()
        {
        }

        public int Load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 11 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 11)
                return 0;

            int index = startIndex + 4;

            timeCode.dropFrameFlag = buffer[index] >> 7;
            timeCode.hours = (buffer[index] & 0x7C) >> 2;
            timeCode.minutes = (Read16(buffer, index++) & 0x3F0) >> 4;
            timeCode.seconds = (Read16(buffer, index++) & 0x07E0) >> 5;
            timeCode.pictures = (Read16(buffer, index++) & 0x1F80) >> 7;
            closedGOP = (buffer[index] & 0x40) >> 6;
            brokenLink = (buffer[index++] & 0x20) >> 5;

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
