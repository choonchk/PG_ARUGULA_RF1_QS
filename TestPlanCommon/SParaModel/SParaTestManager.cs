using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using EqLib;
using InstrLib;
using LibFBAR_TOPAZ;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using LibFBAR_TOPAZ.ANewTestLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;
using SParamTestCommon;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// Load test conditions, run and get result. Create NA equipment.
    /// </summary>
    public class SParaTestManager
    {
        public List<s_Result> Results;
        private TopazEquipmentInitializer m_topazEquipment;
        private SParaTestFactory m_testCondFactory;

        public List<double> TestTime;
        public List<double> FbarTestTime;

        public string NA_StateFile
        {
            get { return m_topazEquipment.NA_StateFile; }
            set
            {
                m_topazEquipment.NA_StateFile = value;
            }
        }

        public ProjectSpecificFactorDataObject ProjectSpecificDataObject { get; set; }

        /// <summary>
        /// TCF Condition_FBAR tab also.
        /// </summary>
        public List<Dictionary<string, string>> TcfConditionFbarTab;

        /// <summary>
        /// TCF Condition_FBAR tab.
        /// </summary>
        public Tuple<bool, string, string[,]> TcfConditionFbarTab2;

        /// <summary>
        /// For Golden Eagle compliant result header.
        /// </summary>
        public SortedDictionary<int, string> TcfHeaderEntries
        {
            set
            {
                m_testCondFactory.HeaderEntries = value;
            }
        }

        private bool m_isDivaInstrument;

        //For DIVA
        public bool isDivaInstrument
        {
            get { return m_isDivaInstrument; }
            set
            {
                m_isDivaInstrument = value;
                CalibrationModel.SetInstrumentDiva(value);
            }
        }

        /// <summary>
        /// Enable access to spara output files.
        /// </summary>
        public SParaFileManager FileManager { get; set; }

        public CalibrationModel CalibrationModel { get; set; }

        public List<TestConditionDataObject> TestConditionCollection { get; set; }
        public SParaTestManager()
        {
            // M9485 = new Agilent.AgNA.Interop.AgNA();
            m_topazEquipment = new TopazEquipmentInitializer();
            CalibrationModel = new CalibrationModel();
            FileManager = new SParaFileManager();
            m_testCondFactory = new SParaTestFactory();
        }

        public void Initialization()
        {
            //string enaAddress = "TCPIP0::localhost::hislip0::instr";
            string enaAddress = "TCPIP0::DESKTOP-SDJAN3S::8025::SOCKET";

            Initialization(Eq.Site[0].SwMatrix, enaAddress, String.Empty);
        }

        /// <summary>
        /// Load TCF and initialize NA equipment.
        /// </summary>
        /// <param name="sheetTrace"></param>
        /// <param name="sheetSegmentTable"></param>
        /// <param name="tcfSheetTrace2"></param>
        public void Initialization(TcfSheetTrace sheetTrace, TcfSheetSegmentTable sheetSegmentTable, Tuple<bool, string, string[,]> tcfSheetTrace2, string naStateFileName)
        {
            m_topazEquipment.SetTraceData(sheetTrace, sheetSegmentTable, tcfSheetTrace2);
            string enaAddress = "TCPIP0::localhost::hislip0::instr";
            //string enaAddress = "TCPIP0::DESKTOP-SDJAN3S::8025::SOCKET";

            Initialization(Eq.Site[0].SwMatrix, enaAddress, naStateFileName);
        }

        public void Initialization(EqSwitchMatrix.EqSwitchMatrixBase equipmentSw, string enaAddress, string naStateFileName)
        {
            StopWatchManager.Instance.Start(0);
            m_testCondFactory.Load(TcfConditionFbarTab2.Item3, TcfConditionFbarTab,
                ProjectSpecificDataObject.FirstTrigOfRaw, ProjectSpecificDataObject.listLNAReg);
            //Results = m_testCondFactory.Get_Results();

            if (m_testCondFactory.IsHasFbarTestCondition)
            {
                

                m_topazEquipment.InitEquipment(enaAddress);

                m_isDivaInstrument = m_topazEquipment.GetVNAEquipmentType();

                //ChoonChin - Change statefile name for DIVA
                if (m_isDivaInstrument)
                {
                    string[] ENAState = naStateFileName.Split('.');
                    string Prefix = ENAState[0];
                    naStateFileName = Prefix + "_DIVA.csa";
                }

                #region "Copy NA Statefile"

                //ChoonChin - 20191204 - Change to copy statefile here:
                ClothoConfigurationDataObject m_doClotho2 = new ClothoConfigurationDataObject();
                string StateFileFolderPath = string.Format(@"C:\Users\Public\Documents\Network Analyzer\{0}",
                naStateFileName);
                string NewStateFilePath = string.Format("{0}FileNeeded\\{1}", m_doClotho2.ClothoRootDir,
                    naStateFileName);

                if (!System.IO.File.Exists(StateFileFolderPath))
                {
                    System.IO.File.Copy(NewStateFilePath, StateFileFolderPath);
                    string msg = "New statefile copied to " + StateFileFolderPath;
                    LoggingManager.Instance.LogInfo(msg);
                }
                else
                {
                    string msg = "State file is " + StateFileFolderPath;
                    LoggingManager.Instance.LogInfo(msg);
                }

                #endregion "Copy NA Statefile"

                m_topazEquipment.NA_StateFile = naStateFileName;
                m_topazEquipment.Initialize(m_isDivaInstrument, ProjectSpecificDataObject.PortEnable, ProjectSpecificDataObject.CalColmnIndexNFset);
                //m_topazEquipment.InitializeFast(m_isDivaInstrument, ProjectSpecificDataObject.PortEnable, ProjectSpecificDataObject.CalColmnIndexNFset);
                m_testCondFactory.Fill(m_topazEquipment.DataModel, m_topazEquipment.ENAEquipmentDriver);
                CalibrationModel = new CalibrationModel();
                CalibrationModel3 calModel = new CalibrationModel3();
                calModel.Initialize(m_topazEquipment.DataModel, m_topazEquipment.ENAEquipmentDriver, Eq.Site[0].SwMatrix);
                CalibrationModel.Initialize(calModel);
                CalibrationModel.DataObject = CreateCalibrationInput(naStateFileName);
                CalibrationModel.SetInstrumentDiva(m_isDivaInstrument);
            }

            m_testCondFactory.Fill();
            TestConditionCollection = m_testCondFactory.TestConditionCollection;
            StopWatchManager.Instance.Stop(0);
        }
               
        #region Test Runner

        private int m_testIndex;
        private UsePreviousTestCacheModel m_upm;

        public void Initialize()
        {
            m_testCondFactory.Clear_Results();
            SparamDelta.Genspec.Clear();
            FileManager.Clear_Results();

            FbarTestTime = new List<double>();

            m_testIndex = 0;
            m_upm = new UsePreviousTestCacheModel();
        }

        public void Run(TestConditionDataObject tc)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            m_upm.SetCurrentTest(tc, m_testIndex);
            RunTest(m_testIndex, tc, m_upm);
            //TestTime[Test] = (double)watch.ElapsedTicks;
            m_testIndex++;

            watch.Stop();
            FbarTestTime.Add(watch.ElapsedMilliseconds);
        }

        public void DoPostRunUnInit()
        {
            //if (DC_Bias_Flag) DC.No_Bias(); //DC Power
            //tmpUnit_No++;
            FileManager.DisableSnpFile();
            HSDIO.FirstScriptNA = true;
            Results = m_testCondFactory.Get_Results();
        }

        public void RunTest(int testIndex, TestConditionDataObject tc,
            UsePreviousTestCacheModel upm)
        {
            Stopwatch watch = new Stopwatch();

            bool isExecuted = true;
            switch (tc.TestModeColumn)
            {
                case "FBAR":
                    isExecuted = RunTestFbarTrigger(testIndex, tc);
                    if (!isExecuted)
                    {
                        try
                        {
                            isExecuted = RunTestFbarMagBetween(tc, upm);
                        }
                        catch (Exception)
                        {
                            string whatiswrong = "";
                            //throw;
                        }
                    }
                    // Temp: RunTestFbarMagBetween would have run what RunTestFbarMeasure run.
                    if (!isExecuted)
                    {
                        isExecuted = RunTestFbarMeasure(tc);
                    }
                    FileManager.SetTraceResult();
                    break;

                case "DC":
                case "COMMON":
                    isExecuted = RunTestDcAndCommon(testIndex, tc, upm);
                    break;

                default:
                    isExecuted = false;
                    break;
            }

            watch.Stop();
            double FbaTestTime11 = watch.Elapsed.TotalMilliseconds;
            //FbarTestTime[testIndex] = FbaTestTime11;
        }

        private bool RunTestFbarTrigger(int Test, TestConditionDataObject tc)
        {
            bool isExecuted = true;
            SparamTrigger tt = tc.TestConditionLine as SparamTrigger;

            switch (tc.TestParameterColumn)
            {
                case "TRIGGER":
                    
                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.m_modelTraceManager = FileManager;
                    TraceSaverMediator traceSaveModel = new TraceSaverMediator(tt, FileManager, m_topazEquipment.DataModel,
                        m_testCondFactory.DataTriggeredModel);
                    tt.TraceFileSnpSavePath = traceSaveModel.GetSnpOutputFilePath();
                    tt.RunTest();
                    traceSaveModel.SaveTrace(tt.ChannelNumber, Test);
                    m_testCondFactory.DataTriggeredModel.IncrementTrigger();
                    FileManager.IncrementFileCount(tt);
                    break;

                case "TRIGGER_NF":    // For Topaz NF test 20160615
                    if (Test == 1)
                    {
                        Thread.Sleep(100);
                    }

                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.RunTest_NF();   // only single trigger
                    FileManager.IncrementFileCount(tt);
                    //tt.FileOutput_Counting++;
                    break;

                case "TRIGGER_NF_DUAL":    // For Topaz NF DUAL test 20161215
                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.RunTest_NF_DUAL();
                    FileManager.IncrementFileCount(tt);
                    break;

                case "TRIGGER2":
                    tt.m_modelTraceManager = FileManager;
                    tt.RunTest();
                    break;

                default:
                    isExecuted = false;
                    break;
            }
            return isExecuted;
        }
        private bool RunTestFbarMeasure(TestConditionDataObject tc)
        {
            TestCaseBase tc2 = tc.TestConditionLine as TestCaseBase;
            bool isValid = tc2 != null;
            if (!isValid) return false;

            tc2.RunTest();
            return true;
        }

        /// <summary>
        /// Handle MagBetween Use_Gain special case.
        /// </summary>
        private bool RunTestFbarMagBetween(TestConditionDataObject tc,
            UsePreviousTestCacheModel upm)
        {
            cMag_Between tcMagBetween = tc.TestConditionLine as cMag_Between;
            if (tcMagBetween == null) return false;
            tcMagBetween.RunTest();
            if (tcMagBetween.Use_Gain.ToUpper() == "V")
            {
                upm.SetUseGain(tcMagBetween.GetResult());
            }
            return true;
        }

        private bool RunTestDcAndCommon(int Test, TestConditionDataObject tc, UsePreviousTestCacheModel upm)
        {
            bool isExecuted = true;

            switch (tc.TestModeColumn)
            {
                case "DC":
                    FbarDcTestFactory dcTestFactory = tc.TestConditionLine as FbarDcTestFactory;
                    switch (tc.TestParameterColumn)
                    {
                        case "DC_SETTINGS":
                        case "DC_SETTING":
                            dcTestFactory.RunTest2(Test);
                            //  Thread.Sleep(100);
                            break;

                        case "TEMP":
                        case "MIPI":
                        case "DC_CURRENT":
                            dcTestFactory.RunTest2(Test);
                            break;
                    }
                    break;

                case "COMMON":
                    switch (tc.TestParameterColumn)
                    {
                        case "DELTA":
                        case "PHASE_DELTA":
                        case "PHASE_DELTA_GEN":
                        case "RL_DELTA_GEN":
                        case "GAIN_DELTA_GEN":
                        case "NF_DELTA":
                            SparamDelta ttd = tc.TestConditionLine as SparamDelta;

                            ttd.Relative_Use_Gain = upm.GetUseGain();
                            ttd.PreviousResult_1 = upm.PreviousTest_1.GetResult();
                            ttd.PreviousResult_2 = upm.PreviousTest_2.GetResult();
                            ttd.RunTest();
                            break;

                        case "RELATIVE_GAIN":
                            // Assign arbitrary result into Relative Gain.
                            SparamRelativeGainDelta ttrgd = tc.TestConditionLine as SparamRelativeGainDelta;
                            TestConditionDataObject pv1 = m_testCondFactory.GetItem(ttrgd.Previous_Test_1);
                            s_Result pv1Result = new s_Result();
                            if (pv1 != null)
                            {
                                pv1Result = pv1.Get_Result(-1);
                            }
                            ttrgd.Previous_Test_1_Result = pv1Result;
                            pv1 = m_testCondFactory.GetItem(ttrgd.Previous_Test_2Int);
                            pv1Result = new s_Result();
                            if (pv1 != null)
                            {
                                pv1Result = pv1.Get_Result(-1);
                            }
                            ttrgd.Previous_Test_2_Result = pv1Result;
                            ttrgd.RunTest();
                            break;

                        case "SUM":
                            SparamSum ttsum = tc.TestConditionLine as SparamSum;
                            ttsum.RunTest();
                            break;
                    }
                    //Results[Test] = COMMON.Result_setting[Test];
                    break;

                default:
                    isExecuted = false;
                    break;
            }

            //Results[Test] = DC.Result_setting[Test];
            return isExecuted;
        }

        #endregion

        #region To Be obsoleted
