using System;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.Examples.RFSA
{
    /// <summary>
    /// 
    /// </summary>
    public class RFSAStartedIQExample
    {
        const Int32 numberOfSamples = 1000;
        niRFSA rfsa;
        string resourceName = "RFSA";
        niComplexNumber[] dataBlock;
        niRFSA_wfmInfo wfmInfo;
        double magnitudeSquared;
        double accumulator;
        double refClockRate;
        double carrierFrequency;
        double iqRate;
        internal void Run()
        {
            try
            {
                InitializeVariables();
                InitializeRFSA();
                ConfigureRFSA();
                RetrieveResults();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
            finally
            {
                /* Close session */
                CloseSession();
                Console.WriteLine("Press any key to exit.....");
                Console.ReadKey();
            }
        }
        private void InitializeVariables()
        {
            dataBlock = new niComplexNumber[numberOfSamples];
            wfmInfo = new niRFSA_wfmInfo();
            accumulator = 0.0;
            refClockRate = 10e6;
            carrierFrequency = 1e9;
            iqRate = 1e6;
        }
        private void InitializeRFSA()
        {
            rfsa = new niRFSA(resourceName, true, false);            
        }
        private void ConfigureRFSA()
        {
            rfsa.ConfigureRefClock("OnboardClock", refClockRate);
            rfsa.ConfigureReferenceLevel("", 0);
            rfsa.ConfigureAcquisitionType(niRFSAConstants.Iq);
            rfsa.ConfigureIQCarrierFrequency("", carrierFrequency);
            rfsa.ConfigureNumberOfSamples("", true, numberOfSamples);
            rfsa.ConfigureIQRate("", iqRate);
        }

        private void RetrieveResults()
        {            
            rfsa.ReadIQSingleRecordComplexF64("", 10.0, dataBlock, numberOfSamples, out wfmInfo);
            if (numberOfSamples > 0)
            {
                for (int i = 0; i < numberOfSamples; ++i)
                {
                    magnitudeSquared = dataBlock[i].Real * dataBlock[i].Real + dataBlock[i].Imaginary * dataBlock[i].Imaginary;

                    /* we need to handle this because log(0) return a range error. */
                    if (magnitudeSquared == 0.0)
                    {
                        magnitudeSquared = 0.00000001;
                    }

                    accumulator += 10.0 * Math.Log10((magnitudeSquared / (2.0 * 50.0)) * 1000.0);

                }

                Console.WriteLine("Average power = {0} dBm" + Environment.NewLine, Math.Round(accumulator / numberOfSamples, 1));
            }

        }
        private void CloseSession()
        {

            try
            {
                if (rfsa != null)
                {
                    rfsa.Close();
                    rfsa = null;
                }
            }
            catch (Exception ex)
            {
                DisplayError(ex);
                Environment.Exit(0);
            }
        }
        static private void DisplayError(Exception ex)
        {
            Console.WriteLine("ERROR:" + Environment.NewLine + ex.GetType() + ": " + ex.Message);
        }
    }
}
