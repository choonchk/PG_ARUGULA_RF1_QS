using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR
{
    public class cPowerMeter : iCommonFunction
    {
        public static string ClassName = "Power Meter Class";
        private string IOAddress;
        private FormattedIO488 ioPM;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();
        
        #region "Enumeration Declaration"
        public enum MeasurementWindow
        {
            // +----------------------+
            // | Upper Measurement    |
            // |  +----------------+  |
            // |  | Upper Window   |  |
            // |  +----------------+  |
            // |  | Lower Window   |  |
            // |  +----------------+  |
            // +======================+
            // | Lower Measurement    |
            // |  (Dual Channel Only) |
            // |  +----------------+  |
            // |  | Upper Window   |  |
            // |  +----------------+  |
            // |  | Lower Window   |  |
            // |  +----------------+  |
            // +----------------------+

            UpperMeasurementUpperWindow = 1,
            UpperMeasurementLowerWindow = 2,
            LowerMeasurementUpperWindow = 3,
            LowerMeasurementLowerWindow = 4
        }
        public enum MathExpressionEnum
        {
            Sense1 = 0, //“(SENS1)”
            Sense2 = 1, //“(SENS2)”
            Sense1MSense1 = 2, //“(SENS1-SENS1)”
            Sense2MSense2 = 3, //“(SENS2-SENS2)”
            Sense1DSense1 = 4, //“(SENS1/SENS1)"
            Sense2DSense2 = 5, //“(SENS2/SENS2)”
            Sense1MSense2 = 6, //“(SENS1-SENS2)”
            Sense2MSense1 = 7, //“(SENS2-SENS1)”
            Sense1DSense2 = 8, //“(SENS1/SENS2)”
            Sense2DSense1 = 9 //“(SENS2/SENS1)”
        }
        public enum ScreenFormat
        {
            Windowed = 0,
            Expanded = 1,
            FullScreen = 2
        }
        public enum WindowFormat
        {
            Digital = 0,
            Analog = 1,
            Snumeric = 2,
            Dnumeric = 3,
            Trace = 4
        }
        public enum ByteOrder
        {
            Normal = 0,
            Swapped = 1
        }
        public enum Character_Data
        {
            ASCii = 0,
            REAL = 1
        }
        public enum Video_Bandwidth
        {
            High = 0,
            Medium = 1,
            Low = 2,
            Off = 3
        }
        public enum Measurement_Mode
        {
            Average = 0,
            Normal = 1
        }
        public enum Measurement_Speed
        {
            NormalSpeed = 20,
            DoubleSpeed = 40,
            FastSpeed = 200
        }
        public enum Unit_Power
        {
            dBm = 0,
            Watts = 1
        }
        public enum PresetData
        {
            Def = 0,
            GSM900 = 1,
            EDGE = 2,
            NADC = 3,
            BLUetooth = 4,
            CDMAone = 5,
            WCDMA = 6,
            CDMA2000 = 7,
            IDEN = 8
        }
        public enum Remote_Interface
        {
            GPIB = 0,
            RS232 = 1,
            RS422 = 2
        }
        public enum TraceResolution
        {
            HighRES = 0,
            MediumRES = 1,
            LowRES = 2
        }
        public enum Trigger_Source
        {
            BUS = 0,
            EXTernal = 1,
            HOLD = 2,
            IMMediate = 3,
            INTernal1 = 4,
            INTernal2 = 5
        }
        public enum Unit_Ratio
        {
            DB = 0,
            PCT = 1
        }
        public enum HighLow
        {
            Low = 0,
            High = 1
        }
        public enum UpperLower
        {
            Lower = 0,
            Upper = 1
        }
        public enum Linearity_Correction_Type
        {
            A_Type = 0,
            D_Type = 1
        }
        #endregion
        #region "Structure"
        public struct Memory_Catalog
        {
            public double MemoryUsed;
            public double MemoryAvailable;
            public string[] NameStr;
            public string[] Type;
            public double[] Size;
        }
        public struct Serial_Settings
        {
            public bool DTR;
            public bool RTS;
            public int BAUD;
            public int BITs;
            public string PACE;
            public string PARity;
            public int SBITs;
            public int Auto;
            public int TransmitBAUD;
            public bool TransmitECHO;
        }
        #endregion
        #region "Conversion Function"
        public static string CMathExpression2Str(MathExpressionEnum MathExpr)
        {
            string returnValue = "";
            switch (MathExpr)
            {
                case MathExpressionEnum.Sense1:
                    returnValue = "\"(SENS1)\"";
                    break;
                case MathExpressionEnum.Sense2:
                    returnValue = "\"(SENS2)\"";
                    break;
                case MathExpressionEnum.Sense1MSense1:
                    returnValue = "\"(SENS1-SENS1)\"";
                    break;
                case MathExpressionEnum.Sense2MSense2:
                    returnValue = "\"(SENS2-SENS2)\"";
                    break;
                case MathExpressionEnum.Sense1DSense1:
                    returnValue = "\"(SENS1/SENS1)\"";
                    break;
                case MathExpressionEnum.Sense2DSense2:
                    returnValue = "\"(SENS2/SENS2)\"";
                    break;
                case MathExpressionEnum.Sense1MSense2:
                    returnValue = "\"(SENS1-SENS2)\"";
                    break;
                case MathExpressionEnum.Sense2MSense1:
                    returnValue = "\"(SENS2-SENS1)\"";
                    break;
                case MathExpressionEnum.Sense1DSense2:
                    returnValue = "\"(SENS1/SENS2)\"";
                    break;
                case MathExpressionEnum.Sense2DSense1:
                    returnValue = "\"(SENS2/SENS1)\"";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized MathExpression Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static MathExpressionEnum CStr2MathExpression(string InputStr)
        {
            MathExpressionEnum returnValue = 0;
            switch (InputStr)
            {
                case "(SENS1)":
                    returnValue = MathExpressionEnum.Sense1;
                    break;
                case "\"(SENS2)\"":
                    returnValue = MathExpressionEnum.Sense2;
                    break;
                case "\"(SENS1-SENS1)\"":
                    returnValue = MathExpressionEnum.Sense1MSense1;
                    break;
                case "\"(SENS2-SENS2)\"":
                    returnValue = MathExpressionEnum.Sense2MSense2;
                    break;
                case "\"(SENS1/SENS1)\"":
                    returnValue = MathExpressionEnum.Sense1DSense1;
                    break;
                case "\"(SENS2/SENS2)\"":
                    returnValue = MathExpressionEnum.Sense2DSense2;
                    break;
                case "\"(SENS1-SENS2)\"":
                    returnValue = MathExpressionEnum.Sense1MSense2;
                    break;
                case "\"(SENS2-SENS1)\"":
                    returnValue = MathExpressionEnum.Sense2MSense1;
                    break;
                case "\"(SENS1/SENS2)\"":
                    returnValue = MathExpressionEnum.Sense1DSense2;
                    break;
                case "\"(SENS2/SENS1)\"":
                    returnValue = MathExpressionEnum.Sense2DSense1;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized MathExpression String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CScreenFormat2Str(ScreenFormat ScreenFmt)
        {
            string returnValue = "";
            switch (ScreenFmt)
            {
                case ScreenFormat.Windowed:
                    returnValue = "WIND";
                    break;
                case ScreenFormat.Expanded:
                    returnValue = "EXP";
                    break;
                case ScreenFormat.FullScreen:
                    returnValue = "FSCR";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Screen Format Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static ScreenFormat CStr2ScreenFormat(string ScreenStr)
        {
            ScreenFormat returnValue = 0;
            switch (ScreenStr.ToUpper())
            {
                case "WIND":
                    returnValue = ScreenFormat.Windowed;
                    break;
                case "EXP":
                    returnValue = ScreenFormat.Expanded;
                    break;
                case "FSCR":
                    returnValue = ScreenFormat.FullScreen;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Screen Format String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CWindowFormat2Str(WindowFormat WinFormat)
        {
            string returnValue = "";
            switch (WinFormat)
            {
                case WindowFormat.Digital:
                    returnValue = "DIG";
                    break;
                case WindowFormat.Analog:
                    returnValue = "ANAL";
                    break;
                case WindowFormat.Snumeric:
                    returnValue = "SNUM";
                    break;
                case WindowFormat.Dnumeric:
                    returnValue = "DNUM";
                    break;
                case WindowFormat.Trace:
                    returnValue = "TRAC";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Window Format Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static WindowFormat CStr2WindowFormat(string WinFormatStr)
        {
            WindowFormat returnValue = 0;
            switch (WinFormatStr)
            {
                case "DIG":
                    returnValue = WindowFormat.Digital;
                    break;
                case "ANAL":
                    returnValue = WindowFormat.Analog;
                    break;
                case "SNUM":
                    returnValue = WindowFormat.Snumeric;
                    break;
                case "DNUM":
                    returnValue = WindowFormat.Dnumeric;
                    break;
                case "TRAC":
                    returnValue = WindowFormat.Trace;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Window Format String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CByteOrder2Str(ByteOrder ByteOrd)
        {
            string returnValue = "";
            switch (ByteOrd)
            {
                case ByteOrder.Normal:
                    returnValue = "NORM";
                    break;
                case ByteOrder.Swapped:
                    returnValue = "SWAP";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Byte Order Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static ByteOrder CStr2ByteOrder(string InputStr)
        {
            ByteOrder returnValue = 0;
            switch (InputStr.ToUpper())
            {
                case "NORM":
                    returnValue = ByteOrder.Normal;
                    break;
                case "SWAP":
                    returnValue = ByteOrder.Swapped;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Byte Order String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CCharacterData2Str(Character_Data CharacData)
        {
            string returnValue = "";
            switch (CharacData)
            {
                case Character_Data.ASCii:
                    returnValue = "ASC";
                    break;
                case Character_Data.REAL:
                    returnValue = "REAL";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Character Data Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Character_Data CStr2CharacterData(string InputStr)
        {
            Character_Data returnValue = 0;
            switch (InputStr.ToUpper())
            {
                case "ASC":
                    returnValue = Character_Data.ASCii;
                    break;
                case "REAL":
                    returnValue = Character_Data.REAL;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Character Data String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CVideoBandwidth2Str(Video_Bandwidth VideoBand)
        {
            string returnValue = "";
            switch (VideoBand)
            {
                case Video_Bandwidth.High:
                    returnValue = "HIGH";
                    break;
                case Video_Bandwidth.Medium:
                    returnValue = "MED";
                    break;
                case Video_Bandwidth.Low:
                    returnValue = "LOW";
                    break;
                case Video_Bandwidth.Off:
                    returnValue = "OFF";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Video Bandwidth Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Video_Bandwidth CStr2VideoBandwidth(string InputStr)
        {
            Video_Bandwidth returnValue = 0;
            switch (InputStr.ToUpper())
            {
                case "HIGH":
                    returnValue = Video_Bandwidth.High;
                    break;
                case "MED":
                    returnValue = Video_Bandwidth.Medium;
                    break;
                case "LOW":
                    returnValue = Video_Bandwidth.Low;
                    break;
                case "OFF":
                    returnValue = Video_Bandwidth.Off;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Video Bandwidth String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CMeasMode2Str(Measurement_Mode MeasMode)
        {
            string returnValue = "";
            switch (MeasMode)
            {
                case Measurement_Mode.Average:
                    returnValue = "AVER";
                    break;
                case Measurement_Mode.Normal:
                    returnValue = "NORM";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Measurement Mode Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Measurement_Mode CStr2MeasMode(string MeasStr)
        {
            Measurement_Mode returnValue = 0;
            switch (MeasStr.ToUpper())
            {
                case "AVER":
                    returnValue = Measurement_Mode.Average;
                    break;
                case "NORM":
                    returnValue = Measurement_Mode.Normal;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Measurement Mode String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CMeasRate2Str(Measurement_Speed MeasSpeed)
        {
            string returnValue = "";
            switch (MeasSpeed)
            {
                case Measurement_Speed.NormalSpeed:
                    returnValue = "NORM";
                    break;
                case Measurement_Speed.DoubleSpeed:
                    returnValue = "DOUB";
                    break;
                case Measurement_Speed.FastSpeed:
                    returnValue = "FAST";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Measurement Rate Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Measurement_Speed CStr2MeasRate(string SpeedStr)
        {
            Measurement_Speed returnValue = 0;
            switch (SpeedStr.ToUpper())
            {
                case "NORM":
                    returnValue = Measurement_Speed.NormalSpeed;
                    break;
                case "DOUB":
                    returnValue = Measurement_Speed.DoubleSpeed;
                    break;
                case "FAST":
                    returnValue = Measurement_Speed.FastSpeed;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Measurement Rate String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CUnit2Str(Unit_Power UnitEnum)
        {
            string returnValue = "";
            switch (UnitEnum)
            {
                case Unit_Power.dBm:
                    returnValue = "DBM";
                    break;
                case Unit_Power.Watts:
                    returnValue = "W";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Unit Power Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Unit_Power CStr2Unit(string UnitStr)
        {
            Unit_Power returnValue = 0;
            switch (UnitStr.ToUpper())
            {
                case "DBM":
                    returnValue = Unit_Power.dBm;
                    break;
                case "W":
                    returnValue = Unit_Power.Watts;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Unit Power String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CPresetData2Str(PresetData PresetDat)
        {
            string returnValue = "";
            switch (PresetDat)
            {
                case PresetData.Def:
                    returnValue = "DEF";
                    break;
                case PresetData.GSM900:
                    returnValue = "GSM900";
                    break;
                case PresetData.EDGE:
                    returnValue = "EDGE";
                    break;
                case PresetData.NADC:
                    returnValue = "NADC";
                    break;
                case PresetData.BLUetooth:
                    returnValue = "BLU";
                    break;
                case PresetData.CDMAone:
                    returnValue = "CDMA";
                    break;
                case PresetData.WCDMA:
                    returnValue = "WCDMA";
                    break;
                case PresetData.CDMA2000:
                    returnValue = "CDMA2000";
                    break;
                case PresetData.IDEN:
                    returnValue = "IDEN";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Preset Data Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static PresetData CStr2PresetData(string PresetStr)
        {
            PresetData returnValue = 0;
            switch (PresetStr.ToUpper())
            {
                case "DEF":
                    returnValue = PresetData.Def;
                    break;
                case "GSM900":
                    returnValue = PresetData.GSM900;
                    break;
                case "EDGE":
                    returnValue = PresetData.EDGE;
                    break;
                case "NADC":
                    returnValue = PresetData.NADC;
                    break;
                case "BLU":
                    returnValue = PresetData.BLUetooth;
                    break;
                case "CDMA":
                    returnValue = PresetData.CDMAone;
                    break;
                case "WCDMA":
                    returnValue = PresetData.WCDMA;
                    break;
                case "CDMA2000":
                    returnValue = PresetData.CDMA2000;
                    break;
                case "IDEN":
                    returnValue = PresetData.IDEN;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Preset Data String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CRemoteInt2Str(Remote_Interface remote)
        {
            string returnValue = "";
            switch (remote)
            {
                case Remote_Interface.GPIB:
                    returnValue = "GPIB";
                    break;
                case Remote_Interface.RS232:
                    returnValue = "RS232";
                    break;
                case Remote_Interface.RS422:
                    returnValue = "RS422";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Remote Interface Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Remote_Interface CStr2RemoteInt(string RemoteStr)
        {
            Remote_Interface returnValue = 0;
            switch (RemoteStr.ToUpper())
            {
                case "GPIB":
                    returnValue = Remote_Interface.GPIB;
                    break;
                case "RS232":
                    returnValue = Remote_Interface.RS232;
                    break;
                case "RS422":
                    returnValue = Remote_Interface.RS422;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Remote Interface String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CTraceRes2Str(TraceResolution TraceRes)
        {
            string returnValue = "";
            switch (TraceRes)
            {
                case TraceResolution.HighRES:
                    returnValue = "HRES";
                    break;
                case TraceResolution.MediumRES:
                    returnValue = "MRES";
                    break;
                case TraceResolution.LowRES:
                    returnValue = "LRES";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Trace Resolution Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CTriggerSource2Str(Trigger_Source TrigSrc)
        {
            string returnValue = "";
            switch (TrigSrc)
            {
                case Trigger_Source.BUS:
                    returnValue = "BUS";
                    break;
                case Trigger_Source.EXTernal:
                    returnValue = "EXT";
                    break;
                case Trigger_Source.HOLD:
                    returnValue = "HOLD";
                    break;
                case Trigger_Source.IMMediate:
                    returnValue = "IMM";
                    break;
                case Trigger_Source.INTernal1:
                    returnValue = "INT1";
                    break;
                case Trigger_Source.INTernal2:
                    returnValue = "INT2";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Trigger Source Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Trigger_Source CStr2TriggerSource(string TrigSrcStr)
        {
            Trigger_Source returnValue = 0;
            switch (TrigSrcStr.ToUpper())
            {
                case "BUS":
                    returnValue = Trigger_Source.BUS;
                    break;
                case "Ext":
                    returnValue = Trigger_Source.IMMediate;
                    break;
                case "HOLD":
                    returnValue = Trigger_Source.HOLD;
                    break;
                case "IMM":
                    returnValue = Trigger_Source.IMMediate;
                    break;
                case "INT1":
                    returnValue = Trigger_Source.INTernal1;
                    break;
                case "INT2":
                    returnValue = Trigger_Source.INTernal2;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Trigger Source String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static string CUnitRatio2Str(Unit_Ratio Ratio)
        {
            string returnValue = "";
            switch (Ratio)
            {
                case Unit_Ratio.DB:
                    returnValue = "DB";
                    break;
                case Unit_Ratio.PCT:
                    returnValue = "PCT";
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Unit Ratio Enum", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static Unit_Ratio CStr2UnitRatio(string RatioStr)
        {
            Unit_Ratio returnValue = 0;
            switch (RatioStr.ToUpper())
            {
                case "DB":
                    returnValue = Unit_Ratio.DB;
                    break;
                case "PCT":
                    returnValue = Unit_Ratio.PCT;
                    break;
                default:
                    common.DisplayError(ClassName, "Unrecognized Unit Ratio String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static HighLow CStr2HighLow(string Input)
        {
            HighLow returnValue;
            switch (Input.ToUpper())
            {
                case "HIGH":
                    returnValue = HighLow.High;
                    break;
                case "LOW":
                    returnValue = HighLow.Low;
                    break;
                default:
                    returnValue = HighLow.High;
                    common.DisplayError(ClassName, "Unrecognized High/Low String", "Unclassified Error");
                    break;
            }
            return returnValue;
        }
        public static UpperLower CStr2UpperLower(string Input)
        {
            UpperLower returnVal;
            switch (Input.ToUpper().Trim())
            {
                case "UPPER":
                case "1":
                    returnVal = UpperLower.Upper;
                    break;
                case "LOWER":
                case "0":
                    returnVal = UpperLower.Lower;
                    break;
                default:
                    returnVal = UpperLower.Lower;
                    common.DisplayError(ClassName, "Unrecognized Upper/Lower String", "Unclassified Error");
                    break;
            }
            return returnVal;
        }
        public static string CLinearity_Type2Str(Linearity_Correction_Type Type)
        {
            string returnVal;
            switch (Type)
            {
                case Linearity_Correction_Type.A_Type:
                    returnVal = "ATYP";
                    break;
                case Linearity_Correction_Type.D_Type:
                    returnVal = "DTYP";
                    break;
                default:
                    returnVal = "DTYP";
                    common.DisplayError(ClassName, "Unrecognized Linearity Correction Type Enum", "Unclassified Error");
                    break;
            }
            return returnVal;
        }
        public static Linearity_Correction_Type CStr2Linearity_Type(string input)
        {
            Linearity_Correction_Type returnVal;
            switch (input.ToUpper().Trim())
            {
                case "ATYP":
                    returnVal = Linearity_Correction_Type.A_Type;
                    break;
                case "DTYP":
                    returnVal = Linearity_Correction_Type.D_Type;
                    break;
                default:
                    returnVal = Linearity_Correction_Type.D_Type;
                    common.DisplayError(ClassName, "Unrecognized Linearity Correction Type String", "Unclassified Error");
                    break;
            }
            return returnVal;
        }
        #endregion

        #region "Class Initialization"
        public cCommonFunction BasicCommand;  // Basic Command for General Equipment
        
        public cCalibration Calibration;
        public cMeasurement Measurement;
        public cConfiguration Configuration;
        public cCalculate Calculate;
        public cFormat Format;
        public cMemory Memory;
        public cSense Sense;
        public cStatus Status;

        void Init(FormattedIO488 IOInit)
        {
            BasicCommand = new cCommonFunction(IOInit);
            Calibration = new cCalibration(IOInit);
            Measurement = new cMeasurement(IOInit);
            Configuration = new cConfiguration(IOInit);
            Calculate = new cCalculate(IOInit);
            Format = new cFormat(IOInit);
            Memory = new cMemory(IOInit);
            Sense = new cSense(IOInit);
            Status = new cStatus(IOInit);

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
                return ioPM;
            }
            set
            {
                ioPM = parseIO;
            }
        }
        public void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    ioPM = new FormattedIO488();
                    ioPM.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioPM.IO = null;
                    return;
                }
                Init(ioPM);
            }
        }
        public void CloseIO()
        {
            ioPM.IO.Close();
        }
        
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  18/1/2011        KKL             VISA Driver for Power Meter E4416A/E4417A

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return ("Power Meter Class Version = v" + VersionStr);
        }

        #region "Class Functional Codes"
        public class cCalibration : cCommonFunction
        {

            public cCalibration(FormattedIO488 parse) : base(parse) { }

            #region Calibration
            public void Zeroing()
            {
                SendCommand("CAL:ZERO:AUTO ONCE");
            }
            public void Zeroing(int ChannelNumber)
            {
                SendCommand("CAL" + ChannelNumber.ToString() + ":ZERO:AUTO ONCE");
            }
            public void CalAllSet()
            {
                SendCommand("CAL:ALL");
            }
            public void CalAll(int ChannelNumber)
            {
                SendCommand("CAL" + ChannelNumber.ToString() + ":ALL");
            }
            public bool CalAllStatus()
            {
                return Convert.ToBoolean(ReadCommand("CAL:ALL?"));
            }
            public bool CalAllStatus(int ChannelNumber)
            {
                return Convert.ToBoolean(ReadCommand("CAL" + ChannelNumber.ToString() + ":ALL?"));
            }
            public void CalAuto(bool State)
            {
                if (State == true)
                {
                    SendCommand("CAL:AUTO ONCE");
                }
                else
                {
                    SendCommand("CAL:AUTO OFF");
                }
            }
            public void CalAuto(int ChannelNumber, bool State)
            {
                if (State == true)
                {
                    SendCommand("CAL" + ChannelNumber.ToString() + ":AUTO ONCE");
                }
                else
                {
                    SendCommand("CAL" + ChannelNumber.ToString() + ":AUTO OFF");
                }
            }
            public bool CalAutoState()
            {
                return Convert.ToBoolean(ReadCommand("CAL:AUTO?"));
            }
            public bool CalAutoState(int ChannelNumber)
            {
                return Convert.ToBoolean(ReadCommand("CAL" + ChannelNumber + ":AUTO?"));
            }
            public void CalTTLInput(bool state)
            {
                SendCommand("CAL:ECON:STAT " + state.GetHashCode().ToString());
            }
            public void CalTTLInput(int ChannelNumber, bool state)
            {
                SendCommand("CAL" + ChannelNumber.ToString().Trim() + ":ECON:STAT " + state.GetHashCode().ToString());
            }
            public bool CalTTLInputStatus()
            {
                bool returnValue;
                returnValue = common.CInt2Bool(Convert.ToInt16(ReadCommand("CAL:ECON:STAT?")));
                return returnValue;
            }
            public bool CalTTLInputStatus(int ChannelNumber)
            {
                bool returnValue;
                returnValue = common.CInt2Bool(Convert.ToInt16(ReadCommand("CAL" + ChannelNumber.ToString().Trim() + ":ECON:STAT?")));
                return returnValue;
            }
            public void CalLockOut(bool State)
            {
                SendCommand("CAL:RCAL " + common.CBool2Int(State));
            }
            public void CalLockOut(int ChannelNumber, bool State)
            {
                SendCommand("CAL" + ChannelNumber.ToString().Trim() + ":RCAL " + common.CBool2Int(State));
            }
            public bool CalLockOutStatus()
            {
                bool returnValue;
                returnValue = common.CInt2Bool(Convert.ToInt16(ReadCommand("CAL:RCAL?")));
                return returnValue;
            }
            public bool CalLockOutStatus(short ChannelNumber)
            {
                bool returnValue;
                returnValue = common.CInt2Bool(Convert.ToInt32(ReadCommand("CAL" + ChannelNumber.ToString().Trim() + ":RCAL?")));
                return returnValue;
            }
            public void CalFactor(double CalFac)
            {
                SendCommand("CAL:RCF " + CalFac.ToString().Trim());
            }
            public void CalFactor(int ChannelNumber, double CalFac)
            {
                SendCommand("CAL" + ChannelNumber + ":RCF " + CalFac.ToString().Trim());
            }
            public void CalFactor(string CalFac)
            {
                SendCommand("CAL:RCF " + CalFac.Trim());
            }
            public void CalFactor(int ChannelNumber, string CalFac)
            {
                SendCommand("CAL" + ChannelNumber + ":RCF " + CalFac.Trim());
            }
            public double CalFactorStatus()
            {
                double returnValue;
                returnValue = Convert.ToDouble(ReadCommand("CAL:RCF?"));
                return returnValue;
            }
            public double CalFactorStatus(int ChannelNumber)
            {
                double returnValue;
                returnValue = Convert.ToDouble(ReadCommand("CAL" + ChannelNumber + ":RCF?"));
                return returnValue;
            }

            #endregion
        }
        public class cDisplay : cCommonFunction
        {
            public cContrast Contrast;
            public cEnable Enable;
            public cScreen Screen;
            public cAnalog Analog;
            public cFormat Format;
            public cMeter Meter;
            public cResolution Resolution;
            public cSelect Select;
            public cTrace Trace;

            public cDisplay(FormattedIO488 parse)
                : base(parse)
            {
                Contrast = new cContrast(parse);
                Enable = new cEnable(parse);
                Screen = new cScreen(parse);
                Analog = new cAnalog(parse);
                Format = new cFormat(parse);
                Meter = new cMeter(parse);
                Resolution = new cResolution(parse);
                Select = new cSelect(parse);
                Trace = new cTrace(parse);

            }

            #region "Display Command"
            public class cContrast : cCommonFunction
            {
                public cContrast(FormattedIO488 parse) : base(parse) { }
                public void SetContrast(double Value)
                {
                    SendCommand("DISP:CONT " + Value.ToString());
                }
                public void SetContrast(string Value)
                {
                    SendCommand("DISP:CONT " + Value.Trim());
                }
                public double GetContrastStatus()
                {
                    return (Convert.ToDouble(ReadCommand("DISP:CONT?")));
                }
                public double GetContrastStatus(string Contrast)
                {
                    return (Convert.ToDouble(ReadCommand("DISP:CONT? " + Contrast.Trim())));
                }
            }
            public class cEnable : cCommonFunction
            {
                public cEnable(FormattedIO488 parse) : base(parse) { }
                public void SetEnable(bool State)
                {
                    SendCommand("DISP:ENAB " + common.CBool2Int(State));
                }
                public bool GetEnableStatus()
                {
                    return (common.CInt2Bool(Convert.ToInt16(ReadCommand("DISP:ENAB?"))));
                }
            }
            public class cScreen : cCommonFunction
            {
                public cScreen(FormattedIO488 parse) : base(parse) { }
                public void SetScreenFormat(ScreenFormat ScrFormat)
                {
                    SendCommand("DISP:SCReen:FORM " + CScreenFormat2Str(ScrFormat));
                }
                public void SetScreenFormat(string ScrFormat)
                {
                    SendCommand("DISP:SCReen:FORM " + ScrFormat.ToUpper().Trim());
                }
                public ScreenFormat GetScreenFormat()
                {
                    ScreenFormat RtnValue;
                    RtnValue = CStr2ScreenFormat(ReadCommand("DISP:SCR:FORM?"));
                    return (RtnValue);
                }
                //public string GetScreenFormat()
                //{
                //    string RtnValue;
                //    RtnValue = ReadCommand("DISP:SCR:FORM?");
                //    return (RtnValue);
                //}
            }
            public class cAnalog : cCommonFunction
            {
                public cAnalog(FormattedIO488 parse) : base(parse) { }
                public void SetAnalog(int UP_Low_Scale, double Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND:ANAL:UPP " + Scale.ToString());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND:ANAL:LOW " + Scale.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Analog Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetAnalog(int UP_Low_Scale, string Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND:ANAL:UPP " + Scale.Trim());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND:ANAL:LOW " + Scale.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Analog Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetAnalog(int Window, int UP_Low_Scale, double Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND" + Window + ":ANAL:UPP " + Scale.ToString());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND" + Window + ":ANAL:LOW " + Scale.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Analog Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetAnalog(int Window, int UP_Low_Scale, string Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND" + Window + ":ANAL:UPP " + Scale.Trim());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND" + Window + ":ANAL:LOW " + Scale.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Analog Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public double GetAnalogStatus(int UP_Low_Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:ANAL:UPP?"));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:ANAL:LOW?"));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Analog Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetAnalogStatus(int UP_Low_Scale, string Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:ANAL:UPP? " + Scale.ToUpper().Trim()));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:ANAL:LOW? " + Scale.ToUpper().Trim()));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Analog Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetAnalogStatus(int Window, int UP_Low_Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":ANAL:UPP?"));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":ANAL:LOW?"));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Analog Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetAnalogStatus(int Window, int UP_Low_Scale, string Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":ANAL:UPP? " + Scale.ToUpper().Trim()));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":ANAL:LOW? " + Scale.ToUpper().Trim()));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Analog Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
            }
            public class cFormat : cCommonFunction
            {
                public cFormat(FormattedIO488 parse) : base(parse) { }
                public void SetWindowFormat(WindowFormat WinFormat)
                {
                    SendCommand("DISP:WIND:FORM " + CWindowFormat2Str(WinFormat));
                }
                public void SetWindowFormat(int Window, WindowFormat WinFormat)
                {
                    SendCommand("DISP:WIND" + Window + ":FORM " + CWindowFormat2Str(WinFormat));
                }
                public void SetWindowFormat(string WinFormat)
                {
                    SendCommand("DISP:WIND:FORM " + WinFormat.ToUpper().Trim());
                }
                public void SetWindowFormat(int Window, string WinFormat)
                {
                    SendCommand("DISP:WIND" + Window + ":FORM " + WinFormat.ToUpper().Trim());
                }
                public WindowFormat GetWindowFormatStatus()
                {
                    return (CStr2WindowFormat(ReadCommand("DISP:WIND:FORM?")));
                }
                public WindowFormat GetWindowFormatStatus(int Window)
                {
                    return (CStr2WindowFormat(ReadCommand("DISP:WIND" + Window + ":FORM?")));
                }
                //public string GetWindowFormatStatus()
                //{
                //    return (ReadCommand("DISP:WIND:FORM?"));
                //}
                //public string GetWindowFormatStatus(int Window)
                //{
                //    return (ReadCommand("DISP:WIND" + Window + ":FORM?"));
                //}
            }
            public class cMeter : cCommonFunction
            {
                public cMeter(FormattedIO488 parse) : base(parse) { }
                public void SetMeter(int UP_Low_Scale, double Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND:MET:UPP " + Scale.ToString());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND:MET:LOW " + Scale.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Meter Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetMeter(int UP_Low_Scale, string Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND:MET:UPP " + Scale.Trim());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND:MET:LOW " + Scale.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Meter Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetMeter(int Window, int UP_Low_Scale, double Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND" + Window + ":MET:UPP " + Scale.ToString());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND" + Window + ":MET:LOW " + Scale.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Meter Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetMeter(int Window, int UP_Low_Scale, string Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND" + Window + ":MET:UPP " + Scale.Trim());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND" + Window + ":MET:LOW " + Scale.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Meter Screen", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public double GetMeterStatus(int UP_Low_Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:MET:UPP?"));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:MET:LOW?"));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Meter Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetMeterStatus(int UP_Low_Scale, string Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:MET:UPP? " + Scale.ToUpper().Trim()));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:MET:LOW? " + Scale.ToUpper().Trim()));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Meter Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetMeterStatus(int Window, int UP_Low_Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":MET:UPP?"));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":MET:LOW?"));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Meter Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetMeterStatus(int Window, int UP_Low_Scale, string Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":MET:UPP? " + Scale.ToUpper().Trim()));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":MET:LOW? " + Scale.ToUpper().Trim()));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Meter Screen", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
            }
            public class cResolution : cCommonFunction
            {
                public cResolution(FormattedIO488 parse) : base(parse) { }
                public void SetResolution(int Resolution)
                {
                    SendCommand("DISP:WIND:RES " + Resolution.ToString());
                }
                public void SetResolution(string Resolution)
                {
                    SendCommand("DISP:WIND:RES " + Resolution.Trim());
                }
                public void SetResolution(int Window, int Resolution)
                {
                    SendCommand("DISP:WIND" + Window + ":RES " + Resolution.ToString());
                }
                public void SetResolution(int Window, string Resolution)
                {
                    SendCommand("DISP:WIND" + Window + ":RES " + Resolution.Trim());
                }
                public void SetResolution(int Window, int Resolution, int Numeric)
                {
                    if (Numeric == 1)
                    {
                        SendCommand("DISP:WIND" + Window + ":RES " + Resolution.ToString());
                    }
                    else if (Numeric == 2)
                    {
                        SendCommand("DISP:WIND" + Window + ":NUM2:RES " + Resolution.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Resolution Display", "Unrecognized Numeric Value. \n\n Please Select 1 or 2 only");
                    }
                }
                public void SetResolution(int Window, string Resolution, int Numeric)
                {
                    if (Numeric == 1)
                    {
                        SendCommand("DISP:WIND" + Window + ":RES " + Resolution.Trim());
                    }
                    else if (Numeric == 2)
                    {
                        SendCommand("DISP:WIND" + Window + ":NUM2:RES " + Resolution.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Resolution Display", "Unrecognized Numeric Value. \n\n Please Select 1 or 2 only");
                    }
                }
                public int GetResolution()
                {
                    return (Convert.ToInt16(ReadCommand("DISP:WIND:RES?")));
                }
                public int GetResolution(int Window)
                {
                    return (Convert.ToInt16(ReadCommand("DISP:WIND" + Window + ":RES?")));
                }
                public int GetResolution(int Window, int Numeric)
                {
                    return (Convert.ToInt16(ReadCommand("DISP:WIND" + Window + ":NUM" + Numeric.ToString() + ":RES?")));
                }
            }
            public class cSelect : cCommonFunction
            {
                public cSelect(FormattedIO488 parse) : base(parse) { }
                public void SelectWindow(int Selection)
                {
                    SendCommand("DISP:WIND:SEL" + Selection.ToString());
                }
                public void SelectWindow(int Window, int Selection)
                {
                    SendCommand("DISP:WIND" + Window.ToString() + ":SEL" + Selection.ToString());
                }
                public bool GetSelectWindowStatus(int Selection)
                {
                    return (common.CInt2Bool(Convert.ToInt16(ReadCommand("DISP:WIND:SEL" + Selection.ToString() + "?"))));
                }
                public bool GetSelectWindowStatus(int Window, int Selection)
                {
                    return (common.CInt2Bool(Convert.ToInt16(ReadCommand("DISP:WIND" + Window.ToString() + ":SEL" + Selection.ToString() + "?"))));
                }
            }
            public class cState : cCommonFunction
            {
                public cState(FormattedIO488 parse) : base(parse) { }
                public void SetWindowState(bool State)
                {
                    SendCommand("DISP:WIND:STAT " + common.CBool2Int(State));
                }
                public void SetWindowState(int Window, bool State)
                {
                    SendCommand("DISP:WIND" + Window + ":STAT " + common.CBool2Int(State));
                }
                public bool GetWindowState()
                {
                    return (common.CInt2Bool(Convert.ToInt16(ReadCommand("DISP:WIND:STAT?"))));
                }
                public bool GetWindowState(int Window)
                {
                    return (common.CInt2Bool(Convert.ToInt16(ReadCommand("DISP:WIND" + Window + ":STAT?"))));
                }
            }
            public class cTrace : cCommonFunction
            {
                public cTrace(FormattedIO488 parse) : base(parse) { }
                public void SetFeed(int Channel)
                {
                    SendCommand("DISP:WIND:TRAC:FEED SENS" + Channel.ToString());
                }
                public void SetFeed(int Window, int Channel)
                {
                    SendCommand("DISP:WIND" + Window.ToString() + ":TRAC:FEED SENS" + Channel.ToString());
                }
                public int GetFeed()
                {
                    return (Convert.ToInt16(ReadCommand("DISP:WIND:TRAC:FEED?")));
                }
                public int GetFeed(int Window)
                {
                    return (Convert.ToInt16(ReadCommand("DISP:WIND" + Window.ToString() + ":TRAC:FEED?")));
                }
                public void SetTrace(int UP_Low_Scale, double Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND:TRAC:UPP " + Scale.ToString());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND:TRAC:LOW " + Scale.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Trace Display", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetTrace(int UP_Low_Scale, string Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND:TRAC:UPP " + Scale.Trim());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND:TRAC:LOW " + Scale.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Trace Display", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetTrace(int Window, int UP_Low_Scale, double Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND" + Window.ToString() + ":TRAC:UPP " + Scale.ToString());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND" + Window.ToString() + ":TRAC:LOW " + Scale.ToString());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Trace Display", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public void SetTrace(int Window, int UP_Low_Scale, string Scale)
                {
                    if (UP_Low_Scale == 1)
                    {
                        SendCommand("DISP:WIND" + Window.ToString() + ":TRAC:UPP " + Scale.Trim());
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        SendCommand("DISP:WIND" + Window.ToString() + ":TRAC:LOW " + Scale.Trim());
                    }
                    else
                    {
                        common.DisplayError(ClassName, "Trace Display", "Error setting the Upper/Lower Scale of the Screen");
                    }
                }
                public double GetTraceStatus(int UP_Low_Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:TRAC:UPP?"));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:TRAC:LOW?"));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Trace Display", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }

                public double GetTraceStatus(int UP_Low_Scale, string Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:TRAC:UPP? " + Scale.Trim()));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:TRAC:LOW? " + Scale.Trim()));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Trace Display", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
                public double GetTraceStatus(int Window, int UP_Low_Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":TRAC:UPP?"));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND" + Window + ":TRAC:LOW?"));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Trace Display", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }

                public double GetTraceStatus(int Window, int UP_Low_Scale, string Scale)
                {
                    double RtnValue;
                    if (UP_Low_Scale == 1)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:TRAC:UPP? " + Scale.Trim()));
                    }
                    else if (UP_Low_Scale == 0)
                    {
                        RtnValue = Convert.ToDouble(ReadCommand("DISP:WIND:TRAC:LOW? " + Scale.Trim()));
                    }
                    else
                    {
                        RtnValue = 0;
                        common.DisplayError(ClassName, "Trace Display", "Error Getting the Upper/Lower Scale of the Screen");
                    }
                    return (RtnValue);
                }
            }
            #endregion
        }
        public class cMeasurement : cCommonFunction
        {
            public cMeasuremet_Fetch Fetch;
            public cMeasuremet_Read Read;
            public cMeasuremet_Measure Measure;

            public cMeasurement(FormattedIO488 parse)
                : base(parse)
            {
                Fetch = new cMeasuremet_Fetch(parse);
                Read = new cMeasuremet_Read(parse);
                Measure = new cMeasuremet_Measure(parse);
            }

            #region "Measurement"
            //Fetch Command measure the data when valids
            public class cMeasuremet_Fetch : cCommonFunction
            {
                public cMeasuremet_Fetch(FormattedIO488 parse) : base(parse) { }
                #region "Power"
                public double Power()
                {
                    return (Convert.ToDouble(ReadCommand("FETC:POW:AC?")));
                }
                public double Power(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":POW:AC?")));
                }
                public double Power(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("FETC:POW:AC? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double Power(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":POW:AC? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Relative Power"
                public double RelativePower()
                {
                    return (Convert.ToDouble(ReadCommand("FETC:REL?")));
                }
                public double RelativePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":REL?")));
                }
                public double RelativePower(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("FETC:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double RelativePower(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Difference Power"
                public double DifferencePower()
                {
                    return (Convert.ToDouble(ReadCommand("FETC:DIFF?")));
                }
                public double DifferencePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":DIFF?")));
                }
                public double DifferencePower(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("FETC:DIFF? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double DifferencePower(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":DIFF? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Relative Difference Power"
                public double RelativeDifferencePower()
                {
                    return (Convert.ToDouble(ReadCommand("FETC:DIFF:REL?")));
                }
                public double RelativeDifferencePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":DIFF:REL?")));
                }
                public double RelativeDifferencePower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("FETC:DIFF:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RelativeDifferencePower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":DIFF:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
                #region "Ratio Power"
                public double RatioPower()
                {
                    return (Convert.ToDouble(ReadCommand("FETC:RAT?")));
                }
                public double RatioPower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":RAT?")));
                }
                public double RatioPower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("FETC:RAT? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RatioPower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":RAT? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
                #region "Relative Ratio Power"
                public double RelativeRatioPower()
                {
                    return (Convert.ToDouble(ReadCommand("FETC:RAT:REL?")));
                }
                public double RelativeRatioPower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":RAT:REL?")));
                }
                public double RelativeRatioPower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("FETC:RAT:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RelativeRatioPower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("FETC" + Window.ToString() + ":RAT:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
            }

            //Read Command Aborts previous command, initiate a trigger and fetch the data
            public class cMeasuremet_Read : cCommonFunction
            {
                public cMeasuremet_Read(FormattedIO488 parse) : base(parse) { }
                #region "Power"
                public double Power()
                {
                    return (Convert.ToDouble(ReadCommand("READ:POW:AC?")));
                }
                public double Power(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":POW:AC?")));
                }
                public double Power(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("READ:POW:AC? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double Power(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":POW:AC? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Relative Power"
                public double RelativePower()
                {
                    return (Convert.ToDouble(ReadCommand("READ:REL?")));
                }
                public double RelativePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":REL?")));
                }
                public double RelativePower(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("READ:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double RelativePower(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Difference Power"
                public double DifferencePower()
                {
                    return (Convert.ToDouble(ReadCommand("READ:DIFF?")));
                }
                public double DifferencePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":DIFF?")));
                }
                public double DifferencePower(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("READ:DIFF? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double DifferencePower(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":DIFF? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Relative Difference Power"
                public double RelativeDifferencePower()
                {
                    return (Convert.ToDouble(ReadCommand("READ:DIFF:REL?")));
                }
                public double RelativeDifferencePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":DIFF:REL?")));
                }
                public double RelativeDifferencePower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("READ:DIFF:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RelativeDifferencePower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":DIFF:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
                #region "Ratio Power"
                public double RatioPower()
                {
                    return (Convert.ToDouble(ReadCommand("READ:RAT?")));
                }
                public double RatioPower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":RAT?")));
                }
                public double RatioPower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("READ:RAT? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RatioPower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":RAT? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
                #region "Relative Ratio Power"
                public double RelativeRatioPower()
                {
                    return (Convert.ToDouble(ReadCommand("READ:RAT:REL?")));
                }
                public double RelativeRatioPower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":RAT:REL?")));
                }
                public double RelativeRatioPower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("READ:RAT:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RelativeRatioPower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("READ" + Window.ToString() + ":RAT:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
            }

            //Measure Command Aborts previous command, configure the equipment and initiate a trigger and fetch the data
            public class cMeasuremet_Measure : cCommonFunction
            {
                public cMeasuremet_Measure(FormattedIO488 parse) : base(parse) { }
                #region "Power"
                public double Power()
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:POW:AC?")));
                }
                public double Power(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":POW:AC?")));
                }
                public double Power(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:POW:AC? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double Power(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":POW:AC? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Relative Power"
                public double RelativePower()
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:REL?")));
                }
                public double RelativePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":REL?")));
                }
                public double RelativePower(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double RelativePower(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Difference Power"
                public double DifferencePower()
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:DIFF?")));
                }
                public double DifferencePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":DIFF?")));
                }
                public double DifferencePower(int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:DIFF? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                public double DifferencePower(int Window, int Resolution, int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":DIFF? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString())));
                }
                #endregion
                #region "Relative Difference Power"
                public double RelativeDifferencePower()
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:DIFF:REL?")));
                }
                public double RelativeDifferencePower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":DIFF:REL?")));
                }
                public double RelativeDifferencePower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:DIFF:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RelativeDifferencePower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":DIFF:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
                #region "Ratio Power"
                public double RatioPower()
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:RAT?")));
                }
                public double RatioPower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":RAT?")));
                }
                public double RatioPower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:RAT? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RatioPower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":RAT? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
                #region "Relative Ratio Power"
                public double RelativeRatioPower()
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:RAT:REL?")));
                }
                public double RelativeRatioPower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":RAT:REL?")));
                }
                public double RelativeRatioPower(int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS:RAT:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                public double RelativeRatioPower(int Window, int Resolution, int ChannelNumber, int ChannelNumber2)
                {
                    return (Convert.ToDouble(ReadCommand("MEAS" + Window.ToString() + ":RAT:REL? DEF," + Resolution.ToString() + ",@" + ChannelNumber.ToString() + ",@" + ChannelNumber2.ToString())));
                }
                #endregion
            }

            #endregion
        }
        public class cConfiguration : cCommonFunction
        {
            public cPower Power;
            public cRelativePower RelativePower;
            public cDifferentPower DifferentPower;
            public cRelativeDifferentPower RelativeDifferentPower;
            public cRatioPower RatioPower;
            public cRelativeRatioPwer RelativeRatioPower;

            public cConfiguration(FormattedIO488 parse)
                : base(parse)
            {
                Power = new cPower(parse);
                RelativePower = new cRelativePower(parse);
                DifferentPower = new cDifferentPower(parse);
                RelativeDifferentPower = new cRelativeDifferentPower(parse);
                RatioPower = new cRatioPower(parse);
                RelativeRatioPower = new cRelativeRatioPwer(parse);
            }

            public class cPower : cCommonFunction
            {
                public cPower(FormattedIO488 parse) : base(parse) { }
                //public void PowerResolution(int Resolution)
                //{
                //    SendCommand("CONF:POW:AC DEF," + Resolution.ToString());
                //}
                public void PowerResolution(int Window, int Resolution)
                {
                    SendCommand("CONF" + Window.ToString() + ":POW:AC DEF," + Resolution.ToString());
                }
                //public void PowerResolution(int Resolution, int ChannelSource)
                //{
                //    SendCommand("CONF:POW:AC DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                //}
                public void PowerResolution(int Window, int Resolution, int ChannelSource)
                {
                    SendCommand("CONF" + Window.ToString() + ":POW:AC DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                }
            }
            public class cRelativePower : cCommonFunction
            {
                public cRelativePower(FormattedIO488 parse) : base(parse) { }
                //public void PowerResolution(int Resolution)
                //{
                //    SendCommand("CONF:POW:AC:REL DEF," + Resolution.ToString());
                //}
                public void PowerResolution(int Window, int Resolution)
                {
                    SendCommand("CONF" + Window.ToString() + ":POW:AC:REL DEF," + Resolution.ToString());
                }
                //public void PowerResolution(int Resolution, int ChannelSource)
                //{
                //    SendCommand("CONF:POW:AC:REL DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                //}
                public void PowerResolution(int Window, int Resolution, int ChannelSource)
                {
                    SendCommand("CONF" + Window.ToString() + ":POW:AC:REL DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                }
            }
            public class cDifferentPower : cCommonFunction
            {
                public cDifferentPower(FormattedIO488 parse) : base(parse) { }
                //public void PowerResolution(int Resolution)
                //{
                //    SendCommand("CONF:DIFF DEF," + Resolution.ToString());
                //}
                public void PowerResolution(int Window, int Resolution)
                {
                    SendCommand("CONF" + Window.ToString() + ":DIFF DEF," + Resolution.ToString());
                }
                //public void PowerResolution(int Resolution, int ChannelSource)
                //{
                //    SendCommand("CONF:DIFF DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                //}
                public void PowerResolution(int Window, int Resolution, int ChannelSource)
                {
                    SendCommand("CONF" + Window.ToString() + ":DIFF DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                }

            }
            public class cRelativeDifferentPower : cCommonFunction
            {
                public cRelativeDifferentPower(FormattedIO488 parse) : base(parse) { }
                //public void PowerResolution(int Resolution)
                //{
                //    SendCommand("CONF:DIFF:REL DEF," + Resolution.ToString());
                //}
                public void PowerResolution(int Window, int Resolution)
                {
                    SendCommand("CONF" + Window.ToString() + ":DIFF:REL DEF," + Resolution.ToString());
                }
                //public void PowerResolution(int Resolution, int ChannelSource)
                //{
                //    SendCommand("CONF:DIFF:REL DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                //}
                public void PowerResolution(int Window, int Resolution, int ChannelSource)
                {
                    SendCommand("CONF" + Window.ToString() + ":DIFF:REL DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                }
            }
            public class cRatioPower : cCommonFunction
            {
                public cRatioPower(FormattedIO488 parse) : base(parse) { }
                //public void PowerResolution(int Resolution)
                //{
                //    SendCommand("CONF:RAT DEF," + Resolution.ToString());
                //}
                public void PowerResolution(int Window, int Resolution)
                {
                    SendCommand("CONF" + Window.ToString() + ":RAT DEF," + Resolution.ToString());
                }
                //public void PowerResolution(int Resolution, int ChannelSource)
                //{
                //    SendCommand("CONF:RAT DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                //}
                public void PowerResolution(int Window, int Resolution, int ChannelSource)
                {
                    SendCommand("CONF" + Window.ToString() + ":RAT DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                }
            }
            public class cRelativeRatioPwer : cCommonFunction
            {
                public cRelativeRatioPwer(FormattedIO488 parse) : base(parse) { }
                //public void PowerResolution(int Resolution)
                //{
                //    SendCommand("CONF:RAT:REL DEF," + Resolution.ToString());
                //}
                public void PowerResolution(int Window, int Resolution)
                {
                    SendCommand("CONF" + Window.ToString() + ":RAT:REL DEF," + Resolution.ToString());
                }
                //public void PowerResolution(int Resolution, int ChannelSource)
                //{
                //    SendCommand("CONF:RAT:REL DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                //}
                public void PowerResolution(int Window, int Resolution, int ChannelSource)
                {
                    SendCommand("CONF" + Window.ToString() + ":RAT:REL DEF," + Resolution.ToString() + ",@" + ChannelSource.ToString());
                }
            }
        }
        public class cCalculate : cCommonFunction
        {
            public cFeed Feed;
            public cGain Gain;
            public cLimit Limit;
            public cMath Math;
            public cPeakHold PeakHold;
            public cRelative Relative;

            public cCalculate(FormattedIO488 parse)
                : base(parse)
            {
                Feed = new cFeed(parse);
                Gain = new cGain(parse);
                Limit = new cLimit(parse);
                Math = new cMath(parse);
                PeakHold = new cPeakHold(parse);
                Relative = new cRelative(parse);
            }

            public class cFeed : cCommonFunction
            {
                public cFeed(FormattedIO488 parse) : base(parse) { }
                public void SetPeak()
                {
                    SendCommand("CALC:FEED \"POW:PEAK\"");
                }
                public void SetPeak(int FeedNumber)
                {
                    SendCommand("CALC:FEED" + FeedNumber + " \"POW:PEAK\"");
                }
                //public void SetPeak(int Window)
                //{
                //    SendCommand("CALC" + Window.ToString() + ":FEED \"POW:PEAK\"");
                //}
                public void SetPeak(int Window, int FeedNumber)
                {
                    SendCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber + " \"POW:PEAK\"");
                }
                //public void SetPeak(int Gate)
                //{
                //    SendCommand("CALC:FEED \"POW:PEAK ON SWEEP" + Gate.ToString() + "\"");
                //}
                //public void SetPeak(int FeedNumber, int Gate)
                //{
                //    SendCommand("CALC:FEED" + FeedNumber + " \"POW:PEAK ON SWEEP" + Gate.ToString() + "\"");
                //}
                //public void SetPeak(int Window, int Gate)
                //{
                //    SendCommand("CALC" + Window.ToString() + ":FEED \"POW:PEAK ON SWEEP" + Gate.ToString() + "\"");
                //}
                public void SetPeak(int Window, int FeedNumber, int Gate)
                {
                    SendCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber + " \"POW:PEAK ON SWEEP" + Gate.ToString() + "\"");
                }

                public void SetPeak2Avg()
                {
                    SendCommand("CALC:FEED \"POW:PTAV\"");
                }
                public void SetPeak2Avg(int FeedNumber)
                {
                    SendCommand("CALC:FEED" + FeedNumber.ToString() + " \"POW:PTAV\"");
                }
                //public void SetPeak2Avg(int FeedNumber, int Gate)
                //{
                //    SendCommand("CALC:FEED" + FeedNumber.ToString() + " \"POW:PTAV ON SWEEP" + Gate.ToString() + "\"");
                //}
                //public void SetPeak2Avg(int Window)
                //{
                //    SendCommand("CALC" + Window.ToString() + ":FEED \"POW:PTAV\"");
                //}
                public void SetPeak2Avg(int Window, int FeedNumber)
                {
                    SendCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber.ToString() + " \"POW:PTAV\"");
                }
                public void SetPeak2Avg(int Window, int FeedNumber, int Gate)
                {
                    SendCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber.ToString() + " \"POW:PTAV ON SWEEP" + Gate.ToString() + "\"");
                }

                public void SetAvg()
                {
                    SendCommand("CALC:FEED \"POW:AVER\"");
                }
                public void SetAvg(int FeedNumber)
                {
                    SendCommand("CALC:FEED" + FeedNumber + " \"POW:AVER\"");
                }
                //public void SetAvg(int FeedNumber, int Gate)
                //{
                //    SendCommand("CALC:FEED" + FeedNumber + " \"POW:AVER ON SWEEP" + Gate.ToString() + "\"");
                //}
                //public void SetAvg(int Window)
                //{
                //    SendCommand("CALC" + Window.ToString() + ":FEED \"POW:AVER\"");
                //}
                public void SetAvg(int Window, int FeedNumber)
                {
                    SendCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber + " \"POW:AVER\"");
                }
                public void SetAvg(int Window, int FeedNumber, int Gate)
                {
                    SendCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber + " \"POW:AVER ON SWEEP" + Gate.ToString() + "\"");
                }

                public void SetOff()
                {
                    SendCommand("CALC:FEED");
                }
                public void SetOff(int FeedNumber)
                {
                    SendCommand("CALC:FEED" + FeedNumber.ToString());
                }
                //public void SetOff(int Window)
                //{
                //    SendCommand("CALC" + Window.ToString() + ":FEED");
                //}
                public void SetOff(int Window, int FeedNumber)
                {
                    SendCommand("CALC" + Window.ToString() + "FEED" + FeedNumber.ToString());
                }

                public string GetFeed()
                {
                    return (ReadCommand("CALC:FEED?"));
                }
                public string GetFeed(int FeedNumber)
                {
                    return (ReadCommand("CALC:FEED" + FeedNumber + "?"));
                }
                //public string GetFeed(int Window)
                //{
                //    return (ReadCommand("CALC" + Window.ToString() + ":FEED?"));
                //}
                public string GetFeed(int Window, int FeedNumber)
                {
                    return (ReadCommand("CALC" + Window.ToString() + ":FEED" + FeedNumber + "?"));
                }
            }
            public class cGain : cCommonFunction
            {
                public cGain(FormattedIO488 parse) : base(parse) { }
                public void State(bool State)
                {
                    if (State == true)
                    {
                        SendCommand("CALC:GAIN:STAT 1");
                    }
                    else
                    {
                        SendCommand("CALC:GAIN:STAT 0");
                    }
                }
                public void State(int Window, bool State)
                {
                    if (State == true)
                    {
                        SendCommand("CALC" + Window.ToString() + ":GAIN:STAT 1");
                    }
                    else
                    {
                        SendCommand("CALC" + Window.ToString() + ":GAIN:STAT 0");
                    }
                }
                public bool State()
                {
                    return (common.CStr2Bool(ReadCommand("CALC:GAIN:STAT?")));
                }
                public bool State(int Window)
                {
                    return (common.CStr2Bool(ReadCommand("CALC" + Window.ToString() + ":GAIN:STAT?")));
                }
                public void Gain(double Gain)
                {
                    SendCommand("CALC:GAIN " + Gain.ToString());
                }
                public void Gain(string Gain)
                {
                    SendCommand("CALC:GAIN " + Gain);
                }
                public void Gain(int Window, double Gain)
                {
                    SendCommand("CALC" + Window.ToString() + ":GAIN " + Gain.ToString());
                }
                public void Gain(int Window, string Gain)
                {
                    SendCommand("CALC" + Window.ToString() + ":GAIN " + Gain);
                }
                public double Gain()
                {
                    return (Convert.ToDouble(ReadCommand("CALC:GAIN?")));
                }
                public double Gain(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("CALC" + Window.ToString() + ":GAIN?")));
                }
            }
            public class cLimit : cCommonFunction
            {
                public cLimit(FormattedIO488 parse) : base(parse) { }
                public void Clear(bool State)
                {
                    if (State == true)
                    {
                        SendCommand("CALC:LIM:CLE:AUTO 1");
                    }
                    else
                    {
                        SendCommand("CALC:LIM:CLE:AUTO 0");
                    }
                }
                public void Clear(int Window, bool State)
                {
                    if (State == true)
                    {
                        SendCommand("CALC" + Window.ToString() + ":LIM:CLE:AUTO 1");
                    }
                    else
                    {
                        SendCommand("CALC" + Window.ToString() + ":LIM:CLE:AUTO 0");
                    }
                }
                public void ClearOnce()
                {
                    SendCommand("CALC:LIM:CLE:AUTO ONCE");
                }
                public void ClearOnce(int Window)
                {
                    SendCommand("CALC" + Window.ToString() + ":LIM:CLE:AUTO ONCE");
                }
                public bool Clear()
                {
                    return (common.CStr2Bool(ReadCommand("CALC:LIM:CLE:AUTO?")));
                }
                public bool Clear(int Window)
                {
                    return (common.CStr2Bool(ReadCommand("CALC" + Window.ToString() + ":LIM:CLE:AUTO?")));
                }
                public void ClearImmediate()
                {
                    SendCommand("CALC:LIM:CLE:IMM");
                }
                public void ClearImmediate(int Window)
                {
                    SendCommand("CALC" + Window.ToString() + ":LIM:CLE:IMM");
                }
                public bool Fail()
                {
                    return (common.CStr2Bool(ReadCommand("CALC:LIM:FAIL?")));
                }
                public bool Fail(int Window)
                {
                    return (common.CStr2Bool(ReadCommand("CALC" + Window.ToString() + ":LIM:FAIL?")));
                }
                public double FailCount()
                {
                    return (Convert.ToDouble(ReadCommand("CALC:LIM:FCO?")));
                }
                public double FailCount(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("CALC" + Window.ToString() + ":LIM:FCO?")));
                }
                public void SetLower(double Data)
                {
                    SendCommand("CALC:LIM:LOW:DATA " + Data.ToString());
                }
                public void SetLower(string Data)
                {
                    SendCommand("CALC:LIM:LOW:DATA " + Data);
                }
                public void SetLower(int Window, double Data)
                {
                    SendCommand("CALC" + Window.ToString() + ":LIM:LOW:DATA " + Data.ToString());
                }
                public void SetLower(int Window, string Data)
                {
                    SendCommand("CALC" + Window.ToString() + ":LIM:LOW:DATA " + Data);
                }
                public double GetLower()
                {
                    return (Convert.ToDouble(ReadCommand("CALC:LIM:LOW:DATA?")));
                }
                public double GetLower(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("CALC" + Window.ToString() + ":LIM:LOW:DATA?")));
                }
                public double GetLower(string Queries)
                {
                    return (Convert.ToDouble(ReadCommand("CALC:LIM:LOW:DATA? " + Queries.Trim())));
                }
                public double GetLower(int Window, string Queries)
                {
                    return (Convert.ToDouble(ReadCommand("CALC" + Window.ToString() + ":LIM:LOW:DATA? " + Queries.Trim())));
                }

                public void SetUpper(double Data)
                {
                    SendCommand("CALC:LIM:UPP:DATA " + Data.ToString());
                }
                public void SetUpper(string Data)
                {
                    SendCommand("CALC:LIM:UPP:DATA " + Data);
                }
                public void SetUpper(int Window, double Data)
                {
                    SendCommand("CALC" + Window.ToString() + ":LIM:UPP:DATA " + Data.ToString());
                }
                public void SetUpper(int Window, string Data)
                {
                    SendCommand("CALC" + Window.ToString() + ":LIM:UPP:DATA " + Data);
                }
                public double GetUpper()
                {
                    return (Convert.ToDouble(ReadCommand("CALC:LIM:UPP:DATA?")));
                }
                public double GetUpper(int Window)
                {
                    return (Convert.ToDouble(ReadCommand("CALC" + Window.ToString() + ":LIM:UPP:DATA?")));
                }
                public double GetUpper(string Queries)
                {
                    return (Convert.ToDouble(ReadCommand("CALC:LIM:UPP:DATA? " + Queries.Trim())));
                }
                public double GetUpper(int Window, string Queries)
                {
                    return (Convert.ToDouble(ReadCommand("CALC" + Window.ToString() + ":LIM:UPP:DATA? " + Queries.Trim())));
                }

                public void State(bool state)
                {
                    if (state == true)
                    {
                        SendCommand("CALC:LIM:STAT 1");
                    }
                    else
                    {
                        SendCommand("CALC:LIM:STAT 0");
                    }
                }
                public void State(int Window, bool state)
                {
                    if (state == true)
                    {
                        SendCommand("CALC" + Window.ToString() + ":LIM:STAT 1");
                    }
                    else
                    {
                        SendCommand("CALC" + Window.ToString() + ":LIM:STAT 0");
                    }
                }
                public bool State()
                {
                    return (common.CStr2Bool(ReadCommand("CALC:LIM:STAT?")));
                }
                public bool State(int Window)
                {
                    return (common.CStr2Bool(ReadCommand("CALC" + Window.ToString() + ":LIM:STAT?")));
                }


            }
            public class cMath : cCommonFunction
            {
                public cMath(FormattedIO488 parse) : base(parse) { }

                public void SetCalcMath(MathExpressionEnum Express)
                {
                    SendCommand("CALC:MATH " + CMathExpression2Str(Express));
                }
                public void SetCalcMath(string Express)
                {
                    SendCommand("CALC:MATH " + Express);
                }
                public void SetCalcMath(int Window, MathExpressionEnum Express)
                {
                    SendCommand("CALC" + Window.ToString() + ":MATH " + CMathExpression2Str(Express));
                }
                public void SetCalcMath(int Window, string Express)
                {
                    SendCommand("CALC" + Window.ToString() + ":MATH " + Express);
                }
                public MathExpressionEnum GetCalcMath()
                {
                    return (CStr2MathExpression(ReadCommand("CALC:MATH?")));
                }

                public MathExpressionEnum GetCalcMath(int Window)
                {
                    return (CStr2MathExpression(ReadCommand("CALC" + Window.ToString() + ":MATH?")));
                }
                public string GetCalcMathStr()
                {
                    return (ReadCommand("CALC:MATH?"));
                }
                public string GetCalcMathStr(int Window)
                {
                    return (ReadCommand("CALC" + Window.ToString() + ":MATH?"));
                }
            }
            public class cPeakHold : cCommonFunction
            {
                public cPeakHold(FormattedIO488 parse) : base(parse) { }
                public void Clear()
                {
                    SendCommand("CALC:PHOL:CLE");
                }
                public void Clear(int Window)
                {
                    SendCommand("CALC" + Window.ToString() + ":PHOL:CLE");
                }
            }
            public class cRelative : cCommonFunction
            {
                public cRelative(FormattedIO488 parse) : base(parse) { }
                public void SetRelative(bool state)
                {
                    if (state == true)
                    {
                        SendCommand("CALC:REL:AUTO 1");
                    }
                    else
                    {
                        SendCommand("CALC:REL:AUTO 0");
                    }
                }
                public void SetRelative(int Window, bool state)
                {
                    if (state == true)
                    {
                        SendCommand("CALC" + Window.ToString() + ":REL:AUTO 1");
                    }
                    else
                    {
                        SendCommand("CALC" + Window.ToString() + ":REL:AUTO 0");
                    }
                }
                public void RelativeOnce()
                {
                    SendCommand("CALC:REL:AUTO ONCE");
                }
                public void RelativeOnce(int Window)
                {
                    SendCommand("CALC" + Window.ToString() + ":REL:AUTO ONCE");
                }
                public bool GetRelative()
                {
                    return (common.CStr2Bool(ReadCommand("CALC:REL:AUTO?")));
                }
                public bool GetRelative(int Window)
                {
                    return (common.CStr2Bool(ReadCommand("CALC" + Window.ToString() + ":REL:AUTO?")));
                }
                public void SetState(bool state)
                {
                    if (state == true)
                    {
                        SendCommand("CALC:REL:STAT 1");
                    }
                    else
                    {
                        SendCommand("CALC:REL:STAT 0");
                    }
                }
                public void SetState(int Window, bool state)
                {
                    if (state == true)
                    {
                        SendCommand("CALC" + Window.ToString() + ":REL:STAT 1");
                    }
                    else
                    {
                        SendCommand("CALC" + Window.ToString() + ":REL:STAT 0");
                    }
                }
                public bool GetState()
                {
                    return (common.CStr2Bool(ReadCommand("CALC:REL:STAT?")));
                }
                public bool GetState(int Window)
                {
                    return (common.CStr2Bool(ReadCommand("CALC" + Window.ToString() + ":REL:STAT?")));
                }
            }

        }
        public class cFormat : cCommonFunction
        {
            public cFormat(FormattedIO488 parse)
                : base(parse)
            {
            }

            public void SetByteOrder(ByteOrder Order)
            {
                SendCommand("FORM:BORD " + CByteOrder2Str(Order));
            }
            public void SetByteOrder(string Order)
            {
                SendCommand("FORM:BORD " + Order);
            }
            public ByteOrder GetByteOrder()
            {
                return (CStr2ByteOrder(ReadCommand("FORM:BORD?")));
            }
            //public string GetByteOrder()
            //{
            //    return (ReadCommand("FORM:BORD?"));
            //}
            public void SetFormatData(Character_Data Data)
            {
                SendCommand("FORM " + CCharacterData2Str(Data));
            }
            public void SetFormatData(string Data)
            {
                SendCommand("FORM " + Data);
            }
            public Character_Data GetFormatData()
            {
                return (CStr2CharacterData(ReadCommand("FORM?")));
            }
            //public string GetFormatData()
            //{
            //    return (ReadCommand("FORM?"));
            //}

        }
        public class cMemory : cCommonFunction
        {
            public cCatalog Catalog;
            public cClear Clear;
            public cFree Free;
            public cRegister Register;
            public cState State;
            public cTable Table;
            public cOutput Output;

            public cMemory(FormattedIO488 parse)
                : base(parse)
            {
                Catalog = new cCatalog(parse);
                Clear = new cClear(parse);
                Free = new cFree(parse);
                Register = new cRegister(parse);
                State = new cState(parse);
                Table = new cTable(parse);
                Output = new cOutput(parse);
            }
            public class cCatalog : cCommonFunction
            {
                public cCatalog(FormattedIO488 parse) : base(parse) { }
                public void QueryCatalogAll(Memory_Catalog Catalog)
                {
                    char[] separator = new char[] { ',' };
                    string[] ReadStr;
                    ReadStr = ReadCommand("MEM:CAT?").Split(separator);
                    Catalog.MemoryUsed = Convert.ToDouble(ReadStr[0]);
                    Catalog.MemoryAvailable = Convert.ToDouble(ReadStr[1]);

                    int i = 2;
                    int Arr = 0;
                    Catalog.NameStr = new string[(ReadStr.Length - 2) / 3];
                    Catalog.Type = new string[(ReadStr.Length - 2) / 3];
                    Catalog.Size = new double[(ReadStr.Length - 2) / 3];
                    do
                    {
                        Catalog.NameStr[Arr] = ReadStr[i];
                        Catalog.Type[Arr] = ReadStr[i + 1];
                        Catalog.Size[Arr] = Convert.ToDouble(ReadStr[i + 2]);
                        i = i + 3;
                    } while (i < ReadStr.Length);

                }
                public void QueryCatalogState(Memory_Catalog Catalog)
                {
                    char[] separator = new char[] { ',' };
                    string[] ReadStr;
                    ReadStr = ReadCommand("MEM:CAT:STAT?").Split(separator);
                    Catalog.MemoryUsed = Convert.ToDouble(ReadStr[0]);
                    Catalog.MemoryAvailable = Convert.ToDouble(ReadStr[1]);

                    int i = 2;
                    int Arr = 0;
                    Catalog.NameStr = new string[(ReadStr.Length - 2) / 3];
                    Catalog.Type = new string[(ReadStr.Length - 2) / 3];
                    Catalog.Size = new double[(ReadStr.Length - 2) / 3];
                    do
                    {
                        Catalog.NameStr[Arr] = ReadStr[i];
                        Catalog.Type[Arr] = ReadStr[i + 1];
                        Catalog.Size[Arr] = Convert.ToDouble(ReadStr[i + 2]);
                        i = i + 3;
                    } while (i < ReadStr.Length);
                }
                public void QueryCatalogTable(Memory_Catalog Catalog)
                {
                    char[] separator = new char[] { ',' };
                    string[] ReadStr;
                    ReadStr = ReadCommand("MEM:CAT:TABL?").Split(separator);
                    Catalog.MemoryUsed = Convert.ToDouble(ReadStr[0]);
                    Catalog.MemoryAvailable = Convert.ToDouble(ReadStr[1]);

                    int i = 2;
                    int Arr = 0;
                    Catalog.NameStr = new string[(ReadStr.Length - 2) / 3];
                    Catalog.Type = new string[(ReadStr.Length - 2) / 3];
                    Catalog.Size = new double[(ReadStr.Length - 2) / 3];
                    do
                    {
                        Catalog.NameStr[Arr] = ReadStr[i];
                        Catalog.Type[Arr] = ReadStr[i + 1];
                        Catalog.Size[Arr] = Convert.ToDouble(ReadStr[i + 2]);
                        i = i + 3;
                    } while (i < ReadStr.Length);
                }
            }
            public class cClear : cCommonFunction
            {
                public cClear(FormattedIO488 parse) : base(parse) { }
                public void Name(string Name)
                {
                    SendCommand("MEM:CLE " + Name.Trim());
                }
                public void Table()
                {
                    SendCommand("MEM:CLE:TABL");
                }
            }
            public class cFree : cCommonFunction
            {
                public cFree(FormattedIO488 parse) : base(parse) { }
                public double GetAll()
                {
                    return (Convert.ToDouble(ReadCommand("MEM:FREE?")));
                }
                public double GetState()
                {
                    return (Convert.ToDouble(ReadCommand("MEM:FREE:STAT?")));
                }
                public double GetTable()
                {
                    return (Convert.ToDouble(ReadCommand("MEM:FREE:TABL?")));
                }
            }
            public class cRegister : cCommonFunction
            {
                public cRegister(FormattedIO488 parse) : base(parse) { }
                public int GetFreeRegister()
                {
                    return (Convert.ToInt32(ReadCommand("MEM:NST?")));
                }
            }
            public class cState : cCommonFunction
            {
                public cState(FormattedIO488 parse) : base(parse) { }
                public string GetCatalog()
                {
                    return (ReadCommand("MEM:STAT:CAT?"));
                }
                public void DefineState(string Name, int Register)
                {
                    SendCommand("MEM:STAT:DEF \"" + Name.Trim() + "\"," + Register.ToString());
                }
                public int GetDefinateState(string Name)
                {
                    return (Convert.ToInt32(ReadCommand("MEM:STAT:DEF? \"" + Name.Trim() + "\"")));
                }
            }
            public class cTable : cCommonFunction
            {
                public cTable(FormattedIO488 parse) : base(parse) { }
                public void SetFrequencyList(double[] List)
                {
                    int FreqLength;
                    string FreqList;

                    FreqLength = List.Length;
                    FreqList = "";
                    for (int i = 0; i <= FreqLength; i++)
                    {
                        if (i == 0)
                        {
                            FreqList = List[0].ToString();
                        }
                        else
                        {
                            FreqList = FreqList + "," + List[i];
                        }
                    }
                    SendCommand("MEM:TABL:FREQ " + FreqList);
                }
                public string GetFrequencyList()
                {
                    return (ReadCommand("MEM:TABL:FREQ?"));
                }
                public int GetFrequencyPoint()
                {
                    return (Convert.ToInt32(ReadCommand("MEM:TABL:FREQ:POIN?")));
                }
                public void SetGainList(double[] List)
                {
                    int Length;
                    string GainList;

                    Length = List.Length;
                    GainList = "";
                    for (int i = 0; i <= Length; i++)
                    {
                        if (i == 0)
                        {
                            GainList = List[0].ToString();
                        }
                        else
                        {
                            GainList = GainList + "," + List[i].ToString();
                        }
                    }
                    SendCommand("MEM:TABL:GAIN " + GainList);
                }
                public string GetGainList()
                {
                    return (ReadCommand("MEM:TABL:GAIN?"));
                }
                public int GetGainPoint()
                {
                    return (Convert.ToInt32(ReadCommand("MEM:TABL:GAIN:POIN?")));
                }
                public void Rename(string Name, string New_Name)
                {
                    SendCommand("MEM:TABL:MOVE \"" + Name.Trim() + "\",\"" + New_Name.Trim() + "\"");
                }
                public void SelectSensor(string Sensor)
                {
                    SendCommand("MEM:TABL:SEL \"" + Sensor.Trim() + "\"");
                }
                public string GetSelectedSensor()
                {
                    return (ReadCommand("MEM:TABL:SEL?"));
                }

            }
            public class cOutput : cCommonFunction
            {
                public cRecorder Recorder;
                public cROscillator ROscillator;
                public cTrigger Trigger;
                public cTTL TTL;

                public cOutput(FormattedIO488 parse)
                    : base(parse)
                {
                    Recorder = new cRecorder(parse);
                    ROscillator = new cROscillator(parse);
                    Trigger = new cTrigger(parse);
                    TTL = new cTTL(parse);
                }
                public class cRecorder : cCommonFunction
                {
                    public cRecorder(FormattedIO488 parse) : base(parse) { }
                    public void SetFeed(int Calc)
                    {
                        SendCommand("OUTP:REC:FEED \"CALC" + Calc.ToString() + "\"");
                    }
                    public void SetFeed(int Recorder, int Calc)
                    {
                        SendCommand("OUTP:REC" + Recorder.ToString() + ":FEED \"CALC" + Calc.ToString() + "\"");
                    }
                    public string GetFeed()
                    {
                        return (ReadCommand("OUTP:REC:FEED?"));
                    }
                    public string GetFeed(int Recorder)
                    {
                        return (ReadCommand("OUTP:REC" + Recorder.ToString() + ":FEED?"));
                    }
                    public void SetLimit(int Scale, double Limit)
                    {
                        if (Scale == 1)
                        {
                            SendCommand("OUTP:REC:LIM:UPP " + Limit.ToString());
                        }
                        else if (Scale == 0)
                        {
                            SendCommand("OUTP:REC:LIM:LOW " + Limit.ToString());
                        }
                    }
                    public void SetLimit(int Recorder, int Scale, double Limit)
                    {
                        if (Scale == 1)
                        {
                            SendCommand("OUTP:REC" + Recorder.ToString() + ":LIM:UPP " + Limit.ToString());
                        }
                        else if (Scale == 0)
                        {
                            SendCommand("OUTP:REC" + Recorder.ToString() + ":LIM:LOW " + Limit.ToString());
                        }
                    }
                    public double GetLimit(int Scale)
                    {
                        if (Scale == 1)
                        {
                            return Convert.ToDouble(ReadCommand("OUTP:REC:LIM:UPP?"));
                        }
                        else if (Scale == 0)
                        {
                            return Convert.ToDouble(ReadCommand("OUTP:REC:LIM:LOW?"));
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    public double GetLimit(int Recorder, int Scale)
                    {
                        if (Scale == 1)
                        {
                            return Convert.ToDouble(ReadCommand("OUTP:REC" + Recorder.ToString() + ":LIM:UPP?"));
                        }
                        else if (Scale == 0)
                        {
                            return Convert.ToDouble(ReadCommand("OUTP:REC" + Recorder.ToString() + ":LIM:LOW?"));
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    public void SetState(bool State)
                    {
                        SendCommand("OUTP:REC:STAT " + State.GetHashCode().ToString());
                    }
                    public void SetState(int Recorder, bool State)
                    {
                        SendCommand("OUTP:REC" + Recorder.ToString() + ":STAT " + State.GetHashCode().ToString());
                    }
                    public bool GetState()
                    {
                        return (common.CStr2Bool(ReadCommand("OUTP:REC:STAT?")));
                    }
                    public bool GetState(int Recorder)
                    {
                        return (common.CStr2Bool(ReadCommand("OUTP:REC" + Recorder.ToString() + ":STAT?")));
                    }
                }
                public class cROscillator : cCommonFunction
                {
                    public cROscillator(FormattedIO488 parse) : base(parse) { }
                    public void SetState(bool State)
                    {
                        SendCommand("OUTP:ROSC:STAT " + State.GetHashCode().ToString());
                    }
                    public bool GetState()
                    {
                        return (common.CStr2Bool(ReadCommand("OUTP:ROSC:STAT?")));
                    }
                }
                public class cTrigger : cCommonFunction
                {
                    public cTrigger(FormattedIO488 parse) : base(parse) { }
                    public void SetTrigger(bool State)
                    {
                        SendCommand("OUTP:TRIG:STAT " + State.GetHashCode().ToString());
                    }
                    public bool GetTrigger()
                    {
                        return (common.CStr2Bool(ReadCommand("OUTP:TRIG:STAT?")));
                    }
                }
                public class cTTL : cCommonFunction
                {
                    public cTTL(FormattedIO488 parse) : base(parse) { }
                    public void SetActiveLevel(HighLow output)
                    {
                        SendCommand("OUTP:TTL:ACT " + output.ToString().ToUpper());
                    }
                    public void SetActiveLevel(int Channel, HighLow output)
                    {
                        SendCommand("OUTP:TTL" + Channel.ToString() + ":ACT " + output.ToString().ToUpper());
                    }
                    public HighLow GetActiveLevel()
                    {
                        return (CStr2HighLow(ReadCommand("OUTP:TTL:ACT?")));
                    }
                    public HighLow GetActiveLevel(int Output)
                    {
                        return (CStr2HighLow(ReadCommand("OUTP:TTL" + Output.ToString() + ":ACT?")));
                    }
                    public void SetFeed(string Limit)
                    {
                        SendCommand("OUTP:TTL:FEED \"" + Limit.Trim() + "\"");
                    }
                    public void SetFeed(int Channel, string Limit)
                    {
                        SendCommand("OUTP:TTL" + Channel.ToString() + ":FEED \"" + Limit.Trim() + "\"");
                    }
                    public string GetFeed()
                    {
                        return ReadCommand("OUTP:TTL:FEED?");
                    }
                    public string GetFeed(int Channel)
                    {
                        return ReadCommand("OUTP:TTL" + Channel.ToString() + ":FEED?");
                    }
                    public void SetState(bool State)
                    {
                        SendCommand("OUTP:TTL:STAT " + State.GetHashCode().ToString());
                    }
                    public void SetState(int Channel, bool State)
                    {
                        SendCommand("OUTP:TTL" + Channel.ToString() + ":STAT " + State.GetHashCode().ToString());
                    }
                    public bool GetState()
                    {
                        return (common.CStr2Bool(ReadCommand("OUTP:TTL:STAT?")));
                    }
                    public bool GetState(int Channel)
                    {
                        return (common.CStr2Bool(ReadCommand("OUTP:TTL" + Channel.ToString() + ":STAT?")));
                    }
                }
            }


        }
        public class cSense : cCommonFunction
        {
            public cAverage Average;
            public cAverage2 Average2;
            public cBandwidth Bandwidth;
            public cCorrection Correction;
            public cDetector Detector;
            public cFrequency Frequency;
            public cMeasurementRate MeasurementRate;
            public cPower Power;
            public cSpeed Speed;
            public cSweep Sweep;
            public cTrace Trace;
            public cV2P V2P;


            public cSense(FormattedIO488 parse)
                : base(parse)
            {
                Average = new cAverage(parse);
                Average2 = new cAverage2(parse);
                Bandwidth = new cBandwidth(parse);
                Correction = new cCorrection(parse);
                Detector = new cDetector(parse);
                Frequency = new cFrequency(parse);
                MeasurementRate = new cMeasurementRate(parse);
                Power = new cPower(parse);
                Speed = new cSpeed(parse);
                Sweep = new cSweep(parse);
                Trace = new cTrace(parse);
                V2P = new cV2P(parse);
            }
            public class cAverage : cCommonFunction
            {
                public cAverage(FormattedIO488 parse) : base(parse) { }
                public void setCount(int Count)
                {
                    SendCommand("AVER:COUN " + Count.ToString());
                }
                public void setCount(string Count)
                {
                    SendCommand("AVER:COUN " + Count);
                }
                public void setCount(int Channel, int Count)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER:COUN " + Count.ToString());
                }
                public void setCount(int Channel, string Count)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER:COUN " + Count);
                }
                public int getCount()
                {
                    return (Convert.ToInt32(ReadCommand("AVER:COUN?")));
                }
                public int getCount(string Count)
                {
                    return (Convert.ToInt32(ReadCommand("AVER:COUN? " + Count)));
                }
                public int getCount(int Channel)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + Channel.ToString() + ":AVER:COUN?")));
                }
                public int getCount(int Channel, string Count)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + Channel.ToString() + ":AVER:COUN? " + Count)));
                }
                public void setAuto(bool State)
                {
                    SendCommand("AVER:COUN:AUTO " + State.GetHashCode().ToString());
                }
                public void setAutoOnce()
                {
                    SendCommand("AVER:COUN:AUTO ONCE");
                }
                public void setAuto(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER:COUN:AUTO " + State.GetHashCode().ToString());
                }
                public void setAutoOnce(int Channel)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER:COUN:AUTO ONCE");
                }
                public bool getAuto()
                {
                    return (common.CStr2Bool(ReadCommand("AVER:COUN:AUTO?")));
                }
                public bool getAuto(int Channel)
                {
                    return (common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":AVER:COUN:AUTO?")));
                }
                public void setStepDetect(bool State)
                {
                    SendCommand("SENS:AVER:SDET " + State.GetHashCode().ToString());
                }
                public void SetStepDetect(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER:SDET " + State.GetHashCode().ToString());
                }
                public bool getStepDetect()
                {
                    return (common.CStr2Bool(ReadCommand("AVER:SDET?")));
                }
                public bool getStepDetect(int Channel)
                {
                    return (common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":AVER:SDET?")));
                }
                public void setState(bool State)
                {
                    SendCommand("AVER " + State.GetHashCode().ToString());
                }
                public void setState(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER " + State.GetHashCode().ToString());
                }
                public bool GetState()
                {
                    return (common.CStr2Bool(ReadCommand("AVER?")));
                }
                public bool GetState(int Channel)
                {
                    return (common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":AVER?")));
                }
            }
            public class cAverage2 : cCommonFunction
            {
                public cAverage2(FormattedIO488 parse) : base(parse) { }
                public void setCount(int Count)
                {
                    SendCommand("AVER2:COUN " + Count.ToString());
                }
                public void setCount(string Count)
                {
                    SendCommand("AVER2:COUN " + Count);
                }
                public void setCount(int Channel, int Count)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER2:COUN " + Count.ToString());
                }
                public void setCount(int Channel, string Count)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER2:COUN " + Count);
                }
                public int getCount()
                {
                    return (Convert.ToInt32(ReadCommand("AVER2:COUN?")));
                }
                public int getCount(string Count)
                {
                    return (Convert.ToInt32(ReadCommand("AVER2:COUN? " + Count)));
                }
                public int getCount(int Channel)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + Channel.ToString() + ":AVER2:COUN?")));
                }
                public int getCount(int Channel, string Count)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + Channel.ToString() + ":AVER2:COUN? " + Count)));
                }
                public void setAuto(bool State)
                {
                    SendCommand("AVER2:COUN:AUTO " + State.GetHashCode().ToString());
                }
                public void setAutoOnce()
                {
                    SendCommand("AVER2:COUN:AUTO ONCE");
                }
                public void setAuto(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER2:COUN:AUTO " + State.GetHashCode().ToString());
                }
                public void setAutoOnce(int Channel)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER2:COUN:AUTO ONCE");
                }
                public bool getAuto()
                {
                    return (common.CStr2Bool(ReadCommand("AVER2:COUN:AUTO?")));
                }
                public bool getAuto(int Channel)
                {
                    return (common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":AVER2:COUN:AUTO?")));
                }
                public void setStepDetect(bool State)
                {
                    SendCommand("SENS:AVER2:SDET " + State.GetHashCode().ToString());
                }
                public void SetStepDetect(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER2:SDET " + State.GetHashCode().ToString());
                }
                public bool getStepDetect()
                {
                    return (common.CStr2Bool(ReadCommand("AVER2:SDET?")));
                }
                public bool getStepDetect(int Channel)
                {
                    return (common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":AVER:SDET?")));
                }
                public void setState(bool State)
                {
                    SendCommand("AVER2 " + State.GetHashCode().ToString());
                }
                public void setState(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":AVER2 " + State.GetHashCode().ToString());
                }
                public bool GetState()
                {
                    return (common.CStr2Bool(ReadCommand("AVER2?")));
                }
                public bool GetState(int Channel)
                {
                    return (common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":AVER2?")));
                }
            }
            public class cBandwidth : cCommonFunction
            {
                public cBandwidth(FormattedIO488 parse) : base(parse) { }

                public void Set(Video_Bandwidth VBW)
                {
                    SendCommand("BAND:VID " + CVideoBandwidth2Str(VBW));
                }
                public void Set(string VBW)
                {
                    SendCommand("BAND:VID " + VBW.Trim());
                }
                public void Set(int Channel, Video_Bandwidth VBW)
                {
                    SendCommand("SENS" + Channel.ToString() + ":BAND:VID " + CVideoBandwidth2Str(VBW));
                }
                public void Set(int Channel, string VBW)
                {
                    SendCommand("SENS" + Channel.ToString() + ":BAND:VID " + VBW.Trim());
                }

                public Video_Bandwidth Get()
                {
                    return CStr2VideoBandwidth(ReadCommand("BAND:VID?"));
                }
                //public string Get()
                //{
                //    return ReadCommand("BAND:VID?");
                //}
                public Video_Bandwidth Get(int Channel)
                {
                    return CStr2VideoBandwidth(ReadCommand("SENS" + Channel.ToString() + ":BAND:VID?"));
                }
                //public string Get(int Channel)
                //{
                //    return ReadCommand("SENS" + Channel.ToString() + ":BAND:VID?");
                //}
            }
            public class cCorrection : cCommonFunction
            {
                public cCorrection(FormattedIO488 parse) : base(parse) { }
                public void SetGain(int Gain)
                {
                    SendCommand("CORR:GAIN1 " + Gain.ToString());
                }
                public void SetGain(string Gain)
                {
                    SendCommand("CORR:GAIN1 " + Gain);
                }
                public void SetGain(int Channel, int Gain)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN1 " + Gain.ToString());
                }
                public void SetGain(int Channel, string Gain)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN1 " + Gain);
                }
                public int GetGain()
                {
                    return (Convert.ToInt32(ReadCommand("CORR:GAIN?")));
                }
                public int GetGain(int Channel)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN?")));
                }
                public int GetGain(string Gain)
                {
                    return (Convert.ToInt32(ReadCommand("CORR:GAIN? " + Gain)));
                }
                public int GetGain(int Channel, string Gain)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN? " + Gain)));
                }

                public void SelectSensorCalTable(string SensorTable)
                {
                    SendCommand("CORR:CSET1 \"" + SensorTable + "\"");
                }
                public void SelectSensorCalTable(int SensorNumber, string SensorTable)
                {
                    SendCommand("CORR:CSET" + SensorNumber.ToString() + " \"" + SensorTable + "\"");
                }
                public void SelectSensorCalTable(int Channel, int SensorNumber, string SensorTable)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:CSET" + SensorNumber.ToString() + " \"" + SensorTable + "\"");
                }
                public string RetrieveSensorCalTable()
                {
                    return (ReadCommand("CORR:CSET1?"));
                }
                public string RetrieveSensorCalTable(int SensorNumber)
                {
                    return (ReadCommand("CORR:CSET" + SensorNumber.ToString() + "?"));
                }
                public string RetrieveSensorCalTable(int Channel, int SensorNumber)
                {
                    return (ReadCommand("SENS" + Channel.ToString() + ":CORR:CSET" + SensorNumber.ToString() + "?"));
                }

                public void SetSensorCalTableState(bool State)
                {
                    SendCommand("CORR:CSET1:STAT " + State.GetHashCode().ToString());
                }
                public void SetSensorCalTableState(int SensorNumber, bool State)
                {
                    SendCommand("CORR:CSET" + SensorNumber.ToString() + ":STAT " + State.GetHashCode().ToString());
                }
                public void SetSensorCalTableState(int Channel, int SensorNumber, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:CSET" + SensorNumber.ToString() + ":STAT " + State.GetHashCode().ToString());
                }
                public bool GetSensorCalTableState()
                {
                    return common.CStr2Bool(ReadCommand("CORR:CSET1:STAT?"));
                }
                public bool GetSensorCalTableState(int SensorNumber)
                {
                    return common.CStr2Bool(ReadCommand("CORR:CSET" + SensorNumber.ToString() + ":STAT?"));
                }
                public bool GetSensorCalTableState(int Channel, int SensorNumber)
                {
                    return common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":CORR:CSET" + SensorNumber.ToString() + ":STAT?"));
                }

                public void SetDutyCycle(double DutyCycle)
                {
                    SendCommand("CORR:GAIN3 " + DutyCycle.ToString() + "PCT");
                }
                public void SetDutyCycle(string DutyCycle)
                {
                    SendCommand("CORR:GAIN3 " + DutyCycle + "PCT");
                }
                public void SetDutyCycle(int Channel, double DutyCycle)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN3 " + DutyCycle.ToString() + "PCT");
                }
                public void SetDutyCycle(int Channel, string DutyCycle)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN3 " + DutyCycle + "PCT");
                }
                public double GetDutyCycle()
                {
                    return (Convert.ToDouble(ReadCommand("CORR:GAIN3?")));
                }
                public double GetDutyCycle(string Input)
                {
                    return (Convert.ToDouble(ReadCommand("CORR:GAIN3? " + Input)));
                }
                public double GetDutyCycle(int Channel)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN3?")));
                }
                public double GetDutyCycle(int Channel, string Input)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN3? " + Input)));
                }

                public void SetDutyCycleState(bool State)
                {
                    SendCommand("CORR:DCYC:STAT " + State.GetHashCode().ToString());
                }
                public void SetDutyCycleState(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:DCYC:STAT " + State.GetHashCode().ToString());
                }
                public bool GetDutyCycleState()
                {
                    return common.CStr2Bool(ReadCommand("CORR:GAIN3:STAT?"));
                }
                public bool GetDutyCycleState(int Channel)
                {
                    return common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN3:STAT?"));
                }

                public double GetFreqOffset()
                {
                    return (Convert.ToDouble(ReadCommand("CORR:GAIN4?")));
                }
                public double GetFreqOffset(int Channel)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN4?")));
                }

                public void SetChannelOffsetState(bool State)
                {
                    SendCommand("CORR:GAIN2:STAT " + State.GetHashCode().ToString());
                }
                public void SetChannelOffsetState(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN2:STAT " + State.GetHashCode().ToString());
                }
                public bool GetChannelOffsetState()
                {
                    return common.CStr2Bool(ReadCommand("CORR:GAIN2:STAT?"));
                }
                public bool GetChannelOffsetState(int Channel)
                {
                    return common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN2:STAT?"));
                }

                public void SetChannelOffset(double OffsetVal)
                {
                    SendCommand("CORR:GAIN2 " + OffsetVal.ToString());
                }
                public void SetChannelOffset(string OffsetVal)
                {
                    SendCommand("CORR:GAIN2 " + OffsetVal);
                }
                public void SetChannelOffset(int Channel, double OffsetVal)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN2 " + OffsetVal.ToString());
                }
                public void SetChannelOffset(int Channel, string OffsetVal)
                {
                    SendCommand("SENS" + Channel.ToString() + ":CORR:GAIN2 " + OffsetVal);
                }
                public double GetChannelOffset()
                {
                    return (Convert.ToDouble(ReadCommand("CORR:GAIN2?")));
                }
                public double GetChannelOffset(string Input)
                {
                    return (Convert.ToDouble(ReadCommand("CORR:GAIN2? " + Input)));
                }
                public double GetChannelOffset(int Channel)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN2?")));
                }
                public double GetChannelOffset(int Channel, string Input)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":CORR:GAIN2? " + Input)));
                }
            }
            public class cDetector : cCommonFunction
            {
                public cDetector(FormattedIO488 parse) : base(parse) { }
                public void SetFunction(Measurement_Mode Mode)
                {
                    SendCommand("DET:FUNC " + CMeasMode2Str(Mode));
                }
                public void SetFunction(string Mode)
                {
                    SendCommand("DET:FUNC " + Mode);
                }
                public void SetFunction(int Channel, Measurement_Mode Mode)
                {
                    SendCommand("SENS" + Channel.ToString() + ":DET:FUNC " + CMeasMode2Str(Mode));
                }
                public void SetFunction(int Channel, string Mode)
                {
                    SendCommand("SENS" + Channel.ToString() + ":DET:FUNC " + Mode);
                }

                public Measurement_Mode GetFunction()
                {
                    return CStr2MeasMode(ReadCommand("DET:FUNC?"));
                }
                //public string GetFunction()
                //{
                //    return ReadCommand("DET:FUNC?");
                //}
                public Measurement_Mode GetFunction(int Channel)
                {
                    return CStr2MeasMode(ReadCommand("SENS" + Channel.ToString() + ":DET:FUNC?"));
                }
                //public string GetFunction(int Channel)
                //{
                //    return ReadCommand("SENS" + Channel.ToString() + ":DET:FUNC?");
                //}
            }
            public class cFrequency : cCommonFunction
            {
                public cFrequency(FormattedIO488 parse) : base(parse) { }

                public void Set(double Frequency)
                {
                    SendCommand("FREQ " + Frequency.ToString());
                }
                public void Set(string Frequency)
                {
                    SendCommand("FREQ " + Frequency);
                }
                public void Set(int Channel, double Frequency)
                {
                    SendCommand("SENS" + Channel.ToString() + ":FREQ " + Frequency.ToString());
                }
                public void Set(int Channel, string Frequency)
                {
                    SendCommand("SENS" + Channel.ToString() + ":FREQ " + Frequency);
                }

                public double Get()
                {
                    return Convert.ToDouble(ReadCommand("FREQ?"));
                }
                public double Get(string Input)
                {
                    return Convert.ToDouble(ReadCommand("FREQ? " + Input));
                }
                public double Get(int Channel)
                {
                    return Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":FREQ?"));
                }
                public double Get(int Channel, string Input)
                {
                    return Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":FREQ? " + Input));
                }
            }
            public class cMeasurementRate : cCommonFunction
            {
                public cMeasurementRate(FormattedIO488 parse) : base(parse) { }
                public void Set(Measurement_Speed Rate)
                {
                    SendCommand("MRAT " + CMeasRate2Str(Rate));
                }
                public void Set(string Rate)
                {
                    SendCommand("MRAT " + Rate);
                }
                public void Set(int Channel, Measurement_Speed Rate)
                {
                    SendCommand("SENS" + Channel.ToString() + ":MRAT " + CMeasRate2Str(Rate));
                }
                public void Set(int Channel, string Rate)
                {
                    SendCommand("SENS" + Channel.ToString() + ":MRAT " + Rate);
                }
                public Measurement_Speed Get()
                {
                    return CStr2MeasRate(ReadCommand("MRAT?"));
                }
                public Measurement_Speed Get(int Channel)
                {
                    return CStr2MeasRate(ReadCommand("SENS" + Channel.ToString() + ":MRAT?"));
                }
            }
            public class cPower : cCommonFunction
            {
                public cPower(FormattedIO488 parse) : base(parse) { }
                public void SetACRange(UpperLower Scale)
                {
                    SendCommand("POW:AC:RANG " + Scale.ToString());
                }
                public void SetACRange(int Channel, UpperLower Scale)
                {
                    SendCommand("SENS" + Channel.ToString() + ":POW:AC:RANG " + Scale.ToString());
                }
                public UpperLower GetACRange()
                {
                    return CStr2UpperLower(ReadCommand("POW:AC:RANG?"));
                }
                public UpperLower GetACRange(int Channel)
                {
                    return CStr2UpperLower(ReadCommand("SENS" + Channel.ToString() + "POW:AC:RANG?"));
                }
                public void SetACAutoRange(bool State)
                {
                    SendCommand("POW:AC:RANG:AUTO " + State.GetHashCode().ToString());
                }
                public void SetACAutoRange(int Channel, bool State)
                {
                    SendCommand("SENS" + Channel.ToString() + ":POW:AC:RANG:AUTO " + State.GetHashCode().ToString());
                }
                public bool GetACAutoRange()
                {
                    return common.CStr2Bool(ReadCommand("POW:AC:RANG:AUTO?"));
                }
                public bool GetACAutoRange(int Channel)
                {
                    return common.CStr2Bool(ReadCommand("SENS" + Channel.ToString() + ":POW:AC:RANG:AUTO?"));
                }
            }
            public class cSpeed : cCommonFunction
            {
                public cSpeed(FormattedIO488 parse) : base(parse) { }

                public void Set(Measurement_Speed Speed)
                {
                    SendCommand("SPE " + Speed.GetHashCode().ToString());
                }
                public void Set(int Speed)
                {
                    SendCommand("SPE " + Speed.ToString());
                }
                public void Set(int Channel, Measurement_Speed Speed)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SPE " + Speed.GetHashCode().ToString());
                }
                public void Set(int Channel, int Speed)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SPE " + Speed.ToString());
                }
                public Measurement_Speed Get()
                {
                    return (CStr2MeasRate(ReadCommand("SPE?")));
                }
                public Measurement_Speed Get(int Channel)
                {
                    return (CStr2MeasRate(ReadCommand("SENS" + Channel.ToString() + ":SPE?")));
                }
            }
            public class cSweep : cCommonFunction
            {
                public cSweep(FormattedIO488 parse) : base(parse) { }
                public void SetOffsetTime(int Gate, double Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":OFFS:TIME " + Time.ToString());
                }
                public void SetOffsetTime(int Gate, string Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":OFFS:TIME " + Time);
                }
                public void SetOffsetTime(int Channel, int Gate, double Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":OFFS:TIME " + Time.ToString());
                }
                public void SetOffsetTime(int Channel, int Gate, string Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":OFFS:TIME " + Time);
                }
                public double GetOffsetTime(int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS:SWE" + Gate.ToString() + ":OFFS:TIME?")));
                }
                public double GetOffsetTime(int Channel, int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":OFFS:TIME?")));
                }

                public void SetSweepTime(int Gate, double Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":TIME " + Time.ToString());
                }
                public void SetSweepTime(int Gate, string Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":TIME " + Time);
                }
                public void SetSweepTime(int Channel, int Gate, double Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":TIME " + Time.ToString());
                }
                public void SetSweepTime(int Channel, int Gate, string Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":TIME " + Time);
                }
                public double GetSweepTime(int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS:SWE" + Gate.ToString() + ":TIME?")));
                }
                public double GetSweepTime(int Channel, int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":TIME?")));
                }

            }
            public class cTrace : cCommonFunction
            {
                public cTrace(FormattedIO488 parse) : base(parse) { }

                public void SetLimit(UpperLower Scale, double Limit)
                {
                    SendCommand("SENS:TRAC:LIM:" + Scale.ToString() + " " + Limit.ToString());
                }
                public void SetLimit(UpperLower Scale, string Limit)
                {
                    SendCommand("SENS:TRAC:LIM:" + Scale.ToString() + " " + Limit);
                }
                public void SetLimit(int Channel, UpperLower Scale, double Limit)
                {
                    SendCommand("SENS" + Channel.ToString() + ":TRAC:LIM:" + Scale.ToString() + " " + Limit.ToString());
                }
                public void SetLimit(int Channel, UpperLower Scale, string Limit)
                {
                    SendCommand("SENS" + Channel.ToString() + ":TRAC:LIM:" + Scale.ToString() + " " + Limit);
                }

                public double GetLimit(UpperLower Scale)
                {
                    return (Convert.ToDouble(ReadCommand("SENS:TRAC:LIM:" + Scale.ToString() + "?")));
                }
                public double GetLimit(UpperLower Scale, String Input)
                {
                    return (Convert.ToDouble(ReadCommand("SENS:TRAC:LIM:" + Scale.ToString() + "? " + Input)));
                }
                public double GetLimit(int Channel, UpperLower Scale)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":TRAC:LIM:" + Scale.ToString() + "?")));
                }
                public double GetLimit(int Channel, UpperLower Scale, String Input)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":TRAC:LIM:" + Scale.ToString() + "? " + Input)));
                }

                public void SetOffsetTime(int Gate, double Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":OFFS:TIME " + Time.ToString());
                }
                public void SetOffsetTime(int Gate, string Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":OFFS:TIME " + Time);
                }
                public void SetOffsetTime(int Channel, int Gate, double Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":OFFS:TIME " + Time.ToString());
                }
                public void SetOffsetTime(int Channel, int Gate, string Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":OFFS:TIME " + Time);
                }

                public double GetOffsetTime(int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS:SWE" + Gate.ToString() + ":OFFS:TIME?")));
                }
                public double GetOffsetTime(int Channel, int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":OFFS:TIME?")));
                }

                public void SetTime(int Gate, double Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":TIME " + Time.ToString());
                }
                public void SetTime(int Gate, string Time)
                {
                    SendCommand("SENS:SWE" + Gate.ToString() + ":TIME " + Time);
                }
                public void SetTime(int Channel, int Gate, double Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":TIME " + Time.ToString());
                }
                public void SetTime(int Channel, int Gate, string Time)
                {
                    SendCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":TIME " + Time);
                }

                public double GetTime(int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS:SWE" + Gate.ToString() + ":TIME?")));
                }
                public double GetTime(int Channel, int Gate)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + Channel.ToString() + ":SWE" + Gate.ToString() + ":TIME?")));
                }

                public void SetUnit(Unit_Power Unit)
                {
                    SendCommand("SENS:TRAC:UNIT " + CUnit2Str(Unit));
                }
                public void SetUnit(string Unit)
                {
                    SendCommand("SENS:TRAC:UNIT " + Unit);
                }
                public void SetUnit(int Channel, Unit_Power Unit)
                {
                    SendCommand("SENS" + Channel.ToString() + ":TRAC:UNIT " + CUnit2Str(Unit));
                }
                public void SetUnit(int Channel, string Unit)
                {
                    SendCommand("SENS" + Channel.ToString() + ":TRAC:UNIT " + Unit);
                }
                public Unit_Power GetUnit()
                {
                    return (CStr2Unit(ReadCommand("SENS:TRAC:UNIT?")));
                }
                public Unit_Power GetUnit(int Channel)
                {
                    return (CStr2Unit(ReadCommand("SENS" + Channel.ToString() + ":TRAC:UNIT?")));
                }
            }
            public class cV2P : cCommonFunction
            {
                public cV2P(FormattedIO488 parse) : base(parse) { }

                public void SetLinearity(Linearity_Correction_Type Type)
                {
                    SendCommand("SENS:V2P " + CLinearity_Type2Str(Type));
                }
                public void SetLinearity(int Channel, Linearity_Correction_Type Type)
                {
                    SendCommand("SENS" + Channel.ToString() + ":V2P " + CLinearity_Type2Str(Type));
                }
                public string GetLinearity()
                {
                    return (ReadCommand("SENS:V2P?"));
                }
                //public Linearity_Correction_Type GetLinearity()
                //{
                //    return CStr2Linearity_Type(ReadCommand("SENS:V2P?"));
                //}
                public string GetLinearity(int Channel)
                {
                    return (ReadCommand("SENS" + Channel.ToString() + ":V2P?"));
                }
                //public Linearity_Correction_Type GetLinearity(int Channel)
                //{
                //    return CStr2Linearity_Type(ReadCommand("SENS" + Channel.ToString() + ":V2P?"));
                //}
            }
        }
        public class cStatus : cCommonFunction
        {
            public cStatus(FormattedIO488 parse) : base(parse) { }
            public void SetEnable(string Value)
            {
                SendCommand("ENAB " + Value);
            }
            public string GetEnable()
            {
                return (ReadCommand("ENAB?"));
            }
            public void SetNTR(string Value)
            {
                SendCommand("NTR " + Value);
            }
            public string GetNTR()
            {
                return (ReadCommand("NTR?"));
            }
            public void SetPTR(string Value)
            {
                SendCommand("PTR " + Value);
            }
            public string GetPTR()
            {
                return (ReadCommand("PTR?"));
            }
        }
        public class cSystem : cCommonFunction
        {
            public cSystem(FormattedIO488 parse) : base(parse) { }
            public void SetGPIBAddress(int Address)
            {
                SendCommand("SYST:COMM:GPIB:ADDR " + Address.ToString());
            }
            public int GetGPIBAddress()
            {
                return (Convert.ToInt32(ReadCommand("SYST:COMM:GPIB:ADDR?")));
            }
            public void SetSerialConfig(int Mode, Serial_Settings Setting)
            {
                if (Mode == 1)
                {
                    SendCommand("SYST:COMM:SER:TRAN:ECHO " + Setting.TransmitECHO.GetHashCode().ToString());
                }
                else
                {
                    SendCommand("SYST:COMM:SER:TRAN:DTR " + Setting.DTR.GetHashCode().ToString());
                    SendCommand("SYST:COMM:SER:TRAN:RTS " + Setting.RTS.GetHashCode().ToString());
                }
                SendCommand("SYST:COMM:SER:TRAN:BAUD " + Setting.BAUD.ToString());
                SendCommand("SYST:COMM:SER:TRAN:BIT " + Setting.BITs.ToString());
                SendCommand("SYST:COMM:SER:TRAN:PACE " + Setting.PACE);
                SendCommand("SYST:COMM:SER:TRAN:PAR " + Setting.PARity);
                SendCommand("SYST:COMM:SER:TRAN:SBIT " + Setting.SBITs);
            }
            public Serial_Settings GetSerialConfig(int Mode)
            {
                Serial_Settings Setting;
                Setting = new Serial_Settings();

                if (Mode == 1)
                {
                    Setting.Auto = Convert.ToInt32(ReadCommand("SYST:COMM:SER:TRAN:AUTO?"));
                    Setting.TransmitECHO = common.CStr2Bool(ReadCommand("SYST:COMM:SER:TRAN:ECHO?"));
                }
                else
                {
                    Setting.DTR = common.CStr2Bool(ReadCommand("SYST:COMM:SER:TRAN:DTR?"));
                    Setting.RTS = common.CStr2Bool(ReadCommand("SYST:COMM:SER:TRAN:RTS?"));
                }
                Setting.BAUD = Convert.ToInt32(ReadCommand("SYST:COMM:SER:TRAN:BAUD?"));
                Setting.BITs = Convert.ToInt32(ReadCommand("SYST:COMM:SER:TRAN:BITs?"));
                Setting.PACE = ReadCommand("SYST:COMM:SER:TRAN:PACE?");
                Setting.PARity = ReadCommand("SYST:COMM:SER:TRAN:PAR?");
                Setting.SBITs = Convert.ToInt32(ReadCommand("SYST:COMM:SER:TRAN:SBIT?"));

                return Setting;
            }
            public string GetHelpHeader()
            {
                return (ReadCommand("SYST:HELP:HEAD?"));
            }
            public void Local()
            {
                SendCommand("SYST:LOC");
            }
            public void SetPresetData(PresetData Data)
            {
                SendCommand("SYST:PRES " + CPresetData2Str(Data));
            }
            public void SetPresetData(string Data)
            {
                SendCommand("SYST:PRES " + Data);
            }

        }
        #endregion
    }
}
