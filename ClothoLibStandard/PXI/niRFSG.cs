namespace NationalInstruments.ModularInstruments.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class niRFSG : object, System.IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;
        private bool _disposed = true;
        // NIRFSG_MAX_ERROR_MESSAGE_SIZE is 1024
        private const int maxErrorMessageSize = 1024;

        public System.Runtime.InteropServices.HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        ~niRFSG() { Dispose(false); }

        /// <summary>
        /// Initializes the NI-RFSG device.
        /// 
        /// niRFSG_init performs the following initialization actions:
        /// 
        /// - Creates a new instrument driver session. 
        /// - Opens a session to the device you specify for the resourceName
        ///   parameter. 
        /// - If the reset parameter is set to VI_TRUE, niRFSG_init 
        ///   resets  the device to a known state. 
        /// - Returns a ViSession handle that you use to identify the
        ///   NI-RFSG device in all subsequent NI-RFSG function calls.
        /// 
        /// NOTE: Before initializing the NI 567X, an NI 54XX AWG module must be associated with the NI 5610 upconverter module in MAX. After association, niRFSG_init initializes both modules. To change the AWG association, modify the NI 5610 Properties page in MAX, or use the niRFSG_InitWithOptions function to override the association in MAX. Refer to the Getting Started Guide for your NI-RFSG device, installed at Start;Programs;National Instruments;NI-RFSG;Documentation for information on MAX association.
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// Resource Name specifies the resource name of the device to initialize. 
        ///  
        /// Examples 
        /// -----------------------------------------------------------
        /// myDAQmxDevice       NI-DAQmx device, device name = 
        ///                     "myDAQmxDevice"
        /// myLogicalName       IVI logical name or virtual instrument, 
        ///                     name = "myLogicalName" 
        /// 
        /// For NI-DAQmx devices, the syntax is just the device name specified in MAX, as shown in Example 1. Typical default names for NI-DAQmx devices in MAX are Dev 2 or PXI Slot 2. You can rename an NI-DAQmx device by right-clicking on the name in MAX and entering a new name.
        /// 
        /// You can also pass in the name of an IVI logical name configured with the IVI Configuration utility. For additional information, refer to the IVI Drivers topic of the MAX Help. 
        /// 
        /// CAUTION: NI-DAQmx device names are not case-sensitive. However, all IVI names, such as logical names, are case-sensitive. If you use an IVI logical name, make sure the name is identical to the name shown in the IVI Configuration Utility.
        /// 
        /// Default Value: None
        /// 
        /// 
        /// </param>
        /// <param name="ID_Query">
        /// Specify whether you want NI-RFSG to perform an ID query.  
        /// 
        /// </param>
        /// <param name="Reset">
        /// Specify whether you want the to reset the NI-RFSG device during the initialization procedure.
        /// 
        /// Defined Values:
        /// VI_TRUE  (1) - Reset device.
        /// VI_FALSE (0) - Do not reset device. (default)
        /// 
        /// </param>
        ///
        public niRFSG(string Resource_Name, bool ID_Query, bool Reset)
        {
            System.IntPtr instrumentHandle;
            Int16 resetVal = Convert.ToInt16(Reset);
            Int16 idQueryVal = Convert.ToInt16(ID_Query);

            int pInvokeResult = PInvoke.init(Resource_Name, idQueryVal, resetVal, out instrumentHandle);
            _handle = new HandleRef(this, instrumentHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// Initializes the NI-RFSG device (the AWG and upconverter).  niRFSG_InitWithOptions has the capability of receiving the AWG resource name through the option string.
        /// 
        /// niRFSG_InitWithOptions performs the following initialization actions:
        /// 
        /// - Creates a new IVI instrument driver session.
        /// 
        /// - Opens a session to the specified device using the interface and address you specify for the resourceName parameter.
        /// 
        /// - If the reset parameter is set to VI_TRUE, niRFSG_InitWithOptions resets all attributes to their default values.
        /// 
        /// - Returns a ViSession handle that you use to identify the NI-RFSG device in all subsequent NI-RFSG function calls.
        /// 
        /// - Sends attribute values specified in the option string parameter to NI-RFSG.
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// Resource Name specifies the resource name of the device to initialize.  
        /// 
        /// Examples 
        /// -----------------------------------------------------------
        /// myDAQmxDevice       NI-DAQmx device, device name = 
        ///                     "myDAQmxDevice"
        /// myLogicalName       IVI logical name or virtual instrument, 
        ///                     name = "myLogicalName"
        /// 
        /// For NI-DAQmx devices, the syntax is just the device name specified in MAX, as shown in Example 1. Typical default names for NI-DAQmx devices in MAX are Dev1 or PXI1Slot1. You can rename an NI-DAQmx device by right-clicking on the name in MAX and entering a new name.
        /// 
        /// You can also pass in the name of an IVI logical name configured with the IVI Configuration utility. For additional information, refer to the IVI Drivers topic of the MAX Help. 
        /// 
        /// CAUTION: NI-DAQmx device names are not case-sensitive. However, all IVI names, such as logical names, are case-sensitive. If you use an IVI logical name, make sure the name is identical to the name shown in the IVI Configuration Utility.
        /// 
        /// 
        /// Default Value:  None
        /// 
        /// 
        /// </param>
        /// <param name="ID_Query">
        /// Specify whether you want NI-RFSG to perform an ID query.
        /// 
        /// </param>
        /// <param name="Reset">
        /// Specify whether you want the to reset the NI-RFSG device during the initialization procedure.
        /// 
        /// Defined Values:
        /// VI_TRUE  (1) - Reset device.
        /// VI_FALSE (0) - Do not reset device. (default)
        /// </param>
        /// <param name="Option_String">
        /// Use this parameter to set the initial value of certain attributes for the session.  The following table lists the attributes and the name you pass in this parameter to identify the attribute.
        /// 
        /// Name              Attribute   
        /// --------------------------------------------
        /// RangeCheck        NIRFSG_ATTR_RANGE_CHECK
        /// QueryInstrStatus  NIRFSG_ATTR_QUERY_INSTRUMENT_STATUS   
        /// Cache             NIRFSG_ATTR_CACHE   
        /// RecordCoercions   NIRFSG_ATTR_RECORD_COERCIONS
        /// DriverSetup       NIRFSG_ATTR_DRIVER_SETUP
        /// 
        /// The format of this string is, "AttributeName=Value" where AttributeName is the name of the attribute and Value is the value to which the attribute will be set.  To set multiple attributes, separate their assignments with a comma.  
        /// 
        /// Example Option String:
        /// "RangeCheck=1,QueryInstrStatus=0,Cache=1,DriverSetup=AWG:pxi1slot4"
        /// 
        /// 
        /// 
        /// </param>
        ///
        public niRFSG(string Resource_Name, bool ID_Query, bool Reset, string Option_String)
        {
            System.IntPtr instrumentHandle;
            Int16 resetVal = Convert.ToInt16(Reset);
            Int16 idQueryVal = Convert.ToInt16(ID_Query);

            int pInvokeResult = PInvoke.InitWithOptions(Resource_Name, idQueryVal, resetVal, Option_String, out instrumentHandle);
            this._handle = new HandleRef(this, instrumentHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// Configures the frequency and power level of the RF output signal. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ConfigureRF.
        /// 
        /// </summary>
        ///
        /// <param name="Frequency">
        /// Specifies the frequency of the generated RF signal. For arbitrary waveform generation, this parameter specifies the center frequency of the signal. The units are expressed in Hertz.
        /// 
        /// NI-RFSG sets NIRFSG_ATTR_FREQUENCY to this value. Refer to NIRFSG_ATTR_FREQUENCY for valid values.
        /// 
        /// </param>
        /// <param name="Power_Level">
        /// Specifies the power level of the generated RF signal. If an arbitrary waveform is being generated, this parameter specifies the average power of the signal. The units are expressed in dBm.
        /// 
        /// NI-RFSG sets NIRFSG_ATTR_POWER_LEVEL to this value. Refer to NIRFSG_ATTR_POWER_LEVEL for valid values.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureRF(System.Double Frequency, System.Double Power_Level)
        {
            int pInvokeResult = PInvoke.ConfigureRF(this._handle, Frequency, Power_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the NI-RFSG device to generate a CW sine tone, apply IQ (vector) modulation to the RF output signal, or play arbitrary waveforms according to scripts.
        /// 
        /// The NI-RFSG device must be in the Configuration or Committed state before calling this function.
        /// 
        /// 
        /// </summary>
        ///
        /// <param name="Generation_Mode">
        /// Specifies the mode used by NI-RFSG for generating an RF output signal. NI-RFSG sets the NIRFSG_ATTR_GENERATION_MODE attribute to this value.
        /// 
        /// After initializing the RF signal generator, or calling niRFSG_reset or niRFSG_ResetDevice, this parameter is set to CW. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureGenerationMode(int Generation_Mode)
        {
            int pInvokeResult = PInvoke.ConfigureGenerationMode(this._handle, Generation_Mode);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Writes an arbitrary waveform to the NI-RFSG device. This function inputs the I and Q vectors of a complex baseband signal. The NI-RFSG device must be in the Configuration state before calling niRFSG_WriteArbWaveform.
        /// 
        /// NOTE:  To use real-value signals, feed the signal as the iData and an array of zeros of the same size as the qData. You can also feed the signal as the iData and pass VI_NULL as the qData. 
        /// 
        /// </summary>
        ///
        /// <param name="Name">
        /// Specifies the name used to store the waveform. 
        /// 
        /// Default Value: "" (empty string)
        /// </param>
        /// <param name="Number_Of_Samples">
        /// The number of samples in both the iData and qData arrays. The iData and qData arrays must have the same length. If the waveform quantum (refer to NIRFSG_ATTR_ARB_WAVEFORM_QUANTUM) is q, then the number of samples should be a multiple of q. The specified number of samples cannot be 0.
        /// </param>
        /// <param name="IData">
        /// NI-RFSG uses the values of this array as the I part of the complex baseband signal. 
        /// </param>
        /// <param name="QData">
        /// NI-RFSG uses the values of this array as the Q part of the complex baseband signal.
        /// </param>
        /// <param name="More_Data_Pending">
        /// Set this value to VI_TRUE to allow data to be appended to the waveform later. Splitting the waveform into multiple data blocks can reduce the memory requirements of the write operation. You can append data to a previously written waveform by using the same waveform name. Set this value to VI_FALSE to indicate that the waveform has no more data.
        /// 
        /// Defined Values:
        /// VI_TRUE - Indicates more data is coming from the waveform.
        /// VI_FALSE - Indicates the waveform has no more data.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int WriteArbWaveform(string Name, int Number_Of_Samples, System.Double[] IData, System.Double[] QData, bool More_Data_Pending)
        {
            Int16 moreDataPending = Convert.ToInt16(More_Data_Pending);
            int pInvokeResult = PInvoke.WriteArbWaveform(this._handle, Name, Number_Of_Samples, IData, QData, moreDataPending);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies the waveform that is generated upon a call to the niRFSG_Initiate when the Generation_Mode parameter of the niRFSG_ConfigureGenerationMode is set to NIRFSG_VAL_ARB_WAVEFORM. You must specify a waveform name if you have written multiple waveforms. The NI-RFSG device must be in the Configuration or Committed state before calling this VI. 
        /// </summary>
        ///
        /// <param name="Name">
        /// Specifies the name of the stored waveform to generate. NI-RFSG sets NIRFSG_ATTR_ARB_SELECTED_WAVEFORM to this value.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int SelectArbWaveform(string Name)
        {
            int pInvokeResult = PInvoke.SelectArbWaveform(this._handle, Name);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Deletes a waveform from the pool of waveforms currently defined. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ClearArbWaveform.
        /// </summary>
        ///
        /// <param name="Name">
        /// Name of the stored waveform to delete.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ClearArbWaveform(string Name)
        {
            int pInvokeResult = PInvoke.ClearArbWaveform(this._handle, Name);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Deletes all waveforms from the pool of waveforms currently defined. Deletes all scripts from the pool of scripts currently defined. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ClearAllArbWaveforms.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ClearAllArbWaveforms()
        {
            int pInvokeResult = PInvoke.ClearAllArbWaveforms(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the signal bandwidth of the arbitrary waveform. The NI-RFSG device must be in the Configuration or Committed state before calling this function.
        /// </summary>
        ///
        /// <param name="Signal_Bandwidth">
        /// Specifies the signal bandwidth used by NI-RFSG for generating an RF output signal. NI-RFSG sets the NIRFSG_ATTR_SIGNAL_BANDWIDTH  attribute to this value. 
        /// 
        /// Default Value: 100 Hz
        /// 
        /// Defined Values:
        /// 0 Hz to 20 MHz
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureSignalBandwidth(System.Double Signal_Bandwidth)
        {
            int pInvokeResult = PInvoke.ConfigureSignalBandwidth(this._handle, Signal_Bandwidth);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the start trigger for software triggering.  The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ConfigureSoftwareStartTrigger.
        /// 
        /// Refer to niRFSG_SendSoftwareEdgeTrigger for more information on using the software trigger.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureSoftwareStartTrigger()
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareStartTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the start trigger for digital edge triggering. Signal output begins when a start trigger is recieved. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ConfigureDigitalEdgeStartTrigger.
        /// </summary>
        ///
        /// <param name="Source">
        /// The source terminal for the digital edge start trigger. NI-RFSG sets NIRFSG_ATTR_DIGITAL_EDGE_START_TRIGGER_SOURCE to this value. Refer to NIRFSG_ATTR_DIGITAL_EDGE_START_TRIGGER_SOURCE for possible values.
        /// 
        /// Default Value: "" (empty string)
        /// 
        /// </param>
        /// <param name="Edge">
        /// Specifies the active edge for the start trigger. NI-RFSG sets NIRFSG_ATTR_DIGITAL_EDGE_START_TRIGGER_EDGE to this value. Refer to NIRFSG_ATTR_DIGITAL_EDGE_START_TRIGGER_EDGE for possible values.
        /// 
        /// NOTE: Currently NIRFSG_VAL_FALLING_EDGE is unsupported.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureDigitalEdgeStartTrigger(string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeStartTrigger(this._handle, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the device to not wait for a start trigger after niRFSG_Initiate is called. Calling niRFSG_DisableStartTrigger is only necessary if the start trigger has been previously configured and now needs to be disabled.
        /// 
        /// The NI-RFSG device must be in the Configuration state before calling niRFSG_DisableStartTrigger.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int DisableStartTrigger()
        {
            int pInvokeResult = PInvoke.DisableStartTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the script trigger for software triggering.  The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ConfigureSoftwareStartTrigger. Refer to niRFSG_SendSoftwareEdgeTrigger for more information on using the software trigger.
        /// </summary>
        ///
        /// <param name="Trigger_Identifier">
        /// This parameter identifies which of the four available script triggers is disabled.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureSoftwareScriptTrigger(string Trigger_Identifier)
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareScriptTrigger(this._handle, Trigger_Identifier);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the specified script trigger for digital edge triggering.
        /// 
        /// The NI-RFSG device must be in the Configuration or Committed state before calling this function.
        /// </summary>
        ///
        /// <param name="Trigger_Identifier">
        /// This parameter identifies which of the four available script triggers is disabled.
        /// </param>
        /// <param name="Source">
        /// The source terminal for the digital edge script trigger. NI-RFSG sets NIRFSG_ATTR_DIGITAL_EDGE_SCRIPT_TRIGGER_SOURCE to this value. Refer to NIRFSG_ATTR_DIGITAL_EDGE_SCRIPT_TRIGGER_SOURCE for possible values.
        /// 
        /// Default Value: "" (empty string)
        /// 
        /// </param>
        /// <param name="Edge">
        /// Specifies the active edge for the digital edge script trigger. NI-RFSG sets NIRFSG_ATTR_DIGITAL_EDGE_SCRIPT_TRIGGER_EDGE to this value. Refer to NIRFSG_ATTR_DIGITAL_EDGE_SCRIPT_TRIGGER_EDGE for possible values.
        /// 
        /// NOTE: Currently NIRFSG_VAL_FALLING_EDGE is unsupported.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureDigitalEdgeScriptTrigger(string Trigger_Identifier, string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeScriptTrigger(this._handle, Trigger_Identifier, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the device to not wait for the specified script trigger. Calling niRFSG_DisableScriptTrigger is only necessary if the script trigger has been previously configured and now needs to be disabled. The NI-RFSG device must be in the Configuration state before calling this function.
        /// </summary>
        /// <param name="Trigger_Identifier">
        /// This parameter identifies which of the four available script triggers is disabled.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int DisableScriptTrigger(string Trigger_Identifier)
        {
            int pInvokeResult = PInvoke.DisableScriptTrigger(this._handle, Trigger_Identifier);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Use niRFSG_SendSoftwareEdgeTrigger to force a particular trigger to occur. The specified trigger is generated regardless of whether the trigger has been configured as a software trigger.
        /// </summary>
        ///
        /// <param name="Trigger">
        /// Specifies the trigger to assert. NI-RFSG can assert a start trigger or a script trigger.
        /// 
        /// Default Value: NIRFSG_VAL_START_TRIGGER
        /// 
        /// </param>
        /// <param name="Trigger_Identifier">
        /// If the script trigger is selected, then this parameter identifies which of the four available script triggers is sent.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int SendSoftwareEdgeTrigger(int Trigger, string Trigger_Identifier)
        {
            int pInvokeResult = PInvoke.SendSoftwareEdgeTrigger(this._handle, Trigger, Trigger_Identifier);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Use this function to route signals (triggers and events) to the output terminal you specify. Refer to your device documentation for valid signal destinations. The NI-RFSG device must be in the Configuration or Committed state before calling this function.
        /// 
        /// If you export a signal with this function and commit the session, the signal is routed to the output terminal you specify. If you then reconfigure the signal to have a different output terminal, the previous output terminal is tristated when the session is next committed. If you change the output terminal to NIRFSG_VAL_DO_NOT_EXPORT_STR or an empty string when you commit the operation, the previous output terminal is tristated.
        /// 
        /// Any signals exported within a session persist after the session closes to prevent signal glitching between sessions. If you wish to have the terminal that the signal was exported to tristated when the session closes, change the output terminal for the exported signal to NIRFSG_VAL_DO_NOT_EXPORT_STR and commit the session again before closing it.
        /// 
        /// You can also tristate all PFI lines by setting the reset parameter in niRFSG_Initialize or niRFSG_reset.
        /// 
        /// 
        /// </summary>
        ///
        /// <param name="Signal">
        /// Signal (trigger or event) to export.
        /// 
        /// Defined Values:
        /// 
        /// -   NIRFSG_VAL_START_TRIGGER
        /// -   NIRFSG_VAL_SCRIPT_TRIGGER (requires Signal Identifier to describe a particular Script Trigger)
        /// -   NIRFSG_VAL_MARKER_EVENT (requires Signal Identifier to describe a particular Marker)
        /// 
        /// 
        /// </param>
        /// <param name="Signal_Identifier">
        /// Describes the signal being exported.
        /// 
        /// Defined Values:
        /// 
        ///  - "ScriptTrigger0"
        ///  - "ScriptTrigger1"
        ///  - "ScriptTrigger2"
        ///  - "ScriptTrigger3"
        ///  - "Marker0"
        ///  - "Marker1"
        ///  - "Marker2"
        ///  - "Marker3"
        ///  - "" (empty String) or VI_NULL
        /// 
        /// </param>
        /// <param name="Output_Terminal">
        /// Output terminal where the signal is exported. You can choose not to export any signal.
        /// 
        /// Defined Values:
        /// 
        /// - NIRFSG_VAL_PFI0_STR 
        /// - NIRFSG_VAL_PFI1_STR
        /// - NIRFSG_VAL_PFI4_STR
        /// - NIRFSG_VAL_PFI5_STR
        /// - NIRFSG_VAL_PXI_TRIG0_STR - NIRFSG_VAL_PXI_TRIG7_STR : the PXI trigger backplane
        /// - "" (empty string) or VI_NULL - the signal is not exported
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ExportSignal(int Signal, string Signal_Identifier, string Output_Terminal)
        {
            int pInvokeResult = PInvoke.ExportSignal(this._handle, Signal, Signal_Identifier, Output_Terminal);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the NI-RFSG device reference clock. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ConfigureRefClock.
        /// 
        /// </summary>
        ///
        /// <param name="Ref_Clock_Source">
        /// Specifies source of reference clock signal.Only certain combinations of clockSource and PXI Chassis Clock 10 are valid, as shown in the table below. Refer to NIRFSG_ATTR_REF_CLOCK_SOURCE for possible values.
        /// 
        /// Default Value: NIRFSG_VAL_ON_BOARD_CLOCK_STR
        /// 
        /// Valid Timing Configurations:
        /// 
        /// Ref Clock Setting        Valid PXI Chassis Clk10 Setting
        /// -----------------------------------------------------
        /// OnBoardClock             None, OnBoardClock
        /// RefIn                    None, RefIn
        /// PXI_CLK_10               None, RefIn
        /// 
        /// NI-RFSG sets NIRFSG_ATTR_REF_CLOCK_SOURCE to this value. 
        /// 
        /// Refer to Timing Configurations for more information on valid timing configurations.
        /// </param>
        /// <param name="Ref_Clock_Rate">
        /// Specifies the reference clock rate, expressed in Hertz. NI-RFSG sets NIRFSG_ATTR_REF_CLOCK_RATE to this value.
        /// 
        /// Default Value: 10E6 (10 MHz, only supported value)
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureRefClock(string Ref_Clock_Source, System.Double Ref_Clock_Rate)
        {
            int pInvokeResult = PInvoke.ConfigureRefClock(this._handle, Ref_Clock_Source, Ref_Clock_Rate);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies the signal to drive the 10 MHz reference clock to the PXI backplane. This option can only be configured when the PXI-5610 is in PXI Slot 2. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_ConfigurePXIChassisClk10.
        /// </summary>
        ///
        /// <param name="PXI_Clk_10_Source">
        /// Specifies the source for the signal that drives the 10 MHz reference clock on the PXI backplane. NI-RFSG sets NIRFSG_ATTR_PXI_CHASSIS_CLK10_SOURCE to this value.
        /// 
        /// Default Value: NIRFSG_VAL_NONE
        /// 
        /// Valid Timing Configurations:
        /// 
        /// PXI Chassis Clk10 Setting      Valid Ref Clock Setting  
        /// ------------------------------------------------------
        /// None, OnBoardClock             OnBoardClock 
        /// None, RefIn                    RefIn (external)
        /// None, RefIn                    PXI_CLK_10 
        /// 
        /// 
        /// Refer to Timing Configurations for more information on valid timing configurations.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigurePXIChassisClk10(string PXI_Clk_10_Source)
        {
            int pInvokeResult = PInvoke.ConfigurePXIChassisClk10(this._handle, PXI_Clk_10_Source);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies a string containing a script that controls waveform generation in script mode. The NI-RFSG device must be in the Configuration or Committed state before calling this function.
        /// 
        /// If this function is called repeatedly, previously written scripts with unique names remain loaded. Previously written scripts with identical names to those being written are replaced.
        /// 
        /// If there are multiple scripts loaded when niRFSG_Initiate is called, then one of the scripts must be designated as the script to generate by setting NIRFSG_ATTR_SCRIPT_TO_GENERATE to the desired script name.  If there is only one script in memory, then there is no need to designate the script to generate. 
        /// 
        /// An error is returned at commit time if the script uses incorrect syntax.
        /// </summary>
        ///
        /// <param name="Script">
        /// Specifies a string containing a syntactically correct script. NI-RFSG supports multiple scripts that may be selected by name with the Selected Script property. Refer to Scripting Instruction for more information on using scripts. 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int WriteScript(string Script)
        {
            int pInvokeResult = PInvoke.WriteScript(this._handle, Script);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Enables or disables signal output. This function can be called in any software state, and it does not change the current software state (the NI-RFSG device remains in the Configuration state, Committed state, or Signal Generation state). Setting enabled to VI_FALSE while in the Signal Generation state stops signal output although generation continues internally.
        /// </summary>
        ///
        /// <param name="Output_Enabled">
        /// NI-RFSG sets NIRFSG_ATTR_OUTPUT_ENABLED to this value. 
        /// 
        /// Default value:  VI_TRUE
        /// 
        /// Defined Values:
        /// VI_TRUE - enables signal output
        /// VI_FALSE - disables signal output
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ConfigureOutputEnabled(bool Output_Enabled)
        {
            Int16 outputEnabled = Convert.ToInt16(Output_Enabled);
            int pInvokeResult = PInvoke.ConfigureOutputEnabled(this._handle, outputEnabled);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates signal generation, causing the NI-RFSG device to leave the Configuration or Committed state and enter the Generation state. If the settings have not been committed to the device before you use this function, they are committed with this function. The operation returns when the RF output signal is settled. 
        /// 
        /// To return to the Configuration state, call niRFSG_Abort.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int Initiate()
        {
            int pInvokeResult = PInvoke.Initiate(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Checks the status of the generation. 
        /// 
        /// Use niRFSG_CheckGenerationStatus to check for any errors that might occur during the signal generation or to check whether the device is done generating. If generation is taking place and no errors have occurred, this function returns a successful status.
        /// 
        /// </summary>
        ///
        /// <param name="Is_Done">
        /// Returns true if waveform generation is done.
        /// 
        /// TODO: Need this documentation looked over!
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int CheckGenerationStatus(out bool Is_Done)
        {
            Int16 done;
            int pInvokeResult = PInvoke.CheckGenerationStatus(this._handle, out done);
            PInvoke.TestForError(this._handle, pInvokeResult);
            Is_Done = Convert.ToBoolean(done);
            return pInvokeResult;
        }

        /// <summary>
        /// Aborts a previously initiated signal generation.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int Abort()
        {
            int pInvokeResult = PInvoke.Abort(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Asserts the configured hardware parameters. Calling niRFSG_Commit moves the NI-RFSG device out of the Configuration state and into the Committed state. 
        /// 
        /// Once you call niRFSG_Commit, changing any attribute reverts the NI-RFSG device to the Configuration state.
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int Commit()
        {
            int pInvokeResult = PInvoke.Commit(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Corrects any drift in RF signal generator output power due to environmental temperature variation. To maintain specified power accuracy over extended periods, the environmental temperature must not vary more than approximately 1 0C for every 0.1 dB of accuracy constraint. In such cases, call niRFSG_PerformThermalCorrection as needed to maintain specified power accuracy.
        /// 
        /// The NI-RFSG device must be in the Generation state before calling this function.
        /// 
        /// Reading this attribute causes transient noise in the RF output signal. Refer to the thermal management section for more information.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int PerformThermalCorrection()
        {
            int pInvokeResult = PInvoke.PerformThermalCorrection(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Waits (up to the time specified in the maxTimeMilliseconds parameter) until the RF output signal is settled. Call this function after calling niRFSG_Commit for applications in which it is important to start signal generation immediately upon calling niRFSG_Initiate. 
        /// 
        /// 
        /// Call niRFSG_WaitUntilSettled after niRFSG_Commit and before niRFSG_Initiate. 
        /// 
        /// viCheckErr ( niRFSG_Commit(vi) );
        /// viCheckErr ( niRFSG_WaitUntilSettled(vi, 10000) );
        /// viCheckErr ( niRFSG_Initiate(vi) );
        /// </summary>
        ///
        /// <param name="Max_Time_Milliseconds">
        /// Defines the maximum time the function waits for the output to be settled. If the maximum time is exceeded, this function returns the Max Time Exceeded error. 
        /// 
        /// If a -1 is given as maxTimeMilliseconds, NI-RFSG waits indefinitely until it is settled. The units are expressed in milliseconds.
        /// 
        /// Default Value: 10000
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int WaitUntilSettled(int Max_Time_Milliseconds)
        {
            int pInvokeResult = PInvoke.WaitUntilSettled(this._handle, Max_Time_Milliseconds);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Resets all attributes to their default values and moves the NI-RFSG device to the Configuration state. niRFSG_reset aborts the generation, clears all routes, and resets session attributes to the initial values. During a reset, routes of signals between this and other devices are released, regardless of which device created the route. For example, a trigger signal exported to a PXI trigger line that is used by another device is no longer exported.
        /// 
        /// In general, calling niRFSG_reset instead of niRFSG_ResetDevice is recommended. niRFSG_reset executes faster than niRFSG_ResetDevice.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int reset()
        {
            int pInvokeResult = PInvoke.reset(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Performs a hard reset on the device. Generation is stopped, all routes are released, external bi-directional terminals are tri-stated, FPGAs are reset, hardware is configured to its default state, and all session attributes are reset to their default states. During a reset, routes of signals between this and other devices are released, regardless of which device created the route. For example, a trigger signal exported to a PXI trigger line that is used by another device is no longer exported.
        /// 
        /// In general, calling niRFSG_reset instead of niRFSG_ResetDevice is recommended. niRFSG_reset executes faster than niRFSG_ResetDevice. Allow 15 seconds after calling this function for the NI-RFSG device to become fully functional again.
        /// 
        /// You must call niRFSG_ResetDevice if the NI-RFSG device has shut down because of high temperature conditions.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int ResetDevice()
        {
            int pInvokeResult = PInvoke.ResetDevice(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Performs a self-test on the NI-RFSG device and returns the test results. niRFSG_self_test performs a simple series of tests ensuring the NI-RFSG device is powered up and responding.  These tests include an FPGA access test, RAM test, FPGA download test, and tests to ensure successful communication and locking of various oscillators on the device.
        /// 
        /// niRFSG_self_test does not affect external I/O connections or connections between devices.  Complete functional testing and calibration are not performed by this function. The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_self_test.
        /// </summary>
        ///
        /// <param name="Self_Test_Result">
        /// This parameter contains the value returned from the NI-RFSG device self test.  Zero means success.  For any other code, refer to NI-RFSG Error and Completion Codes.
        /// 
        /// Self-Test Code    Description
        /// ---------------------------------------
        ///    0              Passed self test
        ///    1              Self test failed
        /// </param>
        /// <param name="Self_Test_Message">
        /// Returns the self-test response string from the NI-RFSG device. Refer to the NI RF Signal Generator Help for an explanation of the string's contents.
        /// 
        /// You must pass a ViChar array with at least 256 bytes.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int self_test(out short Self_Test_Result, System.Text.StringBuilder Self_Test_Message)
        {
            int pInvokeResult = PInvoke.self_test(this._handle, out Self_Test_Result, Self_Test_Message);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Performs an internal (self-) calibration on the device. If the calibration is successful, new calibration data and constants are stored in the onboard nonvolatile memory of the module.
        /// </summary>
        ///
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int SelfCal()
        {
            int pInvokeResult = PInvoke.SelfCal(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the revision numbers of the NI-RFSG driver.
        /// </summary>
        ///
        /// <param name="Instrument_Driver_Revision">
        /// Returns the value of NIRFSG_ATTR_SPECIFIC_DRIVER_REVISION in the form of a string.
        /// 
        /// You must pass a ViChar array with at least 256 bytes.
        /// </param>
        /// <param name="Firmware_Revision">
        /// Returns the value of NIRFSG_ATTR_INSTRUMENT_FIRMWARE_REVISION in the form of a string.
        /// 
        /// You must pass a ViChar array with at least 256 bytes.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int revision_query(System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision)
        {
            int pInvokeResult = PInvoke.revision_query(this._handle, Instrument_Driver_Revision, Firmware_Revision);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Queries and returns the waveform capabilities of the NI-RFSG device. These capabilities are related to the current device configuration.
        /// 
        /// Use niRFSG_QueryArbWaveformCapabilities to query the attributes of the NI-RFSG device such as the maximum and minimum waveform size that you can write.
        /// 
        /// The NI-RFSG device must be in the Configuration or Committed state before calling niRFSG_QueryArbWaveformCapabilities.
        /// </summary>
        ///
        /// <param name="Max_Number_Waveforms">
        /// Returns the value of NIRFSG_ATTR_ARB_MAX_NUMBER_WAVEFORMS. This value is the maximum number of waveforms you can write.
        /// </param>
        /// <param name="Waveform_Quantum">
        /// Returns the value of NIRFSG_ATTR_ARB_WAVEFORM_QUANTUM. If the waveform quantum is q, then the size of the waveform that you write should be a multiple of q. The units are expressed in samples.
        /// </param>
        /// <param name="Min_Waveform_Size">
        /// Returns the value of NIRFSG_ATTR_ARB_WAVEFORM_SIZE_MIN. The number of samples of the waveform that you write must be greater than or equal to this value.
        /// </param>
        /// <param name="Max_Waveform_Size">
        /// Returns the value of NIRFSG_ATTR_ARB_WAVEFORM_SIZE_MAX. The number of samples of the waveform that you write must be less than or equal to this value.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the niRFSG_error_message function.  To obtain additional information about the error condition, call the niRFSG_GetError function.  To clear the error information from the driver, call the niRFSG_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int QueryArbWaveformCapabilities(out int Max_Number_Waveforms, out int Waveform_Quantum, out int Min_Waveform_Size, out int Max_Waveform_Size)
        {
            int pInvokeResult = PInvoke.QueryArbWaveformCapabilities(this._handle, out Max_Number_Waveforms, out Waveform_Quantum, out Min_Waveform_Size, out Max_Waveform_Size);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Allocates onboard memory space for the waveform. Use this function to specify the total size of a waveform before writing the data.  You only need to use this function if you are calling the niRFSG_WriteArbWaveform function multiple times to write a large waveform in blocks. The NI-RFSG device must be in the Configuration state before you call this function.
        /// Supported Devices: NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        /// </summary>
        ///<param name = "name">
        /// name
        /// ViConstString
        /// Specifies the name used to identify the waveform. This string is case-insensitive and alphanumeric, and it does not use reserved words.
        /// 
        ///</param>
        ///<param name = "sizeInSamples">
        /// size_in_samples
        /// ViInt32
        /// Specifies the number of samples to reserve in the onboard memory for the specified waveform.  Each I/Q pair is considered one sample.
        /// 
        ///</param>
        ///<returns>
        /// Allocates onboard memory space for the waveform. Use this function to specify the total size of a waveform before writing the data.  You only need to use this function if you are calling the niRFSG_WriteArbWaveform function multiple times to write a large waveform in blocks. The NI-RFSG device must be in the Configuration state before you call this function.
        /// Supported Devices: NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        ///</returns>
        public int AllocateArbWaveform(string name, int sizeInSamples)
        {
            int pInvokeResult = PInvoke.AllocateArbWaveform(this._handle, name, sizeInSamples);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Changes the external calibration password of the device.
        /// Supported Devices: NI PXIe-5611/5673/5673E
        /// 
        /// </summary>
        ///
        /// <param name = "oldPassword">
        /// Password
        /// ViConstString
        /// Specifies the old (current) external calibration password.  This password is case sensitive.
        /// 
        ///</param>
        /// <param name = "newPassword">
        /// newPassword
        /// ViConstString
        /// Specifies the new (desired) external calibration password.
        /// 
        ///</param>
        ///<returns>
        /// Changes the external calibration password of the device.
        /// Supported Devices: NI PXIe-5611/5673/5673E
        /// 
        ///</returns>
        public int ChangeExternalCalibrationPassword(string oldPassword, string newPassword)
        {
            int pInvokeResult = PInvoke.ChangeExternalCalibrationPassword(this._handle, oldPassword, newPassword);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }


        public int ConfigureDigitalLevelScriptTrigger(string triggerIdentifier, string source, int level)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalLevelScriptTrigger(this._handle, triggerIdentifier, source, level);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Specifies the message signal used for digital modulation when NIRFSG_ATTR_DIGITAL_MODULATION_WAVEFORM_TYPE is set to NIRFSG_VAL_USER_DEFINED.Supported Devices: NI PXI/PXIe-5650/5651/5652
        /// 
        /// </summary>
        ///
        /// <param name = "numberOfSamples">
        /// numberOfSamples
        /// ViInt32
        /// Specifies the number of samples in the message signal.
        /// 
        ///</param>
        /// <param name = "userDefinedWaveform">
        /// userDefinedWaveform
        /// ViInt8[]
        /// Specifies the user-defined message signal used for digital modulation.
        /// 
        ///</param>
        ///<returns>
        /// Specifies the message signal used for digital modulation when NIRFSG_ATTR_DIGITAL_MODULATION_WAVEFORM_TYPE is set to NIRFSG_VAL_USER_DEFINED.Supported Devices: NI PXI/PXIe-5650/5651/5652
        /// 
        ///</returns>
        public int ConfigureDigitalModulationUserDefinedWaveform(int numberOfSamples, sbyte[] userDefinedWaveform)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalModulationUserDefinedWaveform(this._handle, numberOfSamples, userDefinedWaveform);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Configures the NI-RFSG device to apply I/Q (vector) modulation to the RF output signal. I/Q modulation must be enabled in order to generate any arbitrary (non-sine) waveform; if I/Q modulation is disabled, a sine tone is always generated, regardless if an arbitrary waveform is written. The NI-RFSG device must be in the Configuration state before calling this function.
        /// Note&#160;&#160;This function is obsolete.  Use the  NIRFSG_ATTR_GENERATION_MODE attribute to enable I/Q modulation instead.
        /// Upon device initialization, or calling the niRFSG_reset function or the niRFSG_ResetDevice function, I/Q modulation is disabled.
        /// 
        /// </summary>
        ///
        /// <param name = "iQEnabled">
        /// enabled
        /// ViBoolean
        /// NI-RFSG sets the NIRFSG_ATTR_IQ_ENABLED attribute to this value.
        /// Defined Values:
        /// VI_TRUEEnables IQ (vector) modulation (arbitrary waveform generation)VI_FALSEDisables IQ (vector) modulation (sine wave generation)
        /// 
        ///</param>
        ///<returns>
        /// Configures the NI-RFSG device to apply I/Q (vector) modulation to the RF output signal. I/Q modulation must be enabled in order to generate any arbitrary (non-sine) waveform; if I/Q modulation is disabled, a sine tone is always generated, regardless if an arbitrary waveform is written. The NI-RFSG device must be in the Configuration state before calling this function.
        /// Note&#160;&#160;This function is obsolete.  Use the  NIRFSG_ATTR_GENERATION_MODE attribute to enable I/Q modulation instead.
        /// Upon device initialization, or calling the niRFSG_reset function or the niRFSG_ResetDevice function, I/Q modulation is disabled.
        /// 
        ///</returns>
        public int ConfigureIQEnabled(bool iQEnabled)
        {
            Int16 iQEnabledVal = Convert.ToInt16(iQEnabled);
            int pInvokeResult = PInvoke.ConfigureIQEnabled(this._handle, iQEnabledVal);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies the way the driver interprets the NIRFSG_ATTR_POWER_LEVEL attribute.  In average power mode, NI-RFSG automatically scales waveform data to use the maximum dynamic range.  In peak power mode, waveforms are scaled according to the NIRFSG_ATTR_SOFTWARE_SCALING_FACTOR attribute.
        /// Supported Devices&#58; NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        /// </summary>
        ///
        /// <param name = "powerLevelType">
        /// power_level_type
        /// ViInt32
        /// Specifies the way the driver interprets the value of the NIRFSG_ATTR_POWER_LEVEL attribute.
        /// 
        ///</param>
        ///<returns>
        /// Specifies the way the driver interprets the NIRFSG_ATTR_POWER_LEVEL attribute.  In average power mode, NI-RFSG automatically scales waveform data to use the maximum dynamic range.  In peak power mode, waveforms are scaled according to the NIRFSG_ATTR_SOFTWARE_SCALING_FACTOR attribute.
        /// Supported Devices&#58; NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        ///</returns>
        public int ConfigurePowerLevelType(int powerLevelType)
        {
            int pInvokeResult = PInvoke.ConfigurePowerLevelType(this._handle, powerLevelType);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }

        /// <summary>
        /// Creates an empty configuration list.  Once a configuration list is created, the list is enabled using the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute.  Call the niRFSG_CreateConfigurationListStep function to add steps to the configuration list.
        /// Supported Devices&#58; NI PXIe-5673E
        /// 
        /// </summary>
        ///
        /// <param name = "listName">
        /// listName
        /// ViConstString
        /// Specifies the name of the configuration list.  This string may not contain spaces.
        /// 
        ///</param>
        /// <param name = "numberOfAttributes">
        /// numberOfAttributes
        /// const ViInt32
        /// Specifies size of the configurationListAttributes parameter. 
        /// 
        ///</param>
        /// <param name = "configurationListAttributes">
        /// configurationListAttributes[]
        /// const ViAttr
        /// Specifies the attributes that the user intends to change between configuration list steps.  Calling the niRFSG_CreateConfigurationList function allocates space for each of the configuration list attributes.  When you use an NI-RFSG Set attribute function to set one of the attributes in the configuration list, that attribute is set for one of the configuration list steps.  Use the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute to specify which configuration list step to configure.
        /// The following attributes are valid values for this parameter's array elements:
        /// NIRFSG_ATTR_FREQUENCY
        /// NIRFSG_ATTR_POWER_LEVEL
        /// NIRFSG_ATTR_PHASE_OFFSET
        /// NIRFSG_ATTR_TIMER_EVENT_INTERVAL
        /// NIRFSG_ATTR_FREQUENCY_SETTLING
        /// 
        ///</param>
        /// <param name = "setAsActiveList">
        /// setAsActiveList
        /// ViBoolean
        /// Sets this list as the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute when this parameter is enabled.   NI recommends that you set this parameter to VI_TRUE when creating the list.
        /// 
        ///</param>
        ///<returns>
        /// Creates an empty configuration list.  Once a configuration list is created, the list is enabled using the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute.  Call the niRFSG_CreateConfigurationListStep function to add steps to the configuration list.
        /// Supported Devices&#58; NI PXIe-5673E
        /// 
        ///</returns>
        public int CreateConfigurationList(string listName, int numberOfAttributes, niRFSGProperties[] configurationListAttributes, bool setAsActiveList)
        {
            Int16 setAsActiveListVal = Convert.ToInt16(setAsActiveList);
            int pInvokeResult = PInvoke.CreateConfigurationList(this._handle, listName, numberOfAttributes, configurationListAttributes, setAsActiveListVal);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Creates a new configuration list step in the configuration list specified by the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute.  When you create a configuration list step, a new instance of each attribute specified by the configuration list attributes is created.  Configuration list attributes are specified when a configuration list is created.  The new instance of an attribute can be accessed with any Set attribute function using the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST and NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST_STEP attributes.
        /// Supported Devices&#58; NI PXIe-5673E
        /// 
        /// </summary>
        ///
        /// <param name = "setAsActiveStep">
        /// setAsActiveStep
        /// ViBoolean
        /// Sets this step as the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST_STEP attribute list specified by the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute.  NI recommends that you set this parameter to VI_TRUE when creating the list steps.
        /// 
        ///</param>
        ///<returns>
        /// Creates a new configuration list step in the configuration list specified by the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST attribute.  When you create a configuration list step, a new instance of each attribute specified by the configuration list attributes is created.  Configuration list attributes are specified when a configuration list is created.  The new instance of an attribute can be accessed with any Set attribute function using the NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST and NIRFSG_ATTR_ACTIVE_CONFIGURATION_LIST_STEP attributes.
        /// Supported Devices&#58; NI PXIe-5673E
        /// 
        ///</returns>
        public int CreateConfigurationListStep(bool setAsActiveStep)
        {
            Int16 setAsActiveStepVal = Convert.ToInt16(setAsActiveStep);
            int pInvokeResult = PInvoke.CreateConfigurationListStep(this._handle, setAsActiveStepVal);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Deletes a previously created configuration list and all the configuration list steps in the configuration list.  When a configuration list step is deleted, all the instances of the attributes associated with the configuration list step are also removed.
        /// Supported Devices&#58; NI PXIe-5673E
        /// 
        /// </summary>
        ///
        /// <param name = "listName">
        /// listName
        /// ViConstString
        /// Specifies the name of the configuration list.  This string may not contain spaces.
        /// 
        ///</param>
        ///<returns>
        /// Deletes a previously created configuration list and all the configuration list steps in the configuration list.  When a configuration list step is deleted, all the instances of the attributes associated with the configuration list step are also removed.
        /// Supported Devices&#58; NI PXIe-5673E
        /// 
        ///</returns>
        public int DeleteConfigurationList(string listName)
        {
            int pInvokeResult = PInvoke.DeleteConfigurationList(this._handle, listName);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Places the instrument in a quiescent state where it has minimal or no impact on the system to which it is connected.
        /// Supported Devices&#58; NI PXI/PXIe-5650/5651/5652, NI PXI-5610/5670/5671, NI PXIe-5611/5672/5673/5673E
        /// 
        /// </summary>
        ///<returns>
        /// Places the instrument in a quiescent state where it has minimal or no impact on the system to which it is connected.
        /// Supported Devices&#58; NI PXI/PXIe-5650/5651/5652, NI PXI-5610/5670/5671, NI PXIe-5611/5672/5673/5673E
        /// 
        ///</returns>
        public int Disable()
        {
            int pInvokeResult = PInvoke.Disable(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Reads an error code and an error message from the instrument error queue.
        /// Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        /// </summary>
        ///
        /// <param name = "errorCode">
        /// Error_Code
        /// ViInt32*
        /// Returns the error code read from the instrument error queue.
        /// 
        ///</param>
        /// <param name = "errorMessage">
        /// Error_Message
        /// ViChar[]
        /// Returns the error message string read from the instrument error message queue.
        /// You must pass a ViChar array with at least 256 bytes.
        /// 
        ///</param>
        ///<returns>
        /// Reads an error code and an error message from the instrument error queue.
        /// Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        ///</returns>
        public int error_query(out int errorCode, StringBuilder errorMessage)
        {
            int pInvokeResult = PInvoke.error_query(this._handle, out errorCode, errorMessage);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }


        /// <summary>
        /// Returns the channel string that is in the channel table at an index you specify.
        /// Supported Devices&#58; NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        /// </summary>
        ///
        /// <param name = "index">
        /// Index
        /// ViInt32
        /// Specifies a 1-based index into the channel table.
        /// 
        ///</param>
        /// <param name = "bufferSize">
        /// BufferSize
        /// ViInt32
        /// Specifies the size of the buffer for the channel string
        /// 
        ///</param>
        /// <param name = "channelName">
        /// Channel_Name
        /// ViChar[]
        /// Returns a channel string from the channel table at the index you specify in the Index parameter.
        /// Do not modify the contents of the channel string.
        /// 
        ///</param>
        ///<returns>
        /// Returns the channel string that is in the channel table at an index you specify.
        /// Supported Devices&#58; NI PXI-5670/5671, NI PXIe-5672/5673/5673E
        /// 
        ///</returns>
        public int GetChannelName(int index, int bufferSize, StringBuilder channelName)
        {
            int pInvokeResult = PInvoke.GetChannelName(this._handle, index, bufferSize, channelName);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the date and time of the last successful external calibration. The time returned is 24-hour (military) local time; for example, if the device was calibrated at 2:30 PM, this function returns 14 for the hours parameter and 30 for the minutes parameter. 
        /// Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5610/5670/5671, NI PXIe-5611/5672/5673/5673E
        /// 
        /// </summary>
        ///
        /// <param name = "year">
        /// year
        /// ViInt32*
        /// Specifies the year of the last successful calibration.
        /// 
        ///</param>
        /// <param name = "month">
        /// month
        /// ViInt32*
        /// Specifies the month of the last successful calibration.
        /// 
        ///</param>
        /// <param name = "day">
        /// day
        /// ViInt32*
        /// Specifies the day of the last successful calibration.
        /// 
        ///</param>
        /// <param name = "hour">
        /// hour
        /// ViInt32*
        /// Specifies the hour of the last successful calibration.
        /// 
        ///</param>
        /// <param name = "minute">
        /// minute
        /// ViInt32*
        /// Specifies the minute of the last successful calibration.
        /// 
        ///</param>
        /// <param name = "second">
        /// minute
        /// ViInt32*
        /// Specifies the minute of the last successful calibration.
        /// 
        ///</param>
        ///<returns>
        /// Returns the date and time of the last successful external calibration. The time returned is 24-hour (military) local time; for example, if the device was calibrated at 2:30 PM, this function returns 14 for the hours parameter and 30 for the minutes parameter. 
        /// Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5610/5670/5671, NI PXIe-5611/5672/5673/5673E
        /// 
        ///</returns>
        public int GetExternalCalibrationLastDateAndTime(out int year, out int month, out int day, out int hour, out int minute, out int second)
        {
            int pInvokeResult = PInvoke.GetExternalCalibrationLastDateAndTime(this._handle, out year, out month, out day, out hour, out minute, out second);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Resets the attribute to its default value.
        /// Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5610/5670/5671, NI PXIe-5611/5672/5673/5673E
        /// 
        /// </summary>
        /// <param name = "channelName">
        /// channelName
        /// ViConstString
        /// Specifies the name of the channel on which to reset the attribute value if the attribute is channel based. If the attribute is not channel based, set this parameter to &quot;&quot; (empty string) or VI_NULL.
        /// 
        ///</param>
        /// <param name = "attributeID">
        /// attributeID
        /// ViAttr
        /// Passes the ID of an attribute.
        /// From the function panel window, you can use this control as follows:
        /// Click on the control or press &lt;Enter&gt;, &lt;Spacebar&gt;, or
        /// &lt;Ctrl-Down Arrow&gt;, to display a dialog box containing a
        /// hierarchical list of the available attributes. Any attribute
        /// whose value cannot be reset is dim. Help text is shown for 
        /// each attribute. Select an attribute by double-clicking on it 
        /// or by selecting it and then pressing &lt;Enter&gt;.
        /// Read-only attributes appear dim in the list box. If you 
        /// select a read-only attribute, an error message appears.
        /// If the attribute in this ring control has named constants as 
        /// defined values, you can view the constants by moving to the 
        /// Attribute Value control and pressing &lt;Enter&gt;.
        /// 
        ///</param>
        ///<returns>
        /// Resets the attribute to its default value.
        /// Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5610/5670/5671, NI PXIe-5611/5672/5673/5673E
        /// 
        ///</returns>
        public int ResetAttribute(string channelName, int attributeID)
        {
            int pInvokeResult = PInvoke.ResetAttribute(this._handle, channelName, attributeID);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }

        /// <summary>
        /// Performs a software reset of the device, returning it to the default state and applying any initial default settings from the IVI Configuration Store.
        /// <p class="body">Supported Devices: NI PXI/PXIe-5650/5651/5652, NI PXI-5670/5671, NI PXIe-5672/5673/5673E</p>
        /// 
        /// </summary>
        ///
        ///<returns>
        /// Performs a software reset of the device, returning it to the default state and applying any initial default settings from the IVI Configuration Store.
        /// 
        /// 
        ///</returns>
        public int ResetWithDefaults()
        {
            int pInvokeResult = PInvoke.ResetWithDefaults(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }

        /// <summary>
        /// Writes an arbitrary waveform to the NI-RFSG device starting at the position of the last data written in onboard memory. This function takes as data input the data array of a complex baseband signal. If the waveform is already allocated, the  moreDataPending parameter is ignored.  Refer to the niRFSG_AllocateArbWaveform function for more information about allocating waveforms.  
        /// The NI-RFSG device must be in the Configuration state before calling this function.
        /// Supported Devices&#58; NI PXI-5670/5671, NI PXIe-5672/5673/5673E 
        /// 
        /// </summary>
        ///
        /// <param name = "name">
        /// name
        /// ViConstString
        /// Specifies the name used to identify the waveform. This string is case-insensitive and alphanumeric, and it does not use reserved words.
        /// 
        ///</param>
        /// <param name = "numberOfSamples">
        /// numberOfSamples
        /// ViInt32
        /// Specifies the number of samples in both of the data arrays.
        /// 
        ///</param>
        /// <param name = "data">
        /// data
        /// niComplexNumber[]
        /// Specifies the array of data to load into the waveform. The array must have at least as many elements as the value in the size_in_samples parameter in the niRFSG_AllocateArbWaveform function.
        /// 
        ///</param>
        /// <param name = "moreDataPending">
        /// moreDataPending
        /// ViBoolean
        /// Specifies whether or not the data block contains the end of the waveform.  Set this parameter to VI_TRUE to allow data to be appended later to the waveform. Splitting the waveform into multiple data blocks can reduce the memory requirements of the write operation. Append data to a previously written waveform by using the same waveform in the name parameter. Set moreDataPending to VI_FALSE to indicate that this data block contains the end of the waveform. If the waveform is already allocated, this parameter is ignored.
        /// 
        ///</param>
        ///<returns>
        /// Writes an arbitrary waveform to the NI-RFSG device starting at the position of the last data written in onboard memory. This function takes as data input the data array of a complex baseband signal. If the waveform is already allocated, the  moreDataPending parameter is ignored.  Refer to the niRFSG_AllocateArbWaveform function for more information about allocating waveforms.  
        /// The NI-RFSG device must be in the Configuration state before calling this function.
        /// Supported Devices&#58; NI PXI-5670/5671, NI PXIe-5672/5673/5673E 
        /// 
        ///</returns>
        public int WriteArbWaveformComplexF64(string name, int numberOfSamples, niComplexNumber[] data, bool moreDataPending)
        {
            Int16 moreDataPendingVal = Convert.ToInt16(moreDataPending);
            int pInvokeResult = PInvoke.WriteArbWaveformComplexF64(this._handle, name, numberOfSamples, data, moreDataPendingVal);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
        }
        /// <summary>
        /// Writes an arbitrary waveform to the NI-RFSG device starting at the position of the last data written in onboard memory. This function takes as data input the data array of a complex baseband signal. If the waveform is already allocated, the  moreDataPending parameter is ignored.  Refer to the niRFSG_AllocateArbWaveform function for more information about allocating waveforms. 
        /// The NI-RFSG device must be in the Configuration state before calling this function.
        /// Note&#160;&#160;This function only supports NIRFSG_VAL_PEAK_POWER mode as specified in the NIRFSG_ATTR_POWER_LEVEL_TYPE attribute.  If a waveform is downloaded using this function, NIRFSG_ATTR_POWER_LEVEL_TYPE cannot be changed to NIRFSG_VAL_AVERAGE_POWER mode without causing error in the output.
        /// Supported Devices&#58; NI PXIe-5672/5673/5673E 
        /// 
        /// </summary>
        ///
        /// <param name = "name">
        /// name
        /// ViConstString
        /// Specifies the name used to identify the waveform. This string is case-insensitive and alphanumeric, and it does not use reserved words.
        /// 
        ///</param>
        /// <param name = "numberOfSamples">
        /// numberOfSamples
        /// ViInt32
        /// Specifies the number of samples in the data array.
        /// 
        ///</param>
        /// <param name = "data">
        /// data
        /// niComplexNumber[]
        /// Specifies the array of data to load into the waveform. The array must have at least as many elements as the value in the size_in_samples parameter in the niRFSG_AllocateArbWaveform function.
        /// 
        ///</param>
        ///<returns>
        /// Writes an arbitrary waveform to the NI-RFSG device starting at the position of the last data written in onboard memory. This function takes as data input the data array of a complex baseband signal. If the waveform is already allocated, the  moreDataPending parameter is ignored.  Refer to the niRFSG_AllocateArbWaveform function for more information about allocating waveforms. 
        /// The NI-RFSG device must be in the Configuration state before calling this function.
        /// Note&#160;&#160;This function only supports NIRFSG_VAL_PEAK_POWER mode as specified in the NIRFSG_ATTR_POWER_LEVEL_TYPE attribute.  If a waveform is downloaded using this function, NIRFSG_ATTR_POWER_LEVEL_TYPE cannot be changed to NIRFSG_VAL_AVERAGE_POWER mode without causing error in the output.
        /// Supported Devices&#58; NI PXIe-5672/5673/5673E 
        /// 
        ///</returns>
        public int WriteArbWaveformComplexI16(string name, int numberOfSamples, RfsgNIComplexI16[] data)
        {
            int pInvokeResult = PInvoke.WriteArbWaveformComplexI16(this._handle, name, numberOfSamples, data);
            PInvoke.TestForError(this._handle, pInvokeResult); ;
            return pInvokeResult;
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
                PInvoke.close(this._handle);
                this._handle = new System.Runtime.InteropServices.HandleRef(null, System.IntPtr.Zero);
            }
            this._disposed = true;
        }

        public void SetInt32(niRFSGProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetInt32(niRFSGProperties propertyId, int val)
        {
            this.SetInt32(propertyId, "", val);
        }

        public int GetInt32(niRFSGProperties propertyId, string repeatedCapabilityOrChannel)
        {
            int val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }

        public int GetInt32(niRFSGProperties propertyId)
        {
            return this.GetInt32(propertyId, "");
        }

        public void SetDouble(niRFSGProperties propertyId, string repeatedCapabilityOrChannel, System.Double val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetDouble(niRFSGProperties propertyId, System.Double val)
        {
            this.SetDouble(propertyId, "", val);
        }

        public System.Double GetDouble(niRFSGProperties propertyId, string repeatedCapabilityOrChannel)
        {
            System.Double val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }

        public System.Double GetDouble(niRFSGProperties propertyId)
        {
            return this.GetDouble(propertyId, "");
        }

        public void SetBoolean(niRFSGProperties propertyId, string repeatedCapabilityOrChannel, bool val)
        {
            Int16 boolVal = Convert.ToInt16(val);
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), boolVal));
        }

        public void SetBoolean(niRFSGProperties propertyId, bool val)
        {
            this.SetBoolean(propertyId, "", val);
        }

        public bool GetBoolean(niRFSGProperties propertyId, string repeatedCapabilityOrChannel)
        {
            Int16 val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return (val == 1);
        }

        public bool GetBoolean(niRFSGProperties propertyId)
        {
            return this.GetBoolean(propertyId, "");
        }

        public void SetString(niRFSGProperties propertyId, string repeatedCapabilityOrChannel, string val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetString(niRFSGProperties propertyId, string val)
        {
            this.SetString(propertyId, "", val);
        }

        public string GetString(niRFSGProperties propertyId, string repeatedCapabilityOrChannel)
        {
            System.Text.StringBuilder newVal = new System.Text.StringBuilder(512);
            int size = PInvoke.GetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), 512, newVal);
            if ((size < 0))
            {
                PInvoke.ThrowError(this._handle, size);
            }
            else
            {
                if ((size > 0))
                {
                    newVal.Capacity = size;
                    PInvoke.TestForError(this._handle, PInvoke.GetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), size, newVal));
                }
            }
            return newVal.ToString();
        }

        public string GetString(niRFSGProperties propertyId)
        {
            return this.GetString(propertyId, "");
        }

        /// <summary>
        /// Retrieves and then clears the IVI error information for the session or the current execution thread.
        /// 
        /// </summary>
        ///<param>
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param>
        /// Specifies the error code.
        /// 
        ///</param>
        ///<param>
        /// Specifies the error message returned.
        /// 
        ///</param>
        ///<returns>
        /// Retrieves and then clears the IVI error information for the session or the current execution thread.
        /// 
        ///</returns>
        public static int GetError(HandleRef handle, int code, StringBuilder msg)
        {
            int pInvokeResult = 0;
            int size = PInvoke.GetError(handle, out code, 0, null);
            if ((size >= 0))
            {
                msg.Capacity = size;
                pInvokeResult = PInvoke.GetError(handle, out code, size, msg);
            }
            PInvoke.TestForError(handle, pInvokeResult);
            return pInvokeResult;
        }
        /// <summary>
        /// Converts a status code returned by an NI-RFSG function into a user-readable string.
        /// 
        /// </summary>
        ///<param>
        /// Identifies your instrument session. vi is obtained from the niRFSG_init or niRFSG_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param>
        /// Passes the Status parameter that is returned from any NI-RFSG function. The default value is 0 (VI_SUCCESS).
        /// 
        ///</param>
        ///<param>
        /// Returns the user-readable message string that corresponds to the status code you specify.
        /// 
        ///</param>
        ///<returns>
        /// Converts a status code returned by an NI-RFSG function into a user-readable string.
        /// 
        ///</returns>:
        public static int ErrorMessage(HandleRef handle, int code, StringBuilder msg)
        {
            msg.Capacity = maxErrorMessageSize;
            int pInvokeResult = PInvoke.error_message(handle, code, msg);
            PInvoke.TestForError(handle, pInvokeResult);
            return pInvokeResult;
        }

        private class PInvoke
        {
            const string nativeDllName = "niRFSG.dll";

            [DllImport(nativeDllName, EntryPoint = "niRFSG_Abort", CallingConvention = CallingConvention.StdCall)]
            public static extern int Abort(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_AllocateArbWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int AllocateArbWaveform(System.Runtime.InteropServices.HandleRef instrumentHandle, string name, int sizeInSamples);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ChangeExternalCalibrationPassword", CallingConvention = CallingConvention.StdCall)]
            public static extern int ChangeExternalCalibrationPassword(System.Runtime.InteropServices.HandleRef instrumentHandle, string oldPassword, string newPassword);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_CheckGenerationStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int CheckGenerationStatus(System.Runtime.InteropServices.HandleRef instrumentHandle, out Int16 isDone);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ClearAllArbWaveforms", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearAllArbWaveforms(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ClearArbWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearArbWaveform(System.Runtime.InteropServices.HandleRef instrumentHandle, string name);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_Commit", CallingConvention = CallingConvention.StdCall)]
            public static extern int Commit(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureDigitalEdgeScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeScriptTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle, string triggerIdentifier, string source, int edge);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureDigitalEdgeStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeStartTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle, string source, int edge);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureDigitalLevelScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalLevelScriptTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle, string triggerIdentifier, string source, int level);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureDigitalModulationUserDefinedWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalModulationUserDefinedWaveform(System.Runtime.InteropServices.HandleRef instrumentHandle, int numberOfSamples, sbyte[] userDefinedWaveform);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureGenerationMode", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureGenerationMode(System.Runtime.InteropServices.HandleRef instrumentHandle, int generationMode);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureIQEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureIQEnabled(System.Runtime.InteropServices.HandleRef instrumentHandle, Int16 iQEnabled);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureOutputEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureOutputEnabled(System.Runtime.InteropServices.HandleRef instrumentHandle, Int16 outputEnabled);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigurePowerLevelType", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePowerLevelType(System.Runtime.InteropServices.HandleRef instrumentHandle, int powerLevelType);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigurePXIChassisClk10", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePXIChassisClk10(System.Runtime.InteropServices.HandleRef instrumentHandle, string pXIClk10Source);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureRefClock", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRefClock(System.Runtime.InteropServices.HandleRef instrumentHandle, string refClockSource, double refClockRate);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureRF", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRF(System.Runtime.InteropServices.HandleRef instrumentHandle, double frequency, double powerLevel);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureSignalBandwidth", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSignalBandwidth(System.Runtime.InteropServices.HandleRef instrumentHandle, double signalBandwidth);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureSoftwareScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareScriptTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle, string triggerIdentifier);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ConfigureSoftwareStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareStartTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_CreateConfigurationList", CallingConvention = CallingConvention.StdCall)]
            public static extern int CreateConfigurationList(System.Runtime.InteropServices.HandleRef instrumentHandle, string listName, int numberOfAttributes, niRFSGProperties[] configurationListAttributes, Int16 setAsActiveList);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_CreateConfigurationListStep", CallingConvention = CallingConvention.StdCall)]
            public static extern int CreateConfigurationListStep(System.Runtime.InteropServices.HandleRef instrumentHandle, Int16 setAsActiveStep);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_DeleteConfigurationList", CallingConvention = CallingConvention.StdCall)]
            public static extern int DeleteConfigurationList(System.Runtime.InteropServices.HandleRef instrumentHandle, string listName);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_Disable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Disable(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_DisableScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableScriptTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle, string triggerIdentifier);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_DisableStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableStartTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message(System.Runtime.InteropServices.HandleRef instrumentHandle, int errorCode, StringBuilder errorMessage);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query(System.Runtime.InteropServices.HandleRef instrumentHandle, out int errorCode, StringBuilder errorMessage);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ExportSignal", CallingConvention = CallingConvention.StdCall)]
            public static extern int ExportSignal(System.Runtime.InteropServices.HandleRef instrumentHandle, int signal, string signalIdentifier, string outputTerminal);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, out Int16 attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, out int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetAttributeViInt64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt64(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, out long attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, out double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, out System.IntPtr attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, int bufferSize, StringBuilder attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetChannelName", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetChannelName(System.Runtime.InteropServices.HandleRef instrumentHandle, int index, int bufferSize, StringBuilder channelName);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError(System.Runtime.InteropServices.HandleRef instrumentHandle, out int errorCode, int bufferSize, StringBuilder description);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_GetExternalCalibrationLastDateAndTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetExternalCalibrationLastDateAndTime(System.Runtime.InteropServices.HandleRef instrumentHandle, out int year, out int month, out int day, out int hour, out int minute, out int second);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init(string resourceName, Int16 iDQuery, Int16 reset, out System.IntPtr instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_Initiate", CallingConvention = CallingConvention.StdCall)]
            public static extern int Initiate(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions(string resourceName, Int16 iDQuery, Int16 reset, string optionString, out System.IntPtr instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_PerformThermalCorrection", CallingConvention = CallingConvention.StdCall)]
            public static extern int PerformThermalCorrection(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_QueryArbWaveformCapabilities", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryArbWaveformCapabilities(System.Runtime.InteropServices.HandleRef instrumentHandle, out int maxNumberWaveforms, out int waveformQuantum, out int minWaveformSize, out int maxWaveformSize);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetAttribute(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ResetDevice", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetDevice(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_ResetWithDefaults", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetWithDefaults(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query(System.Runtime.InteropServices.HandleRef instrumentHandle, StringBuilder instrumentDriverRevision, StringBuilder firmwareRevision);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SelectArbWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int SelectArbWaveform(System.Runtime.InteropServices.HandleRef instrumentHandle, string name);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test(System.Runtime.InteropServices.HandleRef instrumentHandle, out short selfTestResult, StringBuilder selfTestMessage);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SelfCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int SelfCal(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SendSoftwareEdgeTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int SendSoftwareEdgeTrigger(System.Runtime.InteropServices.HandleRef instrumentHandle, int trigger, string triggerIdentifier);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, Int16 attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, int attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SetAttributeViInt64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt64(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, long attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, double attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, System.Runtime.InteropServices.HandleRef attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelName, int attributeID, string attributeValue);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_WaitUntilSettled", CallingConvention = CallingConvention.StdCall)]
            public static extern int WaitUntilSettled(System.Runtime.InteropServices.HandleRef instrumentHandle, int maxTimeMilliseconds);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_WriteArbWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteArbWaveform(System.Runtime.InteropServices.HandleRef instrumentHandle, string name, int numberOfSamples, double[] iData, double[] qData, Int16 moreDataPending);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_WriteArbWaveformComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteArbWaveformComplexF64(System.Runtime.InteropServices.HandleRef instrumentHandle, string name, int numberOfSamples, niComplexNumber[] data, Int16 moreDataPending);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_WriteArbWaveformComplexI16", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteArbWaveformComplexI16(System.Runtime.InteropServices.HandleRef instrumentHandle, string name, int numberOfSamples, RfsgNIComplexI16[] data);

            [DllImport(nativeDllName, EntryPoint = "niRFSG_WriteScript", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteScript(System.Runtime.InteropServices.HandleRef instrumentHandle, string script);


            public static int TestForError(System.Runtime.InteropServices.HandleRef handle, int status)
            {
                if ((status < 0))
                {
                    PInvoke.ThrowError(handle, status);
                }
                return status;
            }


            public static int ThrowError(System.Runtime.InteropServices.HandleRef handle, int code)
            {
                System.Text.StringBuilder msg = new System.Text.StringBuilder(maxErrorMessageSize);
                PInvoke.GetError(handle, out code, maxErrorMessageSize, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            }
        }
    }

    public class niRFSGConstants
    {
        public const int Enable = 1;

        [ObsoleteAttribute]
        public const string Rtsi7Str = "RTSI7";

        public const string NoneStr = "None";

        [ObsoleteAttribute]
        public const string Rtsi0Str = "RTSI0";

        public const string Pfi1Str = "PFI1";

        public const string PxiTrig3Str = "PXI_Trig3";

        public const int ArbWaveform = 1001;

        public const int MarkerEvent = 2;

        [ObsoleteAttribute]
        public const string Rtsi4Str = "RTSI4";

        public const int RisingEdge = 0;

        public const string Pfi5Str = "PFI5";

        public const int Low = 0;

        public const string PxiTrig2Str = "PXI_Trig2";

        public const string RefInStr = "RefIn";

        public const string PxiTrig7Str = "PXI_Trig7";

        public const int Software = 2;

        public const string PxiTrig6Str = "PXI_Trig6";

        [ObsoleteAttribute]
        public const string Rtsi1Str = "RTSI1";

        public const string Pfi2Str = "PFI2";

        public const string Pfi4Str = "PFI4";

        [ObsoleteAttribute]
        public const string Rtsi5Str = "RTSI5";

        public const int Medium = 1;

        public const Int16 ViTrue = 1;

        public const string Pfi3Str = "PFI3";

        public const int DigitalEdge = 1;

        [ObsoleteAttribute]
        public const string MarkerEvent3 = "marker3";

        [ObsoleteAttribute]
        public const string MarkerEvent2 = "marker2";

        [ObsoleteAttribute]
        public const string MarkerEvent1 = "marker1";

        [ObsoleteAttribute]
        public const string MarkerEvent0 = "marker0";

        public const string PxiTrig1Str = "PXI_Trig1";

        public const string DoNotExportStr = "";

        [ObsoleteAttribute]
        public const string Rtsi2Str = "RTSI2";

        public const int Auto = -1;

        public const int Disable = 0;

        public const int Script = 1002;

        public const int ScriptTrigger = 1;

        public const int Cw = 1000;

        [ObsoleteAttribute]
        public const string Rtsi6Str = "RTSI6";

        public const int StartTrigger = 0;

        public const string PxiTrig5Str = "PXI_Trig5";

        public const int None = 0;

        [ObsoleteAttribute]
        public const string ScriptTrigger2 = "scriptTrigger2";

        [ObsoleteAttribute]
        public const string ScriptTrigger3 = "scriptTrigger3";

        [ObsoleteAttribute]
        public const string ScriptTrigger0 = "scriptTrigger0";

        [ObsoleteAttribute]
        public const string ScriptTrigger1 = "scriptTrigger1";

        [ObsoleteAttribute]
        public const string OnBoardClockStr = "OnBoardClock";

        public const string onboardClockStr = "OnboardClock";

        public const string PxiTrig0Str = "PXI_Trig0";

        [ObsoleteAttribute]
        public const string OnboardClockStr = "OnBoardClock";

        public const string Pfi0Str = "PFI0";

        public const int FallingEdge = 1;

        [ObsoleteAttribute]
        public const string PxiClk10Str = "PXI_CLK10";

        public const int High = 2;

        public const string PxiTrig4Str = "PXI_Trig4";

        public const string PxiStarStr = "PXI_STAR";

        [ObsoleteAttribute]
        public const string Rtsi3Str = "RTSI3";

        public const Int16 ViFalse = 0;

        public const int ArbFilterTypeNone = 10000;

        public const int ArbFilterTypeRootRaisedCosine = 10001;

        public const int ArbFilterTypeRaisedCosine = 10002;

        public const int TimeAfterLock = 12000;

        public const int TimeAfterIo = 12001;

        public const int Ppm = 12002;

        public const int Fm = 2000;

        public const int Pm = 2001;

        public const int Sine = 3000;

        public const int Square = 3001;

        public const int Triangle = 3002;

        public const int Fsk = 4000;

        public const int Ook = 4001;

        public const int Psk = 4002;

        public const int Prbs = 5000;

        public const int UserDefined = 5001;

        public const int HighResolution = 6000;

        public const int DivideDown = 6001;

        public const int AveragePower = 7000;

        public const int PeakPower = 7001;

        public const int DigitalLevel = 8000;

        public const int ActiveHigh = 9000;

        public const int ActiveLow = 9001;

        public const string ClkInStr = "ClkIn";

        public const string ClkOutStr = "ClkOut";

        public const string RefOutStr = "RefOut";

        public const string RefOut2Str = "RefOut2";

        public const string PxiClkStr = "PXI_CLK";
    }

    public enum niRFSGProperties
    {
        /// <summary>
        /// System.Double
        /// </summary>
        Frequency = 1250001,

        /// <summary>
        /// System.Double
        /// </summary>
        PowerLevel = 1250002,

        /// <summary>
        /// System.Double
        /// </summary>
        PeakEnvelopePower = 1150011,

        /// <summary>
        /// System.Boolean
        /// </summary>
        LocalOscillatorOut0Enabled = 1150013,

        /// <summary>
        /// System.Boolean
        /// </summary>
        OutputEnabled = 1250004,

        /// <summary>
        /// System.Double
        /// </summary>
        FrequencyTolerance = 1150006,

        /// <summary>
        /// System.Boolean
        /// </summary>
        AttenuatorHoldEnabled = 1150009,

        /// <summary>
        /// System.Double
        /// </summary>
        AttenuatorHoldMaxPower = 1150010,

        /// <summary>
        /// System.Double
        /// </summary>
        DucPreFilterGain = 1150025,

        /// <summary>
        /// System.Double
        /// </summary>
        IfCarrierFrequency = 1150015,

        /// <summary>
        /// System.Double
        /// </summary>
        IfPower = 1150016,

        /// <summary>
        /// System.Double
        /// </summary>
        UpconverterCenterFrequency = 1154098,

        /// <summary>
        /// System.Double
        /// </summary>
        UpconverterGain = 1154097,

        /// <summary>
        /// System.Double
        /// </summary>
        UpconverterTemperature = 1150017,

        /// <summary>
        /// System.Int32
        /// </summary>
        UpconverterLoopBandwidth = 1150027,

        /// <summary>
        /// System.Int32
        /// </summary>
        AllowOutOfSpecificationUserSettings = 1150014,

        /// <summary>
        /// System.Int32
        /// </summary>
        GenerationMode = 1150018,

        /// <summary>
        /// System.Double
        /// </summary>
        SignalBandwidth = 1150007,

        /// <summary>
        /// System.String
        /// </summary>
        ArbSelectedWaveform = 1250451,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbMaxNumberWaveforms = 1250454,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbWaveformSizeMin = 1250456,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbWaveformSizeMax = 1250457,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbWaveformQuantum = 1250455,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalIfEqualizationEnabled = 1150012,

        /// <summary>
        /// System.Double
        /// </summary>
        IqRate = 1250452,

        /// <summary>
        /// System.Int32
        /// </summary>
        PhaseContinuityEnabled = 1150005,

        /// <summary>
        /// System.Boolean
        /// </summary>
        IqSwapEnabled = 1250404,

        /// <summary>
        /// System.String
        /// </summary>
        RefClockSource = 1150001,

        /// <summary>
        /// System.Double
        /// </summary>
        RefClockRate = 1250322,

        /// <summary>
        /// System.String
        /// </summary>
        PxiChassisClk10Source = 1150004,

        /// <summary>
        /// System.Int32
        /// </summary>
        StartTriggerType = 1250458,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeStartTriggerSource = 1150002,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeStartTriggerEdge = 1250459,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedStartTriggerOutputTerminal = 1150003,

        /// <summary>
        /// System.Int32
        /// </summary>
        ScriptTriggerType = 1150019,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeScriptTriggerSource = 1150020,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeScriptTriggerEdge = 1150021,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedScriptTriggerOutputTerminal = 1150022,

        /// <summary>
        /// System.String
        /// </summary>
        SelectedScript = 1150023,

        /// <summary>
        /// System.String
        /// </summary>
        SerialNumber = 1150026,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RangeCheck = 1050002,

        /// <summary>
        /// System.Boolean
        /// </summary>
        QueryInstrumentStatus = 1050003,

        /// <summary>
        /// System.Boolean
        /// </summary>
        Cache = 1050004,

        /// <summary>
        /// System.Boolean
        /// </summary>
        Simulate = 1050005,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RecordCoercions = 1050006,

        /// <summary>
        /// System.Boolean
        /// </summary>
        InterchangeCheck = 1050021,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverDescription = 1050514,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverPrefix = 1050302,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverVendor = 1050513,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverRevision = 1050551,

        /// <summary>
        /// System.Int32
        /// </summary>
        SpecificDriverClassSpecMajorVersion = 1050515,

        /// <summary>
        /// System.Int32
        /// </summary>
        SpecificDriverClassSpecMinorVersion = 1050516,

        /// <summary>
        /// System.String
        /// </summary>
        SupportedInstrumentModels = 1050327,

        /// <summary>
        /// System.String
        /// </summary>
        GroupCapabilities = 1050401,

        /// <summary>
        /// System.String
        /// </summary>
        InstrumentManufacturer = 1050511,

        /// <summary>
        /// System.String
        /// </summary>
        InstrumentModel = 1050512,

        /// <summary>
        /// System.String
        /// </summary>
        InstrumentFirmwareRevision = 1050510,

        /// <summary>
        /// System.String
        /// </summary>
        LogicalName = 1050305,

        /// <summary>
        /// System.String
        /// </summary>
        IoResourceDescriptor = 1050304,

        /// <summary>
        /// System.String
        /// </summary>
        DriverSetup = 1050007,

        /// <summary>
        /// double
        /// </summary>
        PhaseOffset = 1150024,

        /// <summary>
        /// System.Int32
        /// </summary>	
        ArbOnboardSampleClockMode = 1150029,

        /// <summary>
        /// string
        /// </summary>	
        ArbSampleClockSource = 1150030,

        /// <summary>
        /// double
        /// </summary>	
        ArbSampleClockRate = 1150031,

        /// <summary>
        /// RfsgAnalogModulationType
        /// </summary>	
        AnalogModulationType = 1150032,

        /// <summary>
        /// RfsgAnalogModulationWaveformType
        /// </summary>	
        AnalogModulationWaveformType = 1150033,

        /// <summary>
        /// double
        /// </summary>	
        AnalogModulationWaveformFrequency = 1150034,

        /// <summary>
        /// double
        /// </summary>	
        AnalogModulationFmDeviation = 1150035,

        /// <summary>
        /// RfsgDigitalModulationType
        /// </summary>	
        DigitalModulationType = 1150036,

        /// <summary>
        /// double
        /// </summary>	
        DigitalModulationSymbolRate = 1150037,

        /// <summary>
        /// RfsgDigitalModulationWaveformType
        /// </summary>	
        DigitalModulationWaveformType = 1150038,

        /// <summary>
        /// int
        /// </summary>	
        DigitalModulationPrbsOrder = 1150039,

        /// <summary>
        /// int
        /// </summary>	
        DigitalModulationPrbsSeed = 1150040,

        /// <summary>
        /// double
        /// </summary>	
        DigitalModulationFskDeviation = 1150041,

        /// <summary>
        /// RfsgDigitalEqualizationEnabled
        /// </summary>	
        DirectDownload = 1150042,

        /// <summary>
        /// RfsgPowerLevelType
        /// </summary>	
        PowerLevelType = 1150043,

        /// <summary>
        /// bool
        /// </summary>	
        DigitalPattern = 1150044,

        /// <summary>
        /// bool
        /// </summary>	
        StreamingEnabled = 1150045,

        /// <summary>
        /// string
        /// </summary>	
        StreamingWaveformName = 1150046,

        /// <summary>
        /// long
        /// </summary>	
        StreamingSpaceAvailableInWaveform = 1150047,

        /// <summary>
        /// int
        /// </summary>	
        DataTransferBlockSize = 1150048,

        /// <summary>
        /// bool
        /// </summary>	
        DirectDmaEnabled = 1150049,

        /// <summary>
        /// int
        /// </summary>	
        DirectDmaWindowAddress = 1150050,

        /// <summary>
        /// int
        /// </summary>	
        DirectDmaWindowSize = 1150051,

        /// <summary>
        /// double
        /// </summary>	
        ArbWaveformSoftwareScalingFactor = 1150052,

        /// <summary>
        /// string
        /// </summary>	
        ExportedRefClockOutputTerminal = 1150053,

        /// <summary>
        /// string
        /// </summary>	
        DigitalLevelScriptTriggerSource = 1150054,

        /// <summary>
        /// RfsgDigitalLevelScriptTriggerActiveLevel
        /// </summary>	
        DigitalLevelScriptTriggerActiveLevel = 1150055,

        /// <summary>
        /// RfsgArbFilterType
        /// </summary>	
        ArbFilterType = 1150056,

        /// <summary>
        /// double
        /// </summary>	
        ArbFilterRootRaisedCosineAlpha = 1150057,

        /// <summary>
        /// double
        /// </summary>	
        UpconverterCenterFrequencyIncrement = 1150058,

        /// <summary>
        /// double
        /// </summary>	
        UpconverterCenterFrequencyIncrementAnchor = 1150059,

        /// <summary>
        /// double
        /// </summary>	
        ArbFilterRaisedCosineAlpha = 1150060,

        /// <summary>
        /// long
        /// </summary>	
        MemorySize = 1150061,

        /// <summary>
        /// double
        /// </summary>	
        AnalogModulationPmDeviation = 1150062,

        /// <summary>
        /// string
        /// </summary>	
        ExportedDoneEventOutputTerminal = 1150063,

        /// <summary>
        /// string
        /// </summary>	
        ExportedMarkerEventOutputTerminal = 1150064,

        /// <summary>
        /// string
        /// </summary>	
        ExportedStartedEventOutputTerminal = 1150065,

        /// <summary>
        /// double
        /// </summary>	
        LoOutPower = 1150066,

        /// <summary>
        /// double
        /// </summary>	
        LoInPower = 1150067,

        /// <summary>
        /// double
        /// </summary>	
        ArbTemperature = 1150068,

        /// <summary>
        /// bool
        /// </summary>	
        IqImpairmentEnabled = 1150069,

        /// <summary>
        /// double
        /// </summary>	
        IqIOffset = 1150070,

        /// <summary>
        /// double
        /// </summary>	
        IqQOffset = 1150071,

        /// <summary>
        /// double
        /// </summary>	
        IqGainImbalance = 1150072,

        /// <summary>
        /// double
        /// </summary>	
        IqSkew = 1150073,

        /// <summary>
        /// double
        /// </summary>	
        LoTemperature = 1150075,

        /// <summary>
        /// int
        /// </summary>	
        IqOffsetUnits = 1150081,

        /// <summary>
        /// RfsgFrequencySettlingUnits
        /// </summary>	
        FrequencySettlingUnits = 1150082,

        /// <summary>
        /// double
        /// </summary>	
        FrequencySettling = 1150083,

        /// <summary>
        /// string
        /// </summary>	
        ModuleRevision = 1150084,

        /// <summary>
        /// double
        /// </summary>	
        ExternalGain = 1150085,

        /// <summary>
        /// double
        /// </summary>	
        DataTransferMaximumBandwidth = 1150086,

        /// <summary>
        /// int
        /// </summary>	
        DataTransferPreferredPacketSize = 1150087,

        /// <summary>
        /// int
        /// </summary>	
        DataTransferMaximumInFlightReads = 1150088,

        /// <summary>
        /// int
        /// </summary>	
        ArbOscillatorPhaseDacValue = 1150089,

        /// <summary>
        /// string
        /// </summary>	
        ActiveConfigurationList = 1150096,

        /// <summary>
        /// int
        /// </summary>	
        ActiveConfigurationListStep = 1150097,

        /// <summary>
        /// int
        /// </summary>	
        ConfigurationListStepTriggerType = 1150098,

        /// <summary>
        /// string
        /// </summary>	
        DigitalEdgeConfigurationListStepTriggerSource = 1150099,

        /// <summary>
        /// double
        /// </summary>	
        TimerEventInterval = 1150100,

        /// <summary>
        /// bool
        /// </summary>
        PulseModulationEnabled = 1250051,

        /// <summary>
        /// int
        /// </summary>
        AutomaticThermalCorrection = 1150008,

        /// <summary>
        /// double
        /// </summary>
        LastExternalCalibrationTemperature = 1150077,

        /// <summary>
        /// int
        /// </summary>
        ExternalCalibrationRecommendedInterval = 1150076,

        /// <summary>
        /// string
        /// </summary>
        ExternalCalibrationUserDefinedInfo = 1150078,

        /// <summary>
        /// int
        /// </summary>
        ExternalCalibrationUserDefinedInfoMaxSize = 1150079,
    }

    #region niRFSG types
    public struct RfsgNIComplexI16
    {

        public RfsgNIComplexI16(short Real, short Imaginary)
        {
            _real = Real; _imaginary = Imaginary;
        }

        //==========================================================================================

        //==========================================================================================
        private short _real;
        public short Real
        {
            get
            {
                return _real;
            }
        }

        //==========================================================================================

        //==========================================================================================
        private short _imaginary;
        public short Imaginary
        {
            get
            {
                return _imaginary;
            }
        }

    }
    #endregion

}
