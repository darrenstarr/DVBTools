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
    public class StartCode
    {
        // From the limitation of the video stream information located in the DVD IFO header
        // there's a strong chance that there will never be a need for more than just the
        // first stream.
        public const byte FirstVideoStream = 0xE0;
        public const byte LastVideoStream = 0xE7;

        // Although there's are two more bits allowing for 32 audio streams, in the case
        // of DVD, there are a maximum of 8 streams available. If for some reason
        // a full MPEG-2 implementation is added later, then these values can be adjusted.
        public const byte FirstMPEG1AudioStream = 0xC0;
        public const byte LastMPEG1AudioStream = 0xC7;

        public const byte PrivateStream1 = 0xBD;
        public const byte PrivateStream2 = 0xBF;
        public const byte PaddingStream = 0xBE;
        public const byte ProgramStreamMap = 0xBC;
        public const byte ECMStream = 0xF0;        // probably never used on DVD
        public const byte EMMStream = 0xF1;        // probably never used on DVD
        public const byte AncillaryStream = 0xF9;
        public const byte ProgramStreamDirectory = 0xFF;

        public const byte PackStart = 0xBA;
        public const byte SystemHeader = 0xBB;

        // Start Codes from ISO13818-2
        public const byte Picture = 0x00;             // picture_start_code
        public const byte FirstSlice = 0x01;          // slice_start_code lowest value
        public const byte LastSlice = 0xAF;           // slice_start_code highest value
        public const byte UserData = 0xB2;            // user_data_start_code
        public const byte SequenceHeader = 0xB3;      // sequence_header_start_code
        public const byte SequenceError = 0xB4;       // sequence_error_code
        public const byte Extension = 0xB5;           // extension_start_code
        public const byte SequenceEnd = 0xB7;         // sequence_end_code
        public const byte Group = 0xB8;               // group_start_code    
    }
}
