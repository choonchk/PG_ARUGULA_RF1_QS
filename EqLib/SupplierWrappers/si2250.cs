using System;
using System.Runtime.InteropServices;

namespace SignalCraftTechnologies.ModularInstruments.Interop
{
    public sealed class si2250 : object, System.IDisposable
    {
        private System.Runtime.InteropServices.HandleRef _handle;
        private bool _disposed = true;
        
        #region Constructor

        /// <summary>
        /// This function creates an IVI instrument driver session, typically using the C session instrument handle.
        /// </summary>
        /// <param name="instrumentHandle">
        /// The instrument handle that is used to create an IVI instrument driver session.
        /// </param>
        public si2250(System.IntPtr instrumentHandle)
        {
            _handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            _disposed = false;
        }

        /// <summary>
        /// Opens the I/O session to the instrument. Driver methods and properties that access the instrument are only accessible after Initialize is called. Initialize optionally performs a Reset and queries the instrument to validate the instrument model.
        /// </summary>
        /// <param name="resourceName">
        /// An IVI logical name or an instrument specific string that identifies the address of the instrument, such as a VISA resource descriptor string.
        /// </param>
        /// <param name="idQuery">
        /// Specifies whether to verify the ID of the instrument.
        /// </param>
        /// <param name="reset">
        /// Specifies whether to reset the instrument.
        /// </param>
        public si2250(string resourceName, bool idQuery, bool reset)
        {
            System.IntPtr instrumentHandle;
            int status = NativeMethods.init(resourceName, System.Convert.ToUInt16(idQuery), System.Convert.ToUInt16(reset), out instrumentHandle);
            _handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            NativeMethods.TestForError(_handle, status);
            _disposed = false;
        }

        /// <summary>
        /// Opens the I/O session to the instrument.  Driver methods and properties that access the instrument are only accessible after Initialize is called.  Initialize optionally performs a Reset and queries the instrument to validate the instrument model.
        /// </summary>
        /// <param name="resourceName">
        /// An IVI logical name or an instrument specific string that identifies the address of the instrument, such as a VISA resource descriptor string.
        /// </param>
        /// <param name="idQuery">
        /// Specifies whether to verify the ID of the instrument.
        /// </param>
        /// <param name="reset">
        /// Specifies whether to reset the instrument.
        /// </param>
        /// <param name="optionsString">
        /// The user can use the OptionsString parameter to specify the initial values of certain IVI inherent attributes for the session. The format of an assignment in the OptionsString parameter is "Name=Value", where Name is one of: RangeCheck, QueryInstrStatus, Cache, Simulate, RecordCoercions, InterchangeCheck,or DriverSetup. Value is either true or false except for DriverSetup. If the OptionString parameter contains an assignment for the Driver Setup attribute, the Initialize function assumes that everything following "DriverSetup=" is part of the assignment.
        /// </param>
        public si2250(string resourceName, bool idQuery, bool reset, string optionsString)
        {
            System.IntPtr instrumentHandle;
            int status = NativeMethods.InitWithOptions(resourceName, System.Convert.ToUInt16(idQuery), System.Convert.ToUInt16(reset), optionsString, out instrumentHandle);
            _handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            NativeMethods.TestForError(_handle, status);
            _disposed = false;
        }

        ~si2250() 
        { 
            Dispose(false); 
        }
        #endregion

