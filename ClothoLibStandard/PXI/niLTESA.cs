
//==========================================================================
// .NET library for NI-LTE Analysis toolkit.
//--------------------------------------------------------------------------
// Copyright (c) National Instruments 2012.  All Rights Reserved.			
//--------------------------------------------------------------------------
// Title:	niLTESA.cs
// Purpose: C# wrapper for NI-LTE Signal Analysis 2.0 toolkit.
//==========================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.ModularInstruments.Ltesa
{
    public class niLTESA : IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _isNamedSession;

        ~niLTESA()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns a reference to a new or existing niLTESA analysis session.
        /// </summary>
        ///<param name = "sessionName">
        /// sessionName
        /// char[]
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. If you want to get reference to an already opened session x, specify x as the session name. You can obtain a reference to an already existing session multiple times if you have not called the niLTESA_CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string or NULL to the sessionName parameter.
        /// Tip  National Instruments recommends that you call the niLTESA_CloseSession function for each uniquely-named instance of the niLTESA_OpenSession function or each instance of the niLTESA_OpenSession function with an unnamed session.
        ///</param>
        ///<param name = "toolkitCompatibilityVersion">
        /// compatibilityVersion
        /// int32
        /// Specifies the toolkit compatibility version. 
        ///                         NILTESA_VAL_COMPATIBILITY_VERSION_010000(10000)
        /// Specifies that the toolkit exhibits version 1.0 behavior and all new features in later releases are unavailable. Select this option if you purchased version 1.0 and want to maintain functional behavior.
        ///</param>
        ///<param name = "isNewSession">
        /// isNewSession
        /// int32*
        /// Returns NILTESA_VAL_TRUE if the function creates a new session. This attribute returns NILTESA_VAL_FALSE if the function returns a reference to an existing session. 
        ///</param>
        public niLTESA(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            IntPtr session = new IntPtr();
            int pInvokeResult = PInvoke.niLTESA_OpenSession(sessionName, toolkitCompatibilityVersion, out session, out isNewSession);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, session);
            if (String.IsNullOrEmpty(sessionName))
                _isNamedSession = false;
            else
                _isNamedSession = true;
        }

        /// <summary>
        /// Returns a reference to a new or existing niLTE analysis session.
        /// </summary>
        ///<param name = "toolkitCompatibilityVersion">
        /// compatibilityVersion
        /// int32
        /// Specifies the toolkit compatibility version. 
        ///                         NILTESA_VAL_COMPATIBILITY_VERSION_010000(10000)
        /// Specifies that the toolkit exhibits version 1.0 behavior and all new features in later releases are unavailable. Select this option if you purchased version 1.0 and want to maintain functional behavior.
        ///</param>
        public niLTESA(int toolkitCompatibilityVersion)
        {
            System.IntPtr session;
            int isNewSession;
            int pInvokeResult = PInvoke.niLTESA_OpenSession(string.Empty, toolkitCompatibilityVersion, out session, out isNewSession);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, session);
            _isNamedSession = false;
        }

        public HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        /// <summary>
        /// Returns the power spectrum trace obtained as part of the ACP measurement. The toolkit decides the units based on the NILTESA_ACP_MEASUREMENT_RESULTS_TYPE attribute. 
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals,in hertz (Hz), between the data points in the spectrum.
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm/RBW or dBm/Hz. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array. Pass NULL to the spectrum parameter to get size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ACPGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_ACPGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// <p class="Body">
        /// Performs demodulation, power versus time (PvT), complementary cumulative distribution function (CCDF), channel power (CHP), adjacent channel power (ACP), occupied bandwidth (OBW), and SEM measurements on the input complex waveform. Use the niLTESA_AnalyzeIQComplexF64 function only if the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute is set to e is set to NILTESA_VAL_RECOMMENDED_ACQUISITION_TYPE_IQ.
        /// </summary>
        ///<param name = "t0">
        /// Specifies the trigger (start) time of the waveform array.
        ///</param>
        ///<param name = "dt">
        /// Specifies the time between values in the waveform array.
        ///</param>
        ///<param name = "waveform">
        /// Specifies the complex-valued signal for a baseband-modulated waveform. The real and imaginary parts of this complex data array correspond to the in-phase (I) and quadrature-phase (Q) data, respectively.
        ///</param>
        ///<param name = "numberofSamples">
        /// Specifies the number of complex samples in the waveform array. 
        ///</param>
        ///<param name = "reset">
        /// Specifies whether to reset the measurement and averaging.
        ///</param>
        ///<param name = "averagingDone">
        ///  Indicates that the function has finished performing averaging on the measurements.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int AnalyzeIQComplexF64(double t0, double dt, niComplexNumber[] waveform, int numberofSamples, int reset, out int averagingDone)
        {
            int pInvokeResult = PInvoke.niLTESA_AnalyzeIQComplexF64(Handle, t0, dt, waveform, numberofSamples, reset, out averagingDone);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Performs spectral measurements on the input power spectrum. Pass the powerSpectrumData parameter of the niRFSA_ReadPowerSpectrumF64 (Cluster) function or the powerSpectrum parameter of the niLTESA_RFSAReadGatedPowerSpectrum function to the niLTESA_AnalyzePowerSpectrum function. Use the niLTESA_AnalyzePowerSpectrum function only if the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute is set to e is set to NILTESA_VAL_RECOMMENDED_ACQUISITION_TYPE_SPECTRUM.
        /// </summary>
        ///<param name = "f0">
        /// Specifies the start frequency, in hertz (Hz), of the spectrum.
        ///</param>
        ///<param name = "df">
        /// Specifies the frequency interval, in Hz, between data points in the spectrum.
        ///</param>
        ///<param name = "spectrum">
        /// Specifies the acquired waveform as an array. If averaging is enabled, returns the averaged power spectrum.
        ///</param>
        ///<param name = "powerSpectrumArraySize">
        /// Specifies the size of the spectrum array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int AnalyzePowerSpectrum(double f0, double df, double[] spectrum, int powerSpectrumArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_AnalyzePowerSpectrum(Handle, f0, df, spectrum, powerSpectrumArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Checks for errors on all configured attributes. If the configuration is invalid, this function returns an error.
        /// </summary>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int CheckToolkitError()
        {
            int pInvokeResult = PInvoke.niLTESA_CheckToolkitError(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the channel power (CHP) spectrum trace.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz).
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals, in Hz, between the data points in the spectrum.
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm.
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array. Pass NULL to the spectrum parameter to get size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int CHPGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_CHPGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Closes the niLTE analysis session and releases resources associated with that session. Call this function once for each uniquely named session that you have created. 
        /// </summary>
        public void Close()
        {
            if (!_isNamedSession)
                Dispose();
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                    PInvoke.niLTESA_CloseSession(Handle);
            }
        }
        /// <summary>
        /// Configures the niLTE session to contain PUSCH channels that occupy the available system bandwidth in all 10 subframes of the given frame with the specified modulation scheme. For example, if the system bandwidth is 20 MHz and the PUSCH modulation scheme is QPSK, this function returns a session that contains 10 PUSCH channels designated as pusch0 to pusch9 in subframes 0 to 9 with QPSK modulation. For each PUSCH channel, the resource block offset is 0 and the number of resource blocks is 100.
        /// </summary>
        ///<param name = "pUSCHModulationScheme">
        /// Specifies the modulation scheme for PUSCH transmission. The default value is PuschModulationSchemeQpsk.
        ///     PuschModulationSchemeQpsk (0)
        ///    Specifies a quadrature phase shift keying (QPSK) modulation scheme. This value is the default.
        ///     PuschModulationScheme16qam (1)
        ///    Specifies a 16-quadrature amplitude modulation (QAM) scheme.
        ///     PuschModulationScheme64qam (2)
        ///    Specifies a 64-QAM modulation scheme.
        ///</param>
        ///<param name = "systemBandwidth">
        /// Specifies the LTE system bandwidth, in hertz (Hz), of the generated waveform.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ConfigureFullyFilledPUSCHChannels(int pUSCHModulationScheme, double systemBandwidth)
        {
            int pInvokeResult = PInvoke.niLTESA_ConfigureFullyFilledPUSCHChannels(Handle, pUSCHModulationScheme, systemBandwidth);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Computes the carrier frequency using the value that you specify in the EARFCN parameter, as described in section 5.7.3 of the 3GPP TS 36.101 specifications v8.6.0.
        /// </summary>
        ///<param name = "eARFCN">
        /// Specifies the E-UTRA absolute radio frequency channel number (EARFCN) as described in Table 5.7.3-1 in section 5.7.3 of the 3GPP TS 36.101 v8.6.0 specifications.
        ///</param>
        ///<param name = "reserved">
        /// Set this parameter to 1.
        ///</param>
        ///<param name = "carrierFrequency">
        /// Returns the carrier frequency, in hertz (Hz).
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int EARFCNtoCarrierFrequency(int eARFCN, out double carrierFrequency)
        {
            int pInvokeResult = PInvoke.niLTESA_EARFCNtoCarrierFrequency(eARFCN, 1, out carrierFrequency);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Takes the error code returned by niLTE analysis functions and returns the interpretation as a user-readable string. 
        /// </summary>
        ///<param name = "errorCode">
        /// Specifies the error code that is returned from any of the niLTE analysis functions.
        ///</param>
        ///<param name = "errorMessage">
        /// Returns the user-readable message string that corresponds to the error code you specify. The errorMessage buffer must have at least as many elements as are indicated in the errorMessageLength parameter. If you pass NULL to the errorMessage parameter, the function returns the actual length of the error message.
        ///</param>
        ///<param name = "errorMessageLength">
        /// Specifies the length of the errorMessage buffer.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int GetErrorString(int errorCode, StringBuilder errorMessage, int errorMessageLength)
        {
            int pInvokeResult = PInvoke.niLTESA_GetErrorString(Handle, errorCode, errorMessage, errorMessageLength);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// <p class="Body">Retrieves the constellation of the received signal. The constellation trace consists of the constellation for all the slots in the measurement interval. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "iData">
        /// Returns the real part of the constellation. If you pass NULL to the IData and QData arrays, the function returns the size of the arrays in the actualArraySize parameter.
        ///</param>
        ///<param name = "qData">
        /// Returns the imaginary part of the constellation. If you pass NULL to the IData and QData arrays, the function returns the size of the arrays in the actualArraySize parameter.
        ///</param>
        ///<param name = "length">
        /// Specifies the length of the IData and QData arrays.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the IData and QData arrays. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationDataConstellationTrace(string channelString, double[] iData, double[] qData, int length, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationDataConstellationTrace(Handle, channelString, iData, qData, length, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// <p class="Body">Returns the demodulation reference signal (DMRS) constellation trace. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "iData">
        /// Returns the real part of the constellation. If you pass NULL to the IData and QData arrays, the function returns the size of the arrays in the actualArraySize parameter.
        ///</param>
        ///<param name = "qData">
        /// Returns the imaginary part of the constellation. If you pass NULL to the IData and QData arrays, the function returns the size of the arrays in the actualArraySize parameter.
        ///</param>
        ///<param name = "length">
        /// Specifies the length of the IData and QData arrays.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the IData and QData arrays. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationDMRSConstellationTrace(string channelString, double[] iData, double[] qData, int length, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationDMRSConstellationTrace(Handle, channelString, iData, qData, length, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the error vector magnitude (EVM) per resource block (RB) trace. The number of elements in the trace is equal to the maximum RB allocation in the given measurement interval. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        ///  Returns the start parameter. 
        ///</param>
        ///<param name = "dx">
        ///  Returns the delta parameter. 
        ///</param>
        ///<param name = "eVMperResourceBlock">
        /// Returns the EVM per RB trace.
        ///</param>
        ///<param name = "eVMperRBArraySize">
        /// Specifies the number of elements in the EVMperRB array. Pass NULL to the EVMperRB parameter to get size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the EVMperRB array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationEVMPerResourceBlockTrace(string channelString, out double x0, out double dx, double[] eVMperResourceBlock, int eVMperRBArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationEVMPerResourceBlockTrace(Handle, channelString, out x0, out dx, eVMperResourceBlock, eVMperRBArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the error vector magnitude (EVM) per slot trace. The number of elements in the trace is equal to the number of slots in the given measurement interval. For example, if the measurement interval is set to 20 slots, the trace contains 20 elements. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        ///  Returns the start parameter.
        ///</param>
        ///<param name = "dx">
        ///  Returns the delta parameter. 
        ///</param>
        ///<param name = "eVMperSlot">
        /// Returns the EVM per slot trace.
        ///</param>
        ///<param name = "eVMperSlotArraySize">
        /// Specifies the number of elements in the EVMperSlot array. Pass NULL to the EVMperSlot parameter to get the size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the EVMperSlot array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationEVMPerSlotTrace(string channelString, out double x0, out double dx, double[] eVMperSlot, int eVMperSlotArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationEVMPerSlotTrace(Handle, channelString, out x0, out dx, eVMperSlot, eVMperSlotArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the error vector magnitude (EVM) per subcarrier, in dB or as a percentage, for each iteration when the toolkit processes the acquired waveform. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        ///  Returns the start parameter. 
        ///</param>
        ///<param name = "dx">
        ///  Returns the delta parameter.
        ///</param>
        ///<param name = "eVMperSubcarrier">
        /// Returns the EVM per subcarrier, in dB, for each iteration during processing of the acquired signal.
        ///</param>
        ///<param name = "eVMperSubcarrierArraySize">
        /// Specifies the number of elements in the EVMperSubcarrier array. Pass NULL to the EVMperSubcarrier parameter to get the size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the EVMperSubcarrier array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationEVMPerSubcarrierTrace(string channelString, out double x0, out double dx, double[] eVMperSubcarrier, int eVMperSubcarrierArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationEVMPerSubcarrierTrace(Handle, channelString, out x0, out dx, eVMperSubcarrier, eVMperSubcarrierArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// <p class="Body">Returns the error vector magnitude (EVM) per symbol per subcarrier, in dB or percentage, for each iteration when the toolkit processes the acquired waveform. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "eVMperSymbolperSubcarrier">
        /// Returns the EVM per symbol per subcarrier, in dB or as a percentage, for each iteration during processing of the acquired waveform.
        ///</param>
        ///<param name = "numRows">
        /// Specifies the number of symbols .
        ///</param>
        ///<param name = "numColumns">
        /// Specifies the number of subcarriers.
        ///</param>
        ///<param name = "actualNumRows">
        /// Returns the actual number of rows in the EVMTrace array. If the EVMTrace array is not large
        /// enough to hold all the samples, the function returns an error and this parameter returns the expected number of rows of the EVMTrace.
        ///</param>
        ///<param name = "actualNumColumns">
        /// Returns the actual number of columns in the EVMTrace array. If the EVMTrace array is not large
        /// enough to hold all the samples, the function returns an error and this parameter returns the expected number of columns of the EVMTrace.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationEVMPerSymbolPerSubcarrierTrace(string channelString, double[,] eVMperSymbolperSubcarrier, int numRows, int numColumns, out int actualNumRows, out int actualNumColumns)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationEVMPerSymbolPerSubcarrierTrace(Handle, channelString, eVMperSymbolperSubcarrier, numRows, numColumns, out actualNumRows, out actualNumColumns);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the error vector magnitude (EVM) per symbol trace, in dB or as a percentage. The number of elements in the trace is equal to the number of symbols in the given measurement interval. For example, if the measurement interval is set to 20 slots, the trace contains 20 * 7 symbols/slot = 140 elements for a normal cyclic prefix (CP) mode. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        ///</param>
        ///<param name = "x0">
        ///  Returns the start of the symbol trace.
        ///</param>
        ///<param name = "dx">
        ///  Returns the increment in the symbol index.
        ///</param>
        ///<param name = "eVMperSymbol">
        /// Returns the EVM per symbol trace.
        ///</param>
        ///<param name = "eVMperSymbolArraySize">
        /// Specifies the number of elements in the EVMperSymbol array. Pass NULL to the EVMperSymbol parameter to get the size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the EVMperSymbol array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationEVMPerSymbolTrace(string channelString, out double x0, out double dx, double[] eVMperSymbol, int eVMperSymbolArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationEVMPerSymbolTrace(Handle, channelString, out x0, out dx, eVMperSymbol, eVMperSymbolArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the spectral flatness trace of the channel. This function returns the relative magnitude variation in the channel. You can query this function only if you set the NILTESA_MODACC_EVM_PER_SYMBOL_PER_SUBCARRIER_TRACE_ENABLED attribute or NILTESA_MODACC_ALL_TRACES_ENABLED attribute to NILTESA_VAL_TRUE.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        ///  Returns the start parameter. 
        ///</param>
        ///<param name = "dx">
        ///  Returns the delta parameter. 
        ///</param>
        ///<param name = "spectralFlatness">
        /// Returns the spectral flatness trace of the channel.
        ///</param>
        ///<param name = "spectralFlatnessArraySize">
        /// Specifies the number of elements in the spectralFlatness array. Pass NULL to the spectralFlatnessArraySize parameter to get the size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the spectralFlatness array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationSpectralFlatnessTrace(string channelString, out double x0, out double dx, double[] spectralFlatness, int spectralFlatnessArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationSpectralFlatnessTrace(Handle, channelString, out x0, out dx, spectralFlatness, spectralFlatnessArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the power spectrum trace obtained as part of occupied bandwidth (OBW) measurement.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals, in hertz (Hz), between the data points in the spectrum.
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm/RBW. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the spectrum array. Pass NULL to the spectrum parameter to get size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int OBWGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_OBWGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets the attribute specified in the attributeID parameter to its default value. You can reset only a writable attribute using this function.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "attributeID">
        /// Specifies the ID of the niLTE analysis attribute that you want to reset.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ResetAttribute(string channelString, niLTESAProperties attributeID)
        {
            int pInvokeResult = PInvoke.niLTESA_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets all the attributes of the session to their default values.
        /// </summary>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niLTESA_ResetSession(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Examines the incoming signal to calculate the peak power level. This function then returns the estimated peak power level in the resultantReferenceLevel parameter. Use this function if you need help calculating an approximate setting for the power level for I/Q measurements.
        /// This function queries the NIRFSA_ATTR_REFERENCE_LEVEL attribute and uses this value as the starting point for auto level calculations. Set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to the highest expected power level of the signal for faster convergence. For example, if the device under test (DUT) generates the signal with the average power of x dBm, set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to x + expected PAPR of the signal.
        /// </summary>
        ///<param name = "rFSASession">
        /// Specifies a reference to an NI-RFSA instrument session. The toolkit obtains this parameter from the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session.
        ///</param>
        ///<param name = "hardwareChannelString">
        /// Specifies the RFSA device channel. Set this parameter to "" (empty string) or NULL.
        ///</param>
        ///<param name = "bandwidth">
        /// Specifies the bandwidth, in hertz (Hz), of the signal to be analyzed. The default value is 10M.
        ///</param>
        ///<param name = "measurementInterval">
        /// Specifies the acquisition length, in seconds. The toolkit uses this value to compute the number of samples to acquire from the RF signal analyzer. The default value is 10m.
        ///</param>
        ///<param name = "maxNumberofIterations">
        /// Specifies the maximum number of iterations to perform when computing the reference level to be set on the RF signal analyzer. The default value is 5.
        ///</param>
        ///<param name = "resultantReferenceLevel">
        /// Returns the estimated peak power level, in dBm, of the input signal.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int RFSAAutoLevel(HandleRef rFSASession, string hardwareChannelString, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel)
        {
            int pInvokeResult = PInvoke.niLTESA_RFSAAutoLevel(rFSASession, hardwareChannelString, bandwidth, measurementInterval, maxNumberofIterations, out resultantReferenceLevel);
            TestForError(pInvokeResult, rFSASession);
            return pInvokeResult;
        }
        /// <summary>
        /// Retrieves the recommended hardware settings from the niLTE analysis session and sets these values to the appropriate niRFSA attributes.
        /// This function sets the following attributes:
        ///     Sets the NIRFSA_ATTR_ACQUISITION_TYPE attribute to NIRFSA_VAL_IQ.
        ///     Sets the NIRFSA_ATTR_NUM_RECORDS_IS_FINITE attribute to VI_TRUE.
        ///     Sets the NILTESA_RECOMMENDED_ACQUISITION_LENGTH attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.
        ///     Sets the NILTESA_RECOMMENDED_IQ_ACQUISITION_IQ_RATE attribute to the NIRFSA_ATTR_IQ_RATE attribute.
        ///     If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to NIRFSA_VAL_IQ_POWER_EDGE, this function sets the NILTESA_RECOMMENDED_IQ_ACQUISITION_MINIMUM_QUIET_TIME attribute to the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_MINIMUM_QUIET_TIME attribute. If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to any other value, this function sets the NILTESA_RECOMMENDED_IQ_ACQUISITION_MINIMUM_QUIET_TIME attribute to 0.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NILTESA_RECOMMENDED_ACQUISITION_LENGTH attribute, and sets the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.
        ///     Sets the NIRFSA_ATTR_NUM_SAMPLES_IS_FINITE attribute to VI_TRUE.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NILTESA_RECOMMENDED_IQ_ACQUISITION_PRETRIGGER_DELAY attribute, and sets the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.
        /// </summary>
        ///<param name = "lTEChannelString">
        /// Set this parameter to "" (empty string) or NULL.
        ///</param>
        ///<param name = "rFSASession">
        /// Specifies a reference to an NI-RFSA instrument session. The toolkit obtains this parameter from the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session.
        ///</param>
        ///<param name = "hardwareChannelString">
        /// Specifies the RFSA device channel. Set this parameter to "" (empty string) or NULL.
        ///</param>
        ///<param name = "samplesPerRecord">
        /// Returns the number of samples per record configured for the NI-RFSA session.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int RFSAConfigureHardware(string lTEChannelString, HandleRef rFSASession, string hardwareChannelString, out Int64 samplesPerRecord)
        {
            int pInvokeResult = PInvoke.niLTESA_RFSAConfigureHardware(Handle, lTEChannelString, rFSASession, hardwareChannelString, out samplesPerRecord);
            TestForError(pInvokeResult, rFSASession);
            return pInvokeResult;
        }
        /// <summary>
        /// Configures the NI RF vector signal analyzer and initiates acquisition on the hardware. When the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute is set to NILTESA_VAL_RECOMMENDED_ACQUISITION_TYPE_IQ, this function fetches the waveforms and calls the niLTESA_AnalyzePowerSpectrum in a loop n times to perform measurements on the acquired waveforms, where n is equal
        /// to the number of averages specified.
        /// </summary>
        ///<param name = "rFSASession">
        /// Specifies a reference to an NI-RFSA instrument session. The toolkit obtains this parameter from the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session.
        ///</param>
        ///<param name = "hardwareChannelString">
        /// Specifies the RFSA device channel. Set this parameter to "" (empty string) or NULL.
        ///</param>
        ///<param name = "timeOut">
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of -1 specifies that the function waits until all data is available. The default value is 10.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int RFSAMeasure(HandleRef rFSASession, string hardwareChannelString, double timeOut)
        {
            int pInvokeResult = PInvoke.niLTESA_RFSAMeasure(Handle, rFSASession, hardwareChannelString, timeOut);
            TestForError(pInvokeResult, rFSASession);
            return pInvokeResult;
        }
        /// <summary>
        /// Initiates a gated spectrum acquisition for one or more channels and returns the averaged power spectrum data.
        /// Gated spectrum is the spectrum of the signal acquired in a specified time interval. This acquisition may be initiated by the transition of the signal from idle state to an active state.
        /// </summary>
        ///<param name = "rFSASession">
        /// Specifies a reference to an NI-RFSA instrument session. The toolkit obtains this parameter from the niRFSA_init or niRFSA_InitWithOptions function and identifies a particular instrument session.
        ///</param>
        ///<param name = "hardwareChannelString">
        /// Specifies the RFSA device channel. Set this parameter to "" (empty string) or NULL.
        ///</param>
        ///<param name = "timeout">
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of -1 specifies that the function waits until all data is available. The default value is 10.  
        ///</param>
        ///<param name = "f0">
        /// Returns the start frequency of the spectrum, in hertz (Hz).  
        ///</param>
        ///<param name = "df">
        /// Returns the frequency interval between data points in the spectrum. 
        ///</param>
        ///<param name = "powerSpectrum">
        /// Returns the real-value power spectrum. 
        ///</param>
        ///<param name = "powerSpectrumArraySize">
        /// Specifies the size of the powerSpectrum array. The actualNumPowerSpectrumElements parameter contains the size of the spectrum. To obtain the size of the power spectrum, pass NULL to the powerSpectrum parameter.  
        ///</param>
        ///<param name = "actualNumSpectrumElement">
        /// Returns the actual number of elements populated in the powerSpectrum array. If the powerSpectrum array is not large enough to hold all the samples, the function returns an error and this parameter returns the minimum expected size of the output array.  
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int RFSAReadGatedPowerSpectrum(HandleRef rFSASession, string hardwareChannelString, double timeout, out double f0, out double df, double[] powerSpectrum, int powerSpectrumArraySize, out int actualNumSpectrumElement)
        {
            int pInvokeResult = PInvoke.niLTESA_RFSAReadGatedPowerSpectrum(Handle, rFSASession, hardwareChannelString, timeout, out f0, out df, powerSpectrum, powerSpectrumArraySize, out actualNumSpectrumElement);
            TestForError(pInvokeResult, rFSASession);
            return pInvokeResult;
        }

        /// <summary>
        /// Enables all the measurements that you specify in the measurements parameter and disables all other measurements.
        /// </summary>
        ///<param name = "measurement">
        /// Specifies a list of measurements to perform. You can choose from the following measurements. 
        /// The default value is NILTESA_VAL_MODACC_MEASUREMENT.
        /// NILTESA_VAL_MODACC_MEASUREMENT(1)
        /// Enables modulation accuracy (ModAcc) measurements.
        /// NILTESA_VAL_CHP_MEASUREMENT(2)
        /// Enables channel power (CHP) measurements.
        /// NILTESA_VAL_ACP_MEASUREMENT(4)
        /// Enables adjacent channel power (ACP) measurements.
        /// NILTESA_VAL_OBW_MEASUREMENT(8)
        /// Enables occupied bandwidth (OBW) measurements.
        /// NILTESA_VAL_SEM_MEASUREMENT(16)
        /// Enables spectral emission mask (SEM) measurements.
        /// NILTESA_VAL_PVT_MEASUREMENT(32)
        /// Enables power versus time (PvT) measurements.
        /// NILTESA_VAL_CCDF_MEASUREMENT(64)
        /// Enables complementary cumulative distribution function (CCDF) measurements.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SelectMeasurements(UInt32 measurement)
        {
            int pInvokeResult = PInvoke.niLTESA_SelectMeasurements(Handle, measurement);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Enables all the measurements that you specify in the measurements parameter and disables all other measurements.
        /// </summary>
        ///<param name = "measurement">
        /// Specifies a list of measurements to perform. You can choose from the following measurements.
        /// The default value is NILTESA_VAL_MODACC_MEASUREMENT.
        /// NILTESA_VAL_MODACC_MEASUREMENT(1)
        /// Enables modulation accuracy (ModAcc) measurements.
        /// NILTESA_VAL_CHP_MEASUREMENT(2)
        /// Enables channel power (CHP) measurements.
        /// NILTESA_VAL_ACP_MEASUREMENT(4)
        /// Enables adjacent channel power (ACP) measurements.
        /// NILTESA_VAL_OBW_MEASUREMENT(8)
        /// Enables occupied bandwidth (OBW) measurements.
        /// NILTESA_VAL_SEM_MEASUREMENT(16)
        /// Enables spectral emission mask (SEM) measurements.
        /// NILTESA_VAL_PVT_MEASUREMENT(32)
        /// Enables power versus time (PvT) measurements.
        /// NILTESA_VAL_CCDF_MEASUREMENT(64)
        /// Enables complementary cumulative distribution function (CCDF) measurements.
        ///</param>
        ///<param name = "enableTraces">
        /// Specifies whether to enable traces for the selected measurement.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SelectMeasurementsWithTraces(UInt32 measurement, int enableTraces)
        {
            int pInvokeResult = PInvoke.niLTESA_SelectMeasurementsWithTraces(Handle, measurement, enableTraces);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the absolute spectral mask limit trace. The limit trace depends on the NILTESA_SEM_MASK_TYPE attribute.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        ///</param>
        ///<param name = "dx">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz).  
        ///</param>
        ///<param name = "absoluteLimits">
        /// Returns the absolute spectral mask limit trace. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the absoluteLimits array. Pass NULL to the absoluteLimits parameter to get size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the absoluteLimits array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SEMGetAbsoluteLimitTrace(string channelString, out double x0, out double dx, double[] absoluteLimits, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_SEMGetAbsoluteLimitTrace(Handle, channelString, out x0, out dx, absoluteLimits, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the frequency domain spectrum trace.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "f0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz).  
        ///</param>
        ///<param name = "df">
        /// Returns the frequency intervals, in hertz (Hz), between the data points in the spectrum.  
        ///</param>
        ///<param name = "spectrum">
        /// Returns the array of frequency domain power values, in dBm.  
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the  spectrum array. Pass NULL to the spectrum  parameter to get the size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SEMGetSpectrumTrace(string channelString, out double f0, out double df, double[] spectrum, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_SEMGetSpectrumTrace(Handle, channelString, out f0, out df, spectrum, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        //2.0 methods
        /// <summary>
        /// Configures the niLTE analysis session to contain physical uplink shared channels (PUSCHs) that occupy the available system bandwidth in all 10 subframes of the given frame with the specified modulation scheme. The toolkit configures the PUSCH for time-division duplex (TDD) based on the value of the NILTESA_UL_DL_CONFIGURATION attribute.
        /// For example, if the system bandwidth is 20 MHz and the PUSCH modulation scheme is QPSK, this function returns a session that contains 10 PUSCH channels designated as pusch0 to pusch9 in subframes 0 to 9 with QPSK modulation. For each PUSCH channel, the resource block offset is set to 0 and the number of resource blocks is set to 100.
        /// This function sets the NILTESA_DUPLEX_MODE attribute to NILTESA_VAL_DUPLEX_MODE_UL_FDD. The toolkit uses all other attributes from the session.
        /// </summary>
        ///<param name = "duplexMode">
        /// Specifies the direction and the duplexing technique that the toolkit uses to create the waveform. The default value is NILTESA_VAL_DUPLEX_MODE_UL_FDD.
        ///      NILTESA_VAL_DUPLEX_MODE_UL_FDD (1)
        ///    Specifies that the direction is uplink (UL) and duplexing technique is frequency-division duplexing (FDD) for
        /// the analyzed signal.
        ///    NILTESA_VAL_DUPLEX_MODE_UL_TDD (3)
        ///    Specifies that the direction is uplink (UL) and duplexing technique is time-division duplexing (TDD) for
        /// the analyzed signal.
        ///</param>
        ///<param name = "pUSCHModulationScheme">
        /// Specifies the modulation scheme for PUSCH transmission. The default value is NILTESA_VAL_MODULATION_SCHEME_QPSK.
        ///     NILTESA_VAL_MODULATION_SCHEME_QPSK (0)
        ///    Specifies a quadrature phase-shift keying (QPSK) modulation scheme.
        ///     NILTESA_VAL_MODULATION_SCHEME_16_QAM (1)
        ///    Specifies a 16-quadrature amplitude modulation (QAM) scheme.
        ///     NILTESA_VAL_MODULATION_SCHEME_64_QAM (2)
        ///    Specifies a 64-QAM modulation scheme.
        ///</param>
        ///<param name = "systemBandwidth">
        /// Specifies the LTE system bandwidth, in hertz (Hz), of the generated waveform.
        ///</param>
        ///<param name = "uLDLConfiguration">
        /// Specifies the uplink/downlink (UL/DL) configuration index for the FDD/TDD frame.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ConfigureFullyFilledPUSCHFrame(int duplexMode, int pUSCHModulationScheme, double systemBandwidth, int uLDLConfiguration)
        {
            int pInvokeResult = PInvoke.niLTESA_ConfigureFullyFilledPUSCHFrame(Handle, duplexMode, pUSCHModulationScheme, systemBandwidth, uLDLConfiguration);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the niLTE session to contain physical uplink control channels (PUCCHs) that occupy the available system bandwidth in all 10 subframes of the given frame.
        /// This function sets the NILTESA_DUPLEX_MODE attribute to NILTESA_VAL_DUPLEX_MODE_UL_FDD. The toolkit uses all other attributes from the session.
        /// </summary>
        ///<param name = "duplexMode">
        ///</param>
        ///<param name = "pUCCHFormat">
        /// Specifies the format used for physical uplink control channel (PUCCH) transmission. The default value is NILTESA_VAL_UL_PUCCH_FORMAT_1.
        ///     NILTESA_VAL_UL_PUCCH_FORMAT_1 (0)
        ///    Specifies that the toolkit uses Format 1 for PUCCH transmission.
        ///     NILTESA_VAL_UL_PUCCH_FORMAT_1A (1)
        ///    Specifies that the toolkit uses Format 1A for PUCCH transmission.
        ///     NILTESA_VAL_UL_PUCCH_FORMAT_1B (2)
        ///    Specifies that the toolkit uses Format 1B for PUCCH transmission.
        ///     NILTESA_VAL_UL_PUCCH_FORMAT_2 (3)
        ///    Specifies that the toolkit uses Format 2 for PUCCH transmission.
        ///     NILTESA_VAL_UL_PUCCH_FORMAT_2A (4)
        ///    Specifies that the toolkit uses Format 2A for PUCCH transmission.
        ///     NILTESA_VAL_UL_PUCCH_FORMAT_2B (5)
        ///    Specifies that the toolkit uses Format 2B for PUCCH transmission.
        ///</param>
        ///<param name = "n_PUCCH_1">
        /// Specifies a parameter used in determining the resource block assigned for the physical uplink control channel (PUCCH) formats 1/1a/1b as defined in section 5.4.1 of the 3GPP TS 36.211 v8.8.0 specifications.
        ///</param>
        ///<param name = "pUCCHpowerdB">
        /// Specifies the physical uplink control channel (PUCCH) power level, in dB, relative to the power of the PUCCH demodulation reference signal (DMRS). The default value is 0.
        ///</param>
        ///<param name = "uLDLConfiguration">
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ConfigureFullyFilledPUCCHFrame(int duplexMode, int pUCCHFormat, int n_PUCCH_1, double pUCCHpowerdB, int uLDLConfiguration)
        {
            int pInvokeResult = PInvoke.niLTESA_ConfigureFullyFilledPUCCHFrame(Handle, duplexMode, pUCCHFormat, n_PUCCH_1, pUCCHpowerdB, uLDLConfiguration);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Saves attributes of the session, which you have modified after opening the session, to a file located at the specified path. 
        /// <p class="Body">You can use this function to save the current state of the toolkit session to a file.  You can later load the saved configuration using the niLTESA_LoadConfigurationFromFile function.
        /// </summary>
        ///<param name = "filePath">
        /// Specifies the absolute path to the TDMS file to which the toolkit saves the configuration.
        ///</param>
        ///<param name = "operation">
        /// Specifies the operation to perform on the file. The default value is NILTESA_FILE_OPERATION_MODE_OPEN.
        /// NILTESA_FILE_OPERATION_MODE_OPEN (0)
        /// Opens an existing file to write the niLTE analysis session attribute values.
        /// NILTESA_FILE_OPERATION_MODE_OPENORCREATE (1)
        /// Opens an existing file or creates a new file if the file does not exist.
        /// NILTESA_FILE_OPERATION_MODE_CREATEORREPLACE (2)
        /// Creates a new file or replaces a file if it exists.
        /// NILTESA_FILE_OPERATION_MODE_CREATE (3)
        /// Creates a new file.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SaveConfigurationToFile(string filePath, int operation)
        {
            int pInvokeResult = PInvoke.niLTESA_SaveConfigurationToFile(Handle, filePath, operation);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Reads values of session attributes (configuration) saved in a file and sets these values to the corresponding attributes on the session, thus restoring the state of the toolkit to the original state when the file was saved.
        /// </summary>
        ///<param name = "filePath">
        /// Specifies the absolute path to the file from which the toolkit loads the configuration.
        ///</param>
        ///<param name = "resetSession">
        /// Specifies whether toolkit must reset all the attributes of the session to their default values before setting the new values specified in the file. The default value is NILTESA_VAL_TRUE. 
        /// NILTESA_VAL_FALSE(0)
        /// Specifies that the toolkit does not reset all the attributes of the session to their default values.
        /// NILTESA_VAL_TRUE(1)
        /// Specifies that the toolkit resets all the attributes of the session to their default values before setting new values.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int LoadConfigurationFromFile(string filePath, int resetSession)
        {
            int pInvokeResult = PInvoke.niLTESA_LoadConfigurationFromFile(Handle, filePath, resetSession);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Reads a waveform from a TDMS file. You can save this file using the LTE Analysis Soft Front Panel. The niLTESA_ReadWaveformFromFile function returns the I/Q complex waveform data that you can subsequently download to an RF vector signal generator.
        /// </summary>
        ///<param name = "filePath">
        /// Specifies the complete path of the TDMS file from which the toolkit reads the waveform.
        ///</param>
        ///<param name = "waveformName">
        /// Specifies the name of the waveform to read from the file.
        ///</param>
        ///<param name = "offset">
        /// Specifies the number of samples in the waveform at which the function begins reading the I/Q data.  The default value is 0. If you set count to 1,000 and offset to 2, the function returns 1,000 samples, starting from index 2 and ending at index 1,002. 
        ///</param>
        ///<param name = "count">
        /// Specifies the maximum number of samples of the I/Q complex waveform to read from the file. The default value is -1, which returns all samples. If you set count to 1,000 and offset to 2, the function returns 1,000 samples, starting from index 2 and ending at index 1,002.
        ///</param>
        ///<param name = "t0">
        /// Returns the starting time, in seconds.
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval between baseband I/Q samples, in seconds.
        ///</param>
        ///<param name = "waveform">
        /// Returns the LTE I/Q data. This parameter must be at least equal to the waveformSize parameter. You can pass NULL to the waveform parameter to query the size of the waveform.
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the waveform size, in samples.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Specifies the actual size of the waveform array.
        ///</param>
        ///<param name = "eof">
        /// Specifies whether the end of file has been reached with this read.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public static int ReadWaveformFromFile(string filePath, string waveformName, Int64 offset, Int64 count, out double t0, out double dt, niComplexNumber[] waveform, int dataArraySize, out int actualNumDataArrayElements, out int eof)
        {
            int pInvokeResult = PInvoke.niLTESA_ReadWaveformFromFile(filePath, waveformName, offset, count, out t0, out dt, waveform, dataArraySize, out actualNumDataArrayElements, out eof);
            TestForStaticError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the I/Q power trace for the complementary cumulative distribution function (CCDF) measurements.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        /// Returns the starting time, in seconds. 
        ///</param>
        ///<param name = "dx">
        /// Returns the time interval, in seconds.
        ///</param>
        ///<param name = "data">
        /// Returns the array of CCDF I/Q power measurement values. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the data array. Pass NULL to the data parameter to get size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the spectrum array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int CCDFGetCurrentIterationIQPowerTrace(string channelString, out double x0, out double dx, double[] data, int dataArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_CCDFGetCurrentIterationIQPowerTrace(Handle, channelString, out x0, out dx, data, dataArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the complimentary cumulative distribution function (CCDF) trace for the created waveform.   
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        /// Returns the starting power level relative to the average power.  
        ///</param>
        ///<param name = "dx">
        /// Returns the power interval, in dB. 
        ///</param>
        ///<param name = "data">
        /// Returns an array of the percentage of samples that lie on or above the corresponding power level on the x-axis.  
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the data array. Pass NULL to the data parameter to get size of the array in the actualNumDataArrayElements parameter.  
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the data array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int CCDFGetCurrentIterationProbabilitiesTrace(string channelString, out double x0, out double dx, double[] data, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_CCDFGetCurrentIterationProbabilitiesTrace(Handle, channelString, out x0, out dx, data, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the complimentary cumulative distribution function (CCDF) trace for an ideal Gaussian distribution signal.  
        /// </summary>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL. 
        ///</param>
        ///<param name = "x0">
        /// Returns the starting power level relative to the average power. 
        ///</param>
        ///<param name = "dx">
        /// Returns the power interval, in dB. 
        ///</param>
        ///<param name = "data">
        /// Returns an array of the percentage of samples that lie on or above the corresponding power level on the x-axis. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the data array. Pass NULL to the data parameter to get size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the data array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int CCDFGetCurrentIterationGaussianProbabilitiesTrace(string channelString, out double x0, out double dx, double[] data, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_CCDFGetCurrentIterationGaussianProbabilitiesTrace(Handle, channelString, out x0, out dx, data, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the mask trace for the current iteration obtained as part of the power versus time (PvT) measurement. S
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        ///</param>
        ///<param name = "dx">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz).
        ///</param>
        ///<param name = "data">
        /// Returns the array of mask trace values. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the data array. Pass NULL to the data parameter to get size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the data array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int PvTGetCurrentIterationMaskTrace(string channelString, out double x0, out double dx, double[] data, int dataArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_PvTGetCurrentIterationMaskTrace(Handle, channelString, out x0, out dx, data, dataArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the power spectrum trace obtained as part of the ACP measurement. The toolkit decides the units based on the NILTESA_ACP_MEASUREMENT_RESULTS_TYPE attribute. 
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        /// Returns the starting time, in seconds.  
        ///</param>
        ///<param name = "dx">
        /// Returns the time interval, in seconds. 
        ///</param>
        ///<param name = "data">
        /// Returns the array of frequency domain power values, in dBm/RBW or dBm/Hz. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the data array. Pass NULL to the data parameter to get size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the data array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int PvTGetCurrentIterationPvTTrace(string channelString, out double x0, out double dx, double[] data, int dataArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_PvTGetCurrentIterationPvTTrace(Handle, channelString, out x0, out dx, data, dataArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// <p class="Body">Retrieves the constellation of the received sounding reference signal (SRS). The constellation trace consists of the constellation for all the slots in the measurement interval.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "iData">
        /// Returns the real part of the constellation. If you pass NULL to the IData and QData arrays, the function returns the size of the arrays in the actualArraySize parameter.
        ///</param>
        ///<param name = "qData">
        /// Returns the imaginary part of the constellation. If you pass NULL to the IData and QData arrays, the function returns the size of the arrays in the actualArraySize parameter.
        ///</param>
        ///<param name = "length">
        /// Specifies the length of the IData and QData arrays.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the IData and QData arrays. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ModAccGetCurrentIterationSRSConstellationTrace(string channelString, double[] iData, double[] qData, int length, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ModAccGetCurrentIterationSRSConstellationTrace(Handle, channelString, iData, qData, length, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the I/Q trace obtained as part of adjacent channel power (ACP) measurement. 
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "t0">
        /// Returns the start parameter. 
        ///</param>
        ///<param name = "dt">
        /// Returns the delta parameter.
        ///</param>
        ///<param name = "waveform">
        /// Returns the array of I/Q values acquired from the RF vector signal analyzer. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the waveform array. Pass NULL to the waveform parameter to get size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the waveform array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int ACPGetCurrentIterationIQWaveformTrace(string channelString, out double t0, out double dt, niComplexNumber[] waveform, int dataArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_ACPGetCurrentIterationIQWaveformTrace(Handle, channelString, out t0, out dt, waveform, dataArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the power spectrum trace obtained as part of the ACP measurement. The toolkit decides the units based on the NILTESA_ACP_MEASUREMENT_RESULTS_TYPE attribute. 
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "t0">
        /// Returns the start parameter.  
        ///</param>
        ///<param name = "dt">
        /// Returns the delta parameter.
        ///</param>
        ///<param name = "waveform">
        /// Returns the array of I/Q values acquired from the RF vector signal analyzer. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the waveform array. Pass NULL to the waveform parameter to get size of the array in the actualArraySize parameter.
        ///</param>
        ///<param name = "actualArraySize">
        /// Returns the actual number of elements populated in the waveform array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualArraySize parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SEMGetCurrentIterationIQWaveformTrace(string channelString, out double t0, out double dt, niComplexNumber[] waveform, int dataArraySize, out int actualArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_SEMGetCurrentIterationIQWaveformTrace(Handle, channelString, out t0, out dt, waveform, dataArraySize, out actualArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the relative spectral mask limit trace.
		/// Known issue: This function currently throws an "AceessViolationException" when build using 3.5, 3.0 or 2.0 versions of .NET framework.
		/// Use .NET framework 4.0 for this function.
        /// </summary>
        ///<param name = "channelString">
        ///</param>
        ///<param name = "x0">
        /// Returns the initial frequency of the spectrum, in hertz (Hz). 
        ///</param>
        ///<param name = "dx">
        /// Returns the frequency intervals between the data points in the spectrum, in hertz (Hz).  
        ///</param>
        ///<param name = "relativeLimits">
        /// Returns the array of relative mask limit power values, in dBm. 
        ///</param>
        ///<param name = "dataArraySize">
        /// Specifies the number of elements in the relativeLimits array. Pass NULL to the relativeLimits parameter to get size of the array in the actualNumDataArrayElements parameter.
        ///</param>
        ///<param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the relativeLimits array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        ///</returns>
        public int SEMGetRelativeLimitTrace(string channelString, out double x0, out double dx, double[] relativeLimits, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_SEMGetRelativeLimitTrace(Handle, channelString, out x0, out dx, relativeLimits, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        ///Specifies the array of frequency offsets from the reference channel center frequency, in hertz (Hz), for adjacent channels for adjacent channel power (ACP)    measurement. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not same.
        ///    The default value is [9M, 3.84M, 3.84M]. 
        /// </summary>
        public int SetAcpAdjacentChannelsBandwidths(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.AcpAdjacentChannelsBandwidths, value, valueArraySize);
        }

        /// <summary>
        ///Specifies the array of frequency offsets from the reference channel center frequency, in hertz (Hz), for adjacent channels for adjacent channel power (ACP)    measurement. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not same.
        ///    The default value is [9M, 3.84M, 3.84M].
        /// </summary>
        public int GetAcpAdjacentChannelsBandwidths(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.AcpAdjacentChannelsBandwidths, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies whether to enable the elements in the adjacent channels configuration arrays for adjacent channel power (ACP) measurement.
        ///    The default value is {NILTESA_VAL_TRUE, NILTESA_VAL_TRUE, NILTESA_VAL_TRUE}.
        /// </summary>
        public int SetAcpAdjacentChannelsEnabled(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.AcpAdjacentChannelsEnabled, value, valueArraySize);
        }
        /// <summary>
        ///Specifies whether to enable the elements in the adjacent channels configuration arrays for adjacent channel power (ACP) measurement.
        ///    The default value is {NILTESA_VAL_TRUE, NILTESA_VAL_TRUE, NILTESA_VAL_TRUE}.
        /// </summary>
        public int GetAcpAdjacentChannelsEnabled(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.AcpAdjacentChannelsEnabled, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of bandwidths, in hertz (Hz), for adjacent channels for adjacent channel power (ACP) measurement. The toolkit returns an error if the    array sizes of all the Offset Bands attributes are not same.
        ///    The default value is [10M, 7.5M, 12.5M]. Valid values are 0 to 50M, inclusive.
        /// </summary>
        public int SetAcpAdjacentChannelsFrequencyOffsets(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.AcpAdjacentChannelsFrequencyOffsets, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of bandwidths, in hertz (Hz), for adjacent channels for adjacent channel power (ACP) measurement. The toolkit returns an error if the    array sizes of all the Offset Bands attributes are not same.
        ///    The default value is [10M, 7.5M, 12.5M]. Valid values are 0 to 50M, inclusive.
        /// </summary>
        public int GetAcpAdjacentChannelsFrequencyOffsets(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.AcpAdjacentChannelsFrequencyOffsets, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter that is used as a matched filter.
        ///    The default value is [0, 0.22, 0.22]. 
        /// </summary>
        public int SetAcpAdjacentChannelsRrcFilterAlpha(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.AcpAdjacentChannelsRrcFilterAlpha, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter that is used as a matched filter.
        ///    The default value is [0, 0.22, 0.22]. 
        /// </summary>
        public int GetAcpAdjacentChannelsRrcFilterAlpha(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.AcpAdjacentChannelsRrcFilterAlpha, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies whether to enable the root raised cosine (RRC) filters for the adjacent channels for adjacent channel power (ACP) measurement.
        ///    The default value is [NILTESA_VAL_FALSE, NILTESA_VAL_TRUE, NILTESA_VAL_TRUE].
        /// </summary>
        public int SetAcpAdjacentChannelsRrcFilterEnabled(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.AcpAdjacentChannelsRrcFilterEnabled, value, valueArraySize);
        }
        /// <summary>
        ///Specifies whether to enable the root raised cosine (RRC) filters for the adjacent channels for adjacent channel power (ACP) measurement.
        ///    The default value is [NILTESA_VAL_FALSE, NILTESA_VAL_TRUE, NILTESA_VAL_TRUE].
        /// </summary>
        public int GetAcpAdjacentChannelsRrcFilterEnabled(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.AcpAdjacentChannelsRrcFilterEnabled, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of sidebands for adjacent channels for adjacent channel power (ACP) measurement. The toolkit returns an error if the array sizes of all the    Offset Bands attributes are not same. 
        ///    The default value is   [NILTESA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH, NILTESA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH, NILTESA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH]. 
        /// </summary>
        public int SetAcpAdjacentChannelsSidebands(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.AcpAdjacentChannelsSidebands, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of sidebands for adjacent channels for adjacent channel power (ACP) measurement. The toolkit returns an error if the array sizes of all the    Offset Bands attributes are not same. 
        ///    The default value is   [NILTESA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH, NILTESA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH, NILTESA_VAL_ACP_ADJACENT_CHANNELS_SIDEBANDS_BOTH]. 
        /// </summary>
        public int GetAcpAdjacentChannelsSidebands(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.AcpAdjacentChannelsSidebands, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the method used to average the ACP measurement results.
        ///    The default value is NILTESA_VAL_ACP_AVERAGE_TYPE_LINEAR.
        /// </summary>
        public int SetAcpAverageType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpAverageType, value);
        }
        /// <summary>
        ///Specifies the method used to average the ACP measurement results.
        ///    The default value is NILTESA_VAL_ACP_AVERAGE_TYPE_LINEAR.
        /// </summary>
        public int GetAcpAverageType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpAverageType, out value);
        }
        /// <summary>
        ///Specifies whether to enable ACP measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetAcpEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable ACP measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetAcpEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpEnabled, out value);
        }
        /// <summary>
        ///Specifies the method used to calculate the adjacent channel powers.
        ///    The default value is NILTESA_VAL_ACP_FREQUENCY_LIST_TYPE_STANDARD.
        /// </summary>
        public int SetAcpFrequencyListType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpFrequencyListType, value);
        }
        /// <summary>
        ///Specifies the method used to calculate the adjacent channel powers.
        ///    The default value is NILTESA_VAL_ACP_FREQUENCY_LIST_TYPE_STANDARD.
        /// </summary>
        public int GetAcpFrequencyListType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpFrequencyListType, out value);
        }
        /// <summary>
        ///Specifies the type of the results for ACP measurement.
        ///    The default value is NILTESA_VAL_ACP_MEASUREMENT_RESULTS_TYPE_TOTAL_POWER_REFERENCE.
        /// </summary>
        public int SetAcpMeasurementResultsType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpMeasurementResultsType, value);
        }
        /// <summary>
        ///Specifies the type of the results for ACP measurement.
        ///    The default value is NILTESA_VAL_ACP_MEASUREMENT_RESULTS_TYPE_TOTAL_POWER_REFERENCE.
        /// </summary>
        public int GetAcpMeasurementResultsType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpMeasurementResultsType, out value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages ACP measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int SetAcpNumberOfAverages(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpNumberOfAverages, value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages ACP measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int GetAcpNumberOfAverages(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpNumberOfAverages, out value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the reference channel.
        ///    The default value is 9M.
        /// </summary>
        public int SetAcpReferenceChannelBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.AcpReferenceChannelBandwidth, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the reference channel.
        ///    The default value is 9M.
        /// </summary>
        public int GetAcpReferenceChannelBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.AcpReferenceChannelBandwidth, out value);
        }
        /// <summary>
        ///Specifies the physical layer cell identity as defined in section 6.11 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The analyzer fails to synchronize for physical uplink control channel (PUCCH), if you set the NILTESA_CELL_ID attribute to 175 or 375.   The default value is 0. Valid values are 0 to 503, inclusive.
        /// </summary>
        public int SetCellId(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CellId, value);
        }
        /// <summary>
        ///Specifies the physical layer cell identity as defined in section 6.11 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The analyzer fails to synchronize for physical uplink control channel (PUCCH), if you set the NILTESA_CELL_ID attribute to 175 or 375.   The default value is 0. Valid values are 0 to 503, inclusive.
        /// </summary>
        public int GetCellId(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CellId, out value);
        }
        /// <summary>
        ///Specifies whether to enable channel power measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetChpEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable channel power measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetChpEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpEnabled, out value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), for performing channel power measurement.
        ///    The default value is 9M. Valid values are 1k to 20M, inclusive.
        /// </summary>
        public int SetChpMeasurementBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ChpMeasurementBandwidth, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), for performing channel power measurement.
        ///    The default value is 9M. Valid values are 1k to 20M, inclusive.
        /// </summary>
        public int GetChpMeasurementBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ChpMeasurementBandwidth, out value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages channel power measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int SetChpNumberOfAverages(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpNumberOfAverages, value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages channel power measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int GetChpNumberOfAverages(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpNumberOfAverages, out value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition. 
        ///    The toolkit returns a warning if this value is less than the NILTESA_CHP_MEASUREMENT_BANDWIDTH attribute, and coerces the    NILTESA_CHP_SPAN attribute to 1.2 times the NILTESA_CHP_MEASUREMENT_BANDWIDTH attribute.
        /// </summary>
        public int SetChpSpan(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ChpSpan, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition. 
        ///    The toolkit returns a warning if this value is less than the NILTESA_CHP_MEASUREMENT_BANDWIDTH attribute, and coerces the    NILTESA_CHP_SPAN attribute to 1.2 times the NILTESA_CHP_MEASUREMENT_BANDWIDTH attribute.
        /// </summary>
        public int GetChpSpan(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ChpSpan, out value);
        }
        /// <summary>
        ///Specifies the method used to select the span and measurement bandwidth.
        ///    The default value is NILTESA_VAL_CHP_SPAN_TYPE_STANDARD.
        /// </summary>
        public int SetChpSpanType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpSpanType, value);
        }
        /// <summary>
        ///Specifies the method used to select the span and measurement bandwidth.
        ///    The default value is NILTESA_VAL_CHP_SPAN_TYPE_STANDARD.
        /// </summary>
        public int GetChpSpanType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpSpanType, out value);
        }
        /// <summary>
        ///Specifies the cyclic prefix mode as defined in section 5.2.3 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The default value is NILTESA_VAL_CYCLIC_PREFIX_MODE_NORMAL.
        /// </summary>
        public int SetCyclicPrefixMode(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CyclicPrefixMode, value);
        }
        /// <summary>
        ///Specifies the cyclic prefix mode as defined in section 5.2.3 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The default value is NILTESA_VAL_CYCLIC_PREFIX_MODE_NORMAL.
        /// </summary>
        public int GetCyclicPrefixMode(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CyclicPrefixMode, out value);
        }
        /// <summary>
        ///Specifies the carrier frequency, in hertz (Hz), of the received signal. The toolkit uses this value when the NILTESA_MODACC_COMMON_CLOCK_SOURCE    attribute is set to NILTESA_VAL_TRUE.
        ///    The default value is 1G.
        /// </summary>
        public int SetHardwareSettingsCarrierFrequency(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.HardwareSettingsCarrierFrequency, value);
        }
        /// <summary>
        ///Specifies the carrier frequency, in hertz (Hz), of the received signal. The toolkit uses this value when the NILTESA_MODACC_COMMON_CLOCK_SOURCE    attribute is set to NILTESA_VAL_TRUE.
        ///    The default value is 1G.
        /// </summary>
        public int GetHardwareSettingsCarrierFrequency(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.HardwareSettingsCarrierFrequency, out value);
        }
        /// <summary>
        ///Specifies the maximum real-time bandwidth, in hertz (Hz), of the RF signal analyzer. The toolkit uses this attribute along with the enabled    measurements to determine the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute.
        ///    The default value is 50M. 
        /// </summary>
        public int SetHardwareSettingsMaxRealtimeBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.HardwareSettingsMaxRealtimeBandwidth, value);
        }
        /// <summary>
        ///Specifies the maximum real-time bandwidth, in hertz (Hz), of the RF signal analyzer. The toolkit uses this attribute along with the enabled    measurements to determine the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute.
        ///    The default value is 50M. 
        /// </summary>
        public int GetHardwareSettingsMaxRealtimeBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.HardwareSettingsMaxRealtimeBandwidth, out value);
        }
        /// <summary>
        ///Specifies the trigger delay, in seconds, if you set the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute to NILTESA_VAL_RECOMMENDED_ACQUISITION_TYPE_IQ. The toolkit computes the NILTESA_RECOMMENDED_IQ_ACQUISITION_PRETRIGGER_DELAY and    NILTESA_RECOMMENDED_IQ_ACQUISITION_POSTTRIGGER_DELAY attributes based on the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute.
        /// </summary>
        public int SetHardwareSettingsTriggerDelay(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.HardwareSettingsTriggerDelay, value);
        }
        /// <summary>
        ///Specifies the trigger delay, in seconds, if you set the NILTESA_RECOMMENDED_ACQUISITION_TYPE attribute to NILTESA_VAL_RECOMMENDED_ACQUISITION_TYPE_IQ. The toolkit computes the NILTESA_RECOMMENDED_IQ_ACQUISITION_PRETRIGGER_DELAY and    NILTESA_RECOMMENDED_IQ_ACQUISITION_POSTTRIGGER_DELAY attributes based on the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute.
        /// </summary>
        public int GetHardwareSettingsTriggerDelay(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.HardwareSettingsTriggerDelay, out value);
        }
        /// <summary>
        ///Specifies whether to enable all modulation accuracy (ModAcc) traces.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccAllTracesEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable all modulation accuracy (ModAcc) traces.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccAllTracesEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable the auto detection of channel configuration, allocated resource blocks, and modulation scheme. Auto RB detection is not supported for physical uplink control channel (PUCCH) and PUCCH with sounding reference signal (SRS) channel configurations. Set the NILTESA_MODACC_AUTO_RB_DETECTION_ENABLED attribute to NILTESA_VAL_FALSE when the number of resource blocks configured is 1, because Auto RB detection is not robust in this case. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccAutoRbDetectionEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccAutoRbDetectionEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the auto detection of channel configuration, allocated resource blocks, and modulation scheme. Auto RB detection is not supported for physical uplink control channel (PUCCH) and PUCCH with sounding reference signal (SRS) channel configurations. Set the NILTESA_MODACC_AUTO_RB_DETECTION_ENABLED attribute to NILTESA_VAL_FALSE when the number of resource blocks configured is 1, because Auto RB detection is not robust in this case. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccAutoRbDetectionEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccAutoRbDetectionEnabled, out value);
        }
        /// <summary>
        ///Specifies the method used for uplink (UL) channel estimation.
        ///    The default value is NILTESA_VAL_CHANNEL_ESTIMATION_TYPE_REFERENCE_AND_DATA.
        /// </summary>
        public int SetModaccChannelEstimationType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccChannelEstimationType, value);
        }
        /// <summary>
        ///Specifies the method used for uplink (UL) channel estimation.
        ///    The default value is NILTESA_VAL_CHANNEL_ESTIMATION_TYPE_REFERENCE_AND_DATA.
        /// </summary>
        public int GetModaccChannelEstimationType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccChannelEstimationType, out value);
        }
        /// <summary>
        ///Specifies whether the clock source on the generator used to generate the Sample clock and the carrier frequency is common.
        ///    The default value is NILTESA_VAL_TRUE.
        /// </summary>
        public int SetModaccCommonClockSource(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccCommonClockSource, value);
        }
        /// <summary>
        ///Specifies whether the clock source on the generator used to generate the Sample clock and the carrier frequency is common.
        ///    The default value is NILTESA_VAL_TRUE.
        /// </summary>
        public int GetModaccCommonClockSource(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccCommonClockSource, out value);
        }
        /// <summary>
        ///Specifies whether to enable the constellation trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccConstellationTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccConstellationTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the constellation trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccConstellationTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccConstellationTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable modulation accuracy (ModAcc) measurements on the acquired signal.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable modulation accuracy (ModAcc) measurements on the acquired signal.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEnabled, out value);
        }
        /// <summary>
        ///Specifies the unit in which the toolkit returns error vector magnitude (EVM) results.
        ///    The default value is NILTESA_VAL_EVM_UNIT_DB.
        /// </summary>
        public int SetModaccEvmMeasurementUnit(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEvmMeasurementUnit, value);
        }
        /// <summary>
        ///Specifies the unit in which the toolkit returns error vector magnitude (EVM) results.
        ///    The default value is NILTESA_VAL_EVM_UNIT_DB.
        /// </summary>
        public int GetModaccEvmMeasurementUnit(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEvmMeasurementUnit, out value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per resource block (RB) trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccEvmPerRbTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEvmPerRbTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per resource block (RB) trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccEvmPerRbTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEvmPerRbTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per slot trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccEvmPerSlotTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEvmPerSlotTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per slot trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccEvmPerSlotTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEvmPerSlotTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per subcarrier trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccEvmPerSubcarrierTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEvmPerSubcarrierTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per subcarrier trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccEvmPerSubcarrierTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEvmPerSubcarrierTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per symbol per subcarrier trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccEvmPerSymbolPerSubcarrierTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEvmPerSymbolPerSubcarrierTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per symbol per subcarrier trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccEvmPerSymbolPerSubcarrierTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEvmPerSymbolPerSubcarrierTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per symbol trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccEvmPerSymbolTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccEvmPerSymbolTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the error vector magnitude (EVM) per symbol trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccEvmPerSymbolTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccEvmPerSymbolTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies the length, as a percentage of the cyclic prefix, by which the fast Fourier transform (FFT) window is moved to generate the left and right FFT windows using the center FFT window.
        ///    The default value is 50. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int SetModaccFftWindowLength(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ModaccFftWindowLength, value);
        }
        /// <summary>
        ///Specifies the length, as a percentage of the cyclic prefix, by which the fast Fourier transform (FFT) window is moved to generate the left and right FFT windows using the center FFT window.
        ///    The default value is 50. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int GetModaccFftWindowLength(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ModaccFftWindowLength, out value);
        }
        /// <summary>
        ///Specifies the center fast Fourier transform (FFT) window position within the single carrier-frequency domain multiple access (SC-FDMA) symbol as a percentage of the cyclic prefix. For example, if you specify the    FFT window position as 50% and the cyclic prefix length is 144, the symbol boundary starts at sample number 72.
        ///    The default value is 50. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int SetModaccFftWindowPosition(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ModaccFftWindowPosition, value);
        }
        /// <summary>
        ///Specifies the center fast Fourier transform (FFT) window position within the single carrier-frequency domain multiple access (SC-FDMA) symbol as a percentage of the cyclic prefix. For example, if you specify the    FFT window position as 50% and the cyclic prefix length is 144, the symbol boundary starts at sample number 72.
        ///    The default value is 50. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int GetModaccFftWindowPosition(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ModaccFftWindowPosition, out value);
        }
        /// <summary>
        ///Specifies the type of FFT window configuration used for error vector magnitude (EVM) calculation.
        ///    The default value is NILTESA_VAL_FFT_WINDOW_TYPE_CUSTOM.
        /// </summary>
        public int SetModaccFftWindowType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccFftWindowType, value);
        }
        /// <summary>
        ///Specifies the type of FFT window configuration used for error vector magnitude (EVM) calculation.
        ///    The default value is NILTESA_VAL_FFT_WINDOW_TYPE_CUSTOM.
        /// </summary>
        public int GetModaccFftWindowType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccFftWindowType, out value);
        }
        /// <summary>
        ///Specifies the number of slots to analyze.
        ///    The default value is 1. Valid values are 1 to 20, inclusive.
        /// </summary>
        public int SetModaccMeasurementLength(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccMeasurementLength, value);
        }
        /// <summary>
        ///Specifies the number of slots to analyze.
        ///    The default value is 1. Valid values are 1 to 20, inclusive.
        /// </summary>
        public int GetModaccMeasurementLength(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccMeasurementLength, out value);
        }
        /// <summary>
        ///Specifies whether the toolkit performs modulation accuracy (ModAcc) measurement on the slots that you specify or on any available slots in the    acquired waveform.
        ///    The default value is NILTESA_VAL_MEASUREMENT_MODE_SPECIFIC_SLOT.
        /// </summary>
        public int SetModaccMeasurementMode(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccMeasurementMode, value);
        }
        /// <summary>
        ///Specifies whether the toolkit performs modulation accuracy (ModAcc) measurement on the slots that you specify or on any available slots in the    acquired waveform.
        ///    The default value is NILTESA_VAL_MEASUREMENT_MODE_SPECIFIC_SLOT.
        /// </summary>
        public int GetModaccMeasurementMode(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccMeasurementMode, out value);
        }
        /// <summary>
        ///Specifies the starting slot number to analyze. The slot offset must point to an occupied slot. The current version of the toolkit does not support ModAcc measurements in the UpPTS field of a special subframe.
        ///    The default value is 0. Valid values are 0 to 19, inclusive.
        /// </summary>
        public int SetModaccMeasurementOffset(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccMeasurementOffset, value);
        }
        /// <summary>
        ///Specifies the starting slot number to analyze. The slot offset must point to an occupied slot. The current version of the toolkit does not support ModAcc measurements in the UpPTS field of a special subframe.
        ///    The default value is 0. Valid values are 0 to 19, inclusive.
        /// </summary>
        public int GetModaccMeasurementOffset(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccMeasurementOffset, out value);
        }
        /// <summary>
        ///Specifies the number of iterations over which the toolkit averages modulation accuracy (ModAcc) measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int SetModaccNumberOfAverages(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccNumberOfAverages, value);
        }
        /// <summary>
        ///Specifies the number of iterations over which the toolkit averages modulation accuracy (ModAcc) measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int GetModaccNumberOfAverages(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccNumberOfAverages, out value);
        }
        /// <summary>
        ///Specifies whether to enable the spectral flatness trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetModaccSpectralFlatnessTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ModaccSpectralFlatnessTraceEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable the spectral flatness trace.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetModaccSpectralFlatnessTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ModaccSpectralFlatnessTraceEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable occupied bandwidth (OBW) measurement.
        ///    The OBW measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_OBW_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetObwEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ObwEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable occupied bandwidth (OBW) measurement.
        ///    The OBW measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_OBW_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetObwEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ObwEnabled, out value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages occupied bandwidth (OBW) measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int SetObwNumberOfAverages(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ObwNumberOfAverages, value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages occupied bandwidth (OBW) measurements.
        ///    The default value is 1. Valid values are 1 to 1,000, inclusive.
        /// </summary>
        public int GetObwNumberOfAverages(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ObwNumberOfAverages, out value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the resolution bandwidth (RBW) filter.
        /// </summary>
        public int SetObwResolutionBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ObwResolutionBandwidth, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the resolution bandwidth (RBW) filter.
        /// </summary>
        public int GetObwResolutionBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ObwResolutionBandwidth, out value);
        }
        /// <summary>
        ///Specifies the resolution bandwidth (RBW) filter type for occupied bandwidth (OBW) measurement.
        ///    The default value is NILTESA_VAL_OBW_RESOLUTION_BANDWIDTH_FILTER_TYPE_GAUSSIAN.
        /// </summary>
        public int SetObwResolutionBandwidthFilterType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ObwResolutionBandwidthFilterType, value);
        }
        /// <summary>
        ///Specifies the resolution bandwidth (RBW) filter type for occupied bandwidth (OBW) measurement.
        ///    The default value is NILTESA_VAL_OBW_RESOLUTION_BANDWIDTH_FILTER_TYPE_GAUSSIAN.
        /// </summary>
        public int GetObwResolutionBandwidthFilterType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ObwResolutionBandwidthFilterType, out value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition.
        /// </summary>
        public int SetObwSpan(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ObwSpan, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), for acquisition.
        /// </summary>
        public int GetObwSpan(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ObwSpan, out value);
        }
        /// <summary>
        ///Specifies the method used to configure the NILTESA_OBW_SPAN_TYPE, NILTESA_OBW_RESOLUTION_BANDWIDTH_FILTER_TYPE, and    NILTESA_OBW_RESOLUTION_BANDWIDTH attributes.
        ///    The default value is NILTESA_VAL_OBW_SPAN_TYPE_STANDARD.
        /// </summary>
        public int SetObwSpanType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ObwSpanType, value);
        }
        /// <summary>
        ///Specifies the method used to configure the NILTESA_OBW_SPAN_TYPE, NILTESA_OBW_RESOLUTION_BANDWIDTH_FILTER_TYPE, and    NILTESA_OBW_RESOLUTION_BANDWIDTH attributes.
        ///    The default value is NILTESA_VAL_OBW_SPAN_TYPE_STANDARD.
        /// </summary>
        public int GetObwSpanType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ObwSpanType, out value);
        }
        /// <summary>
        ///Returns the length of the records to acquire, in seconds.
        ///    For demodulating an LTE frame that has a measurement interval of 10 ms, the toolkit requires approximately two frames.
        ///    When the toolkit performs the composite measurement, the toolkit sets the NILTESA_RECOMMENDED_ACQUISITION_LENGTH attribute attribute to the maximum of all the individual measurements' acquisition length attributes. 
        /// </summary>
        public int GetRecommendedAcquisitionLength(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedAcquisitionLength, out value);
        }
        /// <summary>
        ///Indicates the type of acquisition for the current measurement configuration. 
        ///    The toolkit returns an error if the configured measurements require a bandwidth greater than the NILTESA_HARDWARE_SETTINGS_MAX_REALTIME_BANDWIDTH    attribute and if at least one of the measurements enabled requires I/Q mode of acquisition. For example, the toolkit returns an error if you set the    NILTESA_SYSTEM_BANDWIDTH attribute to 20 MHz, NILTESA_HARDWARE_SETTINGS_MAX_REALTIME_BANDWIDTH attribute to 40 MHz,    NILTESA_SEM_ENABLED attribute to NILTESA_VAL_TRUE, and NILTESA_MODACC_ENABLED attribute to NILTESA_VAL_TRUE.    The toolkit returns the error because spectral emission mask (SEM) requires an acquisition bandwidth of 70 MHz, that is, more than one acquisition centered at two frequencies.    However, demodulation requires exactly one acquisition centered at the carrier frequency. Additionally, you cannot perform demodulation measurements    with spectral acquisition.
        /// </summary>
        public int GetRecommendedAcquisitionType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.RecommendedAcquisitionType, out value);
        }
        /// <summary>
        ///Returns the sample rate, in samples per second (S/s), for the RF signal analyzer. 
        ///    If you do not use the niLTESA_RFSAConfigureHardware function, pass this attribute to the niRFSA_ConfigureIQRate function.
        /// </summary>
        public int GetRecommendedIqAcquisitionIqRate(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedIqAcquisitionIqRate, out value);
        }
        /// <summary>
        ///Returns the minimum time, in seconds, for which the signal must be quiet before the device arms the trigger. The signal is quiet when it is below the trigger level if the trigger slope, specified by the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_SLOPE attribute, is set to NIRFSA_VAL_RISING_SLOPE, or above the trigger level if the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_SLOPE attribute, is set to NIRFSA_VAL_FALLING_SLOPE.
        ///    If you do not use the niLTESA_RFSAConfigureHardware function, pass this attribute to the NIRFSA_ATTR_REF_TRIGGER_MINIMUM_QUIET_TIME attribute.
        /// </summary>
        public int GetRecommendedIqAcquisitionMinimumQuietTime(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedIqAcquisitionMinimumQuietTime, out value);
        }
        /// <summary>
        ///Returns the number of records to acquire from the RF signal analyzer.
        /// </summary>
        public int GetRecommendedIqAcquisitionNumberOfRecords(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.RecommendedIqAcquisitionNumberOfRecords, out value);
        }
        /// <summary>
        ///Returns the post-trigger delay, in seconds.  
        ///    This attribute represents the start of the acquired waveform. If the value of the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute is negative, the toolkit sets the NILTESA_RECOMMENDED_IQ_ACQUISITION_POSTTRIGGER_DELAY attribute to 0. If the value of the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute is not negative, the toolkit sets the value of the NILTESA_RECOMMENDED_IQ_ACQUISITION_POSTTRIGGER_DELAY attribute to the negative value of the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute. 
        /// </summary>
        public int GetRecommendedIqAcquisitionPosttriggerDelay(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedIqAcquisitionPosttriggerDelay, out value);
        }
        /// <summary>
        ///Returns the pre-trigger delay, in seconds.  
        ///    If the value of the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute is negative, the toolkit sets the NILTESA_RECOMMENDED_IQ_ACQUISITION_PRETRIGGER_DELAY attribute to the absolute value of the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute. If the value of the NILTESA_HARDWARE_SETTINGS_TRIGGER_DELAY attribute is not negative, the toolkit sets the NILTESA_RECOMMENDED_IQ_ACQUISITION_PRETRIGGER_DELAY attribute to 0. The toolkit uses this attribute to acquire data from before or after the trigger. 
        /// </summary>
        public int GetRecommendedIqAcquisitionPretriggerDelay(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedIqAcquisitionPretriggerDelay, out value);
        }
        /// <summary>
        ///Indicates the time-domain window type used for spectral smoothing. The current version of the toolkit supports only a flat top fast Fourier transform (FFT) window type.
        /// </summary>
        public int GetRecommendedSpectralAcquisitionFftWindowType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.RecommendedSpectralAcquisitionFftWindowType, out value);
        }
        /// <summary>
        ///Returns the resolution bandwidth (RBW), in hertz (Hz), for spectral acquisition.
        /// </summary>
        public int GetRecommendedSpectralAcquisitionRbw(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedSpectralAcquisitionRbw, out value);
        }
        /// <summary>
        ///Returns the span, in hertz (Hz), for spectral acquisition.
        /// </summary>
        public int GetRecommendedSpectralAcquisitionSpan(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.RecommendedSpectralAcquisitionSpan, out value);
        }
        /// <summary>
        ///Returns an array of absolute powers, in dBm or dBm per hertz (Hz), of the negative sideband adjacent channels. The toolkit decides the unit    based on the NILTESA_ACP_MEASUREMENT_RESULTS_TYPE attribute.
        ///    Negative sideband adjacent channels are channels to the left of the reference channel. 
        /// </summary>
        public int GetResultAcpNegativeAbsolutePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultAcpNegativeAbsolutePowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns an array of relative powers, in dB, of the negative sideband adjacent channels. The power is relative to the reference    channel power.
        ///    Negative sideband adjacent channels are channels to the left of the reference channel. 
        /// </summary>
        public int GetResultAcpNegativeRelativePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultAcpNegativeRelativePowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns an array of absolute powers, in dBm or dBm per hertz (Hz), of the positive sideband adjacent channels. The toolkit decides the    unit based on the NILTESA_ACP_MEASUREMENT_RESULTS_TYPE attribute.
        ///    Positive sideband adjacent channels are channels to the right of the reference channel. 
        /// </summary>
        public int GetResultAcpPositiveAbsolutePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultAcpPositiveAbsolutePowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns an array of relative powers, in dB, of the positive sideband adjacent channels. The power is relative to the reference channel power.
        ///    Positive sideband adjacent channels are channels to the right of the reference channel. 
        /// </summary>
        public int GetResultAcpPositiveRelativePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultAcpPositiveRelativePowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the power, in dBm or dBm per hertz (dBm/Hz), of the reference channel. The toolkit decides the unit based on the    NILTESA_ACP_MEASUREMENT_RESULTS_TYPE attribute.
        /// </summary>
        public int GetResultAcpReferenceChannelPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultAcpReferenceChannelPower, out value);
        }
        /// <summary>
        ///Returns the power, in dBm, of the signal.
        /// </summary>
        public int GetResultChpChannelPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultChpChannelPower, out value);
        }
        /// <summary>
        ///Returns the power spectral density, in dBm per hertz (Hz), of the signal.
        /// </summary>
        public int GetResultChpChannelPowerSpectralDensity(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultChpChannelPowerSpectralDensity, out value);
        }
        /// <summary>
        ///Returns the power, in dBm, of non-allocated resource blocks (RBs) in the uplink signal. Refer to Annex E 4.3 of the 3GPP TS 36.521    v8.6.0 specifications for more information.
        /// </summary>
        public int GetResultModaccAbsoluteInBandEmissions(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccAbsoluteInBandEmissions, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the average of carrier frequency offset estimates, in hertz (Hz). 
        ///    The toolkit returns the average of the carrier frequency offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. The carrier frequency offset (CFO) estimation algorithm is not robust if CFO is more than +/-7 KHz, and might result in synchornization failure or bad error vector magnitudes (EVMs).
        /// </summary>
        public int GetResultModaccAverageCarrierFrequencyOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccAverageCarrierFrequencyOffset, out value);
        }
        /// <summary>
        ///Returns the average of I/Q gain imbalance estimates, in dB. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the average of the I/Q gain imbalance measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccAverageIqGainImbalance(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccAverageIqGainImbalance, out value);
        }
        /// <summary>
        ///Returns the average of I/Q offset estimates, in dB. 
        ///    The toolkit returns the average of the I/Q offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccAverageIqOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccAverageIqOffset, out value);
        }
        /// <summary>
        ///Returns the average of quadrature skew estimates, in degrees. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the average of the quadrature skew measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccAverageQuadratureSkew(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccAverageQuadratureSkew, out value);
        }
        /// <summary>
        ///Returns the average of Sample clock offset estimates, in parts per million (ppm). 
        ///    The toolkit returns the average of the Sample clock offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccAverageSampleClockOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccAverageSampleClockOffset, out value);
        }
        /// <summary>
        ///Returns the standard deviation of carrier frequency offset estimates, in hertz (Hz). 
        ///    The toolkit returns the standard deviation of the carrier frequency offset measurement over the number of acquisitions specified by    the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. The carrier frequency offset (CFO) estimation algorithm is not robust if CFO is more than +/-7 KHz, and might result in synchornization failure or bad error vector magnitudes (EVMs).
        /// </summary>
        public int GetResultModaccCarrierFrequencyOffsetStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccCarrierFrequencyOffsetStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the standard deviation of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit returns the standard deviation of the data RMS EVM measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. 
        ///    Use a 'pusch' or 'pucch' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccChannelDataEvmStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccChannelDataEvmStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the maximum of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit returns the maximum of the data RMS error vector magnitude (EVM) measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. 
        ///    Use a 'pusch' or 'pucch' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccChannelDataPeakEvm(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccChannelDataPeakEvm, out value);
        }
        /// <summary>
        ///Returns the average of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit averages the data RMS error vector magnitude (EVM) measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. 
        ///    Use a 'pusch' or 'pucch' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccChannelDataRmsEvm(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccChannelDataRmsEvm, out value);
        }
        /// <summary>
        ///Returns the standard deviation of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit returns the standard deviation of the RMS EVM measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. 
        ///    Use a 'pusch', 'pucch', or 'srs' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels. 
        /// </summary>
        public int GetResultModaccChannelEvmStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccChannelEvmStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the maximum of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified.  The toolkit returns the maximum of the RMS EVM measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. 
        ///    Use a 'pusch', 'pucch', or 'srs' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels. 
        /// </summary>
        public int GetResultModaccChannelPeakEvm(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccChannelPeakEvm, out value);
        }
        /// <summary>
        ///Returns the RMS error vector magnitude (EVM) measurements, in dB or as a percentage.  
        ///    The toolkit performs this measurement over the active channel specified.  The toolkit averages the RMS EVM measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. The current version of the toolkit does not support ModAcc measurements in the UpPTS field of the special subframe.
        ///    Use a 'pusch', 'pucch', or 'srs' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccChannelRmsEvm(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccChannelRmsEvm, out value);
        }
        /// <summary>
        ///Returns the standard deviation of I/Q gain imbalance estimates, in dB. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the standard deviation of the I/Q gain imbalance measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccIqGainImbalanceStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccIqGainImbalanceStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the standard deviation of I/Q offset estimates, in dB. 
        ///    The toolkit returns the standard deviation of the I/Q offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccIqOffsetStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccIqOffsetStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the maximum of carrier frequency offset estimates, in hertz (Hz). 
        ///    The toolkit returns the maximum of the carrier frequency offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. The carrier frequency offset (CFO) estimation algorithm is not robust if CFO is more than +/-7 KHz, and might result in synchornization failure or bad error vector magnitudes (EVMs).
        /// </summary>
        public int GetResultModaccMaximumCarrierFrequencyOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMaximumCarrierFrequencyOffset, out value);
        }
        /// <summary>
        ///Returns the maximum of I/Q gain imbalance estimates, in dB. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the maximum of the I/Q gain imbalance measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMaximumIqGainImbalance(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMaximumIqGainImbalance, out value);
        }
        /// <summary>
        ///Returns the maximum of I/Q offset estimates, in dB. 
        ///    The toolkit returns the maximum of the I/Q offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMaximumIqOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMaximumIqOffset, out value);
        }
        /// <summary>
        ///Returns the maximum of quadrature skew estimates, in degrees. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the maximum of the quadrature skew measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMaximumQuadratureSkew(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMaximumQuadratureSkew, out value);
        }
        /// <summary>
        ///Returns the maximum of Sample clock offset estimates, in parts per million (ppm). 
        ///    The toolkit returns the maximum of the Sample clock offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMaximumSampleClockOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMaximumSampleClockOffset, out value);
        }
        /// <summary>
        ///Returns the minimum of carrier frequency offset estimates, in hertz (Hz). 
        ///    The toolkit returns the minimum of the carrier frequency offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute. The carrier frequency offset (CFO) estimation algorithm is not robust if CFO is more than +/-7 KHz, and might result in synchornization failure or bad error vector magnitudes (EVMs).
        /// </summary>
        public int GetResultModaccMinimumCarrierFrequencyOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMinimumCarrierFrequencyOffset, out value);
        }
        /// <summary>
        ///Returns the minimum of I/Q gain imbalance estimates, in dB. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the minimum of the I/Q gain imbalance measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMinimumIqGainImbalance(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMinimumIqGainImbalance, out value);
        }
        /// <summary>
        ///Returns the minimum of I/Q offset estimates, in dB. 
        ///    The toolkit returns the minimum of the I/Q offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMinimumIqOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMinimumIqOffset, out value);
        }
        /// <summary>
        ///Returns the minimum of quadrature skew estimates, in degrees. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the minimum of the quadrature skew measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMinimumQuadratureSkew(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMinimumQuadratureSkew, out value);
        }
        /// <summary>
        ///Returns the minimum of Sample clock offset estimates, in parts per million (ppm). 
        ///    The toolkit returns the minimum of the Sample clock offset measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccMinimumSampleClockOffset(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccMinimumSampleClockOffset, out value);
        }
        /// <summary>
        ///Returns the standard deviation of quadrature skew estimates, in degrees. 
        ///    The toolkit estimates this attribute only when the allocation in the frequency domain is symmetric. The toolkit returns the standard deviation of the quadrature skew measurement over the number of acquisitions specified by the    NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccQuadratureSkewStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccQuadratureSkewStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the ratio of the power in the uplink signal for the resource blocks (RBs) next to the DC subcarrier to the power of all    allocated RBs in the uplink signal. Refer to Annex E 4.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information.
        /// </summary>
        public int GetResultModaccRelativeInBandEmissionsDc(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccRelativeInBandEmissionsDc, out value);
        }
        /// <summary>
        ///Returns an array of ratios of power in each non-allocated resource block (RB) to the average power of an allocated RB    in the uplink signal. Refer to Annex E 4.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information about this attribute.
        /// </summary>
        public int GetResultModaccRelativeInBandEmissionsGeneralIqImage(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccRelativeInBandEmissionsGeneralIqImage, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the standard deviation of Sample clock offset estimates, in parts per million (ppm). 
        ///    The toolkit returns the standard deviation of the Sample clock offset measurement over the number of acquisitions specified by    the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        /// </summary>
        public int GetResultModaccSampleClockOffsetStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccSampleClockOffsetStandardDeviation, out value);
        }
        /// <summary>
        ///Indicates whether the signal in the acquired waveform conforms to the reference channel configuration and section 5.5.2 of the    3GPP TS 36.211 v8.6.0 specifications. The NI RF signal analyzer might fail to synchronize if the carrier frequency offset (CFO) is more than +/-7 KHz, because CFO estimation in this range is not robust.
        /// </summary>
        public int GetResultModaccSyncFound(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ResultModaccSyncFound, out value);
        }
        /// <summary>
        ///Returns the standard deviation of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit returns the standard deviation of the Demodulation Reference Signal (DMRS) RMS EVM    measurement over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        ///    Use a 'pusch' or 'pucch' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccUlDmrsEvmStandardDeviation(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccUlDmrsEvmStandardDeviation, out value);
        }
        /// <summary>
        ///Returns the maximum of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit returns the maximum of the demodulation reference signal (DMRS) RMS EVM measurement    over the number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        ///    Use a 'pusch' or 'pucch' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccUlDmrsPeakEvm(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccUlDmrsPeakEvm, out value);
        }
        /// <summary>
        ///Returns the average of RMS error vector magnitude (EVM) measurements, in dB or as a percentage. 
        ///    The toolkit performs this measurement over the active channel specified. The toolkit averages the demodulation reference signal (DMRS) RMS EVM measurement over the    number of acquisitions specified by the NILTESA_MODACC_NUMBER_OF_AVERAGES attribute.
        ///    Use a 'pusch' or 'pucch' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetResultModaccUlDmrsRmsEvm(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultModaccUlDmrsRmsEvm, out value);
        }
        /// <summary>
        ///Returns the power, in dBm, of the resolution bandwidth (RBW) filtered signal integrated over the span.
        /// </summary>
        public int GetResultObwCarrierPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultObwCarrierPower, out value);
        }
        /// <summary>
        ///Returns the occupied bandwidth (OBW), in hertz (Hz), of the signal. This value is the frequency range containing 99% of the total power.
        /// </summary>
        public int GetResultObwOccupiedBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultObwOccupiedBandwidth, out value);
        }
        /// <summary>
        ///Returns the lower-bound frequency, in hertz (Hz), of the occupied bandwidth (OBW).
        /// </summary>
        public int GetResultObwStartFrequency(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultObwStartFrequency, out value);
        }
        /// <summary>
        ///Returns the upper-bound frequency, in hertz (Hz), of the occupied bandwidth (OBW).
        /// </summary>
        public int GetResultObwStopFrequency(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultObwStopFrequency, out value);
        }
        /// <summary>
        ///Indicates the status of spectral emission mask (SEM) measurement based on user-configured measurement limits.
        /// </summary>
        public int GetResultSemMeasurementStatus(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ResultSemMeasurementStatus, out value);
        }
        /// <summary>
        ///Returns the peak of negative side band powers, in dBm or dBm/Hz, for each band.
        /// </summary>
        public int GetResultSemNegativeAbsolutePeakPowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativeAbsolutePeakPowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the array of negative side band powers, in dBm or dBm/Hz, for each band.
        /// </summary>
        public int GetResultSemNegativeAbsolutePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativeAbsolutePowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the array of frequencies, in hertz (Hz), corresponding to each negative side band peak power.
        /// </summary>
        public int GetResultSemNegativePeakFrequencies(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativePeakFrequencies, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the array of power margins, in dB, for each negative side band. The power margin is relative to the absolute power values on the    spectral mask. The power margin value indicates the minimum difference between the spectral mask and the acquired spectrum.
        /// </summary>
        public int GetResultSemNegativePowerMargins(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativePowerMargins, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the frequency, in hertz (Hz), corresponding to the peak power in the reference channel.
        /// </summary>
        public int GetResultSemPeakReferenceFrequency(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPeakReferenceFrequency, out value);
        }
        /// <summary>
        ///Returns the peak of positive side band powers, in dBm or dBm/Hz, for each band.
        /// </summary>
        public int GetResultSemPositiveAbsolutePeakPowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositiveAbsolutePeakPowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the array of positive side band powers, in dBm or dBm/Hz, for each band.
        /// </summary>
        public int GetResultSemPositiveAbsolutePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositiveAbsolutePowers, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the array of frequencies, in hertz (Hz), corresponding to each positive side band peak power.
        /// </summary>
        public int GetResultSemPositivePeakFrequencies(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositivePeakFrequencies, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the array of power margins, in dB, for each positive side band. The power margin is relative to the absolute power values on the    spectral mask. The power margin value indicates the minimum difference between the spectral mask and the acquired spectrum.
        /// </summary>
        public int GetResultSemPositivePowerMargins(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositivePowerMargins, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Returns the integrated power, in dBm or dBm/Hz, of the reference channel for the specified integration bandwidth.
        /// </summary>
        public int GetResultSemReferencePower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemReferencePower, out value);
        }
        /// <summary>
        ///Specifies the averaging type for spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_SEM_AVERAGE_TYPE_LINEAR.
        /// </summary>
        public int SetSemAverageType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemAverageType, value);
        }
        /// <summary>
        ///Specifies the averaging type for spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_SEM_AVERAGE_TYPE_LINEAR.
        /// </summary>
        public int GetSemAverageType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemAverageType, out value);
        }
        /// <summary>
        ///Specifies whether to enable spectral emission mask (SEM) measurement.
        ///    The SEM measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_SEM_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetSemEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable spectral emission mask (SEM) measurement.
        ///    The SEM measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_SEM_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetSemEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemEnabled, out value);
        }
        /// <summary>
        ///Specifies the method used to select the offset (out of band) frequencies and the limits for the spectral emission mask (SEM).
        ///    The default value is NILTESA_VAL_SEM_MASK_TYPE_GENERAL.
        /// </summary>
        public int SetSemMaskType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemMaskType, value);
        }
        /// <summary>
        ///Specifies the method used to select the offset (out of band) frequencies and the limits for the spectral emission mask (SEM).
        ///    The default value is NILTESA_VAL_SEM_MASK_TYPE_GENERAL.
        /// </summary>
        public int GetSemMaskType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemMaskType, out value);
        }
        /// <summary>
        ///Specifies the measurement length, in seconds, for spectral emission mask (SEM) measurement.
        ///    The default value is 1m. 
        /// </summary>
        public int SetSemMeasurementLength(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemMeasurementLength, value);
        }
        /// <summary>
        ///Specifies the measurement length, in seconds, for spectral emission mask (SEM) measurement.
        ///    The default value is 1m. 
        /// </summary>
        public int GetSemMeasurementLength(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemMeasurementLength, out value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages spectral emission mask (SEM) measurements.
        ///    The default value is 1. Valid values are 0 to 1,000, inclusive.
        /// </summary>
        public int SetSemNumberOfAverages(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemNumberOfAverages, value);
        }
        /// <summary>
        ///Specifies the number of acquisitions over which the toolkit averages spectral emission mask (SEM) measurements.
        ///    The default value is 1. Valid values are 0 to 1,000, inclusive.
        /// </summary>
        public int GetSemNumberOfAverages(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemNumberOfAverages, out value);
        }
        /// <summary>
        ///Specifies the value (k) that the toolkit uses to compute the measurement filter bandwidth. The measurement bandwidth is equal to (k-1) * RBW. The measurement bandwidth is equal to (k-1) * RBW. If the bandwidth integral for an offset band is set to a value other than 1, the toolkit sets the value of the NILTESA_SEM_OFFSET_BANDS_STEP_FREQUENCY_STATES attribute to NILTESA_VAL_SEM_OFFSET_BANDS_STEP_FREQUENCIES_STATES_AUTO for that offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same. 
        ///    Valid values are 1 to 80, inclusive.
        /// </summary>
        public int SetSemOffsetBandsBandwidthIntegrals(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemOffsetBandsBandwidthIntegrals, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the value (k) that the toolkit uses to compute the measurement filter bandwidth. The measurement bandwidth is equal to (k-1) * RBW. The measurement bandwidth is equal to (k-1) * RBW. If the bandwidth integral for an offset band is set to a value other than 1, the toolkit sets the value of the NILTESA_SEM_OFFSET_BANDS_STEP_FREQUENCY_STATES attribute to NILTESA_VAL_SEM_OFFSET_BANDS_STEP_FREQUENCIES_STATES_AUTO for that offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same. 
        ///    Valid values are 1 to 80, inclusive.
        /// </summary>
        public int GetSemOffsetBandsBandwidthIntegrals(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemOffsetBandsBandwidthIntegrals, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of enum values that enables or disables the offset bands for spectral emission mask (SEM) measurement. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetSemOffsetBandsEnabled(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemOffsetBandsEnabled, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of enum values that enables or disables the offset bands for spectral emission mask (SEM) measurement. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetSemOffsetBandsEnabled(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemOffsetBandsEnabled, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of offset sides for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same. 
        ///    The default value is NILTESA_VAL_SEM_OFFSET_BANDS_OFFSET_SIDES_BOTH. 
        /// </summary>
        public int SetSemOffsetBandsOffsetSides(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemOffsetBandsOffsetSides, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of offset sides for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same. 
        ///    The default value is NILTESA_VAL_SEM_OFFSET_BANDS_OFFSET_SIDES_BOTH. 
        /// </summary>
        public int GetSemOffsetBandsOffsetSides(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemOffsetBandsOffsetSides, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of resolution bandwidths, in hertz (Hz), for each offset band. The toolkit ignores this attribute if you set the    NILTESA_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTH_STATES attribute to NILTESA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_AUTO. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is 0. 
        /// </summary>
        public int SetSemOffsetBandsResolutionBandwidths(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemOffsetBandsResolutionBandwidths, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of resolution bandwidths, in hertz (Hz), for each offset band. The toolkit ignores this attribute if you set the    NILTESA_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTH_STATES attribute to NILTESA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_AUTO. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is 0. 
        /// </summary>
        public int GetSemOffsetBandsResolutionBandwidths(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemOffsetBandsResolutionBandwidths, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of resolution bandwidth (RBW) states for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same. 
        ///    The default value is NILTESA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_MANUAL.
        /// </summary>
        public int SetSemOffsetBandsResolutionBandwidthStates(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemOffsetBandsResolutionBandwidthStates, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of resolution bandwidth (RBW) states for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same. 
        ///    The default value is NILTESA_VAL_SEM_OFFSET_BANDS_RESOLUTION_BANDWIDTHS_STATE_MANUAL.
        /// </summary>
        public int GetSemOffsetBandsResolutionBandwidthStates(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemOffsetBandsResolutionBandwidthStates, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of starting frequency offsets, in hertz (Hz), from the center frequency of the reference channel for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is 0. 
        /// </summary>
        public int SetSemOffsetBandsStartOffsetFrequencies(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemOffsetBandsStartOffsetFrequencies, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of starting frequency offsets, in hertz (Hz), from the center frequency of the reference channel for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is 0. 
        /// </summary>
        public int GetSemOffsetBandsStartOffsetFrequencies(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemOffsetBandsStartOffsetFrequencies, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of end frequency offsets, in hertz (Hz), from the center frequency of the reference channel for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is 0. 
        /// </summary>
        public int SetSemOffsetBandsStopOffsetFrequencies(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemOffsetBandsStopOffsetFrequencies, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of end frequency offsets, in hertz (Hz), from the center frequency of the reference channel for each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        ///    The default value is 0. 
        /// </summary>
        public int GetSemOffsetBandsStopOffsetFrequencies(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemOffsetBandsStopOffsetFrequencies, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of absolute power levels, in dBm, at the beginning of each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        /// </summary>
        public int SetSemStartAbsolutePowersLimits(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemStartAbsolutePowersLimits, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of absolute power levels, in dBm, at the beginning of each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        /// </summary>
        public int GetSemStartAbsolutePowersLimits(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemStartAbsolutePowersLimits, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies the array of absolute power levels, in dBm, at the end of each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        /// </summary>
        public int SetSemStopAbsolutePowersLimits(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemStopAbsolutePowersLimits, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of absolute power levels, in dBm, at the end of each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        /// </summary>
        public int GetSemStopAbsolutePowersLimits(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemStopAbsolutePowersLimits, data, dataSize, out actualNumDataArrayElements);
        }
        /// <summary>
        ///Specifies whether to enable all spectral measurements. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetSpectralMeasurementsAllEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SpectralMeasurementsAllEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable all spectral measurements. 
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetSpectralMeasurementsAllEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SpectralMeasurementsAllEnabled, out value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the LTE uplink signal to analyze.
        ///    The default value is 10 MHz.
        /// </summary>
        public int SetSystemBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SystemBandwidth, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the LTE uplink signal to analyze.
        ///    The default value is 10 MHz.
        /// </summary>
        public int GetSystemBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SystemBandwidth, out value);
        }
        /// <summary>
        ///Indicates the version of the toolkit to which the current version of the toolkit is compatible. 
        /// </summary>
        public int GetToolkitCompatibilityVersion(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ToolkitCompatibilityVersion, out value);
        }
        /// <summary>
        ///Specifies the way in which the cyclic shifts of the physical uplink shared channel (PUSCH) demodulation reference signals (DMRSs) in a slot are configured.
        ///    If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE, the toolkit ignores the NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_FALSE, the toolkit uses the NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE, the toolkit ignores the NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_FALSE, the toolkit uses the NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    The default value is NILTESA_VAL_TRUE.
        /// </summary>
        public int SetUl3gppCyclicShiftEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.Ul3gppCyclicShiftEnabled, value);
        }
        /// <summary>
        ///Specifies the way in which the cyclic shifts of the physical uplink shared channel (PUSCH) demodulation reference signals (DMRSs) in a slot are configured.
        ///    If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE, the toolkit ignores the NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_FALSE, the toolkit uses the NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE, the toolkit ignores the NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_FALSE, the toolkit uses the NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESA_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    The default value is NILTESA_VAL_TRUE.
        /// </summary>
        public int GetUl3gppCyclicShiftEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.Ul3gppCyclicShiftEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable shifting in the physical uplink shared channel (PUSCH) discrete Fourier transform (DFT) precoding. If you enable this attribute, the DC component is at the center of the DFT output.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetUlDftShiftEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlDftShiftEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable shifting in the physical uplink shared channel (PUSCH) discrete Fourier transform (DFT) precoding. If you enable this attribute, the DC component is at the center of the DFT output.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetUlDftShiftEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlDftShiftEnabled, out value);
        }
        /// <summary>
        ///Specifies whether to enable hopping for the uplink signal. The toolkit supports only group and sequence hopping.    Refer to sections 5.5.1.3 and 5.5.1.4 of the 3GPP TS 36.211 v8.6.0 specifications for more information about hopping. If you set this attribute to NILTESA_VAL_TRUE,    you can configure the NILTESA_UL_HOPPING_MODE attribute.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetUlHoppingEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlHoppingEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable hopping for the uplink signal. The toolkit supports only group and sequence hopping.    Refer to sections 5.5.1.3 and 5.5.1.4 of the 3GPP TS 36.211 v8.6.0 specifications for more information about hopping. If you set this attribute to NILTESA_VAL_TRUE,    you can configure the NILTESA_UL_HOPPING_MODE attribute.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetUlHoppingEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlHoppingEnabled, out value);
        }
        /// <summary>
        ///Specifies the hopping mode for the uplink signal. Refer to sections 5.5.1.3 and 5.5.1.4 of the 3GPP TS 36.211 v8.6.0 specifications for more information about hopping modes.    Configure this attribute only if you set the NILTESA_UL_HOPPING_ENABLED attribute to NILTESA_VAL_TRUE.
        ///    The default value is NILTESA_VAL_GROUP_HOPPING.
        /// </summary>
        public int SetUlHoppingMode(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlHoppingMode, value);
        }
        /// <summary>
        ///Specifies the hopping mode for the uplink signal. Refer to sections 5.5.1.3 and 5.5.1.4 of the 3GPP TS 36.211 v8.6.0 specifications for more information about hopping modes.    Configure this attribute only if you set the NILTESA_UL_HOPPING_ENABLED attribute to NILTESA_VAL_TRUE.
        ///    The default value is NILTESA_VAL_GROUP_HOPPING.
        /// </summary>
        public int GetUlHoppingMode(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlHoppingMode, out value);
        }
        /// <summary>
        ///Specifies the number of physical uplink shared channels (PUSCHs) that you can configure in a frame. You can configure one PUSCH channel in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlNumberOfPuschChannels(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlNumberOfPuschChannels, value);
        }
        /// <summary>
        ///Specifies the number of physical uplink shared channels (PUSCHs) that you can configure in a frame. You can configure one PUSCH channel in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlNumberOfPuschChannels(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlNumberOfPuschChannels, out value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in the even slot (ncs0) of the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS). The toolkit ignores this attribute if you set the    NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int SetUlPuschCyclicShiftIndex0(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschCyclicShiftIndex0, value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in the even slot (ncs0) of the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS). The toolkit ignores this attribute if you set the    NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int GetUlPuschCyclicShiftIndex0(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschCyclicShiftIndex0, out value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in the odd slot (ncs1) of the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS). The toolkit ignores this attribute if you set the    NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_FALSE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int SetUlPuschCyclicShiftIndex1(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschCyclicShiftIndex1, value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in the odd slot (ncs1) of the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS). The toolkit ignores this attribute if you set the    NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_FALSE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int GetUlPuschCyclicShiftIndex1(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschCyclicShiftIndex1, out value);
        }
        /// <summary>
        ///Specifies a parameter as defined in section 5.5 of the 3GPP TS 36.211 v8.6.0 specifications to calculate the cyclic shifts and the group and sequence indices associated with the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS). 
        ///    The default value is 0. Valid values are 0 to 29, inclusive.
        /// </summary>
        public int SetUlPuschDeltaSs(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschDeltaSs, value);
        }
        /// <summary>
        ///Specifies a parameter as defined in section 5.5 of the 3GPP TS 36.211 v8.6.0 specifications to calculate the cyclic shifts and the group and sequence indices associated with the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS). 
        ///    The default value is 0. Valid values are 0 to 29, inclusive.
        /// </summary>
        public int GetUlPuschDeltaSs(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschDeltaSs, out value);
        }
        /// <summary>
        ///Specifies the modulation scheme for physical uplink shared channel (PUSCH) transmission.
        ///    Use an active channel string to configure or read this attribute. 
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESA_VAL_MODULATION_SCHEME_64_QAM.
        /// </summary>
        public int SetUlPuschModulationScheme(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschModulationScheme, value);
        }
        /// <summary>
        ///Specifies the modulation scheme for physical uplink shared channel (PUSCH) transmission.
        ///    Use an active channel string to configure or read this attribute. 
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESA_VAL_MODULATION_SCHEME_64_QAM.
        /// </summary>
        public int GetUlPuschModulationScheme(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschModulationScheme, out value);
        }
        /// <summary>
        ///Specifies a parameter as defined in section 5.5 of the 3GPP TS 36.211 v8.6.0 specifications to calculate the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS) cyclic shifts for a specific cell. 
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlPuschNDmrs1(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschNDmrs1, value);
        }
        /// <summary>
        ///Specifies a parameter as defined in section 5.5 of the 3GPP TS 36.211 v8.6.0 specifications to calculate the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS) cyclic shifts for a specific cell. 
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlPuschNDmrs1(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschNDmrs1, out value);
        }
        /// <summary>
        ///Specifies a parameter as defined in section 5.5 of the 3GPP TS 36.211 v8.6.0 specifications to calculate the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS) cyclic shifts for each PUSCH transmission. 
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlPuschNDmrs2(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschNDmrs2, value);
        }
        /// <summary>
        ///Specifies a parameter as defined in section 5.5 of the 3GPP TS 36.211 v8.6.0 specifications to calculate the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS) cyclic shifts for each PUSCH transmission. 
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlPuschNDmrs2(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschNDmrs2, out value);
        }
        /// <summary>
        ///Specifies the number of resource blocks in the frequency domain allocated for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int SetUlPuschNumberOfResourceBlocks(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschNumberOfResourceBlocks, value);
        }
        /// <summary>
        ///Specifies the number of resource blocks in the frequency domain allocated for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        /// </summary>
        public int GetUlPuschNumberOfResourceBlocks(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschNumberOfResourceBlocks, out value);
        }
        /// <summary>
        ///Specifies the physical uplink shared channel (PUSCH) power level, in dB, relative to the power of the PUSCH demodulation reference signal (DMRS).
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetUlPuschPower(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.UlPuschPower, value);
        }
        /// <summary>
        ///Specifies the physical uplink shared channel (PUSCH) power level, in dB, relative to the power of the PUSCH demodulation reference signal (DMRS).
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetUlPuschPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.UlPuschPower, out value);
        }
        /// <summary>
        ///Specifies the starting resource block in the frequency domain for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetUlPuschResourceBlockOffset(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschResourceBlockOffset, value);
        }
        /// <summary>
        ///Specifies the starting resource block in the frequency domain for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetUlPuschResourceBlockOffset(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschResourceBlockOffset, out value);
        }
        /// <summary>
        ///Specifies the subframe number for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int SetUlPuschSubframeNumber(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPuschSubframeNumber, value);
        }
        /// <summary>
        ///Specifies the subframe number for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int GetUlPuschSubframeNumber(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPuschSubframeNumber, out value);
        }

        // New Properties in LTE 1.0.1
        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate sweep time for reference channel power measurements.  If you set this attribute to NILTESA_VAL_FALSE, the toolkit uses the sweep time that you specify in the NILTESA_ACP_REFERENCE_CHANNEL_SWEEP_TIME attribute.
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetAcpReferenceChannelAutoSweepTimeEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpReferenceChannelAutoSweepTimeEnabled, out value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate sweep time for reference channel power measurements.  If you set this attribute to NILTESA_VAL_FALSE, the toolkit uses the sweep time that you specify in the NILTESA_ACP_REFERENCE_CHANNEL_SWEEP_TIME attribute.
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetAcpReferenceChannelAutoSweepTimeEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpReferenceChannelAutoSweepTimeEnabled, value);
        }

        /// <summary>
        ///Specifies the sweep time, in seconds, which you want the toolkit to use for performing reference    channel power measurements.
        ///    The default value is 182.044µ. 
        /// </summary>
        public int GetAcpReferenceChannelSweepTime(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.AcpReferenceChannelSweepTime, out value);
        }

        /// <summary>
        ///Specifies the sweep time, in seconds, which you want the toolkit to use for performing reference    channel power measurements.
        ///    The default value is 182.044µ. 
        /// </summary>
        public int SetAcpReferenceChannelSweepTime(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.AcpReferenceChannelSweepTime, value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate the number of frequency points    (FFT points) for reference channel power measurements. If you set this attribute to NILTESA_VAL_FALSE,    the toolkit uses the number of data points that you specify in the NILTESA_ACP_REFERENCE_CHANNEL_NUM_DATA_POINTS     attribute. 
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetAcpReferenceChannelAutoNumDataPointsEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpReferenceChannelAutoNumDataPointsEnabled, out value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate the number of frequency points    (FFT points) for reference channel power measurements. If you set this attribute to NILTESA_VAL_FALSE,    the toolkit uses the number of data points that you specify in the NILTESA_ACP_REFERENCE_CHANNEL_NUM_DATA_POINTS     attribute. 
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetAcpReferenceChannelAutoNumDataPointsEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpReferenceChannelAutoNumDataPointsEnabled, value);
        }

        /// <summary>
        ///Specifies the number of FFT points to compute for the reference channel in adjacent channel power    (ACP) measurement. This number of FFT points determines the appropriate frequency resolution. The    toolkit ignores this attribute if you set the NILTESA_ACP_REFERENCE_CHANNEL_AUTO_NUM_DATA_POINTS_ENABLED attribute to    NILTESA_VAL_TRUE. 
        ///    The default value is 2,048. 
        /// </summary>
        public int GetAcpReferenceChannelNumDataPoints(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpReferenceChannelNumDataPoints, out value);
        }

        /// <summary>
        ///Specifies the number of FFT points to compute for the reference channel in adjacent channel power    (ACP) measurement. This number of FFT points determines the appropriate frequency resolution. The    toolkit ignores this attribute if you set the NILTESA_ACP_REFERENCE_CHANNEL_AUTO_NUM_DATA_POINTS_ENABLED attribute to    NILTESA_VAL_TRUE. 
        ///    The default value is 2,048. 
        /// </summary>
        public int SetAcpReferenceChannelNumDataPoints(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpReferenceChannelNumDataPoints, value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate the number of FFT segments for    the reference channel in ACP measurements. If you set this attribute to NILTESA_VAL_FALSE, the toolkit    uses the number of FFT segments that you specify in the NILTESA_ACP_REFERENCE_CHANNEL_FFT_SEGMENTS attribute.
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetAcpReferenceChannelAutoNumFftSegmentsEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpReferenceChannelAutoNumFftSegmentsEnabled, out value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate the number of FFT segments for    the reference channel in ACP measurements. If you set this attribute to NILTESA_VAL_FALSE, the toolkit    uses the number of FFT segments that you specify in the NILTESA_ACP_REFERENCE_CHANNEL_FFT_SEGMENTS attribute.
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetAcpReferenceChannelAutoNumFftSegmentsEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpReferenceChannelAutoNumFftSegmentsEnabled, value);
        }

        /// <summary>
        ///Specifies the number of FFT segments that you want the toolkit to use for the reference channel in    adjacent channel power (ACP) measurements.
        ///    The default value is 1. 
        /// </summary>
        public int GetAcpReferenceChannelNumFftSegments(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpReferenceChannelNumFftSegments, out value);
        }

        /// <summary>
        ///Specifies the number of FFT segments that you want the toolkit to use for the reference channel in    adjacent channel power (ACP) measurements.
        ///    The default value is 1. 
        /// </summary>
        public int SetAcpReferenceChannelNumFftSegments(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpReferenceChannelNumFftSegments, value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate sweep time for channel power (CHP)   measurements. 
        ///     The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetChpAutoSweepTimeEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpAutoSweepTimeEnabled, out value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate sweep time for channel power (CHP)   measurements. 
        ///     The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetChpAutoSweepTimeEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpAutoSweepTimeEnabled, value);
        }

        /// <summary>
        ///Specifies the sweep time, in seconds, which you want the toolkit to use for performing channel    power measurements. 
        ///    The default value is 91.022 us. 
        /// </summary>
        public int GetChpSweepTime(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ChpSweepTime, out value);
        }

        /// <summary>
        ///Specifies the sweep time, in seconds, which you want the toolkit to use for performing channel    power measurements. 
        ///    The default value is 91.022 us. 
        /// </summary>
        public int SetChpSweepTime(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.ChpSweepTime, value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate the number of frequency points    (FFT points) for channel power (CHP) measurements. If you set this attribute to NILTESA_VAL_FALSE, the    toolkit uses the number of data points that you specify in the NILTESA_CHP_NUM_DATA_POINTS attribute. 
        ///     The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetChpAutoNumDataPointsEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpAutoNumDataPointsEnabled, out value);
        }

        /// <summary>
        ///Specifies whether you want the toolkit to automatically calculate the number of frequency points    (FFT points) for channel power (CHP) measurements. If you set this attribute to NILTESA_VAL_FALSE, the    toolkit uses the number of data points that you specify in the NILTESA_CHP_NUM_DATA_POINTS attribute. 
        ///     The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetChpAutoNumDataPointsEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpAutoNumDataPointsEnabled, value);
        }

        /// <summary>
        ///Specifies the number of FFT points to compute for the channel power (CHP) measurement. This number    of FFT points determines the appropriate frequency resolution. The toolkit ignores this attribute    if you set the Auto Num Data Point Enabled attribute to NILTESA_VAL_TRUE. 
        ///    The default value is 2,048. 
        /// </summary>
        public int GetChpNumDataPoints(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpNumDataPoints, out value);
        }

        /// <summary>
        ///Specifies the number of FFT points to compute for the channel power (CHP) measurement. This number    of FFT points determines the appropriate frequency resolution. The toolkit ignores this attribute    if you set the Auto Num Data Point Enabled attribute to NILTESA_VAL_TRUE. 
        ///    The default value is 2,048. 
        /// </summary>
        public int SetChpNumDataPoints(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpNumDataPoints, value);
        }

        //get set functions for 2.0 properties
        /// <summary>
        ///Specifies the direction and duplexing technique used in the analyzed frame.
        ///    The default value is uplink NILTESA_VAL_DUPLEX_MODE_UL_FDD.
        /// </summary>
        public int GetDuplexMode(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.DuplexMode, out value);
        }

        /// <summary>
        ///Specifies the direction and duplexing technique used in the analyzed frame.
        ///    The default value is uplink NILTESA_VAL_DUPLEX_MODE_UL_FDD.
        /// </summary>
        public int SetDuplexMode(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.DuplexMode, value);
        }

        /// <summary>
        ///Specifies the uplink/downlink (UL/DL) configuration used in the analyzed frame.
        ///    The default value is 0.
        /// </summary>
        public int GetUlDlConfiguration(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlDlConfiguration, out value);
        }

        /// <summary>
        ///Specifies the uplink/downlink (UL/DL) configuration used in the analyzed frame.
        ///    The default value is 0.
        /// </summary>
        public int SetUlDlConfiguration(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlDlConfiguration, value);
        }

        /// <summary>
        ///Specifies the number of physical uplink control channels (PUCCHs) to transmit in a frame. You can configure one PUCCH channel in each subframe. 
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlNumberOfPucchChannels(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlNumberOfPucchChannels, out value);
        }

        /// <summary>
        ///Specifies the number of physical uplink control channels (PUCCHs) to transmit in a frame. You can configure one PUCCH channel in each subframe. 
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlNumberOfPucchChannels(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlNumberOfPucchChannels, value);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) cyclic shift that the toolkit uses in the even slot. 
        ///    The toolkit ignores this attribute if you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive. 
        /// </summary>
        public int GetUlPucchCyclicShiftIndex0(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchCyclicShiftIndex0, out value);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) cyclic shift that the toolkit uses in the even slot. 
        ///    The toolkit ignores this attribute if you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive. 
        /// </summary>
        public int SetUlPucchCyclicShiftIndex0(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchCyclicShiftIndex0, value);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) cyclic shift that the toolkit uses in the odd slot. 
        ///    The toolkit ignores this attribute if you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive. 
        /// </summary>
        public int GetUlPucchCyclicShiftIndex1(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchCyclicShiftIndex1, out value);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) cyclic shift that the toolkit uses in the odd slot. 
        ///    The toolkit ignores this attribute if you set the NILTESA_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESA_VAL_TRUE. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive. 
        /// </summary>
        public int SetUlPucchCyclicShiftIndex1(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchCyclicShiftIndex1, value);
        }

        /// <summary>
        ///Specifies a parameter used to determine the resource blocks and cyclic shifts assigned for physical uplink control channel (PUCCH) formats as defined in section 5.4 of the 3GPP 36.211 v8.8.0 specifications. 
        ///    The default value is 1. Valid values are 1 to 3, inclusive. 
        /// </summary>
        public int GetUlPucchDeltaPucchShift(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchDeltaPucchShift, out value);
        }

        /// <summary>
        ///Specifies a parameter used to determine the resource blocks and cyclic shifts assigned for physical uplink control channel (PUCCH) formats as defined in section 5.4 of the 3GPP 36.211 v8.8.0 specifications. 
        ///    The default value is 1. Valid values are 1 to 3, inclusive. 
        /// </summary>
        public int SetUlPucchDeltaPucchShift(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchDeltaPucchShift, value);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) format to use. Refer to table 5.4-1 of the 3GPP 36.211 v8.8.0 specifications for more information about PUCCH formats. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESA_VAL_UL_PUCCH_FORMAT_1. 
        /// </summary>
        public int GetUlPucchFormat(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchFormat, out value);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) format to use. Refer to table 5.4-1 of the 3GPP 36.211 v8.8.0 specifications for more information about PUCCH formats. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESA_VAL_UL_PUCCH_FORMAT_1. 
        /// </summary>
        public int SetUlPucchFormat(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchFormat, value);
        }

        /// <summary>
        ///Specifies the number of cyclic shifts used for physical uplink control channel (PUCCH) formats 1/1a/1b in a resource block used for a combination of formats 1/1a/1b and 2/2a/2b. The frame does not contain a mixed resource block if the value of the NILTESA_UL_PUCCH_N_CS_1 attribute is 0. 
        ///    Refer to section 5.4 of the 3GPP 36.211 v8.8.0 specifications for more information about this attribute. 
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int GetUlPucchNCs1(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchNCs1, out value);
        }

        /// <summary>
        ///Specifies the number of cyclic shifts used for physical uplink control channel (PUCCH) formats 1/1a/1b in a resource block used for a combination of formats 1/1a/1b and 2/2a/2b. The frame does not contain a mixed resource block if the value of the NILTESA_UL_PUCCH_N_CS_1 attribute is 0. 
        ///    Refer to section 5.4 of the 3GPP 36.211 v8.8.0 specifications for more information about this attribute. 
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int SetUlPucchNCs1(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchNCs1, value);
        }

        /// <summary>
        ///Specifies a parameter used to determine the resource blocks assigned for physical uplink control channel (PUCCH) formats 1/1a/1b as defined in section 5.4 of the 3GPP 36.211 v8.8.0 specifications. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to N_UL_RBx12-1, inclusive. 
        /// </summary>
        public int GetUlPucchNPucch1(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchNPucch1, out value);
        }

        /// <summary>
        ///Specifies a parameter used to determine the resource blocks assigned for physical uplink control channel (PUCCH) formats 1/1a/1b as defined in section 5.4 of the 3GPP 36.211 v8.8.0 specifications. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to N_UL_RBx12-1, inclusive. 
        /// </summary>
        public int SetUlPucchNPucch1(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchNPucch1, value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the resource block assigned for the physical uplink control channel (PUCCH) formats 2/2a/2b as defined in the 3GPP 36.211 v8.8.0 specifications. 
        ///   Valid values are 0 to N_UL_RB X 12-1, inclusive. 
        /// </summary>
        public int GetUlPucchNPucch2(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchNPucch2, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the resource block assigned for the physical uplink control channel (PUCCH) formats 2/2a/2b as defined in the 3GPP 36.211 v8.8.0 specifications. 
        ///   Valid values are 0 to N_UL_RB X 12-1, inclusive. 
        /// </summary>
        public int SetUlPucchNPucch2(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchNPucch2, value);
        }

        /// <summary>
        ///Specifies the bandwidth, in terms of the number of resource blocks that are available for use by physical uplink control channel (PUCCH) formats 2/2a/2b transmission in each slot. 
        ///    Refer to section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications for more information about this attribute. 
        ///    The default value is 0. Valid values are 0 to N_UL_RB-1, inclusive. 
        /// </summary>
        public int GetUlPucchNRb2(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchNRb2, out value);
        }

        /// <summary>
        ///Specifies the bandwidth, in terms of the number of resource blocks that are available for use by physical uplink control channel (PUCCH) formats 2/2a/2b transmission in each slot. 
        ///    Refer to section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications for more information about this attribute. 
        ///    The default value is 0. Valid values are 0 to N_UL_RB-1, inclusive. 
        /// </summary>
        public int SetUlPucchNRb2(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchNRb2, value);
        }

        /// <summary>
        ///Specifies the power level, in dB, of the physical uplink control channel (PUCCH) data, relative to the PUCCH demodulation reference signal (DMRS) power. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetUlPucchPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.UlPucchPower, out value);
        }

        /// <summary>
        ///Specifies the power level, in dB, of the physical uplink control channel (PUCCH) data, relative to the PUCCH demodulation reference signal (DMRS) power. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetUlPucchPower(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.UlPucchPower, value);
        }

        /// <summary>
        ///Specifies the subframe number for physical uplink control channel (PUCCH) transmission. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    the default value is 0. Valid values are 0 to 9, inclusive. 
        /// </summary>
        public int GetUlPucchSubframeNumber(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlPucchSubframeNumber, out value);
        }

        /// <summary>
        ///Specifies the subframe number for physical uplink control channel (PUCCH) transmission. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    the default value is 0. Valid values are 0 to 9, inclusive. 
        /// </summary>
        public int SetUlPucchSubframeNumber(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlPucchSubframeNumber, value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications.
        ///    The default value is 0. Valid values are 0 to 3, inclusive. 
        /// </summary>
        public int GetUlSrsBSrs(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsBSrs, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications.
        ///    The default value is 0. Valid values are 0 to 3, inclusive. 
        /// </summary>
        public int SetUlSrsBSrs(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsBSrs, value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications.
        ///    The default value is 7. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int GetUlSrsCSrs(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsCSrs, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications.
        ///    The default value is 7. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int SetUlSrsCSrs(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsCSrs, value);
        }

        /// <summary>
        ///Specifies whether to enable the sounding reference signal (SRS).
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetUlSrsEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the sounding reference signal (SRS).
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetUlSrsEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsEnabled, value);
        }

        /// <summary>
        ///Specifies the configuration index that determines the subframes in which the toolkit generates the sounding reference signal (SRS). 
        ///    The default value is 0. Valid values are 0 to 644, inclusive.
        /// </summary>
        public int GetUlSrsISrs(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsISrs, out value);
        }

        /// <summary>
        ///Specifies the configuration index that determines the subframes in which the toolkit generates the sounding reference signal (SRS). 
        ///    The default value is 0. Valid values are 0 to 644, inclusive.
        /// </summary>
        public int SetUlSrsISrs(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsISrs, value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///   The default value is NILTESA_UL_SRS_EVEN_SUBCARRIERS. 
        /// </summary>
        public int GetUlSrsKTc(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsKTc, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///   The default value is NILTESA_UL_SRS_EVEN_SUBCARRIERS. 
        /// </summary>
        public int SetUlSrsKTc(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsKTc, value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.6.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 23, inclusive. 
        /// </summary>
        public int GetUlSrsNRrc(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsNRrc, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.6.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 23, inclusive. 
        /// </summary>
        public int SetUlSrsNRrc(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsNRrc, value);
        }

        /// <summary>
        ///Specifies the cyclic shift on the sounding reference signal (SRS) sequence.
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int GetUlSrsNsrsCs(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsNsrsCs, out value);
        }

        /// <summary>
        ///Specifies the cyclic shift on the sounding reference signal (SRS) sequence.
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int SetUlSrsNsrsCs(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsNsrsCs, value);
        }

        /// <summary>
        ///Specifies the power level, in dB, for the sounding reference signal (SRS). When physical uplink shared channel (PUSCH) or physical uplink control channel (PUCCH) is transmitted along with SRS, this attribute specifies the power level of the SRS, in dB, relative to the PUSCH demodulation reference signal (DMRS) or PUCCH DMRS power.
        ///    The default value is 0. 
        /// </summary>
        public int GetUlSrsPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.UlSrsPower, out value);
        }

        /// <summary>
        ///Specifies the power level, in dB, for the sounding reference signal (SRS). When physical uplink shared channel (PUSCH) or physical uplink control channel (PUCCH) is transmitted along with SRS, this attribute specifies the power level of the SRS, in dB, relative to the PUSCH demodulation reference signal (DMRS) or PUCCH DMRS power.
        ///    The default value is 0. 
        /// </summary>
        public int SetUlSrsPower(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.UlSrsPower, value);
        }

        /// <summary>
        ///Specifies whether the user equipment (UE) is configured to support the simultaneous transmission of    ACK/NACK on physical uplink control channel (PUCCH) and sounding reference signal (SRS) in the same    subframe.n
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetUlSrsSimultaneousAnSrs(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsSimultaneousAnSrs, out value);
        }

        /// <summary>
        ///Specifies whether the user equipment (UE) is configured to support the simultaneous transmission of    ACK/NACK on physical uplink control channel (PUCCH) and sounding reference signal (SRS) in the same    subframe.n
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetUlSrsSimultaneousAnSrs(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsSimultaneousAnSrs, value);
        }

        ///Specifies the subframe configuration index of the sounding reference signal (SRS). This cell-specific attribute specifies the subframes that are reserved to support SRS.
        ///    The default value is 0. Valid values are 0 to 13, inclusive, for time-division duplexing (TDD) and 0 to 14, inclusive, for frequency-division duplexing (FDD). 
        /// </summary>
        public int GetUlSrsSubframeConfigurationIndex(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.UlSrsSubframeConfigurationIndex, out value);
        }

        /// <summary>
        ///Specifies the subframe configuration index of the sounding reference signal (SRS). This cell-specific attribute specifies the subframes that are reserved to support SRS.
        ///    The default value is 0. Valid values are 0 to 13, inclusive, for time-division duplexing (TDD) and 0 to 14, inclusive, for frequency-division duplexing (FDD). 
        /// </summary>
        public int SetUlSrsSubframeConfigurationIndex(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.UlSrsSubframeConfigurationIndex, value);
        }

        /// <summary>
        ///Specifies whether to enable the spectral trace for channel power (CHP) measurement. 
        ///    The CHP measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_CHP_ALL_TRACES_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetChpAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ChpAllTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the spectral trace for channel power (CHP) measurement. 
        ///    The CHP measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_CHP_ALL_TRACES_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetChpAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ChpAllTracesEnabled, value);
        }

        /// <summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter that is used as a matched    filter.
        ///    The default value is 0.22. 
        /// </summary>
        public int GetAcpReferenceChannelRrcFilterAlpha(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.AcpReferenceChannelRrcFilterAlpha, out value);
        }

        /// <summary>
        ///Specifies the roll-off factor of the root raised cosine (RRC) filter that is used as a matched    filter.
        ///    The default value is 0.22. 
        /// </summary>
        public int SetAcpReferenceChannelRrcFilterAlpha(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.AcpReferenceChannelRrcFilterAlpha, value);
        }

        /// <summary>
        ///Specifies whether to enable the root raised cosine (RRC) filter for the reference channel for    adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetAcpReferenceChannelRrcFilterEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpReferenceChannelRrcFilterEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the root raised cosine (RRC) filter for the reference channel for    adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetAcpReferenceChannelRrcFilterEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpReferenceChannelRrcFilterEnabled, value);
        }

        /// <summary>
        ///Specifies whether to enable the spectrum trace for occupied bandwidth (OBW) measurement.
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetObwAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ObwAllTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the spectrum trace for occupied bandwidth (OBW) measurement.
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetObwAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.ObwAllTracesEnabled, value);
        }

        /// <summary>
        ///Specifies the system bandwidth, in hertz (Hz), if you set the NILTESA_SEM_MASK_TYPE attribute to NILTESA_VAL_SEM_MASK_TYPE_CUSTOM.
        ///    The default value is 0M. 
        /// </summary>
        public int GetSemChannelSpan(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemChannelSpan, out value);
        }

        /// <summary>
        ///Specifies the system bandwidth, in hertz (Hz), if you set the NILTESA_SEM_MASK_TYPE attribute to NILTESA_VAL_SEM_MASK_TYPE_CUSTOM.
        ///    The default value is 0M. 
        /// </summary>
        public int SetSemChannelSpan(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemChannelSpan, value);
        }

        /// <summary>
        ///Specifies the type of the measurement results for the spectral emission mask (SEM) measurement. 
        ///    The default value is NILTESA_VAL_SEM_MEASUREMENT_RESULTS_TYPE_TOTAL_POWER_REFERENCE. 
        /// </summary>
        public int GetSemMeasurementResultsType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemMeasurementResultsType, out value);
        }

        /// <summary>
        ///Specifies the type of the measurement results for the spectral emission mask (SEM) measurement. 
        ///    The default value is NILTESA_VAL_SEM_MEASUREMENT_RESULTS_TYPE_TOTAL_POWER_REFERENCE. 
        /// </summary>
        public int SetSemMeasurementResultsType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemMeasurementResultsType, value);
        }

        /// <summary>
        ///Specifies whether to enable the root-raised-cosine (RRC) filter for the reference channel in    spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetSemRrcFilterEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemRrcFilterEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the root-raised-cosine (RRC) filter for the reference channel in    spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetSemRrcFilterEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemRrcFilterEnabled, value);
        }

        /// <summary>
        ///Specifies the integration bandwidth, in hertz (Hz), for the reference channel. 
        ///    The default value is 9M. 
        /// </summary>
        public int GetSemReferenceChannelIntegrationBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemReferenceChannelIntegrationBandwidth, out value);
        }

        /// <summary>
        ///Specifies the integration bandwidth, in hertz (Hz), for the reference channel. 
        ///    The default value is 9M. 
        /// </summary>
        public int SetSemReferenceChannelIntegrationBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemReferenceChannelIntegrationBandwidth, value);
        }

        /// <summary>
        ///Specifies the resolution bandwidth, in hertz (Hz), for the reference channel. The toolkit ignores    this attribute if you set the NILTESA_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE attribute to NILTESA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_AUTO. 
        ///    The default value is 30,000. 
        /// </summary>
        public int GetSemReferenceChannelResolutionBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemReferenceChannelResolutionBandwidth, out value);
        }

        /// <summary>
        ///Specifies the resolution bandwidth, in hertz (Hz), for the reference channel. The toolkit ignores    this attribute if you set the NILTESA_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE attribute to NILTESA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_AUTO. 
        ///    The default value is 30,000. 
        /// </summary>
        public int SetSemReferenceChannelResolutionBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemReferenceChannelResolutionBandwidth, value);
        }

        /// <summary>
        ///Specifies the resolution bandwidth state for the reference channel. 
        ///    The default value is NILTESA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_MANUAL. 
        /// </summary>
        public int GetSemReferenceChannelResolutionBandwidthState(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemReferenceChannelResolutionBandwidthState, out value);
        }

        /// <summary>
        ///Specifies the resolution bandwidth state for the reference channel. 
        ///    The default value is NILTESA_VAL_SEM_REFERENCE_CHANNEL_RESOLUTION_BANDWIDTH_STATE_MANUAL. 
        /// </summary>
        public int SetSemReferenceChannelResolutionBandwidthState(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemReferenceChannelResolutionBandwidthState, value);
        }

        /// <summary>
        ///Specifies the step size, in hertz (Hz), for the RBW filter that is used for the reference channel. The toolkit ignores this attribute if you set the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY_STATE attribute to NILTESA_VAL_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY_STATE_AUTO.
        ///    If you set the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY attribute to a value x, the toolkit coerces the value of the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY attribute to floor (x/2000)*2000, where floor (x) is the greatest integer less than or equal to x. 
        ///    The default value is 15,000. The minimum value of the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY attribute is 2,000.
        /// </summary>
        public int GetSemReferenceChannelStepFrequency(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemReferenceChannelStepFrequency, out value);
        }

        /// <summary>
        ///Specifies the step size, in hertz (Hz), for the RBW filter that is used for the reference channel. The toolkit ignores this attribute if you set the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY_STATE attribute to NILTESA_VAL_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY_STATE_AUTO.
        ///    If you set the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY attribute to a value x, the toolkit coerces the value of the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY attribute to floor (x/2000)*2000, where floor (x) is the greatest integer less than or equal to x. 
        ///    The default value is 15,000. The minimum value of the NILTESA_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY attribute is 2,000.
        /// </summary>
        public int SetSemReferenceChannelStepFrequency(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemReferenceChannelStepFrequency, value);
        }

        /// <summary>
        ///Specifies the step frequency state for the reference channel.
        ///    The default value is NILTESA_VAL_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY_STATE_AUTO.
        /// </summary>
        public int GetSemReferenceChannelStepFrequencyState(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemReferenceChannelStepFrequencyState, out value);
        }

        /// <summary>
        ///Specifies the step frequency state for the reference channel.
        ///    The default value is NILTESA_VAL_SEM_REFERENCE_CHANNEL_STEP_FREQUENCY_STATE_AUTO.
        /// </summary>
        public int SetSemReferenceChannelStepFrequencyState(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemReferenceChannelStepFrequencyState, value);
        }

        /// <summary>
        ///Specifies the array of step frequency states for each offset band. 
        ///    The default value is NILTESA_VAL_SEM_OFFSET_BANDS_STEP_FREQUENCIES_STATES_AUTO. 
        /// </summary>
        public int GetSemOffsetBandsStepFrequencyStates(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemOffsetBandsStepFrequencyStates, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of step frequency states for each offset band. 
        ///    The default value is NILTESA_VAL_SEM_OFFSET_BANDS_STEP_FREQUENCIES_STATES_AUTO. 
        /// </summary>
        public int SetSemOffsetBandsStepFrequencyStates(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemOffsetBandsStepFrequencyStates, value, valueArraySize);
        }

        /// <summary>
        ///Specifies the array of relative attenuations, in dB, for each offset band with respect to the reference channel.
        ///    The default value is 0. 
        /// </summary>
        public int GetSemOffsetBandsRelativeAttenuation(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemOffsetBandsRelativeAttenuation, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of relative attenuations, in dB, for each offset band with respect to the reference channel.
        ///    The default value is 0. 
        /// </summary>
        public int SetSemOffsetBandsRelativeAttenuation(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemOffsetBandsRelativeAttenuation, value, valueArraySize);
        }

        /// <summary>
        ///Specifies the array of absolute stop limit type, for each offset band. If you do not configure any    of the Offset Band attributes, the toolkit dynamically chooses the default values based on the    values of the NILTESA_SEM_MASK_TYPE and NILTESA_SEM_OFFSET_BAND_MASK_STATES attributes. 
        ///    The default value is NILTESA_VAL_SEM_ABSOLUTE_LIMIT_TYPE_SLOPE. 
        /// </summary>
        public int GetSemAbsoluteLimitType(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemAbsoluteLimitType, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of absolute stop limit type, for each offset band. If you do not configure any    of the Offset Band attributes, the toolkit dynamically chooses the default values based on the    values of the NILTESA_SEM_MASK_TYPE and NILTESA_SEM_OFFSET_BAND_MASK_STATES attributes. 
        ///    The default value is NILTESA_VAL_SEM_ABSOLUTE_LIMIT_TYPE_SLOPE. 
        /// </summary>
        public int SetSemAbsoluteLimitType(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemAbsoluteLimitType, value, valueArraySize);
        }

        /// <summary>
        ///Specifies whether to enable all the spectral traces for spectral emission mask (SEM) measurement. 
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int GetSemAllSpectralTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemAllSpectralTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable all the spectral traces for spectral emission mask (SEM) measurement. 
        ///    The default value is NILTESA_VAL_TRUE. 
        /// </summary>
        public int SetSemAllSpectralTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemAllSpectralTracesEnabled, value);
        }

        /// <summary>
        ///Specifies whether to enable all the traces for spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_TRUE.
        /// </summary>
        public int GetSemAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemAllTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable all the traces for spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_TRUE.
        /// </summary>
        public int SetSemAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemAllTracesEnabled, value);
        }

        /// <summary>
        ///Specifies whether to enable the I/Q traces for spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetSemIqTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SemIqTraceEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the I/Q traces for spectral emission mask (SEM) measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetSemIqTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SemIqTraceEnabled, value);
        }

        /// <summary>
        ///Specifies the array of mask states for each offset band for the measurement status    computation.
        ///    The default value is NILTESA_VAL_SEM_MASK_STATES_ABSOLUTE. 
        /// </summary>
        public int GetSemMaskStates(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemMaskStates, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of mask states for each offset band for the measurement status    computation.
        ///    The default value is NILTESA_VAL_SEM_MASK_STATES_ABSOLUTE. 
        /// </summary>
        public int SetSemMaskStates(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemMaskStates, value, valueArraySize);
        }

        /// <summary>
        ///Specifies the array of relative stop limit type for each offset band. 
        ///    The default value is NILTESA_VAL_SEM_RELATIVE_LIMIT_TYPE_SLOPE. 
        /// </summary>
        public int GetSemRelativeLimitType(string channel, int[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetInt32(channel, niLTESAProperties.SemRelativeLimitType, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of relative stop limit type for each offset band. 
        ///    The default value is NILTESA_VAL_SEM_RELATIVE_LIMIT_TYPE_SLOPE. 
        /// </summary>
        public int SetSemRelativeLimitType(string channel, int[] value, int valueArraySize)
        {
            return SetInt32(channel, niLTESAProperties.SemRelativeLimitType, value, valueArraySize);
        }

        /// <summary>
        ///Specifies the array of relative power levels of the SEM measurement limits, in dBc, at the beginning of each offset band. 
        /// </summary>
        public int GetSemStartRelativePowersLimits(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemStartRelativePowersLimits, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of relative power levels of the SEM measurement limits, in dBc, at the beginning of each offset band. 
        /// </summary>
        public int SetSemStartRelativePowersLimits(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemStartRelativePowersLimits, value, valueArraySize);
        }

        /// <summary>
        ///Specifies the array of relative power levels of the SEM measurement limits, in dBc, at the end of each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        /// </summary>
        public int GetSemStopRelativePowersLimits(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemStopRelativePowersLimits, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the array of relative power levels of the SEM measurement limits, in dBc, at the end of each offset band. 
        ///    The toolkit returns an error if the array sizes of all the Offset Bands attributes are not the same.
        /// </summary>
        public int SetSemStopRelativePowersLimits(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemStopRelativePowersLimits, value, valueArraySize);
        }

        /// <summary>
        ///Returns the array of positive side band powers, in dBc, relative to the reference power, for each band.
        /// </summary>
        public int GetResultSemPositiveRelativePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositiveRelativePowers, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Returns the array of negative side band powers, in dBc, relative to the reference power, for each band.
        /// </summary>
        public int GetResultSemNegativeRelativePowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativeRelativePowers, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Returns the array of positive side band peak powers, in dBc, relative to the reference power, for each band.
        /// </summary>
        public int GetResultSemPositiveRelativePeakPowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositiveRelativePeakPowers, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Returns the array of negative side band peak powers, in dBc, relative to the reference power, for each band.
        /// </summary>
        public int GetResultSemNegativeRelativePeakPowers(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativeRelativePeakPowers, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Returns the array of absolute powers, in dBm or dBm/Hz, for each positive side band at the frequency where the worst margin occurs.
        /// </summary>
        public int GetResultSemPositiveAbsolutePowersAtWorstMargin(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemPositiveAbsolutePowersAtWorstMargin, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Returns the array of absolute powers, in dBm or dBm/Hz, for each negative side band at the frequency where the worst margin occurs.
        /// </summary>
        public int GetResultSemNegativeAbsolutePowersAtWorstMargin(string channel, double[] data, int dataSize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultSemNegativeAbsolutePowersAtWorstMargin, data, dataSize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies whether to enable the power versus time (PvT) traces.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetPvtAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.PvtAllTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the power versus time (PvT) traces.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetPvtAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.PvtAllTracesEnabled, value);
        }

        /// <summary>
        ///Specifies the averaging algorithm that the toolkit implements for power versus time (PvT)    measurements. 
        ///    The default value is NILTESA_VAL_PVT_AVERAGING_MODE_RMS_AVERAGING. 
        /// </summary>
        public int GetPvtAveragingMode(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.PvtAveragingMode, out value);
        }

        /// <summary>
        ///Specifies the averaging algorithm that the toolkit implements for power versus time (PvT)    measurements. 
        ///    The default value is NILTESA_VAL_PVT_AVERAGING_MODE_RMS_AVERAGING. 
        /// </summary>
        public int SetPvtAveragingMode(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.PvtAveragingMode, value);
        }

        /// <summary>
        ///Specifies whether to enable power versus time (PvT) measurements on the acquired signal.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetPvtEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.PvtEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable power versus time (PvT) measurements on the acquired signal.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetPvtEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.PvtEnabled, value);
        }

        /// <summary>
        ///Specifies the maximum allowed ramp down time for the test to pass. Refer to section 6.3.4.1 of the    3GPP v36521-1-831 specifications for more information about the test. 
        /// </summary>
        public int GetPvtMaxRampDownTimeLimit(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtMaxRampDownTimeLimit, out value);
        }

        /// <summary>
        ///Specifies the maximum allowed ramp down time for the test to pass. Refer to section 6.3.4.1 of the    3GPP v36521-1-831 specifications for more information about the test. 
        /// </summary>
        public int SetPvtMaxRampDownTimeLimit(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtMaxRampDownTimeLimit, value);
        }

        /// <summary>
        ///Specifies the maximum allowed ramp up time for the test to pass. Refer to section 6.3.4.1 of the    3GPP v36521-1-831 specifications for more information about the test. 
        /// </summary>
        public int GetPvtMaxRampUpTimeLimit(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtMaxRampUpTimeLimit, out value);
        }

        /// <summary>
        ///Specifies the maximum allowed ramp up time for the test to pass. Refer to section 6.3.4.1 of the    3GPP v36521-1-831 specifications for more information about the test. 
        /// </summary>
        public int SetPvtMaxRampUpTimeLimit(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtMaxRampUpTimeLimit, value);
        }

        /// <summary>
        ///Specifies the maximum allowed off power for the test to pass. Refer to section 6.3.4.1 of the 3GPP    v36521-1-831 specifications for more information about the test. 
        /// </summary>
        public int GetPvtMaxTransmitOffPowerLimit(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtMaxTransmitOffPowerLimit, out value);
        }

        /// <summary>
        ///Specifies the maximum allowed off power for the test to pass. Refer to section 6.3.4.1 of the 3GPP    v36521-1-831 specifications for more information about the test. 
        /// </summary>
        public int SetPvtMaxTransmitOffPowerLimit(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtMaxTransmitOffPowerLimit, value);
        }

        /// <summary>
        ///Specifies the interval, in seconds, for power versus time (PvT) measurements.
        /// </summary>
        public int GetPvtMeasurementLength(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtMeasurementLength, out value);
        }

        /// <summary>
        ///Specifies the interval, in seconds, for power versus time (PvT) measurements.
        /// </summary>
        public int SetPvtMeasurementLength(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtMeasurementLength, value);
        }

        /// <summary>
        ///Specifies the number of iterations over which the toolkit averages power versus time (PvT)    measurements.
        ///    The default value is 1. 
        /// </summary>
        public int GetPvtNumberOfAverages(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.PvtNumberOfAverages, out value);
        }

        /// <summary>
        ///Specifies the number of iterations over which the toolkit averages power versus time (PvT)    measurements.
        ///    The default value is 1. 
        /// </summary>
        public int SetPvtNumberOfAverages(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.PvtNumberOfAverages, value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which the ramp down starts.
        /// </summary>
        public int GetPvtRampDownStartLevel(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtRampDownStartLevel, out value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which the ramp down starts.
        /// </summary>
        public int SetPvtRampDownStartLevel(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtRampDownStartLevel, value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which ramp down ends. 
        /// </summary>
        public int GetPvtRampDownStopLevel(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtRampDownStopLevel, out value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which ramp down ends. 
        /// </summary>
        public int SetPvtRampDownStopLevel(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtRampDownStopLevel, value);
        }

        /// <summary>
        ///Specifies the unit to use for the values of the ramp threshold attributes.
        ///    The default value is NILTESA_VAL_PVT_RAMP_THRESHOLD_UNIT_DB. 
        /// </summary>
        public int GetPvtRampThresholdUnit(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.PvtRampThresholdUnit, out value);
        }

        /// <summary>
        ///Specifies the unit to use for the values of the ramp threshold attributes.
        ///    The default value is NILTESA_VAL_PVT_RAMP_THRESHOLD_UNIT_DB. 
        /// </summary>
        public int SetPvtRampThresholdUnit(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.PvtRampThresholdUnit, value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which ramp up starts.
        /// </summary>
        public int GetPvtRampUpStartLevel(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtRampUpStartLevel, out value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which ramp up starts.
        /// </summary>
        public int SetPvtRampUpStartLevel(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtRampUpStartLevel, value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which ramp up ends.
        /// </summary>
        public int GetPvtRampUpStopLevel(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.PvtRampUpStopLevel, out value);
        }

        /// <summary>
        ///Specifies the relative power level from the active slot power at which ramp up ends.
        /// </summary>
        public int SetPvtRampUpStopLevel(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.PvtRampUpStopLevel, value);
        }

        /// <summary>
        ///Returns the average power, in dBm, in the low portion of the TDD signal.
        /// </summary>
        public int GetResultPvtAverageTransmitOffPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtAverageTransmitOffPower, out value);
        }

        /// <summary>
        ///Returns the average power, in dBm, in the high portion of the TDD signal. 
        /// </summary>
        public int GetResultPvtAverageTransmitOnPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtAverageTransmitOnPower, out value);
        }

        /// <summary>
        ///Returns the width of the first active burst in the measurement period. 
        /// </summary>
        public int GetResultPvtBurstWidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtBurstWidth, out value);
        }

        /// <summary>
        ///Indicates the status of the test. Refer to section 6.3.4.1 of the 3GPP v36521-1-831 specifications    for more information about the test.
        /// </summary>
        public int GetResultPvtMeasurementStatus(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ResultPvtMeasurementStatus, out value);
        }

        /// <summary>
        ///Returns the peak power, in dBm, of the signal in the measurement period. 
        /// </summary>
        public int GetResultPvtPeakPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtPeakPower, out value);
        }

        /// <summary>
        ///Returns the measured ramp down time, in seconds. 
        /// </summary>
        public int GetResultPvtRampDownTime(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtRampDownTime, out value);
        }

        /// <summary>
        ///Returns the measured ramp up time, in seconds. 
        /// </summary>
        public int GetResultPvtRampUpTime(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtRampUpTime, out value);
        }

        /// <summary>
        ///Returns the average power, in dBm, of the signal in the measurement period. 
        /// </summary>
        public int GetResultPvtTotalAveragePower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultPvtTotalAveragePower, out value);
        }

        /// <summary>
        ///Specifies whether to return the CCDF measurement traces (for input signal and AWGN) and the I/Q power trace.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetCcdfAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfAllTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to return the CCDF measurement traces (for input signal and AWGN) and the I/Q power trace.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetCcdfAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfAllTracesEnabled, value);
        }

        /// <summary>
        ///Specifies whether to eliminate dead time, if any, present in the TDD signal before performing the complementary cumulative distribution function (CCDF)    measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetCcdfDeadTimeRemovalEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfDeadTimeRemovalEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to eliminate dead time, if any, present in the TDD signal before performing the complementary cumulative distribution function (CCDF)    measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetCcdfDeadTimeRemovalEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfDeadTimeRemovalEnabled, value);
        }

        /// <summary>
        ///Specifies whether to enable the complementary cumulative distribution function (CCDF)    measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetCcdfEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the complementary cumulative distribution function (CCDF)    measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetCcdfEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfEnabled, value);
        }

        /// <summary>
        ///Specifies whether to return the I/Q power trace.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetCcdfIqPowerTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfIqPowerTraceEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to return the I/Q power trace.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetCcdfIqPowerTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfIqPowerTraceEnabled, value);
        }

        /// <summary>
        ///Specifies whether to return the CCDF measurement traces for the input LTE signal and ideal AWGN (for reference).
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetCcdfMeasurementTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfMeasurementTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to return the CCDF measurement traces for the input LTE signal and ideal AWGN (for reference).
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetCcdfMeasurementTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfMeasurementTracesEnabled, value);
        }

        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the RBW filter used. This attribute is applicable even when the NILTESA_CCDF_RBW_FILTER_TYPE attribute is set to NILTESA_VAL_CCDF_RBW_FILTER_TYPE_NONE. 
        ///    The default value is 9 MHz. 
        /// </summary>
        public int GetCcdfRbwFilterBandwidth(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.CcdfRbwFilterBandwidth, out value);
        }

        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the RBW filter used. This attribute is applicable even when the NILTESA_CCDF_RBW_FILTER_TYPE attribute is set to NILTESA_VAL_CCDF_RBW_FILTER_TYPE_NONE. 
        ///    The default value is 9 MHz. 
        /// </summary>
        public int SetCcdfRbwFilterBandwidth(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.CcdfRbwFilterBandwidth, value);
        }

        /// <summary>
        ///Specifies the type of filtering done on the acquired signal before performing the complementary cumulative distribution function (CCDF) measurement.
        ///    The default value is NILTESA_VAL_CCDF_RBW_FILTER_TYPE_NONE. 
        /// </summary>
        public int GetCcdfRbwFilterType(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfRbwFilterType, out value);
        }

        /// <summary>
        ///Specifies the type of filtering done on the acquired signal before performing the complementary cumulative distribution function (CCDF) measurement.
        ///    The default value is NILTESA_VAL_CCDF_RBW_FILTER_TYPE_NONE. 
        /// </summary>
        public int SetCcdfRbwFilterType(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfRbwFilterType, value);
        }

        /// <summary>
        ///Specifies the total number of samples to use for performing the complementary cumulative distribution function (CCDF) measurement.
        ///    The default value is 20000. 
        /// </summary>
        public int GetCcdfSampleCount(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.CcdfSampleCount, out value);
        }

        /// <summary>
        ///Specifies the total number of samples to use for performing the complementary cumulative distribution function (CCDF) measurement.
        ///    The default value is 20000. 
        /// </summary>
        public int SetCcdfSampleCount(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.CcdfSampleCount, value);
        }

        /// <summary>
        ///Returns the average power, in dBm, of the signal. 
        /// </summary>
        public int GetResultCcdfMeanPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfMeanPower, out value);
        }

        /// <summary>
        ///Returns the number of samples for which the instantaneous power is the same as the average power of    the signal, as a percentage of the total number of samples. 
        /// </summary>
        public int GetResultCcdfMeanPowerPercentile(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfMeanPowerPercentile, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.001% of the total samples in the    signal are present. 
        /// </summary>
        public int GetResultCcdfOneHundredThousandthPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfOneHundredThousandthPower, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 1% of the total samples in the signal    are present. 
        /// </summary>
        public int GetResultCcdfOneHundredthPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfOneHundredthPower, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.0001% of the total samples in the    signal are present. 
        /// </summary>
        public int GetResultCcdfOneMillionthPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfOneMillionthPower, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.01% of the total samples in the    signal are present. 
        /// </summary>
        public int GetResultCcdfOneTenThousandthPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfOneTenThousandthPower, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 10% of the total samples in the signal    are present. 
        /// </summary>
        public int GetResultCcdfOneTenthPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfOneTenthPower, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.1% of the total samples in the    signal are present. 
        /// </summary>
        public int GetResultCcdfOneThousandthPower(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfOneThousandthPower, out value);
        }

        /// <summary>
        ///Returns the peak-to-average power ratio (PAPR), in dB, of the signal. 
        /// </summary>
        public int GetResultCcdfPeakToAveragePowerRatio(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.ResultCcdfPeakToAveragePowerRatio, out value);
        }

        /// <summary>
        ///Returns the actual number of data samples after coercion and removal of dead time, used for    complementary cumulative distribution function (CCDF) measurement.
        /// </summary>
        public int GetResultCcdfResultantCount(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.ResultCcdfResultantCount, out value);
        }

        /// <summary>
        ///Specifies the acquisition length for the measurement. The toolkit may specify the acquisition of multiple records of a    specified length, depending on the value of the NILTESA_CCDF_SAMPLE_COUNT attribute. 
        /// </summary>
        public int SetCcdfMeasurementLength(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.CcdfMeasurementLength, value);
        }

        /// <summary>
        ///Specifies the acquisition length for the measurement. The toolkit may specify the acquisition of multiple records of a    specified length, depending on the value of the NILTESA_CCDF_SAMPLE_COUNT attribute. 
        /// </summary>
        public int GetCcdfMeasurementLength(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.CcdfMeasurementLength, out value);
        }

        /// <summary>
        ///Specifies the offset frequency of the reference channel, in hertz (Hz). 
        /// </summary>
        public int SetSemReferenceChannelOffsetFrequency(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemReferenceChannelOffsetFrequency, value);
        }
        /// <summary>
        ///Specifies the offset frequency of the reference channel, in hertz (Hz). 
        /// </summary>
        public int GetSemReferenceChannelOffsetFrequency(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemReferenceChannelOffsetFrequency, out value);
        }

        /// <summary>
        ///Specifies the array of step frequencies for each offset band. The toolkit ignores this attribute if you set the NILTESA_SEM_OFFSET_BANDS_STEP_FREQUENCY_STATES attribute to NILTESA_VAL_SEM_OFFSET_BANDS_STEP_FREQUENCIES_STATES_AUTO. 
        /// </summary>
        public int SetSemOffsetBandsStepFrequencies(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.SemOffsetBandsStepFrequencies, value, valueArraySize);
        }
        /// <summary>
        ///Specifies the array of step frequencies for each offset band. The toolkit ignores this attribute if you set the NILTESA_SEM_OFFSET_BANDS_STEP_FREQUENCY_STATES attribute to NILTESA_VAL_SEM_OFFSET_BANDS_STEP_FREQUENCIES_STATES_AUTO. 
        /// </summary>
        public int GetSemOffsetBandsStepFrequencies(string channel, double[] value, int valueArraySize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.SemOffsetBandsStepFrequencies, value, valueArraySize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies whether to enable the spectral trace for  adjacent channel power (ACP) measurement.
        ///    The ACP measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_ACP_ALL_TRACES_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetAcpAllSpectralTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpAllSpectralTraceEnabled, value);
        }

        /// <summary>
        ///Specifies whether to enable the spectral trace for  adjacent channel power (ACP) measurement.
        ///    The ACP measurement is enabled when the NILTESA_SPECTRAL_MEASUREMENTS_ALL_ENABLED or NILTESA_ACP_ALL_TRACES_ENABLED attribute is set to NILTESA_VAL_TRUE. 
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetAcpAllSpectralTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpAllSpectralTraceEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable all the traces for adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetAcpAllTracesEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpAllTracesEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable all the traces for adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetAcpAllTracesEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpAllTracesEnabled, out value);
        }

        /// <summary>
        ///Specifies whether to enable the I/Q trace for  adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int SetAcpIqTraceEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpIqTraceEnabled, value);
        }

        /// <summary>
        ///Specifies whether to enable the I/Q trace for  adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE. 
        /// </summary>
        public int GetAcpIqTraceEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpIqTraceEnabled, out value);
        }

        /// <summary>
        ///Specifies an array of absolute noise powers, in dBm or dBm per hertz (dBm/Hz), for reference and offset channels. The toolkit subtracts noise powers from the measured channel powers if you set the NILTESA_ACP_NOISE_COMPENSATION_ENABLED attribute to NILTESA_VAL_FALSE.
        ///    The sequence of powers in the Noise Floors array is: Reference Channel, Reference Channel, Negative Power [0], Positive Power[0], Negative Power[1], Positive Power[1] and so on. 
        /// </summary>
        public int SetAcpNoiseFloors(string channel, double[] value, int valueArraySize)
        {
            return SetDouble(channel, niLTESAProperties.AcpNoiseFloors, value, valueArraySize);
        }

        /// <summary>
        ///Specifies an array of absolute noise powers, in dBm or dBm per hertz (dBm/Hz), for reference and offset channels. The toolkit subtracts noise powers from the measured channel powers if you set the NILTESA_ACP_NOISE_COMPENSATION_ENABLED attribute to NILTESA_VAL_FALSE.
        ///    The sequence of powers in the Noise Floors array is: Reference Channel, Reference Channel, Negative Power [0], Positive Power[0], Negative Power[1], Positive Power[1] and so on. 
        /// </summary>
        public int GetAcpNoiseFloors(string channel, double[] data, int dataArraySize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.AcpNoiseFloors, data, dataArraySize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies whether to enable noise compensation for the adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int SetAcpNoiseCompensationEnabled(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.AcpNoiseCompensationEnabled, value);
        }
        /// <summary>
        ///Specifies whether to enable noise compensation for the adjacent channel power (ACP) measurement.
        ///    The default value is NILTESA_VAL_FALSE.
        /// </summary>
        public int GetAcpNoiseCompensationEnabled(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.AcpNoiseCompensationEnabled, out value);
        }

        /// <summary>
        ///Returns an array of absolute powers, in dBm or dBm per hertz (dBm/Hz), of the reference channel, negative sideband adjacent channels, and positive sideband adjacent channels. The sequence of powers in the Absolute Powers array is: Reference Channel, Reference Channel, Negative Power [0], Positive Power[0], Negative Power[1], Positive Power[1] and so on.
        /// </summary>
        public int GetResultAcpAbsolutePowers(string channel, double[] data, int dataArraySize, out int actualNumDataArrayElements)
        {
            return GetDouble(channel, niLTESAProperties.ResultAcpAbsolutePowers, data, dataArraySize, out actualNumDataArrayElements);
        }

        /// <summary>
        ///Specifies the system frame number.   The default value is 0.
        /// </summary>
        public int SetSystemFrameNumber(string channel, int value)
        {
            return SetInt32(channel, niLTESAProperties.SystemFrameNumber, value);
        }

        /// <summary>
        ///Specifies the system frame number.   The default value is 0.
        /// </summary>
        public int GetSystemFrameNumber(string channel, out int value)
        {
            return GetInt32(channel, niLTESAProperties.SystemFrameNumber, out value);
        }

        /// <summary>
        ///Specifies the roll-off factor for the root-raised-cosine (RRC) filter. 
        ///    The default value is 0.22. 
        /// </summary>
        public int SetSemRrcFilterAlpha(string channel, double value)
        {
            return SetDouble(channel, niLTESAProperties.SemRrcFilterAlpha, value);
        }
        
        /// <summary>
        ///Specifies the roll-off factor for the root-raised-cosine (RRC) filter. 
        ///    The default value is 0.22. 
        /// </summary>
        public int GetSemRrcFilterAlpha(string channel, out double value)
        {
            return GetDouble(channel, niLTESAProperties.SemRrcFilterAlpha, out value);
        }

        private int SetInt32(string channelString, niLTESAProperties attributeID, int attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESA_SetScalarAttributeI32(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int SetInt32(string channelString, niLTESAProperties attributeID, int[] data, int dataArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_SetVectorAttributeI32(Handle, channelString, attributeID, data, dataArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetInt32(string channelString, niLTESAProperties attributeID, out int attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESA_GetScalarAttributeI32(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetInt32(string channelString, niLTESAProperties attributeID, int[] data, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_GetVectorAttributeI32(Handle, channelString, attributeID, data, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int SetDouble(string channelString, niLTESAProperties attributeID, double attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESA_SetScalarAttributeF64(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int SetDouble(string channelString, niLTESAProperties attributeID, double[] data, int dataArraySize)
        {
            int pInvokeResult = PInvoke.niLTESA_SetVectorAttributeF64(Handle, channelString, attributeID, data, dataArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetDouble(string channelString, niLTESAProperties attributeID, double[] data, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESA_GetVectorAttributeF64(Handle, channelString, attributeID, data, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetDouble(string channelString, niLTESAProperties attributeID, out double attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESA_GetScalarAttributeF64(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
            }
            // Dispose() does not close a named session. Users must call CloseSession() to close a named session.
            if (!_isNamedSession)
            {
                // Dispose unmanaged resources
                // Handle.Handle is IntPtr.Zero when the session is inactive/closed.
                if (!Handle.Handle.Equals(IntPtr.Zero))
                {
                    PInvoke.niLTESA_CloseSession(Handle);
                }
            }
        }

        private class PInvoke
        {
            const string nativeDllName = "niLTESA_net.dll";

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ACPGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ACPGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_AnalyzeIQComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_AnalyzeIQComplexF64(HandleRef session, double t0, double dt, niComplexNumber[] waveform, int numberofSamples, int reset, out int averagingDone);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_AnalyzePowerSpectrum", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_AnalyzePowerSpectrum(HandleRef session, double f0, double df, double[] spectrum, int powerSpectrumArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_CheckToolkitError", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_CheckToolkitError(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_CHPGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_CHPGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_CloseSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ConfigureFullyFilledPUSCHChannels", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ConfigureFullyFilledPUSCHChannels(HandleRef session, int pUSCHModulationScheme, double systemBandwidth);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_EARFCNtoCarrierFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_EARFCNtoCarrierFrequency(int eARFCN, int reserved, out double carrierFrequency);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_GetErrorString(HandleRef session, int errorCode, StringBuilder errorMessage, int errorMessageLength);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_GetScalarAttributeF64(HandleRef session, string channelString, niLTESAProperties attributeID, out double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_GetScalarAttributeI32(HandleRef session, string channelString, niLTESAProperties attributeID, out int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_GetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_GetVectorAttributeF64(HandleRef session, string channelString, niLTESAProperties attributeID, [Out] double[] data, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_GetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_GetVectorAttributeI32(HandleRef session, string channelString, niLTESAProperties attributeID, [Out] int[] data, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationDataConstellationTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationDataConstellationTrace(HandleRef session, string channelString, [Out] double[] iData, [Out] double[] qData, int length, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationDMRSConstellationTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationDMRSConstellationTrace(HandleRef session, string channelString, [Out] double[] iData, [Out] double[] qData, int length, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationEVMPerResourceBlockTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationEVMPerResourceBlockTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] eVMperResourceBlock, int eVMperRBArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationEVMPerSlotTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationEVMPerSlotTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] eVMperSlot, int eVMperSlotArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationEVMPerSubcarrierTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationEVMPerSubcarrierTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] eVMperSubcarrier, int eVMperSubcarrierArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationEVMPerSymbolPerSubcarrierTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationEVMPerSymbolPerSubcarrierTrace(HandleRef session, string channelString, [Out] double[,] eVMperSymbolperSubcarrier, int numRows, int numColumns, out int actualNumRows, out int actualNumColumns);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationEVMPerSymbolTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationEVMPerSymbolTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] eVMperSymbol, int eVMperSymbolArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationSpectralFlatnessTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationSpectralFlatnessTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] spectralFlatness, int spectralFlatnessArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_OBWGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_OBWGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_OpenSession(string sessionName, int compatibilityVersion, out IntPtr session, out int isNewSession);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ResetAttribute(HandleRef session, string channelString, niLTESAProperties attributeID);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ResetSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_RFSAAutoLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_RFSAAutoLevel(System.Runtime.InteropServices.HandleRef rFSASession, string hardwareChannelString, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_RFSAConfigureHardware", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_RFSAConfigureHardware(HandleRef lTESession, string lTEChannelString, System.Runtime.InteropServices.HandleRef rFSASession, string hardwareChannelString, out Int64 samplesPerRecord);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_RFSAMeasure", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_RFSAMeasure(HandleRef lTESession, System.Runtime.InteropServices.HandleRef rFSASession, string hardwareChannelString, double timeOut);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_RFSAReadGatedPowerSpectrum", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_RFSAReadGatedPowerSpectrum(HandleRef lTESession, System.Runtime.InteropServices.HandleRef rFSASession, string hardwareChannelString, double timeout, out double f0, out double df, [Out] double[] powerSpectrum, int powerSpectrumArraySize, out int actualNumSpectrumElement);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SelectMeasurements", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SelectMeasurements(HandleRef session, UInt32 measurement);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SelectMeasurementsWithTraces", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SelectMeasurementsWithTraces(HandleRef session, UInt32 measurement, int enableTraces);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SEMGetAbsoluteLimitTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SEMGetAbsoluteLimitTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] absoluteLimits, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SEMGetSpectrumTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SEMGetSpectrumTrace(HandleRef session, string channelString, out double f0, out double df, [Out] double[] spectrum, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SetScalarAttributeF64(HandleRef session, string channelString, niLTESAProperties attributeID, double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SetScalarAttributeI32(HandleRef session, string channelString, niLTESAProperties attributeID, int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SetVectorAttributeF64(HandleRef session, string channelString, niLTESAProperties attributeID, double[] data, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SetVectorAttributeI32(HandleRef session, string channelString, niLTESAProperties attributeID, int[] data, int dataArraySize);

            //2.0 PInvokes
            [DllImport(nativeDllName, EntryPoint = "niLTESA_ConfigureFullyFilledPUCCHFrame", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ConfigureFullyFilledPUCCHFrame(HandleRef session, int duplexMode, int pUCCHFormat, int n_PUCCH_1, double pUCCHpowerdB, int uLDLConfiguration);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ConfigureFullyFilledPUSCHFrame", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ConfigureFullyFilledPUSCHFrame(HandleRef session, int duplexMode, int pUSCHModulationScheme, double systemBandwidth, int uLDLConfiguration);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SaveConfigurationToFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SaveConfigurationToFile(HandleRef session, string filePath, int operation);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_LoadConfigurationFromFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_LoadConfigurationFromFile(HandleRef session, string filePath, int resetSession);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ReadWaveformFromFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ReadWaveformFromFile(string filePath, string waveformName, Int64 offset, Int64 count, out double t0, out double dt, [Out] niComplexNumber[] waveform, int dataArraySize, out int actualNumDataArrayElements, out int eof);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_CCDFGetCurrentIterationGaussianProbabilitiesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_CCDFGetCurrentIterationGaussianProbabilitiesTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] data, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_CCDFGetCurrentIterationIQPowerTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_CCDFGetCurrentIterationIQPowerTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] data, int dataArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_CCDFGetCurrentIterationProbabilitiesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_CCDFGetCurrentIterationProbabilitiesTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] data, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_PvTGetCurrentIterationMaskTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_PvTGetCurrentIterationMaskTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] data, int dataArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_PvTGetCurrentIterationPvTTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_PvTGetCurrentIterationPvTTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] data, int dataArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ModAccGetCurrentIterationSRSConstellationTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ModAccGetCurrentIterationSRSConstellationTrace(HandleRef session, string channelString, [Out] double[] iData, [Out] double[] qData, int length, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SEMGetCurrentIterationIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SEMGetCurrentIterationIQWaveformTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] niComplexNumber[] waveform, int dataArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_ACPGetCurrentIterationIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_ACPGetCurrentIterationIQWaveformTrace(HandleRef session, string channelString, out double t0, out double dt, [Out] niComplexNumber[] waveform, int dataArraySize, out int actualArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESA_SEMGetRelativeLimitTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESA_SEMGetRelativeLimitTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] relativeLimits, int dataArraySize, out int actualNumDataArrayElements);

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

        private static int TestForStaticError(int status)
        {
            if (status < 0)
            {
                int size = 0;
                HandleRef dummyHandle = new HandleRef();
                StringBuilder msg = new StringBuilder();

                size = PInvoke.niLTESA_GetErrorString(dummyHandle, status, null, size);
                if ((size >= 0))
                {
                    msg.Capacity = size;
                    PInvoke.niLTESA_GetErrorString(dummyHandle, status, msg, size);
                }
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
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
            int size = PInvoke.niLTESA_GetErrorString(Handle, status, null, 0);
            if ((size >= 0))
            {
                msg.Capacity = size;
                PInvoke.niLTESA_GetErrorString(Handle, status, msg, size);
            }
            return status;
        }
    }

    public class niLTESAConstants
    {
        public const int CyclicPrefixModeNormal = 0;

        public const int CyclicPrefixModeExtended = 1;

        public const int RecommendedAcquisitionTypeIq = 0;

        public const int RecommendedAcquisitionTypeSpectrum = 1;

        public const int RecommendedSpectralAcquisitionFftWindowType = 0;

        public const int False = 0;

        public const int True = 1;

        public const int GroupHopping = 0;

        public const int SequenceHopping = 1;

        public const int PuschModulationSchemeQpsk = 0;

        public const int PuschModulationScheme16qam = 1;

        public const int PuschModulationScheme64qam = 2;

        public const int MeasurementModeAnySlot = 0;

        public const int MeasurementModeSpecificSlot = 1;

        public const int EvmUnitDb = 0;

        public const int EvmUnitPercentageRms = 1;

        public const int ChannelEstimationTypeReference = 0;

        public const int ChannelEstimationTypeReferenceAndData = 1;

        public const int FftWindowType3gpp = 0;

        public const int FftWindowTypeCustom = 1;

        public const int ChpSpanTypeStandard = 0;

        public const int ChpSpanTypeCustom = 1;

        public const int AcpAverageTypeLinear = 0;

        public const int AcpAverageTypePeakHold = 1;

        public const int AcpMeasurementResultsTypeTotalPowerReference = 0;

        public const int AcpMeasurementResultsTypePowerSpectralDensityReference = 1;

        public const int AcpFrequencyListTypeStandard = 0;

        public const int AcpFrequencyListTypeCustom = 1;

        public const int ObwSpanTypeStandard = 0;

        public const int ObwSpanTypeCustom = 1;

        public const int ObwResolutionBandwidthFilterTypeGaussian = 0;

        public const int ObwResolutionBandwidthFilterTypeFlat = 1;

        public const int ObwResolutionBandwidthFilterTypeNone = 2;

        public const int SemAverageTypeLinear = 0;

        public const int SemAverageTypePeakHold = 1;

        public const int SemMaskTypeGeneral = 0;

        public const int SemMaskTypeNs03 = 1;

        public const int SemMaskTypeNs04 = 2;

        public const int SemMaskTypeNs06OrNs07 = 3;

        public const int SemMaskTypeCustom = 4;

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

        public const uint ModAccMeasurement = 1;

        public const uint ChpMeasurement = 2;

        public const uint AcpMeasurement = 4;

        public const uint ObwMeasurement = 8;

        public const uint SemMeasurement = 16;

        public const uint PvtMeasurement = 32;

        public const uint CcdfMeasurement = 64;

        [Obsolete]
        public const int LTESAMaxErrorStringSize = 1024;

        public const int MaxErrorStringSize = 1024;

        //2.0 constants
        public const int DuplexModeUlFdd = 1;

        public const int DuplexModeUlTdd = 3;

        public const int SemMeasurementResultsTypeTotalPowerReference = 0;

        public const int SemMeasurementResultsTypePowerSpectralDensityReference = 1;

        public const int SemReferenceChannelStepFrequencyStateManual = 0;

        public const int SemReferenceChannelStepFrequencyStateAuto = 1;

        public const int SemReferenceChannelResolutionBandwidthStateManual = 0;

        public const int SemReferenceChannelResolutionBandwidthStateAuto = 1;

        public const int SemOffsetBandsStepFrequenciesStatesManual = 0;

        public const int SemOffsetBandsStepFrequenciesStatesAuto = 1;

        public const int SemMaskStatesAbsolute = 0;

        public const int SemMaskStatesRelative = 1;

        public const int SemMaskStatesAbsoluteAndRelative = 2;

        public const int SemMaskStatesAbsoluteOrRelative = 3;

        public const int SemAbsoluteLimitTypeSlope = 0;

        public const int SemAbsoluteLimitTypeFlat = 1;

        public const int SemRelativeLimitTypeSlope = 0;

        public const int SemRelativeLimitTypeFlat = 1;

        public const int PvtAveragingModeRmsAveraging = 0;

        public const int PvtAveragingModeLogAveraging = 1;

        public const int PvtAveragingModePeakHoldAveraging = 2;

        public const int PvtAveragingModeMinimumHoldAveraging = 3;

        public const int PvtRampThresholdUnitDb = 0;

        public const int PvtRampThresholdUnitPercentage = 1;

        public const int CcdfRbwFilterTypeNone = 0;

        public const int CcdfRbwFilterTypeGaussian = 1;

        public const int CcdfRbwFilterTypeFlatTop = 2;

        public const int UlPucchFormat1 = 0;

        public const int UlPucchFormat1A = 1;

        public const int UlPucchFormat1B = 2;

        public const int UlPucchFormat2 = 3;

        public const int UlPucchFormat2A = 4;

        public const int UlPucchFormat2B = 5;

        public const int UlSrsEvenSubcarriers = 0;

        public const int UlSrsOddSubcarriers = 1;

        public const int FileOperationModeOpen = 0;

        public const int FileOperationModeOpenOrCreate = 1;

        public const int FileOperationModeCreateOrReplace = 2;

        public const int FileOperationModeCreate = 3;

    }

    public enum niLTESAProperties
    {
        /// <summary>
        /// double[]
        /// </summary>
        AcpAdjacentChannelsBandwidths = 231,

        /// <summary>
        /// int[]
        /// </summary>
        AcpAdjacentChannelsEnabled = 269,

        /// <summary>
        /// double[]
        /// </summary>
        AcpAdjacentChannelsFrequencyOffsets = 232,

        /// <summary>
        /// double[]
        /// </summary>
        AcpAdjacentChannelsRrcFilterAlpha = 307,

        /// <summary>
        /// int[]
        /// </summary>
        AcpAdjacentChannelsRrcFilterEnabled = 306,

        /// <summary>
        /// int[]
        /// </summary>
        AcpAdjacentChannelsSidebands = 270,

        /// <summary>
        /// int
        /// </summary>
        AcpAverageType = 297,

        /// <summary>
        /// int
        /// </summary>
        AcpEnabled = 227,

        /// <summary>
        /// int
        /// </summary>
        AcpFrequencyListType = 251,

        /// <summary>
        /// int
        /// </summary>
        AcpMeasurementResultsType = 299,

        /// <summary>
        /// int
        /// </summary>
        AcpNumberOfAverages = 298,

        /// <summary>
        /// double
        /// </summary>
        AcpReferenceChannelBandwidth = 230,

        /// <summary>
        /// int
        /// </summary>
        CellId = 14,

        /// <summary>
        /// int
        /// </summary>
        ChpEnabled = 280,

        /// <summary>
        /// double
        /// </summary>
        ChpMeasurementBandwidth = 285,

        /// <summary>
        /// int
        /// </summary>
        ChpNumberOfAverages = 284,

        /// <summary>
        /// double
        /// </summary>
        ChpSpan = 281,

        /// <summary>
        /// int
        /// </summary>
        ChpSpanType = 282,

        /// <summary>
        /// int
        /// </summary>
        CyclicPrefixMode = 13,

        /// <summary>
        /// double
        /// </summary>
        HardwareSettingsCarrierFrequency = 2,

        /// <summary>
        /// double
        /// </summary>
        HardwareSettingsMaxRealtimeBandwidth = 392,

        /// <summary>
        /// double
        /// </summary>
        HardwareSettingsTriggerDelay = 397,

        /// <summary>
        /// int
        /// </summary>
        ModaccAllTracesEnabled = 278,

        /// <summary>
        /// int
        /// </summary>
        ModaccAutoRbDetectionEnabled = 344,

        /// <summary>
        /// int
        /// </summary>
        ModaccChannelEstimationType = 101,

        /// <summary>
        /// int
        /// </summary>
        ModaccCommonClockSource = 103,

        /// <summary>
        /// int
        /// </summary>
        ModaccConstellationTraceEnabled = 276,

        /// <summary>
        /// int
        /// </summary>
        ModaccEnabled = 90,

        /// <summary>
        /// int
        /// </summary>
        ModaccEvmMeasurementUnit = 268,

        /// <summary>
        /// int
        /// </summary>
        ModaccEvmPerRbTraceEnabled = 95,

        /// <summary>
        /// int
        /// </summary>
        ModaccEvmPerSlotTraceEnabled = 93,

        /// <summary>
        /// int
        /// </summary>
        ModaccEvmPerSubcarrierTraceEnabled = 94,

        /// <summary>
        /// int
        /// </summary>
        ModaccEvmPerSymbolPerSubcarrierTraceEnabled = 96,

        /// <summary>
        /// int
        /// </summary>
        ModaccEvmPerSymbolTraceEnabled = 92,

        /// <summary>
        /// double
        /// </summary>
        ModaccFftWindowLength = 106,

        /// <summary>
        /// double
        /// </summary>
        ModaccFftWindowPosition = 105,

        /// <summary>
        /// int
        /// </summary>
        ModaccFftWindowType = 107,

        /// <summary>
        /// int
        /// </summary>
        ModaccMeasurementLength = 401,

        /// <summary>
        /// int
        /// </summary>
        ModaccMeasurementMode = 413,

        /// <summary>
        /// int
        /// </summary>
        ModaccMeasurementOffset = 400,

        /// <summary>
        /// int
        /// </summary>
        ModaccNumberOfAverages = 91,

        /// <summary>
        /// int
        /// </summary>
        ModaccSpectralFlatnessTraceEnabled = 255,

        /// <summary>
        /// int
        /// </summary>
        ObwEnabled = 233,

        /// <summary>
        /// int
        /// </summary>
        ObwNumberOfAverages = 326,

        /// <summary>
        /// double
        /// </summary>
        ObwResolutionBandwidth = 329,

        /// <summary>
        /// int
        /// </summary>
        ObwResolutionBandwidthFilterType = 328,

        /// <summary>
        /// double
        /// </summary>
        ObwSpan = 323,

        /// <summary>
        /// int
        /// </summary>
        ObwSpanType = 324,

        /// <summary>
        /// double
        /// </summary>
        RecommendedAcquisitionLength = 4,

        /// <summary>
        /// int
        /// </summary>
        RecommendedAcquisitionType = 393,

        /// <summary>
        /// double
        /// </summary>
        RecommendedIqAcquisitionIqRate = 5,

        /// <summary>
        /// double
        /// </summary>
        RecommendedIqAcquisitionMinimumQuietTime = 8,

        /// <summary>
        /// int
        /// </summary>
        RecommendedIqAcquisitionNumberOfRecords = 267,

        /// <summary>
        /// double
        /// </summary>
        RecommendedIqAcquisitionPosttriggerDelay = 10,

        /// <summary>
        /// double
        /// </summary>
        RecommendedIqAcquisitionPretriggerDelay = 9,

        /// <summary>
        /// int
        /// </summary>
        RecommendedSpectralAcquisitionFftWindowType = 214,

        /// <summary>
        /// double
        /// </summary>
        RecommendedSpectralAcquisitionRbw = 212,

        /// <summary>
        /// double
        /// </summary>
        RecommendedSpectralAcquisitionSpan = 213,

        /// <summary>
        /// double[]
        /// </summary>
        ResultAcpNegativeAbsolutePowers = 318,

        /// <summary>
        /// double[]
        /// </summary>
        ResultAcpNegativeRelativePowers = 317,

        /// <summary>
        /// double[]
        /// </summary>
        ResultAcpPositiveAbsolutePowers = 320,

        /// <summary>
        /// double[]
        /// </summary>
        ResultAcpPositiveRelativePowers = 319,

        /// <summary>
        /// double
        /// </summary>
        ResultAcpReferenceChannelPower = 316,

        /// <summary>
        /// double
        /// </summary>
        ResultChpChannelPower = 292,

        /// <summary>
        /// double
        /// </summary>
        ResultChpChannelPowerSpectralDensity = 293,

        /// <summary>
        /// double[]
        /// </summary>
        ResultModaccAbsoluteInBandEmissions = 188,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccAverageCarrierFrequencyOffset = 195,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccAverageIqGainImbalance = 207,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccAverageIqOffset = 203,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccAverageQuadratureSkew = 199,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccAverageSampleClockOffset = 191,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccCarrierFrequencyOffsetStandardDeviation = 198,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccChannelDataEvmStandardDeviation = 151,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccChannelDataPeakEvm = 150,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccChannelDataRmsEvm = 148,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccChannelEvmStandardDeviation = 175,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccChannelPeakEvm = 174,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccChannelRmsEvm = 172,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccIqGainImbalanceStandardDeviation = 210,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccIqOffsetStandardDeviation = 206,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMaximumCarrierFrequencyOffset = 197,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMaximumIqGainImbalance = 209,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMaximumIqOffset = 205,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMaximumQuadratureSkew = 201,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMaximumSampleClockOffset = 193,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMinimumCarrierFrequencyOffset = 196,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMinimumIqGainImbalance = 208,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMinimumIqOffset = 204,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMinimumQuadratureSkew = 200,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccMinimumSampleClockOffset = 192,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccQuadratureSkewStandardDeviation = 202,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccRelativeInBandEmissionsDc = 190,

        /// <summary>
        /// double[]
        /// </summary>
        ResultModaccRelativeInBandEmissionsGeneralIqImage = 189,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccSampleClockOffsetStandardDeviation = 194,

        /// <summary>
        /// int
        /// </summary>
        ResultModaccSyncFound = 275,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccUlDmrsEvmStandardDeviation = 155,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccUlDmrsPeakEvm = 154,

        /// <summary>
        /// double
        /// </summary>
        ResultModaccUlDmrsRmsEvm = 152,

        /// <summary>
        /// double
        /// </summary>
        ResultObwCarrierPower = 334,

        /// <summary>
        /// double
        /// </summary>
        ResultObwOccupiedBandwidth = 333,

        /// <summary>
        /// double
        /// </summary>
        ResultObwStartFrequency = 336,

        /// <summary>
        /// double
        /// </summary>
        ResultObwStopFrequency = 335,

        /// <summary>
        /// int
        /// </summary>
        ResultSemMeasurementStatus = 372,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativeAbsolutePeakPowers = 380,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativeAbsolutePowers = 376,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativePeakFrequencies = 384,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativePowerMargins = 382,

        /// <summary>
        /// double
        /// </summary>
        ResultSemPeakReferenceFrequency = 371,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositiveAbsolutePeakPowers = 379,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositiveAbsolutePowers = 375,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositivePeakFrequencies = 383,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositivePowerMargins = 381,

        /// <summary>
        /// double
        /// </summary>
        ResultSemReferencePower = 370,

        /// <summary>
        /// int
        /// </summary>
        SemAverageType = 340,

        /// <summary>
        /// int
        /// </summary>
        SemEnabled = 220,

        /// <summary>
        /// int
        /// </summary>
        SemMaskType = 343,

        /// <summary>
        /// double
        /// </summary>
        SemMeasurementLength = 339,

        /// <summary>
        /// int
        /// </summary>
        SemNumberOfAverages = 341,

        /// <summary>
        /// int[]
        /// </summary>
        SemOffsetBandsBandwidthIntegrals = 360,

        /// <summary>
        /// int[]
        /// </summary>
        SemOffsetBandsEnabled = 351,

        /// <summary>
        /// int[]
        /// </summary>
        SemOffsetBandsOffsetSides = 359,

        /// <summary>
        /// double[]
        /// </summary>
        SemOffsetBandsResolutionBandwidths = 357,

        /// <summary>
        /// int[]
        /// </summary>
        SemOffsetBandsResolutionBandwidthStates = 356,

        /// <summary>
        /// double[]
        /// </summary>
        SemOffsetBandsStartOffsetFrequencies = 352,

        /// <summary>
        /// double[]
        /// </summary>
        SemOffsetBandsStopOffsetFrequencies = 353,

        /// <summary>
        /// double[]
        /// </summary>
        SemStartAbsolutePowersLimits = 361,

        /// <summary>
        /// double[]
        /// </summary>
        SemStopAbsolutePowersLimits = 363,

        /// <summary>
        /// int
        /// </summary>
        SpectralMeasurementsAllEnabled = 211,

        /// <summary>
        /// double
        /// </summary>
        SystemBandwidth = 12,

        /// <summary>
        /// int
        /// </summary>
        ToolkitCompatibilityVersion = 247,

        /// <summary>
        /// int
        /// </summary>
        Ul3gppCyclicShiftEnabled = 22,

        /// <summary>
        /// int
        /// </summary>
        UlDftShiftEnabled = 20,

        /// <summary>
        /// int
        /// </summary>
        UlHoppingEnabled = 266,

        /// <summary>
        /// int
        /// </summary>
        UlHoppingMode = 21,

        /// <summary>
        /// int
        /// </summary>
        UlNumberOfPuschChannels = 24,

        /// <summary>
        /// int
        /// </summary>
        UlPuschCyclicShiftIndex0 = 45,

        /// <summary>
        /// int
        /// </summary>
        UlPuschCyclicShiftIndex1 = 46,

        /// <summary>
        /// int
        /// </summary>
        UlPuschDeltaSs = 51,

        /// <summary>
        /// int
        /// </summary>
        UlPuschModulationScheme = 38,

        /// <summary>
        /// int
        /// </summary>
        UlPuschNDmrs1 = 43,

        /// <summary>
        /// int
        /// </summary>
        UlPuschNDmrs2 = 44,

        /// <summary>
        /// int
        /// </summary>
        UlPuschNumberOfResourceBlocks = 40,

        /// <summary>
        /// double
        /// </summary>
        UlPuschPower = 41,

        /// <summary>
        /// int
        /// </summary>
        UlPuschResourceBlockOffset = 39,

        /// <summary>
        /// int
        /// </summary>
        UlPuschSubframeNumber = 37,

        //New Properties in LTE 1.0.1
        /// <summary>
        /// int
        /// </summary>
        AcpReferenceChannelAutoSweepTimeEnabled = 304,

        /// <summary>
        /// double
        /// </summary>
        AcpReferenceChannelSweepTime = 305,

        /// <summary>
        /// int
        /// </summary>
        AcpReferenceChannelAutoNumDataPointsEnabled = 302,

        /// <summary>
        /// int
        /// </summary>
        AcpReferenceChannelNumDataPoints = 303,

        /// <summary>
        /// int
        /// </summary>
        AcpReferenceChannelAutoNumFftSegmentsEnabled = 300,

        /// <summary>
        /// int
        /// </summary>
        AcpReferenceChannelNumFftSegments = 301,

        /// <summary>
        /// int
        /// </summary>
        ChpAutoSweepTimeEnabled = 286,

        /// <summary>
        /// double
        /// </summary>
        ChpSweepTime = 287,

        /// <summary>
        /// int
        /// </summary>
        ChpAutoNumDataPointsEnabled = 288,

        /// <summary>
        /// int
        /// </summary>
        ChpNumDataPoints = 289,

        //New Properties in 2.0

        /// <summary>
        /// int
        /// </summary>
        DuplexMode = 11,

        /// <summary>
        /// int
        /// </summary>
        UlDlConfiguration = 3,

        /// <summary>
        /// int
        /// </summary>
        UlNumberOfPucchChannels = 25,

        /// <summary>
        /// int
        /// </summary>
        UlPucchCyclicShiftIndex0 = 59,

        /// <summary>
        /// int
        /// </summary>
        UlPucchCyclicShiftIndex1 = 60,

        /// <summary>
        /// int
        /// </summary>
        UlPucchDeltaPucchShift = 61,

        /// <summary>
        /// int
        /// </summary>
        UlPucchFormat = 54,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNCs1 = 56,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNPucch1 = 57,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNPucch2 = 58,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNRb2 = 55,

        /// <summary>
        /// double
        /// </summary>
        UlPucchPower = 66,

        /// <summary>
        /// int
        /// </summary>
        UlPucchSubframeNumber = 53,

        /// <summary>
        /// int
        /// </summary>
        UlSrsBSrs = 71,

        /// <summary>
        /// int
        /// </summary>
        UlSrsCSrs = 70,

        /// <summary>
        /// int
        /// </summary>
        UlSrsEnabled = 429,

        /// <summary>
        /// int
        /// </summary>
        UlSrsISrs = 462,

        /// <summary>
        /// int
        /// </summary>
        UlSrsKTc = 73,

        /// <summary>
        /// int
        /// </summary>
        UlSrsNRrc = 75,

        /// <summary>
        /// int
        /// </summary>
        UlSrsNsrsCs = 72,

        /// <summary>
        /// double
        /// </summary>
        UlSrsPower = 76,

        /// <summary>
        /// int
        /// </summary>
        UlSrsSimultaneousAnSrs = 565,

        /// <summary>
        /// int
        /// </summary>
        UlSrsSubframeConfigurationIndex = 417,

        /// <summary>
        /// int
        /// </summary>
        ChpAllTracesEnabled = 291,

        /// <summary>
        /// double
        /// </summary>
        AcpReferenceChannelRrcFilterAlpha = 512,

        /// <summary>
        /// int
        /// </summary>
        AcpReferenceChannelRrcFilterEnabled = 415,

        /// <summary>
        /// int
        /// </summary>
        ObwAllTracesEnabled = 332,

        /// <summary>
        /// double
        /// </summary>
        SemChannelSpan = 222,

        /// <summary>
        /// int
        /// </summary>
        SemMeasurementResultsType = 342,

        /// <summary>
        /// int
        /// </summary>
        SemRrcFilterEnabled = 419,

        /// <summary>
        /// double
        /// </summary>
        SemReferenceChannelIntegrationBandwidth = 345,

        /// <summary>
        /// double
        /// </summary>
        SemReferenceChannelResolutionBandwidth = 349,

        /// <summary>
        /// int
        /// </summary>
        SemReferenceChannelResolutionBandwidthState = 348,

        /// <summary>
        /// double
        /// </summary>
        SemReferenceChannelStepFrequency = 347,

        /// <summary>
        /// int
        /// </summary>
        SemReferenceChannelStepFrequencyState = 346,

        /// <summary>
        /// int[]
        /// </summary>
        SemOffsetBandsStepFrequencyStates = 354,

        /// <summary>
        /// double[]
        /// </summary>
        SemOffsetBandsRelativeAttenuation = 358,

        /// <summary>
        /// int[]
        /// </summary>
        SemAbsoluteLimitType = 367,

        /// <summary>
        /// int
        /// </summary>
        SemAllSpectralTracesEnabled = 427,

        /// <summary>
        /// int
        /// </summary>
        SemAllTracesEnabled = 369,

        /// <summary>
        /// int
        /// </summary>
        SemIqTraceEnabled = 428,

        /// <summary>
        /// int[]
        /// </summary>
        SemMaskStates = 362,

        /// <summary>
        /// int[]
        /// </summary>
        SemRelativeLimitType = 365,

        /// <summary>
        /// double[]
        /// </summary>
        SemStartRelativePowersLimits = 364,

        /// <summary>
        /// double[]
        /// </summary>
        SemStopRelativePowersLimits = 366,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositiveRelativePowers = 373,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativeRelativePowers = 374,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositiveRelativePeakPowers = 377,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativeRelativePeakPowers = 378,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemPositiveAbsolutePowersAtWorstMargin = 423,

        /// <summary>
        /// double[]
        /// </summary>
        ResultSemNegativeAbsolutePowersAtWorstMargin = 424,

        /// <summary>
        /// int
        /// </summary>
        PvtAllTracesEnabled = 79,

        /// <summary>
        /// int
        /// </summary>
        PvtAveragingMode = 517,

        /// <summary>
        /// int
        /// </summary>
        PvtEnabled = 515,

        /// <summary>
        /// double
        /// </summary>
        PvtMaxRampDownTimeLimit = 524,

        /// <summary>
        /// double
        /// </summary>
        PvtMaxRampUpTimeLimit = 523,

        /// <summary>
        /// double
        /// </summary>
        PvtMaxTransmitOffPowerLimit = 525,

        /// <summary>
        /// double
        /// </summary>
        PvtMeasurementLength = 518,

        /// <summary>
        /// int
        /// </summary>
        PvtNumberOfAverages = 516,

        /// <summary>
        /// double
        /// </summary>
        PvtRampDownStartLevel = 521,

        /// <summary>
        /// double
        /// </summary>
        PvtRampDownStopLevel = 522,

        /// <summary>
        /// int
        /// </summary>
        PvtRampThresholdUnit = 535,

        /// <summary>
        /// double
        /// </summary>
        PvtRampUpStartLevel = 519,

        /// <summary>
        /// double
        /// </summary>
        PvtRampUpStopLevel = 520,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtAverageTransmitOffPower = 533,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtAverageTransmitOnPower = 534,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtBurstWidth = 528,

        /// <summary>
        /// int
        /// </summary>
        ResultPvtMeasurementStatus = 527,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtPeakPower = 532,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtRampDownTime = 530,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtRampUpTime = 529,

        /// <summary>
        /// double
        /// </summary>
        ResultPvtTotalAveragePower = 531,

        /// <summary>
        /// int
        /// </summary>
        CcdfAllTracesEnabled = 547,

        /// <summary>
        /// int
        /// </summary>
        CcdfDeadTimeRemovalEnabled = 539,

        /// <summary>
        /// int
        /// </summary>
        CcdfEnabled = 80,

        /// <summary>
        /// int
        /// </summary>
        CcdfIqPowerTraceEnabled = 546,

        /// <summary>
        /// double
        /// </summary>
        CcdfMeasurementLength = 537,

        /// <summary>
        /// int
        /// </summary>
        CcdfMeasurementTracesEnabled = 545,

        /// <summary>
        /// double
        /// </summary>
        CcdfRbwFilterBandwidth = 541,

        /// <summary>
        /// int
        /// </summary>
        CcdfRbwFilterType = 540,

        /// <summary>
        /// int
        /// </summary>
        CcdfSampleCount = 538,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfMeanPower = 403,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfMeanPowerPercentile = 404,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneHundredThousandthPower = 409,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneHundredthPower = 406,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneMillionthPower = 410,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneTenThousandthPower = 408,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneTenthPower = 405,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneThousandthPower = 407,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfPeakToAveragePowerRatio = 402,

        /// <summary>
        /// int
        /// </summary>
        ResultCcdfResultantCount = 536,

        /// <summary>
        /// double
        /// </summary>
        SemReferenceChannelOffsetFrequency = 350,

        /// <summary>
        /// double[]
        /// </summary>
        SemOffsetBandsStepFrequencies = 355,

        /// <summary>
        /// int
        /// </summary>
        AcpAllSpectralTraceEnabled = 425,

        /// <summary>
        /// int
        /// </summary>
        AcpAllTracesEnabled = 315,

        /// <summary>
        /// int
        /// </summary>
        AcpIqTraceEnabled = 426,

        /// <summary>
        /// double[]
        /// </summary>
        AcpNoiseFloors = 313,

        /// <summary>
        /// int
        /// </summary>
        AcpNoiseCompensationEnabled = 414,

        /// <summary>
        /// double[]
        /// </summary>
        ResultAcpAbsolutePowers = 513,

        /// <summary>
        /// int
        /// </summary>
        SystemFrameNumber = 473,

        /// <summary>
        /// double
        /// </summary>
        SemRrcFilterAlpha = 420,
    }

}