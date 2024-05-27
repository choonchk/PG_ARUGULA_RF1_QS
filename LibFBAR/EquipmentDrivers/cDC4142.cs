using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR.DC
{
    public class cDC4142
    {
        #region "Enumeration Declaration"

        #endregion
        #region "Structure"

        #endregion
        #region "Conversion Function"

        #endregion

        public static string ClassName = "Power Supply DC4142 Class";
        private string IOAddress;
        private FormattedIO488 ioDC4142 = new FormattedIO488();
        private LibFBAR.cGeneral common = new LibFBAR.cGeneral();


        #region "Class Initialization"
        public cCommonFunctionA BasicCommand; // Basic Command for General Equipment
        public clsSystem System;                // System Command
        public clsOutput Output;                // Output Command
        public clsMeasurement Measurement;      // Measurement Command
        public clsCalibration Calibration;      // Calibration Command
        public clsMemory Memory;                // Memory Command
        public clsSearch Search;                // Search Command
        public clsTrigger Trigger;              // Trigger Command

        public void Init(FormattedIO488 IOInit)
        {
            System = new clsSystem(IOInit);
            Output = new clsOutput(IOInit);
            Measurement = new clsMeasurement(IOInit);
            Calibration = new clsCalibration(IOInit);
            Memory = new clsMemory(IOInit);
            Search = new clsSearch(IOInit);
            Trigger = new clsTrigger(IOInit);
            BasicCommand = new cCommonFunctionA(IOInit);
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
                return ioDC4142;
            }
            set
            {
                ioDC4142 = parseIO;
                Init(parseIO);
            }
        }
        public void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    //ioDC4142 = new FormattedIO488();
                    ioDC4142.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioDC4142.IO = null;
                    return;
                }
                Init(ioDC4142);
            }
        }
        public void CloseIO()
        {
            ioDC4142.IO.Close();
        }

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  18/10/2010       KKL             VISA Driver for DC4142

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return ("DC4142 Class Version = v" + VersionStr);
        }

        #region "Class Functional Codes"
        public class clsSystem
        {
            private FormattedIO488 ioDC4142;
            public clsSystem(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            public string GetUnitModule()
            {
                ioDC4142.WriteString("UNT?", true);
                return (ioDC4142.ReadString());
            }
        }
        public class clsOutput
        {
            private FormattedIO488 ioDC4142;
            public clsOutput(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            private void SendCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
            }
            private string ReadCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
                ioDC4142.FlushRead();
                return (ioDC4142.ReadString());
            }
            public void ON(int ChannelNumber)
            {
                //Enable the specified units by setting the output switches to ON
                SendCommand("CN " + ChannelNumber.ToString());
            }
            public void ON(string ChannelNumber)
            {
                //Enable the specified units by setting the output switches to ON
                SendCommand("CN " + ChannelNumber);
            }
            public void OFF(int ChannelNumber)
            {
                //'Disables the specified units by setting the output switches to OFF
                SendCommand("CL " + ChannelNumber.ToString());
            }
            public void OFF(string ChannelNumber)
            {
                //'Disables the specified units by setting the output switches to OFF
                SendCommand("CL " + ChannelNumber);
            }
            public void Set_Voltage(int ChannelNumber, int OutputRange, double OutputVoltage, double ICompliance)
            {
                //Forces output voltage from the specified unit
                SendCommand("DV " + ChannelNumber.ToString() + "," + OutputRange.ToString() + "," + OutputVoltage.ToString() + "," + ICompliance.ToString());
            }
            public void Set_Voltage(int ChannelNumber, string OutputRange, string OutputVoltage, string ICompliance)
            {
                //Forces output voltage from the specified unit
                SendCommand("DV " + ChannelNumber.ToString() + "," + OutputRange + "," + OutputVoltage + "," + ICompliance);
            }
            public void Set_Current(int ChannelNumber, int OutputRange, float OutputCurrent, float VCompliance)
            {
                //Forces output current from the specified unit
                SendCommand("DI " + ChannelNumber.ToString() + "," + OutputRange.ToString() + "," + OutputCurrent.ToString() + "," + VCompliance.ToString());
            }
            public void Set_Current(int ChannelNumber, string OutputRange, string OutputCurrent, string VCompliance)
            {
                //Forces output current from the specified unit
                SendCommand("DI " + ChannelNumber.ToString() + "," + OutputRange + "," + OutputCurrent + "," + VCompliance);
            }
            public void Zero_Output(int ChannelNumber)
            {
                //Sets the specified units to Zero Output
                SendCommand("DZ " + ChannelNumber);
            }
            public void I_Sense(int ChannelNumber, float OutputVoltage, float TargetCurrent, float ICompliance)
            {
                //Specifies I sense SMU
                SendCommand("AVI " + ChannelNumber.ToString() + "," + OutputVoltage.ToString() + "," + ICompliance.ToString());
            }
            public void I_Sense(int ChannelNumber, string OutputVoltage, string TargetCurrent, string ICompliance)
            {
                //Specifies I sense SMU
                SendCommand("AVI " + ChannelNumber + "," + OutputVoltage + "," + ICompliance);
            }
            public void Hold_Delay_Time(double Hold_Time, double Delay_Time)
            {
                //Sets the hold time and delay time
                SendCommand("AT " + Hold_Time.ToString() + "," + Delay_Time.ToString());
            }
        }
        public class clsMeasurement
        {
            private FormattedIO488 ioDC4142;
            string tmp;
            public clsMeasurement(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            private void SendCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
            }
            private string ReadCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
                ioDC4142.FlushRead();
                tmp = ioDC4142.ReadString();
                return (tmp);
            }
            public string IMeasurement(int ChannelNumber, int Measurement_Range)
            {
                //Trigger command for high speed spot I measurement
                return (ReadCommand("TI " + ChannelNumber.ToString() + "," + Measurement_Range.ToString()).Substring(3).Trim());
            }
            public string IMeasurement(int ChannelNumber)
            {
                //Trigger command for high speed spot I measurement
                return (ReadCommand("TI " + ChannelNumber.ToString() + ",0").Substring(3).Trim());
            }
            public string IMeasurement_Average(int ChannelNumber, int Measurement_Range, int Average)
            {
                //This is to cater for Current measurement fluctuation problem
                //Trigger command for high speed spot I measurement
                SendCommand("BDM 1,1");    //Detection Interval
                SendCommand("AV " + Average.ToString());  //Setting Average
                return (ReadCommand("TI " + ChannelNumber.ToString() + "," + Measurement_Range.ToString()).Substring(3).Trim());
            }
            public string IMeasurement_Average(int ChannelNumber, int Average)
            {
                //This is to cater for Current measurement fluctuation problem
                //Trigger command for high speed spot I measurement
                SendCommand("BDM 1,1");    //Detection Interval
                SendCommand("AV " + Average.ToString());  //Setting Average
                return (ReadCommand("TI " + ChannelNumber.ToString() + ",0").Substring(3).Trim());
            }
            public string VMeasurement(int ChannelNumber, int Measurement_Range)
            {
                //Trigger command for high speed spot V measurement
                return (ReadCommand("TV " + ChannelNumber.ToString() + "," + Measurement_Range.ToString()).Substring(3).Trim());
            }
            public string VMeasurement(int ChannelNumber)
            {
                //Trigger command for high speed spot V measurement
                return (ReadCommand("TV " + ChannelNumber.ToString() + ",0").Substring(3).Trim());
            }
            public void Measurement_Mode_Unit(int ChannelNumber)
            {
                SendCommand("MM " + ChannelNumber.ToString());
            }
        }
        public class clsCalibration
        {
            private FormattedIO488 ioDC4142;
            public clsCalibration(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            private void SendCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
            }
            public void Cal_All()
            {
                //Enable the Calibration to all Slots
                SendCommand("CA");
            }
            public void Cal_Channel(int ChannelNumber)
            {
                //Enable the Calibration to Specific Slots
                SendCommand("CA " + ChannelNumber.ToString());
            }
        }
        public class clsMemory
        {
            private FormattedIO488 ioDC4142;
            public clsMemory(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            private void SendCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
            }
            private string ReadCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
                ioDC4142.FlushRead();
                return (ioDC4142.ReadString());
            }
            public string Execute_Memory(string Start_Program, string Stop_Program)
            {
                //Executes the 4142 internal memory program
                return (ReadCommand("RU " + Start_Program.Trim() + "," + Stop_Program.Trim()));
            }
            public void Store(string ProgramMemory)
            {
                //Store a program in the internal program memory of the 4142
                SendCommand("ST " + ProgramMemory.Trim());
            }
            public void Program_Memory()
            {
                //Used with the ST command to store a program in the internal program memory of 4142
                SendCommand("END");
            }
        }
        public class clsSearch
        {
            private FormattedIO488 ioDC4142;
            public clsSearch(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            private void SendCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
            }
            private string ReadCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
                ioDC4142.FlushRead();
                return (ioDC4142.ReadString());
            }
            public void Search_voltage(int ChannelNumber, string Start_voltage, string Stop_voltage, string Ramp_rate, string I_compliance)
            {
                //Specifies the search SMU
                SendCommand("ASV " + ChannelNumber.ToString() + "," + Start_voltage.Trim() + "," + Stop_voltage.Trim() + "," + Ramp_rate.Trim() + "," + I_compliance.Trim());
            }
            public void Search_voltage(int ChannelNumber, float Start_voltage, float Stop_voltage, float Ramp_rate, float I_compliance)
            {
                //Specifies the search SMU
                SendCommand("ASV " + ChannelNumber.ToString() + "," + Start_voltage.ToString() + "," + Stop_voltage.ToString() + "," + Ramp_rate.ToString() + "," + I_compliance.ToString());
            }
            public void Search_operation_measurement(string Operation_mode, string Measurement_mode, string Feedback_integration_time)
            {
                //Sets the search operation mode, search measurement mode and feedback integration time
                SendCommand("ASM " + (Operation_mode.Trim() + "," + Measurement_mode.Trim() + "," + Feedback_integration_time.Trim()));
            }
            public void Search_operation_measurement(string Operation_mode, int Measurement_mode, float Feedback_integration_time)
            {
                //Sets the search operation mode, search measurement mode and feedback integration time
                SendCommand("ASM " + (Operation_mode.Trim() + "," + Measurement_mode.ToString() + "," + Feedback_integration_time.ToString()));
            }
        }
        public class clsTrigger
        {
            private FormattedIO488 ioDC4142;
            public clsTrigger(FormattedIO488 parse)
            {
                ioDC4142 = parse;
            }
            private void SendCommand(string cmd)
            {
                ioDC4142.WriteString(cmd, false);
                ioDC4142.FlushWrite(true);
            }
            public void trigger()
            {
                //Triggers the 4142 to perform measurements, except for high speed spot measurements
                SendCommand("XE");
            }
        }

        public class cCommonFunctionA
        {
            FormattedIO488 IO = new FormattedIO488();
            public cCommonFunctionA(FormattedIO488 parse)
            {
                IO = parse;
            }
            public string DeviceName()
            {
                IO.WriteString("*IDN?", true);
                return (IO.ReadString());
            }
            public void SendCommand(string cmd)
            {
                IO.WriteString(cmd, false);
                IO.FlushWrite(true);
            }
            public string ReadCommand(string cmd)
            {
                IO.WriteString(cmd, false);
                IO.FlushWrite(false);
                IO.FlushRead();
                return (IO.ReadString());
            }
            public void DeviceClear()
            {
                IO.IO.Clear();
            }

            public string Version()
            {
                string VersionStr;
                //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
                //                          //-----------------  ----------- ----------------------------------------------------------------------------------
                VersionStr = "0.01";        //  18/10/2010       KKL             New Coding Version

                //                          //-----------------  ----------- ----------------------------------------------------------------------------------
                return ("Common Code A Version = v" + VersionStr);
            }
        }
        #endregion

    }
}
