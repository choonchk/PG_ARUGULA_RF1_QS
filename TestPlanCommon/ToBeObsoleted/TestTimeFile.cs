using System;
using System.Diagnostics;
using System.IO;
using TestLib;

namespace TestPlanCommon.ToBeObsoleted
{
    /// <summary>
    /// Used by TestRunner. To be replaced with common module StopWatchManager.
    /// </summary>
    public static class TestTimeFile
    {
        public static bool enableTestTimeRecord = false;
        public static StreamWriter sw = null;
        public static Stopwatch PAtesting = new Stopwatch();
        public static Stopwatch After_Runtest = new Stopwatch();
        public static long PAtime, FBARtime, BuildResultTime;
        public static string PAtimestr, FBARtimestr, BuildResultTimestr, After_Runteststr;
        public static Stopwatch FBARtesting = new Stopwatch();
        public static Stopwatch BuildResult = new Stopwatch();

        public static void Open2(int PID)
        {
            if (enableTestTimeRecord)
            {
                sw = new StreamWriter(@"C:\Avago.ATF.Common\Results\SN-" + PID + "_Test_Time_Log.csv", true);
                sw.Write("Category, Test, Test_Time (s), Tests Performed, Timestamp" + Environment.NewLine);
            }
        }
        public static void Open(string fileNamePrefix)
        {
            if (enableTestTimeRecord)
            {
                sw = new StreamWriter(@"C:\Avago.ATF.Common\Results\" + fileNamePrefix + "_Test_Time_Log.csv", false);
                sw.Write("Category, Test, Test_Time (ms), Tests Performed, Timestamp" + Environment.NewLine);
            }
        }

        public static void Pre_After_RunTest()
        {
            if (enableTestTimeRecord)
                After_Runtest.Start();
        }

        public static void PrePaTest()
        {
            if (enableTestTimeRecord)
                PAtesting.Start();
        }
        public static void PreBuildResult()
        {
            if (enableTestTimeRecord)
                BuildResult.Start();
        }

        public static void PostAfterRunTest()
        {
            if (enableTestTimeRecord)
            {

                After_Runtest.Stop();

                After_Runteststr = After_Runtest.Elapsed.ToString().Substring(6);

                sw.Write("After_Runtest," + "Null" + "," + After_Runteststr + ",");

                //if (test.TestPin) sw.Write("Pin-");
                //if (test.TestPout) sw.Write("Pout-");
                //if (test.TestGain) sw.Write("Gain-");

                //foreach (string pinName in PaTest.SmuResources.Keys)
                //{
                //    if (test.SmuSettings[pinName].Test)
                //    {
                //        sw.Write(test.SmuSettings[pinName].iParaName + "-");
                //    }
                //}

                //if (test.TestAcp) sw.Write("ACLR-");
                //if (test.TestH2) sw.Write("H2-");
                //if (test.TestH3) sw.Write("H3-");
                //if (test.TestCpl) sw.Write("Cpl-");
                //if (test.TestPae) sw.Write("PAE-");
                //if (test.TestEvm) sw.Write("EVM-");

                sw.Write("," + DateTime.Now.ToString() + Environment.NewLine);

                After_Runtest.Reset();
            }
        }

        public static void PostPaTest2(iTest test)
        {
            if (enableTestTimeRecord)
            {

                PAtesting.Stop();

                System.Type TestType = test.GetType();



                PAtimestr = PAtesting.Elapsed.ToString().Substring(6);

                sw.Write("PA," + TestType.FullName.ToString() + "," + PAtimestr + ",");

                //if (test.TestPin) sw.Write("Pin-");
                //if (test.TestPout) sw.Write("Pout-");
                //if (test.TestGain) sw.Write("Gain-");

                //foreach (string pinName in PaTest.SmuResources.Keys)
                //{
                //    if (test.SmuSettings[pinName].Test)
                //    {
                //        sw.Write(test.SmuSettings[pinName].iParaName + "-");
                //    }
                //}

                //if (test.TestAcp) sw.Write("ACLR-");
                //if (test.TestH2) sw.Write("H2-");
                //if (test.TestH3) sw.Write("H3-");
                //if (test.TestCpl) sw.Write("Cpl-");
                //if (test.TestPae) sw.Write("PAE-");
                //if (test.TestEvm) sw.Write("EVM-");

                sw.Write("," + DateTime.Now.ToString() + Environment.NewLine);

                PAtesting.Reset();
            }
        }
        public static void PostPaTest(iTest test)
        {
            if (enableTestTimeRecord)
            {

                PAtesting.Stop();

                System.Type TestType = test.GetType();


                PAtime = PAtesting.ElapsedMilliseconds;

                sw.Write("PA," + TestType.FullName.ToString() + "," + PAtime.ToString() + ",");

                //if (test.TestPin) sw.Write("Pin-");
                //if (test.TestPout) sw.Write("Pout-");
                //if (test.TestGain) sw.Write("Gain-");

                //foreach (string pinName in PaTest.SmuResources.Keys)
                //{
                //    if (test.SmuSettings[pinName].Test)
                //    {
                //        sw.Write(test.SmuSettings[pinName].iParaName + "-");
                //    }
                //}

                //if (test.TestAcp) sw.Write("ACLR-");
                //if (test.TestH2) sw.Write("H2-");
                //if (test.TestH3) sw.Write("H3-");
                //if (test.TestCpl) sw.Write("Cpl-");
                //if (test.TestPae) sw.Write("PAE-");
                //if (test.TestEvm) sw.Write("EVM-");

                sw.Write("," + DateTime.Now.ToString() + Environment.NewLine);

                PAtesting.Reset();
            }
        }

        public static void PostBuildResult2(iTest test)
        {
            if (enableTestTimeRecord)
            {

                BuildResult.Stop();

                System.Type TestType = test.GetType();



                BuildResultTimestr = BuildResult.Elapsed.ToString().Substring(6);

                sw.Write("BuildResult," + TestType.FullName.ToString() + "," + BuildResultTimestr + ",");

                //if (test.TestPin) sw.Write("Pin-");
                //if (test.TestPout) sw.Write("Pout-");
                //if (test.TestGain) sw.Write("Gain-");

                //foreach (string pinName in PaTest.SmuResources.Keys)
                //{
                //    if (test.SmuSettings[pinName].Test)
                //    {
                //        sw.Write(test.SmuSettings[pinName].iParaName + "-");
                //    }
                //}

                //if (test.TestAcp) sw.Write("ACLR-");
                //if (test.TestH2) sw.Write("H2-");
                //if (test.TestH3) sw.Write("H3-");
                //if (test.TestCpl) sw.Write("Cpl-");
                //if (test.TestPae) sw.Write("PAE-");
                //if (test.TestEvm) sw.Write("EVM-");

                sw.Write("," + DateTime.Now.ToString() + Environment.NewLine);

                BuildResult.Reset();
            }
        }

        public static void PostBuildResult(iTest test)
        {
            if (enableTestTimeRecord)
            {

                BuildResult.Stop();

                System.Type TestType = test.GetType();



                BuildResultTime = BuildResult.ElapsedMilliseconds;

                sw.Write("BuildResult," + TestType.FullName.ToString() + "," + BuildResultTime.ToString() + ",");

                //if (test.TestPin) sw.Write("Pin-");
                //if (test.TestPout) sw.Write("Pout-");
                //if (test.TestGain) sw.Write("Gain-");

                //foreach (string pinName in PaTest.SmuResources.Keys)
                //{
                //    if (test.SmuSettings[pinName].Test)
                //    {
                //        sw.Write(test.SmuSettings[pinName].iParaName + "-");
                //    }
                //}

                //if (test.TestAcp) sw.Write("ACLR-");
                //if (test.TestH2) sw.Write("H2-");
                //if (test.TestH3) sw.Write("H3-");
                //if (test.TestCpl) sw.Write("Cpl-");
                //if (test.TestPae) sw.Write("PAE-");
                //if (test.TestEvm) sw.Write("EVM-");

                sw.Write("," + DateTime.Now.ToString() + Environment.NewLine);

                BuildResult.Reset();
            }
        }
        public static void PreFbarTest()
        {
            if (enableTestTimeRecord)
                FBARtesting.Start();
        }

        public static void PostNATest(iTest NATest)
        {
            if (enableTestTimeRecord)
            {
                Legacy_FbarTest fbarTest = (Legacy_FbarTest)NATest;
                FBARtesting.Stop();
                FBARtime = FBARtesting.ElapsedMilliseconds;
                FBARtesting.Reset();
                sw.Write("FBAR," + fbarTest.TestParaName + "," + FBARtime.ToString() + ",," + DateTime.Now.ToString() + Environment.NewLine);

                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
            }
        }

        public static void Close()
        {
            if (enableTestTimeRecord)
                sw.Close();
        }
    }
}