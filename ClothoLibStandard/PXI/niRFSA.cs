using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NationalInstruments.ModularInstruments.Interop
{
    public class niRFSA : object, System.IDisposable
    {

        private HandleRef _handle;

        private bool _disposed = true;

        private const string rfsaModule = "niRFSA.dll";

        public HandleRef Handle
        {
            get
            {
                return _handle;
            }
        }

        ~niRFSA() { Dispose(false); }

        /// <summary>
        /// 
        /// Creates a new session for the device and performs the following initialization actions:
        /// 
        /// Creates a new instrument driver session to the RF vector signal analyzer, using the downconverter resource name you specify with resourceName.
        /// Sends initialization commands to reset all hardware modules to a known state necessary for NI-RFSA operation.
        /// 
        /// Note:Before initializing the RF vector signal analyzer, an IF digitizer module (and an LO source for the NI 5663 only) must be associated with the RF downconverter module in MAX. After association, pass the RF downconverter device name to this VI to initialize both modules. To change the digitizer and LO source associations, modify the downconverter Properties page in MAX, or use the niRFSA_InitWithOptions function to override the association specified in MAX. Refer to the NI RF Vector Signal                 Analyzers Getting Started Guide, installed at Start&#187;All Programs?National  Instruments&#187;NI-RFSA&#187;Documentation for information about MAX association.
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// </summary>
        /// <param name="Resource_Name">
        /// 
        /// resourceName
        /// ViRsrc
        /// Specifies the resource name of the device to initialize.
        /// 
        /// Example #
        /// Device Type
        /// Syntax
        /// 1
        /// myDAQmxDevice
        /// NI-DAQmx device, device name =
        ///                &quot;myDAQmxDevice&quot;
        /// 2
        /// myLogicalName
        /// IVI logical name, name =
        ///                   &quot;myLogicalName&quot;
        /// 
        /// For NI-DAQmx devices, the syntax is the device name specified in MAX, as shown in
        ///             Example 1. Typical default names for NI-DAQmx devices in MAX are Dev1 or PXI1Slot2. You
        ///             can rename an NI-DAQmx device by right-clicking on the name in MAX and entering a new
        ///             name. You also can pass in the name of an IVI logical name configured with the IVI
        ///             Configuration utility. For additional information, refer to the Installed Devices&#187;IVI topic of the
        ///             Measurement &amp; Automation Explorer Help.
        /// 
        /// Caution&#160;&#160;NI-DAQmx device names are not case-sensitive. However, all IVI logical names are case-sensitive. If you use an IVI logical name, verify the name is identical to the name shown in the IVI Configuration Utility.
        /// 
        /// </param>
        /// <param name="ID_Query">
        /// 
        /// IDQuery
        /// ViBoolean
        /// specifies whether NI-RFSA performs an ID query. When you perform an ID query, NI-RFSA verifies that the device you initialize is supported. 
        ///                                             VI_TRUE (Yes)
        /// 
        ///                     Perform an ID query. This value is the default.
        /// 
        ///                                                             VI_FALSE (No)
        /// 
        ///                     Do not perform an ID query.
        /// 
        /// </param>
        /// <param name="Reset">
        /// 
        /// reset
        /// ViBoolean
        /// Specifies whether the NI-RFSA device is reset during the initialization procedure.
        /// 
        /// VI_TRUE (Yes)The device is reset.
        /// VI_FALSE (No)The device is not reset. This value is the default.
        /// 
        /// </param>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session.
        /// 
        /// </param>
        public niRFSA(string Resource_Name, bool ID_Query, bool Reset)
        {
            System.IntPtr handle;
            int pInvokeResult = PInvoke.init(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset), out handle);
            _handle = new HandleRef(this, handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// 
        /// Creates a new session for the device and performs the following initialization actions:
        /// Creates a new instrument driver session to the RF vector signal analyzer, using the downconverter resource name you specify with resourceName.
        /// Sends initialization commands to reset all hardware modules to a known state necessary for NI-RFSA operation.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// 
        /// resourceName
        /// ViRsrc
        /// Specifies the resource name of the device to initialize.
        /// 
        /// Example #
        /// Device Type
        /// Syntax
        /// 1
        /// myDAQmxDevice
        /// NI-DAQmx device, device name =
        ///                &quot;myDAQmxDevice&quot;
        /// 2
        /// myLogicalName
        /// IVI logical name, name =
        ///                   &quot;myLogicalName&quot;
        /// 
        /// For NI-DAQmx devices, the syntax is the device name specified in MAX, as shown in
        ///             Example 1. Typical default names for NI-DAQmx devices in MAX are Dev1 or PXI1Slot2. You
        ///             can rename an NI-DAQmx device by right-clicking on the name in MAX and entering a new
        ///             name. You also can pass in the name of an IVI logical name configured with the IVI
        ///             Configuration utility. For additional information, refer to the Installed Devices&#187;IVI topic of the
        ///             Measurement &amp; Automation Explorer Help.
        /// 
        /// Caution&#160;&#160;NI-DAQmx device names are not case-sensitive. However, all IVI logical names are case-sensitive. If you use an IVI logical name, verify the name is identical to the name shown in the IVI Configuration Utility.
        ///  
        /// </param>
        /// <param name="ID_Query">
        /// 
        /// IDQuery
        /// ViBoolean
        /// specifies whether NI-RFSA performs an ID query. When you perform an ID query, NI-RFSA verifies that the device you initialize is supported. 
        ///                                             VI_TRUE (Yes)
        /// 
        ///                     Perform an ID query. This value is the default.
        /// 
        ///                                                             VI_FALSE (No)
        /// 
        ///                     Do not perform an ID query.
        /// 
        /// </param>
        /// <param name="Reset">
        /// 
        /// reset
        /// ViBoolean
        /// Specifies whether the NI-RFSA device is reset during the initialization procedure.
        /// 
        /// VI_TRUE (Yes)The device is reset.
        /// VI_FALSE (No)The device is not reset. This value is the default.
        /// 
        /// </param>
        /// <param name="Option_String">
        /// 
        /// optionString
        /// ViConstString
        /// Sets the initial value of certain attributes for the session. The following attributes are used in this parameter.
        /// 
        /// Name
        /// Attribute
        /// RangeCheck
        /// NIRFSA_ATTR_RANGE_CHECK
        /// QueryInstrStatus
        /// NIRFSA_ATTR_QUERY_INSTRUMENT_STATUS
        /// Cache
        /// NIRFSA_ATTR_CACHE
        /// RecordCoercions
        /// NIRFSA_ATTR_RECORD_COERCIONS
        /// DriverSetup
        /// NIRFSA_ATTR_DRIVER_SETUP
        /// 
        /// The format of this string is, &quot;AttributeName=Value&quot; where AttributeName is the name of the attribute and Value is the value to which the attribute will be set.  To set multiple attributes, separate their assignments with a comma.
        /// 
        /// Example Option String:
        /// &quot;RangeCheck=1,QueryInstrStatus=0,Cache=1,DriverSetup=Digitizer:pxi1slot4&quot;.
        /// 
        /// If you want to use the NI 5600/5601 with an external digitizer for downconverter-only mode, use the following DriverSetup string: DriverSetup=Digitizer:&lt;external&gt;.
        /// 
        /// NI 5663&#8212;LO source&#8212;Specifies the resource name of the LO source to use for this session. If you want to use the NI&#160;5601 with an LO source other than the NI 5652, use the following DriverSetup tag:DriverSetup=LO:&lt;external&gt;.
        /// 
        /// To specify multiple resources in the DriverSetup string, separate their assignments with a semicolon.
        /// 
        /// </param>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session.
        /// 
        /// </param>
        public niRFSA(string Resource_Name, bool ID_Query, bool Reset, string Option_String)
        {
            System.IntPtr handle;
            int pInvokeResult = PInvoke.InitWithOptions(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset), Option_String, out handle);
            _handle = new HandleRef(this, handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// 
        /// Configures whether the session acquires I/Q data or computes a power spectrum over the specified frequency range. 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Acquisition_Type">
        /// 
        /// acquisitionType
        /// ViInt32
        /// Configures the type of acquisition.
        /// 
        ///  IQ
        /// Configures the driver for I/Q acquisitions. This value is the default.
        /// Spectrum
        /// Configures the driver for spectrum acquisitions.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureAcquisitionType(int Acquisition_Type)
        {
            int pInvokeResult = PInvoke.ConfigureAcquisitionType(this._handle, Acquisition_Type);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the reference level. The reference level represents the maximum expected power of an input RF signal.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Reference_Level">
        /// 
        /// referenceLevel
        /// ViReal64
        /// Specifies the expected total integrated power, in dBm, of the RF input signal.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureReferenceLevel(string Channel_List, double Reference_Level)
        {
            int pInvokeResult = PInvoke.ConfigureReferenceLevel(this._handle, Channel_List, Reference_Level);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the carrier frequency of the RF vector signal analyzer hardware for an I/Q acquisition. The carrier frequency is the center frequency of the I/Q acquisition.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Carrier_Frequency">
        /// 
        /// carrierFrequency
        /// ViReal64
        /// Specifies the carrier frequency, in hertz (Hz), of the RF signal to acquire. The RF vector signal analyzer tunes to this frequency. NI-RFSA may coerce this value based on hardware settings and downconversion settings.
        /// 
        /// NI-RFSA sets the NIRFSA_ATTR_IQ_CARRIER_FREQUENCY attribute to this value. Refer to the specifications document that shipped with your device for allowable frequency settings.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureIQCarrierFrequency(string Channel_List, double Carrier_Frequency)
        {
            int pInvokeResult = PInvoke.ConfigureIQCarrierFrequency(this._handle, Channel_List, Carrier_Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the rate at which the device samples I/Q values. The utilized downconverter bandwidth is equal to the coerced iqRate times 0.8.
        /// 
        /// NI 5661&#8212;You should not need to configure an iqRate higher than 25 Samples per second (S/s) because the NI 5600 RF downconverter bandwidth is 20 MHz. If you choose to configure a higher I/Q rate, you may see aliasing effects at negative frequencies because the IF frequency of the NI 5600 RF downconverter is 15 MHz.
        /// NI 5663&#8212;Your maximum allowed instantaneous bandwidth depends on the I/Q carrier frequency you use. Refer to the NI 5601 RF downconverter overview for more information about instantaneous bandwidth.
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="IQ_Rate">
        /// 
        /// iqRate
        /// ViReal64
        /// Specifies the I/Q rate for the acquisition. The value is expressed in samples per second (S/s).
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureIQRate(string Channel_List, double IQ_Rate)
        {
            int pInvokeResult = PInvoke.ConfigureIQRate(this._handle, Channel_List, IQ_Rate);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the number of samples in a finite acquisition or configures the device to continuously acquire samples.
        /// If you configure the device for finite acquisition, it acquires the specified number of samples and then stops the acquisition. You can configure the device to acquire multiple records using the niRFSA_ConfigureNumberOfRecords function. Each record contains the number of samples specified in this function. The default number of records to acquire is 1.
        /// 
        /// If you configure the device to continuously acquire samples, it continues acquiring data until you call the niRFSA_Abort function to abort the acquisition. The device stores data in onboard memory in a circular fashion. After the device fills the memory, it starts overwriting previously acquired data from the beginning of the memory buffer. Retrieve the samples as they are being acquired using the niRFSA fetch IQ functions to avoid overwriting data before you retrieve it.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Number_of_Samples_Is_Finite">
        /// 
        /// numberOfSamplesIsFinite
        /// ViBoolean
        /// Specifies whether to configure the device to acquire a finite number of samples or to acquire samples continuously.
        /// 
        /// VI_TRUEThe device acquires a finite number of samples.
        /// VI_FALSEThe device continuously acquires samples.
        /// 
        /// </param>
        /// <param name="Samples_Per_Record">
        /// 
        /// samplesPerRecord
        /// ViInt64
        /// Specifies the number of samples per record if numberOfSamplesIsFinite is set to VI_TRUE.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureNumberOfSamples(string Channel_List, bool Number_of_Samples_Is_Finite, Int64 Samples_Per_Record)
        {
            int pInvokeResult = PInvoke.ConfigureNumberOfSamples(this._handle, Channel_List, System.Convert.ToUInt16(Number_of_Samples_Is_Finite), Samples_Per_Record);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the number of records in a finite acquisition or configures the device to continuously acquire records. You can only configure the device to acquire multiple records if the numberOfRecordsIsFinite parameter is set to VI_TRUE.
        /// 
        /// If you configure the device to acquire records continuously, it continues acquiring records until you call the niRFSA_abort function to abort the acquisition. The device stores records in onboard memory in a circular fashion. Once the device fills the memory, it starts overwriting previously acquired records from the beginning of the memory buffer. Fetch the records as they are acquired to avoid overwritting data before you retrieve it.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Number_of_Records_Is_Finite">
        /// 
        /// numberOfRecordsIsFinite
        /// ViBoolean
        /// Specifies whether to configure the device to acquire a finite number of records or to acquire records continuously.
        /// 
        /// VI_TRUEThe device acquires a finite number of records.
        /// VI_FALSEThe device continuously acquires records.
        /// 
        /// </param>
        /// <param name="Number_of_Records">
        /// 
        /// numberOfRecords
        /// ViInt64
        /// Specifies the number of records to acquire if numberOfRecordsIsFinite is set to VI_TRUE. The default number of records to acquire is 1.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureNumberOfRecords(string Channel_List, bool Number_of_Records_Is_Finite, Int64 Number_of_Records)
        {
            int pInvokeResult = PInvoke.ConfigureNumberOfRecords(this._handle, Channel_List, System.Convert.ToUInt16(Number_of_Records_Is_Finite), Number_of_Records);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the span and center frequency of the spectrum read by NI-RFSA. A spectrum acquisition consists of data surrounding the center frequency.
        /// 
        /// Note:If you configure the spectrum span to a value larger than the instantaneous bandwidth of the device, NI-RFSA
        ///             performs multiple acquisitions and combines them into a spectrum of the size you
        ///             requested.
        /// 
        /// Note:For the NI 5663, NI-RFSA does not support continuous acquisitions from frequency bands for which the instantaneous bandwidth differs. Refer to the NI 5601 RF Downconverter topic for more information about instantaneous bandwidth.
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Center_Frequency">
        /// 
        /// centerFrequency
        /// ViReal64
        /// Specifies the center frequency of a spectrum acquisition. The value is expressed in hertz (Hz). The NI-RFSA device you use determines the valid range. Refer to your device specifications document for more information about frequency range.
        /// 
        /// </param>
        /// <param name="Span">
        /// 
        /// span
        /// ViReal64
        /// Specifies the span of a spectrum acquisition. The value is expressed in hertz (Hz).
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureSpectrumFrequencyCenterSpan(string Channel_List, double Center_Frequency, double Span)
        {
            int pInvokeResult = PInvoke.ConfigureSpectrumFrequencyCenterSpan(this._handle, Channel_List, Center_Frequency, Span);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the start and stop frequencies of the spectrum read by NI-RFSA.
        /// 
        /// Note:If you configure the spectrum span (stopFrequency &#8211;
        ///                 startFrequency) to a value larger than the instantaneous bandwidth of the device, NI-RFSA
        ///             performs multiple acquisitions and combines them into a spectrum of the size you
        ///             request.
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Start_Frequency">
        /// 
        /// startFrequency
        /// ViReal64
        /// Specifies the lower limit of a span of frequencies. This value is expressed in hertz (Hz).
        /// 
        /// </param>
        /// <param name="Stop_Frequency">
        /// 
        /// stopFrequency
        /// ViReal64
        /// Specifies the upper limit of a span of frequencies. This value is expressed in hertz (Hz).
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureSpectrumFrequencyStartStop(string Channel_List, double Start_Frequency, double Stop_Frequency)
        {
            int pInvokeResult = PInvoke.ConfigureSpectrumFrequencyStartStop(this._handle, Channel_List, Start_Frequency, Stop_Frequency);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the resolution bandwidth of a spectrum acquisition. The resolution bandwidth controls the width of the frequency bins in the power spectrum computed by NI-RFSA. A larger value for resolution bandwidth means the frequency bins are wider, and hence you get fewer bins, or spectral lines.
        /// By default, the resolution bandwidth value corresponds to the 3&#160;decibels (dB) bandwidth of the window type NI-RFSA uses to compute the spectrum. To specify the frequency bin width directly, change the NIRFSA_ATTR_RESOLUTION_BANDWIDTH_TYPE attribute to NIRFSA_VAL_BIN_WIDTH.
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Resolution_Bandwidth">
        /// 
        /// resolutionBandwidth
        /// ViReal64
        /// Specifies the resolution bandwidth of a spectrum acquisition. The value is expressed in hertz (Hz). Configure the type of resolution bandwidth with the NIRFSA_ATTR_RESOLUTION_BANDWIDTH_TYPE attribute.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureResolutionBandwidth(string Channel_List, double Resolution_Bandwidth)
        {
            int pInvokeResult = PInvoke.ConfigureResolutionBandwidth(this._handle, Channel_List, Resolution_Bandwidth);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for a digital edge Start trigger at the beginning of the acquisition.
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Source">
        /// 
        /// source
        /// ViConstString
        /// Specifies the source of the digital edge for the Start trigger.
        /// 
        /// PFI0
        /// The trigger is received on PFI 0.
        /// PFI1
        /// The trigger is received on PFI 1.
        /// PXI_Trig0
        /// The trigger is received on PXI trigger line 0.
        /// PXI_Trig1
        /// The trigger is received on PXI trigger line 1.
        /// PXI_Trig2
        /// The trigger is received on PXI trigger line 2.
        /// PXI_Trig3
        /// The trigger is received on PXI trigger line 3.
        /// PXI_Trig4
        /// The trigger is received on PXI trigger line 4.
        /// PXI_Trig5
        /// The trigger is received on PXI trigger line 5.
        /// PXI_Trig6
        /// The trigger is received on PXI trigger line 6.
        /// PXI_Trig7
        /// The trigger is received on PXI trigger line 7.
        /// PXI_STAR
        /// The trigger is received on the PXI star trigger line.
        /// 
        /// </param>
        /// <param name="Edge">
        /// 
        /// edge
        /// ViInt32
        /// Specifies the edge to detect.
        /// 
        /// Rising EdgeNI-RFSA detects a rising edge.
        /// Falling EdgeNI-RFSA detects a falling edge.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureDigitalEdgeStartTrigger(string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeStartTrigger(this._handle, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for a software Start trigger at the beginning of the acquisition. The device  waits until you call the niRFSA_SendSoftwareEdgeTrigger function to assert the trigger.
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureSoftwareEdgeStartTrigger()
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareEdgeStartTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to not wait for a Start trigger at the beginning of the acquisition. This function is only necessary if you configured a Start trigger in the past and now want to disable it.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int DisableStartTrigger()
        {
            int pInvokeResult = PInvoke.DisableStartTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for a digital edge Reference trigger to mark a reference point within the record.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Source">
        /// 
        /// source
        /// ViConstString
        /// Specifies the source of the digital edge for the Reference trigger.
        /// 
        ///  NIRFSA_VAL_PFI0_STR  (&quot;PFI0&quot;)
        /// The trigger is received on PFI 0.
        /// NIRFSA_VAL_PFI1_STR  (&quot;PFI1&quot;)
        /// The trigger is received on PFI 1.
        ///  NIRFSA_VAL_PXI_TRIG0_STR (&quot;PXI_Trig0&quot;)
        /// The trigger is received on PXI trigger line 0.
        ///  NIRFSA_VAL_PXI_TRIG1_STR (&quot;PXI_Trig1&quot;)
        /// The trigger is received on PXI trigger line 1.
        ///  NIRFSA_VAL_PXI_TRIG2_STR (&quot;PXI_Trig2&quot;)
        /// The trigger is received on PXI trigger line 2.
        ///  NIRFSA_VAL_PXI_TRIG3_STR (&quot;PXI_Trig3&quot;)
        /// The trigger is received on PXI trigger line 3.
        ///  NIRFSA_VAL_PXI_TRIG4_STR (&quot;PXI_Trig4&quot;)
        /// The trigger is received on PXI trigger line 4.
        ///  NIRFSA_VAL_PXI_TRIG5_STR (&quot;PXI_Trig5&quot;)
        /// The trigger is received on PXI trigger line 5.
        ///  NIRFSA_VAL_PXI_TRIG6_STR (&quot;PXI_Trig6&quot;)
        /// The trigger is received on PXI trigger line 6.
        ///  NIRFSA_VAL_PXI_TRIG7_STR (&quot;PXI_Trig7&quot;)
        /// The trigger is received on PXI trigger line 7.
        ///  NIRFSA_VAL_PXI_STAR_STR (&quot;PXI_STAR&quot;)
        /// The trigger is received on the PXI star trigger line.
        /// 
        /// </param>
        /// <param name="Edge">
        /// 
        /// edge
        /// ViInt32
        /// Specifies the trigger edge to detect.
        /// 
        /// NIRFSA_VAL_RISING_EDGE (900)NI-RFSA detects a rising edge.
        /// NIRFSA_VAL_FALLING_EDGE (901)NI-RFSA detects a falling edge.
        /// 
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// 
        /// pretriggerSamples
        /// ViInt64
        /// Specifies the number of samples to store for each record that was acquired in the time period immediately before the trigger occurred. 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureDigitalEdgeRefTrigger(string Source, int Edge, Int64 Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeRefTrigger(this._handle, Source, Edge, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for the complex power of the I/Q data to cross the specified threshold to mark a reference point within the record.
        /// 
        /// To trigger on bursted signals, add a minimum quiet time (configured with the NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_MINIMUM_QUIET_TIME attribute) to ensure the trigger does not occur in the middle of a burst if the acquisition starts while a burst is being generated. The quiet time should be set to a value smaller than the time between bursts, but large enough to ignore power changes within a burst.
        /// 
        /// Supported Devices: NI 5661/5663
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Source">
        /// 
        /// source
        /// ViConstString
        /// Specifies the source of the RF signal for the power edge Reference trigger. The only supported value is channel &quot;0&quot;.
        /// 
        /// </param>
        /// <param name="Level">
        /// 
        /// level
        /// ViReal64
        /// Specifies the threshold above or below which the device  triggers.
        /// 
        /// </param>
        /// <param name="Slope">
        /// 
        /// slope
        /// ViInt32
        /// Specifies whether the device detects a positive or negative slope on the trigger signal.
        /// 
        /// NIRFSA_VAL_RISING_SLOPE (1000)NI-RFSA detects a rising edge (positive slope). This value is the default.
        /// NIRFSA_VAL_FALLING_SLOPE (1001)NI-RFSA detects a falling edge (negative slope).
        /// 
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// 
        /// pretriggerSamples
        /// ViInt64
        /// Specifies the number of samples to store for each record that was acquired in the time period immediately before the trigger occurred. 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureIQPowerEdgeRefTrigger(string Source, double Level, int Slope, Int64 Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigureIQPowerEdgeRefTrigger(this._handle, Source, Level, Slope, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for a software Reference trigger to mark a reference point within the record. The device waits until you call the niRFSA_SendSoftwareEdgeTrigger function to assert the trigger.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Pretrigger_Samples">
        /// 
        /// pretriggerSamples
        /// ViInt64
        /// Specifies the number of samples to store for each record that was acquired in the time period immediately before the trigger occurred. 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureSoftwareEdgeRefTrigger(Int64 Pretrigger_Samples)
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareEdgeRefTrigger(this._handle, Pretrigger_Samples);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to not wait for a Reference trigger to mark a reference point within a record. This function is only necessary if you configured a Reference trigger in the past and now want to disable it.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int DisableRefTrigger()
        {
            int pInvokeResult = PInvoke.DisableRefTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for a digital edge Advance trigger. The Advance trigger indicates where a new record begins.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Source">
        /// 
        /// source
        /// ViConstString
        /// Specifies the source of the digital edge for the Advance trigger.
        /// 
        ///  NIRFSA_VAL_PFI0_STR  (&quot;PFI0&quot;)
        /// The trigger is received on PFI 0.
        /// NIRFSA_VAL_PFI1_STR  (&quot;PFI1&quot;)
        /// The trigger is received on PFI 1.
        ///  NIRFSA_VAL_PXI_TRIG0_STR (&quot;PXI_Trig0&quot;)
        /// The trigger is received on PXI trigger line 0.
        ///  NIRFSA_VAL_PXI_TRIG1_STR (&quot;PXI_Trig1&quot;)
        /// The trigger is received on PXI trigger line 1.
        ///  NIRFSA_VAL_PXI_TRIG2_STR (&quot;PXI_Trig2&quot;)
        /// The trigger is received on PXI trigger line 2.
        ///  NIRFSA_VAL_PXI_TRIG3_STR (&quot;PXI_Trig3&quot;)
        /// The trigger is received on PXI trigger line 3.
        ///  NIRFSA_VAL_PXI_TRIG4_STR (&quot;PXI_Trig4&quot;)
        /// The trigger is received on PXI trigger line 4.
        ///  NIRFSA_VAL_PXI_TRIG5_STR (&quot;PXI_Trig5&quot;)
        /// The trigger is received on PXI trigger line 5.
        ///  NIRFSA_VAL_PXI_TRIG6_STR (&quot;PXI_Trig6&quot;)
        /// The trigger is received on PXI trigger line 6.
        ///  NIRFSA_VAL_PXI_TRIG7_STR (&quot;PXI_Trig7&quot;)
        /// The trigger is received on PXI trigger line 7.
        ///  NIRFSA_VAL_PXI_STAR_STR (&quot;PXI_STAR&quot;)
        /// The trigger is received on the PXI star trigger line.
        /// 
        /// </param>
        /// <param name="Edge">
        /// 
        /// edge
        /// ViInt32
        /// Specifies the trigger edge to detect.
        /// 
        /// NIRFSA_VAL_RISING_EDGE (900)NI-RFSA detects a rising edge.
        /// NIRFSA_VAL_FALLING_EDGE (901)NI-RFSA detects a falling edge.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureDigitalEdgeAdvanceTrigger(string Source, int Edge)
        {
            int pInvokeResult = PInvoke.ConfigureDigitalEdgeAdvanceTrigger(this._handle, Source, Edge);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to wait for a digital edge Advance trigger. The Advance trigger indicates where a new record begins. The device waits until you call the niRFSA_SendSoftwareEdgeTrigger function to assert the trigger.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int ConfigureSoftwareEdgeAdvanceTrigger()
        {
            int pInvokeResult = PInvoke.ConfigureSoftwareEdgeAdvanceTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the device to not use an Advance trigger. This function is only necessary if you configured an Advance trigger in the past and now want to disable it.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// The general meaning of the status code is as follows:
        /// Value           Meaning
        /// 0               Success
        /// Positive Values Warnings
        /// Negative Values Errors
        /// 
        /// </returns>
        public int DisableAdvanceTrigger()
        {
            int pInvokeResult = PInvoke.DisableAdvanceTrigger(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Sends a trigger to the device when you configure a software version of a supported trigger and the device is waiting for the trigger to be sent. This function also can be used to override a hardware trigger.
        /// This function returns an error in the following situations:
        /// You configure an invalid trigger.
        /// The instrument driver is in spectrum mode.
        /// You have not previously called the niRFSA_Initiate function.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Trigger">
        /// 
        /// trigger
        /// ViInt32
        /// Specifies the software signal to send.
        /// 
        /// NIRFSA_VAL_START_TRIGGER (1100)NI-RFSA sends a Start software trigger.
        /// NIRFSA_VAL_REF_TRIGGER (702)NI-RFSA sends a Reference software trigger.
        /// NIRFSA_VAL_ADVANCE_TRIGGER (1102)NI-RFSA sends an Advance software trigger.
        /// NIRFSA_VAL_ARM_REF_TRIGGER (1103)NI-RFSA sends an Arm Reference software trigger.
        /// 
        /// </param>
        /// <param name="Trigger_Identifier">
        /// 
        /// triggerIdentifier
        /// ViConstString
        /// Specifies a particular instance of a trigger. NI-RFSA does not currently support this parameter.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int SendSoftwareEdgeTrigger(int Trigger, string Trigger_Identifier)
        {
            int pInvokeResult = PInvoke.SendSoftwareEdgeTrigger(this._handle, Trigger, Trigger_Identifier);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Routes signals (triggers, clocks, and events) to the specified output terminal.
        /// 
        /// If you export a signal with this function and  commit the session, the signal is routed to the output terminal you specify. If you then reconfigure the signal to have a different output terminal, the previous output terminal is tristated when the session is next committed. If you change the outputTerminal to NIRFSA_VAL_DO_NOT_EXPORT and commit, the previous output terminal is tristated.
        /// 
        /// Any signals, except for those exported over PXI trigger lines, that are exported within a session persist after the session closes to prevent signal glitches between sessions. PXI trigger lines are always set to tristate when a session is closed. If you wish to have the output terminal tristated when the session closes, first change the outputTerminal for the exported signal to NIRFSA_VAL_DO_NOT_EXPORT and commit the session again before closing it.
        /// 
        /// You can also tristate all PFI lines by setting the resetDevice parameter in the niRFSA_init function or by using the niRFSA_reset function.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Signal">
        /// 
        /// signal
        /// ViInt32
        /// Specifies the type of signal to route. You can choose to export the Start, Reference, and Advance triggers and the Ready for Start, Ready for Advance, Ready for Ref, End of Record, and Done events.
        /// 
        /// NIRFSA_VAL_START_TRIGGER (1100)NI-RFSA routes a Start trigger signal.
        /// NIRFSA_VAL_REF_TRIGGER (702)NI-RFSA routes a Reference trigger signal.
        /// NIRFSA_VAL_ADVANCE_TRIGGER (1102)NI-RFSA routes an Advance trigger signal.
        /// NIRFSA_VAL_READY_FOR_START_EVENT (1200)NI-RFSA routes a Ready for Start event signal.
        /// NIRFSA_VAL_READY_FOR_ADVANCE_EVENT (1202)NI-RFSA routes a  Ready for Advance event signal.
        /// NIRFSA_VAL_READY_FOR_REF_EVENT (1201)NI-RFSA routes a Ready for Reference event signal.
        /// NIRFSA_VAL_END_OF_RECORD_EVENT (1203)NI-RFSA routes an End of Record event signal.
        /// NIRFSA_VAL_DONE_EVENT (1204)NI-RFSA routes a Done event signal.
        /// NIRFSA_VAL_REF_CLOCK (1205)NI-RFSA routes a Reference clock signal.
        /// </param>
        /// <param name="Signal_Identifier">
        /// 
        /// signalIdentifier
        /// ViConstString
        /// Specifies a particular instance of a trigger. NI-RFSA does not currently support this parameter.
        /// 
        /// </param>
        /// <param name="Output_Terminal">
        /// 
        /// outputTerminal
        /// ViConstString
        /// Specifies the terminal where the signal will be exported. You can also choose not to export any signal.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int ExportSignal(int Signal, string Signal_Identifier, string Output_Terminal)
        {
            int pInvokeResult = PInvoke.ExportSignal(this._handle, Signal, Signal_Identifier, Output_Terminal);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Configures the NI-RFSA device Reference clock.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Ref_Clock_Source">
        /// 
        /// refClockSource
        /// ViConstString
        /// Specifies the Reference clock source.
        /// 
        /// NIRFSA_VAL_ONBOARD_CLOCK_STR (&quot;OnboardClock&quot;)
        /// NI 5661&#8212;Lock the NI-RFSA device to the NI 5600 RF downconverter onboard clock.
        /// 
        /// NI 5663&#8212;Lock the NI 5622 to the NI 5652 LO source onboard clock. Connect the REF IN/OUT connector on the NI 5652 to the CLK IN terminal on the NI 5622.
        /// NIRFSA_VAL_REF_IN_STR (&quot;RefIn&quot;)
        /// NI 5661&#8212;Lock the NI-RFSA device to the signal at the external FREQ REF IN connector on the
        ///                   NI 5600.
        /// 
        /// NI 5663&#8212;Connect the external signal to the NI&#160;5652 REF IN/OUT connector. Only use this configuration in downconverter-only mode.
        /// NIRFSA_VAL_CLK_IN_STR (&quot;ClkIn&quot;)
        /// NI 5661&#8212;This configuration does not apply to the NI 5661.
        /// 
        /// NI 5663&#8212;Lock the NI 5622 to an external 10 MHz signal. Connect the external signal to the CLK IN connector on the NI 5622, and connect the NI 5622 CLK&#160;OUT connector to the REF IN/OUT connector on the NI 5652.
        /// NIRFSA_VAL_PXI_CLK_STR (&quot;PXI_Clk&quot;)
        /// NI 5661&#8212;Lock the NI-RFSA device to the PXI backplane clock using the NI&#160;5600. You must connect the PXI 10 MHz connector to the FREQ REF IN connector on the
        ///                   NI&#160;5600 front panel to use this option.
        /// 
        /// NI 5663&#8212;Lock the NI 5663 to the PXI backplane clock.
        /// 
        /// </param>
        /// <param name="Ref_Clock_Rate">
        /// 
        /// refClockRate
        /// ViReal64
        /// Specifies the reference clock rate, expressed in hertz (Hz). The default value is 10 MHz, which is the only supported value.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int ConfigureRefClock(string Ref_Clock_Source, double Ref_Clock_Rate)
        {
            int pInvokeResult = PInvoke.ConfigureRefClock(this._handle, Ref_Clock_Source, Ref_Clock_Rate);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Specifies the signal to drive the 10&#160;MHz Reference clock on the PXI backplane. This option can only be configured when the NI&#160;5600 is installed in Slot&#160;2 of the PXI chassis. 
        /// 
        /// Supported Devices: NI 5600 (downconverter only mode), NI 5661
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="PXI_Clk_10_Source">
        /// 
        /// PXIClk10Source
        /// ViConstString
        /// Specifies the signal to drive the 10 MHz Reference clock on the PXI backplane. This option can only be configured when the NI&#160;5600 is in Slot&#160;2 of the PXI chassis. 
        /// 
        ///                                                              NIRFSA_VAL_NONE_STR (&quot;None&quot;)
        /// 
        ///                     The device does not drive the PXI 10 MHz backplane Reference
        ///                   clock.
        /// 
        ///                                                             NIRFSA_VAL_ONBOARD_CLOCK_STR (&quot;OnboardClock&quot;)
        /// 
        ///                     The device drives the PXI 10 MHz backplane Reference clock with
        ///                   the NI&#160;5600 onboard clock. You must connect the 10 MHz OUT connector to the
        ///                   PXI 10 MHz I/O connector on the NI&#160;5600 front panel to use this option.
        /// 
        ///                                                             NIRFSA_VAL_REF_IN_STR (&quot;RefIn&quot;)
        /// 
        ///                     The device drives the PXI 10 MHz backplane Reference clock with
        ///                   the reference source attached to the NI&#160;5600 FREQ REF IN connector. You must
        ///                   connect the 10&#160;MHz&#160;OUT connector to the PXI&#160;10&#160;MHz&#160;I/O connector on the NI&#160;5600 front
        ///                   panel to use this option.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int ConfigurePXIChassisClk10(string PXI_Clk_10_Source)
        {
            int pInvokeResult = PInvoke.ConfigurePXIChassisClk10(this._handle, PXI_Clk_10_Source);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Resets the attribute to its default value.
        /// 
        /// Supported Devices: 
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_Name">
        /// 
        /// channelName
        /// ViConstString
        /// Specifies the name of the channel on which to reset the attribute value if the attribute is channel based. If the attribute is not channel based, set this parameter to &quot;&quot; (empty string) or VI_NULL.
        /// 
        /// </param>
        /// <param name="Attribute_ID">
        /// 
        /// attributeID
        /// ViAttr
        /// Pass the ID of an attribute.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int ResetAttribute(string Channel_Name, int Attribute_ID)
        {
            int pInvokeResult = PInvoke.ResetAttribute(this._handle, Channel_Name, Attribute_ID);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Starts an I/Q acquisition. You may use this function in conjunction with the NI-RFSA fetch IQ functions to retrieve acquired I/Q data, or use the NI-RFSA read IQ functions to both initiate the acquisition and retrieve I/Q data at one time.
        /// 
        /// For improved accuracy, add a wait after calling the niRFSA_Commit function and before calling the niRFSA_Initiate function to allow the device additional settling time. Refer to the specifications document that shipped with your device for information about settling time and accuracy.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int Initiate()
        {
            int pInvokeResult = PInvoke.Initiate(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Fetches binary I/Q data from a single record in an acquisition. The fetch transfers acquired waveform data from device memory to computer memory. The data was acquired to onboard memory previously by the hardware after the acquisition was initiated.
        /// 
        /// This function is not necessary if you use the niRFSA_ReadIQSingleRecordComplexF64 function, as a fetch is performed as part of that function.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Record_Number">
        /// 
        /// recordNumber
        /// ViInt64
        /// Specifies the record to retrieve. Record numbers are zero-based.
        /// 
        /// </param>
        /// <param name="Number_of_Samples">
        /// 
        /// numberOfSamples
        /// ViInt64
        /// Specifies the number of samples to fetch. A value of &#8211;1 specifies that NI-RFSA will fetch all samples. The default value is 1000.
        /// 
        /// </param>
        /// <param name="Timeout">
        /// 
        /// timeout
        /// ViReal64
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of &#8211;1 specifies the function waits until all data is available. A value of 0 specifies the function immediately returns available data. The default value is 10.
        /// 
        /// </param>
        /// <param name="Data">
        /// 
        /// data
        /// NIComplexI16*
        /// Returns the acquired waveform. Allocate an NI ComplexI16 array at least as large as numberOfSamples.
        /// 
        /// </param>
        /// <param name="Waveform_Info">
        /// 
        /// wfmInfo
        /// niRFSA_wfmInfo*
        /// Contains the absolute and relative timestamps for the operation, the time interval, and the actual number of samples read.
        /// 
        /// The following list provides more information about each of these properties:
        /// absolute timestamp&#8212;returns the timestamp, in seconds, of the first fetched sample that is comparable between records and acquisitions.
        /// relative timestamp&#8212;returns a timestamp that corresponds to the difference, in seconds, between the first sample returned and the Reference trigger location.
        /// dt&#8212;returns the time interval between data points in the acquired signal. The I/Q data sampling rate is the reciprocal of this value.
        /// actual samples read&#8212;returns an integer representing the number of samples in the waveform.
        /// offset&#8212;returns the offset to scale data (b) in mx+b form.
        /// gain&#8212;returns the gain to scale data (m) in mx+b form.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int FetchIQSingleRecordComplexI16(string Channel_List, Int64 Record_Number, Int64 Number_of_Samples, double Timeout, short[] Data, out niRFSA_wfmInfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchIQSingleRecordComplexI16(this._handle, Channel_List, Record_Number, Number_of_Samples, Timeout, Data, out Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Fetches I/Q data from a single record in an acquisition. The fetch transfers acquired waveform data from device memory to computer memory. The data was acquired to onboard memory previously by the hardware after the acquisition was initiated.
        /// 
        /// This function is not necessary if you use the niRFSA_ReadIQSingleRecordComplexF64 function, as the fetch is performed as part of that function.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Record_Number">
        /// 
        /// recordNumber
        /// ViInt64
        /// Specifies the record to retrieve. Record numbers are zero-based.
        /// 
        /// </param>
        /// <param name="Number_of_Samples">
        /// 
        /// numberOfSamples
        /// ViInt64
        /// Specifies the number of samples to fetch.
        /// 
        /// </param>
        /// <param name="Timeout">
        /// 
        /// timeout
        /// ViReal64
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of &#8211;1 specifies the function waits until all data is available. A value of 0 specifies the function immediately returns available data. The default value is 10.
        /// 
        /// </param>
        /// <param name="Data">
        /// 
        /// data
        /// NIComplexNumber*
        /// Returns the acquired waveform. Allocate an NIComplexNumber array at least as large as numberOfSamples.
        /// 
        /// </param>
        /// <param name="Waveform_Info">
        /// 
        /// wfmInfo
        /// niRFSA_wfmInfo*
        /// Returns the absolute and relative timestamps for the operation, the time interval, and the actual number of samples read.
        /// 
        /// The following list provides more information about each of these properties:
        /// absolute timestamp&#8212;returns the timestamp, in seconds, of the first fetched sample that is comparable between records and acquisitions.
        /// relative timestamp&#8212;returns a timestamp that corresponds to the difference, in seconds, between the first sample returned and the Reference trigger location.
        /// dt&#8212;returns the time interval between data points in the acquired signal. The I/Q data sampling rate is the reciprocal of this value.
        /// actual samples read&#8212;returns an integer representing the number of samples in the waveform.
        /// offset&#8212;returns the offset to scale data (b) in mx+b form.
        /// gain&#8212;returns the gain to scale data (m) in mx+b form.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int FetchIQSingleRecordComplexF64(string Channel_List, Int64 Record_Number, Int64 Number_of_Samples, double Timeout, niComplexNumber[] Data, out niRFSA_wfmInfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchIQSingleRecordComplexF64(this._handle, Channel_List, Record_Number, Number_of_Samples, Timeout, Data, out Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Initiates an acquisition and fetches a single I/Q data record. Do not use this function if you have configured the device to continuously acquire data samples or to acquire multiple records.
        /// 
        /// Supported Devices: NI 5661/5663
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Timeout">
        /// 
        /// timeout
        /// ViReal64
        /// Specifies in seconds the time allotted for the function to complete before returning a timeout error. A value of &#8211;1 specifies the function waits until all data is available.
        /// 
        /// </param>
        /// <param name="Data">
        /// 
        /// data
        /// NIComplexNumber*
        /// Returns the acquired waveform. Allocate an NIComplexNumber array at least as large as numberOfSamples.
        /// 
        /// </param>
        /// <param name="Data_Array_Size">
        /// 
        /// dataArraySize
        /// ViInt64
        /// Specifies the size of the array for the data parameter. The array needs to be at least as large as the number of samples configured in the niRFSA_ConfigureNumberOfSamples function.
        /// 
        /// </param>
        /// <param name="Waveform_Info">
        /// 
        /// wfmInfo*
        /// niRFSA_wfmInfo
        /// Returns additional information about the data array.
        /// 
        /// The following list provides more information about each of these properties:
        /// absolute timestamp&#8212;returns the timestamp, in seconds, of the first fetched sample that is comparable between records and acquisitions.
        /// relative timestamp&#8212;returns a timestamp that corresponds to the difference, in seconds, between the first sample returned and the Reference trigger location.
        /// dt&#8212;returns the time interval between data points in the acquired signal. The I/Q data sampling rate is the reciprocal of this value.
        /// actual samples read&#8212;returns an integer representing the number of samples in the waveform.
        /// offset&#8212;returns the offset to scale data (b) in mx+b form.
        /// gain&#8212;returns the gain to scale data (m) in mx+b form.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int ReadIQSingleRecordComplexF64(string Channel_List, double Timeout, niComplexNumber[] Data, Int64 Data_Array_Size, out niRFSA_wfmInfo Waveform_Info)
        {
            int pInvokeResult = PInvoke.ReadIQSingleRecordComplexF64(this._handle, Channel_List, Timeout, Data, Data_Array_Size, out Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Fetches binary I/Q data from multiple records in an acquisition. Fetching transfers acquired waveform data from device memory to computer memory. The data was acquired to onboard memory previously by the hardware after the acquisition was initiated.
        /// 
        /// This function is not necessary if you use the niRFSA_ReadIQSingleRecordComplexF64 function, as the fetch is performed as part of that function.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Starting_Record">
        /// 
        /// startingRecord
        /// ViInt64
        /// Specifies the first record to retrieve. Record numbers are zero-based.
        /// 
        /// </param>
        /// <param name="Number_of_Records">
        /// 
        /// numberOfRecords
        /// ViInt64
        /// Specifies the number of records to fetch. A value of &#8211;1 specifies that NI-RFSA fetches all records in an acquisition starting with the record specified by startingRecord. Record numbers are zero-based. The default value is &#8211;1.
        /// 
        /// </param>
        /// <param name="Number_of_Samples">
        /// 
        /// numberofSamples
        /// ViInt64
        /// Specifies the number of samples per record.
        /// </param>
        /// <param name="Timeout">
        /// 
        /// timeout
        /// ViReal64
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of &#8211;1 specifies the function waits until all data is available. A value of 0 specifies the function immediately returns available data. The default value is 10.
        /// 
        /// </param>
        /// <param name="Data">
        /// 
        /// data
        /// NIComplexI16*
        /// Returns the acquired waveform for each record fetched. The waveforms are written sequentially in the array. Allocate an array at least as large as numberOfSamples times numberOfRecords for this parameter.
        /// 
        /// </param>
        /// <param name="Waveform_Info">
        /// 
        /// wfmInfo
        /// niRFSA_wfmInfo*
        /// Returns an array of structures containing information about each record fetched. Each structure contains the absolute and relative timestamps, the dt, and the actual number of samples read for the corresponding record.
        /// 
        /// The following list provides more information about each of these properties:
        /// absolute timestamp&#8212;returns the timestamp in seconds of the first fetched sample that is comparable between records and acquisitions.
        /// relative timestamp&#8212;returns a timestamp that corresponds to the difference in seconds between the first sample returned and the Reference trigger location.
        /// dt&#8212;returns the time interval between data points in the acquired signal. The IQ data sampling rate is the reciprocal of this value.
        /// actual samples read&#8212;returns an integer representing the number of samples in the waveform.
        /// offset&#8212;returns the offset to scale data in mx+b form.
        /// gain&#8212;returns the gain to scale data in mx+b form.
        /// Note:Allocate an array of structures at least as large as numberOfRecords for this parameter.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int FetchIQMultiRecordComplexI16(string Channel_List, Int64 Starting_Record, Int64 Number_of_Records, Int64 Number_of_Samples, double Timeout, short[] Data, niRFSA_wfmInfo[] Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchIQMultiRecordComplexI16(this._handle, Channel_List, Starting_Record, Number_of_Records, Number_of_Samples, Timeout, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Fetches I/Q data from multiple records in an acquisition. A fetch transfers acquired waveform data from device memory to computer memory. The data was acquired to onboard memory previously by the hardware after the acquisition was initiated.
        /// 
        /// This function is not necessary if you use the niRFSA_ReadIQSingleRecordComplexF64 function, as the fetch is performed as part of that function.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Starting_Record">
        /// 
        /// startingRecord
        /// ViInt64
        /// Specifies the first record to retrieve. Record numbers are zero-based.
        /// 
        /// </param>
        /// <param name="Number_of_Records">
        /// 
        /// numberOfRecords
        /// ViInt64
        /// Specifies the number of records to fetch. A value of &#8211;1 specifies that NI-RFSA fetches all records in an acquisition starting with the record specified by startingRecord. Record numbers are zero-based.  The default value is &#8211;1.
        /// 
        /// </param>
        /// <param name="Number_of_Samples">
        /// 
        /// numberOfSamples
        /// ViInt64
        /// Specifies the number of samples per record.
        /// 
        /// </param>
        /// <param name="Timeout">
        /// 
        /// timeout
        /// ViReal64
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of &#8211;1 specifies the function waits until all data is available. A value of 0 specifies the function immediately returns available data. The default value is 10.
        /// 
        /// </param>
        /// <param name="Data">
        /// 
        /// data
        /// NIComplexNumber*
        /// Returns the acquired waveform for each record fetched. The waveforms are written sequentially in the array. Allocate an array at least as large as numberOfSamples times numberOfRecords for this parameter.
        /// 
        /// </param>
        /// <param name="Waveform_Info">
        /// 
        /// wfmInfo
        /// niRFSA_wfmInfo*
        /// Returns an array of structures containing information about each record fetched. Each structure contains the absolute and relative timestamps, the dt, and the actual number of samples read for the corresponding record.
        /// 
        /// The following list provides more information about each of these properties:
        /// absolute timestamp&#8212;returns the timestamp in seconds of the first fetched sample that is comparable between records and acquisitions.
        /// relative timestamp&#8212;returns a timestamp that corresponds to the difference in seconds between the first sample returned and the Reference trigger location.
        /// dt&#8212;returns the time interval between data points in the acquired signal. The IQ data sampling rate is the reciprocal of this value.
        /// actual samples read&#8212;returns an integer representing the number of samples in the waveform.
        /// offset&#8212;returns the offset to scale data in mx+b form.
        /// gain&#8212;returns the gain to scale data in mx+b form.
        /// Note:Allocate an array of structures at least as large as numberOfRecords for this parameter.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int FetchIQMultiRecordComplexF64(string Channel_List, Int64 Starting_Record, Int64 Number_of_Records, Int64 Number_of_Samples, double Timeout, niComplexNumber[] Data, niRFSA_wfmInfo[] Waveform_Info)
        {
            int pInvokeResult = PInvoke.FetchIQMultiRecordComplexF64(this._handle, Channel_List, Starting_Record, Number_of_Records, Number_of_Samples, Timeout, Data, Waveform_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Stops an acquisition previously started with the niRFSA_Initiate function. Calling this function is optional, unless you want to stop an acquisition before it is complete or you are continuously acquiring data.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int Abort()
        {
            int pInvokeResult = PInvoke.Abort(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Initiates a spectrum acquisition and returns power spectrum data.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Timeout">
        /// 
        /// timeout
        /// ViReal64
        /// Specifies the time, in seconds, allotted for the function to complete before returning a timeout error. A value of &#8211;1 specifies the function waits until all data is available. The default value is 10.
        /// 
        /// </param>
        /// <param name="Power_Spectrum_Data">
        /// 
        /// powerSpectrumData[]
        /// ViReal64
        /// Returns power spectrum data. Allocate an array as large as dataArraySize.
        /// 
        /// </param>
        /// <param name="Data_Array_Size">
        /// 
        /// dataArraySize
        /// ViInt64
        /// Specifies the size of the array you specify for the powerSpectrumData parameter. Use the niRFSA_GetNumberOfSpectralLines function to obtain the array size to allocate. The array must be at least as large as the number of spectral lines that NI-RFSA computes for the power spectrum.
        /// 
        /// </param>
        /// <param name="Spectrum_Info">
        /// 
        /// spectrumInfo
        /// niRFSA_spectrumInfo*
        /// Returns additional information about the powerSpectrumData array. This information includes the frequency, in hertz (Hz) corresponding to the first element in the array, the frequency increment, in Hz, between adjacent array elements, and the number of spectral lines the function returned.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int ReadPowerSpectrumF64(string Channel_List, double Timeout, double[] Power_Spectrum_Data, int Data_Array_Size, out niRFSA_spectrumInfo Spectrum_Info)
        {
            int pInvokeResult = PInvoke.ReadPowerSpectrumF64(this._handle, Channel_List, Timeout, Power_Spectrum_Data, Data_Array_Size, out Spectrum_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Returns the number of spectral lines that NI-RFSA will compute with the current power spectrum configuration.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Number_of_Spectral_Lines">
        /// 
        /// numberOfSpectralLines
        /// ViInt32*
        /// Returns the value of the NIRFSA_ATTR_NUM_SPECTRAL_LINES attribute.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int GetNumberOfSpectralLines(string Channel_List, out int Number_of_Spectral_Lines)
        {
            int pInvokeResult = PInvoke.GetNumberOfSpectralLines(this._handle, Channel_List, out Number_of_Spectral_Lines);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Checks the status of the acquisition. Use this function to check for any errors that may occur during signal acquisition or to check whether the device has completed the acquisition operation. 
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Is_Done">
        /// 
        /// isDone
        /// ViBoolean*
        /// Returns signal acquisition status.
        /// 
        /// VI_TRUESignal acquisition is complete.
        /// VI_FALSESignal acquisition is not complete.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int CheckAcquisitionStatus(out bool Is_Done)
        {
            ushort Is_DoneAsUShort;
            int pInvokeResult = PInvoke.CheckAcquisitionStatus(this._handle, out Is_DoneAsUShort);
            Is_Done = System.Convert.ToBoolean(Is_DoneAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Resets the device to a default initialization state.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int reset()
        {
            int pInvokeResult = PInvoke.reset(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Commits settings to hardware. Calling this function is optional. Settings are automatically committed to hardware when you call the niRFSA_Initiate function, a read IQ function, or the niRFSA_ReadPowerSpectrumF64 function. 
        /// 
        /// For improved accuracy, add a wait after calling this function and before calling the niRFSA_Initiate function to allow the device additional settling time. Refer to the specifications document that shipped with your device for information about settling time and accuracy.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int Commit()
        {
            int pInvokeResult = PInvoke.Commit(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Performs a self-test on the NI-RFSA device and returns the test result. This function performs a simple series of tests verifying that the NI-RFSA device is powered on and responding.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="testResult">
        /// 
        /// testResult
        /// ViInt16*
        /// Returns the value from the device self-test. A value of 0 means success.
        /// 
        /// </param>
        /// <param name="testMessage">
        /// 
        /// testMessage[]
        /// ViChar
        /// Returns the self-test response string from the NI-RFSA device.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int self_test(out short testResult, System.Text.StringBuilder testMessage)
        {
            int pInvokeResult = PInvoke.self_test(this._handle, out testResult, testMessage);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Self-calibrates the IF digitizer associated with the NI-RFSA device. If self calibration is performed successfully, the new calibration constants are immediately stored in the self calibration area of the digitizer EEPROM. Refer to the NI High-Speed Digitizers Help for more information about how often to self-calibrate. 
        /// 
        /// Call this function to obtain more accurate acquisition results when the environmental conditions change significantly.
        /// 
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int SelfCal()
        {
            int pInvokeResult = PInvoke.SelfCal(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Corrects for temperature variations while acquiring the same signal for extended periods of time in a continuous acquisition. Measurements are affected by changes in temperature. NI-RFSA internally acquires the
        ///             temperature every time you initiate an acquisition. If you are performing a continuous acquisition, National Instruments recommends calling this function once every 10&#160;minutes in a stable temperature environment to periodically update temperature calibration.
        ///             Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int PerformThermalCorrection()
        {
            int pInvokeResult = PInvoke.PerformThermalCorrection(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Returns the number of points acquired that have not been fetched yet.
        /// 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Record_Number">
        /// 
        /// recordNumber
        /// ViInt64
        /// Specifies the record from which to read the backlog. Record numbers are zero-based. The default value is 0.
        /// 
        /// </param>
        /// <param name="Backlog">
        /// 
        /// backlog
        /// ViInt64*
        /// Returns the number of samples available to read for the requested record.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int GetFetchBacklog(string Channel_List, Int64 Record_Number, out Int64 Backlog)
        {
            int pInvokeResult = PInvoke.GetFetchBacklog(this._handle, Channel_List, Record_Number, out Backlog);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Returns the revision numbers of the NI-RFSA instrument driver.
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Instrument_Driver_Revision">
        /// 
        /// driverRev
        /// ViChar[]
        /// Returns the instrument driver software revision numbers in the form of a string. The value of the NIRFSA_ATTR_SPECIFIC_DRIVER_REVISION attribute is returned.
        /// 
        /// You must pass a ViChar array with at least 256&#160;bytes.
        /// 
        /// </param>
        /// <param name="Firmware_Revision">
        /// 
        /// instRev
        /// ViChar[]
        /// Returns the instrument firmware revision numbers in the form of a string. The value of the NIRFSA_ATTR_INSTRUMENT_FIRMWARE_REVISION attribute is returned.
        /// 
        /// You must pass a ViChar array with at least 256&#160;bytes.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int revision_query(System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision)
        {
            int pInvokeResult = PInvoke.revision_query(this._handle, Instrument_Driver_Revision, Firmware_Revision);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Returns information about the power spectrum NI-RFSA computes.
        /// 
        /// Note:The NI Spectral Measurements Toolkit (SMT) requires this information. 
        /// Supported Devices: NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Spectrum_Info">
        /// 
        /// spectrumInfo
        /// SmtSpectrumInfo*
        /// Returns a cluster containing information about the power spectrum NI-RFSA computes that is needed by the SMT.
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int GetSpectralInfoForSMT(out SmtSpectrumInfo Spectrum_Info)
        {
            int pInvokeResult = PInvoke.GetSpectralInfoForSMT(this._handle, out Spectrum_Info);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// 
        /// Returns the IF response, based upon current NI-RFSA settings. The NI 5661/5663 automatically corrects for the IF response when Digital IF Equalization is enabled, which is the default state. If you are using downconverter only mode, you can use information returned from this VI to correct your measurement.
        /// Supported Devices: NI 5600/5601 (downconverter only mode), NI 5661/5663
        /// 
        /// </summary>
        /// <param name="Instrument_Handle">
        /// 
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        /// </param>
        /// <param name="Channel_List">
        /// 
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Use &quot;&quot; (an empty string) or VI_NULL to specify all channels.
        /// 
        /// </param>
        /// <param name="Buffer_Size">
        /// 
        /// bufferSize
        /// ViInt32
        /// Specifies the size of the array you specify for the powerSpectrumData parameter. Use the niRFSA_GetNumberOfSpectralLines function to learn the array size you need to allocate. The array must be at least as large as the number of spectral lines that NI-RFSA computes for the power spectrum.
        /// 
        /// </param>
        /// <param name="Frequencies">
        /// 
        /// frequencies[]
        /// ViReal64
        /// Returns an array containing the frequencies, in hertz (Hz), that correspond to the response data.
        /// 
        /// Pass VI_NULL if you do not want to use this parameter.
        /// 
        /// </param>
        /// <param name="Magnitude_Response">
        /// 
        /// magnitudeResponse[]
        /// ViReal64
        /// Returns an array containing the magnitude IF response, in decibels (dB). The magnitude IF response is normalized to the center frequency at each frequency in the frequencies array.
        /// 
        /// Pass VI_NULL if you do not want to use this parameter.
        /// 
        /// </param>
        /// <param name="Phase_Response">
        /// 
        /// phaseResponse[]
        /// ViReal64
        /// Returns an array containing the phase IF response, in degrees. The phase IF response is normalized to the center frequency at each frequency entry in the frequencies array.
        /// 
        /// Pass VI_NULL if you do not want to use this parameter.
        /// 
        /// </param>
        /// <param name="Number_of_Frequencies">
        /// 
        /// numberOfFrequencies
        /// ViInt32*
        /// Returns the required number of elements in the frequencies array and the response arrays. If bufferSize is 0, this parameter returns the expected array size. The expected array size depends on which NI-RFSA device you use (NI&#160;5661 or NI&#160;5663) and on the current settings (NI&#160;5663 only).
        /// 
        /// </param>
        /// <returns>
        /// 
        /// status
        /// ViStatus
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.
        /// 
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value
        /// Meaning
        /// 0
        /// Success
        /// Positive Values
        /// Warnings
        /// Negative Values
        /// Errors
        /// 
        /// </returns>
        public int GetFrequencyResponse(string Channel_List, int Buffer_Size, double[] Frequencies, double[] Magnitude_Response, double[] Phase_Response, out int Number_of_Frequencies)
        {
            int pInvokeResult = PInvoke.GetFrequencyResponse(this._handle, Channel_List, Buffer_Size, Frequencies, Magnitude_Response, Phase_Response, out Number_of_Frequencies);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies the IF attenuation settings. Call this function for every measurement taken.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = channelList>
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Specify 0 as the value of this parameter.
        /// 
        ///</param>
        ///<param name = iFFilter>
        /// IFFilter
        /// ViInt32
        /// Specifies the IF filter used.
        /// NIRFSA_VAL_187_5_MHZ_WIDE (1400)
        /// Uses the 187.5 MHz wide bandwidth filter.
        /// NIRFSA_VAL_187_5_MHZ_NARROW (1401)
        /// Uses the 187.5 MHz narrow bandwidth filter.
        /// NIRFSA_VAL_53_MHZ (1402)
        /// Uses the 53 MHz filter.
        /// NIRFSA_VAL_BYPASS (1403)
        /// Bypasses the IF filter.
        /// 
        ///</param>
        ///<param name = numberofAttenuators>
        /// numberOfAttenuators
        /// ViInt32
        /// Specifies the number of attenuators to use during the IF attenuation adjustment.
        /// 
        ///</param>
        ///<param name = attenuatorSettings>
        /// attenuatorSettings
        /// ViReal64* 
        /// Specifies the IF attenuator settings for the measurement. The first element in the array corresponds with IF1, the next element corresponds to IF2, and so on.
        /// 
        ///</param>
        ///<param name = measurement>
        /// measurement
        /// ViReal64
        /// Specifies the measurement taken for the current input configuration.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int CalAdjustIFAttenuationCalibration(string channelList, int iFFilter, int numberofAttenuators, double[] attenuatorSettings, double measurement)
        {
            int pInvokeResult = PInvoke.CalAdjustIFAttenuationCalibration(this._handle, channelList, iFFilter, numberofAttenuators, attenuatorSettings, measurement);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Specifies the IF response settings for the driver.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = channelList>
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Specify 0 as the value of this parameter.
        /// 
        ///</param>
        ///<param name = iFFilter>
        /// IFFilter
        /// ViInt32
        /// Specifies the IF filter used.
        /// NIRFSA_VAL_187_5_MHZ_WIDE (1400)
        /// Uses the 187.5 MHz wide bandwidth path.
        /// NIRFSA_VAL_187_5_MHZ_NARROW (1401)
        /// Uses the 187.5 MHz narrow bandwidth path.
        /// NIRFSA_VAL_53_MHZ (1402)
        /// Uses the 53 MHz path.
        /// NIRFSA_VAL_BYPASS (1403)
        /// Bypasses the IF path.
        /// 
        ///</param>
        ///<param name = rFFrequency>
        /// RFFrequency
        /// ViReal64
        /// Specifies the RF frequency used during the IF response adjustment.
        /// 
        ///</param>
        ///<param name = bandwidth>
        /// bandwidth
        /// ViReal64
        /// Specifies the bandwidth to use for the IF response adjustment.
        /// 
        ///</param>
        ///<param name = numberofMeasurements>
        /// numberOfMeasurements
        /// ViInt32
        /// Specifies the number of measurements to make.
        /// 
        ///</param>
        ///<param name = measurements>
        /// measurements
        /// ViReal64*
        /// Specifies the relevant measurements taken for each IF filter configuration.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int CalAdjustIFResponseCalibration(string channelList, int iFFilter, double rFFrequency, double bandwidth, int numberofMeasurements, double[] measurements)
        {
            int pInvokeResult = PInvoke.CalAdjustIFResponseCalibration(this._handle, channelList, iFFilter, rFFrequency, bandwidth, numberofMeasurements, measurements);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Write the reference level settings to the driver. The reference level calibration data is split into either the default configuration data or the mechanical relay disabled configuration data.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = channelList>
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Specify 0 as the value of this parameter.
        /// 
        ///</param>
        ///<param name = referenceLevelCalDataType>
        /// referenceLevelCalDataType
        /// ViInt32
        /// Specifies whether the reference level calibration data being used is the default configuration data or the mechanical relay disabled configuration data.
        /// NIRFSA_VAL_EXT_CAL_DEFAULT (1800)
        /// The data is the default configuration data.
        /// NIRFSA_VAL_EXT_CAL_MECHANICAL_ATTENUATOR_DISABLED (1801)
        /// The data is the configuration data when the mechanical relay is disabled. Use this option to save uncalibrated measurements for more advanced operation.
        /// 
        ///</param>
        ///<param name = rFBand>
        /// RFBand
        /// ViInt32
        /// Specifies the RF band used during the reference level calibration.
        /// NIRFSA_VAL_RF_BAND_1
        /// The RF band 1 path is used.
        /// NIRFSA_VAL_RF_BAND_2
        /// The RF band 2 path is used.
        /// NIRFSA_VAL_RF_BAND_3
        /// The RF band 3 path is used.
        /// NIRFSA_VAL_RF_BAND_4
        /// The RF band 4 path is used.
        /// 
        ///</param>
        ///<param name = attenuatorTableNumber>
        /// attenuatorTableNumber
        /// ViInt32
        /// Specifies which attenuation table you are using. Valid values are 0 to 2.
        /// 
        ///</param>
        ///<param name = frequency>
        /// frequency
        /// ViReal64
        /// Specifies the frequency for the reference level adjustment.
        /// 
        ///</param>
        ///<param name = measurement>
        /// measurement
        /// ViReal64
        /// Specifies the relevant measurement taken for the current configuration.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int CalAdjustRefLevelCalibration(string channelList, int referenceLevelCalDataType, int rFBand, int attenuatorTableNumber, double frequency, double measurement)
        {
            int pInvokeResult = PInvoke.CalAdjustRefLevelCalibration(this._handle, channelList, referenceLevelCalDataType, rFBand, attenuatorTableNumber, frequency, measurement);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Writes the calibration temperature to the driver. 
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = temperaturedegreesC>
        /// temperature
        /// ViReal64
        /// Specifies the calibration temperature.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int CalSetTemperature(double temperaturedegreesC)
        {
            int pInvokeResult = PInvoke.CalSetTemperature(this._handle, temperaturedegreesC);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Changes the password that is required to initialize an external calibration session.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = oldpassword>
        /// ViConstString
        /// oldPassword
        /// Specifies the previous password used to protect the calibration values.
        /// 
        ///</param>
        ///<param name = newpassword>
        /// ViConstString
        /// newPassword
        /// Specifies the new password to use to protect the calibration values.
        /// The maximum length of the password varies by device. The NI&nbsp;5601 can have a password that is up to 10 characters long.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ChangeExtCalPassword(string oldpassword, string newpassword)
        {
            int pInvokeResult = PInvoke.ChangeExtCalPassword(this._handle, oldpassword, newpassword);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Closes an EEPROM-specific calibration step. This step ensures that all the measurements required for this particular calibration step haven been recorded by the user via the adjust function calls. 
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<returns>
        /// Closes an EEPROM-specific calibration step. This step ensures that all the measurements required for this particular calibration step haven been recorded by the user via the adjust function calls. 
        /// Supported Devices: NI 5601
        /// 
        ///</returns>
        public int CloseCalibrationStep()
        {
            int pInvokeResult = PInvoke.CloseCalibrationStep(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Closes an NI-RFSA external calibration session and, if specified, stores the new calibration constants and calibration data, such as time, in the onboard EEPROM.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = action>
        /// action
        /// ViInt32
        /// Specifies how to use the calibration values from this session as the session is closed.
        /// Defined Values:
        ///           NIRFSA_VAL_EXT_CAL_ABORT               The old calibration constants are kept, and the new ones are discarded.
        /// NIRFSA_VAL_EXT_CAL_COMMITThe new calibration constants are stored in the EEPROM.
        /// 
        ///</param>
        ///<returns>
        /// Closes an NI-RFSA external calibration session and, if specified, stores the new calibration constants and calibration data, such as time, in the onboard EEPROM.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        ///</returns>
        public int CloseExtCal(int action)
        {
            int pInvokeResult = PInvoke.CloseExtCal(this._handle, action);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates an empty RF configuration list for RF list mode. An RF configuration list is composed of configuration list steps. Each step specifies the state of the instrument by specifying values for attributes/properties.
        /// After a configuration list is created, the list is enabled using the NIRFSA_ATTR_ACTIVE_CONFIGURATION_LIST attribute or by setting setAsActiveList to  VI_TRUE. Call the niRFSA_CreateConfigurationListStep function to add steps to the configuration list.
        /// Supported Devices: NI 5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<param name = listName>
        /// listName
        /// ViConstString
        /// Specifies the name of the configuration list. This string  may not contain spaces, special characters, or punctuation marks.
        /// 
        ///</param>
        ///<param name = numberOfListAttributes>
        /// numberOfListAttributes
        /// ViInt32
        /// Specifies the number of list attributes to set.
        /// 
        ///</param>
        ///<param name = listAttributeIDs>
        /// listAttributeIDs
        /// ViAttr[]
        /// Specifies the configuration list attributes that will be set.
        /// You can include the following properties in your configuration list:
        /// NIRFSA_ATTR_IQ_CARRIER_FREQUENCY
        /// NIRFSA_ATTR_REFERENCE_LEVEL
        /// NIRFSA_ATTR_DOWNCONVERTER_CENTER_FREQUENCY
        /// NIRFSA_ATTR_IQ_POWER_EDGE_REF_TRIGGER_LEVEL
        /// NIRFSA_ATTR_TIMER_EVENT_INTERVAL
        /// NIRFSA_ATTR_FREQUENCY_SETTLING
        /// 
        ///</param>
        ///<param name = setAsActiveList>
        /// setAsActiveList
        /// ViBoolean
        /// Sets the current list as the active configuration list when this parameter is set to TRUE. 
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int CreateConfigurationList(string listName, int numberOfListAttributes, niRFSAProperties[] listAttributeIDs, bool setAsActiveList)
        {
            int pInvokeResult = PInvoke.CreateConfigurationList(this._handle, listName, numberOfListAttributes, listAttributeIDs, setAsActiveList);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates a new configuration list step in the configuration list for RF list mode specified by the NIRFSA_ATTR_ACTIVE_CONFIGURATION_LIST attribute. When you create a configuration list step, a new instance of each attribute specified by the configuration list is created. Configuration list attributes are specified when a configuration list is created.  
        /// Supported Devices: NI 5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<param name = setAsActiveStep>
        /// setAsActiveStep
        /// ViBoolean
        /// Sets this step as the active step for the active configuration list. If you do not set this parameter, use the NIRFSA_ATTR_ACTIVE_CONFIGURATION_LIST_STEP attribute to set the active step.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int CreateConfigurationListStep(bool setAsActiveStep)
        {
            int pInvokeResult = PInvoke.CreateConfigurationListStep(this._handle, setAsActiveStep);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Deletes a previously created configuration list and all the configuration list steps in the RF list mode configuration list. When a configuration list step is deleted, all the instances of the properties associated with the configuration list step are also removed.
        /// Supported Devices: NI 5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<param name = listName>
        /// listName
        /// ViConstString
        /// Specifies the name of the configuration list. This string  may not contain spaces, special characters, or punctuation marks.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int DeleteConfigurationList(string listName)
        {
            int pInvokeResult = PInvoke.DeleteConfigurationList(this._handle, listName);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns user-defined information from the onboard EEPROM.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = userdefinedinfo>
        /// info
        /// ViChar[]
        /// Returns the user-defined information stored in the device onboard EEPROM.
        /// 
        ///</param>
        ///<returns>
        /// Returns user-defined information from the onboard EEPROM.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        ///</returns>
        public int GetCalUserDefinedInfo(StringBuilder userdefinedinfo)
        {
            int pInvokeResult = PInvoke.GetCalUserDefinedInfo(this._handle, userdefinedinfo);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns user-defined information from the onboard EEPROM. The size of the cal user defined information is 21 characters.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = infoSize>
        /// infoSize
        /// ViInt32* 
        /// Returns the number of characters of user-defined information that can be stored in the device onboard EEPROM.
        /// 
        ///</param>
        ///<returns>
        /// Returns user-defined information from the onboard EEPROM. The size of the cal user defined information is 21 characters.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        ///</returns>
        public int GetCalUserDefinedInfoMaxSize(out int infoSize)
        {
            int pInvokeResult = PInvoke.GetCalUserDefinedInfoMaxSize(this._handle, out infoSize);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the date and time of the last successful external calibration. The time returned is 24-hour local time; for example, if the device was calibrated at 2:30 PM, this function returns 14 for the hours parameter and 30 for the minutes parameter.
        /// Supported Devices: NI 5600/5601 and NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = year>
        /// year
        /// ViInt32*
        /// Returns the year of the last external calibration. It is expressed as an integer.
        /// 
        ///</param>
        ///<param name = month>
        /// month
        /// ViInt32*
        /// Returns the month of the last external calibration. It is expressed as an integer. For example, December is represented as 12.
        /// 
        ///</param>
        ///<param name = day>
        /// day
        /// ViInt32*
        /// Returns the day of the last external calibration. It is expressed as an integer.
        /// 
        ///</param>
        ///<param name = hour>
        /// hour
        /// ViInt32*
        /// Returns the year of the last external calibration. It is expressed as an integer.
        /// 
        ///</param>
        ///<param name = minute>
        /// minute
        /// ViInt32*
        /// Returns the minute of the last external calibration. It is expressed as an integer.
        /// 
        ///</param>
        ///<returns>
        /// Returns the date and time of the last successful external calibration. The time returned is 24-hour local time; for example, if the device was calibrated at 2:30 PM, this function returns 14 for the hours parameter and 30 for the minutes parameter.
        /// Supported Devices: NI 5600/5601 and NI 5661/5663/5663E
        /// 
        ///</returns>
        public int GetExtCalLastDateAndTime(out int year, out int month, out int day, out int hour, out int minute)
        {
            int pInvokeResult = PInvoke.GetExtCalLastDateAndTime(this._handle, out year, out month, out day, out hour, out minute);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the temperature recorded at the last successful external calibration. The temperature is returned in degrees Celsius.
        /// Supported Devices: NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = temperature>
        /// temperature
        /// ViReal64*
        /// Returns the temperature of the last external calibration. It is expressed in degrees Celsius.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int GetExtCalLastTemp(out double temperature)
        {
            int pInvokeResult = PInvoke.GetExtCalLastTemp(this._handle, out temperature);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns the recommended interval between external calibrations in months.
        /// Supported Devices: NI 5600/5601 and NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = months>
        /// months
        /// ViInt32*
        /// Specifies the recommended maximum interval between external calibrations in months.
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int GetExtCalRecommendedInterval(out int months)
        {
            int pInvokeResult = PInvoke.GetExtCalRecommendedInterval(this._handle, out months);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns coefficients for the specified channel list that you can use to convert binary data to normalized and calibrated data.
        /// The raw data of an acquisition may not perfectly map to the digitizer vertical range. The difference is due to the digitizer's internal hardware settings being set so that NI-RFSA can handle issues such as range overflow. This function returns niRFSA_coefficientInfo structures in the coefficientInfo array that provide gain and offset values that you can use normalize the data. The coefficientInfo array returns one element for each channel specified in the channelList parameter. The element order matches the order specified by the channelList parameter.
        /// To get data that maps to the digitizer vertical range, normalize the raw data from an acquisition by multiplying it by the gain value of the appropriate coefficientInfo element, then adding the offset value from the same element.
        /// Note&nbsp;&nbsp;The coefficients are calculated by NI-RFSA for the current configuration of the device, so they are only valid for acquisitions obtained with the same device configuration.
        /// After applying the gain and offset, the normalized data is such that:
        /// The maximum possible positive binary value maps to the maximum positive value of the NIRFSA_ATTR_DIGITIZER_VERTICAL_RANGE attribute. 
        /// The maximum possible negative binary value maps to the maximum negative value of the NIRFSA_ATTR_DIGITIZER_VERTICAL_RANGE attribute. 
        /// The value of the NIRFSA_ATTR_DIGITIZER_VERTICAL_RANGE attribute. is divided evenly across the possible binary values.
        /// To get the required size of the array, call this function with bufferSize set to 0 and NULL for the coefficientInfo array. This function returns the required size in the numberOfCoefficientSets parameter.
        /// Supported Devices: NI 5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<param name = channelList>
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Specify 0 as the value of this parameter.
        /// 
        ///</param>
        ///<param name = bufferSize>
        /// bufferSize
        /// ViInt32
        /// Specifies the size of the array you specify for the coefficientInfo parameter.
        /// 
        ///</param>
        ///<param name = coefficientInfo>
        /// coefficientInfo
        /// niRFSA_coefficientInfo[]
        /// Specifies the array for storing the coefficient info.
        /// offset is the number that should be added to the data from a peer-to-peer stream after the gain has been applied if you want to scale unscaled data.
        /// gain returns the multiplier that you should use to scale data obtained from a peer-to-peer stream.
        /// 
        ///</param>
        ///<param name = numberofCoefficientSets>
        /// numberOfCoefficientSets
        /// ViInt32*
        /// Returns the number of valid coefficient sets. 
        /// 
        ///</param>
        ///<returns>
        /// Returns coefficients for the specified channel list that you can use to convert binary data to normalized and calibrated data.
        /// The raw data of an acquisition may not perfectly map to the digitizer vertical range. The difference is due to the digitizer's internal hardware settings being set so that NI-RFSA can handle issues such as range overflow. This function returns niRFSA_coefficientInfo structures in the coefficientInfo array that provide gain and offset values that you can use normalize the data. The coefficientInfo array returns one element for each channel specified in the channelList parameter. The element order matches the order specified by the channelList parameter.
        /// To get data that maps to the digitizer vertical range, normalize the raw data from an acquisition by multiplying it by the gain value of the appropriate coefficientInfo element, then adding the offset value from the same element.
        /// Note&nbsp;&nbsp;The coefficients are calculated by NI-RFSA for the current configuration of the device, so they are only valid for acquisitions obtained with the same device configuration.
        /// After applying the gain and offset, the normalized data is such that:
        /// The maximum possible positive binary value maps to the maximum positive value of the NIRFSA_ATTR_DIGITIZER_VERTICAL_RANGE attribute. 
        /// The maximum possible negative binary value maps to the maximum negative value of the NIRFSA_ATTR_DIGITIZER_VERTICAL_RANGE attribute. 
        /// The value of the NIRFSA_ATTR_DIGITIZER_VERTICAL_RANGE attribute. is divided evenly across the possible binary values.
        /// To get the required size of the array, call this function with bufferSize set to 0 and NULL for the coefficientInfo array. This function returns the required size in the numberOfCoefficientSets parameter.
        /// Supported Devices: NI 5663/5663E
        /// 
        ///</returns>
        public int GetNormalizationCoefficients(string channelList, int bufferSize, out niRFSA_coefficientInfo coefficientInfo, out int numberofCoefficientSets)
        {
            int pInvokeResult = PInvoke.GetNormalizationCoefficients(this._handle, channelList, bufferSize, out coefficientInfo, out numberofCoefficientSets);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns coefficients for the specified channel list that you can use to convert unscaled data to scaled I/Q data.
        /// Data from an acquisition may be unscaled if sent by a peer-to-peer stream or fetched as unscaled data. You can use this function to obtain niRFSA_coefficientInfo structures in the coefficientInfo array that provide gain and offset values you can use scale this data into the actual I/Q values. The coefficientInfo array returns one element for each channel specified in the channelList parameter. The element order matches the order specified by the channelList parameter. To get the actual I/Q values, scale the unscaled data from an acquisition by multiplying it by the gain value of the appropriate coefficientInfo element then adding the offset from the same element.
        /// Note&nbsp;&nbsp;The coefficients are calculated by NI-RFSA for the current configuration of the device, so they are only valid for acquisitions obtained with the same device configuration.
        /// To get the required size of the array, call this function with bufferSize set to 0 and NULL for the coefficientInfo array. This function returns the required size in the numberOfCoefficientSets parameter.
        /// Supported Devices: NI 5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<param name = channelList>
        /// channelList
        /// ViConstString
        /// Identifies which channels to apply settings. Specify 0 as the value of this parameter.
        /// 
        ///</param>
        ///<param name = arraySize>
        /// arraySize
        /// ViInt32
        /// Specifies the size of the array you specify for the coefficientInfo parameter.
        /// 
        ///</param>
        ///<param name = coefficientInfo>
        /// coefficientInfo
        /// niRFSA_coefficientInfo[]
        /// Specifies the array for storing the coefficient info.
        /// offset is the number that should be added to the data from a peer-to-peer stream after the gain has been applied if you want to scale unscaled data.
        /// gain returns the multiplier that you should use to scale data obtained from a peer-to-peer stream.
        /// 
        ///</param>
        ///<param name = numberOfCoefficientSets>
        /// numberOfCoefficientSets
        /// ViInt32*
        /// Returns the number of valid coefficient sets. 
        /// 
        ///</param>
        ///<returns>
        /// Returns coefficients for the specified channel list that you can use to convert unscaled data to scaled I/Q data.
        /// Data from an acquisition may be unscaled if sent by a peer-to-peer stream or fetched as unscaled data. You can use this function to obtain niRFSA_coefficientInfo structures in the coefficientInfo array that provide gain and offset values you can use scale this data into the actual I/Q values. The coefficientInfo array returns one element for each channel specified in the channelList parameter. The element order matches the order specified by the channelList parameter. To get the actual I/Q values, scale the unscaled data from an acquisition by multiplying it by the gain value of the appropriate coefficientInfo element then adding the offset from the same element.
        /// Note&nbsp;&nbsp;The coefficients are calculated by NI-RFSA for the current configuration of the device, so they are only valid for acquisitions obtained with the same device configuration.
        /// To get the required size of the array, call this function with bufferSize set to 0 and NULL for the coefficientInfo array. This function returns the required size in the numberOfCoefficientSets parameter.
        /// Supported Devices: NI 5663/5663E
        /// 
        ///</returns>
        public int GetScalingCoefficients(string channelList, int arraySize, out niRFSA_coefficientInfo coefficientInfo, out int numberOfCoefficientSets)
        {
            int pInvokeResult = PInvoke.GetScalingCoefficients(this._handle, channelList, arraySize, out coefficientInfo, out numberOfCoefficientSets);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Returns a writer endpoint handle that can be used with NI-P2P to configure a peer-to-peer stream with the digitizer as an endpoint.
        /// Supported Devices: NI&nbsp;5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<param name = streamEndpoint>
        /// streamEndpoint
        /// ViConstString
        /// Specifies the name of the stream resources you want to use.
        /// 
        ///</param>
        ///<param name = writerHandle>
        /// writerHandle
        /// ViUInt32*
        /// Returns the writer endpoint handle which will be used with NI-P2P API to create a stream with the digitizer as an endpoint.
        /// 
        ///</param>
        ///<returns>
        /// Returns a writer endpoint handle that can be used with NI-P2P to configure a peer-to-peer stream with the digitizer as an endpoint.
        /// Supported Devices: NI&nbsp;5663/5663E
        /// 
        ///</returns>
        public int GetStreamEndpointHandle(string streamEndpoint, out uint writerHandle)
        {
            int pInvokeResult = PInvoke.GetStreamEndpointHandle(this._handle, streamEndpoint, out writerHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Creates and initializes a special NI-RFSA external calibration session. The instrumentHandle returned is an NI-RFSA session that can be used to configure the device using normal attributes and functions. However, flags have been set that allow you to program an external calibration procedure using the special calibration attributes and functions.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = resourceName>
        /// resourceName
        /// ViRsrc
        /// Specifies the resource name of the device to initialize.
        /// Example #
        /// Device Type
        /// Syntax
        /// 1
        /// myDAQmxDevice
        /// NI-DAQmx device, device name =
        ///                &#34;myDAQmxDevice&#34;
        /// 2
        /// myLogicalName
        /// IVI logical name, name =
        ///                   &#34;myLogicalName&#34;
        /// For NI-DAQmx devices, the syntax is the device name specified in MAX, as shown in
        ///             Example 1. Typical default names for NI-DAQmx devices in MAX are Dev1 or PXI1Slot2. You
        ///             can rename an NI-DAQmx device by right-clicking on the name in MAX and entering a new
        ///             name. You also can pass in the name of an IVI logical name configured with the IVI
        ///             Configuration utility. For additional information, refer to the Installed Devices&#187;IVI topic of the
        ///             Measurement &#38; Automation Explorer Help.
        /// Caution&#160;&#160;NI-DAQmx device names are not case-sensitive. However, IVI logical names are case-sensitive. If you use an IVI logical name, verify the name is identical to the name shown in the IVI Configuration Utility.
        /// </td>
        ///  </tr></table>
        /// 
        ///</param>
        ///<param name = password>
        /// password
        /// ViConstString
        /// Specifies the password for opening a calibration session. The initial password is factory configured to "NI". password can be a maximum of ten alphanumeric characters.
        /// 
        ///</param>
        ///<param name = optionstring>
        /// optionString
        /// ViConstString
        /// Sets the initial value of certain options for the session.
        /// The following options are used in this parameter.
        /// calAction:create&#08211;Use this option when starting a calibration step for the first time.
        /// calAction:append&#08211;Use this option when appending to existing calibration data.
        /// 
        ///</param>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session.
        /// 
        ///</param>
        ///<returns>
        /// Creates and initializes a special NI-RFSA external calibration session. The instrumentHandle returned is an NI-RFSA session that can be used to configure the device using normal attributes and functions. However, flags have been set that allow you to program an external calibration procedure using the special calibration attributes and functions.
        /// Supported Devices: NI 5601
        /// 
        ///</returns>
        public int InitExtCal(string resourceName, string password, string optionstring, out HandleRef instrumentHandle)
        {
            IntPtr handle;
            int pInvokeResult = PInvoke.InitExtCal(resourceName, password, optionstring, out handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            instrumentHandle = new HandleRef(this, handle);
            return pInvokeResult;
        }

        /// <summary>
        /// Initializes an EEPROM-specific calibration step.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = calibrationstep>
        /// calibrationStep
        /// ViInt32
        /// Specifies the EEPROM-specific calibration step to initialize.
        /// Defined Values:
        /// NIRFSA_VAL_EXT_CAL_IF_ATTENUATION_CALIBRATION
        /// Initializes the IF attenuation calibration step.
        /// NIRFSA_VAL_EXT_CAL_IF_RESPONSE_CALIBRATION
        /// Initializes the IF response calibration step.
        /// NIRFSA_VAL_EXT_CAL_REF_LEVEL_CALIBRATION
        /// Initializes the reference level calibration step.
        /// 
        ///</param>
        ///<returns>
        /// Initializes an EEPROM-specific calibration step.
        /// Supported Devices: NI 5601
        /// 
        ///</returns>
        public int InitializeCalibrationStep(int calibrationstep)
        {
            int pInvokeResult = PInvoke.InitializeCalibrationStep(this._handle, calibrationstep);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Performs a hard reset on the device which consists of the following actions:
        /// Signal acquisition is stopped
        /// All routes are released
        /// External bidirectional terminals are tristated
        /// FPGAs are reset
        /// Hardware is configured to its default state
        /// All session attributes are reset to their default states
        /// During a device reset, routes of signals between this and other devices are released, regardless of which device created the route.
        /// On the NI 5600, if you are using PXI_CLK10, you continue to drive the clock even after a device reset.
        /// Supported Devices: NI 5600/5601 (external digitizer mode), NI 5661/5663/5663E
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_initWithOptions function and identifies a
        ///             particular instrument session.  
        /// 
        ///</param>
        ///<returns>	
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
        public int ResetDevice()
        {
            int pInvokeResult = PInvoke.ResetDevice(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// niRFSA_SetCalUserDefinedInfo
        /// ViStatus _VI_FUNC niRFSA_SetCalUserDefinedInfo(ViSession vi, 
        ///    ViConstString info);
        /// Purpose
        /// Stores user-defined information in the onboard EEPROM.
        /// Supported Devices: NI 5601
        /// 
        /// </summary>
        ///<param name = instrumentHandle>
        /// vi
        /// ViSession
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param name = userdefinedinfo>
        /// info
        /// ViConstString
        /// Specifies the string to store in the device onboard EEPROM. This string can be up to 21 characters long.
        /// 
        ///</param>
        ///<returns>
        /// niRFSA_SetCalUserDefinedInfo
        /// ViStatus _VI_FUNC niRFSA_SetCalUserDefinedInfo(ViSession vi, 
        ///    ViConstString info);
        /// Purpose
        /// Stores user-defined information in the onboard EEPROM.
        /// Supported Devices: NI 5601
        /// 
        ///</returns>
        public int SetCalUserDefinedInfo(string userdefinedinfo)
        {
            int pInvokeResult = PInvoke.SetCalUserDefinedInfo(this._handle, userdefinedinfo);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
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
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. Examine the status code from each call to an NI-RFSA function to determine if an error has occurred.	
        /// To obtain a text description of the status code and additional information about the error condition, call the niRFSA_GetError function. To clear the error information from NI-RFSA, call the niRFSA_ClearError function.	
        /// The general meaning of the status code is as follows:	
        /// Value           Meaning	
        /// 0               Success	
        /// Positive Values Warnings	
        /// Negative Values Errors	
        ///	
        /// </returns>
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
        /// Converts a status code returned by an NI-RFSA function into a user-readable string.
        /// 
        /// </summary>
        ///<param>
        /// Identifies your instrument session. vi is obtained from the niRFSA_init or niRFSA_InitExtCal function and identifies a particular instrument session.
        /// 
        ///</param>
        ///<param>
        /// Passes the Status parameter that is returned from any NI-RFSA function. The default value is 0 (VI_SUCCESS).
        /// 
        ///</param>
        ///<param>
        /// Returns the user-readable message string that corresponds to the status code you specify.
        /// 
        ///</param>
        ///<returns>
        /// Converts a status code returned by an NI-RFSA function into a user-readable string.
        /// 
        ///</returns>
        public static int ErrorMessage(HandleRef handle, int code, StringBuilder msg)
        {
            int pInvokeResult = 0;
            int size = 256;
            msg.Capacity = size;
            pInvokeResult = PInvoke.error_message(handle, code, msg);
            PInvoke.TestForError(handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Closes the rfsa session and releases resources associated with that session. 
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Closes the rfsa session and releases resources associated with that session. 
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                //Dispose unmanaged resources
                PInvoke.close(this._handle);
                // Note disposing has been done.
                _disposed = true;
            }
        }

        public void SetInt32(niRFSAProperties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        public void SetInt32(niRFSAProperties propertyId, int val)
        {
            this.SetInt32(propertyId, "", val);
        }

        public int GetInt32(niRFSAProperties propertyId, string repeatedCapabilityOrChannel)
        {
            int val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, propertyId, out val));
            return val;
        }

        public int GetInt32(niRFSAProperties propertyId)
        {
            return this.GetInt32(propertyId, "");
        }

        public void SetDouble(niRFSAProperties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        public void SetDouble(niRFSAProperties propertyId, double val)
        {
            this.SetDouble(propertyId, "", val);
        }

        public double GetDouble(niRFSAProperties propertyId, string repeatedCapabilityOrChannel)
        {
            double val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, propertyId, out val));
            return val;
        }

        public double GetDouble(niRFSAProperties propertyId)
        {
            return this.GetDouble(propertyId, "");
        }

        public void SetBoolean(niRFSAProperties propertyId, string repeatedCapabilityOrChannel, bool val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, propertyId, System.Convert.ToUInt16(val)));
        }

        public void SetBoolean(niRFSAProperties propertyId, bool val)
        {
            this.SetBoolean(propertyId, "", val);
        }

        public bool GetBoolean(niRFSAProperties propertyId, string repeatedCapabilityOrChannel)
        {
            ushort val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, propertyId, out val));
            return System.Convert.ToBoolean(val);
        }

        public bool GetBoolean(niRFSAProperties propertyId)
        {
            return this.GetBoolean(propertyId, "");
        }

        public void SetString(niRFSAProperties propertyId, string repeatedCapabilityOrChannel, string val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViString(this._handle, repeatedCapabilityOrChannel, propertyId, val));
        }

        public void SetString(niRFSAProperties propertyId, string val)
        {
            this.SetString(propertyId, "", val);
        }

        public string GetString(niRFSAProperties propertyId, string repeatedCapabilityOrChannel)
        {
            System.Text.StringBuilder newVal = new System.Text.StringBuilder(512);
            int size = PInvoke.GetAttributeViString(this._handle, repeatedCapabilityOrChannel, propertyId, 512, newVal);
            if ((size < 0))
            {
                PInvoke.ThrowError(this._handle, size);
            }
            else
            {
                if ((size > 0))
                {
                    newVal.Capacity = size;
                    PInvoke.TestForError(this._handle, PInvoke.GetAttributeViString(this._handle, repeatedCapabilityOrChannel, propertyId, size, newVal));
                }
            }
            return newVal.ToString();
        }

        public string GetString(niRFSAProperties propertyId)
        {
            return this.GetString(propertyId, "");
        }

        public int GetInt64(string Channel_Name, niRFSAProperties Attribute_ID, out Int64 Attribute_Value)
        {
            int pInvokeResult = PInvoke.GetAttributeViInt64(this._handle, Channel_Name, Attribute_ID, out Attribute_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public int GetInt64(niRFSAProperties Attribute_ID, out Int64 Attribute_Value)
        {
            return GetInt64(String.Empty, Attribute_ID, out Attribute_Value);
        }

        public int GetSession(string Channel_Name, niRFSAProperties Attribute_ID, out System.IntPtr Attribute_Value)
        {
            int pInvokeResult = PInvoke.GetAttributeViSession(this._handle, Channel_Name, Attribute_ID, out Attribute_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public int GetSession(niRFSAProperties Attribute_ID, out System.IntPtr Attribute_Value)
        {
            return GetSession(String.Empty, Attribute_ID, out Attribute_Value);
        }

        public int SetInt64(string Channel_Name, niRFSAProperties Attribute_ID, Int64 Attribute_Value)
        {
            int pInvokeResult = PInvoke.SetAttributeViInt64(this._handle, Channel_Name, Attribute_ID, Attribute_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public int SetInt64(niRFSAProperties Attribute_ID, Int64 Attribute_Value)
        {
            return SetInt64(String.Empty, Attribute_ID, Attribute_Value);
        }

        public int SetSession(string Channel_Name, niRFSAProperties Attribute_ID, System.IntPtr Attribute_Value)
        {
            int pInvokeResult = PInvoke.SetAttributeViSession(this._handle, Channel_Name, Attribute_ID, Attribute_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public int SetSession(niRFSAProperties Attribute_ID, System.IntPtr Attribute_Value)
        {
            return SetSession(String.Empty, Attribute_ID, Attribute_Value);
        }

        private class PInvoke
        {
            [DllImport(rfsaModule, EntryPoint = "niRFSA_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init(string Resource_Name, ushort ID_Query, ushort Reset, out System.IntPtr Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions(string Resource_Name, ushort ID_Query, ushort Reset, string Option_String, out System.IntPtr Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureAcquisitionType", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureAcquisitionType(HandleRef Instrument_Handle, int Acquisition_Type);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureReferenceLevel", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureReferenceLevel(HandleRef Instrument_Handle, string Channel_List, double Reference_Level);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureIQCarrierFrequency", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureIQCarrierFrequency(HandleRef Instrument_Handle, string Channel_List, double Carrier_Frequency);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureIQRate", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureIQRate(HandleRef Instrument_Handle, string Channel_List, double IQ_Rate);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureNumberOfSamples", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureNumberOfSamples(HandleRef Instrument_Handle, string Channel_List, ushort Number_of_Samples_Is_Finite, Int64 Samples_Per_Record);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureNumberOfRecords", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureNumberOfRecords(HandleRef Instrument_Handle, string Channel_List, ushort Number_of_Records_Is_Finite, Int64 Number_of_Records);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureSpectrumFrequencyCenterSpan", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSpectrumFrequencyCenterSpan(HandleRef Instrument_Handle, string Channel_List, double Center_Frequency, double Span);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureSpectrumFrequencyStartStop", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSpectrumFrequencyStartStop(HandleRef Instrument_Handle, string Channel_List, double Start_Frequency, double Stop_Frequency);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureResolutionBandwidth", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureResolutionBandwidth(HandleRef Instrument_Handle, string Channel_List, double Resolution_Bandwidth);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureDigitalEdgeStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeStartTrigger(HandleRef Instrument_Handle, string Source, int Edge);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureSoftwareEdgeStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareEdgeStartTrigger(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_DisableStartTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableStartTrigger(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureDigitalEdgeRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeRefTrigger(HandleRef Instrument_Handle, string Source, int Edge, Int64 Pretrigger_Samples);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureIQPowerEdgeRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureIQPowerEdgeRefTrigger(HandleRef Instrument_Handle, string Source, double Level, int Slope, Int64 Pretrigger_Samples);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureSoftwareEdgeRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareEdgeRefTrigger(HandleRef Instrument_Handle, Int64 Pretrigger_Samples);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_DisableRefTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableRefTrigger(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureDigitalEdgeAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureDigitalEdgeAdvanceTrigger(HandleRef Instrument_Handle, string Source, int Edge);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureSoftwareEdgeAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSoftwareEdgeAdvanceTrigger(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_DisableAdvanceTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int DisableAdvanceTrigger(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SendSoftwareEdgeTrigger", CallingConvention = CallingConvention.StdCall)]
            public static extern int SendSoftwareEdgeTrigger(HandleRef Instrument_Handle, int Trigger, string Trigger_Identifier);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ExportSignal", CallingConvention = CallingConvention.StdCall)]
            public static extern int ExportSignal(HandleRef Instrument_Handle, int Signal, string Signal_Identifier, string Output_Terminal);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigureRefClock", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureRefClock(HandleRef Instrument_Handle, string Ref_Clock_Source, double Ref_Clock_Rate);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ConfigurePXIChassisClk10", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigurePXIChassisClk10(HandleRef Instrument_Handle, string PXI_Clk_10_Source);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ResetAttribute", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetAttribute(HandleRef Instrument_Handle, string Channel_Name, int Attribute_ID);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_Initiate", CallingConvention = CallingConvention.StdCall)]
            public static extern int Initiate(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_FetchIQSingleRecordComplexI16", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchIQSingleRecordComplexI16(HandleRef Instrument_Handle, string Channel_List, Int64 Record_Number, Int64 Number_of_Samples, double Timeout, [Out] short[] Data, out niRFSA_wfmInfo Waveform_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_FetchIQSingleRecordComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchIQSingleRecordComplexF64(HandleRef Instrument_Handle, string Channel_List, Int64 Record_Number, Int64 Number_of_Samples, double Timeout, [Out] niComplexNumber[] Data, out niRFSA_wfmInfo Waveform_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ReadIQSingleRecordComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadIQSingleRecordComplexF64(HandleRef Instrument_Handle, string Channel_List, double Timeout, [Out] niComplexNumber[] Data, Int64 Data_Array_Size, out niRFSA_wfmInfo Waveform_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_FetchIQMultiRecordComplexI16", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchIQMultiRecordComplexI16(HandleRef Instrument_Handle, string Channel_List, Int64 Starting_Record, Int64 Number_of_Records, Int64 Number_of_Samples, double Timeout, [Out] short[] Data, [Out] niRFSA_wfmInfo[] Waveform_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_FetchIQMultiRecordComplexF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int FetchIQMultiRecordComplexF64(HandleRef Instrument_Handle, string Channel_List, Int64 Starting_Record, Int64 Number_of_Records, Int64 Number_of_Samples, double Timeout, [Out] niComplexNumber[] Data, [Out] niRFSA_wfmInfo[] Waveform_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_Abort", CallingConvention = CallingConvention.StdCall)]
            public static extern int Abort(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ReadPowerSpectrumF64", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadPowerSpectrumF64(HandleRef Instrument_Handle, string Channel_List, double Timeout, [In, Out] double[] Power_Spectrum_Data, int Data_Array_Size, out niRFSA_spectrumInfo Spectrum_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetNumberOfSpectralLines", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNumberOfSpectralLines(HandleRef Instrument_Handle, string Channel_List, out int Number_of_Spectral_Lines);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CheckAcquisitionStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int CheckAcquisitionStatus(HandleRef Instrument_Handle, out ushort Is_Done);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_Commit", CallingConvention = CallingConvention.StdCall)]
            public static extern int Commit(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test(HandleRef Instrument_Handle, out short testResult, System.Text.StringBuilder testMessage);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SelfCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int SelfCal(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_PerformThermalCorrection", CallingConvention = CallingConvention.StdCall)]
            public static extern int PerformThermalCorrection(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetFetchBacklog", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetFetchBacklog(HandleRef Instrument_Handle, string Channel_List, Int64 Record_Number, out Int64 Backlog);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query(HandleRef Instrument_Handle, System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetSpectralInfoForSMT", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetSpectralInfoForSMT(HandleRef Instrument_Handle, out SmtSpectrumInfo Spectrum_Info);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetFrequencyResponse", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetFrequencyResponse(HandleRef Instrument_Handle, string Channel_List, int Buffer_Size, [In, Out] double[] Frequencies, [In, Out] double[] Magnitude_Response, [In, Out] double[] Phase_Response, out int Number_of_Frequencies);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close(HandleRef Instrument_Handle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message(HandleRef Instrument_Handle, int Error_Code, System.Text.StringBuilder Error_Message_2);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, out int Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetAttributeViInt64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt64(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, out Int64 Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, out double Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, int Buffer_Size, System.Text.StringBuilder Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, out ushort Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, out System.IntPtr Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, int Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetAttributeViInt64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt64(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, long Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, double Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, string Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, ushort Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession(HandleRef Instrument_Handle, string Channel_Name, niRFSAProperties Attribute_ID, System.IntPtr Attribute_Value);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError(HandleRef Instrument_Handle, out int errorCode, int BufferSize, System.Text.StringBuilder Description);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CalAdjustIFAttenuationCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int CalAdjustIFAttenuationCalibration(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelList, int iFFilter, int numberofAttenuators, double[] attenuatorSettings, double measurement);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CalAdjustIFResponseCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int CalAdjustIFResponseCalibration(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelList, int iFFilter, double rFFrequency, double bandwidth, int numberofMeasurements, double[] measurements);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CalAdjustRefLevelCalibration", CallingConvention = CallingConvention.StdCall)]
            public static extern int CalAdjustRefLevelCalibration(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelList, int referenceLevelCalDataType, int rFBand, int attenuatorTableNumber, double frequency, double measurement);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CalSetTemperature", CallingConvention = CallingConvention.StdCall)]
            public static extern int CalSetTemperature(System.Runtime.InteropServices.HandleRef instrumentHandle, double temperaturedegreesC);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ChangeExtCalPassword", CallingConvention = CallingConvention.StdCall)]
            public static extern int ChangeExtCalPassword(System.Runtime.InteropServices.HandleRef instrumentHandle, string oldpassword, string newpassword);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CloseCalibrationStep", CallingConvention = CallingConvention.StdCall)]
            public static extern int CloseCalibrationStep(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CloseExtCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int CloseExtCal(System.Runtime.InteropServices.HandleRef instrumentHandle, int action);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CreateConfigurationList", CallingConvention = CallingConvention.StdCall)]
            public static extern int CreateConfigurationList(System.Runtime.InteropServices.HandleRef instrumentHandle, string listName, int numberOfListAttributes, niRFSAProperties[] listAttributeIDs, bool setAsActiveList);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_CreateConfigurationListStep", CallingConvention = CallingConvention.StdCall)]
            public static extern int CreateConfigurationListStep(System.Runtime.InteropServices.HandleRef instrumentHandle, bool setAsActiveStep);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_DeleteConfigurationList", CallingConvention = CallingConvention.StdCall)]
            public static extern int DeleteConfigurationList(System.Runtime.InteropServices.HandleRef instrumentHandle, string listName);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetCalUserDefinedInfo", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetCalUserDefinedInfo(System.Runtime.InteropServices.HandleRef instrumentHandle, StringBuilder userdefinedinfo);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetCalUserDefinedInfoMaxSize", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetCalUserDefinedInfoMaxSize(System.Runtime.InteropServices.HandleRef instrumentHandle, out int infoSize);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetExtCalLastDateAndTime", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetExtCalLastDateAndTime(System.Runtime.InteropServices.HandleRef instrumentHandle, out int year, out int month, out int day, out int hour, out int minute);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetExtCalLastTemp", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetExtCalLastTemp(System.Runtime.InteropServices.HandleRef instrumentHandle, out double temperature);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetExtCalRecommendedInterval", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetExtCalRecommendedInterval(System.Runtime.InteropServices.HandleRef instrumentHandle, out int months);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetNormalizationCoefficients", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetNormalizationCoefficients(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelList, int bufferSize, out niRFSA_coefficientInfo coefficientInfo, out int numberofCoefficientSets);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetScalingCoefficients", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetScalingCoefficients(System.Runtime.InteropServices.HandleRef instrumentHandle, string channelList, int arraySize, out niRFSA_coefficientInfo coefficientInfo, out int numberOfCoefficientSets);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_GetStreamEndpointHandle", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetStreamEndpointHandle(System.Runtime.InteropServices.HandleRef instrumentHandle, string streamEndpoint, out uint writerHandle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_InitExtCal", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitExtCal(string resourceName, string password, string optionstring, out System.IntPtr instrumentHandle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_InitializeCalibrationStep", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitializeCalibrationStep(System.Runtime.InteropServices.HandleRef instrumentHandle, int calibrationstep);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_ResetDevice", CallingConvention = CallingConvention.StdCall)]
            public static extern int ResetDevice(System.Runtime.InteropServices.HandleRef instrumentHandle);

            [DllImport(rfsaModule, EntryPoint = "niRFSA_SetCalUserDefinedInfo", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetCalUserDefinedInfo(System.Runtime.InteropServices.HandleRef instrumentHandle, string userdefinedinfo);


            public static int TestForError(HandleRef handle, int status)
            {
                if ((status < 0))
                {
                    PInvoke.ThrowError(handle, status);
                }
                return status;
            }

            public static int ThrowError(HandleRef handle, int code)
            {
                int size = PInvoke.GetError(handle, out code, 0, null);
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                if ((size >= 0))
                {
                    msg.Capacity = size;
                    PInvoke.GetError(handle, out code, size, msg);
                }
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            }
        }
    }

    public class niRFSAConstants
    {

        public const int Iq = 100;

        public const int Spectrum = 101;

        public const string Pfi0Str = "PFI0";

        public const string Pfi1Str = "PFI1";

        public const string Rtsi0Str = "PXI_Trig0";

        public const string Rtsi1Str = "PXI_Trig1";

        public const string Rtsi2Str = "PXI_Trig2";

        public const string Rtsi3Str = "PXI_Trig3";

        public const string Rtsi4Str = "PXI_Trig4";

        public const string Rtsi5Str = "PXI_Trig5";

        public const string Rtsi6Str = "PXI_Trig6";

        public const string Rtsi7Str = "PXI_Trig7";

        public const string PxiStarStr = "PXI_STAR";

        public const int RisingEdge = 900;

        public const int FallingEdge = 901;

        public const int RisingSlope = 1000;

        public const int FallingSlope = 1001;

        public const int StartTrigger = 1100;

        public const int RefTrigger = 702;

        public const int AdvanceTrigger = 1102;

        public const int ArmRefTrigger = 1103;

        public const int ReadyForStartEvent = 1200;

        public const int ReadyForAdvanceEvent = 1202;

        public const int ReadyForRefEvent = 1201;

        public const int EndOfRecordEvent = 1203;

        public const int DoneEvent = 1204;

        public const int RefClock = 1205;

        public const string DoNotExportStr = "";

        public const string ClkOutStr = "ClkOut";

        public const string RefOutStr = "RefOut";

        public const string PxiTrig0Str = "PXI_Trig0";

        public const string PxiTrig1Str = "PXI_Trig1";

        public const string PxiTrig2Str = "PXI_Trig2";

        public const string PxiTrig3Str = "PXI_Trig3";

        public const string PxiTrig4Str = "PXI_Trig4";

        public const string PxiTrig5Str = "PXI_Trig5";

        public const string PxiTrig6Str = "PXI_Trig6";

        public const int Dbm = 200;

        public const int VoltsSquared = 201;

        public const int Dbmv = 202;

        public const int Dbuv = 203;

        public const int Volts = 204;

        public const int Watts = 205;

        public const int Rbw3db = 300;

        public const int Rbw6db = 301;

        public const int RbwBinWidth = 302;

        public const int RbwEnbw = 303;

        public const int NoAveraging = 400;

        public const int RmsAveraging = 401;

        public const int VectorAveraging = 402;

        public const int PeakHoldAveraging = 403;

        public const int Uniform = 500;

        public const int Hanning = 501;

        public const int Hamming = 502;

        public const int BlackmanHarris = 503;

        public const int ExactBlackman = 504;

        public const int Blackman = 505;

        public const int FlatTop = 506;

        public const int _4TermBlackmanHarris = 507;

        public const int _7TermBlackmanHarris = 508;

        public const int LowSideLobe = 509;

        public const int MostRecentSample = 700;

        public const int FirstSample = 701;

        public const int FirstPretriggerSample = 703;

        public const int CurrentReadPosition = 704;

        public const string OnboardClockStr = "OnboardClock";

        public const string RefInStr = "RefIn";

        public const string PxiClk10Str = "PXI_Clk10";

        public const string ClkInStr = "ClkIn";

        public const string RefOut2Str = "RefOut2";

        public const string NoneStr = "None";

        public const int None = 600;

        public const int DigitalEdge = 601;

        public const int SoftwareEdge = 604;

        public const string PxiTrig7Str = "PXI_Trig7";

        public const string TimerEventStr = "TimerEvent";

        public const string CalClkOutStr = "ClkOut";

        public const int IqPowerEdge = 603;

        public const int Low = 800;

        public const int Medium = 801;

        public const int High = 802;

        public const int LoInjectionHighSide = 1300;

        public const int LoInjectionLowSide = 1301;

        public const int Narrow = 800;

        public const int Wide = 802;

        public const int Disabled = 1900;

        public const int Enabled = 1901;

        public const int _1875MhzWide = 1400;

        public const int _1875MhzNarrow = 1401;

        public const int _53Mhz = 1402;

        public const int Bypass = 1403;

        public const int FsuPpm = 2000;

        public const int FsuSecondsAfterLock = 2001;

        public const int FsuSecondsAfterIo = 2002;

        public const int ExtCalRfBand1 = 1700;

        public const int ExtCalRfBand2 = 1701;

        public const int ExtCalRfBand3 = 1702;

        public const int ExtCalRfBand4 = 1703;
    }

    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    public struct niRFSA_wfmInfo
    {

        public Double absoluteInitialX;

        public Double relativeInitialX;

        public Double xIncrement;

        public Int64 actualSamples;

        public Double offset;

        public Double gain;

        public Double reserved1;

        public Double reserved2;

    }

    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
    public struct niRFSA_spectrumInfo
    {

        public Double initialFrequency;

        public Double frequencyIncrement;

        public Int32 numberOfSpectralLines;

        public Double reserved1;

        public Double reserved2;

        public Double reserved3;

        public Double reserved4;

        public Double reserved5;

    }

    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct SmtSpectrumInfo
    {

        public ushort spectrumType;

        public ushort linearDB;

        public ushort window;

        public int windowSize;

        public int FFTSize;

    }

    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct niRFSA_coefficientInfo
    {

        public Double offset;

        public Double gain;

        public Double reserved1;

        public Double reserved2;

    }

    public enum niRFSAProperties
    {

        /// <summary>
        /// System.Int32
        /// </summary>
        AcquisitionType = 1150001,

        /// <summary>
        /// System.Double
        /// </summary>
        ReferenceLevel = 1150004,

        /// <summary>
        /// System.Double
        /// </summary>
        Attenuation = 1150005,

        /// <summary>
        /// System.Double
        /// </summary>
        MixerLevel = 1150006,

        /// <summary>
        /// System.Double
        /// </summary>
        IqCarrierFrequency = 1150059,

        /// <summary>
        /// System.Double
        /// </summary>
        IqRate = 1150007,

        /// <summary>
        /// System.Boolean
        /// </summary>
        NumberOfSamplesIsFinite = 1150008,

        /// <summary>
        /// System.Int64
        /// </summary>
        NumberOfSamples = 1150009,

        /// <summary>
        /// System.Boolean
        /// </summary>
        NumberOfRecordsIsFinite = 1150010,

        /// <summary>
        /// System.Int64
        /// </summary>
        NumberOfRecords = 1150011,

        /// <summary>
        /// System.Double
        /// </summary>
        SpectrumCenterFrequency = 1150002,

        /// <summary>
        /// System.Double
        /// </summary>
        SpectrumSpan = 1150003,

        /// <summary>
        /// System.Int32
        /// </summary>
        PowerSpectrumUnits = 1150012,

        /// <summary>
        /// System.Double
        /// </summary>
        ResolutionBandwidth = 1150013,

        /// <summary>
        /// System.Int32
        /// </summary>
        ResolutionBandwidthType = 1150014,

        /// <summary>
        /// System.Int32
        /// </summary>
        NumberOfSpectralLines = 1150018,

        /// <summary>
        /// System.Int32
        /// </summary>
        SpectrumAveragingMode = 1150016,

        /// <summary>
        /// System.Int32
        /// </summary>
        SpectrumNumberOfAverages = 1150015,

        /// <summary>
        /// System.Int32
        /// </summary>
        FftWindowType = 1150017,

        /// <summary>
        /// System.Int32
        /// </summary>
        FftWindowSize = 1150049,

        /// <summary>
        /// System.Int32
        /// </summary>
        FftSize = 1150050,

        /// <summary>
        /// System.Int32
        /// </summary>
        FetchRelativeTo = 1150045,

        /// <summary>
        /// System.Int32
        /// </summary>
        FetchOffset = 1150046,

        /// <summary>
        /// System.Int32
        /// </summary>
        RecordsDone = 1150047,

        /// <summary>
        /// System.String
        /// </summary>
        RefClockSource = 1150019,

        /// <summary>
        /// System.Double
        /// </summary>
        RefClockRate = 1150020,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedRefClockOutputTerminal = 1150072,

        /// <summary>
        /// System.String
        /// </summary>
        DigitizerSampleClockTimebaseSource = 1150021,

        /// <summary>
        /// System.Double
        /// </summary>
        DigitizerSampleClockTimebaseRate = 1150022,

        /// <summary>
        /// System.String
        /// </summary>
        PxiChassisClk10Source = 1150023,

        /// <summary>
        /// System.Int32
        /// </summary>
        StartTriggerType = 1150024,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeStartTriggerSource = 1150025,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeStartTriggerEdge = 1150026,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedStartTriggerOutputTerminal = 1150027,

        /// <summary>
        /// System.Int32
        /// </summary>
        RefTriggerType = 1150028,

        /// <summary>
        /// System.Int32
        /// </summary>
        RefTriggerPretriggerSamples = 1150035,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeRefTriggerSource = 1150029,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitalEdgeRefTriggerEdge = 1150030,

        /// <summary>
        /// System.String
        /// </summary>
        IqPowerEdgeRefTriggerSource = 1150055,

        /// <summary>
        /// System.Double
        /// </summary>
        IqPowerEdgeRefTriggerLevel = 1150056,

        /// <summary>
        /// System.Int32
        /// </summary>
        IqPowerEdgeRefTriggerSlope = 1150057,

        /// <summary>
        /// System.Double
        /// </summary>
        RefTriggerMinimumQuietTime = 1150058,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedRefTriggerOutputTerminal = 1150032,

        /// <summary>
        /// System.Double
        /// </summary>
        RefTriggerDelay = 1150060,

        /// <summary>
        /// System.Double
        /// </summary>
        StartToRefTriggerHoldoff = 1150033,

        /// <summary>
        /// System.Double
        /// </summary>
        RefToRefTriggerHoldoff = 1150034,

        /// <summary>
        /// System.Int32
        /// </summary>
        AdvanceTriggerType = 1150036,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeAdvanceTriggerSource = 1150037,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedAdvanceTriggerOutputTerminal = 1150038,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArmRefTriggerType = 1150039,

        /// <summary>
        /// System.String
        /// </summary>
        DigitalEdgeArmRefTriggerSource = 1150040,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedReadyForStartEventOutputTerminal = 1150041,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedReadyForAdvanceEventOutputTerminal = 1150042,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedReadyForRefEventOutputTerminal = 1150043,

        /// <summary>
        /// System.String
        /// </summary>
        ExportedEndOfRecordEventOutputTerminal = 1150044,

        /// <summary>
        /// System.Double
        /// </summary>
        LoFrequency = 1150068,

        /// <summary>
        /// System.Int32
        /// </summary>
        DownconverterLoopBandwidth = 1150067,

        /// <summary>
        /// System.Boolean
        /// </summary>
        DigitalIfEqualizationEnabled = 1150048,

        /// <summary>
        /// System.Double
        /// </summary>
        DownconverterGain = 1150065,

        /// <summary>
        /// System.Int32
        /// </summary>
        LoInjectionSide = 1150069,

        /// <summary>
        /// System.Double
        /// </summary>
        DigitizerVerticalRange = 1150070,

        /// <summary>
        /// System.Boolean
        /// </summary>
        EnableFractionalResampling = 1150071,

        /// <summary>
        /// System.String
        /// </summary>
        SerialNumber = 1150053,

        /// <summary>
        /// System.Double
        /// </summary>
        DeviceTemperature = 1150051,

        /// <summary>
        /// System.Double
        /// </summary>
        TemperatureReadInterval = 1150061,

        /// <summary>
        /// System.Boolean
        /// </summary>
        Cache = 1050004,

        /// <summary>
        /// System.Boolean
        /// </summary>
        InterchangeCheck = 1050021,

        /// <summary>
        /// System.String
        /// </summary>
        DriverSetup = 1050007,

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
        /// System.String
        /// </summary>
        SupportedInstrumentModels = 1050327,

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
        /// string
        /// </summary>
        ActiveConfigurationList = 1150092,

        /// <summary>
        /// long
        /// </summary>
        ActiveConfigurationListStep = 1150093,

        /// <summary>
        /// System.Int32
        /// </summary>
        CalRfPathSelection = 1150083,

        /// <summary>
        /// int
        /// </summary>
        DataTransferBlockSize = 1150105,

        /// <summary>
        /// double
        /// </summary>
        DataTransferMaximumBandwidth = 1150104,

        /// <summary>
        /// System.Int32
        /// </summary>
        DigitizerDitherEnabled = 1150080,

        /// <summary>
        /// double
        /// </summary>
        DigitizerTemperature = 1150090,

        /// <summary>
        /// double
        /// </summary>
        DownconverterCenterFrequency = 1150082,

        /// <summary>
        /// double
        /// </summary>
        ExternalGain = 1150094,

        /// <summary>
        /// double
        /// </summary>
        FrequencySettling = 1150088,

        /// <summary>
        /// System.Int32
        /// </summary>
        FrequencySettlingUnits = 1150087,

        /// <summary>
        /// double
        /// </summary>
        If1AttenValue = 1150078,

        /// <summary>
        /// double
        /// </summary>
        If2AttenValue = 1150079,

        /// <summary>
        /// double
        /// </summary>
        IfAttenuation = 1150074,

        /// <summary>
        /// System.Int32
        /// </summary>
        IfFilter = 1150075,

        /// <summary>
        /// double
        /// </summary>
        IfOutputFrequency = 1150086,

        /// <summary>
        /// double
        /// </summary>
        LoTemperature = 1150089,

        /// <summary>
        /// System.Int32
        /// </summary>
        MechanicalAttenuatorEnabled = 1150081,

        /// <summary>
        /// long
        /// </summary>
        MemorySize = 1150085,

        /// <summary>
        /// string
        /// </summary>
        ModuleRevision = 1150091,

        /// <summary>
        /// bool
        /// </summary>
        P2pEnabled = 1150097,

        /// <summary>
        /// bool
        /// </summary>
        P2pEndpointOverflow = 1150103,

        /// <summary>
        /// long
        /// </summary>
        P2pEndpointSize = 1150102,

        /// <summary>
        /// long
        /// </summary>
        P2pFifoEndpointCount = 1150098,

        /// <summary>
        /// long
        /// </summary>
        P2pMostSamplesAvailableInEndpoint = 1150101,

        /// <summary>
        /// bool
        /// </summary>
        P2pOnboardMemoryEnabled = 1150107,

        /// <summary>
        /// long
        /// </summary>
        P2pSamplesAvailableInEndpoint = 1150100,

        /// <summary>
        /// long
        /// </summary>
        P2pSamplesTransferred = 1150099,

        /// <summary>
        /// double
        /// </summary>
        PhaseOffset = 1150106,

        /// <summary>
        /// int
        /// </summary>
        RfAttenuationIndex = 1150076,

        /// <summary>
        /// int
        /// </summary>
        RfAttenuationTable = 1150077,

        /// <summary>
        /// double
        /// </summary>
        TimerEventInterval = 1150096,
    }
}
