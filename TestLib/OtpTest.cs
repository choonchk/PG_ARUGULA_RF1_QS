using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using EqLib;
using ClothoLibAlgo;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using System.Diagnostics;
using WSD.Utility.OtpModule;
using System.Threading;
using ProductionLib;
using System.Text.RegularExpressions;

namespace TestLib
{


    public abstract class OtpTest : TimingBase, iTest
    {
        public bool Initialize(bool finalScript)
        {
            InitializeTiming2(this.TestCon.TestParaName);
            //Eq.Site[Site].HSDIO.AddVectorsToScript(TestCon.MipiCommands, finalScript);
            Initialize();//Initialize OtpModuleSerialTest/OtpMfgLotNumTest
            m_dupResultDetector = new DuplicateDetector();
            return true;
        }

        private DuplicateDetector m_dupResultDetector;
        public byte Site;
        public OtpTestConditions TestCon = new OtpTestConditions();
        public OtpTestResults TestResult;
        public static string mfg_ID = "000001";
        public static string engineeringMode = "";
        public static Dictionary<string, string>[] Unique_ID = new Dictionary<string, string>[Eq.NumSites];
        public HiPerfTimer uTimer = new HiPerfTimer();
        public static bool ENABLE2DID_VALIDATION = false;

        //Web Service 2.0 - DH
        public string Assembly2DID = "999";
        public static string Web2didData2 = "99999999";
        public static string Weblotid = "PT12345657890";
        public static bool WebByPass = false;
        public static bool[] mismatch2DID = new bool[] { false, false };
        public static int[] mismatch2DIDerr = new int[] { 0, 0 };
        public static bool[] Webservice2DIDflag = new bool[] { true, true };

        public int RunTest()
        {
            try
            {
                TestResult = new OtpTestResults();

                if (ResultBuilder.headerFileMode) return 0;

                SwBeginRun(Site);

                Eq.Site[Site].HSDIO.dutSlaveAddress = TestCon.SlaveAddr;
                                
                //Eq.Site[Site].HSDIO.SendNextVectors(false, TestCon.MipiCommands);
                //Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.4, 0.01);
                //Eq.Site[Site].DC["Vio"].ForceVoltage(0, 0.01);
                //Eq.Site[Site].DC["Vio"].ForceVoltage(1.8, 0.01);
                //EqHSDIO.dutSlaveAddress = TestCon.dutSlaveAdd;
                //string readback = Eq.Site[Site].HSDIO.RegRead("21");
                //otpBurnProcedure.Burn(Site, "E5", "CF");

                this.ConfigureVoltageAndCurrent();

                //Web Service 2.0
                if (Webservice2DIDflag[Site])
                {
                    Webservice2DIDflag[Site] = false;

                    string Key2DID = "2DID_OTPID";

                    if (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt"))
                    {
                        Key2DID = string.Format("2DID_OTPID_{0}", Site + 1);
                    }

                    string Camera2DIDstr = ATFCrossDomainWrapper.GetStringFromCache(Key2DID, "00000000000000000000");

                    ////For debug purpose
                    ////MessageBox.Show("GetCamera2DID():  " + Camera2DIDstr);

                    ///To support additional " ' " added at the front of the 2DID passed from Handler Plugin, so it can be viewed on excel
                    ///this part should be obsolete
                    if (Camera2DIDstr.Substring(0, 1) == "'")
                    {
                        Camera2DIDstr = Regex.Replace(Camera2DIDstr, @"[^\d]", "");
                    }

                    if (Camera2DIDstr != "")
                    {
                        if (Camera2DIDstr.Length > 16)
                        {
                            Assembly2DID = Camera2DIDstr.Substring(0, 8);
                        }
                        else
                        {
                            Assembly2DID = Camera2DIDstr;
                        }
                    }

                    //LoggingManager.Instance.LogInfoTestPlan("Assembly2DID is " + OtpTestEngine.Assembly2DID);
                    //LoggingManager.Instance.LogInfoTestPlan("web_2DID is " + OtpTestEngine.Web2didData2);

                    if (Assembly2DID == Web2didData2)
                    {
                        mismatch2DID[Site] = false;
                        mismatch2DIDerr[Site] = 0;
                    }
                    else if (WebByPass == true)
                    {
                        mismatch2DID[Site] = false;
                        mismatch2DIDerr[Site] = -1;
                    }
                    else
                    {
                        mismatch2DID[Site] = true;
                        mismatch2DIDerr[Site] = 1;
                        if (Assembly2DID == "999")
                        {
                            mismatch2DIDerr[Site] = 2;
                        }
                    }
                }

                RunTestCore();
                SwEndRun(Site);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }
        }

        public abstract void RunTestCore();
        public abstract void Initialize();
        public void BuildResults(ref ATFReturnResult results)
        {

            switch (TestCon.TestType.ToUpper())
            {
                case "OTP_BURN_LNA":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_MOD_ID":
                {
                    //m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("MOD", "MODULE") + "_ERROR", "", TestResult.NumBitErrors, 4);
                    m_dupResultDetector.Add(Site, TestCon.TestParaName + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_MOD_2DID":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName + "_PREOTPREADBACK", "", TestResult.Module_2DID_PreOTP, 4);
                    m_dupResultDetector.Add(Site, TestCon.TestParaName + "_STATUS", "", TestResult.NumBitErrors, 4);
                    TestResult.Module_2DID_PreOTP = -999999;
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_MFG_ID_TX":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("TX", "ERROR"), "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }


                case "OTP_BURN_MFG_ID_RX":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("RX", "ERROR"), "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_REV_ID":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_LOCK_BIT":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("LOCK_BIT", "LOCKBIT") + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_FBAR_NOISE_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("FBAR_NOISE_PASS_FLAG", "FBAR-NOISE-PASS-FLAG") + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_NFR_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("NFR_PASS_FLAG", "NFR-PASS-FLAG") + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_RF1_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("RF1_PASS_FLAG", "RF1-PASS-FLAG") + "_ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_BURN_CUSTOM":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName + "-ERROR", "", TestResult.NumBitErrors, 4);
                    TestResult.NumBitErrors = 0;
                    break;
                }

                case "OTP_READ_MOD_ID":
                {
                    m_dupResultDetector.Add(Site, "OTP_MODULE_ID", "", TestResult.Module_ID, 4);
                    break;
                }

                case "OTP_READ_MOD_2DID":
                {
                    ATFCrossDomainWrapper.SetMfgIDAndModuleIDBySite(Site + 1, TestResult.MFG_ID, TestResult.Camera_2DID_Partial);
                        
                    m_dupResultDetector.Add(Site, "M_OTP_MODULE_ID", "", TestResult.Module_2DID, 1);

                    if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                    {
                        Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.Module_2DID.ToString();
                    }
                    else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.Module_2DID.ToString()); }

                    string ModuleIDstr = TestResult.Module_2DID.ToString("D12");
                    m_dupResultDetector.Add(Site, "M_MIPI_LNA_CMOS-PCB-LOT-ID_NOTE_EC_7-6_EB_7-0", "", long.Parse(ModuleIDstr.Substring(0,4)), 1);
                    m_dupResultDetector.Add(Site, "M_MIPI_CMOS-PCB-PANEL-ID_NOTE_ED_7-7_EC_5-0", "", long.Parse(ModuleIDstr.Substring(4, 2)), 1);
                    m_dupResultDetector.Add(Site, "M_MIPI_LNA-PCB-STRIP-ID_NOTE_41_6-3", "", long.Parse(ModuleIDstr.Substring(6, 2)), 1);
                    m_dupResultDetector.Add(Site, "M_MIPI_LNA-PCB-MODULE-ID_NOTE_41_2-0_40_7-0", "", long.Parse(ModuleIDstr.Substring(8, 4)), 1);
                    m_dupResultDetector.Add(Site, "M_MIPI_PCB-ID", "", long.Parse(ModuleIDstr.Substring(0, 8)), 1);


                    int efuse_vs_2did = -999;
                    if (ENABLE2DID_VALIDATION)
                    {
                        if (OTP_Procedure.Read_Camera_2DID(Site, 11) == TestResult.Module_2DID)
                        {
                            efuse_vs_2did = 0;
                        }
                        else
                        {
                            efuse_vs_2did = 1;
                        }
                    }
                    else
                    {
                        efuse_vs_2did = -1;
                    }
                    m_dupResultDetector.Add(Site, "M_OTP_MODULE_ID_2DID_DELTA", "", efuse_vs_2did, 1);
                    break;
                }

                case "OTP_READ_CM_ID":
                {
                    m_dupResultDetector.Add(Site, "M_MIPI_CM-ID", "", TestResult.CM_ID, 1);
                    break;
                }

                case "OTP_READ_MFG_ID":
                {
                    m_dupResultDetector.Add(Site, "M_MFG_ID", "", TestResult.MFG_ID, 1);
                    m_dupResultDetector.Add(Site, "M_OTP_READ-MFG-ID-ERROR", "", TestResult.MFG_ID_ReadError, 1);
                    break;
                }

                case "OTP_READ_LOCK_BIT":
                {
                    m_dupResultDetector.Add(Site, "M_" + "OTP_LOCKBIT_STATUS", "", Convert.ToDouble(TestResult.Lock_Bit), 1, TestCon.RequiresDataCheckFirst);
                    break;
                }

                case "OTP_READ_FBAR_NOISE_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, "M_" + "OTP_FBAR-NOISE-FLAG_STATUS", "", Convert.ToDouble(TestResult.FBAR_Noise_Pass_Flag), 1, TestCon.RequiresDataCheckFirst);
                    break;
                }

                case "OTP_READ_NFR_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, "M_" + "OTP_NFR-FLAG_STATUS", "", Convert.ToDouble(TestResult.FBAR_Noise_Pass_Flag), 1, TestCon.RequiresDataCheckFirst);
                    break;
                }

