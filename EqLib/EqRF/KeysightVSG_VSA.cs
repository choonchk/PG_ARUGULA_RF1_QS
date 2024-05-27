using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using NationalInstruments.ModularInstruments.Interop;
using Keysight.KtM9420.Interop;
using Keysight.SignalStudio;
using Keysight.SignalStudio.N7614;
using ClothoLibAlgo;
using IqWaveform;

using Agilent.AgM9391.Interop;
using Agilent.AgM938x.Interop;
using Agilent.AgM9018.Interop;

namespace EqLib
{
    public partial class EqRF
    {
        public class KeysightVSG_VSA : iEqRF
        {
            #region Variables
            public iEqSG SG { get; set; }
            public iEqSA SA { get; set; }
            public IQ.Waveform ActiveWaveform { get; set; }
            public string VisaAlias { get; set; }
            private byte _site;
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
            //new vars *******************************************************
        
            public bool simulateHardware = false;
            public bool alignVsg = true;
            public bool alignVsa = true;
            public bool[] doVsaAlign = new bool[1000];
            public bool[] doPowerSearch = new bool[1000];
            public double[] rfOffset = new double[1000];
            public double[] scaleOffset = new double[1000];


            // Set Default EXT Trigger source to be front panel
            public AgM9391TriggerEnum vsaTriggerSource = AgM9391TriggerEnum.AgM9391TriggerPXITrigger2;

            public string M9018resource = "M9018";
            public string M9391resource = "M9391";
            public string M9381resource = "M9381";












            public static Agilent.AgM938x.Interop.AgM938x M9381 { get; set; }
            public static Agilent.AgM9391.Interop.AgM9391 M9391 { get; set; }
            //public IKtM9420Ex VXT = new KtM9420();

            //private niPowerServo PowerServo;
            private int errorCode;
            private double LossPout;
            private double VSGanalogLevel;
            private double[] MeasurementTimes;
            private double[] MeasurementLevels;
            public static string PreviousWaveform;
            public static Dictionary<string, WaveformParameter> waveformParameters = new Dictionary<string, WaveformParameter>();
            #endregion

