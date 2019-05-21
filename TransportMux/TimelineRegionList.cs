namespace TransportMux
{
    using System.Collections.Generic;
    using System.IO;

    internal class TimelineRegionList : List<TimelineRegion>
    {
        public const int BitRate = 192000;
        public const int BitsPerByte = 8;
        public const int ByteRate = BitRate / BitsPerByte;
        public const int PacketSize = 188;
        public const int PacketHeaderSize = 4;
        public const int PacketPayloadSize = PacketSize - PacketHeaderSize;
        public const int PESHeaderSize = 14;
        public const int PESPacketPayloadSize = PacketPayloadSize - PESHeaderSize;
        private int ContinuityCounter = 1;

        public TransportPackets ReadMorePackets(long InitialPTS, ushort PID)
        {
            TransportPackets result = new TransportPackets();
            if (Count == 0)
                return result;
                
            result.Append(this[0].Packetize(InitialPTS, PID, ref ContinuityCounter));
            RemoveAt(0);

            return result;
        }

        /// <summary>
        /// Evaluates the list an inserts new items with PTS stamps when the interval has been reached
        /// </summary>
        /// <param name="msInterval"></param>
        public void PadPresentationStamps(long msInterval)
        {
            int i = 0;
            while (i < (Count - 1))
            {
                TimelineRegion regionA = this[i];
                TimelineRegion regionB = this[i + 1];
                if ((regionA.Milliseconds + msInterval) < regionB.Milliseconds)
                {
                    TimelineRegion newRegion = new TimelineRegion();
                    newRegion.ItemLength = 12;
                    newRegion.GeneratePTSPacket = true;
                    newRegion.Milliseconds = regionA.Milliseconds + msInterval;
                    newRegion.PacketStart = newRegion.ExpectedStartPacket;
                    if (regionA.Overlaps(newRegion) || regionB.Overlaps(newRegion))
                        InsertItemAt(newRegion);
                    else
                        Insert(i + 1, newRegion);
                    continue;
                }
                i++;
            }
        }

        public void DumpLog(string fileName)
        {
            FileStream logStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(logStream);

            for (int i = 0; i < Count; i++)
            {
                TimelineRegion region = this[i];
                region.DumpCSV(writer);
            }

            writer.Close();
            logStream.Close();
        }

        public void AddItemAt(SubtitleItem item)
        {
            TimelineRegion newRegion = new TimelineRegion();
            newRegion.ItemLength = (int) item.Length;
            newRegion.SourceFile = item.fileName;
            newRegion.SourceFileOffset = item.StartOffset;
            newRegion.Milliseconds = item.PresentationTime;
            newRegion.PacketStart = newRegion.ExpectedStartPacket;

            if (Count == 0)
            {                
                Add(newRegion);
                return;
            }

            InsertItemAt(newRegion);
        }

        /// <summary>
        /// Returns the first region that has been found which overlaps the given region to be inserted
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private TimelineRegion OverlappingRegion(TimelineRegion region)
        {
            for (int i = 0; i < Count; i++)
            {
                TimelineRegion current = this[i];
                if (current.Overlaps(region))
                    return current;                
            }
            return null;
        }

        /// <summary>
        /// Returns the first region that has been found which overlaps the given region to be inserted
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private TimelineRegion OverlappingRegion(long startPacket, long endPacket)
        {
            for (int i = 0; i < Count; i++)
            {
                TimelineRegion current = this[i];
                if (current.Overlaps(startPacket, endPacket))
                    return current;
            }
            return null;
        }

        private TimelineRegion RegionAtPacket(long packetNumber)
        {
            for (int i = 0; i < Count; i++)
            {
                TimelineRegion current = this[i];
                if (current.Contains(packetNumber))
                    return current;
            }
            return null;
        }

        private void MoveItemBack(TimelineRegion item, long newEndPacket)
        {
            long newStartPacket = newEndPacket - item.PacketCount + 1;
            TimelineRegion overlap = OverlappingRegion(newStartPacket, newEndPacket);
            while (overlap != null)
            {
                MoveItemBack(overlap, newStartPacket - 1);
                //System.Diagnostics.Debug.WriteLine("Moving back recursively an item which was already there to fit a new item in");
                overlap = OverlappingRegion(newStartPacket, newEndPacket);
            }

            item.PacketEnd = newEndPacket;
        }

        private void InsertItemAt(TimelineRegion region)
        {
            // Detect an overlap with the current insert position
            TimelineRegion overlap = OverlappingRegion(region);
            while(overlap != null)
            {
                // Found an overlap, try to correct it
                // If the region we found should present after this one, then we should shift our packet time back to compensate
                // for it, this will make it so that both packets are delivered in time.
                if (overlap.PresentationTimeStamp > region.PresentationTimeStamp)
                {
                    //System.Diagnostics.Debug.WriteLine("Moving current item back to compensate for a later item");
                    region.PacketEnd = overlap.PacketStart - 1;
                }
                else if (overlap.PresentationTimeStamp < region.PresentationTimeStamp)
                {
                    // If the region we found should present before the new region, then the overlapping region
                    // should be shifted back to make sure we can deliver this region in time.
                    System.Diagnostics.Debug.WriteLine("Moving back an item which was already there to fit a new item in");
                    MoveItemBack(overlap, region.PacketStart - 1);
                }
                else
                {
                    //throw new Exception("Don't know what to do when two packets have the same presentation time");
                    break;
                }

                overlap = OverlappingRegion(region);
            }

            TimelineRegion regionBefore = this[0];
            for (int i = 1; i < Count; i++)
            {
                TimelineRegion regionAfter = this[i];
                if (region > regionBefore && region < regionAfter)
                {
                    Insert(i, region);
                    return;
                }
                regionBefore = regionAfter;
            }
            Add(region);
        }
    }
}
