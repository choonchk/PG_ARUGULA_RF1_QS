using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

namespace EqLib
{
    public partial class EqDC
    {
        static double Timeout = 10;

        public class NI4139 : iEqDC
        {
            public nidcpower SMUsession;
            public string VisaAlias { get; set; }
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
            public string ChanNumber { get; set; }
            public string PinName { get; set; }
            public byte Site { get; set; }
            public double priorVoltage { get; set; }
            public double priorCurrentLim { get; set; }
            public double Temp { get; set; }

            ///*NI test Part(case1, case6)*/
            //public int CF = 1000000;
            //public int GBW = 50000;
            //public int PZR = 1;


            public NI4139(string VisaAlias, string ChanNumber, string PinName, byte Site, bool Reset)
            {
                try
                {
                    this.SMUsession = new NationalInstruments.ModularInstruments.Interop.nidcpower(VisaAlias, ChanNumber, Reset, "");

                    this.VisaAlias = VisaAlias;
                    this.ChanNumber = ChanNumber;
                    this.PinName = PinName;
                    this.Site = Site;
                    Eq.InstrumentInfo += GetNiInstrumentInfo(this, SMUsession, Site);

                    SMUsession.SetDouble(nidcpowerProperties.SourceDelay, ChanNumber, 0);
                    SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Normal);
                    //SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Fast); //mario
                    SMUsession.SetDouble(nidcpowerProperties.PowerLineFrequency, ChanNumber, nidcpowerConstants._60Hertz);
                    SMUsession.ConfigureOutputEnabled(ChanNumber, false);
                    SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
                    SMUsession.ConfigureSense(ChanNumber, nidcpowerConstants.Remote);
                    SMUsession.SetInt32(nidcpowerProperties.CurrentLimitAutorange, ChanNumber, nidcpowerConstants.On);
                    SMUsession.SetInt32(nidcpowerProperties.VoltageLevelAutorange, ChanNumber, nidcpowerConstants.Off);
                    SMUsession.ConfigureVoltageLevel(ChanNumber, 0);
                    SMUsession.ConfigureVoltageLevelRange(ChanNumber, 6.5);
                    SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, 0.001);
                    SMUsession.ConfigureOutputEnabled(ChanNumber, true);
                    SMUsession.SetDouble(nidcpowerProperties.VoltageCompensationFrequency, 500);
                    SMUsession.SetDouble(nidcpowerProperties.VoltageGainBandwidth, 20000);
                    SMUsession.SetDouble(nidcpowerProperties.VoltagePoleZeroRatio, 8);

                    try
                    {
                        SMUsession.Initiate();  // This shall return OLP error if previously OLP state exists
                        SMUsession.WaitForEvent(nidcpowerConstants.SourceCompleteEvent, 5);
                    }
                    catch (Exception ex)
                    {
                        // reset OLP
                        if (ex.Message.Contains("OLP"))
                        {
                            ResetOLP();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SMU Initialize");
                }
            }

            void ResetOLP()
            {
                bool stillOLP = true;

                while (stillOLP)
                {
                    try
                    {
                        SMUsession.ResetDevice();
                        SMUsession.Commit();
                        SMUsession.CalSelfCalibrate(ChanNumber);
                        SMUsession.ConfigureVoltageLevel(ChanNumber, 0);
                        stillOLP = false;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }


            public double ReadTemp(double Temp)
            {
                SMUsession.ReadCurrentTemperature(out Temp);
                return Temp;
            }
            public void ForceVoltage(double voltsForce, double currentLimit)
            {
                int error = -1;
                int chanInt = Convert.ToInt16(ChanNumber);

                try
                {
                    if (currentLimit != priorCurrentLim)
                    {
                        // ConfigureCurrentLimit appears to also set the range automatically, because auto-range is on
                        error = SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, currentLimit);
                        priorCurrentLim = currentLimit;

                        //double readRange = SMUsession.GetDouble(nidcpowerProperties.CurrentLimit);
                    }
                    if (voltsForce != priorVoltage)
                    {
                        error = SMUsession.ConfigureVoltageLevel(ChanNumber, voltsForce);
                        priorVoltage = voltsForce;
                    }


                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "ConfigureCurrentLimit");
                }
            }

