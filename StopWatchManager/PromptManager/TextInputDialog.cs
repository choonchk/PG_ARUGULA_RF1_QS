using System;
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
    public partial class TextInputDialog : Form
    {
        public TextInputDialog()
        {
            InitializeComponent();
        }

        public string InputText
        {
            get { return textBox1.Text; }
        }

        public void SetMessage(string message, string dialogTitle, string defaultInput)
        {
            label1.Text = message;
            this.Text = dialogTitle;
            textBox1.Text = defaultInput;
        }

        public void SetMessage(string messageLine1, string messageLine2, 
            string dialogTitle, string defaultInput)
        {
            string msg = String.Format("{1}{0}{2}", Environment.NewLine,
                messageLine1, messageLine2);
            label1.Text = msg;
            this.Text = dialogTitle;
            textBox1.Text = defaultInput;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
        }
    }
}
