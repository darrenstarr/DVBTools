using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // 6.2.3.2 Quant matrix extension
    //quant_matrix_extension() {                          No. of bits     Mnemonic
    //    extension_start_code_identifier                 4               uimsbf
    //    load_intra_quantiser_matrix                     1               uimsbf
    //    if ( load_intra_quantiser_matrix )
    //        intra_quantiser_matrix[64]                  8 * 64          uimsbf
    //    load_non_intra_quantiser_matrix                 1               uimsbf
    //    if ( load_non_intra_quantiser_matrix )
    //        non_intra_quantiser_matrix[64]              8 * 64          uimsbf
    //    load_chroma_intra_quantiser_matrix              1               uimsbf
    //    if ( load_chroma_intra_quantiser_matrix )
    //        chroma_intra_quantiser_matrix[64]           8 * 64          uimsbf
    //    load_chroma_non_intra_quantiser_matrix          1               uimsbf
    //    if ( load_chroma_non_intra_quantiser_matrix )
    //        chroma_non_intra_quantiser_matrix[64]       8 * 64          uimsbf
    //    next_start_code()
    //}
    public class QuantiserMatrixExtension : VideoComponent
    {
        public int extensionStartCodeIdentifier;
        public bool loadIntraQuantiserMatrix;
        public byte[] intraQuantiserMatrix = new byte[64];
        public bool loadNonIntraQuantiserMatrix;
        public byte[] nonIntraQuantiserMatrix = new byte[64];
        public bool loadChromaIntraQuantiserMatrix;
        public byte[] chromaIntraQuantiserMatrix = new byte[64];
        public bool loadChromaNonIntraQuantiserMatrix;
        public byte[] chromaNonIntraQuantiserMatrix = new byte[64];

        public QuantiserMatrixExtension()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        /// <todo>
        /// See if it's logical to implement a function for quantiser matrix loading
        /// </todo>
        public int load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 9 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 9)
                return 0;

            int index = startIndex + 4;

            extensionStartCodeIdentifier = buffer[index++] >> 4;

            int shift = 0;
            loadIntraQuantiserMatrix = (buffer[index] & 0x80) == 0x80 ? true : false;
            shift++;
            if (loadIntraQuantiserMatrix)
            {
                if ((bufferLength - index) < 68)
                    return 0;

                for (int i = 0; i < 64; i++)
                {
                    intraQuantiserMatrix[i] = (byte)((buffer[index] << shift) | (buffer[index + 1] >> (8 - shift)));
                    index++;
                }
            }

            loadNonIntraQuantiserMatrix = (buffer[index] & (0x80 >> shift)) == (0x80 >> shift) ? true : false;
            shift++;
            if (loadNonIntraQuantiserMatrix)
            {
                if ((bufferLength - index) < 68)
                    return 0;

                for (int i = 0; i < 64; i++)
                {
                    nonIntraQuantiserMatrix[i] = (byte)((buffer[index] << shift) | (buffer[index + 1] >> (8 - shift)));
                    index++;
                }
            }

            loadChromaIntraQuantiserMatrix = (buffer[index] & (0x80 >> shift)) == (0x80 >> shift) ? true : false;
            shift++;
            if (loadChromaIntraQuantiserMatrix)
            {
                if ((bufferLength - index) < 68)
                    return 0;

                for (int i = 0; i < 64; i++)
                {
                    chromaIntraQuantiserMatrix[i] = (byte)((buffer[index] << shift) | (buffer[index + 1] >> (8 - shift)));
                    index++;
                }
            }

            loadChromaNonIntraQuantiserMatrix = (buffer[index] & (0x80 >> shift)) == (0x80 >> shift) ? true : false;
            shift++;
            if (loadChromaNonIntraQuantiserMatrix)
            {
                if ((bufferLength - index) < 68)
                    return 0;

                for (int i = 0; i < 64; i++)
                {
                    chromaNonIntraQuantiserMatrix[i] = (byte)((buffer[index] << shift) | (buffer[index + 1] >> (8 - shift)));
                    index++;
                }
            }

            while (index < (bufferLength - 4))
            {
                if ((read32(buffer, index) >> 8) == 1)
                    return index - startIndex;

                index++;
            }

            return 0;
        }
    }
}
