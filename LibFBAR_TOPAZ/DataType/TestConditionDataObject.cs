using LibFBAR_TOPAZ.ANewTestLib;
using LibFBAR_TOPAZ.DataType;

namespace LibFBAR_TOPAZ
{
    public class TestConditionDataObject
    {
        private string m_colTestMode;
        private string m_colTestParameter;

        public int Index { get; set; }

        /// <summary>
        /// FBAR, DC
        /// </summary>
        public string TestModeColumn
        {
            get { return m_colTestMode;}
            set { m_colTestMode = value.ToUpper(); }
        }
        /// <summary>
        /// Trigger, MagBetween...
        /// </summary>
        public string TestParameterColumn
        {
            get { return m_colTestParameter; }
            set { m_colTestParameter = value.ToUpper(); }
        }

        public object TestConditionLine { get; set; }

        public void Initialize()
        {
            switch (TestModeColumn)
            {
                case "FBAR":
                    TestCaseBase tc1 = TestConditionLine as TestCaseBase;
                    tc1.InitSettings();
                    break;
                case "DC":
                    IDcMipiOtpTestCase tc2 = TestConditionLine as IDcMipiOtpTestCase;
                    switch (TestParameterColumn)
                    {
                        case "DC_SETTINGS":
                        case "DC_SETTING":
                            tc2.InitSettings_Pxi(Index, TestParameterColumn);
                            break;
                    }
                    break;
                case "COMMON":
                    break;
                case "X":
                    //Do nothing
                    break;
            }
        }

        public s_Result Get_Result(int testIndex)
        {
            s_Result r = new s_Result();

            switch (TestModeColumn)
            {
                case "FBAR":
                    TestCaseBase tc1 = TestConditionLine as TestCaseBase;
                    r = tc1.GetResult();
                    break;
                case "DC":
                    IDcMipiOtpTestCase tc2 = TestConditionLine as IDcMipiOtpTestCase;
                    r = tc2.GetResult(testIndex);
                    break;
                case "COMMON":
                    TestCaseCalcBase tc3 = TestConditionLine as TestCaseCalcBase;
                    r = tc3.Get_Result();
                    break;
                case "X":
                    //Do nothing
                    break;
            }

            return r;
        }

        public void Clear_Results()
        {
            switch (TestModeColumn)
            {
                case "FBAR":
                    TestCaseBase tc1 = TestConditionLine as TestCaseBase;
                    tc1.Clear_Results();
                    break;
                case "DC":
                    IDcMipiOtpTestCase tc2 = TestConditionLine as IDcMipiOtpTestCase;
                    tc2.Clear_Results();
                    break;
                case "COMMON":
                    TestCaseCalcBase tc3 = TestConditionLine as TestCaseCalcBase;
                    tc3.Clear_Results();
                    break;
                case "X":
                    //Do nothing
                    break;
            }
        }
    }
}