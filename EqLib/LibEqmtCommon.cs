using System;
using Ivi.Visa.Interop;
using System.Windows.Forms;
using MPAD_TestTimer;

namespace EqLib
{
    /// <summary>
    /// Instrument IO base function.
    /// </summary>
    public class LibEqmtCommon
    {
        internal FormattedIO488 MyIo;

        public void OpenIo(string ioAddress, int intTimeOut = 5000)
        {
            try
            {
                ResourceManager mgr = new ResourceManager();
                MyIo = new FormattedIO488 {IO = (IMessage) mgr.Open(ioAddress, AccessMode.NO_LOCK, intTimeOut, "")};
                MyIo.IO.Timeout = intTimeOut;
            }
            catch (SystemException)
            {
                //common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                MyIo.IO = null;
                return;
            }
        }
        public void CloseIo()
        {
            MyIo.IO.Close();
        }
        public void Reset()
        {
            SendCommand("*RST");
            System.Threading.Thread.Sleep(5000);
        }
        public void SendCommand(string cmd)
        {
            MyIo.WriteString(cmd, true);
            //Operation_Complete();
        }
        public void SendCommand(string cmd, bool flushAndEnd)
        {
            MyIo.WriteString(cmd, flushAndEnd);
            //Operation_Complete();
        }
        public string ReadCommand(string cmd)
        {
            MyIo.WriteString(cmd, true);
            return (MyIo.ReadString());
        }
        public string ReadCommand(string cmd, bool flushAndEnd)
        {
            MyIo.WriteString(cmd, flushAndEnd);
            return (MyIo.ReadString());
        }
        public string ReadCommand()
        {
            return (MyIo.ReadString());
        }
        public double[] ReadIeeeBlock(string cmd)
        {
            MyIo.WriteString(cmd, true);
            return ((double[])MyIo.ReadIEEEBlock(IEEEBinaryType.BinaryType_R8, true, true));
        }
        public double[] ReadIeeeBlock(string cmd, IEEEBinaryType binaryType)
        {
            MyIo.WriteString(cmd, true);
            return ((double[])MyIo.ReadIEEEBlock(binaryType, true, true));
        }
        public int Operation_Complete()
        {
            return (Convert.ToInt32(ReadCommand("*OPC?")));
        }

        internal void DisplayError(string message, string title)
        {
            PromptManager.Instance.ShowError(message, title);
        }

        internal void DisplayError(string message, Exception ex)
        {
            PromptManager.Instance.ShowError(message, ex);
        }

        internal void DisplayMessage(string message, string title)
        {
            PromptManager.Instance.ShowInfo(message, title);
        }
    }
}
