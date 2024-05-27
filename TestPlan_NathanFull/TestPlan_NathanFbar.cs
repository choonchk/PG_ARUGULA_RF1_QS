using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.LogService;
using System.Linq;
using System.Windows.Forms;
using LibFBAR_TOPAZ.DataType;
using SParamTestCommon;
using TestPlanCommon;
using TestPlanCommon.CommonModel;
using TestPlanCommon.PaModel;
using TestPlanCommon.SParaModel;
using ToBeObsoleted;
#region Custom Reference Section

using EqLib;
using TestLib;
using SnP_BuddyFileBuilder;
using MPAD_TestTimer;
using LibFBAR_TOPAZ;
using IqWaveform;

#endregion Custom Reference Section

namespace TestPlan_NathanFull
{
    public static class GlobalVariables
    {
        public const string TraceTabName = "Trace";
        public const string SegmentTabName = "Segment";
        public const string ConditionFbarTabName = "Condition_FBAR";

        public const string HSDIOAlias = "NI6570";
        //public const string DIOAlias = "NI6509_01";
        public const string DIOAlias = "DIO2";

        public const EqLib.EqSwitchMatrix.Rev SwitchMatrixBox = EqLib.EqSwitchMatrix.Rev.Y2DNightHawk;

        public const bool Use28OhmCalVerification = false; //not used right now

    }

    public class NightHawkFbar : TestPlanBase, IATFTest
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

        private SParaEquipmentInitializer m_eqInitModel;
        private MultiSiteTestRunner m_modelTestRunner;

        #endregion TestPlan Properties

        private SParaTestManager LibFbar;

