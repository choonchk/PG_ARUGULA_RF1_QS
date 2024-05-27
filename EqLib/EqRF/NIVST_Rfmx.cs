using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MPAD_TestTimer;
using System.Windows.Forms;
using Ivi.Visa.Interop;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;
using NationalInstruments.RFmx.NRMX;
using NationalInstruments.RFmx.LteMX;
using ClothoLibAlgo;
using IqWaveform;

using NationalInstruments;
using NationalInstruments.SystemConfiguration;
using SignalCraftTechnologies.ModularInstruments.Interop;
using ProductionLib;

namespace EqLib
{
    public partial class EqRF
    {
        static bool MXACAL = false;
        static double Timeout = 10;
        public class NIVST_Rfmx : iEqRF
        {
            public HiPerfTimer uTimer = new HiPerfTimer();
            public iEqSG SG { get; set; }
            public iEqSA SA { get; set; }
            public iEqRFExtd RFExtd { get; set; }

            public IQ.Waveform ActiveWaveform { get; set; }
 
            public string VisaAlias { get; set; }
            private byte _site;
            private double _MaxFreq = 6e3;
            public bool _Model;

            public string SerialNumber
            {
                get
                {
                    ModularInstrumentsSystem Modules = new ModularInstrumentsSystem();
                    foreach (DeviceInfo ModulesInfo in Modules.DeviceCollection)
                    {
                        if (ModulesInfo.Name == VisaAlias)
                        {
                            return ModulesInfo.SerialNumber;
                        }
                    }
                    return "NA";
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
                    instrSession.GetInstrumentModel("", out model);

                    if (model == "NI PXIe-5646R") _Model = true;
                    else _Model = false;


                    return _Model;
                }
                set
                {

                }
            }
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
            private PxiVstSg SG_VST;
            private PxiVstSa SA_VST;
            private si2250 RFExtdsession;

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
            private  niPowerServo powerServo;
            public niPowerServo NiPowerServo
            {
                get { return powerServo; }
            }
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
                        errorCode = powerServo.Reset();
                        errorCode = powerServo.Enable();
                    }
                    else
                    {
                        errorCode = powerServo.Reset();
                        errorCode = powerServo.Disable();
                    }

