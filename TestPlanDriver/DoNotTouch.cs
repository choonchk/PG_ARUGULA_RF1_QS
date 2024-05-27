using System;
using System.Collections.Generic;
using System.Linq;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDoNOTTouch());
        }
    }
}
