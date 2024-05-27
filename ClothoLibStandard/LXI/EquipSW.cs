using System;
using Ivi.Visa.Interop;


namespace ClothoLibStandard
{
    public class EquipSW
    {        
        FormattedIO488 myVisaSa = null;

        public EquipSW(FormattedIO488 _myVisaSa) 
        {
            myVisaSa = _myVisaSa;
        }
        ~EquipSW() { }

        public void RESET()
        {
            try
            {
                myVisaSa.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("EquipSW: RESET -> " + ex.Message);
            }
        }

        public void WRITE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
        }
        public void SW_control(string _StatusSW,string _SwitchSlot)
        {
            myVisaSa.WriteString(_StatusSW +" (@"+_SwitchSlot+")", true);
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
            return Convert.ToSingle(myVisaSa.ReadString());
        }
    }
}
