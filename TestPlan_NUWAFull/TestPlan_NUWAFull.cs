using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using System.Linq;
using LibFBAR_TOPAZ.DataType;
using SParamTestCommon;
using TestPlanCommon;
using TestPlanCommon.CommonModel;
using TestPlanCommon.SParaModel;
using ToBeObsoleted;
using System.Windows.Forms;
using TestPlanCommon.PaModel;
using TCPHandlerProtocol;


#region Custom Reference Section

//using InstrLib;
using EqLib;
using TestLib;

//using TestLib_Legacy;
//using ClothoLibAlgo_Legacy;
using SnP_BuddyFileBuilder;
using MPAD_TestTimer;
using LibFBAR_TOPAZ;
using ClothoLibStandard;
using IqWaveform;
using GuCal;

#endregion Custom Reference Section

namespace TestPlan_NUWAFull
{
    public class NUWAFull : TestPlanBase, IATFTest
    {
        #region TestPlan Properties
        // S-Para models.
        private AvagoGUWrapper m_wrapper1;
        private InstrLibWrapper m_wrapper2;
        private ClothoLibAlgoWrapper m_wrapper3;
        private ProdLib1Wrapper m_wrapper4;
        private SParaProductionTestPlan m_modelTpProd;
        private CalibrationController m_modelCalInit;
        private SParaTestConditionReader m_tcfReaderSpara;
        // PA models.
        private PaTestConditionReader m_tcfReader;
        private PaEquipmentInitializer m_eqInitModel;
        private SParaEquipmentInitializer m_eqInitModel2;

        private MultiSiteTestRunner m_modelTestRunner;
        private PaProductionTestPlan m_prodTp;

        private int StopOnContinueFail1A;
        private int StopOnContinueFail2A;

        #endregion TestPlan Properties

        private SParaTestManager LibFbar;
        TCPHandlerProtocol.HontechHandler handler;
        string HandlerAddress = "2";
        string Clotho_User = "DEBUG_USER";
        byte SiteCount = 1;
        bool[] isGUCALSuccess;

        public NUWAFull()
        {
             m_tcfReader = new PaTestConditionReader();
            // S-Para models.
            m_wrapper1 = new AvagoGUWrapper();
            m_wrapper2 = new InstrLibWrapper();
            m_wrapper3 = new ClothoLibAlgoWrapper();
            m_wrapper4 = new ProdLib1Wrapper();
            m_modelCalInit = new CalibrationController();
            m_tcfReaderSpara = new SParaTestConditionReader();
            m_modelTpProd = new SParaProductionTestPlan();
            m_prodTp = new PaProductionTestPlan();

        }

        public override string DoATFInit(string args)
        {
            base.DoATFInit(args);

            StopWatchManager.Instance.IsActivated = true;
            PromptManager.Instance.IsAutoAnswer = false;
            IQ.BasePath = m_doClotho1.ClothoRootDir + @"Waveform\";
            ResultBuilder.headerFileMode = false;           

            m_modelTpState = new TestPlanStateModel();
            // TODO Switch tester Penang Seoul.
            //m_modelTpState.SetTesterSite(new PenangRf2Tester());
            //m_modelTpState.SetTesterSite(new PenangRf1Tester());
            //m_modelTpState.SetTesterSite(new SeoulRf1Tester());

            try
            {
                Clotho_User = ATFRTE.Instance.CurUserName.ToString().ToUpper();
                //MessageBox.ShowInfo(string.Format("User : {0}", Clotho_User));
            }
            catch
            {
                Clotho_User = "DEBUG_USER";
            }

            #region Check Test Site
            bool isSiteDefined = m_modelTpState.CheckTesterType2(m_doClotho1.ConfigXmlPath);
            if (isSiteDefined)
            {
                string msg = "Clotho Tester Type is not configured, abort test plan!";
                PromptManager.Instance.ShowError(msg, "Pinot");
                LoggingManager.Instance.LogError(msg);
                //throw new Exception("Clotho Tester Type is not configured, abort test plan!");
            }

            m_tcfReader.FillMainTabTester(m_modelTpState.Spara_Site);
            m_modelTpState.SetTesterType(m_tcfReader.TCF_Setting["Tester_Type"]);
            if (m_modelTpState.Pa_Site)
            {
                m_tcfReader.FillMainTab3();

                if (Clotho_User == "DEBUG_USER")
                {
                    m_tcfReader.GenerateDicLocalFile(m_tcfReader.TCF_Setting["LocalSettingFile_Path"]);
                }
            }
            m_tcfReader.TCF_Setting["TesterID"] = m_modelTpState.GetTesterId(m_doClotho1.ConfigXmlPath);

            if (m_modelTpState.Pa_Site && m_modelTpState.Spara_Site)
            {
                m_modelTpState.SetTesterSite(new Tester.SeoulRf1Rf2Tester());
            }
            else
            {
                if (m_modelTpState.Spara_Site)
                {
                    m_modelTpState.SetTesterSite(new Tester.SeoulRf2Tester());
                }
                if (m_modelTpState.Pa_Site)
                {
                    m_modelTpState.SetTesterSite(new Tester.PgNUWARf1Tester());
                }
            }
            #endregion Check Test Site
           

            string[] EnabledSiteStrArray = new string[] { "0" };
            if (Clotho_User == "DEBUG_USER")
            {
                SiteCount = Convert.ToByte(m_tcfReader.DicLocalfile["TEST_CONFIG"]["TEST_SITE_COUNT"]);
                EnabledSiteStrArray = m_tcfReader.DicLocalfile["TEST_CONFIG"]["ENABLED_SITES"].Split(',');
            }
            else
            {
                SiteCount = Convert.ToByte(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_Handler_MaxSitesNum, ""));
                if (SiteCount == 2) { EnabledSiteStrArray = new string[] { "0", "1" }; }
            }

            StopWatchManager.Instance.NumSites = SiteCount;            
            Eq.SetNumSites(SiteCount);
            
            Eq.EnabledSites = new byte[EnabledSiteStrArray.Length];
            for( int site= 0;site < EnabledSiteStrArray.Length; site++)
            {
                Eq.EnabledSites[site] = Convert.ToByte(EnabledSiteStrArray[site]);
            }

            ResultBuilder.DuplicatedModuleID = new bool[Eq.NumSites];
            ResultBuilder.DuplicatedModuleIDCtr = new int[Eq.NumSites];

            m_prodTp.ShowInputGui(m_tcfReader.TCF_Setting["Sample_Version"], m_tcfReader.TCF_Setting["ADD_SUBLOTID"], m_tcfReader.TCF_Setting["ADD_DEVICEID"],
                m_tcfReader.TCF_Setting["WebQueryValidation"], m_tcfReader.TCF_Setting["WebServerURL"], m_tcfReader.TCF_Setting["GU_EngineeringMode"]);
           

            m_eqInitModel = new PaEquipmentInitializer();
            m_eqInitModel.SetTester(m_modelTpState.TesterSite);
            m_eqInitModel2 = new SParaEquipmentInitializer();
            m_eqInitModel2.SetTester(m_modelTpState.TesterSite);

            if (m_modelTpState.Spara_Site)
            {
                m_eqInitModel2.InitializeHandler(m_tcfReader.TCF_Setting["Handler_Type"],
                m_modelTpState.TesterSite.GetHandlerName());
            }

            if (m_modelTpState.Pa_Site)
            {
                m_eqInitModel.InitializeHandler(m_tcfReader.TCF_Setting["Handler_Type"],
                    m_tcfReader.TCF_Setting["Handler_Type"]);
            }

            #region Retrieve Handler Information

            if ((m_tcfReader.TCF_Setting["HandlerInfo"] == "TRUE") && (ATFRTE.Instance.CurUserName != null))
            {
                HandlerAddress = ATFRTE.Instance.HandlerAddress;
            }

