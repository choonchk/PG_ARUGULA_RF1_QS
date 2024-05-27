using Avago.ATF.StandardLibrary;
using GuCal;
using EqLib;
using ClothoLibAlgo;
using NationalInstruments.ModularInstruments.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Data;

namespace TestLib
{
    public class RfOnOffTest : TimingBase, iTest
    {
        public byte Site;
        public RfOnOffTestCondition TestCon = new RfOnOffTestCondition();
        public RfOnOffTestResult TestResult;
        public const bool turboServo = false;
        public bool SkipOutputPortOnFail = false;
        public bool Mordor = false;
        public double inputPathGain_withIccCal;
        public double outputPathGain_withIccCal;
        public static double SWtimePreviousPin = -20;
        public static double SWtimePreviousPout = 20;
        public static double SWtimePreviousTPout = 0;
        public static string RFOnOffTraceEnable = "FALSE";
        public static int TraceFileNum = 0;

        private bool blnCpl = false;
        private string folderpath = @"C:\Avago.ATF.Common.x64\RF_Data\";
        //private bool blnRiseTime = false;

        public static int dutSN = 0;
        public static int tracePIDCount = 0;
        public static int currentPIDint = 0;

        public bool Initialize(bool finalScript)
        {
            InitializeTiming2(this.TestCon.TestParaName);
            SwStartRun("RfOnOffTest-Initialize", Site);
            //keng shan added
            string[] StrTemp = new string[2];


            Eq.Site[Site].HSDIO.SetSourceWaveformArry(TestCon.SWTIMECUSTOM);

            blnCpl = (TestCon.Cpl.ToUpper().Contains("FWD") | TestCon.Cpl.ToUpper().Contains("REV")) ? true : false;
            TestCon.RFOnTimeAvgPoint = 5;
            SwStopRun("RfOnOffTest-Initialize", Site);

            return true;

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

        public static void GetSN(string SN)
        {
            dutSN = Convert.ToInt32(SN);
            //RON: To do- handle multisite
            //if (ResultBuilder.SitesAndPhases[0] == 1 && ResultBuilder.SitesAndPhases[1] != 0) dutSN += 2;
            //if (ResultBuilder.SitesAndPhases[1] == 1) dutSN += 2;
        }

        public int RunTest()
        {
            SwBeginRun(Site);

            string[] SWTimeArr = TestCon.SWTIMECUSTOM.Split(':');
            if (TestCon.resetSA)
            {
                SwStartRun("ResetRFSA", Site);
                Eq.Site[Site].RF.ResetRFSA(TestCon.resetSA);
                SwStopRun("ResetRFSA", Site);
            }

            SwStartRun("SASGAbort1", Site);
            Eq.Site[Site].RF.SA.Abort(Site);
            Eq.Site[Site].RF.SG.Abort();
            SwStopRun("SASGAbort1", Site);

            if (TestCon.UsePreviousPin)
            {
                TestCon.TargetPin = (float)SWtimePreviousPin;
                TestCon.TargetPout = (float)SWtimePreviousPout;
                TestCon.TestParaName = TestCon.TestParaName.Replace("_0dBm", "_" + SWtimePreviousTPout + "dBm");
            }
            TestResult = new RfOnOffTestResult();

            if (ResultBuilder.headerFileMode) return 0;

            this.ConfigureVoltageAndCurrent();
            SwStartRun("SendMipiCommands", Site);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands", Site);
            SwStartRun("SetSwitchMatrixPaths", Site);
            SetSwitchMatrixPaths();
            SwStopRun("SetSwitchMatrixPaths", Site);
            SwStartRun("SetActiveWaveform", Site);
            Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, false);
            SwStopRun("SetActiveWaveform", Site);

            SwStartRun("GetLossFactors", Site);
            GetLossFactors();
            SwStopRun("GetLossFactors", Site);

            try
            {
                if (!this.TestCon.TXRX.Contains("RX") & !TestCon.UsePreviousPin)
                    PoutServo();
                else
                    FixedPinSet();  //need to write this funtion so that we can drive power into antenna port for RX test  KH  22 Nov 2017**************************************************
            }
            catch
            {
                Eq.Site[Site].RF.SG.Level = -50;
                Eq.Site[Site].RF.SA.Abort(Site);
                Eq.Site[Site].RF.SG.Abort();
                Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig0;
            }
            SwStartRun("Configure_RFonoff_SA", Site);
            Configure_RFonoff_SA();
            SwStopRun("Configure_RFonoff_SA", Site);

            SwStartRun("HSDIO.SendRFOnOffTestVector", Site);
            Eq.Site[Site].HSDIO.SendRFOnOffTestVector(false, SWTimeArr);
            SwStopRun("HSDIO.SendRFOnOffTestVector", Site);

            RunSwTest(ref TestResult.RfSettling, ref TestResult.rfOnOffTrace);

            if (TestCon.TestSw)
            {
                RunSwTest(ref TestResult.SwSettling, ref TestResult.swOnOffTrace);
            }

            if (SWTimeArr.Length > 2)
            {
                RunSwTest(ref TestResult.Sw2Settling, ref TestResult.sw2OnOffTrace);
            }

            SwStartRun("SASGAbort2", Site);

            Eq.Site[Site].RF.SG.Level = -50;
            Eq.Site[Site].RF.SA.Abort(Site);
            Eq.Site[Site].RF.SG.Abort();
            Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig0;
            SwStopRun("SASGAbort2", Site);
            try
            {
                Eq.Site[Site].HSDIO.TriggerOut = TriggerLine.None;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SwEndRun(Site);

            return 0;
        }

        //keng shan added 
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

        private void PoutServo()
        {
            SwStartRun("PoutServo-ConfigureSASG", Site);

            double sgLevelFromIccCal = GU.IccServoVSGlevel[Site, TestCon.TestParaName + "_IccCal"];

            Eq.Site[Site].RF.ServoEnabled = true;

            if (sgLevelFromIccCal == 0)
            {
                Eq.Site[Site].RF.SG.Level = TestCon.TargetPout - TestCon.ExpectedGain;// +4.0;
            }
            else
            {
                Eq.Site[Site].RF.SG.Level = sgLevelFromIccCal + 4.0;
            }

            Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
            Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;
            Eq.Site[Site].RF.SA.ReferenceLevel = TestCon.TargetPout + Eq.Site[Site].RF.ActiveWaveform.PAR + 3.0;
            double GainAccuracy = 3.0;
            Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;
            Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
            double ExpectedGain = TestCon.ExpectedGain + GainAccuracy + 1;
            SwStopRun("PoutServo-ConfigureSASG", Site);
            SwStartRun("Configure_Servo", Site);
            //Eq.Site[Site].RF.ConfigureServo(TestCon.TargetPout, 0.04,TestCon.ExpectedGain, 4, 10);
            Eq.Site[Site].RF.Configure_Servo(new EqRF.Config(TestCon.TargetPout, ExpectedGain, sgLevelFromIccCal, 0.04, turboServo));
            SwStopRun("Configure_Servo", Site);
            SwStartRun("Servo", Site);
            Eq.Site[Site].RF.Servo(out this.TestResult.Pout, out this.TestResult.Pin, outputPathGain_withIccCal);
            SwStopRun("Servo", Site);

            if (this.TestResult.Pout == float.NegativeInfinity || (double)this.TestResult.Pout == 0.0)
            {
                this.TestResult.Pout = -50f;
            }

            SWtimePreviousPin = TestResult.Pin; // Save previous servo pin
            SWtimePreviousPout = TestResult.Pout;
            SWtimePreviousTPout = TestCon.TargetPout;

            if (blnCpl)
                MeasureInitCplPout(false);
        }

        private void FixedPinSet()  // Added by Ken Hilla Dec 19 2017
        {
            try
            {
                TestResult.Pin = TestCon.TargetPin;

                //Eq.Site[Site].RF.SA.Abort();
                //Eq.Site[Site].RF.SG.Abort();
                SwStartRun("SASG-SetActiveWaveform", Site);

                Eq.Site[Site].RF.SetActiveWaveform(TestCon.ModulationStd, TestCon.WaveformName, false, true);
                Eq.Site[Site].RF.ServoEnabled = false;

                Eq.Site[Site].RF.SG.CenterFrequency = TestCon.FreqSG * 1e6;
                Eq.Site[Site].RF.SG.Level = TestCon.TargetPin;
                Eq.Site[Site].RF.SG.ExternalGain = inputPathGain_withIccCal;

                Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
                Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(TestCon.TargetPin + TestCon.ExpectedGain + Eq.Site[Site].RF.ActiveWaveform.PAR, 30 - outputPathGain_withIccCal - 0.001);
                Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;
                SwStopRun("SASG-SetActiveWaveform", Site);
                SwStartRun("SA.Initiate", Site);
                Eq.Site[Site].RF.SA.Initiate();
                SwStopRun("SA.Initiate", Site);
                SwStartRun("SG.Initiate", Site);

                Eq.Site[Site].RF.SG.Initiate();
                SwStopRun("SG.Initiate", Site);
                SwStartRun("SA.MeasureChanPower", Site);
                Thread.Sleep(1);
                TestResult.Pout = Eq.Site[Site].RF.SA.MeasureChanPower(false);
                Thread.Sleep(1);
                SwStopRun("SA.MeasureChanPower", Site);

                if (blnCpl)
                    MeasureInitCplPout(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        //keng shan added
        private void GetLossFactors()
        {            
            inputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.InputGain);
            outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);
        }

        private void Configure_RFonoff_SA()
        {
            Eq.Site[Site].RF.SA.Abort(Site);
            Eq.Site[Site].RF.SA.CenterFrequency = TestCon.FreqSG * 1e6;
            Eq.Site[Site].RF.SA.ReferenceLevel = Math.Min(TestCon.TargetPout + Eq.Site[Site].RF.ActiveWaveform.PAR, 30.0 - outputPathGain_withIccCal - 0.001);
            Eq.Site[Site].RF.SA.ExternalGain = outputPathGain_withIccCal;
            Eq.Site[Site].RF.SA.SampleRate = TestCon.SaSampleRate;
            Eq.Site[Site].RF.SA.NumberOfSamples = TestCon.SaNumberOfPoints;
            Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig3; //KH
            //Eq.Site[Site].RF.SA.TriggerIn = TriggerLine.PxiTrig3;
        }

        private void RunRfTest()
        {
            Eq.Site[Site].RF.SA.Initiate();

            //Eq.Site[Site].HSDIO.TriggerOut = TriggerLine.PxiTrig3;  // db: doesn't yet work

            Eq.Site[Site].HSDIO.SendTRIGVectors();
            TestResult.rfOnOffTrace = Eq.Site[Site].RF.SA.MeasureIqTrace(true);

            TestResult.RfSettling = CalculateRFONOFF(TestResult.rfOnOffTrace);
        }

        private void RunSwTest(ref RfOnOffTestResult.SettlingTime settlingTime, ref niComplexNumber[] OnOffTrace)
        {
            SwStartRun("RF.SA.Initiate", Site);

            Eq.Site[Site].RF.SA.Initiate();
            SwStopRun("RF.SA.Initiate", Site);
            SwStartRun("HSDIO.SendTRIGVectors", Site);

            Eq.Site[Site].HSDIO.SendTRIGVectors();
            SwStopRun("HSDIO.SendTRIGVectors", Site);
            SwStartRun("SA.MeasureIqTrace", Site);
            OnOffTrace = Eq.Site[Site].RF.SA.MeasureIqTrace(true);
            SwStopRun("SA.MeasureIqTrace", Site);
            SwStartRun("CalculateRFONOFF", Site);
            settlingTime = CalculateRFONOFF(OnOffTrace);
            SwStopRun("CalculateRFONOFF", Site);
        }

        private void MeasureInitCplPout(bool byCalc)
        {
            //switch to CPL
            SwStartRun("MeasureInitCplPout-SwMatrix.ActivatePath", Site);
            // For Quadsite, single Controller dual sites 	
            string BandTemp = TestCon.Band;
            byte SiteTemp = Site;
            if (Site.Equals(0) == false)
            {
                BandTemp = BandTemp + "_" + Site.ToString();
                SiteTemp = 0;
            }
            Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, Operation.MeasureCpl);
            if (TestCon.Cpl.ToUpper().Contains("REV"))
                Eq.Site[SiteTemp].SwMatrix.ActivatePath(BandTemp, Operation.VSGtoANT1);
            //Measure
            SwStopRun("MeasureInitCplPout-SwMatrix.ActivatePath", Site);
            SwStartRun("MeasureInitCplPout-SA.MeasureChanPower", Site);

            Thread.Sleep(1);
            TestResult.InitPout = Eq.Site[Site].RF.SA.MeasureChanPower(byCalc);
            SwStopRun("MeasureInitCplPout-SA.MeasureChanPower", Site);

        }

        [Obsolete]
        private void RunRfTest_withIccMeasure()
        {
            Eq.Site[Site].DC["Vcc"].SetupCurrentTraceMeasurement(0.001, 1E-06, TriggerLine.PxiTrig3);

            Eq.Site[Site].RF.SA.Initiate();

            //Eq.Site[Site].HSDIO.TriggerOut = TriggerLine.PxiTrig3;  // db: doesn't yet work

            Eq.Site[Site].HSDIO.SendTRIGVectors();
            Task<niComplexNumber[]> taskRfTrace = Task<niComplexNumber[]>.Run(() => Eq.Site[Site].RF.SA.MeasureIqTrace(true));
            Task<double[]> taskDcTrace = Task<double[]>.Run(() => Eq.Site[Site].DC["Vcc"].MeasureCurrentTrace());

            TestResult.rfOnOffTrace = taskRfTrace.Result;
            TestResult.TraceIcc = taskDcTrace.Result;

            TestResult.RfSettling = CalculateRFONOFF(TestResult.rfOnOffTrace);
            TestResult.DcSettling = CalculateDCONOFF(TestResult.TraceIcc);
        }

        private void ConfigureVoltageAndCurrent()
        {
            foreach (string current in TestCon.DcSettings.Keys)
            {
                if ((!Eq.Site[Site].HSDIO.IsMipiChannel(current.ToUpper())) || (current.ToUpper() == "VCC"))
                {
                    string msg = String.Format("ForceVoltage on pin {0}", current);
                    SwStartRun(msg, Site);
                    Eq.Site[Site].DC[current].ForceVoltage(TestCon.DcSettings[current].Volts, TestCon.DcSettings[current].Current);
                    SwStopRun(msg, Site);
                }
            }
        }

        //private RfOnOffTestResult.SettlingTime CalculateRFONOFF(niComplexNumber[] rfOnOffTrace)
        //{
        //    RfOnOffTestResult.SettlingTime settlingTime = new RfOnOffTestResult.SettlingTime();

        //    try
        //    {
        //        settlingTime.onTime = 999.0;
        //        settlingTime.offTime = 999.0;
        //        double[] dbmArray = new double[TestCon.SaNumberOfPoints];

        //        for (int i = 0; i < TestCon.SaNumberOfPoints; i++)
        //        {
        //            double vPeak = Math.Sqrt(Math.Pow(rfOnOffTrace[i].Real, 2.0) + Math.Pow(rfOnOffTrace[i].Imaginary, 2.0));
        //            double vRms = vPeak / Math.Sqrt(2.0);
        //            double mW = vRms * vRms * 1000.0 / 50.0;
        //            dbmArray[i] = 10.0 * Math.Log10(mW);
        //        }

        //        double onWindowLengthS = 0.0006745 - 2.5e-6;  // db hardcoded. Where can I pick this up? /to debug only pause here
        //        int onWindowLengthSamples = (int)(onWindowLengthS * TestCon.SaSampleRate);

        //        //for (int j = onWindowLengthSamples; j > 0; j--)
        //        for (int j = 0; j < 1000; j++)
        //        {
        //            if (Math.Abs(dbmArray[j] - TestCon.TargetPout) <= TestCon.PowerAccuracy)
        //            {
        //                settlingTime.onTime = (double)Math.Abs(j - 1) / TestCon.SaSampleRate;
        //                break;
        //            }
        //        }

        //        double offWindowStartS = 0.000675 - 2.5e-6;  // db hardcoded. Where can I pick this up?
        //        int offWindowStartSamples = 1900;//(int)(offWindowStartS * TestCon.SaSampleRate);

        //        for (int k = offWindowStartSamples; k < dbmArray.Length; k++)
        //        {
        //            if (Math.Abs(dbmArray[k] - TestCon.TargetPout) <= TestCon.PowerAccuracy)
        //            {
        //                double RfOutUpper = dbmArray[k];
        //                for (int l = k; l < dbmArray.Length; l++)
        //                {
        //                    if (dbmArray[l] < TestCon.RfOffValue)
        //                    {
        //                        double RfOutLower = dbmArray[l];
        //                        settlingTime.offTime = (double)(l - (k - 1)) / TestCon.SaSampleRate;
        //                        break;
        //                    }
        //                }
        //                break;
        //            }
        //        }
        //        //settlingTime.onTime -= 2.4999999999999998E-06;    // db: I believe this was added to compensate for PreTriggerSample
        //        //settlingTime.offTime -= 2.4999999999999998E-06;   // db: I believe this was added to compensate for PreTriggerSample
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }

