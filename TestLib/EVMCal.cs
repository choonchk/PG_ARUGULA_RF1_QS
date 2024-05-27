using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPAD_TestTimer;
using System.Windows.Forms;
using EqLib;

namespace TestLib
{
    public class EVMCalibrationModel
    {
        public IATFTest myAtfTest;
        public static bool[] runningEVMCAL = new bool[64];
        public static byte siteNo = 0;

        public bool EVMCalibration(IATFTest clothoInterface)
        {
            return false;
        }

        public bool EVMCalibration(IATFTest clothoInterface, string calDirectoryName, string isEVMCALMode, int numSites = 1)
        {
            bool isSuccess = true;

            runningEVMCAL = new bool[numSites];
            //runningEVMVerification = new bool[numSites];


            string calMessage = "";

            this.myAtfTest = clothoInterface;
            string calFileDir = @"C:\Avago.ATF.Common.x64\EVMCalibration\" + calDirectoryName + "\\";
            RfTestBase.EVMCalDirectory = calFileDir + "EVM_ACLR_MEAS" + "\\";
            bool ProductionMode = isEVMCALMode == "FALSE" ? true : false;

            for (byte site = 0; site < numSites; site++)
            {
                RfTestBase.EvmWaveformDic[site] = new Dictionary<string, int>();
                runningEVMCAL[site] = false;
            }

            if (!ProductionMode)
            {
                //string cnt_str = Interaction.InputBox("Do you want to perform Cable Calibration? If so, please enter \"Yes\".", "Cable Calibration", "No", 100, 100);
                Dictionary<string, string> selectionList = new Dictionary<string, string>();
                selectionList.Add("Yes", "Yes");
                selectionList.Add("No", "No");
                string cnt_str = PromptManager.Instance.ShowMultiSelectionDialog(
                    "Do you want to perform EVM Calibration?",
                    "EVM Calibration", selectionList, "No");

                if (cnt_str.ToUpper() == "YES")
                {

                    for (byte site = 0; site < Eq.NumSites; site++)
                    {
                        RfTestBase.EvmWaveformDic[site].Clear();
                        RfTestBase.calFileNo[site] = 0;

                        calMessage = string.Format("Load GU Unit to site{0} and then press OK", site);
                        MessageBox.Show(calMessage, "EVM Cal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        runningEVMCAL[site] = true;
                        siteNo = site;
                        RfTestBase.EvmCalibration_Routine_On = true;
                        myAtfTest.DoATFTest("");
                        RfTestBase.EvmCalibration_Routine_On = false;
                        RfTestBase.EvmCalibration_Validation_On = true;
                        myAtfTest.DoATFTest("");
                        RfTestBase.EvmCalibration_Validation_On = false;
                        runningEVMCAL[site] = false;

                        if (RfTestBase.EvmCalibration_Pass)
                        {
                            calMessage = string.Format("EVMCAL site{0} Completed, press OK and reload test program", site);
                        }
                        else
                        {
                            calMessage = string.Format("EVMCAL site{0} Failed, Possible contact issue! Troubleshoot, Reload test program and Rerun Cal", site);
                        }
                        MessageBox.Show(calMessage, "EVM Cal", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }

                }
            }

            //Dummy run to load FASTEVMCALDATA serially for all sites
            for (byte site = 0; site < numSites; site++)
            {
                runningEVMCAL[site] = true;
                siteNo = site;
                myAtfTest.DoATFTest("");
                runningEVMCAL[site] = false;
            }

            return isSuccess;
        }
    }
}
