using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EqLib;
using ClothoLibAlgo;
using GuCal;
using System.Diagnostics;
using System.Threading.Tasks;
using ProductionLib;

namespace TestLib
{
    public class DcTest : DcTestBase
    {
        public override int DcTestCore()
        {
            Thread.Sleep(TestCon.DelayCurrent);
            //Thread.Sleep(500);
            SetupAndMeasureDc();
            return 0;
        }

        public override void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue;
                string msg = String.Format("ForceVoltage on pin {0}", pinName);
                SwStartRun(msg, Site);
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

        public override void SetupCurrentMeasure1Chan(string pinName)
        {
            Eq.Site[Site].DC[pinName].SetupCurrentMeasure(0.0003, TriggerLine.None);
        }

        public override void MeasureCurrent1Chan(string pinName)
        {            
            TestResult.Imeas[pinName] = Eq.Site[Site].DC[pinName].MeasureCurrent(TestCon.DcSettings[pinName].Avgs);
        }
    }

    public class DcLeakageTest : DcTestBase
    {
        public override int DcTestCore()
        {
            try
            {

                foreach (string PinName in Eq.Site[Site].DC.Keys)
                {
                    string msg = String.Format("ForceVoltage on pin {0}", PinName);
                    SwStartRun(msg, Site);
                    Eq.Site[Site].DC[PinName].ForceVoltage(TestCon.DcSettings[PinName].Volts, TestCon.DcSettings[PinName].Current);  // set the current range here instead of earlier in ConfigureVoltageAndCurrent
                    SwStopRun(msg, Site);
                }
                //Thread.Sleep(TestCon.DelayCurrent); // For current settling before changing to lower current range
                foreach (string PinName in Eq.Site[Site].DC.Keys)
                {
                    if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName.ToUpper())) continue;
                    if (TestCon.DcSettings[PinName].Test)
                    {
                        string msg = String.Format("PreLeakageTest on pin {0}", PinName);
                        SwStartRun(msg, Site);
                        Eq.Site[Site].DC[PinName].PreLeakageTest(TestCon.DcSettings[PinName]);
                        SwStopRun(msg, Site);
                    }
                }

                //SwStartRun("DelayCurrent-50");
                ////Thread.Sleep(TestCon.DelayCurrent); // For current settling before changing to lower current range
                //Thread.Sleep(50); // For current settling before changing to lower current range
                //SwStopRun("DelayCurrent-50");

                SetupAndMeasureDc();

