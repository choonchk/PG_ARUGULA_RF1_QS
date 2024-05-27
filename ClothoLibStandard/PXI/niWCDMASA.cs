//==================================================================================================
// Title        : Wcdmasa.cs
// Copyright    : National Instruments 2010. All Rights Reserved.
// Purpose      : 
//===================================================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.ModularInstruments.Wcdmasa
{
    public class NIWcdmasa : IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _isNamedSession;

        /// <summary>
        /// Looks up an existing niWCDMA generation session and returns the refnum that you can pass to subsequent niWCDMA generation functions. If the lookup fails, the niWCDMASA_OpenSession function creates a new niWCDMA generation session and returns a new refnum.
        /// 
        /// </summary>
        ///<param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niWCDMAG_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        public NIWcdmasa(int toolkitCompatibilityVersion)
        {
            IntPtr handle;
            int isNewSession;
            int pInvokeResult = PInvoke.niWCDMASA_OpenSession(null, toolkitCompatibilityVersion, out handle, out isNewSession);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            _isNamedSession = false;
        }

        /// <summary>
        /// Looks up an existing niWCDMA generation session and returns the refnum that you can pass to subsequent niWCDMA generation functions. If the lookup fails, the niWCDMASA_OpenSession function creates a new niWCDMA generation session and returns a new refnum.
        /// Make sure you call Close for the named session. Dispose does not close named session.
        /// </summary>
        ///<param>
        /// sessionName
        /// char[]
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. To get the reference to an existing open session, call the niWCDMASA_OpenSession function and specify the same name as an existing open session function in the sessionName parameter.
        ///  You can obtain the reference to an existing session multiple times if you have not called the niWCDMASA_CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string to the sessionName parameter. 
        /// Tip&nbsp;&nbsp;National Instruments recommends that you call the niWCDMASA_CloseSession function for each uniquely-named instance of the niWCDMASA_OpenSession function or each instance of the niWCDMASA_OpenSession function with an unnamed session.
        /// 
        ///</param>
        ///<param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niWCDMASA_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        ///<param>
        /// isNewSession
        /// int32*
        /// Returns TRUE if the function creates a new session. This parameter returns FALSE if the function returns a reference to an existing session.
        /// 
        ///</param>
        ///<param>
        /// session
        /// niWCDMASASession*
        /// Returns the niWCDMA generation session.
        /// 
        ///</param>
        public NIWcdmasa(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            IntPtr handle;
            int pInvokeResult = PInvoke.niWCDMASA_OpenSession(sessionName, toolkitCompatibilityVersion, out handle, out isNewSession);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            if (String.IsNullOrEmpty(sessionName))
                _isNamedSession = false;
            else
                _isNamedSession = true;
        }

        ~NIWcdmasa()
        {
            Dispose(false);
        }

        public HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        /// <summary>
        /// Closes the niWCDMA generation session and releases resources associated with that session.
        /// 
        /// </summary>
        public void Close()
        {
            if (!_isNamedSession)
            {
                Dispose();
            }
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                    PInvoke.niWCDMASA_CloseSession(Handle);
            }
        }

        /// <summary>
        /// Returns the power spectrum trace. The toolkit decides the unit based on the Measurement Results Type attribute.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz).
        /// 
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm or dBm/Hz. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ACPGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, Int32 dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ACPGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Performs CCDF, CDA, ModAcc, constellation EVM, ACP, CHP, OBW, and SEM measurements on the input complex waveform. 
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Specifies the trigger (start) time of the data array.
        /// 
        ///</param>
        ///<param name = "dt">
        /// Specifies the time interval between data points in the data array.
        /// 
        ///</param>
        ///<param name = "data">
        /// Specifies the acquired complex-valued signal. The real and imaginary parts of this complex array correspond to the in-phase (I) and quadrature-phase (Q) data, respectively.
        /// 
        ///</param>
        ///<param name = "numberofSamples">
        /// Specifies the number of complex samples in the data array.
        /// 
        ///</param>
        ///<param name = "reset">
        /// Specifies whether to reset the function. If you set the reset parameter to NIWCDMA_VAL_TRUE, the toolkit overwrites the results of previous measurements.
        /// 
        ///</param>
        ///<param name = "averagingDone">
        /// Indicates whether the function has completed averaging.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int AnalyzeIQComplexF64(double t0, double dt, niComplexNumber[] data, int numberofSamples, bool reset, out int averagingDone)
        {
            int pInvokeResult = PInvoke.niWCDMASA_AnalyzeIQComplexF64(Handle, t0, dt, data, numberofSamples, reset, out averagingDone);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the complimentary cumulative distribution function (CCDF) trace for an ideal Gaussian distribution signal.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "x0">
        /// Returns the starting power level relative to the average power. 
        /// 
        ///</param>
        ///<param name = "dx">
        /// Returns the power interval, in dB. 
        /// 
        ///</param>
        ///<param name = "gaussianProbabilities">
        /// Returns the array of number of samples, in percentage, which lie on or above the corresponding power level in x-axis. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the gaussianProbabilities array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the gaussianProbabilities array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CCDFGetGaussianProbabilitiesTrace(string channelString, out double x0, out double dx, double[] gaussianProbabilities, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CCDFGetGaussianProbabilitiesTrace(Handle, channelString, out x0, out dx, gaussianProbabilities, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the complimentary cumulative distribution function (CCDF) trace for the incoming signal.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "x0">
        /// Returns the starting power level relative to the average power. 
        /// 
        ///</param>
        ///<param name = "dx">
        /// Returns the power interval, in dB. 
        /// 
        ///</param>
        ///<param name = "probabilities">
        /// Returns the array of number of samples, in percentage, which lie on or above the corresponding power level in x-axis. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the probabilities array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the probabilities array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CCDFGetProbabilitiesTrace(string channelString, out double x0, out double dx, double[] probabilities, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CCDFGetProbabilitiesTrace(Handle, channelString, out x0, out dx, probabilities, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the power versus spreading code trace. The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULTS_TYPE attribute.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "x0">
        /// Returns the starting spreading code. 
        /// 
        ///</param>
        ///<param name = "dx">
        /// Returns the width of the spreading code. 
        /// 
        ///</param>
        ///<param name = "codeDomainPower">
        /// Returns the array of code power, in dB or dBm. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the codeDomainPower array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the codeDomainPower array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CDAGetCodeDomainPowerTrace(string channelString, out double x0, out double dx, double[] codeDomainPower, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CDAGetCodeDomainPowerTrace(Handle, channelString, out x0, out dx, codeDomainPower, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the EVM versus time trace of the configured EVM measurement channel.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the symbol duration. 
        /// 
        ///</param>
        ///<param name = "eVM">
        /// Returns the array of EVM, in percentage, of the configured EVM measurement channel. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the EVM array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the EVM array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CDAGetEVMTrace(string channelString, out double t0, out double dt, double[] eVM, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CDAGetEVMTrace(Handle, channelString, out t0, out dt, eVM, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the magnitude error versus time trace of the configured EVM measurement channel.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the symbol duration. 
        /// 
        ///</param>
        ///<param name = "magnitudeError">
        /// Returns the array of magnitude error, in percentage, of the configured EVM measurement channel. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the magnitudeError array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the magnitudeError array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CDAGetMagnitudeErrorTrace(string channelString, out double t0, out double dt, double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CDAGetMagnitudeErrorTrace(Handle, channelString, out t0, out dt, magnitudeError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the phase error versus time trace (PvT) of the configured EVM measurement channel.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the symbol duration. 
        /// 
        ///</param>
        ///<param name = "phaseError">
        /// Returns the array of phase error, in degrees, of the configured EVM measurement channel. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the phaseError array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the phaseError array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CDAGetPhaseErrorTrace(string channelString, out double t0, out double dt, double[] phaseError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CDAGetPhaseErrorTrace(Handle, channelString, out t0, out dt, phaseError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the power versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds. 
        /// 
        ///</param>
        ///<param name = "pvT">
        /// Returns the array of power, in dBm, of the signal. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the PvT array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the PvT array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CDAGetPvTTrace(string channelString, out double t0, out double dt, double[] pvT, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CDAGetPvTTrace(Handle, channelString, out t0, out dt, pvT, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Checks for errors on all configured attributes. If the configuration is invalid, this function returns an error. If there are no errors, the function marks the session as verified.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CheckToolkitError()
        {
            int pInvokeResult = PInvoke.niWCDMASA_CheckToolkitError(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the CHP spectrum trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CHPGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_CHPGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Closes the niWCDMA analysis session and releases resources associated with that session. Call this function once for each uniquely-named session that you have created. 
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int CloseSession()
        {
            int pInvokeResult = PInvoke.niWCDMASA_CloseSession(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Specifies the test model that the toolkit uses to configure the session as defined in the section 6.1 of the 3GPP TS 25.213 Specifications 8.4.0. 
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "testModel">
        /// Specifies the test model that the toolkit uses to configure the session. 
        /// NIWCDMASA_VAL_DL_TEST_MODEL_1_4_DPCH (0)
        /// Specifies test model 1 with 4 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_1_8_DPCH (1)
        /// Specifies test model 1 with 8 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_1_16_DPCH (2)
        /// Specifies test model 1 with 16 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_1_32_DPCH (3)
        /// Specifies test model 1 with 32 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_1_64_DPCH (4)
        /// Specifies test model 1 with 64 DPCH. This value is the default.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_2 (5)
        /// Specifies test model 2.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_3_4_DPCH (6)
        /// Specifies test model 3 with 4 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_3_8_DPCH (7)
        /// Specifies test model 3 with 8 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_3_16_DPCH (8)
        /// Specifies test model 3 with 16 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_3_32_DPCH (9)
        /// Specifies test model 3 with 32 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_4_WITHOUT_CPICH (10)
        /// Specifies test model 4 without CPICH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_4_WITH_CPICH (11)
        /// Specifies test model 4 with CPICH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_5_2_HSPDSCH_6_DPCH (12)
        /// Specifies test model 5 with 2 HSPDSCH and 6 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_5_4_HSPDSCH_4_DPCH (13)
        /// Specifies test model 5 with 4 HSPDSCH and 4 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_5_4_HSPDSCH_14_DPCH (14)
        /// Specifies test model 5 with 4 HSPDSCH and 14 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_5_8_HSPDSCH_30_DPCH (15)
        /// Specifies test model 5 with 8 HSPDSCH and 30 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_6_4_HSPDSCH_4_DPCH (16)
        /// Specifies test model 6 with 4 HSPDSCH and 4 DPCH.
        /// NIWCDMASA_VAL_DL_TEST_MODEL_6_8_HSPDSCH_30_DPCH (17)
        /// Specifies test model 6 with 8 HSPDSCH and 30 DPCH.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ConfigureDownlinkTestModel(string channelString, int testModel)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ConfigureDownlinkTestModel(Handle, channelString, testModel);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the EVM versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration.
        /// 
        ///</param>
        ///<param name = "eVM">
        /// Returns the array of EVM, in percentage. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the EVM array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the EVM array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ConstellationEVMGetCurrentIterationEVMTrace(string channelString, out double t0, out double dt, double[] eVM, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ConstellationEVMGetCurrentIterationEVMTrace(Handle, channelString, out t0, out dt, eVM, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the magnitude error versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration.
        /// 
        ///</param>
        ///<param name = "magnitudeError">
        /// Returns the array of magnitude error, in percentage. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the magnitudeError array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the magnitudeError array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ConstellationEVMGetCurrentIterationMagnitudeErrorTrace(string channelString, out double t0, out double dt, double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ConstellationEVMGetCurrentIterationMagnitudeErrorTrace(Handle, channelString, out t0, out dt, magnitudeError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the phase error versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration.
        /// 
        ///</param>
        ///<param name = "phaseError">
        /// Returns the array of phase error, in percentage. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the phaseError array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the phaseError array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ConstellationEVMGetCurrentIterationPhaseErrorTrace(string channelString, out double t0, out double dt, double[] phaseError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ConstellationEVMGetCurrentIterationPhaseErrorTrace(Handle, channelString, out t0, out dt, phaseError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the recovered IQ versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration.
        /// 
        ///</param>
        ///<param name = "recoveredIQ">
        /// Returns the array of recovered chips. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the recoveredIQ array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the recoveredIQ array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ConstellationEVMGetCurrentIterationRecoveredIQTrace(string channelString, out double t0, out double dt, niComplexNumber[] recoveredIQ, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ConstellationEVMGetCurrentIterationRecoveredIQTrace(Handle, channelString, out t0, out dt, recoveredIQ, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the sample duration.
        /// 
        ///</param>
        ///<param name = "acquiredIQ">
        /// Returns the array of unprocessed IQ signal acquired from the RF signal analyzer. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the acquiredIQ array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the acquiredIQ array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int GetCurrentIterationAcquiredIQTrace(string channelString, out double t0, out double dt, niComplexNumber[] acquiredIQ, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_GetCurrentIterationAcquiredIQTrace(Handle, channelString, out t0, out dt, acquiredIQ, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Takes the error code returned by niWCDMA analysis functions and returns the interpretation as a user-readable string. 
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "errorCode">
        /// Specifies the error code that is returned from any of the niWCDMA analysis functions.
        /// 
        ///</param>
        ///<param name = "errorMessage">
        /// Returns the user-readable message string that corresponds to the error code you specify. The errorMessage buffer must have at least as many elements as are indicated in the errorMessageLength parameter. If you pass NULL to the errorMessage parameter, the function returns the actual length of the error message.
        /// 
        ///</param>
        ///<param name = "errorMessageLength">
        /// Specifies the length of the errorMessage buffer.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int GetErrorString(int errorCode, StringBuilder errorMessage, int errorMessageLength)
        {
            int pInvokeResult = PInvoke.niWCDMASA_GetErrorString(Handle, errorCode, errorMessage, errorMessageLength);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the EVM versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration.
        /// 
        ///</param>
        ///<param name = "eVM">
        /// Returns the array of EVM values, in percentage, of the composite signal. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the EVM array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the EVM array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetEVMTrace(string channelString, out double t0, out double dt, double[] eVM, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ModAccGetEVMTrace(Handle, channelString, out t0, out dt, eVM, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the magnitude error versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration. 
        /// 
        ///</param>
        ///<param name = "magnitudeError">
        /// Returns the array of magnitude error, in percentage, of the composite signal. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the magnitudeError array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the magnitudeError array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetMagnitudeErrorTrace(string channelString, out double t0, out double dt, double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ModAccGetMagnitudeErrorTrace(Handle, channelString, out t0, out dt, magnitudeError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the phase error versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration.
        /// 
        ///</param>
        ///<param name = "phaseError">
        /// Returns the array of phase error, in degrees, of the composite signal. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the phaseError array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the phaseError array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetPhaseErrorTrace(string channelString, out double t0, out double dt, double[] phaseError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ModAccGetPhaseErrorTrace(Handle, channelString, out t0, out dt, phaseError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the recovered IQ versus time trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds. 
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval, in seconds, corresponding to the chip duration. 
        /// 
        ///</param>
        ///<param name = "recoveredIQ">
        /// Returns the array of recovered IQ trace. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the recoveredIQ array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the recoveredIQ array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetRecoveredIQTrace(string channelString, out double t0, out double dt, niComplexNumber[] recoveredIQ, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ModAccGetRecoveredIQTrace(Handle, channelString, out t0, out dt, recoveredIQ, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the power spectrum trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz).
        /// 
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int OBWGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_OBWGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets the attribute specified in the attributeID parameter to its default value. You can reset only a writable attribute using this function.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// If the attribute is channel based, this parameter specifies the channel to which the attribute applies. If the attribute is not channel based, set this parameter to "" (empty string) or NULL. 
        /// 
        ///</param>
        ///<param name = "attributeID">
        /// Specifies the ID of the niWCDMA analysis attribute that you want to reset.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ResetAttribute(string channelString, niWcdmaSaProperties attributeID)
        {
            int pInvokeResult = PInvoke.niWCDMASA_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets all the attributes of the session to their default values.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niWCDMASA_ResetSession(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Examines the incoming signal to calculate the peak power level. This function then returns the estimated power level in the resultantReferenceLevel parameter. Use this function if you need help calculating an approximate setting for the power level for IQ measurements.
        /// This function queries the NIRFSA_ATTR_REFERENCE_LEVEL attribute and uses this value as the starting point for auto level calculations. Set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to the highest expected power level of the signal for faster convergence. For example, if the device under test (DUT) operates in the range of -10 dBm to -30 dBm, set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to -10 dBm.
        /// 
        /// </summary>
        ///<param name = "rFSASession">
        /// Specifies a reference to an NI-RFSA instrument session. This parameter is obtained from the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session. 
        /// 
        ///</param>
        ///<param name = "hardwareChannelString">
        /// Specifies the RFSA device channel. Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "bandwidth">
        /// Specifies the bandwidth, in hertz (Hz), of the signal to be analyzed. The default value is 5 M.
        /// 
        ///</param>
        ///<param name = "measurementInterval">
        /// Specifies the acquisition length, in seconds. The toolkit uses this value to compute the number of samples to acquire from the RF signal analyzer. The default value is 10 m. 
        /// 
        ///</param>
        ///<param name = "maxNumberofIterations">
        /// Specifies the maximum number of iterations to perform when computing the reference level to be set on the RF signal analyzer. The default value is 5. 
        /// 
        ///</param>
        ///<param name = "resultantReferenceLevel">
        /// Returns the estimated power level, in dBm, of the input signal. 
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int RFSAAutoLevel(HandleRef rFSASession, string hardwareChannelString, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel)
        {
            int pInvokeResult = PInvoke.niWCDMASA_RFSAAutoLevel(rFSASession, hardwareChannelString, bandwidth, measurementInterval, maxNumberofIterations, out resultantReferenceLevel);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Retrieves the recommended hardware settings from the niWCDMA analysis session and sets these values to the appropriate niRFSA attributes.
        /// This function sets the following attributes:
        ///     Sets the NIRFSA_ATTR_ACQUISITION_TYPE attribute to NIRFSA_VAL_IQ.
        ///     Sets the NIRFSA_ATTR_NUM_RECORDS_IS_FINITE attribute to VI_TRUE.
        ///     Sets the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.
        ///     Sets the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_SAMPLING_RATE attribute to the NIRFSA_ATTR_IQ_RATE attribute.
        ///     If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to NIRFSA_VAL_IQ_POWER_EDGE, this function sets the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME attribute to the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_MINIMUM_QUIET_TIME attribute. If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to any other value, this function sets the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME attribute to 0.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_ACQUISITION_LENGTH attribute, and sets the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.
        ///     Sets the NIRFSA_ATTR_NUM_SAMPLES_IS_FINITE attribute to VI_TRUE.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_PRETRIGGER_DELAY attribute, and sets the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "wCDMASAChannelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "rFSASession">
        /// Specifies a reference to an NI-RFSA instrument session. This parameter is obtained from the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session. 
        /// 
        ///</param>
        ///<param name = "hardwareChannelString">
        /// Specifies the RFSA device channel. Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "resetHardware">
        /// Specifies whether to reset the NI RF signal analyzer. Set this parameter to NIWCDMASA_VAL_TRUE to reset the hardware.
        /// 
        ///</param>
        ///<param name = "samplesPerRecord">
        /// Returns the number of samples per record configured for the NI-RFSA session.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int RFSAConfigureHardware(string wCDMASAChannelString, HandleRef rFSASession, string hardwareChannelString, out long samplesPerRecord)
        {
            int pInvokeResult = PInvoke.niWCDMASA_RFSAConfigureHardware(Handle, wCDMASAChannelString, rFSASession, hardwareChannelString, out samplesPerRecord);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Enables the measurement that you specify in the measurement parameter and disables all other measurements.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "measurement">
        /// Specifies the measurement to perform. You can specify a list of measurements to perform using OR operator. You can choose from the following measurements.
        /// Note&nbsp;&nbsp;Refer to Simultaneous Measurements for performing combinations of measurements simultaneously.
        /// NIWCDMASA_VAL_ACP_MEASUREMENT (0x00000001)
        /// Enables ACP measurements.
        /// NIWCDMASA_VAL_CCDF_MEASUREMENT (0x00000002)
        /// Enables CCDF measurements.
        /// NIWCDMASA_VAL_CDA_MEASUREMENT (0x00000004)
        /// Enables CDA measurements.
        /// NIWCDMASA_VAL_CHP_MEASUREMENT (0x00000008)
        /// Enables CHP measurements.
        /// NIWCDMASA_VAL_CONSTELLATION_EVM_MEASUREMENT (0x00000010)
        /// Enables constellation EVM measurements.
        /// NIWCDMASA_VAL_MODACC_MEASUREMENT (0x00000020)
        /// Enables ModAcc measurements.
        /// NIWCDMASA_VAL_OBW_MEASUREMENT (0x00000040)
        /// Enables OBW measurements.
        /// NIWCDMASA_VAL_SEM_MEASUREMENT (0x00000080)
        /// Enables SEM measurements.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int SelectMeasurements(uint measurement)
        {
            int pInvokeResult = PInvoke.niWCDMASA_SelectMeasurements(Handle, measurement);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Calls the RFSAConfigureHardware and initiates acquisition on the RF signal 
        /// analyzer and then fetches the waveforms and calls the AnalyzeIQComplexF64 
        /// in a loop n times to perform measurements on the acquired waveforms, 
        /// where n is the value of the Number of Records property.
        /// </summary>
        /// <param name="instrumentHandle">
        ///  Specifies a reference to an NI-RFSA instrument session. This parameter is obtained from 
        ///  the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session.
        /// </param>
        /// <param name="timeOut">
        /// specifies the time allotted, in seconds. The VI passes this value to the timeout parameter of the niRFSA FetchIQSingleRecordComplexF64. The default value is 10
        /// </param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        ///</returns>
        public int RFSAMeasure(HandleRef instrumentHandle, double timeOut)
        {
            int pInvokeResult = PInvoke.niWCDMASA_Measure_v1(Handle, instrumentHandle, timeOut);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the absolute spectral mask limit trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "f0">
        /// Indicates the initial frequency of the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "df">
        /// Indicates the frequency intervals between the data points in the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "absoluteLimits">
        /// Indicates the array of absolute mask limit power values, in dBm. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the absoluteLimits array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the absoluteLimits array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int SEMGetAbsoluteLimitTrace(string channelString, out double f0, out double df, double[] absoluteLimits, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_SEMGetAbsoluteLimitTrace(Handle, channelString, out f0, out df, absoluteLimits, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the relative spectral mask limit trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "f0">
        /// Indicates the initial frequency of the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "df">
        /// Indicates the frequency intervals between the data points in the spectrum, in hertz (Hz).
        /// 
        ///</param>
        ///<param name = "relativeLimits">
        /// Indicates the array of relative mask limit power values, in dBm. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the relativeLimits array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the relativeLimits array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int SEMGetRelativeLimitTrace(string channelString, out double f0, out double df, double[] relativeLimits, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_SEMGetRelativeLimitTrace(Handle, channelString, out f0, out df, relativeLimits, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the frequency domain spectrum trace.
        /// 
        /// </summary>
        ///<param name = "session">
        /// Specifies the niWCDMA analysis session.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz). 
        /// 
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm. 
        /// 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array.
        /// 
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niWLAN generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWLANA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int SEMGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niWCDMASA_SEMGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        ///<summary>
        ///Specifies the array of bandwidths, in hertz (Hz), for adjacent channels for ACP measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [3.84M, 3.84M]. Valid values are 300 to 20M, inclusive.
        /// 
        /// </summary>
        public int SetAcpAdjacentChannelsBandwidth(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.AcpAdjacentChannelsBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the array of bandwidths, in hertz (Hz), for adjacent channels for ACP measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [3.84M, 3.84M]. Valid values are 300 to 20M, inclusive.
        /// 
        /// </summary>
        public int GetAcpAdjacentChannelsBandwidth(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.AcpAdjacentChannelsBandwidth, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of enum values that enables or disables the adjacent channels for ACP measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetAcpAdjacentChannelsEnabled(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.AcpAdjacentChannelsEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies the array of enum values that enables or disables the adjacent channels for ACP measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetAcpAdjacentChannelsEnabled(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.AcpAdjacentChannelsEnabled, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of frequency offsets from the reference channel center frequency, in hertz (Hz), for adjacent channels for ACP measurement.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [5M, 10M]. Valid values are 0 to 50M, inclusive.
        /// 
        /// </summary>
        public int SetAcpAdjacentChannelsFrequencyOffsets(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.AcpAdjacentChannelsFrequencyOffsets, channel, value);
        }
        
		///<summary>
        ///Specifies the array of frequency offsets from the reference channel center frequency, in hertz (Hz), for adjacent channels for ACP measurement.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [5M, 10M]. Valid values are 0 to 50M, inclusive.
        /// 
        /// </summary>
        public int GetAcpAdjacentChannelsFrequencyOffsets(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.AcpAdjacentChannelsFrequencyOffsets, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of sidebands for adjacent channels for ACP measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH.
        /// 
        /// </summary>
        public int SetAcpAdjacentChannelsSidebands(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.AcpAdjacentChannelsSidebands, channel, value);
        }
        
		///<summary>
        ///Specifies the array of sidebands for adjacent channels for ACP measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH.
        /// 
        /// </summary>
        public int GetAcpAdjacentChannelsSidebands(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.AcpAdjacentChannelsSidebands, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for ACP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetAcpAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AcpAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for ACP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetAcpAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AcpAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the method used to average the measurement results.
        ///    The default value is NIWCDMASA_VAL_ACP_AVERAGE_TYPE_LINEAR.
        /// 
        /// </summary>
        public int SetAcpAverageType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AcpAverageType, channel, value);
        }
        
		///<summary>
        ///Specifies the method used to average the measurement results.
        ///    The default value is NIWCDMASA_VAL_ACP_AVERAGE_TYPE_LINEAR.
        /// 
        /// </summary>
        public int GetAcpAverageType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AcpAverageType, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable ACP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetAcpEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AcpEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable ACP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetAcpEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AcpEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the type of the measurement results.
        ///    This attribute is applicable for the following results:
        ///    NIWCDMASA_RESULT_ACP_REFERENCE_CHANNEL_POWER attribute.
        ///    NIWCDMASA_RESULT_ACP_NEGATIVE_ABSOLUTE_POWERS attribute.
        ///    NIWCDMASA_RESULT_ACP_POSITIVE_ABSOLUTE_POWERS attribute.
        ///    The default value is NIWCDMASA_VAL_ACP_MEASUREMENT_RESULTS_TYPE_TOTAL_POWER_REFERENCE.
        /// 
        /// </summary>
        public int SetAcpMeasurementResultsType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AcpMeasurementResultsType, channel, value);
        }
        
		///<summary>
        ///Specifies the type of the measurement results.
        ///    This attribute is applicable for the following results:
        ///    NIWCDMASA_RESULT_ACP_REFERENCE_CHANNEL_POWER attribute.
        ///    NIWCDMASA_RESULT_ACP_NEGATIVE_ABSOLUTE_POWERS attribute.
        ///    NIWCDMASA_RESULT_ACP_POSITIVE_ABSOLUTE_POWERS attribute.
        ///    The default value is NIWCDMASA_VAL_ACP_MEASUREMENT_RESULTS_TYPE_TOTAL_POWER_REFERENCE.
        /// 
        /// </summary>
        public int GetAcpMeasurementResultsType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AcpMeasurementResultsType, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the ACP measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int SetAcpNumberOfAverages(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AcpNumberOfAverages, channel, value);
        }
        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the ACP measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int GetAcpNumberOfAverages(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AcpNumberOfAverages, channel, out value);
        }

        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), of the reference channel.
        ///    The default value is 3.84M. Valid values are 300 to 20M, inclusive.
        /// 
        /// </summary>
        public int SetAcpReferenceChannelBandwidth(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.AcpReferenceChannelBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), of the reference channel.
        ///    The default value is 3.84M. Valid values are 300 to 20M, inclusive.
        /// 
        /// </summary>
        public int GetAcpReferenceChannelBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.AcpReferenceChannelBandwidth, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable automatic detection of physical channels.
        ///    If this attribute is enabled, the toolkit detects all the active physical channels present in the signal being analyzed.    The toolkit returns the spreading code, spreading factor, modulation type, and branch type attributes for each active channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The toolkit detects the channel based on the NIWCDMASA_CHANNEL_DETECTION_POWER_THRESHOLD attribute.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetAutoChannelDetectionEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AutoChannelDetectionEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable automatic detection of physical channels.
        ///    If this attribute is enabled, the toolkit detects all the active physical channels present in the signal being analyzed.    The toolkit returns the spreading code, spreading factor, modulation type, and branch type attributes for each active channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The toolkit detects the channel based on the NIWCDMASA_CHANNEL_DETECTION_POWER_THRESHOLD attribute.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetAutoChannelDetectionEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AutoChannelDetectionEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether the toolkit performs the automatic channel detection based on the fixed threshold or user-specified power threshold.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetAutoPowerThresholdEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.AutoPowerThresholdEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether the toolkit performs the automatic channel detection based on the fixed threshold or user-specified power threshold.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetAutoPowerThresholdEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.AutoPowerThresholdEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the carrier frequency, in hertz (Hz), of the received signal.
        ///    The default value is 1G.
        /// 
        /// </summary>
        public int SetCarrierFrequency(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.CarrierFrequency, channel, value);
        }
        
		///<summary>
        ///Specifies the carrier frequency, in hertz (Hz), of the received signal.
        ///    The default value is 1G.
        /// 
        /// </summary>
        public int GetCarrierFrequency(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.CarrierFrequency, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for CCDF measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetCcdfAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CcdfAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for CCDF measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetCcdfAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CcdfAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable CCDF measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetCcdfEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CcdfEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable CCDF measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetCcdfEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CcdfEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the record length, in seconds. The toolkit accumulates the measurement statistics over multiple records.    This attribute specifies the length of each record.    The toolkit computes the number of records based on the NIWCDMASA_CCDF_RECORD_LENGTH and NIWCDMASA_CCDF_SAMPLE_COUNT attributes.
        ///    The default value is 0.001.
        /// 
        /// </summary>
        public int SetCcdfRecordLength(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.CcdfRecordLength, channel, value);
        }
        
		///<summary>
        ///Specifies the record length, in seconds. The toolkit accumulates the measurement statistics over multiple records.    This attribute specifies the length of each record.    The toolkit computes the number of records based on the NIWCDMASA_CCDF_RECORD_LENGTH and NIWCDMASA_CCDF_SAMPLE_COUNT attributes.
        ///    The default value is 0.001.
        /// 
        /// </summary>
        public int GetCcdfRecordLength(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.CcdfRecordLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the resolution bandwidth (RBW), in hertz (Hz), of the RBW filter used for CCDF measurements.
        ///    The default value is 5M. Valid values are 5M to 10M, inclusive.
        /// 
        /// </summary>
        public int SetCcdfResolutionBandwidth(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.CcdfResolutionBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the resolution bandwidth (RBW), in hertz (Hz), of the RBW filter used for CCDF measurements.
        ///    The default value is 5M. Valid values are 5M to 10M, inclusive.
        /// 
        /// </summary>
        public int GetCcdfResolutionBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.CcdfResolutionBandwidth, channel, out value);
        }

        
		///<summary>
        ///Specifies the type of resolution bandwidth (RBW) filter used for CCDF measurements.
        ///    The default value is NIWCDMASA_VAL_CCDF_RESOLUTION_BANDWIDTH_FILTER_TYPE_FLAT.
        /// 
        /// </summary>
        public int SetCcdfResolutionBandwidthFilterType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CcdfResolutionBandwidthFilterType, channel, value);
        }
        
		///<summary>
        ///Specifies the type of resolution bandwidth (RBW) filter used for CCDF measurements.
        ///    The default value is NIWCDMASA_VAL_CCDF_RESOLUTION_BANDWIDTH_FILTER_TYPE_FLAT.
        /// 
        /// </summary>
        public int GetCcdfResolutionBandwidthFilterType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CcdfResolutionBandwidthFilterType, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of samples the toolkit uses for CCDF measurements.    If the sample count is more than the number of samples in a record, the toolkit performs the measurement over multiple acquisitions,    as determined by the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS attribute.
        ///    The default value is 1M.
        /// 
        /// </summary>
        public int SetCcdfSampleCount(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CcdfSampleCount, channel, value);
        }
        
		///<summary>
        ///Specifies the number of samples the toolkit uses for CCDF measurements.    If the sample count is more than the number of samples in a record, the toolkit performs the measurement over multiple acquisitions,    as determined by the NIWCDMASA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS attribute.
        ///    The default value is 1M.
        /// 
        /// </summary>
        public int GetCcdfSampleCount(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CcdfSampleCount, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for CDA measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetCdaAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for CDA measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetCdaAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the branch on which the active channel is present, to perform the EVM measurement.    The toolkit ignores this attribute if the NIWCDMASA_UUT_TYPE attribute is set to NIWCDMASA_VAL_UUT_TYPE_BS.
        ///    The default value is NIWCDMASA_VAL_CHANNEL_BRANCH_Q.
        /// 
        /// </summary>
        public int SetCdaChannelEvmBranch(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaChannelEvmBranch, channel, value);
        }
        
		///<summary>
        ///Specifies the branch on which the active channel is present, to perform the EVM measurement.    The toolkit ignores this attribute if the NIWCDMASA_UUT_TYPE attribute is set to NIWCDMASA_VAL_UUT_TYPE_BS.
        ///    The default value is NIWCDMASA_VAL_CHANNEL_BRANCH_Q.
        /// 
        /// </summary>
        public int GetCdaChannelEvmBranch(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaChannelEvmBranch, channel, out value);
        }

        
		///<summary>
        ///Specifies the spreading code of one of the active channels on which to perform the EVM measurement.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetCdaChannelEvmSpreadingCode(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaChannelEvmSpreadingCode, channel, value);
        }
        
		///<summary>
        ///Specifies the spreading code of one of the active channels on which to perform the EVM measurement.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetCdaChannelEvmSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaChannelEvmSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Specifies the spreading factor of the active channel to perform the EVM measurement.
        ///    The default value is 256.  
        /// 
        /// </summary>
        public int SetCdaChannelEvmSpreadingFactor(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaChannelEvmSpreadingFactor, channel, value);
        }
        
		///<summary>
        ///Specifies the spreading factor of the active channel to perform the EVM measurement.
        ///    The default value is 256.  
        /// 
        /// </summary>
        public int GetCdaChannelEvmSpreadingFactor(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.CdaChannelEvmSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable CDA measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetCdaEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable CDA measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetCdaEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the duration, in slots, for  CDA measurements.
        ///    The default value is 1. Valid values are 1 to 15, inclusive.
        /// 
        /// </summary>
        public int SetCdaMeasurementLength(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaMeasurementLength, channel, value);
        }
        
		///<summary>
        ///Specifies the duration, in slots, for  CDA measurements.
        ///    The default value is 1. Valid values are 1 to 15, inclusive.
        /// 
        /// </summary>
        public int GetCdaMeasurementLength(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaMeasurementLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the measurement offset, in slots, from the beginning of the frame.    The toolkit uses this as the starting reference for performing the measurement.
        ///    The default value is 0. Valid values are 0 to 14, inclusive.
        /// 
        /// </summary>
        public int SetCdaMeasurementOffset(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaMeasurementOffset, channel, value);
        }
        
		///<summary>
        ///Specifies the measurement offset, in slots, from the beginning of the frame.    The toolkit uses this as the starting reference for performing the measurement.
        ///    The default value is 0. Valid values are 0 to 14, inclusive.
        /// 
        /// </summary>
        public int GetCdaMeasurementOffset(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaMeasurementOffset, channel, out value);
        }

        
		///<summary>
        ///Specifies whether the toolkit returns the applicable results as absolute or relative.
        ///    The default value is NIWCDMASA_VAL_CDA_MEASUREMENT_RESULTS_TYPE_ABSOLUTE.
        /// 
        /// </summary>
        public int SetCdaMeasurementResultsType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaMeasurementResultsType, channel, value);
        }
        
		///<summary>
        ///Specifies whether the toolkit returns the applicable results as absolute or relative.
        ///    The default value is NIWCDMASA_VAL_CDA_MEASUREMENT_RESULTS_TYPE_ABSOLUTE.
        /// 
        /// </summary>
        public int GetCdaMeasurementResultsType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaMeasurementResultsType, channel, out value);
        }

        
		///<summary>
        ///Specifies the type of spectrum of the signal.
        ///    The default value is NIWCDMASA_VAL_SPECTRUM_TYPE_NORMAL.
        /// 
        /// </summary>
        public int SetCdaSpectrumType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.CdaSpectrumType, channel, value);
        }
        
		///<summary>
        ///Specifies the type of spectrum of the signal.
        ///    The default value is NIWCDMASA_VAL_SPECTRUM_TYPE_NORMAL.
        /// 
        /// </summary>
        public int GetCdaSpectrumType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.CdaSpectrumType, channel, out value);
        }

        
		///<summary>
        ///Specifies the power threshold, in dB, for automatic channel detection, relative to the total average power of the signal.
        ///    The toolkit assumes that a particular channel is inactive, if the power of that channel present in the WCDMA signal is less than the threshold power.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is -35.
        /// 
        /// </summary>
        public int SetChannelDetectionPowerThreshold(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ChannelDetectionPowerThreshold, channel, value);
        }
        
		///<summary>
        ///Specifies the power threshold, in dB, for automatic channel detection, relative to the total average power of the signal.
        ///    The toolkit assumes that a particular channel is inactive, if the power of that channel present in the WCDMA signal is less than the threshold power.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is -35.
        /// 
        /// </summary>
        public int GetChannelDetectionPowerThreshold(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ChannelDetectionPowerThreshold, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for CHP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetChpAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ChpAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for CHP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetChpAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ChpAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable CHP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetChpEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ChpEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable CHP measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetChpEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ChpEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), for performing CHP measurements.
        ///    The default value is 3.84M. Valid values are 1,000 to 10M, inclusive.
        /// 
        /// </summary>
        public int SetChpMeasurementBandwidth(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ChpMeasurementBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), for performing CHP measurements.
        ///    The default value is 3.84M. Valid values are 1,000 to 10M, inclusive.
        /// 
        /// </summary>
        public int GetChpMeasurementBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ChpMeasurementBandwidth, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the CHP measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int SetChpNumberOfAverages(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ChpNumberOfAverages, channel, value);
        }
        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the CHP measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int GetChpNumberOfAverages(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ChpNumberOfAverages, channel, out value);
        }

        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition.
        ///    The toolkit returns a warning if span is less than measurement bandwidth, and coerces the span to 1.2 times the measurement bandwidth.
        ///    The default value is 5M. Valid values are 100k to 40M, inclusive.
        /// 
        /// </summary>
        public int SetChpSpan(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ChpSpan, channel, value);
        }
        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition.
        ///    The toolkit returns a warning if span is less than measurement bandwidth, and coerces the span to 1.2 times the measurement bandwidth.
        ///    The default value is 5M. Valid values are 100k to 40M, inclusive.
        /// 
        /// </summary>
        public int GetChpSpan(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ChpSpan, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for constellation EVM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetConstellationEvmAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ConstellationEvmAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for constellation EVM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetConstellationEvmAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ConstellationEvmAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the modulation type of the signal.
        ///    The default value is NIWCDMASA_VAL_EVM_CONSTELLATION_TYPE_QPSK.
        /// 
        /// </summary>
        public int SetConstellationEvmConstellationType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ConstellationEvmConstellationType, channel, value);
        }
        
		///<summary>
        ///Specifies the modulation type of the signal.
        ///    The default value is NIWCDMASA_VAL_EVM_CONSTELLATION_TYPE_QPSK.
        /// 
        /// </summary>
        public int GetConstellationEvmConstellationType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ConstellationEvmConstellationType, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable constellation EVM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetConstellationEvmEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ConstellationEvmEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable constellation EVM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetConstellationEvmEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ConstellationEvmEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable IQ offset removal before performing constellation EVM measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetConstellationEvmIqOffsetRemovalEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ConstellationEvmIqOffsetRemovalEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable IQ offset removal before performing constellation EVM measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetConstellationEvmIqOffsetRemovalEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ConstellationEvmIqOffsetRemovalEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the duration, in chips, for constellation EVM measurements.
        ///    The default value is 2,560. If the NIWCDMASA_CONSTELLATION_EVM_CONSTELLATION_TYPE attribute    is set to NIWCDMASA_VAL_EVM_CONSTELLATION_TYPE_64_QAM, the valid values are 1,200 to 5,120 (inclusive).    In all other cases, the valid values are 128 to 5,120 (inclusive).
        /// 
        /// </summary>
        public int SetConstellationEvmMeasurementLength(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ConstellationEvmMeasurementLength, channel, value);
        }
        
		///<summary>
        ///Specifies the duration, in chips, for constellation EVM measurements.
        ///    The default value is 2,560. If the NIWCDMASA_CONSTELLATION_EVM_CONSTELLATION_TYPE attribute    is set to NIWCDMASA_VAL_EVM_CONSTELLATION_TYPE_64_QAM, the valid values are 1,200 to 5,120 (inclusive).    In all other cases, the valid values are 128 to 5,120 (inclusive).
        /// 
        /// </summary>
        public int GetConstellationEvmMeasurementLength(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ConstellationEvmMeasurementLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages constellation EVM measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int SetConstellationEvmNumberOfAverages(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ConstellationEvmNumberOfAverages, channel, value);
        }
        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages constellation EVM measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int GetConstellationEvmNumberOfAverages(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ConstellationEvmNumberOfAverages, channel, out value);
        }

        
		///<summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter used as a matched filter.
        ///    The default value is 0.22. Valid values are 0 to 0.5, inclusive.
        /// 
        /// </summary>
        public int SetConstellationEvmRrcFilterAlpha(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ConstellationEvmRrcFilterAlpha, channel, value);
        }
        
		///<summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter used as a matched filter.
        ///    The default value is 0.22. Valid values are 0 to 0.5, inclusive.
        /// 
        /// </summary>
        public int GetConstellationEvmRrcFilterAlpha(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ConstellationEvmRrcFilterAlpha, channel, out value);
        }

        
		///<summary>
        ///Specifies the type of spectrum of the signal.
        ///    The default value is NIWCDMASA_VAL_SPECTRUM_TYPE_NORMAL.
        /// 
        /// </summary>
        public int SetDemodulationSpectrumType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DemodulationSpectrumType, channel, value);
        }
        
		///<summary>
        ///Specifies the type of spectrum of the signal.
        ///    The default value is NIWCDMASA_VAL_SPECTRUM_TYPE_NORMAL.
        /// 
        /// </summary>
        public int GetDemodulationSpectrumType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DemodulationSpectrumType, channel, out value);
        }

        
		///<summary>
        ///Specifies the modulation type of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is NIWCDMASA_VAL_DL_CHANNEL_MODULATION_TYPE_QPSK.
        /// 
        /// </summary>
        public int SetDlChannelModulationType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlChannelModulationType, channel, value);
        }
        
		///<summary>
        ///Specifies the modulation type of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is NIWCDMASA_VAL_DL_CHANNEL_MODULATION_TYPE_QPSK.
        /// 
        /// </summary>
        public int GetDlChannelModulationType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlChannelModulationType, channel, out value);
        }

        
		///<summary>
        ///Specifies the spreading code of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0.  Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int SetDlChannelSpreadingCode(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlChannelSpreadingCode, channel, value);
        }
        
		///<summary>
        ///Specifies the spreading code of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0.  Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int GetDlChannelSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlChannelSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Specifies the spreading factor of a physical channel.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 256. The valid values are 4, 8, 16, 32, 64, 128, 256, and 512.
        /// 
        /// </summary>
        public int SetDlChannelSpreadingFactor(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlChannelSpreadingFactor, channel, value);
        }
        
		///<summary>
        ///Specifies the spreading factor of a physical channel.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 256. The valid values are 4, 8, 16, 32, 64, 128, 256, and 512.
        /// 
        /// </summary>
        public int GetDlChannelSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlChannelSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Specifies the primary scrambling code used for descrambling the signal.    Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more details.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 0. Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int SetDlPrimaryScramblingCode(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlPrimaryScramblingCode, channel, value);
        }
        
		///<summary>
        ///Specifies the primary scrambling code used for descrambling the signal.    Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more details.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 0. Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int GetDlPrimaryScramblingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlPrimaryScramblingCode, channel, out value);
        }

        
		///<summary>
        ///Specifies whether the toolkit subtracts SCH from the signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetDlSchSubtractionEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlSchSubtractionEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether the toolkit subtracts SCH from the signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetDlSchSubtractionEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlSchSubtractionEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the code offset.    The toolkit uses the following formula to compute the scrambling code:    scrambling code = ((16 * primary code) + code offset).    Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more details.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int SetDlScramblingCodeOffset(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlScramblingCodeOffset, channel, value);
        }
        
		///<summary>
        ///Specifies the code offset.    The toolkit uses the following formula to compute the scrambling code:    scrambling code = ((16 * primary code) + code offset).    Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more details.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int GetDlScramblingCodeOffset(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlScramblingCodeOffset, channel, out value);
        }

        
		///<summary>
        ///Specifies the downlink scrambling code type used for descrambling the signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.    The sequence generated conforms to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0.
        ///    The default value is NIWCDMASA_VAL_DL_SCRAMBLE_CODE_TYPE_STANDARD.
        /// 
        /// </summary>
        public int SetDlScramblingCodeType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlScramblingCodeType, channel, value);
        }
        
		///<summary>
        ///Specifies the downlink scrambling code type used for descrambling the signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.    The sequence generated conforms to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0.
        ///    The default value is NIWCDMASA_VAL_DL_SCRAMBLE_CODE_TYPE_STANDARD.
        /// 
        /// </summary>
        public int GetDlScramblingCodeType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlScramblingCodeType, channel, out value);
        }

        
		///<summary>
        ///Specifies the synchronization signal length that the toolkit uses for synchronization of a BS signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 2.
        /// 
        /// </summary>
        public int SetDlSynchronizationLength(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlSynchronizationLength, channel, value);
        }
        
		///<summary>
        ///Specifies the synchronization signal length that the toolkit uses for synchronization of a BS signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 2.
        /// 
        /// </summary>
        public int GetDlSynchronizationLength(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlSynchronizationLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the method that the toolkit uses for synchronization.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_DL_SYNCHRONIZATION_TYPE_CPICH.
        /// 
        /// </summary>
        public int SetDlSynchronizationType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.DlSynchronizationType, channel, value);
        }
        
		///<summary>
        ///Specifies the method that the toolkit uses for synchronization.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_DL_SYNCHRONIZATION_TYPE_CPICH.
        /// 
        /// </summary>
        public int GetDlSynchronizationType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.DlSynchronizationType, channel, out value);
        }


        
		///<summary>
        ///Specifies whether to enable all the traces for ModAcc measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetModaccAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ModaccAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for ModAcc measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetModaccAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ModaccAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable ModAcc measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetModaccEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ModaccEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable ModAcc measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetModaccEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ModaccEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable IQ offset removal before performing ModAcc measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetModaccIqOffsetRemovalEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ModaccIqOffsetRemovalEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable IQ offset removal before performing ModAcc measurements.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetModaccIqOffsetRemovalEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ModaccIqOffsetRemovalEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the duration, in slots, for ModAcc measurements.
        ///    The default value is 1. Valid values are 1 to 15, inclusive.
        /// 
        /// </summary>
        public int SetModaccMeasurementLength(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ModaccMeasurementLength, channel, value);
        }
        
		///<summary>
        ///Specifies the duration, in slots, for ModAcc measurements.
        ///    The default value is 1. Valid values are 1 to 15, inclusive.
        /// 
        /// </summary>
        public int GetModaccMeasurementLength(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ModaccMeasurementLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the measurement offset, in slots, from the beginning of the frame.    The toolkit uses this as the starting reference for performing the measurement.
        ///    The default value is 0. Valid values are 0 to 14, inclusive.
        /// 
        /// </summary>
        public int SetModaccMeasurementOffset(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ModaccMeasurementOffset, channel, value);
        }
        
		///<summary>
        ///Specifies the measurement offset, in slots, from the beginning of the frame.    The toolkit uses this as the starting reference for performing the measurement.
        ///    The default value is 0. Valid values are 0 to 14, inclusive.
        /// 
        /// </summary>
        public int GetModaccMeasurementOffset(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ModaccMeasurementOffset, channel, out value);
        }

        
		///<summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter used as a matched filter.
        ///    The default value is 0.22. Valid values are 0 to 0.5, inclusive.
        /// 
        /// </summary>
        public int SetModaccRrcFilterAlpha(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ModaccRrcFilterAlpha, channel, value);
        }
        
		///<summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter used as a matched filter.
        ///    The default value is 0.22. Valid values are 0 to 0.5, inclusive.
        /// 
        /// </summary>
        public int GetModaccRrcFilterAlpha(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ModaccRrcFilterAlpha, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of active channels present in the downlink signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetNumberOfDlChannels(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.NumberOfDlChannels, channel, value);
        }
        
		///<summary>
        ///Specifies the number of active channels present in the downlink signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetNumberOfDlChannels(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.NumberOfDlChannels, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of active channels present in the uplink signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0. 
        /// 
        /// </summary>
        public int SetNumberOfUlChannels(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.NumberOfUlChannels, channel, value);
        }
        
		///<summary>
        ///Specifies the number of active channels present in the uplink signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0. 
        /// 
        /// </summary>
        public int GetNumberOfUlChannels(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.NumberOfUlChannels, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for OBW measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetObwAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ObwAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for OBW measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetObwAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ObwAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable OBW measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetObwEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ObwEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable OBW measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetObwEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ObwEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the time domain window type that is used for spectral smoothing.
        ///    The default value is NIWCDMASA_VAL_OBW_FFT_WINDOW_TYPE_GAUSSIAN. Valid values are 0 to 9, inclusive.
        /// 
        /// </summary>
        public int SetObwFftWindowType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ObwFftWindowType, channel, value);
        }
        
		///<summary>
        ///Specifies the time domain window type that is used for spectral smoothing.
        ///    The default value is NIWCDMASA_VAL_OBW_FFT_WINDOW_TYPE_GAUSSIAN. Valid values are 0 to 9, inclusive.
        /// 
        /// </summary>
        public int GetObwFftWindowType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ObwFftWindowType, channel, out value);
        }

        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the OBW measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int SetObwNumberOfAverages(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ObwNumberOfAverages, channel, value);
        }
        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the OBW measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int GetObwNumberOfAverages(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ObwNumberOfAverages, channel, out value);
        }

        
		///<summary>
        ///Specifies the bandwidth of the RBW filter.
        ///    The default value is 30k. Valid values are 10 to 20M, inclusive.
        /// 
        /// </summary>
        public int SetObwResolutionBandwidth(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ObwResolutionBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the bandwidth of the RBW filter.
        ///    The default value is 30k. Valid values are 10 to 20M, inclusive.
        /// 
        /// </summary>
        public int GetObwResolutionBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ObwResolutionBandwidth, channel, out value);
        }

        
		///<summary>
        ///Specifies the RBW filter type for OBW measurement.
        ///    The default value is NIWCDMASA_VAL_OBW_RESOLUTION_BANDWIDTH_FILTER_TYPE_GAUSSIAN.
        /// 
        /// </summary>
        public int SetObwResolutionBandwidthFilterType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.ObwResolutionBandwidthFilterType, channel, value);
        }
        
		///<summary>
        ///Specifies the RBW filter type for OBW measurement.
        ///    The default value is NIWCDMASA_VAL_OBW_RESOLUTION_BANDWIDTH_FILTER_TYPE_GAUSSIAN.
        /// 
        /// </summary>
        public int GetObwResolutionBandwidthFilterType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ObwResolutionBandwidthFilterType, channel, out value);
        }

        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition.
        ///    The default value is 10M. Valid values are 100k to 40M, inclusive.
        /// 
        /// </summary>
        public int SetObwSpan(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.ObwSpan, channel, value);
        }
        
		///<summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition.
        ///    The default value is 10M. Valid values are 100k to 40M, inclusive.
        /// 
        /// </summary>
        public int GetObwSpan(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ObwSpan, channel, out value);
        }

        
		///<summary>
        ///Returns the length of the record to acquire, in seconds.
        ///    This attribute includes delays due to the measurement filter.    If you do not use the niWCDMASA_RFSAConfigureHardware function, multiply this value by the NIRFSA_ATTR_IQ_RATE attribute    and set the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsAcquisitionLength(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.RecommendedHardwareSettingsAcquisitionLength, channel, out value);
        }

        
		///<summary>
        ///Returns the minimum time, in seconds, during which the signal level must be below the trigger value for triggering to occur.
        ///    If you do not use the niWCDMASA_RFSAConfigureHardware function, pass this attribute to the NIRFSA_ATTR_REF_TRIGGER_MINIMUM_QUIET_TIME attribute.
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsMinimumQuietTime(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.RecommendedHardwareSettingsMinimumQuietTime, channel, out value);
        }

        
		///<summary>
        ///Returns the number of records to acquire.
        ///    The number of records is equal to the maximum of the number of averages of all enabled measurements except CCDF.    The toolkit calculates this attribute for CCDF using the following formula: number of records = ceil(samplex count/(record length * sample rate)).
        ///    If you do not use the niWCDMASA_RFSAConfigureHardware function, pass this attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsNumberOfRecords(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.RecommendedHardwareSettingsNumberOfRecords, channel, out value);
        }

        
		///<summary>
        ///Returns the post-trigger delay, in seconds.
        ///    Use this attribute when the actual signal to be measured is not generated immediately when the trigger occurs but is generated after a delay.
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsPosttriggerDelay(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.RecommendedHardwareSettingsPosttriggerDelay, channel, out value);
        }

        
		///<summary>
        ///Returns the pre-trigger delay, in seconds.
        ///    This attribute is used to acquire data prior to the trigger to account for the delays in the measurement process.    If you do not use the niWCDMASA_RFSAConfigureHardware function, multiply this value by the NIRFSA_ATTR_IQ_RATE attribute and pass    the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsPretriggerDelay(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.RecommendedHardwareSettingsPretriggerDelay, channel, out value);
        }

        
		///<summary>
        ///Returns the sample rate, in hertz (Hz), for the RF signal analyzer.
        ///    If you do not use the niWCDMASA_RFSAConfigureHardware function, pass this attribute to the niRFSA_ConfigureIQRate function.
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsSamplingRate(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.RecommendedHardwareSettingsSamplingRate, channel, out value);
        }

        
		///<summary>
        ///Returns an array of absolute powers, in dBm or dBm/Hz, of the negative sideband adjacent channels.    The toolkit decides the unit based on the NIWCDMASA_ACP_MEASUREMENT_RESULT_TYPE attribute.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        /// 
        /// </summary>
        public int GetResultAcpNegativeAbsolutePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultAcpNegativeAbsolutePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns an array of relative powers, in dB, of the negative sideband adjacent channels. The power is relative to the reference channel power.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        /// 
        /// </summary>
        public int GetResultAcpNegativeRelativePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultAcpNegativeRelativePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns an array of absolute powers, in dBm or dBm/Hz, of the positive sideband adjacent channels.    The toolkit decides the unit based on the NIWCDMASA_ACP_MEASUREMENT_RESULT_TYPE attribute.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        /// 
        /// </summary>
        public int GetResultAcpPositiveAbsolutePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultAcpPositiveAbsolutePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns an array of relative powers, in dB, of the positive sideband adjacent channels.    The power is relative to the reference channel power.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        /// 
        /// </summary>
        public int GetResultAcpPositiveRelativePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultAcpPositiveRelativePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the power, in dBm or dBm/Hz, of the reference channel. The toolkit decides the unit based on the NIWCDMASA_ACP_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultAcpReferenceChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultAcpReferenceChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the signal.
        /// 
        /// </summary>
        public int GetResultCcdfMeanPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfMeanPower, channel, out value);
        }

        
		///<summary>
        ///Returns the number of samples whose instantaneous power is same as the average power of the signal, in percentage.
        /// 
        /// </summary>
        public int GetResultCcdfMeanPowerPercentile(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfMeanPowerPercentile, channel, out value);
        }

        
		///<summary>
        ///Returns the power above the average power, in dB, over which 0.001% of the total samples in the signal are present.
        /// 
        /// </summary>
        public int GetResultCcdfOneHundredThousandthPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfOneHundredThousandthPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power above the average power, in dB, over which 1% of the total samples in the signal are present.
        /// 
        /// </summary>
        public int GetResultCcdfOneHundredthPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfOneHundredthPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power above the average power, in dB, over which 0.0001% of the total samples in the signal are present.
        /// 
        /// </summary>
        public int GetResultCcdfOneMillionthPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfOneMillionthPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power above the average power, in dB, over which 0.01% of the total samples in the signal are present.
        /// 
        /// </summary>
        public int GetResultCcdfOneTenThousandthPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfOneTenThousandthPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power above the average power, in dB, over which 10% of the total samples in the signal are present.
        /// 
        /// </summary>
        public int GetResultCcdfOneTenthPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfOneTenthPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power above the average power, in dB, over which 0.1% of the total samples in the signal are present.
        /// 
        /// </summary>
        public int GetResultCcdfOneThousandthPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfOneThousandthPower, channel, out value);
        }

        
		///<summary>
        ///Returns the peak to average power ratio (PAPR), in dB, of the signal.
        /// 
        /// </summary>
        public int GetResultCcdfPeakToAveragePowerRatio(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCcdfPeakToAveragePowerRatio, channel, out value);
        }

        
		///<summary>
        ///Returns the actual number of samples used for CCDF measurements. This value is equal to number of records * record length * sample rate.
        /// 
        /// </summary>
        public int GetResultCcdfResultantCount(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultCcdfResultantCount, channel, out value);
        }

        
		///<summary>
        ///Returns the timing offset of the configured EVM measurement channel with respect to the start of frame.    You can query this attribute only if the NIWCDMASA_UUT_TYPE attribute is set to NIWCDMASA_VAL_UUT_TYPE_BS and the channel    type is DPCH. The toolkit returns -1 for other channels.
        /// 
        /// </summary>
        public int GetResultCdaChannelDpchTimingOffset(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaChannelDpchTimingOffset, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the configured EVM measurement channel of the signal corresponding to the duration    specified using the NIWCDMASA_CDA_MEASUREMENT_LENGTH and NIWCDMASA_CDA_MEASUREMENT_OFFSET attributes.
        /// 
        /// </summary>
        public int GetResultCdaChannelMeanIntervalCodePower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaChannelMeanIntervalCodePower, channel, out value);
        }

        
		///<summary>
        ///Returns the peak EVM, in percentage, of the configured EVM measurement channel.
        /// 
        /// </summary>
        public int GetResultCdaChannelPeakEvm(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaChannelPeakEvm, channel, out value);
        }

        
		///<summary>
        ///Returns the RMS EVM, in percentage, of the configured EVM measurement channel.
        /// 
        /// </summary>
        public int GetResultCdaChannelRmsEvm(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaChannelRmsEvm, channel, out value);
        }

        
		///<summary>
        ///Returns the RMS magnitude error, in percentage, of the configured EVM measurement channel.
        /// 
        /// </summary>
        public int GetResultCdaChannelRmsMagnitudeError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaChannelRmsMagnitudeError, channel, out value);
        }

        
		///<summary>
        ///Returns the RMS phase error, in degrees, of the configured EVM measurement channel.
        /// 
        /// </summary>
        public int GetResultCdaChannelRmsPhaseError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaChannelRmsPhaseError, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the common pilot channel (CPICH).
        /// 
        /// </summary>
        public int GetResultCdaCpichPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaCpichPower, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum power, in dB or dBm, among all the active channels. The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMaximumDlActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMaximumDlActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum power, in dB or dBm, among all the inactive channels. The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMaximumDlInactiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMaximumDlInactiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum power, in dB or dBm, among all the active channels on the I branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMaximumIBranchActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMaximumIBranchActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum power, in dB or dBm, among all the inactive channels on the I branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMaximumIBranchInactiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMaximumIBranchInactiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum power, in dB or dBm, among all the active channels on the Q branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMaximumQBranchActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMaximumQBranchActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum power, in dB or dBm, among all the inactive channels on the Q branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMaximumQBranchInactiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMaximumQBranchInactiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dB or dBm, of all the active channels. The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMeanDlActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMeanDlActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average Power, in dB or dBm, of all the inactive channels. The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMeanDlInactiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMeanDlInactiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dB or dBm, of all the active channels on the I branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMeanIBranchActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMeanIBranchActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dB or dBm, of all the inactive channels on the I branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMeanIBranchInactiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMeanIBranchInactiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dB or dBm, of all the active channels on the Q branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMeanQBranchActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMeanQBranchActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dB or dBm, of all the inactive channels on the Q branch.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaMeanQBranchInactiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaMeanQBranchInactiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the primary synchronization channel (P-SCH).
        /// 
        /// </summary>
        public int GetResultCdaPschPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaPschPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the secondary synchronization channel (S-SCH).
        /// 
        /// </summary>
        public int GetResultCdaSschPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaSschPower, channel, out value);
        }

        
		///<summary>
        ///Returns the sum of the average powers, in dB or dBm, of all the active channels.    The toolkit decides the unit based on the NIWCDMASA_CDA_MEASUREMENT_RESULT_TYPE attribute.
        /// 
        /// </summary>
        public int GetResultCdaTotalActiveChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaTotalActiveChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the total average power, in dBm, of the signal corresponding to the duration specified using the    NIWCDMASA_CDA_MEASUREMENT_LENGTH and NIWCDMASA_CDA_MEASUREMENT_OFFSET attributes.
        /// 
        /// </summary>
        public int GetResultCdaTotalIntervalPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaTotalIntervalPower, channel, out value);
        }

        
		///<summary>
        ///Returns the total average power, in dBm, of the signal.
        /// 
        /// </summary>
        public int GetResultCdaTotalPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaTotalPower, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the DPCCH channel.
        /// 
        /// </summary>
        public int GetResultCdaUlDpcchPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultCdaUlDpcchPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power, in dBm, of the channel.
        /// 
        /// </summary>
        public int GetResultChpChannelPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultChpChannelPower, channel, out value);
        }

        
		///<summary>
        ///Returns the power spectral density, in dBm/Hz, of the channel.
        /// 
        /// </summary>
        public int GetResultChpChannelPowerSpectralDensity(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultChpChannelPowerSpectralDensity, channel, out value);
        }

        
		///<summary>
        ///Returns the average of frequency offset measurements, in hertz (Hz).    The toolkit averages the frequency offset measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmFrequencyError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmFrequencyError, channel, out value);
        }

        
		///<summary>
        ///Returns the average of IQ origin offset measurements, in dB.    The toolkit averages the IQ origin offset measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmIqOriginOffset(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmIqOriginOffset, channel, out value);
        }

        
		///<summary>
        ///Returns the average of magnitude error measurements, in percentage.    The toolkit averages the magnitude error measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmMagnitudeError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmMagnitudeError, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum of frequency offset measurements, in hertz (Hz).    The toolkit returns the maximum of the frequency offset measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmMaximumFrequencyError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmMaximumFrequencyError, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum of IQ origin offset measurements, in dB.    The toolkit returns the maximum of the IQ origin offset measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmMaximumIqOriginOffset(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmMaximumIqOriginOffset, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum of magnitude error measurements, in percentage.    The toolkit returns the maximum of the magnitude error measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmMaximumMagnitudeError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmMaximumMagnitudeError, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum of phase error measurements, in degrees.    The toolkit returns the maximum of the phase error measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmMaximumPhaseError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmMaximumPhaseError, channel, out value);
        }

        
		///<summary>
        ///Returns the maximum of peak EVM measurements, in percentage.    The toolkit returns the maximum of the peak EVM measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmPeakEvm(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmPeakEvm, channel, out value);
        }

        
		///<summary>
        ///Returns the average of phase error measurements, in degrees.    The toolkit averages the phase error measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmPhaseError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmPhaseError, channel, out value);
        }

        
		///<summary>
        ///Returns the average of RMS EVM measurements, in percentage.    The toolkit averages the RMS EVM measurement over the number of acquisitions specified by the NIWCDMASA_CONSTELLATION_EVM_NUMBER_OF_AVERAGES attribute.
        /// 
        /// </summary>
        public int GetResultConstellationEvmRmsEvm(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultConstellationEvmRmsEvm, channel, out value);
        }

        
		///<summary>
        ///Returns the modulation type of the detected downlink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedDlChannelModulationType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedDlChannelModulationType, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading code of the detected downlink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedDlChannelSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedDlChannelSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading factor of the detected downlink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedDlChannelSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedDlChannelSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Returns the branch type of the detected uplink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedUlChannelBranch(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedUlChannelBranch, channel, out value);
        }

        
		///<summary>
        ///Returns the modulation type of the detected uplink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedUlChannelModulationType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedUlChannelModulationType, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading code of the detected uplink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedUlChannelSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedUlChannelSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading factor of the detected uplink channel.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    Use an active channel string to read this attribute.
        /// 
        /// </summary>
        public int GetResultDetectedUlChannelSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultDetectedUlChannelSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Returns the frequency offset, in hertz (Hz), of the composite signal.
        /// 
        /// </summary>
        public int GetResultModaccFrequencyError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccFrequencyError, channel, out value);
        }

        
		///<summary>
        ///Returns the IQ origin offset, in dB, of the composite signal.
        /// 
        /// </summary>
        public int GetResultModaccIqOffset(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccIqOffset, channel, out value);
        }

        
		///<summary>
        ///Returns the RMS magnitude error, in percentage, of the composite signal.
        /// 
        /// </summary>
        public int GetResultModaccMagnitudeError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccMagnitudeError, channel, out value);
        }

        
		///<summary>
        ///Returns the peak among the code domain errors, in dB, of all the active channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakActiveCde(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccPeakActiveCde, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading code of the channel. The channel has a peak code domain error of all the active channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakActiveCdeSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultModaccPeakActiveCdeSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading factor of the channel. The channel has a peak code domain error of all the active channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakActiveCdeSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultModaccPeakActiveCdeSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Returns the peak among the code domain errors, in dB, of all the active channels and inactive channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakCde(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccPeakCde, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading code of the channel. The channel has a peak code domain error of all the active and inactive channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakCdeSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultModaccPeakCdeSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading factor of the channel. The channel has a peak code domain error of all the active and inactive channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakCdeSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultModaccPeakCdeSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Returns the peak EVM, in percentage, of the composite signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakEvm(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccPeakEvm, channel, out value);
        }

        
		///<summary>
        ///Returns the peak among the relative code domain errors, in dB, of all the active channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakRelativeCde(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccPeakRelativeCde, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading code of the channel. The channel has a peak code domain error of all the active channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakRelativeCdeSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultModaccPeakRelativeCdeSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Returns the spreading factor of the channel. The channel has a peak code domain error of all the active channels in the signal.
        /// 
        /// </summary>
        public int GetResultModaccPeakRelativeCdeSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultModaccPeakRelativeCdeSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Returns the RMS phase error, in degrees, of the composite signal.
        /// 
        /// </summary>
        public int GetResultModaccPhaseError(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccPhaseError, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the primary synchronization channel (P-SCH).
        /// 
        /// </summary>
        public int GetResultModaccPschPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccPschPower, channel, out value);
        }

        
		///<summary>
        ///Returns the RMS EVM, in percentage, of the composite signal.
        /// 
        /// </summary>
        public int GetResultModaccRmsEvm(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccRmsEvm, channel, out value);
        }

        
		///<summary>
        ///Returns the average power, in dBm, of the secondary synchronization channel (S-SCH).
        /// 
        /// </summary>
        public int GetResultModaccSschPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultModaccSschPower, channel, out value);
        }

        
		///<summary>
        ///Returns the number of active channels detected in the downlink signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        /// 
        /// </summary>
        public int GetResultNumberOfDetectedDlChannels(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultNumberOfDetectedDlChannels, channel, out value);
        }

        
		///<summary>
        ///Returns the number of active channels detected in the uplink signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        /// 
        /// </summary>
        public int GetResultNumberOfDetectedUlChannels(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultNumberOfDetectedUlChannels, channel, out value);
        }

        
		///<summary>
        ///Returns the power, in dBm, of the RBW filtered signal integrated over the span.
        /// 
        /// </summary>
        public int GetResultObwCarrierPower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultObwCarrierPower, channel, out value);
        }

        
		///<summary>
        ///Returns the OBW of the signal, in hertz (Hz).   This value is the frequency range containing the percentage of power specified.
        /// 
        /// </summary>
        public int GetResultObwOccupiedBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultObwOccupiedBandwidth, channel, out value);
        }

        
		///<summary>
        ///Returns the lower-bound frequency, in hertz (Hz), of the OBW measurements.
        /// 
        /// </summary>
        public int GetResultObwStartFrequency(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultObwStartFrequency, channel, out value);
        }

        
		///<summary>
        ///Returns the upper-bound frequency, in hertz (Hz), of the OBW measurements.
        /// 
        /// </summary>
        public int GetResultObwStopFrequency(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultObwStopFrequency, channel, out value);
        }

        
		///<summary>
        ///Returns the status for the SEM measurement based on the user-configured measurement limits.
        /// 
        /// </summary>
        public int GetResultSemMeasurementStatus(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ResultSemMeasurementStatus, channel, out value);
        }

        
		///<summary>
        ///Returns the array of frequencies, in hertz (Hz), corresponding to each negative side band peak power. 
        /// 
        /// </summary>
        public int GetResultSemNegativePeakFrequencies(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemNegativePeakFrequencies, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the array of power margins, in dB, for each negative side band.    The power margin is relative to either absolute or relative spectral mask based on the Mask States attribute.   This value indicates the minimum difference between the spectral mask and the acquired spectrum.
        /// 
        /// </summary>
        public int GetResultSemNegativePowerMargins(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemNegativePowerMargins, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the array of negative side band peak powers, in dBc, relative to the reference power, for each offset band.
        /// 
        /// </summary>
        public int GetResultSemNegativeRelativePeakPowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemNegativeRelativePeakPowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the array of negative side band powers, in dBc, relative to the reference power, for each offset band.
        /// 
        /// </summary>
        public int GetResultSemNegativeRelativePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemNegativeRelativePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the frequency, in hertz (Hz), corresponding to the peak power in the reference channel.
        /// 
        /// </summary>
        public int GetResultSemPeakReferenceFrequency(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultSemPeakReferenceFrequency, channel, out value);
        }

        
		///<summary>
        ///Returns the array of frequencies, in hertz (Hz), corresponding to each positive side band peak power.
        /// 
        /// </summary>
        public int GetResultSemPositivePeakFrequencies(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemPositivePeakFrequencies, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the array of power margins, in dB, for each positive side band.    The power margin is relative to either absolute or relative spectral mask based on the Mask States attribute.   This value indicates the minimum difference between the spectral mask and the acquired spectrum.
        /// 
        /// </summary>
        public int GetResultSemPositivePowerMargins(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemPositivePowerMargins, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the array of positive side band peak powers, in dBc, relative to the reference power, for each offset band.
        /// 
        /// </summary>
        public int GetResultSemPositiveRelativePeakPowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemPositiveRelativePeakPowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the array of positive side band powers, in dBc, relative to the reference power, for each offset band.
        /// 
        /// </summary>
        public int GetResultSemPositiveRelativePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.ResultSemPositiveRelativePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Returns the integrated power, in dBm, of the reference channel for the specified integration bandwidth.
        /// 
        /// </summary>
        public int GetResultSemReferencePower(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.ResultSemReferencePower, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable all the traces for SEM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetSemAllTracesEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.SemAllTracesEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable all the traces for SEM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetSemAllTracesEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.SemAllTracesEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the averaging type for SEM measurements.
        ///    The default value is NIWCDMASA_VAL_SEM_AVERAGE_TYPE_LINEAR.
        /// 
        /// </summary>
        public int SetSemAverageType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.SemAverageType, channel, value);
        }
        
		///<summary>
        ///Specifies the averaging type for SEM measurements.
        ///    The default value is NIWCDMASA_VAL_SEM_AVERAGE_TYPE_LINEAR.
        /// 
        /// </summary>
        public int GetSemAverageType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.SemAverageType, channel, out value);
        }

        
		///<summary>
        ///Specifies whether to enable SEM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int SetSemEnabled(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.SemEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies whether to enable SEM measurements.
        ///    The default value is NIWCDMASA_VAL_FALSE.
        /// 
        /// </summary>
        public int GetSemEnabled(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.SemEnabled, channel, out value);
        }

        
		///<summary>
        ///Specifies the measurement length, in seconds, for SEM measurements.
        ///    The default value is 0.001.
        /// 
        /// </summary>
        public int SetSemMeasurementLength(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.SemMeasurementLength, channel, value);
        }
        
		///<summary>
        ///Specifies the measurement length, in seconds, for SEM measurements.
        ///    The default value is 0.001.
        /// 
        /// </summary>
        public int GetSemMeasurementLength(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.SemMeasurementLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the array of mask states for each offset band for Measurement Status computation.   The default value is NIWCDMASA_VAL_SEM_MEASUREMENT_LIMITS_MASK_STATES_RELATIVE.
        /// 
        /// </summary>
        public int SetSemMeasurementLimitsMaskStates(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.SemMeasurementLimitsMaskStates, channel, value);
        }
        
		///<summary>
        ///Specifies the array of mask states for each offset band for Measurement Status computation.   The default value is NIWCDMASA_VAL_SEM_MEASUREMENT_LIMITS_MASK_STATES_RELATIVE.
        /// 
        /// </summary>
        public int GetSemMeasurementLimitsMaskStates(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.SemMeasurementLimitsMaskStates, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of absolute power levels, in dBm, at the beginning of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-12.5, -12.5, -24.5, -11.5, -11.5].
        /// 
        /// </summary>
        public int SetSemMeasurementLimitsStartAbsolutePowers(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStartAbsolutePowers, channel, value);
        }
        
		///<summary>
        ///Specifies the array of absolute power levels, in dBm, at the beginning of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-12.5, -12.5, -24.5, -11.5, -11.5].
        /// 
        /// </summary>
        public int GetSemMeasurementLimitsStartAbsolutePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStartAbsolutePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of relative power levels, in dBc, at the beginning of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-30, -30, -30, -30, -30].
        /// 
        /// </summary>
        public int SetSemMeasurementLimitsStartRelativePowers(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStartRelativePowers, channel, value);
        }
        
		///<summary>
        ///Specifies the array of relative power levels, in dBc, at the beginning of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-30, -30, -30, -30, -30].
        /// 
        /// </summary>
        public int GetSemMeasurementLimitsStartRelativePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStartRelativePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of absolute power levels, in dBm, at the end of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-12.5, -24.5, -24.5, -11.5, -11.5].
        /// 
        /// </summary>
        public int SetSemMeasurementLimitsStopAbsolutePowers(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStopAbsolutePowers, channel, value);
        }
        
		///<summary>
        ///Specifies the array of absolute power levels, in dBm, at the end of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-12.5, -24.5, -24.5, -11.5, -11.5].
        /// 
        /// </summary>
        public int GetSemMeasurementLimitsStopAbsolutePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStopAbsolutePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of relative power levels, in dBc, at the end of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-30, -30, -30, -30, -30].
        /// 
        /// </summary>
        public int SetSemMeasurementLimitsStopRelativePowers(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStopRelativePowers, channel, value);
        }
        
		///<summary>
        ///Specifies the array of relative power levels, in dBc, at the end of each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [-30, -30, -30, -30, -30].
        /// 
        /// </summary>
        public int GetSemMeasurementLimitsStopRelativePowers(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemMeasurementLimitsStopRelativePowers, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the SEM measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int SetSemNumberOfAverages(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.SemNumberOfAverages, channel, value);
        }
        
		///<summary>
        ///Specifies the number of acquisitions over which the toolkit averages the SEM measurements.
        ///    The default value is 1. Valid values are 1 to 1,000 (inclusive).
        /// 
        /// </summary>
        public int GetSemNumberOfAverages(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.SemNumberOfAverages, channel, out value);
        }

        
		///<summary>
        ///Specifies the value (k) that the toolkit uses to compute measurement filter bandwidth. The measurement bandwidth is equal    to (k-1)*RBW. If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based    on the UUT type. The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [1, 1, 1, 20, 1]. Valid values are 1 to 80, inclusive.
        /// 
        /// </summary>
        public int SetSemOffsetBandsBandwidthIntegrals(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.SemOffsetBandsBandwidthIntegrals, channel, value);
        }
        
		///<summary>
        ///Specifies the value (k) that the toolkit uses to compute measurement filter bandwidth. The measurement bandwidth is equal    to (k-1)*RBW. If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based    on the UUT type. The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [1, 1, 1, 20, 1]. Valid values are 1 to 80, inclusive.
        /// 
        /// </summary>
        public int GetSemOffsetBandsBandwidthIntegrals(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.SemOffsetBandsBandwidthIntegrals, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of enum values that enables or disables the offset bands for SEM measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int SetSemOffsetBandsEnabled(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.SemOffsetBandsEnabled, channel, value);
        }
        
		///<summary>
        ///Specifies the array of enum values that enables or disables the offset bands for SEM measurements.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_TRUE.
        /// 
        /// </summary>
        public int GetSemOffsetBandsEnabled(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.SemOffsetBandsEnabled, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of offset sides for each offset band.  If none of the offset bands attributes are configured,    the toolkit dynamically chooses the default values based on the UUT type. The toolkit returns an error if the array sizes of all    the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_SEM_OFFSET_BANDS_OFFSET_SIDES_BOTH.
        /// 
        /// </summary>
        public int SetSemOffsetBandsOffsetSides(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.SemOffsetBandsOffsetSides, channel, value);
        }
        
		///<summary>
        ///Specifies the array of offset sides for each offset band.  If none of the offset bands attributes are configured,    the toolkit dynamically chooses the default values based on the UUT type. The toolkit returns an error if the array sizes of all    the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_SEM_OFFSET_BANDS_OFFSET_SIDES_BOTH.
        /// 
        /// </summary>
        public int GetSemOffsetBandsOffsetSides(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.SemOffsetBandsOffsetSides, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of resolution bandwidths, in hertz (Hz), for each offset band.    The toolkit ignores this attribute if the NIWCDMASA_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE attribute is    set to NIWCDMASA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_AUTO.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [30k, 30k, 30k, 50k, 1M]. Valid values are 1,000 to 5M, inclusive.
        /// 
        /// </summary>
        public int SetSemOffsetBandsResolutionBandwidths(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemOffsetBandsResolutionBandwidths, channel, value);
        }
        
		///<summary>
        ///Specifies the array of resolution bandwidths, in hertz (Hz), for each offset band.    The toolkit ignores this attribute if the NIWCDMASA_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE attribute is    set to NIWCDMASA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_AUTO.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [30k, 30k, 30k, 50k, 1M]. Valid values are 1,000 to 5M, inclusive.
        /// 
        /// </summary>
        public int GetSemOffsetBandsResolutionBandwidths(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemOffsetBandsResolutionBandwidths, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of RBW states for each offset band. If none of the offset bands attributes are configured,    the toolkit dynamically chooses the default values based on the UUT type. The toolkit returns an error if the array    sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_MANUAL.
        /// 
        /// </summary>
        public int SetSemOffsetBandsResolutionBandwidthsState(string channel, int[] value)
        {
            return SetVectorInt(niWcdmaSaProperties.SemOffsetBandsResolutionBandwidthsState, channel, value);
        }
        
		///<summary>
        ///Specifies the array of RBW states for each offset band. If none of the offset bands attributes are configured,    the toolkit dynamically chooses the default values based on the UUT type. The toolkit returns an error if the array    sizes of all the offset bands attributes are not same.
        ///    The default value is NIWCDMASA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_MANUAL.
        /// 
        /// </summary>
        public int GetSemOffsetBandsResolutionBandwidthsState(string channel, int[] values, out int actualNumOfElements)
        {
            return GetVectorInt(niWcdmaSaProperties.SemOffsetBandsResolutionBandwidthsState, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of starting frequency offsets from the center frequency of the reference channel, in hertz (Hz), for each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [2.51M, 2.71M, 3.51M, 4M, 8M]. Valid values are 0 to 40M, inclusive.
        /// 
        /// </summary>
        public int SetSemOffsetBandsStartOffsetFrequencies(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemOffsetBandsStartOffsetFrequencies, channel, value);
        }
        
		///<summary>
        ///Specifies the array of starting frequency offsets from the center frequency of the reference channel, in hertz (Hz), for each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [2.51M, 2.71M, 3.51M, 4M, 8M]. Valid values are 0 to 40M, inclusive.
        /// 
        /// </summary>
        public int GetSemOffsetBandsStartOffsetFrequencies(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemOffsetBandsStartOffsetFrequencies, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the array of end frequency offsets from the center frequency of the reference channel, in hertz (Hz), for each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [2.71M, 3.51M, 4M, 8M, 12.5M]. Valid values are 0 to 40M, inclusive.
        /// 
        /// </summary>
        public int SetSemOffsetBandsStopOffsetFrequencies(string channel, double[] value)
        {
            return SetVectorDouble(niWcdmaSaProperties.SemOffsetBandsStopOffsetFrequencies, channel, value);
        }
        
		///<summary>
        ///Specifies the array of end frequency offsets from the center frequency of the reference channel, in hertz (Hz), for each offset band.    If none of the offset bands attributes are configured, the toolkit dynamically chooses the default values based on the UUT type.    The toolkit returns an error if the array sizes of all the offset bands attributes are not same.
        ///    The default value is [2.71M, 3.51M, 4M, 8M, 12.5M]. Valid values are 0 to 40M, inclusive.
        /// 
        /// </summary>
        public int GetSemOffsetBandsStopOffsetFrequencies(string channel, double[] values, out int actualNumOfElements)
        {
            return GetVectorDouble(niWcdmaSaProperties.SemOffsetBandsStopOffsetFrequencies, channel, values, out actualNumOfElements);
        }

        
		///<summary>
        ///Specifies the integration bandwidth, in hertz (Hz), for the reference channel.
        ///    The default value is 3.84M. Valid values are 100k to 40M, inclusive.
        /// 
        /// </summary>
        public int SetSemReferenceChannelIntegrationBandwidth(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.SemReferenceChannelIntegrationBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the integration bandwidth, in hertz (Hz), for the reference channel.
        ///    The default value is 3.84M. Valid values are 100k to 40M, inclusive.
        /// 
        /// </summary>
        public int GetSemReferenceChannelIntegrationBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.SemReferenceChannelIntegrationBandwidth, channel, out value);
        }

        
		///<summary>
        ///Specifies the RBW, in hertz (Hz), for the reference channel.    The toolkit ignores this attribute if the NIWCDMASA_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE attribute    is NIWCDMASA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_AUTO.
        ///    The default value is 30k. Valid values are 1,000 to 5M, inclusive.
        /// 
        /// </summary>
        public int SetSemReferenceChannelResolutionBandwidth(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.SemReferenceChannelResolutionBandwidth, channel, value);
        }
        
		///<summary>
        ///Specifies the RBW, in hertz (Hz), for the reference channel.    The toolkit ignores this attribute if the NIWCDMASA_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE attribute    is NIWCDMASA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_AUTO.
        ///    The default value is 30k. Valid values are 1,000 to 5M, inclusive.
        /// 
        /// </summary>
        public int GetSemReferenceChannelResolutionBandwidth(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.SemReferenceChannelResolutionBandwidth, channel, out value);
        }

        
		///<summary>
        ///Specifies the RBW state for the reference channel.
        ///    The default value is NIWCDMASA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_MANUAL.
        /// 
        /// </summary>
        public int SetSemReferenceChannelResolutionBandwidthState(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.SemReferenceChannelResolutionBandwidthState, channel, value);
        }
        
		///<summary>
        ///Specifies the RBW state for the reference channel.
        ///    The default value is NIWCDMASA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_MANUAL.
        /// 
        /// </summary>
        public int GetSemReferenceChannelResolutionBandwidthState(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.SemReferenceChannelResolutionBandwidthState, channel, out value);
        }

        
		///<summary>
        ///Indicates the WCDMA Analysis Toolkit version in use.
        /// 
        /// </summary>
        public int GetToolkitCompatibilityVersion(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.ToolkitCompatibilityVersion, channel, out value);
        }

        
		///<summary>
        ///Specifies the additional time, in seconds, to acquire data before the trigger occurs.    The toolkit uses this attribute to compute the pre-trigger and post-trigger delays.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetTriggerDelay(string channel, double value)
        {
            return SetScalarDouble(niWcdmaSaProperties.TriggerDelay, channel, value);
        }
        
		///<summary>
        ///Specifies the additional time, in seconds, to acquire data before the trigger occurs.    The toolkit uses this attribute to compute the pre-trigger and post-trigger delays.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetTriggerDelay(string channel, out double value)
        {
            return GetScalarDouble(niWcdmaSaProperties.TriggerDelay, channel, out value);
        }

        
		///<summary>
        ///Specifies the branch type of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is NIWCDMASA_VAL_CHANNEL_BRANCH_Q.
        /// 
        /// </summary>
        public int SetUlChannelBranch(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlChannelBranch, channel, value);
        }
        
		///<summary>
        ///Specifies the branch type of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is NIWCDMASA_VAL_CHANNEL_BRANCH_Q.
        /// 
        /// </summary>
        public int GetUlChannelBranch(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlChannelBranch, channel, out value);
        }

        
		///<summary>
        ///Specifies the modulation type of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is NIWCDMASA_VAL_UL_CHANNEL_MODULATION_TYPE_BPSK.
        /// 
        /// </summary>
        public int SetUlChannelModulationType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlChannelModulationType, channel, value);
        }
        
		///<summary>
        ///Specifies the modulation type of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is NIWCDMASA_VAL_UL_CHANNEL_MODULATION_TYPE_BPSK.
        /// 
        /// </summary>
        public int GetUlChannelModulationType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlChannelModulationType, channel, out value);
        }

        
		///<summary>
        ///Specifies the spreading code of a physical Channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0. Valid values are 0 to 255, inclusive.
        /// 
        /// </summary>
        public int SetUlChannelSpreadingCode(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlChannelSpreadingCode, channel, value);
        }
        
		///<summary>
        ///Specifies the spreading code of a physical Channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 0. Valid values are 0 to 255, inclusive.
        /// 
        /// </summary>
        public int GetUlChannelSpreadingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlChannelSpreadingCode, channel, out value);
        }

        
		///<summary>
        ///Specifies the spreading factor of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 256. The valid values are 2, 4, 8, 16, 32, 64, 128, and 256.
        /// 
        /// </summary>
        public int SetUlChannelSpreadingFactor(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlChannelSpreadingFactor, channel, value);
        }
        
		///<summary>
        ///Specifies the spreading factor of a physical channel.    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The toolkit uses this attribute only for ModAcc and CDA measurements.    The toolkit ignores this attribute if the NIWCDMASA_AUTO_CHANNEL_DETECTION_ENABLED attribute is set to NIWCDMASA_VAL_TRUE.
        ///    The default value is 256. The valid values are 2, 4, 8, 16, 32, 64, 128, and 256.
        /// 
        /// </summary>
        public int GetUlChannelSpreadingFactor(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlChannelSpreadingFactor, channel, out value);
        }

        
		///<summary>
        ///Specifies the slot format of the dedicated physical control channel (DPCCH) used for the synchronization of the uplink signal.    Refer to section 5.2.1.1 of the 3GPP TS 25.211 Specifications 8.4.0 for more details.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_UL_DPCCH_SLOT_FORMAT_0.
        /// 
        /// </summary>
        public int SetUlDpcchSlotFormat(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlDpcchSlotFormat, channel, value);
        }
        
		///<summary>
        ///Specifies the slot format of the dedicated physical control channel (DPCCH) used for the synchronization of the uplink signal.    Refer to section 5.2.1.1 of the 3GPP TS 25.211 Specifications 8.4.0 for more details.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_UL_DPCCH_SLOT_FORMAT_0.
        /// 
        /// </summary>
        public int GetUlDpcchSlotFormat(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlDpcchSlotFormat, channel, out value);
        }

        
		///<summary>
        ///Specifies the uplink scrambling code number.    Refer to section 4.3.2.5 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetUlScramblingCode(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlScramblingCode, channel, value);
        }
        
		///<summary>
        ///Specifies the uplink scrambling code number.    Refer to section 4.3.2.5 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetUlScramblingCode(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlScramblingCode, channel, out value);
        }

        
		///<summary>
        ///Specifies the uplink scrambling code type used for descrambling the signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_UL_SCRAMBLE_CODE_TYPE_LONG.
        /// 
        /// </summary>
        public int SetUlScramblingCodeType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlScramblingCodeType, channel, value);
        }
        
		///<summary>
        ///Specifies the uplink scrambling code type used for descrambling the signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is NIWCDMASA_VAL_UL_SCRAMBLE_CODE_TYPE_LONG.
        /// 
        /// </summary>
        public int GetUlScramblingCodeType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlScramblingCodeType, channel, out value);
        }

        
		///<summary>
        ///Specifies the synchronization signal length that the toolkit uses for synchronization of a UE signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 2.
        /// 
        /// </summary>
        public int SetUlSynchronizationLength(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UlSynchronizationLength, channel, value);
        }
        
		///<summary>
        ///Specifies the synchronization signal length that the toolkit uses for synchronization of a UE signal.    The toolkit uses this attribute only for ModAcc and CDA measurements.
        ///    The default value is 2.
        /// 
        /// </summary>
        public int GetUlSynchronizationLength(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UlSynchronizationLength, channel, out value);
        }

        
		///<summary>
        ///Specifies the type of unit under test (UUT) generating the signal that the toolkit analyzes.
        ///    The default value is NIWCDMASA_VAL_UUT_TYPE_UE.
        /// 
        /// </summary>
        public int SetUutType(string channel, int value)
        {
            return SetScalarInt(niWcdmaSaProperties.UutType, channel, value);
        }
        
		///<summary>
        ///Specifies the type of unit under test (UUT) generating the signal that the toolkit analyzes.
        ///    The default value is NIWCDMASA_VAL_UUT_TYPE_UE.
        /// 
        /// </summary>
        public int GetUutType(string channel, out int value)
        {
            return GetScalarInt(niWcdmaSaProperties.UutType, channel, out value);
        }

        private int SetScalarInt(niWcdmaSaProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            return TestForError(PInvoke.niWCDMASA_SetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetScalarInt(niWcdmaSaProperties propertyId, string repeatedCapabilityOrChannel, out int val)
        {
            return TestForError(PInvoke.niWCDMASA_GetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetScalarDouble(niWcdmaSaProperties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            return TestForError(PInvoke.niWCDMASA_SetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetScalarDouble(niWcdmaSaProperties propertyId, string repeatedCapabilityOrChannel, out double val)
        {
            return TestForError(PInvoke.niWCDMASA_GetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetVectorDouble(niWcdmaSaProperties attributeID, string repeatedCapabilityOrChannel, double[] dataArray)
        {
            return TestForError(PInvoke.niWCDMASA_SetVectorAttributeF64(Handle, repeatedCapabilityOrChannel, attributeID, dataArray, dataArray.Length));
        }

        private int GetVectorDouble(niWcdmaSaProperties attributeID, string repeatedCapabilityOrChannel, double[] dataArray, out int actualNumDataArrayElements)
        {
            return TestForError(PInvoke.niWCDMASA_GetVectorAttributeF64(Handle, repeatedCapabilityOrChannel, attributeID, dataArray, dataArray.Length, out actualNumDataArrayElements));
        }

        private int SetVectorInt(niWcdmaSaProperties attributeID, string repeatedCapabilityOrChannel, int[] dataArray)
        {
            return TestForError(PInvoke.niWCDMASA_SetVectorAttributeI32(Handle, repeatedCapabilityOrChannel, attributeID, dataArray, dataArray.Length));
        }

        private int GetVectorInt(niWcdmaSaProperties attributeID, string repeatedCapabilityOrChannel, int[] dataArray, out int actualNumDataArrayElements)
        {
            return TestForError(PInvoke.niWCDMASA_GetVectorAttributeI32(Handle, repeatedCapabilityOrChannel, attributeID, dataArray, dataArray.Length, out actualNumDataArrayElements));
        }

        private class PInvoke
        {
            const string nativeDllName = "niWCDMASA_net.dll";

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ACPGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ACPGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_AnalyzeIQComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_AnalyzeIQComplexF64(HandleRef session, double t0, double dt, niComplexNumber[] data, int numberofSamples, bool reset, out int averagingDone);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CCDFGetGaussianProbabilitiesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CCDFGetGaussianProbabilitiesTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] gaussianProbabilities, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CCDFGetProbabilitiesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CCDFGetProbabilitiesTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] probabilities, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CDAGetCodeDomainPowerTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CDAGetCodeDomainPowerTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] codeDomainPower, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CDAGetEVMTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CDAGetEVMTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] eVM, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CDAGetMagnitudeErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CDAGetMagnitudeErrorTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CDAGetPhaseErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CDAGetPhaseErrorTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] phaseError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CDAGetPvTTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CDAGetPvTTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] pvT, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CheckToolkitError", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CheckToolkitError(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CHPGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CHPGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_CloseSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ConfigureDownlinkTestModel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ConfigureDownlinkTestModel(HandleRef session, string channelString, int testModel);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ConstellationEVMGetCurrentIterationEVMTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ConstellationEVMGetCurrentIterationEVMTrace(HandleRef session, string channelString, out double t0, out double dt, [Out]double[] eVM, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ConstellationEVMGetCurrentIterationMagnitudeErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ConstellationEVMGetCurrentIterationMagnitudeErrorTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ConstellationEVMGetCurrentIterationPhaseErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ConstellationEVMGetCurrentIterationPhaseErrorTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] phaseError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ConstellationEVMGetCurrentIterationRecoveredIQTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ConstellationEVMGetCurrentIterationRecoveredIQTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] niComplexNumber[] recoveredIQ, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_GetCurrentIterationAcquiredIQTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_GetCurrentIterationAcquiredIQTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQ, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_GetErrorString(HandleRef session, int errorCode, StringBuilder errorMessage, int errorMessageLength);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_GetScalarAttributeF64(HandleRef session, string channelString, niWcdmaSaProperties attributeID, out double value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_GetScalarAttributeI32(HandleRef session, string channelString, niWcdmaSaProperties attributeID, out int value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_GetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_GetVectorAttributeF64(HandleRef session, string channelString, niWcdmaSaProperties attributeID, [Out] double[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_GetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_GetVectorAttributeI32(HandleRef session, string channelString, niWcdmaSaProperties attributeID, [Out] int[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ModAccGetEVMTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ModAccGetEVMTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] eVM, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ModAccGetMagnitudeErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ModAccGetMagnitudeErrorTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ModAccGetPhaseErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ModAccGetPhaseErrorTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] double[] phaseError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ModAccGetRecoveredIQTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ModAccGetRecoveredIQTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] niComplexNumber[] recoveredIQ, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_OBWGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_OBWGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_OpenSession(string sessionName, int toolkitCompatibilityVersion, out IntPtr session, out int isNewSession);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ResetAttribute(HandleRef session, string channelString, niWcdmaSaProperties attributeID);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_ResetSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_RFSAAutoLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_RFSAAutoLevel(HandleRef rFSASession, string hardwareChannelString, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_RFSAConfigureHardware", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_RFSAConfigureHardware(HandleRef session, string wCDMASAChannelString, System.Runtime.InteropServices.HandleRef rFSASession, string hardwareChannelString, out long samplesPerRecord);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SelectMeasurements", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SelectMeasurements(HandleRef session, uint measurement);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SEMGetAbsoluteLimitTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SEMGetAbsoluteLimitTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] absoluteLimits, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SEMGetRelativeLimitTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SEMGetRelativeLimitTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] relativeLimits, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SEMGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SEMGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SetScalarAttributeF64(HandleRef session, string channelString, niWcdmaSaProperties attributeID, double value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SetScalarAttributeI32(HandleRef session, string channelString, niWcdmaSaProperties attributeID, int value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SetVectorAttributeF64(HandleRef session, string channelString, niWcdmaSaProperties attributeID, [In] double[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_SetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_SetVectorAttributeI32(HandleRef session, string channelString, niWcdmaSaProperties attributeID, [In] int[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASA_Measure_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASA_Measure_v1(HandleRef session, HandleRef instrumentHandle, double timeout);
        }

        private int TestForError(int status)
        {
            if (status < 0)
            {
                StringBuilder msg = new StringBuilder();
                status = GetErrorString(status, msg);
                throw new ExternalException(msg.ToString(), status);
            }
            return status;
        }

        private int TestForError(int status, HandleRef rfsaHandle)
        {
            if (status < 0)
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                GetErrorString(status, msg);
                // get RFSA detailed error message.
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSA.GetError(rfsaHandle, status, msg);
                //get RFSA general error message.
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSA.ErrorMessage(rfsaHandle, status, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }

        private int GetErrorString(int status, StringBuilder msg)
        {
            int size = PInvoke.niWCDMASA_GetErrorString(Handle, status, null, 0);
            if ((size >= 0))
            {
                msg.Capacity = size;
                PInvoke.niWCDMASA_GetErrorString(Handle, status, msg, size);
            }
            return status;
        }

        #region IDisposible members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
            }
            if (!_isNamedSession)
            {
                //Dispose unmanaged resources
                if (!Handle.Handle.Equals(IntPtr.Zero))
                {
                    PInvoke.niWCDMASA_CloseSession(Handle);
                }
            }
        }

        #endregion

    }

    public class niWcdmasaConstants
    {
        public const int UutTypeBs = 0;

        public const int UutTypeUe = 1;

        public const int False = 0;

        public const int True = 1;

        public const int DlScrambleCodeTypeStandard = 0;

        public const int DlScrambleCodeTypeLeft = 1;

        public const int DlScrambleCodeTypeRight = 2;

        public const int DlChannelModulationTypeQpsk = 0;

        public const int DlChannelModulationType16Qam = 1;

        public const int DlChannelModulationType64Qam = 2;

        public const int DlSynchronizationTypeCpich = 0;

        public const int DlSynchronizationTypeSch = 1;

        public const int UlScrambleCodeTypeLong = 0;

        public const int UlScrambleCodeTypeShort = 1;

        public const int UlChannelModulationTypeBpsk = 0;

        public const int UlChannelModulationType4Pam = 1;

        public const int ChannelBranchI = 0;

        public const int ChannelBranchQ = 1;

        public const int UlDpcchSlotFormat0 = 0;

        public const int UlDpcchSlotFormat1 = 1;

        public const int UlDpcchSlotFormat2 = 2;

        public const int UlDpcchSlotFormat3 = 3;

        public const int UlDpcchSlotFormat4 = 4;

        public const int CdaMeasurementResultsTypeAbsolute = 0;

        public const int CdaMeasurementResultsTypeRelative = 1;

        public const int SpectrumTypeNormal = 0;

        public const int SpectrumTypeInverted = 1;

        public const int CcdfResolutionBandwidthFilterTypeGaussian = 0;

        public const int CcdfResolutionBandwidthFilterTypeFlat = 1;

        public const int CcdfResolutionBandwidthFilterTypeNone = 2;

        public const int EvmConstellationTypeQpsk = 0;

        public const int EvmConstellationType16Qam = 1;

        public const int EvmConstellationType64Qam = 2;

        public const int AcpAverageTypeLinear = 0;

        public const int AcpAverageTypePeakHold = 1;

        public const int AcpMeasurementResultsTypeTotalPowerReference = 0;

        public const int AcpMeasurementResultsTypePowerSpectralDensityReference = 1;

        public const int ObwFftWindowTypeFlatTop = 0;

        public const int ObwFftWindowTypeUniform = 1;

        public const int ObwFftWindowTypeHanning = 2;

        public const int ObwFftWindowTypeHamming = 3;

        public const int ObwFftWindowTypeGaussian = 4;

        public const int ObwFftWindowTypeBlackman = 5;

        public const int ObwFftWindowTypeBlackmanHarris = 6;

        public const int ObwFftWindowTypeKaiserBessel70Db = 7;

        public const int ObwFftWindowTypeKaiserBessel90Db = 8;

        public const int ObwFftWindowTypeKaiserBessel110Db = 9;

        public const int ObwResolutionBandwidthFilterTypeGaussian = 0;

        public const int ObwResolutionBandwidthFilterTypeFlat = 1;

        public const int ObwResolutionBandwidthFilterTypeNone = 2;

        public const int SemAverageTypeLinear = 0;

        public const int SemAverageTypePeakHold = 1;

        public const int SemReferenceChannelResolutionBandwidthStateManual = 0;

        public const int SemReferenceChannelResolutionBandwidthStateAuto = 1;

        public const int Pass = 0;

        public const int Fail = 1;

        public const int ToolkitCompatibilityVersion010000 = 10000;

        public const int AcpAdjacentChannelsSidebandsNegative = 0;

        public const int AcpAdjacentChannelsSidebandsPositive = 1;

        public const int AcpAdjacentChannelsSidebandsBoth = 2;

        public const int SemOffsetBandsResolutionBandwidthsStateManual = 0;

        public const int SemOffsetBandsResolutionBandwidthsStateAuto = 1;

        public const int SemOffsetBandsOffsetSidesNegative = 0;

        public const int SemOffsetBandsOffsetSidesPositive = 1;

        public const int SemOffsetBandsOffsetSidesBoth = 2;

        public const int SemMeasurementLimitsMaskStatesAbsolute = 0;

        public const int SemMeasurementLimitsMaskStatesRelative = 1;

        public const int AcpMeasurement = 1;

        public const int CcdfMeasurement = 2;

        public const int CdaMeasurement = 4;

        public const int ChpMeasurement = 8;

        public const int ConstellationEvmMeasurement = 16;

        public const int ModaccMeasurement = 32;

        public const int ObwMeasurement = 64;

        public const int SemMeasurement = 128;
    }

    public enum niWcdmaSaProperties
    {
        /// <summary>
        /// double
        /// </summary>
        AcpAdjacentChannelsBandwidth = 8033,

        /// <summary>
        /// int
        /// </summary>
        AcpAdjacentChannelsEnabled = 8011,

        /// <summary>
        /// double
        /// </summary>
        AcpAdjacentChannelsFrequencyOffsets = 8010,

        /// <summary>
        /// int
        /// </summary>
        AcpAdjacentChannelsSidebands = 8012,

        /// <summary>
        /// int
        /// </summary>
        AcpAllTracesEnabled = 8006,

        /// <summary>
        /// int
        /// </summary>
        AcpAverageType = 8005,

        /// <summary>
        /// int
        /// </summary>
        AcpEnabled = 8001,

        /// <summary>
        /// int
        /// </summary>
        AcpMeasurementResultsType = 8009,

        /// <summary>
        /// int
        /// </summary>
        AcpNumberOfAverages = 8003,

        /// <summary>
        /// double
        /// </summary>
        AcpReferenceChannelBandwidth = 8022,

        /// <summary>
        /// int
        /// </summary>
        AutoChannelDetectionEnabled = 5,

        /// <summary>
        /// int
        /// </summary>
        AutoPowerThresholdEnabled = 50,

        /// <summary>
        /// double
        /// </summary>
        CarrierFrequency = 2,

        /// <summary>
        /// int
        /// </summary>
        CcdfAllTracesEnabled = 2004,

        /// <summary>
        /// int
        /// </summary>
        CcdfEnabled = 2001,

        /// <summary>
        /// double
        /// </summary>
        CcdfRecordLength = 2002,

        /// <summary>
        /// double
        /// </summary>
        CcdfResolutionBandwidth = 2026,

        /// <summary>
        /// int
        /// </summary>
        CcdfResolutionBandwidthFilterType = 2025,

        /// <summary>
        /// int
        /// </summary>
        CcdfSampleCount = 2003,

        /// <summary>
        /// int
        /// </summary>
        CdaAllTracesEnabled = 1008,

        /// <summary>
        /// int
        /// </summary>
        CdaChannelEvmBranch = 24,

        /// <summary>
        /// int
        /// </summary>
        CdaChannelEvmSpreadingCode = 1005,

        /// <summary>
        /// double
        /// </summary>
        CdaChannelEvmSpreadingFactor = 1006,

        /// <summary>
        /// int
        /// </summary>
        CdaEnabled = 1001,

        /// <summary>
        /// int
        /// </summary>
        CdaMeasurementLength = 1003,

        /// <summary>
        /// int
        /// </summary>
        CdaMeasurementOffset = 1002,

        /// <summary>
        /// int
        /// </summary>
        CdaMeasurementResultsType = 1004,

        /// <summary>
        /// int
        /// </summary>
        CdaSpectrumType = 1007,

        /// <summary>
        /// double
        /// </summary>
        ChannelDetectionPowerThreshold = 6,

        /// <summary>
        /// int
        /// </summary>
        ChpAllTracesEnabled = 7007,

        /// <summary>
        /// int
        /// </summary>
        ChpEnabled = 7002,

        /// <summary>
        /// double
        /// </summary>
        ChpMeasurementBandwidth = 7020,

        /// <summary>
        /// int
        /// </summary>
        ChpNumberOfAverages = 7004,

        /// <summary>
        /// double
        /// </summary>
        ChpSpan = 7015,

        /// <summary>
        /// int
        /// </summary>
        ConstellationEvmAllTracesEnabled = 5008,

        /// <summary>
        /// int
        /// </summary>
        ConstellationEvmConstellationType = 5030,

        /// <summary>
        /// int
        /// </summary>
        ConstellationEvmEnabled = 5002,

        /// <summary>
        /// int
        /// </summary>
        ConstellationEvmIqOffsetRemovalEnabled = 5006,

        /// <summary>
        /// int
        /// </summary>
        ConstellationEvmMeasurementLength = 5005,

        /// <summary>
        /// int
        /// </summary>
        ConstellationEvmNumberOfAverages = 5004,

        /// <summary>
        /// double
        /// </summary>
        ConstellationEvmRrcFilterAlpha = 5007,

        /// <summary>
        /// int
        /// </summary>
        DemodulationSpectrumType = 5001,

        /// <summary>
        /// int
        /// </summary>
        DlChannelModulationType = 13,

        /// <summary>
        /// int
        /// </summary>
        DlChannelSpreadingCode = 12,

        /// <summary>
        /// int
        /// </summary>
        DlChannelSpreadingFactor = 11,

        /// <summary>
        /// int
        /// </summary>
        DlPrimaryScramblingCode = 8,

        /// <summary>
        /// int
        /// </summary>
        DlSchSubtractionEnabled = 17,

        /// <summary>
        /// int
        /// </summary>
        DlScramblingCodeOffset = 9,

        /// <summary>
        /// int
        /// </summary>
        DlScramblingCodeType = 7,

        /// <summary>
        /// int
        /// </summary>
        DlSynchronizationLength = 37,

        /// <summary>
        /// int
        /// </summary>
        DlSynchronizationType = 14,

        /// <summary>
        /// int
        /// </summary>
        ModaccAllTracesEnabled = 6005,

        /// <summary>
        /// int
        /// </summary>
        ModaccEnabled = 6001,

        /// <summary>
        /// int
        /// </summary>
        ModaccIqOffsetRemovalEnabled = 6006,

        /// <summary>
        /// int
        /// </summary>
        ModaccMeasurementLength = 6003,

        /// <summary>
        /// int
        /// </summary>
        ModaccMeasurementOffset = 6002,

        /// <summary>
        /// double
        /// </summary>
        ModaccRrcFilterAlpha = 6004,

        /// <summary>
        /// int
        /// </summary>
        NumberOfDlChannels = 10,

        /// <summary>
        /// int
        /// </summary>
        NumberOfUlChannels = 20,

        /// <summary>
        /// int
        /// </summary>
        ObwAllTracesEnabled = 11008,

        /// <summary>
        /// int
        /// </summary>
        ObwEnabled = 11001,

        /// <summary>
        /// int
        /// </summary>
        ObwFftWindowType = 11004,

        /// <summary>
        /// int
        /// </summary>
        ObwNumberOfAverages = 11003,

        /// <summary>
        /// double
        /// </summary>
        ObwResolutionBandwidth = 11006,

        /// <summary>
        /// int
        /// </summary>
        ObwResolutionBandwidthFilterType = 11005,

        /// <summary>
        /// double
        /// </summary>
        ObwSpan = 11016,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsAcquisitionLength = 13002,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsMinimumQuietTime = 13008,

        /// <summary>
        /// int
        /// </summary>
        RecommendedHardwareSettingsNumberOfRecords = 13001,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsPosttriggerDelay = 13007,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsPretriggerDelay = 13004,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsSamplingRate = 13003,

        /// <summary>
        /// double
        /// </summary>
        ResultAcpNegativeAbsolutePowers = 8034,

        /// <summary>
        /// double
        /// </summary>
        ResultAcpNegativeRelativePowers = 8037,

        /// <summary>
        /// double
        /// </summary>
        ResultAcpPositiveAbsolutePowers = 8036,

        /// <summary>
        /// double
        /// </summary>
        ResultAcpPositiveRelativePowers = 8035,

        /// <summary>
        /// double
        /// </summary>
        ResultAcpReferenceChannelPower = 8007,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfMeanPower = 2006,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfMeanPowerPercentile = 2007,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneHundredThousandthPower = 2012,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneHundredthPower = 2009,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneMillionthPower = 2013,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneTenThousandthPower = 2011,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneTenthPower = 2008,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneThousandthPower = 2010,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfPeakToAveragePowerRatio = 2005,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfResultantCount = 2014,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaChannelDpchTimingOffset = 1017,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaChannelMeanIntervalCodePower = 1016,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaChannelPeakEvm = 1013,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaChannelRmsEvm = 1012,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaChannelRmsMagnitudeError = 1014,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaChannelRmsPhaseError = 1015,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaCpichPower = 1020,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMaximumDlActiveChannelPower = 1022,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMaximumDlInactiveChannelPower = 1024,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMaximumIBranchActiveChannelPower = 1027,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMaximumIBranchInactiveChannelPower = 1029,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMaximumQBranchActiveChannelPower = 1031,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMaximumQBranchInactiveChannelPower = 1033,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMeanDlActiveChannelPower = 1021,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMeanDlInactiveChannelPower = 1023,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMeanIBranchActiveChannelPower = 1026,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMeanIBranchInactiveChannelPower = 1028,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMeanQBranchActiveChannelPower = 1030,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaMeanQBranchInactiveChannelPower = 1032,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaPschPower = 1018,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaSschPower = 1019,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaTotalActiveChannelPower = 1010,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaTotalIntervalPower = 1011,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaTotalPower = 1009,

        /// <summary>
        /// double
        /// </summary>
        ResultCdaUlDpcchPower = 1025,

        /// <summary>
        /// double
        /// </summary>
        ResultChpChannelPower = 7008,

        /// <summary>
        /// double
        /// </summary>
        ResultChpChannelPowerSpectralDensity = 7009,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmFrequencyError = 5020,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmIqOriginOffset = 5022,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmMagnitudeError = 5016,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmMaximumFrequencyError = 5021,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmMaximumIqOriginOffset = 5023,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmMaximumMagnitudeError = 5017,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmMaximumPhaseError = 5019,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmPeakEvm = 5015,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmPhaseError = 5018,

        /// <summary>
        /// double
        /// </summary>
        ResultConstellationEvmRmsEvm = 5012,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedDlChannelModulationType = 29,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedDlChannelSpreadingCode = 28,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedDlChannelSpreadingFactor = 27,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedUlChannelBranch = 34,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedUlChannelModulationType = 33,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedUlChannelSpreadingCode = 32,

        /// <summary>
        /// int
        /// </summary>
        ResultDetectedUlChannelSpreadingFactor = 31,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccFrequencyError = 6015,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccIqOffset = 6014,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMagnitudeError = 6012,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccPeakActiveCde = 6020,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccPeakActiveCdeSpreadingCode = 6022,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccPeakActiveCdeSpreadingFactor = 6021,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccPeakCde = 6017,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccPeakCdeSpreadingCode = 6019,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccPeakCdeSpreadingFactor = 6018,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccPeakEvm = 6011,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccPeakRelativeCde = 6023,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccPeakRelativeCdeSpreadingCode = 6025,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccPeakRelativeCdeSpreadingFactor = 6024,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccPhaseError = 6013,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccPschPower = 6026,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccRmsEvm = 6010,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccSschPower = 6027,

        /// <summary>
        /// int
        /// </summary>
        ResultNumberOfDetectedDlChannels = 26,

        /// <summary>
        /// int
        /// </summary>
        ResultNumberOfDetectedUlChannels = 30,

        /// <summary>
        /// double
        /// </summary>
        ResultObwCarrierPower = 11010,

        /// <summary>
        /// double
        /// </summary>
        ResultObwOccupiedBandwidth = 11009,

        /// <summary>
        /// double
        /// </summary>
        ResultObwStartFrequency = 11012,

        /// <summary>
        /// double
        /// </summary>
        ResultObwStopFrequency = 11011,

        /// <summary>
        /// int
        /// </summary>
        ResultSemMeasurementStatus = 12015,

        /// <summary>
        /// double
        /// </summary>
        ResultSemNegativePeakFrequencies = 12027,

        /// <summary>
        /// double
        /// </summary>
        ResultSemNegativePowerMargins = 12025,

        /// <summary>
        /// double
        /// </summary>
        ResultSemNegativeRelativePeakPowers = 12021,

        /// <summary>
        /// double
        /// </summary>
        ResultSemNegativeRelativePowers = 12017,

        /// <summary>
        /// double
        /// </summary>
        ResultSemPeakReferenceFrequency = 12055,

        /// <summary>
        /// double
        /// </summary>
        ResultSemPositivePeakFrequencies = 12026,

        /// <summary>
        /// double
        /// </summary>
        ResultSemPositivePowerMargins = 12024,

        /// <summary>
        /// double
        /// </summary>
        ResultSemPositiveRelativePeakPowers = 12020,

        /// <summary>
        /// double
        /// </summary>
        ResultSemPositiveRelativePowers = 12016,

        /// <summary>
        /// double
        /// </summary>
        ResultSemReferencePower = 12054,

        /// <summary>
        /// int
        /// </summary>
        SemAllTracesEnabled = 12013,

        /// <summary>
        /// int
        /// </summary>
        SemAverageType = 12003,

        /// <summary>
        /// int
        /// </summary>
        SemEnabled = 12001,

        /// <summary>
        /// double
        /// </summary>
        SemMeasurementLength = 12002,

        /// <summary>
        /// int
        /// </summary>
        SemMeasurementLimitsMaskStates = 12052,

        /// <summary>
        /// double
        /// </summary>
        SemMeasurementLimitsStartAbsolutePowers = 12008,

        /// <summary>
        /// double
        /// </summary>
        SemMeasurementLimitsStartRelativePowers = 12006,

        /// <summary>
        /// double
        /// </summary>
        SemMeasurementLimitsStopAbsolutePowers = 12009,

        /// <summary>
        /// double
        /// </summary>
        SemMeasurementLimitsStopRelativePowers = 12007,

        /// <summary>
        /// int
        /// </summary>
        SemNumberOfAverages = 12050,

        /// <summary>
        /// int
        /// </summary>
        SemOffsetBandsBandwidthIntegrals = 12051,

        /// <summary>
        /// int
        /// </summary>
        SemOffsetBandsEnabled = 12045,

        /// <summary>
        /// int
        /// </summary>
        SemOffsetBandsOffsetSides = 12049,

        /// <summary>
        /// double
        /// </summary>
        SemOffsetBandsResolutionBandwidths = 12011,

        /// <summary>
        /// int
        /// </summary>
        SemOffsetBandsResolutionBandwidthsState = 12047,

        /// <summary>
        /// double
        /// </summary>
        SemOffsetBandsStartOffsetFrequencies = 12004,

        /// <summary>
        /// double
        /// </summary>
        SemOffsetBandsStopOffsetFrequencies = 12005,

        /// <summary>
        /// double
        /// </summary>
        SemReferenceChannelIntegrationBandwidth = 12040,

        /// <summary>
        /// double
        /// </summary>
        SemReferenceChannelResolutionBandwidth = 12044,

        /// <summary>
        /// int
        /// </summary>
        SemReferenceChannelResolutionBandwidthState = 12043,

        /// <summary>
        /// int
        /// </summary>
        ToolkitCompatibilityVersion = 13009,

        /// <summary>
        /// double
        /// </summary>
        TriggerDelay = 13005,

        /// <summary>
        /// int
        /// </summary>
        UlChannelBranch = 36,

        /// <summary>
        /// int
        /// </summary>
        UlChannelModulationType = 23,

        /// <summary>
        /// int
        /// </summary>
        UlChannelSpreadingCode = 22,

        /// <summary>
        /// int
        /// </summary>
        UlChannelSpreadingFactor = 21,

        /// <summary>
        /// int
        /// </summary>
        UlDpcchSlotFormat = 25,

        /// <summary>
        /// int
        /// </summary>
        UlScramblingCode = 19,

        /// <summary>
        /// int
        /// </summary>
        UlScramblingCodeType = 18,

        /// <summary>
        /// int
        /// </summary>
        UlSynchronizationLength = 38,

        /// <summary>
        /// int
        /// </summary>
        UutType = 4,
    }
}