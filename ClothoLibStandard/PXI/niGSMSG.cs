

//==================================================================================================      : 
//===================================================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;
using NationalInstruments.RFToolkits.Interop;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.RFToolkits.Interop
{
    public class niGSMSG : object, IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _isNamedSession;

        ~niGSMSG()
        {
            Dispose(false);
        }

        /// <summary>
        /// Looks up an existing niGSM Generation session and returns the refnum that you can pass to subsequent niGSM generation functions. If the lookup fails, it creates a new niEDGE analysis session and returns a new refnum.
        /// 
        /// </summary>
        /// <param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niGSMSG_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        public niGSMSG(int toolkitCompatibilityVersion)
        {
            System.IntPtr handle;
            int isNewSession;
            int pInvokeResult = PInvoke.niGSMSG_OpenSession(String.Empty, toolkitCompatibilityVersion, out isNewSession, out handle);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            _isNamedSession = false;
        }
        /// <summary>
        /// Looks up an existing niGSM generation session and returns the refnum that you can pass to subsequent niGSM generation functions. If the lookup fails, the niGSMSG_OpenSession function creates a new niGSM generation session and returns a new refnum.
        /// Make sure you call Close() to close a named session. Calling Dispose() does not close named session.
        /// </summary>
        ///<param>
        /// sessionName
        /// char[]
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. To get the reference to an already-opened session x, specify x as the session name. 
        ///  You can obtain the reference to an existing session multiple times if you have not called the niGSMSG_CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string or NULL to the sessionName parameter.
        /// Tip&nbsp;&nbsp;National Instruments recommends that you call the niGSMSG_CloseSession function for each uniquely-named instance of the niGSMSG_OpenSession function or each instance of the niGSMSG_OpenSession function with an unnamed session.
        /// 
        ///</param>
        ///<param>
        /// toolkitCompatibilityVersion
        /// int32
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    niGSMSG_VAL_TOOLKIT_COMPATIBILITY_VERSION_100 (100)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        ///<param>
        /// isNewSession
        /// int32*
        /// Returns NIGSMSG_VAL_ENABLED_TRUE if the function creates a new session. This parameter returns NIGSMSG_VAL_ENABLED_FALSE if the function returns a reference to an existing session.
        /// 
        ///</param>
        ///<param>
        /// session
        /// HandleRef*
        /// Boolean value indicating if the niGSM Generation session is a new session.
        /// 
        ///</param>
        public niGSMSG(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            System.IntPtr handle;
            int pInvokeResult = PInvoke.niGSMSG_OpenSession(sessionName, toolkitCompatibilityVersion, out isNewSession, out handle);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            if (String.IsNullOrEmpty(sessionName))
                _isNamedSession = false;
            else
                _isNamedSession = true;
        }


        public HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        /// <summary>
        /// Computes the carrier frequency using the values that you specify in the UUT, band, and ARFCN parameters, as described in section 2 of the 3GPP TS 45.005 Specifications.
        /// 
        /// </summary>
        /// <param name = "uUT">
        ///    UUT
        ///   int32
        ///   Specifies the type of UUT. The default value is NIGSMSG_VAL_UUT_MS. 
        ///    NIGSMSG_VAL_UUT_BTS
        ///   Specifies that the signal is generated by a base station (BTS).
        ///       Note
        ///       The current version of the toolkit supports only normal BTS. 
        ///    NIGSMSG_VAL_UUT_MS
        ///   Specifies that the signal is generated by a mobile station (MS). This value is the default.
        /// 
        ///</param>
        /// <param name = "band">
        /// band
        /// int32
        /// Specifies the band of operation. The default value is NIGSMSG_VAL_BAND_PGSM.
        /// NIGSMSG_VAL_BAND_PGSM
        /// Specifies a primary GSM (PGSM) band in the 900 MHz band. This value is the default.
        /// NIGSMSG_VAL_BAND_EGSM
        /// Specifies an extended GSM (EGSM) band in the 900 MHz band.
        /// NIGSMSG_VAL_BAND_RGSM
        /// Specifies a railway GSM (RGSM) band in the 900 MHz band.
        /// NIGSMSG_VAL_BAND_DCS1800
        /// Specifies a digital cellular system 1800 (DCS 1800) band. This band is also known as GSM 1800.
        /// NIGSMSG_VAL_BAND_PCS1900
        /// Specifies a personal communications service 1900 (PCS 1900) band. This band is also known as GSM 1900.
        /// NIGSMSG_VAL_BAND_GSM450
        /// Specifies a GSM 450 band.
        /// NIGSMSG_VAL_BAND_GSM480
        /// Specifies a GSM 480 band.
        /// NIGSMSG_VAL_BAND_GSM850
        /// Specifies a GSM 850 band.
        /// NIGSMSG_VAL_BAND_GSM750
        /// Specifies a GSM 750 band.
        /// NIGSMSG_VAL_BAND_TGSM810
        /// Specifies a terrestrial GSM 810 (T GSM 810) band.
        /// 
        ///</param>
        /// <param name = "aRFCN">
        ///   ARFCN
        ///   int32
        ///   Specifies the absolute RF channel number. The default value is 1.
        /// 
        ///</param>
        /// <param name = "carrierFrequency">
        /// carrierFrequency
        /// float64*
        /// Returns the carrier frequency, in hertz (Hz).
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int ARFCNtoCarrierFrequency(int uUT, int band, int aRFCN, out double carrierFrequency)
        {
            int pInvokeResult = PInvoke.niGSMSG_ARFCNtoCarrierFrequency(uUT, band, aRFCN, out carrierFrequency);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Clears the attributes stored in the RFSG waveform database and clears the waveforms from the RF signal generator memory.
        /// This function clears the waveforms and the attributes of the waveforms that you specify in the waveform parameter. If you set the waveform parameter as empty, this function clears all the waveforms and their attributes.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "waveform">
        ///             waveform
        ///         char[]
        ///         Specifies the names of the waveforms to clear. If you set this parameter as empty, the function clears all the waveforms and their attributes. 
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGClearDatabase(HandleRef rfsgHandle, string waveform)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGClearDatabase(rfsgHandle, waveform);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// 
        /// Closes the niGSM generation session and releases resources associated with that session. Call this function once for each unique named session that you have created.
        /// 
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public void Close()
        {
            if (!_isNamedSession)
                Dispose();
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                    PInvoke.niGSMSG_CloseSession(Handle);
            }
        }
        /// <summary>
        /// Creates a GSM burst according to user-specified parameters.
        /// 
        /// </summary>
        ///
        /// <param name = "t0">
        /// t0
        /// float64*
        /// Indicates the start parameter.
        /// 
        ///</param>
        /// <param name = "dt">
        ///             dt
        ///         float64*
        ///         Indicates the delta parameter. 
        /// 
        ///</param>
        /// <param name = "waveform">
        ///             waveform
        ///         NIComplexNumber[]
        ///         Returns the complex signal values stored in an array.
        /// 
        ///</param>
        /// <param name = "length">
        ///             len
        ///         int32
        ///         Specifies the length of the waveform array.
        /// 
        ///</param>
        /// <param name = "waveformSize">
        ///             waveformSize
        ///         int32*
        ///         Returns the waveform size. If the waveform array is not large enough to hold all the samples, the function returns an error code and the waveformSize parameter returns the minimum expected size of the output
        /// array.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int CreateBurst(out double t0, out double dt, niComplexNumber[] waveform, int length, out int waveformSize)
        {
            int pInvokeResult = PInvoke.niGSMSG_CreateBurst(Handle, out t0, out dt, waveform, length, out waveformSize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Takes the error code returned by niGSM generation functions and returns the interpretation as a user-readable string. 
        /// 
        /// </summary>
        /// <param name = "errorCode">
        ///             errorCode
        ///         int32
        ///         Specifies the error code that is returned from any of the niGSM generation functions.
        /// 
        ///</param>
        /// <param name = "errorMessage">
        ///             errorMessage
        ///         char[]
        ///         Returns the user-readable message string that corresponds to the error code you specify. The errorMessage buffer must have at least as many elements as are indicated in the errorMessageLength parameter. 
        /// 
        ///</param>
        /// <param name = "errorMessageLength">
        ///             errorMessageLength
        ///         int32
        ///         Specifies the length of the errorMessage buffer.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int GetErrorString(int errorCode, StringBuilder errorMessage, int errorMessageLength)
        {
            int pInvokeResult = PInvoke.niGSMSG_GetErrorString(Handle, errorCode, errorMessage, errorMessageLength);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Queries the value of an niGSM generation 64-bit floating point number (float64) attribute. 
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// 
        ///</param>
        /// <param name = "attributeID">
        ///             attribute
        ///         niGSMSG_Attr
        ///         Specifies the ID of a float64 niGSM generation attribute.
        /// 
        ///</param>
        /// <param name = "attributeValue">
        ///             value
        ///         float64*
        ///         Returns the value of the attribute that you specify using the attribute parameter.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int GetScalarAttributeF64(string channelString, niGSMSGProperties attributeID, out double attributeValue)
        {
            int pInvokeResult = PInvoke.niGSMSG_GetScalarAttributeF64(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Queries the value of an niGSM generation 32-bit integer (int32) attribute. 
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// 
        ///</param>
        /// <param name = "attributeID">
        ///             attribute
        ///         niGSMSG_Attr
        ///         Specifies the ID of an int32 niGSM generation attribute.
        /// 
        ///</param>
        /// <param name = "attributeValue">
        ///             value
        ///         int32*
        ///         Returns the value of the attribute that you specify using the attribute parameter.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int GetScalarAttributeI32(string channelString, niGSMSGProperties attributeID, out int attributeValue)
        {
            int pInvokeResult = PInvoke.niGSMSG_GetScalarAttributeI32(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Queries the value of an niGSM generation 32-bit integer number (int32) attribute. 
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// 
        ///</param>
        /// <param name = "attributeID">
        ///             attribute
        ///         niGSMSG_Attr
        ///         Specifies the ID of a int32 niGSM generation attribute.
        /// 
        ///</param>
        /// <param name = "dataArray">
        ///             dataArray
        ///         int32[]
        ///         Specifies the pointer to the int32 array to which you want to set the attribute.
        /// 
        ///</param>
        /// <param name = "dataArraySize">
        ///             dataArraySize
        ///         int32
        ///         Specifies the number of elements in the int32 array.
        /// 
        ///</param>
        /// <param name = "actualNumDataArrayElements">
        ///             actualNumDataArrayElements
        ///         int32*
        ///         If the dataArray array is not large enough to hold all the samples, the function returns an error code and the actualNumDataArrayElements returns the minimum expected size of the output array.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int GetVectorAttributeI32(string channelString, niGSMSGProperties attributeID, int[] dataArray, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niGSMSG_GetVectorAttributeI32(Handle, channelString, attributeID, dataArray, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// niGSMSG_ResetAttribute
        /// int32 __stdcall niGSMSG_ResetAttribute(niGSMSGSession session,
        /// char channelString[], 
        /// Resets the attribute specified in the attributeID parameter to its default value. You can reset only a writable attribute using this function.
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// 
        ///</param>
        /// <param name = "attributeID">
        /// attributeID
        /// niGSMSG_Attr
        /// Specifies the ID of the niGSM generation attribute that you want to reset.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int ResetAttribute(string channelString, niGSMSGProperties attributeID)
        {
            int pInvokeResult = PInvoke.niGSMSG_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// 
        /// Resets all the attributes of the session to their default values.
        /// 
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niGSMSG_ResetSession(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the headroom, in dB, stored in the RFSG waveform database. The function uses the waveform name as the key.
        /// Note
        /// Use the niGSMSG_StoreHeadroom function to store the headroom in the RFSG waveform database.   
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "waveform">
        ///             waveform
        ///         char[]
        ///         Specifies the name of the waveform for which you want to retrieve the headroom. The toolkit uses the waveform parameter as the key to store the waveform attributes in the RFSG waveform database.
        /// 
        ///</param>
        /// <param name = "headroom">
        ///             headroom
        ///         float64*
        ///         Returns the headroom, in dB, stored in the RFSG waveform database. 
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGRetrieveHeadroom(HandleRef rfsgHandle, string waveform, out double headroom)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGRetrieveHeadroom(rfsgHandle, waveform, out headroom);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the IQ rate stored in the RFSG waveform database. The function uses the waveform name as the key.
        /// Note
        /// Use the niGSMSG_StoreIQRate function to store the IQ rate in the RFSG waveform database.   
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "waveform">
        ///             waveform
        ///         char[]
        ///         Specifies the name of the waveform for which you want to retrieve the IQ rate. The toolkit uses the waveform parameter as the key to store the waveform attributes in the RFSG waveform database.
        /// 
        ///</param>
        /// <param name = "iQRate">
        ///             IQRate
        ///         float64*
        ///         Returns the IQ rate stored in the RFSG waveform database for the waveform that you specify in the waveform parameter. 
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGRetrieveIQRate(HandleRef rfsgHandle, string waveform, out double iQRate)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGRetrieveIQRate(rfsgHandle, waveform, out iQRate);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Checks the IQ rate of all the waveforms in the script that you specify in the script parameter. This function returns the IQ rate if the IQ rates are the same for all the waveforms. If the IQ rates are different, the function returns an error.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "script">
        ///             script
        ///         char[]
        ///         Specifies the RFSG script used to generate the signal. The function looks up the IQ rate of all the waveforms contained in the script.
        /// 
        ///</param>
        /// <param name = "iQRate">
        ///             IQRate
        ///         float64*
        ///         Returns the IQ rate if the IQ rates are the same for all the waveforms that you specify in the script parameter. If the IQ rates are different, the function returns an error.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGRetrieveIQRateAllWaveforms(HandleRef rfsgHandle, string script, out double iQRate)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGRetrieveIQRateAllWaveforms(rfsgHandle, script, out iQRate);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Looks up the headroom of all the waveforms contained in the script and returns the minimum of all these headrooms, in dB.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "script">
        ///             script
        ///         char[]
        ///         Specifies the name of the script.
        /// 
        ///</param>
        /// <param name = "headroom">
        ///             headroom
        ///         float64*
        ///         Returns the minimum headroom, in dB, of all the waveforms in the script that you specify in the script parameter. 
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGRetrieveMinimumHeadroomAllWaveforms(HandleRef rfsgHandle, string script, out double headroom)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGRetrieveMinimumHeadroomAllWaveforms(rfsgHandle, script, out headroom);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// niGSMSG_SetScalarAttributeF64
        /// int32 __stdcall niGSMSG_SetScalarAttributeF64(niGSMSGSession session,
        ///     char channelString[], 
        /// Sets the value of an niGSM generation 64-bit floating point number (float64) attribute. 
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// channelString
        /// char[]
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        /// <param name = "attributeID">
        ///             attributeID
        ///         niGSMSG_Attr
        ///         Specifies the ID of a float64 niGSM generation attribute.
        /// 
        ///</param>
        /// <param name = "attributeValue">
        ///             attributeValue
        ///         float64
        ///         Specifies the value to which you want to set the attribute.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int SetScalarAttributeF64(string channelString, niGSMSGProperties attributeID, double attributeValue)
        {
            int pInvokeResult = PInvoke.niGSMSG_SetScalarAttributeF64(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// niGSMSG_SetScalarAttributeI32
        /// int32 __stdcall niGSMSG_SetScalarAttributeI32(niGSMSGSession session,
        ///     char channelString[], 
        ///     niGSMSG_Attr attributeID, 
        /// Sets the value of an niGSM generation 32-bit integer (int32) attribute.
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// 
        ///</param>
        /// <param name = "attributeID">
        ///             attributeID
        ///         niGSMSG_Attr
        ///         Specifies the ID of an int32 niGSM generation attribute.
        /// 
        ///</param>
        /// <param name = "attributeValue">
        ///             attributeValue
        ///         int32
        ///         Specifies the value to which you want to set the attribute.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int SetScalarAttributeI32(string channelString, niGSMSGProperties attributeID, int attributeValue)
        {
            int pInvokeResult = PInvoke.niGSMSG_SetScalarAttributeI32(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// niGSMSG_SetVectorAttributeI32
        /// int32 __stdcall niGSMSG_SetVectorAttributeI32(niGSMSGSession session,
        ///     char channelString[], 
        ///     niGSMSG_Attr attributeID, 
        ///     int32 dataArray[], 
        /// Sets the value of an niGSM generation 32-bit integer number (int32) attribute. 
        /// 
        /// </summary>
        /// <param name = "channelString">
        /// 
        ///</param>
        /// <param name = "attributeID">
        /// attributeID
        /// niGSMSG_Attr
        /// Specifies the ID of a int32 niGSM generation attribute.
        /// 
        ///</param>
        /// <param name = "dataArray">
        ///             dataArray
        ///         int32[]
        ///         Specifies the pointer to the int32 array to which you want to set the attribute.
        /// 
        ///</param>
        /// <param name = "dataArraySize">
        ///             dataArraySize
        ///         int32
        ///         Specifies the number of elements in the int32 array.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int SetVectorAttributeI32(string channelString, niGSMSGProperties attributeID, int[] dataArray, int dataArraySize)
        {
            int pInvokeResult = PInvoke.niGSMSG_SetVectorAttributeI32(Handle, channelString, attributeID, dataArray, dataArraySize);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// niGSMSG_StoreHeadroom
        /// int32 __stdcall niGSMSG_StoreHeadroom(ViSession RFSGHandle, 
        ///                 char waveform[], 
        /// Stores the headroom that you specify in the headroom parameter in the RFSG waveform database.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "waveform">
        ///             waveform
        ///         char[]
        ///         Specifies the name of the waveform for which you want to store the headroom. The toolkit uses the waveform parameter as the key to store the waveform attributes in the RFSG waveform database. 
        /// 
        ///</param>
        /// <param name = "headroom">
        ///             headroom
        ///         float64
        ///         Specifies the headroom, in dB, to store in the RFSG waveform database. 
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGStoreHeadroom(HandleRef rfsgHandle, string waveform, double headroom)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGStoreHeadroom(rfsgHandle, waveform, headroom);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Stores the IQ rate that you specify in the IQRate parameter in the RFSG waveform database.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "waveform">
        ///             waveform
        ///         char[]
        ///         Specifies the IQ rate to store in the RFSG waveform database. 
        /// 
        ///</param>
        /// <param name = "iQRate">
        ///             IQRate
        ///         float64
        ///         Specifies the name of the waveform for which you want to store the IQ rate. The toolkit uses the waveform parameter as the key to store the waveform attributes in the RFSG waveform database. 
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGStoreIQRate(HandleRef rfsgHandle, string waveform, double iQRate)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGStoreIQRate(rfsgHandle, waveform, iQRate);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates the waveform, writes it into the RF signal generator memory, and stores the IQ rate and headroom of the waveform in the RFSG waveform database.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "waveform">
        ///             waveform
        ///         char[]
        ///         specifies the name used to store the waveform. This string is case-insensitive, alphanumeric, and does not use reserved words.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGCreateAndDownloadWaveform(HandleRef rfsgHandle, string waveform)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGCreateAndDownloadWaveform_v1(Handle, rfsgHandle, waveform);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }

        /// <summary>
        /// Queries the selected NI-RFSG script, looks up the waveforms in the script, retrieves the minimum headroom of the waveforms in the script, adds this value to the power level (dBm) parameter, and sets the result to the niRFSG Power Level property. Set the niRFSG Power Level Type property to Peak Power before calling this VI. 
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "powerLevel">
        ///             powerLevel
        ///         double
        ///         Specifies the power level.
        /// 
        ///</param>
        /// <param name = "script">
        ///             script
        ///         char[]
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGConfigurePowerLevel(HandleRef rfsgHandle, double powerLevel, string script)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGConfigurePowerLevel_v1(powerLevel, script, rfsgHandle);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the IQ rate and power level of the waveforms that you specify in the script parameter.
        /// 
        /// </summary>
        /// <param name = "rfsgHandle"> Identifies the RFSG session.</param>
        /// <param name = "script">
        ///         script
        ///         char[]
        ///         script specifies the current RFSG script used to generate the signal. 
        ///</param>
        /// <param name = "powerLevel">
        ///             powerLevel
        ///         double
        ///         Specifies the power level.
        /// 
        ///</param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. Examine the status code from each call to an niGSM generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// 
        /// Value           Meaning
        /// ----------------------------------------
        /// 0               Success 
        /// Positive Values Warnings 
        /// Negative Values Exception will be thrown
        /// </returns>
        public int RFSGConfigureScript(HandleRef rfsgHandle, string script, double powerLevel)
        {
            int pInvokeResult = PInvoke.niGSMSG_RFSGConfigureScript_v1(rfsgHandle, script, powerLevel);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }


        #region Property-Get-Set
        /// <summary>
        /// Indicates the version of the toolkit in use.   
        /// 
        /// </summary>
        public int SetAdvancedToolkitCompatibilityVersion(string channel, int value)
        {
            return SetInt(niGSMSGProperties.AdvancedToolkitCompatibilityVersion, channel, value);
        }

        /// <summary>
        /// Indicates the version of the toolkit in use.   
        /// 
        /// </summary>
        public int GetAdvancedToolkitCompatibilityVersion(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.AdvancedToolkitCompatibilityVersion, channel, out value);
        }

        /// <summary>
        /// Specifies the type of the burst as specified in section 5.2 of 3GPP TS 45.001 Specifications.
        ///     The default value is NIGSMSG_VAL_NORMAL_BURST.
        ///       Refer to the Burst Types topic for more information.   
        /// 
        /// </summary>
        public int SetBurstType(string channel, int value)
        {
            return SetInt(niGSMSGProperties.BurstType, channel, value);
        }

        /// <summary>
        /// Specifies the type of the burst as specified in section 5.2 of 3GPP TS 45.001 Specifications.
        ///     The default value is NIGSMSG_VAL_NORMAL_BURST.
        ///       Refer to the Burst Types topic for more information.   
        /// 
        /// </summary>
        public int GetBurstType(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.BurstType, channel, out value);
        }

        /// <summary>
        /// Specifies the mode of the carrier signal.
        ///     The envelope of the signal varies depending on the carrier mode.
        ///     The default value is NIGSMSG_VAL_CARRIER_MODE_BURST.   
        /// 
        /// </summary>
        public int SetCarrierMode(string channel, int value)
        {
            return SetInt(niGSMSGProperties.CarrierMode, channel, value);
        }

        /// <summary>
        /// Specifies the mode of the carrier signal.
        ///     The envelope of the signal varies depending on the carrier mode.
        ///     The default value is NIGSMSG_VAL_CARRIER_MODE_BURST.   
        /// 
        /// </summary>
        public int GetCarrierMode(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.CarrierMode, channel, out value);
        }

        /// <summary>
        /// Specifies the amount, in dB, by which the output of the modulator is reduced. 
        ///     If you set this attribute to -1, the toolkit automatically adjusts the headroom based on the peak of the signal. 
        ///     The default value is -1.   
        /// 
        /// </summary>
        public int SetHardwareSettingsHeadroom(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.HardwareSettingsHeadroom, channel, value);
        }

        /// <summary>
        /// Specifies the amount, in dB, by which the output of the modulator is reduced. 
        ///     If you set this attribute to -1, the toolkit automatically adjusts the headroom based on the peak of the signal. 
        ///     The default value is -1.   
        /// 
        /// </summary>
        public int GetHardwareSettingsHeadroom(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.HardwareSettingsHeadroom, channel, out value);
        }

        /// <summary>
        /// Returns the actual headroom, in dB, that is applied to the waveform.  
        /// 
        /// </summary>
        public int SetRecommendedHardwareSettingsActualHeadroom(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.RecommendedHardwareSettingsActualHeadroom, channel, value);
        }

        /// <summary>
        /// Returns the actual headroom, in dB, that is applied to the waveform.  
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsActualHeadroom(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.RecommendedHardwareSettingsActualHeadroom, channel, out value);
        }

        /// <summary>
        /// Returns the symbol rate, in hertz (Hz), for generation.
        ///     The modulation symbol rate is 270.833 kilosamples per second. If the oversampling rate is x, the total symbol rate after oversampling is 270.833k * x.   
        /// 
        /// </summary>
        public int SetRecommendedHardwareSettingsIqRate(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.HardwareSettingsRecommendedSettingsIqRate, channel, value);
        }

        /// <summary>
        /// Returns the symbol rate, in hertz (Hz), for generation.
        ///     The modulation symbol rate is 270.833 kilosamples per second. If the oversampling rate is x, the total symbol rate after oversampling is 270.833k * x.   
        /// 
        /// </summary>
        public int GetRecommendedHardwareSettingsIqRate(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.HardwareSettingsRecommendedSettingsIqRate, channel, out value);
        }

        /// <summary>
        /// Specifies the signal-to-noise ratio (SNR) of the waveform generated.
        ///     Noise bandwidth is equal to signal bandwidth.
        ///     The default value is 0.    
        /// 
        /// </summary>
        public int SetIqImpairmentsCarrierToNoiseRatio(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.IqImpairmentsCarrierToNoiseRatio, channel, value);
        }

        /// <summary>
        /// Specifies the signal-to-noise ratio (SNR) of the waveform generated.
        ///     Noise bandwidth is equal to signal bandwidth.
        ///     The default value is 0.    
        /// 
        /// </summary>
        public int GetIqImpairmentsCarrierToNoiseRatio(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.IqImpairmentsCarrierToNoiseRatio, channel, out value);
        }

        /// <summary>
        /// Specifies whether to add impairments to the IQ waveform.
        ///     The default value is NIGSMSG_VAL_FALSE.   
        /// 
        /// </summary>
        public int SetIqImpairmentsAwgnEnabled(string channel, int value)
        {
            return SetInt(niGSMSGProperties.IqImpairmentsAwgnEnabled, channel, value);
        }

        /// <summary>
        /// Specifies whether to add impairments to the IQ waveform.
        ///     The default value is NIGSMSG_VAL_FALSE.   
        /// 
        /// </summary>
        public int GetIqImpairmentsAwgnEnabled(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.IqImpairmentsAwgnEnabled, channel, out value);
        }

        /// <summary>
        /// Specifies the frequency offset, in hertz (Hz).
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int SetIqImpairmentsFrequencyOffset(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.IqImpairmentsFrequencyOffset, channel, value);
        }

        /// <summary>
        /// Specifies the frequency offset, in hertz (Hz).
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int GetIqImpairmentsFrequencyOffset(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.IqImpairmentsFrequencyOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the ratio, in dB, of the mean amplitude of the in-phase (I) signal to the mean amplitude of the quadrature-phase (Q) signal.
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int SetIqImpairmentsGainImbalance(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.IqImpairmentsGainImbalance, channel, value);
        }

        /// <summary>
        /// Specifies the ratio, in dB, of the mean amplitude of the in-phase (I) signal to the mean amplitude of the quadrature-phase (Q) signal.
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int GetIqImpairmentsGainImbalance(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.IqImpairmentsGainImbalance, channel, out value);
        }

        /// <summary>
        /// Specifies the value of the DC offset in the in-phase (I) signal as a percentage of the root mean square (RMS) magnitude of the unaltered I signal.
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int SetIqImpairmentsIOffset(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.IqImpairmentsIOffset, channel, value);
        }

        /// <summary>
        /// Specifies the value of the DC offset in the in-phase (I) signal as a percentage of the root mean square (RMS) magnitude of the unaltered I signal.
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int GetIqImpairmentsIOffset(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.IqImpairmentsIOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the value of the DC offset in the quadrature-phase (Q) signal as percentage of the root mean square (RMS) magnitude of the unaltered Q signal.
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int SetIqImpairmentsQOffset(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.IqImpairmentsQOffset, channel, value);
        }

        /// <summary>
        /// Specifies the value of the DC offset in the quadrature-phase (Q) signal as percentage of the root mean square (RMS) magnitude of the unaltered Q signal.
        ///     The default value is 0.   
        /// 
        /// </summary>
        public int GetIqImpairmentsQOffset(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.IqImpairmentsQOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the deviation in angle from 90 degrees between the in-phase (I) and quadrature-phase (Q) signals.
        ///     The default value is 0.
        ///     Refer to the Quadrature Skew topic for more information.   
        /// 
        /// </summary>
        public int SetIqImpairmentsQuadratureSkew(string channel, double value)
        {
            return SetDouble(niGSMSGProperties.IqImpairmentsQuadratureSkew, channel, value);
        }

        /// <summary>
        /// Specifies the deviation in angle from 90 degrees between the in-phase (I) and quadrature-phase (Q) signals.
        ///     The default value is 0.
        ///     Refer to the Quadrature Skew topic for more information.   
        /// 
        /// </summary>
        public int GetIqImpairmentsQuadratureSkew(string channel, out double value)
        {
            return GetDouble(niGSMSGProperties.IqImpairmentsQuadratureSkew, channel, out value);
        }

        /// <summary>
        /// Specifies the data to be encapsulated in the useful portion of the burst.
        ///     The default value is NIGSMSG_VAL_PAYLOAD_MODE_PN.
        ///     Refer to the Useful Portion of a Burst topic for more information.    
        /// 
        /// </summary>
        public int SetPayloadControlMode(string channel, int value)
        {
            return SetInt(niGSMSGProperties.PayloadControlMode, channel, value);
        }

        /// <summary>
        /// Specifies the data to be encapsulated in the useful portion of the burst.
        ///     The default value is NIGSMSG_VAL_PAYLOAD_MODE_PN.
        ///     Refer to the Useful Portion of a Burst topic for more information.    
        /// 
        /// </summary>
        public int GetPayloadControlMode(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.PayloadControlMode, channel, out value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator. The generated sequence is repeated after (2^PN order)-1 bits.
        ///     This attribute is applicable only if you set the NIGSMSG_PAYLOAD_CONTROL_MODE attribute to NIGSMSG_VAL_PAYLOAD_MODE_PN.
        ///     The default value is 9.   
        /// 
        /// </summary>
        public int SetPayloadControlPnOrder(string channel, int value)
        {
            return SetInt(niGSMSGProperties.PayloadControlPnOrder, channel, value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator. The generated sequence is repeated after (2^PN order)-1 bits.
        ///     This attribute is applicable only if you set the NIGSMSG_PAYLOAD_CONTROL_MODE attribute to NIGSMSG_VAL_PAYLOAD_MODE_PN.
        ///     The default value is 9.   
        /// 
        /// </summary>
        public int GetPayloadControlPnOrder(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.PayloadControlPnOrder, channel, out value);
        }

        /// <summary>
        /// Specifies the initialization seed to use in the pseudorandom bit sequence (PRBS) generator.
        ///     This attribute is applicable only if you set the NIGSMSG_PAYLOAD_CONTROL_MODE attribute to NIGSMSG_VAL_PAYLOAD_MODE_PN.
        ///     The default value is OxBEEFBEEF.   
        /// 
        /// </summary>
        public int SetPayloadControlPnSeed(string channel, int value)
        {
            return SetInt(niGSMSGProperties.PayloadControlPnSeed, channel, value);
        }

        /// <summary>
        /// Specifies the initialization seed to use in the pseudorandom bit sequence (PRBS) generator.
        ///     This attribute is applicable only if you set the NIGSMSG_PAYLOAD_CONTROL_MODE attribute to NIGSMSG_VAL_PAYLOAD_MODE_PN.
        ///     The default value is OxBEEFBEEF.   
        /// 
        /// </summary>
        public int GetPayloadControlPnSeed(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.PayloadControlPnSeed, channel, out value);
        }

        /// <summary>
        /// Specifies the training sequence code used in the burst.
        ///     The default value is NIGSMSG_VAL_TSC0.   
        /// 
        /// </summary>
        public int SetPayloadControlTsc(string channel, int value)
        {
            return SetInt(niGSMSGProperties.PayloadControlTsc, channel, value);
        }

        /// <summary>
        /// Specifies the training sequence code used in the burst.
        ///     The default value is NIGSMSG_VAL_TSC0.   
        /// 
        /// </summary>
        public int GetPayloadControlTsc(string channel, out int value)
        {
            return GetInt(niGSMSGProperties.PayloadControlTsc, channel, out value);
        }

        /// <summary>
        /// Specifies the user-defined bits for the useful portion of the burst.
        ///     Refer to the Useful Portion of a Burst topic for more information.  
        /// 
        /// </summary>
        public int SetPayloadControlUserDefinedBits(string channel, int[] value, int dataArraySize)
        {
            return SetIntArray(niGSMSGProperties.PayloadControlUserDefinedBits, channel, value, dataArraySize);
        }

        /// <summary>
        /// Specifies the user-defined bits for the useful portion of the burst.
        ///     Refer to the Useful Portion of a Burst topic for more information.  
        /// 
        /// </summary>
        public int GetPayloadControlUserDefinedBits(string channel, int[] value, int dataArraySize, out int actualNumDataArrayElements)
        {
            return GetIntArray(niGSMSGProperties.PayloadControlUserDefinedBits, channel, value, dataArraySize, out actualNumDataArrayElements);
        }

        private int SetInt(niGSMSGProperties propertyId, string repeatedCapabilityOrChannel, int value)
        {
            return TestForError(PInvoke.niGSMSG_SetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, value));
        }

        private int SetInt(niGSMSGProperties propertyId, int value)
        {
            return this.SetInt(propertyId, "", value);
        }

        private int GetInt(niGSMSGProperties propertyId, string repeatedCapabilityOrChannel, out int value)
        {
            return TestForError(PInvoke.niGSMSG_GetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, out value));
        }

        private int GetInt(niGSMSGProperties propertyId, out int value)
        {
            return this.GetInt(propertyId, "", out value);
        }

        private int SetDouble(niGSMSGProperties propertyId, string repeatedCapabilityOrChannel, double value)
        {
            return TestForError(PInvoke.niGSMSG_SetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, value));
        }

        private int SetDouble(niGSMSGProperties propertyId, double value)
        {
            return this.SetDouble(propertyId, "", value);
        }

        private int GetDouble(niGSMSGProperties propertyId, string repeatedCapabilityOrChannel, out double value)
        {
            return TestForError(PInvoke.niGSMSG_GetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, (propertyId), out value));

        }

        private int GetDouble(niGSMSGProperties propertyId, out double value)
        {
            return this.GetDouble(propertyId, "", out value);
        }

        private int SetIntArray(niGSMSGProperties propertyId, string repeatedCapabilityOrChannel, int[] value, int dataArraySize)
        {
            return TestForError(PInvoke.niGSMSG_SetVectorAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, value, dataArraySize));
        }

        private int SetIntArray(niGSMSGProperties propertyId, int[] value, int arrayDataSize)
        {
            return this.SetIntArray(propertyId, "", value, arrayDataSize);
        }

        private int GetIntArray(niGSMSGProperties propertyId, string repeatedCapabilityOrChannel, int[] value, int dataArraySize, out int actualNumDataArrayElements)
        {
            return TestForError(PInvoke.niGSMSG_GetVectorAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, value, dataArraySize, out actualNumDataArrayElements));

        }

        private int GetIntArray(niGSMSGProperties propertyId, int[] value, int dataArraySize, out int actualNumDataArrayElements)
        {
            return this.GetIntArray(propertyId, "", value, dataArraySize, out actualNumDataArrayElements);
        }

        #endregion

        private int GetErrorString(int status, StringBuilder msg)
        {
            int size = PInvoke.niGSMSG_GetErrorString(Handle, status, null, 0);
            if ((size >= 0))
            {
                msg.Capacity = size;
                PInvoke.niGSMSG_GetErrorString(Handle, status, msg, size);
            }
            return status;
        }
        public int TestForError(int status)
        {
            if ((status < 0))
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                status = GetErrorString(status, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }

        private int TestForError(int status, HandleRef rfsgHandle)
        {
            if ((status < 0))
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                GetErrorString(status, msg);
                // get RFSG detailed error message.
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSG.GetError(rfsgHandle, status, msg);
                //get rfsg general error message.
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSG.ErrorMessage(rfsgHandle, status, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }

        #region IDisposable Members

        /// <summary>
        /// Closes the niGSM Generation unnamed session and releases resources associated with that unnamed session.
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!_isNamedSession)
            {
                this.Dispose(true);
                System.GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
            }
            // Dispose() does not close a named session. Users must call Close() to close a named session.
            if (!_isNamedSession)
            {
                // Dispose unmanaged resources
                // Handle.Handle is IntPtr.Zero when the session is inactive/closed.
                if (!Handle.Handle.Equals(IntPtr.Zero))
                {
                    PInvoke.niGSMSG_CloseSession(Handle);
                }
            }
        }

        #endregion
        private class PInvoke
        {
            const string nativeDllName = "niGSMSG_net.dll";
            const string nativeLVDllName = "niGSMSG.dll";

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_ARFCNtoCarrierFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_ARFCNtoCarrierFrequency(int uUT, int band, int aRFCN, out double carrierFrequency);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_CloseSession(System.Runtime.InteropServices.HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_CreateBurst", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_CreateBurst(System.Runtime.InteropServices.HandleRef session, out double t0, out double dt, [Out] niComplexNumber[] waveform, int length, out int waveformSize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_GetErrorString(System.Runtime.InteropServices.HandleRef session, int errorCode, StringBuilder errorMessage, int errorMessageLength);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_GetScalarAttributeF64(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID, out double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_GetScalarAttributeI32(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID, out int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_GetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_GetVectorAttributeI32(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID, [In, Out] int[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_OpenSession(string sessionName, int toolkitCompatibilityVersion, out int isNewSession, out IntPtr session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_ResetAttribute(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_ResetSession(System.Runtime.InteropServices.HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGClearDatabase", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGClearDatabase(System.Runtime.InteropServices.HandleRef rFSGHandle, string waveform);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGRetrieveHeadroom", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGRetrieveHeadroom(System.Runtime.InteropServices.HandleRef rFSGHandle, string waveform, out double headroom);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGRetrieveIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGRetrieveIQRate(System.Runtime.InteropServices.HandleRef rFSGHandle, string waveform, out double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGRetrieveIQRateAllWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGRetrieveIQRateAllWaveforms(System.Runtime.InteropServices.HandleRef rFSGHandle, string script, out double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGRetrieveMinimumHeadroomAllWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGRetrieveMinimumHeadroomAllWaveforms(System.Runtime.InteropServices.HandleRef rFSGHandle, string script, out double headroom);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGStoreHeadroom", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGStoreHeadroom(System.Runtime.InteropServices.HandleRef rFSGHandle, string waveform, double headroom);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGStoreIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGStoreIQRate(System.Runtime.InteropServices.HandleRef rFSGHandle, string waveform, double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_SetScalarAttributeF64(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID, double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_SetScalarAttributeI32(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID, int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_SetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_SetVectorAttributeI32(System.Runtime.InteropServices.HandleRef session, string channelString, niGSMSGProperties
               attributeID, int[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGCreateAndDownloadWaveform_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGCreateAndDownloadWaveform_v1(System.Runtime.InteropServices.HandleRef gsmsgsession, System.Runtime.InteropServices.HandleRef rfsgSession, string waveformName);

            [DllImport(nativeLVDllName, EntryPoint = "niGSMSG_ConfigurePowerLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGConfigurePowerLevel_v1(double powerLevel, string script, System.Runtime.InteropServices.HandleRef rfsgSession);

            [DllImport(nativeDllName, EntryPoint = "niGSMSG_RFSGConfigureScript_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niGSMSG_RFSGConfigureScript_v1(System.Runtime.InteropServices.HandleRef rfsgSession, string script, double powerLevel);

        }

    }




    public class niGSMSGConstants
    {

        public const int AccessBurst = 0;

        public const int DummyBurst = 1;

        public const int FrequencyCorrectionBurst = 2;

        public const int NormalBurst = 3;

        public const int NormalBurstCw = 4;

        public const int SynchronizationBurst = 5;

        public const int IdleBurst = 6;

        public const int CarrierModeBurst = 0;

        public const int CarrierModeContinuous = 1;

        public const int Tsc0 = 0;

        public const int Tsc1 = 1;

        public const int Tsc2 = 2;

        public const int Tsc3 = 3;

        public const int Tsc4 = 4;

        public const int Tsc5 = 5;

        public const int Tsc6 = 6;

        public const int Tsc7 = 7;

        public const int PayloadModePn = 0;

        public const int PayloadModeUserDefined = 1;

        public const int False = 0;

        public const int True = 1;

        public const int ToolkitVersion100 = 100;

        public const int UutBts = 0;

        public const int UutMS = 1;

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

    }
    public enum niGSMSGProperties
    {

        /// <summary>
        /// int
        /// </summary>
        AdvancedToolkitCompatibilityVersion = 65533,

        /// <summary>
        /// int
        /// </summary>
        BurstType = 4,

        /// <summary>
        /// int
        /// </summary>
        CarrierMode = 3,

        /// <summary>
        /// double
        /// </summary>
        HardwareSettingsHeadroom = 5,

        /// <summary>
        /// double
        /// </summary>
        RecommendedHardwareSettingsActualHeadroom = 26,

        /// <summary>
        /// double
        /// </summary>
        HardwareSettingsRecommendedSettingsIqRate = 20,

        /// <summary>
        /// double
        /// </summary>
        IqImpairmentsCarrierToNoiseRatio = 19,

        /// <summary>
        /// int
        /// </summary>
        IqImpairmentsAwgnEnabled = 13,

        /// <summary>
        /// double
        /// </summary>
        IqImpairmentsFrequencyOffset = 18,

        /// <summary>
        /// double
        /// </summary>
        IqImpairmentsGainImbalance = 14,

        /// <summary>
        /// double
        /// </summary>
        IqImpairmentsIOffset = 16,

        /// <summary>
        /// double
        /// </summary>
        IqImpairmentsQOffset = 17,

        /// <summary>
        /// double
        /// </summary>
        IqImpairmentsQuadratureSkew = 15,

        /// <summary>
        /// int
        /// </summary>
        PayloadControlMode = 7,

        /// <summary>
        /// int
        /// </summary>
        PayloadControlPnOrder = 10,

        /// <summary>
        /// int
        /// </summary>
        PayloadControlPnSeed = 11,

        /// <summary>
        /// int
        /// </summary>
        PayloadControlTsc = 6,

        /// <summary>
        /// int32
        /// </summary>
        PayloadControlUserDefinedBits = 9,

    }

}


