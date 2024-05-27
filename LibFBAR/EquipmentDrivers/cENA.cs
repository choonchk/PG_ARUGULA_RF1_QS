using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR
{
    #region "Enumeration Declaration"
    public enum e_SParametersDef
    {
        S11 = 0,
        S12,
        S13,
        S14,
        S21,
        S22,
        S23,
        S24,
        S31,
        S32,
        S33,
        S34,
        S41,
        S42,
        S43,
        S44,
        //A, B, C, D,
        SDS32, SDD33, SDS31, SCS31,
        SDD11, SDD22, SDD44, SDS12,
        DELAY
    }
    public enum e_SFormat
    {
        MLOG = 0,
        PHAS,
        GDEL,
        SLIN,
        SLOG,
        SCOM,
        SMIT,
        SADM,
        PLIN,
        PLOG,
        POL,
        MLIN,
        SWR,
        REAL,
        IMAG,
        UPH,
        PPH
    }
    public enum e_Format
    {
        NORM = 0,
        SWAP
    }
    public enum e_FormatData
    {
        ASC = 0,
        REAL,
        REAL32
    }
    public enum e_SNPFormat
    {
        AUTO = 0,
        MA,
        DB,
        RI
    }
    public enum e_SType
    {
        STAT = 0,
        CST,
        DST,
        CDST
    }
    public enum e_OnOff
    {
        Off = 0,
        On
    }
    public enum e_ModeSetting
    {
        StartStop = 0,
        CenterSpan
    }
    public enum e_SweepMode
    {
        Stepped = 0,
        Swept,
        FastStepped,
        FastSwept
    }
    public enum e_SweepGeneration
    {
        STEP = 0,
        ANAL
    }
    public enum e_SweepType
    {
        LIN = 0,
        LOG,
        SEGM,
        POW
    }
    public enum e_TriggerScope
    {
        ALL = 0,
        ACT
    }
    public enum e_TriggerSource
    {
        INT = 0,
        EXT,
        MAN,
        BUS
    }
    public enum e_CalibrationType
    {
        OPEN = 0,
        LOAD,
        SHORT,
        THRU,
        ISOLATION,
        ECAL,
        SUBCLASS,
        TRLLINE,
        TRLREFLECT,
        TRLTHRU
    }
    public enum e_PortMatchType
    {
        NONE = 0,
        SLPC,
        PCSL,
        PLSC,
        SCPL,
        PLPC,
        USER
    }
    public enum e_BalunDevice
    {
        SBAL = 0,
        BBAL,
        SSB,
        BAL
    }
    public enum e_CalStdType
    {
        OPEN = 0,
        SHORT,
        LOAD,
        THRU,
        UTHRU,
        ARBI,
        NONE
    }
    public enum e_CalStdMedia
    {
        COAXIAL = 0,
        WAVEGUIDE
    }
    public enum e_CalStdLengthType
    {
        FIXED = 0,
        SLIDING,
        OFFSET
    }
    #endregion

    #region "Structure"
    public struct s_CalStdTable
    {
        public e_OnOff enable;
        public int calkit_locnum;
        public string calkit_label;
        public int total_calstd;
        public s_CalStdData[] CalStdData;
    }
    public struct s_CalStdData
    {
        public e_CalStdType StdType;
        public string StdLabel;
        public int StdNo;
        public double C0_L0;
        public double C1_L1;
        public double C2_L2;
        public double C3_L3;
        public double OffsetDelay;
        public double OffsetZ0;
        public double OffsetLoss;
        public double ArbImp;
        public double MinFreq;
        public double MaxFreq;
        public e_CalStdMedia Media;
        public e_CalStdLengthType LengthType;
    }
    public struct s_TestSetTable
    {
        public e_OnOff enable;
        public e_OnOff display;
        public int total_testset;
        public s_TestSetData[] TestSetData;
    }
    public struct s_TestSetData
    {
        public int TestSetNo;
        public string Name;
        public int TotalPort;
        public string[] PortLabel;
        public double CtrlA_Voltage;
        public int CtrlA_Decimal;
        public double CtrlB_Voltage;
        public int CtrlB_Decimal;
        public double CtrlC_Voltage;
        public int CtrlC_Decimal;
        public double CtrlD_Voltage;
        public int CtrlD_Decimal;
    }
    public struct s_SegmentTable
    {
        public e_ModeSetting mode;
        public e_OnOff ifbw;
        public e_OnOff pow;
        public e_OnOff del;
        public e_OnOff swp;
        public e_OnOff time;
        public int segm;
        public s_SegmentData[] SegmentData;

    }
    public struct s_SegmentData
    {
        public double Start;
        public double Stop;
        public int Points;
        public double ifbw_value;
        public double pow_value;
        public double del_value;
        public e_SweepMode swp_value;
        public double time_value;
    }
    public struct s_PortMatchDetailSetting
    {
        public e_PortMatchType MatchType;
        public double R;
        public double L;
        public double C;
        public double G;
        public string UserFile;
    }
    public struct s_PortMatchSetting
    {
        public s_PortMatchDetailSetting[] Port;
        public int ChannelNumber;
        public bool Enable;
    }
    public struct s_DiffMatchDetailSetting
    {
        public e_PortMatchType MatchType;
        public double R;
        public double L;
        public double C;
        public double G;
        public string UserFile;
    }
    public struct s_DiffMatchSetting
    {
        public s_DiffMatchDetailSetting[] Port;
        public int ChannelNumber;
        public bool Enable;
    }
    public struct s_PortExtDetailSetting
    {
        public int Port_No;

        public double Coax_Ext;
        public double WaveGuide_Ext;
        public double CutOff_Freq;

        public e_OnOff Loss1_Enb;
        public double Loss1;
        public double Freq1;

        public e_OnOff Loss2_Enb;
        public double Loss2;
        public double Freq2;

        public double Loss_at_DC;
    }
    public struct s_PortExtSetting
    {
        public s_PortExtDetailSetting[] Port;
        public int ChannelNumber;
        public bool Enable;
    }
    #endregion
    public class cENA : iCommonFunction
    {
        public static string ClassName = "ENA E5071A/B/C Class";
        private string IOAddress;
        private FormattedIO488 ioENA;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();


        #region "Conversion Functions"
        public static string Convert_SegmentTable2String(s_SegmentTable SegmentTable, e_OnOff SweepMode)
        {
            string tmpStr;
            tmpStr = "";
            switch (SweepMode)
            {
                case e_OnOff.On:
                    tmpStr = ((int)SegmentTable.mode).ToString();
                    tmpStr += "," + ((int)SegmentTable.ifbw).ToString();
                    tmpStr += "," + ((int)SegmentTable.pow).ToString();
                    tmpStr += "," + ((int)SegmentTable.del).ToString();
                    tmpStr += "," + ((int)SegmentTable.swp).ToString();
                    tmpStr += "," + ((int)SegmentTable.time).ToString();
                    tmpStr += "," + SegmentTable.segm.ToString();
                    for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                    {
                        tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                        if (SegmentTable.ifbw == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].ifbw_value.ToString();
                        if (SegmentTable.pow == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].pow_value.ToString();
                        if (SegmentTable.del == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].del_value.ToString();
                        if (SegmentTable.swp == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].swp_value.ToString();
                        if (SegmentTable.time == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].time_value.ToString();
                    }
                    break;
                case e_OnOff.Off:
                    tmpStr = ((int)SegmentTable.mode).ToString();
                    tmpStr += "," + ((int)SegmentTable.ifbw).ToString();
                    tmpStr += "," + ((int)SegmentTable.pow).ToString();
                    tmpStr += "," + ((int)SegmentTable.del).ToString();
                    tmpStr += "," + ((int)SegmentTable.time).ToString();
                    tmpStr += "," + SegmentTable.segm.ToString();
                    for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                    {
                        tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                        if (SegmentTable.ifbw == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].ifbw_value.ToString();
                        if (SegmentTable.pow == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].pow_value.ToString();
                        if (SegmentTable.del == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].del_value.ToString();
                        if (SegmentTable.time == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].time_value.ToString();
                    }
                    break;

            }
            return (tmpStr);
        }
        #endregion

        /// <summary>
        /// Parsing Equpment Address
        /// </summary>
        public virtual string Address
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
        /// <summary>
        /// Parsing IO (in FormattedIO488)
        /// </summary>
        public virtual FormattedIO488 parseIO
        {
            get
            {
                return ioENA;
            }
            set
            {
                ioENA = value;
            }
        }
        /// <summary>
        /// Open Equipment IO
        /// </summary>
        public virtual void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    ioENA = new FormattedIO488();
                    ioENA.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioENA.IO = null;
                    Init(ioENA, false);
                    return;
                }
                Init(ioENA);
            }
        }
        /// <summary>
        /// Close Equipment IO
        /// </summary>
        public virtual void CloseIO()
        {
            ioENA.IO.Close();
        }
        /// <summary>
        /// Driver Revision control
        /// </summary>
        /// <returns>Driver's Version</returns>
        public virtual string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01a";       //  14/11/2011       KKL             VISA Driver for ENA (Base on minimum required command)
            VersionStr = "0.01b";       //  12/04/2012       KKL             Change the cFormat --> Format : to include the Trace selection before set/get the Format of the desired trace
            //                                   Added coding for Balun features
            VersionStr = "0.01c";       //  28/04/2012       KKL             Added CMRR to e_SParametersDef ENUM

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        #region "Class Initialization"
        public cCommonFunction BasicCommand; // Basic Command for General Equipment (Must be Initialized)
        public cCalculate Calculate;
        public cDisplay Display;
        public cSystem System;
        public cFormat Format;
        public cInitiate Initiate;
        public cMemory Memory;
        public cSense Sense;
        public cTrigger Trigger;

        /// <summary>
        /// Initializing all Parameters
        /// </summary>
        /// <param name="IOInit"></param>
        public virtual void Init(FormattedIO488 IOInit)
        {
            BasicCommand = new cCommonFunction(IOInit);
            Calculate = new cCalculate(IOInit);
            Display = new cDisplay(IOInit);
            System = new cSystem(IOInit);
            Format = new cFormat(IOInit);
            Initiate = new cInitiate(IOInit);
            Memory = new cMemory(IOInit);
            Sense = new cSense(IOInit);
            Trigger = new cTrigger(IOInit);
        }
        public virtual void Init(FormattedIO488 IOInit, bool Enable)
        {
            BasicCommand = new cCommonFunction(IOInit);
            BasicCommand.b_IOEnable = false;
            Calculate = new cCalculate(IOInit);
            Calculate.b_IOEnable = false;
            Display = new cDisplay(IOInit);
            Display.b_IOEnable = false;
            System = new cSystem(IOInit);
            System.b_IOEnable = false;
            Format = new cFormat(IOInit);
            Format.b_IOEnable = false;
            Initiate = new cInitiate(IOInit);
            Initiate.b_IOEnable = false;
            Memory = new cMemory(IOInit);
            Memory.b_IOEnable = false;
            Sense = new cSense(IOInit);
            Sense.b_IOEnable = false;
            Trigger = new cTrigger(IOInit);
            Trigger.b_IOEnable = false;
        }
        #endregion

        #region "Class Functional Codes"
        /// <summary>
        /// Calculate Class Function.
        /// </summary>
        public class cCalculate : cCommonFunction
        {
            public cFixtureSimulator FixtureSimulator;
            public cParameter Par;
            public cFormat Format;
            public cFunction Func;
            public cData Data;

            public cCalculate(FormattedIO488 parse)
                : base(parse)
            {
                FixtureSimulator = new cFixtureSimulator(parse);
                Par = new cParameter(parse);
                Format = new cFormat(parse);
                Func = new cFunction(parse);
                Data = new cData(parse);
            }
            /// <summary>
            /// Fixture Simulator Class Function
            /// </summary>
            public class cFixtureSimulator : cCommonFunction
            {
                public cSended SENDed;
                public cBalun BALun;
                public cFixtureSimulator(FormattedIO488 parse)
                    : base(parse)
                {
                    BALun = new cBalun(parse);
                    SENDed = new cSended(parse);
                }

                public class cSended : cCommonFunction
                {
                    public cPMCircuit PMCircuit;
                    public cZConversion ZConversion;
                    public cSended(FormattedIO488 parse)
                        : base(parse)
                    {
                        PMCircuit = new cPMCircuit(parse);
                        ZConversion = new cZConversion(parse);
                    }
                    public class cPMCircuit : cCommonFunction
                    {
                        public cPMCircuit(FormattedIO488 parse) : base(parse) { }
                        public virtual void R(int PortNumber, double Resistance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public virtual void R(int ChannelNumber, int PortNumber, double Resistance)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public virtual void L(int PortNumber, double Inductance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public virtual void L(int ChannelNumber, int PortNumber, double Inductance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public virtual void C(int PortNumber, double Capacitance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public virtual void C(int ChannelNumber, int PortNumber, double Capacitance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public virtual void G(int PortNumber, double Conductance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }
                        public virtual void G(int ChannelNumber, int PortNumber, double Conductance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }

                        public virtual void Type(int PortNumber, e_PortMatchType PortType)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public virtual void Type(int ChannelNumber, int PortNumber, e_PortMatchType PortType)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }

                        public virtual void User(int PortNumber)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:USER");
                        }
                        public virtual void User(int ChannelNumber, int PortNumber)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:USER");
                        }

                        public virtual void UserFilename(int PortNumber, string S2PFilename)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }
                        public virtual void UserFilename(int ChannelNumber, int PortNumber, string S2PFilename)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }

                        public virtual void State(bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:SEND:PMC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:SEND:PMC:STAT OFF");
                                    break;
                            }
                        }
                        public virtual void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:STAT OFF");
                                    break;
                            }
                        }
                    }
                    public class cZConversion : cCommonFunction
                    {
                        public cZConversion(FormattedIO488 parse) : base(parse) { }
                        public virtual void Imag(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public virtual void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public virtual void Real(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public virtual void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public virtual void Z0(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public virtual void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }

                        public virtual void State(bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:SEND:ZCON:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:SEND:ZCON:STAT OFF");
                                    break;
                            }
                        }
                        public virtual void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:STAT OFF");
                                    break;
                            }
                        }
                    }

                }
                public class cBalun : cCommonFunction
                {
                    public cParameter Parameter;
                    public cDiffMatch DiffMatch;
                    public cDiffZConv DiffZConv;
                    public cCmnZConv CmnZConv;
                    public cBalun(FormattedIO488 parse)
                        : base(parse)
                    {
                        Parameter = new cParameter(parse);
                        DiffMatch = new cDiffMatch(parse);
                        DiffZConv = new cDiffZConv(parse);
                        CmnZConv = new cCmnZConv(parse);
                    }
                    public virtual void Topology(int ChannelNumber, e_BalunDevice Device, string portTopology)
                    {
                        SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:TOP:" + Device.ToString() + " " + portTopology);
                    }
                    public virtual void Device(int ChannelNumber, e_BalunDevice Device)
                    {
                        SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DEV " + Device.ToString());
                    }
                    public virtual e_BalunDevice Device(int ChannelNumber)
                    {
                        return ((e_BalunDevice)Enum.Parse(typeof(e_BalunDevice), (ReadCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DEV?"))));
                    }
                    public class cCmnZConv : cCommonFunction
                    {
                        public cCmnZConv(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public virtual void Imag(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public virtual void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public virtual void Real(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public virtual void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public virtual void Z0(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public virtual void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public virtual void State(bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:BAL:CZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:BAL:CZC:STAT OFF");
                                    break;
                            }
                        }
                        public virtual void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:STAT OFF");
                                    break;
                            }
                        }
                    }
                    public class cDiffZConv : cCommonFunction
                    {
                        public cDiffZConv(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public virtual void Imag(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public virtual void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public virtual void Real(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public virtual void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public virtual void Z0(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public virtual void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public virtual void State(bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:BAL:DZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:BAL:DZC:STAT OFF");
                                    break;
                            }
                        }
                        public virtual void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:STAT OFF");
                                    break;
                            }
                        }
                    }
                    public class cDiffMatch : cCommonFunction
                    {
                        public cDiffMatch(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public virtual void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:STAT OFF");
                                    break;
                            }
                        }
                        public virtual void R(int PortNumber, double Resistance)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DMC:BPORT" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public virtual void R(int ChannelNumber, int PortNumber, double Resistance)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public virtual void L(int PortNumber, double Inductance)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public virtual void L(int ChannelNumber, int PortNumber, double Inductance)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public virtual void C(int PortNumber, double Capacitance)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public virtual void C(int ChannelNumber, int PortNumber, double Capacitance)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public virtual void G(int PortNumber, double Conductance)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }
                        public virtual void G(int ChannelNumber, int PortNumber, double Conductance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }

                        public virtual void Type(int PortNumber, e_PortMatchType PortType)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public virtual void Type(int ChannelNumber, int PortNumber, e_PortMatchType PortType)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public virtual void UserFilename(int PortNumber, string S2PFilename)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }
                        public virtual void UserFilename(int ChannelNumber, int PortNumber, string S2PFilename)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }
                    }
                    public class cParameter : cCommonFunction
                    {
                        public cParameter(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public virtual void Status(int ChannelNumber, int TraceNumber, bool status)
                        {
                            switch (status)
                            {
                                case true:
                                    SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT 1");
                                    break;
                                default:
                                    SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT 0");
                                    break;
                            }
                        }
                        public virtual bool Status(int ChannelNumber, int TraceNumber)
                        {
                            return (common.CStr2Bool(ReadCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT?")));
                        }
                        // Quick Solution
                        public virtual void Parameter(int ChannelNumber, int TraceNumber, e_BalunDevice Device, string TraceLabel)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT ON");
                            switch (Device)
                            {
                                case e_BalunDevice.SSB:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":SSB " + TraceLabel.ToUpper());
                                    break;
                                case e_BalunDevice.SBAL:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":SBAL " + TraceLabel.ToUpper());
                                    break;
                                case e_BalunDevice.BBAL:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":BBAL " + TraceLabel.ToUpper());
                                    break;
                                case e_BalunDevice.BAL:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":BAL " + TraceLabel.ToUpper());
                                    break;
                                default:
                                    common.DisplayError(ClassName + " --> Fixture Analysis - Balun", "Error Defining Balun Device", "Unknown Device");
                                    break;
                            }
                        }
                        public virtual string Parameters(int ChannelNumber, int TraceNumber)
                        {
                            string rtnStr = "";
                            e_BalunDevice Device = (e_BalunDevice)Enum.Parse(typeof(e_BalunDevice), (ReadCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DEV?")));
                            switch (Device)
                            {
                                case e_BalunDevice.SSB:
                                    rtnStr = ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":SSB?");
                                    break;
                                case e_BalunDevice.SBAL:
                                    rtnStr = ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":SBAL?");
                                    break;
                                case e_BalunDevice.BBAL:
                                    rtnStr = ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":BBAL?");
                                    break;
                                case e_BalunDevice.BAL:
                                    rtnStr = ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":BAL?");
                                    break;
                                default:
                                    common.DisplayError(ClassName + " --> Fixture Analysis - Balun", "Error Defining Balun Device", "Unknown Device");
                                    rtnStr = "";
                                    break;
                            }
                            return rtnStr;
                        }
                    }
                }
                public virtual void State(bool Set)
                {
                    switch (Set)
                    {
                        case true:
                            SendCommand(":CALC1:FSIM:STAT ON");
                            break;
                        case false:
                            SendCommand(":CALC1:FSIM:STAT OFF");
                            break;
                    }
                }
                public virtual void State(int ChannelNumber, bool Set)
                {
                    switch (Set)
                    {
                        case true:
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT ON");
                            break;
                        case false:
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT OFF");
                            break;
                    }
                }
                public virtual bool State()
                {
                    return (common.CStr2Bool(ReadCommand(":CALC1:FSIM:STAT?")));
                }
                public virtual bool State(int ChannelNumber)
                {
                    //return (bool.Parse(ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT?")));
                    return (common.CStr2Bool(ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT?")));
                }
            }
            public class cParameter : cCommonFunction
            {
                public cParameter(FormattedIO488 parse) : base(parse) { }
                //public virtual void Count(int count)
                //{
                //    SendCommand("CALC1:PAR:COUN " + count.ToString());
                //}

                public virtual void Count(int ChannelNumber, int Trace)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN " + Trace.ToString());
                }
                public virtual int Count()
                {
                    return (Convert.ToInt32(ReadCommand("CALC1:PAR:COUN?")));
                }
                public virtual int Count(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN?")));
                }
                public virtual void Define(int Trace, e_SParametersDef Define)
                {
                    SendCommand("CALC1:PAR" + Trace.ToString() + ":DEF " + Define.ToString());
                }
                public virtual void Define(int ChannelNumber, int Trace, e_SParametersDef Define)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF " + Define.ToString());
                }
                public virtual void Define_Trace(int ChannelNumber, int Trace, string TraceName)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF " + TraceName);
                }
                public virtual string GetTraceCategory(int ChannelNumber)
                {
                    // ENA: return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?"));
                    // ZVT:
                    return "";
                }
                public virtual string Define(int ChannelNumber, int Trace)
                {
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?"));
                }
                public virtual e_SParametersDef Define_Enum(int ChannelNumber, int Trace)
                {
                    return (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?")));
                }
                public virtual void Select(int Trace)
                {
                    SendCommand("CALC1:PAR" + Trace.ToString() + ":SEL");
                }
                public virtual void Select(int ChannelNumber, int Trace)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":SEL");
                }
                public virtual void SPORT(int Trace, double value)
                {
                    SendCommand("CALC1:PAR" + Trace.ToString() + ":SPOR " + value.ToString());
                }
                public virtual void SPORT(int ChannelNumber, int Trace, double value)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":SPOR " + value.ToString());
                }
            }
            public class cFormat : cCommonFunction
            {
                public cFormat(FormattedIO488 parse) : base(parse) { }
                public virtual void Format(int TraceNumber, e_SFormat format)
                {
                    // added code to select the trace before changing the Format
                    SendCommand("CALC1:PAR" + TraceNumber.ToString() + ":SEL");
                    SendCommand("CALC1:FORM " + format.ToString());
                }
                public virtual void Format(int ChannelNumber, int TraceNumber, e_SFormat format)
                {
                    // added code to select the trace before changing the Format
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format.ToString());
                }
                public virtual void Format(int TraceNumber, string format)
                {
                    // added code to select the trace before changing the Format
                    SendCommand("CALC1:PAR" + TraceNumber.ToString() + ":SEL");
                    SendCommand("CALC1:FORM " + format);
                }
                public virtual void Format(int ChannelNumber, int TraceNumber, string format)
                {
                    // added code to select the trace before changing the Format
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format);
                }
                public virtual e_SFormat Format(int TraceNumber)
                {
                    // added code to select the trace before reading the Format
                    SendCommand("CALC1:PAR" + TraceNumber.ToString() + ":SEL");
                    string tmp = ReadCommand("CALC1:FORM?");
                    e_SFormat Format = (e_SFormat)Enum.Parse(typeof(e_SFormat), tmp);
                    return (Format);
                }
                public virtual e_SFormat Format(int ChannelNumber, int TraceNumber)
                {
                    // added code to select the trace before reading the Format
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    string tmp = ReadCommand("CALC" + ChannelNumber.ToString() + ":FORM?");
                    e_SFormat Format = (e_SFormat)Enum.Parse(typeof(e_SFormat), tmp);
                    return (Format);
                }
            }
            public class cFunction : cCommonFunction
            {

                public cFunction(FormattedIO488 parse)
                    : base(parse)
                {

                }
                //public virtual string Points()
                //{
                //    return (ReadCommand("CALC1:FUNC:POIN?"));
                //}
                //public virtual string Points(int ChannelNumber)
                //{
                //    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":FUNC:POIN?"));
                //}
                public virtual int Points()
                {
                    return (Convert.ToInt32(ReadCommand("CALC1:FUNC:POIN?")));
                }
                public virtual int Points(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("CALC" + ChannelNumber.ToString() + ":FUNC:POIN?")));
                }
            }

            #region "Selected"
            public class cData : cCommonFunction
            {
                public cData(FormattedIO488 parse) : base(parse) { }

                public virtual void ParallelMode(bool bEnable)
                {
                   
                }
                public virtual double[] SData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:SDAT?"));
                }
                public virtual double[] SData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:SDAT?"));
                }
                public virtual double[] FData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:FDAT?"));
                }
                public virtual double[] FData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:FDAT?"));
                }
                public virtual double[] SMemoryData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:SMEM?"));
                }
                public virtual double[] SMemoryData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:SMEM?"));
                }
                public virtual double[] FMemoryData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:FMEM?"));
                }
                public virtual double[] FMemoryData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:FMEM?"));
                }
                public virtual double[] FMultiTrace_Data(string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:MFD? \"" + TraceNumber + "\""));
                }
                public virtual double[] FMultiTrace_Data(int ChannelNumber, string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MFD? \"" + TraceNumber + "\""));
                }
                public virtual double[] UMultiTrace_Data(string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:MSD? \"" + TraceNumber + "\""));
                }
                public virtual double[] UMultiTrace_Data(int ChannelNumber, string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MSD? \"" + TraceNumber + "\""));
                }

                public double[] FM;
                public virtual double[] CAllSData(int ChannelNumber)
                {
                    return null;
                }
                public virtual double[] CAllFData(int ChannelNumber)
                {
                    return null;
                }
                public virtual double[] AllFData(int ChannelNumber)
                {
                    return null;
                }
                public virtual double[] AllSData(int ChannelNumber)
                {
                    return null;
                }
                public virtual string CAllCat(int ChannelNumber)
                {
                    
                    return String.Empty;
                }
            }
            #endregion
        }
        public class cDisplay : cCommonFunction
        {
            public cWindow Window;
            public cDisplay(FormattedIO488 parse)
                : base(parse)
            {
                Window = new cWindow(parse);
            }
            public virtual void Enable(bool state)
            {
                switch (state)
                {
                    case true:
                        SendCommand("DISP:ENAB ON");
                        break;
                    case false:
                        SendCommand("DISP:ENAB OFF");
                        break;
                }
            }
            public virtual void Update(bool state)
            {
                switch (state)
                {
                    case true:
                        SendCommand("DISP:ENAB ON");
                        break;
                    case false:
                        SendCommand("DISP:ENAB OFF");
                        break;
                }
            }
            public class cWindow : cCommonFunction
            {
                public cWindow(FormattedIO488 parse)
                    : base(parse)
                {

                }
                public virtual void Activate(int ChannelNumber)
                {
                    SendCommand("DISP:WIND" + ChannelNumber.ToString() + ":ACT");
                }
                public virtual void Window_Layout(string layout)
                {
                    SendCommand("DISP:SPL " + layout);
                }
                public virtual void Channel_Max(bool state)
                {
                    switch (state)
                    {
                        case true:
                            SendCommand("DISP:MAX ON");
                            break;
                        case false:
                            SendCommand("DISP:MAX OFF");
                            break;
                    }
                }
            }
        }
        public class cSystem : cCommonFunction
        {
            public cSystem(FormattedIO488 parse)
                : base(parse)
            {
            }
            public virtual void Preset()
            {
                SendCommand("SYST:PRES");
            }
        }
        public class cFormat : cCommonFunction
        {
            public cFormat(FormattedIO488 parse) : base(parse) { }
            public virtual void Border(e_Format format)
            {
                SendCommand("FORM:BORD " + format.ToString());
            }
            public virtual void DATA(e_FormatData DataFormat)
            {
                SendCommand("FORM:DATA " + DataFormat.ToString());
            }
        }
        public class cInitiate : cCommonFunction
        {
            public cInitiate(FormattedIO488 parse) : base(parse) { }
            public virtual void Immediate()
            {
                SendCommand("INIT:IMM");
            }
            public virtual void Immediate(int ChannelNumber)
            {
                SendCommand("INIT" + ChannelNumber.ToString() + ":IMM");
            }
            public virtual void Continuous(bool enable)
            {
                switch (enable)
                {
                    case true:
                        SendCommand("INIT:CONT ON");
                        break;
                    case false:
                        SendCommand("INIT:CONT OFF");
                        break;
                }
            }
            public virtual void Continuous(int ChannelNumber, bool enable)
            {
                switch (enable)
                {
                    case true:
                        SendCommand("INIT" + ChannelNumber.ToString() + ":CONT ON");
                        break;
                    case false:
                        SendCommand("INIT" + ChannelNumber.ToString() + ":CONT OFF");
                        break;
                }
            }
        }
        public class cMemory : cCommonFunction
        {
            public cLoad Load;
            public cStore Store;
            public cMemory(FormattedIO488 parse)
                : base(parse)
            {
                Load = new cLoad(parse);
                Store = new cStore(parse);
            }
            public class cLoad : cCommonFunction
            {
                public cLoad(FormattedIO488 parse) : base(parse) { }
                public virtual void State(string StateFile)
                {
                    SendCommand("MMEM:LOAD:STAT \"" + StateFile.Trim() + "\"");
                }
                public virtual void State(int ChannelNumber, string StateFile)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR1:SEL");
                    SendCommand("MMEM:LOAD:STAT \"" + StateFile.Trim() + "\"");
                }
            }
            public class cStore : cCommonFunction
            {
                public cSNP SNP;
                public cStore(FormattedIO488 parse)
                    : base(parse)
                {
                    SNP = new cSNP(parse);
                }
                public class cSNP : cCommonFunction
                {
                    public cSNPType Type;
                    public cSNP(FormattedIO488 parse)
                        : base(parse)
                    {
                        Type = new cSNPType(parse);
                    }
                    public virtual void Data(int Channel, string Filename)
                    {
                        //Not supported by ENA
                    }
                    public virtual void Data(string Filename)
                    {
                        SendCommand("MMEM:STOR:SNP:DATA \"" + Filename.Trim() + "\"");
                    }
                    public virtual void Format(e_SNPFormat format)
                    {
                        SendCommand("MMEM:STOR:SNP:FORM " + format.ToString());
                    }
                    public class cSNPType : cCommonFunction
                    {
                        public cSNPType(FormattedIO488 parse) : base(parse) { }
                        public virtual void S1P(int Port1)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString());
                        }
                        public virtual void S2P(int Port1, int Port2)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void S3P(int Port1, int Port2, int Port3)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public virtual void S4P(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                }
                public virtual void SType(e_SType sType)
                {
                    SendCommand("MMEM:STOR:STYP " + sType.ToString());
                }
                public virtual void State(string Filename)
                {
                    SendCommand("MMEM:STOR:STAT \"" + Filename.Trim() + "\"");
                }
                public virtual void State(int ChannelNumber, string Filename)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR1:SEL");
                    SendCommand("MMEM:STOR:STAT \"" + Filename.Trim() + "\"");
                }
                public virtual void Transfer(string Filename, string Block)
                {
                    SendCommand("MMEM:STOR:TRAN \"" + Filename.Trim() + "\"," + Block.Trim());
                }
                public virtual string Transfer(string Filename)
                {
                    return (ReadCommand("MMEM:STOR:TRAN? \"" + Filename.Trim() + "\""));
                }
            }
        }
        public class cSense : cCommonFunction
        {
            public cMultiplexer Multiplexer;
            public cCorrection Correction;
            public cFrequency Frequency;
            public cSegment Segment;
            public cSweep Sweep;
            public cSense(FormattedIO488 parse)
                : base(parse)
            {
                Multiplexer = new cMultiplexer(parse);
                Correction = new cCorrection(parse);
                Frequency = new cFrequency(parse);
                Segment = new cSegment(parse);
                Sweep = new cSweep(parse);
            }

            public class cMultiplexer : cCommonFunction
            {
                public cMultiplexer(FormattedIO488 parse) : base(parse) { }
                public virtual void Name(int testset_no, string name)
                {
                    SendCommand("SENS:MULT" + testset_no.ToString() + ":NAME " + name.ToUpper());
                }
                public virtual void Name(int ChannelNumber, int testset_no, string name)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":NAME " + name.ToUpper());
                }
                public virtual void State(e_OnOff status, int testset_no)
                {
                    if (status == e_OnOff.On)
                    {
                        SendCommand("SENS:MULT" + testset_no.ToString() + ":STAT ON");
                    }
                    if (status == e_OnOff.Off)
                    {
                        SendCommand("SENS:MULT" + testset_no.ToString() + ":STAT OFF");
                    }
                }
                public virtual void State(int ChannelNumber, e_OnOff status, int testset_no)
                {
                    if (status == e_OnOff.On)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":STAT ON");
                    }
                    if (status == e_OnOff.Off)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":STAT OFF");
                    }
                }
                public virtual void Display(e_OnOff status, int testset_no)
                {
                    if (status == e_OnOff.On)
                    {
                        SendCommand("SENS:MULT" + testset_no.ToString() + ":DISP ON");
                    }
                    if (status == e_OnOff.Off)
                    {
                        SendCommand("SENS:MULT" + testset_no.ToString() + ":DISP OFF");
                    }
                }
                public virtual void Display(int ChannelNumber, e_OnOff status, int testset_no)
                {
                    if (status == e_OnOff.On)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":DISP ON");
                    }
                    if (status == e_OnOff.Off)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":DISP OFF");
                    }
                }
                public virtual void SetPort(int ChannelNumber, int testset_no, int port_no, string label)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":PORT" + port_no.ToString() + " " + label.ToUpper());
                }
                public virtual void SetCtrl_Voltage(int ChannelNumber, int testset_no, string ctrl, double config)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":OUTP:" + ctrl + ":VOLT " + config.ToString());
                }
                public virtual void SetCtrl_HiLo(int ChannelNumber, int testset_no, string ctrl, double config)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":OUTP:" + ctrl + ":DATA " + config.ToString());
                }
            }
            public class cCorrection : cCommonFunction
            {
                public cCollect Collect;
                public cCorrection(FormattedIO488 parse)
                    : base(parse)
                {
                    Collect = new cCollect(parse);
                }
                public class cCollect : cCommonFunction
                {
                    public cAcquire Acquire;
                    public cECAL ECAL;
                    public cMethod Method;
                    public cCalkit Cal_Kit;
                    public cPortExt PortExt;

                    public cCollect(FormattedIO488 parse)
                        : base(parse)
                    {
                        Acquire = new cAcquire(parse);
                        ECAL = new cECAL(parse);
                        Method = new cMethod(parse);
                        Cal_Kit = new cCalkit(parse);
                        PortExt = new cPortExt(parse);
                    }
                    public class cPortExt : cCommonFunction
                    {
                        public cPortExt(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public virtual void State(bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":SENS1:CORR:EXT ON");
                                    break;
                                case false:
                                    SendCommand(":SENS1:CORR:EXT OFF");
                                    break;
                            }
                        }
                        public virtual void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT ON");
                                    break;
                                case false:
                                    SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT OFF");
                                    break;
                            }
                        }
                        public virtual bool State()
                        {
                            return (common.CStr2Bool(ReadCommand(":SENS1:CORR:EXT?")));
                        }
                        public virtual bool State(int ChannelNumber)
                        {
                            return (common.CStr2Bool(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT?")));
                        }
                        public virtual void Loss1(e_OnOff status, int port, int ChannelNumber)
                        {
                            if (status == e_OnOff.On)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":INCL1 ON");
                            }
                            if (status == e_OnOff.Off)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":INCL1 OFF");
                            }
                        }
                        public virtual void Loss1(double loss, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":LOSS1 " + loss.ToString());
                        }
                        public virtual void Loss2(e_OnOff status, int port, int ChannelNumber)
                        {
                            if (status == e_OnOff.On)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":INCL2 ON");
                            }
                            if (status == e_OnOff.Off)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":INCL2 OFF");
                            }
                        }
                        public virtual void Loss2(double loss, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":LOSS2 " + loss.ToString());
                        }
                        public virtual void Freq1(double freq, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":FREQ1 " + freq.ToString());
                        }
                        public virtual void Freq2(double freq, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":FREQ2 " + freq.ToString());
                        }
                        public virtual void Ext(double delay, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + " " + delay.ToString());
                        }
                        public virtual void LossDC(double loss, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":LDC " + loss.ToString());
                        }
                    }
                    public class cAcquire : cCommonFunction
                    {
                        public cAcquire(FormattedIO488 parse) : base(parse) { }
                        public virtual void Isolation(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:ISOL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void Isolation(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:ISOL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void Load(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:LOAD " + Port.ToString());
                        }
                        public virtual void Load(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:LOAD " + Port.ToString());
                        }
                        public virtual void Open(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:OPEN " + Port.ToString());
                        }
                        public virtual void Open(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:OPEN " + Port.ToString());
                        }
                        public virtual void Short(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SHOR " + Port.ToString());
                        }
                        public virtual void Short(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SHOR " + Port.ToString());
                        }
                        public virtual void Subclass(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SUBC " + Port.ToString());
                        }
                        public virtual void Subclass(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SUBC " + Port.ToString());
                        }
                        public virtual void Thru(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:THRU " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void Thru(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:THRU " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void TRLLine(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:TRLL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void TRLLine(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:TRLL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void TRLReflect(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:TRLR " + Port.ToString());
                        }
                        public virtual void TRLReflect(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:TRLR " + Port.ToString());
                        }
                        public virtual void TRLThru(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:TRLT " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void TRLThru(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:TRLT " + Port1.ToString() + "," + Port2.ToString());
                        }
                    }
                    public class cECAL : cCommonFunction
                    {
                        public cECAL(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        public virtual void SOLT1(int Port1)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT1 " + Port1.ToString());
                        }
                        public virtual void SOLT1(int ChannelNumber, int Port1)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT1 " + Port1.ToString());
                        }
                        public virtual void SOLT2(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void SOLT2(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void SOLT3(int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public virtual void SOLT3(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public virtual void SOLT4(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public virtual void SOLT4(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                    public class cMethod : cCommonFunction
                    {
                        public cMethod(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        public virtual void SOLT1(int Port1)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT1 " + Port1.ToString());
                        }
                        public virtual void SOLT1(int ChannelNumber, int Port1)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT1 " + Port1.ToString());
                        }
                        public virtual void SOLT2(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void SOLT2(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public virtual void SOLT3(int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public virtual void SOLT3(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public virtual void SOLT4(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public virtual void SOLT4(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                    public class cCalkit : cCommonFunction
                    {
                        public cCalStd Cal_Std;
                        public cCalkit(FormattedIO488 parse)
                            : base(parse)
                        {
                            Cal_Std = new cCalStd(parse);
                        }
                        public class cCalStd : cCommonFunction
                        {
                            public cCalStd(FormattedIO488 parse)
                                : base(parse)
                            {
                            }
                            public virtual void Std_Label(int ChannelNumber, int stdno, string name)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":LAB \"" + name + "\"");
                            }
                            public virtual void Std_Type(int ChannelNumber, int stdno, string name)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":TYPE " + name);
                            }
                            public virtual void Std_C0(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C0 " + value.ToString());
                            }
                            public virtual void Std_C1(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C1 " + value.ToString());
                            }
                            public virtual void Std_C2(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C2 " + value.ToString());
                            }
                            public virtual void Std_C3(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C3 " + value.ToString());
                            }
                            public virtual void Std_L0(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L0 " + value.ToString());
                            }
                            public virtual void Std_L1(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L1 " + value.ToString());
                            }
                            public virtual void Std_L2(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L2 " + value.ToString());
                            }
                            public virtual void Std_L3(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L3 " + value.ToString());
                            }
                            public virtual void OffSet_Delay(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":DEL " + value.ToString());
                            }
                            public virtual void Offset_Z0(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":Z0 " + value.ToString());
                            }
                            public virtual void Offset_Loss(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":LOSS " + value.ToString());
                            }
                            public virtual void ArbImp(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":ARB " + value.ToString());
                            }
                            public virtual void MinFreq(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":FMIN " + value.ToString());
                            }
                            public virtual void MaxFreq(int ChannelNumber, int stdno, double value)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":FMAX " + value.ToString());
                            }
                            public virtual void Media(int ChannelNumber, int stdno, string name)
                            {
                                SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":CHAR " + name);
                            }
                        }
                        public virtual void Cal_Kit(int Number_CalKit)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT " + Number_CalKit.ToString());
                        }
                        public virtual void Cal_Kit(int ChannelNumber, int Number_CalKit)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT " + Number_CalKit.ToString());
                        }
                        public virtual int Cal_Kit()
                        {
                            return (int.Parse(ReadCommand(":SENS1:CORR:COLL:CKIT?")));
                        }
                        //public virtual int Cal_Kit(int ChannelNumber)
                        //{
                        //    return(int.Parse(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT?")));
                        //}
                        public virtual void Label(string name)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:LAB \"" + name + "\"");
                        }
                        public virtual void Label(int ChannelNumber, string name)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:LAB \"" + name + "\"");
                        }
                        public virtual string Label()
                        {
                            return (ReadCommand(":SENS1:CORR:COLL:CKIT:LAB?"));
                        }
                        public virtual string Label(int ChannelNumber)
                        {
                            return (ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:LAB?"));
                        }
                        public virtual void Order(int SubClass)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT " + SubClass.ToString());
                        }
                        public virtual void Order(int ChannelNumber, int SubClass)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT " + SubClass.ToString());
                        }
                        public virtual int Order()
                        {
                            return (int.Parse(ReadCommand(":SENS1:CORR:COLL:CKIT?")));
                        }
                        //public virtual int Order(int ChannelNumber)
                        //{
                        //    return(int.Parse(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT?")));
                        //}
                        public virtual void Select_SubClass(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:SUBC " + ChannelNumber.ToString());
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:SEL " + ChannelNumber.ToString());
                        }
                        public virtual void Open(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:OPEN " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void SubClass_Open(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:OPEN " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void Open(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:OPEN " + Port_Number.ToString());
                        }
                        public virtual void Short(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:SHOR " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void SubClass_Short(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:SHOR " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void Short(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:SHOR " + Port_Number.ToString());
                        }
                        public virtual void Load(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:LOAD " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void SubClass_Load(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:LOAD " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void Load(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:LOAD " + Port_Number.ToString());
                        }
                        public virtual void Thru(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:THRU " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void SubClass_Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:THRU " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:THRU " + Port_Number_1.ToString() + "," + Port_Number_2.ToString());
                        }
                        public virtual void TRL_Line(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLL " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void TRL_Line(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:TRLL " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void TRL_Reflect(int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLR " + Standard_Number.ToString());
                        }
                        public virtual void TRL_Reflect(int ChannelNumber, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:TRLR " + Standard_Number.ToString());
                        }
                        public virtual void TRL_Thru(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLT " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public virtual void TRL_Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:TRLT " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                    }
                    public virtual void Save(int ChannelNumber)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:SAVE");
                    }
                }
                public virtual void Property(bool enable)
                {
                    switch (enable)
                    {
                        case true:
                            SendCommand("SENS:CORR:PROP ON");
                            break;
                        case false:
                            SendCommand("SENS:CORR:PROP OFF");
                            break;
                    }
                }
                public virtual void Property(int ChannelNumber, bool enable)
                {
                    switch (enable)
                    {
                        case true:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:PROP ON");
                            break;
                        case false:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:PROP OFF");
                            break;
                    }
                }
                public virtual void Clear(int ChannelNumber)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:CLE");
                }
                public virtual void State(bool enable)
                {
                    switch (enable)
                    {
                        case true:
                            SendCommand("SENS:CORR:PROP ON");
                            break;
                        case false:
                            SendCommand("SENS:CORR:PROP OFF");
                            break;
                    }
                }
            }
            public class cFrequency : cCommonFunction
            {
                public cFrequency(FormattedIO488 parse) : base(parse) { }
                public virtual void Center(double Freq)
                {
                    SendCommand("SENS:FREQ:CENT " + Freq.ToString());
                }
                public virtual void Center(string Freq)
                {
                    SendCommand("SENS:FREQ:CENT " + common.convertStr2Val(Freq));
                }
                public virtual void CW(double Freq)
                {
                    SendCommand("SENS:FREQ:CW " + Freq.ToString());
                }
                public virtual void Fixed(double Freq)
                {
                    SendCommand("SENS:FREQ:FIX " + Freq.ToString());
                }
                public virtual void SPAN(double BW)
                {
                    SendCommand("SENS:FREQ:SPAN " + BW.ToString());
                }
                public virtual void Start(double Freq)
                {
                    SendCommand("SENS:FREQ:STAR " + Freq.ToString());
                }
                public virtual void Start(int ChannelNumber, double Freq)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STAR " + Freq.ToString());
                }
                public virtual double Start()
                {
                    return (Convert.ToDouble(ReadCommand("SENS:FREQ:STAR?")));
                }
                public virtual double Start(int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STAR?")));
                }
                public virtual void Stop(double Freq)
                {
                    SendCommand("SENS:FREQ:STOP " + Freq.ToString());
                }
                public virtual void Stop(int ChannelNumber, double Freq)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STOP " + Freq.ToString());
                }
                public virtual double Stop()
                {
                    return (Convert.ToDouble(ReadCommand("SENS:FREQ:STOP?")));
                }
                public virtual double Stop(int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STOP?")));
                }
                public virtual double[] FreqList()
                {
                    return (ReadIEEEBlock("SENS:FREQ:DATA?"));
                }
                public virtual double[] FreqList(int ChannelNumber)
                {
                    return (ReadIEEEBlock("SENS" + ChannelNumber.ToString() + ":FREQ:DATA?"));
                }
                public virtual void Band(int ChannelNumber, double BW)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":BAND " + BW.ToString());
                }
            }
            public class cSegment : cCommonFunction
            {
                public cSegment(FormattedIO488 parse) : base(parse) { }
                public virtual void Data(string SegmentData, e_OnOff sweepmode)
                {
                    switch (sweepmode)
                    {
                        case e_OnOff.On:
                            SendCommand("SENS:SEGM:DATA 6," + SegmentData.Trim());
                            break;
                        case e_OnOff.Off:
                            SendCommand("SENS:SEGM:DATA 5," + SegmentData.Trim());
                            break;
                    }
                }
                public virtual void Data(int ChannelNumber, string SegmentData, e_OnOff sweepmode)
                {
                    switch (sweepmode)
                    {
                        case e_OnOff.On:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 6," + SegmentData.Trim());
                            break;
                        case e_OnOff.Off:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 5," + SegmentData.Trim());
                            break;
                    }
                }
                public virtual void Data(s_SegmentTable SegmentData, e_OnOff sweepmode)
                {
                    switch (sweepmode)
                    {
                        case e_OnOff.On:
                            SendCommand("SENS:SEGM:DATA 6," + Convert_SegmentTable2String(SegmentData, sweepmode));
                            break;
                        case e_OnOff.Off:
                            SendCommand("SENS:SEGM:DATA 5," + Convert_SegmentTable2String(SegmentData, sweepmode));
                            break;
                    }
                }
                public virtual void Data(int ChannelNumber, s_SegmentTable SegmentData, e_OnOff sweepmode)
                {
                    switch (sweepmode)
                    {
                        case e_OnOff.On:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 6," + Convert_SegmentTable2String(SegmentData, sweepmode));
                            break;
                        case e_OnOff.Off:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 5," + Convert_SegmentTable2String(SegmentData, sweepmode));
                            break;
                    }
                }
                public virtual s_SegmentTable Data(int ChannelNumber)
                {
                    string DataFormat;
                    string tmpStr;
                    string[] tmpSegData;
                    long tmpI;
                    int iData = 3;

                    s_SegmentTable ST = new s_SegmentTable();
                    DataFormat = ReadCommand("FORM:DATA?");
                    SendCommand("FORM:DATA ASC");
                    tmpStr = ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA?");
                    tmpSegData = tmpStr.Split(',');

                    for (int s = 0; s < tmpSegData.Length; s++)
                    {
                        tmpI = (long)(Convert.ToDouble(tmpSegData[s]));
                        tmpSegData[s] = tmpI.ToString();
                    }

                    switch (tmpSegData[0])
                    {
                        case "5":
                            ST.mode = (e_ModeSetting)Enum.Parse(typeof(e_ModeSetting), tmpSegData[1]);
                            ST.ifbw = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[2]);
                            ST.pow = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[3]);
                            ST.del = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[4]);
                            ST.time = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[5]);
                            ST.segm = int.Parse(tmpSegData[6]);
                            ST.swp = e_OnOff.Off;
                            ST.SegmentData = new s_SegmentData[ST.segm];
                            for (int iSeg = 0; iSeg < ST.segm; iSeg++)
                            {
                                ST.SegmentData[iSeg].Start = double.Parse(tmpSegData[(iSeg * iData) + 7]);
                                ST.SegmentData[iSeg].Stop = double.Parse(tmpSegData[(iSeg * iData) + 8]);
                                ST.SegmentData[iSeg].Points = int.Parse(tmpSegData[(iSeg * iData) + 9]);
                                tmpI = 10;
                                if (ST.ifbw == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].ifbw_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.pow == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].pow_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.del == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].del_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.time == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].time_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                            }
                            break;
                        case "6":
                            ST.mode = (e_ModeSetting)Enum.Parse(typeof(e_ModeSetting), tmpSegData[1]);
                            ST.ifbw = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[2]);
                            ST.pow = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[3]);
                            ST.del = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[4]);
                            ST.swp = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[5]);
                            ST.time = (e_OnOff)Enum.Parse(typeof(e_OnOff), tmpSegData[6]);
                            ST.segm = int.Parse(tmpSegData[7]);
                            ST.SegmentData = new s_SegmentData[ST.segm];
                            for (int iSeg = 0; iSeg < ST.segm; iSeg++)
                            {
                                ST.SegmentData[iSeg].Start = double.Parse(tmpSegData[(iSeg * iData) + 8]);
                                ST.SegmentData[iSeg].Stop = double.Parse(tmpSegData[(iSeg * iData) + 9]);
                                ST.SegmentData[iSeg].Points = int.Parse(tmpSegData[(iSeg * iData) + 10]);
                                tmpI = 10;
                                if (ST.ifbw == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].ifbw_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.pow == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].pow_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.del == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].del_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.swp == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].swp_value = (e_SweepMode)Enum.Parse(typeof(e_SweepMode), tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                if (ST.time == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].time_value = double.Parse(tmpSegData[(iSeg * iData) + tmpI]);
                                    tmpI++;
                                    if (iSeg == 0) iData++;
                                }
                                //    ST.SegmentData[iSeg].ifbw_value = double.Parse(tmpSegData[(iSeg * 8) + 11]);
                                //    ST.SegmentData[iSeg].pow_value = double.Parse(tmpSegData[(iSeg * 8) + 12]);
                                //    ST.SegmentData[iSeg].del_value = double.Parse(tmpSegData[(iSeg * 8) + 13]);
                                //    ST.SegmentData[iSeg].swp_value = (e_SweepMode)Enum.Parse(typeof(e_SweepMode), tmpSegData[(iSeg * 8) + 14]);
                                //    ST.SegmentData[iSeg].time_value = double.Parse(tmpSegData[(iSeg * 8) + 15]);
                            }

                            break;
                    }
                    SendCommand("FORM:DATA " + DataFormat);

                    return (ST);
                }
                public virtual string SweepPoints()
                {
                    return (ReadCommand("SENS:SEGM:SWE:POIN?"));
                }
                public virtual string SweepPoints(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:SWE:POIN?"));
                }
                public virtual string SweepTime()
                {
                    return (ReadCommand("SENS:SEGM:SWE:TIME:DATA?"));
                }
                public virtual string SweepTime(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:SWE:TIME:DATA?"));
                }
            }
            public class cSweep : cCommonFunction
            {
                public cSweepTime Time;
                public cSweep(FormattedIO488 parse)
                    : base(parse)
                {
                    Time = new cSweepTime(parse);
                }
                public virtual void ASPurious(e_OnOff State)
                {
                    SendCommand("SENS:SWE:ASP " + State.ToString());
                }
                public virtual void Delay(double delay)
                {
                    SendCommand("SENS:SWE:DEL " + delay.ToString());
                }
                public virtual void Generation(e_SweepGeneration SweepGen)
                {
                    SendCommand("SENS:SWE:GEN " + SweepGen.ToString());
                }
                //public virtual void Points(int points)
                //{
                //    SendCommand("SENS:SWE:POIN " + points.ToString());
                //}
                public virtual void Points(int ChannelNumber, int points)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:POIN " + points.ToString());
                }
                public virtual int Points()
                {
                    return (Convert.ToInt32(ReadCommand("SENS:SWE:POIN?")));
                }
                public virtual int Points(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + ChannelNumber.ToString() + ":SWE:POIN?")));
                }
                public class cSweepTime : cCommonFunction
                {
                    public cSweepTime(FormattedIO488 parse) : base(parse) { }
                    public virtual void Auto(e_OnOff state)
                    {
                        SendCommand("SENS:SWE:TIME:AUTO " + state.ToString());
                    }
                    public virtual void Data(double time)
                    {
                        SendCommand("SENS:SWE:TIME:DATA " + time.ToString());
                    }
                }
                public virtual void Type(e_SweepType SweepType)
                {
                    SendCommand("SENS:SWE:TYPE " + SweepType.ToString());
                }
                public virtual void Type(int ChannelNumber, e_SweepType SweepType)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:TYPE " + SweepType.ToString());
                }
                public virtual e_SweepType Type()
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS:SWE:TYPE?");
                    return ((e_SweepType)Enum.Parse(typeof(e_SweepType), tmpStr.ToUpper()));
                }
                public virtual e_SweepType Type(int ChannelNumber)
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS" + ChannelNumber + ":SWE:TYPE?");
                    return ((e_SweepType)Enum.Parse(typeof(e_SweepType), tmpStr.ToUpper()));
                }
            }
        }
        public class cTrigger : cCommonFunction
        {
            public cTriggerExternal External;
            public cTrigger(FormattedIO488 parse)
                : base(parse)
            {
                External = new cTriggerExternal(parse);
            }
            public virtual void Average(e_OnOff State)
            {
                SendCommand("TRIG:AVER " + State.ToString());
            }
            public class cTriggerExternal : cCommonFunction
            {
                public cTriggerExternal(FormattedIO488 parse) : base(parse) { }
                public virtual void Delay(double delay)
                {
                    SendCommand("TRIG:EXT:DEL " + delay.ToString());
                }
                public virtual void LLatency(e_OnOff state)
                {
                    SendCommand("TRIG:EXT:LLAT " + state.ToString());
                }
            }
            public virtual void Immediate()
            {
                SendCommand("TRIG:SEQ:IMM");
            }
            public virtual void Point(e_OnOff state)
            {
                SendCommand("TRIG:SEQ:POIN " + state.ToString());
            }
            public virtual void Single(int channel)
            {
                //// SendCommand("TRIG:SEQ:SING");
                ////KCC
                SendCommand("INIT" + channel);
                SendCommand("TRIG:SING");
            }
            public virtual void Single()
            {
                SendCommand("TRIG:SEQ:SING");
            }
            public virtual void Scope(e_TriggerScope Scope)
            {
                SendCommand("TRIG:SEQ:SCOP " + Scope.ToString());
            }
            public virtual void Source(e_TriggerSource Source)
            {
                SendCommand("TRIG:SEQ:SOUR " + Source.ToString());
            }
        }


        #endregion
    }
}
