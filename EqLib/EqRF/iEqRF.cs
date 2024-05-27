using System.Threading;
using NationalInstruments.ModularInstruments.Interop;
using ClothoLibAlgo;
using IqWaveform;
using System.Collections.Generic;
using NationalInstruments.RFmx.NRMX;
using NationalInstruments.RFmx.LteMX;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.ModularInstruments;

namespace EqLib
{
    public partial class EqRF
    {
        public static iEqRF Get(string VisaAlias, byte Site)
        {
            iEqRF rf;
            if (VisaAlias.Contains("VXT"))
            {
                rf = new EqRF.KeysightVXT();
            }
            else if (VisaAlias.Contains("VST_RFmx"))
            {
                rf = new EqRF.NIVST_Rfmx();
            }
            else
            {
                rf = new EqRF.NIVST();
            }
            rf.VisaAlias = VisaAlias;
            rf.Site = Site;
            return rf;
        }
        public class Config
        {
            public int Iteration;
            public double Freq;
            public double Reflevel;
            public double SpanforTxL; //Txleakage TxRx spacing measurement by Mario 
            public double PinSweepStop;
            public double TargetPout;
            public double ExpectedGain;
            public double manualSetVSGanalogLev;
            public double powerTolerance;
            public bool turbo;
            public double TargetPin;
            public double LossPin;
            public double LossPout;
            public string Modulation;
            public string Waveformname;
            public string TestMode;
            public string PowerMode;
            public double RefChBW;
            public double[] AclrBW;
            public double[] AclrOffsetFreq;
            public string WaveformName;
            public bool TestAcp;
            public int NumberOfOffsets;
            public double Rbw;
            public double SampleRate;
            public double AcquisitionTime;
            public double TriggerDelay;
            public TriggerLine TriggerLine;
            public string Waveform; //Yoonchun
            public double IqLength; //Yoonchun
            public int ACPaverage; //DH
            public string band;
            public bool TestEVM;
            public byte Site;

            public Config(int Iteration)
            {
                this.Iteration = Iteration;
            }

            public Config(int Iteration, double Reflevel)
            {
                this.Iteration = Iteration;
                this.Reflevel = Reflevel;

            }

