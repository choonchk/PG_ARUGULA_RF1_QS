using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

using NationalInstruments.ModularInstruments.Interop;


namespace EqLib
{
    public partial class EqDC
    {
        public class NI6556Pmu : iEqDC
        {
            public string VisaAlias { get; set; }
            public string ChanNumber { get; set; }
            public string PinName { get; set; }
            public byte Site { get; set; }
            public double priorVoltage { get; set; }
            public double priorCurrentLim { get; set; }

            private double measureTimeLength;

            public NI6556Pmu(string VisaAlias, string ChanNumber, string PinName, byte Site)
            {
                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;
                this.Site = Site;
            }
            public void DeviceSelfCal() { }
            public void ForceVoltage(double voltsForce, double currentLimit)
            {
                try
                {
                    EqHSDIO.NI6556.GenSession.STPMU_SourceVoltage(ChanNumber, voltsForce, niHSDIOConstants.StpmuLocalSense, currentLimit);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "ForceVoltage");
                }
            }
            public double ReadTemp(double Temp)
            {
                return Temp;//wait for implement
            }
            public void SetupCurrentMeasure(double aperture, TriggerLine trigLine)
            {
            }
            public double MeasureCurrent(int NumAverages)
            {
                try
                {
                    double[] meas = new double[32];
                    int i = 0;

                    EqHSDIO.NI6556.GenSession.STPMU_MeasureCurrent(ChanNumber, 0.0001 * (double)NumAverages, meas, out i);
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
                catch (Exception e)
                {
                    return new double[16];
                }

            }

            public void SetupVoltageMeasure()
            {
            }
            public double MeasureVoltage(int NumAverages)
            {
                try
                {
                    double[] meas = new double[32];
                    int i = 0;

                    EqHSDIO.NI6556.GenSession.STPMU_MeasureVoltage(ChanNumber, 0.0001 * (double)NumAverages, niHSDIOConstants.StpmuLocalSense, meas, out i);
                    return meas[0];
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "MeasureVoltage");
                    return 0;
                }
            }

            public void PreLeakageTest(ClothoLibAlgo.DcSetting settings)
            {
            }
            public void PostLeakageTest()
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName))  // return MIPI channel to digital state
                {
                    EqHSDIO.NI6556.GenSession.STPMU_DisablePMU(ChanNumber, niHSDIOConstants.StpmuReturnToPrevious);
                }
            }

            public void SetupContinuity(double currentForce)
            {
                EqHSDIO.NI6556.GenSession.STPMU_SourceCurrent(ChanNumber, currentForce, Math.Abs(currentForce), -2, 5);
            }
            public double MeasureContinuity(int avgs)
            {
                double result = MeasureVoltage(avgs);
                if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName))  // return MIPI channel to digital state
                {
                    EqHSDIO.NI6556.GenSession.STPMU_DisablePMU(ChanNumber, niHSDIOConstants.StpmuReturnToPrevious);
                }

                return result;
            }
        }
    }
}
