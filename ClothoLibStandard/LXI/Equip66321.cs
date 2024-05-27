using System;
using Ivi.Visa.Interop;


namespace ClothoLibStandard
{
    public class Equip66321
    {
        FormattedIO488 myVisaSa = null;

        public Equip66321(FormattedIO488 _myVisaSa) 
        {
            myVisaSa = _myVisaSa;
        }
        ~Equip66321() { }

        public void RESET()
        {
            try
            {
                myVisaSa.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("Equip66321: RESET -> " + ex.Message);
            }
        }


        public float READCURRENT(string MEASURE_RANGE)
        {

            switch (MEASURE_RANGE)
            {

                case "RMS":
                    return WRITE_READ_SINGLE("MEAS:CURR:ACDC?");
                case "HIGH":
                    return WRITE_READ_SINGLE("MEAS:CURR:HIGH?");
                case "LOW":
                    return WRITE_READ_SINGLE("MEAS:CURR:LOW?");
                case "MAX":
                    return WRITE_READ_SINGLE("MEAS:CURR:MAX?");
                case "MIN":
                    return WRITE_READ_SINGLE("MEAS:CURR:MIN?");
                default:
                    return -9999;

            }
        }    // end of   66321Readcurrent

        public void WRITE(string _cmd)
        {
            myVisaSa.WriteString(_cmd, true);
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
