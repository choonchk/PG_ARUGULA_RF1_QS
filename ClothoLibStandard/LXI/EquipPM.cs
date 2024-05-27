using System;
using System.Windows.Forms;
using Ivi.Visa.Interop;

namespace ClothoLibStandard
{
    public class EquipPM
    {
        FormattedIO488 myVisaPM = null;

        public EquipPM(FormattedIO488 _myVisaPM)
        {
            myVisaPM = _myVisaPM;
        }
        ~EquipPM() { }

        public void INITIALIZATION()
        {
            try
            {
                myVisaPM.WriteString("*IDN?", true);
                string result = myVisaPM.ReadString();
            }
            catch (Exception ex)
            {
                throw new Exception("EquipPM: Initialization -> " + ex.Message);
            }

        }
        public void Set_Freq(float _cmdMHz)
        {
            myVisaPM.WriteString("Sens:Freq " + Convert.ToString(_cmdMHz) + "MHz", true);
        }

        public void PRESET()
        {
            myVisaPM.WriteString("SYST:PRES", true);
        }
        public void RESET()
        {
            try
            {
                myVisaPM.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("EquipPM: RESET -> " + ex.Message);
            }
        }
        public bool OPERATION_COMPLETE()
        {
            try
            {
                bool _complete = false;
                double _dummy = -99;
                do
                {
                    _dummy = WRITE_READ_DOUBLE("*OPC?");
                } while (_dummy == 0);
                _complete = true;
                return _complete;

            }
            catch (Exception ex)
            {
                throw new Exception("EquipPM: OPERATION_COMPLETE -> " + ex.Message);
            }
        }
        public string QUERY_ERROR()
        {
            string ErrMsg, TempErrMsg = "";
            int ErrNum;
            try
            {
                ErrMsg = WRITE_READ_STRING("SYST:ERR?");
                TempErrMsg = ErrMsg;
                // Remove the error number
                ErrNum = Convert.ToInt16(ErrMsg.Remove((ErrMsg.IndexOf(",")),
                    (ErrMsg.Length) - (ErrMsg.IndexOf(","))));
                if (ErrNum != 0)
                {
                    while (ErrNum != 0)
                    {
                        TempErrMsg = ErrMsg;

                        // Check for next error(s)
                        ErrMsg = WRITE_READ_STRING("SYST:ERR?");

                        // Remove the error number
                        ErrNum = Convert.ToInt16(ErrMsg.Remove((ErrMsg.IndexOf(",")),
                            (ErrMsg.Length) - (ErrMsg.IndexOf(","))));
                    }
                }
                return TempErrMsg;
            }
            catch (Exception ex)
            {
                throw new Exception("EquipPM: QUERY_ERROR --> " + ex.Message);
            }
        }
        public string QUERY_MODEL()
        {
            myVisaPM.WriteString("*IDN?", true);
            string[] arryStringResult = myVisaPM.ReadString().Split(',');
            return arryStringResult[1].Trim();
        }

