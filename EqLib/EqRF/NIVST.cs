using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using ClothoLibAlgo;
using IqWaveform;
using NationalInstruments.RFmx.InstrMX;


namespace EqLib
{
    public partial class EqRF
    {
        public class NIVST : iEqRF
        {
            public iEqSG SG { get; set; }
            public iEqSA SA { get; set; }
            public iEqRFExtd RFExtd { get; set; }

            public IQ.Waveform ActiveWaveform { get; set; }
            public bool _Model;

            public string SerialNumber
            {
                get
                {
                    string sn = "";
                    SA_VST.RFSAsession.GetString(niRFSAProperties.SerialNumber, out sn);
                    return sn;
                }
            }

     		public string Model
            {
                get
                {
                    ModularInstrumentsSystem Modules = new ModularInstrumentsSystem();
                    foreach (DeviceInfo ModulesInfo in Modules.DeviceCollection)
                    {
                        if (ModulesInfo.Name == VisaAlias)
                        {
                            return ModulesInfo.Model;
                        }
                    }
                    return "NI PXIe-5646R";
                }
            }

            public bool IsVST1
            {
                get
                {
                    string model = "";
                    //NIVST_Rfmx.instrSession.GetInstrumentModel("", out model);

                    if (model == "NI PXIe-5646R") _Model = true;
                    else _Model = false;

                    return _Model;
                }
                set
                {
                    _Model = value;
                }
            }
            public string VisaAlias { get; set; }
            private byte _site;
            private double _MaxFreq = 6e3;
            public byte Site
            {
                get
                {
                    if (Eq.CurrentSplitTestPhase == SplitTestPhase.PhaseA)
                    {
                        return (byte)(_site + 1);
                    }
                    else
                    {
                        return _site;
                    }
                }
                set
                {
                    _site = value;
                }
            }
            public RFmxInstrMX InstrSession
            {
                get { return null; }
            }
            public RfmxAcp CRfmxAcp
            {
                get { return null; }
            }
            public RfmxAcpNR CRfmxAcpNR
            {
                get { return null; }
            }
            public  RfmxHar2nd CRfmxH2
            {
                get { return null; }
            }
            public RfmxHar3rd CRfmxH3
            {
                get { return null; }
            }
            public RfmxTxleakage CRfmxTxleakage
            {
                get { return null; }
            }
            public RfmxChp_For_Cal CRfmxCHP_FOR_CAL
            {
                get { return null; }
            }
            public RfmxIQ CRfmxIQ
            {
                get { return null; }
            }
            public RfmxChp CRfmxCHP
            {
                get { return null; }
            }
            public RfmxIQ_EVM CRfmxIQ_EVM
            {
                get { return null; }
            }
            public RfmxIQ_Timing CRfmxIQ_Timing
            {
                get { return null; }
            }
            public RfmxLTE_EVM CRfmxEVM_LTE
            {
                get { return null; }
            }
            public RfmxNR_EVM CRfmxEVM_NR
            {
                get { return null; }
            }
            public RfmxIIP3 CRfmxIIP3
            {
                get { return null; }
            }
            public niPowerServo NiPowerServo
            {
                get { return null; }
            }
            public ManualResetEvent[] ThreadFlags
            {
                get { return null; }
            }
            private PxiVstSg SG_VST;
            private PxiVstSa SA_VST;
            public double MaxFreq
            {
                get
                {
                    return _MaxFreq;
                }
                set
                {
                    _MaxFreq = value;
                }
            }
            private niPowerServo PowerServo;
            private int errorCode;
            private double[] MeasurementTimes;
            private double[] MeasurementLevels;

            private bool _servoEnabled;
            public bool ServoEnabled
            {
                get
                {
                    return _servoEnabled;
                }

                set
                {
                    if (value)
                    {
                        errorCode = PowerServo.Reset();
                        errorCode = PowerServo.Enable();
                    }
                    else
                    {
                        errorCode = PowerServo.Reset();
                        errorCode = PowerServo.Disable();
                    }

                    _servoEnabled = value;
                }
            }

            public NIRFMX RFMX;



