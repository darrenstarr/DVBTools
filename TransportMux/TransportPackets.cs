namespace TransportMux
{
    using System.Collections.Generic;

    public class TransportPackets
    {
        private List<TransportPacket> packetList = new List<TransportPacket>();
        public int Count = 0;

        public void Append(TransportPackets packets)
        {
            while (packets.Count > 0)
            {
                packetList.Add(packets.TakeFirst());
                Count++;
            }
        }

        public TransportPacket this[int index]
        {
            get
            {
                return packetList[index];
            }
        }

        public TransportPacket TakeFirst()
        {
            if (Count == 0)
                return null;

            TransportPacket result = packetList[0];
            Count--;
            packetList.RemoveAt(0);

            return result;
        }

        public void AddPacket(TransportPacket packet)
        {
            Count++;
            packetList.Add(packet);
        }

        public TransportPacket Last()
        {
            if (Count == 0)
                return null;

            TransportPacket result = packetList[Count - 1];
            return result;
        }

        public void RemoveLast()
        {
            if (Count == 0)
                return;
            packetList.RemoveAt(Count - 1);
            Count--;
        }

        /// <summary>
        /// Since the PID might not be assigned prior to buffering (such as in the case of the MPEG2 video stream implementation)
        /// this function changes all the pids of the packets in the buffer to the specified PID
        /// </summary>
        /// <param name="pid">The new pid to use</param>
        public void ChangePID(ushort pid)
        {
            for (int i = 0; i < Count; i++)
            {
                packetList[i].PID = pid;
            }
        }
    }
}
