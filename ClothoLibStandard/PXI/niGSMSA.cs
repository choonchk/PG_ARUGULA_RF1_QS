using System;
using System.Runtime.InteropServices;
using System.Text;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.RFToolkits.Interop
{
    public class niGSMSA : IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _isNamedSession;

        /// <summary>
        /// Looks up an existing niGSM analysis session and returns the refnum that you can pass to subsequent niGSM Analysis functions. If the lookup fails, the niGSMSA_OpenSession function creates a new niGSM analysis session and returns a new refnum.
        /// 
        /// </summary>
        ///<param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niGSMSA_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        public niGSMSA(int toolkitCompatibilityVersion)
        {
            IntPtr handle;
            int isNewSession;
            int pInvokeResult = PInvoke.niGSMSA_OpenSession(string.Empty, toolkitCompatibilityVersion, out isNewSession, out handle);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            _isNamedSession = false;
        }

        /// <summary>
        /// Looks up an existing niGSM analysis session and returns the refnum that you can pass to subsequent niGSM Analysis functions. If the lookup fails, the niGSMSA_OpenSession function creates a new niGSM analysis session and returns a new refnum.
        /// Make sure you call Close for the named session. Dispose does not close named session.
        /// </summary>
        ///<param>
        /// sessionName
        /// char[]
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. To get the reference to an existing open session, call the niGSMSA_OpenSession function and specify the same name as an existing open session function in the sessionName parameter.
        ///  You can obtain the reference to an existing session multiple times if you have not called the niGSMSA_CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string to the sessionName parameter. 
        /// Tip&nbsp;&nbsp;National Instruments recommends that you call the niGSMSA_CloseSession function for each uniquely-named instance of the niGSMSA_OpenSession function or each instance of the niGSMSA_OpenSession function with an unnamed session.
        /// 
        ///</param>
        ///<param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niGSMSA_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
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
        /// niGSMSASession*
        /// Returns the niGSM analysis session.
        /// 
        ///</param>
        public niGSMSA(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            IntPtr handle;
            int pInvokeResult = PInvoke.niGSMSA_OpenSession(sessionName, toolkitCompatibilityVersion, out isNewSession, out handle);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            if (String.IsNullOrEmpty(sessionName))
                _isNamedSession = false;
            else
                _isNamedSession = true;
        }

        ~niGSMSA() { Dispose(false); }

        public HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        /// <summary>
        /// Performs modulation accuracy (ModAcc), power versus time (PvT), transmit power (TxP), and output radio frequency spectrum (ORFS) measurements on the input complex waveform. You can enable all these measurements and perform them simultaneously. Call this function as many times as specified by the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS attribute.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64
        /// Specifies the start parameter.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64
        /// Specifies the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// waveform
        /// NIComplexNumber[]
        /// Specifies the acquired complex-valued signal. The real and imaginary parts of this complex array correspond to the in-phase (I) and quadrature-phase (Q) data, respectively.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the waveform array.
        /// 
        ///</param>
        ///<param>
        /// reset
        /// int32
        /// Resets the measurement and averaging.
        /// 
        ///</param>
        ///<param>
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int AnalyzeIQComplexF64(double t0, double dt, niComplexNumber[] waveform, int length, int reset, out int done)
        {
            int pInvokeResult = PInvoke.niGSMSA_AnalyzeIQComplexF64(Handle, t0, dt, waveform, length, reset, out done);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Computes the carrier frequency using the values that you specify in the UUT, band, and ARFCN parameters, as described in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        /// 
        /// </summary>
        ///<param>
        /// UUT
        /// int32
        /// Specifies the type of UUT generating the signal that the toolkit analyzes. The default value is NIGSMSA_VAL_UUT_MS. 
        ///    NIGSMSA_VAL_UUT_BTS (0)
        ///   Specifies that the toolkit analyzes the signal from a base transmit station (BTS).
        ///       Note  The current version of the toolkit supports only normal BTS. 
        ///    NIGSMSA_VAL_UUT_MS (1)
        ///   Specifies that the toolkit analyzes the signal from a mobile station (MS). This value is the default.
        /// 
        ///</param>
        ///<param>
        /// band
        /// int32
        /// Specifies the band of operation. The default value is niGSMSA_VAL_BAND_PGSM.
        /// niGSMSA_VAL_BAND_PGSM (0)
        /// Specifies a primary GSM (PGSM) band in the 900 MHz band. This value is the default.
        /// niGSMSA_VAL_BAND_EGSM (1)
        /// Specifies an extended GSM (EGSM) band in the 900 MHz band.
        /// niGSMSA_VAL_BAND_RGSM (2)
        /// Specifies a railway GSM (RGSM) band in the 900 MHz band.
        /// niGSMSA_VAL_BAND_DCS1800 (3)
        /// Specifies a digital cellular system 1800 (DCS 1800) band. This band is also known as GSM 1800.
        /// niGSMSA_VAL_BAND_PCS1900 (4)
        /// Specifies a personal communications service 1900 (PCS 1900) band. This band is also known as GSM 1900.
        /// niGSMSA_VAL_BAND_GSM450 (5)
        /// Specifies a GSM 450 band.
        /// niGSMSA_VAL_BAND_GSM480 (6)
        /// Specifies a GSM 480 band.
        /// niGSMSA_VAL_BAND_GSM850 (7)
        /// Specifies a GSM 850 band.
        /// niGSMSA_VAL_BAND_GSM750 (8)
        /// Specifies a GSM 750 band.
        /// niGSMSA_VAL_BAND_TGSM810 (9)
        /// Specifies a terrestrial GSM 810 (T GSM 810) band.
        /// 
        ///</param>
        ///<param>
        /// ARFCN
        /// int32
        /// Specifies the absolute RF channel number. The default value is 1. 
        /// 
        ///</param>
        ///<param>
        /// carrierFrequency
        /// float64*
        /// Returns the carrier frequency, in hertz (Hz), corresponding to the UUT, band, and ARFCN parameters. 
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ARFCNToCarrierFrequency(int uUT, int band, int aRFCN, out double carrierFrequency)
        {
            int pInvokeResult = PInvoke.niGSMSA_ARFCNToCarrierFrequency(uUT, band, aRFCN, out carrierFrequency);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Closes the niGSM analysis session and releases resources associated with that session.
        /// 
        /// </summary>
        public void Close()
        {
            if (!_isNamedSession)
                Dispose();
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                    PInvoke.niGSMSA_CloseSession(Handle);
            }
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// acquiredIQWaveform
        /// NIComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the acquiredIQWaveform parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ModAccGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the hard decision decoded bits from the  useful portion of the burst. The toolkit uses these bits to generate the reference signal for computing the modulation accuracy (ModAcc) of the acquired signal.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// demodulatedBits
        /// int32[]
        /// Returns the hard decision decoded bits from the useful portion of the burst. You can pass NULL to the demodulatedBits parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the demodulatedBits array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the demodulatedBits parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetCurrentIterationDemodulatedBitTrace(out int[] demodulatedBits, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ModAccGetCurrentIterationDemodulatedBitTrace(Handle, out demodulatedBits, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the phase error trace of the useful portion of the last burst analyzed.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// phaseError
        /// float64[]
        /// Returns the frequency corrected phase error of the useful portion of the burst from the last timeslot. You can pass NULL to the phaseError parameter to get size of the array in the traceSize parameter. 
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the phaseError array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the phaseError parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ModAccGetCurrentIterationPhaseErrorTrace(out double t0, out double dt, double[] phaseError, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ModAccGetCurrentIterationPhaseErrorTrace(Handle, out t0, out dt, phaseError, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// acquiredIQWaveform
        /// NIComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the acquiredIQWaveform parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the modulation powers at various offset frequencies. Use the niGSMSA_ORFSGetModulationOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The modulation powers represent the absolute power due to modulation at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// modulationAbsolutePowers
        /// float64[]
        /// Returns the absolute powers due to modulation. You can pass NULL to the modulationAbsolutePowers parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the modulationAbsolutePowers array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the modulationAbsolutePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetModulationAbsolutePowersTrace(double[] modulationAbsolutePowers, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetModulationAbsolutePowersTrace(Handle, modulationAbsolutePowers, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the array of offset frequencies at which output radio frequency spectrum (ORFS) due to modulation measurement is performed.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// modulationOffsetFrequencies
        /// float64[]
        /// Returns the array of offset frequencies at which ORFS due to  modulation measurement is performed. You can pass NULL to the modulationOffsetFrequencies parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the modulationOffsetFrequencies array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the modulationOffsetFrequencies parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetModulationOffsetFrequenciesTrace(double[] modulationOffsetFrequencies, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetModulationOffsetFrequenciesTrace(Handle, modulationOffsetFrequencies, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the modulation powers at various offset frequencies. Use the niGSMSA_ORFSGetModulationOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The modulation powers represent the power relative to the carrier power due to modulation at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// modulationRelativePowers
        /// float64[]
        /// Returns the powers relative to the carrier power, due to modulation. You can pass NULL to the modulationRelativePowers parameter to get size of the array in the traceSize parameter. 
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the modulationRelativePowers array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the modulationRelativePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetModulationRelativePowersTrace(double[] modulationRelativePowers, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetModulationRelativePowersTrace(Handle, modulationRelativePowers, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the switching powers at various offset frequencies. Use the niGSMSA_ORFSGetSwitchingOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The switching powers represent the power due to the switching part of the waveform.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// switchingAbsolutePowers
        /// float64[]
        /// Returns the absolute powers due to switching. You can pass NULL to the switchingAbsolutePowers parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the switchingAbsolutePowers array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the switchingAbsolutePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetSwitchingAbsolutePowersTrace(double[] switchingAbsolutePowers, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetSwitchingAbsolutePowersTrace(Handle, switchingAbsolutePowers, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the offset frequencies at which the output radio frequency spectrum (ORFS) due to switching measurement is performed. The switching powers represent the absolute power due to switching at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// switchingOffsetFrequencies
        /// float64[]
        /// Returns the array of offset frequencies. You can pass NULL to the switchingOffsetFrequencies parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the switchingOffsetFrequencies array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the switchingOffsetFrequencies parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetSwitchingOffsetFrequenciesTrace(double[] switchingOffsetFrequencies, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetSwitchingOffsetFrequenciesTrace(Handle, switchingOffsetFrequencies, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the switching powers at various offset frequencies. Use the niGSMSA_ORFSGetSwitchingOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The switching powers represent the power relative to the carrier due to the switching part of the waveform at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// switchingRelativePowers
        /// float64[]
        /// Returns the powers relative to the carrier power, due to switching. You can pass NULL to the switchingRelativePowers parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the switchingRelativePowers array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the switchingRelativePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ORFSGetSwitchingRelativePowersTrace(double[] switchingRelativePowers, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_ORFSGetSwitchingRelativePowersTrace(Handle, switchingRelativePowers, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the power versus time (PvT) trace averaged across timeslots.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// averageSignalPower
        /// float64[]
        /// Returns the signal power averaged across timeslots. You can pass NULL to the averageSignalPower parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the averageSignalPower array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the averageSignalPower parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int PvTGetAverageSignalPowerTrace(out double t0, out double dt, double[] averageSignalPower, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_PvTGetAverageSignalPowerTrace(Handle, out t0, out dt, averageSignalPower, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// acquiredIQWaveform
        /// NIComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the acquiredIQWaveform parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int PvTGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_PvTGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the lower mask used for power versus time (PvT) measurements.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// lowerMask
        /// float64[]
        /// <td>Returns the lower mask used for PvT measurements. You can pass NULL to the lowerMask parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the lowerMask array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the lowerMask parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int PvTGetLowerMaskTrace(out double t0, out double dt, double[] lowerMask, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_PvTGetLowerMaskTrace(Handle, out t0, out dt, lowerMask, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the upper mask used for power versus time (PvT) measurements.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// upperMask
        /// float64[]
        /// Returns the upper mask used for PvT measurements. You can pass NULL to the upperMask parameter to get size of the array in the traceSize parameter. 
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the upperMask array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the upperMask parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int PvTGetUpperMaskTrace(out double t0, out double dt, double[] upperMask, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_PvTGetUpperMaskTrace(Handle, out t0, out dt, upperMask, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets the attribute specified in the attributeID parameter to its default value.  You can reset only a writable attribute using this function.
        /// 
        /// </summary>
        ///<param name = session>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param name = channelString>
        /// channelString
        /// char*
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = attributeID>
        /// attributeID
        /// niGSMSA_Attr
        /// Specifies the ID of the niGSM analysis attribute that you want to reset.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ResetAttribute(string channelString, niGSMSAProperties attributeID)
        {
            int pInvokeResult = PInvoke.niGSMSA_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets all the attributes of the session to their default values.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niGSMSA_ResetSession(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Examines the incoming signal to calculate the appropriate power level. This function then returns the estimated power level in the resultantReferenceLevel parameter. Use this feature if you need help in calculating an approximate setting for the power level for IQ measurements. This function queries the NIRFSA_ATTR_REFERENCE_LEVEL attribute and uses this value as the starting point for auto level calculations. Set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to the highest expected power level of the signal for faster convergence. For example, if the device under test (DUT) operates in the range of -10 dBm to -30 dBm, set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to -10 dBm.
        /// 
        /// </summary>
        ///<param>
        /// RFSAHandle
        /// ViSession
        /// Identifies the instrument session. The toolkit obtains this value from the niRFSA_init function or the niRFSA_InitWithOptions function.
        /// 
        ///</param>
        ///<param>
        /// bandwidth
        /// float64
        /// Specifies the bandwidth, in hertz (Hz), of the signal to be analyzed.
        /// 
        ///</param>
        ///<param>
        /// measurementInterval
        /// float64
        /// Specifies the acquisition length, in seconds. Function uses this value to compute the number of samples to acquire from the RF signal analyzer.
        /// 
        ///</param>
        ///<param>
        /// maxNumberOfIterations
        /// int32
        /// Specifies the maximum number of iterations to perform when computing the reference level to be set on the RF signal analyzer.
        /// 
        ///</param>
        ///<param>
        /// resultantReferenceLevel
        /// float64*
        /// Returns the estimated power level, in dBm, of the input signal.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int RFSAAutoLevel(HandleRef rfsaHandle, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel)
        {
            int pInvokeResult = PInvoke.niGSMSA_RFSAAutoLevel(Handle, bandwidth, measurementInterval, maxNumberofIterations, out resultantReferenceLevel);
            TestForError(pInvokeResult, rfsaHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Retrieves the recommended hardware settings from the niGSM analysis session and sets these values to the appropriate niRFSA attributes.
        /// This function sets the following NI-RFSA attributes:
        ///     Sets the NIRFSA_ATTR_ACQUISITION_TYPE attribute to NIRFSA_VAL_IQ.
        ///     Sets the NIRFSA_ATTR_NUM_RECORDS_IS_FINITE attribute to VI_TRUE.
        ///     Sets the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_RECORDS attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.
        ///     Sets the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_SAMPLING_RATE_HZ attribute to the NIRFSA_ATTR_IQ_RATE attribute.
        ///     If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to NIRFSA_VAL_IQ_POWER_EDGE, this function sets the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME_SEC attribute to the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_MINIMUM_QUIET_TIME attribute. If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to any other value, this function sets the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME_SEC attribute to 0.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_ACQUISITION_TIME_SEC attribute, and sets the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.
        ///     Sets the NIRFSA_ATTR_NUM_SAMPLES_IS_FINITE attribute to VI_TRUE.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_PRE_TRIGGER_DELAY_SEC attribute, and sets the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// RFSAHandle
        /// ViSession
        /// Identifies the instrument session. The toolkit obtains this value from the niRFSA_init function or the niRFSA_InitWithOptions function.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int RFSAConfigureHardware(HandleRef rfsaHandle)
        {
            int pInvokeResult = PInvoke.niGSMSA_RFSAConfigureHardware(Handle, rfsaHandle);
            TestForError(pInvokeResult, rfsaHandle);
            return pInvokeResult;
        }
        
        /// <summary>
        /// Checks for errors on all configured attributes. If the configuration is invalid, this function returns an error. If there are no errors, the function marks the session as verified.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int ToolkitCheckError()
        {
            int pInvokeResult = PInvoke.niGSMSA_ToolkitCheckError(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the burst power trace averaged across timeslots.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// averagePower
        /// float64[]
        /// Returns the burst power trace averaged across timeslots. You can pass NULL to the averagePower parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the averagePower array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the averagePower parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int TxPGetAveragePowerTrace(out double t0, out double dt, double[] averagePower, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_TxPGetAveragePowerTrace(Handle, out t0, out dt, averagePower, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// niGSMSASession
        /// Specifies the niGSM analysis session.
        /// 
        ///</param>
        ///<param>
        /// t0
        /// float64*
        /// Returns the initial time of the plot, in seconds.
        /// 
        ///</param>
        ///<param>
        /// dt
        /// float64*
        /// Returns the delta parameter.
        /// 
        ///</param>
        ///<param>
        /// acquiredIQWaveform
        /// NIComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the traceSize parameter.
        /// 
        ///</param>
        ///<param>
        /// length
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// traceSize
        /// int32*
        /// Returns the actual number of elements populated in the switchingAbsolutePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the traceSize parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int TxPGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int length, out int traceSize)
        {
            int pInvokeResult = PInvoke.niGSMSA_TxPGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, length, out traceSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the RF signal analyzer and initiates acquisition on the RF signal analyzer. 
        /// This method then fetches the waveforms and calls the Analyze in a loop n times to perform measurements on the acquired waveforms, where n is equal to the number of averages specified.
        ///
        /// </summary>
        /// <param>
        /// rfsaHandle
        /// Identifies the instrument session. The toolkit obtains this value from the niRFSA_init function or the niRFSA_InitWithOptions function.
        /// 
        /// </param>
        /// <param>
        /// timeout
        /// Specifies the time allotted, in seconds. This value is passed to the timeout parameter of the niRFSA_FetchIQ. The default value is 10.
        /// 
        /// </param>
        ///<returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niGSM analysis function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niGSMSA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value Meaning 
        /// 0 Success 
        /// Positive Values Warnings 
        /// Negative Values Exception 
        /// 
        ///</returns>
        public int RFSAMeasure(HandleRef rfsaHandle, double timeout)
        {
            int pInvokeResult = PInvoke.niGSMSA_RFSAMeasure_vi(Handle, rfsaHandle, timeout);
            TestForError(pInvokeResult, rfsaHandle);
            return pInvokeResult;
        }

        /// <summary>
        /// Indicates the version of the toolkit in use.    
        /// 
        /// </summary>
        public int GetAdvancedToolkitCompatibilityVersion(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.AdvancedToolkitCompatibilityVersion, channel, out val);
        }

        /// <summary>
        /// Specifies the absolute RF channel number, as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///     The toolkit uses the NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes to calculate the correct masks for    power versus time (PvT) measurements and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute    is set to NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///     Use the niGSMSA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///      The default value is 1.    
        /// 
        /// </summary>
        public int SetArfcn(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.Arfcn, channel, value);
        }

        /// <summary>
        /// Specifies the absolute RF channel number, as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///     The toolkit uses the NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes to calculate the correct masks for    power versus time (PvT) measurements and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute    is set to NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///     Use the niGSMSA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///      The default value is 1.    
        /// 
        /// </summary>
        public int GetArfcn(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.Arfcn, channel, out val);
        }

        /// <summary>
        /// Specifies the band of operation as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///     The toolkit uses the NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes to calculate the correct masks for    power versus time (PvT) measurements and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute    is set to NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///     Use the niGSMSA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///     The default value is NIGSMSA_VAL_BAND_PGSM.   
        /// 
        /// </summary>
        public int SetBand(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.Band, channel, value);
        }

        /// <summary>
        /// Specifies the band of operation as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///     The toolkit uses the NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes to calculate the correct masks for    power versus time (PvT) measurements and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute    is set to NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///     Use the niGSMSA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///     The default value is NIGSMSA_VAL_BAND_PGSM.   
        /// 
        /// </summary>
        public int GetBand(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.Band, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable burst synchronization before performing output radio frequency spectrum (ORFS),    modulation accuracy (ModAcc), and power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_TRUE.  
        /// 
        /// </summary>
        public int SetBurstSynchronizationEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.BurstSynchronizationEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable burst synchronization before performing output radio frequency spectrum (ORFS),    modulation accuracy (ModAcc), and power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_TRUE.  
        /// 
        /// </summary>
        public int GetBurstSynchronizationEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.BurstSynchronizationEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by modulation accuracy (ModAcc) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.    
        /// 
        /// </summary>
        public int SetModaccAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.ModaccAllTracesEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by modulation accuracy (ModAcc) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.    
        /// 
        /// </summary>
        public int GetModaccAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.ModaccAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable modulation accuracy (ModAcc) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int SetModaccEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.ModaccEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable modulation accuracy (ModAcc) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int GetModaccEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.ModaccEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform modulation accuracy (ModAcc) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int SetModaccNumberOfAverages(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.ModaccNumberOfAverages, channel, value);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform modulation accuracy (ModAcc) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int GetModaccNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.ModaccNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Returns the average frequency error, in hertz (Hz).
        ///     To perform this measurement, the toolkit computes the frequency error of all the bursts and returns the mean of these values.  
        /// 
        /// </summary>
        public int GetModaccResultsAverageFrequencyError(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.ModaccResultsAverageFrequencyError, channel, out val);
        }

        /// <summary>
        /// Returns the average IQ imbalance, in dB.
        ///     To perform this measurement, the toolkit computes the IQ imbalance of all the bursts and returns the mean of these values.  
        /// 
        /// </summary>
        public int GetModaccResultsAverageIqGainImbalance(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.ModaccResultsAverageIqGainImbalance, channel, out val);
        }

        /// <summary>
        /// Returns the average origin offset, in dB.
        ///     To perform this measurement, the toolkit computes the origin offset of all the bursts and returns the mean of these values.  
        /// 
        /// </summary>
        public int GetModaccResultsAverageOriginOffset(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.ModaccResultsAverageOriginOffset, channel, out val);
        }

        /// <summary>
        /// Returns the average root mean square (RMS) phase error, in degrees.
        ///     To perform this measurement, the toolkit computes the RMS phase error of all the bursts and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetModaccResultsAverageRmsPhaseError(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.ModaccResultsAverageRmsPhaseError, channel, out val);
        }

        /// <summary>
        /// Returns the TSC detected while performing modulation accuracy (ModAcc) measurements.  
        /// 
        /// </summary>
        public int GetModaccResultsDetectedTsc(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.ModaccResultsDetectedTsc, channel, out val);
        }

        /// <summary>
        /// Returns the maximum peak phase error, in degrees.
        ///     To perform this measurement, the toolkit computes the peak phase error of all the bursts and returns the maximum of these values.  
        /// 
        /// </summary>
        public int GetModaccResultsMaximumPeakPhaseError(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.ModaccResultsMaximumPeakPhaseError, channel, out val);
        }

        /// <summary>
        /// Specifies the number of occupied timeslots (bursts) in a frame.
        ///     The toolkit uses this attribute to compute the number of records to acquire as shown in the following formula:    number of records = number of averages/number of timeslots, where the number of averages is the maximum of the number    of averages for all enabled measurements. The occupied timeslots must be consecutive timeslots.
        ///     Note: For power versus time (PvT) measurements, set this attribute to 1, which is the only supported value.    The toolkit also uses this attribute to compute the minimum quiet time required to satisfy the triggering condition.
        ///     The default value is 1. Valid values are 1 to 8, inclusive.  
        /// 
        /// </summary>
        public int SetNumberOfTimeslots(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.NumberOfTimeslots, channel, value);
        }

        /// <summary>
        /// Specifies the number of occupied timeslots (bursts) in a frame.
        ///     The toolkit uses this attribute to compute the number of records to acquire as shown in the following formula:    number of records = number of averages/number of timeslots, where the number of averages is the maximum of the number    of averages for all enabled measurements. The occupied timeslots must be consecutive timeslots.
        ///     Note: For power versus time (PvT) measurements, set this attribute to 1, which is the only supported value.    The toolkit also uses this attribute to compute the minimum quiet time required to satisfy the triggering condition.
        ///     The default value is 1. Valid values are 1 to 8, inclusive.  
        /// 
        /// </summary>
        public int GetNumberOfTimeslots(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.NumberOfTimeslots, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by output radio frequency spectrum (ORFS) measurements.
        ///     The default value is NIGSMSA_VAL_TRUE.  
        /// 
        /// </summary>
        public int SetOrfsAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsAllTracesEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by output radio frequency spectrum (ORFS) measurements.
        ///     The default value is NIGSMSA_VAL_TRUE.  
        /// 
        /// </summary>
        public int GetOrfsAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable output radio frequency spectrum (ORFS) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int SetOrfsEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable output radio frequency spectrum (ORFS) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int GetOrfsEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to perform output radio frequency spectrum (ORFS) measurements on the modulated part of the waveform, switching    part of the waveform, or both.
        ///     The default value is NIGSMSA_VAL_MEASUREMENT_TYPE_MODULATION_SWITCHING.  
        /// 
        /// </summary>
        public int SetOrfsMeasurementType(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsMeasurementType, channel, value);
        }

        /// <summary>
        /// Specifies whether to perform output radio frequency spectrum (ORFS) measurements on the modulated part of the waveform, switching    part of the waveform, or both.
        ///     The default value is NIGSMSA_VAL_MEASUREMENT_TYPE_MODULATION_SWITCHING.  
        /// 
        /// </summary>
        public int GetOrfsMeasurementType(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsMeasurementType, channel, out val);
        }

        public int GetOrfsFastAveragingMode(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsFastAveragingMode, channel, out val);
        }

        public int SetOrfsFastAveragingMode(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsFastAveragingMode, channel, value);
        }

        /// <summary>
        /// Specifies the averaging type for performing modulation measurements for output radio frequency spectrum (ORFS).
        ///     The default value is NIGSMSA_VAL_ORFS_LOG_AVERAGING.  
        /// 
        /// </summary>
        public int SetOrfsModulationAveragingMode(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsModulationAveragingMode, channel, value);
        }

        /// <summary>
        /// Specifies the averaging type for performing modulation measurements for output radio frequency spectrum (ORFS).
        ///     The default value is NIGSMSA_VAL_ORFS_LOG_AVERAGING.  
        /// 
        /// </summary>
        public int GetOrfsModulationAveragingMode(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsModulationAveragingMode, channel, out val);
        }

        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS) modulation measurements    are performed.
        ///     This attribute is applicable when the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int SetOrfsModulationOffsetFrequencies(string channel, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.OrfsModulationOffsetFrequencies, channel, dataArray);
        }

        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS) modulation measurements    are performed.
        ///     This attribute is applicable when the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int GetOrfsModulationOffsetFrequencies(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.OrfsModulationOffsetFrequencies, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the modulated part    of the waveform at the carrier reference.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int SetOrfsModulationRbwCarrier(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.OrfsModulationRbwCarrier, channel, value);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the modulated part    of the waveform at the carrier reference.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int GetOrfsModulationRbwCarrier(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.OrfsModulationRbwCarrier, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the modulated part    of the waveform at offsets that are greater than 1,800 kHz.
        ///     The default value is 100k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int SetOrfsModulationRbwFarOffset(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.OrfsModulationRbwFarOffset, channel, value);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the modulated part    of the waveform at offsets that are greater than 1,800 kHz.
        ///     The default value is 100k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int GetOrfsModulationRbwFarOffset(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.OrfsModulationRbwFarOffset, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the modulated part    of the waveform at offsets that are less than 1,800 kHz.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int SetOrfsModulationRbwNearOffset(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.OrfsModulationRbwNearOffset, channel, value);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the modulated part    of the waveform at offsets that are less than 1,800 kHz.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int GetOrfsModulationRbwNearOffset(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.OrfsModulationRbwNearOffset, channel, out val);
        }

        /// <summary>
        /// Specifies whether the toolkit uses the NIGSMSA_ORFS_NOISE_FLOORS attribute to perform noise floor compensation    in the measurement if the NIGSMSA_ORFS_NOISE_COMPENSATION_ENABLED attribute is set to NIGSMSA_VAL_TRUE.    
        /// 
        /// </summary>
        public int SetOrfsNoiseCompensationEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsNoiseCompensationEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether the toolkit uses the NIGSMSA_ORFS_NOISE_FLOORS attribute to perform noise floor compensation    in the measurement if the NIGSMSA_ORFS_NOISE_COMPENSATION_ENABLED attribute is set to NIGSMSA_VAL_TRUE.    
        /// 
        /// </summary>
        public int GetOrfsNoiseCompensationEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsNoiseCompensationEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the noise floor, in dBm, for each frequency offset. The first m elements in the array correspond to the noise    floors at the modulation frequency offsets. The remaining n elements in the array correspond to the noise floors at the    switching frequency offsets. Ensure that m is equal to the number of modulation offset frequencies that you specify in    the NIGSMSA_ORFS_MODULATION_OFFSET_FREQUENCIES attribute and n is equal to the number of switching offset frequencies    that you specify in the NIGSMSA_ORFS_SWITCHING_OFFSET_FREQUENCIES attribute.  
        /// 
        /// </summary>
        public int SetOrfsNoiseFloors(string channel, double[] value, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.OrfsNoiseFloors, channel, dataArray);
        }

        /// <summary>
        /// Specifies the noise floor, in dBm, for each frequency offset. The first m elements in the array correspond to the noise    floors at the modulation frequency offsets. The remaining n elements in the array correspond to the noise floors at the    switching frequency offsets. Ensure that m is equal to the number of modulation offset frequencies that you specify in    the NIGSMSA_ORFS_MODULATION_OFFSET_FREQUENCIES attribute and n is equal to the number of switching offset frequencies    that you specify in the NIGSMSA_ORFS_SWITCHING_OFFSET_FREQUENCIES attribute.  
        /// 
        /// </summary>
        public int GetOrfsNoiseFloors(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.OrfsNoiseFloors, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform output radio frequency spectrum (ORFS) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int SetOrfsNumberOfAverages(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsNumberOfAverages, channel, value);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform output radio frequency spectrum (ORFS) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int GetOrfsNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Specifies the offset frequency mode.
        ///     The default value is NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.  
        /// 
        /// </summary>
        public int SetOrfsOffsetFrequencyMode(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.OrfsOffsetFrequencyMode, channel, value);
        }

        /// <summary>
        /// Specifies the offset frequency mode.
        ///     The default value is NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.  
        /// 
        /// </summary>
        public int GetOrfsOffsetFrequencyMode(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.OrfsOffsetFrequencyMode, channel, out val);
        }

        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS) switching measurements    are performed.
        ///     This attribute is applicable when the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int SetOrfsSwitchingOffsetFrequencies(string channel, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.OrfsSwitchingOffsetFrequencies, channel, dataArray);
        }

        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS) switching measurements    are performed.
        ///     This attribute is applicable when the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int GetOrfsSwitchingOffsetFrequencies(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.OrfsSwitchingOffsetFrequencies, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the switching part    of the waveform at the carrier reference.
        ///     The default value is 300k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int SetOrfsSwitchingRbwCarrier(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.OrfsSwitchingRbwCarrier, channel, value);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the switching part    of the waveform at the carrier reference.
        ///     The default value is 300k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int GetOrfsSwitchingRbwCarrier(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.OrfsSwitchingRbwCarrier, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the switching part    of the waveform at offsets that are greater than 1,800 kHz.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int SetOrfsSwitchingRbwFarOffset(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.OrfsSwitchingRbwFarOffset, channel, value);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the switching part    of the waveform at offsets that are greater than 1,800 kHz.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int GetOrfsSwitchingRbwFarOffset(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.OrfsSwitchingRbwFarOffset, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the switching part    of the waveform at offsets that are less than 1,800 kHz.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int SetOrfsSwitchingRbwNearOffset(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.OrfsSwitchingRbwNearOffset, channel, value);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output radio frequency spectrum (ORFS) in the switching part    of the waveform at offsets that are less than 1,800 kHz.
        ///     The default value is 30k. Valid values are 1k to 5M, inclusive.  
        /// 
        /// </summary>
        public int GetOrfsSwitchingRbwNearOffset(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.OrfsSwitchingRbwNearOffset, channel, out val);
        }

        /// <summary>
        /// Specifies the power control level on the unit under test (UUT), as defined in section 4.1 of the 3GPP TS 45.005    Specifications 8.0.0.
        ///     The toolkit uses this attribute to determine the mask required for power versus time (PvT) measurements.
        ///     The default value is 0. Valid values are 0 to 32, inclusive.  
        /// 
        /// </summary>
        public int SetPowerControlLevel(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PowerControlLevel, channel, value);
        }

        /// <summary>
        /// Specifies the power control level on the unit under test (UUT), as defined in section 4.1 of the 3GPP TS 45.005    Specifications 8.0.0.
        ///     The toolkit uses this attribute to determine the mask required for power versus time (PvT) measurements.
        ///     The default value is 0. Valid values are 0 to 32, inclusive.  
        /// 
        /// </summary>
        public int GetPowerControlLevel(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PowerControlLevel, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int SetPvtAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PvtAllTracesEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int GetPvtAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the averaging type for power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_PVT_RMS_AVERAGING.  
        /// 
        /// </summary>
        public int SetPvtAveragingMode(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PvtAveragingMode, channel, value);
        }

        /// <summary>
        /// Specifies the averaging type for power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_PVT_RMS_AVERAGING.  
        /// 
        /// </summary>
        public int GetPvtAveragingMode(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtAveragingMode, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int SetPvtEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PvtEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int GetPvtEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the 3 dB double-sided bandwidth, in hertz (Hz), of the specified low-pass filter.
        ///     The default value is 500k. Valid values are 10 to 20M, inclusive.  
        /// 
        /// </summary>
        public int SetPvtRbwFilterBandwidth(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.PvtRbwFilterBandwidth, channel, value);
        }

        /// <summary>
        /// Specifies the 3 dB double-sided bandwidth, in hertz (Hz), of the specified low-pass filter.
        ///     The default value is 500k. Valid values are 10 to 20M, inclusive.  
        /// 
        /// </summary>
        public int GetPvtRbwFilterBandwidth(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.PvtRbwFilterBandwidth, channel, out val);
        }

        /// <summary>
        /// Specifies the type of front end low-pass filter used for power versus time (PvT) measurements.
        ///     Note: This is the first filter that is applied on the acquired waveform in the niGSMSA_Analyze function.
        ///      The default value is NIGSMSA_VAL_FILTER_TYPE_GAUSSIAN.  
        /// 
        /// </summary>
        public int SetPvtRbwFilterType(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PvtRbwFilterType, channel, value);
        }

        /// <summary>
        /// Specifies the type of front end low-pass filter used for power versus time (PvT) measurements.
        ///     Note: This is the first filter that is applied on the acquired waveform in the niGSMSA_Analyze function.
        ///      The default value is NIGSMSA_VAL_FILTER_TYPE_GAUSSIAN.  
        /// 
        /// </summary>
        public int GetPvtRbwFilterType(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtRbwFilterType, channel, out val);
        }

        /// <summary>
        /// Specifies whether to use a standard-specified mask or a user-defined mask for power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_MASK_TYPE_STANDARD.  
        /// 
        /// </summary>
        public int SetPvtMaskType(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PvtMaskType, channel, value);
        }

        /// <summary>
        /// Specifies whether to use a standard-specified mask or a user-defined mask for power versus time (PvT) measurements.
        ///     The default value is NIGSMSA_VAL_MASK_TYPE_STANDARD.  
        /// 
        /// </summary>
        public int GetPvtMaskType(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtMaskType, channel, out val);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform power versus time (PvT) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int SetPvtNumberOfAverages(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.PvtNumberOfAverages, channel, value);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform power versus time (PvT) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int GetPvtNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Returns the average power, in dBm, in the useful portion of the burst.
        ///     Refer to the Useful Portion of a Burst topic for more information.    
        /// 
        /// </summary>
        public int GetPvtResultsAveragePower(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.PvtResultsAveragePower, channel, out val);
        }

        /// <summary>
        /// Returns the maximum power, in dBm. The toolkit calculates the average over a specified number of averages and returns the maximum of these averaged values.  
        /// 
        /// </summary>
        public int GetPvtResultsMaximumPower(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.PvtResultsMaximumPower, channel, out val);
        }

        /// <summary>
        /// Indicates whether the power versus time (PvT) measurement passed or failed the mask specification.  
        /// 
        /// </summary>
        public int GetPvtResultsMeasurementStatus(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.PvtResultsMeasurementStatus, channel, out val);
        }

        /// <summary>
        /// Returns the minimum power, in dBm. The toolkit calculates the average over a specified number of averages and returns the minimum of these averaged values.  
        /// 
        /// </summary>
        public int GetPvtResultsMinimumPower(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.PvtResultsMinimumPower, channel, out val);
        }

        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the lower mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.  
        /// 
        /// </summary>
        public int SetPvtUserDefinedLowerMaskRelativePower(string channel, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.PvtUserDefinedLowerMaskRelativePower, channel, dataArray);
        }

        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the lower mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.  
        /// 
        /// </summary>
        public int GetPvtUserDefinedLowerMaskRelativePower(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.PvtUserDefinedLowerMaskRelativePower, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the lower mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int SetPvtUserDefinedLowerMaskTime(string channel, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.PvtUserDefinedLowerMaskTime, channel, dataArray);
        }

        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the lower mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int GetPvtUserDefinedLowerMaskTime(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.PvtUserDefinedLowerMaskTime, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the upper mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.  
        /// 
        /// </summary>
        public int SetPvtUserDefinedUpperMaskRelativePower(string channel, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.PvtUserDefinedUpperMaskRelativePower, channel, dataArray);
        }

        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the upper mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.  
        /// 
        /// </summary>
        public int GetPvtUserDefinedUpperMaskRelativePower(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.PvtUserDefinedUpperMaskRelativePower, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the upper mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int SetPvtUserDefinedUpperMaskTime(string channel, double[] dataArray)
        {
            return SetDoubleArray(niGSMSAProperties.PvtUserDefinedUpperMaskTime, channel, dataArray);
        }

        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the upper mask for a user-defined mask when the NIGSMSA_PVT_MASK_TYPE    attribute is set to NIGSMSA_VAL_MASK_TYPE_USER_DEFINED.  
        /// 
        /// </summary>
        public int GetPvtUserDefinedUpperMaskTime(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niGSMSAProperties.PvtUserDefinedUpperMaskTime, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Returns the length of the record to acquire, in seconds.
        ///     This attribute includes delays due to the measurement filter and may be greater than the ideal GSM burst length. If you do not use the niGSMSA_ConfigureHardware function,    multiply this value by the NIRFSA_ATTR_IQ_RATE attribute and set the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsAcquisitionTime(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.RecommendedHardwareSettingsAcquisitionTime, channel, out val);
        }

        /// <summary>
        /// Returns the minimum time, in seconds, during which the signal level must be below the trigger value for triggering to occur.
        ///     If you do not use the niGSMSA_ConfigureHardware function, pass this attribute to the NIRFSA_ATTR_REF_TRIGGER_MINIMUM_QUIET_TIME attribute.  
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsMinimumQuietTime(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.RecommendedHardwareSettingsMinimumQuietTime, channel, out val);
        }

        /// <summary>
        /// Returns the number of records to acquire.
        ///     The toolkit calculates this attribute using the following formula: number of records = number of averages/number of timeslots,    where the number of averages is the maximum of the number of averages for all enabled measurements.
        ///     If you do not use the niGSMSA_ConfigureHardware function, pass the NIGSMSA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS    attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.    
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsNumberOfRecords(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.RecommendedHardwareSettingsNumberOfRecords, channel, out val);
        }

        /// <summary>
        /// Returns the post-trigger delay, in seconds.
        ///     Add this value to the absolute timestamp element of the wfmInfo parameter in the niRFSA_ReadIQSingleRecordComplexF64 function.    Use this attribute when the actual signal to be measured is not generated immediately when the trigger occurs but is generated after a delay.  
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsPostTriggerDelay(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.RecommendedHardwareSettingsPostTriggerDelay, channel, out val);
        }

        /// <summary>
        /// Returns the pre-trigger delay, in seconds.
        ///     This attribute is used to acquire data prior to the trigger to account for the delays in the measurement process. If you do not use the niGSMSA_ConfigureHardware function,    multiply this value by the NIRFSA_ATTR_IQ_RATE attribute and set the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsPreTriggerDelay(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.RecommendedHardwareSettingsPreTriggerDelay, channel, out val);
        }

        /// <summary>
        /// Returns the recommended sampling rate, in hertz (Hz), for the RF signal analyzer.
        ///       If you do not use the niGSMSA_ConfigureHardware function, pass this attribute to the niRFSA_ConfigureIQRate function.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsSamplingRate(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.RecommendedHardwareSettingsSamplingRate, channel, out val);
        }

        /// <summary>
        /// Specifies the training sequence in the burst, as defined in section 5.2.3 of the 3GPP TS 45.002 Specifications 8.0.0.
        ///     If you set the NIGSMSA_BURST_SYNCHRONIZATION_ENABLED attribute to NIGSMSA_VAL_TRUE and the    NIGSMSA_TSC_DETECTION_ENABLED attribute to NIGSMSA_VAL_FALSE, the toolkit uses the training sequence that you specify    in the NIGSMSA_TSC attribute to synchronize to the start of the burst.
        ///     The default value is NIGSMSA_VAL_TSC0.  
        /// 
        /// </summary>
        public int SetTsc(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.Tsc, channel, value);
        }

        /// <summary>
        /// Specifies the training sequence in the burst, as defined in section 5.2.3 of the 3GPP TS 45.002 Specifications 8.0.0.
        ///     If you set the NIGSMSA_BURST_SYNCHRONIZATION_ENABLED attribute to NIGSMSA_VAL_TRUE and the    NIGSMSA_TSC_DETECTION_ENABLED attribute to NIGSMSA_VAL_FALSE, the toolkit uses the training sequence that you specify    in the NIGSMSA_TSC attribute to synchronize to the start of the burst.
        ///     The default value is NIGSMSA_VAL_TSC0.  
        /// 
        /// </summary>
        public int GetTsc(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.Tsc, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable the auto detection of TSC.
        ///     The default value is NIGSMSA_VAL_TRUE.  
        /// 
        /// </summary>
        public int SetTscAutoDetectionEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TscAutoDetectionEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable the auto detection of TSC.
        ///     The default value is NIGSMSA_VAL_TRUE.  
        /// 
        /// </summary>
        public int GetTscAutoDetectionEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TscAutoDetectionEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable the traces returned by transmit power (TxP) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int SetTxpAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TxpAllTracesEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable the traces returned by transmit power (TxP) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int GetTxpAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TxpAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the averaging type for transmit power (TxP) measurements.
        ///     The default value is NIGSMSA_VAL_TXP_RMS_AVERAGING.  
        /// 
        /// </summary>
        public int SetTxpAveragingMode(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TxpAveragingMode, channel, value);
        }

        /// <summary>
        /// Specifies the averaging type for transmit power (TxP) measurements.
        ///     The default value is NIGSMSA_VAL_TXP_RMS_AVERAGING.  
        /// 
        /// </summary>
        public int GetTxpAveragingMode(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TxpAveragingMode, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable transmit power (TxP) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int SetTxpEnabled(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TxpEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to enable transmit power (TxP) measurements.
        ///     The default value is NIGSMSA_VAL_FALSE.  
        /// 
        /// </summary>
        public int GetTxpEnabled(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TxpEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the 3 dB double-sided bandwidth, in hertz (Hz), of the specified low-pass filter.
        ///     The default value is 500k. Valid values are 10 to 20M, inclusive.  
        /// 
        /// </summary>
        public int SetTxpRbwFilterBandwidth(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.TxpRbwFilterBandwidth, channel, value);
        }

        /// <summary>
        /// Specifies the 3 dB double-sided bandwidth, in hertz (Hz), of the specified low-pass filter.
        ///     The default value is 500k. Valid values are 10 to 20M, inclusive.  
        /// 
        /// </summary>
        public int GetTxpRbwFilterBandwidth(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.TxpRbwFilterBandwidth, channel, out val);
        }

        /// <summary>
        /// Specifies the type of front end low-pass filter used for transmit power (TxP) measurements.
        ///     Note: This is the first filter that is applied on the acquired waveform in the niGSMSA_Analyze function.
        ///     The default value is NIGSMSA_VAL_FILTER_TYPE_GAUSSIAN.  
        /// 
        /// </summary>
        public int SetTxpRbwFilterType(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TxpRbwFilterType, channel, value);
        }

        /// <summary>
        /// Specifies the type of front end low-pass filter used for transmit power (TxP) measurements.
        ///     Note: This is the first filter that is applied on the acquired waveform in the niGSMSA_Analyze function.
        ///     The default value is NIGSMSA_VAL_FILTER_TYPE_GAUSSIAN.  
        /// 
        /// </summary>
        public int GetTxpRbwFilterType(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TxpRbwFilterType, channel, out val);
        }

        /// <summary>
        /// Specifies the additional time, in seconds, to acquire data before the trigger occurs.    The toolkit uses this attribute to compute the pre-trigger and post-trigger delays.
        ///       The default value is 0.  
        /// 
        /// </summary>
        public int SetTriggerDelay(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.TriggerDelay, channel, value);
        }
        /// <summary>
        /// Specifies the additional time, in seconds, to acquire data before the trigger occurs.    The toolkit uses this attribute to compute the pre-trigger and post-trigger delays.
        ///       The default value is 0.  
        /// 
        /// </summary>
        public int GetTriggerDelay(string channel, out double value)
        {
            return GetDouble(niGSMSAProperties.TriggerDelay, channel, out value);
        }

        /// <summary>
        /// Specifies the threshold value. This value is in dBm if the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_ABSOLUTE and in dB if the    NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_RELATIVE.
        ///     If the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_ABSOLUTE, the toolkit uses the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_LEVEL attribute as the threshold level.    The toolkit assumes this value to be in dBm. If the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_RELATIVE, the threshold level is relative to the    peak value in the acquired waveform.  
        /// 
        /// </summary>
        public int SetTxpMeasurementThresholdLevel(string channel, double value)
        {
            return SetDouble(niGSMSAProperties.TxpMeasurementThresholdLevel, channel, value);
        }

        /// <summary>
        /// Specifies the threshold value. This value is in dBm if the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_ABSOLUTE and in dB if the    NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_RELATIVE.
        ///     If the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_ABSOLUTE, the toolkit uses the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_LEVEL attribute as the threshold level.    The toolkit assumes this value to be in dBm. If the NIGSMSA_TXP_MEASUREMENT_THRESHOLD_TYPE attribute is set to NIGSMSA_VAL_RELATIVE, the threshold level is relative to the    peak value in the acquired waveform.  
        /// 
        /// </summary>
        public int GetTxpMeasurementThresholdLevel(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.TxpMeasurementThresholdLevel, channel, out val);
        }

        /// <summary>
        /// Specifies the type of threshold used for transmit power (TxP) measurements.
        ///     The toolkit considers only the points that are above the threshold level that you specify in the    NIGSMSA_TXP_MEASUREMENT_THRESHOLD_LEVEL attribute for performing transmit power measurements.
        ///     The default value is NIGSMSA_VAL_RELATIVE.  
        /// 
        /// </summary>
        public int SetTxpMeasurementThresholdType(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TxpMeasurementThresholdType, channel, value);
        }

        /// <summary>
        /// Specifies the type of threshold used for transmit power (TxP) measurements.
        ///     The toolkit considers only the points that are above the threshold level that you specify in the    NIGSMSA_TXP_MEASUREMENT_THRESHOLD_LEVEL attribute for performing transmit power measurements.
        ///     The default value is NIGSMSA_VAL_RELATIVE.  
        /// 
        /// </summary>
        public int GetTxpMeasurementThresholdType(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TxpMeasurementThresholdType, channel, out val);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform transmit power (TxP) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int SetTxpNumberOfAverages(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.TxpNumberOfAverages, channel, value);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform transmit power (TxP) measurements.
        ///     The default value is 10. Valid values are 1 to 10,000 (inclusive).  
        /// 
        /// </summary>
        public int GetTxpNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.TxpNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Returns the average power, in dBm, in the useful portion of the burst.
        ///       Refer to the Useful Portion of a Burst topic for more information.   
        /// 
        /// </summary>
        public int GetTxpResultsAveragePower(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.TxpResultsAveragePower, channel, out val);
        }

        /// <summary>
        /// Returns the maximum power, in dBm. The toolkit calculates the average over a specified number of averages and returns the    maximum of these averaged values.  
        /// 
        /// </summary>
        public int GetTxpResultsMaximumPower(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.TxpResultsMaximumPower, channel, out val);
        }

        /// <summary>
        /// Returns the minimum power, in dBm. The toolkit calculates the average over a specified number of averages and returns    the minimum of these averaged values.  
        /// 
        /// </summary>
        public int GetTxpResultsMinimumPower(string channel, out double val)
        {
            return GetDouble(niGSMSAProperties.TxpResultsMinimumPower, channel, out val);
        }

        /// <summary>
        /// Specifies the type of unit under test (UUT) generating the signal that the toolkit analyzes.
        ///     The toolkit uses the NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes to calculate the correct masks for    power versus time (PvT) measurements and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute    is set to NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///     Use the niGSMSA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///     The default value is NIGSMSA_VAL_UUT_MS.  
        /// 
        /// </summary>
        public int SetUut(string channel, int value)
        {
            return SetInt32(niGSMSAProperties.Uut, channel, value);
        }

        /// <summary>
        /// Specifies the type of unit under test (UUT) generating the signal that the toolkit analyzes.
        ///     The toolkit uses the NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes to calculate the correct masks for    power versus time (PvT) measurements and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIGSMSA_ORFS_OFFSET_FREQUENCY_MODE attribute    is set to NIGSMSA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///     Use the niGSMSA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIGSMSA_UUT, NIGSMSA_BAND, and NIGSMSA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///     The default value is NIGSMSA_VAL_UUT_MS.  
        /// 
        /// </summary>
        public int GetUut(string channel, out int val)
        {
            return GetInt32(niGSMSAProperties.Uut, channel, out val);
        }

        private int SetInt32(niGSMSAProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            return TestForError(PInvoke.niGSMSA_SetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetInt32(niGSMSAProperties propertyId, string repeatedCapabilityOrChannel, out int val)
        {
            return TestForError(PInvoke.niGSMSA_GetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetDouble(niGSMSAProperties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            return TestForError(PInvoke.niGSMSA_SetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetDouble(niGSMSAProperties propertyId, string repeatedCapabilityOrChannel, out double val)
        {
            return TestForError(PInvoke.niGSMSA_GetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetDoubleArray(niGSMSAProperties propertyId, string repeatedCapabilityOrChannel, double[] dataArray)
        {
            return TestForError(PInvoke.niGSMSA_SetVectorAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, dataArray, dataArray.Length));
        }

        private int GetDoubleArray(niGSMSAProperties propertyId, string repeatedCapabilityOrChannel, double[] dataArray, out int actualNumberOfPoints)
        {
            return TestForError(PInvoke.niGSMSA_GetVectorAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, dataArray, dataArray.Length, out actualNumberOfPoints));
        }

        private class PInvoke
        {
            const string nativeDllName = "niGSMSA_net.dll";
            const string nativeAttributeDllName = "niGSMAnalysisattribs.dll";

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_AnalyzeIQComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_AnalyzeIQComplexF64(HandleRef session, double t0, double dt, niComplexNumber[] waveform, int length, int reset, out int done);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ARFCNToCarrierFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ARFCNToCarrierFrequency(int uUT, int band, int aRFCN, out double carrierFrequency);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_CloseSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_GetErrorString(HandleRef session, int errorCode, StringBuilder errorMessage, int length);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_GetScalarAttributeF64(HandleRef session, string channelString, niGSMSAProperties attributeID, out double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_GetScalarAttributeI32(HandleRef session, string channelString, niGSMSAProperties attributeID, out int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_GetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_GetVectorAttributeF64(HandleRef session, string channelString, niGSMSAProperties attributeID, [In, Out] double[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ModAccGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ModAccGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ModAccGetCurrentIterationDemodulatedBitTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ModAccGetCurrentIterationDemodulatedBitTrace(HandleRef session, out int[] demodulatedBits, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ModAccGetCurrentIterationPhaseErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ModAccGetCurrentIterationPhaseErrorTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] phaseError, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_OpenSession(string sessionName, int toolkitCompatibilityVersion, out int isNewSession, out IntPtr session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetModulationAbsolutePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetModulationAbsolutePowersTrace(HandleRef session, [In, Out] double[] modulationAbsolutePowers, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetModulationOffsetFrequenciesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetModulationOffsetFrequenciesTrace(HandleRef session, [In, Out] double[] modulationOffsetFrequencies, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetModulationRelativePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetModulationRelativePowersTrace(HandleRef session, [In, Out] double[] modulationRelativePowers, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetSwitchingAbsolutePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetSwitchingAbsolutePowersTrace(HandleRef session, [In, Out] double[] switchingAbsolutePowers, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetSwitchingOffsetFrequenciesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetSwitchingOffsetFrequenciesTrace(HandleRef session, [In, Out] double[] switchingOffsetFrequencies, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ORFSGetSwitchingRelativePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ORFSGetSwitchingRelativePowersTrace(HandleRef session, [In, Out] double[] switchingRelativePowers, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_PvTGetAverageSignalPowerTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_PvTGetAverageSignalPowerTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] averageSignalPower, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_PvTGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_PvTGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_PvTGetLowerMaskTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_PvTGetLowerMaskTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] lowerMask, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_PvTGetUpperMaskTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_PvTGetUpperMaskTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] upperMask, int length, out int traceSize);

            [DllImport(nativeAttributeDllName, EntryPoint = "niAttributeReset", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ResetAttribute(HandleRef session, string channelString, niGSMSAProperties attributeID);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ResetSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_RFSAAutoLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_RFSAAutoLevel(System.Runtime.InteropServices.HandleRef rfsaHandle, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_RFSAConfigureHardware", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_RFSAConfigureHardware(HandleRef session, System.Runtime.InteropServices.HandleRef rfsaHandle);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_SetScalarAttributeF64(HandleRef session, string channelString, niGSMSAProperties attributeID, double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_SetScalarAttributeI32(HandleRef session, string channelString, niGSMSAProperties attributeID, int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_SetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_SetVectorAttributeF64(HandleRef session, string channelString, niGSMSAProperties attributeID, double[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_ToolkitCheckError", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_ToolkitCheckError(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_TxPGetAveragePowerTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_TxPGetAveragePowerTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] averagePower, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_TxPGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_TxPGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int length, out int traceSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSA_RFSAMeasure_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSA_RFSAMeasure_vi(HandleRef session, HandleRef rfsaHandle, double timeout);
        }

        private int GetErrorString(int status, StringBuilder msg)
        {
            int size = PInvoke.niGSMSA_GetErrorString(Handle, status, null, 0);
            if ((size >= 0))
            {
                msg.Capacity = size;
                PInvoke.niGSMSA_GetErrorString(Handle, status, msg, size);
            }
            return status;
        }

        private int TestForError(int status)
        {
            if ((status < 0))
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                status = GetErrorString(status, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }

        private int TestForError(int status, HandleRef rfsaHandle)
        {
            if ((status < 0))
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                GetErrorString(status, msg);
                int tempStatus = status;
                //get rfsa detailed error message
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSA.GetError(rfsaHandle, tempStatus, msg);
                //get rfsa general error message
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSA.ErrorMessage(rfsaHandle, status, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }

        #region IDisposable Members

        /// <summary>
        /// Closes the niGSM analysis unnamed session and releases resources associated with that unnamed session.
        /// 
        /// </summary>
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
            // Dispose does not close a named session. Users must call Close() to close a named session.
            if (!_isNamedSession)
            {
                //Dispose unmanaged resources
                if (!Handle.Handle.Equals(IntPtr.Zero))
                {
                    PInvoke.niGSMSA_CloseSession(Handle);
                }
            }
        }

        #endregion
    }

    public class niGSMSAConstants
    {

        public const int UutBts = 0;

        public const int UutMs = 1;

        public const int BandPgsm = 0;

        public const int BandEgsm = 1;

        public const int BandRgsm = 2;

        public const int BandDcs1800 = 3;

        public const int BandPcs1900 = 4;

        public const int BandGsm450 = 5;

        public const int BandGsm480 = 6;

        public const int BandGsm850 = 7;

        public const int BandGsm750 = 8;

        public const int BandTgsm810 = 9;

        public const int False = 0;

        public const int True = 1;

        public const int Tsc0 = 0;

        public const int Tsc1 = 1;

        public const int Tsc2 = 2;

        public const int Tsc3 = 3;

        public const int Tsc4 = 4;

        public const int Tsc5 = 5;

        public const int Tsc6 = 6;

        public const int Tsc7 = 7;

        public const int MaskTypeStandard = 0;

        public const int MaskTypeUserDefined = 1;

        public const int FilterTypeGaussian = 0;

        public const int FilterTypeFlat = 1;

        public const int FilterTypeNone = 2;

        public const int PvtRmsAveraging = 0;

        public const int PvtLogAveraging = 1;

        public const int PvtPeakHoldAveraging = 2;

        public const int PvtMinimumHoldAveraging = 3;

        public const int MeasurementStatusFail = 0;

        public const int MeasurementStatusPass = 1;

        public const int OrfsMeasurementTypeModulationSwitching = 0;

        public const int OrfsMeasurementTypeModulation = 1;

        public const int OrfsMeasurementTypeSwitching = 2;

        public const int OffsetFrequencyModeStandard = 0;

        public const int OffsetFrequencyModeUserDefined = 1;

        public const int OrfsRmsAveraging = 0;

        public const int OrfsLogAveraging = 1;

        public const int Absolute = 0;

        public const int Relative = 1;

        public const int TxpRmsAveraging = 0;

        public const int TxpLogAveraging = 1;

        public const int TxpPeakHoldAveraging = 2;

        public const int TxpMinimumHoldAveraging = 3;

        public const int ToolkitCompatibilityVersion100 = 100;

    }

    public enum niGSMSAProperties
    {

        /// <summary>
        /// int
        /// </summary>
        AdvancedToolkitCompatibilityVersion = 65533,

        /// <summary>
        /// int
        /// </summary>
        Arfcn = 26,

        /// <summary>
        /// int
        /// </summary>
        Band = 25,

        /// <summary>
        /// int
        /// </summary>
        BurstSynchronizationEnabled = 3,

        /// <summary>
        /// int
        /// </summary>
        ModaccAllTracesEnabled = 155,

        /// <summary>
        /// int
        /// </summary>
        ModaccEnabled = 101,

        /// <summary>
        /// int
        /// </summary>
        ModaccNumberOfAverages = 46,

        /// <summary>
        /// double
        /// </summary>
        ModaccResultsAverageFrequencyError = 54,

        /// <summary>
        /// double
        /// </summary>
        ModaccResultsAverageIqGainImbalance = 58,

        /// <summary>
        /// double
        /// </summary>
        ModaccResultsAverageOriginOffset = 56,

        /// <summary>
        /// double
        /// </summary>
        ModaccResultsAverageRmsPhaseError = 49,

        /// <summary>
        /// int
        /// </summary>
        ModaccResultsDetectedTsc = 121,

        /// <summary>
        /// double
        /// </summary>
        ModaccResultsMaximumPeakPhaseError = 52,

        /// <summary>
        /// int
        /// </summary>
        NumberOfTimeslots = 1,

        /// <summary>
        /// int
        /// </summary>
        OrfsAllTracesEnabled = 156,

        /// <summary>
        /// int
        /// </summary>
        OrfsEnabled = 102,

        /// <summary>
        /// int
        /// </summary>
        OrfsMeasurementType = 70,

        /// <summary>
        /// int
        /// </summary>
        OrfsModulationAveragingMode = 69,


        /// <summary>
        /// int
        /// </summary>
        OrfsFastAveragingMode = 0x44,


        /// <summary>
        /// Double[]
        /// </summary>
        OrfsModulationOffsetFrequencies = 83,

        /// <summary>
        /// double
        /// </summary>
        OrfsModulationRbwCarrier = 77,

        /// <summary>
        /// double
        /// </summary>
        OrfsModulationRbwFarOffset = 79,

        /// <summary>
        /// double
        /// </summary>
        OrfsModulationRbwNearOffset = 78,

        /// <summary>
        /// int
        /// </summary>
        OrfsNoiseCompensationEnabled = 76,

        /// <summary>
        /// Double[]
        /// </summary>
        OrfsNoiseFloors = 75,

        /// <summary>
        /// int
        /// </summary>
        OrfsNumberOfAverages = 66,

        /// <summary>
        /// int
        /// </summary>
        OrfsOffsetFrequencyMode = 74,

        /// <summary>
        /// Double[]
        /// </summary>
        OrfsSwitchingOffsetFrequencies = 86,

        /// <summary>
        /// double
        /// </summary>
        OrfsSwitchingRbwCarrier = 80,

        /// <summary>
        /// double
        /// </summary>
        OrfsSwitchingRbwFarOffset = 82,

        /// <summary>
        /// double
        /// </summary>
        OrfsSwitchingRbwNearOffset = 81,

        /// <summary>
        /// int
        /// </summary>
        PowerControlLevel = 22,

        /// <summary>
        /// int
        /// </summary>
        PvtAllTracesEnabled = 154,

        /// <summary>
        /// int
        /// </summary>
        PvtAveragingMode = 10,

        /// <summary>
        /// int
        /// </summary>
        PvtEnabled = 100,

        /// <summary>
        /// int
        /// </summary>
        PvtMaskType = 2,

        /// <summary>
        /// int
        /// </summary>
        PvtNumberOfAverages = 7,

        /// <summary>
        /// double
        /// </summary>
        PvtRbwFilterBandwidth = 6,

        /// <summary>
        /// int
        /// </summary>
        PvtRbwFilterType = 5,

        /// <summary>
        /// double
        /// </summary>
        PvtResultsAveragePower = 12,

        /// <summary>
        /// double
        /// </summary>
        PvtResultsMaximumPower = 18,

        /// <summary>
        /// int
        /// </summary>
        PvtResultsMeasurementStatus = 21,

        /// <summary>
        /// double
        /// </summary>
        PvtResultsMinimumPower = 19,

        /// <summary>
        /// Double[]
        /// </summary>
        PvtUserDefinedLowerMaskRelativePower = 28,

        /// <summary>
        /// Double[]
        /// </summary>
        PvtUserDefinedLowerMaskTime = 27,

        /// <summary>
        /// Double[]
        /// </summary>
        PvtUserDefinedUpperMaskRelativePower = 31,

        /// <summary>
        /// Double[]
        /// </summary>
        PvtUserDefinedUpperMaskTime = 30,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsAcquisitionTime = 35,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsMinimumQuietTime = 38,

        /// <summary>
        /// int
        /// </summary>
        RecommendedHardwareSettingsNumberOfRecords = 33,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsPostTriggerDelay = 37,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsPreTriggerDelay = 36,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsSamplingRate = 34,

        /// <summary>
        /// int
        /// </summary>
        Tsc = 4,

        /// <summary>
        /// int
        /// </summary>
        TscAutoDetectionEnabled = 120,

        /// <summary>
        /// int
        /// </summary>
        TxpAllTracesEnabled = 157,

        /// <summary>
        /// int
        /// </summary>
        TxpAveragingMode = 110,

        /// <summary>
        /// int
        /// </summary>
        TxpEnabled = 118,

        /// <summary>
        /// double
        /// </summary>
        TriggerDelay = 24,

        /// <summary>
        /// double
        /// </summary>
        TxpMeasurementThresholdLevel = 106,

        /// <summary>
        /// int
        /// </summary>
        TxpMeasurementThresholdType = 105,

        /// <summary>
        /// int
        /// </summary>
        TxpNumberOfAverages = 107,

        /// <summary>
        /// double
        /// </summary>
        TxpRbwFilterBandwidth = 104,

        /// <summary>
        /// int
        /// </summary>
        TxpRbwFilterType = 103,

        /// <summary>
        /// double
        /// </summary>
        TxpResultsAveragePower = 111,

        /// <summary>
        /// double
        /// </summary>
        TxpResultsMaximumPower = 114,

        /// <summary>
        /// double
        /// </summary>
        TxpResultsMinimumPower = 115,

        /// <summary>
        /// int
        /// </summary>
        Uut = 0,

    }
}


    