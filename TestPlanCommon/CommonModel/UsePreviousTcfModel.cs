using LibFBAR_TOPAZ;
using LibFBAR_TOPAZ.DataType;

namespace TestPlanCommon.CommonModel
{
    // TODO Redesign needed on UsePreviousTcfModel.
    /// <summary>
    /// Handle Use_Previous column. Store Relative_Use_Gain which is marked by V in Use_Previous. 
    /// </summary>
    /// <remarks>TestFactory-Load() will assign the Previous_Test to tests. Then, will set the test as Previous_Test for the next run. (this design should obsolete)</remarks>
    /// <remarks>TestManager-RunTest() will assign Previous_Test as tests are ran.</remarks>
    /// <remarks>Delta test will take Relative_Use_Gain minus the Previous_Test.</remarks>
    /// <remarks>Mag_At and FreqAt tests will use the Previous_Test.</remarks>
    /// 
    public class UsePreviousTcfModel
    {
        public bool b_Use_Previous;
        /// <summary>
        /// Used by TestFactory-Load() - Relative Gain.
        /// </summary>
        public int Previous_Test_1;
        public int Previous_Test_2;
        /// <summary>
        /// Set the previous testNo into the test case during TestFactory-Load().
        /// </summary>
        public int Previous_TestNo;
        public string[] tmp_Info;
        /// <summary>
        /// Used by Mag_At and FreqAt.
        /// </summary>
        public TestCaseBase Previous_Test;
        /// <summary>
        /// Set in RunTest-Mag_Between. use by NF_Delta test also by the way. Store Relative_Use_Gain which is marked by V in Use_Previous. 
        /// </summary>
        private s_Result Relative_Use_Gain;

        public void Configure(TestConditionDataObject tc, int TestCnt, string tcisUsePrevious)
        {
            b_Use_Previous = false;
            string tmpPreviousStr;   // {Theading} - check for Use Previous Test to avoid crashes in Multithreading
            if (tc.TestModeColumn.ToUpper() != "COMMON")
            {
                tmpPreviousStr = tcisUsePrevious;

                if (tmpPreviousStr == "")
                {
                    Previous_TestNo = TestCnt;
                }
                else if (tmpPreviousStr.ToUpper() != "V")
                {
                    // This case will get an arbitrary test number.
                    Previous_TestNo = int.Parse(tmpPreviousStr);
                    b_Use_Previous = true;
                }
                else if (tmpPreviousStr.ToUpper() == "V")
                {
                    b_Use_Previous = true;
                }
            }
            else if (tc.TestParameterColumn.ToUpper() == "DELTA")
            {
                Previous_TestNo = TestCnt;
            }
            else if (tc.TestParameterColumn.ToUpper() == "PHASE_DELTA")
            {
                tmpPreviousStr = tcisUsePrevious;
                Previous_TestNo = TestCnt;
            }
            else
            {
                Previous_TestNo = TestCnt;
            }

            Configure3(tc, TestCnt);
        }

        public int GetPreviousTestNo(int TestCnt)
        {
            if (Previous_TestNo == TestCnt)
            {
                return -1;      // No previous test set.
            }
            else
            {
                return Previous_TestNo - 1;     // zero based index.
            }
        }

        public s_Result PreviousResult_1;
        public s_Result PreviousResult_2;
        public s_Result CurrentResult;

        private void Configure3(TestConditionDataObject tc, int TestCnt)
        {
            PreviousResult_2 = PreviousResult_1;
            PreviousResult_1 = CurrentResult;
            CurrentResult = tc.Get_Result(TestCnt);
        }

        /// <summary>
        /// Set in RunTest-Mag_Between. Memorize Mag_Between result marked with V.
        /// </summary>
        public void SetUseGain(s_Result useGainResult)
        {
            Relative_Use_Gain = useGainResult;
        }

        /// <summary>
        /// Retrieved by NF_Delta in RunTest. From Mag_Between.
        /// </summary>
        /// <returns></returns>
        public s_Result GetUseGain()
        {
            return Relative_Use_Gain;
        }
    }

    /// <summary>
    /// Redesign of UsePreviousTcfModel.
    /// </summary>
    public class UsePreviousTestCacheModel
    {
        /// <summary>
        /// Set in RunTest-Mag_Between. use by NF_Delta test also by the way. Store Relative_Use_Gain which is marked by V in Use_Previous. 
        /// </summary>
        private s_Result Relative_Use_Gain;

        public void SetCurrentTest(TestConditionDataObject currentTest, int testIndex)
        {
            PreviousTest_2 = PreviousTest_1;
            PreviousTest_1 = CurrentTest;

            TestCaseBase tc = currentTest.TestConditionLine as TestCaseBase;
            if (tc == null)     // Delta & DC will be null.
            {
                tc = new TestCaseBase();
            }
            CurrentTest = tc;

            PreviousTest_2Index = PreviousTest_1Index;
            PreviousTest_1Index = CurrentTest_Index;
            CurrentTest_Index = testIndex;
        }

        public TestCaseBase PreviousTest_1 { get; set; }
        public TestCaseBase PreviousTest_2 { get; set; }
        private TestCaseBase CurrentTest { get; set; }
        public int PreviousTest_1Index { get; set; }
        public int PreviousTest_2Index { get; set; }
        private int CurrentTest_Index { get; set; }

        /// <summary>
        /// Set in RunTest-Mag_Between. Memorize Mag_Between result marked with V.
        /// </summary>
        public void SetUseGain(s_Result useGainResult)
        {
            Relative_Use_Gain = useGainResult;
        }

        /// <summary>
        /// Retrieved by NF_Delta in RunTest. From Mag_Between.
        /// </summary>
        /// <returns></returns>
        public s_Result GetUseGain()
        {
            return Relative_Use_Gain;
        }
    }


}
