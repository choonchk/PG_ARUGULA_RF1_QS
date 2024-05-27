using System;
using System.Collections.Generic;
using LibFBAR_TOPAZ;
using LibFBAR_TOPAZ.ANewEqLib;
using LibFBAR_TOPAZ.ANewSParamTestLib;
using LibFBAR_TOPAZ.ANewTestLib;
using LibFBAR_TOPAZ.DataType;
using MPAD_TestTimer;
using TestPlanCommon.CommonModel;

namespace TestPlanCommon.SParaModel
{
    /// <summary>
    /// Creates FBAR, DC and Common test from test conditions.
    /// </summary>
    public class SParaTestFactory
    {
        private FbarTestFactory3 m_factoryFbar;
        private FbarCommonTestFactory m_factoryCommon;
        private FbarDcTestFactory DC;
        private TcfHeaderGenerator m_modelHeader;

        /// <summary>
        /// For Golden Eagle compliant result header.
        /// </summary>
        public SortedDictionary<int, string> HeaderEntries
        {
            set
            {
                m_modelHeader.SetHeaderEntries(value);
            }
        }

        public DataTriggeredDataModel DataTriggeredModel
        {
            get { return m_factoryFbar.DataTriggered2; }
        }

        public SParaTestFactory()
        {
            m_modelHeader = new TcfHeaderGenerator();
        }

