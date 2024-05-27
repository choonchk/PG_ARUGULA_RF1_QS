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
using ProductionLib;

namespace TestLib
{
    public class IIP3Test : TimingBase, iTest
    {

        public byte Site;
        public IIP3TestCondition TestCon = new IIP3TestCondition();
        public double inputPathGain_withIccCal = 0;
        public double outputPathGain_withIccCal = 0;
        public TriggerLine dcTrigLine;
        private Task CalcTask;
        public bool SkipOutputPortOnFail = false;
        public HiPerfTimer uTimer = new HiPerfTimer();
        public bool Mordor = false;

        public bool Initialize(bool finalScript)
        {
            InitializeTiming(this.TestCon.TestParaName);

            //SwStartRun("IIP3Test-Initialize-RFConfigureSpecAn", Site);
            if (IQ.Mem.ContainsKey(TestCon.ModulationStd + TestCon.WaveformName))
            {
                TestCon.IqWaveform = IQ.Mem[TestCon.ModulationStd + TestCon.WaveformName];
            }
            if (EqLib.EqRF.NIVST_Rfmx.RFmxFlagOn)
            {
                outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain) + 5;

                Eq.Site[Site].RF.RFmxConfigureSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxIIP3, new EqRF.Config(Eq.Site[Site].RF.CRfmxIIP3.GetSpecIteration(), TestCon.FreqSG, TestCon.TargetPin + TestCon.ExpectedGain + outputPathGain_withIccCal + IQ.Mem["TWOTONE"].PAR,
                    TestCon.PowerMode, TestCon.Band, inputPathGain_withIccCal, Site));
                Eq.Site[Site].RF.CRfmxIIP3.SpecIteration();
            }
            //SwStopRun("IIP3Test-Initialize-RFConfigureSpecAn", Site);

