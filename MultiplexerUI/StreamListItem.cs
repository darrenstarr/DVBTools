namespace MultiplexerUI
{
    using DVBToolsCommon;
    using System;
    using System.IO;
    using System.Windows.Forms;

    internal class StreamListItem : ListViewItem
    {
        public enum StreamTypes
        {
            Undefined,
            MPEG2Video,
            MPEG1LayerIIAudio,
            AC3Audio,
            DVBSubtitle,
            MPEG1Video
        };

        private StreamTypes streamType = StreamTypes.Undefined;
        public StreamTypes StreamType
        {
            get
            {
                return streamType;
            }
            set
            {
                streamType = value;
                SubItems[0].Text = StreamTypeToString(value);
            }
        }

        private ushort pid = 0x000;
        public ushort PID
        {
            get
            {
                return pid;
            }
            set
            {
                pid = value;
                InputStream.PID = value;
                SubItems[1].Text = string.Format("0x{0:X3}", value);
            }
        }

        private int streamDelay = 0;
        public int StreamDelay
        {
            get
            {
                return streamDelay;
            }
            set
            {
                if((streamType != StreamTypes.MPEG2Video) && (streamType != StreamTypes.MPEG1Video))
                {
                    streamDelay = value;
                    InputStream.streamDelay = (double)value / 1000;
                    SubItems[5].Text = value.ToString() + "ms";
                }
            }
        }

        private string languageCode = "unk";
        public string LanguageCode
        {
            get
            {
                return languageCode;
            }
            set
            {
                languageCode = value;
                SubItems[2].Text = ISO639Table.V2ToLanguage(value);
                switch (streamType)
                {
                    case StreamTypes.AC3Audio:
                        ((TransportMux.AC3Stream)InputStream).LanguageCode = value;
                        break;
                    case StreamTypes.MPEG1LayerIIAudio:
                        ((TransportMux.MPEGAudioStream)InputStream).LanguageCode = value;
                        break;
                    case StreamTypes.DVBSubtitle:
                        ((TransportMux.SubtitleStream)InputStream).LanguageCode = value;
                        break;
                }
            }
        }

        private uint bitRate = 0;
        public uint BitRate
        {
            get
            {
                return bitRate;
            }
            set
            {
                bitRate = value;
                SubItems[3].Text = bitRate.ToString();
            }
        }

        public string fileName = "";
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                FileInfo info = new FileInfo(value);
                SubItems[4].Text = info.Name;
            }
        }

        public StreamListItem()
            : base()
        {
            Text = "Undefined";
            SubItems.Add("0x000");
            SubItems.Add("Unknown");
            SubItems.Add("0");
            SubItems.Add("");
        }

        private string StreamTypeToString(StreamTypes value)
        {
            switch (value)
            {
                case StreamTypes.MPEG2Video:
                    return "MPEG-2 Video";
                case StreamTypes.MPEG1LayerIIAudio:
                    return "MPEG-1 Layer II Audio";
                case StreamTypes.AC3Audio:
                    return "Dolby AC-3 Audio";
                case StreamTypes.DVBSubtitle:
                    return "DVB Subtitle";
                case StreamTypes.MPEG1Video:
                    return "MPEG-1 Video";
            }
            return "Undefined";
        }

        private StreamTypes FileNameToType(string fileName)
        {
            FileInfo info = new FileInfo(fileName);
            if (!info.Exists)
                throw new Exception("Specified file does not exist");

            string extension = info.Extension.ToLower();
            switch (extension)
            {
                case ".m2v":
                case ".mpv":
                    return StreamTypes.MPEG2Video;
                case ".ac3":
                    return StreamTypes.AC3Audio;
                case ".mpa":
                    return StreamTypes.MPEG1LayerIIAudio;
                case ".idx":
                    return StreamTypes.DVBSubtitle;
            }
            return StreamTypes.Undefined;
        }

        public TransportMux.InputStream InputStream = null;

        private uint DetectMPEGBitRate(string fileName)
        {
            DVBToolsCommon.VideoDecoders.MPEG2Decoder decoder = new DVBToolsCommon.VideoDecoders.MPEG2Decoder();
            return decoder.DetectBitrate(fileName);
        }

        public StreamListItem(string fileName)
            : base()
        {
            streamType = FileNameToType(fileName);
            Text = StreamTypeToString(streamType);

            TransportMux.AC3Stream ac3Stream = null;
            TransportMux.MPEG2VideoStream mpeg2VideoStream = null;
            TransportMux.SubtitleStream subtitleStream = null;
            TransportMux.MPEGAudioStream mpegAudioStream = null;

            switch (streamType)
            {
                case StreamTypes.AC3Audio:
                    ac3Stream = new TransportMux.AC3Stream(fileName);
                    InputStream = ac3Stream;
                    bitRate = ac3Stream.BitRate * 1000;
                    break;
                case StreamTypes.MPEG2Video:
                    mpeg2VideoStream = new TransportMux.MPEG2VideoStream(fileName);
                    if (!mpeg2VideoStream.SequenceExtensionPresent)
                        Text = StreamTypeToString(StreamTypes.MPEG1Video);

                    InputStream = mpeg2VideoStream;
                    bitRate = DetectMPEGBitRate(fileName);
                    break;
                case StreamTypes.MPEG1LayerIIAudio:
                    mpegAudioStream = new TransportMux.MPEGAudioStream(fileName);
                    InputStream = mpegAudioStream;
                    bitRate = (uint)mpegAudioStream.BitRate * 1000;
                    break;
                case StreamTypes.DVBSubtitle:
                    subtitleStream = new TransportMux.SubtitleStream(fileName);
                    InputStream = subtitleStream;
                    bitRate = 192000;
                    break;
                case StreamTypes.Undefined:
                    throw new Exception("unknown stream type encountered");
            }

            SubItems.Add("0x000");
            SubItems.Add("Unknown");
            SubItems.Add(bitRate.ToString());
            SubItems.Add("");

            if (streamType == StreamTypes.MPEG2Video)
                SubItems.Add("");
            else
                SubItems.Add("0ms");

            FileName = fileName;
        }
    }
}