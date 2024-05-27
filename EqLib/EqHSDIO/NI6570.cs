using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using Keysight.KtMDsr.Interop;
using NationalInstruments.ModularInstruments.NIDigital;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using Ivi.Driver.Interop;
using System.Diagnostics;
using ProductionLib;

namespace EqLib
{
    public partial class EqHSDIO
    {
        public class NI6570 : EqHSDIObase
        {
            /*
             * Notes:  Requires the following References added to project (set Copy Local = false):
             *   - NationalInstruments.ModularInstruments.NIDigital.Fx40
             *   - Ivi.Driver
             */

            // The Instrument Session
            public NIDigital DIGI;
            public string SerialNumber
            {
                get
                {
                    ModularInstrumentsSystem Modules = new ModularInstrumentsSystem();
                    foreach (DeviceInfo ModulesInfo in Modules.DeviceCollection)
                    {
                        if (ModulesInfo.Name == VisaAlias)
                        {
                            return ModulesInfo.SerialNumber;
                        }
                    }
                    return "NA";
                }
            }

            #region Private Variables
            private string allRffeChans, allEEPROMChans, allTEMPSENSEChans, allEEPROM_UNIOChans, allRffeChanswoVio;
            private DigitalPinSet
                allEEPROMPins, EEPROMVccPin, EEPROMsckPin,
                allRffePins, allRffePinswoVio, sdata1Pin, sclk1Pin, sdata2Pin, sclk2Pin, trigPin,
                allTEMPSENSEPins, TEMPSENSEsckPin, TEMPSENSEVccPin,
                allEEPROM_UNIOPins, EEPROM_UNIOVpupPin, EEPROM_UNIOSIOPin, EEPROM_UNIOVpup2Pin, EEPROM_UNIOSIO2Pin             // EEPROM UNI/O
                ;
            private string[] allDutPins = new string[] { };
            private List<string> loadedPatternFiles; // used to store previously loaded patterns so we don't try and double load.  Double Loading will cause an error, so always check this list to see if pattern was previously loaded.
            private List<string> loadedPatternNames; // used to check if a pattern of that name was loaded without execution error. (message popup) make TCF debug easier
            private double MIPIClockRate;  // MIPI NRZ Clock Rate (2 x Vector Rate)
            private double EEPROMClockRate;  // EEPROM NRZ Clock Rate (2 x Vector Rate)
            private double UNIORate;
            private bool eepromReadWriteEnabled = false;
            private double StrobePoint;
            private bool forceDigiPatRegeneration = true; //false;  // Set this to true if you want to re-generate all .digipat files from the .vec files, even if the .vec files haven't changed.
            private int NumBitErrors; // Stores the number of bit errors from the most recently executed pattern.
            private Dictionary<string, uint> captureCounters = new Dictionary<string, uint>(); // This dictionary stores the # of captures each .vec contains (for .vec files that are converted to capture format)
            private string fileDir; // This is the path used to store intermediate digipatsrc, digipat, and other files.
            private TrigConfig triggerConfig = TrigConfig.PXI_Backplane;  // No Triggering by default
            private PXI_Trig pxiTrigger = PXI_Trig.PXI_Trig2;  // TRIG0 - TRIG2 used by various other instruments in Clotho;  TRIG7 shouldn't interfere.
            private uint regWriteTriggerCycleDelay = 0;

            private bool debug = false; // Turns on additional console messages if true

            private string I2CVCCChanName; // = "VCC";
            private string I2CSDAChanName; // = "SDA";
            private string I2CSCKChanName; // = "SCK";
            private string TEMPSENSEI2CVCCChanName; // = "TSVCC";

            private string UNIOVPUPChanName = "UNIO_VPUP";
            private string UNIOSIOChanName = "UNIO_SIO";
            private string UNIOVPUP2ChanName = "UNIO2_VPUP";
            private string UNIOSIO2ChanName = "UNIO2_SIO";

            //Temp Sense State
            private bool TempSenseStateOn = false;
            private double TempSenseRaw = 0;

            private DigitalTimeSet tsNRZ;
            // keng shan Added
            private Dictionary<string, uint[]> SourceWaveform = new Dictionary<string, uint[]>();
            private object lockObject = new object();

            public HiPerfTimer uTimer = new HiPerfTimer();
            public CustomMipiLevel m_DeviceMipiLevel { get; private set; }
            public CustomMipiLevel Reconfigurable_DeviceMipiLevel { get; private set; }
            public class CustomMipiLevel
            {
                public double vih, vil, voh, vol, vtt;

                public CustomMipiLevel(double vih, double vil, double voh, double vol, double vtt)
                {
                    this.vih = vih;
                    this.vil = vil;
                    this.voh = voh;
                    this.vol = vol;
                    this.vtt = vtt;
                }
            }
            //  Reuse the channel names varibles from the other vectors to avoid hardcoding names Ken Hilla
            // if you need to add more channels define them in the custom testplan (Digital_Definitions_Part_Specific) then modify the Initialize function here to set them up

            //private const string Sclk1ChanName = "Sclk_TX", Sdata1ChanName = "Sdata_TX", Vio1ChanName = "Vio_TX",    
            //    Sclk2ChanName = "Sclk_RX", Sdata2ChanName = "Sdata_RX", Vio2ChanName = "Vio_RX", TrigChanName = "Trig";

            #region RZ Vectors for RFONOFF and RFONOFFSwitch Test (Seoraksan 1.7)
            //Tx
            uint[] TrigOffRz = new uint[23] { 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] TrigOnRz = new uint[23] { 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 };
            uint[] TrigMaskOnRz = new uint[23] { 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0 };

            //Rx         
            uint[] TrigOffRzRx = new uint[23] { 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] TrigOnRzRx = new uint[23] { 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 };
            uint[] TrigMaskOnRzRx = new uint[23] { 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0 };

            #endregion

            #endregion

            /// <summary>
            /// Initialize the NI 6570 Instrument:
            ///   - Open Instrument session
            ///   - Reset Instrument and Unload All Patterns from Instrument Memory
            ///   - Configure Pin -> Channel Mapping
            ///   - Configure Timing for: MIPI (6556 style NRZ) & MIPI_SCLK_RZ (6570 style RZ)
            ///   - Configure 6570 in Digital mode by default (instead of PPMU mode)
            /// </summary>
            /// <param name="visaAlias">The VISA Alias of the instrument, typically NI6570.</param>
            public override bool Initialize()
            {
                ////Tx
                //TrigOffRz = Digital_Mipi_Trig["TrigOffRz".ToUpper()];
                //TrigOnRz = Digital_Mipi_Trig["TrigOnRz".ToUpper()];
                //TrigMaskOnRz = Digital_Mipi_Trig["TrigMaskOnRz".ToUpper()];

                ////Rx         
                //TrigOffRzRx = Digital_Mipi_Trig["TrigOffRzRx".ToUpper()];
                //TrigOnRzRx = Digital_Mipi_Trig["TrigOnRzRx".ToUpper()];
                //TrigMaskOnRzRx = Digital_Mipi_Trig["TrigMaskOnRzRx".ToUpper()];

                // Clock Rate & Cable Delay
                MIPIClockRate = EqHSDIO.MIPIClockRate; // 52e6; // This is the Non-Return to Zero rate, Actual Vector rate is 1/2 of this.
                EEPROMClockRate = 2e5; // This is the Non-Return to Zero rate, Actual Vector rate is 1/2 of this.
                UNIORate = 1e6;        // This is the Non-Return to Zero rate, UNI/O has no clock.
                //TempSenseClockRate = 2e5;


                // Set these values based on calling ((HSDIO.NI6570)HSDIO.Instrument).shmoo("QC_Test");
                //shmoo("QC_Test");  hosein 09242020
                // Ideally, try to set UserDelay = 0 if possible and only modify StrobePoint.
                StrobePoint = EqHSDIO.StrobePoint; //Maximator compare delay for demo board using flying lead probe         // 77e-9;
                regWriteTriggerCycleDelay = 0;

                // Trigger Configuration;  This applies to the RegWrite command and will send out a hardware trigger
                // on the specified triggers (Digital Pin, PXI Backplane, or Both) at the end of the Register Write operation.
                triggerConfig = TrigConfig.Digital_Pin;
                pxiTrigger = (PXI_Trig)siteTrigArray[Site][2];  // TRIG0 - TRIG2 used by various other instruments in Clotho;  TRIG6 shouldn't interfere.

                #region Initialize Private Variables
                fileDir = Path.GetTempPath() + "NI.Temp\\NI6570_" + Site;
                Directory.CreateDirectory(fileDir);
                #endregion

                #region Initialize Instrument
                // Initialize private variables
                loadedPatternFiles = new List<string> { };
                loadedPatternNames = new List<string> { };

                DIGI = new NIDigital(this.VisaAlias, false, true);
                DIGI.Utility.ResetDevice();
                #endregion

                Eq.InstrumentInfo += GetInstrumentInfo();

                #region NI Pin Map Configuration
                // Make sure you add all needed pins here so that they get auto-added to all NI-6570 digipat files.  If they aren't in allDutPins or allSystemPins, you can't use them.

                //Sclk1ChanName = Get_Digital_Definition("SCLK1_VEC_NAME");  
                //Sdata1ChanName = Get_Digital_Definition("SDATA1_VEC_NAME");  
                //Vio1ChanName = Get_Digital_Definition("VIO1_VEC_NAME");

                //if (Num_Mipi_Bus == 2)
                //{
                //    Sclk2ChanName = Get_Digital_Definition("SCLK2_VEC_NAME");
                //    Sdata2ChanName = Get_Digital_Definition("SDATA2_VEC_NAME");
                //    Vio2ChanName = Get_Digital_Definition("VIO2_VEC_NAME");
                //}


                //ShieldChanName = Get_Digital_Definition("SHIELD_VEC_NAME");  
                TrigChanName = Get_Digital_Definition("TRIG_VEC_NAME");
                I2CVCCChanName = Get_Digital_Definition("I2C_VCC_VEC_NAME");
                I2CSCKChanName = Get_Digital_Definition("I2C_SCK_VEC_NAME");
                I2CSDAChanName = Get_Digital_Definition("I2C_DAC_VEC_NAME");
                TEMPSENSEI2CVCCChanName = Get_Digital_Definition("TEMPSENSE_I2C_VCC_VEC_NAME");

                UNIOVPUPChanName = Get_Digital_Definition("UNIO_VPUP_VEC_NAME");
                UNIOSIOChanName = Get_Digital_Definition("UNIO_SIO_VEC_NAME");
                UNIOVPUP2ChanName = Get_Digital_Definition("UNIO_VPUP2_VEC_NAME");
                UNIOSIO2ChanName = Get_Digital_Definition("UNIO_SIO2_VEC_NAME");

                // Map extra pins that are not included in the TCF as of 10/07/2015
                //PinNamesAndChans[ShieldChanName] = Get_Digital_Definition("SHIELD_CHANNEL"); //"8";  
                PinNamesAndChans[TrigChanName] = Get_Digital_Definition("TRIG_CHANNEL");
                PinNamesAndChans[I2CVCCChanName] = Get_Digital_Definition("I2C_VCC_CHANNEL");
                PinNamesAndChans[I2CSCKChanName] = Get_Digital_Definition("I2C_SCK_CHANNEL");
                PinNamesAndChans[I2CSDAChanName] = Get_Digital_Definition("I2C_SDA_CHANNEL");
                PinNamesAndChans[TEMPSENSEI2CVCCChanName] = Get_Digital_Definition("TEMPSENSE_I2C_VCC_CHANNEL");

                // UNIO
                PinNamesAndChans[UNIOVPUPChanName] = Get_Digital_Definition("UNIO_VPUP_CHANNEL");
                PinNamesAndChans[UNIOSIOChanName] = Get_Digital_Definition("UNIO_SIO_CHANNEL");
                PinNamesAndChans[UNIOVPUP2ChanName] = Get_Digital_Definition("UNIO_VPUP2_CHANNEL");
                PinNamesAndChans[UNIOSIO2ChanName] = Get_Digital_Definition("UNIO_SIO2_CHANNEL");

                if (Num_Mipi_Bus == 2)
                    allRffeChans = Sclk1ChanName.ToUpper() + "," + Sdata1ChanName.ToUpper() + "," + Vio1ChanName.ToUpper() + "," + Sclk2ChanName.ToUpper() + "," + Sdata2ChanName.ToUpper() + "," + Vio2ChanName.ToUpper() + "," + TrigChanName.ToUpper();
                else
                    allRffeChans = Sclk1ChanName.ToUpper() + "," + Sdata1ChanName.ToUpper() + "," + Vio1ChanName.ToUpper() + "," + TrigChanName.ToUpper();

                allRffeChanswoVio = Sclk1ChanName.ToUpper() + "," + Sdata1ChanName.ToUpper() + "," + Sclk2ChanName.ToUpper() + "," + Sdata2ChanName.ToUpper() + /* "," + Vio2ChanName.ToUpper() + */"," + TrigChanName.ToUpper(); // Pinot added (Pcon)
                allEEPROMChans = I2CSCKChanName.ToUpper() + "," + I2CSDAChanName.ToUpper() + "," + I2CVCCChanName.ToUpper();
                allTEMPSENSEChans = I2CSCKChanName.ToUpper() + "," + I2CSDAChanName.ToUpper() + "," + TEMPSENSEI2CVCCChanName.ToUpper();
                allEEPROM_UNIOChans = UNIOVPUPChanName.ToUpper() + "," + UNIOSIOChanName.ToUpper() + "," + UNIOVPUP2ChanName.ToUpper() + "," + UNIOSIO2ChanName.ToUpper();


                if (Num_Mipi_Bus == 2)
                {
                    this.allDutPins = new string[] {
                        Sclk1ChanName, Sdata1ChanName, Vio1ChanName,
                        Sclk2ChanName, Sdata2ChanName, Vio2ChanName, TrigChanName,
                        I2CSCKChanName, I2CSDAChanName, I2CVCCChanName, TEMPSENSEI2CVCCChanName,
                        UNIOVPUPChanName, UNIOSIOChanName, UNIOVPUP2ChanName, UNIOSIO2ChanName
                    };
                }
                else
                {
                    this.allDutPins = new string[] {
                        Sclk1ChanName, Sdata1ChanName, Vio1ChanName, TrigChanName,
                        I2CSCKChanName, I2CSDAChanName, I2CVCCChanName, TEMPSENSEI2CVCCChanName,
                        UNIOVPUPChanName, UNIOSIOChanName, UNIOVPUP2ChanName, UNIOSIO2ChanName
                    };
                }

                // Map all pins that are defined in the TCF as well as any other "extra" pins such as Trigger, and EEPROM
                // Create combined Pin List
                string[] allPinsUpperCase = new string[allDutPins.Length];
                int i = 0;
                foreach (string pin in allDutPins)
                    allPinsUpperCase[i++] = pin.ToUpper();




                // Configure 6570 Pin Map with all pins
                DIGI.PinAndChannelMap.CreatePinMap(allPinsUpperCase, null);
                DIGI.PinAndChannelMap.CreateChannelMap(1);
                foreach (string pin in allDutPins)
                    DIGI.PinAndChannelMap.MapPinToChannel(pin.ToUpper(), 0, PinNamesAndChans[pin]);

                DIGI.PinAndChannelMap.EndChannelMap();

                // Get DigitalPinSets
                allRffePins = DIGI.PinAndChannelMap.GetPinSet(allRffeChans);
                allRffePinswoVio = DIGI.PinAndChannelMap.GetPinSet(allRffeChanswoVio); // Pinot added (Pcon)
                allEEPROMPins = DIGI.PinAndChannelMap.GetPinSet(allEEPROMChans);
                EEPROMsckPin = DIGI.PinAndChannelMap.GetPinSet(I2CSCKChanName.ToUpper());
                EEPROMVccPin = DIGI.PinAndChannelMap.GetPinSet(I2CVCCChanName.ToUpper());

                allEEPROM_UNIOPins = DIGI.PinAndChannelMap.GetPinSet(allEEPROM_UNIOChans);
                EEPROM_UNIOVpupPin = DIGI.PinAndChannelMap.GetPinSet(UNIOVPUPChanName.ToUpper());
                EEPROM_UNIOSIOPin = DIGI.PinAndChannelMap.GetPinSet(UNIOSIOChanName.ToUpper());
                EEPROM_UNIOVpup2Pin = DIGI.PinAndChannelMap.GetPinSet(UNIOVPUP2ChanName.ToUpper());
                EEPROM_UNIOSIO2Pin = DIGI.PinAndChannelMap.GetPinSet(UNIOSIO2ChanName.ToUpper());

                sclk1Pin = DIGI.PinAndChannelMap.GetPinSet(Sclk1ChanName.ToUpper());
                sdata1Pin = DIGI.PinAndChannelMap.GetPinSet(Sdata1ChanName.ToUpper());

                if (Num_Mipi_Bus == 2)
                {
                    sclk2Pin = DIGI.PinAndChannelMap.GetPinSet(Sclk2ChanName.ToUpper());
                    sdata2Pin = DIGI.PinAndChannelMap.GetPinSet(Sdata2ChanName.ToUpper());
                }

                trigPin = DIGI.PinAndChannelMap.GetPinSet(TrigChanName.ToUpper());
                allTEMPSENSEPins = DIGI.PinAndChannelMap.GetPinSet(allTEMPSENSEChans);
                TEMPSENSEsckPin = DIGI.PinAndChannelMap.GetPinSet(I2CSCKChanName.ToUpper());
                TEMPSENSEVccPin = DIGI.PinAndChannelMap.GetPinSet(TEMPSENSEI2CVCCChanName.ToUpper());


                #endregion

                #region MIPI Level Configuration
                double vih = Convert.ToDouble(Get_Digital_Definition("VIH"));
                double vil = Convert.ToDouble(Get_Digital_Definition("VIL"));
                double voh = Convert.ToDouble(Get_Digital_Definition("VOH"));
                double vol = Convert.ToDouble(Get_Digital_Definition("VOL"));
                //double voh = .9;  KH experiment to improve temp sense read
                //double vol = 0.8;
                double vtt = 3.0;
                m_DeviceMipiLevel = new CustomMipiLevel(vih, vil, voh, vol, vtt);
                Reconfigurable_DeviceMipiLevel = m_DeviceMipiLevel;

                allRffePins.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                trigPin.DigitalLevels.ConfigureVoltageLevels(0.0, 5.0, 0.5, 2.5, 5.0); // Set VST Trigger Channel to 5V logic.  VST's PFI0 VIH is 2.0V, absolute max is 5.5V
                #endregion

                #region EEPROM Level Configuration
                vih = 5.0;
                vil = 0.0;
                voh = 2.4;
                vol = 1.0;
                vtt = 5.0;
                allEEPROMPins.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                #endregion

                #region EEPROM UNI/O Level Configuration
                // UNIO
                vih = 2.7;
                vil = 0;
                voh = 2;
                vol = 0.5;
                vtt = 2.7;
                allEEPROM_UNIOPins.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                #endregion

                #region TEMPSENSE Level Configuration
                vih = 3.0;
                vil = 0.0;
                voh = 1.5;
                vol = 1.0;
                vtt = 3.0;
                allTEMPSENSEPins.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                TEMPSENSEVccPin.DigitalLevels.Vcom = vtt;
                #endregion

                #region Timing Variable Declarations
                // Variables
                double period_dbl;
                Ivi.Driver.PrecisionTimeSpan period;
                Ivi.Driver.PrecisionTimeSpan driveOn, driveOn_half;
                Ivi.Driver.PrecisionTimeSpan driveData, driveData_half;
                Ivi.Driver.PrecisionTimeSpan driveReturn, driveReturn_half;
                Ivi.Driver.PrecisionTimeSpan driveOff, driveOff_half;
                Ivi.Driver.PrecisionTimeSpan compareStrobe;
                Ivi.Driver.PrecisionTimeSpan clockRisingEdgeDelay;
                Ivi.Driver.PrecisionTimeSpan clockFallingEdgeDelay;
                #endregion

                #region MIPI Timing Configuration
                #region Timing configuration for Return to Zero format Patterns.

                // All RegRead / RegWrite functions use the RZ format for SCLK
                // Vector Rate is Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / MIPIClockRate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl); //0.5
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl); //0.5
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                DigitalTimeSet tsRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI_RZ.ToString("g"));
                tsRZ.ConfigurePeriod(period);

                // Vio, Sdata, Trig
                tsRZ.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk
                tsRZ.ConfigureDriveEdges(sclk1Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsRZ.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);

                if (Num_Mipi_Bus == 2)
                {
                    tsRZ.ConfigureDriveEdges(sclk2Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                    tsRZ.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);
                }


