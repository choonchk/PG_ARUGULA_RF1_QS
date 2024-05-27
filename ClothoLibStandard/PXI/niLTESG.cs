
//==========================================================================
// .NET library for NI-LTE Generation toolkit.
//--------------------------------------------------------------------------
// Copyright (c) National Instruments 2012.  All Rights Reserved.			
//--------------------------------------------------------------------------
// Title:	niLTESG.cs
// Purpose: C# wrapper for NI-LTE Signal Generation 2.0 toolkit.
//==========================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;

using NationalInstruments.ModularInstruments.Interop;

namespace NationalInstruments.ModularInstruments.niLTESG
{
    public class NiLteSg : IDisposable
    {
        private HandleRef _handle;
        private bool _disposed;
        private bool _isNamedSession;

        #region Implementing the IDisposable interface
        ~NiLteSg()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if ((this._disposed == false))
            {
                if (disposing)
                {
                    // Dispose any managed resources here.
                }

                if (!_isNamedSession)
                    TestForError(PInvoke.niLTESG_CloseSession(Handle));

                // Indicate that the instance has been disposed.
                _handle = new HandleRef(null, System.IntPtr.Zero);
                this._disposed = true;
            }
        }

        #endregion
        public HandleRef Handle
        {
            get
            {
                if (this._disposed)
                    throw new ObjectDisposedException("LTE SG session was disposed.");
                return _handle;
            }
        }

        private void OpenSession(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            IntPtr handle;
            TestForError(PInvoke.niLTESG_OpenSession(sessionName, toolkitCompatibilityVersion, out handle, out isNewSession));
            this._handle = new HandleRef(this, handle);
            this._isNamedSession = !String.IsNullOrEmpty(sessionName);
            this._disposed = false;
        }

        /// <summary>
        /// Looks up an existing niLTE generation session and returns the refnum that you can pass to subsequent niLTE generation functions. If the lookup fails, the OpenSession function creates a new niLTE generation session and returns a new refnum.
        /// </summary>
        /// <param name = "toolkitCompatibilityVersion">
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///     niLTESGConstants.ToolkitCompatibilityVersion010000
        /// Specifies that the toolkit version is 1.0.0.
        /// </param>
        public NiLteSg(int toolkitCompatibilityVersion)
        {
            int isNewSession;
            OpenSession(null, toolkitCompatibilityVersion, out isNewSession);
        }

        /// <summary>
        /// Looks up an existing niLTE generation session and returns the refnum that you can pass to subsequent niLTE generation functions. If the lookup fails, the OpenSession function creates a new niLTE generation session and returns a new refnum.
        /// </summary>
        /// <param name = "sessionName">
        /// Specifies the name of the session that you are looking up or creating. If a session with the same name already exists, this function returns a reference to that session. To get the reference to an already-opened session x, specify x as the session name. 
        ///  You can obtain the reference to an existing session multiple times if you have not called the CloseSession function in that session. You do not need to close the session multiple times. To create an unnamed session, pass an empty string or NULL to the sessionName parameter.
        /// Tip: National Instruments recommends that you call the niLteSg.CloseSession function for each uniquely-named instance of the OpenSession function or each instance of the OpenSession function with an unnamed session.
        /// </param>
        /// <param name = "toolkitCompatibilityVersion">
        /// Specifies the version of the toolkit in use. If the behavior of the toolkit changes in a new version, use this parameter to specify that you want to continue using the behavior of the previous release.
        ///     niLTESGConstants.ToolkitCompatibilityVersion010000
        /// Specifies that the toolkit version is 1.0.0.
        /// </param>
        /// <param name = "isNewSession">
        /// Returns niLTESGConstants.True if the function creates a new session. This parameter returns niLTESGConstants.False if the function returns a reference to an existing session.
        /// </param>
        public NiLteSg(string sessionName, int toolkitCompatibilityVersion, out int isNewSession)
        {
            OpenSession(sessionName, toolkitCompatibilityVersion, out isNewSession);
        }

