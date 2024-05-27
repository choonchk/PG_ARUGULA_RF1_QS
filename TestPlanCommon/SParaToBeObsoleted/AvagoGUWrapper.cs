using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using Avago.ATF.Shares;
using GuCal;
using Avago.ATF.StandardLibrary;
using CalLib;
using EqLib;
using InstrLib;
using TestLib_Legacy;

namespace ToBeObsoleted
{
    /// <summary>
    /// Unused. Can be deleted safely.
    /// </summary>
    public class AvagoGUWrapper
    {
        public GU.UI UItype = GU.UI.FullAutoCal;
        GU.UI ICCCAL_UI = GU.UI.FullAutoCal;

        /// <summary>
        /// this output variable is still used in TP.
        /// </summary>
        private string ProductTag;

        public string DoAtfInitProductTag(string clothoRootDir)
        {
            ProductTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, ""); //All users mode can retrieve now
            if (!clothoRootDir.Contains("TestPlans"))
            {
                if (ProductTag == "")   //Debug Mode
                {
                    ProductTag = "Joker";
                    ICCCAL_UI = GU.UI.FullAutoCal;
                }
                else if (ProductTag.Contains("AFEM-8100-AP1"))
                {
                    ProductTag = "AFEM-8100-AP1";
                    ICCCAL_UI = GU.UI.FullAutoCal;
                }
                else if (ProductTag.Contains("AFEM-8100-AP1"))
                {
                    ProductTag = "AFEM-8100-AP1";
                    ICCCAL_UI = GU.UI.FullAutoCal;
                }
                else
                {
                    ProductTag = "Joker";
                    ICCCAL_UI = GU.UI.FullAutoCal;
                }
            }
            else
            {
                ProductTag = "AFEM-8100-AP1";
                ICCCAL_UI = GU.UI.FullAutoCal;
            }

            return ProductTag;
        }

        public void SetEngineering()
        {
            ICCCAL_UI = GU.UI.FullAutoCal;
        }

        public string DoAtfInitAfterCustomCode()
        {
            try
            {
                int mustIccGuCalCached = -1;
                //mustIccGuCalCached = ATFCrossDomainWrapper.GetIntFromCache(PublishTags.PUBTAG_MUST_IccGuCal, mustIccGuCalCached);

                //GU.DoInit_afterCustomCode(mustIccGuCalCached, ICCCAL_UI, false, true, ProductTag, @"\\10.50.10.35\avago\ZDBFolder");    //  sanjose: @"\\rds35\zdbdatasj\zDBData"   asekr : @"\\10.50.10.35\avago\ZDBFolder"
                //GU.DoInit_afterCustomCode(mustIccGuCalCached, ICCCAL_UI, false, false, ProductTag, @"C:\Avago.ATF.Common\Results", 0, 1, 0, true);    //  sanjose: @"\\rds35\zdbdatasj\zDBData"   asekr : @"\\10.50.10.35\avago\ZDBFolder"
                GU.DoInit_afterCustomCode(mustIccGuCalCached, UItype, false, false, ProductTag,
                    @"\\rds35\zdbdatasj\zDBData", EqLib.Eq.NumSites);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Icc Cal loading error!" +
                                Environment.NewLine +
                                "Test aborted!" +
                                "Error message: " + ex.Message, "Test Plan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //throw new Exception();
                return TestPlanRunConstants.RunFailureFlag + "Icc Cal loading error!";
            }
            return String.Empty;
        }

        public void DoAtfTestPre()
        {
        }

        public void DoAtfTestPre2(bool isHeaderFileMode)
        {
        }

        public void DoAtfTestMoveNext(ATFReturnResult results)
        {
            GU.DoTest_afterCustomCode(ref results);     // still needed.
        }

        public bool IsRunning
        {
            get { return true; }
        }

