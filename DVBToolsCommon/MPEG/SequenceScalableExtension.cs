using System;
using System.Collections.Generic;
using System.Text;

namespace DVBToolsCommon.MPEG
{
    // 6.2.2.5 Sequence scalable extension
    //sequence_scalable_extension() {                     No. of bits     Mnemonic
    //    extension_start_code_identifier                 4               uimsbf        4   0xF0
    //    scalable_mode                                   2               uimsbf        4   0x0C
    //    layer_id                                        4               uimsbf        4   0x03C0
    //    if (scalable_mode == “spatial scalability”) {
    //        lower_layer_prediction_horizontal_size      14              uimsbf        5   0x3FFF
    //        marker_bit                                  1               bslbf         7   0x80
    //        lower_layer_prediction_vertical_size        14              uimsbf        7   0x7FFE
    //        horizontal_subsampling_factor_m             5               uimsbf        8   0x01F0
    //        horizontal_subsampling_factor_n             5               uimsbf        9   0x0F80
    //        vertical_subsampling_factor_m               5               uimsbf        10  0x7C
    //        vertical_subsampling_factor_n               5               uimsbf        10  0x03E0
    //    }
    //    if ( scalable_mode == “temporal scalability” ) {
    //        picture_mux_enable                          1               uimsbf        5   0x20
    //        if ( picture_mux_enable )
    //            mux_to_progressive_sequence             1               uimsbf        5   0x10
    //        picture_mux_order                           3               uimsbf        5   0x0E
    //        picture_mux_factor                          3               uimsbf        5   0x01C0
    //    }
    //    next_start_code()
    //}
    public class SequenceScalableExtension : Extension
    {
        public enum ScalableMode
        {
            DataPartitioning = 0,
            SpatialScalability = 1,
            SNRScalability = 2,
            TemporalScalability = 3
        }

        public int extensionStartCodeIdentifier;
        public ScalableMode scalableMode;
        public int layerId;
        public int lowerLayerPredictionHorizontalSize;
        public int lowerLayerPredictionVerticalSize;
        public int horizontalSubsamplingFactorM;
        public int horizontalSubsamplingFactorN;
        public int verticalSubsamplingFactorM;
        public int verticalSubsamplingFactorN;
        public int pictureMuxEnable;
        public int muxToProgressiveSequence;
        public int pictureMuxOrder;
        public int pictureMuxFactor;

        public SequenceScalableExtension()
            : base()
        {
        }

        public int load(byte[] buffer, int startIndex, int bufferLength)
        {
            // If less than 10 bytes are available for processing then the header and following start code
            // can't be read.
            if ((bufferLength - startIndex) < 10)
                return 0;

            int index = startIndex + 4;

            extensionStartCodeIdentifier = buffer[index] >> 4;
            scalableMode = (ScalableMode)((buffer[index++] & 0xC0) >> 2);
            layerId = (read16(buffer, index++) & 0x03C0) >> 6;

            if (scalableMode == ScalableMode.SpatialScalability)
            {
                if ((bufferLength - startIndex) < 15)
                    return 0;

                lowerLayerPredictionHorizontalSize = read16(buffer, index += 2) & 0x3FFF;
                lowerLayerPredictionVerticalSize = (read16(buffer, index++) & 0x7FFE) >> 1;
                horizontalSubsamplingFactorM = (read16(buffer, index++) & 0x01F0) >> 4;
                horizontalSubsamplingFactorN = (read16(buffer, index++) & 0x0F80) >> 7;
                verticalSubsamplingFactorM = (buffer[index] & 0x7C) >> 6;
                verticalSubsamplingFactorN = (read16(buffer, index) & 0x03E0) >> 5;
            }
            if (scalableMode == ScalableMode.TemporalScalability)
            {
                pictureMuxEnable = (buffer[index] & 0x20) >> 5;
                if (pictureMuxEnable == 1)
                {
                    muxToProgressiveSequence = (buffer[index] & 0x10) >> 4;
                    pictureMuxOrder = (buffer[index] & 0x0E) >> 1;
                    pictureMuxFactor = (read16(buffer, index) & 0x01C0) >> 6;
                }
                else
                {
                    pictureMuxOrder = (buffer[index] & 0x1C) >> 2;
                    pictureMuxFactor = (read16(buffer, index) & 0x0380) >> 7;
                }
            }

            index++;

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
