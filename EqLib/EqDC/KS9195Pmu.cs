using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;


namespace EqLib
{
    public partial class EqDC
    {
        public class Ks9195Pmu : iEqDC
        {
            public string VisaAlias { get; set; }
            public string SerialNumber
            {
                get
                {
                    return "NA";
                }
            }
            public string ChanNumber { get; set; }
            public string PinName { get; set; }
            public byte Site { get; set; }
            public double priorVoltage { get; set; }
            public double priorCurrentLim { get; set; }

            private EqHSDIO.KeysightDSR dsr
            {
                get
                {
                    return (EqHSDIO.KeysightDSR) Eq.Site[Site].HSDIO;
                }
            }


            private double currentForce;
            private bool ppmuIsActive;
            private double measureTimeLength;

            public Ks9195Pmu(string VisaAlias, string ChanNumber, string PinName, byte Site)
            {
                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;
                this.Site = Site;

                Eq.InstrumentInfo += PinName + Site + " = " + dsr.driver.Identity.InstrumentModel + " fw" + dsr.driver.Identity.InstrumentFirmwareRevision + " r" + dsr.driver.Identity.Revision + "*" + SerialNumber + "; ";
            }
            public double ReadTemp(double Temp)
            {
                return Temp;//wait for implement
            }
            public void ForceVoltage(double voltsForce, double currentLimit)
            {
                try
                {
                    //ActivatePPMU(true);

                    //HSDIO.Keysight.driver.PpmuSites.Item[PinName].ForceVoltageMeasureCurrent(voltsForce, currentLimit);
                    dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].Force(Keysight.KtMDsr.Interop.KtMDsrPpmuSiteForceModeEnum.KtMDsrPpmuSiteForceModeVoltage, voltsForce);
                    //dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].ForceVoltageMeasureCurrent(voltsForce, currentLimit);
                    priorVoltage = voltsForce;
                    priorCurrentLim = currentLimit;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ks9195 ForceVoltage");
                }
            }

            public void SetupCurrentMeasure(double aperture, TriggerLine trigLine)
            {
                ActivatePPMU(true);
            }

            public double MeasureCurrent(int NumAverages)
            {
                try
                {
                    double[] results = dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].ForceVoltageMeasureCurrent(priorVoltage, priorCurrentLim);
                    return results[0];
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, TriggerLine trigLine)
            {
                ActivatePPMU(true);
                this.measureTimeLength = measureTimeLength;
            }

            public double[] MeasureCurrentTrace()
            {
                try
                {
                    List<double> measurements = new List<double>();

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    while (sw.ElapsedMilliseconds <= measureTimeLength * 1000 || measurements.Count() < 1)
                    {
                        double[] results = dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].ForceVoltageMeasureCurrent(priorVoltage, priorCurrentLim);
                        measurements.Add(results[0]);
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
                ActivatePPMU(true);
            }

            public double MeasureVoltage(int NumAverages)
            {
                try
                {
                    double[] results = dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].ForceCurrentMeasureVoltage(currentForce);

                    if (double.IsInfinity(results[0]))
                    {
                        results[0] = -999;
                    }
                    
                    return results[0];
                }
                catch (Exception e)
                {
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
                if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName.ToUpper()))
                {
                    if (PinName.Contains("Vsh"))
                    {
                        int[] ChanNum = new int[1];
                        ChanNum[0] = Convert.ToInt16(ChanNumber);

                        dsr.driver.Channels.ClampVoltages(ChanNum, 0.0, settings.Volts);
                        dsr.driver.Channels.ClampVoltagePpmuEnable(ChanNum, true);
                    }
                    ActivatePPMU(true);
                }
                
            }

            public void PostLeakageTest()
            {
                if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName.ToUpper()) && PinName.ToUpper() != "Vio")  // return MIPI channel to digital state
                {
                    ActivatePPMU(false);
                }
            }

            public void SetupContinuity(double currentForce)
            {
                ActivatePPMU(true);
                dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].Force(Keysight.KtMDsr.Interop.KtMDsrPpmuSiteForceModeEnum.KtMDsrPpmuSiteForceModeCurrent, currentForce);
                this.currentForce = currentForce;
            }
            public void DeviceSelfCal() { }
            public double MeasureContinuity(int avgs)
            {
                double result = MeasureVoltage(avgs);

                //if (Eq.Site[Site].HSDIO.IsMipiChannel(PinName))  // return MIPI channel to digital state
                //{
                //    ActivatePPMU(false);
                //}

                return result;
            }

            private void ActivatePPMU(bool activate)
            {
                if (activate != ppmuIsActive)
                {
                    if (activate)
                    {
                        dsr.driver.PpmuSites.Activate(EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber);  // or Inactivate?
                        dsr.driver.PpmuSites.Item[EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber].AverageMode = Keysight.KtMDsr.Interop.KtMDsrPpmuSiteAverageModeEnum.KtMDsrPpmuSiteAverageModeAverage64;  // do this here, since we can't do this during initialization
                    }
                    else
                        dsr.driver.PpmuSites.InactivateAndDisable(EqHSDIO.KeysightDSR.ppmuSiteAliasPrefix + ChanNumber);  // or Inactivate?

                    ppmuIsActive = activate;
                }
            }
        }
        
    }
}
