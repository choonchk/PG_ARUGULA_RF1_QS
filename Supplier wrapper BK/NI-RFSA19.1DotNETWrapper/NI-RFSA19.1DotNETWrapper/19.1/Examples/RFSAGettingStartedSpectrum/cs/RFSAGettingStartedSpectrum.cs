using System;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.Examples.RFSA
{
    /// <summary>
    /// 
    /// </summary>
    public class RFSAGettingStartedSpectrumExample
    {
        const string resourceName = "RFSA";
        int numberOfSpectralLines;
        niRFSA rfsa;
        double[] spectrumDataBlocks;
        niRFSA_spectrumInfo spectrumInfo;
        double refClockRate;
        double greatestPeakPower;
        double greatestPeakFrequency;
        double startFrequency;
        double stopFrequency;
        double resolutionBandwidth;

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
                CloseSession();
                Console.WriteLine("Press any key to exit.....");
                Console.ReadKey();
            }
        }
        private void InitializeVariables()
        {
            spectrumInfo = new niRFSA_spectrumInfo();
            refClockRate = 10e6;
            startFrequency = 990e6;
            stopFrequency = 1010e6;
            resolutionBandwidth = 10e3;
        }
        private void InitializeRFSA()
        {
            rfsa = new niRFSA(resourceName, true, false);
        }
        private void ConfigureRFSA()
        {
            rfsa.ConfigureRefClock("OnboardClock", refClockRate);
            rfsa.ConfigureReferenceLevel("", 0);
            rfsa.ConfigureAcquisitionType(niRFSAConstants.Spectrum);
            rfsa.ConfigureSpectrumFrequencyStartStop("", startFrequency, stopFrequency);
            rfsa.ConfigureResolutionBandwidth("", resolutionBandwidth);
        }

        private void RetrieveResults()
        {
            /* Read the power spectrum */
            /* We need the number of spectral lines in order to know the size of the
             * spectrum array. */

            rfsa.GetNumberOfSpectralLines("", out numberOfSpectralLines);
            spectrumDataBlocks = new double[numberOfSpectralLines];
            rfsa.ReadPowerSpectrumF64("", 10.0, spectrumDataBlocks, numberOfSpectralLines, out spectrumInfo);
            /* Do something useful with the data */
            /* We will find the highest peak in a bin, which is not the actual highest
             * peak and frequency we could find in the acquisition.  For an accurate
             * peak search, we can analyze the data with the Spectral Measurements
             * Toolset. */
            for (int i = 0; i < numberOfSpectralLines; ++i)
            {
                if (
                      (i == 0) ||
                      (spectrumDataBlocks[i] > greatestPeakPower)
                   )
                {
                    greatestPeakPower = spectrumDataBlocks[i];
                    greatestPeakFrequency = spectrumInfo.initialFrequency + spectrumInfo.frequencyIncrement * i;
                }
            }
            Console.WriteLine("The highest peak in a bin is {0} dBm at {1} f MHz." + Environment.NewLine, Math.Round(greatestPeakPower, 1), Math.Round(greatestPeakFrequency / 1e6, 3));


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
