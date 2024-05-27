using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;

using System.Runtime.InteropServices;
using System.Threading;


namespace GuCal
{

    public class WindowControl
    {
        // example calls
        // static WindowControl wnd = new WindowControl();
        // wnd.MinimizeExcel();
        // MessageBox.Show(wnd.ShowOnTop(), "this message should always show up on top");
        // if (wnd.usingLiteDriver) {}

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public WindowControl()
        {
            string ownerProcName = "";

            //check if using Lite Driver
            string file_pathname = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string[] dllPathSplit = file_pathname.Split('\\');
            if (dllPathSplit[2] != "System")
            {
                _usingLiteDriver = true;
                ownerProcName = "devenv";  // Visual Studio will be the owner window
            }
            else
            {
                _usingLiteDriver = false;
                ownerProcName = "Avago.ATF.UIs";  // Clotho will be the owner window
            }

            // grab owner handle
            Process[] ownerProc = Process.GetProcessesByName(ownerProcName);  // gets handle of owner window   
            if (ownerProc.Count() != 0)
            {
                ownerMainHandle = ownerProc[0].MainWindowHandle;
                ownerMainWindow = new ConvertHandleToIWin32Window(ownerMainHandle);
            }
        }

        private static bool _usingLiteDriver;
        private static IntPtr ownerMainHandle;
        private static IWin32Window ownerMainWindow;

        public bool usingLiteDriver
        {
            get { return _usingLiteDriver; }
        }

        public void MinimizeExcel()
        {
            ShowWindowAsync(FindWindow("XLMAIN", null), 6);  // minimize Excel
        }

        public IWin32Window ShowOnTop()
        {
            SetForegroundWindow(ownerMainHandle);  // bring owner window to foreground
            Thread.Sleep(5); // this is necessary so that form doesn't launch before owner window takes foreground. The delay doesn't matter for production, since we never use message boxes or forms during test.
            return (ownerMainWindow);  // return the IWin32Window of owner window, so messageboxes and forms always show up on top of other windows
        }

        private class ConvertHandleToIWin32Window : System.Windows.Forms.IWin32Window
        {
            public ConvertHandleToIWin32Window(IntPtr handle)
            {
                _hwnd = handle;
            }

            public IntPtr Handle
            {
                get { return _hwnd; }
            }

            private IntPtr _hwnd;
        }

    }

}



