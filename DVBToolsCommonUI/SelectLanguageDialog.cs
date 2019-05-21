namespace DVBToolsCommonUI
{
    using DVBToolsCommon;
    using System;
    using System.Windows.Forms;

    public partial class SelectLanguageDialog : Form
    {
        private LanguageCodeItem selectedItem = null;

        public string Language
        {
            get
            {
                if (selectedItem == null)
                    return "";
                return selectedItem.Language;
            }
            set
            {
                LocateLanguage(value);
            }
        }

        public string LanguageCode
        {
            get
            {
                if (selectedItem == null)
                    return "";
                return selectedItem.LanguageCode;
            }
            set
            {
                LocateLanguageCode(value);
            }
        }

        public SelectLanguageDialog()
        {
            InitializeComponent();
            FillList();
        }

        private void LocateLanguage(string language)
        {
            for (int i = 0; i < languageCodeList.Items.Count; i++)
            {
                LanguageCodeItem item = (LanguageCodeItem) languageCodeList.Items[i];
                if (item.Language == language)
                {
                    languageCodeList.Focus();
                    languageCodeList.SelectedIndices.Clear();
                    languageCodeList.TopItem = item;
                    item.Selected = true;
                    return;
                }
            }
        }

        private void LocateLanguageCode(string languageCode)
        {
            for (int i = 0; i < languageCodeList.Items.Count; i++)
            {
                LanguageCodeItem item = (LanguageCodeItem)languageCodeList.Items[i];
                if (item.LanguageCode == languageCode)
                {
                    languageCodeList.Focus();
                    languageCodeList.SelectedIndices.Clear();
                    languageCodeList.TopItem = item;
                    item.Selected = true;
                    return;
                }
            }
        }

        private void FillList()
        {
            LanguageCodeItem unknownItem = new LanguageCodeItem("(Unspecified)", "unk");
            languageCodeList.Items.Add(unknownItem);
            for (int i = 0; i < ISO639Table.LanguageCodes.Length; i += 2)
            {
                LanguageCodeItem newItem = new LanguageCodeItem(ISO639Table.LanguageCodes[i + 1], ISO639Table.LanguageCodes[i]);
                languageCodeList.Items.Add(newItem);
            }
        }

        private void LanguageCodeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(languageCodeList.SelectedItems.Count == 0)
            {
                selectedItem = null;
                okButton.Enabled = false;
                return;
            }

            selectedItem = (LanguageCodeItem) languageCodeList.SelectedItems[0];
            okButton.Enabled = true;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void SelectLanguageDialog_Load(object sender, EventArgs e)
        {

        }
    }

    public class LanguageCodeItem : ListViewItem
    {
        public string Language
        {
            get
            {
                return SubItems[0].Text;
            }
            set
            {
                SubItems[0].Text = value;
            }
        }

        public string LanguageCode
        {
            get
            {
                return SubItems[1].Text;
            }
            set
            {
                SubItems[1].Text = value;
            }
        }

        public LanguageCodeItem(string language, string languageCode) : base()
        {
            Text = language;
            SubItems.Add(languageCode);
        }
    }
}