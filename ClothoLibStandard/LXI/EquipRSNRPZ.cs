using System;
using System.Runtime.InteropServices;

namespace ClothoLibStandard
{
    public class EquipRSNRPZ : object, System.IDisposable
    {
                private System.IntPtr _handle;

        private bool _disposed = true;

        ~EquipRSNRPZ() { Dispose(false); }


        /// <summary>
        /// This function performs the following initialization actions and assigned specified sesnor to channel 1:
        /// 
        /// - Opens a session to the specified device using the interface and address specified in the Resource Name control.
        /// 
        /// - Performs an identification query on the sensor.
        /// 
        /// - Resets the instrument to a known state.
        /// 
        /// - Sends initialization commands to the sensor
        /// 
        /// - Returns an Instrument Handle which is used to differentiate between different sessions of this instrument driver.
        /// 
        /// Note:
        /// 
        /// 1) Do not initilize every sensor with rsnrpz_init function. if you want comunicate with more sensor use rsnrpz_AddSensor for adding a new channel. The reason is the rsnrpz_close destroys all sensor sessions assigned to process.
        /// </summary>
        /// <param name="Resource_Name">
        /// This control specifies the interface and address of the device that is to be initialized (Instrument Descriptor). The exact grammar to be used in this control is shown in the note below. 
        /// 
        /// Default Value:  "USB::0x0aad::0x000b::100000"
        /// 
        /// Notes:
        /// 
        /// (1) Based on the Instrument Descriptor, this operation establishes a communication session with a device.  The grammar for the Instrument Descriptor is shown below.  Optional parameters are shown in square brackets ([]).
        /// 
        /// Interface   Grammar
        /// ------------------------------------------------------
        /// USB         USB::vendor Id::product Id::serial number
        /// 
        /// where:
        ///   vendor Id is 0aad for all Rohde&amp;Schwarz instruments.
        /// 
        ///   product Id depends on your sensor model:
        ///              NRP-Z21 : 0x0003
        ///              NRP-FU  : 0x0004
        ///              FSH-Z1  : 0x000b
        ///              NRP-Z11 : 0x000c
        ///              NRP-Z22 : 0x0013
        ///              NRP-Z23 : 0x0014
        ///              NRP-Z24 : 0x0015
        ///              NRP-Z51 : 0x0016
        ///              NRP-Z52 : 0x0017
        ///              NRP-Z55 : 0x0018
        ///              FSH-Z18 : 0x001a
        ///              NRP-Z91 : 0x0021
        ///              NRP-Z81 : 0x0023
        ///              NRP-Z37 : 0x002d
        ///              NRP-Z27 : 0x002f
        /// 
        ///   serianl number you can find on your sensor. Serial number is number with 6 digits. For exampel 9000003
        /// 
        ///  you can use star '*' for product id or serial number in resource descriptor. Use star only for one connected sensor, because driver opens only first sensor on the bus.
        ///  
        /// The USB keyword is used for USB interface.
        /// 
        /// Examples:
        /// USB   - "USB::0x0aad::0x000b::100000"
        /// USB   - "USB::0x0aad::0x000b::*" - Opens first FSH-Z1 sensor
        /// USB   - "USB::0x0aad::*"         - Opens first R&amp;S sensor
        /// USB   - "*"                      - Opens first R&amp;S sensor
        /// </param>
        /// <param name="ID_Query">
        /// This control specifies if an ID Query is sent to the instrument during the initialization procedure.
        /// 
        /// Valid Range:
        /// VI_FALSE (0) - Skip Query
        /// VI_TRUE  (1) - Do Query (Default Value)
        /// 
        /// Notes:
        ///    
        /// (1) Under normal circumstances the ID Query ensures that the instrument initialized is the type supported by this driver. However circumstances may arise where it is undesirable to send an ID Query to the instrument.  In those cases; set this control to "Skip Query" and this function will initialize the selected interface, without doing an ID Query.
        /// 
        /// </param>
        /// <param name="Reset_Device">
        /// This control specifies if the instrument is to be reset to its power-on settings during the initialization procedure.
        /// 
        /// Valid Range:
        /// VI_FALSE (0) - Don't Reset
        /// VI_TRUE  (1) - Reset Device (Default Value)
        /// 
        /// Notes:
        /// 
        /// (1) If you do not want the instrument reset. Set this control to "Don't Reset" while initializing the instrument.
        /// 
        /// </param>
        /// <param name="Instrument_Handle">
        /// This control returns an Instrument Handle that is used in all subsequent function calls to differentiate between different sessions of this instrument driver.
        /// 
        /// Notes:
        /// 
        /// (1) Each time this function is invoked a Unique Session is opened.  It is possible to have more than one session open for the same resource.
        /// 
        /// </param>
        public EquipRSNRPZ()
        {

        }
        public void Init(string Resource_Name, bool ID_Query, bool Reset_Device)
        {
            int pInvokeResult = PInvoke.init(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset_Device), out this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }


