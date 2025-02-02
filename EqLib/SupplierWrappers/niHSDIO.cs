namespace EqLib
// namespace InstrumentDriverInterop.Ivi
{
    using System;
    using System.Runtime.InteropServices;
    // using NationalInstruments.ModularInstruments.Interop; 
    using NationalInstruments.DAQmx;

    public class niHSDIO : object, System.IDisposable
    {

        public struct nihsdio_wfminfo
        {
            public System.Int64 absoluteTimestamp;
            public System.Int64 relativeTimestamp;
            public System.Int64 dt;
            public int actualSamplesRead;
            public System.Int64 reserved1;
            public System.Int64 reserved2;
        }

        /// <summary>
        /// Creates a new acquisition session.  You can perform static and dynamic acquisition operations with this session.
        /// 
        /// Creating a new session does not automatically tristate your front panel terminals or channels that might be driving voltages 		
	/// from previous sessions (refer to the niHSDIO_close function for more information on lines left driving after closing a 		
	/// session).
        /// 
        /// Pass VI_TRUE to the Reset_Instrument parameter to place your device in a known start-up when creating a new session. This 			
	/// action is equivalent to calling niHSDIO_reset, and it tristates the front panel terminals and channels.
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// Specifies the device name, for example, "Dev1" where "Dev1" is a device name assigned by Measurement & Automation Explorer.
        /// 
        /// </param>
        /// <param name="ID_Query">
        /// Specifies whether the driver performs an ID query on the device. When this parameter is set to VI_TRUE, NI-HSDIO ensures 			
	/// compatibility between the device and the driver.
        /// 
        /// Defined Values:
        /// - VI_TRUE(1) Perform ID query
        /// - VI_FALSE(0) Skip ID query
        /// 
        /// </param>
        /// <param name="Reset_Instrument">
        /// Specifies whether the driver resets the device during initialization of the session. Refer to the niHSDIO_reset function for 		
	/// more information on what happens during a device reset.
        /// 
        /// Defined Values:
        /// - VI_FALSE(0) - Do not reset device
        /// - VI_TRUE(1) - Reset device
        /// 
        /// NOTE: Resetting your device resets the ENTIRE device. Acquisition or generation operations in progress are aborted and 			
	/// cleared.
        /// 
        /// </param>
        /// <param name="Option_String">
        /// Currently unused. Set this string to "".
        /// </param>
        /// <param name="Instrument_Handle">
        /// Returns a VISession handle. Use this handle to identify the device in all subsequent instrument driver function calls related 	
	/// to your acquisition operation.
        /// </param>
        public static niHSDIO InitAcquisitionSession(string Resource_Name, bool ID_Query, bool Reset_Instrument, string Option_String)
        {
            System.IntPtr handle;
            int pInvokeResult = PInvoke.InitAcquisitionSession(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset_Instrument), Option_String, out handle);
            PInvoke.TestForError(System.IntPtr.Zero, pInvokeResult);
            try
            {
                return new niHSDIO(handle);
            }
            catch (System.Exception e)
            {
                PInvoke.close(handle);
                throw e;
            }
        }

        /// <summary>
        /// Creates a new generation session.  You can perform static and dynamic generation operations with this session.
        /// 
        /// Creating a new session does not automatically tristate your front panel terminals or channels that might be driving voltages 			
	/// from previous sessions (refer to the niHSDIO_close function for more information on lines left driving after closing a 			
	/// session).
        /// 
        /// Pass VI_TRUE to the resetInstrument parameter to place your device in a known start-up state when creating a new session. 			
	/// This action is equivalent to calling niHSDIO_reset, and it tristates the front panel terminals and channels.
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// Specifies the device name, for example "Dev1" where "Dev1" is a device name assigned by Measurement & Automation Explorer.
        /// </param>
        /// <param name="ID_Query">
        /// Specifies whether the driver performs an ID query upon the device. When this parameter is set to VI_TRUE, the driver ensures 		
	/// compatibility between the device and driver.
        /// 
        /// Defined Values:
        /// - VI_TRUE(1) Perform ID query
        /// - VI_FALSE(0) Skip ID query
        /// 
        /// </param>
        /// <param name="Reset_Instrument">
        /// Specifies whether the driver resets the device during initialization of the session. Refer to the niHSDIO_reset function for 		
	/// more information on what happens during a device reset.
        /// 
        /// Defined Values:
        /// - VI_FALSE(0) - Do not reset device.
        /// - VI_TRUE(1) - Reset device.
        /// 
        /// NOTE: Resetting your device resets the ENTIRE device. Acquisition or generation operations in progress are aborted and 			
	/// cleared.
        /// 
        /// </param>
        /// <param name="Option_String">
        /// Currently unused.  Set this string to "".
        /// </param>
        /// <param name="Instrument_Handle">
        /// Returns a VISession handle. Use this handle to identify the device in all subsequent instrument driver function calls related 	
	/// to your generation operation.
        /// </param>
        public static niHSDIO InitGenerationSession(string Resource_Name, bool ID_Query, bool Reset_Instrument, string Option_String)
        {
            System.IntPtr handle;
            int pInvokeResult = PInvoke.InitGenerationSession(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset_Instrument), Option_String, out handle);
            PInvoke.TestForError(System.IntPtr.Zero, pInvokeResult);
            try
            {
                return new niHSDIO(handle);
            }
            catch (System.Exception e)
            {
                PInvoke.close(handle);
                throw e;
            }
        }

        /// <summary>
        /// Creates and initializes a special NI-HSDIO external calibration session. The ViSession returned is an NI-HSDIO session that 		
	/// can be used during the calibration session.
        /// 
        /// Multiple calls to this function return the same session ID. Calibration sessions are mutually exclusive with acquisition and 		
	/// generation sessions.
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// 
        /// </param>
        /// <param name="Password">
        /// The calibration password required to open an external calibration session to the device. 
        /// 
        /// Default Value: "" 
        /// </param>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// </param>
        public static niHSDIO InitExtCal(string Resource_Name, string Password)
        {
            System.IntPtr handle;
            int pInvokeResult = PInvoke.InitExtCal(Resource_Name, Password, out handle);
            PInvoke.TestForError(System.IntPtr.Zero, pInvokeResult);
            try
            {
                return new niHSDIO(handle);
            }
            catch (System.Exception e)
            {
                PInvoke.close(handle);
                throw e;
            }
        }

        /// <summary>
        /// Configures the voltage levels for the data channels using a logic family.
        /// 
        /// NOTE: Refer to the device documentation for descriptions of logic families and possible voltage restrictions.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// Identifies channels to apply settings. Use "" or VI_NULL to specify all channels.
        /// </param>
        /// <param name="Logic_Family">
        /// Specifies the logic family for the data voltage levels.
        /// 
        /// Defined Values:
        /// 
        /// -NIHSDIO_VAL_1_8V_LOGIC
        /// -NIHSDIO_VAL_2_5V_LOGIC
        /// -NIHSDIO_VAL_3_3V_LOGIC
        /// -NIHSDIO_VAL_5_0V_LOGIC
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDataVoltageLogicFamily(string Channel_List, int Logic_Family)
        {
            int pInvokeResult = PInvoke.ConfigureDataVoltageLogicFamily(this._handle, Channel_List, Logic_Family);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the voltage levels of the data channels using the specified high and low levels.
        /// 
        /// NOTE: Refer to the device documentation for possible voltage restrictions.
        /// 
        /// NOTE: If you are using an NI 654x device for generation sessions, set High_Level to the appropriate logic family value  and 		
	/// set How_Level to 0. For acquisition sessions with the 
        /// NI 654x, select the same value for High_Level and Low_Level from the following list: 0.9 V, 1.25 V, or 1.65 V.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// Identifies channels to apply settings. Use "" or VI_NULL to specify all channels.
        /// </param>
        /// <param name="Low_Level">
        /// Specifies what voltage identifies logic low level.
        /// 
        /// </param>
        /// <param name="High_Level">
        /// Specifies what voltage identifies logic high level.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDataVoltageCustomLevels(string Channel_List, double Low_Level, double High_Level)
        {
            int pInvokeResult = PInvoke.ConfigureDataVoltageCustomLevels(this._handle, Channel_List, Low_Level, High_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the voltage levels for the trigger channels using a logic family.
        /// 
        /// NOTE: Refer to the device documentation for descriptions of logic families and possible voltage restrictions.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Logic_Family">
        /// Specifies the logic family for the trigger voltage levels.
        /// 
        /// Defined Values:
        /// 
        /// -NIHSDIO_VAL_5_0V_LOGIC
        /// -NIHSDIO_VAL_3_3V_LOGIC
        /// -NIHSDIO_VAL_2_5V_LOGIC
        /// -NIHSDIO_VAL_1_8V_LOGIC
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureTriggerVoltageLogicFamily(int Logic_Family)
        {
            int pInvokeResult = PInvoke.ConfigureTriggerVoltageLogicFamily(this._handle, Logic_Family);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the voltage levels of the trigger channels using user-defined high and low levels.
        /// 
        /// NOTE: Refer to the device documentation for possible voltage restrictions.
        /// 
        /// NOTE: If you are using an NI 654x device for generation sessions, set High_Level to the appropriate logic family value and 			
	/// set How_Level to 0. For acquisition sessions with the 
        /// NI 654x, select the same value for High_Level and Low_Level from the following list: 0.9 V, 1.25 V, or 1.65 V.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Low_Level">
        /// Specifies what voltage identifies logic low level.
        /// 
        /// </param>
        /// <param name="High_Level">
        /// Specifies what voltage identifies logic high level.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureTriggerVoltageCustomLevels(double Low_Level, double High_Level)
        {
            int pInvokeResult = PInvoke.ConfigureTriggerVoltageCustomLevels(this._handle, Low_Level, High_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the voltage levels for the event channels using a logic family.
        /// 
        /// NOTE: Refer to the device documentation for descriptions of logic families and possible voltage restrictions.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Logic_Family">
        /// Specifies the logic family for the event voltage levels.
        /// 
        /// Defined Values:
        /// 
        /// -NIHSDIO_VAL_1_8V_LOGIC
        /// -NIHSDIO_VAL_2_5V_LOGIC
        /// -NIHSDIO_VAL_3_3V_LOGIC
        /// -NIHSDIO_VAL_5_0V_LOGIC
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureEventVoltageLogicFamily(int Logic_Family)
        {
            int pInvokeResult = PInvoke.ConfigureEventVoltageLogicFamily(this._handle, Logic_Family);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the voltage levels of the event channels using user-defined high and low levels.
        /// 
        /// NOTE: Refer to the device documentation for possible voltage restrictions.
        /// 
        /// NOTE: If you are using an NI 654x device for generation sessions, set High_Level to the appropriate logic family value and 			
	/// set How_Level to 0. For acquisition sessions with the 
        /// NI 654x, select the same value for High_Level and Low_Level from the following list: 0.9 V, 1.25 V, or 1.65 V.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Low_Level">
        /// Specifies what voltage identifies logic low level.
        /// 
        /// </param>
        /// <param name="High_Level">
        /// Specifies what voltage identifies logic high level.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureEventVoltageCustomLevels(double Low_Level, double High_Level)
        {
            int pInvokeResult = PInvoke.ConfigureEventVoltageCustomLevels(this._handle, Low_Level, High_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures channels for dynamic acquisition (if vi is an acquisition session) or dynamic generation (if vi is a generation 			
	/// session).
        /// 
        /// NOTE: A channel cannot be assigned to static generation and dynamic generation at the same time.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or 	
	/// niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string identifies which channels are reserved for dynamic operation.
        /// 
        /// Valid Syntax:
        /// "0-19" or "0-15,16-19" or "0-18,19"
        /// "" (empty string) or VI_NULL to specify all channels
        /// "none" to unassign all channels
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int AssignDynamicChannels(string Channel_List)
        {
            int pInvokeResult = PInvoke.AssignDynamicChannels(this._handle, Channel_List);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Commits any pending attributes to hardware and starts the dynamic operation (refer to niHSDIO_CommitDynamic for more 			
	/// information on committing).
        /// 
        /// For an generation operation with a Start trigger configured, calling niHSDIO_Initiate causes the channels to go to their 			
	/// Initial states (refer to niHSDIO_ConfigureInitialState for more information on Initial states).
        /// 
        /// This function is valid only for dynamic operations (acquisition or generation). It is not valid for static operations.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or 	
	/// niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Call this function to pause execution of your program until the dynamic data operation is completed or the function returns a 		
	/// timeout error. niHSDIO_WaitUntilDone is a blocking function that periodically checks the operation status. It returns control 		
	/// to the calling program if the operation completes successfully or an error occurs (including a timeout error).
        /// 
        /// This function is most useful for finite data operations that you expect to complete within a certain time.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or 	
	/// niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// This parameter specifies the number of milliseconds to allow the function to complete before returning. If the specified time 		
	/// elapses before the data operation has completed, the function returns a timeout error.
        /// 
        /// - Setting a value of 0 causes the function to return immediately. This setting can be useful to manually poll for hardware 			
	/// errors after a data operation has been initiated. If no other error has occurred and the data operation is still not 			
	/// complete, the function returns a timeout error.
        /// 
        /// - Setting a value of -1 causes the function to never timeout. Be careful not to use this value during a continuous operation, 		
	/// as it will never return unless a HW error occurs. Perform a manual device reset from Measurement and Automation Explorer if 	
	/// you get stuck in this state or use niHSDIO_reset or niHSDIO_ResetDevice from the other session of the device.
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WaitUntilDone(int Max_Time_Milliseconds)
        {
            int pInvokeResult = PInvoke.WaitUntilDone(this._handle, Max_Time_Milliseconds);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Stops a running dynamic session.  This function is generally not required on finite data operations, as they complete on 			
	/// their own after the last data point is generated or acquired.  This function is generally required for continuous operations 		
	/// or if you wish to interrupt a finite operation before it is completed.
        /// 
        /// This function is valid for dynamic operations (acquisition or generation) only.  It is not valid for static operations.
        /// 
	/// NOTE: To avoid receiving hardware clocking errors when reconfiguring an external clock, explicitly call the niHSDIO_Abort 			
	/// function after your finite operation has completed before performing any clocking reconfiguration.  An external clock that 			
	/// stops sending pulses to the device (even after a finite operation has completed) may cause NI-HSDIO to return an error, 			
	/// stating that the clock became unlocked, if the device has not implicitly aborted yet.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or 	
	/// niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Configures the acquisition size, including the number of acquired records and the minimum record size.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_Per_Record">
        /// Sets the number of samples to be acquired per record. If you need pre- and post-trigger points, configure a Reference trigger 		
	/// and specify the number of pretrigger points.
        /// </param>
        /// <param name="Number_Of_Records">
        /// Sets how many records are acquired. Currently this value must be set to 1.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureAcquisitionSize(int Samples_Per_Record, int Number_Of_Records)
        {
            int pInvokeResult = PInvoke.ConfigureAcquisitionSize(this._handle, Samples_Per_Record, Number_Of_Records);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Selects between acquiring high/low data or valid/invalid data during a static or dynamic acquisition operation.
        /// 
        /// Select High or Low mode to get logic high or logic low values. Select Valid or Invalid mode to determine if the signal is 			
	/// within the specified voltage range (above data voltage low level but below data voltage high level) or outside the range(below 		
	/// data voltage low level or above data voltage high level). Refer to the Data Interpretation topic in the NI Digital Waveform 		
	/// Generator/Analyzer Help to understand how data is returned to you.
        /// 
        /// NOTE: NI 654x/656x devices only support the High or Low mode of data interpretation. NI-HSDIO returns an error if you select 		
	/// Valid or Invalid mode for an acquisition with these devices.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// Identifies channels to apply settings. Use "" or VI_NULL to specify all channels.
        /// </param>
        /// <param name="Data_Interpretation">
        /// Selects the data interpretation mode.
        /// 
        /// Defined Values:
        /// 
        /// -   NIHSDIO_VAL_HIGH_OR_LOW - Data read represents logical values (logic high or logic low)
        /// 
        /// -   NIHSDIO_VAL_VALID_OR_INVALID - Data read represents whether channel data is within the specified voltage range.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDataInterpretation(string Channel_List, int Data_Interpretation)
        {
            int pInvokeResult = PInvoke.ConfigureDataInterpretation(this._handle, Channel_List, Data_Interpretation);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates a waveform acquisition on channels enabled for dynamic acquisition, waits until it acquires the number of samples in 		
	/// Samples_To_Read, and returns the acquired binary data.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in Samples_To_Read. If you set 	
	/// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout 		
	/// error. If you specify a value for Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO 		
	/// returns the available samples after Max_Time_Milliseconds. A value of -1 causes the function to never time out.
        /// 
        /// </param>
        /// <param name="Number_Of_Samples_Read">
        /// Returns the number of samples that were successfully fetched and transferred into data[].
        /// </param>
        /// <param name="Data">
        /// Returns the preallocated array where acquired samples are written.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 		
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ReadWaveformU32(int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, uint[] Data)
        {
            int pInvokeResult = PInvoke.ReadWaveformU32(this._handle, Samples_To_Read, Max_Time_Milliseconds, out Number_Of_Samples_Read, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers acquired binary data from onboard memory to PC memory.  The data was acquired to onboard memory previously by calling 		
	/// the niHSDIO_Initiate function.
        /// 
        /// If the number of samples specified in Samples_To_Read is still not available after the number of milliseconds specified in 	
 	/// Max_Time_Milliseconds, this function returns no data with a timeout error.
        /// 
        /// The fetch position can be modified by using niHSDIO_SetAttributeViInt32 and the NIHSDIO_ATTR_FETCH_RELATIVE_TO or 	
 	/// NIHSDIO_ATTR_FETCH_OFFSET attributes. The default value for NIHSDIO_ATTR_FETCH_RELATIVE_TO is NIHSDIO_VAL_CURRENT_READ_POSITION. 	
	/// The default value for NIHSDIO_ATTR_FETCH_OFFSET is 0.
        /// 
        /// Calling this function is not necessary if you use the niHSDIO_ReadWaveformU32 function, as the fetch is performed as part of 		
	/// that function.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in Samples_To_Read. If you set 	
	/// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout 	
	/// error. If you specify a value for Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO 	
	/// returns the available samples after max time milliseconds. A value of -1 causes the function to never time out.
        /// 
        /// </param>
        /// <param name="Number_Of_Samples_Read">
        /// Returns the number of samples that were successfully fetched and transferred into data[].
        /// </param>
        /// <param name="Data">
        /// Returns the preallocated array where acquired samples are written.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 	
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int FetchWaveformU32(int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, uint[] Data)
        {
            int pInvokeResult = PInvoke.FetchWaveformU32(this._handle, Samples_To_Read, Max_Time_Milliseconds, out Number_Of_Samples_Read, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates a waveform acquisition on channels enabled for dynamic acquisition, waits until it acquires the number of samples in 	
	/// Samples_To_Read, and returns the acquired binary data.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in samples_To_Read. If you set 	
	/// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout 		
	/// error. If you specify a value for Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO 		
	/// returns the available samples after Max_Time_Milliseconds. A value of -1 causes the function to never time out.
        /// 
        /// </param>
        /// <param name="Number_Of_Samples_Read">
        /// Returns the number of samples that were successfully fetched and transferred into data[].
        /// </param>
        /// <param name="Data">
        /// Returns the preallocated array where acquired samples are written.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 	
	/// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ReadWaveformU16(int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, short[] Data)
        {
            int pInvokeResult = PInvoke.ReadWaveformU16(this._handle, Samples_To_Read, Max_Time_Milliseconds, out Number_Of_Samples_Read, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers acquired binary data from onboard memory to PC memory.  The data was acquired to onboard memory previously by calling         
	/// the niHSDIO_Initiate function.
        /// 
        /// If the number of samples specified in Samples_To_Read is still not available after the number of milliseconds specified in         
	/// Max_Time_Milliseconds, this function returns no data with a timeout error.
        /// 
        /// The fetch position can be modified by using niHSDIO_SetAttributeViInt32 and the NIHSDIO_ATTR_FETCH_RELATIVE_TO or         
	/// NIHSDIO_ATTR_FETCH_OFFSET attributes. The default value for NIHSDIO_ATTR_FETCH_RELATIVE_TO is NIHSDIO_VAL_CURRENT_READ_POSITION.       
	/// The default value for NIHSDIO_ATTR_FETCH_OFFSET is 0.
        /// 
        /// Calling this function is not necessary if you use the niHSDIO_ReadWaveformU16 function, as the fetch is performed as part of that      
	/// function.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in Samples_To_Read. If you set
        /// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout error. If
        /// you specify a value for 
        /// Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO returns the available samples after max time 
        /// milliseconds. A 
        /// value of -1 causes the function to never time out.
        /// 
        /// </param>
        /// <param name="Number_Of_Samples_Read">
        /// Returns the number of samples that were successfully fetched and transferred into data[].
        /// </param>
        /// <param name="Data">
        /// Returns the preallocated array where acquired samples are written.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int FetchWaveformU16(int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, short[] Data)
        {
            int pInvokeResult = PInvoke.FetchWaveformU16(this._handle, Samples_To_Read, Max_Time_Milliseconds, out Number_Of_Samples_Read, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates a waveform acquisition on channels enabled for dynamic acquisition, waits until it acquires the number of samples in 
        /// Samples_To_Read, and returns the acquired binary data.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in samples_To_Read. If you set 
        /// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout error. If 
        /// you specify a value for Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO returns the 
        /// available samples after Max_Time_Milliseconds. A value of -1 causes the function to never time out.
        /// 
        /// </param>
        /// <param name="Number_Of_Samples_Read">
        /// Returns the number of samples that were successfully fetched and transferred into data[].
        /// </param>
        /// <param name="Data">
        /// Returns the preallocated array where acquired samples are written.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ReadWaveformU8(int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, byte[] Data)
        {
            int pInvokeResult = PInvoke.ReadWaveformU8(this._handle, Samples_To_Read, Max_Time_Milliseconds, out Number_Of_Samples_Read, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers acquired binary data from onboard memory to PC memory.  The data was acquired to onboard memory previously by calling the 
        /// niHSDIO_Initiate function.
        /// 
        /// If the number of samples specified in Samples_To_Read is still not available after the number of milliseconds specified in 
        /// Max_Time_Milliseconds, this function returns no data with a timeout error.
        /// 
        /// The fetch position can be modified by using niHSDIO_SetAttributeViInt32 and the NIHSDIO_ATTR_FETCH_RELATIVE_TO or 
        /// NIHSDIO_ATTR_FETCH_OFFSET attributes. The default value for NIHSDIO_ATTR_FETCH_RELATIVE_TO is NIHSDIO_VAL_CURRENT_READ_POSITION. The 
        /// default value for NIHSDIO_ATTR_FETCH_OFFSET is 0.
        /// 
        /// Calling this function is not necessary if you use the niHSDIO_ReadWaveformU8 function, as the fetch is performed as part of that 
        /// function.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in Samples_To_Read. If you set 
        /// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout error. If 
        /// you specify a value for Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO returns the 
        /// available samples after max time milliseconds. A value of -1 causes the function to never time out.
        /// 
        /// </param>
        /// <param name="Number_Of_Samples_Read">
        /// Returns the number of samples that were successfully fetched and transferred into data[].
        /// </param>
        /// <param name="Data">
        /// Returns the preallocated array where acquired samples are written.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int FetchWaveformU8(int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, byte[] Data)
        {
            int pInvokeResult = PInvoke.FetchWaveformU8(this._handle, Samples_To_Read, Max_Time_Milliseconds, out Number_Of_Samples_Read, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates a multirecord acquisition and returns the acquired waveform as a two-dimensional array of unsigned 32-bit data.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Starting_Record">
        /// Specifies the first record you want to read.
        /// </param>
        /// <param name="Records_To_Read">
        /// The number of records you want to read.
        /// </param>
        /// <param name="Waveform_Data">
        /// Returns the array of waveform data that contains the records to read.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int ReadMultiRecordU32(int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, uint[] Data, nihsdio_wfminfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.ReadMultiRecordU32(this._handle, Samples_To_Read, Max_Time_Milliseconds, Starting_Record, Number_of_Records, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Fetches the data as a two-dimensional array of unsigned 32-bit integers and returns the number of samples read.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Starting_Record">
        /// Specifies the first record you want to read.
        /// </param>
        /// <param name="Records_To_Read">
        /// Specifies the number of records you want to read.
        /// </param>
        /// <param name="Waveform_Data">
        /// Returns the array of waveform data that contains the records to read.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int FetchMultiRecordU32(int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, uint[] Data, nihsdio_wfminfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchMultiRecordU32(this._handle, Samples_To_Read, Max_Time_Milliseconds, Starting_Record, Number_of_Records, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates a multirecord acquisition, and returns the acquired waveform as a two-dimensional array of unsigned 16-bit data.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Starting_Record">
        /// Specifies the first record you want to read.
        /// </param>
        /// <param name="Records_To_Read">
        /// Specifies the number of records you want to read.
        /// </param>
        /// <param name="Waveform_Data">
        /// Returns the array of waveform data that contains the records to read.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int ReadMultiRecordU16(int Max_Time_Milliseconds, int Samples_To_Read, int Starting_Record, int Number_of_Records, short[] Data, nihsdio_wfminfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.ReadMultiRecordU16(this._handle, Max_Time_Milliseconds, Samples_To_Read, Starting_Record, Number_of_Records, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Fetches the data as a two-dimensional array of unsigned 16-bit integers and returns the number of samples read.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Starting_Record">
        /// Specifies the first record you want to read.
        /// </param>
        /// <param name="Records_To_Read">
        /// Specifies the number of records you want to read.
        /// </param>
        /// <param name="Waveform_Data">
        /// Returns the array of waveform data that contains the records to read.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int FetchMultiRecordU16(int Max_Time_Milliseconds, int Samples_To_Read, int Starting_Record, int Number_of_Records, short[] Data, nihsdio_wfminfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchMultiRecordU16(this._handle, Max_Time_Milliseconds, Samples_To_Read, Starting_Record, Number_of_Records, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Initiates a multirecord acquisition, and returns the acquired waveform as a two-dimensional array of unsigned 8-bit data.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Starting_Record">
        /// Specifies the first record you want to read.
        /// </param>
        /// <param name="Records_To_Read">
        /// Specifies the number of records you want to read.
        /// </param>
        /// <param name="Waveform_Data">
        /// Returns the array of waveform data that contains the records to read.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int ReadMultiRecordU8(int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, byte[] Data, nihsdio_wfminfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.ReadMultiRecordU8(this._handle, Samples_To_Read, Max_Time_Milliseconds, Starting_Record, Number_of_Records, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Fetches the data as a two-dimensional array of unsigned 8-bit integers and returns the number of samples read.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Starting_Record">
        /// Specifies the first record you want to read.
        /// </param>
        /// <param name="Records_To_Read">
        /// Specifies the number of records you want to read.
        /// </param>
        /// <param name="Waveform_Data">
        /// Returns the array of waveform data that contains the records to read.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int FetchMultiRecordU8(int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, byte[] Data, nihsdio_wfminfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchMultiRecordU8(this._handle, Samples_To_Read, Max_Time_Milliseconds, Starting_Record, Number_of_Records, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers acquired waveform data from device memory directly to PC memory allocated by a Direct DMA-compatible device.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// </param>
        /// <param name="Samples_To_Read">
        /// Specifies the number of samples to fetch.
        /// </param>
        /// <param name="Buffer_Size">
        /// Specifies the size (in bytes) of the buffer in memory at which to transfer acquired data.
        /// </param>
        /// <param name="Buffer_Address">
        /// Specifies the location of the buffer in memory at which to transfer acquired data.
        /// </param>
        /// <param name="Waveform_Info">
        /// Returns information about the records. This parameter includes an absolute timestamp, relative timestamp, the number of samples 
        /// acquired, and the dT of the waveform.
        /// </param>
        /// <param name="Offset_to_First_Sample">
        /// Returns the offset of the first sample acquired within the specified buffer. Data is transfered from device memory in 128 bytes 
        /// increments, so the first sample of the acquired data typically occurs at some offset from the start of the buffer when using a 
        /// Reference trigger.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int FetchWaveformDirectDMA(int Max_Time_Milliseconds, int Samples_To_Read, uint Buffer_Size, System.IntPtr Buffer_Address, nihsdio_wfminfo Waveform_Info, uint Offset_to_First_Sample)
        {
            int pInvokeResult = PInvoke.FetchWaveformDirectDMA(this._handle, Max_Time_Milliseconds, Samples_To_Read, Buffer_Size, Buffer_Address, Waveform_Info, Offset_to_First_Sample);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the sample error information from a hardware comparison operation. 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Sample_Errors_to_Read">
        /// Specifies the number of sample errors to fetch.
        /// </param>
        /// <param name="Max_Time_Milliseconds">
        /// Specifies in milliseconds how long to allow the function to complete before returning a timeout error.
        /// 
        /// A value of 0 causes the function to return immediately with up to the number of samples specified in Samples_To_Read. If you set 
        /// Max_Time_Milliseconds to a value other than 0, and timeout occurs before all the samples are acquired, you receive a timeout error. If 
        /// you specify a value for Samples_To_Read that is greater than the number of samples in the device memory, NI-HSDIO returns the 
        /// available samples after max time milliseconds. A value of -1 causes the function to never timeout.
        /// 
        /// Default Value: 10000
        /// </param>
        /// <param name="Number_Of_Samples_Error_Read">
        /// Returns the total number of sample errors read from device memory.
        /// </param>
        /// <param name="Error_Sample_Numbers">
        /// Returned array which indicates the number of samples with errors.
        /// </param>
        /// <param name="Error_Bits">
        /// Returns the bit numbers of the data within the samples with errors. Please note, the sampleNumber[i] and errorBits[i] correspond to 
        /// one another
        /// 
        /// </param>
        /// <param name="Error_Repeat_Counts">
        /// Returns the number of times that error was repeated.
        /// </param>
        /// <param name="Reserved_1">
        /// Reserved filed. Use NULL.
        /// </param>
        /// <param name="Reserved_2">
        /// Reserved filed. Use NULL.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int HWC_FetchSampleErrors(int Number_of_Sample_Errors_to_Read, int Max_Time_Milliseconds, out int Number_Of_Sample_Errors_Read, double[] Sample_Number, uint[] Error_Bits, uint[] Error_Repeat_Counts, out uint Reserved_1, out uint Reserved_2)
        {
            int pInvokeResult = PInvoke.HWC_FetchSampleErrors(this._handle, Number_of_Sample_Errors_to_Read, Max_Time_Milliseconds, out Number_Of_Sample_Errors_Read, Sample_Number, Error_Bits, Error_Repeat_Counts, out Reserved_1, out Reserved_2);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers waveform data from PC memory to onboard memory. Supported devices for this function depend on the data width for your 
        /// device, not on the number of assigned dynamic channels. This function may be used when the data width is 4.
        /// 
        /// If you specify a Waveform_Name not already allocated on the device, the appropriate amount of onboard memory is allocated (if 
        /// available) and the data is stored in that new location.
        /// 
        /// Data is always written to memory starting at the current write position of the waveform. A new waveform's write position is the start 
        /// of the allocated memory. Calling niHSDIO_WriteNamedWaveformU32 moves the next write position to the end of the data just written. 
        /// Thus, subsequent calls to niHSDIO_WriteNamedWaveformU32 append data to the end of previously written data. You may also manually 
        /// change the write position by calling niHSDIO_SetNamedWaveformNextWritePosition. If you try to write past the end of the allocated 
        /// space, NI-HSDIO returns an error. 
        /// 
        /// Waveforms are stored contiguously in onboard memory.  You cannot resize an existing named waveform. Instead, delete the existing 
        /// waveform using niHSDIO_DeleteNamedWaveform and then recreate it with the new size using the same name.
        /// 
        /// This function calls niHSDIO_CommitDynamic - all pending attributes are committed to hardware.
        /// 
        /// When you explicitly call niHSDIO_AllocateNamedWaveform and write waveforms using multiple niHSDIO_WriteNamedWaveformU32 calls, each 
        /// waveform block written must be a multiple of 32 samples for the NI 654x/655x devices or a multiple of 64 samples 
        /// for the NI 656x devices (128 samples if the NI 656x is in 
        /// DDR mode).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="Samples_To_Write">
        /// Specifies the number of samples in data to be written to onboard memory.
        /// 
        /// </param>
        /// <param name="Data">
        /// Specifies the waveform data.
        /// 
        /// If you want to use direct DMA to write your waveform from onboard memory, pass the memory address (pointer value) of the region so 
        /// that you write within the direct DMA window.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WriteNamedWaveformU32(string Waveform_Name, int Samples_To_Write, uint[] Data)
        {
            int pInvokeResult = PInvoke.WriteNamedWaveformU32(this._handle, Waveform_Name, Samples_To_Write, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers waveform data from PC memory to onboard memory. Supported devices for this function depend on the data width for your 
        /// device, not on the number of assigned dynamic channels. This function may be used when the data width is 2.
        /// 
        /// If you specify a waveformName not already allocated on the device, the appropriate amount of onboard memory is allocated (if 
        /// available) and the data is stored in that new location.
        /// 
        /// Data is always written to memory starting at the current write position of the waveform. A new waveform's write position is the start 
        /// of the allocated memory. Calling this function moves the next write position to the end of the data just written. Thus, subsequent 
        /// calls to this function append data to the end of previously written data. You may also manually change the write position by calling 
        /// niHSDIO_SetNamedWaveformNextWritePosition. If you try to write past the end of the allocated space, NI-HSDIO returns an error. 
        /// 
        /// Waveforms are stored contiguously in onboard memory.  You cannot resize an existing named waveform. Instead, delete the existing 
        /// waveform using niHSDIO_DeleteNamedWaveform and then recreate it with the new size using the same name.
        /// 
        /// This function calls niHSDIO_CommitDynamic - all pending attributes are committed to hardware.
        /// 
        /// When you explicitly call niHSDIO_AllocateNamedWaveform and write waveforms using multiple niHSDIO_WriteNamedWaveformU16 calls, each 
        /// waveform block written must be a multiple of 32 samples for the NI 654X/655X devices or a multiple of 64 samples 
        /// for the NI 656X devices (128 samples if the NI 656X is in 
        /// DDR mode).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="Samples_To_Write">
        /// Specifies the number of samples in data to be written to onboard memory.
        /// 
        /// </param>
        /// <param name="Data">
        /// Specifies the waveform data.
        /// 
        /// If you want to use direct DMA to write your waveform from onboard memory, pass the memory address (pointer value) of the region so 
        /// that you write within the direct DMA window.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WriteNamedWaveformU16(string Waveform_Name, int Samples_To_Write, short[] Data)
        {
            int pInvokeResult = PInvoke.WriteNamedWaveformU16(this._handle, Waveform_Name, Samples_To_Write, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers waveform data from PC memory to onboard memory. Supported devices for this function depend on the data width for your 
        /// device, not on the number of assigned dynamic channels. This function may be used when the data width is 1.
        /// 
        /// If you specify a waveformName not already allocated on the device, the appropriate amount of onboard memory is allocated (if 
        /// available) and the data is stored in that new location.
        /// 
        /// Data is always written to memory starting at the current write position of the waveform. A new waveform's write position is the start 
        /// of the allocated memory. Calling this function moves the next write position to the end of the data just written. Thus, subsequent 
        /// calls to this function append data to the end of previously written data. You may also manually change the write position by calling 
        /// niHSDIO_SetNamedWaveformNextWritePosition. If you try to write past the end of the allocated space, NI-HSDIO returns an error. 
        /// 
        /// Waveforms are stored contiguously in onboard memory.  You cannot resize an existing named waveform. Instead, delete the existing 
        /// waveform using niHSDIO_DeleteNamedWaveform and then recreate it with the new size using the same name.
        /// 
        /// This function calls niHSDIO_CommitDynamic - all pending attributes are committed to hardware.
        /// 
        /// When you explicitly call niHSDIO_AllocateNamedWaveform and write waveforms using multiple niHSDIO_WriteNamedWaveformU8 calls, each 
        /// waveform block written must be a multiple of 32 samples for the NI 654X/655X devices or a multiple of 64 samples 
        /// for the NI 656X devices (128 samples if the NI 656X is in 
        /// DDR mode).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="Samples_To_Write">
        /// Specifies the number of samples in data to be written to onboard memory.
        /// 
        /// </param>
        /// <param name="Data">
        /// Specifies the waveform data.
        /// 
        /// If you want to use direct DMA to write your waveform from onboard memory, pass the memory address (pointer value) of the region so 
        /// that you write within the direct DMA window.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WriteNamedWaveformU8(string Waveform_Name, int Samples_To_Write, byte[] Data)
        {
            int pInvokeResult = PInvoke.WriteNamedWaveformU8(this._handle, Waveform_Name, Samples_To_Write, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Transfers multistate digital waveforms from PC memory to onboard memory. Each element of data[] uses one byte per channel per sample. 
        /// The supported values are defined in niHSDIO.h.
        /// 
        /// If you specify a waveformName not already allocated on the device, the appropriate amount of onboard memory is allocated (if 
        /// available), and the data is stored in that new location.
        /// 
        /// Data is always written to memory starting at the current write position of the waveform. A new waveform's write position is the start 
        /// of the allocated memory. Calling this function moves the next write position to the end of the data just written. Thus, subsequent 
        /// calls to this function append data to the end of previously written data. You can manually change the write position by calling 
        /// niHSDIO_SetNamedWaveformNextWritePosition. If you try to write past the end of the allocated space, NI-HSDIO returns an error.
        /// 
        /// Waveforms are stored contiguously in onboard memory. You cannot resize an existing named waveform. Instead, delete the existing 
        /// waveform using niHSDIO_DeleteNamedWaveform and then recreate it with the new size using the same name.
        /// 
        /// This function calls niHSDIO_CommitDynamic--all pending attributes are committed to hardware.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="Samples_To_Write">
        /// Specifies the number of samples in data to be written to onboard memory. This number is not equal to the length of the data[] array, 
        /// since its size is the number of samples to write times the number of channels.
        /// 
        /// </param>
        /// <param name="Data_Layout">
        /// Describes the layout of the waveform contained in data[].
        /// 
        /// Defined Values
        /// 
        /// NIHSDIO_VAL_GROUP_BY_SAMPLE--specifies that consecutive samples in data[] are such that the array contains the first sample from every 
        /// signal in the operation, then the second sample from every signal, up to the last sample from every signal.
        /// 
        /// NIHSDIO_VAL_GROUP_BY_CHANNEL--specifies that consecutive samples in data[] are such that the array contains all the samples from the 
        /// first signal in the operation, then all the samples from the second signal, up to all samples from the last signal. . 
        /// </param>
        /// <param name="Data">
        /// Specifies the digital waveform data. Each value on this array defines the state of one channel of one sample. Supported states are 
        /// defined in niHSDIO.h.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WriteNamedWaveformWDT(string Waveform_Name, int Samples_To_Write, int Data_Layout, byte[] Data)
        {
            int pInvokeResult = PInvoke.WriteNamedWaveformWDT(this._handle, Waveform_Name, Samples_To_Write, Data_Layout, Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Reads and transfers data from a digital .hws file to onboard memory.
        /// 
        /// If you specify a Waveform_Name not already allocated on the device, the appropriate amount of onboard memory is allocated (if 
        /// available), and the data is stored in that new location.
        /// 
        /// Data is always written to memory starting at the current write position of the waveform. A new waveform's write position is the start 
        /// of the allocated memory. Calling this function moves the next write position to the end of the data just written. Thus, subsequent 
        /// calls to this function append data to the end of previously written data. You can manually change the write position by calling 
        /// niHSDIO_SetNamedWaveformNextWritePosition. If you try to write past the end of the allocated space, NI-HSDIO returns an error.
        /// 
        /// Waveforms are stored contiguously in onboard memory. You cannot resize an existing named waveform. Instead, delete the existing 
        /// waveform using niHSDIO_DeleteNamedWaveform and then recreate it with the new size using the same name.
        /// 
        /// This function calls niHSDIO_CommitDynamic--all pending attributes are committed to hardware.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="File_Path">
        /// Specifies the path and file name of the digital .hws file to open. The .hws extension is typically used for .hws files, although using 
        /// this extension is optional. 
        /// </param>
        /// <param name="Use_Rate_From_Waveform">
        /// Controls how the sample rate is computed.
        /// 
        /// Setting this value to TRUE computes the generation rate from the WDT value. If the sample rate has been configured using 
        /// niHSDIO_ConfigureSampleClock function, useRateFromWaveform overrides the sample rate.
        /// </param>
        /// <param name="Waveform_Size">
        /// Returns the number of samples contained in the waveform.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WriteNamedWaveformFromFileHWS(string Waveform_Name, string File_Path, bool Use_Rate_From_Waveform, out int Waveform_Size)
        {
            int pInvokeResult = PInvoke.WriteNamedWaveformFromFileHWS(this._handle, Waveform_Name, File_Path, System.Convert.ToUInt16
            (Use_Rate_From_Waveform), out Waveform_Size);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the idle state for a dynamic generation operation. The idle state may be active in a variety of conditions:
        /// 
        /// -  The generation operation completes normally.
        /// -  The generation operation pauses from an active Pause trigger.
        /// -  The generation operation terminates due to an underflow error.
        /// 
        /// Valid Syntax:
        /// 
        /// The order of channelList determines the order of the pattern string. For example, the following two examples are equivalent:
        /// 
        /// niHSDIO_ConfigureIdleState(vi, "19-0", "0000 0XXX XX11 111Z ZZZZ");
        /// 
        /// niHSDIO_ConfigureIdleState(vi, "0-19", "ZZZZ Z111 11XX XXX0 0000");
        /// 
        /// Refer to Initial and Idle States in the NI Digital Waveform Generator/Analyzer Help for more information.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// Specifies which channels will have their idle value set using the Idle_State string. The order of channels in Channel_List determines 
        /// the order of Idle_State. 
        /// </param>
        /// <param name="Idle_State">
        /// Describes the idle state of a dynamic generation operation. This expression is composed of characters:
        /// 
        /// - 'X' or 'x': keeps the previous value
        /// - '1': sets the channel to logic high
        /// - '0': sets the channel to logic low
        /// - 'Z' or 'z': disables the channel (sets it to high-impedance)
        /// 
        /// The leftmost character in the expression corresponds to the first channel in Channel_List. The number of characters in pattern must 
        /// equal the number of channels specified in Channel_List or an error is returned.
        /// 
        /// The default state of a channel is to keep the previous value.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureIdleState(string Channel_List, string Idle_State)
        {
            int pInvokeResult = PInvoke.ConfigureIdleState(this._handle, Channel_List, Idle_State);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the idle state for a dynamic generation operation. The idle state may be active in a variety of conditions:
        /// 
        /// -   The generation operation completes normally
        /// -   The generation operation pauses from an active Pause trigger
        /// -   The generation operation terminates due to an underflow error
        /// 
        /// Unlike niHSDIO_ConfigureIdleState, which uses a string, this function uses a binary format to represent only logic high and low. If 
        /// you require more choices for your idle state, use the niHSDIO_ConfigureIdleState function.
        /// 
        /// Refer to the Initial and Idle States topic in the NI Digital Waveform Generator/Analyzer Help for more information.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Idle_State">
        /// Bit mask representing the idle state. High is specified with a 1, and low is specified with a 0.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureIdleStateU32(uint Idle_State)
        {
            int pInvokeResult = PInvoke.ConfigureIdleStateU32(this._handle, Idle_State);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the initial state for a dynamic generation operation. The initial state of each channel is driven after the session is initiated 
        /// using niHSDIO_Initiate. Channels remain unchanged until the first waveform sample is generated.
        /// 
        /// The order of Channel_List determines the order of the pattern string. For example, the following two examples are equivalent:
        /// 
        /// - niHSDIO_ConfigureInitialState (vi, "19-0", "0000 0XXX XX11 111Z ZZZZ");
        /// 
        /// - niHSDIO_ConfigureInitialState (vi, "0-19", "ZZZZ Z111 11XX XXX0 0000");
        /// 
        /// Refer to the Initial and Idle States topic in the NI Digital Waveform Generator/Analyzer Help for more information.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// Specifies which channels will have their initial value set using the Initial_State string. The order of channels in the list 
        /// determines the order of the Initial_State string.
        /// </param>
        /// <param name="Initial_State">
        /// This string expression describes the initial state of a dynamic generation operation. This expression is composed of characters:
        /// 
        /// - 'X' or 'x': keeps the previous value
        /// - '1': sets the channel to logic high
        /// - '0': sets the channel to logic low
        /// - 'Z' or 'z': disables the channel or sets it to high-impedance
        /// 
        /// The leftmost character in the expression corresponds to the first channel in channelList. The number of characters in pattern must 
        /// equal the number of channels specified in channelList or an error is returned.
        /// 
        /// The default state of a channel is to keep the previous value.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureInitialState(string Channel_List, string Initial_State)
        {
            int pInvokeResult = PInvoke.ConfigureInitialState(this._handle, Channel_List, Initial_State);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the initial state for a dynamic generation operation. The initial state of each channel is driven after the session is initiated 
        /// using niHSDIO_Initiate. Channels remain unchanged until the first waveform sample is generated.
        /// 
        /// Unlike niHSDIO_ConfigureInitialState which uses a string, this function uses a binary format to represent only logic high and low. If 
        /// you require more choices for your initial state, use the niHSDIO_ConfigureInitialState function.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Initial_State">
        /// Bit mask representing the initial state. High is specified with a 1, and low is specified with a 0.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureInitialStateU32(uint Initial_State)
        {
            int pInvokeResult = PInvoke.ConfigureInitialStateU32(this._handle, Initial_State);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies the number of times to generate a waveform or whether to generate it continuously. This function is only valid when the 
        /// Generation_Mode parameter of niHSDIO_ConfigureGenerationMode is set to NIHSDIO_VAL_WAVEFORM.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Repeat_Mode">
        /// Specifies the repeat mode to configure:
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_FINITE - Calling niHSDIO_Initiate generates the named waveform a finite number of times. The number of times is 
        /// specified by the Repeat_Count parameter.
        /// 
        /// - NIHSDIO_VAL_CONTINUOUS - Calling niHSDIO_Initiate generates the named waveform continuously (until niHSDIO_Abort function is 
        /// called). Repeat_Count is ignored.
        /// 
        /// </param>
        /// <param name="Repeat_Count">
        /// Specifies the number of times to generate the waveform. This parameter is ignored if Repeat_Mode is NIHSDIO_VAL_CONTINUOUS.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain 
        /// additional information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureGenerationRepeat(int Repeat_Mode, int Repeat_Count)
        {
            int pInvokeResult = PInvoke.ConfigureGenerationRepeat(this._handle, Repeat_Mode, Repeat_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the waveform to be generated upon a call to niHSDIO_Initiate when NIHSDIO_ATTR_GENERATION_MODE equals NIHSDIO_VAL_WAVEFORM. This 
	/// function need only be called if multiple waveforms are present in onboard memory (refer to NIHSDIO_ATTR_WAVEFORM_TO_GENERATE for more 

	/// information).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies which waveform to generate upon calling niHSDIO_Initiate.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureWaveformToGenerate(string Waveform_Name)
        {
            int pInvokeResult = PInvoke.ConfigureWaveformToGenerate(this._handle, Waveform_Name);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Reserves waveform space in onboard memory and associates a waveform name with it. Individual waveforms are stored contiguously in onboard 
	/// memory.
        /// 
        /// NOTES:
        /// - niHSDIO_AllocateNamedWaveform sets aside onboard memory space and associates a string name with that space. The name given to the waveform 
	/// is the same name used in the write named waveform functions, as well as the name used in scripts.
        /// 
        /// - If not enough space is available to accommodate a waveform of size sizeInSamples, an error is returned and no memory space is created.
        /// 
        /// - This function does not change any data on the device itself, but rather adds the named reference in software only. Use the write named 
	/// waveform functions to fill the onboard memory with waveform data to be generated.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="Size_In_Samples">
        /// Specifies the number of samples to allocate for the named waveform.
        /// 
        /// The number of bits in the allocated samples differs depending on the device you are using. Refer to your device documentation for more 
	/// information.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning

	///         /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// </returns>
        public int AllocateNamedWaveform(string Waveform_Name, int Size_In_Samples)
        {
            int pInvokeResult = PInvoke.AllocateNamedWaveform(this._handle, Waveform_Name, Size_In_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Modifies where within a named waveform to next write data. Write Named Waveform functions always begins writing at the current write 
	/// position.  Existing data in the waveform is overwritten.
        /// 
        /// The "position" and "offset" parameters are used together to determine where the next write position will be. The "position" parameter 
	/// describes an absolute (NIHSDIO_VAL_START_OF_WAVEFORM) or relative (NIHSDIO_VAL_CURRENT_READ_POSITION) move.  The "offset" is the number of 
	/// samples to shift the next write position. You must always set the write position at a position that is a multiple of 32 samples for the NI 
	/// 654x/655x devices or a multiple of 64 samples for the NI 656x devices (128 samples if the NI 656x is in DDR mode).
        /// 
        /// Examples of combinations of "position" and "offset"
        /// -Position: NIHSDIO_VAL_START_OF_WAVEFORM
        /// -Offset: 0
        /// -Effect: Write location becomes the start of waveform
        /// 
        /// -Position: NIHSDIO_VAL_START_OF_WAVEFORM
        /// -Offset: 5
        /// -Effect: Write location becomes the sixth sample in waveform
        /// 
        /// -Position: NIHSDIO_VAL_START_OF_WAVEFORM
        /// -Offset: -1
        /// -Effect: ERROR-- The device would try to place the write position before start of waveform.
        /// 
        /// -Position: NIHSDIO_VAL_CURRENT_READ_POSITION
        /// -Offset: 0
        /// -Effect: No effect - leaves next write position unchanged
        /// 
        /// -Position: NIHSDIO_VAL_CURRENT_POSITION
        /// -Offset: 10
        /// -Effect: Shift write position 10 samples ahead from current write location. This position setting is only valid if the current write 
	/// position plus this offset is in the waveform. 
        /// 
        /// -Position: NIHSDIO_VAL_CURRENT_POSITION
        /// -Offset: -10
        /// -Effect: Shift write position 10 samples back from current location. This position setting is only valid if the current write position is 
	/// greater than 10.
        /// 
        /// The write position is moved to the end of the most recently written data after each call to a Write Named Waveform function.  Thus you do 
	/// not need to explicitly call niHSDIO_SetNamedWaveformNextWritePosition.
        /// 
        /// Attempting to set the write position past the end of the allocated space results in an error.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies a string representing the name to associate with the allocated waveform memory.
        /// </param>
        /// <param name="Position">
        /// Specifies where to place the write position (in conjunction with offset):
        /// 
        /// Defined Values:
        /// 
        /// -   NIHSDIO_VAL_START_OF_WAVEFORM - Offset is relative to the beginning of the waveform.
        /// -   NIHSDIO_VAL_CURRENT_POSITION - Offset is relative to the current write position in the waveform.
        /// 
        /// </param>
        /// <param name="Offset">
        /// Specifies the write position of the name waveform in conjunction with the mode attribute. Offset is in samples.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int SetNamedWaveformNextWritePosition(string Waveform_Name, int Position, int Offset)
        {
            int pInvokeResult = PInvoke.SetNamedWaveformNextWritePosition(this._handle, Waveform_Name, Position, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Frees a named waveform space in onboard memory.
        /// 
        /// NOTE: This function releases onboard memory space previously allocated by either the niHSDIO_AllocateNamedWaveform 
	/// or niHSDIO_WriteNamedWaveform functions. Any future reference to the deleted waveform results in an error. However, previously written 
	/// scripts that still reference the deleted waveform do not generate an error at initiation.
        /// 
        /// An error is generated if the waveform name is not allocated in onboard memory.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Waveform_Name">
        /// Specifies the name of the waveform to delete.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int DeleteNamedWaveform(string Waveform_Name)
        {
            int pInvokeResult = PInvoke.DeleteNamedWaveform(this._handle, Waveform_Name);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures whether to generate the waveform specified in NIHSDIO_ATTR_WAVEFORM_TO_GENERATE or the script specified in 
	/// NIHSDIO_ATTR_SCRIPT_TO_GENERATE upon calling niHSDIO_Initiate.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Generation_Mode">
        /// Specifies the generation mode to configure.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_WAVEFORM - Calling niHSDIO_Initiate generates the named waveform represented by NIHSDIO_ATTR_WAVEFORM_TO_GENERATE.
        /// - NIHSDIO_VAL_SCRIPTED - Calling niHSDIO_Initiate generates the script represented by NIHSDIO_ATTR_SCRIPT_TO_GENERATE.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Writes a string containing scripts that governs the generation of waveforms. If this function is called repeatedly, previously written 
	/// scripts with unique names remain loaded. Previously written scripts with identical names to those being written are replaced.
        /// 
        /// If multiple scripts are loaded when niHSDIO_Initiate is called, then one of the scripts must be designated as the script to generate by 
	/// setting NIHSDIO_ATTR_SCRIPT_TO_GENERATE to the desired script name. If only one script is in memory, then there is no need to designate the 
	/// script to generate. All waveforms referenced in the scripts must be written before the script is written.
        /// 
        /// An error is returned if the script uses incorrect syntax. This function calls niHSDIO_CommitDynamic. All pending attributes are committed to 
	/// hardware.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Script">
        /// Specifies a string containing a syntactically correct script.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Sets the script to be generated upon a call to the niHSDIO_Initiate function when NIHSDIO_ATTR_GENERATION_MODE is set to 
	/// NIHSDIO_VAL_SCRIPTED. If there are multiple scripts loaded when niHSDIO_Initiate is called, one script must be designated as the script to 
	/// generate or NI-HSDIO returns an error. You only need to call this function if multiple scripts are present in onboard memory (refer to 
	/// NIHSDIO_ATTR_SCRIPT_TO_GENERATE for more information).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Script_Name">
        /// Specifies which script to generate after calling niHSDIO_Initiate.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureScriptToGenerate(string Script_Name)
        {
            int pInvokeResult = PInvoke.ConfigureScriptToGenerate(this._handle, Script_Name);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Sample clock. This function allows you to specify the clock source and rate for the Sample clock.
        /// 
        /// If Clock_Source is set to NIHSDIO_VAL_ON_BOARD_CLOCK_STR, NI-HSDIO coerces the rate to a value that is supported by the hardware. Use 
	/// niHSDIO_GetAttributeViReal64 to get the value for NIHSDIO_ATTR_SAMPLE_CLOCK_RATE to see to what value NI-HSDIO has coerced the Sample clock 
	/// rate.
        /// 
        /// Clock_Source can be set to NIHSDIO_VAL_STROBE_STR for acquisition only.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Clock_Source">
        /// Specifies the Sample clock source. Refer to NIHSDIO_ATTR_SAMPLE_CLOCK_SOURCE for details.
        /// 
        /// Defined Values:
        /// 
        /// -NIHSDIO_VAL_ON_BOARD_CLOCK_STR - The device uses the On Board Clock as the Sample clock source.
        /// -NIHSDIO_VAL_STROBE_STR - The device uses the signal present on the STROBE channel as the Sample clock source. This choice is only valid for 
	/// acquisition operations.
        /// -NIHSDIO_VAL_CLK_IN_STR - The device uses the signal present on the front panel CLK IN SMB jack connector as the Sample clock source.
        /// -NIHSDIO_VAL_PXI_STAR_STR - The device uses the signal present on the PXI_STAR line as the Sample clock source. This choice is valid for 
	/// devices in slots that support PXI_STAR.
        /// </param>
        /// <param name="Clock_Rate">
        /// Specifies the Sample clock rate, expressed in Hz. You must set this property even when you supply an external clock because NI-HSDIO uses 
	/// this property for a number of reasons, including optimal error checking and certain pulse width selections.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureSampleClock(string Clock_Source, double Clock_Rate)
        {
            int pInvokeResult = PInvoke.ConfigureSampleClock(this._handle, Clock_Source, Clock_Rate);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets up channels to be clocked in various ways by the sample clock edges. You have three options for data position: rising edge, falling 
	/// edge, or delayed.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// Identifies channels to apply settings. Use "" or VI_NULL to specify all channels.
        /// </param>
        /// <param name="Position">
        /// Specifies which edge of the Sample clock signal is used to time the operation. You can also configure the device to generate data at a 
	/// configurable delay past each rising edge of the Sample clock.
        /// 
        /// Defined Values
        /// 
        /// NIHSDIO_VAL_SAMPLE_CLOCK_RISING_EDGE - The device samples or generates data on the rising edge of the Sample clock.
        /// NIHSDIO_VAL_SAMPLE_CLOCK_FALLING_EDGE - The device samples or generates data on the falling edge of the Sample clock.
        /// NIHSDIO_VAL_DELAY_FROM_SAMPLE_CLOCK_RISING_EDGE - The device samples or generates data with delay from rising edge of the Sample clock. 
	/// Specify the delay using NIHSDIO_ATTR_DATA_POSITION_DELAY.
        /// 
        /// NOTES:
        /// 
        /// NIHSDIO_VAL_DELAY_FROM_SAMPLE_CLOCK_RISING_EDGE has more jitter than the rising or falling edge values.
        /// 
        /// Certain devices have sample clock frequency limitations when a custom delay is used. Refer to the device documentation for details.
        /// 
        /// To configure a delay on NI 656x devices,you must delay all channels on the device. NI-HSDIO returns an error if you apply a delay to only a 
	/// partial channel list.
        /// 
        /// Default Value: NIHSDIO_VAL_SAMPLE_CLOCK_RISING_EDGE 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDataPosition(string Channel_List, int Position)
        {
            int pInvokeResult = PInvoke.ConfigureDataPosition(this._handle, Channel_List, Position);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets up the data delay with respect to the Sample clock.
        /// 
        /// NOTE:  To configure a delay on NI 656x devices, you must delay all channels on the device. NI-HSDIO returns an error if you apply a delay to 
	/// only a partial channel list.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// Identifies channels to apply settings. Use "" or VI_NULL to specify all channels.
        /// </param>
        /// <param name="Delay">
        /// Specifies the delay after the Sample clock rising edge when the device generates or acquires a new data sample. Data delay is expressed as a 
	/// fraction of the clock period, that is, a fraction of 1/NIHSDIO_ATTR_SAMPLE_CLOCK_RATE. All the channels in the session that use delayed 
	/// Sample clock to position data must have the same delay value.
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDataPositionDelay(string Channel_List, double Delay)
        {
            int pInvokeResult = PInvoke.ConfigureDataPositionDelay(this._handle, Channel_List, Delay);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Reference clock. Use this function when you are using the On Board Clock as a Sample clock, and you want the Sample clock to 
	/// be phase-locked to a Reference signal. Phase-locking the Sample clock to a Reference clock prevents the Sample clock from "drifting" 
	/// relative to the Reference clock.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Clock_Source">
        /// Specifies the PLL reference clock source. Refer to NIHSDIO_ATTR_REF_CLOCK_SOURCE for details.
        /// 
        /// Defined Values:
        /// 
        /// -NIHSDIO_VAL_NONE_STR - The device will not use a Reference clock.
        /// -NIHSDIO_VAL_CLK_IN_STR - The device uses the signal present on the front panel CLK IN SMB jack connector as the Reference clock source.
        /// -NIHSDIO_VAL_PXI_CLK10_STR - The device uses the 10 MHz PXI backplane clock as the Reference clock source. This source is only available for 
	/// PXI devices.
        /// -NIHSDIO_VAL_RTSI7_STR - The device uses the signal on RTSI 7 as the Reference clock source. This source is only available for PCI devices.
        /// </param>
        /// <param name="Clock_Rate">
        /// Specifies the Reference clock rate, expressed in Hz.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_REF_CLOCK_RATE for details.
        /// 
        /// Default Value: 10000000
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureRefClock(string Clock_Source, double Clock_Rate)
        {
            int pInvokeResult = PInvoke.ConfigureRefClock(this._handle, Clock_Source, Clock_Rate);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Delays the Sample clock relative to the Reference clock. Use this function to align the Sample clock of your device to the Sample clock of 
	/// another device in your system. 
        /// 
        /// Only call this function after your session is committed. The effect of this function is immediate.
        /// 
        /// This function generates an error if NIHSDIO_ATTR_REF_CLOCK_SOURCE is set to NIHSDIO_VAL_NONE_STR.
        /// 
        /// This function can only align the device Sample clock to another Sample clock if the other device is using the same reference clock source.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <param name="Adjustment_Time">
        /// Specifies the time in seconds to delay the Sample clock. Values range between 0 and the Sample clock period 
	/// (1/NIHSDIO_ATTR_SAMPLE_CLOCK_RATE).
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int AdjustSampleClockRelativeDelay(double Adjustment_Time)
        {
            int pInvokeResult = PInvoke.AdjustSampleClockRelativeDelay(this._handle, Adjustment_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Start trigger for edge triggering.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Source">
        /// You may specify any valid source terminal for this trigger. Trigger voltages and positions are only relevant if the source of the trigger is 
	/// from the front panel connectors.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_DIGITAL_EDGE_START_TRIGGER_SOURCE for possible values.
        /// </param>
        /// <param name="Edge">
        /// Specifies the edge to detect:
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_RISING_EDGE - rising edge trigger
        /// - NIHSDIO_VAL_FALLING_EDGE - falling edge trigger
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Configures the start trigger for pattern match triggering. This function is only valid for acquisition operations.
        /// 
        /// Valid Syntax:
        /// 
        /// Both of these examples are valid and do the same thing. The order of channelList determines the order of the pattern string.
        /// 
        /// - niHSDIO_ConfigurePatternMatchStartTrigger (vi, "19-0", "0000 0XXX XX11 1111 1111");
        /// 
        /// - niHSDIO_ConfigurePatternMatchStartTrigger (vi, "0-19", "1111 1111 11XX XXX0 0000");
        /// 
        /// NOTE: The logic levels seen by pattern matching are affected by data interpretation.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels will be configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string.
        /// </param>
        /// <param name="Pattern">
        /// This string expression describes the pattern to be matched. This expression is composed of characters:
        /// 
        /// - 'X' or 'x': ignore the channel
        /// - '1' : match on a logic 1
        /// - '0': match on a logic 0
        /// - 'R' or 'r': match on a rising edge
        /// - 'F' or 'f': match on a falling edge
        /// - 'E' or 'e': match on either edge
        /// 
        /// The first character in the expression corresponds to the first channel in Channel_List. The number of characters in pattern must correspond 
	/// to the number of channels specified in Channel_List.
        /// 
        /// 
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts:
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchStartTrigger(string Channel_List, string Pattern, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchStartTrigger(this._handle, Channel_List, Pattern, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Start trigger for pattern match triggering.
        /// 
        /// Unlike niHSDIO_ConfigurePatternMatchStartTrigger which uses a string, this function uses a binary format to only represent high and low. If 
	/// you require more choices for your pattern, use the niHSDIO_ConfigurePatternMatchStartTrigger function.
        /// 
        /// This function is only valid for acquisition operations.
        /// 
        /// NOTE: The logic levels seen by pattern matching are affected by data interpretation.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels will be configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string.
        /// 
        /// </param>
        /// <param name="Pattern">
        /// Specifies the binary pattern that activates the pattern match trigger under the conditions specified in Trigger_When.
        /// 
        /// Bits on channels not specified in Channel_List are ignored.
        /// 
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts:
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchStartTriggerU32(string Channel_List, uint Pattern, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchStartTriggerU32(this._handle, Channel_List, Pattern, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Start trigger for software triggering.
        /// 
        /// 
        /// Refer to niHSDIO_SendSoftwareEdgeTrigger for more information on using the software Start trigger.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Configures the device to not wait for a Start trigger after the niHSDIO_Initiate function is called. Calling this function is only necessary 
	/// if you have configured a Start trigger and now want to disable it.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Configures the Reference trigger for edge triggering in an acquisition. If the Reference trigger asserts before all of the pretrigger 
	/// samples are acquired, then it is ignored. This function is only valid for acquisition operations.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Source">
        /// You may specify any valid source terminal for this trigger. Trigger voltages and positions are only relevant if the source of the trigger is 
	/// from the front panel connectors.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_DIGITAL_EDGE_REF_TRIGGER_SOURCE for possible values.
        /// </param>
        /// <param name="Edge">
        /// Specifies the edge to detect.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_RISING_EDGE - rising edge trigger
        /// - NIHSDIO_VAL_FALLING_EDGE - falling edge trigger
        /// 
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// Specifies the number of necessary pretrigger samples before the Reference trigger is acknowledged.
        /// 
        /// Default Value: 500
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDigitalEdgeRefTrigger(string Source, int Edge, int Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeRefTrigger(this._handle, Source, Edge, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Reference trigger for pattern match triggering. If the reference trigger asserts before all of the pretrigger samples are 
	/// acquired, then it is ignored. This function is only valid for acquisition sessions.
        /// 
        /// Valid Syntax:
        /// 
        /// Both of these examples are valid and do the same thing. The order of channelList determines the order of the pattern string.
        /// 
        /// - niHSDIO_ConfigurePatternMatchRefTrigger (vi, "19-0", "0000 0XXX XX11 111Z ZZZZ");
        /// 
        /// - niHSDIO_ConfigurePatternMatchRefTrigger (vi, "0-19", "ZZZZ Z111 11XX XXX0 0000");
        /// 
        /// NOTE: The logic levels seen by pattern matching are affected by data interpretation.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels are configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string.
        /// </param>
        /// <param name="Pattern">
        /// This string expression describes the pattern to be matched. This expression is composed of characters:
        /// 
        /// - 'X' or 'x': ignore the channel
        /// - '1': match on a logic 1
        /// - '0': match on a logic 0
        /// - 'R' or 'r': match on a rising edge
        /// - 'F' or 'f': match on a falling edge
        /// - 'E' or 'e': match on either edge
        /// 
        /// The leftmost character in the expression corresponds to the first channel in channelList. The number of characters in pattern must 
	/// correspond to the number of channels specified in channelList or an error is returned.
        /// 
        /// 
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// Specifies the number of necessary pretrigger samples before the reference trigger is acknowledged.
        /// 
        /// Default Value: 500
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchRefTrigger(string Channel_List, string Pattern, int Trigger_When, int Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchRefTrigger(this._handle, Channel_List, Pattern, Trigger_When, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Reference trigger for pattern match triggering. If the reference trigger asserts before all of the pretrigger samples are 
	/// acquired, then it is ignored.
        /// 
        /// Unlike niHSDIO_ConfigurePatternMatchRefTrigger which uses a string, this function uses a binary format to only represent high and low. If 
	/// you require more choices for your pattern, use the niHSDIO_ConfigurePatternMatchRefTrigger function.
        /// 
        /// This function is only valid for acquisition sessions.
        /// 
        /// NOTE: The logic levels seen by pattern matching are affected by data interpretation.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels are configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string.
        /// </param>
        /// <param name="Pattern">
        /// Specifies the binary pattern that activates the pattern match trigger under the conditions specified in Trigger_When.
        /// 
        /// Bits on channels not specified in Channel_List are ignored.
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// Specifies the number of necessary pretrigger samples before the reference trigger is acknowledged.
        /// 
        /// Default Value: 500
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchRefTriggerU32(string Channel_List, uint Pattern, int Trigger_When, int Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchRefTriggerU32(this._handle, Channel_List, Pattern, Trigger_When, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the reference trigger for software triggering. If the reference trigger asserts before all of the pretrigger samples are 
	/// acquired, then it is ignored. This function is valid only for acquisition sessions.
        /// 
        /// Refer to niHSDIO_SendSoftwareEdgeTrigger for more information on the software reference trigger.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// Specifies the number of necessary pretrigger samples before the reference trigger is acknowledged.
        /// 
        /// Default Value: 500
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureSoftwareRefTrigger(int Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareRefTrigger(this._handle, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the acquisition operation to have no Reference trigger. Calling this function is only necessary if you have configured a 
	/// Reference trigger and now want to disable it. This function is valid only for acquisition sessions.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int DisableRefTrigger()
        {
            int pInvokeResult = PInvoke.DisableRefTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Advance trigger for edge triggering in an acquisition.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Source">
        /// You may specify any valid source terminal for this trigger. Trigger voltages and positions are only relevant if the source of the trigger is 
	/// from the front panel connectors.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_DIGITAL_EDGE_ADVANCE_TRIGGER_SOURCE for possible values.
        /// </param>
        /// <param name="Edge">
        /// Specifies the edge to detect.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_RISING_EDGE - rising edge trigger
        /// - NIHSDIO_VAL_FALLING_EDGE - falling edge trigger
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDigitalEdgeAdvanceTrigger(string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeAdvanceTrigger(this._handle, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Advance trigger for pattern match triggering. This function is only valid for acquisition operations.
        /// 
        /// Valid Syntax:
        /// 
        /// Both of these examples are valid and do the same thing. The order of channelList determines the order of the pattern string.
        /// 
        /// - niHSDIO_ConfigurePatternMatchAdvanceTrigger (vi, "19-0", "0000 0XXX XX11 1111 1111");
        /// 
        /// - niHSDIO_ConfigurePatternMatchAdvanceTrigger (vi, "0-19", "1111 1111 11XX XXX0 0000");
        /// 
        /// NOTE: The logic levels seen by pattern matching are affected by data interpretation.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels will be configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string.
        /// </param>
        /// <param name="Pattern">
        /// This string expression describes the pattern to be matched. This expression is composed of characters:
        /// 
        /// - 'X' or 'x': ignore the channel
        /// - '1' : match on a logic 1
        /// - '0': match on a logic 0
        /// - 'R' or 'r': match on a rising edge
        /// - 'F' or 'f': match on a falling edge
        /// - 'E' or 'e': match on either edge
        /// 
        /// The first character in the expression corresponds to the first channel in Channel_List. The number of characters in pattern must correspond 
	/// to the number of channels specified in Channel_List.
        /// 
        /// 
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts:
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchAdvanceTrigger(string Channel_List, string Pattern, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchAdvanceTrigger(this._handle, Channel_List, Pattern, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Advance trigger for pattern match triggering. This function is only valid for acquisition operations.
        /// 
        /// Unlike niHSDIO_ConfigurePatternMatchAdvanceTrigger which uses a string, this function uses a binary format to only represent high and low. 
	/// If you require more choices for your pattern, use the niHSDIO_ConfigurePatternMatchAdvanceTrigger function.
        /// 
        /// 
        /// NOTE: The logic levels seen by pattern matching are affected by data interpretation.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels will be configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string.
        /// </param>
        /// <param name="Pattern">
        /// Specifies the binary pattern that activates the pattern match trigger under the conditions specified in Trigger_When.
        /// 
        /// Bits on channels not specified in Channel_List are ignored. 
        /// 
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts:
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchAdvanceTriggerU32(string Channel_List, uint Pattern, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchAdvanceTriggerU32(this._handle, Channel_List, Pattern, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Advance trigger for software triggering.
        /// 
        /// 
        /// Refer to niHSDIO_SendSoftwareEdgeTrigger for more information on using the software Advance trigger.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureSoftwareAdvanceTrigger()
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareAdvanceTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the acquisition operation to have no Advance trigger. Calling this function is only necessary if you have configured a Advance 
	/// trigger and now want to disable it. This function is valid only for acquisition sessions.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int DisableAdvanceTrigger()
        {
            int pInvokeResult = PInvoke.DisableAdvanceTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Script trigger for edge triggering. This function is only valid for generation sessions that use scripting.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Trigger_ID">
        /// Identifies which Script trigger this function configures.
        /// 
        /// Defined Values:
        /// 
        /// NIHSDIO_VAL_SCRIPT_TRIGGER0
        /// NIHSDIO_VAL_SCRIPT_TRIGGER1
        /// NIHSDIO_VAL_SCRIPT_TRIGGER2
        /// NIHSDIO_VAL_SCRIPT_TRIGGER3
        /// </param>
        /// <param name="Source">
        /// You may specify any valid source terminal for this trigger. Trigger voltages and positions are only relevant if the source of the trigger is 
	/// from the front panel connectors.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_DIGITAL_EDGE_SCRIPT_TRIGGER_SOURCE for possible values.
        /// </param>
        /// <param name="Edge">
        /// Specifies the edge to detect.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_RISING_EDGE - rising edge trigger
        /// - NIHSDIO_VAL_FALLING_EDGE - falling edge trigger
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDigitalEdgeScriptTrigger(string Trigger_ID, string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeScriptTrigger(this._handle, Trigger_ID, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Script trigger for level triggering. This function is only valid for generation sessions that use scripting.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Trigger_ID">
        /// Identifies which script trigger this function configures.
        /// 
        /// Defined Values:
        /// 
        /// NIHSDIO_VAL_SCRIPT_TRIGGER0
        /// NIHSDIO_VAL_SCRIPT_TRIGGER1
        /// NIHSDIO_VAL_SCRIPT_TRIGGER2
        /// NIHSDIO_VAL_SCRIPT_TRIGGER3
        /// </param>
        /// <param name="Source">
        /// You may specify any valid source terminal for this trigger. Trigger voltages and positions are only relevant if the source of the trigger is 
	/// from the front panel connectors.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_DIGITAL_LEVEL_SCRIPT_TRIGGER_SOURCE for possible values.
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the active level for the desired trigger.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_HIGH - trigger is active while its source is high
        /// 
        /// - NIHSDIO_VAL_LOW - trigger is active while its source is low
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDigitalLevelScriptTrigger(string Trigger_ID, string Source, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalLevelScriptTrigger(this._handle, Trigger_ID, Source, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the Script trigger for software triggering. This function is only valid for generation sessions that use scripting.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Trigger_ID">
        /// Identifies which script trigger this function configures.
        /// 
        /// Defined Values:
        /// 
        /// NIHSDIO_VAL_SCRIPT_TRIGGER0
        /// NIHSDIO_VAL_SCRIPT_TRIGGER1
        /// NIHSDIO_VAL_SCRIPT_TRIGGER2
        /// NIHSDIO_VAL_SCRIPT_TRIGGER3
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureSoftwareScriptTrigger(string Trigger_ID)
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareScriptTrigger(this._handle, Trigger_ID);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the data operation to not have a Script trigger. Calling this function is only necessary if you  have configured a particular Script 
	/// trigger and now want to disable it. This function is only valid for generation sessions.
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Trigger_ID">
        /// Identifies which Script trigger this function will configure.
        /// 
        /// Defined Values:
        /// 
        /// NIHSDIO_VAL_SCRIPT_TRIGGER0
        /// NIHSDIO_VAL_SCRIPT_TRIGGER1
        /// NIHSDIO_VAL_SCRIPT_TRIGGER2
        /// NIHSDIO_VAL_SCRIPT_TRIGGER3
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int DisableScriptTrigger(string Trigger_ID)
        {
            int pInvokeResult = PInvoke.DisableScriptTrigger(this._handle, Trigger_ID);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the pause trigger for level triggering.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Source">
        /// You may specify any valid source terminal for this trigger. Trigger voltages and positions are only relevant if the source of the trigger is 
	/// from the front panel connectors.
        /// 
        /// Defined Values:
        /// 
        /// Refer to NIHSDIO_ATTR_DIGITAL_LEVEL_PAUSE_TRIGGER_SOURCE for possible values.
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the active level for the desired trigger.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_HIGH - trigger is active while its source is high
        /// - NIHSDIO_VAL_LOW - trigger is active while its source is low
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigureDigitalLevelPauseTrigger(string Source, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalLevelPauseTrigger(this._handle, Source, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the pause trigger for pattern match triggering. This function is valid only for acquisition sessions.
        /// 
        /// Valid Syntax:
        /// 
        /// Both of these examples are valid and do the same thing. The order of channelList determines the order of the pattern string.
        /// 
        /// - niHSDIO_ConfigurePatternMatchPauseTrigger (vi, "19-0", "0000 0XXX XX11 1111 1111");
        /// 
        /// - niHSDIO_ConfigurePatternMatchPauseTrigger (vi, "0-19", "1111 1111 11XX XXX0 0000");
        /// 
        /// NOTE: The values seen by pattern matching is affected by data interpretation.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels are configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string. Ex. "0-19" and "19-0" are reverse of one another.
        /// </param>
        /// <param name="Pattern">
        /// This string expression describes the pattern to be matched. This expression is composed of characters:
        /// 
        /// - 'X' or 'x': ignore the channel
        /// - '1': match on a logic 1
        /// - '0': match on a logic 0
        /// - 'R' or 'r': match on a rising edge
        /// - 'F' or 'f': match on a falling edge
        /// - 'E' or 'e': match on either edge
        /// 
        /// The leftmost character in the expression corresponds to the first channel in channelList. The number of characters in pattern must correspond 
	/// to the number of channels specified in channelList or an error is returned.
        /// 
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchPauseTrigger(string Channel_List, string Pattern, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchPauseTrigger(this._handle, Channel_List, Pattern, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configures the pause trigger for pattern match triggering. This function is valid only for acquisition sessions.
        /// 
        /// Unlike niHSDIO_ConfigurePatternMatchPauseTrigger which uses a string, this function uses a binary format to only represent high and low. If 
	/// you require more choices for your pattern, use the niHSDIO_ConfigurePatternMatchPauseTrigger function.
        /// 
        /// NOTE: The values seen by pattern matching is affected by data interpretation.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string specifies which channels are configured for pattern matching using the pattern string. The order of channels in the list 
	/// determines the order of the pattern string. Ex. "0-19" and "19-0" are reverse of one another.
        /// </param>
        /// <param name="Pattern">
        /// Specifies the binary pattern that activates the pattern match trigger under the conditions specified in Trigger_When.
        /// 
        /// Bits on channels not specified in Channel_List are ignored.
        /// </param>
        /// <param name="Trigger_When">
        /// Specifies the when the trigger asserts.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PATTERN_MATCHES - the trigger activates when the pattern matches
        /// 
        /// - NIHSDIO_VAL_PATTERN_DOES_NOT_MATCH - the trigger activates when the pattern does not match
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ConfigurePatternMatchPauseTriggerU32(string Channel_List, uint Pattern, int Trigger_When)
        {
            int pInvokeResult = PInvoke.ConfigurePatternMatchPauseTriggerU32(this._handle, Channel_List, Pattern, Trigger_When);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Sets the data operation to have no Pause trigger. Calling this function is only necessary if you have configured a Pause trigger and now 
	/// want to disable it.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int DisablePauseTrigger()
        {
            int pInvokeResult = PInvoke.DisablePauseTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Use this function to force a particular edge-based trigger to occur. This function applies to the Start, Reference, Advance, and Script 
	/// triggers, and is valid if the particular trigger is configured for edge, pattern match, or software triggering (for edge or pattern match 
	/// triggers you can use niHSDIO_SendSoftwareEdgeTrigger as a software override).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Trigger">
        /// The trigger to assert. 
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_START_TRIGGER - Start trigger for dynamic acquisition or generation
        /// 
        /// - NIHSDIO_VAL_REF_TRIGGER - Reference trigger for dynamic acqusition
        /// 
        /// - NIHSDIO_VAL_SCRIPT_TRIGGER - Script trigger for dynamic generation
        /// 
        /// 
        /// </param>
        /// <param name="Trigger_Identifier">
        /// Describes the software trigger. For example, NIHSDIO_VAL_SCRIPT_TRIGGER0 could be the identifier for the Script trigger, or you could have 
	/// an empty string for the Start and Reference triggers. 
        /// 
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER0
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER1
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER2
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER3
        ///  - "" (empty string) or VI_NULL
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Use this function to route signals (clocks, triggers, and events) to the output terminal you specify. Refer to your device documentation for 
	/// valid signal destinations.
        /// 
        /// Any routes created within a session persist after the session closes to prevent signal glitching. To unconfigure signal routes created in 
	/// previous sessions, set the Reset_Instrument parameter in niHSDIO_InitGenerationSession or niHSDIO_InitAcquisitionSession to VI_TRUE or use 
	/// niHSDIO_reset.
        /// 
        /// If you export a signal with this function and commit the session, the signal is routed to the output terminal you specify. If you then 
	/// reconfigure the signal to have a different output terminal, the previous output terminal is tristated after the session is committed. If you 
	/// change the output terminal to NIHSDIO_VAL_DO_NOT_EXPORT_STR or an empty string when you commit the operation, the previous output terminal 
	/// is tristated.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Signal">
        /// Signal (clock, trigger, or event) to export.
        /// 
        /// Defined Values:
        /// 
        /// -   NIHSDIO_VAL_SAMPLE_CLOCK
        /// -   NIHSDIO_VAL_REF_CLOCK
        /// -   NIHSDIO_VAL_START_TRIGGER
        /// -   NIHSDIO_VAL_REF_TRIGGER (dynamic acquisition only)
        /// -   NIHSDIO_VAL_ADVANCE_TRIGGER (dynamic acquisition only)
        /// -   NIHSDIO_VAL_DATA_ACTIVE_EVENT (dynamic generation only)
        /// -   NIHSDIO_VAL_READY_FOR_START_EVENT
        /// -   NIHSDIO_VAL_READY_FOR_ADVANCE_EVENT (dynamic acquisition only)
        /// -   NIHSDIO_VAL_END_OF_RECORD_EVENT (dynamic acquisition only)
        /// -   NIHSDIO_VAL_PAUSE_TRIGGER (dynamic generation only)
        /// -   NIHSDIO_VAL_SCRIPT_TRIGGER (dynamic generation only - requires Signal_Identifier to describe a particular Script trigger)
        /// -   NIHSDIO_VAL_MARKER_EVENT (dynamic generation only - requires Signal_Identifier to describe a particular Marker)
        /// -   NIHSDIO_VAL_ONBOARD_REF_CLOCK PCI devices only) - Calling this function with Signal set to this value sets the 
        /// NIHSDIO_ATTR_EXPORTED_ONBOARD_REF_CLOCK_OUTPUT_TERMINAL attribute
        /// 
        /// </param>
        /// <param name="Signal_Identifier">
        /// Describes the signal being exported.
        /// 
        /// Defined Values:
        /// 
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER0
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER1
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER2
        ///  - NIHSDIO_VAL_SCRIPT_TRIGGER3
        ///  - NIHSDIO_VAL_MARKER_EVENT0
        ///  - NIHSDIO_VAL_MARKER_EVENT1
        ///  - NIHSDIO_VAL_MARKER_EVENT2
        ///  - NIHSDIO_VAL_MARKER_EVENT3
        ///  - "" (empty String) or VI_NULL
        /// 
        /// </param>
        /// <param name="Output_Terminal">
        /// Output terminal where the signal is exported.
        /// 
        /// Defined Values:
        /// 
        /// - NIHSDIO_VAL_PFI0_STR - NIHSDIO_VAL_PFI3_STR : PFI connectors
        /// - NIHSDIO_VAL_PXI_TRIG0_STR - NIHSDIO_VAL_PXI_TRIG7_STR : the PXI trigger backplane
        /// - NIHSDIO_VAL_CLK_OUT_STR - CLK OUT coaxial connector on the front panel
        /// - NIHSDIO_VAL_DDC_CLK_OUT_STR - DDC CLK OUT terminal in the Digital Data & Control Connector
        /// - "" (empty string) or VI_NULL - the signal is not exported
        /// 
        /// Trigger and event voltages and positions are only relevant if the destination of the event is one of the front panel connectors.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Use this function to configure channels for static acquisition (if vi is an acquisition session) or static generation (if vi is a generation 
	/// session). A channel cannot be simultaneously assigned to a static generation and dynamic generation.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string identifies which channels will be configured as static.
        /// 
        /// Valid Syntax: "0-19" or "0-15,16-19" or "0-18,19"
        /// 
        /// Special values:
        /// -   "" (empty string) - configure ALL channels for static
        /// -   "None" - unconfigure all static channels
        /// 
        /// Channels cannot be configured for both static generation and dynamic generation.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int AssignStaticChannels(string Channel_List)
        {
            int pInvokeResult = PInvoke.AssignStaticChannels(this._handle, Channel_List);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function immediately reads the digital value on channels configured for static acquisition. Configure a channel for static acquisition 
	/// using the niHSDIO_AssignStaticChannels function. Channels not configured for static acquisition return a zero.
        /// 
        /// Values obtained from static read operations are affected by data interpretation. 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession.
        /// </param>
        /// <param name="Read_Data">
        /// Bit-value of data read from channels configured for static acquisition.
        /// 
        /// The least significant bit of readData corresponds to the lowest physical channel number (for example, readData of 0x00F0 means channels 4-7 
	/// are logic one, while the remaining channels are logic zero or are not configured for static acquisition).
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int ReadStaticU32(out uint Read_Data)
        {
            int pInvokeResult = PInvoke.ReadStaticU32(this._handle, out Read_Data);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function writes to channels configured for static generation. You can configure a channel for static generation using the 
	/// niHSDIO_AssignStaticChannels function.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Write_Data">
        /// Bit-value of data to drive on channels configured for static generation. 1 corresponds to logic high, 0 corresponds to logic low. 
        /// 
        /// The least significant bit of writeData corresponds to the lowest physical channel number (for example, writeData of 0xFF00 means set the 
	/// lower 8 channels to 0, while setting the upper 8 channels to logic high.
        /// 
        /// Data values in writeData corresponding to channels not configured for static generation are ignored. 
        /// 
        /// Static channels explicitly disabled with the niHSDIO_TristateChannels function remain disabled, but the channel data value changes 
	/// internally.  Re-enabling a channel with niHSDIO_TristateChannels causes the channel to drive any value that you have written to it, even 
	/// while the channel was disabled.
        /// 
        /// </param>
        /// <param name="Channel_Mask">
        /// Bit-value of channels to leave unchanged. 1 means to change the channel to whatever is reflected by writeData. 0 means do not alter the 
	/// channel, regardless of writeData.
        /// 
        /// The least significant bit of channelMask corresponds to the lowest physical channel number (e.g. writeData of 0xFFFF and channelMask of 
	/// 0x0080 means set only channel 7 to 1; all other channels remain unchanged).
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int WriteStaticU32(uint Write_Data, uint Channel_Mask)
        {
            int pInvokeResult = PInvoke.WriteStaticU32(this._handle, Write_Data, Channel_Mask);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function self-calibrates the device. During self-calibration, the VCXO oscillator phase D/A converters are recalibrated.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Changes the password that is required to initialize an external calibration session. The password may be up to four characters long.
        /// 
        /// You can call this function from an acquisition, generation, or calibration session.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// </param>
        /// <param name="Old_Password">
        /// The old (current) external calibration password. 
        /// </param>
        /// <param name="New_Password">
        /// The new (desired) external calibration password. 
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int ChangeExtCalPassword(string Old_Password, string New_Password)
        {
            int pInvokeResult = PInvoke.ChangeExtCalPassword(this._handle, Old_Password, New_Password);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Adjusts the voltage of the selected channel(s). The only errors that can be returned are actual calibration process errors. 
        /// 
        /// NOTES: This function is not supported for the NI 654X/656X devices.
        /// 
        /// This function runs a static loopback test before adjusting the voltage. You must disconnect the cable from your device to run this function. 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// </param>
        /// <param name="Channel">
        /// Identifies channels on which voltage will be adjusted. 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int CalAdjustChannelVoltage(string Channel)
        {
            int pInvokeResult = PInvoke.CalAdjustChannelVoltage(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Closes an NI-HSDIO external calibration session and, if specified, stores the new calibration constants and calibration data in the onboard 
	/// EEPROM.
        /// 
        /// NOTE: Whether you commit or cancel, the device is reset and the FPGA is reloaded afterwards. 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// </param>
        /// <param name="Action">
        /// The action to perform upon closing.
        /// 
        /// Defined Values:
        /// 
        /// NIHSDIO_VAL_EXT_CAL_COMMIT (62) - The new calibration constants and data determined during the external calibration session are stored in 
	/// the onboard EEPROM, given that the calibration was complete and passed successfully.
        /// NIHSDIO_VAL_EXT_CAL_CANCEL (63) - No changes are made to the calibration constants and data in the EEPROM.  
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// </returns>
        public int CloseExtCal(int Action)
        {
            int pInvokeResult = PInvoke.CloseExtCal(this._handle, Action);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs a self-test on the device and returns the test results. The self-test function performs a simple series of tests that 
	/// ensure the device is powered up and responding. Complete 
    /// testing and calibration are not performed by this function.
        /// 
        /// This function is internal and does not affect external I/O connections or connections between devices.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Self_Test_Result">
        /// This control contains the value returned from the device self-test.
        /// 
        /// Self-test Code Description:
        /// 0 - Self-test passed
        /// Anything else - Self-test failed
        /// 
        /// 
        /// </param>
        /// <param name="Self_Test_Message">
        /// Returns the self-test response string from the device; you must pass a ViChar array at least IVI_MAX_MESSAGE_BUF_SIZE bytes in length
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Call this function to check the hardware to determine if your dynamic data operation has completed. You can also use this function for 
	/// continuous dynamic data operations to poll for error conditions.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Done">
        /// VI_TRUE is returned if the data operation is complete or an error has occurred.
        /// 
        /// VI_FALSE is returned if the data operation has not completed.
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int IsDone(out bool Done)
        {
            ushort DoneAsUShort;
            int pInvokeResult = PInvoke.IsDone(this._handle, out DoneAsUShort);
            Done = System.Convert.ToBoolean(DoneAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Use this function to force a channel into a high-impedance state.  The effect is immediate - it does not require the session be committed.  
	/// The channel will remain tristated regardless of what other software commands are called.  Call this function again and pass VI_FALSE into 
	/// the Tristate parameter to allow other software commands to control the channel normally.
        /// 
        /// Channels are kept in a high-impedance state while the session remains open. Closing the session does not affect the high-impedance state of 
	/// the channel, but future sessions can now control it.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitGenerationSession.
        /// </param>
        /// <param name="Channel_List">
        /// This string identifies which channels will be tristated. Channels not specified in this list are unaffected.
        /// 
        /// Syntax examples: "2-15" or "0-3, 5, 8-15" or "0, 3, 10"
        /// 
        /// 
        /// </param>
        /// <param name="Tristate">
        /// Defined Values:
        /// 
        /// VI_TRUE: Force the channels specified in Channel_List to remain tristated, ignoring future software commands.
        /// 
        /// VI_FALSE: Allow the channels specified in Channel_List to be untristated by future software commands.
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int TristateChannels(string Channel_List, bool Tristate)
        {
            int pInvokeResult = PInvoke.TristateChannels(this._handle, Channel_List, System.Convert.ToUInt16(Tristate));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Programs the hardware for the dynamic data operation using the attributes you select. Before entering the committed state, most attribute 
	/// values are stored in software only; these values have not yet been programmed to the hardware. Once the session is committed, the hardware 
	/// is configured.
        /// 
        /// For many operations it is not necessary to explicitly call this function because the following functions implicitly commit:
        /// niHSDIO_Initiate 
        /// niHSDIO_ReadWaveformU32
        /// niHSDIO_WriteNamedWaveformU32
        /// niHSDIO_WriteScript
        /// 
        /// Start the operation with niHSDIO_Initiate. Running this function while a dynamic operation is in progress returns an error. Committing only 
	/// programs attributes changed since previous commits.
        /// 
        /// NOTE: Committing some attributes may have immediate effects seen on external instrument connectors. Voltage levels are an example of an 
	/// attribute with an immediate effect when committed.
        /// 
        /// Before committing a session that requires an external clock, ensure the external clock is available. Otherwise you receive an error that the 
	/// device could not find or lock to the external clock.
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int CommitDynamic()
        {
            int pInvokeResult = PInvoke.CommitDynamic(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Programs the hardware for the static data operation using the attributes you select. Before entering the committed state, most attribute 
	/// values are stored in software only; these values have not yet been programmed to the hardware. Once the session is committed, the hardware 
	/// is configured.
        /// 
        /// For most static operations it is not necessary to explicitly call niHSDIO_CommitStatic because the following functions implicitly commit:
        /// niHSDIO_ReadStaticU32
        /// niHSDIO_WriteStaticU32
        /// 
        /// Committing only programs attributes changed since previous commits.
        /// 
        /// NOTE: Committing some attributes may have immediate effects seen on external instrument connectors. Voltage levels are an example of an 
	/// attribute with an immediate effect when committed.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from the function used to initialize the session.
        /// 
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        public int CommitStatic()
        {
            int pInvokeResult = PInvoke.CommitStatic(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Call this function to reset the session to its initial state. All channels and front panel terminals are put into a high-impedance state. 
	/// All software attributes are reset to their initial values.
        /// 
        /// During a reset, routes of signals between this and other devices are released, regardless of which device created the route. For instance, a 
	/// trigger signal being exported to a PXI Trigger line and used by another device will no longer be exported.
        /// 
        /// niHSDIO_reset is applied to the ENTIRE device. If you have both a generation and an acquisition session active, the niHSDIO_reset resets the 
	/// current session, including attributes, and invalidates the other session if it is committed or running. The other session must be closed.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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
        /// Call this function to reset the device to its initial state and reload its FPGA. All channels and front panel terminals are put into a 
	/// high-impedance state. All software attributes are reset to their initial values.  The entire contents of the FPGA and EEPROM files are 
	/// reloaded. Use this function to re-enable your device if it has disabled itself because the device temperature has risen above its optimal 
	/// operating temperature.
        /// 
        /// During a device reset, routes of signals between this and other devices are released, regardless of which device created the route. For 
	/// instance, a trigger signal being exported to a PXI Trigger line and used by another device will no longer be exported.
        /// 
        /// niHSDIO_ResetDevice is applied to the ENTIRE device. If you have both a generation and an acquisition session active, the 
	/// niHSDIO_ResetDevice resets the current session, including attributes, and invalidates the other session if it is committed or running. The 
	/// other session must be closed.
        /// 
        /// 
        /// Generally, calling niHSDIO_reset is acceptable instead of calling niHSDIO_ResetDevice. niHSDIO_reset executes more quickly. 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This handle identifies your instrument session. vi was obtained from niHSDIO_InitAcquisitionSession or niHSDIO_InitGenerationSession.
        /// </param>
        /// <returns>
        /// Reports the status of this operation. To obtain a text description of the status code, call niHSDIO_error_message. To obtain additional 
	/// information concerning the error condition, use niHSDIO_GetError and niHSDIO_ClearError.
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

	/// Purpose
	/// Enables the PMU capabilities on the channels specified in the channelList parameter and sources the voltage specified in the voltageLevel 
	/// parameter. 
	///
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name          Type            Description 
	/// vi            ViSession       Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channelList   ViConstString   Specifies the channels being configured. Specify multiple channels by using a channel list or a channel range. 
	/// A channel list is a comma (,) separated sequence of channel names (for example, 0,2 specifies channels 0 and 2). A channel range is a lower 
	/// bound channel followed by a hyphen (-) or colon (:) followed by an upper bound channel (for example, 0-2 specifies channels 0, 1, and 2). 
	/// Use PFI1 or PFI2 to specify a valid PFI channel. Use DDC_CLKOUT or STROBE to specify a valid clocking terminal. If you configure multiple 
	/// channels with this parameter, all those channels have the same settings. Selecting no channels for this parameter returns an error. 
	/// voltageLevel  ViReal64        Specifies the voltage level, in volts (V), for the output generation channel(s). Valid values range from -2 V 
	/// to +7 V. Refer to the NI 6555/6556 Specifications for more information about accuracy. 
	/// sense         ViInt32         Selects between local or remote sensing on the specified channel(s).
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_STPMU_LOCAL_SENSE (88)�Local sensing of voltages. This value is measured on the DDC connector. This value is the default.
	/// NIHSDIO_VAL_STPMU_REMOTE_SENSE (89)�Remote sensing of voltages. This value is measured on the REMOTE SENSE connector. 
	/// currentRange ViReal64 Specifies the current range, in amps (A), that is used in the ensuing current measurement on the specified channel(s). 
	/// Tightening your current range increases your accuracy. The NI 6555/6556 supports the following eight current range options: 32 mA, 8 mA, 2 
	/// mA, 512 �A, 128 �A, 32 �A, 8 �A, or 2 �A. If you choose a range other than one of these options, NI-HSDIO coerces the value up to the 
	/// nearest range. The default value for this parameter is 0. NI-HSDIO returns an error if you choose a value for this paramter greater than 32 
	/// mA or less than or equal to 0. 
	///
	/// Return Value
	/// Name      Type        Description 
	/// Status    ViStatus    Reports the status of this operation. To obtain a text description of the status code, call the niHSDIO_error_message 
	/// function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value               Meaning 
	/// 0                   Success 
	/// Positive Values     Warnings 
	/// Negative Values     Errors 

        public int STPMU_SourceVoltage(string channelList, double voltageLevel, int sense, double currentRange)
        {
            int pInvokeResult = PInvoke.STPMU_SourceVoltage(this._handle, channelList, voltageLevel, sense, currentRange);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Enables the PMU capabilities on the channels specified in the channelList parameter and sources the current specified in the currentLevel 
	/// parameter while maintaining the voltage within the specified limits. 
	///
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name                 Type             Description 
	/// vi                   ViSession        Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channelList          ViConstString    Specifies the channels being configured. Specify multiple channels by using a channel list or a channel 
	/// range. A channel list is a comma (,) separated sequence of channel names (for example, 0,2 specifies channels 0 and 2). A channel range is a 
	/// lower bound channel followed by a hyphen (-) or colon (:) followed by an upper bound channel (for example, 0-2 specifies channels 0, 1, and 
	/// 2). Use PFI1 or PFI2 to specify a valid PFI channel. Use DDC_CLKOUT or STROBE to specify a valid clocking terminal. If you configure 
	/// multiple channels with this parameter, all those channels have the same settings. Selecting no channels for this parameter returns an error. 
	/// currentLevel         ViReal64         Specifies the current level, in amps (A), to source on the specified channel(s). 
	/// currentLevelRange    ViReal64         Specifies the current range, in amps (A), that is used in the ensuing current measurement on the 
	/// specified channel(s). Tightening your current range increases your accuracy. The NI 6555/6556 supports the following eight current range 
	/// options: 32 mA, 8 mA, 2 mA, 512 �A, 128 �A, 32 �A, 8 �A, or 2 �A. If you choose a range other than one of these options, NI-HSDIO coerces 
	/// the value up to the nearest range. The default value for this parameter is 0. NI-HSDIO returns an error if you choose a value for this 
	/// paramter greater than 32 mA or less than or equal to 0. 
	/// lowerVoltageLimit    ViReal64         Specifies the minimum voltage limit, in volts (V), on the specified channel(s). 
	/// upperVoltageLimit    ViReal64         Specifies the maximum voltage limit, in volts (V), on the specified channel(s). 
	///
	/// Return Value
	/// Name                 Type             Description 
	/// Status               ViStatus         Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value                Meaning 
	/// 0                    Success 
	/// Positive Values      Warnings 
	/// Negative Values      Errors 

	public int STPMU_SourceCurrent(string channelList, double currentLevel, double currentLevelRange, double lowerVoltageLimit, double upperVoltageLimit)
        {
            int pInvokeResult = PInvoke.STPMU_SourceCurrent(this._handle, channelList, currentLevel, currentLevelRange, lowerVoltageLimit, upperVoltageLimit);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Disables the PMU source operations on the channels specified in the channelList parameter. Use the returnState parameter to tristate or 
	/// return each channel to its previous digital state. 
	///
	///  Note  When you use the PMU functions to source a voltage or current on your system, the voltage or current continues to be sourced on the 
	/// selected channels until you reset your device or call this function. The niHSDIO_Abort and niHSDIO_close functions do not stop sourcing the 
	/// voltage or current. 
	///  Note After using this function to disable the PMU source operations, you can still utilize the PMU measure voltage operations. 
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name                 Type             Description 
	/// vi                   ViSession        Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channelList          ViConstString    Specifies the channels being configured. Specify multiple channels by using a channel list or a channel 
	/// range. A channel list is a comma (,) separated sequence of channel names (for example, 0,2 specifies channels 0 and 2). A channel range is a 
	/// lower bound channel followed by a hyphen (-) or colon (:) followed by an upper bound channel (for example, 0-2 specifies channels 0, 1, and 
	/// 2). Use PFI1 or PFI2 to specify a valid PFI channel. Use DDC_CLKOUT or STROBE to specify a valid clocking terminal. If you configure 
	/// multiple channels with this parameter, all those channels have the same settings. Selecting no channels for this parameter returns an error. 
	/// returnState          ViInt32          Selects, after disabling the PPMU functionality, whether to tristate the channel or to return it to its 
	/// previous digital state. 
	///
	/// Return Value
	/// Name                 Type             Description 
	/// Status               ViStatus         Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value                Meaning 
	/// 0                    Success 
	/// Positive Values      Warnings 
	/// Negative Values      Errors 

        public int STPMU_DisablePMU(string channelList, int returnState)
        {
            int pInvokeResult = PInvoke.STPMU_DisablePMU(this._handle, channelList, returnState);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// 
	/// Purpose
	/// Measures the voltage on the channels specified in the channelList parameter. You can call this function at any time, including while the 
	/// device is in a digital state or a PMU state. 
	///
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name                 Type             Description 
	/// vi                   ViSession        Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channelList          ViConstString    Specifies the channels being configured. Specify multiple channels by using a channel list or a channel 
	/// range. A channel list is a comma (,) separated sequence of channel names (for example, 0,2 specifies channels 0 and 2). A channel range is a 
	/// lower bound channel followed by a hyphen (-) or colon (:) followed by an upper bound channel (for example, 0-2 specifies channels 0, 1, and 
	/// 2). Use PFI1 or PFI2 to specify a valid PFI channel. Use DDC_CLKOUT or STROBE to specify a valid clocking terminal. If you configure multiple 
	/// channels with this parameter, all those channels have the same settings. Selecting no channels for this parameter returns an error. 
	/// aperture time (4 us) ViReal64         Configures the amount of time, in seconds, to measure each channel. The aperture time determines the 
	/// number of hardware averages per measurement. The larger the aperture time, the greater the number of hardware averages. The suggested default 
	/// value for this control is 4E-6 (0.000004) seconds. 
	/// sense                ViInt32          Selects between local or remote sensing on the specified channel(s).
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_STPMU_LOCAL_SENSE (88)�Local sensing of voltages. This value is measured on the DDC connector. This value is the default.
	/// NIHSDIO_VAL_STPMU_REMOTE_SENSE (89)�Remote sensing of voltages. This value is measured on the REMOTE SENSE connector. 
	///
	/// Output 
	/// Name                 Type             Description 
	/// measurements         ViReal64[]       Returns an array of double-precision numbers representing the averaged measured voltages, per channel, 
	/// in volts (V), over time. The order of the returned voltages directly corresponds with the order in which the channels are configured in the 
	/// channel list parameter. 
	/// numberOfMeasurements ViInt32          Returns the number of measurements taken. 
	///
	/// Return Value
	/// Name                 Type             Description 
	/// Status               ViStatus         Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value                Meaning 
	/// 0                    Success 
	/// Positive Values      Warnings 
	/// Negative Values      Errors 	

        public int STPMU_MeasureVoltage(string channelList, double apertureTime__4_us_, int sense, double[] measurements, out int numberOfMeasurements)
        {
            int pInvokeResult = PInvoke.STPMU_MeasureVoltage(this._handle, channelList, apertureTime__4_us_, sense, measurements, out numberOfMeasurements);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Measures the current on the channels specified in the channelList parameter. You must call the niHSDIO_STPMU_SourceVoltage or the 
	/// niHSDIO_STPMU_SourceCurrent function before measuring current with this function, or NI-HSDIO returns an error. 
	///
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name                 Type             Description 
	/// vi                   ViSession        Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channelList          ViConstString    Specifies the channels being configured. Specify multiple channels by using a channel list or a channel 
	/// range. A channel list is a comma (,) separated sequence of channel names (for example, 0,2 specifies channels 0 and 2). A channel range is a 
	/// lower bound channel followed by a hyphen (-) or colon (:) followed by an upper bound channel (for example, 0-2 specifies channels 0, 1, and 
	/// 2). Use PFI1 or PFI2 to specify a valid PFI channel. Use DDC_CLKOUT or STROBE to specify a valid clocking terminal. If you configure multiple 
	/// channels with this parameter, all those channels have the same settings. Selecting no channels for this parameter returns an error. 
	/// aperture time (4 us) ViReal64         Configures the amount of time, in seconds, to measure each channel. The aperture time determines the 
	/// number of hardware averages per measurement. The larger the aperture time, the greater the number of hardware averages. The suggested default 
	/// value for this control is 4E-6 (0.000004) seconds. 
	/// Output 
	/// Name                 Type             Description 
	/// measurements         ViReal64[]       Returns an array of double-precision numbers representing the averaged measured currents, per channel, 
	/// in amps (A), over time. The order of the returned currents directly corresponds with the order in which the channels are configured in the 
	/// channel list parameter. 
	/// numberOfMeasurements ViInt32          Returns the number of measurements taken. 
	///
	/// Return Value
	/// Name                 Type             Description 
	/// Status               ViStatus         Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value                Meaning 
	/// 0                    Success 
	/// Positive Values      Warnings 
	/// Negative Values      Errors 

        public int STPMU_MeasureCurrent(string channelList, double aperture_time__4_us_, double[] measurements, out int numberOfMeasurements)
        {
            int pInvokeResult = PInvoke.STPMU_MeasureCurrent(this._handle, channelList, aperture_time__4_us_, measurements, out numberOfMeasurements);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Connects or disconnects the channels specified in the channelList parameter to or from the EXTERNAL FORCE terminal (either on the AUX I/O 
	/// connector or the REMOTE SENSE connector on the device front panel, depending on the value of the connectors parameter). This function does 
	/// not force a voltage or current by itself; it only connects NI-HSDIO to an external device. 
	///
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name                 Type             Description 
	/// vi                   ViSession        Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channelList          ViConstString    Specifies the channels being configured. Specify multiple channels by using a channel list or a channel 
	/// range. A channel list is a comma (,) separated sequence of channel names (for example, 0,2 specifies channels 0 and 2). A channel range is a 
	/// lower bound channel followed by a hyphen (-) or colon (:) followed by an upper bound channel (for example, 0-2 specifies channels 0, 1, and 
	/// 2). Use PFI1 or PFI2 to specify a valid PFI channel. Use DDC_CLKOUT or STROBE to specify a valid clocking terminal. If you configure multiple 
	/// channels with this parameter, all those channels have the same settings. Selecting no channels for this parameter returns an error. 
	/// action               ViInt32          Selects whether to connect or disconnect a channel or channels from the EXTERNAL FORCE pin.
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_STPMU_CONNECT_EXTERNAL (92)�Connects the channel to the EXTERNAL FORCE pin.
	/// NIHSDIO_VAL_STPMU_DISCONNECT_EXTERNAL (93)�Disconnects the channel from the EXTERNAL FORCE pin. 
	///
	/// connector            ViInt32          Selects whether to access the PMU pins on the AUX I/O connector or the REMOTE SENSE connector. 
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_STPMU_AUX_IO_CONNECTOR (94)�Accesses the PMU pins on the AUX I/O connector. This is the default value.
	/// NIHSDIO_VAL_STPMU_REMOTE_SENSE_CONNECTOR (95)�Accesses the PMU pins on the REMOTE SENSE connector. 
	///
	/// Return Value
	/// Name                 Type             Description 
	/// Status               ViStatus         Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value                Meaning 
	/// 0                    Success 
	/// Positive Values      Warnings 
	/// Negative Values      Errors 

        public int STPMU_ExternalForceControl(string channelList, int action, int connector)
        {
            int pInvokeResult = PInvoke.STPMU_ExternalForceControl(this._handle, channelList, action, connector);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Connects or disconnects the channel specified in the channel parameter to or from the EXTERNAL SENSE terminal (either on the AUX I/O connector 
	/// or the REMOTE SENSE connector on the device front panel, depending on the value of the connector parameter). 
	///
	///  Note  You can conduct an external sense operation on only one channel at a time with this function. 
	///  Note  Only NI 6555/6556 devices support this function. 
	///
	/// Parameters
	/// Input 
	/// Name                 Type              Description 
	/// vi                   ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// channel              ViConstString     Specifies the channel on which to conduct the external sense. You can specify only one channel at a 
	/// time with this parameter. Use PFI1 or PFI2 to specify a valid PFI channel. 
	/// action               ViInt32           Selects whether to connect or disconnect a channel or channels from the EXTERNAL SENSE pin.
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_STPMU_CONNECT_EXTERNAL (92)�Connects the channel to the EXTERNAL SENSE pin.
	/// NIHSDIO_VAL_STPMU_DISCONNECT_EXTERNAL (93)�Disconnects the channel from the EXTERNAL SENSE pin. 
	///
	/// connector            ViInt32           Selects whether to access the PMU pins on the AUX I/O connector or the REMOTE SENSE connector.
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_STPMU_AUX_IO_CONNECTOR (94)�Accesses the PMU pins on the AUX I/O connector. This is the default value.
	/// NIHSDIO_VAL_STPMU_REMOTE_SENSE_CONNECTOR (95)�Accesses the PMU pins on the REMOTE SENSE connector. 
	///
	/// Return Value
	/// Name                 Type              Description 
	/// Status               ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value                Meaning 
	/// 0                    Success 
	/// Positive Values      Warnings 
	/// Negative Values      Errors 

        public int STPMU_ExternalSenseControl(string channel, int action, int connector)
        {
            int pInvokeResult = PInvoke.STPMU_ExternalSenseControl(this._handle, channel, action, connector);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Configures the Stop trigger for edge triggering. This function is valid only for generation sessions.
	///
	/// Parameters
	/// Input 
	/// Name                 Type              Description 
	/// vi                   ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// Source               ViConstString     You may specify any valid source terminal for this trigger. Trigger voltages and positions are only 
	/// relevant if the source of the trigger is from the front panel connectors.  Note  Only NI 6555/6556 devices support PFI <24..31> and PXIe 
	/// DStarB. 
	///	
	/// Defined Values:
	/// NIHSDIO_VAL_PFI0_STR ("PFI0") PFI 0 on the front panel connector. 
	/// NIHSDIO_VAL_PFI1_STR ("PFI1") PFI 1 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI2_STR ("PFI2") PFI 2 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI3_STR ("PFI3") PFI 3 on the front panel DDC connector.
	/// NIHSDIO_VAL_PFI24_STR ("PFI24") PFI 24 on the front panel connector. 
	/// NIHSDIO_VAL_PFI25_STR ("PFI25") PFI 25 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI26_STR ("PFI26") PFI 26 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI27_STR ("PFI27") PFI 27 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI28_STR ("PFI28") PFI 28 on the front panel connector. 
	/// NIHSDIO_VAL_PFI29_STR ("PFI29") PFI 29 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI30_STR ("PFI30") PFI 30 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PFI31_STR ("PFI31") PFI 31 on the front panel DDC connector. 
	/// NIHSDIO_VAL_PXI_TRIG0_STR ("PXI_Trig0") PXI trigger line 0. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG1_STR ("PXI_Trig1") PXI trigger line 1. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG2_STR ("PXI_Trig2") PXI trigger line 2. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG3_STR ("PXI_Trig3") PXI trigger line 3. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG4_STR ("PXI_Trig4") PXI trigger line 4. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG5_STR ("PXI_Trig5") PXI trigger line 5. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG6_STR ("PXI_Trig6") PXI trigger line 6. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_PXI_TRIG7_STR ("PXI_Trig7") PXI trigger line 7. (PXI/PXI Express devices) 
	/// NIHSDIO_VAL_RTSI0_STR ("RTSI0") RTSI trigger line 0. (PCI devices) 
	/// NIHSDIO_VAL_RTSI1_STR ("RTSI1") RTSI trigger line 1. (PCI devices) 
	/// NIHSDIO_VAL_RTSI2_STR ("RTSI2") RTSI trigger line 2. (PCI devices) 
	/// NIHSDIO_VAL_RTSI3_STR ("RTSI3") RTSI trigger line 3. (PCI devices) 
	/// NIHSDIO_VAL_RTSI4_STR ("RTSI4") RTSI trigger line 4. (PCI devices) 
	/// NIHSDIO_VAL_RTSI5_STR ("RTSI5") RTSI trigger line 5. (PCI devices) 
	/// NIHSDIO_VAL_RTSI6_STR ("RTSI6") RTSI trigger line 6. (PCI devices) 
	/// NIHSDIO_VAL_RTSI7_STR ("RTSI7") RTSI trigger line 7. (PCI devices) 
	/// NIHSDIO_VAL_PXI_STAR_STR ("PXI_STAR") The device uses the PXI_STAR signal which is present on the PXI backplane. This selection is valid only 
	/// for PXI devices in slots other than Slot 2. 
	/// NIHSDIO_VAL_PXI_DSTARB_STR ("PXIe_DStarB") The device uses the PXIe_StarB signal which is present on the PXI Express backplane. This selection 
	/// is valid only for NI 6555/6556 devices. 
	/// 
	/// Edge                ViInt32           Specifies the edge to detect.
	///
	/// Defined Values
	///
	/// NIHSDIO_VAL_RISING_EDGE (12)�Rising edge trigger.
	/// NIHSDIO_VAL_FALLING_EDGE (13)�Falling edge trigger. 
	///
	/// Return Value
	/// Name                Type              Description 
	/// Status              ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value               Meaning 
	/// 0                   Success 
	/// Positive Values     Warnings 
	/// Negative Values     Errors 

        
	public int ConfigureDigitalEdgeStopTrigger(string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeStopTrigger(this._handle, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Configures the Stop trigger for software triggering.
	///
	/// Refer to niHSDIO_SendSoftwareEdgeTrigger for more information about using the software Stop trigger. This function is valid only for 
	/// generation sessions.
	///
	/// Parameters
	/// Input 
	/// Name               Type              Description 
	/// vi                 ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	///
	/// Return Value
	/// Name               Type              Description 
	/// Status             ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value              Meaning 
	/// 0                  Success 
	/// Positive Values    Warnings 
	/// Negative Values    Errors 

        public int ConfigureSoftwareStopTrigger()
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareStopTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

	/// Purpose
	/// Sets the data operation to have no Stop trigger. Calling this function is only necessary if you have configured a Stop trigger and now want to 
	/// disable it. This function is valid only for generation sessions.
	///
	/// Parameters
	/// Input 
	/// Name               Type              Description 
	/// vi                 ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	///
	/// Return Value
	/// Name               Type              Description 
	/// Status             ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value              Meaning 
	/// 0                  Success 
	/// Positive Values    Warnings 
	/// Negative Values    Errors 

        public int DisableStopTrigger()
        {
            int pInvokeResult = PInvoke.DisableStopTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public void SetInt32(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }
        
        public void SetInt32(niHSDIOProperties propertyId, int val)
        {
            this.SetInt32(propertyId, "", val);
        }
        
        public int GetInt32(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel)
        {
            int val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }
        
        public int GetInt32(niHSDIOProperties propertyId)
        {
            return this.GetInt32(propertyId, "");
        }
        
        public void SetDouble(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel, System.Double val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }
        
        public void SetDouble(niHSDIOProperties propertyId, System.Double val)
        {
            this.SetDouble(propertyId, "", val);
        }
        
        public System.Double GetDouble(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel)
        {
            System.Double val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }
        
        public System.Double GetDouble(niHSDIOProperties propertyId)
        {
            return this.GetDouble(propertyId, "");
        }
        
	/// Purpose
	/// This function sets the value of a ViBoolean attribute. This is a low-level function that you can use to set the values of device-specific 
	/// attributes and inherent IVI attributes.
	///
	/// Parameters
	/// Input 
	/// Name               Type              Description 
	/// vi                 ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// Channel List       ViConstString     If the attribute is channel or instance based, this parameter specifies the name of the channel or 
	/// instance on which to set the value of the attribute; if the attribute is not channel or instance based, pass VI_NULL or an empty string.
	///
	/// You can pass in multiple channels to this function. 
	/// Attribute ID ViAttr The ID of an attribute. 
	///
	/// Value              ViBoolean         The value to which you want to set the attribute; some of the values might not be valid depending on the 
	/// current settings of the instrument session. 
	///
	/// Return Value
	/// Name               Type              Description 
	/// Status             ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value              Meaning 
	/// 0                  Success  
	/// Positive Values    Warnings 
	/// Negative Values    Errors 

        public void SetBoolean(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel, bool val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), System.Convert.ToUInt16(val)));
        }
        
        public void SetBoolean(niHSDIOProperties propertyId, bool val)
        {
            this.SetBoolean(propertyId, "", val);
        }

	/// Purpose
	/// This function queries the value of a ViBoolean attribute. You can use this function to get the values of device-specific attributes and 
	/// inherent IVI attributes.
	///
	/// Parameters
	/// Input 
	/// Name               Type              Description 
	/// vi                 ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// Channel List       ViConstString     If the attribute is channel or instance based, this parameter specifies the name of the channel or 
	/// instance on which to set the value of the attribute; if the attribute is not channel or instance based, pass VI_NULL or an empty string.
	///
	/// You can pass in multiple channels to this function. 
	/// Attribute ID ViAttr The ID of an attribute. 
	/// Output 
	/// Name               Type              Description 
	/// Value              ViBoolean         Returns the current value of the attribute; pass the address of a ViBoolean variable. 
	///
	/// Return Value
	/// Name               Type              Description 
	/// Status             ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value              Meaning 
	/// 0                  Success 
	/// Positive Values    Warnings 
	/// Negative Values    Errors 
        
        public bool GetBoolean(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel)
        {
            System.UInt16 val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return System.Convert.ToBoolean(val);
        }
        
        public bool GetBoolean(niHSDIOProperties propertyId)
        {
            return this.GetBoolean(propertyId, "");
        }
        
	/// Purpose
	/// This function sets the value of a ViString attribute. This is a low-level function that you can use to set the values of device-specific 
	/// attributes and inherent IVI attributes.
	///
	/// Parameters
	/// Input 
	/// Name               Type              Description 
	/// vi                 ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// Channel List       ViConstString     If the attribute is channel or instance based, this parameter specifies the name of the channel or 
	/// instance on which to set the value of the attribute; if the attribute is not channel or instance based, pass VI_NULL or an empty string.
	///
	/// You can pass in multiple channels to this function. 
	/// Attribute ID ViAttr The ID of an attribute. 
	///
	/// Value              ViSession         The value to which you want to set the attribute; some of the values might not be valid depending on the 
	/// current settings of the instrument session. 
	///
	/// Return Value
	/// Name               Type Description 
	/// Status             ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value              Meaning 
	/// 0                  Success 
	/// Positive Values    Warnings 
	/// Negative Values    Errors 

        public void SetString(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel, string val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }
        
        public void SetString(niHSDIOProperties propertyId, string val)
        {
            this.SetString(propertyId, "", val);
        }
    
	/// Purpose
	/// This function queries the value of a ViString attribute. You can use this function to get the values of device-specific attributes and 
	/// inherent IVI attributes.
	///
	/// Parameters
	/// Input 
	/// Name               Type              Description 
	/// vi                 ViSession         Identifies your instrument session. vi was obtained from the niHSDIO_InitAcquisitionSession or 
	/// niHSDIO_InitGenerationSession function. 
	/// Channel List       ViConstString     If the attribute is channel or instance based, this parameter specifies the name of the channel or 
	/// instance on which to set the value of the attribute; if the attribute is not channel or instance based, pass VI_NULL or an empty string.
	///
	/// You can pass in multiple channels to this function. 
	/// Attribute ID ViAttr The ID of an attribute. 
	///
	/// Buf Size           ViInt32           Pass the number of bytes in the ViChar array you specify for the value parameter. If the current value of 
	/// the attribute, including the terminating NULL byte, contains more bytes than you indicate in this parameter, the function copies Array Size-1 
	/// bytes into the buffer, places an ASCII NULL byte at the end of the buffer, and returns the array size you must pass to get the entire value. 
	/// For example, if the value is "123456", and the Array Size is 4, the function places "123" into the buffer and returns 7. 
	/// Output 
	/// Name               Type              Description 
	/// Value              ViString          Returns the current value of the attribute; pass the address of a ViString variable. 
	///
	/// Return Value
	/// Name               Type              Description 
	/// Status             ViStatus          Reports the status of this operation. To obtain a text description of the status code, call the 
	/// niHSDIO_error_message function. To obtain additional information concerning the error condition, use the niHSDIO_GetError and 
	/// niHSDIO_ClearError functions. 
	///
	/// The general meaning of the status code is as follows: 
	///
	/// Value              Meaning 
	/// 0                  Success 
	/// Positive Values    Warnings 
	/// Negative Values    Errors 
    
        public string GetString(niHSDIOProperties propertyId, string repeatedCapabilityOrChannel)
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

        public string GetString(niHSDIOProperties propertyId)
        {
            return this.GetString(propertyId, "");
        }

        private System.IntPtr _handle;

        private bool _disposed = true;

        ~niHSDIO() { Dispose(false); }

        private niHSDIO(System.IntPtr handle)
        {
            this._handle = handle;
            this._disposed = false;
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
                this._handle = System.IntPtr.Zero;
            }
            this._disposed = true;
        }



        public int SetAttributeViInt32(string Channel_List, int Attribute_ID, int Value)
        {
            int pInvokeResult = PInvoke.SetAttributeViInt32(this._handle, Channel_List, Attribute_ID, Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public int GetAttributeViInt32(string Channel_List, int Attribute_ID, out int Value)
        {
            int pInvokeResult = PInvoke.GetAttributeViInt32(this._handle, Channel_List, Attribute_ID, out Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

     
        private class PInvoke
        {
            private const string HSDIOModuleName = @"C:\Program Files\IVI Foundation\IVI\Bin\niHSDIO_64.dll";

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_InitAcquisitionSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitAcquisitionSession(string Resource_Name, ushort ID_Query, ushort Reset_Instrument, string Option_String, out System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_InitGenerationSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitGenerationSession(string Resource_Name, ushort ID_Query, ushort Reset_Instrument, string Option_String, out System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_InitExtCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitExtCal(string Resource_Name, string Password, out System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDataVoltageLogicFamily", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDataVoltageLogicFamily(System.IntPtr Instrument_Handle, string Channel_List, int Logic_Family);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDataVoltageCustomLevels", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDataVoltageCustomLevels(System.IntPtr Instrument_Handle, string Channel_List, double Low_Level, double High_Level);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureTriggerVoltageLogicFamily", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTriggerVoltageLogicFamily(System.IntPtr Instrument_Handle, int Logic_Family);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureTriggerVoltageCustomLevels", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTriggerVoltageCustomLevels(System.IntPtr Instrument_Handle, double Low_Level, double High_Level);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureEventVoltageLogicFamily", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureEventVoltageLogicFamily(System.IntPtr Instrument_Handle, int Logic_Family);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureEventVoltageCustomLevels", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureEventVoltageCustomLevels(System.IntPtr Instrument_Handle, double Low_Level, double High_Level);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_AssignDynamicChannels", CallingConvention = CallingConvention.StdCall)]
            public static extern int AssignDynamicChannels(System.IntPtr Instrument_Handle, string Channel_List);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_Initiate", CallingConvention = CallingConvention.StdCall)]
            public static extern int Initiate(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WaitUntilDone", CallingConvention = CallingConvention.StdCall)]
            public static extern int WaitUntilDone(System.IntPtr Instrument_Handle, int Max_Time_Milliseconds);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_Abort", CallingConvention = CallingConvention.StdCall)]
            public static extern int Abort(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureAcquisitionSize", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureAcquisitionSize(System.IntPtr Instrument_Handle, int Samples_Per_Record, int Number_Of_Records);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDataInterpretation", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDataInterpretation(System.IntPtr Instrument_Handle, string Channel_List, int Data_Interpretation);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadWaveformU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadWaveformU32(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, [In, Out] uint[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchWaveformU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchWaveformU32(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, [In, Out] uint[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadWaveformU16", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadWaveformU16(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, [In, Out] short[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchWaveformU16", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchWaveformU16(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, [In, Out] short[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadWaveformU8", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadWaveformU8(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, [In, Out] byte[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchWaveformU8", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchWaveformU8(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, out int Number_Of_Samples_Read, [In, Out] byte[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadMultiRecordU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadMultiRecordU32(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, uint[] Data, nihsdio_wfminfo Waveform_Info);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchMultiRecordU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchMultiRecordU32(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, uint[] Data, nihsdio_wfminfo Waveform_Info);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadMultiRecordU16", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadMultiRecordU16(System.IntPtr Instrument_Handle, int Max_Time_Milliseconds, int Samples_To_Read, int Starting_Record, int Number_of_Records, short[] Data, nihsdio_wfminfo Waveform_Info);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchMultiRecordU16", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchMultiRecordU16(System.IntPtr Instrument_Handle, int Max_Time_Milliseconds, int Samples_To_Read, int Starting_Record, int Number_of_Records, short[] Data, nihsdio_wfminfo Waveform_Info);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadMultiRecordU8", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadMultiRecordU8(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, byte[] Data, nihsdio_wfminfo Waveform_Info);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchMultiRecordU8", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchMultiRecordU8(System.IntPtr Instrument_Handle, int Samples_To_Read, int Max_Time_Milliseconds, int Starting_Record, int Number_of_Records, byte[] Data, nihsdio_wfminfo Waveform_Info);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_FetchWaveformDirectDMA", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchWaveformDirectDMA(System.IntPtr Instrument_Handle, int Max_Time_Milliseconds, int Samples_To_Read, uint Buffer_Size, System.IntPtr Buffer_Address, nihsdio_wfminfo Waveform_Info, uint Offset_to_First_Sample);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_HWC_FetchSampleErrors",CallingConvention = CallingConvention.StdCall)]
            public static extern int HWC_FetchSampleErrors (System.IntPtr Instrument_Handle, int Number_of_Sample_Errors_to_Read, int Max_Time_Milliseconds, out int Number_Of_Sample_Errors_Read, [In, Out] double[] Sample_Number, [In, Out] uint[] Error_Bits, [In, Out] uint[] Error_Repeat_Counts, out uint Reserved_1, out uint Reserved_2);
            
            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteNamedWaveformU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteNamedWaveformU32(System.IntPtr Instrument_Handle, string Waveform_Name, int Samples_To_Write, uint[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteNamedWaveformU16", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteNamedWaveformU16(System.IntPtr Instrument_Handle, string Waveform_Name, int Samples_To_Write, short[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteNamedWaveformU8", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteNamedWaveformU8(System.IntPtr Instrument_Handle, string Waveform_Name, int Samples_To_Write, byte[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteNamedWaveformWDT", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteNamedWaveformWDT(System.IntPtr Instrument_Handle, string Waveform_Name, int Samples_To_Write, int Data_Layout, byte[] Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteNamedWaveformFromFileHWS", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteNamedWaveformFromFileHWS(System.IntPtr Instrument_Handle, string Waveform_Name, string File_Path, ushort Use_Rate_From_Waveform, out int Waveform_Size);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureIdleState", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureIdleState(System.IntPtr Instrument_Handle, string Channel_List, string Idle_State);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureIdleStateU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureIdleStateU32(System.IntPtr Instrument_Handle, uint Idle_State);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureInitialState", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureInitialState(System.IntPtr Instrument_Handle, string Channel_List, string Initial_State);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureInitialStateU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureInitialStateU32(System.IntPtr Instrument_Handle, uint Initial_State);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureGenerationRepeat", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureGenerationRepeat(System.IntPtr Instrument_Handle, int Repeat_Mode, int Repeat_Count);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureWaveformToGenerate", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureWaveformToGenerate(System.IntPtr Instrument_Handle, string Waveform_Name);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_AllocateNamedWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int AllocateNamedWaveform(System.IntPtr Instrument_Handle, string Waveform_Name, int Size_In_Samples);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SetNamedWaveformNextWritePosition", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetNamedWaveformNextWritePosition(System.IntPtr Instrument_Handle, string Waveform_Name, int Position, int Offset);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DeleteNamedWaveform", CallingConvention = CallingConvention.StdCall)]
            public static extern int DeleteNamedWaveform(System.IntPtr Instrument_Handle, string Waveform_Name);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureGenerationMode", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureGenerationMode(System.IntPtr Instrument_Handle, int Generation_Mode);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteScript", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteScript(System.IntPtr Instrument_Handle, string Script);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureScriptToGenerate", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureScriptToGenerate(System.IntPtr Instrument_Handle, string Script_Name);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureSampleClock", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSampleClock(System.IntPtr Instrument_Handle, string Clock_Source, double Clock_Rate);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDataPosition", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDataPosition(System.IntPtr Instrument_Handle, string Channel_List, int Position);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDataPositionDelay", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDataPositionDelay(System.IntPtr Instrument_Handle, string Channel_List, double Delay);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureRefClock", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRefClock(System.IntPtr Instrument_Handle, string Clock_Source, double Clock_Rate);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_AdjustSampleClockRelativeDelay", CallingConvention = CallingConvention.StdCall)]
            public static extern int AdjustSampleClockRelativeDelay(System.IntPtr Instrument_Handle, double Adjustment_Time);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalEdgeStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeStartTrigger(System.IntPtr Instrument_Handle, string Source, int Edge);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchStartTrigger(System.IntPtr Instrument_Handle, string Channel_List, string Pattern, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchStartTriggerU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchStartTriggerU32(System.IntPtr Instrument_Handle, string Channel_List, uint Pattern, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureSoftwareStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareStartTrigger(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DisableStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableStartTrigger(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalEdgeRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeRefTrigger(System.IntPtr Instrument_Handle, string Source, int Edge, int Pretrigger_Samples);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchRefTrigger(System.IntPtr Instrument_Handle, string Channel_List, string Pattern, int Trigger_When, int Pretrigger_Samples);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchRefTriggerU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchRefTriggerU32(System.IntPtr Instrument_Handle, string Channel_List, uint Pattern, int Trigger_When, int Pretrigger_Samples);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureSoftwareRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareRefTrigger(System.IntPtr Instrument_Handle, int Pretrigger_Samples);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DisableRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableRefTrigger(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalEdgeAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeAdvanceTrigger(System.IntPtr Instrument_Handle, string Source, int Edge);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchAdvanceTrigger(System.IntPtr Instrument_Handle, string Channel_List, string Pattern, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchAdvanceTriggerU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchAdvanceTriggerU32(System.IntPtr Instrument_Handle, string Channel_List, uint Pattern, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureSoftwareAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareAdvanceTrigger(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DisableAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableAdvanceTrigger(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalEdgeScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeScriptTrigger(System.IntPtr Instrument_Handle, string Trigger_ID, string Source, int Edge);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalLevelScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalLevelScriptTrigger(System.IntPtr Instrument_Handle, string Trigger_ID, string Source, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureSoftwareScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareScriptTrigger(System.IntPtr Instrument_Handle, string Trigger_ID);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DisableScriptTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableScriptTrigger(System.IntPtr Instrument_Handle, string Trigger_ID);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalLevelPauseTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalLevelPauseTrigger(System.IntPtr Instrument_Handle, string Source, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchPauseTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchPauseTrigger(System.IntPtr Instrument_Handle, string Channel_List, string Pattern, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigurePatternMatchPauseTriggerU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePatternMatchPauseTriggerU32(System.IntPtr Instrument_Handle, string Channel_List, uint Pattern, int Trigger_When);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DisablePauseTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisablePauseTrigger(System.IntPtr Instrument_Handle);

	    [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureDigitalEdgeStopTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeStopTrigger (System.IntPtr Instrument_Handle, string Source, int Edge);
            
	    [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ConfigureSoftwareStopTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareStopTrigger (System.IntPtr Instrument_Handle);
            
   	    [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_DisableStopTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableStopTrigger (System.IntPtr Instrument_Handle);
            
	    [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SendSoftwareEdgeTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int SendSoftwareEdgeTrigger(System.IntPtr Instrument_Handle, int Trigger, string Trigger_Identifier);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ExportSignal", CallingConvention = CallingConvention.StdCall)]
            public static extern int ExportSignal(System.IntPtr Instrument_Handle, int Signal, string Signal_Identifier, string Output_Terminal);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_AssignStaticChannels", CallingConvention = CallingConvention.StdCall)]
            public static extern int AssignStaticChannels(System.IntPtr Instrument_Handle, string Channel_List);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ReadStaticU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadStaticU32(System.IntPtr Instrument_Handle, out uint Read_Data);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_WriteStaticU32", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteStaticU32(System.IntPtr Instrument_Handle, uint Write_Data, uint Channel_Mask);

	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_SourceVoltage",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_SourceVoltage (System.IntPtr Instrument_Handle, string channelList, double voltageLevel, int sense, double currentRange);
            
	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_SourceCurrent",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_SourceCurrent (System.IntPtr Instrument_Handle, string channelList, double currentLevel, double currentLevelRange, double lowerVoltageLimit, double upperVoltageLimit);
            
	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_DisablePMU",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_DisablePMU (System.IntPtr Instrument_Handle, string channelList, int returnState);
            
	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_MeasureVoltage",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_MeasureVoltage (System.IntPtr Instrument_Handle, string channelList, double apertureTime__4_us_, int sense, [In, Out] double[] measurements, out int numberOfMeasurements);
            
	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_MeasureCurrent",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_MeasureCurrent (System.IntPtr Instrument_Handle, string channelList, double aperture_time__4_us_, [In, Out] double[] measurements, out int numberOfMeasurements);
            
	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_ExternalForceControl",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_ExternalForceControl (System.IntPtr Instrument_Handle, string channelList, int action, int connector);
            
	    [DllImport(HSDIOModuleName, EntryPoint="niHSDIO_STPMU_ExternalSenseControl",CallingConvention=CallingConvention.StdCall)]
            public static extern int STPMU_ExternalSenseControl (System.IntPtr Instrument_Handle, string channel, int action, int connector);
            
            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SelfCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int SelfCal(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ChangeExtCalPassword", CallingConvention = CallingConvention.StdCall)]
            public static extern int ChangeExtCalPassword(System.IntPtr Instrument_Handle, string Old_Password, string New_Password);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_CalAdjustChannelVoltage", CallingConvention = CallingConvention.StdCall)]
            public static extern int CalAdjustChannelVoltage(System.IntPtr Instrument_Handle, string Channel);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_CloseExtCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int CloseExtCal(System.IntPtr Instrument_Handle, int Action);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test(System.IntPtr Instrument_Handle, out short Self_Test_Result, System.Text.StringBuilder Self_Test_Message);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_IsDone", CallingConvention = CallingConvention.StdCall)]
            public static extern int IsDone(System.IntPtr Instrument_Handle, out ushort Done);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_TristateChannels", CallingConvention = CallingConvention.StdCall)]
            public static extern int TristateChannels(System.IntPtr Instrument_Handle, string Channel_List, ushort Tristate);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_CommitDynamic", CallingConvention = CallingConvention.StdCall)]
            public static extern int CommitDynamic(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_CommitStatic", CallingConvention = CallingConvention.StdCall)]
            public static extern int CommitStatic(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_ResetDevice", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetDevice(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close(System.IntPtr Instrument_Handle);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message(System.IntPtr Instrument_Handle, int Error_Code, System.Text.StringBuilder Error_Message_2);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, out ushort Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, out int Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, out double Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, out System.IntPtr Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, int Buf_Size, System.Text.StringBuilder Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, ushort Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, int Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, double Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, System.IntPtr Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString(System.IntPtr Instrument_Handle, string Channel_List, int Attribute_ID, string Value);

            [DllImport(HSDIOModuleName, EntryPoint = "niHSDIO_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError(System.IntPtr Instrument_Handle, out int Error_Code, int Error_Description_Buffer_Size, System.Text.StringBuilder Error_Description);


            public static int TestForError(System.IntPtr handle, int status)
            {
                if ((status < 0))
                {
                    PInvoke.ThrowError(handle, status);
                }
                return status;
            }

            public static int ThrowError(System.IntPtr handle, int code)
            {
                int status;
                int size = PInvoke.GetError(handle, out status, 0, null);
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                if ((size >= 0))
                {
                    msg.Capacity = size;
                    PInvoke.GetError(handle, out status, size, msg);
                }
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            }
        }
    }

    public class niHSDIOConstants
    {             
        public const int _50vLogic = 5;

        public const int _33vLogic = 6;

        public const int _25vLogic = 7;

        public const int _18vLogic = 8;
        
	    public const int _15vLogic = 80;
        
        public const int _12vLogic = 81;

        public const int HighOrLow = 3;

        public const int ValidOrInvalid = 4;

        public const int GroupBySample = 71;

        public const int GroupByChannel = 72;

        public const int Finite = 16;

        public const int Continuous = 17;

        public const int CurrentPosition = 45;

        public const int StartOfWaveform = 44;

        public const int Waveform = 14;

        public const int Scripted = 15;

        public const string OnBoardClockStr = "OnBoardClock";

        public const string ClkInStr = "ClkIn";

        public const string PxiStarStr = "PXI_STAR";

        public const string StrobeStr = "STROBE";

        public const int SampleClockRisingEdge = 18;

        public const int SampleClockFallingEdge = 19;

        public const int DelayFromSampleClockRisingEdge = 20;

        public const string NoneStr = "None";

        public const string PxiClk10Str = "PXI_CLK10";

        public const string Rtsi7Str = "RTSI7";

        public const string Pfi0Str = "PFI0";

        public const string Pfi1Str = "PFI1";

        public const string Pfi2Str = "PFI2";

        public const string Pfi3Str = "PFI3";

        public const string PxiTrig0Str = "PXI_Trig0";

        public const string PxiTrig1Str = "PXI_Trig1";

        public const string PxiTrig2Str = "PXI_Trig2";

        public const string PxiTrig3Str = "PXI_Trig3";

        public const string PxiTrig4Str = "PXI_Trig4";

        public const string PxiTrig5Str = "PXI_Trig5";

        public const string PxiTrig6Str = "PXI_Trig6";

        public const string PxiTrig7Str = "PXI_Trig7";

        public const int RisingEdge = 12;

        public const int FallingEdge = 13;

        public const int PatternMatches = 36;

        public const int PatternDoesNotMatch = 37;

        public const string ScriptTrigger0 = "scriptTrigger0";

        public const string ScriptTrigger1 = "scriptTrigger1";

        public const string ScriptTrigger2 = "scriptTrigger2";

        public const string ScriptTrigger3 = "scriptTrigger3";

        public const int High = 34;

        public const int Low = 35;

        public const int StartTrigger = 53;

        public const int RefTrigger = 54;

        public const int ScriptTrigger = 58;
	
	    public const int StopTrigger = 82;

        public const int SampleClock = 51;

        public const int RefClock = 52;

        public const int AdvanceTrigger = 61;

        public const int PauseTrigger = 57;

        public const int DataActiveEvent = 55;

        public const int MarkerEvent = 59;

        public const int ReadyForStartEvent = 56;

        public const int ReadyForAdvanceEvent = 66;

        public const int EndOfRecordEvent = 68;

        public const int OnboardRefClock = 60;

        public const string DoNotExportStr = "";

        public const string ClkOutStr = "ClkOut";

        public const string DdcClkOutStr = "DDC_ClkOut";

        public const string MarkerEvent0 = "marker0";

        public const string MarkerEvent1 = "marker1";

        public const string MarkerEvent2 = "marker2";

        public const string MarkerEvent3 = "marker3";

        public const int StpmuLocalSense = 88;
        
        public const int StpmuRemoteSense = 89;
        
        public const int StpmuReturnToTristate = 90;
        
        public const int StpmuReturnToPrevious = 91;
        
        public const int StpmuConnectExternal = 92;
        
        public const int StpmuDisconnectExternal = 93;
        
        public const int StpmuAuxIoConnector = 94;
        
        public const int StpmuRemoteSenseConnector = 95;
        
        public const int ExtCalCommit = 62;
        
        public const int ExtCalCancel = 63;
    }
}

    public enum niHSDIOProperties
    {
        
        /// <summary>
        /// System.String
        /// </summary>
        DynamicChannels = 1150002,
        
        /// <summary>
        /// System.String
        /// </summary>
        StaticChannels = 1150003,
        
        /// <summary>
        /// System.Double
        /// </summary>
        DataVoltageHighLevel = 1150007,
        
        /// <summary>
        /// System.Double
        /// </summary>
        DataVoltageLowLevel = 1150006,

        /// <summary>
        /// System.Double
        /// </summary>
        DataTerminationVoltageLevel = 1150161,

        /// <summary>
        /// System.Int32
        /// </summary>
        DatavoltageRange = 1150163,
        
        /// <summary>
        /// System.Double
        /// </summary>
        TriggerVoltageHighLevel = 1150009,
        
        /// <summary>
        /// System.Double
        /// </summary>
        TriggerVoltageLowLevel = 1150008,

        /// <summary>
        /// System.Int32
        /// </summary>
        TriggervoltageRange = 1150172,
        
        /// <summary>
        /// System.Double
        /// </summary>
        EventVoltageHighLevel = 1150080,
        
        /// <summary>
        /// System.Double
        /// </summary>
        EventVoltageLowLevel = 1150079,

        /// <summary>
        /// System.Int32
        /// </summary>
        EventVoltageRange = 1150173,

        /// <summary>
        /// System.Double
        /// </summary>
        ClockVoltageHighLevel = 1150168,

        /// <summary>
        /// System.Double
        /// </summary>
        ClockVoltageLowLevel = 1150169,

        /// <summary>
        /// System.Int32
        /// </summary>
        ClockVoltageRange = 1150173,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        SamplesPerRecord = 1150029,
        
        /// <summary>
        /// System.Double
        /// </summary>
        InputImpedance = 1150070,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DataInterpretation = 1150010,

        /// <summary>
        /// System.Int32
        /// </summary>
        DriveType = 1150139,

        /// <summary>
        /// System.Int32
        /// </summary>
        DataTristateMode = 1150160,

        /// <summary>
        /// System.Int32
        /// </summary>
        DataTerminationMode = 1150175,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        FetchBacklog = 1150031,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        FetchRelativeTo = 1150067,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        FetchOffset = 1150068,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        InitialState = 1150064,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        IdleState = 1150065,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        RepeatMode = 1150026,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        RepeatCount = 1150071,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        GenerationMode = 1150025,
        
        /// <summary>
        /// System.String
        /// </summary>
        WaveformToGenerate = 1150027,
        
        /// <summary>
        /// System.String
        /// </summary>
        ScriptToGenerate = 1150028,
        
        /// <summary>
        /// System.Double
        /// </summary>
        SampleClockRate = 1150014,
        
        /// <summary>
        /// System.String
        /// </summary>
        SampleClockSource = 1150013,
        
        /// <summary>
        /// System.Double
        /// </summary>
        SampleClockImpedance = 1150060,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedSampleClockOutputTerminal = 1150063,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        ExportedSampleClockMode = 1150061,
        
        /// <summary>
        /// System.Double
        /// </summary>
        ExportedSampleClockDelay = 1150062,
        
        /// <summary>
        /// System.Double
        /// </summary>
        RefClockRate = 1150012,
        
        /// <summary>
        /// System.String
        /// </summary>
        RefClockSource = 1150011,
        
        /// <summary>
        /// System.Double
        /// </summary>
        RefClockImpedance = 1150058,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedRefClockOutputTerminal = 1150059,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedOnboardRefClockOutputTerminal = 1150085,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DataPosition = 1150056,
        
        /// <summary>
        /// System.Double
        /// </summary>
        DataPositionDelay = 1150057,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        OscillatorPhaseDacValue = 1150072,
        
        /// <summary>
        /// System.Double
        /// </summary>
        ExportedSampleClockOffset = 1150083,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        StartTriggerType = 1150032,
        
        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeStartTriggerSource = 1150033,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeStartTriggerEdge = 1150034,
        
        /// <summary>
        /// System.String
        /// </summary>
        PatternMatchStartTriggerPattern = 1150035,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        PatternMatchStartTriggerWhen = 1150036,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        StartTriggerPosition = 1150075,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedStartTriggerOutputTerminal = 1150037,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        RefTriggerType = 1150038,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        RefTriggerPretriggerSamples = 1150030,
        
        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeRefTriggerSource = 1150039,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeRefTriggerEdge = 1150040,
        
        /// <summary>
        /// System.String
        /// </summary>
        PatternMatchRefTriggerPattern = 1150041,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        PatternMatchRefTriggerWhen = 1150042,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        RefTriggerPosition = 1150077,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedRefTriggerOutputTerminal = 1150043,
        
        /// <summary>
        /// System.Double
        /// </summary>
        StartToRefTriggerHoldoff = 1150086,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        ScriptTriggerType = 1150044,
        
        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeScriptTriggerSource = 1150045,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeScriptTriggerEdge = 1150046,
        
        /// <summary>
        /// System.String
        /// </summary>
        DigitalLevelScriptTriggerSource = 1150047,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalLevelScriptTriggerWhen = 1150048,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedScriptTriggerOutputTerminal = 1150049,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        PauseTriggerType = 1150050,
        
        /// <summary>
        /// System.String
        /// </summary>
        DigitalLevelPauseTriggerSource = 1150051,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalLevelPauseTriggerWhen = 1150052,
        
        /// <summary>
        /// System.String
        /// </summary>
        PatternMatchPauseTriggerPattern = 1150053,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        PatternMatchPauseTriggerWhen = 1150054,
        
        /// <summary>
        /// System.String
        /// </summary>
        ExportedPauseTriggerOutputTerminal = 1150055,

        /// <summary>
        /// System.Int32
        /// </summary>
        StopTriggerType = 1150152,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeStopTriggerSource = 1150153,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeStopTriggerEdge = 1150154,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeStopTriggerTerminalConfiguration = 1150155,

        /// <summary>
        /// System.Double
        /// </summary>
        DigitalEdgeStopTriggerImpedance = 1150156,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedStopTriggerOutputTerminal = 1150157,

        /// <summary>
        /// System.Int32
        /// </summary>
        ExportedStopTriggerTerminalConfiguration = 1150158,
        
        /// <summary>
        /// System.String
        /// </summary>
        ReadyForStartEventOutputTerminal = 1150016,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        ReadyForStartEventLevelActiveLevel = 1150017,
        
        /// <summary>
        /// System.String
        /// </summary>
        DataActiveEventOutputTerminal = 1150019,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DataActiveEventLevelActiveLevel = 1150020,

        /// <summary>
        /// System.Double
        /// </summary>
        DataDeskew = 1150162,

        /// <summary>
        /// System.Int32
        /// </summary>
        TriggerPositionDelay = 1150164,

        /// <summary>
        /// System.Int32
        /// </summary>
        TriggerDeskew = 1150165,

        /// <summary>
        /// System.Int32
        /// </summary>
        EventPositionDelay = 1150166,

        /// <summary>
        /// System.Int32
        /// </summary>
        EventDeskew = 1150167,

        /// <summary>
        /// System.Int32
        /// </summary>
        DataActiveEventPosition = 1150081,
        
        /// <summary>
        /// System.String
        /// </summary>
        MarkerEventOutputTerminal = 1150022,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        MarkerEventPulsePolarity = 1150023,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        MarkerEventPosition = 1150082,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        TotalAcquisitionMemorySize = 1150073,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        TotalGenerationMemorySize = 1150074,
        
        /// <summary>
        /// System.Int32
        /// </summary>
        DeviceSerialNumber = 1150084,

        /// <summary>
        /// System.Int32
        /// </summary>
        SupportedDataStates = 1150130,

        /// <summary>
        /// System.Int32
        /// </summary>
        HWCSampleErrorBacklog = 1150132,

        /// <summary>
        /// System.Int32
        /// </summary>
        HWCNumSampleErrors = 1150133,

        /// <summary>
        /// System.Double
        /// </summary>
        HWCSamplesCompared = 1150134,

        /// <summary>
        /// System.Int32
        /// </summary>
        SpaceAvailableInStreamingWaveform = 1150141,

        /// <summary>
        /// System.String
        /// </summary>
        StreamingWaveformName = 1150142,

        /// <summary>
        /// System.Int32
        /// </summary>
        DevicePowerConsumption = 1150170,

        /// <summary>
        /// System.Int32
        /// </summary>
        DevicePeakPowerConsumed = 1150171,

        /// <summary>
        /// System.Int32
        /// </summary>
        DataActiveInternalRouteDelay = 1150138,

}