            return true;
        }
        public IIP3TestResults TestResult;

        public int RunTest()
        {
            SwBeginRun(Site);

            TestResult = new IIP3TestResults();

            SwStartRun("SetSwitchMatrixPath-FirstTime", Site);
            SetSwitchMatrixPaths();
            SwStopRun("SetSwitchMatrixPath-FirstTime", Site);
            SwStartRun("GetLossFactors", Site);
            GetLossFactors();
            SwStopRun("GetLossFactors", Site);
            ConfigureVoltageAndCurrent();

            SwStartRun("SendMipiCommands", Site);
            Eq.Site[Site].HSDIO.SendMipiCommands(TestCon.MipiCommands);
            SwStopRun("SendMipiCommands", Site);

            int Get_Iteration = Eq.Site[Site].RF.CRfmxIIP3.GetSpecIteration();

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
                Thread.Sleep(0);
            }
            else
            {
                SwStartRun("ConfigureIIP3", Site);
                Eq.Site[Site].RF.Configure_IIP3(new EqRF.Config_IIP3(Eq.Site[Site].RF.CRfmxIIP3.GetSpecIteration(), TestCon.FreqSG, TestCon.TargetPin, inputPathGain_withIccCal, outputPathGain_withIccCal));
                SwStopRun("ConfigureIIP3", Site);
                SwStartRun("Measure_IIP3", Site);
                Eq.Site[Site].RF.Measure_IIP3();
                SwStopRun("Measure_IIP3", Site);
                
                SwStartRun("CalcResults", Site);
                CalcResults();
                SwStopRun("CalcResults", Site);
            }

            Eq.Site[Site].RF.CRfmxIIP3.SpecIteration();

            //CalcTask = Task.Run(() => CalcResults());
            
            SwStartRun("SG.Abort", Site);
            Eq.Site[Site].RF.SG.Abort();
            SwStopRun("SG.Abort", Site);

            SwEndRun(Site);

            return 0;
        }

        private void CalcResults()
        {

            //bool useCorrelationOffsetsForCalculatedValue = (ResultBuilder.corrFileExists & !GU.runningGU[Site]) | GU.GuMode[Site] == GU.GuModes.Vrfy;

            EqLib.EqRF.RFmxResult _RFmxResult = Eq.Site[Site].RF.RFmxRetrieveResultsSpec(EqLib.EqRF.eRfmx_Measurement_Type.eRfmxIIP3, new EqRF.Config(Eq.Site[Site].RF.CRfmxIIP3.GetSpecIteration()));

            TestResult.lowerTonePower = _RFmxResult.lowerTonePower;
            TestResult.upperTonePower = _RFmxResult.upperTonePower;
            TestResult.lowerIntermodPower = _RFmxResult.lowerIntermodPower;
            TestResult.upperIntermodPower = _RFmxResult.upperIntermodPower;
            TestResult.intermodOrder = _RFmxResult.intermodOrder;


            TestResult.LowTwoTonePower = TestResult.lowerTonePower - outputPathGain_withIccCal;
            TestResult.HighTwoTonePower = TestResult.upperTonePower - outputPathGain_withIccCal;

            TestResult.LowIM3 = TestResult.lowerIntermodPower[0] - outputPathGain_withIccCal;
            TestResult.HighIM3 = TestResult.upperIntermodPower[0] - outputPathGain_withIccCal;

            TestResult.LowIMD = Math.Abs(TestResult.lowerTonePower - TestResult.lowerIntermodPower[0]);
            TestResult.HighIMD = Math.Abs(TestResult.upperTonePower - TestResult.upperIntermodPower[0]);

            TestResult.LowOIP3 = (TestResult.lowerTonePower - outputPathGain_withIccCal) + TestResult.LowIMD / 2;
            TestResult.HighOIP3 = (TestResult.upperTonePower - outputPathGain_withIccCal) + TestResult.HighIMD / 2;

            TestResult.LowGain = (TestResult.lowerTonePower - outputPathGain_withIccCal) - Convert.ToSingle(TestCon.TargetPin);
            TestResult.HighGain = (TestResult.upperTonePower - outputPathGain_withIccCal) - Convert.ToSingle(TestCon.TargetPin);

            TestResult.LowIIP3 = TestResult.LowOIP3 - TestResult.LowGain;
            TestResult.HighIIP3 = TestResult.HighOIP3 - TestResult.HighGain;
           
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
        }
        public void BuildResults(ref ATFReturnResult results)
        {
            if (ResultBuilder.headerFileMode) return;

            bool useCorrelationOffsetsForCalculatedValue = (ResultBuilder.corrFileExists & !GU.runningGU[Site]) | GU.GuMode[Site] == GU.GuModes.Vrfy;

            if (CalcTask != null)
                CalcTask.Wait();
            double LowerIM3Power = 0;
            double UpperIM3Power = 0;
            double lowerTonePower_without_offset = TestResult.lowerTonePower - outputPathGain_withIccCal;
            double upperrTonePower_without_offset = TestResult.upperTonePower - outputPathGain_withIccCal;
            LowerIM3Power = Convert.ToDouble(TestResult.lowerIntermodPower[0]);
            UpperIM3Power = Convert.ToDouble(TestResult.upperIntermodPower[0]);
            double LowerIM3Power_without_offset = LowerIM3Power - outputPathGain_withIccCal;
            double UpperIM3Power_without_offset = UpperIM3Power - outputPathGain_withIccCal;

            if (useCorrelationOffsetsForCalculatedValue)
            {
                //TestResult.lowerTonePower = GU.getValueWithCF(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName + "x", TestResult.lowerTonePower);
                //TestResult.upperTonePower = GU.getValueWithCF(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName.Replace("Lower", "Upper") + "x", TestResult.upperTonePower);
                //LowerIM3Power = GU.getValueWithCF(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName + "x", LowerIM3Power);
                //UpperIM3Power = GU.getValueWithCF(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName.Replace("Lower", "Upper") + "x", UpperIM3Power);

                TestResult.lowerTonePower = GU.getValueWithCF(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName, TestResult.lowerTonePower);
                TestResult.upperTonePower = GU.getValueWithCF(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName.Replace("Lower", "Upper"), TestResult.upperTonePower);
                LowerIM3Power = GU.getValueWithCF(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName, LowerIM3Power);
                UpperIM3Power = GU.getValueWithCF(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName.Replace("Lower", "Upper"), UpperIM3Power);

                TestResult.LowTwoTonePower = TestResult.lowerTonePower - outputPathGain_withIccCal;
                TestResult.HighTwoTonePower = TestResult.upperTonePower - outputPathGain_withIccCal;

                TestResult.LowIM3 = LowerIM3Power - outputPathGain_withIccCal;
                TestResult.HighIM3 = UpperIM3Power - outputPathGain_withIccCal;

                TestResult.LowIMD = Math.Abs(TestResult.lowerTonePower - LowerIM3Power);
                TestResult.HighIMD = Math.Abs(TestResult.upperTonePower - UpperIM3Power);

                TestResult.LowOIP3 = (TestResult.lowerTonePower - outputPathGain_withIccCal) + TestResult.LowIMD / 2;
                TestResult.HighOIP3 = (TestResult.upperTonePower - outputPathGain_withIccCal) + TestResult.HighIMD / 2;

                TestResult.LowGain = (TestResult.lowerTonePower - outputPathGain_withIccCal) - Convert.ToSingle(TestCon.TargetPin);
                TestResult.HighGain = (TestResult.upperTonePower - outputPathGain_withIccCal) - Convert.ToSingle(TestCon.TargetPin);

                TestResult.LowIIP3 = TestResult.LowOIP3 - TestResult.LowGain;
                TestResult.HighIIP3 = TestResult.HighOIP3 - TestResult.HighGain;
            }

            if (TestCon.TestIIP3)
            {
                //ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName + "x", "dB", lowerTonePower_without_offset, 9);  //hosein 04272020
                //ResultBuilder.AddResult(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName + "x", "dB", LowerIM3Power_without_offset, 9);
                //ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_" + TestCon.TestParaName + "x", "dB", TestResult.LowGain, 9);
                //ResultBuilder.AddResult(Site, TestCon.CktID + "IIP3_" + TestCon.TestParaName.Replace("Upper", "Lower") + TestCon.SpecNumber["Para.IIP3"], "dB", TestResult.LowOIP3 - TestResult.LowGain, 9);

                //ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName.Replace("Lower", "Upper") + "x", "dB", upperrTonePower_without_offset, 9);
                //ResultBuilder.AddResult(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName.Replace("Lower", "Upper") + "x", "dB", UpperIM3Power_without_offset, 9);
                //ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_" + TestCon.TestParaName.Replace("Lower", "Upper") + "x", "dB", TestResult.HighGain, 9);
                //ResultBuilder.AddResult(Site, TestCon.CktID + "IIP3_" + TestCon.TestParaName.Replace("Lower", "Upper") + TestCon.SpecNumber["Para.IIP3"], "dB", TestResult.HighOIP3 - TestResult.HighGain, 9);

                ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName , "dB", lowerTonePower_without_offset, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName , "dB", LowerIM3Power_without_offset, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_" + TestCon.TestParaName , "dB", TestResult.LowGain, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "IIP3_" + TestCon.TestParaName.Replace("Upper", "Lower") , "dB", TestResult.LowOIP3 - TestResult.LowGain, 9);

                ResultBuilder.AddResult(Site, TestCon.CktID + "Pout_" + TestCon.TestParaName.Replace("Lower", "Upper") , "dB", upperrTonePower_without_offset, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "IM3_" + TestCon.TestParaName.Replace("Lower", "Upper") , "dB", UpperIM3Power_without_offset, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "Gain_" + TestCon.TestParaName.Replace("Lower", "Upper") , "dB", TestResult.HighGain, 9);
                ResultBuilder.AddResult(Site, TestCon.CktID + "IIP3_" + TestCon.TestParaName.Replace("Lower", "Upper") , "dB", TestResult.HighOIP3 - TestResult.HighGain, 9);
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
        private void GetLossFactors()
        {            
            inputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsgOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.InputGain);
            outputPathGain_withIccCal = CableCal.GetCF(Site, TestCon.Band, TestCon.VsaOperation, TestCon.FreqSG) + GU.getIccCalfactor(Site, TestCon.TestParaName, GU.IccCalGain.OutputGain);

        }


    }
    public class IIP3TestCondition
    {
        public string TestParaName;
        public string PowerMode;
        public string Band;
        public string ModulationStd;
        public string WaveformName;
        public string ParameterNote;
        public IQ.Waveform IqWaveform;
        public Operation VsaOperation;
        public Operation VsgOperation;
        public List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands;
        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();
        public bool resetSA;
        public bool TestItotal;
        public bool TestIIP3;
        public string CktID;
        public string CplpreFix;
        public double FreqSG = 0;
        public float TargetPin;
        public double ExpectedGain;
        public int Iteration;
        public bool VIO32MA = false;
        public bool VIORESET = false;

        public Dictionary<string, string> SpecNumber = new Dictionary<string, string>();
        public Dictionary<string, TestLib.DPAT_Variable> TestDPAT = new Dictionary<string, TestLib.DPAT_Variable>();
    }

    public class IIP3TestResults
    {
        public double LowIMD;
        public double LowOIP3;
        public double LowGain;
        public double LowIM3;
        public double LowTwoTonePower;
        public double LowIIP3;

        public double HighIMD;
        public double HighOIP3;
        public double HighGain;
        public double HighIM3;
        public double HighTwoTonePower;
        public double HighIIP3;

        public double lowerTonePower, upperTonePower;
        public int[] intermodOrder = new int[1];
        public double[] lowerIntermodPower = new double[1];
        public double[] upperIntermodPower = new double[1];
        public double[] worstCaseOutputInterceptPower = new double[1];
        public double[] lowerOutputInterceptPower = new double[1];
        public double[] upperOutputInterceptPower = new double[1];
    }
}
