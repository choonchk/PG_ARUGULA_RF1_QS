using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EqLib;
using GuCal;
using ClothoLibAlgo;
using IqWaveform;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using Ionic.Zip;
using System.Threading.Tasks;
using Avago.ATF.StandardLibrary;
using System.Diagnostics;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using ProductionLib;

namespace TestLib
{

    public class PinSweepTest : TimingBase, iTest
    {
        public PinSweepResults TestResult;
        public PinSweepTestCondition TestCon = new PinSweepTestCondition();
        public byte Site;
        public double inputPathGain_withIccCal = 0;
        public double outputPathGain_withIccCal = 0;
        //public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands; // PinSweepTestCondition already cintains the list. KH
        private AutoResetEvent dcArmedFlag = new AutoResetEvent(false);
        private Task CalcTask;
        public String PinSweepTraceEnable = "FALSE";
        public bool SkipOutputPortOnFail = false;
        public bool Mordor = false;
        public TriggerLine dcTrigLine;

        public static int dutSN = 0;
        public static int tracePIDCount = 0;
        public static int TraceFileNum = 0;
        public static int currentPIDint = 0;
        public HiPerfTimer uTimer = new HiPerfTimer();

        public bool Initialize(bool finalScript)
        {
            InitializeTiming(this.TestCon.TestParaName);

            string waveformName = "PINSWEEP";
            //Eq.Site[Site].HSDIO.AddVectorsToScript(TestCon.MipiCommands, finalScript);

            if (IQ.Mem.ContainsKey(waveformName))
            {
                TestCon.iqWaveform = IQ.Mem[waveformName];

                inputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);
                outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);

                double SAPeakLB = Math.Min(TestCon.PinSweepStop + TestCon.ExpectedGain + IQ.Mem["PINSWEEP"].PAR + outputPathGain_withIccCal, 30);
  

