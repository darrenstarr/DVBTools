namespace TransportMux
{
    using System;
    using System.IO;

    public class TransportMultiplexer
    {
        public static TransportMultiplexer FirstInstance = null;

        public System.Threading.Thread workerThread;
        public bool Failed = false;
        public bool Finished = false;
        public bool PadToConstant = true;

        public ushort ProgramMapPID = 0x1E0;
        public ushort ProgramNumber = 0x2;
        private InputStream pcrStream = null;

        public InputStreams Streams = new InputStreams();
        public ulong BitsPerSecond = 6000000;
        public ulong EndAfter = 0xFFFFFFFFFFFFFFFF;

        public string OutputFileName
        {
            get
            {
                return outputFileName;
            }
            set
            {
                Open(value);
            }
        }

        private string outputFileName = "";
        private TransportPacket NullPacket;
        private FileStream outputFileStream = null;
        private BinaryWriter writer = null;
        private ProgramTables ProgramTables = new ProgramTables();
        private System.Threading.Mutex cancelMutex;
        private bool cancelled = false;

        private bool Cancelled
        {
            get
            {
                cancelMutex.WaitOne();
                bool result =  cancelled;
                cancelMutex.ReleaseMutex();
                return result;
            }
            set
            {
                cancelMutex.WaitOne();
                cancelled = value;
                cancelMutex.ReleaseMutex();
            }
        }

        public long PCRLength
        {
            get
            {
                if (pcrStream != null)
                    return pcrStream.StreamLength;
                return 0;
            }
        }

        public long PCRPosition
        {
            get
            {
                if (pcrStream != null)
                    return pcrStream.Position;
                return 0;
            }
        }

        public TransportMultiplexer()
        {
            if (FirstInstance == null)
                FirstInstance = this;

            NullPacket = new TransportPacket();
            NullPacket.ConstructNullPacket();
        }

        ~TransportMultiplexer()
        {
            Close();
        }

        public void AddStream(InputStream stream)
        {
            Streams.Add(stream);
            ProgramTables.GenerateProgramMap(ProgramMapPID, ProgramNumber, Streams);
            if (stream.PCRStream)
                pcrStream = stream;
        }

        public bool Open(string fileName)
        {
            Close();
            try
            {
                outputFileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 10 * 1024 * 1024, false);
                writer = new BinaryWriter(outputFileStream);
            }
            catch
            {
                Close();
                outputFileName = "";
                return false;
            }
            finally
            {
                outputFileName = fileName;
            }
            return true; 
        }

        public void Close()
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }

            if (outputFileStream != null)
            {
                outputFileStream.Close();
                outputFileStream = null;
            }

            for (int i = 0; i < Streams.Count; i++)
                Streams[i].Close();
        }

        public bool Run()
        {
            if (outputFileName == string.Empty)
                return false;

            // Prepare the stream delays
            MPEG2VideoStream videoStream = (MPEG2VideoStream)Streams.PcrStream;
            if (videoStream == null)
                throw new Exception("No PCR stream specified");

            ushort pcrPID = videoStream.PID;
            double initialPTS = videoStream.InitialPTS;
            for(int i=0; i<Streams.Count; i++)
            {
                InputStream stream = Streams[i];
                if (stream.PID != pcrPID)
                    stream.InitialPTS = initialPTS;
            }

            cancelMutex = new System.Threading.Mutex();
            workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadRun));
            workerThread.Start();
            return true;
        }

        public void Cancel()
        {
            if (workerThread == null)
                return;

            if (workerThread.ThreadState == System.Threading.ThreadState.Running)
            {
                Cancelled = true;
                workerThread.Join(3000);                
            }

            if (workerThread.ThreadState == System.Threading.ThreadState.Running)
            {
                workerThread.Abort();
            }

            cancelMutex = null;
            workerThread = null;
        }

        public ulong currentTime;

        private void ThreadRun()
        {
            if (writer == null)
            {
                Failed = true;                
                return;
            }

            double packetsPerSecond = (double) (BitsPerSecond / 8) / 188;
	        ulong packetTime = 27000000 / (ulong) packetsPerSecond;
	        currentTime = 0;

            ulong nextPacketTime = Streams.NextPacketTime;
	        while(nextPacketTime != 0xFFFFFFFFFFFFFFFF && currentTime < EndAfter)
	        {
		        if(currentTime >= ProgramTables.NextPacketTime)
		        {
                    // Checking for cancel here means that the mutex on the cancel bool will only be triggered about
                    // 20 times per second of video processed. This theoretically can cause a mutex lock as many as
                    // 1000 times a second if I ever tweak the buffer reader on the MPEG-2 video stream to handle
                    // peeking more efficiently
                    if (Cancelled)
                        break;

                    writer.Write(ProgramTables.NextPacket().Packet, 0, 188);
			        currentTime += packetTime;
		        }
		        else if(currentTime >= nextPacketTime)
		        {
                    TransportPacket nextPacket = Streams.TakePacket(currentTime);
			        if(nextPacket.HasPCR)
				        nextPacket.SetPCR(currentTime);

			        if(nextPacket.DecoderStamp < currentTime)
                        throw new Exception(nextPacket.PID.ToString("{0:X4}") + " Decoder Stamp " + nextPacket.DecoderStamp.ToString() + " > current time " + currentTime.ToString());

                    writer.Write(nextPacket.Packet, 0, 188);

                    nextPacketTime = Streams.NextPacketTime;
			        currentTime += packetTime;
		        }
        		else
		        {
                    if (PadToConstant)
                    {
                        NullPacket.IncrementContinuityCounter();
                        writer.Write(NullPacket.Packet, 0, 188);
                    }

			        currentTime += packetTime;
		        }
	        }

            Close();
            Finished = true;
            return;
        }
    }
}