            if (!ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt"))
            {
                if (m_tcfReader.TCF_Setting["Handler_Type"] == "AUTO")
                {
                    try
                    {
                        if (handler != null)
                            handler.Disconnect();

                        if (HandlerAddress != null)
                        {
                            handler = new HontechHandler(Convert.ToInt32(HandlerAddress));
                            handler.Connect();
                            HandlerForce hli = handler.ContactForceQuery();
                        }
                    }
                    catch { }
                }
            }

            #endregion

            StopOnContinueFail1A = Convert.ToInt16(m_tcfReader.TCF_Setting["StopOnContinueFail_1A"]);
            StopOnContinueFail2A = Convert.ToInt16(m_tcfReader.TCF_Setting["StopOnContinueFail_2A"]);

            m_modelCalInit.ReadSmPad(m_wrapper4);


            //HSDIO.useScript = false;
            EqHSDIO.isVioTxPpmu = true;
            EqHSDIO.forceQCVectorRegen = Convert.ToBoolean(m_tcfReader.TCF_Setting["ForceQCVectorToDigiPatternRegeneration"]);
            EqHSDIO.ADJUST_BusIDCapTuningInHex = m_tcfReader.TCF_Setting["ADJUST_BusIDCapTuningInHex"] != ""? m_tcfReader.TCF_Setting["ADJUST_BusIDCapTuningInHex"] : "0F";

            #region Custom Init Coding Section

            //////////////////////////////////////////////////////////////////////////////////
            // ----------- ONLY provide your Custom Init Coding here --------------- //

            try
            {
                m_wrapper4.Wrapper2.InitializeVar();

                #region ProductTag

                //string productTag = m_wrapper1.DoAtfInitProductTag(m_doClotho1.ClothoRootDir);
                //m_modelTpProd.SetProductTag(productTag);

                #endregion ProductTag

                if (m_modelTpState.Pa_Site)
                {
                    bool isSuccess = InitializePaTest(m_eqInitModel);                    
                    if (!isSuccess) return TestPlanRunConstants.RunFailureFlag;
                }

                if (m_modelTpState.Spara_Site)
                {
                    InitializeSParaTest(m_eqInitModel2);
                    if (!m_modelTpState.programLoadSuccess)
                    {
                        return TestPlanRunConstants.RunFailureFlag;
                    }
                }
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
                m_modelTpState.programLoadSuccess = false;
                return TestPlanRunConstants.RunFailureFlag + ex.Message;
            }
            // ----------- END of Custom Init Coding --------------- //
            //////////////////////////////////////////////////////////////////////////////////

            #endregion Custom Init Coding Section

            //StopWatchManager.Instance.Stop();

            //string TestTimeDir = "C:\\Avago.ATF.Common.x64\\Production\\TestTime\\";
            //for (byte site = 0; site < Eq.NumSites; site++)
            //{
            //    StopWatchManager.Instance.SaveToFile(TestTimeDir + string.Format("DoATFInit_TestTimes_Site{0}.txt",site), "DoATFInit_Times", site);
            //    StopWatchManager.Instance.Reset(site);
            //}

            //m_prodTp.LockClotho();

            // NPI Test Time Dashboard.
            if (m_modelTpState.Spara_Site)
            {
                m_modelTpProd.TestTimeLogController.Initialize(new TestPlanStateModel(),
                m_doClotho1.ConfigXmlPath, m_tcfReaderSpara.DataObject.DicTestCondTempNA);

            }

            if (m_modelTpState.Pa_Site)
            {
                m_prodTp.TestTimeLogController.Initialize(m_modelTpState, m_doClotho1.ConfigXmlPath,
                    m_tcfReader.DicTestCondTemp, m_tcfReader.DcResourceSheet);
            }

            return String.Empty;
        }

        public string DoATFUnInit(string args)
        {
            StopWatchManager.Instance.Start(0);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Enter DoATFUnInit: {0}\n", args);

            #region Custom UnInit Coding Section

            //////////////////////////////////////////////////////////////////////////////////
            // ----------- ONLY provide your Custom UnInit Coding here --------------- //

            var processes = from p in System.Diagnostics.Process.GetProcessesByName("EXCEL") select p;

            foreach (var process in processes)
            {
                if (process.MainWindowTitle == "")
                    process.Kill();
            }

            m_modelTpState.SetUnloaded();

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                DPAT.Clear(site);

                if (Eq.Site[site].HSDIO != null)
                {
                    Eq.Site[site].HSDIO.Close();
                }
                if (Eq.Site[site].RF != null)
                {
                    Eq.Site[site].RF.close();
                }

                if (m_modelTestRunner.MachineData[site] != null)
                {
                    m_modelTestRunner.MachineData[site].MQTTExexLotInfo(false, false);
                    m_modelTestRunner.MachineData[site].client_source.Disconnect();
                }
            }

            //foreach (string pinName in PaTest.SmuResources.Keys)
            //{
            //    //if (HSDIO.IsMipiChannel(pinName)) continue;
            //    PaTest.SmuResources[pinName].CloseSession();
            //}

            #region FBAR Unload

            if (LibFbar == null)
            {
                m_modelTpProd.DoAtfUnInit(String.Empty);
                return sb.ToString();
            }
            
            m_modelTpProd.DoAtfUnInit(String.Empty);
            LibFbar.UnInit();

            #endregion FBAR Unload

            //ProgressiveZipDataObject pzDo = new ProgressiveZipDataObject();
            //pzDo.IsRunning = m_wrapper1.IsRunning;
            //pzDo.ModelcAlgorithm = LibFbar;
            //pzDo.ActiveDir = m_modelTpProd.ActiveDirectory;
            //m_wrapper4.Wrapper2.RunProgressiveZip(pzDo);

            #region Calibration Due Date
            if (File.Exists(@"C:\Program Files\PXIe Calibration Log\PXIe Calibration Log.exe"))
            {
                System.Diagnostics.Process.Start(@"C:\Program Files\PXIe Calibration Log\PXIe Calibration Log.exe");
            }
            #endregion

            // ----------- END of Custom UnInit Coding --------------- //
            //////////////////////////////////////////////////////////////////////////////////

            #endregion Custom UnInit Coding Section

            StopWatchManager.Instance.Stop(0);
            return sb.ToString();
        } //DoATFUnInit

        public string DoATFLot(string args)
        {
            StopWatchManager.Instance.Start(0);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Enter DoATFLot: {0}\n", args);

            #region Custom CloseLot Coding Section

            //////////////////////////////////////////////////////////////////////////////////
            // ----------- ONLY provide your Custom CloseLot Coding here --------------- //

            if (LibFbar != null)
            {
                LibFbar.FileManager.m_isNotFirstRun = false;
            }

            //m_prodTp.ShowInputGui(m_tcfReader.TCF_Setting["Sample_Version"], m_tcfReader.TCF_Setting["ADD_SUBLOTID"], m_tcfReader.TCF_Setting["ADD_DEVICEID"]);
            // ----------- END of Custom CloseLot Coding --------------- //
            //////////////////////////////////////////////////////////////////////////////////

            #endregion Custom CloseLot Coding Section

            StopWatchManager.Instance.Stop(0);
            return sb.ToString();
        } //DoATFLot

        private int PassCount;
        private int FailCount;

        public ATFReturnResult DoATFTest(string args)
        {
            ATFReturnResult results = new ATFReturnResult();
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                StopWatchManager.Instance.Start("TIME_DoATFTest", site);
            }

            #region StoponcontinueFail Part 1 of 2

