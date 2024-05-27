using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EqLib;
using GuCal;
using ClothoLibAlgo;
using IqWaveform;
using NationalInstruments.ModularInstruments.Interop;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using Avago.ATF.StandardLibrary;
using System.IO;
using ProductionLib;


namespace TestLib
{
    public class TimingTestFixedOutputPower : TimingTestBase
    {
        public override void TimingTestCore()
        {


            this.ConfigureVoltageAndCurrent();

            SwStartRun("SendMipiCommands", Site);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands", Site);

            // For Quadsite, single Switch Dio to support dual sites 	
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + Site.ToString();
                SiteTemp = 0;
            }

            // Skip Output Port on Contact Failure (11-Nov-2018)
            if (SkipOutputPortOnFail && Eq.Site[SiteTemp].SwMatrix.IsFailedDevicePort(BandTemp, TestCon.VsaOperation))
            {
                //TestResult.iqTrace = new niComplexNumber[Eq.Site[Site].RF.SA.NumberOfSamples];
                TestResult.iqTrace = new niComplexNumber[(int)((TestCon.TimingCondition.After_Udelay * 1.2) * 10)];
            }
            else
            {
                SwStartRun("Task.Run-ConfigRF", Site);
                Task taskConfigRF = Task.Run(() => ConfigRF());
                SwStopRun("Task.Run-ConfigRF", Site);

                SwStartRun("Task.Run-ConfigRF-Wait", Site);
                taskConfigRF.Wait();

                Eq.Site[Site].RF.ThreadFlags[0].WaitOne();
                Eq.Site[Site].RF.ThreadFlags[1].WaitOne();
                SwStopRun("Task.Run-ConfigRF-Wait", Site);

                SwStartRun("Servo_Timing", Site);
                bool f = Eq.Site[Site].RF.Servo_Timing(out TestResult.Pout, out TestResult.Pin, outputPathGain_withIccCal);
                SwStopRun("Servo_Timing", Site);

                //            TestCon.TestParaName


                SwStartRun("Task.Run-SetupAndMeasureDc", Site);
                Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());
                SwStopRun("Task.Run-SetupAndMeasureDc", Site);
                SwStartRun("dcArmedFlag.WaitOne", Site);
                dcArmedFlag.WaitOne();
                SwStopRun("dcArmedFlag.WaitOne", Site);
                useACPchPowerForPout = turboServo;

                if (TestResult.Pout == float.NegativeInfinity || TestResult.Pout == 0.0)
                {
                    TestResult.Pout = -50;
                    useACPchPowerForPout = true;
                }

                SwStartRun("Task.Run-SetupAndMeasureDc-Wait", Site);
                taskSetupAndMeasureDc.Wait();
                SwStopRun("Task.Run-SetupAndMeasureDc-Wait", Site);

                SwStartRun("SASG.Abort", Site);
                Eq.Site[Site].RF.SG.Abort();
                Eq.Site[Site].RF.SA.Abort(Site);

                Eq.Site[Site].RF.ResetRFSA(false);
                SwStopRun("SASG.Abort", Site);

                SwStartRun("RF.Configure_TimingAndMeasure", Site);

                if (TestCon.TestParaName.Contains("ANTTOANT"))
                {
                    Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, Operation.VSAtoANT3);
                }


                Eq.Site[Site].RF.Configure_Timing(new EqRF.Config_Timing(Eq.Site[Site].RF.CRfmxIQ_Timing.GetSpecIteration(), TestCon.FreqSG, Convert.ToDouble(TestResult.Pin), inputPathGain_withIccCal, outputPathGain_withIccCal));

                Eq.Site[Site].RF.ThreadFlags[0].WaitOne();
                Eq.Site[Site].RF.ThreadFlags[1].WaitOne();

                Eq.Site[Site].RF.Measure_Timing();
                SwStopRun("RF.Configure_TimingAndMeasure", Site);