            public void Initialize(Dictionary<byte, int[]> TriggerArray)
            {



                SG = new PxiVstSg(this);
                SA = new PxiVstSa(this);
                RFMX = new EqLib.NIRFMX();
                niPowerServo.ResetFPGA(VisaAlias, @"C:\Users\Public\Documents\National Instruments\FPGA Extensions Bitfiles\NI PXIe-5646R\NI Power Servoing for VST.lvbitx", true);
                //IntPtr niRfsaHandle = InitializeInstr(VisaAlias);
                IntPtr niRfsaHandle = RFMX.InitializeInstr(VisaAlias);

                //SA.Initialize(VisaAlias);
                SG.Initialize(VisaAlias);
                SG_VST = SG as PxiVstSg;
                SA_VST = SA as PxiVstSa;
                SA_VST.RFSAsession = new niRFSA(niRfsaHandle);
                SA_VST.RFSAsession.SetRefClockSource("", niRFSAConstants.PxiClk10Str);
                SA_VST.RFSAsession.ConfigureRefClock(niRFSAConstants.PxiClk10Str, 10e6);
                SA_VST.RFSAsession.SetNumberOfSamplesIsFinite(null, true);
                SA_VST.RFSAsession.SetNumberOfRecordsIsFinite(null, true);
                SA_VST.RFSAsession.SetNumberOfRecords(null, 1);


                SA_VST.TriggerIn = TriggerLine.PxiTrig0;
                SA_VST.TriggerOut = TriggerLine.PxiTrig1;
                PowerServo = new niPowerServo(SA_VST.RFSAsession, true);

                errorCode = PowerServo.DigitalGainStepLimitEnabled(false);
                // errorCode = PowerServo.ResetDigitalGainOnFailureEnabled(false);
                errorCode = PowerServo.FailServoOnDigitalSaturationEnabled(true);

                LoadWaveform("CW", "");
                LoadWaveform("CW_SWITCHINGTIME_OFF", "");
                LoadWaveform("CW_SWITCHINGTIME_ON", "");
                LoadWaveform("PINSWEEP", "");
                LoadWaveform("TWOTONE", "");
                //string rev = "";
                //SG.RFSGsession.GetString(niRFSGProperties.InstrumentModel, out model);
                //sg.RFSGsession.GetString(niRFSGProperties.SpecificDriverRevision, out rev);   // throws exception

                Eq.InstrumentInfo += string.Format("VST{0} = {1}*{2}; ", Site, Model, SerialNumber);
            }

            public void ConfigureServo(double targetOutputPower, double powerTolerance, double expectedGain, ushort minIterations, ushort maxIterations)
            {
                //PowerServo.DigitalGainStepLimitEnabled(VSTref, true, out errorCode);
                //PowerServo.ResetDigitalGainOnFailureEnabled(VSTref, true, out errorCode);

                bool turbo = false;

                double InitialAvgTime = ActiveWaveform.IntialServoMeasTime;
                double FinalAvgTime = ActiveWaveform.FinalServoMeasTime;

                if (turbo)
                {
                    minIterations = 2;
                    maxIterations = 10;

                    InitialAvgTime = 10e-6;
                    FinalAvgTime = 10e-6;
                }
                double SApeakLevDB, VSGanalogLevel, GainAccuracy;
                GainAccuracy = 3;

                // double a, b;
                //PowerServo.CalculateServoParameters(targetOutputPower, 15, 3, ActiveWaveform.PAR, out a, out b);

                errorCode = PowerServo.CalculateServoParameters(targetOutputPower, expectedGain + 4, GainAccuracy, ActiveWaveform.PAR, out SApeakLevDB, out VSGanalogLevel);
                //SA_VST.RFSAsession.SetReferenceLevel("", SApeakLevDB);
                SG_VST.RFSGsession.ConfigureRF(SA_VST.CenterFrequency, VSGanalogLevel + ActiveWaveform.PAR);
                errorCode = PowerServo.ConfigureActiveServoWindow(!turbo && ActiveWaveform.IsBursted, ActiveWaveform.ServoWindowLengthSec);    // If WindowEnabled = false, the servo does not use any triggering
                errorCode = PowerServo.ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride.Arm_Ref_Trig_On_Servo_Done);
                errorCode = PowerServo.Setup(powerTolerance, InitialAvgTime, FinalAvgTime, minIterations, maxIterations, false, 0);