        /// <summary>
        /// This function performs the following initialization actions and adds sensor to channel list:
        /// 
        /// - Opens a session to the specified device using the interface and address specified in the Resource Name control.
        /// 
        /// - Performs an identification query on the Instrument.
        /// 
        /// - Resets the instrument to a known state.
        /// 
        /// - Sends initialization commands to the sensor
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 2 to 31
        /// 
        /// Default Value: 2
        /// 
        /// Note:
        /// 
        /// 1) function rsnrpz_init adds assigne sensor to channel 1 automatically.
        /// </param>
        /// <param name="Resource_Name">
        /// This control specifies the interface and address of the device that is to be initialized (Instrument Descriptor). The exact grammar to be used in this control is shown in the note below. 
        /// 
        /// Default Value:  "USB::0x0aad::0x000b::100000"
        /// 
        /// Notes:
        /// 
        /// (1) Based on the Instrument Descriptor, this operation establishes a communication session with a device.  The grammar for the Instrument Descriptor is shown below.  Optional parameters are shown in square brackets ([]).
        /// 
        /// Interface   Grammar
        /// ------------------------------------------------------
        /// USB         USB::vendor Id::product Id::serial number
        ///             
        /// where:
        ///   vendor Id is 0aad for all Rohde&amp;Schwarz instruments.
        /// 
        ///   product Id depends on your sensor model:
        ///              NRP-Z21 : 0x0003
        ///              NRP-FU  : 0x0004
        ///              FSH-Z1  : 0x000b
        ///              NRP-Z11 : 0x000c
        ///              NRP-Z22 : 0x0013
        ///              NRP-Z23 : 0x0014
        ///              NRP-Z24 : 0x0015
        ///              NRP-Z51 : 0x0016
        ///              NRP-Z52 : 0x0017
        ///              NRP-Z55 : 0x0018
        ///              FSH-Z18 : 0x001a
        ///              NRP-Z91 : 0x0021
        ///              NRP-Z81 : 0x0023
        ///              NRP-Z37 : 0x002d
        ///              NRP-Z27 : 0x002f
        /// 
        ///   serianl number you can find on your sensor. Serial number is number with 6 digits. For exampel 9000003
        /// 
        /// The USB keyword is used for USB interface.
        /// 
        /// Examples:
        /// USB   - "USB::0x0aad::0x000b::100000"
        /// </param>
        /// <param name="ID_Query">
        /// This control specifies if an ID Query is sent to the instrument during the initialization procedure.
        /// 
        /// Valid Range:
        /// VI_FALSE (0) - Skip Query
        /// VI_TRUE  (1) - Do Query (Default Value)
        /// 
        /// Notes:
        ///    
        /// (1) Under normal circumstances the ID Query ensures that the instrument initialized is the type supported by this driver. However circumstances may arise where it is undesirable to send an ID Query to the instrument.  In those cases; set this control to "Skip Query" and this function will initialize the selected interface, without doing an ID Query.
        /// 
        /// </param>
        /// <param name="Reset_Device">
        /// This control specifies if the instrument is to be reset to its power-on settings during the initialization procedure.
        /// 
        /// Valid Range:
        /// VI_FALSE (0) - Don't Reset
        /// VI_TRUE  (1) - Reset Device (Default Value)
        /// 
        /// Notes:
        /// 
        /// (1) If you do not want the instrument reset. Set this control to "Don't Reset" while initializing the instrument.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// BFFC0002  Parameter 2 (ID Query) out of range.
        /// BFFC0003  Parameter 3 (Reset Device) out of range.
        /// BFFC0011  Instrument returned invalid response to ID Query
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int AddSensor(int Channel, string Resource_Name, bool ID_Query, bool Reset_Device)
        {
            int pInvokeResult = PInvoke.AddSensor(this._handle, Channel, Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset_Device));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function immediately sets all the sensors to the IDLE state. Measurements in progress are interrupted. If INIT:CONT ON is set, a new measurement is immediately started since the trigger system is not influenced.
        /// 
        /// Remote-control command(s):
        /// ABOR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chans_abort()
        {
            int pInvokeResult = PInvoke.chans_abort(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns number of available channels (1, 2 or 4 depending on installed options NRP-B2 - Two channel interface and NRP-B5 - Four channel interface).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Count">
        /// This control returns number of available channels (1, 2 or 4 depending on installed options NRP-B2 - Two channel interface and NRP-B5 - Four channel interface).
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chans_getCount(out int Count)
        {
            int pInvokeResult = PInvoke.chans_getCount(this._handle, out Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function starts a single-shot measurement on all channels. The respective sensor goes to the INITIATED state. The command is completely executed when the sensor returns to the IDLE state. The command is ignored when the sensor is not in the IDLE state or when continuous measurements are selected (INIT:CONT ON). The command is only fully executed when the measurement is completed and the trigger system has again reached the IDLE state. INIT is the only remote control command that permits overlapping execution. Other commands can be received and processed while the command is being executed.
        /// 
        /// Remote-control command(s):
        /// STAT:OPER:MEAS?
        /// INIT:IMM
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chans_initiate()
        {
            int pInvokeResult = PInvoke.chans_initiate(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs zeroing using the signal at the sensor input. The sensor must be disconnected from all power sources. If the signal at the input considerably deviates from 0 W, an error message is issued and the function is aborted.
        /// 
        /// Remote-control command(s):
        /// CAL:ZERO:AUTO
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chans_zero()
        {
            int pInvokeResult = PInvoke.chans_zero(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the summary status of zeroing on all channels.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Zeroing_Completed">
        /// This control returns VI_TRUE if all channels have calibration ready.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chans_isZeroingComplete(out bool Zeroing_Completed)
        {
            ushort Zeroing_CompletedAsUShort;
            int pInvokeResult = PInvoke.chans_isZeroingComplete(this._handle, out Zeroing_CompletedAsUShort);
            Zeroing_Completed = System.Convert.ToBoolean(Zeroing_CompletedAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the summary status of measurements on all channels.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Measurement_Completed">
        /// This control returns VI_TRUE if all channels have measurement ready.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chans_isMeasurementComplete(out bool Measurement_Completed)
        {
            ushort Measurement_CompletedAsUShort;
            int pInvokeResult = PInvoke.chans_isMeasurementComplete(this._handle, out Measurement_CompletedAsUShort);
            Measurement_Completed = System.Convert.ToBoolean(Measurement_CompletedAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the sensor to one of the measurement modes.
        /// 
        /// Remote-control command(s):
        /// SENSe[1..4]:FUNCtion
        /// SENSe[1..4]:BUFFer:STATe ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Measurement_Mode">
        /// This control specifies the measurement mode to which sensor should be switched.
        /// 
        /// Valid Values:
        /// RSNRPZ_SENSOR_MODE_CONTAV (0) - ContAv (Default Value)
        /// RSNRPZ_SENSOR_MODE_BUF_CONTAV (1) - Buffered ContAv
        /// RSNRPZ_SENSOR_MODE_TIMESLOT (2) - Timeslot
        /// RSNRPZ_SENSOR_MODE_BURST (3) - Burst
        /// RSNRPZ_SENSOR_MODE_SCOPE (4) - Scope
        /// RSNRPZ_SENSOR_MODE_TIMEGATE (5) - Timegate
        /// RSNRPZ_SENSOR_MODE_CCDF (6) - CCDF
        /// RSNRPZ_SENSOR_MODE_PDF (7) - PDF
        /// 
        /// Notes:
        /// (1) ContAv: After a trigger event, the power is integrated over a time interval (Averaging).
        /// 
        /// (2) Buffered ContAv: The same as ContAv except the buffered mode is switched On.
        /// 
        /// (3) Timeslot: The power is measured simultaneously in a number of timeslots (up to 26).
        /// 
        /// (4) Burst: SENS:POW:BURS:DTOL defines the time interval during which a signal drop below the trigger level is not interpreted as the end of the burst. In the BurstAv mode, the set trigger source is ignored.
        /// 
        /// (5) Scope: A sequence of measurements is performed. The individual measured values are determined as in the ContAv mode.
        /// 
        /// (6) NRP-Z51 supports only RSNRPZ_SENSOR_MODE_CONTAV and RSNRPZ_SENSOR_MODE_BUF_CONTAV.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_mode(int Channel, int Measurement_Mode)
        {
            int pInvokeResult = PInvoke.chan_mode(this._handle, Channel, Measurement_Mode);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures times that is to be excluded at the beginning and at the end of the integration.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TIM:EXCL:STAR
        /// SENS:TIM:EXCL:STOP
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Exclude_Start">
        /// This control sets a time (in seconds) that is to be excluded at the beginning of the integration
        /// 
        /// Valid Range:
        /// NRP-Z21: 0.0 - 0.1 s
        /// FSH-Z1:  0.0 - 0.1 s
        /// 
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Exclude_Stop">
        /// This control sets a time (in seconds) that is to be excluded at the end of the integration.
        /// 
        /// Valid Range:
        /// NRP-Z21: 0.0 - 0.003 s
        /// FSH-Z1:  0.0 - 0.003 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timing_configureExclude(int Channel, double Exclude_Start, double Exclude_Stop)
        {
            int pInvokeResult = PInvoke.timing_configureExclude(this._handle, Channel, Exclude_Start, Exclude_Stop);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets a time that is to be excluded at the beginning of the integration.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TIM:EXCL:STAR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Exclude_Start">
        /// This control sets a time (in seconds) that is to be excluded at the beginning of the integration
        /// 
        /// Valid Range:
        /// NRP-Z21: 0.0 - 0.1 s
        /// FSH-Z1:  0.0 - 0.1 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timing_setTimingExcludeStart(int Channel, double Exclude_Start)
        {
            int pInvokeResult = PInvoke.timing_setTimingExcludeStart(this._handle, Channel, Exclude_Start);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads a time that is to be excluded at the beginning of the integration.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TIM:EXCL:STAR?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Exclude_Start">
        /// This control returns a time (in seconds) that is to be excluded at the beginning of the integration.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timing_getTimingExcludeStart(int Channel, out double Exclude_Start)
        {
            int pInvokeResult = PInvoke.timing_getTimingExcludeStart(this._handle, Channel, out Exclude_Start);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets a time that is to be excluded at the end of the integration.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TIM:EXCL:STOP
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Exclude_Stop">
        /// This control sets a time (in seconds) that is to be excluded at the end of the integration.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: 0.0 - 0.003 s
        /// FSH-Z1:  0.0 - 0.003 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timing_setTimingExcludeStop(int Channel, double Exclude_Stop)
        {
            int pInvokeResult = PInvoke.timing_setTimingExcludeStop(this._handle, Channel, Exclude_Stop);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads a time that is to be excluded at the end of the integration.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TIM:EXCL:STOP?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Exclude_Stop">
        /// This control returns a time (in seconds) that is to be excluded at the end of the integration.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timing_getTimingExcludeStop(int Channel, out double Exclude_Stop)
        {
            int pInvokeResult = PInvoke.timing_getTimingExcludeStop(this._handle, Channel, out Exclude_Stop);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function can be used to reduce the video bandwidth for the Trace and Statistics modes. As a result, trigger sensitivity is increased and the display noise reduced. To prevent signals from being corrupted, the selected video bandwidth should not be smaller than the RF bandwidth of the measurement signal. The "FULL" setting corresponds to a video bandwidth of at least 30 MHz if there is an associated frequency setting (SENSe:FREQuency command) greater than or equal to 500 MHz. If a frequency below 500 MHz is set and the video bandwidth is set to "FULL", the video bandwidth is automatically reduced to approx. 7.5 MHz.
        /// If the video bandwidth is limited with the SENSe:BWIDth:VIDEo command, the sampling rate is also automatically reduced, i.e. the effective time resolution in the Trace mode is reduced accordingly. In the Statistics modes, the measurement time must be increased appropriately if the required sample size is to be maintained:
        /// Video bandwidth Sampling rate   Sampling interval
        /// "Full"            8e7 1/s       12.5 ns
        /// "5 MHz"           4e7 1/s         25 ns
        /// "1.5 MHz"         1e7 1/s        100 ns
        /// "300 kHz"       2.5e6 1/s        400 ns
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// sets the video bandwidth according to a list of available bandwidths. The list can be obtained using function rsnrpz_bandwidth_getBwList.
        /// 
        /// Remote-control command(s):
        /// SENSe:BWIDth:VIDeo
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Bandwidth">
        /// This control sets the video bandwidth according to a list of available bandwidths. The list can be obtained using function rsnrpz_bandwidth_getBwList.
        /// 
        /// Valid Range:
        /// Index of bandwidth in the list
        /// 
        /// Default Value:
        /// 0
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int bandwidth_setBw(int Channel, int Bandwidth)
        {
            int pInvokeResult = PInvoke.bandwidth_setBw(this._handle, Channel, Bandwidth);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries selected video bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENSe:BWIDth:VIDeo?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Bandwidth">
        /// This control returns selected video bandwidth as index in bandwidth list.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int bandwidth_getBw(int Channel, out int Bandwidth)
        {
            int pInvokeResult = PInvoke.bandwidth_getBw(this._handle, Channel, out Bandwidth);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the list of possible video bandwidths.
        /// 
        /// Remote-control command(s):
        /// SENSe:BWIDth:VIDeo:LIST?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Buffer_Size">
        /// This control defines the size of buffer passed to Bandwidth List argument.
        /// 
        /// Valid Range:
        /// &gt; 0
        /// 
        /// Default Value:
        /// 200
        /// </param>
        /// <param name="Bandwidth_List">
        /// This control returns the comma separated list of possible video bandwidths.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int bandwidth_getBwList(int Channel, int Buffer_Size, System.Text.StringBuilder Bandwidth_List)
        {
            int pInvokeResult = PInvoke.bandwidth_getBwList(this._handle, Channel, Buffer_Size, Bandwidth_List);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures all parameters necessary for automatic detection of filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO ON
        /// SENS:AVER:COUN:AUTO:TYPE RES
        /// SENS:AVER:COUN:AUTO:RES &lt;value&gt;
        /// SENS:AVER:TCON REP
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Resolution">
        /// This control defines the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result.
        /// 
        /// Valid Range:
        /// 1 to 4
        /// 
        /// Default Value: 3
        /// 
        /// Notes:
        /// (1) Actual range depend on sensor used and may vary from the range stated above.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_configureAvgAuto(int Channel, int Resolution)
        {
            int pInvokeResult = PInvoke.avg_configureAvgAuto(this._handle, Channel, Resolution);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures all parameters necessary for setting the noise ratio in the measurement result and automatic detection of filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO ON
        /// SENS:AVER:COUN:AUTO:TYPE NSR
        /// SENS:AVER:COUN:AUTO:NSR &lt;value&gt;
        /// SENS:AVER:COUN:AUTO:MTIM &lt;value&gt;
        /// SENS:AVER:TCON REP
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Noise_Ratio">
        /// This control sets the maximum noise ratio in the measurement result. The value is set in dB.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: 0.0 - 1.0
        /// FSH-Z1:  0.0 - 1.0
        /// 
        /// Default Value: 0.1
        /// 
        /// Notes:
        /// (1) This value is not range checked; the actual range depends on sensor used.
        /// </param>
        /// <param name="Upper_Time_Limit">
        /// This control sets the upper time limit (maximum time to fill the filter).
        /// 
        /// Valid Range:
        /// 
        /// NRP-21: 0.01 - 999.99
        /// FSH-Z1: 0.01 - 999.99
        /// 
        /// Default Value: 4.0
        /// 
        /// Notes:
        /// (1) This value is not range checked, the actual range depends on sensor used.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_configureAvgNSRatio(int Channel, double Maximum_Noise_Ratio, double Upper_Time_Limit)
        {
            int pInvokeResult = PInvoke.avg_configureAvgNSRatio(this._handle, Channel, Maximum_Noise_Ratio, Upper_Time_Limit);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures all parameters necessary for manual setting of filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN
        /// SENS:AVER:COUN:AUTO OFF
        /// SENS:AVER:TCON REP
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Count">
        /// This control sets the filter bandwidth.
        /// 
        /// Valid Range:
        /// 1 - 65536
        /// 
        /// Default Value: 4
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_configureAvgManual(int Channel, int Count)
        {
            int pInvokeResult = PInvoke.avg_configureAvgManual(this._handle, Channel, Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function can be used to automatically determine a value for filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO ON|OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Enabled">
        /// This control activates or deactivates automatic determination of a value for filter bandwidth.
        /// If the automatic switchover is activated with the ON parameter, the sensor always defines a suitable filter length.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE (1) - On (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setAutoEnabled(int Channel, bool Auto_Enabled)
        {
            int pInvokeResult = PInvoke.avg_setAutoEnabled(this._handle, Channel, System.Convert.ToUInt16(Auto_Enabled));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the setting of automatic switchover of filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Enabled">
        /// This control returns the setting of automatic switchover of filter bandwidth.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getAutoEnabled(int Channel, out bool Auto_Enabled)
        {
            ushort Auto_EnabledAsUShort;
            int pInvokeResult = PInvoke.avg_getAutoEnabled(this._handle, Channel, out Auto_EnabledAsUShort);
            Auto_Enabled = System.Convert.ToBoolean(Auto_EnabledAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        ///  This function sets an upper time limit can be set via (maximum time). It should never be exceeded. Undesired long measurement times can thus be prevented if the automatic filter length switchover is on.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:MTIM
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Upper_Time_Limit">
        /// This control sets the upper time limit (maximum time to fill the filter).
        /// 
        /// Valid Range:
        /// 
        /// NRP-21: 0.01 - 999.99
        /// FSH-Z1: 0.01 - 999.99
        /// 
        /// Default Value: 4.0
        /// 
        /// Notes:
        /// (1) This value is not range checked, the actual range depends on sensor used.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setAutoMaxMeasuringTime(int Channel, double Upper_Time_Limit)
        {
            int pInvokeResult = PInvoke.avg_setAutoMaxMeasuringTime(this._handle, Channel, Upper_Time_Limit);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries an upper time limit (maximum time to fill the filter).
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:MTIM?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Upper_Time_Limit">
        /// This control returns an upper time limit (maximum time to fill the filter).
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getAutoMaxMeasuringTime(int Channel, out double Upper_Time_Limit)
        {
            int pInvokeResult = PInvoke.avg_getAutoMaxMeasuringTime(this._handle, Channel, out Upper_Time_Limit);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the maximum noise ratio in the measurement result.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:NSR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Noise_Ratio">
        /// This control sets the maximum noise ratio in the measurement result. The value is set in dB.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: 0.0 - 1.0
        /// FSH-Z1:  0.0 - 1.0
        /// 
        /// Default Value: 0.1
        /// 
        /// Notes:
        /// (1) This value is not range checked; the actual range depends on sensor used.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setAutoNoiseSignalRatio(int Channel, double Maximum_Noise_Ratio)
        {
            int pInvokeResult = PInvoke.avg_setAutoNoiseSignalRatio(this._handle, Channel, Maximum_Noise_Ratio);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the maximum noise signal ratio value.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:NSR?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Noise_Ratio">
        /// This control returns a maximum noise signal ratio in dB.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getAutoNoiseSignalRatio(int Channel, out double Maximum_Noise_Ratio)
        {
            int pInvokeResult = PInvoke.avg_getAutoNoiseSignalRatio(this._handle, Channel, out Maximum_Noise_Ratio);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result. This setting does not affect the setting of display.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:RES 1 ... 4
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Resolution">
        /// This control defines the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result.
        /// 
        /// Valid Range:
        /// 1 to 4
        /// 
        /// Default Value: 3
        /// 
        /// Notes:
        /// (1) Actual range depend on sensor used and may vary from the range stated above.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setAutoResolution(int Channel, int Resolution)
        {
            int pInvokeResult = PInvoke.avg_setAutoResolution(this._handle, Channel, Resolution);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result. 
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:RES?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Resolution">
        /// This control returns the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result.
        /// 
        /// Valid Range:
        /// 1 to 4
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getAutoResolution(int Channel, out int Resolution)
        {
            int pInvokeResult = PInvoke.avg_getAutoResolution(this._handle, Channel, out Resolution);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects a method by which the automatic filter length switchover can operate.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:TYPE RES | NSR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Method">
        /// This control selects a method by which the automatic filter length switchover can operate.
        /// 
        /// Valid Values:
        /// RSNRPZ_AUTO_COUNT_TYPE_RESOLUTION (0) - Resolution (Default Value)
        /// RSNRPZ_AUTO_COUNT_TYPE_NSR (1) - Noise Ratio
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setAutoType(int Channel, int Method)
        {
            int pInvokeResult = PInvoke.avg_setAutoType(this._handle, Channel, Method);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns a method by which the automatic filter length switchover can operate.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:TYPE?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Method">
        /// This control returns a method by which the automatic filter length switchover can operate.
        /// 
        /// Valid Values:
        /// Resolution (RSNRPZ_AUTO_COUNT_TYPE_RESOLUTION)
        /// Noise Ratio (RSNRPZ_AUTO_COUNT_TYPE_NSR)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getAutoType(int Channel, out int Method)
        {
            int pInvokeResult = PInvoke.avg_getAutoType(this._handle, Channel, out Method);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the filter bandwidth. The wider the filter the lower the noise and the longer it takes to obtain a measured value.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Count">
        /// This control sets the filter bandwidth.
        /// 
        /// Valid Range:
        /// 1 - 65536
        /// 
        /// Default Value: 4
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setCount(int Channel, int Count)
        {
            int pInvokeResult = PInvoke.avg_setCount(this._handle, Channel, Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Count">
        /// This control returns the filter bandwidth.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getCount(int Channel, out int Count)
        {
            int pInvokeResult = PInvoke.avg_getCount(this._handle, Channel, out Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches the filter function of a sensor on or off. When the filter is switched on, the number of measured values set with rsnrpz_avg_setCount() is averaged. This reduces the effect of noise so that more reliable results are obtained.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Averaging">
        /// This control switches the filter function of a sensor on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE (1)  - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setEnabled(int Channel, bool Averaging)
        {
            int pInvokeResult = PInvoke.avg_setEnabled(this._handle, Channel, System.Convert.ToUInt16(Averaging));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the state of the filter function of a sensor.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Averaging">
        /// This control returns the state of the filter function of a sensor.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getEnabled(int Channel, out bool Averaging)
        {
            ushort AveragingAsUShort;
            int pInvokeResult = PInvoke.avg_getEnabled(this._handle, Channel, out AveragingAsUShort);
            Averaging = System.Convert.ToBoolean(AveragingAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets a timeslot whose measured value is used to automatically determine the filter length.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:SLOT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timeslot">
        /// This control sets a timeslot whose measured value is used to automatically determine the filter length.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: 1 - 8
        /// FSH-Z1:  1 - 8
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setSlot(int Channel, int Timeslot)
        {
            int pInvokeResult = PInvoke.avg_setSlot(this._handle, Channel, Timeslot);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns a timeslot whose measured value is used to automatically determine the filter length.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:SLOT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timeslot">
        /// This control returns a timeslot whose measured value is used to automatically determine the filter length.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getSlot(int Channel, out int Timeslot)
        {
            int pInvokeResult = PInvoke.avg_getSlot(this._handle, Channel, out Timeslot);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function determines whether a new result is calculated immediately after a new measured value is available or only after an entire range of new values is available for the filter.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:TCON MOV | REP
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Terminal_Control">
        /// This control determines the type of terminal control.
        /// 
        /// Valid Values:
        /// RSNRPZ_TERMINAL_CONTROL_MOVING (0) - Moving
        /// RSNRPZ_TERMINAL_CONTROL_REPEAT (1) - Repeat (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_setTerminalControl(int Channel, int Terminal_Control)
        {
            int pInvokeResult = PInvoke.avg_setTerminalControl(this._handle, Channel, Terminal_Control);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the type of terminal control.
        /// 
        /// Remote-control command(s):
        /// SENSe[1..4]:AVERage:TCONtrol?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Terminal_Control">
        /// This control returns the type of terminal control.
        /// 
        /// Valid Values:
        /// RSNRPZ_TERMINAL_CONTROL_MOVING (0) - Moving
        /// RSNRPZ_TERMINAL_CONTROL_REPEAT (1) - Repeat (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_getTerminalControl(int Channel, out int Terminal_Control)
        {
            int pInvokeResult = PInvoke.avg_getTerminalControl(this._handle, Channel, out Terminal_Control);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function initializes the digital filter by deleting the stored measured values.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:RES
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int avg_reset(int Channel)
        {
            int pInvokeResult = PInvoke.avg_reset(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the automatic selection of a measurement range to ON or OFF.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:RANG:AUTO ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Range">
        /// This control sets the automatic selection of a measurement range to ON or OFF.
        /// 
        /// Valid Values:
        /// VI_TRUE (1)  - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int range_setAutoEnabled(int Channel, bool Auto_Range)
        {
            int pInvokeResult = PInvoke.range_setAutoEnabled(this._handle, Channel, System.Convert.ToUInt16(Auto_Range));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the state of automatic selection of a measurement range.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:RANG:AUTO?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Range">
        /// This control returns the state of automatic selection of a measurement range.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int range_getAutoEnabled(int Channel, out bool Auto_Range)
        {
            ushort Auto_RangeAsUShort;
            int pInvokeResult = PInvoke.range_getAutoEnabled(this._handle, Channel, out Auto_RangeAsUShort);
            Auto_Range = System.Convert.ToBoolean(Auto_RangeAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the cross-over level. Shifts the transition ranges between the measurement ranges. This may improve the measurement accuracy for special signals, i.e. signals with a high crest factor.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:RANG:AUTO:CLEV
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Crossover_Level">
        /// This control sets the cross-over level in dB.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: -20.0 - 0.0 dB
        /// FSH-Z1:  -20.0 - 0.0 dB
        /// 
        /// Default Value: 0.0 dB
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int range_setCrossoverLevel(int Channel, double Crossover_Level)
        {
            int pInvokeResult = PInvoke.range_setCrossoverLevel(this._handle, Channel, Crossover_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the cross-over level.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:RANG:AUTO:CLEV?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Crossover_Level">
        /// This control returns the cross-over level in dB.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors 
        /// </returns>
        public int range_getCrossoverLevel(int Channel, out double Crossover_Level)
        {
            int pInvokeResult = PInvoke.range_getCrossoverLevel(this._handle, Channel, out Crossover_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects a measurement range in which the corresponding sensor is to perform a measurement.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:RANG 0 .. 2
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Range">
        /// This control selects a measurement range in which the corresponding sensor is to perform a measurement.
        /// 
        /// Valid Range:
        /// NRP-Z21:  0 to 2
        /// FSH-1:    0 to 2
        /// 
        /// Default Value: 2
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int range_setRange(int Channel, int Range)
        {
            int pInvokeResult = PInvoke.range_setRange(this._handle, Channel, Range);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns a measurement range in which the corresponding sensor is to perform a measurement.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51
        /// 
        /// Remote-control command(s):
        /// SENS:RANG?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Range">
        /// This control returns a measurement range in which the corresponding sensor is to perform a measurement.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int range_getRange(int Channel, out int Range)
        {
            int pInvokeResult = PInvoke.range_getRange(this._handle, Channel, out Range);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures all correction parameters.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:OFFS
        /// SENS:CORR:OFFS:STAT ON | OFF
        /// SENS:CORR:SPD:STAT ON | OFF
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset_State">
        /// This control switches the offset correction on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <param name="Offset">
        /// This control sets a fixed offset value can be defined for multiplying (logarithmically adding) the measured value of a sensor.
        /// 
        /// Valid Range:
        ///   -200.0 to 200.0 dB
        /// 
        /// 
        /// Default Value:
        /// 0.0 dB
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Reserved_1">
        /// This prameter is reserved. Value is ignored.
        /// </param>
        /// <param name="Reserved_2">
        /// This prameter is reserved. Value is ignored.
        /// 
        /// Default Value:
        /// ""
        /// </param>
        /// <param name="S_Parameter_Enable">
        /// This control enables or disables measured-value correction by means of the stored s-parameter device.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_configureCorrections(int Channel, bool Offset_State, double Offset, bool Reserved_1, string Reserved_2, bool S_Parameter_Enable)
        {
            int pInvokeResult = PInvoke.corr_configureCorrections(this._handle, Channel, System.Convert.ToUInt16(Offset_State), Offset, System.Convert.ToUInt16(Reserved_1), Reserved_2, System.Convert.ToUInt16(S_Parameter_Enable));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function informs the R&amp;S NRP about the frequency of the power to be measured since this frequency is not automatically determined. The frequency is used to determine a frequency-dependent correction factor for the measurement results.
        /// 
        /// Remote-control command(s):
        /// SENS:FREQ
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Frequency">
        /// This control sets the frequency in Hz of the power to be measured since this frequency is not automatically determined.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: 10.0e6 - 18.0e9
        /// FSH-Z1:  10.0e6 -  8.0e9
        /// NRP-Z51: 0.0    - 18.0e9 (depends on the calibration data)
        /// 
        /// Default Value: 50.0e6 Hz
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setCorrectionFrequency(int Channel, double Frequency)
        {
            int pInvokeResult = PInvoke.chan_setCorrectionFrequency(this._handle, Channel, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the instrument for the frequency of the power to be measured since this frequency is not automatically determined.
        /// 
        /// Remote-control command(s):
        /// SENSe[1..4]:FREQuency?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Frequency">
        /// This control returns the frequency in Hz of the power to be measured since this frequency is not automatically determined.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getCorrectionFrequency(int Channel, out double Frequency)
        {
            int pInvokeResult = PInvoke.chan_getCorrectionFrequency(this._handle, Channel, out Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// With this function a fixed offset value can be defined for multiplying (logarithmically adding) the measured value of a sensor.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:OFFS
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control sets a fixed offset value can be defined for multiplying (logarithmically adding) the measured value of a sensor.
        /// 
        /// Valid Range:
        ///   -200.0 to 200.0 dB
        /// 
        /// Default Value:
        /// 0.0 dB
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_setOffset(int Channel, double Offset)
        {
            int pInvokeResult = PInvoke.corr_setOffset(this._handle, Channel, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets a fixed offset value defined for multiplying (logarithmically adding) the measured value of a sensor.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:OFFS?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control returns a fixed offset value defined for multiplying (logarithmically adding) the measured value of a sensor.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_getOffset(int Channel, out double Offset)
        {
            int pInvokeResult = PInvoke.corr_getOffset(this._handle, Channel, out Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches the offset correction on or off.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:OFFS:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset_State">
        /// This control switches the offset correction on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE (1)  - On 
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_setOffsetEnabled(int Channel, bool Offset_State)
        {
            int pInvokeResult = PInvoke.corr_setOffsetEnabled(this._handle, Channel, System.Convert.ToUInt16(Offset_State));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the offset correction on or off.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:OFFS:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset_State">
        /// This control returns the offset correction on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_getOffsetEnabled(int Channel, out bool Offset_State)
        {
            ushort Offset_StateAsUShort;
            int pInvokeResult = PInvoke.corr_getOffsetEnabled(this._handle, Channel, out Offset_StateAsUShort);
            Offset_State = System.Convert.ToBoolean(Offset_StateAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function instructs the sensor to perform a measured-value correction by means of the stored s-parameter device.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:SPD:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="S_Parameter_Enable">
        /// This control enables or disables measured-value correction by means of the stored s-parameter device.
        /// 
        /// Valid Values:
        /// VI_TRUE (1)  - On
        /// VI_FALSE (0) - Off (Default Value)
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_setSParamDeviceEnabled(int Channel, bool S_Parameter_Enable)
        {
            int pInvokeResult = PInvoke.corr_setSParamDeviceEnabled(this._handle, Channel, System.Convert.ToUInt16(S_Parameter_Enable));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the state of a measured-value correction by means of the stored s-parameter device.
        /// 
        /// Remote-control command(s):
        /// SENSe[1..4]:CORRection:SPDevice:STATe?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="S_Parameter_Correction">
        /// This control returns the state of S-Parameter correction.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_getSParamDeviceEnabled(int Channel, out bool S_Parameter_Correction)
        {
            ushort S_Parameter_CorrectionAsUShort;
            int pInvokeResult = PInvoke.corr_getSParamDeviceEnabled(this._handle, Channel, out S_Parameter_CorrectionAsUShort);
            S_Parameter_Correction = System.Convert.ToBoolean(S_Parameter_CorrectionAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function can be used to select a loaded data set for S-parameter correction. This data set is accessed by means of a consecutive number, starting with 1 for the first data set. If an invalid data set consecutive number is entered, an error message is output.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only on NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:SPD:SEL
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="S_Parameter">
        /// This control can be used to select a loaded data set for S-parameter correction. This data set is accessed by means of a consecutive number, starting with 1 for the first data set. If an invalid data set consecutive number is entered, an error message is output.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_setSParamDevice(int Channel, int S_Parameter)
        {
            int pInvokeResult = PInvoke.corr_setSParamDevice(this._handle, Channel, S_Parameter);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets selected data set for S-parameter correction. 
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only on NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:SPD:SEL?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="S_Parameter">
        /// This control returns selected data set for S-parameter correction. 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_getSParamDevice(int Channel, out int S_Parameter)
        {
            int pInvokeResult = PInvoke.corr_getSParamDevice(this._handle, Channel, out S_Parameter);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the parameters of the reflection coefficient for measured-value correction.
        /// 
        /// Remote-control command(s):
        /// SENS:SGAM
        /// SENS:SGAM:PHAS
        /// SENS:SGAM:CORR:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Source_Gamma_Correction">
        /// This control enables or disables source gamma correction of the measured value.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <param name="Magnitude">
        /// This control sets the magnitude of the reflection coefficient.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21 0.0 - 1.0
        /// FSH-Z1: 0.0 - 1.0
        /// 
        /// Default Value: 1.0
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Phase">
        /// This control defines the phase angle of the reflection coefficient. Units are degrees.
        /// 
        /// Valid Range:
        /// -360.0 to 360.0 deg
        /// 
        /// Default Value:
        /// 0.0 deg
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_configureSourceGammaCorr(int Channel, bool Source_Gamma_Correction, double Magnitude, double Phase)
        {
            int pInvokeResult = PInvoke.chan_configureSourceGammaCorr(this._handle, Channel, System.Convert.ToUInt16(Source_Gamma_Correction), Magnitude, Phase);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the magnitude of the reflection coefficient for measured-value correction.
        /// 
        /// Remote-control command(s):
        /// SENS:SGAM:MAGN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Magnitude">
        /// This control sets the magnitude of the reflection coefficient.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21 0.0 - 1.0
        /// FSH-Z1: 0.0 - 1.0
        /// 
        /// Default Value: 1.0
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setSourceGammaMagnitude(int Channel, double Magnitude)
        {
            int pInvokeResult = PInvoke.chan_setSourceGammaMagnitude(this._handle, Channel, Magnitude);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the magnitude of the reflection coefficient for measured-value correction.
        /// 
        /// Remote-control command(s):
        /// SENS:SGAM:MAGN?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Magnitude">
        /// This control returns the magnitude of the reflection coefficient for measured-value correction.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getSourceGammaMagnitude(int Channel, out double Magnitude)
        {
            int pInvokeResult = PInvoke.chan_getSourceGammaMagnitude(this._handle, Channel, out Magnitude);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the phase angle of the reflection coefficient for measured-value correction.
        /// 
        /// Remote-control command(s):
        /// SENS:SGAM:PHAS
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Phase">
        /// This control defines the phase angle of the reflection coefficient. Units are degrees.
        /// 
        /// Valid Range:
        /// -360.0 to 360.0 deg
        /// 
        /// Default Value:
        /// 0.0 deg
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setSourceGammaPhase(int Channel, double Phase)
        {
            int pInvokeResult = PInvoke.chan_setSourceGammaPhase(this._handle, Channel, Phase);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the phase angle of the reflection coefficient for measured-value correction.
        /// 
        /// Remote-control command(s):
        /// SENS:SGAM:PHAS?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Phase">
        /// This control returns the phase angle of the reflection coefficient. Units are degrees.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getSourceGammaPhase(int Channel, out double Phase)
        {
            int pInvokeResult = PInvoke.chan_getSourceGammaPhase(this._handle, Channel, out Phase);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches the measured-value correction of the reflection coefficient effect of the source gamma ON or OFF.
        /// 
        /// Remote-control command(s):
        /// SENS:SGAM:CORR:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Source_Gamma_Correction">
        /// This control enables or disables source gamma correction of the measured value.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setSourceGammaCorrEnabled(int Channel, bool Source_Gamma_Correction)
        {
            int pInvokeResult = PInvoke.chan_setSourceGammaCorrEnabled(this._handle, Channel, System.Convert.ToUInt16(Source_Gamma_Correction));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the state of source gamma correction.
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:CORR:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Source_Gamma_Correction">
        /// This control returns the state of source gamma correction.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getSourceGammaCorrEnabled(int Channel, out bool Source_Gamma_Correction)
        {
            ushort Source_Gamma_CorrectionAsUShort;
            int pInvokeResult = PInvoke.chan_getSourceGammaCorrEnabled(this._handle, Channel, out Source_Gamma_CorrectionAsUShort);
            Source_Gamma_Correction = System.Convert.ToBoolean(Source_Gamma_CorrectionAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the parameters of the compensation of the load distortion at the signal output.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM
        /// SENS:RGAM:PHAS
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Magnitude">
        /// This control sets the magnitude of the reflection coefficient of the load for distortion compensation.
        /// 
        /// Valid Range:
        /// 0.0 - 1.0
        /// 
        /// Default Value: 0.0
        /// </param>
        /// <param name="Phase">
        /// This control defines the phase angle (in degrees) of the complex reflection factor of the load at the signal output.
        /// 
        /// Valid Range:
        /// -360.0 to 360.0 deg
        /// 
        /// Default Value:
        /// 0.0 deg
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_configureReflectGammaCorr(int Channel, double Magnitude, double Phase)
        {
            int pInvokeResult = PInvoke.chan_configureReflectGammaCorr(this._handle, Channel, Magnitude, Phase);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the magnitude of the reflection coefficient of the load for distortion compensation.
        /// To deactivate distortion compensation, set Magnitude to 0. Distortion compensation should remain deactivated in the case of questionable measurement values for the reflection coefficient of the load.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:MAGN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Magnitude">
        /// This control sets the magnitude of the reflection coefficient of the load for distortion compensation.
        /// 
        /// Valid Range:
        /// 0.0 - 1.0
        /// 
        /// Default Value: 0.0
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setReflectionGammaMagn(int Channel, double Magnitude)
        {
            int pInvokeResult = PInvoke.chan_setReflectionGammaMagn(this._handle, Channel, Magnitude);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the magnitude of the reflection coefficient of the load for distortion compensation.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:MAGN?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Magnitude">
        /// This control returns the magnitude of the reflection coefficient of the load for distortion compensation.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getReflectionGammaMagn(int Channel, out double Magnitude)
        {
            int pInvokeResult = PInvoke.chan_getReflectionGammaMagn(this._handle, Channel, out Magnitude);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines the phase angle (in degrees) of the complex reflection factor of the load at the signal output.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:PHAS
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Phase">
        /// This control defines the phase angle (in degrees) of the complex reflection factor of the load at the signal output.
        /// 
        /// Valid Range:
        /// -360.0 to 360.0 deg
        /// 
        /// Default Value:
        /// 0.0 deg
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setReflectionGammaPhase(int Channel, double Phase)
        {
            int pInvokeResult = PInvoke.chan_setReflectionGammaPhase(this._handle, Channel, Phase);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the phase angle (in degrees) of the complex reflection factor of the load at the signal output.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:PHAS?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Phase">
        /// This control returns the phase angle (in degrees) of the complex reflection factor of the load at the signal output.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getReflectionGammaPhase(int Channel, out double Phase)
        {
            int pInvokeResult = PInvoke.chan_getReflectionGammaPhase(this._handle, Channel, out Phase);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines reflection gamma uncertainty.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:EUNC
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Uncertainty">
        /// This control defines the uncertainty.
        /// 
        /// Valid Range:
        /// 0.0 to 1.0
        /// 
        /// Default Value:
        /// 0.0
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setReflectionGammaUncertainty(int Channel, double Uncertainty)
        {
            int pInvokeResult = PInvoke.chan_setReflectionGammaUncertainty(this._handle, Channel, Uncertainty);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries reflection gamma uncertainty.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is available only for sensors NRP-Z27 and NRP-Z37
        /// 
        /// Remote-control command(s):
        /// SENS:RGAM:EUNC?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Uncertainty">
        /// This control returns the uncertainty.
        /// 
        /// Valid Range:
        /// 0.0 to 1.0
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getReflectionGammaUncertainty(int Channel, out double Uncertainty)
        {
            int pInvokeResult = PInvoke.chan_getReflectionGammaUncertainty(this._handle, Channel, out Uncertainty);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures all duty cycle parameters.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:DCYC
        /// SENS:CORR:DCYC:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Duty_Cycle_State">
        /// This control switches measured-value correction for a specific duty cycle on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <param name="Duty_Cycle">
        /// This control sets the duty cycle of power to be measured.
        /// 
        /// Valid Range:
        /// 0.001 - 99.999%
        /// 
        /// Default Value: 1.0 %
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_configureDutyCycle(int Channel, bool Duty_Cycle_State, double Duty_Cycle)
        {
            int pInvokeResult = PInvoke.corr_configureDutyCycle(this._handle, Channel, System.Convert.ToUInt16(Duty_Cycle_State), Duty_Cycle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function informs the R&amp;S NRP about the duty cycle of the power to be measured. Specifying a duty cycle only makes sense in the ContAv mode where measurements are performed continuously without taking the timing of the signal into account. For this reason, this setting can only be chosen in the local mode when the sensor performs measurements in the ContAv mode.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:DCYC
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Duty_Cycle">
        /// This control sets the duty cycle of power to be measured.
        /// 
        /// Valid Range:
        /// 0.001 - 99.999 %
        /// 
        /// Default Value: 1.0 %
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_setDutyCycle(int Channel, double Duty_Cycle)
        {
            int pInvokeResult = PInvoke.corr_setDutyCycle(this._handle, Channel, Duty_Cycle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets the size of duty cycle of the power to be measured.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:DCYC?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Duty_Cycle">
        /// This control returns the size of duty cycle of the power to be measured. Units are %.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_getDutyCycle(int Channel, out double Duty_Cycle)
        {
            int pInvokeResult = PInvoke.corr_getDutyCycle(this._handle, Channel, out Duty_Cycle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches measured-value correction for a specific duty cycle on or off.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:DCYC:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Duty_Cycle_State">
        /// This control switches measured-value correction for a specific duty cycle on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_setDutyCycleEnabled(int Channel, bool Duty_Cycle_State)
        {
            int pInvokeResult = PInvoke.corr_setDutyCycleEnabled(this._handle, Channel, System.Convert.ToUInt16(Duty_Cycle_State));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets the setting of duty cycle.
        /// 
        /// Remote-control command(s):
        /// SENS:CORR:DCYC:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Duty_Cycle_State">
        /// This control returns the state of duty cycle.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int corr_getDutyCycleEnabled(int Channel, out bool Duty_Cycle_State)
        {
            ushort Duty_Cycle_StateAsUShort;
            int pInvokeResult = PInvoke.corr_getDutyCycleEnabled(this._handle, Channel, out Duty_Cycle_StateAsUShort);
            Duty_Cycle_State = System.Convert.ToBoolean(Duty_Cycle_StateAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function determines the integration time for a single measurement in the ContAv mode. To increase the measurement accuracy, this integration is followed by a second averaging procedure in a window with a selectable number of values.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:APER
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="ContAv_Aperture">
        /// This control defines the ContAv Aperture in seconds.
        /// 
        /// Valid Range:
        /// NRP-Z21:   0.1e-6 to 0.3 seconds
        /// NRP-Z51:   0.1e-3 to 0.3 seconds
        /// FSH-Z1:    0.1e-6 to 0.3 seconds
        /// 
        /// Default Value: 0.02 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setContAvAperture(int Channel, double ContAv_Aperture)
        {
            int pInvokeResult = PInvoke.chan_setContAvAperture(this._handle, Channel, ContAv_Aperture);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the value of ContAv mode aperture size.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:APER?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="ContAv_Aperture">
        /// This control returns the ContAv Aperture size in seconds.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getContAvAperture(int Channel, out double ContAv_Aperture)
        {
            int pInvokeResult = PInvoke.chan_getContAvAperture(this._handle, Channel, out ContAv_Aperture);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function activates digital lowpass filtering of the sampled video signal.
        /// The problem of instable display values due to a modulation of a test signal can also be eliminated by lowpass filtering of the video signal. The lowpass filter eliminates the variations of the display even in case of unperiodic modulation and does not require any other setting.
        /// If the modulation is periodic, the setting of the sampling window is the better method since it allows for shorter measurement times.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:SMO:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="ContAv_Smoothing">
        /// This control sets the state of digital lowpass filtering of the sampled video signal.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setContAvSmoothingEnabled(int Channel, bool ContAv_Smoothing)
        {
            int pInvokeResult = PInvoke.chan_setContAvSmoothingEnabled(this._handle, Channel, System.Convert.ToUInt16(ContAv_Smoothing));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets the state of digital lowpass filtering of the sampled video signal.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:SMO:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="ContAv_Smoothing">
        /// This control returns the state of digital lowpass filtering of the sampled video signal.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getContAvSmoothingEnabled(int Channel, out bool ContAv_Smoothing)
        {
            ushort ContAv_SmoothingAsUShort;
            int pInvokeResult = PInvoke.chan_getContAvSmoothingEnabled(this._handle, Channel, out ContAv_SmoothingAsUShort);
            ContAv_Smoothing = System.Convert.ToBoolean(ContAv_SmoothingAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches on the buffered ContAv mode, after which data blocks rather than single measured values are then  returned. In this mode a higher data rate is achieved than in the non-buffered ContAv mode.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:BUFF:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="ContAv_Buffered_Mode">
        /// This control turns on or off ContAv buffered measurement mode.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setContAvBufferedEnabled(int Channel, bool ContAv_Buffered_Mode)
        {
            int pInvokeResult = PInvoke.chan_setContAvBufferedEnabled(this._handle, Channel, System.Convert.ToUInt16(ContAv_Buffered_Mode));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the state of ContAv Buffered Measurement Mode.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:BUFF:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="ContAv_Buffered_Mode">
        /// This control returns the state of ContAv Buffered Measurement Mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getContAvBufferedEnabled(int Channel, out bool ContAv_Buffered_Mode)
        {
            ushort ContAv_Buffered_ModeAsUShort;
            int pInvokeResult = PInvoke.chan_getContAvBufferedEnabled(this._handle, Channel, out ContAv_Buffered_ModeAsUShort);
            ContAv_Buffered_Mode = System.Convert.ToBoolean(ContAv_Buffered_ModeAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the number of desired values for the buffered ContAv mode.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:BUFF:SIZE
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Buffer_Size">
        /// This control sets the number of desired values for buffered ContAv mode.
        /// 
        /// Valid Range:
        /// 1 to 1024
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setContAvBufferSize(int Channel, int Buffer_Size)
        {
            int pInvokeResult = PInvoke.chan_setContAvBufferSize(this._handle, Channel, Buffer_Size);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the number of desired values for the buffered ContAv mode.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:AVG:BUFF:SIZE?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Buffer_Size">
        /// This control returns the number of desired values for the buffered ContAv mode.
        /// 
        /// Valid Range:
        /// 1 to 400000
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getContAvBufferSize(int Channel, out int Buffer_Size)
        {
            int pInvokeResult = PInvoke.chan_getContAvBufferSize(this._handle, Channel, out Buffer_Size);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// The end of a burst (power pulse) is recognized when the signal level drops below the trigger level. Especially with modulated signals, this may also happen for a short time within a burst. To prevent the supposed end of the burst is from being recognized too early or incorrectly at these positions, a time interval can be defined via using this function (drop-out tolerance parameter) in which the pulse end is only recognized if the signal level no longer exceeds the trigger level.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51
        /// 
        /// Remote-control command(s):
        /// SENS:POW:BURS:DTOL
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Drop_out_Tolerance">
        /// This parameter defines the Drop-Out Tolerance time interval in seconds.
        /// 
        /// Valid Range:
        /// 0.0 to 3.0e-3 seconds
        /// 
        /// Default Value: 100.0e-6 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int chan_setBurstDropoutTolerance(int Channel, double Drop_out_Tolerance)
        {
            int pInvokeResult = PInvoke.chan_setBurstDropoutTolerance(this._handle, Channel, Drop_out_Tolerance);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the drop-out tolerance parameter.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51
        /// 
        /// Remote-control command(s):
        /// SENS:POW:BURS:DTOL?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Drop_out_Tolerance">
        /// This control returns the drop-out tolerance parameter.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getBurstDropoutTolerance(int Channel, out double Drop_out_Tolerance)
        {
            int pInvokeResult = PInvoke.chan_getBurstDropoutTolerance(this._handle, Channel, out Drop_out_Tolerance);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function enables or disables the chopper in BurstAv mode. Disabling the chopper is only good for fast but unexact and noisy measurements. If the chopper is disabled, averaging is ignored internally also disabled.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:BURSt:CHOPper:STATe
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="BurstAv_Chopper">
        /// This control enables or disables the chopper for BurstAv mode.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setBurstChopperEnabled(int Channel, bool BurstAv_Chopper)
        {
            int pInvokeResult = PInvoke.chan_setBurstChopperEnabled(this._handle, Channel, System.Convert.ToUInt16(BurstAv_Chopper));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the state of the chopper in BurstAv mode.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:BURSt:CHOPper:STATe?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="BurstAv_Chopper">
        /// This control returns the state of the chopper for BurstAv mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getBurstChopperEnabled(int Channel, out bool BurstAv_Chopper)
        {
            ushort BurstAv_ChopperAsUShort;
            int pInvokeResult = PInvoke.chan_getBurstChopperEnabled(this._handle, Channel, out BurstAv_ChopperAsUShort);
            BurstAv_Chopper = System.Convert.ToBoolean(BurstAv_ChopperAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines the time gate measured by the sensor.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:OFFSet:TIME
        /// SENSe:POWer:TGATe[1...16]:FREQuency
        /// SENSe:POWer:TGATe[1...16]:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Offset">
        /// This control is used to specify the start of a timegate for recording measured values in the Timegate Average mode. The start time is referenced to the delayed trigger point (TRIGger:DELay command). Only positive values are valid. If the start of the timegate is before the physical trigger point, the trigger delay must be set to a negative value with an appropriately large magnitude (minimum 51.2 us).
        /// 
        /// Valid Range:
        /// 0.0 to 10.0
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <param name="Time">
        /// This control sets the length of a timegate for recording measured values in the Timegate Average mode.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <param name="Frequency">
        /// This control transfers the carrier frequency of the RF signal to be measured in the timegate indicated. When the R&amp;S NRP-Z81 power sensor is used, it is essential that the current carrier frequency is set. Otherwise non-linearities and temperature dependencies with values considerably higher than those stated in the data sheet would occur. If the frequency which has been entered is below 500 MHz, the video bandwidth of the power sensor is reduced automatically. The center frequency is set for broadband signals (spread-spectrum signals, multicarrier signals), if there is no explicit carrier.
        /// 
        /// Valid Range:
        /// 50.0e6 to 18.0e9 Hz
        /// 
        /// Default Value: 1.0e9 Hz
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_configureTimeGate(int Channel, int Select_Gate, double Offset, double Time, double Frequency)
        {
            int pInvokeResult = PInvoke.timegate_configureTimeGate(this._handle, Channel, Select_Gate, Offset, Time, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function is used to specify the start of a timegate for recording measured values in the Timegate Average mode. The start time is referenced to the delayed trigger point (TRIGger:DELay command). Only positive values are valid. If the start of the timegate is before the physical trigger point, the trigger delay must be set to a negative value with an appropriately large magnitude (minimum 51.2 us).
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:OFFSet:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Offset">
        /// This control sets the start of the timegate after the trigger.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_setOffsetTime(int Channel, int Select_Gate, double Offset)
        {
            int pInvokeResult = PInvoke.timegate_setOffsetTime(this._handle, Channel, Select_Gate, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the start of the timegate after the trigger.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:OFFSet:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Offset">
        /// This control returns the start of the timegate after the trigger.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_getOffsetTime(int Channel, int Select_Gate, out double Offset)
        {
            int pInvokeResult = PInvoke.timegate_getOffsetTime(this._handle, Channel, Select_Gate, out Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the length of a timegate for recording measured values in the Timegate Average mode.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Time">
        /// This control sets the length of a timegate for recording measured values in the Timegate Average mode.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_setTime(int Channel, int Select_Gate, double Time)
        {
            int pInvokeResult = PInvoke.timegate_setTime(this._handle, Channel, Select_Gate, Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the length of the timegate.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Time">
        /// This control returns the length of the timegate.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_getTime(int Channel, int Select_Gate, out double Time)
        {
            int pInvokeResult = PInvoke.timegate_getTime(this._handle, Channel, Select_Gate, out Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function transfers the carrier frequency of the RF signal to be measured in the timegate indicated. When the R&amp;S NRP-Z81 power sensor is used, it is essential that the current carrier frequency is set. Otherwise non-linearities and temperature dependencies with values considerably higher than those stated in the data sheet would occur. If the frequency which has been entered is below 500 MHz, the video bandwidth of the power sensor is reduced automatically. The center frequency is set for broadband signals (spread-spectrum signals, multicarrier signals), if there is no explicit carrier.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:FREQuency
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Frequency">
        /// This control transfers the carrier frequency of the RF signal to be measured in the timegate indicated. When the R&amp;S NRP-Z81 power sensor is used, it is essential that the current carrier frequency is set. Otherwise non-linearities and temperature dependencies with values considerably higher than those stated in the data sheet would occur. If the frequency which has been entered is below 500 MHz, the video bandwidth of the power sensor is reduced automatically. The center frequency is set for broadband signals (spread-spectrum signals, multicarrier signals), if there is no explicit carrier.
        /// 
        /// Valid Range:
        /// 50.0e6 to 18.0e9 Hz
        /// 
        /// Default Value: 1.0e9 Hz
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_setFrequency(int Channel, int Select_Gate, double Frequency)
        {
            int pInvokeResult = PInvoke.timegate_setFrequency(this._handle, Channel, Select_Gate, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the carrier frequency of the RF signal to be measured in the timegate indicated.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:FREQuency?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Frequency">
        /// This control returns the signal's frequency for timegate.
        /// 
        /// Valid Range:
        /// 50.0e6 to 18.0e9 Hz
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_getFrequency(int Channel, int Select_Gate, out double Frequency)
        {
            int pInvokeResult = PInvoke.timegate_getFrequency(this._handle, Channel, Select_Gate, out Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function is used to define the start of an exclusion interval in the specified timegate. The start of the exclusion interval is referenced to the start of the timegate.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:EXCLude:MID:OFFSet
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Offset">
        /// This control is used to define the start of an exclusion interval in the specified timegate. The start of the exclusion interval is referenced to the start of the timegate.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_setMidOffset(int Channel, int Select_Gate, double Offset)
        {
            int pInvokeResult = PInvoke.timegate_setMidOffset(this._handle, Channel, Select_Gate, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the midamble offset after timeslot start in seconds in the Timegate mode.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:EXCLude:MID:OFFSet?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Offset">
        /// This control returns the mid offset of time slot in seconds of the timeslot in the Timegate mode.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_getMidOffset(int Channel, int Select_Gate, out double Offset)
        {
            int pInvokeResult = PInvoke.timegate_getMidOffset(this._handle, Channel, Select_Gate, out Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function is used to specify the length of an exclusion interval.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:EXCLude:MID:TIME
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Length">
        /// This control is used to specify the length of an exclusion interval.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_setMidLength(int Channel, int Select_Gate, double Length)
        {
            int pInvokeResult = PInvoke.timegate_setMidLength(this._handle, Channel, Select_Gate, Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the length of an exclusion interval.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe[1...16]:EXCLude:MID:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Select_Gate">
        /// This control selects the gate.
        /// 
        /// Valid Values:
        /// 1 to 16
        /// 
        /// Default Value:
        /// 1
        /// </param>
        /// <param name="Length">
        /// This control returns the length of an exclusion interval.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_getMidLength(int Channel, int Select_Gate, out double Length)
        {
            int pInvokeResult = PInvoke.timegate_getMidLength(this._handle, Channel, Select_Gate, out Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function enables or disables the chopper stabilization for the Timegate Average mode and to deactivate averaging. Disabling means that each defined timegate is measured only once, providing the shortest possible measurement time for the signal and also making it possible to analyze one-off events. Only high power values (from approx. -30 dBm) should be measured in this mode because of the elevated noise component and the considerable zero drift.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe:CHOPper:STATe
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timegate_Chopper">
        /// This control enables or disables the chopper for Timegate mode.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_setChopperEnabled(int Channel, bool Timegate_Chopper)
        {
            int pInvokeResult = PInvoke.timegate_setChopperEnabled(this._handle, Channel, System.Convert.ToUInt16(Timegate_Chopper));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the state of the chopper in Timegate mode.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TGATe:CHOPper:STATe?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timegate_Chopper">
        /// This control returns the state of the chopper for Timegate mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int timegate_getChopperEnabled(int Channel, out bool Timegate_Chopper)
        {
            ushort Timegate_ChopperAsUShort;
            int pInvokeResult = PInvoke.timegate_getChopperEnabled(this._handle, Channel, out Timegate_ChopperAsUShort);
            Timegate_Chopper = System.Convert.ToBoolean(Timegate_ChopperAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the timegate (depends on trigger event) in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:OFFSet:TIME
        /// SENSe:STATistics:TIME
        /// SENSe:STATistics:[EXCLude]:MID:OFFSet[:TIME]
        /// SENSe:STATistics:[EXCLude]:MID:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control sets the start after the trigger of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Valid Range:
        /// 
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <param name="Time">
        /// This control sets the length of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Valid Range:
        /// 1.0E-6 to 0.3 s
        /// 
        /// Default Value: 0.01 s
        /// </param>
        /// <param name="Midamble_Offset">
        /// This control sets the midamble offset after timeslot start in seconds in the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <param name="Midamble_Length">
        /// This control sets the midamble length in seconds.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_confTimegate(int Channel, double Offset, double Time, double Midamble_Offset, double Midamble_Length)
        {
            int pInvokeResult = PInvoke.stat_confTimegate(this._handle, Channel, Offset, Time, Midamble_Offset, Midamble_Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the X-Axis of statistical measurement.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:RLEVel
        /// SENSe:STATistics:SCALE:X:RANGe
        /// SENSe:STATistics:SCALE:X:POINts
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Reference_Level">
        /// This control sets the lower limit of the level range for the analysis result in both Statistics modes. This level can be assigned to the first pixel. The level assigned to the last pixel is equal to the level of the first pixel plus the level range.
        /// 
        /// Valid Range:
        /// -80.0 to 20.0 dB
        /// 
        /// Default Value: -30.0 dB
        /// </param>
        /// <param name="Range">
        /// This control sets the width of the level range for the analysis result in both Statistics modes.
        /// 
        /// Valid Range:
        /// 0.01 to 100.0
        /// 
        /// Default Value: 50.0
        /// </param>
        /// <param name="Points">
        /// This control sets the measurement-result resolution in both Statistics modes. This function specifies the number of pixels that are to be assigned to the logarithmic level range (rsnrpz_stat_setScaleRange function) for measured value output. The width of the level range divided by N-1, where N is the number of pixels, must not be less than the value which can be read out with rsnrpz_stat_getScaleWidth.
        /// 
        /// Valid Range:
        /// 3 to 8192
        /// 
        /// Default Value: 200
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_confScale(int Channel, double Reference_Level, double Range, int Points)
        {
            int pInvokeResult = PInvoke.stat_confScale(this._handle, Channel, Reference_Level, Range, Points);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the start after the trigger of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:OFFSet:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control sets the start after the trigger of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Valid Range:
        /// 
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setOffsetTime(int Channel, double Offset)
        {
            int pInvokeResult = PInvoke.stat_setOffsetTime(this._handle, Channel, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the start after the trigger of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:OFFSet:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control returns the start after the trigger of the timegate in which the sensor is doing statistic measurements.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getOffsetTime(int Channel, out double Offset)
        {
            int pInvokeResult = PInvoke.stat_getOffsetTime(this._handle, Channel, out Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the length of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time">
        /// This control sets the length of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Valid Range:
        /// 1.0E-6 to 0.3 s
        /// 
        /// Default Value: 0.01 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setTime(int Channel, double Time)
        {
            int pInvokeResult = PInvoke.stat_setTime(this._handle, Channel, Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the length of the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time">
        /// This control returns the length of the timegate in which the sensor is doing statistic measurements.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getTime(int Channel, out double Time)
        {
            int pInvokeResult = PInvoke.stat_getTime(this._handle, Channel, out Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the midamble offset after timeslot start in seconds in the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:[EXCLude]:MID:OFFSet[:TIME]
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control sets the midamble offset after timeslot start in seconds in the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setMidOffset(int Channel, double Offset)
        {
            int pInvokeResult = PInvoke.stat_setMidOffset(this._handle, Channel, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the midamble offset after timeslot start in seconds in the timegate in which the sensor is doing statistic measurements.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:[EXCLude]:MID:OFFSet[:TIME]?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control returns the midamble offset after timeslot start in seconds in the timegate in which the sensor is doing statistic measurements.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getMidOffset(int Channel, out double Offset)
        {
            int pInvokeResult = PInvoke.stat_getMidOffset(this._handle, Channel, out Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the midamble length in seconds.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:[EXCLude]:MID:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Length">
        /// This control sets the midamble length in seconds.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setMidLength(int Channel, double Length)
        {
            int pInvokeResult = PInvoke.stat_setMidLength(this._handle, Channel, Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the midamble length in seconds.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:[EXCLude]:MID:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Length">
        /// This control returns the midamble length in seconds.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getMidLength(int Channel, out double Length)
        {
            int pInvokeResult = PInvoke.stat_getMidLength(this._handle, Channel, out Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the lower limit of the level range for the analysis result in both Statistics modes. This level can be assigned to the first pixel. The level assigned to the last pixel is equal to the level of the first pixel plus the level range.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:RLEVel
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Reference_Level">
        /// This control sets the lower limit of the level range for the analysis result in both Statistics modes. This level can be assigned to the first pixel. The level assigned to the last pixel is equal to the level of the first pixel plus the level range.
        /// 
        /// Valid Range:
        /// -80.0 to 20.0 dB
        /// 
        /// Default Value: -30.0 dB
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setScaleRefLevel(int Channel, double Reference_Level)
        {
            int pInvokeResult = PInvoke.stat_setScaleRefLevel(this._handle, Channel, Reference_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the lower limit of the level range for the analysis result in both Statistics modes. This level can be assigned to the first pixel. The level assigned to the last pixel is equal to the level of the first pixel plus the level range.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:RLEVel?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Reference_Level">
        /// This control returns the lower limit of the level range for the analysis result in both Statistics modes. This level can be assigned to the first pixel. The level assigned to the last pixel is equal to the level of the first pixel plus the level range.
        /// 
        /// Valid Range:
        /// -80.0 to 20.0 dBm
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getScaleRefLevel(int Channel, out double Reference_Level)
        {
            int pInvokeResult = PInvoke.stat_getScaleRefLevel(this._handle, Channel, out Reference_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the width of the level range for the analysis result in both Statistics modes.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:RANGe
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Range">
        /// This control sets the width of the level range for the analysis result in both Statistics modes.
        /// 
        /// Valid Range:
        /// 0.01 to 100.0
        /// 
        /// Default Value: 50.0
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setScaleRange(int Channel, double Range)
        {
            int pInvokeResult = PInvoke.stat_setScaleRange(this._handle, Channel, Range);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the width of the level range for the analysis result in both Statistics modes.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:RANGe?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Range">
        /// This control returns the width of the level range for the analysis result in both Statistics modes.
        /// 
        /// Valid Range:
        /// 0.01 to 100
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getScaleRange(int Channel, out double Range)
        {
            int pInvokeResult = PInvoke.stat_getScaleRange(this._handle, Channel, out Range);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the measurement-result resolution in both Statistics modes. This function specifies the number of pixels that are to be assigned to the logarithmic level range (rsnrpz_stat_setScaleRange function) for measured value output. The width of the level range divided by N-1, where N is the number of pixels, must not be less than the value which can be read out with rsnrpz_stat_getScaleWidth.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:POINts
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Points">
        /// This control the measurement-result resolution in both Statistics modes.
        /// 
        /// Valid Range:
        /// 3 to 8192
        /// 
        /// Default Value: 200
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_setScalePoints(int Channel, int Points)
        {
            int pInvokeResult = PInvoke.stat_setScalePoints(this._handle, Channel, Points);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the measurement-result resolution in both Statistics modes.
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:POINts?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Points">
        /// This control returns the measurement-result resolution in both Statistics modes.
        /// 
        /// Valid Range:
        /// 3 to 8192
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getScalePoints(int Channel, out int Points)
        {
            int pInvokeResult = PInvoke.stat_getScalePoints(this._handle, Channel, out Points);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the greatest attainable level resolution. For the R&amp;S NRP-Z81 power sensor, this value is fixed at 0.006 dB per pixel. If this value is exceeded, a "Settings conflict" message is output. The reason for the conflict may be that the number of pixels that has been selected is too great or that the width chosen for the level range is too small (rsnrpz_stat_setScalePoints and rsnrpz_stat_setScaleRange functions).
        /// 
        /// Remote-control command(s):
        /// SENSe:STATistics:SCALE:X:MPWidth?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Width">
        /// This control returns the greatest attainable level resolution.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int stat_getScaleWidth(int Channel, out double Width)
        {
            int pInvokeResult = PInvoke.stat_getScaleWidth(this._handle, Channel, out Width);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the parameters of Timeslot measurement mode. Both exclude start and stop are set to 10% of timeslot width each.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:TSL:AVG:COUN
        /// SENS:POW:TSL:AVG:WIDT
        /// SENS:TIM:EXCL:STAR
        /// SENS:TIM:EXCL:STOP
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time_Slot_Count">
        /// This control sets the number of simultaneously measured timeslots in the Timeslot mode.
        /// 
        /// Valid Range:
        /// 1 - 128
        /// 
        /// Default Value:
        /// 8
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Width">
        /// This control sets the length in seconds of the timeslot in the Timeslot mode.
        /// 
        /// Valid Range:
        /// 10.0e-6 - 0.1
        /// 
        /// Default Value: 1.0e-3 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_configureTimeSlot(int Channel, int Time_Slot_Count, double Width)
        {
            int pInvokeResult = PInvoke.tslot_configureTimeSlot(this._handle, Channel, Time_Slot_Count, Width);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the number of simultaneously measured timeslots in the Timeslot mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:TSL:AVG:COUN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time_Slot_Count">
        /// This control sets the number of simultaneously measured timeslots in the Timeslot mode.
        /// 
        /// Valid Range:
        /// 1 - 128
        /// 
        /// Default Value:
        /// 8
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_setTimeSlotCount(int Channel, int Time_Slot_Count)
        {
            int pInvokeResult = PInvoke.tslot_setTimeSlotCount(this._handle, Channel, Time_Slot_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the number of simultaneously measured timeslots in the Timeslot mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:TSL:AVG:COUN?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time_Slot_Count">
        /// This control returns the number of simultaneously measured timeslots in the Timeslot mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_getTimeSlotCount(int Channel, out int Time_Slot_Count)
        {
            int pInvokeResult = PInvoke.tslot_getTimeSlotCount(this._handle, Channel, out Time_Slot_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the length of the timeslot in the Timeslot mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:TSL:AVG:WIDT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Width">
        /// This control sets the length in seconds of the timeslot in the Timeslot mode.
        /// 
        /// Valid Range:
        /// 10.0e-6 - 0.1
        /// 
        /// Default Value: 1.0e-3 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_setTimeSlotWidth(int Channel, double Width)
        {
            int pInvokeResult = PInvoke.tslot_setTimeSlotWidth(this._handle, Channel, Width);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the length of the timeslot in the Timeslot mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:POW:TSL:AVG:WIDT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Width">
        /// This control returns the length in seconds of the timeslot in the Timeslot mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_getTimeSlotWidth(int Channel, out double Width)
        {
            int pInvokeResult = PInvoke.tslot_getTimeSlotWidth(this._handle, Channel, out Width);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the start of an exclusion interval in a timeslot. In conjunction with the function rsnrpz_tslot_setTimeSlotMidLength, it is possible to exclude, for example, a midamble from the measurement. The start of the timeslot is used as the reference point for defining the start of the exclusion interval and this applies to each of the timeslots
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TSLot[:AVG]:MID:OFFSet
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control sets sets the start of an exclusion interval in a timeslot.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_setTimeSlotMidOffset(int Channel, double Offset)
        {
            int pInvokeResult = PInvoke.tslot_setTimeSlotMidOffset(this._handle, Channel, Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the start of an exclusion interval in a timeslot.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TSLot[:AVG]:MID:OFFSet?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset">
        /// This control returns sets the start of an exclusion interval in a timeslot.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_getTimeSlotMidOffset(int Channel, out double Offset)
        {
            int pInvokeResult = PInvoke.tslot_getTimeSlotMidOffset(this._handle, Channel, out Offset);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the length of an exclusion interval in a timeslot. In conjunction with the function rsnrpz_tslot_setTimeSlotMidOffset, it can be used, for example, to exclude a midamble from the measurement.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TSLot[:AVG]:MID:TIME
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Length">
        /// This control sets the length of an exclusion interval in a timeslot.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1
        /// 
        /// Default Value: 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_setTimeSlotMidLength(int Channel, double Length)
        {
            int pInvokeResult = PInvoke.tslot_setTimeSlotMidLength(this._handle, Channel, Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the length of an exclusion interval in a timeslot.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TSLot[:AVG]:MID:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Length">
        /// This control returns the length of an exclusion interval in a timeslot.
        /// 
        /// Valid Range:
        /// 0.0 to 0.1 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_getTimeSlotMidLength(int Channel, out double Length)
        {
            int pInvokeResult = PInvoke.tslot_getTimeSlotMidLength(this._handle, Channel, out Length);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function enables or disables the chopper in Time Slot mode. Disabling the chopper is only good for fast but unexact and noisy measurements. If the chopper is disabled, averaging is ignored internally also disabled.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TSLot[:AVG]:CHOPper:STATe
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time_Slot_Chopper">
        /// This control enables or disables the chopper for Time Slot mode.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_setTimeSlotChopperEnabled(int Channel, bool Time_Slot_Chopper)
        {
            int pInvokeResult = PInvoke.tslot_setTimeSlotChopperEnabled(this._handle, Channel, System.Convert.ToUInt16(Time_Slot_Chopper));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the state of the chopper in Time Slot mode.
        /// 
        /// Remote-control command(s):
        /// SENSe:POWer:TSLot[:AVG]:CHOPper:STATe?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Time_Slot_Chopper">
        /// This control returns the state of the chopper for Time Slot mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int tslot_getTimeSlotChopperEnabled(int Channel, out bool Time_Slot_Chopper)
        {
            ushort Time_Slot_ChopperAsUShort;
            int pInvokeResult = PInvoke.tslot_getTimeSlotChopperEnabled(this._handle, Channel, out Time_Slot_ChopperAsUShort);
            Time_Slot_Chopper = System.Convert.ToBoolean(Time_Slot_ChopperAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets parameters of the Scope mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:POIN
        /// SENS:TRAC:TIME
        /// SENS:TRAC:OFFS:TIME
        /// SENS:TRAC:REAL ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Points">
        /// This control sets the number of desired values per Scope sequence.
        /// 
        /// Valid Range:
        /// 1 to 1024
        /// 
        /// Default Value: 312
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Scope_Time">
        /// This control sets the value of scope time.
        /// 
        /// Valid Range:
        /// 0.1e-3 to 0.3 s
        /// 
        /// Default Value: 0.01 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Offset_Time">
        /// This control sets the value of offset time.
        /// 
        /// Valid Range:
        /// 
        /// -5.0e-3 to 100.0 s
        /// 
        /// Default Value: 0.0 s
        /// 
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Realtime">
        /// This control sets the state of real-time measurement.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_configureScope(int Channel, int Scope_Points, double Scope_Time, double Offset_Time, bool Realtime)
        {
            int pInvokeResult = PInvoke.scope_configureScope(this._handle, Channel, Scope_Points, Scope_Time, Offset_Time, System.Convert.ToUInt16(Realtime));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs fast zeroing, but can be called only in the sensor's Trace mode and Statistics modes. In any other measurement mode, the error message NRPERROR_CALZERO is output. Even though the execution time is shorter than that for standard zeroing by a factor of 100 or more, measurement accuracy is not affected. Fast zeroing is available for the entire frequency range.
        /// 
        /// Remote-control command(s):
        /// CAL:ZERO:FAST:AUTO
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_fastZero()
        {
            int pInvokeResult = PInvoke.scope_fastZero(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// For the Scope mode, this function switches the filter function of a sensor on or off. When the filter is switched on, the number of measured values set with SENS:TRAC:AVER:COUN (function rsnrpz_scope_setAverageCount) is averaged. This reduces the effect of noise so that more reliable results are obtained.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Averaging">
        /// This control switches the filter function of a sensor on or off.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAverageEnabled(int Channel, bool Scope_Averaging)
        {
            int pInvokeResult = PInvoke.scope_setAverageEnabled(this._handle, Channel, System.Convert.ToUInt16(Scope_Averaging));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the state of filter function of a sensor.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Averaging">
        /// This control returns the state of filter function of a sensor.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAverageEnabled(int Channel, out bool Scope_Averaging)
        {
            ushort Scope_AveragingAsUShort;
            int pInvokeResult = PInvoke.scope_getAverageEnabled(this._handle, Channel, out Scope_AveragingAsUShort);
            Scope_Averaging = System.Convert.ToBoolean(Scope_AveragingAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the length of the filter for the Scope mode. The wider the filter the lower the noise and the longer it takes to obtain a measured value.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Count">
        /// This control sets the length of the filter for the Scope mode.
        /// 
        /// Valid Range:
        /// 1 to 65536
        /// 
        /// Default Value: 4
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAverageCount(int Channel, int Count)
        {
            int pInvokeResult = PInvoke.scope_setAverageCount(this._handle, Channel, Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the length of the filter for the Scope mode.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN?
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Count">
        /// This control returns the averaging filter length in Scope mode.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAverageCount(int Channel, out int Count)
        {
            int pInvokeResult = PInvoke.scope_getAverageCount(this._handle, Channel, out Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// As soon as a new single value is determined, the filter window is advanced by one value so that the new value is taken into account by the filter and the oldest value is forgotten.
        /// Terminal control then determines in the Scope mode whether a new result will be calculated immediately after a new measured value is available or only after an entire range of new values is available for the filter.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:TCON MOV | REP
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Terminal_Control">
        /// This control determines the type of terminal control.
        /// 
        /// Valid Values:
        ///  RSNRPZ_TERMINAL_CONTROL_MOVING - Moving
        ///  RSNRPZ_TERMINAL_CONTROL_REPEAT - Repeat (Default Value)
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAverageTerminalControl(int Channel, int Terminal_Control)
        {
            int pInvokeResult = PInvoke.scope_setAverageTerminalControl(this._handle, Channel, Terminal_Control);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns selected terminal control type in the Scope mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:TCON?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Terminal_Control">
        /// This control returns the type of terminal control.
        /// 
        /// Valid Values:
        /// Moving (RSNRPZ_TERMINAL_CONTROL_MOVING)
        /// Repeat (RSNRPZ_TERMINAL_CONTROL_REPEAT)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAverageTerminalControl(int Channel, out int Terminal_Control)
        {
            int pInvokeResult = PInvoke.scope_getAverageTerminalControl(this._handle, Channel, out Terminal_Control);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function determines the relative position of the trigger event in relation to the beginning of the Scope measurement sequence.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:OFFS:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset_Time">
        /// This control sets the value of offset time.
        /// 
        /// Valid Range:
        /// 
        /// -5.0e-3 to 100.0 s
        /// 
        /// Default Value: 0.0 s
        /// 
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setOffsetTime(int Channel, double Offset_Time)
        {
            int pInvokeResult = PInvoke.scope_setOffsetTime(this._handle, Channel, Offset_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the relative position of the trigger event in relation to the beginning of the Scope measurement sequence.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:OFFS:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Offset_Time">
        /// This control returns the value of offset time in seconds.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getOffsetTime(int Channel, out double Offset_Time)
        {
            int pInvokeResult = PInvoke.scope_getOffsetTime(this._handle, Channel, out Offset_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the number of desired values per Scope sequence.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:POIN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Points">
        /// This control sets the number of desired values per Scope sequence.
        /// 
        /// Valid Range:
        /// 1 to 1024
        /// 
        /// Default Value: 312
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setPoints(int Channel, int Scope_Points)
        {
            int pInvokeResult = PInvoke.scope_setPoints(this._handle, Channel, Scope_Points);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the number of desired values per Scope sequence.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:POIN?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Points">
        /// This control returns the number of desired values per Scope sequence.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getPoints(int Channel, out int Scope_Points)
        {
            int pInvokeResult = PInvoke.scope_getPoints(this._handle, Channel, out Scope_Points);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// In the default state (OFF), each measurement sequence from the sensor is averaged over several sequences. Since the measured values of a sequence may be closer to each other in time than the measurements, several measurement sequences with a slight time offset are also superimposed on the desired sequence. With the state turned ON - this effect can be switched off, which may increase the measurement speed. This ensures that the measured values of an individual sequence are immediately delivered.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:REAL ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Realtime">
        /// This control sets the state of real-time measurement.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On 
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setRealtimeEnabled(int Channel, bool Realtime)
        {
            int pInvokeResult = PInvoke.scope_setRealtimeEnabled(this._handle, Channel, System.Convert.ToUInt16(Realtime));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the state of real-time measurement setting.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:REAL?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Realtime">
        /// This control returns the state of real-time measurement.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - On (Default Value)
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getRealtimeEnabled(int Channel, out bool Realtime)
        {
            ushort RealtimeAsUShort;
            int pInvokeResult = PInvoke.scope_getRealtimeEnabled(this._handle, Channel, out RealtimeAsUShort);
            Realtime = System.Convert.ToBoolean(RealtimeAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the time to be covered by the Scope sequence.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:TIME
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Time">
        /// This control sets the value of scope time.
        /// 
        /// Valid Range:
        /// 0.1e-3 to 0.3 s
        /// 
        /// Default Value: 0.01 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setTime(int Channel, double Scope_Time)
        {
            int pInvokeResult = PInvoke.scope_setTime(this._handle, Channel, Scope_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the value of the time to be covered by the Scope sequence.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:TIME?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Scope_Time">
        /// This control returns the value of scope time in seconds.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getTime(int Channel, out double Scope_Time)
        {
            int pInvokeResult = PInvoke.scope_getTime(this._handle, Channel, out Scope_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function can be used to automatically determine a value for filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO ON|OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Enabled">
        /// This control activates or deactivates automatic determination of a value for filter bandwidth.
        /// If the automatic switchover is activated with the ON parameter, the sensor always defines a suitable filter length.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAutoEnabled(int Channel, bool Auto_Enabled)
        {
            int pInvokeResult = PInvoke.scope_setAutoEnabled(this._handle, Channel, System.Convert.ToUInt16(Auto_Enabled));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the setting of automatic switchover of filter bandwidth.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Enabled">
        /// This control returns the setting of automatic switchover of filter bandwidth.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAutoEnabled(int Channel, out bool Auto_Enabled)
        {
            ushort Auto_EnabledAsUShort;
            int pInvokeResult = PInvoke.scope_getAutoEnabled(this._handle, Channel, out Auto_EnabledAsUShort);
            Auto_Enabled = System.Convert.ToBoolean(Auto_EnabledAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        ///  This function sets an upper time limit can be set via (maximum time). It should never be exceeded. Undesired long measurement times can thus be prevented if the automatic filter length switchover is on.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO:MTIM
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Upper_Time_Limit">
        /// This control sets the upper time limit (maximum time to fill the filter).
        /// 
        /// Valid Range:
        /// 
        /// NRP-21: 0.01 - 999.99
        /// FSH-Z1: 0.01 - 999.99
        /// 
        /// Default Value: 4.0
        /// 
        /// Notes:
        /// (1) This value is not range checked, the actual range depends on sensor used.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAutoMaxMeasuringTime(int Channel, double Upper_Time_Limit)
        {
            int pInvokeResult = PInvoke.scope_setAutoMaxMeasuringTime(this._handle, Channel, Upper_Time_Limit);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries an upper time limit (maximum time to fill the filter).
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO:MTIM
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Upper_Time_Limit">
        /// This control returns an upper time limit (maximum time to fill the filter).
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAutoMaxMeasuringTime(int Channel, out double Upper_Time_Limit)
        {
            int pInvokeResult = PInvoke.scope_getAutoMaxMeasuringTime(this._handle, Channel, out Upper_Time_Limit);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the maximum noise ratio in the measurement result.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO:NSR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Noise_Ratio">
        /// This control sets the maximum noise ratio in the measurement result. The value is set in dB.
        /// 
        /// Valid Range:
        /// 
        /// NRP-Z21: 0.0 - 1.0
        /// FSH-Z1:  0.0 - 1.0
        /// 
        /// Default Value: 0.1
        /// 
        /// Notes:
        /// (1) This value is not range checked; the actual range depends on sensor used.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAutoNoiseSignalRatio(int Channel, double Maximum_Noise_Ratio)
        {
            int pInvokeResult = PInvoke.scope_setAutoNoiseSignalRatio(this._handle, Channel, Maximum_Noise_Ratio);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the maximum noise signal ratio value.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO:NSR?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Noise_Ratio">
        /// This control returns a maximum noise signal ratio in dB.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAutoNoiseSignalRatio(int Channel, out double Maximum_Noise_Ratio)
        {
            int pInvokeResult = PInvoke.scope_getAutoNoiseSignalRatio(this._handle, Channel, out Maximum_Noise_Ratio);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result. This setting does not affect the setting of display.
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:RES 1 ... 4
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Resolution">
        /// This control defines the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result.
        /// 
        /// Valid Range:
        /// 1 to 4
        /// 
        /// Default Value: 3
        /// 
        /// Notes:
        /// (1) Actual range depend on sensor used and may vary from the range stated above.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAutoResolution(int Channel, int Resolution)
        {
            int pInvokeResult = PInvoke.scope_setAutoResolution(this._handle, Channel, Resolution);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result. 
        /// 
        /// Remote-control command(s):
        /// SENS:AVER:COUN:AUTO:RES?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Resolution">
        /// This control returns the number of significant places for linear units and the number of decimal places for logarithmic units which should be free of noise in the measurement result.
        /// 
        /// Valid Range:
        /// 1 to 4
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int scope_getAutoResolution(int Channel, out int Resolution)
        {
            int pInvokeResult = PInvoke.scope_getAutoResolution(this._handle, Channel, out Resolution);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects a method by which the automatic filter length switchover can operate.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO:TYPE RES | NSR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Method">
        /// This control selects a method by which the automatic filter length switchover can operate.
        /// 
        /// Valid Values:
        /// RSNRPZ_AUTO_COUNT_TYPE_RESOLUTION (0) - Resolution (Default Value)
        /// RSNRPZ_AUTO_COUNT_TYPE_NSR (1) - Noise Ratio
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_setAutoType(int Channel, int Method)
        {
            int pInvokeResult = PInvoke.scope_setAutoType(this._handle, Channel, Method);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns a method by which the automatic filter length switchover can operate.
        /// 
        /// Remote-control command(s):
        /// SENS:TRAC:AVER:COUN:AUTO:TYPE?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Method">
        /// This control returns a method by which the automatic filter length switchover can operate.
        /// 
        /// Valid Values:
        /// Resolution (RSNRPZ_AUTO_COUNT_TYPE_RESOLUTION)
        /// Noise Ratio (RSNRPZ_AUTO_COUNT_TYPE_NSR)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int scope_getAutoType(int Channel, out int Method)
        {
            int pInvokeResult = PInvoke.scope_getAutoType(this._handle, Channel, out Method);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the parameters of internal trigger system.
        /// 
        /// Remote-control command(s):
        /// TRIG:DEL:AUTO ON
        /// TRIG:ATR OFF
        /// TRIG:COUN 1
        /// TRIG:DEL 0.0
        /// TRIG:HOLD 0.0
        /// TRIG:HYST 3DB
        /// TRIG:LEV &lt;value&gt;
        /// TRIG:SLOP POS | NEG
        /// TRIG:SOUR INT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Level">
        /// This control determines the power (in W) a trigger signal must exceed before a trigger event is detected.
        /// 
        /// Valid Range:
        /// 0.1e-6 to 0.2 W
        /// 
        /// Default Value:
        /// 1.0e-6 W
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <param name="Trigger_Slope">
        /// This control determines whether the rising (POSitive) or the falling (NEGative) edge of the signal is used for triggering.
        /// 
        /// Valid Values:
        /// RSNRPZ_SLOPE_POSITIVE (0) - Positive (Default Value)
        /// RSNRPZ_SLOPE_NEGATIVE (1) - Negative
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_configureInternal(int Channel, double Trigger_Level, int Trigger_Slope)
        {
            int pInvokeResult = PInvoke.trigger_configureInternal(this._handle, Channel, Trigger_Level, Trigger_Slope);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the parameters of external trigger system.
        /// 
        /// Remote-control command(s):
        /// TRIG:DEL &lt;value&gt;
        /// TRIG:SOUR EXT
        /// TRIG:COUN 1
        /// TRIG:ATR OFF
        /// TRIG:HOLD 0.0
        /// TRIG:DEL:AUTO ON
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Delay">
        /// This control sets a the delay (in seconds) between the trigger event and the beginning of the actual measurement (integration).
        /// 
        /// Valid Range:
        /// -5.0e-3 to 100.0 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_configureExternal(int Channel, double Trigger_Delay)
        {
            int pInvokeResult = PInvoke.trigger_configureExternal(this._handle, Channel, Trigger_Delay);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs triggering and ensures that the sensor directly changes from the WAIT_FOR_TRG state to the MEASURING state irrespective of the selected trigger source. A trigger delay set with TRIG:DEL is ignored but not the automatic delay determined when TRIG:DEL:AUTO:ON is set.
        /// When the trigger source is HOLD, a measurement can only be started with TRIG.
        /// 
        /// Remote-control command(s):
        /// TRIG:IMM
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_immediate(int Channel)
        {
            int pInvokeResult = PInvoke.trigger_immediate(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function ensures (if parameter is set to On) by means of an automatically determined delay that a measurement is started only after the sensor has settled. This is important when thermal sensors are used.
        /// 
        /// Remote-control command(s):
        /// TRIG:DEL:AUTO ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Delay">
        /// This control enables or disables automatic determination of delay.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setAutoDelayEnabled(int Channel, bool Auto_Delay)
        {
            int pInvokeResult = PInvoke.trigger_setAutoDelayEnabled(this._handle, Channel, System.Convert.ToUInt16(Auto_Delay));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the setting of auto delay feature.
        /// 
        /// Remote-control command(s):
        /// TRIG:DEL:AUTO?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Delay">
        /// This control returns the state of Auto Delay feature.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getAutoDelayEnabled(int Channel, out bool Auto_Delay)
        {
            ushort Auto_DelayAsUShort;
            int pInvokeResult = PInvoke.trigger_getAutoDelayEnabled(this._handle, Channel, out Auto_DelayAsUShort);
            Auto_Delay = System.Convert.ToBoolean(Auto_DelayAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function turns On or Off the auto trigger feature. When auto trigger is set to On, the WAIT_FOR_TRG state is automatically exited when no trigger event occurs within a period that corresponds to the reciprocal of the display update rate.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// TRIG:ATR:STAT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Trigger">
        /// This control enables or disables the Auto Trigger.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setAutoTriggerEnabled(int Channel, bool Auto_Trigger)
        {
            int pInvokeResult = PInvoke.trigger_setAutoTriggerEnabled(this._handle, Channel, System.Convert.ToUInt16(Auto_Trigger));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the state of Auto Trigger.
        /// 
        /// Note:
        ///   
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// TRIG:ATR:STAT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auto_Trigger">
        /// This control returns the state of Auto Trigger.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getAutoTriggerEnabled(int Channel, out bool Auto_Trigger)
        {
            ushort Auto_TriggerAsUShort;
            int pInvokeResult = PInvoke.trigger_getAutoTriggerEnabled(this._handle, Channel, out Auto_TriggerAsUShort);
            Auto_Trigger = System.Convert.ToBoolean(Auto_TriggerAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the number of measurement cycles to be  performed when the measurement is started with INIT.
        /// 
        /// Remote-control command(s):
        /// TRIG:COUN
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Count">
        /// This control sets the number of measurement cycles to be  performed when the measurement is started with INIT.
        /// 
        /// Valid Range:
        /// 1 to 2147483646
        /// 
        /// Default Value:
        /// 1
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setCount(int Channel, int Trigger_Count)
        {
            int pInvokeResult = PInvoke.trigger_setCount(this._handle, Channel, Trigger_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the number of measurement cycles to be  performed when the measurement is started with INIT.
        /// 
        /// Remote-control command(s):
        /// TRIG:COUN?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Count">
        /// This control returns the number of measurement cycles to be  performed when the measurement is started with INIT.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getCount(int Channel, out int Trigger_Count)
        {
            int pInvokeResult = PInvoke.trigger_getCount(this._handle, Channel, out Trigger_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines the delay between the trigger event and the beginning of the actual measurement (integration).
        /// 
        /// Remote-control command(s):
        /// TRIG:DEL
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Delay">
        /// This control sets a the delay (in seconds) between the trigger event and the beginning of the actual measurement (integration).
        /// 
        /// Valid Range:
        /// NRP-Z21: -5.0e-3 to 100.0 s
        /// NRP-Z51:  0.0    to 100.0 s
        /// FSH-Z1:  -5.0e-3 to 100.0 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setDelay(int Channel, double Trigger_Delay)
        {
            int pInvokeResult = PInvoke.trigger_setDelay(this._handle, Channel, Trigger_Delay);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads value of the delay (in seconds) between the trigger event and the beginning of the actual measurement (integration).
        /// 
        /// Remote-control command(s):
        /// TRIG:DEL?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Delay">
        /// This control returns value of the delay (in seconds) between the trigger event and the beginning of the actual measurement (integration).
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getDelay(int Channel, out double Trigger_Delay)
        {
            int pInvokeResult = PInvoke.trigger_getDelay(this._handle, Channel, out Trigger_Delay);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines a period after a trigger event within which all further trigger events are ignored.
        /// 
        /// Remote-control command(s):
        /// TRIG:HOLD
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Holdoff">
        /// This control defines a period (in seconds) after a trigger event within which all further trigger events are ignored.
        /// 
        /// Valid Range:
        /// 0.0 - 10.0 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setHoldoff(int Channel, double Trigger_Holdoff)
        {
            int pInvokeResult = PInvoke.trigger_setHoldoff(this._handle, Channel, Trigger_Holdoff);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the value of a period after a trigger event within which all further trigger events are ignored.
        /// 
        /// Remote-control command(s):
        /// TRIGger[1..4]:HOLDoff?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Holdoff">
        /// This control returns the value of a period (in seconds) after a trigger event within which all further trigger events are ignored.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getHoldoff(int Channel, out double Trigger_Holdoff)
        {
            int pInvokeResult = PInvoke.trigger_getHoldoff(this._handle, Channel, out Trigger_Holdoff);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function is used to specify how far the signal level has to drop below the trigger level before a new signal edge can be detected as a trigger event. Thus, this command can be used to eliminate the effects of noise in the signal on the transition filters of the trigger system.
        /// 
        /// Remote-control command(s):
        /// TRIG:HYST
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Hysteresis">
        /// This control defines the trigger hysteresis in dB.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 dB
        /// 
        /// Default Value: 0.0 dB
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setHysteresis(int Channel, double Trigger_Hysteresis)
        {
            int pInvokeResult = PInvoke.trigger_setHysteresis(this._handle, Channel, Trigger_Hysteresis);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the value of trigger hysteresis.
        /// 
        /// Remote-control command(s):
        /// TRIG:HYST?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Hysteresis">
        /// This control returns the value of trigger hysteresis in dB.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getHysteresis(int Channel, out double Trigger_Hysteresis)
        {
            int pInvokeResult = PInvoke.trigger_getHysteresis(this._handle, Channel, out Trigger_Hysteresis);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function determines the power a trigger signal must exceed before a trigger event is detected. This setting is only used for internal trigger signal source.
        /// 
        /// Remote-control command(s):
        /// TRIG:LEV
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Level">
        /// This control determines the power (in W) a trigger signal must exceed before a trigger event is detected.
        /// 
        /// Valid Range:
        /// NRP-Z21: 0.1e-6  to 0.2 W
        /// NRP-Z51: 0.25e-6 to 0.1 W
        /// FSH-Z1:  0.1e-6  to 0.2 W
        /// 
        /// Default Value:
        /// 1.0e-6 W
        /// 
        /// Notes:
        /// (1) Actual valid range depends on sensor used
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setLevel(int Channel, double Trigger_Level)
        {
            int pInvokeResult = PInvoke.trigger_setLevel(this._handle, Channel, Trigger_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads the power a trigger signal must exceed before a trigger event is detected.
        /// 
        /// Remote-control command(s):
        /// TRIG:LEV?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Level">
        /// This control returns the power (in W) a trigger signal must exceed before a trigger event is detected.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getLevel(int Channel, out double Trigger_Level)
        {
            int pInvokeResult = PInvoke.trigger_getLevel(this._handle, Channel, out Trigger_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function determines whether the rising (POSitive) or the falling (NEGative) edge of the signal is used for triggering.
        /// 
        /// Remote-control command(s):
        /// TRIG:SLOP POSitive | NEGative
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Slope">
        /// This control determines whether the rising (POSitive) or the falling (NEGative) edge of the signal is used for triggering.
        /// 
        /// Valid Values:
        /// RSNRPZ_SLOPE_POSITIVE (0) - Positive (Default Value)
        /// RSNRPZ_SLOPE_NEGATIVE (1) - Negative
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setSlope(int Channel, int Trigger_Slope)
        {
            int pInvokeResult = PInvoke.trigger_setSlope(this._handle, Channel, Trigger_Slope);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads whether the rising (POSitive) or the falling (NEGative) edge of the signal is used for triggering.
        /// 
        /// Remote-control command(s):
        /// TRIG:SLOP?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Slope">
        /// This control returns whether the rising (POSitive) or the falling (NEGative) edge of the signal is used for triggering.
        /// 
        /// Valid Values:
        /// RSNRPZ_SLOPE_POSITIVE (0) - Positive
        /// RSNRPZ_SLOPE_NEGATIVE (1) - Negative
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getSlope(int Channel, out int Trigger_Slope)
        {
            int pInvokeResult = PInvoke.trigger_getSlope(this._handle, Channel, out Trigger_Slope);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the trigger signal source for the WAIT_FOR_TRG state.
        /// 
        /// Remote-control command(s):
        /// TRIG:SOUR BUS | EXT | HOLD | IMM | INT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Source">
        /// This control selects the trigger signal source for the WAIT_FOR_TRG state.
        /// 
        /// Valid Values:
        /// RSNRPZ_TRIGGER_SOURCE_BUS       (0) - Bus 
        /// RSNRPZ_TRIGGER_SOURCE_EXTERNAL  (1) - External
        /// RSNRPZ_TRIGGER_SOURCE_HOLD      (2) - Hold
        /// RSNRPZ_TRIGGER_SOURCE_IMMEDIATE (3) - Immediate (Default Value)
        /// RSNRPZ_TRIGGER_SOURCE_INTERNAL  (4) - Internal
        /// 
        /// Notes:
        /// (1) Bus: The trigger event is initiated by TRIG:IMM or *TRG. In this case, the setting for Trigger Slope is meaningless.
        /// 
        /// (2) External: Triggering is performed with an external signal applied to the trigger connector. The Trigger Slope setting determines whether the rising or the falling edge of the signal is to be used for triggering. Waiting for a trigger event can be skipped by Immediate Trigger.
        /// 
        /// (3) Hold: A measurement can only be triggered when Immediate Trigger is executed.
        /// 
        /// (4) Immediate: The sensor does not remain in the WAIT_FOR_TRG state but immediately changes to the MEASURING state.
        /// 
        /// (5) Internal: The sensor determines the trigger time by means of the signal to be measured. When this signal exceeds the power set by Trigger Level, the measurement is started after the time set by Trigger Delay. Similar to External Trigger, waiting for a trigger event can also be skipped by Immediate Trigger.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setSource(int Channel, int Trigger_Source)
        {
            int pInvokeResult = PInvoke.trigger_setSource(this._handle, Channel, Trigger_Source);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets the trigger signal source for the WAIT_FOR_TRG state.
        /// 
        /// Remote-control command(s):
        /// TRIG:SOUR?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Trigger_Source">
        /// This control returns the trigger signal source for the WAIT_FOR_TRG state.
        /// 
        /// Valid Values:
        /// RSNRPZ_TRIGGER_SOURCE_BUS (0) - Bus
        /// RSNRPZ_TRIGGER_SOURCE_EXTERNAL (1) - External
        /// RSNRPZ_TRIGGER_SOURCE_HOLD (2) - Hold
        /// RSNRPZ_TRIGGER_SOURCE_IMMEDIATE (3) - Immediate
        /// RSNRPZ_TRIGGER_SOURCE_INTERNAL (4) - Internal
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getSource(int Channel, out int Trigger_Source)
        {
            int pInvokeResult = PInvoke.trigger_getSource(this._handle, Channel, out Trigger_Source);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function defines the dropout time value. With a positive (negative) trigger slope, the dropout time is the minimum time for which the signal must be below (above) the power level defined by rsnrpz_trigger_setLevel and rsnrpz_trigger_setHysteresis before triggering can occur again. As with the Holdoff parameter, unwanted trigger events can be excluded. The set dropout time only affects the internal trigger source.
        /// The dropout time parameter is useful when dealing with, for example, GSM signals with several active slots.
        /// 
        /// Remote-control command(s):
        /// TRIGger:DTIMe
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Dropout_Time">
        /// This control defines the dropout time value.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value:
        /// 0.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_setDropoutTime(int Channel, double Dropout_Time)
        {
            int pInvokeResult = PInvoke.trigger_setDropoutTime(this._handle, Channel, Dropout_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the dropout time value.
        /// 
        /// Remote-control command(s):
        /// TRIGger:DTIMe?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Dropout_Time">
        /// This control returns the dropout time value.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int trigger_getDropoutTime(int Channel, out double Dropout_Time)
        {
            int pInvokeResult = PInvoke.trigger_getDropoutTime(this._handle, Channel, out Dropout_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns selected information on a sensor.
        /// 
        /// Remote-control command(s):
        /// SYST:INFO? &lt;Info Type&gt;
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Info_Type">
        /// This control specifies which info should be retrieved from the sensor.
        /// 
        /// Valid Values:
        ///  "Manufacturer"
        ///  "Type"
        ///  "Stock Number"
        ///  "Serial"
        ///  "HWVersion"
        ///  "HWVariant"
        ///  "SW Build"
        ///  "Technology"
        ///  "Function"
        ///  "MinPower"
        ///  "MaxPower" 
        ///  "MinFreq"
        ///  "MaxFreq"
        ///  "Resolution"
        ///  "Impedance"
        ///  "Coupling"
        ///  "Cal. Abs."
        ///  "Cal. Refl."
        ///  "Cal. S-Para."
        ///  "Cal. Misc."
        ///  "Cal. Temp."
        ///  "Cal. Lin."
        ///  "SPD Mnemonic"
        /// 
        /// Default Value:
        /// ""
        /// </param>
        /// <param name="Array_Size">
        /// This control defines the size of array 'Info'.
        /// 
        /// Valid Range:
        /// -
        /// 
        /// Default Value:
        /// 100
        /// </param>
        /// <param name="Info">
        /// This control returns requested informations from the sensor.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_info(int Channel, string Info_Type, int Array_Size, System.Text.StringBuilder Info)
        {
            int pInvokeResult = PInvoke.chan_info(this._handle, Channel, Info_Type, Array_Size, Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns specified parameter header which can be retrieved from selected sensor.
        /// 
        /// Remote-control command(s):
        /// SYST:INFO?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Parameter_Number">
        /// This control defines the position of parameter header to be retrieved.
        /// 
        /// Valid Range:
        /// 0 to (count of headers - 1)
        /// 
        /// Default Value:
        /// 0
        /// 
        /// Notes:
        /// (1) Only Minimum value of the parameter is checked. Maximum value depends on sensor used and can be retrieved by function rsnrpz_chan_infosCount().
        /// </param>
        /// <param name="Array_Size">
        /// This control defines the size of array 'Header'.
        /// 
        /// Valid Range:
        /// -
        /// 
        /// Default Value:
        /// 100
        /// </param>
        /// <param name="Header">
        /// This control returns specified parameter header.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_infoHeader(int Channel, int Parameter_Number, int Array_Size, System.Text.StringBuilder Header)
        {
            int pInvokeResult = PInvoke.chan_infoHeader(this._handle, Channel, Parameter_Number, Array_Size, Header);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the number of info headers for selected channel.
        /// 
        /// Remote-control command(s):
        /// SYST:INFO?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Count">
        /// This control returns the number of available info headers for selected sensor.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_infosCount(int Channel, out int Count)
        {
            int pInvokeResult = PInvoke.chan_infosCount(this._handle, Channel, out Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets status update time, which influences USB traffic during sensor's waiting for trigger.
        /// 
        /// Note:
        /// 
        /// 1) This function is available only for NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// SYST:SUT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Status_Update_Time">
        /// This control sets status update time, which influences USB traffic during sensor's waiting for trigger.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 100.0e-4 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int system_setStatusUpdateTime(int Channel, double Status_Update_Time)
        {
            int pInvokeResult = PInvoke.system_setStatusUpdateTime(this._handle, Channel, Status_Update_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets status update time.
        /// 
        /// Note:
        /// 
        /// 1) This function is available only for NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// SYST:SUT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Status_Update_Time">
        /// This control returns status update time.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int system_getStatusUpdateTime(int Channel, out double Status_Update_Time)
        {
            int pInvokeResult = PInvoke.system_getStatusUpdateTime(this._handle, Channel, out Status_Update_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets result update time, which influences USB traffic if sensor is in continuous sweep mode.
        /// 
        /// Note:
        /// 
        /// 1) This function is available only for NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// SYST:RUT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Result_Update_Time">
        /// This control sets result update time, which influences USB traffic if sensor is in continuous sweep mode.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// 
        /// Default Value: 0.1 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int system_setResultUpdateTime(int Channel, double Result_Update_Time)
        {
            int pInvokeResult = PInvoke.system_setResultUpdateTime(this._handle, Channel, Result_Update_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function gets result update time.
        /// 
        /// Note:
        /// 
        /// 1) This function is available only for NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// SYST:RUT
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Result_Update_Time">
        /// This control gets result update time.
        /// 
        /// Valid Range:
        /// 0.0 to 10.0 s
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int system_getResultUpdateTime(int Channel, out double Result_Update_Time)
        {
            int pInvokeResult = PInvoke.system_getResultUpdateTime(this._handle, Channel, out Result_Update_Time);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function immediately sets selected sensor to the IDLE state. Measurements in progress are interrupted. If INIT:CONT ON is set, a new measurement is immediately started since the trigger system is not influenced.
        /// 
        /// Remote-control command(s):
        /// ABOR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_abort(int Channel)
        {
            int pInvokeResult = PInvoke.chan_abort(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function starts a single-shot measurement on selected channel. The respective sensor goes to the INITIATED state. The command is completely executed when the sensor returns to the IDLE state. The command is ignored when the sensor is not in the IDLE state or when continuous measurements are selected (INIT:CONT ON). The command is only fully executed when the measurement is completed and the trigger system has again reached the IDLE state. INIT is the only remote control command that permits overlapping execution. Other commands can be received and processed while the command is being executed.
        /// 
        /// Remote-control command(s):
        /// STAT:OPER:MEAS?
        /// INITiate[1..4]
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_initiate(int Channel)
        {
            int pInvokeResult = PInvoke.chan_initiate(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects either single-shot or continuous (free-running) measurement cycles.
        /// 
        /// Remote-control command(s):
        /// INIT:CONT ON | OFF
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Continuous_Initiate">
        /// This control enables or disables the continuous measurement mode.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off (Default Value)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setInitContinuousEnabled(int Channel, bool Continuous_Initiate)
        {
            int pInvokeResult = PInvoke.chan_setInitContinuousEnabled(this._handle, Channel, System.Convert.ToUInt16(Continuous_Initiate));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns whether single-shot or continuous (free-running) measurement is selected.
        /// 
        /// Remote-control command(s):
        /// INIT:CONT?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Continuous_Initiate">
        /// This control returns the state of continuous initiate.
        /// 
        /// Valid Values:
        /// VI_TRUE  (1) - On
        /// VI_FALSE (0) - Off
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getInitContinuousEnabled(int Channel, out bool Continuous_Initiate)
        {
            ushort Continuous_InitiateAsUShort;
            int pInvokeResult = PInvoke.chan_getInitContinuousEnabled(this._handle, Channel, out Continuous_InitiateAsUShort);
            Continuous_Initiate = System.Convert.ToBoolean(Continuous_InitiateAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// From the point of view of the R&amp;S NRP basic unit, the sensors are stand-alone measuring devices. They communicate with the R&amp;S NRP via a command set complying with SCPI.
        /// This function prompts the basic unit to send an *RST to the respective sensor. Measurements in progress are interrupted.
        /// 
        /// Remote-control command(s):
        /// SYSTem:SENSor[1..4]:RESet
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_reset(int Channel)
        {
            int pInvokeResult = PInvoke.chan_reset(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// If the signal to be measured has modulation sections just above the video bandwidth of the sensor used, measurement errors might be caused due to aliasing effects. In this case, the sampling rate of the sensor can be set to a safe lower value (Sampling Frequency 2). However, the measurement time required to obtain noise-free results is extended compared to the normal sampling rate (Sampling Frequency 1).
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:SAMP FREQ1 | FREQ2
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Sampling_Frequency">
        /// This control selects the sampling frequency.
        /// 
        /// Valid Values:
        /// RSNRPZ_SAMPLING_FREQUENCY1 (1) - Sampling Frq 1 (High) (Default Value)
        /// RSNRPZ_SAMPLING_FREQUENCY2 (2) - Sampling Frq 2 (Low)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setSamplingFrequency(int Channel, int Sampling_Frequency)
        {
            int pInvokeResult = PInvoke.chan_setSamplingFrequency(this._handle, Channel, Sampling_Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the selected sampling frequency.
        /// 
        /// Note:
        /// 
        /// 1) This function is not available for NRP-Z51.
        /// 
        /// Remote-control command(s):
        /// SENS:SAMP?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Sampling_Frequency">
        /// This control returns the selected sampling frequency.
        /// 
        /// Valid Values:
        /// RSNRPZ_SAMPLING_FREQUENCY1 (1) - Sampling Frq 1 (High)
        /// RSNRPZ_SAMPLING_FREQUENCY2 (2) - Sampling Frq 2 (Low)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getSamplingFrequency(int Channel, out int Sampling_Frequency)
        {
            int pInvokeResult = PInvoke.chan_getSamplingFrequency(this._handle, Channel, out Sampling_Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs zeroing of selected sensor using the signal at the sensor input. The sensor must be disconnected from all power sources. If the signal at the input considerably deviates from 0 W, an error message is issued and the command is aborted.
        /// 
        /// Remote-control command(s):
        /// STAT:OPER:MEAS?
        /// CAL:ZERO:AUTO ONCE
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_zero(int Channel)
        {
            int pInvokeResult = PInvoke.chan_zero(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs zeroing using the signal at the sensor input. The sensor must be disconnected from all power sources. If the signal at the input considerably deviates from 0 W, an error message is issued and the function is aborted.
        /// 
        /// Note(s):
        /// 
        /// (1) This function is valid only for NRP-Z81.
        /// 
        /// Remote-control command(s):
        /// CAL:ZERO:AUTO LFR | UFR
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Zeroing">
        /// This control selects type of advanced zeroing.
        /// 
        /// Valid Values:
        /// RSNRPZ_ZERO_LFR (0) - Low Frequencies
        /// RSNRPZ_ZERO_UFR (1) - High Frequencies
        /// 
        /// Default Value: RSNRPZ_ZERO_LFR (0)
        /// 
        /// Note(s):
        /// 
        /// (1) Low Frequencies - Does zeroing only for low frequencies (&lt; 500 MHZ)
        /// 
        /// (2) High Frequencies - Does zeroing for higher Frequencies (&gt;= 500 MHz).
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_zeroAdvanced(int Channel, int Zeroing)
        {
            int pInvokeResult = PInvoke.chan_zeroAdvanced(this._handle, Channel, Zeroing);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the state of zeroing of the sensor.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Zeroing_Complete">
        /// This control returns the state of the zeroing of the sensor.
        /// 
        /// Valid Values:
        /// Complete (VI_TRUE)
        /// In Progress (VI_FALSE)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_isZeroComplete(int Channel, out bool Zeroing_Complete)
        {
            ushort Zeroing_CompleteAsUShort;
            int pInvokeResult = PInvoke.chan_isZeroComplete(this._handle, Channel, out Zeroing_CompleteAsUShort);
            Zeroing_Complete = System.Convert.ToBoolean(Zeroing_CompleteAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the state of the measurement.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Measurement_Complete">
        /// This control returns the state of the measurement.
        /// 
        /// Valid Values:
        /// Complete (VI_TRUE)
        /// In Progress (VI_FALSE)
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_isMeasurementComplete(int Channel, out bool Measurement_Complete)
        {
            ushort Measurement_CompleteAsUShort;
            int pInvokeResult = PInvoke.chan_isMeasurementComplete(this._handle, Channel, out Measurement_CompleteAsUShort);
            Measurement_Complete = System.Convert.ToBoolean(Measurement_CompleteAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function performs a sensor test and returns a list of strings separated by commas. The contents of this test protocol is sensor-specific. For its meaning, please refer to the sensor documentation.
        /// 
        /// Remote-control command(s):
        /// TEST:SENS?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Result">
        /// This control returns the result string of self-test.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_selfTest(int Channel, System.Text.StringBuilder Result)
        {
            int pInvokeResult = PInvoke.chan_selfTest(this._handle, Channel, Result);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects which measurement results are to be made available in the Trace mode.
        /// 
        /// Note(s):
        /// 
        /// (1) This functions is available only for NRP-Z81
        /// 
        /// Remote-control command(s):
        /// SENSe:AUXiliary NONE | MINMAX | RNDMAX
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auxiliary_Value">
        /// This control selects which measurement results are to be made available in the Trace mode.
        /// 
        /// Valid Values:
        /// RSNRPZ_AUX_NONE   (0) - None
        /// RSNRPZ_AUX_MINMAX (1) - Min Max
        /// RSNRPZ_AUX_RNDMAX (2) - Rnd Max
        /// 
        /// Note(s):
        /// 
        /// (1) None - only the average power of the associated samples
        /// 
        /// (2) Min Max - provides the maximum and minimum as well
        /// 
        /// (3) Rnd Max - provides the maximum and a random sample.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_setAuxiliary(int Channel, int Auxiliary_Value)
        {
            int pInvokeResult = PInvoke.chan_setAuxiliary(this._handle, Channel, Auxiliary_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries which measurement results are available in the Trace mode.
        /// 
        /// Note(s):
        /// 
        /// (1) This functions is available only for NRP-Z81
        /// 
        /// Remote-control command(s):
        /// SENSe:AUXiliary?
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Auxiliary_Value">
        /// This control returns which measurement results are available in the Trace mode.
        /// 
        /// Valid Values:
        /// RSNRPZ_AUX_NONE   (0) - None (Default Value)
        /// RSNRPZ_AUX_MINMAX (1) - Min Max
        /// RSNRPZ_AUX_RNDMAX (2) - Rnd Max
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int chan_getAuxiliary(int Channel, out int Auxiliary_Value)
        {
            int pInvokeResult = PInvoke.chan_getAuxiliary(this._handle, Channel, out Auxiliary_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function initiates an acquisition on the channels that you specifies in channel parameter.  It then waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timeout__ms_">
        /// Pass the maximum length of time in which to allow the read measurement operation to complete.    
        /// 
        /// If the operation does not complete within this time interval, the function returns the RSNRPZ_ERROR_MAX_TIME_EXCEEDED error code.  When this occurs, you can call rsnrpz_chan_abort to cancel the read measurement operation and return the sensor to the Idle state.
        /// 
        /// Units:  milliseconds.  
        /// 
        /// Defined Values:
        /// RSNRPZ_VAL_MAX_TIME_INFINITE
        /// RSNRPZ_VAL_MAX_TIME_IMMEDIATE
        /// 
        /// Default Value: 5000 (ms)
        /// 
        /// Notes:
        /// 
        /// (1) The Maximum Time parameter applies only to this function.  It has no effect on other timeout parameters.
        /// </param>
        /// <param name="Measurement">
        /// Returns single measurement.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_readMeasurement(int Channel, int Timeout__ms_, out double Measurement)
        {
            int pInvokeResult = PInvoke.meass_readMeasurement(this._handle, Channel, Timeout__ms_, out Measurement);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the measurement the sensor acquires for the channel you specify.  The measurement is from an acquisition that you previously initiated.  
        /// 
        /// You use the rsnrpz_chan_initiate function to start an acquisition on the channels that you specify. You use the rsnrpz_chan_isMeasurementComplete function to determine when the acquisition is complete.
        /// 
        /// You can call the rsnrpz_meass_readMeasurement function instead of the rsnrpz_chan_initiate function.  The rsnrpz_meass_readMeasurement function starts an acquisition, waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// 
        /// Note:
        /// 
        /// 1) If the acquisition has not be initialized or measurement is still in progress and value is not available, function returns an error( RSNRPZ_ERROR_MEAS_NOT_AVAILABLE ).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Measurement">
        /// Returns single measurement.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_fetchMeasurement(int Channel, out double Measurement)
        {
            int pInvokeResult = PInvoke.meass_fetchMeasurement(this._handle, Channel, out Measurement);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function initiates an acquisition on the channels that you specifies in channel parameter.  It then waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Time__ms_">
        /// Pass the maximum length of time in which to allow the read measurement operation to complete.    
        /// 
        /// If the operation does not complete within this time interval, the function returns the RSNRPZ_ERROR_MAX_TIME_EXCEEDED error code.  When this occurs, you can call rsnrpz_chan_abort to cancel the read measurement operation and return the sensor to the Idle state.
        /// 
        /// Units:  milliseconds.  
        /// 
        /// Defined Values:
        /// RSNRPZ_VAL_MAX_TIME_INFINITE             RSNRPZ_VAL_MAX_TIME_IMMEDIATE           
        /// 
        /// Default Value: 5000 (ms)
        /// 
        /// Notes:
        /// 
        /// (1) The Maximum Time parameter applies only to this function.  It has no effect on other timeout parameters.
        /// </param>
        /// <param name="Buffer_Size">
        /// Pass the number of elements in the Measurement Array parameter.
        /// 
        /// Default Value: None
        /// 
        /// </param>
        /// <param name="Measurement_Array">
        /// Returns the measurement buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Read_Count">
        /// Indicates the number of points the function places in the Measurement Array parameter.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_readBufferMeasurement(int Channel, int Maximum_Time__ms_, int Buffer_Size, double[] Measurement_Array, out int Read_Count)
        {
            int pInvokeResult = PInvoke.meass_readBufferMeasurement(this._handle, Channel, Maximum_Time__ms_, Buffer_Size, Measurement_Array, out Read_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the buffer measurement the sensor acquires for the channel you specify.  The measurement is from an acquisition that you previously initiated.  
        /// 
        /// You use the rsnrpz_chan_initiate function to start an acquisition on the channels that you specify. You use the rsnrpz_chan_isMeasurementComplete function to determine when the acquisition is complete.
        /// 
        /// You can call the rsnrpz_meas_readBufferMeasurement function instead of the rsnrpz_chan_initiate function.  The rsnrpz_meass_readBufferMeasurement function starts an acquisition, waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// 
        /// Note:
        /// 
        /// 1) If the acquisition has not be initialized or measurement is still in progress and value is not available, function returns an error( RSNRPZ_ERROR_MEAS_NOT_AVAILABLE ).
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Array_Size">
        /// Pass the number of elements in the Measurement Array parameter.
        /// 
        /// Default Value: None
        /// 
        /// </param>
        /// <param name="Measurement_Array">
        /// Returns the measurement buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Read_Count">
        /// Indicates the number of points the function places in the Measurement Array parameter.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_fetchBufferMeasurement(int Channel, int Array_Size, double[] Measurement_Array, out int Read_Count)
        {
            int pInvokeResult = PInvoke.meass_fetchBufferMeasurement(this._handle, Channel, Array_Size, Measurement_Array, out Read_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Triggers a BUS event. If the sensor is in the WAIT_FOR_TRG state and the source for the trigger source is set to BUS, the sensor enters the MEASURING state. This function invalidates all current measuring results. A query of measurement data following this function will thus always return the measured value determined in response to this function.
        /// 
        /// Remote-control command(s):
        /// *TRG
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_sendSoftwareTrigger(int Channel)
        {
            int pInvokeResult = PInvoke.meass_sendSoftwareTrigger(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function initiates an acquisition on the channels that you specifies in channel parameter. It then waits for the acquisition to complete, and returns the auxiliary measurement for the channel you specify.
        /// 
        /// Note(s):
        /// 
        /// (1) If SENSE:AUX is set to None, this function returns error.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timeout__ms_">
        /// Pass the maximum length of time in which to allow the read measurement operation to complete.    
        /// 
        /// If the operation does not complete within this time interval, the function returns the RSNRPZ_ERROR_MAX_TIME_EXCEEDED error code.  When this occurs, you can call rsnrpz_chan_abort to cancel the read measurement operation and return the sensor to the Idle state.
        /// 
        /// Units:  milliseconds.  
        /// 
        /// Defined Values:
        /// RSNRPZ_VAL_MAX_TIME_INFINITE
        /// RSNRPZ_VAL_MAX_TIME_IMMEDIATE
        /// 
        /// Default Value: 5000 (ms)
        /// 
        /// Notes:
        /// 
        /// (1) The Maximum Time parameter applies only to this function.  It has no effect on other timeout parameters.
        /// </param>
        /// <param name="Measurement">
        /// Returns single measurement.
        /// </param>
        /// <param name="Aux_1">
        /// This control returns the first Auxiliary value.
        /// </param>
        /// <param name="Aux_2">
        /// This control returns the second Auxiliary value.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_readMeasurementAux(int Channel, int Timeout__ms_, out double Measurement, out double Aux_1, out double Aux_2)
        {
            int pInvokeResult = PInvoke.meass_readMeasurementAux(this._handle, Channel, Timeout__ms_, out Measurement, out Aux_1, out Aux_2);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the measurement the sensor acquires for the channel you specify.  The measurement is from an acquisition that you previously initiated.  
        /// 
        /// You use the rsnrpz_chan_initiate function to start an acquisition on the channels that you specify. You use the rsnrpz_chan_isMeasurementComplete function to determine when the acquisition is complete.
        /// 
        /// You can call the rsnrpz_meass_readMeasurement function instead of the rsnrpz_chan_initiate function.  The rsnrpz_meass_readMeasurement function starts an acquisition, waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// 
        /// Note(s):
        /// 
        /// 1) If the acquisition has not be initialized or measurement is still in progress and value is not available, function returns an error( RSNRPZ_ERROR_MEAS_NOT_AVAILABLE ).
        /// 
        /// 2) If SENSE:AUX is set to None, this function returns error.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Timeout__ms_">
        /// Pass the maximum length of time in which to allow the read measurement operation to complete.    
        /// 
        /// If the operation does not complete within this time interval, the function returns the RSNRPZ_ERROR_MAX_TIME_EXCEEDED error code.  When this occurs, you can call rsnrpz_chan_abort to cancel the read measurement operation and return the sensor to the Idle state.
        /// 
        /// Units:  milliseconds.  
        /// 
        /// Defined Values:
        /// RSNRPZ_VAL_MAX_TIME_INFINITE
        /// RSNRPZ_VAL_MAX_TIME_IMMEDIATE
        /// 
        /// Default Value: 5000 (ms)
        /// 
        /// Notes:
        /// 
        /// (1) The Maximum Time parameter applies only to this function.  It has no effect on other timeout parameters.
        /// </param>
        /// <param name="Measurement">
        /// Returns single measurement.
        /// </param>
        /// <param name="Aux_1">
        /// This control returns the first Auxiliary value.
        /// </param>
        /// <param name="Aux_2">
        /// This control returns the second Auxiliary value.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_fetchMeasurementAux(int Channel, int Timeout__ms_, out double Measurement, out double Aux_1, out double Aux_2)
        {
            int pInvokeResult = PInvoke.meass_fetchMeasurementAux(this._handle, Channel, Timeout__ms_, out Measurement, out Aux_1, out Aux_2);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function initiates an acquisition on the channels that you specifies in channel parameter.  It then waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Time__ms_">
        /// Pass the maximum length of time in which to allow the read measurement operation to complete.    
        /// 
        /// If the operation does not complete within this time interval, the function returns the RSNRPZ_ERROR_MAX_TIME_EXCEEDED error code.  When this occurs, you can call rsnrpz_chan_abort to cancel the read measurement operation and return the sensor to the Idle state.
        /// 
        /// Units:  milliseconds.  
        /// 
        /// Defined Values:
        /// RSNRPZ_VAL_MAX_TIME_INFINITE             RSNRPZ_VAL_MAX_TIME_IMMEDIATE           
        /// 
        /// Default Value: 5000 (ms)
        /// 
        /// Notes:
        /// 
        /// (1) The Maximum Time parameter applies only to this function.  It has no effect on other timeout parameters.
        /// </param>
        /// <param name="Buffer_Size">
        /// Pass the number of elements in the Measurement Array parameter.
        /// 
        /// Default Value: None
        /// 
        /// </param>
        /// <param name="Measurement_Array">
        /// Returns the measurement buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Aux_1_Array">
        /// Returns the Aux 1 buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Aux_2_Array">
        /// Returns the Aux 2 buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Read_Count">
        /// Indicates the number of points the function places in the Measurement Array parameter.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_readBufferMeasurementAux(int Channel, int Maximum_Time__ms_, int Buffer_Size, double[] Measurement_Array, double[] Aux_1_Array, double[] Aux_2_Array, out int Read_Count)
        {
            int pInvokeResult = PInvoke.meass_readBufferMeasurementAux(this._handle, Channel, Maximum_Time__ms_, Buffer_Size, Measurement_Array, Aux_1_Array, Aux_2_Array, out Read_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the buffer measurement the sensor acquires for the channel you specify.  The measurement is from an acquisition that you previously initiated.  
        /// 
        /// You use the rsnrpz_chan_initiate function to start an acquisition on the channels that you specify. You use the rsnrpz_chan_isMeasurementComplete function to determine when the acquisition is complete.
        /// 
        /// You can call the rsnrpz_meas_readBufferMeasurement function instead of the rsnrpz_chan_initiate function.  The rsnrpz_meass_readBufferMeasurement function starts an acquisition, waits for the acquisition to complete, and returns the measurement for the channel you specify.
        /// 
        /// Note:
        /// 
        /// 1) If the acquisition has not be initialized or measurement is still in progress and value is not available, function returns an error( RSNRPZ_ERROR_MEAS_NOT_AVAILABLE ).
        /// 
        /// 2) If SENSE:AUX is set to None, this function returns error.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to max available channels
        /// 
        /// Default Value: 1
        /// </param>
        /// <param name="Maximum_Time__ms_">
        /// Pass the maximum length of time in which to allow the read measurement operation to complete.    
        /// 
        /// If the operation does not complete within this time interval, the function returns the RSNRPZ_ERROR_MAX_TIME_EXCEEDED error code.  When this occurs, you can call rsnrpz_chan_abort to cancel the read measurement operation and return the sensor to the Idle state.
        /// 
        /// Units:  milliseconds.  
        /// 
        /// Defined Values:
        /// RSNRPZ_VAL_MAX_TIME_INFINITE             RSNRPZ_VAL_MAX_TIME_IMMEDIATE           
        /// 
        /// Default Value: 5000 (ms)
        /// 
        /// Notes:
        /// 
        /// (1) The Maximum Time parameter applies only to this function.  It has no effect on other timeout parameters.
        /// </param>
        /// <param name="Buffer_Size">
        /// Pass the number of elements in the Measurement Array parameter.
        /// 
        /// Default Value: None
        /// 
        /// </param>
        /// <param name="Measurement_Array">
        /// Returns the measurement buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Aux_1_Array">
        /// Returns the Aux 1 buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Aux_2_Array">
        /// Returns the Aux 2 buffer that the sensor acquires.  
        /// 
        /// </param>
        /// <param name="Read_Count">
        /// Indicates the number of points the function places in the Measurement Array parameter.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int meass_fetchBufferMeasurementAux(int Channel, int Maximum_Time__ms_, int Buffer_Size, double[] Measurement_Array, double[] Aux_1_Array, double[] Aux_2_Array, out int Read_Count)
        {
            int pInvokeResult = PInvoke.meass_fetchBufferMeasurementAux(this._handle, Channel, Maximum_Time__ms_, Buffer_Size, Measurement_Array, Aux_1_Array, Aux_2_Array, out Read_Count);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function resets the R&amp;S NRPZ registry to default values.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_preset()
        {
            int pInvokeResult = PInvoke.status_preset(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function checks selected status register for bits defined in Bitmask and returns a logical OR of all defined bits.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Status_Class">
        /// This control selects the status register.
        /// 
        /// Valid Values:
        /// RSNRPZ_STATCLASS_D_CONN (1) - Sensor Connected 
        /// RSNRPZ_STATCLASS_D_ERR  (2) - Sensor Error
        /// RSNRPZ_STATCLASS_O_CAL  (3) - Operation Calibrating Status Register
        /// RSNRPZ_STATCLASS_O_MEAS (4) - Operation Measuring Status Register
        /// RSNRPZ_STATCLASS_O_TRIGGER (5) - Operation Trigger Status Register
        /// RSNRPZ_STATCLASS_O_SENSE (6) - Operation Sense Status Register
        /// RSNRPZ_STATCLASS_O_LOWER (7) - Operation Lower Limit Fail Status Register
        /// RSNRPZ_STATCLASS_O_UPPER (8) - Operation Upper Limit Fail Status Register
        /// RSNRPZ_STATCLASS_Q_POWER (9) - Power Part of Questionable Power Status Register
        /// RSNRPZ_STATCLASS_Q_WINDOW (10) - Questionable Window Status Register
        /// RSNRPZ_STATCLASS_Q_CAL (11) - Questionable Calibration Status Register
        /// RSNRPZ_STATCLASS_Q_ZER (12) - Zero Part of Questionable Power Status Register
        /// 
        /// </param>
        /// <param name="Mask">
        /// This control defines the bit which should be checked in the specified Status Register.
        /// 
        /// You can use following constant for checking operation and questionable registers. To check multiple bits, bitwise OR them together. For example, if you want check sensor on channel 1 and sensor on channel 2, then pass 
        /// RSNRPZ_SENSOR_01 | RSNRPZ_SENSOR_02
        /// 
        /// Valid Values:
        /// constant               bit     value
        /// RSNRPZ_SENSOR_01        1       0x2
        /// RSNRPZ_SENSOR_02        2       0x4
        /// RSNRPZ_SENSOR_03        3       0x8
        /// RSNRPZ_SENSOR_04        4       0x10 
        /// RSNRPZ_SENSOR_05        5       0x20 
        /// RSNRPZ_SENSOR_06        6       0x40 
        /// RSNRPZ_SENSOR_07        7       0x80 
        /// RSNRPZ_SENSOR_08        8       0x100
        /// RSNRPZ_SENSOR_09        9       0x200
        /// RSNRPZ_SENSOR_10       10       0x400 
        /// RSNRPZ_SENSOR_11       11       0x800 
        /// RSNRPZ_SENSOR_12       12       0x1000
        /// RSNRPZ_SENSOR_13       13       0x2000 
        /// RSNRPZ_SENSOR_14       14       0x4000
        /// RSNRPZ_SENSOR_15       15       0x8000
        /// RSNRPZ_SENSOR_16       16       0x10000
        /// RSNRPZ_SENSOR_17       17       0x20000
        /// RSNRPZ_SENSOR_18       18       0x40000
        /// RSNRPZ_SENSOR_19       19       0x80000
        /// RSNRPZ_SENSOR_20       20       0x100000
        /// RSNRPZ_SENSOR_21       21       0x200000
        /// RSNRPZ_SENSOR_22       22       0x400000
        /// RSNRPZ_SENSOR_23       23       0x800000
        /// RSNRPZ_SENSOR_24       24       0x1000000
        /// RSNRPZ_SENSOR_25       25       0x2000000
        /// RSNRPZ_SENSOR_26       26       0x4000000
        /// RSNRPZ_SENSOR_27       27       0x8000000
        /// RSNRPZ_SENSOR_28       28       0x10000000
        /// RSNRPZ_SENSOR_29       29       0x20000000
        /// RSNRPZ_SENSOR_30       30       0x40000000
        /// RSNRPZ_SENSOR_31       31       0x80000000
        /// RSNRPZ_ALL_SENSORS    all       0xFFFFFFFE
        /// 
        /// Default Value:
        /// RSNRPZ_ALL_SENSORS
        /// </param>
        /// <param name="State">
        /// This control returns the bitwise OR of all bits defined by the bitmask.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - Logical True
        /// VI_FALSE (0) - Logical False
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_checkCondition(int Status_Class, uint Mask, out bool State)
        {
            ushort StateAsUShort;
            int pInvokeResult = PInvoke.status_checkCondition(this._handle, Status_Class, Mask, out StateAsUShort);
            State = System.Convert.ToBoolean(StateAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the PTransition and NTransition register of selected status register according to bitmask.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Status_Class">
        /// This control selects the status register.
        /// 
        /// Valid Values:
        /// RSNRPZ_STATCLASS_D_CONN (1) - Sensor Connected 
        /// RSNRPZ_STATCLASS_D_ERR  (2) - Sensor Error
        /// RSNRPZ_STATCLASS_O_CAL  (3) - Operation Calibrating Status Register
        /// RSNRPZ_STATCLASS_O_MEAS (4) - Operation Measuring Status Register
        /// RSNRPZ_STATCLASS_O_TRIGGER (5) - Operation Trigger Status Register
        /// RSNRPZ_STATCLASS_O_SENSE (6) - Operation Sense Status Register
        /// RSNRPZ_STATCLASS_O_LOWER (7) - Operation Lower Limit Fail Status Register
        /// RSNRPZ_STATCLASS_O_UPPER (8) - Operation Upper Limit Fail Status Register
        /// RSNRPZ_STATCLASS_Q_POWER (9) - Power Part of Questionable Power Status Register
        /// RSNRPZ_STATCLASS_Q_WINDOW (10) - Questionable Window Status Register
        /// RSNRPZ_STATCLASS_Q_CAL (11) - Questionable Calibration Status Register
        /// RSNRPZ_STATCLASS_Q_ZER (12) - Zero Part of Questionable Power Status Register
        /// 
        /// Notes:
        /// (1) For meaning of each status register consult Operation Manual.
        /// </param>
        /// <param name="Mask">
        /// This control defines the bit which should be checked in the specified Status Register.
        /// 
        /// You can use following constant for checking operation and questionable registers. To check multiple bits, bitwise OR them together. For example, if you want check sensor on channel 1 and sensor on channel 2, then pass 
        /// RSNRPZ_SENSOR_01 | RSNRPZ_SENSOR_02
        /// 
        /// Valid Values:
        /// constant               bit     value
        /// RSNRPZ_SENSOR_01        1       0x2
        /// RSNRPZ_SENSOR_02        2       0x4
        /// RSNRPZ_SENSOR_03        3       0x8
        /// RSNRPZ_SENSOR_04        4       0x10 
        /// RSNRPZ_SENSOR_05        5       0x20 
        /// RSNRPZ_SENSOR_06        6       0x40 
        /// RSNRPZ_SENSOR_07        7       0x80 
        /// RSNRPZ_SENSOR_08        8       0x100
        /// RSNRPZ_SENSOR_09        9       0x200
        /// RSNRPZ_SENSOR_10       10       0x400 
        /// RSNRPZ_SENSOR_11       11       0x800 
        /// RSNRPZ_SENSOR_12       12       0x1000
        /// RSNRPZ_SENSOR_13       13       0x2000 
        /// RSNRPZ_SENSOR_14       14       0x4000
        /// RSNRPZ_SENSOR_15       15       0x8000
        /// RSNRPZ_SENSOR_16       16       0x10000
        /// RSNRPZ_SENSOR_17       17       0x20000
        /// RSNRPZ_SENSOR_18       18       0x40000
        /// RSNRPZ_SENSOR_19       19       0x80000
        /// RSNRPZ_SENSOR_20       20       0x100000
        /// RSNRPZ_SENSOR_21       21       0x200000
        /// RSNRPZ_SENSOR_22       22       0x400000
        /// RSNRPZ_SENSOR_23       23       0x800000
        /// RSNRPZ_SENSOR_24       24       0x1000000
        /// RSNRPZ_SENSOR_25       25       0x2000000
        /// RSNRPZ_SENSOR_26       26       0x4000000
        /// RSNRPZ_SENSOR_27       27       0x8000000
        /// RSNRPZ_SENSOR_28       28       0x10000000
        /// RSNRPZ_SENSOR_29       29       0x20000000
        /// RSNRPZ_SENSOR_30       30       0x40000000
        /// RSNRPZ_SENSOR_31       31       0x80000000
        /// RSNRPZ_ALL_SENSORS    all       0xFFFFFFFE
        /// 
        /// Default Value:
        /// RSNRPZ_ALL_SENSORS
        /// </param>
        /// <param name="Direction">
        /// This control defines the direction of transition of the event.
        /// 
        /// Valid Values:
        /// RSNRPZ_DIRECTION_NONE (0) - None Transition
        /// RSNRPZ_DIRECTION_PTR (1) - Positive Transition  (Default Value)
        /// RSNRPZ_DIRECTION_NTR (2) - Negative Transition
        /// RSNRPZ_DIRECTION_BOTH (3) - Both Transition
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_catchEvent(int Status_Class, uint Mask, int Direction)
        {
            int pInvokeResult = PInvoke.status_catchEvent(this._handle, Status_Class, Mask, Direction);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function checks the selected status register for events specified by bitmask and sets returns their states. Finally all bits of shadow status register specified by Reset Action will be set to zero.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Status_Class">
        /// This control selects the status register.
        /// 
        /// Valid Values:
        /// RSNRPZ_STATCLASS_D_CONN (1) - Sensor Connected 
        /// RSNRPZ_STATCLASS_D_ERR  (2) - Sensor Error
        /// RSNRPZ_STATCLASS_O_CAL  (3) - Operation Calibrating Status Register
        /// RSNRPZ_STATCLASS_O_MEAS (4) - Operation Measuring Status Register
        /// RSNRPZ_STATCLASS_O_TRIGGER (5) - Operation Trigger Status Register
        /// RSNRPZ_STATCLASS_O_SENSE (6) - Operation Sense Status Register
        /// RSNRPZ_STATCLASS_O_LOWER (7) - Operation Lower Limit Fail Status Register
        /// RSNRPZ_STATCLASS_O_UPPER (8) - Operation Upper Limit Fail Status Register
        /// RSNRPZ_STATCLASS_Q_POWER (9) - Power Part of Questionable Power Status Register
        /// RSNRPZ_STATCLASS_Q_WINDOW (10) - Questionable Window Status Register
        /// RSNRPZ_STATCLASS_Q_CAL (11) - Questionable Calibration Status Register
        /// RSNRPZ_STATCLASS_Q_ZER (12) - Zero Part of Questionable Power Status Register
        /// 
        /// Notes:
        /// (1) For meaning of each status register consult Operation Manual.
        /// </param>
        /// <param name="Mask">
        /// This control defines the bit which should be checked in the specified Status Register.
        /// 
        /// You can use following constant for checking operation and questionable registers. To check multiple bits, bitwise OR them together. For example, if you want check sensor on channel 1 and sensor on channel 2, then pass 
        /// RSNRPZ_SENSOR_01 | RSNRPZ_SENSOR_02
        /// 
        /// Valid Values:
        /// constant               bit     value
        /// RSNRPZ_SENSOR_01        1       0x2
        /// RSNRPZ_SENSOR_02        2       0x4
        /// RSNRPZ_SENSOR_03        3       0x8
        /// RSNRPZ_SENSOR_04        4       0x10 
        /// RSNRPZ_SENSOR_05        5       0x20 
        /// RSNRPZ_SENSOR_06        6       0x40 
        /// RSNRPZ_SENSOR_07        7       0x80 
        /// RSNRPZ_SENSOR_08        8       0x100
        /// RSNRPZ_SENSOR_09        9       0x200
        /// RSNRPZ_SENSOR_10       10       0x400 
        /// RSNRPZ_SENSOR_11       11       0x800 
        /// RSNRPZ_SENSOR_12       12       0x1000
        /// RSNRPZ_SENSOR_13       13       0x2000 
        /// RSNRPZ_SENSOR_14       14       0x4000
        /// RSNRPZ_SENSOR_15       15       0x8000
        /// RSNRPZ_SENSOR_16       16       0x10000
        /// RSNRPZ_SENSOR_17       17       0x20000
        /// RSNRPZ_SENSOR_18       18       0x40000
        /// RSNRPZ_SENSOR_19       19       0x80000
        /// RSNRPZ_SENSOR_20       20       0x100000
        /// RSNRPZ_SENSOR_21       21       0x200000
        /// RSNRPZ_SENSOR_22       22       0x400000
        /// RSNRPZ_SENSOR_23       23       0x800000
        /// RSNRPZ_SENSOR_24       24       0x1000000
        /// RSNRPZ_SENSOR_25       25       0x2000000
        /// RSNRPZ_SENSOR_26       26       0x4000000
        /// RSNRPZ_SENSOR_27       27       0x8000000
        /// RSNRPZ_SENSOR_28       28       0x10000000
        /// RSNRPZ_SENSOR_29       29       0x20000000
        /// RSNRPZ_SENSOR_30       30       0x40000000
        /// RSNRPZ_SENSOR_31       31       0x80000000
        /// RSNRPZ_ALL_SENSORS    all       0xFFFFFFFE
        /// 
        /// Default Value:
        /// RSNRPZ_ALL_SENSORS
        /// </param>
        /// <param name="Reset_Mask">
        /// This control defines which bits of the shadow status register will reset to zero when finishing the function.
        /// 
        /// You can use following constant for reset operation. To reset multiple bits, bitwise OR them together. For example, if you want reset only bits corresponding with sensor on channel 1 and sensor on channel 2, then pass 
        /// RSNRPZ_SENSOR_01 | RSNRPZ_SENSOR_02
        /// 
        /// Valid Values:
        /// constant               bit     value
        /// RSNRPZ_SENSOR_01        1       0x2
        /// RSNRPZ_SENSOR_02        2       0x4
        /// RSNRPZ_SENSOR_03        3       0x8
        /// RSNRPZ_SENSOR_04        4       0x10 
        /// RSNRPZ_SENSOR_05        5       0x20 
        /// RSNRPZ_SENSOR_06        6       0x40 
        /// RSNRPZ_SENSOR_07        7       0x80 
        /// RSNRPZ_SENSOR_08        8       0x100
        /// RSNRPZ_SENSOR_09        9       0x200
        /// RSNRPZ_SENSOR_10       10       0x400 
        /// RSNRPZ_SENSOR_11       11       0x800 
        /// RSNRPZ_SENSOR_12       12       0x1000
        /// RSNRPZ_SENSOR_13       13       0x2000 
        /// RSNRPZ_SENSOR_14       14       0x4000
        /// RSNRPZ_SENSOR_15       15       0x8000
        /// RSNRPZ_SENSOR_16       16       0x10000
        /// RSNRPZ_SENSOR_17       17       0x20000
        /// RSNRPZ_SENSOR_18       18       0x40000
        /// RSNRPZ_SENSOR_19       19       0x80000
        /// RSNRPZ_SENSOR_20       20       0x100000
        /// RSNRPZ_SENSOR_21       21       0x200000
        /// RSNRPZ_SENSOR_22       22       0x400000
        /// RSNRPZ_SENSOR_23       23       0x800000
        /// RSNRPZ_SENSOR_24       24       0x1000000
        /// RSNRPZ_SENSOR_25       25       0x2000000
        /// RSNRPZ_SENSOR_26       26       0x4000000
        /// RSNRPZ_SENSOR_27       27       0x8000000
        /// RSNRPZ_SENSOR_28       28       0x10000000
        /// RSNRPZ_SENSOR_29       29       0x20000000
        /// RSNRPZ_SENSOR_30       30       0x40000000
        /// RSNRPZ_SENSOR_31       31       0x80000000
        /// RSNRPZ_ALL_SENSORS    all       0xFFFFFFFE
        /// 
        /// Default Value:
        /// RSNRPZ_ALL_SENSORS
        /// </param>
        /// <param name="Events">
        /// This control returns the bitwise OR of all bits defined by the bitmask.
        /// 
        /// Valid Values:
        /// VI_TRUE (1) - Logical True
        /// VI_FALSE (0) - Logical False
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_checkEvent(int Status_Class, uint Mask, uint Reset_Mask, out bool Events)
        {
            ushort EventsAsUShort;
            int pInvokeResult = PInvoke.status_checkEvent(this._handle, Status_Class, Mask, Reset_Mask, out EventsAsUShort);
            Events = System.Convert.ToBoolean(EventsAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function enables events defined by Bitmask in enable register respective to the selected status register.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Status_Class">
        /// This control selects the status register.
        /// 
        /// Valid Values:
        /// RSNRPZ_STATCLASS_D_CONN (1) - Sensor Connected 
        /// RSNRPZ_STATCLASS_D_ERR  (2) - Sensor Error
        /// RSNRPZ_STATCLASS_O_CAL  (3) - Operation Calibrating Status Register
        /// RSNRPZ_STATCLASS_O_MEAS (4) - Operation Measuring Status Register
        /// RSNRPZ_STATCLASS_O_TRIGGER (5) - Operation Trigger Status Register
        /// RSNRPZ_STATCLASS_O_SENSE (6) - Operation Sense Status Register
        /// RSNRPZ_STATCLASS_O_LOWER (7) - Operation Lower Limit Fail Status Register
        /// RSNRPZ_STATCLASS_O_UPPER (8) - Operation Upper Limit Fail Status Register
        /// RSNRPZ_STATCLASS_Q_POWER (9) - Power Part of Questionable Power Status Register
        /// RSNRPZ_STATCLASS_Q_WINDOW (10) - Questionable Window Status Register
        /// RSNRPZ_STATCLASS_Q_CAL (11) - Questionable Calibration Status Register
        /// RSNRPZ_STATCLASS_Q_ZER (12) - Zero Part of Questionable Power Status Register
        /// 
        /// Notes:
        /// (1) For meaning of each status register consult Operation Manual.
        /// </param>
        /// <param name="Mask">
        /// This control defines the bits(channels) which should be set to one and will generate SRQ.
        /// 
        /// You can use following constant for enabling SRQ for specified channels. To disable multiple channels, bitwise OR them together. For example, if you want enable SRQ for sensor on channel 1 and sensor on channel 2, then pass 
        /// RSNRPZ_SENSOR_01 | RSNRPZ_SENSOR_02.
        /// 
        /// Valid Values:
        /// constant               bit     value
        /// RSNRPZ_SENSOR_01        1       0x2
        /// RSNRPZ_SENSOR_02        2       0x4
        /// RSNRPZ_SENSOR_03        3       0x8
        /// RSNRPZ_SENSOR_04        4       0x10 
        /// RSNRPZ_SENSOR_05        5       0x20 
        /// RSNRPZ_SENSOR_06        6       0x40 
        /// RSNRPZ_SENSOR_07        7       0x80 
        /// RSNRPZ_SENSOR_08        8       0x100
        /// RSNRPZ_SENSOR_09        9       0x200
        /// RSNRPZ_SENSOR_10       10       0x400 
        /// RSNRPZ_SENSOR_11       11       0x800 
        /// RSNRPZ_SENSOR_12       12       0x1000
        /// RSNRPZ_SENSOR_13       13       0x2000 
        /// RSNRPZ_SENSOR_14       14       0x4000
        /// RSNRPZ_SENSOR_15       15       0x8000
        /// RSNRPZ_SENSOR_16       16       0x10000
        /// RSNRPZ_SENSOR_17       17       0x20000
        /// RSNRPZ_SENSOR_18       18       0x40000
        /// RSNRPZ_SENSOR_19       19       0x80000
        /// RSNRPZ_SENSOR_20       20       0x100000
        /// RSNRPZ_SENSOR_21       21       0x200000
        /// RSNRPZ_SENSOR_22       22       0x400000
        /// RSNRPZ_SENSOR_23       23       0x800000
        /// RSNRPZ_SENSOR_24       24       0x1000000
        /// RSNRPZ_SENSOR_25       25       0x2000000
        /// RSNRPZ_SENSOR_26       26       0x4000000
        /// RSNRPZ_SENSOR_27       27       0x8000000
        /// RSNRPZ_SENSOR_28       28       0x10000000
        /// RSNRPZ_SENSOR_29       29       0x20000000
        /// RSNRPZ_SENSOR_30       30       0x40000000
        /// RSNRPZ_SENSOR_31       31       0x80000000
        /// RSNRPZ_ALL_SENSORS    all       0xFFFFFFFE
        /// 
        /// Default Value:
        /// RSNRPZ_ALL_SENSORS
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_enableEventNotification(int Status_Class, uint Mask)
        {
            int pInvokeResult = PInvoke.status_enableEventNotification(this._handle, Status_Class, Mask);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function disables events defined by Bitmask in enable register respective to the selected status register.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Status_Class">
        /// This control selects the status register.
        /// 
        /// Valid Values:
        /// RSNRPZ_STATCLASS_D_CONN (1) - Sensor Connected 
        /// RSNRPZ_STATCLASS_D_ERR  (2) - Sensor Error
        /// RSNRPZ_STATCLASS_O_CAL  (3) - Operation Calibrating Status Register
        /// RSNRPZ_STATCLASS_O_MEAS (4) - Operation Measuring Status Register
        /// RSNRPZ_STATCLASS_O_TRIGGER (5) - Operation Trigger Status Register
        /// RSNRPZ_STATCLASS_O_SENSE (6) - Operation Sense Status Register
        /// RSNRPZ_STATCLASS_O_LOWER (7) - Operation Lower Limit Fail Status Register
        /// RSNRPZ_STATCLASS_O_UPPER (8) - Operation Upper Limit Fail Status Register
        /// RSNRPZ_STATCLASS_Q_POWER (9) - Power Part of Questionable Power Status Register
        /// RSNRPZ_STATCLASS_Q_WINDOW (10) - Questionable Window Status Register
        /// RSNRPZ_STATCLASS_Q_CAL (11) - Questionable Calibration Status Register
        /// RSNRPZ_STATCLASS_Q_ZER (12) - Zero Part of Questionable Power Status Register
        /// 
        /// Notes:
        /// (1) For meaning of each status register consult Operation Manual.
        /// </param>
        /// <param name="Mask">
        /// This control defines the bit which should be set to zero in the specified Enable Register.
        /// 
        /// You can use following constant for disabling SRQ for specified channels. To disable multiple channels, bitwise OR them together. For example, if you want disable SRQ for sensor on channel 1 and sensor on channel 2, then pass 
        /// RSNRPZ_SENSOR_01 | RSNRPZ_SENSOR_02.
        /// 
        /// Valid Values:
        /// constant               bit     value
        /// RSNRPZ_SENSOR_01        1       0x2
        /// RSNRPZ_SENSOR_02        2       0x4
        /// RSNRPZ_SENSOR_03        3       0x8
        /// RSNRPZ_SENSOR_04        4       0x10 
        /// RSNRPZ_SENSOR_05        5       0x20 
        /// RSNRPZ_SENSOR_06        6       0x40 
        /// RSNRPZ_SENSOR_07        7       0x80 
        /// RSNRPZ_SENSOR_08        8       0x100
        /// RSNRPZ_SENSOR_09        9       0x200
        /// RSNRPZ_SENSOR_10       10       0x400 
        /// RSNRPZ_SENSOR_11       11       0x800 
        /// RSNRPZ_SENSOR_12       12       0x1000
        /// RSNRPZ_SENSOR_13       13       0x2000 
        /// RSNRPZ_SENSOR_14       14       0x4000
        /// RSNRPZ_SENSOR_15       15       0x8000
        /// RSNRPZ_SENSOR_16       16       0x10000
        /// RSNRPZ_SENSOR_17       17       0x20000
        /// RSNRPZ_SENSOR_18       18       0x40000
        /// RSNRPZ_SENSOR_19       19       0x80000
        /// RSNRPZ_SENSOR_20       20       0x100000
        /// RSNRPZ_SENSOR_21       21       0x200000
        /// RSNRPZ_SENSOR_22       22       0x400000
        /// RSNRPZ_SENSOR_23       23       0x800000
        /// RSNRPZ_SENSOR_24       24       0x1000000
        /// RSNRPZ_SENSOR_25       25       0x2000000
        /// RSNRPZ_SENSOR_26       26       0x4000000
        /// RSNRPZ_SENSOR_27       27       0x8000000
        /// RSNRPZ_SENSOR_28       28       0x10000000
        /// RSNRPZ_SENSOR_29       29       0x20000000
        /// RSNRPZ_SENSOR_30       30       0x40000000
        /// RSNRPZ_SENSOR_31       31       0x80000000
        /// RSNRPZ_ALL_SENSORS    all       0xFFFFFFFE
        /// 
        /// Default Value:
        /// RSNRPZ_ALL_SENSORS
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_disableEventNotification(int Status_Class, uint Mask)
        {
            int pInvokeResult = PInvoke.status_disableEventNotification(this._handle, Status_Class, Mask);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function registers message, which will be send to specified window, when SRQ is occured.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Window_Handle">
        /// Handle to the window whose window procedure is to receive the message. If the parameter is set to 0 (NULL), the message is disabled.
        /// </param>
        /// <param name="Message_ID">
        /// Specifies the message to be posted.  If the message ID is set to 0, message will be not posted.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int status_registerWindowMessage(uint Window_Handle, uint Message_ID)
        {
            int pInvokeResult = PInvoke.status_registerWindowMessage(this._handle, Window_Handle, Message_ID);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches state checking of the instrument (reading of the Standard Event Register and checking it for error) status subsystem. Driver functions are using state checking which is by default enabled.
        /// 
        /// Note:
        /// 
        /// (1) In debug mode enable state checking.
        /// 
        /// (2) For better bus throughput and instruments performance disable state checking.
        /// 
        /// (3) When state checking is disabled driver does not check if correct instrument model or option is used with each of the functions. This might cause unexpected behaviour of the instrument.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="State_Checking">
        /// This control switches instrument state checking On or Off.
        /// 
        /// Valid Range:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On (Default Value)
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// BFFC0002  Parameter 2 (State Checking) out of range.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int errorCheckState(bool State_Checking)
        {
            int pInvokeResult = PInvoke.errorCheckState(this._handle, System.Convert.ToUInt16(State_Checking));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function resets the instrument to a known state and sends initialization commands to the instrument that set any necessary programmatic variables such as Headers Off, Short Command form, and Data Transfer Binary to the state necessary for the operation of the instrument driver.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors 
        /// </returns>
        public int reset()
        {
            int pInvokeResult = PInvoke.reset(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function runs the instrument's self test routine and returns the test result(s).
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Self_Test_Result">
        /// This control contains the value returned from the instrument self test.  Zero means success.  For any other code, see the device's operator's manual.
        /// 
        /// </param>
        /// <param name="Self_Test_Message">
        /// This control contains the string returned from the self test. See the device's operation manual for an explanation of the string's contents.
        /// 
        /// Notes:
        /// 
        /// (1) The array must contain at least 256 elements ViChar[256].
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// BFFC0002  Parameter 2 (Self-Test Result) NULL pointer.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int self_test(out short Self_Test_Result, System.Text.StringBuilder Self_Test_Message)
        {
            int pInvokeResult = PInvoke.self_test(this._handle, out Self_Test_Result, Self_Test_Message);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads an error code from the instrument's error queue.
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Error_Code">
        /// This control returns the error code read from the instrument's error queue.
        /// 
        /// </param>
        /// <param name="Error_Message">
        /// This control returns the error message string read from the instrument's error message queue.
        /// 
        /// Notes:
        /// 
        /// (1) The array must contain at least 256 elements ViChar[256].
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// BFFC0002  Parameter 2 (Error Code) NULL pointer.
        /// BFFC0003  Parameter 3 (Error Message) NULL pointer.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int error_query(out int Error_Code, System.Text.StringBuilder Error_Message)
        {
            int pInvokeResult = PInvoke.error_query(this._handle, out Error_Code, Error_Message);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the revision numbers of the instrument driver and instrument firmware, and tells the user with which  instrument firmware this revision of the driver is compatible. 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Instrument_Driver_Revision">
        /// This control returns the Instrument Driver Software Revision.
        /// 
        /// Notes:
        /// 
        /// (1) The array must contain at least 256 elements ViChar[256].
        /// </param>
        /// <param name="Firmware_Revision">
        /// This control returns the Instrument Firmware Revision.
        /// 
        /// Notes:
        /// 
        /// (1) Because instrument does not support firmware revision the array is set to empty string or ignored if used VI_NULL.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// 3FFC0105  Revision query not supported - VI_WARN_NSUP_REV_QUERY
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int revision_query(System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision)
        {
            int pInvokeResult = PInvoke.revision_query(this._handle, Instrument_Driver_Revision, Firmware_Revision);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function disconnect specified channel.
        /// 
        /// Note:
        /// 
        /// 1) rsnrpz_close function disconnect all channels automatically.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// This control accepts the Instrument Handle returned by the Initialize function to select the desired instrument driver session.
        /// 
        /// Default Value:  None
        /// </param>
        /// <param name="Channel">
        /// This control defines the channel number.
        /// 
        /// Valid Range:
        /// 1 to 31
        /// 
        /// Default Value: 2
        /// 
        /// Note:
        /// 
        /// 1) Function rsnrpz_init adds assigne sensor to channel 1 automatically.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsnrpz_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// </returns>
        public int CloseSensor(int Channel)
        {
            int pInvokeResult = PInvoke.CloseSensor(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
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
                this._handle = System.IntPtr.Zero;
            }
            this._disposed = true;
        }

        private class PInvoke
        {
            #region DLL section
            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init(string Resource_Name, ushort ID_Query, ushort Reset_Device, out System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_AddSensor", CallingConvention = CallingConvention.StdCall)]
            public static extern int AddSensor(System.IntPtr Instrument_Handle, int Channel, string Resource_Name, ushort ID_Query, ushort Reset_Device);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chans_abort", CallingConvention = CallingConvention.StdCall)]
            public static extern int chans_abort(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chans_getCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int chans_getCount(System.IntPtr Instrument_Handle, out int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chans_initiate", CallingConvention = CallingConvention.StdCall)]
            public static extern int chans_initiate(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chans_zero", CallingConvention = CallingConvention.StdCall)]
            public static extern int chans_zero(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chans_isZeroingComplete", CallingConvention = CallingConvention.StdCall)]
            public static extern int chans_isZeroingComplete(System.IntPtr Instrument_Handle, out ushort Zeroing_Completed);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chans_isMeasurementComplete", CallingConvention = CallingConvention.StdCall)]
            public static extern int chans_isMeasurementComplete(System.IntPtr Instrument_Handle, out ushort Measurement_Completed);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_mode", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_mode(System.IntPtr Instrument_Handle, int Channel, int Measurement_Mode);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timing_configureExclude", CallingConvention = CallingConvention.StdCall)]
            public static extern int timing_configureExclude(System.IntPtr Instrument_Handle, int Channel, double Exclude_Start, double Exclude_Stop);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timing_setTimingExcludeStart", CallingConvention = CallingConvention.StdCall)]
            public static extern int timing_setTimingExcludeStart(System.IntPtr Instrument_Handle, int Channel, double Exclude_Start);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timing_getTimingExcludeStart", CallingConvention = CallingConvention.StdCall)]
            public static extern int timing_getTimingExcludeStart(System.IntPtr Instrument_Handle, int Channel, out double Exclude_Start);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timing_setTimingExcludeStop", CallingConvention = CallingConvention.StdCall)]
            public static extern int timing_setTimingExcludeStop(System.IntPtr Instrument_Handle, int Channel, double Exclude_Stop);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timing_getTimingExcludeStop", CallingConvention = CallingConvention.StdCall)]
            public static extern int timing_getTimingExcludeStop(System.IntPtr Instrument_Handle, int Channel, out double Exclude_Stop);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_bandwidth_setBw", CallingConvention = CallingConvention.StdCall)]
            public static extern int bandwidth_setBw(System.IntPtr Instrument_Handle, int Channel, int Bandwidth);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_bandwidth_getBw", CallingConvention = CallingConvention.StdCall)]
            public static extern int bandwidth_getBw(System.IntPtr Instrument_Handle, int Channel, out int Bandwidth);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_bandwidth_getBwList", CallingConvention = CallingConvention.StdCall)]
            public static extern int bandwidth_getBwList(System.IntPtr Instrument_Handle, int Channel, int Buffer_Size, System.Text.StringBuilder Bandwidth_List);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_configureAvgAuto", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_configureAvgAuto(System.IntPtr Instrument_Handle, int Channel, int Resolution);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_configureAvgNSRatio", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_configureAvgNSRatio(System.IntPtr Instrument_Handle, int Channel, double Maximum_Noise_Ratio, double Upper_Time_Limit);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_configureAvgManual", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_configureAvgManual(System.IntPtr Instrument_Handle, int Channel, int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setAutoEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setAutoEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Auto_Enabled);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getAutoEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getAutoEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Auto_Enabled);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setAutoMaxMeasuringTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setAutoMaxMeasuringTime(System.IntPtr Instrument_Handle, int Channel, double Upper_Time_Limit);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getAutoMaxMeasuringTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getAutoMaxMeasuringTime(System.IntPtr Instrument_Handle, int Channel, out double Upper_Time_Limit);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setAutoNoiseSignalRatio", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setAutoNoiseSignalRatio(System.IntPtr Instrument_Handle, int Channel, double Maximum_Noise_Ratio);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getAutoNoiseSignalRatio", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getAutoNoiseSignalRatio(System.IntPtr Instrument_Handle, int Channel, out double Maximum_Noise_Ratio);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setAutoResolution", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setAutoResolution(System.IntPtr Instrument_Handle, int Channel, int Resolution);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getAutoResolution", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getAutoResolution(System.IntPtr Instrument_Handle, int Channel, out int Resolution);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setAutoType", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setAutoType(System.IntPtr Instrument_Handle, int Channel, int Method);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getAutoType", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getAutoType(System.IntPtr Instrument_Handle, int Channel, out int Method);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setCount(System.IntPtr Instrument_Handle, int Channel, int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getCount(System.IntPtr Instrument_Handle, int Channel, out int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Averaging);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Averaging);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setSlot", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setSlot(System.IntPtr Instrument_Handle, int Channel, int Timeslot);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getSlot", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getSlot(System.IntPtr Instrument_Handle, int Channel, out int Timeslot);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_setTerminalControl", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_setTerminalControl(System.IntPtr Instrument_Handle, int Channel, int Terminal_Control);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_getTerminalControl", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_getTerminalControl(System.IntPtr Instrument_Handle, int Channel, out int Terminal_Control);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_avg_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int avg_reset(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_range_setAutoEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int range_setAutoEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Auto_Range);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_range_getAutoEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int range_getAutoEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Auto_Range);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_range_setCrossoverLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int range_setCrossoverLevel(System.IntPtr Instrument_Handle, int Channel, double Crossover_Level);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_range_getCrossoverLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int range_getCrossoverLevel(System.IntPtr Instrument_Handle, int Channel, out double Crossover_Level);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_range_setRange", CallingConvention = CallingConvention.StdCall)]
            public static extern int range_setRange(System.IntPtr Instrument_Handle, int Channel, int Range);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_range_getRange", CallingConvention = CallingConvention.StdCall)]
            public static extern int range_getRange(System.IntPtr Instrument_Handle, int Channel, out int Range);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_configureCorrections", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_configureCorrections(System.IntPtr Instrument_Handle, int Channel, ushort Offset_State, double Offset, ushort Reserved_1, string Reserved_2, ushort S_Parameter_Enable);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setCorrectionFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setCorrectionFrequency(System.IntPtr Instrument_Handle, int Channel, double Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getCorrectionFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getCorrectionFrequency(System.IntPtr Instrument_Handle, int Channel, out double Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_setOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_setOffset(System.IntPtr Instrument_Handle, int Channel, double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_getOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_getOffset(System.IntPtr Instrument_Handle, int Channel, out double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_setOffsetEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_setOffsetEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Offset_State);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_getOffsetEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_getOffsetEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Offset_State);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_setSParamDeviceEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_setSParamDeviceEnabled(System.IntPtr Instrument_Handle, int Channel, ushort S_Parameter_Enable);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_getSParamDeviceEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_getSParamDeviceEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort S_Parameter_Correction);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_setSParamDevice", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_setSParamDevice(System.IntPtr Instrument_Handle, int Channel, int S_Parameter);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_getSParamDevice", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_getSParamDevice(System.IntPtr Instrument_Handle, int Channel, out int S_Parameter);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_configureSourceGammaCorr", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_configureSourceGammaCorr(System.IntPtr Instrument_Handle, int Channel, ushort Source_Gamma_Correction, double Magnitude, double Phase);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setSourceGammaMagnitude", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setSourceGammaMagnitude(System.IntPtr Instrument_Handle, int Channel, double Magnitude);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getSourceGammaMagnitude", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getSourceGammaMagnitude(System.IntPtr Instrument_Handle, int Channel, out double Magnitude);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setSourceGammaPhase", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setSourceGammaPhase(System.IntPtr Instrument_Handle, int Channel, double Phase);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getSourceGammaPhase", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getSourceGammaPhase(System.IntPtr Instrument_Handle, int Channel, out double Phase);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setSourceGammaCorrEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setSourceGammaCorrEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Source_Gamma_Correction);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getSourceGammaCorrEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getSourceGammaCorrEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Source_Gamma_Correction);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_configureReflectGammaCorr", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_configureReflectGammaCorr(System.IntPtr Instrument_Handle, int Channel, double Magnitude, double Phase);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setReflectionGammaMagn", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setReflectionGammaMagn(System.IntPtr Instrument_Handle, int Channel, double Magnitude);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getReflectionGammaMagn", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getReflectionGammaMagn(System.IntPtr Instrument_Handle, int Channel, out double Magnitude);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setReflectionGammaPhase", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setReflectionGammaPhase(System.IntPtr Instrument_Handle, int Channel, double Phase);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getReflectionGammaPhase", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getReflectionGammaPhase(System.IntPtr Instrument_Handle, int Channel, out double Phase);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setReflectionGammaUncertainty", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setReflectionGammaUncertainty(System.IntPtr Instrument_Handle, int Channel, double Uncertainty);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getReflectionGammaUncertainty", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getReflectionGammaUncertainty(System.IntPtr Instrument_Handle, int Channel, out double Uncertainty);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_configureDutyCycle", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_configureDutyCycle(System.IntPtr Instrument_Handle, int Channel, ushort Duty_Cycle_State, double Duty_Cycle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_setDutyCycle", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_setDutyCycle(System.IntPtr Instrument_Handle, int Channel, double Duty_Cycle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_getDutyCycle", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_getDutyCycle(System.IntPtr Instrument_Handle, int Channel, out double Duty_Cycle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_setDutyCycleEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_setDutyCycleEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Duty_Cycle_State);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_corr_getDutyCycleEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int corr_getDutyCycleEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Duty_Cycle_State);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setContAvAperture", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setContAvAperture(System.IntPtr Instrument_Handle, int Channel, double ContAv_Aperture);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getContAvAperture", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getContAvAperture(System.IntPtr Instrument_Handle, int Channel, out double ContAv_Aperture);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setContAvSmoothingEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setContAvSmoothingEnabled(System.IntPtr Instrument_Handle, int Channel, ushort ContAv_Smoothing);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getContAvSmoothingEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getContAvSmoothingEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort ContAv_Smoothing);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setContAvBufferedEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setContAvBufferedEnabled(System.IntPtr Instrument_Handle, int Channel, ushort ContAv_Buffered_Mode);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getContAvBufferedEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getContAvBufferedEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort ContAv_Buffered_Mode);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setContAvBufferSize", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setContAvBufferSize(System.IntPtr Instrument_Handle, int Channel, int Buffer_Size);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getContAvBufferSize", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getContAvBufferSize(System.IntPtr Instrument_Handle, int Channel, out int Buffer_Size);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setBurstDropoutTolerance", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setBurstDropoutTolerance(System.IntPtr Instrument_Handle, int Channel, double Drop_out_Tolerance);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getBurstDropoutTolerance", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getBurstDropoutTolerance(System.IntPtr Instrument_Handle, int Channel, out double Drop_out_Tolerance);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setBurstChopperEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setBurstChopperEnabled(System.IntPtr Instrument_Handle, int Channel, ushort BurstAv_Chopper);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getBurstChopperEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getBurstChopperEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort BurstAv_Chopper);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_configureTimeGate", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_configureTimeGate(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, double Offset, double Time, double Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_setOffsetTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_setOffsetTime(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_getOffsetTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_getOffsetTime(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, out double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_setTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_setTime(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, double Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_getTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_getTime(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, out double Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_setFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_setFrequency(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, double Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_getFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_getFrequency(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, out double Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_setMidOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_setMidOffset(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_getMidOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_getMidOffset(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, out double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_setMidLength", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_setMidLength(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, double Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_getMidLength", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_getMidLength(System.IntPtr Instrument_Handle, int Channel, int Select_Gate, out double Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_setChopperEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_setChopperEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Timegate_Chopper);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_timegate_getChopperEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int timegate_getChopperEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Timegate_Chopper);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_confTimegate", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_confTimegate(System.IntPtr Instrument_Handle, int Channel, double Offset, double Time, double Midamble_Offset, double Midamble_Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_confScale", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_confScale(System.IntPtr Instrument_Handle, int Channel, double Reference_Level, double Range, int Points);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setOffsetTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setOffsetTime(System.IntPtr Instrument_Handle, int Channel, double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getOffsetTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getOffsetTime(System.IntPtr Instrument_Handle, int Channel, out double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setTime(System.IntPtr Instrument_Handle, int Channel, double Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getTime(System.IntPtr Instrument_Handle, int Channel, out double Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setMidOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setMidOffset(System.IntPtr Instrument_Handle, int Channel, double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getMidOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getMidOffset(System.IntPtr Instrument_Handle, int Channel, out double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setMidLength", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setMidLength(System.IntPtr Instrument_Handle, int Channel, double Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getMidLength", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getMidLength(System.IntPtr Instrument_Handle, int Channel, out double Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setScaleRefLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setScaleRefLevel(System.IntPtr Instrument_Handle, int Channel, double Reference_Level);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getScaleRefLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getScaleRefLevel(System.IntPtr Instrument_Handle, int Channel, out double Reference_Level);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setScaleRange", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setScaleRange(System.IntPtr Instrument_Handle, int Channel, double Range);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getScaleRange", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getScaleRange(System.IntPtr Instrument_Handle, int Channel, out double Range);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_setScalePoints", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_setScalePoints(System.IntPtr Instrument_Handle, int Channel, int Points);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getScalePoints", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getScalePoints(System.IntPtr Instrument_Handle, int Channel, out int Points);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_stat_getScaleWidth", CallingConvention = CallingConvention.StdCall)]
            public static extern int stat_getScaleWidth(System.IntPtr Instrument_Handle, int Channel, out double Width);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_configureTimeSlot", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_configureTimeSlot(System.IntPtr Instrument_Handle, int Channel, int Time_Slot_Count, double Width);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_setTimeSlotCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_setTimeSlotCount(System.IntPtr Instrument_Handle, int Channel, int Time_Slot_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_getTimeSlotCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_getTimeSlotCount(System.IntPtr Instrument_Handle, int Channel, out int Time_Slot_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_setTimeSlotWidth", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_setTimeSlotWidth(System.IntPtr Instrument_Handle, int Channel, double Width);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_getTimeSlotWidth", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_getTimeSlotWidth(System.IntPtr Instrument_Handle, int Channel, out double Width);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_setTimeSlotMidOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_setTimeSlotMidOffset(System.IntPtr Instrument_Handle, int Channel, double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_getTimeSlotMidOffset", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_getTimeSlotMidOffset(System.IntPtr Instrument_Handle, int Channel, out double Offset);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_setTimeSlotMidLength", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_setTimeSlotMidLength(System.IntPtr Instrument_Handle, int Channel, double Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_getTimeSlotMidLength", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_getTimeSlotMidLength(System.IntPtr Instrument_Handle, int Channel, out double Length);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_setTimeSlotChopperEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_setTimeSlotChopperEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Time_Slot_Chopper);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_tslot_getTimeSlotChopperEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int tslot_getTimeSlotChopperEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Time_Slot_Chopper);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_configureScope", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_configureScope(System.IntPtr Instrument_Handle, int Channel, int Scope_Points, double Scope_Time, double Offset_Time, ushort Realtime);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_fastZero", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_fastZero(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAverageEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAverageEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Scope_Averaging);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAverageEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAverageEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Scope_Averaging);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAverageCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAverageCount(System.IntPtr Instrument_Handle, int Channel, int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAverageCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAverageCount(System.IntPtr Instrument_Handle, int Channel, out int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAverageTerminalControl", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAverageTerminalControl(System.IntPtr Instrument_Handle, int Channel, int Terminal_Control);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAverageTerminalControl", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAverageTerminalControl(System.IntPtr Instrument_Handle, int Channel, out int Terminal_Control);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setOffsetTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setOffsetTime(System.IntPtr Instrument_Handle, int Channel, double Offset_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getOffsetTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getOffsetTime(System.IntPtr Instrument_Handle, int Channel, out double Offset_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setPoints", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setPoints(System.IntPtr Instrument_Handle, int Channel, int Scope_Points);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getPoints", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getPoints(System.IntPtr Instrument_Handle, int Channel, out int Scope_Points);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setRealtimeEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setRealtimeEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Realtime);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getRealtimeEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getRealtimeEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Realtime);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setTime(System.IntPtr Instrument_Handle, int Channel, double Scope_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getTime(System.IntPtr Instrument_Handle, int Channel, out double Scope_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAutoEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAutoEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Auto_Enabled);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAutoEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAutoEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Auto_Enabled);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAutoMaxMeasuringTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAutoMaxMeasuringTime(System.IntPtr Instrument_Handle, int Channel, double Upper_Time_Limit);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAutoMaxMeasuringTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAutoMaxMeasuringTime(System.IntPtr Instrument_Handle, int Channel, out double Upper_Time_Limit);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAutoNoiseSignalRatio", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAutoNoiseSignalRatio(System.IntPtr Instrument_Handle, int Channel, double Maximum_Noise_Ratio);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAutoNoiseSignalRatio", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAutoNoiseSignalRatio(System.IntPtr Instrument_Handle, int Channel, out double Maximum_Noise_Ratio);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAutoResolution", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAutoResolution(System.IntPtr Instrument_Handle, int Channel, int Resolution);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAutoResolution", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAutoResolution(System.IntPtr Instrument_Handle, int Channel, out int Resolution);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_setAutoType", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_setAutoType(System.IntPtr Instrument_Handle, int Channel, int Method);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_scope_getAutoType", CallingConvention = CallingConvention.StdCall)]
            public static extern int scope_getAutoType(System.IntPtr Instrument_Handle, int Channel, out int Method);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_configureInternal", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_configureInternal(System.IntPtr Instrument_Handle, int Channel, double Trigger_Level, int Trigger_Slope);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_configureExternal", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_configureExternal(System.IntPtr Instrument_Handle, int Channel, double Trigger_Delay);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_immediate", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_immediate(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setAutoDelayEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setAutoDelayEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Auto_Delay);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getAutoDelayEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getAutoDelayEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Auto_Delay);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setAutoTriggerEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setAutoTriggerEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Auto_Trigger);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getAutoTriggerEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getAutoTriggerEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Auto_Trigger);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setCount(System.IntPtr Instrument_Handle, int Channel, int Trigger_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getCount(System.IntPtr Instrument_Handle, int Channel, out int Trigger_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setDelay", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setDelay(System.IntPtr Instrument_Handle, int Channel, double Trigger_Delay);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getDelay", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getDelay(System.IntPtr Instrument_Handle, int Channel, out double Trigger_Delay);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setHoldoff", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setHoldoff(System.IntPtr Instrument_Handle, int Channel, double Trigger_Holdoff);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getHoldoff", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getHoldoff(System.IntPtr Instrument_Handle, int Channel, out double Trigger_Holdoff);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setHysteresis", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setHysteresis(System.IntPtr Instrument_Handle, int Channel, double Trigger_Hysteresis);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getHysteresis", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getHysteresis(System.IntPtr Instrument_Handle, int Channel, out double Trigger_Hysteresis);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setLevel(System.IntPtr Instrument_Handle, int Channel, double Trigger_Level);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getLevel(System.IntPtr Instrument_Handle, int Channel, out double Trigger_Level);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setSlope", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setSlope(System.IntPtr Instrument_Handle, int Channel, int Trigger_Slope);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getSlope", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getSlope(System.IntPtr Instrument_Handle, int Channel, out int Trigger_Slope);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setSource(System.IntPtr Instrument_Handle, int Channel, int Trigger_Source);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getSource(System.IntPtr Instrument_Handle, int Channel, out int Trigger_Source);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_setDropoutTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_setDropoutTime(System.IntPtr Instrument_Handle, int Channel, double Dropout_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_trigger_getDropoutTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int trigger_getDropoutTime(System.IntPtr Instrument_Handle, int Channel, out double Dropout_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_info", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_info(System.IntPtr Instrument_Handle, int Channel, string Info_Type, int Array_Size, System.Text.StringBuilder Info);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_infoHeader", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_infoHeader(System.IntPtr Instrument_Handle, int Channel, int Parameter_Number, int Array_Size, System.Text.StringBuilder Header);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_infosCount", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_infosCount(System.IntPtr Instrument_Handle, int Channel, out int Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_system_setStatusUpdateTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int system_setStatusUpdateTime(System.IntPtr Instrument_Handle, int Channel, double Status_Update_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_system_getStatusUpdateTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int system_getStatusUpdateTime(System.IntPtr Instrument_Handle, int Channel, out double Status_Update_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_system_setResultUpdateTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int system_setResultUpdateTime(System.IntPtr Instrument_Handle, int Channel, double Result_Update_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_system_getResultUpdateTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int system_getResultUpdateTime(System.IntPtr Instrument_Handle, int Channel, out double Result_Update_Time);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_abort", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_abort(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_initiate", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_initiate(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setInitContinuousEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setInitContinuousEnabled(System.IntPtr Instrument_Handle, int Channel, ushort Continuous_Initiate);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getInitContinuousEnabled", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getInitContinuousEnabled(System.IntPtr Instrument_Handle, int Channel, out ushort Continuous_Initiate);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_reset(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setSamplingFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setSamplingFrequency(System.IntPtr Instrument_Handle, int Channel, int Sampling_Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getSamplingFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getSamplingFrequency(System.IntPtr Instrument_Handle, int Channel, out int Sampling_Frequency);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_zero", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_zero(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_zeroAdvanced", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_zeroAdvanced(System.IntPtr Instrument_Handle, int Channel, int Zeroing);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_isZeroComplete", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_isZeroComplete(System.IntPtr Instrument_Handle, int Channel, out ushort Zeroing_Complete);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_isMeasurementComplete", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_isMeasurementComplete(System.IntPtr Instrument_Handle, int Channel, out ushort Measurement_Complete);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_selfTest", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_selfTest(System.IntPtr Instrument_Handle, int Channel, System.Text.StringBuilder Result);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_setAuxiliary", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_setAuxiliary(System.IntPtr Instrument_Handle, int Channel, int Auxiliary_Value);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_chan_getAuxiliary", CallingConvention = CallingConvention.StdCall)]
            public static extern int chan_getAuxiliary(System.IntPtr Instrument_Handle, int Channel, out int Auxiliary_Value);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_readMeasurement", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_readMeasurement(System.IntPtr Instrument_Handle, int Channel, int Timeout__ms_, out double Measurement);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_fetchMeasurement", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_fetchMeasurement(System.IntPtr Instrument_Handle, int Channel, out double Measurement);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_readBufferMeasurement", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_readBufferMeasurement(System.IntPtr Instrument_Handle, int Channel, int Maximum_Time__ms_, int Buffer_Size, [In, Out] double[] Measurement_Array, out int Read_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_fetchBufferMeasurement", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_fetchBufferMeasurement(System.IntPtr Instrument_Handle, int Channel, int Array_Size, [In, Out] double[] Measurement_Array, out int Read_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_sendSoftwareTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_sendSoftwareTrigger(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_readMeasurementAux", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_readMeasurementAux(System.IntPtr Instrument_Handle, int Channel, int Timeout__ms_, out double Measurement, out double Aux_1, out double Aux_2);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_fetchMeasurementAux", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_fetchMeasurementAux(System.IntPtr Instrument_Handle, int Channel, int Timeout__ms_, out double Measurement, out double Aux_1, out double Aux_2);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_readBufferMeasurementAux", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_readBufferMeasurementAux(System.IntPtr Instrument_Handle, int Channel, int Maximum_Time__ms_, int Buffer_Size, [In, Out] double[] Measurement_Array, [In, Out] double[] Aux_1_Array, [In, Out] double[] Aux_2_Array, out int Read_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_meass_fetchBufferMeasurementAux", CallingConvention = CallingConvention.StdCall)]
            public static extern int meass_fetchBufferMeasurementAux(System.IntPtr Instrument_Handle, int Channel, int Maximum_Time__ms_, int Buffer_Size, [In, Out] double[] Measurement_Array, [In, Out] double[] Aux_1_Array, [In, Out] double[] Aux_2_Array, out int Read_Count);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_preset", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_preset(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_checkCondition", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_checkCondition(System.IntPtr Instrument_Handle, int Status_Class, uint Mask, out ushort State);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_catchEvent", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_catchEvent(System.IntPtr Instrument_Handle, int Status_Class, uint Mask, int Direction);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_checkEvent", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_checkEvent(System.IntPtr Instrument_Handle, int Status_Class, uint Mask, uint Reset_Mask, out ushort Events);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_enableEventNotification", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_enableEventNotification(System.IntPtr Instrument_Handle, int Status_Class, uint Mask);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_disableEventNotification", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_disableEventNotification(System.IntPtr Instrument_Handle, int Status_Class, uint Mask);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_status_registerWindowMessage", CallingConvention = CallingConvention.StdCall)]
            public static extern int status_registerWindowMessage(System.IntPtr Instrument_Handle, uint Window_Handle, uint Message_ID);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_errorCheckState", CallingConvention = CallingConvention.StdCall)]
            public static extern int errorCheckState(System.IntPtr Instrument_Handle, ushort State_Checking);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test(System.IntPtr Instrument_Handle, out short Self_Test_Result, System.Text.StringBuilder Self_Test_Message);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query(System.IntPtr Instrument_Handle, out int Error_Code, System.Text.StringBuilder Error_Message);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query(System.IntPtr Instrument_Handle, System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_CloseSensor", CallingConvention = CallingConvention.StdCall)]
            public static extern int CloseSensor(System.IntPtr Instrument_Handle, int Channel);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close(System.IntPtr Instrument_Handle);

            [DllImport("rsnrpz_32.dll", EntryPoint = "rsnrpz_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message(System.IntPtr Instrument_Handle, int Status_Code, System.Text.StringBuilder Message);

            #endregion

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
                System.Text.StringBuilder msg = new System.Text.StringBuilder(256);
                PInvoke.error_message(handle, code, msg);
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            }
        }
    }


    public class rsnrpzConstants
    {

        public const int SensorModeContav = 0;

        public const int SensorModeBufContav = 1;

        public const int SensorModeTimeslot = 2;

        public const int SensorModeBurst = 3;

        public const int SensorModeScope = 4;

        public const int SensorModeTimegate = 5;

        public const int SensorModeCcdf = 6;

        public const int SensorModePdf = 7;

        public const int AutoCountTypeResolution = 0;

        public const int AutoCountTypeNsr = 1;

        public const int TerminalControlMoving = 0;

        public const int TerminalControlRepeat = 1;

        public const int SlopePositive = 0;

        public const int SlopeNegative = 1;

        public const int TriggerSourceBus = 0;

        public const int TriggerSourceExternal = 1;

        public const int TriggerSourceHold = 2;

        public const int TriggerSourceImmediate = 3;

        public const int TriggerSourceInternal = 4;

        public const int SamplingFrequency1 = 1;

        public const int SamplingFrequency2 = 2;

        public const int ZeroLfr = 3;

        public const int ZeroUfr = 4;

        public const int AuxNone = 0;

        public const int AuxMinmax = 1;

        public const int AuxRndmax = 2;

        public const int StatclassDConn = 1;

        public const int StatclassDErr = 2;

        public const int StatclassOCal = 3;

        public const int StatclassOMeas = 4;

        public const int StatclassOTrigger = 5;

        public const int StatclassOSense = 6;

        public const int StatclassOLower = 7;

        public const int StatclassOUpper = 8;

        public const int StatclassQPower = 9;

        public const int StatclassQWindow = 10;

        public const int StatclassQCal = 11;

        public const int StatclassQZer = 12;

        public const int DirectionPtr = 1;

        public const int DirectionNtr = 2;

        public const int DirectionBoth = 3;

        public const int DirectionNone = 0;
    }
}
