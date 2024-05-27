using System;
using System.Threading;
using Avago.ATF.LogService;


namespace GuCal
{
    public partial class GU
    {
        public abstract class IccCalBase
        {
            private int site;
            private float TargetPout;
            private double Frequency;
            private float InputPathGain;
            private float OutputPathGain;
            private int DelaySg;
            private string ModulationStd;
            private string Waveform;

            private string poutTestName;
            private string pinTestName;
            private string iccTestName;
            private string keyName;
            private bool ApplyIccTargetCorrection;

            private static object locker = new object();

            public IccCalBase(int _site, float _TargetPout, double _Frequency, float _InputPathGain, float _OutputPathGain, int _DelaySg,
                string _ModulationMode, string _Waveform,
                string _poutTestName, string _pinTestName, string _iccTestName, string _keyName, bool _applyIccTargetCorrection)
            {
                this.site = _site;
                this.TargetPout = _TargetPout;
                this.Frequency = _Frequency;
                this.InputPathGain = _InputPathGain;
                this.OutputPathGain = _OutputPathGain;
                this.DelaySg = _DelaySg;
                this.ModulationStd = _ModulationMode;
                this.Waveform = _Waveform;

                this.poutTestName = _poutTestName;
                this.pinTestName = _pinTestName;
                this.iccTestName = _iccTestName;
                this.keyName = _keyName;
                this.ApplyIccTargetCorrection = _applyIccTargetCorrection;

            }