                // All RegRead / RegWrite functions use the RZ format for SCLK
                // Vector Rate is Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / (MIPIClockRate / 2);
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl); //0.5
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl); //0.5
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint * 2);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                DigitalTimeSet tsRZ_HALF = DIGI.Timing.CreateTimeSet(Timeset.MIPI_RZ_HALF.ToString("g"));
                tsRZ_HALF.ConfigurePeriod(period);

                // Vio, Sdata, Trig
                tsRZ_HALF.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ_HALF.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk
                tsRZ_HALF.ConfigureDriveEdges(sclk1Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsRZ_HALF.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);

                if (Num_Mipi_Bus == 2)
                {
                    tsRZ_HALF.ConfigureDriveEdges(sclk2Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                    tsRZ_HALF.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);
                }


                // All RegRead / RegWrite functions use the RZ format for SCLK
                // Vector Rate is Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / (MIPIClockRate / 4);
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl); //0.5
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl); //0.5
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint * 2.5);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                DigitalTimeSet tsRZ_Quad = DIGI.Timing.CreateTimeSet(Timeset.MIPI_RZ_QUAD.ToString("g"));
                tsRZ_Quad.ConfigurePeriod(period);

                // Vio, Sdata, Trig
                tsRZ_Quad.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ_Quad.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk
                tsRZ_Quad.ConfigureDriveEdges(sclk1Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsRZ_Quad.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);

                if (Num_Mipi_Bus == 2)
                {
                    tsRZ_Quad.ConfigureDriveEdges(sclk2Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                    tsRZ_Quad.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);
                }

                #endregion

                #region Timing configuration for Non Return to Zero format Patterns (eg: 6556 style).
                // Standard .vec files use the Non Return to Zero Format

                //Actual Vector Rate is still 1/2 Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / MIPIClockRate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(3.0 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0); //period / 8;  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                /* *******             DigitalTimeSet tsNRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI.ToString("g"));    */
                tsNRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI.ToString("g"));
                tsNRZ.ConfigurePeriod(period);


                // Vio, Sdata, Trig
                tsNRZ.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsNRZ.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk

                tsNRZ.ConfigureDriveEdges(sclk1Pin, DriveFormat.NonReturn, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsNRZ.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);

                if (Num_Mipi_Bus == 2)
                {
                    tsNRZ.ConfigureDriveEdges(sclk2Pin, DriveFormat.NonReturn, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                    tsNRZ.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);
                }
                #endregion
                #endregion

                #region MIPI RFONOFFTime Timing Configuration

                #region Timing configuration for Return to Zero format Patterns.                                                            ////////////////Thaison, Frank
                // All RegRead / RegWrite functions use the RZ format for SCLK

                // Vector Rate is Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / MIPIClockRate;
                //period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                //driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                //driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                //driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.5 * period_dbl);
                //driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(3.0 * period_dbl);
                //compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint);

                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                //CM WONG: Jedi is 0.5, PC3 is 0
                //driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                //driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint);

                driveOn_half = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl);
                driveData_half = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl);
                driveReturn_half = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1 * period_dbl);
                driveOff_half = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);   // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                tsRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI_RFONOFF.ToString("g"));
                DigitalTimeSet tsRZ_Half = DIGI.Timing.CreateTimeSet(Timeset.MIPI_HALF_RFONOFF.ToString("g"));
                tsRZ.ConfigurePeriod(period);
                tsRZ_Half.ConfigurePeriod(period * 2);

                // Vio, Sdata, Trig
                tsRZ.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk
                tsRZ.ConfigureDriveEdges(sclk1Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsRZ.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);
                tsRZ.ConfigureDriveEdges(sclk2Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsRZ.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);

                // Vio, Sdata, Trig - Half Clk (Read)
                tsRZ_Half.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn_half, driveData_half, driveReturn_half, driveOff_half);
                tsRZ_Half.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk - Half Clk (Read)
                tsRZ_Half.ConfigureDriveEdges(sclk1Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn_half + clockFallingEdgeDelay, driveOff_half + clockFallingEdgeDelay);
                tsRZ_Half.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);
                tsRZ_Half.ConfigureDriveEdges(sclk2Pin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn_half + clockFallingEdgeDelay, driveOff_half + clockFallingEdgeDelay);
                tsRZ_Half.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);
                #endregion

                #region Timing configuration for Non Return to Zero format Patterns (eg: 6556 style).
                // Standard .vec files use the Non Return to Zero Format

                //Actual Vector Rate is still 1/2 Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / MIPIClockRate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.5 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.0 * period_dbl);
                //compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.8 * period_dbl);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0); //period / 8;  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                tsNRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI_SCLK_NRZ_RFONOFF.ToString("g"));
                tsNRZ.ConfigurePeriod(period);


                // Vio, Sdata, Trig
                tsNRZ.ConfigureDriveEdges(allRffePins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsNRZ.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                // Sclk
                tsNRZ.ConfigureDriveEdges(sclk1Pin, DriveFormat.NonReturn, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsNRZ.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);
                tsNRZ.ConfigureDriveEdges(sclk2Pin, DriveFormat.NonReturn, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsNRZ.ConfigureCompareEdgesStrobe(sclk2Pin, compareStrobe);
                #endregion

                #endregion

                #region EEPROM Timing Configuration
                // Current EEPROM Implementation uses the Non Return to Zero Clock Format
                //Actual Vector Rate is 1/2 Clock Toggle Rate.
                period_dbl = 1.0 / EEPROMClockRate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.0);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.0);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.9 * period_dbl);

                // Create EEPROM NRZ Timeset
                DigitalTimeSet tsEEPROMNRZ = DIGI.Timing.CreateTimeSet(Timeset.EEPROM.ToString("g"));
                tsEEPROMNRZ.ConfigurePeriod(period);

                tsEEPROMNRZ.ConfigureDriveEdges(allEEPROMPins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsEEPROMNRZ.ConfigureCompareEdgesStrobe(allEEPROMPins, compareStrobe);

                // Shift all EEPROM SCK edges by 1/4 Period so SDA is stable before clock rising edge
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.25 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.25 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.25 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.25 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.15 * period_dbl);
                // Set EEPROM SCK timing
                tsEEPROMNRZ.ConfigureDriveEdges(EEPROMsckPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);

                #endregion

                #region EEPROM UNI/O Timing Configuration
                // UNIO
                period_dbl = 1.0 / UNIORate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.0);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.0);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.98 * period_dbl);

                // UNIO EEPROM NRZ Timeset
                DigitalTimeSet tsEEPROMUNIONRZ = DIGI.Timing.CreateTimeSet(Timeset.UNIO_EEPROM.ToString("g"));
                tsEEPROMUNIONRZ.ConfigurePeriod(period);

                tsEEPROMUNIONRZ.ConfigureDriveEdges(allEEPROM_UNIOPins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsEEPROMUNIONRZ.ConfigureCompareEdgesStrobe(allEEPROM_UNIOPins, compareStrobe);
                #endregion

                #region TEMPSENSE Timing Configuration
                // Current TEMPSENSE Implementation uses the Non Return to Zero Clock Format
                //Actual Vector Rate is 1/2 Clock Toggle Rate.
                period_dbl = 1.0 / EEPROMClockRate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.0);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.0);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.9 * period_dbl);

                // Create TEMPSENSE NRZ Timeset
                DigitalTimeSet tsTEMPSENSENRZ = DIGI.Timing.CreateTimeSet(Timeset.TEMPSENSE.ToString("g"));
                tsTEMPSENSENRZ.ConfigurePeriod(period);

                tsTEMPSENSENRZ.ConfigureDriveEdges(allTEMPSENSEPins, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsTEMPSENSENRZ.ConfigureCompareEdgesStrobe(allTEMPSENSEPins, compareStrobe);

                // Shift all TEMPSENSE SCK edges by 1/4 Period so SDA is stable before clock rising edge //MM added 0.1 to everything
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.25 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0.25 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.25 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.25 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(1.15 * period_dbl);
                // Set TEMPSENSE SCK timing
                tsTEMPSENSENRZ.ConfigureDriveEdges(TEMPSENSEsckPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);

                #endregion

                #region Configure 6570 for Digital Mode with HighZ Termination

                if (!isVioTxPpmu)
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                }
                else
                {
                    allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                    allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                }


                allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                #endregion

                usingMIPI = true;

                //this.LoadVector_RFOnOffTest();
                //LoadVector_RFOnOffTestRx(bool isNRZ = false); //Rx Trigger

                //this.LoadVector_RFOnOffSwitchTest(bool isNRZ = false);
                //LoadVector_RFOnOffSwitchTest_WithPreMipi(); //Tx Trigger: RFOnOff + SwitchingTime
                //LoadVector_RFOnOffSwitchTest_With3TxPreMipi();    //Tx Trigger: RFOnOff + SwitchingTime, 3TXPreMipi, For TX Band-To-Band

                //LoadVector_RFOnOffSwitchTestRx();   //Rx Trigger: RFOnOff + SwitchingTime
                //LoadVector_RFOnOffSwitchTestRx_WithPreMipi();   //Rx Trigger: RFOnOff + SwitchingTime, 1RXPreMipi, for LNA Output Switching
                //LoadVector_RFOnOffSwitchTestRx_With1Tx2RxPreMipi();    //Rx Trigger: RFOnOff + SwitchingTime, 1TXPreMipi, 2RXPreMipi, For LNA switching time (same output) G0 only

                //LoadVector_RFOnOffSwitchTest2();     //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2
                //LoadVector_RFOnOffSwitchTest2_WithPreMipi();    //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2, 1TXPreMipi, For CPL
                //LoadVector_RFOnOffSwitchTest2_With1Tx2RxPreMipi();    //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2, 1TXPreMipi, 2RXPreMipi, For TERM:TX:RX:TX  

                //LoadVector_RFOnOffSwitchTest2Rx();  //Rx Trigger: RFOnOff + SwitchingTime + SwitchingTime2



                return usingMIPI;
            }

            public override bool ReInitializeVIO(double violevel)
            {
                #region MIPI Level Configuration
                double vih = Math.Round(violevel, 2);
                double vil = Convert.ToDouble(Get_Digital_Definition("VIL"));
                double voh = Convert.ToDouble(Get_Digital_Definition("VOH"));
                double vol = Convert.ToDouble(Get_Digital_Definition("VOL"));
                //double voh = .9;  KH experiment to improve temp sense read
                //double vol = 0.8;
                double vtt = 3.0;

                double Vio1Level_Readback = Math.Round(DIGI.PinAndChannelMap.GetPinSet("VIO1").DigitalLevels.Vih, 2);
                //Do not reconfigure voltage levels if it is same as readback voltage level
                if (vih == Vio1Level_Readback)
                {
                    return true;
                }

                var _DeviceMipiLevel = new CustomMipiLevel(vih, Reconfigurable_DeviceMipiLevel.vil, Reconfigurable_DeviceMipiLevel.voh, Reconfigurable_DeviceMipiLevel.vol, Reconfigurable_DeviceMipiLevel.vtt);
                Reconfigurable_DeviceMipiLevel = _DeviceMipiLevel;

                allRffePins.DigitalLevels.ConfigureVoltageLevels(Reconfigurable_DeviceMipiLevel.vil, Reconfigurable_DeviceMipiLevel.vih, Reconfigurable_DeviceMipiLevel.vol, Reconfigurable_DeviceMipiLevel.voh, Reconfigurable_DeviceMipiLevel.vtt);
                trigPin.DigitalLevels.ConfigureVoltageLevels(0.0, 5.0, 0.5, 2.5, 5.0); // Set VST Trigger Channel to 5V logic.  VST's PFI0 VIH is 2.0V, absolute max is 5.5V
                #endregion

                return true;
            }

            public override string GetInstrumentInfo()
            {
                return string.Format("HSDIO{0} = {1} r{2}*{3}; ", Site, DIGI.Identity.InstrumentModel, DIGI.Identity.InstrumentFirmwareRevision, SerialNumber);
            }

            //public override bool FinishedLoading()
            //{
            //    return true;
            //}
            public override void shmoo(string REG_address)
            {
                double originalStrobePoint = this.StrobePoint;
                //double originalUserDelay = this.UserDelay;

                //double maxdelay = 25e-9;
                double maxstrobe = 175e-9; // (1.0 / ClockRate) * 8.0;
                //double delaystep = 1e-9;
                double strobestep = 1e-9;
                //Console.WriteLine("X-Axis: UserDelay 0nS to " + (Math.Round(maxdelay / 1e-9)).ToString() + "nS");
                Console.WriteLine("Y-Axis: StrobePoint 0nS to " + (Math.Round(maxstrobe / 1e-9)).ToString() + "nS");
                //Console.WindowHeight = Math.Min((int)(maxstrobe / strobestep) + 10, Console.LargestWindowHeight);
                //Console.WindowWidth = Math.Min((int)(maxdelay / delaystep + 2) * 5 + 5, Console.LargestWindowWidth);
                DigitalTimeSet tsNRZ = DIGI.Timing.GetTimeSet(Timeset.MIPI.ToString("g"));
                for (double compareStrobe = 0; compareStrobe < maxstrobe; compareStrobe += strobestep)
                {
                    tsNRZ.ConfigureCompareEdgesStrobe(sdata1Pin, Ivi.Driver.PrecisionTimeSpan.FromSeconds(compareStrobe));
                    tsNRZ.ConfigureCompareEdgesStrobe(sdata2Pin, Ivi.Driver.PrecisionTimeSpan.FromSeconds(compareStrobe));
                    DIGI.PatternControl.Commit();
                    Console.Write(Math.Round(compareStrobe / 1e-9).ToString().PadLeft(2, ' '));
                    //for (double delay = 0; delay < maxdelay; delay += delaystep)
                    string regValue = "";
                    //double delay = 0;
                    {
                        //DIGI.ConfigureUserDelay(HSDIO.SdataChanName.ToUpper(), delay);
                        regValue = this.RegRead(REG_address);
                        this.SendVector("FUNCTIONAL_RX");
                        // int errors = this.GetNumExecErrors("FUNCTIONAL_RX");

                        long[] Fails = sdata2Pin.GetFailCount();
                        long errors = Fails[0];


                        //Console.WriteLine((errors > 0 ? "FAIL: " : "PASS: ") + nameInMemory + " CableDelay: " + delay.ToString() + " -- Bit Errors: " + errors.ToString());
                        Console.BackgroundColor = (errors > 0 ? ConsoleColor.Red : ConsoleColor.Green);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        string errstr = "";
                        if (errors >= 1000000)
                        {
                            errstr = (errors / 1000000).ToString("D");
                            errstr = errstr.PadLeft(3, ' ') + "M ";
                        }
                        else if (errors >= 1000)
                        {
                            errstr = (errors / 1000).ToString("D");
                            errstr = errstr.PadLeft(3, ' ') + "K ";
                        }
                        else
                        {
                            errstr = errors.ToString("D");
                            errstr = errstr.PadLeft(4, ' ') + " ";
                        }
                        Console.Write((errors > 0 ? errstr : "     "));
                        Console.ResetColor();
                    }
                    Console.Write(regValue + "\n");
                }
                /*Console.Write("  ");
                for (double delay = 0; delay < maxdelay; delay += 1e-9)
                {
                    string str = (delay / 1e-9).ToString();
                    Console.Write(str.PadLeft(4,' ') + " ");
                }
                Console.Write("\n");*/

                //this.UserDelay = originalUserDelay;
                this.StrobePoint = originalStrobePoint;

                //DIGI.ConfigureUserDelay(HSDIO.SdataChanName.ToUpper(), originalUserDelay);
                tsNRZ.ConfigureCompareEdgesStrobe(sdata1Pin, Ivi.Driver.PrecisionTimeSpan.FromSeconds(originalStrobePoint));

            }

            private string FormatNumber(double number)
            {
                string stringRepresentation = number.ToString();

                if (stringRepresentation.Length > 5)
                    stringRepresentation = stringRepresentation.Substring(0, 5);

                if (stringRepresentation.Length == 5 && stringRepresentation.EndsWith("."))
                    stringRepresentation = stringRepresentation.Substring(0, 4);

                return stringRepresentation.PadLeft(5);
            }


            /// <summary>
            /// Load the specified vector file into Instrument Memory.
            /// Will automatically convert from .vec format as needed and load into instrument memory.
            /// </summary>
            /// <param name="fullPaths">A list of absolute paths to be loaded.  Currenlty only supports 1 item in the list.</param>
            /// <param name="nameInMemory">The name by which to load and execute the pattern.</param>
            /// <param name="datalogResults">Specifies if the pattern's results should be added to the datalog</param>
            /// <returns>True if pattern load succeeds.</returns>
            //public override bool LoadVector(List<string> fullPaths, string nameInMemory, bool datalogResults)   datalogResults was not being used KH         
            public override bool LoadVector(List<string> fullPaths, string nameInMemory)
            {
                // KEN H
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                bool success = false;
                bool isNeedLoadFullClk = true; // nameInMemory.EndsWith("Fullclk", StringComparison.OrdinalIgnoreCase);
                try
                {
                    nameInMemory = nameInMemory.Replace("_", "");
                    bool isDigipat = fullPaths[0].ToUpper().EndsWith(".DIGIPAT");
                    bool isVec = fullPaths[0].ToUpper().EndsWith(".VEC");
                    bool notLoaded = !loadedPatternFiles.Contains(fullPaths[0] + nameInMemory.ToLower());

                    // If this is a digipat file and it hasn't already been loaded into instrument memory, load it
                    if (isDigipat && notLoaded)
                    {
                        DIGI.LoadPattern(fullPaths[0]);
                        loadedPatternFiles.Add(fullPaths[0] + nameInMemory.ToLower());
                        loadedPatternNames.Add(nameInMemory.ToLower());
                        return true;
                    }
                    // If this is a vec file and it hasn't already been loading into instrument memory convert and then load it
                    else if (isVec && notLoaded)
                    {
                        // Cleanup old files from original release.  This should be a 1 time operation
                        var oldFileDir = new DirectoryInfo(Path.GetDirectoryName(fullPaths[0]));
                        string searchRegex = "*" + Path.GetFileNameWithoutExtension(fullPaths[0]) + "*.digipat*";
                        foreach (var oldFile in oldFileDir.EnumerateFiles(searchRegex)) { oldFile.Delete(); }


                        // Call NI Convert Vec and Load Pattern function.  All Vec files are NRZ
                        uint captureCount = 0;
                        //bool success = this.ConvertVecAndLoadPattern(fullPaths[0], nameInMemory.ToLower(), forceDigiPatRegeneration, Timeset.MIPI, ref captureCount, convertToCapture, true);


                        if (nameInMemory.Contains("QC"))
                        {
                            //success = this.ConvertVecAndLoadPattern(fullPaths[0], nameInMemory.ToLower(), EqHSDIO.forceQCVectorRegen, ref captureCount, Timeset.MIPI_RZ, Timeset.MIPI_RZ_HALF); //forceDigiPatRegeneration turned off for QC vector files
                            if (fullPaths[0].ToUpper().Contains("_NRZ_"))
                            {
                                //NRZ
                                success = this.ConvertVecAndLoadPattern(fullPaths[0], nameInMemory.ToLower(), EqHSDIO.forceQCVectorRegen, ref captureCount, timeSet: isNeedLoadFullClk ? Timeset.MIPI : Timeset.MIPI_HALF,
                                    timeSet_read: isNeedLoadFullClk ? Timeset.MIPI : Timeset.MIPI_HALF);
                            }
                            else
                            {
                                //RZ
                                success = this.ConvertVecAndLoadPattern(fullPaths[0], nameInMemory.ToLower(), EqHSDIO.forceQCVectorRegen, ref captureCount, timeSet: isNeedLoadFullClk ? Timeset.MIPI_RZ : Timeset.MIPI_RZ_HALF,
                                   timeSet_read: isNeedLoadFullClk ? Timeset.MIPI_RZ_HALF : Timeset.MIPI_RZ_QUAD);
                            }
                        }
                        else
                        {
                            success = this.ConvertVecAndLoadPattern(fullPaths[0], nameInMemory.ToLower(), forceDigiPatRegeneration, ref captureCount, Timeset.MIPI_RZ, Timeset.MIPI_RZ_HALF);
                        }

                        //bool success = this.ConvertVecAndLoadPattern(fullPaths[0], nameInMemory.ToLower(), forceDigiPatRegeneration, ref captureCount, Timeset.MIPI_RZ , Timeset.MIPI_RZ_HALF);



                        if (success)
                        {
                            // Store capture counter so that patterns that were converted to capture can know how much to read back later
                            captureCounters.Add(nameInMemory.Replace("_", "").ToUpper(), captureCount);

                            // If the convert and load succeeded, add the vector to the loaded files list
                            // Do this based on filepath & pattern name, as some .vec files are associated
                            // with multiple pattern names and should be loaded multiple times.
                            loadedPatternFiles.Add(fullPaths[0] + nameInMemory.ToLower());
                            loadedPatternNames.Add(nameInMemory.ToLower());
                            return true;
                        }
                        else
                        {
                            throw new Exception("Load Vector failed for " + nameInMemory);
                        }
                    }
                    else
                    {
                        throw new Exception("Unknown File Format for " + nameInMemory);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load vector file:\n" + fullPaths[0] + "\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Generate and load pattern for setting all pins (SCLK, SDATA, VIO) to HiZ mode
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public override bool LoadVector_MipiHiZ()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate vector that will set all pins to High Z for 8 clock cycles.
                    // This is done by not sourcing or comparing any data.  This works because the instrument is configured
                    // to be in the High Z termination mode in the 6570 init section.
                    string[] pins;
                    string[,] pattern;

                    if (Num_Mipi_Bus == 2)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                        pattern = new string[,]
                        { 
                        #region HiZ pattern
                        {"", "0", "0", "1","0", "0", "1", "1", ""},
                        {"", "0", "0", "1","0", "0", "1","1", ""},
                        {"", "0", "0", "1","0", "0", "1", "1", ""},
                        {"", "0", "0", "1","0", "0", "1", "1", ""},
                        {"", "0", "0", "1","0", "0", "1", "1", ""},
                        {"", "0", "0", "1","0", "0", "1", "1", ""},
                        {"", "0", "0", "1","0", "0", "1", "1", ""},
                        {"halt", "0", "0", "1","0", "0", "1", "1", ""}
                        #endregion
                        };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                        pattern = new string[,]
                        { 
                        #region HiZ pattern
                        {"", "0", "0", "1", "1", ""},
                        {"", "0", "0", "1", "1", ""},
                        {"", "0", "0", "1", "1", ""},
                        {"", "0", "0", "1", "1", ""},
                        {"", "0", "0", "1", "1", ""},
                        {"", "0", "0", "1", "1", ""},
                        {"", "0", "0", "1", "1",  ""},
                        {"halt", "0", "0", "1", "1", ""}
                        #endregion
                        };
                    }

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern(HiZ.ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.MIPI))
                    {
                        throw new Exception("Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi HiZ vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Generate and load pattern for sending MIPI Reset
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public override bool LoadVector_MipiReset()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Reset Waveform
                    // Set VIO Pin to HiZ (remove VIO Pin from DUT) for 1/2 of the specified number of seconds,
                    // then return VIO to DUT as 0;
                    double secondsReset = 0.002;
                    int numLines = (int)(MIPIClockRate * secondsReset);

                    string[] pins;
                    string[,] pattern;

                    if (Num_Mipi_Bus == 2)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                        pattern = new string[,]
                        { 
                            #region Reset pattern
                            {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "0", "0", "0", "0","1",""},
                            {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "1", "0", "0", "1","1",""},
                            {"halt", "0", "0", "1", "0", "0", "1","1",""}
                            #endregion
                        };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                        pattern = new string[,]
                        { 
                            #region Reset pattern
                            {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "0", "1", ""},
                            {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "1", "1", ""},
                            {"halt", "0", "0", "1","1", ""}
                            #endregion
                        };
                    }

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern(Reset.ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.MIPI))
                    {
                        throw new Exception("Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi Reset vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Generate and load pattern for sending MIPI VIO Off
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public override bool LoadVector_MipiVioOff()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE VIO Off Waveform

                    double secondsReset = 0.001;
                    int numLines = (int)(MIPIClockRate * secondsReset);
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                    string[,] pattern = new string[,]
                    { 
                    #region Reset pattern
                        {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "0", "0", "0", "0", "1", ""},
                        {"halt", "0", "0", "0", "0", "0", "0", "1",""}
                    #endregion
                    };

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern(VioOff.ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.MIPI))
                    {
                        throw new Exception("Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi VIO Off vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            public override bool LoadVector_MipiVioOn()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE VIO Off Waveform

                    double secondsReset = 0.001;
                    int numLines = (int)(MIPIClockRate * secondsReset);
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                    string[,] pattern = new string[,]
                    { 
                    #region Reset pattern
                        {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "1",  "0", "0", "1", "1", ""},
                        {"halt", "0", "0", "1",  "0", "0", "1",  "1",""}
                    #endregion
                    };

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern(VioOn.ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.MIPI))
                    {
                        throw new Exception("Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi VIO On vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Generate and load all patterns necessary for Register Read and Write (including extended R/W)
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public override bool LoadVector_MipiRegIO()
            {
                bool loadvectorPassflag = true;
                double ClkRate_byTimeset = MIPIClockRate;

                Timeset _Wtimeset = (isRZ ? Timeset.MIPI_RZ : Timeset.MIPI);
                Timeset _Rtimeset = (isRZ ? Timeset.MIPI_RZ_HALF : Timeset.MIPI_HALF);

                if (_Wtimeset.ToString().Contains("HALF") | _Rtimeset.ToString().Contains("HALF"))
                {
                    ClkRate_byTimeset = MIPIClockRate / 2;
                }
                if (_Wtimeset.ToString().Contains("QUAD") | _Rtimeset.ToString().Contains("QUAD"))
                {
                    ClkRate_byTimeset = MIPIClockRate / 4;
                }

                if (!isShareBus)
                {
                    loadvectorPassflag &= LoadVector_MipiRegWrite(_Wtimeset);
                    loadvectorPassflag &= LoadVector_MipiRegRead(Timeset.MIPI_RZ_HALF, Timeset.MIPI_RZ_QUAD);
                    loadvectorPassflag &= LoadVector_MipiExtendedRegWrite(_Wtimeset);
                    loadvectorPassflag &= LoadVector_MipiExtendedRegRead(Timeset.MIPI_RZ_HALF, Timeset.MIPI_RZ_QUAD);
                    loadvectorPassflag &= LoadVector_MultipleMipiExtendedRegWrite(_Wtimeset);
                    loadvectorPassflag &= LoadVector_MultipleMipiExtendedRegWriteWithReg(_Wtimeset);
                    loadvectorPassflag &= LoadVector_TimingTest(_Wtimeset);
                    loadvectorPassflag &= LoadVector_BurstingTest(_Wtimeset);
                    loadvectorPassflag &= LoadVector_TxOTPBurnTemplate(_Rtimeset, ClkRate_byTimeset);
                    loadvectorPassflag &= LoadVector_RxOTPBurnTemplate(_Rtimeset, ClkRate_byTimeset);
                }
                else
                {
                    for (int pair = 1; pair < 3; pair++)
                    {
                        loadvectorPassflag &= LoadVector_MipiRegWrite(_Wtimeset, pair);
                        loadvectorPassflag &= LoadVector_MipiRegRead(Timeset.MIPI_RZ, Timeset.MIPI_RZ_HALF, pair);
                        loadvectorPassflag &= LoadVector_MipiExtendedRegWrite(_Wtimeset, pair);
                        loadvectorPassflag &= LoadVector_MipiExtendedRegRead(Timeset.MIPI_RZ, Timeset.MIPI_RZ_HALF, pair);
                        loadvectorPassflag &= LoadVector_MultipleMipiExtendedRegWrite(_Wtimeset, pair);
                        loadvectorPassflag &= LoadVector_MultipleMipiExtendedRegWriteWithReg(_Wtimeset, pair);
                        loadvectorPassflag &= LoadVector_MultipleMipiExtendedRegMaskedWriteWithReg(_Wtimeset, pair);
                    }
                    //below items need to change 
                    loadvectorPassflag &= LoadVector_TimingTest(_Wtimeset);
                    loadvectorPassflag &= LoadVector_BurstingTest(_Wtimeset);
                    loadvectorPassflag &= LoadVector_TxOTPBurnTemplate(_Rtimeset, ClkRate_byTimeset, 1);
                    loadvectorPassflag &= LoadVector_RxOTPBurnTemplate(_Rtimeset, ClkRate_byTimeset, 2);

                }

                return loadvectorPassflag;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the non-extended register read pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiRegRead(Timeset WriteTimeSet, Timeset ReadTimeset, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Non-Extended Register Read Pattern
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }
                    List<string[]> pattern = new List<string[]>
                            {
                            #region RegisterRead pattern
                                new string[] { "source_start(SrcRegisterRead" + PairString + ")", "0", "0", "1", "X", "Configure source"},
                                new string[] {"capture_start(CapRegisterRead)", "0", "0", "1", "X", "Configure capture"},
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Register Read Command (011)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Register Read Command (011)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Register Read Command (011)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "X", "Pull Down Only"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 7"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 6"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 5"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 4"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 3"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 2"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 1"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 0"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Parity"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "X", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"capture_stop", "0", "0", "1", "X", "stop capture"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                    if (isRZ) pattern = RemoveNRZvector(pattern);
                    pattern = Pairsperator(_pair, pattern);
                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("RegisterRead" + PairString, pins, pattern, true, WriteTimeSet, ReadTimeset))
                    {
                        throw new Exception("Compile Failed");
                    }

                    if (!this.GenerateAndLoadPattern("OTPRegisterRead" + PairString, pins, pattern, true, ReadTimeset)) // OTP Read w/ half rate & Full Speed 
                    {
                        throw new Exception("Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegisterRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the non-extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiRegWrite(Timeset WriteTimeSet, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Non-Extended Register Write Pattern
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }
                    string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.PXI_Backplane ? "1" : "0");
                    List<string[]> patternStart = new List<string[]>
                            {
                            #region RegisterWrite Pattern
                                new string[] {"source_start(SrcRegisterWrite"+PairString+")", "0", "0", "1", "0", "Configure source"},
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Register Write Command (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                    List<string[]> triggerDelay = new List<string[]>
                            {
                                new string[] {"", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };
                    List<string[]> trigger = new List<string[]>
                            {
                            #region Trigger, Idle Halt
                                new string[] {"jump_if(!seqflag0, endofpattern)", "0", "0", "1", "0", "Check if Trigger Required, if not, go to halt."},
                                new string[] {"set_signal(event0)", "0", "0", "1", trigval, "Turn On PXI Backplane Trigger if enabled. Send Digital Pin Trigger if enabled."},
                                new string[] {"repeat(10)", "0", "0", "1", trigval, "Continue Sending Digital Pin Trigger if enabled."},
                                new string[] {"clear_signal(event0)", "0", "0", "1", "0", "PXI Backplane Trigger Off (if enabled). Digital Pin Trigger Off."},
                                new string[] {"repeat(10)", "0", "0", "1", "0", "Digital Pin Trigger Off."},
                                new string[] {"", "0", "0", "1", "X", "Digital Pin Trigger Tristate."},
                                new string[] {"endofpattern:\nrepeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                    // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                    List<string[]> pattern = new List<string[]> { };
                    pattern = pattern.Concat(patternStart).ToList();

                    for (int ff = 0; ff < this.regWriteTriggerCycleDelay; ff++)
                        pattern = pattern.Concat(triggerDelay).ToList();

                    pattern = pattern.Concat(trigger).ToList();


                    if (isRZ) pattern = RemoveNRZvector(pattern);
                    pattern = Pairsperator(_pair, pattern);

                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("RegisterWrite" + PairString, pins, pattern, true, WriteTimeSet))
                    {
                        throw new Exception("Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiExtendedRegWrite(Timeset WriteTimeSet, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Extended Register Write Patterns
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }
                    string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                    List<string[]> writeData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                    List<string[]> triggerDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };
                    List<string[]> trigger = new List<string[]>
                            {
                            #region Trigger, Idle Halt
                                new string[] {"jump_if(!seqflag0, endofpattern)", "0", "0", "1", "0", "Check if Trigger Required, if not, go to halt."},
                                new string[] {"set_signal(event0)", "0", "0", "1", trigval, "Turn On PXI Backplane Trigger if enabled. Send Digital Pin Trigger if enabled."},
                                new string[] {"repeat(49)", "0", "0", "1", trigval, "Continue Sending Digital Pin Trigger if enabled."},
                                new string[] {"clear_signal(event0)", "0", "0", "1", "0", "PXI Backplane Trigger Off (if enabled). Digital Pin Trigger Off."},
                                new string[] {"repeat(49)", "0", "0", "1", "0", "Digital Pin Trigger Off."},
                                new string[] {"", "0", "0", "1", "X", "Digital Pin Trigger Tristate."},
                                new string[] {"endofpattern:\nrepeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                    for (int i = 1; i <= 1; i++)
                    //for (int i = 1; i <= 16; i++)
                    {
                        // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                        List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcExtendedRegisterWrite" + i + PairString + ")", "0", "0", "1", "0", "Configure source"}
                                };
                        pattern = pattern.Concat(patternStart).ToList();

                        for (int j = 0; j < i; j++)
                            pattern = pattern.Concat(writeData).ToList();

                        pattern = pattern.Concat(busPark).ToList();

                        for (int ff = 0; ff < this.regWriteTriggerCycleDelay; ff++)
                            pattern = pattern.Concat(triggerDelay).ToList();

                        pattern = pattern.Concat(trigger).ToList();


                        if (isRZ) pattern = RemoveNRZvector(pattern);
                        pattern = Pairsperator(_pair, pattern);
                        // Generate and load Pattern from the formatted array.
                        // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                        if (!this.GenerateAndLoadPattern("ExtendedRegisterWrite" + i.ToString() + PairString, pins, pattern, true, WriteTimeSet))
                        {
                            throw new Exception("Compile Failed: ExtendedRegisterWrite" + i.ToString());
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MultipleMipiExtendedRegWrite(Timeset WriteTimeSet, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Extended Register Write Patterns
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }
                    string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                    List<string[]> writeData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                    //List<string[]> triggerDelay = new List<string[]>
                    //        {
                    //            new string[] { "", "0", "0", "1", "0", "Trigger Delay Cycle" }
                    //        };
                    //List<string[]> trigger = new List<string[]>
                    //        {
                    //        #region Trigger, Idle Halt
                    //            new string[] {"jump_if(!seqflag0, endofpattern)", "0", "0", "1", "0", "Check if Trigger Required, if not, go to halt."},
                    //            new string[] {"set_signal(event0)", "0", "0", "1", trigval, "Turn On PXI Backplane Trigger if enabled. Send Digital Pin Trigger if enabled."},
                    //            new string[] {"repeat(49)", "0", "0", "1", trigval, "Continue Sending Digital Pin Trigger if enabled."},
                    //            new string[] {"clear_signal(event0)", "0", "0", "1", "0", "PXI Backplane Trigger Off (if enabled). Digital Pin Trigger Off."},
                    //            new string[] {"repeat(49)", "0", "0", "1", "0", "Digital Pin Trigger Off."},
                    //            new string[] {"", "0", "0", "1", "X", "Digital Pin Trigger Tristate."},
                    //            new string[] {"endofpattern:\nrepeat(300)", "0", "0", "1", "X", "Idle"},
                    //            new string[] {"halt", "0", "0", "1", "X", ""}
                    //        #endregion
                    List<string[]> Idle = new List<string[]>
                            {
                            #region  Idle
                                new string[] {"repeat(30)", "0", "0", "1", "0", "Idle"},

                            #endregion
                            };



                    List<string[]> Halt = new List<string[]>
                            {
                            #region  Halt
                                new string[] {"endofpattern:\nrepeat(1000)", "0", "0", "1", "X", "Idle"},  //200us delay to allow trigger command to take effect in CMOS controller
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };


                    for (int numOfWrites = 1; numOfWrites <= 1; numOfWrites++)
                    {

                        // concat pieces into a single Register write 
                        List<string[]> pattern = new List<string[]> { };
                        pattern = pattern.Concat(patternStart).ToList();

                        pattern = pattern.Concat(writeData).ToList();

                        pattern = pattern.Concat(busPark).ToList();

                        pattern = pattern.Concat(Idle).ToList();



                        // Generate full pattern with the correct number of MIPI register writes
                        List<string[]> multiWritePattern = new List<string[]>
                        {
                            new string[] {"source_start(SrcMultipleExtendedRegisterWrite" + numOfWrites + PairString + ")", "0", "0", "1", "0", "Configure source"}
                        };

                        for (int writeNumber = 1; writeNumber <= numOfWrites; writeNumber++)
                        {
                            multiWritePattern = multiWritePattern.Concat(pattern).ToList();
                        }

                        multiWritePattern = multiWritePattern.Concat(Halt).ToList();


                        if (isRZ) multiWritePattern = RemoveNRZvector(multiWritePattern);
                        multiWritePattern = Pairsperator(_pair, multiWritePattern);

                        // Generate and load Pattern from the formatted array.
                        // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                        if (!this.GenerateAndLoadPattern("MultipleExtendedRegisterWrite" + numOfWrites.ToString() + PairString, pins, multiWritePattern, true, WriteTimeSet))
                        {
                            throw new Exception("Compile Failed: MultipleExtendedRegisterWrite" + numOfWrites.ToString());
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register read pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiExtendedRegRead(Timeset WriteTimeSet, Timeset ReadTimeset, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Extended Register Read Patterns
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }
                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterRead Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "X", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                    List<string[]> readData = new List<string[]>
                            {
                            #region Read Data...
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 7"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 6"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 5"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 4"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 3"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 2"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 1"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 0"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Parity"},
                                new string[] {"", "0", "X", "1", "-", ""}
                            #endregion
                            };
                    List<string[]> busParkIdleHalt = new List<string[]>
                            {
                            #region Bus Park, Idle, and Halt
                                new string[] {"", "1", "0", "1", "X", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"capture_stop", "0", "0", "1", "X", "stop capture"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                    for (int i = 1; i <= 1; i++)
                    //   for (int i = 1; i <= 16; i++)
                    {
                        // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                        List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcExtendedRegisterRead" + i + PairString + ")", "0", "0", "1", "X", "Configure source"},
                                    new string[] {"capture_start(CapExtendedRegisterRead" + i + ")", "0", "0", "1", "X", "Configure capture"}
                                };
                        pattern = pattern.Concat(patternStart).ToList();
                        for (int j = 0; j < i; j++)
                            pattern = pattern.Concat(readData).ToList();
                        pattern = pattern.Concat(busParkIdleHalt).ToList();

                        if (isRZ) pattern = RemoveNRZvector(pattern);
                        pattern = Pairsperator(_pair, pattern);

                        // Generate and load Pattern from the formatted array.
                        // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                        if (!this.GenerateAndLoadPattern("ExtendedRegisterRead" + i.ToString() + PairString, pins, pattern, true, WriteTimeSet, ReadTimeset))
                        {
                            throw new Exception("Compile Failed: ExtendedRegisterRead" + i.ToString());
                        }

                        if (!this.GenerateAndLoadPattern("OTPExtendedRegisterRead" + i.ToString() + PairString, pins, pattern, true, ReadTimeset, ReadTimeset)) // OTP Read w/ half rate & half Speed 
                        {
                            throw new Exception("Compile Failed: ExtendedRegisterRead" + i.ToString());
                        }
                        //EqHSDIO.datalogResults["ExtendedRegisterRead" + i.ToString()] = false;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Not Implemented yet
            /// </summary>
            /// <returns>False</returns>
            public override bool LoadVector_EEPROM()
            {
                return LoadVector_EEPROMRead() & LoadVector_EEPROMWrite();// &LoadVector_EEPROMEraseWriteEnable();
            }
            /// <summary>
            /// Internal Function: Used to generate and load the EEPROM read pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_EEPROMRead()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string[] pins = new string[] { I2CSCKChanName.ToUpper(), I2CSDAChanName.ToUpper(), I2CVCCChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcEEPROMRead)", "1", "1", "1", "Configure source" });
                    pattern.Add(new string[] { "capture_start(CapEEPROMRead)", "1", "1", "1", "Configure capture" });
                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Start Condition" });
                    pattern.Add(new string[] { "", "0", "0", "1", "Start Condition" });

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "source", "1", "D", "1", "Write Register Address" });
                        pattern.Add(new string[] { "", "0", "-", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    pattern.Add(new string[] { "", "1", "1", "1", "Start Condition" });
                    pattern.Add(new string[] { "", "0", "0", "1", "Start Condition" });

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Read Device (10101001)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "capture", "1", "V", "1", "Read Register Data" });
                        pattern.Add(new string[] { "", "0", "X", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "H", "1", "NO ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    pattern.Add(new string[] { "", "1", "0", "1", "Stop Condition" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Stop Condition" });

                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });

                    pattern.Add(new string[] { "capture_stop", "1", "1", "1", "" });
                    pattern.Add(new string[] { "halt", "1", "1", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("EEPROMRead", pins, pattern, forceDigiPatRegeneration, Timeset.EEPROM, Timeset.EEPROM))
                    {
                        throw new Exception("EEPROMRead Compile Failed");
                    }

                    //EqHSDIO.datalogResults["EEPROMRead"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load EEPROMRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            /// <summary>
            /// Internal Function: Used to generate and load the EEPROM Write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_EEPROMWrite()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string[] pins = new string[] { I2CSCKChanName.ToUpper(), I2CSDAChanName.ToUpper(), I2CVCCChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    pattern.Add(new string[] { "source_start(SrcEEPROMWrite)", "1", "1", "1", "Configure source" });
                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Start Condition" });
                    pattern.Add(new string[] { "", "0", "0", "1", "Start Condition" });

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "1 - Write Device (10101000)" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "source", "1", "D", "1", "Write Register Address" });
                        pattern.Add(new string[] { "", "0", "-", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "source", "1", "D", "1", "Write Data" });
                        pattern.Add(new string[] { "", "0", "-", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    pattern.Add(new string[] { "", "1", "0", "1", "Stop Condition" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Stop Condition" });

                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });

                    pattern.Add(new string[] { "halt", "1", "1", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern("EEPROMWrite", pins, pattern, forceDigiPatRegeneration, Timeset.EEPROM, Timeset.EEPROM))
                    {
                        throw new Exception("EEPROMWrite Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load EEPROMRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            /// <summary>
            /// Internal Function: Used to generate and load the EEPROM EraseWriteEnable pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_EEPROMEraseWriteEnable()
            {
                throw new NotImplementedException();
                /*
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string[] pins = new string[] { DiChanName.ToUpper(), DoChanName.ToUpper(), VccChanName.ToUpper(), CsChanName.ToUpper(), SkChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "repeat(300)", "0", "0", "1", "0", "0", "Idle" });
                    pattern.Add(new string[] { "", "1", "X", "1", "1", "1", "CS goes high, clock starts" });
                    pattern.Add(new string[] { "", "1", "X", "1", "1", "0", "" });
                    pattern.Add(new string[] { "", "0", "X", "1", "1", "1", "0 - EraseWriteEnable Command (00)" });
                    pattern.Add(new string[] { "", "0", "X", "1", "1", "0", "" });
                    pattern.Add(new string[] { "", "0", "X", "1", "1", "1", "1" });
                    pattern.Add(new string[] { "", "0", "X", "1", "1", "0", "" });
                    for (int i = 0; i < 7; i++)
                    {
                        pattern.Add(new string[] { "", "0", "X", "1", "1", "1", "Register Address" });
                        pattern.Add(new string[] { "", "-", "X", "1", "1", "0", "" });
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        pattern.Add(new string[] { "", "1", "X", "1", "1", "1", "Register Address" });
                        pattern.Add(new string[] { "", "-", "X", "1", "1", "0", "" });
                    }
                    pattern.Add(new string[] { "", "0", "X", "1", "1", "1", "Last Register Address Bit always 0" });
                    pattern.Add(new string[] { "", "0", "X", "1", "1", "0", "" });
                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "", "X", "X", "1", "1", "1", "Data Tristate" });
                        pattern.Add(new string[] { "", "-", "X", "1", "1", "0", "" });
                    }
                    pattern.Add(new string[] { "", "X", "X", "1", "1", "1", "Tristate DI" });
                    pattern.Add(new string[] { "", "X", "X", "1", "1", "0", "" });
                    pattern.Add(new string[] { "", "0", "0", "1", "0", "0", "CS goes low, clock stops" });
                    pattern.Add(new string[] { "repeat(300)", "0", "0", "1", "0", "0", "Idle" });
                    pattern.Add(new string[] { "halt", "0", "0", "1", "0", "0", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: EEPROMEraseWriteEnable is stored in lowercase so we can call it from SendVector().
                    if (!this.GenerateAndLoadPattern("EEPROMEraseWriteEnable".ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.EEPROM))
                    {
                        throw new Exception("EEPROMEraseWriteEnable Compile Failed");
                    }

                    HSDIO.datalogResults["EEPROMEraseWriteEnable"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load EEPROMRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
                 */
            }

            #region EEPROM UNI/O
            // UNIO
            public override bool LoadVector_UNIO_EEPROM()
            {
                return
                    LoadVector_UNIO_EEPROM_Discovery(1) &
                    LoadVector_UNIO_EEPROM_Write(1) &
                    LoadVector_UNIO_EEPROM_ReadID(1) &
                    LoadVector_UNIO_EEPROM_ReadCounter(1) &
                    LoadVector_UNIO_EEPPROM_ReadSerialNumber(1) &
                    LoadVector_UNIO_EEPROM_ReadMID(1) &
                    LoadVector_UNIO_EEPROM_Discovery(2) &
                    LoadVector_UNIO_EEPROM_Write(2) &
                    LoadVector_UNIO_EEPROM_ReadID(2) &
                    LoadVector_UNIO_EEPROM_ReadCounter(2) &
                    LoadVector_UNIO_EEPPROM_ReadSerialNumber(2) &
                    LoadVector_UNIO_EEPROM_ReadMID(2);
            }

            private bool LoadVector_UNIO_EEPROM_Discovery(int bus_no = 1)
            {
                try
                {
                    string[] pins;

                    if (bus_no == 2)
                        pins = new string[] { UNIOSIO2ChanName.ToUpper(), UNIOVPUP2ChanName.ToUpper() };
                    else
                        pins = new string[] { UNIOSIOChanName.ToUpper(), UNIOVPUPChanName.ToUpper() };

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcEEPROMUNIODiscovery" + bus_no.ToString() + ")", "1", "1", "Configure Source" });
                    //pattern.Add(new string[] { "repeat(300)", "1", "1", "Power up 200us" });
                    pattern.Add(new string[] { "repeat(400)", "1", "1", "Power up 200us" });
                    //pattern.Add(new string[] { "repeat(500)", "0", "1", "tRESET/tDSCHG 480us - Reset/Discharge Low Time" });
                    pattern.Add(new string[] { "repeat(600)", "0", "1", "tRESET/tDSCHG 480us - Reset/Discharge Low Time" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "tRRT 10us - Reset Recovery Time" });
                    pattern.Add(new string[] { "", "0", "1", "tDRR 1us - Discovery Response Request" });
                    pattern.Add(new string[] { "repeat(3)", "X", "1", "Master Sampling Window" });
                    pattern.Add(new string[] { "", "L", "1", "Master Sampling Window" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });
                    pattern.Add(new string[] { "halt", "1", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("EEPROMUNIODiscovery" + bus_no.ToString(), pins, pattern, true, Timeset.UNIO_EEPROM))
                    {
                        throw new Exception("EEPROMUNIODiscovery Compilation Failed");
                    }

                    //HSDIO.datalogResults["EEPROMUNIODiscovery"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load EEPROMUNIODiscovery vector\n\n" + e.ToString(), "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_UNIO_EEPROM_Write(int bus_no = 1)
            {
                try
                {
                    string[] pins;

                    if (bus_no == 2)
                        pins = new string[] { UNIOSIO2ChanName.ToUpper(), UNIOVPUP2ChanName.ToUpper() };
                    else
                        pins = new string[] { UNIOSIOChanName.ToUpper(), UNIOVPUPChanName.ToUpper() };

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcUNIOEEPROMUNIOWrite" + bus_no.ToString() + ")", "1", "1", "Configure source" });

                    pattern.Add(new string[] { "repeat(400)", "X", "1", "Start Condition" });

                    for (int i = 0; i < 3; i++)
                    {
                        for (int bit = 7; bit >= 0; bit--)
                        {
                            pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                            for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                            {
                                pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                            }
                            pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                        }

                        pattern.Add(new string[] { "", "0", "1", "ACK" });
                        //pattern.Add(new string[] { "repeat(2)", "X", "1", "ACK" });
                        pattern.Add(new string[] { "", "X", "1", "ACK" });
                        pattern.Add(new string[] { "", "L", "1", "ACK" });
                        pattern.Add(new string[] { "repeat(8)", "X", "1", "ACK" });
                    }

                    pattern.Add(new string[] { "repeat(5200)", "X", "1", "Stop Condition + tWR 150us" });
                    pattern.Add(new string[] { "halt", "X", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Register read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("UNIOEEPROMUNIOWrite" + bus_no.ToString(), pins, pattern, true, Timeset.UNIO_EEPROM))
                    {
                        throw new Exception("UNIOEEPROMUNIOWrite Compilation Failed");
                    }

                    return true;
                }
                catch { }
                return false;
            }

            private bool LoadVector_UNIO_EEPROM_ReadID(int bus_no = 1)
            {
                try
                {
                    string[] pins;

                    if (bus_no == 2)
                        pins = new string[] { UNIOSIO2ChanName.ToUpper(), UNIOVPUP2ChanName.ToUpper() };
                    else
                        pins = new string[] { UNIOSIOChanName.ToUpper(), UNIOVPUPChanName.ToUpper() };

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcUNIOEEPROMUNIOread" + bus_no.ToString() + ")", "1", "1", "Configure source" });
                    pattern.Add(new string[] { "capture_start(CapUNIOEEPROMUNIOread" + bus_no.ToString() + ")", "1", "1", "Configure capture" });

                    // dummy write
                    pattern.Add(new string[] { "repeat(160)", "X", "1", "Start Condition" });

                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }

                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "repeat(9)", "0", "1", "Start from Memory Address 0" });
                        pattern.Add(new string[] { "repeat(3)", "1", "1", "Start from Memory Address 0" });
                    }
                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    // read
                    pattern.Add(new string[] { "repeat(350)", "X", "1", "Stop/Start Condition" });

                    // read command
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }

                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    for (int addr = 0; addr < 19; addr++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                            pattern.Add(new string[] { "", "X", "1", "tPUP" });
                            pattern.Add(new string[] { "capture", "V", "1", "Read - Address " + addr.ToString() + " - Bit " + (7 - i).ToString() });
                            pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                        }
                        pattern.Add(new string[] { "repeat(9)", "0", "1", "ACK - Address " + addr.ToString() });
                        pattern.Add(new string[] { "repeat(3)", "1", "1", "ACK - Address " + addr.ToString() });
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                        pattern.Add(new string[] { "", "X", "1", "tPUP" });
                        pattern.Add(new string[] { "capture", "V", "1", "Read - Address 20 - Bit " + (7 - i).ToString() });
                        pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                    }
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "NACK - Address 20" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "NACK - Address 20" });

                    pattern.Add(new string[] { "repeat(200)", "X", "1", "Stop Condition" });

                    pattern.Add(new string[] { "capture_stop", "X", "1", "" });
                    pattern.Add(new string[] { "halt", "X", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Register read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("UNIOEEPROMUNIOReadID" + bus_no.ToString(), pins, pattern, true, Timeset.UNIO_EEPROM))
                    {
                        throw new Exception("UNIOEEPROMUNIOReadID Compilation Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load UNIOEEPROMUNIOReadID vector\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_UNIO_EEPROM_ReadCounter(int bus_no = 1)
            {
                try
                {
                    string[] pins;

                    if (bus_no == 2)
                        pins = new string[] { UNIOSIO2ChanName.ToUpper(), UNIOVPUP2ChanName.ToUpper() };
                    else
                        pins = new string[] { UNIOSIOChanName.ToUpper(), UNIOVPUPChanName.ToUpper() };

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcUNIOEEPROMUNIOread" + bus_no.ToString() + ")", "1", "1", "Configure source" });
                    pattern.Add(new string[] { "capture_start(CapUNIOEEPROMUNIOread" + bus_no.ToString() + ")", "1", "1", "Configure capture" });

                    // dummy write
                    pattern.Add(new string[] { "repeat(160)", "X", "1", "Start Condition" });

                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }

                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    // start from memory address 125
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "Start from Memory Address 125" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "Start from Memory Address 125" });
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });
                    pattern.Add(new string[] { "repeat(9)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(3)", "1", "1", "" });
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "" });

                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    // read
                    pattern.Add(new string[] { "repeat(350)", "X", "1", "Stop/Start Condition" });

                    // read command
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }

                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    for (int addr = 0; addr < 2; addr++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                            pattern.Add(new string[] { "", "X", "1", "tPUP" });
                            pattern.Add(new string[] { "capture", "V", "1", "Read - Address " + (125 + addr).ToString() + " - Bit " + (7 - i).ToString() });
                            pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                        }
                        pattern.Add(new string[] { "repeat(9)", "0", "1", "ACK - Address " + (125 + addr).ToString() });
                        pattern.Add(new string[] { "repeat(3)", "1", "1", "ACK - Address " + (125 + addr).ToString() });
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                        pattern.Add(new string[] { "", "X", "1", "tPUP" });
                        pattern.Add(new string[] { "capture", "V", "1", "Read - Address 127 - Bit " + (7 - i).ToString() });
                        pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                    }
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "NACK - Address 127" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "NACK - Address 127" });

                    pattern.Add(new string[] { "repeat(200)", "X", "1", "Stop Condition" });

                    pattern.Add(new string[] { "capture_stop", "X", "1", "" });
                    pattern.Add(new string[] { "halt", "X", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Register read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("UNIOEEPROMUNIOReadCounter" + bus_no.ToString(), pins, pattern, true, Timeset.UNIO_EEPROM))
                    {
                        throw new Exception("UNIOEEPROMUNIOReadCounter Compilation Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load UNIOEEPROMUNIOReadCounter vector\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_UNIO_EEPPROM_ReadSerialNumber(int bus_no = 1)
            {
                try
                {
                    string[] pins;

                    if (bus_no == 2)
                        pins = new string[] { UNIOSIO2ChanName.ToUpper(), UNIOVPUP2ChanName.ToUpper() };
                    else
                        pins = new string[] { UNIOSIOChanName.ToUpper(), UNIOVPUPChanName.ToUpper() };

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString() + ")", "1", "1", "Configure source" });
                    pattern.Add(new string[] { "capture_start(CapUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString() + ")", "1", "1", "Configure capture" });

                    pattern.Add(new string[] { "repeat(300)", "X", "1", "Start Condition" });

                    // dummy write
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }
                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(2)", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(8)", "X", "1", "ACK" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "repeat(9)", "0", "1", "Start from Memory Address 0" });
                        pattern.Add(new string[] { "repeat(3)", "1", "1", "Start from Memory Address 0" });
                    }
                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    // read
                    pattern.Add(new string[] { "repeat(350)", "X", "1", "Stop/Start Condition" });

                    // read command
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }

                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(9)", "X", "1", "ACK" });

                    for (int addr = 0; addr < 7; addr++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                            pattern.Add(new string[] { "", "X", "1", "tPUP - 1us" });
                            pattern.Add(new string[] { "capture", "V", "1", "Read - Address " + (addr + 1).ToString() + " - Bit " + (7 - i).ToString() });
                            pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                        }
                        pattern.Add(new string[] { "repeat(9)", "0", "1", "ACK - SN Byte" + (addr + 1).ToString() });
                        pattern.Add(new string[] { "repeat(3)", "1", "1", "ACK - SN Byte" + (addr + 1).ToString() });
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                        pattern.Add(new string[] { "", "X", "1", "tPUP - 1us" });
                        pattern.Add(new string[] { "capture", "V", "1", "Read - CRC - Bit " + (7 - i).ToString() });
                        pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                    }
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "NACK - CRC" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "NACK - CRC" });

                    pattern.Add(new string[] { "repeat(200)", "X", "1", "Stop Condition" });

                    pattern.Add(new string[] { "capture_stop", "X", "1", "" });
                    pattern.Add(new string[] { "halt", "X", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Register read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("UNIOEEPROMUNIOReadSerialNumber" + bus_no.ToString(), pins, pattern, true, Timeset.UNIO_EEPROM))
                    {
                        throw new Exception("UNIOEEPROMUNIOReadSerialNumber Compilation Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load UNIOEEPROMUNIOReadSerialNumber vector\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_UNIO_EEPROM_ReadMID(int bus_no = 1)
            {
                try
                {
                    string[] pins;

                    if (bus_no == 2)
                        pins = new string[] { UNIOSIO2ChanName.ToUpper(), UNIOVPUP2ChanName.ToUpper() };
                    else
                        pins = new string[] { UNIOSIOChanName.ToUpper(), UNIOVPUPChanName.ToUpper() };

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "source_start(SrcUNIOEEPROMUNIOreadMID" + bus_no.ToString() + ")", "1", "1", "Configure source" });
                    pattern.Add(new string[] { "capture_start(CapUNIOEEPROMUNIOreadMID" + bus_no.ToString() + ")", "1", "1", "Configure capture" });

                    pattern.Add(new string[] { "repeat(300)", "X", "1", "Start Condition" });
                    // cmd
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        pattern.Add(new string[] { "repeat(2)", "0", "1", "Bit " + bit.ToString() });
                        for (int bit_frame = 0; bit_frame < 8; bit_frame++)
                        {
                            pattern.Add(new string[] { "source", "D", "1", "Bit " + bit.ToString() });
                        }
                        pattern.Add(new string[] { "repeat(2)", "1", "1", "Bit " + bit.ToString() });
                    }
                    pattern.Add(new string[] { "", "0", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(2)", "X", "1", "ACK" });
                    pattern.Add(new string[] { "", "L", "1", "ACK" });
                    pattern.Add(new string[] { "repeat(8)", "X", "1", "ACK" });

                    for (int addr = 0; addr < 2; addr++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                            pattern.Add(new string[] { "", "X", "1", "tPUP - 1us" });
                            pattern.Add(new string[] { "capture", "V", "1", "Read - Address " + (addr + 1).ToString() + " - Bit " + (7 - i).ToString() });
                            pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                        }
                        pattern.Add(new string[] { "repeat(9)", "0", "1", "ACK - MID Byte" + (addr + 1).ToString() });
                        pattern.Add(new string[] { "repeat(3)", "1", "1", "ACK - MID Byte" + (addr + 1).ToString() });
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "", "0", "1", "tRD - 1us" });
                        pattern.Add(new string[] { "", "X", "1", "tPUP - 1us" });
                        pattern.Add(new string[] { "capture", "V", "1", "Read - Address 127 - Bit " + (7 - i).ToString() });
                        pattern.Add(new string[] { "repeat(9)", "X", "1", "tBIT" });
                    }
                    pattern.Add(new string[] { "repeat(2)", "0", "1", "NACK - MID Byte3" });
                    pattern.Add(new string[] { "repeat(10)", "1", "1", "NACK - MID Byte3" });

                    pattern.Add(new string[] { "repeat(200)", "X", "1", "Stop Condition" });

                    pattern.Add(new string[] { "capture_stop", "X", "1", "" });
                    pattern.Add(new string[] { "halt", "X", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Register read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("UNIOEEPROMUNIOReadMID" + bus_no.ToString(), pins, pattern, true, Timeset.UNIO_EEPROM))
                    {
                        throw new Exception("UNIOEEPROMUNIOReadMID Compilation Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load UNIOEEPROMUNIOReadMID vector\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool UNIO_EEPROMDiscovery(int bus_no = 1)
            {
                bool bEEPROMfound = false;

                if (bus_no > 2 || bus_no < 1)
                    return bEEPROMfound;

                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allEEPROM_UNIOPins.SelectedFunction = SelectedFunction.Digital;
                    allEEPROM_UNIOPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };

                    // Set EEPROM register read address
                    uint[] dataArray = new uint[1];

                    // Configure to source data, register address is up to 8 bits
                    if (bus_no == 2)
                        DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcEEPROMUNIODiscovery" + bus_no.ToString(), SourceDataMapping.Broadcast, 6, BitOrder.MostSignificantBitFirst);
                    else
                        DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcEEPROMUNIODiscovery" + bus_no.ToString(), SourceDataMapping.Broadcast, 6, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("SrcEEPROMUNIODiscovery" + bus_no.ToString(), dataArray);

                    // Burst Pattern
                    passFail = DIGI.PatternControl.BurstPattern("", "EEPROMUNIODiscovery" + bus_no.ToString(), false, new TimeSpan(0, 0, 0, 10));

                    if (passFail.Length > 0)
                        bEEPROMfound = passFail[0];
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    //MessageBox.Show("Failed to discover UNI/O EEPROM." + Environment.NewLine + Environment.NewLine + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return bEEPROMfound;
            }

            public override bool UNIO_EEPROMWriteID(UNIO_EEPROMType device, string dataWrite, int bus_no = 1)
            {
                bool bStatus = false;
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(dataWrite);

                int cmd = ((int)(UNIO_EEPROMOpCode.Access) << 4) | ((int)device << 1);

                if (byteArray.Length > 20)
                {
                    MessageBox.Show("Exceededed maximum data length of 20 characters,\nEEPROM will not be written.", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return bStatus;
                }
                if (bus_no > 2 || bus_no < 1)
                    return bStatus;

                if (UNIO_EEPROMDiscovery(bus_no))
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                    allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    int failCtr = 0;
                    // Set EEPROM data
                    uint[] dataArray = new uint[24];  // 3 bytes x 8 bits
                    uint bitCompare = 0;

                    for (int reg = 0; reg < 20; reg++)
                    {
                        // command
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmd >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[bit] = 255;    // bit 1
                            else
                                dataArray[bit] = 1;       // bit 0
                        }

                        // register address
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)reg >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[8 + bit] = 255;    // bit 1
                            else
                                dataArray[8 + bit] = 1;      // bit 0
                        }

                        // data
                        if (reg < byteArray.Length)
                        {
                            for (int bit = 0; bit < 8; bit++)
                            {
                                bitCompare = ((uint)byteArray[reg] >> (7 - bit)) & 0x01;

                                if (bitCompare == 1)
                                    dataArray[16 + bit] = 255;    // bit 1
                                else
                                    dataArray[16 + bit] = 1;      // bit 0
                            }
                        }
                        else
                        {
                            for (int bit = 0; bit < 8; bit++)
                            {
                                dataArray[16 + bit] = 1;        // bit 0
                            }
                        }

                        // Configure to source data, register address is up to 8 bits
                        if (bus_no == 2)
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), dataArray);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOWrite" + bus_no.ToString(), false, new TimeSpan(0, 0, 20));

                        if (passFail[0] == false)
                            failCtr++;
                    }

                    string dataRead = UNIO_EEPROMReadID(device, bus_no);

                    if (failCtr == 0 && dataRead == dataWrite)
                    {
                        //MessageBox.Show("Writing & readback successful!", "UNI/O EEPROM");
                        bStatus = true;
                    }
                }
                else
                {
                    //MessageBox.Show("UNI/O EEPROM Discovery Failure!", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return bStatus;
            }

            public override bool UNIO_EEPROMWriteCounter(UNIO_EEPROMType device, uint count, int bus_no = 1)
            {
                bool bStatus = false;

                if (bus_no > 2 || bus_no < 1)
                    return bStatus;

                int cmd = ((int)(UNIO_EEPROMOpCode.Access) << 4) | ((int)device << 1);

                byte[] byteArray = new byte[3];

                for (int b = 0; b < 3; b++)
                {
                    byteArray[b] = (byte)(((int)count >> ((2 - b) * 8)) & 0xFF);
                }

                if (UNIO_EEPROMDiscovery())
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                    allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    int failCtr = 0;
                    // Set EEPROM data
                    uint[] dataArray = new uint[24];  // 3 bytes x 8 bits
                    uint bitCompare = 0;
                    int dataCount = 0;

                    for (int reg = 125; reg < 128; reg++)
                    {
                        // command
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmd >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[bit] = 255;    // bit 1
                            else
                                dataArray[bit] = 1;       // bit 0
                        }

                        // register address
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)reg >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[8 + bit] = 255;    // bit 1
                            else
                                dataArray[8 + bit] = 1;      // bit 0
                        }

                        // data
                        if (dataCount < byteArray.Length)
                        {
                            for (int bit = 0; bit < 8; bit++)
                            {
                                bitCompare = ((uint)byteArray[dataCount] >> (7 - bit)) & 0x01;

                                if (bitCompare == 1)
                                    dataArray[16 + bit] = 255;    // bit 1
                                else
                                    dataArray[16 + bit] = 1;      // bit 0
                            }
                            dataCount++;
                        }

                        // Configure to source data, register address is up to 8 bits
                        if (bus_no == 2)
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), dataArray);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOWrite" + bus_no.ToString(), false, new TimeSpan(0, 0, 20));

                        if (passFail[0] == false)
                            failCtr++;
                    }

                    uint ctr = UNIO_EEPROMReadCounter(device, bus_no);

                    if (failCtr == 0 && ctr == count)
                    {
                        bStatus = true;
                    }
                }
                else
                {
                    //MessageBox.Show("UNI/O EEPROM Discovery Failure!", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return bStatus;
            }

            public override bool UNIO_EEPROMFreeze(UNIO_EEPROMType device, int bus_no = 1)
            {
                bool bStatus = false;
                int cmd = ((int)(UNIO_EEPROMOpCode.FreezeROMZoneState) << 4) | ((int)device << 1);
                int addr = 0x55;
                int data = 0xAA;

                if (bus_no > 2 || bus_no < 1)
                    return bStatus;

                if (UNIO_EEPROMDiscovery(bus_no))
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                    allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    // Set EEPROM data
                    uint[] dataArray = new uint[24];  // 3 bytes x 8 bits
                    uint bitCompare = 0;

                    // command
                    for (int bit = 0; bit < 8; bit++)
                    {
                        bitCompare = ((uint)cmd >> (7 - bit)) & 0x01;

                        if (bitCompare == 1)
                            dataArray[bit] = 255;    // bit 1
                        else
                            dataArray[bit] = 1;       // bit 0
                    }

                    // register address
                    for (int bit = 0; bit < 8; bit++)
                    {
                        bitCompare = (uint)(addr >> (7 - bit)) & 0x01;

                        if (bitCompare == 1)
                            dataArray[8 + bit] = 255;    // bit 1
                        else
                            dataArray[8 + bit] = 1;      // bit 0
                    }

                    // data
                    for (int bit = 0; bit < 8; bit++)
                    {
                        bitCompare = (uint)(data >> (7 - bit)) & 0x01;

                        if (bitCompare == 1)
                            dataArray[16 + bit] = 255;    // bit 1
                        else
                            dataArray[16 + bit] = 1;      // bit 0
                    }


                    // Configure to source data, register address is up to 8 bits
                    if (bus_no == 2)
                        DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                    else
                        DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOWrite" + bus_no.ToString(), dataArray);

                    // Burst Pattern
                    passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOWrite" + bus_no.ToString(), false, new TimeSpan(0, 0, 5));

                    if (passFail[0] == true)
                    {
                        //MessageBox.Show("EEPROM state is frozen!", "UNI/O EEPROM");
                        bStatus = true;
                    }
                }
                else
                {
                    //MessageBox.Show("UNI/O EEPROM Discovery Failure!", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return bStatus;
            }

            public override string UNIO_EEPROMReadID(UNIO_EEPROMType device, int bus_no = 1)
            {
                try
                {
                    int cmdWrite = ((int)(UNIO_EEPROMOpCode.Access) << 4) | ((int)device << 1);
                    int cmdRead = ((int)(UNIO_EEPROMOpCode.Access) << 4) | ((int)device << 1) | 1;
                    uint bitCompare = 0;
                    string returnval = "";

                    if (bus_no > 2 || bus_no < 1)
                        return returnval;

                    if (UNIO_EEPROMDiscovery(bus_no))
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                        allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        // Burst pattern & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        // Set EEPROM register read address
                        uint[] dataArray = new uint[16];

                        // command to dummy write
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmdWrite >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[bit] = 255;    // bit 1
                            else
                                dataArray[bit] = 1;      // bit 0
                        }

                        // command to read
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmdRead >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[8 + bit] = 255;    // bit 1
                            else
                                dataArray[8 + bit] = 1;      // bit 0
                        }

                        // Configure to source data, register address is up to 8 bits
                        if (bus_no == 2)
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOread" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOread" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOread" + bus_no.ToString(), dataArray);

                        // Configure to capture 8 bits x 128 addresses
                        if (bus_no == 2)
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "CapUNIOEEPROMUNIOread" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "CapUNIOEEPROMUNIOread" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOReadID" + bus_no.ToString(), false, new TimeSpan(0, 0, 5));

                        // Retrieve captured waveform
                        uint[][] capData = new uint[][] { };
                        DIGI.CaptureWaveforms.Fetch("", "CapUNIOEEPROMUNIOread" + bus_no.ToString(), 20, new TimeSpan(0, 0, 0, 0, 100), ref capData);

                        // Convert captured data to hex string and return
                        for (int b = 0; b < capData[0].Length; b++)
                        {
                            returnval += (char)capData[0][b];
                        }

                        //if (passFail[0] == false)
                        //    MessageBox.Show("UNI/O EEPROM Read Unsuccessful!", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //else
                    //{
                    //    MessageBox.Show("UNI/O EEPROM Discovery Failure!", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //}

                    return returnval.Replace("\0", string.Empty).Trim();
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    //MessageBox.Show("Failed to Read EEPROM.\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }
            }

            public override uint UNIO_EEPROMReadCounter(UNIO_EEPROMType device, int bus_no = 1)
            {
                try
                {
                    int cmdWrite = ((int)(UNIO_EEPROMOpCode.Access) << 4) | ((int)device << 1);
                    int cmdRead = ((int)(UNIO_EEPROMOpCode.Access) << 4) | ((int)device << 1) | 1;
                    uint bitCompare = 0;
                    uint returnval = 0;

                    if (bus_no > 2 || bus_no < 1)
                        return returnval;

                    if (UNIO_EEPROMDiscovery(bus_no))
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                        allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        // Burst pattern & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        // Set EEPROM register read address
                        uint[] dataArray = new uint[16];

                        // command to dummy write
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmdWrite >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[bit] = 255;    // bit 1
                            else
                                dataArray[bit] = 1;      // bit 0
                        }

                        // command to read
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmdRead >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[8 + bit] = 255;    // bit 1
                            else
                                dataArray[8 + bit] = 1;      // bit 0
                        }

                        // Configure to source data, register address is up to 8 bits
                        if (bus_no == 2)
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOread" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOread" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOread" + bus_no.ToString(), dataArray);

                        // Configure to capture 8 bits x 128 addresses
                        if (bus_no == 2)
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "CapUNIOEEPROMUNIOread" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "CapUNIOEEPROMUNIOread" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOReadCounter" + bus_no.ToString(), false, new TimeSpan(0, 0, 5));

                        // Retrieve captured waveform
                        uint[][] capData = new uint[][] { };
                        DIGI.CaptureWaveforms.Fetch("", "CapUNIOEEPROMUNIOread" + bus_no.ToString(), 3, new TimeSpan(0, 0, 0, 0, 100), ref capData);

                        // Convert captured data to hex string and return
                        for (int b = 0; b < capData[0].Length; b++)
                        {
                            returnval += (uint)((int)capData[0][b] << (int)((capData[0].Length - b - 1) * 8));
                        }
                    }

                    return returnval;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    //MessageBox.Show("Failed to Read EEPROM.\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }
            }

            public override string UNIO_EEPROMReadSerialNumber(UNIO_EEPROMType device, int bus_no = 1)
            {
                double serial_number = 0;
                try
                {
                    int cmdWrite = ((int)(UNIO_EEPROMOpCode.SecurityRegisterAccess) << 4) | ((int)device << 1);
                    int cmdRead = ((int)(UNIO_EEPROMOpCode.SecurityRegisterAccess) << 4) | ((int)device << 1) | 1;
                    uint bitCompare = 0;

                    if (bus_no > 2 || bus_no < 1)
                        return "";

                    if (UNIO_EEPROMDiscovery(bus_no))
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                        allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        // Burst pattern & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        // Set EEPROM register read address
                        uint[] dataArray = new uint[16];

                        // command to dummy write
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmdWrite >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[bit] = 255;    // bit 1
                            else
                                dataArray[bit] = 1;      // bit 0
                        }

                        // command to read
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmdRead >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[8 + bit] = 255;    // bit 1
                            else
                                dataArray[8 + bit] = 1;      // bit 0
                        }

                        // Configure to source data, register address is up to 8 bits
                        if (bus_no == 2)
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString(), dataArray);

                        // Configure to capture 8 bits x 128 addresses
                        if (bus_no == 2)
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "CapUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "CapUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOReadSerialNumber" + bus_no.ToString(), false, new TimeSpan(0, 0, 5));

                        // Retrieve captured waveform
                        uint[][] capData = new uint[][] { };
                        DIGI.CaptureWaveforms.Fetch("", "CapUNIOEEPROMUNIOreadSerialNumber" + bus_no.ToString(), 8, new TimeSpan(0, 0, 0, 0, 10), ref capData);

                        //if (passFail[0] == false)
                        //    MessageBox.Show("UNI/O EEPROM Read Serial Number Unsuccessful!", "UNI/O EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Convert captured data to hex string and return
                        if (capData[0].Length > 7)
                        {
                            for (int b = capData[0].Length - 2; b > 0; b--)
                            {
                                serial_number += (double)capData[0][capData[0].Length - b - 1] * Math.Pow(256, (double)b - 1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    //MessageBox.Show("Failed to Read EEPROM Serial Number.\n\n" + e.ToString(), "UNI/O", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return (Convert.ToUInt64(serial_number)).ToString();
            }

            public override string UNIO_EEPROMReadMID(UNIO_EEPROMType device, int bus_no = 1)
            {
                double mfg_id = 0;

                try
                {
                    uint cmd = ((uint)UNIO_EEPROMOpCode.ManufacturerIDRead << 4) | ((uint)device << 1) | 1;
                    uint bitCompare = 0;

                    if (bus_no > 2 || bus_no < 1)
                        return "";

                    if (UNIO_EEPROMDiscovery(bus_no))
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                        allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        // Burst pattern & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        // Set EEPROM register read address
                        uint[] dataArray = new uint[8];

                        // command
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bitCompare = ((uint)cmd >> (7 - bit)) & 0x01;

                            if (bitCompare == 1)
                                dataArray[bit] = 255;    // bit 1
                            else
                                dataArray[bit] = 1;      // bit 0
                        }

                        // Configure to source data, register address is up to 8 bits
                        if (bus_no == 2)
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "SrcUNIOEEPROMUNIOreadMID" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.SourceWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "SrcUNIOEEPROMUNIOreadMID" + bus_no.ToString(), SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcUNIOEEPROMUNIOreadMID" + bus_no.ToString(), dataArray);

                        // Configure to capture 8 bits x 128 addresses
                        if (bus_no == 2)
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIO2ChanName.ToUpper(), "CapUNIOEEPROMUNIOreadMID" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);
                        else
                            DIGI.CaptureWaveforms.CreateSerial(UNIOSIOChanName.ToUpper(), "CapUNIOEEPROMUNIOreadMID" + bus_no.ToString(), 8, BitOrder.MostSignificantBitFirst);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "UNIOEEPROMUNIOReadMID" + bus_no.ToString(), false, new TimeSpan(0, 0, 2));

                        // Retrieve captured waveform
                        uint[][] capData = new uint[][] { };
                        DIGI.CaptureWaveforms.Fetch("", "CapUNIOEEPROMUNIOreadMID" + bus_no.ToString(), 3, new TimeSpan(0, 0, 0, 0, 50), ref capData);

                        // Convert captured data to hex string and return
                        if (capData[0].Length > 2)
                        {
                            for (int b = capData[0].Length - 1; b >= 0; b--)
                            {
                                mfg_id += (double)capData[0][capData[0].Length - b - 1] * Math.Pow(256, (double)b);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                }
                return (Convert.ToUInt64(mfg_id)).ToString();
            }

            #endregion EEPROM UNI/O

            public override bool LoadVector_TEMPSENSEI2C(decimal TempSensorAddress)
            {
                try
                {
                    if (!(TempSensorAddress >= 0 && TempSensorAddress <= 3)) throw new Exception("Temp Sensor Address out of range");
                    I2CTempSensorDeviceAddress = TempSensorAddress; //this only happens if the temp sensor device address is valid

                    LoadVector_ConfigRegister("0", "0", "0", "0", "0", "0", "0", "0", I2CTempSensorDeviceAddress); //seems a little hidden but it's the right place for now

                    return LoadVector_TEMPSENSEI2CRead(I2CTempSensorDeviceAddress);
                }
                catch (Exception e)
                {

                    return false;
                }
            }

            /// <summary>
            /// I2C TEMPSENSE Read - Called by SPara only.
            /// </summary>
            /// <returns>temperature as a double</returns>
            public override double I2CTEMPSENSERead()
            {
                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allTEMPSENSEPins.SelectedFunction = SelectedFunction.Digital;
                    //allTEMPSENSEPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    //double returnval = double.NaN;
                    // Set TEMPSENSEVccPin to Activeload (Constant supply of vcom) to ensure that the Tempsensor is 
                    // On all the time. Initial temperature reading will take approx 200 ms, if tempsensor is On, subsequent
                    // reading will only takes approx 4ms. - RON
                    TEMPSENSEVccPin.DigitalLevels.TerminationMode = TerminationMode.ActiveLoad;
                    double returnval = -20;
                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };

                    // Configure to capture 16 bits
                    DIGI.CaptureWaveforms.CreateSerial(I2CSDAChanName.ToUpper(), "CapTEMPSENSERead", 16, BitOrder.MostSignificantBitFirst);


                    // Burst Pattern
                    passFail = DIGI.PatternControl.BurstPattern("", "TEMPSENSERead", true, new TimeSpan(0, 0, 10));
                    //Thread.Sleep(300);
                    //passFail = DIGI.PatternControl.BurstPattern("", "TEMPSENSERead",true, new TimeSpan(0, 0, 10));

                    // Retreive captured waveform
                    uint[][] capData = new uint[][] { };
                    DIGI.CaptureWaveforms.Fetch("", "CapTEMPSENSERead", 1, new TimeSpan(0, 0, 0, 0, 100), ref capData);

                    if (passFail[0])
                    {
                        // Convert captured data
                        Int32 data = (Int32)capData[0][0];

                        if (ConfigRegisterSettings["Resolution"] == "0")
                        {
                            // shift right by 3 to remove flag bits which are the 3 LSBs
                            data = data >> 3;
                            // convert based on sign bit (bit 13 after the bitshift from above)
                            if ((data & (1 << 13 - 1)) != 0)
                            {
                                // negative conversion
                                returnval = ((double)data - 8192.0) / 16.0;
                            }
                            else
                            {
                                // positive conversion
                                returnval = (double)data / 16.0;
                            }

                        }

                        //// shift right by 3 to remove flag bits which are the 3 LSBs
                        //data = data >> 3;
                        //// convert based on sign bit (bit 13 after the bitshift from above)
                        //if ((data & (1 << 13 - 1)) != 0)
                        //{
                        //    // negative conversion
                        //    returnval = ((double)data - 8192.0) / 16.0;
                        //}
                        //else
                        //{
                        //    // positive conversion
                        //    returnval = (double)data / 16.0;
                        //}
                        else
                        {
                            if ((data & (1 << 16 - 1)) != 0)
                            {
                                // negative conversion
                                returnval = ((double)data - 65536.0) / 128.0;
                            }
                            else
                            {
                                // positive conversion
                                returnval = (double)data / 128.0;
                            }
                        }


                        if (debug) Console.WriteLine("I2CTEMPSENSERead: " + returnval);
                        return returnval;
                    }
                    else
                    {
                        //return double.NaN;
                        return -20;
                    }
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Read I2C TEMPSENSE Register.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return double.NaN;
                }
            }

            /// <summary>
            /// Internal Function: Used to generate and load the TEMPSENSE read pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_TEMPSENSEI2CRead(decimal TempSensorAddress)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    #region Set Device Address
                    //LoadVector_TEMPSENSE12C() filters out the invalid temp sensor addresses before here
                    string[] zTempSensorValidAddresses = { "00", "01", "10", "11" };
                    string zAddress = zTempSensorValidAddresses[(int)TempSensorAddress];

                    string A0 = "1", A1 = "1";

                    switch (zAddress)
                    {
                        case "00":
                            A0 = "0";
                            A1 = "0";
                            break;

                        case "01":
                            A0 = "0";
                            A1 = "1";
                            break;

                        case "10":
                            A0 = "1";
                            A1 = "0";
                            break;

                        case "11":
                            A0 = "1";
                            A1 = "1";
                            break;
                    }
                    #endregion Set Device Address

                    string[] pins = new string[] { I2CSCKChanName.ToUpper(), I2CSDAChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                    List<string[]> patternInit = new List<string[]> { };
                    patternInit.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle with VDD high to turn on Temp Sensor" });
                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("TEMPSENSEOn", pins, patternInit, true, Timeset.EEPROM, Timeset.EEPROM))
                    {
                        throw new Exception("TEMPSENSEOn Compile Failed");
                    }

                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "capture_start(CapTEMPSENSERead)", "1", "1", "1", "Configure capture" });
                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Start Condition" });
                    pattern.Add(new string[] { "", "0", "0", "1", "Start Condition" });

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", A1, "1", A1 + " - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", A0, "1", A0 + " - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Bit" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "", "1", "0", "1", "0 - Register Address (0x0 for MSB)" });
                        pattern.Add(new string[] { "", "0", "-", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    pattern.Add(new string[] { "", "1", "1", "1", "Start Condition" });
                    pattern.Add(new string[] { "", "0", "0", "1", "Start Condition" });

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", A1, "1", A1 + " - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", A0, "1", A0 + " - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Read Bit" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "capture", "1", "V", "1", "Read MSB Data" });
                        pattern.Add(new string[] { "", "0", "X", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "0", "1", "ACK - BY MASTER" });
                    pattern.Add(new string[] { "", "0", "0", "-", "" });

                    for (int i = 0; i < 8; i++)
                    {
                        pattern.Add(new string[] { "capture", "1", "V", "1", "Read LSB Data" });
                        pattern.Add(new string[] { "", "0", "X", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "1", "1", "NO ACK - BY MASTER" });
                    pattern.Add(new string[] { "", "0", "1", "-", "" });

                    pattern.Add(new string[] { "", "1", "0", "1", "Stop Condition" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Stop Condition" });

                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });

                    pattern.Add(new string[] { "capture_stop", "1", "1", "1", "" });
                    pattern.Add(new string[] { "halt", "1", "1", "1", "halt" });

                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("TEMPSENSERead", pins, pattern, forceDigiPatRegeneration, Timeset.EEPROM, Timeset.EEPROM))
                    {
                        throw new Exception("TEMPSENSERead Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load TEMPSENSERead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_ConfigRegister(string Res, string OpMode6, string OpMode5, string IntorCT, string INTPinPol, string CTPinPol, string FaultQ1, string FaultQ0, decimal TempSensorAddress)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    if (!(TempSensorAddress >= 0 && TempSensorAddress <= 3)) throw new Exception("Temp Sensor Address out of range"); //This is only redundant if being called by LoadVector_TEMPSENSEI2C
                    I2CTempSensorDeviceAddress = TempSensorAddress; //this only happens if the temp sensor device address is valid

                    #region Set Device Address
                    //LoadVector_TEMPSENSE12C() filters out the invalid temp sensor addresses before here
                    string[] zTempSensorValidAddresses = { "00", "01", "10", "11" };
                    string zAddress = zTempSensorValidAddresses[(int)I2CTempSensorDeviceAddress];

                    string A0 = "1", A1 = "1";

                    switch (zAddress)
                    {
                        case "00":
                            A0 = "0";
                            A1 = "0";
                            break;

                        case "01":
                            A0 = "0";
                            A1 = "1";
                            break;

                        case "10":
                            A0 = "1";
                            A1 = "0";
                            break;

                        case "11":
                            A0 = "1";
                            A1 = "1";
                            break;
                    }
                    #endregion Set Device Address

                    #region Set the Configuration Register Values
                    ConfigRegisterSettings["FaultQueueBit0"] = FaultQ0; // "0"; // 00 = 1 fault, 01 = 2 faults, 10 = 3 faults, 11 = 4 faults (This is bits 1:0)
                    ConfigRegisterSettings["FaultQueueBit1"] = FaultQ1; // "0"; // 00 = 1 fault, 01 = 2 faults, 10 = 3 faults, 11 = 4 faults (This is bits 1:0)
                    ConfigRegisterSettings["CTPinPolarity"] = CTPinPol; // "0";  // 0 = Active Low, 1 = Active High (This is bit 2)
                    ConfigRegisterSettings["INTPinPolarity"] = INTPinPol; // "0"; // 0 = Active Low, 1 = Active High (This is bit 3)
                    ConfigRegisterSettings["INTorCTMode"] = IntorCT; // "0"; // 0 = Interrupt mode, 1 = Comparator mode (This is bit 4)
                    ConfigRegisterSettings["OperationModeBit5"] = OpMode5; // "0"; // 00 = Continuous conversion, 01 = One shot, 10 = 1 SPS mode, 11 = Shutdown (This is bits 6:5)
                    ConfigRegisterSettings["OperationModeBit6"] = OpMode6; // "0"; // 00 = Continuous conversion, 01 = One shot, 10 = 1 SPS mode, 11 = Shutdown (This is bits 6:5)
                    ConfigRegisterSettings["Resolution"] = Res; // "0"; // 0 = 13 bit resolution, 1 = 16 bit resolution (This is bit 7) 
                    #endregion Set the Configuration Register Values

                    string[] pins = new string[] { I2CSCKChanName.ToUpper(), I2CSDAChanName.ToUpper(), TEMPSENSEI2CVCCChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };
                    pattern.Add(new string[] { "capture_start(CapConfRegWrite)", "1", "1", "1", "Configure capture" });
                    pattern.Add(new string[] { "repeat(1220)", "1", "1", "1", "Idle with VDD high for 6.1ms to allow first ADT7420 conversion" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Start Condition" });
                    pattern.Add(new string[] { "", "0", "0", "1", "Start Condition" });

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - ADT7410 Address (10010" + zAddress + ")" });  //Device address
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", A1, "1", A1 + " - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", A0, "1", A0 + " - ADT7410 Address (10010" + zAddress + ")" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", "0", "1", "0 - Write Bit" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    for (int i = 0; i < 6; i++)
                    {
                        pattern.Add(new string[] { "", "1", "0", "1", "0 - Register Address (0x03 for ConfigRegister)" });
                        pattern.Add(new string[] { "", "0", "-", "1", "" });
                    }

                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Register Address (0x03 for ConfigRegister)" });
                    pattern.Add(new string[] { "", "0", "-", "1", "" });
                    pattern.Add(new string[] { "", "1", "1", "1", "1 - Register Address (0x03 for ConfigRegister)" });
                    pattern.Add(new string[] { "", "0", "-", "1", "" });

                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    pattern.Add(new string[] { "", "1", Res, "1", Res + "- Config Register Bit 7 Resolution" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", OpMode6, "1", OpMode6 + "- Config Register Bit 6 OpMode" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", OpMode5, "1", OpMode5 + "- Config Register Bit 5 OpMode" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", IntorCT, "1", IntorCT + "- Config Register Bit 4 IntOrCTMode" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", INTPinPol, "1", INTPinPol + "- Config Register Bit 3 IntPinPolarity" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", CTPinPol, "1", CTPinPol + "- Config Register Bit 2 CTPinPolarity" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", FaultQ1, "1", FaultQ1 + "- Config Register Bit 1 FaultQueue" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });
                    pattern.Add(new string[] { "", "1", FaultQ0, "1", FaultQ0 + "- Config Register Bit 0 FaultQueue" });
                    pattern.Add(new string[] { "", "0", "-", "-", "" });


                    pattern.Add(new string[] { "", "1", "L", "1", "ACK" });
                    pattern.Add(new string[] { "", "0", "X", "-", "" });

                    pattern.Add(new string[] { "", "1", "0", "1", "Stop Condition" });
                    pattern.Add(new string[] { "", "1", "1", "1", "Stop Condition" });

                    pattern.Add(new string[] { "repeat(300)", "1", "1", "1", "Idle" });

                    pattern.Add(new string[] { "capture_stop", "1", "1", "1", "" });
                    pattern.Add(new string[] { "halt", "1", "1", "1", "halt" });


                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("ConfRegWrite", pins, pattern, true, Timeset.EEPROM, Timeset.EEPROM))
                    {
                        throw new Exception("ConfRegWrite Compile Failed");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load TEMPSENSERead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffTest(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    #region Pattern for RFONOFFTest

                    string TrigExt = "0";
                    string[] pins = new string[]
                    {
                        Sclk1ChanName.ToUpper(),
                        Sdata1ChanName.ToUpper(),
                        Vio1ChanName.ToUpper(),
                        TrigChanName.ToUpper(),
                        Vio2ChanName.ToUpper()
                    };
                    List<string[]> pattern = new List<string[]> { };


                    if (isNRZ)
                    {
                        #region NRZ vector

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });


                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "Clear Trigger" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });


                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffTest", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffTest Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Vector

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffloop)", "0", "0", "1", "0", "1", " wait for trigger", "1" });
                        pattern.Add(new string[] { "rfonoffloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });

                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        ///TRIG ON
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });


                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };


                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(50)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });

                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 0)
                            {
                                pattern.Add(new string[] { "clear_signal(event3)", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });


                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffTest", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffTest Compile Failed");
                        }

                        #endregion
                    }
                    #endregion

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffTest vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";


                    string[] pins = new string[]
                    {
                        Sclk1ChanName.ToUpper(),
                        Sdata1ChanName.ToUpper(),
                        Vio1ChanName.ToUpper(),
                        TrigChanName.ToUpper(),
                        Vio2ChanName.ToUpper()
                    };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });


                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON" }); //ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Source Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });


                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTest Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });


                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" }); //ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(30)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTest Compile Failed");
                        }

                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTest"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffTestRx(bool isNRZ = false)
            {


                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    #region Pattern for RFONOFFTest

                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };


                    if (isNRZ)
                    {
                        #region NRZ vector

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffloop)", "0", "0", "1", "0", "1", " wait for trigger", "1" });
                        pattern.Add(new string[] { "rfonoffloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });

                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        ///TRIG ON
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });

                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };

                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });      //repeat(30) Cheeon
                        TrigExt = "0";

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });

                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 0)
                            {
                                pattern.Add(new string[] { "clear_signal(event3)", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                                pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffTestRx", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffTestRx Compile Failed");
                        }

                        //datalogResults["RFOnOffTest"] = false;

                        #endregion
                    }
                    else
                    {
                        #region RZ Vector

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffloop)", "0", "0", "1", "0", "1", " wait for trigger", "1" });
                        pattern.Add(new string[] { "rfonoffloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });

                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        ///TRIG ON
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });


                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };


                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });      //repeat(30) Cheeon
                        TrigExt = "0";

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });

                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 0)
                            {
                                pattern.Add(new string[] { "clear_signal(event3)", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffTestRx", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffTestRx Compile Failed");
                        }

                        //datalogResults["RFOnOffTestRx"] = false;

                        #endregion
                    }
                    #endregion

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffTest vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest_WithPreMipi(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitchPreMipi)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        // Source Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregON" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTestPreMipi", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTestPreMipi Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitchPreMipi)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(30)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTestPreMipi", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTestPreMipi Compile Failed");
                        }

                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTestPreMipi"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTestPreMipi vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest_With3TxPreMipi(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[]
                    {
                        Sclk1ChanName.ToUpper(),
                        Sdata1ChanName.ToUpper(),
                        Vio1ChanName.ToUpper(),
                        TrigChanName.ToUpper(),
                        Vio2ChanName.ToUpper()
                    };
                    List<string[]> pattern = new List<string[]>();

                    if (isNRZ)
                    {
                        // Not support yet
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch)", "0", "0", "1", "0", "Configure source" });

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigOff[i].ToString(), "1", "0", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", TrigOff[i + 1].ToString(), "1", "0", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOff" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "Wait 1040" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            if (i == 34)
                            {
                                pattern.Add(new string[] { "set_signal(event1)", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                                continue;
                            };

                            pattern.Add(new string[] { "", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "clear_signal(event1)", "1", TrigOff[i].ToString(), "1", "0", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", TrigOff[i + 1].ToString(), "1", "0", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOff" });

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGMaskOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOn[i].ToString(), "1", "0", "TRIGMaskOn" });
                            pattern.Add(new string[] { "", "0", TrigMaskOn[i + 1].ToString(), "1", "0", "TRIGMaskOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGMaskOn" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "SWregOFF" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregOFF" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "Wait 1040" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregON" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            if (i == 34)
                            {
                                pattern.Add(new string[] { "source,set_signal(event1)", "1", "D", "1", "0", "SWregON" });
                                pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregON" });
                                continue;
                            };

                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "SWregON" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregON" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "source,clear_signal(event1)", "1", "D", "1", "0", "SWregOFF" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregOFF" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "halt" });

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch3TxPreMipi)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF2" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF3" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF3" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF3" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF3" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON2" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON2" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON3" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON3" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON3" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON3" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON3" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(30)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        #endregion
                    }

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern("", "RFOnOffSwitchTest3TxPreMipi", pins, pattern, true, Timeset.MIPI_RFONOFF, Timeset.MIPI_HALF_RFONOFF))
                    {
                        throw new Exception("RFOnOffSwitchTest3TxPreMipi Compile Failed");
                    }

                    //datalogResults["RFOnOffSwitchTest3TxPreMipi"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest3TxPreMipi vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        // Not support yet
                        #region NRZ Pattern for RFONOFFSwitchTest


                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitchRx)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });//ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });


                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON" }); //ori 130
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });

                        // Source Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTestRx", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTestRx Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitchRx)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });


                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTestRx", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTestRx Compile Failed");
                        }
                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTestRx"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx_WithPreMipi(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitchPreMipiRx)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });  //Modified to repeat(50) Ori repeat(30) cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        // Source Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregON" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });
                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTestPreMipiRx", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTestPreMipiRx Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitchPreMipiRx)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        // Source Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON" });

                        // Source Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTestPreMipiRx", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTestPreMipiRx Compile Failed");
                        }

                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTestPreMipiRx"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTestPreMipiRx vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTestRx_With1Tx2RxPreMipi(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        // Not support yet
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch)", "0", "0", "1", "0", "Configure source" });

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigOff[i].ToString(), "1", "0", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", TrigOff[i + 1].ToString(), "1", "0", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOff" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "Wait 1040" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            if (i == 34)
                            {
                                pattern.Add(new string[] { "set_signal(event1)", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                                continue;
                            };

                            pattern.Add(new string[] { "", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "clear_signal(event1)", "1", TrigOff[i].ToString(), "1", "0", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", TrigOff[i + 1].ToString(), "1", "0", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOff" });

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGMaskOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOn[i].ToString(), "1", "0", "TRIGMaskOn" });
                            pattern.Add(new string[] { "", "0", TrigMaskOn[i + 1].ToString(), "1", "0", "TRIGMaskOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGMaskOn" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "SWregOFF" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregOFF" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "Wait 1040" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregON" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            if (i == 34)
                            {
                                pattern.Add(new string[] { "source,set_signal(event1)", "1", "D", "1", "0", "SWregON" });
                                pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregON" });
                                continue;
                            };

                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "SWregON" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregON" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "source,clear_signal(event1)", "1", "D", "1", "0", "SWregOFF" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregOFF" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "halt" });

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch1Tx2RxPreMipiRx)", "0", "0", "1", "0", "1", "X", "X", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", "X", "X", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", "X", "X", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "X", "X", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "X", "X", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "X", "X", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "X", "X", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "X", "X", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", "X", "X", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", "X", "X", " Reset seqflag3" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "X", "X", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TRIGMASKON" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "TxPreSWregOFF" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "TxPreSWregOFF" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "TxPreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "TxPreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "TxPreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "PreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "PreSWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "PreSWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "PreSWregOFF2" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "X", "X", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "TxPreSWregON" });
                        pattern.Add(new string[] { "", "X", "X", "0", "1", "1", "0", "1", "TxPreSWregON" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "TxPreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "TxPreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "TxPreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "PreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "PreSWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "PreSWregON2" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "PreSWregON2" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "X", "X", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "X", "X", "SWregON" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "X", "X", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "X", "X", "halt" });

                        #endregion
                    }

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern("", "RFOnOffSwitchTest1Tx2RxPreMipiRx", pins, pattern, true, Timeset.MIPI_RFONOFF, Timeset.MIPI_HALF_RFONOFF))
                    {
                        throw new Exception("RFOnOffSwitchTest1Tx2RxPreMipiRx Compile Failed");
                    }

                    //datalogResults["RFOnOffSwitchTest1Tx2RxPreMipiRx"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest1Tx2RxPreMipiRx vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest2(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch2)", "0", "0", "1", "0", "1", "Configure source" });   //CM Wong: Added 2 for second switch test

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait2080" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF2" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });

                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "SWregON2" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "SWregON2" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest2", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTest2 Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch2)", "0", "0", "1", "0", "1", "Configure source" });   //CM Wong: Added 2 for second switch test

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });   // Ori repeat(30) Cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });



                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(50)", "0", "0", "1", TrigExt, "1", "TRIGOn" });      //Ori repeat(30)  Cheeon
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });



                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(30)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest2", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTest2 Compile Failed");
                        }

                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTest2"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest2_WithPreMipi(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch2PreMipi)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregON" });

                        // Source Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });      //Ori repeat(30)  Cheeon
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF2" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregOFF2" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF2" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON2" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "PreSWregON2" });
                        }

                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "PreSWregON2" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "SWregON2" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "SWregON2" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest2PreMipi", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTest2PreMipi Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch2PreMipi)", "0", "0", "1", "0", "1", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });   // Ori repeat(30) Cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(50)", "0", "0", "1", TrigExt, "1", "TRIGOn" });      //Ori repeat(30)  Cheeon
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });



                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregOFF2" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "PreSWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "PreSWregON2" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "PreSWregON2" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(30)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest2PreMipi", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTest2PreMipi Compile Failed");
                        }

                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTest2PreMipi"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest2PreMipi vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest2_With1Tx2RxPreMipi(bool isNRZ = false)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        // Not support yet
                        #region NRZ Pattern for RFONOFFSwitchTest

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch)", "0", "0", "1", "0", "Configure source" });

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigOff[i].ToString(), "1", "0", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", TrigOff[i + 1].ToString(), "1", "0", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOff" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "Wait 1040" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            if (i == 34)
                            {
                                pattern.Add(new string[] { "set_signal(event1)", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                                continue;
                            };

                            pattern.Add(new string[] { "", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOff" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "clear_signal(event1)", "1", TrigOff[i].ToString(), "1", "0", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", TrigOff[i + 1].ToString(), "1", "0", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOff" });

                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", " Reset seqflag3" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigOn[i].ToString(), "1", "0", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", TrigOn[i + 1].ToString(), "1", "0", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGOn" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGMaskOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "TRIGMaskOn" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOn[i].ToString(), "1", "0", "TRIGMaskOn" });
                            pattern.Add(new string[] { "", "0", TrigMaskOn[i + 1].ToString(), "1", "0", "TRIGMaskOn" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "TRIGMaskOn" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "SWregOFF" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregOFF" });

                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "Wait 1040" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregON" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            if (i == 34)
                            {
                                pattern.Add(new string[] { "source,set_signal(event1)", "1", "D", "1", "0", "SWregON" });
                                pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregON" });
                                continue;
                            };

                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "SWregON" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregON" });

                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "Wait 34788" });

                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "SWregOFF" });
                        for (int i = 0; i < 46; i = i + 2)
                        {
                            pattern.Add(new string[] { "source,clear_signal(event1)", "1", "D", "1", "0", "SWregOFF" });
                            pattern.Add(new string[] { "source", "0", "D", "1", "0", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "SWregOFF" });

                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "halt" });

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch21Tx2RxPreMipi)", "0", "0", "1", "0", "1", "X", "X", "Configure source" });

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", "X", "X", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", "X", "X", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRz[i].ToString(), "1", "0", "1", "X", "X", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "X", "X", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRz[i].ToString(), "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "X", "X", "TRIGOn" });   // Ori repeat(30) Cheeon

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "X", "X", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "X", "X", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRz[i].ToString(), "1", "0", "1", "X", "X", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", "X", "X", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", "X", "X", " Reset seqflag3" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF2" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "X", "X", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "0", "1", "1", "X", "X", "TxPreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "TxPreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregON2" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON2" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "1", "X", "X", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(50)", "0", "0", "1", TrigExt, "1", "X", "X", "TRIGOn" });      //Ori repeat(30)  Cheeon
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "X", "X", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "X", "X", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", "X", "X", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", "X", "X", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", "X", "X", " Reset seqflag3" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "TxPreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregOFF" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF" });

                        //Pre Off operation:
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregOFF2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregOFF2" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "X", "X", "Wait 1040" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregON" });
                        pattern.Add(new string[] { "", "0", "1", "0", "1", "1", "X", "X", "TxPreSWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "X", "X", "TxPreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "X", "X", "TxPreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregON" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregON" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON" });

                        //Pre ON operation:                        
                        pattern.Add(new string[] { "repeat(130)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "1", "PreSWregON2" });
                        pattern.Add(new string[] { "", "X", "X", "1", "0", "1", "0", "0", "PreSWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "X", "X", "1", "0", "1", "1", "D", "PreSWregON2" });
                        }

                        pattern.Add(new string[] { "repeat(104)", "X", "X", "1", "0", "1", "0", "0", "PreSWregON2" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "X", "X", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "X", "X", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "X", "X", "SWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "1", "X", "X", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "1", "X", "X", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(30)", "0", "0", "1", TrigExt, "1", "X", "X", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "X", "X", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "X", "X", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "X", "X", "halt" });

                        #endregion
                    }

                    // Generate and load Pattern from the formatted array.
                    if (!this.GenerateAndLoadPattern("", "RFOnOffSwitchTest21Tx2RxPreMipi", pins, pattern, true, Timeset.MIPI_RFONOFF, Timeset.MIPI_HALF_RFONOFF))
                    {
                        throw new Exception("RFOnOffSwitchTest21Tx2RxPreMipi Compile Failed");
                    }

                    //datalogResults["RFOnOffSwitchTest21Tx2RxPreMipi"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest21Tx2RxPreMipi vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            public override bool LoadVector_RFOnOffSwitchTest2Rx(bool isNRZ = false)
            {


                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string TrigExt = "0";
                    string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper(), Vio2ChanName.ToUpper() };
                    List<string[]> pattern = new List<string[]> { };

                    if (isNRZ)
                    {
                        #region NRZ Pattern for RFONOFFSwitchTest                      

                        //Replicating the following script in pattern Mode:
                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGOFF";
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate TRIGON marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate TRIGOFF";

                        //scriptFull += "\nwait until scripttrigger0\nclear scripttrigger0";
                        //scriptFull += "\ngenerate TRIGON";
                        //scriptFull += "\ngenerate TRIGMASKON";
                        //scriptFull += "\ngenerate " + SWregOFF;
                        //scriptFull += "\nwait 1040";
                        //scriptFull += "\ngenerate " + SWregON + " marker0(169) marker1(169)";
                        //scriptFull += "\nwait 34788";
                        //scriptFull += "\ngenerate " + SWregOFF;


                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch2Rx)", "0", "0", "1", "0", "1", "Configure source" });   //CM Wong: Added 2 for second switch test

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 2080" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(200)", "0", "0", "1", TrigExt, "1", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait2080" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "TRIGOn" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF2" });
                            pattern.Add(new string[] { "", "0", "-", "1", "0", "1", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(208)", "0", "0", "1", "0", "1", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(2080)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(260)", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });

                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "SWregON2" });
                                pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "SWregON2" });
                            pattern.Add(new string[] { "", "0", "-", "1", TrigExt, "1", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(60)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(10)", "0", "0", "1", "0", "1", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest2Rx", pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("RFOnOffSwitchTest2Rx Compile Failed");
                        }

                        #endregion
                    }
                    else
                    {
                        #region RZ Pattern for RFONOFFSwitchTest

                        // Configure source Arry
                        pattern.Add(new string[] { "source_start(SrcRFONOFFSwitch2Rx)", "0", "0", "1", "0", "1", "Configure source" });  //CM WONG: Added 2 for Switch Test 2

                        // waiting seqflag 
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloopend)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloopend:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Trig OFF
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOff" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOff" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigOffRzRx[i].ToString(), "1", "0", "1", "TRIGOff" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOff" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Trig On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGOn" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGOn" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "set_signal(event3)", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "", "1", TrigOnRzRx[i].ToString(), "1", TrigExt, "1", "1", "TRIGOn" });
                        }

                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });

                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGOn" });

                        // wait
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        // clear Event Trig
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        // Trig Mask On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "TRIGMASKON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "TRIGMASKON" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "", "1", TrigMaskOnRzRx[i].ToString(), "1", "0", "1", "TRIGMASKON" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "TRIGMASKON" });

                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop1end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop1)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop1end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });



                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "1", "TRIGOn" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "1", "TRIGOn" });
                        }
                        pattern.Add(new string[] { "repeat(100)", "0", "0", "1", TrigExt, "1", "TRIGOn" });
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregON" });

                        // Wait 
                        pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });



                        // waiting seqflag
                        pattern.Add(new string[] { "set_loop(65535)", "0", "0", "1", "0", "1", " Wait for Trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2:\nrepeat(3200)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "exit_loop_if(seqflag3, rfonoffswloop2end)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "end_loop(rfonoffswloop2)", "0", "0", "1", "0", "1", " wait for trigger" });
                        pattern.Add(new string[] { "rfonoffswloop2end:\nclear_seqflag(seqflag3)", "0", "0", "1", "0", "1", " Reset seqflag3" });

                        // Souce Setting for Off
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregOFF2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregOFF2" });
                        for (int i = 0; i < 23; i++)
                        {
                            pattern.Add(new string[] { "source", "1", "D", "1", "0", "1", "SWregOFF2" });
                        }
                        pattern.Add(new string[] { "repeat(104)", "0", "0", "1", "0", "1", "SWregOFF2" });

                        // Wait
                        pattern.Add(new string[] { "repeat(1040)", "0", "0", "1", "0", "1", "Wait 1040" });

                        // Souce Setting for On
                        pattern.Add(new string[] { "repeat(130)", "0", "0", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "1", "1", "0", "1", "SWregON2" });
                        pattern.Add(new string[] { "", "0", "0", "1", "0", "1", "SWregON2" });
                        for (int i = 0; i < 23; i++)
                        {
                            if (i == 17)
                            {
                                TrigExt = "0";  //Used for external triggering via trigPort on testboard
                                pattern.Add(new string[] { "source,set_signal(event3)", "1", "D", "1", TrigExt, "1", "1", "SWregON2" });
                                continue;
                            };
                            pattern.Add(new string[] { "source", "1", "D", "1", TrigExt, "1", "1", "SWregON2" });
                        }
                        pattern.Add(new string[] { "repeat(50)", "0", "0", "1", TrigExt, "1", "TRIGOn" });      //Ori repeat(50) Cheeon
                        TrigExt = "0";
                        pattern.Add(new string[] { "repeat(5)", "0", "0", "1", "0", "1", "SWregON2" });

                        // Wait 
                        //pattern.Add(new string[] { "repeat(34788)", "0", "0", "1", "0", "1", "Wait 34788" });
                        pattern.Add(new string[] { "clear_signal(event3)", "0", "0", "1", "0", "1", "Clear Trigger" });

                        //End
                        pattern.Add(new string[] { "halt", "0", "0", "1", "0", "1", "halt" });

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern("RFOnOffSwitchTest2Rx", pins, pattern, true, Timeset.MIPI_RFONOFF))
                        {
                            throw new Exception("RFOnOffSwitchTest2Rx Compile Failed");
                        }

                        #endregion
                    }

                    //datalogResults["RFOnOffSwitchTest2Rx"] = false;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load RFOnOffSwitchTest vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }


            /// <summary>
            /// Internal Function: Used to generate and load the non-extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_TimingTest(Timeset WriteTimeSet)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    int MaxPairIndex = (isShareBus ? 3 : 2);

                    for (int mipiPairs = 1; mipiPairs < MaxPairIndex; mipiPairs++)
                    {
                        for (int i = 1; i <= 1; i++)
                        {
                            int p = mipiPairs; // index for MIPI pair
                            string PairString = "";
                            string PatternName = "TimingExtendedRegisterWriteReg";

                            string[] pins;
                            // Generate MIPI / RFFE Extended Register Write Patterns
                            if (!isShareBus)
                                pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                            else
                            {
                                pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                                PairString += "Pair" + p;
                            }

                            string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                            List<string[]> patternStart = new List<string[]>
                            {
                            #region RegisterWrite Pattern
                                new string[] { "source_start(SrcTimingExtendedRegisterWriteReg" + i + PairString + ")", "0", "0", "1", "0", "Configure source"},
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"}
                               
                                #endregion
                            };

                            List<string[]> patternBeforeTrig = new List<string[]>
                            {
                            #region RegisterWrite Pattern                                
                                new string[] {"set_loop(reg0)","0", "0", "1", "0",""},
                                new string[] {"cmdstart:\nsource", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"end_loop(cmdstart)", "0", "-", "1", "-", ""},
                                 new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };

                            List<string[]> patternAfterTrig = new List<string[]>
                            {
                            #region RegisterWrite Pattern                                
                                new string[] {"set_loop(reg3)","0", "0", "1", "0",""},
                                new string[] {"cmdstart1:\nsource", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"end_loop(cmdstart1)", "0", "-", "1", "-", ""}
                                #endregion
                            };

                            List<string[]> patternTrig = new List<string[]>
                            {
                            #region RegisterWrite Pattern                                                               
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                 new string[] {"set_signal(event0)", "0", "0", "1", trigval, "Turn On PXI Backplane Trigger if enabled. Send Digital Pin Trigger if enabled."},
                                new string[] {"repeat(49)", "0", "0", "1", trigval, "Continue Sending Digital Pin Trigger if enabled."},
                                new string[] {"clear_signal(event0)", "0", "0", "1", "0", "PXI Backplane Trigger Off (if enabled). Digital Pin Trigger Off."},
                                new string[] {"repeat(49)", "0", "0", "1", "0", "Digital Pin Trigger Off."}
                                #endregion
                            };
                            List<string[]> triggerDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };

                            List<string[]> BeforetriggerDelay = new List<string[]>
                            {
                              new string[] {"repeat(reg1)", "0", "0", "1", "0", "Repeat Delay"},
                              new string[] {"", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };

                            List<string[]> AftertriggerDelay = new List<string[]>
                            {
                                new string[] {"repeat(reg2)", "0", "0", "1", "0", "Repeat Delay"},
                                new string[] { "", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };

                            List<string[]> Halt = new List<string[]>
                            {
                            #region  Halt
                                new string[] { "", "0", "0", "1", "0", "Idle" },
                                new string[] {"repeat(1000)", "0", "0", "1", "X", "Idle"},  //200us delay to allow trigger command to take effect in CMOS controller
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };


                            // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                            List<string[]> pattern = new List<string[]> { };
                            pattern = pattern.Concat(patternStart).ToList();
                            pattern = pattern.Concat(patternBeforeTrig).ToList();
                            pattern = pattern.Concat(BeforetriggerDelay).ToList();
                            pattern = pattern.Concat(patternTrig).ToList();
                            pattern = pattern.Concat(AftertriggerDelay).ToList();
                            pattern = pattern.Concat(patternAfterTrig).ToList();
                            pattern = pattern.Concat(Halt).ToList();


                            if (isRZ) pattern = RemoveNRZvector(pattern);

                            if (isShareBus)
                            {
                                pattern = Pairsperator(p, pattern);
                                //PatternName += PairString;
                            }

                            // Generate and load Pattern from the formatted array.
                            // Note: Regsiter read/write patterns are generated as Return to Zero Patterns

                            //+ i.ToString() + PairString
                            if (!this.GenerateAndLoadPattern(PatternName + i + PairString, pins, pattern, true, WriteTimeSet))
                            {
                                throw new Exception(PatternName);
                            }

                            //HSDIO.datalogResults["TimingExtendedRegisterWrite" + "Pair" + p.ToString()] = false;
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }


            /// <summary>
            /// Internal Function: Used to generate and load the non-extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_BurstingTest(Timeset WriteTimeSet)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    int MaxPairIndex = (isShareBus ? 3 : 2);

                    for (int mipiPairs = 1; mipiPairs < MaxPairIndex; mipiPairs++)
                    {
                        for (int i = 1; i <= 1; i++)
                        {
                            int p = mipiPairs; // index for MIPI pair
                            string PairString = "";
                            string PatternName = "BurstExtendedRegisterWriteReg";

                            string[] pins;
                            // Generate MIPI / RFFE Extended Register Write Patterns
                            if (!isShareBus)
                                pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                            else
                            {
                                pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                                PairString += "Pair" + p;
                            }

                            string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                            List<string[]> patternStart = new List<string[]>
                            {
                            #region RegisterWrite Pattern
                                new string[] { "source_start(SrcBurstExtendedRegisterWriteReg" + i + PairString + ")", "0", "0", "1", "0", "Configure source"},
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"}
                               
                                #endregion
                            };

                            List<string[]> patternFlow = new List<string[]> { };
                            List<string[]> patternTrigWait = new List<string[]> { };

                            List<string[]> patternBeforeDelay = new List<string[]> { };
                            List<string[]> patternAfterDelay = new List<string[]> { };

                            List<string[]> Halt = new List<string[]>
                            {
                            #region  Halt
                                new string[] { "burst_end:\nreset_trigger(trig0)", "0", "0", "1", "0", "Idle" },
                                new string[] {"repeat(1000)", "0", "0", "1", "X", "Idle"},  //200us delay to allow trigger command to take effect in CMOS controller
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };


                            // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                            List<string[]> pattern = new List<string[]> { };
                            pattern = pattern.Concat(patternStart).ToList();
                            //to check for trigger and loop 
                            //if trigger 0 =true, jump to the end of the pattern
                            for (int n = 1; n <= 36; n++)
                            {
                                patternFlow = new List<string[]>
                                {
                                    #region Flow of Pattern
                                    new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" },
                                    new string[] {"cmdflowServo"+ n.ToString() +":\nrepeat(260)", "0", "0", "1", "0", ""},
                                    new string[] {"jump_if(trig0, burst_end)", "0", "0", "1", "0", ""},
                                    new string[] { "exit_loop_if(trig1, cmdflowendServo"+ n.ToString() +")", "0", "0", "1", "0", ""},
                                    new string[] { "end_loop(cmdflowServo"+ n.ToString() +")", "0", "0", "1", "0", " wait for trigger" },
                                    new string[] { "cmdflowendServo"+ n.ToString() + ":\nreset_trigger(trig1)", "0", "0", "1", "0", " wait for trigger" }
                                    #endregion
                                };

                                patternTrigWait = new List<string[]>
                                {
                                    #region Flow of Pattern
                                    new string[] { "set_loop(65535)", "0", "0", "1", "0", " Wait for Trigger" },
                                    new string[] {"cmdTrigWait"+ n.ToString() +":\nrepeat(260)", "0", "0", "1", "0", ""},
                                    new string[] {"jump_if(trig0, burst_end)", "0", "0", "1", "0", ""},
                                    new string[] { "exit_loop_if(trig1, cmdTrigWaitend"+ n.ToString() +")", "0", "0", "1", "0", ""},
                                    new string[] { "end_loop(cmdTrigWait"+ n.ToString() +")", "0", "0", "1", "0", " wait for trigger" },
                                    new string[] { "cmdTrigWaitend"+ n.ToString() + ":\nreset_trigger(trig1)", "0", "0", "1", "0", " wait for trigger" }
                                    #endregion
                                };

                                patternBeforeDelay = new List<string[]>
                                {
                                #region RegisterWrite Pattern                                
                                    new string[] {"set_loop(1)","0", "0", "1", "0",""},
                                    new string[] {"cmdstartServo"+ n.ToString()+":\nsource", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                    new string[] {"end_loop(cmdstartServo"+ n.ToString() +")", "0", "-", "1", "-", ""},
                                     new string[] {"", "0", "-", "1", "-", ""},
                                    #endregion
                                };

                                patternAfterDelay = new List<string[]>
                                {
                                #region RegisterWrite Pattern                                
                                    new string[] {"set_loop(1)","0", "0", "1", "0",""},
                                    new string[] {"cmdstart1Servo"+ n.ToString() +":\nsource", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "0", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", ""},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                                    new string[] {"", "0", "-", "1", "-", ""},
                                    new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                    new string[] {"end_loop(cmdstart1Servo"+ n.ToString() +")", "0", "-", "1", "-", ""}
                                    #endregion
                                };

                                pattern = pattern.Concat(patternFlow).ToList();
                                pattern = pattern.Concat(patternBeforeDelay).ToList();
                                pattern = pattern.Concat(patternTrigWait).ToList();
                                pattern = pattern.Concat(patternAfterDelay).ToList();
                            }
                            pattern = pattern.Concat(Halt).ToList();


                            if (isRZ) pattern = RemoveNRZvector(pattern);

                            if (isShareBus)
                            {
                                pattern = Pairsperator(p, pattern);
                            }

                            // Generate and load Pattern from the formatted array.
                            // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                            if (!this.GenerateAndLoadPattern(PatternName + i + PairString, pins, pattern, true, WriteTimeSet))
                            {
                                throw new Exception(PatternName);
                            }
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MultipleMipiExtendedRegWriteWithReg(Timeset WriteTimeSet, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Extended Register Write Patterns
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }



                    string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                                 new string[] {"set_loop(reg0)","0", "0", "1", "0",""},
                                new string[] {"cmdstart:\nsource", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                    List<string[]> writeData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };

                    List<string[]> EndLoop = new List<string[]>
                            {
                            #region EndLoop
                                 new string[] {"end_loop(cmdstart)", "0", "-", "1", "-", ""},
                                 new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };

                    List<string[]> Idle = new List<string[]>
                            {
                            #region  Idle
                                new string[] {"repeat(30)", "0", "0", "1", "0", "Idle"},

                            #endregion
                            };

                    List<string[]> Halt = new List<string[]>
                            {
                            #region  Halt
                                new string[] {"endofpattern:\nrepeat(1000)", "0", "0", "1", "X", "Idle"},  //200us delay to allow trigger command to take effect in CMOS controller
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };



                    // concat pieces into a single Register write 
                    List<string[]> pattern = new List<string[]> { };
                    pattern = pattern.Concat(patternStart).ToList();

                    pattern = pattern.Concat(writeData).ToList();

                    pattern = pattern.Concat(busPark).ToList();

                    pattern = pattern.Concat(EndLoop).ToList();



                    // Generate full pattern with the correct number of MIPI register writes
                    List<string[]> multiWritePattern = new List<string[]>
                        {
                            new string[] {"source_start(SrcMultipleExtendedRegisterWritewithreg" + PairString + ")", "0", "0", "1", "0", "Configure source"}
                        };


                    multiWritePattern = multiWritePattern.Concat(pattern).ToList();
                    multiWritePattern = multiWritePattern.Concat(Idle).ToList();
                    multiWritePattern = multiWritePattern.Concat(Halt).ToList();


                    if (isRZ) multiWritePattern = RemoveNRZvector(multiWritePattern);
                    multiWritePattern = Pairsperator(_pair, multiWritePattern);

                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("MultipleExtendedRegisterWritewithreg" + PairString, pins, multiWritePattern, true, WriteTimeSet))
                    {
                        throw new Exception("Compile Failed: MultipleExtendedRegisterWritewithreg");
                    }


                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MultipleMipiExtendedRegMaskedWriteWithReg(Timeset WriteTimeSet, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    double ClkRate_byTimeset = MIPIClockRate;

                    if (WriteTimeSet.ToString().Contains("HALF"))
                    {
                        ClkRate_byTimeset = MIPIClockRate / 2;
                    }
                    else if (WriteTimeSet.ToString().Contains("QUAD"))
                    {
                        ClkRate_byTimeset = MIPIClockRate / 4;
                    }
                    var DelayForEachCmdFrames = 0;// Math.Ceiling((100e-9) * ClkRate_byTimeset);

                    // Generate MIPI / RFFE Extended Register Write Patterns
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }

                    string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern

                                new string[] { $"repeat({Math.Ceiling(3e-6 * ClkRate_byTimeset)})", "0", "0", "1", "0", "Idle"},
                                 new string[] {"set_loop(reg0)","0", "0", "1", "0",""},
                                new string[] {"cmdstart:\nsource", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}

                                #endregion ExtendedRegisterWrite Pattern
                            };
                    List<string[]> writeData = new List<string[]>
                            {
                            #region Write Data...
                            new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion Write Data...
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                            new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}

                            #endregion Bus Park
                            };
                    List<string[]> EndLoop = new List<string[]>
                            {
                            #region EndLoop

                                 new string[] {"end_loop(cmdstart)", "0", "-", "1", "-", ""},
                                 new string[] {"", "0", "-", "1", "-", ""}

                            #endregion EndLoop
                            };
                    List<string[]> Idle = new List<string[]>
                            {
                            #region Idle

                                new string[] {"repeat(30)", "0", "0", "1", "0", "Idle"},

                            #endregion Idle
                            };

                    List<string[]> Halt = new List<string[]>
                            {
                            #region Halt

                                new string[] { $"endofpattern:\nrepeat({Math.Ceiling((20e-6) * ClkRate_byTimeset)})", "0", "0", "1", "X", "Idle"},  //200us delay to allow trigger command to take effect in CMOS controller
                                new string[] {"halt", "0", "0", "1", "X", ""}

                            #endregion Halt
                            };
                    List<string[]> CmdDelay = new List<string[]>
                            {
                            #region CmdDelay
                                new string[] {$"repeat({DelayForEachCmdFrames})", "0", "0", "1", "0", "CmdDelay"},
                            #endregion CmdDelay
                            };

                    // concat pieces into a single Register write
                    List<string[]> pattern = new List<string[]> { };
                    //pattern = pattern.Concat(patternStart).ToList();
                    //pattern = pattern.Concat(writeData).ToList(); //Mask data..
                    //pattern = pattern.Concat(writeData).ToList();
                    //pattern = pattern.Concat(busPark).ToList();
                    //pattern = pattern.Concat(EndLoop).ToList();

                    pattern.AddRange(patternStart);
                    pattern.AddRange(writeData); //Mask data..
                    pattern.AddRange(writeData);
                    pattern.AddRange(busPark);
                    if (DelayForEachCmdFrames > 0)
                        pattern.AddRange(CmdDelay);
                    pattern.AddRange(EndLoop);

                    // Generate full pattern with the correct number of MIPI register writes
                    List<string[]> multiWritePattern = new List<string[]>
                    {
                        new string[] { "source_start(SrcMultipleExtendedRegisterMaskedWritewithregPair" + _pair + ")", "0", "0", "1", "0", "Configure source"}
                    };

                    multiWritePattern.AddRange(pattern);
                    multiWritePattern.AddRange(Idle);
                    multiWritePattern.AddRange(Halt);

                    if (isRZ) multiWritePattern = RemoveNRZvector(multiWritePattern);
                    multiWritePattern = Pairsperator(_pair, multiWritePattern);

                    // Generate and load Pattern from the formatted array.
                    // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                    if (!this.GenerateAndLoadPattern("MultipleExtendedRegisterMaskedWritewithreg" + PairString, pins, multiWritePattern, true, WriteTimeSet))
                    {
                        throw new Exception("Compile Failed: MultipleExtendedRegisterMaskedWritewithreg");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_RxOTPBurnTemplate(Timeset WriteTimeSet, double ClkRate_byTimeset, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    double ClockRate = ClkRate_byTimeset;

                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }

                    List<string[]> pattern;
                    pattern = CreatorLNAOTPpattern(Get_Digital_Definition("RX_FABSUPPLIER"), ClkRate_byTimeset);

                    if (isRZ) pattern = RemoveNRZvector(pattern);
                    pattern = Pairsperator(2, pattern);

                    if (!this.GenerateAndLoadPattern("RxOTPBurnTemplate", pins, pattern, true, WriteTimeSet))
                    {
                        throw new Exception("Compile Failed: RxOTPBurnTemplate");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RxOTPBurnTemplate vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private bool LoadVector_TxOTPBurnTemplate(Timeset WriteTimeSet, double ClkRate_byTimeset, int _pair = 0)
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Extended Register Write Patterns
                    //string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                    string[] pins;
                    string PairString = "";
                    if (_pair == 0)
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), TrigChanName.ToUpper() };
                    }
                    else
                    {
                        pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };
                        PairString = "Pair" + _pair;
                    }

                    List<string[]> pattern;
                    pattern = CreatorCmosOTPpattern_SeoulCMOS(Get_Digital_Definition("TX_FABSUPPLIER"), ClkRate_byTimeset);
                    //pattern = CreatorCmosOTPpattern(Get_Digital_Definition("VIH"), ClkRate_byTimeset);

                    if (isRZ) pattern = RemoveNRZvector(pattern);
                    pattern = Pairsperator(1, pattern);

                    //if (!this.GenerateAndLoadPattern("TxOTPBurnTemplate", pins, pattern, true, Timeset.MIPI, Timeset.MIPI_HALF, false))
                    if (!this.GenerateAndLoadPattern("TxOTPBurnTemplate", pins, pattern, true, WriteTimeSet))
                    {
                        throw new Exception("Compile Failed: TxOTPBurnTemplate");
                    }

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi TxOTPBurnTemplate vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            private List<string[]> CreatorCmosOTPpattern(string _violevel, double ClkRate_byTimeset)
            {
                if (_violevel == "1.8")
                {
                    int Parity_Count = 0;
                    string SlaveBits = Convert.ToString(Convert.ToInt32(Get_Digital_Definition("MIPI1_SLAVE_ADDR"), 16), 2).PadLeft(4, '0');
                    foreach (char _bits in SlaveBits) { if (_bits == '1') Parity_Count++; }

                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "1", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };
                    List<string[]> SoucewriteData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };
                    List<string[]> CmdDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Delay Cycle" }
                            };

                    List<string[]> CmdDelayRep20uS = new List<string[]>
                            {
                         //new string[] { "repeat("+ (int)(20e-6 * 26e6) +")", "0", "0", "1", "0", "Delay Cycle" }
                         new string[] { "repeat("+ (int)(20e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "Delay Cycle" }
                         //new string[] { "repeat("+ (int)(20e-6 * MIPIClockRate) +")", "0", "0", "1", "0", "Delay Cycle" } 
                       //  new string[] { "repeat("+ (int)(260)+")", "0", "0", "1", "0", "Delay Cycle" } //13Mhz
                            };
                    List<string[]> CmdDelayRep2uS = new List<string[]>
                            {
                         //new string[] { "repeat("+ (int)(2e-6 * 26e6) +")", "0", "0", "1", "0", "Delay Cycle" }
                         new string[] { "repeat("+ (int)(2e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "Delay Cycle" }
                            };
                    List<string[]> patternEnd = new List<string[]>   //Data 10
                            {
                            #region patternEnd...
                                new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"halt", "0", "0", "1", "0", ""}
                            #endregion
                            };

                    //////////////////////////////////////////////////////////////////////////////////////////////////

                    List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcTxOTPBurnTemplate)", "0", "0", "1", "0", "Configure source"},
                                    new string[] {"repeat(300)", "0", "0", "1", "0", "delay"}
                                };

                    for (int i = 1; i <= 3; i++)
                    {
                        if (i == 3)
                            pattern = pattern.Concat(CmdDelayRep20uS).ToList();
                        else
                            pattern = pattern.Concat(CmdDelayRep2uS).ToList();

                        pattern = pattern.Concat(patternStart).ToList();
                        pattern = pattern.Concat(SoucewriteData).ToList();
                        pattern = pattern.Concat(busPark).ToList();
                        pattern = pattern.Concat(CmdDelay).ToList();
                    }

                    pattern = pattern.Concat(patternEnd).ToList();

                    return pattern;
                }
                else
                {

                    MessageBox.Show("Failed to generate Cmos pattern for " + _violevel, "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new List<string[]>();
                }
            }
            private List<string[]> CreatorCmosOTPpattern_SeoulOTPDesign(string _violevel, double ClkRate_byTimeset)
            {
                if (true) // _violevel == "1.8")
                {
                    int Parity_Count = 0;
                    string SlaveBits = Convert.ToString(Convert.ToInt32(Get_Digital_Definition("MIPI1_SLAVE_ADDR"), 16), 2).PadLeft(4, '0');
                    foreach (char _bits in SlaveBits) { if (_bits == '1') Parity_Count++; }

                    List<string[]> patternF0 = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "1", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 7 xF0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };

                    List<string[]> patternF0tox80 = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "1", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 7 xF0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "1", "1", "0", "Write Data 7 x80"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };

                    List<string[]> patternF0tox00 = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "1", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 7 xF0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 7 x00"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "1", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };

                    List<string[]> pattern23tox80 = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "1", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 7 x23"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "1", "1", "0", "Write Data 7 x80"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "0", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };

                    List<string[]> SoucewriteData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };

                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };

                    List<string[]> CmdDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Delay Cycle" }
                            };

                    List<string[]> CmdDelayRep20uS = new List<string[]>
                            {
                         //new string[] { "repeat("+ (int)(20e-6 * 26e6) +")", "0", "0", "1", "0", "Delay Cycle" }
                         new string[] { "repeat("+ (int)(20e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "Delay Cycle" }
                         //new string[] { "repeat("+ (int)(20e-6 * MIPIClockRate) +")", "0", "0", "1", "0", "Delay Cycle" } 
                       //  new string[] { "repeat("+ (int)(260)+")", "0", "0", "1", "0", "Delay Cycle" } //13Mhz
                            };

                    List<string[]> CmdDelayRep2uS = new List<string[]>
                            {
                         //new string[] { "repeat("+ (int)(2e-6 * 26e6) +")", "0", "0", "1", "0", "Delay Cycle" }
                         new string[] { "repeat("+ (int)(2e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "Delay Cycle" }
                            };

                    List<string[]> patternEnd = new List<string[]>   //Data 10
                            {
                            #region patternEnd...
                                new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"halt", "0", "0", "1", "0", ""}
                            #endregion
                            };


                    //////////////////////////////////////////////////////////////////////////////////////////////////

                    List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcTxOTPBurnTemplate)", "0", "0", "1", "0", "Configure source"},
                                    new string[] {"repeat(300)", "0", "0", "1", "0", "delay"}
                                };

                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(patternF0tox80).ToList();
                    pattern = pattern.Concat(CmdDelayRep20uS).ToList();
                    pattern = pattern.Concat(patternF0).ToList();
                    pattern = pattern.Concat(SoucewriteData).ToList();
                    pattern = pattern.Concat(busPark).ToList();
                    pattern = pattern.Concat(CmdDelay).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    //pattern = pattern.Concat(CmdDelayRep2uS).ToList();
                    //pattern = pattern.Concat(CmdDelayRep20uS).ToList();
                    pattern = pattern.Concat(pattern23tox80).ToList();
                    pattern = pattern.Concat(patternF0tox00).ToList();
                    pattern = pattern.Concat(patternEnd).ToList();


                    #region Commented out for loop
                    //for (int i = 1; i <= 3; i++)
                    //{
                    //    if (i == 3)
                    //        pattern = pattern.Concat(CmdDelayRep20uS).ToList();
                    //    else
                    //        pattern = pattern.Concat(CmdDelayRep2uS).ToList();

                    //    pattern = pattern.Concat(patternF0).ToList();
                    //    pattern = pattern.Concat(SoucewriteData).ToList();
                    //    pattern = pattern.Concat(busPark).ToList();
                    //    pattern = pattern.Concat(CmdDelay).ToList();
                    //}

                    //pattern = pattern.Concat(patternEnd).ToList(); 
                    #endregion Commented out for loop


                    return pattern;
                }
                else
                {

                    MessageBox.Show("Failed to generate Cmos pattern for " + _violevel, "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new List<string[]>();
                }
            }

            private List<string[]> CreatorCmosOTPpattern_SeoulCMOS(string _fabSupplier, double ClkRate_byTimeset)
            {
                double cDelay;
                List<string[]> patternStart, SoucewriteData, busPark, CmdDelay, pattern, multiWritePattern, EndLoop, Idle, Halt, pattern_Addr0xF0;

                int Parity_Count = 0;
                string SlaveBits = Convert.ToString(Convert.ToInt32(Get_Digital_Definition("MIPI1_SLAVE_ADDR"), 16), 2).PadLeft(4, '0');
                foreach (char _bits in SlaveBits) { if (_bits == '1') Parity_Count++; }

                #region CommonVectors
                busPark = new List<string[]>
                {
                    #region Bus Park
                    new string[] {"", "1", "0", "1", "0", "Bus Park"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    #endregion
                };

                CmdDelay = new List<string[]>
                {
                    new string[] { "repeat(10)", "0", "0", "1", "0", "Delay Cycle" }
                };

                Idle = new List<string[]>
                {
                #region  Idle
                    new string[] {"repeat(30)", "0", "0", "1", "0", "Idle"},
                #endregion
                };

                Halt = new List<string[]>
                {
                #region  Halt
                    new string[] {$"endofpattern:\nrepeat({Math.Ceiling(20e-6 * ClkRate_byTimeset)})", "0", "0", "1", "X", "Idle, 20uS"},  //200us delay to allow trigger command to take effect in CMOS controller
                    new string[] {"halt", "0", "0", "1", "X", ""}
                #endregion
                };

                EndLoop = new List<string[]>
                {
                #region EndLoop
                     new string[] {"end_loop(cmdstart)", "0", "-", "1", "-", ""},
                     new string[] {"", "0", "-", "1", "-", ""}
                #endregion
                };

                SoucewriteData = new List<string[]>
                {
                    #region Write Data...
                    new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"source", "1", "D", "1", "0", "Parity"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    #endregion
                };

                pattern = new List<string[]>
                {
                    new string[] {"source_start(SrcTxOTPBurnTemplate)", "0", "0", "1", "0", "Configure source"},
                    new string[] {"repeat(300)", "0", "0", "1", "0", "delay"}
                };

                pattern_Addr0xF0 = new List<string[]>
                        {
                        #region ExtendedRegisterWrite Pattern
                            //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                            new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "0", "1", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "0", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 7  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 6  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 5  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 4  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 3  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 2  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 1  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 0  0xF0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                        };

                List<string[]> CmdDelayRep2uS = new List<string[]>
                {
                    new string[] { "repeat("+ (int)(2e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "2us Delay Cycle" }
                };

                List<string[]> CmdDelayRep10uS = new List<string[]>
                {
                    new string[] { "repeat("+ (int)(10e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "10us Delay Cycle" }
                };
                List<string[]> CmdDelayRep20uS = new List<string[]>
                {
                    new string[] { "repeat("+ (int)(20e-6 * ClkRate_byTimeset) +")", "0", "0", "1", "0", "20us Delay Cycle" }
                };

                List<string[]> CmdDelayRep100uS = new List<string[]>
                {
                    new string[] { "repeat("+ (int)(100e-6 * ClkRate_byTimeset) +")", "1", "0", "1", "0", "100us Delay Cycle" }
                };

                List<string[]> patternEnd = new List<string[]>   //Data 10
                {
                    #region patternEnd...
                    new string[] {"repeat(100)", "0", "0", "1", "0", "Idle"},
                    new string[] {"", "0", "-", "1", "-", ""},
                    new string[] {"halt", "0", "0", "1", "0", ""}
                    #endregion
                };
                #endregion CommonVectors

                switch (_fabSupplier)
                {
                    case "TSMC_130NM":
                        {
                            #region CMOS_130nm
                            List<string[]> patternStart_0xC0 = new List<string[]>
                        {
                        #region ExtendedRegisterWrite Pattern
                            //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                            new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "0", "1", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "0", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 7  0xC0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 6"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 5"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 4"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                        };
                            List<string[]> patternStart_0xC4 = new List<string[]>
                        {
                        #region ExtendedRegisterWrite Pattern
                            //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                            new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "0", "1", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "0", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", ((Parity_Count % 2 == 0) ? "1" : "0"), "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 7 0xC4"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 6"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 5"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 4"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                        };

                            for (int i = 1; i <= 3; i++)
                            {
                                if (i == 3)
                                    pattern = pattern.Concat(CmdDelayRep100uS).ToList();
                                else
                                    pattern = pattern.Concat(CmdDelay).ToList();

                                if (i == 1)
                                    pattern = pattern.Concat(patternStart_0xC4).ToList();

                                else
                                    pattern = pattern.Concat(patternStart_0xC0).ToList();
                                pattern = pattern.Concat(SoucewriteData).ToList();
                                pattern = pattern.Concat(busPark).ToList();
                                pattern = pattern.Concat(CmdDelay).ToList();
                            }

                            pattern = pattern.Concat(patternEnd).ToList();

                            return pattern;
                            #endregion CMOS_130nm
                        }
                    case "TSMC_65NM":
                        {
                            patternStart = new List<string[]>
                        {
                            #region ExtendedRegisterWrite Pattern
                            new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                            new string[] {"set_loop(reg0)","0", "0", "1", "0",""},
                            new string[] {"cmdstart:\nsource", "0", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"source", "0", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                            new string[] {"", "0", "-", "1", "-", ""},

                            new string[] { "source", "0", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "0", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Slave Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Slave Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Slave Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Slave Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},

                            new string[] { "source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", ""},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Byte Count 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Byte Count 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Byte Count 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Byte Count 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},

                            new string[] { "source", "1", "D", "1", "0", "Address 7"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"source", "1", "D", "1", "0", "Address 6"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Address 5"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Address 4"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Address 3"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Address 2"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Address 1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Address 0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] { "source", "1", "D", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""}, 
                            #endregion ExtendedRegisterWrite Pattern
                        };

                            List<string[]> writeData0xE0 = new List<string[]>   //Data E0
                        {
                            #region Write Data...
                            new string[] {"", "1", "1", "1", "0", "Write Data 7 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Write Data 6 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Write Data 5 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 4 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 3 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 2 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 1 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 0 0xE0"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                        };

                            List<string[]> writeData0xE1 = new List<string[]>   //Data E1
                        {
                            #region Write Data...
                            new string[] {"", "1", "1", "1", "0", "Write Data 7 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Write Data 6 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Write Data 5 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 4 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 3 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 2 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "0", "1", "0", "Write Data 1 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Write Data 0 0xE1"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            new string[] {"", "1", "1", "1", "0", "Parity"},
                            new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                        };

                            /// This pattern has 64 bits program & Efuse Program switch off 
                            /// Required below command
                            /// 1. before pattern burst : Efuse power & program switch on 
                            /// 2. after pattern burst : Efuse power switch off 
                            //pattern = new List<string[]> { };
                            //pattern.AddRange(pattern);

                            pattern.AddRange(CmdDelay);
                            for (int i = 0; i < 64; i++) //replicate the 4 commands write frame 64 times to write all 64 Efuse bits 
                            {
                                pattern.AddRange(pattern_Addr0xF0);
                                pattern.AddRange(SoucewriteData);
                                pattern.AddRange(busPark);
                                pattern.AddRange(CmdDelayRep10uS);

                                pattern.AddRange(pattern_Addr0xF0);
                                pattern.AddRange(SoucewriteData);
                                pattern.AddRange(busPark);
                                pattern.AddRange(CmdDelayRep10uS);

                                pattern.AddRange(pattern_Addr0xF0);
                                //pattern.AddRange(writeData0xE1);
                                pattern.AddRange(SoucewriteData);
                                pattern.AddRange(busPark);
                                pattern.AddRange(CmdDelayRep10uS);

                                pattern.AddRange(pattern_Addr0xF0);
                                //pattern.AddRange(writeData0xE0);
                                pattern.AddRange(SoucewriteData);
                                pattern.AddRange(busPark);
                                pattern.AddRange(CmdDelayRep10uS);
                            }
                            pattern.AddRange(patternEnd);

                            //pattern.AddRange(EndLoop);

                            //multiWritePattern = new List<string[]>
                            //{
                            //     new string[] {"source_start(SrcTxOTPBurnTemplate)", "0", "0", "1", "0", "Configure source"},
                            //};

                            //multiWritePattern.AddRange(pattern);
                            //multiWritePattern.AddRange(Idle);
                            //multiWritePattern.AddRange(Halt);

                            //return multiWritePattern;
                            return pattern;
                        }
                    default:
                        MessageBox.Show("Failed to generate Cmos pattern for VIO: " + _fabSupplier, "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return new List<string[]>();
                }
            }


            private List<string[]> CreatorLNAOTPpattern(string _FabSupplier, double ClkRate_byTimeset)
            {
                if (_FabSupplier == "TSMC")
                {
                    #region TSMC
                    double cDelay = 3e-6;
                    // Generate MIPI / RFFE Extended Register Write Patterns
                    //string[] pins = new string[] { Sclk1ChanName.ToUpper(), Sdata1ChanName.ToUpper(), Vio1ChanName.ToUpper(), Sclk2ChanName.ToUpper(), Sdata2ChanName.ToUpper(), Vio2ChanName.ToUpper(), TrigChanName.ToUpper() };

                    int Parity_Count = 0;
                    string SlaveBits = Convert.ToString(Convert.ToInt32(Get_Digital_Definition("MIPI2_SLAVE_ADDR"), 16), 2).PadLeft(4, '0');
                    foreach (char _bits in SlaveBits) { if (_bits == '1') Parity_Count++; }

                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                //new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"", "0", "0", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "1", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "0", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(0, 1), "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(1, 1), "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(2, 1), "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", SlaveBits.Substring(3, 1), "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", ((Parity_Count%2==0)?"1":"0"), "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                #endregion
                            };
                    List<string[]> SoucewriteData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };
                    List<string[]> CmdDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Delay Cycle" },
                                new string[] {"", "0", "-", "1", "-", ""},
                            };
                    List<string[]> writeData0x20 = new List<string[]>   //Data 20
                            {
                            #region Write Data...
                                new string[] {"", "1", "0", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "1", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                            #endregion
                            };
                    List<string[]> patternEnd = new List<string[]>   //Data 10
                            {
                            #region patternEnd...
                                new string[] {"repeat(10)", "0", "0", "1", "0", "Idle"},
                                new string[] {"halt", "0", "0", "1", "0", ""}
                            #endregion
                            };


                    /// This pattern have 64 bits program & Efuse Program switch off 
                    /// Required below command
                    /// 1. before pattern burst : Efuse power & program switch on 
                    /// 2. after pattern burst : Efuse power switch off 
                    List<string[]> writeData;
                    List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcRxOTPBurnTemplate)", "0", "0", "1", "0", "Configure source"},
                                    new string[] {"repeat(300)", "0", "0", "1", "0", "delay"}
                                };

                    for (int i = 1; i <= 65; i++)
                    {
                        if (i != 65) writeData = new List<string[]>(SoucewriteData);
                        else writeData = new List<string[]>(writeData0x20);

                        if (i == 1)
                        {
                            for (int ff = 0; ff < 10; ff++)
                                pattern = pattern.Concat(CmdDelay).ToList();
                        }
                        else
                        {
                            for (int ff = 0; ff < cDelay * ClkRate_byTimeset; ff++)
                                pattern = pattern.Concat(CmdDelay).ToList();
                        }

                        pattern = pattern.Concat(patternStart).ToList();
                        pattern = pattern.Concat(writeData).ToList();
                        pattern = pattern.Concat(busPark).ToList();
                        pattern = pattern.Concat(CmdDelay).ToList();
                    }

                    pattern = pattern.Concat(patternEnd).ToList();

                    return pattern;
                    #endregion
                }
                if (_FabSupplier == "GF")
                {
                    #region Grobal Foundry
                    double cDelay = 600e-6;

                    List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                                 new string[] {"set_loop(reg0)","0", "0", "1", "0",""},
                                new string[] {"cmdstart:\nsource", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                    List<string[]> writeData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                    List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };

                    List<string[]> EndLoop = new List<string[]>
                            {
                            #region EndLoop
                                 new string[] {"end_loop(cmdstart)", "0", "-", "1", "-", ""},
                                 new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };

                    List<string[]> Idle = new List<string[]>
                            {
                            #region  Idle
                                new string[] {"repeat(30)", "0", "0", "1", "0", "Idle"},

                            #endregion
                            };

                    List<string[]> Halt = new List<string[]>
                            {
                            #region  Halt
                                new string[] {"endofpattern:\nrepeat(1000)", "0", "0", "1", "X", "Idle"},  //200us delay to allow trigger command to take effect in CMOS controller
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                    List<string[]> CmdDelay = new List<string[]>
                            {
                            #region  CmdDelay
                                new string[] {"repeat(" + cDelay * ClkRate_byTimeset + ")", "0", "0", "1", "0", "Idle"},
                            #endregion
                            };

                    // concat pieces into a single Register write 
                    List<string[]> pattern = new List<string[]> { };
                    pattern = pattern.Concat(patternStart).ToList();

                    pattern = pattern.Concat(writeData).ToList();

                    pattern = pattern.Concat(busPark).ToList();

                    pattern = pattern.Concat(CmdDelay).ToList();

                    pattern = pattern.Concat(EndLoop).ToList();



                    // Generate full pattern with the correct number of MIPI register writes
                    List<string[]> multiWritePattern = new List<string[]>
                        {
                             new string[] {"source_start(SrcRxOTPBurnTemplate)", "0", "0", "1", "0", "Configure source"},
                        };


                    multiWritePattern = multiWritePattern.Concat(pattern).ToList();
                    multiWritePattern = multiWritePattern.Concat(Idle).ToList();
                    multiWritePattern = multiWritePattern.Concat(Halt).ToList();


                    return multiWritePattern;
                    #endregion
                }
                else
                {

                    MessageBox.Show("Failed to generate Cmos pattern for " + _FabSupplier, "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new List<string[]>();
                }
            }
            private List<string[]> RemoveNRZvector(List<string[]> VectorList)
            {
                var toRemove = new HashSet<string[]>();

                //To Set NRZ pattern to remove
                string[] remove1 = new string[] { "", "0", "X", "1", "-", "" };
                string[] remove2 = new string[] { "", "0", "-", "1", "-", "" };

                for (int i = 0; i < VectorList.Count; i++)
                {
                    if (VectorList[i].SequenceEqual(remove1) || VectorList[i].SequenceEqual(remove2))
                        VectorList[i] = null;
                }

                VectorList.RemoveAll(s => s == null);

                return VectorList;
            }
            private List<string[]> Pairsperator(int Pair, List<string[]> VectorList)
            {
                if (!isShareBus) return VectorList;

                if (Pair == 1)
                {
                    for (int PattenIndex = 0; PattenIndex < VectorList.Count; PattenIndex++)
                    {
                        VectorList[PattenIndex] = new string[] {
                           VectorList[PattenIndex][0],
                           VectorList[PattenIndex][1],
                           VectorList[PattenIndex][2],
                           VectorList[PattenIndex][3],
                            "X", "X", "1",
                           VectorList[PattenIndex][4],
                           VectorList[PattenIndex][5] };
                    }
                }
                else if (Pair == 2)
                {
                    for (int PattenIndex = 0; PattenIndex < VectorList.Count; PattenIndex++)
                    {
                        VectorList[PattenIndex] = new string[] {
                           VectorList[PattenIndex][0],
                             "X", "X", "1",
                           VectorList[PattenIndex][1],
                           VectorList[PattenIndex][2],
                           VectorList[PattenIndex][3],
                           VectorList[PattenIndex][4],
                           VectorList[PattenIndex][5] };
                    }
                }
                else
                {
                    MessageBox.Show("Failed to Sperate Mipi Pair\n", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return VectorList;
            }

            /// <summary>
            /// Dynamic Register Write function.  This uses NI 6570 source memory to dynamically change
            /// the register address and write values in the pattern.
            /// This supports extended and non-extended register write.
            /// </summary>
            /// <param name="pair">The MIPI pair number to write</param>
            /// <param name="slaveAddress_hex">The slave address to write (hex)</param>
            /// <param name="registerAddress_hex">The register address to write (hex)</param>
            /// <param name="data_hex">The data to write into the specified register in Hex.  Note:  Maximum # of bytes to write is 16.</param>
            /// <param name="sendTrigger">If True, this function will send either a PXI Backplane Trigger, a Digital Pin Trigger, or both based on the configuration in the NI6570 constructor.</param>
            public void SendTimingTestVector(int pair, string slaveAddress_hex, string registerAddress_hex, string data_hex, bool sendTrigger = false)
            {
                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    if (sendTrigger)
                    {
                        // Configure the NI 6570 to connect PXI_TrigX to "event0" that can be used with the set_signal, clear_signal, and pulse_trigger opcodes
                        if (triggerConfig == TrigConfig.Both || triggerConfig == TrigConfig.PXI_Backplane)
                        {
                            DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", pxiTrigger.ToString("g"));
                        }
                        else
                        {
                            // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                            DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");
                        }

                        // Set the Sequencer Flag 0 to indicate that a trigger should be sent on the TrigChan pin
                        if (triggerConfig == TrigConfig.Both || triggerConfig == TrigConfig.Digital_Pin)
                        {
                            DIGI.PatternControl.WriteSequencerFlag("seqflag0", true);
                        }
                        else
                        {
                            // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                            DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);
                        }

                        if (triggerConfig == TrigConfig.None)
                        {
                            throw new Exception("sendTrigger=True requested, but NI 6570 is not configured for Triggering.  Please update the NI6570 Constructor triggerConfig to use TrigConfig.Digital_Pin, TrigConfig.PXI_Backplane, or TrigConfig.Both.");
                        }
                    }
                    else
                    {
                        // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                        DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");

                        // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                        DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);
                    }

                    DIGI.PatternControl.Commit();

                    // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                    if (data_hex.Length % 2 == 1)
                        data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    bool extendedWrite = Convert.ToInt32(registerAddress_hex, 16) > 31;    // any register address > 5 bits requires extended read
                    uint writeByteCount = extendedWrite ? (uint)(data_hex.Length / 2) : 1;

                    string nameInMemory = "TimingExtendedRegisterWritePair" + pair.ToString();


                    string PinName;

                    if (pair == 0) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                    else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();

                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];
                    //if (!extendedWrite)
                    //{
                    //    // Build non-exteded write command
                    //    uint cmdBytesWithParity = generateRFFECommand(slaveAddress_hex, registerAddress_hex, Command.REGISTERWRITE);
                    //    // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                    //    dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                    //    dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                    //    dataArray[2] = calculateParity(Convert.ToUInt32(data_hex, 16)); // final 9 bits
                    //}
                    //else
                    //{
                    //    // Build extended read command data, setting read byte count and register address. 
                    //    // Note, write byte count is 0 indexed.
                    //    uint cmdBytesWithParity = generateRFFECommand(slaveAddress_hex, Convert.ToString(writeByteCount - 1, 16), Command.EXTENDEDREGISTERWRITE);
                    //    // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                    //    dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                    //    dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                    //    dataArray[2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits
                    //    // Convert Hex Data string to bytes and add to data Array
                    //    for (int i = 0; i < writeByteCount * 2; i += 2)
                    //        dataArray[3 + (i / 2)] = (uint)(calculateParity(Convert.ToByte(data_hex.Substring(i, 2), 16)));
                    //}
                    // Configure 6570 to source data calculated above
                    DIGI.SourceWaveforms.CreateSerial(PinName, "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                    // Write Sequencer Register reg0 = 1 -- access only one register address
                    DIGI.PatternControl.WriteSequencerRegister("reg0", 1);

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = nameInMemory;

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    Int64[] failureCount;
                    string CurrentSlaveAddress;
                    if (dutSlaveAddress.Length % 2 == 1)
                        CurrentSlaveAddress = dutSlaveAddress.PadLeft(dutSlaveAddress.Length + 1, '0');
                    else
                        CurrentSlaveAddress = dutSlaveAddress;

                    int Current_MIPI_Bus = Convert.ToUInt16(Get_Digital_Definition("SLAVE_ADDR_" + CurrentSlaveAddress));


                    if (Current_MIPI_Bus == 2)
                        failureCount = sdata2Pin.GetFailCount();
                    else
                        failureCount = sdata1Pin.GetFailCount();

                    //  NumBitErrors = (int)failureCount[EqHSDIO.Num_Mipi_Bus - Current_MIPI_Bus];
                    NumBitErrors = (int)failureCount[0];

                    if (debug) Console.WriteLine("Pair " + pair + " Slave " + slaveAddress_hex + ", RegWrite " + registerAddress_hex + " Bit Errors: " + NumBitErrors.ToString());
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Write Register for Address " + registerAddress_hex + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


            private Dictionary<string, string> SetConfigRegInitVals()
            {
                //The values set here are the defaults
                ConfigRegisterSettings["FaultQueueBit0"] = "0"; // 00 = 1 fault, 01 = 2 faults, 10 = 3 faults, 11 = 4 faults (This is bits 1:0)
                ConfigRegisterSettings["FaultQueueBit1"] = "0"; // 00 = 1 fault, 01 = 2 faults, 10 = 3 faults, 11 = 4 faults (This is bits 1:0)
                ConfigRegisterSettings["CTPinPolarity"] = "0";  // 0 = Active Low, 1 = Active High (This is bit 2)
                ConfigRegisterSettings["INTPinPolarity"] = "0"; // 0 = Active Low, 1 = Active High (This is bit 3)
                ConfigRegisterSettings["INTorCTMode"] = "0"; // 0 = Interrupt mode, 1 = Comparator mode (This is bit 4)
                ConfigRegisterSettings["OperationModeBit5"] = "0"; // 00 = Continuous conversion, 01 = One shot, 10 = 1 SPS mode, 11 = Shutdown (This is bits 6:5)
                ConfigRegisterSettings["OperationModeBit6"] = "0"; // 00 = Continuous conversion, 01 = One shot, 10 = 1 SPS mode, 11 = Shutdown (This is bits 6:5)
                ConfigRegisterSettings["Resolution"] = "0"; // 0 = 13 bit resolution, 1 = 16 bit resolution (This is bit 7)

                return ConfigRegisterSettings;
            }

            /// <summary>
            /// Send the pattern requested by nameInMemory.
            /// If requesting the PID pattern, generate the signal and store the result for later processing by InterpretPID
            /// If requesting the TempSense pattern, generate the signal and store the result for later processing by InterpretTempSense
            /// </summary>
            /// <param name="nameInMemory">The requested pattern to generate</param>
            /// <returns>True if the pattern generated without bit errors</returns>
            public override bool SendVector(string nameInMemory)
            {
                if (!usingMIPI || nameInMemory == null || nameInMemory == "") return true;
                bool vioSpecific = false;

                if (Enum.TryParse(nameInMemory.ToUpper(), out PPMUVioOverrideString vioEnum))
                {
                    vioSpecific = true;
                }
                else
                {
                    nameInMemory = nameInMemory.Replace("_", "");

                    if (!loadedPatternNames.Contains(nameInMemory.ToLower())) // kh for Merlin debug
                    {
                        MessageBox.Show("vector " + nameInMemory + "Not loaded. Check TCF Conditions or HSDIO init", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }

                try
                {

                    if (!isVioTxPpmu) //Pinot added (pcon)
                    {
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    else
                    {
                        if (vioSpecific)
                        {
                            string pin = "";

                            if (nameInMemory.Contains("TX")) pin = "Vio1";
                            else if (nameInMemory.Contains("RX")) pin = "Vio2";

                            bool isDCUnit;
                            double SettingComplianceHi, SettingComplianceLo;

                            if (string.IsNullOrWhiteSpace(pin))
                            {
                                foreach (var s in new string[] { "Vio1", "Vio2" })
                                {
                                    pin = s;
                                    isDCUnit = Eq.Site[Site].DC.ContainsKey(pin) && Eq.Site[Site].DC[pin] is EqDC.iEqDC;
                                    SettingComplianceHi = 0.032;
                                    SettingComplianceLo = 0.0001;

                                    switch (vioEnum)
                                    {
                                        case PPMUVioOverrideString.RESET:                                                    
                                            Eq.Site[Site].DC[pin]?.ForceVoltage(m_DeviceMipiLevel.vil, SettingComplianceLo);
                                            if (isDCUnit) uTimer.wait(1);
                                            Eq.Site[Site].DC[pin]?.ForceVoltage(Reconfigurable_DeviceMipiLevel.vih, SettingComplianceHi);
                                            break;

                                        case PPMUVioOverrideString.VIOOFF:
                                            Eq.Site[Site].DC[pin]?.ForceVoltage(m_DeviceMipiLevel.vil, SettingComplianceLo);
                                            if (isDCUnit) uTimer.wait(1);
                                            break;

                                        case PPMUVioOverrideString.VIOON:
                                            Eq.Site[Site].DC[pin]?.ForceVoltage(Reconfigurable_DeviceMipiLevel.vih, SettingComplianceHi);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                isDCUnit = Eq.Site[Site].DC.ContainsKey(pin) && Eq.Site[Site].DC[pin] is EqDC.iEqDC;
                                SettingComplianceHi = 0.032;
                                SettingComplianceLo = 0.0001;

                                switch (vioEnum)
                                {
                                    case PPMUVioOverrideString.RESET_TX:
                                    case PPMUVioOverrideString.RESET_RX:
                                        Eq.Site[Site].DC[pin]?.ForceVoltage(m_DeviceMipiLevel.vil, SettingComplianceLo);
                                        if (isDCUnit) uTimer.wait(1);
                                        Eq.Site[Site].DC[pin]?.ForceVoltage(Reconfigurable_DeviceMipiLevel.vih, SettingComplianceHi);
                                        break;

                                    case PPMUVioOverrideString.VIOOFF_TX:
                                    case PPMUVioOverrideString.VIOOFF_RX:
                                        Eq.Site[Site].DC[pin]?.ForceVoltage(m_DeviceMipiLevel.vil, SettingComplianceLo);
                                        if (isDCUnit) uTimer.wait(1);
                                        break;

                                    case PPMUVioOverrideString.VIOON_TX:
                                    case PPMUVioOverrideString.VIOON_RX:
                                        Eq.Site[Site].DC[pin]?.ForceVoltage(Reconfigurable_DeviceMipiLevel.vih, SettingComplianceHi);
                                        break;
                                }
                            }

                            return true;
                        }
                        allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                        allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }

                    TEMPSENSEVccPin.DigitalLevels.TerminationMode = TerminationMode.ActiveLoad;

                    // Select pattern to burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = nameInMemory.ToLower();

                    // Send the normal pattern file and store the number of bit errors from the SDATA pin
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));
                    //  Thread.Sleep(500);

                    // Get PassFail Results
                    bool[] passFail = DIGI.PatternControl.GetSitePassFail("");

                    Int64[] failureCount;
                    string CurrentSlaveAddress;
                    if (dutSlaveAddress.Length % 2 == 1)
                        CurrentSlaveAddress = dutSlaveAddress.PadLeft(dutSlaveAddress.Length + 1, '0');
                    else
                        CurrentSlaveAddress = dutSlaveAddress;

                    int Current_MIPI_Bus = dutSlavePairIndex;// Convert.ToUInt16(Get_Digital_Definition("SLAVE_ADDR_" + CurrentSlaveAddress));




                    if (dutSlavePairIndex == 2) //if (Current_MIPI_Bus == 2)
                        failureCount = sdata2Pin.GetFailCount();
                    else
                        failureCount = sdata1Pin.GetFailCount();

                    //  NumBitErrors = (int)failureCount[EqHSDIO.Num_Mipi_Bus - Current_MIPI_Bus];
                    NumBitErrors = (int)failureCount[0];


                    if (debug) Console.WriteLine("SendVector " + nameInMemory + " Bit Errors: " + NumBitErrors.ToString());

                    return passFail[0];

                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }



            //public int SendVector_Return_Error_Count(string nameInMemory)
            //{
            //    if (!usingMIPI) return 0;

            //    try
            //    {
            //        nameInMemory = nameInMemory.Replace("_", "");

            //        // This is not a special case such as PID or TempSense.
            //        allRffePins.SelectedFunction = SelectedFunction.Digital;
            //        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

            //        // Select pattern to burst
            //        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
            //        DIGI.PatternControl.StartLabel = nameInMemory.ToLower();


            //        // DIGI.CaptureWaveforms.CreateSerial(sdataPin, nameInMemory, 1, BitOrder.MostSignificantBitFirst);
            //        //  DIGI.CaptureWaveforms.CreateSerial(sdataPin_RX, nameInMemory, 1, BitOrder.MostSignificantBitFirst);

            //        // Send the normal pattern file and store the number of bit errors from the SDATA pin
            //        DIGI.PatternControl.Initiate();

            //        // Wait for Pattern Burst to complete
            //        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 1000));

            //        DIGI.PatternControl.GetSitePassFail("");
            //        // Get PassFail Results
            //        //bool[] passFail = DIGI.PatternControl.GetSitePassFail("", new TimeSpan(0, 0, 0, 0, 100));
            //        //Int64[] failureCount = sdataPin.GetFailCount(new TimeSpan(0, 0, 10));
            //        //Int64[] failureCount2 = sdataPin_RX.GetFailCount(new TimeSpan(0, 0, 10));


            //        bool[] passFail = DIGI.PatternControl.GetSitePassFail("");
            //        Int64[] failureCount = sdataPin.GetFailCount();

            //        // uint[][] test = new uint[1][];
            //        //  DIGI.CaptureWaveforms.Fetch("0", nameInMemory, -1, TimeSpan.FromSeconds(3), ref test);


            //        int Num_OTP_Bit_Fails = (int)failureCount[0];
            //        if (debug) Console.WriteLine("SendVector " + nameInMemory + " Bit Errors: " + NumExecErrors.ToString());

            //        return Num_OTP_Bit_Fails;

            //    }
            //    catch (Exception e)
            //    {
            //        DIGI.PatternControl.Abort();
            //        MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return 0;
            //    }
            //}

            /// <summary>
            /// Loop through each pattern name in the MipiWaveformNames list and execute them
            /// </summary>
            /// <param name="firstTest">Unused</param>
            /// <param name="MipiWaveformNames">The list of pattern names to execute</param>
            public override void SendNextVectors(bool firstTest, List<string> MipiWaveformNames)
            {
                try
                {
                    if (MipiWaveformNames == null) return;

                    foreach (string nameInMemory in MipiWaveformNames)
                    {
                        SendVector(nameInMemory);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            public override void SendMipiCommands(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands, eMipiTestType _eMipiTestType = eMipiTestType.Write)
            {
                try
                {
                    if (MipiCommands == null) return;

                    switch (_eMipiTestType)
                    {
                        case eMipiTestType.Read:
                            break;
                        case eMipiTestType.WriteRead:
                            break;
                        case eMipiTestType.Write:
                            if (isShareBus) RegWriteMultiplePair(MipiCommands);
                            else RegWriteMultiple(MipiCommands);
                            break;
                        case eMipiTestType.Timing:
                            if (isShareBus) TimingRegWriteMultiplePair(MipiCommands);
                            else TimingRegWriteMultiple(MipiCommands);//, BeforeDelay, AfterDelay  );
                            break;
                        case eMipiTestType.OTPburn:
                            if (dutSlavePairIndex == 1)
                                OTPburnTsmcTx(MipiCommands);
                            else
                                OTPburn(MipiCommands);
                            break;
                        case eMipiTestType.RfBurst:
                            if (isShareBus) BurstRegWriteMultiplePair(MipiCommands);
                            break;
                    }
                    //PAtesting.Stop();
                    //long newtime = PAtesting.ElapsedMilliseconds;
                    //PAtesting.Reset();


                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "HSDIO MIPI Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            //     private void SendRFOnOffTestVector(List<string> namesInMemory)
            //     {
            //         bool flag = false;
            //         uint[] array = new uint[0];
            //         uint[] array2 = new uint[0];
            //         uint[] array3 = new uint[0];
            //         foreach (string current in namesInMemory)
            //         {
            //             if (current.Contains("RFONOFFTIMESW"))
            //             {
            //                 flag = true;
            //                 if (current.Contains("."))
            //                 {
            //                     string[] array4 = current.Split(new char[]
            //{
            //	'.'
            //});
            //                     array4[0] = array4[0].Replace("RFONOFFTIMESW", "");
            //                     string a;
            //                     if ((a = array4[0]) == null)
            //                     {
            //                         goto IL_125;
            //                     }
            //                     if (!(a == "GSMTX"))
            //                     {
            //                         if (!(a == "DCSRX"))
            //                         {
            //                             if (!(a == "TRX3"))
            //                             {
            //                                 if (!(a == "TRX2"))
            //                                 {
            //                                     if (!(a == "DRX"))
            //                                     {
            //                                         goto IL_125;
            //                                     }
            //                                     array = this.SWREG08;
            //                                     array2 = this.SWREG80;
            //                                 }
            //                                 else
            //                                 {
            //                                     array = this.SWREG05;
            //                                     array2 = this.SWREG50;
            //                                 }
            //                             }
            //                             else
            //                             {
            //                                 array = this.SWREG06;
            //                                 array2 = this.SWREG60;
            //                             }
            //                         }
            //                         else
            //                         {
            //                             array = this.SWREG09;
            //                             array2 = this.SWREG90;
            //                         }
            //                     }
            //                     else
            //                     {
            //                         array = this.SWREG01;
            //                         array2 = this.SWREG10;
            //                     }
            //                 IL_130:
            //                     if (array4[1].Contains("ANT2"))
            //                     {
            //                         array3 = array;
            //                         array = array2;
            //                         array2 = array3;
            //                         continue;
            //                     }
            //                     continue;
            //                 IL_125:
            //                     MessageBox.Show("no Exist Band in the SW Reg list");
            //                     goto IL_130;
            //                 }
            //                 if (current.Contains("B2") || current.Contains("B4") || current.Contains("B30"))
            //                 {
            //                     array = this.SWREG03;
            //                     array2 = this.SWREG30;
            //                 }
            //                 else
            //                 {
            //                     array = this.SWREG02;
            //                     array2 = this.SWREG20;
            //                 }
            //                 foreach (string current2 in namesInMemory)
            //                 {
            //                     if (current2.Contains("ANT2"))
            //                     {
            //                         array3 = array;
            //                         array = array2;
            //                         array2 = array3;
            //                         break;
            //                     }
            //                 }
            //             }
            //         }
            //         try
            //         {
            //             this.allRffePins.SelectedFunction = SelectedFunction.Digital;
            //             this.allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
            //             if (flag)
            //             {
            //                 uint[] array5 = new uint[512];
            //                 Array.Copy(array2, 0, array5, 0, array2.Length);
            //                 Array.Copy(array, 0, array5, array2.Length, array.Length);
            //                 Array.Copy(array2, 0, array5, array2.Length + array.Length, array2.Length);
            //                 DIGI.SourceWaveforms.CreateSerial("Sdata".ToUpper(), "SrcRFONOFFSwitch", SourceDataMapping.Broadcast, 1u, BitOrder.MostSignificantBitFirst);
            //                 DIGI.SourceWaveforms.WriteBroadcast("SrcRFONOFFSwitch", array5);
            //                 //DIGI.PatternControl.SetBurstSitesEnabled("site0", true);
            //                 DIGI.PatternControl.StartLabel = "RFOnOffSwitchTest";
            //             }
            //             else
            //             {
            //                 //DIGI.PatternControl.SetBurstSitesEnabled("site0", true);
            //                 DIGI.PatternControl.StartLabel = "RFOnOffTest";
            //             }
            //             DIGI.PatternControl.Initiate();
            //         }
            //         catch (Exception ex)
            //         {
            //             DIGI.PatternControl.Abort();
            //             MessageBox.Show("Failed to send RFONOFFTest Vector.\n\n" + ex.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            //         }
            //     }

            //keng shan ADDED
            public override bool SendRFOnOffTestVector(bool RxMode, string[] SwTimeCustomArry)
            {
                if (!usingMIPI) return true;

                try
                {
                    DIGI.PatternControl.WriteSequencerFlag("seqflag3", false);
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent3", "PXI_Trig3");
                    DIGI.PatternControl.Commit();

                    //Lib_Var.TestCondition.preOnOffOperation = false;
                    RxMode = false;
                    bool isRFONOFFSWITCHTest = false;
                    uint[] SWregON = new uint[] { };
                    uint[] SWregOFF = new uint[] { };
                    string PinName = "";
                    //string SWonString = "";
                    //string SWoffString = "";

                    string[] temp = null;
                    string[] tempSwTimeCustomArry = new string[4];
                    int k = 0;

                    string SrcRFONOFFSwitch = "SrcRFONOFFSwitch";
                    string RFOnOffSwitchTest = "RFOnOffSwitchTest";
                    string RFOnOffTest = "RFOnOffTest";

                    PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();

                    #region Sorting RF onoff vectors for SWregON and SWregOFF

                    if (SwTimeCustomArry[0] != "")
                    {
                        switch (SwTimeCustomArry.Length)
                        {
                            case 2:
                                isRFONOFFSWITCHTest = true;
                                break;
                            case 4:
                                isRFONOFFSWITCHTest = true;
                                SrcRFONOFFSwitch = "SrcRFONOFFSwitch2";
                                RFOnOffSwitchTest = "RFOnOffSwitchTest2";
                                break;
                            default:
                                throw new Exception("The SWCustomArray definition is not correct. Total of ':' is " + tempSwTimeCustomArry.Length);

                        }
                    }
                    #endregion

                    #region Check SWTIMECUSTOM for multiple MIPI configuration
                    int iCnt = 0;
                    int loop = 0;
                    foreach (char a in SwTimeCustomArry[0])
                    {
                        //Has to Detect either TX or RX Mipi base on Slave address
                        //"E" for TX,  "C" for RX. Cannot determine from "TXRX" column in tcf because
                        //certain switching test has switch matrix switching base on RX but testing
                        //TX mipi switches
                        if (loop == 0) { RxMode = (a == 'E') ? false : true; }

                        if (a == ',')
                        {
                            iCnt++;
                        }

                        loop++;
                    }
                    #endregion

                    if (iCnt > 0)
                    {
                        #region Pre-MIPI Configuration
                        switch (iCnt)
                        {
                            case 1:
                                #region ONE PreMIPI
                                //    // Reassigned "swTimeCustomArry" for testcases that requires additional mipi command 
                                //    // prior the MIPI to switch RF switches ON and OFF. (Currently used in PC3 MBOut only)
                                if (SwTimeCustomArry.Length == 2)
                                {
                                    temp = null;
                                    tempSwTimeCustomArry = new string[4];
                                    k = 0;

                                    //preOnOffOperation = true;
                                    RFOnOffSwitchTest = "RFOnOffSwitchTestPreMipi";
                                    SrcRFONOFFSwitch = "SrcRFONOFFSwitchPreMipi";

                                    for (int i = 0; i < SwTimeCustomArry.Length; i++)
                                    {
                                        temp = SwTimeCustomArry[i].Split(',');

                                        for (int j = 0; j < temp.Length; j++)
                                        {
                                            tempSwTimeCustomArry[k] = temp[j];
                                            k++;
                                        }
                                    }
                                    Array.Resize(ref SwTimeCustomArry, tempSwTimeCustomArry.Length);
                                    SwTimeCustomArry = tempSwTimeCustomArry;
                                }
                                else if (SwTimeCustomArry.Length == 4)
                                {
                                    temp = null;
                                    tempSwTimeCustomArry = new string[8];
                                    k = 0;

                                    //preOnOffOperation = true;
                                    RFOnOffSwitchTest = "RFOnOffSwitchTest2PreMipi";
                                    SrcRFONOFFSwitch = "SrcRFONOFFSwitch2PreMipi";

                                    for (int i = 0; i < SwTimeCustomArry.Length; i++)
                                    {
                                        temp = SwTimeCustomArry[i].Split(',');

                                        for (int j = 0; j < temp.Length; j++)
                                        {
                                            tempSwTimeCustomArry[k] = temp[j];
                                            k++;
                                        }
                                    }
                                    Array.Resize(ref SwTimeCustomArry, tempSwTimeCustomArry.Length);
                                    SwTimeCustomArry = tempSwTimeCustomArry;
                                }

                                #endregion

                                break;

                            case 3:

                                #region 3Tx PreMIPI conditions

                                temp = null;
                                tempSwTimeCustomArry = new string[8];
                                k = 0;

                                //preOnOffOperation = true;
                                RFOnOffSwitchTest = "RFOnOffSwitchTest3TxPreMipi";
                                SrcRFONOFFSwitch = "SrcRFONOFFSwitch3TxPreMipi";

                                for (int i = 0; i < SwTimeCustomArry.Length; i++)
                                {
                                    temp = SwTimeCustomArry[i].Split(',');

                                    for (int j = 0; j < temp.Length; j++)
                                    {
                                        tempSwTimeCustomArry[k] = temp[j];
                                        k++;
                                    }
                                }
                                Array.Resize(ref SwTimeCustomArry, tempSwTimeCustomArry.Length);
                                SwTimeCustomArry = tempSwTimeCustomArry;

                                #endregion
                                break;

                            case 4:
                                if (SwTimeCustomArry.Length == 2)   //Not loading on the Vector yet!!
                                {
                                    #region 1Tx & 2Rx PreMIPI condtions (2 Steps)

                                    temp = null;
                                    tempSwTimeCustomArry = new string[10];
                                    k = 0;

                                    //preOnOffOperation = true;
                                    RFOnOffSwitchTest = "RFOnOffSwitchTest1Tx2RxPreMipi";
                                    SrcRFONOFFSwitch = "SrcRFONOFFSwitch1Tx2RxPreMipi";

                                    for (int i = 0; i < SwTimeCustomArry.Length; i++)
                                    {
                                        temp = SwTimeCustomArry[i].Split(',');

                                        for (int j = 0; j < temp.Length; j++)
                                        {
                                            tempSwTimeCustomArry[k] = temp[j];
                                            k++;
                                        }
                                    }
                                    Array.Resize(ref SwTimeCustomArry, tempSwTimeCustomArry.Length);
                                    SwTimeCustomArry = tempSwTimeCustomArry;

                                    #endregion
                                }
                                else if (SwTimeCustomArry.Length == 4)
                                {
                                    #region 1Tx & 2Rx PreMIPI condtions (4 Steps)
                                    temp = null;
                                    tempSwTimeCustomArry = new string[20];
                                    k = 0;

                                    //preOnOffOperation = true;
                                    RFOnOffSwitchTest = "RFOnOffSwitchTest21Tx2RxPreMipi";
                                    SrcRFONOFFSwitch = "SrcRFONOFFSwitch21Tx2RxPreMipi";

                                    for (int i = 0; i < SwTimeCustomArry.Length; i++)
                                    {
                                        temp = SwTimeCustomArry[i].Split(',');

                                        for (int j = 0; j < temp.Length; j++)
                                        {
                                            tempSwTimeCustomArry[k] = temp[j];
                                            k++;
                                        }
                                    }
                                    Array.Resize(ref SwTimeCustomArry, tempSwTimeCustomArry.Length);
                                    SwTimeCustomArry = tempSwTimeCustomArry;
                                    #endregion
                                }
                                else
                                {
                                    throw new Exception("The SWCustomArray definition is not correct. Total of ':' is " + tempSwTimeCustomArry.Length);
                                }

                                break;

                            default:
                                throw new Exception("The PreMipi Configuration is undefined. Total of ',' is " + iCnt);

                        }
                        #endregion

                        if (RxMode)
                        {
                            PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                            SrcRFONOFFSwitch = SrcRFONOFFSwitch + "Rx";
                            RFOnOffSwitchTest = RFOnOffSwitchTest + "Rx";
                            RFOnOffTest = RFOnOffTest + "Rx";
                        }
                    }
                    else
                    {
                        if (RxMode)
                        {
                            PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                            SrcRFONOFFSwitch = SrcRFONOFFSwitch + "Rx";
                            RFOnOffSwitchTest = RFOnOffSwitchTest + "Rx";
                            RFOnOffTest = RFOnOffTest + "Rx";
                        }
                    }

                    #region Burst pattern

                    try
                    {
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        //  & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        if (isRFONOFFSWITCHTest)
                        {
                            // Set vectors array to source waveform data
                            uint[] dataArray = new uint[512];
                            int totalLenth = 0;

                            for (int i = 0; i < SwTimeCustomArry.Length; i++)
                            {
                                Array.Copy(SourceWaveform[SwTimeCustomArry[i]], 0, dataArray, totalLenth, SourceWaveform[SwTimeCustomArry[i]].Length);
                                totalLenth += SourceWaveform[SwTimeCustomArry[i]].Length;
                            }

                            // Configure to source data, register address is up to 8 bits
                            DIGI.SourceWaveforms.CreateSerial(PinName, SrcRFONOFFSwitch, SourceDataMapping.Broadcast, 1, BitOrder.MostSignificantBitFirst);
                            DIGI.SourceWaveforms.WriteBroadcast(SrcRFONOFFSwitch, dataArray);

                            // Burst Pattern RFONOFFSwitchTest
                            DIGI.PatternControl.ConfigurePatternBurstSites("site0");

                            DIGI.PatternControl.StartLabel = RFOnOffSwitchTest;
                        }
                        else
                        {
                            // Burst Pattern RFONOFFTest
                            DIGI.PatternControl.ConfigurePatternBurstSites("site0");

                            DIGI.PatternControl.StartLabel = RFOnOffTest;
                        }

                        // Do not need to call "WaitUntilDone()" as it will block the software triggerring(SeqFlag3)
                        DIGI.PatternControl.Initiate();

                        return true;
                    }
                    catch (Exception e)
                    {
                        DIGI.PatternControl.Abort();
                        MessageBox.Show("Failed to send RFONOFFTest Vector.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    #endregion

                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to generate vector for " + SwTimeCustomArry[0] + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DIGI.PatternControl.Abort();
                    //cm wong: moved up: MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            //keng shan ADDED
            private bool AddtoDic_SourceWaveformArry(string ArryName, string _UsId, string _RegAdd, string _RegData) // only support Register write, not Ext.
            {
                bool isAddsuccese = true;
                string ArryString = null;
                char[] arr;
                string USID = null;
                string RegAdd = null;
                string RegData = null;
                string Command_Parity_bit = null;
                string Data_Parity_bit = null;

                int Command_Parity_Count = 1;
                int Data_Parity_Count = 0;
                string RegWrite = "0;1;0;";
                string BusPark = "0";

                USID = Convert.ToString(Convert.ToInt32(_UsId, 16), 2).PadLeft(4, '0'); arr = USID.ToCharArray(); USID = ""; foreach (char Value in arr) { USID += Convert.ToString(Value); USID += ";"; if (Value == '1') Command_Parity_Count++; }
                RegAdd = Convert.ToString(Convert.ToInt32(_RegAdd, 16), 2).PadLeft(5, '0'); arr = RegAdd.ToCharArray(); RegAdd = ""; foreach (char Value in arr) { RegAdd += Convert.ToString(Value); RegAdd += ";"; if (Value == '1') Command_Parity_Count++; }
                RegData = Convert.ToString(Convert.ToInt32(_RegData, 16), 2).PadLeft(8, '0'); arr = RegData.ToCharArray(); RegData = ""; foreach (char Value in arr) { RegData += Convert.ToString(Value); RegData += ";"; if (Value == '1') Data_Parity_Count++; }

                Command_Parity_bit = (Command_Parity_Count % 2) == 0 ? "1;" : "0;";
                Data_Parity_bit = (Data_Parity_Count % 2) == 0 ? "1;" : "0;";

                ArryString = USID + RegWrite + RegAdd + Command_Parity_bit + RegData + Data_Parity_bit + BusPark;
                uint[] uintArry = ArryString.Split(';').Select(uint.Parse).ToArray();

                if (uintArry.Length == 23)
                    SourceWaveform[ArryName] = uintArry;
                else
                    isAddsuccese = false;

                return isAddsuccese;
            }
            //kengs shan Added
            public override void SetSourceWaveformArry(string customMIPIlist)
            {
                try
                {
                    //string strCustomMIPI;
                    string strUSID = "", strRegAdd, strData;
                    bool isAddsucess = true;

                    //List<string> listCustomString = new List<string>();
                    List<string> listCustomDict = new List<string>();

                    //for (int i = 0; i < customMIPIlist.Count; i++)
                    //{
                    //    customMIPIlist[i].TryGetValue("SWTIMECUSTOM", out strCustomMIPI);
                    //    if (strCustomMIPI != "") listCustomString.Add(strCustomMIPI);
                    //}

                    //for (int j = 0; j < listCustomString.Count; j++)
                    //{
                    string[] strArrList = customMIPIlist.Split(':');

                    for (int k = 0; k < strArrList.Length; k++)
                    {
                        string[] strArrListSplit = strArrList[k].Split(',');

                        for (int m = 0; m < strArrListSplit.Length; m++)
                        {
                            if (!listCustomDict.Contains(strArrListSplit[m]))
                            {
                                var charCustom = strArrListSplit[m].ToCharArray();



                                //if (charCustom[0] == 'T')
                                //    strUSID = "E";
                                //else if (charCustom[0] == 'R')
                                //    strUSID = "C";
                                strUSID = charCustom[0].ToString(); //slave address setting

                                strRegAdd = "0" + charCustom[1].ToString();
                                strData = charCustom[2].ToString() + charCustom[3].ToString();

                                if (isAddsucess)
                                {
                                    isAddsucess = AddtoDic_SourceWaveformArry(strArrListSplit[m], strUSID, strRegAdd, strData);
                                    if (isAddsucess) listCustomDict.Add(strArrListSplit[m]);
                                }
                            }
                        }
                    }
                    //}

                    if (!isAddsucess) MessageBox.Show("Please check your Usid, Reg Add Or Data", "SorceWaveform Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("NI6570: SetSourceWaveformArry error: " + ex.Message);
                }

                /*
                //bool isAddsucess = true;
               
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T000", "E", "00", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T00D", "E", "00", "0D");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T01D", "E", "00", "1D");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T01D", "E", "00", "3D");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T055", "E", "00", "55");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T009", "E", "00", "09");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T1B4", "E", "01", "B4");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T100", "E", "01", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T1C0", "E", "01", "C0");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T204", "E", "02", "04");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T202", "E", "02", "02");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T20D", "E", "02", "0D");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T207", "E", "02", "07");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T206", "E", "02", "06");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T2D0", "E", "02", "D0");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T240", "E", "02", "40");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T300", "E", "03", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T308", "E", "03", "08");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T320", "E", "03", "20");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T3A0", "E", "03", "A0");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T380", "E", "03", "80");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T400", "E", "04", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T401", "E", "04", "01");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("T402", "E", "04", "02");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R100", "C", "01", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R101", "C", "01", "01");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R104", "C", "01", "04");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R106", "C", "01", "06");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R107", "C", "01", "07");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R200", "C", "02", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R201", "C", "02", "01");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R300", "C", "03", "00");
                                
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R86F", "C", "08", "6F");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R85A", "C", "08", "5A");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R84C", "C", "08", "4C");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R84E", "C", "08", "4E");   
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R800", "C", "08", "00");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R847", "C", "08", "47");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("R848", "C", "08", "48");
                
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("RC00", "C", "0C", "00");

                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("B1G0", "C", "08", "47");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("B1G1", "C", "08", "4E");    //Steven: 4C
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("B1G3", "C", "08", "5A");
                if (isAddsucess) isAddsucess = AddtoDic_SourceWaveformArry("B1G5", "C", "08", "6F");
                
                if (!isAddsucess) MessageBox.Show("Please check your Usid, Reg Add Or Data", "SorceWaveform Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            */
            }

            /// <summary>
            /// Not currently supported.  Suport will be added in future driver release.
            /// </summary>
            /// <param name="namesInMemory"></param>
            /// <param name="finalizeScript"></param>
            public override void AddVectorsToScript(List<string> namesInMemory, bool finalizeScript)
            {
            }

            /// <summary>
            /// Returns the number of bit errors from the most recently executed pattern.
            /// </summary>
            /// <param name="nameInMemory">Not Used</param>
            /// <returns>Number of bit errors</returns>
            public override int GetNumExecErrors(string nameInMemory)
            {
                //Int64[] failureCount = sdata1Pin.GetFailCount();
                //return (int)failureCount[0];
                return NumBitErrors;
            }

            /// <summary>
            ///// The PID Value was recorded and stored by the SendVector("ReadPID") function call
            ///// This function returns the value that was previously recorded.
            ///// </summary>
            ///// <param name="nameInMemory">"ReadPID", ignored</param>
            ///// <returns>PID Value recorded by SendVector("ReadPID")</returns>
            //public override int InterpretPID(string nameInMemory)
            //{

            //    try
            //    {
            //        // PID is stored in pidval variable when SendVector(ReadPID) is called
            //        //shmoo("FUNCTIONAL");
            //        return (int)pidval;
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return 0;
            //    }
            //}

            ///// <summary>
            ///// This function sends the TempSense pattern that has been automatically converted
            ///// from the standard .vec format to a NI Capture format.
            ///// This allows the NI 6570 to easily capture the tempsense bits.
            ///// </summary>
            ///// <param name="nameInMemory">"ReadTempSense"</param>
            //private void SendTempSenseVector(string nameInMemory)
            //{

            //    try
            //    {
            //        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
            //        allRffePins.SelectedFunction = SelectedFunction.Digital;
            //        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

            //        // Local Variables
            //        bool[] passFail = new bool[] { };
            //        uint numBits = (captureCounters.ContainsKey(nameInMemory.ToUpper()) ? captureCounters[nameInMemory.ToUpper()] : 8);

            //        // Create the capture waveform.
            //        DIGI.CaptureWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "Cap" + nameInMemory.ToLower(), numBits, BitOrder.MostSignificantBitFirst);

            //        // Choose Pattern to Burst (ReadTempSense)
            //        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
            //        DIGI.PatternControl.StartLabel = nameInMemory.ToLower();

            //        // Burst Pattern
            //        DIGI.PatternControl.Initiate();

            //        // Wait for Pattern Burst to complete
            //        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

            //        // Get PassFail Results for site 0
            //        passFail = DIGI.PatternControl.GetSitePassFail("");
            //        Int64[] failureCount = sdata1Pin.GetFailCount();
            //        NumExecErrors = (int)failureCount[0];
            //        if (debug) Console.WriteLine("SendTempSenseVector " + nameInMemory + " Bit Errors: " + NumExecErrors.ToString());

            //        // Retreive captured waveform, sample count is 1 byte of data
            //        uint[][] data = new uint[][] { };
            //        DIGI.CaptureWaveforms.Fetch("", "Cap" + nameInMemory.ToLower(), 1, new TimeSpan(0, 0, 0, 0, 10), ref data);

            //        // Store Raw TempSense value for later processing by InterpertTempSense function.  Remove Parity and Bus Park bits by shifting right by 2.
            //        TempSenseRaw = data[0][0] >> 2;  //*//CHANGE 10-15-2015
            //    }
            //    catch (Exception e)
            //    {
            //        DIGI.PatternControl.Abort();
            //        MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        TempSenseRaw = 0;
            //    }
            //}

            ///// <summary>
            ///// This function sends the ReadPID pattern.  This is the standard .vec pattern.
            ///// The 6570 uses capture memory to get the data from every H/L location in the original .vec
            ///// </summary>
            ///// <param name="nameInMemory"></param>
            ///// <returns></returns>
            //private uint SendPIDVector(string nameInMemory)
            //{
            //    try
            //    {
            //        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
            //        allRffePins.SelectedFunction = SelectedFunction.Digital;
            //        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

            //        // Local Variables
            //        bool[] passFail = new bool[] { };
            //        uint numBits = (captureCounters.ContainsKey(nameInMemory.ToUpper()) ? captureCounters[nameInMemory.ToUpper()] : 8);

            //        // Create the capture waveform.
            //        DIGI.CaptureWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "Cap" + nameInMemory.ToLower(), numBits, BitOrder.MostSignificantBitFirst);

            //        // Choose Pattern to Burst (ReadTempSense)
            //        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
            //        DIGI.PatternControl.StartLabel = nameInMemory.ToLower();

            //        // Burst Pattern
            //        DIGI.PatternControl.Initiate();

            //        // Wait for Pattern Burst to complete
            //        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

            //        // Get PassFail Results for site 0
            //        passFail = DIGI.PatternControl.GetSitePassFail("");
            //        Int64[] failureCount = sdata1Pin.GetFailCount();
            //        NumExecErrors = (int)failureCount[0];
            //        if (debug) Console.WriteLine("SendPIDVector " + nameInMemory + " Bit Errors: " + NumExecErrors.ToString());

            //        // Retreive captured waveform, sample count is 1 byte of data
            //        uint[][] data = new uint[][] { };
            //        DIGI.CaptureWaveforms.Fetch("", "Cap" + nameInMemory.ToLower(), 1, new TimeSpan(0, 0, 0, 0, 100), ref data);

            //        // Return PID Value as read from DUT.  Remove Parity and Bus Park bits by shifting right by 2.
            //        return data[0][0] >> 2;  //*//CHANGE 10-15-2015

            //    }
            //    catch (Exception e)
            //    {
            //        DIGI.PatternControl.Abort();
            //        MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return 0;
            //    }
            //}

            ///// <summary>
            ///// Compute the temp value in C using the raw value stored in TempSenseRaw
            ///// that was recorded when SendVector("ReadTempSense") was called.
            ///// </summary>
            ///// <param name="nameInMemory">"ReadTempSense"</param>
            ///// <returns>Calculated Temp Value in C</returns>
            //public override double InterpretTempSense(string nameInMemory)
            //{
            //    double TempCalc = 255 - TempSenseRaw;
            //    double TempSenseResult = 137 - (0.65 * (TempCalc - 1));

            //    return TempSenseResult;
            //}

            /// <summary>
            /// Dynamic Multiple Register Write function.  This uses NI 6570 source memory to dynamically change
            /// the register address and write values in the pattern.
            /// This supports extended register write.
            /// </summary>
            /// <param name="registerAddress_hex">The register address to write (hex)</param>
            /// <param name="data_hex">The data to write into the specified register in Hex.  Note:  Maximum # of bytes to write is 16.</param>
            /// <param name="sendTrigger">If True, this function will send either a PXI Backplane Trigger, a Digital Pin Trigger, or both based on the configuration in the NI6570 constructor.</param>
            public override void RegWriteMultiple(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {
                string data_hex = "";
                string registerAddress_hex = "";
                int numOfWrites = 0;
                // Source buffer must contain 512 elements, even if sourcing less
                uint[] dataArray = new uint[512];
                int dataArrayIndex = 0;

                try
                {
                    if ((MipiCommands == null) || (MipiCommands.Count == 0)) return;


                    if (!isVioTxPpmu)
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    else
                    {
                        allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                        allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", "");

                    // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                    DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                    DIGI.PatternControl.Commit();

                    // Build source data from ClsMIPIFrame

                    bool EnableMaskedWrite = IsMaskedWrite(MipiCommands);

                    dataArray = GeneraterwaveformArry(ref numOfWrites, MipiCommands, EnableMaskedWrite);

                    // Configure 6570 to source data calculated above
                    //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "SrcMultipleExtendedRegisterWrite" + numOfWrites.ToString() , SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    //DIGI.SourceWaveforms.WriteBroadcast("SrcMultipleExtendedRegisterWrite" + numOfWrites.ToString() , dataArray);
                    DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "SrcMultipleExtendedRegisterWritewithreg", SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("SrcMultipleExtendedRegisterWritewithreg", dataArray);



                    DIGI.PatternControl.WriteSequencerRegister("reg0", numOfWrites);

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = "MultipleExtendedRegisterWritewithreg";

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));


                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Write Multiple MIPI Registers: MultipleExtendedRegisterWritewithreg.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            public void RegWriteMultiplePair(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {
                string PreviousSlaveAddress = "";

                for (int PairIndex = 1; PairIndex < 3; PairIndex++)
                {
                    string data_hex = "";
                    string registerAddress_hex = "";
                    int numOfWrites = 0;
                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];
                    int dataArrayIndex = 0;

                    //if(PairIndex == 1)
                    //{

                    //    EqHSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI1_SLAVE_ADDR"];
                    //    EqHSDIO.dutSlavePairIndex = 1;
                    //}
                    //else
                    //{
                    //    EqHSDIO.dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI2_SLAVE_ADDR"];
                    //    EqHSDIO.dutSlavePairIndex = 2;
                    //}

                    try
                    {
                        if ((MipiCommands == null) || (MipiCommands.Count == 0)) return;

                        if (!isVioTxPpmu)
                        {
                            // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                            allRffePins.SelectedFunction = SelectedFunction.Digital;
                            allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        }
                        else
                        {
                            allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                            allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                        }
                        // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                        DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", "");

                        // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                        DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                        DIGI.PatternControl.Commit();

                        // Build source data from ClsMIPIFrame
                        bool EnableMaskedWrite = IsMaskedWrite(MipiCommands);

                        dataArray = GeneraterwaveformArry(ref numOfWrites, MipiCommands, EnableMaskedWrite, PairIndex);
                        string PatternName = "MultipleExtendedRegisterWritewithreg";

                        if (EnableMaskedWrite) PatternName = PatternName.Replace("Write", "MaskedWrite");

                        string PinName;
                        string waveformNameinMemory = "";

                        if (PairIndex == 1) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                        else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();


                        if (isShareBus) waveformNameinMemory = PatternName + "Pair" + PairIndex;
                        else waveformNameinMemory = PatternName;


                        // Configure 6570 to source data calculated above
                        //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "SrcMultipleExtendedRegisterWrite" + numOfWrites.ToString() , SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.WriteBroadcast("SrcMultipleExtendedRegisterWrite" + numOfWrites.ToString() , dataArray);
                        //DIGI.SourceWaveforms.CreateSerial(Sdata2ChanName.ToUpper(), "Src" + PatternName, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.CreateSerial(PinName, "Src" + waveformNameinMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.WriteBroadcast("Src" + PatternName, dataArray);
                        DIGI.SourceWaveforms.WriteBroadcast("Src" + waveformNameinMemory, dataArray);



                        DIGI.PatternControl.WriteSequencerRegister("reg0", numOfWrites);

                        // Choose Pattern to Burst
                        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                        DIGI.PatternControl.StartLabel = PatternName + "Pair" + PairIndex;


                        // Burst Pattern
                        DIGI.PatternControl.Initiate();

                        // Wait for Pattern Burst to complete
                        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));



                    }
                    catch (Exception e)
                    {
                        DIGI.PatternControl.Abort();
                        MessageBox.Show("Failed to Write Multiple MIPI Registers: MultipleExtendedRegisterWritewithreg.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            private bool OTPburn(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {
                try
                {
                    //bool isTx = (dutSlaveAddress.ToUpper().Contains(Get_Digital_Definition("MIPI2_SLAVE_ADDR")) ? false : true);
                    bool isTx = (dutSlavePairIndex == 2) ? false : true; //20190311

                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");
                    // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                    DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                    DIGI.PatternControl.Commit();

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };

                    string nameInMemory = (isTx ? "TxOTPBurnTemplate" : "RxOTPBurnTemplate");

                    // Source buffer must contain 512 elements, even if sourcing less

                    string data_hex = "";
                    string registerAddress_hex = "";

                    uint[] dataArray = new uint[512];

                    if (isTx)
                    {
                        #region Tx OTP
                        foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommands)
                        {
                            dutSlaveAddress = command.SlaveAddress_hex;
                            registerAddress_hex = command.Register_hex;
                            data_hex = command.Data_hex;

                            for (int bit = 0; bit < 8; bit++)
                            {
                                int bitVal = (int)Math.Pow(2, bit);

                                if ((bitVal & Convert.ToInt32(data_hex, 16)) == bitVal)
                                {
                                    dataArray = new uint[512];
                                    int dataArrayIndex = 0;

                                    uint OffburnDataDec = Convert.ToUInt32((Convert.ToInt32(registerAddress_hex, 16) << 3) + bit);
                                    uint OnburnDataDec = (1 << 7) + OffburnDataDec;

                                    dataArray[dataArrayIndex] = calculateParity(OffburnDataDec); // data 9 bits
                                    dataArray[dataArrayIndex + 1] = calculateParity(OnburnDataDec); // data 9 bits
                                    dataArray[dataArrayIndex + 2] = calculateParity(OffburnDataDec); // data 9 bits


                                    DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                                    DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                                    // Choose Pattern to Burst
                                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                                    DIGI.PatternControl.StartLabel = nameInMemory;

                                    // Burst Pattern
                                    DIGI.PatternControl.Initiate();

                                    // Wait for Pattern Burst to complete
                                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 100));
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Rx OTP
                        if (Get_Digital_Definition("RX_FABSUPPLIER") == "GF")
                        {
                            #region GF


                            List<MipiSyntaxParser.ClsMIPIFrame> _EfuseProgramMipiCmd = CreatorLNADataArry(MipiCommands);

                            // Build source data from ClsMIPIFrame
                            int dataArrayIndex = 0;
                            int numOfWrites = 0;

                            foreach (MipiSyntaxParser.ClsMIPIFrame command in _EfuseProgramMipiCmd)
                            {
                                dutSlaveAddress = command.SlaveAddress_hex;
                                registerAddress_hex = command.Register_hex;
                                data_hex = command.Data_hex;


                                // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                                if (data_hex.Length % 2 == 1)
                                    data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');

                                // Build extended write command data, setting read byte count and register address. 
                                // Note, write byte count is 0 indexed.
                                uint cmdBytesWithParity;


                                cmdBytesWithParity = generateRFFECommand(Convert.ToString(0, 16), Command.EXTENDEDREGISTERWRITE);
                                // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                                dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                                dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                                dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits

                                // Convert Hex Data string to bytes and add to data Array
                                dataArray[dataArrayIndex + 3] = calculateParity(Convert.ToUInt32(data_hex, 16)); // data 9 bits

                                dataArrayIndex = dataArrayIndex + 4; // set for next command
                                numOfWrites++;

                            }

                            DIGI.SourceWaveforms.CreateSerial(Sdata2ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                            DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                            DIGI.PatternControl.WriteSequencerRegister("reg0", numOfWrites);

                            // Choose Pattern to Burst
                            // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                            DIGI.PatternControl.StartLabel = nameInMemory;

                            // Burst Pattern
                            DIGI.PatternControl.Initiate();

                            // Wait for Pattern Burst to complete
                            DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 100));


                            #endregion
                        }
                        else
                        {
                            #region TSMC
                            for (int Byte = 0; Byte < 8; Byte++)
                            {
                                for (int bit = 0; bit < 8; bit++)
                                {
                                    foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommands)
                                    {
                                        dutSlaveAddress = command.SlaveAddress_hex;
                                        registerAddress_hex = command.Register_hex;
                                        data_hex = command.Data_hex;

                                        int tempReg = Convert.ToInt32(command.Register_hex, 16);
                                        if (tempReg > 7)
                                        {
                                            tempReg = tempReg - 8;
                                        }
                                        int bitVal = (int)Math.Pow(2, bit);
                                        int data_int = Convert.ToInt32(Convert.ToInt32(data_hex, 16));
                                        int Byte_int = tempReg;
                                        Byte_int = Byte_int > 7 ? (Byte_int - 1) / 2 : Byte_int;

                                        if ((Byte == Byte_int) && (bitVal & data_int) == bitVal)
                                        {
                                            dataArray[Byte * 8 + bit] = calculateParity(Convert.ToUInt32("40", 16));
                                            break;
                                        }
                                        else
                                        {
                                            dataArray[Byte * 8 + bit] = calculateParity(Convert.ToUInt32("00", 16));
                                        }
                                    }
                                }
                            }

                            //Last bit Indecater
                            if (dataArray[63] != 1) dataArray[63] = calculateParity(Convert.ToUInt32("4F", 16));
                            else dataArray[63] = calculateParity(Convert.ToUInt32("0F", 16));

                            DIGI.SourceWaveforms.CreateSerial(Sdata2ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                            DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                            // Choose Pattern to Burst
                            // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                            DIGI.PatternControl.StartLabel = nameInMemory;

                            // Burst Pattern
                            DIGI.PatternControl.Initiate();

                            // Wait for Pattern Burst to complete
                            DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 100));
                            #endregion
                        }
                        #endregion
                    }

                    return true;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to RxOTPBurnTemplate. \n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            private bool OTPburnTsmcTx(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {
                bool isTx = (dutSlavePairIndex == 2) ? false : true; //20190311
                try
                {
                    if (!isVioTxPpmu)
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    }
                    else
                    {
                        allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                        allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");
                    // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                    DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                    DIGI.PatternControl.Commit();

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    string nameInMemory = "";

                    nameInMemory = (isTx ? "TxOTPBurnTemplate" : "RxOTPBurnTemplate");

                    // Source buffer must contain 512 elements, even if sourcing less
                    string data_hex = "";
                    string registerAddress_hex = "";
                    uint[] dataArray = new uint[512];

                    #region Common method to send vectors.
                    Action<List<MipiSyntaxParser.ClsMIPIFrame>> _SendGeneratedVectors = new Action<List<MipiSyntaxParser.ClsMIPIFrame>>((cmdList) =>
                    {
                        int dataArrayIndex;
                        dataArrayIndex = 0;

                        foreach (MipiSyntaxParser.ClsMIPIFrame command in cmdList)
                        {

                            dutSlaveAddress = command.SlaveAddress_hex;
                            registerAddress_hex = command.Register_hex;
                            data_hex = command.Data_hex;

                            dataArray[dataArrayIndex] = calculateParity(Convert.ToUInt32(data_hex, 16)); //Convert.ToUInt32(Convert.ToInt32((data_hex) << 1, 16));

                            dataArrayIndex++; // set for next command
                        }
                        DIGI.SourceWaveforms.CreateSerial(isTx ? Sdata1ChanName.ToUpper() : Sdata2ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                        // Choose Pattern to Burst
                        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                        DIGI.PatternControl.StartLabel = nameInMemory;

                        // Burst Pattern
                        DIGI.PatternControl.Initiate();

                        // Wait for Pattern Burst to complete
                        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 100));
                    });
                    #endregion

                    if (isTx)
                    {
                        #region Tx OTP
                        List<MipiSyntaxParser.ClsMIPIFrame> _EfuseProgramMipiCmd;

                        switch (Get_Digital_Definition("TX_FABSUPPLIER"))
                        {
                            #region TSMC IP
                            case "TSMC_130NM":
                                _EfuseProgramMipiCmd = CreatorDataArry(MipiCommands, isTx);
                                _SendGeneratedVectors(_EfuseProgramMipiCmd);
                                break;
                            case "TSMC_65NM":
                                _EfuseProgramMipiCmd = CreatorDataArry(MipiCommands, isTx);
                                _SendGeneratedVectors(_EfuseProgramMipiCmd);
                                break;
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region Rx OTP
                        throw new InvalidOperationException("Not applicable to RX");
                        #endregion
                    }

                    return true;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to load " + (isTx ? "TX" : "RX").ToString() + "_OTPBurnTemplate. \n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            private List<MipiSyntaxParser.ClsMIPIFrame> CreatorDataArry(List<MipiSyntaxParser.ClsMIPIFrame> _mipicmd, bool isTx)
            {
                List<MipiSyntaxParser.ClsMIPIFrame> listmipicmd = new List<EqLib.MipiSyntaxParser.ClsMIPIFrame>();
                string data_hex;
                int indexcmd = 0;

                if (isTx)
                {
                    switch (Eq.Site[Site].HSDIO.Get_Digital_Definition("TX_FABSUPPLIER"))
                    {
                        case "TSMC_130NM":
                            {
                                string RegOTPSigCtrl = Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL");
                                string RegOTPAddrCtrl = Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_ADDR_CTRL");

                                foreach (MipiSyntaxParser.ClsMIPIFrame command in _mipicmd)
                                {
                                    listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex, RegOTPAddrCtrl, command.Data_hex, command.Pair)); //Send 8 bits data to be burned on 0xC4
                                    listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex, RegOTPSigCtrl, "82", command.Pair)); //Send 0xC0 control signal w/ burn enabled 
                                    listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex, RegOTPSigCtrl, "02", command.Pair)); //Send 0xC0 control signal w/ burn disabled
                                }
                                break;
                            }

                        case "TSMC_65NM":
                            {
                                string RegOTPSigCtrl = Eq.Site[Site].HSDIO.Get_Digital_Definition("REG_TX_OTP_SIG_CTRL_CMOS65NM");
                                //int Byte = Convert.ToInt32(_mipicmd.First().Register_hex, 16);

                                bool isFirst = true;
                                bool is2ndMemory = false;
                                int bytescntfor2ndmemory = 6;
                                int indLastFuseCell = -1;

                                foreach (MipiSyntaxParser.ClsMIPIFrame command in _mipicmd)
                                {
                                    is2ndMemory = Convert.ToInt32(command.Register_hex, 16) > 7;
                                    int efusecnt = (is2ndMemory ? bytescntfor2ndmemory : 8);
                                    indexcmd = 0;
                                    dutSlaveAddress = command.SlaveAddress_hex;
                                    data_hex = command.Data_hex;

                                    if (isFirst)
                                    {
                                        for (int Byte = 0; Byte < efusecnt; Byte++)
                                        {
                                            for (int bit = 0; bit < 8; bit++)
                                            {
                                                if (is2ndMemory)
                                                {
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "D0", command.Pair));
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "D2", command.Pair));
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "D2", command.Pair));
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "D0", command.Pair));
                                                }
                                                else
                                                {
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "E0", command.Pair));
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "E1", command.Pair));
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "E1", command.Pair));
                                                    listmipicmd.Add(new MipiSyntaxParser.ClsMIPIFrame(dutSlaveAddress, RegOTPSigCtrl, "E0", command.Pair));
                                                }
                                            }
                                        }
                                        isFirst = false;
                                    }

                                    for (int Byte = 0; Byte < efusecnt; Byte++)
                                    {
                                        for (int bit = 0; bit < 8; bit++)
                                        {
                                            int offset = indexcmd * 4;

                                            indexcmd++;
                                            int bitVal = (int)Math.Pow(2, bit);
                                            int data_int = Convert.ToInt32(Convert.ToInt32(data_hex, 16));

                                            if (data_int == 0) break;

                                            int Byte_int = Convert.ToInt32(command.Register_hex, 16);
                                            Byte_int = Byte_int > 7 ? (Byte_int - 8) : Byte_int; //  (Byte_int - 8) for second memory. 8 is 1st memory num of byte

                                            if ((Byte == Byte_int) && (bitVal & data_int) == bitVal)
                                            {
                                                if (Byte > indLastFuseCell) indLastFuseCell = Byte;

                                                if (is2ndMemory)
                                                {
                                                    listmipicmd[offset + 0].Data_hex = "D8";
                                                    listmipicmd[offset + 1].Data_hex = "DA";
                                                    listmipicmd[offset + 2].Data_hex = "D2";
                                                    listmipicmd[offset + 3].Data_hex = "D0";
                                                }
                                                else
                                                {
                                                    listmipicmd[offset + 0].Data_hex = "E4";
                                                    listmipicmd[offset + 1].Data_hex = "E5";
                                                    listmipicmd[offset + 2].Data_hex = "E1";
                                                    listmipicmd[offset + 3].Data_hex = "E0";
                                                }
                                            }
                                        }
                                    }
                                }

                                //listmipicmd.RemoveRange((indLastFuseCell + 1) * 8, (8 - (indLastFuseCell + 1)) * 8 * 4);
                                break;
                            }
                    }
                }
                else
                {
                    listmipicmd = CreatorLNADataArry(_mipicmd);
                }
                return listmipicmd;
            }

            private List<MipiSyntaxParser.ClsMIPIFrame> CreatorLNADataArry(List<MipiSyntaxParser.ClsMIPIFrame> _mipicmd)
            {
                List<MipiSyntaxParser.ClsMIPIFrame> listmipicmd = new List<EqLib.MipiSyntaxParser.ClsMIPIFrame>();
                int indexcmd = 0;

                foreach (MipiSyntaxParser.ClsMIPIFrame command in _mipicmd)
                {
                    indexcmd++;

                    for (int bit = 0; bit < 8; bit++)
                    {
                        int bitVal = (int)Math.Pow(2, bit);

                        if ((bitVal & Convert.ToInt32(command.Data_hex, 16)) == bitVal)
                        {
                            if (listmipicmd.Count == 0)
                            {
                                string _Data_hex;

                                if (Convert.ToInt32(command.Register_hex, 16) < 8)
                                    _Data_hex = "A1";
                                else
                                    _Data_hex = "A2";

                                listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex,
                               "F0",
                               _Data_hex,
                               command.Pair));
                            }

                            int uRegister_hex = (Convert.ToInt32(command.Register_hex, 16) < 8 ?
                                                    Convert.ToInt32(command.Register_hex, 16) :
                                                    Convert.ToInt32(command.Register_hex, 16) - 8);

                            uint OffburnDataDec = Convert.ToUInt32((uRegister_hex << 3) + bit);
                            uint OnburnDataDec = (1 << 7) + OffburnDataDec;

                            listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex,
                            "F2",
                            OnburnDataDec.ToString("X"),
                            command.Pair));
                        }
                    }

                    if (indexcmd == _mipicmd.Count)
                    {
                        listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex,
                      "F2",
                      "00",
                      command.Pair));

                        listmipicmd.Add(new EqLib.MipiSyntaxParser.ClsMIPIFrame(command.SlaveAddress_hex,
                      "F0",
                      "78",
                      command.Pair));
                    }
                }

                return listmipicmd;
            }
            //Seoul
            private void TimingRegWriteMultiple(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)//, int _nBeforeCmd, double _BeforeDelay, double _AfterDelay)
            {
                string data_hex = "";
                string registerAddress_hex = "";
                int numOfWrites = 0;
                // Source buffer must contain 512 elements, even if sourcing less
                uint[] dataArray = new uint[512];
                int dataArrayIndex = 0;

                string PinName;

                if (dutSlavePairIndex == 1) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();


                foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommands)
                {
                    dutSlaveAddress = command.SlaveAddress_hex;
                    registerAddress_hex = command.Register_hex;
                    data_hex = command.Data_hex;


                    // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                    if (data_hex.Length % 2 == 1)
                        data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');



                    // Build extended write command data, setting read byte count and register address. 
                    // Note, write byte count is 0 indexed.
                    uint cmdBytesWithParity = generateRFFECommand(Convert.ToString(0, 16), Command.EXTENDEDREGISTERWRITE);
                    // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                    dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                    dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                    dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits

                    // Convert Hex Data string to bytes and add to data Array
                    dataArray[dataArrayIndex + 3] = calculateParity(Convert.ToUInt32(data_hex, 16)); // data 9 bits

                    dataArrayIndex = dataArrayIndex + 4; // set for next command
                    numOfWrites++;

                }

                try
                {
                    if ((MipiCommands == null) || (MipiCommands.Count == 0)) return;

                    if (!isVioTxPpmu)
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    else
                    {
                        allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                        allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }

                    // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", pxiTrigger.ToString("g"));

                    // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                    DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                    DIGI.PatternControl.Commit();

                    // Configure 6570 to source data calculated above
                    //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "SrcTimingExtendedRegisterWriteReg", SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.CreateSerial(PinName, "SrcTimingExtendedRegisterWriteRegPair" + Convert.ToString(dutSlavePairIndex), SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("SrcTimingExtendedRegisterWriteReg", dataArray);

                    //Vaild values for register inculde  Reg0-15
                    //Numeric is the data 16bits ( 0 - 65535 )

                    int singleMipiCmdFrmbits = 38;
                    int TriggerLength = 102;

                    int _nAftercCmd = MipiCommands.Count - nBeforeCmd - 1;
                    int _nBits_Before = (int)(((BeforeDelay * 1e-6) - (1 / MIPIClockRate) * singleMipiCmdFrmbits) * MIPIClockRate);
                    int _nBits_After = (int)(((AfterDelay * 1e-6) - ((1 / MIPIClockRate) * (TriggerLength + (singleMipiCmdFrmbits * _nAftercCmd)))) * MIPIClockRate);

                    if (_nBits_Before < 0 || _nBits_After < 0 || _nAftercCmd < 0) throw new Exception("Please check Timing Stript \nCan not set to the delay time due to Number of command");

                    DIGI.PatternControl.WriteSequencerRegister("reg0", nBeforeCmd);
                    DIGI.PatternControl.WriteSequencerRegister("reg1", _nBits_Before); // 58 bits for Fixed Vector
                    DIGI.PatternControl.WriteSequencerRegister("reg2", _nBits_After); // 102 bits for Fixed Vector + (56 * n) bits for Command 
                    DIGI.PatternControl.WriteSequencerRegister("reg3", _nAftercCmd); // -1 for Trig Cmd

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = "TimingExtendedRegisterWriteReg";

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));


                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Write Multiple MIPI Registers: TimingExtendedRegisterWriteReg" + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void TimingRegWriteMultiplePair(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)//, int _nBeforeCmd, double _BeforeDelay, double _AfterDelay)
            {

                string PreviousSlaveAddress = "";

                for (int PairIndex = 1; PairIndex < 3; PairIndex++)
                {

                    string data_hex = "";
                    string registerAddress_hex = "";
                    int numOfWrites = 0;
                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];
                    int dataArrayIndex = 0;


                    foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommands)
                    {
                        if (PairIndex != command.Pair) continue;

                        dutSlaveAddress = command.SlaveAddress_hex;
                        registerAddress_hex = command.Register_hex;
                        data_hex = command.Data_hex;


                        // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                        if (data_hex.Length % 2 == 1)
                            data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');



                        // Build extended write command data, setting read byte count and register address. 
                        // Note, write byte count is 0 indexed.
                        uint cmdBytesWithParity = generateRFFECommand(Convert.ToString(0, 16), Command.EXTENDEDREGISTERWRITE);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits

                        // Convert Hex Data string to bytes and add to data Array
                        dataArray[dataArrayIndex + 3] = calculateParity(Convert.ToUInt32(data_hex, 16)); // data 9 bits

                        dataArrayIndex = dataArrayIndex + 4; // set for next command
                        numOfWrites++;

                    }

                    try
                    {
                        if ((MipiCommands == null) || (MipiCommands.Count == 0)) return;
                        if (numOfWrites == 0) continue;

                        if (!isVioTxPpmu)
                        {
                            // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                            allRffePins.SelectedFunction = SelectedFunction.Digital;
                            allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        }
                        else
                        {
                            allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                            allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                        }

                        // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                        DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", pxiTrigger.ToString("g"));

                        // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                        DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                        DIGI.PatternControl.Commit();


                        string PatternName = "TimingExtendedRegisterWriteReg1";

                        //if (EnableMaskedWrite) PatternName = PatternName.Replace("Write", "MaskedWrite");

                        string PinName;
                        string waveformNameinMemory = "";

                        if (PairIndex == 1) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                        else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();


                        if (isShareBus) waveformNameinMemory = PatternName + "Pair" + PairIndex;
                        else waveformNameinMemory = PatternName;



                        // Configure 6570 to source data calculated above
                        //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "SrcTimingExtendedRegisterWriteReg", SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.CreateSerial(PinName, "SrcTimingExtendedRegisterWriteRegPair" + Convert.ToString(dutSlavePairIndex), SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.WriteBroadcast("SrcTimingExtendedRegisterWriteReg", dataArray);

                        DIGI.SourceWaveforms.CreateSerial(PinName, "Src" + waveformNameinMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.WriteBroadcast("Src" + PatternName, dataArray);
                        DIGI.SourceWaveforms.WriteBroadcast("Src" + waveformNameinMemory, dataArray);


                        //Vaild values for register inculde  Reg0-15
                        //Numeric is the data 16bits ( 0 - 65535 )

                        int singleMipiCmdFrmbits = 38;
                        int TriggerLength = 102;

                        int _nAftercCmd = MipiCommands.Count - nBeforeCmd - 1;
                        int _nBits_Before = (int)(((BeforeDelay * 1e-6) - (1 / MIPIClockRate) * singleMipiCmdFrmbits) * MIPIClockRate);
                        int _nBits_After = (int)(((AfterDelay * 1e-6) - ((1 / MIPIClockRate) * (TriggerLength + (singleMipiCmdFrmbits * _nAftercCmd)))) * MIPIClockRate);

                        if (_nBits_Before < 0 || _nBits_After < 0 || _nAftercCmd < 0) throw new Exception("Please check Timing Stript \nCan not set to the delay time due to Number of command");

                        DIGI.PatternControl.WriteSequencerRegister("reg0", nBeforeCmd);
                        DIGI.PatternControl.WriteSequencerRegister("reg1", _nBits_Before); // 58 bits for Fixed Vector
                        DIGI.PatternControl.WriteSequencerRegister("reg2", _nBits_After); // 102 bits for Fixed Vector + (56 * n) bits for Command 
                        DIGI.PatternControl.WriteSequencerRegister("reg3", _nAftercCmd); // -1 for Trig Cmd

                        // Choose Pattern to Burst
                        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                        //DIGI.PatternControl.StartLabel = "TimingExtendedRegisterWriteReg";
                        DIGI.PatternControl.StartLabel = PatternName + "Pair" + PairIndex;








                        // Burst Pattern
                        DIGI.PatternControl.Initiate();

                        // Wait for Pattern Burst to complete
                        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));


                    }
                    catch (Exception e)
                    {
                        DIGI.PatternControl.Abort();
                        MessageBox.Show("Failed to Write Multiple MIPI Registers: TimingExtendedRegisterWriteReg" + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }


            private void BurstRegWriteMultiplePair(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)//, int _nBeforeCmd, double _BeforeDelay, double _AfterDelay)
            {

                string PreviousSlaveAddress = "";

                for (int PairIndex = 1; PairIndex < 3; PairIndex++)
                {

                    string data_hex = "";
                    string registerAddress_hex = "";
                    int numOfWrites = 0;
                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];
                    int dataArrayIndex = 0;


                    foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommands)
                    {
                        if (PairIndex != command.Pair) continue;

                        dutSlaveAddress = command.SlaveAddress_hex;
                        registerAddress_hex = command.Register_hex;
                        data_hex = command.Data_hex;


                        // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                        if (data_hex.Length % 2 == 1)
                            data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');



                        // Build extended write command data, setting read byte count and register address. 
                        // Note, write byte count is 0 indexed.
                        uint cmdBytesWithParity = generateRFFECommand(Convert.ToString(0, 16), Command.EXTENDEDREGISTERWRITE);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits

                        // Convert Hex Data string to bytes and add to data Array
                        dataArray[dataArrayIndex + 3] = calculateParity(Convert.ToUInt32(data_hex, 16)); // data 9 bits

                        dataArrayIndex = dataArrayIndex + 4; // set for next command
                        numOfWrites++;

                    }

                    try
                    {
                        if ((MipiCommands == null) || (MipiCommands.Count == 0)) return;
                        if (numOfWrites == 0) continue;

                        if (!isVioTxPpmu)
                        {
                            // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                            allRffePins.SelectedFunction = SelectedFunction.Digital;
                            allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        }
                        else
                        {
                            allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                            allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                        }


                        DIGI.Trigger.ConditionalJumpTriggers[0].DigitalEdge.Configure("PXI_Trig5", DigitalEdge.Rising);
                        DIGI.Trigger.ConditionalJumpTriggers[1].DigitalEdge.Configure("PXI_Trig4", DigitalEdge.Rising);


                        string PatternName = "BurstExtendedRegisterWriteReg1";

                        string PinName;
                        string waveformNameinMemory = "";

                        if (PairIndex == 1) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                        else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();


                        if (isShareBus) waveformNameinMemory = PatternName + "Pair" + PairIndex;
                        else waveformNameinMemory = PatternName;

                        // Configure 6570 to source data calculated above
                        //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "SrcTimingExtendedRegisterWriteReg", SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.CreateSerial(PinName, "SrcTimingExtendedRegisterWriteRegPair" + Convert.ToString(dutSlavePairIndex), SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.WriteBroadcast("SrcTimingExtendedRegisterWriteReg", dataArray);

                        DIGI.SourceWaveforms.CreateSerial(PinName, "Src" + waveformNameinMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        //DIGI.SourceWaveforms.WriteBroadcast("Src" + PatternName, dataArray);
                        DIGI.SourceWaveforms.WriteBroadcast("Src" + waveformNameinMemory, dataArray);

                        // Choose Pattern to Burst
                        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                        DIGI.PatternControl.StartLabel = PatternName + "Pair" + PairIndex;

                        // Burst Pattern
                        DIGI.PatternControl.Initiate();

                        // Wait for Pattern Burst to complete
                        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    }
                    catch (Exception e)
                    {
                        DIGI.PatternControl.Abort();
                        MessageBox.Show("Failed to Write Multiple MIPI Registers: TimingExtendedRegisterWriteReg" + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            public override bool Burn(string data_hex, bool invertData = false, int efuseDataByteNum = 0, string efuseCtlAddress = "C0")
            {
                Stopwatch myWatch1 = new Stopwatch();

                int data_dec = Convert.ToInt32(data_hex, 16);

                if (data_dec > 255)
                {
                    MessageBox.Show("Error: Cannot burn decimal values greater than 255", "BurnOTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // burn the data

                myWatch1.Restart();
                double[] OTPTiem = new double[8];


                for (int bit = 0; bit < 8; bit++)
                {
                    int bitVal = (int)Math.Pow(2, bit);

                    if ((bitVal & data_dec) == (invertData ? 0 : bitVal))
                    {
                        //for (int programMode = 1; programMode >= 0; programMode--)
                        //{
                        //    int burnDataDec = (programMode << 7) + (efuseDataByteNum << 3) + bit;
                        //    //HSDIO.Instrument.RegWrite(HSDIO.dutSlaveAddress, efuseCtlAddress, burnDataDec.ToString("X"), false, true);
                        //    //HSDIO.Instrument.RegWrite(efuseCtlAddress, burnDataDec.ToString("X"), false);

                        //    HSDIO.Instrument.RegWrite("3",efuseCtlAddress, burnDataDec.ToString("X"));
                        //    OTPTiem[bit] = myWatch1.Elapsed.TotalMilliseconds;
                        //}

                        int burnDataDec = (1 << 7) + (efuseDataByteNum << 3) + bit;
                        SendVectorOTP(burnDataDec.ToString("X"), "00");
                    }
                }

                return true;
            }
            public override bool SendVectorOTP(string TargetData, string CurrentData = "00", bool isEfuseBurn = false)
            {
                try
                {
                    //bool isTx = (dutSlaveAddress.ToUpper().Contains(Get_Digital_Definition("MIPI2_SLAVE_ADDR")) ? false : true);
                    bool isTx = (dutSlavePairIndex == 2) ? false : true; //20190311

                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");
                    // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                    DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);

                    DIGI.PatternControl.Commit();

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };

                    string nameInMemory = (isTx ? "TxOTPBurnTemplate" : "RxOTPBurnTemplate");

                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];

                    if (isTx)
                    {
                        #region DataArry calculate TX
                        //char[] arr;
                        //string Data = "";
                        //string DataFrameParity = "";
                        //int Parity_Count = 0;

                        //string ExData = TargetData;

                        //Data = Convert.ToString(Convert.ToInt32(ExData, 16), 2).PadLeft(8, '0'); arr = Data.ToCharArray(); Data = ""; foreach (char Value in arr) { Data += Convert.ToString(Value); }
                        //DataFrameParity = Convert.ToString(Convert.ToInt32(ExData, 16), 2).PadLeft(8, '0'); foreach (char Parse_Command in DataFrameParity) { if (Parse_Command == '1') Parity_Count++; }

                        //int reWrite = (Convert.ToInt32(TargetData, 16) << 1) + ((Parity_Count % 2) == 0 ? 1 : 0);

                        //for (int bit = 0; bit < 2; bit++)
                        //{
                        //    dataArray[bit] = (bit == 0 ? (uint)(reWrite) :
                        //        (
                        //            (uint)((reWrite - (Parity_Count % 2 == 0 ? 1 : 0)) - (1 << 8))
                        //        )

                        //    );
                        //}

                        int dataArrayIndex = 0;
                        uint OffburnDataDec = Convert.ToUInt32((Convert.ToInt32(TargetData, 16) & 0x7F));//Convert.ToUInt32((Convert.ToInt32(registerAddress_hex, 16) << 3) + bit);
                        uint OnburnDataDec = Convert.ToUInt32(TargetData, 16);// ; ; (1 << 7) + OffburnDataDec;

                        dataArray[dataArrayIndex] = calculateParity(OffburnDataDec); // data 9 bits
                        dataArray[dataArrayIndex + 1] = calculateParity(OnburnDataDec); // data 9 bits
                        dataArray[dataArrayIndex + 2] = calculateParity(OffburnDataDec); // data 9 bits

                        DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        #endregion
                    }
                    else
                    {
                        #region DataArry calculate RX

                        string Efuse = "00";
                        if (isEfuseBurn) Efuse = "80";

                        int reWrite = ((Convert.ToInt32(TargetData, 16) ^ Convert.ToInt32(CurrentData, 16)) & Convert.ToInt32(TargetData, 16));
                        int inEfuse = (Convert.ToInt32(Efuse, 16));//reWrite += Convert.ToInt32(Efuse, 16) << 8;
                        //int inEfuseForBandGab = (Convert.ToInt32("04", 16));//reWrite += Convert.ToInt3

                        bool isLastIndicator = false;

                        for (int Add = 0; Add < 8; Add++)
                        {
                            if (Add == 0)
                            {
                                for (int bit = 0; bit < 8; bit++)
                                {
                                    int bitVal = (int)Math.Pow(2, bit);
                                    if ((bitVal & reWrite) == bitVal) dataArray[Add * 8 + bit] = (uint)(isLastIndicator ? 158 : 128);
                                    else dataArray[Add * 8 + bit] = (uint)(isLastIndicator ? 31 : 1);
                                }
                            }
                            //else if (Add == 3) //BandGap OTP...
                            //{
                            //    for (int bit = 0; bit < 8; bit++)
                            //    {
                            //        int bitVal = (int)Math.Pow(2, bit);
                            //        if ((bitVal & inEfuseForBandGab) == bitVal) dataArray[Add * 8 + bit] = (uint)(isLastIndicator ? 158 : 128);
                            //        else dataArray[Add * 8 + bit] = (uint)(isLastIndicator ? 31 : 1);
                            //    }
                            //}
                            else if (Add == 7)
                            {
                                for (int bit = 0; bit < 8; bit++)
                                {
                                    int bitVal = (int)Math.Pow(2, bit);
                                    if (bit == 7) isLastIndicator = true;
                                    if ((bitVal & inEfuse) == bitVal) dataArray[Add * 8 + bit] = (uint)(isLastIndicator ? 158 : 128);
                                    else dataArray[Add * 8 + bit] = (uint)(isLastIndicator ? 31 : 1);
                                }
                            }
                            else
                                for (int bit = 0; bit < 8; bit++) dataArray[Add * 8 + bit] = 1;
                        }
                        DIGI.SourceWaveforms.CreateSerial(Sdata2ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                        #endregion
                    }


                    // Configure 6570 to source data calculated above

                    DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = nameInMemory;

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 100));

                    //// Get PassFail Results for site 0
                    //Int64[] failureCount = sdataPin.GetFailCount();
                    //NumExecErrors = (int)failureCount[0];
                    //if (debug) Console.WriteLine("RegWrite " + nameInMemory + " Bit Errors: " + NumExecErrors.ToString());
                    return true;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to RxOTPBurnTemplate. \n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            /// <summary>
            /// Dynamic Register Write function.  This uses NI 6570 source memory to dynamically change
            /// the register address and write values in the pattern.
            /// This supports extended and non-extended register write.
            /// </summary>
            /// <param name="registerAddress_hex">The register address to write (hex)</param>
            /// <param name="data_hex">The data to write into the specified register in Hex.  Note:  Maximum # of bytes to write is 16.</param>
            /// <param name="sendTrigger">If True, this function will send either a PXI Backplane Trigger, a Digital Pin Trigger, or both based on the configuration in the NI6570 constructor.</param>
            public override void RegWrite(string registerAddress_hex, string data_hex, bool sendTrigger = false)
            {
                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    if (!isVioTxPpmu)
                    {
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    else
                    {
                        allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                        allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }

                    if (sendTrigger)
                    {
                        triggerConfig = TrigConfig.PXI_Backplane;
                        // Configure the NI 6570 to connect PXI_TrigX to "event0" that can be used with the set_trigger, clear_trigger, and pulse_trigger opcodes
                        if (triggerConfig == TrigConfig.Both || triggerConfig == TrigConfig.PXI_Backplane)
                        {
                            DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", pxiTrigger.ToString("g"));
                        }
                        else
                        {
                            // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                            DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", "");
                        }

                        // Set the Sequencer Flag 0 to indicate that a trigger should be sent on the TrigChan pin
                        if (triggerConfig == TrigConfig.Both || triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.PXI_Backplane)
                        {
                            DIGI.PatternControl.WriteSequencerFlag("seqflag0", true);
                        }
                        else
                        {
                            // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                            DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);
                        }

                        if (triggerConfig == TrigConfig.None)
                        {
                            throw new Exception("sendTrigger=True requested, but NI 6570 is not configured for Triggering.  Please update the NI6570 Constructor triggerConfig to use TrigConfig.Digital_Pin, TrigConfig.PXI_Backplane, or TrigConfig.Both.");
                        }
                    }
                    else
                    {
                        // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                        DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", "");

                        // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                        DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);
                    }

                    DIGI.PatternControl.Commit();

                    // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                    if (data_hex.Length % 2 == 1)
                        data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    bool extendedWrite = Convert.ToInt32(registerAddress_hex, 16) > 31;    // any register address > 5 bits requires extended read
                    uint writeByteCount = extendedWrite ? (uint)(data_hex.Length / 2) : 1;
                    string nameInMemory = extendedWrite ? "ExtendedRegisterWrite" + writeByteCount.ToString() : "RegisterWrite";

                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];
                    if (!extendedWrite)
                    {
                        // Build non-exteded write command
                        uint cmdBytesWithParity = generateRFFECommand(registerAddress_hex, Command.REGISTERWRITE);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[2] = calculateParity(Convert.ToUInt32(data_hex, 16)); // final 9 bits
                    }
                    else
                    {
                        // Build extended read command data, setting read byte count and register address. 
                        // Note, write byte count is 0 indexed.
                        uint cmdBytesWithParity = generateRFFECommand(Convert.ToString(writeByteCount - 1, 16), Command.EXTENDEDREGISTERWRITE);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits
                        // Convert Hex Data string to bytes and add to data Array
                        for (int i = 0; i < writeByteCount * 2; i += 2)
                            dataArray[3 + (i / 2)] = (uint)(calculateParity(Convert.ToByte(data_hex.Substring(i, 2), 16)));
                    }

                    string PinName;

                    if (dutSlavePairIndex == 1) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                    else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    if (isShareBus) nameInMemory += "Pair" + dutSlavePairIndex;
                    DIGI.PatternControl.StartLabel = nameInMemory;

                    // Configure 6570 to source data calculated above
                    //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.CreateSerial(PinName, "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);



                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    // Get PassFail Results for site 0
                    Int64[] failureCount = sdata1Pin.GetFailCount();
                    NumBitErrors = (int)failureCount[0];
                    if (debug) Console.WriteLine("RegWrite " + registerAddress_hex + " Bit Errors: " + NumBitErrors.ToString());
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent0", "");
                    DIGI.PatternControl.Commit();
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Write Register for Address " + registerAddress_hex + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            /// <summary>
            /// Dynamic Register Read function.  This uses NI 6570 source memory to dynamically change
            /// the register address and uses NI 6570 capture memory to receive the values from the DUT.
            /// This supports extended and non-extended register read.
            /// </summary>
            /// <param name="registerAddress_hex">The register address to read (hex)</param>
            /// <returns>The value of the specified register in Hex</returns>
            public override string RegRead(string registerAddress_hex, bool writeHalf = false)
            {
                try
                {
                    if (!isVioTxPpmu)
                    {
                        // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }
                    else
                    {
                        allRffePinswoVio.SelectedFunction = SelectedFunction.Digital;
                        allRffePinswoVio.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    }

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    bool extendedRead = Convert.ToInt32(registerAddress_hex, 16) > 31;    // any register address > 5 bits requires extended read
                    uint readByteCount = 1;
                    string nameInMemory = extendedRead ? "ExtendedRegisterRead" + readByteCount.ToString() : "RegisterRead";




                    uint[] dataArray = new uint[512];
                    string CurrentSlaveAddress = "";
                    // Source buffer must contain 512 elements, even if sourcing less
                    if (!extendedRead)
                    {
                        // Build non-extended read command data
                        uint cmdBytesWithParity = generateRFFECommand(registerAddress_hex, Command.REGISTERREAD);
                        // Split data into array of data, all must be same # of bits (16) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity;
                    }
                    else
                    {
                        // Build extended read command data, setting read byte count and register address.
                        // Note, read byte count is 0 indexed.
                        uint cmdBytesWithParity = generateRFFECommand(Convert.ToString(readByteCount - 1, 16), Command.EXTENDEDREGISTERREAD);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[2] = (uint)(calculateParity(Convert.ToUInt16(registerAddress_hex, 16)));  // Final 9 bits to contains the address (for extended read) + parity.
                    }


                    string PinName;
                    string waveformNameinMemory = "";

                    if (dutSlavePairIndex == 1) PinName = Sdata1ChanName.ToUpper(); //Sdata1ChanName.ToUpper();
                    else PinName = Sdata2ChanName.ToUpper(); //Sdata1ChanName.ToUpper();


                    if (isShareBus) waveformNameinMemory = nameInMemory + "Pair" + dutSlavePairIndex;
                    else waveformNameinMemory = nameInMemory;

                    // Configure to source data
                    //DIGI.SourceWaveforms.CreateParallel(Sdata1ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast);
                    //DIGI.SourceWaveforms.CreateSerial(Sdata1ChanName.ToUpper(), "Src" + nameInMemory, SourceDataMapping.Broadcast, (uint)(extendedRead ? 9 : 16), BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.CreateSerial(PinName, "Src" + waveformNameinMemory, SourceDataMapping.Broadcast, (uint)(extendedRead ? 9 : 16), BitOrder.MostSignificantBitFirst);
                    //DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);
                    DIGI.SourceWaveforms.WriteBroadcast("Src" + waveformNameinMemory, dataArray);

                    // Configure to capture 8 bits (Ignore Parity)
                    //DIGI.CaptureWaveforms.CreateSerial(SdataChanName.ToUpper(), "Cap" + nameInMemory, readByteCount * 9, BitOrder.MostSignificantBitFirst);

                    // Get Num MIPI Bus and Current Slave address
                    // int Num_MIPI_Bus = Convert.ToUInt16(Get_Digital_Definition("NUM_MIPI_BUS"));
                    if (dutSlaveAddress.Length % 2 == 1)
                        CurrentSlaveAddress = dutSlaveAddress.PadLeft(dutSlaveAddress.Length + 1, '0');
                    else
                        CurrentSlaveAddress = dutSlaveAddress;

                    int Current_MIPI_Bus = dutSlavePairIndex;//  Convert.ToUInt16(Get_Digital_Definition("SLAVE_ADDR_" + CurrentSlaveAddress));


                    if (EqHSDIO.Num_Mipi_Bus == 1)
                        DIGI.CaptureWaveforms.CreateParallel(Sdata1ChanName.ToUpper(), "Cap" + nameInMemory);
                    else  // To Do: use Num_MIPI_Bus to automatically add the correct channels to PinSet paramenter in CreateParallel KH
                        DIGI.CaptureWaveforms.CreateParallel(Sdata1ChanName.ToUpper() + "," + Sdata2ChanName.ToUpper(), "Cap" + nameInMemory);

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    string Labelname = ((Digital_Definitions.ContainsKey(registerAddress_hex)|| writeHalf) ? "OTP" + nameInMemory : nameInMemory);

                    if (isShareBus) Labelname += "Pair" + dutSlavePairIndex;
                    DIGI.PatternControl.StartLabel = Labelname;

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    // Get PassFail Results for site 0
                    passFail = DIGI.PatternControl.GetSitePassFail("");
                    Int64[] failureCount = sdata1Pin.GetFailCount();
                    NumBitErrors = (int)failureCount[0];
                    if (debug) Console.WriteLine("RegRead " + registerAddress_hex + " Bit Errors: " + NumBitErrors.ToString());

                    // Retrieve captured waveform
                    uint[][] capData = new uint[][] { };
                    //uint[][] capData2 = new uint[][] { };

                    //DIGI.CaptureWaveforms.Fetch("", "Cap" + nameInMemory, 1, TimeSpan.FromSeconds(3), ref capData);
                    DIGI.CaptureWaveforms.Fetch("", "Cap" + nameInMemory, 8, TimeSpan.FromSeconds(3), ref capData);

                    //// Remove the parity bit   // this is for serial capture KH
                    //capData[0][0] = (capData[0][0] >> 1) & 0xFF;

                    var cheawklej = 1 << (EqHSDIO.Num_Mipi_Bus - Current_MIPI_Bus);

                    int RegisterData = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if ((capData[0][i] & 1 << (EqHSDIO.Num_Mipi_Bus - Current_MIPI_Bus)) != 0)  // MIPI bus data is represented by bit position. MIPI1 is bit 1: MIPI2 would be at Bit0. This is just masking and recording the correct bit (Bus) in data returned
                            RegisterData |= 1 << (7 - i);
                    }


                    string returnval = RegisterData.ToString("X"); // Convert captured data to hex string and return
                    //string returnval = RegisterData.ToString();

                    if (debug) Console.WriteLine("ReadReg " + registerAddress_hex + ": " + returnval);




                    //Ivi.Driver.PrecisionTimeSpan compareStrobe;
                    //compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(.000001);

                    //tsNRZ.ConfigureCompareEdgesStrobe(allRffePins, compareStrobe);
                    //tsNRZ.ConfigureCompareEdgesStrobe(sclk1Pin, compareStrobe);


                    return returnval;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Read Register for Address " + registerAddress_hex + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }
            }

            /// <summary>
            /// EEPROM Write not currently implemented
            /// </summary>
            /// <param name="dataWrite"></param>
            public override void EepromWrite(string dataWrite)
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(dataWrite + '\0');

                if (byteArray.Length > 256)
                {
                    MessageBox.Show("Exceeded maximum data length of 255 characters,\nEEPROM will not be written.", "EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                for (int tryWrite = 0; tryWrite < 5; tryWrite++)
                {
                    for (ushort reg = 0; reg < byteArray.Length; reg++)
                    {
                        // Burst pattern & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        // Set EEPROM register write address and data
                        uint[] dataArray = new uint[512];
                        dataArray[0] = (byte)reg;
                        dataArray[1] = byteArray[reg];

                        // Configure to source data, register address is up to 8 bits
                        DIGI.SourceWaveforms.CreateSerial(I2CSDAChanName.ToUpper(), "SrcEEPROMWrite", SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcEEPROMWrite", dataArray);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "EEPROMWrite", true, new TimeSpan(0, 0, 10));
                    }

                    if (EepromRead() == dataWrite)
                    {
                        MessageBox.Show("Writing & readback successful:\n\n    " + dataWrite, "EEPROM");
                        return;
                    }
                }

                MessageBox.Show("Writing NOT successful!", "EEPROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            /// <summary>
            /// EEPROM Read
            /// </summary>
            /// <returns></returns>
            public override string EepromRead()
            {
                try
                {
                    if (!eepromReadWriteEnabled)
                    {
                        //eepromReadWriteEnabled = this.SendVector("EEPROMEraseWriteEnable".ToLower());
                    }

                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allEEPROMPins.SelectedFunction = SelectedFunction.Digital;
                    allEEPROMPins.DigitalLevels.TerminationMode = TerminationMode.HighZ;
                    string returnval = "";
                    for (ushort reg = 0; reg < 256; reg++)
                    {
                        // Burst pattern & check for Pass/Fail
                        bool[] passFail = new bool[] { };

                        // Set EEPROM register read address
                        uint[] dataArray = new uint[512];
                        dataArray[0] = reg;

                        // Configure to source data, register address is up to 8 bits
                        DIGI.SourceWaveforms.CreateSerial(I2CSDAChanName.ToUpper(), "SrcEEPROMRead", SourceDataMapping.Broadcast, 8, BitOrder.MostSignificantBitFirst);
                        DIGI.SourceWaveforms.WriteBroadcast("SrcEEPROMRead", dataArray);

                        // Configure to capture 8 bits
                        DIGI.CaptureWaveforms.CreateSerial(I2CSDAChanName.ToUpper(), "CapEEPROMRead", 8, BitOrder.MostSignificantBitFirst);

                        // Burst Pattern
                        passFail = DIGI.PatternControl.BurstPattern("", "EEPROMRead", true, new TimeSpan(0, 0, 10));

                        // Retreive captured waveform
                        uint[][] capData = new uint[][] { };
                        DIGI.CaptureWaveforms.Fetch("", "CapEEPROMRead", 1, new TimeSpan(0, 0, 0, 0, 100), ref capData);

                        // Convert captured data to hex string and return
                        if (capData[0][0] != 0)
                        {
                            returnval += (char)capData[0][0];
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (debug) Console.WriteLine("EEPROMReadReg: " + returnval);
                    return returnval;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Read EEPROM Register.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }
            }

            private bool IsMaskedWrite(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands)
            {
                bool isMaskedWrite = false;
                foreach (MipiSyntaxParser.ClsMIPIFrame command in MipiCommands)
                {
                    if (command.IsMaskedWrite)
                    {
                        isMaskedWrite = true;
                        break;
                    }
                }
                return isMaskedWrite;
            }

            private uint[] GeneraterwaveformArry(ref int _numOfWrites, List<MipiSyntaxParser.ClsMIPIFrame> _MipiCommands, bool _EnableMaskedWrite, int _PairIndex = 0)
            {
                string data_hex = "";
                string registerAddress_hex = "";
                int numOfWrites = 0;
                // Source buffer must contain 512 elements, even if sourcing less
                uint[] dataArray = new uint[512];
                int dataArrayIndex = 0;
                int IndexStep = _EnableMaskedWrite ? 5 : 4;


                foreach (MipiSyntaxParser.ClsMIPIFrame command in _MipiCommands)
                {
                    if (_PairIndex != command.Pair && _PairIndex != 0) continue;

                    dutSlaveAddress = command.SlaveAddress_hex;
                    registerAddress_hex = command.Register_hex;
                    data_hex = command.Data_hex;

                    // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                    if (data_hex.Length % 2 == 1)
                        data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');

                    // Build extended write command data, setting read byte count and register address.
                    // Note, write byte count is 0 indexed.
                    uint cmdBytesWithParity;
                    bool extendedWrite = Convert.ToInt32(registerAddress_hex, 16) > 31;    // any register address > 5 bits requires extended read
                    int _CheckRegWriteValue = Convert.ToInt32(registerAddress_hex, 16);

                    if (extendedWrite == false && EqHSDIO.digitalwriteoption.EnableWrite0 && (_CheckRegWriteValue == 0))
                    {
                        // Build non-exteded write command
                        cmdBytesWithParity = generateRFFECommand(data_hex, Command.REGWRITE0, dutSlaveAddress);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial
                        dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // final 9 bits

                        dataArrayIndex = dataArrayIndex + IndexStep; // set for next command
                        numOfWrites++;
                    }
                    else if (extendedWrite == false && EqHSDIO.digitalwriteoption.EnableRegWrite && EqHSDIO.digitalwriteoption.RegWriteFrames.Any(v => v == _CheckRegWriteValue)) // (!extendedWrite)
                    {
                        // Build non-exteded write command
                        cmdBytesWithParity = generateRFFECommand(registerAddress_hex, Command.REGISTERWRITE, dutSlaveAddress);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial
                        dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(data_hex, 16)); // final 9 bits

                        dataArrayIndex = dataArrayIndex + IndexStep; // set for next command
                        numOfWrites++;
                    }
                    else
                    {
                        if (!command.IsMaskedWrite)
                        {
                            cmdBytesWithParity = generateRFFECommand(Convert.ToString(0, 16), Command.EXTENDEDREGISTERWRITE, dutSlaveAddress);
                            // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial
                            dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                            dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                            dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits

                            // Convert Hex Data string to bytes and add to data Array
                            dataArray[dataArrayIndex + 3] = calculateParity(Convert.ToUInt32(data_hex, 16)); // data 9 bits

                            dataArrayIndex = dataArrayIndex + IndexStep; // set for next command
                            numOfWrites++;
                        }
                        else
                        {
                            cmdBytesWithParity = generateRFFECommand(Convert.ToString(0, 16), Command.MASKEDREGISTERWRITE, dutSlaveAddress);
                            // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial
                            dataArray[dataArrayIndex + 0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                            dataArray[dataArrayIndex + 1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                            dataArray[dataArrayIndex + 2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits

                            // Convert Hex Data string to bytes and add to data Array
                            dataArray[dataArrayIndex + 3] = 0x01; //calculateParity(0xff - Convert.ToUInt32(data_hex, 16)); // data mask 9 bits
                            dataArray[dataArrayIndex + 4] = calculateParity(Convert.ToUInt32(data_hex, 16)); // data 9 bits

                            dataArrayIndex = dataArrayIndex + 5; // set for next command
                            numOfWrites++;
                        }
                    }
                }
                _numOfWrites = numOfWrites;

                return dataArray;
            }

            /// <summary>
            /// Close the NI 6570 session when shutting down the application
            /// and ensure all patterns are unloaded and all channels are disconnected.
            /// </summary>
            public override void Close()
            {
                allRffePins.SelectedFunction = SelectedFunction.Disconnect;
                allEEPROMPins.SelectedFunction = SelectedFunction.Disconnect;
                allTEMPSENSEPins.SelectedFunction = SelectedFunction.Disconnect;
                allEEPROM_UNIOPins.SelectedFunction = SelectedFunction.Disconnect;
                DIGI.Dispose();
            }


            #region Avago SJC Specific Helper Functions

            /// <summary>
            /// NI Internal Function:  Generate the requested RFFE command
            /// </summary>
            /// <param name="registerAddress_hex_or_ByteCount">For non-extended read / write, this is the register address.  For extended read / write, this is the number of bytes to read.</param>
            /// <param name="instruction">EXTENDEDREGISTERWRITE, EXTENDEDREGISTERREAD, REGISTERWRITE, or REGISTERREAD</param>
            /// <returns>The RFFE Command + Parity</returns>
            private uint generateRFFECommand(string registerAddress_hex_or_ByteCount, Command instruction, string _dutSlaveAddress = null)
            {
                int slaveAddress = (Convert.ToByte(_dutSlaveAddress ?? dutSlaveAddress, 16)) << 8;
                int commandFrame = 1 << 14;
                Byte regAddress = Convert.ToByte(registerAddress_hex_or_ByteCount, 16);

                Byte maxRange = 0, modifiedAddress = 0;

                switch (instruction)
                {
                    case Command.EXTENDEDREGISTERWRITELONG:
                        maxRange = 0x07;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x30);
                        break;
                    case Command.EXTENDEDREGISTERREADLONG:
                        maxRange = 0x07;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x38);
                        break;
                    case Command.EXTENDEDREGISTERWRITE:
                        maxRange = 0x0F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x00);
                        break;
                    case Command.EXTENDEDREGISTERREAD:
                        maxRange = 0x0F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x20);
                        break;
                    case Command.REGISTERWRITE:
                        maxRange = 0x1F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x40);
                        break;
                    case Command.REGISTERREAD:
                        maxRange = 0x1F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x60);
                        break;
                    case Command.MASKEDREGISTERWRITE:
                        maxRange = 0x19;
                        maxRange = 0x19;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x19);
                        break;
                    case Command.REGWRITE0:
                        maxRange = 0x7F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x80);
                        break;
                    default:
                        maxRange = 0x0F;
                        modifiedAddress = regAddress;
                        break;
                }

                if (regAddress != (maxRange & regAddress))
                    throw new Exception("Address out of range for requested command");

                // combine command frame, slave address, and modifiedAddress which contains the command and register address
                uint cmd = calculateParity((uint)(slaveAddress | modifiedAddress));
                cmd = (uint)(commandFrame) | cmd;
                return cmd;
            }

            /// <summary>
            /// NI Internal Function: Computes and appends parity 
            /// </summary>
            /// <param name="cmdWithoutParity"></param>
            /// <returns></returns>
            private uint calculateParity(uint cmdWithoutParity)
            {
                int x = (int)cmdWithoutParity;
                x ^= x >> 16;
                x ^= x >> 8;
                x ^= x >> 4;
                x &= 0x0F;
                bool parity = ((0x6996 >> x) & 1) != 0;
                return (uint)(cmdWithoutParity << 1 | Convert.ToByte(!parity));
            }

            /// <summary>
            /// Convert a .vec file to a compiled NI .digipat file and load into NI Digital Instrument Memory.
            ///   Stores the .vec file into a .digipatsrc file, then compiles the .digipatsrc file into a .digipat
            ///   file, and finally loads the .digipat file into instrument memory.
            ///   If the .digipat and .digipatsrc files already exist, the .vec checksum is compared to check for
            ///   any changes to the .vec.  if no changes were made, the previously compiled .digipat is loaded.
            ///   If changes were detected, the .vec is re-converted to .digipat and is then loaded.
            /// </summary>
            /// <param name="vecPath">The absolute path of the .vec file to be loaded</param>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <param name="convertToCapture">If false (default), the .vec file is loaded normally.  If true, H / L in the original .vec will be converted to V, and capture opcodes will be added so that the NI 6570 capture memory can be used.</param>
            /// <param name="load">If true (default), the pattern is loaded into instrument memory after conversion and compilation.  If false, the pattern is converted and compiled, but not loaded.</param>
            /// <returns>True if pattern conversion, compilation, and loading to instrument memory succeeds.</returns>           
            public bool ConvertVecAndLoadPattern(string vecPath, string patternName, bool overwrite, Timeset timeSet, ref uint captureCount, bool load = true)
            {
                patternName = patternName.Replace("_", "").ToLower();

                #region Generate Paths
                string digipatsrcPath = fileDir + "\\" + Path.GetFileNameWithoutExtension(vecPath) + "_" + patternName + ".digipatsrc";
                string digipatPath = fileDir + "\\" + Path.GetFileNameWithoutExtension(vecPath) + "_" + patternName + ".digipat"; ;
                #endregion

                #region Check if files exist and handle appropriately
                if (!File.Exists(vecPath))
                {
                    // Specified .vec not found, can't proceed
                    throw new FileNotFoundException("Specified .vec file Not Found", vecPath);
                }
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath);
                    try
                    {
                        string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                        overwrite = MD5 != ComputeMD5Hash(vecPath);
                    }
                    catch
                    {
                        overwrite = true;
                    }
                    digipatsrcFileMD5.Close();
                }
                // Check if previously compiled digipat exits and overwrite if specified to do so
                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                    Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                        if (File.Exists(Path.ChangeExtension(digipatPath, "tdms_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "tdms_index"));
                    }
                    else
                    {
                        // Count # of captures in current digipatsrc file and return in ref captureCount
                        captureCount = 0;
                        System.IO.StreamReader digipatsrc = new System.IO.StreamReader(digipatsrcPath);
                        string srcline = "";
                        while ((srcline = digipatsrc.ReadLine()) != null)
                        {
                            if (srcline.Contains("capture\t"))
                                captureCount++;
                        }
                        digipatsrc.Close();

                        // Compiled digipat already exists and digipatsrc matches the .vec checksum; Load digipat now.
                        DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }
                if (File.Exists(digipatsrcPath))
                {
                    // Delete digipatsrc file if it already exists, do this after digipat check (don't delete src if digipat already exists)
                    File.Delete(digipatsrcPath);
                }
                #endregion

                #region Convert from .vec to .digipatsrc

                #region Open Vec File and digipatsrc File
                System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath);
                System.IO.StreamReader vecFile = new System.IO.StreamReader(vecPath);
                #endregion

                #region Write Header
                digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(vecPath));
                digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                digipatsrcFile.WriteLine("// Automatically Generated from the ConvertVecAndLoadPattern function.");
                digipatsrcFile.WriteLine("// Original Pattern File: " + Path.GetFileName(vecPath));
                digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                digipatsrcFile.WriteLine("//\n");
                //digipatsrcFile.WriteLine("file_format_version 1.0;");
                digipatsrcFile.WriteLine("file_format_version 1.1;");  //By setting this to 1.1 will force the vector to the LVM (large vector memory) this alows more vectors to be loaded but you can't use "jump" and "call" opcodes
                digipatsrcFile.WriteLine("timeset " + timeSet.ToString("g") + ";");
                digipatsrcFile.Write("\n");
                #endregion


                #region Read Vec File and store in digipatsrc File
                // Read first line of Vec File, this contains the pin names
                string currentline = vecFile.ReadLine();
                string[] vecpins = currentline.ToUpper().Trim().Split(',');
                // Build all DUT Pins pinlist
                string pinlist = string.Join(",", this.allDutPins).ToUpper();
                // Write start of pattern, line contains comma separated pin names.  Make sure we have a Trigger channel.
                digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                digipatsrcFile.WriteLine("{");


                captureCount = 0;
                // Read remaining lines of Vec File, these are the vectors
                while ((currentline = vecFile.ReadLine()) != null)
                {
                    string[] lineData = currentline.Trim().Split(',');
                    // Add a expected channels if necessary.  These are is always appended in their correct column when converting .vec files.
                    string lineOutput = "";

                    foreach (string pin in this.allDutPins)
                    {
                        if (vecpins.Contains(pin.ToUpper()))
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.ToUpper())];
                        }

                        else if (pin.Contains("SCLK"))   // determine if MIPI pin    
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, Get_Digital_Definition("SCLK1_VEC_NAME"))];  // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)  
                        }
                        else if (pin.Contains("SDATA"))   // determine if MIPI pin    
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, Get_Digital_Definition("SDATA1_VEC_NAME"))];  // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)  
                        }
                        else if (pin.Contains("VIO"))   // determine if MIPI pin    
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, Get_Digital_Definition("VIO1_VEC_NAME"))];  // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)  
                        }
                        else // not a MIPI pin
                        {
                            lineOutput += "\tX";
                        }
                    }



                    if (vecFile.EndOfStream)
                    {
                        //if (convertToCapture)
                        //    digipatsrcFile.WriteLine(("capture_stop\t") + timeSet.ToString("g") + "\t" + lineOutput + ";");
                        digipatsrcFile.WriteLine(("halt\t") + timeSet.ToString("g") + lineOutput + ";");
                    }
                    else
                        //digipatsrcFile.WriteLine(("") + timeSet.ToString("g") + "\t" + lineOutput + ";");
                        digipatsrcFile.WriteLine(("") + timeSet.ToString("g") + lineOutput + ";");
                    // }
                }
                // Close out pattern
                digipatsrcFile.WriteLine("}\n");
                #endregion

                #region close text Vec and digipatsrc Files
                vecFile.Close();
                digipatsrcFile.Close();
                #endregion
                #endregion

                // Call the Pattern Compiler and load into memory.
                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, allDutPins);
            }


            /// <summary>
            /// Convert a .vec file to a compiled NI .digipat file and load into NI Digital Instrument Memory.
            ///   Stores the .vec file into a .digipatsrc file, then compiles the .digipatsrc file into a .digipat
            ///   file, and finally loads the .digipat file into instrument memory.
            ///   If the .digipat and .digipatsrc files already exist, the .vec checksum is compared to check for
            ///   any changes to the .vec.  if no changes were made, the previously compiled .digipat is loaded.
            ///   If changes were detected, the .vec is re-converted to .digipat and is then loaded.
            /// </summary>
            /// <param name="vecPath">The absolute path of the .vec file to be loaded</param>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <param name="convertToCapture">If false (default), the .vec file is loaded normally.  If true, H / L in the original .vec will be converted to V, and capture opcodes will be added so that the NI 6570 capture memory can be used.</param>
            /// <param name="load">If true (default), the pattern is loaded into instrument memory after conversion and compilation.  If false, the pattern is converted and compiled, but not loaded.</param>
            /// <returns>True if pattern conversion, compilation, and loading to instrument memory succeeds.</returns>       
            public bool ConvertVecAndLoadPattern(string vecPath, string patternName, bool overwrite, ref uint captureCount, Timeset timeSet, Timeset timeSet_read = Timeset.MIPI_RZ_HALF)
            {
                patternName = patternName.Replace("_", "").ToLower();

                #region Generate Paths
                string digipatsrcPath = fileDir + "\\" + Path.GetFileNameWithoutExtension(vecPath) + "_" + patternName + ".digipatsrc";
                string digipatPath = fileDir + "\\" + Path.GetFileNameWithoutExtension(vecPath) + "_" + patternName + ".digipat";
                #endregion Generate Paths

                #region Check if files exist and handle appropriately
                if (!File.Exists(vecPath))
                {
                    // Specified .vec not found, can't proceed
                    throw new FileNotFoundException("Specified .vec file Not Found", vecPath);
                }
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    using (System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath))
                    {
                        try
                        {
                            string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                            overwrite = MD5 != ComputeMD5Hash(vecPath);
                        }
                        catch
                        {
                            overwrite = true;
                        }
                    }
                }
                // Check if previously compiled digipat exits and overwrite if specified to do so
                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                    Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                        if (File.Exists(Path.ChangeExtension(digipatPath, "tdms_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "tdms_index"));
                    }
                    else
                    {
                        // Count # of captures in current digipatsrc file and return in ref captureCount
                        captureCount = 0;
                        using (System.IO.StreamReader digipatsrc = new System.IO.StreamReader(digipatsrcPath))
                        {
                            string srcline = "";
                            while ((srcline = digipatsrc.ReadLine()) != null)
                            {
                                if (srcline.Contains("capture\t"))
                                    captureCount++;
                            }
                        }

                        // Compiled digipat already exists and digipatsrc matches the .vec checksum; Load digipat now.
                        lock (lockObject)
                            DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }

                #endregion Check if files exist and handle appropriately

                #region Convert from .vec to .digipatsrc

                using (System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath, false))
                {
                    using (System.IO.StreamReader vecFile = new System.IO.StreamReader(vecPath))
                    {
                        bool forceHalfClk = ((timeSet & (Timeset.MIPI_HALF | Timeset.MIPI_RZ_HALF)) > 0);
                        var _TimesetFromVector1 = Timeset.NONE;
                        var _TimesetFromVector2 = Timeset.NONE;
                        var _TimesetFromVector3 = Timeset.NONE;

                        #region Write Header

                        digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(vecPath));
                        digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                        digipatsrcFile.WriteLine("// Automatically Generated from the ConvertVecAndLoadPattern function.");
                        digipatsrcFile.WriteLine("// Original Pattern File: " + Path.GetFileName(vecPath));
                        digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                        digipatsrcFile.WriteLine("//\n");
                        //digipatsrcFile.WriteLine("file_format_version 1.0;");
                        digipatsrcFile.WriteLine("file_format_version 1.1;");  //By setting this to 1.1 will force the vector to the LVM (large vector memory) this alows more vectors to be loaded but you can't use "jump" and "call" opcodes

                        var timesetList = Timeset.NONE;
                        if ((timeSet & (Timeset.MIPI | Timeset.MIPI_HALF)) > 0)
                        {
                            timesetList |= Timeset.MIPI | Timeset.MIPI_HALF;

                            _TimesetFromVector1 = Timeset.MIPI;
                            _TimesetFromVector2 = Timeset.MIPI_HALF;
                            _TimesetFromVector3 = Timeset.MIPI_HALF;
                        }
                        else if ((timeSet & (Timeset.MIPI_RZ | Timeset.MIPI_RZ_HALF | Timeset.MIPI_RZ_QUAD)) > 0)
                        {
                            timesetList = Timeset.MIPI_RZ | Timeset.MIPI_RZ_HALF | Timeset.MIPI_RZ_QUAD;

                            _TimesetFromVector1 = Timeset.MIPI_RZ;
                            _TimesetFromVector2 = Timeset.MIPI_RZ_HALF;
                            _TimesetFromVector3 = Timeset.MIPI_RZ_QUAD;
                        }

                        digipatsrcFile.WriteLine("timeset " + string.Join(", ", timesetList) + ";");

                        digipatsrcFile.Write("\n");

                        #endregion Write Header

                        #region Read Vec File and store in digipatsrc File

                        bool FormatNewTimet = false;
                        int IndexNewTimeSet = 0;
                        // Read first line of Vec File, this contains the pin names
                        string currentline = vecFile.ReadLine();
                        var vecpins = currentline.ToUpper().Trim().Split(',').ToList();

                        if (!vecpins.Any(x => PinNamesAndChans.Keys.ToList().Any(y => y == x)))
                        {
                            MessageBox.Show("Pin names in the vector file do not match with the declared pin name list in the test program.", "Please fix the pin names in the vector file", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                        }
                        if (currentline.Contains("TimeSet"))
                        {
                            FormatNewTimet = true;
                            IndexNewTimeSet = vecpins.IndexOf("TIMESET");
                            vecpins.Remove("TIMESET");
                        }

                        if (!vecpins.All(allDutPins.Contains)) throw new Exception("Requested vec pins are not subset for Modularmain pre-defined pins. Check the vectore file.");

                        string pinlist = string.Join(",", vecpins).ToUpper();
                        // Write start of pattern, line contains comma separated pin names.  Make sure we have a Trigger channel.
                        digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                        digipatsrcFile.WriteLine("{");

                        string[] testdata = new string[vecpins.Count() + 1];
                        for (int i = 0; i < testdata.Length; i++)
                        {
                            testdata[i] = "X";
                        }

                        string lineOutput;
                        string[] lineData;
                        var ArrCount = vecpins.Count();

                        captureCount = 0;
                        // Read remaining lines of Vec File, these are the vectors
                        while ((currentline = vecFile.ReadLine()) != null)
                        {
                            lineOutput = "";
                            lineData = currentline.Trim().ToUpper().Split(',');

                            // Add a expected channels if necessary.  These are is always appended in their correct column when converting .vec files.
                            try
                            {
                                if (FormatNewTimet && (forceHalfClk == false))
                                {
                                    var SelectedTimeset = timeSet;

                                    if (lineData[IndexNewTimeSet].StartsWith("F"))
                                        SelectedTimeset = _TimesetFromVector1;
                                    else if (lineData[IndexNewTimeSet].StartsWith("H"))
                                        SelectedTimeset = _TimesetFromVector2;
                                    else if (lineData[IndexNewTimeSet].StartsWith("Q"))
                                        SelectedTimeset = _TimesetFromVector3;

                                    //switch (lineData[IndexNewTimeSet])
                                    //{
                                    //    case "FULL":
                                    //        SelectedTimeset = _TimesetFromVector1;
                                    //        break;

                                    //    case "HALF":
                                    //        SelectedTimeset = _TimesetFromVector2;
                                    //        break;

                                    //    case "QUAD":
                                    //        SelectedTimeset = _TimesetFromVector3;
                                    //        break;
                                    //}

                                    if (new[] { "H", "L", "V" }.Any(lineData.Contains) && ((SelectedTimeset & (Timeset.MIPI | Timeset.MIPI_RZ)) > 0))
                                        testdata[0] = _TimesetFromVector2.ToString("g");
                                    else
                                        testdata[0] = SelectedTimeset.ToString("g");

                                    for (int i = 0; i < ArrCount; i++)
                                    {
                                        testdata[vecpins.IndexOf(vecpins[i]) + 1] = lineData[i];
                                    }
                                }
                                else
                                {
                                    if (new[] { "H", "L", "V" }.Any(lineData.Contains))
                                        testdata[0] = timeSet_read.ToString("g");
                                    else
                                        testdata[0] = timeSet.ToString("g");

                                    for (int i = 0; i < ArrCount; i++)
                                    {
                                        testdata[vecpins.IndexOf(vecpins[i]) + 1] = lineData[i];
                                    }
                                }

                                lineOutput = string.Join("\t", testdata);
                            }
                            catch (Exception e)
                            {
                                throw new Exception(e.ToString());
                            }

                            if (vecFile.EndOfStream)
                                digipatsrcFile.WriteLine(("halt\t") + lineOutput + ";");
                            else
                                digipatsrcFile.WriteLine(("") + lineOutput + ";");
                        }
                        // Close out pattern
                        digipatsrcFile.WriteLine("}\n");

                        #endregion Read Vec File and store in digipatsrc File
                    }
                }
                #endregion Convert from .vec to .digipatsrc

                // Call the Pattern Compiler and load into memory.
                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, allDutPins);
            }


            /// <summary>
            /// Create a .digipatsrc file from the given inputs and compile into a .digipat file.
            /// Once compilation of the .digipat succeeds, load the pattern into instrument memory.
            /// </summary>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="pinList">The pins associated with this pattern.  These must match the timeset.  For NRZ patterns, the timeset is "MIPI"; otherwise the timeset is "MIPI_SCLK_RZ"</param>
            /// <param name="pattern">The pattern specified by a 2-d array of strings, one column per pin, columns must correspond to pinList array.</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <returns>True if pattern compilation and loading to instrument memory succeeds.</returns>
            public bool GenerateAndLoadPattern(string patternName, string[] pinList, string[,] pattern, bool overwrite, Timeset timeSet)
            {
                // Convert from string[,] into slightly better List<string[]>
                List<string[]> newPattern = new List<string[]>(pattern.GetLength(0));
                for (int x = 0; x < pattern.GetLength(0); x++)
                {
                    string[] tmp = new string[pattern.GetLength(1)];
                    for (int y = 0; y < pattern.GetLength(1); y++)
                        tmp[y] = pattern[x, y];
                    newPattern.Add(tmp);
                }
                loadedPatternNames.Add(patternName.ToLower());
                return GenerateAndLoadPattern(patternName, pinList, newPattern, overwrite, timeSet);
            }

            /// <summary>
            /// Create a .digipatsrc file from the given inputs and compile into a .digipat file.
            /// Once compilation of the .digipat succeeds, load the pattern into instrument memory.
            /// </summary>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="pinList">The pins associated with this pattern.  These must match the timeset.  For NRZ patterns, the timeset is "MIPI"; otherwise the timeset is "MIPI_SCLK_RZ"</param>
            /// <param name="pattern">The pattern specified by a list of string arrays, one list item containing a 1-d array of string for each line in the vector.  1-D array must correspond to pinList array.</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <returns>True if pattern compilation and loading to instrument memory succeeds.</returns>
            public bool GenerateAndLoadPattern(string patternName, string[] vecpins, List<string[]> pattern, bool overwrite, Timeset timeSet, Timeset timeSet_read = Timeset.MIPI_RZ_HALF)
            {
                #region Generate Paths & Constants
                patternName = patternName.Replace("_", "");
                string patternSavePath = fileDir + "\\" + patternName + ".digipat";
                string digipatsrcPath = Path.ChangeExtension(patternSavePath, "digipatsrc");
                string digipatPath = Path.ChangeExtension(patternSavePath, "digipat");
                #endregion

                #region Check if files exist and handle appropriately
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath);
                    try
                    {
                        string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                        overwrite = MD5 != ComputeMD5Hash(pattern);
                    }
                    catch
                    {
                        overwrite = true;
                    }
                    digipatsrcFileMD5.Close();
                }

                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                    Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                    }
                    else
                    {
                        // Compiled digipat already exists, just load it
                        DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }
                if (File.Exists(digipatsrcPath))
                {
                    // Delete digipatsrc file if it already exists, do this after digipat check (don't delete src if digipat already exists)
                    File.Delete(digipatsrcPath);
                }
                #endregion

                #region Generate .digipatsrc

                #region Open digipatsrc File
                System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath);
                #endregion

                #region Write Header
                digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(pattern));
                digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                digipatsrcFile.WriteLine("// Automatically Generated from the GenerateAndLoadPattern function.");
                digipatsrcFile.WriteLine("// Pattern Name: " + patternName);
                digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                digipatsrcFile.WriteLine("//\n");
                //digipatsrcFile.WriteLine("file_format_version 1.0;");
                digipatsrcFile.WriteLine("file_format_version 1.1;");  //By setting this to 1.1 will force the vector to the LVM (large vector memory) this alows more vectors to be loaded but you cant use "jump" and "call" opcodes

                if (timeSet_read == timeSet)
                    digipatsrcFile.WriteLine("timeset " + timeSet.ToString("g") + ";");
                else
                    digipatsrcFile.WriteLine("timeset " + timeSet.ToString("g") + ", " + timeSet_read.ToString("g") + ";");

                digipatsrcFile.Write("\n");
                #endregion

                #region Loop through vectors and store in digipatsrc File

                // Write start of pattern, line contains comma separated pin names
                string pinlist = string.Join(",", this.allDutPins).ToUpper();
                digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                digipatsrcFile.WriteLine("{");

                // Write all vector lines
                foreach (string[] lineData in pattern)
                {
                    // Add Timeset and opcode at the start
                    //string lineOutput = lineData[0] + "\t" + timeSet.ToString("g");

                    string lineOutput = null;
                    int count = 0;

                    foreach (string pin in this.allDutPins)
                    {
                        if (count == 0)
                        {
                            if ((timeSet != timeSet_read) &&
                                (vecpins.Contains(Sdata1ChanName) || vecpins.Contains(Sdata2ChanName)))
                            {
                                if ((lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('H') |
                                    lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('L') |
                                    lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('V') |
                                    lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 5] == "V"))        ///////////////////////add timeSet_read for H/L
                                    lineOutput = lineData[0] + "\t" + timeSet_read.ToString("g");
                                else
                                    lineOutput = lineData[0] + "\t" + timeSet.ToString("g");
                            }
                            else
                            {
                                lineOutput = lineData[0] + "\t" + timeSet.ToString("g");
                            }
                        }

                        if (vecpins.Contains(pin.ToUpper()))
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 1];
                        }

                        else if (pin.Contains("SCLK") && (Array.IndexOf(vecpins, Get_Digital_Definition("SCLK1_VEC_NAME")) != -1))   // determine if MIPI pin    
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, Get_Digital_Definition("SCLK1_VEC_NAME")) + 1];  // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)  
                        }
                        else if (pin.Contains("SDATA") && (Array.IndexOf(vecpins, Get_Digital_Definition("SDATA1_VEC_NAME")) != -1))   // determine if MIPI pin    
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, Get_Digital_Definition("SDATA1_VEC_NAME")) + 1];  // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)  
                        }
                        else if (pin.Contains("VIO") && (Array.IndexOf(vecpins, Get_Digital_Definition("VIO1_VEC_NAME")) != -1))   // determine if MIPI pin    
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, Get_Digital_Definition("VIO1_VEC_NAME")) + 1];  // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)  
                        }
                        else // not a MIPI pin
                        {
                            lineOutput += "\tX";
                        }

                        count++;
                    }
                    // Handle Comment, it is always the last item in the string array
                    if (lineData[lineData.Count() - 1] != "")
                        lineOutput += @"; // " + lineData[lineData.Count() - 1] + "\n";
                    else
                        lineOutput += ";\n";

                    digipatsrcFile.Write(lineOutput);
                }

                // Close out pattern
                digipatsrcFile.WriteLine("}\n");
                #endregion

                #region close text digipatsrc File
                digipatsrcFile.Close();
                #endregion
                #endregion

                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, this.allDutPins, true);
            }

            /// <summary>
            /// Create a .digipatsrc file from the given inputs and compile into a .digipat file.
            /// Once compilation of the .digipat succeeds, load the pattern into instrument memory.
            /// </summary>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="pinList">The pins associated with this pattern.  These must match the timeset.  For NRZ patterns, the timeset is "MIPI"; otherwise the timeset is "MIPI_SCLK_RZ"</param>
            /// <param name="pattern">The pattern specified by a list of string arrays, one list item containing a 1-d array of string for each line in the vector.  1-D array must correspond to pinList array.</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <returns>True if pattern compilation and loading to instrument memory succeeds.</returns>
            public bool GenerateAndLoadPattern(string dummy, string patternName, string[] vecpins, List<string[]> pattern, bool overwrite, Timeset timeSet)
            {
                #region Generate Paths & Constants
                patternName = patternName.Replace("_", "");
                string patternSavePath = fileDir + "\\" + patternName + ".digipat";
                string digipatsrcPath = Path.ChangeExtension(patternSavePath, "digipatsrc");
                string digipatPath = Path.ChangeExtension(patternSavePath, "digipat");
                #endregion

                #region Check if files exist and handle appropriately
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath);
                    try
                    {
                        string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                        overwrite = MD5 != ComputeMD5Hash(pattern);
                    }
                    catch
                    {
                        overwrite = true;
                    }
                    digipatsrcFileMD5.Close();
                }

                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                    Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                    }
                    else
                    {
                        // Compiled digipat already exists, just load it
                        DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }
                if (File.Exists(digipatsrcPath))
                {
                    // Delete digipatsrc file if it already exists, do this after digipat check (don't delete src if digipat already exists)
                    File.Delete(digipatsrcPath);
                }
                #endregion

                #region Generate .digipatsrc

                #region Open digipatsrc File
                System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath);
                #endregion

                #region Write Header
                digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(pattern));
                digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                digipatsrcFile.WriteLine("// Automatically Generated from the GenerateAndLoadPattern function.");
                digipatsrcFile.WriteLine("// Pattern Name: " + patternName);
                digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                digipatsrcFile.WriteLine("//\n");
                digipatsrcFile.WriteLine("file_format_version 1.0;");
                digipatsrcFile.WriteLine("timeset " + timeSet.ToString("g") + ";");
                digipatsrcFile.Write("\n");
                #endregion

                #region Loop through vectors and store in digipatsrc File

                // Write start of pattern, line contains comma separated pin names
                string pinlist = string.Join(",", this.allDutPins).ToUpper();
                digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                digipatsrcFile.WriteLine("{");

                // Write all vector lines
                foreach (string[] lineData in pattern)
                {
                    // Add Timeset and opcode at the start
                    string lineOutput = lineData[0] + "\t" + timeSet.ToString("g");
                    foreach (string pin in this.allDutPins)
                    {
                        if (vecpins.Contains(pin.ToUpper()))
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 1];
                        }
                        else
                        {
                            lineOutput += "\tX";
                        }
                    }
                    // Handle Comment, it is always the last item in the string array
                    if (lineData[lineData.Count() - 1] != "")
                        lineOutput += @"; // " + lineData[lineData.Count() - 1] + "\n";
                    else
                        lineOutput += ";\n";

                    digipatsrcFile.Write(lineOutput);
                }

                // Close out pattern
                digipatsrcFile.WriteLine("}\n");
                #endregion

                #region close text digipatsrc File
                digipatsrcFile.Close();
                #endregion
                #endregion

                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, this.allDutPins, true);
            }

            //keng shan added
            public bool GenerateAndLoadPattern(string dummy, string patternName, string[] vecpins, List<string[]> pattern, bool overwrite, Timeset timeSet_write, Timeset timeSet_read)
            {
                //string dummy is to differentiate between normal and RF On Off pattern generate and loading function
                #region Generate Paths & Constants
                patternName = patternName.Replace("_", "");
                string patternSavePath = fileDir + "\\" + patternName + ".digipat";
                string digipatsrcPath = Path.ChangeExtension(patternSavePath, "digipatsrc");
                string digipatPath = Path.ChangeExtension(patternSavePath, "digipat");
                #endregion

                #region Check if files exist and handle appropriately
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath);
                    try
                    {
                        string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                        overwrite = MD5 != ComputeMD5Hash(pattern);
                    }
                    catch
                    {
                        overwrite = true;
                    }
                    digipatsrcFileMD5.Close();
                }

                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                                Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                    }
                    else
                    {
                        // Compiled digipat already exists, just load it
                        DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }
                if (File.Exists(digipatsrcPath))
                {
                    // Delete digipatsrc file if it already exists, do this after digipat check (don't delete src if digipat already exists)
                    File.Delete(digipatsrcPath);
                }
                #endregion

                #region Generate .digipatsrc

                #region Open digipatsrc File
                System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath);
                #endregion

                #region Write Header
                digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(pattern));
                digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                digipatsrcFile.WriteLine("// Automatically Generated from the GenerateAndLoadPattern function.");
                digipatsrcFile.WriteLine("// Pattern Name: " + patternName);
                digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                digipatsrcFile.WriteLine("//\n");
                digipatsrcFile.WriteLine("file_format_version 1.0;");
                digipatsrcFile.WriteLine("timeset " + timeSet_write.ToString("g") + ", " + timeSet_read.ToString("g") + ";");
                digipatsrcFile.Write("\n");
                #endregion

                #region Loop through vectors and store in digipatsrc File

                // Write start of pattern, line contains comma separated pin names
                string pinlist = string.Join(",", this.allDutPins).ToUpper();
                digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                digipatsrcFile.WriteLine("{");

                // Write all vector lines
                foreach (string[] lineData in pattern)
                {
                    // Add Timeset and opcode at the start
                    string lineOutput = null;
                    int count = 0;

                    foreach (string pin in this.allDutPins)
                    {
                        if (count == 0)
                        {
                            if ((lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('H') | lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('L') | lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('V')))        ///////////////////////add timeSet_read for H/L
                                lineOutput = lineData[0] + "\t" + timeSet_read.ToString("g");
                            else
                                lineOutput = lineData[0] + "\t" + timeSet_write.ToString("g");
                        }

                        if (vecpins.Contains(pin.ToUpper()))
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 1];
                        }
                        else if (vecpins.Contains(pin.TrimEnd(pin[pin.Length - 1]).ToUpper() + "1"))   // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)    the bus
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.TrimEnd(pin[pin.Length - 1]).ToUpper() + "1") + 1];


                        else
                        {
                            lineOutput += "\tX";
                        }
                        count++;
                    }

                    // Handle Comment, it is always the last item in the string array
                    if (lineData[lineData.Count() - 1] != "")
                        lineOutput += @"; // " + lineData[lineData.Count() - 1] + "\n";
                    else
                        lineOutput += ";\n";

                    digipatsrcFile.Write(lineOutput);
                }

                // Close out pattern
                digipatsrcFile.WriteLine("}\n");
                #endregion

                #region close text digipatsrc File
                digipatsrcFile.Close();
                #endregion
                #endregion

                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, this.allDutPins, true);
            }

            public bool GenerateAndLoadPattern(string patternName, string[] vecpins, List<string[]> pattern, bool overwrite, Timeset timeSet_write, Timeset timeSet_read, bool NotUsd)
            {
                #region Generate Paths & Constants
                patternName = patternName.Replace("_", "");
                string patternSavePath = fileDir + "\\" + patternName + ".digipat";
                string digipatsrcPath = Path.ChangeExtension(patternSavePath, "digipatsrc");
                string digipatPath = Path.ChangeExtension(patternSavePath, "digipat");
                #endregion

                #region Check if files exist and handle appropriately
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath);
                    try
                    {
                        string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                        overwrite = MD5 != ComputeMD5Hash(pattern);
                    }
                    catch
                    {
                        overwrite = true;
                    }
                    digipatsrcFileMD5.Close();
                }

                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                    Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                    }
                    else
                    {
                        // Compiled digipat already exists, just load it
                        DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }
                if (File.Exists(digipatsrcPath))
                {
                    // Delete digipatsrc file if it already exists, do this after digipat check (don't delete src if digipat already exists)
                    File.Delete(digipatsrcPath);
                }
                #endregion

                #region Generate .digipatsrc

                #region Open digipatsrc File
                System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath);
                #endregion

                #region Write Header
                digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(pattern));
                digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                digipatsrcFile.WriteLine("// Automatically Generated from the GenerateAndLoadPattern function.");
                digipatsrcFile.WriteLine("// Pattern Name: " + patternName);
                digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                digipatsrcFile.WriteLine("//\n");
                digipatsrcFile.WriteLine("file_format_version 1.0;");

                digipatsrcFile.WriteLine("export " + patternName.ToUpper() + ";");                  //////////////For TriggerPattern

                if (patternName == "Trigger")
                    digipatsrcFile.WriteLine("import TRIGGERCHECK;");                //////////////Dummy Pattern

                if (timeSet_read == timeSet_write)
                    digipatsrcFile.WriteLine("timeset " + timeSet_write.ToString("g") + ";");
                else
                    digipatsrcFile.WriteLine("timeset " + timeSet_write.ToString("g") + ", " + timeSet_read.ToString("g") + ";");
                digipatsrcFile.Write("\n");
                #endregion

                #region Loop through vectors and store in digipatsrc File

                // Write start of pattern, line contains comma separated pin names
                string pinlist = string.Join(",", this.allDutPins).ToUpper();
                digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                digipatsrcFile.WriteLine("{");

                //digipatsrcFile.WriteLine(patternName.ToUpper()+":/n");              //////////////////added by NI 161220


                // Write all vector lines
                foreach (string[] lineData in pattern)
                {
                    // Add Timeset and opcode at the start
                    string lineOutput = null;
                    int count = 0;

                    foreach (string pin in this.allDutPins)
                    {
                        if (count == 0)
                        {
                            if ((lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('H') | lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('L') | lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 2].Contains('V')))        ///////////////////////add timeSet_read for H/L
                                lineOutput = lineData[0] + "\t" + timeSet_read.ToString("g");
                            else
                                lineOutput = lineData[0] + "\t" + timeSet_write.ToString("g");
                        }

                        if (pin.ToUpper().Contains("VIO"))
                        {
                            lineOutput += "\t1";

                        }
                        else
                        {
                            if (vecpins.Contains(pin.ToUpper()))
                            {
                                lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 1];
                            }

                            else if (vecpins.Contains(pin.TrimEnd(pin[pin.Length - 1]).ToUpper() + "1"))   // this uses the same Data in the primary MIPI bus1 (PA) for MIPI Bus2 (RX)    the bus
                                lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.TrimEnd(pin[pin.Length - 1]).ToUpper() + "1") + 1];

                            else
                            {
                                lineOutput += "\tX";
                            }
                        }
                        count++;
                    }

                    // Handle Comment, it is always the last item in the string array
                    if (lineData[lineData.Count() - 1] != "")
                        lineOutput += @"; // " + lineData[lineData.Count() - 1] + "\n";
                    else
                        lineOutput += ";\n";

                    digipatsrcFile.Write(lineOutput);
                }

                // Close out pattern
                digipatsrcFile.WriteLine("}\n");
                #endregion

                #region close text digipatsrc File
                digipatsrcFile.Close();
                #endregion
                #endregion

                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, this.allDutPins, true);
            }

            /// <summary>
            /// NI Internal Function:  Given a digipatsrc file, compile and save into the given digipat file, using
            /// the specified patternName and Pins.  Generate Paths, Check if compiler exists and handle appropriately, Create a dummy pinmap containing the specified pins (pinmap required by compiler), then compile the digipatsrc into digipat.
            /// </summary>
            /// <param name="digipatsrcPath">The Absolute path to the digipatsrc file</param>
            /// <param name="digipatPath">The Absolute path to the desired digipat file output</param>
            /// <param name="patternName">The name of the pattern, used later to load the file into memory</param>
            /// <param name="pins">The pins in the pattern file</param>
            /// <param name="addTrig">If True, this indicates that an extra trigger channel was added, but the pins array doesn't contain it so we should add it during compile</param>
            /// <param name="load">If True, this function will automatically load the pattern into instrument memory after a successful compile</param>
            /// <returns>True if compilation and loading succeeds</returns>
            private bool CompileDigipatSrc(string digipatsrcPath, string digipatPath, string patternName, string[] pins, bool load = true)
            {
                #region Generate Paths
                string compilerPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\National Instruments\\Digital Pattern Compiler\\DigitalPatternCompiler.exe";
                string pinmapPath = Path.GetTempFileName() + ".pinmap";// fileDir + "\\compiler.pinmap";
                #endregion

                #region Check if compiler exists and handle appropriately
                if (!File.Exists(compilerPath))
                {
                    // Compiler not found, can't proceed
                    throw new FileNotFoundException("Digital Pattern Compiler Not Found", compilerPath);
                }
                #endregion

                #region Constants
                patternName = patternName.Replace("_", "");
                string pinmapHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<PinMap schemaVersion=\"1.1\" xmlns=\"http://www.ni.com/TestStand/SemiconductorModule/PinMap.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n	<Instruments>\n		<NIDigitalPatternInstrument name=\"NI6570\" numberOfChannels=\"32\" />\n	</Instruments>\n	<Pins>\n";
                string pinmapMiddle = "\n	</Pins>\n	<PinGroups>\n	</PinGroups>\n	<Sites>\n		<Site siteNumber=\"0\" />\n	</Sites>\n	<Connections>\n";
                string pinmapFooter = "\n	</Connections>\n</PinMap>";
                #endregion

#if NIDEEPDEBUG
            Console.WriteLine("Compiling from .digipatsrc to .digipat");
#endif
                #region Create dummy pinmap to be used by compiler

                using (System.IO.StreamWriter pinmapFile = new StreamWriter(pinmapPath, false))
                {
                    pinmapFile.Write(pinmapHeader);
                    foreach (string pin in pins)
                    {
                        pinmapFile.WriteLine("<DUTPin name=\"" + pin + "\" />");
                    }
                    foreach (string pin in this.allDutPins)
                        if (!pins.Contains(pin.ToUpper())) { pinmapFile.WriteLine("<DUTPin name=\"" + pin.ToUpper() + "\" />"); }
                    pinmapFile.Write(pinmapMiddle);

                    int i = 1;
                    foreach (string pin in pins)
                    {
                        pinmapFile.WriteLine("<Connection pin=\"" + pin + "\" siteNumber=\"0\" instrument=\"NI6570\" channel=\"" + i++ + "\" />");
                    }
                    foreach (string pin in this.allDutPins)
                        if (!pins.Contains(pin.ToUpper())) { pinmapFile.WriteLine("<Connection pin=\"" + pin.ToUpper() + "\" siteNumber=\"0\" instrument=\"NI6570\" channel=\"" + i++ + "\" />"); }
                    pinmapFile.Write(pinmapFooter);
                }
                #endregion

                #region Run Compiler
                // Setup Process
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                // Run Digital Pattern Compiler located at compilerPath
                startInfo.FileName = compilerPath;
                // Pass in the pinmap, compiled digipat path, and text digipatsrc paths; escape spaces properly for cmd line execution
                startInfo.Arguments = " -pinmap " + pinmapPath.Replace(" ", @"^ ") + " -o " + digipatPath.Replace(" ", @"^ ") + " " + digipatsrcPath.Replace(" ", @"^ ");

                // Run Process
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

#if NIDEEPDEBUG
            Console.WriteLine("Compilation " + (process.ExitCode == 0 ? "Succeeded.  Loading Pattern to Instrument Memory." : "Failed"));
#endif
                // Delete Temporary Pinmap
                //File.Delete(pinmapPath);
                #endregion

                #region Load Pattern to Instrument Memory
                // Check if process exited without error and return status.
                if (process.ExitCode == 0)
                {
                    // Compilation completed without error, try loading pattern now.
                    try
                    {
                        if (load)
                        {
                            lock (lockObject)
                                DIGI.LoadPattern(digipatPath);
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    finally
                    {
                        try
                        {
                            File.Delete(pinmapPath);
                            File.Delete(pinmapPath.Replace(".pinmap", ""));
                        }
                        catch { }
                    }
                }
                else
                {
                    return false;
                }
                #endregion
            }

            /// <summary>
            /// NI Internal Function:  Compute the MD5 Hash of any file.
            /// </summary>
            /// <param name="filePath">The absolute path of the file for which to comput the MD5 Hash</param>
            /// <returns>The computed MD5 Hash String</returns>
            private string ComputeMD5Hash(string filePath)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    return BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(filePath))) + "_" + this.version.ToString();
                }
            }
            /// <summary>
            /// NI Internal Function:  Compute the MD5 Hash of any file.
            /// </summary>
            /// <param name="pattern">The List of String Arrays representing a Pattern in memory</param>
            /// <returns>The computed MD5 Hash String</returns>
            private string ComputeMD5Hash(List<string[]> pattern)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    string flattenedPattern = "";
                    foreach (var line in pattern.ToArray())
                    {
                        flattenedPattern += string.Join(",", line);
                    }
                    return BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(flattenedPattern))) + "_" + this.version.ToString();
                }
            }
            #endregion



            #region Avago SJC Specific Enums
            /// <summary>
            /// Used to specify which timeset is used for a specified pattern.
            /// Get the string representation using Timeset.MIPI.ToString("g");
            /// </summary>
            [Flags]
            public enum Timeset
            {
                NONE = 0,
                MIPI = 1 << 0,
                MIPI_HALF = 1 << 1,
                MIPI_SCLK_NRZ = 1 << 2,
                MIPI_RZ = 1 << 3,
                MIPI_RZ_HALF = 1 << 4,
                MIPI_RZ_QUAD = 1 << 5,
                MIPI_RZ_10MHZ = 1 << 6,
                EEPROM = 1 << 7,
                UNIO_EEPROM = 1 << 8,
                TEMPSENSE = 1 << 9,                               ////////migration
                MIPI_RFONOFF = 1 << 10,
                MIPI_HALF_RFONOFF = 1 << 11,
                MIPI_SCLK_NRZ_RFONOFF = 1 << 12,
                MIPI_RZ_EIGHTH = 1 << 13,
            };

            public enum TrigConfig
            {
                PXI_Backplane,
                Digital_Pin,
                Both,
                None
            }

            public enum PXI_Trig
            {
                None,
                PXI_Trig0,
                PXI_Trig1,
                PXI_Trig2,
                PXI_Trig3,
                PXI_Trig4,
                PXI_Trig5,
                PXI_Trig6,
                PXI_Trig7
            }

            /// <summary>
            /// NI Internal Enum:  Used to select which command for which to generate and RFFE packet
            /// </summary>
            private enum Command
            {
                EXTENDEDREGISTERWRITE,
                EXTENDEDREGISTERREAD,
                REGISTERWRITE,
                REGISTERREAD,
                MASKEDREGISTERWRITE,
                REGWRITE0,
                EXTENDEDREGISTERWRITELONG,
                EXTENDEDREGISTERREADLONG
            };

            private System.Version version = new System.Version(1, 0, 1215, 1);
            #endregion

            public override void SendTRIGVectors()
            {
                try
                {
                    DIGI.PatternControl.WriteSequencerFlag("seqflag3", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }

            private TriggerLine _triggerOut;

            public override TriggerLine TriggerOut
            {
                get
                {
                    return _triggerOut;
                }
                set
                {
                    string niTrigLine = TranslateNiTriggerLine(value);
                    //DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 5));
                    DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "PatternOpcodeEvent3", niTrigLine);
                    DIGI.PatternControl.Commit();
                    _triggerOut = value;
                }
            }

            private uint[] TrigOff = new uint[]
            {
                0u,  // This is hardcoded Slave Adddress need to fix. KH
				0u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                1u,
                1u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                0u,
                0u,
                1u,//
				1u,//parity
				1u,
                1u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u
            };

            private uint[] TrigOn = new uint[]
            {
                0u,   // This is hardcoded Slave Adddress need to fix. KH
				0u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                1u,
                1u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                0u,
                0u,
                1u,//
				1u,// parity
				0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u
            };

            private uint[] TrigMaskOn = new uint[]
            {
                1u,
                1u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                0u,
                0u,
                1u,
                1u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                1u,
                1u,
                1u,
                1u,
                1u,
                1u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u,
                0u
            };

            //         private uint[] SWREG01 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG10 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG09 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG90 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG06 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG60 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG05 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG50 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG08 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG80 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG03 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG30 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG02 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u
            //};

            //         private uint[] SWREG20 = new uint[]
            //{
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	1u,
            //	1u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u,
            //	0u
            //};

        }
    }

}