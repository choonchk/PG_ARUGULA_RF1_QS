using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.ModularInstruments.Wcdmasa;
using NationalInstruments.ModularInstruments.Ltesa;
using NationalInstruments.RFToolkits.Interop;

namespace ClothoLibStandard
{
    public class PxiSa
    {
        string resourceName;
        double triggerLevel;
        double maxInPower;
        int arfcn;
        int averages;
        int band;
        int uut;
        int tscDetectionEnabled;
        int tscIn;
        string referenceSource;
        double frequency;

        bool rfsaResourceTextChanged;

        niGSMSA gsmsa;
        niGSMSA gsmsaTxp;
        niEDGESA edgesaPvt;
        niEDGESA edgesaTxp;
        niEDGESA edgesaEvm;
        niEDGESA edgesaAcp;
        NIWcdmasa wcdmaChp;
        NIWcdmasa wcdmaAcp;
        niLTESA lteChp;
        niLTESA lteChpNs07;
        niLTESA lteAcp;
        niLTESA tdscdmaChp;
        niLTESA tdscdmaAcp;
        NIWcdmasa evDoChp;
        NIWcdmasa evDoAcp;
        
        long numberOfSamplesAcp;
        double modCarrier;
        double modFar;
        double modNear;

        niRFSA rfsaSession;

        private void AnalyseAcp(int average)
        {

            edgesaAcp.SetOrfsNumberOfAverages(null, average);
            edgesaAcp.RFSAConfigureHardware(rfsaSession.Handle);
            // rfsaSession.GetInt64(null, niRFSAProperties.NumberOfSamples, out numberOfSamplesAcp);

            niComplexNumber[] waveform = new niComplexNumber[numberOfSamplesAcp];
            niRFSA_wfmInfo wfmInfo;
            int done = 0;

            int numberOfRecords;
            edgesaAcp.GetRecommendedHardwareSettingsNumberOfRecords(null, out numberOfRecords);
            double postTriggerDelay;
            edgesaAcp.GetRecommendedHardwareSettingsPostTriggerDelay(null, out postTriggerDelay);

            rfsaSession.Initiate();

            for (int i = 0; i < numberOfRecords; i++)
            {
                rfsaSession.FetchIQSingleRecordComplexF64(null, i, numberOfSamplesAcp, 2, waveform, out wfmInfo);
                wfmInfo.relativeInitialX = postTriggerDelay;
                edgesaAcp.AnalyzeIQComplexF64(wfmInfo.relativeInitialX, wfmInfo.xIncrement, waveform, (int)numberOfSamplesAcp, Convert.ToInt32(i == 0), out done);
            }
        }

        public int Close()
        {
            try
            {
                if (rfsaSession != null)
                    rfsaSession.Close();
                if (gsmsa != null)
                    gsmsa.Close();
                if (edgesaAcp != null)
                    edgesaAcp.Close();
                if (edgesaEvm != null)
                    edgesaEvm.Close();
                if (edgesaPvt != null)
                    edgesaPvt.Close();

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public int Commit()
        {
            try
            {
                rfsaSession.Commit();

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Commit");
                return -1;
            }
        }

        public int ConfigureAverage(string measMode, string Modulation, int measAverage)
        {
            if (Modulation == "WCDMA")
            {
                if (measMode.ToUpper() == "POUT")
                {
                    wcdmaChp.SetChpNumberOfAverages(null, measAverage);
                    
                }
                else if (measMode.ToUpper() == "ACLR")
                {
                    wcdmaAcp.SetAcpNumberOfAverages(null, measAverage);
                    
                }
            }
            else if (Modulation == "LTE")
            {
                if (measMode.ToUpper() == "POUT")
                {
                    lteChp.SetChpNumberOfAverages(string.Empty, measAverage);
                    
                }
                else if (measMode.ToUpper() == "ACLR")
                {
                    lteAcp.SetAcpNumberOfAverages(string.Empty, measAverage);
                    
                }
            }

            return 0;
        }

        public int Freq(float SetFreq)
        {
            // rfsaSession.Close();


            // double result = 0;
            // wcdmasaSessionPout.SetCarrierFrequency(null, SetFreq * 1E6);            
            rfsaSession.ConfigureIQCarrierFrequency(null, SetFreq );
            

            // result = rfsaSession.GetDouble(niRFSAProperties.FrequencySettling, "");
            // result = rfsaSession.GetDouble(niRFSAProperties.SpectrumCenterFrequency, "");

            // rfsaSession.SetDouble(niRFSAProperties.SpectrumCenterFrequency, SetFreq * 1E6);
            // result = rfsaSession.GetDouble(niRFSAProperties.SpectrumCenterFrequency, "");




            return 0;
        }

        public int InitializeCw()
        {
            try
            {
                if (rfsaSession != null)
                    rfsaSession.Abort();
                    //rfsaSession.Close();

                // Initialise
                //rfsaSession = new niRFSA("VSA", true, false);                
                
                // Reference Clock Source
                rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

                // Trigger
                rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.None);

                // Acquisition mode to Spectrum
                rfsaSession.ConfigureAcquisitionType(niRFSAConstants.Spectrum);                
                
                rfsaSession.ConfigureSpectrumFrequencyCenterSpan(null, 1000000000, 1000000);
                
                // Span                
                rfsaSession.SetDouble(niRFSAProperties.SpectrumSpan, 1000000);
                // RBW
                rfsaSession.SetDouble(niRFSAProperties.ResolutionBandwidth, 10000);
                // Reference level
                rfsaSession.ConfigureReferenceLevel(null, 30);
                
                // turn off the mechanical attenuator
                rfsaSession.SetInt32(niRFSAProperties.MechanicalAttenuatorEnabled, niRFSAConstants.Disabled);

                int triggerType = rfsaSession.GetInt32(niRFSAProperties.RefTriggerType);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "InitializeGsm Error");
                return -1;
            }
        }

