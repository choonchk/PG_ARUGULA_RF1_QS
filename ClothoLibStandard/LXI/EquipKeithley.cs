using System;
using Ivi.Visa.Interop;

namespace ClothoLibStandard
{
    public class EquipKeithley : ClothoLibStandard.Lib_Var
    {
        FormattedIO488 myVisaKeithley = null;
        public EquipKeithley(FormattedIO488 _myVisaKeithley)
        {
            myVisaKeithley = _myVisaKeithley;
        }
        ~EquipKeithley() { }

        public void RESET()
        {
            try
            {

                myVisaKeithley.IO.WriteString("reset()" );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipKeithley: RESET -> " + ex.Message);
            }
        }
        public void SentCmd(string strCmd)
        {
            try
            {
                myVisaKeithley.IO.WriteString(strCmd );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: SentCmd -> " + ex.Message);
            }
        }
        
        public void SetOutput(Keith_Channel val, bool _ON)
        {
            try
            {
                if (_ON)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.output = smu" + val.ToString() + ".OUTPUT_ON" );
                    
                }
                else
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.output = smu" + val.ToString() + ".OUTPUT_OFF" );
                }
            }
            catch(Exception ex)
            {
                throw new Exception("EquipmentKeithley: SetOutput -> " + ex.Message);
            }

        }
        public void VMeasAutoRange(Keith_Channel val, bool _on_off)
        {
            try
            {
                if (_on_off)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".measure.autorangeI = smu" + val.ToString() + "AUTORANGE_ON");
                }
                else
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".measure.autorangeI = smu" + val.ToString() + "AUTORANGE_OFF");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: VMeasAutoRange -> " + ex.Message);
            }
        }
        public void IMeasAutoRange(Keith_Channel val, bool _on_off)
        {
            try
            {
                if (_on_off)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".measure.autorangei = smu" + val.ToString() + ".AUTORANGE_ON");
                }
                else
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".measure.autorangei = smu" + val.ToString() + ".AUTORANGE_OFF");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: IMeasAutoRange -> " + ex.Message);
            }
        }
        public void VSourceAutoRange(Keith_Channel val, bool _on_off)
        {
            try
            {
                if (_on_off)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.autorangev = smu" + val.ToString() + ".AUTORANGE_ON");
                }
                else
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.autorangev = smu" + val.ToString() + ".AUTORANGE_OFF");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: VSourceAutoRange -> " + ex.Message);
            }
        }
        public void ISourceAutoRange(Keith_Channel val, bool _on_off)
        {
            try
            {
                if (_on_off)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.autorangeI = smu" + val.ToString() + "AUTORANGE_ON");
                }
                else
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.autorangeI = smu" + val.ToString() + "AUTORANGE_OFF");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: ISourceAutoRange -> " + ex.Message);
            }
        }
        public void VSourceSet(Keith_Channel val, double dblVoltage, double dblClampI, Keith_VRange _range)
        {
            try
            {
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.func = smu" + val.ToString() + ".OUTPUT_DCVOLTS" );
                if (_range != Keith_VRange._Auto)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.rangev = " + VRange_String(_range).ToString());
                }
                else
                {
                    VSourceAutoRange(val, true);
                }
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.limiti = " + dblClampI.ToString() );
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.levelv = " + dblVoltage.ToString() );
                
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: VSourceSet -> " + ex.Message);
            }
        }
        public void ISourceSet(Keith_Channel val, double dblAmps, double dblClampV, Keith_IRange _range)
        {
            try
            {
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.func = smu" + val.ToString() + ".OUTPUT_DCAMPS" );
                if (_range != Keith_IRange._Auto)
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.rangei = " + IRange_String(_range).ToString());
                }
                else
                {
                    ISourceAutoRange(val, true);
                }
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.limitv = " + dblClampV.ToString() );
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.leveli = " + dblAmps.ToString() );
                
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: ISourceSet -> " + ex.Message);
            }
        }
        public void VChangeLevel(Keith_Channel val, double dblVoltage)
        {
            try
            {
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.levelv = " + dblVoltage.ToString() );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: VChangeLevel -> " + ex.Message);
            }
        }
        public void IChangeLevel(Keith_Channel val, double dblAmps)
        {
            try
            {
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.leveli = " + dblAmps.ToString() );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: IChangeLevel -> " + ex.Message);
            }
        }
        public void ILimit(Keith_Channel val, double dblAmps)
        {
            try
            {
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.limiti = " + dblAmps.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: ILimit -> " + ex.Message);
            }
        }
        public void VLimit(Keith_Channel val, double dblVoltage)
        {
            try
            {
                myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".source.limitv = " + dblVoltage.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: VLimit -> " + ex.Message);
            }
        }
        public void SetNPLC(Keith_Channel val, double dblNPLC)
        {
            try
            {
                if((dblNPLC < 0.001) | (dblNPLC > 25))
                {
                    throw new Exception("EquipmentKeithley: SetNPLC -> must in range: 0.001 < NPLC < 25");
                }
                else
                {
                    myVisaKeithley.IO.WriteString("smu" + val.ToString() + ".measure.nplc = " + dblNPLC.ToString() );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: SetNPLC -> " + ex.Message);
            }
        }
        public void DisplayClear()
        {
            try
            {
                myVisaKeithley.IO.WriteString("display.clear()" );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: DisplayClear -> " + ex.Message);
            }
        }
        public void DisplayVolt(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("display.smu" + val.ToString() + ".measure.func = display.MEASURE_DCVOLTS" );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: DisplayVolt -> " + ex.Message);
            }

        }
        public void DisplayAmps(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("display.smu" + val.ToString() + ".measure.func = display.MEASURE_DCAMPS");
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: DisplayAmps -> " + ex.Message);
            }

        }
        public void DisplayOhms(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("display.smu" + val.ToString() + ".measure.func = display.MEASURE_OHMS" );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: DisplayOhms -> " + ex.Message);
            }

        }
        public void DisplayWatt(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("display.smu" + val.ToString() + ".measure.func = display.MEASURE_WATTS" );
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: DisplayWatt -> " + ex.Message);
            }

        }

        public double MeasV(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("print(smu" + val.ToString() + ".measure.v())" );
                return Convert.ToDouble(myVisaKeithley.ReadString());
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: MeasV -> " + ex.Message);
            }
        }
        public double MeasI(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("print(smu" + val.ToString() + ".measure.i())" );
                return Convert.ToDouble(myVisaKeithley.ReadString());
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: MeasI -> " + ex.Message);
            }
        }
        public double MeasWatt(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("print(smu" + val.ToString() + ".measure.p())" );
                return Convert.ToDouble(myVisaKeithley.ReadString());
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: MeasWatt -> " + ex.Message);
            }
        }
        public double MeasOhms(Keith_Channel val)
        {
            try
            {
                myVisaKeithley.IO.WriteString("print(smu" + val.ToString() + ".measure.r())" );
                return Convert.ToDouble(myVisaKeithley.ReadString());
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: MeasOhms -> " + ex.Message);
            }
        }
        public string ReadString(string strCmd)
        {
            try
            {
                myVisaKeithley.IO.WriteString("print(" + strCmd + ")" );
                return myVisaKeithley.ReadString();
            }
            catch (Exception ex)
            {
                throw new Exception("EquipmentKeithley: SentCmd -> " + ex.Message);
            }
        }

        public double VRange_String(Keith_VRange val)
        {
            if (val == Keith_VRange._260x_100mV) return 100e-3;
            if (val == Keith_VRange._260x_1V) return 1;
            if (val == Keith_VRange._260x_40V) return 40;
            if (val == Keith_VRange._260x_6V) return 6;
            if (val == Keith_VRange._261x_200mV) return 200e-3;
            if (val == Keith_VRange._261x_200V) return 200;
            if (val == Keith_VRange._261x_20V) return 20;
            if (val == Keith_VRange._261x_2V) return 2;
            if (val == Keith_VRange._Auto) return 999;
            else return 0;
        }
        public double IRange_String(Keith_IRange val)
        {
            if (val == Keith_IRange._260x_3A) return 3;
            if (val == Keith_IRange._261x_1_5A) return 1.5;
            if (val == Keith_IRange._261x_10A) return 10;
            if (val == Keith_IRange._all_100mA) return 100e-3;
            if (val == Keith_IRange._all_100nA) return 100e-9;
            if (val == Keith_IRange._all_100uA) return 100e-6;
            if (val == Keith_IRange._all_10mA) return 10e-3;
            if (val == Keith_IRange._all_10uA) return 10e-6;
            if (val == Keith_IRange._all_1A) return 1;
            if (val == Keith_IRange._all_1mA) return 1e-3;
            if (val == Keith_IRange._all_1uA) return 1e-6;
            if (val == Keith_IRange._Auto) return 999;
            else return 0;
        }
    }
}
