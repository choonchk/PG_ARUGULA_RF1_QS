using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using EqLib;
using MPAD_TestTimer;
using TestLib;
using TestPlanCommon.CommonModel;
using System.Threading;
using Avago.ATF.StandardLibrary;
using GuCal;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TestPlan_EdamFull
{
    /// <summary>
    /// CableCalibration & Power meter.
    /// </summary>
    public class CableCalibrationModel
    {
        // CCT Entry Point.
        public bool CableCalibration()
        {
            //m_modelTpState.SetLoadFail(CableCalibration2(m_doClotho1.ClothoRootDir));
            return false;
        }

        public bool CableCalibration(string clothoRootDir, string calDirectoryName, bool isMagicBox)
        {
            bool isSuccess = true;
            GU.isMagicBox = isMagicBox;
            GU.calibrationDirectoryName = calDirectoryName;

            if (!isMagicBox)
            {
                //string cnt_str = Interaction.InputBox("Do you want to perform Cable Calibration? If so, please enter \"Yes\".", "Cable Calibration", "No", 100, 100);
                Dictionary<string, string> selectionList = new Dictionary<string, string>();
                selectionList.Add("Yes", "Yes");
                selectionList.Add("No", "No");
                string cnt_str = PromptManager.Instance.ShowMultiSelectionDialog(
                    "Do you want to perform Cable Calibration?",
                    "Cable Calibration", selectionList, "No");

                CableCal.Initialize(cnt_str.ToUpper() == "YES", calDirectoryName,
                    clothoRootDir + @"Calibration\FreqCalList_Master.csv");
                if (cnt_str.ToUpper() == "YES")
                {
                    //Eq.Site[0].RF.SA.SelfCalibration(1400e6, 6000e6, -60, 20);
                    //Eq.Site[0].RF.SG.SelfCalibration(1400e6, 2700e6, -40, 0);
                    isSuccess = InitializePM();
                }
            }
            else
            {
                #region Check Magicbox cal status
                string magicbox_adi_folder = @"D:/ExpertCalSystem.Data/MagicBox/" + GU.calibrationDirectoryName + @"/Data.Current/";

                if (File.Exists(magicbox_adi_folder + @"MagicBoxMetaData.adi"))
                {
                    string json = File.ReadAllText(magicbox_adi_folder + @"MagicBoxMetaData.adi");
                    Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);
                    foreach (var item in json_Dictionary)
                    {
                        if (item.Key == "isAllPass")
                        {
                            if (item.Value.ToString().ToUpper() != "TRUE")
                            {
                                isSuccess = false;
                                return isSuccess;
                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                }
                else
                {
                    //isSuccess = false;
                    //return isSuccess;
                }
                #endregion Check Magicbox cal status

                CableCal.Initialize(false, calDirectoryName,
                    clothoRootDir + @"Calibration\FreqCalList_Master.csv");
            }

            CableCal.OnboardAtten txAtten = CableCal.OnboardAtten.None;
            CableCal.OnboardAtten rxAtten = CableCal.OnboardAtten.None;
            CableCal.OnboardAtten antAtten = CableCal.OnboardAtten.None;

            double maxSourceFreqMhz = 5940;
            double maxMeasureFreqMhz = 5940;

            float sgLevel = -10f; //org -10f


            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //if (site % 2 == 0) Eq.Site[site].SwMatrixSplit.ActivatePath("Split", Operation.P_VSTtoVST_1);
                //else Eq.Site[site].SwMatrixSplit.ActivatePath("Split", Operation.P_VSTtoVST_2);
                CableCal.CalibrateSourcePath(site, "IN-FBRX", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoTX);
                CableCal.CalibrateSourcePath(site, "IN1-N77", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoTX1);
                CableCal.CalibrateSourcePath(site, "IN2-N79", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoTX2);

                CableCal.CalibrateSourcePath(site, "ANT1", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoANT1);
                CableCal.CalibrateSourcePath(site, "ANT2", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoANT2);
                CableCal.CalibrateSourcePath(site, "ANTU", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoANT3);
                //CableCal.CalibrateSourcePath(site, "ANTL", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoANT4);

            }

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //if (site % 2 == 0) Eq.Site[site].SwMatrixSplit.ActivatePath("Split", Operation.P_VSTtoVST_1);
                //else Eq.Site[site].SwMatrixSplit.ActivatePath("Split", Operation.P_VSTtoVST_2);
                //For Quadsite
                byte SiteTemp = site;
                EqSwitchMatrix.PortCombo measureCalSourcePort;
                if (site.Equals(0) == false)
                {
                    SiteTemp = 0;
                    measureCalSourcePort = Eq.Site[SiteTemp].SwMatrix.GetPath("IN1-N77_1", Operation.VSGtoTX1);
                }
                else
                {
                    measureCalSourcePort = Eq.Site[SiteTemp].SwMatrix.GetPath("IN1-N77", Operation.VSGtoTX1);
                }


                //Reference changed for Modular due to high gain by Mario 2020/02/27
                //////////////////////////////////////////////
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANT1", Operation.VSAtoANT1, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANT2", Operation.VSAtoANT2, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANTU", Operation.VSAtoANT3, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANTL", Operation.VSAtoANT4, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);  //ANTL is currently configured as an input only

                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT-FBRX", Operation.VSAtoRX, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT1-N77", Operation.VSAtoRX1, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT4-N79", Operation.VSAtoRX3, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT3-N79", Operation.VSAtoRX2, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT2-N77", Operation.VSAtoRX4, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);

                // Original for Y2D 
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANT1", Operation.VSAtoANT1, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANT2", Operation.VSAtoANT2, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "ANTU", Operation.VSAtoANT3, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);

                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT-FBRX", Operation.VSAtoRX, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT1-N77", Operation.VSAtoRX1, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT3-N79", Operation.VSAtoRX2, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT4-N79", Operation.VSAtoRX3, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz(site, "OUT2-N77", Operation.VSAtoRX4, rxAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);

                if (CableCal.runCableCal)
                {
                    // Measure H3 path with ENA
                    //Eq.Site[0].EqNetAn.Write(":MMEM:LOAD \"D:\\Merlin_SYSTEMCAL.STA" + "\"");
                    //Eq.Site[0].EqNetAn.Write(":DISP:ENAB 1");
                    //MessageBox.Show("This is a manual step:  Use a through and perform two port calibration using Port 1 and 2 of the ENA.  Click OK when finished", "ECAL");
                    //CableCal.CalibrateMeasurePathWithNA(site, "", Operation.MeasureH3_ANT1, antAtten);
                }
            }
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //For Quadsite
                byte SiteTemp = site;
                EqSwitchMatrix.PortCombo measureCalSourcePort;
                if (site.Equals(0) == false)
                {
                    SiteTemp = 0;
                    measureCalSourcePort = Eq.Site[SiteTemp].SwMatrix.GetPath("N77_1", Operation.VSGtoTXFor_HMU);
                }
                else
                {
                    measureCalSourcePort = Eq.Site[SiteTemp].SwMatrix.GetPath("N77", Operation.VSGtoTXFor_HMU);
                }

                CableCal.CalibrateSourcePathwithHMU_Over6Ghz(site, "N77", txAtten, sgLevel, maxMeasureFreqMhz, -999f, 999f, Operation.VSGtoTXFor_HMU);

            }

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //For Quadsite
                byte SiteTemp = site;
                EqSwitchMatrix.PortCombo measureCalSourcePort;
                if (site.Equals(0) == false)
                {
                    SiteTemp = 0;
                    measureCalSourcePort = Eq.Site[SiteTemp].SwMatrix.GetPath("N77_1", Operation.VSGtoTXFor_HMU);
                }
                else
                {
                    measureCalSourcePort = Eq.Site[SiteTemp].SwMatrix.GetPath("N77", Operation.VSGtoTXFor_HMU);
                }

                //Reference changed for Modular due to high gain by Mario 2020/02/27
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz_2nd(site, "N77", Operation.MeasureH2_ANT1, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz_2nd(site, "N77", Operation.MeasureH2_ANT2, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);
                CableCal.CalibrateMeasurePathWithHMU_Over6Ghz_2nd(site, "N77", Operation.MeasureH2_ANT3, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, 15);

                // Original for Y2D 
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz_2nd(site, "N77", Operation.MeasureH2_ANT1, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz_2nd(site, "N77", Operation.MeasureH2_ANT2, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
                //CableCal.CalibrateMeasurePathWithHMU_Over6Ghz_2nd(site, "N77", Operation.MeasureH2_ANT3, antAtten, sgLevel, maxMeasureFreqMhz, -100f, 0f, measureCalSourcePort, -20);
            }

            if (CableCal.runCableCal)
            {
                MessageBox.Show("Cable Calibration is finished\n\nPlease reconnect loadboard and dock test-head to handler\n\nMust run GU Icc + Corr + Verify", "Cable Calibration");

                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    Eq.Site[site].RF.SG.Abort();
                    Eq.Site[site].RF.SA.Abort(site);
                }
            }

            CableCal.LoadAllCalibratedFiles();

            return isSuccess;
        }


        public bool InitializePM()
        {
            bool isSuccess = true;

            for (byte site = 0; site < 1; site++)
            {
                MessageBoxButtons psZeroButtons = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, "") == "" ?   // in lite driver, allow engineer to cancel
                    MessageBoxButtons.OKCancel :
                    MessageBoxButtons.OK;

                DialogResult psZeroResponse = DialogResult.Cancel;
                int PSzeroMaxTries = 3;

                string model, SN;

                model = "";
                SN = "";

                switch (site)   // this hardcoding won't work, must find a way to auto-detect.
                {

                    default:    //DH To be changed
                        model = "Z11";
                        //SN = "110948";
                        //SN = "110538"; //KCC
                        break;

                        //case 0:
                        //    model = "Z21";
                        //    break;
                        //case 1:
                        //default:
                        //    model = "Z11";
                        //    //SN = "110948";
                        //    SN = "115005";
                        //    break;
                }

                Eq.Site[site].PM = EqPM.Get(model, SN);

                for (int i = 1; i <= PSzeroMaxTries && psZeroResponse != DialogResult.Cancel; i++)
                {
                    if (i == 1)
                    {
                        string msg = string.Format("Please disconnect Site {0} Power Sensor from Switch Matrix, then press OK.", site);
                        psZeroResponse = MessageBox.Show(msg, "Power Sensor Zeroing", psZeroButtons);
                    }
                    else
                    {
                        string msg = string.Format("Power Sensor zeroing failed, please try again.\n\nPlease disconnect Site {0} Power Sensor from Switch Matrix, then press OK.", site);
                        psZeroResponse = MessageBox.Show(msg, "Power Sensor Zeroing, Try " + i, psZeroButtons);
                    }

                    if (psZeroResponse == DialogResult.Cancel) break;
                    else Eq.Site[site].PM.Zero();

                    Eq.Site[site].PM.SetupMeasurement(7000, 0.001, 100);

                    Thread.Sleep(20);

                    double openMeasurement = Eq.Site[site].PM.Measure();

                    psZeroResponse = MessageBox.Show("    Now please reconnect\nPower Sensor to switch matrix, then press OK.", "Power Sensor Zeroing", MessageBoxButtons.OK);

                    double switchMatrixMeasurement = Eq.Site[site].PM.Measure();  // should measure the noise floor of the switch matrix here

                    if (switchMatrixMeasurement > -58 && (switchMatrixMeasurement - openMeasurement > 10))
                        break;
                    else if (i == PSzeroMaxTries)
                    {
                        MessageBox.Show("Power Sensor zeroing failed, please contact engineer for debugging.", "Power Sensor Zeroing", psZeroButtons);
                        isSuccess = false;
                    }
                }

                Eq.Site[site].PM.SetupBurstMeasurement(7000, 0.00025, 10);   // Only setup once at beginning of program.
            }

            for (byte site = 1; site < Eq.NumSites; site++)
            {
                Eq.Site[site].PM = Eq.Site[0].PM;
            }

            return isSuccess;
        }

    }


    public class HsdioModel
    {
        private ConcurrentBag<Task> _VectorTaskBags = new ConcurrentBag<Task>();
        public bool InitializeHSDIO(TestPlanStateModel m_modelTpState)
        {
            //bool isLoadSuccess = InitializeHSDIO2(m_doClotho1.ClothoRootDir, TCF_Setting["CMOS_DIE_TYPE"]);
            //m_modelTpState.SetLoadFail(isLoadSuccess);
            return false;

        }

        public bool InitializeHSDIO2(ITesterSite tester)
        {
            //Experimental change for Steven  MIPIClockRate = 13e6
            EqHSDIO.MIPIClockRate = 52e6; //26e6;// 52e6;  //set back to 52
            EqHSDIO.StrobePoint = (1 / EqHSDIO.MIPIClockRate) * 0.9;  //                StrobePoint = 30e-9; // 40E-9;//69E-9; // 77e-9;//this is for 4ex6

            EqHSDIO.Num_Mipi_Bus = Convert.ToInt16(Digital_Definitions_Part_Specific["NUM_MIPI_BUS"]);

            bool programLoadSuccess = true;

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                string visaAlias = tester.GetVisaAlias(GlobalVariables.HSDIOAlias, site);
                EqHSDIO.EqHSDIObase equipment = EqHSDIO.Get(visaAlias, site);    // need to automate the Visa Alias per site
                equipment.Digital_Definitions = Digital_Definitions_Part_Specific;
                //equipment.Digital_Mipi_Trig = Digital_Mipi_Trig_Part_Specific;
                programLoadSuccess &= equipment.Initialize(Eq.Site[site].DC, EqTriggerArray);
                Eq.Site[site].HSDIO = equipment;

            }
            return programLoadSuccess;
        }

        public bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion,
           Dictionary<string, string> TXQC = null, Dictionary<string, string> RXQC = null, MipiTestConditions tc = null)
        {
            bool programLoadSuccess = true;

            try
            {
                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    EqHSDIO.EqHSDIObase equipment = Eq.Site[site].HSDIO;
                    LoadVector(clothoRootDir, tcfCmosDieType, sampleVersion, TXQC, RXQC, equipment, tc);
                }

                Task.WaitAll(_VectorTaskBags.ToArray());

                do
                {
                    if (_VectorTaskBags.TryTake(out Task t))
                        programLoadSuccess &= (t as Task<bool>).Result;
                } while (!_VectorTaskBags.IsEmpty);
            }
            catch (Exception e)
            {
                return false;
            }
            return programLoadSuccess;
        }

        private bool LoadVector(string clothoRootDir, string tcfCmosDieType,
            string sampleVersion, Dictionary<string, string> TXQCVector, Dictionary<string, string> RXQCVector,
            EqHSDIO.EqHSDIObase equipment, MipiTestConditions tc)
        {
            string vectorBasePath = clothoRootDir + @"RFFE_vectors\QCTEST\" + sampleVersion + "\\";

            bool programLoadSuccess = true;

            string temp = "";
            string vecMemoryName = "";
            int pos = 0;

            foreach (var item in TXQCVector)
            {
                // RFFE_Vectors_QCTest_TXQC
                temp = item.Key;
                pos = temp.IndexOf("TXQC");
                vecMemoryName = temp.Remove(0, pos);
                tc.RunningVectorBags.Add(vecMemoryName);
                _VectorTaskBags.Add(Task.Factory.StartNew(() => equipment.LoadVector(vecMemoryName, vectorBasePath + item.Value)));
            }

            foreach (var item in RXQCVector)
            {
                // RFFE_Vectors_QCTest_RXQC
                temp = item.Key;
                pos = temp.IndexOf("RXQC");
                vecMemoryName = temp.Remove(0, pos);
                tc.RunningVectorBags.Add(vecMemoryName);
                _VectorTaskBags.Add(Task.Factory.StartNew(() => equipment.LoadVector(vecMemoryName, vectorBasePath + item.Value)));
            }

            return programLoadSuccess;
        }

        /// <summary>
        /// S-Para only.
        /// </summary>
        public void InitializeSmu()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();

            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4154_01.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4139_02.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vdd", "NI4139_03.0"));

            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk1", "NI6570.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata1", "NI6570.1"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio1", "NI6570.2"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sclk2", "NI6570.4"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Sdata2", "NI6570.5"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vio2", "NI6570.3"));

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                foreach (KeyValuePair<string, string> kv in DCpinSmuResourceTempList)
                {
                    string DcPinName = kv.Key;
                    string pinName = DcPinName.Remove(0, 2);
                    string VisaAlias = kv.Value.Split('.')[0];
                    string Chan = kv.Value.Split('.')[1];

                    DcPinName = DcPinName.Replace("V.", "").Trim();
                    EqDC.iEqDC dcValue = EqDC.Get(VisaAlias, Chan, DcPinName, site, true);
                    Eq.Site[site].DC.Add(DcPinName, dcValue); //This is what feeds InitializeHSDIO

                    //if (pinName.Contains("Sclk") || pinName.Contains("Sdata") || pinName.Contains("Vio"))
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    //    //pinName = pinName.Replace("V.", "");
                    //    PaTest.SmuResources.Add(pinName, Smu.getSMU(VisaAlias, Chan, pinName, true));
                    //}

                    //    //PaTest.SmuResources.Add(pinName, Smu.getSMU(VisaAlias, Chan, pinName, true));

                }
            }
        }

        //These mipi trigger setting are parts specific.
        public readonly Dictionary<string, uint[]> Digital_Mipi_Trig_Part_Specific = new Dictionary<string, uint[]>()
        {
            //Tx
            {"TrigOffRz".ToUpper(), new uint[23] { 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
            {"TrigOnRz".ToUpper(), new uint[23] { 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 }},
            {"TrigMaskOnRz".ToUpper(), new uint[23] { 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0 }},
            //Rx
            {"TrigOffRzRx".ToUpper(), new uint[23] { 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
            {"TrigOnRzRx".ToUpper(), new uint[23] { 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 }},
            {"TrigMaskOnRzRx".ToUpper(), new uint[23] { 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0 }}
        };

        public readonly Dictionary<string, string> Digital_Definitions_Part_Specific = new Dictionary<string, string>()
            {
                { "PROJECT","EDAM"},
                { "REV_ID_BIT0","SE_E2_B0"},
                { "REV_ID_BIT1","SE_E2_B1"},
                { "REV_ID_BIT2","SE_E2_B2"},
                { "REV_ID_BIT3","SE_E2_B3"},
                { "REV_ID_BIT4","SE_E2_B4"},
                { "REV_ID_BIT5","SE_E2_B5"},
                { "REV_ID_BIT6","SE_E2_B6"},
                { "REV_ID_BIT7","SE_E2_B7"},
                { "REV_ID_NUM_BITS","8"},
                { "REV_ID_READ","21"},


                { "MFG_ID_BIT0","S2_E1_B0"},
                { "MFG_ID_BIT1","S2_E1_B1"},
                { "MFG_ID_BIT2","S2_E1_B2"},
                { "MFG_ID_BIT3","S2_E1_B3"},
                { "MFG_ID_BIT4","S2_E1_B4"},
                { "MFG_ID_BIT5","S2_E1_B5"},
                { "MFG_ID_BIT6","S2_E1_B6"},
                { "MFG_ID_BIT7","S2_E1_B7"},

                { "MFG_ID_BIT8","S2_E0_B0"},
                { "MFG_ID_BIT9","S2_E0_B1"},
                { "MFG_ID_BIT10","S2_E0_B2"},
                { "MFG_ID_BIT11","S2_E0_B3"},
                { "MFG_ID_BIT12","S2_E0_B4"},
                { "MFG_ID_BIT13","S2_E0_B5"},
                { "MFG_ID_BIT14","S2_E0_B6"},
                { "MFG_ID_BIT15","S2_E0_B7"},
                { "MFG_ID_NUM_BITS","16"},
                { "MFG_ID_LSB_READ","41"},
                { "MFG_ID_MSB_READ","40"},

                { "MOD_ID_BIT0","S2_E5_B0"},
                { "MOD_ID_BIT1","S2_E5_B1"},
                { "MOD_ID_BIT2","S2_E5_B2"},
                { "MOD_ID_BIT3","S2_E5_B3"},
                { "MOD_ID_BIT4","S2_E5_B4"},
                { "MOD_ID_BIT5","S2_E5_B5"},
                { "MOD_ID_BIT6","S2_E5_B6"},
                { "MOD_ID_BIT7","S2_E5_B7"},

                { "MOD_ID_BIT8","S2_E4_B0"},
                { "MOD_ID_BIT9","S2_E4_B1"},
                { "MOD_ID_BIT10","S2_E4_B2"},
                { "MOD_ID_BIT11","S2_E4_B3"},
                { "MOD_ID_BIT12","S2_E4_B4"},
                { "MOD_ID_BIT13","S2_E4_B5"},
                { "MOD_ID_BIT14","S2_E4_B6"},
                { "MOD_ID_NUM_BITS","15"},
                { "MOD_ID_LSB_READ","E5"},
                { "MOD_ID_MSB_READ","E4"},

                { "CM_ID","S2_E4_B7"},
                { "CM_ID_BIT0","S6_E0_B4"},
                { "CM_ID_BIT1","S6_E0_B5"},
                { "CM_ID_READ","E0"},// RX Register

                { "FBAR_FLAG_BURN","S2_E3_B0"},
                { "RF1_FLAG_BURN","S2_E3_B1"},
                { "RF2_FLAG_BURN","S2_E3_B2"},
                { "NOISE_FLAG_BURN","SE_E3_B0"},
                { "DPAT_FLAG_BURN","SE_E3_B3"},
                { "LOCK_BIT_BURN","S2_EB_B0"},

                { "FBAR_FLAG_READ","E3"},
                { "RF1_FLAG_READ","E3"},
                { "RF2_FLAG_READ","E3"},
                { "NOISE_FLAG_READ","E3"},
                { "DPAT_FLAG_READ","E3"},
                { "LOCK_BIT_READ","EB"},

                { "CMOS_TX_X_READ", "E6" },
                { "CMOS_TX_X_NUM_BITS", "8" },
                { "CMOS_TX_Y_READ", "E7" },
                { "CMOS_TX_Y_NUM_BITS", "8" },

                { "CMOS_TX_WAFER_LOT_LSB_READ", "E8" },
                { "CMOS_TX_WAFER_LOT_MSB_READ", "E9" },
                { "CMOS_TX_WAFER_LOT_NUM_BITS", "10" },

                { "CMOS_TX_WAFER_ID_READ", "E9" },
                { "CMOS_TX_WAFER_ID_NUM_BITS", "5" },

               // LNA CMOS wafer level position
                {"LNA_X_LSB_READ","E1" },
                {"LNA_X_NUM_BITS","7"},

                {"LNA_Y_LSB_READ","E2" },
                {"LNA_Y_MSB_READ","E1" },
                {"LNA_Y_NUM_BITS","9"},

                {"LNA_WAFER_LOT_LSB_READ","E3" },
                {"LNA_WAFER_LOT_MSB_READ","E4" },
                {"LNA_WAFER_LOT_NUM_BITS","10"},

                {"LNA_WAFER_ID_READ","E4" }, //FC
                {"LNA_WAFER_ID_NUM_BITS","5"}, //5

                {"RX_LOCKBIT_READ", "E7" },

                // Specify Which Slave Address is associated with which MIPI Bus                      
                { "NUM_MIPI_BUS","2"},
                //{ "SLAVE_ADDR_03","1"}, //be obsolated due to duplicate T/Rx. 
                //{ "SLAVE_ADDR_0E","2"},
                //{ "MIPI1_SLAVE_ADDR","03"}, hosein
                { "MIPI1_SLAVE_ADDR","6"},
                { "MIPI2_SLAVE_ADDR","6"},

                // Temperature Sensor information                      
                { "TEMPERATURE_REG","10"},
                { "MIN_TEMP","-20"},
                { "MAX_TEMP","130"},
                { "NUM_TEMP_STEPS","255"},

                // PID Register
                { "PID_REG","1D"},


                //channel names as the appear in the vector  
                {"SCLK1_VEC_NAME","Sclk1"},  //Depending on your vector source this name may be different for example SCLK_TX or SCLK
                {"SDATA1_VEC_NAME","Sdata1"},// these also need to match the entry in the tcf header that assigns the hsdio pins
                {"VIO1_VEC_NAME","Vio1"},
                {"SCLK2_VEC_NAME","Sclk2"},
                {"SDATA2_VEC_NAME","Sdata2"},
                {"VIO2_VEC_NAME","Vio2"},

                {"SHIELD_VEC_NAME","Shield"},
                {"TRIG_VEC_NAME","Trig"},
                {"VRX_VEC_NAME","Vrx"},
                {"VSH_VEC_NAME","Vsh"},
                {"I2C_VCC_VEC_NAME","I2C_VCC"},
                {"I2C_SCK_VEC_NAME","SCK"},
                {"I2C_DAC_VEC_NAME","SDA"},
                {"TEMPSENSE_I2C_VCC_VEC_NAME","TSVCC"},
                {"UNIO_VPUP_VEC_NAME", "UNIO_VPUP" },// Loadboard
                {"UNIO_SIO_VEC_NAME", "UNIO_SIO" }, // Loadboard
                {"UNIO_VPUP2_VEC_NAME", "UNIO2_VPUP" },  // Socket
                {"UNIO_SIO2_VEC_NAME", "UNIO2_SIO" },  // Socket
               
                // Map extra pins that are not included in the TCF as of 10/07/2015
                {"SHIELD_CHANNEL","18"},
                {"TRIG_CHANNEL","15"},
                //ChoonChin - For Temperature
                {"I2C_VCC_CHANNEL","31"},           // Case NightHawk
                {"I2C_SCK_CHANNEL","16"},
                {"I2C_SDA_CHANNEL","23"},   // pin 9
                {"I2C_DAC_CHANNEL","17"},
                {"TEMPSENSE_I2C_VCC_CHANNEL","20"},
                // JJ Low - single-wire EEPROM
                {"UNIO_VPUP_CHANNEL", "29" },   // Loadboard 
                {"UNIO_SIO_CHANNEL", "17" },    // Loadboard 
                {"UNIO_VPUP2_CHANNEL", "25" },   // Socket
                {"UNIO_SIO2_CHANNEL", "13" },    // Socket

                //20190709 new OtpTest
                //mipi Level Configuration
                {"VIH","1.8"}, //1.2 //1.8
                {"VIL","0"},
                {"VOH","0.4"},//0.4 //0.6
                {"VOL","0.8"},//0.5 //0.8

                //20190709 new OtpTest
                {"TX_EFUSE_BYTE0", "E0" },    //MOD_MFG_LOT_MSB  //Hosein 40
                {"TX_EFUSE_BYTE1", "E1" },    //MOD_MFG_LOT_LSB     // Hosei 41
                {"TX_EFUSE_BYTE2", "E2" },    //REV_ID  //hosein 21
                {"TX_EFUSE_BYTE3", "E3" },    //MODULE_ID_BYTE_0, Pass Flag
                {"TX_EFUSE_BYTE4", "E4" },    //MODULE_ID_BYTE_1    //Hosein
                {"TX_EFUSE_BYTE5", "E5" },    //MODULE_ID_BYTE_2  //Hosein  lockbit
                {"TX_EFUSE_BYTE6", "E6" },    //WAFER_0, WAFER_X_COORDINATS 
                {"TX_EFUSE_BYTE7", "E7" },    //WAFER_1, WAFER_Y_COORDINATS 
                {"TX_EFUSE_BYTE8", "E8" },    //WAFER_2, LOTID
                {"TX_EFUSE_BYTE9", "E9" },    //WAFER_3, WAFER_ID
                {"TX_EFUSE_BYTEA", "EA" },    //EXT_PID, 
                {"TX_EFUSE_BYTEB", "EB" },    //WAFER_4, VDAC_OTP_TRIMMING[7:4], TS_OFFSET[3:1], LOCK_BIT[0] VDAC_Tempco[5:4]

                {"RX_EFUSE_BYTE0", "21" },     //Revisiono ID
                {"RX_EFUSE_BYTE1", "40" },     //MODULE_MANUFACTURING_LOT_MSB			
                {"RX_EFUSE_BYTE2", "41" },     //MODULE_MANUFACTURING_LOT_LSB			
                {"RX_EFUSE_BYTE3", "E0" },     //BAND_GAP			
                {"RX_EFUSE_BYTE4", "E1" },     //Wafer Location ID0
                {"RX_EFUSE_BYTE5", "E2" },     //Wafer Location ID1			
                {"RX_EFUSE_BYTE6", "E3" },     //LOT ID LSB[7:0]		
                {"RX_EFUSE_BYTE7", "E4" },     //WAFER ID[6:2] & LOT ID MSB1[1:0]	
                
                {"RX_EFUSE_BYTE8", "20" },     //Extended PID		
                {"RX_EFUSE_BYTE9", "E5" },     //LOT ID MSB2[7:0]	
                {"RX_EFUSE_BYTEA", "E6" },     //77LNA2 boost[1:0], 79LNA1 boost[2:0], 77LNA1 boost[2:0]	
                {"RX_EFUSE_BYTEB", "E7" },     //Lock bit[31], Reserved, 79LNA2 boost[2:0], 77LNA2 boost[2]	
                {"RX_EFUSE_BYTEF", "None" },   //dummay	

                {"MASKED_CMD_NUM", "31" },
                {"MASKED_REGISTER", "" },     //Spare-1, Lock Bit :64bit			
                     
                {"TX_FABSUPPLIER", "COMMON" },     // LNA FAB IP
                {"RX_FABSUPPLIER", "GF" },     // LNA FAB IP
                {"RX_PGM","F0" },
                {"RX_BIT_SELECTOR","F2"},

                //For 2DID
                //2DID PCB_LOT
                {"PCB_LOT_ID_MSB","E0" }, // RX Register
                {"PCB_LOT_ID_LSB","E3" }, // TX Register               
                {"PCB_LOT_ID_NUM_BITS","10" },
                //2DID PCB Panel
                {"PCB_PANEL_ID","E4" },   // TX register                          
                {"PCB_PANEL_NUM_BITS","6" },
                //2DID PCB Strip
                {"PCB_STRIP_ID","41" }, // RX register               
                {"PCB_STRIP_NUM_BITS","4" },
                //2DID ModuleID
                {"PCB_MODULE_ID_MSB","41" }, // RX Register
                {"PCB_MODULE_ID_LSB","40" }, // RX Register                
                {"PCB_MODULE_ID_NUM_BITS","11" },
        };

        public readonly Dictionary<byte, int[]> EqTriggerArray = new Dictionary<byte, int[]>()
        {
            //public enum TriggerLine
            //{
            //    None = 0, 
            //PxiTrig0 = 1,
            //PxiTrig1 = 2,
            //PxiTrig2 = 3,
            //PxiTrig3 = 4,
            //PxiTrig4 = 5,
            //PxiTrig5 = 6,
            //PxiTrig6 = 7,
            //PxiTrig7 = 8,
            //FrontPanel0 = 9,
            //FrontPanel1 = 10,
            //FrontPanel2 = 11,
            //FrontPanel3 == 12
            //}

            {0, new int[4]{1, 2, 3, 4} },
            {1, new int[4]{5, 6, 7, 8} }
        };

    }

    public class SwitchMatrixModel
    {
        /// <summary>
        /// Pinot variant.
        /// </summary>
        public void InitializeSwitchMatrix3(ITesterSite tester, bool isMagicBox)
        {
            //Hardcoded to NumSites = 1 for Quadsite Development. Single DIO card for two sites.
            byte Numsites = 1;

            for (byte site = 0; site < Numsites; site++)
            {
                switch (site)  // ability to use different switch matrix models per site
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        string visaAlias = tester.GetVisaAlias("DIO", site);
                        EqSwitchMatrix.Rev swRev = tester.GetSwitchMatrixRevision();
                        Eq.Site[site].SwMatrix = EqSwitchMatrix.Get(visaAlias, swRev, true);
                        break;
                }

                switch (Eq.Site[site].SwMatrix.Revision)
                {
                    case EqSwitchMatrix.Rev.Y2:

                        #region Rev Y2 JOKER
                        /*
                         * 
                         * 
                        #region Rev Y2

                        // MagicBox
                        #region Y2M path
                        Eq.Site[site].SwMatrix.DefinePath("Y2M", Operation.SGCplrToSGIn, EqSwitchMatrix.DutPort.SGCplr, EqSwitchMatrix.InstrPort.SGIn);
                        Eq.Site[site].SwMatrix.DefinePath("Y2M", Operation.SAOutToSAIn, EqSwitchMatrix.DutPort.SAOut, EqSwitchMatrix.InstrPort.SAIn);
                        Eq.Site[site].SwMatrix.DefinePath("Y2M", Operation.PwrSensorToPwrSensorIn_From_O2, EqSwitchMatrix.DutPort.PwrSensor, EqSwitchMatrix.InstrPort.PwrSensorIn_From_O2);

                        if (isMagicBox)
                        {
                            Eq.Site[site].SwMatrix.ActivatePath("Y2M", Operation.SGCplrToSGIn);
                            Eq.Site[site].SwMatrix.ActivatePath("Y2M", Operation.SAOutToSAIn);
                            Eq.Site[site].SwMatrix.ActivatePath("Y2M", Operation.PwrSensorToPwrSensorIn_From_O2);
                        }
                        #endregion 
                         * 
                        #region NF Power Cal Switch


                        //Eq.Site[site].SwMatrix.DefinePath("MANUAL", Operation.NONE, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #endregion

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        // Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("NS1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);
                        #endregion

                        #region Need to update for RF2

                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N4toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N4);

                        Eq.Site[site].SwMatrix.ActivatePath("IN-MB1", Operation.N1toTx);
                        Eq.Site[site].SwMatrix.ActivatePath("IN-HB2", Operation.N1toTx);
                        Eq.Site[site].SwMatrix.ActivatePath("ANT1", Operation.N2toAnt);
                        Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N3toRx);
                        Eq.Site[site].SwMatrix.ActivatePath("OUT4", Operation.N6toRx);
                        #endregion


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        */

                        #endregion

                        #region Rev Y2 Pinot

                        #region NF Power Cal Switch


                        //Eq.Site[site].SwMatrix.DefinePath("MANUAL", Operation.NONE, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);



                        #endregion

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("NS1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);
                        #endregion

                        #region Need to update for RF2

                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N4);

                        //Eq.Site[site].SwMatrix.ActivatePath("IN-MB1", Operation.N1toTx);
                        //Eq.Site[site].SwMatrix.ActivatePath("IN-HB2", Operation.N1toTx);
                        //Eq.Site[site].SwMatrix.ActivatePath("ANT1", Operation.N2toAnt);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N3toRx);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT4", Operation.N6toRx);
                        #endregion



                        Eq.Site[site].SwMatrix.DefinePath("LMB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG); // for LNA full test 20190516
                        Eq.Site[site].SwMatrix.DefinePath("LMB", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA); // for LNA full test 20190516
                        Eq.Site[site].SwMatrix.DefinePath("LMB", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA); // for LNA full test 20190516

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        ////////////////////////////////////////////////////////////////////////

                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        break;
                    case EqSwitchMatrix.Rev.R:

                        #region Rev R

                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.ENAtoRFOUT_ANT2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4);

                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N4);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N5);

                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);

                        Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.ENAto2GTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.ENAtoTRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.ENAtoTRX3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.ENAtoDCS_PCS_RX, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.ENAtoDRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);

                        Eq.Site[site].SwMatrix.DefinePath("GSMTX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("DCSRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);


                        // Burhan // Added for 6-Port Topaz FBAR

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.N4toRx, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.N5toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.N6toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N4toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.N6toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.N4toRx, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.N5toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.N6toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N4toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.N6toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.N4toRx, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.N5toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.N6toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N6);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N4toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.N6toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N6);

                        //Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("2GMB", Operation.N6toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);

                        //Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("GSMRX", Operation.N6toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N6);

                        //Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N4toRx, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("TRX3", Operation.N6toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N6);

                        //Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("TRX2", Operation.N6toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);

                        //Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N2toAnt, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N3toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N5toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.N6toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);

                        #endregion Rev R

                        break;
                    case EqSwitchMatrix.Rev.C:

                        #region Rev C

                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRFOUT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoRFIN, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRFOUT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRFIN, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF3_FC_2p25G_To_3G);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH3, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSAtoRFOUT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSGtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRFOUT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF3_FC_2p25G_To_3G);
                        //Eq.Site[site].SwMatrix.DefinePath("B2", Operation.MeasureH3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRFOUT, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRFOUT, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA_HPF3_FC_2p25G_To_3G);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH3, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRFOUT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSGtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRFOUT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N3);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF3_FC_2p25G_To_3G);
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureH3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRFOUT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRFOUT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRFIN, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF3_FC_2p25G_To_3G);
                        //Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH3, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        #endregion Rev C

                        break;
                    case EqSwitchMatrix.Rev.E:

                        #region Rev E

                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureCpl, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);


                        //FDD Bands
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4); // Unused port




                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3); // Unused port
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3);





                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3); // Unused port
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3); 





                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4); // Unused port
                        //// cant do coupler because port 4 is being used by RX 

                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4); // Unused port
                        //Eq.Site[site].SwMatrix.DefinePath("B4", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3); 



                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3);



                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4);
                        // Eq.Site[site].SwMatrix.DefinePath("MBHD", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3);






                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3); // Unused port
                        Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4);
                        // Eq.Site[site].SwMatrix.DefinePath("B2", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3);



                        // HD bands Differential TX input

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4);



                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4);



                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B41B", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4);



                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("B38X", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3);



                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        //Eq.Site[site].SwMatrix.DefinePath("BXGP", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N3);



                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("HBHD", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N4);



                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B40F", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N4);


                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.ENAtoTX_Diff_Plus, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.ENAtoRFOUT_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.ENAtoTX_Diff_Neg, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("B41F", Operation.ENAtoCPL, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N4);


                        //Eq.Site[site].SwMatrix.DefinePath("ISORX", Operation.ENAtoRX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3); //RX3
                        //Eq.Site[site].SwMatrix.DefinePath("ISORX", Operation.ENAtoRX2, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N4); //RX4
                        //Eq.Site[site].SwMatrix.DefinePath("ISORX", Operation.ENAtoTRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3); //RX3
                        //Eq.Site[site].SwMatrix.DefinePath("ISORX", Operation.ENAtoTRX3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N4); //RX4


                        #endregion Rev E

                        break;
                    case EqSwitchMatrix.Rev.O2:

                        #region O2
                        Eq.Site[site].SwMatrix.DefinePath(Operation.SGCplrToSGIn, EqSwitchMatrix.DutPort.SGCplr, EqSwitchMatrix.InstrPort.SGIn);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.SAOutToSAIn, EqSwitchMatrix.DutPort.SAOut, EqSwitchMatrix.InstrPort.SAIn);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.PwrSensorToPwrSensorIn_From_O2, EqSwitchMatrix.DutPort.PwrSensor, EqSwitchMatrix.InstrPort.PwrSensorIn_From_O2);
                        Eq.Site[site].SwMatrix.ActivatePath(EqSwitchMatrix.DutPort.SGCplr, EqSwitchMatrix.InstrPort.SGIn);
                        Eq.Site[site].SwMatrix.ActivatePath(EqSwitchMatrix.DutPort.SAOut, EqSwitchMatrix.InstrPort.SAIn);
                        Eq.Site[site].SwMatrix.ActivatePath(EqSwitchMatrix.DutPort.PwrSensor, EqSwitchMatrix.InstrPort.PwrSensorIn_From_O2);

                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath(Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);
                        // B1
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTRX1, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTRX2, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTRX4, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTRX5, EqSwitchMatrix.DutPort.A26, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B1RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B1RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2

                        Eq.Site[site].SwMatrix.DefinePath("B1L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        // B3

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B3RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B3RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B3RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B3RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2
                        Eq.Site[site].SwMatrix.DefinePath("B3RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        // B4

                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B4", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2

                        Eq.Site[site].SwMatrix.DefinePath("B4RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B4RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B4RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B4RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2

                        Eq.Site[site].SwMatrix.DefinePath("B4L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B4L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        // B66ROW
                        // 
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2

                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROW", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B66ROWL", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROWL", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROWL", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66ROWRX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROWRX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66ROWRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B66ROWRX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2

                        // B66US
                        // 
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66US", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B66USL", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66USL", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66USL", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66USRX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66USRX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B66USRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B66USRX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2
                        // B25
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B25L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B25RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B25RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //B2RX
                        Eq.Site[site].SwMatrix.DefinePath("B2RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B2RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B2RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B2RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B2RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        //B32RX
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B32RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        // B34
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B34L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//MB1
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);//MB2
                        // B39
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B39L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B39RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B39RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //B7
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX5, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//mb1

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //
                        //B7
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX5, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);//mb1

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B30RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B30RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B30RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //B40
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);// OUT4
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40CARX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        //B41
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41RX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG_Amp);
                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41PRX", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41L", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41L", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41L", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        // HB2G
                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("HB2G", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("HB2GL", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HB2GL", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HB2GL", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        // HBAUX1
                        Eq.Site[site].SwMatrix.DefinePath("HBAUX1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBAUX1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("HBAUX1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        // HBAUX2
                        Eq.Site[site].SwMatrix.DefinePath("HBAUX2", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBAUX2", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("HBAUX2", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        //B21MIMO
                        Eq.Site[site].SwMatrix.DefinePath("B21MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B21MIMO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B21MIMO", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B21MIMO", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B21MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B21MIMO", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        //B34MIMO
                        Eq.Site[site].SwMatrix.DefinePath("B34MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34MIMO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("B34MIMO", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);

                        //HBMIMO
                        Eq.Site[site].SwMatrix.DefinePath("HBMIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBMIMO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBMIMO", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBMIMO", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBMIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("HBMIMO", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        //MBMIMO1
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        //MBMIMO2
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO2", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO2", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO2", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO2", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG_Amp);

                        Eq.Site[site].SwMatrix.DefinePath("MBMIMO2", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A28, EqSwitchMatrix.InstrPort.VSA);
                        // B3ISO
                        Eq.Site[site].SwMatrix.DefinePath("B3ISO", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3ISO", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3ISO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3ISO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3ISO", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);
                        // B39ISO
                        Eq.Site[site].SwMatrix.DefinePath("B39ISO", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39ISO", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39ISO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39ISO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39ISO", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG_Amp);
                        // B7ISO
                        Eq.Site[site].SwMatrix.DefinePath("B7ISO", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7ISO", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7ISO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7ISO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7ISO", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);

                        // B41ISO
                        Eq.Site[site].SwMatrix.DefinePath("B41ISO", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41ISO", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41ISO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41ISO", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41ISO", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG_Amp);

                        #endregion

                        break;
                    case EqSwitchMatrix.Rev.JM:

                        #region Rev JM - Pinot Modular Switch


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NStoAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N5toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N6toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N6);



                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.J1:
                        #region Rev J1


                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);


                        #region Power Cal Switch

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //TxLeakage
                        //Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.TxLeakage, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.TxLeakage, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.TxLeakage, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.TxLeakage, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.Tx_Leakage);

                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);


                        //Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.TxLeakage, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.TxLeakage, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.Tx_Leakage);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region PA

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region Spara

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N2toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N2toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);

                        //dummy
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N2toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N2toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        //

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);



                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N6toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N6toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N7toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N7);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N7toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N7);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N8toRx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N8);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N8toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N8);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N9toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N9);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N9toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N9);

                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.VSAtoRX);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT2", Operation.VSAtoRX);

                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N6toRx);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT-DRX", Operation.N6toRx);


                        #endregion

                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2D:

                        #region Rev Y2D - Joker RF2 Switch


                        //Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        //Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NStoAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N6toRx, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.N6);

                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2DPN:

                        #region Rev Y2D - Pinot RF2 Switch


                        //Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        //Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-LMB", Operation.N2toAnt, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT-UAT", Operation.NStoAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N6toRx, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.N6);

                        #endregion
                        break;
                    case EqSwitchMatrix.Rev.Y2DNightHawk:
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77F", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);  //by Hosein
                        // Eq.Site[site].SwMatrix.DefinePath("N77F", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA); //by Hosein

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);
                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.MD_RF1:
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        //Eq.Site[site].SwMatrix.DefinePath("N77F", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);  //by Hosein
                        // Eq.Site[site].SwMatrix.DefinePath("N77F", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA); //by Hosein

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);


                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);
                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.EDAM_Modular_RF1:

                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);


                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        break;

                    case EqSwitchMatrix.Rev.Modular_RF1_QUADSITE:

                        #region Power Cal Switch Site 0
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("ANTL", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);


                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region Power Cal Switch Site 1
                        Eq.Site[site].SwMatrix.DefinePath("NONE_1", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("_1", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX_1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("ANTL_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSG);   //By Hosein
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79_1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("ANTL_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSGtoANT4, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSG);


                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.MeasureH3_ANT1, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.MeasureH3_ANT2, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.MeasureH3_ANT3, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77A_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77B_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77C_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.MeasureCpl, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);  //by Hosein for cpl
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77D_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79A_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79B_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79C_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79D_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX_1", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        break;
                }
            }
        }

        /// <summary>
        /// Pinot variant. Backup for RF2.
        /// </summary>
        public void InitializeSwitchMatrix2(ITesterSite tester)
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                switch (site)  // ability to use different switch matrix models per site
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        string visaAlias = tester.GetVisaAlias("DIO", site);
                        EqSwitchMatrix.Rev swRev = tester.GetSwitchMatrixRevision();
                        Eq.Site[site].SwMatrix = EqSwitchMatrix.Get(visaAlias, swRev);
                        break;
                }

                switch (Eq.Site[site].SwMatrix.Revision)
                {
                    case EqSwitchMatrix.Rev.JM:

                        #region Rev JM - Pinot Modular Switch


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NStoAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N5toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N6toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N6);



                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2:

                        #region Rev Y2 JOKER
                        /*
                        #region NF Power Cal Switch


                        //Eq.Site[site].SwMatrix.DefinePath("MANUAL", Operation.NONE, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #endregion

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        // Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("NS1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);
                        #endregion

                        #region Need to update for RF2

                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N4toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N4);

                        Eq.Site[site].SwMatrix.ActivatePath("IN-MB1", Operation.N1toTx);
                        Eq.Site[site].SwMatrix.ActivatePath("IN-HB2", Operation.N1toTx);
                        Eq.Site[site].SwMatrix.ActivatePath("ANT1", Operation.N2toAnt);
                        Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N3toRx);
                        Eq.Site[site].SwMatrix.ActivatePath("OUT4", Operation.N6toRx);
                        #endregion


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        */

                        #endregion

                        #region Rev Y2 Pinot

                        #region NF Power Cal Switch


                        //Eq.Site[site].SwMatrix.DefinePath("MANUAL", Operation.NONE, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #endregion

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("NS1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);
                        #endregion

                        #region Need to update for RF2

                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N4);

                        //Eq.Site[site].SwMatrix.ActivatePath("IN-MB1", Operation.N1toTx);
                        //Eq.Site[site].SwMatrix.ActivatePath("IN-HB2", Operation.N1toTx);
                        //Eq.Site[site].SwMatrix.ActivatePath("ANT1", Operation.N2toAnt);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N3toRx);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT4", Operation.N6toRx);
                        #endregion


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        break;

                    case EqSwitchMatrix.Rev.J1:
                        #region Rev J1


                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);


                        #region Power Cal Switch

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //TxLeakage
                        //Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.TxLeakage, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.TxLeakage, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.TxLeakage, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.TxLeakage, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.Tx_Leakage);

                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);


                        //Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.TxLeakage, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.TxLeakage, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.Tx_Leakage);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region PA

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region Spara

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N2toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N2toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);

                        //dummy
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N2toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N2toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        //

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);



                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N6toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N6toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N7toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N7);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N7toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N7);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N8toRx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N8);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N8toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N8);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N9toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N9);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N9toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N9);

                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.VSAtoRX);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT2", Operation.VSAtoRX);

                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N6toRx);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT-DRX", Operation.N6toRx);


                        #endregion

                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2D:

                        #region Rev Y2D - Pinot RF2 Switch


                        //Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        //Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NStoAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N6toRx, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.N6);

                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2DPN:

                        #region Rev Y2D - Pinot RF2 Switch


                        //Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        //Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-21", Operation.N2toAnt, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N6toRx, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.N6);

                        #endregion
                        break;

                }
            }
        }

        public void InitializeSwitchMatrix2(ITesterSite tester, string DIO_Alias)
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                switch (site)  // ability to use different switch matrix models per site
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        string visaAlias = tester.GetVisaAlias(GlobalVariables.DIOAlias, site);
                        EqSwitchMatrix.Rev swRev = GlobalVariables.SwitchMatrixBox; //tester.GetSwitchMatrixRevision();
                        Eq.Site[site].SwMatrix = EqSwitchMatrix.Get(visaAlias, swRev);
                        break;
                }

                switch (Eq.Site[site].SwMatrix.Revision)
                {
                    case EqSwitchMatrix.Rev.JM:

                        #region Rev JM - Pinot Modular Switch


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NStoAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N5toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N6toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N6);



                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2:

                        #region Rev Y2 JOKER
                        /*
                        #region NF Power Cal Switch


                        //Eq.Site[site].SwMatrix.DefinePath("MANUAL", Operation.NONE, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #endregion

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        // Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("NS1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);
                        #endregion

                        #region Need to update for RF2

                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N4toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N4);

                        Eq.Site[site].SwMatrix.ActivatePath("IN-MB1", Operation.N1toTx);
                        Eq.Site[site].SwMatrix.ActivatePath("IN-HB2", Operation.N1toTx);
                        Eq.Site[site].SwMatrix.ActivatePath("ANT1", Operation.N2toAnt);
                        Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N3toRx);
                        Eq.Site[site].SwMatrix.ActivatePath("OUT4", Operation.N6toRx);
                        #endregion


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        */

                        #endregion

                        #region Rev Y2 Pinot

                        #region NF Power Cal Switch


                        //Eq.Site[site].SwMatrix.DefinePath("MANUAL", Operation.NONE, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);


                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #endregion

                        #region Power Cal Switch
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("NS1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);
                        #endregion

                        #region Need to update for RF2

                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.N2toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N4);

                        //Eq.Site[site].SwMatrix.ActivatePath("IN-MB1", Operation.N1toTx);
                        //Eq.Site[site].SwMatrix.ActivatePath("IN-HB2", Operation.N1toTx);
                        //Eq.Site[site].SwMatrix.ActivatePath("ANT1", Operation.N2toAnt);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N3toRx);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT4", Operation.N6toRx);
                        #endregion


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B32", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////

                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);

                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        //Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSGtoANT_UAT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("DRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        //Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        break;

                    case EqSwitchMatrix.Rev.J1:
                        #region Rev J1


                        //Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);


                        #region Power Cal Switch

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSGtoANT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.VSAtoANT, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.MeasureH2, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //TxLeakage
                        //Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.TxLeakage, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.TxLeakage, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.TxLeakage, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.TxLeakage, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.Tx_Leakage);

                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);


                        //Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.TxLeakage, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.Tx_Leakage);
                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.TxLeakage, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.Tx_Leakage);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region PA

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B1", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);


                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B25", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B3", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B66", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B34", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B39", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B7", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B30", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B41H", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B40A", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("MBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        //////////////////////////////////////////////////////////////////////
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("HBTHRU", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MIMO", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.VSA);

                        #endregion

                        #region Spara

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N2toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N2toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);

                        //dummy
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A1, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N2toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N2toTx, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        //

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N3toAnt, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N4toAnt, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N5toAnt, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.N5);



                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.NS1);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A6, EqSwitchMatrix.InstrPort.NS2);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NS3toAnt3, EqSwitchMatrix.DutPort.A7, EqSwitchMatrix.InstrPort.NS3);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N6toRx, EqSwitchMatrix.DutPort.A8, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N6toRx, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N7toRx, EqSwitchMatrix.DutPort.A10, EqSwitchMatrix.InstrPort.N7);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N7toRx, EqSwitchMatrix.DutPort.A11, EqSwitchMatrix.InstrPort.N7);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N8toRx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N8);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N8toRx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N8);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N9toRx, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N9);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N9toRx, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N9);

                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.VSAtoRX);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT2", Operation.VSAtoRX);

                        //Eq.Site[site].SwMatrix.ActivatePath("OUT1", Operation.N6toRx);
                        //Eq.Site[site].SwMatrix.ActivatePath("OUT-DRX", Operation.N6toRx);


                        #endregion

                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2D:

                        #region Rev Y2D - Pinot RF2 Switch


                        //Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        //Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB2", Operation.N1toTx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT3", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NStoAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NStoAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT3", Operation.NStoAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.NS);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N6toRx, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.N6);

                        #endregion
                        break;

                    case EqSwitchMatrix.Rev.Y2DPN:

                        #region Rev Y2D - Pinot RF2 Switch


                        //Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        //Eq.Site[site].SwMatrix.DefinePath("TERM", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A5, EqSwitchMatrix.InstrPort.TERM);

                        Eq.Site[site].SwMatrix.DefinePath("IN-MB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN-HB1", Operation.N1toTx, EqSwitchMatrix.DutPort.A13, EqSwitchMatrix.InstrPort.N1);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A23, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-UAT", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT-21", Operation.N2toAnt, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.N2);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-DRX", Operation.N3toRx, EqSwitchMatrix.DutPort.A17, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("IN-MLB", Operation.N4toRx, EqSwitchMatrix.DutPort.A25, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-MIMO", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("IN-GSM", Operation.N6toRx, EqSwitchMatrix.DutPort.A21, EqSwitchMatrix.InstrPort.N6);

                        #endregion
                        break;
                    case EqSwitchMatrix.Rev.Y2DNightHawk:
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #region RF1 Switch Settings
                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);
                        #endregion RF1 Switch Settings

                        #region RF2 Switch Settings

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.N1toTx, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.N1toTx, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.N1);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1_N77", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1_N79", Operation.N2toAnt, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.N3toRx, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.N3);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.N4toRx, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.N4);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.N5toRx, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.N5toRx, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.N5);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.N6toRx, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.N6);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2_N77", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2_N79", Operation.N2toAnt, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.N2toAnt, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.N2toAnt, EqSwitchMatrix.DutPort.A22, EqSwitchMatrix.InstrPort.N2);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1_N77", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1_N79", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2_N77", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2_N79", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT1", Operation.NS1toAnt1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.NS);
                        Eq.Site[site].SwMatrix.DefinePath("NS-ANT2", Operation.NS2toAnt2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.NS);

                        #endregion RF2 Switch Settings
                        break;
                    case EqSwitchMatrix.Rev.MD_RF1:
                        Eq.Site[site].SwMatrix.DefinePath("NONE", Operation.NONE, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.NONE);
                        Eq.Site[site].SwMatrix.DefinePath("", Operation.ANTtoTERM, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.TERM);

                        #region RF1 Switch Settings
                        Eq.Site[site].SwMatrix.DefinePath("IN-FBRX", Operation.VSGtoTX, EqSwitchMatrix.DutPort.A12, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTX1, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTX2, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSG);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSG);

                        Eq.Site[site].SwMatrix.DefinePath("IN1-N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("IN2-N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A2, EqSwitchMatrix.InstrPort.VSGtoHMU);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSGtoTXFor_HMU, EqSwitchMatrix.DutPort.A3, EqSwitchMatrix.InstrPort.VSGtoHMU);

                        //ANT1
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT1", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT1, EqSwitchMatrix.DutPort.A4, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT2
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANT2", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT2, EqSwitchMatrix.DutPort.A14, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        //ANT3                        
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("ANTU", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.HMUtoVSAtoANT, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.HMUtoVSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.MeasureH2_ANT3, EqSwitchMatrix.DutPort.A15, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);

                        Eq.Site[site].SwMatrix.DefinePath("OUT1-N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT4-N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT-FBRX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT3-N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("OUT2-N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N77RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B42", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("B48", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX1, EqSwitchMatrix.DutPort.A16, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX3, EqSwitchMatrix.DutPort.A24, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A19, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX2, EqSwitchMatrix.DutPort.A20, EqSwitchMatrix.InstrPort.VSA);
                        Eq.Site[site].SwMatrix.DefinePath("N79RX", Operation.VSAtoRX4, EqSwitchMatrix.DutPort.A18, EqSwitchMatrix.InstrPort.VSA);

                        //Eq.Site[site].SwMatrix.DefinePath("OUT-MLB", Operation.VSAtoRX, EqSwitchMatrix.DutPort.A9, EqSwitchMatrix.InstrPort.VSA);

                        Eq.Site[site].SwMatrix.DefinePath("HPF", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA_HPF2p7GHz);
                        Eq.Site[site].SwMatrix.DefinePath("BYPASS", Operation.MeasureH2, EqSwitchMatrix.DutPort.NONE, EqSwitchMatrix.InstrPort.VSA);
                        #endregion RF1 Switch Settings
                        break;

                }
            }
        }

    }

    public class PaEquipmentInitializer : IEquipmentInitializer
    {
        private ITesterSite m_modelTester;
        private HsdioModel m_modelEqHsdio;

        public void SetTester(ITesterSite tester)
        {
            m_modelEqHsdio = new HsdioModel();
            m_modelTester = tester;
        }

        public void InitializeSwitchMatrix(bool isMagicBox)
        {
            SwitchMatrixModel sw = new SwitchMatrixModel();
            sw.InitializeSwitchMatrix3(m_modelTester, isMagicBox);
        }

        public Dictionary<string, string> Digital_Definitions_Part_Specific
        {
            get
            {
                return m_modelEqHsdio.Digital_Definitions_Part_Specific;
            }
        }
        public bool InitializeHSDIO()
        {
            return m_modelEqHsdio.InitializeHSDIO2(m_modelTester);
        }

        public bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion,
           Dictionary<string, string> TXQC, Dictionary<string, string> RXQC, TestLib.MipiTestConditions testConditions = null)
        {
            return m_modelEqHsdio.LoadVector(clothoRootDir, tcfCmosDieType, sampleVersion, TXQC, RXQC, testConditions);
        }

        public bool LoadVector(string clothoRootDir, string tcfCmosDieType, string sampleVersion)
        {
            return m_modelEqHsdio.LoadVector(clothoRootDir, tcfCmosDieType, sampleVersion);
        }

        public void InitializeSmu()
        {
            throw new NotImplementedException();
        }

        public void InitializeRF()
        {
            List<System.Threading.Tasks.Task> rfInitTasks =
                new List<System.Threading.Tasks.Task>();

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                //if (site % 2 == 0)
                //{
                Eq.Site[site].RF = EqRF.Get("VST_RFmx_" + site, site);
                byte thisSite = site;

                //rfInitTasks.Add(Task.Run(() => Eq.Site[thisSite].RF.Initialize()));
                Eq.Site[thisSite].RF.Initialize(EqTriggerArray);

                //}
                //else
                //{
                //    Eq.Site[site].RF = Eq.Site[site - 1].RF;
                //}
            }

            System.Threading.Tasks.Task.WaitAll(rfInitTasks.ToArray());
        }

        public ValidationDataObject InitializeDC(ClothoLibAlgo.Dictionary.Ordered<string, string[]> DcResourceTempList)
        {
            ValidationDataObject vdo = new ValidationDataObject();

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                foreach (string DcPinName in DcResourceTempList.Keys)
                {
                    string[] resourceDefSplit = DcResourceTempList[DcPinName][site].Split('.');

                    string VisaAlias = resourceDefSplit[0];
                    string Chan = resourceDefSplit[1];


                    try
                    {
                        Eq.Site[site].DC.Add(DcPinName, EqDC.Get(VisaAlias, Chan, DcPinName, site, true));
                    }
                    catch (Exception ex)
                    {
                        PromptManager.Instance.ShowError("DC Initialization", ex);
                        vdo.IsValidated = false;
                        //MessageBox.Show(e.ToString(), "DC Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //programLoadSuccess = false;
                    }
                }
            }

            return vdo;
        }

        public void InitializeChassis()
        {
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                Eq.Site[site].Chassis = new List<Chassis.Chassis_base>();

                for (int chassisNum = 1; chassisNum <= 3; chassisNum++)
                {
                    if (false && site == 0)
                        Eq.Site[site].Chassis.Add(new Chassis.Active_Chassis(""));
                    else
                        Eq.Site[site].Chassis.Add(new Chassis.None("Chassis " + chassisNum));
                }
            }
        }

        public void InitializeHandler(string handlerType, string visaAlias)
        {
            Eq.Handler = EqHandler.Get(!TestLib.ResultBuilder.LiteDriver, handlerType);
            Eq.Handler.Initialize(visaAlias);
        }

        public readonly Dictionary<byte, int[]> EqTriggerArray = new Dictionary<byte, int[]>()
        {
            //public enum TriggerLine
            //{
            //    None = 0, 
            //PxiTrig0 = 1,
            //PxiTrig1 = 2,
            //PxiTrig2 = 3,
            //PxiTrig3 = 4,
            //PxiTrig4 = 5,
            //PxiTrig5 = 6,
            //PxiTrig6 = 7,
            //PxiTrig7 = 8,
            //FrontPanel0 = 9,
            //FrontPanel1 = 10,
            //FrontPanel2 = 11,
            //FrontPanel3 == 12
            //}

            {0, new int[4]{1, 2, 3, 4} },
            {1, new int[4]{5, 6, 7, 8} }
        };
    }

}
