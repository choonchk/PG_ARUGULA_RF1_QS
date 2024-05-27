using System;
using Ivi.Visa.Interop;
using System.Threading ;

namespace LibFBAR_TOPAZ
{
    /// <summary>
    /// Instrument IO base function.
    /// </summary>
    public class LibEqmtCommon2
    {
       // public static Agilent.AgNA.Interop.AgNA M9485 { get; set; }        
        protected FormattedIO488 IO = new FormattedIO488();
        public cSystem System;
        
        public LibEqmtCommon2(FormattedIO488 parse)
        {
            IO = parse;
            System = new cSystem(parse);
        }
        public string DeviceName()
        {
           // M9485.System.IO.WriteString("*IDN?", true);
           // return (M9485.System.IO.ReadString());
            IO.WriteString("*IDN?", true);
            return (IO.ReadString());
        }
        public void SendCommand(string cmd)
        {           
           // M9485.System.IO.WriteString(cmd);//, true);            
            IO.WriteString(cmd, true);
        }
        public string ReadCommand(string cmd)
        {
           // M9485.System.IO.WriteString(cmd, true);
           // return (M9485.System.IO.ReadString());
            IO.WriteString(cmd, true);
            return (IO.ReadString());
        }
        public double[] ReadIEEEBlock(string cmd)
        {

           // M9485.System.IO.WriteString("SENS1:CORR:CSET:STIM 1", true);
            try
            {
               IO.WriteString(cmd, true);
                return ((double[])IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
            }
            catch
            {
                return ((double[])IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
            }
            
            
        }
        public double[] ReadIEEEBlock(string cmd, IEEEBinaryType BinaryType)
        {
            IO.WriteString(cmd, true);
            return ((double[])IO.ReadIEEEBlock(BinaryType, true, true));
        }

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  10/11/2011       KKL             New Coding Version

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return ("Common Code Version = v" + VersionStr);
        }
        public class cSystem
        {
            protected FormattedIO488 IO = new FormattedIO488();
            public cSystem(FormattedIO488 parse) { IO = parse; }
            public void SendCommand(string cmd)
            {
                IO.WriteString(cmd, true); 
            }
            public string ReadCommand(string cmd)
            {
                try
                {
                    IO.WriteString(cmd, true);
                    return (IO.ReadString());
                }
                catch
                {
                    return ("999");
                }
            }
            public string Equipment_ID()
            {
                try
                {
                    return (ReadCommand("*IDN?"));
                }
                catch
                {
                    return ("Error");
                }
            }
            public void Reset()
            {
                SendCommand("*RST");
            }
            public void Clear_Status()
            {
                //SendCommand("*CLR");
                SendCommand("*CLS"); //MM - 07/11/2019
            }
            public void Event_Status_Enable(int NRf)
            {
                SendCommand("*ESE " + NRf.ToString());
            }
            public int Event_Status_Enable()
            {
                return (Convert.ToInt32(ReadCommand("*ESE?")));
            }
            public int Event_Register()
            {
                return (Convert.ToInt32(ReadCommand("*ESR?")));
            }

            public string Event_Register(bool turn2number)
            {
                string x = ReadCommand("*ESR?");

                if (!turn2number)
                {
                    switch (x)
                    {
                        case "+0\n":
                            x = "No Error";
                            break;

                        case "+1\n":
                            x = "Operation Complete";
                            break;

                        case "+4\n":
                            x = "Query Error";
                            break;

                        case "+8\n":
                            x = "Instrument Dependent Error";
                            break;

                        case "+16\n":
                            x = "Execution Error";
                            break;

                        case "+32\n":
                            x = "Commnand Error";
                            break;

                        case "+128\n":
                            x = "Power On";
                            break;

                        default:

                            x = "Alas, undefined!";
                            break;
                    }
                }


                return x;
            }

            public int Status_Byte()
            {
                return (Convert.ToInt32(ReadCommand("*STB?")));
            }
            public void Status_Register_Enable(int Mask)
            {
                SendCommand("*SRE " + Mask.ToString());
            }
            public int Status_Register_Enable()
            {
                return (Convert.ToInt32(ReadCommand("*SRE?")));
            }
            public int Self_Test()
            {
                return (Convert.ToInt32(ReadCommand("*TST?")));
            }
            public void SetOperation_Complete()
            {
                SendCommand("*OPC");
            }
            public int Operation_Complete()
            {
                //Thread.Sleep(1000);
                return (Convert.ToInt32(ReadCommand("*OPC?")));
            }
            public void Wait()
            {
                SendCommand("*WAI");
            }
            public void Trigger()
            {
                SendCommand("*TRG");
            }
            public void Save(int Memory_Location)
            {
                SendCommand("*SAV " + Memory_Location.ToString());
            }
            public void Recall(int Memory_Location)
            {
                SendCommand("*RCL " + Memory_Location.ToString());
            }
        }
    }
}