        public double MeasBurstGSM()
        {
            double result = -999;
            string strSensorType;

            WRITE("*CLS");
            WRITE("*RST");
            strSensorType = WRITE_READ_STRING("SERV:SENS:TYPE?");
            WRITE("SENS:FREQ:900MHz");

            if ((strSensorType.ToUpper() == "E9321A") || (strSensorType.ToUpper() == "E9325A"))
                WRITE("SENS:BW:VID:HIGH");
            if ((strSensorType.ToUpper() == "E9323A") || (strSensorType.ToUpper() == "E9327A"))
                WRITE("SENS:BW:VID:LOW");
            
            WRITE("SENS:SWE1:OFFS:TIME:0.00002");
            WRITE("SENS:SWE1:TIME:0.0052");
            WRITE("INIT:CONT ON");
            WRITE("TRIG:SOUR INT");
            WRITE("TRIG:LEV:AUTO OFF");
            WRITE("TRIG:LEV -20");
            WRITE("TRIG:DEL 0.00002");
            WRITE("TRIG:HOLD 0.004275");
            WRITE("DISP:WIND1:TRACE:LOW -35");
            WRITE("DISP:WIND1:TRACE:UPP 20");
            WRITE("SENS:TRAC:OFFS:TIME -0.0004");
            WRITE("SENS:TRAC:TIME 0.0007");
            WRITE("DISP:WIND1:FROM TRACE");
            WRITE("DISP:WIND2:FORM SNUM");
            WRITE("CALC2:FEED1 \"POW:AVER ON SWEEP1\"");
            result = WRITE_READ_DOUBLE("FETCH2?");



            return result;
        }
        public double MeasBurstLTE()
        {
            double result = -999;
            string strSensorType;

            WRITE("*CLS");
            WRITE("*RST");
            strSensorType = WRITE_READ_STRING("SERV:SENS:TYPE?");
            WRITE("SENS:FREQ:900MHz");

            if ((strSensorType.ToUpper() == "E9321A") || (strSensorType.ToUpper() == "E9325A"))
                WRITE("SENS:BW:VID:HIGH");
            if ((strSensorType.ToUpper() == "E9323A") || (strSensorType.ToUpper() == "E9327A"))
                WRITE("SENS:BW:VID:LOW");

            WRITE("SENS:SWE1:OFFS:TIME:0.00002");
            WRITE("SENS:SWE1:TIME:0.0052");
            WRITE("INIT:CONT ON");
            WRITE("TRIG:SOUR INT");
            WRITE("TRIG:LEV:AUTO OFF");
            WRITE("TRIG:LEV -20");
            WRITE("TRIG:DEL 0.00002");
            WRITE("TRIG:HOLD 0.004275");
            WRITE("DISP:WIND1:TRACE:LOW -35");
            WRITE("DISP:WIND1:TRACE:UPP 20");
            WRITE("SENS:TRAC:OFFS:TIME -0.0004");
            WRITE("SENS:TRAC:TIME 0.0007");
            WRITE("DISP:WIND1:FROM TRACE");
            WRITE("DISP:WIND2:FORM SNUM");
            WRITE("CALC2:FEED1 \"POW:AVER ON SWEEP1\"");
            result = WRITE_READ_DOUBLE("FETCH2?");



            return result;
        }
        public double MeasBurstCDMA()
        {
            double result = -999;
            string strSensorType;

            WRITE("*CLS");
            WRITE("*RST");
            strSensorType = WRITE_READ_STRING("SERV:SENS:TYPE?");
            WRITE("SENS:FREQ:900MHz");

            if ((strSensorType.ToUpper() == "E9321A") || (strSensorType.ToUpper() == "E9325A"))
                WRITE("SENS:BW:VID:HIGH");
            if ((strSensorType.ToUpper() == "E9323A") || (strSensorType.ToUpper() == "E9327A"))
                WRITE("SENS:BW:VID:LOW");

            WRITE("SENS:SWE1:OFFS:TIME:0.00002");
            WRITE("SENS:SWE1:TIME:0.0052");
            WRITE("INIT:CONT ON");
            WRITE("TRIG:SOUR INT");
            WRITE("TRIG:LEV:AUTO OFF");
            WRITE("TRIG:LEV -20");
            WRITE("TRIG:DEL 0.00002");
            WRITE("TRIG:HOLD 0.004275");
            WRITE("DISP:WIND1:TRACE:LOW -35");
            WRITE("DISP:WIND1:TRACE:UPP 20");
            WRITE("SENS:TRAC:OFFS:TIME -0.0004");
            WRITE("SENS:TRAC:TIME 0.0007");
            WRITE("DISP:WIND1:FROM TRACE");
            WRITE("DISP:WIND2:FORM SNUM");
            WRITE("CALC2:FEED1 \"POW:AVER ON SWEEP1\"");
            result = WRITE_READ_DOUBLE("FETCH2?");



            return result;
        }
        public void WRITE(string _cmd)
        {
            myVisaPM.WriteString(_cmd, true);
        }
        public float WRITE_READ_SINGLE(string _cmd)
        {
            myVisaPM.WriteString(_cmd, true);
            return Convert.ToSingle(myVisaPM.ReadString());
        }
        public double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaPM.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaPM.ReadString());
        }
        public string WRITE_READ_STRING(string _cmd)
        {
            myVisaPM.WriteString(_cmd, true);
            return myVisaPM.ReadString();
        }
    }
}
