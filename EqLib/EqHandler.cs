using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ivi.Visa.Interop;
using Avago.ATF.StandardLibrary;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using Avago.ATF.Shares;
using System.Windows.Forms;

namespace EqLib
{
    public class EqHandler
    {

        public static iEqHandler Get(bool handlerDriverRunning, string HandlerType)
        {
            iEqHandler Handler;

            if (HandlerType.Contains("MANUAL"))
            {
                Handler = new EqHandler.Manual();
                Handler.Handler_Type = HandlerType;
            }
            else if (HandlerType.Contains("S1") || HandlerType.Contains("S9"))
            {
                Handler = new EqHandler.S9_Standard();
                Handler.Handler_Type = HandlerType;
            }
            else if (HandlerType.Contains("AUTO"))
            {
                if (ATFRTE.Instance.HandlerType != null)
                {
                    Handler = new EqHandler.HandlerPlugin();
                    Handler.Handler_Type = ATFRTE.Instance.HandlerType.ToString();
                }
                else
                {
                    Handler = new EqHandler.Manual();
                    Handler.Handler_Type = "MANUAL";
                }
            }
            else
            {
                Handler = new EqHandler.Manual();
                Handler.Handler_Type = "MANUAL";
            }
            ATFLogControl.Instance.Log(LogLevel.Warn, LogSource.eTestPlan, "HandlerPlugin: " + Handler.Handler_Type);

            return Handler;
        }

        public interface iEqHandler
        {
            string Handler_Type { get; set; }
            bool Initialize(string VisaAlias);
            bool TrayMapCoord(string xycoord);
            bool CheckSRQStatusByte(int expected);
            bool TrayMapEOT(string BinTags);
            void TrayMapTermination();
            void WaitForDUT();
        }

        public class S9_Standard : iEqHandler
        {
            public string _Handler_Type;
            private FormattedIO488 handlerIO;
            public string HandlerVisaAddress;

            public string Handler_Type
            {
                get
                {
                    return _Handler_Type;
                }
                set
                {
                    _Handler_Type = value;
                }
            }

            public bool Initialize(string VisaAlias)
            {
                this.handlerIO = GetIOInstance(VisaAlias);
                return true;
            }

            public bool TrayMapCoord_ms(string xycoord)
            {
                bool statusbyte;
                try
                {
                    ATFCrossDomainWrapper.SwitchSplitMSitesHandlerPoll(false);

                    string s1 = "";

                    string targetString = ("TMINFO:" + xycoord + "\r\n").Replace(";", "");

                    while (s1 != targetString)
                    {
                        handlerIO.WriteString("TMINFO:" + xycoord + "\r\n", true);
                        Thread.Sleep(50);
                        s1 = handlerIO.ReadString().Replace(";", "");
                        Thread.Sleep(5);
                    }
                    //handlerIO.WriteString("TMEND\r\n", true);
                    //Thread.Sleep(5);
                    //string s2 = handlerIO.ReadString();
                    //Thread.Sleep(5);
                    if (CheckSRQStatusByte(65)) CheckSRQStatusByte(65);
                    handlerIO.WriteString("FULLSITES?\r\n", true);
                    Thread.Sleep(5);
                    string s3 = handlerIO.ReadString();
                    return true;
                }
                catch (Exception e)
                {
                    statusbyte = CheckSRQStatusByte(65);
                    return false;
                }
            }

            public bool TrayMapCoord(string xycoord)
            {
                bool statusbyte;
                try
                {
                    //MessageBox.Show("TrayMapCoord:"+ xycoord);
                    handlerIO.WriteString("TMINFO:" + xycoord + "\r\n", true);
                    Thread.Sleep(5);
                    string s1 = handlerIO.ReadString();
                    Thread.Sleep(5);
                    handlerIO.WriteString("TMEND\r\n", true);
                    Thread.Sleep(5);
                    string s2 = handlerIO.ReadString();
                    Thread.Sleep(5);
                    statusbyte = CheckSRQStatusByte(65);

                    return true;

                }
                catch (Exception e)
                {

                    statusbyte = CheckSRQStatusByte(65);
                    //MessageBox.Show("TrayMapCoord catch: " + Environment.NewLine + e.ToString() + Environment.NewLine + "Status byte is: " + statusbyte.ToString());
                    //throw new Exception("TrayMapCoord NOT successful: \r\n" + e.ToString());
                    return false;

                }
            }

            public bool CheckSRQStatusByte(int expected)
            {
                ATFCrossDomainWrapper.SwitchSplitMSitesHandlerPoll(false);

                for (int x = 1; x < 100; x++)
                {
                    int StatCheck = handlerIO.IO.ReadSTB();

                    if (StatCheck == expected) return false;

                    Thread.Sleep(100);
                }

                return true;
            }