                SA.Initiate();
                SG.Initiate();
            }
            public void Configure_Servo(Config o)
            {
                //PowerServo.DigitalGainStepLimitEnabled(VSTref, true, out errorCode);
                //PowerServo.ResetDigitalGainOnFailureEnabled(VSTref, true, out errorCode);

                bool turbo = false;

                double InitialAvgTime = ActiveWaveform.IntialServoMeasTime;
                double FinalAvgTime = ActiveWaveform.FinalServoMeasTime;
                ushort minIterations, maxIterations;
                if (turbo)
                {
                    minIterations = 2;
                    maxIterations = 10;

                    InitialAvgTime = 10e-6;
                    FinalAvgTime = 10e-6;
                }
                else
                {
                    minIterations = 4;
                    maxIterations = 12;
                }
                double SApeakLevDB, VSGanalogLevel, GainAccuracy;
                GainAccuracy = 3;

                // double a, b;
                //PowerServo.CalculateServoParameters(targetOutputPower, 15, 3, ActiveWaveform.PAR, out a, out b);
                errorCode = PowerServo.CalculateServoParameters(o.TargetPout, o.ExpectedGain, GainAccuracy, ActiveWaveform.PAR, out SApeakLevDB, out VSGanalogLevel);
                if (o.manualSetVSGanalogLev != 0)
                {
                    VSGanalogLevel = o.manualSetVSGanalogLev + (GainAccuracy + 1);
                }
                SA_VST.RFSAsession.SetReferenceLevel("", SApeakLevDB);
                SG_VST.RFSGsession.ConfigureRF(SA_VST.CenterFrequency, VSGanalogLevel + ActiveWaveform.PAR);
                errorCode = PowerServo.ConfigureActiveServoWindow(!turbo && ActiveWaveform.IsBursted, ActiveWaveform.ServoWindowLengthSec);    // If WindowEnabled = false, the servo does not use any triggering
                errorCode = PowerServo.ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride.Arm_Ref_Trig_On_Servo_Done);
                errorCode = PowerServo.Setup(o.powerTolerance, InitialAvgTime, FinalAvgTime, minIterations, maxIterations, false, 0);

                SA.Initiate();
                SG.Initiate();
            }
            public bool Servo(out double Pout, out double Pin, double LossPout)
            {
                double TimeOut = 10;
                double RawDigitalGain;
                double DigitalGaindB;
                bool Done = false;
                bool Failed = false;
                ushort NumAveragesCaptured;

                errorCode = PowerServo.Start();
                errorCode = PowerServo.Wait(TimeOut, out NumAveragesCaptured, out Done, out Failed);
                errorCode = PowerServo.GetServoSteps(NumAveragesCaptured, false, false, 0, out MeasurementTimes, out MeasurementLevels);
                errorCode = PowerServo.GetDigitalGain(out RawDigitalGain, out DigitalGaindB);

                //this.Print_Servo_Data(Failed, RawDigitalGain, DigitalGaindB);

                Pout = MeasurementLevels[MeasurementLevels.Length - 1];
                Pin = SG.Level + DigitalGaindB;

                return !Failed;
            }

            public void Configure_Servo_Timing(Config o)
            {
                //PowerServo.DigitalGainStepLimitEnabled(VSTref, true, out errorCode);
                //PowerServo.ResetDigitalGainOnFailureEnabled(VSTref, true, out errorCode);

                bool turbo = false;

                double InitialAvgTime = ActiveWaveform.IntialServoMeasTime;
                double FinalAvgTime = ActiveWaveform.FinalServoMeasTime;
                ushort minIterations, maxIterations;
                if (turbo)
                {
                    minIterations = 2;
                    maxIterations = 10;

                    InitialAvgTime = 10e-6;
                    FinalAvgTime = 10e-6;
                }
                else
                {
                    minIterations = 4;
                    maxIterations = 12;
                }
                double SApeakLevDB, VSGanalogLevel, GainAccuracy;
                GainAccuracy = 3;

                // double a, b;
                //PowerServo.CalculateServoParameters(targetOutputPower, 15, 3, ActiveWaveform.PAR, out a, out b);
                errorCode = PowerServo.CalculateServoParameters(o.TargetPout, o.ExpectedGain, GainAccuracy, ActiveWaveform.PAR, out SApeakLevDB, out VSGanalogLevel);
                if (o.manualSetVSGanalogLev != 0)
                {
                    VSGanalogLevel = o.manualSetVSGanalogLev + (GainAccuracy + 1);
                }
                SA_VST.RFSAsession.SetReferenceLevel("", SApeakLevDB);
                SG_VST.RFSGsession.ConfigureRF(SA_VST.CenterFrequency, VSGanalogLevel + ActiveWaveform.PAR);
                errorCode = PowerServo.ConfigureActiveServoWindow(!turbo && ActiveWaveform.IsBursted, ActiveWaveform.ServoWindowLengthSec);    // If WindowEnabled = false, the servo does not use any triggering
                errorCode = PowerServo.ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride.Arm_Ref_Trig_On_Servo_Done);
                errorCode = PowerServo.Setup(o.powerTolerance, InitialAvgTime, FinalAvgTime, minIterations, maxIterations, false, 0);

