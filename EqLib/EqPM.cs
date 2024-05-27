using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InstrumentDrivers;
using System.Windows.Forms;

namespace EqLib
{
    public class EqPM
    {
        public static iEqPM Get (string model, string serialNumber)
        {
            iEqPM pm;

            switch (model)
            {
                case "Z11":
                case "Z21":
                case "8S":
                default:
                    pm = new RSNRPZ(model, serialNumber);
                    break;
            }

            pm.Initialize();

            return pm;
        }

        public class RSNRPZ : iEqPM
        {
            public string SN;
            public string Model;

            private rsnrpz myRSnrp;

            private double previousMeasLength = 0;
            private int previousNumAvgs = 0;
            private bool isInitialized = false;

            public RSNRPZ(string Model, string SN)
            {
                this.SN = SN;
                this.Model ="NRP" + Model;
            }

            public void Initialize()
            {
                try
                {
                    if (isInitialized) return;

                    if (SN != "")
                    {
                        string modelCode =
                            Model == "NRPZ11" ? "0x000C" :
                            Model == "NRPZ21" ? "0x0003" :
                            Model == "NRP8S" ? "0x00E2" :
                            "";

                        myRSnrp = new rsnrpz("USB::0x0aad::" + modelCode + "::" + SN, true, true);
                    }
                    else
                    {
                        myRSnrp = new rsnrpz("*", true, true);  // kinda ugly. assume single-site (1 sensor) if SN == ""

                        StringBuilder sb_model = new StringBuilder();
                        StringBuilder sb_type = new StringBuilder();
                        StringBuilder sb_serial = new StringBuilder();
                        myRSnrp.GetSensorInfo(1, sb_model, sb_type, sb_serial);
                        Model = sb_type.ToString();
                        SN = sb_serial.ToString();
                    }

                    myRSnrp.reset();
                    previousMeasLength = 0;

                    isInitialized = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }

            public void Zero()
            {
                myRSnrp.chan_zero(1);
            }

            public void SetupMeasurement(double measureFreqMHz, double measLengthS, int numAvgs)
            {
                try
                {
                    myRSnrp.chan_mode(1, InstrumentDrivers.rsnrpzConstants.SensorModeTimeslot);
                    myRSnrp.chan_setCorrectionFrequency(1, measureFreqMHz * 1e6); // Set corr frequency
                    myRSnrp.trigger_setSource(1, InstrumentDrivers.rsnrpzConstants.TriggerSourceImmediate);
                    SetupMeasLength(measLengthS);
                    SetupNumAverages(numAvgs);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }


            public void SetupBurstMeasurement(double measureFreqMHz, double MeasLengthS, double triggerLevDbm, int numAvgs)
            {
                try
                {
                    myRSnrp.chan_mode(1, InstrumentDrivers.rsnrpzConstants.SensorModeTimeslot);
                    myRSnrp.chan_setCorrectionFrequency(1, measureFreqMHz * 1e6); // Set corr frequency
                    myRSnrp.trigger_setSource(1, InstrumentDrivers.rsnrpzConstants.TriggerSourceInternal);
                    SetupMeasLength(MeasLengthS);
                    double trigLev = Math.Pow(10.0, triggerLevDbm / 10.0) / 1000.0;
                    myRSnrp.trigger_setLevel(1, trigLev);
                    SetupNumAverages(numAvgs);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }

            public void SetupBurstMeasurement(double measureFreqMHz, double MeasLengthS, int numAvgs)
            {
                try
                {
                    myRSnrp.chan_mode(1, InstrumentDrivers.rsnrpzConstants.SensorModeTimeslot);
                    myRSnrp.chan_setCorrectionFrequency(1, measureFreqMHz * 1e6); // Set corr frequency
                    myRSnrp.trigger_setSource(1, Model == "NRP8S"? InstrumentDrivers.rsnrpzConstants.TriggerSourceExternal2 : InstrumentDrivers.rsnrpzConstants.TriggerSourceExternal);
                    SetupMeasLength(MeasLengthS);
                    SetupNumAverages(numAvgs);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }

            public void SetupMeasLength(double measLengthS)
            {
                try
                {
                    if (true | measLengthS != previousMeasLength)
                    {
                        myRSnrp.tslot_configureTimeSlot(1, 1, measLengthS);
                        previousMeasLength = measLengthS;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }

            public void SetupNumAverages(int numAvgs)
            {
                try
                {
                    if (true | numAvgs != previousNumAvgs)
                    {
                        myRSnrp.avg_configureAvgManual(1, numAvgs);
                        previousNumAvgs = numAvgs;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }

            public double Measure()
            {
                double[] measDataWatts = new double[100];
                int readCount;
                double measValDbm = -2000;

                try
                {
                    myRSnrp.meass_readBufferMeasurement(1, 1000, 10, measDataWatts, out readCount);

                    measValDbm = 10d * (double)Math.Log10(1000.0 * Math.Abs(measDataWatts[0]));
                }
                catch (Exception e)
                {
                    myRSnrp.chan_abort(1);
                    MessageBox.Show(measValDbm.ToString() + "\n\n" + e.ToString());
                }

                if (double.IsNaN(measValDbm) || (measValDbm < -100 || measValDbm > 100))    // need this in case of NAN or -inifinity
                {
                    measValDbm = -2000;
                }

                return measValDbm;
            }
        }

        public interface iEqPM
        {
            void Initialize();
            void Zero();
            void SetupMeasurement(double measureFreqMHz, double measLengthS, int numAvgs);
            void SetupBurstMeasurement(double measureFreqMHz, double MeasLengthS, double triggerLevDbm, int numAvgs);
            void SetupBurstMeasurement(double measureFreqMHz, double MeasLengthS, int numAvgs);
            void SetupMeasLength(double measLengthS);
            void SetupNumAverages(int numAvgs);
            double Measure();
        }
    }
}
