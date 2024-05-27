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
using System.Diagnostics;

namespace TestLib
{
    public class RFTestFixedOutputPower : RfTestBase
    {
        public override void RfTestCore()
        {

            this.ConfigureVoltageAndCurrent();

            SwStartRun("SendMipiCommands");

            Stopwatch TestTime1 = new Stopwatch();
            TestTime1.Restart();
            TestTime1.Start();

            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);

            double test  = TestTime1.ElapsedMilliseconds;

            SwStopRun("SendMipiCommands");

            // Skip Output Port on Contact Failure (11-Nov-2018)
            SwStartRun("Check Skip Output Port on Contact Failure");
            bool condition1 = SkipOutputPortOnFail;
            condition1 = condition1 && Eq.Site[Site].SwMatrix.IsFailedDevicePort(TestCon.Band, TestCon.VsaOperation);
            condition1 = condition1 && !GU.runningGUIccCal[Site];
            SwStopRun("Check Skip Output Port on Contact Failure");

            // Skip Output Port on Contact Failure (11-Nov-2018)
            if (condition1)
            {
                SwStartRun("CPM.ExecuteAllDummy");
                foreach (string pinName in Eq.Site[Site].DC.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        TestResult.Imeas[pinName] = 0;
                    }
                }
                CustomPowerMeasurement.ExecuteAllDummy(this);
                SwStopRun("CPM.ExecuteAllDummy");

            }
            else
            {
                SwStartRun("Task.Run-ConfigRF");
                Task taskConfigRF = Task.Run(() => ConfigRF());
                SwStopRun("Task.Run-ConfigRF");

                SwStartRun("Task.Run-ConfigRF-Wait");
                taskConfigRF.Wait();
                SwStopRun("Task.Run-ConfigRF-Wait");

                EqRF.NIVST_Rfmx.ThreadFlags[0].WaitOne();
                EqRF.NIVST_Rfmx.ThreadFlags[1].WaitOne();
                
                SwStartRun("RF.Servo");
                bool f = Eq.Site[Site].RF.Servo(out TestResult.Pout, out TestResult.Pin , outputPathGain_withIccCal);
                SwStopRun("RF.Servo");

                SwStartRun("Task.Run-SetupAndMeasureDc");
                Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());
                SwStopRun("Task.Run-SetupAndMeasureDc");
                SwStartRun("dcArmedFlag.WaitOne");
                dcArmedFlag.WaitOne();
                SwStopRun("dcArmedFlag.WaitOne");

                useACPchPowerForPout = turboServo;
                SwStartRun("SA.MeasureAclr");
                
                if (TestResult.Pout == float.NegativeInfinity || TestResult.Pout == 0.0 || TestResult.Pout == float.PositiveInfinity || TestResult.Pout == float.NaN)
                {
                    TestResult.Pout = -50;
                    useACPchPowerForPout = true;
                }
                
                string[] waveform_Array = TestCon.ModulationStd.StartsWith("N") ?
                    TestCon.WaveformName.Remove(TestCon.WaveformName.IndexOf("M")).Split(new[] { "B", "M", "R" }, StringSplitOptions.None) : new[] { "0", "0", "0" };

                TestTime1.Restart();