        /// <summary>
        /// Configures the niLTE session to contain physical uplink control channels (PUCCH) that occupy the available system bandwidth in all 10 subframes of the given frame.
        /// This function sets the NILTESG_DUPLEX_MODE attribute to NILTESG_VAL_DUPLEX_MODE_UL_FDD. The toolkit uses all other attributes from the session.
        /// </summary>
        /// <param name = "duplexMode">
        /// Specifies the direction and the duplexing technique that the toolkit uses to create the waveform. The default value is NILTESG_VAL_DUPLEX_MODE_UL_FDD.
        ///     NILTESG_VAL_DUPLEX_MODE_DL_FDD (0)
        ///    Specifies that the duplexing technique is frequency-division duplex (FDD) for downlink signal generation.
        ///      NILTESG_VAL_DUPLEX_MODE_UL_FDD (1)
        ///    Specifies that the duplexing technique is FDD for uplink signal generation.
        ///     NILTESG_VAL_DUPLEX_MODE_DL_TDD (2)
        ///    Specifies that the duplexing technique is time-division duplex (TDD) for downlink signal generation.
        ///    NILTESG_VAL_DUPLEX_MODE_UL_TDD (3)
        ///    Specifies that the duplexing technique is TDD for uplink signal generation.
        ///     NILTESG_VAL_DUPLEX_MODE_DLUL_TDD (4)
        ///    Specifies that the duplexing technique is TDD. The generated frame contains both uplink and downlink signals.
        /// </param>
        /// <param name = "pUCCHFormat">
        /// Specifies the format used for physical uplink control channel (PUCCH) transmission. The default value is NILTESG_VAL_UL_PUCCH_FORMAT_1.
        ///     NILTESG_VAL_UL_PUCCH_FORMAT_1 (0)
        ///    Specifies that the toolkit uses Format 1 for PUCCH transmission.
        ///     NILTESG_VAL_UL_PUCCH_FORMAT_1A (1)
        ///    Specifies that the toolkit uses Format 1A for PUCCH transmission.
        ///     NILTESG_VAL_UL_PUCCH_FORMAT_1B (2)
        ///    Specifies that the toolkit uses Format 1B for PUCCH transmission.
        ///     NILTESG_VAL_UL_PUCCH_FORMAT_2 (3)
        ///    Specifies that the toolkit uses Format 2 for PUCCH transmission.
        ///     NILTESG_VAL_UL_PUCCH_FORMAT_2A (4)
        ///    Specifies that the toolkit uses Format 2A for PUCCH transmission.
        ///     NILTESG_VAL_UL_PUCCH_FORMAT_2B (5)
        ///    Specifies that the toolkit uses Format 2B for PUCCH transmission.
        /// </param>
        /// <param name = "n_PUCCH_1">
        /// Specifies a parameter used in determining the resource block assigned for the physical uplink control channel (PUCCH) formats 1/1a/1b/2/2a/2b as defined in section 5.4.1 of the 3GPP TS 36.211 v8.8.0 specifications.
        /// </param>
        /// <param name = "pUCCHpowerdB">
        /// Specifies the physical uplink control channel (PUCCH) power level, in dB, relative to the power of the PUCCH demodulation reference signal (DMRS). The default value is 0.
        /// </param>
        /// <param name = "uLDLConfiguration">
        /// Specifies the uplink/downlink (UL/DL) configuration index for the FDD/TDD frame.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int ConfigureFullyFilledPUCCHFrame(int duplexMode, int pUCCHFormat, int n_PUCCH_1, double pUCCHpowerdB, int uLDLConfiguration)
        {
            int pInvokeResult = PInvoke.niLTESG_ConfigureFullyFilledPUCCHFrame(Handle, duplexMode, pUCCHFormat, n_PUCCH_1, pUCCHpowerdB, uLDLConfiguration);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the complimentary cumulative distribution function (CCDF) trace for an ideal Gaussian distribution signal.
        /// </summary>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "x0">
        /// Returns the starting power level relative to the average power. 
        /// </param>
        /// <param name = "dx">
        /// Returns the power interval, in dB. 
        /// </param>
        /// <param name = "gaussianProbabilities">
        /// Returns an array of the percentage of samples that lie on or above the corresponding power level on the x-axis. 
        /// </param>
        /// <param name = "dataArraySize">
        /// Specifies the number of elements in the probabilities array. Pass NULL to the probabilities parameter to get size of the array in the actualNumDataArrayElements parameter.
        /// </param>
        /// <param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the probabilities array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int CCDFGetGaussianProbabilitiesTrace(string channelString, out double x0, out double dx, double[] gaussianProbabilities, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESG_CCDFGetGaussianProbabilitiesTrace(Handle, channelString, out x0, out dx, gaussianProbabilities, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the complimentary cumulative distribution function (CCDF) trace for the created waveform. Call this function after the execution of the niLTESG_CreateWaveformComplexF64 function or niLTESG_CreateMIMOWaveformsComplexF64 function. Refer to the CCDF Measurement topic for more information about CCDF measurements.
        /// </summary>
        /// <param name = "channelString">
        /// Specifies the active channel string.
        /// </param>
        /// <param name = "x0">
        /// Returns the starting power level relative to the average power.
        /// </param>
        /// <param name = "dx">
        /// Returns the power interval, in dB.
        /// </param>
        /// <param name = "probabilities">
        /// Returns an array of the percentage of samples that lie on or above the corresponding power level on the x-axis.
        /// </param>
        /// <param name = "dataArraySize">
        /// Specifies the number of elements in the probabilities array. Pass NULL to the dataArraySize parameter to get the size of the array in the actualNumDataArrayElements parameter.
        /// </param>
        /// <param name = "actualNumDataArrayElements">
        /// Returns the actual number of elements populated in the probabilities array. If the array is not large enough to hold all the samples, the function returns an error and 
        /// the actualNumDataArrayElements parameter returns the minimum expected size of the output array.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int CCDFGetProbabilitiesTrace(string channelString, out double x0, out double dx, double[] probabilities, int dataArraySize, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESG_CCDFGetProbabilitiesTrace(Handle, channelString, out x0, out dx, probabilities, dataArraySize, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Checks for errors on all configured attributes. If the configuration is invalid, this function returns an error. 
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int CheckToolkitError()
        {
            int pInvokeResult = PInvoke.niLTESG_CheckToolkitError(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Closes the niLTE generation session and releases resources associated with that session. Call this function once for each uniquely-named session that you have created. 
        /// </summary>
        public void CloseSession()
        {
            if (!_isNamedSession)
            {
                Dispose();
            }
            else
            {
                if (!Handle.Handle.Equals(IntPtr.Zero))
                {
                    TestForError(PInvoke.niLTESG_CloseSession(Handle));
                }
            }
        }

        /// <summary>
        /// Specifies the test model that the toolkit uses to configure the session as defined in section 6.1.1 of the 3GPP TS 36.141 v8.6.0 specifications.
        /// </summary>
        /// <param name = "downlinkTestModel">
        /// Specifies the test model that the toolkit uses to configure the session as defined in section 6.1.1 of the 3GPP TS 36.141 Specifications 8.6.0. The default value is NILTESG_VAL_DL_TEST_MODEL_E_TM_1_1.
        ///  NILTESG_VAL_DL_TEST_MODEL_E_TM_1_1 (0)
        ///  Specifies the E-UTRA test model 1.1. 
        ///  NILTESG_VAL_DL_TEST_MODEL_E_TM_1_2 (1)
        ///  Specifies the E-UTRA test model 1.2.
        ///  NILTESG_VAL_DL_TEST_MODEL_E_TM_2 (2)
        ///  Specifies the E-UTRA test model 2.
        ///  NILTESG_VAL_DL_TEST_MODEL_E_TM_3_1 (3)
        ///  Specifies the E-UTRA test model 3.1.
        ///  NILTESG_VAL_DL_TEST_MODEL_E_TM_3_2 (4)
        ///  Specifies the E-UTRA test model 3.2.
        ///  NILTESG_VAL_DL_TEST_MODEL_E_TM_3_3 (5)
        ///  Specifies the E-UTRA test model 3.3.
        /// </param>
        /// <param name = "systemBandwidth">
        /// Specifies the LTE system bandwidth, in hertz (Hz), of the generated waveform.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int ConfigureDownlinkTestModel(int downlinkTestModel, double systemBandwidth)
        {
            int pInvokeResult = PInvoke.niLTESG_ConfigureDownlinkTestModel(Handle, downlinkTestModel, systemBandwidth);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the niLTE session to contain physical uplink shared channels (PUSCH) that occupy the available system bandwidth in all 10 subframes of the given frame with the specified modulation scheme. The toolkit configures the PUSCH for time-division duplex (TDD) based on the value of the NILTESG_UL_DL_CONFIGURATION attribute.  
        /// For example, if the system bandwidth is 20 MHz and the PUSCH modulation scheme is QPSK, this function returns a session that contains 10 PUSCHs designated as pusch0 to pusch9 in subframes 0 to 9 with QPSK modulation. For each PUSCH, the resource block offset is set to 0 and the number of resource blocks is set to 100.
        /// This function sets the NILTESG_DUPLEX_MODE attribute to NILTESG_VAL_DUPLEX_MODE_UL_FDD. The toolkit uses all other attributes from the session.
        /// </summary>
        /// <param name = "duplexMode">
        /// Specifies the direction and the duplexing technique that the toolkit uses to create the waveform. The default value is NILTESG_VAL_DUPLEX_MODE_UL_FDD.
        ///     NILTESG_VAL_DUPLEX_MODE_DL_FDD (0)
        ///    Specifies that the duplexing technique is frequency-division duplex (FDD) for downlink signal generation.
        ///      NILTESG_VAL_DUPLEX_MODE_UL_FDD (1)
        ///    Specifies that the duplexing technique is FDD for uplink signal generation.
        ///     NILTESG_VAL_DUPLEX_MODE_DL_TDD (2)
        ///    Specifies that the duplexing technique is time-division duplex (TDD) for downlink signal generation.
        ///    NILTESG_VAL_DUPLEX_MODE_UL_TDD (3)
        ///    Specifies that the duplexing technique is TDD for uplink signal generation.
        ///     NILTESG_VAL_DUPLEX_MODE_DLUL_TDD (4)
        ///    Specifies that the duplexing technique is TDD. The generated frame contains both uplink and downlink signals.
        /// </param>
        /// <param name = "pUSCHModulationScheme">
        /// Specifies the modulation scheme for PUSCH transmission. The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        ///     NILTESG_VAL_MODULATION_SCHEME_QPSK (0)
        ///    Specifies a quadrature phase-shift keying (QPSK) modulation scheme.
        ///     NILTESG_VAL_MODULATION_SCHEME_16_QAM (1)
        ///    Specifies a 16-quadrature amplitude modulation (QAM) scheme.
        ///     NILTESG_VAL_MODULATION_SCHEME_64_QAM (2)
        ///    Specifies a 64-QAM modulation scheme.
        /// </param>
        /// <param name = "systemBandwidth">
        /// Specifies the LTE system bandwidth, in hertz (Hz), of the generated waveform.
        /// </param>
        /// <param name = "uLDLConfiguration">
        /// Specifies the uplink/downlink (UL/DL) configuration index for the FDD/TDD frame.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int ConfigureFullyFilledPUSCHFrame(int duplexMode, int pUSCHModulationScheme, double systemBandwidth, int uLDLConfiguration)
        {
            int pInvokeResult = PInvoke.niLTESG_ConfigureFullyFilledPUSCHFrame(Handle, duplexMode, pUSCHModulationScheme, systemBandwidth, uLDLConfiguration);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates waveforms according to the parameters you specify, and saves the waveforms to a file.
        /// In addition to the waveform, this function also saves the NILTESG_RECOMMENDED_IQ_RATE and NILTESG_HEADROOM attributes to the file.
        /// </summary>
        /// <param name = "filePath">
        /// Specifies the absolute path to the file to which the toolkit saves the waveforms.
        /// </param>
        /// <param name = "fileOperation">
        /// Specifies the operation to perform on the file.
        ///  NILTESG_FILE_OPERATION_MODE_OPEN(0)
        /// Opens an existing file to write the niLTE generation session attribute values. The niLTESG_CreateAndWriteWaveformsToFile function appends the new waveforms to the end of the existing file if the file already contains LTE waveforms.
        /// NILTESG_FILE_OPERATION_MODE_OPENORCREATE(1)
        /// Opens an existing file or creates a new file if the file does not exist. The niLTESG_CreateAndWriteWaveformsToFile function appends the new waveforms to the end of the existing file if the file already contains LTE waveforms.
        /// NILTESG_FILE_OPERATION_MODE_CREATEORREPLACE(2)
        /// Creates a new file or replaces a file if it exists. The niLTESG_CreateAndWriteWaveformsToFile function overwrites the new waveforms to existing files.
        /// NILTESG_FILE_OPERATION_MODE_CREATE(3)
        /// Creates a new file.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int CreateAndWriteWaveformsToFile(string filePath, int fileOperation)
        {
            int pInvokeResult = PInvoke.niLTESG_CreateAndWriteWaveformsToFile(Handle, filePath, fileOperation);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Reads values of session attributes (configuration) saved in a file and sets these values to the corresponding attributes on the session, thus restoring the state of the toolkit to the original state when the file was saved.
        /// </summary>
        /// <param name = "filePath">
        /// Specifies the absolute path to the file from which the toolkit loads the configuration.
        /// </param>
        /// <param name = "resetSession">
        /// Specifies whether the toolkit must reset all the attributes of the session to their default values before setting the new values specified in the file. The default value is NILTESG_VAL_TRUE. 
        /// NILTESG_VAL_FALSE(0)
        /// Specifies that the toolkit does not reset all the properties of the session to their default values.
        /// NILTESG_VAL_TRUE(1)
        /// Specifies that the toolkit resets all the properties of the session to their default values before setting new values.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int LoadConfigurationFromFile(string filePath, int resetSession)
        {
            int pInvokeResult = PInvoke.niLTESG_LoadConfigurationFromFile(Handle, filePath, resetSession);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Saves attributes of the session, which you have modified after opening the session, to a file located at the specified path. 
        /// You can use this function to save the current state of the toolkit session to a file.  You can later load the saved configuration using the niLTESG_LoadConfigurationFromFile function.
        /// </summary>
        /// <param name = "filePath">
        /// Specifies the absolute path to the TDMS file to which the toolkit saves the configuration.
        /// </param>
        /// <param name = "operation">
        /// Specifies the operation to perform on the file. The default value is NILTESG_FILE_OPERATION_MODE_OPEN(0).
        /// NILTESG_FILE_OPERATION_MODE_OPEN(0)
        /// Opens an existing file to write the niLTE generation session attribute values.
        /// NILTESG_FILE_OPERATION_MODE_OPEN_OR_CREATE(1)
        /// Opens an existing file or creates a new file if the file does not exist.
        /// NILTESG_FILE_OPERATION_MODE_CREATE_OR_REPLACE(2)
        /// Creates a new file or replaces an existing file.
        /// NILTESG_FILE_OPERATION_MODE_CREATE(3)
        /// Creates a new file.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int SaveConfigurationToFile(string filePath, int operation)
        {
            int pInvokeResult = PInvoke.niLTESG_SaveConfigurationToFile(Handle, filePath, operation);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates LTE I/Q data for multiple streams and returns the data as an array of complex waveform data. This function returns one frame (including the idle interval) at a time. For multiframe generation, run the function in a loop for specified number of times with the reset parameter set to NILTESG_VAL_FALSE. For multiframe generation, set the reset parameter to NILTESG_VAL_TRUE for the first frame.
        /// </summary>
        /// <param name = "reset">
        /// Specifies whether to reset the internal states in the created waveform. Set this parameter to NILTESG_VAL_TRUE for the first frame of the generation or if you want to reset the pseudonoise (PN) seed.
        /// </param>
        /// <param name = "t0">
        /// Returns the starting time, in seconds. The size of this array must be at least equal to value of the numberOfAntennas parameter.
        /// </param>
        /// <param name = "dt">
        /// Returns the time interval, in seconds, between baseband I/Q samples. The size of this array must be at least equal to value of the numberOfAntennas parameter.
        /// </param>
        /// <param name = "waveforms">
        /// Returns the LTE I/Q data. This parameter must be equal to the value of the NILTESG_NUMBER_OF_ANTENNAS attribute. The waveforms are written sequentially in the array. Allocate an array at least as large as numberOfAntennas times individualWaveformSize for this parameter. Pass NULL to the waveforms parameter to query the size of the waveform per antenna.
        /// </param>
        /// <param name = "numberofAntennas">
        /// Specifies the number of antennas for which to generate waveforms. This parameter must be equal to the value of the NILTESG_NUMBER_OF_ANTENNAS attribute. 
        /// </param>
        /// <param name = "individualWaveformSize">
        /// Specifies the size of each antenna.
        /// </param>
        /// <param name = "actualNumSamplesinEachWfm">
        /// Returns the actual number of samples in each antenna. If the array is not large enough to hold all the samples, the function returns an error and this parameter returns the minimum expected size of the output array.
        /// </param>
        /// <param name = "generationDone">
        /// Indicates whether the function has generated all the frames. If you generate multiple frames, call this function in a loop and use the generationDone parameter as terminating condition. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int CreateMIMOWaveformsComplexF64(int reset, double[] t0, double[] dt, niComplexNumber[] waveforms, int numberofAntennas, int individualWaveformSize, out int actualNumSamplesinEachWfm, out int generationDone)
        {
            int pInvokeResult = PInvoke.niLTESG_CreateMIMOWaveformsComplexF64(Handle, reset, t0, dt, waveforms, numberofAntennas, individualWaveformSize, out actualNumSamplesinEachWfm, out generationDone);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates a waveform for a single antenna according to the values that you specify. This function generates one frame at a time. For multiframe generation, call this function in a loop for the specified number of times with the reset parameter set to NILTESG_VAL_FALSE. If you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD, for multiframe generation, this function increments the system frame number, which is part of the physical broadcast channel (PBCH) payload. For multiframe generation, set the reset parameter to NILTESG_VAL_TRUE only for the first frame.
        /// </summary>
        /// <param name = "reset">
        /// Specifies whether to reset the internal states in the created waveform. Set this parameter to NILTESG_VAL_TRUE for the first frame of the generation or if you want to reset the pseudonoise (PN) seed.
        /// </param>
        /// <param name = "t0">
        /// Returns the starting time, in seconds.
        /// </param>
        /// <param name = "dt">
        /// Returns the time interval between baseband I/Q samples, in seconds.
        /// </param>
        /// <param name = "waveform">
        /// Returns the LTE I/Q data. This parameter must be at least equal to the waveformSize parameter. You can pass NULL to the waveform parameter to query the size of the waveform.
        /// </param>
        /// <param name = "waveformSize">
        /// Specifies the waveform size in samples.
        /// </param>
        /// <param name = "actualNumWaveformSamples">
        /// Returns the actual number of samples populated in waveform array. If the array is not large enough to hold all the samples, the function returns an error and this parameter returns the minimum expected size of the output array.
        /// </param>
        /// <param name = "generationDone">
        /// Indicates whether the function has generated all the frames. If you generate multiple frames, call this function in a loop and use the generationDone parameter as terminating condition. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int CreateWaveformComplexF64(int reset, out double t0, out double dt, niComplexNumber[] waveform, int waveformSize, out int actualNumWaveformSamples, out int generationDone)
        {
            int pInvokeResult = PInvoke.niLTESG_CreateWaveformComplexF64(Handle, reset, out t0, out dt, waveform, waveformSize, out actualNumWaveformSamples, out generationDone);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Computes the carrier frequency using the values that you specify in the EARFCN and duplexMode parameters, as described in section 5.7.3 of the 3GPP TS 36.101 v8.6.0 specifications.
        /// </summary>
        /// <param name = "eARFCN">
        /// Specifies the E-UTRA absolute radio frequency channel number (EARFCN) as described in Table 5.7.3-1 in section 5.7.3 of the 3GPP TS 36.101 v8.6.0 specifications.
        /// </param>
        /// <param name = "duplexMode">
        /// Specifies the direction and the duplexing technique that the toolkit uses to create the waveform. The default value is NILTESG_VAL_DUPLEX_MODE_UL_FDD.
        ///     NILTESG_VAL_DUPLEX_MODE_DL_FDD (0)
        ///    Specifies that the duplexing technique is frequency-division duplex (FDD) for downlink signal generation.
        ///      NILTESG_VAL_DUPLEX_MODE_UL_FDD (1)
        ///    Specifies that the duplexing technique is FDD for uplink signal generation.
        ///     NILTESG_VAL_DUPLEX_MODE_DL_TDD (2)
        ///    Specifies that the duplexing technique is time-division duplex (TDD) for downlink signal generation.
        ///    NILTESG_VAL_DUPLEX_MODE_UL_TDD (3)
        ///    Specifies that the duplexing technique is TDD for uplink signal generation.
        ///     NILTESG_VAL_DUPLEX_MODE_DLUL_TDD (4)
        ///    Specifies that the duplexing technique is TDD. The generated frame contains both uplink and downlink signals.
        /// </param>
        /// <param name = "carrierFrequency">
        /// Returns the carrier frequency, in hertz (Hz).
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int EARFCNtoCarrierFrequency(int eARFCN, int duplexMode, out double carrierFrequency)
        {
            int pInvokeResult = PInvoke.niLTESG_EARFCNtoCarrierFrequency(eARFCN, duplexMode, out carrierFrequency);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        ///Returns the actual headroom, in dB, that the toolkit applies to the waveform.
        ///    Use an 'antennan' active channel string to read this attribute. Refer to the Configuring Active Channels topic for more information about active channels.
        /// </summary>
        public int GetActualHeadroom(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ActualHeadroom, channel, out value);
        }

        /// <summary>
        /// Takes the error code returned by niLTE generation functions and returns the interpretation as a user-readable string. 
        /// </summary>
        /// <param name = "errorCode">
        /// Specifies the error code that is returned from any of the niLTE generation functions.
        /// </param>
        /// <param name = "errorMessage">
        /// Returns the user-readable message string that corresponds to the error code you specify. The errorMessage buffer must have at least as many elements as are indicated in the errorMessageLength parameter. If you pass NULL to the errorMessage parameter, the function returns the actual length of the error message.
        /// </param>
        /// <param name = "errorMessageLength">
        /// Specifies the length of the errorMessage buffer.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int GetErrorString(int errorCode, StringBuilder errorMessage, int errorMessageLength)
        {
            int pInvokeResult = PInvoke.niLTESG_GetErrorString(Handle, errorCode, errorMessage, errorMessageLength);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Resets the attribute, which you  specify in the attribute parameter, to its default value. You can reset only a writable attribute using this function.
        /// </summary>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "attributeID">
        /// Specifies the ID of the niLTE generation attribute that you want to reset.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int ResetAttribute(string channelString, niLTESGProperties attributeID)
        {
            int pInvokeResult = PInvoke.niLTESG_ResetAttribute(Handle, channelString, attributeID);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Resets all the attributes of the session to their default values. 
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int ResetSession()
        {
            int pInvokeResult = PInvoke.niLTESG_ResetSession(Handle);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Clears the attributes stored in the RFSG database and clears the waveforms from the RF signal generator memory. 
        /// This function clears the waveforms and  attributes of the waveforms that you specify in the waveformName parameter. If you set the waveformName parameter as empty, this function clears all the waveforms and their attributes. 
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "waveform">
        /// Specifies the name of the waveform to clear. If you set this parameter as empty, the function clears all the waveforms and their attributes. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGClearDatabase(HandleRef rfsgHandle, string channelString, string waveform)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGClearDatabase(rfsgHandle, channelString, waveform);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Queries the selected NI-RFSG script, looks up the waveforms in the script, retrieves the minimum headroom of the
        /// waveforms in the script, adds this value to the powerLevel parameter, and sets the result to the NIRFSG_ATTR_POWER_LEVEL attribute. Set the NIRFSG_ATTR_POWER_LEVEL_TYPE attribute to NIRFSG_VAL_PEAK_POWER before calling this function. 
        /// Refer to the Power Scaling topic for more information about power scaling.
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "script">
        /// Specifies the current NI-RFSG script used to generate the signal.
        /// </param>
        /// <param name = "powerLevel">
        /// Specifies the peak power level, in dBm. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGConfigurePowerLevel(HandleRef rfsgHandle, string channelString, string script, double powerLevel)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGConfigurePowerLevel(rfsgHandle, channelString, script, powerLevel);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the I/Q rate and power level of the waveforms that you specify in the script parameter. This function sets the NIRFSG_ATTR_IQ_RATE attribute to the I/Q rate in the RFSG database if the I/Q rates are the same for all the waveforms. The function sets the NIRFSG_ATTR_POWER_LEVEL attribute to the sum of the power level that you specify in the powerLevel parameter
        /// and the minimum headrooms of all the waveforms.
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "script">
        /// Specifies the script that controls waveform generation. NI-RFSG supports multiple scripts that you can select by name using the NIRFSG_ATTR_SELECTED_SCRIPT attribute.
        /// </param>
        /// <param name = "powerLevel">
        /// Specifies the power level, in dBm. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGConfigureScript(HandleRef rfsgHandle, string channelString, string script, double powerLevel)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGConfigureScript(rfsgHandle, channelString, script, powerLevel);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates a waveform for multiple antennas according to parameters that you specify. This function generates one frame at a time. 
        /// </summary>
        /// <param name = "rfsgHandles">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "hardwareChannelStrings">
        /// Specifies the RFSG device channels. Set this parameter to NULL.
        /// </param>
        /// <param name = "numberofAntennas">
        /// Specifies the number of antennas for which to generate waveforms.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name used to store the waveform. This string is case-insensitive, alphanumeric, and does not use reserved words.  
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGCreateAndDownloadMIMOWaveforms(HandleRef[] rfsgHandles, String[] hardwareChannelStrings, int numberofAntennas, string waveformName)
        {
            IntPtr[] intPtrHandles = new IntPtr[rfsgHandles.Length];
            for (int i = 0; i < rfsgHandles.Length; i++)
                intPtrHandles[i] = rfsgHandles[i].Handle;
            int pInvokeResult = PInvoke.niLTESG_RFSGCreateAndDownloadMIMOWaveforms(Handle, intPtrHandles, hardwareChannelStrings, numberofAntennas, waveformName);
            TestForError(rfsgHandles, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates a waveform for a single antenna according to parameters that you specify. This function generates one frame at a time.
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "hardwareChannelString">
        /// Specifies the RFSG device channel. Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name used to store the waveform. This string is case-insensitive, alphanumeric, and does not use reserved words. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGCreateAndDownloadWaveform(HandleRef rfsgHandle, string hardwareChannelString, string waveformName)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGCreateAndDownloadWaveform(Handle, rfsgHandle, hardwareChannelString, waveformName);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the headroom, in dB, stored in the RFSG database. The function uses the waveform name as the key to retrieve the waveform attributes.
        /// Note: Use the niLTESG_RFSGStoreHeadroom function to store the headroom in the RFSG database.  
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name of the waveform for which you want to retrieve the headroom. The toolkit uses the waveformName parameter as the key to retrieve the waveform attributes in the RFSG database.
        /// </param>
        /// <param name = "headroom">
        /// Returns the headroom, in dB, stored in the RFSG database. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGRetrieveHeadroom(HandleRef rfsgHandle, string channelString, string waveformName, out double headroom)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGRetrieveHeadroom(rfsgHandle, channelString, waveformName, out headroom);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the I/Q rate stored in the RFSG database. The function uses the waveform name as the key to retrieve the waveform attributes. 
        /// Note: Use the niLTESG_RFSGStoreIQRate function to store the I/Q rate in the RFSG database.  
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name of the waveform for which you want to retrieve the I/Q rate. The toolkit uses the waveformName parameter as the key to retrieve the waveform attributes from the RFSG database. 
        /// </param>
        /// <param name = "iQRate">
        /// Returns the I/Q rate stored in the RFSG database for the waveform that you specify in the waveformName parameter. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGRetrieveIQRate(HandleRef rfsgHandle, string channelString, string waveformName, out double iQRate)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGRetrieveIQRate(rfsgHandle, channelString, waveformName, out iQRate);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Checks the I/Q rate of all the waveforms in the script that you specify in the script parameter. This function returns the I/Q rate if the I/Q rates are the same for all the waveforms. If the I/Q rates are different, the function returns an error.
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "script">
        /// Specifies the NI-RFSG script used to generate the signal. The function looks up the I/Q rate of all the waveforms contained in the script.
        /// </param>
        /// <param name = "iQRate">
        /// Returns the I/Q rate if the I/Q rates are the same for all the waveforms that you specify in the script parameter. If the I/Q rates are different, the function returns an error.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGRetrieveIQRateAllWaveforms(HandleRef rfsgHandle, string channelString, string script, out double iQRate)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGRetrieveIQRateAllWaveforms(rfsgHandle, channelString, script, out iQRate);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Looks up the headroom of all the waveforms contained in the script and returns the minimum of all these headrooms, in dB.
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "script">
        /// Specifies the NI-RFSG script used to generate the signal.
        /// </param>
        /// <param name = "headroom">
        /// Returns the minimum headroom, in dB, of all the waveforms in the script that you specify in the script parameter. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGRetrieveMinimumHeadroomAllWaveforms(HandleRef rfsgHandle, string channelString, string script, out double headroom)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGRetrieveMinimumHeadroomAllWaveforms(rfsgHandle, channelString, script, out headroom);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Stores the headroom, which you specify in the headroom parameter, in the RFSG database. 
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name of the waveform for which you want to store the headroom. The toolkit uses the waveformName parameter as the key to store the waveform attributes in the RFSG database. 
        /// </param>
        /// <param name = "headroom">
        /// Specifies the headroom, in dB, to store in the RFSG database. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGStoreHeadroom(HandleRef rfsgHandle, string channelString, string waveformName, double headroom)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGStoreHeadroom(rfsgHandle, channelString, waveformName, headroom);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Stores the I/Q rate, which you specify in the IQRate parameter, in the RFSG database. 
        /// </summary>
        /// <param name = "rfsgHandle">
        /// Identifies the instrument session. The toolkit obtains this parameter from the niRFSG_init function or the niRFSG_InitWithOptions function.
        /// </param>
        /// <param name = "channelString">
        /// Set this parameter to "" (or String.Empty) or NULL.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name of the waveform for which you want to store the I/Q rate. The toolkit uses the waveformName parameter as the key to store the waveform attributes in the RFSG database. 
        /// </param>
        /// <param name = "iQRate">
        /// Specifies the I/Q rate to store in the RFSG database. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public int RFSGStoreIQRate(HandleRef rfsgHandle, string channelString, string waveformName, double iQRate)
        {
            int pInvokeResult = PInvoke.niLTESG_RFSGStoreIQRate(rfsgHandle, channelString, waveformName, iQRate);
            TestForError(rfsgHandle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Reads a waveform from a TDMS file. You can save this file using the Generation Interactive Example for LTE. The niLTESG_ReadWaveformFromFile function returns the I/Q complex waveform data that you can subsequently download to an RF vector signal generator.
        /// In addition to the I/Q complex waveform data, the Generation Interactive Example for LTE also saves the NILTESG_IQ_RATE and NILTESG_HEADROOM attributes of the waveform to the file. Use the niLTESG_ReadWaveformFromFile function or TDMS file attributes in your programming environment to read the values of these attributes. 
        /// The NI_RF_IQRate and NI_RF_Headroom attributes are located in the following locations within the TDMS file.
        /// Attribute Name
        /// Datatype
        /// Group Name
        /// Channel Name
        /// NI_RF_IQRate
        /// float64
        /// waveforms
        /// niLTE SG Waveform
        /// NI_RF_Headroom
        /// float64
        /// waveforms
        /// niLTE SG Waveform
        /// </summary>
        /// <param name = "filePath">
        /// Specifies the complete path of the TDMS file from which the toolkit reads the waveform.
        /// </param>
        /// <param name = "waveformName">
        /// Specifies the name of the waveform to read from the file.
        /// </param>
        /// <param name = "offset">
        /// Specifies the number of samples in the waveform at which the function begins reading the I/Q data.  The default value is 0. If you set count to 1,000 and offset to 2, the function returns 1,000 samples, starting from index 2 and ending at index 1,002. 
        /// </param>
        /// <param name = "count">
        /// Specifies the maximum number of samples of the I/Q complex waveform to read from the file. The default value is -1, which returns all samples. If you set count to 1000 and offset to 2, the function returns 1,000 samples, starting from index 2 and ending at index 1,002.
        /// </param>
        /// <param name = "t0">
        /// Returns the starting time, in seconds.
        /// </param>
        /// <param name = "dt">
        /// Returns the time interval between baseband I/Q samples, in seconds.
        /// </param>
        /// <param name = "waveform">
        /// Returns the LTE I/Q data. This parameter must be at least equal to the waveformSize parameter. You can pass NULL to the waveform parameter to query the size of the waveform.
        /// </param>
        /// <param name = "waveformSize">
        /// Specifies the waveform size, in samples.
        /// </param>
        /// <param name = "actualWaveformSize">
        /// Returns the actual size of the waveform.
        /// </param>
        /// <param name = "iQrate">
        /// Returns the I/Q rate, in samples per second (S/s), of the waveform.
        /// </param>
        /// <param name = "headroom">
        /// Returns the headroom, in dB, of the waveform.
        /// </param>
        /// <param name = "eof">
        /// specifies if the end of file has been reached with this read.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code either indicates success or describes an error or warning condition. 
        /// Examine the status code from each call to an niLTE generation function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niLTESA_GetErrorString function. 
        /// The general meaning of the status code is as follows:
        /// Value             Meaning 
        /// 0                 Success 
        /// Positive Values   Warnings 
        /// Negative Values   Exception
        /// </returns>
        public static int ReadWaveformFromFile(string filePath, string waveformName, Int64 offset, Int64 count, out double t0, out double dt, niComplexNumber[] waveform, int waveformSize, out int actualWaveformSize, out double iQrate, out double headroom, out int eof)
        {
            int pInvokeResult = PInvoke.niLTESG_ReadWaveformFromFile(filePath, waveformName, offset, count, out t0, out dt, waveform, waveformSize, out actualWaveformSize, out iQrate, out headroom, out eof);
            TestForStaticError(pInvokeResult);
            return pInvokeResult;
        }

        #region Public Get/Set functions exposed to user
        /// <summary>
        ///Specifies whether the toolkit calculates the headroom or uses the value that you specify in the NILTESG_HEADROOM attribute. For multiframe generation, the toolkit uses    the headroom calculated on the first frame to scale the waveform.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetAutoHeadroomEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.AutoHeadroomEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether the toolkit calculates the headroom or uses the value that you specify in the NILTESG_HEADROOM attribute. For multiframe generation, the toolkit uses    the headroom calculated on the first frame to scale the waveform.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetAutoHeadroomEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.AutoHeadroomEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies whether to add additive white Gaussian noise (AWGN) to the baseband waveform. The toolkit uses the value that you    specify in the NILTESG_CARRIER_TO_NOISE_RATIO attribute to add the AWGN.
        ///    Use an 'antennan' active channel string to configure or read this attribute for the nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int SetAwgnEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.AwgnEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to add additive white Gaussian noise (AWGN) to the baseband waveform. The toolkit uses the value that you    specify in the NILTESG_CARRIER_TO_NOISE_RATIO attribute to add the AWGN.
        ///    Use an 'antennan' active channel string to configure or read this attribute for the nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int GetAwgnEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.AwgnEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies whether the toolkit filters the created waveform.
        ///    The cut-off frequency of the baseband filter is between the occupied bandwidth of the signal and half the sample rate. For example,    for a fully filled signal with a bandwidth of 10 MHz, the occupied bandwidth is 9 MHz (-4.5 MHz to 4.5 MHz)  and the    sample rate is 15.36 MHz. Thus, the cut-off frequency of the filter is between 4.5 MHz and 7.68 MHz.    If the baseband filter is enabled, the adjacent channel power (ACP) leakage of the generated waveform reduces.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetBasebandFilterEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.BasebandFilterEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether the toolkit filters the created waveform.
        ///    The cut-off frequency of the baseband filter is between the occupied bandwidth of the signal and half the sample rate. For example,    for a fully filled signal with a bandwidth of 10 MHz, the occupied bandwidth is 9 MHz (-4.5 MHz to 4.5 MHz)  and the    sample rate is 15.36 MHz. Thus, the cut-off frequency of the filter is between 4.5 MHz and 7.68 MHz.    If the baseband filter is enabled, the adjacent channel power (ACP) leakage of the generated waveform reduces.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetBasebandFilterEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.BasebandFilterEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the carrier frequency offset, in hertz (Hz).
        ///    The default value is 0. Valid values are -120,000 to 120,000, inclusive.
        /// </summary>
        public int SetCarrierFrequencyOffset(string channel, double value)
        {
            return SetDouble(niLTESGProperties.CarrierFrequencyOffset, channel, value);
        }
        /// <summary>
        ///Specifies the carrier frequency offset, in hertz (Hz).
        ///    The default value is 0. Valid values are -120,000 to 120,000, inclusive.
        /// </summary>
        public int GetCarrierFrequencyOffset(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.CarrierFrequencyOffset, channel, out value);
        }

        /// <summary>
        ///Specifies the carrier-to-noise ratio (CNR) of the waveform generated. Noise bandwidth is equal to half the value of the NILTESG_RECOMMENDED_IQ_RATE attribute.    Configure the NILTESG_CARRIER_TO_NOISE_RATIO attribute only if you set the NILTESG_AWGN_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use an 'antennan' active channel string to configure or read this attribute for the nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 50. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int SetCarrierToNoiseRatio(string channel, double value)
        {
            return SetDouble(niLTESGProperties.CarrierToNoiseRatio, channel, value);
        }
        /// <summary>
        ///Specifies the carrier-to-noise ratio (CNR) of the waveform generated. Noise bandwidth is equal to half the value of the NILTESG_RECOMMENDED_IQ_RATE attribute.    Configure the NILTESG_CARRIER_TO_NOISE_RATIO attribute only if you set the NILTESG_AWGN_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use an 'antennan' active channel string to configure or read this attribute for the nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 50. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int GetCarrierToNoiseRatio(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.CarrierToNoiseRatio, channel, out value);
        }

        /// <summary>
        ///Specifies the physical layer cell identity as defined in section 6.11 of the 3GPP TS 36.211 v8.6.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 503, inclusive.
        /// </summary>
        public int SetCellId(string channel, int value)
        {
            return SetInt32(niLTESGProperties.CellId, channel, value);
        }
        /// <summary>
        ///Specifies the physical layer cell identity as defined in section 6.11 of the 3GPP TS 36.211 v8.6.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 503, inclusive.
        /// </summary>
        public int GetCellId(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.CellId, channel, out value);
        }

        /// <summary>
        ///Specifies the clip rate that the toolkit uses to clip the waveform.
        ///    The clip rate is expressed as a percentage of the number of samples. The toolkit ignores this attribute if you set the NILTESG_AUTO_HEADROOM_ENABLED attribute    to NILTESG_VAL_FALSE. If you set the clip rate to 0, the power scaling ensures that none of the samples in the created waveform are clipped.    If you set the clip rate to 10%, the toolkit clips the top 10% of the samples based on the complimentary cumulative distribution function (CCDF) of the signal.
        ///    The default value is 0. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int SetClipRate(string channel, double value)
        {
            return SetDouble(niLTESGProperties.ClipRate, channel, value);
        }
        /// <summary>
        ///Specifies the clip rate that the toolkit uses to clip the waveform.
        ///    The clip rate is expressed as a percentage of the number of samples. The toolkit ignores this attribute if you set the NILTESG_AUTO_HEADROOM_ENABLED attribute    to NILTESG_VAL_FALSE. If you set the clip rate to 0, the power scaling ensures that none of the samples in the created waveform are clipped.    If you set the clip rate to 10%, the toolkit clips the top 10% of the samples based on the complimentary cumulative distribution function (CCDF) of the signal.
        ///    The default value is 0. Valid values are 0 to 100, inclusive.
        /// </summary>
        public int GetClipRate(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ClipRate, channel, out value);
        }

        /// <summary>
        ///Specifies the cyclic prefix mode as defined in section 5.2.3 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The default value is NILTESG_VAL_CYCLIC_PREFIX_MODE_NORMAL.
        /// </summary>
        public int SetCyclicPrefixMode(string channel, int value)
        {
            return SetInt32(niLTESGProperties.CyclicPrefixMode, channel, value);
        }
        /// <summary>
        ///Specifies the cyclic prefix mode as defined in section 5.2.3 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The default value is NILTESG_VAL_CYCLIC_PREFIX_MODE_NORMAL.
        /// </summary>
        public int GetCyclicPrefixMode(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.CyclicPrefixMode, channel, out value);
        }

        /// <summary>
        ///Specifies the ratio (Rho_B/Rho_A) as described in Table 5.2-1 in section 5.2 of the 3GPP TS 36.213 v8.6.0 specifications for the    cell-specific ratio (R_b/R_a) of one, two, or four cell-specific antenna ports.
        ///    This attribute determines the power of the channel resource element (RE) in the symbols that do not contain the reference symbols.
        ///    The default value is NILTESG_VAL_DL_CELL_SPECIFIC_RATIO_P_B_0.
        /// </summary>
        public int SetDlCellSpecificRatio(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlCellSpecificRatio, channel, value);
        }
        /// <summary>
        ///Specifies the ratio (Rho_B/Rho_A) as described in Table 5.2-1 in section 5.2 of the 3GPP TS 36.213 v8.6.0 specifications for the    cell-specific ratio (R_b/R_a) of one, two, or four cell-specific antenna ports.
        ///    This attribute determines the power of the channel resource element (RE) in the symbols that do not contain the reference symbols.
        ///    The default value is NILTESG_VAL_DL_CELL_SPECIFIC_RATIO_P_B_0.
        /// </summary>
        public int GetDlCellSpecificRatio(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlCellSpecificRatio, channel, out value);
        }

        /// <summary>
        ///Specifies whether the frame contains a cell-specific reference signal.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlCellSpecificReferenceSignalsEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlCellSpecificReferenceSignalsEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether the frame contains a cell-specific reference signal.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlCellSpecificReferenceSignalsEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlCellSpecificReferenceSignalsEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the number of physical control format indicator channels (PCFICHs) that you can configure in a frame.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetDlNumberOfPcfichChannels(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlNumberOfPcfichChannels, channel, value);
        }
        /// <summary>
        ///Specifies the number of physical control format indicator channels (PCFICHs) that you can configure in a frame.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetDlNumberOfPcfichChannels(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlNumberOfPcfichChannels, channel, out value);
        }

        /// <summary>
        ///Specifies the number of physical downlink control channel (PDCCHs) that you can configure in a frame. You can configure a maximum of 10 PDCCH channels in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetDlNumberOfPdcchChannels(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlNumberOfPdcchChannels, channel, value);
        }
        /// <summary>
        ///Specifies the number of physical downlink control channel (PDCCHs) that you can configure in a frame. You can configure a maximum of 10 PDCCH channels in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetDlNumberOfPdcchChannels(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlNumberOfPdcchChannels, channel, out value);
        }

        /// <summary>
        ///Specifies the number of physical downlink shared channels (PDSCHs) that you can configure in a frame. You can configure a maximum of 10 PDSCH channels in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetDlNumberOfPdschChannels(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlNumberOfPdschChannels, channel, value);
        }
        /// <summary>
        ///Specifies the number of physical downlink shared channels (PDSCHs) that you can configure in a frame. You can configure a maximum of 10 PDSCH channels in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetDlNumberOfPdschChannels(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlNumberOfPdschChannels, channel, out value);
        }

        /// <summary>
        ///Specifies the number of physical hybrid indicator channels (PHICHs) that you can configure in a frame.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetDlNumberOfPhichChannels(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlNumberOfPhichChannels, channel, value);
        }
        /// <summary>
        ///Specifies the number of physical hybrid indicator channels (PHICHs) that you can configure in a frame.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetDlNumberOfPhichChannels(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlNumberOfPhichChannels, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable the orthogonal frequency division multiple access (OFDMA) channel noise generator (OCNG). Each unused physical resource block (PRB) is    assigned to an individual virtual user equipment (UE). The data for each virtual UE is uncorrelated with data from the other virtual    UEs for the duration of the measurement. The data is quadrature phase-shift keying (QPSK) modulated. Refer to section 8.2.2.1.4 of the 3GPP TS 36.101    v8.6.0 specifications for more information about this attribute.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int SetDlOcngEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlOcngEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable the orthogonal frequency division multiple access (OFDMA) channel noise generator (OCNG). Each unused physical resource block (PRB) is    assigned to an individual virtual user equipment (UE). The data for each virtual UE is uncorrelated with data from the other virtual    UEs for the duration of the measurement. The data is quadrature phase-shift keying (QPSK) modulated. Refer to section 8.2.2.1.4 of the 3GPP TS 36.101    v8.6.0 specifications for more information about this attribute.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int GetDlOcngEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlOcngEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_a.
        ///    The default value is 0.
        /// </summary>
        public int SetDlOcngPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlOcngPower, channel, value);
        }
        /// <summary>
        ///Specifies the power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_a.
        ///    The default value is 0.
        /// </summary>
        public int GetDlOcngPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlOcngPower, channel, out value);
        }

        /// <summary>
        ///Specifies whether the frame contains a physical broadcast channel (PBCH).
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPbchEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPbchEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether the frame contains a physical broadcast channel (PBCH).
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPbchEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPbchEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the system frame number in the created waveform. Configure this attribute if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD.
        ///    The default value is 0. Valid values are 0 to 1,023, inclusive.
        /// </summary>
        public int SetDlPbchInitialSystemFrameNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPbchInitialSystemFrameNumber, channel, value);
        }
        /// <summary>
        ///Specifies the system frame number in the created waveform. Configure this attribute if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD.
        ///    The default value is 0. Valid values are 0 to 1,023, inclusive.
        /// </summary>
        public int GetDlPbchInitialSystemFrameNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPbchInitialSystemFrameNumber, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to transmit on the physical broadcast channel (PBCH).
        ///    The default value is NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD.
        /// </summary>
        public int SetDlPbchPayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPbchPayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to transmit on the physical broadcast channel (PBCH).
        ///    The default value is NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD.
        /// </summary>
        public int GetDlPbchPayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPbchPayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical broadcast channel (PBCH) payload. Configure this attribute only if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    The default value is 9.
        /// </summary>
        public int SetDlPbchPayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPbchPayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical broadcast channel (PBCH) payload. Configure this attribute only if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    The default value is 9.
        /// </summary>
        public int GetDlPbchPayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPbchPayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the physical broadcast channel (PBCH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    The default value is -692093454.
        /// </summary>
        public int SetDlPbchPayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPbchPayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the physical broadcast channel (PBCH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    The default value is -692093454.
        /// </summary>
        public int GetDlPbchPayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPbchPayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    The toolkit uses 24 bits for the channel coding. If you specify a pattern that is fewer than 24 bits, the toolkit repeats the data.
        /// </summary>
        public int SetDlPbchPayloadUserDefinedBits(string channel, int[] value)
        {
            return SetArrayInt32(niLTESGProperties.DlPbchPayloadUserDefinedBits, channel, value);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    The toolkit uses 24 bits for the channel coding. If you specify a pattern that is fewer than 24 bits, the toolkit repeats the data.
        /// </summary>
        public int GetDlPbchPayloadUserDefinedBits(string channel, int[] value, out int actualNumValues)
        {
            return GetArrayInt32(niLTESGProperties.DlPbchPayloadUserDefinedBits, channel, value, out actualNumValues);
        }

        /// <summary>
        ///Specifies the PBCH power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_b.
        ///    The default value is 0.
        /// </summary>
        public int SetDlPbchPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPbchPower, channel, value);
        }
        /// <summary>
        ///Specifies the PBCH power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_b.
        ///    The default value is 0.
        /// </summary>
        public int GetDlPbchPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPbchPower, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable scrambling for  physical broadcast channel (PBCH) transmission. Refer to section 6.6.1 of the 3GPP TS 36.211 v8.6.0 specifications for more information about enabling scrambling for PBCH transmission.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPbchScramblingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPbchScramblingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable scrambling for  physical broadcast channel (PBCH) transmission. Refer to section 6.6.1 of the 3GPP TS 36.211 v8.6.0 specifications for more information about enabling scrambling for PBCH transmission.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPbchScramblingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPbchScramblingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the control format indicator (CFI) that determines the number of symbols used for control information. Refer to section    5.3.4 of the 3GPP TS 36.212 v8.6.0 specifications for more information about this attribute.
        ///    Use a 'pcfich' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 1. Valid values are 1 to 4, inclusive.
        /// </summary>
        public int SetDlPcfichControlFormatIndicator(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPcfichControlFormatIndicator, channel, value);
        }
        /// <summary>
        ///Specifies the control format indicator (CFI) that determines the number of symbols used for control information. Refer to section    5.3.4 of the 3GPP TS 36.212 v8.6.0 specifications for more information about this attribute.
        ///    Use a 'pcfich' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 1. Valid values are 1 to 4, inclusive.
        /// </summary>
        public int GetDlPcfichControlFormatIndicator(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPcfichControlFormatIndicator, channel, out value);
        }

        /// <summary>
        ///Specifies the PCFICH power level (R_a), in dB, relative to the power of the cell-specific reference signal. 
        ///    The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_b.
        ///    Use a 'pcfich' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetDlPcfichPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPcfichPower, channel, value);
        }
        /// <summary>
        ///Specifies the PCFICH power level (R_a), in dB, relative to the power of the cell-specific reference signal. 
        ///    The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_b.
        ///    Use a 'pcfich' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetDlPcfichPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPcfichPower, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable scrambling for physical control format indicator channel (PCFICH) transmission.
        ///    Use a 'pcfichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPcfichScramblingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPcfichScramblingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable scrambling for physical control format indicator channel (PCFICH) transmission.
        ///    Use a 'pcfichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPcfichScramblingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPcfichScramblingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe number for physical control format indicator channel (PCFICH) transmission.
        ///    Use a 'pcfichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int SetDlPcfichSubframeNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPcfichSubframeNumber, channel, value);
        }
        /// <summary>
        ///Specifies the subframe number for physical control format indicator channel (PCFICH) transmission.
        ///    Use a 'pcfichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int GetDlPcfichSubframeNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPcfichSubframeNumber, channel, out value);
        }

        /// <summary>
        ///Specifies the control channel element (CCE) start index to use for mapping the physical downlink control channel (PDCCH) resource elements. If the specified CCE start index    is not available, the toolkit uses the next available CCE start index.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetDlPdcchCceStartIndex(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchCceStartIndex, channel, value);
        }
        /// <summary>
        ///Specifies the control channel element (CCE) start index to use for mapping the physical downlink control channel (PDCCH) resource elements. If the specified CCE start index    is not available, the toolkit uses the next available CCE start index.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetDlPdcchCceStartIndex(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchCceStartIndex, channel, out value);
        }

        /// <summary>
        ///Specifies the size of the control channel elements (CCEs) for the physical downlink control channel (PDCCH) channel. Refer to section 6.8.1 of the 3GPP TS 36.211 v8.6.0 specifications for more information about this attribute. 
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_DL_PDCCH_FORMAT_0.
        /// </summary>
        public int SetDlPdcchFormat(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchFormat, channel, value);
        }
        /// <summary>
        ///Specifies the size of the control channel elements (CCEs) for the physical downlink control channel (PDCCH) channel. Refer to section 6.8.1 of the 3GPP TS 36.211 v8.6.0 specifications for more information about this attribute. 
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_DL_PDCCH_FORMAT_0.
        /// </summary>
        public int GetDlPdcchFormat(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchFormat, channel, out value);
        }

        /// <summary>
        ///Specifies the power level, in dB, for the physical downlink control channel (PDCCH) nil resource elements, relative to the power of the cell-specific reference signal.
        ///    The default value is 0.
        /// </summary>
        public int SetDlPdcchNilElementPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPdcchNilElementPower, channel, value);
        }
        /// <summary>
        ///Specifies the power level, in dB, for the physical downlink control channel (PDCCH) nil resource elements, relative to the power of the cell-specific reference signal.
        ///    The default value is 0.
        /// </summary>
        public int GetDlPdcchNilElementPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPdcchNilElementPower, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to transmit on the physical downlink control channel (PDCCH).
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int SetDlPdcchPayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchPayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to transmit on the physical downlink control channel (PDCCH).
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int GetDlPdcchPayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchPayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the PN order for the physical downlink control channel (PDCCH) payload. Configure this attribute only if you set the NILTESG_DL_PDCCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int SetDlPdcchPayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchPayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the PN order for the physical downlink control channel (PDCCH) payload. Configure this attribute only if you set the NILTESG_DL_PDCCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int GetDlPdcchPayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchPayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the physical downlink control channel (PDCCH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PDCCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int SetDlPdcchPayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchPayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the physical downlink control channel (PDCCH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PDCCH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int GetDlPdcchPayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchPayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PDCCH_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    The toolkit repeats the user-defined bits if the number of bits are fewer than the required number of bits and removes the extra user-defined bits.    For example, if you specify a physical downlink control channel (PDCCH) format of 0, the expected number of user defined bits is 72; if the number of user-defined bits    is less than 72, the toolkit repeats the user-defined bit pattern. If the number of user-defined bits is greater than 72, the toolkit removes the extra bits.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetDlPdcchPayloadUserDefinedBits(string channel, int[] value)
        {
            return SetArrayInt32(niLTESGProperties.DlPdcchPayloadUserDefinedBits, channel, value);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PDCCH_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    The toolkit repeats the user-defined bits if the number of bits are fewer than the required number of bits and removes the extra user-defined bits.    For example, if you specify a physical downlink control channel (PDCCH) format of 0, the expected number of user defined bits is 72; if the number of user-defined bits    is less than 72, the toolkit repeats the user-defined bit pattern. If the number of user-defined bits is greater than 72, the toolkit removes the extra bits.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetDlPdcchPayloadUserDefinedBits(string channel, int[] value, out int actualNumValues)
        {
            return GetArrayInt32(niLTESGProperties.DlPdcchPayloadUserDefinedBits, channel, value, out actualNumValues);
        }

        /// <summary>
        ///Specifies the physical downlink control channel (PDCCH) power level, in dB, relative to the power of the cell-specific reference signal.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetDlPdcchPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPdcchPower, channel, value);
        }
        /// <summary>
        ///Specifies the physical downlink control channel (PDCCH) power level, in dB, relative to the power of the cell-specific reference signal.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetDlPdcchPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPdcchPower, channel, out value);
        }

        /// <summary>
        ///Specifies the radio network temporary identifier (RNTI) for physical downlink control channel (PDCCH) transmission.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 65,535, inclusive.
        /// </summary>
        public int SetDlPdcchRnti(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchRnti, channel, value);
        }
        /// <summary>
        ///Specifies the radio network temporary identifier (RNTI) for physical downlink control channel (PDCCH) transmission.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 65,535, inclusive.
        /// </summary>
        public int GetDlPdcchRnti(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchRnti, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable scrambling for physical downlink control channel (PDCCH) transmission.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPdcchScramblingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchScramblingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable scrambling for physical downlink control channel (PDCCH) transmission.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPdcchScramblingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchScramblingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe number for physical downlink control channel (PDCCH) transmission.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int SetDlPdcchSubframeNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdcchSubframeNumber, channel, value);
        }
        /// <summary>
        ///Specifies the subframe number for physical downlink control channel (PDCCH) transmission.
        ///    Use a 'pdcchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int GetDlPdcchSubframeNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdcchSubframeNumber, channel, out value);
        }

        /// <summary>
        ///Specifies the modulation scheme for the first codeword of physical downlink shared channel (PDSCH) transmission. Refer to section 7.1 of the 3GPP TS 36.211    v8.6.0 specifications for more information.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        /// </summary>
        public int SetDlPdschCodeword0ModulationScheme(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword0ModulationScheme, channel, value);
        }
        /// <summary>
        ///Specifies the modulation scheme for the first codeword of physical downlink shared channel (PDSCH) transmission. Refer to section 7.1 of the 3GPP TS 36.211    v8.6.0 specifications for more information.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        /// </summary>
        public int GetDlPdschCodeword0ModulationScheme(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword0ModulationScheme, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to transmit on the first codeword of physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int SetDlPdschCodeword0PayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword0PayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to transmit on the first codeword of physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int GetDlPdschCodeword0PayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword0PayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the PN order for the codeword 0 payload. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD0_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int SetDlPdschCodeword0PayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword0PayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the PN order for the codeword 0 payload. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD0_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int GetDlPdschCodeword0PayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword0PayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the codeword 0 pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD0_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int SetDlPdschCodeword0PayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword0PayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the codeword 0 pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD0_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int GetDlPdschCodeword0PayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword0PayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD0_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetDlPdschCodeword0PayloadUserDefinedBits(string channel, int[] value)
        {
            return SetArrayInt32(niLTESGProperties.DlPdschCodeword0PayloadUserDefinedBits, channel, value);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD0_PAYLOAD_DATA_TYPE    attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetDlPdschCodeword0PayloadUserDefinedBits(string channel, int[] value, out int actualNumValues)
        {
            return GetArrayInt32(niLTESGProperties.DlPdschCodeword0PayloadUserDefinedBits, channel, value, out actualNumValues);
        }

        /// <summary>
        ///Specifies whether to enable the second physical downlink shared channel (PDSCH) codeword. The toolkit ignores this attribute if you set the NILTESG_DL_PDSCH_TRANSMISSION_MODE    attribute to NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SINGLE_ANTENNA.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int SetDlPdschCodeword1Enabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword1Enabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable the second physical downlink shared channel (PDSCH) codeword. The toolkit ignores this attribute if you set the NILTESG_DL_PDSCH_TRANSMISSION_MODE    attribute to NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SINGLE_ANTENNA.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int GetDlPdschCodeword1Enabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword1Enabled, channel, out value);
        }

        /// <summary>
        ///Specifies the modulation scheme for the second codeword of physical downlink shared channel (PDSCH) transmission. Configure this attribute only if you set the    NILTESG_DL_PDSCH_CODEWORD1_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        /// </summary>
        public int SetDlPdschCodeword1ModulationScheme(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword1ModulationScheme, channel, value);
        }
        /// <summary>
        ///Specifies the modulation scheme for the second codeword of physical downlink shared channel (PDSCH) transmission. Configure this attribute only if you set the    NILTESG_DL_PDSCH_CODEWORD1_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        /// </summary>
        public int GetDlPdschCodeword1ModulationScheme(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword1ModulationScheme, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to transmit on the second codeword of physical downlink shared channel (PDSCH) transmission. Configure this attribute only    if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int SetDlPdschCodeword1PayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword1PayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to transmit on the second codeword of physical downlink shared channel (PDSCH) transmission. Configure this attribute only    if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int GetDlPdschCodeword1PayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword1PayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the pseudonoise (PN) order for the codeword 1 payload. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED    attribute to NILTESG_VAL_TRUE and the NILTESG_DL_PDSCH_CODEWORD1_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int SetDlPdschCodeword1PayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword1PayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the pseudonoise (PN) order for the codeword 1 payload. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED    attribute to NILTESG_VAL_TRUE and the NILTESG_DL_PDSCH_CODEWORD1_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int GetDlPdschCodeword1PayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword1PayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the codeword 1 pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED    attribute to NILTESG_VAL_TRUE and the NILTESG_DL_PDSCH_CODEWORD1_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int SetDlPdschCodeword1PayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschCodeword1PayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the codeword 1 pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED    attribute to NILTESG_VAL_TRUE and the NILTESG_DL_PDSCH_CODEWORD1_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int GetDlPdschCodeword1PayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschCodeword1PayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED attribute to NILTESG_VAL_TRUE    and the NILTESG_DL_PDSCH_CODEWORD1_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetDlPdschCodeword1PayloadUserDefinedBits(string channel, int[] value)
        {
            return SetArrayInt32(niLTESGProperties.DlPdschCodeword1PayloadUserDefinedBits, channel, value);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the NILTESG_DL_PDSCH_CODEWORD1_ENABLED attribute to NILTESG_VAL_TRUE    and the NILTESG_DL_PDSCH_CODEWORD1_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetDlPdschCodeword1PayloadUserDefinedBits(string channel, int[] value, out int actualNumValues)
        {
            return GetArrayInt32(niLTESGProperties.DlPdschCodeword1PayloadUserDefinedBits, channel, value, out actualNumValues);
        }

        /// <summary>
        ///Specifies the number of layers for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdsch' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 1. Valid values are 1 to 4, inclusive.
        /// </summary>
        public int SetDlPdschNumberOfLayers(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschNumberOfLayers, channel, value);
        }
        /// <summary>
        ///Specifies the number of layers for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdsch' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 1. Valid values are 1 to 4, inclusive.
        /// </summary>
        public int GetDlPdschNumberOfLayers(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschNumberOfLayers, channel, out value);
        }

        /// <summary>
        ///Specifies the physical downlink shared channel (PDSCH) power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_a.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetDlPdschPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPdschPower, channel, value);
        }
        /// <summary>
        ///Specifies the physical downlink shared channel (PDSCH) power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_a.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetDlPdschPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPdschPower, channel, out value);
        }

        /// <summary>
        ///Specifies the precoding codebook index for spatial multiplexing. Configure this attribute only if you set the NILTESG_DL_PDSCH_TRANSMISSION_MODE    attribute to NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SPATIAL_MULTIPLEXING. The toolkit ignores the NILTESG_DL_PDSCH_PRECODING_CODEBOOK_INDEX attribute    if you set the NILTESG_DL_PDSCH_PRECODING_MODE attribute to NILTESG_VAL_DL_PDSCH_PRECODING_MODE_WITH_CDD. 
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values 0 to 15, inclusive.
        /// </summary>
        public int SetDlPdschPrecodingCodebookIndex(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschPrecodingCodebookIndex, channel, value);
        }
        /// <summary>
        ///Specifies the precoding codebook index for spatial multiplexing. Configure this attribute only if you set the NILTESG_DL_PDSCH_TRANSMISSION_MODE    attribute to NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SPATIAL_MULTIPLEXING. The toolkit ignores the NILTESG_DL_PDSCH_PRECODING_CODEBOOK_INDEX attribute    if you set the NILTESG_DL_PDSCH_PRECODING_MODE attribute to NILTESG_VAL_DL_PDSCH_PRECODING_MODE_WITH_CDD. 
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values 0 to 15, inclusive.
        /// </summary>
        public int GetDlPdschPrecodingCodebookIndex(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschPrecodingCodebookIndex, channel, out value);
        }

        /// <summary>
        ///Specifies the precoding mode. Configure this attribute only if you set the NILTESG_DL_PDSCH_TRANSMISSION_MODE attribute to    NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SPATIAL_MULTIPLEXING.    The toolkit ignores this attribute if you set the NILTESG_NUMBER_OF_ANTENNAS attribute to 1.
        ///    Use a 'pdsch' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_DL_PDSCH_PRECODING_MODE_WITH_CDD.
        /// </summary>
        public int SetDlPdschPrecodingMode(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschPrecodingMode, channel, value);
        }
        /// <summary>
        ///Specifies the precoding mode. Configure this attribute only if you set the NILTESG_DL_PDSCH_TRANSMISSION_MODE attribute to    NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SPATIAL_MULTIPLEXING.    The toolkit ignores this attribute if you set the NILTESG_NUMBER_OF_ANTENNAS attribute to 1.
        ///    Use a 'pdsch' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_DL_PDSCH_PRECODING_MODE_WITH_CDD.
        /// </summary>
        public int GetDlPdschPrecodingMode(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschPrecodingMode, channel, out value);
        }

        /// <summary>
        ///Specifies the radio network temporary identifier (RNTI) for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 65,535, inclusive.
        /// </summary>
        public int SetDlPdschRnti(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschRnti, channel, value);
        }
        /// <summary>
        ///Specifies the radio network temporary identifier (RNTI) for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 65,535, inclusive.
        /// </summary>
        public int GetDlPdschRnti(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschRnti, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable scrambling for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdsch' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPdschScramblingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschScramblingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable scrambling for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdsch' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPdschScramblingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschScramblingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe number for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int SetDlPdschSubframeNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschSubframeNumber, channel, value);
        }
        /// <summary>
        ///Specifies the subframe number for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int GetDlPdschSubframeNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschSubframeNumber, channel, out value);
        }

        /// <summary>
        ///Specifies the transmission mode for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SINGLE_ANTENNA.
        /// </summary>
        public int SetDlPdschTransmissionMode(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPdschTransmissionMode, channel, value);
        }
        /// <summary>
        ///Specifies the transmission mode for physical downlink shared channel (PDSCH) transmission.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_DL_PDSCH_TRANSMISSION_MODE_SINGLE_ANTENNA.
        /// </summary>
        public int GetDlPdschTransmissionMode(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPdschTransmissionMode, channel, out value);
        }

        /// <summary>
        ///Specifies the virtual resource block (VRB) allocation in VRBs for the current physical downlink shared channel (PDSCH) channel.
        ///    For example, if the VRB allocation is '0-5,7,9,10-15', the VRB allocation string specifies contiguous VRBs from 0 to 5, VRB 7, VRB 9,    and contiguous VRBs from 10 to 15.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetDlPdschVirtualResourceBlockAllocation(string channel, string value)
        {
            return SetString(niLTESGProperties.DlPdschVirtualResourceBlockAllocation, channel, value);
        }
        /// <summary>
        ///Specifies the virtual resource block (VRB) allocation in VRBs for the current physical downlink shared channel (PDSCH) channel.
        ///    For example, if the VRB allocation is '0-5,7,9,10-15', the VRB allocation string specifies contiguous VRBs from 0 to 5, VRB 7, VRB 9,    and contiguous VRBs from 10 to 15.
        ///    Use a 'pdschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetDlPdschVirtualResourceBlockAllocation(string channel, StringBuilder value, out int numChars)
        {
            return GetString(channel, niLTESGProperties.DlPdschVirtualResourceBlockAllocation, value, value.Length, out numChars);
        }

        /// <summary>
        ///Specifies the duration of the physical hybrid indicator channel (PHICH) as defined in section 6.9 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The default value is NILTESG_VAL_DL_PHICH_DURATION_NORMAL.
        /// </summary>
        public int SetDlPhichDuration(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichDuration, channel, value);
        }
        /// <summary>
        ///Specifies the duration of the physical hybrid indicator channel (PHICH) as defined in section 6.9 of the 3GPP TS 36.211 v8.6.0 specifications.
        ///    The default value is NILTESG_VAL_DL_PHICH_DURATION_NORMAL.
        /// </summary>
        public int GetDlPhichDuration(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichDuration, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to transmit on the physical hybrid indicator channel (PHICH). Refer to section 5.3.5 of the 3GPP TS 36.212 v8.6.0 specifications for more information about this attribute. 
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int SetDlPhichPayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichPayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to transmit on the physical hybrid indicator channel (PHICH). Refer to section 5.3.5 of the 3GPP TS 36.212 v8.6.0 specifications for more information about this attribute. 
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int GetDlPhichPayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichPayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical hybrid indicator channel (PHICH) payload. Configure this attribute only if you set the NILTESG_DL_PHICH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 9.
        /// </summary>
        public int SetDlPhichPayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichPayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical hybrid indicator channel (PHICH) payload. Configure this attribute only if you set the NILTESG_DL_PHICH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 9.
        /// </summary>
        public int GetDlPhichPayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichPayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the physical hybrid indicator channel (PHICH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PHICH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is -692,093,454.
        /// </summary>
        public int SetDlPhichPayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichPayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the physical hybrid indicator channel (PHICH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_DL_PHICH_PAYLOAD_DATA_TYPE attribute    to NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is -692,093,454.
        /// </summary>
        public int GetDlPhichPayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichPayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of 0s (ACK), 1s (NACK), and -1s (DTX). Configure this attribute only    if you set the NILTESG_DL_PHICH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_USER_DEFINED_DATA.
        ///    In the array, the number of groups is equal to the number of rows, and the number of elements per physical hybrid indicator channel (PHICH) group is equal to the number of columns.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetDlPhichPayloadUserDefinedData(string channel, int[,] value, int numRows, int numColumns)
        {
            return Set2DArrayInt32(niLTESGProperties.DlPhichPayloadUserDefinedData, channel, value, numRows, numColumns);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of 0s (ACK), 1s (NACK), and -1s (DTX). Configure this attribute only    if you set the NILTESG_DL_PHICH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_DL_PHICH_PAYLOAD_DATA_TYPE_USER_DEFINED_DATA.
        ///    In the array, the number of groups is equal to the number of rows, and the number of elements per physical hybrid indicator channel (PHICH) group is equal to the number of columns.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetDlPhichPayloadUserDefinedData(string channel, int[,] value, int numRows, int numColumns)
        {
            return Get2DArrayInt32(niLTESGProperties.DlPhichPayloadUserDefinedData, channel, value, numRows, numColumns);
        }

        /// <summary>
        ///Specifies the physical hybrid indicator channel (PHICH) power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_b.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels. 
        ///    The default value is 0.
        /// </summary>
        public int SetDlPhichPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPhichPower, channel, value);
        }
        /// <summary>
        ///Specifies the physical hybrid indicator channel (PHICH) power level (R_a), in dB, relative to the power of the cell-specific reference signal. The toolkit uses the    NILTESG_DL_CELL_SPECIFIC_RATIO attribute to calculate R_b. Refer to section 3.3 of the 3GPP TS 36.521 v8.6.0 specifications for more information    about R_b.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels. 
        ///    The default value is 0.
        /// </summary>
        public int GetDlPhichPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPhichPower, channel, out value);
        }

        /// <summary>
        ///Specifies the ratio (N_g) that decides the number of physical hybrid indicator channel (PHICH) groups within a subframe. Refer to section 6.9 of the 3GPP TS 36.211 v8.6.0 specifications for more information about this attribute. 
        ///    The default value is NILTESG_VAL_DL_PHICH_RESOURCE_1.
        /// </summary>
        public int SetDlPhichResource(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichResource, channel, value);
        }
        /// <summary>
        ///Specifies the ratio (N_g) that decides the number of physical hybrid indicator channel (PHICH) groups within a subframe. Refer to section 6.9 of the 3GPP TS 36.211 v8.6.0 specifications for more information about this attribute. 
        ///    The default value is NILTESG_VAL_DL_PHICH_RESOURCE_1.
        /// </summary>
        public int GetDlPhichResource(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichResource, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable scrambling for physical hybrid indicator channel (PHICH) transmission. Refer to section 6.9 of the 3GPP TS 36.211 v8.6.0 specifications    for more information.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPhichScramblingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichScramblingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable scrambling for physical hybrid indicator channel (PHICH) transmission. Refer to section 6.9 of the 3GPP TS 36.211 v8.6.0 specifications    for more information.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPhichScramblingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichScramblingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe number for physical hybrid indicator channel (PHICH) transmission.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int SetDlPhichSubframeNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPhichSubframeNumber, channel, value);
        }
        /// <summary>
        ///Specifies the subframe number for physical hybrid indicator channel (PHICH) transmission.
        ///    Use a 'phichn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int GetDlPhichSubframeNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPhichSubframeNumber, channel, out value);
        }

        /// <summary>
        ///Specifies whether the frame contains a primary synchronization signal.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlPrimarySyncEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlPrimarySyncEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether the frame contains a primary synchronization signal.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlPrimarySyncEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlPrimarySyncEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the power level, in dB, for the primary synchronization signal, relative to the power of the cell-specific reference signal.
        ///    The default value is 0.
        /// </summary>
        public int SetDlPrimarySyncPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlPrimarySyncPower, channel, value);
        }
        /// <summary>
        ///Specifies the power level, in dB, for the primary synchronization signal, relative to the power of the cell-specific reference signal.
        ///    The default value is 0.
        /// </summary>
        public int GetDlPrimarySyncPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlPrimarySyncPower, channel, out value);
        }

        /// <summary>
        ///Specifies whether the frame contains a secondary synchronization signal.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetDlSecondarySyncEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlSecondarySyncEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether the frame contains a secondary synchronization signal.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetDlSecondarySyncEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlSecondarySyncEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the power level, in dB, for the secondary synchronization signal, relative to the power of the cell-specific reference signal.
        ///    The default value is 0.
        /// </summary>
        public int SetDlSecondarySyncPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.DlSecondarySyncPower, channel, value);
        }
        /// <summary>
        ///Specifies the power level, in dB, for the secondary synchronization signal, relative to the power of the cell-specific reference signal.
        ///    The default value is 0.
        /// </summary>
        public int GetDlSecondarySyncPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.DlSecondarySyncPower, channel, out value);
        }

        /// <summary>
        ///Specifies the antenna port used to transmit the synchronization signals.
        ///    In MIMO generation, this attribute determines the ports for which the toolkit generates the synchronization signal. For example,    in a 2 x 2 MIMO generation mode, you can set this attribute to NILTESG_VAL_DL_SYNC_SIGNAL_PORT_0, NILTESG_VAL_DL_SYNC_SIGNAL_PORT_1,    or NILTESG_VAL_DL_SYNC_SIGNAL_PORT_ALL. If you set this attribute to    NILTESG_VAL_DL_SYNC_SIGNAL_PORT_ALL, the energy of the synchronization signals is distributed equally on all the antennas that you    configure using    the NILTESG_NUMBER_OF_ANTENNAS attribute. The toolkit returns an error if the port number exceeds the number of antennas available.
        ///    The default value is NILTESG_VAL_DL_SYNC_SIGNAL_PORT_0.
        /// </summary>
        public int SetDlSyncSignalPort(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DlSyncSignalPort, channel, value);
        }
        /// <summary>
        ///Specifies the antenna port used to transmit the synchronization signals.
        ///    In MIMO generation, this attribute determines the ports for which the toolkit generates the synchronization signal. For example,    in a 2 x 2 MIMO generation mode, you can set this attribute to NILTESG_VAL_DL_SYNC_SIGNAL_PORT_0, NILTESG_VAL_DL_SYNC_SIGNAL_PORT_1,    or NILTESG_VAL_DL_SYNC_SIGNAL_PORT_ALL. If you set this attribute to    NILTESG_VAL_DL_SYNC_SIGNAL_PORT_ALL, the energy of the synchronization signals is distributed equally on all the antennas that you    configure using    the NILTESG_NUMBER_OF_ANTENNAS attribute. The toolkit returns an error if the port number exceeds the number of antennas available.
        ///    The default value is NILTESG_VAL_DL_SYNC_SIGNAL_PORT_0.
        /// </summary>
        public int GetDlSyncSignalPort(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DlSyncSignalPort, channel, out value);
        }

        /// <summary>
        ///Specifies the direction and the duplexing technique that the toolkit uses to create the waveform.
        ///    The default value is NILTESG_VAL_DUPLEX_MODE_UL_FDD.
        /// </summary>
        public int SetDuplexMode(string channel, int value)
        {
            return SetInt32(niLTESGProperties.DuplexMode, channel, value);
        }
        /// <summary>
        ///Specifies the direction and the duplexing technique that the toolkit uses to create the waveform.
        ///    The default value is NILTESG_VAL_DUPLEX_MODE_UL_FDD.
        /// </summary>
        public int GetDuplexMode(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.DuplexMode, channel, out value);
        }

        /// <summary>
        ///Specifies the headroom, in dB, per antenna. The toolkit ignores this attribute if you set the NILTESG_AUTO_HEADROOM_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    The default value is 0. 
        /// </summary>
        public int SetHeadroom(string channel, double value)
        {
            return SetDouble(niLTESGProperties.Headroom, channel, value);
        }
        /// <summary>
        ///Specifies the headroom, in dB, per antenna. The toolkit ignores this attribute if you set the NILTESG_AUTO_HEADROOM_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    The default value is 0. 
        /// </summary>
        public int GetHeadroom(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.Headroom, channel, out value);
        }

        /// <summary>
        ///Specifies the value of the DC offset in the in-phase (I) signal as a percentage of the root mean square (RMS) magnitude of the unaltered I signal.
        ///    Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are -100 to 100, inclusive.
        /// </summary>
        public int SetIDcOffset(string channel, double value)
        {
            return SetDouble(niLTESGProperties.IDcOffset, channel, value);
        }
        /// <summary>
        ///Specifies the value of the DC offset in the in-phase (I) signal as a percentage of the root mean square (RMS) magnitude of the unaltered I signal.
        ///    Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are -100 to 100, inclusive.
        /// </summary>
        public int GetIDcOffset(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.IDcOffset, channel, out value);
        }

        /// <summary>
        ///Specifies the ratio, in dB, of the mean amplitude of the in-phase (I)    signal to the mean amplitude of the quadrature-phase (Q) signal.
        ///    Note: If you set this attribute to a large value, there may be loss of dynamic range at the digital-to-analog converter (DAC).
        ///    I/Q gain imbalance follows the definition shown in the following equation:
        ///    I' = I - (gamma)*sin(phi)*Q + I0
        ///    Q' = (gamma)*cos(phi)*Q + Q0
        ///    where
        ///    gamma = 10^(IQ gain imbalance/20)
        ///    phi = quadrature skew
        ///    I0 = (rms value of I) * (I DC Offset)/100
        ///    Q0 = (rms value of Q) * (Q DC Offset)/100
        ///    Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are -6 to 6, inclusive.
        /// </summary>
        public int SetIqGainImbalance(string channel, double value)
        {
            return SetDouble(niLTESGProperties.IqGainImbalance, channel, value);
        }
        /// <summary>
        ///Specifies the ratio, in dB, of the mean amplitude of the in-phase (I)    signal to the mean amplitude of the quadrature-phase (Q) signal.
        ///    Note: If you set this attribute to a large value, there may be loss of dynamic range at the digital-to-analog converter (DAC).
        ///    I/Q gain imbalance follows the definition shown in the following equation:
        ///    I' = I - (gamma)*sin(phi)*Q + I0
        ///    Q' = (gamma)*cos(phi)*Q + Q0
        ///    where
        ///    gamma = 10^(IQ gain imbalance/20)
        ///    phi = quadrature skew
        ///    I0 = (rms value of I) * (I DC Offset)/100
        ///    Q0 = (rms value of Q) * (Q DC Offset)/100
        ///    Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring active channels.
        ///    The default value is 0. Valid values are -6 to 6, inclusive.
        /// </summary>
        public int GetIqGainImbalance(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.IqGainImbalance, channel, out value);
        }

        /// <summary>
        ///Returns the size, in samples, of the generated I/Q waveform.
        /// </summary>
        public int GetIqWaveformSize(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.IqWaveformSize, channel, out value);
        }

        /// <summary>
        ///Specifies the number of transmit antennas for which the toolkit generates the signal.
        ///    The default value is 1. Valid values are 1, 2, and 4.
        /// </summary>
        public int SetNumberOfAntennas(string channel, int value)
        {
            return SetInt32(niLTESGProperties.NumberOfAntennas, channel, value);
        }
        /// <summary>
        ///Specifies the number of transmit antennas for which the toolkit generates the signal.
        ///    The default value is 1. Valid values are 1, 2, and 4.
        /// </summary>
        public int GetNumberOfAntennas(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.NumberOfAntennas, channel, out value);
        }

        /// <summary>
        ///Specifies the number of frames to generate. 
        ///    The toolkit uses the same frame configuration for all the frames in the uplink direction.    In the downlink direction, when you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD,    the toolkit increments the frame number for successive frames. If you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_PN_SEQUENCE or NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS, the toolkit uses the same frame    configuration for all the frames.
        ///    The default value is 1.
        /// </summary>
        public int SetNumberOfFrames(string channel, int value)
        {
            return SetInt32(niLTESGProperties.NumberOfFrames, channel, value);
        }
        /// <summary>
        ///Specifies the number of frames to generate. 
        ///    The toolkit uses the same frame configuration for all the frames in the uplink direction.    In the downlink direction, when you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_AUTO_CONFIGURE_PAYLOAD,    the toolkit increments the frame number for successive frames. If you set the NILTESG_DL_PBCH_PAYLOAD_DATA_TYPE attribute to    NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_PN_SEQUENCE or NILTESG_VAL_DL_PBCH_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS, the toolkit uses the same frame    configuration for all the frames.
        ///    The default value is 1.
        /// </summary>
        public int GetNumberOfFrames(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.NumberOfFrames, channel, out value);
        }

        /// <summary>
        ///Specifies the number of times the toolkit increases the internally calculated sample rate to get a new sample rate of the waveform.    The toolkit creates the waveform at the new sample rate.
        ///    The default value is 1. Valid values are 1, 2, and 4.
        /// </summary>
        public int SetOversamplingFactor(string channel, int value)
        {
            return SetInt32(niLTESGProperties.OversamplingFactor, channel, value);
        }
        /// <summary>
        ///Specifies the number of times the toolkit increases the internally calculated sample rate to get a new sample rate of the waveform.    The toolkit creates the waveform at the new sample rate.
        ///    The default value is 1. Valid values are 1, 2, and 4.
        /// </summary>
        public int GetOversamplingFactor(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.OversamplingFactor, channel, out value);
        }

        /// <summary>
        ///Specifies the method used for power scaling.
        ///    Refer to the Power Scaling topic for more information about power scaling.
        ///    The default value is NILTESG_VAL_POWER_SCALING_TYPE_REFERENCE_SIGNAL_POWER. 
        /// </summary>
        public int SetPowerScalingType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.PowerScalingType, channel, value);
        }
        /// <summary>
        ///Specifies the method used for power scaling.
        ///    Refer to the Power Scaling topic for more information about power scaling.
        ///    The default value is NILTESG_VAL_POWER_SCALING_TYPE_REFERENCE_SIGNAL_POWER. 
        /// </summary>
        public int GetPowerScalingType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.PowerScalingType, channel, out value);
        }

        /// <summary>
        ///Specifies the value of the DC offset in the quadrature-phase (Q) signal as a percentage of the root mean square (RMS) magnitude of the unaltered Q signal.
        ///    Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are -100 to 100, inclusive.
        /// </summary>
        public int SetQDcOffset(string channel, double value)
        {
            return SetDouble(niLTESGProperties.QDcOffset, channel, value);
        }
        /// <summary>
        ///Specifies the value of the DC offset in the quadrature-phase (Q) signal as a percentage of the root mean square (RMS) magnitude of the unaltered Q signal.
        ///    Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are -100 to 100, inclusive.
        /// </summary>
        public int GetQDcOffset(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.QDcOffset, channel, out value);
        }

        /// <summary>
        ///Specifies the deviation in angle from 90 degrees between the in-phase (I)    and quadrature-phase (Q) signals.
        ///    Quadrature skew follows the definition shown in the following equation:
        ///    I' = I - (gamma)*sin(phi)*Q + I0
        ///    Q' = (gamma)*cos(phi)*Q + Q0
        ///    where
        ///    gamma = 10^(IQ gain imbalance/20)
        ///    phi = quadrature skew
        ///    I0 = (rms value of I) * (I DC Offset)/100
        ///    Q0 = (rms value of Q) * (Q DC Offset)/100
        ///    Refer to the Quadrature Skew topic for more information about this attribute.
        ///      Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are -15 to 15, inclusive.
        /// </summary>
        public int SetQuadratureSkew(string channel, double value)
        {
            return SetDouble(niLTESGProperties.QuadratureSkew, channel, value);
        }
        /// <summary>
        ///Specifies the deviation in angle from 90 degrees between the in-phase (I)    and quadrature-phase (Q) signals.
        ///    Quadrature skew follows the definition shown in the following equation:
        ///    I' = I - (gamma)*sin(phi)*Q + I0
        ///    Q' = (gamma)*cos(phi)*Q + Q0
        ///    where
        ///    gamma = 10^(IQ gain imbalance/20)
        ///    phi = quadrature skew
        ///    I0 = (rms value of I) * (I DC Offset)/100
        ///    Q0 = (rms value of Q) * (Q DC Offset)/100
        ///    Refer to the Quadrature Skew topic for more information about this attribute.
        ///      Use an 'antennan' active channel string to configure or read this attribute for nth channel or NULL string to configure for all channels. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are -15 to 15, inclusive.
        /// </summary>
        public int GetQuadratureSkew(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.QuadratureSkew, channel, out value);
        }

        /// <summary>
        ///Returns the recommended sample rate, in samples per second, for the current channel configuration.
        ///     The dt parameter of the created waveform is the inverse of the recommended sample rate. 
        /// </summary>
        public int GetRecommendedIqRate(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.RecommendedIqRate, channel, out value);
        }

        /// <summary>
        ///Returns the number of samples with an instantaneous power that is the same as the average power of the signal, as a percentage of the total number of samples.
        /// </summary>
        public int GetResultCcdfMeanPowerPercentile(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfMeanPowerPercentile, channel, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.001% of the total samples in the signal are present.
        /// </summary>
        public int GetResultCcdfOneHundredThousandthPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfOneHundredThousandthPower, channel, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 1% of the total samples in the signal are present.
        /// </summary>
        public int GetResultCcdfOneHundredthPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfOneHundredthPower, channel, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.0001% of the total samples in the signal are present.
        /// </summary>
        public int GetResultCcdfOneMillionthPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfOneMillionthPower, channel, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.01% of the total samples in the signal are present.
        /// </summary>
        public int GetResultCcdfOneTenThousandthPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfOneTenThousandthPower, channel, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 10% of the total samples in the signal are present.
        /// </summary>
        public int GetResultCcdfOneTenthPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfOneTenthPower, channel, out value);
        }

        /// <summary>
        ///Returns the power above the average power, in dB, over which 0.1% of the total samples in the signal are present.
        /// </summary>
        public int GetResultCcdfOneThousandthPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfOneThousandthPower, channel, out value);
        }

        /// <summary>
        ///Returns the peak-to-average power ratio (PAPR), in dB, of the signal.
        /// </summary>
        public int GetResultCcdfPeakToAveragePowerRatio(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.ResultCcdfPeakToAveragePowerRatio, channel, out value);
        }

        /// <summary>
        ///Specifies the offset in the Sample clock frequency, in parts per million (ppm), from the value of the NILTESG_RECOMMENDED_IQ_RATE attribute.
        ///    For large offset values with large waveform sizes, clock cycle slips may occur. Clock cycle slips can cause the final waveform size to be    different from the expected size.
        ///    The default value is 0. Valid values are -20 to 20, inclusive.
        /// </summary>
        public int SetSampleClockOffset(string channel, double value)
        {
            return SetDouble(niLTESGProperties.SampleClockOffset, channel, value);
        }
        /// <summary>
        ///Specifies the offset in the Sample clock frequency, in parts per million (ppm), from the value of the NILTESG_RECOMMENDED_IQ_RATE attribute.
        ///    For large offset values with large waveform sizes, clock cycle slips may occur. Clock cycle slips can cause the final waveform size to be    different from the expected size.
        ///    The default value is 0. Valid values are -20 to 20, inclusive.
        /// </summary>
        public int GetSampleClockOffset(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.SampleClockOffset, channel, out value);
        }

        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the signal to be generated.
        ///    The default value is 10M.
        /// </summary>
        public int SetSystemBandwidth(string channel, double value)
        {
            return SetDouble(niLTESGProperties.SystemBandwidth, channel, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in hertz (Hz), of the signal to be generated.
        ///    The default value is 10M.
        /// </summary>
        public int GetSystemBandwidth(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.SystemBandwidth, channel, out value);
        }

        /// <summary>
        ///Indicates the version of the toolkit to which the current version of the toolkit is compatible.
        /// </summary>
        public int GetToolkitCompatibilityVersion(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.ToolkitCompatibilityVersion, channel, out value);
        }

        /// <summary>
        ///Specifies the way in which the cyclic shifts of the physical uplink shared channel (PUSCH) demodulation reference signals (DMRSs) in a slot are configured.
        ///    If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE, the toolkit ignores the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in Section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_FALSE, the toolkit uses the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE, the toolkit ignores the NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in Section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_FALSE, the toolkit uses the NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetUl3gppCyclicShiftEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.Ul3gppCyclicShiftEnabled, channel, value);
        }
        /// <summary>
        ///Specifies the way in which the cyclic shifts of the physical uplink shared channel (PUSCH) demodulation reference signals (DMRSs) in a slot are configured.
        ///    If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE, the toolkit ignores the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in Section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_FALSE, the toolkit uses the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE, the toolkit ignores the NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes, and automatically calculates the cyclic shifts and N_cs, as described in Section 5.5.2.1 of the 3GPP TS 36.211 v8.6.0 specifications. If you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_FALSE, the toolkit uses the NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_0 and NILTESG_UL_PUCCH_CYCLIC_SHIFT_INDEX_1 attributes as values for N_cs in slot 0 and slot 1 respectively. 
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetUl3gppCyclicShiftEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.Ul3gppCyclicShiftEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable shifting in the physical uplink shared channel (PUSCH) discrete Fourier transform (DFT) precoding. If you enable this attribute, the DC component is at the    center of the DFT output.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int SetUlDftShiftEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlDftShiftEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable shifting in the physical uplink shared channel (PUSCH) discrete Fourier transform (DFT) precoding. If you enable this attribute, the DC component is at the    center of the DFT output.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int GetUlDftShiftEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlDftShiftEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable hopping for the uplink signal. The toolkit supports only group and sequence hopping.    Refer to sections 5.5.1.3 and 5.5.1.4 of  the 3GPP TS 36.211 v8.6.0 specifications for more information about hopping.    If you set this attribute to NILTESG_VAL_TRUE, you can configure the NILTESG_UL_HOPPING_MODE attribute.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int SetUlHoppingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlHoppingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable hopping for the uplink signal. The toolkit supports only group and sequence hopping.    Refer to sections 5.5.1.3 and 5.5.1.4 of  the 3GPP TS 36.211 v8.6.0 specifications for more information about hopping.    If you set this attribute to NILTESG_VAL_TRUE, you can configure the NILTESG_UL_HOPPING_MODE attribute.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int GetUlHoppingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlHoppingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the hopping mode for the uplink signal. Refer to sections 5.5.1.3 and 5.5.1.4 of the 3GPP TS 36.211    v8.6.0 specifications for more information about hopping modes. Configure this attribute only if you set the NILTESG_UL_HOPPING_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    The default value is NILTESG_VAL_UL_HOPPING_MODE_GROUP_HOPPING.
        /// </summary>
        public int SetUlHoppingMode(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlHoppingMode, channel, value);
        }
        /// <summary>
        ///Specifies the hopping mode for the uplink signal. Refer to sections 5.5.1.3 and 5.5.1.4 of the 3GPP TS 36.211    v8.6.0 specifications for more information about hopping modes. Configure this attribute only if you set the NILTESG_UL_HOPPING_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    The default value is NILTESG_VAL_UL_HOPPING_MODE_GROUP_HOPPING.
        /// </summary>
        public int GetUlHoppingMode(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlHoppingMode, channel, out value);
        }

        /// <summary>
        ///Specifies the number of physical uplink shared channels (PUSCHs) that you can configure in a frame. You can configure one PUSCH channel in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlNumberOfPuschChannels(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlNumberOfPuschChannels, channel, value);
        }
        /// <summary>
        ///Specifies the number of physical uplink shared channels (PUSCHs) that you can configure in a frame. You can configure one PUSCH channel in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlNumberOfPuschChannels(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlNumberOfPuschChannels, channel, out value);
        }

        /// <summary>
        ///Specifies the cyclic shift to use in the even slot (ncs0) of the physical uplink shared channel demodulation reference signal (PUSCH DMRS). The toolkit ignores this attribute if you set the    NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'puschn' active channel string to configure or read the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int SetUlPuschCyclicShiftIndex0(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschCyclicShiftIndex0, channel, value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in the even slot (ncs0) of the physical uplink shared channel demodulation reference signal (PUSCH DMRS). The toolkit ignores this attribute if you set the    NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'puschn' active channel string to configure or read the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_0 attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int GetUlPuschCyclicShiftIndex0(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschCyclicShiftIndex0, channel, out value);
        }

        /// <summary>
        ///Specifies the cyclic shift to use in the odd slot (ncs1) of the physical uplink shared channel demodulation reference signal (PUSCH DMRS). The toolkit ignores this attribute if you set the    NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'puschn' active channel string to configure or read the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int SetUlPuschCyclicShiftIndex1(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschCyclicShiftIndex1, channel, value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in the odd slot (ncs1) of the physical uplink shared channel demodulation reference signal (PUSCH DMRS). The toolkit ignores this attribute if you set the    NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE.
        ///    Use a 'puschn' active channel string to configure or read the NILTESG_UL_PUSCH_CYCLIC_SHIFT_INDEX_1 attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 11, inclusive.
        /// </summary>
        public int GetUlPuschCyclicShiftIndex1(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschCyclicShiftIndex1, channel, out value);
        }

        /// <summary>
        ///Specifies a standard-defined parameter to calculate the cyclic shifts and the group and sequence indices associated with the physical uplink shared channel demodulation reference signal (PUSCH DMRS).    Refer to section 5.5 of the 3GPP TS 36.211 specifications 8.6.0 for more information about this attribute.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 29, inclusive.
        /// </summary>
        public int SetUlPuschDeltaSs(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschDeltaSs, channel, value);
        }
        /// <summary>
        ///Specifies a standard-defined parameter to calculate the cyclic shifts and the group and sequence indices associated with the physical uplink shared channel demodulation reference signal (PUSCH DMRS).    Refer to section 5.5 of the 3GPP TS 36.211 specifications 8.6.0 for more information about this attribute.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 29, inclusive.
        /// </summary>
        public int GetUlPuschDeltaSs(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschDeltaSs, channel, out value);
        }

        /// <summary>
        ///Specifies the modulation scheme for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        /// </summary>
        public int SetUlPuschModulationScheme(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschModulationScheme, channel, value);
        }
        /// <summary>
        ///Specifies the modulation scheme for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_MODULATION_SCHEME_QPSK.
        /// </summary>
        public int GetUlPuschModulationScheme(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschModulationScheme, channel, out value);
        }

        /// <summary>
        ///Specifies a standard-defined parameter to calculate the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS) cyclic shifts for a specific cell. Refer to section 5.5 of the 3GPP TS 36.211    Specifications 8.6.0 for more information about this attribute.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlPuschNDmrs1(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschNDmrs1, channel, value);
        }
        /// <summary>
        ///Specifies a standard-defined parameter to calculate the physical uplink shared channel (PUSCH) demodulation reference signal (DMRS) cyclic shifts for a specific cell. Refer to section 5.5 of the 3GPP TS 36.211    Specifications 8.6.0 for more information about this attribute.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlPuschNDmrs1(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschNDmrs1, channel, out value);
        }

        /// <summary>
        ///Specifies a standard-defined parameter to calculate the physical uplink shared channel demodulation reference signal (PUSCH DMRS) cyclic shifts for each PUSCH transmission. Refer to section 5.5 of the    3GPP TS 36.211 v8.6.0 specifications for more information about configuring active channels.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlPuschNDmrs2(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschNDmrs2, channel, value);
        }
        /// <summary>
        ///Specifies a standard-defined parameter to calculate the physical uplink shared channel demodulation reference signal (PUSCH DMRS) cyclic shifts for each PUSCH transmission. Refer to section 5.5 of the    3GPP TS 36.211 v8.6.0 specifications for more information about configuring active channels.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlPuschNDmrs2(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschNDmrs2, channel, out value);
        }

        /// <summary>
        ///Specifies the number of resource blocks in the frequency domain allocated for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetUlPuschNumberOfResourceBlocks(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschNumberOfResourceBlocks, channel, value);
        }
        /// <summary>
        ///Specifies the number of resource blocks in the frequency domain allocated for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetUlPuschNumberOfResourceBlocks(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschNumberOfResourceBlocks, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to transmit on the PUSCH channel.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int SetUlPuschPayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschPayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to transmit on the PUSCH channel.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        /// </summary>
        public int GetUlPuschPayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschPayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical uplink shared channel (PUSCH) payload. Configure this attribute only if you set the NILTESG_UL_PUSCH_PAYLOAD_DATA_TYPE  attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int SetUlPuschPayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschPayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical uplink shared channel (PUSCH) payload. Configure this attribute only if you set the NILTESG_UL_PUSCH_PAYLOAD_DATA_TYPE  attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9.
        /// </summary>
        public int GetUlPuschPayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschPayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the PUSCH pseudonoise (PN) generator. Configure this attribute only if you set the    NILTESG_UL_PUSCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int SetUlPuschPayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschPayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the PUSCH pseudonoise (PN) generator. Configure this attribute only if you set the    NILTESG_UL_PUSCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454.
        /// </summary>
        public int GetUlPuschPayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschPayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the    NILTESG_UL_PUSCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetUlPuschPayloadUserDefinedBits(string channel, int[] value)
        {
            return SetArrayInt32(niLTESGProperties.UlPuschPayloadUserDefinedBits, channel, value);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the    NILTESG_UL_PUSCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetUlPuschPayloadUserDefinedBits(string channel, int[] value, out int actualNumValues)
        {
            return GetArrayInt32(niLTESGProperties.UlPuschPayloadUserDefinedBits, channel, value, out actualNumValues);
        }

        /// <summary>
        ///Specifies the physical uplink shared channel (PUSCH) power level, in dB, relative to the power of the PUSCH demodulation reference signal (DMRS).
        ///    Use a 'puschn' active channel string to configure or read the NILTESG_UL_PUSCH_POWER attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetUlPuschPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.UlPuschPower, channel, value);
        }
        /// <summary>
        ///Specifies the physical uplink shared channel (PUSCH) power level, in dB, relative to the power of the PUSCH demodulation reference signal (DMRS).
        ///    Use a 'puschn' active channel string to configure or read the NILTESG_UL_PUSCH_POWER attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetUlPuschPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.UlPuschPower, channel, out value);
        }

        /// <summary>
        ///Specifies the starting resource block in the frequency domain for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetUlPuschResourceBlockOffset(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschResourceBlockOffset, channel, value);
        }
        /// <summary>
        ///Specifies the starting resource block in the frequency domain for physical uplink shared channel (PUSCH) transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetUlPuschResourceBlockOffset(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschResourceBlockOffset, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable scrambling for physical uplink shared channel (PUSCH) transmission. Refer to section 5.3.1 of the 3GPP TS 36.211 v8.6.0 specifications for more information about this attribute.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int SetUlPuschScramblingEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschScramblingEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable scrambling for physical uplink shared channel (PUSCH) transmission. Refer to section 5.3.1 of the 3GPP TS 36.211 v8.6.0 specifications for more information about this attribute.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_TRUE.
        /// </summary>
        public int GetUlPuschScramblingEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschScramblingEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe number for PUSCH transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int SetUlPuschSubframeNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPuschSubframeNumber, channel, value);
        }
        /// <summary>
        ///Specifies the subframe number for PUSCH transmission.
        ///    Use a 'puschn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive.
        /// </summary>
        public int GetUlPuschSubframeNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPuschSubframeNumber, channel, out value);
        }

        /// <summary>
        ///Specifies the radio network temporary identifier (RNTI) for the user equipment (UE).
        ///    The default value is 0. Valid values are 0 to 65,535, inclusive.
        /// </summary>
        public int SetUlRnti(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlRnti, channel, value);
        }
        /// <summary>
        ///Specifies the radio network temporary identifier (RNTI) for the user equipment (UE).
        ///    The default value is 0. Valid values are 0 to 65,535, inclusive.
        /// </summary>
        public int GetUlRnti(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlRnti, channel, out value);
        }

        /// <summary>
        ///Specifies the length of the symbol-shaping window. The symbol-shaping window affects the spectral attributes of the created waveform.    Refer to the Windowing topic for more information about windowing.
        ///    The window length is expressed as a percentage of the cyclic prefix length. The maximum allowed window length is half of the cyclic    prefix length. To disable windowing, set the NILTESG_WINDOW_LENGTH attribute to 0.
        ///    The default value is 0. Valid values are 0 to 50, inclusive.
        /// </summary>
        public int SetWindowLength(string channel, double value)
        {
            return SetDouble(niLTESGProperties.WindowLength, channel, value);
        }
        /// <summary>
        ///Specifies the length of the symbol-shaping window. The symbol-shaping window affects the spectral attributes of the created waveform.    Refer to the Windowing topic for more information about windowing.
        ///    The window length is expressed as a percentage of the cyclic prefix length. The maximum allowed window length is half of the cyclic    prefix length. To disable windowing, set the NILTESG_WINDOW_LENGTH attribute to 0.
        ///    The default value is 0. Valid values are 0 to 50, inclusive.
        /// </summary>
        public int GetWindowLength(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.WindowLength, channel, out value);
        }

        /// <summary>
        ///Specifies whether to apply I/Q impairments such as I DC offset, Q DC offset, quadrature skew, and I/Q gain imbalance to the waveform.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int SetAllIqImpairmentsEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.AllIqImpairmentsEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to apply I/Q impairments such as I DC offset, Q DC offset, quadrature skew, and I/Q gain imbalance to the waveform.
        ///    The default value is NILTESG_VAL_FALSE.
        /// </summary>
        public int GetAllIqImpairmentsEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.AllIqImpairmentsEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the special subframe configuration index as defined in section 4.2 of the 3GPP TS 36.211 v8.8.0 specifications. The value of this attribute is equal to the ratio of uplink pilot timeslot (UpPTS) to downlink pilot timeslot (DwPTS). 
        ///    The default value is 0.
        /// </summary>
        public int SetSpecialSubframeConfiguration(string channel, int value)
        {
            return SetInt32(niLTESGProperties.SpecialSubframeConfiguration, channel, value);
        }
        /// <summary>
        ///Specifies the special subframe configuration index as defined in section 4.2 of the 3GPP TS 36.211 v8.8.0 specifications. The value of this attribute is equal to the ratio of uplink pilot timeslot (UpPTS) to downlink pilot timeslot (DwPTS). 
        ///    The default value is 0.
        /// </summary>
        public int GetSpecialSubframeConfiguration(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.SpecialSubframeConfiguration, channel, out value);
        }

        /// <summary>
        ///Specifies the uplink/downlink (UL/DL) configuration index as defined in section 4.2 of the 3GPP TS 36.211 v8.8.0 specifications for the time-division duplex (TDD) frame. 
        ///    The default value is 0.
        /// </summary>
        public int SetUlDlConfiguration(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlDlConfiguration, channel, value);
        }
        /// <summary>
        ///Specifies the uplink/downlink (UL/DL) configuration index as defined in section 4.2 of the 3GPP TS 36.211 v8.8.0 specifications for the time-division duplex (TDD) frame. 
        ///    The default value is 0.
        /// </summary>
        public int GetUlDlConfiguration(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlDlConfiguration, channel, out value);
        }

        /// <summary>
        ///Specifies the number of physical uplink control channels (PUCCH) that you can configure in a frame. You can configure one PUCCH channel in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int SetUlNumberOfPucchChannels(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlNumberOfPucchChannels, channel, value);
        }
        /// <summary>
        ///Specifies the number of physical uplink control channels (PUCCH) that you can configure in a frame. You can configure one PUCCH channel in each subframe.
        ///    The default value is 0. Valid values are 0 to 10, inclusive.
        /// </summary>
        public int GetUlNumberOfPucchChannels(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlNumberOfPucchChannels, channel, out value);
        }

        /// <summary>
        ///Specifies the cyclic shift to use in slot 0 of the physical uplink control (PUCCH). The toolkit ignores this attribute if you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE.   
        /// </summary>
        public int SetUlPucchCyclicShiftIndex0(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchCyclicShiftIndex0, channel, value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in slot 0 of the physical uplink control (PUCCH). The toolkit ignores this attribute if you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE.   
        /// </summary>
        public int GetUlPucchCyclicShiftIndex0(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchCyclicShiftIndex0, channel, out value);
        }

        /// <summary>
        ///Specifies the cyclic shift to use in slot 1 of the physical uplink control channel (PUCCH). The toolkit ignores this attribute if you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels. 
        ///    The default value is 0. Valid values are 0 to 11, inclusive. 
        /// </summary>
        public int SetUlPucchCyclicShiftIndex1(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchCyclicShiftIndex1, channel, value);
        }
        /// <summary>
        ///Specifies the cyclic shift to use in slot 1 of the physical uplink control channel (PUCCH). The toolkit ignores this attribute if you set the NILTESG_UL_3GPP_CYCLIC_SHIFT_ENABLED attribute to NILTESG_VAL_TRUE. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels. 
        ///    The default value is 0. Valid values are 0 to 11, inclusive. 
        /// </summary>
        public int GetUlPucchCyclicShiftIndex1(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchCyclicShiftIndex1, channel, out value);
        }

        /// <summary>
        ///Specifies the parameter used in determining the resource blocks and cyclic shifts for the physical uplink control channel (PUCCH) transmission as defined in section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 1. Valid values are 1 to 3, inclusive. 
        /// </summary>
        public int SetUlPucchDeltaPucchShift(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchDeltaPucchShift, channel, value);
        }
        /// <summary>
        ///Specifies the parameter used in determining the resource blocks and cyclic shifts for the physical uplink control channel (PUCCH) transmission as defined in section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 1. Valid values are 1 to 3, inclusive. 
        /// </summary>
        public int GetUlPucchDeltaPucchShift(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchDeltaPucchShift, channel, out value);
        }

        /// <summary>
        ///Specifies the format used for physical uplink control channel (PUCCH) transmission. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_UL_PUCCH_FORMAT_1. 
        /// </summary>
        public int SetUlPucchFormat(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchFormat, channel, value);
        }
        /// <summary>
        ///Specifies the format used for physical uplink control channel (PUCCH) transmission. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_UL_PUCCH_FORMAT_1. 
        /// </summary>
        public int GetUlPucchFormat(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchFormat, channel, out value);
        }

        /// <summary>
        ///Specifies the number of cyclic shifts used for physical uplink control channel (PUCCH) formats 1/1a/1b in a resource block used for a combination of formats 1/1a/1b and 2/2a/2b. 
        ///    The frame does not contain a combined resource block if the value of the NILTESG_UL_PUCCH_n_CS_1 attribute is 0. Refer to section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications for more information about this attribute.
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int SetUlPucchNCs1(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchNCs1, channel, value);
        }
        /// <summary>
        ///Specifies the number of cyclic shifts used for physical uplink control channel (PUCCH) formats 1/1a/1b in a resource block used for a combination of formats 1/1a/1b and 2/2a/2b. 
        ///    The frame does not contain a combined resource block if the value of the NILTESG_UL_PUCCH_n_CS_1 attribute is 0. Refer to section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications for more information about this attribute.
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int GetUlPucchNCs1(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchNCs1, channel, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the resource block assigned for the physical uplink control channel (PUCCH) formats 1/1a/1b as defined in section 5.4.1 of the 3GPP TS 36.211 v8.8.0 specifications. 
        /// </summary>
        public int SetUlPucchNPucch1(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchNPucch1, channel, value);
        }
        /// <summary>
        ///Specifies a parameter used in determining the resource block assigned for the physical uplink control channel (PUCCH) formats 1/1a/1b as defined in section 5.4.1 of the 3GPP TS 36.211 v8.8.0 specifications. 
        /// </summary>
        public int GetUlPucchNPucch1(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchNPucch1, channel, out value);
        }

        /// <summary>
        ///Specifies the parameter used in determining the resource block assigned for the physical uplink control channels (PUCCH) formats 2/2a/2b as defined in section 5.4.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///  	 	Get Function: niLTESG_GetULPUCCHnPUCCH2
        ///  	Set Function: niLTESG_SetULPUCCHnPUCCH2
        /// </summary>
        public int SetUlPucchNPucch2(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchNPucch2, channel, value);
        }
        /// <summary>
        ///Specifies the parameter used in determining the resource block assigned for the physical uplink control channels (PUCCH) formats 2/2a/2b as defined in section 5.4.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///  	 	Get Function: niLTESG_GetULPUCCHnPUCCH2
        ///  	Set Function: niLTESG_SetULPUCCHnPUCCH2
        /// </summary>
        public int GetUlPucchNPucch2(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchNPucch2, channel, out value);
        }

        /// <summary>
        ///Specifies the bandwidth, in terms of the number of resource blocks that can be used by physical uplink control channel (PUCCH) formats 2/2a/2b transmission in each slot. Refer to section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications for more information about this attribute.
        ///    The default value is 0. Valid values are 0 to N_UL_RB-1, inclusive.
        /// </summary>
        public int SetUlPucchNRb2(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchNRb2, channel, value);
        }
        /// <summary>
        ///Specifies the bandwidth, in terms of the number of resource blocks that can be used by physical uplink control channel (PUCCH) formats 2/2a/2b transmission in each slot. Refer to section 5.4 of the 3GPP TS 36.211 v8.8.0 specifications for more information about this attribute.
        ///    The default value is 0. Valid values are 0 to N_UL_RB-1, inclusive.
        /// </summary>
        public int GetUlPucchNRb2(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchNRb2, channel, out value);
        }

        /// <summary>
        ///Specifies the type of data to be transmitted on the physical uplink control channel (PUCCH).
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE. 
        /// </summary>
        public int SetUlPucchPayloadDataType(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchPayloadDataType, channel, value);
        }
        /// <summary>
        ///Specifies the type of data to be transmitted on the physical uplink control channel (PUCCH).
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE. 
        /// </summary>
        public int GetUlPucchPayloadDataType(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchPayloadDataType, channel, out value);
        }

        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical uplink control channel (PUCCH) payload. Configure this attribute only if you set the NILTESG_UL_PUCCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9. 
        /// </summary>
        public int SetUlPucchPayloadPnOrder(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchPayloadPnOrder, channel, value);
        }
        /// <summary>
        ///Specifies the pseudonoise (PN) order for the physical uplink control channel (PUCCH) payload. Configure this attribute only if you set the NILTESG_UL_PUCCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 9. 
        /// </summary>
        public int GetUlPucchPayloadPnOrder(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchPayloadPnOrder, channel, out value);
        }

        /// <summary>
        ///Specifies the seed for the physical uplink control channel (PUCCH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_UL_PUCCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454. 
        /// </summary>
        public int SetUlPucchPayloadPnSeed(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchPayloadPnSeed, channel, value);
        }
        /// <summary>
        ///Specifies the seed for the physical uplink control channel (PUCCH) pseudonoise (PN) generator. Configure this attribute only if you set the NILTESG_UL_PUCCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_PN_SEQUENCE.
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is -692093454. 
        /// </summary>
        public int GetUlPucchPayloadPnSeed(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchPayloadPnSeed, channel, out value);
        }

        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the    NILTESG_UL_PUCCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int SetUlPucchPayloadUserDefinedBits(string channel, int[] value)
        {
            return SetArrayInt32(niLTESGProperties.UlPucchPayloadUserDefinedBits, channel, value);
        }
        /// <summary>
        ///Specifies a user-defined bit pattern as an array of zeros and ones. Configure this attribute only if you set the    NILTESG_UL_PUCCH_PAYLOAD_DATA_TYPE attribute to NILTESG_VAL_PAYLOAD_DATA_TYPE_USER_DEFINED_BITS.
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        /// </summary>
        public int GetUlPucchPayloadUserDefinedBits(string channel, int[] value, out int actualNumValues)
        {
            return GetArrayInt32(niLTESGProperties.UlPucchPayloadUserDefinedBits, channel, value, out actualNumValues);
        }

        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) power level, in dB, relative to the power of the PUCCH demodulation reference signal (DMRS).
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int SetUlPucchPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.UlPucchPower, channel, value);
        }
        /// <summary>
        ///Specifies the physical uplink control channel (PUCCH) power level, in dB, relative to the power of the PUCCH demodulation reference signal (DMRS).
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0.
        /// </summary>
        public int GetUlPucchPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.UlPucchPower, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe number for physical uplink control channel (PUCCH) transmission. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive. 
        /// </summary>
        public int SetUlPucchSubframeNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlPucchSubframeNumber, channel, value);
        }
        /// <summary>
        ///Specifies the subframe number for physical uplink control channel (PUCCH) transmission. 
        ///    Use a 'pucchn' active channel string to configure or read this attribute. Refer to the Configuring Active Channels topic for more information about configuring the active channels.
        ///    The default value is 0. Valid values are 0 to 9, inclusive. 
        /// </summary>
        public int GetUlPucchSubframeNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlPucchSubframeNumber, channel, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 3, inclusive. 
        /// </summary>
        public int SetUlSrsBSrs(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsBSrs, channel, value);
        }
        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 3, inclusive. 
        /// </summary>
        public int GetUlSrsBSrs(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsBSrs, channel, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 7. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int SetUlSrsCSrs(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsCSrs, channel, value);
        }
        /// <summary>
        ///Specifies a parameter used in determining the frequency domain resource allocation for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 7. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int GetUlSrsCSrs(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsCSrs, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable the sounding reference signal (SRS). 
        ///    The default value is NILTESG_VAL_FALSE. 
        /// </summary>
        public int SetUlSrsEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable the sounding reference signal (SRS). 
        ///    The default value is NILTESG_VAL_FALSE. 
        /// </summary>
        public int GetUlSrsEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies the configuration index that determines the subframes in which the toolkit generates the sounding reference signal (SRS). 
        ///    The default value is 0. Valid values are 0 to 644, inclusive. 
        /// </summary>
        public int SetUlSrsISrs(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsISrs, channel, value);
        }
        /// <summary>
        ///Specifies the configuration index that determines the subframes in which the toolkit generates the sounding reference signal (SRS). 
        ///    The default value is 0. Valid values are 0 to 644, inclusive. 
        /// </summary>
        public int GetUlSrsISrs(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsISrs, channel, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is NILTESG_VAL_UL_SRS_EVEN_SUBCARRIERS.
        /// </summary>
        public int SetUlSrsKTc(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsKTc, channel, value);
        }
        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is NILTESG_VAL_UL_SRS_EVEN_SUBCARRIERS.
        /// </summary>
        public int GetUlSrsKTc(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsKTc, channel, out value);
        }

        /// <summary>
        ///Specifies whether to enable the Max uplink pilot timeslot (UpPTS) mode for a special subframe. 
        ///    The default value is NILTESG_VAL_TRUE. 
        /// </summary>
        public int SetUlSrsMaxupptsEnabled(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsMaxUpPtsEnabled, channel, value);
        }
        /// <summary>
        ///Specifies whether to enable the Max uplink pilot timeslot (UpPTS) mode for a special subframe. 
        ///    The default value is NILTESG_VAL_TRUE. 
        /// </summary>
        public int GetUlSrsMaxupptsEnabled(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsMaxUpPtsEnabled, channel, out value);
        }

        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 23, inclusive. 
        /// </summary>
        public int SetUlSrsNRrc(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsNRrc, channel, value);
        }
        /// <summary>
        ///Specifies a parameter used in determining the frequency domain starting position for the sounding reference signal (SRS) sequence, as defined in section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications. 
        ///    The default value is 0. Valid values are 0 to 23, inclusive. 
        /// </summary>
        public int GetUlSrsNRrc(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsNRrc, channel, out value);
        }

        /// <summary>
        ///Specifies the cyclic shift on the sounding reference signal (SRS) sequence. 
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int SetUlSrsNsrsCs(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsNsrsCs, channel, value);
        }
        /// <summary>
        ///Specifies the cyclic shift on the sounding reference signal (SRS) sequence. 
        ///    The default value is 0. Valid values are 0 to 7, inclusive. 
        /// </summary>
        public int GetUlSrsNsrsCs(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsNsrsCs, channel, out value);
        }

        /// <summary>
        ///Specifies the number of PRACH resources allocated on the special subframe. 
        ///    Refer to section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications for more information on this attribute. 
        ///    The default value is 0. Valid values are 0 to 6, inclusive. 
        /// </summary>
        public int SetUlSrsNumberOfFormat4Prach(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsNumberOfFormat4Prach, channel, value);
        }
        /// <summary>
        ///Specifies the number of PRACH resources allocated on the special subframe. 
        ///    Refer to section 5.5.3.2 of the 3GPP TS 36.211 v8.8.0 specifications for more information on this attribute. 
        ///    The default value is 0. Valid values are 0 to 6, inclusive. 
        /// </summary>
        public int GetUlSrsNumberOfFormat4Prach(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsNumberOfFormat4Prach, channel, out value);
        }

        /// <summary>
        ///Specifies the power level, in dB, for the sounding reference signal (SRS). When physical uplink shared channel (PUSCH) or physical uplink control channel (PUCCH) signals are transmitted along with the SRS signal, the NILTESG_UL_SRS_POWER attribute specifies the power level of SRS, in dB, relative to the PUSCH demodulation reference signal (DMRS) or PUCCH DMRS power.
        ///    The default value is 0. 
        /// </summary>
        public int SetUlSrsPower(string channel, double value)
        {
            return SetDouble(niLTESGProperties.UlSrsPower, channel, value);
        }
        /// <summary>
        ///Specifies the power level, in dB, for the sounding reference signal (SRS). When physical uplink shared channel (PUSCH) or physical uplink control channel (PUCCH) signals are transmitted along with the SRS signal, the NILTESG_UL_SRS_POWER attribute specifies the power level of SRS, in dB, relative to the PUSCH demodulation reference signal (DMRS) or PUCCH DMRS power.
        ///    The default value is 0. 
        /// </summary>
        public int GetUlSrsPower(string channel, out double value)
        {
            return GetDouble(niLTESGProperties.UlSrsPower, channel, out value);
        }

        /// <summary>
        ///Specifies whether the user equipment (UE) is configured to support the simultaneous transmission of ACK/NACK on PUCCH and SRS in same subframe. 
        ///    The default value is NILTESG_VAL_TRUE. 
        /// </summary>
        public int SetUlSrsSimultaneousAnAndSrs(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsSimultaneousAnAndSrs, channel, value);
        }
        /// <summary>
        ///Specifies whether the user equipment (UE) is configured to support the simultaneous transmission of ACK/NACK on PUCCH and SRS in same subframe. 
        ///    The default value is NILTESG_VAL_TRUE. 
        /// </summary>
        public int GetUlSrsSimultaneousAnAndSrs(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsSimultaneousAnAndSrs, channel, out value);
        }

        /// <summary>
        ///Specifies the subframe configuration index of the sounding reference signal (SRS). This cell-specific attribute specifies the subframes that are reserved to support SRS.
        ///    The default value is 0. Valid values are 0 to 13, inclusive for time-division duplexing (TDD) and 0 to 14, inclusive for frequency-division duplexing (FDD). 
        /// </summary>
        public int SetUlSrsSubframeConfigurationIndex(string channel, int value)
        {
            return SetInt32(niLTESGProperties.UlSrsSubframeConfigurationIndex, channel, value);
        }
        /// <summary>
        ///Specifies the subframe configuration index of the sounding reference signal (SRS). This cell-specific attribute specifies the subframes that are reserved to support SRS.
        ///    The default value is 0. Valid values are 0 to 13, inclusive for time-division duplexing (TDD) and 0 to 14, inclusive for frequency-division duplexing (FDD). 
        /// </summary>
        public int GetUlSrsSubframeConfigurationIndex(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.UlSrsSubframeConfigurationIndex, channel, out value);
        }

        /// <summary>
        ///Specifies the system frame number. 
        ///    The default value is 0.
        /// </summary>
        public int SetSystemFrameNumber(string channel, int value)
        {
            return SetInt32(niLTESGProperties.SystemFrameNumber, channel, value);
        }
        /// <summary>
        ///Specifies the system frame number. 
        ///    The default value is 0.
        /// </summary>
        public int GetSystemFrameNumber(string channel, out int value)
        {
            return GetInt32(niLTESGProperties.SystemFrameNumber, channel, out value);
        }

        #endregion
        #region Private Get/Set methods used internally
        private int GetArrayInt32(niLTESGProperties attributeID, string channelString, int[] dataArray, out int actualNumDataArrayElements)
        {
            int pInvokeResult = PInvoke.niLTESG_GetVectorAttributeI32(Handle, channelString, attributeID, dataArray, dataArray.Length, out actualNumDataArrayElements);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        private int SetArrayInt32(niLTESGProperties attributeID, string channelString, int[] dataArray)
        {
            int pInvokeResult = PInvoke.niLTESG_SetVectorAttributeI32(Handle, channelString, attributeID, dataArray, dataArray.Length);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetDouble(niLTESGProperties attributeID, string channelString, out double attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESG_GetScalarAttributeF64(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        private int SetDouble(niLTESGProperties attributeID, string channelString, double attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESG_SetScalarAttributeF64(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetInt32(niLTESGProperties attributeID, string channelString, out int attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESG_GetScalarAttributeI32(Handle, channelString, attributeID, out attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        private int SetInt32(niLTESGProperties attributeID, string channelString, int attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESG_SetScalarAttributeI32(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int GetString(string channelString, niLTESGProperties attributeID, StringBuilder attributeValue, int bufferSize, out int numberofCharsWritten)
        {
            int pInvokeResult = PInvoke.niLTESG_GetVectorAttributeString(Handle, channelString, attributeID, attributeValue, bufferSize, out numberofCharsWritten);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        private int SetString(niLTESGProperties attributeID, string channelString, string attributeValue)
        {
            int pInvokeResult = PInvoke.niLTESG_SetVectorAttributeString(Handle, channelString, attributeID, attributeValue);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }

        private int Get2DArrayInt32(niLTESGProperties attributeID, string channelString, int[,] dataArray, int numRows, int numColumns)
        {
            int pInvokeResult = PInvoke.niLTESG_Get2DArrayAttributeI32(Handle, channelString, attributeID, dataArray, numRows, numColumns);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        private int Set2DArrayInt32(niLTESGProperties attributeID, string channelString, int[,] dataArray, int numRows, int numColumns)
        {
            int pInvokeResult = PInvoke.niLTESG_Set2DArrayAttributeI32(Handle, channelString, attributeID, dataArray, numRows, numColumns);
            TestForError(pInvokeResult);
            return pInvokeResult;
        }
        #endregion
        private class PInvoke
        {
            const string nativeDllName = "niLTESG_net.dll";

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CCDFGetGaussianProbabilitiesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CCDFGetGaussianProbabilitiesTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] gaussianProbabilities, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CCDFGetProbabilitiesTrace", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CCDFGetProbabilitiesTrace(HandleRef session, string channelString, out double x0, out double dx, [Out] double[] probabilities, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CheckToolkitError", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CheckToolkitError(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CloseSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CloseSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_ConfigureDownlinkTestModel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ConfigureDownlinkTestModel(HandleRef session, int downlinkTestModel, double systemBandwidth);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_ConfigureFullyFilledPUSCHChannels", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ConfigureFullyFilledPUSCHChannels(HandleRef session, int pUSCHModulationScheme, double systemBandwidth);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CreateMIMOWaveformsComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CreateMIMOWaveformsComplexF64(HandleRef session, int reset, double[] t0, double[] dt, [Out] niComplexNumber[] waveforms, int numberofAntennas, int individualWaveformSize, out int actualNumSamplesinEachWfm, out int generationDone);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CreateWaveformComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CreateWaveformComplexF64(HandleRef session, int reset, out double t0, out double dt, [Out] niComplexNumber[] waveform, int waveformSize, out int actualNumWaveformSamples, out int generationDone);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_EARFCNtoCarrierFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_EARFCNtoCarrierFrequency(int eARFCN, int duplexMode, out double carrierFrequency);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_Get2DArrayAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_Get2DArrayAttributeI32(HandleRef session, string channelString, niLTESGProperties attributeID, [Out] int[,] dataArray, int numberofRows, int numberofColumns);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_GetErrorString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_GetErrorString(HandleRef session, int errorCode, StringBuilder errorMessage, int errorMessageLength);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_GetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_GetScalarAttributeF64(HandleRef session, string channelString, niLTESGProperties attributeID, out double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_GetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_GetScalarAttributeI32(HandleRef session, string channelString, niLTESGProperties attributeID, out int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_GetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_GetVectorAttributeI32(HandleRef session, string channelString, niLTESGProperties attributeID, [Out] int[] dataArray, int dataArraySize, out int actualNumDataArrayElements);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_GetVectorAttributeString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_GetVectorAttributeString(HandleRef session, string channelString, niLTESGProperties attributeID, StringBuilder attributeValue, int bufferSize, out int numberofCharsWritten);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_OpenSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_OpenSession(string sessionName, int toolkitCompatibilityVersion, out IntPtr session, out int isNewSession);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ResetAttribute(HandleRef session, string channelString, niLTESGProperties attributeID);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_ResetSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ResetSession(HandleRef session);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGClearDatabase", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGClearDatabase(HandleRef rfsgHandle, string channelString, string waveform);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGConfigurePowerLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGConfigurePowerLevel(HandleRef rfsgHandle, string channelString, string script, double powerLevel);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGConfigureScript", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGConfigureScript(HandleRef rfsgHandle, string channelString, string script, double powerLevel);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGCreateAndDownloadMIMOWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGCreateAndDownloadMIMOWaveforms(HandleRef session, IntPtr[] rfsgHandles, String[] hardwareChannelStrings, int numberofAntennas, string waveformName);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGCreateAndDownloadWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGCreateAndDownloadWaveform(HandleRef session, HandleRef rfsgHandle, string hardwareChannelString, string waveformName);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGRetrieveHeadroom", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGRetrieveHeadroom(HandleRef rfsgHandle, string channelString, string waveformName, out double headroom);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGRetrieveIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGRetrieveIQRate(HandleRef rfsgHandle, string channelString, string waveformName, out double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGRetrieveIQRateAllWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGRetrieveIQRateAllWaveforms(HandleRef rfsgHandle, string channelString, string script, out double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGRetrieveMinimumHeadroomAllWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGRetrieveMinimumHeadroomAllWaveforms(HandleRef rfsgHandle, string channelString, string script, out double headroom);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGStoreHeadroom", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGStoreHeadroom(HandleRef rfsgHandle, string channelString, string waveformName, double headroom);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_RFSGStoreIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_RFSGStoreIQRate(HandleRef rfsgHandle, string channelString, string waveformName, double iQRate);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_Set2DArrayAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_Set2DArrayAttributeI32(HandleRef session, string channelString, niLTESGProperties attributeID, int[,] dataArray, int numberofRows, int numberofColumns);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_SetScalarAttributeF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_SetScalarAttributeF64(HandleRef session, string channelString, niLTESGProperties attributeID, double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_SetScalarAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_SetScalarAttributeI32(HandleRef session, string channelString, niLTESGProperties attributeID, int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_SetVectorAttributeI32", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_SetVectorAttributeI32(HandleRef session, string channelString, niLTESGProperties attributeID, int[] dataArray, int dataArraySize);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_SetVectorAttributeString", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_SetVectorAttributeString(HandleRef session, string channelString, niLTESGProperties attributeID, string attributeValue);

            // 2.0 functions
            [DllImport(nativeDllName, EntryPoint = "niLTESG_ConfigureFullyFilledPUSCHFrame", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ConfigureFullyFilledPUSCHFrame(HandleRef session, int duplexMode, int PUSCHModulationScheme, double systemBandwidth, int ULDLConfiguration);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_ConfigureFullyFilledPUCCHFrame", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ConfigureFullyFilledPUCCHFrame(HandleRef session, int duplexMode, int pUCCHFormat, int n_PUCCH_1, double pUCCHpowerdB, int uLDLConfiguration);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_LoadConfigurationFromFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_LoadConfigurationFromFile(HandleRef session, string filePath, int resetSession);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_SaveConfigurationToFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_SaveConfigurationToFile(HandleRef session, string filePath, int operation);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_CreateAndWriteWaveformsToFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_CreateAndWriteWaveformsToFile(HandleRef session, string filePath, int fileOperation);

            [DllImport(nativeDllName, EntryPoint = "niLTESG_ReadWaveformFromFile", CallingConvention = CallingConvention.StdCall)]
            public static extern int niLTESG_ReadWaveformFromFile(string filePath, string waveformName, Int64 offset, Int64 count, out double t0,
                out double dt, [Out] niComplexNumber[] waveform, int waveformSize, out int actualNumWaveformSamples, out double iQrate,
                out double headroom, out int eof);
        }

        private int TestForError(HandleRef[] rfsgHandles, int status)
        {
            foreach (HandleRef rfsgHandle in rfsgHandles)
                TestForError(rfsgHandle, status);
            return status;
        }
        private int TestForError(HandleRef rfsgHandle, int status)
        {
            if (status < 0)
            {
                ThrowError(rfsgHandle, status);
            }
            return status;
        }
        private int TestForError(int status)
        {
            if (status < 0)
            {
                ThrowError(status);
            }
            return status;
        }

        private static int TestForStaticError(int status)
        {
            if (status < 0)
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder(niLTESGConstants.MaxErrorString);
                HandleRef dummyHandle = new HandleRef();
                PInvoke.niLTESG_GetErrorString(dummyHandle, status, msg, niLTESGConstants.MaxErrorString);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
            }
            return status;
        }

        private int ThrowError(HandleRef rfsgHandle, int status)
        {
            StringBuilder msg = new StringBuilder(niLTESGConstants.MaxErrorString);
            niRFSG.ErrorMessage(rfsgHandle, status, msg);
            if (String.IsNullOrEmpty(msg.ToString()))
                ThrowError(status);
            throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), status);
        }
        private int ThrowError(int code)
        {
            System.Text.StringBuilder msg = new System.Text.StringBuilder(niLTESGConstants.MaxErrorString);
            GetErrorString(code, msg, niLTESGConstants.MaxErrorString);
            throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
        }
    }

    public class niLTESGConstants
    {
        public const int DuplexModeDlFdd = 0;

        public const int DuplexModeUlFdd = 1;

        public const int DuplexModeDlTdd = 2;

        public const int DuplexModeUlTdd = 3;

        public const int DuplexModeDlAndUlTdd = 4;

        public const int CyclicPrefixModeNormal = 0;

        public const int CyclicPrefixModeExtended = 1;

        public const int False = 0;

        public const int True = 1;

        public const int PowerScalingTypeReferenceSignalPower = 0;

        public const int PowerScalingTypeTotalPower = 1;

        public const int DlCellSpecificRatioPB0 = 0;

        public const int DlCellSpecificRatioPB1 = 1;

        public const int DlCellSpecificRatioPB2 = 2;

        public const int DlCellSpecificRatioPB3 = 3;

        public const int DlSyncSignalPort0 = 0;

        public const int DlSyncSignalPort1 = 1;

        public const int DlSyncSignalPort2 = 2;

        public const int DlSyncSignalPort3 = 3;

        public const int DlSyncSignalPortAll = 4;

        public const int DlPbchPayloadDataTypePnSequence = 0;

        public const int DlPbchPayloadDataTypeUserDefinedBits = 1;

        public const int DlPbchPayloadDataTypeAutoConfigurePayload = 2;

        public const int DlPhichResource1by6 = 0;

        public const int DlPhichResource1by2 = 1;

        public const int DlPhichResource1 = 2;

        public const int DlPhichResource2 = 3;

        public const int DlPhichDurationNormal = 0;

        public const int DlPhichDurationExtended = 1;

        public const int DlPhichPayloadDataTypePnSequence = 0;

        public const int DlPhichPayloadDataTypeAllAcks = 1;

        public const int DlPhichPayloadDataTypeAllNacks = 2;

        public const int DlPhichPayloadDataTypeUserDefinedData = 3;

        public const int PayloadDataTypePnSequence = 0;

        public const int PayloadDataTypeUserDefinedBits = 1;

        public const int DlPdcchFormat0 = 0;

        public const int DlPdcchFormat1 = 1;

        public const int DlPdcchFormat2 = 2;

        public const int DlPdcchFormat3 = 3;

        public const int DlPdschTransmissionModeSingleAntenna = 0;

        public const int DlPdschTransmissionModeTransmitDiversity = 1;

        public const int DlPdschTransmissionModeSpatialMultiplexing = 2;

        public const int DlPdschPrecodingModeWithCdd = 0;

        public const int DlPdschPrecodingModeWithoutCdd = 1;

        public const int ModulationSchemeQpsk = 0;

        public const int ModulationScheme16Qam = 1;

        public const int ModulationScheme64Qam = 2;

        public const int UlHoppingModeGroupHopping = 0;

        public const int UlHoppingModeSequenceHopping = 1;

        public const int UlPucchFormat1 = 0;

        public const int UlPucchFormat1a = 1;

        public const int UlPucchFormat1b = 2;

        public const int UlPucchFormat2 = 3;

        public const int UlPucchFormat2a = 4;

        public const int UlPucchFormat2b = 5;

        public const int UlSrsEvenSubcarriers = 0;

        public const int UlSrsOddSubcarriers = 1;

        public const int ToolkitCompatibilityVersion010000 = 10000;

        public const int MaxErrorString = 1024;

        public const int DlTestModelETm11 = 0;

        public const int DlTestModelETm12 = 1;

        public const int DlTestModelETm2 = 2;

        public const int DlTestModelETm31 = 3;

        public const int DlTestModelETm32 = 4;

        public const int DlTestModelETm33 = 5;

        public const int FileOperationModeOpen = 0;

        public const int FileOperationModeOpenOrCreate = 1;

        public const int FileOperationModeCreateOrReplace = 2;

        public const int FileOperationModeCreate = 3;

    }

    public enum niLTESGProperties
    {
        /// <summary>
        /// double
        /// </summary>
        ActualHeadroom = 205,

        /// <summary>
        /// int
        /// </summary>
        AutoHeadroomEnabled = 218,

        /// <summary>
        /// int
        /// </summary>
        AwgnEnabled = 165,

        /// <summary>
        /// int
        /// </summary>
        BasebandFilterEnabled = 217,

        /// <summary>
        /// double
        /// </summary>
        CarrierFrequencyOffset = 160,

        /// <summary>
        /// double
        /// </summary>
        CarrierToNoiseRatio = 166,

        /// <summary>
        /// int
        /// </summary>
        CellId = 11,

        /// <summary>
        /// double
        /// </summary>
        ClipRate = 207,

        /// <summary>
        /// int
        /// </summary>
        CyclicPrefixMode = 7,

        /// <summary>
        /// int
        /// </summary>
        DlCellSpecificRatio = 12,

        /// <summary>
        /// int
        /// </summary>
        DlCellSpecificReferenceSignalsEnabled = 24,

        /// <summary>
        /// int
        /// </summary>
        DlNumberOfPcfichChannels = 21,

        /// <summary>
        /// int
        /// </summary>
        DlNumberOfPdcchChannels = 22,

        /// <summary>
        /// int
        /// </summary>
        DlNumberOfPdschChannels = 23,

        /// <summary>
        /// int
        /// </summary>
        DlNumberOfPhichChannels = 20,

        /// <summary>
        /// int
        /// </summary>
        DlOcngEnabled = 95,

        /// <summary>
        /// double
        /// </summary>
        DlOcngPower = 96,

        /// <summary>
        /// int
        /// </summary>
        DlPbchEnabled = 30,

        /// <summary>
        /// int
        /// </summary>
        DlPbchInitialSystemFrameNumber = 31,

        /// <summary>
        /// int
        /// </summary>
        DlPbchPayloadDataType = 171,

        /// <summary>
        /// int
        /// </summary>
        DlPbchPayloadPnOrder = 172,

        /// <summary>
        /// int
        /// </summary>
        DlPbchPayloadPnSeed = 173,

        /// <summary>
        /// int[]
        /// </summary>
        DlPbchPayloadUserDefinedBits = 175,

        /// <summary>
        /// double
        /// </summary>
        DlPbchPower = 32,

        /// <summary>
        /// int
        /// </summary>
        DlPbchScramblingEnabled = 177,

        /// <summary>
        /// int
        /// </summary>
        DlPcfichControlFormatIndicator = 47,

        /// <summary>
        /// double
        /// </summary>
        DlPcfichPower = 49,

        /// <summary>
        /// int
        /// </summary>
        DlPcfichScramblingEnabled = 48,

        /// <summary>
        /// int
        /// </summary>
        DlPcfichSubframeNumber = 46,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchCceStartIndex = 61,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchFormat = 60,

        /// <summary>
        /// double
        /// </summary>
        DlPdcchNilElementPower = 202,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchPayloadDataType = 55,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchPayloadPnOrder = 56,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchPayloadPnSeed = 57,

        /// <summary>
        /// int[]
        /// </summary>
        DlPdcchPayloadUserDefinedBits = 59,

        /// <summary>
        /// double
        /// </summary>
        DlPdcchPower = 63,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchRnti = 54,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchScramblingEnabled = 53,

        /// <summary>
        /// int
        /// </summary>
        DlPdcchSubframeNumber = 51,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword0ModulationScheme = 77,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword0PayloadDataType = 69,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword0PayloadPnOrder = 70,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword0PayloadPnSeed = 71,

        /// <summary>
        /// int[]
        /// </summary>
        DlPdschCodeword0PayloadUserDefinedBits = 73,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword1Enabled = 223,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword1ModulationScheme = 86,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword1PayloadDataType = 78,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword1PayloadPnOrder = 79,

        /// <summary>
        /// int
        /// </summary>
        DlPdschCodeword1PayloadPnSeed = 80,

        /// <summary>
        /// int[]
        /// </summary>
        DlPdschCodeword1PayloadUserDefinedBits = 82,

        /// <summary>
        /// int
        /// </summary>
        DlPdschNumberOfLayers = 87,

        /// <summary>
        /// double
        /// </summary>
        DlPdschPower = 94,

        /// <summary>
        /// int
        /// </summary>
        DlPdschPrecodingCodebookIndex = 90,

        /// <summary>
        /// int
        /// </summary>
        DlPdschPrecodingMode = 89,

        /// <summary>
        /// int
        /// </summary>
        DlPdschRnti = 67,

        /// <summary>
        /// int
        /// </summary>
        DlPdschScramblingEnabled = 76,

        /// <summary>
        /// int
        /// </summary>
        DlPdschSubframeNumber = 66,

        /// <summary>
        /// int
        /// </summary>
        DlPdschTransmissionMode = 88,

        /// <summary>
        /// char
        /// </summary>
        DlPdschVirtualResourceBlockAllocation = 92,

        /// <summary>
        /// int
        /// </summary>
        DlPhichDuration = 36,

        /// <summary>
        /// int
        /// </summary>
        DlPhichPayloadDataType = 37,

        /// <summary>
        /// int
        /// </summary>
        DlPhichPayloadPnOrder = 38,

        /// <summary>
        /// int
        /// </summary>
        DlPhichPayloadPnSeed = 39,

        /// <summary>
        /// int[]
        /// </summary>
        DlPhichPayloadUserDefinedData = 41,

        /// <summary>
        /// double
        /// </summary>
        DlPhichPower = 44,

        /// <summary>
        /// int
        /// </summary>
        DlPhichResource = 35,

        /// <summary>
        /// int
        /// </summary>
        DlPhichScramblingEnabled = 43,

        /// <summary>
        /// int
        /// </summary>
        DlPhichSubframeNumber = 34,

        /// <summary>
        /// int
        /// </summary>
        DlPrimarySyncEnabled = 26,

        /// <summary>
        /// double
        /// </summary>
        DlPrimarySyncPower = 27,

        /// <summary>
        /// int
        /// </summary>
        DlSecondarySyncEnabled = 28,

        /// <summary>
        /// double
        /// </summary>
        DlSecondarySyncPower = 29,

        /// <summary>
        /// int
        /// </summary>
        DlSyncSignalPort = 13,

        /// <summary>
        /// int
        /// </summary>
        DuplexMode = 5,

        /// <summary>
        /// double
        /// </summary>
        Headroom = 203,

        /// <summary>
        /// double
        /// </summary>
        IDcOffset = 162,

        /// <summary>
        /// double
        /// </summary>
        IqGainImbalance = 164,

        /// <summary>
        /// int
        /// </summary>
        IqWaveformSize = 206,

        /// <summary>
        /// int
        /// </summary>
        NumberOfAntennas = 10,

        /// <summary>
        /// int
        /// </summary>
        NumberOfFrames = 4,

        /// <summary>
        /// int
        /// </summary>
        OversamplingFactor = 8,

        /// <summary>
        /// int
        /// </summary>
        PowerScalingType = 228,

        /// <summary>
        /// double
        /// </summary>
        QDcOffset = 163,

        /// <summary>
        /// double
        /// </summary>
        QuadratureSkew = 161,

        /// <summary>
        /// double
        /// </summary>
        RecommendedIqRate = 204,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfMeanPowerPercentile = 210,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneHundredThousandthPower = 215,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneHundredthPower = 212,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneMillionthPower = 216,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneTenThousandthPower = 214,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneTenthPower = 211,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfOneThousandthPower = 213,

        /// <summary>
        /// double
        /// </summary>
        ResultCcdfPeakToAveragePowerRatio = 208,

        /// <summary>
        /// double
        /// </summary>
        SampleClockOffset = 159,

        /// <summary>
        /// double
        /// </summary>
        SystemBandwidth = 6,

        /// <summary>
        /// int
        /// </summary>
        ToolkitCompatibilityVersion = 200,

        /// <summary>
        /// int
        /// </summary>
        Ul3gppCyclicShiftEnabled = 126,

        /// <summary>
        /// int
        /// </summary>
        UlDftShiftEnabled = 16,

        /// <summary>
        /// int
        /// </summary>
        UlHoppingEnabled = 17,

        /// <summary>
        /// int
        /// </summary>
        UlHoppingMode = 18,

        /// <summary>
        /// int
        /// </summary>
        UlNumberOfPuschChannels = 98,

        /// <summary>
        /// int
        /// </summary>
        UlPuschCyclicShiftIndex0 = 127,

        /// <summary>
        /// int
        /// </summary>
        UlPuschCyclicShiftIndex1 = 128,

        /// <summary>
        /// int
        /// </summary>
        UlPuschDeltaSs = 129,

        /// <summary>
        /// int
        /// </summary>
        UlPuschModulationScheme = 120,

        /// <summary>
        /// int
        /// </summary>
        UlPuschNDmrs1 = 124,

        /// <summary>
        /// int
        /// </summary>
        UlPuschNDmrs2 = 125,

        /// <summary>
        /// int
        /// </summary>
        UlPuschNumberOfResourceBlocks = 122,

        /// <summary>
        /// int
        /// </summary>
        UlPuschPayloadDataType = 112,

        /// <summary>
        /// int
        /// </summary>
        UlPuschPayloadPnOrder = 113,

        /// <summary>
        /// int
        /// </summary>
        UlPuschPayloadPnSeed = 114,

        /// <summary>
        /// int[]
        /// </summary>
        UlPuschPayloadUserDefinedBits = 116,

        /// <summary>
        /// double
        /// </summary>
        UlPuschPower = 123,

        /// <summary>
        /// int
        /// </summary>
        UlPuschResourceBlockOffset = 121,

        /// <summary>
        /// int
        /// </summary>
        UlPuschScramblingEnabled = 119,

        /// <summary>
        /// int
        /// </summary>
        UlPuschSubframeNumber = 110,

        /// <summary>
        /// int
        /// </summary>
        UlRnti = 14,

        /// <summary>
        /// double
        /// </summary>
        WindowLength = 9,

        /// <summary>
        /// int
        /// </summary>
        AllIqImpairmentsEnabled = 243,

        /// <summary>
        /// int
        /// </summary>
        UlDlConfiguration = 229,

        /// <summary>
        /// int
        /// </summary>
        UlNumberOfPucchChannels = 99,

        /// <summary>
        /// int
        /// </summary>
        UlPucchCyclicShiftIndex0 = 146,

        /// <summary>
        /// int
        /// </summary>
        UlPucchCyclicShiftIndex1 = 147,

        /// <summary>
        /// int
        /// </summary>
        UlPucchDeltaPucchShift = 144,

        /// <summary>
        /// int
        /// </summary>
        UlPucchFormat = 134,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNCs1 = 141,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNPucch1 = 142,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNPucch2 = 143,

        /// <summary>
        /// int
        /// </summary>
        UlPucchNRb2 = 140,

        /// <summary>
        /// int
        /// </summary>
        UlPucchPayloadDataType = 135,

        /// <summary>
        /// int
        /// </summary>
        UlPucchPayloadPnOrder = 136,

        /// <summary>
        /// int
        /// </summary>
        UlPucchPayloadPnSeed = 137,

        /// <summary>
        /// int[]
        /// </summary>
        UlPucchPayloadUserDefinedBits = 139,

        /// <summary>
        /// double
        /// </summary>
        UlPucchPower = 148,

        /// <summary>
        /// int
        /// </summary>
        UlPucchSubframeNumber = 133,

        /// <summary>
        /// int
        /// </summary>
        UlSrsBSrs = 154,

        /// <summary>
        /// int
        /// </summary>
        UlSrsCSrs = 153,

        /// <summary>
        /// int
        /// </summary>
        UlSrsEnabled = 231,

        /// <summary>
        /// int
        /// </summary>
        UlSrsISrs = 233,

        /// <summary>
        /// int
        /// </summary>
        UlSrsKTc = 156,

        /// <summary>
        /// int
        /// </summary>
        UlSrsMaxUpPtsEnabled = 234,

        /// <summary>
        /// int
        /// </summary>
        UlSrsNRrc = 158,

        /// <summary>
        /// int
        /// </summary>
        UlSrsNsrsCs = 155,

        /// <summary>
        /// int
        /// </summary>
        UlSrsNumberOfFormat4Prach = 235,

        /// <summary>
        /// double
        /// </summary>
        UlSrsPower = 152,

        /// <summary>
        /// int
        /// </summary>
        UlSrsSimultaneousAnAndSrs = 236,

        /// <summary>
        /// int
        /// </summary>
        UlSrsSubframeConfigurationIndex = 232,

        /// <summary>
        /// int
        /// </summary>
        SpecialSubframeConfiguration = 230,

        /// <summary>
        /// int
        /// </summary>
        SystemFrameNumber = 473,
    }
}