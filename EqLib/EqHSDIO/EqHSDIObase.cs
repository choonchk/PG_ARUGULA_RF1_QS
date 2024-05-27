using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace EqLib
{
    public partial class EqHSDIO
    {
        public enum UNIO_EEPROMType : int
        {
            Socket = 0,
            Loadboard = 7
        }
        private enum UNIO_EEPROMOpCode : int
        {
            Access = 0x0A,
            SecurityRegisterAccess = 0x0B,
            LockSecurityRegister = 0x02,
            ROMZoneRegisterAccess = 0x07,
            FreezeROMZoneState = 0x01,
            ManufacturerIDRead = 0x0C,
            StandardSpeedMode = 0x0D,
            HighSpeedMode = 0x0E
        }

        public enum PPMUVioOverrideString
        {
            RESET, VIOOFF, VIOON,
            RESET_TX, VIOOFF_TX, VIOON_TX,
            RESET_RX, VIOOFF_RX, VIOON_RX
        };

        public static bool usingMIPI = false;
        public static bool isVioTxPpmu; // Pinot added (Pcon)
        public static bool forceQCVectorRegen; //To control QC vectors regeneration through the TCF Main tab
        public static string ADJUST_BusIDCapTuningInHex = "0F";
        public const string dacQ2NamePrefix = "dacQ2";  // check and get rid of KH
        public const string dacQ1NamePrefix = "dacQ1";
        public const string Reset = "RESET", HiZ = "HiZ", RegIO = "regIO", Eeprom = "eeprom", VioOff = "viooff", VioOn = "vioon";
        public static string Sclk1ChanName = "", Sdata1ChanName = "", Vio1ChanName = "", Sclk2ChanName = "", Sdata2ChanName = "", Vio2ChanName = "", TrigChanName = ""; //ShieldChanName = "";
        public static string SkChanName = "SK", DiChanName = "DI", DoChanName = "DO";
        public static double TempSenseRaw;
        public static Dictionary<byte, double> Tempsenseraw = new Dictionary<byte, double>();
        //public static Dictionary<string, bool> datalogResults = new Dictionary<string, bool>();
        //public static string dutSlaveAddress;
        //public static int dutSlavePairIndex;
        public static int Num_Mipi_Bus;
        public static Dictionary<string, string> ConfigRegisterSettings = new Dictionary<string, string>();
        public static decimal I2CTempSensorDeviceAddress;
        public static double MIPIClockRate;
        public static double StrobePoint;

        // Added for HLS2.
        public static double StrobePointNRZHalf;
        public static double StrobePointRZ;
        public static double StrobePointRZHalf;

        public static EqHSDIObase Get(string VisaAlias, byte site)
        {
            EqHSDIObase hsdio;

            if (VisaAlias.Contains("9195"))
            {
                hsdio = new EqHSDIO.KeysightDSR();
            }
            else if (VisaAlias.Contains("6570"))
            {
                hsdio = new EqHSDIO.NI6570();
            }
            else
            {
                throw new Exception("HSDIO visa alias not recognized, model unknown");
            }

            hsdio.VisaAlias = VisaAlias;
            hsdio.Site = site;
            return hsdio;
        }

        //TODO Project specific. For future EqHSDIO redesign.
        public static EqHSDIObase Get(string VisaAlias, byte site, string project)
        {
            EqHSDIObase hsdio = null;

            switch (project)
            {
                case "JOKER":
                case "PINOT":
                    hsdio = new EqHSDIO.NI6570();
                    break;
            }

            hsdio.VisaAlias = VisaAlias;
            hsdio.Site = site;
            return hsdio;
        }

        public static List<string> CreateWaveformList(params string[] waveformNames)
        {
            List<string> MipiWaveformNames = new List<string>();

            foreach (string waveformName in waveformNames)
            {
                if (waveformName != null && waveformName != "" && waveformName != dacQ1NamePrefix && waveformName != dacQ2NamePrefix)
                {
                    string name = waveformName.Replace("_", "");
                    MipiWaveformNames.Add(name);
                }
            }

            return MipiWaveformNames;
        }

        //public static void selectorMipiPair(int Pair) 
        //{
        //    dutSlaveAddress = Eq.Site[0].HSDIO.Digital_Definitions["MIPI"+Pair+"_SLAVE_ADDR"];
        //    dutSlavePairIndex = Pair;
        //}

        private static string TranslateNiTriggerLine(TriggerLine trigLine)
        {
            switch (trigLine)
            {
                case TriggerLine.None:
                    return "";

                case TriggerLine.FrontPanel0:
                    return "PFI0";

                case TriggerLine.FrontPanel1:
                    return "PFI1";

                case TriggerLine.FrontPanel2:
                    return "PFI2";

                case TriggerLine.FrontPanel3:
                    return "PFI3";

                case TriggerLine.PxiTrig0:
                    return "PXI_Trig0";

                case TriggerLine.PxiTrig1:
                    return "PXI_Trig1";

                case TriggerLine.PxiTrig2:
                    return "PXI_Trig2";

                case TriggerLine.PxiTrig3:
                    return "PXI_Trig3";

                case TriggerLine.PxiTrig4:
                    return "PXI_Trig4";

                case TriggerLine.PxiTrig5:
                    return "PXI_Trig5";

                case TriggerLine.PxiTrig6:
                    return "PXI_Trig6";

                case TriggerLine.PxiTrig7:
                    return "PXI_Trig7";

                default:
                    throw new Exception("NI HSDIO trigger line not supported");
            }
        }

        public class DigitalOption
        {
            public bool EnableWrite0 { get; set; }
            public bool EnableRegWrite { get; set; }
            public List<int> RegWriteFrames { get; set; }

            public DigitalOption()
            {
                EnableWrite0 = true;
                EnableRegWrite = true;
                RegWriteFrames = new List<int>();
            }
        }

        public static EqHSDIO.DigitalOption digitalwriteoption = new EqHSDIO.DigitalOption();

        public abstract class EqHSDIObase
        {
            public bool FirstVector = false;
            public bool LastVector = false;
            public bool isRZ = true;
            public bool isShareBus = true;
            public string scriptFull = "";     
            public byte Site { get; set; }
            public string VisaAlias { get; set; }
            public Dictionary<string, string> PinNamesAndChans { get; set; }
            public List<Dictionary<string, string>> customMIPIlist { get; set; }

            public double BeforeDelay;
            public double AfterDelay;
            public int nBeforeCmd;
            public int dutSlavePairIndex;
            public string dutSlaveAddress;
            public static Dictionary<byte, int[]> siteTrigArray;

            public abstract bool Initialize();
            public abstract bool ReInitializeVIO(double violevel);
            public abstract string GetInstrumentInfo();
           // public abstract bool LoadVector(List<string> fullPaths, string nameInMemory, bool datalogResults);
            public abstract bool LoadVector(List<string> fullPaths, string nameInMemory);
            public abstract bool LoadVector_MipiHiZ();
            public abstract bool LoadVector_MipiReset();
            public abstract bool LoadVector_MipiVioOff();
            public abstract bool LoadVector_MipiVioOn();
            public abstract bool LoadVector_MipiRegIO();
            public abstract bool LoadVector_EEPROM();
            public abstract bool LoadVector_TEMPSENSEI2C(decimal TempSensorAddress = 3);
            public abstract bool SendVector(string nameInMemory);
            public abstract bool Burn(string data_hex, bool invertData = false, int efuseDataByteNum = 0, string efuseCtlAddress = "C0");
            public abstract bool SendVectorOTP(string TargetData, string CurrentData = "00", bool isEfuseBurn = false);
            public abstract void SendNextVectors(bool firstTest, List<string> MipiWaveformNames);
            public abstract void SendMipiCommands(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands, eMipiTestType _eMipiTestType = eMipiTestType.Write);
            public abstract void AddVectorsToScript(List<string> namesInMemory, bool finalizeScript);
            public abstract int GetNumExecErrors(string nameInMemory);
            public abstract void RegWriteMultiple(List<MipiSyntaxParser.ClsMIPIFrame> MipiCommands);
            public abstract void RegWrite(string registerAddress_hex, string data_hex, bool sendTrigger = false);
            public abstract string RegRead(string registerAddress_hex, bool writeHalf = false);
            public abstract void EepromWrite(string dataWrite);
            public abstract string EepromRead();
            public abstract bool LoadVector_UNIO_EEPROM();
            public abstract bool UNIO_EEPROMWriteID(UNIO_EEPROMType device, string dataWrite, int bus_no = 1);
            public abstract bool UNIO_EEPROMWriteCounter(UNIO_EEPROMType device, uint count, int bus_no = 1);
            public abstract bool UNIO_EEPROMFreeze(UNIO_EEPROMType device, int bus_no = 1);
            public abstract string UNIO_EEPROMReadID(UNIO_EEPROMType device, int bus_no = 1);
            public abstract uint UNIO_EEPROMReadCounter(UNIO_EEPROMType device, int bus_no = 1);
            public abstract string UNIO_EEPROMReadSerialNumber(UNIO_EEPROMType device, int bus_no = 1);
            public abstract string UNIO_EEPROMReadMID(UNIO_EEPROMType device, int bus_no = 1);
            public abstract double I2CTEMPSENSERead();
            public abstract void Close();
            public abstract void SendTRIGVectors();
            public abstract void shmoo(string nameInMemory);

            //keng shan Added
            //           public abstract bool LoadVector_RFOnOffTest(bool isNRZ = false);
            public abstract bool LoadVector_RFOnOffTest(bool isNRZ = false); // Trigger
            public abstract bool LoadVector_RFOnOffTestRx(bool isNRZ = false); //Rx Trigger
            public abstract bool LoadVector_RFOnOffSwitchTest(bool isNRZ = false);
            public abstract bool LoadVector_RFOnOffSwitchTest_WithPreMipi(bool isNRZ = false); //Tx Trigger: RFOnOff + SwitchingTime
            public abstract bool LoadVector_RFOnOffSwitchTest_With3TxPreMipi(bool isNRZ = false);    //Tx Trigger: RFOnOff + SwitchingTime, 3TXPreMipi, For TX Band-To-Band
            public abstract bool LoadVector_RFOnOffSwitchTestRx(bool isNRZ = false);   //Rx Trigger: RFOnOff + SwitchingTime
            public abstract bool LoadVector_RFOnOffSwitchTestRx_WithPreMipi(bool isNRZ = false);   //Rx Trigger: RFOnOff + SwitchingTime, 1RXPreMipi, for LNA Output Switching
            public abstract bool LoadVector_RFOnOffSwitchTestRx_With1Tx2RxPreMipi(bool isNRZ = false);    //Rx Trigger: RFOnOff + SwitchingTime, 1TXPreMipi, 2RXPreMipi, For LNA switching time (same output) G0 only
            public abstract bool LoadVector_RFOnOffSwitchTest2(bool isNRZ = false);     //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2
            public abstract bool LoadVector_RFOnOffSwitchTest2_WithPreMipi(bool isNRZ = false);    //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2, 1TXPreMipi, For CPL
            public abstract bool LoadVector_RFOnOffSwitchTest2_With1Tx2RxPreMipi(bool isNRZ = false);    //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2, 1TXPreMipi, 2RXPreMipi, For TERM:TX:RX:TX  
            public abstract bool LoadVector_RFOnOffSwitchTest2Rx(bool isNRZ = false);  //Rx Trigger: RFOnOff + SwitchingTime + SwitchingTime2
            public abstract bool SendRFOnOffTestVector(bool RxMode, string[] SwTimeCustomArry);
            public abstract void SetSourceWaveformArry(string customMIPIlist);

            public abstract TriggerLine TriggerOut { get; set; }


            public Dictionary<string, string> Digital_Definitions;
            public Dictionary<string, uint[]> Digital_Mipi_Trig;

            public string[] Get_Specific_Key(string Value, string MatchKey = "")
            {
                if (MatchKey != "")
                {
                    string[] arr = Digital_Definitions.Where(c => c.Value == Value).ToDictionary(c => c.Key, c => c.Value).Keys.ToArray();
                    foreach (string item in arr)
                    {
                        if (item.Contains(MatchKey))
                            return new string[] { item };
                    }
                }
                else
                {
                    string CloestKey = Value;
                    string[] strArray = Digital_Definitions.Where(x => x.Key.Contains(CloestKey)).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();
                    return strArray;
                }

                return null;
            }

            public string Get_Digital_Definition(string key, string initVal = null)
            {
                if (Digital_Definitions.ContainsKey(key.ToUpper()))
                    return Digital_Definitions[key.ToUpper()].ToUpper();
                else if (initVal == null)
                {
                    MessageBox.Show("Warning: The Register definition for: " + key + " does not exist in OTP_Registers_Part_Specific found in Digital_Definitions_Part_Specific.xml", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return string.Empty;
                }
                else
                    return initVal.ToUpper();
            }

            public bool Initialize(Dictionary<string, EqDC.iEqDC> DcResources, Dictionary<byte, int[]> TrigArray)
            {
                try
                {
                    siteTrigArray = TrigArray;
                    Num_Mipi_Bus = Convert.ToUInt16(Get_Digital_Definition("NUM_MIPI_BUS"));
                    PinNamesAndChans = new Dictionary<string, string>();

                    digitalwriteoption.EnableWrite0 = Get_Digital_Definition("EANBLEWRITE0", "TRUE").CIvEquals("TRUE");
                    digitalwriteoption.EnableRegWrite = Get_Digital_Definition("EANBLEREGWRITE", "TRUE").CIvEquals("TRUE");

                    List<int> _regFrames = new List<int>();
                    foreach (var _hVal in Get_Digital_Definition("RegWriteFrames", "1C").SplitToArray(','))
                    {
                        if (int.TryParse(_hVal.Trim(), System.Globalization.NumberStyles.HexNumber, null, out int result) && result.IsInRange(1, 0x1f))
                            _regFrames.Add(result);
                    }

                    digitalwriteoption.RegWriteFrames = _regFrames;

                    Sclk1ChanName = Get_Digital_Definition("SCLK1_VEC_NAME");
                    Sdata1ChanName = Get_Digital_Definition("SDATA1_VEC_NAME");
                    Vio1ChanName = Get_Digital_Definition("VIO1_VEC_NAME");

                    if (Num_Mipi_Bus == 2)
                    {
                        Sclk2ChanName = Get_Digital_Definition("SCLK2_VEC_NAME");
                        Sdata2ChanName = Get_Digital_Definition("SDATA2_VEC_NAME");
                        Vio2ChanName = Get_Digital_Definition("VIO2_VEC_NAME");
                    }



                    // retrieve MIPI pin Visa Alias and Channel Number from our SMU Resource dictionary
                    foreach (string pinName in DcResources.Keys)
                    {
                        if (pinName.ToUpper() == Sdata1ChanName || pinName.ToUpper() == Sclk1ChanName || pinName.ToUpper() == Vio1ChanName || pinName.ToUpper() == Sdata2ChanName || pinName.ToUpper() == Sclk2ChanName || pinName.ToUpper() == Vio2ChanName)
                        {
                            PinNamesAndChans.Add(pinName.ToUpper(), DcResources[pinName].ChanNumber);
                        }
                    }

                    if (!(PinNamesAndChans.ContainsKey(Sclk1ChanName) & PinNamesAndChans.ContainsKey(Sdata1ChanName) & PinNamesAndChans.ContainsKey(Vio1ChanName)))
                    {
                        throw new Exception("Did not find one of the following MIPI channel names in TCF:\n" + Sclk1ChanName + ",  " + Sdata1ChanName + ",  " + Vio1ChanName);
                    }

                    Initialize();

                    LoadVector_MipiReset();
                    LoadVector_MipiVioOff();
                    LoadVector_MipiVioOn();
                    LoadVector_MipiHiZ();
                    LoadVector_MipiRegIO();
                    LoadVector_EEPROM();  // Obsoleted in Pinot
                    LoadVector_TEMPSENSEI2C(0);
                    LoadVector_UNIO_EEPROM();
                    //LoadVector_RFOnOffTest();  
                    //LoadVector_RFOnOffTestRx(); //Rx Trigger

                    //this.LoadVector_RFOnOffSwitchTest();
                    //LoadVector_RFOnOffSwitchTest_WithPreMipi(); //Tx Trigger: RFOnOff + SwitchingTime
                    ////LoadVector_RFOnOffSwitchTest_With3TxPreMipi();    //Tx Trigger: RFOnOff + SwitchingTime, 3TXPreMipi, For TX Band-To-Band

                    //LoadVector_RFOnOffSwitchTestRx();   //Rx Trigger: RFOnOff + SwitchingTime
                    //LoadVector_RFOnOffSwitchTestRx_WithPreMipi();   //Rx Trigger: RFOnOff + SwitchingTime, 1RXPreMipi, for LNA Output Switching
                    ////LoadVector_RFOnOffSwitchTestRx_With1Tx2RxPreMipi();    //Rx Trigger: RFOnOff + SwitchingTime, 1TXPreMipi, 2RXPreMipi, For LNA switching time (same output) G0 only

                    //LoadVector_RFOnOffSwitchTest2();     //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2
                    //LoadVector_RFOnOffSwitchTest2_WithPreMipi();    //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2, 1TXPreMipi, For CPL
                    ////LoadVector_RFOnOffSwitchTest2_With1Tx2RxPreMipi();    //Tx Trigger: RFOnOff + SwitchingTime + SwitchingTime2, 1TXPreMipi, 2RXPreMipi, For TERM:TX:RX:TX  

                    //LoadVector_RFOnOffSwitchTest2Rx();  //Rx Trigger: RFOnOff + SwitchingTime + SwitchingTime2


                    return true;
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString(), "HSDIO MIPI");
                }

                return false;
            }
            
            //public bool LoadVector_PowerMode(string band, string powerMode, string fullPath)
            //{
            //    return LoadVector(new List<string> { fullPath }, powerMode + band);
            //}

            //public bool LoadVector_TXRXport(string band, string rxPort, string fullPath)
            //{
            //    return LoadVector(new List<string> { fullPath }, band + rxPort);
            //}

            //public bool LoadVector_Coupler(string direction, string attenuation, string fullPath)
            //{
            //    return LoadVector(new List<string> { fullPath }, direction + attenuation);
            //}

            //public bool LoadVector_PowerMode(string band, string powerMode, string fullPath1, string fullPath2)
            //{
            //    return LoadVector(new List<string> { fullPath1, fullPath2 }, powerMode + band);
            //}

            //public bool LoadVector_PowerMode(string band, string powerMode, string fullPath1, string fullPath2, string fullPath3)
            //{
            //    return LoadVector(new List<string> { fullPath1, fullPath2, fullPath3 }, powerMode + band);
            //}

            //public bool LoadVector_PowerMode(string band, string powerMode, string fullPath1, string fullPath2, string fullPath3, string fullPath4)
            //{
            //    return LoadVector(new List<string> { fullPath1, fullPath2, fullPath3, fullPath4 }, powerMode + band);
            //}

            //public bool LoadVector_PowerMode(string band, string powerMode, string fullPath1, string fullPath2, string fullPath3, string fullPath4, string fullPath5)
            //{
            //    return LoadVector(new List<string> { fullPath1, fullPath2, fullPath3, fullPath4, fullPath5 }, powerMode + band);
            //}

            //public bool LoadVector(List<string> fullPath, string nameInMemory)
            //{
            //    return LoadVector(fullPath, nameInMemory, false);  //  default is not to datalog number of bit errors
            //}

            //public bool LoadVector(string fullPath, string nameInMemory)
            //{
            //    return LoadVector(new List<string> { fullPath }, nameInMemory, false);  //  default is to datalog number of bit errors
            //}

            //public bool LoadVector_withDatalog(string fullPath, string nameInMemory)
            //{
            //    return LoadVector(new List<string> { fullPath }, nameInMemory, true);  //  default is to datalog number of bit errors
            //}

            //public bool LoadVector_withDatalog(string nameInMemory, string fullPath)
            //{
            //    return LoadVector(new List<string> { fullPath }, nameInMemory, true);  //  default is to datalog number of bit errors
            //}
            
            public bool LoadVector(string nameInMemory, string fullPath)
            {
                return LoadVector(new List<string> { fullPath }, nameInMemory); // All the other LoadVector wrappers are obsolete with new MIPICommand syntax
            }
 
            public bool IsMipiChannel(string pinName)
            {
                bool isMipiCh = false;

                isMipiCh = PinNamesAndChans.ContainsKey(pinName);
                if (EqHSDIO.isVioTxPpmu & pinName.ToUpper().Contains("VIO"))
                    isMipiCh = false;

                return isMipiCh;
            }

            public void selectorMipiPair(int Pair, byte Site)
            {
                dutSlaveAddress = Eq.Site[Site].HSDIO.Digital_Definitions["MIPI" + Pair + "_SLAVE_ADDR"];
                dutSlavePairIndex = Pair;
            }
        }
    }

    public static class ForString
    {
        public static bool CIvEquals(this string value, string what)
        {
            return string.Equals(value, what, StringComparison.OrdinalIgnoreCase);
        }

        public static string[] SplitToArray(this string value, params char[] chars)
        {
            return value.Split(chars, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public static class ForNumeric
    {
        public static bool IsInRange(this int value, int min, int max)
        {
            return (min <= value && value <= max);
        }
    }


    public enum eMipiTestType
    {
        Write,
        Read,
        WriteRead,
        Timing,
        OTPburn,
        OTPread,
        RfBurst
    }
    
    public static class MipiSyntaxParser
    {
        public static List<ClsMIPIFrame> CreateListOfMipiFrames(string mipiCode)
        {
            List<ClsMIPIFrame> _mipiFramesToSend = new List<ClsMIPIFrame>();

            if (string.IsNullOrWhiteSpace(mipiCode)) return _mipiFramesToSend;

            Regex expr = new Regex(@"(.*?)\((.*?)\)");

            var matches = expr.Matches(mipiCode);

            string currentSlaveAddress = "";
            int pair = 0;
            foreach (Match match in matches)
            {
                string mipiFrame_found = match.Groups[0].Value;
                string slaveAddress_found = match.Groups[1].Value;
                string mipiFrameContents_found = match.Groups[2].Value;

                slaveAddress_found = ConvertAllValueTypesToHex(slaveAddress_found);

                if (!string.IsNullOrEmpty(slaveAddress_found))
                {
                    currentSlaveAddress = slaveAddress_found;
                    pair++;
                }

                ClsMIPIFrame stdMipiFrame = ParseStdMipiFrame(mipiFrameContents_found, currentSlaveAddress, pair);

                if (stdMipiFrame != null)
                {
                    _mipiFramesToSend.Add(stdMipiFrame);
                    continue;
                }

                ClsMIPIFrame delayFrame = ParseDelayFrame(mipiFrameContents_found);

                if (delayFrame != null)
                {
                    _mipiFramesToSend.Add(delayFrame);
                    continue;
                }
            }

            return _mipiFramesToSend;
        }

        public class ClsMIPIFrame
        {
            public int Pair;
            public string SlaveAddress_hex, Register_hex, Data_hex;
            public readonly int Delay_ms;
            public bool IsMaskedWrite;
            public bool IsValidFrame
            {
                get
                {
                    long output;
                    bool valid = true;

                    valid &= long.TryParse(SlaveAddress_hex, System.Globalization.NumberStyles.HexNumber, null, out output);
                    valid &= long.TryParse(Register_hex, System.Globalization.NumberStyles.HexNumber, null, out output);
                    valid &= long.TryParse(Data_hex, System.Globalization.NumberStyles.HexNumber, null, out output);

                    return valid;
                }
            }

            public ClsMIPIFrame(string SlaveAddress_hex, string Register_hex, string Data_hex, int Pair = 0, bool IsMaskedWrite = false)
            {
                this.SlaveAddress_hex = SlaveAddress_hex;
                this.Register_hex = Register_hex;
                this.Data_hex = Data_hex;
                this.Pair = Pair;
                this.IsMaskedWrite = IsMaskedWrite;
            }

            public ClsMIPIFrame(int Delay_ms)
            {
                this.Delay_ms = Delay_ms;
            }
        }

        private static ClsMIPIFrame ParseStdMipiFrame(string mipiFrameContents_found, string currentSlaveAddress, int Pair = 0)
        {
            if (string.IsNullOrEmpty(currentSlaveAddress))
            {
                return null;
            }

            Regex exprStdMipiFrame = new Regex(@"(.*),(.*)");

            Match matchStdMipiFrame = exprStdMipiFrame.Match(mipiFrameContents_found);

            if (matchStdMipiFrame.Success)
            {
                string register_found = matchStdMipiFrame.Groups[1].Value;
                string data_found = matchStdMipiFrame.Groups[2].Value;
                bool IsMaskedwrite = false;

                if (register_found.Contains("@"))
                {
                    IsMaskedwrite = true;
                    register_found = register_found.Replace("@", "");
                }

                register_found = ConvertAllValueTypesToHex(register_found);
                data_found = ConvertAllValueTypesToHex(data_found);
                
                return new ClsMIPIFrame(currentSlaveAddress, register_found, data_found, Pair, IsMaskedwrite);
            }
            else
            {
                return null;
            }
        }

        private static ClsMIPIFrame ParseDelayFrame(string mipiFrameContents_found)
        {
            Regex exprDelay = new Regex(@"\+\s*(\d+)");

            Match matchDelay = exprDelay.Match(mipiFrameContents_found);

            if (matchDelay.Success)
            {
                int delay_found = Convert.ToInt32(matchDelay.Groups[1].Value);
                return new ClsMIPIFrame(delay_found);
            }
            else
            {
                return null;
            }
        }

        private static string ConvertAllValueTypesToHex(string val)
        {
            string val_hex;

            if (string.IsNullOrWhiteSpace(val))
            {
                return "";
            }

            else if (BeginsWithHexPrefix(val))
            {
                val_hex = TrimHex(val);
            }

            else if (BeginsWithBinaryPrefix(val))
            {
                val_hex = BinaryToHex(val);
            }

            else
            {
                val_hex = DecimalToHex(val);
            }

            return val_hex.ToUpper();
        }

        private static bool BeginsWithHexPrefix(string val)
        {
            return val.Trim().ToUpper().StartsWith("0X");
        }

        private static bool BeginsWithBinaryPrefix(string val)
        {
            return val.Trim().ToUpper().StartsWith("0B");
        }

        private static string TrimHex(string val_hex)
        {
            var val_hex_trimmed = val_hex.ToUpper().Replace("0X", "").Trim();

            long val_dec;
            bool isHex = long.TryParse(val_hex_trimmed, System.Globalization.NumberStyles.HexNumber, null, out val_dec);

            if (isHex)
            {
                return val_hex_trimmed;
            }
            else
            {
                return val_hex;
            }
        }

        private static string BinaryToHex(string val_bin)
        {
            try
            {
                var val_bin_trimmed = val_bin.ToUpper().Replace("0B", "").Trim();

                return Convert.ToInt32(val_bin_trimmed, 2).ToString("X");
            }
            catch
            {
                return val_bin;
            }
        }

        private static string DecimalToHex(string val_dec)
        {
            int val_dec_int = -1;

            if (int.TryParse(val_dec, out val_dec_int))
            {
                return Convert.ToString(val_dec_int, 16);
            }
            else
            {
                return val_dec;
            }
        }

        /// <summary>
        /// Only a-e are valid for MIPI hex conversion.
        /// </summary>
        private static Dictionary<string, string> m_hexConversion =
            new Dictionary<string, string>
            {
                { "10", "A" },
                { "11", "B" },
                { "12", "C" },
                { "13", "D" },
                { "14", "E" },
                { "A", "A" },
                { "B", "B" },
                { "C", "C" },
                { "D", "D" },
                { "E", "E" },
                { "a", "A" },
                { "b", "B" },
                { "c", "C" },
                { "d", "D" },
                { "e", "E" },
            };
    }
}