                foreach (string PinName in Eq.Site[Site].DC.Keys)
                {
                    if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName.ToUpper())) continue;
                    if (TestCon.DcSettings[PinName].Test)
                    {
                        string msg = String.Format("PostLeakageTest on pin {0}", PinName);
                        SwStartRun(msg, Site);
                        Eq.Site[Site].DC[PinName].PostLeakageTest();
                        SwStopRun(msg, Site);
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public override void ConfigureVoltageAndCurrent()
        {
            /*Original
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper()))
                {
                    if (pinName.ToUpper() != "VCC")
                    {
                        continue; // don't force voltage on MIPI pins
                    }
                }

                Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, 0.03);   // initially leave on higher current range so current doesn't clamp while settling.
            }
            */
            foreach (string pinName in TestCon.DcSettings.Keys)
            {

                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper()) && (TestCon.WaveformName.ToUpper() != "MIPISETHIGH")) continue; // don't force voltage on MIPI pins

                string msg = String.Format("ForceVoltage on pin {0}", pinName);
                SwStartRun(msg, Site);
                if (pinName.ToUpper().Contains("VIO")|| pinName.ToUpper().Contains("SCLK")|| pinName.ToUpper().Contains("SDATA"))
                {
                    Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, 0.0001);
                }
                else
                {
                    Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, 0.5);   // initially leave on higher current range so current doesn't clamp while settling.
                }
                SwStopRun(msg, Site);
            }
            Thread.Sleep(1); 
        }

        public override void SetupCurrentMeasure1Chan(string pinName)
        {
            Eq.Site[Site].DC[pinName].SetupCurrentTraceMeasurement((double)TestCon.DelayCurrent / 1000.0 + (double)TestCon.DcSettings[pinName].Avgs * 0.001, 500e-6, TriggerLine.None);
        }

        public override void MeasureCurrent1Chan(string pinName)
        {
            double[] dcLeakageTrace = Eq.Site[Site].DC[pinName].MeasureCurrentTrace();

            int skipPoints = (int)(dcLeakageTrace.Length * (double)TestCon.DelayCurrent / 1000.0 / ((double)TestCon.DelayCurrent / 1000.0 + (double)TestCon.DcSettings[pinName].Avgs * 0.001));
            skipPoints = Math.Min(skipPoints, dcLeakageTrace.Length - 1);
            TestResult.Imeas[pinName] = dcLeakageTrace.Skip(skipPoints).Average();
        }
    }

    public abstract class DcTestBase : TimingBase, iTest
    {
        public string MathVariable;
        public bool Initialize(bool finalScript)
        {
            string testName = TestCon.CktID + TestCon.TestParaName;
            InitializeTiming(testName);
            //Eq.Site[Site].HSDIO.AddVectorsToScript(TestCon.MipiCommands, finalScript);
            return true;
        }

        public byte Site;
        public DcTestConditions TestCon = new DcTestConditions();
        public DcTestResults TestResult;
        public bool Mordor = false;
        public HiPerfTimer uTimer = new HiPerfTimer();

        public int RunTest()
        {
            try
            {
                //Stopwatch TestTime1 = new Stopwatch();
                //TestTime1.Restart();
                //TestTime1.Start();

                TestResult = new DcTestResults();

                if (ResultBuilder.headerFileMode) return 0;

                SwBeginRun(Site);
                this.ConfigureVoltageAndCurrent();

                SwStartRun("SendMipiCommands", Site);
                if(TestCon.WaveformName.ToUpper() != "MIPISETHIGH")
                {
                    Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
                }

                SwStopRun("SendMipiCommands", Site);

                if (Eq.Site[Site].RF != null)
                {
                    SwStartRun("SGAbort", Site);
                    Eq.Site[Site].RF.SG.Abort();
                    SwStopRun("SGAbort", Site);

                }
                DcTestCore();
                SaveGainVariable();
                SwEndRun(Site);
                //double aa = TestTime1.Elapsed.TotalMilliseconds;
                return 0;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }
        }

        public void SaveGainVariable()
        {
            if (!String.IsNullOrEmpty(MathVariable))
            {
                try
                {
                    ConcurrentDictionary<string, double> CurrentDic = TestResult.Imeas;
                    if(Calculate.MathCalcCurrent[Site]!= null)
                    {
                        if (Calculate.MathCalcCurrent[Site].ContainsKey(MathVariable))
                            Calculate.MathCalcCurrent[Site][MathVariable] = CurrentDic;
                        else
                            Calculate.MathCalcCurrent[Site].Add(MathVariable, CurrentDic);

                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Duplicated condition detect in adding variable: " + this.MathVariable);
                }
            }
        }

        public void BuildResults(ref ATFReturnResult results)
        {
            bool useCorrelationOffsetsForCalculatedValue = (ResultBuilder.corrFileExists & !GU.runningGU[Site]) | GU.GuMode[Site] == GU.GuModes.Vrfy;
            //bool useCorrelationOffsetsForCalculatedValue = false;
            bool TestIccSum = (TestResult.Imeas.Count(chan => chan.Key.ToUpper().Contains("VCC")) > 1 &&
                            TestCon.DcSettings.Count(chan => chan.Key.ToUpper().Contains("VCC") && chan.Value.Test == true) > 1 ?
                            true : false);

            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    if (TestResult.Imeas.ContainsKey(pinName))
                    {
                        if (double.IsNaN(TestResult.Imeas[pinName])) TestResult.Imeas[pinName] = 2;

                        if (useCorrelationOffsetsForCalculatedValue)
                        {                            
                            double current_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + TestCon.TestParaName, TestResult.Imeas[pinName]);

                            if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                            {
                                TestResult.IccSum += current_cal;
                                continue;
                            }

                            TestResult.Itotal += current_cal;
                        }
                        else
                        {
                            if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                            {
                                TestResult.IccSum += TestResult.Imeas[pinName];
                                continue;
                            }

                            TestResult.Itotal += TestResult.Imeas[pinName];
                        }
                    }
                    else
                    {
                        TestResult.Imeas[pinName] = 0;   // this occurs during GU cal
                    }
                }
            }
            if (TestIccSum)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + "IccSum" + TestCon.TestParaName, TestResult.IccSum);
                }
                else
                {
                    TestResult.Itotal += TestResult.IccSum;
                }
            }

            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if(pinName.ToUpper().Contains("VIO"))
                {
                    if (TestCon.DcSettings[pinName].Test)
                        ResultBuilder.AddResult(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + (TestCon.TestParaName != "" ? TestCon.TestParaName : "") , "A", TestResult.Imeas[pinName], 9);  // hosein 05042020  raName : "") + "x", "A",
                }
                else if (TestCon.DcSettings[pinName].Test)
                {
                        ResultBuilder.AddResult(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + (TestCon.TestParaName != "" ? TestCon.TestParaName : "") , "A", TestResult.Imeas[pinName], 9);  // hosein 05042020 + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName]
                }
            }

            if (TestIccSum)
            {
                ResultBuilder.AddResult(Site, TestCon.CktID + "IccSum" + TestCon.TestParaName , "A", TestResult.IccSum, 9);  //hosein 05042020  + TestCon.SpecNumber["Para.ISum"]
            }
            if (TestCon.TestItotal)
            {
                ResultBuilder.AddResult(Site, TestCon.CktID + "Itotal" + TestCon.TestParaName , "A", TestResult.Itotal, 9);  //hosein 05042020  + TestCon.SpecNumber["Para.ISum"]
            }
            if (TestCon.TestWatt)
            {
                if (TestIccSum)
                {
                    if (TestCon.DcSettings["Vdd"].Test)
                    {
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Wswonly" + TestCon.TestParaName + TestCon.SpecNumber["Para.Watt"], "W", ((TestResult.Imeas["Vdd"] * TestCon.DcSettings["Vdd"].Volts) + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts)), 9);
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Wswonly" + TestCon.TestParaName + TestCon.SpecNumber["Para.Watt"], "W", ((TestResult.IccSum * TestCon.DcSettings["Vcc"].Volts) + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts)), 9);
                    }
                }
                else if (!TestIccSum)
                {
                    if (TestCon.DcSettings["Vdd"].Test)
                    {
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Wswonly" + TestCon.TestParaName + TestCon.SpecNumber["Para.Watt"], "W", ((TestResult.Imeas["Vdd"] * TestCon.DcSettings["Vdd"].Volts) + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts)), 9);
                    }
                    else
                    {
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Wswonly" + TestCon.TestParaName + TestCon.SpecNumber["Para.Watt"], "W", ((TestResult.Imeas["Vcc"] * TestCon.DcSettings["Vcc"].Volts) + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts)), 9);
                    }
                }
            }

            /////////////DPAT
            if (Mordor)
            {
                foreach (string Paraname in TestCon.TestDPAT.Keys)
                {
                    DPAT.Initiate(Site, Paraname, TestCon.MipiCommands);
                }
            }
            /////////////////

        }

        public abstract int DcTestCore();

        public abstract void ConfigureVoltageAndCurrent();

        public void SetupAndMeasureDc()
        {
            SwStartRun("SetupCurrentMeasure1Chan", Site);
            List<Task> allDcSetupTasks = new List<Task>();

            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    allDcSetupTasks.Add(Task.Factory.StartNew(() =>
                    SetupCurrentMeasure1Chan(pinName)));
                }
            }
            Task.WaitAll(allDcSetupTasks.ToArray());
            SwStopRun("SetupCurrentMeasure1Chan", Site);

            uTimer.wait(3);

            SwStartRun("MeasureCurrentMeasure1Chan", Site);
            List<Task> allDcMeasTasks = new List<Task>();
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    allDcMeasTasks.Add(Task.Factory.StartNew(() =>
                    MeasureCurrent1Chan(pinName)));
                }
            }
            Task.WaitAll(allDcMeasTasks.ToArray());
            SwStopRun("MeasureCurrentMeasure1Chan", Site);
        }

        public abstract void SetupCurrentMeasure1Chan(string pinName);

        public abstract void MeasureCurrent1Chan(string pinName);
    }

    public class DcTestConditions
    {
        public string TestParaName;
        public string PowerMode;
        public string WaveformName;
        public string Band;
        public string ParameterNote;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public bool TestIcc;
        public bool TestIcc2;
        public bool TestIbatt;
        public bool TestIdd;
        public bool TestItotal;
        public bool TestISdata1;
        public bool TestISclk1;
        public bool TestIiO1;
        public bool TestISdata2;
        public bool TestISclk2;
        public bool TestIiO2;
        public bool TestWatt;
        public int DelayCurrent;
        public string CktID;
        public bool VIO32MA = false;
        public bool VIORESET = false;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();
        public Dictionary<string, TestLib.DPAT_Variable> TestDPAT = new Dictionary<string, TestLib.DPAT_Variable>();
    }

    public class DcTestResults
    {
        public double Itotal = 0;
        public double IccSum = 0;
        public ConcurrentDictionary<string, double> Imeas = new ConcurrentDictionary<string, double>();
    }

}
