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
        public const byte Sequence = 0x01;                  // Sequence Extension ID
        public const byte SequenceDisplay = 0x02;           // Sequence Display Extension ID
        public const byte QuantMatrix = 0x03;               // Quant Matrix Extension ID
        public const byte Copyright = 0x04;                 // Copyright Extension ID
        public const byte SequenceScalable = 0x05;          // Sequence Scalable Extension ID
        public const byte PictureDisplay = 0x07;            // Picture Display Extension ID
        public const byte PictureCoding = 0x08;             // Picture Coding Extension ID
        public const byte PictureSpatialScalable = 0x09;    // Picture Spatial Scalable Extension ID
        public const byte PictureTemporalScalable = 0x0A;   // Picture Temporal Scalable Extension ID
    }
}
