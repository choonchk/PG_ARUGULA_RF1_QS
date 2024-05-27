using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IqWaveform;
using NationalInstruments.ModularInstruments.Interop;

namespace ClothoLibAlgo
{
    public class PinSweepTestConditionBase
    {
        public IQ.Waveform iqWaveform;

        public Dictionary.Ordered<string, DcSetting> DcSettings = new Dictionary.Ordered<string, DcSetting>();

        public const double CompCalcStep = 0.5;
        public const double CompCalcMax = 7.0;
        public List<double> AllCompLevs = new List<double>();  // List to guarantee datalog order

        public float PinSweepStart;
        public float PinSweepStop;
        public float TargetpoutAtRefgain = float.NaN;

    }

    public class PinSweepResultsBase
    {
        public niComplexNumber[] IQtrace;
        public Dictionary<string, double[]> Itrace = new Dictionary<string, double[]>();

        public double[] PoutTrace;
        public double[] PinTrace;
        public double[] GainTrace;

        public double GainMax = -999;
        public double IccSum = 0;
        public double Itotal = 0;
        public double PinAtGainMax = -999;
        public double PoutAtGainMax = -999;
        public Dictionary<string, double> IatGainMax = new Dictionary<string, double>();
        public double PaeAtGainMax = -999;

        public CompressionResults Comp = new CompressionResults();

        public class CompressionResults
        {
            public Dictionary<double, double> Gain = new Dictionary<double, double>();
            public Dictionary<double, double> Pin = new Dictionary<double, double>();
            public Dictionary<double, double> Pout = new Dictionary<double, double>();
            public Dictionary.DoubleKey<double, string, double> I = new Dictionary.DoubleKey<double, string, double>();
            public Dictionary<double, double> PAE = new Dictionary<double, double>();
        }

