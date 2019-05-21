namespace MPEG.PSI
{
    public class AudioStreamDescriptor : Descriptor
    {
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
                return 1;
            }
        }

        /// <summary>
        /// ISO13818-1 2.6.4 free_format_flag
        /// When true, the audio stream may contain one or more frames with the bitrate index set to '0000'
        /// </summary>
        public bool freeFormatFlag = true;

        /// <summary>
        /// ISO13818-1 2.6.4 ID
        /// When true, the ID field of each audio frame in the audio stream is set to '1'.
        /// see ISO13818-3 2.4.2.3 for details.
        /// </summary>
        public bool id = true;
    }
}