                SwStartRun("SetTimingDelay", Site);
                SetTimingDelay(TestCon.TimingCondition.nBefore_Command, TestCon.TimingCondition.Before_Udelay, TestCon.TimingCondition.After_Udelay);
                SwStopRun("SetTimingDelay", Site);
                SwStartRun("SendMipiCommands", Site);
                Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.TimingCondition.Mipi, eMipiTestType.Timing);
                SwStopRun("SendMipiCommands", Site);

                SwStartRun("Timing.RetrieveResults", Site);
                TestResult.iqTrace = Eq.Site[Site].RF.CRfmxIQ_Timing.RetrieveResults(outputPathGain_withIccCal, (int)((TestCon.TimingCondition.After_Udelay * 1.2) * 10));
                SwStopRun("Timing.RetrieveResults", Site);
                SwStartRun("SASG.Abort", Site);
                int GetIteration = Eq.Site[Site].RF.CRfmxIQ_Timing.GetSpecIteration();
                Eq.Site[Site].RF.CRfmxIQ_Timing.SpecIteration();

                Eq.Site[Site].RF.SG.Abort();
                Eq.Site[Site].RF.SA.Abort(Site);


                Eq.Site[Site].RF.ResetRFSA(false);
                SwStopRun("SASG.Abort", Site);


            //Not Implemented yet
#if false
            Task taskConfigRF = Task.Run(() => ConfigRF());

            this.ConfigureVoltageAndCurrent();

            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);

            taskConfigRF.Wait();

            bool f = Eq.Site[Site].RF.Servo(out TestResult.Pout, out TestResult.Pin);
            Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());
            dcArmedFlag.WaitOne();
            useACPchPowerForPout = turboServo;
            if (TestResult.Pout == float.NegativeInfinity || TestResult.Pout == 0.0)
            {
                TestResult.Pout = -50;
                useACPchPowerForPout = true;
            }

            Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                       

            taskSetupAndMeasureDc.Wait();
            //double ICC = TestResult.Imeas["Vcc"];
            //SaveGainVariable();
#endif
            }
        }

        private void ConfigRF()
        {
            SwStartRunThread("ConfigRF-Configure");

            double GainAccuracy = 5.0;
            double sgLevelFromIccCal = GU.IccServoVSGlevel[Site, TestCon.TestParaName + "_IccCal"];

            Eq.Site[Site].RF.ResetRFSA(true);

            if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
            {
                double SAPeakLB = Math.Min((TestCon.TargetPout.Value + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) - inputPathGain_withIccCal, 30 + outputPathGain_withIccCal - 0.001);

                IQ.Waveform newWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
                Eq.Site[Site].RF.SA.ModulationStd = TestCon.ModulationStd;
                Eq.Site[Site].RF.SA.WaveformName = TestCon.WaveformName;

                Eq.Site[Site].RF.Configure_Servo_Timing(new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration(), TestCon.TargetPout.Value, TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, 
                    0f, null, null, null, false, 0, 0f, 0,0 , false, Site));


            }
            else
            {

                Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;
                Eq.Site[Site].RF.SA.ConfigureTrigger(TestCon.WaveformName);
                Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
                Eq.Site[Site].RF.SA.ReferenceLevel = TestCon.TargetPout.Value + TestCon.IqWaveform.PAR - GainAccuracy;


                Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;
                Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
                Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, turboServo, true);

                Eq.Site[Site].RF.ServoEnabled = true;


                if (sgLevelFromIccCal == 0)
                {
                    Eq.Site[Site].RF.SG.Level = TestCon.TargetPout.Value - TestCon.ExpectedGain;
                }
                else
                {
                    Eq.Site[Site].RF.SG.Level = sgLevelFromIccCal + GainAccuracy + 1;
                }

                double ExpectedGain = TestCon.ExpectedGain + GainAccuracy + 1;
                Eq.Site[Site].RF.Configure_Servo(new EqRF.Config(TestCon.TargetPout.Value, ExpectedGain, sgLevelFromIccCal, 0.04, turboServo));
            }
            SwStopRunThread("ConfigRF-Configure");


            //Not Implemented yet
#if false
                            double GainAccuracy = 3.0;
                            float sgLevelFromIccCal = GU.IccServoVSGlevel[Site, TestCon.TestParaName + "_IccCal"];

                            Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;

                            Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;
                            Eq.Site[Site].RF.SA.ConfigureTrigger(TestCon.WaveformName);

                            Eq.Site[Site].RF.ServoEnabled = true;

                            Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
                            Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, turboServo, true);
                            if (sgLevelFromIccCal == 0)
                            {
                                Eq.Site[Site].RF.SG.Level = TestCon.TargetPout.Value - TestCon.ExpectedGain;
                            }
                            else
                            {
                                Eq.Site[Site].RF.SG.Level = sgLevelFromIccCal + GainAccuracy + 1;
                            }

                            Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
                            Eq.Site[Site].RF.SA.ReferenceLevel = TestCon.TargetPout.Value + TestCon.IqWaveform.PAR - GainAccuracy;
                            double ExpectedGain = TestCon.ExpectedGain + GainAccuracy + 1;
                            Eq.Site[Site].RF.Configure_Servo(new EqRF.Config(TestCon.TargetPout.Value, ExpectedGain, sgLevelFromIccCal, 0.04, turboServo));

                            //Eq.Site[Site].RF.ConfigureServo(TestCon.TargetPout.Value, 0.05, TestCon.ExpectedGain, 4, 10);