            #region Setup External Trigger for data capture to smooth the trace
            void SetupTrigger(bool bUseExternalTrigger = true, double TriggerDelay = 0)
            {
                //if (bUseExternalTrigger)
                //{
                //    VXT.Receiver.Triggers.AcquisitionTrigger.Mode = KtM9420AcquisitionTriggerModeEnum.KtM9420AcquisitionTriggerModeExternal;
                //    VXT.Receiver.Triggers.AcquisitionTrigger.Delay = TriggerDelay;
                //    VXT.Receiver.Triggers.ExternalTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                //    VXT.Receiver.Triggers.ExternalTrigger.Slope = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                //    VXT.Receiver.Triggers.ExternalTrigger.Source = KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;
                //    VXT.Receiver.Triggers.ExternalTrigger.Level = 1.4;
                //    VXT.Receiver.Triggers.ExternalTrigger.Enabled = true;
                //}
                //else
                //{
                //    VXT.Receiver.Triggers.AcquisitionTrigger.Mode = KtM9420AcquisitionTriggerModeEnum.KtM9420AcquisitionTriggerModeImmediate;
                //    VXT.Receiver.Triggers.AcquisitionTrigger.Delay = TriggerDelay;
                //    VXT.Receiver.Triggers.ExternalTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                //    VXT.Receiver.Triggers.ExternalTrigger.Slope = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                //    VXT.Receiver.Triggers.ExternalTrigger.Source = KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;
                //    VXT.Receiver.Triggers.ExternalTrigger.Level = 1.4;
                //    VXT.Receiver.Triggers.ExternalTrigger.Enabled = false;
                //}
                //VXT.Apply();
            }
            #endregion
            public void Initialize()
            {
                try
                {
                    M9381 = new AgM938x();
                    M9391 = new AgM9391();


                    int errorcode = -1; string message = string.Empty;
                    bool idquery = true;
                    bool reset = true;
                    string M9391options = string.Format(
                                             "QueryInstrStatus=true, Simulate={0}, M9391Setup= Model=, Trace=false, ShareAllVisaSessions=true", (simulateHardware ? "true" : "false"));
                    string M9381options = string.Format(
                        "QueryInstrStatus=true, Simulate={0},  ShareAllVisaSessions=true, UsePlayArbThreadPool=true", (simulateHardware ? "true" : "false"));

                    bool runSelfTest = false;
                    //if (MessageBox.Show("Do you want to Self-test VSG and VSA", "SelfTest",
                    //    MessageBoxButtons.YesNo) == DialogResult.Yes)
                    runSelfTest = true;


                    ////Initialize M9381
                    M9381.Initialize(M9381resource, idquery, reset, M9381options);



                    // Clear startup messages & warnings if any.
                    do
                    {
                        M9381.Utility.ErrorQuery(ref errorcode, ref message);
                        if (errorcode != 0)
                            Console.WriteLine(message);
                    } while (errorcode != 0);

                    if (runSelfTest)
                    {
                        int vsgResult = 0;
                        string vsgMessage = "";
                        M9381.Utility.SelfTest(ref vsgResult, ref vsgMessage);

                        if (vsgResult != 0)
                        {
                            if (!vsgMessage.Contains("Calibration expired"))
                            {
                                MessageBox.Show(vsgMessage, "M9381A Self-test");
                            }
                        }
                    }


                    // Enable Trigger output on ARB wrap to externally trigger acquisitions, if needed
                    M9381.Triggers.SynchronizationOutputTrigger.Configure(true, AgM938xMarkerEnum.AgM938xMarkerNone,
                                             AgM938xTriggerPolarityEnum.AgM938xTriggerPolarityPositive, 10e-6,
                                             AgM938xSynchronizationTriggerTypeEnum.AgM938xSynchronizationTriggerTypePerArb);
                    M9381.ALC.Enabled = false;
                    M9381.RF.OutputEnabled = true;
                    M9381.Modulation.Enabled = true;


                    M9381.Triggers.ExternalTrigger.Enabled = false;
                    Agilent.AgM938x.Interop.AgM938xTriggerEnum[] triggerList = new Agilent.AgM938x.Interop.AgM938xTriggerEnum[]
                                { Agilent.AgM938x.Interop.AgM938xTriggerEnum.AgM938xTriggerFrontPanelTrigger2,
                                Agilent.AgM938x.Interop.AgM938xTriggerEnum.AgM938xTriggerPXITrigger2 };
                    M9381.Triggers2.SynchronizationOutputTrigger2.SetDestinationList(ref triggerList);

                    M9381.Triggers.SynchronizationOutputTrigger.Enabled = true;
                    //M9381.Triggers.SynchronizationOutputTrigger.PulseWidth = 50e-6;  Old modified 16042014
                    M9381.Triggers.SynchronizationOutputTrigger.PulseWidth = 20e-6;

                    M9381.Apply();

                //Initialize M9391
                    M9391.Initialize(VisaAlias, idquery, reset, M9391options);
                    // Clear startup messages & warnings if any.
                    do
                    {
                        M9391.Utility.ErrorQuery(ref errorcode, ref message);
                        if (errorcode != 0)
                            Console.WriteLine(message);
                    } while (errorcode != 0);

                    if (runSelfTest)
                    {
                        int vsaResult = 0;
                        string vsaMessage = "";
                        M9391.Utility.SelfTest(ref vsaResult, ref vsaMessage);
                        if (vsaResult != 0)
                        {
                            if (!vsaMessage.Contains("Calibration expired"))
                            {
                                MessageBox.Show(vsaMessage, "M9391A Self-test");
                            }
                        }
                    }

                    // Setup the Sync Output trigger to come out on M9311A Trig2 and PXI-TRIG2
                    M9391.Triggers.ExternalTrigger.Holdoff = 16e-9;
                    vsaTriggerSource = AgM9391TriggerEnum.AgM9391TriggerPXITrigger2;
                    // vsaTriggerSource = AgM9391TriggerEnum.AgM9391TriggerFrontPanelTrigger2;
                    M9391.Triggers.ExternalTrigger.Source = vsaTriggerSource;
                    // Initialize the variables used for VSG power search and VSA Alignments

                    configVsaTrigger(Agilent.AgM9391.Interop.AgM9391AcquisitionTriggerModeEnum.AgM9391AcquisitionTriggerModeImmediate);

                    initAlignmentVariables();


                    // Configure the Chassis backplane triggers and hav VSG drive PXI-Trigger2
                    string M9018options = string.Format(
                        "QueryInstrStatus=true, Simulate={0}, M9391Setup= Model=, Trace=false", (simulateHardware ? "true" : "false"));
                    IAgM9018 M9018 = new AgM9018();
                    //Initialize M9018A
                    M9018.Initialize(M9018resource, idquery, reset, M9018options);

                    // Configure the Chassis to allow Trigger Segment 1 to driver trigger segment 2 for PXI Trigger2.
                    // Use PXI Trig2 as it is connected to the M90XA 
                    M9018.TriggerBus.Connect(0, AgM9018TrigBusEnum.AgM9018TrigBus3To2);
                    M9018.TriggerBus.Connect(2, AgM9018TrigBusEnum.AgM9018TrigBus1To2To3);
                    M9018.TriggerBus.Connect(3, AgM9018TrigBusEnum.AgM9018TrigBus3To2To1);
                    //M9018.TemperatureSensors.get_Item("TS1");

                    M9018.Close();
                    M9018 = null;

                    setVsgVsaAlign(true, true);




                    //SG = new PxiVxtSg(this);
                    //SA = new PxiVxtSa(this);
                    //SG_VST = SG as PxiVxtSg;
                    //SA_VST = SA as PxiVxtSa;
                    //bool bShowXapp = false;
                    //string strOptionString = "QueryInstrStatus=true, Simulate=false, DriverSetup= AppStart = ";
                    //strOptionString += bShowXapp ? "true" : "false";
                    ////string temp = VisaAlias;
                    //////if (Site == 1) temp = "PXI168::0::0::INSTR";
                    //VXT.Initialize(
                    //    ResourceName: VisaAlias,
                    //    IdQuery: true,
                    //    Reset: true, OptionString: "QueryInstrStatus=true, Simulate=false, DriverSetup=AppStart=false, NewAppDomain = true");
                    ////Console.WriteLine("VXT_{0}:hash code {1}", this.Site, VXT.GetHashCode());
                    //// Configure Trigger as external triggered.   
                    //VXT.Source.Tigger.SynchronizationOutputTrigger.Type = KtM9420SourceSynchronizationTriggerTypeEnum.KtM9420SourceSynchronizationTriggerTypePerArb;
                    ////VXT.Source.Tigger.SynchronizationOutputTrigger.DataMarker = KtM9420MarkerEnum.KtM9420Marker1;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger.Polarity = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger.PulseWidth = 100e-6;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger.Destination = KtM9420TriggerEnum.KtM9420TriggerPXITrigger0;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger.Enable = true;

                    //VXT.Source.Tigger.SynchronizationOutputTrigger2.Enable = true;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger2.PulseWidth = 100e-6;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger2.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModeLevel;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger2.Polarity = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger2.Destination = KtM9420TriggerEnum.KtM9420TriggerInternalTrigger;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger2.Type = KtM9420SourceSynchronizationTriggerTypeEnum.KtM9420SourceSynchronizationTriggerTypePerArb;

                    //VXT.Source.Tigger.SynchronizationOutputTrigger3.Enable = true;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger3.PulseWidth = 100e-6;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger3.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModeLevel;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger3.Polarity = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger3.Destination = KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;
                    //VXT.Source.Tigger.SynchronizationOutputTrigger3.Type = KtM9420SourceSynchronizationTriggerTypeEnum.KtM9420SourceSynchronizationTriggerTypePerArb;

                    //VXT.Receiver.Triggers.AcquisitionTrigger.Mode = KtM9420AcquisitionTriggerModeEnum.KtM9420AcquisitionTriggerModeImmediate;
                    //VXT.Receiver.Triggers.AcquisitionTrigger.Delay = 60E-6;
                    //VXT.Receiver.Triggers.ExternalTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                    //VXT.Receiver.Triggers.ExternalTrigger.Slope = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                    //VXT.Receiver.Triggers.ExternalTrigger.Source = KtM9420TriggerEnum.KtM9420TriggerInternalTrigger;
                    //VXT.Receiver.Triggers.ExternalTrigger.Level = 1.4;
                    //VXT.Receiver.Triggers.ExternalTrigger.Enabled = false;
                    //VXT.Source2.Modulation2.BasebandPower = -50;
                    //// Load all the waveform. 
                    //VXT.Apply();
                    //SG_VST.Pass_session(VXT);
                    //SA_VST.Pass_session(VXT);
                    //SA.Initialize(VisaAlias);
                    //SG.Initialize(VisaAlias);
                    //LoadWaveform("CW", "");
                    //AddWaveformToMemory("CW", -20 * Math.Log10(VXT.Source.Modulation.ArbRmsValue), VXT.Source.Modulation.ArbSampeRate);
                    ////LoadWaveform("CW_SWITCHINGTIME_OFF", "");
                    ////AddWaveformToMemory("CW_SWITCHINGTIME_OFF.wfm", -20 * Math.Log10(VXT.Source.Modulation.ArbRmsValue), VXT.Source.Modulation.ArbSampeRate);
                    ////LoadWaveform("CW_SWITCHINGTIME_ON", "");
                    ////AddWaveformToMemory("CW_SWITCHINGTIME_ON.wfm", -20 * Math.Log10(VXT.Source.Modulation.ArbRmsValue), VXT.Source.Modulation.ArbSampeRate);
                    ////LoadWaveform("PINSWEEP", "");
                    ////AddWaveformToMemory("PINSWEEP.wfm", -20 * Math.Log10(VXT.Source.Modulation.ArbRmsValue), VXT.Source.Modulation.ArbSampeRate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    //if (VXT != null)
                    //{
                    //    VXT.Close();
                    //    VXT = null;
                    //}
                }
            }
            #region PendingApply Part. By using this flag, IVI driver execute functions in one time..
            [Flags]
            private enum PENDINGAPPLY
            {
                NONE = 0,
                SETUPVSA = 1,
                SETUPVSG = 2,
                SETUPVSAFREQ = 4,
                SETUPVSGFREQ = 8,
                SETUPPOWERSERVO = 16,
                SETUPACPR = 32,
                SETUPHARMONICS = 64,
            }
            private PENDINGAPPLY mPendingApplies = PENDINGAPPLY.NONE;
            private void addPendingApply(PENDINGAPPLY pendingApply)
            {
                mPendingApplies |= pendingApply;
                //Logging(new INFO(returnVal, returnVal.GetType(), strMessage, (double)tick.ElapsedTicks / (Stopwatch.Frequency / 1000), bError));
            }
            private PENDINGAPPLY addPendingApplies(List<PENDINGAPPLY> pendingApplies)
            {

                foreach (PENDINGAPPLY pendingApply in pendingApplies)
                {
                    mPendingApplies |= pendingApply;
                }
                return mPendingApplies;
                //Logging(new INFO(returnVal, returnVal.GetType(), strMessage, (double)tick.ElapsedTicks / (Stopwatch.Frequency / 1000), bError));
            }
            private void removePendingApply(PENDINGAPPLY pendingApply)
            {

                //Logging(new INFO(returnVal, returnVal.GetType(), strMessage, (double)tick.ElapsedTicks / (Stopwatch.Frequency / 1000), bError));
                mPendingApplies &= ~pendingApply;
            }
            private bool hasPendingApply(PENDINGAPPLY pendingApply)
            {
                //Logging(new INFO(returnVal, returnVal.GetType(), strMessage, (double)tick.ElapsedTicks / (Stopwatch.Frequency / 1000), bError));
                return ((mPendingApplies & pendingApply) == pendingApply);
            }
            //private void vsagApply(PENDINGAPPLY inputPendings = PENDINGAPPLY.NONE)
            //{
            //    VXT.Measurement.EnabledMeasurements = 0;