                case "OTP_READ_RF1_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, "M_" + "OTP_RF1-PA-FLAG_STATUS", "", Convert.ToDouble(TestResult.RF1_Pass_Flag), 1, TestCon.RequiresDataCheckFirst);
                    break;
                }

                case "OTP_READ_RF2_PASS_FLAG":
                {
                    m_dupResultDetector.Add(Site, "M_" + "OTP_RF2-SPARA-FLAG_STATUS", "", Convert.ToDouble(TestResult.RF2_Pass_Flag), 1, TestCon.RequiresDataCheckFirst);
                    break;
                }

                case "OTP_READ_REV_ID":
                {
                    m_dupResultDetector.Add(Site, "M_" + "OTP_REV_ID", "", TestResult.REV_ID, 1);
                    break;
                }

                case "OTP_READ_TX_X":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_CMOS-TX-X", "", TestResult.TX_X, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.TX_X.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.TX_X.ToString()); }
                        break;
                    }

                case "OTP_READ_TX_Y":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_CMOS-TX-Y", "", TestResult.TX_Y, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.TX_Y.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.TX_Y.ToString()); }
                        break;
                    }

                case "OTP_READ_WAFER_LOT":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_CMOS-TX-WAFER-LOT", "", TestResult.WAFER_LOT, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.WAFER_LOT.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.WAFER_LOT.ToString()); }
                        break;
                    }

                case "OTP_READ_WAFER_ID":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_CMOS-TX-WAFER-ID", "", TestResult.WAFER_ID, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.WAFER_ID.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.WAFER_ID.ToString()); }
                        break;
                    }

                case "OTP_READ_LNA_X":
                    { 
                        m_dupResultDetector.Add(Site, "M_MIPI_LNA-X", "", TestResult.LNA_X, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.LNA_X.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.LNA_X.ToString()); }
                        break;
                    }

                case "OTP_READ_LNA_Y":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_LNA-Y", "", TestResult.LNA_Y, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.LNA_Y.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.LNA_Y.ToString()); }
                        break;
                    }

                case "OTP_READ_LNA_WAFER_LOT":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_LNA-WAFER-LOT", "", TestResult.LNA_WAFER_LOT, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.LNA_WAFER_LOT.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.LNA_WAFER_LOT.ToString()); }
                        break;
                    }

                case "OTP_READ_LNA_WAFER_ID":
                    {
                        m_dupResultDetector.Add(Site, "M_MIPI_LNA-WAFER-ID", "", TestResult.LNA_WAFER_ID, 1);
                        if (Unique_ID[Site].ContainsKey(TestCon.TestType.ToUpper()))
                        {
                            Unique_ID[Site][TestCon.TestType.ToUpper()] = TestResult.LNA_WAFER_ID.ToString();
                        }
                        else { Unique_ID[Site].Add(TestCon.TestType.ToUpper(), TestResult.LNA_WAFER_ID.ToString()); }
                        break;
                    }

                case "OTP_READ_BYTES": //20190709 new OtpTest
                    {
         
                        m_dupResultDetector.Add(Site, TestCon.TestParaName, "", TestResult.ReadBytes, 1);
                        
                        break; 
                    }

                case "OTP_CHECK_BIT": //20190709 new OtpTest
                    {
                        if (TestCon.TestParaName.Contains("READ-RF1-PASS-FLAG"))
                        {
                            m_dupResultDetector.Add(Site, TestCon.TestParaName, "", Convert.ToDouble(TestResult.CheckBit), 1, true);
                        }
                        else
                        {
                            m_dupResultDetector.Add(Site, TestCon.TestParaName, "", Convert.ToDouble(TestResult.CheckBit), 1);
                        }
                        break;
                    }

                case "OTP_MOD_ID_SELECT":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("MOD", "MODULE") + "-FLAG", "", TestResult.Mod_ID_Flag, 1);
                    break;
                }

                case "OTP_2DID_SELECT":
                {
                    m_dupResultDetector.Add(Site, TestCon.TestParaName.Replace("MOD", "MODULE") + "-FLAG", "", TestResult.Mod_ID_Flag, 1);
                    break;
                }
            }
        }



        private void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                string msg = String.Format("ForceVoltage on pin {0}", pinName);
                SwStartRun(msg, Site);
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper()))
                {
                    SwStopRun(msg, Site);
                    continue; // don't force voltage on MIPI pins                    
                }

                if (pinName.ToUpper() == "VIO1")
                {
                    Eq.Site[Site].HSDIO.ReInitializeVIO(TestCon.DcSettings[pinName].Volts);
                    SwStopRun(msg, Site);
                    continue;
                }

                Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, TestCon.DcSettings[pinName].Current);
                SwStopRun(msg, Site);
            }

            if (TestCon.VIORESET)
            {
                Eq.Site[Site].HSDIO.SendVector(EqHSDIO.Reset);
            }

            if (TestCon.VIO32MA)
            {
                Eq.Site[Site].HSDIO.SendVector(EqHSDIO.PPMUVioOverrideString.VIOON.ToString());
            }
        }
    }

    public class OtpTestConditions
    {
        public string TestParaName;
        public string SlaveAddr;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public string Module_ID_to_Burn;
        public string MFG_ID_to_Burn;
        public string REV_ID_to_Burn;
        public string TestType;
        public bool InitTest = false;
        public bool RequiresDataCheckFirst = false;
        public bool VIO32MA = false;
        public bool VIORESET = false;
        public bool AdditionalOTPSeqRequired = false;
    }

    public class OtpTestResults
    {
        //public int ExpectedValue;
        //public int ReadValue;
        public int Mod_ID_Flag;  //used to bin out specific parts to Bin one if the Mod_ID matched a list from TCF
        public int NumBitErrors;
        public bool BurnPass;
        //public int ModuleID_burned;
        //public int MfgID_Burned;

        public int ReadBytes;
        public bool CheckBit;

        public int Module_ID;
        public long Module_2DID;
        public long MFG_ID_RX = 10;
        public long Camera_2DID_Partial;
        public long Module_2DID_PreOTP;
        public int CM_ID;
        public int MFG_ID;
        public int MFG_ID_ReadError;
        public int REV_ID;
        public int TX_X, TX_Y, WAFER_LOT, WAFER_ID;
        public int LNA_X, LNA_Y, LNA_WAFER_LOT, LNA_WAFER_ID;
        public bool FBAR_Noise_Pass_Flag;
        public bool RF1_Pass_Flag;
        public bool RF2_Pass_Flag;
        public bool Lock_Bit;
    }

    public class OtpBurnTest : OtpTest
    {
        public override void Initialize()
        {

        }
        public override void RunTestCore()
        {
            string msg = "RunTestCore-" + TestCon.TestType.ToUpper();
            SwStartRun(msg, Site);
            bool doBurn = ((ResultBuilder.FailedTests[Site].Count == 0) && (ResultBuilder.FailedQctestCount(Site) == 0) && (mismatch2DID[Site]==false));

            Eq.Site[Site].HSDIO.dutSlaveAddress = TestCon.SlaveAddr;


            if (doBurn) 
            {
                switch (TestCon.TestType.ToUpper())
                {
                    case "OTP_BURN_LNA":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_LNA(Site);
                        break;

                    case "OTP_BURN_MOD_ID":
                        //SwStartRun("mm_OTP_BURN_MOD_ID");
                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            //TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_Mod_ID(Site, TestCon.MFG_ID_to_Burn, TestCon.Module_ID_to_Burn);
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_Mod_ID_SJ(Site, TestCon.MipiCommands, TestCon.MFG_ID_to_Burn, TestCon.Module_ID_to_Burn);



                        //SwStopRun("mm_OTP_BURN_MOD_ID");
                        break;

                    case "OTP_BURN_MOD_2DID":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                        {
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_Mod_2DID(Site, out TestResult.Module_2DID_PreOTP);
                        }
                        break;

                    case "OTP_BURN_MFG_ID_TX":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                        {
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_MFG_ID_TX(Site, int.Parse(mfg_ID));
                        }
                        break;

                    case "OTP_BURN_MFG_ID_RX":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                        {
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_MFG_ID_RX(Site, int.Parse(mfg_ID));
                        }
                        break;

                    case "OTP_BURN_REV_ID":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_Rev_ID(Site, TestCon.REV_ID_to_Burn);
                        break;

                    case "OTP_BURN_FBAR_NOISE_PASS_FLAG":
                    case "OTP_BURN_NFR_PASS_FLAG":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_FBAR_Noise_Pass_Flag(Site);
                        break;


                    case "OTP_BURN_RF1_PASS_FLAG":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_RF1_Pass_Flag(Site);
                        break;

                    case "OTP_BURN_RF2_PASS_FLAG":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_RF2_Pass_Flag(Site);
                        break;

                    case "OTP_BURN_LOCK_BIT":

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_Lock_Bit(Site);
                        break;

                    case "OTP_BURN_CUSTOM": //20190709 New Otptest

                        if (TestCon.InitTest)   // do not burn OTP during Init Test
                            break;
                        else
                            TestResult.NumBitErrors = OTP_Procedure.OTP_Burn_Custom(Site, TestCon.MipiCommands);
                        break;


                    default:
                        MessageBox.Show("Warning: UnKnown OTP Test Type:" + TestCon.TestType.ToUpper() + "Check Spelling", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                }
            }
            else
            {
                TestResult.NumBitErrors = -2;
            }

            SwStopRun(msg, Site);

        }
    }


    public class OtpReadTest : OtpTest
    {
        public override void Initialize()
        {
            for(int i=0; i < Eq.NumSites; i++)
            {
                Unique_ID[i] = new Dictionary<string, string>();
            }
        }
        public override void RunTestCore()
        {
            Eq.Site[Site].HSDIO.dutSlaveAddress = TestCon.SlaveAddr;

            string msg = "RunTestCore-" + TestCon.TestType.ToUpper();
            SwStartRun(msg, Site);

            switch (TestCon.TestType.ToUpper())
            {
                case "OTP_READ_BYTES": //20190709 new OtpTest Mario add for LNA Wafer ID
                    TestResult.ReadBytes = OTP_Procedure.OTP_Read_Bytes(Site,TestCon.MipiCommands);
                    if (TestCon.TestParaName.Contains("OTP_LNA_WAFER-ID"))
                    {
                        TestResult.ReadBytes = (TestResult.ReadBytes >> 2) & (int)(Math.Pow(2, 8) - 1);
                    }
                    if (TestCon.TestParaName.Contains("OTP_STATUS_WAFER-ID"))
                    {
                        TestResult.ReadBytes = (TestResult.ReadBytes >> 2);
                    }
                    break;

                case "OTP_CHECK_BIT": //20190709 new OtpTest                    
                    TestResult.CheckBit = OTP_Procedure.OTP_Check_Bit(Site, TestCon.MipiCommands);
                    break;

                case "OTP_READ_MOD_ID":
                    TestResult.Module_ID = OTP_Procedure.OTP_Read_Mod_ID(Site);

                    if (TestResult.Module_ID == 0)
                        TestResult.Module_ID = OTP_Procedure.OTP_Read_Mod_ID(Site);
                    break;

                case "OTP_READ_MOD_2DID":
                    TestResult.MFG_ID = OTP_Procedure.OTP_Read_MFG_ID(Site);
                    TestResult.Module_2DID = OTP_Procedure.OTP_Read_Mod_2DID(Site);
                    TestResult.Camera_2DID_Partial = OTP_Procedure.Read_Camera_2DID(Site, 15);
                    break;

                case "OTP_READ_CM_ID":
                    TestResult.CM_ID = OTP_Procedure.OTP_Read_CM_ID(Site);
                    break;

                case "OTP_READ_MFG_ID":

                    TestResult.MFG_ID = OTP_Procedure.OTP_Read_MFG_ID(Site);
                    TestResult.MFG_ID_ReadError = -1;

                    if (engineeringMode.ToUpper() == "FALSE")
                    {
                        if (TestResult.MFG_ID == int.Parse(mfg_ID))
                        {
                            TestResult.MFG_ID_ReadError = 0;
                        }
                        else
                        {
                            TestResult.MFG_ID_ReadError = 1;
                        }
                    }
                    break;

                case "OTP_READ_LOCK_BIT":

                    TestResult.Lock_Bit = OTP_Procedure.OTP_Read_Lock_Bit(Site);
                    break;

                case "OTP_READ_FBAR_NOISE_PASS_FLAG":
                case "OTP_READ_NFR_PASS_FLAG":

                    TestResult.FBAR_Noise_Pass_Flag = OTP_Procedure.OTP_Read_FBAR_Noise_Pass_Flag(Site);
                    break;

                case "OTP_READ_RF1_PASS_FLAG":

                    TestResult.RF1_Pass_Flag = OTP_Procedure.OTP_Read_RF1_Pass_Flag(Site);
                    break;

                case "OTP_READ_RF2_PASS_FLAG":

                    TestResult.RF2_Pass_Flag = OTP_Procedure.OTP_Read_RF2_Pass_Flag(Site);
                    break;

                case "OTP_READ_REV_ID":

                    TestResult.REV_ID = OTP_Procedure.OTP_Read_Rev_ID(Site); 
                    break;

                case "OTP_READ_TX_X":

                    TestResult.TX_X = OTP_Procedure.OTP_Read_TX_X(Site);
                    break;

                case "OTP_READ_TX_Y":

                    TestResult.TX_Y = OTP_Procedure.OTP_Read_TX_Y(Site);
                    break;

                case "OTP_READ_WAFER_LOT":

                    TestResult.WAFER_LOT = OTP_Procedure.OTP_Read_WAFER_LOT(Site);
                    break;

                case "OTP_READ_WAFER_ID":

                    TestResult.WAFER_ID = OTP_Procedure.OTP_Read_WAFER_ID(Site);
                    break;

                case "OTP_READ_LNA_X":

                    TestResult.LNA_X = OTP_Procedure.OTP_Read_LNA_X(Site);
                    break;

                case "OTP_READ_LNA_Y":

                    TestResult.LNA_Y = OTP_Procedure.OTP_Read_LNA_Y(Site);
                    break;

                case "OTP_READ_LNA_WAFER_LOT":

                    TestResult.LNA_WAFER_LOT = OTP_Procedure.OTP_Read_LNA_WAFER_LOT(Site);
                    break;

                case "OTP_READ_LNA_WAFER_ID":

                    TestResult.LNA_WAFER_ID = OTP_Procedure.OTP_Read_LNA_WAFER_ID(Site);
                    break;

                case "OTP_MOD_ID_SELECT":

                    TestResult.Mod_ID_Flag = OTP_Procedure.OTP_ModID_Select(Site);
                    break;

                case "OTP_2DID_SELECT":

                    TestResult.Mod_ID_Flag = OTP_Procedure.OTP_2DID_Select(Site);
                    break;

                default:
                    MessageBox.Show("Warning: UnKnown OTP Test Type:" + TestCon.TestType.ToUpper() + "Check Spelling", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
            SwStopRun(msg, Site);
        }

    }

    public static class OTP_Procedure
    {
        public static string mfg_id_str = "-999999";
        // public static Dictionary<string, string> OTP_Register_Definitions;
        public static string LNA_OTP_Revision = "";
        public static bool[] isRx2ndMemory = new bool[4] { false, false, false, false };
        public static bool[] AdditionalOTPSeqRequired = new bool[4] { false, false, false, false };
        public static Dictionary<int, List<long>> ModuleIDSelect = new Dictionary<int, List<long>>(); // used to store list of mod_IDs of parts to Bin out. List comes from TCF
        public static Dictionary<string, string> Str2DIDSelect = new Dictionary<string, string>(); // used to store list of mod_IDs of parts to Bin out. List comes from TCF
        public static bool EnableModuleIDselect = false;
        public static bool Enable2DIDselect = false;
        public static long[] previousCamera2DID = new long[4] { -999999, -999999, -999999, -999999 };

        public static bool EnableOTPburnTemplate = false;

        public static int OTP_Burn_LNA(int Site)
        {
            Stopwatch myWatch = new Stopwatch();
            myWatch.Start();

            int OTP_Burn_failure_Count = -999999;
            bool VerifyPassed = false;

            Eq.Site[Site].HSDIO.SendVector("VIOOFF");
            Thread.Sleep(5);

            Eq.Site[Site].HSDIO.SendVector("VIOON");
            Thread.Sleep(5);

            Eq.Site[Site].DC["Vlna"].ForceVoltage(2.5, .1);

            // check if previously burned
            VerifyPassed = Eq.Site[Site].HSDIO.SendVector("RXOTPVERIFYREV" + LNA_OTP_Revision);
            OTP_Burn_failure_Count = Eq.Site[Site].HSDIO.GetNumExecErrors("RXOTPVERIFYrev" + LNA_OTP_Revision);

            if (VerifyPassed)  // no bit errors means the part was already burned
            {
                OTP_Burn_failure_Count = -999999;
                return OTP_Burn_failure_Count;
            }


            Thread.Sleep(5);
            Eq.Site[Site].HSDIO.SendVector("RXOTPBURNREV" + LNA_OTP_Revision);
            Thread.Sleep(5);


            Eq.Site[Site].DC["Vlna"].ForceVoltage(1.8, 0.1);


            Eq.Site[Site].HSDIO.SendVector("VIOOFF");
            Thread.Sleep(5);

            Eq.Site[Site].HSDIO.SendVector("VIOON");
            Thread.Sleep(5);


            Thread.Sleep(5);
            //OTP_Burn_failure_Count = Eq.Site[Site].HSDIO.SendVector_Return_Error_Count("RXOTPVERIFY");
            Convert.ToUInt16(Eq.Site[Site].HSDIO.SendVector("RXOTPVERIFYrev" + LNA_OTP_Revision));
            OTP_Burn_failure_Count = Eq.Site[Site].HSDIO.GetNumExecErrors("RXOTPVERIFYrev" + LNA_OTP_Revision);

            Thread.Sleep(5);

            myWatch.Stop();
            double testtime = myWatch.ElapsedMilliseconds;


            return OTP_Burn_failure_Count;
        }



        public static int OTP_Burn_Mod_ID(int Site, string mfg_id_specified = "", string mod_id_specified = "")
        {
            if (true)
            //if (FailedTests.Count == 0)   // for Proto-3, turn-on during EVT
            {


                #region HSDIO Sendvector

                int ReadData_Mfg_ID = -999999;
                bool ReadData_Lock_Bit = false;
                int ReadData_Mod_ID = -999999;

                int MFG_ID = -999999;
                int ModuleID = 0;


                //Read_Unit_Number = 0;

                ReadData_Mfg_ID = OTP_Read_MFG_ID(Site);

                ReadData_Lock_Bit = OTP_Read_Lock_Bit(Site);

                ReadData_Mod_ID = OTP_Read_Mod_ID(Site);


                if (ReadData_Mfg_ID != 0 || ReadData_Mod_ID != 0 || ReadData_Lock_Bit != false) // if already burned
                {
                    return -999999; // return if any OTP already burned
                }



                if ((mfg_id_specified != "") || (mod_id_specified != ""))  // If higher level code specifies these optional parameters then don't use the sever to get MFG_ID and MOD_ID
                {

                    MFG_ID = Int32.Parse(mfg_id_specified);
                    ModuleID = Int32.Parse(mod_id_specified);
                }
                else
                {
                    /********************************************************* Not for use in Penang Production MFT and MOD ID will be done at wafer level or specifed as above ***********************************************************************************************************************************/
                    /* New serial id query that returns _otp_unit_id */

                    // Two approach of reading mfg_id
                    // (a) If already burnt in EEPROM, read from there by h/w API. 
                    // (b) Not available in EEPROM, need scan into Clotho MainUI and read from Cross Domain Cache. 
                    // For our cases, follow Option (b) 

                    // Read mfg_id from Cross Domain Cache 
                    // NOTE at this moment, MFG_ID is string for Clotho; Need parse to int so Clotho can write into result file
                    // In the future Clotho will upgrade to directly handle MFG_ID as int

                    string err = "";

                    // get and convert MFG_ID from Clotho to an int
                    if (mfg_id_str != "1")  // "1" means Mod_ID was not specified in Clotho and we set static mfg_id_str to default "0"
                       // mfg_id_str = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_MFG_ID, "");  hosein 09222020
                        mfg_id_str = OtpTest.mfg_ID;

                    //int MFG_ID = -999999;
                    try
                    {
                        MFG_ID = Int32.Parse(mfg_id_str);

                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Invalid MFG_ID was entered. 1 will be used as default", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mfg_id_str = "1"; // will retain this value for future parts so message doesnt keep poping up
                        MFG_ID = 1;
                    }


                    if (MFG_ID > 65535)
                    {
                        MessageBox.Show("Requested MFG_ID: " + MFG_ID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return -999999;
                    }

                    // Now that valid mfg_id is ready, get Module ID
                    try
                    {
                        Tuple<bool, int, string> unique_id_ret = SerialProvider.GetNextModuleID(MFG_ID);

                        if (!unique_id_ret.Item1)
                        {
                            err = unique_id_ret.Item3;
                            MessageBox.Show("Module ID Server is not responding. Fix or Disable Module ID burn in TCF. \n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return -999999;
                        }
                        else
                        {
                            ModuleID = unique_id_ret.Item2;
                        }
                    }

                    catch
                    { // ID Server may be down
                        MessageBox.Show("Exit Test Plan Run, Module ID Server is not responding. Disable Module ID burn in TCF\n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }

                    /*****************************************************************************************************************************************************************************************************/
                }






                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);


                /* Program Module (serial) ID */
                if (ModuleID > 16382)
                {
                    MessageBox.Show("Issued Module ID: " + Convert.ToString(ModuleID) + " is larger than OTP register capacity", "Exit Test Plan Run, Module_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }



                int NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("MOD_ID_NUM_BITS"));
                string[] bitVectors = new string[NumOfBits];

                for (int bit = 0; bit < NumOfBits; bit++)
                {
                    bitVectors[bit] = Eq.Site[Site].HSDIO.Get_Digital_Definition("MOD_ID_BIT" + Convert.ToString(bit));
                }

                char[] mod_id_char = (Convert.ToString(Convert.ToInt32(ModuleID), 2).PadLeft(NumOfBits, '0')).ToCharArray();  //convert to Binary string 
                System.Array.Reverse(mod_id_char);

                int vectorIndex = 0;
                foreach (char Value in mod_id_char)
                {
                    if (Value == '1')
                    {
                        string temp = bitVectors[vectorIndex];
                        Eq.Site[Site].HSDIO.SendVector(bitVectors[vectorIndex]);
                    }
                    vectorIndex++;
                }


                /* Program MFG ID */

                if (MFG_ID > 65535)
                {
                    MessageBox.Show("Requested MFG_ID: " + MFG_ID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -999999;
                }

                bitVectors = null;
                NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_NUM_BITS"));
                bitVectors = new string[NumOfBits];

                for (int bit = 0; bit < NumOfBits; bit++)
                {
                    bitVectors[bit] = Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_BIT" + Convert.ToString(bit));
                }

                char[] mfg_id_char = (Convert.ToString(Convert.ToInt32(MFG_ID), 2).PadLeft(16, '0')).ToCharArray();  //convert to Binary string 
                System.Array.Reverse(mfg_id_char);

                vectorIndex = 0;
                foreach (char Value in mfg_id_char)
                {
                    if (Value == '1')
                    {
                        Eq.Site[Site].HSDIO.SendVector(bitVectors[vectorIndex]);
                    }
                    vectorIndex++;
                }



                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Thread.Sleep(5);

                Eq.Site[Site].HSDIO.SendVector("VIOON");
                Thread.Sleep(5);


                //verify burn successful 
                if (ModuleID != OTP_Read_Mod_ID(Site) || MFG_ID != OTP_Read_MFG_ID(Site))
                {
                    return 1;
                }
                else
                    return 0;


            }

            #endregion

        }

        // product specific
        public static int OTP_Burn_Mod_2DID(int Site, out long Pre2DIDOtpReadBack)
        {
            bool ReadData_Lock_Bit = false;

            Pre2DIDOtpReadBack = OTP_Read_Mod_2DID(Site);
            ReadData_Lock_Bit = OTP_Read_Lock_Bit(Site);

            string Camera2DIDstr = "";
            long Camera_2DID = -999;
            int CM_ID = 0;

            try
            {
                string Key2DID = "2DID_OTPID";
                
                if (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt"))
                {
                   Key2DID = string.Format("2DID_OTPID_{0}", Site + 1);
                }

                Camera2DIDstr = ATFCrossDomainWrapper.GetStringFromCache(Key2DID, "00000000000000000000");

                ////For debug purpose
                //Camera2DIDstr = "'224200229995011007080505";
                ////MessageBox.Show("GetCamera2DID():  " + Camera2DIDstr);

                ///To support additional " ' " added at the front of the 2DID passed from Handler Plugin, so it can be viewed on excel
                ///this part should be obsolete
                if (Camera2DIDstr.Substring(0, 1) == "'")
                {
                    Camera2DIDstr = Camera2DIDstr.Remove(0, 1);
                }

                if (Camera2DIDstr.Length > 12)
                {
                    int.TryParse(Camera2DIDstr.Substring(7, 1), out CM_ID);

                    int start_index = Camera2DIDstr.Length - 11;
                    long.TryParse(Camera2DIDstr.Substring(start_index, 11), out Camera_2DID);
                }
            }
            catch (Exception e)
            {
                ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, "OTP 2DID: Error while retrieving 2DID" + Environment.NewLine + e.Message);
            }
            finally
            {
                ATFCrossDomainWrapper.StoreStringToCache("2DID_OTPID", "NA");
            }

            #region Burn 2DID OTP

            List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands = new List<MipiSyntaxParser.ClsMIPIFrame>();
            string hex_addr = "";
            string hex_data = "00";

            if (CM_ID != 0)
            {
                hex_addr = "3"; // RX Register
                hex_data = ((CM_ID << 4) & 0x30).ToString("X2");

                Eq.Site[Site].HSDIO.selectorMipiPair(2, (Byte)Site);

                MipiCommands.Clear();
                MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_addr, hex_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                if (OTP_Read_CM_ID(Site) == 0)
                {
                    PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);

                    PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
                }
            }

            if (Camera_2DID != 0 && Camera_2DID != -999)
            {
                if (Camera_2DID == previousCamera2DID[Site])
                {
                    return 5;
                }
                previousCamera2DID[Site] = Camera_2DID;

                
                if ((!IsMod2DIDRxEmpty(Site)  && !IsMod2DIDTxEmpty(Site))) // || ReadData_Lock_Bit != false) // if already burned
                {
                    if (Pre2DIDOtpReadBack == Camera_2DID) { return 0; }
                    else { return 7; }
                }

                //Burn PCBLotID, PCBPanel, PCBStrip, PCBModuleID values to registers
                string Camera_2DIDstr = Camera_2DID.ToString().PadLeft(12, '0');
                int NumOfBits = 0;

                if (IsMod2DIDTxEmpty(Site))
                {
                    // TX OTP
                    Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

                    //PCBPANEL LSB And PCBLOTID MSB#
                    NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_NUM_BITS"));
                    hex_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_ID_LSB_EFUSE"); // TX Register
                    hex_data = ((Convert.ToInt32(Camera_2DIDstr.Substring(4, 2)) & 0x3F)|
                        (Convert.ToInt32(Camera_2DIDstr.Substring(0, 4)) & 0x300)>>2).ToString("X2");

                    MipiCommands.Clear();
                    MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_addr, hex_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    //PCBPANEL MSB
                    hex_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_ID_MSB_EFUSE"); // TX Register
                    hex_data = ((Convert.ToInt32(Camera_2DIDstr.Substring(4, 2)) & 0x40)<<1).ToString("X2");

                    MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_addr, hex_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    //PCBLotID LSB
                    NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_NUM_BITS"));
                    hex_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_LSB_EFUSE"); //TX Register
                    hex_data = (Convert.ToInt32(Camera_2DIDstr.Substring(0, 4)) & 0xFF).ToString("X2");

                    MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_addr, hex_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                    {
                        //Checking of the OTP registers is now done so shift away for Tx OTP
                        if ((temp.Pair == 1) && (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER").ToUpper() == "COMMON"))
                        {
                            switch (temp.Register_hex)
                            {
                                case ("E0"): temp.Register_hex = "1"; break;
                                case ("E1"): temp.Register_hex = "2"; break;
                                case ("E2"): temp.Register_hex = "3"; break;
                                case ("E3"): temp.Register_hex = "4"; break;
                                case ("E4"): temp.Register_hex = "5"; break;
                                case ("E5"): temp.Register_hex = "6"; break;
                                case ("E6"): temp.Register_hex = "7"; break;
                                case ("E7"): temp.Register_hex = "8"; break;
                                case ("E8"): temp.Register_hex = "9"; break;
                                case ("E9"): temp.Register_hex = "A"; break;
                                case ("EA"): temp.Register_hex = "B"; break;
                                case ("EB"): temp.Register_hex = "C"; break;
                            }
                        }
                    }

                    PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex, MipiCommands[0].Register_hex);
                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
                    PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
                }

                if (IsMod2DIDRxEmpty(Site))
                {
                    // RX OTP
                    Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);

                    //PCBSTRIP# & PCBMODULEID MSB
                    NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_STRIP_NUM_BITS"));
                    string hex_lsb_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_STRIP_ID_EFUSE");
                    string hex_lsb_data = (((Convert.ToInt32(Camera_2DIDstr.Substring(6, 2)) & 0x0F) << 3) |
                        ((Convert.ToInt32(Camera_2DIDstr.Substring(8, 4)) & 0x700) >> 8)).ToString("X2");

                    MipiCommands.Clear();
                    MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_lsb_addr, hex_lsb_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
                    PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                    //PCB MODULEID LSB
                    NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_NUM_BITS"));
                    hex_lsb_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_LSB_EFUSE");
                    hex_lsb_data = (Convert.ToInt32(Camera_2DIDstr.Substring(8, 4)) & 0xFF).ToString("X2");

                    MipiCommands.Clear();
                    MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_lsb_addr, hex_lsb_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
                    PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
                }

                //verify burn successful 
                if (OTP_Read_Mod_2DID(Site) != Camera_2DID || 
                    OTP_Read_CM_ID(Site) != CM_ID)
                {
                    return 9;
                }
                else
                {
                    //// RX OTP
                    //Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);

                    //MipiCommands.Clear();
                    //// RX lock bit
                    //MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, "B", "80", Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    //PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
                    //Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
                    //PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                    //if (OTP_Read_Rx_Lockbit(Site) > 0)
                    //{
                    return 0;
                    //}
                    //else
                    //{
                    //    return 9;
                    //}
                }
            }
            else
            {
                return 3;
            }

            #endregion
        }

        public static int OTP_Burn_MFG_ID_TX(int Site, int MFGID_Dec)
        {
            int status = -1;
            if (MFGID_Dec > 65535)
            {
                return status;
            }

            string slave_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR");
            string mfg_id_lsb_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_LSB_READ_EFUSE");
            string mfg_id_msb_addr = Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_MSB_READ_EFUSE");
            string data_lsb = (MFGID_Dec & 0xFF).ToString("X2");
            string data_msb = ((MFGID_Dec & 0xFF00) >> 8).ToString("X2");

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands = new List<MipiSyntaxParser.ClsMIPIFrame>();
            MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(slave_addr, mfg_id_lsb_addr, data_lsb, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));
            MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(slave_addr, mfg_id_msb_addr, data_msb, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

            if (OTP_Read_MFG_ID(Site) == 0)
            {
                foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                {
                    //Checking of the OTP registers is now done so shift away for Tx OTP
                    if ((temp.Pair == 1) && (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER").ToUpper() == "COMMON"))
                    {
                        switch (temp.Register_hex)
                        {
                            case ("E0"): temp.Register_hex = "1"; break;
                            case ("E1"): temp.Register_hex = "2"; break;
                            case ("E2"): temp.Register_hex = "3"; break;
                            case ("E3"): temp.Register_hex = "4"; break;
                            case ("E4"): temp.Register_hex = "5"; break;
                            case ("E5"): temp.Register_hex = "6"; break;
                            case ("E6"): temp.Register_hex = "7"; break;
                            case ("E7"): temp.Register_hex = "8"; break;
                            case ("E8"): temp.Register_hex = "9"; break;
                            case ("E9"): temp.Register_hex = "A"; break;
                            case ("EA"): temp.Register_hex = "B"; break;
                            case ("EB"): temp.Register_hex = "C"; break;
                        }
                    }
                }

                PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex, MipiCommands[0].Register_hex);

                Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);

                PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                //verify burn successful  
                if (OTP_Read_MFG_ID(Site) != MFGID_Dec)
                    status = 1;
                else
                    status = 0;
            }
            else
            {
                status = -999999;
            }

            return status;
        }

        public static int OTP_Burn_MFG_ID_RX(int Site, int MFGID_Dec)
        {

            #region HSDIO Sendvector

            Eq.Site[Site].HSDIO.SendVector("VIOOFF");          
            Eq.Site[Site].HSDIO.SendVector("VIOON");

            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte) Site);

            Eq.Site[Site].DC["Vdd"].ForceVoltage(2.5, .1);

            if (OTP_Read_MFG_ID_RX(Site) != 0) // if already burned
            {
                return -999999; // return if any OTP already burned
            }

            Thread.Sleep(5);
            Eq.Site[Site].HSDIO.SendVectorOTP(MFGID_Dec.ToString("X"), "00", true);
            Thread.Sleep(5);


            Eq.Site[Site].DC["Vdd"].ForceVoltage(1.8, 0.1);


            Eq.Site[Site].HSDIO.SendVector("VIOOFF");
            Eq.Site[Site].HSDIO.SendVector("VIOON");

            //verify burn successful  
            if (OTP_Read_MFG_ID_RX(Site) != MFGID_Dec)
            {
                return 1;
            }
            else
                return 0;


            #endregion
        }

        public static int OTP_Burn_MGF_ID_LTN(int Site, List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands, string mfg_id_specified = "", string mod_id_specified = "")
        {
            if (true)
            //if (FailedTests.Count == 0)   // for Proto-3, turn-on during EVT
            {

                #region HSDIO Sendvector

                int MFG_ID = -999999;
                int ModuleID = 0;

                try
                {
                    //mod_id_specified = "0";
                    mod_id_specified = Convert.ToString(ATFCrossDomainWrapper.GetClothoCurrentSN());
                }
                catch
                {
                    mod_id_specified = "999";
                } //Debug Mode

                try
                {
                    //mod_id_specified = "0";
                    mfg_id_specified = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_ASSEMBLY_ID, "");
                    mfg_id_specified = mfg_id_specified.Substring(3, 6);
                }
                catch
                {
                    mfg_id_specified = "1";
                } //Debug Mode


                if ((mfg_id_specified != "") || (mod_id_specified != ""))  // If higher level code specifies these optional parameters then don't use the sever to get MFG_ID and MOD_ID
                {
                    MFG_ID = Int32.Parse(mfg_id_specified);
                    ModuleID = Int32.Parse(mod_id_specified);
                }
                else
                {
                    /********************************************************* Not for use in Penang Production MFT and MOD ID will be done at wafer level or specifed as above ***********************************************************************************************************************************/
                    /* New serial id query that returns _otp_unit_id */

                    // Two approach of reading mfg_id
                    // (a) If already burnt in EEPROM, read from there by h/w API. 
                    // (b) Not available in EEPROM, need scan into Clotho MainUI and read from Cross Domain Cache. 
                    // For our cases, follow Option (b) 

                    // Read mfg_id from Cross Domain Cache 
                    // NOTE at this moment, MFG_ID is string for Clotho; Need parse to int so Clotho can write into result file
                    // In the future Clotho will upgrade to directly handle MFG_ID as int

                    string err = "";

                    // get and convert MFG_ID from Clotho to an int
                    if (mfg_id_str != "1")  // "1" means Mod_ID was not specified in Clotho and we set static mfg_id_str to default "0"
                                            //      mfg_id_str = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_MFG_ID, "");

                        //int MFG_ID = -999999;
                        try
                        {
                            MFG_ID = Int32.Parse(mfg_id_str);

                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show("Invalid MFG_ID was entered. 1 will be used as default", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mfg_id_str = "1"; // will retain this value for future parts so message doesnt keep poping up
                            MFG_ID = 1;
                        }


                    if (MFG_ID > 65535)
                    {
                        MessageBox.Show("Requested MFG_ID: " + MFG_ID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return -999999;
                    }

                    // Now that valid mfg_id is ready, get Module ID
                    try
                    {
                        Tuple<bool, int, string> unique_id_ret = SerialProvider.GetNextModuleID(MFG_ID);

                        if (!unique_id_ret.Item1)
                        {
                            err = unique_id_ret.Item3;
                            MessageBox.Show("Module ID Server is not responding. Fix or Disable Module ID burn in TCF. \n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return -999999;
                        }
                        else
                        {
                            ModuleID = unique_id_ret.Item2;
                        }
                    }

                    catch
                    { // ID Server may be down
                        MessageBox.Show("Exit Test Plan Run, Module ID Server is not responding. Disable Module ID burn in TCF\n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }

                    /*****************************************************************************************************************************************************************************************************/
                }

                /* Program Module (serial) ID */
                if (ModuleID > 32767)
                {
                    MessageBox.Show("Issued Module ID: " + Convert.ToString(ModuleID) + " is larger than OTP register capacity", "Exit Test Plan Run, Module_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }


                /* Program MFG ID */

                if (MFG_ID > 65535)
                {
                    MessageBox.Show("Requested MFG_ID: " + MFG_ID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -999999;
                }

                foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                {
                    Eq.Site[Site].HSDIO.selectorMipiPair(temp.Pair, (byte) Site);
                }

                PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                if (EnableOTPburnTemplate)
                {
                    //re-set with Clotho PID * MFG
                    int MipiIndex = 0;
                    foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                    {
                        string Register_hex = Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_EFUSE_BYTE" + temp.Register_hex);
                        string AssignInfo = Eq.Site[Site].HSDIO.Get_Specific_Key(Register_hex, "READ")[0];
                        int Reg = 0;

                        switch (AssignInfo)
                        {
                            case "MFG_ID_MSB_READ": Reg = (MFG_ID & 0xff00) >> 8; break;
                            case "MFG_ID_LSB_READ": Reg = MFG_ID & 0xff; break;
                            case "MOD_ID_MSB_READ": Reg = (ModuleID & 0xff00) >> 8; break;
                            case "MOD_ID_LSB_READ": Reg = ModuleID & 0xff; break;
                            default:
                                MessageBox.Show("Resigtor " + temp.Register_hex + "is not for Mod_id and Mfg_ID", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }

                        if (OTP_Check_Bit2(Site, Register_hex, temp.Data_hex) != false) //if already burned
                        {
                            PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex); // It should turn-off with OTP Program off bit ex)Tx - TSMC_B10
                            return -999999; // return if any OTP already burned
                        }

                        MipiCommands[MipiIndex].Data_hex = Reg.ToString("X");

                        MipiIndex++;
                    }

                    Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
                }
                else
                {
                    List<string> All_bitVectors = new List<string>();

                    foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                    {
                        Eq.Site[Site].HSDIO.selectorMipiPair(temp.Pair, (byte) Site);

                        string _Register_Hex = "";

                        if (temp.Pair == 1)
                            _Register_Hex = "TX_EFUSE_BYTE" + temp.Register_hex;
                        else
                            _Register_Hex = "RX_EFUSE_BYTE" + temp.Register_hex;

                        _Register_Hex = Eq.Site[Site].HSDIO.Get_Digital_Definition(_Register_Hex);

                        if (OTP_Check_Bit2(Site, _Register_Hex, temp.Data_hex) != false) //if already burned
                        {
                            PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex); // It should turn-off with OTP Program off bit ex)Tx - TSMC_B10
                            return -999999; // return if any OTP already burned
                        }

                        All_bitVectors.AddRange(Vectorselector(Site, temp));
                    }
                    All_bitVectors.RemoveAll(s => s == null);

                    PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                    foreach (string strvector in All_bitVectors)
                    {
                        Eq.Site[Site].HSDIO.SendVector(strvector);
                    }
                }

                PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                {
                    string Dietype = temp.Pair == 1 ? "TX" : "RX";
                    string Register_hex2 = Eq.Site[Site].HSDIO.Get_Digital_Definition(Dietype + "_EFUSE_BYTE" + temp.Register_hex);
                    int readbackOTP = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Register_hex2), 16) & Convert.ToInt32(temp.Data_hex, 16);
                    if (readbackOTP != Convert.ToInt32(temp.Data_hex, 16)) return 0;
                }

                return 1;
            }

            #endregion

        }

        // only works for NDM 2DID OTP mapping
        private static void ProgramID(int Site, int NumOfBits, string ID, string VectorID)
        {
            string[] bitVectors = new string[NumOfBits];

            for (int bit = 0; bit < NumOfBits; bit++)
            {
                bitVectors[bit] = Eq.Site[Site].HSDIO.Get_Digital_Definition(VectorID + Convert.ToString(bit));
            }

            char[] prog_id_char = (Convert.ToString(Convert.ToInt32(ID), 2).PadLeft(NumOfBits, '0')).ToCharArray();  //convert to Binary string 
            System.Array.Reverse(prog_id_char);

            int vectorIndex = 0;
            foreach (char Value in prog_id_char)
            {
                if (Value == '1')
                {
                    string temp = bitVectors[vectorIndex];
                    Eq.Site[Site].HSDIO.SendVector(bitVectors[vectorIndex]);
                }
                vectorIndex++;
            }
        }

        public static int OTP_Burn_Mod_ID_SJ(int Site, List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands, string mfg_id_specified = "", string mod_id_specified = "")
        {

            if (true)
            //if (FailedTests.Count == 0)   // for Proto-3, turn-on during EVT
            {

                #region HSDIO Sendvector

                int MFG_ID = -999999;
                int ModuleID = 0;

                //try  //hosein 04212020
                //{
                //    mod_id_specified = Convert.ToString(ATFCrossDomainWrapper.GetClothoCurrentSN());
                //    mfg_id_specified = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_MFG_ID, "");
                //}
                //catch
                //{
                //    mod_id_specified = "999";
                //    mfg_id_specified = "1";
                //} //Debug Mode
                try
                {
                    //mod_id_specified = Convert.ToString(ATFCrossDomainWrapper.GetClothoCurrentSN());
                    mfg_id_specified = ATFCrossDomainWrapper.GetStringFromCache("MFG", "");  //hosein 09302020

                    //mfg_id_specified = mfgidspecified;
                    //mfg_id_specified = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
                    MFG_ID = Int32.Parse(mfg_id_specified.Substring(3));

                    SerialProvider.SetServerLocation("http://135.141.89.110:6987/SerialProvider");
                    Tuple<bool, int, string> unique_id_ret = SerialProvider.GetNextModuleID(MFG_ID);
                    mod_id_specified = unique_id_ret.Item2.ToString();
                }
                catch
                {
                    mod_id_specified = "999";
                    mfg_id_specified = "2";

                } //Debug Mode

                if ((mfg_id_specified != "") || (mod_id_specified != ""))  // If higher level code specifies these optional parameters then don't use the sever to get MFG_ID and MOD_ID
                {
                    MFG_ID = Int32.Parse(mfg_id_specified.Substring(3));
                    ModuleID = Int32.Parse(mod_id_specified);
                }
                else
                {
                    /********************************************************* Not for use in Penang Production MFT and MOD ID will be done at wafer level or specifed as above ***********************************************************************************************************************************/
                    /* New serial id query that returns _otp_unit_id */

                    // Two approach of reading mfg_id
                    // (a) If already burnt in EEPROM, read from there by h/w API. 
                    // (b) Not available in EEPROM, need scan into Clotho MainUI and read from Cross Domain Cache. 
                    // For our cases, follow Option (b) 

                    // Read mfg_id from Cross Domain Cache 
                    // NOTE at this moment, MFG_ID is string for Clotho; Need parse to int so Clotho can write into result file
                    // In the future Clotho will upgrade to directly handle MFG_ID as int

                    string err = "";

                    // get and convert MFG_ID from Clotho to an int
                    if (mfg_id_str != "1")  // "1" means Mod_ID was not specified in Clotho and we set static mfg_id_str to default "0"
                        //mfg_id_str = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_MFG_ID, "");  hosein 09222020
                        mfg_id_str = OtpTest.mfg_ID;

                    //int MFG_ID = -999999;
                    try
                    {
                        MFG_ID = Int32.Parse(mfg_id_str);

                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Invalid MFG_ID was entered. 1 will be used as default", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mfg_id_str = "1"; // will retain this value for future parts so message doesnt keep poping up
                        MFG_ID = 1;
                    }


                    if (MFG_ID > 65535)
                    {
                        MessageBox.Show("Requested MFG_ID: " + MFG_ID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return -999999;
                    }

                    // Now that valid mfg_id is ready, get Module ID
                    try
                    {
                        Tuple<bool, int, string> unique_id_ret = SerialProvider.GetNextModuleID(MFG_ID);

                        if (!unique_id_ret.Item1)
                        {
                            err = unique_id_ret.Item3;
                            MessageBox.Show("Module ID Server is not responding. Fix or Disable Module ID burn in TCF. \n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return -999999;
                        }
                        else
                        {
                            ModuleID = unique_id_ret.Item2;
                        }
                    }

                    catch
                    { // ID Server may be down
                        MessageBox.Show("Exit Test Plan Run, Module ID Server is not responding. Disable Module ID burn in TCF\n\n" + err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }

                    /*****************************************************************************************************************************************************************************************************/
                }

                /* Program Module (serial) ID */
                if (ModuleID > 32767)
                {
                    MessageBox.Show("Issued Module ID: " + Convert.ToString(ModuleID) + " is larger than OTP register capacity", "Exit Test Plan Run, Module_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }


                /* Program MFG ID */

                if (MFG_ID > 65535)
                {
                    MessageBox.Show("Requested MFG_ID: " + MFG_ID.ToString() + " is larger than OTP register capacity", "Exit Test Plan Run, MFT_ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -999999;
                }

                //MFG_ID = 2;
                //ModuleID = 75;

                //re-set with Clotho PID * MFG
                string origReg0Data = "";
                string origReg1Data = "";
                string origReg3Data = "";
                string origReg4Data = "";


                #region Changeover
                foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands) //this assumes tx mipi
                {
                    switch (temp.Register_hex.Trim())
                    {
                        case "0": //MFG_ID MSB
                            int Reg0 = (MFG_ID & 0xff00) >> 8;
                            origReg0Data = temp.Data_hex;
                            MipiCommands[0].Data_hex = Reg0.ToString("X");
                            MipiCommands[0].Register_hex = "1";
                            break;
                        case "1": //MFG_ID LSB
                            int Reg1 = MFG_ID & 0xff;
                            origReg1Data = temp.Data_hex;
                            MipiCommands[1].Data_hex = Reg1.ToString("X");
                            MipiCommands[1].Register_hex = "2";
                            break;
                        case "3": //MOD_ID MSB
                            int Reg3 = (ModuleID & 0xff00) >> 8;
                            origReg3Data = temp.Data_hex;
                            MipiCommands[2].Data_hex = Reg3.ToString("X");
                            MipiCommands[2].Register_hex = "4";
                            break;
                        case "4": //MOD_ID LSB
                            int Reg4 = ModuleID & 0xff;
                            origReg4Data = temp.Data_hex;
                            MipiCommands[3].Data_hex = Reg4.ToString("X");
                            MipiCommands[3].Register_hex = "5";
                            break;
                        default:
                            MessageBox.Show("Resigtor " + temp.Register_hex + " is not for Mod_id and Mfg_ID", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
                #endregion Changeover

                List<string> All_bitVectors = new List<string>();
                //string Projectspecific = "PINOT";
                int beancounter = 0;
                foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                {
                    if (temp.Pair == 1)
                    {
                        switch (temp.Register_hex)
                        {
                            case ("1"): temp.Register_hex = "0"; break;
                            case ("2"): temp.Register_hex = "1"; break;
                            case ("3"): temp.Register_hex = "2"; break;
                            case ("4"): temp.Register_hex = "3"; break;
                            case ("5"): temp.Register_hex = "4"; break;
                            case ("6"): temp.Register_hex = "5"; break;
                            case ("7"): temp.Register_hex = "6"; break;
                            case ("8"): temp.Register_hex = "7"; break;
                            case ("9"): temp.Register_hex = "8"; break;
                            case ("A"): temp.Register_hex = "9"; break;
                            case ("B"): temp.Register_hex = "A"; break;
                            case ("C"): temp.Register_hex = "B"; break;
                        }
                    }

                    Eq.Site[Site].HSDIO.selectorMipiPair(temp.Pair, (byte)Site);

                    string _Register_Hex = "";

                    if (temp.Pair == 1)
                        _Register_Hex = "TX_EFUSE_BYTE" + temp.Register_hex;
                    else
                        _Register_Hex = "RX_EFUSE_BYTE" + temp.Register_hex;

                    _Register_Hex = Eq.Site[Site].HSDIO.Get_Digital_Definition(_Register_Hex);

                    //if (temp.Pair == 1)
                    //{
                    //    switch (temp.Register_hex)
                    //    {
                    //        case ("1"): temp.Register_hex = "0"; break;
                    //        case ("2"): temp.Register_hex = "1"; break;
                    //        case ("3"): temp.Register_hex = "2"; break;
                    //        case ("4"): temp.Register_hex = "3"; break;
                    //        case ("5"): temp.Register_hex = "4"; break;
                    //        case ("6"): temp.Register_hex = "5"; break;
                    //        case ("7"): temp.Register_hex = "6"; break;
                    //        case ("8"): temp.Register_hex = "7"; break;
                    //        case ("9"): temp.Register_hex = "8"; break;
                    //        case ("A"): temp.Register_hex = "9"; break;
                    //        case ("B"): temp.Register_hex = "A"; break;
                    //        case ("C"): temp.Register_hex = "B"; break;
                    //    }
                    //}

                    if (OTP_Check_Bit2(Site, _Register_Hex, temp.Data_hex) != false) //if already burned
                    {
                        #region Changeover
                        //foreach (MipiSyntaxParser.ClsMIPIFrame x in MipiCommands)
                        //{
                        //    switch (x.Register_hex.Trim())
                        //    {
                        //        case "1": //MFG_ID MSB
                        //            int Reg0 = (MFG_ID & 0xff00) >> 8;
                        //            MipiCommands[0].Data_hex = Reg0.ToString("X");
                        //            MipiCommands[0].Register_hex = "0";
                        //            break;
                        //        case "2": //MFG_ID LSB
                        //            int Reg1 = MFG_ID & 0xff;
                        //            MipiCommands[1].Data_hex = Reg1.ToString("X");
                        //            MipiCommands[1].Register_hex = "1";
                        //            break;
                        //        case "4": //MOD_ID MSB
                        //            int Reg3 = (ModuleID & 0xff00) >> 8;
                        //            MipiCommands[2].Data_hex = Reg3.ToString("X");
                        //            MipiCommands[2].Register_hex = "3";
                        //            break;
                        //        case "5": //MOD_ID LSB
                        //            int Reg4 = ModuleID & 0xff;
                        //            MipiCommands[3].Data_hex = Reg4.ToString("X");
                        //            MipiCommands[3].Register_hex = "4";
                        //            break;
                        //        default:
                        //            MessageBox.Show("Resigtor " + x.Register_hex + " is not for Mod_id and Mfg_ID", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //            break;
                        //    }
                        //}
                        #endregion Changeover
                        if (temp.Pair == 1)
                        {
                            switch (temp.Register_hex)
                            {
                                case ("0"): temp.Register_hex = "1"; break;
                                case ("1"): temp.Register_hex = "2"; break;
                                case ("2"): temp.Register_hex = "3"; break;
                                case ("3"): temp.Register_hex = "4"; break;
                                case ("4"): temp.Register_hex = "5"; break;
                                case ("5"): temp.Register_hex = "6"; break;
                                case ("6"): temp.Register_hex = "7"; break;
                                case ("7"): temp.Register_hex = "8"; break;
                                case ("8"): temp.Register_hex = "9"; break;
                                case ("9"): temp.Register_hex = "A"; break;
                                case ("A"): temp.Register_hex = "B"; break;
                                case ("B"): temp.Register_hex = "C"; break;
                            }
                        }
                        beancounter++;
                        continue;


                        //return -999999; // return if any OTP already burned
                    }

                    if (temp.Pair == 1)
                    {
                        switch (temp.Register_hex)
                        {
                            case ("0"): temp.Register_hex = "1"; break;
                            case ("1"): temp.Register_hex = "2"; break;
                            case ("2"): temp.Register_hex = "3"; break;
                            case ("3"): temp.Register_hex = "4"; break;
                            case ("4"): temp.Register_hex = "5"; break;
                            case ("5"): temp.Register_hex = "6"; break;
                            case ("6"): temp.Register_hex = "7"; break;
                            case ("7"): temp.Register_hex = "8"; break;
                            case ("8"): temp.Register_hex = "9"; break;
                            case ("9"): temp.Register_hex = "A"; break;
                            case ("A"): temp.Register_hex = "B"; break;
                            case ("B"): temp.Register_hex = "C"; break;
                        }
                    }

                    All_bitVectors.AddRange(Vectorselector(Site, temp));
                }
                All_bitVectors.RemoveAll(s => s == null);

                int roduction = 0;
                if (beancounter == 0) //means no bits burned yet and burn can proceed
                {
                    PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);

                    if (EnableOTPburnTemplate)
                    {
                        Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
                    }
                    else
                    {
                        foreach (string strvector in All_bitVectors) //This happens if you have static vectors for each bit that needs burn
                        {
                            Eq.Site[Site].HSDIO.SendVector(strvector);
                        }
                    }

                    PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);


                    foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                    {
                        if (temp.Pair == 1)
                        {
                            switch (temp.Register_hex)
                            {
                                case ("1"): temp.Register_hex = "0"; break;
                                case ("2"): temp.Register_hex = "1"; break;
                                case ("3"): temp.Register_hex = "2"; break;
                                case ("4"): temp.Register_hex = "3"; break;
                                case ("5"): temp.Register_hex = "4"; break;
                                case ("6"): temp.Register_hex = "5"; break;
                                case ("7"): temp.Register_hex = "6"; break;
                                case ("8"): temp.Register_hex = "7"; break;
                                case ("9"): temp.Register_hex = "8"; break;
                                case ("A"): temp.Register_hex = "9"; break;
                                case ("B"): temp.Register_hex = "A"; break;
                                case ("C"): temp.Register_hex = "B"; break;
                            }
                        }
                        string Dietype = temp.Pair == 1 ? "TX" : "RX";
                        string Register_hex2 = Eq.Site[Site].HSDIO.Get_Digital_Definition(Dietype + "_EFUSE_BYTE" + temp.Register_hex);
                        int readbackOTP = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Register_hex2), 16) & Convert.ToInt32(temp.Data_hex, 16);
                        if (readbackOTP != Convert.ToInt32(temp.Data_hex, 16))
                        {
                            #region Changeover
                            //foreach (MipiSyntaxParser.ClsMIPIFrame x in MipiCommands)
                            //{
                            //    switch (x.Register_hex.Trim())
                            //    {
                            //        case "1": //MFG_ID MSB
                            //            int Reg0 = (MFG_ID & 0xff00) >> 8;
                            //            MipiCommands[0].Data_hex = Reg0.ToString("X");
                            //            MipiCommands[0].Register_hex = "0";
                            //            break;
                            //        case "2": //MFG_ID LSB
                            //            int Reg1 = MFG_ID & 0xff;
                            //            MipiCommands[1].Data_hex = Reg1.ToString("X");
                            //            MipiCommands[1].Register_hex = "1";
                            //            break;
                            //        case "4": //MOD_ID MSB
                            //            int Reg3 = (ModuleID & 0xff00) >> 8;
                            //            MipiCommands[2].Data_hex = Reg3.ToString("X");
                            //            MipiCommands[2].Register_hex = "3";
                            //            break;
                            //        case "5": //MOD_ID LSB
                            //            int Reg4 = ModuleID & 0xff;
                            //            MipiCommands[3].Data_hex = Reg4.ToString("X");
                            //            MipiCommands[3].Register_hex = "4";
                            //            break;
                            //        default:
                            //            MessageBox.Show("Resigtor " + x.Register_hex + " is not for Mod_id and Mfg_ID", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //            break;
                            //    }
                            //}
                            #endregion Changeover
                            roduction++;
                            continue;
                        }
                    }

                    #region Changeover
                    foreach (MipiSyntaxParser.ClsMIPIFrame x in MipiCommands)
                    {
                        //if (x.Pair == 1)
                        //{
                        //    switch (x.Register_hex)
                        //    {
                        //        case ("1"): x.Register_hex = "0"; break;
                        //        case ("2"): x.Register_hex = "1"; break;
                        //        case ("3"): x.Register_hex = "2"; break;
                        //        case ("4"): x.Register_hex = "3"; break;
                        //        case ("5"): x.Register_hex = "4"; break;
                        //        case ("6"): x.Register_hex = "5"; break;
                        //        case ("7"): x.Register_hex = "6"; break;
                        //        case ("8"): x.Register_hex = "7"; break;
                        //        case ("9"): x.Register_hex = "8"; break;
                        //        case ("A"): x.Register_hex = "9"; break;
                        //        case ("B"): x.Register_hex = "A"; break;
                        //        case ("C"): x.Register_hex = "B"; break;
                        //    }
                        //}
                        //switch (x.Register_hex.Trim())
                        //{
                        //    case "1": //MFG_ID MSB
                        //        int Reg0 = (MFG_ID & 0xff00) >> 8;
                        //        MipiCommands[0].Data_hex = Reg0.ToString("X");
                        //        MipiCommands[0].Register_hex = "0";
                        //        break;
                        //    case "2": //MFG_ID LSB
                        //        int Reg1 = MFG_ID & 0xff;
                        //        MipiCommands[1].Data_hex = Reg1.ToString("X");
                        //        MipiCommands[1].Register_hex = "1";
                        //        break;
                        //    case "4": //MOD_ID MSB
                        //        int Reg3 = (ModuleID & 0xff00) >> 8;
                        //        MipiCommands[2].Data_hex = Reg3.ToString("X");
                        //        MipiCommands[2].Register_hex = "3";
                        //        break;
                        //    case "5": //MOD_ID LSB
                        //        int Reg4 = ModuleID & 0xff;
                        //        MipiCommands[3].Data_hex = Reg4.ToString("X");
                        //        MipiCommands[3].Register_hex = "4";
                        //        break;
                        //    default:
                        //        MessageBox.Show("Resigtor " + x.Register_hex + " is not for Mod_id and Mfg_ID", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //        break;
                        //}
                    }
                    #endregion Changeover

                }



                if (beancounter != 0)
                {
                    #region Changeover
                    foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
                    {
                        switch (temp.Register_hex.Trim())
                        {
                            case "1": //MFG_ID MSB                            
                                MipiCommands[0].Data_hex = origReg0Data;
                                MipiCommands[0].Register_hex = "0";
                                break;
                            case "2": //MFG_ID LSB                            
                                MipiCommands[1].Data_hex = origReg1Data;
                                MipiCommands[1].Register_hex = "1";
                                break;
                            case "4": //MOD_ID MSB                            
                                MipiCommands[2].Data_hex = origReg3Data;
                                MipiCommands[2].Register_hex = "3";
                                break;
                            case "5": //MOD_ID LSB                            
                                MipiCommands[3].Data_hex = origReg4Data;
                                MipiCommands[3].Register_hex = "4";
                                break;
                            default:
                                //MessageBox.Show("Resigtor " + temp.Register_hex + " is not for Mod_id and Mfg_ID", "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }
                    }
                    #endregion Changeover
                    return -99999; //some bits have already been burned
                }
                else if (roduction != 0) return 1; //a readback after OTP did not return the right value
                return 0;  //all went well
            }

            #endregion

        }

        public static int OTP_Burn_Rev_ID(int Site, string rev_id_specified)
        {
            if (true)
            {


                #region HSDIO Sendvector

                int ReadData = -999999;
                if (rev_id_specified == "")
                {
                    return -999999; // return if no rev specified
                }
                try
                {
                    ReadData = OTP_Read_Rev_ID(Site);
                }
                catch (Exception e)
                {

                    ReadData = 888888;
                }


                if (ReadData != 0) // if already burned
                {
                    if (ReadData == 888888) return 888888;
                    return -999999; // return if any OTP already burned
                }



                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);



                /* Program Rev ID */

                int NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("REV_ID_NUM_BITS"));
                string[] bitVectors = new string[NumOfBits];

                for (int bit = 0; bit < NumOfBits; bit++)
                {
                    bitVectors[bit] = Eq.Site[Site].HSDIO.Get_Digital_Definition("REV_ID_BIT" + Convert.ToString(bit));
                }

                char[] rev_id_char = (Convert.ToString(Convert.ToInt32(rev_id_specified), 2).PadLeft(NumOfBits, '0')).ToCharArray();  //convert to Binary string 
                System.Array.Reverse(rev_id_char);

                int vectorIndex = 0;
                foreach (char Value in rev_id_char)
                {
                    if (Value == '1')
                    {
                        string temp = bitVectors[vectorIndex];
                        Eq.Site[Site].HSDIO.SendVector(bitVectors[vectorIndex]);
                    }
                    vectorIndex++;
                }



                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                ///reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");


                //verify burn successful  and otp lock
                if (Convert.ToInt32(rev_id_specified) != OTP_Read_Rev_ID(Site))
                {
                    return 1;
                }
                else
                    return 0;
            }

            #endregion

        }

        public static int OTP_Burn_Lock_Bit(int Site)
        {
            if (true)   // do not OTP during Init Test
            {


                #region HSDIO Sendvector


                if (OTP_Read_Lock_Bit(Site) == true) // if already burned
                {
                    return -999999; // return if any OTP already burned
                }

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);


                //Burn OTP lock bit lock 
                Eq.Site[Site].HSDIO.SendVector(Eq.Site[Site].HSDIO.Get_Digital_Definition("LOCK_BIT_BURN"));

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");


                //verify burn successful  
                if (OTP_Read_Lock_Bit(Site) != true)
                {
                    return 1;
                }
                else
                    return 0;
            }

            #endregion

        }

        public static int OTP_Burn_Lock_Bit(int Site, bool forceburn)
        {
            if (true)   // do not OTP during Init Test
            {


                #region HSDIO Sendvector

                if (!forceburn)
                {
                    if (OTP_Read_Lock_Bit(Site) == true) // if already burned
                    {
                        return -999999; // return if any OTP already burned
                    }
                }
                

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);


                //Burn OTP lock bit lock 
                //Eq.Site[Site].HSDIO.SendVector(Eq.Site[Site].HSDIO.Get_Digital_Definition("LOCK_BIT_BURN"));
                Eq.Site[Site].HSDIO.RegWrite("EB","01");

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");


                //verify burn successful  
                if (OTP_Read_Lock_Bit(Site) != true)
                {
                    return 1;
                }
                else
                    return 0;
            }

            #endregion

        }

        public static int OTP_Burn_FBAR_Noise_Pass_Flag(int Site)
        {
            if (true)   // do not OTP during Init Test
            {


                #region HSDIO Sendvector

                if (OTP_Read_FBAR_Noise_Pass_Flag(Site) == true) // if already burned
                {
                    return -999999; // return if any OTP already burned
                }

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);


                //Burn FBAR PAss flag
                Eq.Site[Site].HSDIO.SendVector(Eq.Site[Site].HSDIO.Get_Digital_Definition("FBAR_FLAG_BURN"));

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");


                //verify burn successful 
                if (OTP_Read_FBAR_Noise_Pass_Flag(Site) != true)
                {
                    return 1;
                }
                else
                    return 0;
            }

            #endregion

        }

        public static int OTP_Burn_RF1_Pass_Flag(int Site)
        {
            if (true)   // do not OTP during Init Test
            {


                #region HSDIO Sendvector


                if (OTP_Read_RF1_Pass_Flag(Site) != false) // if already burned
                {
                    return -999999; // return if any OTP already burned
                }

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);


                //Burn RF1 Pass flag
                //Eq.Site[Site].HSDIO.SendVector(Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_FLAG_BURN"));

                #region MIPI OTP

                string bit_location = Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_FLAG_BURN");
                string[] bit_info = bit_location.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                if (bit_info.Length > 2)
                {
                    int register = int.Parse(bit_info[1].ToUpper().Replace("E", ""));
                    int bit_number = int.Parse(bit_info[2].ToUpper().Replace("B", ""));
                    if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                    {
                        Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                    }
                    Eq.Site[Site].HSDIO.Burn((1 << bit_number).ToString("X"), false, register);
                    Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                    Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                    Eq.Site[Site].HSDIO.SendVector("VIOON");
                    if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                    {
                        Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                    }
                    Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                    Thread.Sleep(10);
                    Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);
                }
                #endregion

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");


                //verify burn successful  
                if (OTP_Read_RF1_Pass_Flag(Site) != true)
                {
                    return 1;
                }
                else
                    return 0;
            }

            #endregion
        }

        public static int OTP_Burn_RF2_Pass_Flag(int Site)
        {
            if (true)   // do not OTP during Init Test
            {


                #region HSDIO Sendvector


                if (OTP_Read_RF2_Pass_Flag(Site) != false) //if already burned
                {
                    return -999999; // return if any OTP already burned
                }

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, .1);


                //Burn FBAR PAss flag
                Eq.Site[Site].HSDIO.SendVector(Eq.Site[Site].HSDIO.Get_Digital_Definition("RF2_FLAG_BURN"));

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");

                //verify burn successful  
                if (OTP_Read_RF2_Pass_Flag(Site) != true)
                {
                    return 1;
                }
                else
                    return 0;
            }

            #endregion
        }

        //#DPAT - To update when RF1 outlier flag location has been identified
        public static int OTP_Burn_RF1_Outlier_Flag(int Site)
        {
            if (true)   // do not OTP during Init Test
            {
                #region HSDIO Sendvector

                if (OTP_Read_RF1_Outlier_Bit(Site) != false) // if already burned
                {
                    return -999999; // return if any OTP already burned
                }

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.1);

                #region MIPI OTP

                string bit_location = Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_OUTLIER_BURN");
                string[] bit_info = bit_location.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                if (bit_info.Length > 2)
                {
                    int register = int.Parse(bit_info[1].ToUpper().Replace("E", ""));
                    int bit_number = int.Parse(bit_info[2].ToUpper().Replace("B", ""));
                    if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                    {
                        Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                    }
                    Eq.Site[Site].HSDIO.Burn((1 << bit_number).ToString("X"), false, register);
                    Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                    Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                    Eq.Site[Site].HSDIO.SendVector("VIOON");
                    if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                    {
                        Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                    }
                    Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                    Thread.Sleep(10);
                    Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.2);
                }
                #endregion

                Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);

                // reset VIO and verify burn
                Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                Eq.Site[Site].HSDIO.SendVector("VIOON");



                //verify burn successful  
                if (OTP_Read_RF1_Outlier_Bit(Site) != true)
                {
                    return 1;
                }
                else
                    return 0;

                #endregion
            }
        }

        public static int OTP_Burn_Custom(byte Site, int Pair, string SlaveAddress_hex,  string Register_hex, string Data_hex)
        {
            List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands = new List<MipiSyntaxParser.ClsMIPIFrame>();
            MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(SlaveAddress_hex, Register_hex, Data_hex, Pair));
            return OTP_Read_Bytes(Site, MipiCommands);
        }
            
        public static int OTP_Burn_Custom(int Site, List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)   //20190709 new OtpTest
        {
            //if (ResultBuilder.FailedTests[Site].Count != 0)   //  comment out init smaple only 
            //{
            //    return -100;
            //}
                       

            //string Projectspecific = Eq.Site[Site].HSDIO.Get_Digital_Definition("PROJECT");
            List<string> All_bitVectors = new List<string>();
            
            #region Mipi burn check 

            foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
            {
                Eq.Site[Site].HSDIO.selectorMipiPair(temp.Pair, (byte)Site);

                //Eq.Site[0].HSDIO.RegWrite("2B", "0F");        

                string _Register_Hex = "";

                if (temp.Pair == 1)
                    _Register_Hex = "TX_EFUSE_BYTE" + temp.Register_hex;
                else
                {
                    _Register_Hex = "RX_EFUSE_BYTE" + temp.Register_hex;
                    if (Convert.ToInt32(temp.Register_hex, 16) > 7) isRx2ndMemory[Site] = true;
                    else isRx2ndMemory[Site] = false;
                }
                _Register_Hex = Eq.Site[Site].HSDIO.Get_Digital_Definition(_Register_Hex);

                if (!_Register_Hex.Contains("NONE"))
                {
                    if (OTP_Check_Data(Site, temp) != false) //if already burned
                    {
                        return -999999; // return if any OTP already burned
                    }
                }
                //Checking of the OTP registers is now done so shift away for Tx OTP
                if ((temp.Pair == 1) && (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER").ToUpper() == "COMMON"))
                {
                    switch (temp.Register_hex)
                    {
                        case ("0"): temp.Register_hex = "1"; break;
                        case ("1"): temp.Register_hex = "2"; break;
                        case ("2"): temp.Register_hex = "3"; break;
                        case ("3"): temp.Register_hex = "4"; break;
                        case ("4"): temp.Register_hex = "5"; break;
                        case ("5"): temp.Register_hex = "6"; break;
                        case ("6"): temp.Register_hex = "7"; break;
                        case ("7"): temp.Register_hex = "8"; break;
                        case ("8"): temp.Register_hex = "9"; break;
                        case ("9"): temp.Register_hex = "A"; break;
                        case ("A"): temp.Register_hex = "B"; break;
                        case ("B"): temp.Register_hex = "C"; break;
                    }
                }

                All_bitVectors.AddRange(Vectorselector(Site, temp));
            }
            All_bitVectors.RemoveAll(s => s == null);

            #endregion

            PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex, MipiCommands[0].Register_hex);

            if (EnableOTPburnTemplate)
            {
                Eq.Site[Site].HSDIO.SendMipiCommands(MipiCommands, eMipiTestType.OTPburn);
            }
            else
            {
                foreach (string strvector in All_bitVectors)
                {
                    Eq.Site[Site].HSDIO.SendVector(strvector);
                }
            }

            PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex);
            
            foreach (MipiSyntaxParser.ClsMIPIFrame temp in MipiCommands)
            {
                if ((temp.Pair == 1) && (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER").ToUpper() == "COMMON"))
                {
                    switch (temp.Register_hex)
                    {
                        case ("1"): temp.Register_hex = "0"; break;
                        case ("2"): temp.Register_hex = "1"; break;
                        case ("3"): temp.Register_hex = "2"; break;
                        case ("4"): temp.Register_hex = "3"; break;
                        case ("5"): temp.Register_hex = "4"; break;
                        case ("6"): temp.Register_hex = "5"; break;
                        case ("7"): temp.Register_hex = "6"; break;
                        case ("8"): temp.Register_hex = "7"; break;
                        case ("9"): temp.Register_hex = "8"; break;
                        case ("A"): temp.Register_hex = "9"; break;
                        case ("B"): temp.Register_hex = "A"; break;
                        case ("C"): temp.Register_hex = "B"; break;
                    }
                }

                string Dietype = temp.Pair == 1 ? "TX" : "RX";
                string Register_hex2 = Eq.Site[Site].HSDIO.Get_Digital_Definition(Dietype + "_EFUSE_BYTE" + temp.Register_hex);
                if (Register_hex2 == "NONE") continue;
                int readbackOTP = OTP_Procedure.OTP_Read_Bytes((byte)Site, new List<MipiSyntaxParser.ClsMIPIFrame> { temp }) & Convert.ToInt32(temp.Data_hex, 16);
                if (readbackOTP != Convert.ToInt32(temp.Data_hex, 16)) return 1;                    
            }          
            
            return 0;
        }     
        
        public static int OTP_Read_Bytes( byte Site, int Pair, string SlaveAddress_hex, string Register_hex, string mask_Data_hex = "FF")
        {
            List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands = new List<MipiSyntaxParser.ClsMIPIFrame>();
            MipiCommands.Add(new MipiSyntaxParser.ClsMIPIFrame(SlaveAddress_hex, Register_hex, mask_Data_hex, Pair));
            return OTP_Read_Bytes(Site, MipiCommands);
        }

        public static int OTP_Read_Bytes(byte Site, List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
        {
            int calculatedResult = 0;
            int[] Arryreadcustom = new int[MipiCommands.Count];
            int count = 0;
            string _Register_Hex = "";

            //List<MipiSyntaxParser.ClsMIPIFrame> clsMIPIFrames = new List<MipiSyntaxParser.ClsMIPIFrame>();

            foreach (MipiSyntaxParser.ClsMIPIFrame _temp in MipiCommands)
            {
                Eq.Site[Site].HSDIO.selectorMipiPair(_temp.Pair, Site);

                if (_temp.Pair == 1)
                {
                    _Register_Hex = "TX_EFUSE_BYTE" + _temp.Register_hex;
                    //Eq.Site[0].HSDIO.RegWrite("2B", "0F");                   
                }
                else
                    _Register_Hex = "RX_EFUSE_BYTE" + _temp.Register_hex;

                _Register_Hex = Eq.Site[Site].HSDIO.Get_Digital_Definition(_Register_Hex);

                PreOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex, _temp.Register_hex, eMipiTestType.OTPread, _temp);

                string strReadAddress = AdditionalOTPSeqRequired[Site] ? Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_ADDR_RETRIEVE_CMOS65NM") : _Register_Hex;
                string strReadHex = Eq.Site[Site].HSDIO.RegRead(strReadAddress);

                Arryreadcustom[count] = Convert.ToInt32(strReadHex, 16) & Convert.ToInt32(_temp.Data_hex, 16);

                count++;

                PostOTPstage(Site, Eq.Site[Site].HSDIO.dutSlavePairIndex, eMipiTestType.OTPread);
            }

            count = 0;

            foreach (int tempResult in Arryreadcustom)
            {
                calculatedResult += (int)(tempResult * Math.Pow(256, count));
                count++;
            }

            return calculatedResult;
        }

        public static bool OTP_Check_Bit(int site, List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
        {
            int calculatedResult = -999999;
            bool isburn = false;
            int countData = 0;

            int intData = Int32.Parse(MipiCommands[0].Data_hex, System.Globalization.NumberStyles.HexNumber);
            char[] Data_char = (Convert.ToString(intData, 2).PadLeft(8, '0')).ToCharArray();  //convert to Binary string                        

            foreach (char Value in Data_char)
            {
                if (Value == '1')   countData++;
            }

            if (MipiCommands.Count>1 || countData != 1)
            {
                MessageBox.Show("More than one bit check is not supported", "OTP Check Bit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }            

            calculatedResult = OTP_Read_Bytes((byte)site, MipiCommands) & Convert.ToInt32(MipiCommands[0].Data_hex, 16);
            isburn = calculatedResult > 0 ? true : false;

            return isburn; 
        }

        private static bool OTP_Check_Bit2(int site, string Register_hex, string Data_hex) //20190709 new OtpTest
        {
            int calculatedResult = -999999;
            bool isburn = false;

            if((Register_hex=="E7" & Data_hex == "80")|| Eq.Site[site].HSDIO.dutSlavePairIndex == 1)
                calculatedResult = Convert.ToInt32(Eq.Site[site].HSDIO.RegRead(Register_hex), 16) & Convert.ToInt32(Data_hex, 16); 
            else 
                calculatedResult = Convert.ToInt32(Eq.Site[site].HSDIO.RegRead(Register_hex), 16) & Convert.ToInt32(Data_hex, 16); //Comment out init sample only            

            isburn = calculatedResult > 0 ? true : false;
                       
            return isburn;
        }

        private static bool OTP_Check_Data(int site, MipiSyntaxParser.ClsMIPIFrame command)
        {
            int calculatedResult = -999999;
            bool isburn = false;

            calculatedResult = OTP_Read_Bytes((byte)site,new List<MipiSyntaxParser.ClsMIPIFrame> { command }) & Convert.ToInt32(command.Data_hex, 16);

            isburn = calculatedResult > 0 ? true : false;

            return isburn;
        }

        public static int OTP_Read_Mod_ID(int Site)
        {
            int Module_ID = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("MOD_ID_NUM_BITS"));

            ReadData_LSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_EFUSE_BYTE4")),16);
            if (NumOfBits > 8)
            {
                ReadData_MSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_EFUSE_BYTE3")), 16);
                Module_ID = (ReadData_MSB * 256) + ReadData_LSB; 
            }
            else
                Module_ID = ReadData_LSB;

            //Mask only number of bits in case pass Flags share register
            Module_ID = MaskOtherBits(Module_ID, NumOfBits);

            return Module_ID;
        }

        public static long Read_Camera_2DID(int Site, int Digits)
        {
            string Camera2DIDstr = "";
            long Camera_2DID = -999;
            try
            {
                string Key2DID = "2DID_OTPID";

                if (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt"))
                {
                    Key2DID = string.Format("2DID_OTPID_{0}", Site + 1);
                }

                Camera2DIDstr = ATFCrossDomainWrapper.GetStringFromCache(Key2DID, "00000000000000000000");

                ////For debug purpose
                ////MessageBox.Show("GetCamera2DID():  " + Camera2DIDstr);

                ///To support additional " ' " added at the front of the 2DID passed from Handler Plugin, so it can be viewed on excel
                ///this part should be obsolete
                if (Camera2DIDstr.Substring(0, 1) == "'")
                {
                    Camera2DIDstr = Camera2DIDstr.Remove(0, 1);
                }

                if (Camera2DIDstr.Length > Digits)
                {
                    int start_index = Camera2DIDstr.Length - Digits;
                    long.TryParse(Camera2DIDstr.Substring(start_index, Digits), out Camera_2DID);
                }

                return Camera_2DID;
            }
            catch (Exception e)
            {
                ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, "OTP 2DID: Error while retrieving 2DID" + Environment.NewLine + e.Message);
                return -999;
            }
            finally
            {
                ATFCrossDomainWrapper.StoreStringToCache("2DID_OTPID", "NA");                
            }
        }

        //Product Specific
        public static long OTP_Read_Mod_2DID(int Site)
        {           
            int NumOfBits = 0;
            int tempMSB = -999999;
            int tempLSB = -999999;
            long Module2DID = -999999;
            
            string PCBLotIDstr = "";
            string Panelstr = "";
            string Stripstr = "";
            string ModuleIDstr = "";

            //PCBPANEL#
            Eq.Site[Site].HSDIO.selectorMipiPair(1,(byte) Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_NUM_BITS"));
            string PCB_PANEL_ID_MSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_ID_MSB_EFUSE");
            tempMSB = ((OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), PCB_PANEL_ID_MSB)& 0x80)>>1);
            string PCB_PANEL_ID_LSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_ID_LSB_EFUSE");
            tempLSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), PCB_PANEL_ID_LSB);
            tempLSB = MaskOtherBits(tempLSB, NumOfBits -1);
            Panelstr = (tempMSB + tempLSB).ToString("D2");

            Thread.Sleep(1);

            //PCBLotID
            Eq.Site[Site].HSDIO.selectorMipiPair(1,(byte) Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_NUM_BITS"));
            string PCB_LOT_ID_MSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_MSB_EFUSE");
            tempMSB = ((OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), PCB_LOT_ID_MSB) & 0xC0)>>6) * 256;
            string PCB_LOT_ID_LSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_LSB_EFUSE");
            tempLSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), PCB_LOT_ID_LSB);
            PCBLotIDstr = (tempMSB + tempLSB).ToString("D4");

            //PCBSTRIP#
            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_STRIP_NUM_BITS"));
            string PCB_STRIP_ID = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_STRIP_ID_EFUSE");
            tempMSB = (OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), PCB_STRIP_ID) & 0x78)>>3;
            tempMSB = MaskOtherBits(tempMSB, NumOfBits);
            Stripstr = tempMSB.ToString("D2");

            //MODULEID
            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_NUM_BITS"));
            string PCB_MODULE_ID_MSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_MSB_EFUSE");
            string PCB_MODULE_ID_LSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_LSB_EFUSE");
            tempMSB = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), PCB_MODULE_ID_MSB);
            tempMSB = MaskOtherBits(tempMSB, 3) * 256;
            tempLSB = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), PCB_MODULE_ID_LSB);
            ModuleIDstr = (tempMSB + tempLSB).ToString("D4");

            string module2DIDstr = PCBLotIDstr + Panelstr + Stripstr + ModuleIDstr;
            long.TryParse(module2DIDstr, out Module2DID);

            return Module2DID;
        }

        public static bool IsMod2DIDTxEmpty(int Site)
        {
            int NumOfBits = 0;
            int tempMSB = -999999;
            int tempLSB = -999999;

            long PCBLotID = 0;
            long Panel = 0;

            //PCBPANEL#
            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_NUM_BITS"));
            string PCB_PANEL_ID_MSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_ID_MSB_EFUSE");
            tempMSB = ((OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"),PCB_PANEL_ID_MSB) & 0x80) >> 1);
            string PCB_PANEL_ID_LSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_PANEL_ID_LSB_EFUSE");
            tempLSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"),PCB_PANEL_ID_LSB);
            tempLSB = MaskOtherBits(tempLSB, NumOfBits - 1);
            Panel = tempMSB + tempLSB;

            Thread.Sleep(1);

            //PCBLotID
            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_NUM_BITS"));
            string PCB_LOT_ID_MSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_MSB_EFUSE");
            tempMSB = ((OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"),PCB_LOT_ID_MSB)& 0xC0) >> 6) * 256;
            string PCB_LOT_ID_LSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_LOT_ID_LSB_EFUSE");
            tempLSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"),PCB_LOT_ID_LSB);
            PCBLotID = tempMSB + tempLSB;

            long module2DIDTx = PCBLotID + Panel;
            
            if(module2DIDTx > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsMod2DIDRxEmpty(int Site)
        {
            int NumOfBits = 0;
            int tempMSB = -999999;
            int tempLSB = -999999;

            long PCBLotID = 0;
            long Strip = 0;
            long ModuleID = 0;

            //PCBSTRIP#
            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_STRIP_NUM_BITS"));
            string PCB_STRIP_ID = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_STRIP_ID_EFUSE");
            tempMSB = (OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), PCB_STRIP_ID) & 0x78) >> 3;
            tempMSB = MaskOtherBits(tempMSB, NumOfBits);
            Strip = tempMSB;

            //MODULEID
            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);
            NumOfBits = Convert.ToInt16(Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_NUM_BITS"));
            string PCB_MODULE_ID_MSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_MSB_EFUSE");
            string PCB_MODULE_ID_LSB = Eq.Site[Site].HSDIO.Get_Digital_Definition("PCB_MODULE_ID_LSB_EFUSE");
            tempMSB = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), PCB_MODULE_ID_MSB);
            tempMSB = MaskOtherBits(tempMSB, 3) * 256;
            tempLSB = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), PCB_MODULE_ID_LSB);
            ModuleID = tempMSB + tempLSB;

            long module2DIDRx = PCBLotID + Strip + ModuleID;

            if (module2DIDRx > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static int OTP_Read_MFG_ID(int Site)
        {
            int MFG_ID = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_NUM_BITS"));

            ReadData_LSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_LSB_READ_EFUSE"));

            if (NumOfBits > 8)
            { 
                ReadData_MSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_MSB_READ_EFUSE"));
                MFG_ID = (ReadData_MSB * 256) + ReadData_LSB;
            }
            else
                MFG_ID = ReadData_LSB;

            //Mask only number of bits in case pass Flags share register
            MFG_ID = MaskOtherBits(MFG_ID, NumOfBits);

            return MFG_ID;
        }

        public static int OTP_Read_MFG_ID_RX(int Site)
        {
            int MFG_ID = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFGID_RX_NUM_BITS"));

            ReadData_LSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFGID_RX_LSB_READ")), 16);

            if (NumOfBits > 8)
            {
                ReadData_MSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFGID_RX_MSB_READ")), 16);
                MFG_ID = (ReadData_MSB * 256) + ReadData_LSB;
            }
            else
                MFG_ID = ReadData_LSB;

            return MFG_ID;
        }

        public static int OTP_Read_MFG_ID(int Site, string digitalNumBits, string regReadData_LSB, string regReadData_MSB)
        {
            int MFG_ID = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            NumOfBits = Convert.ToInt32(digitalNumBits);

            //ReadData_LSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_LSB_READ")), 16);
            ReadData_LSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(regReadData_LSB), 16);

            if (NumOfBits > 8)
            {
                //ReadData_MSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("MFG_ID_MSB_READ")), 16);
                ReadData_MSB = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(regReadData_MSB), 16);
                MFG_ID = (ReadData_MSB * 256) + ReadData_LSB;
            }
            else
                MFG_ID = ReadData_LSB;

            //Mask only number of bits in case pass Flags share register
            MFG_ID = MaskOtherBits(MFG_ID, NumOfBits);

            return MFG_ID;
        }

        public static int OTP_Read_CM_ID(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);

            ReadData = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CM_ID_READ_EFUSE"));

            int cm_id = checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("CM_ID_Bit0")) ? 1 : 0;
            cm_id += (checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("CM_ID_Bit1")) ? 2 : 0);
            return cm_id;
        }

        public static bool OTP_Read_Lock_Bit(int Site)
        {
            int ReadData = -999999;


            ReadData = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("LOCK_BIT_EFUSE"));


            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("LOCK_BIT_BURN"));
        }

        //#DPAT - To update when RF1 outlier flag location has been identified
        public static bool OTP_Read_RF1_Outlier_Bit(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            ReadData = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_OUTLIER_EFUSE"));

            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_OUTLIER_BURN"));
        }

        public static bool OTP_Read_FBAR_Noise_Pass_Flag(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("FBAR_FLAG_READ")),16);


            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("FBAR_FLAG_BURN"));

        }

        public static bool OTP_Read_Noise_Pass_Flag(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("NOISE_FLAG_READ")), 16);


            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("NOISE_FLAG_BURN"));
        }

        public static bool OTP_Read_RF1_Pass_Flag(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_FLAG_READ")),16);

            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("RF1_FLAG_BURN"));
        }

        //ChoonChin - DPAT passflag
        public static bool OTP_Read_DPAT_Pass_Flag(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("DPAT_FLAG_READ")), 16);


            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("DPAT_FLAG_BURN"));
        }


        public static bool OTP_Read_RF2_Pass_Flag(int Site)
        {
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("RF2_FLAG_READ")), 16);

            return checkBitValue(ReadData, Eq.Site[Site].HSDIO.Get_Digital_Definition("RF2_FLAG_BURN"));
        }

        public static int OTP_Read_TX_X(int Site)
        {
            int TX_X = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_X_NUM_BITS"));
            ReadData_LSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_X_LSB_EFUSE"));
            
            if (NumOfBits > 8)
            {
                ReadData_MSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_X_MSB_EFUSE"))>>7;
                TX_X = (ReadData_MSB * 256) + ReadData_LSB;
            }
            else
                TX_X = ReadData_LSB;

            //Mask only number of bits in case pass Flags share register
            TX_X = MaskOtherBits(TX_X, NumOfBits);

            return TX_X;
        }

        public static int OTP_Read_TX_Y(int Site)
        {
            int TX_Y = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_Y_NUM_BITS"));

            ReadData = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_Y_EFUSE"));

            TX_Y = ReadData;

            //Mask only number of bits in case pass Flags share register
            TX_Y = MaskOtherBits(TX_Y, NumOfBits);

            return TX_Y;
        }

        public static int OTP_Read_WAFER_LOT(int Site)
        {
            int WAFER_LOT = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_WAFER_LOT_NUM_BITS"));

            ReadData_LSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_WAFER_LOT_LSB_EFUSE"));

            if (NumOfBits > 8)
            {
                ReadData_MSB = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_WAFER_LOT_MSB_EFUSE"));
                WAFER_LOT = (ReadData_MSB * 256) + ReadData_LSB;
            }
            else
                WAFER_LOT = ReadData_LSB;

            //Mask only number of bits in case pass Flags share register
            WAFER_LOT = MaskOtherBits(WAFER_LOT, NumOfBits);

            return WAFER_LOT;
        }

        public static int OTP_Read_WAFER_ID(int Site)
        {
            int WAFER_ID = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_WAFER_ID_NUM_BITS"));

            ReadData = OTP_Read_Bytes((byte)Site, 1, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI1_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("CMOS_TX_WAFER_ID_EFUSE"));

            WAFER_ID = ReadData;

            //Mask only the first 6 MSB Bits
            //WAFER_ID = (WAFER_ID & 252) >> 2;
            WAFER_ID = MaskOtherBits(WAFER_ID, NumOfBits, false);

            return WAFER_ID;
        }

        public static int OTP_Read_LNA_X(int Site)
        {
            int LNA_X = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_X_NUM_BITS"));

            ReadData = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_X_LSB_EFUSE"));

            LNA_X = ReadData;

            //Mask only number of bits in case pass Flags share register
            LNA_X = MaskOtherBits(LNA_X, NumOfBits);

            return LNA_X;
        }

        public static int OTP_Read_LNA_Y(int Site)
        {
            int LNA_Y = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_Y_NUM_BITS"));

            //ReadData = ((Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_Y_MSB_READ")), 16) & 0x80) >> 7) * 256;
            //ReadData += Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_Y_LSB_READ")), 16);

            ReadData = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_Y_LSB_EFUSE"));

            LNA_Y = ReadData;

            //Mask only number of bits in case pass Flags share register
            LNA_Y = MaskOtherBits(LNA_Y, NumOfBits);

            return LNA_Y;
        }

        public static int OTP_Read_LNA_WAFER_LOT(int Site)
        {
            int LNA_WAFER_LOT = -999999;
            int ReadData_MSB = -999999;
            int ReadData_LSB = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_WAFER_LOT_NUM_BITS"));

            ReadData_LSB = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_WAFER_LOT_LSB_EFUSE"));

            if (NumOfBits > 8)
            {
                ReadData_MSB = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_WAFER_LOT_MSB_EFUSE"));
                LNA_WAFER_LOT = (ReadData_MSB * 256) + ReadData_LSB;
            }
            else
                LNA_WAFER_LOT = ReadData_LSB;

            //Mask only number of bits in case pass Flags share register
            LNA_WAFER_LOT = MaskOtherBits(LNA_WAFER_LOT, NumOfBits);

            return LNA_WAFER_LOT; 
        }

        public static int OTP_Read_LNA_WAFER_ID(int Site)
        {
            int LNA_WAFER_ID = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_WAFER_ID_NUM_BITS"));

            ReadData = OTP_Read_Bytes((byte)Site, 2, Eq.Site[Site].HSDIO.Get_Digital_Definition("MIPI2_SLAVE_ADDR"), Eq.Site[Site].HSDIO.Get_Digital_Definition("LNA_WAFER_ID_EFUSE"));
     
            LNA_WAFER_ID = ReadData;

            //Mask only the first 6 MSB Bits
            //LNA_WAFER_ID = (LNA_WAFER_ID & 252) >> 2;
            LNA_WAFER_ID = (LNA_WAFER_ID >> 2) & (int)(Math.Pow(2, NumOfBits) - 1);

            return LNA_WAFER_ID;
        }

        public static int OTP_Read_Rev_ID(int Site)
        {
            int REV_ID = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("REV_ID_NUM_BITS"));

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("REV_ID_READ")),16); 

            REV_ID = ReadData;

            //Mask only number of bits in case pass Flags share register
            REV_ID = MaskOtherBits(REV_ID, NumOfBits);

            return REV_ID;
        }

        public static int OTP_Read_USID(int Site)
        {
            int USID = -999999;
            int ReadData = -999999;
            int NumOfBits = 0;

            Eq.Site[Site].HSDIO.selectorMipiPair(1, (byte)Site);

            NumOfBits = Convert.ToInt32(Eq.Site[Site].HSDIO.Get_Digital_Definition("USID_NUM_BITS"));

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("USID_REG")), 16);

            USID = ReadData;

            //Mask only number of bits in case pass Flags share register
            USID = MaskOtherBits(USID, NumOfBits);

            return USID;
        }

        public static int OTP_ModID_Select(int Site)
        {
            bool iS2DID = true;    // must set to true after Proto2A
            int Selected = -999999;
            //At the initial stage, with 2DID there is no MFGID. Currently hardcoded to 10.
            //int MFG_ID = OTP_Read_MFG_ID(Site);
            int MFG_ID = -999999;
            long MOD_ID = -999999;

            if (iS2DID)
            {
                MFG_ID = OTP_Read_MFG_ID(Site);
                MOD_ID = OTP_Read_Mod_2DID(Site);
            }
            else
            {
                MFG_ID = OTP_Read_MFG_ID(Site);
                MOD_ID = OTP_Read_Mod_ID(Site);
            }

            if (ModuleIDSelect.ContainsKey(MFG_ID))
                if (ModuleIDSelect[MFG_ID].Contains(MOD_ID))
                    Selected = 0;//Find selected Mod_ID and MFG_ID

            return Selected;
        }

        public static int OTP_2DID_Select(int Site)
        {
            int Selected = -999999;
            string Camera2DIDstr = "";

            try
            {
                string Key2DID = "2DID_OTPID";

                if (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt"))
                {
                    Key2DID = string.Format("2DID_OTPID_{0}", Site + 1);
                }

                Camera2DIDstr = ATFCrossDomainWrapper.GetStringFromCache(Key2DID, "00000000000000000000");
                //Camera2DIDstr = "214500139993009005070069";
                ////For debug purpose
                ////MessageBox.Show("GetCamera2DID():  " + Camera2DIDstr);

                ///To support additional " ' " added at the front of the 2DID passed from Handler Plugin, so it can be viewed on excel
                ///this part should be obsolete
                if (Camera2DIDstr.Substring(0, 1) == "'")
                {
                    Camera2DIDstr = Camera2DIDstr.Remove(0, 1);
                }
            }
            catch (Exception e)
            {
                ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eTestPlan, "OTP 2DID: Error while retrieving 2DID" + Environment.NewLine + e.Message);
            }
            finally
            {
                ATFCrossDomainWrapper.StoreStringToCache("2DID_OTPID", "NA");
            }

            if (Str2DIDSelect.ContainsKey(Camera2DIDstr))
                Selected = 0;//Find selected Mod_ID and MFG_ID

            return Selected;
        }

        public static int OTP_Read_Rx_Lockbit(int Site)
        {
            int Rx_Lockbit = -999999;
            int ReadData = -999999;

            Eq.Site[Site].HSDIO.selectorMipiPair(2, (byte)Site);

            ReadData = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead(Eq.Site[Site].HSDIO.Get_Digital_Definition("RX_LOCKBIT_READ")), 16);

            Rx_Lockbit = ReadData;

            Rx_Lockbit = (Rx_Lockbit & 0x80) >> 7;

            return Rx_Lockbit;
        }

        //To return the wanted LSB or MSB bits of a byte of Data (8 Bits). Default return the LSB bits.
        public static int MaskOtherBits(int Data, int NumOfBits, bool MSB = false)
        {
            int Mask = 0;
            int DataOut = 0;

            if (MSB == false) //LSB
            {
                for (int i = 0; i < NumOfBits; i++)
                {
                    Mask |= (1 << i);
                }
                DataOut = Data & Mask;
            }
            else//MSB
            {
                DataOut = Data >> (8 - NumOfBits);
            }

            return DataOut;
        }

        public static bool checkBitValue(int Data, String Bitlocation)
        {
            string[] BitNumber = Bitlocation.Split(new char[] { 'B' }, StringSplitOptions.RemoveEmptyEntries);

            int Mask = (1 << Convert.ToInt32(BitNumber[BitNumber.Length - 1]));
            int Value = Data & Mask;

            if (Value == Mask)
                return true;
            else
                return false;
        }

        private static string[] Vectorselector(int Site, MipiSyntaxParser.ClsMIPIFrame _temp)  //20190709 new OtpTest
        {
            try
            {
                string Dietype = _temp.Pair == 1 ? "TX" : "RX";
                int Bitcount = 0;

                int intData = Int32.Parse(_temp.Data_hex, System.Globalization.NumberStyles.HexNumber);
                char[] Data_char = (Convert.ToString(intData, 2).PadLeft(8, '0')).ToCharArray();  //convert to Binary string                        
                string[] bitVectors = new string[Data_char.Length];
                System.Array.Reverse(Data_char);
                foreach (char Value in Data_char)
                {
                    if (Value == '1')
                        bitVectors[Bitcount] = "S" + _temp.SlaveAddress_hex + "_" +
                                     Eq.Site[Site].HSDIO.Get_Digital_Definition(Dietype + "_EFUSE_BYTE" + _temp.Register_hex) + "_" +
                                     "B" + Convert.ToString(Bitcount);

                    Bitcount++;
                }
                return bitVectors;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during Vectorselector in Otpburn" + "\r\n" + e.ToString());
                return null;
            }
        }  

        private static void PreOTPstage(int Site, int _dutSlavePairIndex, string firstOTPByte = null,  eMipiTestType _OtpType = eMipiTestType.OTPburn, MipiSyntaxParser.ClsMIPIFrame clsMIPIFrame = null)
        {
            if (_dutSlavePairIndex == 2)
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("RX_FABSUPPLIER"))
                {
                    case "TSMC":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            EnableOTPburnTemplate = true;

                            Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.1);
                            Eq.Site[Site].HSDIO.RegWrite("F0", "00");
                            Eq.Site[Site].HSDIO.RegWrite("F0", "C0");

                            Eq.Site[Site].DC["Vdd"].ForceVoltage(2.5, 0.1);  //Changed from 1.2V to 2.5V acocording to the Designer input
                            Thread.Sleep(2);

                            if (isRx2ndMemory[Site])
                                Eq.Site[Site].HSDIO.RegWrite("F1", "02");
                            else
                                Eq.Site[Site].HSDIO.RegWrite("F1", "01");
                        }
                        break;

                    case "GF":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            EnableOTPburnTemplate = true;

                            Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.1);
                            Eq.Site[Site].DC["Vdd"].ForceVoltage(2.8, 0.1);
                            Thread.Sleep(2);
                        }
                        break;

                    default: break;
                }
            }
            else
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER"))
                {
                    case "COMMON":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            EnableOTPburnTemplate = true;
                            Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.1);
                            if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                            {
                                Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                            }
                        }
                        break;
                    case "TSMC_130NM":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            Eq.Site[Site].DC["Vio1"].ForceVoltage(0, .0001);
                            Thread.Sleep(5);
                            Eq.Site[Site].DC["Vio1"].ForceVoltage(1.2, .032);
                            Thread.Sleep(5);

                            Eq.Site[Site].DC["Vbatt"].ForceVoltage(2.5, .15); //NUWA uses 2.5V Vbatt 
                            Thread.Sleep(100);
                        }
                        else if (_OtpType == eMipiTestType.OTPread)
                        {
                            if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "") Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex, false);
                        }
                        break;

                    case "TSMC_65NM":                        
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            EnableOTPburnTemplate = true;
                            Eq.Site[Site].DC["Vio1"].ForceVoltage(0, .0001);
                            Thread.Sleep(5);
                            Eq.Site[Site].DC["Vio1"].ForceVoltage(1.2, .032);
                            Thread.Sleep(5);

                            Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "B0"); // Pre Vbatt On command
                            Thread.Sleep(1);
                            Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "F0"); // Pre Vbatt On command
                            Thread.Sleep(1);
                            //**************************************************
                            Eq.Site[Site].DC["Vbatt"].ForceVoltage(2.5, .15); //NUWA uses 2.5V Vbatt 
                            //Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, .15); //NUWA uses 2.5V Vbatt 
                            Thread.Sleep(5);

                            if (Convert.ToInt32(firstOTPByte, 16) < 8) //First Efuse Byte
                            {
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "F4"); // Post Vbatt On command
                                Thread.Sleep(1);
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "E4"); // Post Vbatt On command
                                Thread.Sleep(1);
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "E0"); // Post Vbatt On command
                                Thread.Sleep(1);
                            }
                            else //Second Efuse Byte
                            {
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "F8"); // Post Vbatt On command
                                Thread.Sleep(1);
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "D8"); // Post Vbatt On command
                                Thread.Sleep(1);
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "D0"); // Post Vbatt On command
                                Thread.Sleep(1);
                            }
                        }
                        else if (_OtpType == eMipiTestType.OTPread)
                        {
                            if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "") Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex, false);

                            AdditionalOTPSeqRequired[Site] = true;

                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");

                            Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "B0"); // Post Vbatt On command

                            if (Convert.ToInt32(firstOTPByte, 16) < 8) //First Efuse Byte
                            {
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "B1"); // Post Vbatt On command
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "A1"); // Post Vbatt On command
                            }
                            else //Second Efuse Byte
                            {
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "B2"); // Post Vbatt On command
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "92"); // Post Vbatt On command
                            }

                            Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_ADDR_CTRL_CMOS65NM"), "1" + clsMIPIFrame.Register_hex); // Post Vbatt On command
                        }
                        break;

                    default: break;
                }
            }
        }
        private static void PreOTPstage_Vbatt3p8(int Site, int _dutSlavePairIndex) 
        {
            if (_dutSlavePairIndex == 2)
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("RX_FABSUPPLIER"))
                {
                    case "TSMC":
                        EnableOTPburnTemplate = true;

                        Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.1);
                        Eq.Site[Site].HSDIO.RegWrite("F0", "00");
                        Eq.Site[Site].HSDIO.RegWrite("F0", "C0");

                        Eq.Site[Site].DC["Vdd"].ForceVoltage(1.2, 0.1);
                        Thread.Sleep(2);

                        if (isRx2ndMemory[Site])
                            Eq.Site[Site].HSDIO.RegWrite("F1", "02");
                        else
                            Eq.Site[Site].HSDIO.RegWrite("F1", "01");

                        break;

                    case "GF":
                        EnableOTPburnTemplate = true;

                        Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.1);
                        Eq.Site[Site].DC["Vdd"].ForceVoltage(2.8, 0.1);
                        Thread.Sleep(2);

                        break;

                    default: break;
                }
            }
            else
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER"))
                {
                    case "COMMON":
                        EnableOTPburnTemplate = true;



                        Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.1);
                        //Experimental test condition==========================
                        //6(0x1C,0x40)(0x0,0x07)(0x3,0x02)(0x4,0x40)(0x5,0x08)(0x6,0x20)(0x15,0x85)
                        //(0x13,0xB4)(0x14,0xA8)(0x11,0x00)(0x12,0x00)(0x1C,0x7)(0x2E,0x2)

                        //Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                        Eq.Site[Site].HSDIO.RegWrite("00", "01");
                        //Eq.Site[Site].HSDIO.RegWrite("03", "02");
                        Eq.Site[Site].HSDIO.RegWrite("04", "40");
                        Eq.Site[Site].HSDIO.RegWrite("05", "02");
                        //Eq.Site[Site].HSDIO.RegWrite("06", "20");
                        //Eq.Site[Site].HSDIO.RegWrite("15", "85");
                        Eq.Site[Site].HSDIO.RegWrite("13", "FE");
                        Eq.Site[Site].HSDIO.RegWrite("14", "FE");
                        Eq.Site[Site].HSDIO.RegWrite("11", "00");
                        Eq.Site[Site].HSDIO.RegWrite("12", "00");
                        Eq.Site[Site].HSDIO.RegWrite("1C", "07");
                        Eq.Site[Site].HSDIO.RegWrite("2E", "FF");

                        //==========================================================


                        Eq.Site[Site].HSDIO.RegWrite("1C", "07");
                        Thread.Sleep(50);
                        Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.0, 0.1);
                        if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                        {
                            Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                        }
                        break;
                
                    default: break;
                }
            }
        }

        private static void PostOTPstage(int Site, int _dutSlavePairIndex, eMipiTestType _OtpType = eMipiTestType.OTPburn) //20190709 new OtpTest
        {
            if (_dutSlavePairIndex == 2)
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("RX_FABSUPPLIER"))
                {
                    case "TSMC":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            Eq.Site[Site].HSDIO.RegWrite("F1", "00");

                            Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.0001);
                            Thread.Sleep(1);

                            Eq.Site[Site].HSDIO.RegWrite("F0", "20");
                            Eq.Site[Site].HSDIO.RegWrite("F0", "10");


                            Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                            Thread.Sleep(1);
                            Eq.Site[Site].HSDIO.SendVector("VIOON");
                            Thread.Sleep(1);
                        }
                        break;

                    case "GF":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.0001);
                            Thread.Sleep(1);

                            Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                            Thread.Sleep(1);
                            Eq.Site[Site].HSDIO.SendVector("VIOON");
                            Thread.Sleep(1);
                        }
                        break;

                    default: break;
                }
            }
            else
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER"))
                {
                    case "COMMON":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                            Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                            Eq.Site[Site].HSDIO.SendVector("VIOON");
                            //Eq.Site[Site].HSDIO.RegWrite("2B", "0F");

                            Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                            Thread.Sleep(10);
                            // RF2 uses NI4143 for Vbatt
                            if (Eq.Site[Site].DC["Vbatt"].VisaAlias.Contains("NI4143") ? true : false) Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.15);
                            else Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.2);
                            //
                            Thread.Sleep(1);
                        }
                        break;
                    case "TSMC_130NM":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            Eq.Site[Site].DC["Vio1"].ForceVoltage(0, .0001);
                            Thread.Sleep(5);
                            Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                            Thread.Sleep(1);
                            Eq.Site[Site].DC["Vio1"].ForceVoltage(1.2, .032);
                            Thread.Sleep(5);

                            // RF2 uses NI4143 for Vbatt
                            //if (Eq.Site[Site].DC["Vbatt"].VisaAlias.Contains("NI4143") ? true : false) Eq.Site[Site].DC["Vbatt"].ForceVoltage(5.5, 0.15);
                            //else Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.2);
                            Thread.Sleep(1);
                        }
                        if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                        {
                            Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                        }

                        break;
                    case "TSMC_65NM":
                        if (_OtpType == eMipiTestType.OTPburn)
                        {
                            Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "F0"); // Post Efuse burn command
                            Thread.Sleep(1);
                            Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "30"); // Post Efuse burn command
                            Thread.Sleep(1);
                        }
                        else if (_OtpType == eMipiTestType.OTPread)
                        {
                            if (EqHSDIO.ADJUST_BusIDCapTuningInHex != "")
                            {
                                Eq.Site[Site].HSDIO.RegWrite("2B", EqHSDIO.ADJUST_BusIDCapTuningInHex);
                            }

                            if(AdditionalOTPSeqRequired[Site])
                            {
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "B0"); // Post Efuse burn command
                                Eq.Site[Site].HSDIO.RegWrite(Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM"), "30"); // Post Efuse burn command
                                AdditionalOTPSeqRequired[Site] = false;
                            }
                        }

                        break;

                    default: break;
                }
            }

        }

        private static void PostOTPstage_Vbatt3p8(int Site, int _dutSlavePairIndex) 
        {
            if (_dutSlavePairIndex == 2)
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("RX_FABSUPPLIER"))
                {
                    case "TSMC":
                        Eq.Site[Site].HSDIO.RegWrite("F1", "00");

                        Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.0001);
                        Thread.Sleep(1);

                        Eq.Site[Site].HSDIO.RegWrite("F0", "20");
                        Eq.Site[Site].HSDIO.RegWrite("F0", "10");


                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        Eq.Site[Site].HSDIO.SendVector("VIOON");
                        break;

                    case "GF":
                        Eq.Site[Site].DC["Vdd"].ForceVoltage(0, 0.0001);
                        Thread.Sleep(1);

                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        Eq.Site[Site].HSDIO.SendVector("VIOON");
                        Thread.Sleep(1);

                        break;

                    default: break;
                }
            }
            else
            {
                switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER"))
                {
                    case "COMMON":
                        Eq.Site[Site].HSDIO.RegWrite("1C", "40");
                        Eq.Site[Site].HSDIO.SendVector("VIOOFF");
                        Eq.Site[Site].HSDIO.SendVector("VIOON");
                        //Eq.Site[Site].HSDIO.RegWrite("2B", "0F");

                        Eq.Site[Site].DC["Vbatt"].ForceVoltage(0, 0.0001);
                        Thread.Sleep(10);
                        // RF2 uses NI4143 for Vbatt
                        if (Eq.Site[Site].DC["Vbatt"].VisaAlias.Contains("NI4143") ? true : false) Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.15);
                        else Eq.Site[Site].DC["Vbatt"].ForceVoltage(3.8, 0.2);
                        //
                        Thread.Sleep(1);
                        break;

                    default: break;
                }
            }

        }
    }

    /// <summary>
    /// Ensure no duplicate result. Update the result value if there's duplicate.
    /// </summary>
    public class DuplicateDetector
    {
        public void Add(byte site, string testName, string units, double rawResult, byte decimalPlaces = byte.MaxValue,
            bool skipSpecCheck = false)
        {
            bool isNotExist = !ResultBuilder.ParametersDict[site].ContainsKey(testName);
            if (isNotExist)
            {
                ResultBuilder.AddResult(site, testName, "", rawResult, 1, skipSpecCheck);
                return;
            }

            // Update the value.
            int index = ResultBuilder.results.Data.FindIndex(x => x.Name == testName);
            List<double> r = ResultBuilder.results.Data[index].Vals;
            if(ResultBuilder.ValidSites.Count > 1)
            {
                while (r.Count < site + 1)
                {
                    r.Add(double.NaN);
                }

                r[site] = rawResult;
            }
            else
            {
                r[0] = rawResult;
            }           
        }
    }

}