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
        public class NI6570Pmu : iEqDC
        {
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
            private double measureTimeLength;
            private NationalInstruments.ModularInstruments.NIDigital.DigitalPinSet _pin;
            private NationalInstruments.ModularInstruments.NIDigital.DigitalPinSet pin
            {
                get
                {
                    if (_pin == null)
                    {
                        EqHSDIO.NI6570 hsdio = Eq.Site[Site].HSDIO as EqHSDIO.NI6570;
                        _pin = hsdio.DIGI.PinAndChannelMap.GetPinSet(PinName.ToUpper());
                    }
                    return _pin;
                }
            }

            public NI6570Pmu(string VisaAlias, string ChanNumber, string PinName, byte Site)
            {
                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName.ToUpper();
                this.Site = Site;
            }
            public double ReadTemp(double Temp)
            {
                return Temp;//wait for implement
            }
            public void DeviceSelfCal() { }
            public void ForceVoltage(double voltsForce, double currentLimit)
            {
                try
                {
                    // Configure 6570 for PPMU measurements, Output Voltage, Measure Current
               
                    pin.SelectedFunction = NationalInstruments.ModularInstruments.NIDigital.SelectedFunction.Ppmu;
                    pin.Ppmu.OutputFunction = NationalInstruments.ModularInstruments.NIDigital.PpmuOutputFunction.DCVoltage;
                    // Force Voltage Configure
                    pin.Ppmu.DCVoltage.VoltageLevel = voltsForce;

                    // Using the requested current limit to decide the current level range from the values supported for private release of 6570
                    double range = currentLimit;
                    if (Math.Abs(range) < 2e-6) { range = 2e-6; } // +-2uA
                    else if (Math.Abs(range) < 32e-6) { range = 32e-6; } // +-32uA
                    else if (Math.Abs(range) < 128e-6) { range = 128e-6; } // +-128uA
                    else if (Math.Abs(range) < 2e-3) { range = 2e-3; } // +-2mA
                    else if (Math.Abs(range) < 32e-3) { range = 32e-3; } // +-32mA}

                    pin.Ppmu.DCCurrent.CurrentLevelRange = range;
                    pin.Ppmu.DCVoltage.CurrentLimitRange = range;
                    // Perform Voltage Force
                    pin.Ppmu.Source();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "ForceVoltage");
                }
            }

            public void SetupCurrentMeasure(double aperture, TriggerLine trigLine)
            {
            }

            public double MeasureCurrent(int NumAverages)
            {
                try
                {
                    double[] meas = new double[32];

                    // Measure Current
                    meas = pin.Ppmu.Measure(NationalInstruments.ModularInstruments.NIDigital.PpmuMeasurementType.Current);
                    return meas[0];
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "MeasureCurrent");
                    return 0;
                }
            }

            public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, TriggerLine trigLine)
            {
                this.measureTimeLength = measureTimeLength;
            }

            public double[] MeasureCurrentTrace()
            {
                try
                {
                    List<double> measurements = new List<double>();

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    double result = 0;

                    while (sw.ElapsedMilliseconds <= measureTimeLength * 1000 || measurements.Count() < 1)
                    {
                        result = MeasureCurrent(1);
                        measurements.Add(result);
                    }

                    return measurements.ToArray();
                }
                catch (Exception)
                {
                    return new double[16];
                }

            }

            public void SetupVoltageMeasure()
            {
                // Configure for PPMU Measurements
                pin.SelectedFunction = NationalInstruments.ModularInstruments.NIDigital.SelectedFunction.Ppmu;
            }

            public double MeasureVoltage(int NumAverages)
            {
                try
                {
                    double[] meas = new double[32];
                    // Configure Number of Averages by setting the Apperture Time
                    pin.Ppmu.ConfigureApertureTime(0.0020 * (double)(NumAverages), NationalInstruments.ModularInstruments.NIDigital.PpmuApertureTimeUnits.Seconds);
                    // Measure Voltage
                    meas = pin.Ppmu.Measure(NationalInstruments.ModularInstruments.NIDigital.PpmuMeasurementType.Voltage);

                    return meas[0];
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "MeasureCurrent");
                    return 0;
                }
            }

            public void TransientResponse_Fast(ClothoLibAlgo.DcSetting settings)
            {

            }

            public void TransientResponse_Normal(ClothoLibAlgo.DcSetting settings)
            {

            }

            public void PreLeakageTest(ClothoLibAlgo.DcSetting settings)
            {
                pin.SelectedFunction = NationalInstruments.ModularInstruments.NIDigital.SelectedFunction.Ppmu;
            }

            public void PostLeakageTest()
            {
                //if (HSDIO.IsMipiChannel(PinName))  // return MIPI channel to digital state
                {
                    pin.SelectedFunction = NationalInstruments.ModularInstruments.NIDigital.SelectedFunction.Digital;
                }
            }

            public void SetupContinuity(double currentForce)
            {
                // Configure for PPMU measurements, Output Current, Measure Voltage
                SetupVoltageMeasure();
                pin.Ppmu.OutputFunction = NationalInstruments.ModularInstruments.NIDigital.PpmuOutputFunction.DCCurrent;

                // Using the requested current to decide the current level range from the values supported for private release of 6570
                double range = currentForce;
                if (Math.Abs(range) < 128e-6) { range = 128e-6; } // +-128uA
                else if (Math.Abs(range) < 2e-3) { range = 2e-3; } // +-2mA
                else if (Math.Abs(range) < 32e-3) { range = 32e-3; } // +-32mA}

                // Set the current level range and voltage limits
                pin.Ppmu.DCCurrent.CurrentLevelRange = Math.Abs(range);
                pin.Ppmu.DCCurrent.VoltageLimitHigh = 5;
                pin.Ppmu.DCCurrent.VoltageLimitLow = -2;
                // Configure Current Level and begin Sourcing
                pin.Ppmu.DCCurrent.CurrentLevel = currentForce;
                pin.Ppmu.Source();
            }
            public double MeasureContinuity(int avgs)
            {
                double result = MeasureVoltage(avgs);

                //if (HSDIO.IsMipiChannel(PinName))  // return MIPI channel to digital state
                {
                    pin.SelectedFunction = NationalInstruments.ModularInstruments.NIDigital.SelectedFunction.Digital;
                }

                return result;
            }
        }
    }
}