/*

        [Obsolete("has project specific code.")]
        public void Run_Tests()
        {
            m_testCondFactory.Clear_Results();
            SparamDelta.Genspec.Clear();     
            FileManager.Clear_Results();

            Stopwatch watch = new Stopwatch();
            Stopwatch watch1 = new Stopwatch();
            watch1.Reset();
            watch1.Start();
            TestTime = new List<double>();
            FbarTestTime = new List<double>();

            int testIndex = 0;
            UsePreviousTestCacheModel upm = new UsePreviousTestCacheModel();

            foreach (TestConditionDataObject tc in m_testCondFactory.TestConditionCollection)
            {
                upm.SetCurrentTest(tc, testIndex);
                watch.Reset();
                watch.Start();

                RunTest3(testIndex, tc, upm);

                watch.Stop();
                //TestTime[Test] = (double)watch.ElapsedTicks;
                FbarTestTime.Add(watch.ElapsedMilliseconds);
                testIndex++;
            }
            //if (DC_Bias_Flag) DC.No_Bias(); //DC Power
            //tmpUnit_No++;
            FileManager.DisableSnpFile();
            HSDIO.FirstScriptNA = true;
            Results = m_testCondFactory.Get_Results();
            watch1.Stop();
            long FbaTestTime = watch1.ElapsedMilliseconds;
        }

        public void RunTest3(int testIndex, TestConditionDataObject tc,
                    UsePreviousTestCacheModel upm)
        {
            Stopwatch watch = new Stopwatch();

            bool isExecuted = true;
            switch (tc.TestModeColumn)
            {
                case "FBAR":
                    if (cProject is Hallasan2RF2)
                    {
                        isExecuted = RunTestFbarTrigger3(testIndex, tc);
                    }
                    if (cProject is JokerFull || cProject is JokerRF2 || cProject is PinotRF2)
                    {
                        isExecuted = RunTestFbarTriggerJoker(testIndex, tc);
                    }
                    if (!isExecuted)
                    {
                        isExecuted = RunTestFbarMagBetween(tc, upm);
                    }
                    // Temp: RunTestFbarMagBetween would have run what RunTestFbarMeasure run.
                    if (!isExecuted)
                    {
                        isExecuted = RunTestFbarMeasure(tc);
                    }
                    FileManager.SetTraceResult();
                    break;

                case "DC":
                case "COMMON":
                    isExecuted = RunTestDcAndCommon(testIndex, tc, upm);
                    break;

                default:
                    isExecuted = false;
                    break;
            }

            watch.Stop();
            double FbaTestTime11 = watch.Elapsed.TotalMilliseconds;
            //FbarTestTime[testIndex] = FbaTestTime11;
        }

        private bool RunTestFbarTrigger3(int Test, TestConditionDataObject tc)
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

                    StopWatchManager.Instance.Start("Trigger_Run_SW");
                    // Case HLS2
                    //cProject.SetSwitchMatrixPaths(tt.Band, tt.Select_RX);
                    cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);
                    StopWatchManager.Instance.Stop("Trigger_Run_SW");
                    //Thread.Sleep(20); // This sleep is due to the switching time for the ZTM-15 switchbox.  This should be removed if no longer using the ZTM-15

                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.m_modelTraceManager = FileManager;

                    string tn = String.Format("Trigger_RunTest_{0}", Test);
                    StopWatchManager.Instance.StartTest(tn, "TRIGGER");
                    tt.RunTest();
                    StopWatchManager.Instance.Stop(tn);

                    //tt.FileOutput_Counting = SNP_Cumulative_Count;
                    //Inside .Trigger.RunTest(), the snp file was saved.
                    //Evaluation
                    FileManager.IncrementFileCount(tt);

                    break;

                case "TRIGGER_NF":    // For Topaz NF test 20160615

                    // Case HLS2
                    //cProject.SetSwitchMatrixPaths(tt.Band, tt.Select_RX);
                    cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);
                    if (Test == 1)
                    {
                        Thread.Sleep(100);
                    }

                    FileManager.SetTriggerOnFirstRun(Test, tt);

                    string tn2 = String.Format("Trigger_RunTestNF_{0}", Test);
                    StopWatchManager.Instance.StartTest(tn2, "TRIGGERNF");
                    tt.RunTest_NF();   // only single trigger
                    StopWatchManager.Instance.Stop(tn2);

                    FileManager.IncrementFileCount(tt);

                    //tt.FileOutput_Counting++;
                    break;

                case "TRIGGER_NF_DUAL":    // For Topaz NF DUAL test 20161215
                    //Run_SW(tt.Band, tt.Master_RX);
                    //Run_SW(tt.Band, tt.Slave_RX);
                    // Joker run this, HLS2 does not.
                    cProject.SetSwitchMatrixPaths(tt.Band, tt.Master_RX);
                    cProject.SetSwitchMatrixPaths(tt.Band, tt.Slave_RX);

                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.RunTest_NF_DUAL();
                    FileManager.IncrementFileCount(tt);
                    break;

                case "TRIGGER2":
                    tt.m_modelTraceManager = FileManager;
                    tt.RunTest();
                    break;

                default:
                    isExecuted = false;
                    break;
            }
            return isExecuted;
        }

        /// <summary>
        /// TODO Differs only in the cProject.SetSwitchMatrixPaths.
        /// </summary>
        private bool RunTestFbarTriggerJoker(int Test, TestConditionDataObject tc)
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

                    StopWatchManager.Instance.Start("Trigger_Run_SW");
                    //Run_SW2(tt.Band, tt.Select_RX);
                    cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);
                    StopWatchManager.Instance.Stop("Trigger_Run_SW");
                    //Thread.Sleep(20); // This sleep is due to the switching time for the ZTM-15 switchbox.  This should be removed if no longer using the ZTM-15

                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.m_modelTraceManager = FileManager;

                    string tn = String.Format("Trigger_RunTest_{0}", Test);
                    StopWatchManager.Instance.StartTest(tn, "TRIGGER");
                    tt.RunTest();
                    StopWatchManager.Instance.Stop(tn);

                    //FBAR.TestClass[Test].Trigger.FileOutput_Counting = SNP_Cumulative_Count;
                    ///Inside .Trigger.RunTest(), the snp file was saved.
                    ///Evaluation

                    FileManager.IncrementFileCount(tt);
                    break;

                case "TRIGGER2":
                    tt.RunTest();
                    break;

                case "TRIGGER_NF":    // For Topaz NF test 20160615

                    //m_modelSw.Run_SW_NF2(tt.Band, tt.Select_RX);
                    cProject.SetSwitchMatrixPaths(tt.SwitchIn, tt.SwitchAnt, tt.SwitchOut);
                    if (Test == 1)
                    {
                        Thread.Sleep(100);
                    }

                    FileManager.SetTriggerOnFirstRun(Test, tt);

                    string tn2 = String.Format("Trigger_RunTestNF_{0}", Test);
                    StopWatchManager.Instance.StartTest(tn2, "TRIGGERNF");
                    tt.RunTest_NF();   // only single trigger
                    StopWatchManager.Instance.Stop(tn2);

                    FileManager.IncrementFileCount(tt);

                    //tt.FileOutput_Counting++;
                    break;

                case "TRIGGER_NF_DUAL":    // For Topaz NF DUAL test 20161215
                    //Run_SW(tt.Band, tt.Master_RX);
                    //Run_SW(tt.Band, tt.Slave_RX);
                    // Joker run this, HLS does not.
                    cProject.SetSwitchMatrixPaths(tt.Band, tt.Master_RX);
                    cProject.SetSwitchMatrixPaths(tt.Band, tt.Slave_RX);

                    FileManager.SetTriggerOnFirstRun(Test, tt);
                    tt.FileOutput_Unit = FileManager.tmpUnit_No;
                    tt.RunTest_NF_DUAL();
                    tt.FileOutput_Counting++;
                    break;

                default:
                    isExecuted = false;
                    break;
            }

            return isExecuted;
        }
*/

        #endregion To Be obsoleted

        //ChoonChin - For Topaz temperature read back
        public string TopazTempValue(int ModuleNo)
        {
            return m_topazEquipment.ReadTopazTemp(ModuleNo);
        }

        public double Temp_Topaz()
        {
            return m_topazEquipment.Temp_Topaz();
        }

        //KCC - Added for Clotho Uninit
        public void UnInit()
        {
            //if (Test_Parameters != null)
            //{
            //    Test_Parameters.Clear();
            //}
        }

        private SParaCalibrationDataObject CreateCalibrationInput(string naStateFileName)
        {
            SParaCalibrationDataObject c = new SParaCalibrationDataObject();
            c.TopazCalPower = ProjectSpecificFactor.TopazCalPower;
            c.NA_StateFile = naStateFileName;
            return c;
        }


    }
}