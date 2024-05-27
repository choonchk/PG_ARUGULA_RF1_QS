using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using NationalInstruments.ModularInstruments.Interop;
using NationalInstruments.RFToolkits.Interop;
using IqWaveform;
//using NationalInstruments.ModularInstruments.Ltesa;

namespace EqLib
{

    public interface iEVM
    {
        bool Initialize();
        double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform);
    }

    public static class EVM
    {
        public static List<iEVM> isInitialized = new List<iEVM>();

        public class EDGE : iEVM
        {
            public niEDGESA edgesaEvm;

            #region iEVM Members

            public bool Initialize()
            {
                try
                {
                    int arfcn = 1;
                    int averages = 1;
                    int band = 0;
                    int uut = 1;
                    int tscIn = 0;
                    int tscDetectionEnabled = 0;

                    edgesaEvm = new niEDGESA(niEDGESAConstants.ToolkitCompatibilityVersion100);
                    //Configure toolkit properties
                    edgesaEvm.SetPvtEnabled(null, niEDGESAConstants.False);
                    edgesaEvm.SetEvmEnabled(null, niEDGESAConstants.True);
                    edgesaEvm.SetOrfsEnabled(null, niEDGESAConstants.False);
                    edgesaEvm.SetEvmNumberOfAverages(null, averages);
                    edgesaEvm.SetEvmAllTracesEnabled(null, niEDGESAConstants.True);
                    edgesaEvm.SetTscAutoDetectionEnabled(null, tscDetectionEnabled);
                    edgesaEvm.SetTsc(null, tscIn);
                    edgesaEvm.SetUut(null, uut);
                    edgesaEvm.SetBand(null, band);
                    edgesaEvm.SetArfcn(null, arfcn);
                    edgesaEvm.SetScalarAttributeI32("", niEDGESAProperties.NumberOfTimeslots, 1);
                    edgesaEvm.SetBurstSynchronizationEnabled(null, 1);

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return false;
                }

            }

            public double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform)
            {
                double recPreTriggerDelay;
                edgesaEvm.GetRecommendedHardwareSettingsPreTriggerDelay("", out recPreTriggerDelay);
                long recPreTriggerSamples = (long)((double)waveform.VsaIQrate * recPreTriggerDelay);

                niComplexNumber[] preTrigSampledIqData = new niComplexNumber[iqData.Length + recPreTriggerSamples];
                Array.Copy(iqData, 0, preTrigSampledIqData, recPreTriggerSamples, iqData.Length);

                double recPostTriggerDelay;
                edgesaEvm.GetRecommendedHardwareSettingsPostTriggerDelay("", out recPostTriggerDelay);

                int done;
                edgesaEvm.AnalyzeIQComplexF64(recPostTriggerDelay, 1.0 / (double)waveform.VsaIQrate, preTrigSampledIqData, preTrigSampledIqData.Length, 1, out done);

                double evm;
                edgesaEvm.GetResultsAverageRmsEvm("", out evm);

                return evm;
            }
            
            #endregion
        }
    
        public class GSM : iEVM
        {
            #region iEVM Members

            public bool Initialize()
            {
                throw new NotImplementedException();
            }

            public double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class WCDMA : iEVM
        {
            #region iEVM Members

            public bool Initialize()
            {
                throw new NotImplementedException();
            }

            public double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class LTE : iEVM
        {
            //public niLTESA LteSaEvm;

            #region iEVM Members

            public bool Initialize()
            {
                try
                {
                    int arfcn = 1;
                    int averages = 1;
                    int band = 0;
                    int uut = 1;
                    int tscIn = 0;
                    int tscDetectionEnabled = 0;

                    ////Begin EVM setup
                    //LteSaEvm = new niLTESA(niLTESAConstants.ToolkitCompatibilityVersion010000);
                    
                    ////Configure toolkit properties
                    //LteSaEvm.SetPvtEnabled(null, niLTESAConstants.False);
                    //LteSaEvm.SetEvmEnabled(null, niLTESAConstants.True);
                    //LteSaEvm.SetOrfsEnabled(null, niLTESAConstants.False);
                    //LteSaEvm.SetEvmNumberOfAverages(null, averages);
                    //LteSaEvm.SetEvmAllTracesEnabled(null, niLTESAConstants.True);
                    //LteSaEvm.SetTscAutoDetectionEnabled(null, tscDetectionEnabled);
                    //LteSaEvm.SetTsc(null, tscIn);
                    //LteSaEvm.SetUut(null, uut);
                    //LteSaEvm.SetBand(null, band);
                    //LteSaEvm.SetArfcn(null, arfcn);
                    //LteSaEvm.SetScalarAttributeI32("", niLTESAProperties.NumberOfTimeslots, 1);
                    //LteSaEvm.SetBurstSynchronizationEnabled(null, 1);

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return false;
                }

            }

            public double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform)
            {
                return 999;
            }

            #endregion
        }

        public class LTETDD : iEVM
        {
            #region iEVM Members

            public bool Initialize()
            {
                return true;
            }

            public double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform)
            {
                return 999;
            }

            #endregion
        }

        public class none : iEVM
        {
            #region iEVM Members

            public bool Initialize()
            {
                throw new NotImplementedException();
            }

            public double CalcEvm(niComplexNumber[] iqData, IQ.Waveform waveform)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

    }
}