        #region DisposeMethods
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if ((_disposed == false))
            {
                NativeMethods.close(_handle);
                _handle = new System.Runtime.InteropServices.HandleRef(null, System.IntPtr.Zero);
            }
            _disposed = true;
        }
        #endregion

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
                return _handle.Handle;
            }
        }

        /// <summary>
        /// Clear the external calibration.
        /// </summary>
        public int ClearExternalCalibration()
        {
            int status = NativeMethods.ClearExternalCalibration(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Set the external calibration.
        /// </summary>
        public int SetExternalCalibration()
        {
            int status = NativeMethods.SetExternalCalibration(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the LO signal source used to convert the RX input signal.
        /// </summary>
        /// <param name="Source">
        /// Specifies the LO source to select.
        /// </param>
        public int ConfigureRXLOSource(int source)
        {
            int status = NativeMethods.ConfigureRXLOSource(_handle, source);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the LO signal source used to convert the TX input signal.
        /// </summary>
        /// <param name="Source">
        /// Specifies the LO source to select.
        /// </param>
        public int ConfigureTXLOSource(int source)
        {
            int status = NativeMethods.ConfigureTXLOSource(_handle, source);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the RX signal path for harmonic conversion.
        /// </summary>
        /// <param name="fundamental">
        /// Specifies the fundamental frequency, in Hertz (Hz).
        /// </param>
        /// <param name="harmonicIndex">
        /// Specifies the harmonic of interest.
        /// </param>
        /// <param name="outFrequency">
        /// The output frequency of the converted harmonic, in Hertz (Hz).
        /// </param>
        public int ConfigureHarmonicConverter(double fundamental, int harmonicIndex, out double outFrequency)
        {
            int status = NativeMethods.ConfigureHarmonicConverter(_handle, fundamental, harmonicIndex, out outFrequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the path option of the RX bypass signal path.
        /// </summary>
        /// <param name="pathOption">
        /// Specifies the selected Bypass path.
        /// </param>
        public int ConfigureRXBypassPath(int pathOption)
        {
            int status = NativeMethods.ConfigureRXBypassPath(_handle, pathOption);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the RF gain path option of the RX conversion signal path.
        /// </summary>
        /// <param name="gainPath">
        /// Specifies the conversion gain path.
        /// </param>
        public int ConfigureRXConversionGainPath(int gainPath)
        {
            int status = NativeMethods.ConfigureRXConversionGainPath(_handle, gainPath);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the IF filter path option of the RX conversion signal path.
        /// </summary>
        /// <param name="filterOption">
        /// Specifies the IF filter option.
        /// </param>
        public int ConfigureRXIFFilterPath(int filterOption)
        {
            int status = NativeMethods.ConfigureRXIFFilterPath(_handle, filterOption);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the RX input signal frequency.
        /// </summary>
        /// <param name="frequency">
        /// Specifies the input frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureRXINFrequency(double frequency)
        {
            int status = NativeMethods.ConfigureRXINFrequency(_handle, frequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the RX output signal frequency.
        /// </summary>
        /// <param name="frequency">
        /// Specifies the output frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureRXOUTFrequency(double frequency)
        {
            int status = NativeMethods.ConfigureRXOUTFrequency(_handle, frequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the TX output OUT1 and OUT2 signal path for calibration tone generation.
        /// </summary>
        /// <param name="outFrequency">
        /// Specifies the output frequency, in Hertz (Hz).
        /// </param>
        /// <param name="inFrequency">
        /// The input frequency required, in Hertz (Hz).
        /// </param>
        public int ConfigureCalibrationTone(double outFrequency, out double inFrequency)
        {
            int status = NativeMethods.ConfigureCalibrationTone(_handle, outFrequency, out inFrequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the TX input signal frequency.
        /// </summary>
        /// <param name="frequency">
        /// Specifies the input frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureTXINFrequency(double frequency)
        {
            int status = NativeMethods.ConfigureTXINFrequency(_handle, frequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the TX output OUT1 and OUT2 signal frequency.
        /// </summary>
        /// <param name="frequency">
        /// Specifies the output frequency, in Hertz (Hz).
        /// </param>
        public int ConfigureTXOUTFrequency(double frequency)
        {
            int status = NativeMethods.ConfigureTXOUTFrequency(_handle, frequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Configure the TX output signal path and destination.
        /// </summary>
        /// <param name="pathOption">
        /// Specifies the selected output and path of the TX path.
        /// </param>
        public int ConfigureTXOutputPath(int pathOption)
        {
            int status = NativeMethods.ConfigureTXOutputPath(_handle, pathOption);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Measure the temperature of the hardware module, in degrees Celsius.
        /// </summary>
        /// <param name="temperature">
        /// The temperature, in degrees Celsius (?C).
        /// </param>
        public int MeasureTemperature(out double temperature)
        {
            int status = NativeMethods.MeasureTemperature(_handle, out temperature);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Measure the signal power at the TX output OUT1 and OUT2, in dBm.
        /// </summary>
        /// <param name="power">
        /// The power level, in dBm.
        /// </param>
        public int MeasureTXOUTPower(out double power)
        {
            int status = NativeMethods.MeasureTXOUTPower(_handle, out power);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Get the LO signal frequency currently configured (onboard) or required (LO IN) for the RX conversion signal path.
        /// </summary>
        /// <param name="frequency">
        /// LO frequency, in Hertz (Hz).
        /// </param>
        public int GetRXLOFrequency(out double frequency)
        {
            int status = NativeMethods.GetRXLOFrequency(_handle, out frequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Get the factory calibrated gain from RX IN to RX OUT at the currently configured frequency settings.
        /// </summary>
        /// <param name="gain">
        /// Gain, in dB.
        /// </param>
        public int GetRXPathGain(out double gain)
        {
            int status = NativeMethods.GetRXPathGain(_handle, out gain);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Get the LO signal frequency currently configured (onboard) or required (LO IN) for the TX conversion signal path.
        /// </summary>
        /// <param name="frequency">
        /// LO frequency, in Hertz (Hz).
        /// </param>
        public int GetTXLOFrequency(out double frequency)
        {
            int status = NativeMethods.GetTXLOFrequency(_handle, out frequency);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Get the factory calibrated gain from TX IN to the TX output selected at the currently configured frequency settings.
        /// </summary>
        /// <param name="gain">
        /// Gain, in dB.
        /// </param>
        public int GetTXPathGain(out double gain)
        {
            int status = NativeMethods.GetTXPathGain(_handle, out gain);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Clears the list of interchangeability warnings that the IVI specific driver maintains.
        /// </summary>
        public int ClearInterchangeWarnings()
        {
            int status = NativeMethods.ClearInterchangeWarnings(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Quickly places the instrument in a state where it has no, or minimal, effect on the external system to which it is connected.  This state is not necessarily a known state.
        /// </summary>
        public int Disable()
        {
            int status = NativeMethods.Disable(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Queries the instrument and returns instrument specific error information.  This function can be used when QueryInstrumentStatus is True to retrieve error details when the driver detects an instrument error.
        /// </summary>
        /// <param name="errorCode">
        /// Instrument error code
        /// </param>
        /// <param name="errorMessage">
        /// Instrument error message
        /// </param>
        public int error_query(out int errorCode, System.Text.StringBuilder errorMessage)
        {
            int status = NativeMethods.error_query(_handle, out errorCode, errorMessage);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Returns the oldest record from the coercion record list.  Records are only added to the list if RecordCoercions is True.
        /// </summary>
        /// <param name="coercionRecordBufferSize">
        /// The number of bytes in the ViChar array that the user specifies for the CoercionRecord parameter.
        /// </param>
        /// <param name="coercionRecord">
        /// The coercion record string shall contain the following information: (1) The name of the attribute that was coerced.  This can be the generic name, the COM property name, or the C defined constant. (2) If the attribute is channel-based, the name of the channel.   The channel name can be the specific driver channel string or the virtual channel name that the user specified.(3) If the attribute applies to a repeated capability, the name of the repeated capability. The name can be the specific driver repeated capability token or the virtual repeated capability name that the user specified.(4) The value that the user specified for the attribute.(5) The value to which the attribute was coerced.
        /// </param>
        public int GetNextCoercionRecord(int coercionRecordBufferSize, System.Text.StringBuilder coercionRecord)
        {
            int status = NativeMethods.GetNextCoercionRecord(_handle, coercionRecordBufferSize, coercionRecord);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Returns the oldest warning from the interchange warning list.  Records are only added to the list if InterchangeCheck is True.
        /// </summary>
        /// <param name="interchangeWarningBufferSize">
        /// The number of bytes in the ViChar array that the user specifies for the InterchangeWarning parameter.
        /// </param>
        /// <param name="interchangeWarning">
        /// A string describing the oldest interchangeability warning or empty string if no warrnings remain.
        /// </param>
        public int GetNextInterchangeWarning(int interchangeWarningBufferSize, System.Text.StringBuilder interchangeWarning)
        {
            int status = NativeMethods.GetNextInterchangeWarning(_handle, interchangeWarningBufferSize, interchangeWarning);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Invalidates all of the driver's cached values.
        /// </summary>
        public int InvalidateAllAttributes()
        {
            int status = NativeMethods.InvalidateAllAttributes(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Places the instrument in a known state and configures instrument options on which the IVI specific driver depends (for example, enabling/disabling headers).  For an IEEE 488.2 instrument, Reset sends the command string *RST to the instrument.
        /// </summary>
        public int reset()
        {
            int status = NativeMethods.reset(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Resets the interchangeability checking algorithms of the driver so that methods and properties that executed prior to calling this function have no affect on whether future calls to the driver generate interchangeability warnings.
        /// </summary>
        public int ResetInterchangeCheck()
        {
            int status = NativeMethods.ResetInterchangeCheck(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Does the equivalent of Reset and then, (1) disables class extension capability groups, (2) sets attributes to initial values defined by class specs, and (3) configures the driver to option string settings used when Initialize was last executed.
        /// </summary>
        public int ResetWithDefaults()
        {
            int status = NativeMethods.ResetWithDefaults(_handle);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Retrieves revision information from the instrument.
        /// </summary>
        /// <param name="driverRev">
        /// Returns the revision of the IVI specific driver, which is the value held in the Specific Driver Revision attribute. Refer to the Specific Driver Revision attribute for more information.
        /// </param>
        /// <param name="instrRev">
        /// Returns the firmware revision of the instrument, which is the value held in the Instrument Firmware Revision attribute. Refer to the Instrument Firmware Revision attribute for more information.
        /// </param>
        public int revision_query(System.Text.StringBuilder driverRev, System.Text.StringBuilder instrRev)
        {
            int status = NativeMethods.revision_query(_handle, driverRev, instrRev);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        /// <summary>
        /// Performs an instrument self test, waits for the instrument to complete the test, and queries the instrument for the results.  If the instrument passes the test, TestResult is zero and TestMessage is 'Self test passed'.
        /// </summary>
        /// <param name="testResult">
        /// The numeric result from the self test operation. 0 = no error (test passed)
        /// </param>
        /// <param name="testMessage">
        /// The self test status message
        /// </param>
        public int self_test(out short testResult, System.Text.StringBuilder testMessage)
        {
            int status = NativeMethods.self_test(_handle, out testResult, testMessage);
            NativeMethods.TestForError(_handle, status);
            return status;
        }

        public bool GetBoolean(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            ushort val;
            NativeMethods.TestForError(_handle, NativeMethods.GetAttributeViBoolean(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return System.Convert.ToBoolean(val);
        }

        public bool GetBoolean(si2250Properties propertyId)
        {
            return GetBoolean(propertyId, "");
        }

        public int GetInt32(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            int val;
            NativeMethods.TestForError(_handle, NativeMethods.GetAttributeViInt32(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }

        public int GetInt32(si2250Properties propertyId)
        {
            return GetInt32(propertyId, "");
        }

        public double GetDouble(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            double val;
            NativeMethods.TestForError(_handle, NativeMethods.GetAttributeViReal64(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), out val));
            return val;
        }

        public double GetDouble(si2250Properties propertyId)
        {
            return GetDouble(propertyId, "");
        }

        public string GetString(si2250Properties propertyId, string repeatedCapabilityOrChannel)
        {
            System.Text.StringBuilder newVal = new System.Text.StringBuilder(512);
            int size = NativeMethods.GetAttributeViString(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), 512, newVal);
            if ((size < 0))
            {
                NativeMethods.ThrowError(_handle, size);
            }
            else
            {
                if ((size > 0))
                {
                    newVal.Capacity = size;
                    NativeMethods.TestForError(_handle, NativeMethods.GetAttributeViString(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), size, newVal));
                }
            }
            return newVal.ToString();
        }

        public string GetString(si2250Properties propertyId)
        {
            return GetString(propertyId, "");
        }

        public void SetBoolean(si2250Properties propertyId, string repeatedCapabilityOrChannel, bool val)
        {
            NativeMethods.TestForError(_handle, NativeMethods.SetAttributeViBoolean(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), System.Convert.ToUInt16(val)));
        }

        public void SetBoolean(si2250Properties propertyId, bool val)
        {
            SetBoolean(propertyId, "", val);
        }

        public void SetInt32(si2250Properties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            NativeMethods.TestForError(_handle, NativeMethods.SetAttributeViInt32(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetInt32(si2250Properties propertyId, int val)
        {
            SetInt32(propertyId, "", val);
        }

        public void SetDouble(si2250Properties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            NativeMethods.TestForError(_handle, NativeMethods.SetAttributeViReal64(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetDouble(si2250Properties propertyId, double val)
        {
            SetDouble(propertyId, "", val);
        }

        public void SetString(si2250Properties propertyId, string repeatedCapabilityOrChannel, string val)
        {
            NativeMethods.TestForError(_handle, NativeMethods.SetAttributeViString(_handle, repeatedCapabilityOrChannel, ((int)(propertyId)), val));
        }

        public void SetString(si2250Properties propertyId, string val)
        {
            SetString(propertyId, "", val);
        }

        private class NativeMethods
        {
            private const string NativeDLLName32 = "si2250.dll";
            private const string NativeDLLName64 = "si2250_64.dll";
            private static readonly bool _is64BitProcess = (IntPtr.Size == 8);

            [DllImport(NativeDLLName32, EntryPoint = "si2250_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init_32(string resourceName, ushort idQuery, ushort reset, out System.IntPtr handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init_64(string resourceName, ushort idQuery, ushort reset, out System.IntPtr handle);
            public static int init(string resourceName, ushort idQuery, ushort reset, out System.IntPtr handle)
            {
                if (_is64BitProcess)
                    return init_64(resourceName, idQuery, reset, out handle);
                else
                    return init_32(resourceName, idQuery, reset, out handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions_32(string resourceName, ushort idQuery, ushort reset, string optionsString, out System.IntPtr handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions_64(string resourceName, ushort idQuery, ushort reset, string optionsString, out System.IntPtr handle);
            public static int InitWithOptions(string resourceName, ushort idQuery, ushort reset, string optionsString, out System.IntPtr handle)
            {
                if (_is64BitProcess)
                    return InitWithOptions_64(resourceName, idQuery, reset, optionsString, out handle);
                else
                    return InitWithOptions_32(resourceName, idQuery, reset, optionsString, out handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ClearExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearExternalCalibration_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ClearExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearExternalCalibration_64(System.Runtime.InteropServices.HandleRef handle);
            public static int ClearExternalCalibration(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return ClearExternalCalibration_64(handle);
                else
                    return ClearExternalCalibration_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_SetExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetExternalCalibration_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_SetExternalCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetExternalCalibration_64(System.Runtime.InteropServices.HandleRef handle);
            public static int SetExternalCalibration(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return SetExternalCalibration_64(handle);
                else
                    return SetExternalCalibration_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureRXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXLOSource_32(System.Runtime.InteropServices.HandleRef handle, int source);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureRXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXLOSource_64(System.Runtime.InteropServices.HandleRef handle, int source);
            public static int ConfigureRXLOSource(System.Runtime.InteropServices.HandleRef handle, int source)
            {
                if (_is64BitProcess)
                    return ConfigureRXLOSource_64(handle, source);
                else
                    return ConfigureRXLOSource_32(handle, source);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureTXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXLOSource_32(System.Runtime.InteropServices.HandleRef handle, int source);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureTXLOSource", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXLOSource_64(System.Runtime.InteropServices.HandleRef handle, int source);
            public static int ConfigureTXLOSource(System.Runtime.InteropServices.HandleRef handle, int source)
            {
                if (_is64BitProcess)
                    return ConfigureTXLOSource_64(handle, source);
                else
                    return ConfigureTXLOSource_32(handle, source);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureHarmonicConverter", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureHarmonicConverter_32(System.Runtime.InteropServices.HandleRef handle, double fundamental, int harmonicIndex, out double outFrequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureHarmonicConverter", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureHarmonicConverter_64(System.Runtime.InteropServices.HandleRef handle, double fundamental, int harmonicIndex, out double outFrequency);
            public static int ConfigureHarmonicConverter(System.Runtime.InteropServices.HandleRef handle, double fundamental, int harmonicIndex, out double outFrequency)
            {
                if (_is64BitProcess)
                    return ConfigureHarmonicConverter_64(handle, fundamental, harmonicIndex, out outFrequency);
                else
                    return ConfigureHarmonicConverter_32(handle, fundamental, harmonicIndex, out outFrequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureRXBypassPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXBypassPath_32(System.Runtime.InteropServices.HandleRef handle, int pathOption);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureRXBypassPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXBypassPath_64(System.Runtime.InteropServices.HandleRef handle, int pathOption);
            public static int ConfigureRXBypassPath(System.Runtime.InteropServices.HandleRef handle, int pathOption)
            {
                if (_is64BitProcess)
                    return ConfigureRXBypassPath_64(handle, pathOption);
                else
                    return ConfigureRXBypassPath_32(handle, pathOption);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureRXConversionGainPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXConversionGainPath_32(System.Runtime.InteropServices.HandleRef handle, int gainPath);

            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureRXConversionGainPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXConversionGainPath_64(System.Runtime.InteropServices.HandleRef handle, int gainPath);
            public static int ConfigureRXConversionGainPath(System.Runtime.InteropServices.HandleRef handle, int gainPath)
            {
                if (_is64BitProcess)
                    return ConfigureRXConversionGainPath_64(handle, gainPath);
                else
                    return ConfigureRXConversionGainPath_32(handle, gainPath);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureRXIFFilterPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXIFFilterPath_32(System.Runtime.InteropServices.HandleRef handle, int filterOption);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureRXIFFilterPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXIFFilterPath_64(System.Runtime.InteropServices.HandleRef handle, int filterOption);
            public static int ConfigureRXIFFilterPath(System.Runtime.InteropServices.HandleRef handle, int filterOption)
            {
                if (_is64BitProcess)
                    return ConfigureRXIFFilterPath_64(handle, filterOption);
                else
                    return ConfigureRXIFFilterPath_32(handle, filterOption);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureRXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXINFrequency_32(System.Runtime.InteropServices.HandleRef handle, double frequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureRXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXINFrequency_64(System.Runtime.InteropServices.HandleRef handle, double frequency);
            public static int ConfigureRXINFrequency(System.Runtime.InteropServices.HandleRef handle, double frequency)
            {
                if (_is64BitProcess)
                    return ConfigureRXINFrequency_64(handle, frequency);
                else
                    return ConfigureRXINFrequency_32(handle, frequency);
        }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureRXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXOUTFrequency_32(System.Runtime.InteropServices.HandleRef handle, double frequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureRXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRXOUTFrequency_64(System.Runtime.InteropServices.HandleRef handle, double frequency);
            public static int ConfigureRXOUTFrequency(System.Runtime.InteropServices.HandleRef handle, double frequency)
            {
                if (_is64BitProcess)
                    return ConfigureRXOUTFrequency_64(handle, frequency);
                else
                    return ConfigureRXOUTFrequency_32(handle, frequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureCalibrationTone", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCalibrationTone_32(System.Runtime.InteropServices.HandleRef handle, double outFrequency, out double inFrequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureCalibrationTone", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCalibrationTone_64(System.Runtime.InteropServices.HandleRef handle, double outFrequency, out double inFrequency);
            public static int ConfigureCalibrationTone(System.Runtime.InteropServices.HandleRef handle, double outFrequency, out double inFrequency)
            {
                if (_is64BitProcess)
                    return ConfigureCalibrationTone_64(handle, outFrequency, out inFrequency);
                else
                    return ConfigureCalibrationTone_32(handle, outFrequency, out inFrequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureTXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXINFrequency_32(System.Runtime.InteropServices.HandleRef handle, double frequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureTXINFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXINFrequency_64(System.Runtime.InteropServices.HandleRef handle, double frequency);
            public static int ConfigureTXINFrequency(System.Runtime.InteropServices.HandleRef handle, double frequency)
            {
                if (_is64BitProcess)
                    return ConfigureTXINFrequency_64(handle, frequency);
                else
                    return ConfigureTXINFrequency_32(handle, frequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureTXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOUTFrequency_32(System.Runtime.InteropServices.HandleRef handle, double frequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureTXOUTFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOUTFrequency_64(System.Runtime.InteropServices.HandleRef handle, double frequency);
            public static int ConfigureTXOUTFrequency(System.Runtime.InteropServices.HandleRef handle, double frequency)
            {
                if (_is64BitProcess)
                    return ConfigureTXOUTFrequency_64(handle, frequency);
                else
                    return ConfigureTXOUTFrequency_32(handle, frequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ConfigureTXOutputPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOutputPath_32(System.Runtime.InteropServices.HandleRef handle, int pathOption);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ConfigureTXOutputPath", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureTXOutputPath_64(System.Runtime.InteropServices.HandleRef handle, int pathOption);
            public static int ConfigureTXOutputPath(System.Runtime.InteropServices.HandleRef handle, int pathOption)
            {
                if (_is64BitProcess)
                    return ConfigureTXOutputPath_64(handle, pathOption);
                else
                    return ConfigureTXOutputPath_32(handle, pathOption);
            }


            [DllImport(NativeDLLName32, EntryPoint = "si2250_MeasureTemperature", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTemperature_32(System.Runtime.InteropServices.HandleRef handle, out double temperature);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_MeasureTemperature", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTemperature_64(System.Runtime.InteropServices.HandleRef handle, out double temperature);
            public static int MeasureTemperature(System.Runtime.InteropServices.HandleRef handle, out double temperature)
            {
                if (_is64BitProcess)
                    return MeasureTemperature_64(handle, out temperature);
                else
                    return MeasureTemperature_32(handle, out temperature);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_MeasureTXOUTPower", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTXOUTPower_32(System.Runtime.InteropServices.HandleRef handle, out double power);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_MeasureTXOUTPower", CallingConvention = CallingConvention.StdCall)]
            public static extern int MeasureTXOUTPower_64(System.Runtime.InteropServices.HandleRef handle, out double power);
            public static int MeasureTXOUTPower(System.Runtime.InteropServices.HandleRef handle, out double power)
            {
                if (_is64BitProcess)
                    return MeasureTXOUTPower_64(handle, out power);
                else
                    return MeasureTXOUTPower_32(handle, out power);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetRXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXLOFrequency_32(System.Runtime.InteropServices.HandleRef handle, out double frequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetRXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXLOFrequency_64(System.Runtime.InteropServices.HandleRef handle, out double frequency);
            public static int GetRXLOFrequency(System.Runtime.InteropServices.HandleRef handle, out double frequency)
            {
                if (_is64BitProcess)
                    return GetRXLOFrequency_64(handle, out frequency);
                else
                    return GetRXLOFrequency_32(handle, out frequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetRXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXPathGain_32(System.Runtime.InteropServices.HandleRef handle, out double gain);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetRXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRXPathGain_64(System.Runtime.InteropServices.HandleRef handle, out double gain);
            public static int GetRXPathGain(System.Runtime.InteropServices.HandleRef handle, out double gain)
            {
                if (_is64BitProcess)
                    return GetRXPathGain_64(handle, out gain);
                else
                    return GetRXPathGain_32(handle, out gain);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetTXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXLOFrequency_32(System.Runtime.InteropServices.HandleRef handle, out double frequency);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetTXLOFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXLOFrequency_64(System.Runtime.InteropServices.HandleRef handle, out double frequency);
            public static int GetTXLOFrequency(System.Runtime.InteropServices.HandleRef handle, out double frequency)
            {
                if (_is64BitProcess)
                    return GetTXLOFrequency_64(handle, out frequency);
                else
                    return GetTXLOFrequency_32(handle, out frequency);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetTXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXPathGain_32(System.Runtime.InteropServices.HandleRef handle, out double gain);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetTXPathGain", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetTXPathGain_64(System.Runtime.InteropServices.HandleRef handle, out double gain);
            public static int GetTXPathGain(System.Runtime.InteropServices.HandleRef handle, out double gain)
            {
                if (_is64BitProcess)
                    return GetTXPathGain_64(handle, out gain);
                else
                    return GetTXPathGain_32(handle, out gain);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ClearInterchangeWarnings", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearInterchangeWarnings_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ClearInterchangeWarnings", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearInterchangeWarnings_64(System.Runtime.InteropServices.HandleRef handle);
            public static int ClearInterchangeWarnings(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return ClearInterchangeWarnings_64(handle);
                else
                    return ClearInterchangeWarnings_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_Disable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Disable_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_Disable", CallingConvention = CallingConvention.StdCall)]
            public static extern int Disable_64(System.Runtime.InteropServices.HandleRef handle);
            public static int Disable(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return Disable_64(handle);
                else
                    return Disable_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query_32(System.Runtime.InteropServices.HandleRef handle, out int errorCode, System.Text.StringBuilder errorMessage);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query_64(System.Runtime.InteropServices.HandleRef handle, out int errorCode, System.Text.StringBuilder errorMessage);
            public static int error_query(System.Runtime.InteropServices.HandleRef handle, out int errorCode, System.Text.StringBuilder errorMessage)
            {
                if (_is64BitProcess)
                    return error_query_64(handle, out errorCode, errorMessage);
                else
                    return error_query_32(handle, out errorCode, errorMessage);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetNextCoercionRecord", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextCoercionRecord_32(System.Runtime.InteropServices.HandleRef handle, int coercionRecordBufferSize, System.Text.StringBuilder coercionRecord);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetNextCoercionRecord", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextCoercionRecord_64(System.Runtime.InteropServices.HandleRef handle, int coercionRecordBufferSize, System.Text.StringBuilder coercionRecord);
            public static int GetNextCoercionRecord(System.Runtime.InteropServices.HandleRef handle, int coercionRecordBufferSize, System.Text.StringBuilder coercionRecord)
            {
                if (_is64BitProcess)
                    return GetNextCoercionRecord_64(handle, coercionRecordBufferSize, coercionRecord);
                else
                    return GetNextCoercionRecord_32(handle, coercionRecordBufferSize, coercionRecord);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetNextInterchangeWarning", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextInterchangeWarning_32(System.Runtime.InteropServices.HandleRef handle, int interchangeWarningBufferSize, System.Text.StringBuilder interchangeWarning);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetNextInterchangeWarning", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNextInterchangeWarning_64(System.Runtime.InteropServices.HandleRef handle, int interchangeWarningBufferSize, System.Text.StringBuilder interchangeWarning);
            public static int GetNextInterchangeWarning(System.Runtime.InteropServices.HandleRef handle, int interchangeWarningBufferSize, System.Text.StringBuilder interchangeWarning)
            {
                if (_is64BitProcess)
                    return GetNextInterchangeWarning_64(handle, interchangeWarningBufferSize, interchangeWarning);
                else
                    return GetNextInterchangeWarning_32(handle, interchangeWarningBufferSize, interchangeWarning);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_InvalidateAllAttributes", CallingConvention = CallingConvention.StdCall)]
            public static extern int InvalidateAllAttributes_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_InvalidateAllAttributes", CallingConvention = CallingConvention.StdCall)]
            public static extern int InvalidateAllAttributes_64(System.Runtime.InteropServices.HandleRef handle);
            public static int InvalidateAllAttributes(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return InvalidateAllAttributes_64(handle);
                else
                    return InvalidateAllAttributes_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset_64(System.Runtime.InteropServices.HandleRef handle);
            public static int reset(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return reset_64(handle);
                else
                    return reset_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ResetInterchangeCheck", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetInterchangeCheck_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ResetInterchangeCheck", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetInterchangeCheck_64(System.Runtime.InteropServices.HandleRef handle);
            public static int ResetInterchangeCheck(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return ResetInterchangeCheck_64(handle);
                else
                    return ResetInterchangeCheck_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_ResetWithDefaults", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetWithDefaults_32(System.Runtime.InteropServices.HandleRef handle);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_ResetWithDefaults", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetWithDefaults_64(System.Runtime.InteropServices.HandleRef handle);
            public static int ResetWithDefaults(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return ResetWithDefaults_64(handle);
                else
                    return ResetWithDefaults_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query_32(System.Runtime.InteropServices.HandleRef handle, System.Text.StringBuilder driverRev, System.Text.StringBuilder instrRev);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query_64(System.Runtime.InteropServices.HandleRef handle, System.Text.StringBuilder driverRev, System.Text.StringBuilder instrRev);
            public static int revision_query(System.Runtime.InteropServices.HandleRef handle, System.Text.StringBuilder driverRev, System.Text.StringBuilder instrRev)
            {
                if (_is64BitProcess)
                    return revision_query_64(handle, driverRev, instrRev);
                else
                    return revision_query_32(handle, driverRev, instrRev);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test_32(System.Runtime.InteropServices.HandleRef handle, out short testResult, System.Text.StringBuilder testMessage);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test_64(System.Runtime.InteropServices.HandleRef handle, out short testResult, System.Text.StringBuilder testMessage);
            public static int self_test(System.Runtime.InteropServices.HandleRef handle, out short testResult, System.Text.StringBuilder testMessage)
            {
                if (_is64BitProcess)
                    return self_test_64(handle, out testResult, testMessage);
                else
                    return self_test_32(handle, out testResult, testMessage);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close_32(System.Runtime.InteropServices.HandleRef handle);

            [DllImport(NativeDLLName64, EntryPoint = "si2250_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close_64(System.Runtime.InteropServices.HandleRef handle);
            public static int close(System.Runtime.InteropServices.HandleRef handle)
            {
                if (_is64BitProcess)
                    return close_64(handle);
                else
                    return close_32(handle);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message_32(System.Runtime.InteropServices.HandleRef handle, int errorCode, System.Text.StringBuilder errorMessage);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message_64(System.Runtime.InteropServices.HandleRef handle, int errorCode, System.Text.StringBuilder errorMessage);
            public static int error_message(System.Runtime.InteropServices.HandleRef handle, int errorCode, System.Text.StringBuilder errorMessage)
            {
                if (_is64BitProcess)
                    return error_message_64(handle, errorCode, errorMessage);
                else
                    return error_message_32(handle, errorCode, errorMessage);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out ushort attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out ushort attributeValue);
            public static int GetAttributeViBoolean(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out ushort attributeValue)
            {
                if (_is64BitProcess)
                    return GetAttributeViBoolean_64(handle, repCapIdentifier, attributeID, out attributeValue);
                else
                    return GetAttributeViBoolean_32(handle, repCapIdentifier, attributeID, out attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out int attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out int attributeValue);
            public static int GetAttributeViInt32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out int attributeValue)
            {
                if (_is64BitProcess)
                    return GetAttributeViInt32_64(handle, repCapIdentifier, attributeID, out attributeValue);
                else
                    return GetAttributeViInt32_32(handle, repCapIdentifier, attributeID, out attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out double attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out double attributeValue);
            public static int GetAttributeViReal64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out double attributeValue)
            {
                if (_is64BitProcess)
                    return GetAttributeViReal64_64(handle, repCapIdentifier, attributeID, out attributeValue);
                else
                    return GetAttributeViReal64_32(handle, repCapIdentifier, attributeID, out attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out System.Runtime.InteropServices.HandleRef attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out System.Runtime.InteropServices.HandleRef attributeValue);
            public static int GetAttributeViSession(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, out System.Runtime.InteropServices.HandleRef attributeValue)
            {
                if (_is64BitProcess)
                    return GetAttributeViSession_64(handle, repCapIdentifier, attributeID, out attributeValue);
                else
                    return GetAttributeViSession_32(handle, repCapIdentifier, attributeID, out attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, int attributeValueBufferSize, System.Text.StringBuilder attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, int attributeValueBufferSize, System.Text.StringBuilder attributeValue);
            public static int GetAttributeViString(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, int attributeValueBufferSize, System.Text.StringBuilder attributeValue)
            {
                if (_is64BitProcess)
                    return GetAttributeViString_64(handle, repCapIdentifier, attributeID, attributeValueBufferSize, attributeValue);
                else
                    return GetAttributeViString_32(handle, repCapIdentifier, attributeID, attributeValueBufferSize, attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, ushort attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, ushort attributeValue);
            public static int SetAttributeViBoolean(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, ushort attributeValue)
            {
                if (_is64BitProcess)
                    return SetAttributeViBoolean_64(handle, repCapIdentifier, attributeID, attributeValue);
                else
                    return SetAttributeViBoolean_32(handle, repCapIdentifier, attributeID, attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, int attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, int attributeValue);
            public static int SetAttributeViInt32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, int attributeValue)
            {
                if (_is64BitProcess)
                    return SetAttributeViInt32_64(handle, repCapIdentifier, attributeID, attributeValue);
                else
                    return SetAttributeViInt32_32(handle, repCapIdentifier, attributeID, attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, double attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, double attributeValue);
            public static int SetAttributeViReal64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, double attributeValue)
            {
                if (_is64BitProcess)
                    return SetAttributeViReal64_64(handle, repCapIdentifier, attributeID, attributeValue);
                else
                    return SetAttributeViReal64_32(handle, repCapIdentifier, attributeID, attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, System.Runtime.InteropServices.HandleRef attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, System.Runtime.InteropServices.HandleRef attributeValue);
            public static int SetAttributeViSession(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, System.Runtime.InteropServices.HandleRef attributeValue)
            {
                if (_is64BitProcess)
                    return SetAttributeViSession_64(handle, repCapIdentifier, attributeID, attributeValue);
                else
                    return SetAttributeViSession_32(handle, repCapIdentifier, attributeID, attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString_32(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, string attributeValue);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString_64(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, string attributeValue);
            public static int SetAttributeViString(System.Runtime.InteropServices.HandleRef handle, string repCapIdentifier, int attributeID, string attributeValue)
            {
                if (_is64BitProcess)
                    return SetAttributeViString_64(handle, repCapIdentifier, attributeID, attributeValue);
                else
                    return SetAttributeViString_32(handle, repCapIdentifier, attributeID, attributeValue);
            }

            [DllImport(NativeDLLName32, EntryPoint = "si2250_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError_32(System.Runtime.InteropServices.HandleRef handle, out int errorCode, int errorDescriptionBufferSize, System.Text.StringBuilder errorDescription);
            [DllImport(NativeDLLName64, EntryPoint = "si2250_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError_64(System.Runtime.InteropServices.HandleRef handle, out int errorCode, int errorDescriptionBufferSize, System.Text.StringBuilder errorDescription);
            public static int GetError(System.Runtime.InteropServices.HandleRef handle, out int errorCode, int errorDescriptionBufferSize, System.Text.StringBuilder errorDescription)
            {
                if (_is64BitProcess)
                    return GetError_64(handle, out errorCode, errorDescriptionBufferSize, errorDescription);
                else
                    return GetError_32(handle, out errorCode, errorDescriptionBufferSize, errorDescription);
            }

            public static int TestForError(System.Runtime.InteropServices.HandleRef handle, int status)
            {
                if ((status < 0))
                {
                    NativeMethods.ThrowError(handle, status);
                }
                return status;
            }

            public static int ThrowError(System.Runtime.InteropServices.HandleRef handle, int code)
            {
                int status;
                int size = NativeMethods.GetError(handle, out status, 0, null);
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                if ((size >= 0))
                {
                    msg.Capacity = size;
                    NativeMethods.GetError(handle, out status, size, msg);
                }
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            }
        }
    }

    public static class si2250Constants
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