        public void AnalyzePinSweep(PinSweepTestConditionBase TestCon)
        {
            try
            {
                #region Pin, Pout, Gain

                PoutTrace = new double[IQtrace.Length];
                PinTrace = new double[IQtrace.Length];
                GainTrace = new double[IQtrace.Length];

                for (int i = 0; i < PoutTrace.Length; i++)
                {
                    PoutTrace[i] = 10.0 * Math.Log10((IQtrace[i].Real * IQtrace[i].Real + IQtrace[i].Imaginary * IQtrace[i].Imaginary) / 2.0 / 50.0 * 1000.0);
                    if (Math.Abs(PoutTrace[i]) > 1000)
                    {
                        PoutTrace[i] = -999;
                    }
                    PinTrace[i] = TestCon.PinSweepStop + TestCon.iqWaveform.PowerBackoff[i];
                    GainTrace[i] = PoutTrace[i] - PinTrace[i];
                }

                //for (int i = 0; i < GainTrace.Length; i += 30)
                //{
                //    Console.WriteLine(PinTrace[i] + "\t" + GainTrace[i]);
                //}

                #endregion

                #region Max Gain

                int maxGainIndex = 0;
                int startSearchIndex = Math.Max(GainTrace.Length / 100, PinTrace.ToList().BinarySearch(TestCon.PinSweepStart));  // make sure to always skip the first 1% of points, since there can be garbage
                double maxGainIndexDbl = 0;

                if (float.IsNaN(TestCon.TargetpoutAtRefgain))
                {
                    #region Max Gain Coarse
                    
                    if (false)
                    {   // start at the right, stop when close to MaxGain
                        int averageSize = GainTrace.Length / 500;
                        int startAvgTemp = Math.Max(Array.IndexOf(GainTrace, GainTrace.Max()) - averageSize / 2, 0);

                        double maxGainTarget = GainTrace.Skip(startAvgTemp).Take(averageSize).Average(); ; // GainTrace.Max();
                        bool found0p05dB = false;

                        for (int i = GainTrace.Length - 1; i >= 0; i -= averageSize)
                        {
                            if (!found0p05dB && Math.Abs(maxGainTarget - GainTrace[i]) < 0.05)
                            {
                                maxGainIndex = i;
                                found0p05dB = true;
                            }

                            if (found0p05dB && Math.Abs(maxGainTarget - GainTrace[i]) < 0.01)
                            {
                                maxGainIndex = i;
                                break;
                            }

                        }
                    }

                    if (true)
                    { // search MaxGain throughout entire array
                        GainMax = -10000;
                        int averageSize = GainTrace.Length / 1000;

                        int startAvg = 0;
                        int stopAvg = 0;

                        for (int i = startSearchIndex; i < GainTrace.Length; i += GainTrace.Length / 500)
                        {
                            startAvg = Math.Max(i - averageSize / 2, 0);
                            stopAvg = Math.Min(i + averageSize / 2, GainTrace.Length);

                            double coarseMaxGain = 0;

                            for (int j = startAvg; j < stopAvg; j++)
                            {
                                coarseMaxGain += GainTrace[j];
                            }
                            coarseMaxGain /= (stopAvg - startAvg);

                            if (coarseMaxGain > GainMax)
                            {
                                GainMax = coarseMaxGain;
                                maxGainIndex = i;
                            }
                        }
                    }

                    if (false)
                    {
                        int averageSize = GainTrace.Length / 200;
                        int numSteps = (int)Math.Ceiling(Math.Pow(4.0 * (double)(GainTrace.Length - startSearchIndex), 1.0 / 3.0) + 1.0);
                        maxGainIndex = FindMaxIndex(startSearchIndex, GainTrace.Length - 1, numSteps, averageSize, ref GainTrace);
                    }

                    #endregion

                    #region Max Gain Fine

                    int maxGainIndexStart = Math.Max(maxGainIndex - GainTrace.Length / 20, startSearchIndex);
                    int maxGainNumPoints = Math.Min(GainTrace.Length / 20 * 2, GainTrace.Length - maxGainIndexStart);

                    double[] gainTemp = GainTrace.Skip(maxGainIndexStart).Take(maxGainNumPoints).ToArray();
                    double[] pinTemp = PinTrace.Skip(maxGainIndexStart).Take(maxGainNumPoints).ToArray();

                    alglib.spline1dinterpolant charPoutSpline = new alglib.spline1dinterpolant();
                    alglib.spline1dfitreport charReport = new alglib.spline1dfitreport(); int info;
                    alglib.spline1dfitpenalized(pinTemp, gainTemp, 6, -6, out info, out charPoutSpline, out charReport);

                    //for (int i = 0; i < gainTemp.Length; i++)
                    //{
                    //    double gainCspline = alglib.spline1dcalc(charPoutSpline, pinTemp[i]);
                    //    Console.WriteLine(pinTemp[i] + "\t" + gainTemp[i] + "\t" + gainCspline);
                    //}

                    GainMax = -10000;   // reset since spline's max gain is usually slightly lower than noisy data's max gain

                    // find integar index of max gain via spline
                    for (int i = 0; i < gainTemp.Length; i++)
                    {
                        double pin = pinTemp[i];
                        double gainCspline = alglib.spline1dcalc(charPoutSpline, pin);

                        if (gainCspline >= GainMax)
                        {
                            GainMax = gainCspline;
                            maxGainIndex = i;
                        }
                    }

                    
                    GainMax = -10000;

                    // find interpolated index of max gain via spline
                    maxGainIndex = Math.Min(Math.Max(maxGainIndex, 1), pinTemp.Length - 2);
                    for (double pin = pinTemp[maxGainIndex - 1]; pin <= pinTemp[maxGainIndex + 1]; pin += 0.005)
                    {
                        double gainCspline = alglib.spline1dcalc(charPoutSpline, pin);

                        if (gainCspline >= GainMax)
                        {
                            GainMax = gainCspline;
                            if (pin <= PinTrace[maxGainIndex])
                                maxGainIndexDbl = (double)maxGainIndex - 1 + (pin - pinTemp[maxGainIndex - 1]) / (pinTemp[maxGainIndex] - pinTemp[maxGainIndex - 1]);
                            else
                                maxGainIndexDbl = (double)maxGainIndex + (pin - pinTemp[maxGainIndex]) / (pinTemp[maxGainIndex + 1] - pinTemp[maxGainIndex]);

                            PinAtGainMax = pin;
                        }
                    }

                    maxGainIndex += maxGainIndexStart;    // convert index from slice to full array
                    maxGainIndexDbl += maxGainIndexStart; // convert index from slice to full array

                    PoutAtGainMax = PinAtGainMax + GainMax;

                    #endregion
                }
                else
                {
                    #region find Gain at Specific pout

                    double coarsePout = 0;

                    for (int i = startSearchIndex; i < GainTrace.Length; ++i)
                    {
                        GainMax = -10000;
                        int averageSize = GainTrace.Length / 500;

                        int startAvg = Math.Max(i - averageSize / 2, 0);
                        int stopAvg = Math.Min(i + averageSize / 2, PoutTrace.Length);

                        coarsePout = 0;

                        for (int j = startAvg; j < stopAvg; j++)
                        {
                            coarsePout += PoutTrace[j];
                        }
                        coarsePout /= (stopAvg - startAvg);

                        if (Math.Abs(TestCon.TargetpoutAtRefgain - coarsePout) < 0.05)
                        {
                            GainMax = coarsePout - PinTrace[i]; //tp.Data.PinSweep.GainTrace[i];  to make high accuracy
                            maxGainIndex = GainTrace.ToList().IndexOf(GainTrace[i]);
                            break;
                        }
                    }
                    maxGainIndexDbl = maxGainIndex;
                    PinAtGainMax = PinTrace[maxGainIndex];
                    PoutAtGainMax = coarsePout;
                    #endregion
                }

                #region Icc at Max Gain

                foreach (string iChan in Itrace.Keys)
                {
                    double IccIndexAtMaxGain = maxGainIndexDbl / (double)GainTrace.Length * (double)Itrace[iChan].Length;
                    IatGainMax[iChan] = Calc.InterpLinear(Itrace[iChan], IccIndexAtMaxGain);
                }

                #endregion

                #region PAE at Max Gain

                float dcPowerAtGainMax = 0;
                foreach (string pinName in Itrace.Keys)
                {
                    dcPowerAtGainMax += (float)(TestCon.DcSettings[pinName].Volts * IatGainMax[pinName]);
                }
                PaeAtGainMax = Convert.ToSingle((Math.Pow(10.0, PoutAtGainMax / 10.0) - Math.Pow(10.0, PinAtGainMax / 10.0)) / dcPowerAtGainMax * 100.0 / 1000.0);

                #endregion

                #endregion

                #region All Compressions

                foreach (double comp in TestCon.AllCompLevs)
                {
                    Comp.Gain[comp] = 0;
                    Comp.Pin[comp] = 0;
                    Comp.Pout[comp] = 0;
                    Comp.PAE[comp] = 0;
                    foreach (string iChan in Itrace.Keys)
                    {
                        Comp.I[comp, iChan] = 0;
                    }
                }

                double currentCompLev = PinSweepTestConditionBase.CompCalcStep;

                for (int i = maxGainIndex; i < GainTrace.Length & currentCompLev <= PinSweepTestConditionBase.CompCalcMax; i++)
                {
                    if (GainTrace[i] < GainMax - currentCompLev)
                    {
                        Comp.Gain[currentCompLev] = GainTrace[i];
                        Comp.Pin[currentCompLev] = PinTrace[i];
                        Comp.Pout[currentCompLev] = PoutTrace[i];

                        double dcPower = 0;
                        foreach (string iChan in Itrace.Keys)
                        {
                            double IccIndex = (double)i / (double)GainTrace.Length * (double)Itrace[iChan].Length;
                            Comp.I[currentCompLev, iChan] = Calc.InterpLinear(Itrace[iChan], IccIndex);
                            dcPower += (float)(TestCon.DcSettings[iChan].Volts * Comp.I[currentCompLev, iChan]);
                        }
                        Comp.PAE[currentCompLev] = Convert.ToSingle((Math.Pow(10.0, Comp.Pout[currentCompLev] / 10.0) - Math.Pow(10.0, Comp.Pin[currentCompLev] / 10.0)) / dcPower * 100.0 / 1000.0);

                        currentCompLev += PinSweepTestConditionBase.CompCalcStep;
                    }
                }

                #endregion
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Analyze Pin Sweep");
            }

        }

