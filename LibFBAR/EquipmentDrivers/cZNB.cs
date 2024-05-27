using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Threading;
namespace LibFBAR
{

    public class cZNB : cENA
    {
        public static new string ClassName = "ZVT/ZVA Class";
        private string IOAddress;
        private FormattedIO488 ioENA;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();

        #region "Conversion Functions"
        public static new string Convert_SegmentTable2String(s_SegmentTable SegmentTable, e_OnOff SweepMode)
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
        public override string Address
        {
            get
            {
                return base.Address;
            }
            set
            {
                base.Address = value;
                IOAddress = value;
            }
        }
        ///// <summary>
        ///// Parsing IO (in FormattedIO488)
        ///// </summary>
        //public override FormattedIO488 parseIO
        //{
        //    get
        //    {
        //        return base.parseIO;
        //    }
        //    set
        //    {
        //        parseIO = value;
        //        base.parseIO = value;
        //    }
        //}
        /// <summary>
        /// Open Equipment IO
        /// </summary>
        public override void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    base.parseIO = new FormattedIO488();
                    base.parseIO.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 10000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    base.parseIO.IO = null;
                    return;
                }
                Init(base.parseIO);
            }
        }
        /// <summary>
        /// Close Equipment IO
        /// </summary>
        public override void CloseIO()
        {
            base.parseIO.IO.Close();
        }
        /// <summary>
        /// Driver Revision control
        /// </summary>
        /// <returns>Driver's Version</returns>
        public override string Version()
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
        public new cCommonFunction BasicCommand; // Basic Command for General Equipment (Must be Initialized)
        public new cCalculate Calculate;
        public new cDisplay Display;
        public new cSystem System;
        public new cFormat Format;
        public new cInitiate Initiate;
        public new cMemory Memory;
        public new cSense Sense;
        public new cTrigger Trigger;

        /// <summary>
        /// Initializing all Parameters
        /// </summary>
        /// <param name="IOInit"></param>
        public override void Init(FormattedIO488 IOInit)
        {

            base.BasicCommand = new cCommonFunction(IOInit);
            base.Calculate = new cCalculate(IOInit);
            base.Display = new cDisplay(IOInit);
            base.System = new cSystem(IOInit);
            base.Format = new cFormat(IOInit);
            base.Initiate = new cInitiate(IOInit);
            base.Memory = new cMemory(IOInit);
            base.Sense = new cSense(IOInit);
            base.Trigger = new cTrigger(IOInit);
        }
        #endregion

        #region "Class Functional Codes"
        /// <summary>
        /// Calculate Class Function.
        /// </summary>
        public class cCalculate : cENA.cCalculate
        {
            public new cFixtureSimulator FixtureSimulator;
            public new cParameter Par;
            public new cFormat Format;
            public new cFunction Func;
            public new cData Data;

            public cCalculate(FormattedIO488 parse)
                : base(parse)
            {
                base.FixtureSimulator = new cFixtureSimulator(parse);
                base.Par = new cParameter(parse);
                base.Format = new cFormat(parse);
                base.Func = new cFunction(parse);
                base.Data = new cData(parse);
            }
            /// <summary>
            /// Fixture Simulator Class Function
            /// </summary>
            public class cFixtureSimulator : cENA.cCalculate.cFixtureSimulator
            {
                public new cSended SENDed;
                public new cBalun BALun;
                public cFixtureSimulator(FormattedIO488 parse)
                    : base(parse)
                {
                    base.BALun = new cBalun(parse);
                    base.SENDed = new cSended(parse);
                }

                public class cSended : cENA.cCalculate.cFixtureSimulator.cSended
                {
                    public new cPMCircuit PMCircuit;
                    public new cZConversion ZConversion;
                    public cSended(FormattedIO488 parse)
                        : base(parse)
                    {
                        base.PMCircuit = new cPMCircuit(parse);
                        base.ZConversion = new cZConversion(parse);
                    }
                    public class cPMCircuit : cENA.cCalculate.cFixtureSimulator.cSended.cPMCircuit
                    {
                        public cPMCircuit(FormattedIO488 parse) : base(parse) { }
                        public override void R(int PortNumber, double Resistance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public override void R(int ChannelNumber, int PortNumber, double Resistance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public override void L(int PortNumber, double Inductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public override void L(int ChannelNumber, int PortNumber, double Inductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public override void C(int PortNumber, double Capacitance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public override void C(int ChannelNumber, int PortNumber, double Capacitance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public override void G(int PortNumber, double Conductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }
                        public override void G(int ChannelNumber, int PortNumber, double Conductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }

                        public override void Type(int PortNumber, e_PortMatchType PortType)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public override void Type(int ChannelNumber, int PortNumber, e_PortMatchType PortType)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }

                        public override void User(int PortNumber)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:USER");
                        }
                        public override void User(int ChannelNumber, int PortNumber)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":PAR:USER");
                        }

                        public override void UserFilename(int PortNumber, string S2PFilename)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }
                        public override void UserFilename(int ChannelNumber, int PortNumber, string S2PFilename)
                        {
                            // ZNB: Not existing.
                            //(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:PORT" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }

                        public override void State(bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:SEND:PMC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:SEND:PMC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                        public override void State(int ChannelNumber, bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:PMC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                    }
                    public class cZConversion : cENA.cCalculate.cFixtureSimulator.cSended.cZConversion
                    {
                        public cZConversion(FormattedIO488 parse) : base(parse) { }
                        public override void Imag(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public override void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public override void Real(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public override void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public override void Z0(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public override void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:PORT" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }

                        public override void State(bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:SEND:ZCON:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:SEND:ZCON:STAT OFF");
                                    break;
                            }
                             * */
                        }
                        public override void State(int ChannelNumber, bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:SEND:ZCON:STAT OFF");
                                    break;
                            }
                             * */
                        }
                    }

                }
                public class cBalun : cENA.cCalculate.cFixtureSimulator.cBalun
                {
                    public new cParameter Parameter;
                    public new cDiffMatch DiffMatch;
                    public new cDiffZConv DiffZConv;
                    public new cCmnZConv CmnZConv;
                    public cBalun(FormattedIO488 parse)
                        : base(parse)
                    {
                        base.Parameter = new cParameter(parse);
                        base.DiffMatch = new cDiffMatch(parse);
                        base.DiffZConv = new cDiffZConv(parse);
                        base.CmnZConv = new cCmnZConv(parse);
                    }
                    public override void Topology(int ChannelNumber, e_BalunDevice Device, string portTopology)
                    {
                        // ZNB: Not existing.
                        //SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:TOP:" + Device.ToString() + " " + portTopology);
                    }
                    public override void Device(int ChannelNumber, e_BalunDevice Device)
                    {
                        // ZNB: Not existing.
                        //SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DEV " + Device.ToString());
                    }
                    public override e_BalunDevice Device(int ChannelNumber)
                    {
                        // ZNB: Not existing.
                        //return ((e_BalunDevice)Enum.Parse(typeof(e_BalunDevice), (ReadCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DEV?"))));
                        return e_BalunDevice.BAL;
                    }
                    public class cCmnZConv : cENA.cCalculate.cFixtureSimulator.cBalun.cCmnZConv
                    {
                        public cCmnZConv(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public override void Imag(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public override void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public override void Real(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public override void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public override void Z0(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public override void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public override void State(bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:BAL:CZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:BAL:CZC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                        public override void State(int ChannelNumber, bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:CZC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                    }
                    public class cDiffZConv : cENA.cCalculate.cFixtureSimulator.cBalun.cDiffZConv
                    {
                        public cDiffZConv(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public override void Imag(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public override void Imag(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":IMAG " + value.ToString());
                        }
                        public override void Real(int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public override void Real(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":REAL " + value.ToString());
                        }
                        public override void Z0(int PortNumber, double value)
                        {
                            SendCommand(":CALC1:FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public override void Z0(int ChannelNumber, int PortNumber, double value)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:BPOR" + PortNumber.ToString() + ":Z0 " + value.ToString());
                        }
                        public override void State(bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC1:FSIM:BAL:DZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC1:FSIM:BAL:DZC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                        public override void State(int ChannelNumber, bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {
                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DZC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                    }
                    public class cDiffMatch : cENA.cCalculate.cFixtureSimulator.cBalun.cDiffMatch
                    {
                        public cDiffMatch(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public override void State(int ChannelNumber, bool Set)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (Set)
                            {

                                case true:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:STAT ON");
                                    break;
                                case false:
                                    SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:STAT OFF");
                                    break;
                            }
                             * */
                        }
                        public override void R(int PortNumber, double Resistance)
                        {

                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DMC:BPORT" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public override void R(int ChannelNumber, int PortNumber, double Resistance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:R " + Resistance.ToString());
                        }
                        public override void L(int PortNumber, double Inductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public override void L(int ChannelNumber, int PortNumber, double Inductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:L " + Inductance.ToString());
                        }
                        public override void C(int PortNumber, double Capacitance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public override void C(int ChannelNumber, int PortNumber, double Capacitance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:C " + Capacitance.ToString());
                        }
                        public override void G(int PortNumber, double Conductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }
                        public override void G(int ChannelNumber, int PortNumber, double Conductance)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":PAR:G " + Conductance.ToString());
                        }

                        public override void Type(int PortNumber, e_PortMatchType PortType)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public override void Type(int ChannelNumber, int PortNumber, e_PortMatchType PortType)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":TYPE " + PortType.ToString());
                        }
                        public override void UserFilename(int PortNumber, string S2PFilename)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC1:FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }
                        public override void UserFilename(int ChannelNumber, int PortNumber, string S2PFilename)
                        {
                            // ZNB: Not existing.
                            //SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:BAL:DMC:BPOR" + PortNumber.ToString() + ":USER:FIL \"" + S2PFilename + "\"");
                        }
                    }
                    public class cParameter : cENA.cCalculate.cFixtureSimulator.cBalun.cParameter
                    {
                        public cParameter(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public override void Status(int ChannelNumber, int TraceNumber, bool status)
                        {
                            // ZNB: Not existing.
                            /*
                            switch (status)
                            {
                                case true:
                                    SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT 1");
                                    break;
                                default:
                                    SendCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT 0");
                                    break;
                            }
                             * */
                        }
                        public override bool Status(int ChannelNumber, int TraceNumber)
                        {
                            return common.CStr2Bool("0");
                            //HWL                            return (common.CStr2Bool(ReadCommand("CALC" + ChannelNumber.ToString() + ":FSIM:BAL:PAR" + TraceNumber.ToString() + ":STAT?")));
                        }
                        // Quick Solution
                        public override void Parameter(int ChannelNumber, int TraceNumber, e_BalunDevice Device, string TraceLabel)
                        {

                            // ZNB: Not existing.
                            /*
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
                             * */
                        }
                        public override string Parameters(int ChannelNumber, int TraceNumber)
                        {
                            string rtnStr = "";
                            // ZNB: Not existing.
                            /*
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
                             * */
                            return rtnStr;
                        }
                    }
                }
                public override void State(bool Set)
                {
                    // ZNB: Not existing.
                    /*
                    switch (Set)
                    {
                        case true:
                            SendCommand(":CALC1:FSIM:STAT ON");
                            break;
                        case false:
                            SendCommand(":CALC1:FSIM:STAT OFF");
                            break;
                    }
                     * */
                }
                public override void State(int ChannelNumber, bool Set)
                {
                    // ZNB: Not existing.
                    /*
                    switch (Set)
                    {
                        case true:
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT ON");
                            break;
                        case false:
                            SendCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT OFF");
                            break;
                    }
                     * */
                }
                public override bool State()
                {

                    // ZNB: Not existing.
                    //return (common.CStr2Bool(ReadCommand(":CALC1:FSIM:STAT?")));
                    return false;
                }
                public override bool State(int ChannelNumber)
                {
                    //return (bool.Parse(ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT?")));
                    // ZNB: Not existing.
                    //return (common.CStr2Bool(ReadCommand(":CALC" + ChannelNumber.ToString() + ":FSIM:STAT?")));
                    return false;
                }
            }
            public class cParameter : cENA.cCalculate.cParameter
            {
                public cParameter(FormattedIO488 parse) : base(parse) { }
                //public override void Count(int count)
                //{
                // ENA: SendCommand("CALC1:PAR:COUN " + count.ToString());
                // ZVT: Not existing.
                //}                
                public override void Count(int ChannelNumber, int Trace)
                {
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN " + Trace.ToString());
                    // ZVT: Not existing.
                }
                public override int Count()
                {
                    return 0;
                    //return (Convert.ToInt32(ReadCommand("CALC1:PAR:COUN?")));
                }
                public override int Count(int ChannelNumber)     //HWL
                {
                    // ENA: return (Convert.ToInt32(ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:COUN?")));
                    // ZVT:
                    string read;
                    read = ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:CAT?");
                    string[] parts = (read.Split(','));
                    return (parts.Length / 2);
                }
                public override void Define(int Trace, e_SParametersDef def)
                {
                    // ENA: SendCommand("CALC1:PAR" + Trace.ToString() + ":DEF " + Define.ToString());
                    // ZVT:
                    Define(1, Trace, def);
                }
                public override void Define(int ChannelNumber, int Trace, e_SParametersDef Define)
                {
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF " + Define.ToString());
                    // ZVT:
                    string read = ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:CAT?");
                    if (read.Contains("Trc" + Trace.ToString()))
                    {
                        SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:MEAS 'Trc" + Trace.ToString() + "','" + Define.ToString() + "'");
                    }
                    else
                    {
                        /*
                        // FY20120913: Create new diagram area for each two traces
                        int diag = Trace / 2;
                        if (Trace % 2 == 0)
                        {
                            SendCommand("DISPlay:WINDow" + diag.ToString() + ":STATe ON");
                        }
                         * */
                        SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SDEF 'Trc" + Trace.ToString() + "','" + Define.ToString() + "'");
                        SendCommand("DISP:WIND" + ChannelNumber.ToString() + ":TRAC" + Trace.ToString() + ":FEED 'Trc" + Trace.ToString() + "'");
                    }
                }
                public override void Define_Trace(int ChannelNumber, int Trace, string TraceName)
                {
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF " + TraceName);
                    // ZVT:
                    string read = ReadCommand("CALC1:PAR:CAT?");
                    if (read.Contains("Trc" + Trace.ToString()))
                    {
                        SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:MEAS 'Trc" + Trace.ToString() + "','" + TraceName + "'");
                    }
                    else
                    {
                        // FY20120913: Create new diagram area for each two traces
                        //int diag = (Trace - 1) / 2 + 1;
                        //if ((Trace - 1) % 2 == 0)
                        //{
                        SendCommand("DISPlay:WINDow" + ChannelNumber.ToString() + ":STATe ON");
                        //}
                        SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SDEF 'Trc" + Trace.ToString() + "','" + TraceName + "'");
                        SendCommand("DISP:WIND" + ChannelNumber.ToString() + ":TRAC" + Trace.ToString() + ":FEED 'Trc" + Trace.ToString() + "'");
                    }
                }
                public override string GetTraceCategory(int ChannelNumber)
                {
                    // ENA: return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?"));
                    // ZVT:
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:CAT?"));
                }

                public override string Define(int ChannelNumber, int Trace)
                {
                    // ENA: return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":DEF?"));
                    // ZVT:
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:MEAS? 'Trc" + Trace.ToString() + "'"));
                }

                public override e_SParametersDef Define_Enum(int ChannelNumber, int Trace)
                {
                    // ENA: return (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), (ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":CAT?")));
                    // ZVT:
                    string read;        //HWL
                    string[] readarray;
                    read = ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:CAT?");
                    char[] delimiters = new char[] { ',', '\r', '\n', '\'' };
                    readarray = read.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    read = readarray[(Trace - 1) * 2 + 1];
                    return (e_SParametersDef)Enum.Parse(typeof(e_SParametersDef), read);
                }
                public override void Select(int Trace)
                {
                    string read = ReadCommand("CALC1:PAR:CAT?");
                    if (read.Contains("Trc" + Trace.ToString()))
                    {
                        SendCommand("CALC1:PAR:SEL 'Trc" + Trace.ToString() + "'");

                    }
                }
                public override void Select(int ChannelNumber, int Trace)
                {
                    string read = ReadCommand("CALC" + ChannelNumber.ToString() + ":PAR:CAT?");
                    if (read.Contains("Trc" + Trace.ToString()))
                    {
                        SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc" + Trace.ToString() + "'");

                    }
                }
                public override void SPORT(int Trace, double value)
                {

                    //SendCommand("CALC1:PAR" + Trace.ToString() + ":SPOR " + value.ToString());
                }
                public override void SPORT(int ChannelNumber, int Trace, double value)
                {
                    //SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + Trace.ToString() + ":SPOR " + value.ToString());
                }
            }
            public class cFormat : cENA.cCalculate.cFormat
            {
                public cFormat(FormattedIO488 parse) : base(parse) { }
                public override void Format(int TraceNumber, e_SFormat format)
                {
                    // added code to select the trace before changing the Format
                    // ENA: SendCommand("CALC1:PAR" + TraceNumber.ToString() + ":SEL");
                    // ZVT:
                    Format(1, TraceNumber, format);
                }
                public override void Format(int ChannelNumber, int TraceNumber, e_SFormat format)
                {
                    // added code to select the trace before changing the Format
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    // ZVT:
                    if (format == e_SFormat.SCOM)
                    {
                        format = e_SFormat.SMIT;
                    }
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc" + TraceNumber.ToString() + "'");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format.ToString());

                    //Temporary set to low dist
                    //SendCommand("SENS:POW:GAIN:GLOB LDIS");
                }
                public override void Format(int TraceNumber, string format)
                {
                    // added code to select the trace before changing the Format
                    // ENA: SendCommand("CALC1:PAR" + TraceNumber.ToString() + ":SEL");
                    // ZVT:
                    Format(1, TraceNumber, format);
                }
                public override void Format(int ChannelNumber, int TraceNumber, string format)
                {
                    // added code to select the trace before changing the Format
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    // ZVT:
                    if (format.ToUpper() == "SCOM")
                    {
                        format = "SMIT";
                    }
                    int Trace = ChannelNumber * TraceNumber;
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc" + TraceNumber.ToString() + "'");
                    SendCommand("CALC" + ChannelNumber.ToString() + ":FORM " + format);
                }
                public override e_SFormat Format(int TraceNumber)
                {
                    // added code to select the trace before reading the Format
                    // ENA: SendCommand("CALC1:PAR" + TraceNumber.ToString() + ":SEL");
                    // ZVT:
                    return Format(1, TraceNumber);
                }
                public override e_SFormat Format(int ChannelNumber, int TraceNumber)
                {
                    // added code to select the trace before reading the Format
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR" + TraceNumber.ToString() + ":SEL");
                    // ZVT:
                    int Trace = ChannelNumber * TraceNumber;
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc" + TraceNumber.ToString() + "'");
                    string tmp = ReadCommand("CALC" + ChannelNumber.ToString() + ":FORM?");
                    e_SFormat Format = (e_SFormat)Enum.Parse(typeof(e_SFormat), tmp);
                    return (Format);
                }
            }
            public class cFunction : cENA.cCalculate.cFunction
            {

                public cFunction(FormattedIO488 parse)
                    : base(parse)
                {

                }
                //public override string Points()
                //{
                //    return (ReadCommand("CALC1:FUNC:POIN?"));
                //}
                //public override string Points(int ChannelNumber)
                //{
                //    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":FUNC:POIN?"));
                //}
                public override int Points()
                {
                    return (Convert.ToInt32(ReadCommand("CALC1:FUNC:POIN?")));
                }
                public override int Points(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("CALC" + ChannelNumber.ToString() + ":FUNC:POIN?")));
                }
            }

            #region "Selected"
            public class cData : cENA.cCalculate.cData
            {
                public double steps;
                public double offset;
                public double IFBW;
                public int points;
                public int numofbins;
                public cData(FormattedIO488 parse)
                    : base(parse)
                {
                    steps = 0.0;
                    offset = 0.0;
                    IFBW = 0.0;
                    points = 0;
                    numofbins = 0;
                }
                private int GetBinOffset(string spar)
                {
                    int binoffset;

                    // And process the s parameters
                    switch (spar)
                    {
                        case "s11":
                        case "s12":
                        case "s21":
                        case "s22":
                            binoffset = numofbins * 0;
                            break;
                        case "s33":
                        case "s34":
                        case "s43":
                        case "s44":
                            binoffset = numofbins * 1;
                            break;
                        case "s55":
                        case "s56":
                        case "s65":
                        case "s66":
                            binoffset = numofbins * 2;
                            break;
                        case "s77":
                        case "s78":
                        case "s87":
                        case "s88":
                            binoffset = numofbins * 3;
                            break;
                        default:
                            binoffset = 0;
                            break;
                    }
                    return binoffset;
                }
                public override void ParallelMode(bool bEnable)
                {
                    // ENA: NA
                    // ZVT:
                    // FY20130406
                    // The parallel mode needs to be selected after all frequency parameters have been set, because it uses
                    // the current settings for start-/stop frequency, number of points and IFBW to calculate the offset
                    // for the port groups and the number of bins, which are used to shift the results. The offset is
                    // calculated by IFBW*20 and than rounded up to the next point in the frequency grid, so the number of
                    // bins is always an integer.
                    string s;
                    int chan;
                    int port;
                    int portoffset;
                    int numports;
                    string[] parameters;
                    int i;

                    // Get number of ports
                    numports = int.Parse(ReadCommand(":INSTrument:PORT:COUNt?"));

                    // Get active channel
                    chan = int.Parse(ReadCommand(":INSTrument:NSELect?"));

                    // Get stepsize for channel
                    steps = double.Parse(ReadCommand(":SENSe" + chan.ToString() + ":SWEep:STEP?"));

                    // Get IFBW for channel
                    IFBW = double.Parse(ReadCommand(":SENSe" + chan.ToString() + ":BWIDth?"));

                    // Calculate offset, we need at last 20 times the IFBW
                    offset = steps;
                    while (offset < (20 * IFBW))
                    {
                        offset = offset + steps;
                    }

                    // Get list of traces and meas parameters
                    s = ReadCommand(":CALCulate" + chan.ToString() + ":PARameter:CATalog?");
                    s = s.Replace("'", "");
                    s = s.Trim();
                    parameters = s.Split(new char[] { ',' });

                    // Activate Mode
                    if (bEnable)
                    {
                        // Set port groups
                        SendCommand(":SOURce" + chan.ToString() + ":GROup1:CLEar ALL");
                        for (port = 1; port < numports + 1; port += 2)
                        {
                            portoffset = (port - 1) / 2 + 1;
                            SendCommand(":SOURce" + chan.ToString() + ":GROup" + portoffset.ToString() + " " + port.ToString() + "," + (port + 1).ToString());
                        }

                        // Set arbitrary mode
                        for (port = 1; port < numports + 1; port++)
                        {
                            portoffset = (port - 1) / 2;
                            // Set synthesizer output frequency
                            SendCommand(":SOURce" + chan.ToString() + ":FREQuency" + port.ToString() + ":CONVersion:ARBitrary:IFrequency 1,1," + (offset * portoffset).ToString() + ",SWE");
                            // Set receiver input frequeny
                            SendCommand(":SENSe" + chan.ToString() + ":FREQuency" + port.ToString() + ":OFFSet:WAVes " + (offset * portoffset).ToString());
                        }
                        // Enable Cal and Corr at Base Frequency
                        SendCommand(":SENSe" + chan.ToString() + ":CORRection:CBFReq:STATe ON");
                        // Set the number of bins
                        numofbins = (int)(offset / steps);
                    }
                    else
                    {
                        // Ungroup ports
                        SendCommand(":SOURce" + chan.ToString() + ":GROup1:CLEar ALL");
                        // Disable arbitrary mode
                        SendCommand(":SENSe" + chan.ToString() + ":FREQuency:CONVersion FUNDamental");
                        for (port = 1; port < numports + 1; port++)
                        {
                            SendCommand(":SENSe" + chan.ToString() + ":FREQuency" + port.ToString() + ":OFFSet:WAVes 0");
                        }
                        // Disable cal and corr at Base Frequency
                        SendCommand(":SENSe" + chan.ToString() + ":CORRection:CBFReq:STATe OFF");
                        // Set the number of bins
                        numofbins = 0;
                    }

                    // Restore meas parameters back to original value
                    for (i = 0; i < parameters.Count(); i += 2)
                    {
                        SendCommand(":CALCulate" + chan.ToString() + ":PARameter:MEASure '" + parameters[i] + "', '" + parameters[i + 1] + "'");
                    }
                }
                public override double[] SData()
                {
                    // ENA: return (ReadIEEEBlock("CALC:SEL:DATA:SDAT?"));
                    // ZVT:
                    return SData(1);
                }
                public override double[] SData(int ChannelNumber)
                {
                    // ENA: return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:SDAT?"));
                    // ZVT:
                    // FY20130405 We are doing the shifting
                    string spar;
                    string strc;
                    double[] result;
                    int points;
                    int binoffset;
                    int i;

                    // Read the block data from instrument
                    result = ReadIEEEBlock(":CALCulate" + ChannelNumber.ToString() + ":DATA? SDAT");

                    // Shift the results if the number of bins is greater than zero
                    if (numofbins > 0)
                    {
                        // Calculate the number of points from what we know
                        points = result.Count() / 2;

                        // Query the name of the active trace
                        strc = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:SELect?").Trim();
                        strc = strc.Replace("'", "");

                        // Query the S parameter selected into the active trace
                        spar = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:MEASure? '" + strc + "'").Trim().ToLower();
                        spar = spar.Replace("'", "");

                        // And calculate the binoffset from the S-parameter
                        binoffset = GetBinOffset(spar);

                        // Copy the data in a loop if binoffset > 0
                        if (binoffset > 0)
                        {
                            // Copy data
                            for (i = 0; i < points - binoffset; i++)
                            {
                                int idx1 = i;
                                int idx2 = binoffset + i;
                                result[idx1] = result[idx2];
                                result[idx1 + points] = result[idx2 + points];
                            }
                            // ToDo: Replicate last point
                        }
                    }
                    return result;
                    // FY20130405 We do the shifting in the function
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA? SDAT"));
                }
                public override double[] FData()
                {
                    // ENA: return (ReadIEEEBlock("CALC:SEL:DATA:FDAT?"));
                    // ZVT:
                    return FData(1);
                }
                public override double[] FData(int ChannelNumber)
                {
                    // ENA: return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:FDAT?"));
                    // ZVT:
                    // FY20130405 We are doing the shifting
                    string spar;
                    string strc;
                    double[] result;
                    int points;
                    int binoffset;
                    int i;

                    // Read the block data from instrument
                    result = ReadIEEEBlock(":CALCulate" + ChannelNumber.ToString() + ":DATA? FDAT");

                    // Shift the results if the number of bins is greater than zero
                    if (numofbins > 0)
                    {
                        // Calculate the number of points from what we know
                        points = result.Count();

                        // Query the name of the active trace
                        strc = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:SELect?").Trim();
                        strc = strc.Replace("'", "");

                        // Query the S parameter selected into the active trace
                        spar = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:MEASure? '" + strc + "'").Trim().ToLower();
                        spar = spar.Replace("'", "");

                        // And calculate the binoffset from the S-parameter
                        binoffset = GetBinOffset(spar);

                        // Copy the data in a loop if binoffset > 0
                        if (binoffset > 0)
                        {
                            // Copy data
                            for (i = 0; i < points - binoffset; i++)
                            {
                                int idx1 = i;
                                int idx2 = binoffset + i;
                                result[idx1] = result[idx2];
                            }
                            // ToDo: Replicate last point
                        }
                    }
                    return result;
                    // FY20130405 We do the shifting in the function
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA? FDAT"));
                }
                public override double[] SMemoryData()
                {
                    return SMemoryData(1);
                }
                public override double[] SMemoryData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:SMEM?"));
                }
                public override double[] FMemoryData()
                {
                    return FMemoryData(1);
                }
                public override double[] FMemoryData(int ChannelNumber)
                {
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:FMEM?"));
                }

                //ZVT 18/09/13 modded by LKS
                #region Data enhanced fetch for multi traces

                public override double[] FMultiTrace_Data(string TraceNumber)
                {
                    double[] result;
                    double[] data;
                    string[] traces;
                    int points;
                    int dataLength;
                    int tracesNo;
                    int trace;
                    traces = TraceNumber.Split(new char[] { ',' });

                    tracesNo = traces.Length;

                    // Read the block data from instrument
                    data = ReadIEEEBlock(":CALCulate:DATA? FDAT");

                    //Get data points
                    dataLength = data.Length;

                    //Get reading points
                    points = dataLength / 8;

                    //Build the data
                    result = new double[tracesNo * points * 2];
                    for (int i = 1; i <= tracesNo; i++)
                    {
                        trace = Convert.ToInt32(traces[i - 1]); //Get trace number

                        for (int j = 1; j <= points * 2; j++)
                        {
                            result[i] = data[trace * j - 1];
                        }
                    }

                    return result;

                    // return (ReadIEEEBlock("CALC:SEL:DATA:MFD? \"" + TraceNumber + "\""));
                }
                public override double[] FMultiTrace_Data(int ChannelNumber, string TraceNumber)
                {
                    double[] result;
                    double[] data;
                    string[] traces;
                    int points;
                    int dataLength;
                    int tracesNo;
                    int trace;
                    traces = TraceNumber.Split(new char[] { ',' });

                    tracesNo = traces.Length;

                    // Read the block data from instrument
                    data = ReadIEEEBlock(":CALCulate" + ChannelNumber.ToString() + ":DATA? FDAT");

                    //Get data points
                    dataLength = data.Length;

                    //Get reading points
                    points = dataLength / 8;

                    //Build the data
                    result = new double[tracesNo * points * 2];
                    for (int i = 1; i <= tracesNo; i++)
                    {
                        trace = Convert.ToInt32(traces[i - 1]); //Get trace number

                        for (int j = 1; j <= points * 2; j++)
                        {
                            result[i] = data[trace * j - 1];
                        }
                    }

                    return result;
                    // return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MFD? \"" + TraceNumber + "\""));
                }
                public override double[] UMultiTrace_Data(string TraceNumber)
                {
                    double[] result;
                    double[] data;
                    string[] traces;
                    int points;
                    int dataLength;
                    int tracesNo;
                    int trace;
                    traces = TraceNumber.Split(new char[] { ',' });

                    tracesNo = traces.Length;

                    // Read the block data from instrument
                    data = ReadIEEEBlock(":CALCulate:DATA? SDAT");

                    //Get data points
                    dataLength = data.Length;

                    //Get reading points
                    points = dataLength / 8;

                    //Build the data
                    result = new double[tracesNo * points * 2];
                    for (int i = 1; i <= tracesNo; i++)
                    {
                        trace = Convert.ToInt32(traces[i - 1]); //Get trace number

                        for (int j = 1; j <= points * 2; j++)
                        {
                            result[i] = data[trace * j - 1];
                        }
                    }

                    return result;
                    // return (ReadIEEEBlock("CALC:SEL:DATA:MSD? \"" + TraceNumber + "\""));
                }
                public override double[] UMultiTrace_Data(int ChannelNumber, string TraceNumber)
                {
                    double[] result;
                    double[] data;
                    string[] traces;
                    int points;
                    int dataLength;
                    int tracesNo;
                    int trace;
                    traces = TraceNumber.Split(new char[] { ',' });

                    tracesNo = traces.Length;

                    // Read the block data from instrument
                    data = ReadIEEEBlock(":CALCulate" + ChannelNumber.ToString() + ":DATA? SDAT");

                    //Get data points
                    dataLength = data.Length;

                    //Get reading points
                    points = dataLength / 8;

                    //Build the data
                    result = new double[tracesNo * points * 2];
                    for (int i = 1; i <= tracesNo; i++)
                    {
                        trace = Convert.ToInt32(traces[i - 1]); //Get trace number

                        for (int j = 1; j <= points * 2; j++)
                        {
                            result[i] = data[trace * j - 1];
                        }
                    }

                    return result;
                    // return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":SEL:DATA:MSD? \"" + TraceNumber + "\""));
                }

                #endregion


                public double[] FM;
                public override double[] AllFData(int ChannelNumber)
                {
                    // FY20130405 We are processing all data in one block and handle the shifting
                    string s;
                    string[] parameters;
                    double[] result;
                    int index;
                    int points;
                    int bufferoffset;
                    int binoffset;
                    int i;

                    // Read the block data from instrument
                    result = ReadIEEEBlock("CALCulate" + ChannelNumber + ":DATA:CHAN:ALL? FDAT");
                    //result = ReadIEEEBlock("CALCulate:DATA:ALL? FDATA");
                    // Shift the results if the number of bins is greater than zero
                    if (numofbins > 0)
                    {
                        // Get list of traces and meas parameters to do the correct shifting
                        s = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:CATalog?").Trim();
                        s = s.Replace("'", "");
                        parameters = s.Split(new char[] { ',' });

                        // Calculate the number of points from what we know
                        points = result.Count() / parameters.Count() / 4;

                        // And process all s parameters
                        for (index = 0; index < parameters.Count(); index += 2)
                        {
                            string spar = parameters[index + 1].ToLower();
                            // And calculate the binoffset from the S-parameter
                            binoffset = GetBinOffset(spar);
                            bufferoffset = (index / 2) * points * 2;

                            // Copy the data in a loop if binoffset > 0
                            if (binoffset > 0)
                            {
                                // Copy data
                                for (i = 0; i < points - binoffset; i++)
                                {
                                    int idx1 = bufferoffset + i;
                                    int idx2 = bufferoffset + binoffset + i;
                                    result[idx1] = result[idx2];
                                    result[idx1 + points] = result[idx2 + points];
                                }
                                // ToDo: Replicate last point
                            }
                        }
                    }
                    return result;

                    // FY20130405 Old version w/o shifting
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:CALL? SDATA"));
                }
                public override double[] AllSData(int ChannelNumber)
                {
                    // FY20130405 We are processing all data in one block and handle the shifting
                    string s;
                    string[] parameters;
                    double[] result;
                    int index;
                    int points;
                    int bufferoffset;
                    int binoffset;
                    int i;

                    // Read the block data from instrument
                    result = ReadIEEEBlock("CALCulate" + ChannelNumber + ":DATA:CHAN:ALL? SDAT");
                    //result = ReadIEEEBlock("CALC:DATA:ALL? SDATA");

                    // Shift the results if the number of bins is greater than zero
                    if (numofbins > 0)
                    {
                        // Get list of traces and meas parameters to do the correct shifting
                        s = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:CATalog?").Trim();
                        s = s.Replace("'", "");
                        parameters = s.Split(new char[] { ',' });

                        // Calculate the number of points from what we know
                        points = result.Count() / parameters.Count() / 4;

                        // And process all s parameters
                        for (index = 0; index < parameters.Count(); index += 2)
                        {
                            string spar = parameters[index + 1].ToLower();
                            // And calculate the binoffset from the S-parameter
                            binoffset = GetBinOffset(spar);
                            bufferoffset = (index / 2) * points * 2;

                            // Copy the data in a loop if binoffset > 0
                            if (binoffset > 0)
                            {
                                // Copy data
                                for (i = 0; i < points - binoffset; i++)
                                {
                                    int idx1 = bufferoffset + i;
                                    int idx2 = bufferoffset + binoffset + i;
                                    result[idx1] = result[idx2];
                                    result[idx1 + points] = result[idx2 + points];
                                }
                                // ToDo: Replicate last point
                            }
                        }
                    }
                    return result;

                    // FY20130405 Old version w/o shifting
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:CALL? SDATA"));
                }

                public override double[] CAllFData(int ChannelNumber)
                {
                    // FY20130405 We are processing all data in one block and handle the shifting
                    string s;
                    string[] parameters;
                    double[] result;
                    int index;
                    int points;
                    int bufferoffset;
                    int binoffset;
                    int i;

                    // Read the block data from instrument
                    result = ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA? FDATA");

                    // Shift the results if the number of bins is greater than zero
                    if (numofbins > 0)
                    {
                        // Get list of traces and meas parameters to do the correct shifting
                        s = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:CATalog?").Trim();
                        s = s.Replace("'", "");
                        parameters = s.Split(new char[] { ',' });

                        // Calculate the number of points from what we know
                        points = result.Count() / parameters.Count() / 4;

                        // And process all s parameters
                        for (index = 0; index < parameters.Count(); index += 2)
                        {
                            string spar = parameters[index + 1].ToLower();
                            // And calculate the binoffset from the S-parameter
                            binoffset = GetBinOffset(spar);
                            bufferoffset = (index / 2) * points * 2;

                            // Copy the data in a loop if binoffset > 0
                            if (binoffset > 0)
                            {
                                // Copy data
                                for (i = 0; i < points - binoffset; i++)
                                {
                                    int idx1 = bufferoffset + i;
                                    int idx2 = bufferoffset + binoffset + i;
                                    result[idx1] = result[idx2];
                                    result[idx1 + points] = result[idx2 + points];
                                }
                                // ToDo: Replicate last point
                            }
                        }
                    }
                    return result;

                    // FY20130405 Old version w/o shifting
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:CALL? SDATA"));
                }
                public override double[] CAllSData(int ChannelNumber)
                {
                    // FY20130405 We are processing all data in one block and handle the shifting
                    string s;
                    string[] parameters;
                    double[] result;
                    int index;
                    int points;
                    int bufferoffset;
                    int binoffset;
                    int i;

                    // Read the block data from instrument
                    result = ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA? SDATA");

                    // Shift the results if the number of bins is greater than zero
                    if (numofbins > 0)
                    {
                        // Get list of traces and meas parameters to do the correct shifting
                        s = ReadCommand(":CALCulate" + ChannelNumber.ToString() + ":PARameter:CATalog?").Trim();
                        s = s.Replace("'", "");
                        parameters = s.Split(new char[] { ',' });

                        // Calculate the number of points from what we know
                        points = result.Count() / parameters.Count() / 4;

                        // And process all s parameters
                        for (index = 0; index < parameters.Count(); index += 2)
                        {
                            string spar = parameters[index + 1].ToLower();
                            // And calculate the binoffset from the S-parameter
                            binoffset = GetBinOffset(spar);
                            bufferoffset = (index / 2) * points * 2;

                            // Copy the data in a loop if binoffset > 0
                            if (binoffset > 0)
                            {
                                // Copy data
                                for (i = 0; i < points - binoffset; i++)
                                {
                                    int idx1 = bufferoffset + i;
                                    int idx2 = bufferoffset + binoffset + i;
                                    result[idx1] = result[idx2];
                                    result[idx1 + points] = result[idx2 + points];
                                }
                                // ToDo: Replicate last point
                            }
                        }
                    }
                    return result;

                    // FY20130405 Old version w/o shifting
                    //return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:CALL? SDATA"));
                }
                public override string CAllCat(int ChannelNumber)
                {
                    // FY20120911 We are processing all data in one block
                    return (ReadCommand("CALC" + ChannelNumber.ToString() + ":DATA:CALL:CAT?"));
                }
            }
            #endregion
        }
        public class cDisplay : cENA.cDisplay
        {
            public new cWindow Window;
            public cDisplay(FormattedIO488 parse)
                : base(parse)
            {
                base.Window = new cWindow(parse);
            }
            public override void Enable(bool state)
            {
                switch (state)
                {
                    case true:
                        // ENA: SendCommand("DISP:ENAB ON");
                        // ZVT:
                        SendCommand(":SYST:DISP:UPD ON");
                        break;
                    case false:
                        // ENA: SendCommand("DISP:ENAB OFF");
                        // ZVT:
                        SendCommand(":SYST:DISP:UPD OFF");
                        break;
                }
            }
            public override void Update(bool state)
            {
                switch (state)
                {
                    case true:
                        // ENA: SendCommand("DISP:ENAB ON");
                        // ZVT:
                        SendCommand(":SYST:DISP:UPD ON");
                        break;
                    case false:
                        // ENA: SendCommand("DISP:ENAB OFF");
                        // ZVT:
                        SendCommand(":SYST:DISP:UPD OFF");
                        break;
                }
            }
            public class cWindow : cENA.cDisplay.cWindow
            {
                public cWindow(FormattedIO488 parse)
                    : base(parse)
                {

                }
                public override void Activate(int ChannelNumber)
                {
                    // ENA: SendCommand("DISP:WIND" + ChannelNumber.ToString() + ":ACT");
                    // ZVT:
                    SendCommand("INST:NSEL " + ChannelNumber.ToString());
                }
                public override void Window_Layout(string layout)
                {
                    // ENA: SendCommand("DISP:SPL " + layout);
                    // ZVT: Not existing.
                }
                public override void Channel_Max(bool state)
                {
                    // ENA
                    /*
                    switch (state)
{
    case true:
        SendCommand("DISP:MAX ON");
        break;
    case false:
        SendCommand("DISP:MAX OFF");
        break;
}
                    */
                    // ZVT: Not existing.
                }
            }
        }
        public class cSystem : cENA.cSystem
        {
            public cSystem(FormattedIO488 parse)
                : base(parse)
            {
            }
            public override void Preset()
            {
                // ENA: SendCommand("SYST:PRES");
                SendCommand("*RST");
            }
        }
        public class cFormat : cENA.cFormat
        {
            public cFormat(FormattedIO488 parse) : base(parse) { }
            public override void Border(e_Format format)
            {
                SendCommand("FORM:BORD " + format.ToString());
            }
            public override void DATA(e_FormatData DataFormat)
            {
                // ENA: SendCommand("FORM:DATA " + DataFormat.ToString());
                // ZVT:
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
                SendCommand("FORM:DATA " + dataformat);
            }
        }
        public class cInitiate : cENA.cInitiate
        {
            public cInitiate(FormattedIO488 parse) : base(parse) { }
            public override void Immediate()
            {
                SendCommand("INIT:IMM:ALL");
            }
            public override void Immediate(int ChannelNumber)
            {
                //SendCommand("INIT:IMM:ALL");
                SendCommand("INIT" + ChannelNumber.ToString() + ":IMM");
            }
            public override void Continuous(bool enable)
            {
                switch (enable)
                {
                    case true:
                        SendCommand("INIT:CONT OFF");//MUST OFF FOR ZNB/ZVT
                        break;
                    case false:
                        SendCommand("INIT:CONT OFF");
                        break;
                }
            }
            public override void Continuous(int ChannelNumber, bool enable)
            {
                switch (enable)
                {
                    case true:
                        SendCommand("INIT" + ChannelNumber.ToString() + ":CONT OFF"); //MUST OFF FOR ZNB/ZVT
                        break;
                    case false:
                        SendCommand("INIT" + ChannelNumber.ToString() + ":CONT OFF");
                        break;
                }
            }
        }
        public class cMemory : cENA.cMemory
        {
            public new cLoad Load;
            public new cStore Store;
            public cMemory(FormattedIO488 parse)
                : base(parse)
            {
                base.Load = new cLoad(parse);
                base.Store = new cStore(parse);
            }
            public class cLoad : cENA.cMemory.cLoad
            {
                public cLoad(FormattedIO488 parse) : base(parse) { }
                public override void State(string StateFile)
                {
                    // ENA: SendCommand("MMEM:LOAD:STAT \"" + StateFile.Trim() + "\"");
                    // ZVT:
                    StateFile = StateFile.Replace(".STA", ".ZNX");
                    //StateFile = String.Format("{0}{1}", "C:\\Users\\Public\\Documents\\Rohde-Schwarz\\Vna\\RecallSets\\", StateFile);
                    SendCommand("MMEM:LOAD:STAT 1,\"" + StateFile.Trim() + "\"");
                }
                public override void State(int ChannelNumber, string StateFile)
                {
                    // ENA:
                    /*
                    SendCommand("CALC" + ChannelNumber.ToString() + ":PAR1:SEL");
SendCommand("MMEM:LOAD:STAT \"" + StateFile.Trim() + "\"");
                    */
                    // ZVT:
                    StateFile = StateFile.Replace(".STA", ".ZNX");
                    //StateFile = String.Format("{0}{1}", "C:\\Users\\Public\\Documents\\Rohde-Schwarz\\Vna\\RecallSets\\", StateFile);
                    SendCommand("INST:NSEL " + ChannelNumber.ToString());
                    SendCommand("MMEM:LOAD:STAT 1,\"" + StateFile.Trim() + "\"");
                }
            }
            public class cStore : cENA.cMemory.cStore
            {
                public new cSNP SNP;
                public cStore(FormattedIO488 parse)
                    : base(parse)
                {
                    base.SNP = new cSNP(parse);
                }
                public class cSNP : cENA.cMemory.cStore.cSNP
                {
                    public new cSNPType Type;
                    public cSNP(FormattedIO488 parse)
                        : base(parse)
                    {
                        base.Type = new cSNPType(parse);
                    }
                    public override void Data(int Channel, string Filename)
                    {
                        Filename = String.Format("{0}{1}", "C:\\Users\\Public\\Documents\\Rohde-Schwarz\\Vna\\Traces\\", Filename);
                        SendCommand("MMEM:STOR:TRAC:CHAN " + Channel.ToString() + ", '" + Filename.Trim() + "'"); //ZVT KS Not available for ZNB, use store trace channel instead
                    }
                    public override void Data(string Filename)
                    {
                        //SendCommand("MMEM:STOR:SNP:DATA \"" + Filename.Trim() + "\""); //ZVT KS Not available for ZNB, use store trace channel instead
                    }
                    public override void Format(e_SNPFormat format)
                    {
                        //SendCommand("MMEM:STOR:SNP:FORM " + format.ToString());
                    }
                    public class cSNPType : cENA.cMemory.cStore.cSNP.cSNPType
                    {
                        public cSNPType(FormattedIO488 parse) : base(parse) { }
                        public override void S1P(int Port1)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString());
                        }
                        public override void S2P(int Port1, int Port2)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public override void S3P(int Port1, int Port2, int Port3)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public override void S4P(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("MMEM:STOR:SNP:TYPE:S1P " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                }
                public override void SType(e_SType sType)
                {
                    SendCommand("MMEM:STOR:STYP " + sType.ToString());
                }
                public override void State(string Filename)
                {
                    // ENA: SendCommand("MMEM:STOR:STAT \"" + Filename.Trim() + "\"");
                    // ZVT:
                    SendCommand("MMEM:STOR:STAT 1,\"" + Filename.Trim() + "\"");
                }
                public override void State(int ChannelNumber, string Filename)
                {
                    // ENA: SendCommand("CALC" + ChannelNumber.ToString() + ":PAR1:SEL");
                    //      SendCommand("MMEM:STOR:STAT \"" + Filename.Trim() + "\"");
                    // ZVT: 
                    SendCommand("INST:NSEL " + ChannelNumber.ToString());
                    SendCommand("MMEM:STOR:STAT 1,\"" + Filename.Trim() + "\"");
                }
                public override void Transfer(string Filename, string Block)
                {
                    SendCommand("MMEM:STOR:TRAN \"" + Filename.Trim() + "\"," + Block.Trim()); //ZVT KS 131213 Need to find out
                }
                public override string Transfer(string Filename)
                {
                    return (ReadCommand("MMEM:STOR:TRAN? \"" + Filename.Trim() + "\""));
                }
            }
        }
        public class cSense : cENA.cSense
        {
            public new cMultiplexer Multiplexer;
            public new cCorrection Correction;
            public new cFrequency Frequency;
            public new cSegment Segment;
            public new cSweep Sweep;
            public cSense(FormattedIO488 parse)
                : base(parse)
            {
                base.Multiplexer = new cMultiplexer(parse);
                base.Correction = new cCorrection(parse);
                base.Frequency = new cFrequency(parse);
                base.Segment = new cSegment(parse);
                base.Sweep = new cSweep(parse);
            }

            public class cMultiplexer : cENA.cSense.cMultiplexer
            {
                public cMultiplexer(FormattedIO488 parse) : base(parse) { }
                public override void Name(int testset_no, string name)
                {
                    // ENA: SendCommand("SENS:MULT" + testset_no.ToString() + ":NAME " + name.ToUpper());
                    // ZVT: Not existing.
                }
                public override void Name(int ChannelNumber, int testset_no, string name)
                {
                    // ENA: SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":NAME " + name.ToUpper());
                    // ZVT: Not existing.
                }
                public override void State(e_OnOff status, int testset_no)
                {
                    // ENA:
                    /*
                    if (status == e_OnOff.On)
{
    SendCommand("SENS:MULT" + testset_no.ToString() + ":STAT ON");
}
if (status == e_OnOff.Off)
{
    SendCommand("SENS:MULT" + testset_no.ToString() + ":STAT OFF");
}
                    */
                    // ZVT: Not existing.
                }
                public override void State(int ChannelNumber, e_OnOff status, int testset_no)
                {
                    /*
                    if (status == e_OnOff.On)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":STAT ON");
                    }
                    if (status == e_OnOff.Off)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":STAT OFF");
                    }
                    */
                    // ZVT: Not existing.
                }
                public override void Display(e_OnOff status, int testset_no)
                {
                    // ENA:
                    /*
                    if (status == e_OnOff.On)
{
    SendCommand("SENS:MULT" + testset_no.ToString() + ":DISP ON");
}
if (status == e_OnOff.Off)
{
    SendCommand("SENS:MULT" + testset_no.ToString() + ":DISP OFF");
}
                    */
                    // ZVT: Not existing.
                }

                public override void Display(int ChannelNumber, e_OnOff status, int testset_no)
                {
                    /*
                    if (status == e_OnOff.On)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":DISP ON");
                    }
                    if (status == e_OnOff.Off)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":DISP OFF");
                    }
                     * */
                    // ZVT: Not existing.
                }
                public override void SetPort(int ChannelNumber, int testset_no, int port_no, string label)
                {
                    // ENA: SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":PORT" + port_no.ToString() + " " + label.ToUpper());
                    // ZVT: Not existing.
                }
                public override void SetCtrl_Voltage(int ChannelNumber, int testset_no, string ctrl, double config)
                {
                    // ENA: SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":OUTP:" + ctrl + ":VOLT " + config.ToString());
                    // ZVT: Not existing.
                }
                public override void SetCtrl_HiLo(int ChannelNumber, int testset_no, string ctrl, double config)
                {
                    // ENA: SendCommand("SENS" + ChannelNumber.ToString() + ":MULT" + testset_no.ToString() + ":OUTP:" + ctrl + ":DATA " + config.ToString());
                    // ZVT: Not existing.
                }
            }
            public class cCorrection : cENA.cSense.cCorrection
            {

                /*
                ZVT
                 * :SENS1:CORR:EXT may not working with ZVT
                */
                public new cCollect Collect;
                public cCorrection(FormattedIO488 parse)
                    : base(parse)
                {
                    base.Collect = new cCollect(parse);
                }
                public class cCollect : cENA.cSense.cCorrection.cCollect
                {
                    public new cAcquire Acquire;
                    public new cECAL ECAL;
                    public new cMethod Method;
                    public new cCalkit Cal_Kit;
                    public new cPortExt PortExt;

                    public cCollect(FormattedIO488 parse)
                        : base(parse)
                    {
                        base.Acquire = new cAcquire(parse);
                        base.ECAL = new cECAL(parse);
                        base.Method = new cMethod(parse);
                        base.Cal_Kit = new cCalkit(parse);
                        base.PortExt = new cPortExt(parse);
                    }

                    public class cPortExt : cENA.cSense.cCorrection.cCollect.cPortExt
                    {
                        public cPortExt(FormattedIO488 parse)
                            : base(parse)
                        {
                        }
                        public override void State(bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    //SendCommand(":SENS1:CORR:EXT ON");
                                    break;
                                case false:
                                    //SendCommand(":SENS1:CORR:EXT OFF");
                                    break;
                            }
                        }
                        public override void State(int ChannelNumber, bool Set)
                        {
                            switch (Set)
                            {
                                case true:
                                    //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT ON");
                                    break;
                                case false:
                                    //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT OFF");
                                    break;
                            }
                        }
                        public override bool State()
                        {
                            return false;
                            //return (common.CStr2Bool(ReadCommand(":SENS1:CORR:EXT?")));
                        }
                        public override bool State(int ChannelNumber)
                        {
                            return false;
                            //return (common.CStr2Bool(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT?")));
                        }
                        public override void Loss1(e_OnOff status, int port, int ChannelNumber)
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
                        public override void Loss1(double loss, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":LOSS1 " + loss.ToString());
                        }
                        public override void Loss2(e_OnOff status, int port, int ChannelNumber)
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
                        public override void Loss2(double loss, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":LOSS2 " + loss.ToString());
                        }
                        public override void Freq1(double freq, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":FREQ1 " + freq.ToString());
                        }
                        public override void Freq2(double freq, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":FREQ2 " + freq.ToString());
                        }
                        public override void Ext(double delay, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + " " + delay.ToString());
                        }
                        public override void LossDC(double loss, int port, int ChannelNumber)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:EXT:PORT" + port.ToString() + ":LDC " + loss.ToString());
                        }
                    }
                    public class cAcquire : cENA.cSense.cCorrection.cCollect.cAcquire
                    {
                        public cAcquire(FormattedIO488 parse) : base(parse) { }
                        public override void Isolation(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL ISOL, " + Port1.ToString() + "," + Port2.ToString()); //ZVT KS - Need to find a proper replacement
                            //[SENSe<Ch>: ]CORRection: COLLect[:ACQuire]: SELected <ISOL>

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void Isolation(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL ISOL, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Load(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL MATCH, " + Port.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void Load(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL MATCH, " + Port.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Open(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL OPEN, " + Port.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM PHAS");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void Open(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL OPEN, " + Port.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM PHAS");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Short(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL SHOR, " + Port.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM PHAS");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void Short(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL SHOR, " + Port.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM PHAS");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Subclass(int Port)
                        {
                            //SendCommand("SENS:CORR:COLL:ACQ:SUBC " + Port.ToString());   //ZVT KS not subclasses available for ZNB
                        }
                        public override void Subclass(int ChannelNumber, int Port)
                        {
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SUBC " + Port.ToString());
                        }
                        public override void Thru(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL THROUGH, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void Thru(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void TRLLine(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL LINE1, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void TRLLine(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL LINE1, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void TRLReflect(int Port)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL REFL, " + Port.ToString());  //ZVT KS no equivalent only applicable for THRU

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void TRLReflect(int ChannelNumber, int Port)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL REFL, " + Port.ToString()); //ZVT KS - [SENSe<Ch>:]CORRection:COLLect[:ACQuire]:SELected

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void TRLThru(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:ACQ:SEL THROUGH, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void TRLThru(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, " + Port1.ToString() + "," + Port2.ToString());

                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                    }
                    public class cECAL : cENA.cSense.cCorrection.cCollect.cECAL
                    {
                        public cECAL(FormattedIO488 parse)
                            : base(parse)
                        {

                        }

                        //ZVT KS to find out the use case for this class //SOLT = TOSM
                        public override void SOLT1(int Port1)
                        {
                            //SendCommand("SENS:CORR:COLL:ECAL:SOLT1 " + Port1.ToString()); //ZVT KS -Not equivalent for ECAL kit, need to find out the ECAL procedures for ZNB
                        }
                        public override void SOLT1(int ChannelNumber, int Port1)
                        {
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT1 " + Port1.ToString());
                        }
                        public override void SOLT2(int Port1, int Port2)
                        {
                            //SendCommand("SENS:CORR:COLL:ECAL:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public override void SOLT2(int ChannelNumber, int Port1, int Port2)
                        {
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT2 " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public override void SOLT3(int Port1, int Port2, int Port3)
                        {
                            //SendCommand("SENS:CORR:COLL:ECAL:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public override void SOLT3(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT3 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public override void SOLT4(int Port1, int Port2, int Port3, int Port4)
                        {
                            //SendCommand("SENS:CORR:COLL:ECAL:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public override void SOLT4(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ECAL:SOLT4 " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                    public class cMethod : cENA.cSense.cCorrection.cCollect.cMethod
                    {
                        public cMethod(FormattedIO488 parse)
                            : base(parse)
                        {

                        }
                        public override void SOLT1(int Port1)
                        {
                            SendCommand("SENS:CORR:COLL:METH:DEF 'SOLT1', TOSM, " + Port1.ToString());
                        }
                        public override void SOLT1(int ChannelNumber, int Port1)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:DEF 'SOLT1', TOSM, " + Port1.ToString());
                        }
                        public override void SOLT2(int Port1, int Port2)
                        {
                            SendCommand("SENS:CORR:COLL:METH:DEF 'SOLT2', TOSM, " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public override void SOLT2(int ChannelNumber, int Port1, int Port2)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:DEF 'SOLT2', TOSM, " + Port1.ToString() + "," + Port2.ToString());
                        }
                        public override void SOLT3(int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS:CORR:COLL:METH:DEF 'SOLT3', TOSM, " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public override void SOLT3(int ChannelNumber, int Port1, int Port2, int Port3)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:DEF 'SOLT3', TOSM, " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString());
                        }
                        public override void SOLT4(int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS:CORR:COLL:METH:DEF 'SOLT4', TOSM, " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                        public override void SOLT4(int ChannelNumber, int Port1, int Port2, int Port3, int Port4)
                        {
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:DEF 'SOLT4', TOSM, " + Port1.ToString() + "," + Port2.ToString() + "," + Port3.ToString() + "," + Port4.ToString());
                        }
                    }
                    public class cCalkit : cENA.cSense.cCorrection.cCollect.cCalkit
                    {
                        int numcalkit = 0;
                        static string labelcalkit = "Avago";
                        public new cCalStd Cal_Std;
                        public cCalkit(FormattedIO488 parse)
                            : base(parse)
                        {
                            base.Cal_Std = new cCalStd(parse);
                        }
                        public class cCalStd : cENA.cSense.cCorrection.cCollect.cCalkit.cCalStd
                        {
                            string stdtype;
                            string stdname;
                            double L0 = 0.0;
                            double L1 = 0.0;
                            double L2 = 0.0;
                            double L3 = 0.0;
                            double C0 = 0.0;
                            double C1 = 0.0;
                            double C2 = 0.0;
                            double C3 = 0.0;
                            double DEL = 0.0;
                            double Z0 = 50.0;
                            double LOSS = 0.0;
                            double FMIN = 0.0;
                            double FMAX = 1.0e12;
                            string[] std_type;

                            public cCalStd(FormattedIO488 parse)
                                : base(parse)
                            {
                                std_type = new string[256];
                            }
                            public string TEXT(string s)
                            {
                                s = s.Replace(',', '.');
                                return s;
                            }
                            public override void Std_Label(int ChannelNumber, int stdno, string name)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":LAB \"" + name + "\"");
                                // ZVT: Not existing;
                            }
                            public override void Std_Type(int ChannelNumber, int stdno, string name)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":TYPE " + name);
                                // ZVT:
                                //CORR:CKIT:N50:FOPEN 'NewKit1','',0,1.8E+010,0.0151,0,0,0.22,-0.22,0.0022
                                std_type[stdno] = name.ToUpper();
                                L0 = L1 = L2 = L3 = C0 = C1 = C2 = C3 = DEL = LOSS = 0.0;
                                Z0 = 50.0;
                                FMIN = 1.0e3;
                                FMAX = 1.0e12;
                                SendCommand("*CLS");
                            }
                            void WriteStandards(int ChannelNumber, int stdno)
                            {
                                switch (std_type[stdno])
                                {
                                    case "OPEN":
                                        WriteSingleStandard(ChannelNumber, "FOPEN");
                                        WriteSingleStandard(ChannelNumber, "MOPEN");
                                        break;
                                    case "SHORT":
                                        WriteSingleStandard(ChannelNumber, "FSHORT");
                                        WriteSingleStandard(ChannelNumber, "MSHORT");
                                        break;
                                    case "LOAD":
                                        WriteSingleStandard(ChannelNumber, "MMTCh");
                                        WriteSingleStandard(ChannelNumber, "FMTCh");
                                        break;
                                    case "THRU":
                                        WriteSingleStandard(ChannelNumber, "MMTHrough");
                                        WriteSingleStandard(ChannelNumber, "MFTHrough");
                                        WriteSingleStandard(ChannelNumber, "FFTHrough");
                                        break;
                                }
                            }
                            // FY20120913: Build the command string for the standard from parameters.
                            void WriteSingleStandard(int ChannelNumber, string r_s_standard)
                            {
                                string cmd = "";
                                cmd = cmd + ":SENS:CORR:CKIT:";
                                cmd = cmd + r_s_standard;
                                cmd = cmd + " 'Package','" + labelcalkit + "', ''";
                                cmd = cmd + "," + TEXT(FMIN.ToString());
                                cmd = cmd + "," + TEXT(FMAX.ToString());
                                cmd = cmd + "," + TEXT(DEL.ToString());
                                cmd = cmd + "," + TEXT(LOSS.ToString());
                                cmd = cmd + "," + TEXT(Z0.ToString());
                                if (!r_s_standard.Contains("THrough"))
                                {
                                    cmd = cmd + "," + TEXT(C0.ToString());
                                    cmd = cmd + "," + TEXT(C1.ToString());
                                    cmd = cmd + "," + TEXT(C2.ToString());
                                    cmd = cmd + "," + TEXT(C3.ToString());
                                    cmd = cmd + "," + TEXT(L0.ToString());
                                    cmd = cmd + "," + TEXT(L1.ToString());
                                    cmd = cmd + "," + TEXT(L2.ToString());
                                    cmd = cmd + "," + TEXT(L3.ToString());
                                }
                                /*
                                if (!r_s_standard.Contains("MTCh"))
                                {
                                    if (C0 != 0.0 || C1 != 0.0 || C2 != 0.0 || C3 != 0.0)
                                    {
                                        cmd = cmd + "," + TEXT(DEL.ToString());
                                        cmd = cmd + "," + TEXT(LOSS.ToString());
                                        if (!r_s_standard.Contains("THrough"))
                                        {
                                            cmd = cmd + "," + TEXT(C0.ToString());
                                            cmd = cmd + "," + TEXT(C1.ToString());
                                            cmd = cmd + "," + TEXT(C2.ToString());
                                            cmd = cmd + "," + TEXT(C3.ToString());
                                        }
                                    }
                                    else
                                    {
                                        cmd = cmd + "," + TEXT(DEL.ToString());
                                        cmd = cmd + "," + TEXT(LOSS.ToString());
                                        if (!r_s_standard.Contains("THrough"))
                                        {
                                            cmd = cmd + "," + TEXT(L0.ToString());
                                            cmd = cmd + "," + TEXT(L1.ToString());
                                            cmd = cmd + "," + TEXT(L2.ToString());
                                            cmd = cmd + "," + TEXT(L3.ToString());
                                        }
                                    }
                                }
                                 * */
                                SendCommand(cmd);
                                string rc = ReadCommand(":SYST:ERR?");
                                rc = rc.ToUpper();
                                if (!rc.Contains("NO ERROR"))
                                {
                                    string s = cmd;
                                }
                            }
                            public override void Std_C0(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C0 " + value.ToString());
                                // ZVT:
                                C0 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_C1(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C1 " + value.ToString());
                                // ZVT:
                                C1 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_C2(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C2 " + value.ToString());
                                // ZVT:
                                C2 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_C3(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":C3 " + value.ToString());
                                // ZVT:
                                C3 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_L0(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L0 " + value.ToString());
                                // ZVT:
                                L0 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_L1(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L1 " + value.ToString());
                                // ZVT:
                                L1 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_L2(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L2 " + value.ToString());
                                // ZVT:
                                L2 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Std_L3(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":L3 " + value.ToString());
                                // ZVT:
                                L3 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void OffSet_Delay(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":DEL " + value.ToString());
                                // ZVT:
                                DEL = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Offset_Z0(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":Z0 " + value.ToString());
                                // ZVT:
                                Z0 = value;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Offset_Loss(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":LOSS " + value.ToString());
                                // ZVT:
                                LOSS = value / 1000000000;
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void ArbImp(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":ARB " + value.ToString());
                                // ZVT: Not existing.
                            }
                            public override void MinFreq(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":FMIN " + value.ToString());
                                // ZVT:
                                FMIN = value;
                                if (FMAX < FMIN)
                                {
                                    FMAX = FMIN;
                                }
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void MaxFreq(int ChannelNumber, int stdno, double value)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":FMAX " + value.ToString());
                                // ZVT:
                                FMAX = value;
                                if (FMAX < FMIN)
                                {
                                    FMIN = FMAX;
                                }
                                WriteStandards(ChannelNumber, stdno);
                            }
                            public override void Media(int ChannelNumber, int stdno, string name)
                            {
                                // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:STAN" + stdno.ToString() + ":CHAR " + name);
                                // ZVT: Not existing.
                            }
                        }
                        public override void Cal_Kit(int Number_CalKit)
                        {
                            //SendCommand(":SENS1:CORR:COLL:CKIT " + Number_CalKit.ToString());
                            Cal_Kit(1, Number_CalKit);
                        }
                        public override void Cal_Kit(int ChannelNumber, int Number_CalKit)
                        {
                            // ENA SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT " + Number_CalKit.ToString());
                            // ZVT: For now we select a fixed calkit as the ZVT always refers to the calkits by name and not by number.
                            //      We save the number in case a query is done.
                            numcalkit = Number_CalKit;
                        }
                        public override int Cal_Kit()
                        {
                            // ENA: return(int.Parse(ReadCommand(":SENS1:CORR:COLL:CKIT?")));
                            // ZVT:
                            return numcalkit;
                        }
                        //public override int Cal_Kit(int ChannelNumber)
                        //{
                        //    return(int.Parse(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT?")));
                        //}
                        public override void Label(string name)
                        {
                            SendCommand("SENS:CORR:COLL:CONN:PORT ALL");
                            SendCommand("SENS:CORR:COLL:SCON 'Package'");
                            SendCommand("SENSe:CORRection:CKIT:DMODe 'Package', '" + name + "', '" + name + "', DEL");
                            //SendCommand("SENS:CORR:CKIT:SEL 'Package', '" + name + "'"); //Temporary select calkit without calkit creation
                            // ENA: SendCommand(":SENS1:CORR:COLL:CKIT:LAB \"" + name + "\"");
                            // ZVT:
                            Label(1, name);
                        }
                        public override void Label(int ChannelNumber, string name)
                        {
                            //SendCommand("SENS:CORR:COLL:CONN:PORT ALL");
                            //SendCommand("SENS:CORR:COLL:SCON 'Package'");
                            //SendCommand("SENSe:CORRection:CKIT:DMODe 'Package', '" + name + "', '" + name + "', DEL");
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:CKIT:SEL 'Package', '" + name + "'"); //Temporary select calkit without calkit creation
                            //Dummy Save First
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:RSAV:DEF ON");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:METH:DEF 'SOLT3', TOSM, 1, 2, 3");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL OPEN, 1;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL OPEN, 2;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL OPEN, 3;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL CLOSE, 1;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL CLOSE, 2;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL CLOSE, 3;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL MATCH, 1;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL MATCH, 2;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL MATCH, 3;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, 1,2;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, 1,3;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, 2,3;*OPC?");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORRection:COLLect:SAVE:SELected;*OPC?");

                            // ENA: SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:LAB \"" + name + "\"");
                            // ZVT:
                            labelcalkit = name;
                        }

                        public override string Label()
                        {
                            // ZVT:
                            string label;
                            label = ReadCommand(":SENS:CORR:COLL:CKIT:PORT?");
                            string[] labelArr;
                            try
                            {
                                labelArr = label.Split(',');
                                return labelArr[1].Substring(1, labelArr[1].Length - 2);
                            }
                            catch (Exception ex)
                            {
                                return string.Empty;
                            }
                        }
                        public override string Label(int ChannelNumber)
                        {
                            // ZVT:
                            string label;
                            label = ReadCommand(":SENS:CORR:COLL:CKIT:PORT?");
                            string[] labelArr;
                            try
                            {
                                labelArr = label.Split(',');
                                return labelArr[1].Substring(1, labelArr[1].Length - 2);
                            }
                            catch (Exception ex)
                            {
                                return string.Empty;
                            }
                        }
                        public override void Order(int SubClass)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT " + SubClass.ToString());
                        }
                        public override void Order(int ChannelNumber, int SubClass)
                        {
                            //ZNB not available
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT " + SubClass.ToString());
                        }
                        public override int Order()
                        {
                            //ZNB not available
                            //return (int.Parse(ReadCommand(":SENS1:CORR:COLL:CKIT?")));
                            return 0;
                        }
                        //public override int Order(int ChannelNumber)
                        //{
                        //    return(int.Parse(ReadCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT?")));
                        //}
                        public override void Select_SubClass(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            //ZNB Not equivalent
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:SUBC " + ChannelNumber.ToString());
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:SEL " + ChannelNumber.ToString());
                        }
                        public override void Open(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:ACQ:SEL OPEN, " + Port_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM PHAS");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void SubClass_Open(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            //ZNB Not equivalent
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:OPEN " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public override void Open(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL OPEN, " + Port_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM PHAS");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Short(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:ACQ:SEL SHOR, " + Port_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM PHAS");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void SubClass_Short(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            //ZNB Not equivalent
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:SHOR " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public override void Short(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL SHOR, " + Port_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM PHAS");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Load(int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:ACQ:SEL MATCH, " + Port_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void SubClass_Load(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            //ZNB Not equivalent
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:LOAD " + Port_Number.ToString() + "," + Standard_Number.ToString());
                        }
                        public override void Load(int ChannelNumber, int Port_Number, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL MATCH, " + Port_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void Thru(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:ACQ:SEL THROUGH, " + Port_Number_1.ToString() + "," + Port_Number_2.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void SubClass_Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            //ZNB Not equivalent
                            //SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:CKIT:ORD:THRU " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                        }
                        public override void Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, " + Port_Number_1.ToString() + "," + Port_Number_2.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void TRL_Line(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLL " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void TRL_Line(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL LINE1, " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void TRL_Reflect(int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:ACQ:SEL REFL, " + Standard_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void TRL_Reflect(int ChannelNumber, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL REFL, " + Standard_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                        public override void TRL_Thru(int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS1:CORR:COLL:CKIT:ORD:TRLT " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC:PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM SMIT");

                            SendCommand("CALC:PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC:FORM MLOG");

                            SendCommand("CALC:MARK ON");
                        }
                        public override void TRL_Thru(int ChannelNumber, int Port_Number_1, int Port_Number_2, int Standard_Number)
                        {
                            SendCommand(":SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:SEL THROUGH, " + Port_Number_1.ToString() + "," + Port_Number_2.ToString() + "," + Standard_Number.ToString());
                            Thread.Sleep(1000);
                            //Enhanced Calibration display
                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc1'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM SMIT");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":PAR:SEL 'Trc2'");
                            Thread.Sleep(200);
                            SendCommand("CALC" + ChannelNumber.ToString() + ":FORM MLOG");

                            SendCommand("CALC" + ChannelNumber.ToString() + ":MARK ON");
                        }
                    }
                    public override void Save(int ChannelNumber)
                    {
                        SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:SAVE:SEL");
                    }
                }
                public override void Property(bool enable)
                {
                    switch (enable)
                    {
                        case true:
                            SendCommand("SENS:CORR:COLL:ACQ:RSAV:DEF ON");
                            //SendCommand("SENS:CORR:PROP ON");
                            break;
                        case false:
                            SendCommand("SENS:CORR:COLL:ACQ:RSAV:DEF OFF");
                            //SendCommand("SENS:CORR:PROP OFF");
                            break;
                    }
                }
                public override void Property(int ChannelNumber, bool enable)
                {
                    switch (enable)
                    {
                        case true:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:RSAV:DEF ON");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:PROP ON");
                            break;
                        case false:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:COLL:ACQ:RSAV:DEF OFF");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:PROP OFF");
                            break;
                    }
                }
                public override void Clear(int ChannelNumber)
                {
                    //SendCommand("SENS" + ChannelNumber.ToString() + ":CORR:CLE");
                }
                public override void State(bool enable)
                {
                    switch (enable)
                    {
                        case true:
                            //SendCommand("SENS:CORR:PROP ON");
                            break;
                        case false:
                            //SendCommand("SENS:CORR:PROP OFF");
                            break;
                    }
                }
            }
            public class cFrequency : cENA.cSense.cFrequency
            {
                public cFrequency(FormattedIO488 parse) : base(parse) { }
                public override void Center(double Freq)
                {
                    SendCommand("SENS:FREQ:CENT " + Freq.ToString());
                }
                public override void Center(string Freq)
                {
                    SendCommand("SENS:FREQ:CENT " + common.convertStr2Val(Freq));
                }
                public override void CW(double Freq)
                {
                    SendCommand("SENS:FREQ:CW " + Freq.ToString());
                }
                public override void Fixed(double Freq)
                {
                    SendCommand("SENS:FREQ:FIX " + Freq.ToString());
                }
                public override void SPAN(double BW)
                {
                    SendCommand("SENS:FREQ:SPAN " + BW.ToString());
                }
                public override void Start(double Freq)
                {
                    SendCommand("SENS:FREQ:STAR " + Freq.ToString());
                }
                public override void Start(int ChannelNumber, double Freq)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STAR " + Freq.ToString());
                }
                public override double Start()
                {
                    return (Convert.ToDouble(ReadCommand("SENS:FREQ:STAR?")));
                }
                public override double Start(int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STAR?")));
                }
                public override void Stop(double Freq)
                {
                    SendCommand("SENS:FREQ:STOP " + Freq.ToString());
                }
                public override void Stop(int ChannelNumber, double Freq)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STOP " + Freq.ToString());
                }
                public override double Stop()
                {
                    return (Convert.ToDouble(ReadCommand("SENS:FREQ:STOP?")));
                }
                public override double Stop(int ChannelNumber)
                {
                    return (Convert.ToDouble(ReadCommand("SENS" + ChannelNumber.ToString() + ":FREQ:STOP?")));
                }
                public override double[] FreqList()
                {
                    // ENA: return (ReadIEEEBlock("SENS:FREQ:DATA?"));
                    // ZVT:
                    return FreqList(1);
                }
                public override double[] FreqList(int ChannelNumber)
                {
                    // ENA: return (ReadIEEEBlock("SENS" + ChannelNumber.ToString() + ":FREQ:DATA?"));
                    // ZVT:
                    return (ReadIEEEBlock("CALC" + ChannelNumber.ToString() + ":DATA:STIM?"));
                }
                public override void Band(int ChannelNumber, double BW)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":BAND " + BW.ToString());
                }
            }
            public class cSegment : cENA.cSense.cSegment
            {

                public new e_ModeSetting Mode;
                public new e_OnOff Ifbw;
                public new e_OnOff Pow;
                public new e_OnOff Del;
                public new e_OnOff Time;

                public cSegment(FormattedIO488 parse) : base(parse) { }
                public override void Data(string SegmentData, e_OnOff sweepmode)
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
                public override void Data(int ChannelNumber, string SegmentData, e_OnOff sweepmode)
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
                public override void Data(s_SegmentTable SegmentData, e_OnOff sweepmode)
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
                public override void Data(int ChannelNumber, s_SegmentTable SegmentData, e_OnOff sweepmode)
                {
                    //Assign value
                    Mode = SegmentData.mode;
                    Ifbw = SegmentData.ifbw;
                    Pow = SegmentData.pow;
                    Del = SegmentData.del;
                    Time = SegmentData.time;

                    //Delete all segment first
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DEL:ALL");
                    for (int Seg = 0; Seg < SegmentData.SegmentData.Length; Seg++)
                    {
                        //[SENSe<Ch>:]SEGMent<Seg>:INSert <StartFreq>, <StopFreq>, <Points>, <Power>,
                        //<SegmentTime>|<MeasDelay>, <Unused>, <MeasBandwidth>[, <LO>,
                        //<Selectivity>]
                        SendCommand(String.Format("SENS{0}:SEGM:INS {1}, {2}, {3}, {4}, {5}, 0, {6}",
                            ChannelNumber.ToString(), SegmentData.SegmentData[Seg].Start.ToString(),
                            SegmentData.SegmentData[Seg].Stop.ToString(), SegmentData.SegmentData[Seg].Points.ToString(),
                            SegmentData.SegmentData[Seg].pow_value.ToString(), SegmentData.SegmentData[Seg].time_value.ToString(),
                            SegmentData.SegmentData[Seg].ifbw_value.ToString()));
                    }

                    switch (sweepmode)
                    {
                        case e_OnOff.On:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:STAT ON");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 6," + Convert_SegmentTable2String(SegmentData, sweepmode));
                            break;
                        case e_OnOff.Off:
                            SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:STAT OFF");
                            //SendCommand("SENS" + ChannelNumber.ToString() + ":SEGM:DATA 5," + Convert_SegmentTable2String(SegmentData, sweepmode));
                            break;
                    }

                }


                public override s_SegmentTable Data(int ChannelNumber)
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
                        ST.SegmentData[i].time_value = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:TIME?", ChannelNumber.ToString(), (i + 1).ToString())));
                        ST.SegmentData[i].del_value = double.Parse(ReadCommand(String.Format("SENS{0}:SEGM{1}:SWE:DWEL?", ChannelNumber.ToString(), (i + 1).ToString())));
                    }

                    SendCommand("FORM:DATA " + DataFormat);

                    return (ST);
                }
                public override string SweepPoints()
                {
                    return (ReadCommand("SENS:SEGM:SWE:POIN?"));
                }
                public override string SweepPoints(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:SWE:POIN?"));
                }
                public override string SweepTime()
                {
                    return (ReadCommand("SENS:SEGM:SWE:TIME?"));
                }
                public override string SweepTime(int ChannelNumber)
                {
                    return (ReadCommand("SENS" + ChannelNumber.ToString() + ":SEGM:SWE:TIME?"));
                }
            }
            public class cSweep : cENA.cSense.cSweep
            {
                public new cSweepTime Time;
                public cSweep(FormattedIO488 parse)
                    : base(parse)
                {
                    base.Time = new cSweepTime(parse);
                }
                public override void ASPurious(e_OnOff State)
                {
                    //ZNB not available
                    //SendCommand("SENS:SWE:ASP " + State.ToString());
                }
                public override void Delay(double delay)
                {
                    SendCommand("SENS:SWE:DWEL " + delay.ToString());
                }
                public override void Generation(e_SweepGeneration SweepGen)
                {
                    //ZNB not available
                    // SendCommand("SENS:SWE:GEN " + SweepGen.ToString());
                }
                //public override void Points(int points)
                //{
                //    SendCommand("SENS:SWE:POIN " + points.ToString());
                //}
                public override void Points(int ChannelNumber, int points)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:POIN " + points.ToString());
                }
                public override int Points()
                {
                    return (Convert.ToInt32(ReadCommand("SENS:SWE:POIN?")));
                }
                public override int Points(int ChannelNumber)
                {
                    return (Convert.ToInt32(ReadCommand("SENS" + ChannelNumber.ToString() + ":SWE:POIN?")));
                }
                public class cSweepTime : cENA.cSense.cSweep.cSweepTime
                {
                    public cSweepTime(FormattedIO488 parse) : base(parse) { }
                    public override void Auto(e_OnOff state)
                    {
                        SendCommand("SENS:SWE:TIME:AUTO " + state.ToString());
                    }
                    public override void Data(double time)
                    {
                        SendCommand("SENS:SWE:TIME " + time.ToString());
                    }
                }
                public override void Type(e_SweepType SweepType)
                {
                    SendCommand("SENS:SWE:TYPE " + SweepType.ToString());
                }
                public override void Type(int ChannelNumber, e_SweepType SweepType)
                {
                    SendCommand("SENS" + ChannelNumber.ToString() + ":SWE:TYPE " + SweepType.ToString());
                }
                public override e_SweepType Type()
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS:SWE:TYPE?");
                    return ((e_SweepType)Enum.Parse(typeof(e_SweepType), tmpStr.ToUpper()));
                }
                public override e_SweepType Type(int ChannelNumber)
                {
                    string tmpStr;
                    tmpStr = ReadCommand("SENS" + ChannelNumber + ":SWE:TYPE?");
                    return ((e_SweepType)Enum.Parse(typeof(e_SweepType), tmpStr.ToUpper()));
                }
            }
        }
        public class cTrigger : cENA.cTrigger
        {
            public new cTriggerExternal External;
            public cTrigger(FormattedIO488 parse)
                : base(parse)
            {
                base.External = new cTriggerExternal(parse);
            }
            public override void Average(e_OnOff State)
            {
                SendCommand("TRIG:AVER " + State.ToString());
            }
            public class cTriggerExternal : cENA.cTrigger.cTriggerExternal
            {
                public cTriggerExternal(FormattedIO488 parse) : base(parse) { }
                public override void Delay(double delay)
                {
                    SendCommand("TRIG:EXT:DEL " + delay.ToString());
                }
                public override void LLatency(e_OnOff state)
                {
                    SendCommand("TRIG:EXT:LLAT " + state.ToString());
                }
            }
            public override void Immediate()
            {
                SendCommand("TRIG:SEQ:IMM");
            }
            public override void Point(e_OnOff state)
            {
                SendCommand("TRIG:SEQ:POIN " + state.ToString());
            }
            public override void Single()
            {
                // ENA: SendCommand("TRIG:SEQ:SING");
                // ZVT: Not exisiting.
            }
            public override void Single(int channel)
            {
                // ENA: SendCommand("TRIG:SEQ:SING");
                // ZVT: Not exisiting.
            }
            public override void Scope(e_TriggerScope Scope)
            {
                // ENA: SendCommand("TRIG:SEQ:SCOP " + Scope.ToString());
                // ZVT:
                if (Scope == e_TriggerScope.ACT)
                {
                    SendCommand("INIT:SCOP SING");
                }
                else
                {
                    SendCommand("INIT:SCOP ALL");
                }
            }
            public override void Source(e_TriggerSource Source)
            {
                // ENA: SendCommand("TRIG:SEQ:SOUR " + Source.ToString());
                // ZVT:	
                string triggersource;
                switch (Source)
                {
                    case e_TriggerSource.BUS:
                        triggersource = "IMM";
                        break;
                    case e_TriggerSource.EXT:
                        triggersource = "EXT";
                        break;
                    case e_TriggerSource.INT:
                        triggersource = "IMM";
                        break;
                    case e_TriggerSource.MAN:
                        triggersource = "MAN";
                        break;
                    default:
                        triggersource = "IMM";
                        break;
                }
                SendCommand("TRIG:SEQ:SOUR " + triggersource);
            }
        }


        #endregion
    }
}