        public int InitializeCw_BAK()
        {
            try
            {
                if (rfsaSession != null)
                    rfsaSession.Close();

                // Initialise
                rfsaSession = new niRFSA("VSA", true, false);

                // Reference Clock Source
                // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
                // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
                // Reference clock rate
                rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

                // Trigger
                rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.None);
                // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
                // rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);
                // rfsaSession.SetString(niRFSAProperties.DigitalEdgeRefTriggerSource, niRFSAConstants.Pfi1Str);

                // Disable Reference Trigger
                //rfsaSession.DisableRefTrigger();
                //rfsaSession.DisableStartTrigger();


                // Acquisition mode to Spectrum
                rfsaSession.ConfigureAcquisitionType(niRFSAConstants.Spectrum);
                // Center Frequency, 1 GHz
                // rfsaSession.ConfigureIQCarrierFrequency(null, 1000000000);                

                rfsaSession.ConfigureSpectrumFrequencyCenterSpan(null, 1000000000, 1000000);

                // Span                
                rfsaSession.SetDouble(niRFSAProperties.SpectrumSpan, 1000000);
                // RBW
                rfsaSession.SetDouble(niRFSAProperties.ResolutionBandwidth, 10000);
                // Reference level
                rfsaSession.ConfigureReferenceLevel(null, 30);

                // turn off the mechanical attenuator
                rfsaSession.SetInt32(niRFSAProperties.MechanicalAttenuatorEnabled, niRFSAConstants.Disabled);
                // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);

                int triggerType = rfsaSession.GetInt32(niRFSAProperties.RefTriggerType);


                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "InitializeGsm Error");
                return -1;
            }
        }

        public int InitializeGsmEdge()
        {
            try
            {
                if (rfsaSession != null)
                    rfsaSession.Close();
                if (gsmsa != null)
                    gsmsa.Close();
                if (gsmsaTxp != null)
                    gsmsaTxp.Close();
                if (edgesaAcp != null)
                    edgesaAcp.Close();
                if (edgesaPvt != null)
                    edgesaPvt.Close();
                if (edgesaTxp != null)
                    edgesaTxp.Close();
                if (edgesaEvm != null)
                    edgesaEvm.Close();

                InitialiseMembersGsm();
                rfsaSession = new niRFSA(resourceName, true, false);                
                gsmsa = new niGSMSA(niGSMSAConstants.ToolkitCompatibilityVersion100);
                gsmsaTxp = new niGSMSA(niGSMSAConstants.ToolkitCompatibilityVersion100);
                edgesaAcp = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);
                edgesaPvt = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);
                edgesaEvm = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);

                InitialiseMembersGsm();
                ConfigureGsm();
                ConfigureGsmTxp();

                InitialiseMembersEdgeAcp();
                ConfigureEdgeAcp();

                InitialiseMembersEdgePvt();
                ConfigureEdgePvt();

                InitialiseMembersEdgeEvm();
                ConfigureEdgeEvm();

                // turn off the mechanical attenuator
                rfsaSession.SetInt32(niRFSAProperties.MechanicalAttenuatorEnabled, niRFSAConstants.Disabled);

                // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
                
                

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "InitializeGsm Error");
                return -1;
            }
        }

        public int InitializeGsmEdgeWcdmaLte()
        {
            try
            {
                if (rfsaSession != null)
                    rfsaSession.Close();                
                if (wcdmaChp != null)
                    wcdmaChp.Close();
                if (wcdmaAcp != null)
                    wcdmaAcp.Close();
                if (lteChp != null)
                    lteChp.Close();
                if (lteAcp != null)
                    lteAcp.Close();
                if (evDoChp != null)
                    evDoChp.Close();
                if (tdscdmaAcp != null)
                    tdscdmaAcp.Close();
                if (tdscdmaChp != null)
                    tdscdmaChp.Close();
                
                InitialiseMembersGsm();
                rfsaSession = new niRFSA(resourceName, true, false);

                //KCC - For Portal LTE
                rfsaSession.ConfigureRefClock("OnboardClock", 10e+6);
                rfsaSession.ConfigureReferenceLevel(null, 0);
                rfsaSession.DisableRefTrigger();

                //Jongsoo
                if (false)
                {
                    //gsmsa = new niGSMSA(niGSMSAConstants.ToolkitCompatibilityVersion100);
                    //gsmsaTxp = new niGSMSA(niGSMSAConstants.ToolkitCompatibilityVersion100);
                    //edgesaAcp = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);
                    //edgesaPvt = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);
                    //edgesaEvm = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);

                    //InitialiseMembersGsm();
                    //ConfigureGsm();
                    //ConfigureGsmTxp();

                    //InitialiseMembersEdgeAcp();
                    //ConfigureEdgeAcp();

                    //InitialiseMembersEdgePvt();
                    //ConfigureEdgePvt();

                    //InitialiseMembersEdgeEvm();
                    //ConfigureEdgeEvm();


                    //Configure hardware properties
                    rfsaSession.ConfigureReferenceLevel(null, maxInPower);
                    // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
                    rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);

                    // rfsaSession.ConfigureDigitalEdgeRefTrigger(niRFSAConstants.PxiTrig0Str, niRFSAConstants.RisingEdge, 0);
                    // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
                    // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);

                    // rfsaSession.ConfigureDigitalEdgeRefTrigger(niRFSAConstants.Pfi1Str, niRFSAConstants.RisingEdge, 0);
                    rfsaSession.ConfigureDigitalEdgeRefTrigger(niRFSAConstants.PxiTrig0Str, niRFSAConstants.RisingEdge, 0);
                    string result = rfsaSession.GetString(niRFSAProperties.DigitalEdgeRefTriggerSource);

                    rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);
                }

                ConfigureWcdmaChp();
                ConfigureWcdmaAcp();

                ConfigureLteChp();
                ConfigureLteAcp();
                ConfigureLteChpNs07();

                ConfigureEvDoChp();
                //ConfigureEvDoAcp();

                ConfigureTdscdmaChp();
                ConfigureTdscdmaAcp();

                //Jongsoo
                if (false)
                {
                    // turn off the mechanical attenuator
                    rfsaSession.SetInt32(niRFSAProperties.MechanicalAttenuatorEnabled, niRFSAConstants.Disabled);

                    // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
                    rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
                }

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "InitializeGsm Error");
                return -1;
            }
        }

        public int InitializeSpectrumHarmonic(double freqCenter, double freqSpan, double rbw)
        {
            try
            {                
                // Trigger
                rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.None);
                // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
                // rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);
                // rfsaSession.SetString(niRFSAProperties.DigitalEdgeRefTriggerSource, niRFSAConstants.Pfi1Str);

                // Disable Reference Trigger
                //rfsaSession.DisableRefTrigger();
                //rfsaSession.DisableStartTrigger();


                // Acquisition mode to Spectrum
                rfsaSession.ConfigureAcquisitionType(niRFSAConstants.Spectrum);
                // Center Frequency, 1 GHz
                // rfsaSession.ConfigureIQCarrierFrequency(null, 1000000000);                
                rfsaSession.ConfigureSpectrumFrequencyCenterSpan(null, freqCenter * 1000000, freqSpan);
                // Span                
                // rfsaSession.SetDouble(niRFSAProperties.SpectrumSpan, 1000000);
                // RBW
                rfsaSession.SetDouble(niRFSAProperties.ResolutionBandwidth, rbw);
                // Reference level
                // rfsaSession.ConfigureReferenceLevel(null, 30);

                // turn off the mechanical attenuator
                //rfsaSession.SetInt32(niRFSAProperties.MechanicalAttenuatorEnabled, niRFSAConstants.Disabled);
                //rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);

                //int triggerType = rfsaSession.GetInt32(niRFSAProperties.RefTriggerType);


                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "InitializeGsm Error");
                return -1;
            }
        }

        public int InitializeWcdmaChp()
        {
            try
            {                
                ConfigureWcdmaChp();

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "InitializeWcdmaChp Error");
                return -1;
            }
        }

        public int IqTriggerLevel(double triggerLevel)
        {
            try
            {
                rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public void MeasAcpEdge(int average, ref float Acpr1L, ref float Acpr1U, ref float Acpr2L, ref float Acpr2U)
        {
            try
            {
                AnalyseAcp(average);

                //load data to modulation powers data grid
                int traceSize, length;
                edgesaAcp.ORFSGetModulationOffsetFrequenciesTrace(null, 0, out traceSize);
                length = traceSize;
                double[] modOffset = new double[length];
                double[] modRelativePowers = new double[length];
                // edgesaAcp.ORFSGetModulationOffsetFrequenciesTrace(modOffset, length, out traceSize);
                edgesaAcp.ORFSGetModulationRelativePowersTrace(modRelativePowers, length, out traceSize);

                Acpr1L = (float)modRelativePowers[1];
                Acpr1U = (float)modRelativePowers[2];
                Acpr2L = (float)modRelativePowers[3];
                Acpr2U = (float)modRelativePowers[4];                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasAcpEdge");
            }
        }

        public int MeasAcpEvdo(long samplesPerRecord, ref float ACLR1L, ref float ACLR1U, ref float ACLR2L, ref float ACLR2U)
        {
            try
            {                
                int numRecords, i, done = 0;
                double postTriggerDelay;

                niComplexNumber[] waveform;
                niRFSA_wfmInfo wfmInfo;

                evDoAcp.GetRecommendedHardwareSettingsNumberOfRecords(null, out numRecords);
                evDoAcp.GetRecommendedHardwareSettingsPosttriggerDelay(null, out postTriggerDelay);

                rfsaSession.Initiate();
                waveform = new niComplexNumber[Convert.ToInt32(samplesPerRecord)];
                // negAbPower=new int[];

                //Analyze
                for (i = 0; i < numRecords; i++)
                {
                    rfsaSession.FetchIQSingleRecordComplexF64("", i, samplesPerRecord, 2, waveform, out wfmInfo);
                    wfmInfo.relativeInitialX = postTriggerDelay;
                    evDoAcp.AnalyzeIQComplexF64(wfmInfo.relativeInitialX, wfmInfo.xIncrement, waveform,
                        Convert.ToInt32(samplesPerRecord), (i == 0), out done);
                }

                rfsaSession.Abort();

                //Get ACP Results
                //double refChPower;
                //evDoAcp.GetResultAcpReferenceChannelPower(null, out refChPower);

                double[] negRePower = new double[2];
                double[] posRePower = new double[2];
                int actualNumberOfElements;

                evDoAcp.GetResultAcpNegativeRelativePowers("", negRePower, out actualNumberOfElements);
                evDoAcp.GetResultAcpPositiveRelativePowers("", posRePower, out actualNumberOfElements);

                ACLR1L = Convert.ToSingle(negRePower[0]);
                ACLR1U = Convert.ToSingle(posRePower[0]);
                ACLR2L = Convert.ToSingle(negRePower[1]);
                ACLR2U = Convert.ToSingle(posRePower[1]);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasAcpEvdo");
                return -1;
            }
        }

        public int MeasAcpLte(long samplesPerRecord, ref float ACLR1L, ref float ACLR1U, ref float ACLR2L, ref float ACLR2U, ref float ACLR3L, ref float ACLR3U)
        {
            try
            {
                int acquisitionType;
                int numberOfRecords;
                int actualNumberOfSamples;
                double f0 = 0;
                double df = 0;
                double postTriggerDelay;
                double[] powerSpectrum = null;
                niComplexNumber[] waveform = null;
                int averagingDone;
                
                lteAcp.GetRecommendedIqAcquisitionNumberOfRecords(string.Empty, out numberOfRecords);
                lteAcp.GetRecommendedIqAcquisitionPosttriggerDelay(string.Empty, out postTriggerDelay);
                lteAcp.GetRecommendedAcquisitionType(string.Empty, out acquisitionType);

                if (acquisitionType == niLTESAConstants.RecommendedAcquisitionTypeIq)
                {
                    waveform = new niComplexNumber[samplesPerRecord];
                    rfsaSession.Initiate();
                    niRFSA_wfmInfo waveformInfo;
                    for (int i = 0; i < numberOfRecords; i++)
                    {
                        rfsaSession.FetchIQSingleRecordComplexF64(string.Empty, i, samplesPerRecord, 2, waveform, out waveformInfo);
                        waveformInfo.relativeInitialX = postTriggerDelay;
                        lteAcp.AnalyzeIQComplexF64(waveformInfo.relativeInitialX, waveformInfo.xIncrement, waveform, Convert.ToInt32(samplesPerRecord), (i == 0) ? 1 : 0, out averagingDone);
                    }
                }
                else
                {
                    /*Get upper bound on Size of powerSpectrum array*/
                    int numberOfSamplesForSpectrum;
                    lteAcp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 0, out f0, out df, null, 0, out numberOfSamplesForSpectrum);
                    /*Allocate memory for powerSpectrum*/
                    powerSpectrum = new double[numberOfSamplesForSpectrum];
                    lteAcp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 10, out f0, out df, powerSpectrum, numberOfSamplesForSpectrum, out actualNumberOfSamples);
                    /*Analyze*/
                    lteAcp.AnalyzePowerSpectrum(f0, df, powerSpectrum, powerSpectrum.Length);
                }

                rfsaSession.Abort();
                // rfsaSession.Close();

                double refPower;
                int actualNumNegElements;
                int actualNumPosElements;
                double[] negRefPower = new double[3];
                double[] posRefPower = new double[3];

                /*Get Results*/
                // lteAcp.GetResultAcpReferenceChannelPower(string.Empty, out refPower);
                lteAcp.GetResultAcpNegativeRelativePowers(string.Empty, negRefPower, negRefPower.Length, out actualNumNegElements);
                lteAcp.GetResultAcpPositiveRelativePowers(string.Empty, posRefPower, posRefPower.Length, out actualNumPosElements);

                /*DisplayRectangle Results*/
                // refPwrTextBox.Text = refPower.ToString();
                ACLR1L = Convert.ToSingle(negRefPower[0]);
                ACLR2L = Convert.ToSingle(negRefPower[1]);
                ACLR3L = Convert.ToSingle(negRefPower[2]);
                ACLR1U = Convert.ToSingle(posRefPower[0]);
                ACLR2U = Convert.ToSingle(posRefPower[1]);
                ACLR3U = Convert.ToSingle(posRefPower[2]);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasAcpLte");
                return -1;
            }
        }

        public int MeasAcpTdscdma(long samplesPerRecord, ref float ACLR1L, ref float ACLR1U, ref float ACLR2L, ref float ACLR2U)
        {
            try
            {
                int acquisitionType;
                int numberOfRecords;
                int actualNumberOfSamples;
                double f0 = 0;
                double df = 0;
                double postTriggerDelay;
                double[] powerSpectrum = null;
                niComplexNumber[] waveform = null;
                int averagingDone;

                tdscdmaAcp.GetRecommendedIqAcquisitionNumberOfRecords(string.Empty, out numberOfRecords);
                tdscdmaAcp.GetRecommendedIqAcquisitionPosttriggerDelay(string.Empty, out postTriggerDelay);
                tdscdmaAcp.GetRecommendedAcquisitionType(string.Empty, out acquisitionType);

                if (acquisitionType == niLTESAConstants.RecommendedAcquisitionTypeIq)
                {
                    waveform = new niComplexNumber[samplesPerRecord];
                    rfsaSession.Initiate();
                    niRFSA_wfmInfo waveformInfo;
                    for (int i = 0; i < numberOfRecords; i++)
                    {
                        rfsaSession.FetchIQSingleRecordComplexF64(string.Empty, i, samplesPerRecord, 2, waveform, out waveformInfo);
                        waveformInfo.relativeInitialX = postTriggerDelay;
                        tdscdmaAcp.AnalyzeIQComplexF64(waveformInfo.relativeInitialX, waveformInfo.xIncrement, waveform, Convert.ToInt32(samplesPerRecord), (i == 0) ? 1 : 0, out averagingDone);
                    }
                }
                else
                {
                    /*Get upper bound on Size of powerSpectrum array*/
                    int numberOfSamplesForSpectrum;
                    tdscdmaAcp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 0, out f0, out df, null, 0, out numberOfSamplesForSpectrum);
                    /*Allocate memory for powerSpectrum*/
                    powerSpectrum = new double[numberOfSamplesForSpectrum];
                    tdscdmaAcp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 10, out f0, out df, powerSpectrum, numberOfSamplesForSpectrum, out actualNumberOfSamples);
                    /*Analyze*/
                    tdscdmaAcp.AnalyzePowerSpectrum(f0, df, powerSpectrum, powerSpectrum.Length);
                }

                rfsaSession.Abort();
                // rfsaSession.Close();

                double refPower;
                int actualNumNegElements;
                int actualNumPosElements;
                double[] negRefPower = new double[2];
                double[] posRefPower = new double[2];

                /*Get Results*/
                // tdscdmaAcp.GetResultAcpReferenceChannelPower(string.Empty, out refPower);
                tdscdmaAcp.GetResultAcpNegativeRelativePowers(string.Empty, negRefPower, negRefPower.Length, out actualNumNegElements);
                tdscdmaAcp.GetResultAcpPositiveRelativePowers(string.Empty, posRefPower, posRefPower.Length, out actualNumPosElements);

                /*DisplayRectangle Results*/
                // refPwrTextBox.Text = refPower.ToString();
                ACLR1L = Convert.ToSingle(negRefPower[0]);
                ACLR2L = Convert.ToSingle(negRefPower[1]);
                ACLR1U = Convert.ToSingle(posRefPower[0]);
                ACLR2U = Convert.ToSingle(posRefPower[1]);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasAcpTdscdma");
                return -1;
            }
        }

        public int MeasAcpWcdma(long samplesPerRecord, ref float ACLR1L, ref float ACLR1U, ref float ACLR2L, ref float ACLR2U)
        {
            try
            {
                int numRecords, i, done = 0;
                double postTriggerDelay;
                                
                niComplexNumber[] waveform;
                niRFSA_wfmInfo wfmInfo;

                wcdmaAcp.GetRecommendedHardwareSettingsNumberOfRecords(null, out numRecords);
                wcdmaAcp.GetRecommendedHardwareSettingsPosttriggerDelay(null, out postTriggerDelay);
                
                rfsaSession.Initiate();
                waveform = new niComplexNumber[Convert.ToInt32(samplesPerRecord)];
                // negAbPower=new int[];

                //Analyze
                for (i = 0; i < numRecords; i++)
                {
                    rfsaSession.FetchIQSingleRecordComplexF64("", i, samplesPerRecord, 2, waveform, out wfmInfo);
                    wfmInfo.relativeInitialX = postTriggerDelay;
                    wcdmaAcp.AnalyzeIQComplexF64(wfmInfo.relativeInitialX, wfmInfo.xIncrement, waveform,
                        Convert.ToInt32(samplesPerRecord), (i == 0), out done);
                }

                rfsaSession.Abort();
                
                //Get ACP Results
                //double refChPower;
                //wcdmaAcp.GetResultAcpReferenceChannelPower(null, out refChPower);
                
                double[] negRePower = new double[2];
                double[] posRePower = new double[2];
                int actualNumberOfElements;

                wcdmaAcp.GetResultAcpNegativeRelativePowers("", negRePower, out actualNumberOfElements);
                wcdmaAcp.GetResultAcpPositiveRelativePowers("", posRePower, out actualNumberOfElements);
                                                
                ACLR1L = Convert.ToSingle(negRePower[0]);
                ACLR1U = Convert.ToSingle(posRePower[0]);
                ACLR2L = Convert.ToSingle(negRePower[1]);
                ACLR2U = Convert.ToSingle(posRePower[1]);

                waveform = new niComplexNumber[1];

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasAcpWcdma");
                return -1;
            }
        }

        public int MeasAcpWcdmaEvdo(long samplesPerRecord, ref float ACLR1L, ref float ACLR1U, ref float ACLR2L, ref float ACLR2U)
        {
            try
            {
                int numRecords, i, done = 0;
                double postTriggerDelay;

                niComplexNumber[] waveform;
                niRFSA_wfmInfo wfmInfo;

                wcdmaAcp.GetRecommendedHardwareSettingsNumberOfRecords(null, out numRecords);
                wcdmaAcp.GetRecommendedHardwareSettingsPosttriggerDelay(null, out postTriggerDelay);

                rfsaSession.Initiate();
                waveform = new niComplexNumber[Convert.ToInt32(samplesPerRecord)];
                // negAbPower=new int[];

                //Analyze
                for (i = 0; i < numRecords; i++)
                {
                    rfsaSession.FetchIQSingleRecordComplexF64("", i, samplesPerRecord, 2, waveform, out wfmInfo);
                    wfmInfo.relativeInitialX = postTriggerDelay;
                    wcdmaAcp.AnalyzeIQComplexF64(wfmInfo.relativeInitialX, wfmInfo.xIncrement, waveform,
                        Convert.ToInt32(samplesPerRecord), (i == 0), out done);
                }

                rfsaSession.Abort();

                //Get ACP Results
                //double refChPower;
                //wcdmaAcp.GetResultAcpReferenceChannelPower(null, out refChPower);

                double[] negRePower = new double[2];
                double[] posRePower = new double[2];
                int actualNumberOfElements;

                wcdmaAcp.GetResultAcpNegativeRelativePowers("", negRePower, out actualNumberOfElements);
                wcdmaAcp.GetResultAcpPositiveRelativePowers("", posRePower, out actualNumberOfElements);

                ACLR1L = Convert.ToSingle(negRePower[0]);
                ACLR1U = Convert.ToSingle(posRePower[0]);
                ACLR2L = Convert.ToSingle(negRePower[1]);
                ACLR2U = Convert.ToSingle(posRePower[1]);

                waveform = new niComplexNumber[1];

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasAcpWcdmaEvdo");
                return -1;
            }
        }

        public int MeasPoutLteChp(long samplesPerRecord, ref float Pout)
        {
            try
            {
                int acquisitionType;
                int numberOfRecords;
                int actualNumberOfSamples;
                double f0 = 0;
                double df = 0;
                double postTriggerDelay;
                double[] powerSpectrum = null;
                niComplexNumber[] waveform = null;
                int averagingDone;
                
                lteChp.GetRecommendedIqAcquisitionNumberOfRecords(string.Empty, out numberOfRecords);
                lteChp.GetRecommendedIqAcquisitionPosttriggerDelay(string.Empty, out postTriggerDelay);
                lteChp.GetRecommendedAcquisitionType(string.Empty, out acquisitionType);

                /*Analyze*/
                if (acquisitionType == niLTESAConstants.RecommendedAcquisitionTypeIq)
                {
                    // string testResult = rfsaSession.GetString(niRFSAProperties.DigitalEdgeRefTriggerSource);

                    waveform = new niComplexNumber[samplesPerRecord];
                    rfsaSession.Initiate();
                    niRFSA_wfmInfo waveformInfo;
                    for (int i = 0; i < numberOfRecords; i++)
                    {
                        rfsaSession.FetchIQSingleRecordComplexF64(string.Empty, i, samplesPerRecord, 2, waveform, out waveformInfo);
                        waveformInfo.relativeInitialX = postTriggerDelay;
                        lteChp.AnalyzeIQComplexF64(waveformInfo.relativeInitialX, waveformInfo.xIncrement, waveform, Convert.ToInt32(samplesPerRecord), (i == 0) ? 1 : 0, out averagingDone);
                    }
                }
                else
                {
                    /*Get Upper bound on size of powerSpectrum and allocate memory*/
                    int numberOfSamplesForSpectrum;
                    lteChp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 0, out f0, out df, null, 0, out numberOfSamplesForSpectrum);
                    powerSpectrum = new double[numberOfSamplesForSpectrum];
                    lteChp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 10, out f0, out df, powerSpectrum, numberOfSamplesForSpectrum, out actualNumberOfSamples);
                    /*Analyze*/
                    lteChp.AnalyzePowerSpectrum(f0, df, powerSpectrum, powerSpectrum.Length);
                }

                rfsaSession.Abort();

                double chPower;
                // double chPSD;

                /*Get Results*/
                lteChp.GetResultChpChannelPower(string.Empty, out chPower);
                // lteChp.GetResultChpChannelPowerSpectralDensity(string.Empty, out chPSD);

                /*Display Results*/
                Pout = Convert.ToSingle( chPower);
                // chPsdTextBox.Text = chPSD.ToString();

                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasPoutLteChp");
                return -1;
            }
        }

        public int MeasPoutLteChpNs07(long samplesPerRecord, ref float Pout)
        {
            try
            {
                int acquisitionType;
                int numberOfRecords;
                int actualNumberOfSamples;
                double f0 = 0;
                double df = 0;
                double postTriggerDelay;
                double[] powerSpectrum = null;
                niComplexNumber[] waveform = null;
                int averagingDone;

                lteChpNs07.GetRecommendedIqAcquisitionNumberOfRecords(string.Empty, out numberOfRecords);
                lteChpNs07.GetRecommendedIqAcquisitionPosttriggerDelay(string.Empty, out postTriggerDelay);
                lteChpNs07.GetRecommendedAcquisitionType(string.Empty, out acquisitionType);

                /*Analyze*/
                if (acquisitionType == niLTESAConstants.RecommendedAcquisitionTypeIq)
                {
                    // string testResult = rfsaSession.GetString(niRFSAProperties.DigitalEdgeRefTriggerSource);

                    waveform = new niComplexNumber[samplesPerRecord];
                    rfsaSession.Initiate();
                    niRFSA_wfmInfo waveformInfo;
                    for (int i = 0; i < numberOfRecords; i++)
                    {
                        rfsaSession.FetchIQSingleRecordComplexF64(string.Empty, i, samplesPerRecord, 2, waveform, out waveformInfo);
                        waveformInfo.relativeInitialX = postTriggerDelay;
                        lteChpNs07.AnalyzeIQComplexF64(waveformInfo.relativeInitialX, waveformInfo.xIncrement, waveform, Convert.ToInt32(samplesPerRecord), (i == 0) ? 1 : 0, out averagingDone);
                    }
                }
                else
                {
                    /*Get Upper bound on size of powerSpectrum and allocate memory*/
                    int numberOfSamplesForSpectrum;
                    lteChpNs07.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 0, out f0, out df, null, 0, out numberOfSamplesForSpectrum);
                    powerSpectrum = new double[numberOfSamplesForSpectrum];
                    lteChpNs07.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 10, out f0, out df, powerSpectrum, numberOfSamplesForSpectrum, out actualNumberOfSamples);
                    /*Analyze*/
                    lteChpNs07.AnalyzePowerSpectrum(f0, df, powerSpectrum, powerSpectrum.Length);
                }

                rfsaSession.Abort();

                double chPower;
                // double chPSD;

                /*Get Results*/
                lteChpNs07.GetResultChpChannelPower(string.Empty, out chPower);
                // lteChp.GetResultChpChannelPowerSpectralDensity(string.Empty, out chPSD);

                /*Display Results*/
                Pout = Convert.ToSingle(chPower);
                // chPsdTextBox.Text = chPSD.ToString();

                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasPoutLteChp");
                return -1;
            }
        }

        public int MeasPoutLteChpNs07_ZeroSpan(long samplesPerRecord, ref float Pout)
        {
            try
            {
                float IqRate = 9155.274f;     // IQ Rate is 1.25 * Bandwidth; Using 6.25k as bandwidth
                // float IqRate = 7812.5f;     // IQ Rate is 1.25 * Bandwidth; Using 6.25k as bandwidth
                //int AcquisitionTime = .002;   // Acquisition Time should possibly be an input instead of samplesPerRecord
                //int IqDataPoint = (int) AcquisitionTime * IqRate;
                int IqDataPoint = 2000;  // Should this be samplesPerRecord?
                
                rfsaSession.ConfigureAcquisitionType(niRFSAConstants.Iq);
                rfsaSession.ConfigureIQCarrierFrequency("", 775000000);
                //rfsaSession.ConfigureResolutionBandwidth("", 6250);   // when using IQ acquisition type, cannot use RBW for bandwidth; specify IqRate
                rfsaSession.ConfigureIQRate("", IqRate);
                
                niComplexNumber[] waveform = new niComplexNumber[IqDataPoint];
                niRFSA_wfmInfo wfmInfo;

                rfsaSession.Initiate();
                rfsaSession.FetchIQSingleRecordComplexF64("", 0, IqDataPoint, 5, waveform, out wfmInfo);
                // rfsaSession.ReadIQSingleRecordComplexF64("", 5, waveform, 500, out wfmInfo);

                // implement RBW Filter if necessary

                float[] magnitudeSquared = new float[IqDataPoint];
                float tempMeanPower = 0;

                for (int i = 0; i < IqDataPoint; i++)
                {
                    // Convert from 1 Ohm V to 1 Ohm W
                    magnitudeSquared[i] = (float)(waveform[i].Real * waveform[i].Real + waveform[i].Imaginary * waveform[i].Imaginary);
                    // Sum for Mean Power calculation
                    tempMeanPower = tempMeanPower + magnitudeSquared[i];
                }

                // tempMeanPower / IqDataPoint yields Mean Power
                // Convert from 1 Ohm W to dBm
                Pout = (float)(10 * Math.Log10(tempMeanPower / IqDataPoint) + 30 - 16.9897 - 3);     // 30 dB for mW to W; 16.9897 dB for 50 Ohm load; 3 dB for power split
                
                //Pout = Pout - 25.19f;     // Mike Lyons commented out. Where did this come from?
                // -88.1, -62.9149551

                /*Display Results*/
                // Pout = Convert.ToSingle(chPower);
                // chPsdTextBox.Text = chPSD.ToString();

                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasPoutLteChp");
                return -1;
            }
        }

        public int MeasPoutTdscdmaChp(long samplesPerRecord, ref float Pout)
        {
            try
            {
                int acquisitionType;
                int numberOfRecords;
                int actualNumberOfSamples;
                double f0 = 0;
                double df = 0;
                double postTriggerDelay;
                double[] powerSpectrum = null;
                niComplexNumber[] waveform = null;
                int averagingDone;
                
                tdscdmaChp.GetRecommendedIqAcquisitionNumberOfRecords(string.Empty, out numberOfRecords);
                tdscdmaChp.GetRecommendedIqAcquisitionPosttriggerDelay(string.Empty, out postTriggerDelay);
                tdscdmaChp.GetRecommendedAcquisitionType(string.Empty, out acquisitionType);

                /*Analyze*/
                if (acquisitionType == niLTESAConstants.RecommendedAcquisitionTypeIq)
                {
                    // string testResult = rfsaSession.GetString(niRFSAProperties.DigitalEdgeRefTriggerSource);

                    waveform = new niComplexNumber[samplesPerRecord];
                    rfsaSession.Initiate();
                    niRFSA_wfmInfo waveformInfo;
                    for (int i = 0; i < numberOfRecords; i++)
                    {
                        rfsaSession.FetchIQSingleRecordComplexF64(string.Empty, i, samplesPerRecord, 2, waveform, out waveformInfo);
                        waveformInfo.relativeInitialX = postTriggerDelay;
                        tdscdmaChp.AnalyzeIQComplexF64(waveformInfo.relativeInitialX, waveformInfo.xIncrement, waveform, Convert.ToInt32(samplesPerRecord), (i == 0) ? 1 : 0, out averagingDone);
                    }
                }
                else
                {
                    /*Get Upper bound on size of powerSpectrum and allocate memory*/
                    int numberOfSamplesForSpectrum;
                    tdscdmaChp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 0, out f0, out df, null, 0, out numberOfSamplesForSpectrum);
                    powerSpectrum = new double[numberOfSamplesForSpectrum];
                    tdscdmaChp.RFSAReadGatedPowerSpectrum(rfsaSession.Handle, string.Empty, 10, out f0, out df, powerSpectrum, numberOfSamplesForSpectrum, out actualNumberOfSamples);
                    /*Analyze*/
                    tdscdmaChp.AnalyzePowerSpectrum(f0, df, powerSpectrum, powerSpectrum.Length);
                }

                rfsaSession.Abort();

                double chPower;
                // double chPSD;

                /*Get Results*/
                tdscdmaChp.GetResultChpChannelPower(string.Empty, out chPower);
                // tdscdmaChp.GetResultChpChannelPowerSpectralDensity(string.Empty, out chPSD);

                /*Display Results*/
                Pout = Convert.ToSingle(chPower);
                // chPsdTextBox.Text = chPSD.ToString();

                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasPoutTdscdmaChp");
                return -1;
            }
        }

        public void MeasEvmEdge(ref float Evm)
        {
            edgesaEvm.RFSAMeasure(rfsaSession.Handle, 5);

            double averageRmsEvm;
            edgesaEvm.GetResultsAverageRmsEvm("", out averageRmsEvm);
            Evm = (float)averageRmsEvm;
        }

        public void MeasPoutCw(ref float Pout)
        {
            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", -40, niRFSAConstants.RisingSlope, 0);
            // rfsaSession.Close();            

            //gsmsa.RFSAMeasure(rfsaSession.Handle, 5);

            //double averagePower;
            //gsmsa.GetPvtResultsAveragePower(null, out averagePower);

            //Pout = (float)averagePower;

            // rfsaSession.ReadPowerSpectrumF64(null, 

            // rfsaSession.Initiate();


            try
            {
                int numberOfSpectralLine = 0;
                // rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.None);
                int result = rfsaSession.GetNumberOfSpectralLines(null, out numberOfSpectralLine);
                double[] powerSpectrumData = new double[numberOfSpectralLine];
                niRFSA_spectrumInfo spectrumInfo;
                rfsaSession.ReadPowerSpectrumF64(null, 5, powerSpectrumData, numberOfSpectralLine, out spectrumInfo);   /// THIS TAKES TOO LONG!
                
                double resultMax = -999;

                for (int i = 0; i < numberOfSpectralLine; i++ )
                {
                    if (powerSpectrumData[i] > resultMax)
                        resultMax = powerSpectrumData[i];
                }

                Pout = Convert.ToSingle(resultMax);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void MeasPoutEdgePvt(ref float Pout, ref bool MaskTestResult)
        {
            edgesaPvt.RFSAMeasure(rfsaSession.Handle, 5);

            double averagePower;
            edgesaPvt.GetPvtResultsAveragePower(null, out averagePower);
            Pout = (float)averagePower;

            int measurementStatus;
            edgesaPvt.GetPvtResultsMeasurementStatus(null, out measurementStatus);
            if (measurementStatus == niEDGESAConstants.True)
                MaskTestResult = true;
            else
                MaskTestResult = false;            
        }

        public void MeasPoutEvdoChp(long samplesPerRecord, ref float Pout)
        {            
            int numRecords, i, done = 0;
            double postTriggerDelay;
            double RMSEVM, peakEVM, magnitudeError, phaseError, IQOffset, frequencyError, peakCDE, peakRCDE, peakACDE;

            int peakCDESF, peakCDECode, peakRCDESF, peakRCDECode, peakACDESF, peakACDECode;
            niComplexNumber[] waveform;
            niRFSA_wfmInfo wfmInfo;

            //long samplesPerRecord2 = 0; ;
            //evDoChp.RFSAConfigureHardware(null, rfsaSession.Handle, null, out samplesPerRecord2);

            evDoChp.GetRecommendedHardwareSettingsNumberOfRecords(null, out numRecords);
            evDoChp.GetRecommendedHardwareSettingsPosttriggerDelay(null, out postTriggerDelay);

            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", -50, niRFSAConstants.RisingSlope, 0);

            rfsaSession.Initiate();
            waveform = new niComplexNumber[Convert.ToInt32(samplesPerRecord)];
            // negAbPower=new int[];

            //Analyze
            for (i = 0; i < numRecords; i++)
            {
                rfsaSession.FetchIQSingleRecordComplexF64("", i, samplesPerRecord, 2, waveform, out wfmInfo);
                wfmInfo.relativeInitialX = postTriggerDelay;
                evDoChp.AnalyzeIQComplexF64(wfmInfo.relativeInitialX, wfmInfo.xIncrement, waveform,
                    Convert.ToInt32(samplesPerRecord), (i == 0), out done);
            }

            rfsaSession.Abort();

            //Get Channel Power
            double chPower;
            evDoChp.GetResultChpChannelPower(null, out chPower);
            Pout = (float)chPower;
        }

        public void MeasPoutGsmTxP(ref float Pout)
        {
            // rfsaSession.Close();
            
            // Measure configures RFSA hardware with GSM, intiates acquisition, fetches the waveforms and analyses.
            gsmsaTxp.RFSAMeasure(rfsaSession.Handle, 5);
            
            double averagePower;
            gsmsaTxp.GetTxpResultsAveragePower(null, out averagePower);            
            Pout = (float)averagePower;                       
        }

        public void MeasPoutGsmEdgeIq(ref float Pout)
        {
            niComplexNumber[] waveform = new niComplexNumber[500];
            niRFSA_wfmInfo wfmInfo;

            rfsaSession.Initiate();
            rfsaSession.FetchIQSingleRecordComplexF64("", 0, 500, 5, waveform, out wfmInfo);

            // rfsaSession.ReadIQSingleRecordComplexF64("", 5, waveform, 500, out wfmInfo);

            float[] tempPower = new float[400];
            float magnitudeSquared = 0;

            for (int i = 100; i < 500; i++)
            {
                tempPower[i - 100] = (float)( waveform[i].Real * waveform[i].Real + waveform[i].Imaginary * waveform[i].Imaginary);
                magnitudeSquared = magnitudeSquared + tempPower[i - 100];
            }

            // magnitudeSquared = magnitudeSquared / 400;
            Pout = (float)(10 * Math.Log10(magnitudeSquared / 400) + 30 - 16.9897 - 3);            
        }
        
        public void MeasPoutGsm(ref float Pout)
        {
            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", -40, niRFSAConstants.RisingSlope, 0);
            // rfsaSession.Close();            

            gsmsa.RFSAMeasure(rfsaSession.Handle, 5);

            double averagePower;
            gsmsa.GetPvtResultsAveragePower(null, out averagePower);

            Pout = (float)averagePower;
        }

        public void MeasPoutWcdmaChp(long samplesPerRecord, ref float Pout)
        {
            int numRecords, i, done = 0;
            double postTriggerDelay;
            double RMSEVM, peakEVM, magnitudeError, phaseError, IQOffset, frequencyError, peakCDE, peakRCDE, peakACDE;

            int peakCDESF, peakCDECode, peakRCDESF, peakRCDECode, peakACDESF, peakACDECode;
            niComplexNumber[] waveform;
            niRFSA_wfmInfo wfmInfo;

            //long samplesPerRecord2 = 0; ;
            //wcdmaChp.RFSAConfigureHardware(null, rfsaSession.Handle, null, out samplesPerRecord2);

            wcdmaChp.GetRecommendedHardwareSettingsNumberOfRecords(null, out numRecords);
            wcdmaChp.GetRecommendedHardwareSettingsPosttriggerDelay(null, out postTriggerDelay);
            
            rfsaSession.Initiate();
            waveform = new niComplexNumber[Convert.ToInt32(samplesPerRecord)];
            // negAbPower=new int[];

            //Analyze
            for (i = 0; i < numRecords; i++)
            {
                rfsaSession.FetchIQSingleRecordComplexF64("", i, samplesPerRecord, 2, waveform, out wfmInfo);
                wfmInfo.relativeInitialX = postTriggerDelay;
                wcdmaChp.AnalyzeIQComplexF64(wfmInfo.relativeInitialX, wfmInfo.xIncrement, waveform,
                    Convert.ToInt32(samplesPerRecord), (i == 0), out done);
            }

            rfsaSession.Abort();

            //Get Channel Power
            double chPower;
            wcdmaChp.GetResultChpChannelPower(null, out chPower);
            Pout = (float)chPower;
        }

        public int MeasTempDig(ref double equipTemp)
        {
            try
            {
                rfsaSession.Commit();
                equipTemp = rfsaSession.GetDouble(niRFSAProperties.DigitizerTemperature, "");
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasTempDig");
                return 0;
            }
        }

        public int MeasTempLo(ref double equipTemp)
        {
            try
            {
                rfsaSession.Commit();
                equipTemp = rfsaSession.GetDouble(niRFSAProperties.LoTemperature, "");
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasTempLo");
                return 0;
            }
        }

        public int MeasTempRfDc(ref double equipTemp)
        {
            try
            {
                rfsaSession.Commit();
                equipTemp = rfsaSession.GetDouble(niRFSAProperties.DeviceTemperature, "");
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasTempRfDc");
                return 0;
            }           
        }
        
        public int RefLevel(float RefLevel)
        {
            // Close();
            




            rfsaSession.ConfigureReferenceLevel(null, (double)RefLevel);
            // rfsaSession.Initiate();

            // double result = rfsaSession.GetDouble(niRFSAProperties.ReferenceLevel);


            return 0;
        }

        //public int RfSaConfHw(string measMode, string Modulation, ref long samplesPerRecord)
        //{
        //    if (Modulation == "WCDMA")
        //    {
        //        if (measMode.ToUpper() == "POUT")
        //            wcdmaChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
        //        else if (measMode.ToUpper() == "ACLR")
        //            wcdmaAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
        //    }
        //    else if (Modulation == "1XEVDO")
        //    {
        //        if (measMode.ToUpper() == "POUT")
        //        {
        //            evDoChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
        //        }
        //        else if (measMode.ToUpper() == "ACLR")
        //        {                    
        //            evDoAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
        //        }
        //    }
        //    else if (Modulation == "LTE")
        //    {
        //        if (measMode.ToUpper() == "POUT")
        //            lteChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
        //        else if (measMode.ToUpper() == "ACLR")
        //            lteAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
        //    }            

        //    return 0;
        //}

        public int RfSaConfHw(string measMode, string Modulation, int measAverage, ref long samplesPerRecord)
        {
            if (Modulation == "WCDMA")
            {
                if (measMode.ToUpper() == "POUT")
                {
                    wcdmaChp.SetChpNumberOfAverages(null, measAverage);
                    wcdmaChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);                    
                }
                else if (measMode.ToUpper() == "ACLR")
                {                                      
                    wcdmaAcp.SetAcpReferenceChannelBandwidth(null, 3840000.0);

                    double[] evdoAdjChBw = new double[2];
                    evdoAdjChBw[0] = 3840000;
                    evdoAdjChBw[1] = 3840000;
                    wcdmaAcp.SetAcpAdjacentChannelsBandwidth(null, evdoAdjChBw);

                    double[] evdoFreqOffset = new double[2];
                    evdoFreqOffset[0] = 5000000;
                    evdoFreqOffset[1] = 10000000;
                    wcdmaAcp.SetAcpAdjacentChannelsFrequencyOffsets(null, evdoFreqOffset);

                    wcdmaAcp.SetAcpNumberOfAverages(null, measAverage);
                    wcdmaAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                }
            }
            else if (Modulation == "1XEVDO")
            {
                if (measMode.ToUpper() == "POUT")
                {

                    evDoChp.SetChpNumberOfAverages(null, measAverage);                    
                    evDoChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);                    
                }
                else if (measMode.ToUpper() == "ACLR")
                {
                    wcdmaAcp.SetAcpReferenceChannelBandwidth(null, 1230000);                    
                    
                    double[] evdoAdjChBw = new double[2];
                    evdoAdjChBw[0] = 30000;
                    evdoAdjChBw[1] = 30000;
                    wcdmaAcp.SetAcpAdjacentChannelsBandwidth(null, evdoAdjChBw);

                    double[] evdoFreqOffset = new double[2];
                    evdoFreqOffset[0] = 885000;
                    evdoFreqOffset[1] = 1980000;
                    wcdmaAcp.SetAcpAdjacentChannelsFrequencyOffsets(null, evdoFreqOffset);

                    wcdmaAcp.SetAcpNumberOfAverages(null, measAverage);
                    wcdmaAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                    // rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.SoftwareEdge);
                }
            }
            else if (Modulation == "LTE")
            {
                if (measMode.ToUpper() == "POUT")
                {
                    lteChp.SetChpNumberOfAverages(string.Empty, measAverage);
                    lteChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                }
                else if (measMode.ToUpper() == "ACLR")
                {
                    lteAcp.SetDuplexMode(string.Empty, niLTESAConstants.DuplexModeUlFdd);
                    lteAcp.SetAcpNumberOfAverages(string.Empty, measAverage);
                    lteAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                }
            }
            else if (Modulation == "LTETDD")
            {
                if (measMode.ToUpper() == "POUT")
                {
                    lteChp.SetChpNumberOfAverages(string.Empty, measAverage);
                    lteChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                }
                else if (measMode.ToUpper() == "ACLR")
                {
                    lteAcp.SetDuplexMode(string.Empty, niLTESAConstants.DuplexModeUlTdd);
                    lteAcp.SetAcpNumberOfAverages(string.Empty, measAverage);
                    lteAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                }
            }
            else if (Modulation == "TDSCDMA")
            {
                double desiredAcquisitionTime;
                if (measMode.ToUpper() == "POUT")
                {
                    tdscdmaChp.SetChpNumberOfAverages(string.Empty, measAverage);
                    tdscdmaChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                    ConfigureTdscdmaAcquisitionTime(1280000, out desiredAcquisitionTime);
                    samplesPerRecord = Convert.ToInt64(rfsaSession.GetDouble(niRFSAProperties.IqRate) * desiredAcquisitionTime);
                    rfsaSession.ConfigureNumberOfSamples(string.Empty, true, samplesPerRecord);
                }
                else if (measMode.ToUpper() == "ACLR")
                {
                    tdscdmaAcp.SetAcpNumberOfAverages(string.Empty, measAverage);
                    tdscdmaAcp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                    ConfigureTdscdmaAcquisitionTime(1280000, out desiredAcquisitionTime);
                    samplesPerRecord = Convert.ToInt64(rfsaSession.GetDouble(niRFSAProperties.IqRate) * desiredAcquisitionTime);
                    rfsaSession.ConfigureNumberOfSamples(string.Empty, true, samplesPerRecord);
                }
            }
            else if (Modulation == "NS07")
            {
                if (measMode.ToUpper() == "POUT")
                {

                    // lteChp.SetChpNumberOfAverages(string.Empty, measAverage);
                    lteChpNs07.SetChpNumberOfAverages(string.Empty, measAverage);
                    // lteChp.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                    lteChpNs07.RFSAConfigureHardware("", rfsaSession.Handle, "", out samplesPerRecord);
                }

            }

            return 0;
        }

        public int SelfCalibrate()
        {
            try
            {
                if (rfsaSession != null)
                    rfsaSession.Close();

                InitialiseMembersGsm();
                rfsaSession = new niRFSA(resourceName, true, false);

                rfsaSession.SelfCal();

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SelfCalibrate");
                return -1;
            }
        }

        private void ConfigureGsm()
        {
            //Configure toolkit properties
            gsmsa.SetPvtEnabled(null, niGSMSAConstants.True);
            gsmsa.SetModaccEnabled(null, niGSMSAConstants.False);
            gsmsa.SetOrfsEnabled(null, niGSMSAConstants.False);
            gsmsa.SetTxpEnabled(null, niGSMSAConstants.False);

            gsmsa.SetPvtNumberOfAverages(null, averages);

            gsmsa.SetPvtAllTracesEnabled(null, 0);  // Disabled
            gsmsa.SetPvtRbwFilterType(null, 2); // None

            gsmsa.SetTscAutoDetectionEnabled(null, tscDetectionEnabled);
            gsmsa.SetTsc(null, tscIn);

            gsmsa.SetUut(null, uut);
            gsmsa.SetBand(null, band);
            gsmsa.SetArfcn(null, arfcn);

            gsmsa.SetBurstSynchronizationEnabled(null, 0);

            //Configure hardware properties
            rfsaSession.ConfigureReferenceLevel(null, maxInPower);            


            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
            

            //rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);
            //rfsaSession.SetString(niRFSAProperties.DigitalEdgeRefTriggerSource, niRFSAConstants.Pfi1Str);




            // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
            // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
            rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

            double carrierFrequency;
            gsmsa.ARFCNToCarrierFrequency(uut, band, arfcn, out carrierFrequency);

            rfsaSession.ConfigureIQCarrierFrequency(null, carrierFrequency);
        }

        private void ConfigureGsmTxp()
        {
            //Configure toolkit properties
            gsmsaTxp.SetPvtEnabled(null, niGSMSAConstants.False);
            gsmsaTxp.SetModaccEnabled(null, niGSMSAConstants.False);
            gsmsaTxp.SetOrfsEnabled(null, niGSMSAConstants.False);
            gsmsaTxp.SetTxpEnabled(null, niGSMSAConstants.True);

            gsmsaTxp.SetTxpMeasurementThresholdType(null, niGSMSAConstants.Relative);
            gsmsaTxp.SetTxpMeasurementThresholdLevel(null, -6);
            gsmsaTxp.SetTxpNumberOfAverages(null, averages);
            gsmsaTxp.SetTxpRbwFilterType(null, niGSMSAConstants.FilterTypeNone);

            gsmsaTxp.SetTxpAllTracesEnabled(null, niGSMSAConstants.False);
            

            
            //Configure hardware properties
            rfsaSession.ConfigureReferenceLevel(null, maxInPower);
            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
            //rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);
            //rfsaSession.SetString(niRFSAProperties.DigitalEdgeRefTriggerSource, niRFSAConstants.Pfi1Str);
            // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
            // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
            rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

            double carrierFrequency;
            gsmsaTxp.ARFCNToCarrierFrequency(uut, band, arfcn, out carrierFrequency);
            
            rfsaSession.ConfigureIQCarrierFrequency(null, carrierFrequency);
        }

        public int ConfigureMeasPowerGsmEdgeIq()
        {
            try
            {
                rfsaSession.ConfigureIQRate("", 1000000);
                rfsaSession.ConfigureNumberOfSamples("", true, 500);
                rfsaSession.ConfigureNumberOfRecords("", true, 1);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureMeasPowerGsmEdgeIq");

                return -1;
            }
        }
        private void ConfigureEdgeAcp()
        {
            //Configure toolkit properties
            edgesaAcp.SetPvtEnabled(null, niEDGESAConstants.False);
            edgesaAcp.SetEvmEnabled(null, niEDGESAConstants.False);
            edgesaAcp.SetOrfsEnabled(null, niEDGESAConstants.True);

            edgesaAcp.SetOrfsOffsetFrequencyMode("", niGSMSAConstants.OffsetFrequencyModeUserDefined);

            double[] offsetFreq = new double[4];
            offsetFreq[0] = -400000;
            offsetFreq[1] = 400000;
            offsetFreq[2] = -600000;
            offsetFreq[3] = 600000;
            edgesaAcp.SetOrfsModulationOffsetFrequencies("", offsetFreq);
            edgesaAcp.SetScalarAttributeI32("", niEDGESAProperties.NumberOfTimeslots, 2);

            edgesaAcp.SetOrfsMeasurementType("", niEDGESAConstants.OrfsMeasurementTypeModulation);

            edgesaAcp.SetOrfsAllTracesEnabled(null, niEDGESAConstants.True);
            edgesaAcp.SetOrfsNumberOfAverages(null, averages);

            edgesaAcp.SetOrfsModulationRbwCarrier(null, modCarrier);
            edgesaAcp.SetOrfsModulationRbwNearOffset(null, modNear);
            edgesaAcp.SetOrfsModulationRbwFarOffset(null, modFar);

            edgesaAcp.SetOrfsFastAveragingMode(null, 1);    // Turn on the fast averaging mode

            edgesaAcp.SetNumberOfTimeslots(null, 2);

            //edgesa.SetOrfsSwitchingRbwCarrier(null, swCarrier);
            //edgesa.SetOrfsSwitchingRbwNearOffset(null, swFar);
            //edgesa.SetOrfsSwitchingRbwFarOffset(null, swNear);

            //Configure hardware properties
            rfsaSession.ConfigureReferenceLevel(null, maxInPower);
            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
            // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
            // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
            rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

            double carrierFrequency;
            edgesaAcp.ARFCNToCarrierFrequency(uut, band, arfcn, out carrierFrequency);

            rfsaSession.ConfigureIQCarrierFrequency(null, carrierFrequency);

            edgesaAcp.RFSAConfigureHardware(rfsaSession.Handle);

            rfsaSession.GetInt64(null, niRFSAProperties.NumberOfSamples, out numberOfSamplesAcp);
        }

        private void ConfigureEdgeEvm()
        {
            //Configure toolkit properties
            edgesaEvm.SetPvtEnabled(null, niEDGESAConstants.False);
            edgesaEvm.SetEvmEnabled(null, niEDGESAConstants.True);
            edgesaEvm.SetOrfsEnabled(null, niEDGESAConstants.False);

            edgesaEvm.SetEvmNumberOfAverages(null, averages);
            edgesaEvm.SetEvmAllTracesEnabled(null, niEDGESAConstants.False);

            edgesaEvm.SetTscAutoDetectionEnabled(null, tscDetectionEnabled);
            edgesaEvm.SetTsc(null, tscIn);

            edgesaEvm.SetUut(null, uut);
            edgesaEvm.SetBand(null, band);
            edgesaEvm.SetArfcn(null, arfcn);

            edgesaEvm.SetScalarAttributeI32("", niEDGESAProperties.NumberOfTimeslots, 2);

            //Configure hardware properties
            rfsaSession.ConfigureReferenceLevel(null, maxInPower);
            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
            rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);
            
            
            // rfsaSession.ConfigureDigitalEdgeRefTrigger(niRFSAConstants.Pfi1Str, niRFSAConstants.RisingEdge, 0);
            rfsaSession.ConfigureDigitalEdgeRefTrigger(niRFSAConstants.PxiTrig0Str, niRFSAConstants.RisingEdge, 0);
            

            // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
            //rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
            rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

            double carrierFrequency;
            edgesaEvm.ARFCNToCarrierFrequency(uut, band, arfcn, out carrierFrequency);

            rfsaSession.ConfigureIQCarrierFrequency(null, carrierFrequency);
        }

        private void ConfigureEdgePvt()
        {
            //Configure toolkit properties
            edgesaPvt.SetPvtEnabled(null, niEDGESAConstants.True);
            edgesaPvt.SetEvmEnabled(null, niEDGESAConstants.False);
            edgesaPvt.SetOrfsEnabled(null, niEDGESAConstants.False);

            edgesaPvt.SetPvtNumberOfAverages(null, averages);

            edgesaPvt.SetPvtAllTracesEnabled(null, 0);  // Disabled
            edgesaPvt.SetPvtRbwFilterType(null, 2); // None

            edgesaPvt.SetTscAutoDetectionEnabled(null, tscDetectionEnabled);
            edgesaPvt.SetTsc(null, tscIn);

            edgesaPvt.SetUut(null, uut);
            edgesaPvt.SetBand(null, band);
            edgesaPvt.SetArfcn(null, arfcn);

            edgesaPvt.SetScalarAttributeI32("", niEDGESAProperties.NumberOfTimeslots, 2);
            edgesaPvt.SetBurstSynchronizationEnabled("", niEDGESAConstants.False);

            //Configure hardware properties
            rfsaSession.ConfigureReferenceLevel(null, maxInPower);
            
            // rfsaSession.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, niRFSAConstants.RisingSlope, 0);
            //rfsaSession.SetInt32(niRFSAProperties.RefTriggerType, niRFSAConstants.DigitalEdge);
            //rfsaSession.ConfigureDigitalEdgeRefTrigger(niRFSAConstants.Pfi1Str, niRFSAConstants.RisingEdge, 0);

            // rfsaSession.SetString(niRFSAProperties.RefClockSource, referenceSource);
            // rfsaSession.SetString(niRFSAProperties.RefClockSource, niRFSAConstants.RefInStr);
            rfsaSession.SetDouble(niRFSAProperties.RefClockRate, 10000000);

            double carrierFrequency;
            edgesaPvt.ARFCNToCarrierFrequency(uut, band, arfcn, out carrierFrequency);

            rfsaSession.ConfigureIQCarrierFrequency(null, carrierFrequency);
        }

        private int ConfigureEvDoAcp()
        {
            try
            {
                if (evDoAcp == null)
                {
                    evDoAcp = new NIWcdmasa(niWcdmasaConstants.ToolkitCompatibilityVersion010000);
                }

                //Configure ACP Measurement properties
                evDoAcp.SetAcpEnabled(null, niWcdmasaConstants.True);
                evDoAcp.SetAcpNumberOfAverages(null, 5);
                evDoAcp.SetAcpReferenceChannelBandwidth(null, 1230000);
                evDoAcp.SetAcpAverageType(null, niWcdmasaConstants.AcpAverageTypeLinear);
                evDoAcp.SetAcpMeasurementResultsType(null, niWcdmasaConstants.AcpMeasurementResultsTypeTotalPowerReference);

                double[] evdoAdjChBw = new double[2];
                evdoAdjChBw[0] = 30000;
                evdoAdjChBw[1] = 30000;
                evDoAcp.SetAcpAdjacentChannelsBandwidth(null, evdoAdjChBw);

                double[] evdoFreqOffset = new double[2];
                evdoFreqOffset[0] = 885000;
                evdoFreqOffset[1] = 1980000;
                evDoAcp.SetAcpAdjacentChannelsFrequencyOffsets(null, evdoFreqOffset);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureWcdmaChp");
                return -1;
            }
        }

        private int ConfigureEvDoChp()
        {
            try
            {
                if (evDoChp == null)
                {
                    evDoChp = new NIWcdmasa(niWcdmasaConstants.ToolkitCompatibilityVersion010000);
                }

                // Configure ChPow Measurement Properties
                evDoChp.SetChpEnabled(null, niWcdmasaConstants.True);
                evDoChp.SetChpNumberOfAverages(null, 1);
                evDoChp.SetChpSpan(null, 1230000);
                // wcdmasaSessionPout.SetChpMeasurementBandwidth(null, 3840000.0);
                evDoChp.SetChpMeasurementBandwidth(null, 1230000);
                evDoChp.SetTriggerDelay(null, 0);
                evDoChp.SetCarrierFrequency(null, 1000000000);
                                
                // evDoChp.RFSAAutoLevel(rfsaSession.Handle, "", 5e+6, 0.01, 5, out maxInPower);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureWcdmaChp");
                return -1;
            }
        }

        private int ConfigureLteAcp()
        {
            try
            {
                if (lteAcp == null)
                {
                    lteAcp = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000); 
                }

                //double maxInPower = 0, triggerLevel;
                //int triggerEdge;

                //resourceName = rfsaResourceTextBox.Text;
                //systemBW = (double)systemBandwidthComboBox.SelectedValue;
                //triggerLevel = (double)triggerLevelNumeric.Value;
                //triggerEdge = (int)triggerEdgeComboBox.SelectedValue;

                /*Set toolkit properties*/
                // aSession = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                lteAcp.SelectMeasurements(niLTESAConstants.AcpMeasurement);
                lteAcp.SetHardwareSettingsCarrierFrequency(string.Empty, 1000000000);
                lteAcp.SetSystemBandwidth(string.Empty, 10000000);
                lteAcp.SetHardwareSettingsTriggerDelay(string.Empty, 0);
                lteAcp.SetHardwareSettingsMaxRealtimeBandwidth(string.Empty, 40000000);
                lteAcp.SetAcpMeasurementResultsType(string.Empty, 0);
                lteAcp.SetAcpAverageType(string.Empty, 0);
                lteAcp.SetAcpFrequencyListType(string.Empty, 0);
                lteAcp.SetAcpNumberOfAverages(string.Empty, 1);
                lteAcp.SetAcpReferenceChannelBandwidth(string.Empty, 9000000);

                //double[] chBw = new double[3];
                //chBw[0] = 3840000;
                //chBw[1] = 3840000;
                //chBw[2] = 9000000;
                //lteAcp.SetAcpAdjacentChannelsBandwidths(string.Empty, chBw, 3);

                //double[] freqOffset = new double[3];
                //freqOffset[0] = 7500000;
                //freqOffset[1] = 12500000;
                //freqOffset[2] = 10000000;

                //lteAcp.SetAcpAdjacentChannelsFrequencyOffsets(string.Empty, freqOffset, 3); 
                

                /*Set RFSA properties*/
                //rfsa = new niRFSA(rfsaResourceTextBox.Text, true, false);
                //rfsa.ConfigureRefClock(refClkSourceComboBox.SelectedValue.ToString(), 10e+6);
                //rfsa.ConfigureIQCarrierFrequency(string.Empty, (double)carrierFreqNumeric.Value);
                //rfsa.SetDouble(niRFSAProperties.ExternalGain, -(double)extAttnNumeric.Value);
                //if ((int)autoLevelComboBox.SelectedValue == 1)
                //{
                //    lteAcp.RFSAAutoLevel(rfsa.Handle, string.Empty, systemBW, 0.01, 5, out maxInPower);
                //}
                //rfsa.ConfigureReferenceLevel(null, maxInPower);
                //if ((int)triggerOnComboBox.SelectedValue == 1)
                //{
                //    rfsa.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, triggerEdge, 0);
                //}
                //else
                //{
                //    rfsa.DisableRefTrigger();
                //}
                long samplesPerRecord = 0;
                lteAcp.RFSAConfigureHardware(string.Empty, rfsaSession.Handle, string.Empty, out samplesPerRecord);
                

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureLteAcp");
                return -1;
            }
        }

        private int ConfigureLteChp()
        {
            try
            {
                if (lteChp == null)
                {
                    lteChp = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                }

                double maxInPower = 0, triggerLevel;
                int triggerEdge;

                //resourceName = rfsaResourceTextBox.Text;
                //systemBW = (double)systemBandwidthComboBox.SelectedValue;
                //triggerLevel = (double)triggerLevelNumeric.Value;
                //triggerEdge = (int)triggerEdgeComboBox.SelectedValue;

                /*Set toolkit properties*/
                
                lteChp.SelectMeasurements(niLTESAConstants.ChpMeasurement);
                lteChp.SetHardwareSettingsCarrierFrequency(string.Empty, 1000000000);
                lteChp.SetSystemBandwidth(string.Empty, 10000000);
                lteChp.SetHardwareSettingsTriggerDelay(string.Empty, 0);
                lteChp.SetChpNumberOfAverages(string.Empty, 2);
                lteChp.SetChpSpanType(string.Empty, niLTESAConstants.ChpSpanTypeStandard);
                lteChp.SetChpSpan(string.Empty, 12000000);
                lteChp.SetChpMeasurementBandwidth(string.Empty, 10000000);

                /* Set Sweep Time */
                lteChp.SetChpAutoSweepTimeEnabled(string.Empty, niLTESAConstants.False);
                lteChp.SetChpSweepTime(string.Empty, 0.000125);

                /*Set RFSA properties*/
                //rfsa = new niRFSA(rfsaResourceTextBox.Text, true, false);
                //rfsa.ConfigureRefClock(refClkSourceComboBox.SelectedValue.ToString(), 10e+6);
                //rfsa.ConfigureIQCarrierFrequency(string.Empty, (double)carrierFreqNumeric.Value);
                //rfsa.SetDouble(niRFSAProperties.ExternalGain, -(double)extAttnNumeric.Value);
                //if ((int)autoLevelComboBox.SelectedValue == 1)
                //{
                //    lteChp.RFSAAutoLevel(rfsa.Handle, string.Empty, systemBW, 0.01, 5, out maxInPower);
                //}
                //rfsa.ConfigureReferenceLevel(null, maxInPower);
                //if ((int)triggerOnComboBox.SelectedValue == 1)
                //{
                //    rfsa.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, triggerEdge, 0);
                //}
                //else
                //{
                //    rfsa.DisableRefTrigger();
                //}
                long samplesPerRecord = 0;
                lteChp.RFSAConfigureHardware(string.Empty, rfsaSession.Handle, string.Empty, out samplesPerRecord);

                
                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureLteChp");
                return -1;
            }
        }

        private int ConfigureLteChpNs07()
        {
            try
            {
                if (lteChpNs07 == null)
                {
                    lteChpNs07 = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                }

                double maxInPower = 0, triggerLevel;
                int triggerEdge;

                //resourceName = rfsaResourceTextBox.Text;
                //systemBW = (double)systemBandwidthComboBox.SelectedValue;
                //triggerLevel = (double)triggerLevelNumeric.Value;
                //triggerEdge = (int)triggerEdgeComboBox.SelectedValue;

                /*Set toolkit properties*/
                // Ideal IBW/Bin Ratio == 100
                 
                lteChpNs07.SelectMeasurements(niLTESAConstants.ChpMeasurement);
                lteChpNs07.SetHardwareSettingsCarrierFrequency(string.Empty, 1000000000);
                lteChpNs07.SetSystemBandwidth(string.Empty, 1400000);
                lteChpNs07.SetHardwareSettingsTriggerDelay(string.Empty, 0);
                lteChpNs07.SetChpNumberOfAverages(string.Empty, 4);                             // averages 1-4 were about the same execution speed in LV
                lteChpNs07.SetChpSpanType(string.Empty, niLTESAConstants.ChpSpanTypeCustom);    // custom will use span and measurement bandwidth instead of system bandwidth
                lteChpNs07.SetChpMeasurementBandwidth(string.Empty, 6250);
                lteChpNs07.SetChpSpan(string.Empty, 200000);                                    // span 200kHz with Data points 1024 gave best execution time performance
                lteChpNs07.SetChpAutoSweepTimeEnabled(string.Empty, niLTESAConstants.True);
                lteChpNs07.SetChpAutoNumDataPointsEnabled(string.Empty, niLTESAConstants.False);
                lteChpNs07.SetChpNumDataPoints(string.Empty, 1024);                             // Power of 2

                /*Set RFSA properties*/
                //rfsa = new niRFSA(rfsaResourceTextBox.Text, true, false);
                //rfsa.ConfigureRefClock(refClkSourceComboBox.SelectedValue.ToString(), 10e+6);
                //rfsa.ConfigureIQCarrierFrequency(string.Empty, (double)carrierFreqNumeric.Value);
                //rfsa.SetDouble(niRFSAProperties.ExternalGain, -(double)extAttnNumeric.Value);
                //if ((int)autoLevelComboBox.SelectedValue == 1)
                //{
                //    lteChp.RFSAAutoLevel(rfsa.Handle, string.Empty, systemBW, 0.01, 5, out maxInPower);
                //}
                //rfsa.ConfigureReferenceLevel(null, maxInPower);
                //if ((int)triggerOnComboBox.SelectedValue == 1)
                //{
                //    rfsa.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, triggerEdge, 0);
                //}
                //else
                //{
                //    rfsa.DisableRefTrigger();
                //}
                long samplesPerRecord = 0;
                lteChpNs07.RFSAConfigureHardware(string.Empty, rfsaSession.Handle, string.Empty, out samplesPerRecord);


                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureLteChp");
                return -1;
            }
        }

        private int ConfigureTdscdmaAcp()
        {
            try
            {
                int chipRate = 1280000;     // 1.28 Mcps, 3.84 Mcps, or 7.68 Mcps
                double sweepTime;

                if (tdscdmaAcp == null)
                {
                    tdscdmaAcp = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                }

                //double maxInPower = 0, triggerLevel;
                //int triggerEdge;

                //resourceName = rfsaResourceTextBox.Text;
                //systemBW = (double)systemBandwidthComboBox.SelectedValue;
                //triggerLevel = (double)triggerLevelNumeric.Value;
                //triggerEdge = (int)triggerEdgeComboBox.SelectedValue;

                /*Set toolkit properties*/
                // aSession = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                tdscdmaAcp.SelectMeasurements(niLTESAConstants.AcpMeasurement);
                tdscdmaAcp.SetHardwareSettingsCarrierFrequency(string.Empty, 1000000000);
                tdscdmaAcp.SetSystemBandwidth(string.Empty, 10000000);
                tdscdmaAcp.SetHardwareSettingsTriggerDelay(string.Empty, 0);
                tdscdmaAcp.SetHardwareSettingsMaxRealtimeBandwidth(string.Empty, 40000000);
                tdscdmaAcp.SetAcpMeasurementResultsType(string.Empty, 0);
                tdscdmaAcp.SetAcpAverageType(string.Empty, 0);
                tdscdmaAcp.SetAcpNumberOfAverages(string.Empty, 1);

                // Configure Custom ACP Frequency List
                tdscdmaAcp.SetAcpFrequencyListType(string.Empty, niLTESAConstants.AcpFrequencyListTypeCustom);                
                // Configure Reference Channel
                tdscdmaAcp.SetAcpReferenceChannelBandwidth(string.Empty, (float)chipRate);
                ////KCC
                //tdscdmaAcp.SetAcpReferenceChannelRrcFilterEnabled(string.Empty, niLTESAConstants.True);
                //tdscdmaAcp.SetAcpReferenceChannelRrcFilterAlpha(string.Empty, 0.22);
                
                // Configure Adjacent Channels
                tdscdmaAcp.SetAcpAdjacentChannelsBandwidths(string.Empty, new double[] {(float)chipRate, (float)chipRate }, 2);
                tdscdmaAcp.SetAcpAdjacentChannelsSidebands(string.Empty, new int[] { niLTESAConstants.AcpAdjacentChannelsSidebandsBoth, niLTESAConstants.AcpAdjacentChannelsSidebandsBoth }, 2);
                tdscdmaAcp.SetAcpAdjacentChannelsRrcFilterEnabled(string.Empty, new int[] { niLTESAConstants.True, niLTESAConstants.True }, 2);
                tdscdmaAcp.SetAcpAdjacentChannelsRrcFilterAlpha(string.Empty, new double[] { 0.22, 0.22 }, 2);
                tdscdmaAcp.SetAcpAdjacentChannelsEnabled(string.Empty, new int[] { niLTESAConstants.True, niLTESAConstants.True }, 2);
                // Configure Adjacent Channel Frequency Offsets
                double[] freqOffset = new double[2];
                if (chipRate == 1280000)
                {
                    freqOffset[0] = 1600000;
                    freqOffset[1] = 3200000;
                }
                else if (chipRate == 3840000)
                {
                    freqOffset[0] = 5000000;
                    freqOffset[1] = 10000000;
                }
                else if (chipRate == 7680000)
                {
                    freqOffset[0] = 10000000;
                    freqOffset[1] = 20000000;
                }
                else
                    throw new Exception("Chip Rate is out of limits.");
                tdscdmaAcp.SetAcpAdjacentChannelsFrequencyOffsets(string.Empty, freqOffset, 2); 


                /*Set RFSA properties*/
                //rfsa = new niRFSA(rfsaResourceTextBox.Text, true, false);
                //rfsa.ConfigureRefClock(refClkSourceComboBox.SelectedValue.ToString(), 10e+6);
                //rfsa.ConfigureIQCarrierFrequency(string.Empty, (double)carrierFreqNumeric.Value);
                //rfsa.SetDouble(niRFSAProperties.ExternalGain, -(double)extAttnNumeric.Value);
                //if ((int)autoLevelComboBox.SelectedValue == 1)
                //{
                //    tdscdmaAcp.RFSAAutoLevel(rfsa.Handle, string.Empty, systemBW, 0.01, 5, out maxInPower);
                //}
                //rfsa.ConfigureReferenceLevel(null, maxInPower);
                //if ((int)triggerOnComboBox.SelectedValue == 1)
                //{
                //    rfsa.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, triggerEdge, 0);
                //}
                //else
                //{
                //    rfsa.DisableRefTrigger();
                //}
                long samplesPerRecord = 0;
                tdscdmaAcp.RFSAConfigureHardware(string.Empty, rfsaSession.Handle, string.Empty, out samplesPerRecord);
                ConfigureTdscdmaAcquisitionTime(chipRate, out sweepTime);
                //tdscdmaAcp.SetAcpReferenceChannelAutoSweepTimeEnabled(string.Empty, niLTESAConstants.False);
                //tdscdmaAcp.SetAcpReferenceChannelSweepTime(string.Empty, sweepTime);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureTdscdmaAcp");
                return -1;
            }
        }

        private int ConfigureTdscdmaChp()
        {
            try
            {
                int chipRate = 1280000;     // 1.28 Mcps, 3.84 Mcps, or 7.68 Mcps

                if (tdscdmaChp == null)
                {
                    tdscdmaChp = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                }

                double maxInPower = 0, triggerLevel, sweepTime;
                int triggerEdge;

                //resourceName = rfsaResourceTextBox.Text;
                //systemBW = (double)systemBandwidthComboBox.SelectedValue;
                //triggerLevel = (double)triggerLevelNumeric.Value;
                //triggerEdge = (int)triggerEdgeComboBox.SelectedValue;

                /*Set toolkit properties*/

                tdscdmaChp.SelectMeasurements(niLTESAConstants.ChpMeasurement);
                tdscdmaChp.SetHardwareSettingsCarrierFrequency(string.Empty, 1000000000);
                tdscdmaChp.SetSystemBandwidth(string.Empty, 10000000);
                tdscdmaChp.SetHardwareSettingsTriggerDelay(string.Empty, 0);
                tdscdmaChp.SetChpNumberOfAverages(string.Empty, 2);
                
                // Configure Custom CHP Span and BW settings
                tdscdmaChp.SetChpSpanType(string.Empty, niLTESAConstants.ChpSpanTypeCustom);
                double span;
                if (chipRate == 1280000)
                {
                    span = 3000000;
                }
                else if (chipRate == 3840000)
                {
                    span = 5000000;
                }
                else if (chipRate == 7680000)
                {
                    span = 10000000;
                }
                else
                    throw new Exception("Chip Rate is out of limits.");
                tdscdmaChp.SetChpSpan(string.Empty, span);
                tdscdmaChp.SetChpMeasurementBandwidth(string.Empty, (float)chipRate);
                /* Set Sweep Time */
                tdscdmaChp.SetChpAutoSweepTimeEnabled(string.Empty, niLTESAConstants.False);
                tdscdmaChp.SetChpSweepTime(string.Empty, 0.000660);
                //tdscdmaChp.SetChpAutoNumDataPointsEnabled(string.Empty, niLTESAConstants.False);
                //tdscdmaChp.SetChpNumDataPoints(string.Empty, 256);

                /*Set RFSA properties*/
                //rfsa = new niRFSA(rfsaResourceTextBox.Text, true, false);
                //rfsa.ConfigureRefClock(refClkSourceComboBox.SelectedValue.ToString(), 10e+6);
                //rfsa.ConfigureIQCarrierFrequency(string.Empty, (double)carrierFreqNumeric.Value);
                //rfsa.SetDouble(niRFSAProperties.ExternalGain, -(double)extAttnNumeric.Value);
                //if ((int)autoLevelComboBox.SelectedValue == 1)
                //{
                //    tdscdmaChp.RFSAAutoLevel(rfsa.Handle, string.Empty, systemBW, 0.01, 5, out maxInPower);
                //}
                //rfsa.ConfigureReferenceLevel(null, maxInPower);
                //if ((int)triggerOnComboBox.SelectedValue == 1)
                //{
                //    rfsa.ConfigureIQPowerEdgeRefTrigger("0", triggerLevel, triggerEdge, 0);
                //}
                //else
                //{
                //    rfsa.DisableRefTrigger();
                //}
                long samplesPerRecord = 0;
                tdscdmaChp.RFSAConfigureHardware(string.Empty, rfsaSession.Handle, string.Empty, out samplesPerRecord);
                ConfigureTdscdmaAcquisitionTime(1280000, out sweepTime);
                //tdscdmaChp.SetChpSweepTime(string.Empty, sweepTime);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureLteChp");
                return -1;
            }
        }

        private int ConfigureTdscdmaAcquisitionTime(int chipRate, out double desiredTime)
        {
            try
            {
                int totalSlotChips;
                if (chipRate == 1280000)
                {
                    totalSlotChips = 864;
                }
                else if (chipRate == 3840000)
                {
                    totalSlotChips = 2560;
                }
                else if (chipRate == 7680000)
                {
                    totalSlotChips = 5120;
                }
                else
                    throw new Exception("Chip Rate is out of limits.");

                
                //tdscdmaAcp.SetAcpReferenceChannelAutoSweepTimeEnabled(string.Empty, niLTESAConstants.False);
                //tdscdmaAcp.SetAcpReferenceChannelSweepTime(string.Empty, (float)totalSlotChips / chipRate);
                desiredTime = (float)totalSlotChips / chipRate;
                if (rfsaSession.GetInt32(niRFSAProperties.RefTriggerType) == niRFSAConstants.IqPowerEdge)
                {
                    rfsaSession.SetDouble(niRFSAProperties.RefTriggerMinimumQuietTime, 16f / chipRate);
                }

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureTdscdmaAcquisitionTime");
                desiredTime = 0;
                return -1;
            }
        }

        private int ConfigureWcdmaAcp()
        {
            try
            {
                if (wcdmaAcp == null)
                {
                    wcdmaAcp = new NIWcdmasa(niWcdmasaConstants.ToolkitCompatibilityVersion010000);
                }

                //Configure ACP Measurement properties
                wcdmaAcp.SetAcpEnabled(null, niWcdmasaConstants.True);
                wcdmaAcp.SetAcpNumberOfAverages(null, 5);
                wcdmaAcp.SetAcpReferenceChannelBandwidth(null, 3840000.0);
                wcdmaAcp.SetAcpAverageType(null, niWcdmasaConstants.AcpAverageTypeLinear);
                wcdmaAcp.SetAcpMeasurementResultsType(null, niWcdmasaConstants.AcpMeasurementResultsTypeTotalPowerReference);

                wcdmaAcp.RFSAAutoLevel(rfsaSession.Handle, "", 5e+6, 0.01, 5, out maxInPower);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureWcdmaChp");
                return -1;
            }
        }

        private int ConfigureWcdmaChp()
        {
            try
            {
                if (wcdmaChp == null)
                {
                    wcdmaChp = new NIWcdmasa(niWcdmasaConstants.ToolkitCompatibilityVersion010000);
                }

                // Configure ChPow Measurement Properties
                wcdmaChp.SetChpEnabled(null, niWcdmasaConstants.True);
                wcdmaChp.SetChpNumberOfAverages(null, 1);
                wcdmaChp.SetChpSpan(null, 5000000.0);
                // wcdmasaSessionPout.SetChpMeasurementBandwidth(null, 3840000.0);
                wcdmaChp.SetChpMeasurementBandwidth(null, 5000000.0);
                wcdmaChp.SetTriggerDelay(null, 0);
                wcdmaChp.SetCarrierFrequency(null, 1000000000);

                wcdmaChp.RFSAAutoLevel(rfsaSession.Handle, "", 5e+6, 0.01, 5, out maxInPower);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureWcdmaChp");
                return -1;
            }
        }

        public int ConfigureSpectrumFrequencyCenterSpan(double centerFreq, double span)
        {
            try
            {
                rfsaSession.ConfigureSpectrumFrequencyCenterSpan(null, centerFreq, span);

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureSpectrumFrequencyCenterSpan");
                return -1;
            }
        }

        private void InitialiseMembersGsm()
        {
            resourceName = "VSA";
            triggerLevel = -20;
            maxInPower = 0;
            arfcn = 1;
            averages = 1;
            frequency = 1000000000;

            band = 0;
            uut = 1;
            tscIn = 0;
            tscDetectionEnabled = 0;
            referenceSource = "OnboardClock";            
        }

        private void InitialiseMembersEdgeAcp()
        {
            resourceName = "VSA";
            triggerLevel = -20;
            maxInPower = 0;
            arfcn = 1;
            averages = 1;
            frequency = 1000000000;

            band = 0;
            uut = 1;
            referenceSource = "OnboardClock";

            modCarrier = 30000.0;
            modFar = 100000.0;
            modNear = 30000.0;            
        }

        private void InitialiseMembersEdgePvt()
        {
            resourceName = "VSA";
            triggerLevel = -20;
            maxInPower = 0;
            arfcn = 1;
            averages = 1;
            frequency = 1000000000;

            band = 0;
            uut = 1;
            tscIn = 0;
            tscDetectionEnabled = 0;
            referenceSource = "OnboardClock";            
        }

        private void InitialiseMembersEdgeTxp()
        {
            resourceName = "VSA";
            triggerLevel = -20;
            maxInPower = 0;
            arfcn = 1;
            averages = 1;
            frequency = 1000000000;

            band = 0;
            uut = 1;
            tscIn = 0;
            tscDetectionEnabled = 0;
            referenceSource = "OnboardClock";    
        }

        private void InitialiseMembersEdgeEvm()
        {
            resourceName = "VSA";
            triggerLevel = -20;
            maxInPower = 0;
            arfcn = 1;
            averages = 1;
            frequency = 1000000000;

            band = 0;
            uut = 1;
            tscIn = 0;
            tscDetectionEnabled = 0;
            referenceSource = "OnboardClock";    
        }

        public int PerformThermalCorrection()
        {
            int result = 0;
            try
            {
                rfsaSession.Initiate();
                result = rfsaSession.PerformThermalCorrection();
                rfsaSession.Abort();
                return result;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "PerformThermalCorrection");
                return result;
            }
        }
    }
}
