using System;
using System.Collections.Generic;
using System.Text;

namespace MPEG.PSI
{
    public class VideoStreamDescriptor : Descriptor
    {
        /// <summary>
        /// Defines frame rate codes as specified by ISO13818-2 6.3.3, table 6-4
        /// </summary>
        public enum FrameRateCodes
        {
            Forbidden = 0,
            FrameRate_23_976 = 1,
            FrameRate_24 = 2,
            FrameRate_25 = 3,
            FrameRate_29_97 = 4,
            FrameRate_30 = 5,
            FrameRate_50 = 6,
            FrameRate_59_94 = 7,
            FrameRate_60 = 8
        };

        /// <summary>
        /// Defines frame rates as specified by ISO13818-2 6.3.3, table 6-4
        /// </summary>
        public static readonly double[] FrameRates = new double[] 
        {
            0,
            23.976,
            24,
            25,
            29.97,
            30,
            50,
            59.94,
            60,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };

        /// <summary>
        /// ISO13818-2 Table 8-2. Profile indication
        /// </summary>
        public enum ProfileIdentification
        {
            Reserved = 0,
            High = 1,
            SpatiallyScalable = 2,
            SNRScalable = 3,
            Main = 4,
            Simple = 5
        };

        /// <summary>
        /// ISO13818-2 Table 8-3. Level indication
        /// </summary>
        public enum LevelIdentification
        {
            Reserved = 0,
            High = 4,
            High1440 = 6,
            Main = 8,
            Low = 10
        };

        /// <summary>
        /// ISO13818-2 Table 6-5. Meaning of chroma_format
        /// </summary>
        public enum ChromaFormat
        {
            Reserved = 0,
            FourTwoZero = 1,
            FourTwoTwo = 2,
            FourFourFour = 3
        };

        public override byte DescriptorTag
        {
            get
            {
                return 2;
            }
        }

        public override byte DescriptorLength
        {
            get 
            {
                if (mpeg1OnlyFlag)
                    return 1;
                return 3;
            }
        }

        /// <summary>
        /// ISO13818-1 2.6.3 multiple_frame_rate_flag
        /// set to true to signify multiple frame rates may be present in the video stream
        /// </summary>
        public bool multipleFrameRateFlag = false;

        /// <summary>
        /// ISO13818-1 2.6.3 frame_rate_code
        /// Provides a frame rate code which is defined in ISO13818-2 6.3.3 Table 6-4
        /// The integer value of the enum can be cross referenced to a double via the
        /// FrameRates member enum.
        /// </summary>
        public FrameRateCodes frameRateCode = FrameRateCodes.Forbidden;

        /// <summary>
        /// ISO13818-1 2.6.3 MPEG_1_only_flag
        /// Set to true when the video stream contains only ISO/IEC11172-2 data
        /// </summary>
        public bool mpeg1OnlyFlag = false;

        /// <summary>
        /// ISO13818-1 2.6.3 constrained_parameter_flag
        /// When mpeg1OnlyFlag is true, a true value specifies the video stream will not contain
        /// unconstrained ISO-IEC 11172-2 data. When mpeg1OnlyFlag is set to false, this field must be true.
        /// </summary>
        public bool constrainedParameterFlag = true;

        /// <summary>
        /// ISO13818-1 2.6.3 still_picture_flag
        /// When true specifies the video stream contains only still pictures.
        /// </summary>
        public bool stillPictureFlag = false;

        /// <summary>
        /// ISO13818-1 2.6.3 profile_and_level_indication
        /// Specifies the three components of the profile and level indication.
        /// These values are elaborated across many pages in ISO13818-2 Section 8.
        /// </summary>
        public byte profileAndLevelIndication
        {
            get
            {
                return (byte)((byte)(profileAndLevelIndicationEscapeBit ? 0x80 : 0x00) | (((byte)profileIndication) << 4) | ((byte)levelIndication));
            }
            set
            {
                if ((value & 0x80) == 0x80)
                    profileAndLevelIndicationEscapeBit = true;
                else
                    profileAndLevelIndicationEscapeBit = false;

                profileIndication = (ProfileIdentification)((value >> 4) & 0x7);
                levelIndication = (LevelIdentification)(value & 0xF);
            }
        }

        /// <summary>
        /// Explains the escape bit of the Profile and Level Indication
        /// See ISO13818-2 section 8 for more details
        /// </summary>
        public bool profileAndLevelIndicationEscapeBit = true;

        /// <summary>
        /// Specifies the video profile used
        /// See ISO13818-2 section 8 for more details
        /// </summary>
        public ProfileIdentification profileIndication = ProfileIdentification.Simple;

        /// <summary>
        /// Specifies the video level used
        /// See ISO13818-2 Section 8 for more details.
        /// </summary>
        public LevelIdentification levelIndication = LevelIdentification.Main;

        /// <summary>
        /// ISO13818-1 2.6.3 chroma_format
        /// Defines the chroma format used by the video stream. Defined in detail in ISO13818-2 Table 6-5.
        /// </summary>
        public ChromaFormat chroma = ChromaFormat.FourTwoZero;

        /// <summary>
        /// ISO13818-1 2.6.3 frame_rate_extension_flag
        /// When true specified that both the frame_rate_extension_n and frame_rate_extension_d are non-zero
        /// in any of the video sequences. For ISO/IEC 11172-2 contrained video, these fields should be zero.
        /// </summary>
        public bool frameRateExtensionFlag = false;
    }
}
