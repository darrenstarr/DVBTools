/* This file contains some of the components involved in implementing an MPEG-2 decoder. Specifically, I've added
 * the components of the specification as I come across them to the scale of extracting information from slices.
 * Each component is designed for performance as opposed to complete functionality. The MPEG object classes can
 * be used to extract different stream information for use in programs like multiplexers.
 * 
 * There are also enumerations provided for handling MPEG Systems and even DVD substream identification.
 * 
 * TODO :
 *  - Add support for LPCM information in DVD
 *  - Implement the remaining basic extensions for MPEG
 *  - Consider implementing finer control such as macroblocks
 */

namespace DVBToolsCommon.MPEG
{
    public class StartCodeFull
    {
        // From the limitation of the video stream information located in the DVD IFO header
        // there's a strong chance that there will never be a need for more than just the
        // first stream.
        public const uint FirstVideoStream = 0x000001E0;
        public const uint LastVideoStream = 0x000001E7;

        // Although there's are two more bits allowing for 32 audio streams, in the case
        // of DVD, there are a maximum of 8 streams available. If for some reason
        // a full MPEG-2 implementation is added later, then these values can be adjusted.
        public const uint FirstMPEG1AudioStream = 0x000001C0;
        public const uint LastMPEG1AudioStream = 0x000001C7;

        public const uint PrivateStream1 = 0x000001BD;
        public const uint PrivateStream2 = 0x000001BF;
        public const uint PaddingStream = 0x000001BE;
        public const uint ProgramStreamMap = 0x000001BC;
        public const uint ECMStream = 0x000001F0;        // probably never used on DVD
        public const uint EMMStream = 0x000001F1;        // probably never used on DVD
        public const uint AncillaryStream = 0x000001F9;
        public const uint ProgramStreamDirectory = 0x000001FF;

        public const uint PackStart = 0x000001BA;
        public const uint SystemHeader = 0x000001BB;

        // Start Codes from ISO13818-2
        public const uint Picture = 0x00000100;             // picture_start_code
        public const uint FirstSlice = 0x00000101;          // slice_start_code lowest value
        public const uint LastSlice = 0x000001AF;           // slice_start_code highest value
        public const uint UserData = 0x000001B2;            // user_data_start_code
        public const uint SequenceHeader = 0x000001B3;      // sequence_header_start_code
        public const uint SequenceError = 0x000001B4;       // sequence_error_code
        public const uint Extension = 0x000001B5;           // extension_start_code
        public const uint SequenceEnd = 0x000001B7;         // sequence_end_code
        public const uint Group = 0x000001B8;               // group_start_code
    }
}
