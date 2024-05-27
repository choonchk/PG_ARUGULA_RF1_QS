using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Threading;

namespace LibFBAR
{
    public class cDMM
    {
        public static string ClassName = "DMM Class";
        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();
        
        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  29/12/2011       KKL             New and example for DMM class new development.

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        //static cExcel.cExcel_Lib Excel;
        //public cExcel.cExcel_Lib parseExcel
        //{
        //    set
        //    {
        //        Excel = value;
        //    }
        //}

        #region "Equipment Declarations"
        protected static FormattedIO488 ioDMM = new FormattedIO488();
        protected static LibFBAR.cDMM_344xx DMM = new LibFBAR.cDMM_344xx();
        public FormattedIO488 parseNA_IO
        {
            set
            {
                ioDMM = value;
            }
        }
        public void InitEquipment()
        {
            DMM.parseIO = ioDMM;
            DMM.Init(ioDMM);

        }  //Modify this for new equipment 
        public void InitEq(string address)
        {
            DMM.Address = address;
            DMM.OpenIO();
        }
        #endregion

        #region "Results Declarations"
        private static s_Result[] SaveResult;   //Temporary Array static results not accesible from external
        public s_Result[] Result_setting
        {
            get
            {
                return SaveResult;
            }
        }     // Getting the internal data to public
        public void Init(int Tests)
        {
            SaveResult = new s_Result[Tests];
        }
        public void Clear_Results()
        {
            for (int iClear = 0; iClear < SaveResult.Length; iClear++)
            {
                if (SaveResult[iClear].b_MultiResult)
                {
                    for (int iSubClear = 0; iSubClear < SaveResult[iClear].Multi_Results.Length; iSubClear++)
                    {
                        SaveResult[iClear].Multi_Results[iSubClear].Result_Data = 0;
                    }
                }
                SaveResult[iClear].Result_Data = 0;
            }
        }
        #endregion

        public cTestClasses[] TestClass;
        public class cTestClasses
        {
            public cDC_Voltage DC_Voltage;
            public cDC_Current DC_Current;

            public interface iTestFunction
            {
                void RunTest();
                void InitSettings();
                void MeasureResult();
            }
            public class cDC_Voltage : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "DC Voltage Measurement";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public string Range;
                public string Resolution;

                // Internal Variables
                private LibFBAR.cDMM_344xx.e_Range Range_En;
                private LibFBAR.cDMM_344xx.e_Resolution Resolution_En;
                private int TestOption;

                private double rtnResult;
                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value;
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }   // Multi Threading for Running Test
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings

                public void InitSettings()
                {
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;

                    if (Range == "")
                    {
                        TestOption = 0;
                    }
                    else
                    {
                        if (Enum.IsDefined(typeof(LibFBAR.cDMM_344xx.e_Range), Range))
                        {
                            Range_En = (LibFBAR.cDMM_344xx.e_Range)Enum.Parse(typeof(LibFBAR.cDMM_344xx.e_Range), Range);
                            if (Enum.IsDefined(typeof(LibFBAR.cDMM_344xx.e_Range), Range))
                            {
                                Range_En = (LibFBAR.cDMM_344xx.e_Range)Enum.Parse(typeof(LibFBAR.cDMM_344xx.e_Range), Range);
                                TestOption = 2;
                            }
                        }
                        else
                        {
                            TestOption = 1;
                        }
                    } 
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   
                public void MeasureResult()
                {
                    switch (TestOption)
                    {
                        case 0:
                            rtnResult = DMM.Measurement.DC_Voltage();
                            break;
                        case 1:
                            rtnResult = DMM.Measurement.DC_Voltage(Range, Resolution);
                            break;
                        case 2:
                            rtnResult = DMM.Measurement.DC_Voltage(Range_En, Resolution_En);
                            break;
                    }

                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }
            }
            public class cDC_Current : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "DC Current Measurement";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public string Range;
                public string Resolution;

                // Internal Variables
                private LibFBAR.cDMM_344xx.e_Range Range_En;
                private LibFBAR.cDMM_344xx.e_Resolution Resolution_En;
                private int TestOption;

                private double rtnResult;
                #endregion

                public double parseResult
                {
                    get
                    {
                        return (rtnResult);
                    }
                }
                public string setHeader
                {
                    set
                    {
                        SaveResult[TestNo].Result_Header = value;
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    _mre.Set();
                }   // Multi Threading for Running Test
                public void CallBack_Init(object State)
                {
                    InitSettings();
                    _mre.Set();
                }  //Multi Threading for Initializing Settings

                public void InitSettings()
                {
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;

                    if (Range == "")
                    {
                        TestOption = 0;
                    }
                    else
                    {
                        if (Enum.IsDefined(typeof(LibFBAR.cDMM_344xx.e_Range), Range))
                        {
                            Range_En = (LibFBAR.cDMM_344xx.e_Range)Enum.Parse(typeof(LibFBAR.cDMM_344xx.e_Range), Range);
                            if (Enum.IsDefined(typeof(LibFBAR.cDMM_344xx.e_Range), Range))
                            {
                                Range_En = (LibFBAR.cDMM_344xx.e_Range)Enum.Parse(typeof(LibFBAR.cDMM_344xx.e_Range), Range);
                                TestOption = 2;
                            }
                        }
                        else
                        {
                            TestOption = 1;
                        }
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }
                public void MeasureResult()
                {
                    switch (TestOption)
                    {
                        case 0:
                            rtnResult = DMM.Measurement.DC_Current();
                            break;
                        case 1:
                            rtnResult = DMM.Measurement.DC_Current(Range, Resolution);
                            break;
                        case 2:
                            rtnResult = DMM.Measurement.DC_Current(Range_En, Resolution_En);
                            break;
                    }

                }
                void SetResult()
                {
                    SaveResult[TestNo].Result_Data = rtnResult;
                }
            }
        }

    }
}
