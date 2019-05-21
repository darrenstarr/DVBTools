namespace MultiplexerUI
{
    partial class StreamSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.streamListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.addStreamButton = new System.Windows.Forms.Button();
            this.setLanguageButton = new System.Windows.Forms.Button();
            this.goButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.outputFileName = new System.Windows.Forms.TextBox();
            this.selectOutputFileButton = new System.Windows.Forms.Button();
            this.multiplexerProgress = new System.Windows.Forms.ProgressBar();
            this.enableEndAfter = new System.Windows.Forms.CheckBox();
            this.endAfterValue = new System.Windows.Forms.NumericUpDown();
            this.endAfterUnit = new System.Windows.Forms.ComboBox();
            this.enableForceBitrate = new System.Windows.Forms.CheckBox();
            this.forceBitRateValue = new System.Windows.Forms.NumericUpDown();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.MoveDownButton = new System.Windows.Forms.Button();
            this.SetStreamDelayButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.endAfterValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.forceBitRateValue)).BeginInit();
            this.SuspendLayout();
            // 
            // streamListView
            // 
            this.streamListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.streamListView.FullRowSelect = true;
            this.streamListView.GridLines = true;
            this.streamListView.HideSelection = false;
            this.streamListView.Location = new System.Drawing.Point(12, 25);
            this.streamListView.Name = "streamListView";
            this.streamListView.Size = new System.Drawing.Size(584, 185);
            this.streamListView.TabIndex = 0;
            this.streamListView.UseCompatibleStateImageBehavior = false;
            this.streamListView.View = System.Windows.Forms.View.Details;
            this.streamListView.SelectedIndexChanged += new System.EventHandler(this.streamListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 85;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "PID";
            this.columnHeader2.Width = 50;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Language";
            this.columnHeader3.Width = 80;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Bitrate";
            this.columnHeader4.Width = 78;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "File name";
            this.columnHeader5.Width = 198;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Delay (MS)";
            this.columnHeader6.Width = 68;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Streams";
            // 
            // addStreamButton
            // 
            this.addStreamButton.Location = new System.Drawing.Point(12, 216);
            this.addStreamButton.Name = "addStreamButton";
            this.addStreamButton.Size = new System.Drawing.Size(75, 23);
            this.addStreamButton.TabIndex = 2;
            this.addStreamButton.Text = "&Add";
            this.addStreamButton.UseVisualStyleBackColor = true;
            this.addStreamButton.Click += new System.EventHandler(this.addStreamButton_Click);
            // 
            // setLanguageButton
            // 
            this.setLanguageButton.Enabled = false;
            this.setLanguageButton.Location = new System.Drawing.Point(174, 216);
            this.setLanguageButton.Name = "setLanguageButton";
            this.setLanguageButton.Size = new System.Drawing.Size(99, 23);
            this.setLanguageButton.TabIndex = 3;
            this.setLanguageButton.Text = "Set &Language";
            this.setLanguageButton.UseVisualStyleBackColor = true;
            this.setLanguageButton.Click += new System.EventHandler(this.setLanguageButton_Click);
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(521, 587);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(75, 23);
            this.goButton.TabIndex = 4;
            this.goButton.Text = "&Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(93, 216);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 5;
            this.deleteButton.Text = "&Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 282);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "&Output file name :";
            // 
            // outputFileName
            // 
            this.outputFileName.Location = new System.Drawing.Point(111, 279);
            this.outputFileName.Name = "outputFileName";
            this.outputFileName.Size = new System.Drawing.Size(447, 20);
            this.outputFileName.TabIndex = 7;
            // 
            // selectOutputFileButton
            // 
            this.selectOutputFileButton.Location = new System.Drawing.Point(564, 277);
            this.selectOutputFileButton.Name = "selectOutputFileButton";
            this.selectOutputFileButton.Size = new System.Drawing.Size(32, 23);
            this.selectOutputFileButton.TabIndex = 8;
            this.selectOutputFileButton.Text = "...";
            this.selectOutputFileButton.UseVisualStyleBackColor = true;
            this.selectOutputFileButton.Click += new System.EventHandler(this.selectOutputFileButton_Click);
            // 
            // multiplexerProgress
            // 
            this.multiplexerProgress.Location = new System.Drawing.Point(12, 558);
            this.multiplexerProgress.Name = "multiplexerProgress";
            this.multiplexerProgress.Size = new System.Drawing.Size(584, 23);
            this.multiplexerProgress.TabIndex = 9;
            // 
            // enableEndAfter
            // 
            this.enableEndAfter.AutoSize = true;
            this.enableEndAfter.Location = new System.Drawing.Point(12, 346);
            this.enableEndAfter.Name = "enableEndAfter";
            this.enableEndAfter.Size = new System.Drawing.Size(75, 17);
            this.enableEndAfter.TabIndex = 10;
            this.enableEndAfter.Text = "End after :";
            this.enableEndAfter.UseVisualStyleBackColor = true;
            this.enableEndAfter.CheckedChanged += new System.EventHandler(this.enableEndAfter_CheckedChanged);
            // 
            // endAfterValue
            // 
            this.endAfterValue.Enabled = false;
            this.endAfterValue.Location = new System.Drawing.Point(93, 345);
            this.endAfterValue.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.endAfterValue.Name = "endAfterValue";
            this.endAfterValue.Size = new System.Drawing.Size(51, 20);
            this.endAfterValue.TabIndex = 11;
            this.endAfterValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // endAfterUnit
            // 
            this.endAfterUnit.Enabled = false;
            this.endAfterUnit.FormattingEnabled = true;
            this.endAfterUnit.Items.AddRange(new object[] {
            "Milliseconds",
            "Seconds",
            "Minutes",
            "Hours"});
            this.endAfterUnit.Location = new System.Drawing.Point(152, 344);
            this.endAfterUnit.Name = "endAfterUnit";
            this.endAfterUnit.Size = new System.Drawing.Size(98, 21);
            this.endAfterUnit.TabIndex = 12;
            this.endAfterUnit.Text = "Minutes";
            // 
            // enableForceBitrate
            // 
            this.enableForceBitrate.AutoSize = true;
            this.enableForceBitrate.Location = new System.Drawing.Point(12, 371);
            this.enableForceBitrate.Name = "enableForceBitrate";
            this.enableForceBitrate.Size = new System.Drawing.Size(91, 17);
            this.enableForceBitrate.TabIndex = 13;
            this.enableForceBitrate.Text = "Force bitrate :";
            this.enableForceBitrate.UseVisualStyleBackColor = true;
            this.enableForceBitrate.CheckedChanged += new System.EventHandler(this.enableForceBitrate_CheckedChanged);
            // 
            // forceBitRateValue
            // 
            this.forceBitRateValue.Enabled = false;
            this.forceBitRateValue.Increment = new decimal(new int[] {
            250000,
            0,
            0,
            0});
            this.forceBitRateValue.Location = new System.Drawing.Point(109, 370);
            this.forceBitRateValue.Maximum = new decimal(new int[] {
            15000000,
            0,
            0,
            0});
            this.forceBitRateValue.Name = "forceBitRateValue";
            this.forceBitRateValue.Size = new System.Drawing.Size(88, 20);
            this.forceBitRateValue.TabIndex = 14;
            this.forceBitRateValue.ThousandsSeparator = true;
            this.forceBitRateValue.Value = new decimal(new int[] {
            4500000,
            0,
            0,
            0});
            // 
            // MoveUpButton
            // 
            this.MoveUpButton.Enabled = false;
            this.MoveUpButton.Location = new System.Drawing.Point(279, 216);
            this.MoveUpButton.Name = "MoveUpButton";
            this.MoveUpButton.Size = new System.Drawing.Size(83, 23);
            this.MoveUpButton.TabIndex = 15;
            this.MoveUpButton.Text = "Move &Up";
            this.MoveUpButton.UseVisualStyleBackColor = true;
            this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
            // 
            // MoveDownButton
            // 
            this.MoveDownButton.Enabled = false;
            this.MoveDownButton.Location = new System.Drawing.Point(368, 216);
            this.MoveDownButton.Name = "MoveDownButton";
            this.MoveDownButton.Size = new System.Drawing.Size(83, 23);
            this.MoveDownButton.TabIndex = 16;
            this.MoveDownButton.Text = "Move D&own";
            this.MoveDownButton.UseVisualStyleBackColor = true;
            this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
            // 
            // SetStreamDelayButton
            // 
            this.SetStreamDelayButton.Enabled = false;
            this.SetStreamDelayButton.Location = new System.Drawing.Point(458, 217);
            this.SetStreamDelayButton.Name = "SetStreamDelayButton";
            this.SetStreamDelayButton.Size = new System.Drawing.Size(115, 23);
            this.SetStreamDelayButton.TabIndex = 17;
            this.SetStreamDelayButton.Text = "Set Stream Dela&y";
            this.SetStreamDelayButton.UseVisualStyleBackColor = true;
            this.SetStreamDelayButton.Click += new System.EventHandler(this.SetStreamDelayButton_Click);
            // 
            // StreamSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 622);
            this.Controls.Add(this.SetStreamDelayButton);
            this.Controls.Add(this.MoveDownButton);
            this.Controls.Add(this.MoveUpButton);
            this.Controls.Add(this.forceBitRateValue);
            this.Controls.Add(this.enableForceBitrate);
            this.Controls.Add(this.endAfterUnit);
            this.Controls.Add(this.endAfterValue);
            this.Controls.Add(this.enableEndAfter);
            this.Controls.Add(this.multiplexerProgress);
            this.Controls.Add(this.selectOutputFileButton);
            this.Controls.Add(this.outputFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.setLanguageButton);
            this.Controls.Add(this.addStreamButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.streamListView);
            this.Name = "StreamSelectionForm";
            this.Text = "StreamSelectionForm";
            ((System.ComponentModel.ISupportInitialize)(this.endAfterValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.forceBitRateValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView streamListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button addStreamButton;
        private System.Windows.Forms.Button setLanguageButton;
        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox outputFileName;
        private System.Windows.Forms.Button selectOutputFileButton;
        private System.Windows.Forms.ProgressBar multiplexerProgress;
        private System.Windows.Forms.CheckBox enableEndAfter;
        private System.Windows.Forms.NumericUpDown endAfterValue;
        private System.Windows.Forms.ComboBox endAfterUnit;
        private System.Windows.Forms.CheckBox enableForceBitrate;
        private System.Windows.Forms.NumericUpDown forceBitRateValue;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Button MoveDownButton;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Button SetStreamDelayButton;
    }
}