#endif
        }
        private void SetTimingDelay(int _nBeforeCmd, double _BeforeDelay, double _AfterDelay)
        {
            Eq.Site[Site].HSDIO.BeforeDelay = _BeforeDelay;
                Eq.Site[Site].HSDIO.AfterDelay = _AfterDelay;
                Eq.Site[Site].HSDIO.nBeforeCmd = _nBeforeCmd;
        }
    }

    public class TimingTestFixedInputPower : TimingTestBase
    {
        public override void TimingTestCore()
        {

            TestResult.Pin = TestCon.TargetPin.Value;

            this.ConfigureVoltageAndCurrent();

            SwStartRun("SendMipiCommands", Site);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands", Site);

            // For Quadsite, single Switch Dio to support dual sites 	
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + Site.ToString();
                SiteTemp = 0;
            }

            // Skip Output Port on Contact Failure (11-Nov-2018)
            if (SkipOutputPortOnFail && Eq.Site[SiteTemp].SwMatrix.IsFailedDevicePort(BandTemp, TestCon.VsaOperation))
            {
                //TestResult.iqTrace = new niComplexNumber[Eq.Site[Site].RF.SA.NumberOfSamples];
                TestResult.iqTrace = new niComplexNumber[(int)((TestCon.TimingCondition.After_Udelay * 1.2) * 10)];
            }
            else
            {
                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                {
                    SwStartRun("MeasureTiming", Site);

                    //Eq.Site[Site].RF.SA.SampleRate = 10e6;

                    Eq.Site[Site].RF.Configure_Timing(new EqRF.Config_Timing(Eq.Site[Site].RF.CRfmxIQ_Timing.GetSpecIteration(), TestCon.FreqSG, Convert.ToDouble(TestCon.TargetPin), inputPathGain_withIccCal, outputPathGain_withIccCal));


                    Eq.Site[Site].RF.ThreadFlags[0].WaitOne();
                    Eq.Site[Site].RF.ThreadFlags[1].WaitOne();

                    Eq.Site[Site].RF.Measure_Timing();

                    SetTimingDelay(TestCon.TimingCondition.nBefore_Command, TestCon.TimingCondition.Before_Udelay, TestCon.TimingCondition.After_Udelay);
                    Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.TimingCondition.Mipi, eMipiTestType.Timing);

                    TestResult.iqTrace = Eq.Site[Site].RF.CRfmxIQ_Timing.RetrieveResults(outputPathGain_withIccCal, (int)((TestCon.TimingCondition.After_Udelay * 1.2) * 10));
                    SwStopRun("MeasureTiming", Site);


                }
                else
                {
                    SwStartRun("SA.MeasureIqTrace", Site);

                    Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, false, true);
                    Eq.Site[Site].RF.ServoEnabled = false;

                    Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
                    Eq.Site[Site].RF.SG.Level = TestCon.TargetPin.Value;
                    Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;
                    Eq.Site[Site].RF.SG.Initiate();

                    Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
                    Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(TestCon.TargetPin.Value + TestCon.ExpectedGain + Eq.Site[Site].RF.ActiveWaveform.PAR, 30 - outputPathGain_withIccCal - 0.001);
                    Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;

                    Eq.Site[Site].RF.SA.SampleRate = 10e6;
                    Eq.Site[Site].RF.SA.NumberOfSamples = (int)((TestCon.TimingCondition.After_Udelay * 1.2) * 10);
                    Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig6;

                    Eq.Site[Site].RF.SA.Initiate();

                    useACPchPowerForPout = true;

                    SetTimingDelay(TestCon.TimingCondition.nBefore_Command, TestCon.TimingCondition.Before_Udelay, TestCon.TimingCondition.After_Udelay);
                    Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.TimingCondition.Mipi, eMipiTestType.Timing);

                    //Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);            
                    TestResult.iqTrace = Eq.Site[Site].RF.SA.MeasureIqTrace(true);
                    SwStopRun("SA.MeasureIqTrace", Site);

                }
            }



            Eq.Site[Site].RF.CRfmxIQ_Timing.SpecIteration();

            Eq.Site[Site].RF.SG.Abort();
            Eq.Site[Site].RF.SA.Abort(Site);

            //keng shan added
            float rampLvl = TestCon.TargetPin.Value;
            //do
            //{
            //    Eq.Site[Site].RF.SG.Level = rampLvl - 5;
            //} while (rampLvl < -40);
        }
        private void SetTimingDelay(int _nBeforeCmd, double _BeforeDelay, double _AfterDelay)
        {
            Eq.Site[Site].HSDIO.BeforeDelay = _BeforeDelay;
            Eq.Site[Site].HSDIO.AfterDelay = _AfterDelay;
            Eq.Site[Site].HSDIO.nBeforeCmd = _nBeforeCmd;
        }
    }

    public abstract class TimingTestBase : TimingBase, iTest
    {
        public TimingTestResult TestResult;
        public string MathVariable;
        public TimingTestCondition TestCon = new TimingTestCondition();
        public byte Site;
        public double inputPathGain_withIccCal = 0;
        public double outputPathGain_withIccCal = 0;
        public bool useACPchPowerForPout = false;
        public const bool turboServo = false;
        protected AutoResetEvent dcArmedFlag = new AutoResetEvent(false);
        private Task CalcTask;
        public TriggerLine dcTrigLine;
        public Dictionary<byte, int[]> EqSiteTriggerArray;
        public bool SkipOutputPortOnFail = false;
        public bool Mordor = false;
        public bool resetSA;
        public abstract void TimingTestCore();
        private string folderpath = @"C:\Avago.ATF.Common\DataLog\Timing\";
        public static string SwitchTimeTraceFile_Enable = "FALSE";
        public static int currentPIDint = 0;
        public static int dutSN = 0;
        public HiPerfTimer uTimer = new HiPerfTimer();

        public bool Initialize(bool finalScript)
        {
            InitializeTiming(this.TestCon.TestParaName);

            bool success = true;
            outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);
            //double SAPeakLB = Math.Min((Convert.ToDouble(TestCon.TargetPout) + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) + outputPathGain_withIccCal, 30);

            double SAPeakLB = 0f;

            if (TestCon.TestParaName.ToUpper().Contains("FIXEDPOUT"))
            {
                SAPeakLB = Math.Min((Convert.ToDouble(TestCon.TargetPout) + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) + outputPathGain_withIccCal, 30);
            }
            else if (TestCon.TestParaName.ToUpper().Contains("FIXEDPIN"))
            {
                SAPeakLB = Math.Min(((Convert.ToDouble(TestCon.ExpectedGain)) + (Convert.ToDouble(TestCon.TargetPin)) + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) + outputPathGain_withIccCal, 30);
            }

            double AcquisitionTime = ((((TestCon.TimingCondition.After_Udelay / 1e6) * 1.2f) * Convert.ToDouble(IQ.Mem["PINSWEEP"].VsgIQrate))) * (1f / Convert.ToDouble(IQ.Mem["PINSWEEP"].VsgIQrate));
            double TriggerDelay = 0f;
            success &= Eq.Site[Site].RF.LoadWaveform(TestCon.ModulationStd, TestCon.WaveformName);

            if (IQ.Mem.ContainsKey(TestCon.ModulationStd + TestCon.WaveformName))
            {
                TestCon.IqWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
            }


            //Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxChp, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxCHP.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
            //                                                          IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
            //                                                          TestCon.ModulationStd + TestCon.WaveformName, false, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw));


            //Eq.Site[Site].RF.
            Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxIQ_Timing, new EqRF.Config(Eq.Site[Site].RF.CRfmxIQ_Timing.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, Convert.ToDouble(IQ.Mem["PINSWEEP"].VsgIQrate), AcquisitionTime, TriggerDelay, dcTrigLine, Site));

            // EqLib.EqRF.NIVST_Rfmx.cRfmxCHP.SpecIteration();
            Eq.Site[Site].RF.CRfmxIQ_Timing.SpecIteration();

            return success;
        }

        public int RunTest()
        {
            try
            {
                TestResult = new TimingTestResult();

                if (ResultBuilder.headerFileMode) return 0;

                SwBeginRun(Site);

                SwStartRun("SetSwitchMatrixPaths", Site);
                SetSwitchMatrixPaths();
                SwStopRun("SetSwitchMatrixPaths", Site);
                if (resetSA)
                    //  Eq.Site[Site].RF.ResetRFSA(resetSA);

                    TimingTest();

                SwStartRun("CalcResults-SaveTrace", Site);

                // For Quadsite, single Switch Dio to support dual sites 	
                string BandTemp = TestCon.Band;
                byte SiteTemp = Site;
                if (Site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + Site.ToString();
                    SiteTemp = 0;
                }

                if (SkipOutputPortOnFail && Eq.Site[SiteTemp].SwMatrix.IsFailedDevicePort(BandTemp, TestCon.VsaOperation))
                {
                    TestResult.Pout = -999;
                    TestResult.Gain = -999;
                    TestResult.cSettlingTime.firstVal = -999;
                    TestResult.cSettlingTime.RefVal = -999;
                    TestResult.cSettlingTime.onTime = -999;
                    TestResult.cSettlingTime.offTime = -999;
                }
                else
                {
                    CalcResults();
                }

                SwStopRun("CalcResults-SaveTrace", Site);

                foreach (string pinName in Eq.Site[Site].DC.Keys)
                {
                    if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue;

                    string msg = String.Format("TransientResponse_Normal on pin {0}", pinName);
                    SwStartRun(msg, Site);
                    Eq.Site[Site].DC[pinName].TransientResponse_Normal(TestCon.DcSettings[pinName]);
                    SwStopRun(msg, Site);
                }
                SwEndRun(Site);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }
        }

        public static void PreTest()
        {
            string SN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "");

            if (string.IsNullOrEmpty(SN))
            {
                dutSN = currentPIDint;
                dutSN++;   // Lite Driver
            }
        }

        public void TimingTest()
        {
            useACPchPowerForPout = false;

            GetLossFactors();

            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue;

                string msg = String.Format("TransientResponse_Fast on pin {0}", pinName);
                SwStartRun(msg, Site);
                Eq.Site[Site].DC[pinName].TransientResponse_Fast(TestCon.DcSettings[pinName]);
                SwStopRun(msg, Site);
            }

            TimingTestCore();

            // SaveTrace(TestResult.iqTrace.Length);


            //SpectralAnalysis sa = new SpectralAnalysis(TestCon.IqWaveform, TestResult.iqTrace);
            //sa.ShowPlot(TestCon.IqWaveform, TestCon.TestParaName);

            // Eq.Site[Site].RF.SG.Abort();
        }

        public void BuildResults(ref ATFReturnResult results)
        {
            bool useCorrelationOffsetsForCalculatedValue = (ResultBuilder.corrFileExists & !GU.runningGU[Site]) | GU.GuMode[Site] == GU.GuModes.Vrfy;
            //bool useCorrelationOffsetsForCalculatedValue = false;

            if (CalcTask != null)
                CalcTask.Wait();

            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    if (TestResult.Imeas.ContainsKey(pinName))
                    {
                        if (double.IsNaN(TestResult.Imeas[pinName])) TestResult.Imeas[pinName] = 2;

                        if (useCorrelationOffsetsForCalculatedValue)
                        {
                            double current_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas[pinName]);
                            TestResult.Itotal += current_cal;
                        }
                        else
                        {
                            TestResult.Itotal += TestResult.Imeas[pinName];
                        }
                    }
                    else
                    {
                        TestResult.Imeas[pinName] = 0;   // this occurs during GU cal
                    }
                }
            }

            if (TestCon.TestPin)
            {
                // Mario 2020/02/16
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pin_" + TestCon.TestParaName + "_" + TestCon.ParameterNote, "dBm", TestResult.Pin, 4);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pin_" + TestCon.ParameterNote + "_" + TestCon.TestParaName, "dBm", TestResult.Pin, 4);

            }
            if (TestCon.TestPout)
            {
                if (useACPchPowerForPout & !GU.runningGUIccCal[Site])
                {
                    TestResult.Pout = TestResult.cSettlingTime.RefVal;
                }

                //   TestResult.Pout = TestResult.cSettlingTime.RefVal;

                // Mario 2020/02/16
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pout_" + TestCon.TestParaName + "_" + TestCon.ParameterNote, "dBm", TestResult.Pout, 4);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pout_" + TestCon.ParameterNote + "_" + TestCon.TestParaName, "dBm", TestResult.Pout, 4);
            }
            if (TestCon.TestGain)
            {
                // Mario 2020/02/16
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_" + TestCon.TestParaName + "_" + TestCon.ParameterNote, "dB", (TestResult.Pout - TestResult.Pin), 4);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_" + TestCon.ParameterNote + "_" + TestCon.TestParaName, "dB", (TestResult.Pout - TestResult.Pin), 4);
            }
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    ResultBuilder.AddResult(this.Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_" + TestCon.TestParaName, "A", !ResultBuilder.headerFileMode ? TestResult.Imeas[pinName] : 0, 9);
                }
            }
            if (TestCon.TestItotal)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Itotal_" + TestCon.TestParaName, "A", TestResult.Itotal, 9);
            }

            // Mario 2020/02/12            
            ResultBuilder.AddResult(this.Site, TestCon.CktID + "OnTime_" + TestCon.TestParaName + "_" + TestCon.ParameterNote, "A", TestResult.cSettlingTime.onTime, 9);
            ResultBuilder.AddResult(this.Site, TestCon.CktID + "OffTime_" + TestCon.TestParaName + "_" + TestCon.ParameterNote, "A", TestResult.cSettlingTime.offTime, 9);

            /////////////DPAT
            if(Mordor)
            {
                foreach (string Paraname in TestCon.TestDPAT.Keys)
                {
                    DPAT.Initiate(Site, Paraname, TestCon.MipiCommands);
                }
            }
            /////////////////

            if (SwitchTimeTraceFile_Enable == "TRUE") SaveTrace(); 
        }

        public void SaveGainVariable(Task GainCalc)
        {
            if (!String.IsNullOrEmpty(this.MathVariable))
            {
                try
                {
                    if (GainCalc != null)
                        GainCalc.Wait();
                    //double Gain = TestResult.ACLR.centerChannelPower - TestResult.Pin;
                    double Gain = 0;
                    if (Calculate.MathCalc[Site].ContainsKey(this.MathVariable))   //kh
                        Calculate.MathCalc[Site][this.MathVariable] = Gain;
                    else
                        Calculate.MathCalc[Site].Add(this.MathVariable, Gain);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Duplicated condition detect in adding variable: " + this.MathVariable);
                }
            }
        }

        private void GetLossFactors()
        {           
            inputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.InputGain);
            outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);

        }

        public void SetSwitchMatrixPaths()
        {
            // For Quadsite, single Controller dual sites 	
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + Site.ToString();
                SiteTemp = 0;
            }
            Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, TestCon.VsaOperation);
            Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, TestCon.VsgOperation);
        }

        public void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper())) continue; // don't force voltage on MIPI pins
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

            //Console.WriteLine("Site:{0} Thread hashcode:{1}",Site,Thread.CurrentThread.GetHashCode());
        }

        public bool TryGettingCalFactors()
        {
            bool success = true;

            if (ResultBuilder.headerFileMode) return true;

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            return (success);   // if this method took more than 100ms to complete, that means messageBox was shown because CF was not loaded correctly
        }

        public bool TryGettingCalFactorsfromPort()
        {
            bool success = true;

            if (ResultBuilder.headerFileMode) return true;

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            return (success);   // if this method took more than 100ms to complete, that means messageBox was shown because CF was not loaded correctly
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
                    string msg = String.Format("SetupDCMeas-SetupCurrentMeasure on pin {0}", pinName);
                    SwStartRunThread(msg);

                    if (pinName.Contains("Vrx"))
                    {
                        Eq.Site[Site].DC[pinName].SetupCurrentMeasure(0.01, dcTrigLine);
                    }
                    else
                    {
                        Eq.Site[Site].DC[pinName].SetupCurrentMeasure(Eq.Site[Site].RF.ActiveWaveform.FinalServoMeasTime, dcTrigLine);
                    }
                    SwStopRunThread(msg);

                }
            }

        }

        public void MeasureDc()
        {
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    string msg = String.Format("MeasureDCMeas-MeasureCurrent on pin {0}", pinName);
                    SwStartRunThread(msg);
                    TestResult.Imeas[pinName] = Eq.Site[Site].DC[pinName].MeasureCurrent(TestCon.DcSettings[pinName].Avgs);
                    SwStopRunThread(msg);
                }
            }
            //double ICC = TestResult.Imeas["Vcc"];
            //Eq.Site[Site].DC["Vcc"].SetupCurrentMeasure(Eq.Site[Site].RF.ActiveWaveform.FinalServoMeasTime, dcTrigLine);
            //double Icc = Eq.Site[Site].DC["Vcc"].MeasureCurrent(1);
        }

        private void CalcResults()
        {
            try
            {

                if ((TestResult.iqTrace == null || TestResult.iqTrace.Length == 0))
                {
                    //   MessageBox.Show("NS testing was requested, but IQdataACLR was not captured.\nNS testing requires that IQdataACLR is captured.");
                }

                if (TestResult.iqTrace != null)
                {

                    TestResult.cSettlingTime.RawTrace = new double[TestResult.iqTrace.Length];

                    for (int i = 0; i < TestResult.iqTrace.Length; i++)
                    {
                        TestResult.cSettlingTime.RawTrace[i] =
                            10.0 * Math.Log10((TestResult.iqTrace[i].Real * TestResult.iqTrace[i].Real + TestResult.iqTrace[i].Imaginary * TestResult.iqTrace[i].Imaginary) / 2.0 / 50.0 * 1000.0);  // need to confirm this calculation               
                    }

                    Eq.Site[Site].RF.SA.SampleRate = 10e6;
                    double SaSampleRateMhz = Eq.Site[Site].RF.SA.SampleRate / 1e6;
                    int RefPoint = (int)(TestCon.TimingCondition.After_Udelay * SaSampleRateMhz) / 2;
                    double RefAvgDuration = (TestCon.TimingCondition.After_Udelay * 0.01 * 1e-6) * Eq.Site[Site].RF.SA.SampleRate;
                    double RefAvg = 0;

                    int OnTimeIndex = 0;
                    int OffTimeIndex = 0;
                    double HSdiotoSA_offset = 0.1; //new add for 'offtime'

                    //Ref Search                     
                    for (int i = (int)(RefPoint - RefAvgDuration / 2); i < (int)(RefPoint + RefAvgDuration / 2); i++)
                    {
                        RefAvg += TestResult.cSettlingTime.RawTrace[i];
                    }

                    RefAvg /= RefAvgDuration;
                    if (TestCon.TestParaName.ToUpper().Contains("FIXEDPIN"))
                    {
                        TestResult.Pout = RefAvg;
                        TestResult.Gain = TestResult.Pout - TestResult.Pin;
                    }

                    // Taking average of last 10 points
                    double SumOffLevel = 0;
                    for (int i = TestResult.cSettlingTime.RawTrace.Length - 10; i < TestResult.cSettlingTime.RawTrace.Length; i++)
                    {
                        SumOffLevel += TestResult.cSettlingTime.RawTrace[i];
                    }
                    double RefAvgOff = (SumOffLevel / 10) + TestCon.Threshold;
                    double RfofftimeLevel = Math.Max(RefAvgOff, -30);

                    if (TestCon.IsForwarSearch)
                    {
                        /*Original*/
                        //for (int i = 0; i < RefPoint; i++)
                        //{
                        //    if (Math.Abs(RefAvg - TestResult.cSettlingTime.RawTrace[i]) < TestCon.Threshold)
                        //    {
                        //        OnTimeIndex = i; break;
                        //    }
                        //}                        
                        for (int i = 0; i < RefPoint; i++)
                        {
                            if ((RefAvg - TestCon.Threshold) < (TestResult.cSettlingTime.RawTrace[i]))
                            {
                                OnTimeIndex = i; break;
                            }
                        }

                        for (int i = TestResult.cSettlingTime.RawTrace.Length - 1; i > RefPoint; i--)
                        {
                            if (TestResult.cSettlingTime.RawTrace[i] > RfofftimeLevel)
                            {
                                OffTimeIndex = i; break;
                            }

                        }
                    }
                    else
                    {
                        for (int i = RefPoint; i > 0; i--)
                        {
                            if (Math.Abs(RefAvg - TestResult.cSettlingTime.RawTrace[i]) > TestCon.Threshold)
                            {
                                OnTimeIndex = i; break;
                            }
                        }

                        double check = RefAvg;

                        bool falling = (TestCon.ParameterNote.ToUpper().Contains("CPL010")) ? true : false;

                        // 2020/02/20 Mario added to detect "falling case"
                        if (falling)
                        {
                            for (int i = RefPoint; i < TestResult.cSettlingTime.RawTrace.Length; i++)
                            {
                                RfofftimeLevel = -20;
                                
                                if (TestResult.cSettlingTime.RawTrace[i] > RfofftimeLevel)
                                {
                                    OffTimeIndex = i; break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = RefPoint; i < TestResult.cSettlingTime.RawTrace.Length; i++)
                            {
                                if (TestResult.cSettlingTime.RawTrace[i] < RfofftimeLevel)
                                {
                                    OffTimeIndex = i; break;
                                }
                            }
                        }
                    }

                    TestResult.cSettlingTime.firstVal = TestResult.cSettlingTime.RawTrace[0];
                    TestResult.cSettlingTime.lastVal = TestResult.cSettlingTime.RawTrace[TestResult.cSettlingTime.RawTrace.Length - 1];
                    TestResult.cSettlingTime.RefVal = RefAvg;
                    TestResult.cSettlingTime.onTime = OnTimeIndex / (SaSampleRateMhz * 1e6);
                    TestResult.cSettlingTime.offTime = (OffTimeIndex - ((TestCon.TimingCondition.After_Udelay - HSdiotoSA_offset) * SaSampleRateMhz)) / (SaSampleRateMhz * 1e6);

                    //TestResult.cSettlingTime.offTime = (OffTimeIndex - 2975 ) / (SaSampleRateMhz * 1e6);
                    //if(true)  SaveTrace();



                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CalcResults", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetCurrentPID()
        {
            string currentPIDstr = "";
            currentPIDint = 0;

            currentPIDstr = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "");
            if (!string.IsNullOrEmpty(currentPIDstr))
                int.TryParse(currentPIDstr, out currentPIDint);
            else
                currentPIDint = dutSN;   // Lite Driver

            if ((ResultBuilder.ValidSites.Count > 1) && (Site != 0))
            {
                currentPIDint = currentPIDint + Site;
            }

            return currentPIDint;
        }

        private void SaveTrace()
        {
            string ResultFileName = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, "");
            if (ResultFileName == "") ResultFileName = "Debug";
            string finalfolder = folderpath + ResultFileName + "\\Unit_" + GetCurrentPID().ToString();

            DirectoryInfo GeneratedFolder = new DirectoryInfo(finalfolder);
            GeneratedFolder.Create();

            GeneratedFolder = new DirectoryInfo(finalfolder);
            GeneratedFolder.Create();

            string filePath = finalfolder + @"\" + TestCon.TestParaName + "_" + TestCon.ParameterNote + "_" + DateTime.Now.ToString("_yyyyMMdd_HHmmss_fffff") + ".csv";

            using (StreamWriter debugFile = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                foreach (double n in TestResult.cSettlingTime.RawTrace)
                {
                    debugFile.WriteLine(n);
                }
            }
        }
    }

    public class TimingTestCondition
    {
        public string TestParaName;
        public string Extra;
        public string PowerMode;
        public string Band;
        public string ModulationStd;
        public string WaveformName;
        public double FreqSG;
        public float? TargetPout;
        public float? TargetPin;
        public int ACPaverages;
        public float ExpectedGain;
        public IQ.Waveform IqWaveform;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public cTimingCondition TimingCondition;
        public Operation VsaOperation;
        public Operation VsgOperation = Operation.VSGtoTX;
        public Operation customVsaOperation;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public string CktID;
        public string TestMode;
        public bool TestItotal;
        public bool TestPout;
        public bool TestPin;
        public bool TestGain;
        public bool TestPae;
        public string Regcustom;

        public string DcPinForIccCal;

        public bool IsForwarSearch;
        public double Threshold;
        public string ParameterNote;
        public bool VIO32MA = false;
        public bool VIORESET = false;

        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();
        public Dictionary<string, TestLib.DPAT_Variable> TestDPAT = new Dictionary<string, TestLib.DPAT_Variable>();

        public class cTimingCondition
        {
            public List<MipiSyntaxParser.ClsMIPIFrame> Mipi;
            public double Before_Udelay;
            public double After_Udelay;
            public int nBefore_Command;
        }
    }

    public class TimingTestResult
    {
        public ConcurrentDictionary<string, double> Imeas = new ConcurrentDictionary<string, double>();
        public double Itotal = 0;
        public double Pout = -999;
        public double Pin = -999;
        public double Gain = -999;
        public niComplexNumber[] iqTrace;
        public SettlingTime cSettlingTime;

        public TimingTestResult()
        {
            cSettlingTime = new SettlingTime();
        }

        public class SettlingTime
        {
            public double onTime = -999, offTime = -999, firstVal = -999, lastVal = -999, RefVal = -999;
            public bool blnRiseTime = false;
            public double[] RawTrace = new double[] { 0 };
        }



    }
}