                if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                {
                    Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxIQ, new EqRF.Config(Eq.Site[Site].RF.CRfmxIQ.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, TestCon.PinSweepStop, inputPathGain_withIccCal, outputPathGain_withIccCal, Site));
                    Eq.Site[Site].RF.CRfmxIQ.SpecIteration();
                }
                return true;
            }



            return false;
        }

        public static void PreTest()
        {
            string SN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "");

            if (string.IsNullOrEmpty(SN))
            {
                dutSN = currentPIDint;
                dutSN++;   // Lite Driver
            }

            tracePIDCount++;
        }

        public int RunTest()
        {
            try
            {

                TestResult = new PinSweepResults();

                if (ResultBuilder.headerFileMode) return 0;

                SwBeginRun(Site);

                // Eq.Site[Site].RF.SA.Abort();

                this.ConfigureVoltageAndCurrent();
                SwStartRun("SendMipiCommands", Site);
                Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
                SwStopRun("SendMipiCommands", Site);

                SwStartRun("GetLossFactors", Site);
                GetLossFactors();
                SwStopRun("GetLossFactors", Site);

                //double SAPeakLB = Math.Min((Convert.ToDouble(TestCon.PinSweepStop) + IQ.Mem["PINSWEEP"].PAR) - outputPathGain_withIccCal - 3, 30 + outputPathGain_withIccCal - 0.001);
                double SAPeakLB = Math.Min((Convert.ToDouble(TestCon.PinSweepStop) + TestCon.ExpectedGain + IQ.Mem["PINSWEEP"].PAR) - outputPathGain_withIccCal - 3, 30 + outputPathGain_withIccCal - 0.001);

                int Get_Iteration = Eq.Site[Site].RF.CRfmxIQ.GetSpecIteration();

                // For Quadsite, single Switch Dio to support dual sites 	
                string BandTemp = TestCon.Band;
                byte SiteTemp = Site;
                if (Site.Equals(0) == false)
                {
                    BandTemp = BandTemp + "_" + Site.ToString();
                    SiteTemp = 0;
                }
                // Skip Output Port on Contact Failure (11-Nov-2018)
                bool condition1 = SkipOutputPortOnFail;
                condition1 = condition1 && Eq.Site[SiteTemp].SwMatrix.IsFailedDevicePort(BandTemp, TestCon.VsaOperation);
                condition1 = condition1 && !GU.runningGUIccCal[Site];
                if (condition1)
                {
                    foreach (string pinName in Eq.Site[Site].DC.Keys)
                    {
                        if (TestCon.DcSettings[pinName].Test)
                            TestResult.Itrace[pinName] = new double[500];
                    }
                    TestResult.IQtrace = new NationalInstruments.ModularInstruments.Interop.niComplexNumber[500];
                    SwStartRun("TaskRun-AnalyzePinSweep", Site);
                    CalcTask = Task.Run(() => TestResult.AnalyzePinSweep(TestCon));
                    SwStopRun("TaskRun-AnalyzePinSweep", Site);

                }
                else
                {
                    SwStartRun("TaskRun-SetupAndMeasureDc", Site);
                    Task taskSetupAndMeasureDc = Task.Run(() => SetupAndMeasureDc());
                    SwStopRun("TaskRun-SetupAndMeasureDc", Site);

                    SwStartRun("SetSwitchMatrixPaths", Site);
                    SetSwitchMatrixPaths();
                    Eq.Site[Site].RF.ServoEnabled = false;
                    SwStopRun("SetSwitchMatrixPaths", Site);
                    
                    if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
                    {
                        SwStartRun("RF-Configure_IQ", Site);
                        Eq.Site[Site].RF.Configure_IQ(new EqRF.Config_IQ(Eq.Site[Site].RF.CRfmxIQ.GetSpecIteration(), TestCon.FreqSG, SAPeakLB, TestCon.PinSweepStop, inputPathGain_withIccCal, outputPathGain_withIccCal));
                        SwStopRun("RF-Configure_IQ", Site);

                        SwStartRun("dcArmedFlag.WaitOne", Site);
                        dcArmedFlag.WaitOne();
                        SwStopRun("dcArmedFlag.WaitOne", Site);

                        SwStartRun("NIVST_Rfmx.WaitOne", Site);
                        Eq.Site[Site].RF.ThreadFlags[0].WaitOne();
                        Eq.Site[Site].RF.ThreadFlags[1].WaitOne();
                        SwStopRun("NIVST_Rfmx.WaitOne", Site);

                        SwStartRun("RF.Measure_IQ", Site);
                        Eq.Site[Site].RF.Measure_IQ();
                        SwStopRun("RF.Measure_IQ", Site);

                        SwStartRun("TaskRun-taskMeasureIQ.Wait", Site);
                        Eq.Site[Site].RF.CRfmxIQ.Wait();
                        SwStopRun("TaskRun-taskMeasureIQ.Wait", Site);

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
                                //MessageBox.Show("taskSetupAndMeasureDc.Wait(); in PinSweepTest.cs generated an error " + "\r\n" + e);
                                ATFLogControl.Instance.Log(LogLevel.Error, "taskSetupAndMeasureDc.Wait(); in PinSweepTest.cs generated an error " + TestCon.TestParaName);
                                ATFLogControl.Instance.Log(LogLevel.Error, TestCon.TestParaName);
                            } 
                        }
                        SwStopRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);

                        SwStartRun("SASG.Abort", Site);
                        Eq.Site[Site].RF.SA.Abort(Site);
                        Eq.Site[Site].RF.SG.Abort();
                        SwStopRun("SASG.Abort", Site);

                        SwStartRun("TaskRun-RetrieveDcMeasurement.Wait", Site);
                        RetrieveDcMeasurement();
                        SwStopRun("TaskRun-RetrieveDcMeasurement.Wait", Site);
                        TestCon.IterationSweep = Eq.Site[Site].RF.CRfmxIQ.GetSpecIteration();
                        CalcTask = Task.Run(() => CalcResults(TestCon.IterationSweep));

                    }
                    else
                    {
                        if (TestCon.resetSA)
                        {
                            SwStartRun("RF.ResetRFSA", Site);
                            Eq.Site[Site].RF.ResetRFSA(TestCon.resetSA);
                            SwStopRun("RF.ResetRFSA", Site);
                        }

                        Eq.Site[Site].RF.SetActiveWaveform("PINSWEEP", "", false);
                        Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;
                        Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
                        Eq.Site[Site].RF.SG.Level = TestCon.PinSweepStop - Eq.Site[Site].RF.ActiveWaveform.PAR;

                        Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
                        Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(TestCon.PinSweepStop + TestCon.ExpectedGain + Eq.Site[Site].RF.ActiveWaveform.PAR, 30 - outputPathGain_withIccCal - 0.001);
                        Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;


                        SwStartRun("RFSA-Initiate", Site);
                        Eq.Site[Site].RF.SA.Initiate();
                        SwStopRun("RFSA-Initiate", Site);

                        SwStartRun("dcArmedFlag.WaitOne", Site);
                        dcArmedFlag.WaitOne();
                        SwStopRun("dcArmedFlag.WaitOne", Site);

                        SwStartRun("RFSG-Initiate", Site);
                        Eq.Site[Site].RF.SG.Initiate();
                        SwStopRun("RFSG-Initiate", Site);

                        SwStartRun("SA.MeasureIqTrace", Site);
                        TestResult.IQtrace = Eq.Site[Site].RF.SA.MeasureIqTrace(true);
                        SwStopRun("SA.MeasureIqTrace", Site);

                        //keng shan
                        Eq.Site[Site].RF.SG.Level = -100;


                        if (PinSweepTraceEnable == "TRUE")
                        {
                            if ((tracePIDCount <= TraceFileNum) && (tracePIDCount != 0))
                            {
                                TestResult.AnalyzePinSweep_ShowPlot(TestCon, dutSN, false);//save Pinsweep trace plot
                            }
                        }

                        SwStartRun("RF.SASG.Abort", Site);
                        Eq.Site[Site].RF.SA.Abort(Site);
                        Eq.Site[Site].RF.SG.Abort();
                        SwStopRun("RF.SASG.Abort", Site);

                        SwStartRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);
                        taskSetupAndMeasureDc.Wait();
                        SwStopRun("TaskRun-taskSetupAndMeasureDc.Wait", Site);

                        SwStartRun("Task.Run-AnalyzePinSweep", Site);
                        CalcTask = Task.Run(() => TestResult.AnalyzePinSweep(TestCon));
                        SwStopRun("Task.Run-AnalyzePinSweep", Site);
                    }
                }

                Eq.Site[Site].RF.CRfmxIQ.SpecIteration();
                SwEndRun(Site);

                return 0;

            }
            catch (Exception e)
            {
                Eq.Site[Site].RF.CRfmxIQ.SpecIteration();
                MessageBox.Show("Error happened during RunTest in TestLib.cs" + "\r\n" + e.ToString());

                return 0;
            }


        }

        private void CalcResults( int IterationSweep)
        {
            try
            {
                SwStartRun("RfmxIQ.RetrieveResults", Site);
                TestResult.IQtrace = Eq.Site[Site].RF.CRfmxIQ.RetrieveResults(outputPathGain_withIccCal, IterationSweep);
                SwStopRun("RfmxIQ.RetrieveResults", Site);

                SwStartRun("Task.Run-AnalyzePinSweep", Site);
                TestResult.AnalyzePinSweep(TestCon);
                SwStopRun("Task.Run-AnalyzePinSweep", Site);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "CalcResults", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetSwitchMatrixPaths()
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

        private void GetLossFactors()
        {            
            inputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, "PinSweep_" + TestCon.TestParaName, GU.IccCalGain.InputGain);
            outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, "PinSweep_" + TestCon.TestParaName, GU.IccCalGain.OutputGain);
        }

        public void BuildResults(ref ATFReturnResult results)
        {   
            if (CalcTask != null)
                CalcTask.Wait();
            bool useCorrelationOffsetsForCalculatedValue = (ResultBuilder.corrFileExists & !GU.runningGU[Site]) | GU.GuMode[Site] == GU.GuModes.Vrfy;
            //bool useCorrelationOffsetsForCalculatedValue = false;
            bool TestIccSum = (TestResult.IatGainMax.Count(chan => chan.Key.ToUpper().Contains("VCC")) > 1 && TestCon.DcSettings.Count(chan => chan.Key.ToUpper().Contains("VCC") && chan.Value.Test == true) > 1 ? true : false);

            /* Removed temporarily due to number of parameter header //DH
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ResultBuilder.AddResult(Site, TestCon.CktID + "Pin_MaxGain_" + TestCon.TestParaName + "x", "dBm", TestResult.PinAtGainMax, 4);
            ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_MaxGain_" + TestCon.TestParaName + "x", "dBm", TestResult.PoutAtGainMax, 4);
            if(useCorrelationOffsetsForCalculatedValue)
            {
                double pinAtMaxGain_cal = TestResult.PinAtGainMax + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_MaxGain_" + TestCon.TestParaName + "x");
                double poutAtMaxGain_cal = TestResult.PoutAtGainMax + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_MaxGain_" + TestCon.TestParaName + "x");
                ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_MaxGain_" + TestCon.TestParaName + "x", "dB", (poutAtMaxGain_cal - pinAtMaxGain_cal), 4);
            }
            else
            {
                ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_MaxGain_" + TestCon.TestParaName + "x", "dB", TestResult.GainMax, 4);
            }
            double iccSumAtMaxGain_cal = 0;
            bool TestIccSum = (TestResult.IatGainMax.Count(chan => chan.Key.ToUpper().Contains("VCC")) > 1 && 
                TestCon.DcSettings.Count(chan => chan.Key.ToUpper().Contains("VCC") && chan.Value.Test == true) > 1 ?
                true : false);
            double iTotalAtMaxGain_cal = 0;
            foreach (string pinName in TestCon.DcSettings.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                    {
                        if (useCorrelationOffsetsForCalculatedValue)
                        {
                            iccSumAtMaxGain_cal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_MaxGain_" + TestCon.TestParaName + "x", TestResult.IatGainMax[pinName]);
                        }
                        else
                        {
                            iccSumAtMaxGain_cal += TestResult.IatGainMax[pinName];
                        }
                    }
                    else
                    {
                        if (useCorrelationOffsetsForCalculatedValue)
                        {
                            iTotalAtMaxGain_cal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_MaxGain_" + TestCon.TestParaName + "x", TestResult.IatGainMax[pinName]);
                        }
                        else
                        {
                            iTotalAtMaxGain_cal += TestResult.IatGainMax[pinName];
                        }
                    }

                    ResultBuilder.AddResult(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_MaxGain_" + TestCon.TestParaName + "x", "A", !ResultBuilder.headerFileMode ? TestResult.IatGainMax[pinName] : 0, 9);
                }
            }

            if (TestIccSum)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    iTotalAtMaxGain_cal += GU.getValueWithCF(Site, TestCon.CktID + "IccSum_MaxGain_" + TestCon.TestParaName + "x", iccSumAtMaxGain_cal);
                }
                else
                {
                    iTotalAtMaxGain_cal += iccSumAtMaxGain_cal;
                }
                ResultBuilder.AddResult(Site, TestCon.CktID + "IccSum_MaxGain_" + TestCon.TestParaName + "x", "A", iccSumAtMaxGain_cal, 9);
            }

            if (TestCon.TestItotal)
            {
                if (useCorrelationOffsetsForCalculatedValue)
                {
                    ResultBuilder.AddResult(Site, TestCon.CktID + "Itotal_MaxGain_" + TestCon.TestParaName + "x", "A", iTotalAtMaxGain_cal, 9);
                }
                else
                {
                    ResultBuilder.AddResult(Site, TestCon.CktID + "Itotal_MaxGain_" + TestCon.TestParaName + "x", "A", TestResult.IatGainMax.Values.Sum(), 9);
                }
            }

            #region calculate PAE at Max Gain, with correlation offsets
            if (useCorrelationOffsetsForCalculatedValue)
            {
                if (GU.getGUcalfactor(Site, TestCon.CktID + "PAE_MaxGain_" + TestCon.TestParaName + "x") != 0)
                {
                    MessageBox.Show("Must set PAE Correlation Factor to 0\nfor test " + "PAE_MaxGain_" + TestCon.TestParaName + "x", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    GU.forceReload = true;    // extra nag to force reload
                }

                double pinAtMaxGain_cal = TestResult.PinAtGainMax + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_MaxGain_" + TestCon.TestParaName + "x");
                double poutAtMaxGain_cal = TestResult.PoutAtGainMax + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_MaxGain_" + TestCon.TestParaName + "x");

                float dcPowerAtGainMax_cal = 0;
                string vccName = "";
                foreach (string pinName in TestResult.Itrace.Keys)
                {
                    if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                    {
                        vccName = pinName;
                        continue;
                    }

                    double currentAtGainMax_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_MaxGain_" + TestCon.TestParaName + "x", TestResult.IatGainMax[pinName]);
                    dcPowerAtGainMax_cal += (float)(TestCon.DcSettings[pinName].Volts * currentAtGainMax_cal);
                }
                if (TestIccSum)
                {
                    dcPowerAtGainMax_cal += (float)(TestCon.DcSettings[vccName].Volts * GU.getValueWithCF(Site, TestCon.CktID + "IccSum_MaxGain_" + TestCon.TestParaName + "x", iccSumAtMaxGain_cal));
                }

                TestResult.PaeAtGainMax = Convert.ToSingle((Math.Pow(10.0, poutAtMaxGain_cal / 10.0) - Math.Pow(10.0, pinAtMaxGain_cal / 10.0)) / dcPowerAtGainMax_cal * 100.0 / 1000.0);
            }
            #endregion

            if (Math.Abs(TestResult.PaeAtGainMax) > 1000 || double.IsNaN(TestResult.PaeAtGainMax)) TestResult.PaeAtGainMax = -1;
            if (TestCon.TestPae)
                ResultBuilder.AddResult(Site, TestCon.CktID + "PAE_MaxGain_" + TestCon.TestParaName + "x", "%", TestResult.PaeAtGainMax, 4);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            */
            foreach (double compLev in TestCon.TestCompression.Keys)
            {
                double iccSumAtComp_cal = 0;
                double iTotalAtComp_cal = 0;

                if (TestCon.TestCompression[compLev])
                {
                    if ((TestResult.GainMax - TestResult.GainTrace[TestResult.GainTrace.Length - 1] < 1) && (TestCon.PowerMode == "G6")) //only for G6 mode
                    {
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName , "dBm", !ResultBuilder.headerFileMode ? TestResult.PinTrace[TestResult.PinTrace.Length - 1] : 0, 4);
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName , "dBm", !ResultBuilder.headerFileMode ? TestResult.PoutTrace[TestResult.PoutTrace.Length - 1] : 0, 4);

                        if (useCorrelationOffsetsForCalculatedValue)
                        {
                            double pinAtComp_cal = TestResult.PinTrace[TestResult.PinTrace.Length - 1] + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName );
                            double poutAtComp_cal = TestResult.PoutTrace[TestResult.PoutTrace.Length - 1] + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName );
                            ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_P" + compLev + "dB_" + TestCon.TestParaName , "dB", !ResultBuilder.headerFileMode ? (poutAtComp_cal - pinAtComp_cal) : 0, 4);
                        }
                        else
                        {
                            ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_P" + compLev + "dB_" + TestCon.TestParaName , "dB", !ResultBuilder.headerFileMode ? TestResult.GainTrace[TestResult.GainTrace.Length - 1] : 0, 4);
                        }

                    }
                    else
                    {
                        //ResultBuilder.AddResult(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"], "dBm", !ResultBuilder.headerFileMode ? TestResult.Comp.Pin[compLev] : 0, 4);
                        //ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"], "dBm", !ResultBuilder.headerFileMode ? TestResult.Comp.Pout[compLev] : 0, 4);
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName , "dBm", !ResultBuilder.headerFileMode ? TestResult.Comp.Pin[compLev] : 0, 4);
                        ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName , "dBm", !ResultBuilder.headerFileMode ? TestResult.Comp.Pout[compLev] : 0, 4);

                        if (useCorrelationOffsetsForCalculatedValue)
                        {
                            double pinAtComp_cal = TestResult.Comp.Pin[compLev] + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName );
                            double poutAtComp_cal = TestResult.Comp.Pout[compLev] + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName );
                            ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_P" + compLev + "dB_" + TestCon.TestParaName , "dB", !ResultBuilder.headerFileMode ? (poutAtComp_cal - pinAtComp_cal) : 0, 4);
                        }
                        else
                        {
                            //ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_P" + compLev + "dB_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Gain"], "dB", !ResultBuilder.headerFileMode ? TestResult.Comp.Gain[compLev] : 0, 4);
                            ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_P" + compLev + "dB_" + TestCon.TestParaName , "dB", !ResultBuilder.headerFileMode ? TestResult.Comp.Gain[compLev] : 0, 4);
                        }

                        foreach (string pinName in TestCon.DcSettings.Keys)
                        {
                            if (TestCon.DcSettings[pinName].Test)
                            {
                                if (TestIccSum && pinName.ToUpper().Contains("VCC") == true)
                                {
                                    if (useCorrelationOffsetsForCalculatedValue)
                                    {
                                        iccSumAtComp_cal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_P" + compLev + "dB_" + TestCon.TestParaName, TestResult.Comp.I[compLev, pinName]);
                                    }
                                    else
                                    {
                                        iccSumAtComp_cal += TestResult.Comp.I[compLev, pinName];
                                    }
                                }
                                else
                                {
                                    if (useCorrelationOffsetsForCalculatedValue)
                                    {
                                        iTotalAtComp_cal += GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_P" + compLev + "dB_" + TestCon.TestParaName, TestResult.Comp.I[compLev, pinName]);
                                    }
                                    else
                                    {
                                        iTotalAtComp_cal += TestResult.Comp.I[compLev, pinName];
                                    }
                                }

                                ResultBuilder.AddResult(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_P" + compLev + "dB_" + TestCon.TestParaName, "A", TestResult.Comp.I[compLev, pinName], 9);  //hosein 05042020  n.TestParaName + "x"
                            }
                        }

                        if (TestIccSum)
                        {
                            if (useCorrelationOffsetsForCalculatedValue)
                            {
                                iTotalAtComp_cal += GU.getValueWithCF(Site, TestCon.CktID + "IccSum" + "_P" + compLev + "dB_" + TestCon.TestParaName, iccSumAtComp_cal);
                            }
                            else
                            {
                                iTotalAtComp_cal += iccSumAtComp_cal;
                            }
                            ResultBuilder.AddResult(Site, TestCon.CktID + "IccSum" + "_P" + compLev + "dB_" + TestCon.TestParaName , "A", iccSumAtComp_cal, 9);  //hosein 05042020
                            //ResultBuilder.AddResult(Site, TestCon.CktID + "IccSum" + "_P" + compLev + "dB_" + TestCon.TestParaName + "x", "A", iccSumAtComp_cal, 9);
                        }
                        if (TestCon.TestItotal)
                        {
                            if (useCorrelationOffsetsForCalculatedValue)
                            {
                                ResultBuilder.AddResult(Site, TestCon.CktID + "Itotal_P" + compLev + "dB_" + TestCon.TestParaName + "x", "A", !ResultBuilder.headerFileMode ? iTotalAtComp_cal : 0, 9);
                            }
                            else
                            {
                                ResultBuilder.AddResult(Site, TestCon.CktID + "Itotal_P" + compLev + "dB_" + TestCon.TestParaName + "x", "A", !ResultBuilder.headerFileMode ? TestResult.Comp.I[compLev].Values.Sum() : 0, 9); // .Average()); replaced with .Sum to get Itotal
                            }
                        }

                        #region calculate PAE at compression point, with correlation offsets
                        if (useCorrelationOffsetsForCalculatedValue)
                        {
                            if (TestCon.TestPae)
                            {
                                //if (GU.getGUcalfactor(Site, TestCon.CktID + "PAE_P" + compLev + "dB_" + TestCon.TestParaName + "x") != 0)
                                //{
                                //    MessageBox.Show("Must set PAE Correlation Factor to 0\nfor test " + "PAE_P" + compLev + "dB_" + TestCon.TestParaName + "x", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //    GU.forceReload = true;    // extra nag to force reload
                                //}

                                //double pinAtComp_cal = TestResult.Comp.Pin[compLev] + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pin"]);
                                //double poutAtComp_cal = TestResult.Comp.Pout[compLev] + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName + TestCon.SpecNumber["Para.Pout"]);

                                //ChoonChin 20200730 - fix pae corr issue
                                double pinAtComp_cal = TestResult.Comp.Pin[compLev] + GU.getGUcalfactor(Site, TestCon.CktID + "Pin_P" + compLev + "dB_" + TestCon.TestParaName);
                                double poutAtComp_cal = TestResult.Comp.Pout[compLev] + GU.getGUcalfactor(Site, TestCon.CktID + "Pout_P" + compLev + "dB_" + TestCon.TestParaName);

                                float dcPowerAtComp_cal = 0;
                                string vccName = "";
                                foreach (string pinName in TestResult.Itrace.Keys)
                                {
                                    if (TestIccSum && pinName.ToUpper().Contains("VCC"))
                                    {
                                        vccName = pinName;
                                        continue;
                                    }

                                    double currentAtComp_cal = GU.getValueWithCF(Site, TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_P" + compLev + "dB_" + TestCon.TestParaName, TestResult.Comp.I[compLev, pinName]);

                                    //string ParaName = TestCon.CktID + TestCon.DcSettings[pinName].iParaName + "_P" + compLev + "dB_" + TestCon.TestParaName + "x";

                                    dcPowerAtComp_cal += (float)(TestCon.DcSettings[pinName].Volts * currentAtComp_cal);
                                }
                                if (TestIccSum)
                                {
                                    dcPowerAtComp_cal += (float)(TestCon.DcSettings[vccName].Volts * GU.getValueWithCF(Site, TestCon.CktID + "IccSum" + "_P" + compLev + "dB_" + TestCon.TestParaName, iccSumAtComp_cal));
                                }
                                TestResult.Comp.PAE[compLev] = Convert.ToSingle((Math.Pow(10.0, poutAtComp_cal / 10.0) - Math.Pow(10.0, pinAtComp_cal / 10.0)) / dcPowerAtComp_cal * 100.0 / 1000.0);
                            }
                        }
                        #endregion

                        if (!ResultBuilder.headerFileMode)
                            if (Math.Abs(TestResult.Comp.PAE[compLev]) > 1000 || double.IsNaN(TestResult.Comp.PAE[compLev])) TestResult.Comp.PAE[compLev] = -1;
                        if (TestCon.TestPae)
                            ResultBuilder.AddResult(Site, TestCon.CktID + "PAE_P" + compLev + "dB_" + TestCon.TestParaName + "x", "A", !ResultBuilder.headerFileMode ? TestResult.Comp.PAE[compLev] : 0, 4);

                    }

                }
                
            }

            #region Save Trace File

            if (PinSweepTraceEnable == "TRUE")
            {
                string ResultFileName = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, "");
                if (ResultFileName == "") ResultFileName = "Debug";
                string path = @"C:\Avago.ATF.Common\DataLog\PoutDropPlots\" + ResultFileName + "\\Unit_" + GetCurrentPID().ToString();
                string FileName = TestCon.TestParaName + "_Plot.csv";
                string Header = string.Format("PoutDrop_{0:yyyy-MM-dd_HH:mm:ss}_{1}", DateTime.Now, TestCon.TestParaName);
                DirectoryInfo GeneratedFolder = new DirectoryInfo(path);
                GeneratedFolder.Create();
                FileInfo fi = new FileInfo(path + "\\" + FileName);
                StreamWriter writeMBdata = fi.CreateText();
                int count = TestResult.PoutTrace.Count();
                writeMBdata.Write(Header + Environment.NewLine);

                writeMBdata.Write("No.,Pin,Pout,Gain,");
                foreach (string iChan in TestResult.Itrace.Keys)
                {
                    writeMBdata.Write(iChan.ToString().Replace('V', 'I') + ",");
                }
                writeMBdata.Write(Environment.NewLine);

                for (int i = 0; i < count; i++)
                {
                    writeMBdata.Write(i.ToString() + "," + TestResult.PinTrace[i] + "," + TestResult.PoutTrace[i] + "," + TestResult.GainTrace[i] + ",");

                    foreach (string iChan in TestResult.Itrace.Keys)
                    {
                        double iCurrent = (double)i / (double)count * (double)TestResult.Itrace[iChan].Length;
                        writeMBdata.Write(Calc.InterpLinear(TestResult.Itrace[iChan], iCurrent) + ",");
                    }
                    writeMBdata.Write(Environment.NewLine);
                }
                writeMBdata.Close();
            }
            #endregion

            //keng shan Added
            if (TestCon.TestPoutDrop)
            {
                #region Gain Linearity Variable

                int Pin_refIndex = 0;
                int refPin = -15;

                double refGaindrop = 0;
                double PoutTolerance = 0.08;
                double Gain10toMaxLimit = 2.0;
                double GainMintoMaxLimit = 3.0;
                double MinGain10toMax = 0;
                int GainDelta10toMax = 0;
                int GainPout10Index = 0;
                double GainAt10dBm;
                int IndexMaxgain, isGainDropFail;
                float MaxGainPin, MaxGainPout;
                double GainDeltainPinSweep, GainDeltainPinSweep2;
                #endregion

                double PowerMax = TestResult.PoutTrace.Max();
                double EndofPower = TestResult.PoutTrace[TestResult.PoutTrace.Length - 1];
                double EndofGain = TestResult.GainTrace[TestResult.GainTrace.Length - 1];



                #region Extract Gain at 10dBm and Index, Pin reference Index
                double[] gainArray = new double[TestResult.PoutTrace.Length];

                for (int i = 0; i < 11; i++)
                {
                    TestResult.PoutTrace[i] = -999;
                }

                for (int i = 0; i < TestResult.PoutTrace.Length; i++)
                {
                    gainArray[i] = TestResult.PoutTrace[i] - TestResult.PinTrace[i];
                   // TestResult.GainTrace[i] = TestResult.PoutTrace[i] - TestResult.PinTrace[i];
                    if (Math.Abs(TestResult.PoutTrace[i] - 10) < PoutTolerance)
                    {
                        GainAt10dBm = refGaindrop = TestResult.GainTrace[i];
                        GainPout10Index = i;
                    }

                    if (Math.Abs(TestResult.PinTrace[i] - refPin) < 0.005) Pin_refIndex = i;
                }
                #endregion

                #region Calculate Gain Linearity

                double maxGain = gainArray.Max();

                double tempMaxGain = 0;
                int maxGainIndex = IndexMaxgain = gainArray.ToList().IndexOf(maxGain);

                MaxGainPin = Convert.ToSingle(TestResult.PinTrace[maxGainIndex]);
                MaxGainPout = Convert.ToSingle(TestResult.PoutTrace[maxGainIndex]);

                if (MaxGainPin < refPin)
                {
                    for (int k = Pin_refIndex; k < TestResult.PoutTrace.Length; k++)
                    {
                        if (TestResult.GainTrace[k] > tempMaxGain)
                        {
                            maxGain = tempMaxGain = TestResult.GainTrace[k];
                            maxGainIndex = IndexMaxgain = k;
                        }
                    }

                    MaxGainPin = Convert.ToSingle(TestResult.PinTrace[maxGainIndex]);
                    MaxGainPout = Convert.ToSingle(TestResult.PoutTrace[maxGainIndex]);
                }

                GainDeltainPinSweep = maxGain - refGaindrop;

             //   if (maxGainIndex < GainPout10Index) GainDelta10toMax += 1;
             //   else
             //   {
                for (int j = GainPout10Index; j <= GainPout10Index + 1; j++)
                    {
                        if (j == GainPout10Index) MinGain10toMax = TestResult.GainTrace[j];
                        //else if (MinGain10toMax > TestResult.GainTrace[j]) MinGain10toMax = MinGain10toMax = TestResult.GainTrace[j];
                    }
              //  }
                GainDeltainPinSweep2 = maxGain - MinGain10toMax;

                if (GainDeltainPinSweep > Gain10toMaxLimit) GainDelta10toMax += 2;
                if (GainDeltainPinSweep2 > GainMintoMaxLimit) GainDelta10toMax += 4;

                isGainDropFail = GainDelta10toMax;

                #endregion

                #region Save Trace File

                //if (EnableGainDropPlot && TempTestCount != 0)
                //{
                //    string path = @"C:\Avago.ATF.Common\DataLog\GainLinearityPlots\" + DateAndTime + @"\Unit_" + TempTestCount.ToString();
                //    string FileName = TestParaName + "_Plot.csv";
                //    string Header = string.Format("GainLin_{0:yyyy-MM-dd_HH:mm:ss}_{1}", DateTime.Now, TestParaName);
                //    DirectoryInfo GeneratedFolder = new DirectoryInfo(path);
                //    GeneratedFolder.Create();
                //    FileInfo fi = new FileInfo(path + "\\" + FileName);
                //    StreamWriter writeMBdata = fi.CreateText();
                //    int count = TestResult.PoutTrace.Count();
                //    writeMBdata.Write(Header + Environment.NewLine);
                //    writeMBdata.Write("Index of Pout@10,Index of Pin@-15,Index of MaxGain" + Environment.NewLine);
                //    writeMBdata.Write(GainPout10Index.ToString() + "," + Pin_refIndex.ToString() + "," + maxGainIndex.ToString() + Environment.NewLine);
                //    writeMBdata.Write(Environment.NewLine);
                //    writeMBdata.Write("No.,Pin,Pout");
                //    writeMBdata.Write(Environment.NewLine);
                //    for (int i = 0; i < count; i++)
                //    {
                //        writeMBdata.Write(i.ToString() + "," + TestResult.PinTrace[i] + "," + TestResult.PoutTrace[i] + Environment.NewLine);
                //    }
                //    writeMBdata.Close();
                //}
                #endregion

                ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_atPin" + TestCon.PinSweepStop + "dBm_" + TestCon.TestParaName + "x", "dB", EndofPower);
                ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_atPin" + TestCon.PinSweepStop + "dBm_" + TestCon.TestParaName + "x", "dB", EndofGain);

                if (TestCon.DcSettings["Vcc"].Test)
                {
                    double[] EndofIccArray = TestResult.Itrace["Vcc"];
                    double EndofIcc = EndofIccArray[EndofIccArray.Count() - 1];
                    ResultBuilder.AddResult(Site, TestCon.CktID + "Icc_atPin" + TestCon.PinSweepStop + "dBm_" + TestCon.TestParaName + "x", "dB", EndofIcc);
                }

                ResultBuilder.AddResult(Site, TestCon.CktID + "PowerDropDelta_x_" + TestCon.TestParaName + "x", "dB", PowerMax - EndofPower);
                ResultBuilder.AddResult(Site, TestCon.CktID + "GainLinearity_x_" + TestCon.TestParaName + "x", "dB", GainDeltainPinSweep2, 4);
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

        private int GetCurrentPID()
        {
            string currentPIDstr = "";
            currentPIDint = 0;

            currentPIDstr = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_SN, "");
            if (!string.IsNullOrEmpty(currentPIDstr))
                int.TryParse(currentPIDstr, out currentPIDint);
            else
                currentPIDint = dutSN;   // Lite Driver

            if ((ResultBuilder.ValidSites.Count > 1) && (Site != 0 ))
            {
                currentPIDint = currentPIDint + Site;
            }

            return currentPIDint;
        }

        private void ConfigureVoltageAndCurrent()
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
        }

        public void SetupAndMeasureDc()
        {
            SetupDcMeasurement();

            dcArmedFlag.Set();
        }

        public void SetupDcMeasurement()
        {
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    string msg = String.Format("SetupCurrentTraceMeasurement on pin {0}", pinName);
                    SwStartRunThread(msg);
                    Eq.Site[Site].DC[pinName].SetupCurrentTraceMeasurement(TestCon.iqWaveform.FinalServoMeasTime, 5e-6, dcTrigLine);
                    SwStopRunThread(msg);
                }
            }
        }

        public void RetrieveDcMeasurement()
        {
            foreach (string pinName in Eq.Site[Site].DC.Keys)
            {
                if (TestCon.DcSettings[pinName].Test)
                {
                    string msg = String.Format("MeasureCurrentTrace on pin {0}", pinName);
                    SwStartRunThread(msg);
                    TestResult.Itrace[pinName] = Eq.Site[Site].DC[pinName].MeasureCurrentTrace();
                    SwStopRunThread(msg);
                }
            }
        }

        public bool TryGettingCalFactors()
        {
            //bool success = true;    this code will throw exception if no ANT2 defined. copied TryGettingCalFactors from other RF tests

            //if (ResultBuilder.headerFileMode) return true;

            //success &= 0 != CableCal.GetCF(Site, TestCon.CktID + TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);

            //success &= 0 != CableCal.GetCF(Site, TestCon.CktID + TestCon.Band, Operation.VSAtoANT1, TestCon.FreqSG);
            //success &= 0 != CableCal.GetCF(Site, TestCon.CktID + TestCon.Band, Operation.VSAtoANT2, TestCon.FreqSG);

            //return (success);
            bool success = true;

            if (ResultBuilder.headerFileMode) return true;

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG);

            success &= 0 != CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG);

            //foreach (string customTestName in CustomPowerMeasurement.Mem.Keys)
            //{
            //    if (TestCon.TestCustom[customTestName])
            //    {
            //        success &= 0 != CableCal.GetCF(Site, TestCon.CktID + CustomPowerMeasurement.Mem[customTestName].isBandSpecific ? CustomPowerMeasurement.Mem[customTestName].band : TestCon.Band, CustomPowerMeasurement.Mem[customTestName].measurePath, TestCon.FreqSG * (double)CustomPowerMeasurement.Mem[customTestName].channelBandwidth);
            //    }
            //}

            return (success);   // if this method took more than 100ms to complete, that means messageBox was shown because CF was not loaded correctly
        }
    }

    public class PinSweepTestCondition : PinSweepTestConditionBase
    {
        public string TestParaName;
        public string PowerMode;
        public string Band;
        public string ModulationStd;
        public string WaveformName;
        public string ParameterNote;
        public Operation VsaOperation;
        public Operation VsgOperation;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public bool TestItotal;
        public string CktID;
        public string CplpreFix;
        public double FreqSG = 0;
        public bool resetSA;
        public bool TestPout;
        public bool TestPin;
        public bool TestGain;

        public bool TestPae;
        public int IterationSweep;
        //keng shan Added
        public bool TestPoutDrop;
        public bool TestGainLinearity;
        public static int Iteration;
        public float ExpectedGain = 0;
        public bool VIO32MA = false;
        public bool VIORESET = false;

        public Dictionary<double, bool> TestCompression = new Dictionary<double, bool>();
        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();
        public Dictionary<string, TestLib.DPAT_Variable> TestDPAT = new Dictionary<string, TestLib.DPAT_Variable>();

        public PinSweepTestCondition()
        {
            for (double comp = PinSweepTestConditionBase.CompCalcStep; comp <= PinSweepTestConditionBase.CompCalcMax; comp += PinSweepTestConditionBase.CompCalcStep)
            {
                this.AllCompLevs.Add(comp);
                this.TestCompression.Add(comp, false);
            }
        }
    }

    public class PinSweepResults : PinSweepResultsBase
    {
        public void AnalyzePinSweep_ShowPlot(PinSweepTestCondition TestCon, int dutSN, bool createDebugZipFile)
        {
            AnalyzePinSweep(TestCon);

            string title = "Pin Sweep,  " + TestCon.TestParaName + "\n" + "MaxGain search begins at " + TestCon.PinSweepStart + "dBm";

            Dictionary<string, double[]> xPoints = new Dictionary<string, double[]>();
            Dictionary<string, double[]> yPoints = new Dictionary<string, double[]>();
            Dictionary<string, double[]> zPoints = new Dictionary<string, double[]>();
            Dictionary<string, SeriesChartType> chartTypes = new Dictionary<string, SeriesChartType>();
            Dictionary<string, int> markerSizes = new Dictionary<string, int>();
            Dictionary<string, bool> showDataLabels = new Dictionary<string, bool>();

            xPoints.Add("Gain", PinTrace);
            yPoints.Add("Gain", GainTrace);
            zPoints.Add("Gain", PoutTrace);
            chartTypes.Add("Gain", SeriesChartType.Point);
            markerSizes.Add("Gain", 2);
            showDataLabels.Add("Gain", false);

            xPoints.Add("MaxGain", new double[] { PinAtGainMax });
            yPoints.Add("MaxGain", new double[] { GainMax });
            chartTypes.Add("MaxGain", SeriesChartType.Point);
            markerSizes.Add("MaxGain", 12);
            showDataLabels.Add("MaxGain", true);

            foreach (double compLev in TestCon.TestCompression.Keys)
            {
                if (TestCon.TestCompression[compLev])
                {
                    string seriesName = "P" + compLev + "dB";
                    xPoints.Add(seriesName, new double[] { Comp.Pin[compLev] });
                    yPoints.Add(seriesName, new double[] { Comp.Gain[compLev] });
                    zPoints.Add(seriesName, new double[] { Comp.Pout[compLev] });
                    chartTypes.Add(seriesName, SeriesChartType.Point);
                    markerSizes.Add(seriesName, 12);
                    showDataLabels.Add(seriesName, true);
                }
            }

            double yMin = Math.Floor(yPoints["Gain"].Skip(yPoints["Gain"].Length / 100).Min());
            double yMax = Math.Ceiling(yPoints["Gain"].Skip(yPoints["Gain"].Length / 100).Max());

            string dateCode = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", DateTime.Now);
            string zipFileDir = @"C:\Avago.ATF.Common.x64\RF_Data\PinSweep\";
            string zipFileName = TestCon.TestParaName + "_SN" + dutSN + "_" + dateCode;
            string plotFullPath = zipFileDir + zipFileName + ".jpg";
            string iqFullPath = zipFileDir + zipFileName + ".csv";
            string InputOutputPath = zipFileDir + zipFileName + ".csv";
            string CurrentTracePath = zipFileDir + "I_" + zipFileName + ".csv";

            if (!Directory.Exists(zipFileDir)) Directory.CreateDirectory(zipFileDir);

            try
            {
                string pinName = "Vcc";
                if (TestCon.CktID.Equals("PR_")) { pinName = "Vlna"; }

                if (TestCon.DcSettings[pinName].Test)
                {
                    // Add Current Trace Log
                    using (StreamWriter debugfile = new StreamWriter(new FileStream(CurrentTracePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                    {
                        debugfile.WriteLine(pinName);
                        for (int i = 0; i < Itrace[pinName].Length; i++)
                        {
                            debugfile.WriteLine(Itrace[pinName][i]);
                        }
                    }
                }

                // Add Simple Pinsweep log file for Input vs Pout 
                using (StreamWriter debugfile = new StreamWriter(new FileStream(InputOutputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    debugfile.WriteLine("Pin,Pout");

                    for (int i = 0; i < xPoints["Gain"].Length; i++)
                    {
                        if (xPoints["Gain"][i] >= TestCon.PinSweepStart)
                        {
                            debugfile.WriteLine(xPoints["Gain"][i] + "," + zPoints["Gain"][i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("PinSweepTest Trace logging error: {0}", e.ToString()));
            }

            if (createDebugZipFile)
            {
                Calc.Charts.CreateChartPinSweep(plotFullPath, title, xPoints, yPoints, "Pin (dBm)", "Gain (dB)", 1, yMin, yMax, chartTypes, markerSizes, showDataLabels, false, false, createDebugZipFile);

                Calc.WriteIqDebugFile(iqFullPath, ref IQtrace);
                using (ZipFile zip = new ZipFile(zipFileDir + zipFileName + ".zip"))
                {
                    zip.AddFiles(new List<string>() { plotFullPath, iqFullPath }, false, "");
                    zip.Save();
                }
                File.Delete(plotFullPath);
                File.Delete(iqFullPath);
                //MessageBox.Show("Pin Sweep debug zip file saved to:\n" + zipFileDir + zipFileName + ".zip");
            }
        }
    }

}
