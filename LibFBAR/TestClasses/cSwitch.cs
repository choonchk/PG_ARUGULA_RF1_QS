using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Threading;
using LibFBAR.Switch;

namespace LibFBAR
{
    public class cSwitch : cSwitch_common
    {
        public static string ClassName = "Switch Class";
        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();

        private static Dictionary<string, string> SwitchKey_Detail = new Dictionary<string, string>();
        private static List<string> SwitchKey = new List<string>();

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  05/01/2011       KKL             New and example for Switch Test class new development.

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
        private string Switch_EquipmentName;

        public void InitEquipment()
        {
            Set_Switch_Equipment(Switch_EquipmentName);
            Init();
        }
        
        #endregion

        //#region "Results Declarations"
        //private static s_Result[] SaveResult;   //Temporary Array static results not accesible from external
        //public s_Result[] Result_setting
        //{
        //    get
        //    {
        //        return SaveResult;
        //    }
        //}     // Getting the internal data to public
        //public void Init(int Tests)
        //{
        //    SaveResult = new s_Result[Tests];
        //}
        //public void Clear_Results()
        //{
        //    for (int iClear = 0; iClear < SaveResult.Length; iClear++)
        //    {
        //        if (SaveResult[iClear].b_MultiResult)
        //        {
        //            for (int iSubClear = 0; iSubClear < SaveResult[iClear].Multi_Results.Length; iSubClear++)
        //            {
        //                SaveResult[iClear].Multi_Results[iSubClear].Result_Data = 0;
        //            }
        //        }
        //        SaveResult[iClear].Result_Data = 0;
        //    }
        //}
        //#endregion

        public cTestClasses[] TestClass;
        public class cTestClasses
        {
            public cOpenClose OpenClose;
            public cOpen Open;
            public cClose Close;

            public interface iTestFunction
            {
                void RunTest();
                void InitSettings();
                void MeasureResult();
            }
            public class cOpenClose : cSwitch_common, iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Switch Control Open Close Settings";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public string inputStr;

                // Internal Variables
                private string Open_Str;
                private string Close_Str;
                #endregion

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
                    if (SwitchKey.Contains(inputStr))
                    {
                        SwitchKey_Detail.TryGetValue(inputStr + "_Open", out Open_Str);
                        SwitchKey_Detail.TryGetValue(inputStr + "_Close", out Close_Str);
                    }
                }
                public void RunTest()
                {
                    SwitchCmd.Open(Open_Str);
                    SwitchCmd.Close(Close_Str);
                    //MeasureResult();
                }
                public void MeasureResult()
                {
                    
                }
            }
            public class cOpen : cSwitch_common, iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Switch Control Open Settings";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public string inputStr;

                // Internal Variables
                private string Open_Str;
                #endregion

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
                    if (SwitchKey.Contains(inputStr))
                    {
                        SwitchKey_Detail.TryGetValue(inputStr + "_Open", out Open_Str);
                    }
                }
                public void RunTest()
                {
                    SwitchCmd.Open(Open_Str);
                    //MeasureResult();
                }
                public void MeasureResult()
                {

                }
            }
            public class cClose : cSwitch_common, iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Switch Control Close Settings";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;

                public string inputStr;

                // Internal Variables
                private string Close_Str;
                #endregion

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
                    if (SwitchKey.Contains(inputStr))
                    {
                        SwitchKey_Detail.TryGetValue(inputStr + "_Close", out Close_Str);
                    }
                }
                public void RunTest()
                {
                    SwitchCmd.Close(Close_Str);
                    //MeasureResult();
                }
                public void MeasureResult()
                {

                }
            }
        }
    }
}