            string SublotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "");

            if (SublotId.Contains("1A") || SublotId.Contains("1B") || SublotId.Contains("1C"))
            {
                if ((StopOnContinueFail1A != 0) && (FailCount == StopOnContinueFail1A))
                {
                    FailCount = 0;
                    ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, "Unit under testing Continue Failures more than " + StopOnContinueFail1A + " times!!!");
                    System.Windows.Forms.MessageBox.Show("Unit under testing Continue Failures more than " + StopOnContinueFail1A + " times!!!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_modelTpState.programLoadSuccess = false;
                    //return new ATFReturnResult(TestPlanRunConstants.RunSkipFlag);
                }
            }
            else
            {
                if ((StopOnContinueFail2A != 0) && (FailCount == StopOnContinueFail2A))
                {
                    FailCount = 0;
                    ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, "Unit under testing Continue Failures more than " + StopOnContinueFail2A + " times!!!");
                    System.Windows.Forms.MessageBox.Show("Unit under testing Continue Failures more than " + StopOnContinueFail2A + " times!!!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_modelTpState.programLoadSuccess = false;
                    //return new ATFReturnResult(TestPlanRunConstants.RunSkipFlag);
                }
            }

            if (!m_modelTpState.programLoadSuccess)
            {
                System.Windows.Forms.MessageBox.Show("Program was not loaded successfully.\nPlease resolve errors and reload program.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string err = "Program was not loaded successfully.\nPlease resolve errors and reload program";
                return new ATFReturnResult(err);
            }

            #endregion

            #region Custom Test Coding Section

            try
            {
                if (!m_modelTpState.programLoadSuccess)
                {
                    PromptManager.Instance.ShowError("Program was not loaded successfully.\nPlease resolve errors and reload program.");
                    return results;
                }
                if (m_modelTpState.programUnloaded)
                {
                    PromptManager.Instance.ShowError("Program was loaded without shutting down Clotho.\nPlease shut down Clotho then reload.");
                    return results;
                }
                string meup = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_ASSEMBLY_ID, "");
                // sw2 is used to benchmark actual DoATFTest time against StopWatchManager's.
                //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_ASSEMBLY_ID, "MFG" + meup);
                if (meup.Contains("MFG"))
                {
                    ATFCrossDomainWrapper.StoreStringToCache("MFG", meup);
                }
                else
                {
                    ATFCrossDomainWrapper.StoreStringToCache("MFG", "MFG" + meup);
                }

                
                bool reloadPaTestsFromTcf = false;

                if (reloadPaTestsFromTcf)
                {
                    PaEquipmentInitializer eqInitModel = new PaEquipmentInitializer();
                    LoadPaTestFromTcf(eqInitModel.Digital_Definitions_Part_Specific, eqInitModel.EqTriggerArray);
                    m_modelTestRunner.InitializeAllPaTests();
                }

                //if (!PaTest.headerFileMode) GU.DoTest_beforeCustomCode();

                for (int x = 1; x <= 1; x++)
                {
                    for(byte site = 0; site < Eq.NumSites; site++)
                    {
                        ResultBuilder.Clear(site);
                    }

                    // PA Test execution section.
                    if (m_modelTpState.Pa_Site && !m_tcfReaderSpara.DataObject.ENA_Cal_Enable)
                    {
                        RunPaTest();
                        m_modelTpState.programLoadSuccess =
                            m_prodTp.DetectDoubleUnitSoftware(m_tcfReader.TCF_Setting["PauseTestOnDuplicateModuleID"]);
                        m_modelTpState.SetLoadFail(m_modelTpState.programLoadSuccess);
                    }

                    #region Spara test
                    // SPara Test execution section.
                    if (m_modelTpState.Spara_Site)
                    {
                        m_modelTpState.CurrentTestResultFileName = m_modelTpProd.SetTestResult("AFEM-8300-AP1");

                        if (m_tcfReaderSpara.DataObject.ENA_Cal_Enable)
                        {
                            m_modelCalInit.Calibrate2(LibFbar.CalibrationModel, results);
                            ATFResultBuilder.AddResult(ref results, "M_TIME_FbarTest", "ms", 5.0f);
                            //ResultBuilder.AddResult(0, "M_TIME_FbarTest", "ms", 0f/*fbartestTime*/);
                            return results;
                        }

                        RunSParaTest(results);                     
                    } //if (Spara_Site) 
                    #endregion Spara test


                    m_wrapper1.DoAtfTestMoveNext(results);
                } //for (GU.runLoop = 1; GU.runLoop <= GU.numRunLoops; GU.runLoop++)

                

            } // Try
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError("Error happened during the runtest at", ex);
                //return new ATFReturnResult(TestPlanRunConstants.RunFailureFlag);
            }

            #endregion Custom Test Coding Section

            #region Retrieve Handler Information
            string Handler_SiteNo = "0";
            string keyArm = "";
            string Handler_ArmNo = "0";
            string keyForce = "";
            string Handler_WorkpressForce = "0";
            string keyEPValue = "";
            string Handler_EPValue = "0";
            string keyContactX = "";
            string ContactX = "0";
            string keyContactY = "";
            string ContactY = "0";
            string keyContactZ = "";
            string ContactZ = "0";
            string keyEPOffset = "";
            string EPOffset = "0";
            string keyTrayCoordinateX = "";
            string TrayCoordinateX = "0";
            string keyTrayCoordinateY = "";
            string TrayCoordinateY = "0";
            string keyTouchDownCount = "";
            string TouchDownCount = "0";
            string workPressNo = "999";

            if (!ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt"))
            {
                if (m_tcfReader.TCF_Setting["HandlerInfo"] == "TRUE")
                {
                    try
                    {
                        HandlerForce hli = handler.ContactForceQuery();

                        //Handler_SiteNo = hli.SiteNo.ToString();
                        Handler_SiteNo = HandlerAddress;    // Do not use hli.SiteNo, it is not consistent readback from handler
                        Handler_ArmNo = hli.ArmNo.ToString();
                        Handler_WorkpressForce = hli.PlungerForce.ToString();
                        Handler_EPValue = hli.EPVoltage.ToString();
                    }
                    catch
                    {
                        ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, "Handler TCPIP connection not available");
                    }
                }
                foreach (byte site in ResultBuilder.ValidSites)
                {
                    ResultBuilder.AddResult(site, "M_Handler-SiteNo", "", Convert.ToDouble(Handler_SiteNo));
                    ResultBuilder.AddResult(site, "M_Handler-ArmNo", "", Convert.ToDouble(Handler_ArmNo));
                    ResultBuilder.AddResult(site, "M_Handler-WorkpressForce", "kgF", Convert.ToDouble(Handler_WorkpressForce));
                    ResultBuilder.AddResult(site, "M_Handler-EPvalue", "V", Convert.ToDouble(Handler_EPValue));

                    //Add Result for Workpress Number
                    if (SiteCount == 2)
                    {
                        workPressNo = HandlerInfo.QuadSiteWorkpressNo[Convert.ToInt16(Handler_SiteNo) - 1][site][Handler_ArmNo];
                    }
                    else if (SiteCount == 1)
                    {
                        workPressNo = HandlerInfo.DualSiteWorkpressNo[Convert.ToInt16(Handler_SiteNo) - 1][site][Handler_ArmNo];
                    }
                    ResultBuilder.AddResult(site, "M_Handler-WorkPress", "", Convert.ToDouble(workPressNo));
                }
            }
            else
            {
                Handler_SiteNo = HandlerAddress;

                foreach (byte site in ResultBuilder.ValidSites)
                {
                    ResultBuilder.AddResult(site, "M_Handler-SiteNo", "", Convert.ToDouble(Handler_SiteNo));
                    keyArm = string.Format("ARM_NO_{0}", site + 1);
                    Handler_ArmNo = ATFCrossDomainWrapper.GetStringFromCache(keyArm, "0");
                    ResultBuilder.AddResult(site, "M_Handler-ArmNo", "", Convert.ToDouble(Handler_ArmNo));
                    keyForce = string.Format("PLUNGFORCE_{0}", site + 1);
                    Handler_WorkpressForce = ATFCrossDomainWrapper.GetStringFromCache(keyForce, "0");
                    ResultBuilder.AddResult(site, "M_Handler-WorkpressForce", "kgF", Convert.ToDouble(Handler_WorkpressForce));
                    keyEPValue = string.Format("EP_VALUE_{0}", site + 1);
                    Handler_EPValue = ATFCrossDomainWrapper.GetStringFromCache(keyEPValue, "0");
                    ResultBuilder.AddResult(site, "M_Handler-EPvalue", "V", Convert.ToDouble(Handler_EPValue));
                    keyEPOffset = string.Format("EPOFFSET_{0}", site + 1);
                    EPOffset = ATFCrossDomainWrapper.GetStringFromCache(keyEPOffset, "0");
                    ResultBuilder.AddResult(site, "M_Handler-EPoffset", "", Convert.ToDouble(EPOffset));
                    keyContactX = string.Format("CONTACTX_{0}", site + 1);
                    ContactX = ATFCrossDomainWrapper.GetStringFromCache(keyContactX, "0");
                    ResultBuilder.AddResult(site, "M_Handler-ContactX", "", Convert.ToDouble(ContactX));
                    keyContactY = string.Format("CONTACTY_{0}", site + 1);
                    ContactY = ATFCrossDomainWrapper.GetStringFromCache(keyContactY, "0");
                    ResultBuilder.AddResult(site, "M_Handler-ContactY", "", Convert.ToDouble(ContactY));
                    keyContactZ = string.Format("CONTACTZ_{0}", site + 1);
                    ContactZ = ATFCrossDomainWrapper.GetStringFromCache(keyContactZ, "0");
                    ResultBuilder.AddResult(site, "M_Handler-ContactZ", "", Convert.ToDouble(ContactZ));
                    keyTrayCoordinateX = string.Format("TRAYCOORDINATE_X_{0}", site + 1);
                    TrayCoordinateX = ATFCrossDomainWrapper.GetStringFromCache(keyTrayCoordinateX, "0");
                    ResultBuilder.AddResult(site, "M_Handler-TrayCoordinateX", "", Convert.ToDouble(TrayCoordinateX));
                    keyTrayCoordinateY = string.Format("TRAYCOORDINATE_Y_{0}", site + 1);
                    TrayCoordinateY = ATFCrossDomainWrapper.GetStringFromCache(keyTrayCoordinateY, "0");
                    ResultBuilder.AddResult(site, "M_Handler-TrayCoordinateY", "", Convert.ToDouble(TrayCoordinateY));
                    keyTouchDownCount = string.Format("TOUCHDOWNCOUNT_{0}", site + 1);
                    TouchDownCount = ATFCrossDomainWrapper.GetStringFromCache(keyTouchDownCount, "0");
                    ResultBuilder.AddResult(site, "M_Handler-TouchDownCount", "", Convert.ToDouble(TouchDownCount));

                    //Add Result for Workpress Number
                    if (SiteCount == 2)
                    {
                        workPressNo = HandlerInfo.QuadSiteWorkpressNo[Convert.ToInt16(Handler_SiteNo) - 1][site][Handler_ArmNo];
                    }
                    else if (SiteCount == 1)
                    {
                        workPressNo = HandlerInfo.DualSiteWorkpressNo[Convert.ToInt16(Handler_SiteNo) - 1][site][Handler_ArmNo];
                    }
                    ResultBuilder.AddResult(site, "M_Handler-WorkPress", "", Convert.ToDouble(workPressNo));
                }
            }

            #endregion

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                StopWatchManager.Instance.Stop("TIME_DoATFTest", site);
            }

            //m_prodTp.LockClotho();
            // NPI Test time dashboard. Used for production.
            if (m_modelTpState.Spara_Site)
            {
                m_modelTpProd.TestTimeLogController.Save();
            }

            if (m_modelTpState.Pa_Site)
            {
                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    m_prodTp.TestTimeLogController.Save(site, Eq.NumSites);
                    // Not used in production. To be commented out for production mode.
                    // StopWatchManager.Instance.SaveToFile(@"C:\Temp\PATestTime-V2.txt", "Pa Test V2");
                    StopWatchManager.Instance.Clear(site);
                }
            }

            

            TestPlanCommon.ToBeObsoleted.TestTimeFile.PostAfterRunTest();
            TestPlanCommon.ToBeObsoleted.TestTimeFile.Close();

            #region StoponcontinueFail Part 2 of 2 + Webservice 2.0
            foreach (byte site in ResultBuilder.ValidSites)
            {
                //Web Service 2.0
                OtpTest.Webservice2DIDflag[site] = true;

                if (OtpTest.mismatch2DID[site] == true)
                {
                    LoggingManager.Instance.LogInfoTestPlan("def.mismatch2DID == true and value: " + OtpTest.mismatch2DIDerr[site]);
                    ResultBuilder.AddResult(site, "M_TEST-CHECK2DID-STATUS", "NA", Convert.ToDouble(OtpTest.mismatch2DIDerr[site]));
                }
                else
                {
                    if (OtpTest.WebByPass == true)
                    {
                        ResultBuilder.AddResult(site, "M_TEST-CHECK2DID-STATUS", "NA", -1);
                    }
                    else
                    {
                        ResultBuilder.AddResult(site, "M_TEST-CHECK2DID-STATUS", "NA", 0);
                    }
                    LoggingManager.Instance.LogInfoTestPlan("def.mismatch2DID == false");
                }

                try
                {
                    if (OtpTest.Web2didData2 == null || OtpTest.Web2didData2 == "")
                    {
                        ResultBuilder.AddResult(site, "M_TEST-CHECK2DID-DATA", "NA", 999);

                    }
                    else
                    {
                        ResultBuilder.AddResult(site, "M_TEST-CHECK2DID-DATA", "NA", Convert.ToDouble(OtpTest.Web2didData2));

                    }
                }
                catch
                {
                    ResultBuilder.AddResult(site, "M_TEST-CHECK2DID-DATA", "NA", 999);
                }

                if (ResultBuilder.FailedTests[site].Count > 0)
                {
                    FailCount++;
                    PassCount = FailCount * -1;
                    ResultBuilder.AddResult(site, "M_CONTINUOUS-PASSFAIL-COUNT", "NA", Convert.ToDouble(PassCount));
                }
                else
                {
                    FailCount = 0;
                    ResultBuilder.AddResult(site, "M_CONTINUOUS-PASSFAIL-COUNT", "NA", Convert.ToDouble(FailCount));
                }
            }
            #endregion

            // This line should not be reached.
            return ResultBuilder.FormatResultsForReturnToClotho();
        }

        private void InitializeSParaTest(SParaEquipmentInitializer eqInitModel)
        {
            string SheetName = "Condition_FBAR";

            Tuple<bool, string, string[,]> NaSheetContents =
                ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(SheetName, 1, 1, 7000, 300);

            // Lib_Var.DCpinSmuResourceTempList = new List<KeyValuePair<string, string>>();

            if (NaSheetContents.Item1 == false)
            {
                PromptManager.Instance.ShowError(
                    "Error reading Excel Range", NaSheetContents.Item2, "");
                m_modelTpState.SetLoadFail();
            }

            try
            {
                m_tcfReaderSpara.FillRow(NaSheetContents);
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
            }

            // Pinot did not call this.
            //string result2 = m_wrapper2.DoAtfInit2();
            //if (!String.IsNullOrEmpty(result2))
            //{
            //    {
            //        return result2;
            //    }
            //}

            #region Load Exel Na headersequence

            Tuple<bool, string, string[,]> NaSheetContents2;

            try
            {
                SheetName = "HeaderSequence";
                NaSheetContents2 = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(SheetName, 1, 1, 100, 2);

                if (NaSheetContents2.Item1 == false)
                {
                    PromptManager.Instance.ShowError("Error reading Excel Range", NaSheetContents2.Item2);
                    m_modelTpState.SetLoadFail();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //ChoonChin - Modified for 6ports 'FULL' to 'RF2'
            string tempFbarTesterName = m_tcfReader.TCF_Setting["Tester_Type"] == "SPARA"
                ? "RF2"
                : m_tcfReader.TCF_Setting["Tester_Type"];

            #endregion Load Exel Na headersequence

            m_tcfReaderSpara.FillEnaState();

            #region Copy statefile to Network Analyzer folder

            //Copy statefile from testplan folder to NA folder is statefile is new, skip if already exist
            try
            {
                string StateFileFolderPath = string.Format(@"C:\Users\Public\Documents\Network Analyzer\{0}", m_tcfReaderSpara.DataObject.EnaStateFile);
                string NewStateFilePath = string.Format("{0}FileNeeded\\{1}", m_doClotho1.ClothoRootDir, m_tcfReaderSpara.DataObject.EnaStateFile);

                if (!File.Exists(StateFileFolderPath))
                {
                    File.Copy(NewStateFilePath, StateFileFolderPath);
                    string msg = "New statefile copied to " + StateFileFolderPath;
                    LoggingManager.Instance.LogInfo(msg);
                }
                else
                {
                    string msg = "State file exist at " + StateFileFolderPath;
                    LoggingManager.Instance.LogInfo(msg);
                }
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
                m_modelTpState.SetLoadFail();
                return;
            }

            #endregion Copy statefile to Network Analyzer folder

            //DC & MIPI
            if (!m_modelTpState.Pa_Site)
            {
                try
                {
                    eqInitModel.InitializeSmu();
                }
                catch (Exception ex)
                {
                    PromptManager.Instance.ShowError("SMU Initialization", ex);
                    //programLoadSuccess = false;
                    return;
                }
            }

            if (!m_modelTpState.Pa_Site)
            {

                try
                {
                    #region Initialize Digital

                    bool isSuccess2 = eqInitModel.InitializeHSDIO();
                    m_modelTpState.SetLoadFail(isSuccess2);

                    //used to Bin out parts from a list in the TCF
                    PaProductionTestPlan prodTp = new PaProductionTestPlan();
                    isSuccess2 = prodTp.LoadModuleIDSelectList2();
                    m_modelTpState.SetLoadFail(isSuccess2);

                    #endregion Initialize Digital

                    eqInitModel.InitializeSwitchMatrix(false);
                }
                catch (Exception e)
                {
                    PromptManager.Instance.ShowInfo("Yup, it's GenVector");
                }
            }

            StopWatchManager.Instance.Start("FBAR_Test_Loading",0);

            #region FBAR test loading

            LibFbar = new SParaTestManager();
            LibFbar.TcfHeaderEntries = SParaTestConditionReader.GetSheetHeaderSequence(NaSheetContents2);
            PinotRF2 project = new PinotRF2();
            LibFbar.ProjectSpecificDataObject = GetProjectSpecificFactor(project);

            m_modelTpProd.Reset();
            
            #region DIVA Instrument checking

            //For DIVA- to be changed to better method later
            //string ConfigFilePath = @"C:\Users\Public\Documents\Network Analyzer\VnaConfig.txt";
            //try
            //{
            //    if (File.Exists(ConfigFilePath))
            //    {
            //        //Read
            //        StreamReader dSR = new StreamReader(ConfigFilePath);
            //        string RS = dSR.ReadLine();
            //        string[] aRS = RS.Split('=');

            //        if (aRS[1].ToUpper().Contains("TRUE"))
            //        {
            //            LibFbar.isDivaInstrument = true;
            //        }
            //        else
            //            LibFbar.isDivaInstrument = false;

            //        dSR.Close();
            //    }
            //    else
            //    {
            //        //Not DIVA
            //        LibFbar.isDivaInstrument = false;
            //    }
            //}
            //catch
            //{
            //    //Not DIVA
            //    LibFbar.isDivaInstrument = false;
            //}

            #endregion DIVA Instrument checking
            
            try
            {
                //LibFbar.parse_ENA_IO = myENA;
                SParaTestConditionFactory tcfData = m_tcfReaderSpara.DataObject;
                s_SNPFile sf = new s_SNPFile();
                sf.ENASNPFileOutput_Enable = tcfData.EnaStateFileEnable;
                sf.FileOutput_Enable = tcfData.TraceFileEnable;
                sf.FileOutput_Path = m_modelTpProd.ActiveDirectory;
                sf.SNPFileOutput_Path = m_modelTpProd.SNP_Files_Dir;
                sf.FileOutput_FileName = "FEM";
                sf.FileOuuput_Count = tcfData.TraceFileOutput_Count;
                LibFbar.FileManager.InitializeSnpFile(sf);

                LibFbar.TcfConditionFbarTab = tcfData.DicTestCondTempNA;
                Lib_Var.StateFileGenerationFlag = false; //Statefile generation flag cheeon 18-July-2017
                if (m_modelTpProd.FBAR_Test)
                {
                    Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRange(4, 1, 1, 500, 100);
                    LibFbar.TcfConditionFbarTab2 = NaSheetContents;
                    TcfSheetTrace sheet1 = new TcfSheetTrace("Trace", 500, 100);
                    TcfSheetSegmentTable sheet2 = new TcfSheetSegmentTable("Segment", 3000, 20);
                    LibFbar.Initialization(sheet1, sheet2, TempTrace, tcfData.EnaStateFile);
                }

                //  ClothoLibStandard.Lib_Var.ºSJNISw_Enable = true;
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError("No ENA detected, please check ENA connection", ex);
                LogToLogServiceAndFile(LogLevel.Error, ex.ToString());
                m_modelTpProd.FBAR_Test = false;
                // return TestPlanRunConstants.RunFailureFlag + "Failed FBAR Test Loading";    //Temporary disabled for PCB AutoCal debug only
            }

            #endregion FBAR test loading

            StopWatchManager.Instance.Stop("FBAR_Test_Loading",0);

            #region Split test 2nd Switch Matrix

            // CCT :Pinot does not run this.
            /*m_wrapper4.Initialize();
                    LibFBAR_TOPAZ.cFBAR.SwitchesDictionary_Global =
                        m_wrapper4.SwitchesDictionary1; //Passing it to cFBAR because of NF switching
                    //At this point, all the switches are ready to use.

                    #region Switch Definitions per Band

                    m_wrapper4.SetSwitchDefinitionPerBand();
                    m_wrapper4.SetSwitchMatrixNF();
                    m_wrapper4.SetSwitchMatrixObsolete();

                    #endregion Switch Definitions per Band
            */
            #endregion Split test 2nd Switch Matrix

            Calibrate(LibFbar.CalibrationModel, m_wrapper4);

            // Cal may override ena cal enable.
            m_tcfReaderSpara.DataObject.ENA_Cal_Enable = m_modelCalInit.ENA_Cal_Enable;

            //if (!ENA_Cal_Enable || ENA_Cal_Enable) //skip prod GUI and Icc Cal if auto subcal needed
            if (!m_tcfReaderSpara.DataObject.ENA_Cal_Enable) //skip prod GUI and Icc Cal if auto subcal needed //Original
            {
#if (DEBUG)
                    PaProductionTestPlan tp = new PaProductionTestPlan();
                    tp.ShowInputGui();
#endif
            }

            GuCalibrationModel gu = new GuCalibrationModel();
            string destCFpath =
                ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");
            int mustIccGuCalCached = ATFCrossDomainWrapper.GetIntFromCache(PublishTags.PUBTAG_MUST_IccGuCal, -1);
            string isEngineeringMode = m_tcfReader.TCF_Setting["GU_EngineeringMode"];
            string productTagName = Path.GetFileName(destCFpath).ToUpper().Replace(".CSV", ""); // CF
            gu.SetTesterType(m_tcfReader.TCF_Setting["Tester_Type"]);
            bool[] isSuccess = gu.GuCalibration(this, destCFpath, mustIccGuCalCached,
                isEngineeringMode, productTagName);
            // Note: Not &= but set directly.
            m_modelTpState.programLoadSuccess = isSuccess[0];
#if false

            #region Icc Cal

                        //string ProductTagName = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "").ToUpper();

                        //GU.DoInit_afterCustomCode(0, 1, 0, false, false, ProductTagName, @"\\10.50.10.35\avago\ZDBFolder\zDBData");    //  sanjose: @"\\rds35\zdbdatasj\zDBData"   asekr : @"\\10.50.10.35\avago\ZDBFolder\zDBData"

            #region Init Independent Cal

                        m_wrapper4.Wrapper2.CreateSplitTestResultDatabase("");

            #endregion Init Independent Cal

                        if (m_tcfReader.DataObject.ENA_Cal_Enable)
                        {
                            m_wrapper1.SetEngineering();
                        }

                        string result = m_wrapper1.DoAtfInitAfterCustomCode();
                        if (!String.IsNullOrEmpty(result))
                        {
                            {
                                return result;
                            }
                        }

            #endregion Icc Cal

#endif
        }

        private bool InitializePaTest(PaEquipmentInitializer eqInitModel)
        {
            m_modelTestRunner = new MultiSiteTestRunner();
            m_modelTestRunner.InitializeSiteStopwatches();

            TestPlanCommon.ToBeObsoleted.TestTimeFile.enableTestTimeRecord =
                m_tcfReader.TCF_Setting["SaveTestTime"].ToUpper() == "TRUE" ? true : false;

            bool isMagicBox = m_prodTp.InitializeMagicBox(true); // MagicBox detection does not support multi-site yet 16-Oct-2018 (JJ Low)

            eqInitModel.InitializeSwitchMatrix(isMagicBox); // 16-Oct-2018 (JJ Low)

            eqInitModel.InitializeRF();

            CableCalibrationModel ccm = new CableCalibrationModel();
            bool isSuccess = ccm.CableCalibration(m_doClotho1.ClothoRootDir, "NUWA", isMagicBox);
            m_modelTpState.SetLoadFail(isSuccess);

            eqInitModel.InitializeChassis();

            //DefineNsTests();

            DefineCustomPowerMeasurements();

            LoadPaTestFromTcf(eqInitModel.Digital_Definitions_Part_Specific, eqInitModel.EqTriggerArray);
            //LoadNaTestFromTcf();

            if (ResultBuilder.headerFileMode) return false;

            if (!m_modelTpState.programLoadSuccess) return false;

            ClothoLibAlgo.Dictionary.Ordered<string, string[]> dcResourceList =
                m_tcfReader.GetDcResourceDefinitions();
            ValidationDataObject vdo = eqInitModel.InitializeDC(dcResourceList);
            m_modelTpState.SetLoadFail(vdo);
            if (!m_modelTpState.programLoadSuccess) return false;

            #region Initialize Digital

            isSuccess = eqInitModel.InitializeHSDIO();
            m_modelTpState.SetLoadFail(isSuccess);

            Dictionary<string, string> TXQCVector = new Dictionary<string, string> { };
            Dictionary<string, string> RXQCVector = new Dictionary<string, string> { };

            foreach(var item in m_tcfReader.TCF_Setting)
            {
                if (item.Key.StartsWith("#")) { continue; }
                if (item.Key.Contains("RFFE_Vectors_QCTest_TXQC"))
                {
                    TXQCVector.Add(item.Key, item.Value);
                }

                if (item.Key.Contains("RFFE_Vectors_QCTest_RXQC"))
                {
                    RXQCVector.Add(item.Key, item.Value);
                }
            }
            StopWatchManager.Instance.Start("InitHSDIO", 0);
            isSuccess = eqInitModel.LoadVector(
                m_doClotho1.ClothoRootDir, m_tcfReader.TCF_Setting["CMOS_DIE_TYPE"], m_tcfReader.TCF_Setting["Sample_Version"], TXQCVector, RXQCVector);
            m_modelTpState.SetLoadFail(isSuccess);
            StopWatchManager.Instance.Stop("InitHSDIO", 0);
            //used to Bin out parts from a list in the TCF
            PaProductionTestPlan prodTp = new PaProductionTestPlan();
            isSuccess = prodTp.LoadModuleIDSelectList2();
            isSuccess = prodTp.LoadModule2DIDSelectList();
            m_modelTpState.SetLoadFail(isSuccess);

            #endregion Initialize Digital

            //Hsdio.EepromWrite("LBSPCSCV0701-001");
            //string pcbID = Hsdio.EepromRead();
            //ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, pcbID);

            if (!m_modelTpState.programLoadSuccess) return false;

            Thread.Sleep(1000);
            //initRf.Wait(60000);
            bool isMQTTPlugin = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_Type, "MANUAL").ToLower().Contains("mqtt");
            vdo = m_modelTestRunner.InitializeAllPaTests(m_tcfReader.TCF_Setting["MQTT_ENABLE"], isMQTTPlugin);
            m_modelTpState.SetLoadFail(vdo);
            if (!vdo.IsValidated)
            {
                MessageBox.ShowError(vdo);
                //for (byte site = 0; site < Eq.NumSites; site++)
                //{
                //    Eq.Site[site].PM.SetupBurstMeasurement(7000, 0.0025, 1);   // Only setup once at beginning of program.
                //}

            }

            ResultBuilder.Isfirststep = true;
            m_modelTestRunner.RunThroughTests();
            ResultBuilder.Isfirststep = false;

            m_modelTestRunner.PowerDownComplete(m_tcfReader.TCF_Setting["Tester_Type"]);

            string TestTimeDir = "C:\\Avago.ATF.Common.x64\\Production\\TestTime\\";
            for (byte site = 0; site < Eq.NumSites; site++)
            {
                StopWatchManager.Instance.SaveToFile(TestTimeDir + string.Format("DoATFInit_TestTimes_Site{0}.txt", site), "DoATFInit_Times", site);
                StopWatchManager.Instance.Reset(site);
            }

            bool GUCALValid = false;

            if (!m_modelTpState.Spara_Site)
            {
                OtpTest.ENABLE2DID_VALIDATION = Convert.ToBoolean(m_tcfReader.TCF_Setting["2DID_VALIDATION"]);
                string isEngineeringMode = m_tcfReader.TCF_Setting["GU_EngineeringMode"];
                string isEVMCAL = m_tcfReader.TCF_Setting["EVMCAL"];

                //Commented out FastEVM cal as it is not used ATM.
                //EVMCalibrationModel cal = new EVMCalibrationModel();
                //cal.EVMCalibration(this, "EDAM", isEVMCAL, Eq.NumSites);

                #region Read and store contactor and loadboard IDs

                string load_board_id = "LB-8243-081-NAN";
                string contactor_id = "LN-8243-NAN-NAN";
                string all_load_board_id = "";
                string all_contactor_id = "";
                string separator = "";

                ////Only eeprom per testboard, regardless single site or multisite
                //load_board_id = Eq.Site[0].HSDIO.UNIO_EEPROMReadID(EqHSDIO.UNIO_EEPROMType.Loadboard, 1);
                //load_board_id = load_board_id.Contains("8255") ? load_board_id : "LB-8255-XXX-NAN";
                //all_load_board_id = load_board_id;

                for (byte site=0; site<Eq.NumSites; site++)
                {      
                    if(site < (Eq.NumSites-1)) { separator = "_"; }
                    else { separator = ""; }

                    load_board_id = Eq.Site[site].HSDIO.UNIO_EEPROMReadID(EqHSDIO.UNIO_EEPROMType.Loadboard, 1);
                    load_board_id = load_board_id.Contains("8255") ? load_board_id : "LB-8255-XXX-NAN";
                    all_load_board_id += load_board_id + separator;

                    contactor_id = Eq.Site[site].HSDIO.UNIO_EEPROMReadID(EqHSDIO.UNIO_EEPROMType.Socket, 2);
                    contactor_id = contactor_id.Contains("8255") ? contactor_id : "LN-8255-XXX-NAN";
                    all_contactor_id += contactor_id + separator;
                }
               

                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_DIB_ID, all_load_board_id);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_CONTACTOR_ID, all_contactor_id);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_INSTRUMENT_INFO, Eq.InstrumentInfo);
               
                #endregion

                GuCalibrationModel gu = new GuCalibrationModel();
                string destCFpath =
                    ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");
                int mustIccGuCalCached = ATFCrossDomainWrapper.GetIntFromCache(PublishTags.PUBTAG_MUST_IccGuCal, -1);
                //string isEngineeringMode = ""; //m_tcfReader.m_tcfReader.TCF_Setting["GU_EngineeringMode"];
                //string productTagName = Path.GetFileName(destCFpath).ToUpper().Replace(".CSV", ""); // CF
                string ProductTagGU = m_tcfReader.TCF_Setting["GuPartNo"];
                string productTagName = ProductTagGU != "" ? ProductTagGU : Path.GetFileName(destCFpath).ToUpper().Replace(".CSV", "");
                gu.SetTesterType(m_tcfReader.TCF_Setting["Tester_Type"]); ///added
                isGUCALSuccess = gu.GuCalibration(this, destCFpath, mustIccGuCalCached,
                    isEngineeringMode, productTagName);

                
                //Evaluate if GUCAL is successful. Either site Passed indicate that the GUCAL is successful
                for (byte site = 0; site < Eq.NumSites; site++)
                {
                    GUCALValid = GUCALValid || isGUCALSuccess[site];

                    if (isGUCALSuccess[site] == false)
                    {
                        // Entry Point for MQTT commands to automatically turn off testSite
                        string ERR = string.Format("Site{0} failed GUCAL. If continue running multisite testing, please ensure TestSite{0} is disabled on Handler", site+1);
                        ATFLogControl.Instance.Log(LogLevel.Error, ERR);
                    }
                }
            }
            // Note: Not &= but set directly.
            m_modelTpState.programLoadSuccess = isSuccess && GUCALValid;

            //PinSweepTraceFile.Initialize(true, @"C:\Avago.ATF.Common\Results.RemoteShare\", 0.1, 1, 1000);
            //SwitchingTimeTraceFile.Initialize(true, @"C:\Avago.ATF.Common\Results.RemoteShare\", 1, 1000, 21);
            //Topaz_FbarTest.DataFiles.SNP.Enable = false;

            return m_modelTpState.programLoadSuccess;
        }

        private void RunSParaTest(ATFReturnResult results)
        {
            if (m_tcfReaderSpara.DataObject.ENA_Cal_Enable || !m_modelTpProd.FBAR_Test) return;

            ResultBuilder.BeforeTest(); //20190709 New Otptest

            m_modelTpProd.DoPreTest(results, m_tcfReaderSpara.DataObject, LibFbar,
                m_doClotho1.ClothoRootDir);

            StopWatchManager.Instance.Start("ProdFbarTest",0);

            // HSDIO.Instrument.SendVector(HSDIO.HiZ);
            //m_wrapper2.SetHsdioFirstScriptNA();
                        
            //Run test
            LibFbar.FileManager.tmpUnit_No = m_modelTpProd.Unit_ID;
            //m_wrapper4.CallcAlgoRunTest(LibFbar);
            LibFbar.Initialize();
            foreach (TestConditionDataObject tc in LibFbar.TestConditionCollection)
            {
                Switch(tc);
                LibFbar.Run(tc);
            }
            LibFbar.DoPostRunUnInit();

            SParaResultBuilder.BuildResult(LibFbar.Results, results);

            m_modelTpProd.DoPostTestJoker2(results, LibFbar,
                m_eqInitModel.Digital_Definitions_Part_Specific);


            if (m_modelTestRunner == null)      // For full setup, PA initialized this.
            {
                m_modelTestRunner = new MultiSiteTestRunner();
            }
            m_modelTestRunner.PowerDownDcAndDigitalPins2();
            m_modelTpProd.DoPostTest3(results, m_wrapper4, LibFbar,
                m_wrapper1.IsRunning);

          


        }

        private void RunPaTest()
        {
            ResultBuilder.BeforeTest();
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
            //RunTestsAllSites_ParallelFbarAndPa_parallel();
            //RunTestsAllSites_ParallelFbarThenParallelPa();
            //RunTestsAllSites_ParallelFbarThenSerialPa();
            //RunTestsAllSites_SerialPAthenFBAR_NoSplit();          

            #region Loop through validSites list to check if the Gucal status is valid
            foreach (byte site in ResultBuilder.ValidSites)
            {
                if (!GU.runningGU[site] && (isGUCALSuccess[site] == false))
                {
                    // Entry Point for MQTT commands to automatically turn off testSite
                    string ERR = string.Format("Site{0} failed GUCAL. Please disable this TestSite!!", site + 1);
                    ATFLogControl.Instance.Log(LogLevel.Error, ERR);
                }
            }
            #endregion

            StopWatchManager.Instance.Start("TP-DoATFTest-RunPaTestsAllSitesSerial2",0);
            //m_modelTestRunner.RunPaTestsAllSitesSerial2();
            m_modelTestRunner.RunPaTestsAllSitesParallel2();
            StopWatchManager.Instance.Stop("TP-DoATFTest-RunPaTestsAllSitesSerial2",0);

            BuildResult(false);

            m_modelTestRunner.PowerDownComplete(m_tcfReader.TCF_Setting["Tester_Type"]);           

            for (byte site = 0; site < Eq.NumSites; site++)
            {
                if(Calculate.MathCalc[site]!=null)
                Calculate.MathCalc[site].Clear();

                if(Calculate.MathCalc_ACLR1[site] != null)
                Calculate.MathCalc_ACLR1[site].Clear();

                if(Calculate.MathCalc_ACLR2[site] != null)
                Calculate.MathCalc_ACLR2[site].Clear();

                if(Calculate.MathCalc_EACLR[site] != null)
                Calculate.MathCalc_EACLR[site].Clear();

                if(Calculate.MathCalcCurrent[site] != null)
                Calculate.MathCalcCurrent[site].Clear();
            }

            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
        }

        private void BuildResult(bool isDebugMode)
        {
            string isEngineeringMode = m_tcfReader.TCF_Setting["GU_EngineeringMode"];
            bool EngineeringMode = isEngineeringMode == "FALSE" ? false : true;

            if (!isDebugMode)
            {
                m_modelTestRunner.BuildAllResults_PaTest2();
                if (!m_modelTpState.Spara_Site) m_modelTestRunner.CheckData_BurnOTP_PassFlag(isGUCALSuccess, EngineeringMode);
               return;
            }
            
            // Debug Mode.
            bool stateIsActive = StopWatchManager.Instance.IsActivated;
            StopWatchManager.Instance.IsActivated = true;
            //StopWatchManager.Instance.Start("BuildAllResults_PaTest2");
            m_modelTestRunner.BuildAllResults_PaTest2();
            //StopWatchManager.Instance.Stop("BuildAllResults_PaTest2");
            if (!m_modelTpState.Spara_Site)
            {
                //StopWatchManager.Instance.Start("CheckData_BurnOTP_PassFlag");
                m_modelTestRunner.CheckData_BurnOTP_PassFlag(isGUCALSuccess, EngineeringMode);
                //StopWatchManager.Instance.Stop("CheckData_BurnOTP_PassFlag");
            }

            double t1 = StopWatchManager.Instance.GetStopwatch("BuildAllResults_PaTest2",0).ElapsedMs;
            double t2 = StopWatchManager.Instance.GetStopwatch("CheckData_BurnOTP_PassFlag",0).ElapsedMs;

            string msg = String.Format("Performance Check:BuildResult {0}ms, BurnOTP {1}ms, Total:{2}ms, result Count:{3}",
                t1.ToString(), t2.ToString(), t1 + t2, ResultBuilder.results.Data.Count);
            //PromptManager.Instance.ShowInfo(msg);
            Console.WriteLine(msg);
            StopWatchManager.Instance.IsActivated = false;


        }

        //DoATFTest

        //Logger
        private static void LogToLogServiceAndFile(LogLevel logLev, string str)
        {
            LoggingManager.Instance.LogError(str);
            Console.WriteLine(str);
        }

        public void DefineNsTests()
        {
            ClothoLibAlgo.Calc.NStestConditions.Define("NS12", -3.2e6, 6.25e3);
            ClothoLibAlgo.Calc.NStestConditions.Define("NS15", 7.0e6, 6.25e3);
        }

        public void DefineCustomPowerMeasurements()
        {
            CustomPowerMeasurement.Define("Cpl", Operation.MeasureCpl, -20, CustomPowerMeasurement.Units.dBc, CustomPowerMeasurement.MeasureWith.VSA);  //By Hosein
            CustomPowerMeasurement.Define("H2", Operation.MeasureH2_ANT2, -40, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);
            CustomPowerMeasurement.Define("H3", Operation.MeasureH2_ANT2, -40, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);

            //Changed order Harmonic should be measured befor Tx leakage in order to avoid register conflict
            CustomPowerMeasurement.Define("TxLeakage", "B1", Operation.VSAtoRX1, 5, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);
            //CustomPowerMeasurement.Define("TxLeakage", "B1", Operation.VSAtoRX, -40, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);
            //CustomPowerMeasurement.Define("TxLeakage", "B3", Operation.VSAtoRX, -40, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);
            //CustomPowerMeasurement.Define("TxLeakage", "B7", Operation.VSAtoRX, -40, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);
            //CustomPowerMeasurement.Define("TxLeakage", "B25", Operation.VSAtoRX, -40, CustomPowerMeasurement.Units.dBm, CustomPowerMeasurement.MeasureWith.VSA);
        }

        public void LoadPaTestFromTcf(Dictionary<string, string> Digital_Definitions_Part_Specific, Dictionary<byte, int[]> SiteTrigArray)
        {
            try
            {
                PaTestFactory myTestFactory = new PaTestFactory();
                myTestFactory.Initialize(Digital_Definitions_Part_Specific, SiteTrigArray, m_tcfReader.TCF_Setting);
                iTest[][] allPaTests = myTestFactory.PopulateAllPaTests(
                    m_tcfReader, m_modelTpState);
                m_modelTpState.SetLoadFail(myTestFactory.ValDataObject.IsValidated);
                m_modelTestRunner.Initialize(allPaTests);
                m_modelTestRunner.Initialize(m_tcfReader.TCF_Setting["Tester_Type"],
                    String.Empty);
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
                m_modelTpState.SetLoadFail();
            }
        }

        private ProjectSpecificFactorDataObject GetProjectSpecificFactor(ProjectSpecificFactor.Projectbase ps)
        {
            ProjectSpecificFactor.SetProject(ps);

            ProjectSpecificFactorDataObject psDo = new ProjectSpecificFactorDataObject();
            psDo.CalColmnIndexNFset = ps.CalColmnIndexNFset;
            psDo.FirstTrigOfRaw = ps.FirstTrigOfRaw;
            psDo.PortEnable = ps.PortEnable;
            psDo.listLNAReg = ps.listLNAReg;
            return psDo;
        }

        private void Calibrate(CalibrationModel model, ProdLib1Wrapper wrapper4)
        {
            DialogResult isAutoCal = m_modelCalInit.PromptAutoSubCal();
            switch (isAutoCal)
            {
                case DialogResult.Cancel:
                    break;
                case DialogResult.No:
                    m_modelCalInit.PerformManualECal(wrapper4);
                    break;
                case DialogResult.Yes:
                    TcfSheetCalProcedure sheet = new TcfSheetCalProcedure("Calibration Procedure", 2000, 50);
                    try
                    {
                        model.Load_Calibration(sheet);
                    }
                    catch (Exception ex)
                    {
                        PromptManager.Instance.ShowError("Error loading Calibration Procedure sheet.", ex);
                    }

                    bool Fbar_cal = true;
                    while (Fbar_cal)
                    {
                        string chosenCalType = m_modelCalInit.PromptAutoManualOrVerify(LibFbar.CalibrationModel, ProjectSpecificFactor.TopazCalPower);
                        bool isManualCal = chosenCalType == "SW";
                        Fbar_cal = chosenCalType != "ABORT";

                        // Perform manual cal.
                        bool SW_TEST = true;
                        SparaManualCalSwitchingModel calSwitch =
                            new SparaManualCalSwitchingModel(Eq.Site[0].SwMatrix);
                        while (isManualCal && SW_TEST)
                        {
                            //string SW_cmd = Interaction.InputBox("please enter \"Channel & Switch controller\", if you want to stop enter \"X\"Network Analyzer Calibration", "X", 150, 150);
                            string SW_cmd =
                                PromptManager.Instance.ShowTextInputDialog(
                                    "please enter \"Channel & Switch controller\",",
                                    "if you want to stop enter \"X\"",
                                    "Network Analyzer Calibration", "X");
                            SW_TEST = calSwitch.Run_Manual_SW(SW_cmd);
                            if (SW_cmd.ToUpper() == "X" || SW_cmd.ToUpper() == "")
                            {
                                break;
                            }
                            model.Verify_ECAL_SCPI(SW_cmd);

                            //  SW_TEST = LibFbar.Run_Manual_SW(SW_cmd, Product);
                        }

                    }
                    break;
            }
        }

        private bool Switch(TestConditionDataObject tc)
        {
            bool isExecuted = true;
            SparamTrigger tt = tc.TestConditionLine as SparamTrigger;

            switch (tc.TestParameterColumn)
            {
                case "TRIGGER":
                    ////ChoonChin - debug
                    //if (Test == 1691)
                    //{
                    //}
                    StopWatchManager.Instance.Start("Trigger_Run_SW",0);
                    ProjectSpecificFactor.cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);
                    StopWatchManager.Instance.Stop("Trigger_Run_SW",0);
                    break;
                case "TRIGGER_NF":    // For Topaz NF test 20160615
                    ProjectSpecificFactor.cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);
                    break;
                case "TRIGGER_NF_DUAL":    // For Topaz NF DUAL test 20161215
                    // Joker run this, HLS does not.
                    ProjectSpecificFactor.cProject.SetSwitchMatrixPaths(tt.Band, tt.Master_RX);
                    ProjectSpecificFactor.cProject.SetSwitchMatrixPaths(tt.Band, tt.Slave_RX);
                    break;

                default:
                    isExecuted = false;
                    break;
            }

            return isExecuted;
        }
    } 
} 