            //    foreach (PENDINGAPPLY pendingApply in Enum.GetValues(typeof(PENDINGAPPLY)))
            //    {
            //        if (hasPendingApply(pendingApply))
            //        {
            //            //Logging( functionName + "HAS PENDINGAPPLY CHECK : " + pendingApply.ToString());
            //            switch (pendingApply)
            //            {
            //                case PENDINGAPPLY.SETUPVSA:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsa;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //                case PENDINGAPPLY.SETUPVSAFREQ:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsaFrequency;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //                case PENDINGAPPLY.SETUPVSG:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsg;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //                case PENDINGAPPLY.SETUPVSGFREQ:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsgFrequency;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //                case PENDINGAPPLY.SETUPPOWERSERVO:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsPowerServo;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //                case PENDINGAPPLY.SETUPACPR:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsAcpr;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //                case PENDINGAPPLY.SETUPHARMONICS:
            //                    VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsHarmonics;
            //                    //Logging( functionName + "PENDINGAPPLY ENABLED : " + pendingApply.ToString());
            //                    break;
            //            }
            //            removePendingApply(pendingApply);
            //            //Logging(functionName + "PENDINGAPPLY REMOVED : " + pendingApply.ToString());
            //        }
            //    }
            //    VXT.Measurement.Process();
            //    //Logging(functionName + "Measurement PROCESSED");
            //    //Logging(new INFO(returnVal, returnVal.GetType(), strMessage, (double)tick.ElapsedTicks / (Stopwatch.Frequency / 1000), bError));
            //    return;
            //}
            //private void VXTApplyChange(bool setupVsa = false, bool setupVsg = false, bool setupVsaFrequency = false, bool setupVsgFrequency = false, bool setupPowerServo = false, bool setupAcpr = false, bool setupHarmonics = false, bool isProcess = true)
            //{

            //    // Config active measure enable symbol according input parameters
            //    if (setupVsa) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsa; }

            //    if (setupVsg) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsg; }

            //    if (setupVsaFrequency) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsaFrequency; }

            //    if (setupVsgFrequency) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsSetupVsgFrequency; }

            //    if (setupPowerServo) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsPowerServo; }

            //    if (setupAcpr) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsAcpr; }

            //    if (setupHarmonics) { VXT.Measurement.EnabledMeasurements |= (int)KtM9420MeasurementsEnum.KtM9420MeasurementsHarmonics; }

