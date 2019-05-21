using System;
using System.Collections.Generic;
using System.Text;

using DVBToolsCommon;

namespace TransportMux
{
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