                    _servoEnabled = value;
                }
            }

            public NIRFMX RFMX;
            private RFmxInstrMX instrSession;            
            public RFmxInstrMX InstrSession
            {
                get { return instrSession; }
            }

            private RfmxAcp cRfmxAcp;
            public RfmxAcp CRfmxAcp
            {
                get { return cRfmxAcp; }
            }
            private RfmxAcpNR cRfmxAcpNR;
            public RfmxAcpNR CRfmxAcpNR
            {
                get { return cRfmxAcpNR; }
            }
            private RfmxHar2nd cRfmxH2;
            public RfmxHar2nd CRfmxH2
            {
                get { return cRfmxH2; }
            }
            private RfmxHar3rd cRfmxH3;
            public RfmxHar3rd CRfmxH3
            {
                get { return cRfmxH3; }
            }
            private RfmxTxleakage cRfmxTxleakage;
            public RfmxTxleakage CRfmxTxleakage
            {
                get { return cRfmxTxleakage; }
            }
            private RfmxIQ cRfmxIQ;
            public RfmxIQ CRfmxIQ
            {
                get { return cRfmxIQ; }
            }
            private RfmxIQ_EVM cRfmxIQ_EVM;
            public RfmxIQ_EVM CRfmxIQ_EVM
            {
                get { return cRfmxIQ_EVM; }
            }
            private RfmxIQ_Timing cRfmxIQ_Timing;
            public RfmxIQ_Timing CRfmxIQ_Timing
            {
                get { return cRfmxIQ_Timing; }
            }
            private RfmxIIP3 cRfmxIIP3;
            public RfmxIIP3 CRfmxIIP3
            {
                get { return cRfmxIIP3; }
            }
            private RfmxChp cRfmxCHP;
            public RfmxChp CRfmxCHP
            {
                get { return cRfmxCHP; }
            }
            private RfmxChp_For_Cal cRfmxCHP_FOR_CAL;
            public RfmxChp_For_Cal CRfmxCHP_FOR_CAL
            {
                get { return cRfmxCHP_FOR_CAL; }
            }
            private RfmxLTE_EVM cRfmxEVM_LTE;
            public RfmxLTE_EVM CRfmxEVM_LTE
            {
                get { return cRfmxEVM_LTE; }
            }
            private RfmxNR_EVM cRfmxEVM_NR;
            public RfmxNR_EVM CRfmxEVM_NR
            {
                get { return cRfmxEVM_NR; }
            }

            private ManualResetEvent[] threadFlags;
            public ManualResetEvent[] ThreadFlags
            {
                get { return threadFlags; }
            }
            public double VSGanalogLevel;
            public static readonly bool RFmxFlagOn = true;
            public static bool NR = false;
            public static Dictionary<byte, int[]> siteTrigArray;
            public static bool MXAInit = false;
            System.Text.StringBuilder ctestmessage = new System.Text.StringBuilder(512);

            public NIVST_Rfmx()
            {
                //cRfmxAcp = new RfmxAcp();
            }

            public void Initialize(Dictionary<byte, int[]> TriggerArray)
            {
                siteTrigArray = TriggerArray;
                short chmu = 0;

                SG = new PxiVstSg(this);
                SA = new PxiVstSa(this);
                RFExtd = new PxiRFExtd(this); //Added by Hosein

                SA.Initialize(VisaAlias);
                SG.Initialize(VisaAlias);
                RFExtd.Initialize("SC2250_"+ Site); //Added by Hosein
                RFExtd.Self_Test(out chmu, ctestmessage); //HMU self-Test added by Mario

                if (MXACAL && !MXAInit)
                {
                    Dictionary<string, string> selectionList = new Dictionary<string, string>();
                    selectionList.Add("Yes", "Yes");
                    selectionList.Add("No", "No");
                    string cnt_str = PromptManager.Instance.ShowMultiSelectionDialog("Do you want to perform Cable Calibration with MXA?", "Cable Calibration", selectionList, "No");

                    if (cnt_str.ToUpper() == "YES")
                    {
                        FormattedIO488 myVisaSa = EqLib.EquipSA.OpenIO("MXA"); //Added by Hosein
                        Eq.EqMXA = new EquipSA(myVisaSa); //Added by Hosein
                        Eq.EqMXA.INITIALIZATION(1); //Added by Hosein

                        MXAInit = true;
                    }
                }
                
                SG_VST = SG as PxiVstSg;
                SA_VST = SA as PxiVstSa;

                //Quadsite
                TriggerLine trigInTerminal = (TriggerLine)siteTrigArray[Site][0];
                TriggerLine trigOutTerminal = (TriggerLine)siteTrigArray[Site][1];

                SA_VST.TriggerIn = trigInTerminal;
                SA_VST.TriggerOut = trigOutTerminal;

                powerServo = new niPowerServo(SA_VST.RFSAsession, true);


                errorCode = powerServo.DigitalGainStepLimitEnabled(false);
               // errorCode = PowerServo.ResetDigitalGainOnFailureEnabled(false);
                errorCode = powerServo.FailServoOnDigitalSaturationEnabled(true);

                LoadWaveform("CW", "");
                LoadWaveform("CW_SWITCHINGTIME_OFF", "");
                LoadWaveform("CW_SWITCHINGTIME_ON", "");
                LoadWaveform("PINSWEEP", "");
                LoadWaveform("TWOTONE", "");
                //string rev = "";
                //SG.RFSGsession.GetString(niRFSGProperties.InstrumentModel, out model);
                //sg.RFSGsession.GetString(niRFSGProperties.SpecificDriverRevision, out rev);   // throws exception
                Eq.InstrumentInfo += string.Format("VST{0} = {1}*{2}; ", Site, Model, SerialNumber);

                cRfmxAcp = new RfmxAcp(Site);
                cRfmxAcpNR = new RfmxAcpNR(Site);
                cRfmxH2 = new RfmxHar2nd(Site);
                cRfmxH3 = new RfmxHar3rd(Site);
                cRfmxCHP = new RfmxChp(Site);
                cRfmxCHP_FOR_CAL = new RfmxChp_For_Cal(Site);
                cRfmxIQ = new RfmxIQ(Site);
                cRfmxIQ_EVM = new RfmxIQ_EVM(Site);
                cRfmxIQ_Timing = new RfmxIQ_Timing(Site);
                cRfmxIIP3 = new RfmxIIP3(Site);
                cRfmxTxleakage = new RfmxTxleakage(Site);

                cRfmxEVM_LTE = new RfmxLTE_EVM(Site);
                cRfmxEVM_NR = new RfmxNR_EVM(Site);
#if !DEBUG
                ConfigureDebugSettings(VisaAlias, false, false);
#endif
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

                errorCode = powerServo.CalculateServoParameters(targetOutputPower, expectedGain + 4, GainAccuracy, ActiveWaveform.PAR, out SApeakLevDB, out VSGanalogLevel);
                //SA_VST.RFSAsession.SetReferenceLevel("", SApeakLevDB);
                SG_VST.RFSGsession.ConfigureRF(SA_VST.CenterFrequency, VSGanalogLevel + ActiveWaveform.PAR);
                errorCode = powerServo.ConfigureActiveServoWindow(!turbo && ActiveWaveform.IsBursted, ActiveWaveform.ServoWindowLengthSec);    // If WindowEnabled = false, the servo does not use any triggering
                errorCode = powerServo.ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride.Arm_Ref_Trig_On_Servo_Done);
                errorCode = powerServo.Setup(powerTolerance, InitialAvgTime, FinalAvgTime, minIterations, maxIterations, false, 0);

                SA.Initiate();
                SG.Initiate();
            }

            public void Configure_Servo(Config o)
            {
                //PowerServo.DigitalGainStepLimitEnabled(VSTref, true, out errorCode);
                //PowerServo.ResetDigitalGainOnFailureEnabled(VSTref, true, out errorCode);

                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                for (int i = 0; i < ThreadFlags.Length; i++)
                {
                    threadFlags[i] = new ManualResetEvent(false);
                    threadFlags[i].Reset();
                }

                bool turbo = false;
         
                double InitialAvgTime = IQ.Mem[SA_VST.ModulationStd + SA_VST.WaveformName].IntialServoMeasTime;
                double FinalAvgTime = IQ.Mem[SA_VST.ModulationStd + SA_VST.WaveformName].FinalServoMeasTime;
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
                    maxIterations = 40;  //hosein 04062021
                }

                double DesiredAccuracy = 0.05; //org 0.05
                double SApeakLevDB, GainAccuracy;
                GainAccuracy = 5;

                double TargetPoutAfterLoss = o.TargetPout + o.LossPout;
                double InitialGain = o.ExpectedGain + o.LossPout;

                errorCode = powerServo.CalculateServoParameters(TargetPoutAfterLoss, InitialGain, GainAccuracy, IQ.Mem[SA_VST.ModulationStd + SA_VST.WaveformName].PAR, out SApeakLevDB, out VSGanalogLevel);
                if (o.manualSetVSGanalogLev != 0)
                {
                    VSGanalogLevel = o.manualSetVSGanalogLev + (GainAccuracy + 1);
                }

                SA_VST.SetLOshare(o.TestEVM);
                SG_VST.SetLOshare(o.TestEVM);

                //SA_VST.SetLOshare(false);
                //SG_VST.SetLOshare(false);

                ThreadPool.QueueUserWorkItem(cRfmxAcp.CommitSpec, cRfmxAcp.GetSpecIteration());

                Eq.Site[Site].RF.SetActiveWaveform(SA_VST.ModulationStd, SA_VST.WaveformName, false, true);
                SG_VST.Abort();
                SG_VST.RFSGsession.SetExternalGain("", o.LossPin + (20 * Math.Log10(Eq.Site[Site].RF.SG.Scaling_Factor)));
                SG_VST.RFSGsession.SetArbPreFilterGain("", 0);
                SG_VST.RFSGsession.ConfigureRF(o.Freq * 1e6, VSGanalogLevel + ActiveWaveform.PAR);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", 0e6);
                SG_VST.Initiate();

                //SG_VST.RFSGsession.GetUpconverterCenterFrequency("", out double a);

                threadFlags[0].WaitOne();
                errorCode = powerServo.FailServoOnDigitalSaturationEnabled(false);
                errorCode = powerServo.DigitalGainStepLimitEnabled(false);
                errorCode = powerServo.ConfigureActiveServoWindow(!turbo && ActiveWaveform.IsBursted, ActiveWaveform.ServoWindowLengthSec);    // If WindowEnabled = false, the servo does not use any triggering
                errorCode = powerServo.ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride.Arm_Ref_Trig_On_Servo_Done);
                errorCode = powerServo.Setup(DesiredAccuracy, InitialAvgTime, FinalAvgTime, minIterations, maxIterations, false, 0);


                threadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();
                //    WaitHandle.WaitAll(ThreadFlags, 100);

            }

            public bool Servo(out double Pout, out double Pin, double LossPout)
            {
                double TimeOut = 10;  //hosein 02102021
                double RawDigitalGain;
                double DigitalGaindB;
                bool Done = false;
                bool Failed = false;
                ushort NumAveragesCaptured;

                cRfmxAcp.InitiateSpec(cRfmxAcp.GetSpecIteration());

                errorCode = powerServo.Start();
                errorCode = powerServo.Wait(TimeOut, out NumAveragesCaptured, out Done, out Failed);
                SG_VST.SendSoftwareTrigger();
                //errorCode = PowerServo.GetServoSteps(NumAveragesCaptured, false, false, 0, out MeasurementTimes, out MeasurementLevels);
                errorCode = powerServo.GetServoSteps(NumAveragesCaptured, true, true, 0, out MeasurementTimes, out MeasurementLevels);
                errorCode = powerServo.GetDigitalGain(out RawDigitalGain, out DigitalGaindB);

                cRfmxAcp.WaitForAcq();

                Pout = MeasurementLevels[MeasurementLevels.Length - 1] - LossPout;
                Pin = VSGanalogLevel + DigitalGaindB;

                double gain = Pout - Pin;

#if false

                if (Failed)
                {

                }
#endif
                return !Failed;
            }

            public void Configure_Servo_Timing(Config o)
            {
                //PowerServo.DigitalGainStepLimitEnabled(VSTref, true, out errorCode);
                //PowerServo.ResetDigitalGainOnFailureEnabled(VSTref, true, out errorCode);

                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                for (int i = 0; i < threadFlags.Length; i++)
                {
                    ThreadFlags[i] = new ManualResetEvent(false);
                    ThreadFlags[i].Reset();
                }

                bool turbo = false;

                double InitialAvgTime = IQ.Mem[SA_VST.ModulationStd + SA_VST.WaveformName].IntialServoMeasTime;
                double FinalAvgTime = IQ.Mem[SA_VST.ModulationStd + SA_VST.WaveformName].FinalServoMeasTime;
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

                double DesiredAccuracy = 0.05;
                double SApeakLevDB, GainAccuracy;
                GainAccuracy = 5;

                double TargetPoutAfterLoss = o.TargetPout + o.LossPout;
                double InitialGain = o.ExpectedGain + o.LossPout;

                errorCode = powerServo.CalculateServoParameters(TargetPoutAfterLoss, InitialGain, GainAccuracy, IQ.Mem[SA_VST.ModulationStd + SA_VST.WaveformName].PAR, out SApeakLevDB, out VSGanalogLevel);
                if (o.manualSetVSGanalogLev != 0)
                {
                    VSGanalogLevel = o.manualSetVSGanalogLev + (GainAccuracy + 1);
                }

                ThreadPool.QueueUserWorkItem(cRfmxIQ_Timing.CommitSpec_ForServo, cRfmxIQ_Timing.GetSpecIteration());

                SG_VST.Abort();
                Eq.Site[Site].RF.SetActiveWaveform(SA_VST.ModulationStd, SA_VST.WaveformName, false, true);
                SG_VST.RFSGsession.SetExternalGain("", o.LossPin + (20 * Math.Log10(Eq.Site[Site].RF.SG.Scaling_Factor)));
                SG_VST.RFSGsession.SetArbPreFilterGain("", 0);
                SG_VST.RFSGsession.ConfigureRF(o.Freq * 1e6, VSGanalogLevel + ActiveWaveform.PAR);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", -0e6);
                SG_VST.Initiate();

                threadFlags[0].WaitOne(); // edward 6/12 Dec

                errorCode = powerServo.FailServoOnDigitalSaturationEnabled(false);
                errorCode = powerServo.DigitalGainStepLimitEnabled(false);
                errorCode = powerServo.ConfigureActiveServoWindow(!turbo && ActiveWaveform.IsBursted, ActiveWaveform.ServoWindowLengthSec);    // If WindowEnabled = false, the servo does not use any triggering
                errorCode = powerServo.ConfigureVSAReferenceTriggerOverride(ReferenceTriggerOverride.Arm_Ref_Trig_On_Servo_Done);
                errorCode = powerServo.Setup(DesiredAccuracy, InitialAvgTime, FinalAvgTime, minIterations, maxIterations, false, 0);


                ThreadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();
              


            }

            public bool Servo_Timing(out double Pout, out double Pin, double LossPout)
            {
                double TimeOut = 10;
                double RawDigitalGain;
                double DigitalGaindB;
                bool Done = false;
                bool Failed = false;
                ushort NumAveragesCaptured;

                cRfmxIQ_Timing.InitiateSpec_ForServo(cRfmxIQ_Timing.GetSpecIteration());

                errorCode = powerServo.Start();
                errorCode = powerServo.Wait(TimeOut, out NumAveragesCaptured, out Done, out Failed);
                errorCode = powerServo.GetServoSteps(NumAveragesCaptured, false, false, 0, out MeasurementTimes, out MeasurementLevels);
                errorCode = powerServo.GetDigitalGain(out RawDigitalGain, out DigitalGaindB);

                cRfmxAcp.WaitForAcq();

                Pout = MeasurementLevels[MeasurementLevels.Length - 1] - LossPout;
                Pin = VSGanalogLevel + DigitalGaindB;

                double gain = Pout - Pin;


                return !Failed;
            }

            public void Configure_CHP(Config o)
            {
                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                for (int i = 0; i < ThreadFlags.Length; i++)
                {
                    ThreadFlags[i] = new ManualResetEvent(false);
                    ThreadFlags[i].Reset();
                }
                cRfmxAcp.ConfigFreq(o.Freq);

                ThreadPool.QueueUserWorkItem(cRfmxAcp.CommitSpec, cRfmxAcp.GetSpecIteration());

                Eq.Site[Site].RF.SetActiveWaveform(SA_VST.ModulationStd, SA_VST.WaveformName, false, true);
                SG_VST.RFSGsession.SetExternalGain("", o.LossPin + (20 * Math.Log10(Eq.Site[Site].RF.SG.Scaling_Factor)));
                SG_VST.RFSGsession.SetArbPreFilterGain("", 0);
                SG_VST.RFSGsession.ConfigureRF(o.Freq * 1e6, o.TargetPout + ActiveWaveform.PAR);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", 0e6);
                SG_VST.Initiate();

                ThreadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();

                WaitHandle.WaitAll(ThreadFlags, 100);

            }
            public void Measure_CHP()
            {
                cRfmxAcp.InitiateSpec(cRfmxAcp.GetSpecIteration());       
            }

            public void Measure_CHP(double SOAK_Delay)
            {
                if (SOAK_Delay > 0)
                {
                    //SG_VST.Initiate(); //ChoonChin - already initiated at ConfigureCHP.
                    Thread.Sleep((int)SOAK_Delay);
                    cRfmxAcp.InitiateSpec(cRfmxAcp.GetSpecIteration());
                }
                else
                {
                    cRfmxAcp.InitiateSpec(cRfmxAcp.GetSpecIteration());
                    //SG_VST.Initiate();
                }
            }

            public void Configure_IQ(Config_IQ o)
            {

                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                for (int i = 0; i < threadFlags.Length; i++)
                {
                    threadFlags[i] = new ManualResetEvent(false);
                    threadFlags[i].Reset();
                }

                Eq.Site[Site].RF.SA.ReferenceLevel = o.Reflevel;
                cRfmxIQ.SetFreq(o.Freq * 1e6);
                ThreadPool.QueueUserWorkItem(cRfmxIQ.CommitSpec, cRfmxIQ.GetSpecIteration());

                Eq.Site[Site].RF.SetActiveWaveform("PINSWEEP", "", false, true);

                SG_VST.RFSGsession.SetExternalGain("", o.LossPin + (20 * Math.Log10(Eq.Site[Site].RF.SG.Scaling_Factor)));
                SG_VST.RFSGsession.SetArbPreFilterGain("", 0);
               // SG_VST.RFSGsession.ConfigureRF(o.Freq * 1e6, 2);
                SG_VST.RFSGsession.ConfigureRF(o.Freq * 1e6, o.PinSweepStop);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", -40e6);
                threadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();

               // ThreadFlags[1].WaitOne();


            }
            public void Measure_IQ()
            {
                cRfmxIQ.InitiateSpec(cRfmxIQ.GetSpecIteration());
                SG_VST.Initiate();
            }

            public void Configure_IQ_EVM(Config_IQ_EVM o)
            {

                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                for (int i = 0; i < threadFlags.Length; i++)
                {
                    threadFlags[i] = new ManualResetEvent(false);
                    threadFlags[i].Reset();
                }

                Eq.Site[Site].RF.SA.ReferenceLevel = o.Reflevel; // [BurhanEVM]
                //cRfmxIQ.Set_ReferenceLevel(o.Reflevel); // [BurhanEVM]
                SG_VST.SetLOshare(true);

                cRfmxIQ_EVM.SetFreq(o.Freq * 1e6);
                cRfmxIQ_EVM.CommitSpec(cRfmxIQ_EVM.GetSpecIteration());

                threadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();

                // ThreadFlags[1].WaitOne();
            }

            public void Measure_IQ_EVM()
            {
                cRfmxIQ_EVM.InitiateSpec(cRfmxIQ_EVM.GetSpecIteration());
            }


            public void Configure_IIP3(Config_IIP3 o)
            {

                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                SA_VST.SetLOshare(true);
                SG_VST.SetLOshare(true);

                for (int i = 0; i < threadFlags.Length; i++)
                {
                    threadFlags[i] = new ManualResetEvent(false);
                    threadFlags[i].Reset();
                }

                //Thread.Sleep(1);  //hosein 05212020 delay
                ThreadPool.QueueUserWorkItem(cRfmxIIP3.CommitSpec, cRfmxIIP3.GetSpecIteration());
                Thread.Sleep(1);  //hosein 05212020

                Eq.Site[Site].RF.SetActiveWaveform("TWOTONE", "", false, true);
                Eq.Site[Site].RF.ServoEnabled = false;

                SG_VST.RFSGsession.SetExternalGain("", o.LossPin + (20 * Math.Log10(Eq.Site[Site].RF.SG.Scaling_Factor)));

                SG_VST.RFSGsession.SetArbPreFilterGain("", -10);
                SG_VST.RFSGsession.SetPowerLevel("", (o.TargetPin + 3) + IQ.Mem["TWOTONE"].PAR);
                SG_VST.RFSGsession.SetFrequency("", o.Freq * 1e6);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", 10e6);

                ThreadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();

                threadFlags[0].WaitOne();
                threadFlags[1].WaitOne();



            }
            public void Measure_IIP3()
            {
                SG_VST.Initiate();
                cRfmxIIP3.InitiateSpec(cRfmxIIP3.GetSpecIteration());
                
            }

            public void Configure_Timing(Config_Timing o)
            {

                threadFlags = new ManualResetEvent[Enum.GetNames(typeof(ThreadFlag)).Length];

                for (int i = 0; i < threadFlags.Length; i++)
                {
                    threadFlags[i] = new ManualResetEvent(false);
                    threadFlags[i].Reset();
                }

                Eq.Site[Site].RF.ServoEnabled = false;
                SG_VST.RFSGsession.Abort();
                cRfmxIQ_Timing.SetFreq(o.Freq * 1e6);
                ThreadPool.QueueUserWorkItem(cRfmxIQ_Timing.CommitSpec, cRfmxIQ_Timing.GetSpecIteration());


                Eq.Site[Site].RF.SetActiveWaveform("CW", "", false, true);
                SG_VST.RFSGsession.SetExternalGain("", o.LossPin + (20 * Math.Log10(Eq.Site[Site].RF.SG.Scaling_Factor)));
                SG_VST.RFSGsession.SetArbPreFilterGain("", 0);
                SG_VST.RFSGsession.ConfigureRF(o.Freq * 1e6, o.TargetPin + IQ.Mem["CW"].PAR);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", -40e6);
                SG_VST.RFSGsession.Initiate();

                threadFlags[Convert.ToInt16(ThreadFlag.SG)].Set();



            }
            public void Measure_Timing()
            {

                cRfmxIQ_Timing.InitiateSpec(cRfmxIQ_Timing.GetSpecIteration());
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
               // SA.SampleRate = (double)newWaveform.VsgIQrate;
                //SA.NumberOfSamples = newWaveform.SamplesPerRecord;
                ActiveWaveform = newWaveform;
            }

            private List<string> loadedWaveforms = new List<string>();

            public bool LoadWaveform(string ModStd, string WvfrmName)
            {
                try
                {
                    if ((ModStd == null | WvfrmName == null) || (ModStd == "" & WvfrmName == "")) return true;

                    if (!IQ.Load(ModStd, WvfrmName, _Model)) return false;

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
                
            }

            public void RFmxCommitSpec(eRfmx _eRfmx, RFmxCofig c)
            {

            }

            public void RFmxInitiateSpec(eRfmx _eRfmx, RFmxCofig c)
            {
      
            }

            public RFmxResult RFmxRetrieveResultsSpec(eRfmx _eRfmx, RFmxCofig c)
            {
        
                return null;
            }

            public void RFmxConfigureSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {
                switch (_eRfmx)
                {
                    case eRfmx_Measurement_Type.eRfmxAcp: cRfmxAcp.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.IqLength, c.ACPaverage, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxAcpNR: cRfmxAcpNR.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.IqLength, c.ACPaverage, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxHar2nd: cRfmxH2.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.Waveform, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.IqLength, c.ACPaverage, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxHar3rd: cRfmxH3.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.Waveform, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.IqLength, c.ACPaverage, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxTxleakage: cRfmxTxleakage.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.SpanforTxL, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.IqLength, c.ACPaverage, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxIQ: cRfmxIQ.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxIIP3: cRfmxIIP3.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.band, c.Site); break;

                    //case eRfmx_Measurement_Type.eRfmxChp: cRfmxCHP.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.Rbw); break;
                    case eRfmx_Measurement_Type.eRfmxChp: cRfmxCHP.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.WaveformName, c.Waveform, c.Rbw, c.PowerMode, c.Site); break;  // removed by hosein
                                                                                                                                                                                              //case eRfmx_Measurement_Type.eRfmxChp: cRfmxCHP.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.AclrBW, c.AclrOffsetFreq, c.WaveformName, c.Waveform, c.TestAcp, c.NumberOfOffsets, c.Rbw, c.PowerMode, c.IqLength, c.ACPaverage, c.Site); break;


                    case eRfmx_Measurement_Type.eRfmxIQ_EVM: cRfmxIQ_EVM.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.SampleRate, c.AcquisitionTime, c.WaveformName, c.Site); break; // [BurhanEVM]
                    case eRfmx_Measurement_Type.eRfmxIQ_Timing: cRfmxIQ_Timing.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.SampleRate, c.AcquisitionTime, c.TriggerDelay, c.Site); break;
                    case eRfmx_Measurement_Type.eRfmxEVM:
                        switch (IQ.Mem[c.Modulation + c.WaveformName].EVM_Type)
                        {
                            case "LTE": cRfmxEVM_LTE.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.WaveformName, c.band, c.Site);; break;
                            //case eRfmx_EVM_Type.LTEA: cRfmxEVM_LTEA.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.WaveformName, c.band);; break;
                            case "NR": cRfmxEVM_NR.ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.WaveformName, c.band, c.Site);; break;
                            default: throw new Exception(IQ.Mem[c.Modulation + c.Waveform].EVM_Type + " : Not yet Implemented RFmx");
                        }

                        break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }
            }

            public void RFmxCommitSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {
                switch (_eRfmx)
                {
                    case eRfmx_Measurement_Type.eRfmxAcp: cRfmxAcp.CommitSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxAcpNR: cRfmxAcpNR.CommitSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxIIP3: cRfmxIIP3.CommitSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxEVM:
                        switch (IQ.Mem[c.Modulation + c.Waveform].EVM_Type)
                        {
                            case "LTE": cRfmxEVM_LTE.CommitSpec(c.Iteration); break;
                            //case eRfmx_EVM_Type.LTEA: cRfmxEVM_LTEA.CommitSpec(c.Iteration); break;
                            case "NR": cRfmxEVM_NR.CommitSpec(c.Iteration); break;
                            default: throw new Exception(IQ.Mem[c.Modulation + c.Waveform].EVM_Type + " : Not yet Implemented RFmx");
                        }
                        break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }
            }

            public void RFmxInitiateSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {
                switch (_eRfmx)
                {
                    case eRfmx_Measurement_Type.eRfmxAcp: cRfmxAcp.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxChp: cRfmxCHP.InitiateSpec(c.Iteration); break;  // by Hosein 12/28/2019
                    case eRfmx_Measurement_Type.eRfmxAcpNR: cRfmxAcpNR.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxHar2nd: cRfmxH2.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxHar3rd: cRfmxH3.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxTxleakage: cRfmxTxleakage.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxIQ: cRfmxIQ.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxIIP3: cRfmxIIP3.InitiateSpec(c.Iteration); break;
                    case eRfmx_Measurement_Type.eRfmxEVM:
                        switch (IQ.Mem[c.Modulation + c.Waveform].EVM_Type)
                        {
                            case "LTE": cRfmxEVM_LTE.InitiateSpec(cRfmxEVM_LTE.GetSpecIteration()); break;
                            //case eRfmx_EVM_Type.LTEA: cRfmxEVM_LTEA.InitiateSpec(cRfmxEVM_CDMA2K.GetSpecIteration()); break;
                            case "NR": cRfmxEVM_NR.InitiateSpec(cRfmxEVM_NR.GetSpecIteration()); break;
                            default: throw new Exception(IQ.Mem[c.Modulation + c.Waveform].EVM_Type + " : Not yet Implemented RFmx");
                        }
                        break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }
            }

            public RFmxResult RFmxRetrieveResultsSpec(eRfmx_Measurement_Type _eRfmx, Config c)
            {
                RFmxResult _RFmxResult = new EqLib.EqRF.RFmxResult();
                double averageChannelPower = 0f;
                double PeaksearchPower = 0f;

                switch (_eRfmx)
                {
                    case eRfmx_Measurement_Type.eRfmxAcp:

                        if(!c.TestAcp)
                            averageChannelPower = cRfmxAcp.RetrieveResults(c.Iteration);
                        _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);
                        break;
                    case eRfmx_Measurement_Type.eRfmxAcpNR:

                        if (!c.TestAcp)
                            averageChannelPower = cRfmxAcpNR.RetrieveResults(c.Iteration);
                        _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);
                        break;
                    case eRfmx_Measurement_Type.eRfmxHar2nd:

                        //200310 Mario NightHawk Peaksearch Measurment
                        //if (cRfmxH2.FreqforHar > 6000)
                        //{
                        //    PeaksearchPower = cRfmxH2.RetrieveResults_Peak(c.Iteration);
                        //    _RFmxResult = new EqLib.EqRF.RFmxResult(PeaksearchPower);

                        //}
                        //else
                        {
                            averageChannelPower = cRfmxH2.RetrieveResults(c.Iteration);
                            _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);
                        }
                        break;
                    case eRfmx_Measurement_Type.eRfmxHar3rd:

                        averageChannelPower = cRfmxH3.RetrieveResults(c.Iteration);
                        _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);
                        break;
                    case eRfmx_Measurement_Type.eRfmxTxleakage:

                        averageChannelPower = cRfmxTxleakage.RetrieveResults(c.Iteration);
                        _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);
                        break;

                    case eRfmx_Measurement_Type.eRfmxChp:   // by hosein 12/29/2019

                        averageChannelPower = cRfmxCHP.RetrieveResults(c.Iteration);  // by hosein 12/29/2019
                        _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);  // by hosein 12/29/2019
                        break;   // by hosein 12/29/2019

                    case eRfmx_Measurement_Type.eRfmxIIP3:

                        double lowerTonePower;
                        double upperTonePower;
                        int[] intermodOrder = new int[1];
                        double[] lowerIntermodPower = new double[1];
                        double[] upperIntermodPower = new double[1];

                         cRfmxIIP3.RetrieveResults(c.Iteration, out lowerTonePower, out upperTonePower, ref lowerIntermodPower, ref upperIntermodPower, ref intermodOrder);
                        _RFmxResult = new EqLib.EqRF.RFmxResult(lowerTonePower, upperTonePower, lowerIntermodPower, upperIntermodPower, intermodOrder);
                        break;

                    case eRfmx_Measurement_Type.eRfmxEVM:

                        switch (IQ.Mem[c.Modulation + c.Waveform].EVM_Type)
                        {
                            case "LTE": averageChannelPower = cRfmxEVM_LTE.RetrieveResults(cRfmxEVM_LTE.GetSpecIteration(), c.WaveformName); break;
                            //case eRfmx_EVM_Type.LTEA: averageChannelPower = cRfmxEVM_LTEA.RetrieveResults(cRfmxEVM_CDMA2K.GetSpecIteration()); break;
                            case "NR": averageChannelPower = cRfmxEVM_NR.RetrieveResults(cRfmxEVM_NR.GetSpecIteration()); break;

                            default: throw new Exception(IQ.Mem[c.Modulation + c.Waveform].EVM_Type + " : Not yet Implemented RFmx");
                        }

                        //averageChannelPower = CRfmxEVM.RetrieveResults(c.Iteration);

                        _RFmxResult = new EqLib.EqRF.RFmxResult(averageChannelPower);
                        break;
                    default: throw new Exception(_eRfmx.ToString() + " : Not yet Implemented RFmx");
                }

                return _RFmxResult;
            }

            public void Configure_IIP3_RFSG_Parameters(double LossPin, double RFfrequency, float TargetPin)
            {
                SG_VST.RFSGsession.SetSelectedScript("", IQ.Mem["TWOTONE"].ScriptName);
                SG_VST.RFSGsession.SetIqRate("", (double)IQ.Mem["TWOTONE"].VsgIQrate);
                SG_VST.Level = Convert.ToSingle(TargetPin + 3);
                SG_VST.RFSGsession.SetExternalGain("", (LossPin));
                SG_VST.RFSGsession.SetArbPreFilterGain("", -5);
                SG_VST.RFSGsession.SetUpconverterFrequencyOffset("", -40e6);
                SG_VST.RFSGsession.SetFrequency("", (RFfrequency) * 1e6);
                SG_VST.RFSGsession.Initiate();
            }

            public void ResetDriver()
            {
                RFMX.ResetDriver();
            }
            public void ResetRFSA(bool Enable)
            {
                if (Enable)
                    errorCode = powerServo.Enable();
                else
                {
                    errorCode = powerServo.Reset();
                    errorCode = powerServo.Disable();

                }


            }
            public void close()
            {
                if(RFMX != null)
                    RFMX.close();
                SA_VST.RFSAsession.Close();
            }

            public static string TranslateTriggerLine(TriggerLine trigLine)
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

            public void ConfigureDebugSettings(string aliasName, bool requestedValueDebugEnabled, bool requestedValueCBreakPointsEnabled)
            {
                const int noOfRetries = 100;
                const int msToWaitbeforeRetrying = 200;
                ResourceProperty isDebugSupportedProperty = ResourceProperty.RegisterSimpleType(typeof(bool), 0x10001000);
                ResourceProperty debugSessionConfigurationProperty = ResourceProperty.RegisterSimpleType(typeof(UInt32), 0x10002000);
                ResourceProperty usingCBreakpointsProperty = ResourceProperty.RegisterSimpleType(typeof(bool), 0x10003000);
                //Open a session to localhost
                SystemConfiguration session = new SystemConfiguration("");
                //Create a filter
                Filter devicefilter = new Filter(session, FilterMode.MatchValuesAll) { UserAlias = aliasName };
                //Find hardware based on given alias
                ResourceCollection resources = session.FindHardware(devicefilter);
                if (resources.Count == 0)
                    throw new Exception("Error: No hardware found with the given alias!!!");
                //Always use the device at index 0 to read and write the settings
                HardwareResourceBase hwResource = resources[0];
                bool isDebugSupported = Convert.ToBoolean(hwResource.GetPropertyValue(isDebugSupportedProperty));
                if (isDebugSupported)
                {
                    hwResource.SetPropertyValue(debugSessionConfigurationProperty, Convert.ToUInt32(requestedValueDebugEnabled));
                    hwResource.SetPropertyValue(usingCBreakpointsProperty, requestedValueCBreakPointsEnabled);
                    //Save the changes 
                    bool requiresRestart = false;
                    hwResource.SaveChanges(out requiresRestart);
                    //Read back the saved change to confirm the settings bave been successfully applied.
                    //Retry multiple times as it can take time for the settings to take effect
                    //If there is a long time gap between changing the settings and Creating/Initializing 
                    //the RFmx session then re-try logic can be skipped.
                    for (int i = 0; i < noOfRetries; i++) //Retry
                    {
                        Object myobj = hwResource.GetPropertyValue(debugSessionConfigurationProperty);
                        bool value1 = Convert.ToBoolean(myobj);
                        myobj = hwResource.GetPropertyValue(usingCBreakpointsProperty);
                        bool value2 = Convert.ToBoolean(myobj);
                        if (value1 == requestedValueDebugEnabled && value2 == requestedValueCBreakPointsEnabled)
                            return;//Settings successfully applied
                        System.Threading.Thread.Sleep(msToWaitbeforeRetrying);//Wait for before retrying
                    }
                    throw new Exception("Error: Unable to update settings");
                }
                else
                    throw new Exception("Error: Device does not support debugging");
            }

            public class PxiVstSg : iEqSG
            {
                public NIVST_Rfmx vst { get; set; }
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
                public PxiVstSg(NIVST_Rfmx vst)
                {
                    this.vst = vst;
                }

                public void Initialize(string VSGname)
                {
                    if (RFSGsession == null)
                    {
                        RFSGsession = new niRFSG(VSGname, true, true, "DriverSetup=Bitfile:NI-RFIC.lvbitx");
                    }


                    if (vst.IsVST1) _Scaling_Factor = 1;
                    else
                    {
                        _Scaling_Factor = 0.8;
                        RFSGsession.SetArbWaveformSoftwareScalingFactor("", _Scaling_Factor);
                    }

                    RFSGsession.ConfigureRefClock(niRFSGConstants.PxiClkStr, 10e6);
                    RFSGsession.ConfigureGenerationMode(niRFSGConstants.Script);
                    RFSGsession.ConfigurePowerLevelType(niRFSGConstants.PeakPower);

                    RFSGsession.SetLoFrequencyStepSize("", 50e3);
                    RFSGsession.ConfigureSoftwareScriptTrigger(niRFSGConstants.ScriptTrigger0);

                    //Quadsite: The following line is to support BurstMode. Commented out as there are not enough lines to support
                    //single controller two site on single chassis
                    //RFSGsession.ExportSignal(niRFSGConstants.ScriptTrigger, niRFSGConstants.ScriptTrigger0, niRFSGConstants.PxiTrig5Str);

                    TriggerLine trigOutTerminal = (TriggerLine)siteTrigArray[vst.Site][0];
                    ExportSignal(0, trigOutTerminal);
                    ExportSignal(1, TriggerLine.FrontPanel0);

                    //Quadsite: The following line is to support BurstMode. Commented out as there are not enough lines to support
                    //single controller two site on single chassis
                    //ExportSignal(2, TriggerLine.PxiTrig4);




                    //RFSGsession.SetRfBlankingSource("", niRFSGConstants.Marker2EventStr);

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
                    if (Flag)
                    {
                        if (!LOShare)
                        {
                            RFSGsession.Abort();
                            RFSGsession.SetLoOutEnabled("", true);//Modified by jake
                            LOShare = true;

                        }

                    }
                    else
                    {
                        if (LOShare)
                        {
                            RFSGsession.Abort();
                            RFSGsession.SetLoOutEnabled("", Flag);//Modified by jake
                            LOShare = false;
                        }
                    }
                    //RFSGsession.GetLoFrequencyStepSize("", out double aa);
                    //RFSGsession.GetUpconverterCenterFrequency("", out double a);
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
                      //  RFSGsession.SetUpconverterFrequencyOffset("", -40e6);
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

                public void SetLofreq(double LO_Freq)
                {
                    //RFSGsession.GetUpconverterCenterFrequency("", out double aa);
                    RFSGsession.Abort();
                    RFSGsession.SetUpconverterFrequencyOffset("", LO_Freq);
                    RFSGsession.Initiate();

                }

                public void Abort()
                {
                    RFSGsession.Abort();
                }
                public void SelfCalibration(double startfreq, double stopfreq, double minrefpwr, double maxrefpwr)
                {
                    Avago.ATF.Logger.ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Info, "RFSG SelfCal Running");  // added by Hosein 01242020
                    RFSGsession.SelfCalibrateRange(0, startfreq, stopfreq, minrefpwr, maxrefpwr);
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
                public NIVST_Rfmx vst { get; set; }
                public bool LOShare { get; set; }
                internal niRFSA RFSAsession;
                static IntPtr niRfsaHandle;
                private double _rfFrequency, _refLevel, _externalGain, _sampleRate;
                private string _ModulationStd;
                private string _WaveformName;
                private long _numberOfSamples;
                private TriggerLine _triggerIn, _triggerOut;

                public PxiVstSa(NIVST_Rfmx vst)
                {
                    this.vst = vst;
                }

                public void Initialize(string VSAname)
                {
                    vst.instrSession  = new RFmxInstrMX(VSAname, "DriverSetup=Bitfile:NI-RFIC.lvbitx");
                    vst.instrSession.ConfigureFrequencyReference("", RFmxInstrMXConstants.PxiClock, 10e6);

                    string trigOutTerminal = TranslateTriggerLine((TriggerLine)siteTrigArray[vst.Site][1]);
                    vst.instrSession.ExportSignal(RFmxInstrMXExportSignalSource.ReferenceTrigger, trigOutTerminal);
                    vst.instrSession.DangerousGetNIRfsaHandle(out niRfsaHandle);
                    vst.instrSession.SetDownconverterFrequencyOffset("", -40 * 1e6);
                    
  
                    RFSAsession = new niRFSA(niRfsaHandle);


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
                    if (Flag)
                    {
                        if (!LOShare)
                        {
                            vst.instrSession.SetLOSource("", "LO_In");//Modified by jake
                            LOShare = true;
                        }

                    }
                    else
                    {
                        if (LOShare)
                        {
                            vst.instrSession.SetLOSource("", "Onboard");//Modified by jake
                            LOShare = false;
                        }
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
                      //  RFSAsession.SetReferenceLevel("", value);
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

                public double MeasureChanPower(int centerChanBwMultiplier = 1)
                {
                    //RFSAsession.Abort();
                    niComplexNumber[] Data = MeasureIqTrace(false);

                    SpectralAnalysis sa = new SpectralAnalysis(vst.ActiveWaveform, Data);

                    double chPow = sa.GetCenterChannelPower(centerChanBwMultiplier);

                    return chPow;
                }  //Added by Hosein

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
                    int _Iteration = thisIteration == int.MinValue ? vst.cRfmxEVM_LTE.GetSpecIteration() : thisIteration;
                    vst.cRfmxAcp.RetrieveResults(_Iteration, ref aclrResults);
                }

                public void MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace, ref byte site, bool NR)
                {
                    if (NR) Eq.Site[site].RF.CRfmxAcpNR.RetrieveResults(Eq.Site[site].RF.CRfmxAcpNR.GetSpecIteration(), ref aclrResults);
                }
                public void MeasureEVM(string EVMtype, out double EVMresult, int thisIteration = int.MinValue)
                {
                    int _Iteration = thisIteration == int.MinValue ? vst.cRfmxEVM_LTE.GetSpecIteration() : thisIteration;
                    switch (EVMtype)
                    {                       
                        case "LTE": EVMresult = vst.cRfmxEVM_LTE.RetrieveResults(_Iteration, WaveformName); break;
                        //case eRfmx_EVM_Type.LTEA: EVMresult = cRfmxEVM_LTEA.RetrieveResults(cRfmxEVM_CDMA2K.GetSpecIteration()); break;
                        case "NR": EVMresult = vst.cRfmxEVM_NR.RetrieveResults(_Iteration); break;

                        default: throw new Exception(EVMtype + " : Not yet Implemented RFmx");
                    }                   
                }
                public void Initiate()
                {
                    RFSAsession.Initiate();
                }

                public void Abort(byte site)
                {
                    Eq.Site[site].RF.NiPowerServo.Reset();
                    
                    RFSAsession.Abort();
                }

                public void SelfCalibration(double startfreq, double stopfreq, double minrefpwr, double maxrefpwr)
                {
                    Avago.ATF.Logger.ATFLogControl.Instance.Log(Avago.ATF.LogService.LogLevel.Info, "RFSA SelfCal Running");  // added by Hosein 01242020
                    RFSAsession.SelfCalibrateRange(0, startfreq, stopfreq, minrefpwr, maxrefpwr);
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

            public class PxiRFExtd : iEqRFExtd
            {

                public NIVST_Rfmx RFExtd { get; set; }
                public double OutFrequency;

                public PxiRFExtd(NIVST_Rfmx RFExtd)
                {
                    this.RFExtd = RFExtd;
                }

                public void Initialize(string Visa)
                {
                    RFExtd.RFExtdsession = new si2250(Visa, false, true);

                    RFExtd.RFExtdsession.reset();
                    RFExtd.RFExtdsession.ConfigureTXLOSource(si2250Constants.LoSourceOnboard);
                    RFExtd.RFExtdsession.ConfigureRXLOSource(si2250Constants.LoSourceOnboard);
                    //RFExtdsession.ConfigureRXIFFilter(1400);
                    RFExtd.RFExtdsession.ConfigureRXIFFilterPath(si2250Constants.RxIfFilterPathEnabled);
                    RFExtd.RFExtdsession.ConfigureRXBypassPath(si2250Constants.RxBypassPathFilterBank);
                }
                public void ConfigureTXPort(int Port)
                {
                    if (Port == 1) RFExtd.RFExtdsession.ConfigureTXOutputPath(si2250Constants.TxOutputPathTxOut1);
                    else RFExtd.RFExtdsession.ConfigureTXOutputPath(si2250Constants.TxOutputPathTxOut0Direct);
                }
                public void ConfigureCalibrationTone(double OutFrequency, out double InFrequency)
                {
                    InFrequency = 0f;
                    RFExtd.RFExtdsession.ConfigureCalibrationTone(OutFrequency, out InFrequency);
                }
                public void ConfigureTXInputFreq(double Freq)
                {
                    RFExtd.RFExtdsession.ConfigureTXINFrequency(Freq);
                }
                public void ConfigureTXOutputFreq(double Freq)
                {
                    RFExtd.RFExtdsession.ConfigureTXOUTFrequency(Freq);
                }

                public void ConfigureRXBypass(int Path)
                {
                    if (Path == 0) RFExtd.RFExtdsession.ConfigureRXBypassPath(si2250Constants.RxBypassPathFilterBank);
                    else RFExtd.RFExtdsession.ConfigureRXBypassPath(si2250Constants.RxBypassPathLna);
                }
                public void ConfigureRXDownconversion()
                {
                    RFExtd.RFExtdsession.ConfigureRXConversionGainPath(si2250Constants.RxConversionGainPathLna);
                }
                public void ConfigureLosource()
                {
                    int errChk;

                    errChk = RFExtd.RFExtdsession.ConfigureRXLOSource(si2250Constants.LoSourceOnboard);
                    errChk = RFExtd.RFExtdsession.ConfigureRXConversionGainPath(si2250Constants.RxConversionGainPathNone);
                    errChk = RFExtd.RFExtdsession.ConfigureRXIFFilterPath(si2250Constants.RxIfFilterPathEnabled);
                }

                public void ConfigureHarmonicConverter(double Fundamental, int HarmonicIndex, out double OutFrequency)
                {
                    OutFrequency = 0f;
                    RFExtd.RFExtdsession.ConfigureHarmonicConverter(Fundamental, HarmonicIndex, out OutFrequency);
                }
                public double HMU_MeasureTemperature(out double Temperature)
                {
                    double ctemp = 0;
                    RFExtd.RFExtdsession.MeasureTemperature(out Temperature);
                    ctemp = Temperature;
                    return ctemp;
                }

                public int Self_Test(out short TestResult, System.Text.StringBuilder TestMessage)
                {
                    RFExtd.RFExtdsession.self_test(out TestResult, TestMessage);
                    return TestResult;
                }
            }
        }

        public class RfmxAcp
        {
            List<RFmxSpecAnMX> specAcp;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double Freq;
            public byte siteNo;
            public RfmxAcp(byte site)
            {
                siteNo = site;
                specAcp = new List<RFmxSpecAnMX>();
            }

            public void ConfigFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw , 
                string PowerMode, double IqLength, int ACPaverage, byte site)
            {
                if (Rbw == 0) Rbw = 100e3;
                string selectorString;
                string test;
                int NumberOfCarriers = 1;
                double rbw = 0;
                double tempFreq = 0;

                siteNo = site;
                string acptrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);


                test = "ACP" + Iteration.ToString();
                specAcp.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                specAcp[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                specAcp[Iteration].ConfigureDigitalEdgeTrigger("", acptrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specAcp[Iteration].ConfigureReferenceLevel("", Reflevel);
                specAcp[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation      

                if (TestAcp)
                {
                    specAcp[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Acp, false);

                    //Sequential FFT
                    if (ACPaverage == 0)
                    {
                        specAcp[Iteration].Acp.Configuration.ConfigureMeasurementMethod("", RFmxSpecAnMXAcpMeasurementMethod.SequentialFft);
                        specAcp[Iteration].Acp.Configuration.SetSequentialFftSize("", 1024);
                        ACPaverage = 1;
                    }

                    specAcp[Iteration].Acp.Configuration.ConfigurePowerUnits("", RFmxSpecAnMXAcpPowerUnits.dBm);
                    specAcp[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.FlatTop, 1);
#if false
                            //if (WaveformName == "TS01")
                            //{
                            //    //specAnAcpSignal[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.Gaussian, 1);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.Gaussian);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, 0.530e-3);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.True, 10, RFmxSpecAnMXAcpAveragingType.Rms);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);
                            //}
                            //else if (WaveformName == "TC6")
                            //{
                            //    //specAnAcpSignal[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.Gaussian, 1);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.Gaussian);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, IqLength);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.True, 3, RFmxSpecAnMXAcpAveragingType.Rms);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);
                            //}
                            //else
                            //{
                            //    specAcp[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.FftBased);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, IqLength);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.False, 1, RFmxSpecAnMXAcpAveragingType.Rms);
                            //    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);
                            //}
#endif
                    specAcp[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.FftBased);
                    specAcp[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, IqLength);
                    specAcp[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.True, ACPaverage, RFmxSpecAnMXAcpAveragingType.Rms);
                    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);

                    for (int j = 0; j < NumberOfCarriers; j++)
                    {
                        selectorString = RFmxSpecAnMX.BuildCarrierString("", j);

                        specAcp[Iteration].Acp.Configuration.ConfigureCarrierIntegrationBandwidth(selectorString,
                                                                                      RefChBW);
                        specAcp[Iteration].Acp.Configuration.ConfigureCarrierMode(selectorString,
                                                                      RFmxSpecAnMXAcpCarrierMode.Active);
                        specAcp[Iteration].Acp.Configuration.ConfigureCarrierRrcFilter("", RFmxSpecAnMXAcpCarrierRrcFilterEnabled.False, 0.22);
                        specAcp[Iteration].Acp.Configuration.ConfigureCarrierFrequency(selectorString, 0);
                    }

                    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfOffsets("", AdjChsBW.Length);

                    for (int j = 0; j < NumberOfOffsets; j++)
                    {
                        selectorString = RFmxSpecAnMX.BuildOffsetString("", j);
                        specAcp[Iteration].Acp.Configuration.ConfigureOffsetIntegrationBandwidth(selectorString,
                                                                                     AdjChsBW[j]);
                        specAcp[Iteration].Acp.Configuration.ConfigureOffset(selectorString,
                                                                 AdjChsFreqOffset[j],
                                                                 RFmxSpecAnMXAcpOffsetSideband.Both,
                                                                 RFmxSpecAnMXAcpOffsetEnabled.True);
                        specAcp[Iteration].Acp.Configuration.ConfigureOffsetPowerReference(selectorString,
                                                                               RFmxSpecAnMXAcpOffsetPowerReferenceCarrier.Closest,
                                                                               0);
                        //specAnAcpSignal[Iteration].Acp.Configuration.ConfigureOffsetRelativeAttenuation(selectorString,
                        //                                                            0.0);
                        specAcp[Iteration].Acp.Configuration.ConfigureOffsetRrcFilter(selectorString, RFmxSpecAnMXAcpOffsetRrcFilterEnabled.False, 0.22);
                    }
                    //_instrSession.SetDownconverterFrequencyOffset("", 0e6);
                }
                else
                {
                    double a = (Rbw == -1 ? 100e3 : Rbw);

                    specAcp[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, false);
                    specAcp[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", RefChBW);
                    specAcp[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    specAcp[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, (Rbw == -1 ? 100e3 : Rbw), RFmxSpecAnMXChpRbwFilterType.FftBased);

                    if (IqLength == 0)
                    {
                        specAcp[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);//Original
                    }
                    else
                    {
                        specAcp[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.False, IqLength);
                    }

                    specAcp[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.False, 1, RFmxSpecAnMXChpAveragingType.Rms);
                    //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                }

                // Set down freq commented out after upgraded to latest RFMx driver.
                Eq.Site[siteNo].RF.InstrSession.SetLOSource("", "Onboard");
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0e6); //200601 Mario
                specAcp[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specAcp[Iteration].Commit("");
            }
            public void CommitSpec(object Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", 0e6); // edward       
                specAcp[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }
            public void InitiateSpec(int Iteration)
            {
                specAcp[Convert.ToInt32(Iteration)].Initiate("", "");
           
            }
            public void WaitForAcq()
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public void RetrieveResults(int Iteration , ref AclrResults aclrResults)
            {

                RFmxResult _RFmxResult = new EqLib.EqRF.RFmxResult();
                double[] lowerAbsolutePowers = null, upperAbsolutePowers = null; double totalCarrierPower = 0f;
                double[] lowerRelativePowers = null;
                double[] upperRelativePowers = null;

                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                

                specAcp[Iteration].Acp.Results.FetchOffsetMeasurementArray("", 10, ref lowerRelativePowers, ref upperRelativePowers,
                            ref lowerAbsolutePowers, ref upperAbsolutePowers);
                totalCarrierPower = 999;
                specAcp[Iteration].Acp.Results.FetchTotalCarrierPower("", 10, out totalCarrierPower);


                aclrResults = new AclrResults();

                aclrResults.centerChannelPower = totalCarrierPower;

                for (int i = 0; i < lowerRelativePowers.Length; i++)
                {
                    AdjCh res = new AdjCh();

                    if (i == 2) res.Name = "E-ACLR";
                    else res.Name = "ACLR" + (i + 1).ToString();

                    res.lowerDbc = lowerRelativePowers[i];
                    res.upperDbc = upperRelativePowers[i];

                    aclrResults.adjacentChanPowers.Add(res);
                }


  
            }

            public double RetrieveResults(int Iteration)
            {
                double averageChannelPower = 0;
                double averageChannelPsd = 0;
                double relativeChannelPower = 0;

                try
                {
                    Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(5);
                    specAcp[Iteration].Chp.Results.FetchCarrierMeasurement("", 1, out averageChannelPower, out averageChannelPsd, out relativeChannelPower);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                return averageChannelPower;
            }
        }
        public class RfmxAcpNR
        {
            List<RFmxSpecAnMX> specAcpForNR_L;
            List<RFmxSpecAnMX> specAcpForNR_U;

            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double Freq;
            public byte siteNo;

            public RfmxAcpNR(byte site)
            {
                siteNo = site;
                specAcpForNR_L = new List<RFmxSpecAnMX>();
                specAcpForNR_U = new List<RFmxSpecAnMX>();
            }

            public void ConfigFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw, string PowerMode, double IqLength, int ACPaverage, byte site)
            {
                if (Rbw == 0) Rbw = 100e3;
                string selectorString;
                string test;
                int NumberOfCarriers = 1;
                double offset = 0;

                if (WaveformName.Contains("100M")) offset = 100;
                if (WaveformName.Contains("80M")) offset = 80;

                siteNo = site;
                string acptrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                //else offset = 10e6;

                //test = "ACPL" + Iteration.ToString();
                //specAcp.Insert(Iteration, NIVST_Rfmx.instrSession.GetSpecAnSignalConfiguration(test));
                //specAcp[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                //specAcp[Iteration].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                //specAcp[Iteration].ConfigureReferenceLevel("", Reflevel);
                //specAcp[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation      

                if (TestAcp)
                {

                    //if (WaveformName.Contains("NR") || RefChBW > 90e6)
                    {
                        test = "ACPL" + Iteration.ToString();
                        specAcpForNR_L.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                        specAcpForNR_L[Iteration].ConfigureFrequency("", (FreqSG - offset) * 1e6);
                        specAcpForNR_L[Iteration].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                        specAcpForNR_L[Iteration].ConfigureReferenceLevel("", Reflevel);
                        specAcpForNR_L[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation     
                        specAcpForNR_L[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Acp, false);

                        specAcpForNR_L[Iteration].Acp.Configuration.ConfigurePowerUnits("", RFmxSpecAnMXAcpPowerUnits.dBm);
                        specAcpForNR_L[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.FlatTop, 1);


                        specAcpForNR_L[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, 0.0003); //IqLength
                        specAcpForNR_L[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.FftBased);
                        specAcpForNR_L[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.False, 3, RFmxSpecAnMXAcpAveragingType.Rms);//true,ACPaverage
                        specAcpForNR_L[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);


                        for (int j = 0; j < NumberOfCarriers; j++)
                        {
                            selectorString = RFmxSpecAnMX.BuildCarrierString("", j);

                            specAcpForNR_L[Iteration].Acp.Configuration.ConfigureCarrierIntegrationBandwidth(selectorString,
                                                                                          RefChBW);
                            specAcpForNR_L[Iteration].Acp.Configuration.ConfigureCarrierMode(selectorString,
                                                                          RFmxSpecAnMXAcpCarrierMode.Active);
                            specAcpForNR_L[Iteration].Acp.Configuration.ConfigureCarrierRrcFilter("", RFmxSpecAnMXAcpCarrierRrcFilterEnabled.False, 0.22);
                            specAcpForNR_L[Iteration].Acp.Configuration.ConfigureCarrierFrequency(selectorString, 0);
                        }



                        test = "ACPU" + Iteration.ToString();
                        specAcpForNR_U.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                        specAcpForNR_U[Iteration].ConfigureFrequency("", (FreqSG + offset) * 1e6);
                        specAcpForNR_U[Iteration].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                        specAcpForNR_U[Iteration].ConfigureReferenceLevel("", Reflevel);
                        specAcpForNR_U[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation     
                        specAcpForNR_U[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Acp, false);

                        specAcpForNR_U[Iteration].Acp.Configuration.ConfigurePowerUnits("", RFmxSpecAnMXAcpPowerUnits.dBm);
                        specAcpForNR_U[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.FlatTop, 1);

                        specAcpForNR_U[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, 0.0003);
                        specAcpForNR_U[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.FftBased);

                        specAcpForNR_U[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.False, 3, RFmxSpecAnMXAcpAveragingType.Rms);
                        specAcpForNR_U[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);


                        for (int j = 0; j < NumberOfCarriers; j++)
                        {
                            selectorString = RFmxSpecAnMX.BuildCarrierString("", j);

                            specAcpForNR_U[Iteration].Acp.Configuration.ConfigureCarrierIntegrationBandwidth(selectorString,
                                                                                          RefChBW);
                            specAcpForNR_U[Iteration].Acp.Configuration.ConfigureCarrierMode(selectorString,
                                                                          RFmxSpecAnMXAcpCarrierMode.Active);
                            specAcpForNR_U[Iteration].Acp.Configuration.ConfigureCarrierRrcFilter("", RFmxSpecAnMXAcpCarrierRrcFilterEnabled.False, 0.22);
                            specAcpForNR_U[Iteration].Acp.Configuration.ConfigureCarrierFrequency(selectorString, 0);
                        }

                        Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0); //offset * 1e6
                        specAcpForNR_L[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                        specAcpForNR_L[Iteration].Commit("");

                        Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0); //-offset * 1e6
                        specAcpForNR_U[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                        specAcpForNR_U[Iteration].Commit("");

                    }
                }
            }

            public void CommitSpec(object Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", 0e6);

                specAcpForNR_L[Convert.ToInt32(Iteration)].Commit("");
                specAcpForNR_U[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }

            public void InitiateSpec(int Iteration)
            {
                specAcpForNR_L[Convert.ToInt32(Iteration)].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                specAcpForNR_U[Convert.ToInt32(Iteration)].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void WaitForAcq()
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }

            public void RetrieveResults(int Iteration, ref AclrResults aclrResults)
            {

                double EUTRA_L = 0f;
                double EUTRA_H = 0f;

                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);

                specAcpForNR_L[Iteration].Acp.Results.FetchTotalCarrierPower("", 10, out EUTRA_L);

                specAcpForNR_U[Iteration].Acp.Results.FetchTotalCarrierPower("", 10, out EUTRA_H);


                aclrResults.adjacentChanPowers.RemoveAt(2);

                for (int i = 0; i < 1; i++)
                {
                    AdjCh res = new AdjCh();

                    res.Name = "E-ACLR";
                    res.lowerDbc = EUTRA_L - aclrResults.centerChannelPower;
                    res.upperDbc = EUTRA_H - aclrResults.centerChannelPower;


                    aclrResults.adjacentChanPowers.Add(res);
                }


            }

            public double RetrieveResults(int Iteration)
            {
                double averageChannelPower = 0;
                double averageChannelPsd = 0;
                double relativeChannelPower = 0;

                try
                {
                    Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                    specAcpForNR_L[Iteration].Chp.Results.FetchCarrierMeasurement("", 1, out averageChannelPower, out averageChannelPsd, out relativeChannelPower);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                return averageChannelPower;
            }
        }
        public class RfmxChp
        {
            List<RFmxSpecAnMX> specCHP;
            //RFmxInstrMX _instrSession;
            double averagePower, psd, relativePower, timeout;  // added by hosein 12/29/2019
            public byte siteNo;

            public RfmxChp(byte site)
            {
                siteNo = site;
                specCHP = new List<RFmxSpecAnMX>();
            }
            public int Iteration;
            public bool Initialize(bool FinalScript)
            {
                return false;
            }

            //ConfigureSpec(c.Iteration, c.Freq, c.Reflevel, c.RefChBW, c.WaveformName, c.Waveform, c.Rbw, c.PowerMode, c.Site)
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, string WaveformName, string Waveform, double Rbw, string PowerMode, byte Site)
            {
                siteNo = Site;
                string ChpTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                if (Rbw == 0) Rbw = 100e3;
                string test;

                test = "CHP" + Iteration.ToString();

                //specCHP.Insert(Iteration, NIVST_Rfmx.instrSession.GetSpecAnSignalConfiguration(test));  removed by hosein  12/29/2019
                specCHP.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));  // added by hosein 12/29/2019
                specCHP[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                specCHP[Iteration].ConfigureDigitalEdgeTrigger("", ChpTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specCHP[Iteration].ConfigureReferenceLevel("", Reflevel);
                specCHP[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation      


                specCHP[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true); 
                specCHP[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", RefChBW); 
                specCHP[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1); 
                specCHP[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, (Rbw == -1 ? 100e3 : Rbw), RFmxSpecAnMXChpRbwFilterType.FftBased); 
                specCHP[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001); 
                specCHP[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.False, 1, RFmxSpecAnMXChpAveragingType.Rms);
                //specCHP[Iteration].Initiate("","");  //added by hosein

                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0e6);
                specCHP[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specCHP[Iteration].Commit("");  
                
            }
            public void CommitSpec(int Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specCHP[Iteration].Commit("");
            }
            public void InitiateSpec(int Iteration)
            {
                //specCHP[Iteration].Initiate("","");
                specCHP[Convert.ToInt32(Iteration)].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout); 
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public double RetrieveResults(int Iteration)
            {
                Spectrum<float> spectrum = null;
                timeout = 30;  //30 hosein 02102021
                specCHP[Iteration].Chp.Results.FetchSpectrum("", timeout , ref spectrum);
                specCHP[Iteration].Chp.Results.FetchCarrierMeasurement("", timeout , out averagePower, out psd, out relativePower);
                return averagePower;
            }
        }
        public class RfmxIQ
        {
            List<RFmxSpecAnMX> specIQ;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double Freq;
            public byte siteNo;

            public RfmxIQ(byte site)
            {
                siteNo = site;
                specIQ = new List<RFmxSpecAnMX>();
            }

            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw, string PowerMode, byte site)
            {
                siteNo = site;
                string IqTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;

                test = "IQ" + Iteration.ToString();
                specIQ.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                specIQ[Iteration].ConfigureRF("", FreqSG * 1e6, Reflevel, 0);
                specIQ[Iteration].ConfigureDigitalEdgeTrigger("", IqTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specIQ[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.IQ, false);

                specIQ[Iteration].IQ.Configuration.ConfigureAcquisition("", Convert.ToDouble(IQ.Mem["PINSWEEP"].VsaIQrate), 1, Convert.ToDouble(IQ.Mem["PINSWEEP"].SamplesPerRecord) / Convert.ToDouble(IQ.Mem["PINSWEEP"].VsaIQrate), 0);
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", -40e6);
                specIQ[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specIQ[Iteration].Commit("");
            }
            public void CommitSpec(object Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);

                //specIQ[Convert.ToInt16(Iteration)].ConfigureFrequency("", Freq);
                //specIQ[Convert.ToInt16(Iteration)].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);

                //specIQ[Convert.ToInt16(Iteration)].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);

                specIQ[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }
            public void InitiateSpec(object Iteration)
            {
                specIQ[Convert.ToInt32(Iteration)].Initiate("", "");
            }
            public void SetFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }

            public void Wait()
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public niComplexNumber[] RetrieveResults(double LossPout, int IterationIQ)
            {
                //_instrSession.WaitForAcquisitionComplete(Timeout);

                ComplexWaveform<ComplexSingle> data = null;

                specIQ[IterationIQ].IQ.Results.FetchData("", 100, 0, (int)IQ.Mem["PINSWEEP"].SamplesPerRecord, ref data);


                niComplexNumber[] resultIQ = new niComplexNumber[IQ.Mem["PINSWEEP"].SamplesPerRecord];

                double[] Real = data.GetRealDataArray(false);
                double[] Imaginary = data.GetImaginaryDataArray(false);

                double externalAttenLinear = Math.Sqrt(Math.Pow(10, (-LossPout / 10)));

                for (int i = 0; i < IQ.Mem["PINSWEEP"].SamplesPerRecord; i++)
                {
                    resultIQ[i].Real = Real[i] * externalAttenLinear;
                    resultIQ[i].Imaginary = Imaginary[i] * externalAttenLinear;
                }

                //double Lin_mag = 0f;
                //double[] Log_mag_result = new double[IQ.Mem["PINSWEEP"].SamplesPerRecord];
                //double[] Lin_mag_result = new double[IQ.Mem["PINSWEEP"].SamplesPerRecord];
                //double Average_calculation = 0f;
                //double calculation = 0f;

                //double[] ffj = new double[IQ.Mem["PINSWEEP"].SamplesPerRecord];
                //for (int i = 0; i < IQ.Mem["PINSWEEP"].SamplesPerRecord; i++)
                //{
                //    resultIQ[i].Real = Real[i] * externalAttenLinear;
                //    ffj[i] = Real[i] * externalAttenLinear;
                //    resultIQ[i].Imaginary = Imaginary[i] * externalAttenLinear;
                //    Lin_mag = System.Math.Sqrt(System.Math.Pow(resultIQ[i].Real, 2.0) + System.Math.Pow(resultIQ[i].Imaginary, 2.0));
                //    calculation = Lin_mag / System.Math.Sqrt(2);
                //    Average_calculation = ((calculation * calculation) * 1000) / 50;
                //    Log_mag_result[i] = (10 * (Math.Log10(Average_calculation)));
                //}


                return resultIQ;
            }
        }

        public class RfmxIQ_EVM
        {
            List<RFmxSpecAnMX> specIQ_EVM;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double carrierBandwidth = 100e6; /* (Hz) */
            public double Freq;
            public byte siteNo;

            public RfmxIQ_EVM(byte site)
            {
                siteNo = site;
                specIQ_EVM = new List<RFmxSpecAnMX>();
            }

            public bool Initialize(bool FinalScript)
            {
                return false;
            }

            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double VSA_SamplingRateHz, double VSA_AcquisitionTimeSec, string WaveformName, byte site)
            {
                siteNo = site;
                string IqTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;

                test = "IQ_EVM" + Iteration.ToString();

                if (WaveformName.Contains("B5M")) this.carrierBandwidth = 5e6;
                else if (WaveformName.Contains("B10M")) this.carrierBandwidth = 10e6;
                else if (WaveformName.Contains("B20M")) this.carrierBandwidth = 20e6;
                else if (WaveformName.Contains("B30M")) this.carrierBandwidth = 30e6;
                else if (WaveformName.Contains("B40M")) this.carrierBandwidth = 40e6;
                else if (WaveformName.Contains("B50M")) this.carrierBandwidth = 50e6;
                else if (WaveformName.Contains("B80M")) this.carrierBandwidth = 80e6;
                else this.carrierBandwidth = 100e6;

                specIQ_EVM.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                specIQ_EVM[Iteration].ConfigureRF("", FreqSG * 1e6, Reflevel, 0);
                specIQ_EVM[Iteration].ConfigureDigitalEdgeTrigger("", IqTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specIQ_EVM[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.IQ, false);

                //specIQ[Iteration].IQ.Configuration.GetSampleRate("", out double VSA_SamplingRateHz); // [BurhanEVM]
                //specIQ[Iteration].IQ.Configuration.GetAcquisitionTime("", out double VSA_AcquisitionTimeSec);// [BurhanEVM]
                double FinalAcqTime = VSA_AcquisitionTimeSec * 1.2; // Add additional 10% of the waveform for end header extraction
                specIQ_EVM[Iteration].IQ.Configuration.ConfigureAcquisition("", VSA_SamplingRateHz, 1, FinalAcqTime, 0);
                //specIQ_EVM[Iteration].IQ.Configuration.ConfigureAcquisition("", VSA_SamplingRateHz, 1, FinalAcqTime, 1e-8);
                specIQ_EVM[Iteration].IQ.Configuration.ConfigureBandwidth("", RFmxSpecAnMXIQBandwidthAuto.False, carrierBandwidth);

                //_instrSession.SetOptimizePathForSignalBandwidth("", RFmxInstrMXOptimizePathForSignalBandwidth.Enabled);
                //_instrSession.SetRFAttenuationAuto("", RFmxInstrMXRFAttenuationAuto.True);
                //_instrSession.SetOptimizePathForSignalBandwidth("", RFmxInstrMXOptimizePathForSignalBandwidth.Enabled);

                //RFmxInstrMXPreampEnabled xxxx;
                //_instrSession.GetPreampEnabled("", out xxxx);
                //_instrSession.SetPreampEnabled("", RFmxInstrMXPreampEnabled.Automatic);

                //specIQ_EVM[Iteration].IQ.Configuration.SetDeleteRecordOnFetch("", RFmxSpecAnMXIQDeleteRecordOnFetch.True); // Seems no improvement observed !!!

                //_instrSession.SetLOInjectionSide("", RFmxInstrMXLOInjectionSide.HighSide); // NI erro pops out !!!
                //_instrSession.SetCleanerSpectrum("", RFmxInstrMXCleanerSpectrum.Enabled); // NI erro pops out !!!
                //_instrSession.SetDigitizerDitherEnabled("", RFmxInstrMXDigitizerDitherEnabled.Enabled); // Make repeatibility worse !!!
                //_instrSession.SetTuningSpeed("", RFmxInstrMXTuningSpeed.Normal); // NI erro pops out !!!

                //_instrSession.SetDownconverterFrequencyOffset("", -40e6); // To make sure the Lo leakage of the instrument is not in the frequency range of the measurement
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0e6); // To make sure the Lo leakage of the instrument is not in the frequency range of the measurement
                Eq.Site[siteNo].RF.InstrSession.SetLOSource("", "LO_In");
                specIQ_EVM[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specIQ_EVM[Iteration].Commit("");
            }

            public void CommitSpec(object Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);

                //specIQ[Convert.ToInt16(Iteration)].ConfigureFrequency("", Freq);
                //specIQ[Convert.ToInt16(Iteration)].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);

                //specIQ[Convert.ToInt16(Iteration)].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);

                specIQ_EVM[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }
            public void InitiateSpec(object Iteration)
            {
                specIQ_EVM[Convert.ToInt32(Iteration)].Initiate("", "");
            }
            public void SetFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }

            public void Set_ReferenceLevel(double RefPower) // [BurhanEVM - Added]
            {
                specIQ_EVM[Convert.ToInt32(Iteration)].SetReferenceLevel("", RefPower);
            }

            public niComplexNumber[] RetrieveResults(double LossPout, double VSA_SamplingRateHz, double FinalAcqTime)
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(1); // [Burhan]

                ComplexWaveform<ComplexSingle> data = null;

                //specIQ[Iteration].IQ.Configuration.ConfigureAcquisition("", VSA_SamplingRateHz, 1, FinalAcqTime, 0);

                specIQ_EVM[Iteration].IQ.Results.FetchData("", 100, 0, -1, ref data); // A value of -1 specifies that RFmx fetches all samples.

                //niComplexNumber[] resultIQ = new niComplexNumber[IQ.Mem["PINSWEEP"].SamplesPerRecord];
                niComplexNumber[] resultIQ = new niComplexNumber[data.SampleCount];

                double[] Real = data.GetRealDataArray(false);
                double[] Imaginary = data.GetImaginaryDataArray(false);

                double externalAttenLinear = Math.Sqrt(Math.Pow(10, (-LossPout / 10)));

                //for (int i = 0; i < IQ.Mem["PINSWEEP"].SamplesPerRecord; i++)
                for (int i = 0; i < data.SampleCount; i++)
                {
                    resultIQ[i].Real = Real[i] * externalAttenLinear;
                    resultIQ[i].Imaginary = Imaginary[i] * externalAttenLinear;
                }

                return resultIQ;
            }

            public niComplexNumber[,] RetrieveResults_Multiple(double LossPout, int TotalTraces)
            {
                int Number_Of_IQ_Trace = TotalTraces;
                ComplexWaveform<ComplexSingle>[] Data_IQ = new ComplexWaveform<ComplexSingle>[Number_Of_IQ_Trace];

                for (int i = 0; i < Number_Of_IQ_Trace; i++)
                {
                    specIQ_EVM[Iteration].Initiate("", "");
                    Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(1);
                    specIQ_EVM[Iteration].IQ.Results.FetchData("", 10, 0, -1, ref Data_IQ[i]); // A value of -1 specifies that RFmx fetches all samples.
                }

                niComplexNumber[,] resultIQ = new niComplexNumber[Number_Of_IQ_Trace, Data_IQ[0].SampleCount];
                double externalAttenLinear = Math.Sqrt(Math.Pow(10, (-LossPout / 10)));

                for (int i = 0; i < Number_Of_IQ_Trace; i++)
                {
                    double[] Real = Data_IQ[i].GetRealDataArray(false);
                    double[] Imaginary = Data_IQ[i].GetImaginaryDataArray(false);

                    for (int j = 0; j < Data_IQ[0].SampleCount; j++)
                    {
                        resultIQ[i, j].Real = Real[j] * externalAttenLinear;
                        resultIQ[i, j].Imaginary = Imaginary[j] * externalAttenLinear;
                    }
                }
                return resultIQ;
            }

            public niComplexNumber[] RetrieveResults(double LossPout)
            {
                //Thread.Sleep(200);

                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(1); // [Burhan]

                //specIQ[Iteration].WaitForMeasurementComplete("",1);

                ComplexWaveform<ComplexSingle> data = null;
                ComplexWaveform<ComplexSingle> data2 = null;
                ComplexWaveform<ComplexSingle> data3 = null;
                ComplexWaveform<ComplexSingle> data4 = null;
                ComplexWaveform<ComplexSingle> data5 = null;

                specIQ_EVM[Iteration].Initiate("", "");
                specIQ_EVM[Iteration].IQ.Results.FetchData("", 10, 0, -1, ref data); // A value of -1 specifies that RFmx fetches all samples.

                specIQ_EVM[Iteration].Initiate("", "");
                specIQ_EVM[Iteration].IQ.Results.FetchData("", 10, 0, -1, ref data2); // A value of -1 specifies that RFmx fetches all samples.

                specIQ_EVM[Iteration].Initiate("", "");
                specIQ_EVM[Iteration].IQ.Results.FetchData("", 10, 0, -1, ref data3); // A value of -1 specifies that RFmx fetches all samples.

                specIQ_EVM[Iteration].Initiate("", "");
                specIQ_EVM[Iteration].IQ.Results.FetchData("", 10, 0, -1, ref data4); // A value of -1 specifies that RFmx fetches all samples.

                specIQ_EVM[Iteration].Initiate("", "");
                specIQ_EVM[Iteration].IQ.Results.FetchData("", 10, 0, -1, ref data5); // A value of -1 specifies that RFmx fetches all samples.

                niComplexNumber[] resultIQ = new niComplexNumber[data.SampleCount];

                double[] Real = data.GetRealDataArray(false);
                double[] Imaginary = data.GetImaginaryDataArray(false);

                double externalAttenLinear = Math.Sqrt(Math.Pow(10, (-LossPout / 10)));

                for (int i = 0; i < data.SampleCount; i++)
                {
                    resultIQ[i].Real = Real[i] * externalAttenLinear;
                    resultIQ[i].Imaginary = Imaginary[i] * externalAttenLinear;
                }
                return resultIQ;
            }
        }

        public class RfmxIQ_Timing
        {
            List<RFmxSpecAnMX> specCHP_Timing;
            List<RFmxSpecAnMX> specIQ_Timing;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double Freq;
            public byte siteNo;

            public RfmxIQ_Timing(byte site)
            {
                siteNo = site;
                specCHP_Timing = new List<RFmxSpecAnMX>();
                specIQ_Timing = new List<RFmxSpecAnMX>();
            }

            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double SampleRate, double AcquisitionTime, double TriggerDelay , byte site)
            {
                siteNo = site;
                string ChpTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);
                string IqTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][2]);

                string test;


                test = "CHP_Timing" + Iteration.ToString();
                specCHP_Timing.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                specCHP_Timing[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                specCHP_Timing[Iteration].ConfigureDigitalEdgeTrigger("", ChpTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specCHP_Timing[Iteration].ConfigureReferenceLevel("", Reflevel);
                specCHP_Timing[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation      


                specCHP_Timing[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, false);
                specCHP_Timing[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 100e3);
                specCHP_Timing[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                specCHP_Timing[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 100e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                specCHP_Timing[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                specCHP_Timing[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.False, 1, RFmxSpecAnMXChpAveragingType.Rms);


                test = "IQ_Timing" + Iteration.ToString();
                specIQ_Timing.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                specIQ_Timing[Iteration].ConfigureRF("", FreqSG * 1e6, Reflevel, 0);
                specIQ_Timing[Iteration].ConfigureDigitalEdgeTrigger("", IqTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specIQ_Timing[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.IQ, false);

                specIQ_Timing[Iteration].IQ.Configuration.ConfigureAcquisition("", Convert.ToDouble(IQ.Mem["PINSWEEP"].VsaIQrate), 1, AcquisitionTime, 0);
                specIQ_Timing[Iteration].SetTriggerDelay("", TriggerDelay);

            }
            public void CommitSpec_ForServo(object Iteration)
            {
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0e6);
            
                specCHP_Timing[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }
            public void CommitSpec(object Iteration)
            {
                string IqTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][2]);

                //  _instrSession.SetDownconverterFrequencyOffset("", -0e6);
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                specIQ_Timing[Convert.ToInt32(Iteration)].ConfigureFrequency("", Freq);
                specIQ_Timing[Convert.ToInt32(Iteration)].ConfigureDigitalEdgeTrigger("", IqTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specIQ_Timing[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }
            public void InitiateSpec_ForServo(object Iteration)
            {
                specCHP_Timing[Convert.ToInt32(Iteration)].Initiate("", "");
            }

            public void InitiateSpec(object Iteration)
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                specIQ_Timing[Convert.ToInt32(Iteration)].Initiate("", "");
            }

            public void SetFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public niComplexNumber[] RetrieveResults(double LossPout, int NumberOfSamples)
            {
                //_instrSession...WaitForAcquisitionComplete(Timeout);

                ComplexWaveform<ComplexSingle> data = null;

                specIQ_Timing[Iteration].IQ.Results.FetchData("", 1, 0, NumberOfSamples, ref data);


                niComplexNumber[] resultIQ = new niComplexNumber[NumberOfSamples];

                double[] Real = data.GetRealDataArray(false);
                double[] Imaginary = data.GetImaginaryDataArray(false);

                double externalAttenLinear = Math.Sqrt(Math.Pow(10, (-LossPout / 10)));

                for (int i = 0; i < NumberOfSamples; i++)
                {
                    resultIQ[i].Real = Real[i] * externalAttenLinear;
                    resultIQ[i].Imaginary = Imaginary[i] * externalAttenLinear;
                }

                double Lin_mag = 0f;
                double[] Log_mag_result = new double[NumberOfSamples];
                double[] Lin_mag_result = new double[NumberOfSamples];
                double Average_calculation = 0f;
                double calculation = 0f;

                double[] ffj = new double[NumberOfSamples];
                for (int i = 0; i < NumberOfSamples; i++)
                {
                    resultIQ[i].Real = Real[i] * externalAttenLinear;
                    ffj[i] = Real[i] * externalAttenLinear;
                    resultIQ[i].Imaginary = Imaginary[i] * externalAttenLinear;
                    Lin_mag = System.Math.Sqrt(System.Math.Pow(resultIQ[i].Real, 2.0) + System.Math.Pow(resultIQ[i].Imaginary, 2.0));
                    calculation = Lin_mag / System.Math.Sqrt(2);
                    Average_calculation = ((calculation * calculation) * 1000) / 50;
                    Log_mag_result[i] = (10 * (Math.Log10(Average_calculation)));
                }


                return resultIQ;
            }
        }
        public class RfmxIIP3
        {
            List<RFmxSpecAnMX> specIIP3;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public byte siteNo;

            public RfmxIIP3(byte site)
            {
                siteNo = site;
                specIIP3 = new List<RFmxSpecAnMX>();
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, 
                bool TestAcp, int NumberOfOffsets, double Rbw, string PowerMode, string Band, byte site)
            {

                siteNo = site;
                string IIP3TrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;

                test = "IIP3" + Iteration.ToString();
                specIIP3.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));

                specIIP3[Iteration].ConfigureFrequency("", (FreqSG * 1e6));
                specIIP3[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation
                specIIP3[Iteration].ConfigureReferenceLevel("", Reflevel);
                specIIP3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.IM, false);

                //specIIP3[Iteration].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);

                if (PowerMode == "G5" || PowerMode == "G6")
                {
                    specIIP3[Iteration].IM.Configuration.ConfigureAveraging("", RFmxSpecAnMXIMAveragingEnabled.True, 10, RFmxSpecAnMXIMAveragingType.Rms);
                    specIIP3[Iteration].IM.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXIMRbwFilterAutoBandwidth.False, 5e3, RFmxSpecAnMXIMRbwFilterType.FftBased);
                    specIIP3[Iteration].IM.Configuration.ConfigureSweepTime("", RFmxSpecAnMXIMSweepTimeAuto.True, 0.005);
                }
                else if (PowerMode == "G3" || PowerMode == "G4" || ((PowerMode == "G0") && (Band =="B25")))
                {
                    specIIP3[Iteration].IM.Configuration.ConfigureAveraging("", RFmxSpecAnMXIMAveragingEnabled.True, 10, RFmxSpecAnMXIMAveragingType.Rms);
                    specIIP3[Iteration].IM.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXIMRbwFilterAutoBandwidth.False, 5e3, RFmxSpecAnMXIMRbwFilterType.FftBased);
                    specIIP3[Iteration].IM.Configuration.ConfigureSweepTime("", RFmxSpecAnMXIMSweepTimeAuto.True, 0.005);
                }   
                else
                {
                    specIIP3[Iteration].IM.Configuration.ConfigureAveraging("", RFmxSpecAnMXIMAveragingEnabled.False, 1, RFmxSpecAnMXIMAveragingType.Rms);
                    specIIP3[Iteration].IM.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXIMRbwFilterAutoBandwidth.False, 10e3, RFmxSpecAnMXIMRbwFilterType.FftBased);
                    specIIP3[Iteration].IM.Configuration.ConfigureSweepTime("", RFmxSpecAnMXIMSweepTimeAuto.True, 0.005);
                }

                
                specIIP3[Iteration].IM.Configuration.ConfigureFft("", RFmxSpecAnMXIMFftWindow.FlatTop, 1);
                specIIP3[Iteration].IM.Configuration.ConfigureFrequencyDefinition("", RFmxSpecAnMXIMFrequencyDefinition.Absolute);

                specIIP3[Iteration].IM.Configuration.ConfigureMeasurementMethod("", RFmxSpecAnMXIMMeasurementMethod.Normal);
                specIIP3[Iteration].IM.Configuration.ConfigureFundamentalTones("", ((FreqSG - 0.5) * 1e6), ((FreqSG + 0.5) * 1e6));
                specIIP3[Iteration].IM.Configuration.ConfigureAutoIntermodsSetup("", RFmxSpecAnMXIMAutoIntermodsSetupEnabled.True, 3);
                specIIP3[Iteration].IM.Configuration.SetLocalPeakSearchEnabled("", RFmxSpecAnMXIMLocalPeakSearchEnabled.False);

                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 10e6);
                specIIP3[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specIIP3[Iteration].Commit("");
                //  specIIP3[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.FrequencyAndReferenceLevel); ////////NI 160713 
            }
            public void CommitSpec(object Iteration)
            {
                //_instrSession.SetCleanerSpectrum("", RFmxInstrMXCleanerSpectrum.Disabled);
                //_instrSession.SetDownconverterFrequencyOffset("", 10e6);
                //specIIP3[Convert.ToInt32(Iteration)].ConfigureReferenceLevel("", -25);
                specIIP3[Convert.ToInt32(Iteration)].Commit("");
                Eq.Site[siteNo].RF.ThreadFlags[Convert.ToInt16(ThreadFlag.SA)].Set();
            }
            public void InitiateSpec(int Iteration)
            {
                specIIP3[Iteration].Initiate("", "");
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public void RetrieveResults(int Iteration, out double lowerTonePower, out double upperTonePower, ref double[] lowerIntermodPower, ref double[] upperIntermodPower, ref int[] intermodOrder)
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                specIIP3[Iteration].IM.Results.FetchFundamentalMeasurement("", 10, out lowerTonePower, out upperTonePower);
                specIIP3[Iteration].IM.Results.FetchIntermodMeasurementArray("", 10, ref intermodOrder, ref lowerIntermodPower, ref upperIntermodPower);
     
            }
        }
        public class RfmxHar2nd
        {
            List<RFmxSpecAnMX> specH2;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double FreqforHar;
            public byte siteNo;

            public RfmxHar2nd(byte site)
            {
                siteNo = site;
                specH2 = new List<RFmxSpecAnMX>();
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, string Waveform, bool TestAcp, int NumberOfOffsets, double Rbw, string PowerMode, double IqLength, int ACPaverage, byte Site)
            {
                siteNo = Site;
                string h2TrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;
                double SAPeakLB = -15; //-30
                test = "H2" + Iteration.ToString();

                if (WaveformName.Contains("TS01"))
                {
                    SAPeakLB = -15; //-30
                    specH2.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));

                    specH2[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 2);
                    specH2[Iteration].ConfigureDigitalEdgeTrigger("", h2TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH2[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH2[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH2[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", 20e6);
                    specH2[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH2[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH2[Iteration].Txp.Configuration.SetAveragingCount("", 2);

                    ////Original
                    //specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true);
                    //specH2[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 20e6);
                    //specH2[Iteration].Chp.Configuration.ConfigureSpan("", 20e6);
                    //specH2[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    //specH2[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 10e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                    //specH2[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                }
                else if (WaveformName.Contains("M1RB"))
                {
                    FreqSG = HarmonicFreq(Waveform, FreqSG); //Harmonic Freq calculation for 1RB Signal //Yoonchun 

                    //Before
                    //if (FreqSGForHarmonic * 2 > 6000)
                    //{
                    //    double rfExtddRxOutFreq = 0;                  

                    //    Eq.Site[Site].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 2, 1, out rfExtddRxOutFreq);
                    //    specH2[Iteration].ConfigureFrequency("", rfExtddRxOutFreq);
                    //}
                    //else
                    //{
                    //    specH2[Iteration].ConfigureFrequency("", (FreqSGForHarmonic * 1e6) * 2);
                    //}

                    SAPeakLB = -10; //-30

                    specH2.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                    specH2[Iteration].ConfigureDigitalEdgeTrigger("", h2TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH2[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH2[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    if (FreqSG * 2 > 6000)
                    {
                        double rfExtddRxOutFreq = 0;
                        double FreqSGForHarmonic_over6Ghz = 0;
                        double H2FreqOffset1rb = 0;
                        double totalRB = 0;

                        if ((Waveform.Contains("5M1RB")))
                        {
                            totalRB = 25;
                        }
                        else if ((Waveform.Contains("10M1RB")))
                        {
                            totalRB = 50;
                        }

                        // 1 rb = 180KHz
                        // H2FreqOffset1rb = TotalOccupiedBW/2 - (0.18MHz/2) = (TotalRB * 0.18MHz) - 0.09MHz
                        // For 10MHz waveform with 50 RB the value should be:
                        // H3FreqOffset1rb = 4.41 * 3 = 13.23 MHz(Previously it was hard coded as 13.2 MHz)
                        // H2FreqOffset1rb = 4.41 * 2 = 8.82 MHz(Previously it was hard coded as 8.8 MHz)
                        H2FreqOffset1rb = ((totalRB * 0.18e6) / 2 - 0.09e6) * 2;

                        Eq.Site[Site].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 2, 1, out rfExtddRxOutFreq);

                        if (Waveform.Contains("10M1RB49S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq + H2FreqOffset1rb;
                        else if (Waveform.Contains("5M1RB24S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq + H2FreqOffset1rb;
                        else if (Waveform.Contains("10M1RB0S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq - H2FreqOffset1rb;
                        else if (Waveform.Contains("5M1RB0S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq - H2FreqOffset1rb;

                        specH2[Iteration].ConfigureFrequency("", FreqSGForHarmonic_over6Ghz);
                    }
                    else
                    {
                        specH2[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 2);
                    }

                    specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH2[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", ((Rbw != 0) ? Rbw : 1e6)); 
                    specH2[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH2[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH2[Iteration].Txp.Configuration.SetAveragingCount("", 2);

                    ////Original
                    //specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true);
                    //specH2[Iteration].Chp.Configuration.ConfigureSpan("", 2e6);
                    //specH2[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 1e6); //1MHz CHP measurement for 1RB Harmonic
                    //specH2[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    //specH2[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 10e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                    //specH2[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                    //specH2[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.True, 3, RFmxSpecAnMXChpAveragingType.Rms); //Added
                }
                else if (WaveformName.Contains("M12RB"))
                {

                    FreqSG = HarmonicFreq(Waveform, FreqSG); //Harmonic Freq calculation for 1RB Signal //Yoonchun 

                    SAPeakLB = -10; //-30

                    specH2.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                    specH2[Iteration].ConfigureDigitalEdgeTrigger("", h2TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH2[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH2[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    if (FreqSG * 2 > 6000)
                    {
                        double rfExtddRxOutFreq = 0;
                        double FreqSGForHarmonic_over6Ghz = 0;
                        double H2FreqOffset12rb = 0;
                        double totalRB = 0;

                        if ((Waveform.Contains("5M12RB")))
                        {
                            totalRB = 25;
                        }
                        else if ((Waveform.Contains("10M12RB")))
                        {
                            totalRB = 50;
                        }

                        // 1 rb = 180KHz
                        // FreqOffset = TotalOccupiedBW/2 - (0.18MHz/2*NumofRB) = (TotalRB * 0.18MHz) - (0.09MHz*NumofRB)
                        // For 10MHz waveform with 50 RB the value should be:
                        // H3FreqOffsetrb = FreqOffset * 3
                        // H2FreqOffsetrb = FreqOffset * 2
                        H2FreqOffset12rb = ((totalRB * 0.18e6) / 2 - (12*0.09e6)) * 2;

                        Eq.Site[Site].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 2, 1, out rfExtddRxOutFreq);

                        if (Waveform.Contains("10M12RB38S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq + H2FreqOffset12rb;
                        else if (Waveform.Contains("10M12RB0S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq - H2FreqOffset12rb;

                        specH2[Iteration].ConfigureFrequency("", FreqSGForHarmonic_over6Ghz);
                    }
                    else
                    {
                        specH2[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 2);
                    }

                    specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH2[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", ((Rbw != 0) ? Rbw : 1e6));
                    specH2[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH2[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH2[Iteration].Txp.Configuration.SetAveragingCount("", 2);
                }
                else
                {
                    //FreqSG = HarmonicFreq(Waveform, FreqSG); //Harmonic Freq calculation for 1RB Signal //Yoonchun //Removed by Hosein

                    //double FreqSGForHarmonic = HarmonicFreq(Waveform, FreqSG); //Harmonic Freq calculation for 1RB Signal //Yoonchun 
                    double FreqSGForHarmonic = FreqSG; //Added by Hosein
                    double DeltaFreq = (FreqSG - FreqSGForHarmonic) * 2;
                    FreqforHar = (FreqSGForHarmonic * 1e6) * 2;

                    specH2.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));  //added by Hosein

                    if (FreqSGForHarmonic * 2 > 6000)
                    {
                        double rfExtddRxOutFreq = 0;

                        Eq.Site[Site].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 2, 1, out rfExtddRxOutFreq);
                        specH2[Iteration].ConfigureFrequency("", rfExtddRxOutFreq);
                    }
                    else
                    {
                        specH2[Iteration].ConfigureFrequency("", (FreqSGForHarmonic * 1e6) * 2);
                    }


                    SAPeakLB = -30; //-20

                    specH2.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                    specH2[Iteration].ConfigureDigitalEdgeTrigger("", h2TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH2[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH2[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);

                    specH2[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH2[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", RefChBW);
                    specH2[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH2[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH2[Iteration].Txp.Configuration.SetAveragingCount("", 2);

                    //Original
                    //specH2[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true);
                    //specH2[Iteration].Chp.Configuration.ConfigureSpan("", 20e6);
                    //specH2[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", RefChBW);
                    //specH2[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    //specH2[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 10e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                    //specH2[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                    //specH2[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.True, 3, RFmxSpecAnMXChpAveragingType.Rms); //Added
                }
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", -40e6);
                specH2[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specH2[Iteration].Commit("");
            }
            //Ron - updated to support all rb numbers instead of just 1 rb.
            public double HarmonicFreq(string waveformName, double freqSG)
            {
                string bandWidth = "";
                string resourceBlock = "";
                string rbStart = "";
                double freq_Start = 0;
                double freq_Stop = 0;
                double increment = 0;
                double guardBand = 0;
                bool fullRBflag = false;
                double adjustedFreq = 0;

                string[] waveform_Array = waveformName.Split(new[] { "M", "RB"}, StringSplitOptions.None);
                bandWidth = waveform_Array[0];
                resourceBlock = waveform_Array[1];
                rbStart = waveform_Array[2].Remove(waveform_Array[2].IndexOf("S"));
                if (String.IsNullOrEmpty(rbStart))
                {
                    rbStart = "0";
                }

                guardBand = (Convert.ToDouble(bandWidth) * 0.1) / 2; //Resource Block is allocated with 90% LTE Channel BW except for LTE1p4M

                switch(bandWidth)
                {
                    case "1p4":
                    case "1.4":
                        if (resourceBlock == "6") 
                        { 
                            fullRBflag = true;
                            guardBand = 0.32 / 2; // Occupied BW of LTE1p4M is 1.08MHz
                        }
                        break;

                    case "3":
                        if (resourceBlock == "15") { fullRBflag = true; }
                        break;

                    case "5":
                        if (resourceBlock == "25") { fullRBflag = true; }
                        break;

                    case "10":
                        if (resourceBlock == "50") { fullRBflag = true; }
                        break;

                    case "15":
                        if (resourceBlock == "75") { fullRBflag = true; }
                        break;

                    case"20":
                        if (resourceBlock == "100") { fullRBflag = true; }
                        break;

                    default:
                        fullRBflag = false;
                        break;
                }
                
                if (fullRBflag == true)
                {
                    freq_Start = freqSG;
                }
                else
                {
                    increment = ((Convert.ToDouble(resourceBlock)) * 0.09);
                    freq_Start = freqSG - (Convert.ToDouble(bandWidth) / 2); //Start Freq of Measurment BW
                    freq_Stop = freqSG + (Convert.ToDouble(bandWidth) / 2); //Stop Freq of Measurment BW

                    if(Convert.ToDouble(rbStart) == 0)
                    {
                        adjustedFreq = freq_Start + guardBand + increment;
                    }
                    else
                    {
                        adjustedFreq = freq_Stop - guardBand - increment;
                    }
                }

                return adjustedFreq;
            }
            public void CommitSpec(int Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specH2[Convert.ToInt32(Iteration)].Commit("");
            }
            public void InitiateSpec(int Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specH2[Convert.ToInt32(Iteration)].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public double RetrieveResults(int Iteration)
            {
                double averageChannelPower = 0;

                specH2[Iteration].Txp.Results.GetAverageMeanPower("", out averageChannelPower);
                return averageChannelPower;
            }
            public double RetrieveResults_Peak(int Iteration)
            {
                double PeaksearchPower = 0;

                specH2[Iteration].Txp.Results.GetMaximumPower("", out PeaksearchPower);                
                return PeaksearchPower;
            }
        }
        public class RfmxHar3rd
        {
            List<RFmxSpecAnMX> specH3;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public byte siteNo;

            public RfmxHar3rd(byte site)
            {
                siteNo = site;
                specH3 = new List<RFmxSpecAnMX>();
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, string Waveform, bool TestAcp, int NumberOfOffsets,
                double Rbw, string PowerMode, double IqLength, int ACPaverage, byte site)
            {
                string test;
                double SAPeakLB = -30;

                siteNo = site;
                string H3TrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                test = "H3" + Iteration.ToString();
                if (WaveformName.Contains("TS01"))
                {
                    SAPeakLB = -30;
                    specH3.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));

                    specH3[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 3);
                    specH3[Iteration].ConfigureDigitalEdgeTrigger("", H3TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH3[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH3[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH3[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", 20e6);
                    specH3[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH3[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH3[Iteration].Txp.Configuration.SetAveragingCount("", 2);

                    ////Original
                    //specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true);
                    //specH3[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 20e6);
                    //specH3[Iteration].Chp.Configuration.ConfigureSpan("", 20e6);
                    //specH3[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    //specH3[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 10e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                    //specH3[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                }                                    
                else if(WaveformName.Contains("M1RB"))
                {
                    FreqSG = HarmonicFreq(Waveform, FreqSG); //Harmonic Freq calculation for 1RB Signal //Yoonchun 

                    SAPeakLB = -30; //-20

                    specH3.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                    specH3[Iteration].ConfigureDigitalEdgeTrigger("", H3TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH3[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH3[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    if (FreqSG * 3 > 6000)
                    {
                        double rfExtddRxOutFreq = 0;
                        double FreqSGForHarmonic_over6Ghz = 0;
                        double H3FreqOffset1rb = 0;
                        double totalRB = 0;

                        if ((Waveform.Contains("5M1RB")))
                        {
                            totalRB = 25;
                        }
                        else if ((Waveform.Contains("10M1RB")))
                        {
                            totalRB = 50;
                        }

                        // 1 rb = 180KHz
                        // H2FreqOffset1rb = TotalOccupiedBW/2 - (0.18MHz/2) = (TotalRB * 0.18MHz) - 0.09MHz
                        // For 10MHz waveform with 50 RB the value should be:
                        // H3FreqOffset1rb = 4.41 * 3 = 13.23 MHz(Previously it was hard coded as 13.2 MHz)
                        // H2FreqOffset1rb = 4.41 * 2 = 8.82 MHz(Previously it was hard coded as 8.8 MHz)
                        H3FreqOffset1rb = ((totalRB * 0.18e6) / 2 - 0.09e6) * 3;

                        Eq.Site[siteNo].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 3, 1, out rfExtddRxOutFreq);

                        if (Waveform.Contains("10M1RB49S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq + H3FreqOffset1rb;
                        else if (Waveform.Contains("5M1RB24S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq + H3FreqOffset1rb;
                        else if (Waveform.Contains("10M1RB0S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq - H3FreqOffset1rb;
                        else if (Waveform.Contains("5M1RB0S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq - H3FreqOffset1rb;

                        specH3[Iteration].ConfigureFrequency("", FreqSGForHarmonic_over6Ghz);
                    }
                    else
                    {
                        specH3[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 3);
                    }

                    specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH3[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", ((Rbw != 0) ? Rbw : 1e6));
                    specH3[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH3[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH3[Iteration].Txp.Configuration.SetAveragingCount("", 2);

                    ////Original
                    //specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true);
                    //specH3[Iteration].Chp.Configuration.ConfigureSpan("", 2e6);
                    //specH3[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 1e6); //1MHz CHP measurement for 1RB Harmonic
                    //specH3[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    //specH3[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 10e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                    //specH3[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                    //specH3[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.True, 3, RFmxSpecAnMXChpAveragingType.Rms); //Added
                }
                else if (WaveformName.Contains("M12RB"))
                {
                    FreqSG = HarmonicFreq(Waveform, FreqSG); //Harmonic Freq calculation for 1RB Signal //Yoonchun 

                    SAPeakLB = -10; //-30

                    specH3.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                    specH3[Iteration].ConfigureDigitalEdgeTrigger("", H3TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH3[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH3[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    if (FreqSG * 3 > 6000)
                    {
                        double rfExtddRxOutFreq = 0;
                        double FreqSGForHarmonic_over6Ghz = 0;
                        double H3FreqOffset12rb = 0;
                        double totalRB = 0;

                        if ((Waveform.Contains("5M12RB")))
                        {
                            totalRB = 25;
                        }
                        else if ((Waveform.Contains("10M12RB")))
                        {
                            totalRB = 50;
                        }

                        // 1 rb = 180KHz
                        // FreqOffset = TotalOccupiedBW/2 - (0.18MHz/2*NumofRB) = (TotalRB * 0.18MHz) - (0.09MHz*NumofRB)
                        // For 10MHz waveform with 50 RB the value should be:
                        // H3FreqOffsetrb = FreqOffset * 3
                        // H2FreqOffsetrb = FreqOffset * 2
                        H3FreqOffset12rb = ((totalRB * 0.18e6) / 2 - (12 * 0.09e6)) * 3;

                        Eq.Site[siteNo].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 3, 1, out rfExtddRxOutFreq);

                        if (Waveform.Contains("10M12RB38S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq + H3FreqOffset12rb;
                        else if (Waveform.Contains("10M12RB0S")) FreqSGForHarmonic_over6Ghz = rfExtddRxOutFreq - H3FreqOffset12rb;

                        specH3[Iteration].ConfigureFrequency("", FreqSGForHarmonic_over6Ghz);
                    }
                    else
                    {
                        specH3[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 3);
                    }

                    specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH3[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", ((Rbw != 0) ? Rbw : 1e6));
                    specH3[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH3[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH3[Iteration].Txp.Configuration.SetAveragingCount("", 2);
                }
                else
                {
                    SAPeakLB = -30; //-20
            
                    specH3.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));

                    if (FreqSG * 3 > 6000)
                    {
                        double rfExtddRxOutFreq = 0;

                        Eq.Site[siteNo].RF.RFExtd.ConfigureHarmonicConverter((FreqSG * 1e6) * 3, 1, out rfExtddRxOutFreq);
                        specH3[Iteration].ConfigureFrequency("", rfExtddRxOutFreq);
                    }
                    else
                    {
                        specH3[Iteration].ConfigureFrequency("", (FreqSG * 1e6) * 3);
                    }

                    specH3[Iteration].ConfigureDigitalEdgeTrigger("", H3TrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                    specH3[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                    specH3[Iteration].ConfigureReferenceLevel("", SAPeakLB);

                    specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                    specH3[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                    specH3[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", RefChBW);
                    specH3[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.True);
                    specH3[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                    specH3[Iteration].Txp.Configuration.SetAveragingCount("", 2);

                    //specH3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, true);
                    //specH3[Iteration].Chp.Configuration.ConfigureSpan("", 20e6);
                    //specH3[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", RefChBW);
                    //specH3[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                    //specH3[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 10e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                    //specH3[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                    //specH3[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.True, 3, RFmxSpecAnMXChpAveragingType.Rms); //Added
                }
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", -40e6);
                specH3[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specH3[Iteration].Commit("");
            }
            //Ron - updated to support all rb numbers instead of 1 rb.
            public double HarmonicFreq(string waveformName, double freqSG)
            {
                string bandWidth = "";
                string resourceBlock = "";
                string rbStart = "";
                double freq_Start = 0;
                double freq_Stop = 0;
                double increment = 0;
                double guardBand = 0;
                bool fullRBflag = false;
                double adjustedFreq = 0;

                string[] waveform_Array = waveformName.Split(new[] { "M", "RB" }, StringSplitOptions.None);
                bandWidth = waveform_Array[0];
                resourceBlock = waveform_Array[1];
                rbStart = waveform_Array[2].Remove(waveform_Array[2].IndexOf("S"));
                if (String.IsNullOrEmpty(rbStart))
                {
                    rbStart = "0";
                }

                guardBand = (Convert.ToDouble(bandWidth) * 0.1) / 2; //Resource Block is allocated with 90% LTE Channel BW except for LTE1p4M

                switch (bandWidth)
                {
                    case "1p4":
                    case "1.4":
                        if (resourceBlock == "6")
                        {
                            fullRBflag = true;
                            guardBand = 0.32 / 2; // Occupied BW of LTE1p4M is 1.08MHz
                        }
                        break;

                    case "3":
                        if (resourceBlock == "15") { fullRBflag = true; }
                        break;

                    case "5":
                        if (resourceBlock == "25") { fullRBflag = true; }
                        break;

                    case "10":
                        if (resourceBlock == "50") { fullRBflag = true; }
                        break;

                    case "15":
                        if (resourceBlock == "75") { fullRBflag = true; }
                        break;

                    case "20":
                        if (resourceBlock == "100") { fullRBflag = true; }
                        break;

                    default:
                        fullRBflag = false;
                        break;
                }

                if (fullRBflag == true)
                {
                    freq_Start = freqSG;
                }
                else
                {
                    increment = ((Convert.ToDouble(resourceBlock)) * 0.09);
                    freq_Start = freqSG - (Convert.ToDouble(bandWidth) / 2); //Start Freq of Measurment BW
                    freq_Stop = freqSG + (Convert.ToDouble(bandWidth) / 2); //Stop Freq of Measurment BW

                    if (Convert.ToDouble(rbStart) == 0)
                    {
                        adjustedFreq = freq_Start + guardBand + increment;
                    }
                    else
                    {
                        adjustedFreq = freq_Stop - guardBand - increment;
                    }
                }

                return adjustedFreq;
            }
            public void CommitSpec(int Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specH3[Convert.ToInt32(Iteration)].Commit("");

            }
            public void InitiateSpec(int Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specH3[Convert.ToInt32(Iteration)].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public double RetrieveResults(int Iteration)
            {
                double averageChannelPower = 0;

                //Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                specH3[Iteration].Txp.Results.GetAverageMeanPower("", out averageChannelPower);

                return averageChannelPower;
            }
        }
        public class RfmxTxleakage
        {
            List<RFmxSpecAnMX> specTxleakage;
            //RFmxInstrMX _instrSession;
            public int Iteration;

            public byte siteNo;

            public RfmxTxleakage(byte site)
            {
                siteNo = site;
                specTxleakage = new List<RFmxSpecAnMX>();
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double SpanforTxL, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, bool TestAcp, int NumberOfOffsets,
                double Rbw, string PowerMode, double IqLength, int ACPaverage, byte site)
            {

                siteNo = site;
                string LeakTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;

                test = "Txleakage" + Iteration.ToString();
                specTxleakage.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                if (SpanforTxL == 15 && FreqSG == 1910) //mario hardcoding later fixed...
                {
                    FreqSG = 1922.5;
                }
                else if (SpanforTxL == 20 && FreqSG == 1780)
                { 
                    FreqSG = 1795;
                }
                specTxleakage[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                specTxleakage[Iteration].ConfigureDigitalEdgeTrigger("", LeakTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specTxleakage[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                specTxleakage[Iteration].ConfigureReferenceLevel("", Reflevel);

                specTxleakage[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Txp, false);
                specTxleakage[Iteration].Txp.Configuration.SetMeasurementInterval("", IqLength);
                specTxleakage[Iteration].Txp.Configuration.SetRbwFilterType("", RFmxSpecAnMXTxpRbwFilterType.None);
                specTxleakage[Iteration].Txp.Configuration.SetRbwFilterAlpha("", 0);
                specTxleakage[Iteration].Txp.Configuration.SetRbwFilterBandwidth("", SpanforTxL * 1e6);
                specTxleakage[Iteration].Txp.Configuration.SetAveragingEnabled("", RFmxSpecAnMXTxpAveragingEnabled.False);
                specTxleakage[Iteration].Txp.Configuration.SetAveragingType("", RFmxSpecAnMXTxpAveragingType.Rms);
                specTxleakage[Iteration].Txp.Configuration.SetAveragingCount("", 1);

                ////Original
                //specTxleakage[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, false);
                //specTxleakage[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", SpanforTxL * 1e6); //mario
                //specTxleakage[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                //specTxleakage[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.True, 100e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                //specTxleakage[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.False, IqLength);
                ////specTxleakage[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001); //Original
                ///
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", -40e6);
                specTxleakage[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                specTxleakage[Iteration].Commit("");
            }
            public void CommitSpec()
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specTxleakage[Convert.ToInt32(Iteration)].Commit("");
            }
            public void InitiateSpec(int Iteration)
            {
                //_instrSession.SetDownconverterFrequencyOffset("", -40e6);
                specTxleakage[Convert.ToInt32(Iteration)].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public double RetrieveResults(int Iteration)
            {
                double averageChannelPower = 0;

                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
                specTxleakage[Iteration].Txp.Results.GetAverageMeanPower("", out averageChannelPower);

                return averageChannelPower;
            }
        }

        public class RfmxChp_For_Cal
        {
            List<RFmxSpecAnMX> specCHP;
            //public RFmxInstrMX _instrSession;
            public byte siteNo;

            public RfmxChp_For_Cal(byte site)
            {
                siteNo = site;
                specCHP = new List<RFmxSpecAnMX>();
            }

            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double SpanforTxL, byte site)
            {
                siteNo = site;
                string ChpTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;

                test = "Txleakage" + Iteration.ToString();
                specCHP.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));                
                specCHP[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                specCHP[Iteration].ConfigureDigitalEdgeTrigger("", ChpTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specCHP[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                specCHP[Iteration].ConfigureReferenceLevel("", Reflevel);                

                specCHP[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, false);
                specCHP[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 10 * 1e6);
                specCHP[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                specCHP[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 100e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                specCHP[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
            }
            public void ConfigureSpec_Calibration(int Iteration, double FreqSG, double Reflevel, double SpanforTxL, byte site) //Mario
            {
                siteNo = site;
                string ChpTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);
                string test;

                test = "Txleakage" + Iteration.ToString();
                specCHP.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetSpecAnSignalConfiguration(test));
                specCHP[Iteration].ConfigureFrequency("", FreqSG * 1e6);
                specCHP[Iteration].ConfigureDigitalEdgeTrigger("", ChpTrigStr, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specCHP[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation  
                specCHP[Iteration].ConfigureReferenceLevel("", Reflevel);

                specCHP[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, false);
                specCHP[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", 1e6);
                specCHP[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                specCHP[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, 100e3, RFmxSpecAnMXChpRbwFilterType.FftBased);
                specCHP[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0 * 1e6);
            }
            public void CommitSpec(int Iteration)
            {
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", -40e6);
                specCHP[Iteration].Commit("");
            }
            public void InitiateSpec(int Iteration)
            {
                specCHP[Iteration].Initiate("", "");
            }
            public float RetrieveResults(int Iteration)
            {
                double averageChannelPower = 0;
                double averageChannelPsd = 0;
                double relativeChannelPower = 0;
                double value = 0;
                //instrSession...WaitForAcquisitionComplete(Timeout); //TODO
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout); // JJ Low (5-May-2017)
                specCHP[Iteration].Chp.Results.FetchCarrierMeasurement("", 100, out averageChannelPower, out averageChannelPsd, out relativeChannelPower);
                //specAnChPowSignal[Iteration].Chp.Results.GetAverageChannelPower("", out averageChannelPower);
                specCHP[Iteration].Chp.Results.GetCarrierIntegrationBandwidth("", out value);

                return  (float)averageChannelPower;

            }
        }
        public class RfmxNR_EVM
        {
            //public string Type { get; set; }

            List<RFmxNRMX> specNR_EVM;
            //RFmxInstrMX _instrSession;
            const int NumberOfResourceBlockClusters = 1;
            public int Iteration;
            public double Freq;


            public string selectedPorts = "";
            public double centerFrequency;                                                /* (Hz) */
            public double referenceLevel = 0.0;                                                   /* (dBm) */
            public double externalAttenuation = 0.0;                                              /* (dB) */

            public bool enableTrigger = true;

            public int band;
            public int cellID = 0;
            public double carrierBandwidth = 100e6;                                               /* (Hz) */
            public double subcarrierSpacing = 30e3;                                               /* (Hz) */

            public RFmxNRMXPuschModulationType puschModulationType;

            public string puschSlotAllocation = "0-Last";
            public string puschSymbolAllocation = "0-Last";
            public string waveformName = "";

            public double measurementOffset = 0.0;
            public double measurementLength = 2;

            public RFmxNRMXPuschTransformPrecodingEnabled precodingEnabled;
            public int averagingCount = 1;

            public double timeout = 30.0;  //30 hosein 02102021
            public byte siteNo;

            public RfmxNR_EVM(byte site)
            {
                siteNo = site;
                specNR_EVM = new List<RFmxNRMX>();
            }

            public void ConfigFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, string WaveformName, string Band, byte site)
            {
                siteNo = site;
                string EvmTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string subblockString;
                string carrierString;
                string bandwidthPartString;
                string userString;
                string puschString;
                string puschClusterString;
                string test;

                //this.Iteration = Iteration;
                //this.Freq = Freq;
                //this.Reflevel = Reflevel;
                //this.band = band;
                //this.waveformName = waveformName;

                if (Band.Contains("B40") || Band.Contains("B41") || Band.Contains("B48")) Band = "B41"; // work around due to NI bug

                int band = int.Parse(Band.Substring(1,2));
                if (band == 40) band = 41;// work around due to NI bug
                if (band == 25) band = 2;
                if (band == 39) band = 38;
                //string name =  WaveformName.Split('M')[1].Split('R')[0];

                if (WaveformName.StartsWith("NRND") || WaveformName.StartsWith("ND"))
                    this.precodingEnabled = RFmxNRMXPuschTransformPrecodingEnabled.True;
                else
                    this.precodingEnabled = RFmxNRMXPuschTransformPrecodingEnabled.False;

                if (WaveformName.Contains("B5M")) this.carrierBandwidth = 5e6;
                else if (WaveformName.Contains("B10M")) this.carrierBandwidth = 10e6;
                else if (WaveformName.Contains("B20M")) this.carrierBandwidth = 20e6;
                else if (WaveformName.Contains("B30M")) this.carrierBandwidth = 30e6;
                else if (WaveformName.Contains("B40M")) this.carrierBandwidth = 40e6;
                else if (WaveformName.Contains("B50M")) this.carrierBandwidth = 50e6;
                else if (WaveformName.Contains("B80M")) this.carrierBandwidth = 80e6;
                else this.carrierBandwidth = 100e6;
                
                if (WaveformName.Contains("UQ")) this.puschModulationType = RFmxNRMXPuschModulationType.Qpsk;
                if (WaveformName.Contains("U1")) this.puschModulationType = RFmxNRMXPuschModulationType.Qam16;               
                if (WaveformName.Contains("U6")) this.puschModulationType = RFmxNRMXPuschModulationType.Qam64;               
                if (WaveformName.Contains("U2")) this.puschModulationType = RFmxNRMXPuschModulationType.Qam256;

                if (WaveformName.Contains("SC15")) subcarrierSpacing = 15e3;
                else subcarrierSpacing = 30e3;

                //const int NumberOfResourceBlockClusters = 1;
                int[] puschResourceBlockOffset = new int[NumberOfResourceBlockClusters];
                int[] puschNumberOfResourceBlocks = new int[NumberOfResourceBlockClusters];

                int resourceBlock = Convert.ToInt32(WaveformName.Split('M')[1].Split('R')[0]);//getResourceBlock(WaveformName);
                int resourceBlockOffset = Convert.ToInt32(WaveformName.Split('R')[1].Split('S')[0]);//getResourceBlockOffset(WaveformName);  //

                for (int i = 0; i < NumberOfResourceBlockClusters; i++)
                {
                    puschResourceBlockOffset[i] = resourceBlockOffset; //Yoonchun
                    puschNumberOfResourceBlocks[i] = resourceBlock;
                }


                test = "EVM_NR" + Iteration.ToString();
                while (specNR_EVM.Count <= Iteration)
                    specNR_EVM.Add(null);

                specNR_EVM.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetNRSignalConfiguration(test));


                specNR_EVM[Iteration].SetSelectedPorts("", "");

                specNR_EVM[Iteration].ConfigureRF("", FreqSG * 1e6, Reflevel, 0);
                specNR_EVM[Iteration].ConfigureDigitalEdgeTrigger("", EvmTrigStr, RFmxNRMXDigitalEdgeTriggerEdge.Rising, 0, true);
                specNR_EVM[Iteration].SetFrequencyRange("", RFmxNRMXFrequencyRange.Range1);

                specNR_EVM[Iteration].ComponentCarrier.SetBandwidth("", carrierBandwidth);
                specNR_EVM[Iteration].ComponentCarrier.SetCellID("", cellID);
                specNR_EVM[Iteration].SetBand("", band);
                specNR_EVM[Iteration].ComponentCarrier.SetBandwidthPartSubcarrierSpacing("", subcarrierSpacing);
                specNR_EVM[Iteration].SetAutoResourceBlockDetectionEnabled("", RFmxNRMXAutoResourceBlockDetectionEnabled.False);


                specNR_EVM[Iteration].ComponentCarrier.SetPuschTransformPrecodingEnabled("", precodingEnabled);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschSlotAllocation("", puschSlotAllocation);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschSymbolAllocation("", puschSymbolAllocation);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschModulationType("", puschModulationType);

                specNR_EVM[Iteration].ComponentCarrier.SetPuschNumberOfResourceBlockClusters("", NumberOfResourceBlockClusters);


                subblockString = RFmxNRMX.BuildSubblockString("", 0);
                carrierString = RFmxNRMX.BuildCarrierString(subblockString, 0);
                bandwidthPartString = RFmxNRMX.BuildBandwidthPartString(carrierString, 0);
                userString = RFmxNRMX.BuildUserString(bandwidthPartString, 0);
                puschString = RFmxNRMX.BuildPuschString(userString, 0);
                for (int i = 0; i < 1; i++)
                {
                    puschClusterString = RFmxNRMX.BuildPuschClusterString(puschString, i);
                    specNR_EVM[Iteration].ComponentCarrier.SetPuschResourceBlockOffset(puschClusterString, puschResourceBlockOffset[i]);
                    specNR_EVM[Iteration].ComponentCarrier.SetPuschNumberOfResourceBlocks(puschClusterString, puschNumberOfResourceBlocks[i]);

                }

                specNR_EVM[Iteration].ComponentCarrier.SetPuschDmrsPowerMode("", RFmxNRMXPuschDmrsPowerMode.CdmGroups);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschDmrsPower("", 0);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschDmrsConfigurationType("", RFmxNRMXPuschDmrsConfigurationType.Type1);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschMappingType("", RFmxNRMXPuschMappingType.TypeA);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschDmrsTypeAPosition("", 2);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschDmrsDuration("", RFmxNRMXPuschDmrsDuration.SingleSymbol);
                specNR_EVM[Iteration].ComponentCarrier.SetPuschDmrsAdditionalPositions("", 0);

                specNR_EVM[Iteration].SelectMeasurements("", RFmxNRMXMeasurementTypes.ModAcc, false);

                specNR_EVM[Iteration].ModAcc.Configuration.SetSynchronizationMode("", RFmxNRMXModAccSynchronizationMode.Slot);
                specNR_EVM[Iteration].ModAcc.Configuration.SetAveragingEnabled("", RFmxNRMXModAccAveragingEnabled.False);
                specNR_EVM[Iteration].ModAcc.Configuration.SetAveragingCount("", averagingCount);

                specNR_EVM[Iteration].ModAcc.Configuration.SetMeasurementLengthUnit("", RFmxNRMXModAccMeasurementLengthUnit.Slot);
                specNR_EVM[Iteration].ModAcc.Configuration.SetMeasurementOffset("", 0);

                if (WaveformName.Contains("SLOT1"))
                {
                    measurementLength = 1;
                }
                else
                {
                    measurementLength = 2;
                }

                specNR_EVM[Iteration].ModAcc.Configuration.SetMeasurementLength("", measurementLength);

                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0e6);
                specNR_EVM[Iteration].SetLimitedConfigurationChange("", RFmxNRMXLimitedConfigurationChange.NoChange);

                specNR_EVM[Iteration].Commit("");


            }
            private int getResourceBlock(string name)
            {
                if (name.Contains("273R"))
                {
                    return 273;
                }
                else if (name.Contains("270R"))
                {
                    return 270;
                }
                else if (name.Contains("217R"))
                {
                    return 217;
                }
                else if (name.Contains("216R"))
                {
                    return 216;
                }
                return -1;
            }
            public void CommitSpec(int Iteration)
            {

                specNR_EVM[Iteration].Commit("");

            }
            public void InitiateSpec(int Iteration)
            {

                specNR_EVM[Iteration].Initiate("", "");
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);

            }
            public void WaitForAcq()
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public void RetrieveResults(int Iteration, ref AclrResults aclrResults)
            {
              
            }

            public double RetrieveResults(int Iteration)
            {
                double compositeRmsEvmMean;                                                /* (%) */
                double compositePeakEvmMaximum;                                            /* (%) */
                int compositePeakEvmSlotIndex;
                int compositePeakEvmSymbolIndex;
                int compositePeakEvmSubcarrierIndex;

                double componentCarrierFrequencyErrorMean;                                 /* (Hz) */
                double componentCarrierIQOriginOffsetMean;                                 /* (dBc) */
                double componentCarrierIQGainImbalanceMean;                                /* (dB) */
                double componentCarrierQuadratureErrorMean;                                /* (deg) */
                double inBandEmissionMargin;

                double timeout;

                ComplexSingle[] puschDataConstellation, puschDmrsConstellation;

                AnalogWaveform<float> rmsEvmPerSubcarrierMean;
                AnalogWaveform<float> rmsEvmPerSymbolMean;

                Spectrum<float> spectralFlatness;
                Spectrum<float> spectralFlatnessLowerMask;
                Spectrum<float> spectralFlatnessUpperMask;

                specNR_EVM[Iteration].ModAcc.Results.GetCompositeRmsEvmMean("", out compositeRmsEvmMean);
                                
#region
                //specNR_EVM[Iteration].ModAcc.Results.GetCompositePeakEvmMaximum("", out compositePeakEvmMaximum);
                //specNR_EVM[Iteration].ModAcc.Results.GetCompositePeakEvmSlotIndex("", out compositePeakEvmSlotIndex);
                //specNR_EVM[Iteration].ModAcc.Results.GetCompositePeakEvmSymbolIndex("", out compositePeakEvmSymbolIndex);
                //specNR_EVM[Iteration].ModAcc.Results.GetCompositePeakEvmSubcarrierIndex("", out compositePeakEvmSubcarrierIndex);

                //specNR_EVM[Iteration].ModAcc.Results.GetComponentCarrierFrequencyErrorMean("", out componentCarrierFrequencyErrorMean);
                //specNR_EVM[Iteration].ModAcc.Results.GetComponentCarrierIQOriginOffsetMean("", out componentCarrierIQOriginOffsetMean);
                //specNR_EVM[Iteration].ModAcc.Results.GetComponentCarrierIQGainImbalanceMean("", out componentCarrierIQGainImbalanceMean);
                //specNR_EVM[Iteration].ModAcc.Results.GetComponentCarrierQuadratureErrorMean("", out componentCarrierQuadratureErrorMean);
                //specNR_EVM[Iteration].ModAcc.Results.GetInBandEmissionMargin("", out inBandEmissionMargin);

                //specNR_EVM[Iteration].ModAcc.Results.FetchPuschDataConstellationTrace("", timeout, ref puschDataConstellation);

                //specNR_EVM[Iteration].ModAcc.Results.FetchPuschDmrsConstellationTrace("", timeout, ref puschDmrsConstellation);

                //specNR_EVM[Iteration].ModAcc.Results.FetchRmsEvmPerSubcarrierMeanTrace("", timeout, ref rmsEvmPerSubcarrierMean);

                //specNR_EVM[Iteration].ModAcc.Results.FetchRmsEvmPerSymbolMeanTrace("", timeout, ref rmsEvmPerSymbolMean);

                //specNR_EVM[Iteration].ModAcc.Results.FetchSpectralFlatnessTrace("", timeout, ref spectralFlatness,
                //   ref spectralFlatnessLowerMask, ref spectralFlatnessUpperMask);
                #endregion

                return compositeRmsEvmMean;
            }
        }

        

        public class RfmxLTE_EVM //: iEVM
        {
            //public string Type { get; set; }

            List<RFmxLteMX> specLTE_EVM;
            //RFmxInstrMX _instrSession;
            public int Iteration;
            public double Freq;

            public double centerFrequency;                                                /* (Hz) */
            public double referenceLevel = 0.0;                                                   /* (dBm) */
            public double externalAttenuation = 0.0;                                              /* (dB) */
            
            public int band;
            public int cellID = 0;
            public double carrierBandwidth = 10e6;                                               /* (Hz) */
            
            public string waveformName = "";

            public double measurementOffset = 0.0;
            public double measurementLength = 2;

            public int averagingCount = 1;
            public byte siteNo;

            public RFmxLteMXDuplexScheme duplexScheme = RFmxLteMXDuplexScheme.Fdd;

            double[] componentCarrierBandwidth;// = new double[1];//{ 20e6, 20e6, 20e6 };
            double[] componentCarrierFrequency;// = new double[1];//{ -9.9e6, 0, 9.9e6 };
            int[] componentCarrierCellId;// = new int[1];//{ 0, 0, 0 };
            int numberOfComponentCarriers;

            double meanRmsCompositeEvm;
            double maxPeakCompositeEvm;
            double meanFrequencyError;
            int peakCompositeEvmSlotIndex;
            int peakCompositeEvmSymbolIndex;
            int peakCompositeEvmSubcarrierIndex;

            double[] meanRMSCompositeEvmArray;
            double[] maximumPeakCompositeEvmArray;
            double[] meanFrequencyErrorArray;
            int[] peakCompositeEvmSlotIndexArray;
            int[] peakCompositeEvmSymbolIndexArray;
            int[] peakCompositeEvmSubcarrierIndexArray;

            public RfmxLTE_EVM(byte site)
            {
                siteNo = site;
                specLTE_EVM = new List<RFmxLteMX>();
            }

            public void ConfigFreq(double Freq)
            {
                this.Freq = Freq;
            }
            public bool Initialize(bool FinalScript)
            {
                return false;
            }
            public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, string WaveformName, string Band, byte site)
            {
                siteNo = site;
                string EvmTrigStr = NIVST_Rfmx.TranslateTriggerLine((TriggerLine)NIVST_Rfmx.siteTrigArray[siteNo][0]);

                string test;

                if (WaveformName.Contains("60M"))
                {
                    carrierBandwidth = 60e6;
                    componentCarrierBandwidth = new double[] { 20e6, 20e6, 20e6 };
                    componentCarrierFrequency = new double[] { -9.9e6, 0, 9.9e6 };
                    componentCarrierCellId = new int[] { 0, 0, 0 };                    
                }
                else if (WaveformName.Contains("40M"))
                {
                    carrierBandwidth = 40e6;
                    componentCarrierBandwidth = new double[] { 20e6, 20e6 };
                    componentCarrierFrequency = new double[] { -9.9e6, 9.9e6 };
                    componentCarrierCellId = new int[] { 0, 0 };
                }
                else if (WaveformName.Contains("35M"))
                {
                    this.carrierBandwidth = 35e6;
                    this.componentCarrierBandwidth = new double[] { 20e6, 15e6 };
                    this.componentCarrierFrequency = new double[] { -7.425e6, 9.675e6 };
                    this.componentCarrierCellId = new int[] { 0, 0 };
                }
                else if (WaveformName.Contains("20M")) this.carrierBandwidth = 20e6;
                //else if (WaveformName.Contains("15M")) this.carrierBandwidth = 15e6;
                else if (WaveformName.Contains("10M")) this.carrierBandwidth = 10e6;
                else if (WaveformName.Contains("5M")) this.carrierBandwidth = 5e6;
                //    this.carrierBandwidth = 80e6;

                if (Band.Contains("B41")) Band = "B41"; // work around due to NI bug Band.Contains("B40") ||
                if (Band.Contains("B40") || Band.Contains("B42") || Band.Contains("B48")) Band = "B40";
                int band = int.Parse(Band.Substring(1));
                //if (band == 40) band = 41;// work around due to NI bug
                
                if (WaveformName.StartsWith("LT")) this.duplexScheme = RFmxLteMXDuplexScheme.Tdd;
                else this.duplexScheme = RFmxLteMXDuplexScheme.Fdd;

                test = "EVM_LTE" + Iteration.ToString();


                specLTE_EVM.Insert(Iteration, Eq.Site[siteNo].RF.InstrSession.GetLteSignalConfiguration(test));
                //Reflevel = Reflevel - 5;
                specLTE_EVM[Iteration].ConfigureRF("", FreqSG * 1e6, Reflevel, 0);

                //if (duplexScheme == RFmxLteMXDuplexScheme.Tdd) specLTE_EVM[Iteration].ConfigureIQPowerEdgeTrigger("", "0", RFmxLteMXIQPowerEdgeTriggerSlope.Rising,
                //      -20, 0, RFmxLteMXTriggerMinimumQuietTimeMode.Auto, 80e-6,
                //      RFmxLteMXIQPowerEdgeTriggerLevelType.Relative, true);

                //else
                    specLTE_EVM[Iteration].ConfigureDigitalEdgeTrigger("", EvmTrigStr, RFmxLteMXDigitalEdgeTriggerEdge.Rising, 0, true);


                if (carrierBandwidth > 20e6)
                {
                    numberOfComponentCarriers = componentCarrierBandwidth.Length;
                    specLTE_EVM[Iteration].ComponentCarrier.ConfigureSpacing("", RFmxLteMXComponentCarrierSpacingType.Nominal, -1);
                    specLTE_EVM[Iteration].ConfigureNumberOfComponentCarriers("", numberOfComponentCarriers);
                    specLTE_EVM[Iteration].ComponentCarrier.ConfigureArray("", componentCarrierBandwidth, componentCarrierFrequency, componentCarrierCellId);
                    //_instrSession.SetLOSource("", "LO_IN");

                    //_instrSession.SetDownconverterFrequencyOffset("", 0);
                }

                else
                {
                    specLTE_EVM[Iteration].ComponentCarrier.Configure("", carrierBandwidth, 0, 0);
                    //_instrSession.SetDownconverterFrequencyOffset("", 0e6);
                }


                    specLTE_EVM[Iteration].ConfigureBand("", band);
                specLTE_EVM[Iteration].ConfigureDuplexScheme("", duplexScheme, RFmxLteMXUplinkDownlinkConfiguration.Configuration0);
                specLTE_EVM[Iteration].ConfigureAutoDmrsDetectionEnabled("", RFmxLteMXAutoDmrsDetectionEnabled.True);

                specLTE_EVM[Iteration].SelectMeasurements("", RFmxLteMXMeasurementTypes.ModAcc, false);
                //if (duplexScheme == RFmxLteMXDuplexScheme.Tdd) specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureSynchronizationModeAndInterval("", RFmxLteMXModAccSynchronizationMode.Slot, 0, 2);
                //else
                    specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureSynchronizationModeAndInterval("", RFmxLteMXModAccSynchronizationMode.Slot, 0, 2);

                specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureEvmUnit("", RFmxLteMXModAccEvmUnit.Percentage);
                specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureInBandEmissionMaskType("", RFmxLteMXModAccInBandEmissionMaskType.Release11Onwards);
                specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureAveraging("", RFmxLteMXModAccAveragingEnabled.False, 1);

                //_instrSession.SetLOSource("", "LO_In");
                //_instrSession.SetDownconverterFrequencyOffset("", -100e6);
                Eq.Site[siteNo].RF.InstrSession.SetDownconverterFrequencyOffset("", 0e6);
                specLTE_EVM[Iteration].SetLimitedConfigurationChange("", RFmxLteMXLimitedConfigurationChange.NoChange);

                   //RFSAsession.SetLoFrequencyStepSize("", 50e3);



                specLTE_EVM[Iteration].Commit("");
                //specLTE_EVM[Iteration].Initiate("", "");

            }

            //public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double SpanforTxL, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw, string PowerMode)
            //{
            //    string test;
            //    double[] componentCarrierBandwidth = { 20e6, 20e6, 20e6 };
            //    double[] componentCarrierFrequency = { -9.9e6, 0, 9.9e6 };
            //    int[] componentCarrierCellId = { 0, 0, 0 };

            //    test = "EVM" + Iteration.ToString();

            //    specLTE_EVM.Insert(Iteration, NIVST_Rfmx.instrSession.GetLteSignalConfiguration(test));

            //    specLTE_EVM[Iteration].ConfigureRF("", FreqSG, Reflevel, 0);
            //    specLTE_EVM[Iteration].ConfigureDigitalEdgeTrigger("", RFmxLteMXConstants.PxiTriggerLine0, RFmxLteMXDigitalEdgeTriggerEdge.Rising, 0, true);

            //    specLTE_EVM[Iteration].ComponentCarrier.ConfigureSpacing("", RFmxLteMXComponentCarrierSpacingType.Nominal, -1);
            //    specLTE_EVM[Iteration].ConfigureBand("", 1);
            //    specLTE_EVM[Iteration].ConfigureDuplexScheme("", RFmxLteMXDuplexScheme.Tdd, RFmxLteMXUplinkDownlinkConfiguration.Configuration0);
            //    specLTE_EVM[Iteration].ConfigureNumberOfComponentCarriers("", 3);
            //    specLTE_EVM[Iteration].ComponentCarrier.ConfigureArray("", componentCarrierBandwidth, componentCarrierFrequency, componentCarrierCellId);
            //    specLTE_EVM[Iteration].ConfigureAutoDmrsDetectionEnabled("", RFmxLteMXAutoDmrsDetectionEnabled.True);

            //    specLTE_EVM[Iteration].SelectMeasurements("", RFmxLteMXMeasurementTypes.ModAcc, false);
            //    specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureSynchronizationModeAndInterval("", RFmxLteMXModAccSynchronizationMode.Slot, 0, 2);
            //    specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureEvmUnit("", RFmxLteMXModAccEvmUnit.Percentage);
            //    specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureInBandEmissionMaskType("", RFmxLteMXModAccInBandEmissionMaskType.Release11Onwards);
            //    specLTE_EVM[Iteration].ModAcc.Configuration.ConfigureAveraging("", RFmxLteMXModAccAveragingEnabled.False, 1);
            //    specLTE_EVM[Iteration].Commit("");
            //}
            public void CommitSpec(int Iteration)
            {

                specLTE_EVM[Iteration].Commit("");
            }
            public void InitiateSpec(int Iteration)
            {
                string aa = "";
               //_instrSession.SetDownconverterFrequencyOffset("", -10e6);
               //_instrSession.SetLOSource("", "onboard");
                //specLTE_EVM[Iteration].Commit("");
                //_instrSession.GetLOSource("", out aa);
                specLTE_EVM[Iteration].Initiate("", "");
                //string a = "";
                //_instrSession.GetLOSource("", out a);

                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);


                //string VALUE = "";
                //double sad = 0f;
                ////_instrSession.SetLOSource("", "LO_IN");

                ////specLTE_EVM[Iteration].Commit("");
                //_instrSession.GetDownconverterCenterFrequency("", out sad);
                //_instrSession.GetDownconverterFrequencyOffset("", out sad);
                //_instrSession.GetLOFrequency("", out sad);
                //_instrSession.GetLOSource("", out VALUE);

            }
            public void WaitForAcq()
            {
                Eq.Site[siteNo].RF.InstrSession.WaitForAcquisitionComplete(Timeout);
            }
            public void SpecIteration()
            {
                Iteration++;
            }
            public int GetSpecIteration()
            {
                return Iteration;
            }
            public void RetrieveResults(int Iteration, ref AclrResults aclrResults)
            {

            }

            public double RetrieveResults(int Iteration, string WaveformName)
            {
                double timeout = 10; //hosein 02102021
                               
                double meanIQOriginOffset;
                double meanIQGainImbalance;
                double meanIQQuadratureError;
                double meanRmsEvm;
                double meanRmsQpskEvm;
                double meanRms16QamEvm;
                double meanRms64QamEvm;
                double meanRms256QamEvm;
                double meanRms1024QamEvm;
                
                ComplexSingle[] qpskConstellation = null, qam16Constellation = null, qam64Constellation = null, qam256Constellation = null, qam1024Constellation = null;
                AnalogWaveform<float> meanRmsEvmPerSubcarrier = null;


                if (WaveformName.Contains("60M"))
                {
                    carrierBandwidth = 60e6;
                    numberOfComponentCarriers = 3;
                }
                else if (WaveformName.Contains("40M"))
                {
                    carrierBandwidth = 40e6;
                    numberOfComponentCarriers = 2;
                }
                else if (WaveformName.Contains("35M"))
                {
                    this.carrierBandwidth = 35e6;
                    numberOfComponentCarriers = 2;
                }
                else if (WaveformName.Contains("20M")) this.carrierBandwidth = 20e6;
                //else if (WaveformName.Contains("15M")) this.carrierBandwidth = 15e6;
                else if (WaveformName.Contains("10M")) this.carrierBandwidth = 10e6;
                else if (WaveformName.Contains("5M")) this.carrierBandwidth = 5e6;

                if (this.carrierBandwidth > 20e6)
                {
                    meanRmsCompositeEvm = 0;

                    specLTE_EVM[Iteration].ModAcc.Results.FetchCompositeEvmArray("", timeout, ref meanRMSCompositeEvmArray,
                    ref maximumPeakCompositeEvmArray, ref meanFrequencyErrorArray, ref peakCompositeEvmSymbolIndexArray,
                    ref peakCompositeEvmSubcarrierIndexArray, ref peakCompositeEvmSlotIndexArray);

                    for (int i = 0; i < numberOfComponentCarriers; i++)
                    {
                        if (meanRmsCompositeEvm < meanRMSCompositeEvmArray[i]) meanRmsCompositeEvm = meanRMSCompositeEvmArray[i];
                    }
                }
                else
                {
                    specLTE_EVM[Iteration].ModAcc.Results.FetchCompositeEvm("", timeout, out meanRmsCompositeEvm, out maxPeakCompositeEvm,
                                                      out meanFrequencyError,
                      out peakCompositeEvmSymbolIndex, out peakCompositeEvmSubcarrierIndex, out peakCompositeEvmSlotIndex);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchIQImpairments("", timeout, out meanIQOriginOffset, out meanIQGainImbalance,
                    //                                 out meanIQQuadratureError);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdschEvm("", timeout, out meanRmsEvm, out meanRmsQpskEvm, out meanRms16QamEvm,
                    //                                 out meanRms64QamEvm, out meanRms256QamEvm);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdsch1024QamEvm("", timeout, out meanRms1024QamEvm);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdschQpskConstellation("", timeout, ref qpskConstellation);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdsch16QamConstellation("", timeout, ref qam16Constellation);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdsch64QamConstellation("", timeout, ref qam64Constellation);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdsch256QamConstellation("", timeout, ref qam256Constellation);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchPdsch1024QamConstellation("", timeout, ref qam1024Constellation);
                    //specLTE_EVM[Iteration].ModAcc.Results.FetchEvmPerSubcarrierTrace("", timeout, ref meanRmsEvmPerSubcarrier);
                }


                return meanRmsCompositeEvm;
            }
            //public double RetrieveResults(int Iteration)
            //{

            //    double timeout = 10;

            //    double[] meanRMSCompositeEvm;
            //    double[] maximumPeakCompositeEvm;
            //    double[] meanFrequencyError;
            //    int[] peakCompositeEvmSlotIndex;
            //    int[] peakCompositeEvmSymbolIndex;
            //    int[] peakCompositeEvmSubcarrierIndex;

            //    double[] meanIQOriginOffset;
            //    double[] meanIQGainImbalance;
            //    double[] meanIQQuadratureError;
            //    double[] inBandEmissionMargin;

            //    ComplexSingle[][] dataConstellation, dmrsDataConstellation;

            //    AnalogWaveform<float>[] meanRmsEvmPerSubcarrier;

            //    specLTE_EVM[Iteration].ModAcc.Results.FetchCompositeEvmArray("", timeout, ref meanRMSCompositeEvm,
            //        ref maximumPeakCompositeEvm, ref meanFrequencyError, ref peakCompositeEvmSymbolIndex,
            //        ref peakCompositeEvmSubcarrierIndex, ref peakCompositeEvmSlotIndex);
            //    //specLTE_EVM[Iteration].ModAcc.Results.FetchIQImpairmentsArray("", timeout, ref meanIQOriginOffset,
            //    //    ref meanIQGainImbalance, ref meanIQQuadratureError);
            //    //specLTE_EVM[Iteration].ModAcc.Results.FetchInBandEmissionMarginArray("", timeout, ref inBandEmissionMargin);
            //    //for (int i = 0; i < numberOfComponentCarriers; i++)
            //    //{
            //    //    subblockCarrierString = RFmxspecLTE_EVM[Iteration]MX.BuildCarrierString("", i);
            //    //    specLTE_EVM[Iteration].ModAcc.Results.FetchPuschConstellationTrace(subblockCarrierString, timeout,
            //    //        ref dataConstellation[i], ref dmrsDataConstellation[i]);
            //    //    specLTE_EVM[Iteration].ModAcc.Results.FetchEvmPerSubcarrierTrace(subblockCarrierString, timeout,
            //    //        ref meanRmsEvmPerSubcarrier[i]);
            //    //}
            //}
        }
        public interface iEVM
        {
            string Type { get; set; }
        }
        public enum eRfmx_Measurement_Type
        {
            eRfmxAcp,
            eRfmxAcpNR,
            eRfmxChp,
            eRfmxIQ,
            eRfmxIQ_EVM,
            eRfmxChp_Timing,
            eRfmxIQ_Timing,
            eRfmxIIP3,
            eRfmxHar2nd,
            eRfmxHar3rd,
            eRfmxTxleakage,
            eRfmxChp_For_Cal,
            eRfmxEVM,
    
        }
        public enum eRfmx_EVM_Type
        {
            LTE,
            LTEA,
            NR
        }

        private enum ThreadFlag
        {
             SA,
             SG
        }
    }
}
