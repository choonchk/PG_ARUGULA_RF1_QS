using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;

namespace EqLib
{
    public class NIRFMX
    {
        public IntPtr niRfsaHandle;
        internal static RFmxInstrMX instrSession;

        public RfmxAcp cRfmxAcp;
        public RfmxIIP3 cRfmxIIP3;
        

        public IntPtr InitializeInstr(string resourceName)
        {
            /* Create a new RFmx Session */
            instrSession = new RFmxInstrMX(resourceName, "DriverSetup=Bitfile:NI Power Servoing for VST.lvbitx");

            instrSession.ConfigureFrequencyReference("", RFmxInstrMXConstants.PxiClock, 10e6);
            instrSession.ExportSignal(RFmxInstrMXExportSignalSource.ReferenceTrigger, RFmxInstrMXConstants.PxiTriggerLine1);
            instrSession.DangerousGetNIRfsaHandle(out niRfsaHandle);
            instrSession.SetDownconverterFrequencyOffset("", -40 * 1e6);

            cRfmxAcp = new RfmxAcp();
            cRfmxIIP3 = new RfmxIIP3();

            return niRfsaHandle;
        }

        public void ResetDriver()
        {
            instrSession.ResetDriver();
        }    
        public void close()
        {
            instrSession.Close();         
        }        
    }

