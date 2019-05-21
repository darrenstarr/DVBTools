namespace MultiplexerUI
{
    using System;
    using System.Windows.Forms;

    public partial class GetIntegerValueDialog : Form
    {
        public int MaximumValue
        {
            get
            {
                return (int)ValueChooser.Maximum;
            }
            set
            {
                ValueChooser.Maximum = value;
            }
        }

        public int MinimumValue
        {
            get
            {
                return (int) ValueChooser.Minimum;
            }
            set
            {
                ValueChooser.Minimum = value;
            }
        }

        public int Value
        {
            get
            {
                return (int)ValueChooser.Value;
            }
            set
            {
                ValueChooser.Value = value;
            }
        }

        public string UnitsString
        {
            get
            {
                return UnitsLabel.Text;
            }
            set
            {
                UnitsLabel.Text = value;
            }
        }

        public GetIntegerValueDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        static public DialogResult Show(string Caption, string UnitsString, int MinimumValue, int MaximumValue, ref int Result)
        {
            GetIntegerValueDialog dialog = new GetIntegerValueDialog
            {
                Text = Caption,
                UnitsString = UnitsString,
                MinimumValue = MinimumValue,
                MaximumValue = MaximumValue,
                Value = Result
            };

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                Result = dialog.Value;
            return result;
        }
    }
}