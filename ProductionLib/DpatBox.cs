using System;
using System.Windows.Forms;

namespace ProductionLib2
{
    public partial class DpatBox : Form
    {
        public DpatBox()
        {
            InitializeComponent();
        }

        //Ok
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "batman")
            {
                this.DialogResult = DialogResult.OK;               
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.Retry;
                MessageBox.Show("Password incorrect, please try again or press cancel to unload program...");
            }
        }

        //Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
