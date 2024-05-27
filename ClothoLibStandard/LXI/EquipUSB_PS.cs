using System;
using Ivi.Visa.Interop;

namespace ClothoLibStandard
{
    public class EquipUSB_PS
    {
        FormattedIO488 myVisaSa = null;

        public EquipUSB_PS(FormattedIO488 _myVisaSa) 
        {
            myVisaSa = _myVisaSa;
        }
        ~EquipUSB_PS() { }

        public void RESET()
        {
            try
            {
                myVisaSa.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("EquipUSB_PS: RESET -> " + ex.Message);
            }
        }
        public void PRESET()
        {
            myVisaSa.WriteString("SYST:PRES", true);
        }

        public void WRITE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
        }
        public void Set_Freq(float _cmdMHz)
        {
            myVisaSa.WriteString("Sens:Freq "+ Convert.ToString(_cmdMHz) + "MHz", true);
        }
        public double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaSa.ReadString());
        }
        public string WRITE_READ_STRING(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
            return myVisaSa.ReadString();
        }
        public float WRITE_READ_SINGLE(string _cmd)
        {

            myVisaSa.WriteString(_cmd, true);
            //myVisaSa.WriteString("*OPC?", true);
            return Convert.ToSingle(myVisaSa.ReadString());
            
            
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
    }
}
