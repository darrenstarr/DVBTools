namespace TransportMux
{
    using System.Collections.Generic;

    public class InputStreams
    {
        private List<InputStream> streams = new List<InputStream>();
        private int NextStream = 0;

        public InputStream this[long index]
        {
            get
            {
                return streams[(int)index];
            }
        }

        public InputStream PcrStream
        {
            get
            {
                for(int i=0; i<streams.Count; i++)
                    if(streams[i].PCRStream)
                        return streams[i];

                return null;
            }
        }

        public long Count
        {
            get
            {
                return streams.Count;
            }
        }

        public void Add(InputStream stream)
        {
            streams.Add(stream);
        }

        public ulong NextPacketTime
        {
            get
            {
                if(streams.Count == 0)
                    return 0xFFFFFFFFFFFFFFFF;

                ulong result = 0xFFFFFFFFFFFFFFFF;
                for(int i=0; i<streams.Count; i++)
                {
                    if(streams[i].NextPacketTime < result)
                        result = streams[i].NextPacketTime;
                }

                return result;
            }
        }

        public TransportPacket TakePacket(ulong currentTime)
        {
            if (streams.Count == 0)
                return null;

            if (streams.Count == 1)
            {
                if (currentTime < streams[0].NextPacketTime)
                    return null;

                return streams[0].TakePacket();
            }

            int index = 0;
            int i = NextStream;
            while (streams[i].NextPacketTime > currentTime)
            {
                i++;
                if (i >= streams.Count)
                    i = 0;

                index++;
                if (index >= streams.Count)
                    return null;
            }
            NextStream = i + 1;
            if (NextStream >= streams.Count)
                NextStream = 0;

            return streams[i].TakePacket();
        }

        public ushort PcrPID
        {
            get
            {
                for(int i=0; i<streams.Count; i++)
                    if(streams[i].PCRStream)
                        return streams[i].PID;

                return 0x000;
            }
        }
    }
}