                SA.Initiate();
                SG.Initiate();
            }

            public bool Servo_Timing(out double Pout, out double Pin, double LossPout)
            {
                double TimeOut = 10;
                double RawDigitalGain;
                double DigitalGaindB;
                bool Done = false;
                bool Failed = false;
                ushort NumAveragesCaptured;

                errorCode = PowerServo.Start();
                errorCode = PowerServo.Wait(TimeOut, out NumAveragesCaptured, out Done, out Failed);
                errorCode = PowerServo.GetServoSteps(NumAveragesCaptured, false, false, 0, out MeasurementTimes, out MeasurementLevels);
                errorCode = PowerServo.GetDigitalGain(out RawDigitalGain, out DigitalGaindB);

                //this.Print_Servo_Data(Failed, RawDigitalGain, DigitalGaindB);

                Pout = MeasurementLevels[MeasurementLevels.Length - 1];
                Pin = SG.Level + DigitalGaindB;

                return !Failed;
            }
            public void Configure_CHP(Config o)
            {

            }
            public void Measure_CHP()
            {

            }
            public void Measure_CHP(double SOAK_Delay)
            {

            }
            public void Configure_IQ(Config_IQ o)
            {

            }
            public void Measure_IQ()
            {

            }

            public void Configure_IQ_EVM(Config_IQ_EVM o)
            {

            }
            public void Measure_IQ_EVM()
            {

            }

            public void Configure_IIP3(Config_IIP3 o)
            {

            }
            public void Measure_IIP3()
            {

            }


            public void Configure_Timing(Config_Timing o)
            {


            }
            public void Measure_Timing()
            {

            }

            public void SetActiveWaveform(string ModStd, string WvfrmName, bool useServoScript, bool setSG = true)
            {

                ModStd = ModStd.ToUpper();
                WvfrmName = WvfrmName.ToUpper();

                if (!IQ.Mem.ContainsKey(ModStd + WvfrmName))
                {
                    MessageBox.Show("Requested waveform:\n" + ModStd + ", " + WvfrmName + "\n\nIs not yet supported.",
                        "Waveform Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                IQ.Waveform newWaveform = IQ.Mem[ModStd + WvfrmName];

                if (setSG && useServoScript) SG_VST.RFSGsession.SetSelectedScript("", newWaveform.ScriptNameServo);

                if (ActiveWaveform == null || ActiveWaveform.FullName != newWaveform.FullName)
                {
                    if (setSG)
                    {
                        if (!useServoScript) SG_VST.RFSGsession.SetSelectedScript("", newWaveform.ScriptName);
                        SG_VST.RFSGsession.SetIqRate("", (double)newWaveform.VsgIQrate);
                    }

                }
                SA.SampleRate = (double)newWaveform.VsaIQrate;
                SA.NumberOfSamples = newWaveform.SamplesPerRecord;
                ActiveWaveform = newWaveform;
            }

            private List<string> loadedWaveforms = new List<string>();

            public bool LoadWaveform(string ModStd, string WvfrmName)
            {
                try
                {
                    if ((ModStd == null | WvfrmName == null) || (ModStd == "" & WvfrmName == "")) return true;

                    if (!IQ.Load(ModStd, WvfrmName, false)) return false;

                    if (!loadedWaveforms.Contains(IQ.Mem[ModStd + WvfrmName].FullName))
                    {
                        loadedWaveforms.Add(IQ.Mem[ModStd + WvfrmName].FullName);

                        SG_VST.RFSGsession.WriteArbWaveform(IQ.Mem[ModStd + WvfrmName].FullName, IQ.Mem[ModStd + WvfrmName].Idata.Length, IQ.Mem[ModStd + WvfrmName].Idata, IQ.Mem[ModStd + WvfrmName].Qdata, false);
                        SG_VST.RFSGsession.WriteScript(IQ.Mem[ModStd + WvfrmName].Script);

                        SG_VST.RFSGsession.WriteArbWaveform(IQ.Mem[ModStd + WvfrmName].FullNameServo, IQ.Mem[ModStd + WvfrmName].IdataServo.Length, IQ.Mem[ModStd + WvfrmName].IdataServo, IQ.Mem[ModStd + WvfrmName].QdataServo, false);
                        SG_VST.RFSGsession.WriteScript(IQ.Mem[ModStd + WvfrmName].ScriptServo);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return false;
                }
            }

            public void RFmxConfigureSpec(eRfmx _eRfmx, RFmxCofig c)
            {
                switch (_eRfmx)
                {
                    case eRfmx.eRfmxAcp: RFMX.cRfmxAcp.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.TestAcp, c.NumberOfOffsets, c.Rbw); break;
                    case eRfmx.eRfmxIIP3: RFMX.cRfmxIIP3.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel); break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }
            }

            public void RFmxCommitSpec(eRfmx _eRfmx, RFmxCofig c)
            {
                switch (_eRfmx)
                {
                    case eRfmx.eRfmxAcp: RFMX.cRfmxAcp.CommitSpec(c.Iteration); break;
                    case eRfmx.eRfmxIIP3: RFMX.cRfmxIIP3.CommitSpec(c.Iteration); break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }
            }

            public void RFmxInitiateSpec(eRfmx _eRfmx, RFmxCofig c)
            {
                switch (_eRfmx)
                {
                    case eRfmx.eRfmxAcp: RFMX.cRfmxAcp.InitiateSpec(c.Iteration); break;
                    case eRfmx.eRfmxIIP3: RFMX.cRfmxIIP3.InitiateSpec(c.Iteration); break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }
            }

            public RFmxResult RFmxRetrieveResultsSpec(eRfmx _eRfmx, RFmxCofig c)
            {
                RFmxResult _RFmxResult = new EqLib.EqRF.RFmxResult();

                switch (_eRfmx)
                {
                    case eRfmx.eRfmxAcp:
                        RFMX.cRfmxAcp.RetrieveResults(c.Iteration);
                        break;

                    case eRfmx.eRfmxIIP3:

                        double lowerTonePower;
                        double upperTonePower;
                        int[] intermodOrder = new int[1];
                        double[] lowerIntermodPower = new double[1];
                        double[] upperIntermodPower = new double[1];

                        RFMX.cRfmxIIP3.RetrieveResults(c.Iteration, out lowerTonePower, out upperTonePower, ref lowerIntermodPower, ref upperIntermodPower, ref intermodOrder);
                        _RFmxResult = new EqLib.EqRF.RFmxResult(lowerTonePower, upperTonePower, lowerIntermodPower, upperIntermodPower, intermodOrder);
                        break;

                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }

                return _RFmxResult;
            }


            public void RFmxConfigureSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {

            }

            public void RFmxCommitSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {

            }

            public void RFmxInitiateSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {

            }

            public RFmxResult RFmxRetrieveResultsSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {

                return null;
            }

            public void Configure_IIP3_RFSG_Parameters(double LossPin, double RFfrequency, float TargetPin)
            {
                SG_VST.RFSGsession.SetSelectedScript("", IQ.Mem["TWOTONE"].ScriptName);
                SG_VST.RFSGsession.SetIqRate("", (double)IQ.Mem["TWOTONE"].VsgIQrate);
                SG_VST.Level = Convert.ToSingle(TargetPin + 3);
                SG_VST.RFSGsession.SetExternalGain("", (LossPin));
                SG_VST.RFSGsession.SetArbPreFilterGain("", -5);
                SG_VST.RFSGsession.SetUpconverterCenterFrequency("", (RFfrequency + 20) * 1e6);
                SG_VST.RFSGsession.SetFrequency("", (RFfrequency) * 1e6);
                SG_VST.RFSGsession.Initiate();
            }

            public void ResetDriver()
            {
                RFMX.ResetDriver();
            }
            public void ResetRFSA(bool Enable)
            {
                SA_VST.RFSAsession.reset();
                SA_VST.RFSAsession.SetRefClockSource("", niRFSAConstants.PxiClk10Str);
                SA_VST.RFSAsession.ConfigureRefClock(niRFSAConstants.PxiClk10Str, 10e6);
                SA_VST.RFSAsession.SetNumberOfSamplesIsFinite(null, true);
                SA_VST.RFSAsession.SetNumberOfRecordsIsFinite(null, true);
                SA_VST.RFSAsession.SetNumberOfRecords(null, 1);
                SA_VST.TriggerIn = TriggerLine.PxiTrig0;
                SA_VST.TriggerOut = TriggerLine.PxiTrig1;
                PowerServo = new niPowerServo(SA_VST.RFSAsession, true);

                errorCode = PowerServo.DigitalGainStepLimitEnabled(false);
                errorCode = PowerServo.ResetDigitalGainOnFailureEnabled(false);
                errorCode = PowerServo.FailServoOnDigitalSaturationEnabled(true);
            }
            public void close()
            {
                RFMX.close();
                SA_VST.RFSAsession.Close();
            }
            private void Print_Servo_Data(bool Failed, double RawDigitalGain, double DigitalGaindB)
            {
                Console.WriteLine();
                Console.WriteLine(ActiveWaveform.ModulationStd + " " + ActiveWaveform.WaveformName);
                Console.WriteLine("Time\t\tServo Level (dBm)");
                for (int i = 0; i < MeasurementTimes.Length; i++)
                {
                    Console.WriteLine("{0:0.0000}\t\t{1:00.000}", MeasurementTimes[i], MeasurementLevels[i]);
                }
                Console.WriteLine("Raw Gain: {0:0.000}", RawDigitalGain);
                Console.WriteLine("Gain dB: {0:0.000}", DigitalGaindB);

                if (Failed) Console.WriteLine("Servo Failed");
                else Console.WriteLine("Servo Passed");
            }

            public class PxiVstSg : iEqSG
            {
                public NIVST vst { get; set; }
                public bool LOShare { get; set; }
                private double _Scaling_Factor;
                public double Scaling_Factor
                {
                    get
                    {
                        return _Scaling_Factor;
                    }
                    set
                    {
                        _Scaling_Factor = value;
                    }
                }
                internal niRFSG RFSGsession;
                private double _rfFrequency, _powerLevel, _externalGain;
                private string _ModulationStd;
                private string _WaveformName;

                public PxiVstSg(NIVST vst)
                {
                    this.vst = vst;
                }

                public void Initialize(string VSGname)
                {
                    if (RFSGsession == null)
                    {
                        RFSGsession = new niRFSG(VSGname, true, false, "DriverSetup=Bitfile:NI Power Servoing for VST.lvbitx");
                    }
                    RFSGsession.ConfigureRefClock(niRFSGConstants.PxiClkStr, 10e6);
                    RFSGsession.ConfigureGenerationMode(niRFSGConstants.Script);
                    RFSGsession.ConfigurePowerLevelType(niRFSGConstants.PeakPower);

                    ExportSignal(0, TriggerLine.PxiTrig0);
                    ExportSignal(1, TriggerLine.FrontPanel0);
                }
                public void ApplyChange()
                { }
                public string Model
                {
                    get
                    {
                        string model = "";
                        RFSGsession.GetString(niRFSGProperties.InstrumentModel, out model);
                        return model;
                    }
                }
                public void SetLOshare(bool Flag)
                {
      
                }
                public double Level
                {
                    get
                    {
                        return _powerLevel;
                    }
                    set
                    {
                        RFSGsession.SetDouble(niRFSGProperties.PowerLevel, value + vst.ActiveWaveform.PAR);
                        _powerLevel = value;
                    }
                }
                public double ReadTemp()
                {
                    double temp;
                    RFSGsession.GetDeviceTemperature("", out temp);
                    return temp;
                }
                public double ExternalGain
                {
                    get
                    {
                        return _externalGain;
                    }
                    set
                    {
                        RFSGsession.SetExternalGain("", value);
                        _externalGain = value;
                    }
                }

                public string ModulationStd
                {
                    get
                    {
                        return _ModulationStd;
                    }
                    set
                    {
                        _ModulationStd = value;
                    }
                }

                public string WaveformName
                {
                    get
                    {
                        return _WaveformName;
                    }
                    set
                    {
                        _WaveformName = value;
                    }
                }

                public double CenterFrequency
                {
                    get
                    {
                        return _rfFrequency;
                    }
                    set
                    {
                        RFSGsession.SetUpconverterCenterFrequency("", value + 60e6);
                        RFSGsession.SetFrequency("", value);
                        _rfFrequency = value;
                    }
                }

                public void SendSoftwareTrigger()
                {
                    RFSGsession.SendSoftwareEdgeTrigger(niRFSGConstants.ScriptTrigger, "scriptTrigger0");
                }

                public void ExportSignal(byte markerNum, TriggerLine TrigLine)
                {
                    string MarkerEventStr =
                        markerNum == 0 ? niRFSGConstants.Marker0EventStr :
                        markerNum == 1 ? niRFSGConstants.Marker1EventStr :
                        markerNum == 2 ? niRFSGConstants.Marker2EventStr :
                        markerNum == 3 ? niRFSGConstants.Marker3EventStr :
                        niRFSGConstants.Marker0EventStr;

                    RFSGsession.ExportSignal(niRFSGConstants.MarkerEvent, MarkerEventStr, TranslateTriggerLine(TrigLine));  // cannot have marker event exported to PFI0, since PFI0 must now be configured as an input from HSDIO.
                }

                public void Initiate()
                {
                    RFSGsession.Initiate();
                }
                public void SelfCalibration(double startfreq, double stopfreq, double minrefpwr, double maxrefpwr)
                {
                    RFSGsession.SelfCalibrateRange(0, startfreq, stopfreq, minrefpwr, maxrefpwr);
                }

                public void SetLofreq(double LO_Freq)
                {

                }

                public void Abort()
                {
                    RFSGsession.Abort();
                }

                private string TranslateTriggerLine(TriggerLine trigLine)
                {
                    switch (trigLine)
                    {
                        case TriggerLine.None:
                            return niRFSGConstants.DoNotExportStr;

                        case TriggerLine.FrontPanel0:
                            return niRFSGConstants.Pfi0Str;

                        case TriggerLine.FrontPanel1:
                            return niRFSGConstants.Pfi1Str;

                        case TriggerLine.FrontPanel2:
                            return niRFSGConstants.Pfi2Str;

                        case TriggerLine.FrontPanel3:
                            return niRFSGConstants.Pfi3Str;

                        case TriggerLine.PxiTrig0:
                            return niRFSGConstants.PxiTrig0Str;

                        case TriggerLine.PxiTrig1:
                            return niRFSGConstants.PxiTrig1Str;

                        case TriggerLine.PxiTrig2:
                            return niRFSGConstants.PxiTrig2Str;

                        case TriggerLine.PxiTrig3:
                            return niRFSGConstants.PxiTrig3Str;

                        case TriggerLine.PxiTrig4:
                            return niRFSGConstants.PxiTrig4Str;

                        case TriggerLine.PxiTrig5:
                            return niRFSGConstants.PxiTrig5Str;

                        case TriggerLine.PxiTrig6:
                            return niRFSGConstants.PxiTrig6Str;

                        case TriggerLine.PxiTrig7:
                            return niRFSGConstants.PxiTrig7Str;

                        default:
                            throw new Exception("NI SG trigger line not supported");
                    }
                }
            }

            public class PxiVstSa : iEqSA
            {
                public NIVST vst { get; set; }
                public bool LOShare { get; set; }
                internal niRFSA RFSAsession;
                private double _rfFrequency, _refLevel, _externalGain, _sampleRate;
                private string _ModulationStd;
                private string _WaveformName;
                private long _numberOfSamples;
                private TriggerLine _triggerIn, _triggerOut;

                public PxiVstSa(NIVST vst)
                {
                    this.vst = vst;
                }

                public void Initialize(string VSAname)
                {
                    if (RFSAsession == null)
                    {
                        RFSAsession = new niRFSA(VSAname, true, false, "DriverSetup=Bitfile:NI Power Servoing for VST.lvbitx");
                    }
                    RFSAsession.ConfigureRefClock(niRFSAConstants.PxiClk10Str, 10e6);
                    RFSAsession.SetNumberOfSamplesIsFinite(null, true);
                    RFSAsession.SetNumberOfRecordsIsFinite(null, true);
                    RFSAsession.SetNumberOfRecords(null, 1);
                    TriggerIn = TriggerLine.PxiTrig0;
                    TriggerOut = TriggerLine.PxiTrig1;
                }

                public string Model
                {
                    get
                    {
                        string model = "";
                        RFSAsession.GetString(niRFSAProperties.InstrumentModel, out model);
                        return model;
                    }
                }

                public void SetLOshare(bool Flag)
                {



                }
                public double CenterFrequency
                {
                    get
                    {
                        return _rfFrequency;
                    }
                    set
                    {
                        RFSAsession.SetIqCarrierFrequency("", value);
                        _rfFrequency = value;
                    }
                }

                public double ReferenceLevel
                {
                    get
                    {
                        return _refLevel;
                    }
                    set
                    {
                        RFSAsession.SetReferenceLevel("", value);
                        _refLevel = value;
                    }
                }

                public double ExternalGain
                {
                    get
                    {
                        return _externalGain;
                    }
                    set
                    {
                        RFSAsession.SetExternalGain(null, value);
                        _externalGain = value;
                    }
                }

                public string ModulationStd
                {
                    get
                    {
                        return _ModulationStd;
                    }
                    set
                    {
                        _ModulationStd = value;
                    }
                }

                public string WaveformName
                {
                    get
                    {
                        return _WaveformName;
                    }
                    set
                    {
                        _WaveformName = value;
                    }
                }

                public void ConfigureTrigger(string waveformName) { }
                public double SampleRate
                {
                    get
                    {
                        return _sampleRate;
                    }
                    set
                    {
                        RFSAsession.SetIqRate(null, value);
                        _sampleRate = value;
                    }
                }

                public long NumberOfSamples
                {
                    get
                    {
                        return _numberOfSamples;
                    }
                    set
                    {
                        RFSAsession.SetNumberOfSamples(null, value);
                        _numberOfSamples = value;
                    }
                }

                public double ReadTemp()
                {
                    double Temp;
                    RFSAsession.GetDeviceTemperature("", out Temp);
                    return Temp;
                }

                public TriggerLine TriggerIn
                {
                    get
                    {
                        return _triggerIn;
                    }
                    set
                    {
                        string niTrigLine = TranslateTriggerLine(value);
                        RFSAsession.ConfigureDigitalEdgeRefTrigger(niTrigLine, niRFSAConstants.RisingEdge, 0);
                        _triggerIn = value;
                    }
                }

                public TriggerLine TriggerOut
                {
                    get
                    {
                        return _triggerOut;
                    }
                    set
                    {
                        string niTrigLine = TranslateTriggerLine(value);
                        RFSAsession.ExportSignal(niRFSAConstants.RefTrigger, "", niTrigLine);
                        _triggerOut = value;
                    }
                }

                public double MeasureChanPower(bool byCalc = true)
                {
                    if (byCalc)
                    {
                        niComplexNumber[] Data = MeasureIqTrace(false);

                        SpectralAnalysis sa = new SpectralAnalysis(vst.ActiveWaveform, Data);

                        double chPow = sa.GetCenterChannelPower(3);

                        return chPow;
                    }
                    else
                    {
                        RFSAsession.Abort();
                        RFSAsession.ConfigureAcquisitionType(niRFSAConstants.Spectrum);

                        RFSAsession.ConfigureSpectrumFrequencyCenterSpan("", vst.SA.CenterFrequency, vst.ActiveWaveform.RefChBW * 3.0);

                        TriggerOut = TriggerLine.None;

                        RFSAsession.Commit();

                        int numSpectrumLines = 0;
                        RFSAsession.GetNumberOfSpectralLines("", out numSpectrumLines);

                        double[] powerSpectrum = new double[numSpectrumLines];
                        niRFSA_spectrumInfo spectruminfo;
                        RFSAsession.ReadPowerSpectrumF64("", 10, powerSpectrum, numSpectrumLines, out spectruminfo);

                        //for (int i = 0; i < powerSpectrum.Length; i++)
                        //{
                        //    double freq = spectruminfo.initialFrequency + spectruminfo.frequencyIncrement * i;
                        //    Console.WriteLine(freq + "\t" + powerSpectrum[i]);
                        //}

                        double totalPower = 0;

                        for (int i = 0; i < powerSpectrum.Length; i++)
                        {
                            totalPower += Math.Pow(10.0, powerSpectrum[i]);
                        }

                        totalPower = Math.Log10(totalPower);


                        RFSAsession.ConfigureAcquisitionType(niRFSAConstants.Iq);
                        TriggerOut = TriggerLine.PxiTrig1;
                        return totalPower;
                    }
                }

                public niComplexNumber[] MeasureIqTrace(bool Initiated)
                {
                    niComplexNumber[] Data = new niComplexNumber[NumberOfSamples];
                    niRFSA_wfmInfo wfmInfo;

                    int error = 0;

                    try
                    {
                        if (Initiated)
                        {
                            error = RFSAsession.FetchIQSingleRecordComplexF64("", 0, NumberOfSamples, 10, Data, out wfmInfo);
                        }
                        else
                        {
                            error = RFSAsession.ReadIQSingleRecordComplexF64("", 10, Data, NumberOfSamples, out wfmInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("RF trace measurement timed out.\n\n" + e.ToString());
                    }

                    RFSAsession.Abort();

                    return Data;
                }

                public void MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace, ref byte site, int thisIteration = int.MinValue)
                {
                    iqTrace = MeasureIqTrace(true);
                }
                public void MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace, ref byte site, bool NR)
                {
                    iqTrace = MeasureIqTrace(true);
                }
                public void MeasureEVM(string EVMtype, out double EVMresult, int thisIteration = int.MinValue)
                {
                    EVMresult = 99999;
                }
                public void Initiate()
                {
                    RFSAsession.Initiate();
                }
                public void SelfCalibration(double startfreq, double stopfreq, double minrefpwr, double maxrefpwr)
                {
                    RFSAsession.SelfCalibrateRange(0, startfreq, stopfreq, minrefpwr, maxrefpwr);
                }
                public void Abort(byte site)
                {
                    RFSAsession.Abort();
                }

                private string TranslateTriggerLine(TriggerLine trigLine)
                {
                    switch (trigLine)
                    {
                        case TriggerLine.None:
                            return niRFSAConstants.DoNotExportStr;

                        case TriggerLine.FrontPanel0:
                            return niRFSAConstants.Pfi0Str;

                        case TriggerLine.FrontPanel1:
                            return niRFSAConstants.Pfi1Str;

                        case TriggerLine.PxiTrig0:
                            return niRFSAConstants.PxiTrig0Str;

                        case TriggerLine.PxiTrig1:
                            return niRFSAConstants.PxiTrig1Str;

                        case TriggerLine.PxiTrig2:
                            return niRFSAConstants.PxiTrig2Str;

                        case TriggerLine.PxiTrig3:
                            return niRFSAConstants.PxiTrig3Str;

                        case TriggerLine.PxiTrig4:
                            return niRFSAConstants.PxiTrig4Str;

                        case TriggerLine.PxiTrig5:
                            return niRFSAConstants.PxiTrig5Str;

                        case TriggerLine.PxiTrig6:
                            return niRFSAConstants.PxiTrig6Str;

                        case TriggerLine.PxiTrig7:
                            return niRFSAConstants.PxiTrig7Str;

                        default:
                            throw new Exception("NI SA trigger line not supported");

                    }
                }
            }
        }
    }
}
