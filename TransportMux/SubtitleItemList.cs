namespace TransportMux
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class SubtitleItemList : List<SubtitleItem>
    {
        public TimelineRegionList RegionList = new TimelineRegionList();

	    public SubtitleItemList(string fileName) : base()
	    {
            Load(fileName);
	    }

        private void Load(string fileName)
	    {
            FileInfo fileInfo = new FileInfo(fileName);
            if(!fileInfo.Exists)
                return;

            string filePath = fileInfo.Directory.FullName + @"\";

            FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(inputStream);

            SubtitleItem currentItem = new SubtitleItem();

            while(inputStream.Position < inputStream.Length)
            {
                string line = reader.ReadLine().Trim();

                if(line == string.Empty)
                    continue;

                if(line == "[Packet]")
                {
                    if(currentItem.IsValid)
                    {
                        RegionList.AddItemAt(currentItem);
                        Add(currentItem);
                        currentItem = new SubtitleItem();
                    }
                }
                else if(line.StartsWith("PresentationTime="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.PresentationTime = Convert.ToInt64(line.Substring(index));
                }
                else if(line.StartsWith("SourceFile="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.fileName = filePath + line.Substring(index);
                }
                else if(line.StartsWith("StartOffset="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.StartOffset = Convert.ToInt64(line.Substring(index));
                }
                else if(line.StartsWith("Length="))
                {
                    int index = line.IndexOf("=") + 1;
                    currentItem.Length = Convert.ToInt64(line.Substring(index));
                }
            }

            if (currentItem.IsValid)
            {
                RegionList.AddItemAt(currentItem);
                Add(currentItem);
            }

            RegionList.PadPresentationStamps(500);
            //RegionList.DumpLog("c:\\temp\\regionlist.csv");
	    }
    }
}
