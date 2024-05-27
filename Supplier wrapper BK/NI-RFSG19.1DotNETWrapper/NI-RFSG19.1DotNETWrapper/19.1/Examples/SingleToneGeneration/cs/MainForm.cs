using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.ModularInstruments.Interop;


namespace NationalInstruments.Examples.SingleToneGeneration
{
    public partial class MainForm : Form
    {
        niRFSG _rfsgSession;
        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        void startGeneration()
        {
            string resourceName;
            double frequency, power;     
            resourceName = ResourceNameTextBox.Text;
            frequency = (double)frequencyNumeric.Value;
            power = (double)powerLevelNumeric.Value;

            errorTextBox.Text = "No error.";
            // Initialize the NIRfsg session
            if (_rfsgSession == null)
            {
                _rfsgSession = new niRFSG(resourceName, true, false);
            }
             _rfsgSession.ConfigureRF(frequency, power);
             _rfsgSession.Initiate();
             rfsgStatusTimer.Start();
        }

        void closeSession()
        {
            if (_rfsgSession != null)
            {
                _rfsgSession.close();
            }
        }
        void stopGeneration()
        {
            try
            {
                if (_rfsgSession != null)
                {
                    // Disable the output.  This sets the noise floor as low as possible.
                    _rfsgSession.ConfigureOutputEnabled(false);

                    // Close the RFSG NIRfsg session
                    _rfsgSession.close();

                    startButton.Enabled = true;
                    updateButton.Enabled = false;
                    stopButton.Enabled = false;
                    rfsgStatusTimer.Enabled = false;
                }
                _rfsgSession = null;
            }
            catch (Exception ex)
            {
                errorTextBox.Text = "Error in StopGeneration(): " + ex.Message;
            }
        }
        void CheckGeneration()
        {
            // Check the status of the RFSG 
            bool isDone = false;
            _rfsgSession.CheckGenerationStatus(out isDone);
        }
        private void rfsgStatusTimer_Tick(object sender, System.EventArgs e)
        {
            try
            {
                CheckGeneration();
            }
            catch (System.Exception ex)
            {
                if (ex.Message != "")
                {
                    errorTextBox.Text = ex.Message;
                }
                else
                {
                    errorTextBox.Text = "Error";
                }
            }
        }
        
        private void startButton_Click(object sender, EventArgs e)
        {
            try
            {                
                errorTextBox.Text = "No Error";
                startButton.Enabled = false;
                updateButton.Enabled = true;
                stopButton.Enabled = true;
                rfsgStatusTimer.Enabled = true;
                startGeneration();               
            }
            catch (System.Exception ex)
            {
                if (ex.Message != "")
                {
                    errorTextBox.Text = ex.Message;
                }
                else
                {
                    errorTextBox.Text = "Error";
                }
            }
            finally
            {
                if (_rfsgSession != null)
                {
                    _rfsgSession.Abort();
                    _rfsgSession.reset();
                }
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            stopGeneration();
        }
        private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            stopGeneration();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            UpdateGeneration();
        }

        private void UpdateGeneration()
        {
            double frequency, power;
            try
            {
                startButton.Enabled = true;
                // Read in all the control values 
                frequency = (double)frequencyNumeric.Value;
                power = (double)powerLevelNumeric.Value;

                // Abort generation 
                _rfsgSession.Abort();

                // Configure the instrument 
                _rfsgSession.ConfigureRF(frequency, power);

                // Initiate Generation 
                _rfsgSession.Initiate();

                // Start the status checking timer 
                startButton.Enabled = false;

            }
            catch (Exception ex)
            {
                if (ex.Message != "")
                {
                    errorTextBox.Text = ex.Message;
                }
                else
                {
                    errorTextBox.Text = "Error";
                }
            }
        }

    }
}