            public bool TrayMapEOT(string BinTags)
            {

                try
                {
                    bool blah = false;
                    string fullSitesResultHex = WriteAndReadWithErrorReporting("FULLSITES?\r", "", ref blah);

                    string binningCommand = ":AAAAAAAA,AAAAAAAA,AAAAAAAA," + (new string(BinTags.Replace("-1", "G").Replace(",", "").Reverse().ToArray())).PadLeft(8, 'A') + ";";

                    int echoFailCount = 0;

                    while (echoFailCount++ < 50)
                    {
                        try
                        {
                            bool success = false;

                            string result = WriteAndReadWithErrorReporting("BINON" + binningCommand + "\r", "", ref success).Trim();

                            if (result == "ECHO" + binningCommand)
                            {
                                WriteWithErrorReporting("ECHOOK\r", "", ref success);
                                break;
                            }
                            else
                            {
                                WriteWithErrorReporting("ECHONG\r", "", ref success);
                                if (echoFailCount > 40)
                                {
                                    throw new Exception("Handler has failed over 40 times to echo the correct bins");
                                }
                                Thread.Sleep(1);
                            }
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(50);
                            if (echoFailCount > 40)
                            {
                                throw new Exception("Handler is repeatedly erroring during BINON command\n\n" + e.ToString());
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    ATFLogControl.Instance.Log(LogLevel.Error, ex.ToString());
                    return false;
                }

                return true;
            }

            public void TrayMapTermination()
            {
                handlerIO.WriteString("TMTERM\r\n", true);
                Thread.Sleep(5);
                string s1 = handlerIO.ReadString();
                ATFCrossDomainWrapper.SwitchSplitMSitesHandlerPoll(true);
            }

            private string WriteAndReadWithErrorReporting(string cmd, string extraErrorMsg, ref bool success)
            {
                WriteWithErrorReporting(cmd, extraErrorMsg, ref success);

                int readFailCount = 0;

                if (!success) return "";

                while (readFailCount++ < 5)
                {
                    try
                    {
                        string s = handlerIO.ReadString();
                        success = true;
                        return s;
                    }
                    catch (Exception e)
                    {
                        ATFLogControl.Instance.Log(LogLevel.Error, "Handler driver failed to read after writing:  " + cmd + ", " + extraErrorMsg + "\n" + e.ToString());
                        Thread.Sleep(500);
                        success = false;
                    }
                }

                return "";

            }

            private void WriteWithErrorReporting(string cmd, string extraErrorMsg, ref bool success)
            {
                int failCount = 0;

                while (failCount++ < 5)
                {
                    try
                    {
                        handlerIO.WriteString(cmd, true);
                        success = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        ATFLogControl.Instance.Log(LogLevel.Error, "Handler driver failed to write:  " + cmd + ", " + extraErrorMsg + "\n" + e.ToString());
                        Thread.Sleep(500);
                        success = false;
                    }
                }
            }

            private static FormattedIO488 GetIOInstance(string VisaAddress)
            {
                FormattedIO488 ioInstance = new FormattedIO488();
                try
                {
                    ResourceManager grm = new ResourceManager();
                    ioInstance.IO = (IMessage)grm.Open(VisaAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (Exception e)
                { }


                return ioInstance;
            }

            public void WaitForDUT()
            {
            }
        }

        public class Manual : iEqHandler
        {
            private string _Handler_Type;
            public string HandlerVisaAddress;

            public string Handler_Type
            {
                get
                {
                    return _Handler_Type;
                }
                set
                {
                    _Handler_Type = value;
                }
            }

            public bool Initialize(string VisaAlias)
            {
                return true;
            }


            public bool TrayMapCoord(string xycoord)
            {
                if (xycoord.Contains(","))
                {
                    string[] coord = xycoord.Split(',');
                    MessageBox.Show("Insert Cal standard from column: " + coord[0] + "        Row: " + coord[1]);
                }
                else
                    MessageBox.Show("Insert Cal Standard: " + xycoord);

                return true;
            }

            public bool CheckSRQStatusByte(int expected)
            {
                return true;
            }

            public bool TrayMapEOT(string BinTags)
            {

                return true;
            }

            public void TrayMapTermination()
            {
            }

            public void WaitForDUT()
            {
            }
        }

        public class HandlerPlugin : iEqHandler
        {
            private string _Handler_Type;
            public string HandlerVisaAddress;

            public string Handler_Type
            {
                get
                {
                    return _Handler_Type;
                }
                set
                {
                    _Handler_Type = value;
                }
            }

            public bool Initialize(string VisaAlias)
            {
                return true;
            }

            public bool TrayMapCoord(string xycoord)
            {
                throw new NotImplementedException();
            }

            public bool CheckSRQStatusByte(int expected)
            {
                throw new NotImplementedException();
            }

            public bool TrayMapEOT(string BinTags)
            {
                return true;
            }

            public void TrayMapTermination()
            {
            }

            public void WaitForDUT()
            {
            }
        }
    }
}
