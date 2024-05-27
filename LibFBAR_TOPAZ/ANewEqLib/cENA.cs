using System;
using Ivi.Visa.Interop;
using MPAD_TestTimer;
using LibFBAR_TOPAZ.ANewEqLib;

namespace LibFBAR_TOPAZ
{
    /// <summary>
    /// To be replaced with new ENA class.
    /// </summary>
    public class cENA
    {
        public static string ClassName = "M9485 Class";//"ENA E5071A/B/C Class";
        private string IOAddress;
        //private Agilent.AgNA.Interop.AgNA ioENA;
        private FormattedIO488 ioENA;
        public Ivi.Visa.Interop.ResourceManagerClass mgr = new ResourceManagerClass();
        public Ivi.Visa.Interop.FormattedIO488Class M9485A = new FormattedIO488Class();


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
                        tmpStr += "," + SegmentTable.SegmentData[Seg].ifbw_value.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].pow_value.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].del_value.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].swp_value.ToString();
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
                        tmpStr += "," + SegmentTable.SegmentData[Seg].ifbw_value.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].pow_value.ToString();
                        tmpStr += "," + SegmentTable.SegmentData[Seg].del_value.ToString();
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
        /// <summary>
        /// Parsing IO (in FormattedIO488)
        /// </summary>
        public FormattedIO488 parseIO
        {
            get
            {
                return ioENA;
            }
            set
            {
                ioENA = parseIO;
            }
        }
        /// <summary>
        /// Open Equipment IO
        /// </summary>
        public void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    M9485A.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 20000, "");
                    M9485A.IO.Timeout = 90000;
                }
                catch (SystemException ex)
                {
                    PromptManager.Instance.ShowError(ex);
                    ioENA.IO = null;
                    return;
                }
                //Init(ioENA);
                Init(M9485A);
            }
        }
        /// <summary>
        /// Close Equipment IO
        /// </summary>
        public void CloseIO()
        {
            ioENA.IO.Close();
        }
        /// <summary>
        /// Driver Revision control
        /// </summary>
        /// <returns>Driver's Version</returns>
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01a";        //  14/11/2011       KKL             VISA Driver for ENA (Base on minimum required command)

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        #region "Class Initialization"
        public LibEqmtCommon2 BasicCommand; // Basic Command for General Equipment (Must be Initialized)
        public cCalculate Calculate;
        public cDisplay Display;
        public cFormat Format;
        public cInitiate Initiate;
        public cMemory Memory;
        public cSense Sense;
        public cTrigger Trigger;
        public cExternalDevice ExternalDevice; //Seoul 05112018
        /// <summary>
        /// Initializing all Parameters
        /// </summary>
        /// <param name="IOInit"></param>
        public void Init(FormattedIO488 IOInit)
        {
            BasicCommand = new LibEqmtCommon2(IOInit);
            Calculate = new cCalculate(IOInit);
            Display = new cDisplay(IOInit);
            Format = new cFormat(IOInit);
            Initiate = new cInitiate(IOInit);
            Memory = new cMemory(IOInit);
            Sense = new cSense(IOInit);
            Trigger = new cTrigger(IOInit);
            ExternalDevice = new cExternalDevice(IOInit);
        }
        #endregion

        #region "Class Functional Codes"
        /// <summary>
        /// Calculate Class Function.
        /// </summary>
        public class cCalculate : LibEqmtCommon2
        {
            public cFixtureSimulator FixtureSimulator;
            public cParameter Par;
            public cFormat Format;
            public cFunction Func;
            public cData Data;
            public cMath Math;

            public cCalculate(FormattedIO488 parse)
                : base(parse)
            {
                FixtureSimulator = new cFixtureSimulator(parse);
                Par = new cParameter(parse);
                Format = new cFormat(parse);
                Func = new cFunction(parse);
                Data = new cData(parse);
                Math = new cMath(parse);

            }
            /// <summary>
            /// Fixture Simulator Class Function
            /// </summary>
            public class cFixtureSimulator : LibEqmtCommon2
            {
                public cSended SENDed;
                public cFixtureSimulator(FormattedIO488 parse)
                    : base(parse)
                {
                    SENDed = new cSended(parse);
                }

                public class cSended : LibEqmtCommon2
                {
                    public cPMCircuit PMCircuit;
                    public cZConversion ZConversion;
                    public cSended(FormattedIO488 parse)
                        : base(parse)
                    {
                        PMCircuit = new cPMCircuit(parse);
                        ZConversion = new cZConversion(parse);
                    }
                    public class cPMCircuit : LibEqmtCommon2
                    {
                        public cPMCircuit(FormattedIO488 parse) : base(parse) { }
                        public void R(int PortNumber, double Resistance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:R " + Resistance.ToString());
                        }
                        public void R(int ChannelNumber, int PortNumber, double Resistance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:R " + Resistance.ToString());
                        }
                        public void L(int PortNumber, double Inductance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:L " + Inductance.ToString());
                        }
                        public void L(int ChannelNumber, int PortNumber, double Inductance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:L " + Inductance.ToString());
                        }
                        public void C(int PortNumber, double Capacitance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:C " + Capacitance.ToString());
                        }
                        public void C(int ChannelNumber, int PortNumber, double Capacitance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:C " + Capacitance.ToString());
                        }
                        public void G(int PortNumber, double Conductance)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:G " + Conductance.ToString());
                        }
                        public void G(int ChannelNumber, int PortNumber, double Conductance)
                        {
                            SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:G " + Conductance.ToString());
                        }

                        public void Type(int PortNumber, e_PortMatchType PortType)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public void Type(int ChannelNumber, int PortNumber, e_PortMatchType PortType)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }

                        public void User(int PortNumber)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:USER");
                        }
                        public void User(int ChannelNumber, int PortNumber)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:USER");
                        }

                        public void UserFilename(int PortNumber, string S2PFilename)
                        {
                            SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:USER:FIL \"" + S2PFilename + "\"");
                        }
                        public void UserFilename(int ChannelNumber, int PortNumber, string S2PFilename)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + "PAR:USER:FIL \"" + S2PFilename + "\"");
                        }

                        public void State(bool Set)
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
                        public void State(int ChannelNumber, bool Set)
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
                    public class cZConversion : LibEqmtCommon2
                    {
                        public cZConversion(FormattedIO488 parse) : base(parse) { }
                        public void Imag(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public void Real(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public void Z0(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }

                        public void State(bool Set)
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
                        public void State(int ChannelNumber, bool Set)
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
                public void State(bool Set)
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
                public void State(int ChannelNumber, bool Set)
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
                public bool State()
                {
                    return (bool.Parse(ReadCommand(":CALC1:FSIM:STAT?")));
                }
                public bool State(int ChannelNumber)
                {
                    return (bool.Parse(ReadCommand(":CALC" + ChannelNumber + ":FSIM:STAT?")));
                }
            }
            public class cParameter : LibEqmtCommon2
            {
                public cParameter(FormattedIO488 parse) : base(parse) { }
                //public void Count(int count)
                //{
                //    SendCommand("CALC1:PAR:COUN " + count.ToString());
                //}
                //public void Count(int ChannelNumber, int Trace)
                //{
                //    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN " + Trace.ToString());
                //}
                public void Count(int ChannelNumber, int Trace)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN " + Trace.ToString());
                    SendCommand("SOUR" + ChannelNumber.ToString() + ":POW:COUP OFF");

                }
                public int Count()
                {
                    return (Convert.ToInt32(ReadCommand("CALC1:PAR:COUN?")));
                }
                public int Count(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN?")));
                }
                public void Define(int Trace, e_SParametersDef Define)
                {
                    SendCommand("CALC1:PAR" + Trace.ToString() + ":DEF " + Define.ToString());
                }
                public void Define(int ChannelNumber, int Trace, e_SParametersDef Define)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF " + Define.ToString());
                }
                public void Define_Channel(int ChannelNumber)
                {
                    SendCommand("DISPlay:WINDow" + ChannelNumber.ToString() + ":STAT ON");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:DEL:ALL");
                }
                public void Define_Trace(int ChannelNumber, int Trace, string TraceName)
                {

                    if(TraceName.ToUpper().Contains("GDEL"))
                    {
                        TraceName = TraceName.Replace("GDEL", "S");
                    }
                    //if (TraceName == "GDEL21") TraceName = "S21"; //Added for GDEL21 Trace
                    //if (TraceName == "GDEL32") TraceName = "S32";
                    //if (TraceName == "GDEL42") TraceName = "S42";
                    //if (TraceName == "GDEL52") TraceName = "S52";
                    //if (TraceName == "GDEL62") TraceName = "S62";

                    SendCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + Trace.ToString() + ":PAR '" + TraceName + "'");
                }
                public string GetAllCategory(int ChannelNumber)
                {
                    // ENA: return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?"));
                    // ZVT:
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:CAT?"));
                    //CALC:MEAS:PAR?

                }
                public string GetTraceCategory(int ChannelNumber)
                {
                    return (ReadCommand("SYST:MEAS:CAT? " + ChannelNumber));
                }
                public e_SParametersDef Define_Enum(int ChannelNumber, int Trace)
                {
                    return (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?")));
                }
                public void Select(int Trace)
                {
                    SendCommand("CALC1:PAR" + Trace.ToString() + ":SEL");
                }
                public void Select(int ChannelNumber, int Trace)
                {
                    // SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":SEL");
                    SendCommand("DISP:WIND" + ChannelNumber.ToString() + ":TRAC" + Trace.ToString() + ":SEL");
                }
                public void SPORT(int Trace, double value)
                {
                    SendCommand("CALC1:PAR" + Trace.ToString() + ":SPOR " + value.ToString());
                }
                public void SPORT(int ChannelNumber, int Trace, double value)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":SPOR " + value.ToString());
                }
                public void SelectTrace(int ChannelNumber, int TraceNumber)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:MNUM:SEL " + TraceNumber.ToString() + " , fast");
                }
            }
            public class cFormat : LibEqmtCommon2
            {
                public cFormat(FormattedIO488 parse) : base(parse) { }
                public void Format(e_SFormat format)
                {
                    SendCommand("CALC1:FORM " + format.ToString());
                }
                public void Format(int ChannelNumber, e_SFormat format)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format.ToString());
                }
                public void Format(string format)
                {
                    SendCommand("CALC1:FORM " + format);
                }
                public void Format(int ChannelNumber, string format)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format);
                }
                public e_SFormat Format()
                {
                    string tmp = ReadCommand("CALC1:FORM?");
                    e_SFormat Format = (e_SFormat)Enum.Parse(typeof(e_SFormat), tmp);
                    return (Format);
                }
                public e_SFormat Format(int ChannelNumber)
                {
                    string tmp = ReadCommand("CALC" + ChannelNumber.ToString() + ":FORM?");
                    e_SFormat Format = (e_SFormat)Enum.Parse(typeof(e_SFormat), tmp);
                    return (Format);
                }

                //New
                public void Format(int ChannelNumber, int TraceNumber, e_SFormat format)
                {
                    // added code to select the trace before changing the Format

                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:mnumber:SEL " + TraceNumber.ToString());
                    // SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format.ToString());
                }
                public void Format(int ChannelNumber, int TraceNumber, string format)
                {
                    // added code to select the trace before changing the Format
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format);
                }
                public e_SFormat Format(int ChannelNumber, int TraceNumber)
                {
                    // added code to select the trace before reading the Format
                    //SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    string tmp = ReadCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM?");//" + ChannelNumber.ToString() + "
                    //string tmp = "SMIT";
                    e_SFormat Format = (e_SFormat)Enum.Parse(typeof(e_SFormat), tmp);
                    return (Format);
                }
                public string returnFormat(int ChannelNumber, int TraceNumber)
                {
                    // added code to select the trace before reading the Format
                    //SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM?"));//" + ChannelNumber.ToString() + "      
                }
                public void setFormat(int ChannelNumber, int TraceNumber, e_SFormat Format)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + Format);
                }
                public void setFormat(int ChannelNumber, int TraceNumber, string Format)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":FORM " + Format);
                }

                //public string Format()
                //{
                //    return (ReadCommand("CALC1:FORM?"));
                //}
                //public string Format(int ChannelNumber)
                //{
                //    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":FORM?"));
                //}
            }
            public class cFunction : LibEqmtCommon2
            {

                public cFunction(FormattedIO488 parse)
                    : base(parse)
                {

                }
                //public string Points()
                //{
                //    return (ReadCommand("CALC1:FUNC:POIN?"));
                //}
                //public string Points(int ChannelNumber)
                //{
                //    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":FUNC:POIN?"));
                //}
                public int Points()
                {
                    return (Convert.ToInt32(ReadCommand("CALC1:FUNC:POIN?")));
                }
                public int Points(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("CALC" + ChannelNumber.ToString() + ":FUNC:POIN?")));
                }
            }
            public class cMath : LibEqmtCommon2
            {
                public cMath(FormattedIO488 parse)
                    : base(parse)
                {

                }
                public void SetMath(int ChannelNumber, int TraceNumber)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":MATH:MEM");
                }
                public void MathOperation(int ChannelNumber, int TraceNumber)
                {
                    SendCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":MATH:FUNC DIV");
                }
            }
            #region "Selected"
            public class cData : LibEqmtCommon2
            {
                public int numofbins;
                public cData(FormattedIO488 parse) : base(parse) { }
                public double[] SData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:SDAT?"));
                }
                public double[] SData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:SDAT?"));
                }
                public double[] FData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:FDAT?"));
                }
                public double[] FData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:FDAT?"));
                }
                public double[] FData(int ChannelNumber, int TraceNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":DATA:FDATA?"));
                }
                public double[] SMemoryData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:SMEM?"));
                }
                public double[] SMemoryData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:SMEM?"));
                }
                public double[] FMemoryData()
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:FMEM?"));
                }
                public double[] FMemoryData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:FMEM?"));
                }
                public double[] FMultiTrace_Data(string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:MFD? \"" + TraceNumber + "\""));
                }
                public double[] FMultiTrace_Data(int ChannelNumber, string TraceNumber)
                {
                    // return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MFD? \"" + TraceNumber + "\""));
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:MFD? \"" + TraceNumber + "\""));
                }
                public double[] FMultiTrace_Data(int ChannelNumber, int TraceNumber)
                {
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MFD? \"" + TraceNumber + "\""));
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:MFD? \"" + TraceNumber.ToString() + "\""));
                }
                public double[] UMultiTrace_Data(string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC:SEL:DATA:MSD? \"" + TraceNumber + "\""));
                }
                public double[] UMultiTrace_Data(int ChannelNumber, string TraceNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MSD? \"" + TraceNumber + "\""));
                }

            }

            #endregion
        }
        public class cDisplay : LibEqmtCommon2
        {
            public cWindow Window;
            public cDisplay(FormattedIO488 parse)
                : base(parse)
            {
                Window = new cWindow(parse);
            }
            public void Enable(bool state)
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
            public void Update(bool state)
            {
                switch (state)
                {
                    case true:
                        SendCommand("DISP:UPD:STAT ON");
                        break;
                    case false:
                        SendCommand("DISP:UPD:STAT OFF");
                        break;
                }
            }
            public void WindowsTurnOn(int Wind)
            {
                SendCommand(":DISP:WIND" + Wind + " ON");
            }
            public void Visible(bool state)
            {
                switch (state)
                {
                    case true:
                        SendCommand(":DISP:VIS ON");
                        break;
                    case false:
                        SendCommand(":DISP:VIS OFF");
                        break;
                }


            }
            public class cWindow : LibEqmtCommon2
            {
                public cWindow(FormattedIO488 parse)
                    : base(parse)
                {

                }
                public void Activate(int ChannelNumber)
                {
                    SendCommand("DISP:WIND" + ChannelNumber.ToString() + ":ACT");
                }

                public void Window_Layout(string layout)
                {
                    SendCommand("DISP:SPL " + layout);
                }
                public void Window_Layout(int layout)
                {

                    SendCommand("DISP:SPL " + layout);
                }
                public void Channel_Max(bool state)
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
                public void Channel_Max(int ChannelNumber) //Seoul 05112018
                {
                    SendCommand("DISP:WIND" + ChannelNumber + ":SIZE MAX");
                }
                public void Channel_Min(int ChannelNumber)
                {
                    SendCommand("DISP:WIND" + ChannelNumber + ":SIZE MIN");
                }
            }
        }
        public class cFormat : LibEqmtCommon2
        {
            public cFormat(FormattedIO488 parse) : base(parse) { }
            public void Border(e_Format format)
            {
                SendCommand("FORM:BORD " + format.ToString());
            }
            public void DATA(e_FormatData DataFormat)
            {
                string dataformat;
                switch (DataFormat)
                {
                    case e_FormatData.ASC:
                        dataformat = "ASC";
                        break;
                    case e_FormatData.REAL32:
                        dataformat = "REAL,32";
                        break;
                    case e_FormatData.REAL:
                        dataformat = "REAL,64";
                        break;
                    default:
                        dataformat = "ASC";
                        break;
                }

                SendCommand("FORM:DATA " + dataformat);//DataFormat.ToString());
            }
        }
        public class cInitiate : LibEqmtCommon2
        {
            public cInitiate(FormattedIO488 parse) : base(parse) { }
            public void Immediate()
            {
                SendCommand("INIT:IMM");
            }
            public void Immediate(int ChannelNumber)
            {
                SendCommand("INIT" + ChannelNumber.ToString() + ":IMM");
            }
            public void Continuous(bool enable)
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
            public void Continuous(int ChannelNumber, bool enable)
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
            public void SourcePower(e_OnOff State) //Seoul 05112018
            {
                SendCommand(":OUTP " + State.ToString());
            }
        }
        public class cMemory : LibEqmtCommon2
        {
            public cLoad Load;
            public cStore Store;
            public cMemory(FormattedIO488 parse)
                : base(parse)
            {
                Load = new cLoad(parse);
                Store = new cStore(parse);
            }
            public class cLoad : LibEqmtCommon2
            {
                public cLoad(FormattedIO488 parse) : base(parse) { }
                public void State(string StateFile)
                {
                    //M9485.System.IO.WriteString("MMEM:LOAD 'C:\\Users\\Public\\Public Documents\\Network Analyzer\\at001.csa'");
                    ///SendCommand("MMEM:LOAD:STAT \"" + StateFile.Trim() + "\"");
                    SendCommand("MMEM:LOAD \"" + StateFile.Trim() + "\"");


                }
                public void State(string StateFile, bool dk)
                {
                    //SendCommand("MMEM:LOAD:STAT \"" + StateFile.Trim() + "\"");
                    SendCommand("MMEM:LOAD \"" + StateFile.Trim() + "\"");
                }
            }
            public class cStore : LibEqmtCommon2
            {
                public cSNP SNP;
                public cStore(FormattedIO488 parse)
                    : base(parse)
                {
                    SNP = new cSNP(parse);
                }
                public class cSNP : LibEqmtCommon2
                {
                    public cSNPType Type;
                    public cSNP(FormattedIO488 parse)
                        : base(parse)
                    {
                        Type = new cSNPType(parse);
                    }
                    public void Data(string Filename)
                    {
                        SendCommand("MMEM:STOR:DATA \"" + Filename.Trim() + "\"");
                        //MMEM:STOR:DATA
                    }
                    public void Data(int Channel, int portnum, string ports, string Filename)
                    {
                        string fileExt = ".s" + portnum.ToString() + "p";
                        Filename = String.Format("{0}{1}", Filename, fileExt);
                        SendCommand("CALC" + Channel.ToString() + ":MEAS:DATA:SNP:PORTs:Save '" + ports + "', '" + Filename.Trim() + "'"); //ZVT KS Not available for ZNB, use store trace channel instead   
                        //SendCommand("MMEMory:STORe:TRACe:PORTs " + Channel.ToString() + ", '" + Filename.Trim() + "', COMPlex, CIMPedance, " + ports); //ZVT KS Not available for ZNB, use store trace channel instead                        
                    }
                    public void Data(int Channel, int Trace, int portnum, string ports, string Filename)
                    {
                        string[] nport = ports.Trim('\"', '\n').Split(',');
                        string fileExt = ".s" + nport.Length.ToString() + "p";
                        //string fileExt = ".s" + portnum.ToString() + "p";
                       
                        Filename = String.Format("{0}{1}", Filename, fileExt);
                        SendCommand("CALC" + Channel.ToString() + ":MEAS" + Trace.ToString() + ":DATA:SNP:PORTs:Save '" + ports + "', '" + Filename.Trim() + "'"); //ZVT KS Not available for ZNB, use store trace channel instead   
                        //SendCommand("MMEMory:STORe:TRACe:PORTs " + Channel.ToString() + ", '" + Filename.Trim() + "', COMPlex, CIMPedance, " + ports); //ZVT KS Not available for ZNB, use store trace channel instead                        
                    }
                    public void Format(e_SNPFormat format)
                    {
                        SendCommand("MMEM:STOR:SNP:FORM " + format.ToString());
                    }
                    public class cSNPType : LibEqmtCommon2
                    {
                        public cSNPType(FormattedIO488 parse) : base(parse) { }
                        public void S1P(int Port1)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString());

                        }
                        public void S2P(int Port1, int Port2)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void S3P(int Port1, int Port2, int Port3)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void S4P(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                }
                public void SType(e_SType sType)
                {
                    SendCommand("MMEM:STOR:STYP " + sType.ToString());
                }
                public void State(string Filename)
                {
                    //SendCommand("MMEM:STOR:STAT \"" + Filename.Trim() + "\""); 
                    SendCommand("MMEM:STOR \"" + Filename.Trim() + "\"");
                }
                public void Transfer(string Filename, string Block)
                {
                    SendCommand("MMEM:STOR:TRAN \"" + Filename.Trim() + "\"," + Block.Trim());
                }
                public string Transfer(string Filename)
                {
                    return (ReadCommand("MMEM:STOR:TRAN? \"" + Filename.Trim() + "\""));
                }
                public void InitMemory()
                {
                    SendCommand("SYST:DATA:MEM:INIT\n");
                }
                public int ReturnOffset()
                {
                    return (int.Parse(ReadCommand("SYST:DATA:MEM:OFFSet?")));
                }
                public void AddParameterSDATA(int ChanNum, int TracNum, int NOP)
                {
                    SendCommand("SYST:DATA:MEM:ADD '" + (ChanNum).ToString() + ":" + (TracNum).ToString() + ":SDATA:" + NOP + "'");
                }
                public void AddParameterFDATA(int ChanNum, int TracNum, int NOP)
                {
                    SendCommand("SYST:DATA:MEM:ADD '" + (ChanNum).ToString() + ":" + (TracNum).ToString() + ":FDATA:" + NOP + "'");
                }
                public void AllocateMemory(string VNA_MemoryMap)
                {
                    SendCommand("SYST:DATA:MEM:COMM '" + VNA_MemoryMap + "'");
                }
                public void AllocateMemory(string VNA_MemoryMap, int ChanNum)
                {
                    SendCommand("SYST:DATA:MEM:COMM '" + VNA_MemoryMap + ChanNum.ToString() + "'");
                }
                public int SizeOfMemory()
                {
                    return (int.Parse(ReadCommand("SYST:DATA:MEM:SIZE?")));
                }
                public string MemoryCatalog()
                {
                    return (ReadCommand("SYST:DATA:MEM:CATalog?"));
                }
            }
        }
        public class cSense : LibEqmtCommon2
        {
            public cCorrection Correction;
            public cFrequency Frequency;
            public cSegment Segment;
            public cSweep Sweep;
            public cNoise Noise; //Seoul 05112018
            public cAverage Average;

            public cSense(FormattedIO488 parse)
                : base(parse)
            {
                Correction = new cCorrection(parse);
                Frequency = new cFrequency(parse);
                Segment = new cSegment(parse);
                Sweep = new cSweep(parse);
                Noise = new cNoise(parse); //Seoul 05112018
                Average = new cAverage(parse);
            }
            public class cCorrection : LibEqmtCommon2
            {
                public cCollect Collect;
                public cCorrection(FormattedIO488 parse)
                    : base(parse)
                {
                    Collect = new cCollect(parse);
                }
                public class cCollect : LibEqmtCommon2
                {
                    public cAcquire Acquire;
                    public cMethod Method;
                    public cECAL ECAL;
                    public cCalkit Cal_Kit;
                    public cGuidedCal GuidedCal;
                    public cUnGuidedCal UnGuidedCal;
                    public cCset Cset; //Seoul 05112018

                    public cCollect(FormattedIO488 parse)
                        : base(parse)
                    {
                        Acquire = new cAcquire(parse);
                        Method = new cMethod(parse);
                        Cal_Kit = new cCalkit(parse);
                        ECAL = new cECAL(parse);
                        GuidedCal = new cGuidedCal(parse);
                        UnGuidedCal = new cUnGuidedCal(parse);
                        Cset = new cCset(parse); //Seoul 05112018
                    }
                    public class cAcquire : LibEqmtCommon2
                    {
                        public cAcquire(FormattedIO488 parse) : base(parse) { }
                        public void Isolation(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:ISOL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void Isolation(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:ISOL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void Load(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:LOAD " + Port.ToString());
                        }
                        public void Load(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:LOAD " + Port.ToString());
                        }
                        public void Open(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:OPEN " + Port.ToString());
                        }
                        public void Open(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:OPEN " + Port.ToString());
                        }
                        public void Short(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SHOR " + Port.ToString());
                        }
                        public void Short(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SHOR " + Port.ToString());
                        }
                        public void Subclass(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SUBC " + Port.ToString());
                        }
                        public void Subclass(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SUBC " + Port.ToString());
                        }
                        public void Thru(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:THRU " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void Thru(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:THRU " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void TRLLine(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:TRLL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void TRLLine(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:TRLL " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void TRLReflect(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:TRLR " + Port.ToString());
                        }
                        public void TRLReflect(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:TRLR " + Port.ToString());
                        }
                        public void TRLThru(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:TRLT " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void TRLThru(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:TRLT " + Port1.ToString() + "," + Port2.ToString());
                        }
                    }
                    public class cMethod : LibEqmtCommon2
                    {
                        public cMethod(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        public void SOLT1(int Port1)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT1 " + Port1.ToString());
                        }
                        public void SOLT1(int ChannelNumber, int Port1)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT1 " + Port1.ToString());
                        }
                        public void SOLT2(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void SOLT2(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void SOLT3(int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void SOLT3(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void SOLT4(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS:CORR:COLL:METH:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public void SOLT4(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                    //Modified by COO
                    public class cECAL : LibEqmtCommon2
                    {
                        public cECAL(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        public void SOLT1(int Port1)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT1 " + Port1.ToString());
                        }
                        public void SOLT1(int ChannelNumber, int Port1)
                        {
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT1 " + Port1.ToString());
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:ECAL:ACQ SOLT , " + Port1.ToString()); //seoul
                        }
                        public void SOLT2(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void SOLT2(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public void SOLT3(int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void SOLT3(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void SOLT4(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS:CORR:COLL:ECAL:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public void SOLT4(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public string ModuleList() //Seoul 05112018
                        {
                            return (ReadCommand(":SENS:CORR:CKIT:ECAL:LIST?"));
                        }
                        public string ModuleInfo(string module)
                        {
                            return (ReadCommand("SENS:CORR:CKIT:ECAL" + module + ":INF?"));
                        }
                        public void PathState_2Port(int EcalModuleNum)
                        {
                            SendCommand(":CONT:ECAL:MOD" + EcalModuleNum.ToString() + ":PATH:STAT AB,1");
                        }
                        public void SelectEcal(int ChannelNumber, string CalKit)
                        {
                            SendCommand(":SENS" + ChannelNumber + ":CORR:COLL:GUID:ECAL:SEL \"" + CalKit + "\"");
                        }
                        public void saveEcalSnp(string Calkit, string direction, string FileName)
                        {
                            SendCommand(":SYST:COMM:ECAL:EXP:SNP \"" + Calkit + "\", \"" + direction + "\", \"" + FileName + "\"");
                        }
                    }
                    public class cCalkit : LibEqmtCommon2
                    {
                        public cCalkit(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        public void Cal_Kit(int Number_CalKit)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT " + Number_CalKit.ToString());
                        }

                        public void Cal_Kit(int ChannelNumber, int Number_CalKit)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT " + Number_CalKit.ToString());
                        }
                        public int Cal_Kit()
                        {
                            return (int.Parse(ReadCommand(":SENS1:CORR:COLL:CKIT?")));
                        }
                        //public int Cal_Kit(int ChannelNumber)
                        //{
                        //    return(int.Parse(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT?")));
                        //}
                        public void Label(string name)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:LAB \"" + name + "\"");
                        }
                        public void Label(int ChannelNumber, string name)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:LAB \"" + name + "\"");
                        }
                        public string Label()
                        {
                            return (ReadCommand(":SENS1:CORR:COLL:CKIT:LAB?"));
                        }
                        public string Label(int ChannelNumber)
                        {
                            return (ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:LAB?"));
                        }
                        public void Order(int SubClass)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT " + SubClass.ToString());
                        }
                        public void Order(int ChannelNumber, int SubClass)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT " + SubClass.ToString());
                        }
                        public int Order()
                        {
                            return (int.Parse(ReadCommand(":SENS1:CORR:COLL:CKIT?")));
                        }
                        //public int Order(int ChannelNumber)
                        //{
                        //    return(int.Parse(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT?")));
                        //}
                        public void Open(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:OPEN " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Open(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:OPEN " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Short(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:SHOR " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Short(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:SHOR " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Load(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:LOAD " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Load(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:LOAD " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Thru(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:THRU " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public void Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:THRU " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public void TRL_Line(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLL " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public void TRL_Line(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:TRLL " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public void TRL_Reflect(int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLR " + Standard_Number.ToString());
                        }
                        public void TRL_Reflect(int ChannelNumber, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:TRLR " + Standard_Number.ToString());
                        }
                        public void TRL_Thru(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLT " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public void TRL_Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:TRLT " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                    }
                    public class cGuidedCal : LibEqmtCommon2
                    {
                        public cGuidedCal(FormattedIO488 parse)
                            : base(parse)
                        {

                        }

                        public void Delete_AllCalData()
                        {
                            SendCommand("CSET:DALL");
                        }
                        //instrument.WriteString(":SENS" + NFchArray[i] + ":CORR:COLL:GUID:THRU:PORT " + NFsrcPortNum + "," + NFrcvPortNum);
                        //Specify the calibration Thru method to Defined Thru for each port pair           
                        //instrument.WriteString(":SENS" + NFchArray[i] + ":CORR:COLL:GUID:PATH:TMET " + NFsrcPortNum + "," + NFrcvPortNum + ", \"Undefined Thru\"");
                        public void ChannelMode(bool mode) //Seoul 05112018
                        {
                            if (mode) SendCommand("SENS:CORR:COLL:GUID:CHAN:MODE 1");
                            else SendCommand("SENS:CORR:COLL:GUID:CHAN:MODE 0");
                        }

                        public void DefineConnectorType(int ChannelNumber, int Port_Number, string connector)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:CONN:PORT" + Port_Number.ToString() + ":SEL " + " \"" + connector + "\"");

                        }
                        public void setCalKit(int ChannelNumber, int Port_Number, string KitName)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:CKIT:PORT" + Port_Number.ToString() + ":SEL " + " \"" + KitName + "\"");
                        }
                        public void DefineThru(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port1.ToString() + "," + Port2.ToString() + "," + Port1.ToString() + "," + Port3.ToString() + "," + Port2.ToString() + "," + Port3.ToString());

                            //   SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void DefineThru(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port1.ToString() + "," + Port2.ToString() + "," + Port1.ToString() + "," + Port3.ToString() + "," + Port1.ToString() + "," + Port4.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port2.ToString() + "," + Port4.ToString() + "," + Port3.ToString() + "," + Port4.ToString());

                            //   SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void DefineThru(int ChannelNumber, int Port1, int Port2, int Port3, int Port4, int Port5)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port1.ToString() + "," + Port2.ToString() + "," + Port1.ToString() + "," + Port3.ToString() + "," + Port1.ToString() + "," + Port4.ToString() + "," + Port1.ToString() + "," + Port5.ToString()
                                + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port2.ToString() + "," + Port4.ToString() + "," + Port2.ToString() + "," + Port5.ToString() + ","
                                + Port3.ToString() + "," + Port4.ToString() + "," + Port3.ToString() + "," + Port5.ToString() + "," + Port4.ToString() + "," + Port5.ToString());

                            //   SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port2.ToString() + "," + Port3.ToString());
                        }
                        public void DefineThru(int ChannelNumber, int Port1, int Port2, int Port3, int Port4, int Port5, int Port6)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port1.ToString() + "," + Port2.ToString() + "," + Port1.ToString() + "," + Port3.ToString() + "," + Port1.ToString() + "," + Port4.ToString() + "," + Port1.ToString() + "," + Port5.ToString() + "," + Port1.ToString() + "," + Port6.ToString()
                                + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port2.ToString() + "," + Port4.ToString() + "," + Port2.ToString() + "," + Port5.ToString() + "," + Port2.ToString() + "," + Port6.ToString() + ","
                                + Port3.ToString() + "," + Port4.ToString() + "," + Port3.ToString() + "," + Port5.ToString() + "," + Port3.ToString() + "," + Port6.ToString() + ","
                                + Port4.ToString() + "," + Port5.ToString() + "," + Port4.ToString() + "," + Port6.ToString() + "," + Port5.ToString() + "," + Port6.ToString());

                            //   SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + Port2.ToString() + "," + Port3.ToString());
                        }

                        public void InitGuidedCal(int ChannelNumber)
                        {
                            try
                            {
                                SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:INIT");
                            }
                            catch (Exception e)
                            {
                                string meup = "Break here because there is an error." + "\r\n" + e.ToString();
                                throw new Exception("InitGuidedCal has an error." + "\r\n" + e.ToString());
                            }
                        }
                        public void AcquireStandNum(int ChannelNumber, int Standard_Number)
                        {
                            // SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:ACQ STAN12");

                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:ACQ STAN" + Standard_Number.ToString());
                        }
                        public string CalibrationStep(int ChannelNumber)
                        {
                            try
                            {
                                return (ReadCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:STEP?"));
                            }
                            catch(Exception e)
                            {
                                return "There is an error in the STEP " + "\r\n" + e.ToString();
                            }
                            
                        }

                        public string CalibrationStepDesc(int ChannelNumber, int stepnum)
                        {
                            try
                            {
                                return (ReadCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:DESC? " + stepnum.ToString()));
                            }
                            catch (Exception e)
                            {
                                return "There is an error in the STEP DESCRIPTION " + "\r\n" + e.ToString();
                            }

                        }

                        //---------- For Topaz NF test ----------------- 20160615
                        public void Define_ENR_type(int ChannelNumber, string ENR_Path) // * CS method : Select ENR file from ENR_Path
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:ENR:FILENAME \"" + ENR_Path + "\"");
                        }
                        public void Connect_NStoPort(int ChannelNumber, string port_type) // * CS method
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:SOUR:CONN \"" + port_type.ToString() + "\"");
                        }

                        public void Select_Cal_Method(int ChannelNumber) // * CS method
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:CAL:METHOD \'Scalar\'");
                            SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:CAL:RMEThod \'NoiseSource\'");
                        }
                        public string Select_Trace(int ChannelNumber) // * CS method
                        {
                            string active_trace = ReadCommand("SYST:MEAS:CAT? " + ChannelNumber);
                            return active_trace;
                        }

                        public void unDefineThru_set(int ChannelNumber, int NFsrcPortNum, int NFrcvPortNum)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:THRU:PORT " + NFsrcPortNum.ToString() + "," + NFrcvPortNum.ToString());
                        }
                        public void unDefineThru(int ChannelNumber, int NFsrcPortNum, int NFrcvPortNum)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:PATH:TMET " + NFsrcPortNum.ToString() + "," + NFrcvPortNum.ToString() + ", \"Undefined Thru\"");
                        }
                        public void input_connector_type(int ChannelNumber, int NFsrcPortNum, string DUTinPortConnType)
                        {
                            SendCommand("SENS" + ChannelNumber + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUTinPortConnType + "'");
                        }
                        public void output_connector_type(int ChannelNumber, int NFsrcPortNum, string DUToutPortConnType)
                        {
                            SendCommand("SENS" + ChannelNumber + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUToutPortConnType + "'");
                        }
                        public void Src_calkit(int ChannelNumber, int NFsrcPortNum, string CalKitName)
                        {
                            SendCommand("SENS" + ChannelNumber + ":CORR:COLL:GUID:CKIT:PORT" + NFsrcPortNum + " '" + CalKitName + "'");
                        }
                        public void Rsv_calkit(int ChannelNumber, int NFrcvPortNum, string CalKitName)
                        {
                            SendCommand("SENS" + ChannelNumber + ":CORR:COLL:GUID:CKIT:PORT" + NFrcvPortNum + " '" + CalKitName + "'");
                        }
                        public void setNS_CalKit(int ChannelNumber, string KitName)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:SOUR:CKIT \"" + KitName + "\"");
                        }
                        //instrument.WriteString("SENS" + NFchArray[i] + ":CORR:COLL:GUID:CONN:PORT" + NFsrcPortNum + " '" + DUTinPortConn + "'"); // Define DUT input port connector type
                        //instrument.WriteString("SENS" + NFchArray[i] + ":CORR:COLL:GUID:CONN:PORT" + NFrcvPortNum + " '" + DUToutPortConn + "'"); // Define DUT output port connector type
                        //instrument.WriteString("SENS" + NFchArray[i] + ":CORR:COLL:GUID:CKIT:PORT" + NFsrcPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT input port
                        //instrument.WriteString("SENS" + NFchArray[i] + ":CORR:COLL:GUID:CKIT:PORT" + NFrcvPortNum + " '" + CalKitName + "'"); // Define Cal kit for DUT output port
                        //instrument.WriteString("SENS" + NFchArray[i] + ":NOIS:SOUR:CONN '" + DUToutPortConn + "'"); // Define Noise Source connector type
                        //---------- For Topaz NF test ----------------- 20160615

                    }
                    public class cUnGuidedCal : LibEqmtCommon2
                    {
                        public cUnGuidedCal(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        //public void DefineConnectorType(int ChannelNumber, int Port_Number, string connector)
                        //{
                        //    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:CONN:PORT" + Port_Number.ToString() + ":SEL " + " \"" + connector + "\"");
                        //}
                        public void setCalKit(int ChannelNumber, int Port_Number, string KitName)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:PORT:SEL " + " \"" + KitName + "\"");
                        }
                        //public void DefineThru(int ChannelNumber, int Port_Number)
                        //{
                        //    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:THRU:PORT" + Port_Number.ToString());
                        //}
                        //public void InitGuidedCal(int ChannelNumber)
                        //{
                        //    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:INIT");
                        //}
                        public void AcquireStandNum(int ChannelNumber, int Standard_Number)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ STAN" + Standard_Number.ToString());
                        }
                        public void ResponseCal(int ChannelNumber)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH RESP");
                        }
                        public void ResponseTHRU(int ChannelNumber)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH TRAN1");
                        }
                        public void ResponseTHRU_ISO(int ChannelNumber)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH TRAN2");
                        }
                        //public string CalibrationStep(int ChannelNumber)
                        //{
                        //    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:STEP?"));
                        //}
                    }
                    public class cCset : LibEqmtCommon2 //Seoul 05112018
                    {
                        public cCset(FormattedIO488 parse)
                           : base(parse)
                        {

                        }
                        public void CharacterizedFixture(string CalSetA, string CalSetB, int port, string snpFile, e_SweepType Format)
                        {
                            SendCommand(":CSET:FIXT:CHAR \"" + CalSetA + "\"" + ",\"" + CalSetB + "\"" + "," + port + "," + "\"" + snpFile + "\"" + "," + Format);
                        }
                        public void Combine2Snp(string AdaperSnp, string EcalSnp, string FixtureSnp, e_SweepType Format)
                        {
                            SendCommand(":CSET:FIXT:CASC \"" + AdaperSnp + "\"" + ",\"" + EcalSnp + "\"" + ",\"" + FixtureSnp + "\"" + "," + Format);
                        }
                        public void Copy(int ChannelNumber, string CalSetName)
                        {
                            SendCommand(":SENS" + ChannelNumber + ":CORR:CSET:COPY \"" + CalSetName + "\"");
                        }
                        public void Delete(string CalSetName)
                        {
                            SendCommand(":CSET:DEL \"" + CalSetName + "\"");
                        }
                        public string Exist(string CalSetName)
                        {
                            return (ReadCommand(":CSET:EXIS? \"" + CalSetName + "\""));
                        }
                        public void newENR(string ENRfile, string FixtureSnp, string newENR)
                        {
                            SendCommand(":CSET:FIXT:ENR:EMB \"" + ENRfile + "\"" + ",\"" + FixtureSnp + "\"" + ",\"" + newENR + "\"");
                        }
                        public void Save(int ChannelNumber, string CalSetName)
                        {
                            SendCommand("SENS" + ChannelNumber + ":CORR:COLL:GUID:SAVE:CSET \"" + CalSetName + "\"");
                        }

                    }
                    public void Save()
                    {
                        SendCommand("SENS:CORR:COLL:SAVE");
                    }
                    public void Save(int ChannelNumber)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:SAVE");
                    }
                    public void Save_SmartCal(int ChannelNumber)
                    {

                        //   SendCommand("SENS3:CORR:COLL:GUID:SAVE:IMM");

                        //  SendCommand("SENS12:CORR:COLL:GUID:SAVE:IMM");


                        SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:SAVE:IMM");
                        //   SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:SAVE");
                    }
                    //public void Save(int ChannelNumber)
                    //{
                    //    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:GUID:SAVE:IMM");
                    //}
                }
                public void Property(bool enable)
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
                public void Property(int ChannelNumber, bool enable)
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
                public void Clear()
                {
                    SendCommand("SENS:CORR:CLE");
                }
                public void Clear(int ChannelNumber)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:CLE");
                }
                public void State(bool enable)
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
            public class cFrequency : LibEqmtCommon2
            {
                public cFrequency(FormattedIO488 parse) : base(parse) { }
                public void Center(double Freq)
                {
                    SendCommand("SENS:FREQ:CENT " + Freq.ToString());
                }
                public void CW(double Freq)
                {
                    SendCommand("SENS:FREQ:CW " + Freq.ToString());
                }
                public void Fixed(double Freq)
                {
                    SendCommand("SENS:FREQ:FIX " + Freq.ToString());
                }
                public void SPAN(double BW)
                {
                    SendCommand("SENS:FREQ:SPAN " + BW.ToString());
                }
                public void Start(double Freq)
                {
                    SendCommand("SENS:FREQ:STAR " + Freq.ToString());
                }
                public void Start(int ChannelNumber, double Freq)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STAR " + Freq.ToString());
                }
                public double Start()
                {
                    return (Convert.ToDouble(ReadCommand("SENS:FREQ:STAR?")));
                }
                public double Start(int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STAR?")));
                }
                public void Stop(double Freq)
                {
                    SendCommand("SENS:FREQ:STOP " + Freq.ToString());
                }
                public void Stop(int ChannelNumber, double Freq)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STOP " + Freq.ToString());
                }
                public double Stop()
                {
                    return (Convert.ToDouble(ReadCommand("SENS:FREQ:STOP?")));
                }
                public double Stop(int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STOP?")));
                }
                public double[] FreqList()
                {
                    return (ReadIEEEBlock("SENS:FREQ:DATA?"));
                }
                public double[] FreqList(int ChannelNumber)
                {
                    // SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:MNUM 1");
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":X?"));
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":MEAS1:FUNC:DATA?"));

                    //"CLAC" + ChannelNumber.ToString() + ":PAR:MNUM 1"

                    //return (ReadIEEEBlock("SENS" + ChannelNumber.ToString() + ":FREQ:DATA?"));
                }
                public double[] FreqList(int ChannelNumber, int TraceNumber)
                {
                    // SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:MNUM " + TraceNumber.ToString());
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":X?"));

                    //"CLAC" + ChannelNumber.ToString() + ":PAR:MNUM 1"

                    //return (ReadIEEEBlock("SENS" + ChannelNumber.ToString() + ":FREQ:DATA?"));
                }
                public string FreqList(int ChannelNumber, string TraceNumber) //Seoul 05112018
                {
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":MEAS" + TraceNumber.ToString() + ":X?"));
                }
            }
            public class cSegment : LibEqmtCommon2
            {
                public new e_ModeSetting Mode;
                public new e_OnOff Ifbw;
                public new e_OnOff Pow;
                public new e_OnOff Del;
                public new e_OnOff Time;

                public cSegment(FormattedIO488 parse) : base(parse) { }
                public void Data(string SegmentData, e_OnOff sweepmode)
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
                public void Data(int ChannelNumber, string SegmentData, e_OnOff sweepmode)
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
                public void Data(s_SegmentTable SegmentData, e_OnOff sweepmode)
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
                public void Data(int ChannelNumber, s_SegmentTable SegmentData, e_OnOff sweepmode)
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

                public s_SegmentTable Data(int ChannelNumber)
                {
                    int count;
                    string DataFormat;
                    DataFormat = ReadCommand("FORM:DATA?");
                    SendCommand("FORM:DATA ASC");
                    count = int.Parse(ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:COUN?"));
                    s_SegmentTable ST = new s_SegmentTable();

                    ST.mode = Mode;

                    ST.ifbw = (e_OnOff)Enum.Parse(typeof(e_OnOff), ReadCommand(String.Format("SENS{0}:SEGM:BWID:CONT?", ChannelNumber.ToString())));
                    ST.pow = (e_OnOff)Enum.Parse(typeof(e_OnOff), ReadCommand(String.Format("SENS{0}:SEGM:POW:CONT?", ChannelNumber.ToString())));
                    ST.del = (e_OnOff)Enum.Parse(typeof(e_OnOff), ReadCommand(String.Format("SENS{0}:SEGM:SWE:DWEL:CONT?", ChannelNumber.ToString())));
                    ST.time = (e_OnOff)Enum.Parse(typeof(e_OnOff), ReadCommand(String.Format("SENS{0}:SEGM:SWE:TIME:CONT?", ChannelNumber.ToString())));

                    ST.segm = count;
                    ST.SegmentData = new s_SegmentData[ST.segm];
                    for (int i = 0; i < count; i++)
                    {
                        ST.SegmentData[i].Start = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:FREQ:STAR?", ChannelNumber.ToString(), (i + 1).ToString())));
                        ST.SegmentData[i].Stop = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:FREQ:STOP?", ChannelNumber.ToString(), (i + 1).ToString())));
                        ST.SegmentData[i].Points = int.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:POIN?", ChannelNumber.ToString(), (i + 1).ToString())));
                        ST.SegmentData[i].ifbw_value = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:BWID?", ChannelNumber.ToString(), (i + 1).ToString())));
                        ST.SegmentData[i].pow_value = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:POW?", ChannelNumber.ToString(), (i + 1).ToString())));
                        // ST.SegmentData[i].time_value = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:TIME?", ChannelNumber.ToString(), (i + 1).ToString())));
                        ST.SegmentData[i].del_value = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:DWEL?", ChannelNumber.ToString(), (i + 1).ToString())));
                    }

                    SendCommand("FORM:DATA " + DataFormat);

                    return (ST);

#if false
                    string DataFormat;
                    string tmpStr;
                    string[] tmpSegData;
                    long tmpI;

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
                            tmpI = 7;
                            for (int iSeg = 0; iSeg < ST.segm; iSeg++)
                            {
                                ST.SegmentData[iSeg].Start = double.Parse(tmpSegData[tmpI]);
                                tmpI++;
                                ST.SegmentData[iSeg].Stop = double.Parse(tmpSegData[tmpI]);
                                tmpI++;
                                ST.SegmentData[iSeg].Points = int.Parse(tmpSegData[tmpI]);
                                tmpI++;
                                
                                if (ST.ifbw == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].ifbw_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.pow == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].pow_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.del == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].del_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.time == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].time_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
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
                            tmpI = 8;
                            for (int iSeg = 0; iSeg < ST.segm; iSeg++)
                            {
                                ST.SegmentData[iSeg].Start = double.Parse(tmpSegData[tmpI]);
                                tmpI++;
                                ST.SegmentData[iSeg].Stop = double.Parse(tmpSegData[tmpI]);
                                tmpI++;
                                ST.SegmentData[iSeg].Points = int.Parse(tmpSegData[tmpI]);
                                tmpI++;
                                //tmpI = 10;
                                if (ST.ifbw == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].ifbw_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.pow == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].pow_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.del == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].del_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.swp == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].swp_value = (e_SweepMode)Enum.Parse(typeof(e_SweepMode), tmpSegData[tmpI]);
                                    tmpI++;
                                }
                                if (ST.time == e_OnOff.On)
                                {
                                    ST.SegmentData[iSeg].time_value = double.Parse(tmpSegData[tmpI]);
                                    tmpI++;
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

                    return(ST);
#endif
                }
                public string SweepPoints()
                {
                    return (ReadCommand("SENS:SEGM:SWE:POIN?"));
                }
                public string SweepPoints(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:SWE:POIN?"));
                }
                public string SweepTime()
                {
                    return (ReadCommand("SENS:SEGM:SWE:TIME:DATA?"));
                }
                public string SweepTime(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:SWE:TIME:DATA?"));
                }

                public void Add_SegmentTable2String(int ChannelNumber, s_SegmentTable SegmentTable, bool[] enabledPortList)
                {

                    //SendCommand("SENS" + ChannelNumber.ToString() + ":COUP:PAR 1");
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":COUP:PAR ON");
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":COUP:PAR:STATE OFF");
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":COUP:PAR:STATE ON");

                    //SendCommand("SENS:COUP:PAR 1");
                    //SendCommand("SENS:COUP:PAR 0");

                    string tmpStr;
                    tmpStr = "";
                    int segment = 0;
                    //switch (SweepMode)
                    //{
                    //case e_OnOff.On:
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DEL:ALL");
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
                            tmpStr += "," + SegmentTable.SegmentData[Seg].ifbw_value;
                        if (SegmentTable.pow == e_OnOff.On)
                        {
                            if (enabledPortList[0]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n1_value.ToString();
                            if (enabledPortList[1]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n2_value.ToString();
                            if (enabledPortList[2]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n3_value.ToString();
                            if (enabledPortList[3]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n4_value.ToString();
                            if (enabledPortList[4]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n5_value.ToString();
                            if (enabledPortList[5]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n6_value.ToString();
                            if (enabledPortList[6]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n7_value.ToString();
                            if (enabledPortList[7]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n8_value.ToString();
                            if (enabledPortList[8]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n9_value.ToString();
                            if (enabledPortList[9]) tmpStr += "," + SegmentTable.SegmentData[Seg].pow_n10_value.ToString();


                        }
                        if (SegmentTable.del == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].del_value.ToString();
                        if (SegmentTable.swp == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].swp_value.ToString();
                        if (SegmentTable.time == e_OnOff.On)
                            tmpStr += "," + SegmentTable.SegmentData[Seg].time_value.ToString();
                        segment = Seg + 1;
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":ADD");
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":STAT ON");
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":FREQ:STAR " + SegmentTable.SegmentData[Seg].Start.ToString());
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":FREQ:STOP " + SegmentTable.SegmentData[Seg].Stop.ToString());
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":SWE:POIN " + SegmentTable.SegmentData[Seg].Points.ToString());
                        if (SegmentTable.ifbw == e_OnOff.On) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":BWID:CONT ON");
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":BWID " + SegmentTable.SegmentData[Seg].ifbw_value.ToString());
                        if (SegmentTable.pow == e_OnOff.On)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW:CONT ON");
                            if (enabledPortList[0]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW1 " + SegmentTable.SegmentData[Seg].pow_n1_value.ToString());
                            if (enabledPortList[1]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW2 " + SegmentTable.SegmentData[Seg].pow_n2_value.ToString());
                            if (enabledPortList[2]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW3 " + SegmentTable.SegmentData[Seg].pow_n3_value.ToString());
                            if (enabledPortList[3]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW4 " + SegmentTable.SegmentData[Seg].pow_n4_value.ToString());
                            if (enabledPortList[4]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW5 " + SegmentTable.SegmentData[Seg].pow_n5_value.ToString());
                            if (enabledPortList[5]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW6 " + SegmentTable.SegmentData[Seg].pow_n6_value.ToString());
                            if (enabledPortList[6]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW7 " + SegmentTable.SegmentData[Seg].pow_n7_value.ToString());
                            if (enabledPortList[7]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW8 " + SegmentTable.SegmentData[Seg].pow_n8_value.ToString());
                            if (enabledPortList[8]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW9 " + SegmentTable.SegmentData[Seg].pow_n9_value.ToString());
                            if (enabledPortList[9]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW10 " + SegmentTable.SegmentData[Seg].pow_n10_value.ToString());

                        }

                    }
                    //    break;
                    //case e_OnOff.Off:
                    //    tmpStr = ((int)SegmentTable.mode).ToString();
                    //    tmpStr += "," + ((int)SegmentTable.ifbw).ToString();
                    //    tmpStr += "," + ((int)SegmentTable.pow).ToString();
                    //    tmpStr += "," + ((int)SegmentTable.del).ToString();
                    //    tmpStr += "," + ((int)SegmentTable.time).ToString();
                    //    tmpStr += "," + SegmentTable.segm.ToString();
                    //    for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                    //    {
                    //        tmpStr += "," + SegmentTable.SegmentData[Seg].Start.ToString();
                    //        tmpStr += "," + SegmentTable.SegmentData[Seg].Stop.ToString();
                    //        tmpStr += "," + SegmentTable.SegmentData[Seg].Points.ToString();
                    //        if (SegmentTable.ifbw == e_OnOff.On)
                    //            tmpStr += "," + SegmentTable.SegmentData[Seg].ifbw_value.ToString();
                    //        if (SegmentTable.pow == e_OnOff.On)
                    //            tmpStr += "," + SegmentTable.SegmentData[Seg].pow_value.ToString();
                    //        if (SegmentTable.del == e_OnOff.On)
                    //            tmpStr += "," + SegmentTable.SegmentData[Seg].del_value.ToString();
                    //        if (SegmentTable.time == e_OnOff.On)
                    //            tmpStr += "," + SegmentTable.SegmentData[Seg].time_value.ToString();
                    //    }
                    //    break;                       
                    // }
                    //return (tmpStr);
                }

                public void ChangeSegmentPower(int ChannelNumber,int Srcport, int PortPower, s_SegmentTable SegmentTable)
                {               
                    string tmpStr;
                    tmpStr = "";
                    int segment = 0;

                    for (int Seg = 1; Seg < SegmentTable.SegmentData.Length+1; Seg++)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + Srcport + " " + PortPower);
                    }
                }

                public void ChangeSegmPower_Allport(int ChannelNumber, s_SegmentTable SegmentTable, bool[] enabledPortList)
                {
                    string tmpStr;
                    tmpStr = "";
                    int segment = 0;
                    for (int Seg = 0; Seg < SegmentTable.SegmentData.Length; Seg++)
                    {
                        if (SegmentTable.pow == e_OnOff.On)
                        {
                            segment = Seg + 1;
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW:CONT ON");
                            if (enabledPortList[0]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW1 " + SegmentTable.SegmentData[Seg].pow_n1_value.ToString());
                            if (enabledPortList[1]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW2 " + SegmentTable.SegmentData[Seg].pow_n2_value.ToString());
                            if (enabledPortList[2]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW3 " + SegmentTable.SegmentData[Seg].pow_n3_value.ToString());
                            if (enabledPortList[3]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW4 " + SegmentTable.SegmentData[Seg].pow_n4_value.ToString());
                            if (enabledPortList[4]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW5 " + SegmentTable.SegmentData[Seg].pow_n5_value.ToString());
                            if (enabledPortList[5]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW6 " + SegmentTable.SegmentData[Seg].pow_n6_value.ToString());
                            if (enabledPortList[6]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW7 " + SegmentTable.SegmentData[Seg].pow_n7_value.ToString());
                            if (enabledPortList[7]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW8 " + SegmentTable.SegmentData[Seg].pow_n8_value.ToString());
                            if (enabledPortList[8]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW9 " + SegmentTable.SegmentData[Seg].pow_n9_value.ToString());
                            if (enabledPortList[9]) SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + segment + ":POW10 " + SegmentTable.SegmentData[Seg].pow_n10_value.ToString());
                        }
                    }
                }

                public void ChangeSegmentPowerVerifyOnly(int ChannelNumber, int PortPower, s_SegmentTable SegmentTable)
                {
                    for (int Seg = 1; Seg < SegmentTable.SegmentData.Length + 1; Seg++)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + 1 + " " + PortPower);
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + 2 + " " + PortPower);
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + 3 + " " + PortPower);
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + 4 + " " + PortPower);
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + 5 + " " + PortPower);
                        SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM" + Seg + ":POW" + 6 + " " + PortPower);
                    }
                }
            }
            public class cSweep : LibEqmtCommon2
            {
                public cSweepTime Time;
                public cSweep(FormattedIO488 parse)
                    : base(parse)
                {
                    Time = new cSweepTime(parse);
                }
                public void ASPurious(e_OnOff State)
                {
                    SendCommand("SENS:SWE:ASP " + State.ToString());
                }
                public void Delay(double delay)
                {
                    SendCommand("SENS:SWE:DEL " + delay.ToString());
                }
                public void DwellTime(double delay, int ChannelNumber)
                {
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:GEN STEP");

                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:DWEL " + delay.ToString());
                }
                public void DwellTime_Auto(bool Auto, int ChannelNumber)
                {
                    switch (Auto)
                    {
                        case true:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:DWEL:AUTO ON");
                            break;
                        case false:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:DWEL:AUTO OFF");
                            break;
                    }

                }
                public void SweepSpeed(int ChannelNumber, e_Format Speed)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:SPE " + Speed);
                }
                public void Generation(e_SweepGeneration SweepGen)
                {
                    SendCommand("SENS:SWE:GEN " + SweepGen.ToString());
                }
                //public void Points(int points)
                //{
                //    SendCommand("SENS:SWE:POIN " + points.ToString());
                //}
                public void Points(int ChannelNumber, int points)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:POIN " + points.ToString());
                }
                public int Points()
                {
                    return (Convert.ToInt32(ReadCommand("SENS:SWE:POIN?")));
                }
                public int Points(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + ChannelNumber.ToString() + ":SWE:POIN?")));
                }
                public class cSweepTime : LibEqmtCommon2
                {
                    public cSweepTime(FormattedIO488 parse) : base(parse) { }
                    public void Auto(e_OnOff state)
                    {
                        SendCommand("SENS:SWE:TIME:AUTO " + state.ToString());
                    }
                    public void Data(double time)
                    {
                        SendCommand("SENS:SWE:TIME:DATA " + time.ToString());
                    }
                }
                public void Type(e_SweepType SweepType)
                {
                    SendCommand("SENS:SWE:TYPE " + SweepType.ToString());
                }
                public void Type(int ChannelNumber, e_SweepType SweepType)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:TYPE " + SweepType.ToString());
                }
                public e_SweepType Type()
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS:SWE:TYPE?");
                    return ((e_SweepType)Enum.Parse(typeof(e_SweepType), tmpStr.ToUpper()));
                }
                public e_SweepType Type(int ChannelNumber)
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS" + ChannelNumber + ":SWE:TYPE?");
                    return ((e_SweepType)Enum.Parse(typeof(e_SweepType), tmpStr.ToUpper()));
                }

                public void Cal(int ChannelNumber)
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS:CORR:COLL:GUID:CKIT:CAT?");
                }

            }
            public class cNoise : LibEqmtCommon2 //Seoul 05112018
            {
                public cNoise(FormattedIO488 parse)
                    : base(parse)
                {

                }
                public void DefineConnectorType(int ChannelNumber, string connector)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:SOUR:CONN \"" + connector + "\"");
                }
                public void ExDCName(string DeviceName, int ChannelNumber)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:EXDC:NAME \"" + DeviceName.Trim() + "\"");
                }
                public void LoadENRfile(int ChannelNumber, string ENR_Path)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:ENR:FILENAME \"" + ENR_Path + "\"");
                }
                public void PortMapping(int ChannelNumber, int NFsrcPortNum, int NFrcvPortNum)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:PMAP " + NFsrcPortNum.ToString() + "," + NFrcvPortNum.ToString());
                }
                public string InputPort(int ChannelNumber)
                {
                    return (ReadCommand(":SENS" + ChannelNumber.ToString() + ":NOIS:PMAP:INP?"));
                }
                public string OutputPort(int ChannelNumber)
                {
                    return (ReadCommand(":SENS" + ChannelNumber.ToString() + ":NOIS:PMAP:OUTP?"));
                }
                public void setCalKit(int ChannelNumber, string KitName)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:SOUR:CKIT \"" + KitName + "\"");
                }
                public void SelectCalMethod(int ChannelNumber, e_NoiseCalMethod method)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:CAL:METH \"" + method + "\"");//\'Scalar\'");                    
                }
                public void SelectRecMethod(int ChannelNumber, e_NoiseCalMethod method)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":NOIS:CAL:RMEThod \"" + method + "\""); //\'NoiseSource\'");
                }
                public void Temperature(int ChannelNumber, float temperature)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:TCOL:USER:VAL " + temperature.ToString());
                }
            }
            public class cAverage : LibEqmtCommon2
            {
                public cAverage(FormattedIO488 parse)
                    : base(parse)
                {

                }
                public void Count(int ChannelNumber, int count)
                {
                    SendCommand("SENS" + ChannelNumber + ":AVER:COUN " + count);
                }
                public string State(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber + ":AVER?"));
                }
                public void State(int ChannelNumber, e_OnOff state)
                {
                    SendCommand("SENS" + ChannelNumber + ":AVER " + state);
                }
            }
            public string Class(int ChannelNumber)
            {
                return (ReadCommand("SENS" + ChannelNumber.ToString() + ":CLAS:NAME?"));
            }
            public double ReadTemp(e_TempUnit unit, int ModuleNumber)
            {
                return (double.Parse(ReadCommand("SENS:TEMP? \"" + unit + "\"" + ",\"" + ModuleNumber.ToString() + "\"")));
            }
        }
    
        public class cTrigger : LibEqmtCommon2
        {
            public cTriggerExternal External;
            public cTrigger(FormattedIO488 parse)
                : base(parse)
            {
                External = new cTriggerExternal(parse);
            }
            public void Average(e_OnOff State)
            {
                SendCommand("TRIG:AVER " + State.ToString());
            }
            public class cTriggerExternal : LibEqmtCommon2
            {
                public cTriggerExternal(FormattedIO488 parse) : base(parse) { }
                public void Delay(double delay)
                {
                    SendCommand("TRIG:EXT:DEL " + delay.ToString());
                }
                public void LLatency(e_OnOff state)
                {
                    SendCommand("TRIG:EXT:LLAT " + state.ToString());
                }
            }
            public void Immediate()
            {
                SendCommand("TRIG:SEQ:IMM");
            }
            public void Point(e_OnOff state)
            {
                SendCommand("TRIG:SEQ:POIN " + state.ToString());
            }
            public void Single(int channel)
            {
                //// SendCommand("TRIG:SEQ:SING");
                ////KCC
                SendCommand("INIT" + channel.ToString() + ":IMM");
                // SendCommand("TRIG:SING");
            }
            public void Single()
            {
                //// SendCommand("TRIG:SEQ:SING");
                ////KCC

                SendCommand("INIT1");
                SendCommand("TRIG:SING");
            }
            public void Scope(e_TriggerScope Scope)
            {
                SendCommand("TRIG:SEQ:SCOP " + Scope.ToString());
            }
            public void Source(e_TriggerSource Source)
            {
                SendCommand("TRIG:SEQ:SOUR " + Source.ToString());

            }
            public void Mode(e_TriggerMode Source, int ChannelNumber)
            {
                SendCommand("SENS" + ChannelNumber.ToString() + ":SWEEP:MODE " + Source.ToString());

                //   SendCommand("SENS" + ChannelNumber.ToString() + ":SWEEP:MODE CONT");
            }
            public void Hold() //Seoul 05112018
            {
                SendCommand(":SYST:CHAN:HOLD");
            }

            public double Read(int ChannelNumber, string TraceNumber)
            {



                string Result = ReadCommand("CALC" + ChannelNumber.ToString() + ":DATA:MFD? \"" + TraceNumber + "\"");
                // string a = ReadCommand("CALC" + ChannelNumber.ToString() + ":DATA:MFD? 1");
                return 0;
            }

            public void Cal(int channel)
            {
                //// SendCommand("TRIG:SEQ:SING");
                ////KCC
                SendCommand("INIT" + channel.ToString() + ":IMM");
                // SendCommand("TRIG:SING");
            }

            //ChoonChin: For Topaz temperature readback
            public string ReadVnaTemp(int Module)
            {
                //SENS:TEMPerature? CELSius , Module
                return (ReadCommand("SENS:TEMPerature? CELSius , " + Module));
            }
        }
    public class cExternalDevice : LibEqmtCommon2 //Seoul 05112018
    {
        public cExternalDevice(FormattedIO488 parse)
            : base(parse)
        {

        }
        public void Add(string DeviceName)
        {
            SendCommand("SYST:CONF:EDEV:ADD \"" + DeviceName.Trim() + "\"");
        }
        public void Driver(string DeviceName, e_EDeviceDriver DeviceDriver)
        {
            SendCommand("SYST:CONF:EDEV:DRIV \"" + DeviceName.Trim() + "\"" + ",\"" + DeviceDriver + "\"");
        }
        public void IOConfig(string DeviceName, string Address)
        {
            SendCommand("SYST:CONF:EDEV:IOC \"" + DeviceName.Trim() + "\"" + ",\"" + Address.Trim() + "\"");
        }
        public void Load(string DeviceName, string ConfigFile)
        {
            SendCommand("SYST:CONF:EDEV:LOAD \"" + ConfigFile.Trim() + "\"" + ",\"" + DeviceName.Trim() + "\"");
        }
        public void Save(string DeviceName, string ConfigFile)
        {
            SendCommand("SYST:CONF:EDEV:SAVE \"" + ConfigFile.Trim() + "\"" + ",\"" + DeviceName.Trim() + "\"");
        }
        public void State(string DeviceName, e_OnOff State)
        {
            SendCommand("SYST:CONF:EDEV:STAT \"" + DeviceName.Trim() + "\"" + "," + State);
        }
        public void Type(string DeviceName, string DeviceType)
        {
            SendCommand("SYST:CONF:EDEV:DTYPE \"" + DeviceName.Trim() + "\"" + ",\"" + DeviceType.Trim() + "\"");
        }
    }


    #endregion
    }
}