        private static double SearchSpline(double yTarget, alglib.spline1dinterpolant spline, double xStart, double xStop)
        {
            double xVal = (xStart + xStop) / 2.0;
            double yVal;
            double ds, d2s;
            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {
                alglib.spline1ddiff(spline, xVal, out yVal, out ds, out d2s);

                if (Math.Abs(yTarget - yVal) < Math.Abs(yTarget * 0.000001))
                {
                    return xVal;
                }

                xVal += Math.Min((yTarget - yVal) / ds, (xStop - xStart) / 6);
                if (xVal < xStart | xVal > xStop)
                {
                    xVal = (double)random.Next((int)xStart, (int)xStop);
                }
            }

            return 0;
        }

        private static int FindMaxIndex(int startIndex, int stopIndex, int numSteps, int pointsToAvg, ref double[] array)
        {   // Divide and Conquer algorithm

            List<double> arrayDecimated = new List<double>();
            List<int> arrayDecimatedIndices = new List<int>();

            double stepSize = Math.Max((double)(stopIndex - startIndex) / (double)(numSteps - 1), 1.0);

            for (double index = startIndex; (int)index <= stopIndex; index += stepSize)
            {
                int indexInt = (int)index;

                int startAvg = Math.Max(indexInt - pointsToAvg / 2, 0);
                int stopAvg = Math.Min(indexInt + pointsToAvg / 2, array.Length - 1);

                double avg = 0;

                for (int j = startAvg; j <= stopAvg; j++)
                {
                    avg += array[j];
                }
                avg /= (stopAvg - startAvg + 1);

                arrayDecimated.Add(avg);
                arrayDecimatedIndices.Add(indexInt);
            }

            int actualNumSteps = arrayDecimated.Count();

            int maxIndexDecimated = arrayDecimated.IndexOf(arrayDecimated.Max());

            int nextStartIndex = 0, nextStopIndex = 0;

            if (stepSize == 1)
            {
                return arrayDecimatedIndices[maxIndexDecimated];
            }

            if (maxIndexDecimated == 0)
            {
                nextStartIndex = arrayDecimatedIndices[0];
                nextStopIndex = arrayDecimatedIndices[1];
            }
            else if (maxIndexDecimated == actualNumSteps - 1)
            {
                nextStartIndex = arrayDecimatedIndices[actualNumSteps - 2];
                nextStopIndex = arrayDecimatedIndices[actualNumSteps - 1];
            }
            else
            {
                //if (arrayDecimated[maxIndexDecimated - 1] > arrayDecimated[maxIndexDecimated + 1])
                //{
                //    nextStartIndex = arrayDecimatedIndices[maxIndexDecimated - 1];
                //    nextStopIndex = arrayDecimatedIndices[maxIndexDecimated];
                //}
                //else
                //{
                //    nextStartIndex = arrayDecimatedIndices[maxIndexDecimated];
                //    nextStopIndex = arrayDecimatedIndices[maxIndexDecimated + 1];
                //}

                nextStartIndex = arrayDecimatedIndices[maxIndexDecimated - 1];
                nextStopIndex = arrayDecimatedIndices[maxIndexDecimated + 1];
            }

            return FindMaxIndex(nextStartIndex, nextStopIndex, numSteps, pointsToAvg, ref array);

        }

    }

}
