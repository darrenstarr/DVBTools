using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DVBToolsCommon.VideoDecoders
{
    public class MPEG2Decoder
    {
        private const int MPEG2WriteBufferLength = 1024 * 1024;

        private String m_outputFileName = "";
        protected int m_streamId = 0;
        protected FileStream m_outputFile;
        protected BinaryWriter m_outputStream;

        private Byte[] writeBuffer = new Byte[MPEG2WriteBufferLength];
        private int bufferUsed = 0;
        private int picturesInSequence = 0;
        private int slicesInSequence = 0;
        private MPEG.SequenceHeader sequenceHeader = new MPEG.SequenceHeader();
        private MPEG.PictureHeader pictureHeader = new MPEG.PictureHeader();
        private MPEG.Slice slice = new MPEG.Slice();
        private MPEG.GroupOfPicturesHeader groupOfPicturesHeader = new MPEG.GroupOfPicturesHeader();
        private MPEG.SequenceExtension sequenceExtension = new MPEG.SequenceExtension();
        private MPEG.SequenceDisplayExtension sequenceDisplayExtension = new MPEG.SequenceDisplayExtension();
        private MPEG.PictureCodingExtension pictureCodingExtension = new MPEG.PictureCodingExtension();
        private MPEG.SequenceScalableExtension sequenceScalableExtension = new MPEG.SequenceScalableExtension();

        // This is calculated during header and extension decoding, it is calculated using 
        //  bits 13..12 = SequenceExtension.horizontalSizeExtension
        //  bits 11..0 = SequenceHeader.horizontalSizeValue
        private int mpegPictureHeight;

        private void resetPresenceFlags()
        {
            sequenceExtension.Present = false;
            sequenceDisplayExtension.Present = false;
            pictureCodingExtension.Present = false;
            sequenceScalableExtension.Present = false;
        }

        public MPEG2Decoder()
        {
        }

        ~MPEG2Decoder()
        {
            close();
        }

        virtual public String OutputFileName
        {
            get
            {
                return m_outputFileName;
            }
            set
            {
                m_outputFileName = value;
            }
        }

        virtual public int streamId
        {
            get
            {
                return m_streamId;
            }
            set
            {
                m_streamId = value;
            }
        }

        public uint DetectBitrate(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte [] buffer = new byte[1024 * 1024];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            uint bitRate = 0;

            int index = 0;
            while ((index + 4) < buffer.Length)
            {
                index = nextStartCode(buffer, index, buffer.Length);
                if (index == -1)
                    break;
//                    throw new Exception("Could not locate a sequence header in the first 512KB of the file");

                UInt32 startCode = read32(buffer, index);
                index += 4;
                switch (startCode)
                {
                    case MPEG.StartCodeFull.SequenceHeader:
                        int count = sequenceHeader.load(buffer, index - 4, buffer.Length - index + 4);
                        if (count != 0)
                        {
                            //System.Diagnostics.Debug.WriteLine(sequenceHeader.bitRateValue.ToString());
                            if (sequenceHeader.bitRateValue > bitRate)
                                bitRate = (uint) sequenceHeader.bitRateValue;

                            index += count - 4;
                        }
                        break;
                }
            }
            return bitRate * 400;
        }

        public virtual bool open()
        {
            FileInfo fi = new FileInfo(OutputFileName);
            if (fi.Exists)
                fi.Delete();

            try
            {
                m_outputFile = new FileStream(OutputFileName, FileMode.CreateNew, FileAccess.Write);
                m_outputStream = new BinaryWriter(m_outputFile);
            }
            catch
            {
                close();
                return false;
            }
            return true;
        }

        private int nextStartCode(byte [] buffer, int startIndex, int length)
        {
            int index = startIndex;

            while (index < (length - 4))
            {
                if (buffer[index] == 0x00 &&
                   buffer[index + 1] == 0x00 &&
                   buffer[index + 2] == 0x01)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        private int nextStartCode(int startIndex)
        {            
            return nextStartCode(writeBuffer, startIndex, bufferUsed);
        }

        public void close()
        {
            if (m_outputFile == null)
                return;

            processBuffer();
            m_outputStream.Write(writeBuffer, 0, bufferUsed);

            m_outputStream.Close();
            m_outputStream = null;

            m_outputFile.Close();
            m_outputFile = null;
        }

        private void processBuffer()
        {
            int index = 0;

            while (index < bufferUsed)
            {
                int newIndex = nextStartCode(index);
                if (newIndex == -1)
                {
                    if (index == 0)
                        return;

                    index -= 4;

                    m_outputStream.Write(writeBuffer, 0, index);
                    for (int i = index; i < bufferUsed; i++)
                        writeBuffer[i - index] = writeBuffer[i];

                    bufferUsed -= index;
                    return;
                }

                index = newIndex;

                UInt32 startCode = read32(index);
                index += 4;
                int count;
                switch (startCode)
                {
                    case MPEG.StartCodeFull.SequenceHeader:
                        count = sequenceHeader.load(writeBuffer, index - 4, bufferUsed);
                        //System.Diagnostics.Debug.WriteLine("Sequence Start Code at 0x" + String.Format("{0:X8}", m_outputFile.Position + index - 4) + ", pictures = " + picturesInSequence.ToString() + ", slices " + slicesInSequence.ToString());
                        if (count != 0)
                        {
                            index += count - 4;
                            mpegPictureHeight = sequenceHeader.verticalSizeValue;
                            resetPresenceFlags();

                            picturesInSequence = 0;
                            slicesInSequence = 0;
                        }
                        break;

                    case MPEG.StartCodeFull.Extension:
                        switch (writeBuffer[index] >> 4)
                        {
                            case MPEG.ExtensionStartCode.Sequence:
                                count = sequenceExtension.load(writeBuffer, index - 4, bufferUsed);
                                if(count > 0)
                                    mpegPictureHeight |= sequenceExtension.verticalSizeExtension << 12;
                                break;
                            case MPEG.ExtensionStartCode.SequenceDisplay:
                                count = sequenceDisplayExtension.load(writeBuffer, index - 4, bufferUsed);
                                break;
                            case MPEG.ExtensionStartCode.PictureCoding:
                                count = pictureCodingExtension.load(writeBuffer, index - 4, bufferUsed);
                                break;
                            case MPEG.ExtensionStartCode.SequenceScalable:
                                count = sequenceScalableExtension.load(writeBuffer, index - 4, bufferUsed);
                                break;
                            default:
                                count = 0;
                                break;
                        }
                        if (count > 0)
                            index += count - 4;

                        break;

                    case MPEG.StartCodeFull.Picture:
                        count = pictureHeader.load(writeBuffer, index - 4, bufferUsed);
                        if (count > 0)
                        {
                            index += count - 4;
                            picturesInSequence++;
                        }
                        break;

                    case MPEG.StartCodeFull.Group:
                        count = groupOfPicturesHeader.load(writeBuffer, index - 4, bufferUsed);
                        if(count > 0)
                            index += count - 4;

                        break;
                    default:
                        if (startCode >= MPEG.StartCodeFull.FirstSlice && startCode <= MPEG.StartCodeFull.LastSlice)
                        {
                            count = slice.load(mpegPictureHeight, sequenceScalableExtension.Present, 0, writeBuffer, index - 4, bufferUsed);
                            if (count > 0)
                            {
                                index += count - 4;

                                slicesInSequence++;
                            }
                        }
                        break;
                }
            }
        }

        public virtual void consume(Byte[] packet, int packetStart, int packetLength)
        {
            if (bufferUsed + packetLength > MPEG2WriteBufferLength)
                processBuffer();

            if (bufferUsed + packetLength > MPEG2WriteBufferLength)
                throw new Exception("Can not process video buffer, errors exist");

            // TODO : must find a better copy operation to use here
            for (int i = 0; i < packetLength; i++)
                writeBuffer[bufferUsed++] = packet[packetStart++];
        }

        private UInt16 read16(int index)
        {
            return (UInt16)((((UInt16)writeBuffer[index]) << 8) | ((UInt16)writeBuffer[index + 1]));
        }

        private UInt32 read32(int index)
        {
            return (((UInt32)writeBuffer[index + 0]) << 24) | (((UInt32)writeBuffer[index + 1]) << 16) | (((UInt32)writeBuffer[index + 2]) << 8) | ((UInt32)writeBuffer[index + 3]);
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
}
