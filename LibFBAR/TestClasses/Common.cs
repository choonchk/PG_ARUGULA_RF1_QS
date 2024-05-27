using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using Avago.ATF.StandardLibrary;

namespace LibFBAR
{
    #region "Enum"

    #endregion

    #region "Structure"
    public struct s_Result
    {
        public int TestNumber;
        public bool Enable;
        public bool b_MultiResult;
        public string Result_Header;
        public double Result_Data;
        public s_mRslt[] Multi_Results;
        public double Misc;
        //public string Result_Unit;  // if required
    }
    public struct s_mRslt
    {
        public bool Enable;
        public string Result_Header;
        public double Result_Data;
    }

    public struct s_TestSenario
    {
        public bool Multithread;
        public int Start_Items;
        public int Stop_Items;
        public int Items;
    }
    public struct s_SNPFile
    {
        public string FileOutput_Path;
        public int FileOutput_HeaderCount;
        public string FileOutput_FileName;
        public string FileOutput_Mode;
        public bool FileOutput_Enable;
        public int FileOuuput_Count;
        public int FileOutput_Iteration;
        public List<Tuple<string, string, string>> Impedance_Dictionary;
        public List<string> FileOutput_HeaderName;
    }
    #endregion
    public class cCommon
    {
        #region "Enumerations"

        #endregion
        #region "Structure"

        #endregion
        #region "Flags"
        static bool b_CheckFlag;     // To Check for Errors
        public static bool b_ErrorFlag;
        #endregion
        public static string ClassName = "Common Class";
        static LibFBAR.cFunction Math_Func = new cFunction();
        static LibFBAR.cGeneral General = new LibFBAR.cGeneral();

        public string Version()
        {
            string VersionStr;
            //VersionStr = "X.XX"       //Date (DD/MM/YYYY)  Person          Reason for Change?
            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            VersionStr = "0.01";        //  17/02/2012       KKL             New and example for Common class new development.

            //                          //-----------------  ----------- ----------------------------------------------------------------------------------
            return (ClassName + " Version = v" + VersionStr);
        }

        #region "Equipment Declarations"

        #endregion
        #region "Results Declarations"
        private static s_Result[] SaveResult;   //Temporary Array static results not accesible from external
        public s_Result[] Result_setting
        {
            get
            {
                return SaveResult;
            }
            set
            {
                SaveResult = value;
            }
        }     // Getting the internal data to public
        public void Init(int Tests)
        {
            SaveResult = new s_Result[Tests + 1];
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
            public cDelta Delta;
            public cSum Sum;
            public cRelativeGainDelta RelativeGain;
            public interface iTestFunction
            {
                void RunTest();
                void InitSettings();
                void MeasureResult();
            }
            public class cDelta : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Delta Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public string Previous_Info;
                public string Fix_Number;
                public bool b_Absolute;
                public bool b_FixNumber;

                // Internal Variables
                private int Previous_Test_1;
                private int Previous_Test_2;

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
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    string[] tmp_Info;
                    string tmp_info;
                    if (Fix_Number.ToUpper() != "")
                    {
                        b_FixNumber = true;
                    }
                    else
                    {
                        b_FixNumber = false;
                    }
                    if (!b_FixNumber)
                    {
                        tmp_Info = Previous_Info.Split(',');
                        if (tmp_Info.Length == 2)
                        {
                            Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) - 1;
                            Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) - 1;
                        }

                        else
                        {
                            General.DisplayError(ClassName + "_" + SubClassName, "Error converting Previous Information for Delta Class", "Previous Information : " + Previous_Info);
                        }
                    }
                    else
                    {
                        tmp_info = Previous_Info;

                        Previous_Test_1 = Convert.ToInt32(tmp_info) - 1;
 
                    }
 
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    double Previous_Data_1 = 0,
                           Previous_Data_2 = 0;

                    if (SaveResult[Previous_Test_1].b_MultiResult)
                    {
                        Previous_Data_1 = SaveResult[Previous_Test_1].Multi_Results[0].Result_Data; //Magnitude data for cFreq_At
                    }
                    else
                    {
                        Previous_Data_1 = SaveResult[Previous_Test_1].Result_Data;
                    }

                    if (SaveResult[Previous_Test_2].b_MultiResult)
                    {
                        Previous_Data_2 = SaveResult[Previous_Test_2].Multi_Results[0].Result_Data; //Magnitude data for cFreq_At
                    }
                    else
                    {
                        Previous_Data_2 = SaveResult[Previous_Test_2].Result_Data;
                    }

