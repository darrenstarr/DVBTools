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
