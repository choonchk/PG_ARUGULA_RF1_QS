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
    public partial class ErrorDisplayDialog : Form
    {
        public ErrorDisplayDialog()
        {
            InitializeComponent();
        }

        public void Initialize(string message)
        {
            Initialize(message, String.Empty);
        }

        public void Initialize(string message, string title)
        {
            this.Text = "Error: " + title;
            this.textBoxTitle.Text = message;
            this.textBoxMessage.Text = String.Empty;
            this.textBoxMessage.Visible = false;
        }

        public void Initialize(string message1, string message2, string title)
        {
            this.Text = "Error: " + title;
            this.textBoxTitle.Text = message1;
            this.textBoxMessage.Text = message2;
            this.textBoxMessage.Visible = true;

        }

        public void Initialize(Exception ex)
        {
            if (ex == null)
            {
                string msg = "No exception.";
                Initialize(msg, "No Error");
                return;
            }

            string errorText = String.Format("{0}{2}{1}",
                ex.GetType(), ex.StackTrace, Environment.NewLine);
            Initialize(ex.Message, errorText, ex.GetType().ToString());
        }

        public void Initialize(string message, Exception ex)
        {
            if (ex == null)
            {
                Initialize(message, String.Empty, message);
                return;
            }

            string errorText = String.Format("{0}{2}{1}",
                ex.GetType(), ex.StackTrace, Environment.NewLine);
            string msg = String.Format("{1}{0}{2}",
                Environment.NewLine, message, ex.Message);
            string title = ex.GetType().ToString();

            Initialize(msg, errorText, title);
        }

        public void Initialize(string message, string title, Exception ex)
        {
            if (ex == null)
            {
                Initialize(message, title, message);
                return;
            }

            string errorText = String.Format("{0}{2}{1}",
                ex.GetType(), ex.StackTrace, Environment.NewLine);
            string msg = String.Format("{1}{0}{2}",
                Environment.NewLine, message, ex.Message);

            Initialize(msg, errorText, title);
        }
    }
}
