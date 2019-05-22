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
    /// <summary>
    /// A base class to use for handling parsing of MPEG headers. It simply includes some utility functions
    /// that simplify reading big-endian variables.
    /// </summary>
    public class VideoComponent
    {
        protected VideoComponent()
        {
        }

        protected ushort Read16(byte[] buffer, int index)
        {
            return (ushort)((((ushort)buffer[index]) << 8) | ((ushort)buffer[index + 1]));
        }

        protected uint Read32(byte[] buffer, int index)
        {
            return (((uint)buffer[index + 0]) << 24) | (((uint)buffer[index + 1]) << 16) | (((uint)buffer[index + 2]) << 8) | ((uint)buffer[index + 3]);
        }
    }
}