        public NightHawkFbar()
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
        }

        private void AddDictEntry(Dictionary<string, string> RefDict, string zkey, string zval)
        {
            RefDict.Add(zkey, zval);
        } 

        public override string DoATFInit(string args)
        {            
            base.DoATFInit(args);

            StopWatchManager.Instance.IsActivated = true;
            StopWatchManager.Instance.Start(0);

            DoAtfInit2();
            ValidationDataObject vdo;

            try
            {
                if (m_modelTpState.Pa_Site)
                {
                    //bool isSuccess = InitializePaTest(m_eqInitModel);
                    //if (!isSuccess) return TestPlanRunConstants.RunFailureFlag;
                }
                if (m_modelTpState.Spara_Site)
                {
                    ReadTcfAndInitSParaTest();
                    InitializeEquipment(m_eqInitModel);
                    
                    InitializeSParaTest(GlobalVariables.TraceTabName, GlobalVariables.SegmentTabName);

                    //TODO This is the Modular Cal.
                    //m_modelCalInit.PromptSubCalModular(m_tcfReaderSpara);
                    //TODO This is the current cal.
                    Calibrate(LibFbar.CalibrationModel, m_wrapper4);
                    InitializeGuCal();
                    vdo = Validate(null);
                    if (!vdo.IsValidated) return vdo.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                vdo = Validate(ex);
                return vdo.ErrorMessage;
            }

            StopWatchManager.Instance.Stop(0);

            InitializeProductionTestPlan();

            vdo = Validate(null);
            return vdo.ErrorMessage;
        }
        public ATFReturnResult DoATFTest(string args)
        {
            StopWatchManager.Instance.Start("TIME_DoATFTest", 0);
            //Debugger.Break();
            ATFReturnResult results = new ATFReturnResult();

            #region Clotho - Example for Argument Parsing

            // ----------- Example for Argument Parsing --------------- //
            //Dictionary<string, string> dict = new Dictionary<string, string>();
            //if (!ArgParser.parseArgString(args, ref dict))
            //{
            //    err = "Invalid Argument String" + args;
            //    MessageBox.Show(err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return new ATFReturnResult(err);
            //}

            //int simHW;
            //try
            //{
            //    simHW = ArgParser.getIntItem(ArgParser.TagSimMode, dict);
            //}
            //catch (Exception ex)
            //{
            //    err = ex.Message;
            //    MessageBox.Show(err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return new ATFReturnResult(err);
            //}
            // ----------- END of Argument Parsing Example --------------- //

            #endregion Clotho - Example for Argument Parsing

            #region Custom Test Coding Section

            //////////////////////////////////////////////////////////////////////////////////
            // ----------- ONLY provide your Custom Test Coding here --------------- //

            try
            {
                SnP_BuddyFileBuilder.SnPFileBuilder SecResFile = new SnPFileBuilder();

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

                //if (!PaTest.headerFileMode) GU.DoTest_beforeCustomCode();

                for (int x = 1; x <= 1; x++)
                {
                    foreach (byte site in ResultBuilder.ValidSites)
                    {
                        ResultBuilder.Clear(site);
                        ResultBuilder.FailedTests[site].Clear(); //ChoonChin - 20191203 - clear failed test for lockbit burn
                    }

                    //if (m_modelTpState.Pa_Site && !m_tcfReaderSpara.DataObject.ENA_Cal_Enable)
                    //{
                    //    RunPaTest();
                    //    if (!m_modelTpState.Spara_Site)
                    //    {
                    //        return ResultBuilder.FormatResultsForReturnToClotho();
                    //    }
                    //}

                    if (m_modelTpState.Spara_Site)
                    {
                        m_modelTpState.CurrentTestResultFileName = m_modelTpProd.SetTestResult("AFEM-8300-AP1");

                        if (m_tcfReaderSpara.DataObject.ENA_Cal_Enable)
                        {
                            m_modelCalInit.DataObject = LibFbar.ProjectSpecificDataObject.CalibrationInputDataObject;
                            m_modelCalInit.Calibrate(LibFbar.CalibrationModel, results);
                            ATFResultBuilder.AddResult(ref results, "M_TIME_FbarTest", "ms", 5.0f);
                            //ResultBuilder.AddResult(0, "FbarTestTime", "ms", 0f/*fbartestTime*/);
                            return results;
                        }

                        RunSParaTest(results);
                    } //if (Spara_Site)

                    m_wrapper1.DoAtfTestMoveNext(results);
                } //for (GU.runLoop = 1; GU.runLoop <= GU.numRunLoops; GU.runLoop++)
            } // Try
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError("Error happened during the runtest at", ex);
                //return new ATFReturnResult(TestPlanRunConstants.RunFailureFlag);
            }

            #endregion Custom Test Coding Section

            //Internal_DUT_Count++;

            StopWatchManager.Instance.Stop("TIME_DoATFTest", 0);

            // Used for production.
            m_modelTpProd.TestTimeLogController.Save();
            // Not used in production.
            //StopWatchManager.Instance.SaveToFile(@"C:\TEMP\DoATFTest_TestTime.csv", "runtest", ',');
            StopWatchManager.Instance.Clear(0);
            return ResultBuilder.FormatResultsForReturnToClotho();
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
                if (Eq.Site[site].HSDIO != null)
                {
                    Eq.Site[site].HSDIO.Close();
                }
                if (Eq.Site[site].RF != null)
                {
                    Eq.Site[site].RF.close();
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
            InitializeSParaTest(GlobalVariables.TraceTabName, GlobalVariables.SegmentTabName);
            CloseLotInitialize(GlobalVariables.TraceTabName, GlobalVariables.SegmentTabName);
            
            if (LibFbar != null)
            {
                LibFbar.FileManager.m_isNotFirstRun = false;
            }
            

            // ----------- END of Custom CloseLot Coding --------------- //
            //////////////////////////////////////////////////////////////////////////////////

            #endregion Custom CloseLot Coding Section

            StopWatchManager.Instance.Stop(0);
            return sb.ToString();
        }

        /// <summary>
        /// Basic initialization code.
        /// </summary>
        private void DoAtfInit2()
        {
            IQ.BasePath = m_doClotho1.ClothoRootDir + @"Waveform\";
            ResultBuilder.headerFileMode = false;

            Eq.SetNumSites(1);

            m_tcfReaderSpara.FillMainTab();
            // Copy settings from Spara to m_tcfReader. (Workaround)
            m_tcfReader.TCF_Setting = new Dictionary<string, string>();
            m_tcfReader.TCF_Setting.Add("Tester_Type", m_tcfReaderSpara.TCF_Setting["Tester_Type"]);
            m_tcfReader.TCF_Setting.Add("Handler_Type", m_tcfReaderSpara.TCF_Setting["Handler_Type"]);
            m_tcfReader.TCF_Setting.Add("Sample_Version", m_tcfReaderSpara.TCF_Setting["Sample_Version"]);

            m_modelTpState = new TestPlanStateModel();
            // TODO Switch tester Penang Seoul.
            //m_modelTpState.SetTesterSite(new TestPlan_PinotFull.Tester.PenangJokerRf2Tester());
            m_modelTpState.SetTesterSite(new Tester.SjHls2Rf2Tester1());

            m_eqInitModel = new SParaEquipmentInitializer();
            m_eqInitModel.SetTester(m_modelTpState.TesterSite);
            //m_eqInitModel.InitializeHandler(m_tcfReader.TCF_Setting["Handler_Type"], m_modelTpState.TesterSite.GetHandlerName());
            m_eqInitModel.InitializeHandler(m_tcfReader.TCF_Setting["Handler_Type"], GlobalVariables.HSDIOAlias);
            m_modelCalInit.ReadSmPad(m_wrapper4);

            bool isSiteDefined = m_modelTpState.CheckTesterType2(m_doClotho1.ConfigXmlPath);
            if (isSiteDefined)
            {
                string msg = "Clotho Tester Type is not configured, abort test plan!";
                PromptManager.Instance.ShowError(msg, "DoAtfInit2 Error");
                LoggingManager.Instance.LogError(msg);
                //throw new Exception("Clotho Tester Type is not configured, abort test plan!");
            }

            //HSDIO.useScript = false;

            m_wrapper4.Wrapper2.InitializeVar();

            string productTag = m_wrapper1.DoAtfInitProductTag(m_doClotho1.ClothoRootDir);
            m_modelTpProd.SetProductTag(productTag);
        }

        private void ReadTcfAndInitSParaTest()
        {
            #region Read TCF

            Tuple<bool, string, string[,]> tcfSheetCondFbar =
                ATFCrossDomainWrapper.Excel_Get_IputRangeByValue("Condition_FBAR", 1, 1, 7000, 300);
            if (tcfSheetCondFbar.Item1 == false)
            {
                PromptManager.Instance.ShowError(
                    "Error reading Excel Range", tcfSheetCondFbar.Item2, "");
                m_modelTpState.SetLoadFail();
            }

            m_tcfReaderSpara.FillRow(tcfSheetCondFbar);

            Tuple<bool, string, string[,]> tcfSheetHeaderSequence =
                ATFCrossDomainWrapper.Excel_Get_IputRangeByValue("HeaderSequence", 1, 1, 100, 2);
            if (tcfSheetHeaderSequence.Item1 == false)
            {
                PromptManager.Instance.ShowError("Error reading Excel Range", tcfSheetHeaderSequence.Item2);
                m_modelTpState.SetLoadFail();
            }

            m_tcfReaderSpara.FillEnaState();

            #endregion Read TCF

            // Initialize LibFBar.
            string tempFbarTesterName = m_tcfReader.TCF_Setting["Tester_Type"] == "SPARA"
                ? "RF2"
                : m_tcfReader.TCF_Setting["Tester_Type"];

            LibFbar = new SParaTestManager();
            LibFbar.TcfHeaderEntries = SParaTestConditionReader.GetSheetHeaderSequence(tcfSheetHeaderSequence);
            LibFbar.TcfConditionFbarTab2 = tcfSheetCondFbar;

            PinotRF2 projectType = new PinotRF2(); //Change this to reflect NightHawk

            LibFbar.ProjectSpecificDataObject = GetProjectSpecificFactor(projectType);
        }
        private void InitializeEquipment(SParaEquipmentInitializer eqInitModel)
        {
            //ChoonChin - 20191204 - Move to SParaTestManager's Initialization function.
            //CopyStateFileToNAFolder();

            m_modelTpProd.SetDPatOutlier(m_tcfReaderSpara.DataObject.DPAT_Flag);

            //DC & MIPI
            LoggingManager.Instance.LogInfoTestPlan("Initialize SMU.");
            eqInitModel.InitializeSmu();

            LoggingManager.Instance.LogInfoTestPlan("Initialize HSDIO");

            //TestPlan_NightHawkFull.PaEquipmentInitializer m = new PaEquipmentInitializer();
            //bool isSuccess2 = m.LoadVector(
            //    m_doClotho1.ClothoRootDir, "", ""); //m_tcfReaderSpara.TCF_Setting["CMOS_DIE_TYPE"],
            //    //m_tcfReaderSpara.TCF_Setting["Sample_Version"]);

            bool isSuccess2 = eqInitModel.InitializeHSDIO();
            m_modelTpState.SetLoadFail(isSuccess2);
            isSuccess2 = eqInitModel.LoadVector(
                m_doClotho1.ClothoRootDir, "", m_tcfReaderSpara.TCF_Setting["Sample_Version"]); //m_tcfReaderSpara.TCF_Setting["CMOS_DIE_TYPE"],
                //m_tcfReaderSpara.TCF_Setting["Sample_Version"]);
            m_modelTpState.SetLoadFail(isSuccess2);

            LoggingManager.Instance.LogInfoTestPlan("Initialize Switch Matrix");
            eqInitModel.InitializeSwitchMatrix(false, "");

            ProjectSpecificFactor.cProject.SetEquipment(Eq.Site[0].SwMatrix);
        }

        //DoATFLot
        private void InitializeSParaTest()
        {
            StopWatchManager.Instance.Start("FBAR_Test_Loading",0);

            m_modelTpProd.Reset();

            LoggingManager.Instance.LogInfoTestPlan("Initialize Lib FBAR.");

            try
            {
                //LibFbar.parse_ENA_IO = myENA;
                SParaTestConditionFactory tcfData = m_tcfReaderSpara.DataObject;
                s_SNPFile sf = new s_SNPFile();
                sf.ENASNPFileOutput_Enable = tcfData.EnaStateFileEnable;    //ChoonChin - 20191122 - Name should be EnaSNPFileEnable...
                sf.SNPFileOutput_Path = m_modelTpProd.SNP_Files_Dir;
                sf.FileOutput_Enable = tcfData.TraceFileEnable;
                sf.FileOutput_Path = m_modelTpProd.ActiveDirectory;
                sf.FileOutput_FileName = "FEM";
                sf.FileOuuput_Count = tcfData.TraceFileOutput_Count;
                LibFbar.FileManager.InitializeSnpFile(sf);
                LibFbar.TcfConditionFbarTab = tcfData.DicTestCondTempNA;


                //to CCT:  Excel_Get_InputRange is obsolete
                //Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRange(4, 1, 1, 500, 100);

                Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(4, 1, 1, 500, 100);
                TcfSheetTrace sheet1 = new TcfSheetTrace("Trace", 500, 100);
                TcfSheetSegmentTable sheet2 = new TcfSheetSegmentTable("Segment", 3000, 20);
                LibFbar.Initialization(sheet1, sheet2, TempTrace, tcfData.EnaStateFile);
                
            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
                LogToLogServiceAndFile(LogLevel.Error, ex.ToString());
                m_modelTpProd.FBAR_Test = false;
                // return TestPlanRunConstants.RunFailureFlag + "Failed FBAR Test Loading";    //Temporary disabled for PCB AutoCal debug only
            }

            StopWatchManager.Instance.Stop("FBAR_Test_Loading",0);
        }
        private void InitializeSParaTest(string TraceTabName, string SegmentTabName)
        {
            StopWatchManager.Instance.Start("FBAR_Test_Loading",0);

            m_modelTpProd.Reset();

            LoggingManager.Instance.LogInfoTestPlan("Initialize Lib FBAR.");

            try
            {
                //LibFbar.parse_ENA_IO = myENA;
                SParaTestConditionFactory tcfData = m_tcfReaderSpara.DataObject;
                s_SNPFile sf = new s_SNPFile();
                sf.ENASNPFileOutput_Enable = tcfData.EnaStateFileEnable;    //ChoonChin - 20191122 - Name should be EnaSNPFileEnable...
                sf.SNPFileOutput_Path = m_modelTpProd.SNP_Files_Dir;
                sf.FileOutput_Enable = tcfData.TraceFileEnable;
                sf.FileOutput_Path = m_modelTpProd.ActiveDirectory;
                sf.FileOutput_FileName = "FEM";
                sf.FileOuuput_Count = tcfData.TraceFileOutput_Count;
                sf.SNPFileOuuput_Count = tcfData.SNPFileOutput_Count;
                LibFbar.FileManager.InitializeSnpFile(sf);
                LibFbar.TcfConditionFbarTab = tcfData.DicTestCondTempNA;


                //to CCT:  Excel_Get_InputRange is obsolete
                //Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRange(4, 1, 1, 500, 100);

                Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(TraceTabName, 1, 1, 500, 100);
                TcfSheetTrace sheet1 = new TcfSheetTrace(TraceTabName, 500, 100);
                TcfSheetSegmentTable sheet2 = new TcfSheetSegmentTable(SegmentTabName, 3000, 20);
                LibFbar.Initialization(sheet1, sheet2, TempTrace, tcfData.EnaStateFile);

            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
                LogToLogServiceAndFile(LogLevel.Error, ex.ToString());
                m_modelTpProd.FBAR_Test = false;
                // return TestPlanRunConstants.RunFailureFlag + "Failed FBAR Test Loading";    //Temporary disabled for PCB AutoCal debug only
            }

            StopWatchManager.Instance.Stop("FBAR_Test_Loading",0);
        }

        private void CloseLotInitialize(string TraceTabName, string SegmentTabName)
        {
            StopWatchManager.Instance.Start("FBAR_CloseLot_Loading",0);

            m_modelTpProd.Reset();

            LoggingManager.Instance.LogInfoTestPlan("Close Lot Initialize");

            try
            {
                //LibFbar.parse_ENA_IO = myENA;
                SParaTestConditionFactory tcfData = m_tcfReaderSpara.DataObject;
                s_SNPFile sf = new s_SNPFile();
                sf.ENASNPFileOutput_Enable = tcfData.EnaStateFileEnable;    //ChoonChin - 20191122 - Name should be EnaSNPFileEnable...
                sf.SNPFileOutput_Path = m_modelTpProd.SNP_Files_Dir;
                sf.FileOutput_Enable = tcfData.TraceFileEnable;
                sf.FileOutput_Path = m_modelTpProd.ActiveDirectory;
                sf.FileOutput_FileName = "FEM";
                sf.FileOuuput_Count = tcfData.TraceFileOutput_Count;
                sf.SNPFileOuuput_Count = tcfData.SNPFileOutput_Count;
                LibFbar.FileManager.InitializeSnpFile(sf);
                LibFbar.TcfConditionFbarTab = tcfData.DicTestCondTempNA;
                


                //to CCT:  Excel_Get_InputRange is obsolete
                //Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRange(4, 1, 1, 500, 100);

                Tuple<bool, string, string[,]> TempTrace = ATFCrossDomainWrapper.Excel_Get_IputRangeByValue(TraceTabName, 1, 1, 500, 100);
                TcfSheetTrace sheet1 = new TcfSheetTrace(TraceTabName, 500, 100);
                TcfSheetSegmentTable sheet2 = new TcfSheetSegmentTable(SegmentTabName, 3000, 20);
                LibFbar.Initialization(sheet1, sheet2, TempTrace, tcfData.EnaStateFile);

            }
            catch (Exception ex)
            {
                PromptManager.Instance.ShowError(ex);
                LogToLogServiceAndFile(LogLevel.Error, ex.ToString());
                m_modelTpProd.FBAR_Test = false;
                // return TestPlanRunConstants.RunFailureFlag + "Failed FBAR Test Loading";    //Temporary disabled for PCB AutoCal debug only
            }

            StopWatchManager.Instance.Stop("FBAR_CloseLot_Loading",0);
        }

        private void InitializeGuCal()
        {
            LoggingManager.Instance.LogInfoTestPlan("Initialize GUCal.");

            GuCalibrationModel gu = new GuCalibrationModel();
            string destCFpath =
                ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");
            int mustIccGuCalCached = ATFCrossDomainWrapper.GetIntFromCache(PublishTags.PUBTAG_MUST_IccGuCal, -1);
            string isEngineeringMode = m_tcfReaderSpara.TCF_Setting["GU_EngineeringMode"];
            string productTagName = Path.GetFileName(destCFpath).ToUpper().Replace(".CSV", ""); // CF
            gu.SetTesterType(m_tcfReader.TCF_Setting["Tester_Type"]);
            //ChoonChin - Don't follow CF file  name
            productTagName = "Hallasan2";
            bool[] isSuccess = gu.GuCalibration(this, destCFpath, mustIccGuCalCached,
                isEngineeringMode, productTagName);
            // Note: Not &= but set directly.
            m_modelTpState.programLoadSuccess = isSuccess[0];
        }

        private void InitializeProductionTestPlan()
        {
            ValidationDataObject vdo = Validate(null);
            if (!vdo.IsValidated) return;

            //used to Bin out parts from a list in the TCF
            PaProductionTestPlan prodTp = new PaProductionTestPlan();
            bool isSuccess2 = prodTp.LoadModuleIDSelectList2();
            m_modelTpState.SetLoadFail(isSuccess2);

            string TestTimeDir = "C:\\Avago.ATF.Common.x64\\Production\\TestTime\\";
            StopWatchManager.Instance.SaveToFile(TestTimeDir + "DoATFInit_TestTimes.txt", "DoATFInit_Times",0);
            StopWatchManager.Instance.Reset(0);

            // Enabled only after test transfer to Penang. For production dashboard. Clash with GUCal.
            if (m_modelTpState.Spara_Site)
            {
                m_modelTpProd.TestTimeLogController.Initialize(new TestPlanStateModel(),
                m_doClotho1.ConfigXmlPath, m_tcfReaderSpara.DataObject.DicTestCondTempNA);
            }

            // Cal may override ena cal enable.
            m_tcfReaderSpara.DataObject.ENA_Cal_Enable = m_modelCalInit.ENA_Cal_Enable;

            //if (!ENA_Cal_Enable || ENA_Cal_Enable) //skip prod GUI and Icc Cal if auto subcal needed
            if (!m_tcfReaderSpara.DataObject.ENA_Cal_Enable) //skip prod GUI and Icc Cal if auto subcal needed //Original
            {
#if (DEBUG)
                SParaProductionTestPlan tp =
 new SParaProductionTestPlan();
                tp.ShowInputGui("8200");
#endif
            }
        }
        private void RunSParaTest(ATFReturnResult results)
        {
            if (m_tcfReaderSpara.DataObject.ENA_Cal_Enable && !m_modelTpProd.FBAR_Test) return;

            m_modelTpProd.DoPreTest(results, m_tcfReaderSpara.DataObject, LibFbar, m_doClotho1.ClothoRootDir);

            StopWatchManager.Instance.Start("ProdFbarTest",0);

            // HSDIO.Instrument.SendVector(HSDIO.HiZ);
            //m_wrapper2.SetHsdioFirstScriptNA();

            //Run test
            LibFbar.FileManager.tmpUnit_No = m_modelTpProd.Unit_ID;
            LibFbar.Initialize();
            foreach (TestConditionDataObject tc in LibFbar.TestConditionCollection)
            {
                Switch(tc);
                
                
                LibFbar.Run(tc);
            }
            LibFbar.DoPostRunUnInit();

            m_modelTpProd.SetUnitId(LibFbar.Results);
            SParaResultBuilder.BuildResult(LibFbar.Results, results);
            bool isSuccess = m_modelTpProd.DetectDoubleUnit(
                m_tcfReaderSpara.DataObject.PauseTestOnDuplicate, LibFbar.Results);
            m_modelTpState.SetLoadFail(isSuccess);

            m_modelTpProd.DoPostTestJoker2(results, LibFbar, m_eqInitModel.Digital_Definitions_Part_Specific);
            //m_modelTpProd.DoPostTestHallasan2(results, m_wrapper2, LibFbar,
            //    m_eqInitModel.Digital_Definitions_Part_Specific);
            // Need to enable this for production
            //bool isDPatOutlierPass = m_modelTpProd.CheckDPatOutlier(results,
            //    m_eqInitModel.Digital_Definitions_Part_Specific);
            bool isDPatOutlierPass = false;
            if (isDPatOutlierPass)
            {
                return;     // Don't proceed
            }

            m_modelTpProd.DoPostTestJoker3(results, m_wrapper2, LibFbar, m_wrapper3.FailedTestCount,
                m_eqInitModel.Digital_Definitions_Part_Specific, m_modelTpState.litedrivermode,
                m_modelTpState.fbar_tester_id);

            m_modelTestRunner = new MultiSiteTestRunner();
            m_modelTestRunner.PowerDownDcAndDigitalPins2();
            m_modelTpProd.DoPostTest3(results, m_wrapper4, LibFbar,
                m_wrapper1.IsRunning);
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
                    //MessageBox.ShowDialogOKCancel("Switch has been set for " + "\r\n" + "Antenna = " + tt.SwitchAnt + "\r\n" + "Rx = " + tt.SwitchOut + "\r\n" + "Tx In " + tt.SwitchIn + "\r\n" + "Click OK when finished", "Switch Box Setting");
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

                    #region Calibration
                    
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
                        string chosenCalType = m_modelCalInit.PromptAutoManualOrVerify(LibFbar.CalibrationModel,
                            ProjectSpecificFactor.TopazCalPower);
                        //ChoonChin - 20191205 - Cancel to quit
                        if (chosenCalType == "CANCEL")
                        {
                            break;
                        }

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
                                    "please enter \"Channel Number\",",
                                    "if you want to stop enter \"X\"",
                                    "Network Analyzer Calibration", "X");

                            if (SW_cmd.ToUpper() == "X" || SW_cmd.ToUpper() == "")
                            {
                                break;
                            }
                            else
                            {
                                string CH_N = "";
                                string tx_port = "";
                                string ant_port = "";
                                string rx_port = "";
                                int size = LibFbar.TcfConditionFbarTab.Count;
                                int equi_index = -1;
                                bool SW_TEST1 = false;

                                for (int i = 0; i < size; i++)
                                {
                                    if (LibFbar.TestConditionCollection[i].TestParameterColumn == "TRIGGER")
                                    {
                                        equi_index = i;
                                        break;
                                    }
                                }

                                for (int i = 0; i < size; i++)
                                {

                                    LibFbar.TcfConditionFbarTab[i].TryGetValue("Channel Number", out CH_N);
                                    if (CH_N == SW_cmd)
                                    {
                                        LibFbar.TcfConditionFbarTab[i].TryGetValue("Switch_In", out tx_port);
                                        SW_TEST1 = calSwitch.Run_Manual_SW2(tx_port);
                                        LibFbar.TcfConditionFbarTab[i].TryGetValue("Switch_ANT", out ant_port);
                                        SW_TEST1 = calSwitch.Run_Manual_SW2(ant_port);
                                        LibFbar.TcfConditionFbarTab[i].TryGetValue("Switch_Out", out rx_port);
                                        SW_TEST1 = calSwitch.Run_Manual_SW2(rx_port);

                                        PromptManager.Instance.ShowInfo("You can start cal with ecal now.\r\n"+"After finished the channel cal, press the OK button" + "\r\n");

                                        break;
                                    }


                                }

                            }

                            //model.Verify_ECAL_SCPI(SW_cmd);
                            //  SW_TEST = LibFbar.Run_Manual_SW(SW_cmd, Product);
                        }
                    }
                    #endregion

                    break;
            }

        }
        

        /// <summary>
        /// Copy statefile from testplan folder to NA folder is statefile is new, skip if already exist
        /// </summary>
        private void CopyStateFileToNAFolder()
        {
            string StateFileFolderPath = string.Format(@"C:\Users\Public\Documents\Network Analyzer\{0}",
                m_tcfReaderSpara.DataObject.EnaStateFile);
            string NewStateFilePath = string.Format("{0}FileNeeded\\{1}", m_doClotho1.ClothoRootDir,
                m_tcfReaderSpara.DataObject.EnaStateFile);

            if (!File.Exists(StateFileFolderPath))
            {
                File.Copy(NewStateFilePath, StateFileFolderPath);
                string msg = "New statefile copied to " + StateFileFolderPath;
                LoggingManager.Instance.LogInfo(msg);
            }
            else
            {
                string msg = "State file is " + StateFileFolderPath;
                LoggingManager.Instance.LogInfo(msg);
            }
        }

        private ValidationDataObject Validate(Exception ex)
        {
            ValidationDataObject vdo = new ValidationDataObject();
            if (ex != null)
            {
                PromptManager.Instance.ShowError(ex);
                m_modelTpState.SetLoadFail();
                vdo.ErrorMessage = TestPlanRunConstants.RunFailureFlag + ex.Message;
                return vdo;
            }

            if (!m_modelTpState.programLoadSuccess)
            {
                vdo.ErrorMessage = TestPlanRunConstants.RunFailureFlag;
            }
            return vdo;
        }

        private ProjectSpecificFactorDataObject GetProjectSpecificFactor(ProjectSpecificFactor.Projectbase ps)
        {
            ProjectSpecificFactor.SetProject(ps);

            ProjectSpecificFactorDataObject psDo = new ProjectSpecificFactorDataObject();
            psDo.CalColmnIndexNFset = ps.CalColmnIndexNFset;
            psDo.FirstTrigOfRaw = ps.FirstTrigOfRaw;
            psDo.PortEnable = ps.PortEnable;
            psDo.listLNAReg = ps.listLNAReg;

            CalibrationInputDataObject cal = new CalibrationInputDataObject();
            cal.CalContinue = ProjectSpecificFactor.CalContinue;
            cal.Fbar_cal = ProjectSpecificFactor.Fbar_cal;
            cal.Using28Ohm = ProjectSpecificFactor.Using28Ohm;
            cal.CalDone = ProjectSpecificFactor.CalDone;
            psDo.CalibrationInputDataObject = cal;

            return psDo;
        }

        //Logger
        private static void LogToLogServiceAndFile(LogLevel logLev, string str)
        {
            LoggingManager.Instance.LogError(str);
            Console.WriteLine(str);
        }
    }
}