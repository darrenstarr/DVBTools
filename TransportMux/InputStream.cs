namespace TransportMux
{
    using DVBToolsCommon;
    using System;

    public abstract class InputStream
    {
        public virtual long StreamLength
        {
            get
            {
                throw new Exception("Shouldn't call StreamLength() on non-PCR streams");
            }
        }

        public virtual long Position
        {
            get
            {
                throw new Exception("Shouldn't call exception on non-PCR streams");
            }
        }

        public abstract void Close();

        public abstract TransportPacket TakePacket();

        public abstract ulong NextPacketTime
        {
            get;
        }

        public abstract void GenerateProgramMap(ByteArray Map);
        
        public virtual bool PCRStream
        {
            get
            {
                return false;
            }
        }

        private ushort pid;
        public virtual ushort PID
        {
            get
            {
                return pid;
            }
            set
            {
                pid = value;
            }
        }

        public double streamDelay = 0;
        private double initialPTS = 0;
        public virtual double InitialPTS
        {
            get
            {
                return initialPTS;
            }
            set
            {
                initialPTS = value;
            }
        }
    }
}