                if (Eq.Site[Site].RF.IsVST1)
                {
                    if (TestCon.TestEvm) InitiateEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                    if (TestCon.TestAcp1) Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                    if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60) Eq.Site[Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcpNR, new EqRF.Config(EqRF.NIVST_Rfmx.cRfmxAcpNR.GetSpecIteration(), -30));
                    if (TestCon.TestEvm) Eq.Site[Site].RF.SA.MeasureEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type, out TestResult.EVM); //TestResult.EVM = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxEVM, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.CRfmxEVM.GetSpecIteration(), -30)).averageChannelPower;
                    if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60) Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site, TestCon.ModulationStd.StartsWith("N"));
                    if (TestCon.TestEvm) SpecIterationEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                    if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60) EqRF.NIVST_Rfmx.cRfmxAcpNR.SpecIteration();
                }
                else
                {
                    if (TestCon.TestEvm) InitiateEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                    if (TestCon.TestAcp1) Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                    if (TestCon.TestEvm) Eq.Site[Site].RF.SA.MeasureEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type, out TestResult.EVM); //TestResult.EVM = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eR
                    if (TestCon.TestEvm) SpecIterationEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                }
              
                //if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60)
                //{
                //    Eq.Site[Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcpNR, new EqRF.Config(EqRF.NIVST_Rfmx.cRfmxAcpNR.GetSpecIteration(), -30));
                //    Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site, TestCon.ModulationStd.StartsWith("N"));
                //    EqRF.NIVST_Rfmx.cRfmxAcpNR.SpecIteration();
                //}
                double test1 = TestTime1.Elapsed.TotalMilliseconds;// ElapsedMilliseconds;
                SwStopRun("SA.MeasureAclr");
                
                //Eq.Site[Site].RF.SG.SetLofreq();
                
                SwStartRun("Task.Run-SetupAndMeasureDc-Wait");
                taskSetupAndMeasureDc.Wait();
                SwStopRun("Task.Run-SetupAndMeasureDc-Wait");

                SwStartRun("CustomPowerMeasurement-ExecuteAll");
                CustomPowerMeasurement.ExecuteAll(this);
                SwStopRun("CustomPowerMeasurement-ExecuteAll");

                Eq.Site[Site].RF.SG.Abort();
                Eq.Site[Site].RF.SA.Abort();

                // Skip Output Port on Contact Failure (11-Nov-2018)
                if (useACPchPowerForPout)
                {
                    TestResult.Pout = TestResult.ACLR.centerChannelPower;
                }
                if (TestCon.ExpectedGain > 0 &&
                    (TestResult.Pout - TestResult.Pin < TestCon.ExpectedGain - 20))
                {
                    Eq.Site[Site].SwMatrix.AddFailedDevicePort(TestCon.Band, TestCon.VsaOperation);
                }
            }

             Eq.Site[Site].RF.ResetRFSA(false);
            //Eq.Site[Site].RF.ServoEnabled = false;
            int GetIteration = EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration();
            EqLib.EqRF.RfmxAcp.Iteration++;
            //double ICC = TestResult.Imeas["Vcc"];
            //SaveGainVariable();
        }

        public void SpecIterationEVM(string EVMtype)
        {
            switch (EVMtype)
            {
                case "LTE": EqRF.NIVST_Rfmx.cRfmxEVM_LTE.SpecIteration();  break;
                //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                case "NR": EqRF.NIVST_Rfmx.cRfmxEVM_NR.SpecIteration();  break;
                default: throw new Exception(EVMtype + " : Not yet Implemented RFmx");
            }
        }
        public void InitiateEVM(string EVMtype)
        {
            switch (EVMtype)
            {
                
                case "LTE": EqRF.NIVST_Rfmx.cRfmxEVM_LTE.InitiateSpec(EqRF.NIVST_Rfmx.cRfmxEVM_LTE.GetSpecIteration()); break;
                //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                case "NR": EqRF.NIVST_Rfmx.cRfmxEVM_NR.InitiateSpec(EqRF.NIVST_Rfmx.cRfmxEVM_NR.GetSpecIteration()); break;                
                default: throw new Exception(EVMtype + " : Not yet Implemented RFmx");
            }
        }
        private void ConfigRF()
        {
            SwStartRunThread("ConfigRF-Configure");

            double GainAccuracy = 5.0;
            float sgLevelFromIccCal = GU.IccServoVSGlevel[Site, TestCon.TestParaName + "_IccCal"];

            Eq.Site[Site].RF.ServoEnabled = true;

            if (!Eq.Site[Site].RF.ServoEnabled)
            {
                 Eq.Site[Site].RF.ResetRFSA(true);
            }
      

            if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
            {
                double SAPeakLB = Math.Min((TestCon.TargetPout.Value + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) - inputPathGain_withIccCal, 30 + outputPathGain_withIccCal - 0.001);

                IQ.Waveform newWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
                Eq.Site[Site].RF.SA.ModulationStd = TestCon.ModulationStd;
                Eq.Site[Site].RF.SA.WaveformName = TestCon.WaveformName;

                Eq.Site[Site].RF.Configure_Servo(new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration(), TestCon.TargetPout.Value, TestCon.FreqSG, TestCon.ExpectedGain ,SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, 0f, null, null, null, false, 0, 0f, 0, 0, TestCon.TestEvm));
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
                SwStartRunThread("ConfigRF-Configure_Servo");
                Eq.Site[Site].RF.Configure_Servo(new EqRF.Config(TestCon.TargetPout.Value, ExpectedGain, sgLevelFromIccCal, 0.04, turboServo));
                SwStopRunThread("ConfigRF-Configure_Servo");
            }

            SwStopRunThread("ConfigRF-Configure");
        }
    }

    public class RFTestFixedInputPower : RfTestBase
    {
        public override void RfTestCore()
        {

            TestResult.Pin = TestCon.TargetPin.Value;

            this.ConfigureVoltageAndCurrent();

            SwStartRun("SendMipiCommands");
            //Eq.Site[Site].HSDIO.SendVector("VIOOFF");
            //Thread.Sleep(1);
            //Eq.Site[Site].HSDIO.SendVector("VIOON");
            //Thread.Sleep(1);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands");

            //if (TestCon.TestParaName.Contains("B1_HPME_CW_x_FixedPin_-15dBm_1980MHz_3.8Vcc_1.2Vdd_0xFF_0xFF_IN-MB1_ANT2_x")) Thread.Sleep(20);

            // Skip Output Port on Contact Failure (11-Nov-2018)
            SwStartRun("Check Skip Output Port on Contact Failure");
            bool condition1 = SkipOutputPortOnFail;
            condition1 = condition1 && Eq.Site[Site].SwMatrix.IsFailedDevicePort(TestCon.Band, TestCon.VsaOperation);
            condition1 = condition1 && TestCon.TargetPin > -10;
            condition1 = condition1 && !GU.runningGUIccCal[Site];
            SwStopRun("Check Skip Output Port on Contact Failure");

            
            //else Thread.Sleep(5);
            // Skip Output Port on Contact Failure (11-Nov-2018)
            if (condition1)
            {
                foreach (string pinName in Eq.Site[Site].DC.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        TestResult.Imeas[pinName] = 0;
                    }
                }
                CustomPowerMeasurement.ExecuteAllDummy(this);
            }
            else
            {
                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                {
                    SwStartRun("RF.ResetRFSA");
                    Eq.Site[Site].RF.ResetRFSA(false);
                    SwStopRun("RF.ResetRFSA");

                    double SAPeakLB = Math.Min((TestCon.TargetPin.Value + TestCon.ExpectedGain + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) - inputPathGain_withIccCal, 30 + outputPathGain_withIccCal - 0.001);
                    
                    IQ.Waveform newWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
                    Eq.Site[Site].RF.SA.ModulationStd = TestCon.ModulationStd;
                    Eq.Site[Site].RF.SA.WaveformName = TestCon.WaveformName;

                    SwStartRun("RF.Configure_CHP");
                    Eq.Site[Site].RF.Configure_CHP(new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration(), TestCon.TargetPin.Value, TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, 0f, null, null, null, false, 0, 0f, 0, 0, TestCon.TestEvm));
                    SwStopRun("RF.Configure_CHP");

                    SwStartRun("Task.Run-SetupAndMeasureDc");
                    Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());  // moved here KH Power supplies would time out when it was started earlier
                    SwStopRun("Task.Run-SetupAndMeasureDc");

                    useACPchPowerForPout = true;

                    SwStartRun("dcArmedFlag.WaitOne");
                    dcArmedFlag.WaitOne();
                    SwStopRun("dcArmedFlag.WaitOne");

                    SwStartRun("RF.Measure_CHP");
                    Eq.Site[Site].RF.Measure_CHP();

                    EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration()));

                    TestResult.ACLR.centerChannelPower = _RFmxResult.averageChannelPower - outputPathGain_withIccCal;
                    SwStopRun("RF.Measure_CHP");

                    if (TestCon.TestAcp1)
                    {
                        SwStartRun("SA.MeasureAclr");
                        Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                        SwStopRun("SA.MeasureAclr");
                    }

                    SwStartRun("Task.Run-taskSetupAndMeasureDc.Wait");
                    taskSetupAndMeasureDc.Wait();

                    CustomPowerMeasurement.ExecuteAll(this);
                    

                    Eq.Site[Site].RF.SG.Abort();
                    Eq.Site[Site].RF.SA.Abort();
                    SwStopRun("Task.Run-taskSetupAndMeasureDc.Wait");
                                        
                }
                else
                {
                    SwStartRun("SA.Initiate");

                    Eq.Site[Site].RF.ResetRFSA(false);

                    Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, false, true);
                    Eq.Site[Site].RF.ServoEnabled = false;

                    Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
                    Eq.Site[Site].RF.SG.Level = TestCon.TargetPin.Value;
                    Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;

                    Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
                    Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(TestCon.TargetPin.Value + TestCon.ExpectedGain + Eq.Site[Site].RF.ActiveWaveform.PAR, 30 - outputPathGain_withIccCal - 0.001);
                    Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;
                    Eq.Site[Site].RF.SA.Initiate();
                    SwStopRun("SA.Initiate");

                    SwStartRun("Task.Run-SetupAndMeasureDc");
                    Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());  // moved here KH Power supplies would time out when it was started earlier
                    SwStopRun("Task.Run-SetupAndMeasureDc");

                    useACPchPowerForPout = true;

                    SwStartRun("dcArmedFlag.WaitOne");
                    dcArmedFlag.WaitOne();
                    SwStopRun("dcArmedFlag.WaitOne");
                    Thread.Sleep(1);
                    SwStartRun("SG.Initiate");
                    Eq.Site[Site].RF.SG.Initiate();
                    SwStopRun("SG.Initiate");
                    SwStartRun("SA.MeasureAclr");
                    Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                    CustomPowerMeasurement.ExecuteAll(this);
                    SwStopRun("SA.MeasureAclr");
                    SwStartRun("Task.Run-SetupAndMeasureDc");
                    taskSetupAndMeasureDc.Wait();
                    SwStopRun("Task.Run-SetupAndMeasureDc");
                }

                // Skip Output Port on Contact Failure (11-Nov-2018)
                if (useACPchPowerForPout)
                {
                    TestResult.Pout = TestResult.ACLR.centerChannelPower;
                }
                if (TestCon.ExpectedGain > 0 &&
                    (TestResult.Pout - TestResult.Pin < TestCon.ExpectedGain - 20))
                {
                    Eq.Site[Site].SwMatrix.AddFailedDevicePort(TestCon.Band, TestCon.VsaOperation);
                }
            }

            int GetIteration = EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration();
            EqLib.EqRF.RfmxAcp.Iteration++;
            //keng shan added
            float rampLvl = TestCon.TargetPin.Value;
            //do
            //{
            //    Eq.Site[Site].RF.SG.Level = rampLvl - 5;
            //} while (rampLvl < -40);
        }


        }

    public abstract class RfTestBase : TimingBase, iTest
    {
        public RfTestResult TestResult;
        public string MathVariable;
        public RfTestCondition TestCon = new RfTestCondition();
        public byte Site;
        public float inputPathGain_withIccCal = 0;
        public float outputPathGain_withIccCal = 0;
        public bool useACPchPowerForPout = false;
        public const bool turboServo = false;
        protected AutoResetEvent dcArmedFlag = new AutoResetEvent(false);
        private Task CalcTask;
        public TriggerLine dcTrigLine;
        public bool resetSA;
        public bool SkipOutputPortOnFail = false;
        
        public abstract void RfTestCore();


        public bool Initialize(bool finalScript)
        {
            bool success = true;
            InitializeTiming(this.TestCon.TestParaName);

            success &= Eq.Site[Site].RF.LoadWaveform(TestCon.ModulationStd, TestCon.WaveformName);

            if (IQ.Mem.ContainsKey(TestCon.ModulationStd + TestCon.WaveformName))
            {
                TestCon.IqWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
            }

            if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
            {
                outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);
                //outputPathGain_withIccCal = 0; //lindsay
                string[] waveform_Array = TestCon.ModulationStd.StartsWith("N") ?
                    TestCon.WaveformName.Remove(TestCon.WaveformName.IndexOf("M")).Split(new[] { "B", "M", "R" }, StringSplitOptions.None) : new[] { "0", "0" };

                double SAPeakLB = 0f;
           
                if (TestCon.TestMode == "RF") SAPeakLB = 5 + Convert.ToDouble(TestCon.TargetPout) + outputPathGain_withIccCal + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR;
                else SAPeakLB = Math.Min((Convert.ToDouble(TestCon.TargetPin) + TestCon.ExpectedGain + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) + outputPathGain_withIccCal + 5, 30);

                if (TestCon.TestMode == "RF" || TestCon.TestMode == "RF_FIXED_PIN")
                {

                    if (Eq.Site[Site].RF.IsVST1)
                    {

                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm));

                        EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.SpecIteration();

                        if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60)
                        {
                            Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcpNR, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxAcpNR.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                            IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                            TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, false));
                            EqLib.EqRF.NIVST_Rfmx.cRfmxAcpNR.SpecIteration();
                        }

                    }
                    else
                    {

                            Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                          IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                          TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm));

                            EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.SpecIteration();
                        


                    }


                    if (TestCon.TestCustom["H2"])
                    {
                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar2nd, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxH2.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, Site));
                        EqLib.EqRF.NIVST_Rfmx.cRfmxH2.SpecIteration();
                    }
                    if (TestCon.TestCustom["H3"])
                    {
                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar3rd, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxH3.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, Site));
                        EqLib.EqRF.NIVST_Rfmx.cRfmxH3.SpecIteration();
                    }

                    if (TestCon.TestCustom["Cpl"])    // added by hosein 12/29/2019
                    {
                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxChp, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxCHP.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, Site));
                        EqLib.EqRF.NIVST_Rfmx.cRfmxCHP.SpecIteration();
                    }


                    if (TestCon.TestCustom["TxLeakage"])
                    {
                        foreach(TestLib.RfTestCondition.cTxleakageCondition Reflevel in TestCon.TxleakageCondition)
                        {
                            Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxTxleakage, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxTxleakage.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, Reflevel.ReferenceLevel, Reflevel.SpanforTxL, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                   IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                   TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm));
                            EqLib.EqRF.NIVST_Rfmx.cRfmxTxleakage.SpecIteration();
                        }
                    }
                    

                        if (TestCon.TestEvm)
                        {
                            switch (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type)
                            {

                                case "LTE":
                                    Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxEVM, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxEVM_LTE.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, TestCon.ModulationStd + TestCon.WaveformName, TestCon.Band));

                                    EqRF.NIVST_Rfmx.cRfmxEVM_LTE.SpecIteration();
                                    break;
                                //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                                case "NR":
                                    Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxEVM, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.cRfmxEVM_NR.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, TestCon.ModulationStd + TestCon.WaveformName, TestCon.Band));

                                    EqRF.NIVST_Rfmx.cRfmxEVM_NR.SpecIteration();
                                    break;
                                default: throw new Exception(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type + " : Not yet Implemented RFmx");
                            }

                    }
                    
                }
            }


            //Eq.Site[Site].HSDIO.AddVectorsToScript(TestCon.MipiCommands, finalScript);

            return success;
        }

        public int RunTest()
        {
            try
            {


                TestResult = new RfTestResult(TestCon);

                if (ResultBuilder.headerFileMode) return 0;

                SwBeginRun();

                SwStartRun("SetSwitchMatrixPaths");
                SetSwitchMatrixPaths();
                SwStopRun("SetSwitchMatrixPaths");

                if (GU.runningGUIccCal[Site])
                    {
                        RfTestIccCal();
                    }
                    else
                    {
                        RfTest();
                    }

                SwStartRun("Task.Run-CalcResults-SaveGain");
                CalcTask = Task.Run(() => CalcResults());
                SaveGainVariable(CalcTask);
                SwStopRun("Task.Run-CalcResults-SaveGain");

                SwEndRun();

                //    EqLib.EqRF.NIVST_Rfmx.cRfmxAcp.SpecIteration();
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }
        }

        public void RfTest()
        {
            useACPchPowerForPout = false;

            GetLossFactors();

            RfTestCore();

            //SpectralAnalysis sa = new SpectralAnalysis(TestCon.IqWaveform, TestResult.iqTrace);
            //sa.ShowPlot(TestCon.IqWaveform, TestCon.TestParaName);

            //Eq.Site[Site].RF.SG.Abort();
        }

        public void RfTestIccCal()
        {
            this.ConfigureVoltageAndCurrent();
            SwStartRun("SendMipiCommands");
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands");

            if (!TestCon.DcSettings[TestCon.DcPinForIccCal].Test) return;

            string pinTestName = this.TestCon.CktID + "Pin_" + TestCon.TestParaName;
            string poutTestName = this.TestCon.CktID + "Pout_" + TestCon.TestParaName;
            string iccTestName = this.TestCon.CktID + TestCon.DcSettings[TestCon.DcPinForIccCal].iParaName + "_" + TestCon.TestParaName;
            string keyName = TestCon.TestParaName;

            double RFfrequency = TestCon.FreqSG * 1e6;

            float inputPathGain = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);
            float outputPathGain = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            double expectedPout = TestCon.TargetPout.HasValue ?
                TestCon.TargetPout.Value :
                TestCon.TargetPin.Value + TestCon.ExpectedGain;
            //GU.finalRefDataDict[GU.selectedBatch][poutTestName].Values.Average();

            double refLevel = expectedPout + TestCon.IqWaveform.PAR + outputPathGain;

            Eq.Site[Site].RF.ServoEnabled = false;
            Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, false);

            Eq.Site[Site].RF.SG.CenterFrequency = RFfrequency;
            Eq.Site[Site].RF.SG.Level = -100;
            Eq.Site[Site].RF.SG.ExternalGain = 0;

            Eq.Site[Site].RF.SA.CenterFrequency = RFfrequency;
            Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(refLevel, 30.0);
            Eq.Site[Site].RF.SA.ExternalGain = 0;   // set external atten to 0, as GU code expects uncalibrated Pout measurement

            SwStartRun("SASG.Initiate");

            Eq.Site[Site].RF.SA.Initiate();
            Eq.Site[Site].RF.SG.Initiate();
            SwStopRun("SASG.Initiate");

            SwStartRun("ICCCal-Execute");

            IccCal myIccCal = new IccCal(Site, Eq.Site[Site].DC[TestCon.DcPinForIccCal], (float)expectedPout, TestCon.FreqSG, inputPathGain, outputPathGain, 0,
                TestCon.ModulationStd, TestCon.WaveformName,
                poutTestName, pinTestName, iccTestName, keyName, true);

            double icc = 0;

            myIccCal.Execute(ref TestResult.Pin, ref TestResult.Pout, ref icc);

            TestResult.Imeas[TestCon.DcPinForIccCal] = icc;

            SwStartRun("ICCCal-Execute");

            SwStartRun("SASGAbort");
            Eq.Site[Site].RF.SA.Abort();
            Eq.Site[Site].RF.SG.Abort();
            SwStopRun("SASGAbort");

        }

        public void BuildResults(ref ATFReturnResult results)
        {
            bool useCorrelationOffsetsForCalculatedValue = (ResultBuilder.corrFileExists & !GU.runningGU[Site]) | GU.GuMode[Site] == GU.GuModes.Vrfy;
            //bool useCorrelationOffsetsForCalculatedValue = false;

            if (CalcTask != null)
                CalcTask.Wait();

            bool TestIccSum = (TestResult.Imeas.Count(chan => chan.Key.ToUpper().Contains("VCC")) > 1 &&
                TestCon.DcSettings.Count(chan => chan.Key.ToUpper().Contains("VCC") && chan.Value.Test == true) > 1 ?
                true : false);

            if (useCorrelationOffsetsForCalculatedValue)
            {
                if(TestIccSum)
                {
                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (pinName.ToUpper().Contains("VCC") & TestCon.DcSettings[pinName].Test)
                        {
                            TestResult.IccSum += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);
                        }
                    }
                }

                if (TestCon.TestItotal)
                {
                    if(TestIccSum)
                    {
                        TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum);
                    }

                    foreach(string pinName in TestCon.DcSettings.Keys)
                    {
                        if(TestCon.DcSettings[pinName].Test)
                        {
                            if (TestIccSum && pinName.ToUpper().Contains("VCC") || pinName.ToUpper().Contains("VIO"))
                                continue;

                            TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);
                        }
                    }
                }
            }
            else
            {
                if (TestIccSum)
                {
                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (pinName.ToUpper().Contains("VCC") & TestCon.DcSettings[pinName].Test)
                        {
                            TestResult.IccSum += TestResult.Imeas[pinName];
                        }
                    }
                }

                if (TestIccSum)
                {
                    TestResult.Itotal += TestResult.IccSum;
                }

                foreach (string pinName in TestCon.DcSettings.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        if ((TestIccSum && pinName.ToUpper().Contains("VCC"))|| pinName.ToUpper().Contains("VIO"))
                            continue;

                        TestResult.Itotal += TestResult.Imeas[pinName];
                    }
                }
            }

            if (TestCon.TestPin)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pin_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"], "dBm", TestResult.Pin, 4);
            }
            if (TestCon.TestPout)
            {
                if (useACPchPowerForPout & !GU.runningGUIccCal[Site])
                {
                    TestResult.Pout = TestResult.ACLR.centerChannelPower;
                }

                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"], "dBm", TestResult.Pout, 4);
            }
            if (TestCon.TestGain)
            {
                if(useCorrelationOffsetsForCalculatedValue)
                {   
                    if(TestCon.TestParaName.ToUpper().Contains("FIXEDPIN"))
                    {
                        double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", (pout_cal - TestResult.Pin), 4);
                    }
                    else if(TestCon.TestParaName.ToUpper().Contains("FIXEDPOUT"))
                    {
                        double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"]);
                        double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", (pout_cal - pin_cal), 4);
                    }
                }
                else
                {
                    ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", (TestResult.Pout - TestResult.Pin), 4);
                }
            }
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    ResultBuilder.AddResult(this.Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], "A", !ResultBuilder.headerFileMode ? TestResult.Imeas[pinName] : 0, 9);
                }
            }

            if (TestIccSum)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], "A", TestResult.IccSum, 9);
            }
            if (TestCon.TestIeff)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    if (TestIccSum)
                    {
                        TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) *
                        TestCon.DcSettings["Vcc"].Volts /
                        TestCon.DcSettings["Vbatt"].Volts / 0.82) +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]);
                    }
                    else
                    {
                        TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vcc"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Icc"], TestResult.Imeas["Vcc"]) *
                        TestCon.DcSettings["Vcc"].Volts /
                        TestCon.DcSettings["Vbatt"].Volts / 0.82) +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]);
                    }
                    //JOKER
                    //TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) *
                    //    TestCon.DcSettings["Vcc"].Volts /
                    //    3.8 / 1) + 
                    //    GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]);
                }
                else
                {
                    if (TestIccSum) TestResult.Ieff = ((TestResult.IccSum * TestCon.DcSettings["Vcc"].Volts / TestCon.DcSettings["Vbatt"].Volts) / 0.82) + TestResult.Imeas["Vbatt"];
                    else TestResult.Ieff = ((TestResult.Imeas["Vcc"] * TestCon.DcSettings["Vcc"].Volts / TestCon.DcSettings["Vbatt"].Volts) / 0.82) + TestResult.Imeas["Vbatt"];
                    //TestResult.Ieff = ((TestResult.IccSum * TestCon.DcSettings["Vcc"].Volts / 3.8) / 1) + TestResult.Imeas["Vbatt"];
                }

                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Ieff_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Ieff"], "A", TestResult.Ieff, 9);
            }
            if (TestCon.TestPcon)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    if (TestIccSum)
                    {
                        TestResult.Pcon = (GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) * TestCon.DcSettings["Vcc"].Volts +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]) * TestCon.DcSettings["Vbatt"].Volts +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vio1"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vio1"].iParaName], TestResult.Imeas["Vio1"]) * TestCon.DcSettings["Vio1"].Volts);
                    }
                    else
                    {
                        TestResult.Pcon = (GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vcc"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vcc"].iParaName], TestResult.Imeas["Vcc"]) * TestCon.DcSettings["Vcc"].Volts +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]) * TestCon.DcSettings["Vbatt"].Volts +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vio1"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vio1"].iParaName], TestResult.Imeas["Vio1"]) * TestCon.DcSettings["Vio1"].Volts);
                    }
                }
                else
                {
                    if (TestIccSum) TestResult.Pcon = ((TestResult.IccSum * TestCon.DcSettings["Vcc"].Volts) + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts) + (TestResult.Imeas["Vio1"] * TestCon.DcSettings["Vio1"].Volts));
                    else TestResult.Pcon = ((TestResult.Imeas["Vcc"] * TestCon.DcSettings["Vcc"].Volts) + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts) + (TestResult.Imeas["Vio1"] * TestCon.DcSettings["Vio1"].Volts));
                }

                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pcon_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pcon"], "W", TestResult.Pcon, 9);
            }
            if (TestCon.TestItotal)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Itotal_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], "A", TestResult.Itotal, 9);
            }
            if (TestCon.TestPae)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    #region calculate PAE, with correlation factors
                    if (GU.getGUcalfactor(Site, TestCon.CktID + "PAE_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"]) != 0)
                    {
                        MessageBox.Show("Must set PAE Correlation Factor to 0\nfor test " + TestCon.CktID + "PAE_" + TestCon.TestParaName, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        GU.forceReload = true;    // extra nag to force reload
                    }

                    double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"]);
                    double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);

                    float dcPower_cal = 0;
                    string vccName = "";
                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test)
                        {
                            if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                            {
                                vccName = pinName;
                                continue;
                            }

                            double current_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);

                            dcPower_cal += (float)(TestCon.DcSettings[pinName].Volts * current_cal);
                        }
                    }
                    if(TestIccSum)
                    {
                        dcPower_cal += (float)(GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) * TestCon.DcSettings[vccName].Volts);
                    }
                    float pae_cal = Convert.ToSingle((Math.Pow(10, pout_cal / 10) - Math.Pow(10, pin_cal / 10)) / dcPower_cal * 100 / 1000);
                    #endregion

                    if (Math.Abs(pae_cal) > 1000 || float.IsNaN(pae_cal)) pae_cal = -1;

                    ResultBuilder.AddResult(this.Site, TestCon.CktID + "PAE_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"], "%", pae_cal, 4);
                }
                else
                {
                    #region calculate PAE, without correlation factors
                    float dcPower = 0;
                    string vccName = "";
                    foreach (string pinName in TestCon.DcSettings.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test)
                        {
                            if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                            {
                                vccName = pinName;
                                continue;
                            }

                            dcPower += (float)(TestCon.DcSettings[pinName].Volts * TestResult.Imeas[pinName]);
                        }
                    }
                    if (TestIccSum)
                    {
                        dcPower += (float)(TestResult.IccSum * TestCon.DcSettings[vccName].Volts);
                    }

                    float pae = Convert.ToSingle((Math.Pow(10, TestResult.Pout / 10) - Math.Pow(10, TestResult.Pin / 10)) / dcPower * 100 / 1000);
                    #endregion

                    if (Math.Abs(pae) > 1000 || float.IsNaN(pae)) pae = -1;

                    ResultBuilder.AddResult(this.Site, TestCon.CktID + "PAE_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"], "%", pae, 4);
                }
            }
            if (TestCon.TestAcp1)
            {
                int Count = 1;
                foreach (AdjCh adjCH in TestResult.ACLR.adjacentChanPowers)
                {
                    if(Count == 3)
                    {
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName + TestCon.SpecNumber["Para.EUTRA"], "dB", adjCH.lowerDbc, 4);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName + TestCon.SpecNumber["Para.EUTRA"], "dB", adjCH.upperDbc, 4);
                    }
                    else
                    {
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ACLR" + Count], "dB", adjCH.lowerDbc, 4);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ACLR" + Count], "dB", adjCH.upperDbc, 4);
                    }

                    Count++;
                }
            }

            foreach (string NsTestName in ClothoLibAlgo.Calc.NStestConditions.Mem.Keys)
            {
                if (TestCon.TestNS[NsTestName])
                {
                    ResultBuilder.AddResult(this.Site, TestCon.CktID + NsTestName + "_x_" + TestCon.TestParaName, "dB", TestResult.NS[NsTestName], 4);
                }
            }
            if (TestCon.TestEvm)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "EVM_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.EVM"], "%", TestResult.EVM, 4);
            }

            foreach (string testName in CustomPowerMeasurement.Mem.Keys)
            {
                if (TestCon.TestCustom[testName])
                {
                    if (CustomPowerMeasurement.Mem[testName].logUnits == CustomPowerMeasurement.Units.dBm)
                    {   // dBm
                        if (testName.Equals("TxLeakage"))
                        {
                            foreach (RfTestCondition.cTxleakageCondition currTxleakage in TestCon.TxleakageCondition)
                            {
                                //     ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + currTxleakage.RxActiveBand + currTxleakage.Port + "_" + TestCon.TestParaName, CustomPowerMeasurement.Mem["TxLeakage"].logUnits.ToString(), TestResult.CustomTestDbm[testName + currTxleakage.RxActiveBand + currTxleakage.Port + "_" + TestCon.TestParaName], 4);
                                ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + currTxleakage.RxActiveBand + "_" + TestCon.TestParaNameForTxleakage + currTxleakage.Port + "_x_NOTE_" + (TestCon.ParameterNote == "" ? "" : TestCon.ParameterNote + "_") + currTxleakage.SpecNumber, CustomPowerMeasurement.Mem["TxLeakage"].logUnits.ToString(), TestResult.CustomTestDbm[testName + currTxleakage.RxActiveBand + currTxleakage.Port + "_" + TestCon.TestParaName + currTxleakage.SpecNumber], 4);
                            }
                        }
                        else
                            ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber[testName], CustomPowerMeasurement.Mem[testName].logUnits.ToString(), TestResult.CustomTestDbm[testName + "_" + TestCon.TestParaName], 4); //mario
                    }
                    else
                    {   // dBc
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + "_x_" + TestCon.TestParaName, CustomPowerMeasurement.Mem[testName].logUnits.ToString(), TestResult.CustomTestDbm[testName + "_" + TestCon.TestParaName] - TestResult.Pout, 4);
                    }
                }
            }
        }
        
        public void SaveGainVariable(Task GainCalc)
        {
            if (!String.IsNullOrEmpty(this.MathVariable))
            {
                try
                {
                    if (GainCalc != null)
                        GainCalc.Wait();
                    double Gain = TestResult.ACLR.centerChannelPower - TestResult.Pin;
                    if (Calculate.MathCalc.ContainsKey(this.MathVariable))   //kh
                        Calculate.MathCalc[this.MathVariable] = Gain;
                    else
                        Calculate.MathCalc.Add(this.MathVariable, Gain);
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
            //inputPathGain_withIccCal = 0; outputPathGain_withIccCal =0; //lindsay
        }

        public void SetSwitchMatrixPaths()
        {
            Eq.Site[Site].SwMatrix.ActivatePath(TestCon.Band, TestCon.VsaOperation);
            Eq.Site[Site].SwMatrix.ActivatePath(TestCon.Band, TestCon.VsgOperation);

        }

        public void ConfigureVoltageAndCurrent()
        {
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(pinName.ToUpper()) && (pinName.ToUpper() != "VIO1")) continue; // don't force voltage on MIPI pins

                string msg = String.Format("ForceVoltage on pin {0}", pinName);
                SwStartRun(msg);
                Eq.Site[Site].DC[pinName].ForceVoltage(TestCon.DcSettings[pinName].Volts, TestCon.DcSettings[pinName].Current);
                SwStopRun(msg);

            }
            //Console.WriteLine("Site:{0} Thread hashcode:{1}",Site,Thread.CurrentThread.GetHashCode());
        }

        public bool TryGettingCalFactors()
        {
            bool success = true;

            if (ResultBuilder.headerFileMode) return true;

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            foreach (string customTestName in CustomPowerMeasurement.Mem.Keys)
            {
                if (TestCon.TestCustom[customTestName])
                {
                    success &= 0 != CableCal.GetCF(Site, CustomPowerMeasurement.Mem[customTestName].isBandSpecific ? CustomPowerMeasurement.Mem[customTestName].band : TestCon.Band, CustomPowerMeasurement.Mem[customTestName].measurePath, TestCon.FreqSG * (double)CustomPowerMeasurement.Mem[customTestName].channelBandwidth);
                }
            }

            return (success);   // if this method took more than 100ms to complete, that means messageBox was shown because CF was not loaded correctly
        }

        public bool TryGettingCalFactorsfromPort()
        {
            bool success = true;

            if (ResultBuilder.headerFileMode) return true;

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            foreach (string customTestName in CustomPowerMeasurement.Mem.Keys)
            {
                if (TestCon.TestCustom[customTestName])
                {
                    success &= 0 != CableCal.GetCF(Site, CustomPowerMeasurement.Mem[customTestName].isBandSpecific ? CustomPowerMeasurement.Mem[customTestName].band : TestCon.Band, CustomPowerMeasurement.Mem[customTestName].measurePath, TestCon.FreqSG * (double)CustomPowerMeasurement.Mem[customTestName].channelBandwidth);
                }
            }

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
                if (TestCon.TestNS.Values.Contains(true) && (TestResult.iqTrace == null || TestResult.iqTrace.Length == 0))
                {
                    MessageBox.Show("NS testing was requested, but IQdataACLR was not captured.\nNS testing requires that IQdataACLR is captured.");
                }


                if (TestResult.iqTrace != null)
                {
                    SpectralAnalysis sa = new SpectralAnalysis(TestCon.IqWaveform, TestResult.iqTrace);
                    TestResult.ACLR = sa.GetAclrResults();


                    foreach (string customTestName in TestResult.IQdataCustom.Keys)
                    {
                        SpectralAnalysis sa1 = new SpectralAnalysis(TestCon.IqWaveform, TestResult.IQdataCustom[customTestName]);
                        String[] arr = customTestName.Split('_');
                        String channelBandwidthIndex = (arr[0].Contains("TxLeakage") ? "TxLeakage" : arr[0]);

                        TestResult.CustomTestDbm.Add(customTestName, sa1.GetCenterChannelPower((int)CustomPowerMeasurement.Mem[channelBandwidthIndex].channelBandwidth));
                    }
                }
                if (TestCon.TestEvm)
                {
                    //evm = IqWaveform.evmToolkit.CalcEvm(Data.IQdataACLR, IqWaveform);
                }



            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CalcResults", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class RfTestCondition
    {
        public string TestParaName;
        public string TestParaNameForTxleakage;
        public string TestMode;
        public string PowerMode;
        public string Band;
        public string ModulationStd;
        public string WaveformName;
        public double FreqSG;
        public float? TargetPout;
        public float? TargetPin;
        public int ACPaverages;
        public float ExpectedGain;
        public int NumberOfOffsets;
        public double ReflevelforTxleakage;

        public static int Iteration;

        public IQ.Waveform IqWaveform;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public List<cTxleakageCondition> TxleakageCondition;
        public Operation VsaOperation;
        public Operation VsgOperation = Operation.VSGtoTX;
        public Operation customVsaOperation;
        public double expectedLevelDbc;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public string CktID;
        public string CplpreFix;
        public string CABand;
        public string ParameterNote;
        public bool TestIcc;
        public bool TestIcc2;
        public bool TestIbatt;
        public bool TestIdd;
        public bool TestItotal;
        public bool TestPout;
        public bool TestPin;
        public bool TestGain;
        public bool TestPae;
        public bool TestAcp1;
        public bool TestAcp2;
        public bool TestEUTRA;
        public bool TestEvm;
        public bool TestIeff;
        public bool TestPcon;
        public bool TestIio1;

        public Dictionary<string, bool> TestNS = new Dictionary<string, bool>();
        public Dictionary<string, bool> TestCustom = new Dictionary<string, bool>();
        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();

        public string DcPinForIccCal;

        public class cTxleakageCondition
        {
            public List<MipiSyntaxParser.ClsMIPIFrame> Mipi;
            public string RxActiveBand;
            public string Port;
            public string SpecNumber;
            public double ReferenceLevel;
            public double SpanforTxL;
            public Operation ePort;
        }
    }

    public class RfTestResult
    {
        public ConcurrentDictionary<string, double> Imeas = new ConcurrentDictionary<string, double>();
        public ConcurrentDictionary<string, double> Imeas_Ieff = new ConcurrentDictionary<string, double>();

        public double Itotal = 0;
        public double IccSum = 0;
        public double Pout = -999;
        public double Pin = -999;
        public double Gain = -999;
        public double Ieff = 0;
        public double Pcon = 0;
        public niComplexNumber[] iqTrace;
        public AclrResults ACLR;
        public double EVM = -999;

        public Dictionary<string, niComplexNumber[]> IQdataCustom;
        public Dictionary<string, double> CustomTestDbm;
        public Dictionary<string, double> NS;

        public RfTestResult(RfTestCondition TestCon)
        {
            IQdataCustom = new Dictionary<string, niComplexNumber[]>();
            NS = new Dictionary<string, double>();
            CustomTestDbm = new Dictionary<string, double>();
            ACLR = new AclrResults();

            foreach (string name in TestCon.IqWaveform.AclrSettings.Name)
            {
                ACLR.adjacentChanPowers.Add(new AdjCh() { Name = name, lowerDbc = 0, upperDbc = 0 });
            }

            foreach (string name in TestCon.TestCustom.Keys)
            {
                if (TestCon.TestCustom[name])
                {
                    CustomTestDbm[name] = 0;
                }
            }
        }
    }


}
