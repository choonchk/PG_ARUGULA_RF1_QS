//==================================================================================================
// Title        : niWCDMASG.cs
// Copyright    : National Instruments 2010. All Rights Reserved.
// Purpose      : 
//===================================================================================================
using System;
using System.Runtime.InteropServices;
using System.Text;
using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.RFToolkits.Interop
{
    public class niWCDMASG : IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _isNamedSession;

        ~niWCDMASG()
        {
            Dispose(false);
        }

        /// <summary>
        /// Looks up an existing niWCDMA generation session and returns the refnum that you can pass to subsequent niWCDMA generation functions. If the lookup fails, the niWCDMASG_OpenSession function creates a new niWCDMA generation session and returns a new refnum.
        /// 
        /// </summary>
        ///<param name = "toolkitCompatibilityVersion">
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    NIWCDMASG_VAL_TOOLKIT_COMPATIBILITY_VERSION_010000 (10000)
        ///   Specifies that the toolkit version is 1.0.0.
        /// 
        ///</param>
        public niWCDMASG(int toolkitCompatibilityVersion)
        {
            IntPtr handle;
            int isNewSession;
            int pInvokeResult = PInvoke.niWCDMASG_OpenSession(string.Empty, toolkitCompatibilityVersion, out handle, out isNewSession);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, handle);
            _isNamedSession = false;
        }

        /// <summary>
        /// Looks up an existing niWCDMA generation session and returns the refnum that you can pass to subsequent niWCDMA generation functions. If the lookup fails, the niWCDMASG_OpenSession function creates a new niWCDMA generation session and returns a new refnum.
        /// 
        /// </summary>
        ///<param name = "sessionName">
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. To get the reference to an already-opened session x, specify x as the session name. 
        ///  You can obtain the reference to an existing session multiple times if you have not called the niWCDMASG_CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string or NULL to the sessionName parameter.
        /// Tip&nbsp;&nbsp;National Instruments recommends that you call the niWCDMASG_CloseSession function for each uniquely-named instance of the niWCDMASG_OpenSession function or each instance of the niWCDMASG_OpenSession function with an unnamed session.
        /// 
        ///</param>
        ///<param name = "toolkitCompatibilityVersion">
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///    NIWCDMASG_VAL_TOOLKIT_COMPATIBILITY_VERSION_010000 (10000)
        ///   Specifies that the toolkit version is 1.0.0.
        ///</param>
        ///<param name = "isNewSession">
        /// Returns NIWCDMASG_VAL_TRUE if the function creates a new session. This parameter returns NIWCDMASG_VAL_FALSE if the function returns a reference to an existing session.
        ///</param>
        public niWCDMASG(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            System.IntPtr instrumentHandle;
            int pInvokeResult = PInvoke.niWCDMASG_OpenSession(sessionName, toolkitCompatibilityVersion, out instrumentHandle, out isNewSession);
            TestForError(pInvokeResult);
            _handle = new HandleRef(this, instrumentHandle);
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
        /// Closes the niWCDMA generation session and releases resources associated with that session. Call this function once for each uniquely-named session that you have created. 
        /// 
        /// </summary>
        public void Close()
        {
            if (!_isNamedSession)
                Dispose();
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                    PInvoke.niWCDMASG_CloseSession(Handle);
            }
        }
        /// <summary>
        /// Specifies the test model that the toolkit uses to configure the session as defined in the section 6.1 of the 3GPP TS 25.213 Specifications 8.4.0. 
        /// 
        /// </summary>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "testModel">
        /// Specifies the test model that the toolkit uses to configure the session. 
        /// NIWCDMASG_VAL_DL_TEST_MODEL_1_4_DPCH (0)
        /// Specifies test model 1 with 4 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_1_8_DPCH (1)
        /// Specifies test model 1 with 8 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_1_16_DPCH (2)
        /// Specifies test model 1 with 16 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_1_32_DPCH (3)
        /// Specifies test model 1 with 32 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_1_64_DPCH (4)
        /// Specifies test model 1 with 64 DPCH. This value is the default.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_2 (5)
        /// Specifies test model 2.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_3_4_DPCH (6)
        /// Specifies test model 3 with 4 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_3_8_DPCH (7)
        /// Specifies test model 3 with 8 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_3_16_DPCH (8)
        /// Specifies test model 3 with 16 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_3_32_DPCH (9)
        /// Specifies test model 3 with 32 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_4_WITHOUT_CPICH (10)
        /// Specifies test model 4 without CPICH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_4_WITH_CPICH (11)
        /// Specifies test model 4 with CPICH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_5_2_HSPDSCH_6_DPCH (12)
        /// Specifies test model 5 with 2 HSPDSCH and 6 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_5_4_HSPDSCH_4_DPCH (13)
        /// Specifies test model 5 with 4 HSPDSCH and 4 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_5_4_HSPDSCH_14_DPCH (14)
        /// Specifies test model 5 with 4 HSPDSCH and 14 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_5_8_HSPDSCH_30_DPCH (15)
        /// Specifies test model 5 with 8 HSPDSCH and 30 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_6_4_HSPDSCH_4_DPCH (16)
        /// Specifies test model 6 with 4 HSPDSCH and 4 DPCH.
        /// NIWCDMASG_VAL_DL_TEST_MODEL_6_8_HSPDSCH_30_DPCH (17)
        /// Specifies test model 6 with 8 HSPDSCH and 30 DPCH.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ConfigureDownlinkTestModel(string channelString, int testModel)
        {
            int pInvokeResult = PInvoke.niWCDMASG_ConfigureDownlinkTestModel(Handle, channelString, testModel);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Creates WCDMA IQ data and returns the data as complex waveform data. This function returns one frame (including the idle interval) at a time. For multi-frame generation, run the function in a while loop for specified number of times with the reset input set to NIWCDMA_VAL_FALSE. 
        /// 
        /// </summary>
        ///<param name = "t0">
        /// Returns the start time of the waveform.
        /// 
        ///</param>
        ///<param name = "dt">
        /// Returns the time interval between waveform samples.
        /// 
        ///</param>
        ///<param name = "waveform">
        /// Returns the WCDMA I/Q data. This parameter must at least be of size waveformSize. You can pass NULL to the waveform parameter to query the size of the waveform.
        /// 
        ///</param>
        ///<param name = "waveformSize">
        /// Specifies the waveform size in samples.
        ///</param>
        ///<param name = "actualNumWaveformSamples">
        /// Returns the actual number of samples populated in waveform array. If the array is not large enough to hold all the samples, the function returns an error and this parameter returns the minimum expected size of the output array.
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        ///
        /// </returns>
        public int CreateWaveformComplexF64(out double t0, out double dt, niComplexNumber[] waveform, int waveformSize, out int actualNumWaveformSamples)
        {
            int pInvokeResult = PInvoke.niWCDMASG_CreateWaveformComplexF64(Handle, 1, out t0, out dt, waveform, waveformSize, out actualNumWaveformSamples, 0);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Takes the error code returned by niWCDMA generation functions and returns the interpretation as a user-readable string. 
        /// 
        /// </summary>
        ///<param name = "errorCode">
        /// Specifies the error code that is returned from any of the niWCDMA generation functions.
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
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        ///
        /// </returns>
        public int GetErrorString(int errorCode, StringBuilder errorMessage, int errorMessageLength)
        {
            int pInvokeResult = PInvoke.niWCDMASG_GetErrorString(Handle, errorCode, errorMessage, errorMessageLength);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets the attribute specified in the attributeID parameter to its default value. You can reset only a writable attribute using this function.
        /// 
        /// </summary>
        ///<param name = "channelString">
        /// If the attribute is channel based, this parameter specifies the channel to which the attribute applies. If the attribute is not channel based, set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "attributeID">
        /// Specifies the ID of the niWCDMA generation attribute that you want to reset.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ResetAttribute(string channelString, niWCDMASGProperties attributeID)
        {
            int pInvokeResult = PInvoke.niWCDMASG_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Resets all the attributes of the session to their default values. 
        /// 
        /// </summary>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niWCDMASG_ResetSession(Handle);
            TestForError(pInvokeResult);
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
            int pInvokeResult = PInvoke.niWCDMASG_RFSGCreateAndDownloadWaveform_v1(rfsgHandle, Handle, waveform);
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
        public int RFSGConfigurePowerLevel(HandleRef rfsgHandle, string channelString, string script, double powerLevel)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGConfigurePowerLevel_v1(rfsgHandle, channelString, script, powerLevel);
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
        public int RFSGConfigureScript(HandleRef rfsgHandle, string channelString, string script, double powerLevel)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGConfigureScript_v1(rfsgHandle, channelString, script, powerLevel);
            TestForError(pInvokeResult, rfsgHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Clears the attributes stored in the RFSG waveform database and clears the waveforms from the RF signal generator memory. 
        /// This function clears the waveforms and the attributes of the waveforms that you specify in the waveformName parameter. If you set the waveformName parameter as empty, this function clears all the waveforms and their attributes. 
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "waveform">
        /// Specifies the names of the waveforms to clear. If you set this parameter as empty, the function clears all the waveforms and their attributes. 
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int RFSGClearDatabase(HandleRef rFSGHandle, string channelString, string waveform)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGClearDatabase(rFSGHandle, channelString, waveform);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the headroom, in dB, stored in the RFSG waveform database. The function uses the waveform name as the key. 
        /// Note
        /// Use the niWCDMASG_RFSGStoreHeadroom function to store the headroom in the RFSG waveform database.  
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "waveformName">
        /// Specifies the name of the waveform for which you want to retrieve the headroom. The toolkit uses the waveformName parameter as the key to store the waveform attributes in the RFSG waveform database.
        /// 
        ///</param>
        ///<param name = "headroom">
        /// Returns the headroom, in dB, stored in the RFSG waveform database. 
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int RFSGRetrieveHeadroom(HandleRef rFSGHandle, string channelString, string waveformName, out double headroom)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGRetrieveHeadroom(rFSGHandle, channelString, waveformName, out headroom);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// <p class="Body">Returns the IQ rate stored in the RFSG waveform database. The function uses the waveform name as the key. 
        /// Note
        /// Use the niWCDMASG_RFSGStoreIQRate function to store the headroom in the RFSG waveform database.  
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "waveformName">
        /// Specifies the name of the waveform for which you want to retrieve the IQ rate. The toolkit uses the waveformName parameter as the key to store the waveform attributes in the RFSG waveform database. 
        /// 
        ///</param>
        ///<param name = "iQRate">
        /// Returns the IQ rate stored in the RFSG waveform database for the waveform that you specify in the waveformName parameter. 
        /// 
        ///</param>
        ///<returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        ///
        /// </returns>
        public int RFSGRetrieveIQRate(HandleRef rFSGHandle, string channelString, string waveformName, out double iQRate)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGRetrieveIQRate(rFSGHandle, channelString, waveformName, out iQRate);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Checks the IQ rate of all the waveforms in the script that you specify in the script parameter. This function returns the IQ rate if the IQ rates are the same for all the waveforms. If the IQ rates are different, the function returns an error.
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "script">
        /// Specifies the RFSG script used to generate the signal. The function looks up the IQ rate of all the waveforms contained in the script.
        /// 
        ///</param>
        ///<param name = "iQRate">
        /// Returns the IQ rate if the IQ rates are the same for all the waveforms that you specify in the script parameter. If the IQ rates are different, the function returns an error.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int RFSGRetrieveIQRateAllWaveforms(HandleRef rFSGHandle, string channelString, string script, out double iQRate)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGRetrieveIQRateAllWaveforms(rFSGHandle, channelString, script, out iQRate);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Looks up the headroom of all the waveforms contained in the script and returns the minimum of all these headrooms, in dB.
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "script">
        /// Specifies the RFSG script used to generate the signal. The function looks up the headroom of all the waveforms contained in the script and returns the minimum of all these headrooms, in dB.
        /// 
        ///</param>
        ///<param name = "headroom">
        /// Returns the minimum headroom, in dB, of all the waveforms in the script that you specify in the script parameter. 
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int RFSGRetrieveMinimumHeadroomAllWaveforms(HandleRef rFSGHandle, string channelString, string script, out double headroom)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGRetrieveMinimumHeadroomAllWaveforms(rFSGHandle, channelString, script, out headroom);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Stores the headroom that you specify in the headroom parameter in the RFSG waveform database. 
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "waveformName">
        /// Specifies the name of the waveform for which you want to store the headroom. The toolkit uses the waveformName parameter as the key to store the waveform attributes in the RFSG waveform database. 
        /// 
        ///</param>
        ///<param name = "headroom">
        /// Specifies the headroom, in dB, to store in the RFSG waveform database. 
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int RFSGStoreHeadroom(HandleRef rFSGHandle, string channelString, string waveformName, double headroom)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGStoreHeadroom(rFSGHandle, channelString, waveformName, headroom);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Stores the IQ rate that you specify in the IQRate parameter in the RFSG waveform database. 
        /// 
        /// </summary>
        ///<param name = "rFSGHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// 
        ///</param>
        ///<param name = "channelString">
        /// Set this parameter to "" (empty string) or NULL.
        /// 
        ///</param>
        ///<param name = "waveformName">
        /// Specifies the IQ rate to store in the RFSG waveform database. 
        /// 
        ///</param>
        ///<param name = "iQRate">
        /// Specifies the name of the waveform for which you want to store the IQ rate. The toolkit uses the waveformName parameter as the key to store the waveform attributes in the RFSG waveform database. 
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition.	
        /// Examine the status code from each call to an niWCDMA generation function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niWCDMASG_GetErrorString function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int RFSGStoreIQRate(HandleRef rFSGHandle, string channelString, string waveformName, double iQRate)
        {
            int pInvokeResult = PInvoke.niWCDMASG_RFSGStoreIQRate(rFSGHandle, channelString, waveformName, iQRate);
            TestForError(pInvokeResult, rFSGHandle);
            return pInvokeResult;
        }
        /// <summary>
        /// Returns the actual headroom used by the toolkit. You can use this value to configure the peak power of the generation hardware.
        /// 
        /// </summary>
        public int GetActualHeadroom(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.ActualHeadroom, channel, out value);
        }

        /// <summary>
        /// Specifies whether to enable auto headroom.
        ///    The default value is NIWCDMASG_VAL_TRUE.
        /// 
        /// </summary>
        public int SetAutoHeadroomEnabled(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.AutoHeadroomEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether to enable auto headroom.
        ///    The default value is NIWCDMASG_VAL_TRUE.
        /// 
        /// </summary>
        public int GetAutoHeadroomEnabled(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.AutoHeadroomEnabled, channel, out value);
        }

        /// <summary>
        /// Specifies whether additive white Gaussian noise (AWGN) must be added to the baseband waveform.
        ///    The default value is NIWCDMASG_VAL_FALSE.
        /// 
        /// </summary>
        public int SetAwgnEnabled(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.AwgnEnabled, channel, value);
        }
        /// <summary>
        /// Specifies whether additive white Gaussian noise (AWGN) must be added to the baseband waveform.
        ///    The default value is NIWCDMASG_VAL_FALSE.
        /// 
        /// </summary>
        public int GetAwgnEnabled(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.AwgnEnabled, channel, out value);
        }

        /// <summary>
        /// Specifies the offset, in hertz (Hz), from the value you specify in the center frequency of the RF signal generator.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetCarrierFrequencyOffset(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.CarrierFrequencyOffset, channel, value);
        }
        /// <summary>
        /// Specifies the offset, in hertz (Hz), from the value you specify in the center frequency of the RF signal generator.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetCarrierFrequencyOffset(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.CarrierFrequencyOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the carrier to noise ratio (CNR) of the waveform generated. Noise bandwidth is equal to signal bandwidth.
        ///    The default value is 50.
        /// 
        /// </summary>
        public int SetCarrierToNoiseRatio(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.CarrierToNoiseRatio, channel, value);
        }
        /// <summary>
        /// Specifies the carrier to noise ratio (CNR) of the waveform generated. Noise bandwidth is equal to signal bandwidth.
        ///    The default value is 50.
        /// 
        /// </summary>
        public int GetCarrierToNoiseRatio(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.CarrierToNoiseRatio, channel, out value);
        }

        /// <summary>
        /// Specifies the extended acquisition indicators used for creating AICH real-valued signals.    Refer to section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int SetDlAichExtendedIndicator(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlAichExtendedIndicator, channel, value);
        }
        /// <summary>
        /// Specifies the extended acquisition indicators used for creating AICH real-valued signals.    Refer to section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int GetDlAichExtendedIndicator(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlAichExtendedIndicator, channel, out value);
        }

        /// <summary>
        /// Specifies the relative amplitude weighting factor used for creating AICH real-valued signals.    Refer to section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0.5. Valid values are 0 to 1, inclusive.
        /// 
        /// </summary>
        public int SetDlAichGamma(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.DlAichGamma, channel, value);
        }
        /// <summary>
        /// Specifies the relative amplitude weighting factor used for creating AICH real-valued signals.    Refer to section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0.5. Valid values are 0 to 1, inclusive.
        /// 
        /// </summary>
        public int GetDlAichGamma(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.DlAichGamma, channel, out value);
        }

        /// <summary>
        /// Specifies the acquisition indicators used for creating AICH real-valued signals.    Refer to section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int SetDlAichIndicators(string channel, int[] value)
        {
            return SetIntArray(niWCDMASGProperties.DlAichIndicators, channel, value);
        }
        /// <summary>
        /// Specifies the acquisition indicators used for creating AICH real-valued signals.    Refer to section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int GetDlAichIndicators(string channel, int[] value, out int actualNumberOfPoints)
        {
            return GetIntArray(niWCDMASGProperties.DlAichIndicators, channel, value, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the signature pattern as defined in section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int SetDlAichSignature(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlAichSignature, channel, value);
        }
        /// <summary>
        /// Specifies the signature pattern as defined in section 5.3.3.7 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int GetDlAichSignature(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlAichSignature, channel, out value);
        }

        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int SetDlChannelDataType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlChannelDataType, channel, value);
        }
        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int GetDlChannelDataType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlChannelDataType, channel, out value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_DL_CHANNEL_DATA_TYPE attribute is    set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_CHANNEL_PN_ORDER attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 9. Valid values are 5 to 31, inclusive.
        /// 
        /// </summary>
        public int SetDlChannelPnOrder(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlChannelPnOrder, channel, value);
        }
        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_DL_CHANNEL_DATA_TYPE attribute is    set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_CHANNEL_PN_ORDER attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 9. Valid values are 5 to 31, inclusive.
        /// 
        /// </summary>
        public int GetDlChannelPnOrder(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlChannelPnOrder, channel, out value);
        }

        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_DL_CHANNEL_DATA_TYPE attribute is    set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_CHANNEL_PN_SEED attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int SetDlChannelPnSeed(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlChannelPnSeed, channel, value);
        }
        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_DL_CHANNEL_DATA_TYPE attribute is    set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_CHANNEL_PN_SEED attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int GetDlChannelPnSeed(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlChannelPnSeed, channel, out value);
        }

        /// <summary>
        /// Specifies the power of the downlink channel relative to the carrier power, in dB.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    If you do not configure this attribute for a channel, the toolkit chooses the default value based on the total    number of channels and the channels for which you have configured this attribute.
        ///    Refer to the Relative Power topic for more information.
        ///    Valid values are -60 to 0, inclusive.
        ///    Note: The sum of all the relative channel powers must be 0 dB. The relative channel powers are equally divided for the non-configured channels.
        /// 
        /// </summary>
        public int SetDlChannelRelativePower(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.DlChannelRelativePower, channel, value);
        }
        /// <summary>
        /// Specifies the power of the downlink channel relative to the carrier power, in dB.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    If you do not configure this attribute for a channel, the toolkit chooses the default value based on the total    number of channels and the channels for which you have configured this attribute.
        ///    Refer to the Relative Power topic for more information.
        ///    Valid values are -60 to 0, inclusive.
        ///    Note: The sum of all the relative channel powers must be 0 dB. The relative channel powers are equally divided for the non-configured channels.
        /// 
        /// </summary>
        public int GetDlChannelRelativePower(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.DlChannelRelativePower, channel, out value);
        }

        /// <summary>
        /// Specifies the spreading code number for the downlink channel.    The spreading code number denotes a particular orthogonal variable spread factor (OVSF) code of a given spread factor.    If the spreading code of the channel conflicts with that of any other channel, the toolkit returns an error.    Refer to section 5.1 of the 3GPP TS 25.213 Specifications 8.4.0 for the downlink spreading operation and section 4.3    of the 3GPP TS 25.213 Specifications 8.4.0 for OVSF code generation.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int SetDlChannelSpreadingCode(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlChannelSpreadingCode, channel, value);
        }
        /// <summary>
        /// Specifies the spreading code number for the downlink channel.    The spreading code number denotes a particular orthogonal variable spread factor (OVSF) code of a given spread factor.    If the spreading code of the channel conflicts with that of any other channel, the toolkit returns an error.    Refer to section 5.1 of the 3GPP TS 25.213 Specifications 8.4.0 for the downlink spreading operation and section 4.3    of the 3GPP TS 25.213 Specifications 8.4.0 for OVSF code generation.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int GetDlChannelSpreadingCode(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlChannelSpreadingCode, channel, out value);
        }

        /// <summary>
        /// Specifies the timing offset for the downlink channel.    The toolkit multiplies the value you specify by 256. Therefore, the timing offset is always specified in multiples of 256 chips.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 149, inclusive.
        /// 
        /// </summary>
        public int SetDlChannelTimingOffset(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlChannelTimingOffset, channel, value);
        }
        /// <summary>
        /// Specifies the timing offset for the downlink channel.    The toolkit multiplies the value you specify by 256. Therefore, the timing offset is always specified in multiples of 256 chips.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 149, inclusive.
        /// 
        /// </summary>
        public int GetDlChannelTimingOffset(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlChannelTimingOffset, channel, out value);
        }

        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the required payload length, the toolkit uses a subset of the required length    from the beginning of the array for waveform generation. If the array length is less than the required payload length,    the user-defined bit pattern is repeated until the required length is achieved.    If the NIWCDMASG_DL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_DL_CHANNEL_USER_DEFINED_BITS attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int SetDlChannelUserDefinedBits(string channel, int[] value)
        {
            return SetIntArray(niWCDMASGProperties.DlChannelUserDefinedBits, channel, value);
        }
        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the required payload length, the toolkit uses a subset of the required length    from the beginning of the array for waveform generation. If the array length is less than the required payload length,    the user-defined bit pattern is repeated until the required length is achieved.    If the NIWCDMASG_DL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_DL_CHANNEL_USER_DEFINED_BITS attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int GetDlChannelUserDefinedBits(string channel, int[] value, out int actualNumberOfPoints)
        {
            return GetIntArray(niWCDMASGProperties.DlChannelUserDefinedBits, channel, value, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int SetDlDpcchDataType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlDpcchDataType, channel, value);
        }
        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int GetDlDpcchDataType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlDpcchDataType, channel, out value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_DL_DPCCH_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_DPCCH_PN_ORDER attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 9.
        /// 
        /// </summary>
        public int SetDlDpcchPnOrder(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlDpcchPnOrder, channel, value);
        }
        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_DL_DPCCH_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_DPCCH_PN_ORDER attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 9.
        /// 
        /// </summary>
        public int GetDlDpcchPnOrder(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlDpcchPnOrder, channel, out value);
        }

        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_DL_DPCCH_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_DPCCH_PN_SEED attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int SetDlDpcchPnSeed(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlDpcchPnSeed, channel, value);
        }
        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_DL_DPCCH_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS, the toolkit ignores the NIWCDMASG_DL_DPCCH_PN_SEED attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int GetDlDpcchPnSeed(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlDpcchPnSeed, channel, out value);
        }

        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the required    length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern is repeated until the required length is achieved.    If the NIWCDMASG_DL_DPCCH_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE, the toolkit ignores the NIWCDMASG_DL_DPCCH_USER_DEFINED_BITS attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int SetDlDpcchUserDefinedBits(string channel, int[] value)
        {
            return SetIntArray(niWCDMASGProperties.DlDpcchUserDefinedBits, channel, value);
        }
        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the required    length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern is repeated until the required length is achieved.    If the NIWCDMASG_DL_DPCCH_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE, the toolkit ignores the NIWCDMASG_DL_DPCCH_USER_DEFINED_BITS attribute.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int GetDlDpcchUserDefinedBits(string channel, int[] value, out int actualNumberOfPoints)
        {
            return GetIntArray(niWCDMASGProperties.DlDpcchUserDefinedBits, channel, value, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the slot format used for downlink dedicated physical channel (DPCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown    in Figure 9 and Table 11 in section 5.3.2 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DL_DPCH_SLOT_FORMAT_0_SF_512.
        /// 
        /// </summary>
        public int SetDlDpchSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlDpchSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for downlink dedicated physical channel (DPCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown    in Figure 9 and Table 11 in section 5.3.2 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DL_DPCH_SLOT_FORMAT_0_SF_512.
        /// 
        /// </summary>
        public int GetDlDpchSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlDpchSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the hybrid ARQ acknowledgement indicator status.    The toolkit multiplies this value with the signature sequences.    Refer to the NIWCDMASG_DL_EHICH_SIGNATURE attribute for more information about signature sequences.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 1. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int SetDlEhichAckIndicator(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlEhichAckIndicator, channel, value);
        }
        /// <summary>
        /// Specifies the hybrid ARQ acknowledgement indicator status.    The toolkit multiplies this value with the signature sequences.    Refer to the NIWCDMASG_DL_EHICH_SIGNATURE attribute for more information about signature sequences.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 1. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int GetDlEhichAckIndicator(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlEhichAckIndicator, channel, out value);
        }

        /// <summary>
        /// Specifies the orthogonal signature sequence index used for the enhanced dedicated channel (E-DCH) hybrid ARQ indicator channel (EHICH).    Refer to section 5.3.2.4 of the 3GPP 25.211 Specifications 8.4.0 for the signature sequences and the signature hopping pattern.    In each slot, 40 bits are transmitted. The slot number and sequence index determine the signature sequence.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 39, inclusive.
        /// 
        /// </summary>
        public int SetDlEhichSignature(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlEhichSignature, channel, value);
        }
        /// <summary>
        /// Specifies the orthogonal signature sequence index used for the enhanced dedicated channel (E-DCH) hybrid ARQ indicator channel (EHICH).    Refer to section 5.3.2.4 of the 3GPP 25.211 Specifications 8.4.0 for the signature sequences and the signature hopping pattern.    In each slot, 40 bits are transmitted. The slot number and sequence index determine the signature sequence.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 39, inclusive.
        /// 
        /// </summary>
        public int GetDlEhichSignature(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlEhichSignature, channel, out value);
        }

        /// <summary>
        /// Specifies the relative grant status for the ERGCH channel.    The toolkit multiplies this relative grant status with the signature sequence.    Refer to the NIWCDMASG_DL_ERGCH_SIGNATURE attribute for more information about signature sequences.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 1. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int SetDlErgchRelativeGrant(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlErgchRelativeGrant, channel, value);
        }
        /// <summary>
        /// Specifies the relative grant status for the ERGCH channel.    The toolkit multiplies this relative grant status with the signature sequence.    Refer to the NIWCDMASG_DL_ERGCH_SIGNATURE attribute for more information about signature sequences.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 1. Valid values are -1 to 1, inclusive.
        /// 
        /// </summary>
        public int GetDlErgchRelativeGrant(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlErgchRelativeGrant, channel, out value);
        }

        /// <summary>
        /// Specifies the orthogonal signature sequence index used for the enhanced dedicated channel (E-DCH) relative grant channel (ERGCH).    Refer to section 5.3.2.4 of the 3GPP 25.211 Specifications 8.4.0 for the signature sequences and the signature hopping pattern.    In each slot, 40 bits are transmitted. The slot number and sequence index determine the signature sequence.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 39, inclusive.
        /// 
        /// </summary>
        public int SetDlErgchSignature(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlErgchSignature, channel, value);
        }
        /// <summary>
        /// Specifies the orthogonal signature sequence index used for the enhanced dedicated channel (E-DCH) relative grant channel (ERGCH).    Refer to section 5.3.2.4 of the 3GPP 25.211 Specifications 8.4.0 for the signature sequences and the signature hopping pattern.    In each slot, 40 bits are transmitted. The slot number and sequence index determine the signature sequence.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 39, inclusive.
        /// 
        /// </summary>
        public int GetDlErgchSignature(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlErgchSignature, channel, out value);
        }

        /// <summary>
        /// Specifies the slot format used for downlink fractional dedicated physical channel (FDPCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as    shown in Figure 12B and Table 16c in section 5.3.2.6 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_FDPCH_SLOT_FORMAT_0_SF_256.
        /// 
        /// </summary>
        public int SetDlFdpchSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlFdpchSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for downlink fractional dedicated physical channel (FDPCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as    shown in Figure 12B and Table 16c in section 5.3.2.6 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_FDPCH_SLOT_FORMAT_0_SF_256.
        /// 
        /// </summary>
        public int GetDlFdpchSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlFdpchSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the slot format used for downlink high speed physical downlink shared channel (HSPDSCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown    in Figure 26B and Table 26 in section 5.3.3.13 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_HSPDSCH_SLOT_FORMAT_0_SF_16_QPSK.
        /// 
        /// </summary>
        public int SetDlHspdschSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlHspdschSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for downlink high speed physical downlink shared channel (HSPDSCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown    in Figure 26B and Table 26 in section 5.3.3.13 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_HSPDSCH_SLOT_FORMAT_0_SF_16_QPSK.
        /// 
        /// </summary>
        public int GetDlHspdschSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlHspdschSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the number of indicators per frame for the paging indicator channel (PICH).    One PICH radio frame consists of 300 bits, of which 288 bits are used for carrying the paging indicators    and the remaining 12 bits are not transmitted. The number of paging indicators per frame determines the payload size,    and each payload bit is repeated (288/PICH indicators per frame) times.    Refer to section 5.3.310 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 18. Valid values are 18, 36, 72, and 144.
        /// 
        /// </summary>
        public int SetDlNumberOfPichIndicatorsPerFrame(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlNumberOfPichIndicatorsPerFrame, channel, value);
        }
        /// <summary>
        /// Specifies the number of indicators per frame for the paging indicator channel (PICH).    One PICH radio frame consists of 300 bits, of which 288 bits are used for carrying the paging indicators    and the remaining 12 bits are not transmitted. The number of paging indicators per frame determines the payload size,    and each payload bit is repeated (288/PICH indicators per frame) times.    Refer to section 5.3.310 of the 3GPP TS 25.211 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 18. Valid values are 18, 36, 72, and 144.
        /// 
        /// </summary>
        public int GetDlNumberOfPichIndicatorsPerFrame(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlNumberOfPichIndicatorsPerFrame, channel, out value);
        }

        /// <summary>
        /// Specifies the type of the downlink channel. Refer to section 5.3 of the 3GPP TS 25.211 Specifications 8.4.0 for information about the downlink channels.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DL_PHYSICAL_CHANNEL_TYPE_DPCH.
        /// 
        /// </summary>
        public int SetDlPhysicalChannelType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlPhysicalChannelType, channel, value);
        }
        /// <summary>
        /// Specifies the type of the downlink channel. Refer to section 5.3 of the 3GPP TS 25.211 Specifications 8.4.0 for information about the downlink channels.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_DL_PHYSICAL_CHANNEL_TYPE_DPCH.
        /// 
        /// </summary>
        public int GetDlPhysicalChannelType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlPhysicalChannelType, channel, out value);
        }

        /// <summary>
        /// Specifies the primary scrambling code used for creating the complex scrambling sequence. Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0. Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int SetDlPrimaryScramblingCode(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlPrimaryScramblingCode, channel, value);
        }
        /// <summary>
        /// Specifies the primary scrambling code used for creating the complex scrambling sequence. Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0. Valid values are 0 to 511, inclusive.
        /// 
        /// </summary>
        public int GetDlPrimaryScramblingCode(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlPrimaryScramblingCode, channel, out value);
        }

        /// <summary>
        /// Specifies the slot format used for downlink secondary common control physical channel (SCCPCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown    in Figure 17 and Table 18 in section 5.3.3.4 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_SCCPCH_SLOT_FORMAT_0_SF_256.
        /// 
        /// </summary>
        public int SetDlSccpchSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlSccpchSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for downlink secondary common control physical channel (SCCPCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown    in Figure 17 and Table 18 in section 5.3.3.4 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_SCCPCH_SLOT_FORMAT_0_SF_256.
        /// 
        /// </summary>
        public int GetDlSccpchSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlSccpchSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the secondary scramble code used for creating the complex scrambling sequence. Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int SetDlScramblingCodeOffset(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlScramblingCodeOffset, channel, value);
        }
        /// <summary>
        /// Specifies the secondary scramble code used for creating the complex scrambling sequence. Refer to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int GetDlScramblingCodeOffset(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlScramblingCodeOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the downlink scrambling code type used for creating the complex scrambling sequence. The sequence generated conforms to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0.
        ///    The default value is NIWCDMASG_VAL_DL_SCRAMBLING_CODE_TYPE_STANDARD.
        /// 
        /// </summary>
        public int SetDlScramblingCodeType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlScramblingCodeType, channel, value);
        }
        /// <summary>
        /// Specifies the downlink scrambling code type used for creating the complex scrambling sequence. The sequence generated conforms to section 5.2 of the 3GPP TS 25.213 Specifications 8.4.0.
        ///    The default value is NIWCDMASG_VAL_DL_SCRAMBLING_CODE_TYPE_STANDARD.
        /// 
        /// </summary>
        public int GetDlScramblingCodeType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlScramblingCodeType, channel, out value);
        }

        /// <summary>
        /// Specifies the scrambling code group for S-SCH. There are 64 different scrambling code groups.    Refer to section 5.2.3.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0. Valid values are 0 to 63, inclusive.
        /// 
        /// </summary>
        public int SetDlSschCodeGroup(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DlSschCodeGroup, channel, value);
        }
        /// <summary>
        /// Specifies the scrambling code group for S-SCH. There are 64 different scrambling code groups.    Refer to section 5.2.3.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0. Valid values are 0 to 63, inclusive.
        /// 
        /// </summary>
        public int GetDlSschCodeGroup(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DlSschCodeGroup, channel, out value);
        }

        /// <summary>
        /// Specifies the duplexing technique used in the generated frame.
        ///    The default value is NIWCDMASG_VAL_DUPLEX_MODE_DL_ONLY_FDD.
        /// 
        /// </summary>
        public int SetDuplexMode(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.DuplexMode, channel, value);
        }
        /// <summary>
        /// Specifies the duplexing technique used in the generated frame.
        ///    The default value is NIWCDMASG_VAL_DUPLEX_MODE_DL_ONLY_FDD.
        /// 
        /// </summary>
        public int GetDuplexMode(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.DuplexMode, channel, out value);
        }

        /// <summary>
        /// Specifies the value, in dB, for headroom.    The toolkit uses this attribute for scaling the waveform when the NIWCDMASG_AUTO_HEADROOM_ENABLED attribute is set to NIWCDMASG_VAL_FALSE.
        ///    The default value is 0. Valid values are 0 to 30, inclusive.
        /// 
        /// </summary>
        public int SetHeadroom(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.Headroom, channel, value);
        }
        /// <summary>
        /// Specifies the value, in dB, for headroom.    The toolkit uses this attribute for scaling the waveform when the NIWCDMASG_AUTO_HEADROOM_ENABLED attribute is set to NIWCDMASG_VAL_FALSE.
        ///    The default value is 0. Valid values are 0 to 30, inclusive.
        /// 
        /// </summary>
        public int GetHeadroom(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.Headroom, channel, out value);
        }

        /// <summary>
        /// Specifies the value of the DC offset in the in-phase (I) signal as a percentage of the root mean square (RMS) magnitude of the unaltered I signal.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetIdcOffset(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.IDcOffset, channel, value);
        }
        /// <summary>
        /// Specifies the value of the DC offset in the in-phase (I) signal as a percentage of the root mean square (RMS) magnitude of the unaltered I signal.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetIdcOffset(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.IDcOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the ratio, in dB, of the mean amplitude of the in-phase (I) signal to the mean amplitude of the quadrature-phase (Q) signal.
        ///    The default value is 0. Valid values are -6 to 6, inclusive.
        ///    Refer to the IQ Gain Imbalance topic for more information.
        /// 
        /// </summary>
        public int SetIqGainImbalance(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.IqGainImbalance, channel, value);
        }
        /// <summary>
        /// Specifies the ratio, in dB, of the mean amplitude of the in-phase (I) signal to the mean amplitude of the quadrature-phase (Q) signal.
        ///    The default value is 0. Valid values are -6 to 6, inclusive.
        ///    Refer to the IQ Gain Imbalance topic for more information.
        /// 
        /// </summary>
        public int GetIqGainImbalance(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.IqGainImbalance, channel, out value);
        }

        /// <summary>
        /// Returns the sample rate, in hertz (Hz), for generation.
        ///    The chip rate is 3.84 megachips per second. The sample rate of the generated signal equals the chip rate multiplied by the oversampling factor.
        /// 
        /// </summary>
        public int GetIqRate(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.IqRate, channel, out value);
        }

        /// <summary>
        /// Specifies the number of downlink channels used for creating the waveform.
        ///    The default value is 5. Valid values are 1 to 80, inclusive.
        /// 
        /// </summary>
        public int SetNumberOfPhysicalDlChannels(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.NumberOfPhysicalDlChannels, channel, value);
        }
        /// <summary>
        /// Specifies the number of downlink channels used for creating the waveform.
        ///    The default value is 5. Valid values are 1 to 80, inclusive.
        /// 
        /// </summary>
        public int GetNumberOfPhysicalDlChannels(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.NumberOfPhysicalDlChannels, channel, out value);
        }

        /// <summary>
        /// Specifies the number of uplink channels used for creating the waveform.
        ///    The default value is 2. Valid values are 0 to 20, inclusive.
        /// 
        /// </summary>
        public int SetNumberOfPhysicalUlChannels(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.NumberOfPhysicalUlChannels, channel, value);
        }
        /// <summary>
        /// Specifies the number of uplink channels used for creating the waveform.
        ///    The default value is 2. Valid values are 0 to 20, inclusive.
        /// 
        /// </summary>
        public int GetNumberOfPhysicalUlChannels(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.NumberOfPhysicalUlChannels, channel, out value);
        }

        /// <summary>
        /// Specifies the value of the DC offset in the quadrature-phase (Q) signal as percentage of the root mean square (RMS) magnitude of the unaltered Q signal.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int SetQdcOffset(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.QDcOffset, channel, value);
        }
        /// <summary>
        /// Specifies the value of the DC offset in the quadrature-phase (Q) signal as percentage of the root mean square (RMS) magnitude of the unaltered Q signal.
        ///    The default value is 0.
        /// 
        /// </summary>
        public int GetQdcOffset(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.QDcOffset, channel, out value);
        }

        /// <summary>
        /// Specifies the deviation in angle from 90 degrees between the in-phase (I) and quadrature-phase (Q) signals.
        ///    The default value is 0.
        ///    Refer Quadrature Skew topic for more information.
        /// 
        /// </summary>
        public int SetQuadratureSkew(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.QuadratureSkew, channel, value);
        }
        /// <summary>
        /// Specifies the deviation in angle from 90 degrees between the in-phase (I) and quadrature-phase (Q) signals.
        ///    The default value is 0.
        ///    Refer Quadrature Skew topic for more information.
        /// 
        /// </summary>
        public int GetQuadratureSkew(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.QuadratureSkew, channel, out value);
        }

        /// <summary>
        /// Indicates the WCDMA Generation Toolkit version in use.
        /// 
        /// </summary>
        public int GetToolkitCompatibilityVersion(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.ToolkitCompatibilityVersion, channel, out value);
        }

        /// <summary>
        /// Specifies whether to use the I or Q branch for the uplink channel.    For DPCCH and EDPCCH the branch type must be Q and I respectively.    The toolkit gives an error if user configures otherwise.    The uplink dedicated physical channels are I/Q code multiplexed.    Refer section 4.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_BRANCH_I. 
        /// 
        /// </summary>
        public int SetUlChannelBranch(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlChannelBranch, channel, value);
        }
        /// <summary>
        /// Specifies whether to use the I or Q branch for the uplink channel.    For DPCCH and EDPCCH the branch type must be Q and I respectively.    The toolkit gives an error if user configures otherwise.    The uplink dedicated physical channels are I/Q code multiplexed.    Refer section 4.2 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_BRANCH_I. 
        /// 
        /// </summary>
        public int GetUlChannelBranch(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlChannelBranch, channel, out value);
        }

        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int SetUlChannelDataType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlChannelDataType, channel, value);
        }
        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int GetUlChannelDataType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlChannelDataType, channel, out value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_UL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_CHANNEL_PN_ORDER attribute.
        ///    The default value is 9. Valid values are 5 to 31, inclusive.
        /// 
        /// </summary>
        public int SetUlChannelPnOrder(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlChannelPnOrder, channel, value);
        }
        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_UL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_CHANNEL_PN_ORDER attribute.
        ///    The default value is 9. Valid values are 5 to 31, inclusive.
        /// 
        /// </summary>
        public int GetUlChannelPnOrder(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlChannelPnOrder, channel, out value);
        }

        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_UL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_CHANNEL_PN_SEED attribute.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int SetUlChannelPnSeed(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlChannelPnSeed, channel, value);
        }
        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_UL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_CHANNEL_PN_SEED attribute.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int GetUlChannelPnSeed(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlChannelPnSeed, channel, out value);
        }

        /// <summary>
        /// Specifies the power of the uplink channel relative to the carrier power, in dB.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    If you do not configure this attribute for a channel, the toolkit chooses the default value based on the total number of    channels and the channels for which you have configured this attribute.
        ///    Refer to Relative Power topic for more information.
        ///    Valid values are -60 to 0, inclusive.
        ///    Note: The sum of all the relative channel powers must be 0 dB. The relative channel powers are equally divided for the non-configured channels.
        /// 
        /// </summary>
        public int SetUlChannelRelativePower(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.UlChannelRelativePower, channel, value);
        }
        /// <summary>
        /// Specifies the power of the uplink channel relative to the carrier power, in dB.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    If you do not configure this attribute for a channel, the toolkit chooses the default value based on the total number of    channels and the channels for which you have configured this attribute.
        ///    Refer to Relative Power topic for more information.
        ///    Valid values are -60 to 0, inclusive.
        ///    Note: The sum of all the relative channel powers must be 0 dB. The relative channel powers are equally divided for the non-configured channels.
        /// 
        /// </summary>
        public int GetUlChannelRelativePower(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.UlChannelRelativePower, channel, out value);
        }

        /// <summary>
        /// Specifies the spreading code number for the uplink channel.    The spreading code number denotes a particular orthogonal variable spread factor (OVSF) code of a given spread factor.    If the spreading code of the channel conflicts with the any of the other channels, the toolkit returns an error.    Refer to section 4.2 of the 3GPP TS 25.213 Specifications 8.4.0 for the spreading operation and section 4.3 of the specification for OVSF code generation.
        ///    The default value is 0. Valid values are 0 to 255, inclusive.
        /// 
        /// </summary>
        public int SetUlChannelSpreadingCode(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlChannelSpreadingCode, channel, value);
        }
        /// <summary>
        /// Specifies the spreading code number for the uplink channel.    The spreading code number denotes a particular orthogonal variable spread factor (OVSF) code of a given spread factor.    If the spreading code of the channel conflicts with the any of the other channels, the toolkit returns an error.    Refer to section 4.2 of the 3GPP TS 25.213 Specifications 8.4.0 for the spreading operation and section 4.3 of the specification for OVSF code generation.
        ///    The default value is 0. Valid values are 0 to 255, inclusive.
        /// 
        /// </summary>
        public int GetUlChannelSpreadingCode(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlChannelSpreadingCode, channel, out value);
        }

        /// <summary>
        /// Specifies the timing offset for the uplink channel.    The toolkit multiplies the value you specify by 256.    Therefore, the timing offset is always specified in multiples of 256 chips.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 149, inclusive.
        /// 
        /// </summary>
        public int SetUlChannelTimingOffset(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlChannelTimingOffset, channel, value);
        }
        /// <summary>
        /// Specifies the timing offset for the uplink channel.    The toolkit multiplies the value you specify by 256.    Therefore, the timing offset is always specified in multiples of 256 chips.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is 0. Valid values are 0 to 149, inclusive.
        /// 
        /// </summary>
        public int GetUlChannelTimingOffset(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlChannelTimingOffset, channel, out value);
        }

        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the required    length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern is    repeated until the required length is achieved.    If the NIWCDMASG_UL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_UL_CHANNEL_USER_DEFINED_BITS attribute.
        ///    Use an active channel string to configure this attribute.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int SetUlChannelUserDefinedBits(string channel, int[] value)
        {
            return SetIntArray(niWCDMASGProperties.UlChannelUserDefinedBits, channel, value);
        }
        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the required    length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern is    repeated until the required length is achieved.    If the NIWCDMASG_UL_CHANNEL_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_UL_CHANNEL_USER_DEFINED_BITS attribute.
        ///    Use an active channel string to configure this attribute.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int GetUlChannelUserDefinedBits(string channel, int[] value, out int actualNumberOfPoints)
        {
            return GetIntArray(niWCDMASGProperties.UlChannelUserDefinedBits, channel, value, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the slot format used for uplink dedicated physical control channel (DPCCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown in    Figure 1 and Table 2 in section 5.2.2.1 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_DPCCH_SLOT_FORMAT_0_SF_256.
        /// 
        /// </summary>
        public int SetUlDpcchSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlDpcchSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for uplink dedicated physical control channel (DPCCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown in    Figure 1 and Table 2 in section 5.2.2.1 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_DPCCH_SLOT_FORMAT_0_SF_256.
        /// 
        /// </summary>
        public int GetUlDpcchSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlDpcchSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the slot format used for uplink dedicated physical data channel (DPDCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown in    Figure 1 and Table 1 in section 5.2.2.1 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_DPDCH_SLOT_FORMAT_0_SF_256. 
        /// 
        /// </summary>
        public int SetUlDpdchSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlDpdchSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for uplink dedicated physical data channel (DPDCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown in    Figure 1 and Table 1 in section 5.2.2.1 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_DPDCH_SLOT_FORMAT_0_SF_256. 
        /// 
        /// </summary>
        public int GetUlDpdchSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlDpdchSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the slot format used for uplink E-DCH dedicated physical data channel (EDPDCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown in    figure 2B and table 5B in section 5.2.1.3 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_EDPDCH_SLOT_FORMAT_0_SF_256. 
        /// 
        /// </summary>
        public int SetUlEdpdchSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlEdpdchSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for uplink E-DCH dedicated physical data channel (EDPDCH).    The slot format determines the channel spreading code, spreading factor, bitrate, and frame structure as shown in    figure 2B and table 5B in section 5.2.1.3 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_EDPDCH_SLOT_FORMAT_0_SF_256. 
        /// 
        /// </summary>
        public int GetUlEdpdchSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlEdpdchSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies the type of uplink frame.
        ///    The default value is NIWCDMASG_VAL_UL_FRAME_TYPE_NON_PRACH.
        /// 
        /// </summary>
        public int SetUlFrameType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlFrameType, channel, value);
        }
        /// <summary>
        /// Specifies the type of uplink frame.
        ///    The default value is NIWCDMASG_VAL_UL_FRAME_TYPE_NON_PRACH.
        /// 
        /// </summary>
        public int GetUlFrameType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlFrameType, channel, out value);
        }

        /// <summary>
        /// Specifies the type of the uplink channel. Refer to section 5.2 of the 3GPP TS 25.211 Specifications 8.4.0 for more information about uplink channels.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_PHYSICAL_CHANNEL_TYPE_DPDCH.
        /// 
        /// </summary>
        public int SetUlPhysicalChannelType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPhysicalChannelType, channel, value);
        }
        /// <summary>
        /// Specifies the type of the uplink channel. Refer to section 5.2 of the 3GPP TS 25.211 Specifications 8.4.0 for more information about uplink channels.
        ///    Use an active channel string to configure this attribute. Refer to the Active Channels topic for more information.
        ///    The default value is NIWCDMASG_VAL_UL_PHYSICAL_CHANNEL_TYPE_DPDCH.
        /// 
        /// </summary>
        public int GetUlPhysicalChannelType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPhysicalChannelType, channel, out value);
        }

        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int SetUlPrachControlMessageDataType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachControlMessageDataType, channel, value);
        }
        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int GetUlPrachControlMessageDataType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachControlMessageDataType, channel, out value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_PN_ORDER attribute.
        ///    The default value is 9.
        /// 
        /// </summary>
        public int SetUlPrachControlMessagePnOrder(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachControlMessagePnOrder, channel, value);
        }
        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_PN_ORDER attribute.
        ///    The default value is 9.
        /// 
        /// </summary>
        public int GetUlPrachControlMessagePnOrder(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachControlMessagePnOrder, channel, out value);
        }

        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_PN_SEED attribute.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int SetUlPrachControlMessagePnSeed(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachControlMessagePnSeed, channel, value);
        }
        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_PN_SEED attribute.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int GetUlPrachControlMessagePnSeed(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachControlMessagePnSeed, channel, out value);
        }

        /// <summary>
        /// Specifies the power of the  message control relative to the carrier power in dB.
        ///    The default value is 0. Valid values are -60 to 0.
        /// 
        /// </summary>
        public int SetUlPrachControlMessagePower(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.UlPrachControlMessagePower, channel, value);
        }
        /// <summary>
        /// Specifies the power of the  message control relative to the carrier power in dB.
        ///    The default value is 0. Valid values are -60 to 0.
        /// 
        /// </summary>
        public int GetUlPrachControlMessagePower(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.UlPrachControlMessagePower, channel, out value);
        }

        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the    required length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern    is repeated until the required length is achieved.    If the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_USER_DEFINED_BITS attribute.
        ///    The default is an empty array. Valid values include arrays of zeros and ones. 
        /// 
        /// </summary>
        public int SetUlPrachControlMessageUserDefinedBits(string channel, int[] value)
        {
            return SetIntArray(niWCDMASGProperties.UlPrachControlMessageUserDefinedBits, channel, value);
        }
        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the    required length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern    is repeated until the required length is achieved.    If the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_UL_PRACH_CONTROL_MESSAGE_USER_DEFINED_BITS attribute.
        ///    The default is an empty array. Valid values include arrays of zeros and ones. 
        /// 
        /// </summary>
        public int GetUlPrachControlMessageUserDefinedBits(string channel, int[] value, out int actualNumberOfPoints)
        {
            return GetIntArray(niWCDMASGProperties.UlPrachControlMessageUserDefinedBits, channel, value, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int SetUlPrachDataMessageDataType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachDataMessageDataType, channel, value);
        }
        /// <summary>
        /// Specifies the type of payload for the channel.
        ///    The default value is NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE.
        /// 
        /// </summary>
        public int GetUlPrachDataMessageDataType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachDataMessageDataType, channel, out value);
        }

        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_UL_PRACH_DATA_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_DATA_MESSAGE_PN_ORDER attribute.
        ///    The default value is 9. 
        /// 
        /// </summary>
        public int SetUlPrachDataMessagePnOrder(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachDataMessagePnOrder, channel, value);
        }
        /// <summary>
        /// Specifies the order (length of memory) of the pseudorandom bit sequence (PRBS) generator.    The generated sequence is repeated (2^PN order)-1 bits.    If the NIWCDMASG_UL_PRACH_DATA_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_DATA_MESSAGE_PN_ORDER attribute.
        ///    The default value is 9. 
        /// 
        /// </summary>
        public int GetUlPrachDataMessagePnOrder(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachDataMessagePnOrder, channel, out value);
        }

        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_UL_PRACH_DATA_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_DATA_MESSAGE_PN_SEED attribute.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int SetUlPrachDataMessagePnSeed(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachDataMessagePnSeed, channel, value);
        }
        /// <summary>
        /// Specifies the initialization seed used for the pseudorandom bit sequence (PRBS) generator.    If the NIWCDMASG_UL_PRACH_DATA_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_USER_DEFINED_BITS,    the toolkit ignores the NIWCDMASG_UL_PRACH_DATA_MESSAGE_PN_SEED attribute.
        ///    The default value is -692,093,454.
        /// 
        /// </summary>
        public int GetUlPrachDataMessagePnSeed(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachDataMessagePnSeed, channel, out value);
        }

        /// <summary>
        /// Specifies the power of the message data relative to the carrier power, in dB.
        ///    The default value is 0. Valid values are -60 to 0, inclusive.
        /// 
        /// </summary>
        public int SetUlPrachDataMessagePower(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.UlPrachDataMessagePower, channel, value);
        }
        /// <summary>
        /// Specifies the power of the message data relative to the carrier power, in dB.
        ///    The default value is 0. Valid values are -60 to 0, inclusive.
        /// 
        /// </summary>
        public int GetUlPrachDataMessagePower(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.UlPrachDataMessagePower, channel, out value);
        }

        /// <summary>
        /// Specifies the slot format used for the message part of the PRACH channel.    The slot format determines the channel spreading factor, bitrate, and frame structure as shown in Figure 5    and Table 6 in section 5.2.2.1.3 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    The default value is NIWCDMASG_VAL_PRACH_DATA_MESSAGE_SLOT_FORMAT_0_SF_256. 
        /// 
        /// </summary>
        public int SetUlPrachDataMessageSlotFormat(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachDataMessageSlotFormat, channel, value);
        }
        /// <summary>
        /// Specifies the slot format used for the message part of the PRACH channel.    The slot format determines the channel spreading factor, bitrate, and frame structure as shown in Figure 5    and Table 6 in section 5.2.2.1.3 of the 3GPP TS 25.211 Specifications 8.4.0.
        ///    The default value is NIWCDMASG_VAL_PRACH_DATA_MESSAGE_SLOT_FORMAT_0_SF_256. 
        /// 
        /// </summary>
        public int GetUlPrachDataMessageSlotFormat(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachDataMessageSlotFormat, channel, out value);
        }

        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the    required length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern    is repeated until the required length is achieved.    If the NIWCDMASG_UL_PRACH_DATA_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_UL_PRACH_DATA_MESSAGE_USER_DEFINED_BITS attribute.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int SetUlPrachDataMessageUserDefinedBits(string channel, int[] value)
        {
            return SetIntArray(niWCDMASGProperties.UlPrachDataMessageUserDefinedBits, channel, value);
        }
        /// <summary>
        /// Specifies a user-defined bit pattern as an array of zeros and ones.    If the array length is greater than the configured payload length, the toolkit uses a subset of the    required length from the beginning of the array for waveform generation.    If the array length is less than the configured payload length, the user-defined bit pattern    is repeated until the required length is achieved.    If the NIWCDMASG_UL_PRACH_DATA_MESSAGE_DATA_TYPE attribute is set to NIWCDMASG_VAL_DATA_TYPE_PN_SEQUENCE,    the toolkit ignores the NIWCDMASG_UL_PRACH_DATA_MESSAGE_USER_DEFINED_BITS attribute.
        ///    The default is an empty array. Valid values include arrays of zeros and ones.
        /// 
        /// </summary>
        public int GetUlPrachDataMessageUserDefinedBits(string channel, int[] value, out int actualNumberOfPoints)
        {
            return GetIntArray(niWCDMASGProperties.UlPrachDataMessageUserDefinedBits, channel, value, out actualNumberOfPoints);
        }

        /// <summary>
        /// Specifies the length of the message, in units of frames. One frame corresponds to 10 milliseconds.
        ///    The default value is 1. Valid values are 1 to 2, inclusive.
        /// 
        /// </summary>
        public int SetUlPrachMessageLength(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachMessageLength, channel, value);
        }
        /// <summary>
        /// Specifies the length of the message, in units of frames. One frame corresponds to 10 milliseconds.
        ///    The default value is 1. Valid values are 1 to 2, inclusive.
        /// 
        /// </summary>
        public int GetUlPrachMessageLength(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachMessageLength, channel, out value);
        }

        /// <summary>
        /// Specifies the number of idle slots in the PRACH waveform.
        ///    The default value is 2. Valid values are 2 to 10, inclusive.
        /// 
        /// </summary>
        public int SetUlPrachNumberOfIdleSlots(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachNumberOfIdleSlots, channel, value);
        }
        /// <summary>
        /// Specifies the number of idle slots in the PRACH waveform.
        ///    The default value is 2. Valid values are 2 to 10, inclusive.
        /// 
        /// </summary>
        public int GetUlPrachNumberOfIdleSlots(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachNumberOfIdleSlots, channel, out value);
        }

        /// <summary>
        /// Specifies the number of preambles in the PRACH waveform.
        ///    The default value is 4. Valid values are 1 to 4, inclusive.
        /// 
        /// </summary>
        public int SetUlPrachNumberOfPreambles(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachNumberOfPreambles, channel, value);
        }
        /// <summary>
        /// Specifies the number of preambles in the PRACH waveform.
        ///    The default value is 4. Valid values are 1 to 4, inclusive.
        /// 
        /// </summary>
        public int GetUlPrachNumberOfPreambles(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachNumberOfPreambles, channel, out value);
        }

        /// <summary>
        /// Specifies the power of the preamble relative to the carrier power, in dB.
        ///    The default value is 0. Valid values are -60 to 0, inclusive.
        /// 
        /// </summary>
        public int SetUlPrachPreamblePower(string channel, double value)
        {
            return SetDouble(niWCDMASGProperties.UlPrachPreamblePower, channel, value);
        }
        /// <summary>
        /// Specifies the power of the preamble relative to the carrier power, in dB.
        ///    The default value is 0. Valid values are -60 to 0, inclusive.
        /// 
        /// </summary>
        public int GetUlPrachPreamblePower(string channel, out double value)
        {
            return GetDouble(niWCDMASGProperties.UlPrachPreamblePower, channel, out value);
        }

        /// <summary>
        /// Specifies the signature for the preamble. Each preamble, which is of length 4,096 chips, consists of 256 repetitions of a signature of length 16 chips.    There are a maximum of 16 available signatures. Refer to section 4.3.3.3 of the 3GPP TS 25.213 Specifications 8.4.0 for    more information about PRACH preamble signatures.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int SetUlPrachPreambleSignature(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachPreambleSignature, channel, value);
        }
        /// <summary>
        /// Specifies the signature for the preamble. Each preamble, which is of length 4,096 chips, consists of 256 repetitions of a signature of length 16 chips.    There are a maximum of 16 available signatures. Refer to section 4.3.3.3 of the 3GPP TS 25.213 Specifications 8.4.0 for    more information about PRACH preamble signatures.
        ///    The default value is 0. Valid values are 0 to 15, inclusive.
        /// 
        /// </summary>
        public int GetUlPrachPreambleSignature(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachPreambleSignature, channel, out value);
        }

        /// <summary>
        /// Specifies the PRACH waveform type. The PRACH transmission consists of one or several preambles of length 4,096 chips and a message of length 10 ms or 20 ms.    Refer to section 5.2.2.1 of the 3GPP TS 25.211 Specifications 4.8.0 for more information.
        ///    The default value is NIWCDMASG_VAL_PRACH_WAVEFORM_TYPE_PREAMBLE_PLUS_IDLE.
        /// 
        /// </summary>
        public int SetUlPrachWaveformType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlPrachWaveformType, channel, value);
        }
        /// <summary>
        /// Specifies the PRACH waveform type. The PRACH transmission consists of one or several preambles of length 4,096 chips and a message of length 10 ms or 20 ms.    Refer to section 5.2.2.1 of the 3GPP TS 25.211 Specifications 4.8.0 for more information.
        ///    The default value is NIWCDMASG_VAL_PRACH_WAVEFORM_TYPE_PREAMBLE_PLUS_IDLE.
        /// 
        /// </summary>
        public int GetUlPrachWaveformType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlPrachWaveformType, channel, out value);
        }

        /// <summary>
        /// Specifies the uplink scrambling code number. Refer to section 4.3.2.5 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0.    If the NIWCDMASG_UL_FRAME_TYPE attribute is set to NIWCDMASG_VAL_UL_FRAME_TYPE_PRACH, valid values are 0 to 8,191 (inclusive).    If the NIWCDMASG_UL_FRAME_TYPE attribute is set to NIWCDMASG_VAL_UL_FRAME_TYPE_NON_PRACH, valid values are 0 to 16,777,215 (inclusive).
        /// 
        /// </summary>
        public int SetUlScramblingCode(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlScramblingCode, channel, value);
        }
        /// <summary>
        /// Specifies the uplink scrambling code number. Refer to section 4.3.2.5 of the 3GPP TS 25.213 Specifications 8.4.0 for more information.
        ///    The default value is 0.    If the NIWCDMASG_UL_FRAME_TYPE attribute is set to NIWCDMASG_VAL_UL_FRAME_TYPE_PRACH, valid values are 0 to 8,191 (inclusive).    If the NIWCDMASG_UL_FRAME_TYPE attribute is set to NIWCDMASG_VAL_UL_FRAME_TYPE_NON_PRACH, valid values are 0 to 16,777,215 (inclusive).
        /// 
        /// </summary>
        public int GetUlScramblingCode(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlScramblingCode, channel, out value);
        }

        /// <summary>
        /// Specifies the uplink scrambling code type used for creating the complex scrambling sequence.    If the NIWCDMASG_UL_FRAME_TYPE attribute is set to NIWCDMASG_VAL_UL_FRAME_TYPE_PRACH, the scrambling code type    must be long according to section 4.3.2.5 of the 3GPP TS 25.213 Specifications 8.4.0.
        ///    The default value is NIWCDMASG_VAL_UL_SCRAMBLING_CODE_TYPE_LONG.
        /// 
        /// </summary>
        public int SetUlScramblingCodeType(string channel, int value)
        {
            return SetInt32(niWCDMASGProperties.UlScramblingCodeType, channel, value);
        }
        /// <summary>
        /// Specifies the uplink scrambling code type used for creating the complex scrambling sequence.    If the NIWCDMASG_UL_FRAME_TYPE attribute is set to NIWCDMASG_VAL_UL_FRAME_TYPE_PRACH, the scrambling code type    must be long according to section 4.3.2.5 of the 3GPP TS 25.213 Specifications 8.4.0.
        ///    The default value is NIWCDMASG_VAL_UL_SCRAMBLING_CODE_TYPE_LONG.
        /// 
        /// </summary>
        public int GetUlScramblingCodeType(string channel, out int value)
        {
            return GetInt32(niWCDMASGProperties.UlScramblingCodeType, channel, out value);
        }

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
                    PInvoke.niWCDMASG_CloseSession(Handle);
                }
            }
        }

        private int SetInt32(niWCDMASGProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            return TestForError(PInvoke.niWCDMASG_SetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetInt32(niWCDMASGProperties propertyId, string repeatedCapabilityOrChannel, out int val)
        {
            return TestForError(PInvoke.niWCDMASG_GetScalarAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetDouble(niWCDMASGProperties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            return TestForError(PInvoke.niWCDMASG_SetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        private int GetDouble(niWCDMASGProperties propertyId, string repeatedCapabilityOrChannel, out double val)
        {
            return TestForError(PInvoke.niWCDMASG_GetScalarAttributeF64(Handle, repeatedCapabilityOrChannel, propertyId, out val));
        }

        private int SetIntArray(niWCDMASGProperties propertyId, string repeatedCapabilityOrChannel, int[] val)
        {
            return TestForError(PInvoke.niWCDMASG_SetVectorAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, val, val.Length));
        }

        private int GetIntArray(niWCDMASGProperties propertyId, string repeatedCapabilityOrChannel, int[] val, out int actualNumberOfPoints)
        {
            return TestForError(PInvoke.niWCDMASG_GetVectorAttributeI32(Handle, repeatedCapabilityOrChannel, propertyId, val, val.Length, out actualNumberOfPoints));
        }

        private class PInvoke
        {
            const string nativeDllName = "niWCDMASG_net.dll";


            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_CloseSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_ConfigureDownlinkTestModel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_ConfigureDownlinkTestModel(HandleRef session, string channelString, int testModel);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_CreateWaveformComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_CreateWaveformComplexF64(HandleRef session, int reserved1, out double t0, out double dt, [In, Out] niComplexNumber[] waveform, int waveformSize, out int actualNumWaveformSamples, int reserved2);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_GetErrorString(HandleRef session, int errorCode, StringBuilder errorMessage, int errorMessageLength);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_GetScalarAttributeF64(HandleRef session, string channelString, niWCDMASGProperties attributeID, out double value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_GetScalarAttributeI32(HandleRef session, string channelString, niWCDMASGProperties attributeID, out int value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_GetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_GetVectorAttributeF64(HandleRef session, string channelString, niWCDMASGProperties attributeID, [In, Out] double[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_GetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_GetVectorAttributeI32(HandleRef session, string channelString, niWCDMASGProperties attributeID, [In, Out] int[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_OpenSession(string sessionName, int toolkitCompatibilityVersion, out IntPtr session, out int isNewSession);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_ResetAttribute(HandleRef session, string channelString, niWCDMASGProperties attributeID);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_ResetSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGCreateAndDownloadWaveform_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGCreateAndDownloadWaveform_v1(System.Runtime.InteropServices.HandleRef rfsgSession, System.Runtime.InteropServices.HandleRef wcdmasgSession, string waveformName);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGConfigurePowerLevel_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGConfigurePowerLevel_v1(System.Runtime.InteropServices.HandleRef rfsgSession, string channelString, string script, double powerLevel);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGConfigureScript_v1", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGConfigureScript_v1(System.Runtime.InteropServices.HandleRef rfsgSession, string channelString, string script, double powerLevel);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGClearDatabase", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGClearDatabase(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string waveform);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGRetrieveHeadroom", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGRetrieveHeadroom(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string waveformName, out double headroom);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGRetrieveIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGRetrieveIQRate(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string waveformName, out double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGRetrieveIQRateAllWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGRetrieveIQRateAllWaveforms(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string script, out double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGRetrieveMinimumHeadroomAllWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGRetrieveMinimumHeadroomAllWaveforms(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string script, out double headroom);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGStoreHeadroom", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGStoreHeadroom(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string waveformName, double headroom);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_RFSGStoreIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_RFSGStoreIQRate(System.Runtime.InteropServices.HandleRef rFSGHandle, string channelString, string waveformName, double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_SetScalarAttributeF64(HandleRef session, string channelString, niWCDMASGProperties attributeID, double value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_SetScalarAttributeI32(HandleRef session, string channelString, niWCDMASGProperties attributeID, int value);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_SetVectorAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_SetVectorAttributeF64(HandleRef session, string channelString, niWCDMASGProperties attributeID, double[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niWCDMASG_SetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niWCDMASG_SetVectorAttributeI32(HandleRef session, string channelString, niWCDMASGProperties attributeID, int[] dataArray, int dataArraySize);


        }
        private int GetErrorString(int status, StringBuilder msg)
        {
            int size = PInvoke.niWCDMASG_GetErrorString(Handle, status, null, 0);
            if ((size >= 0))
            {
                msg.Capacity = size;
                PInvoke.niWCDMASG_GetErrorString(Handle, status, msg, size);
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

        private int TestForError(int status, HandleRef rfsgHandle)
        {
            if ((status < 0))
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                GetErrorString(status, msg);
                int tempStatus = status;
                //get rfsg detailed error message
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSG.GetError(rfsgHandle, tempStatus, msg);
                //get rfsg general error message
                if (String.IsNullOrEmpty(msg.ToString()))
                    niRFSG.ErrorMessage(rfsgHandle, status, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }
    }

    public class niWCDMASGConstants
    {

        public const int DuplexModeDlOnlyFdd = 0;

        public const int DuplexModeUlOnlyFdd = 1;

        public const int False = 0;

        public const int True = 1;

        public const int DlScramblingCodeTypeStandard = 0;

        public const int DlScramblingCodeTypeLeft = 1;

        public const int DlScramblingCodeTypeRight = 2;

        public const int DlPhysicalChannelTypePsch = 0;

        public const int DlPhysicalChannelTypeSsch = 1;

        public const int DlPhysicalChannelTypeAich = 2;

        public const int DlPhysicalChannelTypeCpich = 3;

        public const int DlPhysicalChannelTypeDpch = 4;

        public const int DlPhysicalChannelTypeFdpch = 5;

        public const int DlPhysicalChannelTypePccpch = 6;

        public const int DlPhysicalChannelTypePich = 7;

        public const int DlPhysicalChannelTypeSccpch = 8;

        public const int DlPhysicalChannelTypeEagch = 9;

        public const int DlPhysicalChannelTypeEhich = 10;

        public const int DlPhysicalChannelTypeErgch = 11;

        public const int DlPhysicalChannelTypeHspdsch = 12;

        public const int DlPhysicalChannelTypeHsscch = 13;

        public const int DataTypePnSequence = 0;

        public const int DataTypeUserDefinedBits = 1;

        public const int DlDpchSlotFormat0Sf512 = 0;

        public const int DlDpchSlotFormat1Sf512 = 3;

        public const int DlDpchSlotFormat2Sf256 = 5;

        public const int DlDpchSlotFormat3Sf256 = 8;

        public const int DlDpchSlotFormat4Sf256 = 11;

        public const int DlDpchSlotFormat5Sf256 = 14;

        public const int DlDpchSlotFormat6Sf256 = 17;

        public const int DlDpchSlotFormat7Sf256 = 20;

        public const int DlDpchSlotFormat8Sf128 = 23;

        public const int DlDpchSlotFormat9Sf128 = 26;

        public const int DlDpchSlotFormat10Sf128 = 29;

        public const int DlDpchSlotFormat11Sf128 = 32;

        public const int DlDpchSlotFormat12Sf64 = 35;

        public const int DlDpchSlotFormat13Sf32 = 38;

        public const int DlDpchSlotFormat14Sf16 = 41;

        public const int DlDpchSlotFormat15Sf8 = 44;

        public const int DlDpchSlotFormat16Sf4 = 47;

        public const int FdpchSlotFormat0Sf256 = 0;

        public const int FdpchSlotFormat1Sf256 = 1;

        public const int FdpchSlotFormat2Sf256 = 2;

        public const int FdpchSlotFormat3Sf256 = 3;

        public const int FdpchSlotFormat4Sf256 = 4;

        public const int FdpchSlotFormat5Sf256 = 5;

        public const int FdpchSlotFormat6Sf256 = 6;

        public const int FdpchSlotFormat7Sf256 = 7;

        public const int FdpchSlotFormat8Sf256 = 8;

        public const int FdpchSlotFormat9Sf256 = 9;

        public const int HspdschSlotFormat0Sf16Qpsk = 0;

        public const int HspdschSlotFormat1Sf1616qam = 1;

        public const int HspdschSlotFormat2Sf1664qam = 2;

        public const int SccpchSlotFormat0Sf256 = 0;

        public const int SccpchSlotFormat1Sf256 = 1;

        public const int SccpchSlotFormat2Sf256 = 2;

        public const int SccpchSlotFormat3Sf256 = 3;

        public const int SccpchSlotFormat4Sf128 = 4;

        public const int SccpchSlotFormat5Sf128 = 5;

        public const int SccpchSlotFormat6Sf128 = 6;

        public const int SccpchSlotFormat7Sf128 = 7;

        public const int SccpchSlotFormat8Sf64 = 8;

        public const int SccpchSlotFormat9Sf64 = 9;

        public const int SccpchSlotFormat10Sf32 = 10;

        public const int SccpchSlotFormat11Sf32 = 11;

        public const int SccpchSlotFormat12Sf16 = 12;

        public const int SccpchSlotFormat13Sf16 = 13;

        public const int SccpchSlotFormat14Sf8 = 14;

        public const int SccpchSlotFormat15Sf8 = 15;

        public const int SccpchSlotFormat16Sf4 = 16;

        public const int SccpchSlotFormat17Sf4 = 17;

        public const int UlFrameTypePrach = 0;

        public const int UlFrameTypeNonPrach = 1;

        public const int UlScramblingCodeTypeLong = 0;

        public const int UlScramblingCodeTypeShort = 1;

        public const int UlPhysicalChannelTypeDpcch = 0;

        public const int UlPhysicalChannelTypeDpdch = 1;

        public const int UlPhysicalChannelTypeEdpcch = 2;

        public const int UlPhysicalChannelTypeEdpdch = 3;

        public const int UlPhysicalChannelTypeHsdpcch = 4;

        public const int UlBranchI = 0;

        public const int UlBranchQ = 1;

        public const int UlDpcchSlotFormat0Sf256 = 0;

        public const int UlDpcchSlotFormat1Sf256 = 3;

        public const int UlDpcchSlotFormat2Sf256 = 4;

        public const int UlDpcchSlotFormat3Sf256 = 7;

        public const int UlDpcchSlotFormat4Sf256 = 8;

        public const int UlDpdchSlotFormat0Sf256 = 0;

        public const int UlDpdchSlotFormat1Sf128 = 1;

        public const int UlDpdchSlotFormat2Sf64 = 2;

        public const int UlDpdchSlotFormat3Sf32 = 3;

        public const int UlDpdchSlotFormat4Sf16 = 4;

        public const int UlDpdchSlotFormat5Sf8 = 5;

        public const int UlDpdchSlotFormat6Sf4 = 6;

        public const int EdpdchSlotFormat0Sf256 = 0;

        public const int EdpdchSlotFormat1Sf128 = 1;

        public const int EdpdchSlotFormat2Sf64 = 2;

        public const int EdpdchSlotFormat3Sf32 = 3;

        public const int EdpdchSlotFormat4Sf16 = 4;

        public const int EdpdchSlotFormat5Sf8 = 5;

        public const int EdpdchSlotFormat6Sf4 = 6;

        public const int EdpdchSlotFormat7Sf2 = 7;

        public const int EdpdchSlotFormat8Sf4 = 8;

        public const int EdpdchSlotFormat9Sf2 = 9;

        public const int PrachWaveformTypeSinglePreamble = 0;

        public const int PrachWaveformTypeSingleIdle = 1;

        public const int PrachWaveformTypeMessage = 2;

        public const int PrachWaveformTypePreamblePlusIdle = 3;

        public const int PrachWaveformTypePreamblePlusIdlePlusMessage = 4;

        public const int PrachDataMessageSlotFormat0Sf256 = 0;

        public const int PrachDataMessageSlotFormat1Sf128 = 1;

        public const int PrachDataMessageSlotFormat2Sf64 = 2;

        public const int PrachDataMessageSlotFormat3Sf32 = 3;

        public const int ToolkitCompatibilityVersion010000 = 10000;

        public const int DlTestModel1_4Dpch = 0;
        public const int DlTestModel1_8Dpch = 1;
        public const int DlTestModel1_16Dpch = 2;
        public const int DlTestModel1_32Dpch = 3;
        public const int DlTestModel1_64Dpch = 4;
        public const int DlTestModel2 = 5;
        public const int DlTestModel3_4Dpch = 6;
        public const int DlTestModel3_8Dpch = 7;
        public const int DlTestModel3_16Dpch = 8;
        public const int DlTestModel3_32Dpch = 9;
        public const int DlTestModel4_WithoutCpich = 10;
        public const int DlTestModel4_WithCpich = 11;
        public const int DlTestModel5_2Hspdsch_6Dpch = 12;
        public const int DlTestModel5_4Hspdsch_4Dpch = 13;
        public const int DlTestModel5_4Hspdsch_14Dpch = 14;
        public const int DlTestModel5_8Hspdsch_30Dpch = 15;
        public const int DlTestModel6_4Hspdsch_4Dpch = 16;
        public const int DlTestModel6_8Hspdsch_30Dpch = 17;

    }

    public enum niWCDMASGProperties
    {
        /// <summary>
        /// double
        /// </summary>
        ActualHeadroom = 7,

        /// <summary>
        /// int
        /// </summary>
        AutoHeadroomEnabled = 5,

        /// <summary>
        /// int
        /// </summary>
        AwgnEnabled = 9006,

        /// <summary>
        /// double
        /// </summary>
        CarrierFrequencyOffset = 9001,

        /// <summary>
        /// double
        /// </summary>
        CarrierToNoiseRatio = 9007,

        /// <summary>
        /// int
        /// </summary>
        DlAichExtendedIndicator = 1062,

        /// <summary>
        /// double
        /// </summary>
        DlAichGamma = 1063,

        /// <summary>
        /// int[]
        /// </summary>
        DlAichIndicators = 1061,

        /// <summary>
        /// int
        /// </summary>
        DlAichSignature = 1060,

        /// <summary>
        /// int
        /// </summary>
        DlChannelDataType = 1013,

        /// <summary>
        /// int
        /// </summary>
        DlChannelPnOrder = 1014,

        /// <summary>
        /// int
        /// </summary>
        DlChannelPnSeed = 1015,

        /// <summary>
        /// double
        /// </summary>
        DlChannelRelativePower = 1010,

        /// <summary>
        /// int
        /// </summary>
        DlChannelSpreadingCode = 1011,

        /// <summary>
        /// int
        /// </summary>
        DlChannelTimingOffset = 1012,

        /// <summary>
        /// int[]
        /// </summary>
        DlChannelUserDefinedBits = 1016,

        /// <summary>
        /// int
        /// </summary>
        DlDpcchDataType = 1085,

        /// <summary>
        /// int
        /// </summary>
        DlDpcchPnOrder = 1086,

        /// <summary>
        /// int
        /// </summary>
        DlDpcchPnSeed = 1087,

        /// <summary>
        /// int[]
        /// </summary>
        DlDpcchUserDefinedBits = 1088,

        /// <summary>
        /// int
        /// </summary>
        DlDpchSlotFormat = 1080,

        /// <summary>
        /// int
        /// </summary>
        DlEhichAckIndicator = 1201,

        /// <summary>
        /// int
        /// </summary>
        DlEhichSignature = 1200,

        /// <summary>
        /// int
        /// </summary>
        DlErgchRelativeGrant = 1221,

        /// <summary>
        /// int
        /// </summary>
        DlErgchSignature = 1220,

        /// <summary>
        /// int
        /// </summary>
        DlFdpchSlotFormat = 1100,

        /// <summary>
        /// int
        /// </summary>
        DlHspdschSlotFormat = 1240,

        /// <summary>
        /// int
        /// </summary>
        DlNumberOfPichIndicatorsPerFrame = 1140,

        /// <summary>
        /// int
        /// </summary>
        DlPhysicalChannelType = 1001,

        /// <summary>
        /// int
        /// </summary>
        DlPrimaryScramblingCode = 1020,

        /// <summary>
        /// int
        /// </summary>
        DlSccpchSlotFormat = 1260,

        /// <summary>
        /// int
        /// </summary>
        DlScramblingCodeOffset = 1022,

        /// <summary>
        /// int
        /// </summary>
        DlScramblingCodeType = 1021,

        /// <summary>
        /// int
        /// </summary>
        DlSschCodeGroup = 1040,

        /// <summary>
        /// int
        /// </summary>
        DuplexMode = 3,

        /// <summary>
        /// double
        /// </summary>
        Headroom = 6,

        /// <summary>
        /// double
        /// </summary>
        IDcOffset = 9003,

        /// <summary>
        /// double
        /// </summary>
        IqGainImbalance = 9005,

        /// <summary>
        /// double
        /// </summary>
        IqRate = 9500,

        /// <summary>
        /// int
        /// </summary>
        NumberOfPhysicalDlChannels = 1000,

        /// <summary>
        /// int
        /// </summary>
        NumberOfPhysicalUlChannels = 2000,

        /// <summary>
        /// double
        /// </summary>
        QDcOffset = 9004,

        /// <summary>
        /// double
        /// </summary>
        QuadratureSkew = 9002,

        /// <summary>
        /// int
        /// </summary>
        ToolkitCompatibilityVersion = 65533,

        /// <summary>
        /// int
        /// </summary>
        UlChannelBranch = 2061,

        /// <summary>
        /// int
        /// </summary>
        UlChannelDataType = 2013,

        /// <summary>
        /// int
        /// </summary>
        UlChannelPnOrder = 2014,

        /// <summary>
        /// int
        /// </summary>
        UlChannelPnSeed = 2015,

        /// <summary>
        /// double
        /// </summary>
        UlChannelRelativePower = 2010,

        /// <summary>
        /// int
        /// </summary>
        UlChannelSpreadingCode = 2011,

        /// <summary>
        /// int
        /// </summary>
        UlChannelTimingOffset = 2012,

        /// <summary>
        /// int[]
        /// </summary>
        UlChannelUserDefinedBits = 2016,

        /// <summary>
        /// int
        /// </summary>
        UlDpcchSlotFormat = 2040,

        /// <summary>
        /// int
        /// </summary>
        UlDpdchSlotFormat = 2060,

        /// <summary>
        /// int
        /// </summary>
        UlEdpdchSlotFormat = 2100,

        /// <summary>
        /// int
        /// </summary>
        UlFrameType = 2001,

        /// <summary>
        /// int
        /// </summary>
        UlPhysicalChannelType = 2002,

        /// <summary>
        /// int
        /// </summary>
        UlPrachControlMessageDataType = 2513,

        /// <summary>
        /// int
        /// </summary>
        UlPrachControlMessagePnOrder = 2514,

        /// <summary>
        /// int
        /// </summary>
        UlPrachControlMessagePnSeed = 2515,

        /// <summary>
        /// double
        /// </summary>
        UlPrachControlMessagePower = 2511,

        /// <summary>
        /// int[]
        /// </summary>
        UlPrachControlMessageUserDefinedBits = 2516,

        /// <summary>
        /// int
        /// </summary>
        UlPrachDataMessageDataType = 2507,

        /// <summary>
        /// int
        /// </summary>
        UlPrachDataMessagePnOrder = 2508,

        /// <summary>
        /// int
        /// </summary>
        UlPrachDataMessagePnSeed = 2509,

        /// <summary>
        /// double
        /// </summary>
        UlPrachDataMessagePower = 2505,

        /// <summary>
        /// int
        /// </summary>
        UlPrachDataMessageSlotFormat = 2517,

        /// <summary>
        /// int[]
        /// </summary>
        UlPrachDataMessageUserDefinedBits = 2510,

        /// <summary>
        /// int
        /// </summary>
        UlPrachMessageLength = 2502,

        /// <summary>
        /// int
        /// </summary>
        UlPrachNumberOfIdleSlots = 2501,

        /// <summary>
        /// int
        /// </summary>
        UlPrachNumberOfPreambles = 2500,

        /// <summary>
        /// double
        /// </summary>
        UlPrachPreamblePower = 2503,

        /// <summary>
        /// int
        /// </summary>
        UlPrachPreambleSignature = 2504,

        /// <summary>
        /// int
        /// </summary>
        UlPrachWaveformType = 2140,

        /// <summary>
        /// int
        /// </summary>
        UlScramblingCode = 2020,

        /// <summary>
        /// int
        /// </summary>
        UlScramblingCodeType = 2021,

    }
}


    