        public List<TestConditionDataObject> TestConditionCollection { get; set; }
        public void Load(string[,] sheetCondFbar, List<Dictionary<string, string>> DicTestCondTempNA, int cProjectFirstTrigOfRaw, string[] listLNAReg)
        {
            Initialize(sheetCondFbar);

            var TestCnt = 0;

            int zforeachcounter = 0;
            //m_factoryFbar.Reset();  // reset the DataTRigger.
            UsePreviousTcfModel upm = new UsePreviousTcfModel();

            foreach (Dictionary<string, string> TestCond in DicTestCondTempNA)
            {
                if (zforeachcounter == 10)
                {
                    int y = 0;
                }

                m_modelHeader.SetCurrentTestCondition(TestCond);
                SParaEnaTestConditionReader reader = new SParaEnaTestConditionReader(TestCond);

                TestConditionDataObject tc = new TestConditionDataObject();
                tc.TestModeColumn = m_modelHeader.ReadTcfData("Test Mode");
                tc.TestParameterColumn = m_modelHeader.ReadTcfData("Test Parameter");

                try
                {
                    Load(tc, TestCnt, TestCond, upm, reader, listLNAReg);
                    zforeachcounter++;
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Error in Processing Test Parameter : {0}\r\n{1}\r\n{2}", tc.TestModeColumn, tc.TestParameterColumn, zforeachcounter);
                    ValidationDataObject vdo = new ValidationDataObject(msg, "Unrecognized Test Parameter", ex);
                    PromptManager.Instance.ShowError(vdo);
                }

                // Set the previous test.
                SetPreviousTest(upm, tc, TestCnt);
                // Setting Up Data Triggering and Grabbing Mechanism
                string tcSParam = m_modelHeader.ReadTcfData("S-Parameter").ToUpper();
                //m_factoryFbar.SetupDataTriggerObject(tp, TestCnt, cProject.FirstTrigOfRaw,
                //    TestCondition_Test, tcSParam);
                m_factoryFbar.SetupDataTriggerObject(tc, TestCnt, cProjectFirstTrigOfRaw,
                    tcSParam, this);
                if (tc.TestParameterColumn != "TEMP")
                {
                    TestCnt++;
                }
            }
            m_factoryFbar.FinalizeDataTriggerObject();
            Fill(m_factoryFbar.DataTriggered2);

        }
        public void Load(TestConditionDataObject tc2, int TestCnt, Dictionary<string, string> TestCond, UsePreviousTcfModel upm, SParaEnaTestConditionReader reader, string[] listLNAReg)
        {
            bool isLoaded = false;

            switch (tc2.TestModeColumn)
            {
                case "FBAR":
                    SparamTrigger tt = m_factoryFbar.Load_TestConditionTrigger(tc2.TestParameterColumn,
                        TestCnt, reader);
                    isLoaded = tt != null;
                    if (isLoaded)
                    {
                        tc2.TestConditionLine = tt;
                        break;
                    }
                    TestCaseBase tcb = m_factoryFbar.Load_TestConditionMeasure(tc2.TestParameterColumn,
                        TestCnt, upm, m_modelHeader, reader);
                    isLoaded = tcb != null;
                    if (isLoaded)
                    {
                        //CCThai Workaround to support number for FREQ_AT use previous
                        string upNumber = reader.ReadTcfData("Use_Previous");
                        tcb.Previous_Test = SetPreviousTest(tc2.TestParameterColumn, upNumber, TestCnt);
                        tc2.TestConditionLine = tcb;
                        break;
                    }
                    tcb = m_factoryFbar.Load_TestConditionUnused(tc2.TestParameterColumn,
                        TestCnt, upm, m_modelHeader, reader);
                    isLoaded = tcb != null;
                    if (isLoaded)
                    {
                        tc2.TestConditionLine = tcb;
                    }
                    break;

                case "DC":
                    DCMipiOtpTestController dmoTest = m_factoryFbar.Load_TestConditionDc(tc2.TestParameterColumn,
                        TestCnt, TestCond, m_modelHeader, reader, listLNAReg);
                    DC.Create(dmoTest);
                    isLoaded = dmoTest != null;
                    if (isLoaded)
                    {
                        tc2.TestConditionLine = DC;
                    }
                    break;

                case "COMMON":
                    TestCaseCalcBase tcb2 = m_factoryCommon.Load_TestCondition(tc2.TestParameterColumn, upm, m_modelHeader, reader);
                    isLoaded = tcb2 != null;
                    if (isLoaded)
                    {
                        tc2.TestConditionLine = tcb2;
                        break;
                    }
                    tcb2 = m_factoryCommon.Load_TestConditionRelativeGain(tc2.TestParameterColumn, upm, m_modelHeader, reader);
                    isLoaded = tcb2 != null;
                    if (isLoaded)
                    {
                        tc2.TestConditionLine = tcb2;
                        break;
                    }
                    break;

                default:
                    break;
            }

            if (isLoaded)
            {
                Add(tc2);
                return;
            }

            // Cannot load.
            string msg = String.Format("Unknown TCF Test Mode:{0}, Test Parameter:{1}",
                tc2.TestModeColumn, tc2.TestParameterColumn);
            PromptManager.Instance.ShowError(msg);
        }
        private void Initialize(string[,] sheetCondFbar)
        {
            m_factoryFbar = new FbarTestFactory3();
            m_factoryCommon = new FbarCommonTestFactory();
            DC = new FbarDcTestFactory();

            DC.Load_DC_ChannelSettings();
            //COMMON.Init(TotalTest);
            m_factoryFbar.Initialize(sheetCondFbar);
            TestConditionCollection = new List<TestConditionDataObject>();

        }

        public void Fill()
        {
            foreach (TestConditionDataObject tc in TestConditionCollection)
            {
                tc.Initialize();
            }
        }

        public void Fill(SParameterMeasurementDataModel spDataModel, TopazEquipmentDriver equipment)
        {
            // Called after TestConditions are loaded.
            foreach (TestConditionDataObject tc in TestConditionCollection)
            {
                if (tc == null) continue;

                switch (tc.TestModeColumn)
                {
                    case "FBAR":
                        SparamTrigger tc1 = tc.TestConditionLine as SparamTrigger;
                        if (tc1 != null)
                        {
                            tc1.Initialize(spDataModel);
                            tc1.EquipmentENA = equipment;
                        }
                        cNF_Topaz_At tc2 = tc.TestConditionLine as cNF_Topaz_At;
                        if (tc2 != null)
                        {
                            tc2.Initialize(spDataModel);
                            tc2.EquipmentENA = equipment;
                        }
                        TestCaseBase tcb = tc.TestConditionLine as TestCaseBase;
                        if (tcb != null)
                        {
                            tcb.Initialize(spDataModel);
                        }
                        break;
                }
            }
        }

