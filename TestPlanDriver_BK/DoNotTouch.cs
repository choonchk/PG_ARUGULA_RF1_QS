using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;


namespace TestPlanDriver
{
    static class Program
    {
        [MTAThread]
        static void Main()
        {
            bool bOn = false;
            try
            {
                bOn = (ConfigurationManager.AppSettings["Logger.OnOff"] == "1");
            }
            catch
            {
                Trace.WriteLine("Fail to read Logger.OnOff from app.config");
                // this is not fatal
            }

            if (bOn)
            {
                try
                {
                    // This is MSMQ based singelton, so it's OK to put it here 
                    System.Diagnostics.Process.Start(@"C:\Avago.ATF.Common\Logger\Avago.ATF.LogService.exe");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Fail to start logger as expected: " + ex.Message);
                    // this is neither fatal 
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDoNOTTouch());
        }
    }
}
