using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

using Avago.ATF.Shares;
using Avago.ATF.LightweightDriver;
using Avago.ATF.StandardLibrary;
using Avago.ATF.IOLibrary;

using Avago.ATF.CrossDomainAccess; 


namespace TestPlanDriver
{
    public partial class FormDoNOTTouch : Form
    {
        // Switch Rule from here 
        // NOTE for x86, need 3 layer upwards 
        //      for ANY CPU, only need 2 layer upwards 
        private string RulePath = "";        
        private string TestPlanPath = "";

        private bool m_1stTimeRunTestPlan = true; 
        
        TestPlanLightweightRunner TheRunner = new TestPlanLightweightRunner();


        public FormDoNOTTouch()
        {
            InitializeComponent();

            string liteDriverStartupPath = Application.StartupPath;

            // "C:\\Avago.ATF\\Data\\ATFTestPlanTemplate\\TestPlanDriver\\bin\\Debug"
            // So no matter that what's the end, just pick the substring until 2rd '\' then append "\Data\TestPlans"
            // Here need set up the TestPlan Root Folder Path 
            bool validPath = false; 
            int catchPos = liteDriverStartupPath.IndexOf('\\', 0);
            if (catchPos > -1)
            {
                // get 1st, then find 2nd
                catchPos = liteDriverStartupPath.IndexOf('\\', catchPos + 1);
                if (catchPos > -1)
                {
                    string versionRootPath = liteDriverStartupPath.Substring(0, catchPos);

                    // get the 2nd, enough for us to build
                    ATFRTE.Instance.TestPlanRootFolder = versionRootPath + @"\Data\TestPlans";

                    string TestPlanDriverDir = liteDriverStartupPath.Remove(liteDriverStartupPath.IndexOf("TestPlanDriver")) + "TestPlanDriver\\";
                    TestPlanPath = TestPlanDriverDir + "TestPlan.cs";
                    RulePath = TestPlanDriverDir + "Rule.cs";

                    validPath = File.Exists(TestPlanPath) & File.Exists(RulePath);
                }
            }
            
            if (!validPath)
            {
                MessageBox.Show("Fail to build up the test plans root folder path from " + liteDriverStartupPath, "Abort!");
                Application.Exit(); 
            }

            DirectoryInfo directory = new DirectoryInfo(ATFRTE.Instance.TestPlanRootFolder);
            foreach (DirectoryInfo d in directory.GetDirectories())
                comboBoxPackages.Items.Add(d.Name.Trim());

            if (comboBoxPackages.Items.Count > 0)
            {
                int index = 0;

                for (int i = 0; i < comboBoxPackages.Items.Count; i++)
                {
                    if ( Application.StartupPath.ToUpper().Contains(comboBoxPackages.Items[i].ToString().Substring(0, 3).ToUpper()) )
                    {
                        index = i;
                        break;
                    }
                }
                index = 0;
                comboBoxPackages.SelectedIndex = index;
                ATFRTE.Instance.CurPackageTag = (string)comboBoxPackages.Items[index];
            }
        }