            //    // Process
            //    if (isProcess) { VXT.Measurement.Process(); }
            //    else
            //    {
            //        if (setupVsa) { addPendingApply(PENDINGAPPLY.SETUPVSA); }
            //        if (setupVsg) { addPendingApply(PENDINGAPPLY.SETUPVSG); }
            //        if (setupVsaFrequency) { addPendingApply(PENDINGAPPLY.SETUPVSAFREQ); }
            //        if (setupVsgFrequency) { addPendingApply(PENDINGAPPLY.SETUPVSGFREQ); }
            //        if (setupPowerServo) { addPendingApply(PENDINGAPPLY.SETUPPOWERSERVO); }
            //        if (setupAcpr) { addPendingApply(PENDINGAPPLY.SETUPACPR); }
            //        if (setupHarmonics) { addPendingApply(PENDINGAPPLY.SETUPHARMONICS); }
            //    }
            //    // Logging(new INFO(returnVal, returnVal.GetType(), strMessage, (double)tick.ElapsedTicks / (Stopwatch.Frequency / 1000), bError));
            //    return;
            //}
            #endregion
            public void ConfigFFTACPR(ref double[] ACPROffset, double ACPRSpan, double ACPRDuration, KtM9420ChannelFilterShapeEnum ACPRFilterType, double ACPRAlpha, double[] ACPRBandwidth, bool bUseChannelPowerAsReference)
            {
                //int numAcprMeas = ACPROffset.Length;
                //if (!bUseChannelPowerAsReference)
                //{
                //    numAcprMeas += 1;
                //}
                //double[] localAcprOffsetFreq = new double[numAcprMeas];
                //double[] localAcprSpan = new double[numAcprMeas];
                //double[] localAcprDuration = new double[numAcprMeas];
                //KtM9420ChannelFilterShapeEnum[] localAcprShape = new KtM9420ChannelFilterShapeEnum[numAcprMeas];
                //double[] localAcprAlpha = new double[numAcprMeas];
                //double[] localAcprBandwidth = new double[numAcprMeas];
                //Array.Copy(
                //            sourceArray: ACPROffset,
                //            sourceIndex: 0,
                //            destinationArray: localAcprOffsetFreq,
                //            destinationIndex: 0,
                //            length: numAcprMeas
                //        );
                //Array.Copy(
                //            sourceArray: ACPRBandwidth,
                //            sourceIndex: 0,
                //            destinationArray: localAcprBandwidth,
                //            destinationIndex: 0,
                //            length: numAcprMeas
                //        );
                //localAcprSpan = Enumerable.Repeat(ACPRSpan, numAcprMeas).ToArray();
                //localAcprDuration = Enumerable.Repeat(ACPRDuration, numAcprMeas).ToArray();
                //localAcprShape = Enumerable.Repeat(ACPRFilterType, numAcprMeas).ToArray();
                //localAcprAlpha = Enumerable.Repeat(ACPRAlpha, numAcprMeas).ToArray();


                //VXT.Measurement.Acpr.AcquisitionMode = KtM9420AcquisitionModeEnum.KtM9420AcquisitionModeFFT;
                //VXT.Measurement.Acpr.SetAcprParameter(
                //                                        OffsetFrequency: localAcprOffsetFreq,
                //                                        Span: localAcprSpan,
                //                                        Duration: localAcprDuration
                //                                    );
                ////Console.WriteLine("localAcprOffsetFreq: localAcprSpan: localAcprDuration:");
                ////for(int i=0;i<numAcprMeas;i++)
                ////{
                ////    Console.WriteLine(localAcprOffsetFreq[i].ToString() + " " + localAcprSpan[i].ToString() + " " + localAcprDuration[i].ToString()+"\n");
                ////}
               
                //VXT.Measurement.Acpr.AveragingNumber = 1;
                //VXT.Measurement.Acpr.UseChanPwrForRef = bUseChannelPowerAsReference;
                //VXT.Measurement.Acpr.ConfigureFilter(
                //                                        Shape: localAcprShape,
                //                                        Alpha: localAcprAlpha,
                //                                        Bandwidth: localAcprBandwidth
                //                                    );
                ////Console.WriteLine("localAcprShape: localAcprAlpha: localAcprBandwidth:");
                ////for (int i = 0; i < numAcprMeas; i++)
                ////{
                ////    Console.WriteLine(localAcprShape[i].ToString() + " " + localAcprAlpha[i].ToString() + " " + localAcprBandwidth[i].ToString() + "\n");
                ////}
            }
            public void WaveformConversion(IQ.Waveform Waveform, string BasePath)
            {

                try
                {
                  //  string WavefomrOutputFileName = Waveform.FullName + ".WAVEFORM" ;
                  //  double startTime=Waveform.WvfrmStartTime;
                  //  double timetoLoad=Waveform.WvfrmTimetoLoad;
                  //  //double[] IQdata = new double[Waveform.Idata.Count() * 2];
                  //  //byte[] Marker = new byte[Waveform.Idata.Count()];

                  //  //double RmsPower = 0;
                  //  //for (int i = 0; i < Waveform.Markers.Count(); i++)
                  //  //{
                  //  //    Marker[2 * i] = (byte)((Waveform.Markers[i] * (double)Waveform.VsgIQrate / 4) * 4);
                  //  //    Marker[2 * i + 1] = (byte)((Waveform.Markers[i] * (double)Waveform.VsgIQrate / 4) * 4 + 1);
                  //  //}

                  //  //for (int i = 0; i < Waveform.Idata.Count(); i++)
                  //  //{
                  //  //    IQdata[2 * i] = Waveform.Idata[i];
                  //  //    IQdata[2 * i + 1] = Waveform.Qdata[i];
                  //  //    RmsPower += Math.Pow(IQdata[2 * i], 2) + Math.Pow(IQdata[2 * i + 1], 2);
                  //  //}
                  //  //RmsPower = Math.Sqrt(RmsPower / (Waveform.Idata.Count() * 2 * 4));// calculate RmsPower
                  //  //VXT.Source3.Modulation3.IQ.UploadArbDoublesWithMarkers(WavefomrOutputFileName, IQdata, Marker, (double)Waveform.VsgIQrate, RmsPower, 1, KtM9420MarkerEnum.KtM9420MarkerNone);

                  //  if (WavefomrOutputFileName.Contains("CW"))
                  //      VXT.Source.GeneratePowerRampArb(
                  //RefName: WavefomrOutputFileName,
                  //MinPower: 0,
                  //MaxPower: 0,
                  //Duration: 100e-6,
                  //SampelRate: 4e6
                  //  );//Generate CW waveform
                  //  else VXT.Source.LoadShortWaveform(
                  // FilePath: "C:\\Avago.ATF.Common\\Waveforms",
                  // FileName: WavefomrOutputFileName,
                  // StartTime: startTime,
                  // TimeToLoad: timetoLoad
                  //          );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            public struct PowerServo
            {
                public float pin;
                public float pout;
                public bool failed;
                public PowerServo(float Pin, float Pout, bool Failed)
                {
                    pin = Pin;
                    pout = Pout;
                    failed = Failed;
                }
            };
            public struct WaveformParameter
            {
                public double rmsValue;
                public double sampleRate;
                public double FFTAcqRate;
                public double FFTAcqDur;
                public WaveformParameter(double rms, double sample,double AcqRate,double AcqDur)
                {
                    rmsValue = rms;
                    sampleRate = sample;
                    FFTAcqRate = AcqRate;
                    FFTAcqDur = AcqDur;
                }
            };
            public void AddWaveformToMemory(string Waveformname, double rmsValue, double sampleRate)
            {
                // Check name
                string name = IQ.Mem[Waveformname].FullName + ".WAVEFORM" + "Short";
                if (waveformParameters.ContainsKey(name))
                {
                    waveformParameters.Remove(name);
                }

                double AcqRate = IQ.Mem[Waveformname].AcqRate;
                double AcqDur = IQ.Mem[Waveformname].AcqDur;
                // Add into dictionary
                waveformParameters.Add(
                    key: name,
                    value: new WaveformParameter(
                        rmsValue,
                        sampleRate,
                        AcqRate,
                        AcqDur
                    )
                );

            }
            private List<string> loadedWaveforms = new List<string>();

            private bool _servoEnabled;
            public bool ServoEnabled
            {
                get
                {
                    return _servoEnabled;
                }
                set
                {
                    _servoEnabled = value;

                    if (!value) return;
                    

                    //VXT.Apply();
                }
            }
            static List<double> AclrResult = new List<double>();
            static double ReceiverInputPower = 0;
            static double SourceOutputPower = 0;
            public void ConfigureServo(double targetOutputPower, double powerTolerance, ushort minIterations, ushort maxIterations)
            {
                try
                {
                   
                    //// Setup power servo
                    // ReceiverInputPower = targetOutputPower + SA_VST.ExternalGain;//Manually apply external gain
                    ////double SourceOutputPower = TargetPout + LossPout + LossPin - ExpectedGain; 
                    // SourceOutputPower = SG.Level-SG_VST.ExternalGain;

                    //VXT.Measurement.PowerServo.AcqusitionMode = KtM9420AcquisitionModeEnum.KtM9420AcquisitionModeFFT;
                    ////Console.WriteLine("VXT hash code:{0}",VXT.GetHashCode());
                    ////Setup FFT acquisition    
                    //double FFTAcqRate=waveformParameters[ActiveWaveform.FullName+".WAVEFORM"+"Short"].FFTAcqRate;
                    //double FFTAcqDur = waveformParameters[ActiveWaveform.FullName + ".WAVEFORM" + "Short"].FFTAcqDur;
                    //KtM9420FFTAcquisitionLengthEnum FFTsize = KtM9420FFTAcquisitionLengthEnum.KtM9420FFTAcquisitionLength_512;
                    ////if (ActiveWaveform.FullName.Contains("WCDMA")) FFTsize = KtM9420FFTAcquisitionLengthEnum.KtM9420FFTAcquisitionLength_256;

                    ////VXT.FFTAcquisition.Configure(KtM9420FFTAcquisitionLengthEnum.KtM9420FFTAcquisitionLength_512, (double)ActiveWaveform.VsgIQrate * 3, 0.004, KtM9420FFTWindowShapeEnum.KtM9420FFTWindowShapeHann);
                    //VXT.FFTAcquisition.Configure(
                    //    Length:FFTsize,
                    //    SampleRate:FFTAcqRate,
                    //    Duration:FFTAcqDur,
                    //    WindowShape:KtM9420FFTWindowShapeEnum.KtM9420FFTWindowShapeGaussian);
                    ////Console.WriteLine("ActiveWaveformName:{0} FFTAcqRate:{1} FFTAcqDur:{2}", ActiveWaveform.FullName, FFTAcqRate, FFTAcqDur);
                    ////// Filter shape should be raised cosine with alpha 0.22 to be same as LTE spec and avoid calculation error.
                    //KtM9420ChannelFilterShapeEnum FilterType = KtM9420ChannelFilterShapeEnum.KtM9420ChannelFilterShapeRectangular;
                    //if (ActiveWaveform.FullName.Contains("10M1RB")) FilterType = KtM9420ChannelFilterShapeEnum.KtM9420ChannelFilterShapeNone;
                    //VXT.FFTAcquisition.ChannelFilter.Configure(FilterType, 0.22, ActiveWaveform.RefChBW);
                    ////Console.WriteLine("ChannelFilter.Configure Bandwidth: {0}", ActiveWaveform.RefChBW);
                    //VXTApplyChange(
                    //       setupVsg: true,
                    //       setupVsgFrequency: true,
                    //       setupVsa: true,
                    //       setupVsaFrequency: true,
                    //       isProcess: false
                    //       );
                    //VXT.Measurement.PowerServo.Configure(
                    //    InputPower: ReceiverInputPower, 
                    //    OutputPower:SourceOutputPower,
                    //    OutputPowerMargin:powerTolerance,
                    //    OverheadTime:6e-4);//The 3dB GainAccuracy will lead to overload during servo so minus 3 dB here and manually apply external gain.
                    ////Console.WriteLine("ReceiverInputPower:{0} SourceOutputPower:{1}", ReceiverInputPower, SourceOutputPower);
                    //double[] ACPRoffset = new double[ActiveWaveform.AclrSettings.OffsetHz.Count() * 2];
                    //double[] ACPRBwHz = new double[ActiveWaveform.AclrSettings.BwHz.Count() * 2];
                    //for (int i = 0; i < ActiveWaveform.AclrSettings.OffsetHz.Count(); i++)
                    //{
                    //    ACPRoffset[2 * i] = ActiveWaveform.AclrSettings.OffsetHz[i];
                    //    ACPRoffset[2 * i + 1] = 0 - ActiveWaveform.AclrSettings.OffsetHz[i];
                    //    ACPRBwHz[2 * i] = ActiveWaveform.AclrSettings.BwHz[i];
                    //    ACPRBwHz[2 * i + 1] = ActiveWaveform.AclrSettings.BwHz[i];
                    //    //ACPRBwHz[2 * i] = 3.84e6;
                    //    //ACPRBwHz[2 * i + 1] = 3.84e6;
                    //}
                    //bool UseChannelPowerForReference = true;

                    //KtM9420ChannelFilterShapeEnum ACPFilterType = KtM9420ChannelFilterShapeEnum.KtM9420ChannelFilterShapeRaisedCosine;
                    //if (ActiveWaveform.FullName.Contains("WCDMA")) ACPFilterType = KtM9420ChannelFilterShapeEnum.KtM9420ChannelFilterShapeRectangular;
                    //ConfigFFTACPR(ref ACPRoffset,
                    //                    (double)FFTAcqRate / 1.25,
                    //                    FFTAcqDur,
                    //                ACPFilterType,
                    //                    0.22,
                    //                    ACPRBwHz,
                    //                    UseChannelPowerForReference);
                    //PENDINGAPPLY mPendingApplies = addPendingApplies(new List<PENDINGAPPLY>
                    //                                {
                    //                                    PENDINGAPPLY.SETUPVSG,
                    //                                    PENDINGAPPLY.SETUPVSGFREQ,
                    //                                    PENDINGAPPLY.SETUPVSA,
                    //                                    PENDINGAPPLY.SETUPVSAFREQ,
                    //                                    PENDINGAPPLY.SETUPPOWERSERVO,
                    //                                    PENDINGAPPLY.SETUPACPR
                    //                                });
                    //vsagApply(mPendingApplies);// Have to apply changes here if move it to servo function,the servo will not perform at beginning.
                   
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString ());
                }
                
            }

            public bool Servo(out double Pout, out double Pin)
            {
               bool overLoad = false;
                int servoCount = 0;
                bool Failed = false;
               double channelpower = 0;
                try
                {
                //    double[] acprResultPower = new double[ActiveWaveform.AclrSettings.OffsetHz.Count() * 2];
                //    bool[] acprResultOverload = new bool[ActiveWaveform.AclrSettings.OffsetHz.Count() * 2];
                //    double[] FFTdata = new double[512];
                //    VXT.Measurement.PowerServo.ReadPowerServo
                //                                       (
                //                                       ChannelPower: ref channelpower,
                //                                       ServoPass: ref Failed,
                //                                       Overload: ref overLoad,
                //                                       ServoCount: ref servoCount
                //                                       );
                //    #region workaround for power servo trigger
                //    //VXT.Source.Tigger.SynchronizationOutputTrigger.Destination = KtM9420TriggerEnum.KtM9420TriggerPXITrigger0;
                //    //VXT.Receiver.Triggers.ExternalTrigger.Source = KtM9420TriggerEnum.KtM9420TriggerPXITrigger0;
                ////    VXT.Measurement.PowerServo.ReadMagnitudeData(
                ////    Data: ref FFTdata
                ////);
                //    #endregion
                //    VXT.Measurement.Acpr.ReadAcpr
                //                        (
                //                        Acpr: ref acprResultPower,
                //                        Overload: ref acprResultOverload
                //                        );

                //    if (AclrResult.Count() != 0) AclrResult.Clear();
                //    for (int i = 0; i < ActiveWaveform.AclrSettings.OffsetHz.Count() * 2; i++)
                //    {
                //        if (AclrResult.Count() <acprResultPower.Count()) AclrResult.Add(acprResultPower[i]);
                //    }
                //    Pin = (float)(VXT.Source.RF.Level + VXT.Source2.Modulation2.BasebandPower + SG_VST.ExternalGain);//Manually apply external gain
                //    Pout = (float)(channelpower - SA_VST.ExternalGain);//Manually apply external gain
                //    //Console.WriteLine("Thread{0}a: ServoCount{1}", Thread.CurrentThread.GetHashCode(), servoCount);
                Pout = 0;
                    Pin = 2;
                    return !Failed;
                
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    Pout = 0;
                    Pin = 0;
                    return !Failed;

                }

            }

            //public void SetActiveWaveform(string ModStd, string WvfrmName, bool useServoScript, bool setSG = true)
            //{
            //    throw new NotImplementedException();
            //}

            //public bool LoadWaveform(string ModStd, string WvfrmName)
            //{
            //    throw new NotImplementedException();
            //}

    

            public void SetActiveWaveform(string ModStd, string WvfrmName, bool useServoScript, bool setSG = true)
            {
                try
                {
                    //bool setwaveform=false;
                    //if (PreviousWaveform != WvfrmName)
                    //{
                    //    setwaveform = true;
                    //    PreviousWaveform = WvfrmName;
                    //}

                    //if (setwaveform)
                    //{
                    //    VXT.Source.Tigger.SynchronizationOutputTrigger.Type =
                    //    KtM9420SourceSynchronizationTriggerTypeEnum.KtM9420SourceSynchronizationTriggerTypePerArb;
                    //    VXT.Source.Modulation.ArbPlayConfigure(
                    //         WaveformName: (ModStd + WvfrmName).Replace("_", "") + ".WAVEFORM" + "Short",
                    //         ArbPlayMode:KtM9420ArbPlayModeEnum.KtM9420ArbPlayModePlayArb,
                    //         ArbPlayDuration:0);
                    //   // VXT.Source3.Modulation3.PlayArb((ModStd + WvfrmName).Replace("_", "") + ".WAVEFORM", KtM9420StartEventEnum.KtM9420StartEventExternalTrigger);
                    //    IQ.Waveform newWaveform = IQ.Mem[ModStd + WvfrmName];
                    //    ActiveWaveform = newWaveform;
                    //    SA.SampleRate = (double)newWaveform.VsaIQrate;
                    //    SA.NumberOfSamples = newWaveform.SamplesPerRecord;
                    //}
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(" returned Error with " + ex.Message);
                }
            }

            public bool LoadWaveform(string ModStd, string WvfrmName)
            {
                try
                {

                    //if ((ModStd == null | WvfrmName == null) || (ModStd == "" & WvfrmName == "")) return true;

                    //if (!IQ.Load(ModStd, WvfrmName)) return false;

                    //string BasePath = IQ.BasePath;

                    ////if (WvfrmName != "") BasePath += ModStd.ToUpper() + "\\" + WvfrmName.ToUpper() + "\\";
                    ////else if (ModStd.Contains("CW") || ModStd.Contains("PINSWEEP")) BasePath += "CW" + "\\";
                    ////if ((!File.Exists(BasePath + IQ.Mem[ModStd + WvfrmName].FullName + ".wfm"))) EqRF.KeysightVXT.WaveformConversion(IQ.Mem[ModStd + WvfrmName], BasePath);

                    //if (!loadedWaveforms.Contains(IQ.Mem[ModStd + WvfrmName].FullName))
                    //{
                    //    loadedWaveforms.Add(IQ.Mem[ModStd + WvfrmName].FullName);
                    //    WaveformConversion(IQ.Mem[ModStd + WvfrmName], BasePath);
                    //    try
                    //    {
                           
                    //        string WavefrminMem = ModStd + WvfrmName;
                    //        AddWaveformToMemory(WavefrminMem, -20 * Math.Log10(VXT.Source.Modulation.ArbRmsValue), VXT.Source.Modulation.ArbSampeRate);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        string strMessage = " returned Error with " + ex.Message;
                    //        MessageBox.Show(strMessage);
                    //        return false;
                    //    }

                    //}

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return false;
                }
            }

    public void setVsgVsaAlign(bool vsg, bool vsa)
    {
        alignVsg = vsg;
        alignVsa = vsa;
    }

    public void initAlignmentVariables()
    {
        // Reset the doPowerSeach booleans to force power search run after each init
        for (int i = 0; i < doPowerSearch.Length; i++)
        {
            doVsaAlign[i] = true;
            doPowerSearch[i] = true;
            rfOffset[i] = 0;
            scaleOffset[i] = 0;
        }
    }


            public void configVsaTrigger(AgM9391AcquisitionTriggerModeEnum triggerType) //Anthony
            {
                // Common Setup for VSA Trigger
                M9391.Triggers.AcquisitionTrigger.Mode = triggerType;

                if (triggerType == Agilent.AgM9391.Interop.AgM9391AcquisitionTriggerModeEnum.AgM9391AcquisitionTriggerModeExternal)
                {
                    // VSA will trigger on falling edge of Sync output pulse from VSG with zero delay.  
                    // Adjust the pulse width of the VSG sync output trigger to get the requried delay
                    M9391.Triggers.AcquisitionTrigger.Delay = 100e-6;
                    M9391.Triggers.AcquisitionTrigger.TimeoutMode = AgM9391TriggerTimeoutModeEnum.AgM9391TriggerTimeoutModeAutoTriggerOnTimeout;
                    M9391.Triggers.AcquisitionTrigger.Timeout = 100;
                    M9391.Triggers.ExternalTrigger.Slope = AgM9391TriggerSlopeEnum.AgM9391TriggerSlopeNegative;
                    M9391.Triggers.ExternalTrigger.Source = vsaTriggerSource;
                }
            }







            public class PxiVSG_VSA_Sa : iEqSA
            {
                public KeysightVXT VXT { get; set; }
                public IKtM9420Ex RFSAsession;
                private double _rfFrequency, _refLevel, _externalGain, _sampleRate;
                private long _numberOfSamples;

                private TriggerLine _digitalEdgeRefTrigger;


                public void Pass_session(IKtM9420Ex session)
                {
                    RFSAsession = session;
                }

                //public PxiVxtSa(KeysightVXT VXT)
                //{
                //    this.VXT = VXT;
                //}
                public void Initialize(string VSAname)
                {
                    TriggerIn = TriggerLine.PxiTrig0;
                }

                public string Model
                {
                    get
                    {
                        return "KeysightM9420A";
                    }
                }
                public double ReadTemp()
                {
                    return 0;
                }

                public double CenterFrequency
                {
                    get
                    {
                        return _rfFrequency;
                    }
                    set
                    {
                        M9381.RF.Frequency = value;
                        M9381.Modulation.BasebandFrequency = 0;    //may need to take tjis out
                        RFSAsession.Receiver.RF.Frequency = value;
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
                        //RFSAsession.Receiver.RF.Power =  Math.Min(value+ExternalGain,33);
                        //RFSAsession.Receiver.RF.InputPort = KtM9420PortEnum.KtM9420PortRFInput;
                        //RFSAsession.Receiver.RF.PeakerToAverage = waveformParameters[VXT.ActiveWaveform.FullName+".WAVEFORM"+"Short"].rmsValue;
                        ////Console.WriteLine("RFSAsession hash code:{0}", RFSAsession.GetHashCode());
                        ////Console.WriteLine("Receiver.RF.Power:{0} PeakerToAverage:{1} Thread:{2}",value + ExternalGain, RFSAsession.Receiver.RF.PeakerToAverage,Thread.CurrentThread.GetHashCode());
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
                        // set VXT SA external gain here
                        _externalGain = value;
                    }
                }

                public void ConfigureTrigger(string waveformName)
                {
                    ////mPendingApplies = PENDINGAPPLY.NONE;
                    //double delay = 0;
                    //if (waveformName.Contains("5M8RB")) delay = 150e-6;
                    //if (waveformName.Contains("10M1RB")) delay = 7.7408E-05;
                    //if (waveformName.Contains("100RB")) delay = 50E-6;
                    //if (waveformName.Contains("WCDMAGTC1")) delay = 5.897777778E-05;
                    //RFSAsession.Receiver.Triggers.AcquisitionTrigger.Mode = KtM9420AcquisitionTriggerModeEnum.KtM9420AcquisitionTriggerModeExternal;
                    //RFSAsession.Receiver.Triggers.AcquisitionTrigger.Delay = delay;
                    //RFSAsession.Receiver.Triggers.ExternalTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                    //RFSAsession.Receiver.Triggers.ExternalTrigger.Slope = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                    //RFSAsession.Receiver.Triggers.ExternalTrigger.Source = KtM9420TriggerEnum.KtM9420TriggerInternalTrigger;
                    //RFSAsession.Receiver.Triggers.ExternalTrigger.Level = 1.4;
                    //RFSAsession.Receiver.Triggers.ExternalTrigger.Enabled = true;
                }

                public TriggerLine TriggerIn
                {
                    get
                    {
                        return _digitalEdgeRefTrigger;
                    }
                    set
                    {
                        //RFSAsession.Receiver.Triggers.AcquisitionTrigger.Mode = KtM9420AcquisitionTriggerModeEnum.KtM9420AcquisitionTriggerModeExternal;
                        //RFSAsession.Receiver.Triggers.AcquisitionTrigger.Delay = 0;
                        //RFSAsession.Receiver.Triggers.ExternalTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                        //RFSAsession.Receiver.Triggers.ExternalTrigger.Slope = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                        //RFSAsession.Receiver.Triggers.ExternalTrigger.Source = VXT.TranslateTriggerLine(value);
                        //RFSAsession.Receiver.Triggers.ExternalTrigger.Level = 1.4;
                        //RFSAsession.Receiver.Triggers.ExternalTrigger.Enabled = true;

                        _digitalEdgeRefTrigger = value;
                    }
                }

                public TriggerLine TriggerOut
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public double MeasureChanPower()
                {
                    return 0;
                }
                public double PowerAcqusition(double FreqOffset, double Bandwidth, KtM9420ChannelFilterShapeEnum KtM9420ChannelFilterShape, double Alpha)
                {
                    double MeasuredPower = -100;
                //    bool Overload = true;
                //    RFSAsession.AcquisitionMode = KtM9420AcquisitionModeEnum.KtM9420AcquisitionModePower;
                //    RFSAsession.PowerAcquisition.Configure((double)VXT.ActiveWaveform.VsaIQrate, 500e-6);
                //    if (Bandwidth < 1e6) Bandwidth = (double)VXT.ActiveWaveform.VsaIQrate;
                //    RFSAsession.PowerAcquisition.ChannelFilter.Configure(KtM9420ChannelFilterShape, Alpha,
                //Bandwidth);
                //    RFSAsession.PowerAcquisition.OffsetFrequency = FreqOffset;
                //    RFSAsession.Apply();
                //    RFSAsession.Arm();
                //    RFSAsession.WaitForData(500);
                //    RFSAsession.PowerAcquisition.ReadPower(0, ref MeasuredPower, ref Overload);

                    return MeasuredPower;
                }
                //public niComplexNumber[] MeasureIqTrace(bool Initiated)
                //{
                //    //if (!Initiated) Initiate();
                //    //double[] iqData = null;
                //    //bool overLoad = false;
                //    //try
                //    //{
                //    //    RFSAsession.WaitForData(500);
                //    //    // You can pull the entire amount or a subset using a different starting point
                //    //    RFSAsession.IQAcquisition.ReadIQData(0, 0, (int)NumberOfSamples, ref iqData, ref overLoad);
                //    //}
                //    //catch (Exception e)
                //    //{
                //    //    MessageBox.Show("Time out for capture data!");
                //    //}


                //    //niComplexNumber[] RawData = new niComplexNumber[NumberOfSamples];
                //    ////int j = 0;
                //    ////for (int i = 0; i < iqData.Count() - 1; i = i + 2)
                //    ////{
                //    ////    RawData[j].Imaginary = iqData[i] ;
                //    ////    RawData[j].Real = iqData[i + 1];
                //    ////    j++;
                //    ////}
                //    //niComplexNumber[] Data = ApplyExternalGain(RawData);//Manually apply external gain on data array
                //    //return Data;
                //}
                public niComplexNumber[] ApplyExternalGain(niComplexNumber[] Data)
                {
                    //double[] PoutTrace = new double[Data.Count()];
                    //// Convert Volt to dBm and apply external Gain
                    //for (int i = 0; i < PoutTrace.Length; i++)
                    //{
                    //    PoutTrace[i] = 10.0 * Math.Log10((Data[i].Real * Data[i].Real + Data[i].Imaginary * Data[i].Imaginary) / 2.0 / 50.0 * 1000.0) - ExternalGain;
                    //}
                    ////convert dBm to Volt
                    niComplexNumber[] DataVolt = new niComplexNumber[NumberOfSamples];
                    //for (int i = 0; i < PoutTrace.Length; i++)
                    //{
                    //    DataVolt[i].Imaginary = Math.Sqrt(Math.Pow(10, PoutTrace[i] / 10) / 1000 * 2 * 50 / 2);
                    //    DataVolt[i].Real = DataVolt[i].Imaginary;
                    //}
                    return DataVolt;
                }
                public void MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace)
                {
                    
                    //if (VXT._servoEnabled)   // db modified from:  Temp.TestCon.TestMode == "RF"
                    //{
                    //    for (int i = 0; i < VXT.ActiveWaveform.AclrSettings.Name.Count; i++)
                    //    {
                    //        aclrResults.adjacentChanPowers[i].upperDbc = AclrResult[i * 2];
                    //        aclrResults.adjacentChanPowers[i].lowerDbc = AclrResult[i * 2 + 1];
                    //    }

                    //}
                    //else
                    //{
                    //    iqTrace = MeasureIqTrace(true);
                    //}
                }

                public void Initiate()
                {

                    ////You first need to put it into IQ capture mode
                    //RFSAsession.AcquisitionMode = KtM9420AcquisitionModeEnum.KtM9420AcquisitionModeIQ;
                    //// Set units.
                    //RFSAsession.IQAcquisition.Units = KtM9420IQUnitsEnum.KtM9420IQUnitsVoltsPeak;
                    //// setup parameters are: IQ word size, sample rate, # of samples
                    //RFSAsession.IQAcquisition.Configure((double)VXT.ActiveWaveform.VsaIQrate, (int)NumberOfSamples);
                    //RFSAsession.Apply();
                    //// arm the VSA
                    //RFSAsession.Arm();
                    //RFSAsession.WaitUntilArmed(-1);
                }

                public void Abort()
                {

                }

                void iEqSA.Initialize(string VSAname)
                {
                    throw new NotImplementedException();
                }

                double iEqSA.MeasureChanPower()
                {
                    throw new NotImplementedException();
                }

                niComplexNumber[] iEqSA.MeasureIqTrace(bool Initiated)
                {
                    throw new NotImplementedException();
                }

                void iEqSA.MeasureAclr(ref AclrResults aclrResults, ref niComplexNumber[] iqTrace)
                {
                    throw new NotImplementedException();
                }

                void iEqSA.Initiate()
                {
                    throw new NotImplementedException();
                }

                void iEqSA.Abort()
                {
                    throw new NotImplementedException();
                }

                void iEqSA.ConfigureTrigger(string waveformname)
                {
                    throw new NotImplementedException();
                }

                double iEqSA.ReadTemp()
                {
                    double temperature = 0;
                    
                    temperature = M9391.Modules.get_Item("M9300A").Temperature;

                    return temperature;
                }

                public double SampleRate
                {
                    get
                    {
                        return _sampleRate;
                    }
                    set
                    {
                          // add code to set VXT here
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

                        // add code to set VXT here
                        _numberOfSamples = value;
                    }
                }

                string iEqSA.Model
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

  

                double iEqSA.ReferenceLevel
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                double iEqSA.ExternalGain
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                double iEqSA.SampleRate
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                long iEqSA.NumberOfSamples
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                TriggerLine iEqSA.TriggerIn
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                TriggerLine iEqSA.TriggerOut
                {
                    get
                    {
                        throw new NotImplementedException();
                    }

                    set
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            public class PxiVSG_VSA_Sg : iEqSG
            {
                public KeysightVXT VXT { get; set; }
                public IKtM9420Ex RFSGsession;
                private double _rfFrequency, _powerLevel, _externalGain;
                private double instRFpower;
                //public PxiVxtSg(KeysightVXT VXT)
                //{
                //    this.VXT = VXT;
                //}
                //public void Pass_session(IKtM9420Ex session)
                //{
                //    RFSGsession = session;
                //}
                public void Initialize(string VSGname)
                {

                }
                public double ReadTemp()
                {
                    return 0;
                }

                public string Model
                {
                    get
                    {
                        return "KeysightM9381";
                    }
                }

                public double Level
                {
                    get
                    {
                        return _powerLevel;
                    }
                    set
                    {
                    
                        if (value > 12f) instRFpower = 19; //Keeps from overdriving the basebandPower which is max 0
                        else instRFpower = 6;

                        M9381.RF.Level = instRFpower;
                        M9381.Modulation.BasebandPower = (value - instRFpower);
                        _powerLevel = value;
                        M9381.Apply();
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
                        _externalGain = value;
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
                        M9381.RF.Frequency = value * 1e6f;
                        M9381.Modulation.BasebandFrequency = 0;
                        _rfFrequency = value;
                    }
                }

                public void SendSoftwareTrigger()
                {

                }

                public void ExportSignal(byte markerNum, TriggerLine TrigLine)
                {
                    //KtM9420MarkerEnum MarkerEventStr =
                    //   markerNum == 0 ? KtM9420MarkerEnum.KtM9420Marker1 :
                    //   markerNum == 1 ? KtM9420MarkerEnum.KtM9420Marker2 :
                    //   markerNum == 2 ? KtM9420MarkerEnum.KtM9420Marker3 :
                    //   markerNum == 3 ? KtM9420MarkerEnum.KtM9420Marker4 :
                    //   KtM9420MarkerEnum.KtM9420Marker1;
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.Type = KtM9420SourceSynchronizationTriggerTypeEnum.KtM9420SourceSynchronizationTriggerTypeDataMarker;
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.DataMarker = MarkerEventStr;
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.Mode = KtM9420TriggerModeEnum.KtM9420TriggerModePulse;
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.Polarity = KtM9420TriggerPolarityEnum.KtM9420TriggerPolarityPositive;
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.PulseWidth = 10e-6;
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.Destination = VXT.TranslateTriggerLine(TrigLine);
                    //RFSGsession.Source.Tigger.SynchronizationOutputTrigger.Enable = true;
                }

                public void Initiate()
                {
                    M9381.RF.OutputEnabled = true;
                }

                public void Abort()
                {
                    M9381.Modulation.BasebandPower = -50;
                    M9381.Apply();
                    M9381.RF.OutputEnabled = true;
                }
            }
            //private KtM9420TriggerEnum TranslateTriggerLine(TriggerLine trigLine)
            //{
            //    //switch (trigLine)
            //    //{
            //    //    case TriggerLine.FrontPanel0:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;

            //    //    case TriggerLine.FrontPanel1:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;

            //    //    case TriggerLine.FrontPanel2:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;

            //    //    case TriggerLine.FrontPanel3:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerFrontPanelTrigger;

            //    //    default:
            //    //    case TriggerLine.PxiTrig0:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger0;

            //    //    case TriggerLine.PxiTrig1:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger1;

            //    //    case TriggerLine.PxiTrig2:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger2;

            //    //    case TriggerLine.PxiTrig3:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger3;

            //    //    case TriggerLine.PxiTrig4:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger4;

            //    //    case TriggerLine.PxiTrig5:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger5;

            //    //    case TriggerLine.PxiTrig6:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger6;

            //    //    case TriggerLine.PxiTrig7:
            //    //        return KtM9420TriggerEnum.KtM9420TriggerPXITrigger7;

            //    //}




            //}
        }

    }
}
