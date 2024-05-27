using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPAD_TestTimer
{
    public partial class MultipleSelectionDialog : Form
    {
        public MultipleSelectionDialog()
        {
            InitializeComponent();
        }

        private string m_chosenResponseButtonText;
        private int m_clickCounter;

        /// <summary>
        /// Returns empty string if Cancel or top right X button is clicked.
        /// </summary>
        public string SelectedButton
        {
            get { return m_chosenResponseButtonText; }
        }

        private void MultipleSelectionDialog_Load(object sender, EventArgs e)
        {
            m_chosenResponseButtonText = String.Empty;
        }

        public void SetMessage(string textBox1text, string dialogTitle,
            Dictionary<string, string> selectionList, string defaultSelectedKeyName)
        {
            this.Text = String.IsNullOrEmpty(dialogTitle) ? this.Text : dialogTitle;
            SetMessage(textBox1text, selectionList, defaultSelectedKeyName);
        }

        public void SetMessage(string textBox1text, string dialogTitle,
            string message1, string message2, string message3, string message4, string defaultSelectedKeyName)
        {
            this.Text = String.IsNullOrEmpty(dialogTitle) ? this.Text : dialogTitle;

            Dictionary<string, string> selectionList = new Dictionary<string, string>();

            selectionList.Add("Message1", message1);
            selectionList.Add("Message2", message2);
            selectionList.Add("Message3", message3);
            selectionList.Add("Message4", message4);

            SetMessage(textBox1text, selectionList, defaultSelectedKeyName);
        }

        public void SetMessage(string textBox1text, 
            Dictionary<string, string> selectionList, string defaultSelectedKeyName)
        {
            textBox1.Text = textBox1text;

            buttonOK.Tag = defaultSelectedKeyName;
            buttonCancel.Tag = String.Empty;

            int i = 0;
            foreach (var de in selectionList)
            {
                switch (i)
                {
                    case 0:
                        SetButton(button1, de, defaultSelectedKeyName);
                        break;
                    case 1:
                        SetButton(button2, de, defaultSelectedKeyName);
                        break;
                    case 2:
                        SetButton(button3, de, defaultSelectedKeyName);
                        break;
                    case 3:
                        SetButton(button4, de, defaultSelectedKeyName);
                        break;
                    case 4:
                        SetButton(button5, de, defaultSelectedKeyName);
                        break;
                }

                i++;
            }
        }

        public void SetMessage(string textBox1text, string message1, 
            string message2, string message3, string message4, string defaultSelectedKeyName)
        {
            SetMessage(textBox1text, String.Empty, message1, message2, message3,
                message4, defaultSelectedKeyName);
        }
        
        private void SetButton(Button btn, KeyValuePair<string, string> pair,
            string defaultSelectedKeyName)
        {
            btn.Tag = pair.Key;
            btn.Text = pair.Value;
            btn.Visible = true;

            if (pair.Key == defaultSelectedKeyName)
            {
                btn.BackColor = Color.LightGreen;
                //btn.Select();
            }
        }

        private void buttonAll_Click(object sender, EventArgs e)
        {
            // Must click 5 times to select.
            m_clickCounter++;
            if (m_clickCounter != 5)
            {
                this.DialogResult = DialogResult.None;
                return;
            }

            m_chosenResponseButtonText = (sender as Button).Tag.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_chosenResponseButtonText = (sender as Button).Tag.ToString();
            this.DialogResult = DialogResult.OK;
        }
    }
}
