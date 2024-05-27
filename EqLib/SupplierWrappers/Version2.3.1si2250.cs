//------------------------------------------------------------------------------
//  Do not modify. Any changes to this file may cause incorrect behavior.
//
//  Runtime Version:4.0.30319.42000
//------------------------------------------------------------------------------
namespace SignalCraftTechnologies.ModularInstruments.Interop
{
    using System;
    using System.Runtime.InteropServices;

    public class si2250 : object, System.IDisposable
    {

        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _disposed = true;

        ~si2250() { Dispose(false); }


        /// <summary>
        /// This function creates an IVI instrument driver session, typically using the C session instrument handle.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// The instrument handle that is used to create an IVI instrument driver session.
        /// </param>
        public si2250(System.IntPtr Instrument_Handle)
        {
            this._handle = new System.Runtime.InteropServices.HandleRef(this, Instrument_Handle);
            this._disposed = false;
        }

        /// <summary>
        /// Opens the I/O session to the instrument. Driver methods and properties that access the instrument are only accessible after Initialize is called. Initialize optionally performs a Reset and queries the instrument to validate the instrument model.
        /// </summary>
        /// <param name="ResourceName">
        /// An IVI logical name or an instrument specific string that identifies the address of the instrument, such as a VISA resource descriptor string.
        /// </param>
        /// <param name="IdQuery">
        /// Specifies whether to verify the ID of the instrument.
        /// </param>
        /// <param name="Reset">
        /// Specifies whether to reset the instrument.
        /// </param>
        /// <param name="Vi">
        /// Unique identifier for an IVI session.
        /// </param>
        public si2250(string ResourceName, bool IdQuery, bool Reset)
        {
            System.IntPtr instrumentHandle;
            int pInvokeResult = PInvoke.init(ResourceName, System.Convert.ToUInt16(IdQuery), System.Convert.ToUInt16(Reset), out instrumentHandle);
            this._handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// Opens the I/O session to the instrument.  Driver methods and properties that access the instrument are only accessible after Initialize is called.  Initialize optionally performs a Reset and queries the instrument to validate the instrument model.
        /// </summary>
        /// <param name="ResourceName">
        /// An IVI logical name or an instrument specific string that identifies the address of the instrument, such as a VISA resource descriptor string.
        /// </param>
        /// <param name="IdQuery">
        /// Specifies whether to verify the ID of the instrument.
        /// </param>
        /// <param name="Reset">
        /// Specifies whether to reset the instrument.
        /// </param>
        /// <param name="OptionsString">
        /// The user can use the OptionsString parameter to specify the initial values of certain IVI inherent attributes for the session. The format of an assignment in the OptionsString parameter is "Name=Value", where Name is one of: RangeCheck, QueryInstrStatus, Cache, Simulate, RecordCoercions, InterchangeCheck,or DriverSetup. Value is either true or false except for DriverSetup. If the OptionString parameter contains an assignment for the Driver Setup attribute, the Initialize function assumes that everything following "DriverSetup=" is part of the assignment.
        /// </param>
        /// <param name="Vi">
        /// Unique identifier for an IVI session.
        /// </param>
        public si2250(string ResourceName, bool IdQuery, bool Reset, string OptionsString)
        {
            System.IntPtr instrumentHandle;
            int pInvokeResult = PInvoke.InitWithOptions(ResourceName, System.Convert.ToUInt16(IdQuery), System.Convert.ToUInt16(Reset), OptionsString, out instrumentHandle);
            this._handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// Gets the instrument handle.
        /// </summary>
        /// <value>
        /// The value is the IntPtr that represents the handle to the instrument.
        /// </value>
        public System.IntPtr Handle
        {
            get
            {
                return this._handle.Handle;
            }
        }

        /// <summary>
        /// Clear the external calibration.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int ClearExternalCalibration()
        {
            int pInvokeResult = PInvoke.ClearExternalCalibration(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Set the external calibration.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int SetExternalCalibration()
        {
            int pInvokeResult = PInvoke.SetExternalCalibration(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the LO signal source used to convert the RX input signal.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Source">
        /// Specifies the LO source to select.
        /// </param>
        public int ConfigureRXLOSource(int Source)
        {
            int pInvokeResult = PInvoke.ConfigureRXLOSource(this._handle, Source);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the LO signal source used to convert the TX input signal.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Source">
        /// Specifies the LO source to select.
        /// </param>
        public int ConfigureTXLOSource(int Source)
        {
            int pInvokeResult = PInvoke.ConfigureTXLOSource(this._handle, Source);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the RX signal path for harmonic conversion.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Fundamental">
        /// Specifies the fundamental frequency, in Hertz (Hz).
        /// </param>
        /// <param name="HarmonicIndex">
        /// Specifies the harmonic of interest.
        /// </param>
        /// <param name="OutFrequency">
        /// The output frequency of the converted harmonic, in Hertz (Hz).
        /// </param>
        public int ConfigureHarmonicConverter(double Fundamental, int HarmonicIndex, out double OutFrequency)
        {
            int pInvokeResult = PInvoke.ConfigureHarmonicConverter(this._handle, Fundamental, HarmonicIndex, out OutFrequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the path option of the RX bypass signal path.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="PathOption">
        /// Specifies the selected Bypass path.
        /// </param>
        public int ConfigureRXBypassPath(int PathOption)
        {
            int pInvokeResult = PInvoke.ConfigureRXBypassPath(this._handle, PathOption);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the RF gain path option of the RX conversion signal path.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="GainPath">
        /// Specifies the conversion gain path.
        /// </param>
        public int ConfigureRXConversionGainPath(int GainPath)
        {
            int pInvokeResult = PInvoke.ConfigureRXConversionGainPath(this._handle, GainPath);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the IF filter path option of the RX conversion signal path.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="FilterOption">
        /// Specifies the IF filter option.
        /// </param>
        public int ConfigureRXIFFilterPath(int FilterOption)
        {
            int pInvokeResult = PInvoke.ConfigureRXIFFilterPath(this._handle, FilterOption);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the RX input signal frequency.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Frequency">
        /// Specifies the input frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureRXINFrequency(double Frequency)
        {
            int pInvokeResult = PInvoke.ConfigureRXINFrequency(this._handle, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the RX output signal frequency.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Frequency">
        /// Specifies the output frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureRXOUTFrequency(double Frequency)
        {
            int pInvokeResult = PInvoke.ConfigureRXOUTFrequency(this._handle, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the TX output OUT1 and OUT2 signal path for calibration tone generation.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="OutFrequency">
        /// Specifies the output frequency, in Hertz (Hz).
        /// </param>
        /// <param name="InFrequency">
        /// The input frequency required, in Hertz (Hz).
        /// </param>
        public int ConfigureCalibrationTone(double OutFrequency, out double InFrequency)
        {
            int pInvokeResult = PInvoke.ConfigureCalibrationTone(this._handle, OutFrequency, out InFrequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the TX input signal frequency.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Frequency">
        /// Specifies the input frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureTXINFrequency(double Frequency)
        {
            int pInvokeResult = PInvoke.ConfigureTXINFrequency(this._handle, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the TX output OUT1 and OUT2 signal frequency.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Frequency">
        /// Specifies the output frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureTXOUTFrequency(double Frequency)
        {
            int pInvokeResult = PInvoke.ConfigureTXOUTFrequency(this._handle, Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Configure the TX output signal path and destination.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="PathOption">
        /// Specifies the selected output and path of the TX path.
        /// </param>
        public int ConfigureTXOutputPath(int PathOption)
        {
            int pInvokeResult = PInvoke.ConfigureTXOutputPath(this._handle, PathOption);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Measure the temperature of the hardware module, in degrees Celsius.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Temperature">
        /// The temperature, in degrees Celsius (?C).
        /// </param>
        public int MeasureTemperature(out double Temperature)
        {
            int pInvokeResult = PInvoke.MeasureTemperature(this._handle, out Temperature);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Measure the signal power at the TX output OUT1 and OUT2, in dBm.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Power">
        /// The power level, in dBm.
        /// </param>
        public int MeasureTXOUTPower(out double Power)
        {
            int pInvokeResult = PInvoke.MeasureTXOUTPower(this._handle, out Power);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Get the LO signal frequency currently configured (onboard) or required (LO IN) for the RX conversion signal path.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Frequency">
        /// LO frequency, in Hertz (Hz).
        /// </param>
        public int GetRXLOFrequency(out double Frequency)
        {
            int pInvokeResult = PInvoke.GetRXLOFrequency(this._handle, out Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Get the factory calibrated gain from RX IN to RX OUT at the currently configured frequency settings.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Gain">
        /// Gain, in dB.
        /// </param>
        public int GetRXPathGain(out double Gain)
        {
            int pInvokeResult = PInvoke.GetRXPathGain(this._handle, out Gain);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Get the LO signal frequency currently configured (onboard) or required (LO IN) for the TX conversion signal path.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Frequency">
        /// LO frequency, in Hertz (Hz).
        /// </param>
        public int GetTXLOFrequency(out double Frequency)
        {
            int pInvokeResult = PInvoke.GetTXLOFrequency(this._handle, out Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Get the factory calibrated gain from TX IN to the TX output selected at the currently configured frequency settings.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="Gain">
        /// Gain, in dB.
        /// </param>
        public int GetTXPathGain(out double Gain)
        {
            int pInvokeResult = PInvoke.GetTXPathGain(this._handle, out Gain);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Clears the list of interchangeability warnings that the IVI specific driver maintains.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int ClearInterchangeWarnings()
        {
            int pInvokeResult = PInvoke.ClearInterchangeWarnings(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Quickly places the instrument in a state where it has no, or minimal, effect on the external system to which it is connected.  This state is not necessarily a known state.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int Disable()
        {
            int pInvokeResult = PInvoke.Disable(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Queries the instrument and returns instrument specific error information.  This function can be used when QueryInstrumentStatus is True to retrieve error details when the driver detects an instrument error.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="ErrorCode">
        /// Instrument error code
        /// </param>
        /// <param name="ErrorMessage">
        /// Instrument error message
        /// </param>
        public int error_query(out int ErrorCode, System.Text.StringBuilder ErrorMessage)
        {
            int pInvokeResult = PInvoke.error_query(this._handle, out ErrorCode, ErrorMessage);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the oldest record from the coercion record list.  Records are only added to the list if RecordCoercions is True.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="CoercionRecordBufferSize">
        /// The number of bytes in the ViChar array that the user specifies for the CoercionRecord parameter.
        /// </param>
        /// <param name="CoercionRecord">
        /// The coercion record string shall contain the following information: (1) The name of the attribute that was coerced.  This can be the generic name, the COM property name, or the C defined constant. (2) If the attribute is channel-based, the name of the channel.   The channel name can be the specific driver channel string or the virtual channel name that the user specified.(3) If the attribute applies to a repeated capability, the name of the repeated capability. The name can be the specific driver repeated capability token or the virtual repeated capability name that the user specified.(4) The value that the user specified for the attribute.(5) The value to which the attribute was coerced.
        /// </param>
        public int GetNextCoercionRecord(int CoercionRecordBufferSize, System.Text.StringBuilder CoercionRecord)
        {
            int pInvokeResult = PInvoke.GetNextCoercionRecord(this._handle, CoercionRecordBufferSize, CoercionRecord);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the oldest warning from the interchange warning list.  Records are only added to the list if InterchangeCheck is True.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="InterchangeWarningBufferSize">
        /// The number of bytes in the ViChar array that the user specifies for the InterchangeWarning parameter.
        /// </param>
        /// <param name="InterchangeWarning">
        /// A string describing the oldest interchangeability warning or empty string if no warrnings remain.
        /// </param>
        public int GetNextInterchangeWarning(int InterchangeWarningBufferSize, System.Text.StringBuilder InterchangeWarning)
        {
            int pInvokeResult = PInvoke.GetNextInterchangeWarning(this._handle, InterchangeWarningBufferSize, InterchangeWarning);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Invalidates all of the driver's cached values.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int InvalidateAllAttributes()
        {
            int pInvokeResult = PInvoke.InvalidateAllAttributes(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Places the instrument in a known state and configures instrument options on which the IVI specific driver depends (for example, enabling/disabling headers).  For an IEEE 488.2 instrument, Reset sends the command string *RST to the instrument.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int reset()
        {
            int pInvokeResult = PInvoke.reset(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Resets the interchangeability checking algorithms of the driver so that methods and properties that executed prior to calling this function have no affect on whether future calls to the driver generate interchangeability warnings.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int ResetInterchangeCheck()
        {
            int pInvokeResult = PInvoke.ResetInterchangeCheck(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Does the equivalent of Reset and then, (1) disables class extension capability groups, (2) sets attributes to initial values defined by class specs, and (3) configures the driver to option string settings used when Initialize was last executed.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        public int ResetWithDefaults()
        {
            int pInvokeResult = PInvoke.ResetWithDefaults(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Retrieves revision information from the instrument.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="DriverRev">
        /// Returns the revision of the IVI specific driver, which is the value held in the Specific Driver Revision attribute. Refer to the Specific Driver Revision attribute for more information.
        /// </param>
        /// <param name="InstrRev">
        /// Returns the firmware revision of the instrument, which is the value held in the Instrument Firmware Revision attribute. Refer to the Instrument Firmware Revision attribute for more information.
        /// </param>
        public int revision_query(System.Text.StringBuilder DriverRev, System.Text.StringBuilder InstrRev)
        {
            int pInvokeResult = PInvoke.revision_query(this._handle, DriverRev, InstrRev);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Performs an instrument self test, waits for the instrument to complete the test, and queries the instrument for the results.  If the instrument passes the test, TestResult is zero and TestMessage is 'Self test passed'.
        /// </summary>
        /// <param name="Vi">
        /// The ViSession handle that you obtain from the IviDriver_init or IviDriver_InitWithOptions function.  The handle identifies a particular instrument session.
        /// </param>
        /// <param name="TestResult">
        /// The numeric result from the self test operation. 0 = no error (test passed)
        /// </param>
        /// <param name="TestMessage">
        /// The self test status message
        /// </param>
        public int self_test(out short TestResult, System.Text.StringBuilder TestMessage)
        {
            int pInvokeResult = PInvoke.self_test(this._handle, out TestResult, TestMessage);
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
                this._handle = new System.Runtime.InteropServices.HandleRef(null, System.IntPtr.Zero);
            }
            this._disposed = true;
        }

        public bool GetBoolean(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            ushort val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return System.Convert.ToBoolean(val);
        }

        public bool GetBoolean(si2250Properties propertyId)
        {
            return this.GetBoolean(propertyId, "");
        }

        public int GetInt32(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            int val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }

        public int GetInt32(si2250Properties propertyId)
        {
            return this.GetInt32(propertyId, "");
        }

        public double GetDouble(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            double val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }

        public double GetDouble(si2250Properties propertyId)
        {
            return this.GetDouble(propertyId, "");
        }

        public string GetString(si2250Properties propertyId, string repeatedCapabilityOrChannel)
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

        public string GetString(si2250Properties propertyId)
        {
            return this.GetString(propertyId, "");
        }

        public void SetBoolean(si2250Properties propertyId, string repeatedCapabilityOrChannel, bool val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), System.Convert.ToUInt16(val)));
        }

        public void SetBoolean(si2250Properties propertyId, bool val)
        {
            this.SetBoolean(propertyId, "", val);
        }

        public void SetInt32(si2250Properties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetInt32(si2250Properties propertyId, int val)
        {
            this.SetInt32(propertyId, "", val);
        }

        public void SetDouble(si2250Properties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetDouble(si2250Properties propertyId, double val)
        {
            this.SetDouble(propertyId, "", val);
        }

        public void SetString(si2250Properties propertyId, string repeatedCapabilityOrChannel, string val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetString(si2250Properties propertyId, string val)
        {
            this.SetString(propertyId, "", val);
        }

        private class PInvoke
        {
            private const string nativeDLLName32 = "si2250.dll";
            private const string nativeDLLName64 = "si2250_64.dll";
            private static readonly bool is64BitProcess = (IntPtr.Size == 8);

            [DllImport(nativeDLLName32, EntryPoint = "si2250_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init_32(string ResourceName, ushort IdQuery, ushort Reset, out System.IntPtr Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init_64(string ResourceName, ushort IdQuery, ushort Reset, out System.IntPtr Vi);
            public static int init(string ResourceName, ushort IdQuery, ushort Reset, out System.IntPtr Vi)
            {
                if (is64BitProcess)
                    return init_64(ResourceName, IdQuery, Reset, out Vi);
                else
                    return init_32(ResourceName, IdQuery, Reset, out Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions_32(string ResourceName, ushort IdQuery, ushort Reset, string OptionsString, out System.IntPtr Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions_64(string ResourceName, ushort IdQuery, ushort Reset, string OptionsString, out System.IntPtr Vi);
            public static int InitWithOptions(string ResourceName, ushort IdQuery, ushort Reset, string OptionsString, out System.IntPtr Vi)
            {
                if (is64BitProcess)
                    return InitWithOptions_64(ResourceName, IdQuery, Reset, OptionsString, out Vi);
                else
                    return InitWithOptions_32(ResourceName, IdQuery, Reset, OptionsString, out Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ClearExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearExternalCalibration_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ClearExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearExternalCalibration_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int ClearExternalCalibration(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return ClearExternalCalibration_64(Vi);
                else
                    return ClearExternalCalibration_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_SetExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetExternalCalibration_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_SetExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetExternalCalibration_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int SetExternalCalibration(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return SetExternalCalibration_64(Vi);
                else
                    return SetExternalCalibration_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureRXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXLOSource_32(System.Runtime.InteropServices.HandleRef Vi, int Source);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureRXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXLOSource_64(System.Runtime.InteropServices.HandleRef Vi, int Source);
            public static int ConfigureRXLOSource(System.Runtime.InteropServices.HandleRef Vi, int Source)
            {
                if (is64BitProcess)
                    return ConfigureRXLOSource_64(Vi, Source);
                else
                    return ConfigureRXLOSource_32(Vi, Source);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureTXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXLOSource_32(System.Runtime.InteropServices.HandleRef Vi, int Source);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureTXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXLOSource_64(System.Runtime.InteropServices.HandleRef Vi, int Source);
            public static int ConfigureTXLOSource(System.Runtime.InteropServices.HandleRef Vi, int Source)
            {
                if (is64BitProcess)
                    return ConfigureTXLOSource_64(Vi, Source);
                else
                    return ConfigureTXLOSource_32(Vi, Source);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureHarmonicConverter", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureHarmonicConverter_32(System.Runtime.InteropServices.HandleRef Vi, double Fundamental, int HarmonicIndex, out double OutFrequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureHarmonicConverter", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureHarmonicConverter_64(System.Runtime.InteropServices.HandleRef Vi, double Fundamental, int HarmonicIndex, out double OutFrequency);
            public static int ConfigureHarmonicConverter(System.Runtime.InteropServices.HandleRef Vi, double Fundamental, int HarmonicIndex, out double OutFrequency)
            {
                if (is64BitProcess)
                    return ConfigureHarmonicConverter_64(Vi, Fundamental, HarmonicIndex, out OutFrequency);
                else
                    return ConfigureHarmonicConverter_32(Vi, Fundamental, HarmonicIndex, out OutFrequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureRXBypassPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXBypassPath_32(System.Runtime.InteropServices.HandleRef Vi, int PathOption);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureRXBypassPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXBypassPath_64(System.Runtime.InteropServices.HandleRef Vi, int PathOption);
            public static int ConfigureRXBypassPath(System.Runtime.InteropServices.HandleRef Vi, int PathOption)
            {
                if (is64BitProcess)
                    return ConfigureRXBypassPath_64(Vi, PathOption);
                else
                    return ConfigureRXBypassPath_32(Vi, PathOption);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureRXConversionGainPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXConversionGainPath_32(System.Runtime.InteropServices.HandleRef Vi, int GainPath);

            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureRXConversionGainPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXConversionGainPath_64(System.Runtime.InteropServices.HandleRef Vi, int GainPath);
            public static int ConfigureRXConversionGainPath(System.Runtime.InteropServices.HandleRef Vi, int GainPath)
            {
                if (is64BitProcess)
                    return ConfigureRXConversionGainPath_64(Vi, GainPath);
                else
                    return ConfigureRXConversionGainPath_32(Vi, GainPath);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureRXIFFilterPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXIFFilterPath_32(System.Runtime.InteropServices.HandleRef Vi, int FilterOption);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureRXIFFilterPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXIFFilterPath_64(System.Runtime.InteropServices.HandleRef Vi, int FilterOption);
            public static int ConfigureRXIFFilterPath(System.Runtime.InteropServices.HandleRef Vi, int FilterOption)
            {
                if (is64BitProcess)
                    return ConfigureRXIFFilterPath_64(Vi, FilterOption);
                else
                    return ConfigureRXIFFilterPath_32(Vi, FilterOption);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureRXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXINFrequency_32(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureRXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXINFrequency_64(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            public static int ConfigureRXINFrequency(System.Runtime.InteropServices.HandleRef Vi, double Frequency)
            {
                if (is64BitProcess)
                    return ConfigureRXINFrequency_64(Vi, Frequency);
                else
                    return ConfigureRXINFrequency_32(Vi, Frequency);
        }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureRXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXOUTFrequency_32(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureRXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXOUTFrequency_64(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            public static int ConfigureRXOUTFrequency(System.Runtime.InteropServices.HandleRef Vi, double Frequency)
            {
                if (is64BitProcess)
                    return ConfigureRXOUTFrequency_64(Vi, Frequency);
                else
                    return ConfigureRXOUTFrequency_32(Vi, Frequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureCalibrationTone", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCalibrationTone_32(System.Runtime.InteropServices.HandleRef Vi, double OutFrequency, out double InFrequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureCalibrationTone", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCalibrationTone_64(System.Runtime.InteropServices.HandleRef Vi, double OutFrequency, out double InFrequency);
            public static int ConfigureCalibrationTone(System.Runtime.InteropServices.HandleRef Vi, double OutFrequency, out double InFrequency)
            {
                if (is64BitProcess)
                    return ConfigureCalibrationTone_64(Vi, OutFrequency, out InFrequency);
                else
                    return ConfigureCalibrationTone_32(Vi, OutFrequency, out InFrequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureTXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXINFrequency_32(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureTXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXINFrequency_64(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            public static int ConfigureTXINFrequency(System.Runtime.InteropServices.HandleRef Vi, double Frequency)
            {
                if (is64BitProcess)
                    return ConfigureTXINFrequency_64(Vi, Frequency);
                else
                    return ConfigureTXINFrequency_32(Vi, Frequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureTXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOUTFrequency_32(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureTXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOUTFrequency_64(System.Runtime.InteropServices.HandleRef Vi, double Frequency);
            public static int ConfigureTXOUTFrequency(System.Runtime.InteropServices.HandleRef Vi, double Frequency)
            {
                if (is64BitProcess)
                    return ConfigureTXOUTFrequency_64(Vi, Frequency);
                else
                    return ConfigureTXOUTFrequency_32(Vi, Frequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ConfigureTXOutputPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOutputPath_32(System.Runtime.InteropServices.HandleRef Vi, int PathOption);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ConfigureTXOutputPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOutputPath_64(System.Runtime.InteropServices.HandleRef Vi, int PathOption);
            public static int ConfigureTXOutputPath(System.Runtime.InteropServices.HandleRef Vi, int PathOption)
            {
                if (is64BitProcess)
                    return ConfigureTXOutputPath_64(Vi, PathOption);
                else
                    return ConfigureTXOutputPath_32(Vi, PathOption);
            }


            [DllImport(nativeDLLName32, EntryPoint = "si2250_MeasureTemperature", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTemperature_32(System.Runtime.InteropServices.HandleRef Vi, out double Temperature);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_MeasureTemperature", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTemperature_64(System.Runtime.InteropServices.HandleRef Vi, out double Temperature);
            public static int MeasureTemperature(System.Runtime.InteropServices.HandleRef Vi, out double Temperature)
            {
                if (is64BitProcess)
                    return MeasureTemperature_64(Vi, out Temperature);
                else
                    return MeasureTemperature_32(Vi, out Temperature);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_MeasureTXOUTPower", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTXOUTPower_32(System.Runtime.InteropServices.HandleRef Vi, out double Power);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_MeasureTXOUTPower", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTXOUTPower_64(System.Runtime.InteropServices.HandleRef Vi, out double Power);
            public static int MeasureTXOUTPower(System.Runtime.InteropServices.HandleRef Vi, out double Power)
            {
                if (is64BitProcess)
                    return MeasureTXOUTPower_64(Vi, out Power);
                else
                    return MeasureTXOUTPower_32(Vi, out Power);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetRXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXLOFrequency_32(System.Runtime.InteropServices.HandleRef Vi, out double Frequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetRXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXLOFrequency_64(System.Runtime.InteropServices.HandleRef Vi, out double Frequency);
            public static int GetRXLOFrequency(System.Runtime.InteropServices.HandleRef Vi, out double Frequency)
            {
                if (is64BitProcess)
                    return GetRXLOFrequency_64(Vi, out Frequency);
                else
                    return GetRXLOFrequency_32(Vi, out Frequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetRXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXPathGain_32(System.Runtime.InteropServices.HandleRef Vi, out double Gain);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetRXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXPathGain_64(System.Runtime.InteropServices.HandleRef Vi, out double Gain);
            public static int GetRXPathGain(System.Runtime.InteropServices.HandleRef Vi, out double Gain)
            {
                if (is64BitProcess)
                    return GetRXPathGain_64(Vi, out Gain);
                else
                    return GetRXPathGain_32(Vi, out Gain);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetTXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXLOFrequency_32(System.Runtime.InteropServices.HandleRef Vi, out double Frequency);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetTXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXLOFrequency_64(System.Runtime.InteropServices.HandleRef Vi, out double Frequency);
            public static int GetTXLOFrequency(System.Runtime.InteropServices.HandleRef Vi, out double Frequency)
            {
                if (is64BitProcess)
                    return GetTXLOFrequency_64(Vi, out Frequency);
                else
                    return GetTXLOFrequency_32(Vi, out Frequency);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetTXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXPathGain_32(System.Runtime.InteropServices.HandleRef Vi, out double Gain);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetTXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXPathGain_64(System.Runtime.InteropServices.HandleRef Vi, out double Gain);
            public static int GetTXPathGain(System.Runtime.InteropServices.HandleRef Vi, out double Gain)
            {
                if (is64BitProcess)
                    return GetTXPathGain_64(Vi, out Gain);
                else
                    return GetTXPathGain_32(Vi, out Gain);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ClearInterchangeWarnings", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearInterchangeWarnings_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ClearInterchangeWarnings", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearInterchangeWarnings_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int ClearInterchangeWarnings(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return ClearInterchangeWarnings_64(Vi);
                else
                    return ClearInterchangeWarnings_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_Disable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Disable_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_Disable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Disable_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int Disable(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return Disable_64(Vi);
                else
                    return Disable_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query_32(System.Runtime.InteropServices.HandleRef Vi, out int ErrorCode, System.Text.StringBuilder ErrorMessage);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query_64(System.Runtime.InteropServices.HandleRef Vi, out int ErrorCode, System.Text.StringBuilder ErrorMessage);
            public static int error_query(System.Runtime.InteropServices.HandleRef Vi, out int ErrorCode, System.Text.StringBuilder ErrorMessage)
            {
                if (is64BitProcess)
                    return error_query_64(Vi, out ErrorCode, ErrorMessage);
                else
                    return error_query_32(Vi, out ErrorCode, ErrorMessage);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetNextCoercionRecord", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextCoercionRecord_32(System.Runtime.InteropServices.HandleRef Vi, int CoercionRecordBufferSize, System.Text.StringBuilder CoercionRecord);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetNextCoercionRecord", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextCoercionRecord_64(System.Runtime.InteropServices.HandleRef Vi, int CoercionRecordBufferSize, System.Text.StringBuilder CoercionRecord);
            public static int GetNextCoercionRecord(System.Runtime.InteropServices.HandleRef Vi, int CoercionRecordBufferSize, System.Text.StringBuilder CoercionRecord)
            {
                if (is64BitProcess)
                    return GetNextCoercionRecord_64(Vi, CoercionRecordBufferSize, CoercionRecord);
                else
                    return GetNextCoercionRecord_32(Vi, CoercionRecordBufferSize, CoercionRecord);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetNextInterchangeWarning", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextInterchangeWarning_32(System.Runtime.InteropServices.HandleRef Vi, int InterchangeWarningBufferSize, System.Text.StringBuilder InterchangeWarning);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetNextInterchangeWarning", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextInterchangeWarning_64(System.Runtime.InteropServices.HandleRef Vi, int InterchangeWarningBufferSize, System.Text.StringBuilder InterchangeWarning);
            public static int GetNextInterchangeWarning(System.Runtime.InteropServices.HandleRef Vi, int InterchangeWarningBufferSize, System.Text.StringBuilder InterchangeWarning)
            {
                if (is64BitProcess)
                    return GetNextInterchangeWarning_64(Vi, InterchangeWarningBufferSize, InterchangeWarning);
                else
                    return GetNextInterchangeWarning_32(Vi, InterchangeWarningBufferSize, InterchangeWarning);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_InvalidateAllAttributes", CallingConvention = CallingConvention.StdCall)]
            public static extern int InvalidateAllAttributes_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_InvalidateAllAttributes", CallingConvention = CallingConvention.StdCall)]
            public static extern int InvalidateAllAttributes_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int InvalidateAllAttributes(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return InvalidateAllAttributes_64(Vi);
                else
                    return InvalidateAllAttributes_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int reset(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return reset_64(Vi);
                else
                    return reset_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ResetInterchangeCheck", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetInterchangeCheck_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ResetInterchangeCheck", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetInterchangeCheck_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int ResetInterchangeCheck(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return ResetInterchangeCheck_64(Vi);
                else
                    return ResetInterchangeCheck_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_ResetWithDefaults", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetWithDefaults_32(System.Runtime.InteropServices.HandleRef Vi);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_ResetWithDefaults", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetWithDefaults_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int ResetWithDefaults(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return ResetWithDefaults_64(Vi);
                else
                    return ResetWithDefaults_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query_32(System.Runtime.InteropServices.HandleRef Vi, System.Text.StringBuilder DriverRev, System.Text.StringBuilder InstrRev);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query_64(System.Runtime.InteropServices.HandleRef Vi, System.Text.StringBuilder DriverRev, System.Text.StringBuilder InstrRev);
            public static int revision_query(System.Runtime.InteropServices.HandleRef Vi, System.Text.StringBuilder DriverRev, System.Text.StringBuilder InstrRev)
            {
                if (is64BitProcess)
                    return revision_query_64(Vi, DriverRev, InstrRev);
                else
                    return revision_query_32(Vi, DriverRev, InstrRev);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test_32(System.Runtime.InteropServices.HandleRef Vi, out short TestResult, System.Text.StringBuilder TestMessage);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test_64(System.Runtime.InteropServices.HandleRef Vi, out short TestResult, System.Text.StringBuilder TestMessage);
            public static int self_test(System.Runtime.InteropServices.HandleRef Vi, out short TestResult, System.Text.StringBuilder TestMessage)
            {
                if (is64BitProcess)
                    return self_test_64(Vi, out TestResult, TestMessage);
                else
                    return self_test_32(Vi, out TestResult, TestMessage);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close_32(System.Runtime.InteropServices.HandleRef Vi);

            [DllImport(nativeDLLName64, EntryPoint = "si2250_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close_64(System.Runtime.InteropServices.HandleRef Vi);
            public static int close(System.Runtime.InteropServices.HandleRef Vi)
            {
                if (is64BitProcess)
                    return close_64(Vi);
                else
                    return close_32(Vi);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message_32(System.Runtime.InteropServices.HandleRef Vi, int ErrorCode, System.Text.StringBuilder ErrorMessage);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message_64(System.Runtime.InteropServices.HandleRef Vi, int ErrorCode, System.Text.StringBuilder ErrorMessage);
            public static int error_message(System.Runtime.InteropServices.HandleRef Vi, int ErrorCode, System.Text.StringBuilder ErrorMessage)
            {
                if (is64BitProcess)
                    return error_message_64(Vi, ErrorCode, ErrorMessage);
                else
                    return error_message_32(Vi, ErrorCode, ErrorMessage);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out ushort AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out ushort AttributeValue);
            public static int GetAttributeViBoolean(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out ushort AttributeValue)
            {
                if (is64BitProcess)
                    return GetAttributeViBoolean_64(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
                else
                    return GetAttributeViBoolean_32(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out int AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out int AttributeValue);
            public static int GetAttributeViInt32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out int AttributeValue)
            {
                if (is64BitProcess)
                    return GetAttributeViInt32_64(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
                else
                    return GetAttributeViInt32_32(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out double AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out double AttributeValue);
            public static int GetAttributeViReal64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out double AttributeValue)
            {
                if (is64BitProcess)
                    return GetAttributeViReal64_64(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
                else
                    return GetAttributeViReal64_32(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out System.Runtime.InteropServices.HandleRef AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out System.Runtime.InteropServices.HandleRef AttributeValue);
            public static int GetAttributeViSession(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, out System.Runtime.InteropServices.HandleRef AttributeValue)
            {
                if (is64BitProcess)
                    return GetAttributeViSession_64(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
                else
                    return GetAttributeViSession_32(Vi, RepCapIdentifier, AttributeID, out AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, int AttributeValueBufferSize, System.Text.StringBuilder AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, int AttributeValueBufferSize, System.Text.StringBuilder AttributeValue);
            public static int GetAttributeViString(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, int AttributeValueBufferSize, System.Text.StringBuilder AttributeValue)
            {
                if (is64BitProcess)
                    return GetAttributeViString_64(Vi, RepCapIdentifier, AttributeID, AttributeValueBufferSize, AttributeValue);
                else
                    return GetAttributeViString_32(Vi, RepCapIdentifier, AttributeID, AttributeValueBufferSize, AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, ushort AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, ushort AttributeValue);
            public static int SetAttributeViBoolean(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, ushort AttributeValue)
            {
                if (is64BitProcess)
                    return SetAttributeViBoolean_64(Vi, RepCapIdentifier, AttributeID, AttributeValue);
                else
                    return SetAttributeViBoolean_32(Vi, RepCapIdentifier, AttributeID, AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, int AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, int AttributeValue);
            public static int SetAttributeViInt32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, int AttributeValue)
            {
                if (is64BitProcess)
                    return SetAttributeViInt32_64(Vi, RepCapIdentifier, AttributeID, AttributeValue);
                else
                    return SetAttributeViInt32_32(Vi, RepCapIdentifier, AttributeID, AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, double AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, double AttributeValue);
            public static int SetAttributeViReal64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, double AttributeValue)
            {
                if (is64BitProcess)
                    return SetAttributeViReal64_64(Vi, RepCapIdentifier, AttributeID, AttributeValue);
                else
                    return SetAttributeViReal64_32(Vi, RepCapIdentifier, AttributeID, AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, System.Runtime.InteropServices.HandleRef AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, System.Runtime.InteropServices.HandleRef AttributeValue);
            public static int SetAttributeViSession(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, System.Runtime.InteropServices.HandleRef AttributeValue)
            {
                if (is64BitProcess)
                    return SetAttributeViSession_64(Vi, RepCapIdentifier, AttributeID, AttributeValue);
                else
                    return SetAttributeViSession_32(Vi, RepCapIdentifier, AttributeID, AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString_32(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, string AttributeValue);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString_64(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, string AttributeValue);
            public static int SetAttributeViString(System.Runtime.InteropServices.HandleRef Vi, string RepCapIdentifier, int AttributeID, string AttributeValue)
            {
                if (is64BitProcess)
                    return SetAttributeViString_64(Vi, RepCapIdentifier, AttributeID, AttributeValue);
                else
                    return SetAttributeViString_32(Vi, RepCapIdentifier, AttributeID, AttributeValue);
            }

            [DllImport(nativeDLLName32, EntryPoint = "si2250_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError_32(System.Runtime.InteropServices.HandleRef Vi, out int ErrorCode, int ErrorDescriptionBufferSize, System.Text.StringBuilder ErrorDescription);
            [DllImport(nativeDLLName64, EntryPoint = "si2250_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError_64(System.Runtime.InteropServices.HandleRef Vi, out int ErrorCode, int ErrorDescriptionBufferSize, System.Text.StringBuilder ErrorDescription);
            public static int GetError(System.Runtime.InteropServices.HandleRef Vi, out int ErrorCode, int ErrorDescriptionBufferSize, System.Text.StringBuilder ErrorDescription)
            {
                if (is64BitProcess)
                    return GetError_64(Vi, out ErrorCode, ErrorDescriptionBufferSize, ErrorDescription);
                else
                    return GetError_32(Vi, out ErrorCode, ErrorDescriptionBufferSize, ErrorDescription);
            }

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

    public class si2250Constants
    {

        public const int LoSourceOnboard = 1600;

        public const int LoSourceLoIn = 1601;

        public const int RxBypassPathFilterBank = 1500;

        public const int RxBypassPathLna = 1501;

        public const int RxConversionGainPathNone = 1300;

        public const int RxConversionGainPath5dBAtten = 1301;

        public const int RxConversionGainPath10dBAtten = 1302;

        public const int RxConversionGainPathLna = 1303;

        public const int RxIfFilterPathEnabled = 1400;

        public const int RxIfFilterPathDisabled = 1401;

        public const int TxOutputPathTxOut0Direct = 1100;

        public const int TxOutputPathTxOut0Filter = 1101;

        public const int TxOutputPathTxOut1 = 1102;

        public const int TxOutputPathTxOut2Internal = 1103;

        public const int RxInputPathRxIn = 1200;

        public const int RxInputPathTxOut2Internal = 1201;
    }

    public enum si2250Properties
    {

        /// <summary>
        /// System.String
        /// </summary>
        DriverSetup = 1050007,

        /// <summary>
        /// System.String
        /// </summary>
        IoResourceDescriptor = 1050304,

        /// <summary>
        /// System.String
        /// </summary>
        LogicalName = 1050305,

        /// <summary>
        /// System.String
        /// </summary>
        GroupCapabilities = 1050401,

        /// <summary>
        /// System.String
        /// </summary>
        SupportedInstrumentModels = 1050327,

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
        SpecificDriverDescription = 1050514,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverPrefix = 1050302,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverRevision = 1050551,

        /// <summary>
        /// System.String
        /// </summary>
        SpecificDriverVendor = 1050513,

        /// <summary>
        /// System.String
        /// </summary>
        InstrumentFirmwareRevision = 1050510,

        /// <summary>
        /// System.String
        /// </summary>
        InstrumentManufacturer = 1050511,

        /// <summary>
        /// System.String
        /// </summary>
        InstrumentModel = 1050512,

        /// <summary>
        /// System.Boolean
        /// </summary>
        Cache = 1050004,

        /// <summary>
        /// System.Boolean
        /// </summary>
        InterchangeCheck = 1050021,

        /// <summary>
        /// System.Boolean
        /// </summary>
        QueryInstrumentStatus = 1050003,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RangeCheck = 1050002,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RecordCoercions = 1050006,

        /// <summary>
        /// System.Boolean
        /// </summary>
        Simulate = 1050005,

        /// <summary>
        /// System.String
        /// </summary>
        ModuleRevision = 1150023,

        /// <summary>
        /// System.String
        /// </summary>
        ModuleSerialNumber = 1150022,

        /// <summary>
        /// System.Double
        /// </summary>
        ModuleTemperature = 1150024,

        /// <summary>
        /// System.String
        /// </summary>
        ExternalCalibrationDatetime = 1150017,

        /// <summary>
        /// System.Double
        /// </summary>
        ExternalCalibrationTemperature = 1150018,

        /// <summary>
        /// System.String
        /// </summary>
        FactoryCalibrationDate = 1150019,

        /// <summary>
        /// System.Int32
        /// </summary>
        FactoryCalibrationInterval = 1150021,

        /// <summary>
        /// System.Double
        /// </summary>
        FactoryCalibrationTemperature = 1150020,

        /// <summary>
        /// System.Double
        /// </summary>
        RxLoFrequency = 1150015,

        /// <summary>
        /// System.Int32
        /// </summary>
        RxLoSource = 1150016,

        /// <summary>
        /// System.Double
        /// </summary>
        TxLoFrequency = 1150013,

        /// <summary>
        /// System.Int32
        /// </summary>
        TxLoSource = 1150014,

        /// <summary>
        /// System.Int32
        /// </summary>
        RxBypassPath = 1150011,

        /// <summary>
        /// System.Int32
        /// </summary>
        RxConversionGainPath = 1150010,

        /// <summary>
        /// System.Int32
        /// </summary>
        RxIfFilterPath = 1150009,

        /// <summary>
        /// System.Double
        /// </summary>
        RxInFrequency = 1150006,

        /// <summary>
        /// System.Int32
        /// </summary>
        RxInputPath = 1150008,

        /// <summary>
        /// System.Double
        /// </summary>
        RxOutFrequency = 1150007,

        /// <summary>
        /// System.Double
        /// </summary>
        RxPathGain = 1150012,

        /// <summary>
        /// System.Double
        /// </summary>
        TxInFrequency = 1150001,

        /// <summary>
        /// System.Double
        /// </summary>
        TxOutFrequency = 1150002,

        /// <summary>
        /// System.Int32
        /// </summary>
        TxOutputPath = 1150003,

        /// <summary>
        /// System.Double
        /// </summary>
        TxOutputPower = 1150004,

        /// <summary>
        /// System.Double
        /// </summary>
        TxPathGain = 1150005,
    }
}