        public bool GuCalibration(IATFTest clothoInterface,
    string destCFpath, int mustIccGuCalCached, string isEngineeringMode,
    string ProductTagName)
        {
            bool isSuccess = true;

            string ProductTagGU = "";

            ////Format for package name in production. E.G. for PA "AFEM-9100-SG1-RF1_BE-PXI-NI_v0001"
            ////ProductTagGU is customized for every products.
            //if (ProductTagName == "")
            //{
            //    ProductTagGU = ProductTagName = "Seoraksan1p7";
            //}
            //else if (ProductTagName.StartsWith("AFEM-9100") && ProductTagName.Contains("RF1"))
            //{
            //    ProductTagGU = "AFEM-9100-SG1-RF1";
            //}
            //else if (ProductTagName.StartsWith("AFEM-9106") && ProductTagName.Contains("RF1"))
            //{
            //    ProductTagGU = "AFEM-9106-SG1-RF1";
            //}
            //else
            //{
            //    string destCFpath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");
            //    ProductTagGU = Path.GetFileName(destCFpath).ToUpper().Replace(".CSV", "");  // CF               
            //}

            ProductTagGU = ProductTagName = "Seoraksan1p7";

            GU.DoInit_afterCustomCode(mustIccGuCalCached, UItype, false, false, ProductTagGU, @"\\rds35\zdbdatasj\zDBData", Eq.NumSites);    //  sanjose: @"\\rds35\zdbdatasj\zDBData"   asekr : @"\\10.50.10.35\avago\ZDBFolder"

            bool ProductionMode = isEngineeringMode == "FALSE" ? true : false;

            if (UItype == GU.UI.FullAutoCal && GU.GuInitSuccess)
            {
                AutoCal myAutoCal = new AutoCal(clothoInterface, ProductionMode,0);

                myAutoCal.guTrayCoord = new Dictionary<int, string>();

                int guDutIndex = 0;
                for (int row = 0; row < 5; row++)   // define a bunch of GU tray coordinates, starting below cal substrates, 7 columns wide
                {
                    for (int col = 0; col < 8; col++)
                    {
                        myAutoCal.guTrayCoord.Add(guDutIndex++, col + "," + row);
                    }
                }



                myAutoCal.ShowForm();

                // Engineering mode. Skip Button bybasses failed GU
                if (isEngineeringMode == "TRUE")
                {
                    // Production Mode. Skip Button fails program load
                    isSuccess = !GU.thisProductsGuStatus.IsVerifyExpired(0);
                    // Note: Not &= but set directly.
                    //m_modelTpState.programLoadSuccess = isSuccess;
                }
            }

            return isSuccess;
        }


    }

    public class InstrLibWrapper
    {
        public enum HSDIO_Define
        {
            //NI6570_1 = 0,
            //NI6570_0 = 1,
            //NI6570_2 = 2,
            //NI6570_3 = 5,
            //NI6570_4 = 4,
            //NI6570_5 = 3,

            // Cheeon modified
            NI6570_5 = 0,
            NI6570_7 = 1,
            NI6570_4 = 2,
            NI6570_10 = 5,
            NI6570_11 = 4,
            NI6570_9 = 3,
        }

        /// <summary>
        /// Required.
        /// </summary>
        public bool DoAtfInitMipiWaveformLoad(string clothoRootDir,
            Dictionary<string, int> Script_List)
        {
            string vectorBasePath = clothoRootDir + @"RFFE_vectors\";
            bool programLoadSuccess = true;
            programLoadSuccess &= HSDIO.Initialize(PaTest.SmuResources, false, "E", true); //Slave address here is actually meaningless

            foreach (KeyValuePair<string, int> Script_Test in Script_List)
            {
                programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + Script_Test.Key + ".vec", Script_Test.Key);
            }


            //for (int i = 0; i < Script.Length; i++)
            //{
            //    System.IO.File.Delete(vectorBasePath + Script[i] + ".vec");
            //}

            #region Loading of the named vectors

            // added by CheeOn 27092017 functional test
            if (System.IO.File.Exists(vectorBasePath + "\\OTP\\RX\\OTPLNA_VERIFY_Rev15.vec"))
            {
                programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\OTP\\RX\\OTPLNA_VERIFY_Rev15.vec", "RXOTPVERIFY");
            }
            else
            { 
                MessageBox.Show("Error: Specified LNA OTP file does not exist at RFFE_VEctors\\OTP\\RX\\  Check revision on TCF Main Tab");
            }

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\OB61_NRZ_Full_speed_Functional_Test.vec", "FUNCTIONAL");
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\2018_OMH_4FX9A_Functional_Test_split_v08.vec", "RXFUNCTIONAL");

            //End


            // programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\OTP\\TX\\JEDI_A5B_Burn_OTP_00_01_52_01_TEST.vec", "OTP");
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\OTP\\TX\\WRITE_3_C0_A8.vec", "ROCK");

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\1C80_test2.vec", "TRIGOFF");
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\1C01_test2.vec", "TRIGON");

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\WRITE_3_2B_0F.vec", "TX2B", false, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\WRITE_E_2B_0F.vec", "RX2B", false, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\VIOOFF.vec", "VIOOFF", false, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\VIOON.vec", "VIOON", false, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\TEMP\\WRITE_3_10_00.vec", "TEMPENABLE", false, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\TEMP\\READ_3_10_00_100050_100065.vec", "TEMP", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\PID\\READ_TXSEORAKSAN_1D_52MHZ.vec", "TXPID", true, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\PID\\READ_RXSEORAKSAN_1D_52MHZ.vec", "RXPID", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MID\\READ_TXSEORAKSAN_1E_52MHZ.vec", "TXMID", true, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MID\\READ_RXSEORAKSAN_1E_52MHZ.vec", "RXMID", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\USID\\READ_TXSEORAKSAN_1F_52MHZ.vec", "TXUSID", true, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\USID\\READ_RXSEORAKSAN_1F_52MHZ.vec", "RXUSID", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E0_00_100050_100065_52MHZ.vec", "READE0", true, false);      //MFG_ID MSB
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E1_00_100050_100065_52MHZ.vec", "READE1", true, false);      //MFG_ID LSB
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E2_00_100050_100065_52MHZ.vec", "READE2", true, false);      //RevID
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E3_00_100050_100065_52MHZ.vec", "READE3", true, false);      // Module_ID MSB    FBAR FLAG = Bit#7, RF1 Flag = bit#6 
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E4_00_100050_100065_52MHZ.vec", "READE4", true, false);      //Module_ID LSB
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E5_00_100050_100065_52MHZ.vec", "READE5", true, false);      // lock Bit

            //OTP burn FBAR flag
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\UNITID\\ModuleID\\RegE5_Bit0.vec", "RegE5_Bit0", true, false); // FBAR PASS FLAG bit#0(E5)

            #endregion Loading of the named vectors

            return programLoadSuccess;
        }

        /// <summary>
        /// Required.
        /// </summary>
        /// <returns></returns>
        public string DoAtfInitSmu()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList =
                new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vlna", "NI4143.1"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbiast", "NI4143.2"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcpl", "NI4143.3"));

            foreach (KeyValuePair<string, string> kv in DCpinSmuResourceTempList)
            {
                string DcPinName = kv.Key;
                string pinName = DcPinName.Remove(0, 2);
                string VisaAlias = kv.Value.Split('.')[0];
                string Chan = kv.Value.Split('.')[1];

                try
                {
                    PaTest.SmuResources.Add(pinName, Smu.getSMU(VisaAlias, Chan, pinName, true));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SMU Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //programLoadSuccess = false;
                    return TestPlanRunConstants.RunFailureFlag + "Failed SMU Initialization";
                }
            }
            return String.Empty;

        }

        public string[] DoAtfInitGenVector(List<Dictionary<string, string>> DicTestCondMipi, 
            string[] Script, Dictionary<string, int> Script_List)
        {
            List<string> List_Vector = new List<string>();
            string[] result = GenVector.CustomList(DicTestCondMipi, Script, Script_List, ref List_Vector);
            return result;
        }

        /// <summary>
        /// OTP - Burn lock bit. Required production best practice.
        /// </summary>
        public void ProdBestPracticeOtpEFuse()
        {
            HSDIO.Instrument.SendVector("VIOON");
            PaTest.SmuResources["Vbatt"].ForceVoltage(5.5, 0.02);
            PaTest.SmuResources["Vlna"].ForceVoltage(0, 0.1);
            PaTest.SmuResources["Vcc"].ForceVoltage(0, 0.1);
            HSDIO.Instrument.SendVector("VIOOFF");
            HSDIO.Instrument.SendVector("VIOON");
        }

        /// <summary>
        /// After OTP - Burn lock bit, before progressive zip. Required production best practice.
        /// </summary>
        public void ProdBestPractice1()
        {
            //HSDIO.Instrument.SendVector("viooff");
            Eq.Site[0].HSDIO.SendVector("viooff");
        }

        /// <summary>
        /// After OTP - Burn lock bit, before progressive zip. Required production best practice.
        /// </summary>
        public void ProdBestPractice2()
        {
            // HSDIO.Instrument.SendVector(HSDIO.HiZ);  

            foreach (string pinName in PaTest.SmuResources.Keys)
            {
                //if (HSDIO.IsMipiChannel(pinName)) continue;
                if (pinName.ToUpper().Contains("SCLK") || pinName.ToUpper().Contains("SDATA")) continue; // || pinName.ToUpper().Contains("VIO")

                //PaTest.SmuResources[pinName].ForceVoltage(0, PaTest.SmuResources[pinName].priorCurrentLim);

                Eq.Site[0].DC[pinName].ForceVoltage(0, Eq.Site[0].DC[pinName].priorCurrentLim);
            }
            //Eq.Site[0].HSDIO.SendVector(EqHSDIO.HiZ);
        }

        /// <summary>
        /// Required production best practices.
        /// </summary>
        public void DoAtfUnInit()
        {
            foreach (string pinName in PaTest.SmuResources.Keys)
            {
                if (HSDIO.IsMipiChannel(pinName)) continue;
                PaTest.SmuResources[pinName].CloseSession();
            }
        }

        public int DoATFTest2(string cmd)
        {
            Eq.Site[0].HSDIO.SendVector(cmd);
            return Eq.Site[0].HSDIO.GetNumExecErrors(cmd);
        }

        public void DoATFTestReset()
        {
            Eq.Site[0].HSDIO.SendVector("VIOOFF".ToLower());
            Thread.Sleep(5);
            Eq.Site[0].HSDIO.SendVector("VIOON".ToLower());
            Thread.Sleep(5);
        }

        public void SetHsdioFirstScriptNA()
        {
            HSDIO.FirstScriptNA = true;
        }

        public bool IsHsdioUseScript
        {
            get { return HSDIO.useScript; }
            set { HSDIO.useScript = value; }
        }

        public string DoAtfInit2()
        {
            HSDIO_Define[] GetNum = (HSDIO_Define[])Enum.GetValues(typeof(HSDIO_Define));
            string[] PinName_Array = new string[6]; PinName_Array[0] = "Sdata1"; PinName_Array[1] = "Sclk1"; PinName_Array[2] = "Vio1";
            PinName_Array[3] = "Sdata2"; PinName_Array[4] = "Sclk2"; PinName_Array[5] = "Vio2";
            int PinNameCount = 0;

            foreach (HSDIO_Define Get in GetNum)
            {
                //string DcPinName = kv.Key;
                string pinName = PinName_Array[PinNameCount];
                string VisaAlias = Get.ToString().Split('_')[0];
                string Chan = Get.ToString().Split('_')[1];

                try
                {
                    PaTest.SmuResources.Add(pinName, Smu.getSMU(VisaAlias, Chan, pinName, true));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SMU Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //programLoadSuccess = false;
                    return TestPlanRunConstants.RunFailureFlag + "Failed SMU Initialization";
                }
                PinNameCount++;
            }

            return String.Empty;
        }
    }

    /// <summary>
    /// ClothoLibAlgo depends on TestLib_Legacy, where PaTest belongs. So all PATest is here.
    /// </summary>
    public class ClothoLibAlgoWrapper
    {
        /// <summary>
        /// Required. ProdBestPractice.
        /// </summary>
        public int FailedTestCount
        {
            get { return PaTest.FailedTests.Count; }
        }

        public bool IsHeaderFileMode
        {
            get { return PaTest.headerFileMode; }
        }


        public void DoATFInit1(bool isHsdioUseScript, string[] NAScript,
            Dictionary<string, int> Script_List)
        {
            if (isHsdioUseScript)
            {
                int ii = 0;

                for (int i = 0; i < NAScript.Length; i++)
                {
                    PaTest.CreateHsdioMipiScriptNA(NAScript[i], Script_List, i, NAScript.Length);
                }

                // CC: Only PA will need this.
                //foreach (PaTest test in AllPATests.Values)
                //{
                //    try
                //    {
                //        ScriptCount = model2.GetPAScriptCount(test);
                //    }
                //    catch { ScriptCount = 0; }

                //    for (int k = 0; k < ScriptCount; k++)
                //    {
                //        test.CreateHsdioMipiScriptPA(PAScript[ii], Script_List, ii, PAScript.Length);
                //        ii++;
                //    }

                //}

            }
        }

        public void DoATFInit2(string configXmlPath)
        {
            if (!System.IO.File.Exists(configXmlPath))
            {
                MessageBox.Show("Unble to read OTP Test Site, abort test plan!", "Jedi", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ATFLogControl.Instance.Log(LogLevel.Error, "Unble to read OTP Test Site, abort test plan!");
                throw new Exception("Unble to read OTP Test Site, abort test plan!");
            }

            System.Xml.XmlDocument clothoConfig = new System.Xml.XmlDocument();
            clothoConfig.Load(configXmlPath);

            System.Xml.XmlNodeList clothoConfigNodes =
                clothoConfig.SelectNodes("/ATFConfiguration/ToolSection/ConfigItem");

            foreach (System.Xml.XmlNode xn in clothoConfigNodes)
            {
                if (xn.Attributes["name"].Value.ToString().Trim() == "Facilities")
                {
                    if (xn.Attributes["value"].Value.ToString().Trim().ToUpper().EndsWith("_SITE1") == true)
                    {
                        PaTest.tester_site_no = 1;
                    }
                    else if (xn.Attributes["value"].Value.ToString().Trim().EndsWith("_SITE2") == true)
                    {
                        PaTest.tester_site_no = 2;
                    }
                    else
                    {
#if (DEBUG)
                                    PaTest.tester_site_no = 1;
#else
                        ProductionLib.OTP_SiteConfigMessage otp_error = new ProductionLib.OTP_SiteConfigMessage();
                        otp_error.ShowDialog();
                        ATFLogControl.Instance.Log(LogLevel.Error, "Unble to read OTP Test Site, abort test plan!");
                        throw new Exception("Unble to read OTP Test Site, abort test plan!");
#endif
                    }

                    break;
                }
            }
        }

        public void DoATFInit3()
        {
            PaTest.PATestTime = false;
            PaTest.Site_Number = '1';
        }
    }

    public class StubInstrLibWrapper
    {

        public void ProdBestPracticeOtpEFuse()
        {
        }

        public void ProdBestPractice1()
        {
        }

        public void ProdBestPractice2()
        {
        }

        /// <summary>
        /// Required production best practices.
        /// </summary>
        public void DoAtfUnInit()
        {
        }

        public int DoATFTest2(string cmd)
        {
            return 999999;
        }

        public void DoATFTestReset()
        {
        }

        public void SetHsdioFirstScriptNA()
        {
        }

        public bool IsHsdioUseScript
        {
            get { return false; }
            set { /*HSDIO.useScript = value;*/ }
        }

        /// <summary>
        /// Required.
        /// </summary>
        public bool DoAtfInitMipiWaveformLoad(string clothoRootDir,
            Dictionary<string, int> Script_List)
        {
            string vectorBasePath = clothoRootDir + @"RFFE_vectors\";
            bool programLoadSuccess = true;
            programLoadSuccess &= HSDIO.Initialize(PaTest.SmuResources, false, "E", true); //Slave address here is actually meaningless

            foreach (KeyValuePair<string, int> Script_Test in Script_List)
            {
                programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + Script_Test.Key + ".vec", Script_Test.Key);
            }


            //for (int i = 0; i < Script.Length; i++)
            //{
            //    System.IO.File.Delete(vectorBasePath + Script[i] + ".vec");
            //}

            #region Loading of the named vectors

            // added by CheeOn 27092017 functional test
            if (System.IO.File.Exists(vectorBasePath + "\\OTP\\RX\\OTPLNA_VERIFY_Rev15.vec"))
            {
                programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\OTP\\RX\\OTPLNA_VERIFY_Rev15.vec", "RXOTPVERIFY");
            }
            else
            {
                MessageBox.Show("Error: Specified LNA OTP file does not exist at RFFE_VEctors\\OTP\\RX\\  Check revision on TCF Main Tab");
            }

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\OB61_NRZ_Full_speed_Functional_Test.vec", "FUNCTIONAL");
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\2018_OMH_4FX9A_Functional_Test_split_v08.vec", "RXFUNCTIONAL");

            //End


            // programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\OTP\\TX\\JEDI_A5B_Burn_OTP_00_01_52_01_TEST.vec", "OTP");
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\OTP\\TX\\WRITE_3_C0_A8.vec", "ROCK");

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\1C80_test2.vec", "TRIGOFF");
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\1C01_test2.vec", "TRIGON");

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\WRITE_3_2B_0F.vec", "TX2B", false, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\WRITE_E_2B_0F.vec", "RX2B", false, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\VIOOFF.vec", "VIOOFF", false, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MipiTest\\VIOON.vec", "VIOON", false, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\TEMP\\WRITE_3_10_00.vec", "TEMPENABLE", false, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\TEMP\\READ_3_10_00_100050_100065.vec", "TEMP", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\PID\\READ_TXSEORAKSAN_1D_52MHZ.vec", "TXPID", true, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\PID\\READ_RXSEORAKSAN_1D_52MHZ.vec", "RXPID", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MID\\READ_TXSEORAKSAN_1E_52MHZ.vec", "TXMID", true, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\MID\\READ_RXSEORAKSAN_1E_52MHZ.vec", "RXMID", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\USID\\READ_TXSEORAKSAN_1F_52MHZ.vec", "TXUSID", true, false);
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\USID\\READ_RXSEORAKSAN_1F_52MHZ.vec", "RXUSID", true, false);

            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E0_00_100050_100065_52MHZ.vec", "READE0", true, false);      //MFG_ID MSB
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E1_00_100050_100065_52MHZ.vec", "READE1", true, false);      //MFG_ID LSB
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E2_00_100050_100065_52MHZ.vec", "READE2", true, false);      //RevID
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E3_00_100050_100065_52MHZ.vec", "READE3", true, false);      // Module_ID MSB    FBAR FLAG = Bit#7, RF1 Flag = bit#6 
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E4_00_100050_100065_52MHZ.vec", "READE4", true, false);      //Module_ID LSB
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\READE\\READ_E_E5_00_100050_100065_52MHZ.vec", "READE5", true, false);      // lock Bit

            //OTP burn FBAR flag
            programLoadSuccess &= HSDIO.LoadVector_PowerMode(vectorBasePath + "\\UNITID\\ModuleID\\RegE5_Bit0.vec", "RegE5_Bit0", true, false); // FBAR PASS FLAG bit#0(E5)

            #endregion Loading of the named vectors

            return programLoadSuccess;
        }

        public static string[] LoadHsdioMipiWaveform(string[] Script)
        {
            string[] NAScript = new string[10000];
            string[] PAScript = new string[10000];
            int NAScript_Count = 0;
            int PAScript_Count = 0;

            for (int i = 0; i < Script.Length; i++)
            {
                string[] Split = Script[i].Split('_');
                int Split_Count = Split.Length;

                if (Split[Split_Count - 1].Contains("NA"))
                {
                    NAScript[NAScript_Count] = Script[i];
                    NAScript_Count++;
                }

                if (Split[Split_Count - 1].Contains("PA"))
                {
                    PAScript[PAScript_Count] = Script[i];
                    PAScript_Count++;
                }
                else
                {
                    int asda = 0;
                }
            }

            Array.Resize(ref NAScript, NAScript_Count);
            Array.Resize(ref PAScript, PAScript_Count);
            return NAScript;
        }

        private enum HSDIO_Define
        {
            //NI6570_1 = 0,
            //NI6570_0 = 1,
            //NI6570_2 = 2,
            //NI6570_3 = 5,
            //NI6570_4 = 4,
            //NI6570_5 = 3,

            // Cheeon modified
            NI6570_5 = 0,
            NI6570_7 = 1,
            NI6570_4 = 2,
            NI6570_10 = 5,
            NI6570_11 = 4,
            NI6570_9 = 3,
        }

        /// <summary>
        /// Required.
        /// </summary>
        public string DoAtfInit2()
        {
            HSDIO_Define[] GetNum = (HSDIO_Define[])Enum.GetValues(typeof(HSDIO_Define));
            string[] PinName_Array = new string[6]; PinName_Array[0] = "Sdata1"; PinName_Array[1] = "Sclk1"; PinName_Array[2] = "Vio1";
            PinName_Array[3] = "Sdata2"; PinName_Array[4] = "Sclk2"; PinName_Array[5] = "Vio2";
            int PinNameCount = 0;

            foreach (HSDIO_Define Get in GetNum)
            {
                //string DcPinName = kv.Key;
                string pinName = PinName_Array[PinNameCount];
                string VisaAlias = Get.ToString().Split('_')[0];
                string Chan = Get.ToString().Split('_')[1];

                try
                {
                    PaTest.SmuResources.Add(pinName, Smu.getSMU(VisaAlias, Chan, pinName, true));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SMU Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //programLoadSuccess = false;
                    return TestPlanRunConstants.RunFailureFlag + "Failed SMU Initialization";
                }
                PinNameCount++;
            }

            return String.Empty;
        }

        /// <summary>
        /// Required.
        /// </summary>
        public string DoAtfInitSmu()
        {
            List<KeyValuePair<string, string>> DCpinSmuResourceTempList =
                new List<KeyValuePair<string, string>>();
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcc", "NI4139.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbatt", "NI4143.0"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vlna", "NI4143.1"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vbiast", "NI4143.2"));
            DCpinSmuResourceTempList.Add(new KeyValuePair<string, string>("V.Vcpl", "NI4143.3"));

            foreach (KeyValuePair<string, string> kv in DCpinSmuResourceTempList)
            {
                string DcPinName = kv.Key;
                string pinName = DcPinName.Remove(0, 2);
                string VisaAlias = kv.Value.Split('.')[0];
                string Chan = kv.Value.Split('.')[1];

                try
                {
                    PaTest.SmuResources.Add(pinName, Smu.getSMU(VisaAlias, Chan, pinName, true));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SMU Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //programLoadSuccess = false;
                    return TestPlanRunConstants.RunFailureFlag + "Failed SMU Initialization";
                }
            }
            return String.Empty;

        }

        /// <summary>
        /// Required.
        /// </summary>
        public string[] DoAtfInitGenVector(List<Dictionary<string, string>> DicTestCondMipi,
            string[] Script, Dictionary<string, int> Script_List)
        {
            List<string> List_Vector = new List<string>();
            string[] result = GenVector.CustomList(DicTestCondMipi, Script, Script_List, ref List_Vector);
            return result;
        }
    }

    public class StubClothoLibAlgoWrapper
    {
        /// <summary>
        /// Required. ProdBestPractice.
        /// </summary>
        public int FailedTestCount
        {
            get { return 0; }
        }

        public bool IsHeaderFileMode
        {
            get { return true; }
        }

        public void DoATFInit1(bool isHsdioUseScript, string[] NAScript,
            Dictionary<string, int> Script_List)
        {
        }

        public void DoATFInit2(string configXmlPath)
        {
        }

        public void DoATFInit3()
        {
        }
    }

}