        //    return settlingTime;
        //}

        private RfOnOffTestResult.SettlingTime CalculateRFONOFF(niComplexNumber[] rfOnOffTrace)
        {
            double dblAvgRFInitValue = -999;
            double dblFirstValue = -999;
            double dblRefVal = 0;
            double timePerPoint = 1 / TestCon.SaSampleRate;

            RfOnOffTestResult.SettlingTime settlingTime = new RfOnOffTestResult.SettlingTime();

            try
            {
                settlingTime.onTime = 999.0;
                settlingTime.offTime = 999.0;
                double[] dbmArray = new double[TestCon.SaNumberOfPoints];

                int RFOnSample = 1397;

                for (int i = 0; i < TestCon.SaNumberOfPoints; i++)
                {
                    double vPeak = Math.Sqrt(Math.Pow(rfOnOffTrace[i].Real, 2.0) + Math.Pow(rfOnOffTrace[i].Imaginary, 2.0));
                    double vRms = vPeak / Math.Sqrt(2.0);
                    double mW = vRms * vRms * 1000.0 / 50.0;
                    dbmArray[i] = 10.0 * Math.Log10(mW);
                }

                settlingTime.RawTrace = dbmArray;

                double onWindowLengthS = 0.0006745 - 2.5e-6;  // db hardcoded. Where can I pick this up? /to debug only pause here
                int onWindowLengthSamples = (int)(onWindowLengthS * TestCon.SaSampleRate);

                #region Switching Time Calculation

                //automatically detect rise / fall time measurement
                settlingTime.blnRiseTime = false;

                for (int i = 0; i < RFOnSample - 1; i++)
                {
                    if (Convert.ToString(dbmArray[i]) != "-Infinity")
                    {
                        dblFirstValue = dbmArray[i];
                        break;
                    }
                }

                if (dblFirstValue < dbmArray[RFOnSample])
                {
                    settlingTime.blnRiseTime = true;
                }

                #region Get Initial Value
                double dblTotalValue = 0;
                int iCnt = 0;

                //if (TestCon.strSWSpecPos.ToUpper() == "E")
                //{
                //    for (int iSample = RFOnSample; iCnt < TestCon.RFOnTimeAvgPoint; iSample--)
                //    {
                //        if (Convert.ToString(dbmArray[iSample]) != "-Infinity")
                //        {
                //            dblTotalValue += dbmArray[iSample];
                //            iCnt++;
                //        }
                //    }
                //}
                //else
                //{
                for (int iSample = 0; iCnt < TestCon.RFOnTimeAvgPoint; iSample++)
                {
                    if (Convert.ToString(dbmArray[iSample]) != "-Infinity")
                    {
                        dblTotalValue += dbmArray[iSample];
                        iCnt++;
                    }
                }
                //}

                dblAvgRFInitValue = dblTotalValue / iCnt;

                #endregion

                double dblPercent = 0;
                iCnt = 0;

                dblPercent = Math.Abs(10 * Math.Log10(0.9)); //fixed at 10%

                for (int i = RFOnSample - 10; i < RFOnSample + 10; i++)
                {
                    if (Convert.ToString(dbmArray[i]) != "-Infinity")
                    {
                        dblRefVal += dbmArray[i];
                        iCnt++;
                    }

                }
                dblRefVal = dblRefVal / iCnt;

                settlingTime.lastVal = dblRefVal;//only show 1397 data

                if (settlingTime.blnRiseTime)
                    dblRefVal -= dblPercent;
                else
                {
                    dblPercent = Math.Abs(10 * Math.Log10(0.8)); //fixed at 10%
                    dblRefVal += dblPercent;
                }


                if (settlingTime.blnRiseTime)
                {
                    if (dblAvgRFInitValue > dblRefVal)   //-70, every band has different iso value
                    {
                        settlingTime.onTime = 999;
                    }
                    else
                    {
                        for (int iSample = 0; iSample < RFOnSample - 1; iSample++)
                        {
                            if (Convert.ToString(dbmArray[iSample]) != "-Infinity")
                            {
                                if (dbmArray[iSample] >= dblRefVal)
                                {
                                    settlingTime.onTime = Math.Abs((iSample + 1)) * timePerPoint;
                                    //if (settlingTime.onTime > 3.5e-6)
                                    //    MessageBox.Show("fail");//TestResult.Pout = dbmArray[iSample+1];
                                    break;
                                }
                                else if (iSample == RFOnSample - 2)
                                {
                                    settlingTime.onTime = 0;
                                }
                            }
                        }
                    }
                }
                else    //fall time (may not need)
                {
                    if (dblAvgRFInitValue < dblRefVal)
                    {
                        settlingTime.offTime = 999;
                    }
                    else
                    {
                        for (int iSample = 0; iSample < RFOnSample - 1; iSample++)
                        {
                            if (Convert.ToString(dbmArray[iSample]) != "-Infinity")
                            {
                                if (dbmArray[iSample] <= dblRefVal) //if (dbmArray[iSample] - dblRefValue < 1)
                                {
                                    settlingTime.offTime = Math.Abs((iSample - 1)) * timePerPoint;
                                    //if (settlingTime.offTime > 4e-6)
                                    //    MessageBox.Show("fail");
                                    //TestResult.Pout = dbmArray[iSample - 1];
                                    break;
                                }
                                else if (iSample == RFOnSample - 2)
                                {
                                    settlingTime.offTime = 0;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            settlingTime.firstVal = dblFirstValue;


            return settlingTime;
        }


        private RfOnOffTestResult.SettlingTime CalculateDCONOFF(double[] TraceIcc)
        {
            RfOnOffTestResult.SettlingTime settlingTime = new RfOnOffTestResult.SettlingTime();

            try
            {
                settlingTime.onTime = 999.0;
                settlingTime.offTime = 999.0;
                double num = TraceIcc[25] * 0.1;
                for (int i = 1; i < 29; i++)
                {
                    if (TraceIcc[i] >= TraceIcc[25] - num && TraceIcc[i] <= TraceIcc[25] + num)
                    {
                        settlingTime.onTime = (double)i * 1E-06;
                        break;
                    }
                }
                num = TraceIcc[600] * 0.9;
                for (int j = 606; j < TraceIcc.Length - 1; j++)
                {
                    if (TraceIcc[j] <= TraceIcc[25] - num)
                    {
                        settlingTime.offTime = (double)(j + 1 - 606) * 1E-06;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return settlingTime;
        }

        public void BuildResults(ref ATFReturnResult results)
        {
            int TraceQtty = 1;

            if (TestCon.TestPin)
            {
                ResultBuilder.AddResult(Site, TestCon.CktID + "Pin_" + TestCon.TestParaName, "dBm", TestResult.Pin, 4);
            }
            if (TestCon.TestPout)
            {
                ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName, "dBm", TestResult.Pout, 4);
            }

            if (this.TestResult.RfSettling.blnRiseTime)
                ResultBuilder.AddResult(Site, TestCon.CktID + "RF_OnOfftime_" + TestCon.TestParaName, "S", TestResult.RfSettling.onTime, 9);
            else// Auto detect for rise or fall doesnt always work on bad part which will trow error when clotho looks at header name. Temporarly record both columns
                ResultBuilder.AddResult(Site, TestCon.CktID + "RF_OnOfftime_" + TestCon.TestParaName, "S", TestResult.RfSettling.offTime, 9);
            ResultBuilder.AddResult(Site, TestCon.CktID + "RF_OnOffFirstValue_" + TestCon.TestParaName, "dBm", TestResult.RfSettling.firstVal, 9);
            ResultBuilder.AddResult(Site, TestCon.CktID + "RF_OnOffLastValue_" + TestCon.TestParaName, "dBm", TestResult.RfSettling.lastVal, 9);

            if (TestCon.TestSw)
            {
                ++TraceQtty;
                if (this.TestResult.SwSettling.blnRiseTime)
                    ResultBuilder.AddResult(Site, TestCon.CktID + "SWtime_" + TestCon.TestParaName, "S", TestResult.SwSettling.onTime, 9);
                else
                    ResultBuilder.AddResult(Site, TestCon.CktID + "SWtime_" + TestCon.TestParaName, "S", TestResult.SwSettling.offTime, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "SWFirstValue_" + TestCon.TestParaName, "dBm", TestResult.SwSettling.firstVal, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "SWLastValue_" + TestCon.TestParaName, "dBm", TestResult.SwSettling.lastVal, 9);
                if (blnCpl)
                    ResultBuilder.AddResult(Site, TestCon.CktID + "SWInitialValue_" + TestCon.TestParaName, "dBm", TestResult.InitPout, 9);
            }

            string[] SWTimeArr = TestCon.SWTIMECUSTOM.Split(':');

            if (SWTimeArr.Length > 2)
            {
                ++TraceQtty;
                if (this.TestResult.Sw2Settling.blnRiseTime)
                    ResultBuilder.AddResult(Site, TestCon.CktID + "SW2time_" + TestCon.TestParaName, "S", TestResult.Sw2Settling.onTime, 9);
                else
                    ResultBuilder.AddResult(Site, TestCon.CktID + "SW2time_" + TestCon.TestParaName, "S", TestResult.Sw2Settling.offTime, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "SW2FirstValue_" + TestCon.TestParaName, "dBm", TestResult.Sw2Settling.firstVal, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "SW2LastValue_" + TestCon.TestParaName, "dBm", TestResult.Sw2Settling.lastVal, 9);
                if (blnCpl)
                    ResultBuilder.AddResult(Site, TestCon.CktID + "SW2InitialValue_" + TestCon.TestParaName, "dBm", TestResult.InitPout, 9);
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


            #region Switching Time Trace file saving


            if (RFOnOffTraceEnable == "TRUE")
            {
                if (tracePIDCount <= TraceFileNum)
                {
                    SaveTrace(TraceQtty);
                }
            }
            #endregion
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

        private void SaveTrace(int TraceNo)
        {
            double[] OutPower = new double[TestCon.SaNumberOfPoints];

            #region generate folder
            string RFOnOffRawData = "";

            string ResultFileName = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, "");
            if (ResultFileName == "") ResultFileName = "Debug";
            string finalfolder = folderpath + ResultFileName + "\\Unit_" + GetCurrentPID();


            DirectoryInfo GeneratedFolder = new DirectoryInfo(finalfolder);
            GeneratedFolder.Create();

            GeneratedFolder = new DirectoryInfo(finalfolder);
            GeneratedFolder.Create();


            RFOnOffRawData = finalfolder + @"\" + TestCon.TestParaName + "_" + DateTime.Now.ToString("_yyyyMMdd_HHmmss_fffff") + ".csv";
            FileInfo RFOnOfffile = new FileInfo(RFOnOffRawData);
            StreamWriter writeRFOnOffdata = RFOnOfffile.CreateText();
            DataTable dtRFOnOffData = new DataTable();
            #endregion

            #region GenerateFile
            try
            {
                for (int count = 0; count < TraceNo; count++)
                {
                    if (count == 0)
                    {
                        DataColumn[] dcRFOnOffData = new DataColumn[TraceNo + 1];
                        for (int iDc = 0; iDc < dcRFOnOffData.Length; iDc++)
                        {
                            dcRFOnOffData[iDc] = new DataColumn();
                            if (iDc == 0)
                            {
                                dcRFOnOffData[iDc].ColumnName = "Sample No.";
                                dcRFOnOffData[iDc].DataType = System.Type.GetType("System.Int32");
                            }
                            else
                            {
                                dcRFOnOffData[iDc].ColumnName = "Data" + iDc;
                                dcRFOnOffData[iDc].DataType = Type.GetType("System.Double");
                            }
                        }
                        dtRFOnOffData.Columns.AddRange(dcRFOnOffData);

                        for (int k = 0; k < TestCon.SaNumberOfPoints; k++)
                        {
                            DataRow drRFOnOffData = dtRFOnOffData.NewRow();
                            dtRFOnOffData.Rows.Add(drRFOnOffData);
                            dtRFOnOffData.Rows[k][0] = k + 1;
                        }
                    }

                    switch (count)
                    {
                        case 0: OutPower = TestResult.RfSettling.RawTrace; break;
                        case 1: OutPower = TestResult.SwSettling.RawTrace; break;
                        case 2: OutPower = TestResult.Sw2Settling.RawTrace; break;
                    }

                    for (int k = 0; k < TestCon.SaNumberOfPoints; k++)
                    {
                        dtRFOnOffData.Rows[k][count + 1] = OutPower[k];
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Write RFOnOff Test Raw data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            for (int iDc = 0; iDc < dtRFOnOffData.Columns.Count; iDc++)
            {
                writeRFOnOffdata.Write(dtRFOnOffData.Columns[iDc].ColumnName + ",");
            }
            writeRFOnOffdata.Write(Environment.NewLine);
            foreach (DataRow dr in dtRFOnOffData.Rows)
            {

                for (int iDc = 0; iDc < dtRFOnOffData.Columns.Count; iDc++)
                {
                    writeRFOnOffdata.Write(dr[iDc] + ",");
                }
                writeRFOnOffdata.Write(Environment.NewLine);
            }

            writeRFOnOffdata.Close();
            #endregion
        }
    }

    public class RfOnOffTestCondition
    {
        public string PowerMode;
        public string ModulationStd;
        public string WaveformName;
        public string Band;
        public string CktID;
        public double FreqSG;
        public string TestParaName;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public string MipiDacBitQ1;
        public string MipiDacBitQ2;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public float TargetPin;
        public float TargetPout;
        public float ExpectedGain;
        public double SaSampleRate = 40e6;
        public int SaNumberOfPoints = 3000;
        public double PowerAccuracy = 0.5;
        public double RfOffValue = -20.0;
        public bool TestPin;
        public bool TestPout;
        public bool TestSw;
        public bool resetSA;
        //keng shan added
        public string SWTIMECUSTOM;
        public Operation VsaOperation;
        public Operation VsgOperation;
        public double RFOnTimeLimH = -999;
        public double RFOnTimeLimL = -999;
        public int RFOnTimeAvgPoint = 1;
        public string strSWLvlSpec, strSWSpecPos;
        public bool UsePreviousPin = false;
        public string Cpl;
        public string TXRX;

        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();
        public Dictionary<string, TestLib.DPAT_Variable> TestDPAT = new Dictionary<string, TestLib.DPAT_Variable>();
    }

    public class RfOnOffTestResult
    {
        public double Pin;
        public double Pout;
        public double InitPout;
        public niComplexNumber[] rfOnOffTrace;
        public niComplexNumber[] swOnOffTrace;
        public niComplexNumber[] sw2OnOffTrace;
        public niComplexNumber[] iqTrace;
        public double[] TraceIcc;
        public SettlingTime RfSettling = new SettlingTime();
        public SettlingTime SwSettling = new SettlingTime();
        public SettlingTime Sw2Settling = new SettlingTime();
        public SettlingTime DcSettling = new SettlingTime();
        public AclrResults ACLR;

        public class SettlingTime
        {
            public double onTime, offTime, firstVal, lastVal;
            public bool blnRiseTime = false;
            public double[] RawTrace = new double[] { 0 };
        }
    }
}
