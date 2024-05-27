using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;

namespace LibFBAR
{
    #region "Enumeration Declaration"
    #endregion
    #region "Structure"
    #endregion
    public class cMXA_N9020A : iCommonFunction
    {

        public static string ClassName = "MXA N9020A Class";
        private string IOAddress;
        private FormattedIO488 ioMXA;
        private static LibFBAR.cGeneral common = new LibFBAR.cGeneral();

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
                return ioMXA;
            }
            set
            {
                ioMXA = parseIO;
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
                    ioMXA = new FormattedIO488();
                    ioMXA.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    common.DisplayError(ClassName, "Initialize IO Error", ex.Message);
                    ioMXA.IO = null;
                    return;
                }
                Init(ioMXA);
            }
        }
        /// <summary>
        /// Close Equipment IO
        /// </summary>
        public void CloseIO()
        {
            ioMXA.IO.Close();
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
            VersionStr = "0.01a";        //  13/02/2012       KKL             VISA Driver for MXA N9020A 

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }


        #region "Class Initialization"
        public cCommonFunction BasicCommand; // Basic Command for General Equipment (Must be Initialized)

        /// <summary>
        /// Initializing all Parameters
        /// </summary>
        /// <param name="IOInit"></param>
        public void Init(FormattedIO488 IOInit)
        {
            BasicCommand = new cCommonFunction(IOInit);

        }
        #endregion

        #region "Class Functional Codes"

        #endregion
    }
}
