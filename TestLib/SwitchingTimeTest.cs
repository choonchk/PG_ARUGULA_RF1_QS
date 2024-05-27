using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EqLib;
using ClothoLibAlgo;
using NationalInstruments.ModularInstruments.Interop;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace TestLib
{

    public class SwitchingTimeTest : iTest
    {
        public SwitchingTimeTestCondition TestCon = new SwitchingTimeTestCondition();
        public SwitchingTimeResultOn TestResultOn;
        public SwitchingTimeResultOff TestResultOff;

        public bool Initialize(bool finalScript)
        {
            return true;
        }

        public byte Site;
        private Stopwatch switchingTimer = new Stopwatch();
        private SwitchTimeType SwitchingTimeType;
        private AutoResetEvent dcArmedFlag = new AutoResetEvent(false);
        private Task CalcTask;

        public int RunTest()
        {
            try
            {
                TestResultOn = new SwitchingTimeResultOn();
                TestResultOff = new SwitchingTimeResultOff();

                if (ResultBuilder.headerFileMode) return 0;

                this.ConfigureVoltageAndCurrent();
                Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);

                SetSwitchMatrixPaths();

                RunTestCore();

                return 0;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }


        }

        private void RunTestCore()
        {
            Eq.Site[Site].RF.ServoEnabled = false;

            double RFfrequency = TestCon.FreqSG * 1e6;

            float inputPathGain = CableCal.GetCF(Site, TestCon.Band, Operation.VSGtoTX, TestCon.FreqSG);
            float outputPathGain = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            Eq.Site[Site].RF.SetActiveWaveform("CW_SWITCHINGTIME_OFF", "", false);

            Eq.Site[Site].RF.SG.CenterFrequency = RFfrequency;
            Eq.Site[Site].RF.SG.Level = TestCon.TargetPin;
            Eq.Site[Site].RF.SG.ExternalGain = inputPathGain;

            Eq.Site[Site].RF.SA.CenterFrequency = RFfrequency;
            Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(TestCon.TargetPin + TestCon.ExpectedGain + Eq.Site[Site].RF.ActiveWaveform.PAR, 30 - outputPathGain - 0.001);
            Eq.Site[Site].RF.SA.ExternalGain = (double)outputPathGain;
            Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig5;

            Eq.Site[Site].RF.SG.Initiate();

            if (TestCon.TestSwitchingOff) CaptureDataOff();
            else SwitchDeviceOff();

            if (TestCon.TestSwitchingOn) CaptureDataOn();
            
            CalcTask = Task.Run(() => CalcResults());

            Eq.Site[Site].RF.SG.Abort();
            Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig0;
        }

        private void CalcResults()
        {
            if (TestCon.TestSwitchingOn) TestResultOn.CalculateSettling(TestCon);
            if (TestCon.TestSwitchingOff) TestResultOff.CalculateSettling(TestCon, TestResultOn.PoutSettled - 30);
        }

        public void BuildResults(ref ATFReturnResult results)
        {
            if (CalcTask != null)
                CalcTask.Wait();

            if (TestCon.TestSwitchingOn) BuildResults_On();
            if (TestCon.TestSwitchingOff) BuildResults_Off();
        }

        private void BuildResults_On()
        {
            if (double.IsInfinity(TestResultOn.PoutSettled) || double.IsNaN(TestResultOn.PoutSettled) || ResultBuilder.headerFileMode)
            {
                TestResultOn.PoutSettled = -999;
            }
            ResultBuilder.AddResult(Site, "Pout_" + TestCon.TestParaName, "dBm", TestResultOn.PoutSettled, 4);

            if (double.IsInfinity(TestResultOn.PoutSettlingTime) || double.IsNaN(TestResultOn.PoutSettlingTime) || ResultBuilder.headerFileMode)
            {
                TestResultOn.PoutSettlingTime = -999;
            }
            ResultBuilder.AddResult(Site, "RFOntime_" + TestCon.TestParaName, "s", TestResultOn.PoutSettlingTime);
        }

        private void BuildResults_Off()
        {
            if (double.IsInfinity(TestResultOff.PoutSettled) || double.IsNaN(TestResultOff.PoutSettled) || ResultBuilder.headerFileMode)
            {
                TestResultOff.PoutSettled = -999;
            }

            if (double.IsInfinity(TestResultOff.PoutSettlingTime) || double.IsNaN(TestResultOff.PoutSettlingTime) || ResultBuilder.headerFileMode)
            {
                TestResultOff.PoutSettlingTime = -999;
            }

            ResultBuilder.AddResult(Site, "SWtime_" + TestCon.TestParaName, "s", TestResultOff.PoutSettlingTime);
        }

        public void SetSwitchMatrixPaths()
        {
            Eq.Site[Site].SwMatrix.ActivatePath(TestCon.Band, TestCon.VsaOperation);
            Eq.Site[Site].SwMatrix.ActivatePath(TestCon.Band, Operation.VSGtoTX);
        }

        private void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName)) continue; // don't force voltage on MIPI pins

                Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, TestCon.DcSettings[pinName].Current);
            }
        }

        public void SetupAndMeasureDc()
        {
            SetupDcMeasurement();

            dcArmedFlag.Set();

            MeasureDc();
        }

        public void SetupDcMeasurement()
        {
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    Eq.Site[Site].DC[pinName].SetupCurrentTraceMeasurement(Eq.Site[Site].RF.ActiveWaveform.FinalServoMeasTime, 1e-6, TriggerLine.PxiTrig1);
                }
            }
        }

        public void MeasureDc()
        {
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    double[] tempIccSwitching = Eq.Site[Site].DC[pinName].MeasureCurrentTrace();

                    if (SwitchingTimeType == SwitchTimeType.On)
                    {
                        TestResultOn.Itrace[pinName] = tempIccSwitching;
                    }
                    else
                    {
                        TestResultOff.Itrace[pinName] = tempIccSwitching;
                    }
                }
            }
        }

        private void CaptureDataOff()
        {
            SwitchingTimeType = SwitchTimeType.Off;  // necessary so that SMU measurement is placed in correct location

            bool setSG = Eq.Site[Site].RF.ActiveWaveform.WaveformName.Contains("CW_SWITCHINGTIME_");

            Eq.Site[Site].RF.SetActiveWaveform("CW_SWITCHINGTIME_" + SwitchingTimeType.ToString().ToUpper(), "", false, setSG);

            Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());

            Eq.Site[Site].RF.SA.Initiate();

            dcArmedFlag.WaitOne();

            SwitchDeviceOff();

            TestResultOff.IQtrace = Eq.Site[Site].RF.SA.MeasureIqTrace(true);
            TestResultOff.VsaIQrate = Eq.Site[Site].RF.ActiveWaveform.VsaIQrate;

            taskSetupAndMeasureDc.Wait();
        }

        private void CaptureDataOn()
        {
            SwitchingTimeType =  SwitchTimeType.On;  // necessary so that SMU measurement is placed in correct location

            bool setSG = Eq.Site[Site].RF.ActiveWaveform.WaveformName == "PINSWEEP";

            Eq.Site[Site].RF.SetActiveWaveform("CW_SWITCHINGTIME_" + SwitchingTimeType.ToString().ToUpper(), "", false, setSG);

            Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());

            Eq.Site[Site].RF.SA.Initiate();

            dcArmedFlag.WaitOne();

            while (switchingTimer.Elapsed.TotalMilliseconds < (TestCon.OffDurationMs - 1.5)) { }   // subtract 1.5ms to account for other overhead. DB verified with scope.
            SwitchDeviceOn();

            TestResultOn.IQtrace = Eq.Site[Site].RF.SA.MeasureIqTrace(true);
            TestResultOn.VsaIQrate = Eq.Site[Site].RF.ActiveWaveform.VsaIQrate;

            taskSetupAndMeasureDc.Wait();
        }

        private void SwitchDeviceOff()
        {
            Eq.Site[Site].HSDIO.RegWrite("1C", "80", true);   // PM Trigger de-activate
            switchingTimer.Restart();
        }

        private void SwitchDeviceOn()
        {
            Eq.Site[Site].HSDIO.RegWrite("1C", "0", true);   // PM Trigger activate
        }

        public bool TryGettingCalFactors()
        {
            bool success = true;

            if (ResultBuilder.headerFileMode) return true;

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, Operation.VSGtoTX, TestCon.FreqSG);

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, Operation.VSAtoANT1, TestCon.FreqSG);
            success &= 0 != CableCal.GetCF(Site, TestCon.Band, Operation.VSAtoANT2, TestCon.FreqSG);

            return (success);   // if this method took more than 100ms to complete, that means messageBox was shown because CF was not loaded correctly
        }

        public enum SwitchTimeType
        {
            On, Off
        }

        public class SwitchingTimeTestCondition
        {
            public string TestParaName;
            public string PowerMode;
            public string Band;
            public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
            public double OffDurationMs = 10;
            public Operation VsaOperation;

            public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();

            public double FreqSG = 0;
            public float ExpectedGain = 0;
            public float TargetPout;
            public float TargetPin;

            public bool TestSwitchingOn;
            public bool TestSwitchingOff;
        }

        public class SwitchingTimeResultOn : SwitchingTimeResult
        {
            public void CalculateSettling(SwitchingTimeTestCondition TestCon)
            {
                PoutTrace = new double[IQtrace.Length];

                #region Interpolate current arrays to match pout array

                foreach (string pinName in TestCon.DcSettings.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        double origLength = Itrace[pinName].Length;
                        double SettledLength = PoutTrace.Length;

                        double[] origArray = Itrace[pinName].ToArray();
                        Itrace[pinName] = new double[(int)SettledLength];

                        for (int i = 0; i < SettledLength; i++)
                        {
                            double IccIndex = (double)i * origLength / SettledLength;
                            Itrace[pinName][i] = Calc.InterpLinear(origArray, IccIndex);
                        }

                        if (false)  // smoothing
                        {
                            Itrace[pinName] = Calc.Regression.MovingAverage(Itrace[pinName], 10);
                        }
                    }
                }

                #endregion

                for (int i = 0; i < PoutTrace.Length; i++)
                {
                    PoutTrace[i] = 10.0 * Math.Log10((IQtrace[i].Real * IQtrace[i].Real + IQtrace[i].Imaginary * IQtrace[i].Imaginary) / 2.0 / 50.0 * 1000.0);
                }

                #region Find current settling times

                foreach (string pinName in TestCon.DcSettings.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        //double[] smoothItrace = Calc.MovingAverage(Itrace[pinName], (int) (ClsInstrLib.I[1].RF.ActiveWaveform.VsaIQrate * 100e-6m));

                        ISettled[pinName] = Itrace[pinName].Skip(Itrace[pinName].Length * 99 / 100).Take(Itrace[pinName].Length * 1 / 100).Average();
                        ISettlingTime[pinName] = Itrace[pinName].Length / (double)VsaIQrate;

                        double iSettlingTolerance = Math.Abs(ISettled[pinName] * 0.1);   // settle to within 90%

                        // this helps detect the "missing burst" characteristic

                        //for (int i = Itrace[pinName].Length - 1; i >= 0; i--)
                        //{
                        //    if (Math.Abs(smoothItrace[i] - ISettled[pinName]) > iSettlingTolerance)
                        //    {
                        //        ISettlingTime[pinName] = (double)(i + 1) / (double)ClsInstrLib.I[1].RF.ActiveWaveform.VsaIQrate;
                        //        break;
                        //    }
                        //}

                        //if (!ISettlingTime.ContainsKey(pinName))
                        {
                            for (int i = 0; i < Itrace[pinName].Length; i++)
                            {
                                if (Math.Abs(Itrace[pinName][i] - ISettled[pinName]) <= iSettlingTolerance)
                                {
                                    ISettlingTime[pinName] = (double)(i + 1) / (double)VsaIQrate;
                                    break;
                                }
                            }
                        }

                        if (!ISettlingTime.ContainsKey(pinName))
                        {
                            ISettlingTime[pinName] = 0;
                        }

                    }
                }

                #endregion

                #region find pout Settling time

                PoutSettled = PoutTrace.Skip(PoutTrace.Length * 99 / 100).Take(PoutTrace.Length * 1 / 100).Average();
                GainSettled = PoutSettled - TestCon.TargetPin;

                PoutSettlingTime = PoutTrace.Length / (double)VsaIQrate;
                double rfSettlingTolerance = 1;  // dB from settled value

                for (int i = PoutTrace.Length - 1; i >= 0; i--)
                {
                    if (Math.Abs(PoutTrace[i] - PoutSettled) > rfSettlingTolerance)
                    {
                        PoutSettlingTime = (double)(i + 1) / (double)VsaIQrate;
                        break;
                    }
                }

                #endregion

                TimeTrace = new double[IQtrace.Length];
                for (int i = 0; i < IQtrace.Length; i++)
                {
                    TimeTrace[i] = (double)i * 1e6 / (double)VsaIQrate;
                }

                if (false)
                {
                    #region Create File

                    string[] IQmag = new string[IQtrace.Length + 1];

                    IQmag[0] = "Time(us),Pout(dBm),";

                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test) IQmag[0] += TestCon.DcSettings[pinName].iParaName + ",";
                    }

                    for (int i = 0; i < IQtrace.Length; i++)
                    {
                        IQmag[i + 1] = ((decimal)i * 1e6m / VsaIQrate).ToString() + "," + PoutTrace[i];
                    }

                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test)
                        {
                            for (int i = 0; i < Itrace[pinName].Length; i++)
                            {
                                IQmag[i + 1] += "," + Itrace[pinName][i];
                            }
                        }
                    }

                    File.WriteAllLines("SwitchingTest_" + SwitchTimeType.On.ToString() + " .csv", IQmag);

                    #endregion
                }
            }
        }

        public class SwitchingTimeResultOff : SwitchingTimeResult
        {
            public void CalculateSettling(SwitchingTimeTestCondition TestCon, double rfSettledLevel)
            {
                PoutTrace = new double[IQtrace.Length];

                #region Interpolate current arrays to match pout array

                foreach (string pinName in TestCon.DcSettings.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        double origLength = Itrace[pinName].Length;
                        double SettledLength = PoutTrace.Length;

                        double[] origArray = Itrace[pinName].ToArray();
                        Itrace[pinName] = new double[(int)SettledLength];

                        for (int i = 0; i < SettledLength; i++)
                        {
                            double IccIndex = (double)i * origLength / SettledLength;
                            Itrace[pinName][i] = Calc.InterpLinear(origArray, IccIndex);
                        }

                        if (false)  // smoothing
                        {
                            Itrace[pinName] = Calc.Regression.MovingAverage(Itrace[pinName], 10);
                        }
                    }
                }

                #endregion

                for (int i = 0; i < PoutTrace.Length; i++)
                {
                    PoutTrace[i] = 10.0 * Math.Log10((IQtrace[i].Real * IQtrace[i].Real + IQtrace[i].Imaginary * IQtrace[i].Imaginary) / 2.0 / 50.0 * 1000.0);
                }

                #region Find current settling times

                foreach (string pinName in TestCon.DcSettings.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        //double[] smoothItrace = Calc.MovingAverage(Itrace[pinName], (int) (ClsInstrLib.I[1].RF.ActiveWaveform.VsaIQrate * 100e-6m));

                        ISettled[pinName] = Itrace[pinName].Skip(Itrace[pinName].Length * 99 / 100).Take(Itrace[pinName].Length * 1 / 100).Average();
                        ISettlingTime[pinName] = Itrace[pinName].Length / (double)VsaIQrate;

                        double iSettlingValue = 100e-6;   // settle to <100uA

                        for (int i = 0; i < Itrace[pinName].Length; i++)
                        {
                            if (Itrace[pinName][i] < iSettlingValue)
                            {
                                ISettlingTime[pinName] = (double)i / (double)VsaIQrate;
                                break;
                            }
                        }

                        if (!ISettlingTime.ContainsKey(pinName))
                        {
                            ISettlingTime[pinName] = 0;
                        }

                    }
                }

                #endregion

                #region find pout Settling time

                PoutSettled = PoutTrace.Skip(PoutTrace.Length * 99 / 100).Take(PoutTrace.Length * 1 / 100).Average();
                GainSettled = PoutSettled - TestCon.TargetPin;

                PoutSettlingTime = 0;

                for (int i = PoutTrace.Length - 1; i >= 0; i--)
                {
                    if (PoutTrace[i] > rfSettledLevel)
                    {
                        PoutSettlingTime = (double)(i + 1) / (double)VsaIQrate;
                        break;
                    }
                }

                #endregion

                TimeTrace = new double[IQtrace.Length];
                for (int i = 0; i < IQtrace.Length; i++)
                {
                    TimeTrace[i] = (double)i * 1e6 / (double)VsaIQrate;
                }

                if (false)
                {
                    #region Create File

                    string[] IQmag = new string[IQtrace.Length + 1];

                    IQmag[0] = "Time(us),Pout(dBm),";

                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test) IQmag[0] += TestCon.DcSettings[pinName].iParaName + ",";
                    }

                    for (int i = 0; i < IQtrace.Length; i++)
                    {
                        IQmag[i + 1] = ((decimal)i * 1e6m / VsaIQrate).ToString() + "," + PoutTrace[i];
                    }

                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test)
                        {
                            for (int i = 0; i < Itrace[pinName].Length; i++)
                            {
                                IQmag[i + 1] += "," + Itrace[pinName][i];
                            }
                        }
                    }

                    File.WriteAllLines("SwitchingTest_" + SwitchTimeType.Off.ToString() + " .csv", IQmag);

                    #endregion
                }
            }
        }

        public class SwitchingTimeResult
        {
            public niComplexNumber[] IQtrace;
            public ConcurrentDictionary<string, double[]> Itrace = new ConcurrentDictionary<string, double[]>();

            public double[] PoutTrace;
            public double[] TimeTrace;

            public double PoutSettled;
            public double GainSettled;
            public Dictionary<string, double> ISettled = new Dictionary<string, double>();

            public double PoutSettlingTime;
            public Dictionary<string, double> ISettlingTime = new Dictionary<string, double>();

            public decimal VsaIQrate;
        }
    }
}
