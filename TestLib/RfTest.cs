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
using System.IO;
using Avago.ATF.LogService;
using Avago.ATF.Logger;
using ProductionLib;

namespace TestLib
{
    public class RFTestFixedOutputPower : RfTestBase
    {
        public String DutyCyclePDM = "FALSE";
        public override void RfTestCore()
        {

            bool burst = true;

            #region Fullwaveform Burst Mode #1

            List<MipiSyntaxParser.ClsMIPIFrame> Mipiburstcommand = new List<MipiSyntaxParser.ClsMIPIFrame>();

            if ((burst == true) && TestCon.WaveformName.Contains("FULL"))
            {
                Eq.Site[Site].HSDIO.selectorMipiPair(1, Site);

                string hex_lsb_addr = "";
                string hex_lsb_data = "";

                for (int servo = 0; servo <= 36; servo++)
                {
                    hex_lsb_addr = "1C";
                    hex_lsb_data = "80";

                    Mipiburstcommand.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_lsb_addr, hex_lsb_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));

                    hex_lsb_addr = "1C";
                    hex_lsb_data = "07";

                    Mipiburstcommand.Add(new MipiSyntaxParser.ClsMIPIFrame(Eq.Site[Site].HSDIO.dutSlaveAddress, hex_lsb_addr, hex_lsb_data, Eq.Site[Site].HSDIO.dutSlavePairIndex, false));
                }
            }

            #endregion

            int servnum = 1;
            servagain:
            this.ConfigureVoltageAndCurrent();

            SwStartRun("SendMipiCommands", Site);

