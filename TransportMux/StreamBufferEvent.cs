using System;
using System.Collections.Generic;
using System.Text;

namespace TransportMux
{
    public class StreamBufferEvent
    {
        public StreamBufferEvent(int amount, double processAt)
        {
            Amount = amount;
            ProcessAt1Hz = processAt;
        }

        public StreamBufferEvent(int amount, long processAt)
        {
            Amount = amount;
            ProcessAt = processAt;
        }

        public int Amount = 0;
        public long ProcessAt = 0;
        public double ProcessAt1Hz
        {
            get
            {
                return (double)ProcessAt / 27000000;
            }
            set
            {
                ProcessAt = (long)(value * 27000000);
            }
        }
        public long ProcessAt90Khz
        {
            get
            {
                return ProcessAt / 300;
            }
            set
            {
                ProcessAt = value * 300;
            }
        }
    }
}