        /// <summary>
        /// Nothing to do with Rule stuff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonInit_Click(object sender, EventArgs e)
        {
            if (ATFRTE.Instance.CurPackageTag.Length < 1)
            {
                MessageBox.Show("MUST Provide Valid Package", "Abort Test Plan Drive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(ATFRTE.Instance.TestPlanRootFolder + @"\" + ATFRTE.Instance.CurPackageTag))
            {
                MessageBox.Show("Package " + ATFRTE.Instance.CurPackageTag + " NOT Exist!", "Abort Test Plan Drive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string testplanClassName = ClothoFileUtilities.Get1stMatchedSeg(TestPlanPath, TestPlanContentConstants.TAG_TestPlanClassStart, TestPlanContentConstants.TAG_TestPlanClassEnd);
            if (testplanClassName == "")
                return;

            IATFTest testPlanInstance;
            try
            {
                testPlanInstance = (IATFTest)Activator.CreateInstance(Type.GetType(testplanClassName));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to Create '" + testplanClassName + "' Instance: " + ex.Message, "Abort Test Plan Drive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            
            string ruleClassName = ""; 
            IATFAdaptiveSampling ruleInstance = null;

            if (checkBoxAdaptiveSamplingOnOff.Checked)
            {
                ruleClassName = ClothoFileUtilities.Get1stMatchedSeg(RulePath, TestPlanContentConstants.TAG_RuleClassStart, TestPlanContentConstants.TAG_RuleClassEnd);
                if (ruleClassName == "")
                    return;

                try
                {
                    ruleInstance = (IATFAdaptiveSampling)Activator.CreateInstance(Type.GetType(ruleClassName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fail to Create '" + ruleClassName + "' Instance: " + ex.Message, "Abort Test Plan Drive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            

            if (textBoxTestArgString.Text.Trim().EndsWith(";"))
            {
                MessageBox.Show("Parameter string Must NOT End with ';'.", "Abort Test Plan Drive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

            Trace.WriteLine("Call INIT with " + textBoxInitArgString.Text.Trim());
            string initRet = TheRunner.Init(testPlanInstance, textBoxInitArgString.Text.Trim(), TestPlanPath, ruleInstance, RulePath, true);
            labelResultFilePath.Text = TheRunner.ResultFileName;
            Trace.WriteLine("INIT Result: " + initRet);
            if (initRet.StartsWith("Fail"))
            {
                MessageBox.Show(initRet, "INIT FAILURE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (checkBoxAdaptiveSamplingOnOff.Checked)
                // First time clean up
                ATFSharedData.Instance.ResetAdaptiveSamplingRuleConfig(); 

            buttonInit.Enabled = false;
            buttonStartTestPlan.Enabled = true;
            buttonDoLot.Enabled = true; 
            buttonUnInit.Enabled = true;
            checkBoxAdaptiveSamplingOnOff.Enabled = false;
            checkBoxCalFileInterpolate.Enabled = true; 
            textBoxDoLotArgString.Enabled = true;
            textBoxTestArgString.Enabled = true;
            numericUpDownLoopCnt.Enabled = true;
            numericUpDownLoopDelay.Enabled = true; 
        }


        /// <summary>
        /// Nothing to do with Rule Stuff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUnInit_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Call UNINIT with " + textBoxUnInitArgString.Text.Trim());
            string uninitRet = TheRunner.UnInit(textBoxUnInitArgString.Text.Trim());
            Trace.WriteLine("UNINIT Result: " + uninitRet);
        }


        /// <summary>
        /// Rule Relevant 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStartTestPlan_Click(object sender, EventArgs e)
        {
            int loopcnt = (int)numericUpDownLoopCnt.Value;
            string info = "";

            ATFReturnResult testplanRet = null; 
            string ruleRet = "";
            
            listBoxRunResult.Items.Add("****************************************");
            listBoxRunResult.Items.Add("");
            listBoxRunResult.SelectedIndex = listBoxRunResult.Items.Count - 1;                
            

            this.Cursor = Cursors.WaitCursor;
            try
            {
                for (int idx = 0; idx < loopcnt; idx++)
                {
                    listBoxRunResult.Items.Add(string.Format("{0}:     #{1}/{2} Run ({3})", DateTime.Now.ToString(), idx + 1, loopcnt, textBoxTestArgString.Text.Trim()));

                    if (checkBoxAdaptiveSamplingOnOff.Checked)
                    {
                        Dictionary<string, bool> tempMask = ATFSharedData.Instance.ASRuleBitMask;
                        StringBuilder sbTemp = new StringBuilder("BitMask: ");
                        foreach (string key in tempMask.Keys)
                            sbTemp.AppendFormat("{0}: {1}; ", key, tempMask[key] ? "1" : "0");
                        listBoxRunResult.Items.Add(sbTemp.ToString());
                    }
                    
                    listBoxRunResult.SelectedIndex = listBoxRunResult.Items.Count - 1;
                    
                    
                    testplanRet = TheRunner.Test(textBoxTestArgString.Text.Trim(), checkBoxAdaptiveSamplingOnOff.Checked, ref ruleRet, m_1stTimeRunTestPlan);
                    listBoxRunResult.Items.Add(string.Format("{0}:     TestPlan Return: {1}", DateTime.Now.ToString(), 
                        StringProcessHelper.ConvertFullResultToStringWithLengthLimit(testplanRet, 500)));
                    

                    if (checkBoxAdaptiveSamplingOnOff.Checked)
                    {
                        listBoxRunResult.Items.Add(string.Format("{0}:     Rule Return: {1}", DateTime.Now.ToString(), ruleRet));

                        Dictionary<string, bool> tempMask = ATFSharedData.Instance.ASRuleBitMask;
                        StringBuilder sbTemp = new StringBuilder("BitMask: ");
                        foreach (string key in tempMask.Keys)
                            sbTemp.AppendFormat("{0}: {1}; ", key, tempMask[key] ? "1" : "0");
                        listBoxRunResult.Items.Add(sbTemp.ToString());
                    }
                    listBoxRunResult.SelectedIndex = listBoxRunResult.Items.Count - 1;
                    
                    // After 1st time run complete
                    if (m_1stTimeRunTestPlan)
                    {                        
                        m_1stTimeRunTestPlan = false;
                    }

                    Thread.Sleep((int)numericUpDownLoopDelay.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Test Plan Execution Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }



        private void checkBoxCalFileInterpolate_CheckedChanged(object sender, EventArgs e)
        {
            ATFCrossDomainWrapper.Cal_SwitchInterpolationFlag(checkBoxCalFileInterpolate.Checked); 
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void buttonDoLot_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Call DoLot with " + textBoxDoLotArgString.Text.Trim());
            string doLotRet = TheRunner.CloseLot(textBoxDoLotArgString.Text.Trim());
            Trace.WriteLine("DoLot Result: " + doLotRet);
        }

        private void comboBoxPackages_SelectedIndexChanged(object sender, EventArgs e)
        {
            ATFRTE.Instance.CurPackageTag = (string)comboBoxPackages.SelectedItem;
        }

        private void FormDoNOTTouch_Load(object sender, EventArgs e)
        {

        }

    }
}