            public Config(int Iteration, double Freq, double Reflevel)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
            }
            public Config(int Iteration, double Freq, double Reflevel, string PowerMode)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.PowerMode = PowerMode;
            }
            public Config(int Iteration, double Freq, double Reflevel, string PowerMode, string band, double LossPin, byte site)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.PowerMode = PowerMode;
                this.band = band;
                this.LossPin = LossPin;
                this.Site = site;
            }
            public Config(int Iteration, double Freq, double Reflevel, double SampleRate, double AcquisitionTime, double TriggerDelay, TriggerLine TriggerLine, byte site)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.SampleRate = SampleRate;
                this.AcquisitionTime = AcquisitionTime;
                this.TriggerDelay = TriggerDelay;
                this.TriggerLine = TriggerLine;
                this.Site = site;
            }

            public Config(int Iteration, double Freq, double Reflevel, double SampleRate, double AcquisitionTime, double TriggerDelay, TriggerLine TriggerLine, string WaveformName, byte site)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.SampleRate = SampleRate;
                this.AcquisitionTime = AcquisitionTime;
                this.TriggerDelay = TriggerDelay;
                this.TriggerLine = TriggerLine;
                this.WaveformName = WaveformName;
                this.Site = site;
            }

            public Config(double TargetPout, double ExpectedGain, double manualSetVSGanalogLev, double powerTolerance, bool turbo)
            {
                this.TargetPout = TargetPout;
                this.ExpectedGain = ExpectedGain;
                this.manualSetVSGanalogLev = manualSetVSGanalogLev;
                this.powerTolerance = powerTolerance;
                this.turbo = turbo;

            }

            public Config(int Iteration, double TargetPout, double Freq, double ExpectedGain, double Reflevel, double SpanforTxL,  double LossPin, double LossPout, double RefChBW, double[] AclrBW, double[] AclrOffsetFreq,
                string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw, double IqLength, int ACPaverage, bool EVM, byte site)
            {
                this.Iteration = Iteration;
                this.TargetPout = TargetPout;

                //mario
                if (Freq == 1910 && SpanforTxL == 15)
                {
                    this.Freq = 1922.5;
                }
                else if (Freq == 1780 && SpanforTxL == 20)
                {
                    this.Freq = 1795;
                }
                else if (Freq == 2565 && SpanforTxL == 50)
                {
                    this.Freq = 2595;
                }
                else if (Freq == 2560 && SpanforTxL == 50)
                {
                    this.Freq = 2595;
                }
                else if (Freq == 2570 && SpanforTxL == 50)
                {
                    this.Freq = 2595;
                }
                else if (SpanforTxL == 160 || SpanforTxL == 10 || SpanforTxL == 0)
                {
                    this.Freq = Freq;
                }
                //
                this.ExpectedGain = ExpectedGain;
                this.Reflevel = Reflevel;
                this.SpanforTxL = SpanforTxL;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
                this.RefChBW = RefChBW;
                this.AclrBW = AclrBW;
                this.AclrOffsetFreq = AclrOffsetFreq;
                this.WaveformName = WaveformName;
                this.TestAcp = TestAcp;
                this.NumberOfOffsets = NumberOfOffsets;
                this.Rbw = Rbw;
                this.IqLength = IqLength;//Yoonchun
                this.ACPaverage = ACPaverage;//DH
                this.TestEVM = EVM;
                this.Site = site;
            }

            public Config(int Iteration, double TargetPout, double Freq, double ExpectedGain, double Reflevel, double SpanforTxL, double LossPin, double LossPout, double RefChBW, double[] AclrBW, double[] AclrOffsetFreq,
                string WaveformName, string Waveform, bool TestAcp, int NumberOfOffsets, double Rbw, double IqLength, int ACPaverage, byte Site)
            {
                this.Iteration = Iteration;
                this.TargetPout = TargetPout;
                this.Site = Site;
                //mario
                if (Freq == 1910 && SpanforTxL == 15)
                {
                    this.Freq = 1922.5;
                }
                else if (Freq == 1780 && SpanforTxL == 20)
                {
                    this.Freq = 1795;
                }
                else if (SpanforTxL == 160 || SpanforTxL == 10 || SpanforTxL == 0)
                {
                    this.Freq = Freq;
                }
                //
                this.ExpectedGain = ExpectedGain;
                this.Reflevel = Reflevel;
                this.SpanforTxL = SpanforTxL;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
                this.RefChBW = RefChBW;
                this.AclrBW = AclrBW;
                this.AclrOffsetFreq = AclrOffsetFreq;
                this.WaveformName = WaveformName;
                this.TestAcp = TestAcp;
                this.NumberOfOffsets = NumberOfOffsets;
                this.Rbw = Rbw;
                this.Waveform = Waveform;
                this.IqLength = IqLength;//Yoonchun
                this.ACPaverage = ACPaverage;//DH

            }

            public Config(int Iteration, double Freq, double Reflevel, double PinSweepStop, double LossPin, double LossPout, byte site)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.PinSweepStop = PinSweepStop;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
                this.Site = site;
            }

            public Config(int Iteration, double Freq, double Reflevel, string WaveformName, string band, byte site)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.WaveformName = WaveformName;
                this.band = band;
                this.Site = site;
            }

        }

        public class Config_IQ
        {
            public int Iteration;
            public double Freq;
            public double Reflevel;
            public double PinSweepStop;
            public double LossPin;
            public double LossPout;

            public Config_IQ(int Iteration, double Freq, double Reflevel, double PinSweepStop, double LossPin, double LossPout)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.PinSweepStop = PinSweepStop;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
            }
        }

        public class Config_IQ_EVM
        {
            public int Iteration;
            public double Freq;
            public double Reflevel;
            public double LossPin;
            public double LossPout;

            public Config_IQ_EVM(int Iteration, double Freq, double Reflevel, double PinSweepStop, double LossPin, double LossPout)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
            }
        }

        public class Config_IIP3
        {
            public int Iteration;
            public double Freq;
            public double TargetPin;
            public double Reflevel;
            public double LossPin;
            public double LossPout;

            public Config_IIP3(int Iteration, double Freq, double TargetPin, double LossPin, double LossPout)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.TargetPin = TargetPin;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
            }
        }


        public class Config_Timing
        {
            public int Iteration;
            public double Freq;
            public double TargetPin;
            public double LossPin;
            public double LossPout;

            public Config_Timing(int Iteration, double Freq, double TargetPin, double LossPin, double LossPout)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.TargetPin = TargetPin;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
            }
        }

        public class RFmxCofig
        {
            public int Iteration;
            public double Freq;
            public double Reflevel;
            public double SpanforTxL; //Txleakage TxRx spacing measurement by Mario 
            public double TargetPin;
            public double TargetPout;
            public double ExpectedGain;
            public double manualSetVSGanalogLev;
            public double powerTolerance;
            public bool turbo;
            public double LossPin;
            public double LossPout;
            public string Modulation;
            public string Waveformname;
            public string TestMode;
            public double RefChBW;
            public double[] AclrBW;
            public double[] AclrOffsetFreq;
            public string WaveformName;
            public bool TestAcp;
            public int NumberOfOffsets;
            public double Rbw;

            public RFmxCofig(int Iteration)
            {
                this.Iteration = Iteration;
            }

            public RFmxCofig(double TargetPout, double ExpectedGain, double manualSetVSGanalogLev, double powerTolerance, bool turbo, double LossPin)
            {
                this.TargetPout = TargetPout;
                this.ExpectedGain = ExpectedGain;
                this.manualSetVSGanalogLev = manualSetVSGanalogLev;
                this.powerTolerance = powerTolerance;
                this.turbo = turbo;
                this.LossPin = LossPin;

            }

            public RFmxCofig(int Iteration, double Freq, double Reflevel)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;
            }


            public RFmxCofig(int Iteration, double Freq, double TargetPin, double TargetPout, double ExpectedGain, double manualSetVSGanalogLev, double LossPin, double LossPout, string Modulation, string Waveformname, string TestMode)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.TargetPin = TargetPin;
                this.TargetPout = TargetPout;
                this.ExpectedGain = ExpectedGain;
                this.manualSetVSGanalogLev = manualSetVSGanalogLev;
                this.LossPin = LossPin;
                this.LossPout = LossPout;
                this.Modulation = Modulation;
                this.Waveformname = Waveformname;
                this.TestMode = TestMode;

            }


            public RFmxCofig(int Iteration, double Freq, double Reflevel, double RefChBW, double[] AclrBW, double[] AclrOffsetFreq, string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw)
            {
                this.Iteration = Iteration;
                this.Freq = Freq;
                this.Reflevel = Reflevel;               
                this.RefChBW = RefChBW;
                this.AclrBW = AclrBW;
                this.AclrOffsetFreq = AclrOffsetFreq;
                this.WaveformName = WaveformName;
                this.TestAcp = TestAcp;
                this.NumberOfOffsets = NumberOfOffsets;
                this.Rbw = Rbw;
            }

        }


        public class RFmxResult
        {
            public double averageChannelPower;
            public double lowerTonePower;
            public double upperTonePower;
            public double[] lowerIntermodPower;
            public double[] upperIntermodPower;
            public int[] intermodOrder;

            public RFmxResult()
            {
            }

            public RFmxResult(double averageChannelPower)
            {
                this.averageChannelPower = averageChannelPower;
            }

            public RFmxResult(double lowerTonePower, double upperTonePower, double[] lowerIntermodPower, double[] upperIntermodPower, int[] intermodOrder)
            {
                this.lowerTonePower = lowerTonePower;
                this.upperTonePower = upperTonePower;
                this.lowerIntermodPower = lowerIntermodPower;
                this.upperIntermodPower = upperIntermodPower;
                this.intermodOrder = intermodOrder;
            }
        }

        public interface iEqRF
        {
            iEqSG SG { get; set; }
            iEqSA SA { get; set; }
            iEqRFExtd RFExtd { get; set; }
            RFmxInstrMX InstrSession { get; }
            RfmxAcp CRfmxAcp { get; }
            RfmxAcpNR CRfmxAcpNR { get; }
            RfmxHar2nd CRfmxH2 { get; }
            RfmxHar3rd CRfmxH3 { get; }
            RfmxTxleakage CRfmxTxleakage { get; }
            RfmxIQ CRfmxIQ { get; }
            RfmxChp CRfmxCHP { get; }
            RfmxIQ_EVM CRfmxIQ_EVM { get; }
            RfmxIQ_Timing CRfmxIQ_Timing { get; }
            RfmxLTE_EVM CRfmxEVM_LTE { get; }
            RfmxNR_EVM CRfmxEVM_NR { get; }
            RfmxIIP3 CRfmxIIP3 { get; }
            RfmxChp_For_Cal CRfmxCHP_FOR_CAL { get; }
            niPowerServo NiPowerServo { get; }
            ManualResetEvent[] ThreadFlags { get; }

            IQ.Waveform ActiveWaveform { get; set; }
            string VisaAlias { get; set; }
            byte Site { get; set; }
            bool IsVST1 { get; set; }
            double MaxFreq { get; set; }
            void Initialize(Dictionary<byte, int[]> TrigArray);
            bool ServoEnabled { get; set; }
            void ConfigureServo(double targetOutputPower, double powerTolerance, double expectedGain, ushort minIterations, ushort maxIterations);
            void Configure_Servo(Config servoInfor);

            bool Servo(out double Pout, out double Pin, double LossPout);

            void Configure_Servo_Timing(Config servoInfor);

            bool Servo_Timing(out double Pout, out double Pin, double LossPout);

            void Configure_CHP(Config o);
            void Measure_CHP();
            void Measure_CHP(double SOAK_Delay);

            void Configure_IQ(Config_IQ PinSweepInfor);
            void Measure_IQ();

            void Configure_IQ_EVM(Config_IQ_EVM EVMInfor);
            void Measure_IQ_EVM();

            void Configure_IIP3(Config_IIP3 IIP3);
            void Measure_IIP3();

            void Configure_Timing(Config_Timing o);
            void Measure_Timing();

            void SetActiveWaveform(string ModStd, string WvfrmName, bool useServoScript, bool setSG = true);
            bool LoadWaveform(string ModStd, string WvfrmName);
            void Configure_IIP3_RFSG_Parameters(double LossPin, double RFfrequency, float TargetPin);
            void ResetDriver();
            void ResetRFSA(bool Enable);
            void close();

            void RFmxConfigureSpec(eRfmx _eRfmx, RFmxCofig RfmxCondition);
            void RFmxCommitSpec(eRfmx _eRfmx, RFmxCofig RfmxCondition);
            void RFmxInitiateSpec(eRfmx _eRfmx, RFmxCofig RfmxCondition);
            RFmxResult RFmxRetrieveResultsSpec(eRfmx _eRfmx, RFmxCofig RfmxCondition);

            void RFmxConfigureSpec(eRfmx_Measurement_Type _eRfmx, Config RfmxCondition);
            void RFmxCommitSpec(eRfmx_Measurement_Type _eRfmx, Config RfmxCondition);
            void RFmxInitiateSpec(eRfmx_Measurement_Type _eRfmx, Config RfmxCondition);
            RFmxResult RFmxRetrieveResultsSpec(eRfmx_Measurement_Type _eRfmx, Config RfmxCondition);


        }

        public interface iEqSG
        {
            void Initialize(string VSGname);
            string Model { get; }
            double CenterFrequency { get; set; }
            double Level { get; set; }
            double Scaling_Factor { get; set; }
            double ExternalGain { get; set; }
            string ModulationStd { get; set; }
            string WaveformName { get; set; }
            bool LOShare { get; set; }
            void SetLOshare(bool Flag);
            void ApplyChange();
            void SendSoftwareTrigger();
            void ExportSignal(byte markerNum, TriggerLine TrigLine);
            void Initiate();
            void SetLofreq(double LO_Freq);
            void Abort();
            double ReadTemp();
            void SelfCalibration(double startfreq, double stopfreq, double minrefpwr, double maxrefpwr);
        }

        public interface iEqSA
        {
            void Initialize(string VSAname);
            string Model { get; }
            double CenterFrequency { get; set; }
            double ReferenceLevel { get; set; }
            double ExternalGain { get; set; }
            string ModulationStd { get; set; }
            string WaveformName { get; set; }
            double SampleRate { get; set; }
            long NumberOfSamples { get; set; }
            bool LOShare { get; set; }
            void SetLOshare(bool Flag);
            TriggerLine TriggerIn { get; set; }
            TriggerLine TriggerOut { get; set; }
            double MeasureChanPower(bool byCalc = true);
            niComplexNumber[] MeasureIqTrace(bool Initiated);
            void MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace, ref byte site, int thisIteration = int.MinValue);
            void MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace, ref byte site, bool NR);
            void MeasureEVM(string EVMtype, out double EVMresult, int thisIteration = int.MinValue);
            void Initiate();
            void Abort(byte site);
            void ConfigureTrigger(string waveformname);
            double ReadTemp();
            void  SelfCalibration(double startfreq, double stopfreq, double minrefpwr, double maxrefpwr);
        }

        public interface iEqRFExtd
        {
            void Initialize(string VSGname);
            void ConfigureTXPort(int Port);
            void ConfigureTXInputFreq(double Freq);
            void ConfigureTXOutputFreq(double Freq);
            void ConfigureCalibrationTone(double OutFrequency, out double InFrequency);
            void ConfigureRXBypass(int Path);
            void ConfigureRXDownconversion();
            void ConfigureHarmonicConverter(double Fundamental, int HarmonicIndex, out double OutFrequency);
            double HMU_MeasureTemperature(out double Temperature);
            int Self_Test(out short TestResult, System.Text.StringBuilder TestMessage);
        }
    }

    public enum TriggerLine
    {
        None, PxiTrig0, PxiTrig1, PxiTrig2, PxiTrig3, PxiTrig4, PxiTrig5, PxiTrig6, PxiTrig7, FrontPanel0, FrontPanel1, FrontPanel2, FrontPanel3
    }

}
