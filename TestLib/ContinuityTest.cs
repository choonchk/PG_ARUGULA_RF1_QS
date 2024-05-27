using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using EqLib;
using GuCal;
using ClothoLibAlgo;
using System.Diagnostics;


namespace TestLib
{

    public class ContinuityTest : iTest
    {
        public bool Initialize(bool finalScript)
        {
            //Eq.Site[Site].HSDIO.AddVectorsToScript(TestCon.MipiCommands, finalScript);  Not supported
            return true;
        }

        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;

        public byte Site;
        public ContinuityTestConditions TestCon = new ContinuityTestConditions();
        public ContinuityTestResults TestResult;


        public int RunTest()
        {
            try
            {
                TestResult = new ContinuityTestResults();

                if (ResultBuilder.headerFileMode) return 0;

                this.ConfigureVoltageAndCurrent();
                Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
                //MipiVio();
                Setup();
                Thread.Sleep(TestCon.DelayCurrent);
                Measure();
                foreach (string pinName in TestCon.DcSettings.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        ResultBuilder.AddResult(Site, pinName + "_" + TestCon.TestParaName, "V", !ResultBuilder.headerFileMode ? TestResult.Vmeas[pinName] : 0, 6);
                    }
                }
                return 0;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }


        }

        private void Setup()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    Eq.Site[Site].DC[pinName].SetupContinuity(TestCon.DcSettings[pinName].Current);
                }
            }
        }

        private void Measure()
        {
            Stopwatch stopwatchPA = new Stopwatch();

            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    stopwatchPA.Reset();

                    TestResult.Vmeas[pinName] = Eq.Site[Site].DC[pinName].MeasureContinuity(TestCon.DcSettings[pinName].Avgs);

                    double time1 = stopwatchPA.ElapsedMilliseconds;

                }
            }
        }
        private void MipiVio()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    if (pinName != "Vio") Eq.Site[Site].DC[pinName].PreLeakageTest(TestCon.DcSettings[pinName]);

                    if (pinName.Contains("sh"))
                    {
                        Eq.Site[Site].DC[pinName].SetupContinuity(TestCon.DcSettings[pinName].Current);
                        Thread.Sleep(1);
                        TestResult.Vmeas[pinName] = Eq.Site[Site].DC[pinName].MeasureContinuity(1);
                    }
                    else
                    {
                        Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, TestCon.DcSettings[pinName].Current);
                        //Thread.Sleep(1);
                        TestResult.Vmeas[pinName] = Eq.Site[Site].DC[pinName].MeasureCurrent(TestCon.DcSettings[pinName].Avgs);
                        Eq.Site[Site].DC[pinName].ForceVoltage(0, TestCon.DcSettings[pinName].Current);   
                    }
                   
                     Eq.Site[Site].DC[pinName].PostLeakageTest();
                    if (pinName == "Vio")  Eq.Site[Site].DC[pinName].ForceVoltage(1.8, 0.1);
                }
            }
        }
        public void BuildResults(ref ATFReturnResult results)
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    ResultBuilder.AddResult(Site, pinName + "_" + TestCon.TestParaName, "V", !ResultBuilder.headerFileMode ? TestResult.Vmeas[pinName] : 0, 6);
                }
            }
        }

        private void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue;// don't force voltage on MIPI pins

                //if (TestCon.DcSettings[pinName].Test) continue;  // don't set voltage if we're testing Continuity on this pin

                Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, TestCon.DcSettings[pinName].Current);
            }
        }
    }

    public class ContinuityTestConditions
    {
        public string TestParaName;
        public int DelayCurrent;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
    }

    public class ContinuityTestResults
    {
        public Dictionary<string, double> Vmeas = new Dictionary<string, double>();
    }
}
