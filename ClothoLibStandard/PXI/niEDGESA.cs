using System;
using System.Runtime.InteropServices;
using System.Text;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.RFToolkits.Interop
{
    public class niEDGESA
    {
        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _isNamedSession;

        /// <summary>
        /// Looks up an existing niEDGE analysis session and returns the refnum that you can pass to subsequent niEDGE analysis functions. If the lookup fails, the niEDGESA_OpenSession function creates a new niEDGE analysis session and returns a new refnum.
        /// 
        /// </summary>
        /// <param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niEDGESA_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        public niEDGESA(int toolkitCompatibilityVersion)
        {
            IntPtr handle;
            short isNewSession;
            int pInvokeResult = PInvoke.niEDGESA_OpenSession(string.Empty, toolkitCompatibilityVersion, out isNewSession, out handle);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            _isNamedSession = false;
        }
        /// <summary>
        /// Looks up an existing niEDGE analysis session and returns the refnum that you can pass to subsequent niEDGE analysis functions. If the lookup fails, the niEDGESA_OpenSession function creates a new niEDGE analysis session and returns a new refnum.
        /// Make sure you call Close for the named session. Dispose does not close named session.
        /// </summary>
        ///<param>
        /// sessionName
        /// char[]
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. To get the reference to an already-opened session x, specify x as the session name. 
        ///  You can obtain the reference to an existing session multiple times if you have not called the niEDGESA_CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string or NULL to the sessionName parameter.
        /// Tip&nbsp;&nbsp;National Instruments recommends that you call the niEDGESA_CloseSession function for each uniquely-named instance of the niEDGESA_OpenSession function or each instance of the niEDGESA_OpenSession function with an unnamed session.
        /// 
        ///</param>
        ///<param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niEDGESA_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        ///<param>
        /// isNewSession
        /// int32*
        /// Returns NIEDGESA_VAL_ENABLED_TRUE if the function creates a new session. This parameter returns NIEDGESA_VAL_ENABLED_FALSE if the function returns a reference to an existing session.
        /// 
        ///</param>
        ///<param>
        /// session
        /// HandleRef*
        /// Returns the niEDGE analysis session.
        /// 
        ///</param>
        public niEDGESA(string sessionName, int toolkitCompatibilityVersion, out short isNewSession)
        {
            IntPtr handle;
            int pInvokeResult = PInvoke.niEDGESA_OpenSession(sessionName, toolkitCompatibilityVersion, out isNewSession, out handle);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            if (String.IsNullOrEmpty(sessionName))
                _isNamedSession = false;
            else
                _isNamedSession = true;
        }

        ~niEDGESA() { Dispose(false); }

        public HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        /// <summary>
        /// Performs error vector magnitude (EVM), power versus time (PvT), and output radio frequency spectrum (ORFS) measurements on the input complex waveform. You can enable all these measurements and perform them simultaneously. Call this function as many times as specified by the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS attribute.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// niComplexNumber[]
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
        /// done
        /// int*
        /// Indicates whether the function has finished performing the measurements.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int AnalyzeIQComplexF64(double t0, double dt, niComplexNumber[] waveform, int length, int reset, out int done)
        {
            int pInvokeResult = PInvoke.niEDGESA_AnalyzeIQComplexF64(Handle, t0, dt, waveform, length, reset, out done);
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
        /// Specifies the type of UUT generating the signal that the toolkit analyzes. The default value is NIEDGESA_VAL_UUT_MS. 
        ///    NIEDGESA_VAL_UUT_BTS (0)
        ///   Specifies that the toolkit analyzes the signal from a base transmit station (BTS).
        ///       Note  The current version of the toolkit supports only normal BTS. 
        ///    NIEDGESA_VAL_UUT_MS (1)
        ///   Specifies that the toolkit analyzes the signal from a mobile station (MS). This value is the default.
        /// 
        ///</param>
        ///<param>
        /// band
        /// int32
        /// Specifies the band of operation. The default value is NIEDGESA_VAL_BAND_PGSM.
        /// NIEDGESA_VAL_BAND_PGSM (0)
        /// Specifies a primary GSM (PGSM) band in the 900 MHz band. This value is the default.
        /// NIEDGESA_VAL_BAND_EGSM (1)
        /// Specifies an extended GSM (EGSM) band in the 900 MHz band.
        /// NIEDGESA_VAL_BAND_RGSM (2)
        /// Specifies a railway GSM (RGSM) band in the 900 MHz band.
        /// NIEDGESA_VAL_BAND_DCS1800 (3)
        /// Specifies a digital cellular system 1800 (DCS 1800) band. This band is also known as GSM 1800.
        /// NIEDGESA_VAL_BAND_PCS1900 (4)
        /// Specifies a personal communications service 1900 (PCS 1900) band. This band is also known as GSM 1900.
        /// NIEDGESA_VAL_BAND_GSM450 (5)
        /// Specifies a GSM 450 band.
        /// NIEDGESA_VAL_BAND_GSM480 (6)
        /// Specifies a GSM 480 band.
        /// NIEDGESA_VAL_BAND_GSM850 (7)
        /// Specifies a GSM 850 band.
        /// NIEDGESA_VAL_BAND_GSM750 (8)
        /// Specifies a GSM 750 band.
        /// NIEDGESA_VAL_BAND_TGSM810 (9)
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
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ARFCNToCarrierFrequency(int uUT, int band, int aRFCN, out double carrierFrequency)
        {
            int pInvokeResult = PInvoke.niEDGESA_ARFCNToCarrierFrequency(uUT, band, aRFCN, out carrierFrequency);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Closes the niEDGE analysis session and releases resources associated with that session.
        /// 
        /// </summary>
        public void Close()
        {
            if (!_isNamedSession)
                Dispose();
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                    PInvoke.niEDGESA_CloseSession(Handle);
            }
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to data parameter to get size of the array in actualArraySize parameter.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// niComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the acquiredIQWaveform parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int EVMGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_EVMGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the hard decision decoded bits from the useful portion of the burst. The toolkit uses these bits to generate the reference signal for computing the error vector magnitude (EVM) of the acquired signal.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// demodulatedBits
        /// int32[]
        /// Returns the hard decision decoded bits from the useful portion of the burst. You can pass NULL to the demodulatedBits parameter to get size of the array in the actualNumDataArrayElements parameter. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the demodulatedBits array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the demodulatedBits parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int EVMGetCurrentIterationDemodulatedBitsTrace(int[] demodulatedBits, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_EVMGetCurrentIterationDemodulatedBitsTrace(Handle, demodulatedBits, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns a trace of the mean error vector magnitude (EVM), multiplied by 100, for the last burst in the current iteration.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// EVM
        /// float64[]
        /// Returns a trace of the mean EVM, multiplied by 100, for the last burst in the current iteration. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the EVM array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the EVM parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int EVMGetCurrentIterationEVMTrace(out double t0, out double dt, double[] eVM, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_EVMGetCurrentIterationEVMTrace(Handle, out t0, out dt, eVM, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the magnitude error trace for the last burst in the current iteration.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// magnitudeError
        /// float64[]
        /// Returns the magnitude error trace for the last burst in the current iteration. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the magnitudeError array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the magnitudeError parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int EVMGetCurrentIterationMagnitudeErrorTrace(out double t0, out double dt, double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_EVMGetCurrentIterationMagnitudeErrorTrace(Handle, out t0, out dt, magnitudeError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the phase of the error vector trace for the last analyzed burst in the current iteration.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// Returns the phase of the error vector trace for the last analyzed burst in the current iteration. You can pass NULL to the phaseError parameter to get size of the array in the actualNumDataArrayElements parameter. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the phaseError array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the phaseError parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int EVMGetCurrentIterationPhaseErrorTrace(out double t0, out double dt, double[] phaseError, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_EVMGetCurrentIterationPhaseErrorTrace(Handle, out t0, out dt, phaseError, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Takes the error code returned by niEDGE analysis functions and returns the interpretation as a user-readable string. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// errorCode
        /// int32
        /// Specifies the error code that is returned from any of the niEDGE analysis functions.
        /// 
        ///</param>
        ///<param>
        /// errorMessage
        /// char[]
        /// Returns the user-readable message string that corresponds to the error code you specify. The errorMessage buffer must have at least as many elements as are indicated in the errorMessageLength parameter. If you pass NULL to the errorMessage parameter, the function returns the actual length of the error message.
        /// 
        ///</param>
        ///<param>
        /// errorMessageLength
        /// int32
        /// Specifies the length of the errorMessage buffer.
        /// 
        ///</param>
        ///<returns>
        /// Takes the error code returned by niEDGE analysis functions and returns the interpretation as a user-readable string. 
        /// 
        ///</returns>
        public int GetErrorString(int errorCode, StringBuilder errorMessage, int errorMessageLength)
        {
            int pInvokeResult = PInvoke.niEDGESA_GetErrorString(Handle, errorCode, errorMessage, errorMessageLength);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Queries the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of a float64 niEDGE analysis attribute.
        /// 
        ///</param>
        ///<param>
        /// attributeValue
        /// float64*
        /// Returns the value of the attribute that you specify using the attributeID parameter.
        /// 
        ///</param>
        ///<returns>
        /// Queries the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        ///</returns>
        public int GetScalarAttributeF64(string channelString, niEDGESAProperties attributeID, out double attributeValue)
        {
            int pInvokeResult = PInvoke.niEDGESA_GetScalarAttributeF64(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Queries the value of an niEDGE analysis 32-bit integer (int32) attribute. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of an int32 niEDGE analysis attribute.
        /// 
        ///</param>
        ///<param>
        /// attributeValue
        /// int32*
        /// Returns the value of the attribute that you specify using the attributeID parameter.
        /// 
        ///</param>
        ///<returns>
        /// Queries the value of an niEDGE analysis 32-bit integer (int32) attribute. 
        /// 
        ///</returns>
        public int GetScalarAttributeI32(string channelString, niEDGESAProperties attributeID, out int attributeValue)
        {
            int pInvokeResult = PInvoke.niEDGESA_GetScalarAttributeI32(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Queries the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of a float64 niEDGE analysis attribute.
        /// 
        ///</param>
        ///<param>
        /// dataArray
        /// float64*
        /// Specifies the pointer to the float64 array to which you want to set the attribute.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the float64 array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements in the dataArray array. If the dataArray array is not large enough to hold all the samples, the function returns an error code and the actualNumDataArrayElements returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Queries the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        ///</returns>
        public int GetVectorAttributeF64(string channelString, niEDGESAProperties attributeID, double[] dataArray, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_GetVectorAttributeF64(Handle, channelString, attributeID, dataArray, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// niComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the acquiredIQWaveform parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the modulation powers at various offset frequencies. Use the niEDGESA_ORFSGetModulationOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The modulation powers represent the absolute power due to modulation at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// modulationAbsolutePowers
        /// float64[]
        /// Returns the absolute powers due to modulation. You can pass NULL to the modulationAbsolutePowers parameter to get size of the array in the actualNumDataArrayElements parameter. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the switchingAbsolutePowers array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the modulationAbsolutePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetModulationAbsolutePowersTrace(double[] modulationAbsolutePowers, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetModulationAbsolutePowersTrace(Handle, modulationAbsolutePowers, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the array of offset frequencies at which output radio frequency spectrum (ORFS) due to modulation measurement is performed.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// modulationOffsetFrequencies
        /// float64[]
        /// Returns the array of offset frequencies at which ORFS due modulation measurement is performed. You can pass NULL to the modulationOffsetFrequencies parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the modulationOffsetFrequencies array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the modulationOffsetFrequencies parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetModulationOffsetFrequenciesTrace(double[] modulationOffsetFrequencies, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetModulationOffsetFrequenciesTrace(Handle, modulationOffsetFrequencies, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the modulation powers at various offset frequencies. Use the niEDGESA_ORFSGetModulationOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The modulation powers represent the power relative to the carrier power due to modulation at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// modulationRelativePowers
        /// float64[]
        /// Returns the powers relative to the carrier power, due to modulation. You can pass NULL to the modulationRelativePowers parameter to get size of the array in the actualNumDataArrayElements parameter. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the modulationRelativePowers array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the modulationRelativePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetModulationRelativePowersTrace(double[] modulationRelativePowers, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetModulationRelativePowersTrace(Handle, modulationRelativePowers, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the switching powers at various offset frequencies. Use the niEDGESA_ORFSGetSwitchingOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The switching powers represent the power due to the switching part of the waveform.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// switchingAbsolutePowers
        /// float64[]
        /// Returns the absolute powers due to switching. You can pass NULL to the switchingAbsolutePowers parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the modulationAbsolutePowers array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the switchingAbsolutePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetSwitchingAbsolutePowersTrace(double[] switchingAbsolutePowers, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetSwitchingAbsolutePowersTrace(Handle, switchingAbsolutePowers, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the offset frequencies at which the output radio frequency spectrum (ORFS) due to switching measurement is performed. The switching powers represent the absolute power due to switching at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// switchingOffsetFrequencies
        /// float64[]
        /// Returns the array of offset frequencies. You can pass NULL to the switchingOffsetFrequencies parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the switchingOffsetFrequencies array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the switchingOffsetFrequencies parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetSwitchingOffsetFrequenciesTrace(double[] switchingOffsetFrequencies, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetSwitchingOffsetFrequenciesTrace(Handle, switchingOffsetFrequencies, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the switching powers at various offset frequencies. Use the niEDGESA_ORFSGetSwitchingOffsetFrequenciesTrace function to get a list of the corresponding offset frequencies. The switching powers represent the power relative to the carrier due to the switching part of the waveform at each offset frequency.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// switchingRelativePowers
        /// float64[]
        /// Returns the powers relative to the carrier power, due to switching. You can pass NULL to the switchingRelativePowers parameter to get size of the array in the actualNumDataArrayElements parameter. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the switchingRelativePowers array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the switchingRelativePowers parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ORFSGetSwitchingRelativePowersTrace(double[] switchingRelativePowers, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_ORFSGetSwitchingRelativePowersTrace(Handle, switchingRelativePowers, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the power versus time (PvT) trace averaged across timeslots.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// Returns the signal power averaged across timeslots. You can pass NULL to the averageSignalPower parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the averageSignalPower array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the averageSignalPower parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int PvTGetAverageSignalPowerTrace(out double t0, out double dt, double[] averageSignalPower, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_PvTGetAverageSignalPowerTrace(Handle, out t0, out dt, averageSignalPower, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the unprocessed IQ signal acquired from the RF signal analyzer.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// niComplexNumber[]
        /// Returns the unprocessed IQ signal acquired from RF Signal Analyzer. You can pass NULL to the acquiredIQWaveform parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the acquiredIQWaveform array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the acquiredIQWaveform parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int PvTGetCurrentIterationAcquiredIQWaveformTrace(out double t0, out double dt, niComplexNumber[] acquiredIQWaveform, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_PvTGetCurrentIterationAcquiredIQWaveformTrace(Handle, out t0, out dt, acquiredIQWaveform, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the lower mask used for power versus time (PvT) measurements.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// Returns the lower mask used for PvT measurements. You can pass NULL to the lowerMask parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the lowerMask array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the lowerMask parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int PvTGetLowerMaskTrace(out double t0, out double dt, double[] lowerMask, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_PvTGetLowerMaskTrace(Handle, out t0, out dt, lowerMask, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the upper mask used for power versus time (PvT) measurements.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
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
        /// Returns the upper mask used for PvT measurements. You can pass NULL to the upperMask parameter to get size of the array in the actualNumDataArrayElements parameter. 
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the upperMask array.
        /// 
        ///</param>
        ///<param>
        /// actualNumDataArrayElements
        /// int32*
        /// Returns the actual number of elements populated in the upperMask parameter. If the array is not large enough to hold all the samples, the function returns an error and the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int PvTGetUpperMaskTrace(out double t0, out double dt, double[] upperMask, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niEDGESA_PvTGetUpperMaskTrace(Handle, out t0, out dt, upperMask, dataArraySize, out actualNumDataArrayElements);
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
        public int RFSAMeasure(HandleRef rfsaHandle, double timeout)
        {
            int pInvokeResult = PInvoke.niEDGESA_RFSAMeasure_vi(Handle, rfsaHandle, timeout);
            TestForError(pInvokeResult, rfsaHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets the attribute specified in the attributeID parameter to its default value. You can reset only a writable attribute using this function.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of the niEDGE analysis attribute that you want to reset.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ResetAttribute(string channelString, niEDGESAProperties attributeID)
        {
            int pInvokeResult = PInvoke.niEDGESA_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets all the attributes of the session to their default values.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niEDGESA_ResetSession(Handle);
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
        /// Specifies the acquisition length, in seconds. The niEDGESA_RFSAAutoLevel function uses this value to compute the number of samples to acquire from the RF signal analyzer. 
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
        /// Examines the incoming signal to calculate the appropriate power level. This function then returns the estimated power level in the resultantReferenceLevel parameter. Use this feature if you need help in calculating an approximate setting for the power level for IQ measurements. This function queries the NIRFSA_ATTR_REFERENCE_LEVEL attribute and uses this value as the starting point for auto level calculations. Set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to the highest expected power level of the signal for faster convergence. For example, if the device under test (DUT) operates in the range of -10 dBm to -30 dBm, set the NIRFSA_ATTR_REFERENCE_LEVEL attribute to -10 dBm.
        /// 
        ///</returns>
        public int RFSAAutoLevel(HandleRef rfsaHandle, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel)
        {
            int pInvokeResult = PInvoke.niEDGESA_RFSAAutoLevel(Handle, bandwidth, measurementInterval, maxNumberofIterations, out resultantReferenceLevel);
            TestForError(pInvokeResult, rfsaHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Retrieves the recommended hardware settings from the niEDGE analysis session and sets these values to the appropriate niRFSA attributes.
        /// This function sets the following NI-RFSA attributes:
        ///     Sets the NIRFSA_ATTR_ACQUISITION_TYPE attribute to NIRFSA_VAL_IQ.
        ///     Sets the NIRFSA_ATTR_NUM_RECORDS_IS_FINITE attribute to VI_TRUE.
        ///     Sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_RECORDS attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.
        ///     Sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_SAMPLING_RATE_HZ attribute to the NIRFSA_ATTR_IQ_RATE attribute.
        ///     If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to NIRFSA_VAL_IQ_POWER_EDGE, this function sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME_SEC attribute to the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_MINIMUM_QUIET_TIME attribute. If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to any other value, this function sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME_SEC attribute to 0.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_ACQUISITION_TIME_SEC attribute, and sets the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.
        ///     Sets the NIRFSA_ATTR_NUM_SAMPLES_IS_FINITE attribute to VI_TRUE.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_PRE_TRIGGER_DELAY_SEC attribute, and sets the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// RFSAHandle
        /// ViSession
        /// Identifies the instrument session. The toolkit obtains this value from the niRFSA_init function or the niRFSA_InitWithOptions function.
        /// 
        ///</param>
        ///<returns>
        /// Retrieves the recommended hardware settings from the niEDGE analysis session and sets these values to the appropriate niRFSA attributes.
        /// This function sets the following NI-RFSA attributes:
        ///     Sets the NIRFSA_ATTR_ACQUISITION_TYPE attribute to NIRFSA_VAL_IQ.
        ///     Sets the NIRFSA_ATTR_NUM_RECORDS_IS_FINITE attribute to VI_TRUE.
        ///     Sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_RECORDS attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.
        ///     Sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_SAMPLING_RATE_HZ attribute to the NIRFSA_ATTR_IQ_RATE attribute.
        ///     If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to NIRFSA_VAL_IQ_POWER_EDGE, this function sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME_SEC attribute to the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_MINIMUM_QUIET_TIME attribute. If the NIRFSA_ATTR_REF_TRIGGER_TYPE attribute is set to any other value, this function sets the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_MINIMUM_QUIET_TIME_SEC attribute to 0.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_ACQUISITION_TIME_SEC attribute, and sets the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.
        ///     Sets the NIRFSA_ATTR_NUM_SAMPLES_IS_FINITE attribute to VI_TRUE.
        ///     Retrieves the coerced NIRFSA_ATTR_IQ_RATE attribute, multiplies this value by the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_PRE_TRIGGER_DELAY_SEC attribute, and sets the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.
        /// 
        ///</returns>
        public int RFSAConfigureHardware(HandleRef rfsaHandle)
        {
            int pInvokeResult = PInvoke.niEDGESA_RFSAConfigureHardware(Handle, rfsaHandle);
            TestForError(pInvokeResult, rfsaHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Sets the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of a float64 niEDGE analysis attribute.
        /// 
        ///</param>
        ///<param>
        /// attributeValue
        /// float64
        /// Specifies the value to which you want to set the attribute.
        /// 
        ///</param>
        ///<returns>
        /// Sets the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        ///</returns>
        public int SetScalarAttributeF64(string channelString, niEDGESAProperties attributeID, double attributeValue)
        {
            int pInvokeResult = PInvoke.niEDGESA_SetScalarAttributeF64(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Sets the value of an niEDGE analysis 32-bit integer (int32) attribute. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of an int32 niEDGE analysis attribute.
        /// 
        ///</param>
        ///<param>
        /// attributeValue
        /// int32
        /// Specifies the value to which you want to set the attribute.
        /// 
        ///</param>
        ///<returns>
        /// Sets the value of an niEDGE analysis 32-bit integer (int32) attribute. 
        /// 
        ///</returns>
        public int SetScalarAttributeI32(string channelString, niEDGESAProperties attributeID, int attributeValue)
        {
            int pInvokeResult = PInvoke.niEDGESA_SetScalarAttributeI32(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Sets the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<param>
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param>
        /// attributeID
        /// niEDGESAProperties
        /// Specifies the ID of a float64 niEDGE analysis attribute.
        /// 
        ///</param>
        ///<param>
        /// dataArray
        /// float64*
        /// Specifies the pointer to the float64 array to which you want to set the attribute.
        /// 
        ///</param>
        ///<param>
        /// dataArraySize
        /// int32
        /// Specifies the number of elements in the float64 array.
        /// 
        ///</param>
        ///<returns>
        /// Sets the value of an niEDGE analysis 64-bit floating point number (float64) attribute. 
        /// 
        ///</returns>
        public int SetVectorAttributeF64(string channelString, niEDGESAProperties attributeID, double[] dataArray, int dataArraySize)
        {
            int pInvokeResult = PInvoke.niEDGESA_SetVectorAttributeF64(Handle, channelString, attributeID, dataArray, dataArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Checks for errors on all configured attributes. If the configuration is invalid, this function returns an error. If there are no errors, the function marks the session as verified.
        /// 
        /// </summary>
        ///<param>
        /// session
        /// HandleRef
        /// Specifies the niEDGE analysis session.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niEDGE analysis function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niEDGESA_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ToolkitCheckError()
        {
            int pInvokeResult = PInvoke.niEDGESA_ToolkitCheckError(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Indicates the version of the toolkit in use.   
        /// 
        /// </summary>
        public int GetAdvancedToolkitCompatibilityVersion(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.AdvancedToolkitCompatibilityVersion, channel, out val);
        }

        /// <summary>
        /// Specifies the absolute RF channel number, as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///    The toolkit uses the NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes to calculate the correct masks for power versus time (PvT) measurements    and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to    NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///    Use the niEDGESA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///    The default value is 1.   
        /// 
        /// </summary>
        public int SetArfcn(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.Arfcn, channel, value);
        }
        /// <summary>
        /// Specifies the absolute RF channel number, as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///    The toolkit uses the NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes to calculate the correct masks for power versus time (PvT) measurements    and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to    NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///    Use the niEDGESA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///    The default value is 1.   
        /// 
        /// </summary>
        public int GetArfcn(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.Arfcn, channel, out val);
        }

        /// <summary>
        /// Specifies the band of operation as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///    The toolkit uses the NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes to calculate the correct masks for power versus time (PvT) measurements    and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to    NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///    Use the niEDGESA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///    The default value is NIEDGESA_VAL_BAND_PGSM.   
        /// 
        /// </summary>
        public int SetBand(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.Band, channel, value);
        }
        /// <summary>
        /// Specifies the band of operation as defined in section 2 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///    The toolkit uses the NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes to calculate the correct masks for power versus time (PvT) measurements    and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to    NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///    Use the niEDGESA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///    The default value is NIEDGESA_VAL_BAND_PGSM.   
        /// 
        /// </summary>
        public int GetBand(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.Band, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable burst synchronization before performing power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_TRUE.   
        /// 
        /// </summary>
        public int SetBurstSynchronizationEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.BurstSynchronizationEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable burst synchronization before performing power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_TRUE.   
        /// 
        /// </summary>
        public int GetBurstSynchronizationEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.BurstSynchronizationEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by error vector magnitude (EVM) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetEvmAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.EvmAllTracesEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable all the traces returned by error vector magnitude (EVM) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetEvmAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.EvmAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether the toolkit compensates for the amplitude droop.
        ///    The default value is NIEDGESA_VAL_FALSE.
        ///    Use the Contents tab in the help to navigate to EDGE Attributes >> EVM >> NIEDGESA_EVM_AMPLITUDE_DROOP_COMPENSATION_ENABLED >> Amplitude Droop    for a graphical representation of the amplitude droop.   
        /// 
        /// </summary>
        public int SetEvmAmplitudeDroopCompensationEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.EvmAmplitudeDroopCompensationEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether the toolkit compensates for the amplitude droop.
        ///    The default value is NIEDGESA_VAL_FALSE.
        ///    Use the Contents tab in the help to navigate to EDGE Attributes >> EVM >> NIEDGESA_EVM_AMPLITUDE_DROOP_COMPENSATION_ENABLED >> Amplitude Droop    for a graphical representation of the amplitude droop.   
        /// 
        /// </summary>
        public int GetEvmAmplitudeDroopCompensationEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.EvmAmplitudeDroopCompensationEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable error vector magnitude (EVM) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetEvmEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.EvmEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable error vector magnitude (EVM) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetEvmEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.EvmEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform error vector magnitude (EVM)    measurements.
        ///    The default value is 10. Valid values are 1 to 10,000 (inclusive).   
        /// 
        /// </summary>
        public int SetEvmNumberOfAverages(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.EvmNumberOfAverages, channel, value);
        }
        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform error vector magnitude (EVM)    measurements.
        ///    The default value is 10. Valid values are 1 to 10,000 (inclusive).   
        /// 
        /// </summary>
        public int GetEvmNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.EvmNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Returns the mean amplitude droop, in dB.
        ///    To perform this measurement, the toolkit computes the amplitude droop of all the bursts and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetEvmResultsAverageAmplitudeDroop(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.EvmResultsAverageAmplitudeDroop, channel, out val);
        }

        /// <summary>
        /// Returns the average frequency error, in hertz (Hz).
        ///    To perform this measurement, the toolkit computes the frequency error of all the bursts and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetEvmResultsAverageFrequencyError(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.EvmResultsAverageFrequencyError, channel, out val);
        }

        /// <summary>
        /// Returns the average magnitude error, multiplied by 100.
        ///    To perform this measurement, the toolkit computes the difference between the magnitude of an ideal constellation    point and the magnitude of the actual sample and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetEvmResultsAverageMagnitudeError(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.EvmResultsAverageMagnitudeError, channel, out val);
        }

        /// <summary>
        /// Returns the average origin offset, in dB.
        ///    To perform this measurement, the toolkit computes the origin offset of all the bursts and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetEvmResultsAverageOriginOffset(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.EvmResultsAverageOriginOffset, channel, out val);
        }

        /// <summary>
        /// Returns the mean phase error, in degrees.
        ///    To perform this measurement, the toolkit computes the phase error of all the bursts and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetEvmResultsAveragePhaseError(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.EvmResultsAveragePhaseError, channel, out val);
        }

        /// <summary>
        /// Returns the TSC detected while performing error vector magnitude (EVM) measurements.  
        /// 
        /// </summary>
        public int GetEvmResultsDetectedTsc(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.EvmResultsDetectedTsc, channel, out val);
        }

        /// <summary>
        /// Specifies the number of occupied timeslots (bursts) in a frame.
        ///    The toolkit uses this attribute to compute the number of records to acquire as shown in the following formula:    number of records = number of averages/number of timeslots, where the number of averages is the maximum of the number    of averages for all enabled measurements. The occupied timeslots must be consecutive timeslots.
        ///    Note: For power versus time (PvT) measurements, set this attribute to 1, which is the only supported value.    The toolkit also uses this attribute to compute the minimum quiet time required to satisfy the triggering condition. 
        ///    The default value is 1. Valid values are 1 to 8, inclusive.   
        /// 
        /// </summary>
        public int SetNumberOfTimeslots(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.NumberOfTimeslots, channel, value);
        }
        /// <summary>
        /// Specifies the number of occupied timeslots (bursts) in a frame.
        ///    The toolkit uses this attribute to compute the number of records to acquire as shown in the following formula:    number of records = number of averages/number of timeslots, where the number of averages is the maximum of the number    of averages for all enabled measurements. The occupied timeslots must be consecutive timeslots.
        ///    Note: For power versus time (PvT) measurements, set this attribute to 1, which is the only supported value.    The toolkit also uses this attribute to compute the minimum quiet time required to satisfy the triggering condition. 
        ///    The default value is 1. Valid values are 1 to 8, inclusive.   
        /// 
        /// </summary>
        public int GetNumberOfTimeslots(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.NumberOfTimeslots, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by output radio frequency spectrum (ORFS)    measurements.
        ///    The default value is NIEDGESA_VAL_TRUE.   
        /// 
        /// </summary>
        public int SetOrfsAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsAllTracesEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable all the traces returned by output radio frequency spectrum (ORFS)    measurements.
        ///    The default value is NIEDGESA_VAL_TRUE.   
        /// 
        /// </summary>
        public int GetOrfsAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable output radio frequency spectrum (ORFS) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetOrfsEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable output radio frequency spectrum (ORFS) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetOrfsEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to perform output radio frequency spectrum (ORFS) measurements on the modulated    part of the waveform, switching part of the waveform, or both.
        ///    The default value is NIEDGESA_VAL_MEASUREMENT_TYPE_MODULATION_SWITCHING.   
        /// 
        /// </summary>
        public int SetOrfsMeasurementType(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsMeasurementType, channel, value);
        }
        /// <summary>
        /// Specifies whether to perform output radio frequency spectrum (ORFS) measurements on the modulated    part of the waveform, switching part of the waveform, or both.
        ///    The default value is NIEDGESA_VAL_MEASUREMENT_TYPE_MODULATION_SWITCHING.   
        /// 
        /// </summary>
        public int GetOrfsMeasurementType(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsMeasurementType, channel, out val);
        }

        /// <summary>
        /// Specifies the averaging type for performing modulation measurements for output radio frequency    spectrum (ORFS).
        ///    The default value is NIEDGESA_VAL_ORFS_LOG_AVERAGING.   
        /// 
        /// </summary>
        public int SetOrfsModulationAveragingMode(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsModulationAveragingMode, channel, value);
        }
        /// <summary>
        /// Specifies the averaging type for performing modulation measurements for output radio frequency    spectrum (ORFS).
        ///    The default value is NIEDGESA_VAL_ORFS_LOG_AVERAGING.   
        /// 
        /// </summary>
        public int GetOrfsModulationAveragingMode(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsModulationAveragingMode, channel, out val);
        }


        public int GetOrfsFastAveragingMode(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsFastAveragingMode, channel, out val);
        }

        public int SetOrfsFastAveragingMode(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsFastAveragingMode, channel, value);
        }

        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS)    modulation measurements are performed.
        ///    This attribute is applicable when the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int SetOrfsModulationOffsetFrequencies(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.OrfsModulationOffsetFrequencies, channel, value);
        }
        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS)    modulation measurements are performed.
        ///    This attribute is applicable when the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int GetOrfsModulationOffsetFrequencies(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.OrfsModulationOffsetFrequencies, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the modulated part of the waveform at the carrier reference.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int SetOrfsModulationRbwCarrier(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.OrfsModulationRbwCarrier, channel, value);
        }
        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the modulated part of the waveform at the carrier reference.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int GetOrfsModulationRbwCarrier(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.OrfsModulationRbwCarrier, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the modulated part of the waveform at offsets that are greater    than 1,800 kHz.
        ///    The default value is 100k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int SetOrfsModulationRbwFarOffset(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.OrfsModulationRbwFarOffset, channel, value);
        }
        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the modulated part of the waveform at offsets that are greater    than 1,800 kHz.
        ///    The default value is 100k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int GetOrfsModulationRbwFarOffset(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.OrfsModulationRbwFarOffset, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the modulated part of the waveform at offsets that are less    than 1,800 kHz.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int SetOrfsModulationRbwNearOffset(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.OrfsModulationRbwNearOffset, channel, value);
        }
        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the modulated part of the waveform at offsets that are less    than 1,800 kHz.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int GetOrfsModulationRbwNearOffset(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.OrfsModulationRbwNearOffset, channel, out val);
        }

        /// <summary>
        /// Specifies whether the toolkit uses the NIEDGESA_ORFS_NOISE_FLOORS attribute to perform noise    floor compensation in the measurement if the NIEDGESA_ORFS_NOISE_COMPENSATION_ENABLED attribute is set to NIEDGESA_VAL_TRUE.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetOrfsNoiseCompensationEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsNoiseCompensationEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether the toolkit uses the NIEDGESA_ORFS_NOISE_FLOORS attribute to perform noise    floor compensation in the measurement if the NIEDGESA_ORFS_NOISE_COMPENSATION_ENABLED attribute is set to NIEDGESA_VAL_TRUE.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetOrfsNoiseCompensationEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsNoiseCompensationEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the noise floor, in dBm, for each frequency offset.    The first m elements in the array correspond to the noise floors at the modulation frequency offsets.    The remaining n elements in the array correspond to the noise floors at the switching frequency offsets.    Ensure that m is equal to the number of modulation offset frequencies that you specify in the NIEDGESA_ORFS_MODULATION_OFFSET_FREQUENCIES attribute    and n is equal to the number of switching offset frequencies that you specify in the NIEDGESA_ORFS_SWITCHING_OFFSET_FREQUENCIES attribute.   
        /// 
        /// </summary>
        public int SetOrfsNoiseFloors(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.OrfsNoiseFloors, channel, value);
        }
        /// <summary>
        /// Specifies the noise floor, in dBm, for each frequency offset.    The first m elements in the array correspond to the noise floors at the modulation frequency offsets.    The remaining n elements in the array correspond to the noise floors at the switching frequency offsets.    Ensure that m is equal to the number of modulation offset frequencies that you specify in the NIEDGESA_ORFS_MODULATION_OFFSET_FREQUENCIES attribute    and n is equal to the number of switching offset frequencies that you specify in the NIEDGESA_ORFS_SWITCHING_OFFSET_FREQUENCIES attribute.   
        /// 
        /// </summary>
        public int GetOrfsNoiseFloors(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.OrfsNoiseFloors, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform output radio frequency spectrum    (ORFS) measurements.
        ///    The default value is 10. Valid values are 1 to 10,000 (inclusive).   
        /// 
        /// </summary>
        public int SetOrfsNumberOfAverages(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsNumberOfAverages, channel, value);
        }
        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform output radio frequency spectrum    (ORFS) measurements.
        ///    The default value is 10. Valid values are 1 to 10,000 (inclusive).   
        /// 
        /// </summary>
        public int GetOrfsNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Specifies the offset frequency mode.
        ///    The default value is NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.   
        /// 
        /// </summary>
        public int SetOrfsOffsetFrequencyMode(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.OrfsOffsetFrequencyMode, channel, value);
        }
        /// <summary>
        /// Specifies the offset frequency mode.
        ///    The default value is NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.   
        /// 
        /// </summary>
        public int GetOrfsOffsetFrequencyMode(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.OrfsOffsetFrequencyMode, channel, out val);
        }

        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS)    switching measurements are performed.
        ///    This attribute is applicable when the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int SetOrfsSwitchingOffsetFrequencies(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.OrfsSwitchingOffsetFrequencies, channel, value);
        }
        /// <summary>
        /// Specifies the offset frequencies, in hertz (Hz), at which output radio frequency spectrum (ORFS)    switching measurements are performed.
        ///    This attribute is applicable when the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int GetOrfsSwitchingOffsetFrequencies(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.OrfsSwitchingOffsetFrequencies, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the switching part of the waveform at the carrier reference.
        ///    The default value is 300k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int SetOrfsSwitchingRbwCarrier(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.OrfsSwitchingRbwCarrier, channel, value);
        }
        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the switching part of the waveform at the carrier reference.
        ///    The default value is 300k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int GetOrfsSwitchingRbwCarrier(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.OrfsSwitchingRbwCarrier, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the switching part of the waveform at offsets that are greater    than 1,800 kHz.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int SetOrfsSwitchingRbwFarOffset(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.OrfsSwitchingRbwFarOffset, channel, value);
        }
        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the switching part of the waveform at offsets that are greater    than 1,800 kHz.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int GetOrfsSwitchingRbwFarOffset(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.OrfsSwitchingRbwFarOffset, channel, out val);
        }

        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the switching part of the waveform at offsets that are less    than 1,800 kHz.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int SetOrfsSwitchingRbwNearOffset(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.OrfsSwitchingRbwNearOffset, channel, value);
        }
        /// <summary>
        /// Specifies the resolution bandwidth (RBW), in hertz (Hz), that is used for measuring the output    radio frequency spectrum (ORFS) in the switching part of the waveform at offsets that are less    than 1,800 kHz.
        ///    The default value is 30k. Valid values are 1k to 5M, inclusive.   
        /// 
        /// </summary>
        public int GetOrfsSwitchingRbwNearOffset(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.OrfsSwitchingRbwNearOffset, channel, out val);
        }

        /// <summary>
        /// Specifies the power control level on the unit under test (UUT), as defined in section 4.1 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///    The toolkit uses this attribute to determine the mask required for power versus time (PvT) measurements.
        ///    The default value is 0. Valid values are 0 to 32, inclusive.   
        /// 
        /// </summary>
        public int SetPowerControlLevel(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PowerControlLevel, channel, value);
        }
        /// <summary>
        /// Specifies the power control level on the unit under test (UUT), as defined in section 4.1 of the 3GPP TS 45.005 Specifications 8.0.0.
        ///    The toolkit uses this attribute to determine the mask required for power versus time (PvT) measurements.
        ///    The default value is 0. Valid values are 0 to 32, inclusive.   
        /// 
        /// </summary>
        public int GetPowerControlLevel(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PowerControlLevel, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable all the traces returned by power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetPvtAllTracesEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PvtAllTracesEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable all the traces returned by power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetPvtAllTracesEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtAllTracesEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the averaging type for power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_PVT_RMS_AVERAGING.   
        /// 
        /// </summary>
        public int SetPvtAveragingMode(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PvtAveragingMode, channel, value);
        }
        /// <summary>
        /// Specifies the averaging type for power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_PVT_RMS_AVERAGING.   
        /// 
        /// </summary>
        public int GetPvtAveragingMode(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtAveragingMode, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetPvtEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PvtEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetPvtEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies whether to use a standard-specified mask or a user-defined mask for power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_MASK_TYPE_STANDARD.   
        /// 
        /// </summary>
        public int SetPvtMaskType(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PvtMaskType, channel, value);
        }
        /// <summary>
        /// Specifies whether to use a standard-specified mask or a user-defined mask for power versus time (PvT) measurements.
        ///    The default value is NIEDGESA_VAL_MASK_TYPE_STANDARD.   
        /// 
        /// </summary>
        public int GetPvtMaskType(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtMaskType, channel, out val);
        }

        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform power versus time (PvT)    measurements.
        ///    The default value is 10. Valid values are 1 to 10,000 (inclusive).   
        /// 
        /// </summary>
        public int SetPvtNumberOfAverages(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PvtNumberOfAverages, channel, value);
        }
        /// <summary>
        /// Specifies the number of bursts that the toolkit averages to perform power versus time (PvT)    measurements.
        ///    The default value is 10. Valid values are 1 to 10,000 (inclusive).   
        /// 
        /// </summary>
        public int GetPvtNumberOfAverages(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtNumberOfAverages, channel, out val);
        }

        /// <summary>
        /// Specifies the 3 dB double-sided bandwidth, in hertz (Hz), of the specified RBW filter.
        ///    The default value is 500k. Valid values are 10 to 20M, inclusive.   
        /// 
        /// </summary>
        public int SetPvtRbwFilterBandwidth(string channel, double value)
        {
            return SetDouble(niEDGESAProperties.PvtRbwFilterBandwidth, channel, value);
        }
        /// <summary>
        /// Specifies the 3 dB double-sided bandwidth, in hertz (Hz), of the specified RBW filter.
        ///    The default value is 500k. Valid values are 10 to 20M, inclusive.   
        /// 
        /// </summary>
        public int GetPvtRbwFilterBandwidth(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.PvtRbwFilterBandwidth, channel, out val);
        }

        /// <summary>
        /// Specifies the type of front end RBW filter used for power versus time (PvT) measurements.
        ///    Note: This is the first filter that is applied on the acquired waveform in the niEDGESA_AnalyzeIQComplexF64 function.
        ///    The default value is NIEDGESA_VAL_FILTER_TYPE_GAUSSIAN.   
        /// 
        /// </summary>
        public int SetPvtRbwFilterType(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.PvtRbwFilterType, channel, value);
        }
        /// <summary>
        /// Specifies the type of front end RBW filter used for power versus time (PvT) measurements.
        ///    Note: This is the first filter that is applied on the acquired waveform in the niEDGESA_AnalyzeIQComplexF64 function.
        ///    The default value is NIEDGESA_VAL_FILTER_TYPE_GAUSSIAN.   
        /// 
        /// </summary>
        public int GetPvtRbwFilterType(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtRbwFilterType, channel, out val);
        }

        /// <summary>
        /// Returns the average power, in dBm, in the useful portion of the burst.
        ///    Refer to the Useful Portion of a Burst topic for more information.   
        /// 
        /// </summary>
        public int GetPvtResultsAveragePower(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.PvtResultsAveragePower, channel, out val);
        }

        /// <summary>
        /// Returns the maximum power, in dBm. The toolkit calculates the average over a specified number of averages and returns the maximum of these averaged values.   
        /// 
        /// </summary>
        public int GetPvtResultsMaximumPower(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.PvtResultsMaximumPower, channel, out val);
        }

        /// <summary>
        /// Indicates whether the power versus time (PvT) measurement has passed or failed the mask    specification.   
        /// 
        /// </summary>
        public int GetPvtResultsMeasurementStatus(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.PvtResultsMeasurementStatus, channel, out val);
        }

        /// <summary>
        /// Returns the minimum power, in dBm. The toolkit calculates the average over a specified number of averages and returns the minimum of these averaged values.   
        /// 
        /// </summary>
        public int GetPvtResultsMinimumPower(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.PvtResultsMinimumPower, channel, out val);
        }

        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the lower mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.   
        /// 
        /// </summary>
        public int SetPvtUserDefinedLowerMaskRelativePower(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.PvtUserDefinedLowerMaskRelativePower, channel, value);
        }
        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the lower mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.   
        /// 
        /// </summary>
        public int GetPvtUserDefinedLowerMaskRelativePower(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.PvtUserDefinedLowerMaskRelativePower, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the lower mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int SetPvtUserDefinedLowerMaskTime(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.PvtUserDefinedLowerMaskTime, channel, value);
        }
        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the lower mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int GetPvtUserDefinedLowerMaskTime(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.PvtUserDefinedLowerMaskTime, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the upper mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.   
        /// 
        /// </summary>
        public int SetPvtUserDefinedUpperMaskRelativePower(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.PvtUserDefinedUpperMaskRelativePower, channel, value);
        }
        /// <summary>
        /// Specifies the relative power values (y-axis), in dB, of the upper mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.    Power values are relative to the average power of the acquired signal.   
        /// 
        /// </summary>
        public int GetPvtUserDefinedUpperMaskRelativePower(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.PvtUserDefinedUpperMaskRelativePower, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the upper mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int SetPvtUserDefinedUpperMaskTime(string channel, double[] value)
        {
            return SetDoubleArray(niEDGESAProperties.PvtUserDefinedUpperMaskTime, channel, value);
        }
        /// <summary>
        /// Specifies the time values (x-axis), in seconds, of the upper mask for a user-defined mask when the    NIEDGESA_PVT_MASK_TYPE attribute is set to NIEDGESA_VAL_MASK_TYPE_USER_DEFINED.   
        /// 
        /// </summary>
        public int GetPvtUserDefinedUpperMaskTime(string channel, double[] dataArray, out int actualNumberOfPoints)
        {
            return GetDoubleArray(niEDGESAProperties.PvtUserDefinedUpperMaskTime, channel, dataArray, out actualNumberOfPoints);
        }

        /// <summary>
        /// Returns the length of the record to acquire, in seconds.
        ///    This attribute includes delays due to the measurement filter and may be greater than the ideal EDGE burst length.    If you do not use the niEDGESA_RFSAConfigureHardware function, multiply this value by the NIRFSA_ATTR_IQ_RATE attribute and set    the result to the NIRFSA_ATTR_NUM_SAMPLES attribute.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsAcquisitionTime(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.RecommendedHardwareSettingsAcquisitionTime, channel, out val);
        }

        /// <summary>
        /// Returns the minimum time, in seconds, during which the signal level must be below the trigger value    for triggering to occur.
        ///    If you do not use the niEDGESA_RFSAConfigureHardware function, pass  this attribute to the NIRFSA_ATTR_REF_TRIGGER_MINIMUM_QUIET_TIME attribute.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsMinimumQuietTime(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.RecommendedHardwareSettingsMinimumQuietTime, channel, out val);
        }

        /// <summary>
        /// Returns the number of records to acquire.
        ///    The toolkit calculates this attribute using the following formula: number of records = number of averages/number of timeslots,    where the number of averages is the maximum of the number of averages for all enabled measurements.
        ///    If you do not use the niEDGESA_RFSAConfigureHardware function, pass the NIEDGESA_RECOMMENDED_HARDWARE_SETTINGS_NUMBER_OF_RECORDS    attribute to the NIRFSA_ATTR_NUM_RECORDS attribute.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsNumberOfRecords(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.RecommendedHardwareSettingsNumberOfRecords, channel, out val);
        }

        /// <summary>
        /// Returns the post-trigger delay, in seconds.
        ///    Add this value to the absolute timestamp element of the wfmInfo parameter in the niRFSA_ReadIQSingleRecordComplexF64 function.    Use this attribute when the actual signal to be measured is not generated immediately when the trigger occurs but is generated after a delay.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsPostTriggerDelay(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.RecommendedHardwareSettingsPostTriggerDelay, channel, out val);
        }

        /// <summary>
        /// Returns the pre-trigger delay, in seconds.
        ///    This attribute is used to acquire data prior to the trigger to account for the delays in the measurement process.    If you do not use the niEDGESA_RFSAConfigureHardware function, multiply this value by the NIRFSA_ATTR_IQ_RATE attribute and set    the result to the NIRFSA_ATTR_REF_TRIGGER_PRETRIGGER_SAMPLES attribute.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsPreTriggerDelay(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.RecommendedHardwareSettingsPreTriggerDelay, channel, out val);
        }

        /// <summary>
        /// Returns the recommended sampling rate, in hertz (Hz), for the RF signal analyzer.
        ///    If you do not use the niEDGESA_RFSAConfigureHardware function, pass this attribute to the niRFSA_ConfigureIQRate function.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsSamplingRate(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.RecommendedHardwareSettingsSamplingRate, channel, out val);
        }

        /// <summary>
        /// Returns the error vector magnitude (EVM) value, multiplied by 100, at which no more than 5 percent of the    symbols have an EVM exceeding this value.   
        /// 
        /// </summary>
        public int GetResults95thPercentileEvm(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.Results95thPercentileEvm, channel, out val);
        }

        /// <summary>
        /// Returns the average root mean square (RMS) error vector magnitude (EVM), multiplied by 100.
        ///    To perform this measurement, the toolkit computes the RMS EVM of all the bursts and returns the mean of these values.   
        /// 
        /// </summary>
        public int GetResultsAverageRmsEvm(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.ResultsAverageRmsEvm, channel, out val);
        }

        /// <summary>
        /// Returns the maximum peak error vector magnitude (EVM), multiplied by 100.
        ///    To perform this measurement, the toolkit computes the peak EVM of all the bursts and returns the maximum of these values.   
        /// 
        /// </summary>
        public int GetResultsMaximumPeakEvm(string channel, out double val)
        {
            return GetDouble(niEDGESAProperties.ResultsMaximumPeakEvm, channel, out val);
        }

        /// <summary>
        /// Specifies the training sequence in the burst, as defined in section 5.2.3 of the 3GPP TS 45.002 Specifications 8.0.0.
        ///    If you set the NIEDGESA_BURST_SYNCHRONIZATION_ENABLED attribute to NIEDGESA_VAL_TRUE and    the NIEDGESA_TSC_AUTO_DETECTION_ENABLED attribute to NIEDGESA_VAL_FALSE,    the toolkit uses the training sequence that you specify in the NIEDGESA_TSC attribute to synchronize to the start of the burst.
        ///    The default value is NIEDGESA_VAL_TSC0.   
        /// 
        /// </summary>
        public int SetTsc(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.Tsc, channel, value);
        }
        /// <summary>
        /// Specifies the training sequence in the burst, as defined in section 5.2.3 of the 3GPP TS 45.002 Specifications 8.0.0.
        ///    If you set the NIEDGESA_BURST_SYNCHRONIZATION_ENABLED attribute to NIEDGESA_VAL_TRUE and    the NIEDGESA_TSC_AUTO_DETECTION_ENABLED attribute to NIEDGESA_VAL_FALSE,    the toolkit uses the training sequence that you specify in the NIEDGESA_TSC attribute to synchronize to the start of the burst.
        ///    The default value is NIEDGESA_VAL_TSC0.   
        /// 
        /// </summary>
        public int GetTsc(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.Tsc, channel, out val);
        }

        /// <summary>
        /// Specifies whether to enable the auto detection of TSC.
        ///    The default value is NIEDGESA_VAL_TRUE.   
        /// 
        /// </summary>
        public int SetTscAutoDetectionEnabled(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.TscAutoDetectionEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable the auto detection of TSC.
        ///    The default value is NIEDGESA_VAL_TRUE.   
        /// 
        /// </summary>
        public int GetTscAutoDetectionEnabled(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.TscAutoDetectionEnabled, channel, out val);
        }

        /// <summary>
        /// Specifies the type of unit under test (UUT) generating the signal that the toolkit analyzes.
        ///    The toolkit uses the NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes to calculate the correct masks for power versus time (PvT) measurements    and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to    NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///    Use the niEDGESA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///    The default value is NIEDGESA_VAL_UUT_MS.   
        /// 
        /// </summary>
        public int SetUut(string channel, int value)
        {
            return SetInt32(niEDGESAProperties.Uut, channel, value);
        }
        /// <summary>
        /// Specifies the type of unit under test (UUT) generating the signal that the toolkit analyzes.
        ///    The toolkit uses the NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes to calculate the correct masks for power versus time (PvT) measurements    and the offset frequencies for output radio frequency spectrum (ORFS) measurements if the NIEDGESA_ORFS_OFFSET_FREQUENCY_MODE attribute is set to    NIEDGESA_VAL_OFFSET_FREQUENCY_MODE_STANDARD.
        ///    Use the niEDGESA_ARFCNToCarrierFrequency function to calculate the carrier frequency corresponding to the    NIEDGESA_UUT, NIEDGESA_BAND, and NIEDGESA_ARFCN attributes. Use the niRFSA_ConfigureIQCarrierFrequency function    to set the carrier frequency on the RF signal analyzer.
        ///    The default value is NIEDGESA_VAL_UUT_MS.   
        /// 
        /// </summary>
        public int GetUut(string channel, out int val)
        {
            return GetInt32(niEDGESAProperties.Uut, channel, out val);
        }

        private int SetInt32(niEDGESAProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            return TestForError(PInvoke.niEDGESA_SetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetInt32(niEDGESAProperties propertyId, string repeatedCapabilityOrChannel, out int val)
        {
            return TestForError(PInvoke.niEDGESA_GetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetDouble(niEDGESAProperties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            return TestForError(PInvoke.niEDGESA_SetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetDouble(niEDGESAProperties propertyId, string repeatedCapabilityOrChannel, out double val)
        {
            return TestForError(PInvoke.niEDGESA_GetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetDoubleArray(niEDGESAProperties propertyId, string repeatedCapabilityOrChannel, double[] dataArray)
        {
            return TestForError(PInvoke.niEDGESA_SetVectorAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, dataArray, dataArray.Length));
        }

        private int GetDoubleArray(niEDGESAProperties propertyId, string repeatedCapabilityOrChannel, double[] dataArray, out int actualNumberOfPoints)
        {
            return TestForError(PInvoke.niEDGESA_GetVectorAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, dataArray, dataArray.Length, out actualNumberOfPoints));
        }


        private class PInvoke
        {
            const string nativeDllName = "niEDGESA_net.dll";
            const string nativeAttributeDllName = "niEDGEAnalysisattribs.dll";

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_AnalyzeIQComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_AnalyzeIQComplexF64(HandleRef session, double t0, double dt, niComplexNumber[] waveform, int length, int reset, out int done);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ARFCNToCarrierFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ARFCNToCarrierFrequency(int uUT, int band, int aRFCN, out double carrierFrequency);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_CloseSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_EVMGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_EVMGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_EVMGetCurrentIterationDemodulatedBitsTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_EVMGetCurrentIterationDemodulatedBitsTrace(HandleRef session, [In, Out] int[] demodulatedBits, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_EVMGetCurrentIterationEVMTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_EVMGetCurrentIterationEVMTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] eVM, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_EVMGetCurrentIterationMagnitudeErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_EVMGetCurrentIterationMagnitudeErrorTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] magnitudeError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_EVMGetCurrentIterationPhaseErrorTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_EVMGetCurrentIterationPhaseErrorTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] phaseError, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_GetErrorString(HandleRef session, int errorCode, StringBuilder errorMessage, int errorMessageLength);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_GetScalarAttributeF64(HandleRef session, string channelString, niEDGESAProperties attributeID, out double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_GetScalarAttributeI32(HandleRef session, string channelString, niEDGESAProperties attributeID, out int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_GetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_GetVectorAttributeF64(HandleRef session, string channelString, niEDGESAProperties attributeID, [In, Out] double[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_OpenSession(string sessionName, int toolkitCompatibilityVersion, out Int16 isNewSession, out IntPtr session);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetModulationAbsolutePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetModulationAbsolutePowersTrace(HandleRef session, [In, Out] double[] modulationAbsolutePowers, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetModulationOffsetFrequenciesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetModulationOffsetFrequenciesTrace(HandleRef session, [In, Out] double[] modulationOffsetFrequencies, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetModulationRelativePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetModulationRelativePowersTrace(HandleRef session, [In, Out] double[] modulationRelativePowers, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetSwitchingAbsolutePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetSwitchingAbsolutePowersTrace(HandleRef session, [In, Out] double[] switchingAbsolutePowers, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetSwitchingOffsetFrequenciesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetSwitchingOffsetFrequenciesTrace(HandleRef session, [In, Out] double[] switchingOffsetFrequencies, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ORFSGetSwitchingRelativePowersTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ORFSGetSwitchingRelativePowersTrace(HandleRef session, [In, Out] double[] switchingRelativePowers, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_PvTGetAverageSignalPowerTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_PvTGetAverageSignalPowerTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] averageSignalPower, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_PvTGetCurrentIterationAcquiredIQWaveformTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_PvTGetCurrentIterationAcquiredIQWaveformTrace(HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] acquiredIQWaveform, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_PvTGetLowerMaskTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_PvTGetLowerMaskTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] lowerMask, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_PvTGetUpperMaskTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_PvTGetUpperMaskTrace(HandleRef session, out double t0, out double dt, [In, Out] double[] upperMask, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeAttributeDllName, EntryPoint = "niAttributeReset", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ResetAttribute(HandleRef session, string channelString, niEDGESAProperties attributeID);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ResetSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_RFSAAutoLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_RFSAAutoLevel(System.Runtime.InteropServices.HandleRef rfsaHandle, double bandwidth, double measurementInterval, int maxNumberofIterations, out double resultantReferenceLevel);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_RFSAConfigureHardware", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_RFSAConfigureHardware(HandleRef session, System.Runtime.InteropServices.HandleRef rfsaHandle);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_SetScalarAttributeF64(HandleRef session, string channelString, niEDGESAProperties attributeID, double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_SetScalarAttributeI32(HandleRef session, string channelString, niEDGESAProperties attributeID, int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_SetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_SetVectorAttributeF64(HandleRef session, string channelString, niEDGESAProperties attributeID, double[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_ToolkitCheckError", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_ToolkitCheckError(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niEDGESA_RFSAMeasure_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niEDGESA_RFSAMeasure_vi(HandleRef session, HandleRef rfsaHandle, double timeout);
        }

        private int GetErrorString(int status, StringBuilder msg)
        {
            int size = PInvoke.niEDGESA_GetErrorString(Handle, status, null, 0);
            if ((size >= 0))
            {
                msg.Capacity = size;
                PInvoke.niEDGESA_GetErrorString(Handle, status, msg, size);
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
                    PInvoke.niEDGESA_CloseSession(Handle);
                }
            }
        }

        #endregion
    }

    public class niEDGESAConstants
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

        public const int PvtPeakHoldAveraging = 3;

        public const int PvtMinimumHoldAveraging = 4;

        public const int MeasurementStatusFail = 0;

        public const int MeasurementStatusPass = 1;

        public const int OrfsMeasurementTypeModulationSwitching = 0;

        public const int OrfsMeasurementTypeModulation = 1;

        public const int OrfsMeasurementTypeSwitching = 2;

        public const int OffsetFrequencyModeStandard = 0;

        public const int OffsetFrequencyModeUserDefined = 1;

        public const int OrfsRmsAveraging = 0;

        public const int OrfsLogAveraging = 1;

        public const int ToolkitCompatibilityVersion100 = 100;

    }

    public enum niEDGESAProperties
    {

        /// <summary>
        /// int
        /// </summary>
        AdvancedToolkitCompatibilityVersion = 65533,

        /// <summary>
        /// int
        /// </summary>
        Arfcn = 114,

        /// <summary>
        /// int
        /// </summary>
        Band = 113,

        /// <summary>
        /// int
        /// </summary>
        BurstSynchronizationEnabled = 2,

        /// <summary>
        /// int
        /// </summary>
        EvmAllTracesEnabled = 150,

        /// <summary>
        /// int
        /// </summary>
        EvmAmplitudeDroopCompensationEnabled = 6,

        /// <summary>
        /// int
        /// </summary>
        EvmEnabled = 4,

        /// <summary>
        /// int
        /// </summary>
        EvmNumberOfAverages = 7,

        /// <summary>
        /// double
        /// </summary>
        EvmResultsAverageAmplitudeDroop = 24,

        /// <summary>
        /// double
        /// </summary>
        EvmResultsAverageFrequencyError = 20,

        /// <summary>
        /// double
        /// </summary>
        EvmResultsAverageMagnitudeError = 16,

        /// <summary>
        /// double
        /// </summary>
        EvmResultsAverageOriginOffset = 22,

        /// <summary>
        /// double
        /// </summary>
        EvmResultsAveragePhaseError = 18,

        /// <summary>
        /// int
        /// </summary>
        EvmResultsDetectedTsc = 27,

        /// <summary>
        /// int
        /// </summary>
        NumberOfTimeslots = 1,

        /// <summary>
        /// int
        /// </summary>
        OrfsAllTracesEnabled = 151,

        /// <summary>
        /// int
        /// </summary>
        OrfsEnabled = 33,

        /// <summary>
        /// int
        /// </summary>
        OrfsMeasurementType = 34,


        /// <summary>
        /// int
        /// </summary>
        OrfsFastAveragingMode = 0x32,

        /// <summary>
        /// int
        /// </summary>
        OrfsModulationAveragingMode = 51,

        /// <summary>
        /// double[]
        /// </summary>
        OrfsModulationOffsetFrequencies = 52,

        /// <summary>
        /// double
        /// </summary>
        OrfsModulationRbwCarrier = 41,

        /// <summary>
        /// double
        /// </summary>
        OrfsModulationRbwFarOffset = 43,

        /// <summary>
        /// double
        /// </summary>
        OrfsModulationRbwNearOffset = 42,

        /// <summary>
        /// int
        /// </summary>
        OrfsNoiseCompensationEnabled = 40,

        /// <summary>
        /// double[]
        /// </summary>
        OrfsNoiseFloors = 39,

        /// <summary>
        /// int
        /// </summary>
        OrfsNumberOfAverages = 48,

        /// <summary>
        /// int
        /// </summary>
        OrfsOffsetFrequencyMode = 37,

        /// <summary>
        /// double[]
        /// </summary>
        OrfsSwitchingOffsetFrequencies = 55,

        /// <summary>
        /// double
        /// </summary>
        OrfsSwitchingRbwCarrier = 44,

        /// <summary>
        /// double
        /// </summary>
        OrfsSwitchingRbwFarOffset = 46,

        /// <summary>
        /// double
        /// </summary>
        OrfsSwitchingRbwNearOffset = 45,

        /// <summary>
        /// int
        /// </summary>
        PowerControlLevel = 163,

        /// <summary>
        /// int
        /// </summary>
        PvtAllTracesEnabled = 152,

        /// <summary>
        /// int
        /// </summary>
        PvtAveragingMode = 82,

        /// <summary>
        /// int
        /// </summary>
        PvtEnabled = 67,

        /// <summary>
        /// int
        /// </summary>
        PvtMaskType = 70,

        /// <summary>
        /// int
        /// </summary>
        PvtNumberOfAverages = 79,

        /// <summary>
        /// double
        /// </summary>
        PvtRbwFilterBandwidth = 69,

        /// <summary>
        /// int
        /// </summary>
        PvtRbwFilterType = 68,

        /// <summary>
        /// double
        /// </summary>
        PvtResultsAveragePower = 84,

        /// <summary>
        /// double
        /// </summary>
        PvtResultsMaximumPower = 90,

        /// <summary>
        /// int
        /// </summary>
        PvtResultsMeasurementStatus = 93,

        /// <summary>
        /// double
        /// </summary>
        PvtResultsMinimumPower = 91,

        /// <summary>
        /// double[]
        /// </summary>
        PvtUserDefinedLowerMaskRelativePower = 74,

        /// <summary>
        /// double[]
        /// </summary>
        PvtUserDefinedLowerMaskTime = 73,

        /// <summary>
        /// double[]
        /// </summary>
        PvtUserDefinedUpperMaskRelativePower = 77,

        /// <summary>
        /// double[]
        /// </summary>
        PvtUserDefinedUpperMaskTime = 76,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsAcquisitionTime = 105,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsMinimumQuietTime = 108,

        /// <summary>
        /// int
        /// </summary>
        RecommendedHardwareSettingsNumberOfRecords = 103,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsPostTriggerDelay = 107,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsPreTriggerDelay = 106,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsSamplingRate = 104,

        /// <summary>
        /// double
        /// </summary>
        Results95thPercentileEvm = 14,

        /// <summary>
        /// double
        /// </summary>
        ResultsAverageRmsEvm = 10,

        /// <summary>
        /// double
        /// </summary>
        ResultsMaximumPeakEvm = 13,

        /// <summary>
        /// int
        /// </summary>
        Tsc = 3,

        /// <summary>
        /// int
        /// </summary>
        TscAutoDetectionEnabled = 5,

        /// <summary>
        /// int
        /// </summary>
        Uut = 0,

    }
}


    