            public bool Execute(ref double Pin, ref double Pout, ref double Icc)
            {
                if (currentGuDutSN[site] == -1) return true;  // if no device in this site, exit

                const float COARSE_DB_STEP = 1f;       // used for determining maximum input power for fine search
                const float FINE_DB_UPPER_MARGIN = 1f;       // add up to FINE_DB_UPPER_MARGIN to maximum input power for fine search
                const float IDD_TOLERANCE = 200f;      // servos to tolerance of target_idd/IDD_TOLERANCE
                const int MAX_SEARCH_ATTEMPTS = 10;        // will perform the entire SA search up to MAX_SEARCH_ATTEMPTS times until succeeds

                lock (locker)
                {
                    ApplyIccServoTargetCorrection[keyName + iccCalTestNameExtension] = ApplyIccTargetCorrection;
                    if (iccCalFactorRedirect.ContainsKey(keyName + iccCalTestNameExtension)) return true;
                    if (!factorAddEnabledTests.Contains(keyName + iccCalTestNameExtension) & IccCalTemplateExists) return true;
                }

                if (!benchTestNameList.ContainsValue(iccTestName))
                {
                    if (IccCalTemplateExists)
                    {
                        LogToLogServiceAndFile(LogLevel.Error, "ERROR: Icc Cal method was passed \"" + iccTestName + "\" but this test name is not found in the bench data file\n" + benchDataPath + "\n      Cannot continue " + "GU Calibration");
                        runningGUIccCal[site] = false;
                        runningGU[site] = false;
                        forceReload = true;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (!benchTestNameList.ContainsValue(pinTestName))
                {
                    if (IccCalTemplateExists)
                    {
                        LogToLogServiceAndFile(LogLevel.Error, "ERROR: Icc Cal method was passed \"" + pinTestName + "\" but this test name is not found in the bench data file\n" + benchDataPath + "\n      Cannot continue " + "GU Calibration");
                        runningGUIccCal[site] = false;
                        runningGU[site] = false;
                        forceReload = true;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (!benchTestNameList.ContainsValue(poutTestName))
                {
                    if (IccCalTemplateExists)
                    {
                        LogToLogServiceAndFile(LogLevel.Error, "ERROR: Icc Cal method was passed \"" + poutTestName + "\" but this test name is not found in the bench data file\n" + benchDataPath + "\n      Cannot continue " + "GU Calibration");
                        runningGUIccCal[site] = false;
                        runningGU[site] = false;
                        forceReload = true;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (IccCalTemplateExists && !iccCalTemplateTestNameList.Contains(keyName + iccCalTestNameExtension))
                {
                    LogToLogServiceAndFile(LogLevel.Error, "ERROR: Icc Cal method was passed \"" + poutTestName + "\" but " + poutTestName + iccCalTestNameExtension + " is not found in the Icc Cal Template file\n" + iccCalTemplatePath + "\n      Cannot continue " + "GU Calibration");
                    runningGUIccCal[site] = false;
                    runningGU[site] = false;
                    forceReload = true;
                    return false;
                }

                double benchIcc = GU.finalRefDataDict[selectedBatch, iccTestName, currentGuDutSN[site]];
                if (ApplyIccTargetCorrection) benchIcc += IccServoTargetCorrection[site, keyName + iccCalTestNameExtension];
                double benchPin = GU.finalRefDataDict[selectedBatch, pinTestName, currentGuDutSN[site]];
                double benchPout = GU.finalRefDataDict[selectedBatch, poutTestName, currentGuDutSN[site]];

                IccCalTestNames.Add(pinTestName, poutTestName, iccTestName, keyName, TargetPout, Frequency, ModulationStd + "-" + Waveform);

                if (!IccCalTemplateExists)
                {
                    Pin = benchPin;
                    Pout = benchPout;
                    Icc = benchIcc;
                    GuIccCalFailed[site] = true;
                    return true;
                }

                double iccTolerance = benchIcc / IDD_TOLERANCE;
                double rfLvl = 0;
                bool coarseSearchDone = false;
                bool servoPassed = false;

                // BEGIN SEARCH ATTEMPTS LOOP.  Each search attempt performs a coarse and fine search with slightly different boundaries, to maximize servo success
                for (int searchAttempt = 0; searchAttempt < MAX_SEARCH_ATTEMPTS; searchAttempt++)
                {
                    // COARSE DETERMINATION OF HI POWER LIMIT    (ensures that we don't compress the part during fine search)
                    coarseSearchDone = false;
                    double pinLoLim = benchPin - 10f;  // P-in starts out this low
                    double pinHiLim = benchPin + 10f;  // P-in could potentially go this high, but not likely since P-in stops increasing once measured Icc exceeds bench Icc

                    for (int coarseIndex = 0; coarseIndex < (pinHiLim - pinLoLim) / COARSE_DB_STEP + 1; coarseIndex++)
                    {
                        rfLvl = pinLoLim + coarseIndex * COARSE_DB_STEP + searchAttempt / MAX_SEARCH_ATTEMPTS * 0.9f;

                        SetPowerLevel(rfLvl - InputPathGain);

                        Thread.Sleep(DelaySg);

                        Icc = MeasureIcc();

                        if (Icc > benchIcc + iccTolerance)
                        {
                            pinHiLim = rfLvl + (float)searchAttempt / (float)MAX_SEARCH_ATTEMPTS * FINE_DB_UPPER_MARGIN;
                            pinLoLim = rfLvl - COARSE_DB_STEP * 3;
                            coarseSearchDone = true;
                            break;
                        }
                    }  // coarse precision loop

                    if (!coarseSearchDone) break;

                    // FINE ICC SEARCH    (within icc_tolerance)
                    double previousIcc = 0;
                    double previousRFLvl = 0f;
                    double iccPinSlope = 0f;

                    for (int iteration = 1; iteration < 50; iteration++)
                    {
                        previousRFLvl = rfLvl;
                        previousIcc = Icc;

                        if (iteration == 1)
                        {
                            rfLvl = pinHiLim - 2f;
                        }
                        else if (iteration == 2)
                        {
                            rfLvl = pinHiLim - 1f;
                        }
                        else
                        {
                            rfLvl = rfLvl + (float)(benchIcc - Icc) * 0.8f / (iccPinSlope);   // *0.8 helps avoid overshooting beyong limits
                            if (rfLvl < pinLoLim | rfLvl > pinHiLim)
                            {
                                break;  // P-in exceeded limits, try next search attempt
                            }
                        }

                        SetPowerLevel(rfLvl - InputPathGain);

                        Thread.Sleep(DelaySg);

                        Icc = MeasureIcc();

                        if (Math.Abs(Icc - benchIcc) <= iccTolerance)
                        {
                            servoPassed = true;
                            break;
                        }

                        if (iteration > 1)
                        {
                            iccPinSlope = (double)(Icc - previousIcc) / (rfLvl - previousRFLvl);
                        }

                    }  // fine precision loop

                    if (servoPassed) break;  // conclude search attempts

                }  // search attempts (includes coarse and fine precision searches)


                Pout = MeasurePout();

                Pout -= OutputPathGain;

                Pin = rfLvl;

                if (!servoPassed)
                {
                    LogToLogServiceAndFile(LogLevel.Warn, "Icc search failed, site " + (site + 1) + ", test " + poutTestName + ", target Icc: " + benchIcc + "A, measured Icc: " + Icc + "A");
                    GuIccCalFailed[site] = true;
                }

                return servoPassed;

            }

            public abstract float MeasurePout();
            public abstract double MeasureIcc();
            public abstract void SetPowerLevel(double powerLevel);
        }

    }
}



