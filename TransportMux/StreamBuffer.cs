namespace TransportMux
{
    using System.Collections.Generic;

    public class StreamBuffer
    {
        public int bufferLength = 0;
        public List<StreamBufferEvent> buffer = new List<StreamBufferEvent>();

        public long Clock = 0;

        public int BufferSize = 2592;

        public bool CanAdd(int Amount)
        {
            if ((bufferLength + Amount) > BufferSize)
                return false;

            return true;
        }

        public void AddEvent(int Amount, long ProcessAt)
        {
            if (ProcessAt <= Clock)
            {
                StreamBufferEvent bevent = new StreamBufferEvent(Amount, ProcessAt);
                bufferLength += Amount;
            }
            else
                AddEvent(new StreamBufferEvent(Amount, ProcessAt));
        }

        public void AddEvent(StreamBufferEvent bufferEvent)
        {
            if (bufferEvent.ProcessAt <= Clock)
                bufferLength += bufferEvent.Amount;
            else
            {
                if (buffer.Count == 0)
                    buffer.Add(bufferEvent);
                else
                {
                    int index = buffer.Count - 1;
                    while (index > 0 && buffer[index].ProcessAt > bufferEvent.ProcessAt)
                        index--;

                    if (index == buffer.Count - 1)
                        buffer.Add(bufferEvent);
                    else
                    {
                        if (buffer[index].ProcessAt < bufferEvent.ProcessAt)
                            index++;
                        buffer.Insert(index, bufferEvent);
                    }
                }
            }
        }

        public void SetTime(long clock)
        {
            Clock = clock;

            while (buffer.Count > 0 && buffer[0].ProcessAt <= clock)
            {
                bufferLength += buffer[0].Amount;
                buffer.RemoveAt(0);
            }
        }

        public long NextEventTime
        {
            get
            {
                if (buffer.Count == 0)
                    return -1;

                return buffer[0].ProcessAt;
            }
        }
    }
}
