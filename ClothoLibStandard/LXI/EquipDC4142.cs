using System;
using System.Threading;
using Ivi.Visa.Interop;

namespace ClothoLibStandard 
{
    public class EquipDC4142 : ClothoLibStandard.Lib_Var
    {
        FormattedIO488 myVisaDC4142 = null;


        public EquipDC4142(FormattedIO488 _myVisaDC4142) 
        {
            myVisaDC4142 = _myVisaDC4142;
        }

        ~EquipDC4142() { }

        public void CAL()
        {
            //myVisaDC4142.WriteString("*CLS", true);
            myVisaDC4142.WriteString("CA", true);

        }
        public void RESET()
        {
            try
            {
                myVisaDC4142.WriteString("*RST", true);
            }
            catch (Exception ex)
            {
                //throw new Exception("EquipPS: RESET -> " + ex.Message);
            }
        }

        public void PRESET()
        {
            myVisaDC4142.WriteString("SYST:PRES", true);
        }

        public void Cal_channel(string ch)
        {
            myVisaDC4142.WriteString("CA " + ch, true);
        }

        public void Search_votlage(string ch, string Start_voltage, string Stop_voltage, double  Ramp_rate, double  I_compliance)
        {

            myVisaDC4142.WriteString("ASV " + ch + "," + Start_voltage + "," + Stop_voltage + "," + Ramp_rate + "," + I_compliance, true);
        }

 
        public void I_sense(string ch, string Output_voltage, string target_current, double I_compliance)
        {

            myVisaDC4142.WriteString("AVI " + ch + "," + Output_voltage + "," + target_current + "," + I_compliance, true);
        }


        public void Hold_delay_time(string Hold_time, string delay_time)
        {
            myVisaDC4142.WriteString("AT " + Hold_time + "," + delay_time, true);
        }

        public void Search_operation_measurement(string Operation_mode, string Measurement_mode, string Feedback_integration_time)
        {
            myVisaDC4142.WriteString("ASM " + Operation_mode  + "," + Measurement_mode + "," + Feedback_integration_time , true);
        }

        public void Measurement_mode_unit(string ch)
        {
            myVisaDC4142.WriteString("MM " + ch , true);
        }

        public void Trigger()
        {
            myVisaDC4142.WriteString("XE", true);
        }

        public void Zero_output(string ch)
        {
            myVisaDC4142.WriteString("DZ " + ch , true);
        }

        public void Program_memory()
        {
            myVisaDC4142.WriteString("END", true);
        }

        public void Output_voltage(float ch, float Output_range, float Output_voltage, float I_compliance)
        {
            myVisaDC4142.WriteString("DV " + ch + "," + Output_range + "," + Output_voltage + "," + I_compliance , true);
        }

        public void Output_current(float ch, float Output_range, float Output_current, float V_compliance)
        {
            myVisaDC4142.WriteString("DI " + ch + "," + Output_range + "," + Output_current + "," + V_compliance , true);
        }

        public double I_measurement(float ch, float I_measurement_range)
        {
            double _value = 0;
            _value = WRITE_READ_DOUBLE("TI " + ch + "," + I_measurement_range);
            return _value;
        }

        public double I_Meas_Average(float ch, float I_measurement_range, float Average)
        {
            double _value = 0;
            myVisaDC4142.WriteString("BDM 1,1", true);
            myVisaDC4142.WriteString("AV " + Average, true);
            _value = WRITE_READ_DOUBLE("TI " + ch + "," + I_measurement_range);
            return _value;
        }

        public void HighSpeed_Setting()
        {
            myVisaDC4142.WriteString("AAD 1,0" , true);
            myVisaDC4142.WriteString("AAD 2,0" , true);
            myVisaDC4142.WriteString("AAD 4,0" , true);
            myVisaDC4142.WriteString("AAD 6,0" , true);
            myVisaDC4142.WriteString("AAD 7,0" , true);
            myVisaDC4142.WriteString("AAD 8,0" , true);
            myVisaDC4142.WriteString("AV 3" , true);
        }

        public double  MeasureCurrent(string ch, string I_measurement_range)
        {
            double _value = 0;
              _value = WRITE_READ_DOUBLE("FMT2;TI " + ch + "," + I_measurement_range);
            return _value;
        }

        public double  V_measurement(string ch, string V_measurement_range)
        {
            double _value = 0;
              _value = WRITE_READ_DOUBLE("TV " + ch + "," + V_measurement_range);
            return _value;
        }


        public void WRITE(string _cmd)
        {
            myVisaDC4142.WriteString(_cmd, true);
        }
        public double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaDC4142.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaDC4142.ReadString());
        }
        public string WRITE_READ_STRING(string _cmd)
        {
            try
            {
                //bool opcflag = OPERATION_COMPLETE(); 
                myVisaDC4142.WriteString(_cmd, true);
                return myVisaDC4142.ReadString();
            }
            catch (Exception ex)
            {
                throw new Exception("EquipDC4142: command -> " + _cmd + ex.Message);
            }
        }
        public string READ_STRING()
        {
            try
            {
                //bool opcflag = OPERATION_COMPLETE(); 
                //myVisaDC4142.WriteString(_cmd, true);
                return myVisaDC4142.ReadString();
            }
            catch (Exception ex)
            {
                throw new Exception("EquipDC4142: command -> " + ex.Message + "_");//ºNumParameter);
            }
        }
        public float WRITE_READ_SINGLE(string _cmd)
        {
            myVisaDC4142.WriteString(_cmd, true);
            return Convert.ToSingle(myVisaDC4142.ReadString());
        }
        public float[] READ_IEEEBlock(IEEEBinaryType _type)
        {
            return (float[])myVisaDC4142.ReadIEEEBlock(_type, true, true);
        }
        public float[] WRITE_READ_IEEEBlock(string _cmd, IEEEBinaryType _type)
        {
            myVisaDC4142.WriteString(_cmd, true);
            return (float[])myVisaDC4142.ReadIEEEBlock(_type, true, true);
        }
    }
}