                    if (!b_FixNumber)
                    {
                        if (b_Absolute)
                        {
                            //rtnResult = Math.Abs(SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data);
                            rtnResult = Math.Abs(Previous_Data_1 - Previous_Data_2);
                        }
                        else
                        {
                            //rtnResult = SaveResult[Previous_Test_1].Result_Data - SaveResult[Previous_Test_2].Result_Data;
                            rtnResult = Previous_Data_1 - Previous_Data_2;
                        }
                    }
                    else
                    {
                        if (b_Absolute)
                        {
                            //rtnResult = Math.Abs(Convert.ToDouble(Fix_Number) - SaveResult[Previous_Test_1].Result_Data);
                            rtnResult = Math.Abs(Convert.ToDouble(Fix_Number) - Previous_Data_1);
                        }
                        else
                        {
                            //rtnResult = Convert.ToDouble(Fix_Number) - SaveResult[Previous_Test_1].Result_Data;
                            rtnResult = Convert.ToDouble(Fix_Number) - Previous_Data_1;
                        }
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].Result_Data = rtnResult;
                    //SaveResult[TestNo].Result_Header = "Delta_";
                    //SaveResult[TestNo].Result_Header += "_" + SaveResult[Previous_Test_1].Result_Header + "_" + SaveResult[Previous_Test_2].Result_Header;
                }
            }
            public class cSum : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Delta Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public string Previous_Info;
                public bool b_Absolute;


                // Internal Variables
                private int Previous_Test_1;
                private int Previous_Test_2;

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
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    string[] tmp_Info;
                    tmp_Info = Previous_Info.Split(',');

                    
                    if (tmp_Info.Length == 2)
                    {
                        Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) - 1;
                        Previous_Test_2 = Convert.ToInt32(tmp_Info[1]) - 1;
                    }

                    else
                    {
                        General.DisplayError(ClassName + "_" + SubClassName, "Error converting Previous Information for Delta Class", "Previous Information : " + Previous_Info);
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    if (b_Absolute)
                    {
                        rtnResult = Math.Abs(SaveResult[Previous_Test_1].Result_Data + SaveResult[Previous_Test_2].Result_Data);
                    }
                    else
                    {
                        rtnResult = SaveResult[Previous_Test_1].Result_Data + SaveResult[Previous_Test_2].Result_Data;
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].Result_Data = rtnResult;
                    //SaveResult[TestNo].Result_Header = "Delta_";
                    //SaveResult[TestNo].Result_Header += "_" + SaveResult[Previous_Test_1].Result_Header + "_" + SaveResult[Previous_Test_2].Result_Header;
                }
            }
            public class cRelativeGainDelta : iTestFunction
            {
                #region "Declarations"
                private string SubClassName = "Relative Gain Delta Class";    // Sub Class Naming

                // External Variables
                public ManualResetEvent _mre;
                public int TestNo;
                public string Previous_Info;
                public bool b_Absolute;

                // Internal Variables
                private int Previous_Test_1;
                private string Previous_Test_2;

                private double rtnResult;

                private string errInfo = null;
                private float value = 0f;

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
                        SaveResult[TestNo].Result_Header = value.Replace(" ", "_");
                    }
                }
                public void CallBack(object State)
                {
                    RunTest();
                    MeasureResult();
                    _mre.Set();
                }
                public void InitSettings()
                {
                    string[] tmp_Info;
                    tmp_Info = Previous_Info.Split(',');
                    if (tmp_Info.Length == 2)
                    {
                        Previous_Test_1 = Convert.ToInt32(tmp_Info[0]) - 1;
                        Previous_Test_2 = tmp_Info[1];
                    }

                    else
                    {
                        General.DisplayError(ClassName + "_" + SubClassName, "Error converting Previous Information for Relative_Gain Class", "Previous Information : " + Previous_Info);
                    }
                }
                public void RunTest()
                {
                    MeasureResult();
                    SetResult();
                }   //Function to call and run test. Not in used for FBAR
                public void MeasureResult()
                {
                    ATFResultBuilder.RecallResultByParameterName(Previous_Test_2, ref value, ref errInfo);

                    if (b_Absolute)
                    {
                        rtnResult = Math.Abs(Math.Abs(SaveResult[Previous_Test_1].Result_Data) - Math.Abs(value));
                    }
                    else
                    {
                        rtnResult = SaveResult[Previous_Test_1].Result_Data - value;
                    }
                }
                void SetResult()
                {
                    SaveResult[TestNo].Enable = true;
                    SaveResult[TestNo].b_MultiResult = false;
                    SaveResult[TestNo].Result_Data = rtnResult;
                    //SaveResult[TestNo].Result_Header = "Delta_";
                    //SaveResult[TestNo].Result_Header += "_" + SaveResult[Previous_Test_1].Result_Header + "_" + SaveResult[Previous_Test_2].Result_Header;
                }
            }
        }

    }
}
