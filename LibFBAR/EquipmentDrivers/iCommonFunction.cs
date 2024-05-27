using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Threading;
using ClothoLibStandard;

namespace LibFBAR
{
    public struct s_EquipmentInfo
    {
        public string Manufacturer;
        public string ModelNumber;
        public string SerialNumber;
        public string FirmwareVersion;
    }
    public interface iCommonFunction
    {
        string Address
        {
            get;
            set;
        }

        void OpenIO();
        void CloseIO();
        string Version();
    }
    public class cCommonFunction
    {
        protected FormattedIO488 IO = new FormattedIO488();
        public cSystem System;
        public bool b_IOEnable = true;
        HiPerfTimer timer = new HiPerfTimer();
        public cCommonFunction(FormattedIO488 parse)
        {
            IO = parse;
            System = new cSystem(parse);
        }
        public string DeviceName()
        {
            IO.WriteString("*IDN?", true);
            return (IO.ReadString());
        }
        public s_EquipmentInfo DeviceInfo()
        {
            //KKL : 10 May 2013 - Read back equipment info
            IO.WriteString("*IDN?", true);
            string[] Data = IO.ReadString().Split(',');
            s_EquipmentInfo Info = new s_EquipmentInfo();
            if (Data.Length >= 4)
            {
                Info.Manufacturer = Data[0];
                Info.ModelNumber = Data[1];
                Info.SerialNumber = Data[2];
                Info.FirmwareVersion = Data[3];
            }
            return Info;
        }
        public void SendCommand(string cmd)
        {
            IO.WriteString(cmd, true);
        }
        public string ReadCommand(string cmd)
        {
            IO.WriteString(cmd, true);
            try
            {
                return (IO.ReadString());
            }
            catch
            {
                timer.wait(2000);   
                return (IO.ReadString());
            }


        }
        public double[] ReadIEEEBlock(string cmd)
        {
            IO.WriteString(cmd, true);
            return ((double[])IO.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8 , true, true));
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
                IO.WriteString(cmd, true);
                return (IO.ReadString()); 
            }
            public string Equipment_ID()
            {
                return (ReadCommand("*IDN?"));
            }
            public void Reset()
            {
                SendCommand("*RST");
            }
            public void Clear_Status()
            {
                SendCommand("*CLR");
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
                int cnt = 0;
            OPC:
                try
                {
                    if (cnt <= 5)
                        return (Convert.ToInt32(ReadCommand("*OPC?")));
                    else
                        return 0;
                    
                }
                catch
                {
                    cnt++;
                    Thread.Sleep(5000);
                    goto OPC;
                }
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

            public string QueryError()
            {
                string ErrMsg, TempErrMsg = "";
                int ErrNum;
                try
                {
                    ErrMsg = ReadCommand("SYST:ERR?");
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
                            ErrMsg = ReadCommand("SYST:ERR?");

                            // Remove the error number
                            ErrNum = Convert.ToInt16(ErrMsg.Remove((ErrMsg.IndexOf(",")),
                                (ErrMsg.Length) - (ErrMsg.IndexOf(","))));
                        }
                    }
                    return TempErrMsg;
                }
                catch (Exception ex)
                {
                    throw new Exception("QUERY_ERROR --> " + ex.Message);
                }
            }
        }
    }
}
