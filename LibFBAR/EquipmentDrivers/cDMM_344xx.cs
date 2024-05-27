using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR
{
    public class cDMM_344xx : iCommonFunction
    {
        public static string ClassName = "ENA E5071A/B/C Class";
        private string IOAddress;
        private FormattedIO488 ioDMM;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();

        #region "Enumeration Declaration"
        public enum e_Range
        {
            MIN = 0,
            MAX,
            DEF
        }
        public enum e_Resolution
        {
            MIN = 0,
            MAX,
            DEF
        }
        public enum e_Auto
        {
            OFF = 0,
            ON
        }
        #endregion

        #region "Structure"
        
        #endregion

        #region "Conversion Functions"
        
        #endregion

        /// <summary>
        /// Parsing Equpment Address
        /// </summary>
        public string Address
        {
            get
            {
                return IOAddress;
            }
            set
            {
                IOAddress = value;
            }
        }
        /// <summary>
        /// Parsing IO (in FormattedIO488)
        /// </summary>
        public FormattedIO488 parseIO
        {
            get
            {
                return ioDMM;
            }
            set
            {
                ioDMM = parseIO;
            }
        }
        /// <summary>
        /// Open Equipment IO
        /// </summary>
        public void OpenIO()
        {
            if (IOAddress.Length > 6)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    ioDMM = new FormattedIO488();
                    ioDMM.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioDMM.IO = null;
                    return;
                }
                Init(ioDMM);
            }
        }
        /// <summary>
        /// Close Equipment IO
        /// </summary>
        public void CloseIO()
        {
            ioDMM.IO.Close();
        }
        /// <summary>
        /// Driver Revision control
        /// </summary>
        /// <returns>Driver's Version</returns>
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01a";        //  29/12/2011       KKL             VISA Driver for DMM E344xx (Base on minimum required command)

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        #region "Class Initialization"
        public cCommonFunction BasicCommand; // Basic Command for General Equipment (Must be Initialized)
        public cMeasurement Measurement;

        /// <summary>
        /// Initializing all Parameters
        /// </summary>
        /// <param name="IOInit"></param>
        public void Init(FormattedIO488 IOInit)
        {
            BasicCommand = new cCommonFunction(IOInit);
            Measurement = new cMeasurement(IOInit);
        }
        #endregion

        #region "Class Functional Codes"
        public class cMeasurement : cCommonFunction
        {
            public cMeasurement(FormattedIO488 parse) : base(parse) { }
            #region "DC Voltage"
            public double DC_Voltage()
            {
                return (Convert.ToDouble(ReadCommand("MEAS:VOLT:DC?")));
            }
            public double DC_Voltage(string Range, string Resolution)
            {
                return (Convert.ToDouble( ReadCommand("MEAS:VOLT:DC? " + Range + "," + Resolution)));
            }
            public double DC_Voltage(double Range, double Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:VOLT:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double DC_Voltage(e_Range Range, double Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:VOLT:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double DC_Voltage(double Range, e_Resolution Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:VOLT:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double DC_Voltage(e_Range Range, e_Resolution Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:VOLT:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            #endregion
            #region "DC Current"
            public double DC_Current()
            {
                return (Convert.ToDouble(ReadCommand("MEAS:CURR:DC?")));
            }
            public double DC_Current(string Range, string Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:CURR:DC? " + Range + "," + Resolution)));
            }
            public double DC_Current(double Range, double Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:CURR:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double DC_Current(e_Range Range, double Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:CURR:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double DC_Current(double Range, e_Resolution Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:CURR:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double DC_Current(e_Range Range, e_Resolution Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:CURR:DC? " + Range.ToString() + "," + Resolution.ToString())));
            }
            #endregion
            #region "Resistance"
            public double Resistance()
            {
                return (Convert.ToDouble(ReadCommand("MEAS:RES?")));
            }
            public double Resistance(string Range, String Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:RES? " + Range + "," + Resolution)));
            }
            public double Resistance(double Range, double Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:RES? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double Resistance(e_Range Range, double Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:RES? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double Resistance(double Range, e_Resolution Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:RES? " + Range.ToString() + "," + Resolution.ToString())));
            }
            public double Resistance(e_Range Range, e_Resolution Resolution)
            {
                return (Convert.ToDouble(ReadCommand("MEAS:RES? " + Range.ToString() + "," + Resolution.ToString())));
            }
            #endregion

        }
        #endregion

    }
}
