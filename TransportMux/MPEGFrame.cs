namespace TransportMux
{
    using DVBToolsCommon;
    using System.Collections.Generic;

    public class MPEGFrame
    {
        public long  StartIndex = 0;
        public long EndIndex = 0;
        public long Length
        {
            get
            {
                return EndIndex - StartIndex + 1;
            }
        }

        public int TemporalReference;
        public char FrameType;
        public long FrameNumber;
        public long PresentationNumber;

        //public byte[] PESPacket = null;
        public ByteArray PESPacket = null;

        public double DTS;
        public double PTS;

        public double VBVDelay;

        public bool StartOfGOP = false;
    }

    public class MPEGFrames : List<MPEGFrame>
    {
    }
}
