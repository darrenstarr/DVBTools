namespace MultiplexerUI
{
    using DVBToolsCommon;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    public partial class StreamSelectionForm : Form
    {
        Form parentForm;
        TransportMux.TransportMultiplexer multiplexer = null;
        Timer progressTimer = new Timer();
        long displayBitRate = 0;

        public StreamSelectionForm(Form parent)
        {
            parentForm = parent;
            InitializeComponent();
            progressTimer.Interval = 100;
            progressTimer.Tick += new EventHandler(UpdateMultiplexProgress);
        }

        protected override void OnClosed(EventArgs e)
        {
            if(parentForm != null)
                parentForm.Show();

            base.OnClosed(e);
        }

        StreamListItem findByPID(ushort PID)
        {
            for (int i = 0; i < streamListView.Items.Count; i++)
            {
                StreamListItem item = (StreamListItem)streamListView.Items[i];
                if (item.PID == PID)
                    return item;
            }
            return null;
        }

        ushort nextUnusedPID()
        {
            ushort pid = 0x1E2;
            while (findByPID(pid) != null)
                pid++;
            return pid;
        }

        private void addStreamButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "All Asset Types|*.m2v;*.mpa;*.mpv;*.ac3;*.dvb.idx";
            fileDialog.RestoreDirectory = true;
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < fileDialog.FileNames.Length; i++)
                {
                    StreamListItem newItem = new StreamListItem(fileDialog.FileNames[i]);
                    if (newItem.StreamType == StreamListItem.StreamTypes.MPEG2Video)
                    {
                        newItem.PID = 0x1E1;
                        streamListView.Items.Insert(0, newItem);
                    }
                    else
                    {
                        newItem.PID = nextUnusedPID();
                        Regex expresion = new Regex(@"_[a-z][a-z][a-z]_");        // could be a language
                        Match m = expresion.Match(fileDialog.FileNames[i]);
                        if (m != null && m.Success)
                        {
                            string code = fileDialog.FileNames[i].Substring(m.Index + 1, m.Length - 2);
                            if (ISO639Table.IsV2Entry(code))
                                newItem.LanguageCode = code;
                        }

                        streamListView.Items.Add(newItem);

                    }
                }
            }

            displayBitRate = 0;
            for (int i = 0; i < streamListView.Items.Count; i++)
            {
                StreamListItem item = (StreamListItem)streamListView.Items[i];
                displayBitRate += item.BitRate * 125 / 100;
            }
            if (!enableForceBitrate.Checked)
                forceBitRateValue.Value = displayBitRate;
        }

        private void setLanguageButton_Click(object sender, EventArgs e)
        {
            if (streamListView.SelectedItems.Count == 0)
            {
                setLanguageButton.Enabled = false;
                return;
            }

            if (streamListView.SelectedItems.Count == 1 && ((StreamListItem)streamListView.SelectedItems[0]).StreamType == StreamListItem.StreamTypes.MPEG2Video)
            {
                setLanguageButton.Enabled = false;
                return;
            }

            string InitialLanguage = ((StreamListItem)streamListView.SelectedItems[0]).LanguageCode;
            for (int i = 1; i < streamListView.SelectedItems.Count; i++)
            {
                StreamListItem item = (StreamListItem)streamListView.SelectedItems[i];
                if (item.StreamType != StreamListItem.StreamTypes.MPEG2Video && InitialLanguage != ((StreamListItem)streamListView.SelectedItems[i]).LanguageCode)
                {
                    InitialLanguage = "unk";
                    break;
                }
            }

            DVBToolsCommonUI.SelectLanguageDialog languageDialog = new DVBToolsCommonUI.SelectLanguageDialog();
            languageDialog.LanguageCode = InitialLanguage;
            if (languageDialog.ShowDialog() != DialogResult.OK)
                return;

            for (int i = 0; i < streamListView.SelectedItems.Count; i++)
            {
                StreamListItem item = (StreamListItem)streamListView.SelectedItems[i];
                if (item.StreamType != StreamListItem.StreamTypes.MPEG2Video)
                    item.LanguageCode = languageDialog.LanguageCode;
            }
        }

        private void streamListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (streamListView.SelectedItems.Count == 0)
            {
                setLanguageButton.Enabled = false;
                deleteButton.Enabled = false;
                MoveDownButton.Enabled = false;
                MoveUpButton.Enabled = false;
                return;
            }

            deleteButton.Enabled = true;

            if (streamListView.SelectedItems.Count == 1 && ((StreamListItem)streamListView.SelectedItems[0]).StreamType == StreamListItem.StreamTypes.MPEG2Video)
            {
                setLanguageButton.Enabled = false;
                SetStreamDelayButton.Enabled = false;

                MoveDownButton.Enabled = false;
                MoveUpButton.Enabled = false;

                return;
            }

            setLanguageButton.Enabled = true;
            if (streamListView.SelectedIndices[0] == 0)
                SetStreamDelayButton.Enabled = false;
            else
                SetStreamDelayButton.Enabled = true;

            if (streamListView.SelectedItems.Count == 1)
            {
                StreamListItem item = (StreamListItem)streamListView.SelectedItems[0];
                if(item.PID > 0x1E2)
                    MoveUpButton.Enabled = true;
                else
                    MoveUpButton.Enabled = false;

                if (((StreamListItem)(streamListView.Items[streamListView.Items.Count - 1])).PID == item.PID)
                    MoveDownButton.Enabled = false;
                else
                    MoveDownButton.Enabled = true;
                return;
            }

            MoveUpButton.Enabled = false;
            MoveDownButton.Enabled = false;
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            multiplexer = new TransportMux.TransportMultiplexer();
            multiplexer.OutputFileName = outputFileName.Text;

            if (enableEndAfter.Checked)
            {
                long endAfter = (long) endAfterValue.Value;
                switch (endAfterUnit.Text.ToLower())
                {
                    case "milliseconds":
                        endAfter *= 27000;
                        break;
                    case "seconds":
                        endAfter *= 27000000;
                        break;
                    case "minutes":
                        endAfter *= (27000000 * 60);
                        break;
                    case "hours":
                        endAfter *= (long)((long)27000000 * (long)3600);
                        break;
                    default:
                        throw new Exception("Invalid value for \"endAfterUnits\"");
                }
                multiplexer.EndAfter = (ulong)endAfter;
            }

            uint totalBitRate = 0;
            for (int i = 0; i < streamListView.Items.Count; i++)
            {
                StreamListItem item = (StreamListItem)streamListView.Items[i];
                totalBitRate += item.BitRate * 125 / 100;
                multiplexer.AddStream(item.InputStream);
            }

            if (enableForceBitrate.Checked)
            {
                multiplexer.BitsPerSecond = (uint) forceBitRateValue.Value;
            }
            else
            {
                multiplexer.BitsPerSecond = totalBitRate;            
            }
            
            multiplexer.Run();

            progressTimer.Start();
        }

        private void UpdateMultiplexProgress(object sender, EventArgs e)
        {
            int pcrLength = (int)(multiplexer.PCRLength / 1024);
            int pcrPosition = (int)(multiplexer.PCRPosition / 1024);

            if (multiplexerProgress.Maximum != pcrLength || multiplexerProgress.Value != pcrPosition)
            {
                multiplexerProgress.Minimum = 0;
                multiplexerProgress.Maximum = pcrLength;
                multiplexerProgress.Value = pcrPosition;
            }

            if (multiplexer.Finished)
            {
                progressTimer.Stop();
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (streamListView.SelectedItems.Count == 0)
            {
                deleteButton.Enabled = false;
                return;
            }

            while(streamListView.SelectedItems.Count > 0)
                streamListView.Items.Remove(streamListView.SelectedItems[0]);

            deleteButton.Enabled = false;
        }

        private void selectOutputFileButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Transport Stream (*.ts)|*.ts";
            if (dialog.ShowDialog() == DialogResult.OK)
                outputFileName.Text = dialog.FileName;
        }

        private void enableEndAfter_CheckedChanged(object sender, EventArgs e)
        {
            if (enableEndAfter.Checked)
            {
                endAfterUnit.Enabled = true;
                endAfterValue.Enabled = true;
            }
            else
            {
                endAfterUnit.Enabled = false;
                endAfterValue.Enabled = false;
            }
        }

        private void enableForceBitrate_CheckedChanged(object sender, EventArgs e)
        {
            if (enableForceBitrate.Checked)
                forceBitRateValue.Enabled = true;
            else
                forceBitRateValue.Enabled = false;
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            if (streamListView.SelectedItems.Count != 1)
            {
                MoveUpButton.Enabled = false;
                MoveDownButton.Enabled = false;
            }

            int selectedItemIndex = streamListView.SelectedIndices[0];
            int swapItemIndex = selectedItemIndex + 1;

            if (swapItemIndex < streamListView.Items.Count)
            {
                StreamListItem itemA = (StreamListItem)streamListView.Items[selectedItemIndex];
                StreamListItem itemB = (StreamListItem)streamListView.Items[swapItemIndex];
                streamListView.Items.RemoveAt(swapItemIndex);
                streamListView.Items.Insert(selectedItemIndex, itemB);

                ushort PIDSwap = itemB.PID;
                itemB.PID = itemA.PID;
                itemA.PID = PIDSwap;
            }

            if (streamListView.SelectedItems.Count == 1)
            {
                StreamListItem item = (StreamListItem)streamListView.SelectedItems[0];
                if (item.PID > 0x1E2)
                    MoveUpButton.Enabled = true;
                else
                    MoveUpButton.Enabled = false;

                if (((StreamListItem)(streamListView.Items[streamListView.Items.Count - 1])).PID == item.PID)
                    MoveDownButton.Enabled = false;
                else
                    MoveDownButton.Enabled = true;
                return;
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            if (streamListView.SelectedItems.Count != 1)
            {
                MoveUpButton.Enabled = false;
                MoveDownButton.Enabled = false;
            }

            int selectedItemIndex = streamListView.SelectedIndices[0];
            int swapItemIndex = selectedItemIndex - 1;

            if (swapItemIndex > 0)
            {
                StreamListItem itemA = (StreamListItem)streamListView.Items[selectedItemIndex];
                StreamListItem itemB = (StreamListItem)streamListView.Items[swapItemIndex];
                streamListView.Items.RemoveAt(swapItemIndex);
                streamListView.Items.Insert(selectedItemIndex, itemB);

                ushort PIDSwap = itemB.PID;
                itemB.PID = itemA.PID;
                itemA.PID = PIDSwap;
            }

            if (streamListView.SelectedItems.Count == 1)
            {
                StreamListItem item = (StreamListItem)streamListView.SelectedItems[0];
                if (item.PID > 0x1E2)
                    MoveUpButton.Enabled = true;
                else
                    MoveUpButton.Enabled = false;

                if (((StreamListItem)(streamListView.Items[streamListView.Items.Count - 1])).PID == item.PID)
                    MoveDownButton.Enabled = false;
                else
                    MoveDownButton.Enabled = true;
                return;
            }
        }

        private void SetStreamDelayButton_Click(object sender, EventArgs e)
        {
            if (streamListView.SelectedIndices.Count == 0 || streamListView.SelectedIndices[0] == 0)
            {
                SetStreamDelayButton.Enabled = false;
                return;
            }

            int oldValue = 0;
            StreamListItem item = (StreamListItem)streamListView.SelectedItems[0];
            oldValue = item.StreamDelay;

            for (int i = 1; i < streamListView.SelectedItems.Count; i++)
            {
                item = (StreamListItem)streamListView.SelectedItems[i];
                if (item.StreamDelay != oldValue)
                {
                    oldValue = 0;
                    break;
                }
            }

            if (GetIntegerValueDialog.Show("Delay", "ms", 0, 4000, ref oldValue) == DialogResult.Cancel)
                return;

            for (int i = 0; i < streamListView.SelectedItems.Count; i++)
            {
                item = (StreamListItem)streamListView.SelectedItems[i];
                item.StreamDelay = oldValue;
            }
        }
    }

    class StreamListItem : ListViewItem
    {
        public enum StreamTypes
        {
            Undefined,
            MPEG2Video,
            MPEG1LayerIIAudio,
            AC3Audio,
            DVBSubtitle
        };

        StreamTypes streamType = StreamTypes.Undefined;
        public StreamTypes StreamType
        {
            get
            {
                return streamType;
            }
            set
            {
                streamType = value;
                SubItems[0].Text = streamTypeToString(value);
            }
        }

        ushort pid = 0x000;
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

        int streamDelay = 0;
        public int StreamDelay
        {
            get
            {
                return streamDelay;
            }
            set
            {
                if(streamType != StreamTypes.MPEG2Video)
                {
                    streamDelay = value;
                    InputStream.streamDelay = (double)value / 1000;
                    SubItems[5].Text = value.ToString() + "ms";
                }
            }
        }

        string languageCode = "unk";
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

        uint bitRate = 0;
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

        string streamTypeToString(StreamTypes value)
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
            }
            return "Undefined";
        }

        StreamTypes fileNameToType(string fileName)
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

        uint detectMPEGBitRate(string fileName)
        {
            DVBToolsCommon.VideoDecoders.MPEG2Decoder decoder = new DVBToolsCommon.VideoDecoders.MPEG2Decoder();
            return decoder.DetectBitrate(fileName);
        }

        public StreamListItem(string fileName)
            : base()
        {
            streamType = fileNameToType(fileName);
            Text = streamTypeToString(streamType);

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
                    InputStream = mpeg2VideoStream;
                    bitRate = detectMPEGBitRate(fileName);
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