        public void Fill(DataTriggeredDataModel triggerDm)
        {
            // Called after TestConditions are loaded.
            foreach (TestConditionDataObject tc in TestConditionCollection)
            {
                if (tc == null) continue;

                switch (tc.TestModeColumn)
                {
                    case "FBAR":
                        SparamTrigger tc1 = tc.TestConditionLine as SparamTrigger;
                        if (tc1 != null)
                        {
                            tc1.InitializeTriggerData(triggerDm);
                        }
                        break;
                }

            }
        }

        public TestConditionDataObject GetItem(int testIndex)
        {
            if (TestConditionCollection.Count == 0 || testIndex >= TestConditionCollection.Count) return null;
            return TestConditionCollection[testIndex];
        }

        public List<s_Result> Get_Results()
        {
            List<s_Result> rl = new List<s_Result>();
            int testIndex = 0;
            foreach (TestConditionDataObject tc in TestConditionCollection)
            {
                s_Result r = tc.Get_Result(testIndex);
                rl.Add(r);
                testIndex++;
            }

            return rl;
        }

        public bool IsHasFbarTestCondition
        {
            get
            {
                foreach (TestConditionDataObject tc in TestConditionCollection)
                {
                    if (tc.TestModeColumn == "FBAR") return true;
                }
                return false;
            }
        }

        public void Clear_Results()
        {
            foreach (TestConditionDataObject tc in TestConditionCollection)
            {
                if (tc == null) continue;
                tc.Clear_Results();
            }

            DC.Clear_Results();
            //COMMON.Clear_Results();
        }

        /// <summary>
        /// Responsible to handle V set in Mag_Between (e.g. HLS2).
        /// </summary>
        /// <param name="upm"></param>
        /// <param name="tc"></param>
        /// <param name="TestCnt"></param>
        private void SetPreviousTest(UsePreviousTcfModel upm, TestConditionDataObject tc, int TestCnt)
        {
            string isUsePrevious = m_modelHeader.ReadTcfData("Use_Previous");
            upm.Configure(tc, TestCnt, isUsePrevious);
            int prevNo = upm.GetPreviousTestNo(TestCnt);
            if (prevNo == -1)
            {
                upm.Previous_Test = tc.TestConditionLine as TestCaseBase;
            }
            else
            {
                TestConditionDataObject prevTest = GetItem(prevNo);
                upm.Previous_Test = prevTest.TestConditionLine as TestCaseBase;
            }
        }

        /// <summary>
        /// Find a test number. (Ignore V). Responsible to handle number set in Freq_At (e.g. Pinot).
        /// </summary>
        /// <param name="usePreviousTcfString">V or one number. V will be ignored. Number will be processed.</param>
        /// <param name="TestCnt"></param>
        private TestCaseBase SetPreviousTest(string testParameterColumn, string usePreviousTcfString, int TestCnt)
        {
            TestCaseBase tcb = null;

            switch (testParameterColumn.ToUpper())
            {
                // Handle Use-Previous
                case "FREQ_AT":
                    if (String.IsNullOrEmpty(usePreviousTcfString) || usePreviousTcfString.ToUpper() == "V")
                    {
                        return tcb;
                    }
                    else
                    {
                        // Case is one number.
                        int prevTestNo = Convert.ToInt32(usePreviousTcfString) - 1;
                        TestConditionDataObject previousTest = GetItem(prevTestNo);
                        tcb = previousTest.TestConditionLine as TestCaseBase;
                        //string msg2 = String.Format("Use Previous test for test no {0} is set to {1}.", TestCnt, prevTestNo);
                        //MPAD_TestTimer.LoggingManager.Instance.LogInfo(msg2);
                    }

                    break;
            }

            return tcb;
        }
        private void Add(TestConditionDataObject tc)
        {
            TestConditionCollection.Add(tc);
        }
    }
}