            private TriggerLine _trigLine;

            public void SetupCurrentMeasure(double aperture, TriggerLine trigLine)
            {
                try
                {
                    _trigLine = trigLine;

                    SMUsession.Abort();

                    SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);
                    SMUsession.SetDouble(nidcpowerProperties.ApertureTime, ChanNumber, aperture);

                    if (trigLine == TriggerLine.None)
                    {
                        SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                    }
                    else
                    {
                        SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);
                        SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);

                        string niTrigLine = TranslateNiTriggerLine(trigLine);
                        SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, niTrigLine);
                    }

                    SMUsession.Initiate();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Site: " + Site + "\n\nPinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "SetupCurrentMeasure");
                }
            }
            public double MeasureCurrent(int NumAverages)
            {
                int error = -1;

                try
                {
                    double[] volt = new double[NumAverages];
                    double[] curr = new double[NumAverages];
                    ushort[] measComp = new ushort[NumAverages];
                    int actCount = 0;
                    double[] voltSingle = new double[1];
                    double[] currSingle = new double[1];

                    for (int avg = 0; avg < NumAverages; avg++)
                    {
                        if (_trigLine == TriggerLine.None)
                        {
                            error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureCurrent, out curr[avg]);
                        }
                        else
                        {
                            error = SMUsession.FetchMultiple(ChanNumber, Timeout, 1, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                            volt[avg] = voltSingle[0];
                            curr[avg] = currSingle[0];
                        }
                    }

                    return curr.Average();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Site: " + Site + "\n\nPinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureCurrent");
                    return 0;
                }
            }

            int NumTraceSamples;
            public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, TriggerLine trigLine)
            {
                try
                {
                    _trigLine = trigLine;

                    SMUsession.Abort();

                    SMUsession.SetDouble(nidcpowerProperties.ApertureTime, ChanNumber, aperture);
                  //  aperture = SMUsession.GetDouble(nidcpowerProperties.ApertureTime);

                    NumTraceSamples = (int)(measureTimeLength / aperture);

                    SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);

                    if (trigLine == TriggerLine.None)
                    {

                        SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.SoftwareEdge);
                        
                    }
                    else
                    {
                        string trig = TranslateNiTriggerLine(trigLine);
                        SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                        SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, trig);
                    }

                    //if (triggered)
                    //{
                    //    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                    //    if (triggerFromSA) SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig1");
                    //    else SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig0");
                    //}
                    //else
                    //{
                    //    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.SoftwareEdge);
                    //}

                    SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, NumTraceSamples);
                    SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, 1);

                    SMUsession.Initiate();

                    if (trigLine == TriggerLine.None) SMUsession.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Site: " + Site + "\n\nPinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "SetupCurrentTraceMeasurement");
                }
            }
            public double[] MeasureCurrentTrace()
            {
                int error = -1;

                try
                {
                    ushort[] measComp = new ushort[NumTraceSamples];
                    int actCount = 0;
                    double[] voltSingle = new double[NumTraceSamples];
                    double[] currSingle = new double[NumTraceSamples];

                    error = SMUsession.FetchMultiple(ChanNumber, Timeout, NumTraceSamples, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                    return currSingle;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Site: " + Site + "\n\nPinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureCurrentTrace");
                    return new double[4];
                }
            }

            public void SetupVoltageMeasure()
            {
                int error = -1;

                try
                {
                    SMUsession.Abort();

                    double measureTimeLength = 0.001;

                    SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                    SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);

                    double dcSampleRate = 200e3;   // this is fixed for NI hardware
                    int SamplesToAvg = (int)(dcSampleRate * measureTimeLength);

                    SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, SamplesToAvg);

                    SMUsession.Initiate();


                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "SetupVoltageMeasure4154");

                }
            }
            public double MeasureVoltage(int NumAverages)
            {
                int error = -1;

                try
                {
                    double[] volts = new double[NumAverages];

                    for (int avg = 0; avg < NumAverages; avg++)
                    {
                        error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureVoltage, out volts[avg]);
                    }

                    return volts.Average();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Site: " + Site + "\n\nPinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureVoltage");
                    return 0;
                }
            }

            public void TransientResponse_Fast(ClothoLibAlgo.DcSetting settings)
            {
                SMUsession.Abort();
                SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Fast);
                SMUsession.Initiate();
            }

            public void TransientResponse_Normal(ClothoLibAlgo.DcSetting settings)
            {
                SMUsession.Abort();
                SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Normal);
                SMUsession.Initiate();
            }

            public void PreLeakageTest(ClothoLibAlgo.DcSetting settings)
            {
                /*Original
                SMUsession.Abort();
                SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Custom);
                SMUsession.Initiate();
                */

                SMUsession.Abort();
                //SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Custom);
                //SMUsession.SetDouble(nidcpowerProperties.VoltageCompensationFrequency, 500);
                //SMUsession.SetDouble(nidcpowerProperties.VoltageGainBandwidth, 20000);
                //SMUsession.SetDouble(nidcpowerProperties.VoltagePoleZeroRatio, 8);
                SMUsession.SetDouble(nidcpowerProperties.VoltageCompensationFrequency, 77.5);
                SMUsession.SetDouble(nidcpowerProperties.VoltageGainBandwidth, 27466);
                SMUsession.SetDouble(nidcpowerProperties.VoltagePoleZeroRatio, 5.77);
                SMUsession.SetDouble(nidcpowerProperties.CurrentCompensationFrequency, 40000);
                SMUsession.SetDouble(nidcpowerProperties.CurrentGainBandwidth, 200000);
                SMUsession.SetDouble(nidcpowerProperties.CurrentPoleZeroRatio, 0.6);
                SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Custom);

                SMUsession.Initiate();
            }
            public void PostLeakageTest()
            {
                SMUsession.Abort();
                SMUsession.SetInt32(nidcpowerProperties.TransientResponse, nidcpowerConstants.Normal);
                SMUsession.Initiate();
            }

            public void SetupContinuity(double currentForce)
            {
                SMUsession.Abort();
                SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcCurrent);
                SMUsession.ConfigureCurrentLevel(ChanNumber, currentForce);
                SMUsession.ConfigureCurrentLevelRange(ChanNumber, Math.Abs(currentForce));

                SetupCurrentMeasure(0.0003, TriggerLine.None);
            }

            public double MeasureContinuity(int avgs)
            {
                double result = MeasureVoltage(avgs);

                SMUsession.Abort();
                SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);

                return result;
            }
            public void DeviceSelfCal()
            {
                SMUsession.CalSelfCalibrate(ChanNumber);

                SMUsession.SetDouble(nidcpowerProperties.SourceDelay, ChanNumber, 0);
                SMUsession.SetDouble(nidcpowerProperties.PowerLineFrequency, ChanNumber, nidcpowerConstants._60Hertz);
                SMUsession.ConfigureOutputEnabled(ChanNumber, false);
                SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
                SMUsession.ConfigureSense(ChanNumber, nidcpowerConstants.Remote);
                SMUsession.SetInt32(nidcpowerProperties.CurrentLimitAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.SetInt32(nidcpowerProperties.VoltageLevelAutorange, ChanNumber, nidcpowerConstants.Off);
                SMUsession.ConfigureVoltageLevel(ChanNumber, 0);
                SMUsession.ConfigureVoltageLevelRange(ChanNumber, 6.5);
                SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, 0.001);
                SMUsession.ConfigureOutputEnabled(ChanNumber, true);
                SMUsession.SetDouble(nidcpowerProperties.VoltageCompensationFrequency, 500);
                SMUsession.SetDouble(nidcpowerProperties.VoltageGainBandwidth, 20000);
                SMUsession.SetDouble(nidcpowerProperties.VoltagePoleZeroRatio, 8);
                SMUsession.Initiate();

                priorVoltage = 0;
                priorCurrentLim = 0;
            }
        }
    }
}
