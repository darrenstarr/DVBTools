using System;
using System.Collections.Generic;
using System.Text;

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
        public const UInt32 FirstVideoStream = 0x000001E0;
        public const UInt32 LastVideoStream = 0x000001E7;

        // Although there's are two more bits allowing for 32 audio streams, in the case
        // of DVD, there are a maximum of 8 streams available. If for some reason
        // a full MPEG-2 implementation is added later, then these values can be adjusted.
        public const UInt32 FirstMPEG1AudioStream = 0x000001C0;
        public const UInt32 LastMPEG1AudioStream = 0x000001C7;

        public const UInt32 PrivateStream1 = 0x000001BD;
        public const UInt32 PrivateStream2 = 0x000001BF;
        public const UInt32 PaddingStream = 0x000001BE;
        public const UInt32 ProgramStreamMap = 0x000001BC;
        public const UInt32 ECMStream = 0x000001F0;        // probably never used on DVD
        public const UInt32 EMMStream = 0x000001F1;        // probably never used on DVD
        public const UInt32 AncillaryStream = 0x000001F9;
        public const UInt32 ProgramStreamDirectory = 0x000001FF;

        public const UInt32 PackStart = 0x000001BA;
        public const UInt32 SystemHeader = 0x000001BB;

        // Start Codes from ISO13818-2
        public const UInt32 Picture = 0x00000100;             // picture_start_code
        public const UInt32 FirstSlice = 0x00000101;          // slice_start_code lowest value
        public const UInt32 LastSlice = 0x000001AF;           // slice_start_code highest value
        public const UInt32 UserData = 0x000001B2;            // user_data_start_code
        public const UInt32 SequenceHeader = 0x000001B3;      // sequence_header_start_code
        public const UInt32 SequenceError = 0x000001B4;       // sequence_error_code
        public const UInt32 Extension = 0x000001B5;           // extension_start_code
        public const UInt32 SequenceEnd = 0x000001B7;         // sequence_end_code
        public const UInt32 Group = 0x000001B8;               // group_start_code
    }

    public class StartCode
    {
        // From the limitation of the video stream information located in the DVD IFO header
        // there's a strong chance that there will never be a need for more than just the
        // first stream.
        public const Byte FirstVideoStream = 0xE0;
        public const Byte LastVideoStream = 0xE7;

        // Although there's are two more bits allowing for 32 audio streams, in the case
        // of DVD, there are a maximum of 8 streams available. If for some reason
        // a full MPEG-2 implementation is added later, then these values can be adjusted.
        public const Byte FirstMPEG1AudioStream = 0xC0;
        public const Byte LastMPEG1AudioStream = 0xC7;

        public const Byte PrivateStream1 = 0xBD;
        public const Byte PrivateStream2 = 0xBF;
        public const Byte PaddingStream = 0xBE;
        public const Byte ProgramStreamMap = 0xBC;
        public const Byte ECMStream = 0xF0;        // probably never used on DVD
        public const Byte EMMStream = 0xF1;        // probably never used on DVD
        public const Byte AncillaryStream = 0xF9;
        public const Byte ProgramStreamDirectory = 0xFF;

        public const Byte PackStart = 0xBA;
        public const Byte SystemHeader = 0xBB;

        // Start Codes from ISO13818-2
        public const Byte Picture = 0x00;             // picture_start_code
        public const Byte FirstSlice = 0x01;          // slice_start_code lowest value
        public const Byte LastSlice = 0xAF;           // slice_start_code highest value
        public const Byte UserData = 0xB2;            // user_data_start_code
        public const Byte SequenceHeader = 0xB3;      // sequence_header_start_code
        public const Byte SequenceError = 0xB4;       // sequence_error_code
        public const Byte Extension = 0xB5;           // extension_start_code
        public const Byte SequenceEnd = 0xB7;         // sequence_end_code
        public const Byte Group = 0xB8;               // group_start_code    
    }

    //extension_start_code_identifier     Name
    //                           0000     reserved
    //                           0001     Sequence Extension ID
    //                           0010     Sequence Display Extension ID
    //                           0011     Quant Matrix Extension ID
    //                           0100     Copyright Extension ID
    //                           0101     Sequence Scalable Extension ID
    //                           0110     reserved
    //                           0111     Picture Display Extension ID
    //                           1000     Picture Coding Extension ID
    //                           1001     Picture Spatial Scalable Extension ID
    //                           1010     Picture Temporal Scalable Extension ID
    //                           1011     reserved
    //                           1100     reserved
    //                            ...     ...
    //                           1111     reserved
    public class ExtensionStartCode
    {
        public const Byte Sequence = 0x01;                  // Sequence Extension ID
        public const Byte SequenceDisplay = 0x02;           // Sequence Display Extension ID
        public const Byte QuantMatrix = 0x03;               // Quant Matrix Extension ID
        public const Byte Copyright = 0x04;                 // Copyright Extension ID
        public const Byte SequenceScalable = 0x05;          // Sequence Scalable Extension ID
        public const Byte PictureDisplay = 0x07;            // Picture Display Extension ID
        public const Byte PictureCoding = 0x08;             // Picture Coding Extension ID
        public const Byte PictureSpatialScalable = 0x09;    // Picture Spatial Scalable Extension ID
        public const Byte PictureTemporalScalable = 0x0A;   // Picture Temporal Scalable Extension ID
    }

    public class DVDSubstreamID
    {
        public const Byte FirstSubtitleStream = 0x20;
        public const Byte LastSubtitleStream = 0x3F;
        public const Byte FirstAC3Stream = 0x80;
        public const Byte LastAC3Stream = 0x87;
        public const Byte FirstDTSStream = 0x88;
        public const Byte LastDTSStream = 0x8F;
    }

    /// <summary>
    /// A base class to use for handling parsing of MPEG headers. It simply includes some utility functions
    /// that simplify reading big-endian variables.
    /// </summary>
    public class VideoComponent
    {
        protected VideoComponent()
        {
        }

        protected UInt16 read16(Byte[] buffer, int index)
        {
            return (UInt16)((((UInt16)buffer[index]) << 8) | ((UInt16)buffer[index + 1]));
        }

        protected UInt32 read32(Byte[] buffer, int index)
        {
            return (((UInt32)buffer[index + 0]) << 24) | (((UInt32)buffer[index + 1]) << 16) | (((UInt32)buffer[index + 2]) << 8) | ((UInt32)buffer[index + 3]);
        }
    }

    /// <summary>
    /// Adds the Present? property to VideoComponent so tracking if an extension has been processed is included
    /// </summary>
    public class Extension : VideoComponent
    {
        public bool Present = false;

        protected Extension() : base()
        {
        }
    }                 
}
