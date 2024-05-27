using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR.Switch
{
    #region "Enumeration Declaration"
    public enum CardPair_FunctionConfig
    {
        Wire1 = 1,
        Wire2,
        Biwire2,
        Wire4
    }
    public enum ScanConfig_Source
    {
        BUS,
        EXTernal,
        IMMediate,
        TIMer,
        MIX,
        HOLD
    }
    public enum ScanConfig_ArmCount
    {
        MIN,
        MAX,
        INFinity
    }
    public enum ScanConfig_Timer
    {
        MIN,
        MAX
    }
    public enum Output
    {
        OFF,
        ON
    }
    public enum Digital_ConfigMode
    {
        Static_Mode1 = 1,
        Static_Mode2,
        Read_or_Write_Strobe,
        Read_and_Write_Strobe,
        Full_Handshake
    }
    public enum Polarity
    {
        Neg,
        Pos
    }
    public enum State
    {
        Off,
        On
    }
    public enum SystemMode
    {
        SCPI,
        HP3488
    }
    #endregion

    #region "Structure"
    public struct Digital_Data
    {
        public int digits;
        public int length;
        public string blockdata;
    }
    #endregion
    public class cSwitch3499 : iCommonFunction
    {
        public static string ClassName = "Switch 3499 Class";
        private string IOAddress;
        private FormattedIO488 ioSwitch;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();

        #region "Class Initialization"
        public cSwitchCmd Switch;
        public cPlugIn PlugIn;
        public cScanList ScanList;
        public cScanning ScanCommand;
        public cStateStorage StateStorage;
        public cDigital Digital;
        public cSystem System;
        public cSysMode SysMode;
        public cRS232 RS232;

        public cCommonFunction BasicCommand;  // Basic Command for General Equipment
        public void Init(FormattedIO488 IOInit)
        {

            BasicCommand = new cCommonFunction(IOInit);
            Switch = new cSwitchCmd(IOInit);
            PlugIn = new cPlugIn(IOInit);
            ScanList = new cScanList(IOInit);
            ScanCommand = new cScanning(IOInit);
            StateStorage = new cStateStorage(IOInit);
            Digital = new cDigital(IOInit);
            System = new cSystem(IOInit);
            SysMode = new cSysMode(IOInit);
            RS232 = new cRS232(IOInit);
        }

        #endregion

        #region "Conversion Function"
        public static CardPair_FunctionConfig ConvStr2CardPair_FunctionConfig(int Config)
        {

            switch (Config)
            {
                case 1:
                    {
                        return (CardPair_FunctionConfig.Wire1);
                    }
                case 2:
                    {
                        return (CardPair_FunctionConfig.Wire2);
                    }
                case 3:
                    {
                        return (CardPair_FunctionConfig.Biwire2);
                    }
                case 4:
                    {
                        return (CardPair_FunctionConfig.Wire4);
                    }
                default:
                    {
                        common.DisplayError(ClassName, "Error in ConvStr2CardPair_FunctionConfig()", "Error in converting Integer " + Config.ToString() + " to CardPair Function Configuration");
                        return (CardPair_FunctionConfig.Wire1);
                    }
            }
        }
        public static ScanConfig_Source ConvStr2ScanConfig_Source(string sSource)
        {
            switch (sSource.ToUpper())
            {
                case "BUS":
                    {
                        return (ScanConfig_Source.BUS);
                    }
                case "EXT":
                case "EXTERNAL":
                    {
                        return (ScanConfig_Source.EXTernal);
                    }
                case "IMM":
                case "IMMEDIATE":
                    {
                        return (ScanConfig_Source.IMMediate);
                    }
                case "TIM":
                case "TIMER":
                    {
                        return (ScanConfig_Source.TIMer);
                    }
                case "MIX":
                    {
                        return (ScanConfig_Source.MIX);
                    }
                case "HOLD":
                    {
                        return (ScanConfig_Source.HOLD);
                    }
                default:
                    {
                        common.DisplayError(ClassName, "Error converting string to Source Scan Config", "Unable to convert string : " + sSource);
                        return (ScanConfig_Source.IMMediate);
                    }
            }
        }
        public static Output ConvStr2Output(string sOutput)
        {
            switch (sOutput.Trim())
            {
                case "0":
                    return (Output.OFF);
                case "1":
                    return (Output.ON);
                default:
                    common.DisplayError(ClassName, "Error converting string to Output", "Unable to Convert String : " + sOutput);
                    return (Output.OFF);
            }
        }
        public static Digital_ConfigMode ConvStr2Digital_Mode(string dMode)
        {
            switch (dMode)
            {
                case "1":
                    return (Digital_ConfigMode.Static_Mode1);
                case "2":
                    return (Digital_ConfigMode.Static_Mode2);
                case "3":
                    return (Digital_ConfigMode.Read_or_Write_Strobe);
                case "4":
                    return (Digital_ConfigMode.Read_and_Write_Strobe);
                case "5":
                    return (Digital_ConfigMode.Full_Handshake);
                default:
                    common.DisplayError(ClassName, "Error Converting String to Digital Mode", "Unable to convert String : " + dMode);
                    return (Digital_ConfigMode.Static_Mode1);
            }
        }
        public static Polarity ConvStr2Polarity(string Polar)
        {
            switch (Polar.ToUpper())
            {
                case "0":
                case "NEG":
                    return (Polarity.Neg);
                case "1":
                case "POS":
                    return (Polarity.Pos);
                default:
                    common.DisplayError(ClassName, "Error in Converting String to Polarity", "Unable to convert string : " + Polar);
                    return (Polarity.Pos);
            }
        }
        public static State ConvStr2State(string sState)
        {
            switch (sState.ToUpper())
            {
                case "0":
                case "OFF":
                    return (State.Off);
                case "1":
                case "ON":
                    return (State.On);
                default:
                    common.DisplayError(ClassName, "Error in converting string 2 State", "Unable to convert string : " + sState);
                    return (State.Off);
            }
        }
        public static SystemMode ConvStr2SysMode(string sMode)
        {
            switch (sMode.ToUpper())
            {
                case "0":
                case "SCPI":
                    return (SystemMode.SCPI);
                case "1":
                case "HP3488":
                case "HP3488A":
                    return (SystemMode.HP3488);
                default:
                    common.DisplayError(ClassName, "Error in converting String to SysMode", "Unable to convert string : " + sMode);
                    return (SystemMode.SCPI);
            }
        }
        #endregion
        public string Address
        {
            get
            {
                return IOAddress;
            }
            set
            {
                IOAddress = value;
            }
        }
        public FormattedIO488 parseIO
        {
            get
            {
                return ioSwitch;
            }
            set
            {
                ioSwitch = parseIO;
            }
        }
        public void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    ioSwitch = new FormattedIO488();
                    ioSwitch.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioSwitch.IO = null;
                    return;
                }
                Init(ioSwitch);
            }
        }
        public void CloseIO()
        {
            ioSwitch.IO.Close();
        }
        
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  18/10/2010       KKL             VISA Driver for Switch 3488/3499

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return ("Switch Class Version = v" + VersionStr);
        }

        #region "Class Functional Codes"
        public class cSwitchCmd : cCommonFunction
        {
            public cSwitchCmd(FormattedIO488 parse) : base(parse) { }

            public void Close(int ChannelNumber)
            {
                SendCommand("CLOSE (@" + ChannelNumber.ToString() + ")");
            }
            public void Close(string ChannelNumber)
            {
                SendCommand("CLOSE (@" + ChannelNumber.Trim() + ")");
            }
            public string GetClose(int ChannelNumber)
            {
                return (ReadCommand("CLOSE? (@" + ChannelNumber.ToString() + ")"));
            }
            public string GetClose(string ChannelNumber)
            {
                return (ReadCommand("CLOSE? (@" + ChannelNumber.ToString() + ")"));
            }
            public string GetCloseState()
            {
                return (ReadCommand("CLOS:STAT?"));
            }
            public void Open(int ChannelNumber)
            {
                SendCommand("OPEN (@" + ChannelNumber.ToString() + ")");
            }
            public void Open(string ChannelNumber)
            {
                SendCommand("OPEN (@" + ChannelNumber.Trim() + ")");
            }
            public void OpenAll()
            {
                SendCommand("OPEN ALL");
            }
            public string GetOpen(int ChannelNumber)
            {
                return (ReadCommand("OPEN? (@" + ChannelNumber.ToString() + ")"));
            }
            public string GetOpen(string ChannelNumber)
            {
                return (ReadCommand("OPEN? (@" + ChannelNumber.ToString() + ")"));
            }
        }
        public class cPlugIn : cCommonFunction
        {
            public cPlugIn(FormattedIO488 parse) : base(parse) { }

            public void CardPair(int Slot1, int Slot2)
            {
                SendCommand("ROUT:CPA " + Slot1.ToString() + "," + Slot2.ToString());
            }
            public void CardPair(int Slot1)
            {
                SendCommand("ROUT:CPA " + Slot1.ToString());
            }
            public void CardPair_Cancel()
            {
                SendCommand("ROUT:CPA -1");
            }
            public string CardPair()
            {
                return (ReadCommand("ROUT:CPA?"));
            }
            public void SetFunctionConfig(int Slot, CardPair_FunctionConfig Config)
            {
                SendCommand("ROUT:FUNC " + Slot.ToString() + "," + Config.GetHashCode().ToString());
            }
            public CardPair_FunctionConfig FunctionConfig(int Slot)
            {
                CardPair_FunctionConfig Config = ConvStr2CardPair_FunctionConfig(Convert.ToInt16(ReadCommand("ROUT:FUNC? " + Slot.ToString())));
                return (Config);
            }
            public string GetFunctionConfig(int Slot)
            {
                return (ReadCommand("ROUT:FUNC? " + Slot.ToString()));
            }
        }
        public class cScanList : cCommonFunction
        {
            public cScanList(FormattedIO488 parse) : base(parse) { }

            public void ScanList(string List)
            {
                SendCommand("SCAN (@" + List + ")");
            }
            public void ScanList_Single(string Channel)
            {
                SendCommand("SCAN (@" + Channel.Trim() + ")");
            }
            public void ScanList_Single(int Channel)
            {
                SendCommand("SCAN (@" + Channel.ToString() + ")");
            }
            public void ScanList_Multiple(string List)
            {
                SendCommand("SCAN (@" + List.ToString() + ")");
            }
            public void ScanList_Sequential(string StartChannel, string EndChannel)
            {
                SendCommand("SCAN (@" + StartChannel.Trim() + ":" + EndChannel.Trim() + ")");
            }
            public void ScanList_Sequential(int StartChannel, int EndChannel)
            {
                SendCommand("SCAN (@" + StartChannel.ToString() + ":" + EndChannel.ToString() + ")");
            }
            public string ScanList()
            {
                return (ReadCommand("SCAN?"));
            }
            public void Clear()
            {
                SendCommand("SCAN CLE");
            }
            public int ScanListSize()
            {
                return (Convert.ToInt16(ReadCommand("SCAN:SIZE?")));
            }
        }
        public class cScanning : cCommonFunction
        {
            public cScanning(FormattedIO488 parse) : base(parse) { }

            public void Initiate()
            {
                SendCommand("INIT");
            }
            public void Abort()
            {
                SendCommand("ABOR");
            }
            public void Trigger()
            {
                SendCommand("TRIG");
            }
        }
        public class cScanConfig : cCommonFunction
        {
            public cArm Arm;
            public cTrigger Trigger;
            public cRoute Route;
            public cConfigure Configure;

            public cScanConfig(FormattedIO488 parse)
                : base(parse)
            {
                Arm = new cArm(parse);
                Trigger = new cTrigger(parse);
                Route = new cRoute(parse);
                Configure = new cConfigure(parse);
            }

            public class cArm : cCommonFunction
            {
                public cArm(FormattedIO488 parse) : base(parse) { }

                public void SetArm(string Config)
                {
                    SendCommand("ARM:SOUR " + Config.Trim());
                }
                public void SetArm(ScanConfig_Source Config)
                {
                    SendCommand("ARM:SOUR " + Config.ToString());
                }
                //public string SetArm()
                //{
                //    return (ReadCommand("ARM:SOUR?"));
                //}
                public ScanConfig_Source SetArm()
                {
                    string rslt;
                    rslt = ReadCommand("ARM:SOUR?");
                    return (ConvStr2ScanConfig_Source(rslt));
                }
                public void SetCount(int Count)
                {
                    SendCommand("ARM:COUN " + Count.ToString());
                }
                public void SetCount(ScanConfig_ArmCount Count)
                {
                    SendCommand("ARM:COUN " + Count.ToString());
                }
                public int GetCount()
                {
                    return (int.Parse(ReadCommand("ARM:COUN?")));
                }
                public int GetCount(ScanConfig_ArmCount Count)
                {
                    return (int.Parse(ReadCommand("ARM:COUN? " + Count.ToString())));
                }
                public void SetTimer(double Timer)
                {
                    SendCommand("ARM:TIM " + Timer.ToString());
                }
                public void SetTimer(ScanConfig_Timer Timer)
                {
                    SendCommand("ARM:TIM " + Timer.ToString());
                }
                public double GetTimer()
                {
                    return (double.Parse(ReadCommand("ARM:TIM?")));
                }
                public double GetTimer(ScanConfig_Timer Timer)
                {
                    return (double.Parse(ReadCommand("ARM:TIM? " + Timer.ToString())));
                }
            }
            public class cTrigger : cCommonFunction
            {
                public cTrigger(FormattedIO488 parse) : base(parse) { }

                public void SetTrigger(string Trig)
                {
                    SendCommand("TRIG:SOUR " + Trig.Trim());
                }
                public void SetTrigger(ScanConfig_Source Trig)
                {
                    SendCommand("TRIG:SOUR " + Trig.ToString());
                }
                public string vTrigger()
                {
                    return (ReadCommand("TRIG:SOUR?"));
                }
                public ScanConfig_Source SetTrigger()
                {
                    string rslt = ReadCommand("TRIG:SOUR?");
                    return (ConvStr2ScanConfig_Source(rslt));
                }
                public void SetTimer(double Timer)
                {
                    SendCommand("ARM:TIM " + Timer.ToString());
                }
                public void SetTimer(ScanConfig_Timer Timer)
                {
                    SendCommand("ARM:TIM " + Timer.ToString());
                }
                public double GetTimer()
                {
                    return (double.Parse(ReadCommand("ARM:TIM?")));
                }
                public double GetTimer(ScanConfig_Timer Timer)
                {
                    return (double.Parse(ReadCommand("ARM:TIM? " + Timer.ToString())));
                }
            }
            public class cRoute : cCommonFunction
            {
                public cRoute(FormattedIO488 parse) : base(parse) { }

                public void Delay(float Delay, string ChannelList)
                {
                    SendCommand("DEL " + Delay.ToString() + ",(@" + ChannelList.Trim() + ")");
                }
                public void Delay(float Delay, int ChannelList)
                {
                    SendCommand("DEL " + Delay.ToString() + ",(@" + ChannelList.ToString() + ")");
                }
                public void Delay(float Delay, int StartChannel, int EndChannel)
                {
                    SendCommand("DEL " + Delay.ToString() + ",(@" + StartChannel.ToString() + ":" + EndChannel.ToString() + ")");
                }
                public void DelayAll(float Delay)
                {
                    SendCommand("DEL " + Delay.ToString() + ",ALL");
                }
                public string Delay(string ChannelList)
                {
                    return (ReadCommand("DEL? (@" + ChannelList.Trim() + ")"));
                }
                public float Delay(int ChannelList)
                {
                    string rlst = ReadCommand("DEL? (@" + ChannelList.ToString() + ")");
                    return (Convert.ToSingle(rlst));
                }
                public float[] DelayArray(string ChannelList)
                {
                    string[] rslt = ReadCommand("DEL? " + "(@" + ChannelList.Trim() + ")").Split(',');
                    float[] arrDouble = new float[rslt.Length];
                    for (int i = 0; i < rslt.Length; i++)
                    {
                        arrDouble[i] = float.Parse(rslt[i]);
                    }
                    return (arrDouble);
                }
                public float[] DelayArray(int StartChannel, int EndChannel)
                {
                    string[] rslt = ReadCommand("DEL? " + "(@" + StartChannel.ToString() + ":" + EndChannel.ToString() + ")").Split(',');
                    float[] arrDouble = new float[rslt.Length];
                    for (int i = 0; i < rslt.Length; i++)
                    {
                        arrDouble[i] = float.Parse(rslt[i]);
                    }
                    return (arrDouble);
                }
            }
            public class cConfigure : cCommonFunction
            {
                public cConfigure(FormattedIO488 parse) : base(parse) { }

                public void Source(int slot)
                {
                    SendCommand("CONF:EXT:TRIG:SOUR " + slot.ToString());
                }
                public int Source()
                {
                    return (Convert.ToInt16(ReadCommand("CONF:EXT:TRIG:SOUR?")));
                }
                public void Output(int Output)
                {
                    SendCommand("CONF:EXT:TRIG:OUTP " + Output.ToString());
                }
                public void Output(Output Output)
                {
                    SendCommand("CONF:EXT:TRIG:OUTP " + Output.ToString());
                }
                public void Output(bool Output)
                {
                    if (Output == true)
                    {
                        SendCommand("CONF:EXT:TRIG:OUTP 1");
                    }
                    else
                    {
                        SendCommand("CONF:EXT:TRIG:OUTP 0");
                    }
                }
                //public int Output()
                //{
                //    return (Convert.ToInt32(ReadCommand("CONF:EXT:TRIG:OUTP?")));
                //}
                public Output Output()
                {
                    return (ConvStr2Output(ReadCommand("CONF:EXT:TRIG:OUTP?")));
                }
            }
        }
        public class cStateStorage : cCommonFunction
        {
            public cStateStorage(FormattedIO488 parse) : base(parse) { }

            public void Save(int Memory_Location)
            {
                if (Memory_Location > 50)
                {
                    common.DisplayError(ClassName, "Memory Locations Exceeded", "Memory Locations exceed 50 locations");
                }
                else
                {
                    SendCommand("*SAV " + Memory_Location.ToString());
                }
            }
            public void Recall(int Memory_Location)
            {
                if (Memory_Location > 50)
                {
                    common.DisplayError(ClassName, "Memory Locations Exceeded", "Memory Locations exceed 50 locations");
                }
                else
                {
                    SendCommand("*RCL " + Memory_Location.ToString());
                }
            }
            public void Delete(int Memory_Location)
            {
                SendCommand("SYST:STAT:DEL " + Memory_Location.ToString());
            }
            public void DeleteAll()
            {
                SendCommand("SYST:STAT:DEL ALL");
            }

        }
        public class cDigital : cCommonFunction
        {
            public cInput Input;
            public cOutput Output;
            public cConfiguration Configuration;
            public cIO_Memory IO_Memory;

            public cDigital(FormattedIO488 parse)
                : base(parse)
            {
                Input = new cInput(parse);
                Output = new cOutput(parse);
                Configuration = new cConfiguration(parse);
                IO_Memory = new cIO_Memory(parse);
            }

            public class cInput : cCommonFunction
            {
                public cInput(FormattedIO488 parse) : base(parse) { }

                public int Bit(int Bit_Port)
                {
                    return (Convert.ToInt32(ReadCommand("SENS:DIG:DATA:BIT? " + Bit_Port.ToString())));
                }
                public int Byte(int Port)
                {
                    return (Convert.ToInt32(ReadCommand("SENS:DIG:DATA:BYTE? " + Port.ToString())));
                }
                public int Word(int Port)
                {
                    return (Convert.ToInt32(ReadCommand("SENS:DIG:DATA:WORD? " + Port.ToString())));
                }
                public int LWord(int Port)
                {
                    return (Convert.ToInt32(ReadCommand("SENS:DIG:DATA:LWORD? " + Port.ToString())));
                }
                public Digital_Data Block_Byte(int Port, int Size)
                {
                    string RtnData = ReadCommand("SENS:DIG:DATA:BYTE:BLOCK? " + Port.ToString() + "," + Size.ToString());
                    Digital_Data rtnRslt = new Digital_Data();

                    rtnRslt.digits = Size.ToString().Length;
                    rtnRslt.length = Size;
                    if (RtnData.Substring(0, 1) == "#")
                    {
                        rtnRslt.blockdata = RtnData.Substring((2 + rtnRslt.digits), Size);
                    }
                    else
                    {
                        rtnRslt.blockdata = RtnData.Substring((1 + rtnRslt.digits), Size);
                    }
                    return (rtnRslt);
                }
                public Digital_Data Block_Word(int Port, int Size)
                {
                    string RtnData = ReadCommand("SENS:DIG:DATA:WORD:BLOCK? " + Port.ToString() + "," + Size.ToString());
                    Digital_Data rtnRslt = new Digital_Data();

                    rtnRslt.digits = Size.ToString().Length;
                    rtnRslt.length = Size;
                    if (RtnData.Substring(0, 1) == "#")
                    {
                        rtnRslt.blockdata = RtnData.Substring((2 + rtnRslt.digits), Size);
                    }
                    else
                    {
                        rtnRslt.blockdata = RtnData.Substring((1 + rtnRslt.digits), Size);
                    }
                    return (rtnRslt);
                }
                public Digital_Data Block_LWORD(int Port, int Size)
                {
                    string RtnData = ReadCommand("SENS:DIG:DATA:LWORD:BLOCK? " + Port.ToString() + "," + Size.ToString());
                    Digital_Data rtnRslt = new Digital_Data();

                    rtnRslt.digits = Size.ToString().Length;
                    rtnRslt.length = Size;
                    if (RtnData.Substring(0, 1) == "#")
                    {
                        rtnRslt.blockdata = RtnData.Substring((2 + rtnRslt.digits), Size);
                    }
                    else
                    {
                        rtnRslt.blockdata = RtnData.Substring((1 + rtnRslt.digits), Size);
                    }
                    return (rtnRslt);
                }
            }
            public class cOutput : cCommonFunction
            {
                public cOutput(FormattedIO488 parse) : base(parse) { }

                public void Bit(int Bit_Port, int data)
                {
                    SendCommand("SOUR:DIG:DATA:BIT " + Bit_Port.ToString() + "," + data.ToString());
                }
                public void Byte(int port, int data)
                {
                    SendCommand("SOUR:DIG:DATA:BYTE " + port.ToString() + "," + data.ToString());
                }
                public void Word(int port, int data)
                {
                    SendCommand("SOUR:DIG:DATA:WORD " + port.ToString() + "," + data.ToString());
                }
                public void LWord(int port, int data)
                {
                    SendCommand("SOUR:DIG:DATA:LWORD " + port.ToString() + "," + data.ToString());
                }
                public void Block_Byte(int port, Digital_Data block_data)
                {
                    SendCommand("SOUR:DIG:DATA:BYTE:BLOCK " + port.ToString() + ",#" + block_data.digits.ToString() + block_data.length.ToString() + block_data.blockdata);
                }
                public void Block_Byte(int port, string block_data)
                {
                    int size = block_data.Length;

                    SendCommand("SOUR:DIG:DATA:BYTE:BLOCK " + port.ToString() + ",#" + size.ToString().Length + size.ToString() + block_data.Trim());
                }
                public void Block_Word(int port, Digital_Data block_data)
                {
                    SendCommand("SOUR:DIG:DATA:WORD:BLOCK " + port.ToString() + ",#" + block_data.digits.ToString() + block_data.length.ToString() + block_data.blockdata);
                }
                public void Block_Word(int port, string block_data)
                {
                    int size = block_data.Length;

                    SendCommand("SOUR:DIG:DATA:WORD:BLOCK " + port.ToString() + ",#" + size.ToString().Length + size.ToString() + block_data.Trim());
                }
                public void Block_LWord(int port, Digital_Data block_data)
                {
                    SendCommand("SOUR:DIG:DATA:LWORD:BLOCK " + port.ToString() + ",#" + block_data.digits.ToString() + block_data.length.ToString() + block_data.blockdata);
                }
                public void Block_LWord(int port, string block_data)
                {
                    int size = block_data.Length;

                    SendCommand("SOUR:DIG:DATA:LWORD:BLOCK " + port.ToString() + ",#" + size.ToString().Length + size.ToString() + block_data.Trim());
                }
            }
            public class cConfiguration : cCommonFunction
            {
                public cDigitialData Data;
                public cConfiguration(FormattedIO488 parse)
                    : base(parse)
                {
                    Data = new cDigitialData(parse);
                }

                public void Mode(int Port, Digital_ConfigMode dMode)
                {
                    SendCommand("SOUR:DIG:MODE " + Port.ToString() + "," + dMode.GetHashCode().ToString());
                }
                public void Mode(int Port, int dMode)
                {
                    SendCommand("SOUR:DIG:MODE " + Port.ToString() + "," + dMode.ToString());
                }
                //public int Mode(int Port)
                //{
                //    return(Convert.ToInt32(ReadCommand("SOUR:DIG:MODE? " + Port.ToString())));
                //}
                public Digital_ConfigMode Mode(int Port)
                {
                    return (ConvStr2Digital_Mode(ReadCommand("SOUR:DIG:MODE? " + Port.ToString())));
                }

                public void Control_Polarity(int Slot, Polarity Polar)
                {
                    SendCommand("SOUR:CONT:POL " + Slot.ToString() + "," + Polar.GetHashCode().ToString());
                }
                public Polarity Control_Polarity(int Slot)
                {
                    return (ConvStr2Polarity(ReadCommand("SOUR:CONT:POL? " + Slot.ToString())));
                }
                public void Flag_Polarity(int Slot, Polarity Polar)
                {
                    SendCommand("SOUR:FLAG:POL " + Slot.ToString() + "," + Polar.GetHashCode().ToString());
                }
                public Polarity Flag_Polarity(int Slot)
                {
                    return (ConvStr2Polarity(ReadCommand("SOUR:FLAG:POL? " + Slot.ToString())));
                }
                public void IO_Polarity(int Slot, Polarity Polar)
                {
                    SendCommand("SOUR:IO:POL " + Slot.ToString() + "," + Polar.GetHashCode().ToString());
                }
                public Polarity IO_Polarity(int Slot)
                {
                    return (ConvStr2Polarity(ReadCommand("SOUR:IO:POL? " + Slot.ToString())));
                }
                public class cDigitialData : cCommonFunction
                {
                    public cDigitialData(FormattedIO488 parse) : base(parse) { }

                    public void Byte_Polarity(int Port, Polarity Polar)
                    {
                        SendCommand("SOUR:DIG:DATA:BYTE:POL " + Port.ToString() + "," + Polar.GetHashCode().ToString());
                    }
                    public Polarity Byte_Polarity(int Port)
                    {
                        return (ConvStr2Polarity(ReadCommand("SOUR:DIG:DATA:BYTE:POL? " + Port.ToString())));
                    }
                    public void Word_Polarity(int Port, Polarity Polar)
                    {
                        SendCommand("SOUR:DIG:DATA:WORD:POL " + Port.ToString() + "," + Polar.GetHashCode().ToString());
                    }
                    public Polarity Word_Polarity(int Port)
                    {
                        return (ConvStr2Polarity(ReadCommand("SOUR:DIG:DATA:WORD:POL? " + Port.ToString())));
                    }
                    public void LWord_Polarity(int Port, Polarity Polar)
                    {
                        SendCommand("SOUR:DIG:DATA:LWORD:POL " + Port.ToString() + "," + Polar.GetHashCode().ToString());
                    }
                    public Polarity LWord_Polarity(int Port)
                    {
                        return (ConvStr2Polarity(ReadCommand("SOUR:DIG:DATA:LWORD:POL? " + Port.ToString())));
                    }
                }
            }

            public class cIO_Memory : cCommonFunction
            {
                public cData Data;
                public cTrace Trace;

                public cIO_Memory(FormattedIO488 parse)
                    : base(parse)
                {
                    Data = new cData(parse);
                    Trace = new cTrace(parse);
                }

                public class cTrace : cCommonFunction
                {
                    public cTrace(FormattedIO488 parse) : base(parse) { }

                    public void Define(string System_Memory_Name, int size)
                    {
                        SendCommand("SOUR:DIG:TRAC:DEF " + System_Memory_Name.Trim() + "," + size.ToString());
                    }
                    public void Define(string System_Memory_Name, int size, string fill_data)
                    {
                        SendCommand("SOUR:DIG:TRAC:DEF " + System_Memory_Name.Trim() + "," + size.ToString() + "," + fill_data.Trim());
                    }
                    public int Define(string System_Memory_Name)
                    {
                        return (Convert.ToInt32(ReadCommand("SOUR:DIG:TRAC:DEF? " + System_Memory_Name.Trim())));
                    }
                    public string Memory_Catalog()
                    {
                        return (ReadCommand("SOUR:DIG:TRAC:DEF:CAT?"));
                    }
                    //public string[] Memory_Catalog()
                    //{
                    //    return(ReadCommand("SOUR:DIG:TRAC:DEF:CAT?").Split(','));
                    //}
                    public void Delete_Memory(string System_Memory_Name)
                    {
                        SendCommand("SOUR:DIGLTRAC:DEL " + System_Memory_Name.Trim());
                    }
                    public void Delete_ALL_Memory()
                    {
                        SendCommand("SOUR:DIGLTRAC:DEL:ALL");
                    }
                }
                public class cData : cCommonFunction
                {
                    public cData(FormattedIO488 parse) : base(parse) { }

                    public void Save_Memory(string System_Memory_Name, Digital_Data block_data)
                    {
                        SendCommand("SOUR:DIG:TRAC " + System_Memory_Name.Trim() + ",#" + block_data.digits.ToString() + block_data.length.ToString() + block_data.blockdata.Trim());
                    }
                    public void Save_Memory(string System_Memory_Name, string block_data)
                    {
                        int size = block_data.Length;
                        SendCommand("SOUR:DIG:TRAC " + System_Memory_Name.Trim() + ",#" + size.ToString().Length + size.ToString() + block_data.Trim());
                    }
                    public void Load_Memory_Byte(int Port, string System_Memory_Name)
                    {
                        SendCommand("SOUR;DIG:DATA:BYTE:TRAC " + Port.ToString() + "," + System_Memory_Name.Trim());
                    }
                    public void Load_Memory_Word(int Port, string System_Memory_Name)
                    {
                        SendCommand("SOUR;DIG:DATA:WORD:TRAC " + Port.ToString() + "," + System_Memory_Name.Trim());
                    }
                    public void Load_Memory_LWord(int Port, string System_Memory_Name)
                    {
                        SendCommand("SOUR;DIG:DATA:LWORD:TRAC " + Port.ToString() + "," + System_Memory_Name.Trim());
                    }
                    //public string Save_Memory(string System_Memory_Name)
                    //{
                    //    return(ReadCommand("SOUR:DIG:TRAC? " + System_Memory_Name.Trim()));
                    //}
                    public Digital_Data Save_Memory(string System_Memory_Name)
                    {
                        string RtnData = ReadCommand("SOUR:DIG:TRAC? " + System_Memory_Name.Trim());
                        Digital_Data rtnRslt = new Digital_Data();

                        if (RtnData.Substring(0, 1) == "#")
                        {
                            rtnRslt.digits = Convert.ToInt32(RtnData.Substring(1, 1));
                            rtnRslt.length = Convert.ToInt32(RtnData.Substring(2, rtnRslt.digits));
                            rtnRslt.blockdata = RtnData.Substring((1 + rtnRslt.length.ToString().Length), rtnRslt.length);
                        }
                        else
                        {
                            rtnRslt.digits = Convert.ToInt32(RtnData.Substring(0, 1));
                            rtnRslt.length = Convert.ToInt32(RtnData.Substring(1, rtnRslt.digits));
                            rtnRslt.blockdata = RtnData.Substring((rtnRslt.length.ToString().Length), rtnRslt.length);
                        }

                        return (rtnRslt);
                    }
                }
            }
        }
        public class cSystem : cCommonFunction
        {
            public cSystem(FormattedIO488 parse) : base(parse) { }

            public int Status_Condition()
            {
                return (Convert.ToInt32(ReadCommand("STAT:OPER:COND?")));
            }
            public int Status_Event()
            {
                return (Convert.ToInt32(ReadCommand("STAT:OPER?")));
            }
            public void Enable_Mask(int Mask)
            {
                SendCommand("STAT:OPER:ENAB " + Mask.ToString());
            }
            public int Enable_Mask()
            {
                return (Convert.ToInt32(ReadCommand("STAT:OPER:ENAB?")));
            }
            public void Preset()
            {
                SendCommand("STAT:PRES");
            }
            public int Error()
            {
                return (Convert.ToInt32(ReadCommand("SYST:ERR?")));
            }
            public string Version()
            {
                return (ReadCommand("SYST:VERS?"));
            }
            public string CardType(int slot)
            {
                return (ReadCommand("SYST:CTYPE? " + slot.ToString()));
            }
            public void CardReset(int slot)
            {
                SendCommand("SYST:CPON " + slot.ToString());
            }
            public void CardReset_ALL()
            {
                SendCommand("SYST:CPON ALL");
            }

        }
        public class cDiagnostic : cCommonFunction
        {
            public cDiagnostic(FormattedIO488 parse) : base(parse) { }

            public void Monitor(int slot)
            {
                SendCommand("DIAG:MON " + slot.ToString());
            }
            public void Monitor_disable()
            {
                SendCommand("DIAG:MON -1");
            }

            public int monitor()
            {
                return (Convert.ToInt32(ReadCommand("DIAG:MON?")));
            }
            public void Display(string Message)
            {
                SendCommand("DIAG:DISP " + Message.Trim());
            }
            public void Display_State(int state)
            {
                SendCommand("DIAG:DISP:STAT " + state.ToString());
            }
            //public void Display_State(int state)
            //{
            //    SendCommand("DIAG:DISP:STAT " + state.ToString());
            //}
            public void Display_State(State sState)
            {
                SendCommand("DIAG:DISP:STAT " + sState.GetHashCode().ToString());
            }
            //public int Display_State()
            //{
            //    return (Convert.ToInt32(ReadCommand("DIAG:DISP:STAT?")));
            //}
            public State Display_State()
            {
                return (ConvStr2State(ReadCommand("DIAG:DISP:STAT?")));
            }
            public void CycleCount_Clear(int Channel)
            {
                SendCommand("DIAG:CYCL:CLE " + Channel.ToString());
            }
            public int CycleCount(int Channel)
            {
                return (Convert.ToInt32(ReadCommand("DIAG:CYCL? " + Channel.ToString())));
            }
            public int CycleCount_Max(int Slot)
            {
                return (Convert.ToInt32(ReadCommand("DIAG:CYCL:MAX? " + Slot.ToString())));
            }
        }
        public class cSysMode : cCommonFunction
        {
            public cSysMode(FormattedIO488 parse) : base(parse) { }

            //public void SysMode(int sMode)
            //{
            //    SendCommand("SYSMODE " + sMode.ToString());
            //}
            //public void SysMode(string sMode)
            //{
            //    SendCommand("SYSMODE " + sMode.Trim());
            //}
            public void SysMode(SystemMode sMode)
            {
                SendCommand("SYSMODE " + sMode.GetHashCode());
            }
            //public int SysMode()
            //{
            //    return (Convert.ToInt32(ReadCommand("SYSMODE?")));
            //}
            //public string SysMode()
            //{
            //    return (ReadCommand("SYSMODE?"));
            //}
            public SystemMode SysMode()
            {
                return (ConvStr2SysMode(ReadCommand("SYSMODE?")));
            }
        }
        public class cRS232 : cCommonFunction
        {
            public cRS232(FormattedIO488 parse) : base(parse) { }
            public void Local()
            {
                SendCommand("SYST:LOC");
            }
            public void Remote()
            {
                SendCommand("SYST:REM");
            }
            public void RWLock()
            {
                SendCommand("SYST:RWL");
            }
        }
        #endregion
    }
    public class cSwitch3488 : iCommonFunction
    {
        public static string ClassName = "Switch 3488 Class";
        private string IOAddress;
        private FormattedIO488 ioSwitch;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();

        #region "Class Initialization"
        public cStandard Standard;
        public cSysMode SysMode;

        public cCommonFunction BasicCommand;  // Basic Command for General Equipment

        void Init(FormattedIO488 IOInit)
        {
            Standard = new cStandard(IOInit);
            BasicCommand = new cCommonFunction(IOInit);
            SysMode = new cSysMode(IOInit);
        }
        #endregion

        #region "Conversion Function"
        public static SystemMode ConvStr2SysMode(string sMode)
        {
            switch (sMode.ToUpper())
            {
                case "0":
                case "SCPI":
                    return (SystemMode.SCPI);
                case "1":
                case "HP3488":
                case "HP3488A":
                    return (SystemMode.HP3488);
                default:
                    common.DisplayError(ClassName, "Error in converting String to SysMode", "Unable to convert string : " + sMode);
                    return (SystemMode.SCPI);
            }
        }
        #endregion

        public string Address
        {
            get
            {
                return IOAddress;
            }
            set
            {
                IOAddress = value;
            }
        }

        public FormattedIO488 parseIO
        {
            get
            {
                return ioSwitch;
            }
            set
            {
                ioSwitch = parseIO;
            }
        }
        public void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    ioSwitch = new FormattedIO488();
                    ioSwitch.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioSwitch.IO = null;
                    return;
                }
                Init(ioSwitch);
            }
        }
        public void CloseIO()
        {
            ioSwitch.IO.Close();
        }
        
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  15/1/2011       KKL             VISA Driver for Switch 3488/3499

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return ("Switch 3488 Class Version = v" + VersionStr);
        }

        #region "Class Functional Codes"
        public class cStandard : cCommonFunction
        {
            public cStandard(FormattedIO488 parse) : base(parse) { }

            public void Close(int Channel)
            {
                SendCommand("CLOSE " + Channel.ToString());
            }
            public void Close(string Channel)
            {
                SendCommand("CLOSE " + Channel.Trim());
            }
            public void Close(int[] Channel)
            {
                int Channel_Length = Channel.Length;
                string ChannelStr;
                ChannelStr = Channel[0].ToString();
                if (Channel_Length > 1)
                {
                    for (int i = 1; i <= Channel_Length; i++)
                    {
                        ChannelStr += "," + Channel[i].ToString();
                    }
                    SendCommand("CLOSE " + ChannelStr.Trim());
                }
                else
                {
                    SendCommand("CLOSE " + Channel[0].ToString());
                }
            }
            public void Open(int Channel)
            {
                SendCommand("OPEN " + Channel.ToString());
            }
            public void Open(string Channel)
            {
                SendCommand("OPEN " + Channel.Trim());
            }
            public void Open(int[] Channel)
            {
                int Channel_Length = Channel.Length;
                string ChannelStr;
                ChannelStr = Channel[0].ToString();
                if (Channel_Length > 1)
                {
                    for (int i = 1; i <= Channel_Length; i++)
                    {
                        ChannelStr += "," + Channel[i].ToString();
                    }
                    SendCommand("OPEN " + ChannelStr.Trim());
                }
                else
                {
                    SendCommand("OPEN " + Channel[0].ToString());
                }
            }
            public void View(int Channel)
            {
                SendCommand("VIEW " + Channel.ToString());
            }
            public void View(string Channel)
            {
                SendCommand("VIEW " + Channel.Trim());
            }
            public string CardType(int Slot)
            {
                return (ReadCommand("CTYPE?"));
            }
            public void Card_Reset(int Slot)
            {
                SendCommand("CRESET " + Slot.ToString());
            }
            public void Card_Reset(string Slot)
            {
                SendCommand("CRESET " + Slot.Trim());
            }


        }
        public class cSysMode : cCommonFunction
        {
            public cSysMode(FormattedIO488 parse) : base(parse) { }

            public void SysMode(int sMode)
            {
                SendCommand("SYSMODE " + sMode.ToString());
            }
            public void SysMode(string sMode)
            {
                SendCommand("SYSMODE " + sMode.Trim());
            }
            public void SysMode(SystemMode sMode)
            {
                SendCommand("SYSMODE " + sMode.GetHashCode());
            }
            //public int SysMode()
            //{
            //    return (Convert.ToInt32(ReadCommand("SYSMODE?")));
            //}
            //public string SysMode()
            //{
            //    return (ReadCommand("SYSMODE?"));
            //}
            public SystemMode SysMode()
            {
                return (ConvStr2SysMode(ReadCommand("SYSMODE?")));
            }
        }
        #endregion
    }
}