            Stopwatch TestTime1 = new Stopwatch();
            TestTime1.Restart();
            TestTime1.Start();
            //Eq.Site[Site].HSDIO.selectorMipiPair(1, Site);
            //Eq.Site[Site].HSDIO.SendVector("VIOON");
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);

            double test  = TestTime1.ElapsedMilliseconds;

            SwStopRun("SendMipiCommands", Site);

            // Skip Output Port on Contact Failure (11-Nov-2018)
            //SwStartRun("Check Skip Output Port on Contact Failure");
            bool condition1 = false;
            //condition1 = condition1 && Eq.Site[Site].SwMatrix.IsFailedDevicePort(TestCon.Band, TestCon.VsaOperation);
            //condition1 = condition1 && !GU.runningGUIccCal[Site];
            //SwStopRun("Check Skip Output Port on Contact Failure");

            // For Quadsite, single Switch Dio to support dual sites 	
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + Site.ToString();
                SiteTemp = 0;
            }

            SkipTest = false;
            if (SkipOutputPortOnFail && Eq.Site[SiteTemp].SwMatrix.IsFailedDevicePort(BandTemp, TestCon.VsaOperation) && !GU.runningGUIccCal[Site])
            {
                if (!InitTest)
                {
                    condition1 = true;
                    SkipTest = true;
                }
            }
            // Skip Output Port on Contact Failure (11-Nov-2018)
            if (condition1)
            {
                SwStartRun("CPM.ExecuteAllDummy", Site);
                foreach (string pinName in Eq.Site[Site].DC.Keys)
                {
                    if (TestCon.DcSettings[pinName].Test)
                    {
                        TestResult.Imeas[pinName] = 0;
                    }
                }
                CustomPowerMeasurement.ExecuteAllDummy(this);

                if (TestCon.TestEvm)
                {
                    if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM")
                    {
                        Eq.Site[Site].RF.CRfmxIQ_EVM.SpecIteration();
                    }
                    else { SpecIterationEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type); }
                }
                SwStopRun("CPM.ExecuteAllDummy", Site);
            }
            else
            {
                SwStartRun("Task.Run-ConfigRF", Site);
                Task taskConfigRF = Task.Run(() => ConfigRF());
                SwStopRun("Task.Run-ConfigRF", Site);

                SwStartRun("Task.Run-ConfigRF-Wait", Site);
                taskConfigRF.Wait();
                SwStopRun("Task.Run-ConfigRF-Wait", Site);

                Eq.Site[Site].RF.ThreadFlags[0].WaitOne();
                Eq.Site[Site].RF.ThreadFlags[1].WaitOne();
                Stopwatch PaONTime = new Stopwatch();
                PaONTime.Start();

                //EVAL
                SwStartRun("RF.Servo", Site);

                if ((burst== true) && TestCon.WaveformName.Contains("FULL"))
                {
                    Eq.Site[Site].HSDIO.selectorMipiPair(1, Site);
                    Eq.Site[Site].HSDIO.SendMipiCommands(Mipiburstcommand, eMipiTestType.RfBurst);
                }

                try
                {
                    //SwStartRun("RF.Servo");
                    bool f = Eq.Site[Site].RF.Servo(out TestResult.Pout, out TestResult.Pin, outputPathGain_withIccCal);
                    //SwStopRun("RF.Servo");
                    //if (f is false)
                    //{
                    //    goto servagain;
                        //Eq.Site[Site].RF.Servo(out TestResult.Pout, out TestResult.Pin, outputPathGain_withIccCal);
                    //}
                }
                catch (Exception e)
                {
                    string zTargetFile = string.Format(@"C:\Temp\PowerServoErrorLog_{0}.csv", Site);                    
                    
                    using (StreamWriter ErrorFile = new StreamWriter(zTargetFile, true))
                    {
                        string zstring2write = "\r\n" + TestCon.TestParaName + ",";
                        zstring2write += TestCon.TestParaNameForTxleakage + ",";
                        zstring2write += TestCon.TestMode + ",";
                        zstring2write += TestCon.PowerMode + ",";
                        zstring2write += TestCon.Band + ",";
                        zstring2write += TestCon.ModulationStd + ",";
                        zstring2write += TestCon.WaveformName + ",";
                        zstring2write += outputPathGain_withIccCal.ToString() + "\r\n";


                        //Record test number, Pin, Target Pout, pathloss, test header if possible
                        ErrorFile.WriteLine(zstring2write);
                    }

                    //Commented out as long as servo is not looped
                    //Eq.Site[Site].RF.SG.Abort();
                    //Eq.Site[Site].RF.SA.Abort();
                    servnum = servnum + 1;
                    if (servnum < 2)
                    {
                        goto servagain;
                    }

                    TestResult.Pout = 0;
                    TestResult.Pin = 0;
                    ATFLogControl.Instance.Log(LogLevel.Error, "PowerServoError " + TestCon.TestParaName + ", Site: " + Site);
                    //throw;
                }

                //EVAL
                SwStopRun("RF.Servo", Site);

                SwStartRun("Task.Run-SetupAndMeasureDc", Site);
                Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());
                SwStopRun("Task.Run-SetupAndMeasureDc", Site);
                SwStartRun("dcArmedFlag.WaitOne", Site);
                dcArmedFlag.WaitOne();
                SwStopRun("dcArmedFlag.WaitOne", Site);

                useACPchPowerForPout = turboServo;
                SwStartRun("SA.MeasureAclr", Site);
                
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
                    if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60) Eq.Site[Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcpNR, new EqRF.Config(Eq.Site[Site].RF.CRfmxAcpNR.GetSpecIteration(), -30));
                    if (TestCon.TestEvm) Eq.Site[Site].RF.SA.MeasureEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type, out TestResult.EVM); //TestResult.EVM = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxEVM, new EqRF.Config(EqLib.EqRF.NIVST_Rfmx.CRfmxEVM.GetSpecIteration(), -30)).averageChannelPower;
                    if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60) Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site, TestCon.ModulationStd.StartsWith("N"));
                    if (TestCon.TestEvm) SpecIterationEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                    if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60) Eq.Site[Site].RF.CRfmxAcpNR.GetSpecIteration();
                }
                else
                {
                    if (TestCon.TestEvm)
                    {
                        if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM")
                        {
                            double SAPeakLB = Math.Min((TestCon.TargetPout.Value + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) + outputPathGain_withIccCal, 30);
                            Eq.Site[Site].RF.Configure_IQ_EVM(new EqRF.Config_IQ_EVM(Eq.Site[Site].RF.CRfmxIQ_EVM.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, Convert.ToDouble(TestCon.TargetPout.Value), inputPathGain_withIccCal, outputPathGain_withIccCal));
                        }
                        else
                        {
                            InitiateEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                        }
                    }

                    if (TestCon.TestEUTRA || TestCon.TestAcp1)
                    { this.TestCon.IterationACP = Eq.Site[Site].RF.CRfmxAcp.Iteration; }
                    //if (TestCon.TestAcp1 || TestCon.TestEUTRA) Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site); //20200617 Mario
                    //if (TestCon.TestEUTRA) Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site); //20200617 Mario
                    //if (TestCon.TestEvm) Eq.Site[Site].RF.SA.MeasureEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type, out TestResult.EVM); //TestResult.EVM = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eR
                    if (TestCon.TestEvm)
                    {
                        if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM")
                        {
                            TestResult.iqTrace_Multiple = Eq.Site[Site].RF.CRfmxIQ_EVM.RetrieveResults_Multiple(outputPathGain_withIccCal, TestCon.TotalTraces);
                            Eq.Site[Site].RF.CRfmxIQ_EVM.SpecIteration();
                        }
                        else
                        {
                            this.TestCon.IterationEVM = GetIterationEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                            SpecIterationEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type);
                        }
                    }
                }
              
                //if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60)
                //{
                //    Eq.Site[Site].RF.RFmxInitiateSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcpNR, new EqRF.Config(EqRF.NIVST_Rfmx.cRfmxAcpNR.GetSpecIteration(), -30));
                //    Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site, TestCon.ModulationStd.StartsWith("N"));
                //    EqRF.NIVST_Rfmx.cRfmxAcpNR.SpecIteration();
                //}
                double test1 = TestTime1.Elapsed.TotalMilliseconds;// ElapsedMilliseconds;
                SwStopRun("SA.MeasureAclr", Site);

                //Eq.Site[Site].RF.SG.SetLofreq();

                SwStartRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        //Debugger.Break();

                        taskSetupAndMeasureDc.Wait();                        

                        //Debugger.Break();
                    }
                    catch (Exception e)
                    {
                        if (i < 3) continue;
                        //throw;
                        ATFLogControl.Instance.Log(LogLevel.Error, "taskSetupAndMeasureDc.Wait(); in RfTestFixedOutputPower generated an error " + TestCon.TestParaName);
                        ATFLogControl.Instance.Log(LogLevel.Error, TestCon.TestParaName);
                    }
                }
                SwStopRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);


                SwStartRun("CustomPowerMeasurement-ExecuteAll", Site);
                CustomPowerMeasurement.ExecuteAll(this);
                SwStopRun("CustomPowerMeasurement-ExecuteAll", Site);                

                Eq.Site[Site].RF.SG.Abort();
                Eq.Site[Site].RF.SA.Abort(Site);

                PaONTime.Stop();

                if(TestCon.DutyCycle > 0)
                {
                    if (DutyCyclePDM.Equals("TRUE"))
                    {
                        // Send MIpi PDM to emulate TDD DUTY cycle
                        //TX :
                        Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                        Eq.Site[Site].HSDIO.dutSlavePairIndex = 1;
                        Eq.Site[Site].HSDIO.RegWrite("1C", "80");

                        //RX :
                        Eq.Site[Site].HSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI2_SLAVE_ADDR"];
                        Eq.Site[Site].HSDIO.dutSlavePairIndex = 2;
                        Eq.Site[Site].HSDIO.RegWrite("1C", "80");
                    }

                    int PaOffDelay = (int)(((double)(100 * PaONTime.ElapsedMilliseconds / TestCon.DutyCycle)) - (double)PaONTime.ElapsedMilliseconds);
                    uTimer.wait(PaOffDelay);                    
                }

                // Skip Output Port on Contact Failure (11-Nov-2018)
                if (useACPchPowerForPout)
                {
                    TestResult.Pout = TestResult.ACLR.centerChannelPower;
                }
                if (TestCon.ExpectedGain > 0 &&
                    (TestResult.Pout - TestResult.Pin < TestCon.ExpectedGain - 10))
                {
                    Eq.Site[SiteTemp].SwMatrix.AddFailedDevicePort(BandTemp, TestCon.VsaOperation);
                }
            }

             Eq.Site[Site].RF.ResetRFSA(false);
            //Eq.Site[Site].RF.ServoEnabled = false;
            int GetIteration = Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration();
            Eq.Site[Site].RF.CRfmxAcp.SpecIteration();
            //double ICC = TestResult.Imeas["Vcc"];
            //SaveGainVariable();
        }

        public int GetIterationEVM(string EVMtype)
        {
            int _Iteration = 0;
            switch (EVMtype)
            {
                case "LTE": _Iteration = Eq.Site[Site].RF.CRfmxEVM_LTE.GetSpecIteration(); break;
                //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                case "NR": _Iteration = Eq.Site[Site].RF.CRfmxEVM_NR.GetSpecIteration(); break;
                default: throw new Exception(EVMtype + " : Not yet Implemented RFmx");
            }
            return _Iteration;
        }

        public void SpecIterationEVM(string EVMtype)
        {
            switch (EVMtype)
            {
                case "LTE": Eq.Site[Site].RF.CRfmxEVM_LTE.SpecIteration();  break;
                //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                case "NR": Eq.Site[Site].RF.CRfmxEVM_NR.SpecIteration();  break;
                default: throw new Exception(EVMtype + " : Not yet Implemented RFmx");
            }
        }
        public void InitiateEVM(string EVMtype)
        {
            switch (EVMtype)
            {
                
                case "LTE": Eq.Site[Site].RF.CRfmxEVM_LTE.InitiateSpec(Eq.Site[Site].RF.CRfmxEVM_LTE.GetSpecIteration()); break;
                //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                case "NR": Eq.Site[Site].RF.CRfmxEVM_NR.InitiateSpec(Eq.Site[Site].RF.CRfmxEVM_NR.GetSpecIteration()); break;                
                default: throw new Exception(EVMtype + " : Not yet Implemented RFmx");
            }
        }
        private void ConfigRF()
        {
            SwStartRunThread("ConfigRF-Configure");

            double GainAccuracy = 5.0;
            double sgLevelFromIccCal = GU.IccServoVSGlevel[Site, TestCon.TestParaName + "_IccCal"];

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

                Eq.Site[Site].RF.SA.SampleRate = Convert.ToDouble(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].VsgIQrate); //[BurhanEVM]
                IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].VsaIQrate = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].VsgIQrate;

                Eq.Site[Site].RF.Configure_Servo(new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration(), TestCon.TargetPout.Value, TestCon.FreqSG, TestCon.ExpectedGain,SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal,
                    0f, null, null, null, false, 0, 0f, 0, 0, TestCon.TestEvm, Site));
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

            if (TestCon.TestReadReg1C)
            {
                Eq.Site[Site].HSDIO.ReInitializeVIO(TestCon.DcSettings["Vio1"].Volts);
                uTimer.wait_us(50);              
            }

            SwStartRun("SendMipiCommands", Site);
            //Eq.Site[Site].HSDIO.SendVector("VIOOFF");
            //Thread.Sleep(1);
            //Eq.Site[Site].HSDIO.SendVector("VIOON");
            //Thread.Sleep(1);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands", Site);

            //if (TestCon.TestParaName.Contains("B1_HPME_CW_x_FixedPin_-15dBm_1980MHz_3.8Vcc_1.2Vdd_0xFF_0xFF_IN-MB1_ANT2_x")) Thread.Sleep(20);

            // Skip Output Port on Contact Failure (11-Nov-2018)
            //SwStartRun("Check Skip Output Port on Contact Failure");
            bool condition1 = false;
            //condition1 = condition1 && Eq.Site[Site].SwMatrix.IsFailedDevicePort(TestCon.Band, TestCon.VsaOperation);
            //condition1 = condition1 && TestCon.TargetPin > -10;
            //condition1 = condition1 && !GU.runningGUIccCal[Site];
            //SwStopRun("Check Skip Output Port on Contact Failure");

            // For Quadsite, single DIo for dualsites 
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + Site.ToString();
                SiteTemp = 0;
            }

            SkipTest = false;
            if (SkipOutputPortOnFail && Eq.Site[SiteTemp].SwMatrix.IsFailedDevicePort(BandTemp, TestCon.VsaOperation) && !GU.runningGUIccCal[Site] && (TestCon.TargetPin > -10))
            {
                if (!InitTest)
                {
                    condition1 = true;
                    SkipTest = true;
                }
            }

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
                    SwStartRun("RF.ResetRFSA", Site);
                    Eq.Site[Site].RF.ResetRFSA(false);
                    SwStopRun("RF.ResetRFSA", Site);

                    double SAPeakLB = Math.Min((TestCon.TargetPin.Value + TestCon.ExpectedGain + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].PAR) - inputPathGain_withIccCal, 30 + outputPathGain_withIccCal - 0.001);
                    
                    IQ.Waveform newWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
                    Eq.Site[Site].RF.SA.ModulationStd = TestCon.ModulationStd;
                    Eq.Site[Site].RF.SA.WaveformName = TestCon.WaveformName;

                    SwStartRun("RF.Configure_CHP", Site);
                    Eq.Site[Site].RF.Configure_CHP(new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration(), TestCon.TargetPin.Value, TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal,
                        0f, null, null, null, false, 0, 0f, 0, 0, TestCon.TestEvm, Site));
                    SwStopRun("RF.Configure_CHP", Site);

                    SwStartRun("Task.Run-SetupAndMeasureDc", Site);
                    Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());  // moved here KH Power supplies would time out when it was started earlier
                    SwStopRun("Task.Run-SetupAndMeasureDc", Site);

                    useACPchPowerForPout = true;

                    SwStartRun("dcArmedFlag.WaitOne", Site);
                    dcArmedFlag.WaitOne();
                    SwStopRun("dcArmedFlag.WaitOne", Site);

                    SwStartRun("RF.Measure_CHP", Site);
                    //Eq.Site[Site].RF.Measure_CHP();
                    Eq.Site[Site].RF.Measure_CHP(TestCon.Soak_Delay);

                    EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration()));

                    TestResult.ACLR.centerChannelPower = _RFmxResult.averageChannelPower - outputPathGain_withIccCal;
                    SwStopRun("RF.Measure_CHP", Site);

                    if (TestCon.TestAcp1)
                    {
                        SwStartRun("SA.MeasureAclr", Site);
                        Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                        SwStopRun("SA.MeasureAclr", Site);
                    }

                    SwStartRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            //Debugger.Break();

                            taskSetupAndMeasureDc.Wait();

                            //Debugger.Break();
                        }
                        catch (Exception e)
                        {
                            if (i < 3) continue;
                            //throw;
                            //MessageBox.Show("taskSetupAndMeasureDc.Wait(); in RfTestFixedOutputPower generated an error " + "\r\n" + e);
                            ATFLogControl.Instance.Log(LogLevel.Error, "taskSetupAndMeasureDc.Wait(); in RfTestFixedOutputPower generated an error " + TestCon.TestParaName);
                            ATFLogControl.Instance.Log(LogLevel.Error, TestCon.TestParaName);
                        }
                    }
                    SwStopRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);


                    CustomPowerMeasurement.ExecuteAll(this);

                    if (TestCon.TestReadReg1C)
                    {
                        Eq.Site[Site].HSDIO.selectorMipiPair(1, Site);
                        TestResult.Reg1C = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("1C"), 16);
                        TestResult.Register_x48 = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("48"), 16);
                        TestResult.Register_InRevID = Convert.ToInt32(Eq.Site[Site].HSDIO.RegRead("21"), 16);
                    }

                    Eq.Site[Site].RF.SG.Abort();
                    Eq.Site[Site].RF.SA.Abort(Site);
                    //SwStopRun("Task.Run-taskSetupAndMeasureDc.Wait");
                                        
                }
                else
                {
                    SwStartRun("SA.Initiate", Site);

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
                    SwStopRun("SA.Initiate", Site);

                    SwStartRun("Task.Run-SetupAndMeasureDc", Site);
                    Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());  // moved here KH Power supplies would time out when it was started earlier
                    SwStopRun("Task.Run-SetupAndMeasureDc", Site);

                    useACPchPowerForPout = true;

                    SwStartRun("dcArmedFlag.WaitOne", Site);
                    dcArmedFlag.WaitOne();
                    SwStopRun("dcArmedFlag.WaitOne", Site);
                    Thread.Sleep(1);
                    SwStartRun("SG.Initiate", Site);
                    Eq.Site[Site].RF.SG.Initiate();
                    SwStopRun("SG.Initiate", Site);
                    SwStartRun("SA.MeasureAclr", Site);
                    Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site);
                    CustomPowerMeasurement.ExecuteAll(this);
                    SwStopRun("SA.MeasureAclr", Site);
                    SwStartRun("Task.Run-SetupAndMeasureDc", Site);
                    taskSetupAndMeasureDc.Wait();
                    SwStopRun("Task.Run-SetupAndMeasureDc", Site);
                }

                // Skip Output Port on Contact Failure (11-Nov-2018)
                if (useACPchPowerForPout)
                {
                    TestResult.Pout = TestResult.ACLR.centerChannelPower;
                }
                if (TestCon.ExpectedGain > 0 &&
                    (TestResult.Pout - TestResult.Pin < TestCon.ExpectedGain - 10))
                {
                    Eq.Site[SiteTemp].SwMatrix.AddFailedDevicePort(BandTemp, TestCon.VsaOperation);
                }
            }

            int GetIteration = Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration();
            Eq.Site[Site].RF.CRfmxAcp.SpecIteration();
        }


    }

    public abstract class RfTestBase : TimingBase, iTest
    {
        public RfTestResult TestResult;
        public string MathVariable;
        public RfTestCondition TestCon = new RfTestCondition();
        public byte Site;
        public double inputPathGain_withIccCal = 0;
        public double outputPathGain_withIccCal = 0;
        public bool useACPchPowerForPout = false;
        public const bool turboServo = false;
        protected AutoResetEvent dcArmedFlag = new AutoResetEvent(false);
        private Task CalcTask;
        public TriggerLine dcTrigLine;
        public Dictionary<byte, int[]> EqSiteTriggerArray;
        public bool resetSA;
        public bool SkipOutputPortOnFail = false;
        public bool Mordor = false;
        public HiPerfTimer uTimer = new HiPerfTimer();
        public static bool EvmCalibration_Routine_On = false;
        public static bool EvmCalibration_Validation_On = false;
        public static bool EvmCalibration_Pass = true;
        public static Dictionary<string, int>[] EvmWaveformDic = new Dictionary<string, int>[Eq.NumSites];
        public static int[] calFileNo = new int[Eq.NumSites];
        public static string EVMCalDirectory = @"C:\Avago.ATF.Common.x64\EVMCalibration\EDAM\EVM_ACLR_MEAS\";
        public static bool InitTest = false;
        public bool SkipTest = false;

        public abstract void RfTestCore();


        public bool Initialize(bool finalScript)
        {
            bool success = true;
            InitializeTiming(this.TestCon.TestParaName);
            dcTrigLine = (TriggerLine)EqSiteTriggerArray[Site][0];
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

                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm, Site));

                        Eq.Site[Site].RF.CRfmxAcp.SpecIteration();

                        if (TestCon.TestAcp1 && Convert.ToInt16(waveform_Array[1]) > 60)
                        {
                            Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcpNR, new EqRF.Config(Eq.Site[Site].RF.CRfmxAcpNR.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                            IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                            TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw,
                                                                            IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, false, Site));
                            Eq.Site[Site].RF.CRfmxAcpNR.SpecIteration();
                        }

                    }
                    if (TestCon.TestAcp1 == false && TestCon.TestEUTRA) //20200618 Mario
                    {

                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                      IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                      TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestEUTRA, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw,
                                      IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm, Site));

                        Eq.Site[Site].RF.CRfmxAcp.SpecIteration();
                    }
                    else
                    {

                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxAcp, new EqRF.Config(Eq.Site[Site].RF.CRfmxAcp.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                      IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                      TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw,
                                      IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm, Site));

                        Eq.Site[Site].RF.CRfmxAcp.SpecIteration();                  
                    }


                    if (TestCon.TestCustom["H2"])
                    {
                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar2nd, new EqRF.Config(Eq.Site[Site].RF.CRfmxH2.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, TestCon.Harm2MeasBW, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, Site));
                        Eq.Site[Site].RF.CRfmxH2.SpecIteration();
                    }
                    if (TestCon.TestCustom["H3"])
                    {
                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxHar3rd, new EqRF.Config(Eq.Site[Site].RF.CRfmxH3.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, TestCon.Harm2MeasBW, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, Site));
                        Eq.Site[Site].RF.CRfmxH3.SpecIteration();
                    }

                    if (TestCon.TestCustom["Cpl"])    // added by hosein 12/29/2019
                    {
                        Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxChp, new EqRF.Config(Eq.Site[Site].RF.CRfmxCHP.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, SAPeakLB, 0, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                       IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                       TestCon.ModulationStd + TestCon.WaveformName, TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, Site));
                        Eq.Site[Site].RF.CRfmxCHP.SpecIteration();
                    }


                    if (TestCon.TestCustom["TxLeakage"])
                    {
                        foreach(TestLib.RfTestCondition.cTxleakageCondition Reflevel in TestCon.TxleakageCondition)
                        {
                            Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxTxleakage, new EqRF.Config(Eq.Site[Site].RF.CRfmxTxleakage.GetSpecIteration(), Convert.ToSingle(TestCon.TargetPout), TestCon.FreqSG, TestCon.ExpectedGain, Reflevel.ReferenceLevel, Reflevel.SpanforTxL, inputPathGain_withIccCal, outputPathGain_withIccCal, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].RefChBW,
                                                                   IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.ToArray(), IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.OffsetHz.ToArray(),
                                                                   TestCon.ModulationStd + TestCon.WaveformName, TestCon.TestAcp1, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].AclrSettings.BwHz.Count, IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Rbw, 
                                                                   IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds - IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].Markers[0], TestCon.ACPaverages, TestCon.TestEvm, Site));
                            Eq.Site[Site].RF.CRfmxTxleakage.SpecIteration();
                        }
                    }
                    

                    if (TestCon.TestEvm)
                    {
                        switch (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type)
                        {

                            case "LTE":
                                Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxEVM, new EqRF.Config(Eq.Site[Site].RF.CRfmxEVM_LTE.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, TestCon.ModulationStd + TestCon.WaveformName, TestCon.Band, Site));

                                Eq.Site[Site].RF.CRfmxEVM_LTE.SpecIteration();
                                break;
                            //case "LTEA": cRfmxEVM_LTEA.InitiateSpec(c.Iteration); break;
                            case "NR":
                                Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxEVM, new EqRF.Config(Eq.Site[Site].RF.CRfmxEVM_NR.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, TestCon.ModulationStd + TestCon.WaveformName, TestCon.Band, Site));

                                Eq.Site[Site].RF.CRfmxEVM_NR.SpecIteration();
                                break;
                            case "FASTEVM":
                                {
                                    Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxIQ_EVM, new EqRF.Config(Eq.Site[Site].RF.CRfmxIQ_EVM.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, Convert.ToDouble(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].VsgIQrate), Convert.ToDouble(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].SubsetLengthSeconds), 0, 0, TestCon.ModulationStd + TestCon.WaveformName, Site));

                                    Eq.Site[Site].RF.CRfmxIQ_EVM.SpecIteration();
                                }
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

                SwBeginRun(Site);

                SwStartRun("SetSwitchMatrixPaths", Site);
                SetSwitchMatrixPaths();
                SwStopRun("SetSwitchMatrixPaths", Site);

                if (GU.runningGUIccCal[Site])
                {
                    RfTestIccCal();
                }
                else
                {
                    RfTest();
                }

                if ((TestCon.TestEvm) && (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM"))
                {
                    if (EvmCalibration_Routine_On)
                    {
                        FastEvmCal(Site);
                    }

                    if (InitTest == false)
                    {
                        //Load EVMCAL files
                        string filename = "S" + Site + "_" + "IQ_Ref_" + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort;
                        EVMCal.Loop_Setting_Output_Reference_IQ_FileNamePath_Str = EVMCalDirectory + filename + ".csv"; // Use the same file name as the calibration file name

                        if (!File.Exists(EVMCal.Loop_Setting_Output_Reference_IQ_FileNamePath_Str))
                        {
                            ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, "FASTEVM Cal file not available, please run EVMCAL!");
                        }
                        else
                        {
                            EVMCal.ReadIn_Original_IQ_Datafile(filename);
                            EVMCal.Extract_Ref_IQ_Information(filename);
                        }
                    }
                }

                SwStartRun("Task.Run-CalcResults-SaveGain", Site);
                CalcTask = Task.Run(() => CalcResults());
                SaveGainVariable(CalcTask);
                SwStopRun("Task.Run-CalcResults-SaveGain", Site);

                if ((TestCon.TestEvm) && (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM"))
                {
                    Application.DoEvents();
                }

                SwEndRun(Site);

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
            SwStartRun("SendMipiCommands", Site);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands", Site);

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

            SwStartRun("SASG.Initiate", Site);

            Eq.Site[Site].RF.SA.Initiate();
            Eq.Site[Site].RF.SG.Initiate();
            SwStopRun("SASG.Initiate", Site);

            SwStartRun("ICCCal-Execute", Site);

            IccCal myIccCal = new IccCal(Site, Eq.Site[Site].DC[TestCon.DcPinForIccCal], (float)expectedPout, TestCon.FreqSG, inputPathGain, outputPathGain, 0,
                TestCon.ModulationStd, TestCon.WaveformName,
                poutTestName, pinTestName, iccTestName, keyName, true);

            double icc = 0;

            myIccCal.Execute(ref TestResult.Pin, ref TestResult.Pout, ref icc);

            TestResult.Imeas[TestCon.DcPinForIccCal] = icc;

            SwStartRun("ICCCal-Execute", Site);

            SwStartRun("SASGAbort", Site);
            Eq.Site[Site].RF.SA.Abort(Site);
            Eq.Site[Site].RF.SG.Abort();
            SwStopRun("SASGAbort", Site);

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
                            TestResult.IccSum += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_" + TestCon.TestParaName, TestResult.Imeas[pinName]);
                            //TestResult.IccSum += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);
                        }
                    }
                }

                if (TestCon.TestItotal)
                {
                    if(TestIccSum)
                    {
                        //TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum);
                        TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + "IccSum_" + TestCon.TestParaName, TestResult.IccSum);
                    }

                    foreach(string pinName in TestCon.DcSettings.Keys)
                    {
                        if(TestCon.DcSettings[pinName].Test)
                        {
                            if (TestIccSum && pinName.ToUpper().Contains("VCC") || pinName.ToUpper().Contains("VIO"))
                                continue;
                                                       
                            TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_" + TestCon.TestParaName, TestResult.Imeas[pinName]);
                            //TestResult.Itotal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);
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
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pin_" + TestCon.TestParaName , "dBm", TestResult.Pin, 4);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pin_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"], "dBm", TestResult.Pin, 4);
            }
            if (TestCon.TestPout)
            {
                if (useACPchPowerForPout & !GU.runningGUIccCal[Site])
                {
                    TestResult.Pout = TestResult.ACLR.centerChannelPower;
                }

                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pout_" + TestCon.TestParaName , "dBm", TestResult.Pout, 4);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"], "dBm", TestResult.Pout, 4);
            }
            if (TestCon.TestGain)
            {
                if(useCorrelationOffsetsForCalculatedValue)
                {   
                    if(TestCon.TestParaName.ToUpper().Contains("FIXEDPIN"))
                    {
                        double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName) ;
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_" + TestCon.TestParaName , "dB", (pout_cal - TestResult.Pin), 4);
                        //double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);
                       // ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", (pout_cal - TestResult.Pin), 4);
                    }
                    else if(TestCon.TestParaName.ToUpper().Contains("FIXEDPOUT"))
                    {
                        double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_" + TestCon.TestParaName);
                        double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_" + TestCon.TestParaName , "dB", (pout_cal - pin_cal), 4);
                       // double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"]);
                        //double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);
                       // ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", (pout_cal - pin_cal), 4);
                    }
                }
                else
                {

                    ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_" + TestCon.TestParaName , "dB", (TestResult.Pout - TestResult.Pin), 4);
                    //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Gain_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", (TestResult.Pout - TestResult.Pin), 4);
                }
            }
            if (TestCon.TestReadReg1C)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "MIPI_" + TestCon.TestParaName + "Reg1C", "", TestResult.Reg1C);
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "MIPI_" + TestCon.TestParaName + "Regx48", "", TestResult.Register_x48);
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "MIPI_" + TestCon.TestParaName + "RegInRevID", "", TestResult.Register_InRevID);
            }
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    ResultBuilder.AddResult(this.Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_" + TestCon.TestParaName , "A", !ResultBuilder.headerFileMode ? TestResult.Imeas[pinName] : 0, 9);
                    //ResultBuilder.AddResult(this.Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], "A", !ResultBuilder.headerFileMode ? TestResult.Imeas[pinName] : 0, 9);
                }
            }

            if (TestIccSum)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "IccSum_" + TestCon.TestParaName, "A", TestResult.IccSum, 9);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], "A", TestResult.IccSum, 9);
            }
            if (TestCon.TestIeff)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    if (TestIccSum)
                    {
                        //TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) *
                        //TestCon.DcSettings["Vcc"].Volts /
                        //TestCon.DcSettings["Vbatt"].Volts / 0.82) +
                       // GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]);
                        TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + "IccSum_" + TestCon.TestParaName, TestResult.IccSum) *
                       TestCon.DcSettings["Vcc"].Volts /
                       TestCon.DcSettings["Vbatt"].Volts / 0.82) +
                       GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vbatt"]);
                    }
                    else
                    {
                        //TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vcc"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Icc"], TestResult.Imeas["Vcc"]) *
                        //TestCon.DcSettings["Vcc"].Volts /
                        //TestCon.DcSettings["Vbatt"].Volts / 0.82) +
                        //GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]);
                        TestResult.Ieff = (GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vcc"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vcc"]) *
                        TestCon.DcSettings["Vcc"].Volts /
                        TestCon.DcSettings["Vbatt"].Volts / 0.82) +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vbatt"]);
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

                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Ieff_" + TestCon.TestParaName, "A", TestResult.Ieff, 9);
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Ieff_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Ieff"], "A", TestResult.Ieff, 9);
            }
            if (TestCon.TestPcon)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    if (TestIccSum)
                    {
                        //TestResult.Pcon = (GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) * TestCon.DcSettings["Vcc"].Volts +
                        //GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]) * TestCon.DcSettings["Vbatt"].Volts +
                        //GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vio1"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vio1"].iParaName], TestResult.Imeas["Vio1"]) * TestCon.DcSettings["Vio1"].Volts);
                        TestResult.Pcon = ((GU.getValueWithCF(Site, TestCon.CktID + "IccSum_" + TestCon.TestParaName , TestResult.IccSum) * TestCon.DcSettings["Vcc"].Volts)/TestCon.pconMultiplier +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vbatt"]) * TestCon.DcSettings["Vbatt"].Volts +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vio1"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vio1"]) * TestCon.DcSettings["Vio1"].Volts);
                    }
                    else
                    {
                        //TestResult.Pcon = (GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vcc"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vcc"].iParaName], TestResult.Imeas["Vcc"]) * TestCon.DcSettings["Vcc"].Volts +
                        //GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vbatt"].iParaName], TestResult.Imeas["Vbatt"]) * TestCon.DcSettings["Vbatt"].Volts +
                        //GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vio1"].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings["Vio1"].iParaName], TestResult.Imeas["Vio1"]) * TestCon.DcSettings["Vio1"].Volts);
                        TestResult.Pcon = ((GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vcc"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vcc"]) * TestCon.DcSettings["Vcc"].Volts)/ TestCon.pconMultiplier +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vbatt"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vbatt"]) * TestCon.DcSettings["Vbatt"].Volts +
                        GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings["Vio1"].iParaName + "_" + TestCon.TestParaName , TestResult.Imeas["Vio1"]) * TestCon.DcSettings["Vio1"].Volts);
                    }
                }
                else
                {
                    if (TestIccSum) TestResult.Pcon = ((TestResult.IccSum * TestCon.DcSettings["Vcc"].Volts)/TestCon.pconMultiplier + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts) + (TestResult.Imeas["Vio1"] * TestCon.DcSettings["Vio1"].Volts));
                    else TestResult.Pcon = ((TestResult.Imeas["Vcc"] * TestCon.DcSettings["Vcc"].Volts)/TestCon.pconMultiplier + (TestResult.Imeas["Vbatt"] * TestCon.DcSettings["Vbatt"].Volts) + (TestResult.Imeas["Vio1"] * TestCon.DcSettings["Vio1"].Volts));
                }

                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pcon_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pcon"], "W", TestResult.Pcon, 9);
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Pcon_" + TestCon.TestParaName , "W", TestResult.Pcon, 9);

            }
            if (TestCon.TestItotal)
            {
                //ResultBuilder.AddResult(this.Site, TestCon.CktID + "Itotal_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], "A", TestResult.Itotal, 9);
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "Itotal_" + TestCon.TestParaName , "A", TestResult.Itotal, 9);

            }
            if (TestCon.TestPae)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    #region calculate PAE, with correlation factors
                    if (GU.getGUcalfactor(Site, TestCon.CktID + "PAE_" + TestCon.TestParaName) != 0) //ChoonChin 20200730 - fix pae corr issue
                        //if (GU.getGUcalfactor(Site, TestCon.CktID + "PAE_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"]) != 0)   //     hosein 04272020               if (GU.getGUcalfactor(Site, TestCon.CktID + "PAE_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"]) != 0)

                    {
                        MessageBox.Show("Must set PAE Correlation Factor to 0\nfor test " + TestCon.CktID + "PAE_" + TestCon.TestParaName, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        GU.forceReload = true;    // extra nag to force reload
                    }

                    //double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"]);
                    //double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);

                    
                    //double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"]);
                    //double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);

                    //ChoonChin 20200730 - fix pae corr issue
                    double pin_cal = TestResult.Pin + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_" + TestCon.TestParaName);
                    double pout_cal = TestResult.Pout + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName);

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

                            //double current_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);                            
                            //double current_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + TestCon.TestParaName + TestCon.SpecNumber["Para." + TestCon.DcSettings[pinName].iParaName], TestResult.Imeas[pinName]);
                            string ParamName = TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_" + TestCon.TestParaName;
                            double current_cal = GU.getValueWithCF(Site, ParamName, TestResult.Imeas[pinName]);

                            dcPower_cal += (float)(TestCon.DcSettings[pinName].Volts * current_cal);
                        }
                    }
                    if(TestIccSum)
                    {
                        //dcPower_cal += (float)(GU.getValueWithCF(Site, TestCon.CktID + "IccSum_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.ISum"], TestResult.IccSum) * TestCon.DcSettings[vccName].Volts);
                        dcPower_cal += (float)(GU.getValueWithCF(Site, TestCon.CktID + "IccSum_" + TestCon.TestParaName, TestResult.IccSum) * TestCon.DcSettings[vccName].Volts);

                    }
                    float pae_cal = Convert.ToSingle((Math.Pow(10, pout_cal / 10) - Math.Pow(10, pin_cal / 10)) / dcPower_cal * 100 / 1000);
                    #endregion

                    if (Math.Abs(pae_cal) > 1000 || float.IsNaN(pae_cal)) pae_cal = -1;

                    //ResultBuilder.AddResult(this.Site, TestCon.CktID + "PAE_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"], "%", pae_cal, 4);

                    ResultBuilder.AddResult(this.Site, TestCon.CktID + "PAE_" + TestCon.TestParaName , "%", pae_cal, 4);

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
                    //ResultBuilder.AddResult(this.Site, TestCon.CktID + "PAE_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.PAE"], "%", pae, 4);

                    ResultBuilder.AddResult(this.Site, TestCon.CktID + "PAE_" + TestCon.TestParaName , "%", pae, 4);
                }
            }
            if (TestCon.TestAcp1) //20200617 Mario
            {
                int Count = 1;
                foreach (AdjCh adjCH in TestResult.ACLR.adjacentChanPowers)
                {
                    if(Count == 3)
                    {
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName , "dB", adjCH.lowerDbc, 4);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName , "dB", adjCH.upperDbc, 4);
                        double aclrLower = GU.getValueWithCF(Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName, adjCH.lowerDbc);
                        double aclrUpper = GU.getValueWithCF(Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName, adjCH.upperDbc);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_W_" + TestCon.TestParaName, "dB", Math.Max(aclrLower,aclrUpper), 4); 
                    }
                    else
                    {
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName , "dB", adjCH.lowerDbc, 4);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName , "dB", adjCH.upperDbc, 4);
                    }

                    Count++;
                }
            }
            if (TestCon.TestAcp1 == false && TestCon.TestEUTRA) //20200617 Mario
            {
                int Count = 1;
                foreach (AdjCh adjCH in TestResult.ACLR.adjacentChanPowers)
                {
                    if (Count == 3)
                    {
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName, "dB", adjCH.lowerDbc, 4);
                        ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName, "dB", adjCH.upperDbc, 4);
                    }
                    else
                    {
                        //ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_L_" + TestCon.TestParaName, "dB", adjCH.lowerDbc, 4);
                        //ResultBuilder.AddResult(this.Site, TestCon.CktID + adjCH.Name + "_U_" + TestCon.TestParaName, "dB", adjCH.upperDbc, 4);
                    }

                    Count++;
                }
            }

            foreach (string NsTestName in ClothoLibAlgo.Calc.NStestConditions.Mem.Keys)
            {
                if (TestCon.TestNS[NsTestName])
                {
                    ResultBuilder.AddResult(this.Site, TestCon.CktID + NsTestName + "_" + TestCon.TestParaName, "dB", TestResult.NS[NsTestName], 4);
                    //ResultBuilder.AddResult(this.Site, TestCon.CktID + NsTestName + "_x_" + TestCon.TestParaName, "dB", TestResult.NS[NsTestName], 4);

                }
            }
            if (TestCon.TestEvm)
            {
                ResultBuilder.AddResult(this.Site, TestCon.CktID + "EVM_" + TestCon.TestParaName , "%", TestResult.EVM, 4);
               // ResultBuilder.AddResult(this.Site, TestCon.CktID + "EVM_x_" + TestCon.TestParaName + TestCon.SpecNumber["Para.EVM"], "%", TestResult.EVM, 4);
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
                                ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + currTxleakage.RxActiveBand + "_" + TestCon.TestParaNameForTxleakage + currTxleakage.Port + "_NOTE_" + (TestCon.ParameterNote == "" ? "" : TestCon.ParameterNote + "_") + currTxleakage.SpecNumber, CustomPowerMeasurement.Mem["TxLeakage"].logUnits.ToString(), TestResult.CustomTestDbm[testName + currTxleakage.RxActiveBand + currTxleakage.Port + "_" + TestCon.TestParaName + currTxleakage.SpecNumber], 4);
                                //ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + currTxleakage.RxActiveBand + "_" + TestCon.TestParaNameForTxleakage + currTxleakage.Port + "_x_NOTE_" + (TestCon.ParameterNote == "" ? "" : TestCon.ParameterNote + "_") + currTxleakage.SpecNumber, CustomPowerMeasurement.Mem["TxLeakage"].logUnits.ToString(), TestResult.CustomTestDbm[testName + currTxleakage.RxActiveBand + currTxleakage.Port + "_" + TestCon.TestParaName + currTxleakage.SpecNumber], 4);
                            }
                        }
                        else
                            ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + "_" + TestCon.TestParaName , CustomPowerMeasurement.Mem[testName].logUnits.ToString(), TestResult.CustomTestDbm[testName + "_" + TestCon.TestParaName], 4); //mario

                           // ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + "_x_" + TestCon.TestParaName + TestCon.SpecNumber[testName], CustomPowerMeasurement.Mem[testName].logUnits.ToString(), TestResult.CustomTestDbm[testName + "_" + TestCon.TestParaName], 4); //mario
                    }
                    else
                    {   // dBc

                        ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + "_" + TestCon.TestParaName, CustomPowerMeasurement.Mem[testName].logUnits.ToString(), TestResult.CustomTestDbm[testName + "_" + TestCon.TestParaName] - TestResult.Pout, 4);
                        //ResultBuilder.AddResult(this.Site, TestCon.CktID + testName + "_x_" + TestCon.TestParaName, CustomPowerMeasurement.Mem[testName].logUnits.ToString(), TestResult.CustomTestDbm[testName + "_" + TestCon.TestParaName] - TestResult.Pout, 4);
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

        public void SaveGainVariable(Task GainCalc)
        {
            if (!String.IsNullOrEmpty(this.MathVariable))
            {
                try
                {
                    if (GainCalc != null)
                        GainCalc.Wait();

                    if (TestCon.TestEUTRA && (Calculate.MathCalc_EACLR[Site] != null))
                    {
                        double AclrL_Temp = 0;
                        double AclrH_Temp = 0;

                        AclrL_Temp = TestResult.ACLR.adjacentChanPowers[2].lowerDbc;
                        AclrH_Temp = TestResult.ACLR.adjacentChanPowers[2].upperDbc;

                        if (Calculate.MathCalc_EACLR[Site].ContainsKey(this.MathVariable))
                        {
                            Calculate.MathCalc_EACLR[Site][this.MathVariable][0] = AclrL_Temp;
                            Calculate.MathCalc_EACLR[Site][this.MathVariable][1] = AclrH_Temp;
                        }
                        else
                        {
                            Calculate.MathCalc_EACLR[Site].Add(this.MathVariable, new double[2] { AclrL_Temp, AclrH_Temp });
                        }
                    }

                    if (TestCon.TestAcp1 && (Calculate.MathCalc_ACLR1[Site]!= null))
                    {
                        double AclrL_Temp = 0;
                        double AclrH_Temp = 0;

                        AclrL_Temp = TestResult.ACLR.adjacentChanPowers[0].lowerDbc;
                        AclrH_Temp = TestResult.ACLR.adjacentChanPowers[0].upperDbc;

                        if (Calculate.MathCalc_ACLR1[Site].ContainsKey(this.MathVariable))
                        {
                            Calculate.MathCalc_ACLR1[Site][this.MathVariable][0] = AclrL_Temp;
                            Calculate.MathCalc_ACLR1[Site][this.MathVariable][1] = AclrH_Temp;
                        }
                        else
                        {
                            Calculate.MathCalc_ACLR1[Site].Add(this.MathVariable, new double[2] { AclrL_Temp, AclrH_Temp });
                        }
                    }

                    if (TestCon.TestAcp2 && (Calculate.MathCalc_ACLR2[Site]!= null))
                    {
                        double AclrL_Temp = 0;
                        double AclrH_Temp = 0;

                        AclrL_Temp = TestResult.ACLR.adjacentChanPowers[1].lowerDbc;
                        AclrH_Temp = TestResult.ACLR.adjacentChanPowers[1].upperDbc;

                        if (Calculate.MathCalc_ACLR2[Site].ContainsKey(this.MathVariable))
                        {
                            Calculate.MathCalc_ACLR2[Site][this.MathVariable][0] = AclrL_Temp;
                            Calculate.MathCalc_ACLR2[Site][this.MathVariable][1] = AclrH_Temp;
                        }
                        else
                        {
                            Calculate.MathCalc_ACLR2[Site].Add(this.MathVariable, new double[2] { AclrL_Temp, AclrH_Temp });
                        }
                    }

                    if (TestCon.TestGain && (Calculate.MathCalc[Site]!= null))
                    {
                        double Gain = TestResult.ACLR.centerChannelPower - TestResult.Pin;
                        if (Calculate.MathCalc[Site].ContainsKey(this.MathVariable))
                            Calculate.MathCalc[Site][this.MathVariable] = Gain;
                        else
                            Calculate.MathCalc[Site].Add(this.MathVariable, Gain);
                    }

                    //For DC
                    ConcurrentDictionary<string, double> CurrentDic = TestResult.Imeas;
                    if(Calculate.MathCalcCurrent[Site] != null)
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

        private void GetLossFactors()
        {            
            inputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.InputGain);
            outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);
            //inputPathGain_withIccCal = 0; outputPathGain_withIccCal =0; //lindsay
        }

        public void SetSwitchMatrixPaths()
        {
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp +"_"+ Site.ToString();
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
            Eq.Site[Site].DC["Vio1"].ForceVoltage(TestCon.DcSettings["Vio1"].Volts, TestCon.DcSettings["Vio1"].Current);

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

        private niComplexNumber[] Transfer_iqTrace(niComplexNumber[,] iqTrace_Multiple)
        {
            niComplexNumber[] resultIQ = new niComplexNumber[iqTrace_Multiple.Length / 5];
            //
            for (int i = 0; i < iqTrace_Multiple.Length / 5; i++)
            {
                resultIQ[i].Real = iqTrace_Multiple[0, i].Real;
                resultIQ[i].Imaginary = iqTrace_Multiple[0, i].Imaginary;
            }
            //
            return resultIQ;
        }

        static string dateCode = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", DateTime.Now);

        private void FastEvmCal(byte site)
        {
            if (EvmWaveformDic[site].ContainsKey(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort))
            {
                return; // Removed for debugging only !!!
            }
            else
            {
                EvmWaveformDic[site].Add(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort, calFileNo[site]);
                calFileNo[site]++;
            }

            //FastEVMAnalysis fastEvm = new FastEVMAnalysis(); // Initalize

            // Overwrite default values
            string filename_Y = "c:\\temp\\test_S" + site + "_" + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort + ".csv";

            // Purely for debugging and calibration!!!            
            int NOP = TestResult.iqTrace_Multiple.Length / TestCon.TotalTraces;

            string[] tmp_string = new string[NOP];

            // Put in the header at the first line
            for (int Counter = 0; Counter < (NOP); Counter++)
            {
                // Capture the first record only for calibration routine
                tmp_string[Counter] = TestResult.iqTrace_Multiple[0, Counter].Real.ToString().Trim() + " , " + TestResult.iqTrace_Multiple[0, Counter].Imaginary.ToString().Trim();
            }
            File.WriteAllLines(filename_Y, tmp_string);

            // Use the same IqTrace for Calibration waveform extraction in this case
            EVMCal.Input_Pilot_Length = 50;
            EVMCal.Input_First_Header_Search_Guess_Point = 10;

            // Define file naming and path
            EVMCal.CalDIR = EVMCalDirectory;
            EVMCal.Calibrate_Extract_Output_Synced_Original_Reference_FileNamePath = EVMCal.CalDIR + "S" + site + "_"+ "IQ_Sync_" + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort + ".csv";
            EVMCal.Calibrate_Extract_Output_Reference_IQ_FileNamePath = EVMCal.CalDIR + "S" + site + "_" + "IQ_Ref_" + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort + ".csv";

            // Save a backup
            string[] backupFilesArray = new string[] {EVMCal.Calibrate_Extract_Output_Synced_Original_Reference_FileNamePath,
                EVMCal.Calibrate_Extract_Output_Reference_IQ_FileNamePath};

            foreach (string backupFile in backupFilesArray)
            {
                string tempFile = backupFile;
                if (File.Exists(tempFile))
                {
                    tempFile = tempFile.Insert(tempFile.LastIndexOf('\\'), "\\Backup");
                    tempFile = tempFile.Insert(tempFile.LastIndexOf('\\'), "\\" + dateCode);
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFile));
                    File.Delete(tempFile);
                    File.Copy(backupFile, tempFile);
                }
            }

            EVMCal.EVM_Generate_Ref_IQ_Ver2("", filename_Y, false, true, 0, 0, 0, 0, true, 0, 0); // Generate the IQ calibration file for this particular waveform
        }

        private void CalcResults()
        {
            try
            {
                if (TestCon.TestNS.Values.Contains(true) && (TestResult.iqTrace == null || TestResult.iqTrace.Length == 0))
                {
                    MessageBox.Show("NS testing was requested, but IQdataACLR was not captured.\nNS testing requires that IQdataACLR is captured.");
                }


                if (TestResult.iqTrace != null && IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type != "FASTEVM")
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

                if (TestCon.TestAcp1 || TestCon.TestEUTRA)
                {
                    if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM")
                    {
                        FastEVMAnalysis_ACLR fastEvm_ACLR = new FastEVMAnalysis_ACLR();
                        TestResult.ACLR = fastEvm_ACLR.Capture_ACLR(TestCon.IqWaveform, TestResult.iqTrace_Multiple, TestCon.TotalTraces);
                    }
                    else
                    {
                        if (SkipTest == false)
                        {
                            Eq.Site[Site].RF.SA.MeasureAclr(ref TestResult.ACLR, ref TestResult.iqTrace, ref Site, TestCon.IterationACP);
                        }

                    }
                }

                if (TestCon.TestEvm)
                {
                    if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type == "FASTEVM")
                    {
                        TestResult.EVM = -999;

                        if (InitTest == true) { return; }

                        // Best setting as of 23 Sept 2021 => [20:2:2]:10:[20:10]:[1]:[10:5]
                        // Best setting as of 20 Oct 2021 => [10:2:2]:10:[0:5:1]:[50:75]:[3]:[10:5]:F:F

                        FastEVMAnalysis fastEvm = new FastEVMAnalysis();

                        // Overwrite default values
                        fastEvm.Record_Number_Total = TestCon.TotalTraces; // 10; Can reduce the number depending on the quality of measured IQ from VST !!!
                        fastEvm.EVM_Iteration_Value = 2; // Typically will only need value = 2 for good extraction;
                        //
                        fastEvm.MicroTune_Cycle = 2; // Number of micro tune cycle
                        fastEvm.EVM_Sampling_Percentage = 10; // Obseleted : EVM fast calculation sampling percentage

                        fastEvm.Avoid_Region_Percentage = 0; // In theory this number must be set to 0
                        fastEvm.BlockSplit_Number = 5; // 10; // 5; // Number to block to be used, seems value of 10 (230pts) will have worse results than value of 5 (460 pts)
                        fastEvm.Block_Averaging_Number = 1; // Seems value set to 1 still giving good result

                        //fastEvm.IQ_Min_Limit_Percentage = 50; // Seems 45 a good number to start with
                        //fastEvm.IQ_Max_Limit_Percentage = 75; // seems 75 a good number to start with

                        fastEvm.IQ_Averaging_Level = 3; // Seems value 3 IQ data averaging value still the best

                        fastEvm.Input_First_Header_Search_Guess_Point = 10;
                        fastEvm.Input_First_Header_Search_Window_Width_NOP = 5;

                        fastEvm.NOP_Multiplierx2_Activate = false;
                        fastEvm.NOP_Multiplier_Value = 1; // Meaning NOP multiplication by 2^n

                        fastEvm.LowPassFilter_Activate = false;

                        fastEvm.IQ_Capture_Filename = "IQ_Capture_" + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort;

                        if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() == "SC30B100M270R0S3X50CFRPAPR71DB3XFASTEVM")
                        {
                            fastEvm.IQ_Min_Limit_Percentage = 65;
                            fastEvm.IQ_Max_Limit_Percentage = 90;
                        }
                        else if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() == "SC30B100M273R0S3X50CFRPAPR95DB3XFASTEVM")
                        {
                            fastEvm.IQ_Min_Limit_Percentage = 50;
                            fastEvm.IQ_Max_Limit_Percentage = 75;
                        }
                        else if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() == "SC30B20M51R0S3X10SLOT1CFR9P5FASTEVM")
                        {
                            fastEvm.IQ_Min_Limit_Percentage = 35;
                            fastEvm.IQ_Max_Limit_Percentage = 90;
                        }
                        else if (IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() == "SC30B40M106R0S3X10SLOT1CFR9P5FASTEVM")
                        {
                            fastEvm.IQ_Min_Limit_Percentage = 35;
                            fastEvm.IQ_Max_Limit_Percentage = 90;
                        }
                        else
                        {
                            fastEvm.IQ_Min_Limit_Percentage = 0;
                            fastEvm.IQ_Max_Limit_Percentage = 100;
                        }

                        fastEvm.Waveform_SamplingRate_Hz = Convert.ToDouble(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].VsaIQrate);
                        if (TestCon.WaveformName.Contains("B5M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 5e6;
                        else if (TestCon.WaveformName.Contains("B10M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 10e6;
                        else if (TestCon.WaveformName.Contains("B20M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 20e6;
                        else if (TestCon.WaveformName.Contains("B30M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 30e6;
                        else if (TestCon.WaveformName.Contains("B40M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 40e6;
                        else if (TestCon.WaveformName.Contains("B50M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 50e6;
                        else if (TestCon.WaveformName.Contains("B60M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 60e6;
                        else if (TestCon.WaveformName.Contains("B70M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 70e6;
                        else if (TestCon.WaveformName.Contains("B80M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 80e6;
                        else if (TestCon.WaveformName.Contains("B90M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 90e6;
                        else if (TestCon.WaveformName.Contains("B100M")) fastEvm.Waveform_Bandwidth_Cutoff_Hz = 100e6;
                        else fastEvm.Waveform_Bandwidth_Cutoff_Hz = 100e6;

                        string filename = "S"+ Site + "_" + "IQ_Ref_" + IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort;

                        string Loop_Setting_Output_Reference_IQ_FileNamePath_Str = EVMCalDirectory + filename + ".csv"; // Use the same file name as the calibration file name

                        if (!File.Exists(Loop_Setting_Output_Reference_IQ_FileNamePath_Str)) { return; }

                        TestResult.EVM = fastEvm.FastEVM_Calculate_Method2(TestResult.iqTrace_Multiple, EVMCal.OriIQDataDic[filename], EVMCal.OriIQDataNumDic[filename], EVMCal.OriIQDataNOPDic[filename],
                            EVMCal.RefIQxDic[filename], EVMCal.RefIQxNOPDic[filename]); // Perform calculation, return EVM result

                        if(EvmCalibration_Validation_On)
                        {
                            // To catch gross contact/Missing unit issue during EVMCAL
                            if (TestResult.EVM > 3)
                            {
                                ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, "Possible Contact Issue: " +
                                    IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].WaveformName.Trim() + "_" + TestCon.FreqSG + "_" + TestCon.InputPort + "_" + TestCon.OutputPort + "_ResidualEVM > 3%");

                                EvmCalibration_Pass = false;
                            }
                        }

                        //Console.WriteLine("EVM = " + TestResult.EVM.ToString());
                    }
                    else
                    {
                        if (SkipTest == false)
                        {
                            //evm = IqWaveform.evmToolkit.CalcEvm(Data.IQdataACLR, IqWaveform);
                            Eq.Site[Site].RF.SA.MeasureEVM(IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName].EVM_Type, out TestResult.EVM, TestCon.IterationEVM);
                        }
                    }
                }              
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CalcResults", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static class EVMCal
    {
        public static string Loop_Setting_Output_Reference_IQ_FileNamePath_Str = "C:\\ExpertCalSystem.Data\\MagicBox\\EVM_ACLR_Measurement\\Output_Ref_IQ_Train_V1p3.csv";

        public static Dictionary<string, double[,]> OriIQDataDic = new Dictionary<string, double[,]>();
        public static Dictionary<string, double[,]> RefIQxDic = new Dictionary<string, double[,]>();
        public static Dictionary<string, int> RefIQxNOPDic = new Dictionary<string, int>();
        public static Dictionary<string, int> OriIQDataNumDic = new Dictionary<string, int>();
        public static Dictionary<string, int> OriIQDataNOPDic = new Dictionary<string, int>();

        public static void ReadIn_Original_IQ_Datafile(string key)
        {
            if (OriIQDataDic.ContainsKey(key))
            {
                return;
            }

            // Added as VST implementation require Header placement further out in time, which we need to keep track the location later for correct signal sync
            double[,] tmp_header = new double[3, 2500];

            int Header_Start_Pointer = 0;
            int Counter_Header = 0;

            string[] tmp_string3;
            string[] tmp_split2;

            tmp_string3 = File.ReadAllLines(Loop_Setting_Output_Reference_IQ_FileNamePath_Str); // Input file (Reference IQ)
            int NumberOfPoint_Original_IQ = tmp_string3.Length;
            //
            // Start Header search
            for (int i = 0; i < NumberOfPoint_Original_IQ; i++)
            {
                tmp_split2 = tmp_string3[i].Split(',').ToArray();

                // Load in header trace from original IQ train (search from original iq train using mask)
                // Look for mask = -1
                if ((Convert.ToInt32(tmp_split2[2].Trim()) == -1))
                {
                    // Capture the location of the header in the waveform ... Burhan 5th March 2021
                    if (Counter_Header == 0)
                    {
                        Header_Start_Pointer = i;
                    }

                    tmp_header[0, Counter_Header] = Convert.ToDouble(tmp_split2[0].Trim());
                    tmp_header[1, Counter_Header] = Convert.ToDouble(tmp_split2[1].Trim());
                    tmp_header[2, Counter_Header] = Convert.ToDouble(tmp_split2[2].Trim());
                    Counter_Header = Counter_Header + 1;
                }

                // Exit for if the header signal found when mask transition to actual IQ dataset ( > 0 )
                if (Convert.ToInt32(tmp_split2[2].Trim()) > 0)
                {
                    break;
                }
            }

            OriIQDataDic.Add(key, tmp_header);
            OriIQDataNumDic.Add(key, Counter_Header);
            OriIQDataNOPDic.Add(key, NumberOfPoint_Original_IQ);
        }

        public static void Extract_Ref_IQ_Information(string key)
        {
            if (RefIQxDic.ContainsKey(key))
            {
                return;
            }

            double[,] tmp_Ref_IQx = new double[3, 2500];
            // Load back in Ref_IQ and Measured_IQ
            string[] tmp_string3;
            string[] tmp_split3;

            tmp_string3 = File.ReadAllLines(Loop_Setting_Output_Reference_IQ_FileNamePath_Str); // Input file
            int NumberOfPoint_Ref_IQ = tmp_string3.Length;

            for (int i = 0; i < NumberOfPoint_Ref_IQ; i++)
            {
                tmp_split3 = tmp_string3[i].Split(',').ToArray();
                tmp_Ref_IQx[0, i] = Convert.ToDouble(tmp_split3[0].Trim());
                tmp_Ref_IQx[1, i] = Convert.ToDouble(tmp_split3[1].Trim());
                tmp_Ref_IQx[2, i] = Convert.ToDouble(tmp_split3[2].Trim());
            }

            RefIQxDic.Add(key, tmp_Ref_IQx);
            RefIQxNOPDic.Add(key, NumberOfPoint_Ref_IQ);
        }

        // *****************************************************************************************************************************************
        // *****************************************************************************************************************************************
        // Calibration routine
        // *****************************************************************************************************************************************
        // *****************************************************************************************************************************************
        private const double pi = 3.14159265359;

        public static int Input_Pilot_Length = 50;
        public static int Input_First_Header_Search_Guess_Point = 10;

        // Output
        public static string EVM_First_Header_Marker = "";
        public static string EVM_Second_Header_Marker = "";
        public static string EVM_IQ_Train_Length = "";

        // Input
        public static string Calibrate_Header_Delay = "";
        public static string Calibrate_Header_NOP = "";

        public static string CalDIR = "";

        public static string Calibrate_Extract_Output_Synced_Original_Reference_FileNamePath = "C:\\ExpertCalSystem.Data\\MagicBox\\EVM_ACLR_Measurement\\Output_Ref_IQ_Output_Test.csv";
        public static string Calibrate_Extract_Output_Reference_IQ_FileNamePath = "C:\\ExpertCalSystem.Data\\MagicBox\\EVM_ACLR_Measurement\\Output_Ref_IQ_Train_V1p3.csv";

        public static void EVM_Generate_Ref_IQ_Ver2(string Original_IQ_FileNamePath, string Output_FileNamePath,
                         bool Generate_Simulation_Ref_IQ, bool Ref_IQ_Flag,
                         int undefine_point_NOP, double random_amplitude_input, double gain_amplitude_input, double Deg_Offset,
                         bool Long_Search_Flag, int First_Header_Search_Guess_Point, int First_Header_Search_Window_Width_NOP)
        {
            // *****************************************************************************************************
            // *****************************************************************************************************
            // Note that Ver2 will not require the original IQ information, will work based on the feeded IQ dataset
            // *****************************************************************************************************
            // *****************************************************************************************************

            var watch = new System.Diagnostics.Stopwatch();

            string[] tmp_string3;
            string[] tmp_split2;

            tmp_string3 = File.ReadAllLines(Output_FileNamePath); // Input file (just this one single file !!!)
            int NumberOfPoint_Original_IQ = tmp_string3.Length;

            int Header_Delay = Input_First_Header_Search_Guess_Point; //200;
            int Header_NOP = Input_Pilot_Length; // 400;
            int Counter_Header = 0;

            double[,] tmp_header = new double[2, Header_NOP];

            // Generate Header waveform ( act as pilot signal as well )
            for (int i = Header_Delay; i < (Header_NOP + Header_Delay); i++)
            {
                tmp_split2 = tmp_string3[i].Split(',').ToArray();
                tmp_header[0, Counter_Header] = Convert.ToDouble(tmp_split2[0].Trim());
                tmp_header[1, Counter_Header] = Convert.ToDouble(tmp_split2[1].Trim());
                Counter_Header = Counter_Header + 1;
            }

            // [0] = I dataset
            // [1] = Q dataset
            // [2] = Header merit
            string[,] Train_Content = new string[3, NumberOfPoint_Original_IQ];

            for (int i = 0; i < NumberOfPoint_Original_IQ; i++)
            {
                if (tmp_string3[i].Trim() != "") // Avoid null issue of array with no data ... 
                {
                    tmp_split2 = tmp_string3[i].Split(',').ToArray();
                    Train_Content[0, i] = tmp_split2[0].Trim();
                    Train_Content[1, i] = tmp_split2[1].Trim();
                }
            }

            // ********************
            // Capture header array 
            // ********************
            SPAR_Correction.Real_Imag Var_One;
            double[] Test_Header_Angle = new double[Counter_Header];
            double[] Reference_Header_Angle = new double[Counter_Header];

            int counter_j = 0;

            for (int j = 0; j < Counter_Header; j++)
            {
                Var_One.Real = Convert.ToDouble(tmp_header[0, j]);
                Var_One.Imag = Convert.ToDouble(tmp_header[1, j]);
                // Calculate Ref Phase angle
                Reference_Header_Angle[j] = Angle_Cal(Var_One.Real, Var_One.Imag); // Leave the phase results within the +/- 180 Deg ...
            }

            int First_Header_Marker = Header_Delay;
            int Second_Header_Marker = -9999;

            double Merit_Search_Value_SecondHeader = -9999;

            watch.Reset();
            watch.Start();

            double Test_Header_Angle_Upper;
            double Test_Header_Angle_Middle;
            double Test_Header_Angle_Lower;
            double Delta_Angle = 9e99;

            double Max_Angle = -9e9;
            double Min_Angle = 9e9;

            // First header already set and known, just need to find second header
            // Check the correlation merit, use the RSquare as merit number
            for (int i = (Header_Delay + Header_NOP); i < NumberOfPoint_Original_IQ; i++)  // *** <= Can cut down the window further to gain speed
            {
                counter_j = 0;

                // Make sure not exceeding the dataset length ...
                if ((Counter_Header + i) < NumberOfPoint_Original_IQ)
                {
                    Max_Angle = -9e9;
                    Min_Angle = 9e9;

                    // Populate both arrays
                    for (int j = 0; j < Counter_Header; j++)
                    {
                        Var_One.Real = Convert.ToDouble(Train_Content[0, j + i]);
                        Var_One.Imag = Convert.ToDouble(Train_Content[1, j + i]);

                        // Calculate measured header Phase angle
                        Test_Header_Angle[counter_j] = Angle_Cal(Var_One.Real, Var_One.Imag);

                        Delta_Angle = 9e99;

                        Test_Header_Angle_Upper = Test_Header_Angle[counter_j] + 360;
                        Test_Header_Angle_Middle = Test_Header_Angle[counter_j];
                        Test_Header_Angle_Lower = Test_Header_Angle[counter_j] - 360;

                        if (Math.Abs(Test_Header_Angle_Upper - Reference_Header_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Upper - Reference_Header_Angle[j]);
                            Test_Header_Angle[counter_j] = Test_Header_Angle_Upper;
                        }

                        if (Math.Abs(Test_Header_Angle_Middle - Reference_Header_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Middle - Reference_Header_Angle[j]);
                            Test_Header_Angle[counter_j] = Test_Header_Angle_Middle;
                        }

                        if (Math.Abs(Test_Header_Angle_Lower - Reference_Header_Angle[j]) < Delta_Angle)
                        {
                            Delta_Angle = Math.Abs(Test_Header_Angle_Lower - Reference_Header_Angle[j]);
                            Test_Header_Angle[counter_j] = Test_Header_Angle_Lower;
                        }

                        if (Test_Header_Angle[j] < Reference_Header_Angle[j]) // Move by one cycle up (360Deg if the test signal less then the ref signal)
                        {
                            Test_Header_Angle[j] = Test_Header_Angle[j] + 360;
                        }

                        if (Max_Angle < (Test_Header_Angle[j] - Reference_Header_Angle[j]))
                        {
                            Max_Angle = (Test_Header_Angle[j] - Reference_Header_Angle[j]);
                        }

                        if (Min_Angle > (Test_Header_Angle[j] - Reference_Header_Angle[j]))
                        {
                            Min_Angle = (Test_Header_Angle[j] - Reference_Header_Angle[j]);
                        }

                        counter_j += 1;
                    }

                    // Manage remaining issue with 360 max delta which some time is an issue
                    if ((Max_Angle - Min_Angle) > 300) // If the answer greated than 300Deg separation, typical a good one with now issue will have a lot less than 300 Deg
                    {
                        for (int j = 0; j < Counter_Header; j++)
                        {
                            Test_Header_Angle[j] = Test_Header_Angle[j] - 45; // create reasonable size degree separation
                            if (Test_Header_Angle[j] < Reference_Header_Angle[j]) // Move by one cycle up (360Deg if the test signal less then the ref signal)
                            {
                                Test_Header_Angle[j] = Test_Header_Angle[j] + 360;
                            }
                            Test_Header_Angle[j] = Test_Header_Angle[j] + 45; // re-nstate the offset back after re-alignment
                        }
                    }

                    // Scan for Header directly (in this case both I and Q dataset are used)
                    Train_Content[2, i] = RSquare_Value(Reference_Header_Angle, Test_Header_Angle).ToString().Trim(); // Save the RSquare merit results for later process use

                    // Search for second header within given window
                    if (Convert.ToDouble(Train_Content[2, i]) > Merit_Search_Value_SecondHeader)
                    {
                        Merit_Search_Value_SecondHeader = Convert.ToDouble(Train_Content[2, i]);
                        Second_Header_Marker = i;
                    }
                }
                else
                {
                    Train_Content[2, i] = "0";
                }
            }

            // Perform alignment and averaging ( Use header as alignment marker )
            // Note that for Ref_IQ we will not apply and parameter coef e.g. Gain and Offset !!!
            // Make sure the Original and the Ref having the same sampling rate (use this test to check)

            watch.Stop();

            // ***************************
            // Generate new file as output
            // ***************************

            EVM_First_Header_Marker = First_Header_Marker.ToString().Trim();
            EVM_Second_Header_Marker = Second_Header_Marker.ToString().Trim();
            EVM_IQ_Train_Length = NumberOfPoint_Original_IQ.ToString().Trim();

            // Save to array class for later use
            string[] tmp_string5 = new string[Second_Header_Marker - First_Header_Marker];

            int Counter_XXX = 0;

            string[] tmp_string6 = new string[Second_Header_Marker - First_Header_Marker];

            for (int i = First_Header_Marker; i < Second_Header_Marker; i++)
            {
                tmp_string5[Counter_XXX] = Train_Content[0, i].Trim() + "," +
                                 Train_Content[1, i].Trim();

                string Mask_Value = "";

                if ((i >= Header_Delay) && (i < (Header_NOP + Header_Delay)))
                {
                    Mask_Value = "-1";
                    //
                    //Making sure header (which operate as pilot signal as well operate at more linear region)
                    //Train_Content[0, i] = (Convert.ToDouble(Train_Content[0, i]) / 10).ToString();
                    //Train_Content[1, i] = (Convert.ToDouble(Train_Content[1, i]) / 10).ToString();
                    //
                }
                else
                {
                    Mask_Value = "1";
                }
                tmp_string6[Counter_XXX] = Train_Content[0, i].Trim() + "," +
                                 Train_Content[1, i].Trim() + "," +
                                 Mask_Value.Trim();
                Counter_XXX = Counter_XXX + 1;
            }

            //string Calibrate_Extract_Output_Synced_Original_Reference_FileNamePath = "";
            //string Calibrate_Extract_Output_Reference_IQ_FileNamePath = "";

            if (!Directory.Exists(CalDIR)) Directory.CreateDirectory(CalDIR);

            // Save to file
            string filename_X = Calibrate_Extract_Output_Synced_Original_Reference_FileNamePath.Trim();
            File.WriteAllLines(filename_X, tmp_string5);

            string filename_X2 = Calibrate_Extract_Output_Reference_IQ_FileNamePath.Trim();
            File.WriteAllLines(filename_X2, tmp_string6);

        }

        private static double Angle_Cal(Double Real_Input, Double Imag_Input)
        {

            // Task : Calculate the phase base on the real and imaginary part
            Double Angle_Cal_Tmp = new Double();

            // Check the quadrant
            // Quadrant 1
            if (Real_Input >= 0 && Imag_Input >= 0)
            {
                if (Real_Input == 0)
                {
                    Angle_Cal_Tmp = Math.Atan(9E+300);
                }
                else
                {
                    Angle_Cal_Tmp = Math.Atan(Math.Abs(Imag_Input / Real_Input));
                }
            }

            // Quadrant 2
            else if (Real_Input <= 0 && Imag_Input >= 0)
            {
                if (Real_Input == 0)
                {
                    Angle_Cal_Tmp = pi - Math.Atan(9E+300);
                }
                else
                {
                    Angle_Cal_Tmp = pi - Math.Atan(Math.Abs(Imag_Input / Real_Input));
                }
            }

            // Quadrant 3
            else if (Real_Input <= 0 && Imag_Input <= 0)
            {
                if (Real_Input == 0)
                {
                    Angle_Cal_Tmp = pi + Math.Atan(9E+300);
                }
                else
                {
                    Angle_Cal_Tmp = pi + Math.Atan(Math.Abs(Imag_Input / Real_Input));
                }
            }

            // Quadrant 4
            else if (Real_Input >= 0 && Imag_Input <= 0)
            {
                if (Real_Input == 0)
                {
                    Angle_Cal_Tmp = (2 * pi) - Math.Atan(9E+300);
                }
                else
                {
                    Angle_Cal_Tmp = (2 * pi) - Math.Atan(Math.Abs(Imag_Input / Real_Input));
                }
            }

            else
            {
                // Error
                //Show_Error "Quadrant unknown", "sub Angle_Cal() clsVNA", Show_On_Screen
            }

            return (Angle_Cal_Tmp);
        }

        private static double RSquare_Value(double[] xVals, double[] yVals)
        {
            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (int i = 0; i < xVals.Length; i++)
            {
                double x = xVals[i];
                double y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            //
            double count = xVals.Length;
            //
            double rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            double dblR = rNumerator / Math.Sqrt(rDenom);
            //
            return (dblR * dblR * 100); // RSequare unit in Percentage
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
        public double Soak_Delay;
        public double DutyCycle = 0;
        public double Burstdelay = 0;
        public double pconMultiplier = 0.9;
        public string InputPort = "";
        public string OutputPort = "";
        public int TotalTraces = 10;


        public bool TestReadReg1C;

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

        public int IterationACP;
        public int IterationEVM;
        public double Harm2MeasBW;

        public bool VIO32MA = false;
        public bool VIORESET = false;

        public Dictionary<string, bool> TestNS = new Dictionary<string, bool>();
        public Dictionary<string, bool> TestCustom = new Dictionary<string, bool>();
        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();
        public Dictionary<string, TestLib.DPAT_Variable> TestDPAT = new Dictionary<string, TestLib.DPAT_Variable>();

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
        public niComplexNumber[,] iqTrace_Multiple; // [BurhanEVM - Add]
        public AclrResults ACLR;
        public double EVM = -999;

        public double Reg1C = 0;
        public double Register_x48 = 0;
        public double Register_InRevID = 0;

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