    public class RfmxAcp
    {
        List<RFmxSpecAnMX> specAcp;
        RFmxInstrMX _instrSession;
        public RfmxAcp()
        {
            specAcp = new List<RFmxSpecAnMX>();
            _instrSession = NIRFMX.instrSession;
        }

        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec(int Iteration, double FreqSG, double Reflevel, double RefChBW, double[] AdjChsBW, double[] AdjChsFreqOffset, string WaveformName, bool TestAcp, int NumberOfOffsets, double Rbw)
        {
            string selectorString;
            string test;
            int NumberOfCarriers = 1;
            double rbw = 0;

            test = "ACP" + Iteration.ToString();
            specAcp.Insert(Iteration, _instrSession.GetSpecAnSignalConfiguration(test));
            specAcp[Iteration].ConfigureFrequency("", FreqSG * 1e6);
            specAcp[Iteration].ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
            specAcp[Iteration].ConfigureReferenceLevel("", Reflevel);
            specAcp[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation      


            if (TestAcp)
            {
                specAcp[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Acp, false);

                specAcp[Iteration].Acp.Configuration.ConfigurePowerUnits("", RFmxSpecAnMXAcpPowerUnits.dBm);
                specAcp[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.FlatTop, 1);

                if (WaveformName == "TS01")
                {
                    //specAnAcpSignal[Iteration].Acp.Configuration.ConfigureFft("", RFmxSpecAnMXAcpFftWindow.Gaussian, 1);
                    specAcp[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.True, Rbw, RFmxSpecAnMXAcpRbwFilterType.Gaussian);
                    specAcp[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.False, 0.530e-3);
                    specAcp[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.True, 10, RFmxSpecAnMXAcpAveragingType.Rms);
                    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);
                }
                else
                {
                    specAcp[Iteration].Acp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXAcpRbwAutoBandwidth.False, Rbw, RFmxSpecAnMXAcpRbwFilterType.FftBased);
                    specAcp[Iteration].Acp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXAcpSweepTimeAuto.True, 0.005);
                    specAcp[Iteration].Acp.Configuration.ConfigureAveraging("", RFmxSpecAnMXAcpAveragingEnabled.True, 3, RFmxSpecAnMXAcpAveragingType.Rms);
                    specAcp[Iteration].Acp.Configuration.ConfigureNumberOfCarriers("", NumberOfCarriers);
                }

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
            }
            else
            {

                specAcp[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Chp, false);
                specAcp[Iteration].Chp.Configuration.ConfigureIntegrationBandwidth("", RefChBW);
                specAcp[Iteration].Chp.Configuration.ConfigureFft("", RFmxSpecAnMXChpFftWindow.FlatTop, 1);
                specAcp[Iteration].Chp.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXChpRbwAutoBandwidth.False, (Rbw == -1 ? 100e3 : Rbw), RFmxSpecAnMXChpRbwFilterType.FftBased);
                specAcp[Iteration].Chp.Configuration.ConfigureSweepTime("", RFmxSpecAnMXChpSweepTimeAuto.True, 0.001);
                specAcp[Iteration].Chp.Configuration.ConfigureAveraging("", RFmxSpecAnMXChpAveragingEnabled.False, 1, RFmxSpecAnMXChpAveragingType.Rms);
            }

        }
        public void CommitSpec(int Iteration)
        {

        }
        public void InitiateSpec(int Iteration)
        {

        }
        public void RetrieveResults(int Iteration)
        {

        }        
    }
    public class RfmxChp
    {
        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec()
        {

        }
        public void CommitSpec()
        {

        }
        public void InitiateSpec()
        {

        }
        public void RetrieveResults()
        {

        }
    }
    public class RfmxIQ
    {
        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec()
        {

        }
        public void CommitSpec()
        {

        }
        public void InitiateSpec()
        {

        }
        public void RetrieveResults()
        {

        }
    }
    public class RfmxIIP3
    {
        List<RFmxSpecAnMX> specIIP3;
        RFmxInstrMX _instrSession;
        public RfmxIIP3()
        {
            specIIP3 = new List<RFmxSpecAnMX>();
            _instrSession = NIRFMX.instrSession;
        }
        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec(int Iteration, double Freq, double Reflevel)
        {
            string test;

            test = "IIP3" + Iteration.ToString();
            _instrSession.SetCleanerSpectrum("", RFmxInstrMXCleanerSpectrum.Disabled);
            _instrSession.SetDownconverterFrequencyOffset("", -40 * 1e6);
            while (specIIP3.Count <= Iteration)
                specIIP3.Add(null);
            specIIP3.Insert(Iteration, _instrSession.GetSpecAnSignalConfiguration(test));
            specIIP3[Iteration].ConfigureFrequency("", (Freq * 1e6));
            specIIP3[Iteration].ConfigureExternalAttenuation("", 0);// externalAttenuation
            specIIP3[Iteration].ConfigureReferenceLevel("", Reflevel);
            specIIP3[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.IM, true);
            specIIP3[Iteration].IM.Configuration.ConfigureAveraging("", RFmxSpecAnMXIMAveragingEnabled.False, 1, RFmxSpecAnMXIMAveragingType.Rms);
            specIIP3[Iteration].IM.Configuration.ConfigureRbwFilter("", RFmxSpecAnMXIMRbwFilterAutoBandwidth.False, 10e3, RFmxSpecAnMXIMRbwFilterType.FftBased);

            specIIP3[Iteration].IM.Configuration.ConfigureSweepTime("", RFmxSpecAnMXIMSweepTimeAuto.True, 0.005);
            specIIP3[Iteration].IM.Configuration.ConfigureFft("", RFmxSpecAnMXIMFftWindow.FlatTop, 1);
            specIIP3[Iteration].IM.Configuration.ConfigureFrequencyDefinition("", RFmxSpecAnMXIMFrequencyDefinition.Absolute);

            specIIP3[Iteration].IM.Configuration.ConfigureMeasurementMethod("", RFmxSpecAnMXIMMeasurementMethod.Normal);
            specIIP3[Iteration].IM.Configuration.ConfigureFundamentalTones("", ((Freq - 0.5) * 1e6), ((Freq + 0.5) * 1e6));
            specIIP3[Iteration].IM.Configuration.ConfigureAutoIntermodsSetup("", RFmxSpecAnMXIMAutoIntermodsSetupEnabled.True, 3);

            specIIP3[Iteration].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.FrequencyAndReferenceLevel); ////////NI 160713 
        }
        public void CommitSpec(int Iteration)
        {
            _instrSession.SetDownconverterFrequencyOffset("", -40 * 1e6);
            specIIP3[Iteration].Commit("");
        }
        public void InitiateSpec(int Iteration)
        {
            specIIP3[Iteration].Initiate("", "");
        }
        public void RetrieveResults(int Iteration, out double lowerTonePower, out double upperTonePower, ref double[] lowerIntermodPower, ref double[] upperIntermodPower, ref int[] intermodOrder)
        {
            _instrSession.WaitForAcquisitionComplete(1);
            specIIP3[Iteration].IM.Results.FetchFundamentalMeasurement("", 10, out lowerTonePower, out upperTonePower);
            specIIP3[Iteration].IM.Results.FetchIntermodMeasurementArray("", 10, ref intermodOrder, ref lowerIntermodPower, ref upperIntermodPower);
        }
    }
    public class RfmxHar2nd
    {
        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec()
        {

        }
        public void CommitSpec()
        {

        }
        public void InitiateSpec()
        {

        }
        public void RetrieveResults()
        {

        }
    }
    public class RfmxHar3rd
    {
        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec()
        {

        }
        public void CommitSpec()
        {

        }
        public void InitiateSpec()
        {

        }
        public void RetrieveResults()
        {

        }
    }
    public class RfmxTxleakage
    {
        public bool Initialize(bool FinalScript)
        {
            return false;
        }
        public void ConfigureSpec()
        {

        }
        public void CommitSpec()
        {

        }
        public void InitiateSpec()
        {

        }
        public void RetrieveResults()
        {

        }
    }
    
    public enum eRfmx
    {
        eRfmxAcp,
        eRfmxChp,
        eRfmxIQ,
        eRfmxIIP3,
        eRfmxHar2nd,
        eRfmxHar3rd,
        eRfmxTxleakage
    }

}
